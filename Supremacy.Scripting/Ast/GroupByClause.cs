using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class GroupByClause : QueryClause
    {
        private Expression _discriminator;
        private Expression _projection;

        public Expression Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        public Expression Discriminator
        {
            get { return _discriminator; }
            set { _discriminator = value; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as GroupByClause;
            if (clone == null)
                return;

            clone._discriminator = Clone(cloneContext, _discriminator);
            clone._projection = Clone(cloneContext, _projection);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _discriminator, prefix, postfix);
            Walk(ref _projection, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("group ");

            DumpChild(_projection, sw, indentChange);

            sw.Write(" by ");

            DumpChild(_discriminator, sw, indentChange);
        }

        protected override string MethodName
        {
            get { return "GroupBy"; }
        }
    }
}