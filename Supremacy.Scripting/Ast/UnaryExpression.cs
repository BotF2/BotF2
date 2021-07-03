using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class UnaryExpression : Expression
    {
        private static Dictionary<MSAst.ExpressionType, List<Type>> _predefinedOperators;

        public MSAst::ExpressionType Operator { get; set; }
        public Expression Operand { get; set; }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            MSAst.Expression operand = Operand.Transform(generator);

            switch (Operator)
            {
                case MSAst.ExpressionType.Negate:
                    return MSAst.Expression.Negate(operand);
                case MSAst.ExpressionType.Not:
                    return MSAst.Expression.Not(operand);
                case MSAst.ExpressionType.OnesComplement:
                    return MSAst.Expression.OnesComplement(operand);
                default:
                    throw new NotSupportedException(Operator.ToString());
            }
        }

        private static string GetOperatorToken(MSAst.ExpressionType @operator)
        {
            switch (@operator)
            {
                case MSAst.ExpressionType.OnesComplement:
                    return "~";
                case MSAst.ExpressionType.Not:
                    return "!";
                case MSAst.ExpressionType.Negate:
                    return "-";
                case MSAst.ExpressionType.UnaryPlus:
                    return "+";
            }

            return OperatorInfo.GetOperatorInfo(@operator).Name;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            string operatorToken = GetOperatorToken(Operator);

            sw.Write(operatorToken);

            bool parenthesize = Operand.ShouldParenthesize;

            if (parenthesize)
            {
                sw.Write("(");
            }

            Operand.Dump(sw, indentChange);

            if (parenthesize)
            {
                sw.Write(")");
            }
        }

        // <summary>
        //   This routine will attempt to simplify the unary expression when the
        //   argument is a constant.
        // </summary>
        private ConstantExpression TryReduceConstant(ParseContext ec, ConstantExpression e)
        {
            if (e is EmptyConstantCastExpression)
            {
                return TryReduceConstant(ec, ((EmptyConstantCastExpression)e).Child);
            }

            if (e is SideEffectConstantExpression)
            {
                ConstantExpression r = TryReduceConstant(ec, ((SideEffectConstantExpression)e).ConstantValue);
                return r == null ? null : new SideEffectConstantExpression(r, e, r.Span);
            }

            Type exprType = e.Type;

            switch (Operator)
            {
                case MSAst.ExpressionType.UnaryPlus:
                    // Unary numeric promotions
                    if (exprType == TypeManager.CoreTypes.Byte)
                    {
                        return new ConstantExpression<int>((byte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.SByte)
                    {
                        return new ConstantExpression<int>((sbyte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Int16)
                    {
                        return new ConstantExpression<int>((short)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt16)
                    {
                        return new ConstantExpression<int>((ushort)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Char)
                    {
                        return new ConstantExpression<int>((char)e.Value, e.Span);
                    }

                    // Predefined operators
                    if (exprType == TypeManager.CoreTypes.UInt32 || exprType == TypeManager.CoreTypes.UInt32 ||
                        exprType == TypeManager.CoreTypes.Int64 || exprType == TypeManager.CoreTypes.UInt64 ||
                        exprType == TypeManager.CoreTypes.Single || exprType == TypeManager.CoreTypes.Double ||
                        exprType == TypeManager.CoreTypes.Decimal)
                    {
                        return e;
                    }

                    return null;

                case MSAst.ExpressionType.Negate:
                    // Unary numeric promotions
                    if (exprType == TypeManager.CoreTypes.Byte)
                    {
                        return new ConstantExpression<int>(-(byte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.SByte)
                    {
                        return new ConstantExpression<int>(-(sbyte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Int16)
                    {
                        return new ConstantExpression<int>(-(short)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt16)
                    {
                        return new ConstantExpression<int>(-(ushort)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Char)
                    {
                        return new ConstantExpression<int>(-(char)e.Value, e.Span);
                    }

                    // Predefined operators
                    if (exprType == TypeManager.CoreTypes.UInt32)
                    {
                        int value = (int)e.Value;
                        if (value == int.MinValue)
                        {
                            if (ec.ConstantCheckState)
                            {
                                ConstantFold.Error_CompileTimeOverflow(ec, Span);
                                return null;
                            }
                            return e;
                        }
                        return new ConstantExpression<int>(-value, e.Span);
                    }
                    if (exprType == TypeManager.CoreTypes.Int64)
                    {
                        long value = (long)e.Value;
                        if (value == long.MinValue)
                        {
                            if (ec.ConstantCheckState)
                            {
                                ConstantFold.Error_CompileTimeOverflow(ec, Span);
                                return null;
                            }
                            return e;
                        }
                        return new ConstantExpression<long>(-value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt32)
                    {
                        return new ConstantExpression<long>(-(uint)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt64)
                    {
                        return null;
                    }

                    if (exprType == TypeManager.CoreTypes.Single)
                    {
                        return new ConstantExpression<float>(-(float)e.Value, e.Span);
                    }
                    if (exprType == TypeManager.CoreTypes.Double)
                    {
                        return new ConstantExpression<double>(-(double)e.Value, e.Span);
                    }
                    if (exprType == TypeManager.CoreTypes.Decimal)
                    {
                        return new ConstantExpression<decimal>(-(decimal)e.Value, e.Span);
                    }

                    return null;

                case MSAst.ExpressionType.Not:
                    if (exprType != TypeManager.CoreTypes.Boolean)
                    {
                        return null;
                    }

                    bool b = (bool)e.Value;
                    return new ConstantExpression<bool>(!b, e.Span);

                case MSAst.ExpressionType.OnesComplement:
                    // Unary numeric promotions
                    if (exprType == TypeManager.CoreTypes.Byte)
                    {
                        return new ConstantExpression<int>(~(byte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.SByte)
                    {
                        return new ConstantExpression<int>(~(sbyte)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Int16)
                    {
                        return new ConstantExpression<int>(~(short)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt16)
                    {
                        return new ConstantExpression<int>(~(ushort)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Char)
                    {
                        return new ConstantExpression<int>(~(char)e.Value, e.Span);
                    }

                    // Predefined operators
                    if (exprType == TypeManager.CoreTypes.Int32)
                    {
                        return new ConstantExpression<int>(~(int)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt32)
                    {
                        return new ConstantExpression<uint>(~(uint)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.Int64)
                    {
                        return new ConstantExpression<long>(~(long)e.Value, e.Span);
                    }

                    if (exprType == TypeManager.CoreTypes.UInt64)
                    {
                        return new ConstantExpression<ulong>(~(ulong)e.Value, e.Span);
                    }
                    if (e is EnumConstantExpression)
                    {
                        e = TryReduceConstant(ec, ((EnumConstantExpression)e).Child);
                        if (e != null)
                        {
                            e = new EnumConstantExpression(e, exprType);
                        }

                        return e;
                    }
                    return null;
            }
            throw new Exception("Can not constant fold: " + Operator);
        }

        protected Expression ResolveOperator(ParseContext ec, Expression expr)
        {
            ExpressionClass = ExpressionClass.Value;

            if (_predefinedOperators == null)
            {
                CreatePredefinedOperatorsTable();
            }

            Type exprType = expr.Type;

            //
            // Primitive types first
            //
            if (TypeManager.IsPrimitiveType(exprType))
            {
                Expression bestExpr = ResolvePrimitivePredefinedType(ec, expr);
                if (bestExpr == null)
                {
                    return null;
                }

                Type = bestExpr.Type;
                Operand = bestExpr;
                return this;
            }

            //
            // E operator ~(E x);
            //
            return Operator == MSAst.ExpressionType.OnesComplement && TypeManager.IsEnumType(exprType)
                ? ResolveEnumOperator(ec, expr)
                : ResolveUserType(ec, expr);
        }

        protected virtual Expression ResolveEnumOperator(ParseContext ec, Expression expr)
        {
            Type underlyingType = Enum.GetUnderlyingType(expr.Type);

            Expression bestExpr = ResolvePrimitivePredefinedType(ec, EmptyCastExpression.Create(expr, underlyingType));
            if (bestExpr == null)
            {
                return null;
            }

            Operand = bestExpr;
            Type = expr.Type;

            return EmptyCastExpression.Create(this, Type);
        }

        private Expression ResolvePrimitivePredefinedType(ParseContext ec, Expression expr)
        {
            expr = DoNumericPromotion(ec, Operator, expr);

            Type exprType = expr.Type;
            List<Type> predefined = _predefinedOperators[Operator];

            return predefined.Any(t => t == exprType) ? expr : null;
        }

        private static Expression DoNumericPromotion(ParseContext ec, MSAst.ExpressionType op, Expression expr)
        {
            Type exprType = expr.Type;

            if (((op == MSAst.ExpressionType.UnaryPlus || op == MSAst.ExpressionType.Negate || op == MSAst.ExpressionType.OnesComplement) &&
                exprType == TypeManager.CoreTypes.Byte) || exprType == TypeManager.CoreTypes.SByte ||
                exprType == TypeManager.CoreTypes.Int16 || exprType == TypeManager.CoreTypes.UInt16 ||
                exprType == TypeManager.CoreTypes.Char)
            {
                return ConvertExpression.MakeImplicitNumericConversion(ec, expr, expr.Type, TypeManager.CoreTypes.Int32);
            }

            return op == MSAst.ExpressionType.Negate && exprType == TypeManager.CoreTypes.UInt32
                ? ConvertExpression.MakeImplicitNumericConversion(ec, expr, expr.Type, TypeManager.CoreTypes.Int64)
                : expr;
        }

        //
        // Unary user type overload resolution
        //
        private Expression ResolveUserType(ParseContext ec, Expression expr)
        {
            Expression bestExpr = ResolveUserOperator(ec, expr);
            if (bestExpr != null)
            {
                return bestExpr;
            }

            List<Type> predefined = _predefinedOperators[Operator];
            foreach (Type t in predefined)
            {
                Expression operExpr = ConvertExpression.MakeExplicitConversion(ec, expr, t, expr.Span);
                if (operExpr == null)
                {
                    continue;
                }

                //
                // decimal type is predefined but has user-operators
                //
                operExpr = operExpr.Type == TypeManager.CoreTypes.Decimal ? ResolveUserType(ec, operExpr) : ResolvePrimitivePredefinedType(ec, operExpr);

                if (operExpr == null)
                {
                    continue;
                }

                if (bestExpr == null)
                {
                    bestExpr = operExpr;
                    continue;
                }

                int result = MethodGroupExpression.BetterTypeConversion(ec, bestExpr.Type, t);
                if (result == 0)
                {
                    ec.ReportError(
                        35,
                        string.Format(
                            "Operator '{0}' is ambiguous on an operand of type '{1}'.",
                            GetOperatorToken(Operator),
                            TypeManager.GetCSharpName(expr.Type)),
                        Span);
                    break;
                }

                if (result == 2)
                {
                    bestExpr = operExpr;
                }
            }

            if (bestExpr == null)
            {
                return null;
            }

            //
            // HACK: Decimal user-operator is included in standard operators
            //
            if (bestExpr.Type == TypeManager.CoreTypes.Decimal)
            {
                return bestExpr;
            }

            Operand = bestExpr;
            Type = bestExpr.Type;

            return this;
        }

        protected virtual Expression ResolveUserOperator(ParseContext ec, Expression expr)
        {
            OperatorInfo opType;

            switch (Operator)
            {
                case MSAst.ExpressionType.Not:
                    opType = OperatorInfo.GetOperatorInfo(MSAst.ExpressionType.Not); break;
                case MSAst.ExpressionType.OnesComplement:
                    opType = OperatorInfo.GetOperatorInfo(MSAst.ExpressionType.OnesComplement); break;
                case MSAst.ExpressionType.Negate:
                    opType = OperatorInfo.GetOperatorInfo(MSAst.ExpressionType.Negate); break;
                case MSAst.ExpressionType.UnaryPlus:
                    opType = OperatorInfo.GetOperatorInfo(MSAst.ExpressionType.UnaryPlus); break;
                default:
                    throw new InternalErrorException(Operator.ToString());
            }

            string opName = opType.SignatureName;

            if (!(MemberLookup(ec, null, expr.Type, opName, MemberTypes.Method, AllBindingFlags, expr.Span) is MethodGroupExpression userOp))
            {
                return null;
            }

            Arguments args = new Arguments(1) { new Argument(expr) };

            userOp = userOp.OverloadResolve(ec, ref args, false, expr.Span);

            if (userOp == null)
            {
                return null;
            }

            Operand = args[0].Value;

            return new UserOperatorCall(userOp, args, expr.Span);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            Operand = Operand.Resolve(ec);

            if (Operand == null)
            {
                return null;
            }

            if (TypeManager.IsNullableType(Operand.Type))
            {
                return new LiftedUnaryOperator(Operator, Operand).Resolve(ec);
            }

            //
            // Attempt to use a constant folding operation.
            //
            if (Operand is ConstantExpression cexpr)
            {
                cexpr = TryReduceConstant(ec, cexpr);
                if (cexpr != null)
                {
                    return cexpr;
                }
            }

            Expression expr = ResolveOperator(ec, Operand);
            if (expr == null)
            {
                OnErrorOperatorCannotBeApplied(ec, Span, GetOperatorToken(Operator), Operand.Type);
            }

            //
            // Reduce unary operator on predefined types
            //
            return expr == this && Operator == MSAst.ExpressionType.UnaryPlus ? Operand : expr;
        }

        public static void OnErrorOperatorCannotBeApplied(ParseContext ec, SourceSpan loc, string oper, Type t)
        {
            ec.ReportError(
                23,
                string.Format(
                    "The '{0}' operator cannot be applied to operand of type '{1}'.",
                    oper,
                    TypeManager.GetCSharpName(t)),
                loc);
        }

        private static void CreatePredefinedOperatorsTable()
        {
            //
            // 7.6.1 Unary plus operator
            //
            _predefinedOperators = new Dictionary<MSAst.ExpressionType, List<Type>>
                                   {
                                       {
                                           MSAst.ExpressionType.UnaryPlus,
                                           new List<Type>
                                           {
                                               TypeManager.CoreTypes.Int32,
                                               TypeManager.CoreTypes.UInt32,
                                               TypeManager.CoreTypes.Int64,
                                               TypeManager.CoreTypes.UInt64,
                                               TypeManager.CoreTypes.Single,
                                               TypeManager.CoreTypes.Double,
                                               TypeManager.CoreTypes.Decimal
                                           }
                                       },
                                       {
                                           MSAst.ExpressionType.Negate,
                                           new List<Type>
                                           {
                                               TypeManager.CoreTypes.Int32,
                                               TypeManager.CoreTypes.Int64,
                                               TypeManager.CoreTypes.Single,
                                               TypeManager.CoreTypes.Double,
                                               TypeManager.CoreTypes.Decimal
                                           }
                                       },
                                       {
                                           MSAst.ExpressionType.Not,
                                           new List<Type>
                                           {
                                               TypeManager.CoreTypes.Boolean
                                           }
                                       },
                                       {
                                           MSAst.ExpressionType.OnesComplement,
                                           new List<Type>
                                           {
                                               TypeManager.CoreTypes.Int32,
                                               TypeManager.CoreTypes.UInt32,
                                               TypeManager.CoreTypes.Int64,
                                               TypeManager.CoreTypes.UInt64
                                           }
                                       }
                                   };
        }
    }

    public class LiftedUnaryOperator : UnaryExpression
    {
        private Unwrap _unwrap;
        private Expression _userOperator;

        public LiftedUnaryOperator(MSAst.ExpressionType @operator, Expression operand)
        {
            Operator = @operator;
            Operand = operand;
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
            {
                return this;
            }

            _unwrap = Unwrap.Create(Operand, false);

            if (_unwrap == null)
            {
                return null;
            }

            Expression res = ResolveOperator(ec, _unwrap);
            if (res != this)
            {
                if (_userOperator == null)
                {
                    return res;
                }
            }
            else
            {
                res = Operand = LiftExpression(ec, Operand);
            }

            if (res == null)
            {
                return null;
            }

            ExpressionClass = ExpressionClass.Value;
            Type = res.Type;

            return this;
        }

        private static Expression LiftExpression(ParseContext ec, Expression expr)
        {
            TypeExpression liftedType = new NullableTypeExpression(expr.Type, expr.Span);

            liftedType = liftedType.ResolveAsTypeTerminal(ec, false);

            if (liftedType == null)
            {
                return null;
            }

            expr.Type = liftedType.Type;

            return expr;
        }

        protected override Expression ResolveEnumOperator(ParseContext ec, Expression expr)
        {
            expr = base.ResolveEnumOperator(ec, expr);

            if (expr == null)
            {
                return null;
            }

            Operand = LiftExpression(ec, Operand);

            return LiftExpression(ec, expr);
        }

        protected override Expression ResolveUserOperator(ParseContext ec, Expression expr)
        {
            expr = base.ResolveUserOperator(ec, expr);
            if (expr == null)
            {
                return null;
            }

            //
            // When a user operator is of non-nullable type
            //
            if (Operand is Unwrap)
            {
                _userOperator = LiftExpression(ec, expr);
                return _userOperator;
            }

            return expr;
        }
    }
}