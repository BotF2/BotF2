using System;

using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class QueryExpression : QueryClause
    {
        public QueryClause Query
        {
            get { return Next; }
            set { Next = value; }
        }

/*
        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            var query = this.Query;
            Walk(ref query, prefix, postfix);
            this.Query = query;
        }
*/

        public override Expression BuildQueryClause(ParseContext ec, Expression lSide)
        {
            return Next.BuildQueryClause(ec, lSide);
        }

        private Expression _resolved;
        public override Expression DoResolve(ParseContext ec)
        {
            if (_resolved != null)
                return _resolved;

            var oldCounter = QueryScope.TransparentParameter.Counter;

            var e = BuildQueryClause(ec, null);

            //var oldScope = ec.CurrentScope;
            //ec.CurrentScope = this.Scope;
            e = e.Resolve(ec);
            //ec.CurrentScope = oldScope;

            //
            // Reset counter in probing mode to ensure that all transparent
            // identifier anonymous types are created only once
            //
            if (ec.IsInProbingMode)
                QueryScope.TransparentParameter.Counter = oldCounter;

            return (Expression = _resolved = e);
        }

        protected override string MethodName
        {
            get { throw new NotSupportedException(); }
        }

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