using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class LetClause : RangeVariableQueryClause
    {
        public Expression Initializer
        {
            get => Expression;
            set => Expression = value;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("let ");
            DumpChild(RangeVariable, sw, indentChange);
            sw.Write(" = ");
            DumpChild(Initializer, sw, indentChange);

            sw.WriteLine();
            DumpChild(Next, sw, indentChange);
        }

        protected override string MethodName => "Select";

        public override Expression BuildQueryClause(ParseContext ec, Expression leftSide)
        {
            Expression = CreateRangeVariableType(Scope, ec, RangeVariable, Expression);

            return base.BuildQueryClause(ec, leftSide);
        }
    }
}