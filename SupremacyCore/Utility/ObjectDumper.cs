using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Supremacy.Utility
{
    public static class ObjectDumper
    {
        public static string DumpObject(object value)
        {
            using (var sw = new StringWriter())
            using (var writer = new IndentedTextWriter(sw))
            {
                DumpObject(value, writer);
                return sw.ToString();
            }
        }

        public static void DumpObject(object value, IndentedTextWriter writer, bool topLevel = true)
        {
            if (value == null)
            {
                writer.Write("null");
                return;
            }

            var valueType = value.GetType();
            var typeCode = Type.GetTypeCode(valueType);

            if (typeCode == TypeCode.Char)
            {
                writer.Write("'{0}'", value);
                return;
            }

            if (typeCode == TypeCode.String)
            {
                writer.Write("\"{0}\"", value);
                return;
            }

            if (typeCode != TypeCode.Object)
            {
                writer.Write("{0}", value);
                return;
            }

            if (!topLevel)
            {
                writer.Write("{{{0}}}", value);
                return;
            }

            var array = value as Array;
            if (array != null)
            {
                writer.Write(
                    "{{{0}[{1}]}}",
                    valueType.GetElementType().FullName,
                    string.Join(",", Enumerable.Range(0, array.Rank).Select(array.GetLength)));

                ++writer.Indent;

                var offsets = new int[array.Rank];
                var lengths = new int[array.Rank];

                for (var i = 0; i < array.Rank; i++)
                {
                    offsets[i] = 0;
                    lengths[i] = array.GetLength(i);
                }

                foreach (var item in array)
                {
                    writer.WriteLine();

                    if (offsets[offsets.Length - 1] >= lengths[offsets.Length - 1])
                    {
                        for (var i = lengths.Length - 1; i >= 0; i--)
                        {
                            if (offsets[i] + 1 < lengths[i])
                            {
                                ++offsets[i];
                                break;
                            }
                            offsets[i] = 0;
                        }
                    }

                    writer.Write("[{0}]: ", string.Join(", ", offsets));
                    DumpObject(item, writer, false);

                    ++offsets[offsets.Length - 1];
                }
            }
            else
            {
                writer.Write("{{{0}}}", valueType.FullName);
                ++writer.Indent;

                foreach (var property in valueType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var indexParameters = property.GetIndexParameters();
                    if (indexParameters.Length != 0)
                        continue;

                    writer.WriteLine();
                    writer.Write("{0}: ", property.Name);

                    DumpObject(property.GetValue(value, null), writer, false);
                }
            }

            --writer.Indent;
        }

    }
}