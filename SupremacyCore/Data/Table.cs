// Table.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Data
{
    [Serializable]
    public sealed class TableColumn
    {
        public TableColumn(string name) : this(name, typeof(string)) { }

        public TableColumn([NotNull] string name, [NotNull] Type valueType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        public string Name { get; }
        public Type ValueType { get; }

        internal object ParseValue(string valueText)
        {
            if (ValueType.IsEnum)
            {
                return Enum.Parse(ValueType, valueText);
            }

            return Convert.ChangeType(valueText, ValueType);
        }
    }

    [Serializable]
    public struct TableValue
    {
        public static readonly TableValue Null = new TableValue(null);

        private readonly string _value;

        public TableValue(string value)
        {
            _value = value;
        }

        public static implicit operator string(TableValue tableValue)
        {
            return tableValue._value;
        }

        public static explicit operator int?(TableValue tableValue)
        {
            return int.TryParse(tableValue._value, out int result) ? (int?)result : null;
        }

        public static explicit operator float?(TableValue tableValue)
        {
            return float.TryParse(tableValue._value, out float result) ? (float?)result : null;
        }

        public static explicit operator double?(TableValue tableValue)
        {
            return double.TryParse(tableValue._value, out double result) ? (double?)result : null;
        }

        public static explicit operator Percentage?(TableValue tableValue)
        {
            return Percentage.TryParse(tableValue._value, out Percentage result) ? (Percentage?)result : null;
        }
    }

    [Serializable]
    public class Table : Table<string>
    {
        public Table(Table<string> baseTable)
        {
            NameInternal = baseTable.NameInternal;
            RowKeyTypeInternal = baseTable.RowKeyTypeInternal;
            ColumnsInternal = baseTable.ColumnsInternal;
            RowsInternal = baseTable.RowsInternal;
        }
        public Table(string name) : base(name) { }
    }

    [Serializable]
    public class Table<TRowKey>
    {
        internal static readonly Regex TableNameRegex = new Regex("[ _a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal static readonly Regex ColumnNameRegex = new Regex("([_a-z][ _a-z0-9]*)(\\<[^>]\\>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        internal static readonly Regex RowHeadingStartRegex = new Regex("RowHeadingsStart(\\<[^>]\\>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        protected readonly object SyncRoot;

        protected internal string NameInternal;
        protected internal Type RowKeyTypeInternal = typeof(string);
        protected internal KeyedCollectionBase<string, TableColumn> ColumnsInternal;
        protected internal KeyedCollectionBase<TRowKey, TableRow<TRowKey>> RowsInternal;

        public string Name
        {
            get
            {
                lock (SyncRoot)
                {
                    return NameInternal ?? string.Empty;
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (!TableNameRegex.IsMatch(value))
                {
                    throw new ArgumentException("Illegal table name: " + value);
                }

                NameInternal = value;
            }
        }

        public IIndexedKeyedCollection<string, TableColumn> Columns
        {
            get
            {
                lock (SyncRoot)
                {
                    return ColumnsInternal;
                }
            }
        }

        public IIndexedKeyedCollection<TRowKey, TableRow<TRowKey>> Rows
        {
            get
            {
                lock (SyncRoot)
                {
                    return RowsInternal;
                }
            }
        }

        public TableRow<TRowKey> this[int rowIndex]
        {
            get
            {
                lock (SyncRoot)
                {
                    return RowsInternal[rowIndex];
                }
            }
        }

        public TableRow<TRowKey> this[TRowKey rowKey]
        {
            get
            {
                lock (SyncRoot)
                {
                    if (RowsInternal.TryGetValue(rowKey, out TableRow<TRowKey> row))
                    {
                        return row;
                    }
                }
                return null;
            }
        }

        public Table()
        {
            SyncRoot = new object();
            ColumnsInternal = new KeyedCollectionBase<string, TableColumn>(o => o.Name, StringComparer.OrdinalIgnoreCase);
            RowsInternal = new KeyedCollectionBase<TRowKey, TableRow<TRowKey>>(o => o.Key);
        }

        public Table(string name)
            : this()
        {
            NameInternal = name;
        }

        private enum ReadState
        {
            TableStart = 0,
            ColumnHeadingsStart = 1,
            RowHeadingsStart = 2,
            Rows = 3,
            TableEnd = 4
        }

        public bool TryGetRow(TRowKey key, out TableRow<TRowKey> row)
        {
            return RowsInternal.TryGetValue(key, out row);
        }

        public bool TryGetValue(TRowKey key, string columnName, out string value)
        {
            if (!TryGetRow(key, out TableRow<TRowKey> row))
            {
                value = null;
                return false;
            }

            return row.TryGetColumn(columnName, out value);
        }

        public bool TryGetValue(TRowKey key, string columnName, out TableValue value)
        {
            if (TryGetRow(key, out TableRow<TRowKey> row) && row.TryGetColumn(columnName, out string stringValue))
            {
                value = new TableValue(stringValue);
                return true;
            }

            value = TableValue.Null;
            return false;
        }

        public bool TryGetValue(TRowKey key, int columnIndex, out string value)
        {
            if (!TryGetRow(key, out TableRow<TRowKey> row))
            {
                value = null;
                return false;
            }

            return row.TryGetColumn(columnIndex, out value);
        }

        public bool TryGetValue(TRowKey key, int columnIndex, out TableValue value)
        {
            if (TryGetRow(key, out TableRow<TRowKey> row) && row.TryGetColumn(columnIndex, out string stringValue))
            {
                value = new TableValue(stringValue);
                return true;
            }

            value = TableValue.Null;
            return false;
        }

        public TableValue GetValue(TRowKey key, string columnName)
        {
            _ = TryGetValue(key, columnName, out TableValue value);
            return value;
        }

        public TableValue GetValue(TRowKey key, int columnIndex)
        {
            _ = TryGetValue(key, columnIndex, out TableValue value);
            return value;
        }

        public void Read(TextReader reader)
        {
            Table<TRowKey> tempTable = ReadFromStream(reader);
            if (tempTable != null)
            {
                lock (SyncRoot)
                {
                    ColumnsInternal = tempTable.ColumnsInternal;
                    RowsInternal = tempTable.RowsInternal;
                }
            }
            else
            {
                lock (SyncRoot)
                {
                    ColumnsInternal.Clear();
                    RowsInternal.Clear();
                }
            }
        }

        public void Write(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            lock (SyncRoot)
            {
                writer.WriteLine("TableStart\t" + Name);
                writer.Write("ColumnHeadingsStart");
                foreach (TableColumn column in ColumnsInternal)
                {
                    writer.Write("\t" + column.Name);
                    if (column.ValueType != typeof(string))
                    {
                        writer.Write("<{0}>", column.ValueType.FullName);
                    }
                }
                writer.WriteLine();
                writer.WriteLine("RowHeadingsStart");
                for (int i = 0; i < RowsInternal.Count; i++)
                {
                    writer.Write(RowsInternal[i].Name);
                    for (int j = 0; j < RowsInternal[i].GetValues().Count; j++)
                    {
                        writer.Write("\t" + RowsInternal[i][j]);
                    }
                    writer.WriteLine();
                }
                writer.WriteLine("TableEnd");
            }
        }

        public static Table<TRowKey> ReadFromStream(TextReader reader)
        {
            Table<TRowKey> table = null;
            ReadState state = ReadState.TableStart;
            string line;
            string tableOut = "";

            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            while (((line = reader.ReadLine()) != null)
                && ((line = line.Trim()).Length > 0))
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] tokens = line.Split(new[] { '\t' }, StringSplitOptions.None);
                if (tokens.Length == 0)
                {
                    continue;
                }

                switch (state)
                {
                    case ReadState.TableStart:
                        string tableName;
                        if (tokens[0] != "TableStart")
                        {
                            throw new TableParseException(
                                "expected TableStart, found \""
                                + tokens[0] + "\"");
                        }
                        if (tokens.Length == 1)
                        {
                            throw new TableParseException(
                                "expected table name, found newline");
                        }
                        if (TableNameRegex.IsMatch(tokens[1]))
                        {
                            tableName = tokens[1];
                            tableOut += tableName + ": ";
                        }
                        else
                        {
                            throw new TableParseException(
                                "illegal table name: \"" + tokens[1]
                                + "\"");
                        }
                        //if ((tokens.Length > 2)
                        //    && !tokens[2].StartsWith("#"))
                        //{
                        //    throw new TableParseException(
                        //        "expected newline, found \"" +
                        //        tokens[2] + "\"");
                        //}
                        table = new Table<TRowKey>(tableName);
                        state = ReadState.ColumnHeadingsStart;
                        break;

                    case ReadState.ColumnHeadingsStart:
                        if (tokens[0] != "ColumnHeadingsStart")
                        {
                            throw new TableParseException(
                                "expected ColumnHeadingsStart, found \""
                                + tokens[0] + "\"");
                        }
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            if (tokens[i].StartsWith("#"))
                            {
                                break;
                            }
                            if (ColumnNameRegex.IsMatch(tokens[i]))
                            {
                                Debug.Assert(table != null);
                                string columnDefinition = tokens[i];
                                Match match = ColumnNameRegex.Match(columnDefinition);
                                Type columnType = typeof(string);
                                if (match.Groups[2].Success)
                                {
                                    columnType = Type.GetType(match.Groups[2].Value);
                                }

                                table.ColumnsInternal.Add(new TableColumn(match.Groups[1].Value, columnType));
                                //tableOut += "Columns: " + new TableColumn(match.Groups[1].Value, columnType).ToString()
                            }
                            else
                            {
                                throw new TableParseException(
                                    "illegal column name: \"" + tokens[i]
                                    + "\"");
                            }
                        }
                        state = ReadState.RowHeadingsStart;
                        break;

                    case ReadState.RowHeadingsStart:
                        Debug.Assert(table != null);
                        Match rowHeadingsStartMatch = RowHeadingStartRegex.Match(tokens[0]);
                        if (!rowHeadingsStartMatch.Success)
                        {
                            throw new TableParseException(
                                "expected RowHeadingsStart, found \""
                                + tokens[0] + "\"");
                        }
                        //if ((tokens.Length > 1)
                        //    && !tokens[1].StartsWith("#"))
                        //{
                        //    throw new TableParseException(
                        //        "expected newline, found \""
                        //        + tokens[1] + "\"");
                        //}
                        state = ReadState.Rows;
                        if (rowHeadingsStartMatch.Groups[1].Success)
                        {
                            table.RowKeyTypeInternal = Type.GetType(rowHeadingsStartMatch.Groups[1].Value);
                        }

                        break;

                    case ReadState.Rows:
                        TableRow<TRowKey> row;
                        if (tokens[0] == "TableEnd")
                        {
                            state = ReadState.TableStart;
                            break;
                        }
                        if (Regex.IsMatch(tokens[0], "[_a-zA-Z0-9]+"))
                        {
                            Debug.Assert(table != null);
                            row = new TableRow<TRowKey>(tokens[0])
                            {
                                Owner = table
                            };
                            table.RowsInternal.Add(row);
                        }
                        else
                        {
                            throw new TableParseException(
                                "illegal row name: \"" + tokens[0]
                                + "\"");
                        }
                        if (((tokens.Length - 1) > table.ColumnsInternal.Count)
                            && !tokens[table.ColumnsInternal.Count].StartsWith("#"))
                        {
                            throw new TableParseException(
                                "row " + row.Name + " has more values than "
                                + "the number of columns in the table");
                        }
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            row.GetValues().Add(tokens[i]);
                            tableOut += tokens[i];
                        }
                        break;
                }

                if (state == ReadState.TableEnd)
                {
                    break;
                }
            }

            if (GameLog.Core.GameInitDataDetails.IsDebugEnabled)
            {
                //GameLog.Client.GameInitData.DebugFormat(tableOut);
                string _values = "";
                string tableString = "";
                try
                {
                    if (table != null)
                    {
                        foreach (TableRow<TRowKey> row in table.Rows)
                        {
                            foreach (object column in row.GetValues())
                            {
                                _values += column + ";";
                            }
                            tableString = table.Name + ";" + _values;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (table != null)
                    {
                        GameLog.Client.GameInitData.DebugFormat("not able to log into Log.txt for {0} exception {0} {1}", table.Name, e.Message, e.StackTrace);
                    }
                }
                if (tableString.Length > 60)
                {
                    tableString = tableString.Substring(0, 60) + "...";
                }

                GameLog.Client.GameInitDataDetails.DebugFormat(tableString);
            }

            return table;
        }
    }
}
