// StringTableReader.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;
using System.IO;
using System.Text;

namespace Supremacy.Text
{
    public static class StringTableReader
    {
        private static bool IsCrOrLf(char c)
        {
            return (c == '\r') || (c == '\n');
        }

        public static StringTableDocument Read(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            using (StreamReader reader = new StreamReader(fileName))
            {
                return Read(reader);
            }
        }

        public static StringTableDocument Read(TextReader textReader)
        {
            StringTableDocument doc = new StringTableDocument();
            StringTableNode previousNode = null;
            StringTableNode currentNode = null;
            StringBuilder buffer = new StringBuilder();
            string inFile = textReader.ReadToEnd();

            for (int i = 0; i < inFile.Length; i++)
            {
                char c = inFile[i];
                bool done = false;

                if (currentNode == null)
                {
                    if ((previousNode != null) && (previousNode.NodeType == StringTableNodeType.Key))
                    {
                        currentNode = StringTableNode.CreateValue();
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        currentNode = StringTableNode.CreateWhitespace();
                    }
                    else if (c == '#')
                    {
                        currentNode = StringTableNode.CreateComment();
                        continue;
                    }
                    else if (c == '[')
                    {
                        currentNode = StringTableNode.CreateKey();
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (IsCrOrLf(c) && (buffer.Length == 0) && (previousNode != null)
                    && (previousNode.NodeType == StringTableNodeType.Key))
                {
                    continue;
                }

                switch (currentNode.NodeType)
                {
                    case StringTableNodeType.Whitespace:
                        if (char.IsWhiteSpace(c))
                        {
                            _ = buffer.Append(c);
                        }
                        else
                        {
                            done = true;
                        }
                        break;


                    case StringTableNodeType.Comment:
                        if (!IsCrOrLf(c))
                        {
                            _ = buffer.Append(c);
                        }
                        else
                        {
                            done = true;
                        }
                        break;

                    case StringTableNodeType.Key:
                        if (c != ']')
                        {
                            _ = buffer.Append(c);
                        }
                        else
                        {
                            ++i;
                            done = true;
                        }
                        break;

                    case StringTableNodeType.Value:
                        if (((buffer.ToString().Trim().Length == 0) || IsWhitespace(buffer.ToString()))
                            && ((c == '[') || (c == '#')))
                        {
                            --i;
                            done = true;
                        }
                        else
                        {
                            try
                            {
                                string nextChars = inFile.Substring(i, 4);
                                if (nextChars == "\r\n\r\n")
                                {
                                    done = true;
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                GameLog.Core.GameData.Error(e);
                            }

                            _ = buffer.Append(c);
                        }
                        break;

                    default:
                        /* This should never happen. */
                        break;
                }

                /* Value nodes are the only design of nodes that are allowed to be empty. */
                if (done && ((buffer.Length > 0) || (currentNode.NodeType == StringTableNodeType.Value)))
                {
                    if (currentNode.NodeType == StringTableNodeType.Value)
                    {
                        currentNode.Content = buffer.ToString().Trim();
                    }
                    else
                    {
                        currentNode.Content = buffer.ToString();
                    }
                    doc.Nodes.Add(currentNode);
                    if (currentNode.NodeType == StringTableNodeType.Value)
                    {
                        StringTableEntry entry = new StringTableEntry(
                            previousNode,
                            currentNode);
                        if (!doc.Entries.Contains(entry.Key))
                        {
                            doc.Entries.Add(entry);
                        }
                    }
                    previousNode = currentNode;
                    currentNode = null;
                    buffer = new StringBuilder();
                    --i;
                }
            }

            if (buffer.Length > 0)
            {
                if (currentNode.NodeType == StringTableNodeType.Value)
                {
                    currentNode.Content = buffer.ToString().Trim();
                }
                else
                {
                    currentNode.Content = buffer.ToString();
                }
                if ((currentNode.Content.Length > 0)
                    || (currentNode.NodeType == StringTableNodeType.Value))
                {
                    doc.Nodes.Add(currentNode);
                    if (currentNode.NodeType == StringTableNodeType.Value)
                    {
                        StringTableEntry entry = new StringTableEntry(
                            previousNode,
                            currentNode);
                        doc.Entries.Add(entry);
                    }
                    previousNode = currentNode;
                    currentNode = null;
                    buffer = new StringBuilder();
                }
            }

            doc.OnLoaded();

            return doc;
        }

        private static bool IsWhitespace(string buffer)
        {
            foreach (char c in buffer)
            {
                if (!(char.IsWhiteSpace(c) || IsCrOrLf(c)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
