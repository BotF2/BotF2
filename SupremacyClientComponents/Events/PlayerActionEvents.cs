// PlayerActionEvents.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity.Utility;

using Supremacy.Orbitals;

namespace Supremacy.Client.Events
{
    public static class PlayerActionEvents
    {
        public static readonly CompositePresentationEvent<Fleet> FleetRouteUpdated;
        public static readonly CompositePresentationEvent<Pair<Fleet, FleetOrder>> FleetOrderAssigned;

        static PlayerActionEvents()
        {
            FleetRouteUpdated = new CompositePresentationEvent<Fleet>();
            FleetOrderAssigned = new CompositePresentationEvent<Pair<Fleet, FleetOrder>>();
        }
    }
}