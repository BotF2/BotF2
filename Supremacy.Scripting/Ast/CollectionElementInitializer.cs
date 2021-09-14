using System.Linq;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class CollectionElementInitializer : InvokeExpression
    {
        public override Expression DoResolve(ParseContext parseContext)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
            {
                return this;
            }

            Target = new AddMemberAccessExpression(
                parseContext.CurrentInitializerVariable,
                Span);

            return base.DoResolve(parseContext);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("{ ");
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i != 0)
                {
                    sw.Write(", ");
                }

                DumpChild(Arguments[i], sw, indentChange);
            }
            sw.Write(" }");
        }

        public MSAst.ElementInit TransformInitializer(ScriptGenerator generator)
        {
            return MSAst.Expression.ElementInit(
                (MethodInfo)MethodGroup,
                Arguments.Select(o => o.Value.Transform(generator)));
        }

        private sealed class AddMemberAccessExpression : MemberAccessExpression
        {
            public AddMemberAccessExpression(Expression left, SourceSpan span)
            {
                Left = left;
                Name = "Add";
                Span = span;
            }


            internal AddMemberAccessExpression()
            {
                // For cloning purposes only.
            }

        }
    }
}