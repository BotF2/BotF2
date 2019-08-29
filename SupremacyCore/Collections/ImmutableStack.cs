using System;
using System.Collections;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    public sealed class ImmutableStack<T> : IStack<T>
    {
        private sealed class EmptyStack : IStack<T>
        {
            public bool IsEmpty
            {
                get { return true; }
            }

            public T Peek()
            {
                throw new Exception("Empty stack");
            }

            public IStack<T> Pop()
            {
                throw new Exception("Empty stack");
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private static readonly EmptyStack s_empty = new EmptyStack();

        public static IStack<T> Empty
        {
            get { return s_empty; }
        }

        private readonly T head;
        private readonly IStack<T> tail;

        private ImmutableStack(T head, IStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public bool IsEmpty
        {
            get { return false; }
        }

        public T Peek()
        {
            return head;
        }

        public IStack<T> Pop()
        {
            return tail;
        }

        public static IStack<T> Push(T head, IStack<T> tail)
        {
            return new ImmutableStack<T>(head, tail);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (IStack<T> stack = this; !stack.IsEmpty; stack = stack.Pop())
                yield return stack.Peek();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
