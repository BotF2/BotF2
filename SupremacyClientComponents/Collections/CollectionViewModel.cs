///////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2008-2009 David Hill. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

using Supremacy.Client.Input;

namespace Supremacy.Client.Collections.CollectionViewModel
{
    /// <summary>
    /// A generic ViewModel that wraps an underlying collection model.
    /// </summary>
    /// <typeparam name="T">The type of the items in the underlying collection</typeparam>
    public class CollectionViewModel<T> : ObservableCollection<T>, ICollectionView where T : class
    {
        // The underlying model that we're providing a CollectionViewModel for.
        private readonly IEnumerable<T> _items;

        public CollectionViewModel(IEnumerable<T> items)
        {
            // Set reference to the underlying model.
            _items = items;

            // If the underlying model supports it,
            // monitor the underlying model for changes.
            var observableItems = _items as INotifyCollectionChanged;
            if (observableItems != null)
                observableItems.CollectionChanged += OnModelChanged;

            // Initialize the view.
            UpdateView();

            // Initialize the CollectionViewModel's built-in commands.
            _selectNextCommand = new DelegateCommand(SelectNext, CanSelectNext);
            _selectPreviousCommand = new DelegateCommand(SelectPrevious, CanSelectPrevious);
            _sortByCommand = new DelegateCommand<string>(SortBy);
            _groupByCommand = new DelegateCommand<string>(GroupBy);
        }

        private int _currentIndex = -1;
        private SortDescriptionCollection _sortDescriptions;
        private ObservableCollection<GroupDescription> _groupDescriptions;

        // This should really be Predicate<T> but the ICollectionView
        // interface defines the filter predicate with an object type parameter.
        private Predicate<object> _filter;

        #region ICollectionView Members

        public event EventHandler CurrentChanged;

        public event CurrentChangingEventHandler CurrentChanging;

        public bool Contains(object item)
        {
            return base.Contains(item as T);
        }

        public object CurrentItem
        {
            get { return _currentIndex == -1 ? null : this[_currentIndex]; }
        }

        public int CurrentPosition
        {
            get { return _currentIndex; }
        }

        public bool IsCurrentAfterLast
        {
            get { return _currentIndex >= Count; }
        }

        public bool IsCurrentBeforeFirst
        {
            get { return _currentIndex < 0; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool MoveCurrentTo(object item)
        {
            if (Contains(item))
            {
                return UpdateCurrentIndex(IndexOf(item as T));
            }

            // If item is not in collection or is null, move to unselected state.
            return UpdateCurrentIndex(-1);
        }

        public bool MoveCurrentToFirst()
        {
            return UpdateCurrentIndex(0);
        }

        public bool MoveCurrentToLast()
        {
            return UpdateCurrentIndex(Count - 1);
        }

        public bool MoveCurrentToNext()
        {
            return UpdateCurrentIndex(_currentIndex + 1);
        }

        public bool MoveCurrentToPosition(int position)
        {
            return UpdateCurrentIndex(position);
        }

        public bool MoveCurrentToPrevious()
        {
            return UpdateCurrentIndex(_currentIndex > 0 ? _currentIndex - 1 : _currentIndex);
        }

        public void Refresh()
        {
            UpdateCurrentIndex(-1);
        }

        public IEnumerable SourceCollection
        {
            get { Debug.Assert(_items != null); return _items; }
        }

        public bool CanFilter
        {
            get { return true; }
        }

        Predicate<object> ICollectionView.Filter
        {
            get { return _filter; }
            set { _filter = value; UpdateView(); }
        }

        public Predicate<T> Filter
        {
            get { return _filter; }
        }

        public bool CanSort
        {
            get { return true; }
        }

        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (_sortDescriptions == null)
                {
                    _sortDescriptions = new SortDescriptionCollection();

                    // Monitor sort description collection for any changes.
                    ((INotifyCollectionChanged)_sortDescriptions).CollectionChanged += (sender, e) => UpdateView();
                }
                return _sortDescriptions;
            }
        }

        public bool CanGroup
        {
            get { return true; }
        }

        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get
            {
                if (_groupDescriptions == null)
                {
                    _groupDescriptions = new ObservableCollection<GroupDescription>();

                    // Monitor group description collection for any changes.
                    _groupDescriptions.CollectionChanged += (sender, e) => UpdateView();
                }
                return _groupDescriptions;
            }
        }

        public ReadOnlyObservableCollection<object> Groups
        {
            get { throw new NotImplementedException(); }
        }

        public IDisposable DeferRefresh()
        {
            throw new NotImplementedException();
        }

        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return System.Globalization.CultureInfo.InvariantCulture;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Commands - SelectNext, SelectPrevious, SortBy, GroupBy

        private readonly DelegateCommand _selectNextCommand;
        private readonly DelegateCommand _selectPreviousCommand;
        private readonly DelegateCommand<string> _sortByCommand;
        private readonly DelegateCommand<string> _groupByCommand;

        public ICommand SelectNextCommand
        {
            get { return _selectNextCommand; }
        }

        bool CanSelectNext()
        {
            return (_currentIndex < Count - 1);
        }

        private void SelectNext()
        {
            MoveCurrentToNext();
        }

        public ICommand SelectPreviousCommand
        {
            get { return _selectPreviousCommand; }
        }

        private bool CanSelectPrevious()
        {
            return (_currentIndex > 0);
        }

