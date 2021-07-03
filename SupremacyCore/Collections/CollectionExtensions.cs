using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Supremacy.Annotations;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Collections
{
    // ReSharper disable ConditionIsAlwaysTrueOrFalse
    // ReSharper disable ParameterTypeCanBeEnumerable.Global
    // ReSharper disable HeuristicUnreachableCode
    // ReSharper disable CompareNonConstrainedGenericWithNull
    public static class CollectionExtensions
    {
        #region Sorted List Extensions

        /// <summary>
        /// Adds an item to the sorted list.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to add.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        public static void AddSorted<TValue, TKey>([NotNull] this IList<TValue> list, [NotNull] TValue item, [NotNull] Func<TValue, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            _ = LocateSorted(list, keySelector(item), keySelector, out int index);

            list.Insert(index, item);
        }

        /// <summary>
        /// Locates the specified item in the sorted list or determines where it belongs.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to search.</param>
        /// <param name="key">Key of item to locate.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        /// <param name="index">Index of the item or where it belongs.</param>
        /// <returns>True if present; false otherwise.</returns>
        private static bool LocateSorted<TValue, TKey>([NotNull] IList<TValue> list, [NotNull] TKey key, [NotNull] Func<TValue, TKey> keySelector, out int index) where TKey : IComparable<TKey>
        {
            int l = -1;
            int r = list.Count;

            while (true)
            {
                int p = (r - l) / 2;
                if (0 == p)
                {
                    index = r;
                    return false;
                }

                p += l;

                int compare = key.CompareTo(keySelector(list[p]));
                if (compare == 0)
                {
                    index = p;
                    return true;
                }

                if (compare > 0)
                {
                    l = p;
                }
                else
                {
                    r = p;
                }
            }
        }

        #endregion

        #region CollectionBase wrappers
        [Serializable]
        private sealed class ListRange<T> : SimpleListBase<T>, ICollection<T>
        {
            private readonly IList<T> _wrappedList;
            private readonly int _start;
            private int _count;

            public ListRange(IList<T> wrappedList, int start, int count)
            {
                _wrappedList = wrappedList;
                _start = start;
                _count = count;
            }

            public override int Count => Math.Min(_count, _wrappedList.Count - _start);

            public override void Clear()
            {
                if (_wrappedList.Count - _start < _count)
                {
                    _count = _wrappedList.Count - _start;
                }

                while (_count > 0)
                {
                    _wrappedList.RemoveAt(_start + _count - 1);
                    --_count;
                }
            }

            public override void Insert(int index, T item)
            {
                if (index < 0 || index > _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                _wrappedList.Insert(_start + index, item);
                ++_count;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                _wrappedList.RemoveAt(_start + index);
                --_count;
            }

            public override bool Remove(T item)
            {
                if (_wrappedList.IsReadOnly)
                {
                    throw new NotSupportedException("Cannot modify read-only collection.");
                }

                return base.Remove(item);
            }

            public override T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    return _wrappedList[_start + index];
                }
                set
                {
                    if (index < 0 || index >= _count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    _wrappedList[_start + index] = value;
                }
            }

            bool ICollection<T>.IsReadOnly => _wrappedList.IsReadOnly;
        }

        public static IList<T> Range<T>([NotNull] this IList<T> list, int start, int count)
        {
            if (list == null)
            {
                throw new ArgumentOutOfRangeException(nameof(list));
            }

            if ((start < 0) || (start > list.Count) || ((start == list.Count) && (count != 0)))
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if ((count < 0) || (count > list.Count) || ((count + start) > list.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return new ListRange<T>(list, start, count);
        }

        [Serializable]
        private sealed class ReadOnlyCollection<T> : ICollection<T>
        {
            private readonly ICollection<T> _wrappedCollection; // The collection we are wrapping (never null).

            public ReadOnlyCollection(ICollection<T> wrappedCollection)
            {
                _wrappedCollection = wrappedCollection;
            }

            private static void MethodModifiesCollection()
            {
                throw new NotSupportedException("Cannot modify read-only collection.");
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _wrappedCollection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_wrappedCollection).GetEnumerator();
            }

            public bool Contains(T item)
            {
                return _wrappedCollection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _wrappedCollection.CopyTo(array, arrayIndex);
            }

            public int Count => _wrappedCollection.Count;

            public bool IsReadOnly => true;

            public void Add(T item)
            {
                MethodModifiesCollection();
            }

            public void Clear()
            {
                MethodModifiesCollection();
            }

            public bool Remove(T item)
            {
                MethodModifiesCollection();
                return false;
            }
        }

        [NotNull]
        public static ICollection<T> AsReadOnly<T>([NotNull] this ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return new ReadOnlyCollection<T>(collection);
        }

        [Serializable]
        private sealed class ReadOnlyList<T> : IList<T>
        {
            private readonly IList<T> _wrappedList; // The list we are wrapping (never null).

            public ReadOnlyList(IList<T> wrappedList)
            {
                _wrappedList = wrappedList;
            }

            private static void MethodModifiesCollection()
            {
                throw new NotSupportedException("Cannot modify read-only collection.");
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _wrappedList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_wrappedList).GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return _wrappedList.IndexOf(item);
            }

            public bool Contains(T item)
            {
                return _wrappedList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _wrappedList.CopyTo(array, arrayIndex);
            }

            public int Count => _wrappedList.Count;

            public bool IsReadOnly => true;

            public T this[int index]
            {
                get => _wrappedList[index];
                // ReSharper disable ValueParameterNotUsed
                set => MethodModifiesCollection();
                // ReSharper restore ValueParameterNotUsed
            }

            public void Add(T item)
            {
                MethodModifiesCollection();
            }

            public void Clear()
            {
                MethodModifiesCollection();
            }

            public void Insert(int index, T item)
            {
                MethodModifiesCollection();
            }

            public void RemoveAt(int index)
            {
                MethodModifiesCollection();
            }

            public bool Remove(T item)
            {
                MethodModifiesCollection();
                return false;
            }
        }

        [Serializable]
        private sealed class ListAsIndexedCollection<T> : IIndexedCollection<T>
        {
            private readonly IList<T> _list;

            public ListAsIndexedCollection([NotNull] IList<T> list)
            {
                _list = list ?? throw new ArgumentNullException(nameof(list));
            }

            #region Implementation of IEnumerable

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of IIndexedEnumerable<T>

            public int Count => _list.Count;

            public T this[int index] => _list[index];

            #endregion

            #region Implementation of IIndexedCollection<T>

            public bool Contains(T value)
            {
                return _list.Contains(value);
            }

            public int IndexOf(T value)
            {
                return _list.IndexOf(value);
            }

            public void CopyTo(T[] array, int destinationIndex)
            {
                _list.CopyTo(array, destinationIndex);
            }

            #endregion
        }

        [CanBeNull]
        public static IList<T> AsReadOnly<T>([CanBeNull] this IList<T> list)
        {
            return list == null ? null : list.IsReadOnly ? list : new ReadOnlyList<T>(list);
        }

        [Serializable]
        private sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            // The dictionary that is wrapped
            private readonly IDictionary<TKey, TValue> _wrappedDictionary;

            public ReadOnlyDictionary([NotNull] IDictionary<TKey, TValue> wrappedDictionary)
            {
                _wrappedDictionary = wrappedDictionary ?? throw new ArgumentNullException(nameof(wrappedDictionary));
            }

            private static void MethodModifiesCollection()
            {
                throw new NotSupportedException("Cannot modify read-only dictionary");
            }

            public void Add(TKey key, TValue value)
            {
                MethodModifiesCollection();
            }

            public bool ContainsKey(TKey key)
            {
                return _wrappedDictionary.ContainsKey(key);
            }

            public ICollection<TKey> Keys => _wrappedDictionary.Keys.AsReadOnly();

            public ICollection<TValue> Values => _wrappedDictionary.Values.AsReadOnly();

            public bool Remove(TKey key)
            {
                MethodModifiesCollection();
                return false; // never reached
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _wrappedDictionary.TryGetValue(key, out value);
            }

            public TValue this[TKey key]
            {
                get => _wrappedDictionary[key];
                // ReSharper disable ValueParameterNotUsed
                set => MethodModifiesCollection();
                // ReSharper restore ValueParameterNotUsed
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                MethodModifiesCollection();
            }

            public void Clear()
            {
                MethodModifiesCollection();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return _wrappedDictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                _wrappedDictionary.CopyTo(array, arrayIndex);
            }

            public int Count => _wrappedDictionary.Count;

            public bool IsReadOnly => true;

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                MethodModifiesCollection();
                return false; // never reached
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _wrappedDictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_wrappedDictionary).GetEnumerator();
            }
        }

        public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary == null ? null : dictionary.IsReadOnly ? dictionary : new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        [Serializable]
        private sealed class TypedEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator _wrappedEnumerator;

            public TypedEnumerator(IEnumerator wrappedEnumerator)
            {
                _wrappedEnumerator = wrappedEnumerator;
            }

            T IEnumerator<T>.Current => (T)_wrappedEnumerator.Current;

            void IDisposable.Dispose()
            {
                if (_wrappedEnumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            object IEnumerator.Current => _wrappedEnumerator.Current;

            bool IEnumerator.MoveNext()
            {
                return _wrappedEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                _wrappedEnumerator.Reset();
            }
        }

        [Serializable]
        private sealed class TypedEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable _wrappedEnumerable;

            public TypedEnumerable(IEnumerable wrappedEnumerable)
            {
                _wrappedEnumerable = wrappedEnumerable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new TypedEnumerator<T>(_wrappedEnumerable.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _wrappedEnumerable.GetEnumerator();
            }
        }

        [CanBeNull]
        public static IEnumerable<T> TypedAs<T>([CanBeNull] this IEnumerable untypedCollection)
        {
            return untypedCollection == null
                ? (IEnumerable<T>)null
                : untypedCollection is IEnumerable<T> enumerables ? enumerables : new TypedEnumerable<T>(untypedCollection);
        }

        [Serializable]
        private sealed class ArrayWrapper<T> : SimpleListBase<T>, IList
        {
            private readonly T[] _wrappedArray;

            public ArrayWrapper(T[] wrappedArray)
            {
                _wrappedArray = wrappedArray;
            }

            public override int Count => _wrappedArray.Length;

            public override void Clear()
            {
                int count = _wrappedArray.Length;
                for (int i = 0; i < count; ++i)
                {
                    _wrappedArray[i] = default;
                }
            }

            public override void Insert(int index, T item)
            {
                if (index < 0 || index > _wrappedArray.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index + 1 < _wrappedArray.Length)
                {
                    Array.Copy(_wrappedArray, index, _wrappedArray, index + 1, _wrappedArray.Length - index - 1);
                }

                if (index < _wrappedArray.Length)
                {
                    _wrappedArray[index] = item;
                }
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= _wrappedArray.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index < _wrappedArray.Length - 1)
                {
                    Array.Copy(_wrappedArray, index + 1, _wrappedArray, index, _wrappedArray.Length - index - 1);
                }

                _wrappedArray[_wrappedArray.Length - 1] = default;
            }

            public override T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _wrappedArray.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    return _wrappedArray[index];
                }
                set
                {
                    if (index < 0 || index >= _wrappedArray.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    _wrappedArray[index] = value;
                }
            }

            public override void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (array.Length < _wrappedArray.Length)
                {
                    throw new ArgumentException("array is too short", nameof(array));
                }

                if (arrayIndex < 0 || arrayIndex >= array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }

                if (array.Length + arrayIndex < _wrappedArray.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }

                Array.Copy(_wrappedArray, 0, array, arrayIndex, _wrappedArray.Length);
            }

            public override IEnumerator<T> GetEnumerator()
            {
                return ((IList<T>)_wrappedArray).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _wrappedArray.GetEnumerator();
            }

            bool IList.IsFixedSize => true;
        }
        #endregion CollectionBase wrappers

        #region Consecutive items
        public static void RemoveRange<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] IEnumerable<TKey> keys)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            foreach (TKey key in keys)
            {
                _ = dictionary.Remove(key);
            }
        }
        #endregion Consecutive items

        #region Find and IndexOfSubsequence
        public static int CountWhere<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            int count = 0;

            foreach (T item in collection)
            {
                if (predicate(item))
                {
                    ++count;
                }
            }

            return count;
        }

        public static ICollection<T> RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (collection is T[] v)
            {
                collection = new ArrayWrapper<T>(v);
            }

            if (collection.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(collection));
            }

            if (collection is IList<T> list)
            {
                int i = -1, j = 0;
                int listCount = list.Count;
                List<T> removed = new List<T>();

                // Remove item where predicate is true, compressing items to lower in the list. This is much more
                // efficient than the naive algorithm that uses IList<T>.Remove().
                while (j < listCount)
                {
                    T item = list[j];
                    if (predicate(item))
                    {
                        removed.Add(item);
                    }
                    else
                    {
                        ++i;
                        if (i != j)
                        {
                            list[i] = item;
                        }
                    }
                    ++j;
                }

                ++i;
                if (i < listCount)
                {
                    // remove items from the end.
                    if (list is IList lists && lists.IsFixedSize)
                    {
                        // An array or similar. Null out the last elements.
                        while (i < listCount)
                        {
                            list[i++] = default;
                        }
                    }
                    else
                    {
                        // Normal list.
                        while (i < listCount)
                        {
                            list.RemoveAt(listCount - 1);
                            --listCount;
                        }
                    }
                }

                return removed;
            }
            else
            {
                // We have to copy all the items to remove to a List, because collections can't be modifed 
                // during an enumeration.
                List<T> removed = new List<T>();

                foreach (T item in collection)
                {
                    if (predicate(item))
                    {
                        removed.Add(item);
                    }
                }

                foreach (T item in removed)
                {
                    _ = collection.Remove(item);
                }

                return removed;
            }
        }

        public static bool TryFindFirstItem<T>(this IEnumerable<T> collection, Func<T, bool> condition, out T foundItem)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            foreach (T item in collection.Where(condition))
            {
                foundItem = item;
                return true;
            }

            foundItem = default;
            return false;
        }

        /// <summary>
        /// Attempts to locate the last item in a collection which matches the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of item in the collection.</typeparam>
        /// <param name="collection">The collection to search.</param>
        /// <param name="condition">The condition to check.</param>
        /// <param name="foundItem">The last item matching <paramref name="condition"/> if any were found.</param>
        /// <returns><c>true</c> if any matching items were found; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// If the collection is an <see cref="IList&lt;T&gt;"/> or <see cref="IIndexedEnumerable&lt;T&gt;"/>,
        /// then the collection will simply be searched in reverse.  If it is an <see cref="ICollection&lt;T&gt;"/>
        /// with 8,192 elements or less, the collection will be copied into a new arary, which will then be
        /// searched in reverse.  Otherwise, a new linked list will be created and populated by the collection,
        /// then searched in reverse.  This approach is taken under the assumption that the predicate may be
        /// non-trivial, and therefore it may be cheaper to construct a reversed list than to apply the predicate
        /// to every item in the collection.  A linked list is used in the worst case to avoid allocating large
        /// chunks of contiguous memory for an array (if the collection size is known) and/or the overhead of
        /// array resizing (if the collection size is not known).
        /// </remarks>
        public static bool TryFindLastItem<T>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, bool> condition, [CanBeNull] out T foundItem)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            foundItem = default;

            IList<T> list = collection as IList<T>;
            if (list == null)
            {
                const int listConstructionThreshold = 8192;

                ICollection<T> trueCollection = collection as ICollection<T>;
                if (trueCollection?.Count <= listConstructionThreshold)
                {
                    list = new T[trueCollection.Count];
                    collection.CopyTo(list);
                }
            }
            if (list != null)
            {
                for (int index = list.Count - 1; index >= 0; --index)
                {
                    T item = list[index];
                    if (!condition(item))
                    {
                        continue;
                    }

                    foundItem = item;
                    return true;
                }

                foundItem = default;
                return false;
            }

            if (collection is IIndexedEnumerable<T> indexedEnumerable)
            {
                for (int index = indexedEnumerable.Count - 1; index >= 0; --index)
                {
                    T item = indexedEnumerable[index];
                    if (!condition(item))
                    {
                        continue;
                    }

                    foundItem = item;
                    return true;
                }

                foundItem = default;
                return false;
            }

            LinkedList<T> linkedList = new LinkedList<T>(collection);
            for (LinkedListNode<T> currentNode = linkedList.Last; currentNode.Previous != null; currentNode = currentNode.Previous)
            {
                if (condition(currentNode.Value))
                {
                    foundItem = currentNode.Value;
                    return true;
                }
            }

            return false;
        }

        public static int FirstIndexWhere<T>([NotNull] this IIndexedEnumerable<T> list, [NotNull] Func<T, bool> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            int index = 0;
            foreach (T item in list)
            {
                if (predicate(item))
                {
                    return index;
                }

                ++index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int FirstIndexWhere<T>([NotNull] this IList<T> list, Func<T, bool> predicate)
        {
            return FirstIndexWhere(new ListAsIndexedCollection<T>(list), predicate);
        }

        public static int LastIndexWhere<T>([NotNull] this IList<T> list, [NotNull] Func<T, bool> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return LastIndexWhere(new ListAsIndexedCollection<T>(list), predicate);
        }

        public static int LastIndexWhere<T>(this IIndexedEnumerable<T> list, Func<T, bool> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            for (int index = list.Count - 1; index >= 0; --index)
            {
                if (predicate(list[index]))
                {
                    return index;
                }
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int FirstIndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            return FirstIndexOf(new ListAsIndexedCollection<T>(list), item, equalityComparer);
        }

        public static int FirstIndexOf<T>(this IIndexedEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (equalityComparer == null)
            {
                throw new ArgumentNullException(nameof(equalityComparer));
            }

            int index = 0;
            foreach (T x in list)
            {
                if (equalityComparer.Equals(x, item))
                {
                    return index;
                }

                ++index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int LastIndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            return LastIndexOf(new ListAsIndexedCollection<T>(list), item, equalityComparer);
        }

        public static int LastIndexOf<T>(this IIndexedEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (equalityComparer == null)
            {
                throw new ArgumentNullException(nameof(equalityComparer));
            }

            for (int index = list.Count - 1; index >= 0; --index)
            {
                if (equalityComparer.Equals(list[index], item))
                {
                    return index;
                }
            }

            // didn't find any item that matches.
            return -1;
        }
        #endregion Find and IndexOfSubsequence

        #region Set operations (coded except EqualSets)
        public static bool SetEquals<T>([NotNull] this IEnumerable<T> firstCollection, [NotNull] IEnumerable<T> secondCollection)
        {
            return SetEquals(firstCollection, secondCollection, EqualityComparer<T>.Default);
        }

        public static bool SetEquals<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection,
            [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (firstCollection == null)
            {
                throw new ArgumentNullException(nameof(firstCollection));
            }

            if (secondCollection == null)
            {
                throw new ArgumentNullException(nameof(secondCollection));
            }

            if (equalityComparer == null)
            {
                throw new ArgumentException(nameof(equalityComparer));
            }

            if ((!(firstCollection is HashSet<T> firstSet)) || (firstSet.Comparer != equalityComparer))
            {
                firstSet = new HashSet<T>(firstCollection, equalityComparer);
            }

            if ((!(secondCollection is HashSet<T> secondSet)) || (secondSet.Comparer != equalityComparer))
            {
                secondSet = new HashSet<T>(secondCollection, equalityComparer);
            }

            return firstSet.SetEquals(secondSet);
        }
        #endregion Set operations

        #region String representations (not yet coded)
        [NotNull]
        public static string ToString<T>([NotNull] this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return ToString(collection, true, "{", ",", "}");
        }

        [NotNull]
        public static string ToString<T>(
            [NotNull] this IEnumerable<T> collection,
            bool recursive,
            [NotNull] string start,
            [NotNull] string separator,
            [NotNull] string end)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            if (collection == null)
            {
                return "null";
            }

            bool firstItem = true;

            StringBuilder builder = new StringBuilder();

            _ = builder.Append(start);

            // Call ToString on each item and put it in.
            foreach (T item in collection)
            {
                if (!firstItem)
                {
                    _ = builder.Append(separator);
                }

                // "TypedAs<object>((IEnumerable)item)" is never null because item is never 'null'.
                // ReSharper disable AssignNullToNotNullAttribute
                if (item == null)
                {
                    _ = builder.Append("null");
                }
                else
                {
                    _ = recursive && item is IEnumerable enumerable && !(item is string)
                        ? builder.Append(ToString(TypedAs<object>(enumerable), true, start, separator, end))
                        : builder.Append(item.ToString());
                }
                // ReSharper restore AssignNullToNotNullAttribute

                firstItem = false;
            }

            _ = builder.Append(end);
            return builder.ToString();
        }
        #endregion String representations

        #region Shuffles and Permutations
        public static T RandomElement<T>([NotNull] this IEnumerable<T> collection)
        {
            if (!SelectRandomElement(collection, out T result))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            return result;
        }

        public static T RandomElementOrDefault<T>([NotNull] this IEnumerable<T> collection)
        {
            _ = SelectRandomElement(collection, out T result);
            return result;
        }

        private static bool SelectRandomElement<T>([NotNull] IEnumerable<T> collection, out T result)
        {
            result = default;

            if (!collection.Any())
            {
                return false;
            }

            IList trueList = collection as IList;
            IIndexedEnumerable<T> indexable = collection as IIndexedEnumerable<T>;

            int count;

            if (trueList != null)
            {
                count = trueList.Count;
            }
            else if (indexable != null)
            {
                count = indexable.Count;
            }
            else if (collection is ICollection list)
            {
                count = list.Count;
            }
            else
            {
                count = collection.Count();
            }

            int skipCount = RandomProvider.Shared.Next(count);

            if (trueList != null)
            {
                result = (T)trueList[skipCount];
            }
            else if (indexable != null)
            {
                result = indexable[skipCount];
            }
            else
            {
                result = collection.Skip(skipCount).First();
            }

            return true;
        }

        public static T[] Randomize<T>([NotNull] this IEnumerable<T> collection)
        {
            // We have to copy all items anyway, and there isn't a way to produce the items
            // on the fly that is linear. So copying to an array and shuffling it is an efficient as we can get.
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            T[] array = collection.ToArray();

            int count = array.Length;
            for (int i = count - 1; i >= 1; --i)
            {
                // Pick an random number 0 through i inclusive.
                int j = RandomProvider.Shared.Next(i + 1);

                // Swap array[i] and array[j]
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            return array;
        }

        public static void RandomizeInPlace<T>([NotNull] this IList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (list.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(list));
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                int j = RandomProvider.Shared.Next(i, list.Count);

                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }
        #endregion Shuffles and Permutations

        #region MinElement and MaxElement
        public static T MaxElement<T, TComparand>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, TComparand> comparandResolver)
            where TComparand : IComparable<TComparand>
        {
            return MaxElement(collection, comparandResolver, Comparer<TComparand>.Default);
        }

        public static T MaxElement<T, TComparand>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, TComparand> comparandResolver,
            [NotNull] IComparer<TComparand> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (comparandResolver == null)
            {
                throw new ArgumentNullException(nameof(comparandResolver));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            TComparand maxSoFar = default;
            T maxElementSoFar = default;
            bool foundOne = false;

            // Go through the collection, keeping the maximum found so far.
            foreach (T item in collection)
            {
                TComparand comparand = comparandResolver(item);
                if (!foundOne || comparer.Compare(maxSoFar, comparand) < 0)
                {
                    maxSoFar = comparand;
                    maxElementSoFar = item;
                }
                foundOne = true;
            }

            // If the collection was empty, throw an exception.
            if (!foundOne)
            {
                throw new InvalidOperationException("Sequence is empty.");
            }

            return maxElementSoFar;
        }

        public static T MinElement<T, TComparand>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, TComparand> comparandResolver)
            where TComparand : IComparable<TComparand>
        {
            return MinElement(collection, comparandResolver, Comparer<TComparand>.Default);
        }

        public static T MinElement<T, TComparand>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, TComparand> comparandResolver,
            [NotNull] IComparer<TComparand> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            TComparand minSoFar = default;
            T minElementSoFar = default;
            bool foundOne = false;

            // Go through the collection, keeping the minimum found so far.
            foreach (T item in collection)
            {
                TComparand comparand = comparandResolver(item);
                if (!foundOne || comparer.Compare(minSoFar, comparand) > 0)
                {
                    minSoFar = comparand;
                    minElementSoFar = item;
                }
                foundOne = true;
            }

            // If the collection was empty, throw an exception.
            if (!foundOne)
            {
                throw new InvalidOperationException("Sequence is empty.");
            }

            return minElementSoFar;
        }
        #endregion MinElement and MaxElement

        #region Sorting and operations on sorted collections

        [NotNull]
        public static T[] Sort<T>([NotNull] this IEnumerable<T> collection, [NotNull] IComparer<T> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            T[] array = collection.ToArray();

            Array.Sort(array, comparer);
            return array;
        }

        [NotNull]
        public static T[] Sort<T>([NotNull] this IEnumerable<T> collection, [NotNull] Comparison<T> comparison)
        {
            return Sort(collection, new ComparerFromComparison<T>(comparison));
        }

        public static IList<T> SortInPlace<T>([NotNull] this IList<T> list, [NotNull] IComparer<T> comparer)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            // If we have an array, use the built-in array sort (faster than going through IList accessors
            // with virtual calls).
            if (list is T[] v)
            {
                Array.Sort(v, comparer);
                return list;
            }

            if (list.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(list));
            }

            // Instead of a recursive procedure, we use an explicit stack to hold
            // ranges that we still need to sort.
            int[] leftStack = new int[32], rightStack = new int[32];
            int stackPtr = 0;

            int l = 0; // the inclusive left edge of the current range we are sorting.
            int r = list.Count - 1; // the inclusive right edge of the current range we are sorting.

            // Loop until we have nothing left to sort. On each iteration, l and r contains the bounds
            // of something to sort (unless r <= l), and leftStack/rightStack have a stack of unsorted
            // pieces (unles stackPtr == 0).
            for (; ; )
            {
                if (l == r - 1)
                {
                    // We have exactly 2 elements to sort. Compare them and swap if needed.
                    T e1 = list[l];
                    T e2 = list[r];
                    if (comparer.Compare(e1, e2) > 0)
                    {
                        list[r] = e1;
                        list[l] = e2;
                    }
                    l = r; // sort complete, find other work from the stack.
                }
                else if (l < r)
                {
                    // Sort the items in the inclusive range l .. r

                    // Get the left, middle, and right-most elements and sort them, yielding e1=smallest, e2=median, e3=largest
                    int m = l + ((r - l) / 2);
                    T e1 = list[l], e2 = list[m], e3 = list[r], temp;
                    if (comparer.Compare(e1, e2) > 0)
                    {
                        temp = e1;
                        e1 = e2;
                        e2 = temp;
                    }
                    if (comparer.Compare(e1, e3) > 0)
                    {
                        temp = e3;
                        e3 = e2;
                        e2 = e1;
                        e1 = temp;
                    }
                    else if (comparer.Compare(e2, e3) > 0)
                    {
                        temp = e2;
                        e2 = e3;
                        e3 = temp;
                    }

                    if (l == r - 2)
                    {
                        // We have exactly 3 elements to sort, and we've done that. Store back and we're done.
                        list[l] = e1;
                        list[m] = e2;
                        list[r] = e3;
                        l = r; // sort complete, find other work from the stack.
                    }
                    else
                    {
                        // Put the smallest at the left, largest in the middle, and the median at the right (which is the partitioning value)
                        list[l] = e1;
                        list[m] = e3;
                        T partition; // The partition value.
                        list[r] = partition = e2;

                        // Partition into three parts, items <= partition, items == partition, and items >= partition
                        int i = l, j = r;
                        T firstItem;
                        for (; ; )
                        {
                            do
                            {
                                ++i;
                                firstItem = list[i];
                            }
                            while (comparer.Compare(firstItem, partition) < 0);

                            T secondItem;
                            do
                            {
                                --j;
                                secondItem = list[j];
                            }
                            while (comparer.Compare(secondItem, partition) > 0);

                            if (j < i)
                            {
                                break;
                            }

                            list[i] = secondItem;
                            list[j] = firstItem; // swap items to continue the partition.
                        }

                        // Move the partition value into place.
                        list[r] = firstItem;
                        list[i] = partition;
                        ++i;

                        // We have partitioned the list. 
                        //    Items in the inclusive range l .. j are <= partition.
                        //    Items in the inclusive range i .. r are >= partition.
                        //    Items in the inclusive range j+1 .. i - 1 are == partition (and in the correct final position).
                        // We now need to sort l .. j and i .. r.
                        // To do this, we stack one of the lists for later processing, and change l and r to the other list.
                        // If we always stack the larger of the two sub-parts, the stack cannot get greater
                        // than log2(Count) in size; i.e., a 32-element stack is enough for the maximum list size.
                        if ((j - l) > (r - i))
                        {
                            // The right partition is smaller. Stack the left, and get ready to sort the right.
                            leftStack[stackPtr] = l;
                            rightStack[stackPtr] = j;
                            l = i;
                        }
                        else
                        {
                            // The left partition is smaller. Stack the right, and get ready to sort the left.
                            leftStack[stackPtr] = i;
                            rightStack[stackPtr] = r;
                            r = j;
                        }
                        ++stackPtr;
                    }
                }
                else if (stackPtr > 0)
                {
                    // We have a stacked sub-list to sort. Pop it off and sort it.
                    --stackPtr;
                    l = leftStack[stackPtr];
                    r = rightStack[stackPtr];
                }
                else
                {
                    // We have nothing left to sort.
                    break;
                }
            }

            return list;
        }

        public static IList<T> SortInPlace<T>([NotNull] this IList<T> list, [NotNull] Comparison<T> comparison)
        {
            return SortInPlace(list, new ComparerFromComparison<T>(comparison));
        }
        #endregion Sorting

        #region Miscellaneous operations on IEnumerable

        public static bool Any<T>([NotNull] this IEnumerable<T> collection, Func<T, int, bool> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int i = 0;

            for (IEnumerator<T> e = collection.GetEnumerator(); e.MoveNext(); i++)
            {
                if (predicate(e.Current, i))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CountAtLeast<T>([NotNull] this IEnumerable<T> collection, int count)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return Any(collection, (_, i) => (i + 1) >= count);
        }

        [NotNull]
        public static IEnumerable<T> ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T item in collection)
            {
                action(item);
            }

            return collection;
        }

        [NotNull]
        public static IEnumerable<T> ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T, int> action)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int i = 0;
            foreach (T item in collection)
            {
                action(item, i++);
            }

            return collection;
        }
        #endregion Miscellaneous operations on IEnumerable

        #region Miscellaneous operations on IList
        public static void AddRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.IsReadOnly)
            {
                throw new ArgumentException("CollectionBase is read-only.", nameof(collection));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _ = items.ForEach(collection.Add);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] IList<T> dest)
        {
            CopyTo(source, dest, 0, int.MaxValue);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] T[] dest)
        {
            CopyTo(source, dest, 0);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] T[] dest, int destIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (destIndex < 0 || destIndex > dest.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(destIndex));
            }

            foreach (T item in source)
            {
                if (destIndex >= dest.Length)
                {
                    throw new ArgumentException("Array is too small.", nameof(dest));
                }

                dest[destIndex++] = item;
            }
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] IList<T> dest, int destIndex, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (dest.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(dest));
            }

            int destCount = dest.Count;

            if (destIndex < 0 || destIndex > destCount)
            {
                throw new ArgumentOutOfRangeException(nameof(destIndex));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            using (IEnumerator<T> sourceEnum = source.GetEnumerator())
            {
                // First, overwrite items to the end of the destination list.
                while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
                {
                    dest[destIndex++] = sourceEnum.Current;
                    --count;
                }

                // Second, insert items until done.
                while (count > 0 && sourceEnum.MoveNext())
                {
                    dest.Insert(destCount++, sourceEnum.Current);
                    --count;
                }
            }
        }

        public static void CopyTo<T>([NotNull] this IList<T> source, int sourceIndex, [NotNull] IList<T> dest, int destIndex, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (dest.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(dest));
            }

            int sourceCount = source.Count;
            int destCount = dest.Count;

            if (sourceIndex < 0 || sourceIndex >= sourceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (destIndex < 0 || destIndex > destCount)
            {
                throw new ArgumentOutOfRangeException(nameof(destIndex));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > sourceCount - sourceIndex)
            {
                count = sourceCount - sourceIndex;
            }

            if (source == dest && sourceIndex > destIndex)
            {
                while (count > 0)
                {
                    dest[destIndex++] = source[sourceIndex++];
                    --count;
                }
            }
            else
            {
                int si, di;

                // First, insert any items needed at the end
                if (destIndex + count > destCount)
                {
                    int numberToInsert = destIndex + count - destCount;
                    si = sourceIndex + (count - numberToInsert);
                    di = destCount;
                    count -= numberToInsert;
                    while (numberToInsert > 0)
                    {
                        dest.Insert(di++, source[si++]);
                        --numberToInsert;
                    }
                }

                // Do the copy, from end to beginning in case of overlap.
                si = sourceIndex + count - 1;
                di = destIndex + count - 1;
                while (count > 0)
                {
                    dest[di--] = source[si--];
                    --count;
                }
            }
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] IList<TResult> dest)
        {
            SelectInto(source, selector, dest, 0, int.MaxValue);
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] IList<TResult> dest, int destIndex, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (dest.IsReadOnly)
            {
                throw new ArgumentException("List is read-only.", nameof(dest));
            }

            int destCount = dest.Count;

            if (destIndex < 0 || destIndex > destCount)
            {
                throw new ArgumentOutOfRangeException(nameof(destIndex));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            using (IEnumerator<T> sourceEnum = source.GetEnumerator())
            {
                // First, overwrite items to the end of the destination list.
                while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
                {
                    dest[destIndex++] = selector(sourceEnum.Current);
                    --count;
                }

                // Second, insert items until done.
                while (count > 0 && sourceEnum.MoveNext())
                {
                    dest.Insert(destCount++, selector(sourceEnum.Current));
                    --count;
                }
            }
        }
        #endregion Miscellaneous operations on IList

        #region CollectionBase Conversions

        [NotNull]
        public static HashSet<T> ToHashSet<T>([NotNull] this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        #endregion
    }

    [Serializable]
    [DebuggerDisplay("{DebuggerDisplayString()}")]
    internal abstract class SimpleCollectionBase<T> : ICollection<T>, ICollection
    {
        public override string ToString()
        {
            return CollectionExtensions.ToString(this);
        }

        #region ICollection<T> Members
        public virtual void Add(T item)
        {
            throw new NotSupportedException();
        }

        public abstract void Clear();

        public abstract bool Remove(T item);

        public virtual bool Contains(T item)
        {
            IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
            foreach (T i in this)
            {
                if (equalityComparer.Equals(i, item))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            int count = Count;

            if (count == 0)
            {
                return;
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Value must be non-negative");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Value must be non-negative");
            }

            if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Array is too small.", nameof(arrayIndex));
            }

            int index = arrayIndex, i = 0;
            foreach (T item in this)
            {
                if (i >= count)
                {
                    break;
                }

                array[index] = item;
                ++index;
                ++i;
            }
        }

        public virtual T[] ToArray()
        {
            int count = Count;

            T[] array = new T[count];
            CopyTo(array, 0);
            return array;
        }

        public abstract int Count { get; }

        bool ICollection<T>.IsReadOnly => false;

        public virtual ICollection<T> AsReadOnly()
        {
            return CollectionExtensions.AsReadOnly(this);
        }
        #endregion

        #region Delegate operations
        public virtual bool Exists(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.Any(predicate);
        }

        public virtual bool TrueForAll(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.All(predicate);
        }

        public virtual int CountWhere(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return CollectionExtensions.CountWhere(this, predicate);
        }

        public virtual IEnumerable<T> FindAll(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.Where(predicate);
        }

        public virtual ICollection<T> RemoveAll(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.RemoveWhere(predicate);
        }

        public virtual void ForEach(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _ = CollectionExtensions.ForEach(this, action);
        }

        public virtual IEnumerable<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            return this.Select(o => converter(o));
        }
        #endregion

        #region IEnumerable<T> Members
        public abstract IEnumerator<T> GetEnumerator();
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            int count = Count;

            if (count == 0)
            {
                return;
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be non-negative.");
            }

            if (index >= array.Length || count > array.Length - index)
            {
                throw new ArgumentException("Array is too small.", nameof(index));
            }

            int i = 0;
            // TODO: Look into this
            foreach (object o in (ICollection)this)
            {
                if (i >= count)
                {
                    break;
                }

                array.SetValue(o, index);
                ++index;
                ++i;
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (T item in this)
            {
                yield return item;
            }
        }
        #endregion

        internal string DebuggerDisplayString()
        {
            const int maxlength = 250;

            StringBuilder builder = new StringBuilder();

            _ = builder.Append('{');

            // Call ToString on each item and put it in.
            bool firstItem = true;
            foreach (T item in this)
            {
                if (builder.Length >= maxlength)
                {
                    _ = builder.Append(",...");
                    break;
                }

                if (!firstItem)
                {
                    _ = builder.Append(',');
                }

                if (item == null)
                {
                    _ = builder.Append("null");
                }
                else
                {
                    _ = builder.Append(item.ToString());
                }

                firstItem = false;
            }

            _ = builder.Append('}');
            return builder.ToString();
        }
    }

    [Serializable]
    internal abstract class SimpleListBase<T> : SimpleCollectionBase<T>, IList<T>, IList
    {
        public abstract override int Count { get; }

        public abstract override void Clear();

        public abstract T this[int index] { get; set; }

        public abstract void Insert(int index, T item);

        public abstract void RemoveAt(int index);

        public override IEnumerator<T> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; ++i)
            {
                yield return this[i];
            }
        }

        public override bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public override void Add(T item)
        {
            Insert(Count, item);
        }

        public override bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public virtual void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            Range(index, count).CopyTo(array, arrayIndex);
        }

        public new virtual IList<T> AsReadOnly()
        {
            return CollectionExtensions.AsReadOnly(this);
        }

        public virtual T Find(Func<T, bool> predicate)
        {
            return this.FirstOrDefault(predicate);
        }

        public virtual bool TryFind(Func<T, bool> predicate, out T foundItem)
        {
            return this.TryFindFirstItem(predicate, out foundItem);
        }

        public virtual T FindLast(Func<T, bool> predicate)
        {
            return this.LastOrDefault(predicate);
        }

        public virtual bool TryFindLast(Func<T, bool> predicate, out T foundItem)
        {
            return this.TryFindLastItem(predicate, out foundItem);
        }

        public virtual int FindIndex(Func<T, bool> predicate)
        {
            return this.FirstIndexWhere(predicate);
        }

        public virtual int FindIndex(int index, Func<T, bool> predicate)
        {
            int foundIndex = Range(index, Count - index).FirstIndexWhere(predicate);
            return foundIndex < 0 ? -1 : foundIndex + index;
        }

        public virtual int FindIndex(int index, int count, Func<T, bool> predicate)
        {
            int foundIndex = Range(index, count).FirstIndexWhere(predicate);
            return foundIndex < 0 ? -1 : foundIndex + index;
        }

        public virtual int FindLastIndex(Func<T, bool> predicate)
        {
            return this.LastIndexWhere(predicate);
        }

        public virtual int FindLastIndex(int index, Func<T, bool> predicate)
        {
            return Range(0, index + 1).LastIndexWhere(predicate);
        }

        public virtual int FindLastIndex(int index, int count, Func<T, bool> predicate)
        {
            int foundIndex = Range(index - count + 1, count).LastIndexWhere(predicate);

            return foundIndex >= 0 ? foundIndex + index - count + 1 : -1;
        }

        public virtual int IndexOf(T item)
        {
            return this.FirstIndexOf(item, EqualityComparer<T>.Default);
        }

        public virtual int IndexOf(T item, int index)
        {
            int foundIndex = Range(index, Count - index).FirstIndexOf(item, EqualityComparer<T>.Default);

            return foundIndex >= 0 ? foundIndex + index : -1;
        }

        public virtual int IndexOf(T item, int index, int count)
        {
            int foundIndex = Range(index, count).FirstIndexOf(item, EqualityComparer<T>.Default);

            return foundIndex >= 0 ? foundIndex + index : -1;
        }

        public virtual int LastIndexOf(T item)
        {
            return this.LastIndexOf(item, EqualityComparer<T>.Default);
        }

        public virtual int LastIndexOf(T item, int index)
        {
            return Range(0, index + 1).LastIndexOf(item, EqualityComparer<T>.Default);
        }

        public virtual int LastIndexOf(T item, int index, int count)
        {
            int foundIndex = Range(index - count + 1, count).LastIndexOf(item, EqualityComparer<T>.Default);

            return foundIndex >= 0 ? foundIndex + index - count + 1 : -1;
        }

        public virtual IList<T> Range(int start, int count)
        {
            return CollectionExtensions.Range(this, start, count);
        }

        private static T ConvertToItemType(string name, object value)
        {
            try
            {
                return (T)value;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "Cannot convert item of type {1} to type {2}: {0}",
                        value,
                        value.GetType().FullName,
                        typeof(T).FullName),
                    name);
            }
        }

        int IList.Add(object value)
        {
            int count = Count;
            Insert(count, ConvertToItemType("value", value));
            return count;
        }

        void IList.Clear()
        {
            Clear();
        }

        bool IList.Contains(object value)
        {
            return (value is T || value == null) && Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return value is T || value == null ? IndexOf((T)value) : -1;
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, ConvertToItemType("value", value));
        }

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => ((ICollection<T>)this).IsReadOnly;

        void IList.Remove(object value)
        {
            if (value is T || value == null)
            {
                _ = Remove((T)value);
            }
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get => this[index];

            set => this[index] = ConvertToItemType("value", value);
        }
    }
    // ReSharper restore CompareNonConstrainedGenericWithNull
    // ReSharper restore HeuristicUnreachableCode
    // ReSharper restore ParameterTypeCanBeEnumerable.Global
    // ReSharper restore ConditionIsAlwaysTrueOrFalse
}