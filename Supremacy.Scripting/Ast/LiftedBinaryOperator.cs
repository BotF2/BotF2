using System;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class LiftedBinaryOperator : BinaryExpression
    {
        private Unwrap left_unwrap, right_unwrap;
        private bool left_null_lifted, right_null_lifted;
        private Expression left_orig, right_orig;
        private Expression user_operator;
        private ConstructorInfo wrap_ctor;

        public LiftedBinaryOperator(ExpressionType op, Expression left, Expression right, SourceSpan span)
        {
            Operator = op;
            Left = left;
            Right = right;
            Span = span;
        }

        //
        // CSC 2 has this behavior, it allows structs to be compared
        // with the null literal *outside* of a generics context and
        // inlines that as true or false.
        //
        private Expression CreateNullConstant(ParseContext ec, Expression expr)
        {
            // FIXME: Handle side effect constants
            ConstantExpression<bool> c = new ConstantExpression<bool>(Operator == ExpressionType.NotEqual, Span);

            if (Operator.IsEquality())
            {
                ec.ReportError(
                    472,
                    string.Format(
                        "The result of comparing value type '{0}' with null is '{1}'.",
                        TypeManager.GetCSharpName(expr.Type),
                        c),
                    Severity.Warning,
                    Span);
            }
            else
            {
                ec.ReportError(
                    464,
                    string.Format(
                        "The result of comparing type '{0}' with null is always '{1}'.",
                        TypeManager.GetCSharpName(expr.Type),
                        c),
                    Severity.Warning,
                    Span);
            }

            return ReducedExpression.Create(c, this);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            if (ExpressionClass != ExpressionClass.Invalid)
            {
                return this;
            }

            if (Operator.IsLogical())
            {
                OnErrorOperatorCannotBeApplied(ec, Left, Right, Operator, Span);
                return null;
            }

            bool use_default_call = Operator.IsBitwise() || Operator.IsEquality();
            left_orig = Left;
            if (TypeManager.IsNullableType(Left.Type))
            {
                Left = left_unwrap = Unwrap.Create(Left, use_default_call);
                if (Left == null)
                {
                    return null;
                }
            }

            right_orig = Right;
            if (TypeManager.IsNullableType(Right.Type))
            {
                Right = right_unwrap = Unwrap.Create(Right, use_default_call);
                if (Right == null)
                {
                    return null;
                }
            }

            //
            // Some details are in 6.4.2, 7.2.7
            // Arguments can be lifted for equal operators when the return type is bool and both
            // arguments are of same type
            //	
            if (left_orig.IsNull)
            {
                Left = Right;
                left_null_lifted = true;
                ResultType = TypeManager.CoreTypes.Boolean;
            }

            if (right_orig.IsNull)
            {
                Right = Left;
                right_null_lifted = true;
                ResultType = TypeManager.CoreTypes.Boolean;
            }

            ExpressionClass = ExpressionClass.Value;
            return DoResolveCore(ec, left_orig, right_orig);
        }

        bool IsBitwiseBoolean
        {
            get
            {
                return Operator.IsBitwise() && left_unwrap != null && right_unwrap != null &&
                       left_unwrap.Type == TypeManager.CoreTypes.Boolean &&
                       right_unwrap.Type == TypeManager.CoreTypes.Boolean;
            }
        }

        private Expression LiftResult(ParseContext ec, Expression res_expr)
        {
            TypeExpression lifted_type;

            //
            // Avoid double conversion
            //
            if (left_unwrap == null || left_null_lifted || !TypeManager.IsEqual(left_unwrap.Type, Left.Type) || (left_unwrap != null && right_null_lifted))
            {
                lifted_type = new NullableTypeExpression(Left.Type, Span);
                lifted_type = lifted_type.ResolveAsTypeTerminal(ec, false);
                if (lifted_type == null)
                {
                    return null;
                }

                if (Left is CastExpression)
                {
                    ((CastExpression)Left).DestinationType = new TypeExpression(lifted_type.Type);
                }
                else
                {
                    Left = EmptyCastExpression.Create(Left, lifted_type.Type);
                }
            }

            if (right_unwrap == null || right_null_lifted || !TypeManager.IsEqual(right_unwrap.Type, Right.Type) || (right_unwrap != null && left_null_lifted))
            {
                lifted_type = new NullableTypeExpression(Right.Type, Span);
                lifted_type = lifted_type.ResolveAsTypeTerminal(ec, false);

                if (lifted_type == null)
                {
                    return null;
                }

                if (Right is CastExpression)
                {
                    ((CastExpression)Right).DestinationType = new TypeExpression(lifted_type.Type);
                }
                else
                {
                    Right = EmptyCastExpression.Create(Right, lifted_type.Type);
                }
            }

            if (!Operator.IsComparison())
            {
                lifted_type = new NullableTypeExpression(res_expr.Type, Span);
                lifted_type = lifted_type.ResolveAsTypeTerminal(ec, false);

                if (lifted_type == null)
                {
                    return null;
                }

                wrap_ctor = new NullableInfo(lifted_type.Type).Constructor;
                ResultType = res_expr.Type = lifted_type.Type;
            }

            if (left_null_lifted)
            {
                Left = LiftedNullExpression.Create(Right.Type, Left.Span);

                if (Operator.IsArithmetic() || Operator.IsShift() || Operator.IsBitwise())
                {
                    return LiftedNullExpression.CreateFromExpression(ec, res_expr);
                }

                //
                // Value types and null comparison
                //
                if (right_unwrap == null || Operator.IsRelational())
                {
                    return CreateNullConstant(ec, right_orig).Resolve(ec);
                }
            }

            if (right_null_lifted)
            {
                Right = LiftedNullExpression.Create(Left.Type, Right.Span);

                if (Operator.IsArithmetic() || Operator.IsShift() || Operator.IsBitwise())
                {
                    return LiftedNullExpression.CreateFromExpression(ec, res_expr);
                }

                //
                // Value types and null comparison
                //
                if (left_unwrap == null || Operator.IsRelational())
                {
                    return CreateNullConstant(ec, left_orig).Resolve(ec);
                }
            }

            return res_expr;
        }

        protected override Expression ResolveOperatorPredefined(ParseContext ec, PredefinedOperator[] operators, bool primitives_only, Type enum_type)
        {
            Expression e = base.ResolveOperatorPredefined(ec, operators, primitives_only, enum_type);

            if (e == this || enum_type != null)
            {
                return LiftResult(ec, e);
            }

            //
            // 7.9.9 Equality operators and null
            //
            // The == and != operators permit one operand to be a value of a nullable type and
            // the other to be the null literal, even if no predefined or user-defined operator
            // (in unlifted or lifted form) exists for the operation.
            //
            if (e == null && Operator.IsEquality())
            {
                if ((left_null_lifted && right_unwrap != null) || (right_null_lifted && left_unwrap != null))
                {
                    return LiftResult(ec, this);
                }
            }

            return e;
        }

        protected override Expression ResolveUserOperator(ParseContext ec, Type l, Type r)
        {
            Expression expr = base.ResolveUserOperator(ec, l, r);
            if (expr == null)
            {
                return null;
            }

            expr = LiftResult(ec, expr);
            if (expr is ConstantExpression)
            {
                return expr;
            }

            ResultType = expr.Type;
            user_operator = expr;

            return this;
        }
    }
}