// StringHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Text;

namespace Supremacy.Utility
{
    public static class StringHelper
    {
        public static bool IsTrue(string value)
        {
            if (value != null)
            {
                value = value.Trim();
                return (value.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || value.Equals("1", StringComparison.Ordinal));
            }
            return false;
        }

        public static bool IsFalse(string value)
        {
            if (value != null)
            {
                value = value.Trim();
                return (value.Equals("false", StringComparison.OrdinalIgnoreCase)
                        || value.Equals("0", StringComparison.Ordinal));
            }
            return false;
        }

        public static string QuoteString(string value)
        {
            if (value == null)
                return null;

            var sb = new StringBuilder(value.Length + 2);
            var bracketDepth = 0;

            sb.Append('"');

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                var last = i == 0 ? '\0' : value[i - 1];
                if (c == '{' && last != '\\')
                    ++bracketDepth;
                else if (c == '}' && last != '\\')
                    --bracketDepth;
                else if (c == '"' && bracketDepth == 0)
                    sb.Append('\\');
                sb.Append(c);
            }

            sb.Append('"');

            return sb.ToString();
        }
    }
}
