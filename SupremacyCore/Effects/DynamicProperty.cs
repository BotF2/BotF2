// DynamicProperty.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Supremacy.Annotations;

namespace Supremacy.Effects
{
    public delegate TValue CoerceDynamicPropertyCallback<TValue>(
        DynamicObject source,
        TValue modifiedValue,
        out bool revertToPreviousValue);

    public delegate void DynamicPropertyChangedCallback<TValue>(
        DynamicObject source,
    DynamicPropertyChangedEventArgs<TValue> e);

    public delegate bool ValidateValueCallback<in TValue>(TValue value);

    public abstract class DynamicProperty
    {
        internal static readonly object UnsetValue = new object();
        internal static readonly object Synchronized;

        internal InsertionSortMap MetadataMap;

        protected static readonly Dictionary<FromNameKey, DynamicProperty> RegisteredProperties;

        private static int GlobalIndexCount;

        private readonly int _globalIndex;
        private readonly string _name;
        private readonly Type _ownerType;
        private readonly Type _propertyType;

        protected DynamicProperty(string name, Type ownerType, Type propertyType)
        {
            _name = name;
            _ownerType = ownerType;
            _propertyType = propertyType;

            MetadataMap = new InsertionSortMap();

            lock (Synchronized)
            {
                _globalIndex = GetUniqueGlobalIndex(ownerType, name);
            }
        }

        public Type OwnerType => _ownerType;

        public string Name => _name;

        public Type PropertyType => _propertyType;

        internal int GlobalIndex => _globalIndex;

        internal static DynamicProperty PropertyFromKey([NotNull] FromNameKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            RuntimeHelpers.RunClassConstructor(key.OwnerType.TypeHandle);

            lock (Synchronized)
            {
                DynamicProperty property;

                if (RegisteredProperties.TryGetValue(key, out property))
                    return property;
            }

            return null;
        }

        internal static int GetUniqueGlobalIndex(Type ownerType, string name)
        {
            lock (Synchronized)
            {
                DynamicProperty registeredProperty;

                if (RegisteredProperties.TryGetValue(new FromNameKey(name, ownerType), out registeredProperty))
                    return registeredProperty.GlobalIndex;
            }

            if (GlobalIndexCount < 65535)
                return GlobalIndexCount++;

            if (ownerType != null)
                throw DynamicPropertyLimitExceeded(ownerType.Name + "." + name);

            throw DynamicPropertyLimitExceeded("ConstantProperty");
        }

        private static InvalidOperationException DynamicPropertyLimitExceeded(string propertyName)
        {
            return new InvalidOperationException(
                string.Format(
                    "Could not register '{0}' because the DynamicProperty limit has been exceeded.",
                    propertyName));
        }

        static DynamicProperty()
        {
            Synchronized = new object();
            RegisteredProperties = new Dictionary<FromNameKey, DynamicProperty>();
        }

        #region Dependent Type: FromNameKey
        protected internal class FromNameKey : IEquatable<FromNameKey>
        {
            private readonly string _name;
            private readonly Type _ownerType;
            private readonly int _hashCode;

            public FromNameKey(string name, Type ownerType)
            {
                _name = name;
                _ownerType = ownerType;
                _hashCode = _name.GetHashCode() ^ _ownerType.GetHashCode();
            }

            public string Name => _name;

            public Type OwnerType => _ownerType;

            public override bool Equals(object o)
            {
                return (((o != null) && (o is FromNameKey)) && Equals(o));
            }

            public bool Equals(FromNameKey key)
            {
                return (_name.Equals(key._name) && (_ownerType == key._ownerType));
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
        #endregion
    }

    public sealed class DynamicProperty<TValue> : DynamicProperty
    {
        private readonly DynamicPropertyMetadata<TValue> _defaultMetadata;
        private readonly ValidateValueCallback<TValue> _validateValueCallback;

        private DynamicProperty(
            string name,
            Type ownerType,
            DynamicPropertyMetadata<TValue> defaultMetadata,
            ValidateValueCallback<TValue> validateValueCallback)
            : base(name, ownerType, typeof(TValue))
        {
            _defaultMetadata = defaultMetadata;
            _validateValueCallback = validateValueCallback;
        }

        public DynamicPropertyMetadata<TValue> DefaultMetadata => _defaultMetadata;

        public ValidateValueCallback<TValue> ValidateValueCallback => _validateValueCallback;

