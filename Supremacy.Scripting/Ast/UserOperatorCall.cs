using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class UserOperatorCall : Expression
    {
        public delegate Expression ExpressionTreeExpression(ParseContext ec, MethodGroupExpression mg);

        private Arguments _arguments;
        private MethodGroupExpression _method;

        public UserOperatorCall(MethodGroupExpression method, Arguments args, SourceSpan span)
        {
            _method = method;
            _arguments = args;

            Type = ((MethodInfo)method).ReturnType;
            ExpressionClass = ExpressionClass.Value;
            Span = span;
        }

        internal UserOperatorCall()
        {
            // For cloning purposes only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as UserOperatorCall;
            if (clone == null)
                return;

            clone._arguments = _arguments.Clone(cloneContext);
            clone._method = Clone(cloneContext, _method);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            //
            // We are born fully resolved
            //
            return this;
        }

		public override MSAst.Expression TransformCore (ScriptGenerator generator)
		{
		    return MSAst.Expression.Call(
		        (MethodInfo)_method,
		        _arguments.Transform(generator));
		}

        protected internal MethodGroupExpression Method
        {
            get { return _method; }
        }

        protected internal Arguments Arguments
        {
            get { return _arguments; }
        }
    }
}