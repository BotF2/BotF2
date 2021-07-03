using System;
using System.Globalization;
using System.Threading;

namespace Supremacy.Utility
{
    public static class TryConvert
    {
        private const NumberStyles IntegerNumberStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign;
        private const NumberStyles RealNumberStyle = IntegerNumberStyle | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent;
        private const NumberStyles DecimalNumberStyle = IntegerNumberStyle | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
        private const DateTimeStyles DateTimeStyle = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

        #region Int16 Conversions

        public static short? ToInt16(string value)
        {
            return ToInt16(value, CultureInfo.CurrentCulture);
        }

        public static short? ToInt16(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (short.TryParse(value, IntegerNumberStyle, provider, out short result))
            {
                return result;
            }

            return null;
        }

        public static short? ToInt16(byte value)
        {
            return value;
        }

        public static short? ToInt16(sbyte value)
        {
            return value;
        }

        public static short? ToInt16(short value)
        {
            return value;
        }

        public static short? ToInt16(ushort value)
        {
            return (short?)value;
        }

        public static short? ToInt16(long value)
        {
            return (short?)value;
        }

        public static short? ToInt16(ulong value)
        {
            return (short?)value;
        }

        public static short? ToInt16(int value)
        {
            return (short?)value;
        }

        public static short? ToInt16(uint value)
        {
            return (short?)value;
        }

        public static short? ToInt16(float value)
        {
            return (short?)value;
        }

        public static short? ToInt16(double value)
        {
            return (short?)value;
        }

        public static short? ToInt16(decimal value)
        {
            return (short?)value;
        }

        public static short? ToInt16(bool value)
        {
            return (short?)(value ? 1 : 0);
        }

        public static short? ToInt16(char value)
        {
            return (short?)value;
        }

        public static short? ToInt16(DateTime value)
        {
            return null;
        }

        public static short? ToInt16(object value)
        {
            return ToInt16(value, CultureInfo.CurrentCulture);
        }

