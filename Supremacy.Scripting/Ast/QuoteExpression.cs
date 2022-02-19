using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    internal class QuoteExpression : ShimExpression
    {
        public QuoteExpression(LambdaExpression expression)
                : base(expression) { }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            return MSAst.Expression.Quote(Expression.Transform(generator));
        }

        public override Expression DoResolve(ParseContext ec)
        {
            LambdaExpression lambdaExpression = (LambdaExpression)Expression;

            if (lambdaExpression == null)
            {
                return null;
            }

            Type = TypeManager.CoreTypes.GenericExpression.MakeGenericType(lambdaExpression.LambdaType);
            return this;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            DumpChild(Expression, sw);
        }
    }
}