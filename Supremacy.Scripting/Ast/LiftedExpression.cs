using System;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class LiftedExpression : Expression
    {
        private Expression _child;
        private readonly Unwrap _unwrap;

        public LiftedExpression(Expression child, Unwrap unwrap, Type type)
        {
            _child = child;
            _unwrap = unwrap;

            Type = type;
            Span = child.Span;
        }

        public LiftedExpression(Expression child, Expression unwrap, Type type)
            : this(child, unwrap as Unwrap, type) { }

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return _child.Transform(generator);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            // It's null when lifting non-nullable type
            if (_unwrap == null)
            {
                // S -> T? is wrap only
                if (TypeManager.IsNullableType(Type))
                {
                    return Wrap.Create(_child, Type);
                }

                // S -> T can be simplified
                return _child;
            }

            // Wrap target for T?
            if (TypeManager.IsNullableType(Type))
            {
                _child = Wrap.Create(_child, Type);
                if (_child == null)
                {
                    return null;
                }
            }

            ExpressionClass = ExpressionClass.Value;
            return this;
        }
    }
}