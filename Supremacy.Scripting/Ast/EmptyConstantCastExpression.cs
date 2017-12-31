using System;

using Microsoft.Scripting.Actions;

using Supremacy.Annotations;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class EmptyConstantCastExpression : ConstantExpression
    {
        private bool _isImplicitConversionRequired;
        private ConstantExpression _child;

        public EmptyConstantCastExpression([NotNull] ConstantExpression child, Type targetType, bool isImplicitConversionRequired = false)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            _child = child;
            _isImplicitConversionRequired = isImplicitConversionRequired;

            Type = targetType;
            ExpressionClass = _child.ExpressionClass;
        }

        internal EmptyConstantCastExpression()
        {
            // For cloning purposes only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as EmptyConstantCastExpression;
            if (clone == null)
                return;

            clone._isImplicitConversionRequired = _isImplicitConversionRequired;
            clone._child = Clone(cloneContext, _child);
        }

        public ConstantExpression Child
        {
            get { return _child; }
        }

        public override bool IsZeroInteger
        {
            get { return _child.IsZeroInteger; }
        }

        public override object Value
        {
            get { return _child.Value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _child, prefix, postfix);
        }

        public override ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
        {
            // FIXME: check that 'type' can be converted to 'target_type' first
            return _child.ConvertExplicitly(inCheckedContext, targetType);
        }

        public override ConstantExpression ConvertImplicitly(Type targetType)
        {
            // FIXME: Do we need to check user conversions?
            if (!TypeUtils.IsImplicitlyConvertible(Type, targetType))
                return null;
            return _child.ConvertImplicitly(targetType);
        }

        public override MSAst TransformCore(Runtime.ScriptGenerator generator)
        {
            if (_isImplicitConversionRequired)
                return Microsoft.Scripting.Ast.Utils.Convert(_child.Transform(generator), Type);

            return generator.ConvertTo(
                Type, 
                ConversionResultKind.ExplicitCast,
                _child.Transform(generator));
        }
    }
}