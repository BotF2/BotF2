using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

using System.Linq;

namespace Supremacy.Scripting.Ast
{
    public class BinaryExpression : Expression
    {
        private static PredefinedOperator[] _standardOperators;

        private Expression _left;
        private Type _resultType;
        private Expression _right;

        public MSAst.ExpressionType Operator { get; set; }

        public Expression Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public Expression Right
        {
            get { return _right; }
            set { _right = value; }
        }

        protected Type ResultType
        {
            get { return _resultType; }
            set
            {
                _resultType = value;
                Type = _resultType;
            }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _left, prefix, postfix);
            Walk(ref _right, prefix, postfix);
        }

        /*
                public override Expression DoResolve(ParseContext parseContext)
                {
                    if ((this.Left == null) || (this.Right == null))
                        return null;

                    this.Left = this.Left.Resolve(parseContext);
                    this.Right = this.Right.Resolve(parseContext);

                    if ((this.Left == null) || (this.Right == null))
                        return null;

                    var leftType = this.Left.ExpressionType;
                    var rightType = this.Right.ExpressionType;

                    if ((leftType == TypeManager.CoreTypes.Decimal)

                    if (TypeUtils.IsNumeric(leftType) && TypeUtils.IsNumeric(rightType))
                    {
                        int leftX, leftY, rightX, rightY;

                        var conversionOrder = TypeUtils.GetNumericConversionOrder(
                            Type.GetTypeCode(leftType),
                            out leftX,
                            out leftY);
                    }

                    return base.DoResolve(parseContext);
                }
        */

        public override Expression DoResolve(ParseContext ec)
        {
            if (_left == null)
                return null;

            _left = _left.Resolve(ec);

            if (_left == null)
                return null;

            if ((Operator == MSAst.ExpressionType.TypeAs) || (Operator == MSAst.ExpressionType.TypeIs))
            {
                _right = _right.Resolve(ec);
                return this;
            }

            var lc = _left as ConstantExpression;

            if (lc != null && lc.Type == TypeManager.CoreTypes.Boolean &&
                ((Operator == MSAst.ExpressionType.AndAlso && lc.IsDefaultValue) ||
                 (Operator == MSAst.ExpressionType.OrElse && !lc.IsDefaultValue)))
            {

                // FIXME: resolve right expression as unreachable
                // right.Resolve (ec);

                ec.ReportError(
                    429,
                    "Unreachable expression code detected.",
                    Severity.Warning,
                    Span);

                return _left;
            }

            _right = _right.Resolve(ec);

            if (_right == null)
                return null;

            ExpressionClass = ExpressionClass.Value;

            var rc = _right as ConstantExpression;

            // The conversion rules are ignored in enum context but why
            if (lc != null && rc != null && (TypeManager.IsEnumType(_left.Type) || TypeManager.IsEnumType(_right.Type)))
            {
                lc = EnumLiftUp(ec, lc, rc, Span);
                if (lc != null)
                    rc = EnumLiftUp(ec, rc, lc, Span);
            }

            if (rc != null && lc != null)
            {
                var previousErrorCount = ec.CompilerErrorCount;
                var e = ConstantFold.BinaryFold(
                    ec,
                    Operator,
                    lc,
                    rc,
                    Span);
                if (e != null || ec.CompilerErrorCount != previousErrorCount)
                    return e;
            }

            // Comparison warnings
            if (Operator.IsComparison())
            {
                if (_left.Equals(_right))
                {
                    ec.ReportError(
                        1718,
                        "A comparison made to same variable. Did you mean to compare something else?",
                        Severity.Warning,
                        Span);
                }
                CheckUselessComparison(ec, lc, _right.Type);
                CheckUselessComparison(ec, rc, _left.Type);
            }

            /*
                        if (TypeManager.IsDynamicType(_left.ExpressionType) || TypeManager.IsDynamicType(_right.ExpressionType))
                        {
                            var args = new Arguments(2);
                            args.Add(new Argument(_left));
                            args.Add(new Argument(_right));
                            return new DynamicExpressionStatement(this, args, this.Span).Resolve(ec);
                        }
            */

            if ((TypeManager.IsNullableType(_left.Type) && (_right is NullLiteral || TypeManager.IsNullableType(_right.Type) || TypeManager.IsValueType(_right.Type))) ||
                 (TypeManager.IsValueType(_left.Type) && _right is NullLiteral) ||
                 (TypeManager.IsNullableType(_right.Type) && (_left is NullLiteral || TypeManager.IsNullableType(_left.Type) || TypeManager.IsValueType(_left.Type))) ||
                 (TypeManager.IsValueType(_right.Type) && _left is NullLiteral))
            {
                return new LiftedBinaryOperator(Operator, _left, _right, Span).Resolve(ec);
            }

            return DoResolveCore(ec, _left, _right);
        }

