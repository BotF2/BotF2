using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class SimpleQueryExpression : Expression
    {
        private Expression _projection;

        public Expression Projection
        {
            get => _projection;
            set => _projection = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _projection, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("select ");
            DumpChild(_projection, sw, indentChange);
        }
    }
}