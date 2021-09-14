using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using Supremacy.Client.Data;

namespace Supremacy.Client
{
    [ValueConversion(typeof(object), typeof(string))]
    public sealed class StringFormatConverter : ValueConverter<StringFormatConverter>
    {
        public CharacterCasing CharacterCasing { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string result = !(parameter is string format) ? value.ToString() : string.Format(format, value);

            if (CharacterCasing == CharacterCasing.Upper)
            {
                return result.ToUpper();
            }

            if (CharacterCasing == CharacterCasing.Lower)
            {
                return result.ToLower();
            }

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object[] MultiConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string formatString))
            {
                return null;
            }

            return string.Format(formatString, values);
        }
    }
}