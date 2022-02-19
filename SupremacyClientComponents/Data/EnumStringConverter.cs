using System;
using System.Globalization;

using System.Windows.Controls;

using Supremacy.Text;

namespace Supremacy.Client.Data
{
    public class LocalizedEnumStringConverter : ValueConverter<EnumStringConverter>
    {
        public CharacterCasing CharacterCasing { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = ConvertToString(value);
            if (stringValue == null || CharacterCasing == CharacterCasing.Normal)
            {
                return stringValue;
            }

            if (CharacterCasing == CharacterCasing.Upper)
            {
                return stringValue.ToUpper();
            }

            return stringValue.ToLower();
        }

        private static string ConvertToString(object value)
        {
            if (!(value is Enum enumValue))
            {
                return (value != null) ? value.ToString() : string.Empty;
            }

            LocalizedTextDatabase textDatabase = LocalizedTextDatabase.Instance;


            if (!textDatabase.Groups.TryGetValue(enumValue.GetType(), out LocalizedTextGroup @group))
            {
                return string.Format("{{! Unknown Text Group: {0} !}}", @group);
            }


            string entryName = enumValue.ToString();

            if (!@group.Entries.TryGetValue(entryName, out LocalizedString localizedString))
            {
                return entryName;
            }

            return localizedString.LocalText;
        }
    }
}