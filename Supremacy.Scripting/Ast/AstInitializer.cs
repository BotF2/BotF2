using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public static class AstInitializer
    {
        public static void Initialize(ParseContext parseContext, IAst ast)
        {
            Ast.Walk(
                ref ast,
                (ref IAst o) => Prefix(parseContext, ref o),
                (ref IAst o) => Postfix(parseContext, ref o));
        }

        public static void Initialize(ParseContext parseContext, ref Expression expression)
        {
            Ast.Walk(
                ref expression,
                (ref IAst o) => Prefix(parseContext, ref o),
                (ref IAst o) => Postfix(parseContext, ref o));
        }

        private static bool Prefix(ParseContext parseContext, ref IAst ast)
        {
            ast.BeginInit(parseContext, true);
            return true;
        }

        private static bool Postfix(ParseContext parseContext, ref IAst ast)
        {
            ast.EndInit(parseContext);
            return false;
        }
    }
}