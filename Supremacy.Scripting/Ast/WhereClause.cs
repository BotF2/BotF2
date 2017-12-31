using System.ComponentModel;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class WhereClause : QueryClause
    {
        //private Expression _body;
        //private Expression _predicate;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Expression Predicate
        {
            get { return Expression; }
            set { Expression = value; }
        }

        public Expression Body
        {
            get { return null; }
            set { return; }
        }

/*
        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _predicate, prefix, postfix);
            Walk(ref _body, prefix, postfix);
        }
*/

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("where ");

            var indentShift = "where ".Length;

            sw.Indent += indentShift;

            try
            {
                DumpChild(Expression, sw, indentChange);
            }
            finally
            {
                sw.Indent -= indentShift;
            }

            sw.WriteLine();

            DumpChild(Next, sw, indentChange);
        }

        protected override string MethodName
        {
            get { return "Where"; }
        }
    }
}