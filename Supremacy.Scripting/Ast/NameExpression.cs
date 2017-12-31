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

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Name);

            if (_typeArguments.Count == 0)
                return;

            sw.Write("<");
            for (var i = 0; i < _typeArguments.Count; i++)
            {
                var typeArgument = _typeArguments[i];

                if (i != 0)
                    sw.Write(", ");

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
                return null;

            _inTransit = true;
            
            var e = DoSimpleNameResolve(ec);
            
            _inTransit = false;

            if (e == null)
                return this;

            if ((ec.CurrentScope == null) || 
                ((e is ParameterReference) && (((ParameterReference)e).Parameter is QueryScope.TransparentParameter)) ||
                ec.CurrentScope.CheckInvariantMeaningInBlock(Name, e, Span))
            {
                return e;
            }

            return this;
        }

        private Expression DoSimpleNameResolve(ParseContext ec)
        {
            Expression e = null;

            var currentScope = ec.CurrentScope;
            if (currentScope != null)
            {
                e = currentScope.TopLevel.GetParameterReference(Name, Span);
                if (e != null)
                    return e.Resolve(ec);
            }

            var languageContext = (ScriptLanguageContext)ec.Compiler.SourceUnit.LanguageContext;
            if (languageContext.TryResolveType(this, out Type type, out int code, out string message))
            {
                if (type.IsGenericType)
                    return new GenericTypeExpression(type, TypeArguments, Span);
                return new TypeExpression(type);
            }

            return e;
        }

        public bool IdenticalNameAndTypeName(ParseContext parseContext, Expression resolvedTo, SourceSpan span)
        {
            var languageContext = (ScriptLanguageContext)parseContext.Compiler.SourceUnit.LanguageContext;
            return (resolvedTo != null) && (resolvedTo.Type != null) &&
                   (resolvedTo.Type.Name == Name) &&
                   languageContext.TryResolveType(this, out Type type, out int errorCode, out string errorMessage);
        }

        public override FullNamedExpression ResolveAsTypeStep(ParseContext ec, bool silent)
        {
            var errors = ec.CompilerErrorCount;
            var fne = ec.LookupNamespaceOrType(
                Name,
                Span,
                ignoreAmbiguousReferences: false,
                genericArity: HasTypeArguments ? TypeArguments.Count : 0);

            if (fne != null)
            {
                if (fne.Type == null)
                    return fne;

                var nested = ResolveNested(fne.Type);
                if (nested != null)
                    return nested.ResolveAsTypeStep(ec, false);

                if (HasTypeArguments)
                {
                    if (TypeManager.IsGenericType(fne.Type))
                    {
                        var ct = new GenericTypeExpression(fne.Type, TypeArguments, Span);
                        return ct.ResolveAsTypeStep(ec, false);
                    }

                    fne.OnErrorTypeArgumentsCannotBeUsed(ec, Span);
                }

                return fne;
            }

            if (silent || errors != ec.CompilerErrorCount)
                return null;

            OnErrorTypeOrNamespaceNotFound(ec);
            return null;
        }

        protected virtual void OnErrorTypeOrNamespaceNotFound(ParseContext ec)
        {
            if (HasTypeArguments)
            {
                var retval = ec.LookupNamespaceOrType(ReflectionUtils.GetNormalizedTypeName(Name), Span, true);
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
                    return true;

                parent = parent.BaseType;
            }

            return false;
        }

        private FullNamedExpression ResolveNested(Type t)
        {
            if (!TypeManager.IsGenericTypeDefinition(t) && !TypeManager.IsGenericType(t))
                return null;

            var declaringType = t;
            while (declaringType != null && !IsNestedChild(t, declaringType))
                declaringType = declaringType.DeclaringType;

            if (declaringType == null)
                return null;

            var genericParameters = t.GetGenericArguments();
            var genericArity = HasTypeArguments ? TypeArguments.Count : 0;

            for (;
                (declaringType != null) && TypeManager.IsGenericType(declaringType);
                declaringType = declaringType.DeclaringType)
            {
                var genericArguments = declaringType.GetGenericArguments();

                if ((genericArity + genericArguments.Length) != genericParameters.Length)
                    continue;

                var newTypeArguments = new TypeArguments();

                foreach (var typeArgument in genericArguments)
                    newTypeArguments.Add(new TypeExpression(typeArgument, Span));

                if (HasTypeArguments)
                    newTypeArguments.Add(TypeArguments);

                return new GenericTypeExpression(t, newTypeArguments, Span);
            }

            return null;
        }
    }
}