using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class ConstantFold
    {
        public static readonly Type[] BinaryPromotions = new Type[]
                                                         {
                                                             TypeManager.CoreTypes.Decimal,
                                                             TypeManager.CoreTypes.Double,
                                                             TypeManager.CoreTypes.Single,
                                                             TypeManager.CoreTypes.UInt64,
                                                             TypeManager.CoreTypes.Int64,
                                                             TypeManager.CoreTypes.UInt32
                                                         };

        //
        // Performs the numeric promotions on the left and right expresions
        // and deposits the results on `lc' and `rc'.
        //
        // On success, the types of `lc' and `rc' on output will always match,
        // and the pair will be one of:
        //
        // TODO: BinaryFold should be called as an optimization step only,
        // error checking here is weak
        //		
        private static bool DoBinaryNumericPromotions(ref ConstantExpression left, ref ConstantExpression right)
        {
            Type ltype = left.Type;
            Type rtype = right.Type;

            foreach (Type t in BinaryPromotions)
            {
                if (t == ltype)
                {
                    return t == rtype || ConvertPromotion(ref right, ref left, t);
                }

                if (t == rtype)
                {
                    return t == ltype || ConvertPromotion(ref left, ref right, t);
                }
            }

            left = left.ConvertImplicitly(TypeManager.CoreTypes.Int32);
            right = right.ConvertImplicitly(TypeManager.CoreTypes.Int32);
            return left != null && right != null;
        }

        private static bool ConvertPromotion(ref ConstantExpression prim, ref ConstantExpression second, Type type)
        {
            ConstantExpression c = prim.ConvertImplicitly(type);
            if (c != null)
            {
                prim = c;
                return true;
            }

            if (type == TypeManager.CoreTypes.UInt32)
            {
                type = TypeManager.CoreTypes.Int64;
                prim = prim.ConvertImplicitly(type);
                second = second.ConvertImplicitly(type);
                return prim != null && second != null;
            }

            return false;
        }

        internal static void Error_CompileTimeOverflow(ParseContext rc, SourceSpan loc)
        {
            rc.ReportError(
                220, 
                "The operation overflows at compile time in checked mode.",
                loc);
        }

        // <summary>
        //   ConstantExpression expression folder for binary operations.
        //
        //   Returns null if the expression can not be folded.
        // </summary>
        static public ConstantExpression BinaryFold(ParseContext ec, MSAst.ExpressionType oper,
                             ConstantExpression left, ConstantExpression right, SourceSpan loc)
        {
            ConstantExpression result = null;

            if (left is EmptyConstantCastExpression)
            {
                return BinaryFold(ec, oper, ((EmptyConstantCastExpression)left).Child, right, loc);
            }

            if (left is SideEffectConstantExpression)
            {
                result = BinaryFold(ec, oper, ((SideEffectConstantExpression)left).ConstantValue, right, loc);
                return result == null ? null : new SideEffectConstantExpression(result, left, loc);
            }

            if (right is EmptyConstantCastExpression)
            {
                return BinaryFold(ec, oper, left, ((EmptyConstantCastExpression)right).Child, loc);
            }

            if (right is SideEffectConstantExpression)
            {
                result = BinaryFold(ec, oper, left, ((SideEffectConstantExpression)right).ConstantValue, loc);
                return result == null ? null : new SideEffectConstantExpression(result, right, loc);
            }

            Type leftType = left.Type;
            Type rightType = right.Type;

            bool booleanResult;

            if (leftType == TypeManager.CoreTypes.Boolean && leftType == rightType)
            {
                bool lv = (bool)left.Value;
                bool rv = (bool)right.Value;
                switch (oper)
                {
                    case MSAst.ExpressionType.And:
                    case MSAst.ExpressionType.AndAlso:
                        return new ConstantExpression<bool>(lv && rv) { Span = left.Span };
                    case MSAst.ExpressionType.Or:
                    case MSAst.ExpressionType.OrElse:
                        return new ConstantExpression<bool>(lv || rv) { Span = left.Span };
                    case MSAst.ExpressionType.ExclusiveOr:
                        return new ConstantExpression<bool>(lv ^ rv) { Span = left.Span };
                    case MSAst.ExpressionType.Equal:
                        return new ConstantExpression<bool>(lv == rv) { Span = left.Span };
                    case MSAst.ExpressionType.NotEqual:
                        return new ConstantExpression<bool>(lv != rv) { Span = left.Span };
                }
                return null;
            }

            if (left is EnumConstantExpression && rightType == leftType)
            {
                switch (oper)
                {
                    // E operator |(E x, E y);
                    // E operator &(E x, E y);
                    // E operator ^(E x, E y);
                    case MSAst.ExpressionType.Or:
                    case MSAst.ExpressionType.And:
                    case MSAst.ExpressionType.ExclusiveOr:
                        return BinaryFold(ec, oper, ((EnumConstantExpression)left).Child,
                                ((EnumConstantExpression)right).Child, loc).TryReduce(ec, leftType, loc);

                    // U operator -(E x, E y);
                    case MSAst.ExpressionType.Subtract:
                        result = BinaryFold(ec, oper, ((EnumConstantExpression)left).Child, ((EnumConstantExpression)right).Child, loc);
                        return result.TryReduce(ec, ((EnumConstantExpression)left).Child.Type, loc);

                    //
                    // bool operator ==(E x, E y);
                    // bool operator !=(E x, E y);
                    // bool operator <(E x, E y);
                    // bool operator >(E x, E y);
                    // bool operator <=(E x, E y);
                    // bool operator >=(E x, E y);
                    // 
                    case MSAst.ExpressionType.Equal:
                    case MSAst.ExpressionType.NotEqual:
                    case MSAst.ExpressionType.LessThan:
                    case MSAst.ExpressionType.GreaterThan:
                    case MSAst.ExpressionType.LessThanOrEqual:
                    case MSAst.ExpressionType.GreaterThanOrEqual:
                        return BinaryFold(ec, oper, ((EnumConstantExpression)left).Child, ((EnumConstantExpression)right).Child, loc);
                }
                return null;
            }

            switch (oper)
            {
                case MSAst.ExpressionType.Or:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<int>)
                    {
                        int res = ((ConstantExpression<int>)left).Value | ((ConstantExpression<int>)right).Value;

                        return new ConstantExpression<int>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<uint>)
                    {
                        uint res = ((ConstantExpression<uint>)left).Value | ((ConstantExpression<uint>)right).Value;

                        return new ConstantExpression<uint>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<long>)
                    {
                        long res = ((ConstantExpression<long>)left).Value | ((ConstantExpression<long>)right).Value;

                        return new ConstantExpression<long>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<ulong>)
                    {
                        ulong res = ((ConstantExpression<ulong>)left).Value |
                            ((ConstantExpression<ulong>)right).Value;

                        return new ConstantExpression<ulong>(res) { Span = left.Span };
                    }
                    break;

                case MSAst.ExpressionType.And:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                        return null;

                    //
                    // int operator &(int x, int y);
                    // uint operator &(uint x, uint y);
                    // long operator &(long x, long y);
                    // ulong operator &(ulong x, ulong y);
                    //
                    if (left is ConstantExpression<int>)
                    {
                        int res = ((ConstantExpression<int>)left).Value & ((ConstantExpression<int>)right).Value;
                        return new ConstantExpression<int>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<uint>)
                    {
                        uint res = ((ConstantExpression<uint>)left).Value & ((ConstantExpression<uint>)right).Value;
                        return new ConstantExpression<uint>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<long>)
                    {
                        long res = ((ConstantExpression<long>)left).Value & ((ConstantExpression<long>)right).Value;
                        return new ConstantExpression<long>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<ulong>)
                    {
                        ulong res = ((ConstantExpression<ulong>)left).Value &
                            ((ConstantExpression<ulong>)right).Value;

                        return new ConstantExpression<ulong>(res) { Span = left.Span };
                    }
                    break;

                case MSAst.ExpressionType.ExclusiveOr:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<int>)
                    {
                        int res = ((ConstantExpression<int>)left).Value ^ ((ConstantExpression<int>)right).Value;
                        return new ConstantExpression<int>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<uint>)
                    {
                        uint res = ((ConstantExpression<uint>)left).Value ^ ((ConstantExpression<uint>)right).Value;

                        return new ConstantExpression<uint>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<long>)
                    {
                        long res = ((ConstantExpression<long>)left).Value ^ ((ConstantExpression<long>)right).Value;

                        return new ConstantExpression<long>(res) { Span = left.Span };
                    }
                    if (left is ConstantExpression<ulong>)
                    {
                        ulong res = ((ConstantExpression<ulong>)left).Value ^
                            ((ConstantExpression<ulong>)right).Value;

                        return new ConstantExpression<ulong>(res) { Span = left.Span };
                    }
                    break;

                case MSAst.ExpressionType.Add:
                    if (leftType == TypeManager.CoreTypes.Null)
                    {
                        return right;
                    }

                    if (rightType == TypeManager.CoreTypes.Null)
                    {
                        return left;
                    }

                    //
                    // If both sides are strings, then concatenate, if
                    // one is a string, and the other is not, then defer
                    // to runtime concatenation
                    //
                    if (leftType == TypeManager.CoreTypes.String || rightType == TypeManager.CoreTypes.String)
                    {
                        return leftType == rightType ? new ConstantExpression<string>((string)left.Value + (string)right.Value, left.Span) : null;
                    }

                    //
                    // handle "E operator + (E x, U y)"
                    // handle "E operator + (Y y, E x)"
                    //
                    EnumConstantExpression lc = left as EnumConstantExpression;
                    EnumConstantExpression rc = right as EnumConstantExpression;
                    if (lc != null || rc != null)
                    {
                        if (lc == null)
                        {
                            lc = rc;
                            leftType = lc.Type;
                            right = left;
                        }

                        // U has to be implicitly convetible to E.base
                        right = right.ConvertImplicitly(lc.Child.Type);
                        if (right == null)
                        {
                            return null;
                        }

                        result = BinaryFold(ec, oper, lc.Child, right, loc);
                        if (result == null)
                        {
                            return null;
                        }

                        result = result.TryReduce(ec, leftType, loc);
                        return result == null ? null : new EnumConstantExpression(result, leftType);
                    }

                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    try
                    {
                        if (left is ConstantExpression<double>)
                        {
                            double res = ec.ConstantCheckState
                                ? ((ConstantExpression<double>)left).Value +
                                           ((ConstantExpression<double>)right).Value
                                : ((ConstantExpression<double>)left).Value +
                                         ((ConstantExpression<double>)right).Value;
                            return new ConstantExpression<double>(res) { Span = left.Span };
                        }
                        if (left is ConstantExpression<float>)
                        {
                            float res = ec.ConstantCheckState
                                ? ((ConstantExpression<float>)left).Value +
                                           ((ConstantExpression<float>)right).Value
                                : ((ConstantExpression<float>)left).Value +
                                         ((ConstantExpression<float>)right).Value;
                            result = new ConstantExpression<float>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<ulong>)
                        {
                            ulong res = ec.ConstantCheckState
                                ? ((ConstantExpression<ulong>)left).Value +
                                           ((ConstantExpression<ulong>)right).Value
                                : ((ConstantExpression<ulong>)left).Value +
                                         ((ConstantExpression<ulong>)right).Value;
                            result = new ConstantExpression<ulong>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<long>)
                        {
                            long res = ec.ConstantCheckState
                                ? ((ConstantExpression<long>)left).Value +
                                           ((ConstantExpression<long>)right).Value
                                : ((ConstantExpression<long>)left).Value +
                                         ((ConstantExpression<long>)right).Value;
                            result = new ConstantExpression<long>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<uint>)
                        {
                            uint res = ec.ConstantCheckState
                                ? ((ConstantExpression<uint>)left).Value +
                                           ((ConstantExpression<uint>)right).Value
                                : ((ConstantExpression<uint>)left).Value +
                                         ((ConstantExpression<uint>)right).Value;
                            result = new ConstantExpression<uint>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<int>)
                        {
                            int res = ec.ConstantCheckState
                                ? ((ConstantExpression<int>)left).Value +
                                           ((ConstantExpression<int>)right).Value
                                : ((ConstantExpression<int>)left).Value +
                                         ((ConstantExpression<int>)right).Value;
                            result = new ConstantExpression<int>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<decimal>)
                        {
                            decimal res = ec.ConstantCheckState
                                ? ((ConstantExpression<decimal>)left).Value +
                                    ((ConstantExpression<decimal>)right).Value
                                : ((ConstantExpression<decimal>)left).Value +
                                    ((ConstantExpression<decimal>)right).Value;
                            result = new ConstantExpression<decimal>(res) { Span = left.Span };
                        }
                    }
                    catch (OverflowException)
                    {
                        Error_CompileTimeOverflow(ec, loc);
                    }

                    return result;

                case MSAst.ExpressionType.Subtract:
                    //
                    // handle "E operator - (E x, U y)"
                    // handle "E operator - (Y y, E x)"
                    //
                    lc = left as EnumConstantExpression;
                    rc = right as EnumConstantExpression;
                    if (lc != null || rc != null)
                    {
                        if (lc == null)
                        {
                            lc = rc;
                            leftType = lc.Type;
                            right = left;
                        }

                        // U has to be implicitly convetible to E.base
                        right = right.ConvertImplicitly(lc.Child.Type);
                        if (right == null)
                        {
                            return null;
                        }

                        result = BinaryFold(ec, oper, lc.Child, right, loc);
                        if (result == null)
                        {
                            return null;
                        }

                        result = result.TryReduce(ec, leftType, loc);
                        return result == null ? null : new EnumConstantExpression(result, leftType);
                    }

                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    try
                    {
                        if (left is ConstantExpression<double>)
                        {
                            double res = ec.ConstantCheckState
                                ? ((ConstantExpression<double>)left).Value -
                                           ((ConstantExpression<double>)right).Value
                                : ((ConstantExpression<double>)left).Value -
                                         ((ConstantExpression<double>)right).Value;
                            result = new ConstantExpression<double>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<float>)
                        {
                            float res = ec.ConstantCheckState
                                ? ((ConstantExpression<float>)left).Value -
                                           ((ConstantExpression<float>)right).Value
                                : ((ConstantExpression<float>)left).Value -
                                         ((ConstantExpression<float>)right).Value;
                            result = new ConstantExpression<float>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<ulong>)
                        {
                            ulong res = ec.ConstantCheckState
                                ? ((ConstantExpression<ulong>)left).Value -
                                           ((ConstantExpression<ulong>)right).Value
                                : ((ConstantExpression<ulong>)left).Value -
                                         ((ConstantExpression<ulong>)right).Value;
                            result = new ConstantExpression<ulong>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<long>)
                        {
                            long res = ec.ConstantCheckState
                                ? ((ConstantExpression<long>)left).Value -
                                           ((ConstantExpression<long>)right).Value
                                : ((ConstantExpression<long>)left).Value -
                                         ((ConstantExpression<long>)right).Value;
                            result = new ConstantExpression<long>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<uint>)
                        {
                            uint res = ec.ConstantCheckState
                                ? ((ConstantExpression<uint>)left).Value -
                                           ((ConstantExpression<uint>)right).Value
                                : ((ConstantExpression<uint>)left).Value -
                                         ((ConstantExpression<uint>)right).Value;
                            result = new ConstantExpression<uint>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<int>)
                        {
                            int res = ec.ConstantCheckState
                                ? ((ConstantExpression<int>)left).Value -
                                           ((ConstantExpression<int>)right).Value
                                : ((ConstantExpression<int>)left).Value -
                                         ((ConstantExpression<int>)right).Value;
                            result = new ConstantExpression<int>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<decimal>)
                        {
                            decimal res = ec.ConstantCheckState
                                ? ((ConstantExpression<decimal>)left).Value -
                                    ((ConstantExpression<decimal>)right).Value
                                : ((ConstantExpression<decimal>)left).Value -
                                    ((ConstantExpression<decimal>)right).Value;
                            return new ConstantExpression<decimal>(res) { Span = left.Span };
                        }
                        else
                        {
                            throw new Exception("Unexepected subtraction input: " + left);
                        }
                    }
                    catch (OverflowException)
                    {
                        Error_CompileTimeOverflow(ec, loc);
                    }

                    return result;

                case MSAst.ExpressionType.Multiply:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                        return null;

                    try
                    {
                        if (left is ConstantExpression<double>)
                        {
                            double res = ec.ConstantCheckState
                                ? ((ConstantExpression<double>)left).Value *
                                    ((ConstantExpression<double>)right).Value
                                : ((ConstantExpression<double>)left).Value *
                                    ((ConstantExpression<double>)right).Value;
                            return new ConstantExpression<double>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<float>)
                        {
                            float res = ec.ConstantCheckState
                                ? ((ConstantExpression<float>)left).Value *
                                    ((ConstantExpression<float>)right).Value
                                : ((ConstantExpression<float>)left).Value *
                                    ((ConstantExpression<float>)right).Value;
                            return new ConstantExpression<float>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<ulong>)
                        {
                            ulong res = ec.ConstantCheckState
                                ? ((ConstantExpression<ulong>)left).Value *
                                    ((ConstantExpression<ulong>)right).Value
                                : ((ConstantExpression<ulong>)left).Value *
                                    ((ConstantExpression<ulong>)right).Value;
                            return new ConstantExpression<ulong>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<long>)
                        {
                            long res = ec.ConstantCheckState
                                ? ((ConstantExpression<long>)left).Value *
                                    ((ConstantExpression<long>)right).Value
                                : ((ConstantExpression<long>)left).Value *
                                    ((ConstantExpression<long>)right).Value;
                            return new ConstantExpression<long>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<uint>)
                        {
                            uint res = ec.ConstantCheckState
                                ? ((ConstantExpression<uint>)left).Value *
                                    ((ConstantExpression<uint>)right).Value
                                : ((ConstantExpression<uint>)left).Value *
                                    ((ConstantExpression<uint>)right).Value;
                            return new ConstantExpression<uint>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<int>)
                        {
                            int res = ec.ConstantCheckState
                                ? ((ConstantExpression<int>)left).Value *
                                    ((ConstantExpression<int>)right).Value
                                : ((ConstantExpression<int>)left).Value *
                                    ((ConstantExpression<int>)right).Value;
                            return new ConstantExpression<int>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<decimal>)
                        {
                            decimal res = ec.ConstantCheckState
                                ? ((ConstantExpression<decimal>)left).Value *
                                    ((ConstantExpression<decimal>)right).Value
                                : ((ConstantExpression<decimal>)left).Value *
                                    ((ConstantExpression<decimal>)right).Value;
                            return new ConstantExpression<decimal>(res) { Span = left.Span };
                        }
                        else
                        {
                            throw new Exception("Unexepected multiply input: " + left);
                        }
                    }
                    catch (OverflowException)
                    {
                        Error_CompileTimeOverflow(ec, loc);
                    }
                    break;

                case MSAst.ExpressionType.Divide:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    try
                    {
                        if (left is ConstantExpression<double>)
                        {
                            double res = ec.ConstantCheckState
                                ? ((ConstantExpression<double>)left).Value /
                                    ((ConstantExpression<double>)right).Value
                                : ((ConstantExpression<double>)left).Value /
                                    ((ConstantExpression<double>)right).Value;
                            return new ConstantExpression<double>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<float>)
                        {
                            float res = ec.ConstantCheckState
                                ? ((ConstantExpression<float>)left).Value /
                                    ((ConstantExpression<float>)right).Value
                                : ((ConstantExpression<float>)left).Value /
                                    ((ConstantExpression<float>)right).Value;
                            return new ConstantExpression<float>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<ulong>)
                        {
                            ulong res = ec.ConstantCheckState
                                ? ((ConstantExpression<ulong>)left).Value /
                                    ((ConstantExpression<ulong>)right).Value
                                : ((ConstantExpression<ulong>)left).Value /
                                    ((ConstantExpression<ulong>)right).Value;
                            return new ConstantExpression<ulong>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<long>)
                        {
                            long res = ec.ConstantCheckState
                                ? ((ConstantExpression<long>)left).Value /
                                    ((ConstantExpression<long>)right).Value
                                : ((ConstantExpression<long>)left).Value /
                                    ((ConstantExpression<long>)right).Value;
                            return new ConstantExpression<long>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<uint>)
                        {
                            uint res = ec.ConstantCheckState
                                ? ((ConstantExpression<uint>)left).Value /
                                    ((ConstantExpression<uint>)right).Value
                                : ((ConstantExpression<uint>)left).Value /
                                    ((ConstantExpression<uint>)right).Value;
                            return new ConstantExpression<uint>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<int>)
                        {
                            int res = ec.ConstantCheckState
                                ? ((ConstantExpression<int>)left).Value /
                                    ((ConstantExpression<int>)right).Value
                                : ((ConstantExpression<int>)left).Value /
                                    ((ConstantExpression<int>)right).Value;
                            return new ConstantExpression<int>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<decimal>)
                        {
                            decimal res = ec.ConstantCheckState
                                ? ((ConstantExpression<decimal>)left).Value /
                                    ((ConstantExpression<decimal>)right).Value
                                : ((ConstantExpression<decimal>)left).Value /
                                    ((ConstantExpression<decimal>)right).Value;
                            return new ConstantExpression<decimal>(res) { Span = left.Span };
                        }
                        else
                        {
                            throw new Exception("Unexepected division input: " + left);
                        }
                    }
                    catch (OverflowException)
                    {
                        Error_CompileTimeOverflow(ec, loc);

                    }
                    catch (DivideByZeroException)
                    {
                        ec.ReportError(20, "Division by constant zero.", loc);
                    }

                    break;

                case MSAst.ExpressionType.Modulo:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    try
                    {
                        if (left is ConstantExpression<double>)
                        {
                            double res = ec.ConstantCheckState
                                ? ((ConstantExpression<double>)left).Value %
                                           ((ConstantExpression<double>)right).Value
                                : ((ConstantExpression<double>)left).Value %
                                         ((ConstantExpression<double>)right).Value;
                            return new ConstantExpression<double>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<float>)
                        {
                            float res = ec.ConstantCheckState
                                ? ((ConstantExpression<float>)left).Value %
                                           ((ConstantExpression<float>)right).Value
                                : ((ConstantExpression<float>)left).Value %
                                         ((ConstantExpression<float>)right).Value;
                            return new ConstantExpression<float>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<ulong>)
                        {
                            ulong res = ec.ConstantCheckState
                                ? ((ConstantExpression<ulong>)left).Value %
                                           ((ConstantExpression<ulong>)right).Value
                                : ((ConstantExpression<ulong>)left).Value %
                                         ((ConstantExpression<ulong>)right).Value;
                            return new ConstantExpression<ulong>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<long>)
                        {
                            long res = ec.ConstantCheckState
                                ? ((ConstantExpression<long>)left).Value %
                                           ((ConstantExpression<long>)right).Value
                                : ((ConstantExpression<long>)left).Value %
                                         ((ConstantExpression<long>)right).Value;
                            return new ConstantExpression<long>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<uint>)
                        {
                            uint res = ec.ConstantCheckState
                                ? ((ConstantExpression<uint>)left).Value %
                                           ((ConstantExpression<uint>)right).Value
                                : ((ConstantExpression<uint>)left).Value %
                                         ((ConstantExpression<uint>)right).Value;
                            return new ConstantExpression<uint>(res) { Span = left.Span };
                        }
                        else if (left is ConstantExpression<int>)
                        {
                            int res = ec.ConstantCheckState
                                ? ((ConstantExpression<int>)left).Value %
                                           ((ConstantExpression<int>)right).Value
                                : ((ConstantExpression<int>)left).Value %
                                         ((ConstantExpression<int>)right).Value;
                            return new ConstantExpression<int>(res) { Span = left.Span };
                        }
                        else
                        {
                            throw new Exception("Unexepected modulus input: " + left);
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        ec.ReportError(20, "Division by constant zero.", loc);
                    }
                    catch (OverflowException)
                    {
                        Error_CompileTimeOverflow(ec, loc);
                    }
                    break;

                //
                // There is no overflow checking on left shift
                //
                case MSAst.ExpressionType.LeftShift:
                    if (!(right.ConvertImplicitly(TypeManager.CoreTypes.Int32) is ConstantExpression<int> ic))
                    {
                        BinaryExpression.OnErrorOperatorCannotBeApplied(ec, left, right, oper, loc);
                        return null;
                    }

                    int lshift_val = ic.Value;
                    if (left.Type == TypeManager.CoreTypes.UInt64)
                    {
                        return new ConstantExpression<ulong>(((ConstantExpression<ulong>)left).Value << lshift_val) { Span = left.Span };
                    }

                    if (left.Type == TypeManager.CoreTypes.Int64)
                    {
                        return new ConstantExpression<long>(((ConstantExpression<long>)left).Value << lshift_val) { Span = left.Span };
                    }

                    if (left.Type == TypeManager.CoreTypes.UInt32)
                    {
                        return new ConstantExpression<uint>(((ConstantExpression<uint>)left).Value << lshift_val) { Span = left.Span };
                    }

                    left = left.ConvertImplicitly(TypeManager.CoreTypes.Int32);
                    if (left.Type == TypeManager.CoreTypes.Int32)
                    {
                        return new ConstantExpression<int>(((ConstantExpression<int>)left).Value << lshift_val) { Span = left.Span };
                    }

                    BinaryExpression.OnErrorOperatorCannotBeApplied(ec, left, right, oper, loc);
                    break;

                //
                // There is no overflow checking on right shift
                //
                case MSAst.ExpressionType.RightShift:
                    if (!(right.ConvertImplicitly(TypeManager.CoreTypes.Int32) is ConstantExpression<int> sic))
                    {
                        BinaryExpression.OnErrorOperatorCannotBeApplied(ec, left, right, oper, loc);
                        return null;
                    }
                    int rshift_val = sic.Value;
                    if (left.Type == TypeManager.CoreTypes.UInt64)
                    {
                        return new ConstantExpression<ulong>(((ConstantExpression<ulong>)left).Value >> rshift_val) { Span = left.Span };
                    }

                    if (left.Type == TypeManager.CoreTypes.Int64)
                    {
                        return new ConstantExpression<long>(((ConstantExpression<long>)left).Value >> rshift_val) { Span = left.Span };
                    }

                    if (left.Type == TypeManager.CoreTypes.UInt32)
                    {
                        return new ConstantExpression<uint>(((ConstantExpression<uint>)left).Value >> rshift_val) { Span = left.Span };
                    }

                    left = left.ConvertImplicitly(TypeManager.CoreTypes.Int32);
                    if (left.Type == TypeManager.CoreTypes.Int32)
                    {
                        return new ConstantExpression<int>(((ConstantExpression<int>)left).Value >> rshift_val) { Span = left.Span };
                    }

                    BinaryExpression.OnErrorOperatorCannotBeApplied(ec, left, right, oper, loc);
                    break;

                case MSAst.ExpressionType.Equal:
                    if (TypeManager.IsReferenceType(leftType) && TypeManager.IsReferenceType(leftType))
                    {
                        if (left.IsNull || right.IsNull)
                        {
                            return ReducedExpression.Create(
                                new ConstantExpression<bool>(left.IsNull == right.IsNull, left.Span),
                                new BinaryExpression { Operator = oper, Left = left, Right = right });
                        }

                        return left is ConstantExpression<string> && right is ConstantExpression<string>
                            ? new ConstantExpression<bool>(
                                ((ConstantExpression<string>)left).Value == ((ConstantExpression<string>)right).Value) { Span = left.Span }
                            : null;
                    }

                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value ==
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value ==
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value ==
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value ==
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value ==
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value ==
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };

                case MSAst.ExpressionType.NotEqual:
                    if (TypeManager.IsReferenceType(leftType) && TypeManager.IsReferenceType(leftType))
                    {
                        if (left.IsNull || right.IsNull)
                        {
                            return ReducedExpression.Create(
                                new ConstantExpression<bool>(left.IsNull != right.IsNull, left.Span),
                                new BinaryExpression { Operator = oper, Left = left, Right = right });
                        }

                        return left is ConstantExpression<string> && right is ConstantExpression<string>
                            ? new ConstantExpression<bool>(
                                ((ConstantExpression<string>)left).Value != ((ConstantExpression<string>)right).Value) { Span = left.Span }
                            : null;
                    }

                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value !=
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value !=
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value !=
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value !=
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value !=
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value !=
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };

                case MSAst.ExpressionType.LessThan:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value <
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value <
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value <
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value <
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value <
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value <
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };

                case MSAst.ExpressionType.GreaterThan:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value >
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value >
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value >
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value >
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value >
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value >
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };

                case MSAst.ExpressionType.GreaterThanOrEqual:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value >=
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value >=
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value >=
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value >=
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value >=
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value >=
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };

                case MSAst.ExpressionType.LessThanOrEqual:
                    if (!DoBinaryNumericPromotions(ref left, ref right))
                    {
                        return null;
                    }

                    if (left is ConstantExpression<double>)
                    {
                        booleanResult = ((ConstantExpression<double>)left).Value <=
                            ((ConstantExpression<double>)right).Value;
                    }
                    else if (left is ConstantExpression<float>)
                    {
                        booleanResult = ((ConstantExpression<float>)left).Value <=
                            ((ConstantExpression<float>)right).Value;
                    }
                    else if (left is ConstantExpression<ulong>)
                    {
                        booleanResult = ((ConstantExpression<ulong>)left).Value <=
                            ((ConstantExpression<ulong>)right).Value;
                    }
                    else if (left is ConstantExpression<long>)
                    {
                        booleanResult = ((ConstantExpression<long>)left).Value <=
                            ((ConstantExpression<long>)right).Value;
                    }
                    else if (left is ConstantExpression<uint>)
                    {
                        booleanResult = ((ConstantExpression<uint>)left).Value <=
                            ((ConstantExpression<uint>)right).Value;
                    }
                    else if (left is ConstantExpression<int>)
                    {
                        booleanResult = ((ConstantExpression<int>)left).Value <=
                            ((ConstantExpression<int>)right).Value;
                    }
                    else
                        return null;

                    return new ConstantExpression<bool>(booleanResult) { Span = left.Span };
            }

            return null;
        }
    }
}