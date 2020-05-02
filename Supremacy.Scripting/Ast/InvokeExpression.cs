using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class InvokeExpression : Expression
    {
        private TypeArguments _typeArguments;
        private Arguments _arguments;
        private bool _argumentsResolved;
        private Expression _target;

        public InvokeExpression()
        {
            _typeArguments = new TypeArguments();
            _arguments = new Arguments();
        }

        public InvokeExpression(Expression target, IEnumerable<Argument> arguments)
            : this()
        {
            Target = target ?? throw new ArgumentNullException("target");

            if (arguments != null)
            {
                Arguments.AddRange(arguments);
            }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is InvokeExpression clone))
            {
                return;
            }

            clone._typeArguments = _typeArguments.Clone(cloneContext);
            clone._arguments = _arguments.Clone(cloneContext);
            clone.MethodGroup = Clone(cloneContext, MethodGroup);
            clone._argumentsResolved = _argumentsResolved;
            clone._target = Clone(cloneContext, _target);
        }
        
        public Expression Target
        {
            get => _target;
            set => _target = value;
        }

        public virtual TypeArguments TypeArguments => Target is MemberAccessExpression memberAccess ? memberAccess.TypeArguments : _typeArguments;

        public Arguments Arguments
        {
            get => _arguments;
            protected set => _arguments = value;
        }

        public override bool IsPrimaryExpression => true;

        protected MethodGroupExpression MethodGroup { get; set; }

        public override void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            Walk(ref _target, prefix, postfix);

            for (int i = 0; i < _arguments.Count; i++)
            {
                Argument argument = _arguments[i];
                Walk(ref argument, prefix, postfix);
                _arguments[i] = argument;
            }
        }

        public override MSAst TransformCore(ScriptGenerator generator)
        {
            Expression instanceExpression = MethodGroup.InstanceExpression;

            return MSAst.Call(
                instanceExpression?.Transform(generator),
                (MethodInfo)MethodGroup,
                _arguments.Select(o => o.Value.Transform(generator)));
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

            bool isExtensionInvocation = MethodGroup is ExtensionMethodGroupExpression;
            int firstArgumentIndex = isExtensionInvocation ? 1 : 0;

            for (int i = firstArgumentIndex; i < _arguments.Count; i++)
            {
                Argument argument = _arguments[i];

                if (i > firstArgumentIndex)
                {
                    sw.Write(", ");
                }

                argument.Dump(sw, indentChange);
            }

            sw.Write(")");
        }

        public override Expression DoResolve(ParseContext ec)
        {
            // Don't resolve already resolved expression
            if (ExpressionClass != ExpressionClass.Invalid)
            {
                return this;
            }

            Expression resolvedTarget = Target.Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
            if (resolvedTarget == null)
            {
                return null;
            }

            // Next, evaluate all the expressions in the argument list
            if (_arguments != null && !_argumentsResolved)
            {
                _arguments.Resolve(ec);
                _argumentsResolved = true;
            }

            Type expressionType = resolvedTarget.Type;

            MethodGroup = resolvedTarget as MethodGroupExpression;

            if (MethodGroup == null)
            {
                if (expressionType != null && TypeManager.IsDelegateType(expressionType))
                {
                    MethodGroup = (resolvedTarget = new MemberAccessExpression
                    {
                        Left = resolvedTarget,
                        Name = "Invoke",
                        Span = Span
                    }.Resolve(ec)) as MethodGroupExpression;
                }

                if (!(resolvedTarget is MemberExpression memberExpression))
                {
                    resolvedTarget.OnErrorUnexpectedKind(ec, ResolveFlags.MethodGroup, Span);
                    return null;
                }

                if (MethodGroup == null)
                {
                    MethodGroup = ec.LookupExtensionMethod(
                        memberExpression.Type,
                        memberExpression.Name,
                        Span);

                    if (MethodGroup != null)
                    {
                        ((ExtensionMethodGroupExpression)MethodGroup).ExtensionExpression = memberExpression.InstanceExpression;
                    }
                }
                
                if (MethodGroup == null)
                {
                    ec.ReportError(
                        1955,
                        string.Format(
                            "The member '{0}' cannot be used as method or delegate.",
                            resolvedTarget.GetSignatureForError()),
                        Span);

                    return null;
                }
            }

            MethodGroup = DoResolveOverload(ec);

            if (MethodGroup == null)
            {
                return null;
            }

            MethodInfo method = (MethodInfo)MethodGroup;
            if (method != null)
            {
                Type = method.ReturnType;

                // TODO: this is a copy of mg.ResolveMemberAccess method
                Expression instanceExpression = MethodGroup.InstanceExpression;
                if (method.IsStatic)
                {
                    if (instanceExpression == null ||
                        instanceExpression is EmptyExpression ||
                        MethodGroup.IdenticalTypeName)
                    {
                        MethodGroup.InstanceExpression = null;
                    }
                    else
                    {
                        // TODO: MemberExpression.error176(ec, loc, mg.GetSignatureForError());
                        return null;
                    }
                }
                else
                {
                    if (instanceExpression == null || instanceExpression == EmptyExpression.Null)
                    {
                        // TODO: SimpleName.Error_ObjectRefRequired(ec, loc, mg.GetSignatureForError());
                    }
                }
            }

            if (method == null)
            {
                return null;
            }

            _ = IsSpecialMethodInvocation(ec, method, Span);

            ExpressionClass = ExpressionClass.Value;
            return this;
        }

        protected virtual MethodGroupExpression DoResolveOverload(ParseContext ec)
        {
            return MethodGroup.OverloadResolve(ec, ref _arguments, false, Span);
        }

        public static bool IsSpecialMethodInvocation(ParseContext ec, MethodBase method, SourceSpan location)
        {
            if (!TypeManager.IsSpecialMethod(method))
            {
                return false;
            }

            if (ec.HasSet(ParseContext.Options.InvokeSpecialName))
            {
                return false;
            }

            ec.ReportError(
                571,
                string.Format(
                    "'{0}': cannot explicitly call operator or accessor.",
                    TypeManager.GetCSharpSignature(method, true)),
                location);

            return true;
        }
    }
}