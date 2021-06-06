// PriorityQueue.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Pathfinding
{
    [Serializable]
    internal class PriorityQueue<TPriority, TValue>
    {
        private readonly SortedDictionary<TPriority, Queue<TValue>> _list;

        public PriorityQueue()
        {
            _list = new SortedDictionary<TPriority, Queue<TValue>>();
        }

        public void Enqueue(TPriority priority, TValue value)
        {
            Queue<TValue> q;
            if (!_list.TryGetValue(priority, out q))
            {
                q = new Queue<TValue>();
                _list.Add(priority, q);
            }
            q.Enqueue(value);
        }

        public TValue Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = _list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) // nothing left of the top priority.
                _list.Remove(pair.Key);
            return v;
        }

        public bool IsEmpty => !_list.Any();
    }

    //[Serializable]
    //public class PriorityQueue<T>
    //{
    //    private List<T> _queue;
    //    private Comparer<T> _comparer;

    //    public PriorityQueue(Comparer<T> comparer)
    //    {
    //        _queue = new List<T>();
    //        _comparer = comparer;
    //    }

    //    public PriorityQueue(Comparer<T> comparer, int initialCapacity)
    //    {
    //        _queue = new List<T>(initialCapacity);
    //        _comparer = comparer;
    //    }

    //    public T this[int index]
    //    {
    //        get { return _queue[index]; }
    //    }

    //    protected virtual int Compare(int i, int j)
    //    {
    //        return _comparer.Compare(_queue[i], _queue[j]);
    //    }

    //    protected void Swap(int i, int j)
    //    {
    //        T temp = _queue[i];
    //        _queue[i] = _queue[j];
    //        _queue[j] = temp;
    //    }

    //    public bool IsEmpty
    //    {
    //        get { return (_queue.Count == 0); }
    //    }

    //    public int Push(T item)
    //    {
    //        int p = _queue.Count;
    //        int p2;
    //        _queue.Add(item);
    //        do
    //        {
    //            if (p == 0)
    //                break;
    //            p2 = (p - 1) / 2;
    //            if (Compare(p, p2) < 0)
    //            {
    //                Swap(p, p2);
    //                p = p2;
    //            }
    //            else
    //            {
    //                break;
    //            }
    //        } while (true);
    //        return p;
    //    }

    //    public T Pop()
    //    {
    //        T result = _queue[0];
    //        int p = 0;
    //        int p1;
    //        int p2;
    //        int pn;
    //        _queue[0] = _queue[_queue.Count - 1];
    //        _queue.RemoveAt(_queue.Count - 1);
    //        do
    //        {
    //            pn = p;
    //            p1 = 2 * p + 1;
    //            p2 = 2 * p + 2;
    //            if ((_queue.Count > p1) && (Compare(p, p1) > 0))
    //                p = p1;
    //            if ((_queue.Count > p2) && (Compare(p, p2) > 0))
    //                p = p2;
    //            if (p == pn)
    //                break;
    //            Swap(p, pn);
    //        } while (true);
    //        return result;
    //    }

    //    public void Update(T item)
    //    {
    //        Update(_queue.IndexOf(item));
    //    }

    //    public void Update(int index)
    //    {
    //        if (index < 0)
    //            return;

    //        int p = index;
    //        int p1;
    //        int p2;
    //        int pn;

    //        do
    //        {
    //            if (p == 0)
    //                break;
    //            p2 = (p - 1) / 2;
    //            if (Compare(p, p2) < 0)
    //            {
    //                Swap(p, p2);
    //                p = p2;
    //            }
    //            else
    //            {
    //                break;
    //            }
    //        } while (true);

    //        if (p < index)
    //            return;

    //        do
    //        {
    //            pn = p;
    //            p1 = 2 * p + 1;
    //            p2 = 2 * p + 2;
    //            if ((_queue.Count > p1) && (Compare(p, p1) > 0))
    //                p = p1;
    //            if ((_queue.Count > p2) && (Compare(p, p2) > 0))
    //                p = p2;
    //            if (p == pn)
    //                break;
    //            Swap(p, pn);
    //        } while (true);
    //    }

    //    public void Clear()
    //    {
    //        _queue.Clear();
    //    }
    //}
}
