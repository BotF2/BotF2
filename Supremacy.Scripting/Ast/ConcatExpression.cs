using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class ConcatExpression : Expression
    {
        private Arguments _arguments;

        public ConcatExpression(SourceSpan loc, Expression left, Expression right)
        {
            Span = loc;
            Type = TypeManager.CoreTypes.String;
            ExpressionClass = ExpressionClass.Value;

            _arguments = new Arguments(2);

            Append(left);
            Append(right);
        }

        internal ConcatExpression()
        {
            // For cloning purposes only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ConcatExpression;
            if (clone == null)
                return;

            clone._arguments = _arguments.Clone(cloneContext);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            return this;
        }

        public void Append(Expression operand)
        {
            //
            // Constant folding
            //
            var sc = operand as ConstantExpression<string>;
            if (sc != null)
            {
                if (_arguments.Count != 0)
                {
                    var lastArgument = _arguments[_arguments.Count - 1];
                    var lastExprConstant = lastArgument.Value as ConstantExpression<string>;

                    if (lastExprConstant != null)
                    {
                        lastArgument.Value = new ConstantExpression<string>(
                            lastExprConstant.Value + sc.Value,
                            sc.Span);

                        return;
                    }
                }
            }
            else
            {
                //
                // Multiple (3+) concatenation are resolved as multiple StringConcat instances
                //
                var concatOper = operand as ConcatExpression;
                if (concatOper != null)
                {
                    _arguments.AddRange(concatOper._arguments);
                    return;
                }
            }

            _arguments.Add(new Argument(operand));
        }

		public override MSAst.Expression TransformCore (ScriptGenerator generator)
		{
            //if (_arguments.Count != 2)
            //    throw new InvalidOperationException("arguments.Count != 2");

            return Microsoft.Scripting.Ast.Utils.ComplexCallHelper(
                CommonMembers.StringConcat,
                _arguments.Transform(generator));

            //return MSAst.Expression.Add(
            //    _arguments[0].Value.Transform(generator),
            //    _arguments[1].Value.Transform(generator),
            //    CommonMembers.StringConcat);
		}

        public override void Dump(SourceWriter sw, int indentChange)
        {
            int i = 0;
            foreach (var argument in _arguments)
            {
                if (i++ != 0)
                    sw.Write(" + ");
                if (!argument.Value.IsPrimaryExpression)
                    sw.Write("(");
                DumpChild(argument.Value, sw, indentChange);
                if (!argument.Value.IsPrimaryExpression)
                    sw.Write(")");
            }
        }
    }
}