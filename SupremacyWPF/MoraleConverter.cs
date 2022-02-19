// MoraleConverter.cs
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
using System.Windows.Media;
using Supremacy.Game;
using Supremacy.Types;

namespace Supremacy.Client
{
    [ValueConversion(typeof(int), typeof(string))]
    public class MoraleConverter : IValueConverter
    {
        public static readonly MoraleConverter Instance = new MoraleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Supremacy.Data.Table table = GameContext.Current.Tables.MoraleTables["MoraleLabels"];
            int morale = (int)value;
            string result = value.ToString();
            if (table != null)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (morale >= Number.ParseInt32(table[i][0]))
                    {
                        result = table[i][1];
                        break;
                    }
                }
            }
            if (parameter != null)
            {
                result = result.ToUpperInvariant();
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(int), typeof(Brush))]
    public class MoraleBrushConverter : IValueConverter
    {
        public static readonly MoraleBrushConverter Instance = new MoraleBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Supremacy.Data.Table table = GameContext.Current.Tables.MoraleTables["MoraleColors"];
            int morale = (int)value;
            if (table != null)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (morale >= Number.ParseInt32(table[i][0]))
                    {
                        return new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(table[i][1]));
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
