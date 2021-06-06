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

        private object _trueValue = True;
        private object _falseValue = False;

        public object TrueValue
        {
            get { return _trueValue; }
            set { _trueValue = value; }
        }

        public object FalseValue
        {
            get { return _falseValue; }
            set { _falseValue = value; }
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedCondition = ValueConversionHelper.Convert(value, typeof(bool), culture: culture);
            if (convertedCondition == DependencyProperty.UnsetValue || !Equals(convertedCondition, True))
                return ValueConversionHelper.Convert(_falseValue, targetType, parameter, culture);
            return ValueConversionHelper.Convert(_trueValue, targetType, parameter, culture);
        }

        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                object convertedCondition = ValueConversionHelper.Convert(value, typeof(bool), culture: culture);
                if (convertedCondition == DependencyProperty.UnsetValue || !Equals(convertedCondition, True))
                    return ValueConversionHelper.Convert(_falseValue, targetType, parameter, culture);
            }
            return ValueConversionHelper.Convert(_trueValue, targetType, parameter, culture);
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