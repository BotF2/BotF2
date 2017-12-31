using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class ConditionalLogicalOperator : UserOperatorCall
    {
        public ConditionalLogicalOperator(MethodGroupExpression operMethod, Arguments arguments, SourceSpan loc)
            : base(operMethod, arguments, loc)
        {}

        internal ConditionalLogicalOperator()
        {
            // For cloning purposes only.
        }

        public override Expression DoResolve(ParseContext ec)
        {
            var method = (MethodInfo)Method;

            Type = method.ReturnType;

            var parameterDatapd = TypeManager.GetParameterData(method);

            if (!TypeManager.IsEqual(Type, Type) ||
                !TypeManager.IsEqual(Type, parameterDatapd.Types[0]) || 
                !TypeManager.IsEqual(Type, parameterDatapd.Types[1]))
            {
                ec.ReportError(
                    217,
                    string.Format(
                        "A user-defined operator '{0}' must have parameters and return values of " +
                        "the same type in order to be applicable as a short circuit operator.",
                        TypeManager.GetCSharpSignature(method)),
                    Span);
                return null;
            }

            var leftCopy = (Expression)new EmptyExpression(Type);

            var opTrue = GetOperatorTrue(ec, leftCopy, Span);
            var opFalse = GetOperatorFalse(ec, leftCopy, Span);

            if (opTrue == null || opFalse == null)
            {
                ec.ReportError(
                    218,
                    string.Format(
                        "The type '{0}' must have operator 'true' and operator 'false' " +
                        "defined when '{1}' is used as a short circuit operator.",
                        TypeManager.GetCSharpName(Type),
                        TypeManager.GetCSharpSignature(method)),
                    Span);

                return null;
            }

            ExpressionClass = ExpressionClass.Value;
            
            return this;
        }
    }
}