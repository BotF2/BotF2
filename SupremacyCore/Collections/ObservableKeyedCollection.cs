// File:ObservableKeyedCollection.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.IO.Serialization;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private string _text;

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



            //_keyRetriever = keyRetriever ?? throw new ArgumentNullException(nameof(keyRetriever));
            _keyRetriever = keyRetriever ?? null;
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
                    _text = "Step_0873: Searched Key was null";
                    Console.WriteLine(_text);
                    GameLog.Client.GeneralDetails.DebugFormat(_text);
                    throw new ArgumentNullException(nameof(key));
                }

                // next works, but it is too often
                //_text = "Step_0874: working on key > " + key.ToString();
                //Console.WriteLine(_text);
                //GameLog.Core.General.ErrorFormat(_text);

                //searching for crashes
                if (key.ToString() == "-1")
                {
                    _text = "Step_0875: Searched Key was -1, sometimes this crashes";
                    Console.WriteLine(_text);
                    GameLog.Client.GeneralDetails.DebugFormat(_text);
                    return _keyValueMap.Values.FirstOrDefault(); // this is cheating !!
                }

                //searching for crashes
                //if (key.ToString() == "999")
                //{
                //    _text = "Searched Key was '999', sometimes this crashes";
                //    Console.WriteLine(_text);
                //    GameLog.Client.GeneralDetails.DebugFormat(_text);
                //    return _keyValueMap.Values.FirstOrDefault(); // this is cheating !!
                //}

                //searching for crashes
                //if (key.ToString() == "789")
                //{
                //    _text = "Searched Key was '789', sometimes this crashes";
                //    Console.WriteLine(_text);
                //    GameLog.Client.GeneralDetails.DebugFormat(_text);
                //    return _keyValueMap.Values.FirstOrDefault(); // this is cheating !!
                //}


                if ((_keyValueMap != null) && _keyValueMap.TryGetValue(key, out TValue value))
                {
                    // works - but a lot of output    GameLog.Client.General.ErrorFormat("Searching Key = {0}", key.ToString());
                    return value;
                }

                foreach (TValue item in Items)
                {
                    if (_keyComparer.Equals(GetKeyForItem(item), key))
                    {
                        return item;
                    }
                }
                // avoids crashes

                _text = "Step_0878: Key not found >> " + key.ToString();
                Console.WriteLine(_text);
                GameLog.Core.General.ErrorFormat(_text);

                throw new KeyNotFoundException();

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
            try
            {
            _keyRetriever = reader.Read<Func<TValue, TKey>>();
            }
            catch { }


            if (Count < _threshold)
            {
                return;
            }

            _keyValueMap = new Dictionary<TKey, TValue>(Count, KeyComparer);

            foreach (TValue item in Items)
            {
                //_text = "Step_4199: GetKeyForItem= " + item;
                //Console.WriteLine(_text);
                //if (_keyRetriever != null)  // 2023-06-24
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
            _text = "OnKeyCollision: key= " + key.ToString()
                    + "item= " + item.ToString();
            Console.WriteLine(_text);
            GameLog.Core.General.ErrorFormat(_text);
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
                _ = _keyValueMap.Remove(key);
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