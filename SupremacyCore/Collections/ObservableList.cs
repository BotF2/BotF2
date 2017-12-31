// ObservableList.cs
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
using System.Collections.Specialized;
using System.ComponentModel;

using Supremacy.Annotations;

namespace Supremacy.Collections
{
    [Serializable]
    public class ObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<T> _internalList;

        public ObservableList()
        {
            _internalList = new List<T>();
        }

        public ObservableList([NotNull] IEnumerable<T> initialItems)
        {
            if (initialItems == null)
                throw new ArgumentNullException("initialItems");
            _internalList = new List<T>(initialItems);
        }

        public void AddRange([NotNull] IEnumerable<T> items)
        {
            InsertRange(Count, items);
        }

        public void InsertRange(int index, [NotNull] IEnumerable<T> items)
        {
            if ((index < 0) || (index > Count))
                throw new ArgumentOutOfRangeException("index");
            if (items == null)
                throw new ArgumentNullException("items");

            _internalList.InsertRange(index, items);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items, index);
            OnPropertyChanged("Count");
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _internalList.Insert(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            OnPropertyChanged("Count");
        }

        public void RemoveAt(int index)
        {
            if (index >= Count)
                throw new ArgumentOutOfRangeException("index");
            var oldItem = _internalList[index];
            _internalList.RemoveAt(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem, index);
            OnPropertyChanged("Count");
        }

        public T this[int index]
        {
            get { return _internalList[index]; }
            set
            {
                object oldItem = null;
                if (index < Count)
                    oldItem = _internalList[index];
                _internalList[index] = value;
                OnCollectionChanged(
                    NotifyCollectionChangedAction.Replace,
                    oldItem,
                    value,
                    index);
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            _internalList.Clear();
            OnCollectionReset();
            OnPropertyChanged("Count");
        }

        public bool Contains(T item)
        {
            return _internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var index = _internalList.IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    changedItem,
                    index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList changedItems, int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    changedItems,
                    index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    newItem,
                    oldItem,
                    index));
        }
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    newItems,
                    oldItems));
        }

        protected void OnCollectionReset()
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
