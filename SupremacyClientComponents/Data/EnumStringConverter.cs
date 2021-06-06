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
                return stringValue;

            if (CharacterCasing == CharacterCasing.Upper)
                return stringValue.ToUpper();

            return stringValue.ToLower();
        }

        private static string ConvertToString(object value)
        {
            Enum enumValue = value as Enum;
            if (enumValue == null)
                return (value != null) ? value.ToString() : string.Empty;

            LocalizedTextDatabase textDatabase = LocalizedTextDatabase.Instance;

            LocalizedTextGroup group;

            if (!textDatabase.Groups.TryGetValue(enumValue.GetType(), out @group))
                return string.Format("{{! Unknown Text Group: {0} !}}", @group);

            LocalizedString localizedString;

            string entryName = enumValue.ToString();

            if (!@group.Entries.TryGetValue(entryName, out localizedString))
                return entryName;

            return localizedString.LocalText;
        }
    }
}