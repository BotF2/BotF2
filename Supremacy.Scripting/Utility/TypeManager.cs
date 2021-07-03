using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using Supremacy.Scripting.Ast;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Utility
{
    public static partial class TypeManager
    {
        public static readonly Assembly ThisAssembly;
        public static readonly TypeExpression ExpressionTypeExpression;

        private static readonly Dictionary<Assembly, bool> _assemblyInternalsVisibleAttributes;
        private static readonly Dictionary<MemberInfo, ParametersCollection> _methodParameters;
        private static readonly Dictionary<Type, Type[]> _interfaceCache;

        static TypeManager()
        {
            ThisAssembly = typeof(TypeManager).Assembly;

            BuiltinTypeToClrTypeMap = new Dictionary<BuiltinType, Type>
                                      {
                                          { BuiltinType.Boolean, CoreTypes.Boolean },
                                          { BuiltinType.Byte, CoreTypes.Byte },
                                          { BuiltinType.Decimal, CoreTypes.Decimal },
                                          { BuiltinType.Double, CoreTypes.Double },
                                          { BuiltinType.Int16, CoreTypes.Int16 },
                                          { BuiltinType.Int32, CoreTypes.Int32 },
                                          { BuiltinType.Int64, CoreTypes.Int64 },
                                          { BuiltinType.Null, CoreTypes.Null },
                                          { BuiltinType.SByte, CoreTypes.SByte },
                                          { BuiltinType.Single, CoreTypes.Single },
                                          { BuiltinType.UInt16, CoreTypes.UInt16 },
                                          { BuiltinType.UInt32, CoreTypes.UInt32 },
                                          { BuiltinType.UInt64, CoreTypes.UInt64 },
                                          { BuiltinType.Char, CoreTypes.Char },
                                          { BuiltinType.Object, CoreTypes.Object }
                                      };

            ExpressionTypeExpression = new TypeExpression(CoreTypes.Expression, SourceSpan.None);

            ClrTypeToBuiltinMap = BuiltinTypeToClrTypeMap.ToDictionary(o => o.Value, o => o.Key);

            _moduleBuilder = AppDomain.CurrentDomain
                .DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave)
                .DefineDynamicModule(_assemblyName.Name, true);

            _assemblyInternalsVisibleAttributes = new Dictionary<Assembly, bool>();
            _methodParameters = new Dictionary<MemberInfo, ParametersCollection>();
            _interfaceCache = new Dictionary<Type, Type[]>();
        }

        private static MethodInfo _activatorCreateInstance;
        public static MethodInfo ActivatorCreateInstance
        {
            get
            {
                if (_activatorCreateInstance == null)
                {
                    _activatorCreateInstance = typeof(Activator).GetMethod("CreateInstance", Type.EmptyTypes);
                }

                return _activatorCreateInstance;
            }
            set => _activatorCreateInstance = value;
        }

        /// <summary>
        ///   Returns the C# name of a type if possible, or the full type name otherwise
        /// </summary>
        public static string GetCSharpName(Type t)
        {
            if ((t == null) || (t == typeof(DynamicNull)))
            {
                return "null";
            }

            //if (t == typeof(ArglistAccess))
            //    return "__arglist";

            //if (t == typeof(AnonymousMethodBody))
            //    return "anonymous method";

            //if (t == typeof(MethodGroupExpr))
            //    return "method group";

            if ((PredefinedAttributes.Dynamic != null) && t.GetCustomAttributes(PredefinedAttributes.Dynamic, true).Any())
            {
                return "dynamic";
            }

            if (t.IsGenericType && !t.IsGenericTypeDefinition)
            {
                StringBuilder sb = new StringBuilder();
                _ = ReflectionUtils.FormatTypeName(sb, t, o => GetCSharpName(o.Name, o));
                return sb.ToString();
            }

            return GetCSharpName(t.Name, t);
        }

        public static string GetCSharpName(string name, Type type)
        {
            if (type.FullName.Length > 3)
            {
                if (type == CoreTypes.Int32)
                {
                    return "int";
                }

                if (type == CoreTypes.Int64)
                {
                    return "long";
                }

                if (type == CoreTypes.String)
                {
                    return "string";
                }

                if (type == CoreTypes.Boolean)
                {
                    return "bool";
                }

                if (type == CoreTypes.Void)
                {
                    return "void";
                }

                if (type == CoreTypes.Object)
                {
                    return "object";
                }

                if (type == CoreTypes.UInt32)
                {
                    return "uint";
                }

                if (type == CoreTypes.Int16)
                {
                    return "short";
                }

                if (type == CoreTypes.UInt16)
                {
                    return "ushort";
                }

                if (type == CoreTypes.UInt64)
                {
                    return "ulong";
                }

                if (type == CoreTypes.Single)
                {
                    return "float";
                }

                if (type == CoreTypes.Double)
                {
                    return "double";
                }

                if (type == CoreTypes.Decimal)
                {
                    return "decimal";
                }

                if (type == CoreTypes.Char)
                {
                    return "char";
                }

                if (type == CoreTypes.Byte)
                {
                    return "byte";
                }

                if (type == CoreTypes.SByte)
                {
                    return "sbyte";
                }
            }

            return name.Replace('+', '.');
        }

        public static string GetCSharpName(Type[] types)
        {
            if (types.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < types.Length; ++i)
            {
                if (i > 0)
                {
                    _ = sb.Append(", ");
                }

                _ = sb.Append(GetCSharpName(types[i]));
            }

            return sb.ToString();
        }

        public const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.Static | BindingFlags.Instance |
                                               BindingFlags.DeclaredOnly;

        public static PropertyInfo GetPropertyFromAccessor(MethodBase mb)
        {
            if (!mb.IsSpecialName)
            {
                return null;
            }

            string name = mb.Name;
            if (name.Length < 5)
            {
                return null;
            }

            if (name[3] != '_')
            {
                return null;
            }

            if (name.StartsWith("get") || name.StartsWith("set"))
            {
                MemberInfo[] pi = mb.DeclaringType.FindMembers(
                    MemberTypes.Property,
                    AllMembers,
                    Type.FilterName,
                    name.Substring(4));

                // This can happen when property is indexer (it can have same name but different parameters)
                return (from PropertyInfo p in pi
                        from accessor in p.GetAccessors(true)
                        where accessor == mb || GetParameterData(accessor).Equals(GetParameterData(mb))
                        select p).FirstOrDefault();
            }

            return null;
        }

        private static bool IsValidProperty(PropertyInfo pi)
        {
            MethodInfo getMethod = pi.GetGetMethod(true);
            MethodInfo setMethod = pi.GetSetMethod(true);

            int gCount = 0;
            int sCount = 0;

            if (getMethod != null && setMethod != null)
            {
                gCount = getMethod.GetParameters().Length;
                sCount = setMethod.GetParameters().Length;
                if (gCount + 1 != sCount)
                {
                    return false;
                }
            }
            else if (getMethod != null)
            {
                gCount = getMethod.GetParameters().Length;
            }
            else if (setMethod != null)
            {
                sCount = setMethod.GetParameters().Length;
            }

            //
            // DefaultMemberName and indexer name has to match to identify valid C# indexer
            //
            if ((sCount > 1) || (gCount > 0))
            {
                DefaultMemberAttribute defaultMemberAttribute = pi.DeclaringType
                    .GetCustomAttributes(PredefinedAttributes.DefaultMember, false)
                    .Cast<DefaultMemberAttribute>()
                    .FirstOrDefault();

                if (defaultMemberAttribute == null)
                {
                    return false;
                }

                if (defaultMemberAttribute.MemberName != pi.Name)
                {
                    return false;
                }

                if (getMethod != null && "get_" + defaultMemberAttribute.MemberName != getMethod.Name)
                {
                    return false;
                }

                if (setMethod != null && "set_" + defaultMemberAttribute.MemberName != setMethod.Name)
                {
                    return false;
                }
            }

            return true;
        }

        public static MemberInfo GetEventFromAccessor(MethodBase mb)
        {
            if (!mb.IsSpecialName)
            {
                return null;
            }

            string name = mb.Name;
            if (name.Length < 5)
            {
                return null;
            }

            if (name.StartsWith("add_"))
            {
                return mb.DeclaringType.GetEvent(name.Substring(4), AllMembers);
            }

            return name.StartsWith("remove_") ? mb.DeclaringType.GetEvent(name.Substring(7), AllMembers) : null;
        }

        public static bool IsSpecialMethod(MethodBase mb)
        {
            if (!mb.IsSpecialName)
            {
                return false;
            }

            PropertyInfo pi = GetPropertyFromAccessor(mb);
            if (pi != null)
            {
                return IsValidProperty(pi);
            }

            if (GetEventFromAccessor(mb) != null)
            {
                return true;
            }

            string name = mb.Name;
            return name.StartsWith("op_") ? OperatorInfo.GetOperatorInfo(name) != null : false;
        }

        public static string GetCSharpSignature(MethodBase mb)
        {
            return GetCSharpSignature(mb, false);
        }

        public static string GetCSharpSignature(MethodBase mb, bool showAccessor)
        {
            StringBuilder sig = new StringBuilder(GetCSharpName(mb.DeclaringType));
            _ = sig.Append('.');

            int accessorEnd = 0;
            ParametersCollection parameters = GetParameterData(mb);
            string parametersSignature = parameters.GetSignatureForError();

            if (!mb.IsConstructor && IsSpecialMethod(mb))
            {
                string signatureName = OperatorInfo.GetSignatureName(mb.Name);
                if (signatureName != null)
                {
                    if (signatureName == "explicit" || signatureName == "implicit")
                    {
                        _ = sig.Append(signatureName);
                        _ = sig.Append(" operator ");
                        _ = sig.Append(GetCSharpName(((MethodInfo)mb).ReturnType));
                    }
                    else
                    {
                        _ = sig.Append("operator ");
                        _ = sig.Append(signatureName);
                    }
                    _ = sig.Append(parametersSignature);
                    return sig.ToString();
                }

                bool isGetter = mb.Name.StartsWith("get_");
                bool isSetter = mb.Name.StartsWith("set_");

                if (isGetter || isSetter || mb.Name.StartsWith("add_"))
                {
                    accessorEnd = 3;
                }
                else if (mb.Name.StartsWith("remove_"))
                {
                    accessorEnd = 6;
                }

                // Is indexer
                if (parameters.Count > (isGetter ? 0 : 1))
                {
                    _ = sig.Append("this[");

                    _ = sig.Append(
                        isGetter
                            ? parametersSignature.Substring(1, parametersSignature.Length - 2)
                            : parametersSignature.Substring(1, parametersSignature.LastIndexOf(',') - 1));

                    _ = sig.Append(']');
                }
                else
                {
                    _ = sig.Append(mb.Name.Substring(accessorEnd + 1));
                }
            }
            else
            {
                if (mb.Name == ".ctor")
                {
                    _ = sig.Append(ReflectionUtils.GetNormalizedTypeName(mb.DeclaringType));
                }
                else
                {
                    _ = sig.Append(mb.Name);

                    if (mb.IsGenericMethod)
                    {
                        Type[] args = mb.GetGenericArguments();
                        _ = sig.Append('<');
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (i > 0)
                            {
                                _ = sig.Append(',');
                            }

                            _ = sig.Append(GetCSharpName(args[i]));
                        }
                        _ = sig.Append('>');
                    }
                }

                _ = sig.Append(parametersSignature);
            }

            if (showAccessor && accessorEnd > 0)
            {
                _ = sig.Append('.');
                _ = sig.Append(mb.Name.Substring(0, accessorEnd));
            }

            return sig.ToString();
        }

        public static ParametersCollection GetParameterData(PropertyInfo pi)
        {
            if (_methodParameters.TryGetValue(pi, out ParametersCollection pd))
            {
                return pd;
            }

            if (pi is PropertyBuilder)
            {
                return ParametersCompiled.EmptyReadOnlyParameters;
            }

            ParameterInfo[] p = pi.GetIndexParameters();

            pd = ParametersImported.Create(p, null);
            _methodParameters[pi] = pd;

            return pd;
        }

        public static ParametersCollection GetParameterData(MethodBase mb)
        {
            ParametersCollection pd;

            if (mb.IsGenericMethod && !mb.IsGenericMethodDefinition)
            {
                MethodInfo mi = ((MethodInfo)mb).GetGenericMethodDefinition();
                pd = GetParameterData(mi);
                pd = mi.IsGenericMethod
                    ? pd.InflateTypes(mi.GetGenericArguments(), mb.GetGenericArguments())
                    : pd.InflateTypes(mi.DeclaringType.GetGenericArguments(), mb.GetGenericArguments());
                return pd;
            }

            pd = ParametersImported.Create(mb);
            return pd;
        }

        private static readonly Dictionary<BuiltinType, Type> BuiltinTypeToClrTypeMap;
        private static readonly Dictionary<Type, BuiltinType> ClrTypeToBuiltinMap;

        public static Type GetClrTypeFromBuiltinType(BuiltinType builtinType)
        {
            return !BuiltinTypeToClrTypeMap.TryGetValue(builtinType, out Type clrType) ? null : clrType;
        }

        public static BuiltinType? GetBuiltinTypeFromClrType(Type type)
        {
            return !ClrTypeToBuiltinMap.TryGetValue(type, out BuiltinType builtinType) ? null : (BuiltinType?)builtinType;
        }

        public static bool TryParseLiteral(BuiltinType literalType, string text, out object value, out Type clrType)
        {
            if (!BuiltinTypeToClrTypeMap.TryGetValue(literalType, out clrType))
            {
                value = null;
                return false;
            }

            switch (literalType)
            {
                case BuiltinType.SByte:
                    sbyte parsedSByte;
                    _ = sbyte.TryParse(text, out parsedSByte);
                    value = parsedSByte;
                    break;

                case BuiltinType.Byte:
                    byte parsedByte;
                    _ = byte.TryParse(text, out parsedByte);
                    value = parsedByte;
                    break;

                case BuiltinType.Int16:
                    short parsedInt16;
                    _ = short.TryParse(text, out parsedInt16);
                    value = parsedInt16;
                    break;

                case BuiltinType.UInt16:
                    ushort parsedUInt16;
                    _ = ushort.TryParse(text, out parsedUInt16);
                    value = parsedUInt16;
                    break;

                case BuiltinType.Int32:
                    int parsedInt32;
                    _ = int.TryParse(text, out parsedInt32);
                    value = parsedInt32;
                    break;

                case BuiltinType.UInt32:
                    uint parsedUInt32;
                    _ = uint.TryParse(text, out parsedUInt32);
                    value = parsedUInt32;
                    break;

                case BuiltinType.Int64:
                    long parsedInt64;
                    _ = long.TryParse(text, out parsedInt64);
                    value = parsedInt64;
                    break;

                case BuiltinType.UInt64:
                    ulong parsedUInt64;
                    _ = ulong.TryParse(text, out parsedUInt64);
                    value = parsedUInt64;
                    break;

                case BuiltinType.Single:
                    float parsedSingle;
                    _ = float.TryParse(text, out parsedSingle);
                    value = parsedSingle;
                    break;

                case BuiltinType.Double:
                    double parsedDouble;
                    _ = double.TryParse(text, out parsedDouble);
                    value = parsedDouble;
                    break;

                case BuiltinType.Decimal:
                    decimal parsedDecimal;
                    _ = decimal.TryParse(text, out parsedDecimal);
                    value = parsedDecimal;
                    break;

                case BuiltinType.Boolean:
                    bool parsedBoolean;
                    _ = bool.TryParse(text, out parsedBoolean);
                    value = parsedBoolean;
                    break;

                case BuiltinType.Char:
                    bool parsedChar = text.Length == 3 && text[0] == '\'' && text[2] == '\'';
                    value = parsedChar ? text[1] : (object)null;

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

        public static Type DropGenericTypeArguments(Type t)
        {
            if (!t.IsGenericType || t.IsGenericTypeDefinition)
            {
                return t;
            }
            // Micro-optimization: a generic typebuilder is always a generic type definition
            return t is TypeBuilder ? t : t.GetGenericTypeDefinition();
        }

        public static bool HasElementType(Type t)
        {
            return t.IsArray || t.IsPointer || t.IsByRef;
        }

        //
        // Checks whether `invocationAssembly' is same or a friend of the assembly
        //
        public static bool IsThisOrFriendAssembly(Assembly invocationAssembly, [Annotations.NotNull] Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (invocationAssembly == null)
            {
                invocationAssembly = Assembly.GetExecutingAssembly();
            }

            if (invocationAssembly == assembly)
            {
                return true;
            }

            if (_assemblyInternalsVisibleAttributes.TryGetValue(assembly, out bool visible))
            {
                return visible;
            }

            IEnumerable<InternalsVisibleToAttribute> internalsVisibleAttributes = assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false).Cast<InternalsVisibleToAttribute>();
            if (!internalsVisibleAttributes.Any())
            {
                _assemblyInternalsVisibleAttributes.Add(assembly, false);
                return false;
            }

            bool isFriend = false;

            AssemblyName invocationAssemblyName = invocationAssembly.GetName();
            byte[] invocationAssemblyToken = invocationAssemblyName.GetPublicKeyToken();

            foreach (InternalsVisibleToAttribute attr in internalsVisibleAttributes)
            {
                if (string.IsNullOrEmpty(attr.AssemblyName))
                {
                    continue;
                }

                AssemblyName assemblyName = null;

                try { assemblyName = new AssemblyName(attr.AssemblyName); }
                catch (FileLoadException) { }
                catch (ArgumentException) { }

                if (assemblyName == null || (assemblyName.Name != invocationAssemblyName.Name))
                {
                    continue;
                }

                byte[] keyToken = assemblyName.GetPublicKeyToken();
                if (keyToken != null)
                {
                    if (invocationAssemblyToken.Length == 0)
                    {
                        throw new CompilerErrorException(
                            CompilerErrors.FriendAssemblyNameNotMatching,
                            assemblyName.FullName,
                            invocationAssembly.FullName);
                    }

                    if (!keyToken.SequenceEqual(invocationAssemblyToken))
                    {
                        continue;
                    }
                }

                isFriend = true;
                break;
            }

            _assemblyInternalsVisibleAttributes.Add(assembly, isFriend);
            return isFriend;
        }

        public static bool CheckAccessLevel(ParseContext parseContext, Type checkType)
        {
            checkType = DropGenericTypeArguments(checkType);

            /*
             * Broken Microsoft runtime, return public for arrays, no matter what 
             * the accessibility is for their underlying class, and they return 
             * NonPublic visibility for pointers.
             */
            if (HasElementType(checkType))
            {
                return CheckAccessLevel(parseContext, checkType.GetElementType());
            }

            if (checkType.IsGenericParameter)
            {
                return true;
            }

            TypeAttributes checkAttributes = checkType.Attributes & TypeAttributes.VisibilityMask;

            try
            {
                switch (checkAttributes)
                {
                    case TypeAttributes.Public:
                        return true;

                    case TypeAttributes.NotPublic:
                        return IsThisOrFriendAssembly(ThisAssembly, checkType.Assembly);

                    case TypeAttributes.NestedPublic:
                        return CheckAccessLevel(parseContext, checkType.DeclaringType);

                    case TypeAttributes.NestedPrivate:
                    case TypeAttributes.NestedFamily:
                    case TypeAttributes.NestedFamANDAssem:
                        return false;

                    case TypeAttributes.NestedFamORAssem:
                        return IsThisOrFriendAssembly(ThisAssembly, checkType.Assembly);

                    case TypeAttributes.NestedAssembly:
                        return IsThisOrFriendAssembly(ThisAssembly, checkType.Assembly);
                }
            }
            catch (CompilerErrorException e)
            {
                parseContext.ReportError(
                    e.ErrorCode,
                    e.ErrorMessage,
                    e.ErrorSeverity,
                    SourceSpan.None);
                return false;
            }

            throw new ArgumentOutOfRangeException("checkType");
        }

        public static bool IsFamilyAccessible(Type type, Type parent)
        {
            if (type.Equals(parent))
            {
                return true;
            }

            if (type.IsGenericParameter)
            {
                Type typeConstraint = type.GetGenericParameterConstraints().FirstOrDefault(o => o.IsClass);
                if ((typeConstraint != null) && typeConstraint.IsSubclassOf(parent))
                {
                    return true;
                }
            }

            do
            {
                if (IsInstantiationOfSameGenericType(type, parent))
                {
                    return true;
                }

                type = type.BaseType;
            }
            while (type != null);

            return false;
        }

        public static int GetNumberOfTypeArguments(Type t)
        {
            return t.IsGenericParameter ? 0 : t.IsGenericType ? t.GetGenericArguments().Length : 0;
        }

        public static bool IsInstantiationOfSameGenericType(Type type, Type parent)
        {
            int tCount = GetNumberOfTypeArguments(type);
            int pCount = GetNumberOfTypeArguments(parent);

            if (tCount != pCount)
            {
                return false;
            }

            type = DropGenericTypeArguments(type);
            parent = DropGenericTypeArguments(parent);

            return type.Equals(parent);
        }


        //
        // Checks whether `type' is a subclass or nested child of `base_type'.
        //
        public static bool IsNestedFamilyAccessible(Type type, Type baseType)
        {
            do
            {
                if (IsFamilyAccessible(type, baseType))
                {
                    return true;
                }

                // Handle nested types.
                type = type.DeclaringType;
            } while (type != null);

            return false;
        }

        //
        // Checks whether `type' is a nested child of `parent'.
        //
        public static bool IsNestedChildOf(Type type, Type parent)
        {
            if (type == null)
            {
                return false;
            }

            type = DropGenericTypeArguments(type);
            parent = DropGenericTypeArguments(parent);

            if (IsEqual(type, parent))
            {
                return false;
            }

            type = type.DeclaringType;
            while (type != null)
            {
                if (IsEqual(type, parent))
                {
                    return true;
                }

                type = type.DeclaringType;
            }

            return false;
        }

        public static bool IsGenericParameter(Type type)
        {
            return type.IsGenericParameter;
        }

        public static int GenericParameterPosition(Type type)
        {
            return type.GenericParameterPosition;
        }

        public static bool IsGenericType(Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsGenericTypeDefinition(Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        public static bool ContainsGenericParameters(Type type)
        {
            return type.ContainsGenericParameters;
        }

        public static bool IsEqual(Type a, Type b)
        {
            if (a.Equals(b))
            {
                // MS BCL returns true even if enum types are different
                return a.BaseType == CoreTypes.Enum || b.BaseType == CoreTypes.Enum ? a.FullName == b.FullName : true;
            }

            if (IsGenericParameter(a) && IsGenericParameter(b))
            {
                return (a.DeclaringMethod != b.DeclaringMethod) &&
                    ((a.DeclaringMethod == null) || (b.DeclaringMethod == null))
                    ? false
                    : a.GenericParameterPosition == b.GenericParameterPosition;
            }

            if (a.IsArray && b.IsArray)
            {
                return (a.GetArrayRank() == b.GetArrayRank()) &&
                        IsEqual(a.GetElementType(), b.GetElementType());
            }

            if (a.IsByRef && b.IsByRef)
            {
                return IsEqual(a.GetElementType(), b.GetElementType());
            }

            if (IsGenericType(a) && IsGenericType(b))
            {
                Type genericDefinitionA = DropGenericTypeArguments(a);
                Type genericDefinitionB = DropGenericTypeArguments(b);

                if (genericDefinitionA != genericDefinitionB)
                {
                    return false;
                }

                if (genericDefinitionA.IsEnum && genericDefinitionB.IsEnum)
                {
                    return true;
                }

                Type[] genericArgumentsA = a.GetGenericArguments();
                Type[] genericArgumentsB = b.GetGenericArguments();

                return genericArgumentsA.Length != genericArgumentsB.Length
                    ? false
                    : genericArgumentsA
                    .Zip(genericArgumentsB, Tuple.Create)
                    .All(tuple => IsEqual(tuple.Item1, tuple.Item2));
            }

            return false;
        }

        public static bool IsPrivateAccessible(Type type, Type parent)
        {
            if (type == null)
            {
                return false;
            }

            return type.Equals(parent) ? true : DropGenericTypeArguments(type) == DropGenericTypeArguments(parent);
        }

        public static bool IsSpecialType(Type t)
        {
            return (t == typeof(ArgIterator)) || (t == typeof(TypedReference));
        }

        /// <summary>
        ///   This function returns the interfaces in the type `t'.  Works with
        ///   both types and TypeBuilders.
        /// </summary>
        public static Type[] GetInterfaces(Type t)
        {

            if (_interfaceCache.TryGetValue(t, out Type[] interfaces))
            {
                return interfaces;
            }

            //
            // The reason for catching the Array case is that Reflection.Emit
            // will not return a TypeBuilder for Array types of TypeBuilder types,
            // but will still throw an exception if we try to call GetInterfaces
            // on the type.
            //
            // Since the array interfaces are always constant, we return those for
            // the System.Array
            //

            if (t.IsArray)
            {
                t = CoreTypes.Array;
            }

            if (IsGenericType(t))
            {
                Type[] baseInterfaces = t.BaseType == null ? Type.EmptyTypes : GetInterfaces(t.BaseType);
                Type[] typeInterfaces = IsGenericType(t) ? t.GetGenericTypeDefinition().GetInterfaces() : Type.EmptyTypes;

                int baseCount = baseInterfaces.Length;

                interfaces = new Type[baseCount + typeInterfaces.Length];

                baseInterfaces.CopyTo(interfaces, 0);
                typeInterfaces.CopyTo(interfaces, baseCount);
            }
            else
            {
                interfaces = t.GetInterfaces();
            }

            _interfaceCache[t] = interfaces;

            return interfaces;
        }

        #region MemberLookup implementation
        //
        // Whether we allow private members in the result (since FindMembers
        // uses NonPublic for both protected and private), we need to distinguish.
        //

        internal class Closure
        {
            internal bool PrivateOk;

            // Who is invoking us and which type is being queried currently.
            internal Type InvocationType;
            internal Type QualifierType;

            // The assembly that defines the type is that is calling us
            internal Assembly InvocationAssembly;
            internal IList AlmostMatch;

            private bool CheckValidFamilyAccess(bool isStatic, MemberInfo m)
            {
                if (InvocationType == null)
                {
                    return false;
                }

                if (isStatic && QualifierType == null)
                {
                    // It resolved from a simple name, so it should be visible.
                    return true;
                }

                if (IsNestedChildOf(InvocationType, m.DeclaringType))
                {
                    return true;
                }

                for (Type t = InvocationType; t != null; t = t.DeclaringType)
                {
                    if (!IsFamilyAccessible(t, m.DeclaringType))
                    {
                        continue;
                    }

                    // Although a derived class can access protected members of its base class
                    // it cannot do so through an instance of the base class (CS1540).
                    // => Ancestry should be: declaring_type ->* invocation_type ->*  qualified_type
                    if (isStatic || QualifierType == null ||
                        IsInstantiationOfSameGenericType(t, QualifierType) ||
                        IsFamilyAccessible(QualifierType, t))
                    {
                        return true;
                    }
                }

                if (AlmostMatch != null)
                {
                    _ = AlmostMatch.Add(m);
                }

                return false;
            }

            //
            // This filter filters by name + whether it is ok to include private
            // members in the search
            //
            internal bool Filter(MemberInfo m, object filterCriteria)
            {
                //
                // Hack: we know that the filter criteria will always be in the
                // `closure' // fields. 
                //

                if ((filterCriteria != null) && (m.Name != (string)filterCriteria))
                {
                    return false;
                }

                if (((QualifierType == null) || (QualifierType == InvocationType)) &&
                    (InvocationType != null) &&
                    IsPrivateAccessible(m.DeclaringType, InvocationType))
                {
                    return true;
                }

                //
                // Ugly: we need to find out the type of `m', and depending
                // on this, tell whether we accept or not
                //
                if (m is MethodBase mb)
                {
                    MethodAttributes ma = mb.Attributes & MethodAttributes.MemberAccessMask;

                    if (ma == MethodAttributes.Public)
                    {
                        return true;
                    }

                    if (ma == MethodAttributes.PrivateScope)
                    {
                        return false;
                    }

                    if (ma == MethodAttributes.Private)
                    {
                        return PrivateOk ||
                               IsPrivateAccessible(InvocationType, m.DeclaringType) ||
                               IsNestedChildOf(InvocationType, m.DeclaringType);
                    }

                    if (IsThisOrFriendAssembly(InvocationAssembly, mb.DeclaringType.Assembly))
                    {
                        if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamORAssem)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamANDAssem)
                        {
                            return false;
                        }
                    }

                    // Family, FamORAssem or FamANDAssem
                    return CheckValidFamilyAccess(mb.IsStatic, m);
                }

                if (m is FieldInfo fi)
                {
                    FieldAttributes fa = fi.Attributes & FieldAttributes.FieldAccessMask;

                    if (fa == FieldAttributes.Public)
                    {
                        return true;
                    }

                    if (fa == FieldAttributes.PrivateScope)
                    {
                        return false;
                    }

                    if (fa == FieldAttributes.Private)
                    {
                        return PrivateOk ||
                               IsPrivateAccessible(InvocationType, m.DeclaringType) ||
                               IsNestedChildOf(InvocationType, m.DeclaringType);
                    }

                    if (IsThisOrFriendAssembly(InvocationAssembly, fi.DeclaringType.Assembly))
                    {
                        if ((fa == FieldAttributes.Assembly) ||
                            (fa == FieldAttributes.FamORAssem))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((fa == FieldAttributes.Assembly) ||
                            (fa == FieldAttributes.FamANDAssem))
                        {
                            return false;
                        }
                    }

                    // Family, FamORAssem or FamANDAssem
                    return CheckValidFamilyAccess(fi.IsStatic, m);
                }

                //
                // EventInfos and PropertyInfos, return true because they lack
                // permission information, so we need to check later on the methods.
                //
                return true;
            }
        }

        //
        // Looks up a member called `name' in the `queried_type'.  This lookup
        // is done by code that is contained in the definition for `invocation_type'
        // through a qualifier of type `qualifier_type' (or null if there is no qualifier).
        //
        // `invocation_type' is used to check whether we're allowed to access the requested
        // member wrt its protection level.
        //
        // When called from MemberAccess, `qualifier_type' is the type which is used to access
        // the requested member (`class B { A a = new A (); a.foo = 5; }'; here invocation_type
        // is B and qualifier_type is A).  This is used to do the CS1540 check.
        //
        // When resolving a SimpleName, `qualifier_type' is null.
        //
        // The `qualifier_type' is used for the CS1540 check; it's normally either null or
        // the same than `queried_type' - except when we're being called from BaseAccess;
        // in this case, `invocation_type' is the current type and `queried_type' the base
        // type, so this'd normally trigger a CS1540.
        //
        // The binding flags are `bf' and the kind of members being looked up are `mt'
        //
        // The return value always includes private members which code in `invocation_type'
        // is allowed to access (using the specified `qualifier_type' if given); only use
        // BindingFlags.NonPublic to bypass the permission check.
        //
        // The 'almost_match' argument is used for reporting error CS1540.
        //
        // Returns an array of a single element for everything but Methods/Constructors
        // that might return multiple matches.
        //
        public static MemberInfo[] MemberLookup(
            Type invocationType,
            Type qualifierType,
            Type queriedType,
            MemberTypes mt,
            BindingFlags originalBf,
            string name,
            IList almostMatch)
        {
            return RealMemberLookup(
                invocationType,
                qualifierType,
                queriedType,
                mt,
                originalBf,
                name,
                almostMatch);
        }

        private static MemberInfo[] RealMemberLookup(
            Type invocationType,
            Type qualifierType,
            Type queriedType,
            MemberTypes mt,
            BindingFlags originalBf,
            string name,
            IList almostMatch)
        {
            BindingFlags bf;

            List<MethodBase> methodList = null;
            Type currentType = queriedType;
            bool searching = (originalBf & BindingFlags.DeclaredOnly) == 0;
            bool skipInterfaceCheck = true;
            bool alwaysOkFlag = invocationType != null && IsNestedChildOf(invocationType, queriedType);

            Closure closure = new Closure
            {
                InvocationType = invocationType,
                InvocationAssembly = invocationType?.Assembly,
                QualifierType = qualifierType,
                AlmostMatch = almostMatch
            };

            // This is from the first time we find a method
            // in most cases, we do not actually find a method in the base class
            // so we can just ignore it, and save the arraylist allocation
            MemberInfo[] firstMembersList = null;
            bool useFirstMembersList = false;

            do
            {
                //
                // `NonPublic' is lame, because it includes both protected and
                // private methods, so we need to control this behavior by
                // explicitly tracking if a private method is ok or not.
                //
                // The possible cases are:
                //    public, private and protected (internal does not come into the
                //    equation)
                //
                bf = ((invocationType != null) &&
                    ((invocationType == currentType) || IsNestedChildOf(invocationType, currentType))) || alwaysOkFlag
                    ? originalBf | BindingFlags.NonPublic
                    : originalBf;

                closure.PrivateOk = (originalBf & BindingFlags.NonPublic) != 0;

                //var languageContext = (SxeLanguageContext)parseContext.Compiler.SourceUnit.LanguageContext;
                //var memberGroup = languageContext.DefaultBinderState.Binder.GetMember(
                //    MemberRequestKind.Get,
                //    currentType,
                //    name);
                //MemberInfo[] list = memberGroup.Select(o => o.GetMemberInfo()).ToArray();


                MemberInfo[] list = MemberLookup_FindMembers(currentType, mt, bf, name, closure.Filter, out bool usedCache);

                //
                // When queried for an interface type, the cache will automatically check all
                // inherited members, so we don't need to do this here.  However, this only
                // works if we already used the cache in the first iteration of this loop.
                //
                // If we used the cache in any further iteration, we can still terminate the
                // loop since the cache always looks in all base classes.
                //

                if (usedCache)
                {
                    searching = false;
                }
                else
                {
                    skipInterfaceCheck = false;
                }

                if (currentType == CoreTypes.Object)
                {
                    searching = false;
                }
                else
                {
                    currentType = currentType.BaseType;

                    //
                    // This happens with interfaces, they have a null
                    // basetype.  Look members up in the Object class.
                    //
                    if (currentType == null)
                    {
                        currentType = CoreTypes.Object;
                        searching = true;
                    }
                }

                if (list.Length == 0)
                {
                    continue;
                }

                //
                // Events and types are returned by both `static' and `instance'
                // searches, which means that our above FindMembers will
                // return two copies of the same.
                //
                if (list.Length == 1 && !(list[0] is MethodBase))
                {
                    return list;
                }

                //
                // Multiple properties: we query those just to find out the indexer
                // name
                //
                if (list[0] is PropertyInfo)
                {
                    return list;
                }

                //
                // We found an event: the cache lookup returns both the event and
                // its private field.
                //
                if (list[0] is EventInfo)
                {
                    return (list.Length == 2) && (list[1] is FieldInfo) ? (new[] { list[0] }) : list;
                }

                //
                // We found methods, turn the search into "method scan"
                // mode.
                //

                if (firstMembersList != null)
                {
                    if (useFirstMembersList)
                    {
                        methodList = CopyNewMethods(methodList, firstMembersList);
                        useFirstMembersList = false;
                    }

                    methodList = CopyNewMethods(methodList, list);
                }
                else
                {
                    firstMembersList = list;
                    useFirstMembersList = true;
                    mt &= MemberTypes.Method | MemberTypes.Constructor;
                }
            }
            while (searching);

            if (useFirstMembersList)
            {
                return firstMembersList;
            }

            if (methodList != null && methodList.Count > 0)
            {
                return methodList.ToArray();
            }
            //
            // This happens if we already used the cache in the first iteration, in this case
            // the cache already looked in all interfaces.
            //
            if (skipInterfaceCheck)
            {
                return null;
            }

            //
            // Interfaces do not list members they inherit, so we have to
            // scan those.
            // 
            if (!queriedType.IsInterface)
            {
                return null;
            }

            if (queriedType.IsArray)
            {
                queriedType = CoreTypes.Array;
            }

            Type[] ifaces = GetInterfaces(queriedType);
            return ifaces?.Select(itype => MemberLookup(null, null, itype, mt, bf, name, null))
                .FirstOrDefault(x => x != null);
        }

        public static List<MethodBase> CopyNewMethods(IList<MethodBase> targetList, IList newMembers)
        {
            return Enumerable
                .Concat(targetList ?? Enumerable.Empty<MethodBase>(), newMembers.OfType<MethodBase>())
                .Distinct()
                .ToList();
        }

        /// <summary>
        ///   This method is only called from within MemberLookup.  It tries to use the member
        ///   cache if possible and falls back to the normal FindMembers if not.  The `used_cache'
        ///   flag tells the caller whether we used the cache or not.  If we used the cache, then
        ///   our return value will already contain all inherited members and the caller don't need
        ///   to check base classes and interfaces anymore.
        /// </summary>
        private static MemberInfo[] MemberLookup_FindMembers(
            Type t,
            MemberTypes mt,
            BindingFlags bf,
            string name,
            MemberFilter filter,
            out bool usedCache)
        {
            //
            // We have to take care of arrays specially, because GetType on
            // a TypeBuilder array will return a Type, not a TypeBuilder,
            // and we can not call FindMembers on this type.
            //
            if (t.IsArray)
            {
                usedCache = true;
                return TypeHandle.ArrayType.MemberCache.FindMembers(
                    mt,
                    bf,
                    name,
                    filter,
                    null);
            }

            if (IsGenericType(t) && (mt == MemberTypes.NestedType))
            {
                //
                // This happens if we're resolving a class'es base class and interfaces
                // in TypeContainer.DefineType().  At this time, the types aren't
                // populated yet, so we can't use the cache.
                //
                MemberInfo[] info = t.FindMembers(
                    mt,
                    bf | BindingFlags.DeclaredOnly,
                    filter,
                    name);
                usedCache = false;
                return info;
            }

            //
            // This call will always succeed.  There is exactly one TypeHandle instance per
            // type, TypeHandle.GetMemberCache() will, if necessary, create a new one, and return
            // the corresponding MemberCache.
            //
            MemberCache cache = TypeHandle.GetMemberCache(t);
            usedCache = true;

            return cache.FindMembers(mt, bf, name, filter, null);
        }

        private static readonly Dictionary<Type, MemberCache> _typeMemberCache = new Dictionary<Type, MemberCache>();

        public static MemberCache LookupBaseInterfacesCache(Type t)
        {
            Type[] interfaces = GetInterfaces(t);

            if (interfaces != null && interfaces.Length == 1)
            {
                return TypeHandle.GetMemberCache(interfaces[0]);
            }

            // TODO: the builder_to_member_cache should be indexed by 'ifaces', not 't'
            if (_typeMemberCache.TryGetValue(t, out MemberCache cache))
            {
                return cache;
            }

            cache = new MemberCache(interfaces);
            _typeMemberCache[t] = cache;

            return cache;
        }
        #endregion

        #region Anonymous Type Management

        public const string AnonymousTypeNamePrefix = "<>__AnonType";
        private static readonly Dictionary<AnonymousTypeKey, AnonymousTypeClass> _anonymousClasses = new Dictionary<AnonymousTypeKey, AnonymousTypeClass>();
        private static readonly AssemblyName _assemblyName = new AssemblyName { Name = "DynamicLinqTypes" };
        private static readonly ModuleBuilder _moduleBuilder;

        internal static bool IsAnonymousType(Type type)
        {
            return _anonymousClasses.Values.Any(o => o.Type == type);
        }

        private sealed class AnonymousTypeKey : IEquatable<AnonymousTypeKey>
        {
            private readonly string[] _memberNames;
            private readonly Type[] _memberTypes;
            private readonly int _hashCode;

            internal AnonymousTypeKey(AnonymousObjectInitializer initializer)
            {
                if (initializer == null)
                {
                    throw new ArgumentNullException("initializer");
                }

                AnonymousMemberDeclarator[] memberDeclarators = initializer.MemberDeclarators
                    .OrderBy(o => o.Name, StringComparer.Ordinal)
                    .ToArray();

                _memberNames = memberDeclarators.Select(o => o.Name).ToArray();
                _memberTypes = memberDeclarators.Select(o => o.Initializer.Type).ToArray();

                _hashCode = memberDeclarators.Aggregate(
                    6551,
                    (hash, member) => hash ^ (hash << 5) ^ member.Name.GetHashCode());
            }

            public bool Equals(AnonymousTypeKey other)
            {
                return _memberNames.SequenceEqual(other._memberNames) &&
                       _memberTypes.Zip(other._memberTypes, (a, b) => new { a, b }).All(o => IsEqual(o.a, o.b));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as AnonymousTypeKey);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        internal static AnonymousTypeClass GetAnonymousType(AnonymousObjectInitializer initializer)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }

            if (initializer.MemberDeclarators.Count == 0)
            {
                throw new ArgumentOutOfRangeException("initializer", "fields must have at least 1 field definition");
            }

            lock (_anonymousClasses)
            {
                AnonymousTypeKey key = new AnonymousTypeKey(initializer);

                if (_anonymousClasses.TryGetValue(key, out AnonymousTypeClass anonymousClass))
                {
                    return _anonymousClasses[key];
                }

                TypeBuilder typeBuilder = _moduleBuilder.DefineType(
                    AnonymousTypeNamePrefix + _anonymousClasses.Count,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

                foreach (AnonymousMemberDeclarator member in initializer.MemberDeclarators)
                {
                    _ = typeBuilder.DefineField(
                        member.Name,
                        member.Type,
                        FieldAttributes.Public);
                }

                anonymousClass = new AnonymousTypeClass(
                    initializer,
                    typeBuilder.CreateType());

                _anonymousClasses[key] = anonymousClass;

                return anonymousClass;
            }
        }
        #endregion

        public static string GetMethodName(MethodInfo m)
        {
            return !m.IsGenericMethodDefinition && !m.IsGenericMethod
                ? m.Name
                : MemberName.MakeName(m.Name, (m.GetGenericArguments() ?? Type.EmptyTypes).Length);
        }

        private static readonly Dictionary<MethodBase, MethodBase> _methodOverrides = new Dictionary<MethodBase, MethodBase>();

        public static void RegisterOverride(MethodBase overrideMethod, MethodBase baseMethod)
        {
            if (_methodOverrides.TryGetValue(overrideMethod, out MethodBase knownBaseMethod))
            {
                if (knownBaseMethod != baseMethod)
                {
                    throw new InternalErrorException("Override mismatch: " + overrideMethod);
                }
            }
            else
            {
                _methodOverrides[overrideMethod] = baseMethod;
            }
        }

        public static bool IsOverride(this MethodBase m)
        {
            m = DropGenericMethodArguments(m);

            return m.IsVirtual &&
                   (m.Attributes & MethodAttributes.NewSlot) == 0 &&
                   (m is MethodBuilder || _methodOverrides.ContainsKey(m));
        }

        public static MethodBase DropGenericMethodArguments(this MethodBase m)
        {
            return m.IsGenericMethod
                       ? ((MethodInfo)m).GetGenericMethodDefinition()
                       : m;
        }

        public static bool IsValueType(this Type t)
        {
            if (IsGenericParameter(t))
            {
                GenericConstraints constraints = GetTypeParameterConstraints(t);
                return constraints == null ? false : constraints.IsValueType;
            }

            return IsStruct(t) || IsEnumType(t);
        }

        public static GenericConstraints GetTypeParameterConstraints(this Type t)
        {
            if (!t.IsGenericParameter)
            {
                throw new InvalidOperationException();
            }

            return ReflectionConstraints.GetConstraints(t);
        }

        public static bool IsReferenceType(this Type t)
        {
            if (IsGenericParameter(t))
            {
                GenericConstraints constraints = GetTypeParameterConstraints(t);
                return constraints == null ? false : constraints.IsReferenceType;
            }

            return !IsStruct(t) && !IsEnumType(t);
        }

        public static bool IsStruct(this Type t)
        {
            return t.BaseType == CoreTypes.ValueType && t != CoreTypes.Enum && t.IsSealed;
        }

        public static bool IsInterfaceType(this Type t)
        {
            return t.IsInterface;
        }

        public static bool IsEnumType(this Type t, bool allowNullable = false)
        {
            t = DropGenericTypeArguments(t);

            if (t.BaseType == CoreTypes.Enum)
            {
                return true;
            }

            if (!allowNullable)
            {
                return false;
            }


            return IsNullableType(t, out Type baseType) ? IsEnumType(baseType, true) : false;
        }

        public static bool IsNullableType(this Type t)
        {
            return CoreTypes.GenericNullable == DropGenericTypeArguments(t);
        }

        public static bool IsNullableType(this Type t, out Type baseType)
        {
            if (!IsNullableType(t))
            {
                baseType = null;
                return false;
            }

            baseType = t.GetGenericArguments()[0];
            return true;
        }

        public static bool HasGenericArguments(Type type)
        {
            return GetNumberOfTypeArguments(type) > 0;
        }

        /// <remarks>
        ///  The following is used to check if a given type implements an interface.
        ///  The cache helps us reduce the expense of hitting Type.GetInterfaces everytime.
        /// </remarks>
        public static bool ImplementsInterface(Type t, Type iface)
        {
            //
            // FIXME OPTIMIZATION:
            // as soon as we hit a non-TypeBuiler in the interface
            // chain, we could return, as the `Type.GetInterfaces'
            // will return all the interfaces implement by the type
            // or its bases.
            //
            do
            {
                Type[] interfaces = GetInterfaces(t);

                if (interfaces != null)
                {
                    if (interfaces.Any(i => i == iface || IsVariantOf(i, iface)))
                    {
                        return true;
                    }
                }

                t = t.BaseType;
            } while (t != null);

            return false;
        }

        public static bool IsVariantOf(Type type1, Type type2)
        {
            if (!type1.IsGenericType || !type2.IsGenericType)
            {
                return false;
            }

            Type genericTargetType = DropGenericTypeArguments(type2);

            if (DropGenericTypeArguments(type1) != genericTargetType)
            {
                return false;
            }

            Type[] t1 = type1.GetGenericArguments();
            Type[] t2 = type2.GetGenericArguments();
            Type[] definitionArgs = genericTargetType.GetGenericArguments();

            for (int i = 0; i < definitionArgs.Length; ++i)
            {
                Variance v = GetTypeParameterVariance(definitionArgs[i]);
                if (v == Variance.None)
                {
                    if (t1[i] == t2[i])
                    {
                        continue;
                    }

                    return false;
                }

                if (v == Variance.Covariant)
                {
                    if (!TypeUtils.IsImplicitlyConvertible(t1[i], t2[i]))
                    {
                        return false;
                    }
                }
                else if (!TypeUtils.IsImplicitlyConvertible(t2[i], t1[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static Variance GetTypeParameterVariance(Type type)
        {
            switch (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)
            {
                case GenericParameterAttributes.Covariant:
                    return Variance.Covariant;
                case GenericParameterAttributes.Contravariant:
                    return Variance.Contravariant;
                default:
                    return Variance.None;
            }
        }

        public static string GetFullNameSignature(MemberInfo mi)
        {
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null)
            {
                MethodBase pmi = pi.GetGetMethod(true) ?? pi.GetSetMethod(true);
                if (GetParameterData(pmi).Count > 0)
                {
                    mi = pmi;
                }
            }
            return (mi is MethodBase)
                       ? GetCSharpSignature(mi as MethodBase)
                       : GetCSharpName(mi.DeclaringType) + '.' + mi.Name;
        }

        public static FieldInfo GetGenericFieldDefinition(FieldInfo fi)
        {
            if (fi.DeclaringType.IsGenericTypeDefinition || !fi.DeclaringType.IsGenericType)
            {
                return fi;
            }

            Type t = fi.DeclaringType.GetGenericTypeDefinition();
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            // TODO: use CodeGen.Module.Builder.ResolveField (fi.MetadataToken);
            foreach (FieldInfo f in t.GetFields(bf).Where(f => f.MetadataToken == fi.MetadataToken))
            {
                return f;
            }

            return fi;
        }

        public static bool IsSubclassOf(Type type, Type baseType)
        {
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            if (type.IsSubclassOf(baseType))
            {
                return true;
            }

            do
            {
                if (IsEqual(type, baseType))
                {
                    return true;
                }

                type = type.BaseType;
            } while (type != null);

            return false;
        }

        public static bool IsDelegateType(Type t)
        {
            if (IsGenericParameter(t))
            {
                return false;
            }

            if (t == CoreTypes.Delegate || t == CoreTypes.MulticastDelegate)
            {
                return false;
            }

            t = DropGenericTypeArguments(t);
            return IsSubclassOf(t, CoreTypes.Delegate);
        }

        private static bool IsSignatureEqual(Type a, Type b)
        {
            // Consider the following example (bug #77674):
            //
            //     public abstract class A
            //     {
            //        public abstract T Foo<T> ();
            //     }
            //
            //     public abstract class B : A
            //     {
            //        public override U Foo<T> ()
            //        { return default (U); }
            //     }
            //
            // Here, `T' and `U' are method type parameters from different methods
            // (A.Foo and B.Foo), so both `==' and Equals() will fail.
            //
            // However, since we're determining whether B.Foo() overrides A.Foo(),
            // we need to do a signature based comparision and consider them equal.

            if (a == b)
            {
                return true;
            }

            if (a.IsGenericParameter && b.IsGenericParameter &&
                (a.DeclaringMethod != null) && (b.DeclaringMethod != null))
            {
                return a.GenericParameterPosition == b.GenericParameterPosition;
            }

            if (a.IsArray && b.IsArray)
            {
                return a.GetArrayRank() != b.GetArrayRank() ? false : IsSignatureEqual(a.GetElementType(), b.GetElementType());
            }

            if (a.IsByRef && b.IsByRef)
            {
                return IsSignatureEqual(a.GetElementType(), b.GetElementType());
            }

            if (IsGenericType(a) && IsGenericType(b))
            {
                if (DropGenericTypeArguments(a) != DropGenericTypeArguments(b))
                {
                    return false;
                }

                Type[] aargs = a.GetGenericArguments();
                Type[] bargs = b.GetGenericArguments();

                return aargs.Length != bargs.Length ? false : !aargs.Where((t, i) => !IsSignatureEqual(t, bargs[i])).Any();
            }

            return false;
        }

        public static bool ArrayContainsMethod(MemberInfo[] array, MethodBase newMethod, bool ignoreDeclType)
        {
            Type[] newArgs = GetParameterData(newMethod).Types;

            foreach (MethodBase method in array)
            {
                if (!ignoreDeclType && method.DeclaringType != newMethod.DeclaringType)
                {
                    continue;
                }

                if (method.Name != newMethod.Name)
                {
                    continue;
                }

                if (method is MethodInfo && newMethod is MethodInfo &&
                    !IsSignatureEqual(
                         ((MethodInfo)method).ReturnType,
                         ((MethodInfo)newMethod).ReturnType))
                {
                    continue;
                }

                Type[] oldArgs = GetParameterData(method).Types;
                int oldCount = oldArgs.Length;
                int i;

                if (newArgs.Length != oldCount)
                {
                    continue;
                }

                for (i = 0; i < oldCount; i++)
                {
                    if (!IsSignatureEqual(oldArgs[i], newArgs[i]))
                    {
                        break;
                    }
                }
                if (i != oldCount)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static MethodBase TryGetBaseDefinition(MethodBase m)
        {
            m = DropGenericMethodArguments(m);
            return _methodOverrides[m];
        }

        private static readonly Dictionary<Tuple<Type, string>, Type> _typeHash = new Dictionary<Tuple<Type, string>, Type>();

        public static Type GetConstructedType(Type t, string dim)
        {

            Tuple<Type, string> key = Tuple.Create(t, dim);
            if (_typeHash.TryGetValue(key, out Type constructedType))
            {
                return constructedType;
            }

            constructedType = t.Module.GetType(t + dim);

            if (constructedType != null)
            {
                _typeHash[key] = constructedType;
                return constructedType;
            }

            if (t.IsGenericParameter || t.IsGenericType)
            {
                int pos = 0;
                Type result = t;
                while ((pos < dim.Length) && (dim[pos] == '['))
                {
                    pos++;

                    if (dim[pos] == ']')
                    {
                        result = result.MakeArrayType();
                        pos++;

                        if (pos < dim.Length)
                        {
                            continue;
                        }

                        _typeHash[key] = result;
                        return result;
                    }

                    int rank = 0;
                    while (dim[pos] == ',')
                    {
                        pos++;
                        rank++;
                    }

                    if ((dim[pos] != ']') || (pos != dim.Length - 1))
                    {
                        break;
                    }

                    result = result.MakeArrayType(rank + 1);
                    _typeHash[key] = result;
                    return result;
                }
            }

            return null;
        }

        public static bool IsEqual(Type[] a, Type[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] == null || b[i] == null)
                {
                    if (a[i] == b[i])
                    {
                        continue;
                    }

                    return false;
                }

                if (!IsEqual(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static int InferTypeArguments(ParseContext ec, Arguments arguments, ref MethodBase method)
        {
            TypeInferenceBase typeInference = TypeInferenceBase.CreateInstance(arguments);

            Type[] inferredArgs = typeInference.InferMethodArguments(ec, method);
            if (inferredArgs == null)
            {
                return typeInference.InferenceScore;
            }

            if (inferredArgs.Length == 0)
            {
                return 0;
            }

            method = ((MethodInfo)method).MakeGenericMethod(inferredArgs);
            return 0;
        }

        public static bool ImplicitConversionExists(ParseContext ec, Expression source, Type targetType)
        {
            if (source is LambdaExpression lambda)
            {
                return !IsDelegateType(targetType) &&
                    DropGenericTypeArguments(targetType) != CoreTypes.GenericExpression
                    ? false
                    : lambda.ImplicitStandardConversionExists(ec, targetType);
            }

            return TypeUtils.IsImplicitlyConvertible(source.Type, targetType);
        }

        public static Variance CheckTypeVariance(Type t, Variance expected, IMemberContext member)
        {
            if (t.IsGenericType)
            {
                Type[] typeArgsDefinition = DropGenericTypeArguments(t).GetGenericArguments();
                Type[] typeArgs = t.GetGenericArguments();

                for (int i = 0; i < typeArgsDefinition.Length; ++i)
                {
                    Variance variance = GetTypeParameterVariance(typeArgsDefinition[i]);
                    _ = CheckTypeVariance(typeArgs[i], (Variance)((int)variance * (int)expected), member);
                }

                return expected;
            }

            return t.IsArray ? CheckTypeVariance(t.GetElementType(), expected, member) : Variance.None;
        }

        public static bool IsDynamicType(Type t)
        {
            if (t == typeof(DynamicObject))
            {
                return true;
            }

            return t != CoreTypes.Object ? false : t.IsDefined(PredefinedAttributes.Dynamic, false);
        }

        //
        // Returns the MethodBase for "Invoke" from a delegate type, this is used
        // to extract the signature of a delegate.
        //
        public static MethodInfo GetDelegateInvokeMethod(ParseContext ctx, Type containerType, Type delegateType)
        {
            Type dt = delegateType;

            Type[] gArgs = null;
            if (IsGenericType(delegateType))
            {
                gArgs = delegateType.GetGenericArguments();
                dt = DropGenericTypeArguments(delegateType);
            }

            Expression ml = Expression.MemberLookup(
                ctx,
                containerType,
                null,
                dt,
                "Invoke",
                SourceSpan.None);

            if (!(ml is MethodGroupExpression mg))
            {
                ctx.ReportError(
                    -100,
                    "Internal error: could not find Invoke method!");

                // FIXME: null will cause a crash later
                return null;
            }

            MethodInfo invoke = (MethodInfo)mg.Methods[0];

            if (gArgs != null)
            {
                ParametersCollection p = GetParameterData(invoke);
                p = p.InflateTypes(gArgs, gArgs);
                _methodParameters[invoke] = p;
                return invoke;
            }

            return invoke;
        }

        public static ParametersCollection GetDelegateParameters(ParseContext ec, Type t)
        {
            MethodInfo invokeMethod = GetDelegateInvokeMethod(ec, t, t);
            return GetParameterData(invokeMethod);
        }

        private static readonly Dictionary<FieldInfo, IConstant> _fieldConstants = new Dictionary<FieldInfo, IConstant>();

        public static void RegisterConstant(FieldInfo fb, IConstant ic)
        {
            _fieldConstants.Add(fb, ic);
        }

        public static IConstant GetConstant(FieldInfo fb)
        {
            if (fb == null)
            {
                return null;
            }

            return _fieldConstants.TryGetValue(fb, out IConstant constant) ? constant : null;
        }
    }

    public enum Variance
    {
        //
        // Don't add or modify internal values, they are used as -/+ calculation signs
        //
        None = 0,
        Covariant = 1,
        Contravariant = -1
    }
}
