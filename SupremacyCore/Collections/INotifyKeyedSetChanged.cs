// INotifyKeyedSetChanged.cs
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

namespace Supremacy.Collections
{
    /// <summary>
    /// A collection implementing this interface will notify listeners of
    /// dynamic changes, e.g. when items get added and removed or the whole
    /// list is refreshed.
    /// </summary>
    public interface INotifyKeyedSetChanged<TKey, TValue>
        where TValue : class
    {
        /// <summary>
        /// Occurs when the collection changes, either by adding or removing
        /// an item.
        /// </summary>
        event NotifyKeyedSetChangedEventHandler<TKey, TValue> KeyedSetChanged;
    }


    /// <summary>
    /// Describes the action that caused a 
    /// <see cref="E:Supremacy.Collections.INotifyKeyedSetChanged.CollectionChanged"></see>
    /// event.
    /// </summary>
    public enum NotifyKeyedSetChangedAction
    {
        Add,
        Remove,
        Replace,
        ItemChange,
        Reset
    }


    /// <summary>
    /// Represents the method that handles the
    /// <see cref="E:Supremacy.Collections.INotifyKeyedSetChanged.CollectionChanged"></see> 
    /// event.
    /// </summary>
    public delegate void NotifyKeyedSetChangedEventHandler<TKey, TValue>(
        object sender, NotifyKeyedSetChangedEventArgs<TKey, TValue> e)
        where TValue : class;


    /// <summary>
    /// Provides data for the 
    /// <see cref="E:Supremacy.Collections.INotifyKeyedSetChanged.CollectionChanged"></see>
    /// event.
    /// </summary>
    [Serializable]
    public class NotifyKeyedSetChangedEventArgs<TKey, TValue> : EventArgs
        where TValue : class
    {
        private KeyedSetChange<TKey, TValue> _change;

        public KeyedSetChange<TKey, TValue> Change
        {
            get { return _change; }
        }

        public NotifyKeyedSetChangedEventArgs(NotifyKeyedSetChangedAction action)
        {
            _change = new KeyedSetChange<TKey, TValue>(action);
        }

        public NotifyKeyedSetChangedEventArgs(NotifyKeyedSetChangedAction action, TValue item)
        {
            _change = new KeyedSetChange<TKey, TValue>(action, item);
        }

        public NotifyKeyedSetChangedEventArgs(NotifyKeyedSetChangedAction action, TValue item, TKey key)
        {
            _change = new KeyedSetChange<TKey, TValue>(action, item, key);
        }

        public NotifyKeyedSetChangedEventArgs(NotifyKeyedSetChangedAction action, TValue newItem, TValue oldItem)
        {
            _change = new KeyedSetChange<TKey, TValue>(action, newItem, oldItem);
        }

        public NotifyKeyedSetChangedEventArgs(NotifyKeyedSetChangedAction action, TValue newItem, TValue oldItem, TKey key)
        {
            _change = new KeyedSetChange<TKey, TValue>(action, newItem, oldItem, key);
        }
    }

    [Serializable]
    public class KeyedSetChange<TKey, TValue> where TValue : class
    {
        #region Fields
        private NotifyKeyedSetChangedAction _action;
        private bool _hasOldKey;
        private bool _hasNewKey;
        private TKey _oldKey;
        private TKey _newKey;
        private TValue _oldItem;
        private TValue _newItem;
        #endregion

        #region Properties
        public NotifyKeyedSetChangedAction Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public bool HasOldKey
        {
            get { return _hasOldKey; }
        }

        public bool HasNewKey
        {
            get { return _hasNewKey; }
        }

        public TKey OldKey
        {
            get { return _oldKey; }
        }

        public TKey NewKey
        {
            get { return _newKey; }
        }

        public TValue OldItem
        {
            get { return _oldItem; }
        }

        public TValue NewItem
        {
            get { return _newItem; }
        }
        #endregion

        #region Constructors
        public KeyedSetChange(NotifyKeyedSetChangedAction action)
        {
            if (action != NotifyKeyedSetChangedAction.Reset)
            {
                throw new ArgumentException(
                    "action must be Reset to use this constructor");
            }
            InitializeAdd(action, null, default(TKey), false);
        }

        public KeyedSetChange(NotifyKeyedSetChangedAction action, TValue item)
        {
            if ((action != NotifyKeyedSetChangedAction.Add)
                && (action != NotifyKeyedSetChangedAction.Remove)
                && (action != NotifyKeyedSetChangedAction.ItemChange)
                && (action != NotifyKeyedSetChangedAction.Reset))
            {
                throw new ArgumentException(
                    "action must be Add|Remove|ItemChange|Reset to use this constructor");
            }
            if (action == NotifyKeyedSetChangedAction.Reset)
            {
                InitializeAdd(action, null, default(TKey), false);
            }
            else if (action == NotifyKeyedSetChangedAction.ItemChange)
            {
                InitializeRemove(action, item, default(TKey), false);
            }
            else
            {
                InitializeAddOrRemove(action, item, default(TKey), false);
            }
        }

