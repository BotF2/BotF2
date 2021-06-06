// CivHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Universe;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Game
{
    /// <summary>
    /// Helper class containing logic related to the <see cref="Civilization"/> type.
    /// </summary>
    public static class CivHelper
    {
        /// <summary>
        /// Gets the current ranking of the specified <see cref="Civilization"/>.
        /// </summary>
        /// <param name="civ">The <see cref="Civilization"/>.</param>
        /// <returns>The current ranking.</returns>
        public static int GetRank(Civilization civ)
        {

            //This is for a game where the victory condition is to control most of the map.
            //Hideously inefficient, but is not used at the moment, it's just to flesh the idea
            //out for when we do actually start using it
            Dictionary<Civilization, int> sectorsOwned = new Dictionary<Civilization, int>();
            for (int x = 0; x <= GameContext.Current.Universe.Map.Width; x++)
            {
                for (int y = 0; y <= GameContext.Current.Universe.Map.Height; y++)
                {
                    if (GameContext.Current.Universe.Map[x, y].IsOwned)
                    {
                        Civilization owner = GameContext.Current.Universe.Map[x, y].Owner;
                        if (!sectorsOwned.ContainsKey(owner))
                        {
                            sectorsOwned.Add(owner, 0);
                        }
                        sectorsOwned[owner]++;

                        //TODO: Need to take account of alliances
                    }
                }
            }

            if (!sectorsOwned.ContainsKey(civ))
            {
                return 0;
            }

            IOrderedEnumerable<KeyValuePair<Civilization, int>> sectorsOwnerSorted = sectorsOwned.OrderByDescending(x => x.Value);
            for (int i = 0; i <= sectorsOwnerSorted.Count(); i++)
            {
                if (sectorsOwnerSorted.ElementAt(i).Key == civ)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int GetClosestDistanceBetweenBorderClaims(Civilization civ1, Civilization civ2)
        {
            int nearestDistance = -1;
            SectorMap map = GameContext.Current.Universe.Map;
            SectorClaimGrid sectorClaims = GameContext.Current.SectorClaims;
            List<MapLocation> locations1 = new List<MapLocation>();
            List<MapLocation> locations2 = new List<MapLocation>();
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Width; y++)
                {
                    MapLocation location = new MapLocation(x, y);
                    Sector sector = GameContext.Current.Universe.Map[location];
                    StarSystem system = (sector != null) ? sector.System : null;
                    if ((system != null) && system.IsInhabited)
                    {
                        if ((sector.System.Name == civ1.HomeSystemName)
                            || (sector.System.Name == civ2.HomeSystemName))
                        {
                            continue;
                        }
                    }
                    if (sectorClaims.IsClaimedByCiv(location, civ1))
                    {
                        if (sectorClaims.IsClaimedByCiv(location, civ2))
                        {
                            return 0;
                        }
                        locations1.Add(location);
                    }
                    else if (sectorClaims.IsClaimedByCiv(location, civ2))
                    {
                        locations2.Add(location);
                    }
                }
            }
            foreach (MapLocation location1 in locations1)
            {
                foreach (MapLocation location2 in locations2)
                {
                    int distance = MapLocation.GetDistance(location1, location2);
                    if ((distance < nearestDistance) || (nearestDistance == -1))
                    {
                        nearestDistance = MapLocation.GetDistance(location1, location2);
                    }
                }
            }
            return nearestDistance;
        }
    }
}
