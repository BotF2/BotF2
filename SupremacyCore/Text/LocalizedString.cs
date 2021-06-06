using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

using Supremacy.Annotations;
using Supremacy.Resources;
using Supremacy.Types;

namespace Supremacy.Text
{
    [Serializable]
    [ContentProperty("Values")]
    [DefaultProperty("Values")]
    public class LocalizedString : SupportInitializeBase
    {
        private readonly LocalizedStringValueCollection _values;
        private object _name;
        
        public LocalizedString()
        {
            _values = new LocalizedStringValueCollection();
        }

        public object Name
        {
            get { return _name; }
            set
            {
                VerifyInitializing();
                _name = value;
            }
        }

        public LocalizedStringValueCollection Values => _values;

        public string LocalText
        {
            get
            {
                LocalizedStringValue value;
                
                if (TryGetValue(ResourceManager.CurrentCulture, false, out value))
                    return value.Text;

                return null;
            }
        }

        public void Merge([NotNull] LocalizedString other, bool overwrite = false)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (LocalizedStringValue value in other._values)
            {
                if (_values.Contains(value.Language))
                {
                    if (!overwrite)
                        continue;

                    _values.Remove(value.Language);
                    _values.Add(value);
                }
                else
                {
                    _values.Add(value);
                }
            }
        }

        public bool TryGetValue(CultureInfo language, bool exactMatchOnly, out LocalizedStringValue value)
        {
            CultureInfo actualLanguage;
            return TryGetValue(language, exactMatchOnly, out value, out actualLanguage);
        }

        public bool TryGetValue(CultureInfo language, bool exactMatchOnly, out LocalizedStringValue value, out CultureInfo actualLanguage)
        {
            actualLanguage = language;

            if (exactMatchOnly)
                return _values.TryGetValue(language, out value);

            if (_values.TryGetValue(language, out value))
                return true;

            while (!language.IsNeutralCulture &&
                   language.Parent != CultureInfo.InvariantCulture)
            {
                language = language.Parent;
                actualLanguage = language;

                if (_values.TryGetValue(language, out value))
                    return true;
            }

            language = ResourceManager.NeutralCulture;
            actualLanguage = language;

            if (_values.TryGetValue(language, out value))
                return true;

            while (!language.IsNeutralCulture &&
                   language.Parent != CultureInfo.InvariantCulture)
            {
                language = language.Parent;
                actualLanguage = language;

                if (_values.TryGetValue(language, out value))
                    return true;
            }

            if (language != CultureInfo.InvariantCulture &&
                _values.TryGetValue(CultureInfo.InvariantCulture, out value))
            {
                actualLanguage = CultureInfo.InvariantCulture;
                return true;
            }

            actualLanguage = null;
            return false;
        }

        public override string ToString()
        {
            return LocalText;
        }
    }
}