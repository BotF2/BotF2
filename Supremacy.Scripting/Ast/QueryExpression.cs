using System;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class QueryExpression : QueryClause
    {
        public QueryClause Query
        {
            get => Next;
            set => Next = value;
        }

        public override Expression BuildQueryClause(ParseContext ec, Expression lSide)
        {
            return Next.BuildQueryClause(ec, lSide);
        }

        private Expression _resolved;
        public override Expression DoResolve(ParseContext ec)
        {
            if (_resolved != null)
            {
                return _resolved;
            }

            int oldCounter = QueryScope.TransparentParameter.Counter;

            Expression e = BuildQueryClause(ec, null);
            e = e.Resolve(ec);

            //
            // Reset counter in probing mode to ensure that all transparent
            // identifier anonymous types are created only once
            //
            if (ec.IsInProbingMode)
            {
                QueryScope.TransparentParameter.Counter = oldCounter;
            }

            return Expression = _resolved = e;
        }

        protected override string MethodName => throw new NotSupportedException();

        public override System.Linq.Expressions.Expression TransformCore(ScriptGenerator generator)
        {
            return Expression.Transform(generator);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            Query.Dump(sw, indentChange);
        }
    }
}