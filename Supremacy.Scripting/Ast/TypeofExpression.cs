using System;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class TypeofExpression : Expression
    {
        private Expression _typeExpression;

        public Expression TypeExpression
        {
            get => _typeExpression;
            set => _typeExpression = value;
        }

        public override bool IsPrimaryExpression => true;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _typeExpression, prefix, postfix);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _typeExpression = _typeExpression.ResolveAsTypeStep(parseContext, false);
            
            if (_typeExpression == null)
            {
                return null;
            }

            Type = _typeExpression.Type;

            return this;
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Constant(
                Type,
                typeof(Type));
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("typeof(");
            DumpChild(_typeExpression, sw, indentChange);
            sw.Write(")");
        }
    }
}