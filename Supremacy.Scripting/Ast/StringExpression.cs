using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Scripting.Utils;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

using MSAst = System.Linq.Expressions;

namespace Supremacy.Scripting.Ast
{
    public class StringExpression : Expression
    {
        private readonly List<Expression> _contents;

        public StringExpression()
        {
            _contents = new List<Expression>();
        }

        public IList<Expression> Contents
        {
            get { return _contents; }
        }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            var clone = target as StringExpression;
            if (clone == null)
                return;

            clone._contents.Clear();
            clone._contents.AddRange(Clone<Expression>(cloneContext, _contents));
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            var builder = new StringBuilder();
            var arguments = new List<Expression>();

            if (_contents.Count == 1 && _contents[0] is StringLiteralContent)
                return new ConstantExpression<string>(((StringLiteralContent)_contents[0]).Text);

            foreach (var content in _contents)
            {
                var literal = content as StringLiteralContent;
                if (literal != null)
                {
                    if (literal.Text != null)
                        AppendLiteral(parseContext, builder, literal);
                    continue;
                }

                builder.Append('{');
                builder.Append(arguments.Count);
                builder.Append('}');

                arguments.Add(content);
            }

            return new CallExpression(
                CommonMembers.StringFormat,
                new TypeExpression(typeof(string)),
                ArrayUtils.Insert(
                    new Argument(new ConstantExpression<string>(builder.ToString())),
                    arguments.Select(e => new Argument(e)).ToArray()))
                   {
                       Span = Span,
                       FileName = parseContext.Compiler.SourceUnit.Path
                   }
                .Resolve(parseContext);
        }

        private void AppendLiteral(ParseContext parseContext, StringBuilder builder, [NotNull] StringLiteralContent literal)
        {
            if (literal == null)
                throw new ArgumentNullException("literal");

            var text = literal.Text;
            if (text == null)
                return;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '\\')
                {
                    if (++i >= text.Length)
                    {
                        parseContext.ReportError(
                            CompilerErrors.UnrecognizedEscapeSequence,
                            literal.Span);

                        return;
                    }

                    ch = TranslateEscapeCharacter(ch);
                }

                if (ch == '{' || ch == '}')
                    builder.Append(ch);
    
                builder.Append(ch);
            }
        }

        public static char TranslateEscapeCharacter(char source)
        {
            switch (source)
            {
                case 't':
                    return '\t';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                default:
                    return source;
            }
        }
    }

    public class StringLiteralContent : Expression
    {
        public string Text { get; set; }

        public override bool IsPrimaryExpression
        {
            get { return true; }
        }

        public override MSAst.Expression TransformCore(ScriptGenerator generator)
        {
            return MSAst.Expression.Constant(
                Text,
                Type);
        }

        public override void Dump(SourceWriter sw, int indentChange)
        {
            sw.Write(Text);
        }

        public override Expression DoResolve(ParseContext rc)
        {
            Type = typeof(string);
            return this;
        }
    }
}