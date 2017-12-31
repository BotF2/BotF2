// WeakList.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Collections
{
    public class WeakList<T>
        : IList<T>
        where T : class
    {
        private readonly object _syncRoot;
        private readonly List<WeakReference<T>> _list;

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        public WeakList()
        {
            _list = new List<WeakReference<T>>();
        }

        public WeakList(object syncRoot)
            : this()
        {
            _syncRoot = syncRoot ?? new object();
        }

        public WeakList(IEnumerable<T> initialContents)
            : this(null, initialContents) {}

        public WeakList(object syncRoot, IEnumerable<T> initialContents)
            : this(syncRoot)
        {
            if (initialContents != null)
                AddRange(initialContents);
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (SyncRoot)
            {
                _list.AddRange(items.Select(o => WeakReference<T>.Create(o)));
            }
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            lock (SyncRoot)
            {
                foreach (var item in items)
                    Remove(item);
            }
        }

        protected static IEnumerable<T> SelectAlive(IEnumerable<WeakReference<T>> source)
        {
            foreach (var target in source.Where(o => o.IsAlive).Select(o => o.Target))
                yield return target;         
        }

        #region Implementation of IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return SelectAlive(_list).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of ICollection<T>
        public void Add(T item)
        {
            lock (SyncRoot)
            {
                _list.Add(WeakReference<T>.Create(item));
            }
        }

        protected void Compact()
        {
            lock (SyncRoot)
            {
                _list.RemoveAll(o => !o.IsAlive);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                _list.Clear();
            }
        }
        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return _list.Any(o => o.IsAlive && (o.Target == item));
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                Compact();
                foreach (var item in this)
                    array[arrayIndex++] = item;
            }
        }
        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return _list.RemoveFirstWhere(o => o.IsAlive && (o.Target == item));
            }
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    Compact();
                    return _list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region Implementation of IList<T>
        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                Compact();
                return _list.FirstIndexWhere(o => o.IsAlive && (o.Target == item));                
            }
        }

        public void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                Compact();
                _list.Insert(index, WeakReference<T>.Create(item));
            }
        }

        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                _list.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    Compact();
                    return _list[index].Target;
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    _list[index] = WeakReference<T>.Create(value);
                }
            }
        }
        #endregion
    }
}