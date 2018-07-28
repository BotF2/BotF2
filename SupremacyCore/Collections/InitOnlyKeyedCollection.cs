using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Supremacy.Annotations;
using Supremacy.Types;

namespace Supremacy.Collections
{
    [Serializable]
    public abstract class InitOnlyKeyedCollection<TKey, TValue> : SupportInitializeBase, IIndexedKeyedCollection<TKey, TValue>, IList
    {
        private readonly KeyedCollectionBase<TKey, TValue> _innerCollection;

        protected InitOnlyKeyedCollection([NotNull] Func<TValue, TKey> keyRetriever)
        {
            if (keyRetriever == null)
                throw new ArgumentNullException("keyRetriever");

            _innerCollection = new KeyedCollectionBase<TKey, TValue>(keyRetriever);
        }

        public void Add(TValue item)
        {
            VerifyInitializing();
            _innerCollection.Add(item);
        }

        int IList.Add(object value)
        {
            VerifyInitializing();
            VerifyCompatibleValue(value);
            _innerCollection.Add((TValue)value);
            return _innerCollection.Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (!(value is TValue))
                return false;
            return Contains((TValue)value);
        }
        public void Clear()
        {
            VerifyInitializing();
            _innerCollection.Clear();
        }

        int IList.IndexOf(object value)
        {
            if (!(value is TValue))
                return -1;
            return _innerCollection.IndexOf((TValue)value);
        }

        void IList.Insert(int index, object value)
        {
            VerifyInitializing();
            VerifyCompatibleValue(value);

            _innerCollection.Insert(index, (TValue)value);
        }

        void IList.Remove(object value)
        {
            VerifyInitializing();

            if (value is TValue)
                Remove((TValue)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return !IsInitializing; }
        }

        public void Insert(int index, TValue item)
        {
            VerifyInitializing();
            _innerCollection.Insert(index, item);
        }

        public bool Remove(TValue item)
        {
            VerifyInitializing();
            return _innerCollection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            VerifyInitializing();
            _innerCollection.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                VerifyInitializing();
                VerifyCompatibleValue(value);
                ((IList<TValue>)_innerCollection)[index] = (TValue)value;
            }
        }

        public void RemoveRange(IEnumerable<TValue> items)
        {
            VerifyInitializing();
            _innerCollection.RemoveRange(items);
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            VerifyInitializing();
            _innerCollection.AddRange(items);
        }

        public int RemoveRange(IEnumerable<TKey> keys)
        {
            VerifyInitializing();
            return _innerCollection.RemoveRange(keys);
        }

        public bool Remove(TKey key)
        {
            VerifyInitializing();
            return _innerCollection.Remove(key);
        }

        public bool Replace(TKey key, TValue newValue)
        {
            VerifyInitializing();
            return _innerCollection.Replace(key, newValue);
        }

        public bool Replace(TValue oldValue, TValue newValue)
        {
            VerifyInitializing();
            return _innerCollection.Replace(oldValue, newValue);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get { return _innerCollection[key]; }
        }

        public bool Contains(TKey key)
        {
            return _innerCollection.Contains(key);
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _innerCollection.KeyComparer; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _innerCollection.Keys; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _innerCollection.TryGetValue(key, out value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_innerCollection).CopyTo(array, index);
        }

        public int Count
        {
            get { return _innerCollection.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_innerCollection).IsSynchronized; }
        }

        public TValue this[int index]
        {
            get { return _innerCollection[index]; }
        }

        public bool Contains(TValue value)
        {
            return _innerCollection.Contains(value);
        }

        public int IndexOf(TValue value)
        {
            return _innerCollection.IndexOf(value);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _innerCollection.CollectionChanged += value; }
            remove { _innerCollection.CollectionChanged -= value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static void VerifyCompatibleValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (!(value is TValue))
            {
                throw new ArgumentException(
                    string.Format(
                        "Item of type '{0}' is not valid for a collection with element type '{1}'.",
                        value.GetType().Name,
                        typeof(TValue).Name));
            }
        }
    }
}