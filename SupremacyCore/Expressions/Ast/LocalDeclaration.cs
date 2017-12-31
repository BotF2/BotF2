using M = Microsoft.Modeling.Languages;
using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Expressions.Ast
{
    public class LocalDeclaration : M.Ast
    {
        public string VariableName { get; set; }
        public IExpression Initializer { get; set; }

        public MSAst CreateParameter(M.CompilerContext context)
        {
            return MSAst.Parameter(typeof(object), this.VariableName);
        }
    }
}