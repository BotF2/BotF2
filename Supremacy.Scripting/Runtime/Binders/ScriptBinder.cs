using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using MSAst = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using TypeUtils = Supremacy.Scripting.Utility.TypeUtils;

namespace Supremacy.Scripting.Runtime.Binders
{
    public class ScriptBinder : DefaultBinder
    {
        private readonly ScriptLanguageContext _context;
        private readonly Dictionary<Type, IList<Type>> _extensionTypes;
        private bool _registeredInterfaceExtensions;    // true if someone has registered extensions for interfaces

        public ScriptLanguageContext Context
        {
            get { return _context; }
        }

        public ScriptBinder(ScriptLanguageContext context)
            : base(context.DomainManager)
        {
            _context = context;
            _extensionTypes = new Dictionary<Type, IList<Type>>();

            _context.DomainManager.AssemblyLoaded += DomainManager_AssemblyLoaded;

            foreach (var assembly in _context.DomainManager.GetLoadedAssemblyList())
                DomainManager_AssemblyLoaded(this, new AssemblyLoadedEventArgs(assembly));
        }

        /// <summary>
        /// Event handler for when our domain manager has an assembly loaded by the user hosting the script
        /// runtime.  Here we can gather any information regarding extension methods.  
        /// 
        /// Currently DLR-style extension methods become immediately available w/o an explicit import step.
        /// </summary>
        private void DomainManager_AssemblyLoaded(object sender, AssemblyLoadedEventArgs e)
        {
            var assembly = e.Assembly;

            var extensions =
                (
                    from type in assembly.GetTypes()
                    let extendedTypes =
                        (from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                         where method.IsDefined(typeof(ExtensionAttribute), false)
                         let parameters = method.GetParameters()
                         where parameters.Length != 0
                         let parameterType = parameters[0].ParameterType
                         select Tuple.Create(
                             parameterType.IsGenericType
                                 ? parameterType.GetGenericTypeDefinition()
                                 : parameterType,
                             type)).Distinct()
                    from extendedType in extendedTypes
                    select new
                           {
                               ExtendedType = extendedType.Item1,
                               ExtendingType = extendedType.Item2
                           }
                ).ToList();

            lock (_extensionTypes)
            {
                foreach (var extension in extensions)
                {
                    if (extension.ExtendedType.IsInterface)
                        _registeredInterfaceExtensions = true;

                    IList<Type> typeList;
                    
                    if (!_extensionTypes.TryGetValue(extension.ExtendedType, out typeList))
                        _extensionTypes[extension.ExtendedType] = typeList = new List<Type>();
                    else if (typeList.IsReadOnly)
                        _extensionTypes[extension.ExtendedType] = typeList = new List<Type>(typeList);

                    if (!typeList.Contains(extension.ExtendingType))
                        typeList.Add(extension.ExtendingType);
                }
            }
        }

        #region ActionBinder overrides
        public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
        {
            var conversionResultKind = level.ToConversionResultKind();
            
            if (!conversionResultKind.HasValue)
                return toType.IsAssignableFrom(fromType);

            var conversionResult = ConvertExpression(
                MSAst.Default(fromType),
                toType,
                conversionResultKind.Value,
                _context.OverloadResolver);

            return (conversionResult != null);
        }

        public override object Convert(object obj, Type toType)
        {
            if (toType == typeof(int))
                return System.Convert.ToInt32(obj);
            if (toType == typeof(decimal))
                return System.Convert.ToDecimal(obj);
            if (toType == typeof(char))
            {
                var stringValue = obj as string;
                if (stringValue != null)
                {
                    if (stringValue == string.Empty)
                        return '\0';
                    return stringValue[0];
                }
            }
            return base.Convert(obj, toType);
        }

        public override Candidate PreferConvert(Type t1, Type t2)
        {
            return Candidate.Ambiguous;
        }
        #endregion

        #region Conversions
        internal MSAst ConvertTo(Type toType, ConversionResultKind kind, MSAst expression)
        {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(expression, "arg");

            if (expression.Type == toType)
                return expression;

            // try all the conversions - first look for conversions against the expression type,
            // these can be done w/o any additional tests.  Then look for conversions against the 
            // restricted type.

            var res = TryConvertToObject(toType, expression.Type, expression) ??
                      TryAllConversions(toType, kind, expression.Type, expression) ??
                      //TryAllConversions(toType, kind, expression.Type, expression) ??
                      MakeErrorTarget(toType, kind, expression);

            if ((kind == ConversionResultKind.ExplicitTry || kind == ConversionResultKind.ImplicitTry) &&
                toType.IsValueType)
            {
                res = AstUtils.Convert(
                    res,
                    typeof(object));
            }

            // TODO: Revisit explicit casts.
            if (kind == ConversionResultKind.ExplicitCast)
            {
                res = AstUtils.Convert(
                    res,
                    toType);
            }

            return res;
        }

