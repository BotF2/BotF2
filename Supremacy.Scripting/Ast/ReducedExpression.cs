using System;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    /// <summary>
    /// Created during resolving phase when an expression is wrapped or constantified
    /// and original expression can be used later (e.g. for expression trees).
    /// </summary>
    public class ReducedExpression : Expression
    {
        #region Dependent Type: ReducedConstantExpression
        private sealed class ReducedConstantExpression : EmptyConstantCastExpression
        {
            private readonly Expression _originalExpression;

            public ReducedConstantExpression(ConstantExpression baseExpression, Expression originalExpression)
                : base(baseExpression, baseExpression.Type)
            {
                _originalExpression = originalExpression;
            }

            public override ConstantExpression ConvertImplicitly(Type targetType)
            {
                ConstantExpression c = base.ConvertImplicitly(targetType);
                if (c != null)
                {
                    c = new ReducedConstantExpression(c, _originalExpression);
                }

                return c;
            }

            public override ConstantExpression ConvertExplicitly(bool inCheckedContext, Type targetType)
            {
                ConstantExpression c = base.ConvertExplicitly(inCheckedContext, targetType);
                if (c != null)
                {
                    c = new ReducedConstantExpression(c, _originalExpression);
                }

                return c;
            }
        }

        #endregion

        private readonly Expression _baseExpression, _originalExpression;

        private ReducedExpression(Expression baseExpression, Expression originalExpression)
        {
            _baseExpression = baseExpression;
            _originalExpression = originalExpression;

            Span = originalExpression.Span;
        }

        public static ConstantExpression Create(ConstantExpression baseExpression, Expression originalExpression)
        {
            return new ReducedConstantExpression(baseExpression, originalExpression);
        }

        public static Expression Create(Expression expr, Expression originalExpression)
        {
            return expr is ConstantExpression c ? Create(c, originalExpression) : (Expression)new ReducedExpression(expr, originalExpression);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            ExpressionClass = _baseExpression.ExpressionClass;
            Type = _baseExpression.Type;
            return this;
        }

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return _originalExpression.Transform(generator);
        }
    }
}