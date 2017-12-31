// GameEvents.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Events;

using Supremacy.Universe;

namespace Supremacy.Client.Events
{
    public static class GameEvents
    {
        public static readonly CompositePresentationEvent<TradeRoute> TradeRouteEstablished = new CompositePresentationEvent<TradeRoute>();
        public static readonly CompositePresentationEvent<TradeRoute> TradeRouteCancelled = new CompositePresentationEvent<TradeRoute>();
    }
}