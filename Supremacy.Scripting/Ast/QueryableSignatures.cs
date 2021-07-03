using System;

namespace Supremacy.Scripting.Ast
{

    internal class Queryable
    {
        public Queryable<T> Cast<T>()
        {
            throw new NotImplementedException();
        }
    }

    internal class Queryable<T> : Queryable
    {
        public Queryable<T> Where(Func<T, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public Queryable<U> Select<U>(Func<T, U> selector)
        {
            throw new NotImplementedException();
        }

        public Queryable<V> SelectMany<U, V>(
            Func<T, Queryable<U>> selector,
            Func<T, U, V> resultSelector)
        {
            throw new NotImplementedException();
        }

        public Queryable<V> Join<U, K, V>(
            Queryable<U> inner,
            Func<T, K> outerKeySelector,
            Func<U, K> innerKeySelector,
            Func<T, U, V> resultSelector)
        {
            throw new NotImplementedException();
        }

        public Queryable<V> GroupJoin<U, K, V>(
            Queryable<U> inner,
            Func<T, K> outerKeySelector,
            Func<U, K> innerKeySelector,
            Func<T, Queryable<U>, V> resultSelector)
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

        public Queryable<_Group<K, T>> GroupBy<K>(Func<T, K> keySelector)
        {
            throw new NotImplementedException();
        }

        public Queryable<_Group<K, E>> GroupBy<K, E>(
            Func<T, K> keySelector,
            Func<T, E> elementSelector)
        {
            throw new NotImplementedException();
        }
    }

    internal class _OrderedQueryable<T> : Queryable<T>
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

    internal class _Group<K, T> : Queryable<T>
    {
        public K Key => throw new NotImplementedException();
    }

}