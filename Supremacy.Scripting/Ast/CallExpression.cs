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
        private TypeArguments _typeArguments;
        private MethodInfo _method;
        private Expression _target;

        public CallExpression()
        {
            _typeArguments = new TypeArguments();
            Arguments = new Arguments();
        }

        public CallExpression(string methodName, Expression target, IEnumerable<Argument> arguments)
            : this()
        {
            MethodName = methodName ?? throw new ArgumentNullException("methodName");
            Target = target ?? throw new ArgumentNullException("target");

            if (arguments != null)
            {
                Arguments.AddRange(arguments);
            }
        }

        public CallExpression(MethodInfo method, Expression target, IEnumerable<Argument> arguments)
            : this()
        {
            Method = method ?? throw new ArgumentNullException("method");
            Target = target ?? throw new ArgumentNullException("target");

            if (arguments != null)
            {
                Arguments.AddRange(arguments);
            }
        }

        public MethodInfo Method
        {
            get => _method;
            set
            {
                _method = value;
                Type = _method?.ReturnType;
            }
        }

        public Expression Target
        {
            get => _target;
            set => _target = value;
        }

        public string MethodName { get; set; }

        public virtual TypeArguments TypeArguments => Target is MemberAccessExpression memberAccess ? memberAccess.TypeArguments : _typeArguments;

        public Arguments Arguments { get; private set; }

        public override bool IsPrimaryExpression => true;

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _target, prefix, postfix);

            for (int i = 0; i < Arguments.Count; i++)
            {
                Argument argument = Arguments[i];
                Walk(ref argument, prefix, postfix);
                Arguments[i] = argument;
            }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            Expression target = Target;
            Type type = (Type)null;

            MSAst transformedTarget;

            if (target is TypeExpression targetType)
            {
                type = targetType.Type;
                transformedTarget = MSAst.Constant(type, typeof(Type));
            }
            else
            {
                transformedTarget = target.Transform(generator);
            }

            MethodInfo method = Method;
            if (method != null)
            {
                return MSAstUtil.ComplexCallHelper(
                    (type != null) ? null : transformedTarget,
                    method,
                    Arguments.Select(o => o.Value.Transform(generator)).ToArray());
            }

            return type != null
                ? MSAst.Call(
                    type,
                    MethodName,
                    TypeArguments.ResolvedTypes,
                    Arguments.Transform(generator))
                : MSAst.Call(
                transformedTarget,
                MethodName,
                TypeArguments.ResolvedTypes,
                Arguments.Transform(generator));
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is CallExpression clone))
            {
                return;
            }

            clone.Arguments = Arguments.Clone(cloneContext);
            clone._typeArguments = _typeArguments.Clone();
            clone._target = Clone(cloneContext, _target);
            clone._method = _method;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            bool parenthesize = !_target.IsPrimaryExpression;

            if (parenthesize)
            {
                sw.Write("(");
            }

            _target.Dump(sw, indentChange);

            if (parenthesize)
            {
                sw.Write(")");
            }

            sw.Write(".");
            sw.Write(Method != null ? Method.Name : MethodName);

            if (_typeArguments.Count != 0)
            {
                sw.Write("<");
                for (int i = 0; i < _typeArguments.Count; i++)
                {
                    FullNamedExpression typeArgument = _typeArguments[i];

                    if (i != 0)
                    {
                        sw.Write(", ");
                    }

                    typeArgument.Dump(sw, indentChange);
                }
                sw.Write(">");
            }

            sw.Write("(");

            for (int i = 0; i < Arguments.Count; i++)
            {
                Argument argument = Arguments[i];

                if (i != 0)
                    sw.Write(", ");

                argument.Dump(sw, indentChange);
            }

            sw.Write(")");
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            Expression targetResolved = Target.Resolve(
                parseContext,
                ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);

            if (targetResolved == null)
            {
                return null;
            }

            Target = targetResolved;

            Arguments arguments = Arguments;
            if (arguments != null)
            {
                Arguments.Resolve(parseContext);
            }

            _ = _typeArguments.Resolve(parseContext);

            return this;
        }
    }
}