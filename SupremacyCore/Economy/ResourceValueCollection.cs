// ResourceValueCollection.cs
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

using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Economy
{
    [Serializable]
    public sealed class ResourceValueCollection
        : EnumKeyedValueList<ResourceType, int>,
          IOwnedDataSerializableAndRecreatable,
          ICloneable,
        IEnumerable<IPair<ResourceType, int>>
    {
        public const int MaxValue = int.MaxValue;
        public const int MinValue = 0;

        public bool MeetsOrExceeds(ResourceValueCollection resources)
        {
            if (resources == null)
                throw new ArgumentNullException("resources");

            return !EnumHelper.GetValues<ResourceType>().Any(o => this[o] < resources[o]);
        }

        public void Add(ResourceType resourceType, int value)
        {
            this[resourceType] += value;
        }

        public void Add(ResourceValueCollection resources)
        {
            if (resources == null)
                throw new ArgumentNullException("resources");

            foreach (var resource in EnumHelper.GetValues<ResourceType>())
                this[resource] += resources[resource];
        }

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ResourceValueCollection Clone()
        {
            var clone = new ResourceValueCollection();
            for (var i = 0; i < Values.Length; i++)
                clone.Values[i] = Values[i];
            return clone;
        }
        #endregion

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(ToArray());
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            var values = reader.ReadInt32Array();
            for (var i = 0; i < values.Length; i++)
                Values[i] = values[i];
        }

        #region Implementation of IEnumerable
        IEnumerator<IPair<ResourceType, int>> IEnumerable<IPair<ResourceType, int>>.GetEnumerator()
        {
            foreach (var resourceType in EnumHelper.GetValues<ResourceType>())
                yield return new Pair<ResourceType, int>(resourceType, this[resourceType]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IPair<ResourceType, int>>)this).GetEnumerator();
        }
        #endregion
    }
}