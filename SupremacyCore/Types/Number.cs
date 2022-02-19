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
        public static short ParseInt16(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (short.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out short result))
                {
                    return result;
                }
            }
            return default;
        }

        public static int ParseInt32(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (int.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out int result))
                {
                    return result;
                }
            }
            return default;
        }

        public static long ParseInt64(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (long.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out long result))
                {
                    return result;
                }
            }
            return default;
        }

        public static ushort ParseUInt16(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (ushort.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out ushort result))
                {
                    return result;
                }
            }
            return default;
        }

        public static uint ParseUInt32(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (uint.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out uint result))
                {
                    return result;
                }
            }
            return default;
        }

        public static ulong ParseUInt64(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (ulong.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out ulong result))
                {
                    return result;
                }
            }
            return default;
        }

        public static float ParseSingle(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (float.TryParse(value, NumberStyles.Float, ResourceManager.NeutralCulture, out float result))
                {
                    return result;
                }
            }
            return default;
        }

        public static double ParseDouble(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (double.TryParse(value, NumberStyles.Float, ResourceManager.NeutralCulture, out double result))
                {
                    return result;
                }
            }
            return default;
        }

        public static decimal ParseDecimal(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (decimal.TryParse(value, NumberStyles.Currency, ResourceManager.NeutralCulture, out decimal result))
                {
                    return result;
                }
            }
            return default;
        }

        public static byte ParseByte(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (byte.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out byte result))
                {
                    return result;
                }
            }
            return default;
        }

        public static sbyte ParseSByte(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (sbyte.TryParse(value, NumberStyles.Integer, ResourceManager.NeutralCulture, out sbyte result))
                {
                    return result;
                }
            }
            return default;
        }

        public static Percentage ParsePercentage(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (Percentage.TryParse(value, out Percentage result))
                {
                    return result;
                }
            }
            return default;
        }

        public static bool ParseBoolean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();

            if (string.Equals(bool.TrueString, value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(bool.FalseString, value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return ParseInt32(value) != 0;
        }
    }
}