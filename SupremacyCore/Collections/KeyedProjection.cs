using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Utility;

namespace Supremacy.Collections
{
    [Serializable]
    public class KeyedProjection<TKey, TInnerValue, TValue> : IIndexedKeyedCollection<TKey, TValue>
    {
        private readonly DelegatingWeakCollectionChangedListener _innerCollectionChangedListener;
        private readonly IIndexedKeyedCollection<TKey, TInnerValue> _innerCollection;
        private readonly Func<TInnerValue, TValue> _valueSelector;

        public KeyedProjection([NotNull] IIndexedKeyedCollection<TKey, TInnerValue> innerCollection, [NotNull] Func<TInnerValue, TValue> valueSelector)
        {
            if (innerCollection == null)
                throw new ArgumentNullException("innerCollection");
            if (valueSelector == null)
                throw new ArgumentNullException("valueSelector");

            _innerCollection = innerCollection;
            _valueSelector = valueSelector;
            _innerCollectionChangedListener = new DelegatingWeakCollectionChangedListener((sender, args) => CollectionChanged.Raise(this, args));

            CollectionChangedEventManager.AddListener(innerCollection, _innerCollectionChangedListener);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _innerCollection.Select(_valueSelector).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get { return _valueSelector(_innerCollection[key]); }
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
            TInnerValue innerValue;
            
            if (_innerCollection.TryGetValue(key, out innerValue))
            {
                value = _valueSelector(innerValue);
                return true;
            }

            value = default(TValue);
            return false;
        }

        public int Count
        {
            get { return _innerCollection.Count; }
        }

        public TValue this[int index]
        {
            get { return _valueSelector(_innerCollection[index]); }
        }

        public bool Contains(TValue value)
        {
            return _innerCollection.Any(o => Equals(_valueSelector(o), value));
        }

        public int IndexOf(TValue value)
        {
            return _innerCollection.FirstIndexWhere(o => Equals(_valueSelector(o), value));
        }

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}