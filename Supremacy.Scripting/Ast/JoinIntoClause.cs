using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class JoinIntoClause : JoinClause
    {
        private RangeDeclaration _intoName;

        public RangeDeclaration IntoName
        {
            get { return _intoName; }
            set { _intoName = value; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as JoinIntoClause;
            if (clone == null)
                return;

            clone._intoName = Clone(cloneContext, _intoName);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _intoName, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("join ");
            DumpChild(VariableName, sw, indentChange);
            sw.Write(" in ");
            DumpChild(Initializer, sw, indentChange);
            sw.Write(" on ");
            DumpChild(LeftKey, sw, indentChange);
            sw.Write(" equals ");
            DumpChild(RightKey, sw, indentChange);
            sw.Write(" into ");
            DumpChild(IntoName, sw, indentChange);

            sw.WriteLine();
            sw.Write(' ', Span.Start.Column - 1);
            DumpChild(Body, sw, indentChange);
        }

        protected override string MethodName
        {
            get { return "GroupJoin"; }
        }
    }
}