using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Supremacy.Text
{
    /// <summary>
    /// Converts a string of formatted text to a <see cref="Inline"/>s.
    /// <remarks>
    /// Supported markup:
    /// <list>
    /// <item>[c arg] - colored text, where <c>arg</c> is a Color value or <c>{ResourceKey}</c></item>
    /// <item>[f arg] - colored text, where <c>arg</c> is a Brush value or <c>{ResourceKey}</c></item>
    /// <item>[b] - bold</item>
    /// <item>[b] - bold</item>
    /// <item>[i] - italics</item>
    /// <item>[u] - underline</item>
    /// <item>[h url] - hyperlink</item>
    /// <item>[nl/] - line break</item>
    /// <item>[/] - close tag</item>
    /// <item>[[ - escape for '[' character</item>
    /// </list>
    /// </remarks>
    /// </summary>
    [ValueConversion(typeof(string), typeof(IEnumerable<Inline>))]
    public sealed class RichTextConverter : TypeConverter
    {
        public static readonly Uri EmptyLinkUri = new Uri("empty:///", UriKind.Absolute);

        private static readonly BrushConverter _brushConverter = new BrushConverter();

        private static readonly Regex _resourceReferenceRegex = new Regex(
            @"\{(?<ResourceKey>\w+)\}",
            RegexOptions.Singleline | RegexOptions.Compiled);

        #region InlineType Enum

        private enum InlineType
        {
            Run,
            LineBreak,
            Hyperlink,
            Bold,
            Italic,
            Underline,
            Colored,
            Foreground
        }

        #endregion

        #region RichTextLinkData Struct

        private struct RichTextLinkData
        {
            public readonly int StartOffset;
            public readonly Uri NavigateUri;

            public RichTextLinkData(int startOffset, Uri navigateUri)
            {
                StartOffset = startOffset;
                NavigateUri = navigateUri;
            }
        }

        #endregion

        #region Methods

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                try
                {
                    return ParseRichText(stringValue);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Malformed rich text string: " + stringValue, e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is RichText richText && destinationType == typeof(string))
            {
                return richText.Text;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private RichText ParseRichText(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return RichText.Empty;
            }

            RichText richText = new RichText();
            string[] lines = source.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Stack<InlineType> stack = new Stack<InlineType>();
            Stack<RichTextLinkData> links = new Stack<RichTextLinkData>();
            Stack<TextStyle> styles = new Stack<TextStyle>();
            TextStyle currentStyle = new TextStyle();

            foreach (string line in lines)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < line.Length; ++i)
                {
                    char current = line[i];
                    char? next = (i + 1 < line.Length) ? line[i + 1] : (char?)null;

                    if (current == '[' && next != '[')
                    {
                        string text = sb.ToString();
                        bool endOfPart = next == '/';

                        sb.Length = 0;
                        i += endOfPart ? 2 : 1;
                        current = line[i];

                        while (i < line.Length && current != ']')
                        {
                            _ = sb.Append(current);

                            if (++i < line.Length)
                            {
                                current = line[i];
                            }
                        }

                        int partLength = text.Length;
                        if (partLength > 0)
                        {
                            _ = richText.Append(text, currentStyle);
                        }

                        if (endOfPart && stack.Count != 0)
                        {
                            InlineType currentInlineType = stack.Pop();
                            if (currentInlineType == InlineType.Hyperlink)
                            {
                                RichTextLinkData linkData = links.Pop();
                                if (richText.Length - linkData.StartOffset > 0)
                                {
                                    richText.PutUserData(linkData.StartOffset, richText.Length, linkData.NavigateUri);
                                }
                            }
                            if (styles.Count != 0)
                            {
                                currentStyle = styles.Pop();
                            }
                        }

                        string tag = sb.ToString();

                        if (tag.Length > 0)
                        {
                            string parameter = null;

                            int parameterDelimiter = tag.IndexOf(' ');
                            if (parameterDelimiter > 0)
                            {
                                parameter = tag.Substring(parameterDelimiter + 1);
                                tag = tag.Substring(0, parameterDelimiter);
                            }

                            InlineType inlineType = GetInlineType(tag.TrimEnd('/'));
                            if (inlineType == InlineType.LineBreak)
                            {
                                _ = richText.Append(Environment.NewLine);
                            }
                            else if (inlineType != InlineType.Run)
                            {

                                if (stack.Count != 0)
                                {
                                    styles.Push(currentStyle);
                                }

                                ResolveStyle(inlineType, parameter, ref currentStyle, out Uri linkUri);

                                stack.Push(inlineType);

                                if (inlineType == InlineType.Hyperlink)
                                {
                                    links.Push(new RichTextLinkData(richText.Length, linkUri));
                                }
                            }
                        }

                        sb.Length = 0;
                    }
                    else
                    {
                        if (current == '[' && next == '[')
                        {
                            ++i;
                        }

                        _ = sb.Append(current);
                    }
                }

                if (sb.Length > 0)
                {
                    _ = richText.Append(sb.ToString(), currentStyle);
                }
            }

            while (links.Count != 0)
            {
                RichTextLinkData linkData = links.Pop();
                richText.PutUserData(linkData.StartOffset, richText.Length, linkData);
            }

            return richText.Length == 0 ? RichText.Empty : richText;
        }

        private static InlineType GetInlineType(string type)
        {
            switch (type)
            {
                case "c":
                    return InlineType.Colored;
                case "f":
                    return InlineType.Foreground;
                case "b":
                    return InlineType.Bold;
                case "i":
                    return InlineType.Italic;
                case "u":
                    return InlineType.Underline;
                case "h":
                    return InlineType.Hyperlink;
                case "nl":
                    return InlineType.LineBreak;
                default:
                    return InlineType.Run;
            }
        }

        private static void ResolveStyle(InlineType inlineType, string param, ref TextStyle currentStyle, out Uri navigateUri)
        {
            navigateUri = null;

            switch (inlineType)
            {
                case InlineType.Hyperlink:
                    {
                        if (!Uri.TryCreate(param, UriKind.Absolute, out navigateUri))
                        {
                            navigateUri = EmptyLinkUri;
                        }

                        break;
                    }

                case InlineType.Bold:
                    {
                        currentStyle.FontWeight = FontWeights.Bold;
                        break;
                    }

                case InlineType.Italic:
                    {
                        currentStyle.FontStyle = FontStyles.Italic;
                        break;
                    }

                case InlineType.Underline:
                    {
                        currentStyle.Effect = TextEffectStyle.StraightUnderline;
                        break;
                    }

                case InlineType.Colored:
                    {
                        currentStyle.Foreground = (Brush)_brushConverter.ConvertFromInvariantString(param);

                        if (currentStyle.Foreground != null && currentStyle.Foreground.CanFreeze)
                        {
                            currentStyle.Foreground.Freeze();
                        }

                        break;
                    }

                case InlineType.Foreground:
                    {
                        Match match = _resourceReferenceRegex.Match(param);
                        currentStyle.Foreground = match.Success
                            ? Application.Current != null ? Application.Current.TryFindResource(match.Groups["ResourceKey"].Value) as Brush : null
                            : (Brush)_brushConverter.ConvertFromInvariantString(param);

                        break;
                    }
            }
        }

        #endregion
    }
}