using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class ConditionalExpression : Expression
    {
        public Expression Test { get; set; }
        public Expression IfTrue { get; set; }
        public Expression IfFalse { get; set; }

        public override bool ShouldParenthesize => true;

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is ConditionalExpression clone))
            {
                return;
            }

            clone.Test = Clone(cloneContext, Test);
            clone.IfTrue = Clone(cloneContext, IfTrue);
            clone.IfFalse = Clone(cloneContext, IfFalse);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            Test = Test.Resolve(parseContext);
            IfTrue = IfTrue.Resolve(parseContext);
            IfFalse = IfFalse.Resolve(parseContext);

            if (Test == null || IfTrue == null || IfFalse == null)
            {
                return null;
            }

            ExpressionClass = ExpressionClass.Value;

            System.Type trueType = IfTrue.Type;
            System.Type falseType = IfFalse.Type;

            Type = trueType;

            // First, if an implicit conversion exists from IfTrue to IfFalse, then the result type is of type IfFalse.Type
            if (!TypeManager.IsEqual(trueType, falseType))
            {
                Expression conv = ConvertExpression.MakeImplicitConversion(parseContext, IfTrue, falseType, Span);
                if (conv != null)
                {
                    // Check if both can convert implicitly to each other's type
                    Type = falseType;

                    if (ConvertExpression.MakeImplicitConversion(parseContext, IfFalse, trueType, Span) != null)
                    {
                        parseContext.ReportError(
                            CompilerErrors.ConditionalOperandsBothHaveImplicitConversions,
                            IfTrue.Span,
                            trueType,
                            falseType);

                        return null;
                    }

                    IfTrue = conv;
                }
                else if ((conv = ConvertExpression.MakeImplicitConversion(parseContext, IfFalse, trueType, Span)) != null)
                {
                    IfFalse = conv;
                }
                else
                {
                    parseContext.ReportError(
                        CompilerErrors.ConditionalOperandsHaveNoImplicitConversion,
                        IfTrue.Span,
                        trueType,
                        falseType);

                    return null;
                }
            }

            // Dead code optimalization
            if (Test is ConstantExpression c)
            {
                bool isFalse = c.IsDefaultValue;

                parseContext.ReportError(
                    CompilerErrors.UnreachableExpression,
                    isFalse ? IfTrue.Span : IfFalse.Span);

                return ReducedExpression.Create(isFalse ? IfFalse : IfTrue, this).Resolve(parseContext);
            }

            return this;
        }

        public override TypeExpression ResolveAsTypeTerminal(ParseContext ec, bool silent)
        {
            return null;
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Condition(
                Test.Transform(generator),
                IfTrue.Transform(generator),
                IfFalse.Transform(generator));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            Test.Dump(sw, indentChange);
            sw.Write(" ? ");
            IfTrue.Dump(sw, indentChange);
            sw.Write(" : ");
            IfFalse.Dump(sw, indentChange);
        }
    }
}