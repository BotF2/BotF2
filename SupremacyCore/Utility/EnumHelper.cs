using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Supremacy.Utility
{
    public static class EnumHelper
    {
        public static T[] GetValues<T>() where T : struct
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string[] GetNames<T>() where T : struct {
            return Enum.GetNames(typeof(T));
        }

        public static bool IsDefined<T>(T value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static bool IsDefined<T>(T value, out int ordinal) where T : struct
        {
            ordinal = Array.IndexOf(GetValues<T>(), value);
            return (ordinal >= 0);
        }

        public static bool MatchAttribute<T>(this T source, Attribute attribute) where T : struct
        {
            bool result;
            if (EnumAttributeMatchCache.TryGetValue(new Tuple<Enum, Attribute>(source as Enum, attribute), out result))
            {
                result = false;
                foreach (Attribute customAttribute in Attribute.GetCustomAttributes(source.GetType().GetField(source.ToString()), attribute.GetType()))
                {
                    if (attribute.Match(customAttribute))
                    {
                        result = true;
                        break;
                    }
                }
                EnumAttributeMatchCache[new Tuple<Enum, Attribute>(source as Enum, attribute)] = result;
            }
            return result;
        }

        public static TAttribute GetAttribute<TEnum, TAttribute>(this TEnum enumValue) where TEnum : struct where TAttribute : Attribute
        {
            int ordinal;

            if (!IsDefined(enumValue, out ordinal)) { }
                return null;

            AttributeCollection attributes;
            if (!EnumValueAttributeCache.TryGetItem(enumValue as Enum, out attributes))
            {
                string name = GetNames<TEnum>()[ordinal];
                FieldInfo field = (typeof(TEnum)).GetField(name);

                attributes = new AttributeCollection(
                    Enumerable.ToArray(
                        Enumerable.Cast<Attribute> (
                            field.GetCustomAttributes(false))));

                attributes = EnumValueAttributeCache.GetOldest(enumValue as Enum, attributes);
            }

            return Enumerable.FirstOrDefault(Enumerable.OfType<TAttribute>(attributes));
        }

        public static Nullable<T> Parse<T>(string value) where T : struct
        {
            T result;
            if (TryParse(value, false, out result))
            {
                return result;
            }
            return null;
        }

        public static bool TryParse<T>(string value, out T result) where T : struct
        {
            return TryParse(value, false, out result);
        }

        public static bool TryParse<T>(string value, bool ignoreCase, out T result) where T : struct
        {
            result = new T();
            if (!(typeof(T).IsEnum))
            {
                return false;
            }

            if (value == null)
            {
                return false;
            }

            value = value.Trim();

            if (value.Length == 0)
            {
                return false;
            }

            try
            {
                ulong resultValue = 0;
                if ((char.IsDigit(value[0]) || (value[0] == '-')) || (value[0] == '+')) {
                    Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                    try
                    {
                        Object convertedValue = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                        result = (T)convertedValue;
                        return true;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                }

                string[] flagsArray = value.Split(EnumSeparators);
                HashEntry hashEntry = GetHashEntry<T>();

                string[] names = hashEntry.Names;
                for (int i = 0; i < flagsArray.Length; i++)
                {
                    flagsArray[i] = flagsArray[i].Trim();

                    bool valueFound = false;

                    for (int j = 0; j < names.Length; j++)
                    {
                        if (ignoreCase)
                        {
                            if (!string.Equals(names[j], flagsArray[i], StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }
                        else if (!string.Equals(names[j], flagsArray[i], StringComparison.Ordinal)) {
                            continue;
                        }

                        ulong enumValue = hashEntry.Values[j];
                        resultValue |= enumValue;
                        valueFound = true;
                        break;
                    }

                    if (valueFound)
                    {
                        continue;
                    }

                    return false;
                }

                result = (T)Convert.ChangeType(resultValue, Enum.GetUnderlyingType(typeof(T)));
                return true;
            }
            catch (Exception)
            {
                result = new T();
                return false;
            }
        }

        public static T ParseOrGetDefault<T>(string value) where T : struct
        {
            return ParseOrGetDefault<T>(value, false);
        }

        public static T ParseOrGetDefault<T>(string value, bool ignoreCase) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return new T();

            }
            return (T)Enum.Parse(typeof(T), value.Trim(), ignoreCase);
        }

        internal class HashEntry
        {
            public readonly string[]  Names;
            public readonly ulong[] Values;

            public HashEntry(string[] names, ulong[] values) {
                Names = names;
                Values = values;
            }
        };

        private static ulong ToUInt64(Object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException("Invalid type: " + value.GetType().Name);
        }

        private static HashEntry GetHashEntry<T>() where T : struct
        {
            HashEntry entry = (HashEntry)FieldInfoHash[typeof(T)];
            if (entry == null)
            {
                if (FieldInfoHash.Count > 100)
                    FieldInfoHash.Clear();

                T[] enumValues;
                ulong[] values;
                string[] names;

                if ((typeof(T)).BaseType == typeof(Enum))
                {
                    enumValues = GetValues<T>();
                    names = GetNames<T>();
                    values = new ulong[enumValues.Length];

                    for (int i = 0; i < enumValues.Length; i++)
                        values[i] = Convert.ToUInt64(enumValues[i]);
                }
                else
                {
                    FieldInfo[] fields = (typeof(T)).GetFields(BindingFlags.Public | BindingFlags.Static);

                    values = new ulong[fields.Length];
                    names = new string[fields.Length];

                    for (int i = 0; i < fields.Length; i++)
                    {
                        names[i] = fields[i].Name;
                        values[i] = ToUInt64(fields[i].GetValue(null));
                    }

                    for (int j = 1; j < values.Length; j++)
                    {
                        int index = j;
                        string name = names[j];
                        ulong value = values[j];
                        bool setValue = false;

                        while (values[index - 1] > value)
                        {
                            names[index] = names[index - 1];
                            values[index] = values[index - 1];
                            index--;
                            setValue = true;
                            if (index == 0)
                                break;
                        }

                        if (!setValue)
                            continue;

                        names[index] = name;
                        values[index] = value;
                    }
                }

                entry = new HashEntry(names, values);
                FieldInfoHash[typeof(T)] = entry;
            }
            return entry;
        }

        static EnumHelper()
        {
            EnumAttributeMatchCache = new Dictionary <Tuple<Enum, Attribute>, bool>();
            EnumValueAttributeCache = new TvdP.Collections.Cache<Enum, AttributeCollection>();
            FieldInfoHash = Hashtable.Synchronized(new Hashtable());
            EnumSeparators = new char[] { ',' };
        }

        static readonly Dictionary<Tuple<Enum, Attribute>, bool> EnumAttributeMatchCache;
        static readonly TvdP.Collections.Cache<Enum, AttributeCollection> EnumValueAttributeCache;
        static readonly char[] EnumSeparators;
        static readonly Hashtable FieldInfoHash;

    }
}