        private static TValue AutoGenerateDefaultValue(
            ValidateValueCallback<TValue> validateValueCallback,
            string name,
            Type ownerType)
        {
            var defaultValue = default(TValue);
            ValidateDefaultValue(defaultValue, validateValueCallback, name, ownerType);
            return defaultValue;
        }

        private static void ValidateDefaultValue(
            TValue defaultValue,
            ValidateValueCallback<TValue> validateValueCallback,
            string name,
            Type ownerType)
        {
            if ((validateValueCallback == null) || validateValueCallback(defaultValue))
                return;

            throw new ArgumentException(
                string.Format(
                    "The default value of the DynamicProperty '{0}' for type '{1}' failed validation.",
                    name,
                    ownerType.FullName));
        }

        private static DynamicPropertyMetadata<TValue> AutoGeneratePropertyMetadata(
            ValidateValueCallback<TValue> validateValueCallback,
            string name,
            Type ownerType)
        {
            var defaultValue = default(TValue);
            if ((validateValueCallback != null) && !validateValueCallback(defaultValue))
            {
                throw new ArgumentException(
                    string.Format(
                    "The default value of the DynamicProperty '{0}' for type '{1}' failed validation.",
                    name,
                    ownerType.FullName));
            }
            return new DynamicPropertyMetadata<TValue>(defaultValue);
        }

        public static DynamicProperty<TValue> Register(string name, Type ownerType)
        {
            return Register(name, ownerType, null, null);
        }

        public static DynamicProperty<TValue> Register(string name, Type ownerType, DynamicPropertyMetadata<TValue> typeMetadata)
        {
            return Register(name, ownerType, typeMetadata, null);
        }

        public static DynamicProperty<TValue> Register(
            string name,
            Type ownerType,
            DynamicPropertyMetadata<TValue> typeMetadata,
            ValidateValueCallback<TValue> validateValueCallback)
        {
            RegisterParameterValidation(name, ownerType);

            DynamicPropertyMetadata<TValue> defaultMetadata = null;

            if ((typeMetadata != null) && typeMetadata.IsDefaultValueModified)
                defaultMetadata = new DynamicPropertyMetadata<TValue>(typeMetadata.DefaultValue);

            var property = RegisterCommon(name, ownerType, defaultMetadata, validateValueCallback);
            if (typeMetadata != null)
                property.OverrideMetadata(ownerType, typeMetadata);

            return property;
        }

        public void OverrideMetadata(Type forType, DynamicPropertyMetadata<TValue> typeMetadata)
        {
            DynamicPropertyMetadata<TValue> metadata;
            DynamicObjectType type;
            SetupOverrideMetadata(forType, typeMetadata, out type, out metadata);
            ProcessOverrideMetadata(forType, typeMetadata, type, metadata);
        }

        private void ProcessOverrideMetadata(
            Type forType,
            DynamicPropertyMetadata<TValue> typeMetadata,
            DynamicObjectType dType,
            DynamicPropertyMetadata<TValue> baseMetadata)
        {
            lock (Synchronized)
            {
                if (MetadataMap[dType.Id] != UnsetValue)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Metadata already registered for type '{0}'.",
                            forType.Name));
                }
                MetadataMap[dType.Id] = typeMetadata;
            }

            typeMetadata.Merge(baseMetadata, this);
            typeMetadata.Seal(this, forType);
        }

        private void SetupOverrideMetadata(
            Type forType,
            DynamicPropertyMetadata<TValue> typeMetadata,
            out DynamicObjectType dType,
            out DynamicPropertyMetadata<TValue> baseMetadata)
        {
            if (forType == null)
                throw new ArgumentNullException("forType");

            if (typeMetadata == null)
                throw new ArgumentNullException("typeMetadata");

            if (typeMetadata.IsSealed)
                throw new ArgumentException("Type metadata is already in use.");

            if (!typeof(DynamicObject).IsAssignableFrom(forType))
            {
                throw new ArgumentException(
                    string.Format(
                        "Type '{0}' does not inherit from DynamicObject.",
                        forType.Name));
            }

            if (typeMetadata.IsDefaultValueModified)
            {
                ValidateDefaultValue(
                    typeMetadata.DefaultValue,
                    ValidateValueCallback,
                    Name,
                    OwnerType);
            }

            dType = DynamicObjectType.FromSystemType(forType);
            baseMetadata = GetMetadata(dType.BaseType);
            if (!baseMetadata.GetType().IsAssignableFrom(typeMetadata.GetType()))
            {
                throw new ArgumentException("Overridi");
            }
        }

