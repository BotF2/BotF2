using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(IEnumerable), typeof(bool))]
    public sealed class ContainsConverter : ValueConverter<ContainsConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return false;

            var collection = value as IEnumerable;
            if (collection != null)
                return collection.Cast<object>().Contains(parameter);

            return false;
        }
    }
}