        private void SelectPrevious()
        {
            MoveCurrentToPrevious();
        }

        public ICommand SortByCommand
        {
            get { return _sortByCommand; }
        }

        private void SortBy(string property)
        {
            SortDescriptions.Clear();
            SortDescriptions.Add(new SortDescription(property, ListSortDirection.Ascending));
        }

        public ICommand GroupByCommand
        {
            get { return _groupByCommand; }
        }

        private void GroupBy(string property)
        {
            GroupDescriptions.Clear();
            GroupDescriptions.Add(new PropertyGroupDescription(property));
        }
        #endregion

        private void UpdateView()
        {
            IQueryable<T> results = BuildQuery();

            Clear();

            foreach (T item in results)
            {
                Add(item);
            }

            UpdateCurrentIndex(-1);
        }

        private IQueryable<T> BuildQuery()
        {
            // Build up a dynamic query using Linq Expressions.
            IQueryable<T> query = _items.AsQueryable<T>();

            Expression viewExpression = query.Expression;

            // Start with the filter expression.
            if (Filter != null)
            {
                Expression<Func<T, bool>> filterLambda = item => Filter(item);

                viewExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new[] { query.ElementType },   // TSource.
                    viewExpression,
                    filterLambda);
            }

            // Append the sorting expression(s).
            var sortMethodName = "OrderBy";

            foreach (var sortDescription in SortDescriptions)
            {
                // Create local parameter for lambda - know your closures!
                var sortProperty = typeof(T).GetProperty(sortDescription.PropertyName, BindingFlags.Instance | BindingFlags.Public);
                var itemParamter = Expression.Parameter(typeof(T), "item");

                viewExpression = Expression.Call(
                    typeof(Queryable),
                    sortMethodName + (sortDescription.Direction == ListSortDirection.Ascending ? "" : "Descending"),
                    new[] { query.ElementType, sortProperty.PropertyType },
                    // TSource, TKey
                    viewExpression,
                    Expression.Lambda(
                        Expression.Property(itemParamter, sortProperty),
                        itemParamter));

                // Switch to ThenBy for subsequent sorting.
                sortMethodName = "ThenBy";
            }

            // Append the grouping expression(s).
            // TODO: Support nested grouping properly...
            foreach (var groupDescription in GroupDescriptions.OfType<PropertyGroupDescription>())
            {
                // Create local parameter for lambda - know your closures!
                var groupProperty = typeof(T).GetProperty(groupDescription.PropertyName, BindingFlags.Instance | BindingFlags.Public);
                var itemParameter = Expression.Parameter(typeof(T), "item");

                viewExpression = Expression.Call(
                    typeof(Queryable),
                    "GroupBy",
                    new[] { query.ElementType, typeof(object) },
                    // TSource, TKey
                    viewExpression,
                    Expression.Lambda(
                        Expression.Property(
                            itemParameter,
                            groupProperty),
                        itemParameter));

                // Sort by the grouping key.
                var groupingType = typeof(IGrouping<,>).MakeGenericType(groupProperty.PropertyType, typeof(T));
                var groupingParameter = Expression.Parameter(groupingType, "g");

                viewExpression = Expression.Call(
                    typeof(Queryable),
                    "OrderBy",
                    new[] { groupingType, groupProperty.PropertyType },
                    // TSource, TKey
                    viewExpression,
                    Expression.Lambda(
                        Expression.Property(
                            groupingParameter,
                            "Key"),
                        groupingParameter));

                // Flatten the groups using SelectMany.
                Expression<Func<IGrouping<object, T>, IEnumerable<T>>> smLambda = g => g;

                viewExpression = Expression.Call(
                    typeof(Queryable),
                    "SelectMany",
                    new Type[] { typeof(IGrouping<object, T>), query.ElementType },
                    // TSource, TResult
                    viewExpression,
                    Expression.Lambda(
                        Expression.Convert(groupingParameter, typeof(IEnumerable<T>)),
                        groupingParameter));
            }

            // Return the query.
            return query.Provider.CreateQuery<T>(viewExpression);
        }

        private void OnModelChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    // If the removed item is in the current view, just remove it.
                    foreach (T item in args.OldItems)
                    {
                        if (base.Contains(item))
                            Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    // Could optimize here, but just update the whole view for now...
                    UpdateView();
                    break;
            }
        }

        private object GetPropertyValue(T item, string propertyName)
        {
            var pi = item.GetType().GetProperty(propertyName);
            return pi != null ? pi.GetValue(item, null) : null;
        }

        private bool UpdateCurrentIndex(int index)
        {
            // Calculate new index bounded by -1 and the current collection size.
            int newIndex;
            newIndex = Math.Max(index, -1);
            newIndex = Math.Min(newIndex, Count - 1);

            if (_currentIndex != newIndex)
            {
                if (CurrentChanging != null)
                {
                    CurrentChanging(this, new CurrentChangingEventArgs(false));
                }

                _currentIndex = newIndex;

                if (CurrentChanged != null)
                {
                    CurrentChanged(this, new EventArgs());
                }

                OnPropertyChanged(new PropertyChangedEventArgs("CurrentPosition"));
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentItem"));
                
                _selectNextCommand.RaiseCanExecuteChanged();
                _selectPreviousCommand.RaiseCanExecuteChanged();
            }

            return _currentIndex != -1;
        }
    }
}