        public KeyedSetChange(NotifyKeyedSetChangedAction action, TValue item, TKey key)
        {
            if ((action != NotifyKeyedSetChangedAction.Add)
                && (action != NotifyKeyedSetChangedAction.Remove)
                && (action != NotifyKeyedSetChangedAction.ItemChange)
                && (action != NotifyKeyedSetChangedAction.Reset))
            {
                throw new ArgumentException(
                    "action must be Add|Remove|ItemChange|Reset to use this constructor");
            }
            if (action == NotifyKeyedSetChangedAction.Reset)
            {
                InitializeAdd(action, null, default(TKey), false);
            }
            else if (action == NotifyKeyedSetChangedAction.ItemChange)
            {
                InitializeRemove(action, item, key, true);
            }
            else
            {
                InitializeAddOrRemove(action, item, key, true);
            }
        }

        public KeyedSetChange(NotifyKeyedSetChangedAction action, TValue newItem, TValue oldItem)
        {
            if (action != NotifyKeyedSetChangedAction.Replace)
            {
                throw new ArgumentException(
                    "action must be Replcae to use this constructor");
            }
            else
            {
                InitializeReplace(action, oldItem, newItem, default(TKey), false);
            }
        }

        public KeyedSetChange(NotifyKeyedSetChangedAction action, TValue newItem, TValue oldItem, TKey key)
        {
            if (action != NotifyKeyedSetChangedAction.Replace)
            {
                throw new ArgumentException(
                    "action must be Replcae to use this constructor");
            }
            else
            {
                InitializeReplace(action, oldItem, newItem, key, true);
            }
        }
        #endregion

        #region Methods
        private void InitializeAdd(NotifyKeyedSetChangedAction action, TValue item, TKey key, bool hasKey)
        {
            _action = action;
            _newKey = key;
            _hasNewKey = hasKey;
            _newItem = item;
        }

        private void InitializeAddOrRemove(NotifyKeyedSetChangedAction action, TValue item, TKey key, bool hasKey)
        {
            if (action == NotifyKeyedSetChangedAction.Add)
            {
                InitializeAdd(action, item, key, hasKey);
            }
            else if (action == NotifyKeyedSetChangedAction.Remove)
            {
                InitializeRemove(action, item, key, hasKey);
            }
            else
            {
                throw new ArgumentException("action must be Add|Remove");
            }
        }

        private void InitializeRemove(NotifyKeyedSetChangedAction action, TValue item, TKey key, bool hasKey)
        {
            _action = action;
            _oldKey = key;
            _hasOldKey = hasKey;
            _oldItem = item;
        }

        private void InitializeReplace(NotifyKeyedSetChangedAction action, TValue oldItem, TValue newItem, TKey key, bool hasKey)
        {
            InitializeAdd(action, newItem, key, hasKey);
            InitializeRemove(action, oldItem, key, hasKey);
        }
        #endregion
    }

    [Serializable]
    public class KeyedSetChangeSet<TKey, TValue> : 
        IEnumerable<KeyedSetChange<TKey, TValue>>
        where TValue : class
    {
        private Dictionary<TKey, KeyedSetChange<TKey, TValue>> itemChanges;
        private List<KeyedSetChange<TKey, TValue>> setChanges;

        public KeyedSetChangeSet()
        {
            itemChanges = new Dictionary<TKey,KeyedSetChange<TKey,TValue>>();
            setChanges = new List<KeyedSetChange<TKey,TValue>>();
        }

        public void Add(KeyedSetChange<TKey, TValue> change)
        {
            if (change == null)
            {
                throw new ArgumentNullException("change");
            }
            if (change.Action == NotifyKeyedSetChangedAction.ItemChange)
            {
                itemChanges[change.OldKey] = change;
            }
            else
            {
                setChanges.Add(change);
            }
        }

        public void Clear()
        {
            itemChanges.Clear();
            setChanges.Clear();
        }

        #region IEnumerable<KeyedSetChangeSet<TKey,TValue>> Members
        public IEnumerator<KeyedSetChange<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyedSetChange<TKey, TValue> change in setChanges)
                yield return change;
            foreach (KeyedSetChange<TKey, TValue> change in itemChanges.Values)
                yield return change;
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
