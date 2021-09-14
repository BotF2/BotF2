// StringCaseConverter.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Supremacy.Client
{
    [ValueConversion(typeof(object), typeof(string))]
    public class CharacterCasingConverter : IValueConverter
    {
        public CharacterCasingConverter()
        {
            Casing = CharacterCasing.Normal;
        }

        public CharacterCasing Casing
        {
            get; set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }

            switch (Casing)
            {
                case CharacterCasing.Lower:
                    return value.ToString().ToLower();
                case CharacterCasing.Upper:
                    return value.ToString().ToUpper();
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}