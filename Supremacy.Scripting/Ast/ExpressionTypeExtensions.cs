using System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public static class ExpressionTypeExtensions
    {
        public static bool IsComparison(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Equal ||
                   expressionType == ExpressionType.NotEqual ||
                   expressionType == ExpressionType.GreaterThan ||
                   expressionType == ExpressionType.GreaterThanOrEqual ||
                   expressionType == ExpressionType.LessThan ||
                   expressionType == ExpressionType.LessThanOrEqual;
        }

        public static bool IsEquality(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Equal ||
                   expressionType == ExpressionType.NotEqual;
        }

        public static bool IsLogical(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.AndAlso ||
                   expressionType == ExpressionType.OrElse;
        }

        public static bool IsShift(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.LeftShift ||
                   expressionType == ExpressionType.RightShift;
        }

        public static bool IsAddition(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Add;
        }

        public static bool IsSubtraction(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Subtract;
        }

        public static bool IsRelational(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.GreaterThan ||
                   expressionType == ExpressionType.GreaterThanOrEqual ||
                   expressionType == ExpressionType.LessThan ||
                   expressionType == ExpressionType.LessThanOrEqual;
        }

        public static bool IsBitwise(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.And ||
                   expressionType == ExpressionType.Or ||
                   expressionType == ExpressionType.ExclusiveOr ||
                   expressionType == ExpressionType.Not;
        }

        public static bool IsArithmetic(this ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Add ||
                   expressionType == ExpressionType.Subtract ||
                   expressionType == ExpressionType.Modulo ||
                   expressionType == ExpressionType.Multiply ||
                   expressionType == ExpressionType.Divide;
        }

        public static bool Matches(this ExpressionType expressionType, OperatorFlags flags)
        {
            return (((flags & OperatorFlags.Addition) != 0) && expressionType.IsAddition()) ||
                   (((flags & OperatorFlags.Arithmetic) != 0) && expressionType.IsArithmetic()) ||
                   (((flags & OperatorFlags.Bitwise) != 0) && expressionType.IsBitwise()) ||
                   (((flags & OperatorFlags.Comparison) != 0) && expressionType.IsComparison()) ||
                   (((flags & OperatorFlags.Equality) != 0) && expressionType.IsEquality()) ||
                   (((flags & OperatorFlags.Logical) != 0) && expressionType.IsLogical()) ||
                   (((flags & OperatorFlags.Relational) != 0) && expressionType.IsRelational()) ||
                   (((flags & OperatorFlags.Shift) != 0) && expressionType.IsShift()) ||
                   (((flags & OperatorFlags.Subtraction) != 0) && expressionType.IsSubtraction());
        }
    }
}