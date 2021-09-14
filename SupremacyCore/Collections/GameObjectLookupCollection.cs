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
            _keyLookupResolver = keyLookupResolver ?? throw new ArgumentNullException(nameof(keyLookupResolver));
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
            _valueLookupResolver = valueLookupResolver ?? throw new ArgumentNullException(nameof(valueLookupResolver));
            _keyResolver = keyResolver ?? throw new ArgumentNullException(nameof(keyResolver));
            _lookupDictionary = new Dictionary<int, int>();
        }

        #region Properties and Indexers
        public TValue this[TKey key]
        {
            get
            {
                int keyLookup = _keyLookupResolver(key);
                return !_lookupDictionary.TryGetValue(keyLookup, out int valueLookup) ? null : _valueResolver(valueLookup);
            }
            set => _lookupDictionary[_keyLookupResolver(key)] = _valueLookupResolver(value);
        }
        #endregion

        #region Implementation of IEnumerable
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (int valueLookup in _lookupDictionary.Values)
            {
                yield return _valueResolver(valueLookup);
            }
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
            {
                throw new ArgumentNullException(nameof(item));
            }

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

        public int Count => _lookupDictionary.Count;

        public bool IsReadOnly => false;

        public bool Contains(TKey key)
        {
            return _lookupDictionary.ContainsKey(_keyLookupResolver(key));
        }

        public IEqualityComparer<TKey> KeyComparer => EqualityComparer<TKey>.Default;

        public bool Remove(TKey item)
        {
            return _lookupDictionary.Remove(_keyLookupResolver(item));
        }
        #endregion

        #region Implementation of IOwnedDataSerializable
        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            int[] keys = reader.ReadOptimizedInt32Array();
            int[] values = reader.ReadOptimizedInt32Array();
            for (int i = 0; i < keys.Length; i++)
            {
                _lookupDictionary[keys[i]] = values[i];
            }
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            int[] keys = _lookupDictionary.Keys.Select(id => id).ToArray();
            int[] values = keys.Select(key => _lookupDictionary[key]).Select(id => id).ToArray();
            writer.WriteOptimized(keys);
            writer.WriteOptimized(values);
        }
        #endregion

        public IEnumerable<TKey> Keys => _lookupDictionary.Values.Select(_ => _keyResolver(_valueResolver(_)));

        public bool TryGetValue(TKey key, out TValue value)
        {
            int internalKey = _keyLookupResolver(key);
            if (!_lookupDictionary.TryGetValue(internalKey, out int valueId))
            {
                value = default;
                return false;
            }
            value = _valueResolver(valueId);
            return true;
        }
    }
}