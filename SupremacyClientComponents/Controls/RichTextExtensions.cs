using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using Supremacy.Collections;
using Supremacy.Text;

namespace Supremacy.Client.Controls
{
    public static class RichTextExtensions
    {
        private static readonly DelegatingWeakEventListener<RoutedEventArgs> HyperlinkClickedWeakEventListener;

        static RichTextExtensions()
        {
            HyperlinkClickedWeakEventListener = new DelegatingWeakEventListener<RoutedEventArgs>(OnHyperlinkClick);
        }

        private static void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var link = sender as Hyperlink;
            if (link == null)
                return;

            var args = new HyperlinkClickedEventArgs(link.NavigateUri, link, link.DataContext);
            
            link.RaiseEvent(args);
            
            if (args.Handled)
                e.Handled = true;
        }

        public static Span ToSpan(this RichText richText, bool handleAccessKeyCharacter = true)
        {
            var span = new Span();
            var length = -1;
            var stack = ImmutableStack<Span>.Empty;
            var currentUri = (Uri)null;

            foreach (var part in richText.GetFormattedParts())
            {
                var substring = richText.Text.Substring(part.Offset, part.Length);
                var inline = (Inline)null;

                var linkData = richText.GetUserData<Uri>(part.Offset, part.Offset + part.Length);

                if (currentUri != null && (linkData.Count == 0 || linkData[0] != currentUri))
                {
                    var hyperlink = (Hyperlink)span;
                    span = stack.Peek();
                    stack = stack.Pop();
                    span.Inlines.Add(hyperlink);
                    currentUri = null;
                }

                if (linkData.Count != 0 && linkData[0] != currentUri)
                {
                    currentUri = linkData[0];
                    stack = stack.Push(span);
                    span = new Hyperlink();

                    if (currentUri != RichTextConverter.EmptyLinkUri)
                        ((Hyperlink)span).NavigateUri = currentUri;

                    GenericWeakEventManager.AddListener(
                        span,
                        "Click",
                        HyperlinkClickedWeakEventListener);
                }

                if (handleAccessKeyCharacter && substring.Length != 0)
                {
                    length = length == -1 ? substring.IndexOf('_') : length;

                    if (length == substring.Length - 1)
                    {
                        length = 0;
                    }
                    else if (length == -1)
                    {
                        inline = new Run(substring);
                    }
                    else
                    {
                        var head = new Run(substring.Substring(0, length));
                        var mnemonic = new Run(substring.Substring(length + 1, 1));
                        var tail = new Run(substring.Substring(length + 2));

                        mnemonic.TextDecorations.Add(TextDecorations.Underline);

                        inline = new Span
                                 {
                                     Inlines =
                                         {
                                             head,
                                             mnemonic,
                                             tail
                                         }
                                 };

                        handleAccessKeyCharacter = false;
                    }
                }
                else
                {
                    inline = new Run(substring);
                }

                if (inline == null)
                    continue;

                span.Inlines.Add(inline);

                // ReSharper disable RedundantCheckBeforeAssignment

                if (part.Style.FontWeight != inline.FontWeight)
                    inline.FontWeight = part.Style.FontWeight;

                if (part.Style.FontStyle != inline.FontStyle)
                    inline.FontStyle = part.Style.FontStyle;

                if (part.Style.Foreground != null && part.Style.Foreground != inline.Foreground)
                    inline.Foreground = part.Style.Foreground;

                if (part.Style.Background != null && part.Style.Background != inline.Background)
                    inline.Background = part.Style.Background;

                // ReSharper restore RedundantCheckBeforeAssignment

                var pen = (Pen)null;

                if (part.Style.EffectBrush != null && part.Style.EffectBrush != part.Style.Foreground)
                    pen = new Pen { Brush = part.Style.EffectBrush };

                switch (part.Style.Effect)
                {
                    case TextEffectStyle.None:
                        continue;

                    case TextEffectStyle.StraightUnderline:
                        if (pen == null)
                        {
                            inline.TextDecorations = TextDecorations.Underline;
                            continue;
                        }
                        inline.TextDecorations.Add(
                            new TextDecoration
                            {
                                Location = TextDecorationLocation.Underline,
                                Pen = pen
                            });
                        continue;

                    case TextEffectStyle.WeavyUnderline:
                        if (pen == null)
                        {
                            inline.TextDecorations = TextDecorations.Underline;
                            continue;
                        }
                        inline.TextDecorations.Add(
                            new TextDecoration
                            {
                                Location = TextDecorationLocation.Underline,
                                Pen = pen
                            });
                        continue;

                    case TextEffectStyle.StrikeOut:
                        if (pen == null)
                        {
                            inline.TextDecorations = TextDecorations.Strikethrough;
                            continue;
                        }
                        inline.TextDecorations.Add(
                            new TextDecoration
                            {
                                Location = TextDecorationLocation.Strikethrough,
                                Pen = pen
                            });
                        continue;

                    default:
                        throw new InvalidOperationException("Invalid text effect: " + part.Style.Effect);
                }
            }

            return span;
        }
    }
}