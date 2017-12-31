using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

using Supremacy.Threading;
using Supremacy.Utility;

#pragma warning disable 420

namespace Supremacy.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    internal class ConcurrentStack<T> : IEnumerable<T>, ICollection
    {
        private volatile Node _head;

        public ConcurrentStack()
        {
        }

        public ConcurrentStack(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var node = collection.Aggregate<T, Node>(
                null,
                (current, item) => new Node(item) { Next = current });

            _head = node;
        }

        public bool IsEmpty
        {
            get { return (Interlocked.CompareExchange(ref _head, null, null) == null); }
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            ToList().CopyTo((T[])array, index);
        }

        public int Count
        {
            get
            {
                int num = 0;
                for (Node node = Interlocked.CompareExchange(ref _head, null, null); node != null; node = node.Next)
                {
                    num++;
                }
                return num;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            Node node = Interlocked.CompareExchange(ref _head, null, null);
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
                if (node == _head)
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void Clear()
        {
            Interlocked.Exchange(ref _head, null);
        }

        [SuppressMessage("Warning", "CS0420")]
        public void Push(T item)
        {
            var wait = new SpinWait();
            var node = new Node(item);

            while (true)
            {
                Node head = Interlocked.CompareExchange(ref _head, null, null);
                node.Next = head;
                if (Interlocked.CompareExchange(ref _head, node, head) == head)
                {
                    return;
                }
                wait.Spin();
            }
        }

        public T[] ToArray()
        {
            return ToList().ToArray();
        }

        private List<T> ToList()
        {
            var list = new List<T>();

            for (var node = Interlocked.CompareExchange(ref _head, null, null);
                 node != null;
                 node = node.Next)
            {
                list.Add(node.Value);
            }
            return list;
        }

        public bool TryPeek(out T result)
        {
            Node head = _head;
            if (head == null)
            {
                result = default(T);
                return false;
            }
            result = head.Value;
            return true;
        }

        [SuppressMessage("Warning", "CS0420")]
        public bool CompareAndPop(T comparand)
        {
            SpinWait wait = new SpinWait();
            Retry:
            Node node = Interlocked.CompareExchange(ref _head, null, null);
            if ((node != null) && Equals(node.Value, comparand))
            {
                if (Interlocked.CompareExchange(ref _head, node.Next, node) != node)
                {
                    wait.Spin();
                    goto Retry;
                }
                return true;
            }
            return false;
        }

        [SuppressMessage("Warning", "CS0420")]
        public bool TryPop(out T result)
        {
            var wait = new SpinWait();
        Retry:
            Node node = Interlocked.CompareExchange(ref _head, null, null);
            if (node == null)
            {
                result = default(T);
                return false;
            }
            Node next = node.Next;
            if (Interlocked.CompareExchange(ref _head, next, node) != node)
            {
                wait.Spin();
                goto Retry;
            }
            result = node.Value;
            return true;
        }

        #region Nested type: Node

        private class Node
        {
            internal Node Next;
            internal readonly T Value;

            internal Node(T value)
            {
                Value = value;
                Next = null;
            }
        }

        #endregion

        #region Nested type: SpinWait

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
        private struct SpinWait
        {
            internal const int YieldFrequency = 0xfa0;

            private int _count;

            public void Spin()
            {
                _count = ++_count % 0x7fffffff;
                if (Environment.ProcessorCount == 1)
                {
                    AsyncHelper.Yield();
                }
                else
                {
                    int num = _count % YieldFrequency;
                    if (num > 0)
                    {
                        int iterations = (int)(1f + (num * 0.032f));
                        Thread.SpinWait(iterations);
                    }
                    else
                    {
                        AsyncHelper.Yield();
                    }
                }
            }
        }

        #endregion
    }

    internal class SingleLinkNode<T>
    {
        // Note: the Next member cannot be a property since it participates in
        // many CAS operations
        public SingleLinkNode<T> Next;
        public T Item;
    }

    public class ConcurrentQueue<T>
    {
        private SingleLinkNode<T> _head;
        private SingleLinkNode<T> _tail;

        public ConcurrentQueue()
        {
            _head = new SingleLinkNode<T>();
            _tail = _head;
        }

        public void Enqueue(T item)
        {
            SingleLinkNode<T> oldTail = null;

            SingleLinkNode<T> newNode = new SingleLinkNode<T>
                                        {
                                            Item = item
                                        };

            bool newNodeWasAdded = false;
            while (!newNodeWasAdded)
            {
                oldTail = _tail;
                SingleLinkNode<T> oldTailNext = oldTail.Next;

                if (_tail == oldTail)
                {
                    if (oldTailNext == null)
                        newNodeWasAdded = SyncMethods.CompareAndSwap(ref _tail.Next, null, newNode);
                    else
                        SyncMethods.CompareAndSwap(ref _tail, oldTail, oldTailNext);
                }
            }

            SyncMethods.CompareAndSwap(ref _tail, oldTail, newNode);
        }

        public bool Dequeue(out T item)
        {
            item = default(T);

            bool haveAdvancedHead = false;
            while (!haveAdvancedHead)
            {
                SingleLinkNode<T> oldHead = _head;
                SingleLinkNode<T> oldTail = _tail;
                SingleLinkNode<T> oldHeadNext = oldHead.Next;

                if (oldHead == _head)
                {
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null)
                        {
                            return false;
                        }
                        SyncMethods.CompareAndSwap(ref _tail, oldTail, oldHeadNext);
                    }

                    else
                    {
                        item = oldHeadNext.Item;
                        haveAdvancedHead =
                            SyncMethods.CompareAndSwap(ref _head, oldHead, oldHeadNext);
                    }
                }
            }
            return true;
        }

        public T Dequeue()
        {
            T result;
            Dequeue(out result);
            return result;
        }
    }

    //public class ConcurrentQueue<T> : IEnumerable<T> where T : class
    //{
    //    private InternalQueue<T> _queue;
    //    private int _count;

    //    public int Count
    //    {
    //        get { return _count; }
    //    }

    //    internal class Node<T>
    //    {
    //        public T Value; 
    //        public Node<T> Next; 

    //        public Node(T value, Node<T> next)
    //        {
    //            this.Value = value;
    //            this.Next = next;
    //        }
    //    }

    //    internal class InternalQueue<T>
    //    {
    //        public Node<T> Head = null; 
    //        public Node<T> Tail = null; 
    //    }

    //    public ConcurrentQueue()
    //    {
    //        _queue = new InternalQueue<T>();
    //        Node<T> node = new Node<T>(null, null);
    //        _queue.Head = node;
    //        _queue.Tail = node;
    //    }

    //    public void EnqueueRange(IEnumerable<T> items)
    //    {
    //        foreach (T item in items)
    //            Enqueue(item);
    //    }

    //    public void Enqueue(T value)
    //    {
    //        Node<T> node = new Node<T>(value, null);
    //        Node<T> tail;
    //        Node<T> next;
    //        while (true)
    //        {
    //            tail = _queue.Tail;
    //            next = tail.Next;
    //            if (tail == _queue.Tail) 
    //            {
    //                if (next == null)
    //                {
    //                    if (next == Interlocked.CompareExchange(ref tail.Next, node, next))
    //                        break; 
    //                }
    //                else
    //                {
    //                    Interlocked.CompareExchange(ref _queue.Tail, next, tail);
    //                }
    //            }
    //        } 
    //        Interlocked.CompareExchange(ref _queue.Tail, node, tail);
    //        Interlocked.Increment(ref _count);
    //    }

    //    public T Dequeue()
    //    {
    //        T value;
    //        Node<T> head;
    //        Node<T> tail;
    //        Node<T> next;
    //        while (true)
    //        {
    //            head = _queue.Head;
    //            tail = _queue.Tail;
    //            next = head.Next;
    //            if (head == _queue.Head)
    //            {
    //                if (head == tail) 
    //                {
    //                    if (next == null) 
    //                        return null; 
    //                    Interlocked.CompareExchange(ref _queue.Tail, next, tail);
    //                }
    //                else 
    //                {
    //                    value = next.Value;
    //                    if (Interlocked.CompareExchange(ref _queue.Head, next, head) == head)
    //                    {
    //                        Interlocked.Decrement(ref _count);
    //                        return value;
    //                    }
    //                }
    //            }
    //        } 
    //    }

    //    #region IEnumerable<T> Members
    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        Node<T> node = _queue.Head;
    //        while (node != null)
    //        {
    //            yield return node.Value;
    //            node = node.Next;
    //            if (node == _queue.Head)
    //                break;
    //        }
    //    }
    //    #endregion

    //    #region IEnumerable Members
    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //    #endregion
    //} 

    public interface IStack<T> : IEnumerable<T>
    {
        IStack<T> Pop();
        T Peek();
        bool IsEmpty { get; }
    }

    public interface IQueue<T> : IEnumerable<T>
    {
        bool IsEmpty { get; }
        T Peek();
        IQueue<T> Enqueue(T value);
        IQueue<T> Dequeue();
    }

    public static class Extensions
    {
        public static IStack<T> Push<T>(this IStack<T> s, T t)
        {
            return ImmutableStack<T>.Push(t, s);
        }

        static public IStack<T> Reverse<T>(this IStack<T> stack)
        {
            IStack<T> r = ImmutableStack<T>.Empty;
            for (IStack<T> f = stack; !f.IsEmpty; f = f.Pop())
                r = r.Push(f.Peek());
            return r;
        }
    }

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

    public sealed class ImmutableQueue<T> : IQueue<T> where T : class
    {
        private readonly IStack<T> _backwards;
        private readonly IStack<T> _forwards;

        private sealed class EmptyQueue : IQueue<T>
        {
            public bool IsEmpty
            {
                get { return true; }
            }

            public T Peek()
            {
                throw new Exception("empty queue");
            }

            public IQueue<T> Enqueue(T value)
            {
                return new ImmutableQueue<T>(ImmutableStack<T>.Empty.Push(value), ImmutableStack<T>.Empty);
            }

            public IQueue<T> Dequeue()
            {
                throw new Exception("empty queue");
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

        private static readonly IQueue<T> s_empty = new EmptyQueue();

        public static IQueue<T> Empty
        {
            get { return s_empty; }
        }

        public bool IsEmpty
        {
            get { return false; }
        }

        private ImmutableQueue(IStack<T> f, IStack<T> b)
        {
            _forwards = f;
            _backwards = b;
        }

        public T Peek()
        {
            return _forwards.Peek();
        }

        public IQueue<T> Enqueue(T value)
        {
            return new ImmutableQueue<T>(_forwards, _backwards.Push(value));
        }

        public IQueue<T> EnqueueRange(IEnumerable<T> items)
        {
            IQueue<T> result = this;
            foreach (T item in items)
                result = result.Enqueue(item);
            return result;
        }

        public IQueue<T> Dequeue()
        {
            IStack<T> f = _forwards.Pop();
            if (!f.IsEmpty)
                return new ImmutableQueue<T>(f, _backwards);
            if (_backwards.IsEmpty)
                return Empty;
            return new ImmutableQueue<T>(_backwards.Reverse(), ImmutableStack<T>.Empty);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in _forwards)
                yield return t;
            foreach (T t in _backwards.Reverse())
                yield return t;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

#pragma warning restore 420
