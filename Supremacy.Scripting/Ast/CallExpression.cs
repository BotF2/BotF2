using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;
using MSAstUtil = Microsoft.Scripting.Ast.Utils;

namespace Supremacy.Scripting.Ast
{
    public class CallExpression : Expression
    {
        private Arguments _arguments;
        private TypeArguments _typeArguments;
        private MethodInfo _method;
        private Expression _target;

        public CallExpression()
        {
            _typeArguments = new TypeArguments();
            _arguments = new Arguments();
        }

        public CallExpression(string methodName, Expression target, IEnumerable<Argument> arguments)
            : this()
        {
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            if (target == null)
                throw new ArgumentNullException("target");

            MethodName = methodName;
            Target = target;

            if (arguments != null)
                Arguments.AddRange(arguments);
        }

        public CallExpression(MethodInfo method, Expression target, IEnumerable<Argument> arguments)
            : this()
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (target == null)
                throw new ArgumentNullException("target");

            Method = method;
            Target = target;

            if (arguments != null)
                Arguments.AddRange(arguments);
        }

        public MethodInfo Method
        {
            get { return _method; }
            set
            {
                _method = value;
                Type = (_method == null) ? null : _method.ReturnType;
            }
        }

        public Expression Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public string MethodName { get; set; }

        public virtual TypeArguments TypeArguments
        {
            get
            {
                var memberAccess = Target as MemberAccessExpression;
                if (memberAccess != null)
                    return memberAccess.TypeArguments;
                return _typeArguments;
            }
        }

        public Arguments Arguments
        {
            get { return _arguments; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _target, prefix, postfix);

            for (var i = 0; i < _arguments.Count; i++)
            {
                var argument = _arguments[i];
                Walk(ref argument, prefix, postfix);
                _arguments[i] = argument;
            }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            var target = Target;
            var type = (Type)null;
            var targetType = target as TypeExpression;

            MSAst transformedTarget;

            if (targetType != null)
            {
                type = targetType.Type;
                transformedTarget = MSAst.Constant(type, typeof(Type));
            }
            else
            {
                transformedTarget = target.Transform(generator);
            }

            var method = Method;
            if (method != null)
            {
                return MSAstUtil.ComplexCallHelper(
                    (type != null) ? null : transformedTarget,
                    method,
                    Arguments.Select(o => o.Value.Transform(generator)).ToArray());
            }

            if (type != null)
            {
                return MSAst.Call(
                    type,
                    MethodName,
                    TypeArguments.ResolvedTypes,
                    _arguments.Transform(generator));
            }

            return MSAst.Call(
                transformedTarget,
                MethodName,
                TypeArguments.ResolvedTypes,
                _arguments.Transform(generator));
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as CallExpression;
            if (clone == null)
                return;

            clone._arguments = _arguments.Clone(cloneContext);
            clone._typeArguments = _typeArguments.Clone();
            clone._target = Clone(cloneContext, _target);
            clone._method = _method;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var parenthesize = !_target.IsPrimaryExpression;

            if (parenthesize)
                sw.Write("(");

            _target.Dump(sw, indentChange);

            if (parenthesize)
                sw.Write(")");

            sw.Write(".");
            sw.Write(Method != null ? Method.Name : MethodName);

            if (_typeArguments.Count != 0)
            {
                sw.Write("<");
                for (var i = 0; i < _typeArguments.Count; i++)
                {
                    var typeArgument = _typeArguments[i];

                    if (i != 0)
                        sw.Write(", ");

                    typeArgument.Dump(sw, indentChange);
                }
                sw.Write(">");
            }

            sw.Write("(");

            for (var i = 0; i < _arguments.Count; i++)
            {
                var argument = _arguments[i];

                if (i != 0)
                    sw.Write(", ");

                argument.Dump(sw, indentChange);
            }

            sw.Write(")");
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            var targetResolved = Target.Resolve(
                parseContext,
                ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);

            if (targetResolved == null)
                return null;

            Target = targetResolved;

            var arguments = Arguments;
            if (arguments != null)
                _arguments.Resolve(parseContext);

            _typeArguments.Resolve(parseContext);

            return this;
        }
    }
}