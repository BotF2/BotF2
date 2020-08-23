using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;

namespace Supremacy.Collections
{
    [Serializable]
    public class DelegatingIndexedCollection<TValue, TCollection> : IIndexedCollection<TValue>
    {
        private readonly TCollection _baseCollection;
        private readonly Func<TCollection, IEnumerable<TValue>> _asEnumerableCallback;
        private readonly Func<TCollection, int> _countCallback;
        private readonly Func<TCollection, int, TValue> _indexerCallback;

        public DelegatingIndexedCollection(
            [NotNull] TCollection baseCollection,
            [NotNull] Func<TCollection, IEnumerable<TValue>> asEnumerableCallback,
            [NotNull] Func<TCollection, int> countCallback,
            [NotNull] Func<TCollection, int, TValue> indexerCallback)
        {
            _baseCollection = baseCollection;
            _asEnumerableCallback = asEnumerableCallback ?? throw new ArgumentNullException(nameof(asEnumerableCallback));
            _countCallback = countCallback ?? throw new ArgumentNullException(nameof(countCallback));
            _indexerCallback = indexerCallback ?? throw new ArgumentNullException(nameof(indexerCallback));
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _asEnumerableCallback(_baseCollection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _countCallback(_baseCollection);

        public TValue this[int index] => _indexerCallback(_baseCollection, index);

        public bool Contains(TValue value)
        {
            return _asEnumerableCallback(_baseCollection).Contains(value);
        }

        public int IndexOf(TValue value)
        {
            int i = 0;
            IEnumerator<TValue> enumerator = _asEnumerableCallback(_baseCollection).GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (Equals(enumerator.Current, value))
                {
                    return i;
                }

                ++i;
            }

            return -1;
        }

        public void CopyTo([NotNull] TValue[] array, int destinationIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (destinationIndex < 0 ||
                destinationIndex + Count >= (array.Length + destinationIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            int i = destinationIndex;
            IEnumerator<TValue> enumerator = _asEnumerableCallback(_baseCollection).GetEnumerator();

            while (enumerator.MoveNext())
            {
                array[i++] = enumerator.Current;
            }
        }
    }
}