using System;
using System.ComponentModel;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class ConvertExpression : Expression
    {
        private FullNamedExpression _targetType;
        private Expression _operand;

        public Expression Operand
        {
            get { return _operand; }
        }

        public ConvertExpression() {}

        public ConvertExpression([NotNull] FullNamedExpression targetType, [NotNull] Expression operand) : this(targetType, operand, operand.Span) {}

        public ConvertExpression([NotNull] FullNamedExpression targetType, [NotNull] Expression operand, SourceSpan span)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (operand == null)
                throw new ArgumentNullException("operand");

            _operand = operand;
            _targetType = targetType;

            Span = span;
            Type = _targetType.Type;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ConvertExpression;
            if (clone == null)
                return;

            clone._operand = Clone(cloneContext, _operand);
            clone._targetType = Clone(cloneContext, _targetType);
        }

        [DefaultValue(false)]
        public bool IsImplicitConversionRequired
        {
            get; set;
        }

        public FullNamedExpression TargetType
        {
            get { return _targetType; }
            set
            {
                _targetType = value;
                Type = (_targetType == null) ? null : _targetType.Type;
            }
        }

        [DefaultValue(false)]
        public override bool IsPrimaryExpression
        {
            get { return IsImplicitConversionRequired && _operand.IsPrimaryExpression; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _targetType, prefix, postfix);
            Walk(ref _operand, prefix, postfix);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            if (IsImplicitConversionRequired)
            {
                DumpChild(_operand, sw, indentChange);
            }
            else
            {
                sw.Write("(");
                DumpChild(_targetType, sw, indentChange);
                sw.Write(")");
            }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            var transformedOperator = _operand.Transform(generator);

            if (transformedOperator.Type == _targetType.Type)
                return transformedOperator;

            if (Type == TypeManager.CoreTypes.String)
            {
                return MSAst.Call(
                    transformedOperator,
                    CommonMembers.ObjectToString);
            }

            return generator.ConvertTo(
                Type,
                IsImplicitConversionRequired
                    ? ConversionResultKind.ImplicitCast
                    : ConversionResultKind.ExplicitCast,
                transformedOperator);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _operand = _operand.Resolve(parseContext);
            _targetType = _targetType.ResolveAsTypeStep(parseContext, false);

            Type = _targetType.Type;

            return this;
        }

        public static Expression MakeImplicitConversion(ParseContext ec, Expression source, Type destinationType, SourceSpan location)
        {
            if (source is LambdaExpression)
            {
                if (((LambdaExpression)source).ImplicitStandardConversionExists(ec, destinationType))
                    return source;

                var returnType = destinationType;
                
                if (TypeManager.IsDelegateType(returnType))
                    returnType = returnType.GetMethod("Invoke").ReturnType;

                if (TypeManager.IsEqual(((LambdaExpression)source).Body.Type, returnType))
                    return source;

                return null;
            }
            
            if (TypeManager.IsEqual(source.Type, destinationType))
                return source;

            return new ConvertExpression(TypeExpression.Create(destinationType), source, location)
                   {
                       IsImplicitConversionRequired = true
                   }.Resolve(ec);
        }

