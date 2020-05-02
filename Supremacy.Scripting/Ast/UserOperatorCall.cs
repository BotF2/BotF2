using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class UserOperatorCall : Expression
    {
        public delegate Expression ExpressionTreeExpression(ParseContext ec, MethodGroupExpression mg);

        public UserOperatorCall(MethodGroupExpression method, Arguments args, SourceSpan span)
        {
            Method = method;
            Arguments = args;

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

            if (!(target is UserOperatorCall clone))
            {
                return;
            }

            clone.Arguments = Arguments.Clone(cloneContext);
            clone.Method = Clone(cloneContext, Method);
        }

        public override Expression DoResolve(ParseContext ec)
        {
            //
            // We are born fully resolved
            //
            return this;
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            return MSAst.Expression.Call(
                (MethodInfo)Method,
                Arguments.Transform(generator));
        }

        protected internal MethodGroupExpression Method { get; private set; }

        protected internal Arguments Arguments { get; private set; }
    }
}