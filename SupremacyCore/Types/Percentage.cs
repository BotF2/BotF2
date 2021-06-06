// Percentage.cs
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

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Supremacy.Types
{
    /// <summary>
    /// A 32-bit floating point value which represents a percentage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type is freely convertible to and from <see cref="float"/> and <see cref="double"/> types using
    /// standard percentage representation (e.g. a <see cref="Percentage"/> value representing <c>1%</c> is
    /// backed by a <see cref="float"/> value of <c>0.01f</c>).
    /// </para>
    /// <para>
    /// <see cref="Percentage"/> values have a maximum precision of one hundredth of a percent.  Rounding and
    /// error correction are automatically applied when converting from other numeric types, e.g. <c>0.0333333f</c>
    /// would be rounded to <c>3.33%</c>, and <c>0.0349998f</c> would be error-corrected to <c>3.5%</c> .
    /// </para>
    /// </remarks>
    [Serializable]
    [ImmutableObject(true)]
    [TypeConverter(typeof(PercentageConverter))]
    public struct Percentage : IConvertible, IEquatable<Percentage>, IComparable<Percentage>, IComparable, IFormattable
    {
        #region Constants

        private const string DefaultFormatString = "0%";

        public static readonly Percentage MinValue = 0;
        public static readonly Percentage MaxValue = float.MaxValue;

        #endregion

        #region Fields
        private readonly float _value;
        #endregion

        #region Constructors
        public Percentage(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Value cannot be NaN or Infinity.", "value");

            ApplyErrorCorrectionAndLimitPrecision(ref value);

            if (value > float.MaxValue)
                throw new ArgumentOutOfRangeException("value", "Value must be <= MaxValue.");
            if (value < float.MinValue)
                throw new ArgumentOutOfRangeException("value", "Value must be >= MinValue.");

            _value = value;
        }

        public Percentage(double value)
            : this((float)value)
        {
            if (value > MaxValue)
                throw new ArgumentOutOfRangeException("value", "Value must be <= MaxValue.");
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (!(obj is Percentage))
            {
                return false;
            }
            return (((Percentage)obj)._value == _value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString(DefaultFormatString);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Percentage))
            {
                throw new ArgumentException(
                    "Value is not a Supremacy.Types.Percentage");
            }

            Percentage other = (Percentage)obj;
            if (_value == other._value)
            {
                return 0;
            }
            if (_value > other._value)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        #endregion

        #region IComparable<Percentage> Members
        public int CompareTo(Percentage other)
        {
            return _value.CompareTo(other._value);
        }
        #endregion

        #region IConvertible Members
        public TypeCode GetTypeCode()
        {
            return TypeCode.Single;
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
            return _value;
        }

        public string ToString(IFormatProvider provider)
        {
            return _value.ToString(DefaultFormatString, provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(_value, conversionType, provider);
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

        #region IEquatable<Percentage> Members
        public bool Equals(Percentage other)
        {
            return (other._value == _value);
        }
        #endregion

        #region IFormattable Members
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _value.ToString(format ?? DefaultFormatString, formatProvider);
        }
        #endregion

        #region Operators
        public static implicit operator float(Percentage weight)
        {
            return weight._value;
        }

        public static implicit operator Percentage(float value)
        {
            return new Percentage(value);
        }

        public static explicit operator double(Percentage weight)
        {
            return Convert.ToDouble(weight._value);
        }

        public static explicit operator Percentage(double value)
        {
            return new Percentage(value);
        }

        public static explicit operator decimal(Percentage weight)
        {
            return Convert.ToDecimal(weight._value);
        }

        public static explicit operator Percentage(decimal value)
        {
            return new Percentage((double)value);
        }
        #endregion

        public static bool TryParse(string value, out Percentage result)
        {
            float floatResult;

            bool applyScaling = false;

            if (value == null)
                throw new ArgumentNullException("value");

            string valueString = value.Trim();
            if (valueString.EndsWith("%"))
            {
                valueString = valueString.Substring(0, valueString.Length - 1);
                applyScaling = true;
            }

            if (float.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out floatResult))
            {
                if (applyScaling)
                    floatResult /= 100f;

                // NOTE: Error correction and precsion limiting is applied during the float->Percentage conversion.

                result = floatResult;
                return true;
            }

            result = default(Percentage);
            return false;
        }

        /// <summary>
        /// This method limits the precision of Percentage values and attempts to detect and correct
        /// floating point errors in the process.
        /// </summary>
        /// <param name="value">The value to check for errors and correct.</param>
        /// <remarks>
        /// This method works by first rounding <paramref name="value"/> to the nearest whole
        /// percentage and checking to see if the rounded value is very close to the original value.
        /// If so, then the rounded value is accepted.  Otherwise, this process is repeated by
        /// rounding to the nearest tenth of a percent.  If the resulting values from the second
        /// check are very close, the rounded value is taken; otherwise, the original value is
        /// rounded to the nearest hundredth of a percent.
        /// </remarks>
        private static void ApplyErrorCorrectionAndLimitPrecision(ref float value)
        {
            float nearestWholePercentage = (float)Math.Round(value, 2);
            if (FloatUtil.AreClose(value, nearestWholePercentage))
            {
                value = nearestWholePercentage;
                return;
            }

            float nearestTenthPercentage = (float)Math.Round(value, 3);
            if (FloatUtil.AreClose(value, nearestTenthPercentage))
            {
                value = nearestTenthPercentage;
                return;
            }

            value = (float)Math.Round(value, 4);
        }

        public static Percentage Parse(string value)
        {
            Percentage result;
            TryParse(value, out result);
            return result;
        }
    }

    public sealed class PercentageConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(IConvertible).IsAssignableFrom(sourceType))
                return true;
            return TypeDescriptor.GetConverter(typeof(float)).CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return TypeDescriptor.GetConverter(typeof(float)).CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            IConvertible convertible = value as IConvertible;
            if (value is string)
            {
                return Percentage.Parse(value.ToString());
            }
            if (convertible != null)
            {
                return new Percentage(convertible.ToSingle(CultureInfo.InvariantCulture.NumberFormat));
            }
            return TypeDescriptor.GetConverter(typeof(float)).ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String))
                return value.ToString();
            return TypeDescriptor.GetConverter(typeof(float)).ConvertTo(context, culture, value, destinationType);
        }
    }

    internal static class FloatUtil
    {
        internal const float Epsilon = 1.192093E-07f;
        internal const float MaxPrecision = 1.677722E+07f;
        internal const float InverseMaxPrecision = (1f / MaxPrecision);

        public static bool AreClose(float a, float b)
        {
            if (a == b)
                return true;

            float epsilon = ((Math.Abs(a) + Math.Abs(b)) + 10f) * Epsilon;
            float delta = a - b;

            return (-epsilon < delta) && (epsilon > delta);
        }

        public static bool IsCloseToDivideByZero(float numerator, float denominator)
        {
            return Math.Abs(denominator) <= (Math.Abs(numerator) * InverseMaxPrecision);
        }

        public static bool IsOne(float a)
        {
            return Math.Abs(a - 1f) < (10f * Epsilon);
        }

        public static bool IsZero(float a)
        {
            return Math.Abs(a) < (10f * Epsilon);
        }
    }
}
// ReSharper restore CompareOfFloatsByEqualityOperator
