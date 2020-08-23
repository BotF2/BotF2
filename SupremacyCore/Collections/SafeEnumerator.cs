using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Supremacy.Collections
{
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        private readonly ReaderWriterLockSlim _syncLock;

        public SafeEnumerator(IEnumerable<T> inner, ReaderWriterLockSlim syncLock)
        {
            _syncLock = syncLock;
            _syncLock.EnterReadLock();
            _inner = inner.GetEnumerator();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _syncLock.ExitReadLock();
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }

        public T Current => _inner.Current;

        object IEnumerator.Current => Current;

        #endregion
    }
}
