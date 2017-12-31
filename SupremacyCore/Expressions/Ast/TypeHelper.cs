using System;
using System.Collections.Generic;

namespace Supremacy.Expressions.Ast
{
    internal static class TypeHelper
    {
        private static readonly Dictionary<BuiltinType, Type> BuiltinTypeToClrTypeMap;

        static TypeHelper()
        {
            BuiltinTypeToClrTypeMap = new Dictionary<BuiltinType, Type>
                                      {
                                          { BuiltinType.Boolean, typeof(bool) },
                                          { BuiltinType.Byte, typeof(byte) },
                                          { BuiltinType.Decimal, typeof(decimal) },
                                          { BuiltinType.Double, typeof(double) },
                                          { BuiltinType.Int16, typeof(short) },
                                          { BuiltinType.Int32, typeof(int) },
                                          { BuiltinType.Int64, typeof(long) },
                                          { BuiltinType.Null, typeof(object) },
                                          { BuiltinType.SByte, typeof(sbyte) },
                                          { BuiltinType.Single, typeof(float) },
                                          { BuiltinType.UInt16, typeof(ushort) },
                                          { BuiltinType.UInt32, typeof(uint) },
                                          { BuiltinType.UInt64, typeof(ulong) },
                                      };
        }

        public static bool TryParseLiteral(string text, TypeName literalType, out object value, out Type clrType, out BuiltinType? builtinType)
        {
            if (!literalType.IsBuiltinType)
                throw new ArgumentException("Literal type must be a built-in type.", "literalType");

            builtinType = (BuiltinType)Enum.Parse(typeof(BuiltinType), text);
            clrType = BuiltinTypeToClrTypeMap[builtinType.Value];

            switch (builtinType)
            {
                case BuiltinType.SByte:
                    sbyte parsedSByte;
                    value = sbyte.TryParse(text, out parsedSByte);
                    break;

                case BuiltinType.Byte:
                    byte parsedByte;
                    value = byte.TryParse(text, out parsedByte);
                    break;

                case BuiltinType.Int16:
                    short parsedInt16;
                    value = short.TryParse(text, out parsedInt16);
                    break;

                case BuiltinType.UInt16:
                    ushort parsedUInt16;
                    value = ushort.TryParse(text, out parsedUInt16);
                    break;

                case BuiltinType.Int32:
                    int parsedInt32;
                    value = int.TryParse(text, out parsedInt32);
                    break;

                case BuiltinType.UInt32:
                    uint parsedUInt32;
                    value = uint.TryParse(text, out parsedUInt32);
                    break;

                case BuiltinType.Int64:
                    long parsedInt64;
                    value = long.TryParse(text, out parsedInt64);
                    break;

                case BuiltinType.UInt64:
                    ulong parsedUInt64;
                    value = ulong.TryParse(text, out parsedUInt64);
                    break;

                case BuiltinType.Single:
                    float parsedSingle;
                    value = float.TryParse(text, out parsedSingle);
                    break;

                case BuiltinType.Double:
                    double parsedDouble;
                    value = double.TryParse(text, out parsedDouble);
                    break;

                case BuiltinType.Decimal:
                    decimal parsedDecimal;
                    value = decimal.TryParse(text, out parsedDecimal);
                    break;

                case BuiltinType.Boolean:
                    bool parsedBoolean;
                    value = bool.TryParse(text, out parsedBoolean);
                    break;

                case BuiltinType.Null:
                    value = null;
                    break;
                    
                default:
                    value = null;
                    return false;
            }

            return true;
        }
    }
}