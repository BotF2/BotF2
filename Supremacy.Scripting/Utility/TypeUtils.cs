using System;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using Supremacy.Scripting.Ast;

using Expression = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Utility
{
    internal static class TypeUtils
    {
        internal static bool IsStatic(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.IsSealed && type.IsAbstract;
        }
        
        internal static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }


        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }


        internal static bool IsBoolean(this Type type)
        {
            return GetNonNullableType(type) == typeof(bool);
        }


        internal static bool IsNumeric(this Type type)
        {
            type = GetNonNullableType(type);
            return !type.IsEnum ? IsNumeric(Type.GetTypeCode(type)) : false;
        }

        internal static bool IsNumeric(this TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }


        internal static bool IsArithmetic(this Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }


        internal static bool IsUnsignedInt(this Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsIntegralType(this Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int64:
                    case TypeCode.Int32:
                    case TypeCode.Int16:
                    case TypeCode.UInt64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt16:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        return true;
                }
            }
            return false;
        }


        internal static bool IsIntegerOrBoolean(this Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int64:
                    case TypeCode.Int32:
                    case TypeCode.Int16:
                    case TypeCode.UInt64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt16:
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        return true;
                }
            }
            return false;
        }

        internal static bool CanAssign(Type to, Expression from)
        {
            if (CanAssign(to, from.Type))
            {
                return true;
            }

            return to.IsValueType &&
                to.IsGenericType &&
                to.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                ConstantCheck.Check(from, null);
        }

        internal static bool CanAssign(Type to, Type from)
        {
            if (to == from)
            {
                return true;
            }
            // Reference types
            if (!to.IsValueType && !from.IsValueType)
            {
                if (to.IsAssignableFrom(from))
                {
                    return true;
                }
                // Arrays can be assigned if they have same rank and assignable element types.
                if (to.IsArray && from.IsArray &&
                    to.GetArrayRank() == from.GetArrayRank() &&
                    CanAssign(to.GetElementType(), from.GetElementType()))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsGeneric(this Type type)
        {
            return type.ContainsGenericParameters || type.IsGenericTypeDefinition;
        }

        internal static bool CanCompareToNull(Type type)
        {
            // This is a bit too conservative.
            return !type.IsValueType;
        }

        /// <summary>
        /// Returns a numerical code of the size of a type.  All types get both a horizontal
        /// and vertical code.  Types that are lower in both dimensions have implicit conversions
        /// to types that are higher in both dimensions.
        /// </summary>
        internal static bool GetNumericConversionOrder(this TypeCode code, out int x, out int y)
        {
            // implicit conversions:
            //     0     1     2     3     4
            // 0:       U1 -> U2 -> U4 -> U8
            //          |     |     |
            //          v     v     v
            // 1: I1 -> I2 -> I4 -> I8
            //          |     |     
            //          v     v     
            // 2:       R4 -> R8

            switch (code)
            {
                case TypeCode.Byte: x = 0; y = 0; break;
                case TypeCode.UInt16: x = 1; y = 0; break;
                case TypeCode.UInt32: x = 2; y = 0; break;
                case TypeCode.UInt64: x = 3; y = 0; break;

                case TypeCode.SByte: x = 0; y = 1; break;
                case TypeCode.Int16: x = 1; y = 1; break;
                case TypeCode.Int32: x = 2; y = 1; break;
                case TypeCode.Int64: x = 3; y = 1; break;

                case TypeCode.Single: x = 1; y = 2; break;
                case TypeCode.Double: x = 2; y = 2; break;

                default:
                    x = y = 0;
                    return false;
            }
            return true;
        }

        internal static bool IsImplicitlyConvertible(int fromX, int fromY, int toX, int toY)
        {
            return fromX <= toX && fromY <= toY;
        }

        internal static bool HasBuiltinEquality(Type left, Type right)
        {
            // Reference type can be compared to interfaces
            if (left.IsInterface && !right.IsValueType ||
                right.IsInterface && !left.IsValueType)
            {
                return true;
            }

            // Reference types compare if they are assignable
            if (!left.IsValueType && !right.IsValueType)
            {
                if (CanAssign(left, right) || CanAssign(right, left))
                {
                    return true;
                }
            }

            // Nullable<T> vs null
            if (NullVsNullable(left, right) || NullVsNullable(right, left))
            {
                return true;
            }

            if (left != right)
            {
                return false;
            }

            if (left == typeof(bool) || IsNumeric(left) || left.IsEnum)
            {
                return true;
            }

            return false;
        }

        private static bool NullVsNullable(Type left, Type right)
        {
            return IsNullableType(left) && right == typeof(DynamicNull);
        }

        
        internal static bool AreEquivalent(Type t1, Type t2)
        {
#if CLR2 || SILVERLIGHT // type equivalence not implemented on Silverlight
            return t1 == t2;
#else
            return t1 == t2 || t1.IsEquivalentTo(t2);
#endif
        }


        internal static Type GetConstantType(this Type type)
        {
            // If it's a visible type, we're done
            if (type.IsVisible)
            {
                return type;
            }

            // Get the visible base type
            Type baseType = type;
            do
            {
                baseType = baseType.BaseType;
            } while (!baseType.IsVisible);

            // If it's one of the known reflection types,
            // return the known type.
            if (baseType == typeof(Type) ||
                baseType == typeof(ConstructorInfo) ||
                baseType == typeof(EventInfo) ||
                baseType == typeof(FieldInfo) ||
                baseType == typeof(MethodInfo) ||
                baseType == typeof(PropertyInfo))
            {
                return baseType;
            }

            // else return the original type
            return type;
        }

        internal static bool IsConvertible(this Type type)
        {
            type = GetNonNullableType(type);
            
            if (type.IsEnum)
            {
                return true;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }

        internal static Type GetNonNoneType(this Type type)
        {
            return (type == typeof(DynamicNull)) ? typeof(object) : type;
        }

        internal static bool IsFloatingPoint(this Type type)
        {
            type = GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool AreReferenceAssignable(Type dest, Type src)
        {
            if (dest == src)
            {
                return true;
            }

            return !dest.IsValueType && !src.IsValueType && AreAssignable(dest, src);
        }

        internal static bool AreAssignable(Type dest, Type src)
        {
            if (dest == src)
            {
                return true;
            }

            if (dest.IsAssignableFrom(src))
            {
                return true;
            }

            if (dest.IsArray && src.IsArray && dest.GetArrayRank() == src.GetArrayRank() &&
                AreReferenceAssignable(dest.GetElementType(), src.GetElementType()))
            {
                return true;
            }

            return src.IsArray && dest.IsGenericType &&
                (dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.ICollection<>))
                && (/* TODO: Fix this ghetto-ass shit. */ dest.GetGenericArguments()[0].IsGenericParameter || (dest.GetGenericArguments()[0] == src.GetElementType()));
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination)
        {
            if (destination.IsGenericParameter)
            {
                if (ConstraintChecker.CheckConstraints(
                    null,
                    destination.DeclaringType,
                    new[] { destination },
                    new[] { source },
                    SourceSpan.None,
                    true))
                {
                    return true;
                }
            }

            if (source.IsGenericType &&
                destination.IsGenericType &&
                !source.IsGenericTypeDefinition)
            {
                if (destination.IsGenericTypeDefinition)
                {
                    return source.GetGenericTypeDefinition() == destination;
                }

                Type[] sArgs = source.GetGenericArguments();
                Type[] dArgs = destination.GetGenericArguments();

                if (sArgs.Length == dArgs.Length)
                {
                    bool success = !sArgs
                        .Where((t, i) => !dArgs[i].IsGenericParameter && !AreAssignable(t, dArgs[i]))
                        .Any();

                    if (success)
                    {
                        return true;
                    }
                }
            }

            return IsIdentityConversion(source, destination) ||
                   IsImplicitNumericConversion(source, destination) ||
                   IsImplicitReferenceConversion(source, destination) ||
                   IsImplicitBoxingConversion(source, destination);
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination, bool considerUserDefined)
        {
            return IsImplicitlyConvertible(source, destination) ||
                (considerUserDefined && GetUserDefinedCoercionMethod(source, destination, true) != null);
        }

        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly)
        {
            // check for implicit coercions first
            Type nnExprType = GetNonNullableType(convertFrom);
            Type nnConvType = GetNonNullableType(convertToType);

            // try exact match on types
            MethodInfo[] eMethods = nnExprType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo method = FindConversionOperator(eMethods, convertFrom, convertToType, implicitOnly);

            if (method != null)
            {
                return method;
            }

            MethodInfo[] cMethods = nnConvType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            method = FindConversionOperator(cMethods, convertFrom, convertToType, implicitOnly);

            if (method != null)
            {
                return method;
            }

            // try lifted conversion
            if (nnExprType != convertFrom || nnConvType != convertToType)
            {
                method = FindConversionOperator(eMethods, nnExprType, nnConvType, implicitOnly) ??
                         FindConversionOperator(cMethods, nnExprType, nnConvType, implicitOnly);
                
                if (method != null)
                {
                    return method;
                }
            }

            return null;
        }

        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly)
        {
            return (from mi in methods
                    where mi.Name == "op_Implicit" || (!implicitOnly && mi.Name == "op_Explicit")
                    where mi.ReturnType == typeTo
                    let pis = mi.GetParameters()
                    where pis[0].ParameterType == typeFrom
                    select mi).FirstOrDefault();
        }

        internal static bool IsIdentityConversion(Type source, Type destination)
        {
            return source == destination;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode tcSource = Type.GetTypeCode(source);
            TypeCode tcDest = Type.GetTypeCode(destination);

            switch (tcSource)
            {
                case TypeCode.SByte:
                    switch (tcDest)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch (tcDest)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int16:
                    switch (tcDest)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (tcDest)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int32:
                    switch (tcDest)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (tcDest)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (tcDest)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Char:
                    switch (tcDest)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Single:
                    return tcDest == TypeCode.Double;
            }
            return false;
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination)
        {
            return AreAssignable(destination, source);
        }

        private static bool IsImplicitBoxingConversion(Type source, Type destination)
        {
            if (source.IsValueType() &&
                (destination == TypeManager.CoreTypes.Object || destination == TypeManager.CoreTypes.ValueType))
            {
                return true;
            }

            return source.IsEnumType() && destination == typeof(Enum);
        }
    }
}