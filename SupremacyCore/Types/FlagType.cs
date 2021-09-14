// FlagType.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace Supremacy.Types
{
    [Serializable]
    public abstract class FlagType : IEquatable<FlagType>
    {
        #region Constants
        private const string EmptyName = "0";
        private const int EmptyValue = 0;
        #endregion

        #region Fields
        private readonly int _value;
        #endregion

        #region Constructors
        protected FlagType(int value)
        {
            _value = value;
        }
        #endregion

        #region Properties and Indexers
        public string Name => ToString();

        public int Value => _value;

        public bool IsEmpty => _value == EmptyValue;
        #endregion

        #region Methods
        public override bool Equals(object value)
        {
            return _value.Equals(value as FlagType);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public static string[] GetNames()
        {
            return GetNamesCore<FlagType>();
        }

        public static FlagType[] GetValues()
        {
            return GetValuesCore<FlagType>();
        }

        public bool IsSet(FlagType value)
        {
            return (this & value) == value;
        }

        public static T Parse<T>(string value) where T : FlagType
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("value cannot be null or empty");
            }

            string[] names = value.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 1)
            {
                return ParseSingleValue<T>(names[0]);
            }

            T result = CreateEmptyValue<T>();

            foreach (string name in names)
            {
                result |= ParseSingleValue<T>(name);
            }

            return result;
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return EmptyName;
            }

            StringCollection names = new StringCollection();

            foreach (
                FieldInfo fieldInfo in
                    GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
            {
                if (_value == ((FlagType)fieldInfo.GetValue(this))._value)
                {
                    _ = names.Add(fieldInfo.Name);
                }
            }

            if (names.Count == 0)
            {
                _ = names.Add(string.Format("0x{0:x8}", _value));
            }

            StringBuilder sb = new StringBuilder();

            _ = sb.Append(names[0]);

            for (int i = 1; i < names.Count; i++)
            {
                _ = sb.Append(" | ");
                _ = sb.Append(names[i]);
            }

            return sb.ToString();
        }

        protected internal static T FromInt32<T>(int value) where T : FlagType
        {
            foreach (T definedValue in GetValuesCore<T>())
            {
                if (value == definedValue)
                {
                    return definedValue;
                }
            }
            return (T)Activator.CreateInstance(typeof(T), value);
        }

        protected internal static string[] GetNamesCore<T>() where T : FlagType
        {
            IDictionary<string, T> nameValuePaurs = GetNameValuePairs<T>();
            string[] names = new string[nameValuePaurs.Keys.Count];
            nameValuePaurs.Keys.CopyTo(names, 0);
            return names;
        }

        protected internal static IDictionary<string, T> GetNameValuePairs<T>() where T : FlagType
        {
            Dictionary<string, T> nameValuePairs = new Dictionary<string, T>();
            foreach (FieldInfo fieldInfo in typeof(T).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!nameValuePairs.ContainsKey(fieldInfo.Name)
                    && typeof(T).IsAssignableFrom(fieldInfo.FieldType))
                {
                    nameValuePairs[fieldInfo.Name] = (T)fieldInfo.GetValue(null);
                }
            }
            return nameValuePairs;
        }

        protected internal static T[] GetValuesCore<T>() where T : FlagType
        {
            List<T> values = new List<T>(GetNameValuePairs<T>().Values);
            values.Sort();
            return values.ToArray();
        }

        protected static T CreateEmptyValue<T>() where T : FlagType
        {
            return FromInt32<T>(0);
        }

        protected bool IsCompatibleValue(FlagType value)
        {
            if (value == null)
            {
                return false;
            }

            return GetType().IsAssignableFrom(value.GetType())
                    || value.GetType().IsAssignableFrom(GetType());
        }

        protected static int ToInt32<T>(T value) where T : FlagType
        {
            if (value == null)
            {
                return EmptyValue;
            }

            return value._value;
        }

        private static T ParseSingleValue<T>(string value) where T : FlagType
        {
            IDictionary<string, T> nameValuePairs = GetNameValuePairs<T>();
            if (nameValuePairs.ContainsKey(value))
            {
                return nameValuePairs[value];
            }

            return FromInt32<T>(int.Parse(value));
        }
        #endregion

        #region IEquatable<FlagType> Members
        public bool Equals(FlagType value)
        {
            if (value is null)
            {
                return false;
            }

            if (!IsCompatibleValue(value))
            {
                return false;
            }

            return _value.Equals(value);
        }
        #endregion

        #region Operators
        public static bool operator ==(FlagType a, FlagType b)
        {
            return Equals(a, b);
        }

        public static implicit operator int(FlagType value)
        {
            return ToInt32(value);
        }

        public static implicit operator FlagType(int value)
        {
            return FromInt32<FlagType>(value);
        }

        public static bool operator !=(FlagType a, FlagType b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}