        private void CheckUselessComparison(ParseContext ec, ConstantExpression c, Type type)
        {
            if (c == null ||
                !TypeUtils.IsIntegralType(type)
                || c is ConstantExpression<string>
                || c is ConstantExpression<bool>
                || c is ConstantExpression<float>
                || c is ConstantExpression<double>
                || c is ConstantExpression<decimal>)
            {
                return;
            }

            long value = 0;

            if (c is ConstantExpression<ulong>)
            {
                ulong uvalue = ((ConstantExpression<ulong>)c).Value;
                if (uvalue > long.MaxValue)
                {
                    if (type == TypeManager.CoreTypes.Byte ||
                        type == TypeManager.CoreTypes.SByte ||
                        type == TypeManager.CoreTypes.Int16 ||
                        type == TypeManager.CoreTypes.UInt16 ||
                        type == TypeManager.CoreTypes.Int32 ||
                        type == TypeManager.CoreTypes.UInt32 ||
                        type == TypeManager.CoreTypes.Int64 ||
                        type == TypeManager.CoreTypes.Char)
                    {
                        WarnUselessComparison(ec, type);
                    }
                    return;
                }
                value = (long)uvalue;
            }
            else if (c is ConstantExpression<byte>)
                value = ((ConstantExpression<byte>)c).Value;
            else if (c is ConstantExpression<sbyte>)
                value = ((ConstantExpression<sbyte>)c).Value;
            else if (c is ConstantExpression<short>)
                value = ((ConstantExpression<short>)c).Value;
            else if (c is ConstantExpression<ushort>)
                value = ((ConstantExpression<ushort>)c).Value;
            else if (c is ConstantExpression<int>)
                value = ((ConstantExpression<int>)c).Value;
            else if (c is ConstantExpression<uint>)
                value = ((ConstantExpression<uint>)c).Value;
            else if (c is ConstantExpression<long>)
                value = ((ConstantExpression<long>)c).Value;
            else if (c is ConstantExpression<char>)
                value = ((ConstantExpression<char>)c).Value;

            if (value == 0)
                return;

            if (IsValueOutOfRange(value, type))
                WarnUselessComparison(ec, type);
        }

        private void WarnUselessComparison(ParseContext ec, Type type)
        {
            ec.ReportError(
                652,
                string.Format(
                    "A comparison between a constant and a variable is useless.  The constant is out of the range of the variable type '{0}'.",
                    TypeManager.GetCSharpName(type)),
                Severity.Warning,
                Span);
        }

        private static bool IsValueOutOfRange(long value, Type type)
        {
            if (TypeUtils.IsUnsignedInt(type) && value < 0)
                return true;

            return type == TypeManager.CoreTypes.SByte && (value >= 0x80 || value < -0x80) ||
                   type == TypeManager.CoreTypes.Byte && value >= 0x100 ||
                   type == TypeManager.CoreTypes.Int16 && (value >= 0x8000 || value < -0x8000) ||
                   type == TypeManager.CoreTypes.UInt16 && value >= 0x10000 ||
                   type == TypeManager.CoreTypes.Int32 && (value >= 0x80000000 || value < -0x80000000) ||
                   type == TypeManager.CoreTypes.UInt32 && value >= 0x100000000;
        }

        protected Expression DoResolveCore(ParseContext ec, Expression leftOrig, Expression rightOrig)
        {
            var expr = ResolveOperator(ec);
            if (expr == null)
                OnErrorOperatorCannotBeApplied(ec, leftOrig, rightOrig, Operator, Span);

            if (Left == null || Right == null)
                throw new InternalErrorException("Invalid conversion");

            if (Operator == MSAst.ExpressionType.Or)
                CheckBitwiseOrOnSignExtended(ec);

            return expr;
        }

        private void CheckBitwiseOrOnSignExtended(ParseContext ec)
        {
            var leftCast = Left as CastExpression;
            if (leftCast != null)
            {
                if (IsUnsigned(leftCast.UnderlyingType))
                    leftCast = null;
            }

            var rightCast = Right as CastExpression;
            if (rightCast != null)
            {
                if (IsUnsigned(rightCast.UnderlyingType))
                    rightCast = null;
            }

            if (leftCast == null && rightCast == null)
                return;

            ec.ReportError(
                675,
                string.Format(
                    "The operator '|' used on the sign-extended type '{0}'.  Consider casting to a smaller unsigned type first.",
                    TypeManager.GetCSharpName(leftCast != null ? leftCast.UnderlyingType : rightCast.UnderlyingType)),
                Severity.Warning,
                Span);
        }

