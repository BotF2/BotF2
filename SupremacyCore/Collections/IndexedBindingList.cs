// IndexedBindingList.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace Supremacy.Collections
{
    [Serializable]
    public class IndexedBindingList<T> : BindingList<T>, IBindingList, IDeserializationCallback
        where T : class
    {
        #region Fields
        private readonly ArrayList<string> _indexedProperties;
        private readonly object _syncLock;

        [NonSerialized]
        private Dictionary<string, Index<T>> _indexes;
        private int _maxIndexSize = 32;
        private PropertyComparer<T> _sortComparer;
        #endregion

        #region Constructors
        public IndexedBindingList()
        {
            _syncLock = new object();
            _indexedProperties = new ArrayList<string>();
            _indexes = new Dictionary<string, Index<T>>();
        }
        #endregion

        #region Properties
        public int MaxIndexSize
        {
            get { return _maxIndexSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("must be a positive integer", "value");
                _maxIndexSize = value;
            }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                if (_sortComparer != null)
                {
                    return _sortComparer.Direction;
                }
                return ListSortDirection.Ascending;
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                if (_sortComparer != null)
                {
                    return _sortComparer.Property;
                }
                return null;
            }
        }

        protected override bool IsSortedCore
        {
            get { return (_sortComparer != null); }
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }
        #endregion

        #region IBindingList Members
        public virtual ICollection<T> Find(PropertyDescriptor property, object value)
        {
            return FindAllCore(property, value);
        }

        public void AddIndex(PropertyDescriptor property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            lock (_syncLock)
            {
                if (!_indexedProperties.Contains(property.Name))
                {
                    _indexedProperties.Add(property.Name);
                    _indexes.Add(property.Name, new Index<T>(property));
                }
            }

            OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, property));
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            lock (_syncLock)
            {
                if (_indexedProperties.Remove(property.Name))
                    _indexes.Remove(property.Name);
            }

            OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, property));
        }
        #endregion

        #region IDeserializationCallback Members
        public void OnDeserialization(object sender)
        {
            RestorePropertyChangedHooks();
            RebuildIndexes();
        }

        private void RestorePropertyChangedHooks()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                SetItem(i, Items[i]);
            }
        }
        #endregion

        #region Methods
        protected PropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            if (_indexes.ContainsKey(propertyName))
            {
                return _indexes[propertyName].Property;
            }
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            if (property != null)
            {
                return TypeDescriptor.CreateProperty(
                    property.ReflectedType,
                    property.Name,
                    property.PropertyType);
            }
            return null;
        }

        public void AddIndex(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");
            PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(propertyName);
            if (propertyDescriptor != null)
                AddIndex(propertyDescriptor);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            lock (_syncLock)
            {
                foreach (T item in items)
                    Add(item);
            }
        }

        public virtual ICollection<T> Find(string propertyName, object value)
        {
            PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(propertyName);
            if (propertyName != null)
            {
                return FindAllCore(propertyDescriptor, value);
            }
            return new List<T>();
        }

        public void ClearIndexes()
        {
            lock(_syncLock)
            {
                _indexedProperties.Clear();
                _indexes.Clear();
            }
        }

        public void Reset()
        {
            lock (_syncLock)
            {
                ClearIndexes();
                ClearItems();
            }
        }

        public void RemoveIndex(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");
            PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(propertyName);
            if (propertyDescriptor != null)
                RemoveIndex(propertyDescriptor);
        }

        private int GetIndexableHashCode(object value)
        {
            if (value == null)
                return -1;
            return (Math.Abs(value.GetHashCode()) % _maxIndexSize);
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            List<T> items = Items as List<T>;
            if (items != null)
            {
                _sortComparer = new PropertyComparer<T>(property, direction);
                items.Sort(_sortComparer);
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        protected virtual IList<T> FindAllCore(PropertyDescriptor property, object value)
        {
            List<T> results = new List<T>();
            int hashCode = GetIndexableHashCode(value);
            if (hashCode >= 0)
            {
                if (_indexes.ContainsKey(property.Name))
                {
                    Index<T> index = _indexes[property.Name];
                    foreach (T item in index.LookupTable[hashCode])
                    {
                        if (property.GetValue(item) == value)
                            results.Add(item);
                    }
                }
                else
                {
                    foreach (T item in Items)
                    {
                        object propertyValue = property.GetValue(item);
                        if (propertyValue == value)
                            results.Add(item);
                    }
                }
            }
            return results;
        }

        protected override int FindCore(PropertyDescriptor property, object value)
        {
            IList<T> results = FindAllCore(property, value);
            if (results.Count > 0)
                return Items.IndexOf(results[0]);
            return -1;
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if ((e.ListChangedType == ListChangedType.ItemChanged)
                || (e.ListChangedType == ListChangedType.ItemDeleted)
                    || (e.ListChangedType == ListChangedType.ItemAdded))
            {
                lock (_syncLock)
                {
                    if ((e.PropertyDescriptor != null) && _indexes.ContainsKey(e.PropertyDescriptor.Name))
                    {
                        UpdateIndex(_indexes[e.PropertyDescriptor.Name], Items[e.NewIndex]);
                    }
                    else
                    {
                        foreach (Index<T> index in _indexes.Values)
                        {
                            UpdateIndex(index, Items[e.NewIndex]);
                        }
                    }
                }
            }
            base.OnListChanged(e);
        }

        protected void RebuildIndexes()
        {
            lock (_syncLock)
            {
                _indexes = new Dictionary<string, Index<T>>();
                foreach (string propertyName in _indexedProperties)
                {
                    PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(propertyName);
                    if (propertyDescriptor != null)
                    {
                        _indexes.Add(propertyDescriptor.Name, new Index<T>(propertyDescriptor));
                        foreach (T item in Items)
                        {
                            UpdateIndex(_indexes[propertyDescriptor.Name], item);
                        }
                    }
                }
            }
        }

        protected override void RemoveSortCore()
        {
            _sortComparer = null;
        }

        protected void UpdateIndex(Index<T> index, T item)
        {
            UpdateIndex(index, item, true);
        }

        protected void UpdateIndex(Index<T> index, T item, bool removeOld)
        {
            object newValue = index.Property.GetValue(item);

            if (removeOld)
            {
                foreach (List<T> lookupTable in index.LookupTable.Values)
                {
                    lookupTable.Remove(item);
                }
            }

            if (newValue != null)
            {
                int hashCode = GetIndexableHashCode(newValue);
                if (hashCode >= 0)
                {
                    index.EnsureList(hashCode);
                    index.LookupTable[hashCode].Add(item);
                }
            }
        }
        #endregion

        #region Index<T> Class
        #pragma warning disable 693
        protected sealed class Index<T>

        {
            #region Fields
            private readonly Dictionary<int, List<T>> _lookupTable;
            private readonly PropertyDescriptor _property;
            #endregion

            #region Constructors
            public Index(PropertyDescriptor property)
            {
                _property = property;
                _lookupTable = new Dictionary<int, List<T>>();
            }
            #endregion

            #region Properties
            public Dictionary<int, List<T>> LookupTable
            {
                get { return _lookupTable; }
            }

            public PropertyDescriptor Property
            {
                get { return _property; }
            }
            #endregion

            #region Methods
            public void EnsureList(int index)
            {
                if (!_lookupTable.ContainsKey(index))
                    _lookupTable.Add(index, new List<T>());
            }
            #endregion
        }
        #pragma warning restore 693
        #endregion

        #region PropertyComparer<T> Class
        #pragma warning disable 693
        protected sealed class PropertyComparer<T> : IComparer<T>
        {
            #region Fields
            private readonly ListSortDirection _direction;
            private readonly PropertyDescriptor _property;
            #endregion

            #region Constructors
            public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                _property = property;
                _direction = direction;
            }
            #endregion

            #region Properties
            public PropertyDescriptor Property
            {
                get { return _property; }
            }

            public ListSortDirection Direction
            {
                get { return _direction; }
            }
            #endregion

            #region IComparer<T> Members
            public int Compare(T x, T y)
            {
                int result = Comparer.DefaultInvariant.Compare(
                    _property.GetValue(x),
                    _property.GetValue(y));
                if (_direction == ListSortDirection.Descending)
                    result *= -1;
                return result;
            }
            #endregion
        }
        #pragma warning restore 693
        #endregion
    }
}