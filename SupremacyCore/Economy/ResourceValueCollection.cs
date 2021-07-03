// ResourceValueCollection.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Supremacy.Economy
{
    [Serializable]
    public sealed class ResourceValueCollection
        : Dictionary<ResourceType, int>,
          IOwnedDataSerializableAndRecreatable,
          ICloneable
    {
        public const int MaxValue = int.MaxValue;
        public const int MinValue = 0;

        public ResourceValueCollection()
        {
            _ = EnumHelper.GetValues<ResourceType>().ForEach(r => base.Add(r, 0));
        }

        public bool MeetsOrExceeds(ResourceValueCollection resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            return !EnumHelper.GetValues<ResourceType>().Any(o => this[o] < resources[o]);
        }

        public new void Add(ResourceType resourceType, int value)
        {
            this[resourceType] += value;
        }

        public void Add(ResourceValueCollection resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException("resources");
            }

            foreach (ResourceType resource in EnumHelper.GetValues<ResourceType>())
            {
                this[resource] += resources[resource];
            }
        }

        public ResourceValueCollection Clone()
        {
            ResourceValueCollection clone = new ResourceValueCollection();
            _ = EnumHelper.GetValues<ResourceType>().ForEach(r => clone[r] = this[r]);
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(this);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Dictionary<ResourceType, int> data = reader.ReadDictionary<ResourceType, int>();
            _ = EnumHelper.GetValues<ResourceType>().ForEach(r => this[r] = data[r]);
        }

        public ResourceValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}