        private static bool IsUnsigned(Type t)
        {
            if (t == null)
                return false;

            if (t.IsPointer)
                return true;

            return (t == TypeManager.CoreTypes.UInt32 ||
                    t == TypeManager.CoreTypes.UInt64 ||
                    t == TypeManager.CoreTypes.UInt16 ||
                    t == TypeManager.CoreTypes.Byte);
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            switch (Operator)
            {
                case MSAst.ExpressionType.Add:
                    {
                        return MSAst.Expression.Add(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.And:
                    {
                        return MSAst.Expression.And(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.AndAlso:
                    {
                        return MSAst.Expression.AndAlso(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Coalesce:
                    {
                        return MSAst.Expression.Coalesce(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Divide:
                    {
                        return MSAst.Expression.Divide(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Equal:
                    {
                        return MSAst.Expression.Equal(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.ExclusiveOr:
                    {
                        return MSAst.Expression.ExclusiveOr(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.GreaterThan:
                    {
                        return MSAst.Expression.GreaterThan(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.GreaterThanOrEqual:
                    {
                        return MSAst.Expression.GreaterThanOrEqual(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.LeftShift:
                    {
                        return MSAst.Expression.LeftShift(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.LessThan:
                    {
                        return MSAst.Expression.LessThan(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.LessThanOrEqual:
                    {
                        return MSAst.Expression.LessThanOrEqual(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Modulo:
                    {
                        return MSAst.Expression.Modulo(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Multiply:
                    {
                        return MSAst.Expression.Multiply(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.NotEqual:
                    {
                        return MSAst.Expression.NotEqual(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Or:
                    {
                        return MSAst.Expression.Or(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.OrElse:
                    {
                        return MSAst.Expression.OrElse(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.RightShift:
                    {
                        return MSAst.Expression.RightShift(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.Subtract:
                    {
                        return MSAst.Expression.Subtract(
                            Left.Transform(generator),
                            Right.Transform(generator));
                    }
                case MSAst.ExpressionType.TypeAs:
                    {
                        return MSAst.Expression.TypeAs(
                            Left.Transform(generator),
                            Right.Type);
                    }
                case MSAst.ExpressionType.TypeIs:
                    {
                        return MSAst.Expression.TypeIs(
                            Left.Transform(generator),
                            Right.Type);
                    }
            }

            return generator.Operator(
                Operator,
                Left.Transform(generator),
                Right.Transform(generator));
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as BinaryExpression;
            if (clone == null)
                return;

            clone._left = Clone(cloneContext, _left);
            clone._right = Clone(cloneContext, _right);
            clone._resultType = _resultType;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var operatorToken = GetOperatorToken(Operator) ??
                                Operator.ToString();

            //var parenthesizeLeft = !this.Left.IsPrimaryExpression;
            //var parenthesizeRight = !this.Right.IsPrimaryExpression;

            //if (parenthesizeLeft)
            //    sb.Append("(");

            var leftEnd = Left.Span.End;
            var rightStart = Right.Span.Start;

            Left.Dump(sw, indentChange);

            //if (parenthesizeLeft)
            //    sb.Append(")");

            sw.Write(" ");
            sw.Write(operatorToken);

            if (rightStart.Line != leftEnd.Line)
            {
                sw.WriteLine();
            }
            else
            {
                sw.Write(" ");
            }

            //if (parenthesizeRight)
            //    sb.Append("(");

            Right.Dump(sw, indentChange);

            //if (parenthesizeRight)
            //    sb.Append(")");
        }

        private static string GetOperatorToken(MSAst.ExpressionType @operator)
        {
            string operatorToken;
            switch (@operator)
            {
                case System.Linq.Expressions.ExpressionType.Add:
                    operatorToken = "+";
                    break;
                case System.Linq.Expressions.ExpressionType.And:
                    operatorToken = "&";
                    break;
                case System.Linq.Expressions.ExpressionType.AndAlso:
                    operatorToken = "&&";
                    break;
                case System.Linq.Expressions.ExpressionType.Coalesce:
                    operatorToken = "??";
                    break;
                case System.Linq.Expressions.ExpressionType.Divide:
                    operatorToken = "/";
                    break;
                case System.Linq.Expressions.ExpressionType.Equal:
                    operatorToken = "==";
                    break;
                case System.Linq.Expressions.ExpressionType.ExclusiveOr:
                    operatorToken = "^";
                    break;
                case System.Linq.Expressions.ExpressionType.GreaterThan:
                    operatorToken = ">";
                    break;
                case System.Linq.Expressions.ExpressionType.GreaterThanOrEqual:
                    operatorToken = ">=";
                    break;
                case System.Linq.Expressions.ExpressionType.LeftShift:
                    operatorToken = "<<";
                    break;
                case System.Linq.Expressions.ExpressionType.LessThan:
                    operatorToken = "<";
                    break;
                case System.Linq.Expressions.ExpressionType.LessThanOrEqual:
                    operatorToken = "<=";
                    break;
                case System.Linq.Expressions.ExpressionType.Modulo:
                    operatorToken = "%";
                    break;
                case System.Linq.Expressions.ExpressionType.Multiply:
                    operatorToken = "*";
                    break;
                case System.Linq.Expressions.ExpressionType.NotEqual:
                    operatorToken = "!=";
                    break;
                case System.Linq.Expressions.ExpressionType.Or:
                    operatorToken = "|";
                    break;
                case System.Linq.Expressions.ExpressionType.OrElse:
                    operatorToken = "||";
                    break;
                case System.Linq.Expressions.ExpressionType.RightShift:
                    operatorToken = ">>";
                    break;
                case System.Linq.Expressions.ExpressionType.Subtract:
                    operatorToken = "-";
                    break;
                case System.Linq.Expressions.ExpressionType.TypeAs:
                    operatorToken = "as";
                    break;
                case System.Linq.Expressions.ExpressionType.TypeIs:
                    operatorToken = "is";
                    break;
                default:
                    operatorToken = OperatorInfo.GetOperatorInfo(@operator).Name;
                    break;
            }
            return operatorToken;
        }

        public static void OnErrorOperatorCannotBeApplied(ParseContext parseContext, Expression left, Expression right, MSAst.ExpressionType expressionType, SourceSpan location)
        {
            OnErrorOperatorCannotBeApplied(
                parseContext,
                left,
                right,
                GetOperatorToken(expressionType),
                location);
        }


        public static void OnErrorOperatorCannotBeApplied(ParseContext parseContext, Expression left, Expression right, string oper, SourceSpan location)
        {
            parseContext.ReportError(
                19,
                string.Format(
                    "Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'.",
                    oper,
                    TypeManager.GetCSharpName(left.Type),
                    TypeManager.GetCSharpName(right.Type)),
                location);
        }

        //
        // 7.2.6.2 Binary numeric promotions
        //
        public bool DoBinaryOperatorPromotion(ParseContext ec)
        {
            Type ltype = _left.Type;
            Type rtype = _right.Type;

            Expression temp;

            foreach (Type t in ConstantFold.BinaryPromotions)
            {
                if (t == ltype)
                    return t == rtype || DoNumericPromotion(ec, ref _right, ref _left, t);

                if (t == rtype)
                    return t == ltype || DoNumericPromotion(ec, ref _left, ref _right, t);
            }

            Type int32 = TypeManager.CoreTypes.Int32;
            if (ltype != int32)
            {
                var c = _left as ConstantExpression;
                if (c != null)
                    temp = c.ConvertImplicitly(int32);
                else
                    temp = ConvertExpression.MakeImplicitNumericConversion(ec, _left, _left.Type, int32);

                if (temp == null)
                    return false;
                _left = temp;
            }

            if (rtype != int32)
            {
                var c = _right as ConstantExpression;
                if (c != null)
                    temp = c.ConvertImplicitly(int32);
                else
                    temp = ConvertExpression.MakeImplicitNumericConversion(ec, _right, _right.Type, int32);

                if (temp == null)
                    return false;
                _right = temp;
            }

            return true;
        }

        static bool DoNumericPromotion(ParseContext ec, ref Expression primaryExpression, ref Expression secondaryExpression, Type type)
        {
            Expression temp;

            var c = primaryExpression as ConstantExpression;
            if (c != null)
            {
                temp = c.ConvertImplicitly(type);
                if (temp != null)
                {
                    primaryExpression = temp;
                    return true;
                }
            }

            if (type == TypeManager.CoreTypes.UInt32)
            {
                var expressionType = primaryExpression.Type;
                if (expressionType == TypeManager.CoreTypes.Int32 ||
                    expressionType == TypeManager.CoreTypes.Int16 ||
                    expressionType == TypeManager.CoreTypes.SByte)
                {
                    type = TypeManager.CoreTypes.Int64;

                    if (type != secondaryExpression.Type)
                    {
                        c = secondaryExpression as ConstantExpression;
                        if (c != null)
                            temp = c.ConvertImplicitly(type);
                        else
                            temp = ConvertExpression.MakeImplicitNumericConversion(ec, secondaryExpression, secondaryExpression.Type, type);
                        if (temp == null)
                            return false;
                        secondaryExpression = temp;
                    }
                }
            }
            else if (type == TypeManager.CoreTypes.UInt64)
            {
                //
                // A compile-time error occurs if the other operand is of type sbyte, short, int, or long
                //
                if (type == TypeManager.CoreTypes.Int32 || type == TypeManager.CoreTypes.Int64 ||
                    type == TypeManager.CoreTypes.Int16 || type == TypeManager.CoreTypes.SByte)
                    return false;
            }

            temp = ConvertExpression.MakeImplicitNumericConversion(ec, primaryExpression, primaryExpression.Type, type);
            if (temp == null)
                return false;

            primaryExpression = temp;
            return true;
        }

        private Expression ResolveOperator(ParseContext ec)
        {
            var leftType = _left.Type;
            var rightType = _right.Type;
            var primitivesOnly = false;

            if (_standardOperators == null)
                CreateStandardOperatorsTable();

            //
            // Handles predefined primitive types
            //
            if (TypeManager.IsPrimitiveType(leftType) && TypeManager.IsPrimitiveType(rightType))
            {
                if ((Operator != MSAst.ExpressionType.LeftShift) && (Operator != MSAst.ExpressionType.RightShift))
                {
                    if (leftType != TypeManager.CoreTypes.Boolean && !DoBinaryOperatorPromotion(ec))
                        return null;

                    primitivesOnly = true;
                }
            }
            else
            {
                // Enums
                var leftIsEnum = TypeManager.IsEnumType(leftType);
                var rightIsEnum = TypeManager.IsEnumType(rightType);

                Expression expr;
                if (leftIsEnum || rightIsEnum)
                {
                    expr = ResolveOperatorEnum(ec, leftIsEnum, rightIsEnum, leftType, rightType);

                    // TODO: Can this be ambiguous
                    if (expr != null)
                        return expr;
                }

                // User operators
                expr = ResolveUserOperator(ec, leftType, rightType);
                if (expr != null)
                    return expr;

                // Predefined reference types equality
                if (Operator.IsEquality())
                {
                    expr = ResolveOperatorEqualityRerefence(ec, leftType, rightType);
                    if (expr != null)
                        return expr;
                }
            }

            return ResolveOperatorPredefined(ec, _standardOperators, primitivesOnly, null);
        }

        //
        // 7.9.6 Reference type equality operators
        //
        private BinaryExpression ResolveOperatorEqualityRerefence(ParseContext ec, Type leftType, Type rightType)
        {
            //
            // operator != (object a, object b)
            // operator == (object a, object b)
            //

            if (Left.ExpressionClass == ExpressionClass.MethodGroup || Right.ExpressionClass == ExpressionClass.MethodGroup)
                return null;

            Type = TypeManager.CoreTypes.Boolean;

            var leftIsGenericParameter = TypeManager.IsGenericParameter(leftType);

            if (TypeManager.IsEqual(leftType, rightType))
            {
                if (leftIsGenericParameter)
                {
                    //
                    // Only allow to compare same reference type parameter
                    //
                    if (TypeManager.IsReferenceType(leftType))
                    {
                        Left = new BoxedCastExpression(Left, TypeManager.CoreTypes.Object);
                        Right = new BoxedCastExpression(Right, TypeManager.CoreTypes.Object);
                        return this;
                    }

                    return null;
                }

                if (Left is LambdaExpression)
                    return null;

                if (TypeManager.IsValueType(leftType))
                    return null;

                return this;
            }

            var rightIsGenericParameter = TypeManager.IsGenericParameter(rightType);

            //
            // a, Both operands are reference-type values or the value null
            // b, One operand is a value of type T where T is a type-parameter and
            // the other operand is the value null. Furthermore T does not have the
            // value type constrain
            //
            if (Left is NullLiteral || Right is NullLiteral)
            {
                GenericConstraints constraints;
                if (leftIsGenericParameter)
                {
                    constraints = TypeManager.GetTypeParameterConstraints(leftType);
                    if (constraints != null && constraints.HasValueTypeConstraint)
                        return null;

                    Left = new BoxedCastExpression(Left, TypeManager.CoreTypes.Object);
                    return this;
                }

                if (rightIsGenericParameter)
                {
                    constraints = TypeManager.GetTypeParameterConstraints(rightType);
                    if (constraints != null && constraints.HasValueTypeConstraint)
                        return null;

                    Right = new BoxedCastExpression(Right, TypeManager.CoreTypes.Object);
                    return this;
                }
            }

            //
            // An interface is converted to the object before the
            // standard conversion is applied. It's not clear from the
            // standard but it looks like it works like that.
            //
            if (leftIsGenericParameter)
            {
                if (!TypeManager.IsReferenceType(leftType))
                    return null;

                leftType = TypeManager.CoreTypes.Object;
                Left = new BoxedCastExpression(Left, leftType);
            }
            else if (leftType.IsInterface)
            {
                leftType = TypeManager.CoreTypes.Object;
            }
            else if (TypeManager.IsStruct(leftType))
            {
                return null;
            }

            if (rightIsGenericParameter)
            {
                if (!TypeManager.IsReferenceType(rightType))
                    return null;

                rightType = TypeManager.CoreTypes.Object;
                Right = new BoxedCastExpression(Right, rightType);
            }
            else if (rightType.IsInterface)
            {
                rightType = TypeManager.CoreTypes.Object;
            }
            else if (TypeManager.IsStruct(rightType))
            {
                return null;
            }


            const string refComparison = "Possible unintended reference comparison.  " +
                                         "Consider casting the {0} side of the expression to 'string' to compare the values.";

            //
            // A standard implicit conversion exists from the type of either
            // operand to the type of the other operand
            //
            if (TypeUtils.IsImplicitlyConvertible(Left.Type, rightType) &&
                !TypeUtils.IsImplicitNumericConversion(Left.Type, rightType))
            {
                if (leftType == TypeManager.CoreTypes.String)
                {
                    ec.ReportError(
                        253,
                        string.Format(refComparison, "right"),
                        Severity.Warning,
                        Span);
                }

                return this;
            }

            if (TypeUtils.IsImplicitlyConvertible(Right.Type, leftType) &&
                !TypeUtils.IsImplicitNumericConversion(Right.Type, leftType))
            {
                if (rightType == TypeManager.CoreTypes.String)
                {
                    ec.ReportError(
                        252,
                        string.Format(refComparison, "left"),
                        Severity.Warning,
                        Span);
                }

                return this;
            }

            return null;
        }

        private Expression ResolveOperatorEnum(ParseContext ec, bool leftIsEnum, bool rightIsEnum, Type leftType, Type rightType)
        {
            //
            // bool operator == (E x, E y);
            // bool operator != (E x, E y);
            // bool operator < (E x, E y);
            // bool operator > (E x, E y);
            // bool operator <= (E x, E y);
            // bool operator >= (E x, E y);
            //
            // E operator & (E x, E y);
            // E operator | (E x, E y);
            // E operator ^ (E x, E y);
            //
            // U operator - (E e, E f)
            // E operator - (E e, U x)
            //
            // E operator + (U x, E e)
            // E operator + (E e, U x)
            //
            if (!((Operator.IsComparison() || Operator.IsBitwise()) ||
                  (Operator == MSAst.ExpressionType.Subtract && leftIsEnum) ||
                  (Operator == MSAst.ExpressionType.Add && (leftIsEnum != rightIsEnum || Type != null))))	// type != null for lifted null
                return null;

            var leftTemp = Left;
            var rightTemp = Right;

            Type underlyingType;
            Expression expr;

            if (Operator.IsComparison() || Operator.IsBitwise())
            {
                if (rightIsEnum)
                {
                    expr = ConvertExpression.MakeImplicitConversion(ec, Left, rightType, Span);
                    if (expr != null)
                    {
                        Left = expr;
                        leftType = expr.Type;
                    }
                }
                else if (leftIsEnum)
                {
                    expr = ConvertExpression.MakeImplicitConversion(ec, Right, leftType, Span);
                    if (expr != null)
                    {
                        Right = expr;
                        rightType = expr.Type;
                    }
                }
            }

            if (TypeManager.IsEqual(leftType, rightType))
            {
                underlyingType = Enum.GetUnderlyingType(leftType);

                if (Left is ConstantExpression)
                    Left = ((ConstantExpression)Left).ConvertExplicitly(false, underlyingType);
                else
                    Left = EmptyCastExpression.Create(Left, underlyingType);

                if (Right is ConstantExpression)
                    Right = ((ConstantExpression)Right).ConvertExplicitly(false, underlyingType);
                else
                    Right = EmptyCastExpression.Create(Right, underlyingType);
            }
            else if (leftIsEnum)
            {
                underlyingType = Enum.GetUnderlyingType(leftType);

                if (Operator != MSAst.ExpressionType.Subtract && Operator != MSAst.ExpressionType.Add)
                {
                    var c = Right as ConstantExpression;
                    if (c == null || !c.IsDefaultValue)
                        return null;
                }
                else
                {
                    if (!TypeUtils.IsImplicitlyConvertible(Right.Type, underlyingType))
                        return null;

                    Right = ConvertExpression.MakeImplicitConversion(ec, Right, underlyingType, Right.Span);
                }

                if (Left is ConstantExpression)
                    Left = ((ConstantExpression)Left).ConvertExplicitly(false, underlyingType);
                else
                    Left = EmptyCastExpression.Create(Left, underlyingType);

            }
            else if (rightIsEnum)
            {
                underlyingType = Enum.GetUnderlyingType(rightType);

                if (Operator != MSAst.ExpressionType.Add)
                {
                    var c = Left as ConstantExpression;
                    if (c == null || !c.IsDefaultValue)
                        return null;
                }
                else
                {
                    if (!TypeUtils.IsImplicitlyConvertible(Left.Type, underlyingType))
                        return null;

                    Left = ConvertExpression.MakeImplicitConversion(ec, Left, underlyingType, Left.Span);
                }

                if (Right is ConstantExpression)
                    Right = ((ConstantExpression)Right).ConvertExplicitly(false, underlyingType);
                else
                    Right = EmptyCastExpression.Create(Right, underlyingType);

            }
            else
            {
                return null;
            }

            //
            // C# specification uses explicit cast syntax which means binary promotion
            // should happen, however it seems that csc does not do that
            //
            if (!DoBinaryOperatorPromotion(ec))
            {
                Left = leftTemp;
                Right = rightTemp;
                return null;
            }

            Type resultType = null;

            if (Operator.IsBitwise() || Operator == MSAst.ExpressionType.Subtract || Operator == MSAst.ExpressionType.Add)
            {
                var promotedType = leftIsEnum ? Left.Type : Right.Type;

                ConvertExpression.MakeExplicitConversion(
                    ec,
                    new EmptyExpression(promotedType),
                    underlyingType,
                    Span);

                if (Operator == MSAst.ExpressionType.Subtract && rightIsEnum && leftIsEnum)
                    resultType = underlyingType;
                else if (Operator == MSAst.ExpressionType.Add && rightIsEnum)
                    resultType = rightType;
                else
                    resultType = leftType;
            }

            expr = ResolveOperatorPredefined(ec, _standardOperators, true, resultType);
            if (expr == null)
                return expr;

            //
            // Section: 7.16.2
            //

            //
            // If the return type of the selected operator is implicitly convertible to the type of x
            //
            if (TypeUtils.IsImplicitlyConvertible(expr.Type, leftType))
                return expr;

            //
            // Otherwise, if the selected operator is a predefined operator, if the return type of the
            // selected operator is explicitly convertible to the type of x, and if y is implicitly
            // convertible to the type of x or the operator is a shift operator, then the operation
            // is evaluated as x = (T)(x op y), where T is the type of x
            //
            expr = ConvertExpression.MakeExplicitConversion(ec, expr, leftType, Span);
            if (expr == null)
                return null;

            if (TypeUtils.IsImplicitlyConvertible(leftTemp.Type, leftType))
                return expr;

            return null;
        }

        // at least one of 'left' or 'right' is an enumeration constant (EnumConstant or SideEffectConstant or ...)
        // if 'left' is not an enumeration constant, create one from the type of 'right'
        private ConstantExpression EnumLiftUp(ParseContext ec, ConstantExpression left, ConstantExpression right, SourceSpan loc)
        {
            switch (Operator)
            {
                case MSAst.ExpressionType.Or:
                case MSAst.ExpressionType.And:
                case MSAst.ExpressionType.ExclusiveOr:
                case MSAst.ExpressionType.Equal:
                case MSAst.ExpressionType.NotEqual:
                case MSAst.ExpressionType.LessThan:
                case MSAst.ExpressionType.LessThanOrEqual:
                case MSAst.ExpressionType.GreaterThan:
                case MSAst.ExpressionType.GreaterThanOrEqual:
                    if (TypeManager.IsEnumType(left.Type))
                        return left;

                    if (left.IsZeroInteger)
                        return left.TryReduce(ec, right.Type, loc);

                    break;

                case MSAst.ExpressionType.Add:
                case MSAst.ExpressionType.Subtract:
                    return left;

                case MSAst.ExpressionType.Multiply:
                case MSAst.ExpressionType.Divide:
                case MSAst.ExpressionType.Modulo:
                case MSAst.ExpressionType.LeftShift:
                case MSAst.ExpressionType.RightShift:
                    if (TypeManager.IsEnumType(right.Type) || TypeManager.IsEnumType(left.Type))
                        break;
                    return left;
            }

            OnErrorOperatorCannotBeApplied(ec, _left, _right, GetOperatorToken(Operator), Span);

            return null;
        }

        static void CreateStandardOperatorsTable()
        {
            var temp = new List<PredefinedOperator>();
            var boolType = TypeManager.CoreTypes.Boolean;

            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Int32, OperatorFlags.Arithmetic | OperatorFlags.Bitwise));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.UInt32, OperatorFlags.Arithmetic | OperatorFlags.Bitwise));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Int64, OperatorFlags.Arithmetic | OperatorFlags.Bitwise));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.UInt64, OperatorFlags.Arithmetic | OperatorFlags.Bitwise));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Single, OperatorFlags.Arithmetic));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Double, OperatorFlags.Arithmetic));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Decimal, OperatorFlags.Arithmetic));

            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Int32, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.UInt32, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Int64, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.UInt64, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Single, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Double, OperatorFlags.Comparison, boolType));
            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.Decimal, OperatorFlags.Comparison, boolType));

            temp.Add(new PredefinedOperator(TypeManager.CoreTypes.String, OperatorFlags.Equality, boolType));

            temp.Add(new PredefinedStringOperator(TypeManager.CoreTypes.String, OperatorFlags.Addition));
            temp.Add(new PredefinedStringOperator(TypeManager.CoreTypes.String, TypeManager.CoreTypes.Object, OperatorFlags.Addition));
            temp.Add(new PredefinedStringOperator(TypeManager.CoreTypes.Object, TypeManager.CoreTypes.String, OperatorFlags.Addition));

            temp.Add(new PredefinedOperator(boolType, OperatorFlags.Bitwise | OperatorFlags.Logical | OperatorFlags.Equality, boolType));

            temp.Add(new PredefinedShiftOperator(TypeManager.CoreTypes.Int32, OperatorFlags.Shift));
            temp.Add(new PredefinedShiftOperator(TypeManager.CoreTypes.UInt32, OperatorFlags.Shift));
            temp.Add(new PredefinedShiftOperator(TypeManager.CoreTypes.Int64, OperatorFlags.Shift));
            temp.Add(new PredefinedShiftOperator(TypeManager.CoreTypes.UInt64, OperatorFlags.Shift));

            _standardOperators = temp.ToArray();
        }

        //
        // Build-in operators method overloading
        //
        protected virtual Expression ResolveOperatorPredefined(ParseContext ec, PredefinedOperator[] operators, bool primitivesOnly, Type enumType)
        {
            PredefinedOperator bestOperator = null;

            var leftType = Left.Type;
            var rightType = Right.Type;

            var operatorFlags = Enum.GetValues(typeof(OperatorFlags))
                .Cast<OperatorFlags>()
                .Where(o => Operator.Matches(o))
                .Aggregate((a, b) => a | b);

            foreach (var po in operators)
            {
                if ((po.Operator & operatorFlags) == 0)
                    continue;

                if (primitivesOnly)
                {
                    if (!po.IsPrimitiveApplicable(leftType, rightType))
                        continue;
                }
                else
                {
                    if (!po.IsApplicable(ec, Left, Right))
                        continue;
                }

                if (bestOperator == null)
                {
                    bestOperator = po;
                    if (primitivesOnly)
                        break;

                    continue;
                }

                bestOperator = po.ResolveBetterOperator(ec, bestOperator);

                if (bestOperator == null)
                {
                    ec.ReportError(
                        34,
                        string.Format(
                            "Operator '{0}' is ambiguous on operands of type '{1}' and '{2}'.",
                            GetOperatorToken(Operator),
                            TypeManager.GetCSharpName(leftType),
                            TypeManager.GetCSharpName(rightType)),
                        Span);

                    bestOperator = po;
                    break;
                }
            }

            if (bestOperator == null)
                return null;

            var expr = bestOperator.ConvertResult(ec, this);

            //
            // Optimize &/&& constant expressions with 0 value
            //
            if (Operator == MSAst.ExpressionType.And || Operator == MSAst.ExpressionType.AndAlso)
            {
                var rc = Right as ConstantExpression;
                var lc = Left as ConstantExpression;

                if ((lc != null && lc.IsDefaultValue) || (rc != null && rc.IsDefaultValue))
                {
                    //
                    // The result is a constant with side-effect
                    //
                    var sideEffect = (rc == null)
                                         ? new SideEffectConstantExpression(lc, Right, Span)
                                         : new SideEffectConstantExpression(rc, Left, Span);

                    return ReducedExpression.Create(sideEffect, expr);
                }
            }

            if (enumType == null)
                return expr;

            //
            // HACK: required by enum_conversion
            //
            expr.Type = enumType;
            return EmptyCastExpression.Create(expr, enumType);
        }

        //
        // Performs user-operator overloading
        //
        protected virtual Expression ResolveUserOperator(ParseContext ec, Type l, Type r)
        {
            MSAst.ExpressionType userOper;

            if (Operator == MSAst.ExpressionType.AndAlso)
                userOper = MSAst.ExpressionType.And;
            else if (Operator == MSAst.ExpressionType.OrElse)
                userOper = MSAst.ExpressionType.Or;
            else
                userOper = Operator;

            var methodName = OperatorInfo.GetOperatorInfo(userOper).SignatureName;

            var leftOperators = MemberLookup(ec, null, l, methodName, MemberTypes.Method, AllBindingFlags, Span) as MethodGroupExpression;
            MethodGroupExpression rightOperators = null;

            if (!TypeManager.IsEqual(r, l))
            {
                rightOperators = MemberLookup(ec, null, r, methodName, MemberTypes.Method, AllBindingFlags, Span) as MethodGroupExpression;
                if (rightOperators == null && leftOperators == null)
                    return null;
            }
            else if (leftOperators == null)
            {
                return null;
            }

            var args = new Arguments(2);

            var leftArg = new Argument(Left);
            args.Add(leftArg);

            var rightArg = new Argument(Right);
            args.Add(rightArg);

            MethodGroupExpression union;

            //
            // User-defined operator implementations always take precedence
            // over predefined operator implementations
            //
            if (leftOperators != null && rightOperators != null)
            {
                if (IsPredefinedUserOperator(l, userOper))
                {
                    union = rightOperators.OverloadResolve(ec, ref args, true, Span) ?? leftOperators;
                }
                else if (IsPredefinedUserOperator(r, userOper))
                {
                    union = leftOperators.OverloadResolve(ec, ref args, true, Span) ?? rightOperators;
                }
                else
                {
                    union = MethodGroupExpression.MakeUnionSet(leftOperators, rightOperators, Span);
                }
            }
            else if (leftOperators != null)
            {
                union = leftOperators;
            }
            else
            {
                union = rightOperators;
            }

            union = union.OverloadResolve(ec, ref args, true, Span);
            if (union == null)
                return null;

            Expression operExpr;

            if (userOper != Operator)
            {
                operExpr = new ConditionalLogicalOperator(union, args, Span).Resolve(ec);
            }
            else
            {
                operExpr = new UserOperatorCall(union, args, Span);

                //
                // This is used to check if a test 'x == null' can be optimized to a reference equals,
                // and not invoke user operator
                //
                if (Operator.IsEquality())
                {
                    if ((Left is NullLiteral && IsBuildInEqualityOperator(r)) ||
                        (Right is NullLiteral && IsBuildInEqualityOperator(l)))
                    {
                        Type = TypeManager.CoreTypes.Boolean;
                        if (Left is NullLiteral || Right is NullLiteral)
                            operExpr = ReducedExpression.Create(this, operExpr).Resolve(ec);
                    }
                    else if (l != r)
                    {
                        var mi = (MethodInfo)union;

                        //
                        // Two System.Delegate(s) are never equal
                        //
                        if (mi.DeclaringType == TypeManager.CoreTypes.MulticastDelegate)
                            return null;
                    }
                }
            }

            Left = leftArg.Value;
            Right = rightArg.Value;

            return operExpr;
        }

        private static bool IsPredefinedUserOperator(Type t, MSAst.ExpressionType op)
        {
            //
            // Some predefined types have user operators
            //
            return (op.IsEquality() && (t == TypeManager.CoreTypes.String || t == TypeManager.CoreTypes.Decimal));
        }

        static bool IsBuildInEqualityOperator(Type t)
        {
            return t == TypeManager.CoreTypes.Object || t == TypeManager.CoreTypes.String ||
                   t == TypeManager.CoreTypes.Delegate || TypeManager.IsDelegateType(t);
        }

        protected class PredefinedOperator
        {
            protected readonly Type Left;
            protected readonly Type Right;
            public readonly OperatorFlags Operator;
            public Type ReturnType;

            public PredefinedOperator(Type ltype, Type rtype, OperatorFlags opMask)
                : this(ltype, rtype, opMask, ltype)
            {
            }

            public PredefinedOperator(Type type, OperatorFlags opMask, Type returnType)
                : this(type, type, opMask, returnType)
            {
            }

            public PredefinedOperator(Type type, OperatorFlags opMask)
                : this(type, type, opMask, type)
            {
            }

            public PredefinedOperator(Type ltype, Type rtype, OperatorFlags opMask, Type returnType)
            {
                Left = ltype;
                Right = rtype;
                Operator = opMask;
                ReturnType = returnType;
            }

            public virtual Expression ConvertResult(ParseContext ec, BinaryExpression b)
            {
                b.Type = ReturnType;

                b.Left = ConvertExpression.MakeImplicitConversion(ec, b.Left, Left, b.Left.Span);
                b.Right = ConvertExpression.MakeImplicitConversion(ec, b.Right, Right, b.Right.Span);

                //
                // A user operators does not support multiple user conversions, but decimal type
                // is considered to be predefined type therefore we apply predefined operators rules
                // and then look for decimal user-operator implementation
                //
                if (Left == TypeManager.CoreTypes.Decimal)
                    return b.ResolveUserOperator(ec, b.Left.Type, b.Right.Type);

                return b;
            }

            public bool IsPrimitiveApplicable(Type ltype, Type rtype)
            {
                //
                // We are dealing with primitive types only
                //
                return Left == ltype && ltype == rtype;
            }

            public virtual bool IsApplicable(ParseContext ec, Expression lexpr, Expression rexpr)
            {
                if (TypeManager.IsEqual(Left, lexpr.Type) &&
                    TypeManager.IsEqual(Right, rexpr.Type))
                    return true;

                return TypeUtils.IsImplicitlyConvertible(lexpr.Type, Left, true) &&
                       TypeUtils.IsImplicitlyConvertible(rexpr.Type, Right, true);
            }

            public PredefinedOperator ResolveBetterOperator(ParseContext ec, PredefinedOperator bestOperator)
            {
                int result = 0;
                if (Left != null && bestOperator.Left != null)
                {
                    result = MethodGroupExpression.BetterTypeConversion(ec, bestOperator.Left, Left);
                }

                //
                // When second arguments are same as the first one, the result is same
                //
                if (Right != null && (Left != Right || bestOperator.Left != bestOperator.Right))
                {
                    result |= MethodGroupExpression.BetterTypeConversion(ec, bestOperator.Right, Right);
                }

                if (result == 0 || result > 2)
                    return null;

                return result == 1 ? bestOperator : this;
            }
        }

        class PredefinedStringOperator : PredefinedOperator
        {
            public PredefinedStringOperator(Type type, OperatorFlags opMask)
                : base(type, opMask, type)
            {
                ReturnType = TypeManager.CoreTypes.String;
            }

            public PredefinedStringOperator(Type ltype, Type rtype, OperatorFlags opMask)
                : base(ltype, rtype, opMask)
            {
                ReturnType = TypeManager.CoreTypes.String;
            }

            public override Expression ConvertResult(ParseContext ec, BinaryExpression b)
            {
                //
                // Use original expression for nullable arguments
                //
                var unwrap = b.Left as Unwrap;
                if (unwrap != null)
                    b.Left = unwrap.Original;

                unwrap = b.Right as Unwrap;
                if (unwrap != null)
                    b.Right = unwrap.Original;

                b.Left = ConvertExpression.MakeImplicitConversion(ec, b.Left, Left, b.Left.Span);
                b.Right = ConvertExpression.MakeImplicitConversion(ec, b.Right, Right, b.Right.Span);

                //
                // Start a new concat expression using converted expression
                //
                return new ConcatExpression(b.Span, b.Left, b.Right).Resolve(ec);
            }
        }

        class PredefinedShiftOperator : PredefinedOperator
        {
            public PredefinedShiftOperator(Type ltype, OperatorFlags opMask) :
                base(ltype, TypeManager.CoreTypes.Int32, opMask)
            {
            }

            public override Expression ConvertResult(ParseContext ec, BinaryExpression b)
            {
                b.Left = ConvertExpression.MakeImplicitConversion(ec, b.Left, Left, b.Left.Span);

                var exprTreeExpr = ConvertExpression.MakeImplicitConversion(ec, b.Right, TypeManager.CoreTypes.Int32, b.Right.Span);

                var rightMask = (Left == TypeManager.CoreTypes.Int32) || (Left == TypeManager.CoreTypes.UInt32) ? 0x1f : 0x3f;

                //
                // b = b.left >> b.right & (0x1f|0x3f)
                //
                b.Right = new BinaryExpression
                {
                    Operator = MSAst.ExpressionType.And,
                    Left = b.Right,
                    Right = new ConstantExpression<int>(rightMask, b.Right.Span).Resolve(ec)
                };

                //
                // Expression tree representation does not use & mask
                //
                b.Right = ReducedExpression.Create(b.Right, exprTreeExpr).Resolve(ec);
                b.Type = ReturnType;

                return b;
            }
        }
    }

    [Flags]
    public enum OperatorFlags
    {
        Logical = 0x0001,
        Bitwise = 0x0002,
        Arithmetic = 0x0004,
        Comparison = 0x0008,
        Relational = 0x0010,
        Equality = 0x0020,
        Shift = 0x0040,
        Addition = 0x0080,
        Subtraction = 0x0100
    }
}