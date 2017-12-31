// KeyedCollectionBase.cs
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
using System.Linq;

using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Collections
{
    internal sealed class KeyedCollectionBaseDebugView<TKey, TValue>
    {
        private readonly KeyedCollectionBase<TKey, TValue> _target;

        public KeyedCollectionBaseDebugView(KeyedCollectionBase<TKey, TValue> target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            _target = target;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                var items = new TValue[_target.Count];
                _target.CopyTo(items, 0);
                return items;
            }
        }
    }

    [Serializable]
    [DebuggerTypeProxy(typeof(KeyedCollectionBaseDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public class KeyedCollectionBase<TKey, TValue>
        : CollectionBase<TValue>,
          IIndexedKeyedCollection<TKey, TValue>
    {
        private const int DefaultDictionaryCreationThreshold = 0;
        private const int NeverCreateDictionaryThreshold = -1;

        private IEqualityComparer<TKey> _keyComparer;
        private int _threshold;
        private Dictionary<TKey, TValue> _keyValueMap;
        private int _keyCount;

        private Func<TValue, TKey> _keyRetriever;

        public KeyedCollectionBase(Func<TValue, TKey> keyRetriever)
            : this(keyRetriever, null, DefaultDictionaryCreationThreshold) { }

        public KeyedCollectionBase(Func<TValue, TKey> keyRetriever, IEqualityComparer<TKey> comparer)
            : this(keyRetriever, comparer, DefaultDictionaryCreationThreshold) { }

        public KeyedCollectionBase(Func<TValue, TKey> keyRetriever, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
        {
            if (keyRetriever == null)
                throw new ArgumentNullException("keyRetriever");

            if (dictionaryCreationThreshold < NeverCreateDictionaryThreshold)
                throw new ArgumentOutOfRangeException("dictionaryCreationThreshold");

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            if (dictionaryCreationThreshold == NeverCreateDictionaryThreshold)
                dictionaryCreationThreshold = int.MaxValue;

            _keyRetriever = keyRetriever;
            _keyComparer = comparer;
            _threshold = dictionaryCreationThreshold;
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _keyComparer ?? EqualityComparer<TKey>.Default; }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                if (ReferenceEquals(key, null))
                    throw new ArgumentNullException("key");
                
                TValue value;
                if ((_keyValueMap != null) && _keyValueMap.TryGetValue(key, out value))
                    return value;

                foreach (var item in Items)
                {
                    if (_keyComparer.Equals(GetKeyForItem(item), key))
                        return item;
                }

                throw new KeyNotFoundException();
            }
        }

        public int RemoveRange(IEnumerable<TKey> keys)
        {
            var values = new List<TValue>();
            
            foreach (var key in keys)
            {
                TValue value;
                if (TryGetValue(key, out value))
                    values.Add(value);
            }

            var removedItemCount = values.Count;

            base.RemoveRange(values);

            return removedItemCount;
        }

        public bool Contains(TKey key)
        {
            if (ReferenceEquals(key, null))
                throw new ArgumentNullException("key");
            
            if (_keyValueMap != null)
                return _keyValueMap.ContainsKey(key);

            return Items.Any(o => _keyComparer.Equals(GetKeyForItem(o), key));
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyValueMap != null)
            {
                if (_keyValueMap.TryGetValue(key, out value))
                    return true;
            }

            foreach (var item in Items.Where(o => KeyComparer.Equals(GetKeyForItem(o), key)))
            {
                value = item;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool Remove(TKey key)
        {
            if (ReferenceEquals(key, null))
                throw new ArgumentNullException("key");
            
            if (_keyValueMap != null)
            {
                TValue value;
                if (_keyValueMap.TryGetValue(key, out value))
                    return Remove(value);
                return false;
            }

            for (var i = 0; i < Items.Count; i++)
            {
                if (!_keyComparer.Equals(GetKeyForItem(Items[i]), key))
                    continue;
                RemoveItem(i);
                return true;
            }

            return false;
        }

        public bool Replace(TKey key, TValue newValue)
        {
            TValue oldValue;

            return TryGetValue(key, out oldValue) &&
                   Replace(oldValue, newValue);
        }

        public bool Replace(TValue oldValue, TValue newValue)
        {
            var index = IndexOf(oldValue);
            if (index < 0)
                return false;
            
            SetItem(index, newValue);

            return true;
        }

        protected internal IDictionary<TKey, TValue> KeyValueMap
        {
            get { return _keyValueMap; }
        }

        protected void ChangeItemKey(TValue item, TKey newKey)
        {
            var oldKey = GetKeyForItem(item);
            
            if (!Contains(item))
                throw new ArgumentException("Item does not exist in the collection.");

            if (_keyComparer.Equals(oldKey, newKey))
                return;

            if (!ReferenceEquals(newKey, null))
                AddKey(newKey, item);

            if (!ReferenceEquals(oldKey, null))
                RemoveKey(oldKey);
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            if (_keyValueMap != null)
                _keyValueMap.Clear();

            _keyCount = 0;
        }

        protected internal TKey GetKeyForItem(TValue item)
        {
            return _keyRetriever(item);
        }

        protected override void InsertItem(int index, TValue item)
        {
            var key = GetKeyForItem(item);
            
            if (!ReferenceEquals(key, null))
                AddKey(key, item);

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var key = GetKeyForItem(Items[index]);
            
            if (!ReferenceEquals(key, null))
                RemoveKey(key);

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TValue item)
        {
            var newKey = GetKeyForItem(item);
            var oldKey = GetKeyForItem(Items[index]);

            if (_keyComparer.Equals(oldKey, newKey))
            {
                if (!ReferenceEquals(newKey, null) && (_keyValueMap != null))
                    _keyValueMap[newKey] = item;
            }
            else
            {
                if (!ReferenceEquals(newKey, null))
                    AddKey(newKey, item);
                if (!ReferenceEquals(oldKey, null))
                    RemoveKey(oldKey);
            }

            base.SetItem(index, item);
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write(_threshold);
            writer.WriteObject(_keyComparer);
            writer.WriteObject(_keyRetriever);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _threshold = reader.ReadInt32();
            _keyComparer = reader.Read<IEqualityComparer<TKey>>();
            _keyRetriever = reader.Read<Func<TValue, TKey>>();

            if (Count < _threshold)
                return;

            _keyValueMap = new Dictionary<TKey, TValue>(Count, KeyComparer);

            foreach (var item in Items)
                AddKey(GetKeyForItem(item), item);
        }

        private void AddKey(TKey key, TValue item)
        {
            if (_keyValueMap != null)
            {
                if (_keyValueMap.ContainsKey(key))
                    OnKeyCollision(key, item);
                else
                    _keyValueMap.Add(key, item);
            }
            else if (_keyCount == _threshold)
            {
                _keyValueMap = CreateKeyToIndexMap();
                _keyValueMap.Add(key, item);
            }
            else
            {
                if (Contains(key))
                    OnKeyCollision(key, item);
                _keyCount++;
            }
        }

        protected virtual void OnKeyCollision(TKey key, TValue item)
        {
            GameLog.Print("####### KeyCollision: key={0}; item={1}", key.ToString(), item.ToString());
            throw new ArgumentException("Collection already contains an item with the specified key.");
        }

        private Dictionary<TKey, TValue> CreateKeyToIndexMap()
        {
            return Items.ToDictionary(GetKeyForItem);
        }

        private void RemoveKey(TKey key)
        {
            if (ReferenceEquals(key, null))
                throw new ArgumentNullException("key");
            
            if (_keyValueMap != null)
                _keyValueMap.Remove(key);
            else
                _keyCount--;
        }

        #region Implementation of IIndexedKeyedCollection<TKey,TValue>
        public IEnumerable<TKey> Keys
        {
            get { return Items.Select(GetKeyForItem).Where(o => !ReferenceEquals(o, null)); }
        }
        #endregion
    }
}