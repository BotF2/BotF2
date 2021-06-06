using System;
using System.Globalization;
using System.Windows.Data;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsTypeConverter : ValueConverter<IsTypeConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = parameter as Type;
            if (type == null)
                return false;

            if (value == null)
                return false;

            return type.IsInstanceOfType(value);
        }
    }
}