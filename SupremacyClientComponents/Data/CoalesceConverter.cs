using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(object), typeof(object))]
    public class CoalesceConverter : ValueConverter<CoalesceConverter>
    {
        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.FirstOrDefault(o => o != null) ?? parameter;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}