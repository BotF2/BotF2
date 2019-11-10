using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Supremacy.Collections
{
    // Adds strong typing to WeakReference.Target using generics. Also,
    // the Create factory method is used in place of a constructor
    // to handle the case where target is null, but we want the 
    // reference to still appear to be alive.

    // Provides a weak reference to a null target object, which, unlike
    // other weak references, is always considered to be alive. This 
    // facilitates handling null dictionary values, which are perfectly
    // legal.
    public class WeakNullReference<T> : WeakReference<T> where T : class
    {
        public static readonly WeakNullReference<T> Singleton = new WeakNullReference<T>();

        private WeakNullReference() : base(null) { }

        public override bool IsAlive
        {
            get { return true; }
        }
    }

    // Provides a weak reference to an object of the given type to be used in
    // a WeakDictionary along with the given comparer.
    internal sealed class WeakKeyReference<T> : WeakReference<T> where T : class
    {
        public readonly int HashCode;

        public WeakKeyReference(T key, IEqualityComparer<object> comparer)
            : base(key)
        {
            // retain the object's hash code immediately so that even
            // if the target is GC'ed we will be able to find and
            // remove the dead weak reference.
            HashCode = comparer.GetHashCode(key);
        }
    }

    // Compares objects of the given type or WeakKeyReferences to them
    // for equality based on the given comparer. Note that we can only
    // implement IEqualityComparer<T> for T = object as there is no 
    // other common base between T and WeakKeyReference<T>. We need a
    // single comparer to handle both types because we don't want to
    // allocate a new weak reference for every lookup.
    internal sealed class WeakKeyComparer<T> : IEqualityComparer<object>
        where T : class
    {

        private readonly IEqualityComparer<T> _comparer;

        internal WeakKeyComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            _comparer = comparer;
        }

        public int GetHashCode(object obj)
        {
            var weakKey = obj as WeakKeyReference<T>;
            if (weakKey != null) return weakKey.HashCode;
            return _comparer.GetHashCode((T)obj);
        }

        // Note: There are actually 9 cases to handle here.
        //
        //  Let Wa = Alive Weak Reference
        //  Let Wd = Dead Weak Reference
        //  Let S  = Strong Reference
        //  
        //  x  | y  | Equals(x,y)
        // -------------------------------------------------
        //  Wa | Wa | comparer.Equals(x.Target, y.Target) 
        //  Wa | Wd | false
        //  Wa | S  | comparer.Equals(x.Target, y)
        //  Wd | Wa | false
        //  Wd | Wd | x == y
        //  Wd | S  | false
        //  S  | Wa | comparer.Equals(x, y.Target)
        //  S  | Wd | false
        //  S  | S  | comparer.Equals(x, y)
        // -------------------------------------------------
        public new bool Equals(object x, object y)
        {
            bool xIsDead, yIsDead;
            
            var first = GetTarget(x, out xIsDead);
            var second = GetTarget(y, out yIsDead);

            if (xIsDead)
                return yIsDead && x == y;

            if (yIsDead)
                return false;

            return _comparer.Equals(first, second);
        }

        private static T GetTarget(object obj, out bool isDead)
        {
            var wref = obj as WeakKeyReference<T>;
            T target;
            if (wref != null)
            {
                target = wref.Target;
                isDead = !wref.IsAlive;
            }
            else
            {
                target = (T)obj;
                isDead = false;
            }
            return target;
        }
    }

    /// <summary>
    /// Represents a dictionary mapping keys to values.
    /// </summary>
    /// 
    /// <remarks>
    /// Provides the plumbing for the portions of IDictionary<TKey,
    /// TValue> which can reasonably be implemented without any
    /// dependency on the underlying representation of the dictionary.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(Prefix + "DictionaryDebugView`2" + Suffix)]
    public abstract class BaseDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private const string Prefix = "System.Collections.Generic.Mscorlib_";
        private const string Suffix = ",mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089";

        private KeyCollection _keys;
        private ValueCollection _values;

        public abstract int Count { get; }
        public abstract void Clear();
        public abstract void Add(TKey key, TValue value);
        public abstract bool ContainsKey(TKey key);
        public abstract bool Remove(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        protected abstract void SetValue(TKey key, TValue value);

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_keys == null)
                    _keys = new KeyCollection(this);

                return _keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_values == null)
                    _values = new ValueCollection(this);

                return _values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException();

                return value;
            }
            set
            {
                SetValue(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (!TryGetValue(item.Key, out value))
                return false;

            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Copy(this, array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;

            return Remove(item.Key);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private abstract class Collection<T> : ICollection<T>
        {
            protected readonly IDictionary<TKey, TValue> Dictionary;

            protected Collection(IDictionary<TKey, TValue> dictionary)
            {
                Dictionary = dictionary;
            }

            public int Count
            {
                get { return Dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Copy(this, array, arrayIndex);
            }

            public virtual bool Contains(T item)
            {
                foreach (var element in this)
                    if (EqualityComparer<T>.Default.Equals(element, item))
                        return true;
                return false;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var pair in Dictionary)
                    yield return GetItem(pair);
            }

            protected abstract T GetItem(KeyValuePair<TKey, TValue> pair);

            public bool Remove(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Add(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(Prefix + "DictionaryKeyCollectionDebugView`2" + Suffix)]
        private class KeyCollection : Collection<TKey>
        {
            public KeyCollection(IDictionary<TKey, TValue> dictionary)
                : base(dictionary) { }

            protected override TKey GetItem(KeyValuePair<TKey, TValue> pair)
            {
                return pair.Key;
            }
            public override bool Contains(TKey item)
            {
                return Dictionary.ContainsKey(item);
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(Prefix + "DictionaryValueCollectionDebugView`2" + Suffix)]
        private class ValueCollection : Collection<TValue>
        {
            public ValueCollection(IDictionary<TKey, TValue> dictionary)
                : base(dictionary) { }

            protected override TValue GetItem(KeyValuePair<TKey, TValue> pair)
            {
                return pair.Value;
            }
        }

        private static void Copy<T>(ICollection<T> source, T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if ((array.Length - arrayIndex) < source.Count)
                throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex.");

            foreach (var item in source)
                array[arrayIndex++] = item;
        }
    }

    /// <summary>
    /// A generic dictionary, which allows both its keys and values 
    /// to be garbage collected if there are no other references
    /// to them than from the dictionary itself.
    /// </summary>
    /// 
    /// <remarks>
    /// If either the key or value of a particular entry in the dictionary
    /// has been collected, then both the key and value become effectively
    /// unreachable. However, left-over WeakReference objects for the key
    /// and value will physically remain in the dictionary until
    /// RemoveCollectedEntries is called. This will lead to a discrepancy
    /// between the Count property and the number of iterations required
    /// to visit all of the elements of the dictionary using its
    /// enumerator or those of the Keys and Values collections. Similarly,
    /// CopyTo will copy fewer than Count elements in this situation.
    /// </remarks>
    public sealed class WeakDictionary<TKey, TValue> : BaseDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {

        private readonly Dictionary<object, WeakReference<TValue>> _dictionary;
        private readonly WeakKeyComparer<TKey> _comparer;

        public WeakDictionary()
            : this(0, null) { }

        public WeakDictionary(int capacity)
            : this(capacity, null) { }

        public WeakDictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer) { }

        public WeakDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _comparer = new WeakKeyComparer<TKey>(comparer);
            _dictionary = new Dictionary<object, WeakReference<TValue>>(capacity, _comparer);
        }

        // WARNING: The count returned here may include entries for which
        // either the key or value objects have already been garbage
        // collected. Call RemoveCollectedEntries to weed out collected
        // entries and update the count accordingly.
        public override int Count
        {
            get { return _dictionary.Count; }
        }

        public override void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            var weakKey = new WeakKeyReference<TKey>(key, _comparer);
            var weakValue = WeakReference<TValue>.Create(value);
            _dictionary.Add(weakKey, weakValue);
        }

        public override bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public override bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            WeakReference<TValue> weakValue;
            if (_dictionary.TryGetValue(key, out weakValue))
            {
                value = weakValue.Target;
                return weakValue.IsAlive;
            }
            value = null;
            return false;
        }

        protected override void SetValue(TKey key, TValue value)
        {
            var weakKey = new WeakKeyReference<TKey>(key, _comparer);
            _dictionary[weakKey] = WeakReference<TValue>.Create(value);
        }

        public override void Clear()
        {
            _dictionary.Clear();
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _dictionary)
            {
                var weakKey = (WeakReference<TKey>)(kvp.Key);
                var weakValue = kvp.Value;
                var key = weakKey.Target;
                var value = weakValue.Target;
                if (weakKey.IsAlive && weakValue.IsAlive)
                    yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        // Removes the left-over weak references for entries in the dictionary
        // whose key or value has already been reclaimed by the garbage
        // collector. This will reduce the dictionary's Count by the number
        // of dead key-value pairs that were eliminated.
        public void RemoveCollectedEntries()
        {
            List<object> toRemove = null;
            foreach (var pair in _dictionary)
            {
                var weakKey = (WeakReference<TKey>)(pair.Key);
                var weakValue = pair.Value;

                if (weakKey.IsAlive && weakValue.IsAlive)
                    continue;

                if (toRemove == null)
                    toRemove = new List<object>();

                toRemove.Add(weakKey);
            }

            if (toRemove == null)
                return;

            foreach (var key in toRemove)
                _dictionary.Remove(key);
        }
    }
}