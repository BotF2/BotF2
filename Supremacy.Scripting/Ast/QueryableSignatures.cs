using System;

namespace Supremacy.Scripting.Ast
{
    // ReSharper disable InconsistentNaming
    internal class _Queryable
    {
        public _Queryable<T> Cast<T>()
        {
            throw new NotImplementedException();
        }
    }

    internal class _Queryable<T> : _Queryable
    {
        public _Queryable<T> Where(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public _Queryable<U> Select<U>(Func<T, U> selector)
        {
            throw new NotImplementedException();
        }

        public _Queryable<V> SelectMany<U, V>(
            Func<T, _Queryable<U>> selector,
            Func<T, U, V> resultSelector)
        {
            throw new NotImplementedException();
        }

        public _Queryable<V> Join<U, K, V>(
            _Queryable<U> inner,
            Func<T, K> outerKeySelector,
            Func<U, K> innerKeySelector,
            Func<T, U, V> resultSelector)
        {
            throw new NotImplementedException();
        }

        public _Queryable<V> GroupJoin<U, K, V>(
            _Queryable<U> inner,
            Func<T, K> outerKeySelector,
            Func<U, K> innerKeySelector,
            Func<T, _Queryable<U>, V> resultSelector)
        {
            throw new NotImplementedException();
        }

        public _OrderedQueryable<T> OrderBy<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }

        public _OrderedQueryable<T> OrderByDescending<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }

        public _Queryable<_Group<K, T>> GroupBy<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }

        public _Queryable<_Group<K, E>> GroupBy<K, E>(
            Func<T, K> keySelector,
            Func<T, E> elementSelector)
        {
            throw new NotImplementedException();
        }
    }

    internal class _OrderedQueryable<T> : _Queryable<T>
    {
        public _OrderedQueryable<T> ThenBy<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }

        public _OrderedQueryable<T> ThenByDescending<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }
    }

    internal class _Group<K, T> : _Queryable<T>
    {
        public K Key
        {
            get { throw new NotImplementedException(); }
        }
    }
    // ReSharper restore InconsistentNaming
}