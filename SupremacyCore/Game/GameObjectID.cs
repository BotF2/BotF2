// GameObjectID.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

    using System;
    using System.ComponentModel;
    using System.Globalization;

    namespace Supremacy.Game
    {
        [Serializable]
        [ImmutableObject(true)]
        public struct GameObjectID : IComparable<GameObjectID>, IEquatable<GameObjectID>, IComparable<int>, IEquatable<int>, IConvertible, IFormattable, ICloneable
        {
            private const int InvalidValue = -1;
            public static readonly GameObjectID InvalidID = InvalidValue;

            private readonly int _value;

            public bool IsValid
            {
                get { return (_value != InvalidValue); }
            }

            public GameObjectID(int value)
            {
                _value = (value < 0) ? InvalidValue : value;
            }

            public static bool operator ==(GameObjectID a, GameObjectID b)
            {
                return (a._value == b._value);
            }

            public static bool operator !=(GameObjectID a, GameObjectID b)
            {
                return (a._value != b._value);
            }

            public static implicit operator int(GameObjectID objectId)
            {
                return objectId._value;
            }

            public static implicit operator GameObjectID(int value)
            {
                return new GameObjectID(value);
            }

            public override string ToString()
            {
                return _value.ToString();
            }

            public override bool Equals(object obj)
            {
                IConvertible convertible = obj as IConvertible;
                if (convertible != null)
                    return Equals(convertible.ToInt32(NumberFormatInfo.InvariantInfo));
                return false;
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            #region IEquatable<GameObjectID> Members
            public bool Equals(GameObjectID other)
            {
                return _value.Equals(other._value);
            }
            #endregion

            #region IComparable<GameObjectID> Members
            public int CompareTo(GameObjectID other)
            {
                return Convert.ToByte(_value);
            }
            #endregion

            #region IConvertible Members
            public TypeCode GetTypeCode()
            {
                return TypeCode.Int32;
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                return Convert.ToBoolean(_value);
            }

            public byte ToByte(IFormatProvider provider)
            {
                return Convert.ToByte(_value);
            }

            public char ToChar(IFormatProvider provider)
            {
                return Convert.ToChar(_value);
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                return Convert.ToDateTime(_value);
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                return Convert.ToDecimal(_value);
            }

            public double ToDouble(IFormatProvider provider)
            {
                return Convert.ToDouble(_value);
            }

            public short ToInt16(IFormatProvider provider)
            {
                return Convert.ToInt16(_value);
            }

            public int ToInt32(IFormatProvider provider)
            {
                return Convert.ToInt32(_value);
            }

            public long ToInt64(IFormatProvider provider)
            {
                return Convert.ToInt64(_value);
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                return Convert.ToSByte(_value);
            }

            public float ToSingle(IFormatProvider provider)
            {
                return Convert.ToSingle(_value);
            }

            public string ToString(IFormatProvider provider)
            {
                return Convert.ToString(_value);
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                return Convert.ChangeType(_value, conversionType);
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                return Convert.ToUInt16(_value);
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                return Convert.ToUInt32(_value);
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                return Convert.ToUInt64(_value);
            }
            #endregion

            #region IComparable<int> Members
            public int CompareTo(int other)
            {
                return _value.CompareTo(other);
            }
            #endregion

            #region IEquatable<int> Members
            public bool Equals(int other)
            {
                return _value.Equals(other);
            }
            #endregion

            #region IFormattable Members
            public string ToString(string format, IFormatProvider formatProvider)
            {
                return _value.ToString(format, formatProvider);
            }
            #endregion

            #region ICloneable Members
            object ICloneable.Clone()
            {
                return Clone();
            }

            public int Clone()
            {
                return _value;
            }
            #endregion
        }
    }