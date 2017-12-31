// EmptyKeyedCollection.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Supremacy.Collections
{
    [Serializable]
    public sealed class EmptyKeyedCollection<TKey, TValue> : IIndexedKeyedCollection<TKey, TValue>
    {
        private static EmptyKeyedCollection<TKey, TValue> _instance;

        public static EmptyKeyedCollection<TKey, TValue> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EmptyKeyedCollection<TKey, TValue>();
                return _instance;
            }
        }

        #region Implementation of IEnumerable
        public IEnumerator<TValue> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of IKeyedCollection<TKey,TValue>
        public TValue this[TKey key]
        {
            get { throw new KeyNotFoundException(); }
        }

        public bool Contains(TKey key)
        {
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public TKey GetKeyForItem(TValue item)
        {
            return default(TKey);
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return EqualityComparer<TKey>.Default; }
        }
        #endregion

        #region Implementation of IIndexedKeyedCollection<TKey,TValue>
        public int Count
        {
            get { return 0; }
        }

        TValue IIndexedEnumerable<TValue>.this[int index]
        {
            get { throw new ArgumentOutOfRangeException("index");  }
        }

        public IEnumerable<TKey> Keys
        {
            get { yield break; }
        }

        public bool Contains(TValue value)
        {
            return false;
        }

        public int IndexOf(TValue value)
        {
            return -1;
        }
        #endregion

        #region Implementation of INotifyCollectionChanged
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add {}
            remove {}
        }
        #endregion
    }
}