// GameObjectList.cs
//
// Copyright (c) 2008 Mike Strobel
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
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Game;

using System.Linq;

using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Collections
{
    [Serializable]
    public class GameObjectList<T> : IList<T>, INotifyCollectionChanged, IIndexedCollection<T>, INotifyPropertyChanged
        where T : GameObject
    {
        private const string CountPropertyName = "Count";
        private const string IndexerName = "Item[]";

        private readonly Func<int, T> _lookupFunction;
        private readonly List<int> _internalList;

        public GameObjectList([NotNull] Func<int, T> lookupFunction)
        {
            if (lookupFunction == null)
                throw new ArgumentNullException("lookupFunction");

            _lookupFunction = lookupFunction;
            _internalList = new List<int>();
        }

        public void TrimExcess()
        {
            _internalList.TrimExcess();
        }

        #region Implementation of IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.Select(id => _lookupFunction(id)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of ICollection<T>
        public void Add([NotNull] T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            Insert(_internalList.Count, item);
        }

        public void Add(int itemId)
        {
            Insert(_internalList.Count, itemId);
        }

        public void AddRange([NotNull] IEnumerable<int> itemIds)
        {
            if (itemIds == null)
                throw new ArgumentNullException("itemIds");

            InsertRange(_internalList.Count, itemIds);
        }

        public void AddRange([NotNull] IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            InsertRange(_internalList.Count, items);
        }

        public void Clear()
        {
            _internalList.Clear();
            OnCollectionReset();
        }

        public bool Contains([CanBeNull] T item)
        {
            if (item == null)
                return false;
            return _internalList.Contains(item.ObjectID);
        }

        public bool Contains(int itemId)
        {
            return _internalList.Contains(itemId);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            _internalList.Select(id => _lookupFunction(id)).ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove([CanBeNull] T item)
        {
            if (item == null)
                return false;
            int index = _internalList.IndexOf(item.ObjectID);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public bool Remove(int itemId)
        {
            var index = _internalList.IndexOf(itemId);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public int Count
        {
            get { return _internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region Implementation of IList<T>
        public int IndexOf([CanBeNull] T item)
        {
            if (item == null)
                return -1;
            return _internalList.IndexOf(item.ObjectID);
        }

        public void Insert(int index, [NotNull] T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            _internalList.Insert(index, item.ObjectID);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void InsertRange(int index, [NotNull] IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            _internalList.InsertRange(index, items.Select(o => o.ObjectID));
            OnCollectionReset();
        }

        private void Insert(int index, int itemId)
        {
            _internalList.Insert(index, itemId);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, _lookupFunction(itemId), index);
        }

        public void InsertRange(int index, [NotNull] IEnumerable<int> itemIds)
        {
            if (itemIds == null)
                throw new ArgumentNullException("itemIds");
            _internalList.InsertRange(index, itemIds);
            OnCollectionReset();
        }

        public void RemoveAt(int index)
        {
            var item = _internalList[index];
            _internalList.RemoveAt(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        T IList<T>.this[int itemId]
        {
            get { return this[itemId]; }
            set { this[itemId] = value; }
        }

        T IIndexedEnumerable<T>.this[int itemId]
        {
            get { return this[itemId]; }
        }

        public T this[int itemId]
        {
            get { return _lookupFunction(_internalList[itemId]); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _internalList[itemId] = value.ObjectID;
            }
        }
        #endregion

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerName);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, Func<object> itemAccessor, int index)
        {
            var handler = CollectionChanged;
            if (handler == null)
                return;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    itemAccessor(),
                    index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
                                           object item,
                                           int index,
                                           int oldIndex)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index,
                    oldIndex));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
                                           object oldItem,
                                           object newItem,
                                           int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    newItem,
                    oldItem,
                    index));
        }

        protected void OnCollectionReset()
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }

    [Serializable]
    public class UniverseObjectList<T> : GameObjectList<T>
        where T : UniverseObject
    {
        public UniverseObjectList()
            : base(id => GameContext.Current.Universe.Get<T>(id)) { }
    }
}