/*
        //
        // 6.1.6 Implicit reference conversions
        //
        public static bool ImplicitReferenceConversionExists(Expression expr, Type target_type)
        {
            if (TypeManager.IsStruct(target_type))
                return false;

            var exprType = expr.ExpressionType;

            // from the null type to any reference-type.
            if (exprType == TypeManager.CoreTypes.Null)
                return target_type != typeof(LambdaExpression);

            if (TypeManager.IsGenericParameter(exprType))
                return MakeImplicitTypeParameterConversion(expr, target_type) != null;

            //
            // notice that it is possible to write "ValueType v = 1", the ValueType here
            // is an abstract class, and not really a value type, so we apply the same rules.
            //
            if (target_type == TypeManager.CoreTypes.Object || TypeManager.IsDynamicType(target_type))
            {
                //
                // A pointer type cannot be converted to object
                //
                if (exprType.IsPointer)
                    return false;

                if (TypeManager.IsValueType(exprType))
                    return false;

                if (exprType.IsClass || exprType.IsInterface || exprType == TypeManager.CoreTypes.Enum)
                {
                    // No mcs internal types are convertible
                    return exprType.Module != typeof(Convert).Module;
                }

                return false;
            }
            else if (target_type == TypeManager.CoreTypes.ValueType)
            {
                return exprType == TypeManager.CoreTypes.Enum;
            }
            else if (TypeManager.IsSubclassOf(exprType, target_type))
            {
                //
                // Special case: enumeration to System.Enum.
                // System.Enum is not a value type, it is a class, so we need
                // a boxing conversion
                //
                if (target_type == TypeManager.CoreTypes.Enum || TypeManager.IsGenericParameter(exprType))
                    return false;

                return true;
            }

            // This code is kind of mirrored inside ImplicitStandardConversionExists
            // with the small distinction that we only probe there
            //
            // Always ensure that the code here and there is in sync

            // from any class-type S to any interface-type T.
            if (target_type.IsInterface)
            {
                if (TypeManager.ImplementsInterface(exprType, target_type))
                {
                    return !TypeManager.IsGenericParameter(exprType) &&
                        !TypeManager.IsValueType(exprType);
                }
            }

            if (exprType.IsArray)
            {
                // from an array-type S to an array-type of type T
                if (target_type.IsArray && exprType.GetArrayRank() == target_type.GetArrayRank())
                {

                    //
                    // Both SE and TE are reference-types
                    //
                    var exprElementType = exprType.GetElementType();
                    if (!TypeManager.IsReferenceType(exprElementType))
                        return false;

                    var targetElementType = target_type.GetElementType();
                    if (!TypeManager.IsReferenceType(targetElementType))
                        return false;

                    if (_empty == null)
                        _empty = new EmptyExpression(exprElementType);
                    else
                        _empty.SetType(exprElementType);

                    return ImplicitStandardConversionExists(_empty, targetElementType);
                }

                // from an array-type to System.Array
                if (target_type == TypeManager.array_type)
                    return true;

                // from an array-type of type T to IList<T>
                if (Array_To_IList(exprType, target_type, false))
                    return true;

                return false;
            }

            if (TypeManager.IsVariantOf(exprType, target_type))
                return true;

            // from any interface type S to interface-type T.
            if (exprType.IsInterface && target_type.IsInterface)
            {
                return TypeManager.ImplementsInterface(exprType, target_type);
            }

            // from any delegate type to System.Delegate
            if (target_type == TypeManager.CoreTypes.Delegate &&
                (exprType == TypeManager.CoreTypes.Delegate || TypeManager.IsDelegateType(exprType)))
                return true;

            if (TypeManager.IsEqual(exprType, target_type))
                return true;

            return false;
        }
*/

