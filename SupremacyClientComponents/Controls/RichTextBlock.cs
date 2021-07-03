using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Supremacy.Text;

namespace Supremacy.Client.Controls
{
    public sealed class RichTextBlock : TextBlock
    {
        public static readonly DependencyProperty RichTextProperty;

        private string _registeredAccessKey;

        static RichTextBlock()
        {
            RichTextProperty = DependencyProperty.Register(
                "RichText",
                typeof(RichText),
                typeof(RichTextBlock),
                new PropertyMetadata(
                    null,
                    (d, e) => ((RichTextBlock)d).OnRichTextChange((RichText)e.NewValue)));
        }

        public RichText RichText
        {
            get => (RichText)GetValue(RichTextProperty);
            set => SetValue(RichTextProperty, value);
        }

        private void OnRichTextChange(RichText richText)
        {
            if (richText == null)
            {
                return;
            }

            Inlines.Clear();
            Inlines.Add(richText.ToSpan());
            RegisterAccessKey();
        }

        private string GetAccessKey()
        {
            return GetAccessKey(RichText.Text);
        }

        private string GetAccessKey(string text)
        {
            int num = text.IndexOf('_');
            if (num == -1 || num == text.Length - 1)
            {
                return null;
            }

            return text.Substring(num + 1, 1);
        }

        private void RegisterAccessKey()
        {
            if (_registeredAccessKey != null)
            {
                AccessKeyManager.Unregister(_registeredAccessKey, this);
                _registeredAccessKey = null;
            }

            string accessKey = GetAccessKey();

            if (string.IsNullOrEmpty(accessKey))
            {
                return;
            }

            AccessKeyManager.Register(accessKey, this);
            _registeredAccessKey = accessKey;
        }
    }
}