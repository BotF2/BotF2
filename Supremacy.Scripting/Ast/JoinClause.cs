using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class JoinClause : QueryClause
    {
        private Expression _body;
        private Expression _initializer;
        private Expression _leftKey;
        private Expression _rightKey;
        private RangeDeclaration _variableName;

        public RangeDeclaration VariableName
        {
            get => _variableName;
            set => _variableName = value;
        }

        public Expression Initializer
        {
            get => _initializer;
            set => _initializer = value;
        }

        public Expression Body
        {
            get => _body;
            set => _body = value;
        }

        public Expression LeftKey
        {
            get => _leftKey;
            set => _leftKey = value;
        }

        public Expression RightKey
        {
            get => _rightKey;
            set => _rightKey = value;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is JoinClause clone))
            {
                return;
            }

            clone._variableName = Clone(cloneContext, _variableName);
            clone._initializer = Clone(cloneContext, _initializer);
            clone._leftKey = Clone(cloneContext, _leftKey);
            clone._rightKey = Clone(cloneContext, _rightKey);
            clone._body = Clone(cloneContext, _body);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            base.Walk(prefix, postfix);

            Walk(ref _variableName, prefix, postfix);
            Walk(ref _initializer, prefix, postfix);
            Walk(ref _leftKey, prefix, postfix);
            Walk(ref _rightKey, prefix, postfix);
            Walk(ref _body, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("join ");
            DumpChild(_variableName, sw, indentChange);
            sw.Write(" in ");
            DumpChild(_initializer, sw, indentChange);
            sw.Write(" on ");
            DumpChild(_leftKey, sw, indentChange);
            sw.Write(" equals ");
            DumpChild(_rightKey, sw, indentChange);

            sw.WriteLine();
            sw.Write(' ', Span.Start.Column - 1);
            DumpChild(_body, sw, indentChange);
        }

        protected override string MethodName => "Join";
    }
}