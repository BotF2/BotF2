// NonNullValueConverter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;

namespace Supremacy.Client
{
    [ValueConversion(typeof(object), typeof(object))]
    public class NonNullValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length == 0))
                return null;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                    return values[i];
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] {value};
        }
        #endregion
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class MatchExistsValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length == 0))
                return false;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == parameter)
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
        #endregion
    }
}