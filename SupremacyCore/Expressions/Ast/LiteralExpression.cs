using System;

using M = Microsoft.Modeling.Languages;
using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Expressions.Ast
{
    public enum LiteralKind
    {
        Null,
        Integer,
        Real,
        Text,
        Character,
        Logical
    }

    public class LiteralExpression : Expression
    {
        public LiteralKind Kind { get; set; }
        public string Text { get; set; }
        public TypeName Type { get; set; }

        public override MSAst TransformCore(M.CompilerContext context)
        {
            object literalValue;
            Type literalType;              
            BuiltinType? builtinType;

            var literalText = this.Text;
            var literalTypeName = this.Type;

            if (literalText == null)
            {
                context.Error(
                    this.sourceLocationIncludingLeadingWhitespace,
                    "Literal expression cannot have null text value.");
                return null;
            }

            if (TypeHelper.TryParseLiteral(literalText, literalTypeName, out literalValue, out literalType, out builtinType))
            {
                return MSAst.Constant(
                    literalValue,
                    literalType);
            }

            var typeName = (string)null;

            if (builtinType.HasValue)
                typeName = builtinType.Value.ToString();
            else if (literalType != null)
                typeName = literalType.Name;
            else if (this.Type != null)
                typeName = this.Type.Name;

            if (typeName != null)
            {
                context.Error(
                    this.sourceLocationIncludingLeadingWhitespace,
                    "Invalid literal value: \"{0}:\"; expected literal of type {1}.",
                    literalText,
                    typeName);
            }
            else
            {
                context.Error(
                    this.sourceLocationIncludingLeadingWhitespace,
                    "Unexpected literal expression: \"{0}:\"; unable to determine type.",
                    literalText,
                    typeName);
            }

            return null;
        }
    }
}