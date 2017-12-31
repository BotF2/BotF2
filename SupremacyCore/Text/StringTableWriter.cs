// StringTableWriter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;

namespace Supremacy.Text
{
    public static class StringTableWriter
    {
        public static void Write(string fileName, StringTableDocument table)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                Write(writer, table);
            }
        }

        public static void Write(TextWriter writer, StringTableDocument table)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (table == null)
                throw new ArgumentNullException("table");

            for (int i = 0; i < table.Nodes.Count; i++)
            {
                StringTableNode node = table.Nodes[i];
                switch (node.NodeType)
                {
                    case StringTableNodeType.Comment:
                        writer.Write("#");
                        writer.Write(node.Content);
                        break;
                    case StringTableNodeType.Key:
                        if (node.Content.Length > 0)
                        {
                            writer.Write("[");
                            writer.Write(node.Content);
                            writer.Write("]");
                            writer.WriteLine();
                        }
                        else if (i < (table.Nodes.Count - 1))
                        {
                            if (table.Nodes[i + 1].NodeType == StringTableNodeType.Value)
                                i++;
                        }
                        break;
                    default:
                        writer.Write(node.Content);
                        break;
                }
            }
        }
    }
}
