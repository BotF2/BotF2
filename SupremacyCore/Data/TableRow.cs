// TableRow.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Annotations;

namespace Supremacy.Data
{
    [Serializable]
    public sealed class TableRow<TKey> : IComparable<TableRow<TKey>>, IComparable
    {
        private readonly TKey _key;
        private readonly List<object> _values;
        private string _name;
        private Table<TKey> _owner;

        public TKey Key
        {
            get { return _key; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Table<TKey> Owner
        {
            get { return _owner; }
            internal set { _owner = value; }
        }

        public IList<object> Values
        {
            get { return _values; }
        }

        public TableRow([NotNull] string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _key = ParseKey(name);
            _name = name;
            _owner = null;
            _values = new List<object>();
        }

        private static TKey ParseKey(string keyText)
        {
            if (typeof(TKey).IsEnum)
                return (TKey)Enum.Parse(typeof(TKey), keyText);
            return (TKey)Convert.ChangeType(keyText, typeof(TKey));
        }

        private int ColumnIndex(string columnName)
        {
            var columns = _owner.Columns;

            for (var index = 0; index < columns.Count; index++)
            {
                var column = columns[index];
                if (string.Equals(column.Name, columnName, StringComparison.InvariantCulture))
                    return index;
            }
            
            return -1;
        }

        public string this[int columnIndex]
        {
            get { return Convert.ToString(_values[columnIndex]); }
            set { _values[columnIndex] = _owner.Columns[columnIndex].ParseValue(value); }
        }

        public string this[string columnName]
        {
            get { return this[ColumnIndex(columnName)]; }
            set { this[ColumnIndex(columnName)] = value; }
        }

        public bool TryGetColumn(string columnName, out string value)
        {
            var columnIndex = ColumnIndex(columnName);
            if (columnIndex == -1)
            {
                value = null;
                return false;
            }

            value = _values[columnIndex] as string;

            return (value != null);
        }

        public bool TryGetColumn(int columnIndex, out string value)
        {
            if (columnIndex < 0 || columnIndex >= _values.Count)
            {
                value = null;
                return false;
            }

            value = _values[columnIndex] as string;

            return (value != null);
        }

        #region IComparable<TableRow> Members
        public int CompareTo(TableRow<TKey> other)
        {
            if (other == null)
                return 1;
            return _name.CompareTo(other._name);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(object obj)
        {
            var row = obj as TableRow<TKey>;
            if (row == null)
                return 1;
            return _name.CompareTo(row._name);
        }
        #endregion
    }
}
