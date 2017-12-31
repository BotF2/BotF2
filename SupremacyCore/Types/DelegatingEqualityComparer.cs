using System;
using System.Collections.Generic;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    public class DelegatingEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalityComparison;
        private readonly Func<T, int> _hashCodeResolver;

        public DelegatingEqualityComparer([NotNull] Func<T, T, bool> equalityComparison, [NotNull] Func<T,int> hashCodeResolver)
        {
            if (equalityComparison == null)
                throw new ArgumentNullException("equalityComparison");
            if (hashCodeResolver == null)
                throw new ArgumentNullException("hashCodeResolver");
            _equalityComparison = equalityComparison;
            _hashCodeResolver = hashCodeResolver;
        }

        #region Implementation of IEqualityComparer<T>
        public bool Equals(T x, T y)
        {
            return _equalityComparison(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (ReferenceEquals(obj, null))
                return 0;
            return _hashCodeResolver(obj);
        }
        #endregion
    }
}