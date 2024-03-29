// NetValueConverter.cs
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

using Microsoft.Practices.ServiceLocation;

using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Types;
using Supremacy.Client.Context;

namespace Supremacy.Client
{
    [ValueConversion(typeof(int), typeof(Brush))]
    public class NetValueBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value < 0 ? Brushes.Red : parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class NetValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? "+" + value : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Number.ParseInt32(value.ToString());
        }
    }

    [ValueConversion(typeof(object), typeof(Brush))]
    public class RelationshipStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush defaultBrush = Brushes.Transparent;

            Civilization civ = value as Civilization;
            if (civ != null)
            {
                IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
                if (appContext == null)
                {
                    return defaultBrush;
                }

                if (civ.CivID == appContext.LocalPlayer.EmpireID)
                {
                    return defaultBrush;
                }

                Civilization playerCiv = appContext.LocalPlayer.Empire;
                if (playerCiv == null)
                {
                    return defaultBrush;
                }

                if (!DiplomacyHelper.IsContactMade(playerCiv, civ))
                {
                    return defaultBrush;
                }

                value = DiplomacyHelper.GetForeignPowerStatus(appContext.LocalPlayer.Empire, civ);
            }
            else if (value is ForeignPowerStatus status)
            {
                switch (status)
                {
                    case ForeignPowerStatus.OwnerIsSubjugated:
                    case ForeignPowerStatus.CounterpartyIsSubjugated:
                        return Brushes.Orange;

                    case ForeignPowerStatus.AtWar:
                        return Brushes.Crimson;

                    case ForeignPowerStatus.Neutral:
                        return Brushes.Silver;

                    case ForeignPowerStatus.Peace:
                    case ForeignPowerStatus.Friendly:
                    case ForeignPowerStatus.Affiliated:
                        return Brushes.LimeGreen;

                    case ForeignPowerStatus.OwnerIsMember:
                    case ForeignPowerStatus.CounterpartyIsMember:
                        return Brushes.Violet;

                    case ForeignPowerStatus.Allied:
                        return Brushes.DeepSkyBlue;

                    case ForeignPowerStatus.OwnerIsUnreachable:
                    case ForeignPowerStatus.CounterpartyIsUnreachable:
                    // TODO: Figure out the correct brush

                    default:
                        return defaultBrush;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
