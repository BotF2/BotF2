// StringTable.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using Supremacy.Types;

namespace Supremacy.Resources
{
    [Serializable]
    public class StringTable
    {
        private static readonly Regex KeyRegex = new Regex(@"^\[([^\[]+)\]$", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly Dictionary<string, string> _strings;
        private static string _text;
        private static readonly string newline = Environment.NewLine;

        public ICollection<string> Keys => _strings.Keys;

        public ICollection<string> Values => _strings.Values;

        public string this[string key]
        {
            get => _strings.ContainsKey(key) ? _strings[key] : null;
            set => _strings[key] = value;
        }

        public StringTable()
        {
            _strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private enum ReadState
        {
            ReadKey,
            ReadValue
        }

        public static StringTable Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                string _text = "#### ....could not find " + fileName + " but en.txt is fine";
                //GameLog.Client.General.ErrorFormat(_text);
                Console.WriteLine(_text);

                //throw new FileNotFoundException(
                //    "String table file could not be located: "
                //    + fileName);
            }

            string key = null;
            StringTable result = new StringTable();
            StringBuilder buffer = new StringBuilder();
            ReadState state = ReadState.ReadKey;
            List<string> lines = new List<string>(File.ReadAllLines(fileName).Select(o => o.Trim()));

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if ((line.Length == 0) || line.StartsWith("#"))
                {
                    continue;
                }

                if (state == ReadState.ReadKey)
                {
                    if (KeyRegex.IsMatch(line))
                    {
                        key = KeyRegex.Match(line).Groups[1].Value;
                        state = ReadState.ReadValue;
                        continue;
                    }

                    throw new StringTableParseException(
                        "Expected key, found something else.");
                }

                if (KeyRegex.IsMatch(line))
                {
                    --i;
                    result._strings[key] = buffer.ToString();
                    buffer.Length = 0;
                    state = ReadState.ReadKey;
                }
                else
                {
                    if (buffer.Length > 0)
                    {
                        _ = buffer.AppendLine();
                    }

                    _ = buffer.Append(line);
                }

            }

            _text = "EN.txt_Content-Key;Value;empty1;empty2;empty3";
            foreach (var item in result._strings)
            {
                _text += newline + item.Key + ";\"" + item.Value + "\"";
            }
            string file = Path.Combine(ResourceManager.GetResourcePath("Resources\\Data"),"EN-Txt-Content");
    
    
            //file = file.Replace(".\\", "");
            try
            {
                StreamWriter streamWriter = new StreamWriter(file + ".txt");
                streamWriter.Write(_text);
                streamWriter.Close();

                streamWriter = new StreamWriter(file + ".csv");
                streamWriter.Write(_text);
                streamWriter.Close();
            }
            catch (Exception)
            {

                //throw;
            }

            return result;
        }

        [Serializable]
        public class StringTableParseException : SupremacyException
        {
            public StringTableParseException()
            { }

            public StringTableParseException(string message)
                : base(message) { }

            public StringTableParseException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}
