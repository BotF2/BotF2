using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Supremacy.Collections
{
    [ContractClass(typeof(KeyedLookupContracts<,>))]
    public interface IKeyedLookup<in TKey, out TValue> : IEnumerable<TValue>
    {
        [Pure]
        TValue this[TKey key] { get; }
        [Pure]
        bool Contains(TKey key);
        [Pure]
        IEqualityComparer<TKey> KeyComparer { get; }
    }

    [ContractClassFor(typeof(IKeyedLookup<,>))]
    internal sealed class KeyedLookupContracts<TKey, TValue> : IKeyedLookup<TKey, TValue>
    {
        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(Contract.ForAll(this, o => !ReferenceEquals(o, null)));
        }

        #region Implementation of IKeyedCollection<TKey,TValue>
        bool IKeyedLookup<TKey, TValue>.Contains(TKey key)
        {
            return false;
        }

        TValue IKeyedLookup<TKey, TValue>.this[TKey key]
        {
            get
            {
                var @this = (IKeyedLookup<TKey, TValue>)this;
                Contract.Ensures(@this.Contains(key));
                Contract.EnsuresOnThrow<KeyNotFoundException>(!@this.Contains(key));
                return default(TValue);
            }
        }

        IEqualityComparer<TKey> IKeyedLookup<TKey, TValue>.KeyComparer
        {
            get
            {
                Contract.Ensures(Contract.Result<IEqualityComparer<TKey>>() != null);
                return EqualityComparer<TKey>.Default;
            }
        }
        #endregion

        #region Implementation of IEnumerable
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>)this).GetEnumerator();
        }
        #endregion
    }

    [ContractClass(typeof(IndexedEnumerableContracts<>))]
    public interface IIndexedEnumerable<out T> : IEnumerable<T>
    {
        [Pure]
        int Count { get; }
        [Pure]
        T this[int index] { get; }
    }

    [ContractClassFor(typeof(IIndexedEnumerable<>))]
    internal sealed class IndexedEnumerableContracts<T> : IIndexedEnumerable<T>
    {
        [ContractInvariantMethod]
        private void Invariants()
        {
            var @this = (IIndexedEnumerable<T>)this;
            Contract.Invariant(@this.Count >= 0);
        }

        T IIndexedEnumerable<T>.this[int index]
        {
            get
            {
                var @this = (IIndexedEnumerable<T>)this;
                Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
                Contract.Requires/*<ArgumentOutOfRangeException>*/(index < @this.Count);
                return default(T);
            }
        }

        int IIndexedEnumerable<T>.Count
        {
            get { return 0; }
        }

        #region Implementation of IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    [ContractClass(typeof(IndexedCollectionContracts<>))]
    public interface IIndexedCollection<T> : IIndexedEnumerable<T>
    {
        [Pure]
        bool Contains(T value);
        [Pure]
        int IndexOf(T value);
    }

    public interface IObservableIndexedCollection<T> 
        : IIndexedCollection<T>, INotifyCollectionChanged { }

    [ContractClassFor(typeof(IIndexedCollection<>))]
    internal sealed class IndexedCollectionContracts<T> : IIndexedCollection<T>
    {
        public bool Contains(T value)
        {
            var @this = (IIndexedCollection<T>)this;
            Contract.Ensures(!Contract.Result<bool>() || Contract.Exists(@this, o => Equals(o, value)));
            return false;
        }

        int IIndexedCollection<T>.IndexOf(T value)
        {
            var @this = (IIndexedCollection<T>)this;
            Contract.Ensures(Contract.Result<int>() >= -1 && Contract.Result<int>() < @this.Count);
            Contract.Ensures(Contract.Result<int>() == -1 || Contract.Exists(this, o => ReferenceEquals(o, value)));
            return -1;
        }

        #region Implementation of IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of IIndexedEnumerable<T>
        public int Count
        {
            get { return 0; }
        }

        public T this[int index]
        {
            get { return default(T); }
        }
        #endregion
    }

    [ContractClass(typeof(KeyedCollectionContracts<,>))]
    public interface IKeyedCollection<TKey, TValue> : IKeyedLookup<TKey, TValue>
    {
        [Pure]
        IEnumerable<TKey> Keys { get; }
        [Pure]
        bool TryGetValue(TKey key, out TValue value);
    }

    [ContractClassFor(typeof(IKeyedCollection<,>))]
    internal sealed class KeyedCollectionContracts<TKey, TValue> : IKeyedCollection<TKey, TValue>
    {
        #region Implementation of IKeyedCollection<TKey,TValue>
        IEnumerable<TKey> IKeyedCollection<TKey, TValue>.Keys
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<TKey>>() != null);
                yield break;
            }
        }

        bool IKeyedCollection<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            var @this = (IKeyedCollection<TKey, TValue>)this;
            Contract.Ensures(!Contract.Result<bool>() || @this.Contains(key));
            value = default(TValue);
            return false;
        }
        #endregion

        #region Implementation of IEnumerable
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>)this).GetEnumerator();
        }
        #endregion

        #region Implementation of IKeyedEnumerable<TKey,TValue>
        public TValue this[TKey key]
        {
            get { return default(TValue); }
        }

        public bool Contains(TKey key)
        {
            return false;
        }

        public TKey GetKeyForItem(TValue item)
        {
            return default(TKey);
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return null; }
        }
        #endregion
    }

    [ContractClass(typeof(IndexedKeyedCollectionContracts<,>))]
    public interface IIndexedKeyedCollection<TKey, TValue>
        : IKeyedCollection<TKey, TValue>,
          IObservableIndexedCollection<TValue> { }

    [ContractClassFor(typeof(IIndexedKeyedCollection<,>))]
    internal sealed class IndexedKeyedCollectionContracts<TKey, TValue> : IIndexedKeyedCollection<TKey, TValue>
    {
        #region Implementation of IEnumerable
        public IEnumerator<TValue> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of IKeyedEnumerable<TKey,TValue>
        public IEnumerable<TKey> Keys
        {
            get { yield break; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        TValue IKeyedLookup<TKey, TValue>.this[TKey key]
        {
            get { return default(TValue); }
        }

        public bool Contains(TKey key)
        {
            return false;
        }

        public TKey GetKeyForItem(TValue item)
        {
            return default(TKey);
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return EqualityComparer<TKey>.Default; }
        }
        #endregion

        #region Implementation of IIndexedEnumerable<TValue>
        public int Count
        {
            get { return 0; }
        }

        TValue IIndexedEnumerable<TValue>.this[int index]
        {
            get { return default(TValue); }
        }
        #endregion

        #region Implementation of IIndexedCollection<TValue>
        public bool Contains(TValue value)
        {
            return false;
        }

        public int IndexOf(TValue value)
        {
            return 0;
        }
        #endregion

        #region Implementation of INotifyCollectionChanged
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { }
            remove { }
        }
        #endregion
    }
}