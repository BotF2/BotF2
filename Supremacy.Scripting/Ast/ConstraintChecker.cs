using System;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public abstract class ConstraintChecker
    {
        protected readonly Type[] GenericParameters;
        protected readonly Type[] ArgumentTypes;
        protected readonly SourceSpan Span;
        protected readonly ParseContext ParseContext;
        protected readonly bool Silent;

        protected ConstraintChecker(
            Type[] genericParameters, Type[] argumentTypes, SourceSpan span, ParseContext parseContext, bool silent)
        {
            GenericParameters = genericParameters;
            ArgumentTypes = argumentTypes;
            Span = span;
            ParseContext = parseContext;
            Silent = silent;
        }

        /// <summary>
        ///   Check the constraints; we're called from ResolveAsTypeTerminal()
        ///   after fully resolving the constructed type.
        /// </summary>
        public bool CheckConstraints(ParseContext ec)
        {
            return !GenericParameters.Where((t, i) => !CheckConstraints(ec, i)).Any();
        }

        protected bool CheckConstraints(ParseContext ec, int index)
        {
            Type atype = ArgumentTypes[index];
            Type ptype = GenericParameters[index];

            if (atype == ptype)
            {
                return true;
            }

            Expression aexpr = new EmptyExpression(atype);

            GenericConstraints gc = TypeManager.GetTypeParameterConstraints(ptype);
            if (gc == null)
            {
                return true;
            }

            bool isClass, isStruct;
            if (atype.IsGenericParameter)
            {
                GenericConstraints agc = TypeManager.GetTypeParameterConstraints(atype);
                if (agc != null)
                {
/*
                    if (agc is Constraints)
                    {
                        // FIXME: No constraints can be resolved here, we are in
                        // completely wrong/different context. This path is hit
                        // when resolving base type of unresolved generic type
                        // with constraints. We are waiting with CheckConsttraints
                        // after type-definition but not in this case
                        if (!((Constraints)agc).Resolve(null, null, Report))
                            return true;
                    }
*/
                    isClass = agc.IsReferenceType;
                    isStruct = agc.IsValueType;
                }
                else
                {
                    isClass = isStruct = false;
                }
            }
            else
            {
                isClass = TypeManager.IsReferenceType(atype);
                isStruct = TypeManager.IsValueType(atype) && !TypeUtils.IsNullableType(atype);
            }

            //
            // First, check the `class' and `struct' constraints.
            //
            if (gc.HasReferenceTypeConstraint && !isClass)
            {
                if (!Silent)
                {
                    ec.ReportError(
                        452,
                        string.Format(
                            "The type '{0}' must be " +
                            "a reference type in order to use it " +
                            "as type parameter '{1}' in the " +
                            "generic type or method '{2}'.",
                            TypeManager.GetCSharpName(atype),
                            TypeManager.GetCSharpName(ptype),
                            GetSignatureForError()),
                        Severity.Error,
                        Span);
                }

                return false;
            }

            if (!Silent && gc.HasValueTypeConstraint && !isStruct)
            {
                if (!Silent)
                {
                    ec.ReportError(
                        453,
                        string.Format(
                            "The type '{0}' must be a " +
                            "non-nullable value type in order to use it " +
                            "as type parameter '{1}' in the " +
                            "generic type or method '{2}'.",
                            TypeManager.GetCSharpName(atype),
                            TypeManager.GetCSharpName(ptype),
                            GetSignatureForError()),
                        Severity.Error,
                        Span);
                }

                return false;
            }

            //
            // The class constraint comes next.
            //
            if (gc.HasClassConstraint)
            {
                if (!CheckConstraint(ec, ptype, aexpr, gc.ClassConstraint))
                {
                    return false;
                }
            }

            //
            // Now, check the interface constraints.
            //
            if (gc.InterfaceConstraints != null)
            {
                if (gc.InterfaceConstraints.Any(it => !CheckConstraint(ec, ptype, aexpr, it)))
                {
                    return false;
                }
            }

            //
            // Finally, check the constructor constraint.
            //

            if (!gc.HasConstructorConstraint)
            {
                return true;
            }

            if (TypeManager.IsValueType(atype))
            {
                return true;
            }

            if (HasDefaultConstructor(atype))
            {
                return true;
            }

            if (!Silent)
            {
                ec.ReportError(
                    310,
                    string.Format(
                        "The type '{0}' must have a public " +
                        "parameterless constructor in order to use it " +
                        "as parameter '{1}' in the generic type or " +
                        "method '{2}'",
                        TypeManager.GetCSharpName(atype),
                        TypeManager.GetCSharpName(ptype),
                        GetSignatureForError()),
                    Severity.Error,
                    Span);
            }

            return false;
        }

        protected bool CheckConstraint(
            ParseContext ec,
            Type ptype,
            Expression expr,
            Type ctype)
        {
            //
            // All this is needed because we don't have
            // real inflated type hierarchy
            //
            if (TypeManager.HasGenericArguments(ctype))
            {
                Type[] types = ctype.GetGenericArguments();
                TypeArguments newArgs = new TypeArguments();

                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t.IsGenericParameter)
                    {
                        int pos = t.GenericParameterPosition;
                        if (t.DeclaringMethod == null && this is MethodConstraintChecker)
                        {
                            Type parent = ((MethodConstraintChecker)this).DeclaringType;
                            t = parent.GetGenericArguments()[pos];
                        }
                        else
                        {
                            t = ArgumentTypes[pos];
                        }
                    }
                    newArgs.Add(new TypeExpression(t) { Span = Span });
                }

                GenericTypeExpression ct = new GenericTypeExpression(ctype, newArgs, Span);
                if (ct.ResolveAsTypeStep(ec, false) == null)
                {
                    return false;
                }

                ctype = ct.Type;
            }
            else if (ctype.IsGenericParameter)
            {
                int pos = ctype.GenericParameterPosition;
                if (ctype.DeclaringMethod == null)
                {
                    // FIXME: Implement
                    return true;
                }
                ctype = ArgumentTypes[pos];
            }

            Type expressionType = expr.Type;

            if (TypeUtils.IsImplicitlyConvertible(expressionType, ctype))
            {
                return true;
            }

            if (Silent)
            {
                return false;
            }

            if (TypeUtils.IsNullableType(expressionType) && ctype.IsInterface)
            {
                ec.ReportError(
                    313,
                    string.Format(
                        "The type '{0}' cannot be used as type parameter '{1}' in the generic type or method '{2}'.  " +
                        "The nullable type '{0}' never satisfies interface constraint of type '{3}'.",
                        TypeManager.GetCSharpName(expressionType),
                        TypeManager.GetCSharpName(ptype),
                        GetSignatureForError(),
                        TypeManager.GetCSharpName(ctype)),
                    Severity.Error,
                    Span);
            }
            else
            {
                ec.ReportError(
                    309,
                    string.Format(
                        "The type '{0}' must be convertible to '{1}' in order to " +
                        "use it as parameter '{2}' in the generic type or method '{3}'.",
                        TypeManager.GetInterfaces(expressionType),
                        TypeManager.GetInterfaces(ctype),
                        TypeManager.GetInterfaces(ptype),
                        GetSignatureForError()),
                    Severity.Error,
                    Span);
            }

            return false;
        }

        private static bool HasDefaultConstructor(Type atype)
        {
            if (atype.IsAbstract)
            {
                return false;
            }

            atype = TypeManager.DropGenericTypeArguments(atype);

            MemberInfo[] list = TypeManager.MemberLookup(
                null,
                null,
                atype,
                MemberTypes.Constructor,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                ConstructorInfo.ConstructorName,
                null);

            return list == null
                ? false
                : list
                .OfType<MethodBase>()
                .Select(TypeManager.GetParameterData)
                .Any(o => o.Count == 0);
        }

        protected abstract string GetSignatureForError();

        public static bool CheckConstraints(
            ParseContext ec,
            MethodBase definition,
            MethodBase instantiated,
            SourceSpan loc,
            bool silent = false)
        {
            MethodConstraintChecker checker = new MethodConstraintChecker(
                definition,
                instantiated.DeclaringType,
                definition.GetGenericArguments(),
                instantiated.GetGenericArguments(),
                loc,
                ec,
                silent);

            return checker.CheckConstraints(ec);
        }

        public static bool CheckConstraints(
            ParseContext ec,
            Type gt,
            Type[] genParams,
            Type[] typeArguments,
            SourceSpan loc,
            bool silent = false)
        {
            TypeConstraintChecker checker = new TypeConstraintChecker(
                gt,
                genParams,
                typeArguments,
                loc,
                ec,
                silent);

            return checker.CheckConstraints(ec);
        }

        protected class MethodConstraintChecker : ConstraintChecker
        {
            private readonly MethodBase _definition;
            public Type DeclaringType;

            public MethodConstraintChecker(
                MethodBase definition,
                Type declaringType,
                Type[] genericParameters,
                Type[] typeArguments,
                SourceSpan loc,
                ParseContext parseContext,
                bool silent)
                : base(genericParameters, typeArguments, loc, parseContext, silent)
            {
                _definition = definition;
                DeclaringType = declaringType;
            }

            protected override string GetSignatureForError()
            {
                return TypeManager.GetCSharpSignature(_definition);
            }
        }

        protected class TypeConstraintChecker : ConstraintChecker
        {
            private readonly Type _gt;

            public TypeConstraintChecker(
                Type gt,
                Type[] genericParameters,
                Type[] typeArguments,
                SourceSpan loc,
                ParseContext parseContext,
                bool silent)
                : base(genericParameters, typeArguments, loc, parseContext, silent)
            {
                _gt = gt;
            }

            protected override string GetSignatureForError()
            {
                return TypeManager.GetCSharpName(_gt);
            }
        }
    }
}