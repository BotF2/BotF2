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
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to add.</param>
        public static void AddSorted<T>([NotNull] this IList<T> list, [NotNull] T item) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (item == null)
                throw new ArgumentNullException("item");

            int index;

            LocateSorted(list, item, ItemAsKeySelector, out index);

            list.Insert(index, item);
        }

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
                throw new ArgumentNullException("list");
            if (item == null)
                throw new ArgumentNullException("item");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            int index;

            LocateSorted(list, keySelector(item), keySelector, out index);

            list.Insert(index, item);
        }

        /// <summary>
        /// Adds an item to the sorted list or replaces it if already present.
        /// </summary>
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to add.</param>
        public static void AddOrReplaceSorted<T>([NotNull] this IList<T> list, [NotNull] T item) where T : IComparable<T>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (item == null)
                throw new ArgumentNullException("item");

            int index;

            if (LocateSorted(list, item, ItemAsKeySelector, out index))
                list[index] = item;
            else
                list.Insert(index, item);
        }

        /// <summary>
        /// Adds an item to the sorted list or replaces it if already present.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to add.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        public static void AddOrReplaceSorted<TValue, TKey>([NotNull] this IList<TValue> list, [NotNull] TValue item, [NotNull] Func<TValue, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (item == null)
                throw new ArgumentNullException("item");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            int index;

            if (LocateSorted(list, keySelector(item), keySelector, out index))
                list[index] = item;
            else
                list.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the sorted list.
        /// </summary>
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was removed; false if not or if not present.</returns>
        public static bool RemoveSorted<T>([CanBeNull] this IList<T> list, [NotNull] T item) where T : IComparable<T>
        {
            return RemoveSorted(list, item, ItemAsKeySelector);
        }

        /// <summary>
        /// Removes an item from the sorted list.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="key">Key of item to locate.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        /// <returns>True if the item was removed; false if not or if not present.</returns>
        public static bool RemoveSorted<TValue, TKey>([NotNull] this IList<TValue> list, [NotNull] TKey key, [NotNull] Func<TValue, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (key == null)
                throw new ArgumentNullException("key");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            int index;

            if (LocateSorted(list, key, keySelector, out index))
            {
                list.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines the index of the specified item in the sorted list.
        /// </summary>
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to locate.</param>
        /// <returns>Index of the item if present; -1 otherwise.</returns>
        public static int IndexOfSorted<T>([NotNull] this IList<T> list, [NotNull] T item) where T : IComparable<T>
        {
            return IndexOfSorted(list, item, ItemAsKeySelector);
        }

        /// <summary>
        /// Determines the index of the specified item in the sorted list.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="key">Key of item to locate.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        /// <returns>Index of the item if present; -1 otherwise.</returns>
        public static int IndexOfSorted<TValue, TKey>([NotNull] this IList<TValue> list, [NotNull] TKey key, [NotNull] Func<TValue, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (key == null)
                throw new ArgumentNullException("key");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            int index;

            if (LocateSorted(list, key, keySelector, out index))
                return index;

            return -1;
        }

        /// <summary>
        /// Determines whether the sorted list contains the specified item.
        /// </summary>
        /// <typeparam name="T">Type of items in the list.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="item">Item to locate.</param>
        /// <returns>True if present; false otherwise.</returns>
        public static bool ContainsSorted<T>([NotNull] this IList<T> list, [NotNull] T item) where T : IComparable<T>
        {
            return ContainsSorted(list, item, ItemAsKeySelector);
        }

        /// <summary>
        /// Determines whether the sorted list contains the specified item.
        /// </summary>
        /// <typeparam name="TValue">Type of items in the list.</typeparam>
        /// <typeparam name="TKey">Type of key for the item.</typeparam>
        /// <param name="list">List to modify.</param>
        /// <param name="key">Key of item to locate.</param>
        /// <param name="keySelector">Selector for getting the key of an item.</param>
        /// <returns>True if present; false otherwise.</returns>
        public static bool ContainsSorted<TValue, TKey>([NotNull] this IList<TValue> list, [NotNull] TKey key, [NotNull] Func<TValue, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (key == null)
                throw new ArgumentNullException("key");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            return list.IndexOfSorted(key, keySelector) != -1;
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
            var l = -1;
            var r = list.Count;

            while (true)
            {
                var p = (r - l) / 2;
                if (0 == p)
                {
                    index = r;
                    return false;
                }

                p += l;

                var compare = key.CompareTo(keySelector(list[p]));
                if (compare == 0)
                {
                    index = p;
                    return true;
                }

                if (compare > 0)
                    l = p;
                else
                    r = p;
            }
        }

        /// <summary>
        /// Identity selector that returns the item as its own key.
        /// </summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="item">Item.</param>
        /// <returns>Item.</returns>
        private static T ItemAsKeySelector<T>(T item)
        {
            return item;
        }

        #endregion

        #region CollectionBase wrappers
        [Serializable]
        private sealed class IndexedCollectionRange<T> : IIndexedCollection<T>
        {
            private readonly IIndexedCollection<T> _wrappedList;
            private readonly int _start;
            private readonly int _count;

            public IndexedCollectionRange(IIndexedCollection<T> wrappedList, int start, int count)
            {
                if (wrappedList == null)
                    throw new ArgumentOutOfRangeException("wrappedList");
                if ((start < 0) || (start > wrappedList.Count) || ((start == wrappedList.Count) && (count != 0)))
                    throw new ArgumentOutOfRangeException("start");
                if ((count < 0) || (count > wrappedList.Count) || ((count + start) > wrappedList.Count))
                    throw new ArgumentOutOfRangeException("count");

                _wrappedList = wrappedList;
                _start = start;
                _count = count;
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (var i = _start; i < _start + _count; i++)
                    yield return _wrappedList[i];
            }

            public int Count
            {
                get { return _count; }
            }

            public T this[int index]
            {
                get { return _wrappedList[_start + index]; }
            }

            public bool Contains(T value)
            {
                return _wrappedList.Contains(value);
            }

            public int IndexOf(T value)
            {
                return (_wrappedList.IndexOf(value) - _start);
            }

            public void CopyTo([NotNull] T[] array, int destinationIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (destinationIndex < 0)
                    throw new ArgumentOutOfRangeException("destinationIndex", "Index must be greater than or equal to zero.");
                if ((array.Length - destinationIndex) > Count)
                    throw new ArgumentOutOfRangeException("destinationIndex", "Target array is not large enough.");

                for (var i = 0; i < _count; i++)
                    array[destinationIndex + i] = _wrappedList[_start + i];
            }

            #region Implementation of IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

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

            public override int Count
            {
                get { return Math.Min(_count, _wrappedList.Count - _start); }
            }

            public override void Clear()
            {
                if (_wrappedList.Count - _start < _count)
                    _count = _wrappedList.Count - _start;

                while (_count > 0)
                {
                    _wrappedList.RemoveAt(_start + _count - 1);
                    --_count;
                }
            }

            public override void Insert(int index, T item)
            {
                if (index < 0 || index > _count)
                    throw new ArgumentOutOfRangeException("index");

                _wrappedList.Insert(_start + index, item);
                ++_count;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException("index");

                _wrappedList.RemoveAt(_start + index);
                --_count;
            }

            public override bool Remove(T item)
            {
                if (_wrappedList.IsReadOnly)
                    throw new NotSupportedException("Cannot modify read-only collection.");

                return base.Remove(item);
            }

            public override T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    return _wrappedList[_start + index];
                }
                set
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    _wrappedList[_start + index] = value;
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get { return _wrappedList.IsReadOnly; }
            }
        }

        public static IList<T> Range<T>([NotNull] this IList<T> list, int start, int count)
        {
            if (list == null)
                throw new ArgumentOutOfRangeException("list");
            if ((start < 0) || (start > list.Count) || ((start == list.Count) && (count != 0)))
                throw new ArgumentOutOfRangeException("start");
            if ((count < 0) || (count > list.Count) || ((count + start) > list.Count))
                throw new ArgumentOutOfRangeException("count");

            return new ListRange<T>(list, start, count);
        }

        public static IIndexedCollection<T> Range<T>([NotNull] this IIndexedCollection<T> list, int start, int count)
        {
            if (list == null)
                throw new ArgumentOutOfRangeException("list");
            if ((start < 0) || (start > list.Count) || ((start == list.Count) && (count != 0)))
                throw new ArgumentOutOfRangeException("start");
            if ((count < 0) || (count > list.Count) || ((count + start) > list.Count))
                throw new ArgumentOutOfRangeException("count");

            return new IndexedCollectionRange<T>(list, start, count);
        }

        [Serializable]
        private sealed class ArrayRange<T> : SimpleListBase<T>
        {
            private readonly T[] _wrappedArray;
            private readonly int _start;
            private int _count;

            public ArrayRange(T[] wrappedArray, int start, int count)
            {
                _wrappedArray = wrappedArray;
                _start = start;
                _count = count;
            }

            public override int Count
            {
                get { return _count; }
            }

            public override void Clear()
            {
                Array.Copy(
                    _wrappedArray, _start + _count, _wrappedArray, _start, _wrappedArray.Length - (_start + _count));
                InitializeRange(_wrappedArray, _wrappedArray.Length - _count, _count, default(T));
                _count = 0;
            }

            public override void Insert(int index, T item)
            {
                if (index < 0 || index > _count)
                    throw new ArgumentOutOfRangeException("index");

                var i = _start + index;

                if (i + 1 < _wrappedArray.Length)
                    Array.Copy(_wrappedArray, i, _wrappedArray, i + 1, _wrappedArray.Length - i - 1);
                if (i < _wrappedArray.Length)
                    _wrappedArray[i] = item;

                if (_start + _count < _wrappedArray.Length)
                    ++_count;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException("index");

                var i = _start + index;

                if (i < _wrappedArray.Length - 1)
                    Array.Copy(_wrappedArray, i + 1, _wrappedArray, i, _wrappedArray.Length - i - 1);
                _wrappedArray[_wrappedArray.Length - 1] = default(T);

                --_count;
            }

            public override T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    return _wrappedArray[_start + index];
                }
                set
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    _wrappedArray[_start + index] = value;
                }
            }
        }

        public static IList<T> Range<T>([NotNull] this T[] array, int start, int count)
        {
            if (array == null)
                throw new ArgumentOutOfRangeException("array");
            if (start < 0 || start > array.Length || (start == array.Length && count != 0))
                throw new ArgumentOutOfRangeException("start");
            if (count < 0 || count > array.Length || count + start > array.Length)
                throw new ArgumentOutOfRangeException("count");

            return new ArrayRange<T>(array, start, count);
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

            public int Count
            {
                get { return _wrappedCollection.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

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
                throw new ArgumentNullException("collection");
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

            public int Count
            {
                get { return _wrappedList.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public T this[int index]
            {
                get { return _wrappedList[index]; }
                // ReSharper disable ValueParameterNotUsed
                set { MethodModifiesCollection(); }
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
                if (list == null)
                    throw new ArgumentNullException("list");
                _list = list;
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

            public int Count
            {
                get { return _list.Count; }
            }

            public T this[int index]
            {
                get { return _list[index]; }
            }

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
            if (list == null)
                return null;
            if (list.IsReadOnly)
                return list;
            return new ReadOnlyList<T>(list);
        }

        [Serializable]
        private sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            // The dictionary that is wrapped
            private readonly IDictionary<TKey, TValue> _wrappedDictionary;

            public ReadOnlyDictionary([NotNull] IDictionary<TKey, TValue> wrappedDictionary)
            {
                if (wrappedDictionary == null)
                    throw new ArgumentNullException("wrappedDictionary");

                _wrappedDictionary = wrappedDictionary;
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

            public ICollection<TKey> Keys
            {
                get { return AsReadOnly(_wrappedDictionary.Keys); }
            }

            public ICollection<TValue> Values
            {
                get { return AsReadOnly(_wrappedDictionary.Values); }
            }

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
                get { return _wrappedDictionary[key]; }
                // ReSharper disable ValueParameterNotUsed
                set { MethodModifiesCollection(); }
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

            public int Count
            {
                get { return _wrappedDictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

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
            if (dictionary == null)
                return null;
            if (dictionary.IsReadOnly)
                return dictionary;
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        [Serializable]
        private sealed class TypedEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator _wrappedEnumerator;

            public TypedEnumerator(IEnumerator wrappedEnumerator)
            {
                _wrappedEnumerator = wrappedEnumerator;
            }

            T IEnumerator<T>.Current
            {
                get { return (T)_wrappedEnumerator.Current; }
            }

            void IDisposable.Dispose()
            {
                if (_wrappedEnumerator is IDisposable)
                    ((IDisposable)_wrappedEnumerator).Dispose();
            }

            object IEnumerator.Current
            {
                get { return _wrappedEnumerator.Current; }
            }

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
            if (untypedCollection == null)
                return null;
            if (untypedCollection is IEnumerable<T>)
                return (IEnumerable<T>)untypedCollection;
            return new TypedEnumerable<T>(untypedCollection);
        }

        [Serializable]
        private sealed class TypedCollection<T> : ICollection<T>
        {
            private readonly ICollection _wrappedCollection;

            public TypedCollection(ICollection wrappedCollection)
            {
                _wrappedCollection = wrappedCollection;
            }

            private static void MethodModifiesCollection()
            {
                throw new NotSupportedException("Cannot modify strongly-typed collection.");
            }

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

            public bool Contains(T item)
            {
                IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
                foreach (var obj in _wrappedCollection)
                {
                    if (obj is T && equalityComparer.Equals(item, (T)obj))
                        return true;
                }
                return false;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _wrappedCollection.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _wrappedCollection.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new TypedEnumerator<T>(_wrappedCollection.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _wrappedCollection.GetEnumerator();
            }
        }

        [CanBeNull]
        public static ICollection<T> TypedAs<T>([CanBeNull] this ICollection untypedCollection)
        {
            if (untypedCollection == null)
                return null;
            if (untypedCollection is ICollection<T>)
                return (ICollection<T>)untypedCollection;
            return new TypedCollection<T>(untypedCollection);
        }

        [Serializable]
        private sealed class TypedIndexedCollection<TSource, TTarget> : IIndexedCollection<TTarget>
        {
            private readonly IIndexedCollection<TSource> _baseCollection;
            private readonly Func<TSource, TTarget> _conversionCallback;
            private readonly Func<TTarget, TSource> _reverseConversionCallback;

            // ReSharper disable UnusedMember.Local
            public TypedIndexedCollection([NotNull] IIndexedCollection<TSource> baseCollection)
                : this(
                baseCollection,
                o => (TTarget)Convert.ChangeType(o, typeof(TTarget)),
                o => (TSource)Convert.ChangeType(o, typeof(TSource))) { }
            // ReSharper restore UnusedMember.Local

            public TypedIndexedCollection(
                [NotNull] IIndexedCollection<TSource> baseCollection,
                [NotNull] Func<TSource, TTarget> conversionCallback,
                [NotNull] Func<TTarget, TSource> reverseConversionCallback)
            {
                if (baseCollection == null)
                    throw new ArgumentNullException("baseCollection");
                if (conversionCallback == null)
                    throw new ArgumentNullException("conversionCallback");
                if (reverseConversionCallback == null)
                    throw new ArgumentNullException("reverseConversionCallback");

                _baseCollection = baseCollection;
                _conversionCallback = conversionCallback;
                _reverseConversionCallback = reverseConversionCallback;
            }

            public IEnumerator<TTarget> GetEnumerator()
            {
                return _baseCollection.Select(o => _conversionCallback(o)).GetEnumerator();
            }

            public int Count
            {
                get { return _baseCollection.Count; }
            }

            public TTarget this[int index]
            {
                get { return _conversionCallback(_baseCollection[index]); }
            }

            public bool Contains(TTarget value)
            {
                return _baseCollection.Contains(_reverseConversionCallback(value));
            }

            public int IndexOf(TTarget value)
            {
                return _baseCollection.IndexOf(_reverseConversionCallback(value));
            }

            public void CopyTo(TTarget[] array, int destinationIndex)
            {
                _baseCollection.Select(o => _conversionCallback(o)).CopyTo(array, destinationIndex);
            }

            #region Implementation of IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        [Serializable]
        private sealed class TypedList<T> : IList<T>
        {
            private readonly IList _wrappedList;

            public TypedList(IList wrappedList)
            {
                _wrappedList = wrappedList;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new TypedEnumerator<T>(_wrappedList.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _wrappedList.GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return _wrappedList.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _wrappedList.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _wrappedList.RemoveAt(index);
            }

            public void Add(T item)
            {
                _wrappedList.Add(item);
            }

            public void Clear()
            {
                _wrappedList.Clear();
            }

            public bool Contains(T item)
            {
                return _wrappedList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _wrappedList.CopyTo(array, arrayIndex);
            }

            public T this[int index]
            {
                get { return (T)_wrappedList[index]; }
                set { _wrappedList[index] = value; }
            }

            public int Count
            {
                get { return _wrappedList.Count; }
            }

            public bool IsReadOnly
            {
                get { return _wrappedList.IsReadOnly; }
            }

            public bool Remove(T item)
            {
                if (!_wrappedList.Contains(item))
                    return false;
                _wrappedList.Remove(item);
                return true;
            }
        }

        [CanBeNull]
        public static IList<T> TypedAs<T>([CanBeNull] this IList untypedList)
        {
            if (untypedList == null)
                return null;
            if (untypedList is IList<T>)
                return (IList<T>)untypedList;
            return new TypedList<T>(untypedList);
        }

        public static IIndexedCollection<TTarget> TypedAs<TSource, TTarget>([NotNull] this IIndexedCollection<TSource> list)
            where TTarget : TSource
        {
            if (list == null)
                return null;
            if (list is IIndexedCollection<TTarget>)
                return (IIndexedCollection<TTarget>)list;

            return new TypedIndexedCollection<TSource, TTarget>(
                list,
                o => (TTarget)o,
                o => (TSource)o);
        }

        public static IIndexedCollection<TTarget> TypedAs<TSource, TTarget>(
            [NotNull] this IIndexedCollection<TSource> list,
            [NotNull] Func<TSource, TTarget> conversionCallback,
            [NotNull] Func<TTarget, TSource> reverseConversionCallback)
            where TTarget : TSource
        {
            if (list == null)
                return null;
            if (conversionCallback == null)
                throw new ArgumentNullException("conversionCallback");
            if (reverseConversionCallback == null)
                throw new ArgumentNullException("reverseConversionCallback");
            if (list is IIndexedCollection<TTarget>)
                return (IIndexedCollection<TTarget>)list;

            return new TypedIndexedCollection<TSource, TTarget>(
                list,
                conversionCallback,
                reverseConversionCallback);
        }

        [Serializable]
        private sealed class UntypedCollection<T> : ICollection
        {
            private readonly ICollection<T> _wrappedCollection;

            public UntypedCollection(ICollection<T> wrappedCollection)
            {
                _wrappedCollection = wrappedCollection;
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException("array");

                var i = 0;
                var count = _wrappedCollection.Count;

                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", index, "Value must be non-negative.");
                if (index >= array.Length || count > array.Length - index)
                    throw new ArgumentException("Argument is too small.", "index");

                foreach (var item in _wrappedCollection)
                {
                    if (i >= count)
                        break;

                    array.SetValue(item, index);
                    ++index;
                    ++i;
                }
            }

            public int Count
            {
                get { return _wrappedCollection.Count; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return this; }
            }

            public IEnumerator GetEnumerator()
            {
                return ((IEnumerable)_wrappedCollection).GetEnumerator();
            }
        }

        [CanBeNull]
        public static ICollection Untyped<T>([CanBeNull] this ICollection<T> typedCollection)
        {
            if (typedCollection == null)
                return null;
            if (typedCollection is ICollection)
                return (ICollection)typedCollection;
            return new UntypedCollection<T>(typedCollection);
        }

        [Serializable]
        private sealed class UntypedList<T> : IList
        {
            private readonly IList<T> _wrappedList;

            public UntypedList(IList<T> wrappedList)
            {
                _wrappedList = wrappedList;
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
                            "Cannot convert value to type {1}: {0}",
                            value,
                            typeof(T)),
                        name);
                }
            }

            public int Add(object value)
            {
                // We assume that Add always adds to the end. Is this true?
                _wrappedList.Add(ConvertToItemType("value", value));
                return _wrappedList.Count - 1;
            }

            public void Clear()
            {
                _wrappedList.Clear();
            }

            public bool Contains(object value)
            {
                if (value is T)
                    return _wrappedList.Contains((T)value);
                return false;
            }

            public int IndexOf(object value)
            {
                if (value is T)
                    return _wrappedList.IndexOf((T)value);
                return -1;
            }

            public void Insert(int index, object value)
            {
                _wrappedList.Insert(index, ConvertToItemType("value", value));
            }

            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return _wrappedList.IsReadOnly; }
            }

            public void Remove(object value)
            {
                if (value is T)
                    _wrappedList.Remove((T)value);
            }

            public void RemoveAt(int index)
            {
                _wrappedList.RemoveAt(index);
            }

            public object this[int index]
            {
                get { return _wrappedList[index]; }
                set { _wrappedList[index] = ConvertToItemType("value", value); }
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException("array");

                var i = 0;
                var count = _wrappedList.Count;

                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", index, "Value must be non-negative.");
                if (index >= array.Length || count > array.Length - index)
                    throw new ArgumentOutOfRangeException("index", "Value is too small.");

                foreach (var item in _wrappedList)
                {
                    if (i >= count)
                        break;

                    array.SetValue(item, index);
                    ++index;
                    ++i;
                }
            }

            public int Count
            {
                get { return _wrappedList.Count; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return this; }
            }

            public IEnumerator GetEnumerator()
            {
                return ((IEnumerable)_wrappedList).GetEnumerator();
            }
        }

        [CanBeNull]
        public static IList Untyped<T>([CanBeNull] this IList<T> typedList)
        {
            if (typedList == null)
                return null;
            if (typedList is IList)
                return (IList)typedList;
            return new UntypedList<T>(typedList);
        }

        [Serializable]
        private sealed class ArrayWrapper<T> : SimpleListBase<T>, IList
        {
            private readonly T[] _wrappedArray;

            public ArrayWrapper(T[] wrappedArray)
            {
                _wrappedArray = wrappedArray;
            }

            public override int Count
            {
                get { return _wrappedArray.Length; }
            }

            public override void Clear()
            {
                var count = _wrappedArray.Length;
                for (var i = 0; i < count; ++i)
                    _wrappedArray[i] = default(T);
            }

            public override void Insert(int index, T item)
            {
                if (index < 0 || index > _wrappedArray.Length)
                    throw new ArgumentOutOfRangeException("index");

                if (index + 1 < _wrappedArray.Length)
                    Array.Copy(_wrappedArray, index, _wrappedArray, index + 1, _wrappedArray.Length - index - 1);
                if (index < _wrappedArray.Length)
                    _wrappedArray[index] = item;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= _wrappedArray.Length)
                    throw new ArgumentOutOfRangeException("index");

                if (index < _wrappedArray.Length - 1)
                    Array.Copy(_wrappedArray, index + 1, _wrappedArray, index, _wrappedArray.Length - index - 1);
                _wrappedArray[_wrappedArray.Length - 1] = default(T);
            }

            public override T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _wrappedArray.Length)
                        throw new ArgumentOutOfRangeException("index");

                    return _wrappedArray[index];
                }
                set
                {
                    if (index < 0 || index >= _wrappedArray.Length)
                        throw new ArgumentOutOfRangeException("index");

                    _wrappedArray[index] = value;
                }
            }

            public override void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (array.Length < _wrappedArray.Length)
                    throw new ArgumentException("array is too short", "array");
                if (arrayIndex < 0 || arrayIndex >= array.Length)
                    throw new ArgumentOutOfRangeException("arrayIndex");
                if (array.Length + arrayIndex < _wrappedArray.Length)
                    throw new ArgumentOutOfRangeException("arrayIndex");

                Array.Copy(_wrappedArray, 0, array, arrayIndex, _wrappedArray.Length);
            }

            public override IEnumerator<T> GetEnumerator()
            {
                return ((IList<T>)_wrappedArray).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList)_wrappedArray).GetEnumerator();
            }

            bool IList.IsFixedSize
            {
                get { return true; }
            }
        }

        public static IList<T> AsWritableList<T>([NotNull] this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            return new ArrayWrapper<T>(array);
        }
        #endregion CollectionBase wrappers

        #region Replacing
        public static IEnumerable<T> Replace<T>(
            this IEnumerable<T> collection,
            T targetItem,
            T replacementItem)
        {
            return Replace(
                collection,
                targetItem,
                replacementItem,
                EqualityComparer<T>.Default);
        }

        public static IEnumerable<T> Replace<T>(
            this IEnumerable<T> collection,
            T targetItem,
            T replacementItem,
            IEqualityComparer<T> equalityComparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            foreach (var item in collection)
            {
                if (equalityComparer.Equals(item, targetItem))
                    yield return replacementItem;
                else
                    yield return item;
            }
        }

        public static IEnumerable<T> Replace<T>(
            this IEnumerable<T> collection,
            Func<T, bool> predicate,
            T replacementItem)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            foreach (var item in collection)
            {
                if (predicate(item))
                    yield return replacementItem;
                else
                    yield return item;
            }
        }

        public static void ReplaceInPlace<T>([NotNull] this IList<T> list, T itemFind, T replaceWith)
        {
            ReplaceInPlace(list, itemFind, replaceWith, EqualityComparer<T>.Default);
        }

        public static void ReplaceInPlace<T>(
            this IList<T> list,
            T targetItem,
            T replacementItem,
            IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");
            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var listCount = list.Count;
            for (var index = 0; index < listCount; ++index)
            {
                if (equalityComparer.Equals(list[index], targetItem))
                    list[index] = replacementItem;
            }
        }

        public static void ReplaceInPlace<T>(
            this IList<T> list,
            Func<T, bool> predicate,
            T replacementItem)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var listCount = list.Count;
            for (var index = 0; index < listCount; ++index)
            {
                if (predicate(list[index]))
                    list[index] = replacementItem;
            }
        }
        #endregion Replacing

        #region Consecutive items
        public static void RemoveRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (items == null)
                throw new ArgumentNullException("items");

            foreach (var item in items)
                collection.Remove(item);
        }

        public static void RemoveRange<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] IEnumerable<TKey> keys)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            if (keys == null)
                throw new ArgumentNullException("keys");
            foreach (var key in keys)
                dictionary.Remove(key);
        }

        public static IList<T> RemoveDuplicatesInPlace<T>([NotNull] this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            return RemoveDuplicatesInPlace(list, EqualityComparer<T>.Default);
        }

        public static IList<T> RemoveDuplicatesInPlace<T>([NotNull] this IList<T> list, [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return RemoveDuplicatesInPlace(list, equalityComparer.Equals);
        }

        public static IList<T> RemoveDuplicatesInPlace<T>([NotNull] this IList<T> list, [NotNull] Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var current = default(T);
            int i = -1, j = 0;
            var listCount = list.Count;

            // Remove duplicates, compressing items to lower in the list.
            while (j < listCount)
            {
                var item = list[j];
                if (i < 0 || !predicate(current, item))
                {
                    current = item;
                    ++i;
                    if (i != j)
                        list[i] = current;
                }
                ++j;
            }

            ++i;

            if (i >= listCount)
                return list;

            // remove items from the end.
            if (list is ArrayWrapper<T> || (list is IList && ((IList)list).IsFixedSize))
            {
                // An array or similar. Null out the last elements.
                while (i < listCount)
                    list[i++] = default(T);
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

            return list;
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IList<T> list, int count)
        {
            return FirstConsecutiveEqual(list, count, EqualityComparer<T>.Default);
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IList<T> list, int count, [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return FirstConsecutiveEqual(list, count, equalityComparer.Equals);
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IList<T> list, int count, [NotNull] Func<T, T, bool> predicate)
        {
            return FirstConsecutiveEqual(new ListAsIndexedCollection<T>(list), count, predicate);
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IIndexedEnumerable<T> list, int count)
        {
            return FirstConsecutiveEqual(list, count, EqualityComparer<T>.Default);
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IIndexedEnumerable<T> list, int count, [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return FirstConsecutiveEqual(list, count, equalityComparer.Equals);
        }

        public static int FirstConsecutiveEqual<T>([NotNull] this IIndexedEnumerable<T> list, int count, [NotNull] Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            var listCount = list.Count;
            if (listCount < count)
                return -1; // Can't find run longer than the list itself.
            if (count == 1)
                return 0; // Run of 1 must be the first item in the list.

            int start = 0, index = 0;
            var current = default(T);
            var runLength = 0;

            // Go through the list, looking for a run of the given length.
            foreach (var item in list)
            {
                if (index > 0 && predicate(current, item))
                {
                    ++runLength;
                    if (runLength >= count)
                        return start;
                }
                else
                {
                    current = item;
                    start = index;
                    runLength = 1;
                }

                ++index;
            }

            return -1;
        }

        public static int FirstConsecutiveWhere<T>([NotNull] this IList<T> list, int count, [NotNull] Func<T, bool> predicate)
        {
            return FirstConsecutiveWhere(new ListAsIndexedCollection<T>(list), count, predicate);
        }

        public static int FirstConsecutiveWhere<T>([NotNull] this IIndexedEnumerable<T> list, int count, [NotNull] Func<T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            var listCount = list.Count;
            if (count > listCount)
                return -1; // Can't find run longer than the list itself.

            int index = 0, start = -1;
            var runLength = 0;

            // Scan the list in order, looking for the number of consecutive true items.
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    if (start < 0)
                        start = index;
                    ++runLength;
                    if (runLength >= count)
                        return start;
                }
                else
                {
                    runLength = 0;
                    start = -1;
                }

                ++index;
            }

            return -1;
        }
        #endregion Consecutive items

        #region Find and IndexOfSubsequence
        public static int CountWhere<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var count = 0;

            foreach (var item in collection)
            {
                if (predicate(item))
                    ++count;
            }

            return count;
        }

        public static bool RemoveFirstWhere<T>([NotNull] this IList<T> list, [NotNull] Func<T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var index = list.FirstIndexWhere(predicate);
            if (index < 0)
                return false;

            list.RemoveAt(index);

            return true;
        }

        public static ICollection<T> RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (collection is T[])
                collection = new ArrayWrapper<T>((T[])collection);
            if (collection.IsReadOnly)
                throw new ArgumentException("List is read-only.", "collection");

            var list = collection as IList<T>;
            if (list != null)
            {
                int i = -1, j = 0;
                var listCount = list.Count;
                var removed = new List<T>();

                // Remove item where predicate is true, compressing items to lower in the list. This is much more
                // efficient than the naive algorithm that uses IList<T>.Remove().
                while (j < listCount)
                {
                    var item = list[j];
                    if (predicate(item))
                    {
                        removed.Add(item);
                    }
                    else
                    {
                        ++i;
                        if (i != j)
                            list[i] = item;
                    }
                    ++j;
                }

                ++i;
                if (i < listCount)
                {
                    // remove items from the end.
                    if (list is IList && ((IList)list).IsFixedSize)
                    {
                        // An array or similar. Null out the last elements.
                        while (i < listCount)
                            list[i++] = default(T);
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
                var removed = new List<T>();

                foreach (var item in collection)
                    if (predicate(item))
                        removed.Add(item);

                foreach (var item in removed)
                    collection.Remove(item);

                return removed;
            }
        }

        public static bool TryFindFirstItem<T>(this IEnumerable<T> collection, Func<T, bool> condition, out T foundItem)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (condition == null)
                throw new ArgumentNullException("condition");

            foreach (var item in collection.Where(condition))
            {
                foundItem = item;
                return true;
            }

            foundItem = default(T);
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
                throw new ArgumentNullException("collection");
            if (condition == null)
                throw new ArgumentNullException("condition");

            foundItem = default(T);

            var list = collection as IList<T>;
            if (list == null)
            {
                const int listConstructionThreshold = 8192;

                var trueCollection = collection as ICollection<T>;
                if ((trueCollection != null) && (trueCollection.Count <= listConstructionThreshold))
                {
                    list = new T[trueCollection.Count];
                    collection.CopyTo(list);
                }
            }
            if (list != null)
            {
                for (var index = list.Count - 1; index >= 0; --index)
                {
                    var item = list[index];
                    if (!condition(item))
                        continue;
                    foundItem = item;
                    return true;
                }

                foundItem = default(T);
                return false;
            }

            var indexedEnumerable = collection as IIndexedEnumerable<T>;
            if (indexedEnumerable != null)
            {
                for (var index = indexedEnumerable.Count - 1; index >= 0; --index)
                {
                    var item = indexedEnumerable[index];
                    if (!condition(item))
                        continue;
                    foundItem = item;
                    return true;
                }

                foundItem = default(T);
                return false;
            }

            var linkedList = new LinkedList<T>(collection);
            for (var currentNode = linkedList.Last; currentNode.Previous != null; currentNode = currentNode.Previous)
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
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var index = 0;
            foreach (var item in list)
            {
                if (predicate(item))
                    return index;
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
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return LastIndexWhere(new ListAsIndexedCollection<T>(list), predicate);
        }

        public static int LastIndexWhere<T>(this IIndexedEnumerable<T> list, Func<T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            for (var index = list.Count - 1; index >= 0; --index)
            {
                if (predicate(list[index]))
                    return index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static IEnumerable<int> IndicesWhere<T>(this IIndexedEnumerable<T> list, Func<T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var index = 0;
            foreach (var item in list)
            {
                if (predicate(item))
                    yield return index;
                ++index;
            }
        }

        public static IEnumerable<int> IndicesWhere<T>(this IList<T> list, Func<T, bool> predicate)
        {
            return IndicesWhere(new ListAsIndexedCollection<T>(list), predicate);
        }

        public static int FirstIndexOf<T>(this IList<T> list, T item)
        {
            return FirstIndexOf(list, item, EqualityComparer<T>.Default);
        }

        public static int FirstIndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            return FirstIndexOf(new ListAsIndexedCollection<T>(list), item, equalityComparer);
        }

        public static int FirstIndexOf<T>(this IIndexedEnumerable<T> list, T item)
        {
            return FirstIndexOf(list, item, EqualityComparer<T>.Default);
        }

        public static int FirstIndexOf<T>(this IIndexedEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            var index = 0;
            foreach (var x in list)
            {
                if (equalityComparer.Equals(x, item))
                    return index;
                ++index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int LastIndexOf<T>(this IList<T> list, T item)
        {
            return LastIndexOf(list, item, EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            return LastIndexOf(new ListAsIndexedCollection<T>(list), item, equalityComparer);
        }

        public static int LastIndexOf<T>(this IIndexedEnumerable<T> list, T item)
        {
            return LastIndexOf(list, item, EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IIndexedEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            for (var index = list.Count - 1; index >= 0; --index)
            {
                if (equalityComparer.Equals(list[index], item))
                    return index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static IEnumerable<int> IndicesOf<T>(this IList<T> list, T item)
        {
            return IndicesOf(list, item, EqualityComparer<T>.Default);
        }

        public static IEnumerable<int> IndicesOf<T>(this IList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            return IndicesOf(new ListAsIndexedCollection<T>(list), item, equalityComparer);
        }

        public static IEnumerable<int> IndicesOf<T>(this IIndexedEnumerable<T> list, T item)
        {
            return IndicesOf(list, item, EqualityComparer<T>.Default);
        }

        public static IEnumerable<int> IndicesOf<T>(this IIndexedEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            var index = 0;
            foreach (var x in list)
            {
                if (equalityComparer.Equals(x, item))
                    yield return index;
                ++index;
            }
        }

        public static int FirstIndexOfSequence<T>(this IList<T> list, IEnumerable<T> sequence)
        {
            return FirstIndexOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static int FirstIndexOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return FirstIndexOfSequence(new ListAsIndexedCollection<T>(list), sequence, equalityComparer);
        }

        public static int FirstIndexOfSequence<T>(this IIndexedEnumerable<T> list, IEnumerable<T> sequence)
        {
            return FirstIndexOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static int FirstIndexOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            // Create a set of the items we are looking for, for efficient lookup.
            var setToLookFor = new HashSet<T>(sequence, equalityComparer);

            // Scan the list for the items.
            var index = 0;
            foreach (var x in list)
            {
                if (setToLookFor.Contains(x))
                    return index;
                ++index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int FirstIndexOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return FirstIndexOfSequence(new ListAsIndexedCollection<T>(list), sequence, predicate);
        }

        public static int FirstIndexOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            // Scan the list for the items.
            var index = 0;
            foreach (var x in list)
            {
                foreach (var y in sequence)
                {
                    if (predicate(x, y))
                        return index;
                }

                ++index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int LastIndexOfSequence<T>(this IList<T> list, IEnumerable<T> sequence)
        {
            return LastIndexOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static int LastIndexOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            return LastIndexOfSequence(new ListAsIndexedCollection<T>(list), sequence, EqualityComparer<T>.Default);
        }

        public static int LastIndexOfSequence<T>(this IIndexedEnumerable<T> list, IEnumerable<T> sequence)
        {
            return LastIndexOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static int LastIndexOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            // Create a set of the items we are looking for, for efficient lookup.
            var setToLookFor = new HashSet<T>(sequence, equalityComparer);

            // Scan the list
            for (var index = list.Count - 1; index >= 0; --index)
            {
                if (setToLookFor.Contains(list[index]))
                    return index;
            }

            // didn't find any item that matches.
            return -1;
        }

        public static int LastIndexOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            return LastIndexOfSequence(new ListAsIndexedCollection<T>(list), sequence, predicate);
        }

        public static int LastIndexOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            // Scan the list
            for (var index = list.Count - 1; index >= 0; --index)
            {
                foreach (var y in sequence)
                {
                    if (predicate(list[index], y))
                    {
                        return index;
                    }
                }
            }

            // didn't find any item that matches.
            return -1;
        }

        public static IEnumerable<int> IndicesOfSequence<T>(this IList<T> list, IEnumerable<T> sequence)
        {
            return IndicesOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static IEnumerable<int> IndicesOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            return IndicesOfSequence(new ListAsIndexedCollection<T>(list), sequence, equalityComparer);
        }

        public static IEnumerable<int> IndicesOfSequence<T>(
            this IList<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            return IndicesOfSequence(new ListAsIndexedCollection<T>(list), sequence, predicate);
        }

        public static IEnumerable<int> IndicesOfSequence<T>(this IIndexedEnumerable<T> list, IEnumerable<T> sequence)
        {
            return IndicesOfSequence(list, sequence, EqualityComparer<T>.Default);
        }

        public static IEnumerable<int> IndicesOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            IEqualityComparer<T> equalityComparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            // Create a set of the items we are looking for, for efficient lookup.
            var setToLookFor = new HashSet<T>(sequence, equalityComparer);

            // Scan the list
            var index = 0;
            foreach (var x in list)
            {
                if (setToLookFor.Contains(x))
                    yield return index;
                ++index;
            }
        }

        public static IEnumerable<int> IndicesOfSequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> sequence,
            Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (sequence == null)
                throw new ArgumentNullException("sequence");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            // Scan the list for the items.
            var index = 0;
            foreach (var x in list)
            {
                foreach (var y in sequence)
                {
                    if (predicate(x, y))
                        yield return index;
                }

                ++index;
            }
        }

        public static int IndexOfSubsequence<T>(this IList<T> list, IEnumerable<T> pattern)
        {
            return IndexOfSubsequence(list, 0, pattern);
        }

        public static int IndexOfSubsequence<T>(this IList<T> list, int startIndex, IEnumerable<T> pattern)
        {
            return IndexOfSubsequence(list, pattern, EqualityComparer<T>.Default);
        }

        public static int IndexOfSubsequence<T>(this IList<T> list, IEnumerable<T> pattern, Func<T, T, bool> predicate)
        {
            return IndexOfSubsequence(list, 0, pattern, predicate);
        }

        public static int IndexOfSubsequence<T>(
            this IList<T> list, int startIndex, IEnumerable<T> pattern, Func<T, T, bool> predicate)
        {
            return IndexOfSubsequence(new ListAsIndexedCollection<T>(list), startIndex, pattern);
        }

        public static int IndexOfSubsequence<T>(
            this IList<T> list,
            IEnumerable<T> pattern,
            IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return IndexOfSubsequence(list, pattern, equalityComparer.Equals);
        }

        public static int IndexOfSubsequence<T>(this IIndexedEnumerable<T> list, IEnumerable<T> pattern)
        {
            return IndexOfSubsequence(list, 0, pattern);
        }

        public static int IndexOfSubsequence<T>(this IIndexedEnumerable<T> list, int startIndex, IEnumerable<T> pattern)
        {
            return IndexOfSubsequence(list, pattern, EqualityComparer<T>.Default);
        }

        public static int IndexOfSubsequence<T>(this IIndexedEnumerable<T> list, IEnumerable<T> pattern, Func<T, T, bool> predicate)
        {
            return IndexOfSubsequence(list, 0, pattern, predicate);
        }

        public static int IndexOfSubsequence<T>(
            this IIndexedEnumerable<T> list, int startIndex, IEnumerable<T> pattern, Func<T, T, bool> predicate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            // Put the pattern into an array for performance (don't keep allocating enumerators).
            var patternArray = pattern.ToArray();

            var listCount = list.Count;
            var patternCount = patternArray.Length;

            if (patternCount == 0)
                return 0; // A zero-length pattern occurs anywhere.

            if (listCount == 0)
                return -1; // No room for a pattern;

            for (var start = startIndex; start <= listCount - patternCount; ++start)
            {
                for (var count = 0; count < patternCount; ++count)
                {
                    if (!predicate(list[start + count], patternArray[count]))
                        goto NoMatch;
                }

                // Got through the whole pattern; we have a match.
                return start;

            NoMatch:
                // No match found at start.
                ;
            }

            // No match found anywhere.
            return -1;
        }

        public static int IndexOfSubsequence<T>(
            this IIndexedEnumerable<T> list,
            IEnumerable<T> pattern,
            IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            return IndexOfSubsequence(list, pattern, equalityComparer.Equals);
        }
        #endregion Find and IndexOfSubsequence

        #region Set operations (coded except EqualSets)
        public static bool IsSubsetOf<T>([NotNull] this IEnumerable<T> firstCollection, [NotNull] IEnumerable<T> secondCollection)
        {
            return IsSubsetOf(firstCollection, secondCollection, EqualityComparer<T>.Default);
        }

        public static bool IsSubsetOf<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection,
            [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (firstCollection == null)
                throw new ArgumentNullException("firstCollection");
            if (secondCollection == null)
                throw new ArgumentNullException("secondCollection");
            if (equalityComparer == null)
                throw new ArgumentException("equalityComparer");

            var firstSet = firstCollection as HashSet<T>;
            if ((firstSet == null) || (firstSet.Comparer != equalityComparer))
                firstSet = new HashSet<T>(firstCollection, equalityComparer);

            var secondSet = secondCollection as HashSet<T>;
            if ((secondSet == null) || (secondSet.Comparer != equalityComparer))
                secondSet = new HashSet<T>(secondCollection, equalityComparer);

            return firstSet.IsSubsetOf(secondSet);
        }

        public static bool IsProperSubsetOf<T>([NotNull] this IEnumerable<T> firstCollection, [NotNull] IEnumerable<T> secondCollection)
        {
            return IsProperSubsetOf(firstCollection, secondCollection, EqualityComparer<T>.Default);
        }

        public static bool IsProperSubsetOf<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection,
            [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (firstCollection == null)
                throw new ArgumentNullException("firstCollection");
            if (secondCollection == null)
                throw new ArgumentNullException("secondCollection");
            if (equalityComparer == null)
                throw new ArgumentException("equalityComparer");

            var firstSet = firstCollection as HashSet<T>;
            if ((firstSet == null) || (firstSet.Comparer != equalityComparer))
                firstSet = new HashSet<T>(firstCollection, equalityComparer);

            var secondSet = secondCollection as HashSet<T>;
            if ((secondSet == null) || (secondSet.Comparer != equalityComparer))
                secondSet = new HashSet<T>(secondCollection, equalityComparer);

            return firstSet.IsProperSupersetOf(secondSet);
        }

        public static bool IsDisjointSetWith<T>([NotNull] this IEnumerable<T> firstCollection, [NotNull] IEnumerable<T> secondCollection)
        {
            return IsDisjointSetWith(firstCollection, secondCollection, EqualityComparer<T>.Default);
        }

        public static bool IsDisjointSetWith<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection,
            [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (firstCollection == null)
                throw new ArgumentNullException("firstCollection");
            if (secondCollection == null)
                throw new ArgumentNullException("secondCollection");
            if (equalityComparer == null)
                throw new ArgumentException("equalityComparer");

            var set1 = new HashSet<T>(firstCollection, equalityComparer);

            foreach (var item2 in secondCollection)
            {
                if (set1.Contains(item2))
                    return false;
            }

            return true;
        }

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
                throw new ArgumentNullException("firstCollection");
            if (secondCollection == null)
                throw new ArgumentNullException("secondCollection");
            if (equalityComparer == null)
                throw new ArgumentException("equalityComparer");

            var firstSet = firstCollection as HashSet<T>;
            if ((firstSet == null) || (firstSet.Comparer != equalityComparer))
                firstSet = new HashSet<T>(firstCollection, equalityComparer);

            var secondSet = secondCollection as HashSet<T>;
            if ((secondSet == null) || (secondSet.Comparer != equalityComparer))
                secondSet = new HashSet<T>(secondCollection, equalityComparer);

            return firstSet.SetEquals(secondSet);
        }

        [NotNull]
        public static IEnumerable<T> SymmetricExcept<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection)
        {
            return SymmetricExcept(firstCollection, secondCollection, EqualityComparer<T>.Default);
        }

        [NotNull]
        public static IEnumerable<T> SymmetricExcept<T>(
            [NotNull] this IEnumerable<T> firstCollection,
            [NotNull] IEnumerable<T> secondCollection,
            [NotNull] IEqualityComparer<T> equalityComparer)
        {
            if (firstCollection == null)
                throw new ArgumentNullException("firstCollection");
            if (secondCollection == null)
                throw new ArgumentNullException("secondCollection");
            if (equalityComparer == null)
                throw new ArgumentException("equalityComparer");

            var set1 = new HashSet<T>(firstCollection, equalityComparer);
            var set2 = new HashSet<T>(secondCollection, equalityComparer);

            if (set1.Count > set2.Count)
            {
                set1.SymmetricExceptWith(set2);
                return set1;
            }

            set2.SymmetricExceptWith(set1);
            return set2;
        }

        [NotNull]
        public static IEnumerable<Tuple<TFirst, TSecond>> CartesianProduct<TFirst, TSecond>(
            [NotNull] this IEnumerable<TFirst> first,
            [NotNull] IEnumerable<TSecond> second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            return (from firstItem in first
                    from secondItem in second
                    select new Tuple<TFirst, TSecond>(firstItem, secondItem)).Cast<Tuple<TFirst, TSecond>>();
        }
        #endregion Set operations

        #region String representations (not yet coded)
        [NotNull]
        public static string ToString<T>([NotNull] this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
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
                throw new ArgumentNullException("start");
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (end == null)
                throw new ArgumentNullException("end");

            if (collection == null)
                return "null";

            var firstItem = true;

            var builder = new StringBuilder();

            builder.Append(start);

            // Call ToString on each item and put it in.
            foreach (var item in collection)
            {
                if (!firstItem)
                    builder.Append(separator);

                // "TypedAs<object>((IEnumerable)item)" is never null because item is never 'null'.
                // ReSharper disable AssignNullToNotNullAttribute
                if (ReferenceEquals(item, null))
                    builder.Append("null");
                else if (recursive && item is IEnumerable && !(item is string))
                    builder.Append(ToString(TypedAs<object>((IEnumerable)item), true, start, separator, end));
                else
                    builder.Append(item.ToString());
                // ReSharper restore AssignNullToNotNullAttribute

                firstItem = false;
            }

            builder.Append(end);
            return builder.ToString();
        }

        [NotNull]
        public static string ToString<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary)
        {
            var firstItem = true;

            if (dictionary == null)
                return "null";

            var builder = new StringBuilder();

            builder.Append("{");

            // Call ToString on each item and put it in.
            foreach (var pair in dictionary)
            {
                if (!firstItem)
                    builder.Append(", ");

                // "TypedAs<object>((IEnumerable)pair.Key/Value)" is never null because pair.Key/Value is never 'null'.
                // ReSharper disable AssignNullToNotNullAttribute

                if (ReferenceEquals(pair.Key, null))
                    builder.Append("null");
                else if (pair.Key is IEnumerable && !(pair.Key is string))
                    builder.Append(ToString(TypedAs<object>((IEnumerable)pair.Key), true, "{", ",", "}"));
                else
                    builder.Append(pair.Key.ToString());

                builder.Append("->");

                if (ReferenceEquals(pair.Value, null))
                    builder.Append("null");
                else if (pair.Value is IEnumerable && !(pair.Value is string))
                    builder.Append(ToString(TypedAs<object>((IEnumerable)pair.Value), true, "{", ",", "}"));
                else
                    builder.Append(pair.Value.ToString());

                // ReSharper restore AssignNullToNotNullAttribute

                firstItem = false;
            }

            builder.Append("}");
            return builder.ToString();
        }
        #endregion String representations

        #region Shuffles and Permutations
        public static T RandomElement<T>([NotNull] this IEnumerable<T> collection)
        {
            T result;
            if (!SelectRandomElement(collection, out result))
                throw new InvalidOperationException("Sequence contains no elements");
            return result;
        }

        public static T RandomElementOrDefault<T>([NotNull] this IEnumerable<T> collection)
        {
            T result;
            SelectRandomElement(collection, out result);
            return result;
        }

        private static bool SelectRandomElement<T>([NotNull] IEnumerable<T> collection, out T result)
        {
            result = default(T);

            if (!collection.Any())
                return false;

            var list = collection as ICollection;
            var trueList = collection as IList;
            var indexable = collection as IIndexedEnumerable<T>;

            int count;

            if (trueList != null)
                count = trueList.Count;
            else if (indexable != null)
                count = indexable.Count;
            else if (list != null)
                count = list.Count;
            else
                count = collection.Count();

            var skipCount = RandomProvider.Shared.Next(count);

            if (trueList != null)
                result = (T)trueList[skipCount];
            else if (indexable != null)
                result = indexable[skipCount];
            else
                result = collection.Skip(skipCount).First();

            return true;
        }

        public static T[] Randomize<T>([NotNull] this IEnumerable<T> collection)
        {
            // We have to copy all items anyway, and there isn't a way to produce the items
            // on the fly that is linear. So copying to an array and shuffling it is an efficient as we can get.
            if (collection == null)
                throw new ArgumentNullException("collection");

            var array = collection.ToArray();

            var count = array.Length;
            for (var i = count - 1; i >= 1; --i)
            {
                // Pick an random number 0 through i inclusive.
                var j = RandomProvider.Shared.Next(i + 1);

                // Swap array[i] and array[j]
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            return array;
        }

        public static void RandomizeInPlace<T>([NotNull] this IList<T> list)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (list.IsReadOnly) throw new ArgumentException("List is read-only.", "list");

            for (int i = 0; i < list.Count - 1; i++)
            {
                int j = RandomProvider.Shared.Next(i, list.Count);

                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }

        [NotNull]
        public static T[] RandomSubset<T>([NotNull] this IEnumerable<T> collection, int count)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            // We need random access to the items in the collection. If it's not already an 
            // IList<T>, copy to a temporary list.
            var list = collection as IList<T> ?? new List<T>(collection);

            var listCount = list.Count;
            if (count < 0 || count > listCount)
                throw new ArgumentOutOfRangeException("count");

            var result = new T[count]; // the result array.
            var swappedValues = new Dictionary<int, T>(count); // holds swapped values from the list.

            for (var i = 0; i < count; ++i)
            {
                // Set j to the index of the item to swap with, and value to the value to swap with.
                T value;
                var j = RandomProvider.Shared.Next(listCount - i) + i;

                // Swap values of i and j in the list. The list isn't actually changed; instead,
                // swapped values are stored in the dictionary swappedValues.
                if (!swappedValues.TryGetValue(j, out value))
                    value = list[j];

                result[i] = value;

                if (i == j)
                    continue;

                if (swappedValues.TryGetValue(i, out value))
                    swappedValues[j] = value;
                else
                    swappedValues[j] = list[i];
            }

            return result;
        }

        [NotNull]
        public static IEnumerable<T[]> GeneratePermutations<T>([NotNull] this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var array = collection.ToArray();

            if (array.Length == 0)
                yield break;

            var state = new int[array.Length - 1];
            var maxLength = state.Length;

            yield return array;

            if (array.Length == 1)
                yield break;

            // The following algorithm makes two swaps for each
            // permutation generated.
            // This is not optimal in terms of number of swaps, but
            // is still O(1), and shorter and clearer to understand.
            var i = 0;
            for (; ; )
            {
                T temp;
                if (state[i] < i + 1)
                {
                    if (state[i] > 0)
                    {
                        temp = array[i + 1];
                        array[i + 1] = array[state[i] - 1];
                        array[state[i] - 1] = temp;
                    }

                    temp = array[i + 1];
                    array[i + 1] = array[state[i]];
                    array[state[i]] = temp;

                    yield return array;

                    ++state[i];
                    i = 0;
                }
                else
                {
                    temp = array[i + 1];
                    array[i + 1] = array[i];
                    array[i] = temp;

                    state[i] = 0;
                    ++i;
                    if (i >= maxLength)
                        yield break;
                }
            }
        }

        [NotNull]
        public static IEnumerable<T[]> GenerateSortedPermutations<T>([NotNull] this IEnumerable<T> collection)
            where T : IComparable<T>
        {
            return GenerateSortedPermutations(collection, Comparer<T>.Default);
        }

        [NotNull]
        public static IEnumerable<T[]> GenerateSortedPermutations<T>([NotNull] this IEnumerable<T> collection, [NotNull] IComparer<T> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var array = collection.ToArray();
            var length = array.Length;
            if (length == 0)
                yield break;

            Array.Sort(array, comparer);

            yield return array;
            if (length == 1)
                yield break;

            // Keep generating the next permutation until we're done. Algorithm is
            // due to Jeffrey A. Johnson ("SEPA - a Simple Efficient Permutation Algorithm")
            for (; ; )
            {
                // Find the key point -- where array[key]<array[key+1]. Everything after the
                // key is the tail.
                var key = length - 2;
                while (comparer.Compare(array[key], array[key + 1]) >= 0)
                {
                    --key;
                    if (key < 0)
                        yield break;
                }

                // Find the last item in the tail less than key.
                var swap = length - 1;
                while (comparer.Compare(array[swap], array[key]) <= 0)
                    --swap;

                // Swap it with the key.
                var temp = array[key];
                array[key] = array[swap];
                array[swap] = temp;

                // Reverse the tail.
                var i = key + 1;
                var j = length - 1;
                while (i < j)
                {
                    temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    ++i;
                    --j;
                }

                yield return array;
            }
        }

        [NotNull]
        public static IEnumerable<T[]> GenerateSortedPermutations<T>([NotNull] this IEnumerable<T> collection, [NotNull] Comparison<T> comparison)
        {
            return GenerateSortedPermutations(collection, new ComparerFromComparison<T>(comparison));
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
                throw new ArgumentNullException("collection");
            if (comparandResolver == null)
                throw new ArgumentNullException("comparandResolver");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var maxSoFar = default(TComparand);
            var maxElementSoFar = default(T);
            var foundOne = false;

            // Go through the collection, keeping the maximum found so far.
            foreach (var item in collection)
            {
                var comparand = comparandResolver(item);
                if (!foundOne || comparer.Compare(maxSoFar, comparand) < 0)
                {
                    maxSoFar = comparand;
                    maxElementSoFar = item;
                }
                foundOne = true;
            }

            // If the collection was empty, throw an exception.
            if (!foundOne)
                throw new InvalidOperationException("Sequence is empty.");

            return maxElementSoFar;
        }

        public static T MaxElement<T, TComparand>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, TComparand> comparandResolver,
            [NotNull] Comparison<TComparand> comparison)
        {
            return MaxElement(collection, comparandResolver, new ComparerFromComparison<TComparand>(comparison));
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
                throw new ArgumentNullException("collection");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var minSoFar = default(TComparand);
            var minElementSoFar = default(T);
            var foundOne = false;

            // Go through the collection, keeping the minimum found so far.
            foreach (var item in collection)
            {
                var comparand = comparandResolver(item);
                if (!foundOne || comparer.Compare(minSoFar, comparand) > 0)
                {
                    minSoFar = comparand;
                    minElementSoFar = item;
                }
                foundOne = true;
            }

            // If the collection was empty, throw an exception.
            if (!foundOne)
                throw new InvalidOperationException("Sequence is empty.");

            return minElementSoFar;
        }

        public static T MinElement<T, TComparand>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, TComparand> comparandResolver,
            [NotNull] Comparison<TComparand> comparison)
        {
            return MinElement(collection, comparandResolver, new ComparerFromComparison<TComparand>(comparison));
        }

        public static int MaxElementIndex<T>([NotNull] this IList<T> list)
            where T : IComparable<T>
        {
            return MaxElementIndex(list, Comparer<T>.Default);
        }

        public static int MaxElementIndex<T>([NotNull] this IList<T> list, [NotNull] IComparer<T> comparer)
        {
            return MaxElementIndex(new ListAsIndexedCollection<T>(list), comparer);
        }

        public static int MaxElementIndex<T>([NotNull] this IList<T> list, [NotNull] Comparison<T> comparison)
        {
            return MaxElementIndex(list, new ComparerFromComparison<T>(comparison));
        }

        public static int IndexOfMinElement<T>([NotNull] this IList<T> list)
            where T : IComparable<T>
        {
            return IndexOfMinElement(list, Comparer<T>.Default);
        }

        public static int IndexOfMinElement<T>([NotNull] this IList<T> list, [NotNull] IComparer<T> comparer)
        {
            return IndexOfMinElement(new ListAsIndexedCollection<T>(list), comparer);
        }

        public static int IndexOfMinElement<T>([NotNull] this IList<T> list, [NotNull] Comparison<T> comparison)
        {
            return IndexOfMinElement(list, new ComparerFromComparison<T>(comparison));
        }

        public static int MaxElementIndex<T>([NotNull] this IIndexedEnumerable<T> list)
            where T : IComparable<T>
        {
            return MaxElementIndex(list, Comparer<T>.Default);
        }

        public static int MaxElementIndex<T>([NotNull] this IIndexedEnumerable<T> list, [NotNull] IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var maxSoFar = default(T);
            var indexSoFar = -1;

            // Go through the collection, keeping the maximum found so far.
            var i = 0;
            foreach (var item in list)
            {
                if (indexSoFar < 0 || comparer.Compare(maxSoFar, item) < 0)
                {
                    maxSoFar = item;
                    indexSoFar = i;
                }

                ++i;
            }

            return indexSoFar;
        }

        public static int MaxElementIndex<T>([NotNull] this IIndexedEnumerable<T> list, [NotNull] Comparison<T> comparison)
        {
            return MaxElementIndex(list, new ComparerFromComparison<T>(comparison));
        }

        public static int IndexOfMinElement<T>([NotNull] this IIndexedEnumerable<T> list)
            where T : IComparable<T>
        {
            return IndexOfMinElement(list, Comparer<T>.Default);
        }

        public static int IndexOfMinElement<T>([NotNull] this IIndexedEnumerable<T> list, [NotNull] IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var minSoFar = default(T);
            var indexSoFar = -1;

            // Go through the collection, keeping the minimum found so far.
            var i = 0;
            foreach (var item in list)
            {
                if (indexSoFar < 0 || comparer.Compare(minSoFar, item) > 0)
                {
                    minSoFar = item;
                    indexSoFar = i;
                }

                ++i;
            }

            return indexSoFar;
        }

        public static int IndexOfMinElement<T>([NotNull] this IIndexedEnumerable<T> list, [NotNull] Comparison<T> comparison)
        {
            return IndexOfMinElement(list, new ComparerFromComparison<T>(comparison));
        }
        #endregion MinElement and MaxElement

        #region Sorting and operations on sorted collections

        [NotNull]
        public static T[] Sort<T>([NotNull] this IEnumerable<T> collection)
            where T : IComparable<T>
        {
            return Sort(collection, Comparer<T>.Default);
        }

        [NotNull]
        public static T[] Sort<T>([NotNull] this IEnumerable<T> collection, [NotNull] IComparer<T> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var array = collection.ToArray();

            Array.Sort(array, comparer);
            return array;
        }

        [NotNull]
        public static T[] Sort<T>([NotNull] this IEnumerable<T> collection, [NotNull] Comparison<T> comparison)
        {
            return Sort(collection, new ComparerFromComparison<T>(comparison));
        }

        public static IList<T> SortInPlace<T>([NotNull] this IList<T> list)
            where T : IComparable<T>
        {
            return SortInPlace(list, Comparer<T>.Default);
        }

        public static IList<T> SortInPlace<T>([NotNull] this IList<T> list, [NotNull] IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            // If we have an array, use the built-in array sort (faster than going through IList accessors
            // with virtual calls).
            if (list is T[])
            {
                Array.Sort((T[])list, comparer);
                return list;
            }

            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            // Instead of a recursive procedure, we use an explicit stack to hold
            // ranges that we still need to sort.
            int[] leftStack = new int[32], rightStack = new int[32];
            var stackPtr = 0;

            var l = 0; // the inclusive left edge of the current range we are sorting.
            var r = list.Count - 1; // the inclusive right edge of the current range we are sorting.

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
                    var m = l + (r - l) / 2;
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
                                break;

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

        [NotNull]
        public static T[] StableSort<T>([NotNull] this IEnumerable<T> collection)
            where T : IComparable<T>
        {
            return StableSort(collection, Comparer<T>.Default);
        }

        [NotNull]
        public static T[] StableSort<T>([NotNull] this IEnumerable<T> collection, [NotNull] IComparer<T> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            T[] array = collection.ToArray();

            StableSortInPlace(AsWritableList(array), comparer);
            return array;
        }

        [NotNull]
        public static T[] StableSort<T>([NotNull] this IEnumerable<T> collection, [NotNull] Comparison<T> comparison)
        {
            return StableSort(collection, new ComparerFromComparison<T>(comparison));
        }

        public static IList<T> StableSortInPlace<T>([NotNull] this IList<T> list)
            where T : IComparable<T>
        {
            return StableSortInPlace(list, Comparer<T>.Default);
        }

        public static IList<T> StableSortInPlace<T>([NotNull] this IList<T> list, [NotNull] IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            // The stable sort algorithms also uses QuickSort. An additional array of indices (order) is
            // used to maintain the original order of items in the array, and that array is used
            // as a secondary compare when the primary compare returns equal.
            var order = new int[list.Count];
            for (var x = 0; x < order.Length; ++x)
                order[x] = x;

            // Instead of a recursive procedure, we use an explicit stack to hold
            // ranges that we still need to sort.
            int[] leftStack = new int[32], rightStack = new int[32];
            var stackPtr = 0;

            var leftEdge = 0; // the inclusive left edge of the current range we are sorting.
            var rightEdge = list.Count - 1; // the inclusive right edge of the current range we are sorting.

            // Loop until we have nothing left to sort. On each iteration, l and r contains the bounds
            // of something to sort (unless r <= l), and leftStack/rightStack have a stack of unsorted
            // pieces (unles stackPtr == 0).
            for (; ; )
            {
                int comparisonResult; // holds the result of a comparison temporarily.
                if (leftEdge == rightEdge - 1)
                {
                    // We have exactly 2 elements to sort. Compare them and swap if needed.
                    var e1 = list[leftEdge];
                    var o1 = order[leftEdge];
                    var e2 = list[rightEdge];
                    var o2 = order[rightEdge];
                    if ((comparisonResult = comparer.Compare(e1, e2)) > 0 || (comparisonResult == 0 && o1 > o2))
                    {
                        list[rightEdge] = e1;
                        order[rightEdge] = o1;
                        list[leftEdge] = e2;
                        order[leftEdge] = o2;
                    }
                    leftEdge = rightEdge; // sort complete, find other work from the stack.
                }
                else if (leftEdge < rightEdge)
                {
                    // Sort the items in the inclusive range l .. r

                    // Get the left, middle, and right-most elements and sort them, yielding e1=smallest, e2=median, e3=largest
                    var m = leftEdge + (rightEdge - leftEdge) / 2;
                    T e1 = list[leftEdge], e2 = list[m], e3 = list[rightEdge], temp;
                    int o1 = order[leftEdge], o2 = order[m], o3 = order[rightEdge], otemp;
                    if ((comparisonResult = comparer.Compare(e1, e2)) > 0 || (comparisonResult == 0 && o1 > o2))
                    {
                        temp = e1;
                        e1 = e2;
                        e2 = temp;
                        otemp = o1;
                        o1 = o2;
                        o2 = otemp;
                    }
                    if ((comparisonResult = comparer.Compare(e1, e3)) > 0 || (comparisonResult == 0 && o1 > o3))
                    {
                        temp = e3;
                        e3 = e2;
                        e2 = e1;
                        e1 = temp;
                        otemp = o3;
                        o3 = o2;
                        o2 = o1;
                        o1 = otemp;
                    }
                    else if ((comparisonResult = comparer.Compare(e2, e3)) > 0 || (comparisonResult == 0 && o2 > o3))
                    {
                        temp = e2;
                        e2 = e3;
                        e3 = temp;
                        otemp = o2;
                        o2 = o3;
                        o3 = otemp;
                    }

                    if (leftEdge == rightEdge - 2)
                    {
                        // We have exactly 3 elements to sort, and we've done that. Store back and we're done.
                        list[leftEdge] = e1;
                        list[m] = e2;
                        list[rightEdge] = e3;
                        order[leftEdge] = o1;
                        order[m] = o2;
                        order[rightEdge] = o3;
                        leftEdge = rightEdge; // sort complete, find other work from the stack.
                    }
                    else
                    {
                        // Put the smallest at the left, largest in the middle, and the median at the right (which is the partitioning value)
                        list[leftEdge] = e1;
                        order[leftEdge] = o1;
                        list[m] = e3;
                        order[m] = o3;
                        T partition; // The partition value.
                        list[rightEdge] = partition = e2;
                        int orderPartition; // The order of the partition value;
                        order[rightEdge] = orderPartition = o2;

                        // Partition into three parts, items <= partition, items == partition, and items >= partition
                        int i = leftEdge, j = rightEdge;
                        T firstItem;
                        int firstOrder;
                        for (; ; )
                        {
                            do
                            {
                                ++i;
                                firstItem = list[i];
                                firstOrder = order[i];
                            }
                            while ((comparisonResult = comparer.Compare(firstItem, partition)) < 0 ||
                                   (comparisonResult == 0 && firstOrder < orderPartition));

                            T secondItem;
                            int secondOrder;
                            do
                            {
                                --j;
                                secondItem = list[j];
                                secondOrder = order[j];
                            }
                            while ((comparisonResult = comparer.Compare(secondItem, partition)) > 0 ||
                                   (comparisonResult == 0 && secondOrder > orderPartition));

                            if (j < i)
                                break;

                            list[i] = secondItem;
                            list[j] = firstItem; // swap items to continue the partition.
                            order[i] = secondOrder;
                            order[j] = firstOrder;
                        }

                        // Move the partition value into place.
                        list[rightEdge] = firstItem;
                        order[rightEdge] = firstOrder;
                        list[i] = partition;
                        order[i] = orderPartition;
                        ++i;

                        // We have partitioned the list. 
                        //    Items in the inclusive range l .. j are <= partition.
                        //    Items in the inclusive range i .. r are >= partition.
                        //    Items in the inclusive range j+1 .. i - 1 are == partition (and in the correct final position).
                        // We now need to sort l .. j and i .. r.
                        // To do this, we stack one of the lists for later processing, and change l and r to the other list.
                        // If we always stack the larger of the two sub-parts, the stack cannot get greater
                        // than log2(Count) in size; i.e., a 32-element stack is enough for the maximum list size.
                        if ((j - leftEdge) > (rightEdge - i))
                        {
                            // The right partition is smaller. Stack the left, and get ready to sort the right.
                            leftStack[stackPtr] = leftEdge;
                            rightStack[stackPtr] = j;
                            leftEdge = i;
                        }
                        else
                        {
                            // The left partition is smaller. Stack the right, and get ready to sort the left.
                            leftStack[stackPtr] = i;
                            rightStack[stackPtr] = rightEdge;
                            rightEdge = j;
                        }
                        ++stackPtr;
                    }
                }
                else if (stackPtr > 0)
                {
                    // We have a stacked sub-list to sort. Pop it off and sort it.
                    --stackPtr;
                    leftEdge = leftStack[stackPtr];
                    rightEdge = rightStack[stackPtr];
                }
                else
                {
                    // We have nothing left to sort.
                    break;
                }
            }

            return list;
        }

        public static IList<T> StableSortInPlace<T>([NotNull] this IList<T> list, [NotNull] Comparison<T> comparison)
        {
            return StableSortInPlace(list, new ComparerFromComparison<T>(comparison));
        }

        public static int BinarySearch<T>([NotNull] this IList<T> list, T item, out int index)
            where T : IComparable<T>
        {
            return BinarySearch(list, item, Comparer<T>.Default, out index);
        }

        public static int BinarySearch<T>([NotNull] this IList<T> list, T item, [NotNull] IComparer<T> comparer, out int index)
        {
            return BinarySearch(new ListAsIndexedCollection<T>(list), item, comparer, out index);
        }

        public static int BinarySearch<T>([NotNull] this IList<T> list, T item, [NotNull] Comparison<T> comparison, out int index)
        {
            return BinarySearch(list, item, new ComparerFromComparison<T>(comparison), out index);
        }

        public static int BinarySearch<T>([NotNull] this IIndexedEnumerable<T> list, T item, out int index)
            where T : IComparable<T>
        {
            return BinarySearch(list, item, Comparer<T>.Default, out index);
        }

        public static int BinarySearch<T>([NotNull] this IIndexedEnumerable<T> list, T item, [NotNull] IComparer<T> comparer, out int index)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var l = 0;
            var r = list.Count;

            while (r > l)
            {
                var m = l + (r - l) / 2;
                var middleItem = list[m];
                var comp = comparer.Compare(middleItem, item);
                if (comp < 0)
                {
                    // middleItem < item
                    l = m + 1;
                }
                else if (comp > 0)
                {
                    r = m;
                }
                else
                {
                    // Found something equal to item at m. Now we need to find the start and end of this run of equal items.
                    int lFound = l, rFound = r, found = m;

                    // Find the start of the run.
                    l = lFound;
                    r = found;
                    while (r > l)
                    {
                        m = l + (r - l) / 2;
                        middleItem = list[m];
                        comp = comparer.Compare(middleItem, item);
                        if (comp < 0)
                        {
                            // middleItem < item
                            l = m + 1;
                        }
                        else
                        {
                            r = m;
                        }
                    }
                    Debug.Assert(l == r, "Left and Right were not equal");
                    index = l;

                    // Find the end of the run.
                    l = found;
                    r = rFound;
                    while (r > l)
                    {
                        m = l + (r - l) / 2;
                        middleItem = list[m];
                        comp = comparer.Compare(middleItem, item);
                        if (comp <= 0)
                        {
                            // middleItem <= item
                            l = m + 1;
                        }
                        else
                        {
                            r = m;
                        }
                    }
                    Debug.Assert(l == r, "Left and Right were not equal");
                    return l - index;
                }
            }

            // We did not find the item. l and r must be equal. 
            Debug.Assert(l == r);
            index = l;
            return 0;
        }

        public static int BinarySearch<T>([NotNull] this IIndexedEnumerable<T> list, T item, [NotNull] Comparison<T> comparison, out int index)
        {
            return BinarySearch(list, item, new ComparerFromComparison<T>(comparison), out index);
        }

        [NotNull]
        public static IEnumerable<T> MergeSorted<T>([NotNull] this IEnumerable<T> collection, [NotNull] params IEnumerable<T>[] collections)
            where T : IComparable<T>
        {
            return MergeSorted(collection, Comparer<T>.Default, collections);
        }

        [NotNull]
        public static IEnumerable<T> MergeSorted<T>(
            [NotNull] this IEnumerable<T> collection,
            [NotNull] IComparer<T> comparer,
            [NotNull] params IEnumerable<T>[] collections)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (collections == null)
                collections = new IEnumerable<T>[0];
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var appendedCollections = new IEnumerable<T>[collections.Length + 1];

            appendedCollections[0] = collection;
            collections.CopyTo(appendedCollections, 1);

            var enumerators = new IEnumerator<T>[collections.Length];
            var more = new bool[collections.Length];
            var smallestItem = default(T);

            try
            {
                // Get enumerators from each collection, and advance to the first element.
                for (var i = 0; i < appendedCollections.Length; ++i)
                {
                    if (appendedCollections[i] == null)
                        continue;
                    enumerators[i] = appendedCollections[i].GetEnumerator();
                    more[i] = enumerators[i].MoveNext();
                }

                for (; ; )
                {
                    // Find the smallest item, and which collection it is in.
                    var smallestItemIndex = -1;
                    for (var i = 0; i < enumerators.Length; ++i)
                    {
                        if (!more[i])
                            continue;
                        var item = enumerators[i].Current;
                        if (smallestItemIndex < 0 || comparer.Compare(smallestItem, item) > 0)
                        {
                            smallestItemIndex = i;
                            smallestItem = item;
                        }
                    }

                    // If no smallest item found, we're done.
                    if (smallestItemIndex == -1)
                        yield break;

                    // Yield the smallest item.
                    yield return smallestItem;

                    // Advance the enumerator it came from.
                    more[smallestItemIndex] = enumerators[smallestItemIndex].MoveNext();
                }
            }
            finally
            {
                // Dispose all enumerators.
                foreach (var e in enumerators)
                {
                    if (e != null)
                        e.Dispose();
                }
            }
        }

        public static IEnumerable<T> MergeSorted<T>(
            this IEnumerable<T> collection, Comparison<T> comparison, params IEnumerable<T>[] collections)
        {
            return MergeSorted(collection, new ComparerFromComparison<T>(comparison), collections);
        }

        public static int LexicographicalCompare<T>(this IEnumerable<T> sequence1, IEnumerable<T> sequence2)
            where T : IComparable<T>
        {
            return LexicographicalCompare(sequence1, sequence2, Comparer<T>.Default);
        }

        public static int LexicographicalCompare<T>(
            this IEnumerable<T> sequence1, IEnumerable<T> sequence2, Comparison<T> comparison)
        {
            return LexicographicalCompare(sequence1, sequence2, new ComparerFromComparison<T>(comparison));
        }

        public static int LexicographicalCompare<T>(
            this IEnumerable<T> sequence1, IEnumerable<T> sequence2, IComparer<T> comparer)
        {
            if (sequence1 == null)
                throw new ArgumentNullException("sequence1");
            if (sequence2 == null)
                throw new ArgumentNullException("sequence2");
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            using (IEnumerator<T> enum1 = sequence1.GetEnumerator(), enum2 = sequence2.GetEnumerator())
            {
                bool continue1, continue2;

                while (true)
                {
                    continue1 = enum1.MoveNext();
                    continue2 = enum2.MoveNext();
                    if (!continue1 || !continue2)
                        break;

                    var compare = comparer.Compare(enum1.Current, enum2.Current);
                    if (compare != 0)
                        return compare;
                }

                // If both continue1 and continue2 are false, we reached the end of both sequences at the same
                // time and the sequences are equal. Otherwise, the shorter sequence is considered first.
                if (continue1 == continue2)
                    return 0;
                if (continue1)
                    return 1;
                return -1;
            }
        }
        #endregion Sorting

        #region Miscellaneous operations on IEnumerable

        public static bool Any<T>([NotNull] this IEnumerable<T> collection, Func<T, int, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var i = 0;

            for (var e = collection.GetEnumerator(); e.MoveNext(); i++)
            {
                if (predicate(e.Current, i))
                    return true;
            }

            return false;
        }

        public static bool CountAtLeast<T>([NotNull] this IEnumerable<T> collection, int count)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            return Any(collection, (o, i) => (i + 1) >= count);
        }

        [NotNull]
        public static IEnumerable<T> ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var item in collection)
                action(item);

            return collection;
        }

        [NotNull]
        public static IEnumerable<T> ForEach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T, int> action)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (action == null)
                throw new ArgumentNullException("action");

            var i = 0;
            foreach (var item in collection)
                action(item, i++);

            return collection;
        }

        [NotNull]
        public static IEnumerable<T> Concat<T>([NotNull] this IEnumerable<T> collection, [NotNull] params IEnumerable<T>[] collections)
        {
            if (collection == null)
                throw new ArgumentNullException("collections");

            foreach (var item in collection)
                yield return item;

            foreach (var otherCollection in collections)
            {
                foreach (var item in otherCollection)
                    yield return item;
            }
        }
        #endregion Miscellaneous operations on IEnumerable

        #region Miscellaneous operations on IList
        public static void AddRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> items)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (collection.IsReadOnly)
                throw new ArgumentException("CollectionBase is read-only.", "collection");
            if (items == null)
                throw new ArgumentNullException("items");

            items.ForEach(collection.Add);
        }

        public static void Initialize<T>([NotNull] this IList<T> list, T value)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var count = list.Count;
            for (var i = 0; i < count; ++i)
            {
                list[i] = value;
            }
        }

        public static void Initialize<T>([NotNull] this T[] array, T value)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            for (var i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        public static void InitializeRange<T>([NotNull] this IList<T> list, int start, int count, T value)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            if (count == 0)
                return;
            if (start < 0 || start >= list.Count)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0 || count > list.Count || start > list.Count - count)
                throw new ArgumentOutOfRangeException("count");

            for (var i = start; i < count + start; ++i)
                list[i] = value;
        }

        public static void InitializeRange<T>([NotNull] this T[] array, int start, int count, T value)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (count == 0)
                return;
            if (start < 0 || start >= array.Length)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0 || count > array.Length || start > array.Length - count)
                throw new ArgumentOutOfRangeException("count");

            for (var i = start; i < count + start; ++i)
                array[i] = value;
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] IList<T> dest)
        {
            CopyTo(source, dest, 0, int.MaxValue);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] IList<T> dest, int destIndex)
        {
            CopyTo(source, dest, destIndex, int.MaxValue);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] T[] dest)
        {
            CopyTo(source, dest, 0);
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] T[] dest, int destIndex)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            if (destIndex < 0 || destIndex > dest.Length)
                throw new ArgumentOutOfRangeException("destIndex");

            using (var sourceEnum = source.GetEnumerator())
            {
                // Overwrite items to the end of the destination array. If we hit the end, throw.
                while (sourceEnum.MoveNext())
                {
                    if (destIndex >= dest.Length)
                        throw new ArgumentException("Array is too small.", "dest");
                    dest[destIndex++] = sourceEnum.Current;
                }
            }
        }

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] IList<T> dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");
            if (dest.IsReadOnly)
                throw new ArgumentException("List is read-only.", "dest");

            var destCount = dest.Count;

            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            using (var sourceEnum = source.GetEnumerator())
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

        public static void CopyTo<T>([NotNull] this IEnumerable<T> source, [NotNull] T[] dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            var destCount = dest.Length;

            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0 || destIndex + count > destCount)
                throw new ArgumentOutOfRangeException("count");

            using (var sourceEnum = source.GetEnumerator())
            {
                // First, overwrite items to the end of the destination array.
                while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
                {
                    dest[destIndex++] = sourceEnum.Current;
                    --count;
                }
            }
        }

        public static void CopyTo<T>([NotNull] this IList<T> source, int sourceIndex, [NotNull] IList<T> dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");
            if (dest.IsReadOnly)
                throw new ArgumentException("List is read-only.", "dest");

            var sourceCount = source.Count;
            var destCount = dest.Count;

            if (sourceIndex < 0 || sourceIndex >= sourceCount)
                throw new ArgumentOutOfRangeException("sourceIndex");
            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (count > sourceCount - sourceIndex)
                count = sourceCount - sourceIndex;

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
                    var numberToInsert = destIndex + count - destCount;
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

        public static void CopyTo<T>([NotNull] this IList<T> source, int sourceIndex, [NotNull] T[] dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            var sourceCount = source.Count;
            var destCount = dest.Length;

            if (sourceIndex < 0 || sourceIndex >= sourceCount)
                throw new ArgumentOutOfRangeException("sourceIndex");
            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0 || destIndex + count > destCount)
                throw new ArgumentOutOfRangeException("count");

            if (count > sourceCount - sourceIndex)
                count = sourceCount - sourceIndex;

            if (source is T[])
            {
                // Array.Copy is probably faster, and also handles any overlapping issues.
                Array.Copy((T[])source, sourceIndex, dest, destIndex, count);
            }
            else
            {
                var si = sourceIndex;
                var di = destIndex;
                while (count > 0)
                {
                    dest[di++] = source[si++];
                    --count;
                }
            }
        }
        
        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] IList<TResult> dest)
        {
            SelectInto(source, selector, dest, 0, int.MaxValue);
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] IList<TResult> dest, int destIndex)
        {
            SelectInto(source, selector, dest, destIndex, int.MaxValue);
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] TResult[] dest)
        {
            SelectInto(source, selector, dest, 0);
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] TResult[] dest, int destIndex)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            if (destIndex < 0 || destIndex > dest.Length)
                throw new ArgumentOutOfRangeException("destIndex");

            using (var sourceEnum = source.GetEnumerator())
            {
                // Overwrite items to the end of the destination array. If we hit the end, throw.
                while (sourceEnum.MoveNext())
                {
                    if (destIndex >= dest.Length)
                        throw new ArgumentException("Array is too small.", "dest");
                    dest[destIndex++] = selector(sourceEnum.Current);
                }
            }
        }

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] IList<TResult> dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");
            if (dest.IsReadOnly)
                throw new ArgumentException("List is read-only.", "dest");

            var destCount = dest.Count;

            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            using (var sourceEnum = source.GetEnumerator())
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

        public static void SelectInto<T, TResult>([NotNull] this IEnumerable<T> source, Func<T, TResult> selector, [NotNull] TResult[] dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            var destCount = dest.Length;

            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0 || destIndex + count > destCount)
                throw new ArgumentOutOfRangeException("count");

            using (var sourceEnum = source.GetEnumerator())
            {
                // First, overwrite items to the end of the destination array.
                while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
                {
                    dest[destIndex++] = selector(sourceEnum.Current);
                    --count;
                }
            }
        }

        public static void SelectInto<T, TResult>([NotNull] this IList<T> source, Func<T, TResult> selector, int sourceIndex, [NotNull] IList<TResult> dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");
            if (dest.IsReadOnly)
                throw new ArgumentException("List is read-only.", "dest");

            var sourceCount = source.Count;
            var destCount = dest.Count;

            if (sourceIndex < 0 || sourceIndex >= sourceCount)
                throw new ArgumentOutOfRangeException("sourceIndex");
            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (count > sourceCount - sourceIndex)
                count = sourceCount - sourceIndex;

            if (source == dest && sourceIndex > destIndex)
            {
                while (count > 0)
                {
                    dest[destIndex++] = selector(source[sourceIndex++]);
                    --count;
                }
            }
            else
            {
                int si, di;

                // First, insert any items needed at the end
                if (destIndex + count > destCount)
                {
                    var numberToInsert = destIndex + count - destCount;

                    si = sourceIndex + (count - numberToInsert);
                    di = destCount;
                    count -= numberToInsert;

                    while (numberToInsert > 0)
                    {
                        dest.Insert(di++, selector(source[si++]));
                        --numberToInsert;
                    }
                }

                // Do the copy, from end to beginning in case of overlap.
                si = sourceIndex + count - 1;
                di = destIndex + count - 1;

                while (count > 0)
                {
                    dest[di--] = selector(source[si--]);
                    --count;
                }
            }
        }

        public static void SelectInto<T, TResult>([NotNull] this IList<T> source, Func<T, TResult> selector, int sourceIndex, [NotNull] TResult[] dest, int destIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dest == null)
                throw new ArgumentNullException("dest");

            var sourceCount = source.Count;
            var destCount = dest.Length;

            if (sourceIndex < 0 || sourceIndex >= sourceCount)
                throw new ArgumentOutOfRangeException("sourceIndex");
            if (destIndex < 0 || destIndex > destCount)
                throw new ArgumentOutOfRangeException("destIndex");
            if (count < 0 || destIndex + count > destCount)
                throw new ArgumentOutOfRangeException("count");

            if (count > sourceCount - sourceIndex)
                count = sourceCount - sourceIndex;

            while (count > 0)
            {
                dest[destIndex++] = selector(source[sourceIndex++]);
                --count;
            }
        }

        public static IList<T> ReverseInPlace<T>([NotNull] this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var i = 0;
            var j = list.Count - 1;

            while (i < j)
            {
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
                i++;
                j--;
            }

            return list;
        }

        [NotNull]
        public static IEnumerable<T> Rotate<T>([NotNull] this IList<T> source, int amountToRotate)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return Rotate(new ListAsIndexedCollection<T>(source), amountToRotate);
        }

        [NotNull]
        public static IEnumerable<T> Rotate<T>([NotNull] this IIndexedEnumerable<T> source, int amountToRotate)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var count = source.Count;
            if (count != 0)
            {
                amountToRotate = amountToRotate % count;
                if (amountToRotate < 0)
                    amountToRotate += count;

                // Do it in two parts.
                for (var i = amountToRotate; i < count; ++i)
                    yield return source[i];
                for (var i = 0; i < amountToRotate; ++i)
                    yield return source[i];
            }
        }

        public static IList<T> RotateInPlace<T>([NotNull] this IList<T> list, int amountToRotate)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (list is T[])
                list = new ArrayWrapper<T>((T[])list);
            if (list.IsReadOnly)
                throw new ArgumentException("List is read-only.", "list");

            var count = list.Count;
            if (count != 0)
            {
                amountToRotate = amountToRotate % count;
                if (amountToRotate < 0)
                    amountToRotate += count;

                var itemsLeft = count;
                var indexStart = 0;
                while (itemsLeft > 0)
                {
                    // Rotate an orbit of items through the list. If itemsLeft is relatively prime
                    // to count, this will rotate everything. If not, we need to do this several times until
                    // all items have been moved.
                    var index = indexStart;
                    var itemStart = list[indexStart];
                    for (; ; )
                    {
                        --itemsLeft;
                        var nextIndex = index + amountToRotate;
                        if (nextIndex >= count)
                            nextIndex -= count;
                        if (nextIndex == indexStart)
                        {
                            list[index] = itemStart;
                            break;
                        }

                        list[index] = list[nextIndex];
                        index = nextIndex;
                    }

                    // Move to the next orbit.
                    ++indexStart;
                }
            }

            return list;
        }
        #endregion Miscellaneous operations on IList

        #region CollectionBase Conversions

        [NotNull]
        public static HashSet<T> ToHashSet<T>([NotNull] this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        #endregion

        #region Aggregate Strings

        [NotNull]
        public static string AggregateString<TSource>(
            [NotNull] this IEnumerable<TSource> source,
            [CanBeNull] string separator,
            [CanBeNull] [InstantHandle] Func<StringBuilder, TSource, StringBuilder> funcAppend = null)
        {
            return source.AggregateString(new StringBuilder(), separator, funcAppend, (_ => (_).ToString()));
        }

        [NotNull]
        public static TResult AggregateString<TSource, TResult>(
            [NotNull] this IEnumerable<TSource> source,
            [CanBeNull] string separator,
            [InstantHandle] [CanBeNull] Func<StringBuilder, TSource, StringBuilder> funcAppend,
            [InstantHandle] [NotNull] Func<StringBuilder, TResult> funcConvert)
        {
            return source.AggregateString(new StringBuilder(), separator, funcAppend, funcConvert);
        }

        [NotNull]
        public static string AggregateString<TSource>(
            [NotNull] this IEnumerable<TSource> source,
            [CanBeNull] string seed,
            [CanBeNull] string separator,
            [InstantHandle] [CanBeNull] Func<StringBuilder, TSource, StringBuilder> funcAppend = null)
        {
            return source.AggregateString(new StringBuilder(seed), separator, funcAppend, (_ => (_).ToString()));
        }

        [NotNull]
        public static TResult AggregateString<TSource, TResult>(
            [NotNull] this IEnumerable<TSource> source,
            [CanBeNull] string seed,
            [CanBeNull] string separator,
            [CanBeNull] [InstantHandle] Func<StringBuilder, TSource, StringBuilder> funcAppend,
            [InstantHandle] [NotNull] Func<StringBuilder, TResult> funcConvert)
        {
            return source.AggregateString(new StringBuilder(seed), separator, funcAppend, funcConvert);
        }

        [NotNull]
        public static string AggregateString<TSource>(
            [NotNull] this IEnumerable<TSource> source,
            [NotNull] StringBuilder seed,
            [CanBeNull] string separator,
            [CanBeNull] Func<StringBuilder, TSource, StringBuilder> funcAppend = null)
        {
            return source.AggregateString(seed, separator, funcAppend, (_ => (_).ToString()));
        }

        [NotNull]
        public static TResult AggregateString<TSource, TResult>(
            [NotNull] this IEnumerable<TSource> source,
            [NotNull] StringBuilder seed,
            [CanBeNull] string separator,
            [CanBeNull] [InstantHandle] Func<StringBuilder, TSource, StringBuilder> funcAppend,
            [InstantHandle] [NotNull] Func<StringBuilder, TResult> funcConvert)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (seed == null)
                throw new ArgumentNullException("seed");

            return source.Aggregate(
                seed,
                (sb, item) =>
                {
                    if (separator != null && sb.Length > seed.Length)
                        sb.Append(separator);
                    if (funcAppend != null)
                        return funcAppend(sb, item);
                    return sb.Append(item);
                },
                funcConvert);
        }

        [NotNull]
        public static string AggregateString<TSource>(
            [NotNull] this IEnumerable<TSource> source,
            [InstantHandle] [NotNull] Func<StringBuilder, TSource, StringBuilder> funcAppend)
        {
            return source.AggregateString(new StringBuilder(), null, funcAppend);
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
            foreach (var i in this)
            {
                if (equalityComparer.Equals(i, item))
                    return true;
            }
            return false;
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            var count = Count;

            if (count == 0)
                return;

            if (array == null)
                throw new ArgumentNullException("array");
            if (count < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "Value must be non-negative");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "Value must be non-negative");
            if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
                throw new ArgumentException("Array is too small.", "arrayIndex");

            int index = arrayIndex, i = 0;
            foreach (var item in this)
            {
                if (i >= count)
                    break;

                array[index] = item;
                ++index;
                ++i;
            }
        }

        public virtual T[] ToArray()
        {
            var count = Count;

            var array = new T[count];
            CopyTo(array, 0);
            return array;
        }

        public abstract int Count { get; }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public virtual ICollection<T> AsReadOnly()
        {
            return CollectionExtensions.AsReadOnly(this);
        }
        #endregion

        #region Delegate operations
        public virtual bool Exists(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return this.Any(predicate);
        }

        public virtual bool TrueForAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return this.All(predicate);
        }

        public virtual int CountWhere(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return CollectionExtensions.CountWhere(this, predicate);
        }

        public virtual IEnumerable<T> FindAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return this.Where(predicate);
        }

        public virtual ICollection<T> RemoveAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return this.RemoveWhere(predicate);
        }

        public virtual void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            CollectionExtensions.ForEach(this, action);
        }

        public virtual IEnumerable<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            return this.Select(o => converter(o));
        }
        #endregion

        #region IEnumerable<T> Members
        public abstract IEnumerator<T> GetEnumerator();
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            var count = Count;

            if (count == 0)
                return;

            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", index, "Value must be non-negative.");
            if (index >= array.Length || count > array.Length - index)
                throw new ArgumentException("Array is too small.", "index");

            var i = 0;
            // TODO: Look into this
            foreach (var o in (ICollection)this)
            {
                if (i >= count)
                    break;

                array.SetValue(o, index);
                ++index;
                ++i;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item;
            }
        }
        #endregion

        internal string DebuggerDisplayString()
        {
            const int maxlength = 250;

            var builder = new StringBuilder();

            builder.Append('{');

            // Call ToString on each item and put it in.
            var firstItem = true;
            foreach (var item in this)
            {
                if (builder.Length >= maxlength)
                {
                    builder.Append(",...");
                    break;
                }

                if (!firstItem)
                    builder.Append(',');

                if (ReferenceEquals(item, null))
                    builder.Append("null");
                else
                    builder.Append(item.ToString());

                firstItem = false;
            }

            builder.Append('}');
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
            var count = Count;
            for (var i = 0; i < count; ++i)
            {
                yield return this[i];
            }
        }

        public override bool Contains(T item)
        {
            return (IndexOf(item) >= 0);
        }

        public override void Add(T item)
        {
            Insert(Count, item);
        }

        public override bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;
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
            var foundIndex = Range(index, Count - index).FirstIndexWhere(predicate);
            if (foundIndex < 0)
                return -1;
            return foundIndex + index;
        }

        public virtual int FindIndex(int index, int count, Func<T, bool> predicate)
        {
            var foundIndex = Range(index, count).FirstIndexWhere(predicate);
            if (foundIndex < 0)
                return -1;
            return foundIndex + index;
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
            var foundIndex = Range(index - count + 1, count).LastIndexWhere(predicate);

            if (foundIndex >= 0)
                return foundIndex + index - count + 1;
            return -1;
        }

        public virtual int IndexOf(T item)
        {
            return this.FirstIndexOf(item, EqualityComparer<T>.Default);
        }

        public virtual int IndexOf(T item, int index)
        {
            var foundIndex = Range(index, Count - index).FirstIndexOf(item, EqualityComparer<T>.Default);

            if (foundIndex >= 0)
                return foundIndex + index;
            return -1;
        }

        public virtual int IndexOf(T item, int index, int count)
        {
            var foundIndex = Range(index, count).FirstIndexOf(item, EqualityComparer<T>.Default);

            if (foundIndex >= 0)
                return foundIndex + index;
            return -1;
        }

        public virtual int LastIndexOf(T item)
        {
            return this.LastIndexOf(item, EqualityComparer<T>.Default);
        }

        public virtual int LastIndexOf(T item, int index)
        {
            var foundIndex = Range(0, index + 1).LastIndexOf(item, EqualityComparer<T>.Default);

            return foundIndex;
        }

        public virtual int LastIndexOf(T item, int index, int count)
        {
            var foundIndex = Range(index - count + 1, count).LastIndexOf(item, EqualityComparer<T>.Default);

            if (foundIndex >= 0)
                return foundIndex + index - count + 1;
            return -1;
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
            var count = Count;
            Insert(count, ConvertToItemType("value", value));
            return count;
        }

        void IList.Clear()
        {
            Clear();
        }

        bool IList.Contains(object value)
        {
            if (value is T || value == null)
                return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (value is T || value == null)
                return IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, ConvertToItemType("value", value));
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return ((ICollection<T>)this).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            if (value is T || value == null)
                Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }

            set { this[index] = ConvertToItemType("value", value); }
        }
    }
    // ReSharper restore CompareNonConstrainedGenericWithNull
    // ReSharper restore HeuristicUnreachableCode
    // ReSharper restore ParameterTypeCanBeEnumerable.Global
    // ReSharper restore ConditionIsAlwaysTrueOrFalse
}