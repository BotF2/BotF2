// StringTableNode.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.IO;

namespace Supremacy.Text
{
    [Serializable]
    public sealed class StringTableNode : INotifyPropertyChanged
    {
        private StringTableNodeType _nodeType;
        private string _content;

        public StringTableNodeType NodeType
        {
            get { return _nodeType; }
            internal set
            {
                _nodeType = value;
                OnPropertyChanged("NodeType");
            }
        }

        public string Content
        {
            get { return _content; }
            internal set
            {
                _content = value;
                OnPropertyChanged("Content");
            }
        }

        private StringTableNode(StringTableNodeType nodeType, string content)
        {
            _nodeType = nodeType;
            _content = content ?? String.Empty;
        }

        private static void CheckForNewlines(string content)
        {
            if (content.Contains(Environment.NewLine))
                throw new ArgumentException("content must not contain newline");
        }

        public static StringTableNode CreateWhitespace()
        {
            return CreateWhitespace(String.Empty);
        }

        public static StringTableNode CreateWhitespace(string content)
        {
            if (content == null)
                content = String.Empty;

            for (int i = 0; i < content.Length; i++)
            {
                if (!Char.IsWhiteSpace(content[i]))
                {
                    throw new ArgumentException(
                        "'content' argument contains non-whitespace characters");
                }
            }

            return new StringTableNode(StringTableNodeType.Whitespace, content);
        }

        public static StringTableNode CreateComment()
        {
            return CreateComment(String.Empty);
        }

        public static StringTableNode CreateComment(string content)
        {
            CheckForNewlines(content);
            return new StringTableNode(StringTableNodeType.Comment, content);
        }

        public static StringTableNode CreateKey()
        {
            return CreateKey(String.Empty);
        }

        public static StringTableNode CreateKey(string content)
        {
            CheckForNewlines(content);
            return new StringTableNode(StringTableNodeType.Key, content);
        }

        public static StringTableNode CreateValue()
        {
            return CreateValue(String.Empty);
        }

        public static StringTableNode CreateValue(string content)
        {
            CheckForNewlines(content);
            return new StringTableNode(StringTableNodeType.Value, content);
        }

        internal void Write(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (NodeType == StringTableNodeType.Comment)
                writer.Write("#");
            else if (NodeType == StringTableNodeType.Key)
                writer.Write("[");
            writer.Write(_content);
            if (NodeType == StringTableNodeType.Key)
                writer.Write("]" + Environment.NewLine);
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public enum StringTableNodeType
    {
        Whitespace,
        Newline,
        Comment,
        Key,
        Value
    }
}
