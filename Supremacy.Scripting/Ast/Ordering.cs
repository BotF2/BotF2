using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public enum OrderingDirection
    {
        Ascending,
        Descending
    }

    public class Ordering : Ast
    {
        private Expression _expression;

        public OrderingDirection Direction { get; set; }
        
        public Expression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _expression, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            Expression.Dump(sw, indentChange);

            sw.Write(" ");
            sw.Write(Direction.ToString().ToLowerInvariant());
        }
    }
}