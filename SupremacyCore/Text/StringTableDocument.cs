// StringTableDocument.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Supremacy.Text
{
    [Serializable]
    public sealed class StringTableDocument
    {
        private StringTableEntryCollection _entries;
        private List<StringTableNode> _nodes;

        public IList<StringTableNode> Nodes
        {
            get { return _nodes; }
        }

        internal StringTableEntryCollection Entries
        {
            get { return _entries; }
        }

        internal StringTableDocument()
        {
            _nodes = new List<StringTableNode>();
            _entries = new StringTableEntryCollection();
        }

        public StringTableEntry FindEntry(string key)
        {
            lock (_entries)
            {
                if (_entries.Contains(key))
                    return _entries[key];
                return null;
            }
        }

        public StringTableEntry FindOrCreateEntry(string key)
        {
            lock (_entries)
            {
                if (_entries.Contains(key))
                    return _entries[key];
                return CreateEntry(key, String.Empty);
            }
        }

        public StringTableEntry CreateEntry(string key, string value)
        {
            lock (_entries)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                if (_entries.Contains(key))
                    throw new InvalidOperationException("entry already exists: " + key);

                StringTableNode keyNode = StringTableNode.CreateKey(key);
                StringTableNode valueNode = StringTableNode.CreateValue(value);
                StringTableEntry entry = new StringTableEntry(keyNode, valueNode);
                StringBuilder whitespace = new StringBuilder();

                if (_nodes.Count > 0)
                {
                    StringTableNode node = _nodes[_nodes.Count - 1];
                    if (node.NodeType == StringTableNodeType.Whitespace)
                    {
                        if (!node.Content.EndsWith(Environment.NewLine + Environment.NewLine))
                        {
                            if (node.Content.EndsWith(Environment.NewLine))
                            {
                                whitespace.AppendLine();
                            }
                            else
                            {
                                whitespace.AppendLine();
                                whitespace.AppendLine();
                            }
                        }
                    }
                    else
                    {
                        whitespace.AppendLine();
                        whitespace.AppendLine();
                    }

                    if (whitespace.Length > 0)
                        _nodes.Add(StringTableNode.CreateWhitespace(whitespace.ToString()));
                }

                _nodes.Add(keyNode);
                _nodes.Add(valueNode);

                try
                {
                    _entries.Add(entry);
                    entry.KeyChanged += Entry_KeyChanged;
                }
                catch
                {
                    Debugger.Break();
                }

                return entry;
            }
        }

        public bool RemoveEntry(string key)
        {
            return RemoveEntry(FindEntry(key));
        }

        public bool RemoveEntry(StringTableEntry entry)
        {
            if ((entry == null) || !_entries.Contains(entry))
                return false;

            if (!Nodes.Contains(entry.KeyNode))
                return false;

            int start = Nodes.IndexOf(entry.KeyNode);
            int range = Nodes.IndexOf(entry.ValueNode) - start;
            for (int i = start;
                 range-- >= 0;
                 Nodes.RemoveAt(i))
            {
                continue;
            }

            _entries.Remove(entry);

            return true;
        }

        internal void OnLoaded()
        {
            foreach (StringTableEntry entry in Entries)
                entry.KeyChanged += Entry_KeyChanged;
        }

        void Entry_KeyChanged(object sender, KeyChangedEventArgs e)
        {
            lock (_entries)
            {
                try
                {
                    _entries.Remove(e.OldKey);
                    _entries.Add((StringTableEntry)sender);
                }
                catch
                {
                    Debugger.Break();
                }
            }
        }
    }

    [Serializable]
    public sealed class StringTableEntryCollection : Collections.KeyedCollectionBase<string, StringTableEntry>
    {
        public StringTableEntryCollection() : base(o => o.Key) { }
    }
}
