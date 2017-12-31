// ITradeCenter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Types;
using System.Collections.Generic;
using Supremacy.Universe;

namespace Supremacy.Economy
{
    public interface ITradeCenter : IUniverseObject
    {
        Meter CreditsFromTrade { get; }
        IList<TradeRoute> TradeRoutes { get; }
        void UpdateCreditsFromTrade();
    }

    public static class TradeCenter
    {
        public static void ResetCreditsFromTrade(this ITradeCenter source)
        {
            source.CreditsFromTrade.SaveCurrentAndResetToBase();
        }
    }
}