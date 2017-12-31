using System.Collections.Generic;

using M = Microsoft.Modeling.Languages;
using MSAst = System.Linq.Expressions.Expression;

using System.Linq;

namespace Supremacy.Expressions.Ast
{
    public class ConditionScopeExpression : Expression, M.IScopeAst
    {
        private readonly List<LocalDeclaration> _locals = new List<LocalDeclaration>();

        public IList<LocalDeclaration> Locals
        {
            get { return _locals; }
        }

        public SimpleQueryExpression SimpleQuery { get; set; }
        public QueryExpression Query { get; set; }

        public bool IsSimpleQuery
        {
            get { return (this.SimpleQuery != null); }
        }

        public override System.Linq.Expressions.Expression TransformCore(M.CompilerContext context)
        {
            var location = base.sourceLocationIncludingLeadingWhitespace;

            this.Scope = new M.Scope
                         {
                             Context = context,
                             Members = new M.SymbolTable(null, context),
                             FileName = location.FileName,
                             Span = location.Span
                         };

            var locals = _locals.ToArray();
            
            foreach (var local in locals)
            {
                
            }

            var queryExpression = (IExpression)this.SimpleQuery ?? this.Query;
            if (queryExpression != null)
                return queryExpression.Transform(context);

            context.Error(
                this.sourceLocationIncludingLeadingWhitespace,
                "A condition scope must end with a query expression.");

            return null;
        }

        public M.SymbolTable Members
        {
            get { return (this.Scope != null) ? this.Scope.Members : null; }
        }

        public M.IScope Scope { get; private set; }
    }

    public class QueryExpression : Expression, IQueryExpression
    {
        public RangeDeclaration VariableName { get; set; }
        public IExpression Initializer { get; set; }
        public IQueryBody Body { get; set; }
    }

    public class FromExpression : QueryExpression, IQueryBody {}
    public class IntoExpression : QueryExpression {}
}