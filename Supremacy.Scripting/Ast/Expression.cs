using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class Expression : Ast
    {
        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is Expression clone))
            {
                return;
            }

            clone.ExpressionClass = ExpressionClass;
            clone.Type = Type;
        }

        [DebuggerStepThrough]
        public virtual Expression DoResolve(ParseContext parseContext)
        {
            return this;
        }

        [DebuggerStepThrough]
        public virtual Expression DoResolveLValue(ParseContext parseContext, Expression rightSide)
        {
            return null;
        }

        public MSAst TransformedExpression { get; protected set; }

        public MSAst Transform(ScriptGenerator generator)
        {
            return TransformCore(generator);
        }

        public virtual bool ShouldParenthesize => false;

        public virtual bool IsPrimaryExpression => false;

        public virtual MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Default(Type);
        }

        public Type Type { get; set; }

        /// <summary>
        ///   Resolves an expression and performs semantic analysis on it.
        /// </summary>
        [DebuggerStepThrough]
        public Expression Resolve(ParseContext ec)
        {
            return Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
        }

        [DebuggerStepThrough]
        public Expression ResolveLValue(ParseContext ec, Expression rightSide)
        {
            int errors = ec.CompilerErrorCount;
            _ = rightSide == EmptyExpression.OutAccess;

            Expression e = DoResolveLValue(ec, rightSide);

            if (e == null)
            {
                if (errors == ec.CompilerErrorCount)
                {

                }
                return null;
            }

            if (e.ExpressionClass == ExpressionClass.Invalid)
            {
                throw new Exception("Expression " + e + " ExprClass is Invalid after resolve");
            }

            if ((e.Type == null) && !(e is GenericTypeExpression))
            {
                throw new Exception("Expression " + e + " did not set its type after Resolve");
            }

            return e;
        }

        [DebuggerStepThrough]
        public Expression Resolve(ParseContext ec, ResolveFlags flags)
        {
            //if ((flags & ResolveFlags.MaskExprClass) == ResolveFlags.Type)
            //    return ResolveAsTypeStep(ec, false);

            Expression e;

            try
            {

                if (this is NameExpression)
                {
                    bool intermediate = (flags & ResolveFlags.Intermediate) == ResolveFlags.Intermediate;
                    e = ((NameExpression)this).DoResolve(ec, intermediate);
                }
                else
                {
                    e = DoResolve(ec);
                }
            }
            catch (SyntaxErrorException ex)
            {
                ec.Compiler.Errors.Add(
                    ec.Compiler.SourceUnit,
                    ex.Message,
                    Span,
                    ex.ErrorCode,
                    ex.Severity);

                return this;
            }

            return e;
        }

        //
        // This is used if the expression should be resolved as a type or namespace name.
        // the default implementation fails.   
        //
        public virtual FullNamedExpression ResolveAsTypeStep(ParseContext parseContext, bool silent)
        {
            return null;
        }

        public virtual TypeExpression ResolveAsTypeTerminal(ParseContext ec, bool silent)
        {
            TypeExpression te = ResolveAsBaseTerminal(ec, silent);
            if (te == null)
            {
                return null;
            }

            if (te is GenericTypeExpression ct)
            {
                // TODO: silent flag is ignored
                _ = ct.CheckConstraints(ec);
            }

            return te;
        }

        public virtual string GetSignatureForError()
        {
            return GetType().Name;
        }

        protected ResolveFlags ExpressionClassToResolveFlags
        {
            get
            {
                switch (ExpressionClass)
                {
                    case ExpressionClass.Type:
                    case ExpressionClass.Namespace:
                        return ResolveFlags.Type;

                    case ExpressionClass.MethodGroup:
                        return ResolveFlags.MethodGroup;

                    case ExpressionClass.TypeParameter:
                        return ResolveFlags.TypeParameter;

                    case ExpressionClass.Value:
                    case ExpressionClass.Variable:
                    case ExpressionClass.PropertyAccess:
                    case ExpressionClass.EventAccess:
                    case ExpressionClass.IndexerAccess:
                        return ResolveFlags.VariableOrValue;

                    default:
                        throw new InternalErrorException(
                            Span +
                            " " +
                            GetType().Name +
                            " expression class is Invalid after resolve.");
                }
            }
        }

        public virtual ExpressionClass ExpressionClass
        {
            get; protected set;
        }

        public virtual string ExpressionClassName
        {
            get
            {
                switch (ExpressionClass)
                {
                    case ExpressionClass.Value:
                        return "value";
                    case ExpressionClass.Variable:
                        return "variable";
                    case ExpressionClass.Namespace:
                        return "namespace";
                    case ExpressionClass.Type:
                        return "type";
                    case ExpressionClass.MethodGroup:
                        return "method group";
                    case ExpressionClass.PropertyAccess:
                        return "property access";
                    case ExpressionClass.EventAccess:
                        return "event access";
                    case ExpressionClass.IndexerAccess:
                        return "indexer access";
                    case ExpressionClass.Nothing:
                        return "null";
                    case ExpressionClass.TypeParameter:
                        return "type parameter";
                    default:
                    case ExpressionClass.Invalid:
                        return "Invalid";
                }
            }
        }

        public virtual bool IsNull => false;

        public TypeExpression ResolveAsBaseTerminal(ParseContext ec, bool silent)
        {
            int errorCount = ec.CompilerErrorCount;
            FullNamedExpression fne = ResolveAsTypeStep(ec, silent);

            if (fne == null)
            {
                return null;
            }

            if (!(fne is TypeExpression typeExpression))
            {
                if (!silent && (errorCount == ec.CompilerErrorCount))
                {
                    ec.ReportError(
                        CompilerErrors.UnexpectedExpressionType,
                        Span,
                        GetSignatureForError(),
                        "type");
                }

                return null;
            }

            if (!TypeManager.CheckAccessLevel(ec, typeExpression.Type))
            {
                ec.ReportError(
                    CompilerErrors.MemberIsInaccessible,
                    Span,
                    TypeManager.GetCSharpName(typeExpression.Type));

                return null;
            }

            typeExpression.Span = Span;

            return typeExpression;
        }

        //
        // FIXME: Probably implement a cache for (t,name,current_access_set)?
        //
        // This code could use some optimizations, but we need to do some
        // measurements.  For example, we could use a delegate to `flag' when
        // something can not any longer be a method-group (because it is something
        // else).
        //
        // Return values:
        //     If the return value is an Array, then it is an array of
        //     MethodBases
        //   
        //     If the return value is an MemberInfo, it is anything, but a Method
        //
        //     null on error.
        //
        // FIXME: When calling MemberLookup inside an `Invocation', we should pass
        // the arguments here and have MemberLookup return only the methods that
        // match the argument count/type, unlike we are doing now (we delay this
        // decision).
        //
        // This is so we can catch correctly attempts to invoke instance methods
        // from a static body (scan for error 120 in ResolveSimpleName).
        //
        //
        // FIXME: Potential optimization, have a static ArrayList
        //

        public static Expression MemberLookup(ParseContext ctx, Type containerType, Type queriedType, string name,
                               MemberTypes mt, BindingFlags bf, SourceSpan loc)
        {
            return MemberLookup(ctx, containerType, null, queriedType, name, mt, bf, loc);
        }

        //
        // Lookup type `queried_type' for code in class `container_type' with a qualifier of
        // `qualifier_type' or null to lookup members in the current class.
        //

        public static Expression MemberLookup(ParseContext ctx, Type containerType,
                               Type qualifierType, Type queriedType,
                               string name, MemberTypes mt,
                               BindingFlags bf, SourceSpan span)
        {
            ArrayList almostMatchedMembers = new ArrayList();

            MemberInfo[] mi = TypeManager.MemberLookup(
                containerType,
                qualifierType,
                queriedType,
                mt,
                bf,
                name,
                almostMatchedMembers);

            if (mi == null)
            {
                return null;
            }

            if (mi.Length > 1)
            {
                bool isInterface = qualifierType != null && qualifierType.IsInterface;
                ArrayList methods = new ArrayList(2);
                List<MemberInfo> nonMethods = null;

                foreach (MemberInfo m in mi)
                {
                    if (m is MethodBase)
                    {
                        _ = methods.Add(m);
                        continue;
                    }

                    if (nonMethods == null)
                    {
                        nonMethods = new List<MemberInfo>(2);
                    }

                    bool isCandidate = true;
                    for (int i = 0; i < nonMethods.Count; ++i)
                    {
                        MemberInfo nonMethod = nonMethods[i];
                        if (nonMethod.DeclaringType.IsInterface && TypeManager.ImplementsInterface(m.DeclaringType, nonMethod.DeclaringType))
                        {
                            _ = nonMethods.Remove(nonMethod);
                            --i;
                        }
                        else if (m.DeclaringType.IsInterface && TypeManager.ImplementsInterface(nonMethod.DeclaringType, m.DeclaringType))
                        {
                            isCandidate = false;
                            break;
                        }
                    }

                    if (isCandidate)
                    {
                        nonMethods.Add(m);
                    }
                }

                if (methods.Count == 0 && nonMethods != null && nonMethods.Count > 1)
                {
                    ctx.ReportError(
                        229,
                        string.Format(
                            "Ambiguity found between '{0}' and '{1}'",
                            TypeManager.GetFullNameSignature(nonMethods[1]),
                            TypeManager.GetFullNameSignature(nonMethods[0])),
                        Severity.Error,
                        span);
                    
                    return null;
                }

                if ((methods.Count == 0) && (nonMethods != null))
                {
                    return ExprClassFromMemberInfo(containerType, nonMethods[0], span);
                }

                if (nonMethods != null && nonMethods.Count > 0)
                {
                    MethodBase method = (MethodBase)methods[0];
                    MemberInfo nonMethod = nonMethods[0];
                    if (method.DeclaringType == nonMethod.DeclaringType)
                    {
                        // Cannot happen with C# code, but is valid in IL
                        ctx.ReportError(
                            229,
                            string.Format(
                                "Ambiguity between '{0}' and '{1}'.",
                                TypeManager.GetFullNameSignature(nonMethod),
                                TypeManager.GetCSharpSignature(method)),
                            Severity.Error,
                            span);
                        return null;
                    }

                    if (isInterface)
                    {
                        ctx.ReportError(
                            467,
                            string.Format(
                                "Ambiguity between method '{0}' and non-method '{1}'.  Using method '{0}'.",
                                TypeManager.GetCSharpSignature(method),
                                TypeManager.GetFullNameSignature(nonMethod)),
                            Severity.Warning,
                            span);
                    }
                }

                return new MethodGroupExpression(
                    methods.Cast<MemberInfo>().ToArray(),
                    queriedType,
                    span);
            }

            return mi[0] is MethodBase ? new MethodGroupExpression(mi, queriedType, span) : ExprClassFromMemberInfo(containerType, mi[0], span);
        }

