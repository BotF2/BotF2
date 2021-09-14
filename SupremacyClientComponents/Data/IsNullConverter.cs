using System;
using System.Globalization;
using System.Linq;

namespace Supremacy.Client.Data
{
    public class IsNullConverter : BooleanValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? TrueValue : FalseValue;
        }

        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(o => o is null) ? TrueValue : FalseValue;
        }
    }
}