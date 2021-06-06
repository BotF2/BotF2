using System.Linq;
using System.Reflection;
using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class ElementAccessExpression : Expression
    {
        private Expression _target;

        public ElementAccessExpression()
        {
            Arguments = new Arguments();
        }

        public Expression Target
        {
            get => _target;
            set => _target = value;
        }

        public Arguments Arguments { get; }

        public override bool IsPrimaryExpression => true;

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is ElementAccessExpression clone))
            {
                return;
            }

            Arguments.CloneTo(cloneContext, clone.Arguments);
            clone._target = Clone(cloneContext, _target);
        }

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
            MSAst[] arguments = Arguments.Transform(generator);

            if (Target is PropertyExpression propertyExpression)
            {
                MSAst instance = null;

                if (propertyExpression.InstanceExpression != null)
                    instance = propertyExpression.InstanceExpression.TransformCore(generator);

                // ReSharper disable AssignNullToNotNullAttribute
                return MSAst.MakeIndex(
                    instance,
                    propertyExpression.PropertyInfo,
                    arguments);
                // ReSharper restore AssignNullToNotNullAttribute
            }

            return MSAst.ArrayIndex(
                Target.Transform(generator),
                arguments);
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            _target = _target.Resolve(parseContext);
            Arguments.Resolve(parseContext);

            if (_target is TypeExpression typeExpression)
            {
                MethodInfo[] indexers = typeExpression.Type
                    .GetProperties(BindingFlags.Static | BindingFlags.Public)
                    .Where(o => o.Name == "Item")
                    .Select(o => o.GetGetMethod(false))
                    .Where(o => o.GetParameters().Length != 0)
                    .ToArray();

                if (indexers.Length == 0)
                {
                    parseContext.ReportError(
                        CompilerErrors.MissingIndexerValue,
                        Span,
                        typeExpression.GetSignatureForError());

                    return null; // TODO
                }

                using (parseContext.Set(ParseContext.Options.InvokeSpecialName))
                {
                    return new InvokeExpression(
                        new MethodGroupExpression(
                            indexers,
                            typeExpression.Type,
                            Span),
                        Arguments).DoResolve(parseContext);
                }
            }

            if (_target is PropertyExpression propertyExpression)
            {
                MethodInfo[] indexers = propertyExpression.Type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(o => o.Name == "Item")
                    .Select(o => o.GetGetMethod(false))
                    .Where(o => o.GetParameters().Length != 0)
                    .ToArray();

                if (indexers.Length == 0)
                {
                    parseContext.ReportError(
                        CompilerErrors.MissingIndexerValue,
                        Span,
                        propertyExpression.Type.FullName);

                    return null; // TODO
                }

                using (parseContext.Set(ParseContext.Options.InvokeSpecialName))
                {
                    return new InvokeExpression(
                        new MethodGroupExpression(
                            indexers,
                            propertyExpression.Type,
                            Span)
                        {
                            InstanceExpression = _target
                        },
                        Arguments).DoResolve(parseContext);
                }
            }

            Arguments.Resolve(parseContext);

            return this;
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

            sw.Write("[");

            int lastStartLine = Span.Start.Line;

            for (int i = 0; i < Arguments.Count; i++)
            {
                Argument argument = Arguments[i];

                if (i != 0)
                {
                    sw.Write(",");

                    if (argument.Span.Start.Line != lastStartLine)
                    {
                        sw.WriteLine();
                        sw.Write(' ', argument.Span.Start.Column - 1 + indentChange);
                    }
                    else
                    {
                        sw.Write(" ");
                    }

                    lastStartLine = argument.Span.Start.Line;
                }

                argument.Dump(sw, indentChange);
            }

            sw.Write("]");
        }
    }
}