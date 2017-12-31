using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class CompilationUnit : Expression
    {
        private readonly List<UsingEntry> _usingDirectives = new List<UsingEntry>();

        public IList<UsingEntry> UsingDirectives
        {
            get { return _usingDirectives; }
        }

        public Expression Expression { get; set; }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            var expression = Expression;
            Walk(ref expression, prefix, postfix);
            Expression = expression;
        }

        public override Expression DoResolve(Runtime.ParseContext parseContext)
        {
            foreach (var usingEntry in _usingDirectives)
                parseContext.AddUsing(usingEntry);

            return Expression.Resolve(parseContext);
        }

        public override void Dump(Runtime.SourceWriter sw, int indentChange)
        {
            foreach (var usingEntry in _usingDirectives)
                sw.WriteLine(usingEntry.ToString());

            DumpChild(Expression, sw, indentChange);
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as CompilationUnit;
            if (clone == null)
                return;

            clone._usingDirectives.AddRange(_usingDirectives.Select(o => o.Clone(cloneContext)));
            clone.Expression = Clone(cloneContext, Expression);
        }
    }
}