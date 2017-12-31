// Wheel.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Types
{
    [Serializable]
    public sealed class Wheel<T>
    {
        [Serializable]
        private class WheelNode<TNode>
            where TNode : T
        {
            public WheelNode<TNode> Previous;
            public WheelNode<TNode> Next;
            public TNode Value;
        }

        private WheelNode<T> Head;
        private WheelNode<T> LastInsert;

        public Wheel()
        {
            Head = new WheelNode<T>();
            LastInsert = null;
        }

        public void Insert(T value)
        {
            if (LastInsert == null)
            {
                Head.Value = value;
                Head.Next = Head;
                Head.Previous = Head;
                LastInsert = Head;
            }
            else
            {
                WheelNode<T> newNode = new WheelNode<T>();
                newNode.Value = value;
                lock (this)
                {
                    newNode.Next = LastInsert.Next;
                    newNode.Previous = LastInsert;
                    LastInsert.Next.Previous = newNode;
                    LastInsert.Next = newNode;
                    LastInsert = newNode;
                }
            }
        }

        public bool Contains(T value)
        {
            WheelNode<T> current = Head;
            if (current == null)
                return false;
            do
            {
                if (current.Value.Equals(value))
                    return true;
                current = current.Next;
            }
            while (current != Head);
            return false;
        }

        public int GetDistance(T a, T b)
        {
            int dist1 = 0, dist2 = 0;
            if (!Contains(a) || !Contains(b))
                return -1;
            WheelNode<T> start = Head;
            WheelNode<T> current;
            while (!start.Value.Equals(a))
                start = start.Next;
            current = start;
            while (!current.Value.Equals(b))
            {
                current = current.Next;
                dist1++;
            }
            current = start;
            while (!current.Value.Equals(b))
            {
                current = current.Previous;
                dist2++;
            }
            return Math.Min(dist1, dist2);
        }
    }
}
