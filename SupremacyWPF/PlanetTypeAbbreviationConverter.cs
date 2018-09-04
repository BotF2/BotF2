// PlanetTypeAbbreviationConverter.cs
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
    [ValueConversion(typeof(PlanetType), typeof(string))]
    public class PlanetTypeAbbreviationConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlanetType)
            {
                string result;
                switch ((PlanetType)value)
                {
                    case PlanetType.Barren:
                        result = "J";
                        break;
                    case PlanetType.Desert:
                        result = "G";
                        break;
                    case PlanetType.Crystalline:
                        result = "D";
                        break;
                    case PlanetType.GasGiant:
                        result = "B";
                        break;
                    case PlanetType.Volcanic:
                        result = "K";
                        break;
                    case PlanetType.Oceanic:
                        result = "O";
                        break;
                    case PlanetType.Rogue:
                        result = "R";
                        break;
                    case PlanetType.Demon:
                        result = "Y";
                        break;
                    case PlanetType.Jungle:
                        result = "L";
                        break;
                    case PlanetType.Terran:
                        result = "M";
                        break;
                    case PlanetType.Arctic:
                        result = "P";
                        break;
                    case PlanetType.Asteroids:
                        result = "A";
                        break;
                    default:
                        result = string.Empty;
                        break;
                }
                return result;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
}