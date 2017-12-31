// TextHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.IO;
using System.Text;

namespace Supremacy.Utility
{
    public static class TextHelper
    {
        public static string TrimParagraphs(string text)
        {
            if (text == null)
                return null;
            if (text.Length == 0)
                return text;

            var result = new StringBuilder(text.Length);
            var reader = new StringReader(text);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if ((line.Length > 0) || (result.Length > 0))
                {
                    result.AppendLine(line.Trim());
                }
            }

            return result.ToString();
        }
    }
}
