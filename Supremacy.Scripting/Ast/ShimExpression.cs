using System;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class ShimExpression : Expression
    {
        private Expression _expression;

        public ShimExpression() {}

        public ShimExpression([NotNull] Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            _expression = expression;
        }

        public Expression Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;

                if (_expression != null)
                    Type = _expression.Type;
            }
        }
        
        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _expression, prefix, postfix);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            return (_expression == null) ? this : _expression.Resolve(parseContext);
        }
    }
}