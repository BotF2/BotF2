using System;

using M = Microsoft.Modeling.Languages;
using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Expressions.Ast
{
    public interface IExpression : M.IAst
    {
        MSAst TransformedExpression { get; }
        MSAst Transform(M.CompilerContext context);
    }

    public class Expression : M.Ast, IExpression
    {
        public MSAst TransformedExpression { get; protected set; }

        public MSAst Transform(M.CompilerContext context)
        {
            return this.TransformedExpression ?? (this.TransformedExpression = TransformCore(context));
        }

        public virtual MSAst TransformCore(M.CompilerContext context)
        {
            throw new NotImplementedException();
        }
    }
}