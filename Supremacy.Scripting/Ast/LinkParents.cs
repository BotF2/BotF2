using System.Collections.Generic;

namespace Supremacy.Scripting.Ast
{
    public class LinkParents
    {
        private readonly Stack<IAst> _stack = new Stack<IAst>();

        private LinkParents()
        {
            _stack.Push(null);
        }

        public static void Link(IAst node)
        {
            LinkParents parents = new LinkParents();
            Ast.Walk(ref node, parents.Prefix, parents.Postfix);
        }

        public bool Postfix(ref IAst node)
        {
            _ = _stack.Pop();
            return true;
        }

        public bool Prefix(ref IAst node)
        {
            node.ParentAst = _stack.Peek();
            _stack.Push(node);
            return true;
        }
    }
}