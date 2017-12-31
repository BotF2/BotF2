// InvariantNumber.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;

using Supremacy.Resources;

namespace Supremacy.Types
{
    public static class Number
    {
        public static Int16 ParseInt16(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Int16 result;
                if (Int16.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Int16);
        }

        public static Int32 ParseInt32(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Int32 result;
                if (Int32.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Int32);
        }

        public static Int64 ParseInt64(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Int64 result;
                if (Int64.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Int64);
        }

        public static UInt16 ParseUInt16(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                UInt16 result;
                if (UInt16.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(UInt16);
        }

        public static UInt32 ParseUInt32(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                UInt32 result;
                if (UInt32.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(UInt32);
        }

        public static UInt64 ParseUInt64(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                UInt64 result;
                if (UInt64.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(UInt64);
        }

        public static Single ParseSingle(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Single result;
                if (Single.TryParse(value, NumberStyles.Float, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Single);
        }

        public static Double ParseDouble(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Double result;
                if (Double.TryParse(value, NumberStyles.Float, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Double);
        }

        public static Decimal ParseDecimal(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Decimal result;
                if (Decimal.TryParse(value, NumberStyles.Currency, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Decimal);
        }

        public static Byte ParseByte(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Byte result;
                if (Byte.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(Byte);
        }

        public static SByte ParseSByte(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                SByte result;
                if (SByte.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out result))
                    return result;
            }
            return default(SByte);
        }

        public static Percentage ParsePercentage(String value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Percentage result;
                if (Percentage.TryParse(value, out result))
                    return result;
            }
            return default(Percentage);
        }

        public static bool ParseBoolean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (string.Equals(Boolean.TrueString, value, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(Boolean.FalseString, value, StringComparison.OrdinalIgnoreCase))
                return false;

            return (ParseInt32(value) != 0);
        }
    }
}