        internal static bool ValuesEqual(TValue first, TValue second)
        {
            if (typeof(TValue).IsValueType)
                return Equals(first, second);
            return ReferenceEquals(first, second);
        }

        /// <summary> 
        /// Reteive metadata for a DynamicObject type described by the
        /// given DynamicObjectType 
        /// </summary>
        public DynamicPropertyMetadata<TValue> GetMetadata(DynamicObjectType dynamicObjectType)
        {
            if (null != dynamicObjectType)
            {
                // Do we in fact have any overrides at all?
                int index = MetadataMap.Count - 1;
                int id;
                object value;

                if (index < 0)
                {
                    // No overrides or it's the base class 
                    return _defaultMetadata;
                }

                if (index == 0)
                {
                    // Only 1 override
                    MetadataMap.GetKeyValuePair(index, out id, out value);

                    // If there is overriden metadata, then there is a base class with
                    // lower or equal Id of this class, or this class is already a base class 
                    // of the overridden one. Therefore dependencyObjectType won't ever
                    // become null before we exit the while loop
                    while (dynamicObjectType.Id > id)
                    {
                        dynamicObjectType = dynamicObjectType.BaseType;
                    }

                    if (id == dynamicObjectType.Id)
                    {
                        // Return the override
                        return (DynamicPropertyMetadata<TValue>)value;
                    }
                    // Return default metadata 
                }
                else
                {
                    // We have more than 1 override for this class, so we will have to loop through
                    // both the overrides and the class Id 
                    if (0 != dynamicObjectType.Id)
                    {
                        do
                        {
                            // Get the Id of the most derived class with overridden metadata
                            MetadataMap.GetKeyValuePair(index, out id, out value);
                            --index;

                            // If the Id of this class is less than the override, then look for an override 
                            // with an equal or lower Id until we run out of overrides
                            while ((dynamicObjectType.Id < id) && (index >= 0))
                            {
                                MetadataMap.GetKeyValuePair(index, out id, out value);
                                --index;
                            }

                            // If there is overriden metadata, then there is a base class with
                            // lower or equal Id of this class, or this class is already a base class 
                            // of the overridden one. Therefore dependencyObjectType won't ever
                            // become null before we exit the while loop
                            while (dynamicObjectType.Id > id)
                            {
                                dynamicObjectType = dynamicObjectType.BaseType;
                            }

                            if (id == dynamicObjectType.Id)
                            {
                                // Return the override
                                return (DynamicPropertyMetadata<TValue>)value;
                            }
                        }
                        while (index >= 0);
                    }
                }
            }
            return _defaultMetadata;
        }

        public bool IsValidValue(TValue value)
        {
            if (_validateValueCallback != null)
                return _validateValueCallback(value);
            return true;
        }

        private static void RegisterParameterValidation(string name, Type ownerType)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentException("Value must be a non-null, non-empty string.", "name");
            if (ownerType == null)
                throw new ArgumentNullException("ownerType");
        }

        private static DynamicProperty<TValue> RegisterCommon(
            string name,
            Type ownerType,
            DynamicPropertyMetadata<TValue> defaultMetadata,
            ValidateValueCallback<TValue> validateValueCallback)
        {
            var key = new FromNameKey(name, ownerType);

            lock (Synchronized)
            {
                if (RegisteredProperties.ContainsKey(key))
                {
                    throw new ArgumentException(
                        string.Format(
                            "A DynamicProperty named '{0}' is already registered for type '{1}'.",
                            name,
                            ownerType.FullName),
                        "name");
                }

                if (defaultMetadata == null)
                {
                    defaultMetadata = AutoGeneratePropertyMetadata(validateValueCallback, name, ownerType);
                }
                else if (!defaultMetadata.IsDefaultValueModified)
                {
                    defaultMetadata.DefaultValue = AutoGenerateDefaultValue(
                        validateValueCallback,
                        name,
                        ownerType);
                }
            }

            var property = new DynamicProperty<TValue>(name, ownerType, defaultMetadata, validateValueCallback);

            defaultMetadata.Seal(property, null);

            lock (Synchronized)
            {
                RegisteredProperties[key] = property;
            }

            return property;
        }
    }

    #region Dependent Type: ModifiedValue
    internal class ModifiedValue<TValue>
    {
        public TValue BaseValue { get; set; }
        public TValue ComputedValue { get; set; }
        public TValue CoercedValue { get; set; }
    }
    #endregion
}