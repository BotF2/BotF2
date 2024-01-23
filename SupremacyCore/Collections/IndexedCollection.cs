// IndexedCollection.cs
//
// Copyright (c) 2007 Aaron Erickson, Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

using Supremacy.Annotations;

using E = System.Linq.Expressions.Expression;
using Supremacy.Utility;

namespace Supremacy.Collections
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IndexableAttribute : Attribute { }

    [Serializable]
    public class IndexedCollection<T> : IIndexedCollection<T>,
                                        IList<T>,
                                        IDeserializationCallback,
                                        INotifyCollectionChanged,
                                        INotifyPropertyChanged
    {
        #region Index Class
        [Serializable]
        protected internal sealed class Index
        {
            public PropertyInfo Property { get; }

            public Func<T, object> Accessor { get; }

            public Dictionary<int, HashSet<T>> LookupTable { get; }

            public Index(PropertyInfo property)
            {
                Property = property;
                LookupTable = new Dictionary<int, HashSet<T>>(1);

                ParameterExpression itemParameter = E.Parameter(typeof(T));
                E propertyAccess = E.Property(itemParameter, property);

                if (propertyAccess.Type != typeof(object))
                {
                    propertyAccess = E.Convert(propertyAccess, typeof(object));
                }

                Accessor = E.Lambda<Func<T, object>>(propertyAccess, itemParameter).Compile();
            }
        }
        #endregion

        public const int DefaultMaxKeyCount = 32;
        public const int InfiniteMaxKeyCount = int.MaxValue;

        private bool _isChangeNotificationEnabled;
        private readonly int _maxKeyCount;

        [NonSerialized]
        protected ReaderWriterLockSlim SyncLock;

        [NonSerialized] private Dictionary<string, Index> _indexes;
        private string _text;

        public IEqualityComparer<T> Comparer { get; }

        public IndexedCollection()
            : this(InfiniteMaxKeyCount) { }

        public IndexedCollection(int maxKeyCount)
            : this(maxKeyCount, (IList<T>)null) { }

        public IndexedCollection([CanBeNull] IIndexedEnumerable<T> initialContents)
            : this(DefaultMaxKeyCount, initialContents) { }

        public IndexedCollection(int maxKeyCount, [CanBeNull] IIndexedEnumerable<T> initialContents)
            : this(maxKeyCount, initialContents, EqualityComparer<T>.Default) { }

        public IndexedCollection(int maxKeyCount, [CanBeNull] IIndexedEnumerable<T> initialContents, [NotNull] IEqualityComparer<T> comparer)
        {
            SyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            _maxKeyCount = maxKeyCount;
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

            if (initialContents != null)
            {
                InternalCollection = new List<T>(initialContents.Count);
                initialContents.CopyTo(InternalCollection, 0, initialContents.Count);
            }
            else
            {
                InternalCollection = new List<T>();
            }

            BuildIndexes();
        }

        public IndexedCollection([CanBeNull] IList<T> initialContents)
            : this(DefaultMaxKeyCount, initialContents) { }

        public IndexedCollection(int maxKeyCount, [CanBeNull] IList<T> initialContents)
            : this(maxKeyCount, initialContents, EqualityComparer<T>.Default) { }

        public IndexedCollection(int maxKeyCount, [CanBeNull] IList<T> initialContents, [NotNull] IEqualityComparer<T> comparer)
        {
            SyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            _maxKeyCount = maxKeyCount;
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

            if (initialContents != null)
            {
                InternalCollection = new List<T>(initialContents.Count);
                initialContents.CopyTo(InternalCollection);
            }
            else
            {
                InternalCollection = new List<T>();
            }

            BuildIndexes();
        }

        protected IList<T> InternalCollection { get; }

        public bool IsChangeNotificationEnabled
        {
            get => _isChangeNotificationEnabled;
            set
            {
                if (_isChangeNotificationEnabled == value)
                {
                    return;
                }

                SyncLock.EnterReadLock();

                try
                {
                    if (_isChangeNotificationEnabled == value)
                    {
                        return;
                    }

                    _isChangeNotificationEnabled = value;

                    foreach (T item in InternalCollection)
                    {
                        if (!(item is INotifyPropertyChanged notifyPropertyChangedItem))
                        {
                            continue;
                        }

                        if (value)
                        {
                            notifyPropertyChangedItem.PropertyChanged += ItemPropertyChangedCallback;
                        }
                        else
                        {
                            notifyPropertyChangedItem.PropertyChanged -= ItemPropertyChangedCallback;
                        }
                    }
                }
                finally
                {
                    SyncLock.ExitReadLock();
                }
            }
        }

        public void BuildIndexes()
        {
            if (_indexes != null)
            {
                return;
            }

            SyncLock.EnterReadLock();

            try
            {
                _indexes = new Dictionary<string, Index>();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    foreach (Attribute attribute in prop.GetCustomAttributes(true))
                    {
                        if (attribute is IndexableAttribute)
                        {
                            _indexes.Add(prop.Name, new Index(prop));
                        }
                    }
                }

                foreach (T item in InternalCollection)
                {
                    foreach (Index index in _indexes.Values)
                    {
                        InsertIndexValue(index, item);
                    }
                }
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        protected internal int GetIndexableHashCode(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Math.Abs(value.GetHashCode()) % _maxKeyCount;
        }

        public void OnItemPropertyChanged(T source, string propertyName)
        {
            if (Contains(source))
            {
                OnItemPropertyChangedInternal(source, propertyName);
            }
        }

        protected void OnItemPropertyChangedInternal(T source, string propertyName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            Index index = GetIndexByProperty(propertyName);
            if (index == null)
            {
                return;
            }

            object propertyValue = index.Accessor(source);
            if (propertyValue == null)
            {
                return;
            }

            int hashCode = GetIndexableHashCode(propertyValue);

            SyncLock.EnterReadLock();

            try
            {
                if (!index.LookupTable.ContainsKey(hashCode))
                {
                    index.LookupTable.Add(hashCode, new HashSet<T>());
                }
                else if (index.LookupTable[hashCode].Contains(source))
                {
                    return;
                }

                _ = index.LookupTable[hashCode].Add(source);

                IList<int> removedKeys = null;

                foreach (int lookupIndex in index.LookupTable.Keys)
                {
                    if (lookupIndex == hashCode)
                    {
                        continue;
                    }

                    if (index.LookupTable[lookupIndex].Remove(source) &&
                        index.LookupTable[lookupIndex].Count == 0)
                    {
                        if (removedKeys == null)
                        {
                            removedKeys = new List<int>();
                        }

                        removedKeys.Add(lookupIndex);
                    }
                }

                if (removedKeys != null)
                {
                    index.LookupTable.RemoveRange(removedKeys);
                }
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        public bool PropertyHasIndex(string propertyName)
        {
            return _indexes.ContainsKey(propertyName);
        }

        public bool PropertyHasIndex(PropertyInfo property)
        {
            return property != null && _indexes.ContainsKey(property.Name);
        }

        protected internal Index GetIndexByProperty(string propName)
        {
            return _indexes.ContainsKey(propName) ? _indexes[propName] : null;
        }

        private void ItemPropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            OnItemPropertyChangedInternal((T)sender, e.PropertyName);
        }

        protected void InsertIndexValue(Index index, T item)
        {
            object propertyValue = index.Accessor(item);
            if (propertyValue == null)
            {
                return;
            }

            int hashCode = GetIndexableHashCode(propertyValue);
            if (index.LookupTable.ContainsKey(hashCode))
            {
                _ = index.LookupTable[hashCode].Add(item);
            }
            else
            {
                index.LookupTable.Add(hashCode, new HashSet<T> { item });
            }
        }

        #region ICollection<T> Members
        public void Add(T item)
        {
            InsertItem(Count, item, false);
        }

        protected void InsertItem(int listIndex, T item, bool upgradeableLockAlreadyHeld)
        {
            //_text = "Step_4100: listIndex= " + listIndex + ", item= " + item.ToString();
            //Console.WriteLine(_text);
            //GameLog.Client.GameData.DebugFormat(_text);


            if ((listIndex < 0) || (listIndex > Count))
            {
                _text = "Step_4102: listIndex= " + listIndex + ", item= " + item.ToString();
                Console.WriteLine(_text);
                GameLog.Client.GameData.DebugFormat(_text);
                //throw new ArgumentOutOfRangeException(nameof(listIndex));

            }

            bool downgraded = false;
            SyncLock.EnterUpgradeableReadLock();
            try
            {
                if (Contains(item))
                {
                    return;
                }

                SyncLock.EnterWriteLock();

                try
                {
                    InternalCollection.Insert(listIndex, item);
                    OnItemAdded(item);
                }
                finally
                {
                    SyncLock.ExitWriteLock();
                    SyncLock.EnterReadLock();
                    SyncLock.ExitUpgradeableReadLock();
                    downgraded = true;
                }

                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, listIndex);
            }
            finally
            {
                if (downgraded)
                {
                    SyncLock.ExitReadLock();
                }
                else
                {
                    SyncLock.ExitUpgradeableReadLock();
                }
            }
        }

        private void OnItemAdded(T item)
        {
            foreach (Index index in _indexes.Values)
            {
                InsertIndexValue(index, item);
            }

            if (!IsChangeNotificationEnabled)
            {
                return;
            }

            if (item is INotifyPropertyChanged notifyPropertyChangedItem)
            {
                notifyPropertyChangedItem.PropertyChanged += ItemPropertyChangedCallback;
            }
        }

        public void AddMany(IEnumerable<T> items)
        {
            if (!items.Any())
            {
                return;
            }

            SyncLock.EnterWriteLock();

            try
            {
                foreach (T item in items)
                {
                    Add(item);
                }
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            bool downgraded = false;
            SyncLock.EnterUpgradeableReadLock();
            try
            {
                foreach (T item in InternalCollection)
                {
                    if (!IsChangeNotificationEnabled)
                    {
                        continue;
                    }

                    if (item is INotifyPropertyChanged notifyPropertyChangedItem)
                    {
                        notifyPropertyChangedItem.PropertyChanged -= ItemPropertyChangedCallback;
                    }
                }

                SyncLock.EnterWriteLock();

                try
                {
                    foreach (Index index in _indexes.Values)
                    {
                        index.LookupTable.Clear();
                    }

                    InternalCollection.Clear();
                }
                finally
                {
                    SyncLock.ExitWriteLock();
                    SyncLock.EnterReadLock();
                    SyncLock.EnterUpgradeableReadLock();
                    downgraded = true;
                }

                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionReset();
            }
            finally
            {
                if (downgraded)
                {
                    SyncLock.ExitReadLock();
                }
                else
                {
                    SyncLock.ExitUpgradeableReadLock();
                }
            }
        }

        public bool Contains(T value)
        {
            SyncLock.EnterReadLock();
            try
            {
                IEqualityComparer<T> comparer = Comparer;
                bool indexFound = false;
                foreach (Index index in _indexes.Values)
                {
                    object propertyValue = index.Accessor(value);
                    if (propertyValue != null)
                    {
                        int indexableHashCode = GetIndexableHashCode(propertyValue);

                        if (index.LookupTable.TryGetValue(indexableHashCode, out HashSet<T> indexItems) &&
                            indexItems.Contains(value, comparer))
                        {
                            return true;
                        }
                    }
                    indexFound = true;
                }
                return !indexFound && InternalCollection.Contains(value);
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        int IIndexedCollection<T>.IndexOf(T value)
        {
            return InternalCollection.IndexOf(value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            SyncLock.EnterReadLock();
            try
            {
                InternalCollection.CopyTo(array, arrayIndex);
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        public bool IsEmpty => Count == 0;

        public int Count
        {
            get
            {
                SyncLock.EnterReadLock();
                try
                {
                    return InternalCollection.Count;
                }
                finally
                {
                    SyncLock.ExitReadLock();
                }
            }
        }

        T IIndexedEnumerable<T>.this[int index] => InternalCollection[index];

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            bool downgraded = false;
            SyncLock.EnterUpgradeableReadLock();
            try
            {
                int itemIndex = InternalCollection.IndexOf(item);
                if (itemIndex >= 0)
                {
                    SyncLock.EnterWriteLock();
                    try
                    {
                        OnItemRemoved(item);
                        InternalCollection.RemoveAt(itemIndex);
                    }
                    finally
                    {
                        SyncLock.ExitWriteLock();
                        SyncLock.EnterReadLock();
                        SyncLock.ExitUpgradeableReadLock();
                        downgraded = true;
                    }

                    OnPropertyChanged("Count");
                    OnPropertyChanged("Item[]");
                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, itemIndex);

                    return true;
                }
            }
            finally
            {
                if (downgraded)
                {
                    SyncLock.ExitReadLock();
                }
                else
                {
                    SyncLock.ExitUpgradeableReadLock();
                }
            }
            return false;
        }

        private void OnItemRemoved(T item)
        {
            if (IsChangeNotificationEnabled && item is INotifyPropertyChanged notifyPropertyChangedItem)
            {
                notifyPropertyChangedItem.PropertyChanged -= ItemPropertyChangedCallback;
            }

            foreach (Index index in _indexes.Values)
            {
                object propertyValue = index.Accessor(item);
                if (propertyValue == null)
                {
                    continue;
                }

                if (index.LookupTable.ContainsKey(GetIndexableHashCode(propertyValue)))
                {
                    _ = index.LookupTable[GetIndexableHashCode(propertyValue)].Remove(item);
                }
            }
        }

        public int RemoveMany(IEnumerable<T> items)
        {
            if (!items.Any())
            {
                return 0;
            }

            int removedCount = 0;
            SyncLock.EnterWriteLock();

            try
            {
                foreach (T item in items)
                {
                    if (Remove(item))
                    {
                        ++removedCount;
                    }
                }
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }

            return removedCount;
        }
        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return new SafeEnumerator<T>(InternalCollection, SyncLock);
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IDeserializationCallback Members
        public virtual void OnDeserialization(object sender)
        {
            SyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            BuildIndexes();
        }
        #endregion

        #region INotifyCollectionChanged Members
        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler == null)
            {
                return;
            }

            SyncLock.EnterReadLock();

            try
            {
                handler(this, e);
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IList<T>
        int IList<T>.IndexOf(T item)
        {
            return InternalCollection.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            InsertItem(index, item, false);
        }
        void IList<T>.RemoveAt(int index) { }

        T IList<T>.this[int index]
        {
            get => InternalCollection[index];
            set => ReplaceItem(index, value, false);
        }

        protected void ReplaceItem(int listIndex, T newItem, bool upgradeableLockAlreadyHeld)
        {
            if (listIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(listIndex));
            }

            if (newItem == null)
            {
                throw new ArgumentNullException(nameof(newItem));
            }

            bool downgraded = false;
            SyncLock.EnterUpgradeableReadLock();

            try
            {
                if (listIndex >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(listIndex));
                }

                T oldItem = InternalCollection[listIndex];

                try
                {
                    OnItemRemoved(oldItem);
                    InternalCollection[listIndex] = newItem;
                    OnItemAdded(newItem);
                }
                finally
                {
                    SyncLock.ExitWriteLock();
                    SyncLock.EnterReadLock();
                    SyncLock.ExitUpgradeableReadLock();
                    downgraded = true;
                }

                OnCollectionChanged(
                    NotifyCollectionChangedAction.Replace,
                    oldItem,
                    newItem,
                    listIndex);
            }
            finally
            {
                if (downgraded)
                {
                    SyncLock.ExitReadLock();
                }
                else
                {
                    SyncLock.ExitUpgradeableReadLock();
                }
            }
        }
        #endregion
    }

    public static class IndexedCollectionExtention
    {
        public static IEnumerable<TResult> GroupJoin<T, TInner, TKey, TResult>(
            this IndexedCollection<T> outer,
            IndexedCollection<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Func<T, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (outer == null || inner == null || outerKeySelector == null || innerKeySelector == null || resultSelector == null)
            {
                throw new ArgumentNullException();
            }

            bool haveIndex = false;

            if (innerKeySelector.NodeType == ExpressionType.Lambda
                && innerKeySelector.Body.NodeType == ExpressionType.MemberAccess
                && outerKeySelector.NodeType == ExpressionType.Lambda
                && outerKeySelector.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression membExpInner = (MemberExpression)innerKeySelector.Body;
                MemberExpression membExpOuter = (MemberExpression)outerKeySelector.Body;
                Dictionary<int, HashSet<TInner>> innerIndex = new Dictionary<int, HashSet<TInner>>();
                Dictionary<int, HashSet<T>> outerIndex = new Dictionary<int, HashSet<T>>();

                if (inner.PropertyHasIndex(membExpInner.Member.Name)
                    && outer.PropertyHasIndex(membExpOuter.Member.Name))
                {
                    innerIndex = inner.GetIndexByProperty(membExpInner.Member.Name).LookupTable;
                    outerIndex = outer.GetIndexByProperty(membExpOuter.Member.Name).LookupTable;
                    haveIndex = true;
                }

                if (haveIndex)
                {
                    foreach (int outerKey in outerIndex.Keys)
                    {
                        HashSet<T> outerGroup = outerIndex[outerKey];
                        if (innerIndex.TryGetValue(outerKey, out HashSet<TInner> innerGroup))
                        {
                            //do a join on the GROUPS based on key result
                            IEnumerable<TInner> innerEnum = innerGroup.AsEnumerable();
                            IEnumerable<T> outerEnum = outerGroup.AsEnumerable();
                            foreach (TResult resultItem in outerEnum.GroupJoin(innerEnum, outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector))
                            {
                                yield return resultItem;
                            }
                        }
                    }
                }
            }
            if (!haveIndex)
            {
                //do normal group join
                IEnumerable<TInner> innerEnum = inner.AsEnumerable();
                IEnumerable<T> outerEnum = outer.AsEnumerable();
                foreach (TResult resultItem in outerEnum.GroupJoin(innerEnum, outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector, comparer))
                {
                    yield return resultItem;
                }
            }
        }

        public static IEnumerable<TResult> GroupJoin<T, TInner, TKey, TResult>(
            this IndexedCollection<T> outer,
            IndexedCollection<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Func<T, IEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TResult> Join<T, TInner, TKey, TResult>(
            this IndexedCollection<T> outer,
            IndexedCollection<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Func<T, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (outer == null || inner == null || outerKeySelector == null || innerKeySelector == null || resultSelector == null)
            {
                throw new ArgumentNullException();
            }

            bool haveIndex = false;
            if (innerKeySelector.NodeType == ExpressionType.Lambda
                && innerKeySelector.Body.NodeType == ExpressionType.MemberAccess
                && outerKeySelector.NodeType == ExpressionType.Lambda
                && outerKeySelector.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression membExpInner = (MemberExpression)innerKeySelector.Body;
                MemberExpression membExpOuter = (MemberExpression)outerKeySelector.Body;
                Dictionary<int, HashSet<TInner>> innerIndex = null;
                Dictionary<int, HashSet<T>> outerIndex = null;

                if (inner.PropertyHasIndex(membExpInner.Member.Name)
                    && outer.PropertyHasIndex(membExpOuter.Member.Name))
                {
                    innerIndex = inner.GetIndexByProperty(membExpInner.Member.Name).LookupTable;
                    outerIndex = outer.GetIndexByProperty(membExpOuter.Member.Name).LookupTable;
                    haveIndex = true;
                }

                if (haveIndex)
                {
                    foreach (int outerKey in outerIndex.Keys)
                    {
                        HashSet<T> outerGroup = outerIndex[outerKey];
                        if (innerIndex.TryGetValue(outerKey, out HashSet<TInner> innerGroup))
                        {
                            //do a join on the GROUPS based on key result
                            IEnumerable<TInner> innerEnum = innerGroup.AsEnumerable();
                            IEnumerable<T> outerEnum = outerGroup.AsEnumerable();
                            foreach (TResult resultItem in outerEnum.Join(innerEnum, outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector, comparer))
                            {
                                yield return resultItem;
                            }
                        }
                    }
                }
            }
            if (!haveIndex)
            {
                //this will happen if we don't have keys in the right places
                IEnumerable<TInner> innerEnum = inner.AsEnumerable();
                IEnumerable<T> outerEnum = outer.AsEnumerable();
                foreach (TResult resultItem in outerEnum.Join(innerEnum, outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector, comparer))
                {
                    yield return resultItem;
                }
            }
        }

        public static IEnumerable<TResult> Join<T, TInner, TKey, TResult>(
            this IndexedCollection<T> outer,
            IndexedCollection<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Func<T, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static bool HasIndexablePropertyOnLeft<T>(E leftSide, IndexedCollection<T> sourceCollection)
#pragma warning restore IDE0051 // Remove unused private members
        {
            return leftSide.NodeType == ExpressionType.MemberAccess
                    && sourceCollection.PropertyHasIndex(((MemberExpression)leftSide).Member.Name);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static int? GetHashRight<T>(IndexedCollection<T> sourceCollection, E leftSide, E rightSide)
#pragma warning restore IDE0051 // Remove unused private members
        {
            //rightside is where we get our hash...
            switch (rightSide.NodeType)
            {
                //shortcut constants, dont eval, will be faster
                case ExpressionType.Constant:
                    ConstantExpression constExp
                        = (ConstantExpression)rightSide;
                    return sourceCollection.GetIndexableHashCode(constExp.Value);
                //case ExpressionType.MemberAccess:
                //    return null; //member expressions cant eval

                //if not constant (which is provably terminal in a tree), convert back to Lambda and eval to get the hash.
                default:
                    //Lambdas can be created from expressions... yay
                    LambdaExpression evalRight = E.Lambda(rightSide, null);
                    //Compile that mutherf-ker, invoke it, and get the resulting hash
                    return sourceCollection.GetIndexableHashCode(evalRight.Compile().DynamicInvoke(null));
            }
        }

        //extend the where when we are working with indexable collections! 
        public static IEnumerable<TSource> Where<TSource>(
            this IndexedCollection<TSource> sourceCollection,
            Expression<Func<TSource, bool>> expr)
        {
            //our indexes work from the hash values of that which is indexed, regardless of type
            //bool noIndex = true;
            const bool done = false;
            //IEnumerable<TSource> results;
            Expression<Func<TSource, bool>> innerExpr = expr;

            //if (innerExpr.Body.NodeType == ExpressionType.Lambda)
            //{
            //    innerExpr = (Expression<Func<TSource, bool>>)innerExpr.Body;
            //}
            //else if (innerExpr.Body.NodeType == ExpressionType.Invoke)
            //{
            //    innerExpr = (Expression<Func<TSource, bool>>)((InvocationExpression)innerExpr.Body).Expression;
            //}

            //switch (innerExpr.Body.NodeType)
            //{
            //    case ExpressionType.NotEqual:
            //        UnaryExpression unExp = (UnaryExpression)innerExpr.Body;
            //        results = sourceCollection.Except(sourceCollection.Where(
            //            Expression.Lambda<Func<TSource, bool>>(unExp.Operand, expr.Parameters)));
            //        foreach (var result in results)
            //            yield return result;
            //        done = true;
            //        break;

            //    case ExpressionType.OrElse:
            //    case ExpressionType.AndAlso:
            //        bool isUnion = (innerExpr.Body.NodeType == ExpressionType.OrElse);
            //        BinaryExpression binExp = (BinaryExpression)innerExpr.Body;
            //        Expression<Func<TSource, bool>> binLeft = Expression.Lambda<Func<TSource, bool>>(
            //            binExp.Left,
            //            expr.Parameters);
            //        Expression<Func<TSource, bool>> binRight = Expression.Lambda<Func<TSource, bool>>(
            //            binExp.Right,
            //            expr.Parameters);
            //        results = isUnion
            //            ? sourceCollection.Where(binLeft).Union(sourceCollection.Where(binRight))
            //            : sourceCollection.Where(binLeft).Intersect(sourceCollection.Where(binRight));
            //        foreach (var result in results)
            //            yield return result;
            //        done = true;
            //        break;
            //}

            if (!done)
            {
                //indexes only work on equality expressions here
                //if (expr.Body.NodeType == ExpressionType.Equal)
                //{
                //    //Equality is a binary expression
                //    BinaryExpression binExp = (BinaryExpression)expr.Body;
                //    //Get some aliases for either side
                //    E leftSide = binExp.Left;
                //    E rightSide = binExp.Right;

                //    int? hashRight = GetHashRight(sourceCollection, leftSide, rightSide);

                //    //if we were able to create a hash from the right side (likely)
                //    if (hashRight.HasValue && HasIndexablePropertyOnLeft(leftSide, sourceCollection))
                //    {
                //        //cast to MemberExpression - it allows us to get the property
                //        MemberExpression propExp = (MemberExpression)leftSide;
                //        string property = propExp.Member.Name;
                //        Dictionary<int, HashSet<TSource>> myIndex =
                //                sourceCollection.GetIndexByProperty(property).LookupTable;
                //        //try
                //        //{
                //        if (myIndex.ContainsKey(hashRight.Value))
                //        {
                //            IEnumerable<TSource> sourceEnum = myIndex[hashRight.Value].AsEnumerable();

                //            if (sourceEnum != null && sourceEnum.Count() != 0)
                //            {

                //                foreach (TSource item in sourceEnum.Where(expr.Compile()))
                //                {
                //                    yield return item;
                //                }
                //            }
                //        }
                //        //} catch
                //        //{

                //        //}

                //        noIndex = false; //we found an index, whether it had values or not is another matter
                //    }
                //}
                //if (noIndex) //no index?  just do it the normal slow way then...
                //{
                    IEnumerable<TSource> sourceEnum = sourceCollection.AsEnumerable();
                    foreach (TSource resultItem in sourceEnum.Where(expr.Compile()))
                    {
                        yield return resultItem;
                    }
                //}
            }
        }

        public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            return source.ToLookup<TSource, TSource, TKey, TElement>(
                keySelector,
                elementSelector,
                comparer);
        }

        public static Lookup<TKey, TElement> ToLookup<TSource, TSourceConstraint, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSourceConstraint, TElement> elementSelector)
            where TSourceConstraint : TSource
        {
            return ToLookup(
                source,
                keySelector,
                elementSelector,
                EqualityComparer<TKey>.Default);
        }

        public static Lookup<TKey, TElement> ToLookup<TSource, TSourceConstraint, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSourceConstraint, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
            where TSourceConstraint : TSource
        {
            if (source == null || keySelector == null || elementSelector == null) //comparer may be null
            {
                throw new ArgumentNullException();
            }

            IndexedCollection<TSource>.Index index = null;
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            Func<TSource, TKey> keySelectorCompiled = keySelector.Compile();

            if (keySelector.Body is MemberExpression propExpr)
            {
                string property = propExpr.Member.Name;
                index = source.GetIndexByProperty(property);
            }

            if (index != null)
            {
                foreach (int key in index.LookupTable.Keys)
                {
                    Grouping<TKey, TElement> group = null;
                    foreach (TSource item in index.LookupTable[key])
                    {
                        if (item is TSourceConstraint constraint)
                        {
                            if (group == null)
                            {
                                group = new Grouping<TKey, TElement>(keySelectorCompiled(item));
                            }

                            group.Add(elementSelector(constraint));
                        }
                    }
                    if (group != null)
                    {
                        lookup.Add(group);
                    }
                }
            }
            else
            {
                foreach (TSource item in source)
                {
                    if (item is TSourceConstraint constraint)
                    {
                        Grouping<TKey, TElement> group;
                        TKey key = keySelectorCompiled(item);
                        if (lookup.Contains(key))
                        {
                            group = (Grouping<TKey, TElement>)lookup.Dictionary[key];
                        }
                        else
                        {
                            group = new Grouping<TKey, TElement>(key);
                            lookup.Add(group);
                        }
                        group.Add(elementSelector(constraint));
                    }
                }
            }

            return lookup;
        }

        public static Lookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            return ToLookup(source, keySelector, t => t, EqualityComparer<TKey>.Default);
        }

        public static Lookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return ToLookup(source, keySelector, t => t, comparer);
        }

        public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            return GroupBy(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return GroupBy(source, keySelector, (TSource t) => t, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return GroupBy(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null || keySelector == null || elementSelector == null) //comparer may be null
            {
                throw new ArgumentNullException();
            }

            Lookup<TKey, TElement> lookup = ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
            foreach (TKey key in lookup.Keys)
            {
                yield return lookup.Dictionary[key];
            }
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null || keySelector == null || resultSelector == null) //comparer may be null
            {
                throw new ArgumentNullException();
            }

            Lookup<TKey, TSource> lookup = ToLookup(source, keySelector, comparer);
            foreach (TKey key in lookup.Keys)
            {
                yield return resultSelector(key, lookup[key]);
            }
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, elementSelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IndexedCollection<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null || keySelector == null || elementSelector == null || resultSelector == null) //comparer may be null
            {
                throw new ArgumentNullException();
            }

            Lookup<TKey, TElement> lookup = ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
            foreach (TKey key in lookup.Keys)
            {
                yield return resultSelector(key, lookup[key]);
            }
        }
    }

    public class Grouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
    {
        internal Grouping(TKey key)
        {
            Key = key;
        }

        public TKey Key { get; }
    }

    public class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        internal Dictionary<TKey, IGrouping<TKey, TElement>> Dictionary;
        internal List<TKey> Keys;

        internal Lookup(IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, IGrouping<TKey, TElement>>(comparer);
            Keys = new List<TKey>();
        }

        public int Count => Dictionary.Count;

        public IEnumerable<TElement> this[TKey key] => Dictionary.ContainsKey(key) ? Dictionary[key] : Enumerable.Empty<TElement>();

        public bool Contains(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (TKey key in Keys)
            {
                yield return Dictionary[key];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Add(IGrouping<TKey, TElement> item)
        {
            TKey key = item.Key;
            Keys.Add(key);
            Dictionary.Add(key, item);
        }
    }
}
