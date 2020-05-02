using System.ComponentModel;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class SelectClause : QueryClause
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Projection
        {
            get => Expression;
            set => Expression = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Expression projection = Projection;
            Walk(ref projection, prefix, postfix);
            Projection = projection;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("select ");
            DumpChild(Projection, sw, indentChange);
        }

        protected override string MethodName => "Select";

        public bool IsRequired => !(Projection is NameExpression nameExpression) ? true : nameExpression.Name != Scope.Parameters.FixedParameters[0].Name;
    }
}