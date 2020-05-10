namespace Supremacy.Scripting.Ast
{
    public class QueryDeclaration : RangeDeclaration
    {
        private FullNamedExpression _elementType;

        public override FullNamedExpression ElementType
        {
            get => _elementType;
            set => _elementType = value;
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _elementType, prefix, postfix);
        }
    }
}