using System;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
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
        private FullNamedExpression _literalType;
        private object _value;

        public LiteralKind Kind { get; set; }
        public string Text { get; set; }

        public FullNamedExpression LiteralType
        {
            get { return _literalType; }
            set { _literalType = value; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override bool IsNull
        {
            get { return (Kind == LiteralKind.Null); }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _literalType, prefix, postfix);
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            if (Kind == LiteralKind.Null)
                return MSAst.Default(typeof(object));
            return MSAst.Constant(
                _value,
                Type);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Text);
        }

        public override Expression DoResolve(ParseContext rc)
        {
            if (Kind == LiteralKind.Null)
                return new DefaultValueExpression(typeof(object));

            var literalText = Text;
            if (literalText == null)
            {
                rc.Compiler.Errors.Add(
                    rc.Compiler.SourceUnit,
                    "Literal expression cannot have null text value.",
                    Span,
                    -1,
                    Severity.Error);

                return null;
            }

            _literalType = _literalType.ResolveAsTypeStep(rc, false);

            if ((_literalType == null) || (_literalType.Type == null))
                return null;

            if (Kind == LiteralKind.Text)
                return new InterpolatedStringExpression(this).Resolve(rc);

            var builtinType = TypeManager.GetBuiltinTypeFromClrType(_literalType.Type);

            if (!builtinType.HasValue)
            {
                rc.ReportError(
                    CompilerErrors.LiteralValueMustBeBuiltinType,
                    Span,
                    TypeManager.GetCSharpName(_literalType.Type));

                return null;
            }

            Type type;

            if (TypeManager.TryParseLiteral(
                builtinType.Value,
                literalText,
                out _value,
                out type))
            {
                Type = type;

                return ConstantExpression.Create(
                    Type,
                    _value,
                    Span);
            }

            rc.ReportError(
                CompilerErrors.InvalidLiteralValue,
                Span,
                literalText,
                TypeManager.GetCSharpName(_literalType.Type));

            return null;
        }
    }
}