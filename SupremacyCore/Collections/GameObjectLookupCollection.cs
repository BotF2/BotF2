// GameObjectLookupCollection.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.IO.Serialization;

namespace Supremacy.Collections
{
    [Serializable]
    public class GameObjectLookupCollection<TKey, TValue> : 
        ICollection<TValue>,
        IKeyedCollection<TKey, TValue>,
        IOwnedDataSerializableAndRecreatable
        where TKey : class
        where TValue : class
    {
        private readonly Func<TKey, int> _keyLookupResolver;
        private readonly Func<TValue, TKey> _keyResolver;

        private readonly Dictionary<int, int> _lookupDictionary;
        private readonly Func<TValue, int> _valueLookupResolver;
        private readonly Func<int, TValue> _valueResolver;

        public GameObjectLookupCollection(
            [NotNull] Func<TKey, int> keyLookupResolver,
            [NotNull] Func<TValue, TKey> keyResolver,
            [NotNull] Func<TValue, int> valueLookupResolver,
            [NotNull] Func<int, TValue> valueResolver)
        {
            if (keyLookupResolver == null)
                throw new ArgumentNullException("keyLookupResolver");
            if (keyResolver == null)
                throw new ArgumentNullException("keyResolver");
            if (valueLookupResolver == null)
                throw new ArgumentNullException("valueLookupResolver");
            if (valueResolver == null)
                throw new ArgumentNullException("valueResolver");
            _keyLookupResolver = keyLookupResolver;
            _valueResolver = valueResolver;
            _valueLookupResolver = valueLookupResolver;
            _keyResolver = keyResolver;
            _lookupDictionary = new Dictionary<int, int>();
        }

        #region Properties and Indexers
        public TValue this[TKey key]
        {
            get
            {
                var keyLookup = _keyLookupResolver(key);
                int valueLookup;
                if (!_lookupDictionary.TryGetValue(keyLookup, out valueLookup))
                    return null;
                return _valueResolver(valueLookup);
            }
            set { _lookupDictionary[_keyLookupResolver(key)] = _valueLookupResolver(value); }
        }
        #endregion

        #region Implementation of IEnumerable
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var valueLookup in _lookupDictionary.Values)
                yield return _valueResolver(valueLookup);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of ICollection<TValue>
        public void Add([NotNull] TValue item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            _lookupDictionary[_keyLookupResolver(_keyResolver(item))] = _valueLookupResolver(item);
        }

        public void Clear()
        {
            _lookupDictionary.Clear();
        }

        public bool Contains(TValue item)
        {
            return _lookupDictionary.ContainsKey(_keyLookupResolver(_keyResolver(item)));
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _lookupDictionary.Values.Select(_valueResolver).ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(TValue item)
        {
            return _lookupDictionary.Remove(_keyLookupResolver(_keyResolver(item)));
        }

        public int Count
        {
            get { return _lookupDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Contains(TKey key)
        {
            return _lookupDictionary.ContainsKey(_keyLookupResolver(key));
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return EqualityComparer<TKey>.Default; }
        }

        public bool Remove(TKey item)
        {
            return _lookupDictionary.Remove(_keyLookupResolver(item));
        }
        #endregion

        #region Implementation of IOwnedDataSerializable
        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            var keys = reader.ReadOptimizedInt32Array();
            var values = reader.ReadOptimizedInt32Array();
            for (var i = 0; i < keys.Length; i++)
                _lookupDictionary[keys[i]] = values[i];
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            var keys = _lookupDictionary.Keys.Select(id => (int)id).ToArray();
            var values = keys.Select(key => _lookupDictionary[key]).Select(id => (int)id).ToArray();
            writer.WriteOptimized(keys);
            writer.WriteOptimized(values);
        }
        #endregion

        public IEnumerable<TKey> Keys
        {
            get { return _lookupDictionary.Values.Select(_ => _keyResolver(_valueResolver(_))); }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var internalKey = _keyLookupResolver(key);
            int valueId;
            if (!_lookupDictionary.TryGetValue(internalKey, out valueId))
            {
                value = default(TValue);
                return false;
            }
            value = _valueResolver(valueId);
            return true;
        }
    }
}