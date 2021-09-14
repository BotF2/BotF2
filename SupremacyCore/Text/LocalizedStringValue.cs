using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

using Supremacy.Types;

namespace Supremacy.Text
{
    [Serializable]
    [DefaultProperty("Text")]
    [ContentProperty("Text")]
    [DictionaryKeyProperty("Language")]
    public class LocalizedStringValue : SupportInitializeBase
    {
        private string _text;
        private CultureInfo _language;

        public string Text
        {
            get => _text;
            set
            {
                VerifyInitializing();
                _text = value;
            }
        }

        [TypeConverter(typeof(LanguageConverter))]
        public CultureInfo Language
        {
            get => _language;
            set
            {
                VerifyInitializing();
                _language = value;
            }
        }
    }
}