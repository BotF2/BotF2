using System;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    ///   The value is constant, but when emitted has a side effect.  This is
    ///   used by BitwiseAnd to ensure that the second expression is invoked
    ///   regardless of the value of the left side.  
    /// </summary>
    public class SideEffectConstantExpression : ConstantExpression
    {
        private readonly ConstantExpression _value;
        private readonly Expression _sideEffect;

        public SideEffectConstantExpression(ConstantExpression value, Expression sideEffect, SourceSpan location)
        {
            _value = value;
            
            while (sideEffect is SideEffectConstantExpression)
                sideEffect = ((SideEffectConstantExpression)sideEffect)._sideEffect;

            _sideEffect = sideEffect;

            Span = location;
            Type = value.Type;
            ExpressionClass = ExpressionClass.Value;
        }

        public Expression SideEffect
        {
            get { return _sideEffect; }
        }

        public ConstantExpression ConstantValue
        {
            get { return _value; }
        }

        public override bool IsZeroInteger
        {
            get { return _value.IsZeroInteger; }
        }

        public override ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
        {
            var newValue = _value.ConvertExplicitly(inCheckedContext, targetType);
            
            if (newValue == null)
                return null;

            return new SideEffectConstantExpression(newValue, _sideEffect, newValue.Span);
        }
    }
}