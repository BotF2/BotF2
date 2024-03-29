using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using System.Linq;

using Supremacy.Scripting.Runtime;
using Supremacy.Scripting.Utility;

namespace Supremacy.Scripting.Ast
{
    public class InterpolatedStringExpression : Expression
    {
        private LiteralExpression _literalExpression;
        private Func<char, char> _translateEscapeCharacter;

        public InterpolatedStringExpression(LiteralExpression textLiteral)
            : this(textLiteral, TranslateEscapeCharacter) { }

        public InterpolatedStringExpression(
            LiteralExpression literalExpression,
            Func<char, char> translateEscapeCharacter)
        {
            if (literalExpression == null)
            {
                throw new ArgumentNullException("literalExpression");
            }

            if (literalExpression.Kind != LiteralKind.Text)
            {
                throw new ArgumentException("Argument must be a Text literal expression.", "literalExpression");
            }

            _literalExpression = literalExpression;
            _translateEscapeCharacter = translateEscapeCharacter ?? throw new ArgumentNullException("translateEscapeCharacter");
        }

        internal InterpolatedStringExpression()
        {
            // For cloning purposes only.
        }

        public override void CloneTo<T>(CloneContext cloneContext, T target)
        {
            base.CloneTo(cloneContext, target);

            if (!(target is InterpolatedStringExpression clone))
            {
                return;
            }

            clone._literalExpression = Clone(cloneContext, _literalExpression);
            clone._translateEscapeCharacter = _translateEscapeCharacter;
        }

        public override Expression DoResolve(ParseContext parseContext)
        {
            const int startIndex = 0;

            char[] source = _literalExpression.Text.ToCharArray();
            if (source[startIndex] != '"')
            {
                return null;
            }

            int index = startIndex;
            if (++index >= source.Length)
            {
                return null;
            }

            ScriptLanguageContext language = (ScriptLanguageContext)parseContext.Compiler.SourceUnit.LanguageContext;
            int startOfString = index;

            InterpolatedStringBuilder interpolatedStringBuilder = new InterpolatedStringBuilder(
                parseContext,
                startIndex,
                CreateSourceSpanCallback);

            while (source[index] != '"')
            {
                char c = source[index];
                if (c == '\\')
                {
                    interpolatedStringBuilder.AddString(ExtractString(source, startOfString, index), index);

                    if (++index >= source.Length)
                    {
                        return null;
                    }

                    interpolatedStringBuilder.AddString(_translateEscapeCharacter(source[index]).ToString(), index);

                    if (++index >= source.Length)
                    {
                        return null;
                    }

                    startOfString = index;
                    continue;
                }
                if ((c == '{') &&
                    (index < (source.Length - 2)) &&
                    (source[index + 1] != '{') &&
                    (source[index + 1] != '}'))
                {
                    interpolatedStringBuilder.AddString(ExtractString(source, startOfString, index), index);

                    if (++index >= source.Length)
                    {
                        return null;
                    }

                    string interpolatedExpressionSpan = ExtractInterpolatedExpression(
                        parseContext,
                        source,
                        ref index);

                    //var expressionLength = interpolatedExpressionSpan.Length;

                    //index += expressionLength + 1;

                    SourceUnit interpolatedSource = language.CreateSnippet(
                        interpolatedExpressionSpan,
                        SourceCodeKind.Expression);

                    Expression interpolatedExpression = language.ParseExpression(interpolatedSource);

                    if (interpolatedExpression != null)
                    {
                        interpolatedStringBuilder.AddExpression(
                            interpolatedExpression,
                            index);

                        if (index >= source.Length)
                        {
                            return null;
                        }

                        startOfString = index;
                        continue;
                    }
                    return null;
                }

                if (++index >= source.Length)
                {
                    return null;
                }
            }

            interpolatedStringBuilder.AddString(ExtractString(source, startOfString, index), index);

            index++;

            return interpolatedStringBuilder.CreateExpression(index).DoResolve(parseContext);
        }

        private string ExtractInterpolatedExpression(ParseContext parseContext, char[] source, ref int index)
        {
            StringBuilder resultBuilder = new StringBuilder();

            while (index < source.Length)
            {
                if (source[index] == '{')
                {
                    parseContext.ThrowError(
                        CompilerErrors.InterpolatedExpressionCannotContainBraces,
                        CreateSourceSpanCallback(index, index + 1));
                }

                if (source[index] == '}')
                {
                    ++index;
                    break;
                }

                if (source[index] == '\\' && index < source.Length - 1 && source[index + 1] == '"')
                {
                    ++index;
                }

                _ = resultBuilder.Append(source[index++]);
            }

            return resultBuilder.ToString();
        }

        private SourceSpan CreateSourceSpanCallback(int start, int length)
        {
            return new SourceSpan(
                new SourceLocation(
                    _literalExpression.Span.Start.Index + start,
                    _literalExpression.Span.Start.Line,
                    _literalExpression.Span.Start.Column + start),
                new SourceLocation(
                    _literalExpression.Span.Start.Index + start + length,
                    _literalExpression.Span.Start.Line,
                    _literalExpression.Span.Start.Column + start + length));
        }

        private static string ExtractString(char[] source, int startOfString, int index)
        {
            return new string(source, startOfString, index - startOfString);
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

    internal class InterpolatedStringBuilder
    {
        private readonly Func<int, int, SourceSpan> _sourceSpanCallback;
        private readonly ParseContext _parseContext;
        private readonly StringBuilder _stringBuilder;
        private readonly int _startIndex;
        private int _stringStartIndex;
        private List<Expression> _expressions;

        public InterpolatedStringBuilder(ParseContext parseContext, int startIndex, Func<int, int, SourceSpan> sourceSpanCallback)
        {
            _parseContext = parseContext ?? throw new ArgumentNullException("parseContext");
            _startIndex = startIndex;
            _sourceSpanCallback = sourceSpanCallback ?? throw new ArgumentNullException("sourceSpanCallback");
            _stringStartIndex = startIndex;
            _stringBuilder = new StringBuilder();
            _expressions = null;
        }

        public void AddString(string s, int endIndex)
        {
            _ = _stringBuilder.Append(s);
        }

        public Expression CreateExpression(int endIndex)
        {
            return _expressions == null
                ? new ConstantExpression<string>(_stringBuilder.ToString())
                : (Expression)new CallExpression(
                CommonMembers.StringFormat,
                new TypeExpression(typeof(string)),
                ArrayUtils.Insert(
                    new Argument(new ConstantExpression<string>(_stringBuilder.ToString())),
                    _expressions.Select(e => new Argument(e)).ToArray()))
                {
                    Span = _sourceSpanCallback(_stringStartIndex, endIndex - _startIndex),
                    FileName = _parseContext.Compiler.SourceUnit.Path
                };
        }

        public void AddExpression(Expression expression, int endIndex)
        {
            if (_expressions == null)
            {
                _expressions = new List<Expression>();
            }

            _ = _stringBuilder.AppendFormat("{{{0}}}", _expressions.Count);
            _expressions.Add(expression);

            _stringStartIndex = endIndex;
        }
    }
}