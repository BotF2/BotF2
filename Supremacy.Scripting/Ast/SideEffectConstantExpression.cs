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
        public SideEffectConstantExpression(ConstantExpression value, Expression sideEffect, SourceSpan location)
        {
            ConstantValue = value;

            while (sideEffect is SideEffectConstantExpression)
            {
                sideEffect = ((SideEffectConstantExpression)sideEffect).SideEffect;
            }

            SideEffect = sideEffect;

            Span = location;
            Type = value.Type;
            ExpressionClass = ExpressionClass.Value;
        }

        public Expression SideEffect { get; }

        public ConstantExpression ConstantValue { get; }

        public override bool IsZeroInteger => ConstantValue.IsZeroInteger;

        public override ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
        {
            ConstantExpression newValue = ConstantValue.ConvertExplicitly(inCheckedContext, targetType);

            return newValue == null ? null : new SideEffectConstantExpression(newValue, SideEffect, newValue.Span);
        }
    }
}