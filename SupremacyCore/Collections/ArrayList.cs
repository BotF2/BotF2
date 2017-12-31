// History.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    [Serializable]
    public class ArrayList<T> : IList<T>, IList, IIndexedCollection<T>
    {
        #region Fields
        private T[] _items;
        #endregion

        #region Constructors
        public ArrayList()
        {
            _items = new T[0];
        }

        public ArrayList(IEnumerable<T> collection) : this()
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            AddRange(collection);
        }

        public ArrayList(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            _items = new T[collection.Count];
            collection.CopyTo(_items, 0);
        }
        #endregion

        #region IList<T> Members
        public int Count
        {
            get { return _items.Length; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public void Add(T item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");
            lock (_items)
            {
                Insert(_items.Length, item);
            }
        }

        public bool Contains(T item)
        {
            lock (_items)
            {
                //if (ReferenceEquals(item, null))
                //{
                //    for (int i = 0; i < _items.Length; i++)
                //    {
                //        if (ReferenceEquals(_items[i], null))
                //        {
                //            return true;
                //        }
                //    }
                //    return false;
                //}
                //EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                //for (int i = 0; i < _items.Length; i++)
                //{
                //    if (comparer.Equals(_items[i], item))
                //    {
                //        return true;
                //    }
                //}
                //return false;
                return ((ICollection<T>)_items).Contains(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_items).GetEnumerator();
        }

        public bool Remove(T item)
        {
            if (!ReferenceEquals(item, null))
            {
                lock (_items)
                {
                    int index = IndexOf(item);
                    if (index != -1)
                    {
                        RemoveAt(index);
                        return true;
                    }
                }
            }
            return false;
        }

        public int IndexOf(T item)
        {
            lock (_items)
            {
                return Array.IndexOf(_items, item);
            }
        }

        public void Insert(int index, T item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");
            lock (_items)
            {
                T[] newItems = new T[_items.Length + 1];
                if (index > 0)
                    Array.Copy(_items, 0, newItems, 0, index);
                newItems[index] = item;
                if ((_items.Length - index) > 0)
                    Array.Copy(_items, index, newItems, index + 1, _items.Length - index);
                _items = newItems;
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "value must be >= 0");
            if (index >= _items.Length)
                throw new ArgumentOutOfRangeException("index", "value must be < Count");
            lock (_items)
            {
                T[] newItems = new T[_items.Length - 1];
                if (index > 0)
                    Array.Copy(_items, 0, newItems, 0, index);
                if ((_items.Length - index - 1) > 0)
                    Array.Copy(_items, index + 1, newItems, index, _items.Length - index - 1);
                _items = newItems;
            }
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public void Clear()
        {
            var items = _items;
            if ((items == null) || (items.Length == 0))
                return;
            lock (this)
            {
                if ((_items == null) || (_items.Length == 0))
                    return;
                _items = new T[0];
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_items)
            {
                _items.CopyTo(array, arrayIndex);
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region Methods
        public int RemoveWhere(Predicate<T> predicate)
        {
            int result = 0;
            if (predicate != null)
            {
                lock (_items)
                {
                    for (int i = 0; i < _items.Length; i++)
                    {
                        if (predicate(_items[i]) && Remove(_items[i]))
                        {
                            ++result;
                            --i;
                        }
                    }
                }
            }
            return result;
        }

        public int RemoveRange(IEnumerable<T> items)
        {
            int result = 0;
            lock (_items)
            {
                foreach (T item in items)
                {
                    if (Remove(item))
                        ++result;
                }
            }
            return result;
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_items)
            {
                foreach (T item in items)
                {
                    Add(item);
                }
            }
        }
        #endregion

        #region IList Members
        int IList.Add(object value)
        {
            Add((T)value);
            return IndexOf((T)value);
        }

        void IList.Clear()
        {
            Clear();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            _items.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return _items.Length; }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }
        #endregion
    }
}