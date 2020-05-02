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
            TypeArguments = typeArguments;

            Span = span;
        }

        internal GenericTypeExpression()
        {
            // For cloning purposes only.
        }

        public TypeArguments TypeArguments { get; private set; }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is GenericTypeExpression clone))
            {
                return;
            }

            clone.TypeArguments = TypeArguments.Clone(cloneContext);
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
            {
                return this;
            }

            ExpressionClass = ExpressionClass.Type;

            if (!TypeArguments.Resolve(parseContext))
            {
                return null;
            }

            _resolvedTypeParameters = _openType.GetGenericArguments();

            Type[] resolvedTypes = TypeArguments.ResolvedTypes;
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
                TypeArguments.ResolvedTypes,
                Span,
                false);
        }

        public override bool CheckAccessLevel(ParseContext parseContext)
        {
            return TypeManager.CheckAccessLevel(parseContext, _openType);
        }

        public override bool IsClass => _openType.IsClass;

        public override bool IsValueType => TypeManager.IsStruct(_openType);

        public override bool IsInterface => _openType.IsInterface;

        public override bool IsSealed => _openType.IsSealed;

        public override bool Equals(object obj)
        {
            if (!(obj is GenericTypeExpression genericTypeExpression))
            {
                return false;
            }

            return (Type == null) || (genericTypeExpression.Type == null) ? false : Type == genericTypeExpression.Type;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (_openType == TypeManager.CoreTypes.GenericNullable)
            {
                if (TypeArguments.ResolvedTypes != null)
                {
                    sw.Write(TypeManager.GetCSharpName(TypeArguments.ResolvedTypes[0]));
                    sw.Write("?");
                    return;
                }

                DumpChild(TypeArguments[0], sw);
                sw.Write("?");
                return;
            }
            if (TypeArguments.ResolvedTypes != null)
            {
                sw.Write(TypeManager.GetCSharpName(_openType.MakeGenericType(TypeArguments.ResolvedTypes)));
                return;
            }

            sw.Write(TypeManager.GetCSharpName(ReflectionUtils.GetNormalizedTypeName(_openType), _openType));
            
            sw.Write("<");

            int i = 0;
            foreach (FullNamedExpression typeArgument in TypeArguments)
            {
                if (i++ != 0)
                {
                    sw.Write(", ");
                }

                typeArgument.Dump(sw);
            }

            sw.Write(">");
        }
    }
}