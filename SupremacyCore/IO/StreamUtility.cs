// StreamUtility.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using Supremacy.IO.Compression;
using Supremacy.IO.Serialization;

namespace Supremacy.IO
{
    public static class StreamUtility
    {
        internal static BinaryFormatter CreateFormatter()
        {
            return new BinaryFormatter
                   {
                       AssemblyFormat = FormatterAssemblyStyle.Simple,
                       FilterLevel = TypeFilterLevel.Low,
                       TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
                       Context = new StreamingContext(StreamingContextStates.Persistence)
                   };
        }

        public static T Read<T>(byte[] buffer) where T : class
        {
            using (var sin = new SerializationReader(MiniLZO.Decompress(buffer)))
            {
                return sin.ReadObject() as T;
            }
        }

        public static byte[] Write(object value)
        {
            using (var sout = new SerializationWriter())
            {
                sout.OptimizeForSize = true;
                sout.WriteObject(value);
                sout.AppendTokenTables();
                sout.Flush();
                var results = MiniLZO.Compress((MemoryStream)sout.BaseStream);
                return results;
            }
        }
    }
}