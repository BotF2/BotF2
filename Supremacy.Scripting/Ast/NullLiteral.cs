using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class NullLiteral : ConstantExpression<DynamicNull>
    {
        // Default type of null is an object
        public NullLiteral(SourceSpan location)
            : base(null)
        {
            Span = location;
        }

        public override bool IsNull
        {
            get { return true; }
        }

        public override bool IsZeroInteger
        {
            get { return true; }
        }

        public override void OnErrorValueCannotBeConverted(ParseContext ec, SourceSpan location, Type target, bool expl)
        {
            if (TypeManager.IsGenericParameter(target))
            {
                ec.ReportError(
                    403,
                    string.Format(
                        "Cannot convert null to the type parameter '{0}' because it could be a value " +
                        "type. Consider using 'default ({0})' instead.",
                        target.Name),
                    location);

                return;
            }

            if (TypeManager.IsValueType(target))
            {
                ec.ReportError(
                    37,
                    string.Format(
                        "Cannot convert null to .{0}' because it is a value type.",
                        TypeManager.GetCSharpName(target)),
                    location);

                return;
            }

            base.OnErrorValueCannotBeConverted(ec, location, target, expl);
        }

        public override ConstantExpression ConvertImplicitly(Type targetType)
        {
            // Null literal is of object type
            if (targetType == TypeManager.CoreTypes.Object)
                return this;

            return base.ConvertImplicitly(targetType);
        }

		public override System.Linq.Expressions.Expression TransformCore (ScriptGenerator generator)
		{
		    return System.Linq.Expressions.Expression.Constant(null);
		}
    }
}