        public static short? ToInt16(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToInt16((bool)value);
                case TypeCode.Char:
                    return ToInt16((char)value);
                case TypeCode.SByte:
                    return ToInt16((sbyte)value);
                case TypeCode.Byte:
                    return ToInt16((byte)value);
                case TypeCode.Int16:
                    return ToInt16((short)value);
                case TypeCode.UInt16:
                    return ToInt16((ushort)value);
                case TypeCode.Int32:
                    return ToInt16((int)value);
                case TypeCode.UInt32:
                    return ToInt16((uint)value);
                case TypeCode.Int64:
                    return ToInt16((long)value);
                case TypeCode.UInt64:
                    return ToInt16((ulong)value);
                case TypeCode.Single:
                    return ToInt16((float)value);
                case TypeCode.Double:
                    return ToInt16((double)value);
                case TypeCode.Decimal:
                    return ToInt16((decimal)value);
                case TypeCode.DateTime:
                    return ToInt16((DateTime)value);
                case TypeCode.String:
                    return ToInt16((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToInt16(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region UInt16 Conversions

        public static ushort? ToUInt16(string value)
        {
            return ToUInt16(value, CultureInfo.CurrentCulture);
        }

        public static ushort? ToUInt16(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (ushort.TryParse(value, IntegerNumberStyle, provider, out ushort result))
            {
                return result;
            }

            return null;
        }

        public static ushort? ToUInt16(byte value)
        {
            return value;
        }

        public static ushort? ToUInt16(sbyte value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(short value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(ushort value)
        {
            return value;
        }

        public static ushort? ToUInt16(long value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(ulong value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(int value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(uint value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(float value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(double value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(decimal value)
        {
            return (ushort?)value;
        }

        public static ushort? ToUInt16(bool value)
        {
            return (ushort?)(value ? 1 : 0);
        }

        public static ushort? ToUInt16(char value)
        {
            return value;
        }

        public static ushort? ToUInt16(DateTime value)
        {
            return null;
        }

        public static ushort? ToUInt16(object value)
        {
            return ToUInt16(value, CultureInfo.CurrentCulture);
        }

        public static ushort? ToUInt16(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToUInt16((bool)value);
                case TypeCode.Char:
                    return ToUInt16((char)value);
                case TypeCode.SByte:
                    return ToUInt16((sbyte)value);
                case TypeCode.Byte:
                    return ToUInt16((byte)value);
                case TypeCode.Int16:
                    return ToUInt16((short)value);
                case TypeCode.UInt16:
                    return ToUInt16((ushort)value);
                case TypeCode.Int32:
                    return ToUInt16((int)value);
                case TypeCode.UInt32:
                    return ToUInt16((uint)value);
                case TypeCode.Int64:
                    return ToUInt16((long)value);
                case TypeCode.UInt64:
                    return ToUInt16((ulong)value);
                case TypeCode.Single:
                    return ToUInt16((float)value);
                case TypeCode.Double:
                    return ToUInt16((double)value);
                case TypeCode.Decimal:
                    return ToUInt16((decimal)value);
                case TypeCode.DateTime:
                    return ToUInt16((DateTime)value);
                case TypeCode.String:
                    return ToUInt16((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToUInt16(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Int32 Conversions

        public static int? ToInt32(string value)
        {
            return ToInt32(value, CultureInfo.CurrentCulture);
        }

        public static int? ToInt32(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value, IntegerNumberStyle, provider, out int result))
            {
                return result;
            }

            return null;
        }

        public static int? ToInt32(byte value)
        {
            return value;
        }

        public static int? ToInt32(sbyte value)
        {
            return value;
        }

        public static int? ToInt32(short value)
        {
            return value;
        }

        public static int? ToInt32(ushort value)
        {
            return value;
        }

        public static int? ToInt32(long value)
        {
            return (int?)value;
        }

        public static int? ToInt32(ulong value)
        {
            return (int?)value;
        }

        public static int? ToInt32(int value)
        {
            return value;
        }

        public static int? ToInt32(uint value)
        {
            return (int?)value;
        }

        public static int? ToInt32(float value)
        {
            return (int?)value;
        }

        public static int? ToInt32(double value)
        {
            return (int?)value;
        }

        public static int? ToInt32(decimal value)
        {
            return (int?)value;
        }

        public static int? ToInt32(bool value)
        {
            return value ? 1 : 0;
        }

        public static int? ToInt32(char value)
        {
            return value;
        }

        public static int? ToInt32(DateTime value)
        {
            return null;
        }

        public static int? ToInt32(object value)
        {
            return ToInt32(value, CultureInfo.CurrentCulture);
        }

        public static int? ToInt32(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToInt32((bool)value);
                case TypeCode.Char:
                    return ToInt32((char)value);
                case TypeCode.SByte:
                    return ToInt32((sbyte)value);
                case TypeCode.Byte:
                    return ToInt32((byte)value);
                case TypeCode.Int16:
                    return ToInt32((short)value);
                case TypeCode.UInt16:
                    return ToInt32((ushort)value);
                case TypeCode.Int32:
                    return ToInt32((int)value);
                case TypeCode.UInt32:
                    return ToInt32((uint)value);
                case TypeCode.Int64:
                    return ToInt32((long)value);
                case TypeCode.UInt64:
                    return ToInt32((ulong)value);
                case TypeCode.Single:
                    return ToInt32((float)value);
                case TypeCode.Double:
                    return ToInt32((double)value);
                case TypeCode.Decimal:
                    return ToInt32((decimal)value);
                case TypeCode.DateTime:
                    return ToInt32((DateTime)value);
                case TypeCode.String:
                    return ToInt32((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToInt32(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region UInt32 Conversions

        public static uint? ToUInt32(string value)
        {
            return ToUInt32(value, CultureInfo.CurrentCulture);
        }

        public static uint? ToUInt32(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (uint.TryParse(value, IntegerNumberStyle, provider, out uint result))
            {
                return result;
            }

            return null;
        }

        public static uint? ToUInt32(byte value)
        {
            return value;
        }

        public static uint? ToUInt32(sbyte value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(short value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(ushort value)
        {
            return value;
        }

        public static uint? ToUInt32(long value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(ulong value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(int value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(uint value)
        {
            return value;
        }

        public static uint? ToUInt32(float value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(double value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(decimal value)
        {
            return (uint?)value;
        }

        public static uint? ToUInt32(bool value)
        {
            return (uint?)(value ? 1 : 0);
        }

        public static uint? ToUInt32(char value)
        {
            return value;
        }

        public static uint? ToUInt32(DateTime value)
        {
            return null;
        }

        public static uint? ToUInt32(object value)
        {
            return ToUInt32(value, CultureInfo.CurrentCulture);
        }

        public static uint? ToUInt32(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToUInt32((bool)value);
                case TypeCode.Char:
                    return ToUInt32((char)value);
                case TypeCode.SByte:
                    return ToUInt32((sbyte)value);
                case TypeCode.Byte:
                    return ToUInt32((byte)value);
                case TypeCode.Int16:
                    return ToUInt32((short)value);
                case TypeCode.UInt16:
                    return ToUInt32((ushort)value);
                case TypeCode.Int32:
                    return ToUInt32((int)value);
                case TypeCode.UInt32:
                    return ToUInt32((uint)value);
                case TypeCode.Int64:
                    return ToUInt32((long)value);
                case TypeCode.UInt64:
                    return ToUInt32((ulong)value);
                case TypeCode.Single:
                    return ToUInt32((float)value);
                case TypeCode.Double:
                    return ToUInt32((double)value);
                case TypeCode.Decimal:
                    return ToUInt32((decimal)value);
                case TypeCode.DateTime:
                    return ToUInt32((DateTime)value);
                case TypeCode.String:
                    return ToUInt32((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToUInt32(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Int64 Conversions

        public static long? ToInt64(string value)
        {
            return ToInt64(value, CultureInfo.CurrentCulture);
        }

        public static long? ToInt64(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (long.TryParse(value, IntegerNumberStyle, provider, out long result))
            {
                return result;
            }

            return null;
        }

        public static long? ToInt64(byte value)
        {
            return value;
        }

        public static long? ToInt64(sbyte value)
        {
            return value;
        }

        public static long? ToInt64(short value)
        {
            return value;
        }

        public static long? ToInt64(ushort value)
        {
            return value;
        }

        public static long? ToInt64(long value)
        {
            return value;
        }

        public static long? ToInt64(ulong value)
        {
            return (long?)value;
        }

        public static long? ToInt64(int value)
        {
            return value;
        }

        public static long? ToInt64(uint value)
        {
            return value;
        }

        public static long? ToInt64(float value)
        {
            return (long?)value;
        }

        public static long? ToInt64(double value)
        {
            return (long?)value;
        }

        public static long? ToInt64(decimal value)
        {
            return (long?)value;
        }

        public static long? ToInt64(bool value)
        {
            return value ? 1 : 0;
        }

        public static long? ToInt64(char value)
        {
            return value;
        }

        public static long? ToInt64(DateTime value)
        {
            return null;
        }

        public static long? ToInt64(object value)
        {
            return ToInt64(value, CultureInfo.CurrentCulture);
        }

        public static long? ToInt64(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToInt64((bool)value);
                case TypeCode.Char:
                    return ToInt64((char)value);
                case TypeCode.SByte:
                    return ToInt64((sbyte)value);
                case TypeCode.Byte:
                    return ToInt64((byte)value);
                case TypeCode.Int16:
                    return ToInt64((short)value);
                case TypeCode.UInt16:
                    return ToInt64((ushort)value);
                case TypeCode.Int32:
                    return ToInt64((int)value);
                case TypeCode.UInt32:
                    return ToInt64((uint)value);
                case TypeCode.Int64:
                    return ToInt64((long)value);
                case TypeCode.UInt64:
                    return ToInt64((ulong)value);
                case TypeCode.Single:
                    return ToInt64((float)value);
                case TypeCode.Double:
                    return ToInt64((double)value);
                case TypeCode.Decimal:
                    return ToInt64((decimal)value);
                case TypeCode.DateTime:
                    return ToInt64((DateTime)value);
                case TypeCode.String:
                    return ToInt64((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToInt64(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region UInt64 Conversions

        public static ulong? ToUInt64(string value)
        {
            return ToUInt64(value, CultureInfo.CurrentCulture);
        }

        public static ulong? ToUInt64(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (ulong.TryParse(value, IntegerNumberStyle, provider, out ulong result))
            {
                return result;
            }

            return null;
        }

        public static ulong? ToUInt64(byte value)
        {
            return value;
        }

        public static ulong? ToUInt64(sbyte value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(short value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(ushort value)
        {
            return value;
        }

        public static ulong? ToUInt64(long value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(ulong value)
        {
            return value;
        }

        public static ulong? ToUInt64(int value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(uint value)
        {
            return value;
        }

        public static ulong? ToUInt64(float value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(double value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(decimal value)
        {
            return (ulong?)value;
        }

        public static ulong? ToUInt64(bool value)
        {
            return value ? 1ul : 0ul;
        }

        public static ulong? ToUInt64(char value)
        {
            return value;
        }

        public static ulong? ToUInt64(DateTime value)
        {
            return null;
        }

        public static ulong? ToUInt64(object value)
        {
            return ToUInt64(value, CultureInfo.CurrentCulture);
        }

        public static ulong? ToUInt64(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToUInt64((bool)value);
                case TypeCode.Char:
                    return ToUInt64((char)value);
                case TypeCode.SByte:
                    return ToUInt64((sbyte)value);
                case TypeCode.Byte:
                    return ToUInt64((byte)value);
                case TypeCode.Int16:
                    return ToUInt64((short)value);
                case TypeCode.UInt16:
                    return ToUInt64((ushort)value);
                case TypeCode.Int32:
                    return ToUInt64((int)value);
                case TypeCode.UInt32:
                    return ToUInt64((uint)value);
                case TypeCode.Int64:
                    return ToUInt64((long)value);
                case TypeCode.UInt64:
                    return ToUInt64((ulong)value);
                case TypeCode.Single:
                    return ToUInt64((float)value);
                case TypeCode.Double:
                    return ToUInt64((double)value);
                case TypeCode.Decimal:
                    return ToUInt64((decimal)value);
                case TypeCode.DateTime:
                    return ToUInt64((DateTime)value);
                case TypeCode.String:
                    return ToUInt64((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToUInt64(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Byte Conversions

        public static byte? ToByte(string value)
        {
            return ToByte(value, CultureInfo.CurrentCulture);
        }

        public static byte? ToByte(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (byte.TryParse(value, IntegerNumberStyle, provider, out byte result))
            {
                return result;
            }

            return null;
        }

        public static byte? ToByte(byte value)
        {
            return value;
        }

        public static byte? ToByte(sbyte value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(short value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(ushort value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(long value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(ulong value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(int value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(uint value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(float value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(double value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(decimal value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(bool value)
        {
            return (byte?)(value ? 1 : 0);
        }

        public static byte? ToByte(char value)
        {
            return (byte?)value;
        }

        public static byte? ToByte(DateTime value)
        {
            return null;
        }

        public static byte? ToByte(object value)
        {
            return ToByte(value, CultureInfo.CurrentCulture);
        }

        public static byte? ToByte(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToByte((bool)value);
                case TypeCode.Char:
                    return ToByte((char)value);
                case TypeCode.SByte:
                    return ToByte((sbyte)value);
                case TypeCode.Byte:
                    return ToByte((byte)value);
                case TypeCode.Int16:
                    return ToByte((short)value);
                case TypeCode.UInt16:
                    return ToByte((ushort)value);
                case TypeCode.Int32:
                    return ToByte((int)value);
                case TypeCode.UInt32:
                    return ToByte((uint)value);
                case TypeCode.Int64:
                    return ToByte((long)value);
                case TypeCode.UInt64:
                    return ToByte((ulong)value);
                case TypeCode.Single:
                    return ToByte((float)value);
                case TypeCode.Double:
                    return ToByte((double)value);
                case TypeCode.Decimal:
                    return ToByte((decimal)value);
                case TypeCode.DateTime:
                    return ToByte((DateTime)value);
                case TypeCode.String:
                    return ToByte((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToByte(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region SByte Conversions

        public static sbyte? ToSByte(string value)
        {
            return ToSByte(value, CultureInfo.CurrentCulture);
        }

        public static sbyte? ToSByte(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (sbyte.TryParse(value, IntegerNumberStyle, provider, out sbyte result))
            {
                return result;
            }

            return null;
        }

        public static sbyte? ToSByte(byte value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(sbyte value)
        {
            return value;
        }

        public static sbyte? ToSByte(short value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(ushort value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(long value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(ulong value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(int value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(uint value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(float value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(double value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(decimal value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(bool value)
        {
            return (sbyte?)(value ? 1 : 0);
        }

        public static sbyte? ToSByte(char value)
        {
            return (sbyte?)value;
        }

        public static sbyte? ToSByte(DateTime value)
        {
            return null;
        }

        public static sbyte? ToSByte(object value)
        {
            return ToSByte(value, CultureInfo.CurrentCulture);
        }

        public static sbyte? ToSByte(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToSByte((bool)value);
                case TypeCode.Char:
                    return ToSByte((char)value);
                case TypeCode.SByte:
                    return ToSByte((sbyte)value);
                case TypeCode.Byte:
                    return ToSByte((byte)value);
                case TypeCode.Int16:
                    return ToSByte((short)value);
                case TypeCode.UInt16:
                    return ToSByte((ushort)value);
                case TypeCode.Int32:
                    return ToSByte((int)value);
                case TypeCode.UInt32:
                    return ToSByte((uint)value);
                case TypeCode.Int64:
                    return ToSByte((long)value);
                case TypeCode.UInt64:
                    return ToSByte((ulong)value);
                case TypeCode.Single:
                    return ToSByte((float)value);
                case TypeCode.Double:
                    return ToSByte((double)value);
                case TypeCode.Decimal:
                    return ToSByte((decimal)value);
                case TypeCode.DateTime:
                    return ToSByte((DateTime)value);
                case TypeCode.String:
                    return ToSByte((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToSByte(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Boolean Conversions

        public static bool? ToBoolean(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return null;
        }

        public static bool? ToBoolean(string value, IFormatProvider provider)
        {
            return ToBoolean(value);
        }

        public static bool? ToBoolean(byte value)
        {
            return value != 0;
        }

        public static bool? ToBoolean(sbyte value)
        {
            return value != 0;
        }

        public static bool? ToBoolean(short value)
        {
            return value != 0;
        }

        public static bool? ToBoolean(ushort value)
        {
            return value != 0;
        }

        public static bool? ToBoolean(long value)
        {
            return value != 0L;
        }

        public static bool? ToBoolean(ulong value)
        {
            return value != 0ul;
        }

        public static bool? ToBoolean(int value)
        {
            return value != 0;
        }

        public static bool? ToBoolean(uint value)
        {
            return value != 0u;
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public static bool? ToBoolean(float value)
        {
            return value != 0f;
        }

        public static bool? ToBoolean(double value)
        {
            return value != 0d;
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        public static bool? ToBoolean(decimal value)
        {
            return value != 0m;
        }

        public static bool? ToBoolean(bool value)
        {
            return value;
        }

        public static bool? ToBoolean(char value)
        {
            return null;
        }

        public static bool? ToBoolean(DateTime value)
        {
            return null;
        }

        public static bool? ToBoolean(object value)
        {
            return ToBoolean(value, CultureInfo.CurrentCulture);
        }

        public static bool? ToBoolean(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToBoolean((bool)value);
                case TypeCode.Char:
                    return ToBoolean((char)value);
                case TypeCode.SByte:
                    return ToBoolean((sbyte)value);
                case TypeCode.Byte:
                    return ToBoolean((byte)value);
                case TypeCode.Int16:
                    return ToBoolean((short)value);
                case TypeCode.UInt16:
                    return ToBoolean((ushort)value);
                case TypeCode.Int32:
                    return ToBoolean((int)value);
                case TypeCode.UInt32:
                    return ToBoolean((uint)value);
                case TypeCode.Int64:
                    return ToBoolean((long)value);
                case TypeCode.UInt64:
                    return ToBoolean((ulong)value);
                case TypeCode.Single:
                    return ToBoolean((float)value);
                case TypeCode.Double:
                    return ToBoolean((double)value);
                case TypeCode.Decimal:
                    return ToBoolean((decimal)value);
                case TypeCode.DateTime:
                    return ToBoolean((DateTime)value);
                case TypeCode.String:
                    return ToBoolean((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToBoolean(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Double Conversions

        public static double? ToDouble(string value)
        {
            return ToDouble(value, CultureInfo.CurrentCulture);
        }

        public static double? ToDouble(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (double.TryParse(value, RealNumberStyle, provider, out double result))
            {
                return result;
            }

            return null;
        }

        public static double? ToDouble(byte value)
        {
            return value;
        }

        public static double? ToDouble(sbyte value)
        {
            return value;
        }

        public static double? ToDouble(short value)
        {
            return value;
        }

        public static double? ToDouble(ushort value)
        {
            return value;
        }

        public static double? ToDouble(long value)
        {
            return value;
        }

        public static double? ToDouble(ulong value)
        {
            return value;
        }

        public static double? ToDouble(int value)
        {
            return value;
        }

        public static double? ToDouble(uint value)
        {
            return value;
        }

        public static double? ToDouble(float value)
        {
            return value;
        }

        public static double? ToDouble(double value)
        {
            return value;
        }

        public static double? ToDouble(decimal value)
        {
            return (double?)value;
        }

        public static double? ToDouble(bool value)
        {
            return value ? 1 : 0;
        }

        public static double? ToDouble(char value)
        {
            return value;
        }

        public static double? ToDouble(DateTime value)
        {
            return null;
        }

        public static double? ToDouble(object value)
        {
            return ToDouble(value, CultureInfo.CurrentCulture);
        }

        public static double? ToDouble(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToDouble((bool)value);
                case TypeCode.Char:
                    return ToDouble((char)value);
                case TypeCode.SByte:
                    return ToDouble((sbyte)value);
                case TypeCode.Byte:
                    return ToDouble((byte)value);
                case TypeCode.Int16:
                    return ToDouble((short)value);
                case TypeCode.UInt16:
                    return ToDouble((ushort)value);
                case TypeCode.Int32:
                    return ToDouble((int)value);
                case TypeCode.UInt32:
                    return ToDouble((uint)value);
                case TypeCode.Int64:
                    return ToDouble((long)value);
                case TypeCode.UInt64:
                    return ToDouble((ulong)value);
                case TypeCode.Single:
                    return ToDouble((float)value);
                case TypeCode.Double:
                    return ToDouble((double)value);
                case TypeCode.Decimal:
                    return ToDouble((decimal)value);
                case TypeCode.DateTime:
                    return ToDouble((DateTime)value);
                case TypeCode.String:
                    return ToDouble((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToDouble(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Single Conversions

        public static float? ToSingle(string value)
        {
            return ToSingle(value, CultureInfo.CurrentCulture);
        }

        public static float? ToSingle(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (float.TryParse(value, RealNumberStyle, provider, out float result))
            {
                return result;
            }

            return null;
        }

        public static float? ToSingle(byte value)
        {
            return value;
        }

        public static float? ToSingle(sbyte value)
        {
            return value;
        }

        public static float? ToSingle(short value)
        {
            return value;
        }

        public static float? ToSingle(ushort value)
        {
            return value;
        }

        public static float? ToSingle(long value)
        {
            return value;
        }

        public static float? ToSingle(ulong value)
        {
            return value;
        }

        public static float? ToSingle(int value)
        {
            return value;
        }

        public static float? ToSingle(uint value)
        {
            return value;
        }

        public static float? ToSingle(float value)
        {
            return value;
        }

        public static float? ToSingle(double value)
        {
            return (float?)value;
        }

        public static float? ToSingle(decimal value)
        {
            return (float?)value;
        }

        public static float? ToSingle(bool value)
        {
            return value ? 1 : 0;
        }

        public static float? ToSingle(char value)
        {
            return value;
        }

        public static float? ToSingle(DateTime value)
        {
            return null;
        }

        public static float? ToSingle(object value)
        {
            return ToSingle(value, CultureInfo.CurrentCulture);
        }

        public static float? ToSingle(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToSingle((bool)value);
                case TypeCode.Char:
                    return ToSingle((char)value);
                case TypeCode.SByte:
                    return ToSingle((sbyte)value);
                case TypeCode.Byte:
                    return ToSingle((byte)value);
                case TypeCode.Int16:
                    return ToSingle((short)value);
                case TypeCode.UInt16:
                    return ToSingle((ushort)value);
                case TypeCode.Int32:
                    return ToSingle((int)value);
                case TypeCode.UInt32:
                    return ToSingle((uint)value);
                case TypeCode.Int64:
                    return ToSingle((long)value);
                case TypeCode.UInt64:
                    return ToSingle((ulong)value);
                case TypeCode.Single:
                    return ToSingle((float)value);
                case TypeCode.Double:
                    return ToSingle((double)value);
                case TypeCode.Decimal:
                    return ToSingle((decimal)value);
                case TypeCode.DateTime:
                    return ToSingle((DateTime)value);
                case TypeCode.String:
                    return ToSingle((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToSingle(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Decimal Conversions

        public static decimal? ToDecimal(string value)
        {
            return ToDecimal(value, CultureInfo.CurrentCulture);
        }

        public static decimal? ToDecimal(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (decimal.TryParse(value, DecimalNumberStyle, provider, out decimal result))
            {
                return result;
            }

            return null;
        }

        public static decimal? ToDecimal(byte value)
        {
            return value;
        }

        public static decimal? ToDecimal(sbyte value)
        {
            return value;
        }

        public static decimal? ToDecimal(short value)
        {
            return value;
        }

        public static decimal? ToDecimal(ushort value)
        {
            return value;
        }

        public static decimal? ToDecimal(long value)
        {
            return value;
        }

        public static decimal? ToDecimal(ulong value)
        {
            return value;
        }

        public static decimal? ToDecimal(int value)
        {
            return value;
        }

        public static decimal? ToDecimal(uint value)
        {
            return value;
        }

        public static decimal? ToDecimal(float value)
        {
            return (decimal?)value;
        }

        public static decimal? ToDecimal(double value)
        {
            return (decimal?)value;
        }

        public static decimal? ToDecimal(decimal value)
        {
            return value;
        }

        public static decimal? ToDecimal(bool value)
        {
            return value ? 1 : 0;
        }

        public static decimal? ToDecimal(char value)
        {
            return value;
        }

        public static decimal? ToDecimal(DateTime value)
        {
            return null;
        }

        public static decimal? ToDecimal(object value)
        {
            return ToDecimal(value, CultureInfo.CurrentCulture);
        }

        public static decimal? ToDecimal(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToDecimal((bool)value);
                case TypeCode.Char:
                    return ToDecimal((char)value);
                case TypeCode.SByte:
                    return ToDecimal((sbyte)value);
                case TypeCode.Byte:
                    return ToDecimal((byte)value);
                case TypeCode.Int16:
                    return ToDecimal((short)value);
                case TypeCode.UInt16:
                    return ToDecimal((ushort)value);
                case TypeCode.Int32:
                    return ToDecimal((int)value);
                case TypeCode.UInt32:
                    return ToDecimal((uint)value);
                case TypeCode.Int64:
                    return ToDecimal((long)value);
                case TypeCode.UInt64:
                    return ToDecimal((ulong)value);
                case TypeCode.Single:
                    return ToDecimal((float)value);
                case TypeCode.Double:
                    return ToDecimal((double)value);
                case TypeCode.Decimal:
                    return ToDecimal((decimal)value);
                case TypeCode.DateTime:
                    return ToDecimal((DateTime)value);
                case TypeCode.String:
                    return ToDecimal((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToDecimal(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region Char Conversions

        public static char? ToChar(string value)
        {
            return ToChar(value, CultureInfo.CurrentCulture);
        }

        public static char? ToChar(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length == 1)
            {
                return value[0];
            }

            return null;
        }

        public static char? ToChar(byte value)
        {
            return (char?)value;
        }

        public static char? ToChar(sbyte value)
        {
            if (value < 0)
            {
                return null;
            }

            return (char?)value;
        }

        public static char? ToChar(short value)
        {
            if (value < 0)
            {
                return null;
            }

            return (char?)value;
        }

        public static char? ToChar(ushort value)
        {
            return (char?)value;
        }

        public static char? ToChar(long value)
        {
            if (value < 0)
            {
                return null;
            }

            return (char?)value;
        }

        public static char? ToChar(ulong value)
        {
            return (char?)value;
        }

        public static char? ToChar(int value)
        {
            if (value < 0)
            {
                return null;
            }

            return (char?)value;
        }

        public static char? ToChar(uint value)
        {
            return (char?)value;
        }

        public static char? ToChar(float value)
        {
            return null;
        }

        public static char? ToChar(double value)
        {
            return null;
        }

        public static char? ToChar(decimal value)
        {
            return null;
        }

        public static char? ToChar(bool value)
        {
            return null;
        }

        public static char? ToChar(char value)
        {
            return value;
        }

        public static char? ToChar(DateTime value)
        {
            return null;
        }

        public static char? ToChar(object value)
        {
            return ToChar(value, CultureInfo.CurrentCulture);
        }

        public static char? ToChar(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToChar((bool)value);
                case TypeCode.Char:
                    return ToChar((char)value);
                case TypeCode.SByte:
                    return ToChar((sbyte)value);
                case TypeCode.Byte:
                    return ToChar((byte)value);
                case TypeCode.Int16:
                    return ToChar((short)value);
                case TypeCode.UInt16:
                    return ToChar((ushort)value);
                case TypeCode.Int32:
                    return ToChar((int)value);
                case TypeCode.UInt32:
                    return ToChar((uint)value);
                case TypeCode.Int64:
                    return ToChar((long)value);
                case TypeCode.UInt64:
                    return ToChar((ulong)value);
                case TypeCode.Single:
                    return ToChar((float)value);
                case TypeCode.Double:
                    return ToChar((double)value);
                case TypeCode.Decimal:
                    return ToChar((decimal)value);
                case TypeCode.DateTime:
                    return ToChar((DateTime)value);
                case TypeCode.String:
                    return ToChar((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToChar(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region DateTime Conversions

        public static DateTime? ToDateTime(string value)
        {
            return ToDateTime(value, CultureInfo.CurrentCulture);
        }

        public static DateTime? ToDateTime(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            if (DateTime.TryParse(value, provider, DateTimeStyle, out DateTime result))
            {
                return result;
            }

            return null;
        }

        public static DateTime? ToDateTime(byte value)
        {
            return null;
        }

        public static DateTime? ToDateTime(sbyte value)
        {
            return null;
        }

        public static DateTime? ToDateTime(short value)
        {
            return null;
        }

        public static DateTime? ToDateTime(ushort value)
        {
            return null;
        }

        public static DateTime? ToDateTime(long value)
        {
            return null;
        }

        public static DateTime? ToDateTime(ulong value)
        {
            return null;
        }

        public static DateTime? ToDateTime(int value)
        {
            return null;
        }

        public static DateTime? ToDateTime(uint value)
        {
            return null;
        }

        public static DateTime? ToDateTime(float value)
        {
            return null;
        }

        public static DateTime? ToDateTime(double value)
        {
            return null;
        }

        public static DateTime? ToDateTime(decimal value)
        {
            return null;
        }

        public static DateTime? ToDateTime(bool value)
        {
            return null;
        }

        public static DateTime? ToDateTime(char value)
        {
            return null;
        }

        public static DateTime? ToDateTime(DateTime value)
        {
            return value;
        }

        public static DateTime? ToDateTime(object value)
        {
            return ToDateTime(value, CultureInfo.CurrentCulture);
        }

        public static DateTime? ToDateTime(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return null;
            }

            IConvertible convertible = value as IConvertible;
            TypeCode typeCode = (convertible != null) ? convertible.GetTypeCode() : Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToDateTime((bool)value);
                case TypeCode.Char:
                    return ToDateTime((char)value);
                case TypeCode.SByte:
                    return ToDateTime((sbyte)value);
                case TypeCode.Byte:
                    return ToDateTime((byte)value);
                case TypeCode.Int16:
                    return ToDateTime((short)value);
                case TypeCode.UInt16:
                    return ToDateTime((ushort)value);
                case TypeCode.Int32:
                    return ToDateTime((int)value);
                case TypeCode.UInt32:
                    return ToDateTime((uint)value);
                case TypeCode.Int64:
                    return ToDateTime((long)value);
                case TypeCode.UInt64:
                    return ToDateTime((ulong)value);
                case TypeCode.Single:
                    return ToDateTime((float)value);
                case TypeCode.Double:
                    return ToDateTime((double)value);
                case TypeCode.Decimal:
                    return ToDateTime((decimal)value);
                case TypeCode.DateTime:
                    return ToDateTime((DateTime)value);
                case TypeCode.String:
                    return ToDateTime((string)value);
            }

            if (convertible != null)
            {
                try { return convertible.ToDateTime(provider); }
                catch { return null; }
            }

            return null;
        }

        #endregion

        #region String Conversions

        public static string ToString(string value)
        {
            return value;
        }

        public static string ToString(string value, IFormatProvider provider)
        {
            return value;
        }

        public static string ToString(byte value)
        {
            return value.ToString();
        }

        public static string ToString(sbyte value)
        {
            return value.ToString();
        }

        public static string ToString(short value)
        {
            return value.ToString();
        }

        public static string ToString(ushort value)
        {
            return value.ToString();
        }

        public static string ToString(long value)
        {
            return value.ToString();
        }

        public static string ToString(ulong value)
        {
            return value.ToString();
        }

        public static string ToString(int value)
        {
            return value.ToString();
        }

        public static string ToString(uint value)
        {
            return value.ToString();
        }

        public static string ToString(float value)
        {
            return value.ToString();
        }

        public static string ToString(double value)
        {
            return value.ToString();
        }

        public static string ToString(decimal value)
        {
            return value.ToString();
        }

        public static string ToString(bool value)
        {
            return value.ToString();
        }

        public static string ToString(char value)
        {
            return value.ToString();
        }

        public static string ToString(DateTime value)
        {
            return value.ToShortTimeString();
        }

        public static string ToString(byte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(sbyte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(short value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(ushort value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(long value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(ulong value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(int value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(uint value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(float value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(double value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(decimal value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(bool value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(char value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(DateTime value, IFormatProvider provider)
        {
            return value.ToShortTimeString();
        }

        public static string ToString(object value)
        {
            return ToString(value, CultureInfo.CurrentCulture);
        }

        public static string ToString(object value, IFormatProvider provider)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is IConvertible convertible)
            {
                return convertible.ToString(provider);
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, provider);
            }

            return value.ToString();
        }

        #endregion

        #region ChangeType

        public static object ChangeType(object value, Type conversionType)
        {
            return ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
        }

        public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
        {
            if (conversionType == null)
            {
                return null;
            }

            if (!(value is IConvertible convertible))
            {
                if (value != null && value.GetType() == conversionType)
                {
                    return value;
                }

                return null;
            }

            TypeCode typeCode = Type.GetTypeCode(conversionType);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToDateTime(value);
                case TypeCode.Char:
                    return ToDateTime(value);
                case TypeCode.SByte:
                    return ToDateTime(value);
                case TypeCode.Byte:
                    return ToDateTime(value);
                case TypeCode.Int16:
                    return ToDateTime(value);
                case TypeCode.UInt16:
                    return ToDateTime(value);
                case TypeCode.Int32:
                    return ToDateTime(value);
                case TypeCode.UInt32:
                    return ToDateTime(value);
                case TypeCode.Int64:
                    return ToDateTime(value);
                case TypeCode.UInt64:
                    return ToDateTime(value);
                case TypeCode.Single:
                    return ToDateTime(value);
                case TypeCode.Double:
                    return ToDateTime(value);
                case TypeCode.Decimal:
                    return ToDateTime(value);
                case TypeCode.DateTime:
                    return ToDateTime(value);
                case TypeCode.String:
                    return ToDateTime(value);
                default:
                    return convertible.ToType(conversionType, provider);
            }
        }

        public static object ChangeType(object value, TypeCode typeCode)
        {
            return ChangeType(value, typeCode, Thread.CurrentThread.CurrentCulture);
        }

        public static object ChangeType(object value, TypeCode typeCode, IFormatProvider provider)
        {
            if (!(value is IConvertible convertible))
            {
                return null;
            }

            switch (typeCode)
            {
                case TypeCode.Object:
                    return value;
                case TypeCode.Boolean:
                    return convertible.ToBoolean(provider);
                case TypeCode.Char:
                    return convertible.ToChar(provider);
                case TypeCode.SByte:
                    return convertible.ToSByte(provider);
                case TypeCode.Byte:
                    return convertible.ToByte(provider);
                case TypeCode.Int16:
                    return convertible.ToInt16(provider);
                case TypeCode.UInt16:
                    return convertible.ToUInt16(provider);
                case TypeCode.Int32:
                    return convertible.ToInt32(provider);
                case TypeCode.UInt32:
                    return convertible.ToUInt32(provider);
                case TypeCode.Int64:
                    return convertible.ToInt64(provider);
                case TypeCode.UInt64:
                    return convertible.ToUInt64(provider);
                case TypeCode.Single:
                    return convertible.ToSingle(provider);
                case TypeCode.Double:
                    return convertible.ToDouble(provider);
                case TypeCode.Decimal:
                    return convertible.ToDecimal(provider);
                case TypeCode.DateTime:
                    return convertible.ToDateTime(provider);
                case TypeCode.String:
                    return convertible.ToString(provider);
                default:
                    return null;
            }
        }

        #endregion
    }
}