        #region Conversion attempt helpers
        /// <summary>Checks if the conversion is to object and produces a target if it is.</summary>
        private static MSAst TryConvertToObject(Type toType, Type knownType, MSAst arg)
        {
            if (toType == typeof(object))
                return knownType.IsValueType ? MakeBoxingTarget(arg) : arg;
            return null;
        }

        /// <summary>Checks if any conversions are available and if so builds the target for that conversion.</summary>
        private MSAst TryAllConversions(Type toType, ConversionResultKind kind, Type knownType, MSAst arg)
        {
            return
                TryAssignableConversion(toType, knownType, arg) ?? // known type -> known type
                TryExtensibleConversion(toType, knownType, arg) ?? // Extensible<T> -> Extensible<T>.Value
                TryUserDefinedConversion(kind, toType, knownType, arg) ?? // op_Implicit
                TryImplicitNumericConversion(toType, knownType, arg) ?? // op_Implicit
                TryNullableConversion(toType, kind, knownType, arg) ?? // null -> Nullable<T> or T -> Nullable<T>
                TryNullConversion(toType, knownType); // null -> reference type
        }

        /// <summary>Checks if the conversion can be handled by a simple cast.</summary>
        private static MSAst TryAssignableConversion(Type toType, Type type, MSAst arg)
        {
            if (toType.IsAssignableFrom(type) ||
                (type == typeof(DynamicNull) && (toType.IsClass || toType.IsInterface)))
            {
                // MakeSimpleConversionTarget handles the ConversionResultKind check
                return MakeSimpleConversionTarget(toType, arg);
            }

            return null;
        }

        /// <summary>Checks if the conversion can be handled by calling a user-defined conversion method.</summary>
        internal MSAst TryUserDefinedConversion(ConversionResultKind kind, Type toType, Type type, MSAst arg)
        {
            var fromType = GetUnderlyingType(type);

            var res = TryOneConversion(kind, toType, type, fromType, "op_Implicit", true, arg) ??
                      TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, true, arg);

            if (kind == ConversionResultKind.ExplicitCast ||
                kind == ConversionResultKind.ExplicitTry)
            {
                // finally try explicit conversions
                res = res ??
                      TryOneConversion(kind, toType, type, fromType, "op_Explicit", false, arg) ??
                      TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, false, arg);
            }

