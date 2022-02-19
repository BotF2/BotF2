// StringTableEntry.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

namespace Supremacy.Text
{
    [Serializable]
    public class KeyChangedEventArgs : EventArgs
    {
        private string _oldKey;
        private string _newKey;

        public string OldKey => _oldKey;

        public string NewKey => _newKey;

        public KeyChangedEventArgs(string oldKey, string newKey)
        {
            _oldKey = oldKey;
            _newKey = newKey;
        }
    }

    [Serializable]
    public sealed class StringTableEntry : INotifyPropertyChanged
    {
        private StringTableNode _keyNode;
        private StringTableNode _valueNode;

        public event EventHandler<KeyChangedEventArgs> KeyChanged;

        public string Key
        {
            get => _keyNode.Content;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value == string.Empty)
                {
                    throw new ArithmeticException("value cannot be empty");
                }

                if (value != _keyNode.Content)
                {
                    string oldKey = Key;
                    _keyNode.Content = value;
                    OnKeyChanged(oldKey, value);
                    OnPropertyChanged("Key");
                }
            }
        }

        internal StringTableNode KeyNode => _keyNode;

        internal StringTableNode ValueNode => _valueNode;

        public string Value
        {
            get => ValueNode.Content;
            set
            {
                ValueNode.Content = value;
                OnPropertyChanged("Value");
            }
        }

        public StringTableEntry(StringTableNode keyNode, StringTableNode valueNode)
        {
            _keyNode = keyNode ?? throw new ArgumentNullException("keyNode");
            _valueNode = valueNode ?? throw new ArgumentNullException("valueNode");
        }

        private void OnKeyChanged(string oldKey, string newKey)
        {
            KeyChanged?.Invoke(this, new KeyChangedEventArgs(oldKey, newKey));
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
