using System;
using System.Globalization;
using System.Windows.Data;

namespace Supremacy.Client.Data
{
    public class EnumToInt32Converter : ValueConverter<EnumToInt32Converter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = value as Enum;
            if (enumValue != null)
                return System.Convert.ToInt32(value);
            return Binding.DoNothing;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = value as int?;
            if (intValue.HasValue && targetType.IsEnum)
                return Enum.ToObject(targetType, intValue.Value);
            return Binding.DoNothing;
        }
    }
}