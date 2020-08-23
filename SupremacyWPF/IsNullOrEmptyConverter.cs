using System;
using System.Globalization;
using System.Windows.Data;

namespace Supremacy.Client
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsNullOrEmptyConverter : IValueConverter
    {
        #region Implementation of IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? true : (object)(value.ToString() == string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}