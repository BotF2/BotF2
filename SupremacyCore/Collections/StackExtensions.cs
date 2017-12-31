using System;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    public static class StackExtensions
    {
        public static bool TryPop<T>(this Stack<T> stack, out T item)
        {
            if (stack == null)
                throw new ArgumentNullException("stack");
            
            if (stack.Count == 0)
            {
                item = default(T);
                return false;
            }

            item = stack.Pop();
            return true;
        }

        public static bool TryPeek<T>(this Stack<T> stack, out T item)
        {
            if (stack == null)
                throw new ArgumentNullException("stack");
            
            if (stack.Count == 0)
            {
                item = default(T);
                return false;
            }

            item = stack.Peek();
            return true;
        }
    }
}