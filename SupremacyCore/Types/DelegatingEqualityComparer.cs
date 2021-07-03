using System;
using System.Collections.Generic;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    public class DelegatingEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalityComparison;
        private readonly Func<T, int> _hashCodeResolver;

        public DelegatingEqualityComparer([NotNull] Func<T, T, bool> equalityComparison, [NotNull] Func<T, int> hashCodeResolver)
        {
            _equalityComparison = equalityComparison ?? throw new ArgumentNullException("equalityComparison");
            _hashCodeResolver = hashCodeResolver ?? throw new ArgumentNullException("hashCodeResolver");
        }

        #region Implementation of IEqualityComparer<T>
        public bool Equals(T x, T y)
        {
            return _equalityComparison(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return _hashCodeResolver(obj);
        }
        #endregion
    }
}