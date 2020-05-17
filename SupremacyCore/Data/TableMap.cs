// TableMap.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Supremacy.Data
{
    [Serializable]
    public sealed class TableMap : ICollection<Table>, IEnumerable<Table>
    {
        private readonly OrderedDictionary _tables;
        private readonly object _syncRoot;

        public Table this[string tableName]
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_tables.Contains(tableName))
                        return (Table)_tables[tableName];
                    return null;
                }
            }
            set { lock (_syncRoot) _tables[tableName] = value; }
        }

        public TableMap()
        {
            _syncRoot = new object();
            _tables = new OrderedDictionary();
        }

        public bool TryGetValue(string tableName, string rowKey, string columnKey, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];

            if (GameLog.Core.GameInitData.IsDebugEnabled == true)
                GameLog.Core.GameInitData.DebugFormat("tableName = {0} for string rowKey, string columnKey, out string value", tableName);

            TableRow<string> row;
            if (!table.TryGetRow(rowKey, out row))
                return false;

            return row.TryGetColumn(columnKey, out value);
        }

        public bool TryGetValue<TRowKey>(string tableName, TRowKey rowKey, string columnKey, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];
            GameLog.Core.GameInitData.DebugFormat("tableName = {0} for TRowKey rowKey, string columnKey, out string value", tableName);

            var typedTable = table as Table<TRowKey>;
            if (typedTable == null)
                return false;

            TableRow<TRowKey> row;
            if (!typedTable.TryGetRow(rowKey, out row))
                return false;

            return row.TryGetColumn(columnKey, out value);
        }
        
        public bool TryGetValue(string tableName, string rowKey, int columnIndex, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];

            // used a lot of times
            // MessageDialogButtons out of EnumStrings.txt is looking for translation for OK / CANCEL and so on ... used thousands times
            // TechCategory         out of EnumStrings.txt is looking for translation for research fields like Weapons ...
            // SitRepCategory       out of EnumStrings.txt is looking for translation for research fields like First Contact...

            if (tableName != "MessageDialogButtons" && tableName != "TechCategory" && tableName != "SitRepCategory")  // this one in EnumStrings.txt is looking for translation for OK / CANCEL and so on ... used thousands times
                GameLog.Core.GameInitData.DebugFormat("tableName = {0} for string rowKey, int columnIndex, out string value", tableName);

            TableRow<string> row;
            if (!table.TryGetRow(rowKey, out row))
                return false;

            return row.TryGetColumn(columnIndex, out value);
        }

        public bool TryGetValue<TRowKey>(string tableName, TRowKey rowKey, int columnIndex, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];
            GameLog.Core.GameInitData.DebugFormat("tableName = {0} for TRowKey rowKey, int columnIndex, out string value", tableName);

            var typedTable = table as Table<TRowKey>;
            if (typedTable == null)
                return false;

            TableRow<TRowKey> row;
            if (!typedTable.TryGetRow(rowKey, out row))
                return false;

            return row.TryGetColumn(columnIndex, out value);
        }
        
        public bool TryGetValue(string tableName, int rowIndex, string columnKey, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];
            GameLog.Core.GameInitData.DebugFormat("tableName = {0} for int rowIndex, string columnKey, out string value", tableName);

            if (rowIndex < 0 || rowIndex >= table.RowsInternal.Count)
                return false;

            var row = table.RowsInternal[rowIndex];
            if (row == null)
                return false;

            return row.TryGetColumn(columnKey, out value);
        }
        
        public bool TryGetValue(string tableName, int rowIndex, int columnIndex, out string value)
        {
            value = null;

            Table table;
            if (!_tables.Contains(tableName))
            {
                return false;
            }
            table = (Table)_tables[tableName];
            GameLog.Core.GameInitData.DebugFormat("tableName = {0} for int rowIndex, int columnIndex, out string value", tableName);

            var row = table.RowsInternal[rowIndex];
            if (row == null)
                return false;

            return row.TryGetColumn(columnIndex, out value);
        }

        public void Write(string path)
        {
            var writer = new StreamWriter(path);
            Write(writer);
            GameLog.Core.GameInitData.DebugFormat("writer = {0}", writer);
            writer.Flush();
            writer.Close();
        }

        public static TableMap ReadFromFile(string path)
        {
            GameLog.Client.GameInitData.DebugFormat("reading {0}", path);
            TableMap map = new TableMap();
            map.Read(path);
            return map;
        }

        public void Read(string path)
        {
            FileStream stream = File.Open(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream);
            Read(reader);
            reader.Close();
        }

        public void Write(TextWriter writer)
        {
            lock (_syncRoot)
            {
                foreach (Table table in _tables.Values)
                {
                    table.Write(writer);
                    writer.WriteLine();
                }
            }
        }

        public void Read(TextReader reader)
        {
            lock (_syncRoot)
            {
                Table<string> newTable;
                while ((newTable = Table<string>.ReadFromStream(reader)) != null)
                {
                    _tables.Add(newTable.Name, new Table(newTable));
                    //GameLog.Core.GameInitData.DebugFormat("newTable.Name = {0}", newTable.Name);

                    //int i = 0;
                    string tableContentString = newTable.Name + ": ";

                    
                    //int tableRowMax = _tables.newTable.Rows.Count;
                    ////for (int i = 0; Table(newTable); i++)
                    //    foreach (var entry in newTable.Rows.Count)
                    //        for (int i = 0; Table(newTable); i++)
                    //        {
                    //            for (int j = 0; newTable.Columns.Count; j++)
                    //            {
                    //                tableContentString += newTable.Rows[j].Values;
                    //                //i = i + 1;
                    //            }
                    //        }
                    //GameLog.Core.GameInitData.DebugFormat("newTable.Name = {0}", tableContentString);
                }

            }
        }

        #region IEnumerable<Table> Members
        public IEnumerator<Table> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<Table>)_tables.Values.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tables.Values.GetEnumerator();
        }
        #endregion

        #region ICollection<Table> Members

        public void Add(Table item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(Table item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(Table[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsReadOnly
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Remove(Table item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
