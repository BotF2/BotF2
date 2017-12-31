using System.Collections;
using System.Collections.Generic;

namespace System.Linq.Expressions
{
    /// <summary>
    /// An IQueryable wrapper that allows us to visit the query's expression tree just before LINQ to SQL gets to it.
    /// This is based on the excellent work of Tomas Petricek: http://tomasp.net/blog/linq-expand.aspx
    /// </summary>
    public class ExpandableQuery<T> : IOrderedQueryable<T>
    {
        #region Fields
        private readonly IQueryable<T> _inner;
        private readonly ExpandableQueryProvider<T> _provider;
        #endregion

        // Original query, that we're wrapping

        #region Constructors
        internal ExpandableQuery(IQueryable<T> inner)
        {
            _inner = inner;
            _provider = new ExpandableQueryProvider<T>(this);
        }
        #endregion

        #region Properties and Indexers
        internal IQueryable<T> InnerQuery
        {
            get { return _inner; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return _inner.ToString();
        }
        #endregion

        #region IOrderedQueryable<T> Members
        Expression IQueryable.Expression
        {
            get { return _inner.Expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
        #endregion
    }

    internal class ExpandableQueryProvider<T> : IQueryProvider
    {
        #region Fields
        private readonly ExpandableQuery<T> _query;
        #endregion

        #region Constructors
        internal ExpandableQueryProvider(ExpandableQuery<T> query)
        {
            _query = query;
        }
        #endregion

        // The following four methods first call ExpressionExpander to visit the expression tree, then call
        // upon the inner query to do the remaining work.

        #region IQueryProvider Members
        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return new ExpandableQuery<TElement>(_query.InnerQuery.Provider.CreateQuery<TElement>(expression.Expand()));
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            return _query.InnerQuery.Provider.CreateQuery(expression.Expand());
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return _query.InnerQuery.Provider.Execute<TResult>(expression.Expand());
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return _query.InnerQuery.Provider.Execute(expression.Expand());
        }
        #endregion
    }
}