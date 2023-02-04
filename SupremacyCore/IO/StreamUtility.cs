// File:StreamUtility.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Supremacy.IO.Compression;
using Supremacy.IO.Serialization;

namespace Supremacy.IO
{
    public static class StreamUtility
    {
#pragma warning disable IDE0052 // Ungelesene private Member entfernen
        private static string _text;
        private static readonly string newline = Environment.NewLine;
#pragma warning restore IDE0052 // Ungelesene private Member entfernen

        private static int count;
        private static string c_hex;
        private static string X2_text;

        //private static int _count;

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
            using (SerializationReader sin = new SerializationReader(MiniLZO.Decompress(buffer)))
            {
                count = 0;
                //while (count > sin.BytesRemaining)
                //{
                //    _text += sin.ReadObject().ToString();
                //    count += 1;
                //}
                //    _text = i + buffer[i].ToString() + newline;
                //}
                //Console.WriteLine("HEX-Reading: " + _text + ", out of buffer");
                Console.WriteLine("HEX-Reading: - output deactivated-");
                c_hex = "";
                X2_text = "";
                _text = c_hex + X2_text + count;
                

                //Console.WriteLine("HEX-Reading-SIN: " + _text + ", out of buffer" + sin.ToString());

                //count = 0;
                //int hexcount = 0;
                //_text = "";
                //for (int i = 0; i < buffer.Length; i++)
                //{
                //    char c = (char)buffer[i];
                //    hexcount++;
                //    if (hexcount != 15)
                //    {
                //        c_hex += c + " - ";
                //        X2_text += buffer[i].ToString("X2") + " ";

                //        //hexcount = 0;   
                //    }
                //    else
                //    {
                //        c_hex += c + " - ";
                //        X2_text += buffer[i].ToString("X2") + " ";
                //        Console.WriteLine(c_hex);
                //        Console.WriteLine(X2_text);
                //        c_hex = "";
                //        X2_text = "";
                //        hexcount = 0;
                //    }

                //    //_text += i + " > " + c + " > " + buffer[i].ToString("X2") + ", dec: " + buffer[i] + newline;
                //    //Console.WriteLine("HEX-Reading-BUFFER: " + _text + ", out of buffer");
                //    //count += 1;
                //    //if (hexcount == 15)
                //    //    Console.WriteLine("HEX-Reading-BUFFER: " + _text + ", out of buffer");
                //}
                //Console.WriteLine("HEX-Reading-BUFFER: " + _text + ", out of buffer");

                //for (int i = 0; i < 16; i++)
                //{
                //    hextext += c;
                //}


                //_text = "";

                //for (int i = 35; i < 48; i++)
                //{
                //    char c = (char)buffer[i];
                //    _text += c;
                //    Console.WriteLine(i + " > " + c + " > " + buffer[i].ToString("X2") + ", dec: " + buffer[i] + newline);
                //}
                //Console.WriteLine("HEX-Reading: " + _text + ", out of savedgame");

                return sin.ReadObject() as T;
            }
        }

        public static byte[] Write(object value)
        {
            using (SerializationWriter sout = new SerializationWriter())
            {
                sout.OptimizeForSize = true;
                sout.WriteObject(value);
                _ = sout.AppendTokenTables();
                sout.Flush();
                byte[] results = MiniLZO.Compress((MemoryStream)sout.BaseStream);
                return results;
            }
        }
    }
}