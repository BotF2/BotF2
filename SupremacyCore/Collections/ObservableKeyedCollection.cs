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
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] items = new TValue[_target.Count];
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
            if (dictionaryCreationThreshold < NeverCreateDictionaryThreshold)
            {
                throw new ArgumentOutOfRangeException(nameof(dictionaryCreationThreshold));
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            if (dictionaryCreationThreshold == NeverCreateDictionaryThreshold)
            {
                dictionaryCreationThreshold = int.MaxValue;
            }

            _keyRetriever = keyRetriever ?? throw new ArgumentNullException(nameof(keyRetriever));
            _keyComparer = comparer;
            _threshold = dictionaryCreationThreshold;
        }

        public IEqualityComparer<TKey> KeyComparer => _keyComparer ?? EqualityComparer<TKey>.Default;

        public virtual TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if ((_keyValueMap != null) && _keyValueMap.TryGetValue(key, out TValue value))
                {
                    return value;
                }

                foreach (TValue item in Items)
                {
                    if (_keyComparer.Equals(GetKeyForItem(item), key))
                    {
                        return item;
                    }
                }

                // if reaching this, next will be the KeyNotFoundException
                int itemsCount = -1;
                if (Items != null) itemsCount = Items.Count();
                string KeyNotFoundString = "KeyNotFoundString, Count of Keys: " + itemsCount;
                Console.WriteLine(KeyNotFoundString);
                foreach (var item in Items)
                {
                    Console.WriteLine(item);
                    KeyNotFoundString += "," + item;
                }
                throw new KeyNotFoundException(KeyNotFoundString);
            }
        }

        public int RemoveRange(IEnumerable<TKey> keys)
        {
            List<TValue> values = new List<TValue>();

            foreach (TKey key in keys)
            {
                if (TryGetValue(key, out TValue value))
                {
                    values.Add(value);
                }
            }

            int removedItemCount = values.Count;

            RemoveRange(values);

            return removedItemCount;
        }

        public bool Contains(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _keyValueMap != null ? _keyValueMap.ContainsKey(key) : Items.Any(o => _keyComparer.Equals(GetKeyForItem(o), key));
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyValueMap != null && _keyValueMap.TryGetValue(key, out value))
            {
                return true;
            }

            foreach (TValue item in Items.Where(o => KeyComparer.Equals(GetKeyForItem(o), key)))
            {
                value = item;
                return true;
            }

            value = default;
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_keyValueMap != null)
            {
                return _keyValueMap.TryGetValue(key, out TValue value) && Remove(value);
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (!_keyComparer.Equals(GetKeyForItem(Items[i]), key))
                {
                    continue;
                }

                RemoveItem(i);
                return true;
            }

            return false;
        }

        public bool Replace(TKey key, TValue newValue)
        {
            return TryGetValue(key, out TValue oldValue) &&
                   Replace(oldValue, newValue);
        }

        public bool Replace(TValue oldValue, TValue newValue)
        {
            int index = IndexOf(oldValue);
            if (index < 0)
            {
                return false;
            }

            SetItem(index, newValue);

            return true;
        }

        protected internal IDictionary<TKey, TValue> KeyValueMap => _keyValueMap;

        protected void ChangeItemKey(TValue item, TKey newKey)
        {
            TKey oldKey = GetKeyForItem(item);

            if (!Contains(item))
            {
                throw new ArgumentException("Item does not exist in the collection.");
            }

            if (_keyComparer.Equals(oldKey, newKey))
            {
                return;
            }

            if (newKey is object)
            {
                AddKey(newKey, item);
            }

            if (oldKey is object)
            {
                RemoveKey(oldKey);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            _keyValueMap?.Clear();

            _keyCount = 0;
        }

        protected internal TKey GetKeyForItem(TValue item)
        {
            return _keyRetriever(item);
        }

        protected override void InsertItem(int index, TValue item)
        {
            TKey key = GetKeyForItem(item);

            if (key is object)
            {
                AddKey(key, item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            TKey key = GetKeyForItem(Items[index]);

            if (key is object)
            {
                RemoveKey(key);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TValue item)
        {
            TKey newKey = GetKeyForItem(item);
            TKey oldKey = GetKeyForItem(Items[index]);

            if (_keyComparer.Equals(oldKey, newKey))
            {
                if (newKey is object && (_keyValueMap != null))
                {
                    _keyValueMap[newKey] = item;
                }
            }
            else
            {
                if (newKey is object)
                {
                    AddKey(newKey, item);
                }

                if (oldKey is object)
                {
                    RemoveKey(oldKey);
                }
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
            {
                return;
            }

            _keyValueMap = new Dictionary<TKey, TValue>(Count, KeyComparer);

            foreach (TValue item in Items)
            {
                AddKey(GetKeyForItem(item), item);
            }
        }

        private void AddKey(TKey key, TValue item)
        {
            if (_keyValueMap != null)
            {
                if (_keyValueMap.ContainsKey(key))
                {
                    OnKeyCollision(key, item);
                }
                else
                {
                    _keyValueMap.Add(key, item);
                }
            }
            else if (_keyCount == _threshold)
            {
                _keyValueMap = CreateKeyToIndexMap();
                _keyValueMap.Add(key, item);
            }
            else
            {
                if (Contains(key))
                {
                    OnKeyCollision(key, item);
                }

                _keyCount++;
            }
        }

        protected virtual void OnKeyCollision(TKey key, TValue item)
        {
            GameLog.Core.General.ErrorFormat("KeyCollision: key={0}; item={1}", key.ToString(), item.ToString());
            throw new ArgumentException("Collection already contains an item with the specified key.");
        }

        private Dictionary<TKey, TValue> CreateKeyToIndexMap()
        {
            return Items.ToDictionary(GetKeyForItem);
        }

        private void RemoveKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_keyValueMap != null)
            {
                _keyValueMap.Remove(key);
            }
            else
            {
                _keyCount--;
            }
        }

        #region Implementation of IIndexedKeyedCollection<TKey,TValue>
        public IEnumerable<TKey> Keys => Items.Select(GetKeyForItem).Where(o => o is object);
        #endregion
    }
}