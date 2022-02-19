using System;
using System.Globalization;
using System.Windows;

using System.Windows.Data;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanValueConverter : ValueConverter<BooleanValueConverter>
    {
        public static readonly object True = true;
        public static readonly object False = false;

        public object TrueValue { get; set; } = True;

        public object FalseValue { get; set; } = False;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedCondition = ValueConversionHelper.Convert(value, typeof(bool), culture: culture);
            if (convertedCondition == DependencyProperty.UnsetValue || !Equals(convertedCondition, True))
            {
                return ValueConversionHelper.Convert(FalseValue, targetType, parameter, culture);
            }

            return ValueConversionHelper.Convert(TrueValue, targetType, parameter, culture);
        }

        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                object convertedCondition = ValueConversionHelper.Convert(value, typeof(bool), culture: culture);
                if (convertedCondition == DependencyProperty.UnsetValue || !Equals(convertedCondition, True))
                {
                    return ValueConversionHelper.Convert(FalseValue, targetType, parameter, culture);
                }
            }
            return ValueConversionHelper.Convert(TrueValue, targetType, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object[] MultiConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}