using System;
using System.Collections;
using System.Collections.Generic;

using Supremacy.Annotations;

using System.Linq;

namespace Supremacy.Collections
{
    public static class IndexedEnumerable
    {
        #region Dependent Type: EmptyIndexedCollection<T>
        private sealed class EmptyIndexedCollection<T> : IIndexedCollection<T>
        {
            private static EmptyIndexedCollection<T> _instance;

            internal static EmptyIndexedCollection<T> Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new EmptyIndexedCollection<T>();
                    }

                    return _instance;
                }
            }
            #region Implementation of IEnumerable
            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion

            #region Implementation of IIndexedEnumerable<T>
            public int Count => 0;

            public T this[int index] => throw new ArgumentOutOfRangeException(nameof(index));
            #endregion

            #region Implementation of IIndexedCollection<T>
            public bool Contains(T value)
            {
                return false;
            }

            public int IndexOf(T value)
            {
                return -1;
            }
            #endregion
        }
        #endregion

        #region Dependent Type: IndexedEnumerableWrapper<T, TInternal>
        private sealed class IndexedEnumerableWrapper<T, TInternal> : IIndexedEnumerable<T>
        {
            private readonly IIndexedEnumerable<TInternal> _innerItems;
            private readonly Func<TInternal, T> _conversionCallback;

            public IndexedEnumerableWrapper(
                [NotNull] IIndexedEnumerable<TInternal> innerItems,
                [NotNull] Func<TInternal, T> conversionCallback)
            {
                _innerItems = innerItems ?? throw new ArgumentNullException(nameof(innerItems));
                _conversionCallback = conversionCallback ?? throw new ArgumentNullException(nameof(conversionCallback));
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _innerItems.Select(o => _conversionCallback(o)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _innerItems.Count;

            public T this[int index] => _conversionCallback(_innerItems[index]);
        }
        #endregion

        public static IIndexedEnumerable<T> Empty<T>()
        {
            return EmptyIndexedCollection<T>.Instance;
        }

        public static IIndexedEnumerable<TDestination> Cast<TSource, TDestination>(this IIndexedEnumerable<TSource> source)
        {
            return new IndexedEnumerableWrapper<TDestination, TSource>(source, o => (TDestination)(object)o);
        }

        public static IIndexedEnumerable<T> Single<T>(T value)
        {
            return new SingleValueIndexedEnumerable<T>(value);
        }

        public static IIndexedEnumerable<T> Concat<T>([NotNull] this IIndexedEnumerable<T> first, [NotNull] IIndexedEnumerable<T> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            return new IndexedEnumerableConcatenation<T>(first, second);
        }

        #region SingleValueIndexedEnumerable Class

        private sealed class SingleValueIndexedEnumerable<T> : IIndexedEnumerable<T>
        {
            private readonly T _value;
            public SingleValueIndexedEnumerable(T value)
            {
                _value = value;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return _value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => 1;

            public T this[int index]
            {
                get
                {
                    if (index != 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    return _value;
                }
            }
        }

        #endregion

        private sealed class IndexedEnumerableConcatenation<T> : IIndexedEnumerable<T>
        {
            private readonly int _secondStart;
            private readonly IIndexedEnumerable<T> _first;
            private readonly IIndexedEnumerable<T> _second;

            public IndexedEnumerableConcatenation([NotNull] IIndexedEnumerable<T> first, [NotNull] IIndexedEnumerable<T> second)
            {
                _first = first ?? throw new ArgumentNullException(nameof(first));
                _second = second ?? throw new ArgumentNullException(nameof(second));
                _secondStart = first.Count;
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _first.Count + _second.Count;

            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    return index >= _secondStart ? _second[index - _secondStart] : _first[index];
                }
            }
        }
    }
}