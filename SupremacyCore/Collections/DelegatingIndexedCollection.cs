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
            if (asEnumerableCallback == null)
                throw new ArgumentNullException("asEnumerableCallback");
            if (countCallback == null)
                throw new ArgumentNullException("countCallback");
            if (indexerCallback == null)
                throw new ArgumentNullException("indexerCallback");

            _baseCollection = baseCollection;
            _asEnumerableCallback = asEnumerableCallback;
            _countCallback = countCallback;
            _indexerCallback = indexerCallback;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _asEnumerableCallback(_baseCollection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _countCallback(_baseCollection); }
        }

        public TValue this[int index]
        {
            get { return _indexerCallback(_baseCollection, index); }
        }

        public bool Contains(TValue value)
        {
            return _asEnumerableCallback(_baseCollection).Contains(value);
        }

        public int IndexOf(TValue value)
        {
            var i = 0;
            var enumerator = _asEnumerableCallback(_baseCollection).GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (Equals(enumerator.Current, value))
                    return i;
                ++i;
            }

            return -1;
        }

        public void CopyTo([NotNull] TValue[] array, int destinationIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (destinationIndex < 0 ||
                destinationIndex + Count >= (array.Length + destinationIndex))
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var i = destinationIndex;
            var enumerator = _asEnumerableCallback(_baseCollection).GetEnumerator();

            while (enumerator.MoveNext())
                array[i++] = enumerator.Current;
        }
    }
}