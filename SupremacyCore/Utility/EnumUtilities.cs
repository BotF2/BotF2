// 
// EnumUtilities.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using System.Linq;
using System.Threading;

using Supremacy.Collections;

namespace Supremacy.Utility
{
    public static class EnumUtilities
    {
        public static object NextEnum(Type enumType)
        {
            Array values = Enum.GetValues(enumType);
            return values.GetValue(RandomProvider.Shared.Next(values.Length));
        }

        public static T NextEnum<T>() where T : struct
        {
            EnumValueCollection<T> values = GetValues<T>();
            return values[RandomProvider.Shared.Next(values.Count)];
        }

        #region Cache Management

        private static Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        private static CacheEntry<T> GetEntry<T>() where T : struct
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException(string.Format("{0} is not an enum type.", type.FullName));
            }


            Dictionary<Type, object> cache = _cache;
            if (cache.TryGetValue(type, out object value))
            {
                return (CacheEntry<T>)value;
            }

            CacheEntry<T> newEntry = new CacheEntry<T>();

            while (true)
            {
                Dictionary<Type, object> newCache = new Dictionary<Type, object>(cache) { { type, newEntry } };

                if (Interlocked.CompareExchange(ref _cache, newCache, cache) == cache)
                {
                    return newEntry;
                }

                cache = _cache;

                if (cache.TryGetValue(type, out value))
                {
                    return (CacheEntry<T>)value;
                }
            }
        }

        #endregion

        public static EnumValueCollection<T> GetValues<T>() where T : struct
        {
            return EnumValueCollection<T>.AllValues;
        }

        public static IIndexedCollection<string> GetNames<T>() where T : struct
        {
            return GetEntry<T>().NameCollection;
        }

        public static IIndexedCollection<FieldInfo> GetFields<T>() where T : struct
        {
            return GetEntry<T>().FieldCollection;
        }

        public static AttributeCollection GetAttributes<T>() where T : struct
        {
            return GetEntry<T>().TypeAttributes;
        }

        public static IDictionary<T, int> GetOrdinalLookup<T>() where T : struct
        {
            return GetEntry<T>().ReadOnlyOrdinalLookup;
        }

        public static string GetName<T>(T value) where T : struct
        {
            CacheEntry<T> cacheEntry = GetEntry<T>();
            int ordinal = cacheEntry.OrdinalLookup[value];

            return cacheEntry.Names[ordinal];
        }

        public static FieldInfo GetField<T>(T value) where T : struct
        {
            CacheEntry<T> cacheEntry = GetEntry<T>();
            int ordinal = cacheEntry.OrdinalLookup[value];

            return cacheEntry.Fields[ordinal];
        }

        public static AttributeCollection GetAttributes<T>(T value) where T : struct
        {
            CacheEntry<T> cacheEntry = GetEntry<T>();
            int ordinal = cacheEntry.OrdinalLookup[value];

            return cacheEntry.ValueAttributes[ordinal];
        }

        public static bool IsFlagEnum<T>() where T : struct
        {
            return GetEntry<T>().IsFlagType;
        }

        public static ulong ToUInt64<T>(T value) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(string.Format("{0} is not an enum type.", typeof(T).FullName));
            }

            return ToUInt64Internal(value);
        }

        private static unsafe ulong ToUInt64Internal<T>(T value) where T : struct
        {
            TypedReference r = __makeref(value);

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            var v = &r;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return **(byte**)v;

                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return **(ushort**)v;

                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return **(uint**)v;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return **(ulong**)v;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region CacheEntry Class

        private sealed class CacheEntry<T> where T : struct
        {
            public readonly T[] Values;
            public readonly string[] Names;
            public readonly ulong[] RawValues;
            public readonly FieldInfo[] Fields;
            public readonly IIndexedCollection<string> NameCollection;
            public readonly IIndexedCollection<FieldInfo> FieldCollection;
            public readonly AttributeCollection TypeAttributes;
            public readonly AttributeCollection[] ValueAttributes;
            public readonly Dictionary<T, int> OrdinalLookup;
            public readonly IDictionary<T, int> ReadOnlyOrdinalLookup;
            public readonly bool IsFlagType;

            public CacheEntry()
            {
                Values = (T[])Enum.GetValues(typeof(T));
                Names = Enum.GetNames(typeof(T));
                OrdinalLookup = new Dictionary<T, int>();
                TypeAttributes = new AttributeCollection(Attribute.GetCustomAttribute(typeof(T), typeof(Attribute), false));
                IsFlagType = TypeAttributes.Matches(new FlagsAttribute());

                int valueCount = Values.Length;

                Fields = new FieldInfo[valueCount];
                RawValues = new ulong[valueCount];
                ValueAttributes = new AttributeCollection[valueCount];

                Dictionary<string, FieldInfo> fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(f => f.Name);

                for (int i = 0; i < Names.Length; i++)
                {
                    T value = Values[i];
                    FieldInfo field = fields[Names[i]];

                    RawValues[i] = ToUInt64Internal(value);
                    Fields[i] = field;
                    ValueAttributes[i] = new AttributeCollection(Attribute.GetCustomAttribute(field, typeof(Attribute), false));
                    OrdinalLookup[value] = i;
                }

                NameCollection = new ArrayWrapper<string>(Names);
                FieldCollection = new ArrayWrapper<FieldInfo>(Fields);

                ReadOnlyOrdinalLookup = OrdinalLookup.AsReadOnly();
            }
        }

        #endregion
    }
}