/*
        static Expression ImplicitReferenceConversion(Expression expr, Type targetType, bool explicitCast)
        {
            var exprType = expr.ExpressionType;

            if (exprType == TypeManager.CoreTypes.Void)
                return null;

            if (TypeManager.IsGenericParameter(exprType))
                return MakeImplicitTypeParameterConversion(expr, targetType);

            //
            // from the null type to any reference-type.
            //
            var nl = expr as NullLiteral;
            if (nl != null)
            {
                return nl.ConvertImplicitly(targetType);
            }

            if (ImplicitReferenceConversionExists(expr, targetType))
            {
                // 
                // Avoid wrapping implicitly convertible reference type
                //
                if (!explicitCast)
                    return expr;

                return EmptyCastExpression.Create(expr, targetType);
            }

            bool useClassCast;
            if (ImplicitBoxingConversionExists(expr, targetType, out useClassCast))
            {
                if (useClassCast)
                    return new CastExpression(expr, targetType);
                else
                    return new BoxedCastExpression(expr, targetType);
            }

            return null;
        }
*/


        public static Expression MakeExplicitConversion(ParseContext ec, Expression source, Type destinationType, SourceSpan location)
        {
            return new ConvertExpression(TypeExpression.Create(destinationType), source, location)
            {
                IsImplicitConversionRequired = false
            }.Resolve(ec);
        }

        public static Expression ImplicitNullableConversion(ParseContext ec, Expression expr, Type targetType)
        {
            Type exprType = expr.Type;

            //
            // From null to any nullable type
            //
            if (exprType == TypeManager.CoreTypes.Null)
                return ec == null ? EmptyExpression.Null : LiftedNullExpression.Create(targetType, expr.Span);

            // S -> T?
            var elementType = targetType.GetGenericArguments()[0];

            // S? -> T?
            if (TypeManager.IsNullableType(exprType))
                exprType = exprType.GetGenericArguments()[0];

            //
            // Predefined implicit identity or implicit numeric conversion
            // has to exist between underlying type S and underlying type T
            //

            // Handles probing
            if (ec == null)
            {
                if (exprType == elementType)
                    return EmptyExpression.Null;

                return MakeImplicitNumericConversion(ec, null, exprType, elementType);
            }

            Expression unwrap;
            if (exprType != expr.Type)
                unwrap = Unwrap.Create(expr);
            else
                unwrap = expr;

            Expression conv = exprType == elementType ? unwrap : MakeImplicitNumericConversion(ec, unwrap, exprType, elementType);
            if (conv == null)
                return null;

            if (exprType != expr.Type)
                return new LiftedExpression(conv, unwrap, targetType).Resolve(ec);

            // Do constant optimization for S -> T?
            if (unwrap is ConstantExpression)
                conv = ((ConstantExpression)unwrap).ConvertImplicitly(elementType);

            return Wrap.Create(conv, targetType);
        }

        internal static Expression MakeImplicitNumericConversion(ParseContext ec, Expression value, Type sourceType, Type targetType)
        {
            if (!TypeUtils.IsImplicitNumericConversion(sourceType, targetType))
                return null;
            if (TypeManager.IsEqual(sourceType, targetType))
                return value;
            return MakeImplicitConversion(ec, value, targetType, value.Span);
        }

        /*private static Expression MakeImplicitTypeParameterConversion(Expression expr, Type targetType)
        {
            var exprType = expr.ExpressionType;

            var gc = TypeManager.GetTypeParameterConstraints(exprType);

            if (gc == null)
            {
                if (targetType == TypeManager.CoreTypes.Object)
                    return new CastExpression(expr, targetType);

                return null;
            }

            // We're converting from a type parameter which is known to be a reference type.
            var baseType = GetTypeParameterEffectiveBaseType(gc);

            if (TypeManager.IsSubclassOf(baseType, targetType))
                return new CastExpression(expr, targetType);

            if (targetType.IsInterface)
            {
                if (TypeManager.ImplementsInterface(baseType, targetType))
                    return new CastExpression(expr, targetType);

                foreach (Type t in gc.InterfaceConstraints)
                {
                    if (TypeManager.IsSubclassOf(t, targetType))
                        return new CastExpression(expr, targetType);
                    if (TypeManager.ImplementsInterface(t, targetType))
                        return new CastExpression(expr, targetType);
                }
            }

            foreach (Type t in gc.InterfaceConstraints)
            {
                if (!TypeManager.IsGenericParameter(t))
                    continue;
                if (TypeManager.IsSubclassOf(t, targetType))
                    return new CastExpression(expr, targetType);
                if (TypeManager.ImplementsInterface(t, targetType))
                    return new CastExpression(expr, targetType);
            }

            return null;
        }

        static Type GetTypeParameterEffectiveBaseType(GenericConstraints gc)
        {
            var list = new List<Type> { gc.EffectiveBaseClass };
            list.AddRange(from t in gc.InterfaceConstraints
                          where TypeManager.IsGenericParameter(t)
                          select TypeManager.GetTypeParameterConstraints(t)
                          into newGc where newGc != null select GetTypeParameterEffectiveBaseType(newGc));
            return FindMostEncompassedType(list);
        }

        /// <summary>
        ///  Finds "most encompassed type" according to the spec (13.4.2)
        ///  amongst the methods in the MethodGroupExpr
        /// </summary>
        private static Type FindMostEncompassedType(IList<Type> types)
        {
            Type best = null;

            if (types.Count == 0)
                return null;

            if (types.Count == 1)
                return types[0];

            var expr = EmptyExpression.Grab();

            foreach (var t in types)
            {
                if (best == null)
                {
                    best = t;
                    continue;
                }

                expr.SetType(t);
                //if (ImplicitStandardConversionExists(expr, best))
                if (TypeUtils.IsImplicitlyConvertible(expr.ExpressionType, best))
                    best = t;
            }

            expr.SetType(best);
            
            var bestCopy = best;
            if (types.Where(t => bestCopy != t).Any(t => !TypeUtils.IsImplicitlyConvertible(expr.ExpressionType, t)))
                best = null;

            EmptyExpression.Release(expr);

            return best;
        }

        static bool Array_To_IList(Type array, Type list, bool isExplicit)
        {
            if ((array.GetArrayRank() != 1) || !TypeManager.IsGenericType(list))
                return false;

            var gt = TypeManager.DropGenericTypeArguments(list);
            if ((gt != TypeManager.CoreTypes.GenericListInterface) &&
                (gt != TypeManager.CoreTypes.GenericCollectionInterface) &&
                (gt != TypeManager.CoreTypes.GenericEnumerableInterface))
            {
                return false;
            }

            var elementType = array.GetElementType();
            var argType = list.GetGenericArguments()[0];

            if (elementType == argType)
                return true;

            if (isExplicit)
                return ExplicitReferenceConversionExists(elementType, argType);

            Type t = TypeManager.GetElementType(array);
            if (MyEmptyExpr == null)
                MyEmptyExpr = new EmptyExpression(t);
            else
                MyEmptyExpr.SetType(t);

            return ImplicitReferenceConversionExists(MyEmptyExpr, argType);
        }

        static bool IList_To_Array(Type list, Type array)
        {
            if (!TypeManager.IsGenericType(list) || !array.IsArray || array.GetArrayRank() != 1)
                return false;

            Type gt = TypeManager.DropGenericTypeArguments(list);
            if (gt != TypeManager.generic_ilist_type &&
                gt != TypeManager.generic_icollection_type &&
                gt != TypeManager.generic_ienumerable_type)
                return false;

            Type arg_type = TypeManager.TypeToCoreType(TypeManager.GetTypeArguments(list)[0]);
            Type element_type = TypeManager.GetElementType(array);

            if (element_type == arg_type)
                return true;

            if (MyEmptyExpr == null)
                MyEmptyExpr = new EmptyExpression(element_type);
            else
                MyEmptyExpr.SetType(element_type);

            return ImplicitReferenceConversionExists(MyEmptyExpr, arg_type) || ExplicitReferenceConversionExists(element_type, arg_type);
        }

        /// <summary>
        ///   Implements Explicit Reference conversions
        /// </summary>
        static Expression ExplicitReferenceConversion(Expression source, Type source_type, Type target_type)
        {
            bool target_is_value_type = TypeManager.IsStruct(target_type);

            //
            // From object to a generic parameter
            //
            if (source_type == TypeManager.object_type && TypeManager.IsGenericParameter(target_type))
                return source == null ? EmptyExpression.Null : new UnboxCast(source, target_type);

            //
            // Explicit type parameter conversion.
            //
            if (TypeManager.IsGenericParameter(source_type))
                return ExplicitTypeParameterConversion(source, source_type, target_type);

            //
            // From object to any reference type or value type (unboxing)
            //
            if (source_type == TypeManager.object_type)
                return source == null ? EmptyExpression.Null :
                    target_is_value_type ? (Expression)new UnboxCast(source, target_type) : new ClassCast(source, target_type);

            //
            // Unboxing conversion from the types object and System.ValueType to any non-nullable-value-type
            //
            if (source_type == TypeManager.value_type && target_is_value_type)
                return source == null ? EmptyExpression.Null : new UnboxCast(source, target_type);

            //
            // From any class S to any class-type T, provided S is a base class of T
            //
            if (TypeManager.IsSubclassOf(target_type, source_type))
                return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

            //
            // From any class type S to any interface T, provides S is not sealed
            // and provided S does not implement T.
            //
            if (target_type.IsInterface && !source_type.IsSealed &&
                !TypeManager.ImplementsInterface(source_type, target_type))
            {
                return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);
            }

            //
            // From any interface-type S to to any class type T, provided T is not
            // sealed, or provided T implements S.
            //
            if (source_type.IsInterface)
            {
                if (!target_type.IsSealed || TypeManager.ImplementsInterface(target_type, source_type))
                {
                    if (target_type.IsClass)
                        return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

                    //
                    // Unboxing conversion from any interface-type to any non-nullable-value-type that
                    // implements the interface-type
                    //
                    return source == null ? EmptyExpression.Null : new UnboxCast(source, target_type);
                }

                //
                // From System.Collecitons.Generic.IList<T> and its base interfaces to a one-dimensional
                // array type S[], provided there is an implicit or explicit reference conversion from S to T.
                //
                if (IList_To_Array(source_type, target_type))
                    return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

                return null;
            }

            if (source_type.IsArray)
            {
                if (target_type.IsArray)
                {
                    //
                    // From System.Array to any array-type
                    //
                    if (source_type == TypeManager.array_type)
                        return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

                    //
                    // From an array type S with an element type Se to an array type T with an
                    // element type Te provided all the following are true:
                    //     * S and T differe only in element type, in other words, S and T
                    //       have the same number of dimensions.
                    //     * Both Se and Te are reference types
                    //     * An explicit reference conversions exist from Se to Te
                    //
                    if (source_type.GetArrayRank() == target_type.GetArrayRank())
                    {

                        source_type = TypeManager.GetElementType(source_type);
                        if (!TypeManager.IsReferenceType(source_type))
                            return null;

                        Type target_type_element = TypeManager.GetElementType(target_type);
                        if (!TypeManager.IsReferenceType(target_type_element))
                            return null;

                        if (ExplicitReferenceConversionExists(source_type, target_type_element))
                            return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

                        return null;
                    }
                }

                //
                // From a single-dimensional array type S[] to System.Collections.Generic.IList<T> and its base interfaces, 
                // provided that there is an explicit reference conversion from S to T
                //
                if (Array_To_IList(source_type, target_type, true))
                    return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

                return null;
            }

            //
            // From System delegate to any delegate-type
            //
            if (source_type == TypeManager.delegate_type && TypeManager.IsDelegateType(target_type))
                return source == null ? EmptyExpression.Null : new ClassCast(source, target_type);

            return null;
        }
*/

        public static bool ImplicitConversionExists(ParseContext ec, Expression source, Type targetType)
        {
            var sourceLambda = source as LambdaExpression;
            if (sourceLambda != null)
                return sourceLambda.ImplicitStandardConversionExists(ec, targetType);

            return TypeUtils.IsImplicitlyConvertible(source.Type, targetType, true);
        }
    }
}