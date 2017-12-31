// ObservableObject.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Diagnostics;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;

namespace Supremacy.Effects
{
    [Serializable]
    public class DynamicObject : GameObject
    {
        [NonSerialized] private DynamicObjectType _dType;
        [NonSerialized] private FrugalMap _dynamicPropertyValues;

        public DynamicObject() {}

        public DynamicObject(int objectId) : base(objectId) {}

        public DynamicObjectType DynamicObjectType
        {
            get
            {
                if (_dType == null)
                    _dType = DynamicObjectType.FromSystemTypeInternal(GetType());
                return _dType;
            }
        }

        public void InvalidateProperty<TValue>([NotNull] DynamicProperty<TValue> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var value = GetValueInternal(property, false);
            if (value == null)
                return;

            if (value.HasModifiers)
                value.ResetComputedValue();
            else if (value.UsesCoercion)
                value.ResetCoercedValue(true);
        }

        public DynamicPropertyValue<TValue> GetValue<TValue>([NotNull] DynamicProperty<TValue> property)
        {
            return GetValueInternal(property, true);
        }

        private DynamicPropertyValue<TValue> GetValueInternal<TValue>(DynamicProperty<TValue> property, bool createIfMissing)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            DynamicPropertyValue<TValue> value;
            var globalIndex = property.GlobalIndex;

            lock (DynamicProperty.Synchronized)
            {
                var mapItem = _dynamicPropertyValues[globalIndex];
                if (mapItem == DynamicProperty.UnsetValue)
                {
                    if (!createIfMissing)
                        return null;
                    value = new DynamicPropertyValue<TValue>(property, this);
                    _dynamicPropertyValues[globalIndex] = value;
                }
                else
                {
                    value = (DynamicPropertyValue<TValue>)mapItem;
                }
            }

            return value;
        }

        #region Implementation of IOwnedDataSerializable
        public override void SerializeOwnedData([NotNull] SerializationWriter writer, [CanBeNull] object context)
    	{
    	    base.SerializeOwnedData(writer, context);

            var propertyCount = _dynamicPropertyValues.Count;

            writer.Write(propertyCount);

            for (var i = 0; i < propertyCount; i++)
            {
                object value;

                _dynamicPropertyValues.GetKeyValuePair(i, out int key, out value);

                DynamicProperty property;
                object baseValue;

                ExtractBaseValue(value, out property, out baseValue);

                writer.WriteTokenizedObject(new DynamicPropertyKey(property));
                writer.WriteObject(baseValue);
            }
    	}

        [Serializable]
        private sealed class DynamicPropertyKey : IOwnedDataSerializableAndRecreatable
        {
            private Type _ownerType;
            private string _name;

            public DynamicPropertyKey(DynamicProperty property)
            {
                _ownerType = property.OwnerType;
                _name = property.Name;
            }

            [UsedImplicitly]
            public DynamicPropertyKey()
            {
                // For serialization purposes only
            }

            public static implicit operator DynamicProperty.FromNameKey(DynamicPropertyKey key)
            {
                return new DynamicProperty.FromNameKey(key._name, key._ownerType);
            }

            #region Implementation of IOwnedDataSerializable

            void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
            {
                _ownerType = Type.GetType(reader.ReadString());
                _name = reader.ReadString();
            }

            void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
            {
                Debug.Assert(_ownerType != null);
                Debug.Assert(_name != null);

                writer.Write(_ownerType.FullName + ", " + _ownerType.Assembly.GetName().Name);
                writer.Write(_name);
            }

            #endregion
        }

        private static void ExtractBaseValue(dynamic dynamicPropertyValue, out DynamicProperty property, out object baseValue)
        {
            property = dynamicPropertyValue.Property;
            baseValue = dynamicPropertyValue.BaseValue;
        }

        private void DeserializeBaseValue(DynamicPropertyKey propertyKey, object baseValue)
        {
            var property = DynamicProperty.PropertyFromKey(propertyKey);
            if (property == null)
                return;

            lock (DynamicProperty.Synchronized)
            {
                dynamic value = _dynamicPropertyValues[property.GlobalIndex];

                if (value == DynamicProperty.UnsetValue)
                {
                    value = _dynamicPropertyValues[property.GlobalIndex] = Activator.CreateInstance(
                        typeof(DynamicPropertyValue<>).MakeGenericType(property.PropertyType),
                        property,
                        this);
                }

                value.BaseValue = baseValue;
            }
        }

        public override void DeserializeOwnedData([NotNull] SerializationReader reader, [CanBeNull] object context)
		{
            base.DeserializeOwnedData(reader, context);

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var key = (DynamicPropertyKey)reader.ReadTokenizedObject();
                var baseValue = reader.ReadObject();

                DeserializeBaseValue(key, baseValue);
            }
		}
        #endregion

        #region Overrides of Cloneable

        protected override Cloneable CreateInstance(ICloneContext context)
        {
            return new DynamicObject();
        }

        public override void CloneFrom(Cloneable original, ICloneContext context)
        {
            var source = (DynamicObject)original;

            lock (DynamicProperty.Synchronized)
            {
                var propertyCount = source._dynamicPropertyValues.Count;

                for (var i = 0; i < propertyCount; i++)
                {
                    source._dynamicPropertyValues.GetKeyValuePair(i, out int key, out object value);

                    if (value == DynamicProperty.UnsetValue)
                        continue;

                    dynamic @this = this;
                    dynamic sourcePropertyValue = value;
                    dynamic propertyValue = @this.GetValue(sourcePropertyValue.Property);

                    propertyValue.BaseValue = sourcePropertyValue.BaseValue;
                }
            }

        }

        #endregion
    }
}