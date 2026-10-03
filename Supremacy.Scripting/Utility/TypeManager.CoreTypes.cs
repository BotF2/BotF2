using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Utility
{
    public static partial class TypeManager
    {
        public static bool IsPrimitiveType(Type t)
        {
            return t == CoreTypes.Int32 || t == CoreTypes.UInt32 ||
                    t == CoreTypes.Int64 || t == CoreTypes.UInt64 ||
                    t == CoreTypes.Single || t == CoreTypes.Double ||
                    t == CoreTypes.Char || t == CoreTypes.Int16 ||
                    t == CoreTypes.Boolean || t == CoreTypes.SByte ||
                    t == CoreTypes.Byte || t == CoreTypes.UInt16;
        }

        public static class CoreTypes
        {
            private static readonly dynamic _dynamic = 0; // It used by GetField("_dynamic", ...),
            // if you removed this line be leave GetField() call, the game will crash mid-game

            //_dynamic = null;
            //_text = _dynamic.ToString();

            public static readonly Type Dynamic = typeof(CoreTypes).GetField("_dynamic", BindingFlags.NonPublic | BindingFlags.Static).FieldType;
            public static readonly Type Null = typeof(DynamicNull);
            public static readonly Type Object = typeof(object);

            public static readonly Type String = typeof(string);
            public static readonly Type Char = typeof(char);

            public static readonly Type SByte = typeof(sbyte);
            public static readonly Type Byte = typeof(byte);
            public static readonly Type Int16 = typeof(short);
            public static readonly Type UInt16 = typeof(ushort);
            public static readonly Type Int32 = typeof(int);
            public static readonly Type UInt32 = typeof(uint);
            public static readonly Type Int64 = typeof(long);
            public static readonly Type UInt64 = typeof(ulong);

            public static readonly Type Void = typeof(void);

            public static readonly Type Boolean = typeof(bool);

            public static readonly Type Single = typeof(float);
            public static readonly Type Double = typeof(double);
            public static readonly Type Decimal = typeof(decimal);

            public static readonly Type Enum = typeof(Enum);

            public static readonly Type Array = typeof(Array);

            public static readonly Type ValueType = typeof(ValueType);

            public static readonly Type Delegate = typeof(Delegate);
            public static readonly Type MulticastDelegate = typeof(MulticastDelegate);

            public static readonly Type MarshalByRefObject = typeof(MarshalByRefObject);

            public static readonly Type GenericNullable = typeof(Nullable<>);

            public static readonly Type EnumerableInterface = typeof(IEnumerable);
            public static readonly Type GenericEnumerableInterface = typeof(IEnumerable<>);
            public static readonly Type EnumeratorInterface = typeof(IEnumerator);
            public static readonly Type GenericEnumeratorInterface = typeof(IEnumerator<>);
            public static readonly Type CollectionInterface = typeof(ICollection);
            public static readonly Type GenericCollectionInterface = typeof(ICollection<>);
            public static readonly Type ListInterface = typeof(IList);
            public static readonly Type GenericListInterface = typeof(IList<>);

            public static readonly Type Expression = typeof(System.Linq.Expressions.Expression);
            public static readonly Type GenericExpression = typeof(System.Linq.Expressions.Expression<>);

            public static dynamic Dynamic1 => _dynamic;
        }
    }
}