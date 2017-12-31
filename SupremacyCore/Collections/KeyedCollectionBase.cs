// KeyedCollectionBase.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Threading;

namespace Supremacy.Collections
{
    /// <summary>
    /// Provides the abstract base class for a collection whose keys are embedded in the values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of items in the collection.</typeparam>
    [Serializable]
    public class KeyedCollectionBase<TKey, TValue> : KeyedCollection<TKey, TValue>, INotifyCollectionChanged, IDeserializationCallback
    {
        [NonSerialized]
        protected ReaderWriterLockSlim SyncLock;
        private readonly Func<TValue, TKey> _keyRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedCollectionBase&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="keyRetriever">The key retriever.</param>
        public KeyedCollectionBase(Func<TValue, TKey> keyRetriever) : base()
        {
            if (keyRetriever == null)
                throw new ArgumentNullException("keyRetriever");
            _keyRetriever = keyRetriever;
            SyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        /// <summary>
        /// Adds the specified items.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<TValue> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            foreach (TValue item in items)
                Add(item);
        }

        /// <summary>
        /// Removes the specified items.
        /// </summary>
        /// <param name="items">The items to remove.</param>
        public void RemoveRange(IEnumerable<TValue> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            foreach (TValue item in items)
            {
                if (Count == 0)
                    break;
                Remove(item);
            }
        }

        /// <summary>
        /// Removes the items corresponding to the specified keys.
        /// </summary>
        /// <param name="keys">The keys of the items to remove.</param>
        public void RemoveRange(IEnumerable<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            foreach (TKey key in keys)
            {
                if (Count == 0)
                    break;
                Remove(key);
            }
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TValue"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The <typeparamref name="TValue"/> with the specified key.</param>
        /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (base.Dictionary != null)
                return base.Dictionary.TryGetValue(key, out value);
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override TKey GetKeyForItem(TValue item)
        {
            return _keyRetriever(item);
        }

        /// <summary>
        /// Trims the excess slots in the internal list.
        /// </summary>
        public void TrimExcess()
        {
            List<TValue> items = base.Items as List<TValue>;
            if (items != null)
            {
                SyncLock.EnterWriteLock();
                try
                {
                    items.TrimExcess();
                }
                finally
                {
                    SyncLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="TValue"/> with the specified key.</value>
        public new TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                    return value;
                if (typeof(TValue).IsValueType)
                    throw new KeyNotFoundException("No item exists with specified key: " + key);
                return default(TValue);
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Collections.Generic.IList`1"/> wrapper around the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Collections.Generic.IList`1"/> wrapper around the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.</returns>
        public new IList<TValue> Items
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.
        /// </exception>
        protected override void InsertItem(int index, TValue item)
        {
            SyncLock.EnterWriteLock();
            try
            {
                base.InsertItem(index, item);
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            TValue item;
            SyncLock.EnterWriteLock();
            try
            {
                item =Items[index];
                base.RemoveItem(index);
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
        /// </summary>
        protected override void ClearItems()
        {
            SyncLock.EnterWriteLock();
            try
            {
                base.ClearItems();
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
            OnCollectionReset();
        }

        /// <summary>
        /// Replaces the item at the specified index with the specified item.
        /// </summary>
        /// <param name="index">The zero-based index of the item to be replaced.</param>
        /// <param name="item">The new item.</param>
        protected override void SetItem(int index, TValue item)
        {
            SyncLock.EnterWriteLock();
            try
            {
                base.SetItem(index, item);
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        #region INotifyCollectionChanged Members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        [field:NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                SyncLock.EnterReadLock();
                try
                {
                    CollectionChanged(this, e);
                }
                finally
                {
                    SyncLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index));
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <param name="oldIndex">The old index.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
                                           object item,
                                           int index,
                                           int oldIndex)
        {
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index,
                    oldIndex));
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="oldItem">The old item.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="index">The index.</param>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action,
                                           object oldItem,
                                           object newItem,
                                           int index)
        {
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    action,
                    newItem,
                    oldItem,
                    index));
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event when the collection is reset.
        /// </summary>
        protected void OnCollectionReset()
        {
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        #region IDeserializationCallback Members
        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            if (SyncLock == null)
                SyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }
        #endregion
    }
}