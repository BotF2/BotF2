// MapHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Game;
using Supremacy.Orbitals;

namespace Supremacy.Universe
{
    public static class MapHelper
    {
        public static IList<Sector> GetSectorsWithinRadius(Sector origin, int radius)
        {
            var sectors = new List<Sector>();
            var map = GameContext.Current.Universe.Map;
            var location = origin.Location;
            var startX = Math.Max(0, location.X - radius);
            var startY = Math.Max(0, location.Y - radius);
            var endX = Math.Min(map.Width - 1, location.X + radius);
            var endY = Math.Min(map.Height - 1, location.Y + radius);

            for (var x = startX; x < endX; x++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var sector = map[x, y];

                    if (MapLocation.GetDistance(location, sector.Location) <= radius)
                        sectors.Add(sector);
                }
            }

            return sectors;
        }

        public static MapLocation FindNearestFriendlySector(Fleet fleet)
        {
            var closestSite = GameContext.Current.Universe.FindNearestOwned(
                fleet.Location,
                fleet.Owner,
                (UniverseObject item) => (item is Station || item is StarSystem) && item.Owner == fleet.Owner);

            if (closestSite != null)
                return closestSite.Location;

            return fleet.Location;
        }
    }
}
