using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class Declaration : Ast
    {
        public string VariableName { get; set; }

        public virtual FullNamedExpression ElementType
        {

            get => null;
            set { }

        }

        public bool HasExplicitType => ElementType != null;

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is Declaration clone))
            {
                return;
            }

            clone.VariableName = VariableName;
            clone.ElementType = Clone(cloneContext, ElementType);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            FullNamedExpression elementType = ElementType;
            Walk(ref elementType, prefix, postfix);
            ElementType = elementType;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            FullNamedExpression elementType = ElementType;
            if (elementType != null)
            {
                elementType.Dump(sw, indentChange);
                sw.Write(" ");
            }

            sw.Write(VariableName);
        }
    }
}