            return res;
        }

        /// <summary>
        ///   Helper that checkes both types to see if either one defines the specified conversion
        ///   method.
        /// </summary>
        private MSAst TryOneConversion(
            ConversionResultKind kind,
            Type toType,
            Type type,
            Type fromType,
            string methodName,
            bool isImplicit,
            MSAst arg)
        {
            var conversions = GetMember(fromType, methodName);
            var res = TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, arg);
            if (res != null)
                return res;

            // then on the type we're trying to convert to
            conversions = GetMember(toType, methodName);
            return TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, arg);
        }

        /// <summary>
        ///   Checks if any of the members of the MemberGroup provide the applicable conversion and 
        ///   if so uses it to build a conversion rule.
        /// </summary>
        private static MSAst TryUserDefinedConversion(
            ConversionResultKind kind, Type toType, Type type, MemberGroup conversions, bool isImplicit, MSAst arg)
        {
            var checkType = GetUnderlyingType(type);

            foreach (var mt in conversions)
            {
                if (mt.MemberType != TrackerTypes.Method)
                    continue;

                var method = (MethodTracker)mt;

                if (isImplicit && method.Method.IsDefined(typeof(ExplicitConversionMethodAttribute), true))
                {
                    continue;
                }

                if (method.Method.ReturnType == toType)
                {
                    // TODO: IsAssignableFrom?  IsSubclass?
                    var pis = method.Method.GetParameters();

                    if (pis.Length == 1 && pis[0].ParameterType.IsAssignableFrom(checkType))
                    {
                        // we can use this method
                        if (type == checkType)
                            return MakeConversionTarget(kind, method, type, isImplicit, arg);
                        return MakeExtensibleConversionTarget(kind, method, type, isImplicit, arg);
                    }
                }
            }
            return null;
        }

        /// <summary>Checks if the conversion is to applicable by extracting the value from Extensible of T.</summary>
        private static MSAst TryExtensibleConversion(Type toType, Type type, MSAst arg)
        {
            var extensibleType = typeof(Extensible<>).MakeGenericType(toType);
            if (extensibleType.IsAssignableFrom(type))
            {
                return MakeExtensibleTarget(extensibleType, arg);
            }
            return null;
        }

        /// <summary>Checks if there's an implicit numeric conversion for primitive data types.</summary>
        private static MSAst TryImplicitNumericConversion(Type toType, Type type, MSAst arg)
        {
            var checkType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Extensible<>))
            {
                checkType = type.GetGenericArguments()[0];
            }

            if (TypeUtils.IsNumeric(toType) && TypeUtils.IsNumeric(checkType))
            {
                // check for an explicit conversion
                int toX, toY, fromX, fromY;
                if (TypeUtils.GetNumericConversionOrder(Type.GetTypeCode(toType), out toX, out toY) &&
                    TypeUtils.GetNumericConversionOrder(Type.GetTypeCode(checkType), out fromX, out fromY))
                {
                    if (TypeUtils.IsImplicitlyConvertible(fromX, fromY, toX, toY))
                    {
                        // MakeSimpleConversionTarget handles the ConversionResultKind check
                        if (type == checkType)
                        {
                            return MakeSimpleConversionTarget(toType, arg);
                        }
                        return MakeSimpleExtensibleConversionTarget(toType, arg);
                    }
                }
            }
            return null;
        }

        /// <summary>Checks if there's a conversion to/from Nullable of T.</summary>
        private static MSAst TryNullableConversion(Type toType, ConversionResultKind kind, Type knownType, MSAst arg)
        {
            if (toType.IsGenericType && (toType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                if (knownType == typeof(DynamicNull))
                    return MakeNullToNullableOfTTarget(toType);

                if (knownType == toType.GetGenericArguments()[0])
                    return MakeTToNullableOfTTarget(toType, knownType, arg);

                if ((kind == ConversionResultKind.ExplicitCast) || (kind == ConversionResultKind.ExplicitTry))
                {
                    if (knownType != typeof(object))
                    {
                        // when doing an explicit cast we'll do things like int -> Nullable<float>
                        return MakeConvertingToTToNullableOfTTarget(toType, kind, arg);
                    }
                }
            }

            return null;
        }

        /// <summary>Checks to see if there's a conversion of null to a reference type</summary>
        private static MSAst TryNullConversion(Type toType, Type knownType)
        {
            if (knownType == typeof(DynamicNull) && !toType.IsValueType)
                return MakeNullTarget(toType);
            return null;
        }
        #endregion

        #region Rule production helpers
        /// <summary>Helper to produce an error when a conversion cannot occur</summary>
        private static MSAst MakeErrorTarget(Type toType, ConversionResultKind kind, MSAst arg)
        {
            MSAst target;

            switch (kind)
            {
                case ConversionResultKind.ImplicitCast:
                case ConversionResultKind.ExplicitCast:
                    //target = DefaultBinder.MakeError(
                    //    _binder.Binder.MakeConversionError(toType, arg),
                    //    toType);
                    target = arg;
                    break;
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    target = GetTryConvertReturnValue(toType);
                    break;
                default:
                    throw new InvalidOperationException(kind.ToString());
            }

            return target;
        }

        /// <summary>Helper to produce a rule which just boxes a value type</summary>
        private static MSAst MakeBoxingTarget(MSAst arg)
        {
            // MakeSimpleConversionTarget handles the ConversionResultKind check
            return MakeSimpleConversionTarget(typeof(object), arg);
        }

        /// <summary>Helper to produce a conversion rule by calling the helper method to do the convert</summary>
        private static MSAst MakeConversionTarget(
            ConversionResultKind kind, MethodTracker method, Type fromType, bool isImplicit, MSAst arg)
        {
            var param = AstUtils.Convert(arg, fromType);

            return MakeConversionTargetWorker(kind, method, isImplicit, param);
        }

        /// <summary>Helper to produce a conversion rule by calling the helper method to do the convert</summary>
        private static MSAst MakeExtensibleConversionTarget(
            ConversionResultKind kind, MethodTracker method, Type fromType, bool isImplicit, MSAst arg)
        {
            return MakeConversionTargetWorker(kind, method, isImplicit, GetExtensibleValue(fromType, arg));
        }

        /// <summary>
        ///   Helper to produce a conversion rule by calling the method to do the convert.  This version takes the parameter
        ///   to be passed to the conversion function and we call it w/ our own value or w/ our Extensible.Value.
        /// </summary>
        private static MSAst MakeConversionTargetWorker(
            ConversionResultKind kind, MethodTracker method, bool isImplicit, MSAst param)
        {
            return WrapForThrowingTry(
                kind,
                isImplicit,
                AstUtils.SimpleCallHelper(
                    method.Method,
                    param
                    ),
                method.Method.ReturnType);
        }

        /// <summary>
        ///   Helper to wrap explicit conversion call into try/catch incase it throws an exception.  If
        ///   it throws the default value is returned.
        /// </summary>
        private static MSAst WrapForThrowingTry(ConversionResultKind kind, bool isImplicit, MSAst ret, Type retType)
        {
            if (!isImplicit && kind == ConversionResultKind.ExplicitTry)
            {
                var convFailed = GetTryConvertReturnValue(retType);
                //var tmp = MSAst.Variable(convFailed.Type == typeof(object) ? typeof(object) : ret.Type, "tmp");
                return AstUtils.Convert(ret, convFailed.Type == typeof(object) ? typeof(object) : ret.Type);
                //ret = MSAst.Block(
                //        new[] { tmp },
                //        AstUtils.Try(
                //            MSAst.Assign(tmp, AstUtils.Convert(ret, tmp.Type))
                //        ).Catch(
                //            typeof(Exception),
                //            MSAst.Assign(tmp, convFailed)
                //        ),
                //        tmp
                //     );
            }
            return ret;
        }

        /// <summary>
        ///   Helper to produce a rule when no conversion is required (the strong type of the expression
        ///   input matches the type we're converting to or has an implicit conversion at the IL level)
        /// </summary>
        private static MSAst MakeSimpleConversionTarget(Type toType, MSAst arg)
        {
            return AstUtils.Convert(arg, CompilerHelpers.GetVisibleType(toType));
        }

        /// <summary>
        ///   Helper to produce a rule when no conversion is required from an extensible type's
        ///   underlying storage to the type we're converting to.  The type of extensible type
        ///   matches the type we're converting to or has an implicit conversion at the IL level.
        /// </summary>
        private static MSAst MakeSimpleExtensibleConversionTarget(Type toType, MSAst arg)
        {
            var extType = typeof(Extensible<>).MakeGenericType(toType);

            return AstUtils.Convert(
                GetExtensibleValue(extType, arg),
                toType);
        }

        /// <summary>Helper to extract the value from an Extensible of T</summary>
        private static MSAst MakeExtensibleTarget(Type extensibleType, MSAst arg)
        {
            return MSAst.Property(
                MSAst.Convert(arg, extensibleType),
                extensibleType.GetProperty("Value"));
        }

        /// <summary>Helper to convert a null value to nullable of T</summary>
        private static MSAst MakeNullToNullableOfTTarget(Type toType)
        {
            return MSAst.Call(typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(toType));
        }

        /// <summary>Helper to produce the rule for converting T to Nullable of T</summary>
        private static MSAst MakeTToNullableOfTTarget(Type toType, Type knownType, MSAst arg)
        {
            // T -> Nullable<T>
            return MSAst.New(
                toType.GetConstructor(new[] { knownType }),
                AstUtils.Convert(arg, knownType));
        }

        /// <summary>Helper to produce the rule for converting T to Nullable of T</summary>
        private static MSAst MakeConvertingToTToNullableOfTTarget(Type toType, ConversionResultKind kind, MSAst arg)
        {
            var valueType = toType.GetGenericArguments()[0];

            // ConvertSelfToT -> Nullable<T>
            if (kind == ConversionResultKind.ExplicitCast)
            {
                // if the conversion to T fails we just throw
                var conversion = ConvertExpression(arg, valueType);

                return MSAst.New(
                    toType.GetConstructor(new[] { valueType }),
                    conversion);
            }
            else
            {
                var conversion = ConvertExpression(arg, valueType);

                // if the conversion to T succeeds then produce the nullable<T>, otherwise return default(retType)

                return MSAst.Condition(
                    MSAst.NotEqual(
                        conversion,
                        AstUtils.Constant(null)),
                    MSAst.New(
                        toType.GetConstructor(new[] { valueType }),
                        MSAst.Convert(
                            conversion,
                            valueType)),
                    GetTryConvertReturnValue(toType));
            }
        }

        /// <summary>
        ///   Helper to extract the Value of an Extensible of T from the
        ///   expression being converted.
        /// </summary>
        private static MSAst GetExtensibleValue(Type extType, MSAst arg)
        {
            return MSAst.Property(
                AstUtils.Convert(
                    arg,
                    extType
                    ),
                extType.GetProperty("Value")
                );
        }

        /// <summary>
        ///   Helper that checks if fromType is an Extensible of T or a subtype of 
        ///   Extensible of T and if so returns the T.  Otherwise it returns fromType.
        /// 
        ///   This is used to treat extensible types the same as their underlying types.
        /// </summary>
        private static Type GetUnderlyingType(Type fromType)
        {
            var currentType = fromType;

            do
            {
                if (currentType.IsGenericType && (currentType.GetGenericTypeDefinition() == typeof(Extensible<>)))
                    fromType = currentType.GetGenericArguments()[0];
                currentType = currentType.BaseType;
            }
            while (currentType != null);

            return fromType;
        }

        /// <summary>Creates a target which returns null for a reference type.</summary>
        private static MSAst MakeNullTarget(Type toType)
        {
            return AstUtils.Constant(null, toType);
        }

        private static MSAst ConvertExpression(MSAst expr, Type toType)
        {
            if (expr == null)
                throw new ArgumentNullException("expr");
            if (toType == null)
                throw new ArgumentNullException("toType");

            var exprType = expr.Type;

            if (toType == typeof(object))
            {
                if (exprType.IsValueType)
                    return AstUtils.Convert(expr, toType);
                return expr;
            }

            if (toType.IsAssignableFrom(exprType))
            {
                return expr;
            }

            //Type visType = CompilerHelpers.GetVisibleType(toType);

            return MSAst.Convert(expr, toType);
        }

        private MemberGroup GetMember(Type type, string name)
        {
            var foundMembers = type.GetMember(name);
            if (!PrivateBinding)
                foundMembers = CompilerHelpers.FilterNonVisibleMembers(type, foundMembers);

            var members = new MemberGroup(foundMembers);

            // check for generic types w/ arity...
            var types = type.GetNestedTypes(BindingFlags.Public);
            var genName = name + ReflectionUtils.GenericArityDelimiter;
            List<Type> genTypes = null;
            foreach (var t in types)
            {
                if (t.Name.StartsWith(genName))
                {
                    if (genTypes == null)
                        genTypes = new List<Type>();
                    genTypes.Add(t);
                }
            }

            if (genTypes != null)
            {
                var mt = new List<MemberTracker>(members);
                mt.AddRange(genTypes.Select(MemberTracker.FromMemberInfo));
                return new MemberGroup(mt.ToArray());
            }

            if (members.Count == 0)
            {
                members =
                    new MemberGroup(
                        type.GetMember(
                            name,
                            BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static |
                            BindingFlags.Instance));
                if (members.Count == 0)
                {
                    //members = GetAllExtensionMembers(type, name);
                }
            }

            return members;
        }
        #endregion

        #endregion

        #region Extension Types
        public override IList<Type> GetExtensionTypes(Type t)
        {
            var list = new List<Type>(base.GetExtensionTypes(t));

            AddExtensionTypes(t, list);

            return list;
        }

        private void AddExtensionTypes(Type t, List<Type> list)
        {
            lock (_extensionTypes)
            {
                IList<Type> userExtensions;

                if (_extensionTypes.TryGetValue(t, out userExtensions))
                    list.AddRange(userExtensions.Where(o => !list.Contains(o)));

                if (_registeredInterfaceExtensions)
                {
                    foreach (var interfaceType in t.GetInterfaces())
                    {
                        IList<Type> extendingTypes;
                        if (_extensionTypes.TryGetValue(interfaceType, out extendingTypes))
                            list.AddRange(extendingTypes.Where(o => !list.Contains(o)).ToList());
                    }
                }
            }

            if (t.BaseType != null)
                AddExtensionTypes(t.BaseType, list);
        }

        public bool HasExtensionTypes(Type t)
        {
            return _extensionTypes.ContainsKey(t);
        }
        #endregion
    }
}