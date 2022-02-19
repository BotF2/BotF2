using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class LambdaParameter : Ast
    {
        private Expression _type;
        public string Name { get; set; }

        public bool HasExplicitType => _type != null;

        public Expression Type
        {
            get => _type;
            set => _type = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _type, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            Expression type = Type;
            if (type != null)
            {
                type.Dump(sw, indentChange);
                sw.Write(" ");
            }

            sw.Write(Name);
        }
    }
}