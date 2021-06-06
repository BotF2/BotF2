// 
// EffectiveTradeRouteIncomeConverter.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Globalization;
using System.Windows.Data;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Universe;

using System.Linq;
using Supremacy.Client.Context;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(TradeRoute), typeof(int))]
    public class EffectiveTradeRouteIncomeConverter : ValueConverter<EffectiveTradeRouteIncomeConverter>
    {
        protected Civilization LocalPlayerEmpire
        {
            get
            {
                IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
                if (appContext == null)
                    return null;
                return appContext.LocalPlayer.Empire;
            }
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TradeRoute tradeRoute = value as TradeRoute;
            if (tradeRoute == null || !tradeRoute.IsAssigned)
                return 0;

            return GetCreditsForTradeRoute(tradeRoute);
        }

        private int GetCreditsForTradeRoute(TradeRoute tradeRoute)
        {
            Civilization empire = LocalPlayerEmpire;
            if (empire == null)
                return tradeRoute.Credits;

            Colony colony;

            if (tradeRoute.SourceColony.OwnerID == empire.CivID)
                colony = tradeRoute.SourceColony;
            else if (tradeRoute.TargetColony.OwnerID == empire.CivID)
                colony = tradeRoute.TargetColony;
            else
                colony = null;

            if (colony == null)
                return tradeRoute.Credits;

            double bonus = colony.Buildings
                              .Where(o => o.IsActive)
                              .SelectMany(o => o.BuildingDesign.Bonuses)
                              .Where(o => o.BonusType == BonusType.PercentTradeIncome)
                              .Sum(o => 0.01 * o.Amount);

            return (int)((1.0 + bonus) * tradeRoute.Credits);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object MultiConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return null;

            int total = 0;

            for (int i = 0; i < values.Length; i++)
            {
                TradeRoute tradeRoute = values[i] as TradeRoute;
                if (tradeRoute != null && tradeRoute.IsAssigned)
                    total += GetCreditsForTradeRoute(tradeRoute);
            }

            return total;
        }

        public override object[] MultiConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}