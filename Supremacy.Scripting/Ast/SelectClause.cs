using System.ComponentModel;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class SelectClause : QueryClause
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Projection
        {
            get { return Expression; }
            set { Expression = value; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            var projection = Projection;
            Walk(ref projection, prefix, postfix);
            Projection = projection;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("select ");
            DumpChild(Projection, sw, indentChange);
        }

        protected override string MethodName
        {
            get { return "Select"; }
        }

        public bool IsRequired
        {
            get
            {
                var nameExpression = Projection as NameExpression;
                if (nameExpression == null)
                    return true;
                return (nameExpression.Name != Scope.Parameters.FixedParameters[0].Name);
            }
        }
    }
}