using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class Declaration : Ast
    {
        public string VariableName { get; set; }

        public virtual FullNamedExpression ElementType
        {
            // ReSharper disable ValueParameterNotUsed
            get { return null; }
            set { }
            // ReSharper restore ValueParameterNotUsed
        }

        public bool HasExplicitType
        {
            get { return (ElementType != null); }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as Declaration;
            if (clone == null)
                return;

            clone.VariableName = VariableName;
            clone.ElementType = Clone(cloneContext, ElementType);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            var elementType = ElementType;
            Walk(ref elementType, prefix, postfix);
            ElementType = elementType;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var elementType = ElementType;
            if (elementType != null)
            {
                elementType.Dump(sw, indentChange);
                sw.Write(" ");
            }

            sw.Write(VariableName);
        }
    }
}