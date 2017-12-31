using System;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class DefaultValueExpression : Expression
    {
        private Expression _typeExpression;
        private Type _resolvedType;

        public DefaultValueExpression() {}

        public DefaultValueExpression([NotNull] Expression typeExpression)
        {
            if (typeExpression == null)
                throw new ArgumentNullException("typeExpression");
            _typeExpression = typeExpression;
        }

        public DefaultValueExpression([NotNull] Type resolvedType)
        {
            if (resolvedType == null)
                throw new ArgumentNullException("resolvedType");
            _resolvedType = resolvedType;
            Type = resolvedType;
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as DefaultValueExpression;
            if (clone == null)
                return;

            clone._typeExpression = Clone(cloneContext, _typeExpression);
            clone._resolvedType = _resolvedType;
        }

        public Expression TypeExpression
        {
            get { return _typeExpression; }
            set { _typeExpression = value; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            return MSAst.Default(_resolvedType ?? TypeManager.CoreTypes.Object);
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _typeExpression, prefix, postfix);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            if (_typeExpression == null)
                return this;

            var typeExpression = _typeExpression.ResolveAsTypeTerminal(parseContext, false);
            if (typeExpression == null)
                return null;

            _resolvedType = typeExpression.Type;

            if (_resolvedType.IsAbstract && _resolvedType.IsSealed)
            {
                parseContext.ReportError(
                    -244, 
                    "The 'default value' operator cannot be applied to an operand of a static type.",
                    Span);
            }

            if (TypeManager.IsReferenceType(_resolvedType))
                return ConstantExpression.Create(_resolvedType, null, Span);

            Type = _resolvedType;
            ExpressionClass = ExpressionClass.Variable;

            return this;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write("default(");
            TypeExpression.Dump(sw, indentChange);
            sw.Write(")");
        }
    }
}