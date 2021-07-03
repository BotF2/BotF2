using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

using Supremacy.Resources;
using Supremacy.Text;

namespace Supremacy.Markup
{
    [MarkupExtensionReturnType(typeof(string))]
    public sealed class LocalizedStringExtension : MarkupExtension
    {
        public object Group { get; set; }
        public string Entry { get; set; }
        public CharacterCasing CharacterCasing { get; set; }

        #region Overrides of MarkupExtension
        public override object ProvideValue(IServiceProvider serviceProvider)
        {


            if (!LookupValue(out string localText, out CultureInfo language))
            {
                return localText;
            }

            switch (CharacterCasing)
            {
                case CharacterCasing.Lower:
                    return language.TextInfo.ToLower(localText);
                case CharacterCasing.Upper:
                    return language.TextInfo.ToUpper(localText);
            }

            return localText;
        }

        private bool LookupValue(out string value, out CultureInfo culture)
        {
            culture = ResourceManager.NeutralCulture;

            LocalizedTextDatabase textDatabase = LocalizedTextDatabase.Instance;

            if (Group == null)
            {
                if (string.IsNullOrWhiteSpace(Entry))
                {
                    value = "{! Group and Key Required !}";
                }
                else
                {
                    value = string.Format("{{! Group Required for Entry {0} !}}", Entry);
                }

                return false;
            }


            if (!textDatabase.Groups.TryGetValue(Group, out LocalizedTextGroup group))
            {
                value = string.Format("{{! Unknown Text Group: {0} !}}", Group);
                return false;
            }


            if (!group.Entries.TryGetValue(Entry, out LocalizedString entry))
            {
                entry = group.DefaultEntry;

                if (entry == null)
                {
                    if (group.Entries.Count == 0)
                    {
                        value = string.Format("{{! Empty Text Group: {0} !}}", Group);
                    }
                    else
                    {
                        value = string.Format("{{! Missing Text Entry: {0} !}}", Entry);
                    }

                    return false;
                }
            }


            if (entry.TryGetValue(ResourceManager.CurrentCulture, true, out LocalizedStringValue result, out culture))
            {
                value = result.Text;
                return true;
            }

            if (entry.TryGetValue(ResourceManager.NeutralCulture, true, out result, out culture))
            {
                value = result.Text;
                return true;
            }

            culture = ResourceManager.NeutralCulture;
            value = entry.LocalText;

            return true;
        }

        #endregion
    }
}