#pragma warning disable 168
        public static Expression ExprClassFromMemberInfo(Type containerType, MemberInfo mi, SourceSpan span)
        {
            EventInfo eventInfo = mi as EventInfo;
            if (eventInfo != null)
            {
                throw new NotSupportedException("Event access is not supported.");
            }

            FieldInfo fieldInfo = mi as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.IsLiteral ||
                    (fieldInfo.IsInitOnly && (fieldInfo.FieldType == TypeManager.CoreTypes.Decimal))
                    ? new ConstantMemberExpression(fieldInfo, span)
                    : (Expression)new FieldExpression(containerType, fieldInfo, span);
            }

            PropertyInfo propertyInfo = mi as PropertyInfo;
            if (propertyInfo != null)
            {
                return new PropertyExpression(containerType, (PropertyInfo)mi, span);
            }

            Type typeInfo = mi as Type;
            return typeInfo != null ? new TypeExpression(typeInfo) { Span = span } : null;
        }
#pragma warning restore 168

        public const MemberTypes AllMemberTypes =
            MemberTypes.Constructor |
            MemberTypes.Event |
            MemberTypes.Field |
            MemberTypes.Method |
            MemberTypes.NestedType |
            MemberTypes.Property;

        public const BindingFlags AllBindingFlags =
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.Instance;

        public static Expression MemberLookup(ParseContext ctx, Type containerType, Type queriedType,
                               string name, SourceSpan loc)
        {
            return MemberLookup(ctx, containerType, null, queriedType, name,
                         AllMemberTypes, AllBindingFlags, loc);
        }

        public static Expression MemberLookup(ParseContext ctx, Type containerType, Type qualifierType,
                               Type queriedType, string name, SourceSpan loc)
        {
            return MemberLookup(ctx, containerType, qualifierType, queriedType,
                         name, AllMemberTypes, AllBindingFlags, loc);
        }

        public static MethodGroupExpression MethodLookup(ParseContext ctx, Type containerType, Type queriedType,
                               string name, SourceSpan loc)
        {
            return (MethodGroupExpression)MemberLookup(ctx, containerType, null, queriedType, name,
                         MemberTypes.Method, AllBindingFlags, loc);
        }

        /// <summary>
        ///   This is a wrapper for MemberLookup that is not used to "probe", but
        ///   to find a final definition.  If the final definition is not found, we
        ///   look for private members and display a useful debugging message if we
        ///   find it.
        /// </summary>
        protected Expression MemberLookupFinal(ParseContext ec, Type qualifierType,
                                Type queriedType, string name,
                                MemberTypes mt, BindingFlags bf,
                                SourceSpan loc)
        {
            int errors = ec.CompilerErrorCount;
            Expression e = MemberLookup(ec, null, qualifierType, queriedType, name, mt, bf, loc);

            if (e != null || errors != ec.CompilerErrorCount)
            {
                return e;
            }

            // No errors were reported by MemberLookup, but there was an error.
            return OnErrorMemberLookupFailed(
                ec,
                null,
                qualifierType,
                queriedType,
                name,
                null,
                mt,
                bf);
        }

        protected virtual Expression OnErrorMemberLookupFailed(ParseContext ec, Type containerType, Type qualifierType,
                               Type queriedType, string name, string className,
                               MemberTypes mt, BindingFlags bf)
        {
            MemberInfo[] lookup = null;
            if (queriedType == null)
            {
                className = "global::";
            }
            else
            {
                lookup = TypeManager.MemberLookup(queriedType, null, queriedType,
                    mt, (bf & ~BindingFlags.Public) | BindingFlags.NonPublic,
                    name, null);

                if (lookup != null)
                {
                    Expression e = OnErrorMemberLookupFailed(ec, queriedType, lookup);

                    //
                    // FIXME: This is still very wrong, it should be done inside
                    // OverloadResolve to do correct arguments matching.
                    // Requires MemberLookup accessiblity check removal
                    //
                    if (e == null || (mt & (MemberTypes.Method | MemberTypes.Constructor)) == 0)
                    {
                        MemberInfo mi = lookup[0];
                        if (qualifierType != null && containerType != null && qualifierType != containerType &&
                            TypeManager.IsNestedFamilyAccessible(containerType, mi.DeclaringType))
                        {
                            // Although a derived class can access protected members of
                            // its base class it cannot do so through an instance of the
                            // base class (CS1540).  If the qualifier_type is a base of the
                            // ec.CurrentType and the lookup succeeds with the latter one,
                            // then we are in this situation.
                            ec.ReportError(
                                1540,
                                string.Format(
                                    "Cannot access protected member '{0}' via a qualifier of type '{1}'.  " +
                                    "The qualifier must be of type '{2}' or derived from it.",
                                    TypeManager.GetFullNameSignature(mi),
                                    TypeManager.GetCSharpName(qualifierType),
                                    TypeManager.GetCSharpName(containerType)),
                                Severity.Error,
                                Span);
                        }
                        else
                        {
                            ec.ReportError(
                                CompilerErrors.MemberIsInaccessible,
                                Span,
                                TypeManager.GetFullNameSignature(mi));
                        }
                    }

                    return e;
                }

                lookup = TypeManager.MemberLookup(
                    queriedType,
                    null,
                    queriedType,
                    AllMemberTypes,
                    AllBindingFlags | BindingFlags.NonPublic,
                    name,
                    null);
            }

            if (lookup == null)
            {
                if (className != null)
                {
                    ec.ReportError(
                        103,
                        string.Format(
                            "The name '{0}' does not exist in the current context.",
                            name),
                        Severity.Error,
                        Span);
                }
                else
                {
                    ec.ReportError(
                        117,
                        string.Format(
                            "'{0}' does not contain a definition for '{1}'.",
                            TypeManager.GetCSharpName(queriedType),
                            name),
                        Severity.Error,
                        Span);
                }

                return null;
            }

            if (TypeManager.MemberLookup(
                    queriedType,
                    null,
                    queriedType,
                    AllMemberTypes,
                    AllBindingFlags |
                    BindingFlags.NonPublic,
                    name,
                    null) == null)
            {
                if ((lookup.Length == 1) && (lookup[0] is Type))
                {
                    Type t = (Type)lookup[0];

                    ec.ReportError(
                        305,
                        string.Format(
                            "Using the generic type '{0}' " +
                            "requires {1} type arguments.",
                            TypeManager.GetCSharpName(t),
                            TypeManager.GetNumberOfTypeArguments(t)),
                        Severity.Error,
                        Span);

                    return null;
                }
            }

            return OnErrorMemberLookupFailed(ec, queriedType, lookup);
        }

        protected virtual Expression OnErrorMemberLookupFailed(ParseContext ec, Type type, MemberInfo[] members)
        {
            if (members.Any(t => !(t is MethodBase)))
            {
                return null;
            }

            // By default propagate the closest candidates upwards
            return new MethodGroupExpression(members, type, Span, true);
        }

        public static bool IsAccessorAccessible(Type invocationType, MethodInfo mi, out bool mustDoCs1540Check)
        {
            MethodAttributes ma = mi.Attributes & MethodAttributes.MemberAccessMask;

            mustDoCs1540Check = false; // by default we do not check for this

            if (ma == MethodAttributes.Public)
            {
                return true;
            }

            //
            // If only accessible to the current class or children
            //
            if (ma == MethodAttributes.Private)
            {
                return TypeManager.IsPrivateAccessible(invocationType, mi.DeclaringType) ||
                    TypeManager.IsNestedChildOf(invocationType, mi.DeclaringType);
            }

            if (TypeManager.IsThisOrFriendAssembly(invocationType.Assembly, mi.DeclaringType.Assembly))
            {
                if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamORAssem)
                {
                    return true;
                }
            }
            else
            {
                if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamANDAssem)
                {
                    return false;
                }
            }

            // Family and FamANDAssem require that we derive.
            // FamORAssem requires that we derive if in different assemblies.
            if (!TypeManager.IsNestedFamilyAccessible(invocationType, mi.DeclaringType))
            {
                return false;
            }

            if (!TypeManager.IsNestedChildOf(invocationType, mi.DeclaringType))
            {
                mustDoCs1540Check = true;
            }

            return true;
        }

        public void OnErrorTypeArgumentsCannotBeUsed(ParseContext parseContext, SourceSpan location)
        {
            // Better message for possible generic expressions
            if (ExpressionClass == ExpressionClass.MethodGroup || ExpressionClass == ExpressionClass.Type)
            {
                string name = ExpressionClass == ExpressionClass.Type ? ExpressionClassName : "method";
                parseContext.ReportError(
                    308,
                    string.Format(
                        "The non-generic {0} '{1}' cannot be used with the type arguments.",
                        name,
                        GetSignatureForError()),
                    Severity.Error,
                    location);
            }
            else
            {
                parseContext.ReportError(
                    307,
                    string.Format(
                        "The {0} '{1}' cannot be used with type arguments.",
                        ExpressionClassName,
                        GetSignatureForError()),
                    Severity.Error,
                    location);
            }
        }

        public virtual void OnErrorValueCannotBeConverted(ParseContext ec, SourceSpan location, Type target, bool expl)
        {
            OnErrorValueCannotBeConvertedCore(ec, location, target, expl);
        }

        protected void OnErrorValueCannotBeConvertedCore(ParseContext ec, SourceSpan loc, Type target, bool explicitConversion)
        {
            Type sourceType = Type;

            if (TypeManager.IsGenericParameter(sourceType) &&
                TypeManager.IsGenericParameter(target) &&
                sourceType.Name == target.Name)
            {
                string sig1 = sourceType.DeclaringMethod == null ?
                    TypeManager.GetCSharpName(sourceType.DeclaringType) :
                    TypeManager.GetCSharpSignature(sourceType.DeclaringMethod);
                string sig2 = target.DeclaringMethod == null ?
                    TypeManager.GetCSharpName(target.DeclaringType) :
                    TypeManager.GetCSharpSignature(target.DeclaringMethod);
                ec.ReportError(
                    -1,
                    String.Format(
                        "The generic parameter '{0}' of '{1}' cannot be converted to the generic parameter '{0}' of '{2}'.",
                        sourceType.Name,
                        sig1,
                        sig2),
                    loc);
            }
            else if (sourceType.FullName == target.FullName)
            {
                ec.ReportError(
                    -1,
                    String.Format(
                        "The type '{0}' has two conflicting definitions, one comes from '{1}' and the other from '{2}'.",
                        sourceType.FullName,
                        sourceType.FullName,
                        target.Assembly.FullName),
                    loc);
            }

            if (explicitConversion)
            {
                ec.ReportError(
                    30,
                    string.Format(
                        "Cannot convert type '{0}' to '{1}'.",
                        TypeManager.GetCSharpName(sourceType),
                        TypeManager.GetCSharpName(target)),
                    loc);

                return;
            }

            var explicitConversionExists = (CompilerHelpers.GetExplicitConverter(sourceType, target) != null);
            if (explicitConversionExists)
            {
                ec.ReportError(
                    266,
                    string.Format(
                        "Cannot implicitly convert type '{0}' to '{1}'.  " +
                        "An explicit conversion exists (are you missing a cast?).",
                        TypeManager.GetCSharpName(sourceType),
                        TypeManager.GetCSharpName(target)),
                    loc);
                return;
            }

            ec.ReportError(
                29,
                string.Format(
                    "Cannot implicitly convert type '{0}' to '{1}'",
                    TypeManager.GetCSharpName(sourceType),
                    TypeManager.GetCSharpName(target)),
                loc);
        }

        public void OnErrorUnexpectedKind(ParseContext ec, ResolveFlags flags, SourceSpan span)
        {
            string[] valid = new string[4];
            int count = 0;

            if ((flags & ResolveFlags.VariableOrValue) != 0)
            {
                valid[count++] = "variable";
                valid[count++] = "value";
            }

            if ((flags & ResolveFlags.Type) != 0)
            {
                valid[count++] = "type";
            }

            if ((flags & ResolveFlags.MethodGroup) != 0)
            {
                valid[count++] = "method group";
            }

            if (count == 0)
            {
                valid[count++] = "unknown";
            }

            StringBuilder sb = new StringBuilder(valid[0]);
            for (int i = 1; i < count - 1; i++)
            {
                _ = sb.Append("', '");
                _ = sb.Append(valid[i]);
            }
            if (count > 1)
            {
                _ = sb.Append("' or '");
                _ = sb.Append(valid[count - 1]);
            }

            ec.ReportError(
                CompilerErrors.UnexpectedExpressionKind,
                span,
                ExpressionClassName,
                sb.ToString());
        }

        /// <summary>
        ///   Returns an expression that can be used to invoke operator true
        ///   on the expression if it exists.
        /// </summary>
        protected static Expression GetOperatorTrue(ParseContext ec, Expression e, SourceSpan loc)
        {
            return GetOperatorTrueOrFalse(ec, e, true, loc);
        }

        /// <summary>
        ///   Returns an expression that can be used to invoke operator false
        ///   on the expression if it exists.
        /// </summary>
        public static Expression GetOperatorFalse(ParseContext ec, Expression e, SourceSpan loc)
        {
            return GetOperatorTrueOrFalse(ec, e, false, loc);
        }

        static Expression GetOperatorTrueOrFalse(ParseContext ec, Expression e, bool isTrue, SourceSpan location)
        {
            ExpressionType @operator = isTrue ? ExpressionType.IsTrue : ExpressionType.IsFalse;
            string methodName = OperatorInfo.GetOperatorInfo(@operator).SignatureName;

            MethodGroupExpression operatorGroup = MethodLookup(ec, null, e.Type, methodName, location);
            if (operatorGroup == null)
            {
                return null;
            }

            Arguments arguments = new Arguments(1) { new Argument(e) };

            operatorGroup = operatorGroup.OverloadResolve(
                ec,
                ref arguments,
                false,
                location);

            return operatorGroup == null ? null : new UserOperatorCall(operatorGroup, arguments, location);
        }

        protected Expression CreateExpressionFactoryCall(ParseContext ec, string name, Arguments args)
        {
            return CreateExpressionFactoryCall(ec, name, null, args, Span);
        }

        protected Expression CreateExpressionFactoryCall(ParseContext ec, string name, TypeArguments typeArguments, Arguments args)
        {
            return CreateExpressionFactoryCall(ec, name, typeArguments, args, Span);
        }

        public static Expression CreateExpressionFactoryCall(ParseContext ec, string name, TypeArguments typeArguments, Arguments args, SourceSpan location)
        {
            return new InvokeExpression(
                new MemberAccessExpression(
                    TypeManager.ExpressionTypeExpression,
                    name,
                    typeArguments,
                    location),
                args);
        }
    }
}