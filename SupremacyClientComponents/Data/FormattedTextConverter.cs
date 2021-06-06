using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Supremacy.Client.Controls;

namespace Supremacy.Client.Data
{
    public interface ILinkCommandSite
    {
        ICommand LinkCommand { get; }
        object LinkCommandParameter { get; }
    }

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
    public class FormattedTextConverter : ValueConverter<FormattedTextConverter>
    {
        private static readonly DelegatingWeakEventListener<RoutedEventArgs> HyperlinkClickedWeakEventListener;
        
        private static readonly ColorConverter _colorConverter = new ColorConverter();
        private static readonly BrushConverter _brushConverter = new BrushConverter();

        private static readonly Regex _resourceReferenceRegex = new Regex(
            @"\{(?<ResourceKey>\w+)\}",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static FormattedTextConverter()
        {
            HyperlinkClickedWeakEventListener = new DelegatingWeakEventListener<RoutedEventArgs>(OnHyperlinkClick);
        }

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

        #region Methods
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the GTMT#binding source.</param>
        /// <param name="targetType">The type of the GTMT#binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = (string)value;

            if (string.IsNullOrWhiteSpace(source))
                return Binding.DoNothing;

            List<Inline> inlines = new List<Inline>();
            string[] lines = source.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                StringBuilder sb = new StringBuilder();
                Span parentSpan = new Span();

                for (int i = 0; i < line.Length; ++i)
                {
                    char current = line[i];
                    char? next = (i + 1 < line.Length) ? line[i + 1] : (char?)null;

                    if (current == '[' && next != '[')
                    {
                        string text = sb.ToString();
                        
                        sb.Length = 0;
                        i += (next == '/') ? 2 : 1;
                        current = line[i];

                        while (i < line.Length && current != ']')
                        {
                            sb.Append(current);

                            if (++i < line.Length)
                                current = line[i];
                        }

                        if (text.Length > 0)
                            parentSpan.Inlines.Add(text);

                        if (next == '/' && parentSpan.Parent != null)
                        {
                            parentSpan = (Span)parentSpan.Parent;
                        }
                        else
                        {
                            string[] tag = sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (tag.Length > 0)
                            {
                                InlineType inlineType = GetInlineType(tag[0].TrimEnd('/'));
                                if (inlineType == InlineType.LineBreak)
                                {
                                    parentSpan.Inlines.Add(new LineBreak());
                                }
                                else if (inlineType != InlineType.Run)
                                {
                                    string tagParam = (tag.Length > 1) ? tag[1] : null;
                                    Span newParentSpan = CreateSpan(inlineType, tagParam);

                                    parentSpan.Inlines.Add(newParentSpan);
                                    parentSpan = newParentSpan;
                                }
                            }
                        }

                        sb = new StringBuilder();
                    }
                    else
                    {
                        if (current == '[' && next == '[')
                            ++i;

                        sb.Append(current);
                    }
                }

                if (sb.Length > 0)
                    parentSpan.Inlines.Add(sb.ToString());

                inlines.Add(parentSpan);
            }

            return inlines.ToArray();
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

        private static Span CreateSpan(InlineType inlineType, string param)
        {
            Span span = null;

            switch (inlineType)
            {
                case InlineType.Hyperlink:
                {
                    Uri uri;

                    if (!Uri.TryCreate(param, UriKind.Absolute, out uri))
                        uri = null;

                        Hyperlink link = new Hyperlink();
                    link.NavigateUri = uri;

                    GenericWeakEventManager.AddListener(
                        link,
                        "Click",
                        HyperlinkClickedWeakEventListener);

                    span = link;
                    break;
                }

                case InlineType.Bold:
                {
                    span = new Bold();
                    break;
                }

                case InlineType.Italic:
                {
                    span = new Italic();
                    break;
                }

                case InlineType.Underline:
                {
                    span = new Underline();
                    break;
                }

                case InlineType.Colored:
                {
                    span = new Span
                           {
                               Foreground = new SolidColorBrush(
                                   (Color)_colorConverter.ConvertFromInvariantString(param))
                           };
                    break;
                }

                case InlineType.Foreground:
                {
                    span = new Span();

                        Match match = _resourceReferenceRegex.Match(param);
                    if (!match.Success)
                    {
                        span.Foreground = (Brush) _brushConverter.ConvertFromInvariantString(param);
                        return span;
                    }

                    span.SetResourceReference(
                        TextElement.ForegroundProperty,
                        match.Groups["ResourceKey"].Value);

                    break;
                }

                default:
                    span = new Span();
                    break;
            }

            return span ??
                   new Span();
        }

        private static void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link == null)
                return;

            link.RaiseEvent(new HyperlinkClickedEventArgs(link.NavigateUri, link, link.DataContext));
        }
        #endregion
    }
}