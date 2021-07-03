// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Orbitals;
using System;
using System.Collections.Generic;

namespace Supremacy.Universe
{
    public static class MapHelper
    {
        public static IList<Sector> GetSectorsWithinRadius(Sector origin, int radius)
        {
            List<Sector> sectors = new List<Sector>();
            SectorMap map = GameContext.Current.Universe.Map;
            MapLocation location = origin.Location;
            int startX = Math.Max(0, location.X - radius);
            int startY = Math.Max(0, location.Y - radius);
            int endX = Math.Min(map.Width - 1, location.X + radius);
            int endY = Math.Min(map.Height - 1, location.Y + radius);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Sector sector = map[x, y];

                    if (MapLocation.GetDistance(location, sector.Location) <= radius)
                    {
                        sectors.Add(sector);
                    }
                }
            }

            return sectors;
        }

        public static MapLocation FindNearestFriendlySector(Fleet fleet)
        {
            UniverseObject closestSite = GameContext.Current.Universe.FindNearestOwned(
                fleet.Location,
                fleet.Owner,
                (UniverseObject item) => (item is Station || item is StarSystem) && item.Owner == fleet.Owner);

            if (closestSite != null)
            {
                return closestSite.Location;
            }

            return fleet.Location;
        }
    }
}
