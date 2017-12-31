// CollectionBase.cs
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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Supremacy.IO.Serialization;
using Supremacy.Types;

namespace Supremacy.Collections
{
    internal sealed class CollectionBaseDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public CollectionBaseDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionBaseDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class CollectionBase<T> : IList<T>, IList, IObservableIndexedCollection<T>, IOwnedDataSerializableAndRecreatable
    {
        private const int DefaultCapacity = 4;
        private IList<T> _items;

        [NonSerialized]
        private object _syncRoot;
        [NonSerialized]
        private StateScope _suppressChangeNotificationsScope;

        public CollectionBase()
            : this(DefaultCapacity)
        {
        }

        public CollectionBase(int initialCapacity)
        {
            _items = new List<T>(initialCapacity);
            _suppressChangeNotificationsScope = new StateScope();
        }

        public CollectionBase(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            _items = list;
            _suppressChangeNotificationsScope = new StateScope();
        }

        public ReadOnlyCollectionBase<T> AsReadOnly()
        {
            return new ReadOnlyCollectionBase<T>(_items);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        protected IList<T> Items
        {
            get { return _items; }
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set
            {
                if (_items.IsReadOnly)
                    ThrowNotSupportedOnReadOnlyCollection();

                if ((index < 0) || (index >= _items.Count))
                    throw new ArgumentOutOfRangeException("index");

                SetItem(index, value);
            }
        }

        public void Add(T item)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            InsertItem(_items.Count, item);
        }

        public void Clear()
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            ClearItems();
        }

        public void CopyTo(T[] array, int index)
        {
            _items.CopyTo(array, index);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if ((index < 0) || (index > _items.Count))
                throw new ArgumentOutOfRangeException("index");

            InsertItem(index, item);
        }

        public bool Remove(T item)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            var index = _items.IndexOf(item);
            if (index < 0)
                return false;

            RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            if ((index < 0) || (index >= _items.Count))
                throw new ArgumentOutOfRangeException("index");

            RemoveItem(index);
        }

        protected virtual void InsertItem(int index, T item)
        {
            _items.Insert(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected virtual void RemoveItem(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        protected virtual void ClearItems()
        {
            _items.Clear();
            OnCollectionReset();
        }

        protected virtual void SetItem(int index, T item)
        {
            var oldItem = _items[index];
            _items[index] = item;
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return _items.IsReadOnly; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    var c = _items as ICollection;
                    if (c != null)
                        _syncRoot = c.SyncRoot;
                    else
                        Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Rank != 1)
                throw new ArgumentException(SR.ArgumentException_ArrayMustBeSingleDimensional, "array");

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException(SR.ArgumentException_ArrayHasNonZeroLowerBound, "array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", SR.ArgumentOutOfRangeException_ValueMustBeNonNegative);

            if ((array.Length - index) < Count)
                throw new ArgumentOutOfRangeException("index", SR.ArgumentOutOfRangeException_IndexIsOutsideArrayBounds);

            var typedArray = array as T[];
            if (typedArray != null)
            {
                _items.CopyTo(typedArray, index);
            }
            else
            {
                var targetType = array.GetType().GetElementType();
                var sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    throw new ArgumentException(SR.ArgumentException_ArrayTypeIncompatibleWithCollectionType, "array");

                /*
                 * We can't cast array of value type to object[], so we don't support widening of primitive types here.
                 */
                var objects = array as object[];
                if (objects == null)
                    throw new ArgumentException(SR.ArgumentException_ArrayTypeIncompatibleWithCollectionType, "array");

                var count = _items.Count;
                try
                {
                    for (var i = 0; i < count; i++)
                        objects[index++] = _items[i];
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(SR.ArgumentException_ArrayTypeIncompatibleWithCollectionType, "array");
                }
            }
        }

        object IList.this[int index]
        {
            get { return _items[index]; }
            set
            {
                VerifyValueType(value);
                this[index] = (T)value;
            }
        }

        bool IList.IsReadOnly
        {
            get { return _items.IsReadOnly; }
        }

        bool IList.IsFixedSize
        {
            get
            {
                // There is no IList<T>.IsFixedSize, so we must assume false if our
                // internal item collection does not implement IList
                var list = _items as IList;
                if (list != null)
                    return list.IsFixedSize;
                return false;
            }
        }

        int IList.Add(object value)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            VerifyValueType(value);
            Add((T)value);
            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
                return IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            VerifyValueType(value);
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (_items.IsReadOnly)
                ThrowNotSupportedOnReadOnlyCollection();

            if (IsCompatibleObject(value))
                Remove((T)value);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            var itemList = items as IList ?? items.ToList();
            var insertionIndex = Count;

            using (_suppressChangeNotificationsScope.Enter())
            {
                foreach (var item in itemList.Cast<T>())
                    Add(item);
            }

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    itemList,
                    insertionIndex));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            var changeEvents = new List<NotifyCollectionChangedEventArgs>();
            var lastItemIndex = -1;
            var firstItemIndex = -1;
            var changedItems = new List<T>();

            using (_suppressChangeNotificationsScope.Enter())
            {
                foreach (var item in items)
                {
                    if (Count == 0)
                        break;

                    var itemIndex = Items.IndexOf(item);

                    RemoveItem(itemIndex);

                    if ((lastItemIndex < 0) || (itemIndex == (lastItemIndex + 1)))
                    {
                        changedItems.Add(item);

                        if (firstItemIndex < 0)
                            firstItemIndex = itemIndex;

                        lastItemIndex = itemIndex;
                    }
                    else
                    {
                        NotifyCollectionChangedEventArgs args;

                        if (changedItems.Count == 1)
                        {
                            args = new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                changedItems[0],
                                firstItemIndex);
                        }
                        else
                        {
                            args = new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                changedItems,
                                firstItemIndex);
                        }

                        changeEvents.Add(args);

                        changedItems = new List<T>();
                        firstItemIndex = -1;
                        lastItemIndex = -1;
                    }
                }
            }

            foreach (var changeEvent in changeEvents)
                OnCollectionChanged(changeEvent);
        }

        private static void ThrowNotSupportedOnReadOnlyCollection()
        {
            throw new InvalidOperationException("Operation is not supported on read-only collections.");
        }

        private static bool IsCompatibleObject(object value)
        {
            return (value is T) || ((value == null) && !typeof(T).IsValueType);
        }

        private static void VerifyValueType(object value)
        {
            if (IsCompatibleObject(value))
                return;

            throw new ArgumentException(
                string.Format(
                    "Value is incompatible with type {0}: {1}",
                    typeof(T).FullName,
                    value));
        }

        public void TrimExcess()
        {
            var itemList = _items as List<T>;
            if (itemList == null)
                return;

            itemList.TrimExcess();
        }

        #region INotifyCollectionChanged Implementation
        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressChangeNotificationsScope.IsWithin)
                return;

            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    items,
                    startingIndex));
        }

        protected void OnCollectionChanged(
            NotifyCollectionChangedAction action,
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

        protected void OnCollectionChanged(
            NotifyCollectionChangedAction action,
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
        #endregion

        #region Deserialization Callback
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_syncRoot == null)
                _syncRoot = new object();
            if (_suppressChangeNotificationsScope == null)
                _suppressChangeNotificationsScope = new StateScope();
        }
        #endregion

        #region Implementation of IOwnedDataSerializable
        public virtual void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Initialize();

            _items = reader.ReadList<T>();
        }

        public virtual void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_items);
        }
        #endregion
    }
}