// MapLocationConverter.cs
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

using Supremacy.Universe;

namespace Supremacy.Client
{
    [ValueConversion(typeof(MapLocation), typeof(string))]
    public class MapLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MapLocation)
            {
                return ((MapLocation)value).ToString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
