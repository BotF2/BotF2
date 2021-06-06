using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Supremacy.Client.Data;

namespace Supremacy.Client.Controls
{
    public class HyperlinkClickedEventArgs : RoutedEventArgs
    {
        private readonly Uri _navigateUri;
        private readonly object _dataContext;

        public HyperlinkClickedEventArgs(Uri navigateUri) : base(TextBlockExtensions.HyperlinkClickedEvent)
        {
            _navigateUri = navigateUri;
        }

        public HyperlinkClickedEventArgs(Uri navigateUri, object source, object dataContext = null) : base(TextBlockExtensions.HyperlinkClickedEvent, source)
        {
            _navigateUri = navigateUri;
            _dataContext = dataContext;
        }

        public Uri NavigateUri => _navigateUri;

        public object DataContext => _dataContext;
    }

    public static class TextBlockExtensions
    {
        #region Constructors and Finalizers
        static TextBlockExtensions()
        {
            FormattedTextProperty = DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(string),
                typeof(TextBlockExtensions),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnFormattedTextChanged));
        }
        #endregion

        #region HyperlinkClicked Attached Event
        public static readonly RoutedEvent HyperlinkClickedEvent = EventManager.RegisterRoutedEvent(
            "HyperlinkClicked",
            RoutingStrategy.Bubble,
            typeof(EventHandler<HyperlinkClickedEventArgs>),
            typeof(TextBlockExtensions));

        public static void AddHyperlinkClickedHandler(UIElement element, EventHandler<HyperlinkClickedEventArgs> handler)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (handler == null)
                throw new ArgumentNullException("handler");
            element.AddHandler(HyperlinkClickedEvent, handler);
        }

        public static void RemoveHyperlinkClickedHandler(UIElement element, EventHandler<HyperlinkClickedEventArgs> handler)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (handler == null)
                throw new ArgumentNullException("handler");
            element.RemoveHandler(HyperlinkClickedEvent, handler);
        }
        #endregion

        #region FormattedText Property
        /// <summary>
        /// Gets the inlines.
        /// <remarks>
        /// Important: if the inlines in the TextBlock are changed after this property is set,
        /// the get will not reflect these changes.
        /// </remarks>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetFormattedText(DependencyObject o)
        {
            return (string)o.GetValue(FormattedTextProperty);
        }

        /// <summary>
        /// Sets the inlines.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="value"></param>
        public static void SetFormattedText(DependencyObject o, string value)
        {
            o.SetValue(FormattedTextProperty, value);
        }

        /// <summary>
        /// Identifies the <c>FormattedText</c> attached dependency property.
        /// </summary>
        public static readonly DependencyProperty FormattedTextProperty;

        private static void OnFormattedTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            InlineCollection inlines = null;

            var textBlock = o as TextBlock;
            if (textBlock != null)
            {
                inlines = textBlock.Inlines;
            }
            else
            {
                var paragraph = o as Paragraph;
                if (paragraph != null)
                {
                    inlines = paragraph.Inlines;
                }
                else
                {
                    var span = o as Span;
                    if (span != null)
                        inlines = span.Inlines;
                }
            }

            if (inlines == null)
                return;

            inlines.Clear();

            if (e.NewValue == null)
                return;

            var formattedInlines = FormattedTextConverter.Instance.Convert(
                e.NewValue,
                typeof(IEnumerable<Inline>),
                null,
                CultureInfo.InvariantCulture) as IEnumerable<Inline>;

            if (formattedInlines != null)
                inlines.AddRange(formattedInlines);
        }
        #endregion
        }
}