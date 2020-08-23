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
        private readonly List<object> _values;

        public TKey Key { get; }

        public string Name { get; set; }

        public Table<TKey> Owner { get; internal set; }

        public IList<object> GetValues()
        {
            return _values;
        }

        public TableRow([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Key = ParseKey(name);
            Name = name;
            Owner = null;
            _values = new List<object>();
        }

        private static TKey ParseKey(string keyText)
        {
            if (typeof(TKey).IsEnum)
            {
                return (TKey)Enum.Parse(typeof(TKey), keyText);
            }

            return (TKey)Convert.ChangeType(keyText, typeof(TKey));
        }

        private int ColumnIndex(string columnName)
        {
            Collections.IIndexedKeyedCollection<string, TableColumn> columns = Owner.Columns;

            for (int index = 0; index < columns.Count; index++)
            {
                TableColumn column = columns[index];
                if (string.Equals(column.Name, columnName, StringComparison.InvariantCulture))
                {
                    return index;
                }
            }

            return -1;
        }

        public string this[int columnIndex]
        {
            get => Convert.ToString(_values[columnIndex]);
            set => _values[columnIndex] = Owner.Columns[columnIndex].ParseValue(value);
        }

        public string this[string columnName]
        {
            get => this[ColumnIndex(columnName)];
            set => this[ColumnIndex(columnName)] = value;
        }

        public bool TryGetColumn(string columnName, out string value)
        {
            int columnIndex = ColumnIndex(columnName);
            if (columnIndex == -1)
            {
                value = null;
                return false;
            }

            value = _values[columnIndex] as string;

            return value != null;
        }

        public bool TryGetColumn(int columnIndex, out string value)
        {
            if (columnIndex < 0 || columnIndex >= _values.Count)
            {
                value = null;
                return false;
            }

            value = _values[columnIndex] as string;

            return value != null;
        }

        #region IComparable<TableRow> Members
        public int CompareTo(TableRow<TKey> other)
        {
            if (other == null)
            {
                return 1;
            }

            return Name.CompareTo(other.Name);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(object obj)
        {
            TableRow<TKey> row = obj as TableRow<TKey>;
            if (row == null)
            {
                return 1;
            }

            return Name.CompareTo(row.Name);
        }
        #endregion
    }
}
