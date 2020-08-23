using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Supremacy.Scripting.Utility
{
    /// <summary>OperatorInfo provides a mapping from DLR ExpressionType to their associated .NET methods.</summary>
    internal class OperatorInfo
    {
        private static readonly Dictionary<string, OperatorInfo> _operatorsByMethod;
        private static readonly OperatorInfo[] _operators;

        static OperatorInfo()
        {
            _operators = new List<OperatorInfo>
                         {
                             new OperatorInfo(
                                 ExpressionType.Decrement,
                                 "op_Decrement",
                                 "--"),
                             new OperatorInfo(
                                 ExpressionType.Increment,
                                 "op_Increment",
                                 "++"),
                             new OperatorInfo(
                                 ExpressionType.Negate,
                                 "op_UnaryNegation",
                                 "-"),
                             new OperatorInfo(
                                 ExpressionType.UnaryPlus,
                                 "op_UnaryPlus",
                                 "+"),
                             new OperatorInfo(
                                 ExpressionType.Not,
                                 "op_LogicalNot",
                                 "-"),
                             new OperatorInfo(
                                 ExpressionType.IsTrue,
                                 "op_True",
                                 "true"),
                             new OperatorInfo(
                                 ExpressionType.IsFalse,
                                 "op_False",
                                 "false"),
                             new OperatorInfo(
                                 ExpressionType.OnesComplement,
                                 "op_OnesComplement",
                                 "~"),
                             new OperatorInfo(
                                 ExpressionType.Add,
                                 "op_Addition",
                                 "+"),
                             new OperatorInfo(
                                 ExpressionType.Subtract,
                                 "op_Subtraction",
                                 "-"),
                             new OperatorInfo(
                                 ExpressionType.Multiply,
                                 "op_Multiply",
                                 "*"),
                             new OperatorInfo(
                                 ExpressionType.Divide,
                                 "op_Division",
                                 "/"),
                             new OperatorInfo(
                                 ExpressionType.Modulo,
                                 "op_Modulus",
                                 "%"),
                             new OperatorInfo(
                                 ExpressionType.ExclusiveOr,
                                 "op_ExclusiveOr",
                                 "^"),
                             new OperatorInfo(
                                 ExpressionType.And,
                                 "op_BitwiseAnd",
                                 "&"),
                             new OperatorInfo(
                                 ExpressionType.Or,
                                 "op_BitwiseOr",
                                 "|"),
                             new OperatorInfo(
                                 ExpressionType.AndAlso,
                                 "op_LogicalAnd",
                                 "&&"),
                             new OperatorInfo(
                                 ExpressionType.OrElse,
                                 "op_LogicalOr",
                                 "||"),
                             new OperatorInfo(
                                 ExpressionType.LeftShift,
                                 "op_LeftShift",
                                 "<<"),
                             new OperatorInfo(
                                 ExpressionType.RightShift,
                                 "op_RightShift",
                                 ">>"),
                             new OperatorInfo(
                                 ExpressionType.Equal,
                                 "op_Equality",
                                 "=="),
                             new OperatorInfo(
                                 ExpressionType.GreaterThan,
                                 "op_GreaterThan",
                                 ">"),
                             new OperatorInfo(
                                 ExpressionType.LessThan,
                                 "op_LessThan",
                                 "<"),
                             new OperatorInfo(
                                 ExpressionType.NotEqual,
                                 "op_Inequality",
                                 "!="),
                             new OperatorInfo(
                                 ExpressionType.GreaterThanOrEqual,
                                 "op_GreaterThanOrEqual",
                                 ">="),
                             new OperatorInfo(
                                 ExpressionType.LessThanOrEqual,
                                 "op_LessThanOrEqual",
                                 "<="),
                             new OperatorInfo(
                                 ExpressionType.MultiplyAssign,
                                 "op_MultiplicationAssignment",
                                 "*="),
                             new OperatorInfo(
                                 ExpressionType.SubtractAssign,
                                 "op_SubtractionAssignment",
                                 "-="),
                             new OperatorInfo(
                                 ExpressionType.ExclusiveOrAssign,
                                 "op_ExclusiveOrAssignment",
                                 "^="),
                             new OperatorInfo(
                                 ExpressionType.LeftShiftAssign,
                                 "op_LeftShiftAssignment",
                                 "<<="),
                             new OperatorInfo(
                                 ExpressionType.RightShiftAssign,
                                 "op_RightShiftAssignment",
                                 ">>="),
                             new OperatorInfo(
                                 ExpressionType.ModuloAssign,
                                 "op_ModulusAssignment",
                                 "%="),
                             new OperatorInfo(
                                 ExpressionType.AddAssign,
                                 "op_AdditionAssignment",
                                 "+="),
                             new OperatorInfo(
                                 ExpressionType.AndAssign,
                                 "op_BitwiseAndAssignment",
                                 "&="),
                             new OperatorInfo(
                                 ExpressionType.OrAssign,
                                 "op_BitwiseOrAssignment",
                                 "|="),
                             new OperatorInfo(
                                 ExpressionType.DivideAssign,
                                 "op_DivisionAssignment",
                                 "/=")
                         }.ToArray();

            _operatorsByMethod = _operators.ToDictionary(o => o.Name, o => o);
        }

        private OperatorInfo(ExpressionType op, string name, string signatureName)
        {
            Operator = op;
            Name = name;
            SignatureName = signatureName;
        }

        /// <summary>The operator the OperatorInfo provides info for.</summary>
        public ExpressionType Operator { get; }

        /// <summary>
        ///   The primary method name associated with the method.  This method name is
        ///   usally in the form of op_Operator (e.g. op_Addition).
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///   The secondary method name associated with the method.  This method name is
        ///   usually a standard .NET method name with pascal casing (e.g. Add).
        /// </summary>
        public string SignatureName { get; }

        /// <summary>Given an operator returns the OperatorInfo associated with the operator or null</summary>
        public static OperatorInfo GetOperatorInfo(ExpressionType op)
        {
            return _operators.FirstOrDefault(info => info.Operator == op);
        }

        public static OperatorInfo GetOperatorInfo(string name)
        {
            return _operatorsByMethod.TryGetValue(name, out OperatorInfo operatorInfo) ? operatorInfo : null;
        }

        public static string GetSignatureName(string name)
        {
            if (name == "op_Implicit")
            {
                return "implicit";
            }

            if (name == "op_Explicit")
            {
                return "explicit";
            }

            return _operatorsByMethod.TryGetValue(name, out OperatorInfo operatorInfo) ? operatorInfo.SignatureName : null;
        }
    }
}