using System;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class NameExpression : TypeNameExpression
    {
        private readonly TypeArguments _typeArguments;

        public NameExpression()
        {
            _typeArguments = new TypeArguments();
        }

        public override bool IsPrimaryExpression => true;

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Name);

            if (_typeArguments.Count == 0)
            {
                return;
            }

            sw.Write("<");
            for (int i = 0; i < _typeArguments.Count; i++)
            {
                FullNamedExpression typeArgument = _typeArguments[i];

                if (i != 0)
                {
                    sw.Write(", ");
                }

                typeArgument.Dump(sw, indentChange);
            }
            sw.Write(">");
        }

        [DebuggerStepThrough]
        public override Expression DoResolve(ParseContext ec)
        {
            return SimpleNameResolve(ec);
        }

        [DebuggerStepThrough]
        public override Expression DoResolveLValue(ParseContext ec, Expression rightSide)
        {
            return SimpleNameResolve(ec);
        }

        [DebuggerStepThrough]
        public Expression DoResolve(ParseContext ec, bool intermediate)
        {
            return SimpleNameResolve(ec);
        }

        private bool _inTransit;

        [DebuggerStepThrough]
        private Expression SimpleNameResolve(ParseContext ec)
        {
            if (_inTransit)
            {
                return null;
            }

            _inTransit = true;

            Expression e = DoSimpleNameResolve(ec);
            
            _inTransit = false;

            if (e == null)
            {
                return this;
            }

            return (ec.CurrentScope == null) ||
                ((e is ParameterReference) && (((ParameterReference)e).Parameter is QueryScope.TransparentParameter)) ||
                ec.CurrentScope.CheckInvariantMeaningInBlock(Name, e, Span)
                ? e
                : (this);
        }

        private Expression DoSimpleNameResolve(ParseContext ec)
        {
            Expression e = null;

            Scope currentScope = ec.CurrentScope;
            if (currentScope != null)
            {
                e = currentScope.TopLevel.GetParameterReference(Name, Span);
                if (e != null)
                {
                    return e.Resolve(ec);
                }
            }

            ScriptLanguageContext languageContext = (ScriptLanguageContext)ec.Compiler.SourceUnit.LanguageContext;
            return languageContext.TryResolveType(this, out Type type, out _, out _)
                ? type.IsGenericType ? new GenericTypeExpression(type, TypeArguments, Span) : new TypeExpression(type)
                : e;
        }

        public bool IdenticalNameAndTypeName(ParseContext parseContext, Expression resolvedTo, SourceSpan span)
        {
            ScriptLanguageContext languageContext = (ScriptLanguageContext)parseContext.Compiler.SourceUnit.LanguageContext;
            return (resolvedTo != null) && (resolvedTo.Type != null) &&
                   (resolvedTo.Type.Name == Name) &&
                   languageContext.TryResolveType(this, out _, out _, out _);
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            int errors = ec.CompilerErrorCount;
            FullNamedExpression fne = ec.LookupNamespaceOrType(
                Name,
                Span,
                ignoreAmbiguousReferences: false,
                genericArity: HasTypeArguments ? TypeArguments.Count : 0);

            if (fne != null)
            {
                if (fne.Type == null)
                {
                    return fne;
                }

                FullNamedExpression nested = ResolveNested(fne.Type);
                if (nested != null)
                {
                    return nested.ResolveAsTypeStep(ec, false);
                }

                if (HasTypeArguments)
                {
                    if (TypeManager.IsGenericType(fne.Type))
                    {
                        GenericTypeExpression ct = new GenericTypeExpression(fne.Type, TypeArguments, Span);
                        return ct.ResolveAsTypeStep(ec, false);
                    }

                    fne.OnErrorTypeArgumentsCannotBeUsed(ec, Span);
                }

                return fne;
            }

            if (silent || errors != ec.CompilerErrorCount)
            {
                return null;
            }

            OnErrorTypeOrNamespaceNotFound(ec);
            return null;
        }

        protected virtual void OnErrorTypeOrNamespaceNotFound(ParseContext ec)
        {
            if (HasTypeArguments)
            {
                FullNamedExpression retval = ec.LookupNamespaceOrType(ReflectionUtils.GetNormalizedTypeName(Name), Span, true);
                if (retval != null)
                {
                    retval.OnErrorTypeArgumentsCannotBeUsed(ec, Span);
                    return;
                }
            }

            ec.ReportError(
                CompilerErrors.TypeOrNamespaceNotFound,
                Span,
                Name);
        }

        private static bool IsNestedChild(Type t, Type parent)
        {
            while (parent != null)
            {
                if (TypeManager.IsNestedChildOf(t, TypeManager.DropGenericTypeArguments(parent)))
                {
                    return true;
                }

                parent = parent.BaseType;
            }

            return false;
        }

        private FullNamedExpression ResolveNested(Type t)
        {
            if (!TypeManager.IsGenericTypeDefinition(t) && !TypeManager.IsGenericType(t))
            {
                return null;
            }

            Type declaringType = t;
            while (declaringType != null && !IsNestedChild(t, declaringType))
            {
                declaringType = declaringType.DeclaringType;
            }

            if (declaringType == null)
            {
                return null;
            }

            Type[] genericParameters = t.GetGenericArguments();
            int genericArity = HasTypeArguments ? TypeArguments.Count : 0;

            for (;
                (declaringType != null) && TypeManager.IsGenericType(declaringType);
                declaringType = declaringType.DeclaringType)
            {
                Type[] genericArguments = declaringType.GetGenericArguments();

                if ((genericArity + genericArguments.Length) != genericParameters.Length)
                {
                    continue;
                }

                TypeArguments newTypeArguments = new TypeArguments();

                foreach (Type typeArgument in genericArguments)
                {
                    newTypeArguments.Add(new TypeExpression(typeArgument, Span));
                }

                if (HasTypeArguments)
                {
                    newTypeArguments.Add(TypeArguments);
                }

                return new GenericTypeExpression(t, newTypeArguments, Span);
            }

            return null;
        }
    }
}