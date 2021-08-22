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
            if (!(sender is Hyperlink link))
            {
                return;
            }

            HyperlinkClickedEventArgs args = new HyperlinkClickedEventArgs(link.NavigateUri, link, link.DataContext);

            link.RaiseEvent(args);

            if (args.Handled)
            {
                e.Handled = true;
            }
        }

        public static Span ToSpan(this RichText richText, bool handleAccessKeyCharacter = true)
        {
            Span span = new Span();
            int length = -1;
            IStack<Span> stack = ImmutableStack<Span>.Empty;
            Uri currentUri = null;

            foreach (RichString part in richText.GetFormattedParts())
            {
                string substring = richText.Text.Substring(part.Offset, part.Length);
                Inline inline = null;

                System.Collections.Generic.IList<Uri> linkData = richText.GetUserData<Uri>(part.Offset, part.Offset + part.Length);

                if (currentUri != null && (linkData.Count == 0 || linkData[0] != currentUri))
                {
                    Hyperlink hyperlink = (Hyperlink)span;
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
                    {
                        ((Hyperlink)span).NavigateUri = currentUri;
                    }

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
                        Run head = new Run(substring.Substring(0, length));
                        Run mnemonic = new Run(substring.Substring(length + 1, 1));
                        Run tail = new Run(substring.Substring(length + 2));

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
                {
                    continue;
                }

                span.Inlines.Add(inline);

                if (part.Style.FontWeight != inline.FontWeight)
                {
                    inline.FontWeight = part.Style.FontWeight;
                }

                if (part.Style.FontStyle != inline.FontStyle)
                {
                    inline.FontStyle = part.Style.FontStyle;
                }

                if (part.Style.Foreground != null && part.Style.Foreground != inline.Foreground)
                {
                    inline.Foreground = part.Style.Foreground;
                }

                if (part.Style.Background != null && part.Style.Background != inline.Background)
                {
                    inline.Background = part.Style.Background;
                }

                Pen pen = null;

                if (part.Style.EffectBrush != null && part.Style.EffectBrush != part.Style.Foreground)
                {
                    pen = new Pen { Brush = part.Style.EffectBrush };
                }

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