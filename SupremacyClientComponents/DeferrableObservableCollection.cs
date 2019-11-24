using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Supremacy.Client
{
    /// <summary>
    /// Provides an <see cref="ObservableCollection{T}"/> that is capable of suspending its property change notifications until a bulk update is complete.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    public class DeferrableObservableCollection<T> : ObservableCollection<T>
    {
        private bool _isDirty;
        private readonly bool _useStableSort;
        private readonly IComparer<T> _sortComparer;
        private int _usageCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferrableObservableCollection{T}"/> class.
        /// </summary>
        public DeferrableObservableCollection()
            : this(null, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferrableObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="sortComparer">A comparer used to sort items; otherwise <see langword="null"/>.</param>
        public DeferrableObservableCollection(IComparer<T> sortComparer)
            : this(sortComparer, false)
        {
            // No-op
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferrableObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="sortComparer">A comparer used to sort items; otherwise <see langword="null"/>.</param>
        /// <param name="useStableSort">if set to <c>true</c> then equivalent items will be maintained in the order they are added.</param>
        public DeferrableObservableCollection(IComparer<T> sortComparer, bool useStableSort)
        {
            _sortComparer = sortComparer;
            _useStableSort = useStableSort;
        }

        /// <summary>
        /// Adjusts the index for the specified item, when using a sort comparer.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>An adjusted index for the specified index, so that it is properly sorted.</returns>
        private int AdjustIndex(T item, int index)
        {
            if (null != _sortComparer)
            {
                var count = Count;
                if (count > 0)
                {
                    // Find a new index which sorts the item correctly
                    var newIndex = -1;

                    // Use a different algorithm depending on whether the insertion order should be preserved, both
                    //   for readability and performance
                    if (_useStableSort)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var compare = _sortComparer.Compare(item, this[i]);
                            
                            if (compare > 0)
                                continue;

                            newIndex = i;

                            if (compare != 0)
                                break;

                            if (i >= index)
                                break;
                            
                            if (i >= index || i != count - 1)
                                continue;

                            newIndex++;
                            break;
                        }
                    }
                    else
                    {
                        // To find the proper index, we will divide and conquer. At first, the start/end index will
                        //   encompass the entire list. We will then compare the item in the middle of the range
                        //   and determine if we can insert the new item at that location. If not, then we can use
                        //   the compare results to disregard the items to the left (if compare > 0) or right
                        //   (if compare < 0).
                        var startIndex = 0;
                        var endIndex = count - 1;
                        while (true)
                        {
                            if (startIndex > endIndex)
                                break;

                            // Test index will be the middle slot
                            var testIndex = startIndex + (endIndex - startIndex) / 2;

                            var compare = _sortComparer.Compare(item, this[testIndex]);
                            if (compare < 0)
                            {
                                // New item was less than the Test item. Therefore, it is possible to insert the New item
                                //   at this location, but we will continue to search for a better match.
                                endIndex = testIndex - 1;
                                newIndex = testIndex;
                            }
                            else if (0 == compare)
                            {
                                // New item was equal to the Test item, so we can stop.
                                newIndex = testIndex;
                                break;
                            }
                            else
                            {
                                // New item was greater than the Test item, so we need to continue to look for a match. If
                                //   all else fails, we will insert at the end.
                                startIndex = testIndex + 1;
                            }
                        }
                    }

                    index = (-1 != newIndex) ? newIndex : count;
                }
            }

            return index;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds multiple items to the collection.
        /// </summary>
        /// <param name="items">The collection of items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                    Add(item);
            }
        }

        /// <summary>
        /// Flags that a bulk update is about to begin.
        /// </summary>
        public void BeginUpdate()
        {
            _usageCounter = Math.Min(int.MaxValue - 1, _usageCounter + 1);
        }

        /// <summary>
        /// Flags that a bulk update has ended.
        /// </summary>
        public void EndUpdate()
        {
            if (_usageCounter <= 0)
                return;

            // Decrement the counter
            _usageCounter = Math.Max(0, _usageCounter - 1);

            if (_usageCounter != 0 || !_isDirty)
                return;

            // Flag as not dirty
            _isDirty = false;

            // Raise property changed events since there was a pending suspended change
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            index = AdjustIndex(item, index);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Gets whether there are any suspended property changes.
        /// </summary>
        /// <value>
        /// <c>true</c> if there are any suspended property changes; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirty
        {
            get { return _isDirty; }
        }

        /// <summary>
        /// Gets whether property change notifications are currently suspended.
        /// </summary>
        /// <value>
        /// <c>true</c> if property change notifications are currently suspended; otherwise, <c>false</c>.
        /// </value>
        public bool IsPropertyChangeSuspended
        {
            get { return (_usageCounter > 0); }
        }

        /// <summary>
        /// Raises the <c>CollectionChanged</c> event with the provided arguments.
        /// </summary>
        /// <param name="e">A <see cref="NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsPropertyChangeSuspended)
                _isDirty = true;
            else
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Raises the <c>PropertyChanged</c> event with the provided arguments.
        /// </summary>
        /// <param name="e">A <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (IsPropertyChangeSuspended)
                _isDirty = true;
            else
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            index = AdjustIndex(item, index);
            base.SetItem(index, item);
        }

        /// <summary>
        /// Returns a strongly-typed array containing the items in this collection.
        /// </summary>
        /// <returns>A strongly-typed array containing the items in this collection.</returns>
        public T[] ToArray()
        {
            var result = new T[Count];
            CopyTo(result, 0);
            return result;
        }
    }
}