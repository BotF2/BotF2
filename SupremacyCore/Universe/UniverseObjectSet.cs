// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.IO.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Universe
{
    /// <summary>
    /// Represents an observable collection of <see cref="UniverseObject"/>s, keyed
    /// on <see cref="Supremacy.Universe.UniverseObject.ObjectID"/>.
    /// </summary>
    [Serializable]
    public class UniverseObjectSet : 
        IndexedCollection<UniverseObject>,
        IKeyedCollection<int, UniverseObject>,
        IOwnedDataSerializableAndRecreatable
    {
        [NonSerialized]
        private Index _objectIdIndex;

        public UniverseObjectSet() 
            : base(InfiniteMaxKeyCount)
        {
            IsChangeNotificationEnabled = true;
            FetchObjectIdIndex();
        }

        protected UniverseObjectSet(IList<UniverseObject> internalCollection)
            : base(InfiniteMaxKeyCount, internalCollection)
        {
            IsChangeNotificationEnabled = true;
            FetchObjectIdIndex();
        }

        protected UniverseObjectSet(IIndexedEnumerable<UniverseObject> internalCollection)
            : base(InfiniteMaxKeyCount, internalCollection)
        {
            IsChangeNotificationEnabled = true;
            FetchObjectIdIndex();
        }

        public UniverseObject this[int objectId]
        {
            get { return GetObjectById(objectId); }
        }

        public void AddOrUpdateItem(UniverseObject item)
        {
            SyncLock.EnterUpgradeableReadLock();

            try
            {
                var itemIndex = ((IList<UniverseObject>)this).IndexOf(item);
                if (itemIndex >= 0)
                    ReplaceItem(itemIndex, item, true);
                else
                    InsertItem(Count, item, true);
            }
            finally
            {
                SyncLock.ExitUpgradeableReadLock();
            }
        }

        private UniverseObject GetObjectById(int objectId)
        {
            SyncLock.EnterReadLock();

            try
            {
                int hashCode = GetIndexableHashCode(objectId);
                if (_objectIdIndex.LookupTable.ContainsKey(hashCode))
                {
                    foreach (var item in _objectIdIndex.LookupTable[hashCode])
                    {
                        if (item.ObjectID == objectId)
                            return item;
                    }
                }
                return null;
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        private void FetchObjectIdIndex()
        {
            _objectIdIndex = GetIndexByProperty("ObjectID");
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            FetchObjectIdIndex();
        }

        /// <summary>
        /// Clones this <see cref="UniverseObjectSet"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public UniverseObjectSet Clone()
        {
            SyncLock.EnterReadLock();

            try
            {
                var count = Count;
                var internalCollection = InternalCollection;
                var cloneInternalCollection = new List<UniverseObject>(count);

                for (var i = 0; i < count; i++)
                    cloneInternalCollection[i] = internalCollection[i];

                return new UniverseObjectSet(cloneInternalCollection);
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Creates an array containing all of the items in this <see cref="UniverseObjectSet"/>.
        /// </summary>
        /// <returns>The item array.</returns>
        public UniverseObject[] ToArray()
        {
            SyncLock.EnterReadLock();
            try
            {
                var array = new UniverseObject[Count];
                CopyTo(array, 0);
                return array;
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        #region IOwnedDataSerializable Members
        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(ToArray());
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            AddMany(reader.ReadOptimizedObjectArray().Cast<UniverseObject>());
            BuildIndexes();
            FetchObjectIdIndex();
        }
        #endregion

        #region Implementation of IKeyedCollection<GameObjectID,UniverseObject>
        UniverseObject IKeyedLookup<int, UniverseObject>.this[int key]
        {
            get { return this[key]; }
        }

        bool IKeyedLookup<int, UniverseObject>.Contains(int key)
        {
            var @this = (IKeyedCollection<int, UniverseObject>)this;

            return @this.TryGetValue(key, out UniverseObject item);
        }

        IEqualityComparer<int> IKeyedLookup<int, UniverseObject>.KeyComparer => EqualityComparer<int>.Default;

        IEnumerable<int> IKeyedCollection<int, UniverseObject>.Keys
        {
            get
            {
                SyncLock.EnterReadLock();
                try
                {
                    return this.Select(o => o.ObjectID);
                }
                finally
                {
                    SyncLock.ExitReadLock();
                }
            }
        }

        bool IKeyedCollection<int, UniverseObject>.TryGetValue(int key, out UniverseObject value)
        {
            SyncLock.EnterReadLock();
            try
            {
                value = GetObjectById(key);
                return (value != null);
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }
        #endregion
    }
}
