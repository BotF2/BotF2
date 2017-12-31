using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   A reference expression to generic type
    /// </summary>	
    class GenericTypeExpression : TypeExpression
    {
        private TypeArguments _typeArguments;
        private Type[] _resolvedTypeParameters;	// TODO: Waiting for constrains check cleanup
        private Type _openType;

        /// <summary>
        ///   Instantiate the generic type `t' with the type arguments `args'.
        ///   Use this constructor if you already know the fully resolved
        ///   generic type.
        /// </summary>		
        public GenericTypeExpression(Type t, TypeArguments typeArguments, SourceSpan span)
            : base(t)
        {
            _openType = t.GetGenericTypeDefinition();
            _typeArguments = typeArguments;

            Span = span;
        }

        internal GenericTypeExpression()
        {
            // For cloning purposes only.
        }

        public TypeArguments TypeArguments
        {
            get { return _typeArguments; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as GenericTypeExpression;
            if (clone == null)
                return;

            clone._typeArguments = _typeArguments.Clone(cloneContext);
            clone._resolvedTypeParameters = _resolvedTypeParameters;
            clone._openType = _openType;
        }

        public override string GetSignatureForError()
        {
            return TypeManager.GetCSharpName(Type);
        }

        protected override TypeExpression DoResolveAsTypeStep(ParseContext parseContext)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
                return this;

            ExpressionClass = ExpressionClass.Type;

            if (!_typeArguments.Resolve(parseContext))
                return null;

            _resolvedTypeParameters = _openType.GetGenericArguments();

            var resolvedTypes = _typeArguments.ResolvedTypes;
            if (resolvedTypes.Length != _resolvedTypeParameters.Length)
            {
                parseContext.ReportError(
                    CompilerErrors.InvalidNumberOfTypeArguments,
                    Span,
                    TypeManager.GetCSharpName(_openType),
                    TypeManager.GetNumberOfTypeArguments(_openType));

                return null;
            }

            // Now bind the parameters
            Type = _openType.MakeGenericType(resolvedTypes);

            return this;
        }

        /// <summary>
        ///   Check the constraints; we're called from ResolveAsTypeTerminal()
        ///   after fully resolving the constructed type.
        /// </summary>
        public bool CheckConstraints(ParseContext ec)
        {
            return ConstraintChecker.CheckConstraints(
                ec,
                _openType,
                _resolvedTypeParameters,
                _typeArguments.ResolvedTypes,
                Span,
                false);
        }

        public override bool CheckAccessLevel(ParseContext parseContext)
        {
            return TypeManager.CheckAccessLevel(parseContext, _openType);
        }

        public override bool IsClass
        {
            get { return _openType.IsClass; }
        }

        public override bool IsValueType
        {
            get { return TypeManager.IsStruct(_openType); }
        }

        public override bool IsInterface
        {
            get { return _openType.IsInterface; }
        }

        public override bool IsSealed
        {
            get { return _openType.IsSealed; }
        }

        public override bool Equals(object obj)
        {
            var genericTypeExpression = obj as GenericTypeExpression;
            if (genericTypeExpression == null)
                return false;

            if ((Type == null) || (genericTypeExpression.Type == null))
                return false;

            return (Type == genericTypeExpression.Type);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (_openType == TypeManager.CoreTypes.GenericNullable)
            {
                if (_typeArguments.ResolvedTypes != null)
                {
                    sw.Write(TypeManager.GetCSharpName(_typeArguments.ResolvedTypes[0]));
                    sw.Write("?");
                    return;
                }

                DumpChild(_typeArguments[0], sw);
                sw.Write("?");
                return;
            }
            if (_typeArguments.ResolvedTypes != null)
            {
                sw.Write(TypeManager.GetCSharpName(_openType.MakeGenericType(_typeArguments.ResolvedTypes)));
                return;
            }

            sw.Write(TypeManager.GetCSharpName(ReflectionUtils.GetNormalizedTypeName(_openType), _openType));
            
            sw.Write("<");

            int i = 0;
            foreach (var typeArgument in _typeArguments)
            {
                if (i++ != 0)
                    sw.Write(", ");
                typeArgument.Dump(sw);
            }

            sw.Write(">");
        }
    }
}