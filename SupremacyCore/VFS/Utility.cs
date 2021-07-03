#region LGPL License
/*
Jad Engine Library
Copyright (C) 2007 Jad Engine Project Team

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

#region Using Statements
using System.Globalization;

#endregion

namespace Supremacy.VFS
{
    public static class Utility
    {
        #region Fields

        /// <summary>
        /// Array of valid wildcards.
        /// </summary>
        private static readonly char[] Wildcards = new[] { '*', '?' };

        #endregion

        #region Methods

        /// <summary>
        /// Evaluates whether a file path matches a specified pattern, which may include the <c>*</c> and <c>?</c> wildcards.
        /// </summary>
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="path">Filename to match.</param>
        /// <param name="caseSensitive">If the match is case sensitive or not.</param>
        /// <param name="culture">The culture to use for string comparisons.</param>
        /// <returns><c>true</c> if the <paramref name="path"/> matches the <paramref name="pattern"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Based on robagar C# port of Jack Handy Codeproject article:
        /// http://www.codeproject.com/string/wildcmp.asp#xx1000279xx
        /// </remarks>
        public static bool Match(string pattern, string path, bool caseSensitive, CultureInfo culture)
        {
            // if not concerned about case, convert both string and pattern
            // to lower case for comparison
            if (!caseSensitive)
            {
                pattern = pattern.ToLower(culture);
                path = path.ToLower(culture);
            }

            // if pattern doesn't actually contain any wildcards, use simple equality
            if (pattern.IndexOfAny(Wildcards) == -1)
            {
                return path == pattern;
            }

            // otherwise do pattern matching
            int i = 0;
            int j = 0;
            while (i < path.Length && j < pattern.Length && pattern[j] != '*')
            {
                if ((pattern[j] != path[i]) && (pattern[j] != '?'))
                {
                    return false;
                }
                i++;
                j++;
            }

            // if we have reached the end of the pattern without finding a * wildcard,
            // the match must fail if the string is longer or shorter than the pattern
            if (j == pattern.Length)
            {
                return path.Length == pattern.Length;
            }

            int cp = 0;
            int mp = 0;
            while (i < path.Length)
            {
                if (j < pattern.Length && pattern[j] == '*')
                {
                    if (j++ >= pattern.Length)
                    {
                        return true;
                    }
                    mp = j;
                    cp = i + 1;
                }
                else if (j < pattern.Length && (pattern[j] == path[i] || pattern[j] == '?'))
                {
                    j++;
                    i++;
                }
                else
                {
                    j = mp;
                    i = cp++;
                }
            }

            while (j < pattern.Length && pattern[j] == '*')
            {
                j++;
            }

            return j >= pattern.Length;
        }

        #endregion
    }
}
