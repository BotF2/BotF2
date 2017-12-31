using M = Microsoft.Modeling.Languages;
using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Expressions.Ast
{
    public class SimpleQueryExpression : Expression
    {
        public IExpression Projection { get; set; }
    }
}