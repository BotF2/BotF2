using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class LiftedNullExpression : ConstantExpression
    {
        private LiftedNullExpression(Type nullableType, SourceSpan location)
            : base(nullableType, null)
        {
            ExpressionClass = ExpressionClass.Value;
            Span = location;
        }

        public static ConstantExpression Create(Type nullable, SourceSpan location)
        {
            return new LiftedNullExpression(nullable, location);
        }

        public static Expression CreateFromExpression(ParseContext ec, Expression e)
        {
            ec.ReportError(
                458,
                string.Format(
                    "The result of the expression is always 'null' of type '{0}'.",
                    TypeManager.GetCSharpName(e.Type)),
                Severity.Warning,
                e.Span);

            return ReducedExpression.Create(
                Create(e.Type, e.Span),
                e);
        }
    }
}