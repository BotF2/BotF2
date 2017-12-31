using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class ThenbyClause : QueryClause
    {
        public OrderingDirection? Direction { get; set; }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            DumpChild(Expression, sw, indentChange);

            if (Direction.HasValue)
            {
                sw.Write(" ");
                sw.Write(Direction.Value.ToString().ToLowerInvariant());
            }

            if (Next is ThenbyClause)
                sw.Write(", ");
            else
                sw.WriteLine();

            DumpChild(Next, sw, indentChange);
        }

        protected override string MethodName
        {
            get { return (Direction == OrderingDirection.Descending) ? "ThenByDescending" : "ThenBy"; }
        }
    }
}