using System.Linq;
using System.Reflection;
using Supremacy.Scripting.Runtime;

using MSAst = System.Linq.Expressions.Expression;

namespace Supremacy.Scripting.Ast
{
    public class ElementAccessExpression : Expression
    {
        private readonly Arguments _arguments;

        private Expression _target;

        public ElementAccessExpression()
        {
            _arguments = new Arguments();
        }

        public Expression Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public Arguments Arguments
        {
            get { return _arguments; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as ElementAccessExpression;
            if (clone == null)
                return;

            _arguments.CloneTo(cloneContext, clone._arguments);
            clone._target = Clone(cloneContext, _target);
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
            var arguments = Arguments.Transform(generator);

            var propertyExpression = Target as PropertyExpression;
            if (propertyExpression != null)
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
            _arguments.Resolve(parseContext);

            var typeExpression = _target as TypeExpression;
            if (typeExpression != null)
            {
                var indexers = typeExpression.Type
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
                        _arguments).DoResolve(parseContext);
                }
            }

            var propertyExpression = _target as PropertyExpression;
            if (propertyExpression != null)
            {
                var indexers = propertyExpression.Type
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
                        _arguments).DoResolve(parseContext);
                }
            }

            _arguments.Resolve(parseContext);
            
            return this;
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            var parenthesize = !_target.IsPrimaryExpression;

            if (parenthesize)
                sw.Write("(");

            _target.Dump(sw, indentChange);

            if (parenthesize)
                sw.Write(")");

            sw.Write("[");

            var lastStartLine = Span.Start.Line;

            for (var i = 0; i < _arguments.Count; i++)
            {
                var argument = _arguments[i];

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