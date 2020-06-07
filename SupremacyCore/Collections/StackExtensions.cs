using System;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    public static class StackExtensions
    {
        public static IStack<T> Push<T>(this IStack<T> s, T t)
        {
            return ImmutableStack<T>.Push(t, s);
        }

        public static IStack<T> Reverse<T>(this IStack<T> stack)
        {
            IStack<T> r = ImmutableStack<T>.Empty;
            for (IStack<T> f = stack; !f.IsEmpty; f = f.Pop())
            {
                r = r.Push(f.Peek());
            }

            return r;
        }

        public static bool TryPop<T>(this Stack<T> stack, out T item)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            if (stack.Count == 0)
            {
                item = default;
                return false;
            }

            item = stack.Pop();
            return true;
        }

        public static bool TryPeek<T>(this Stack<T> stack, out T item)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            if (stack.Count == 0)
            {
                item = default;
                return false;
            }

            item = stack.Peek();
            return true;
        }
    }
}