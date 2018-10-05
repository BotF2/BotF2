// 
// PlayerAI.cs
// 
// Copyright (c) 2011-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.AI
{
    public static class PlayerAI
    {
        #region Constants

        private const int DangerRange = 4;
        private const int MaxDistanceInConvexHull = 3;

        #endregion

        #region Methods

        public static ConvexHullSet CreateDesiredBorders(Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var map = GameContext.Current.Universe.Map;
            var sectorClaims = GameContext.Current.SectorClaims;
            var disjointSets = new List<IEnumerable<MapLocation>>();
            var convexHulls = new List<ConvexHull>();

            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    var location = new MapLocation(x, y);
                    if (!sectorClaims.IsClaimedByCiv(location, civ))
                        continue;
                    var disjointSet = new List<MapLocation>(1) { location };
                    disjointSets.Add(disjointSet);
                }
            }

            for (var i = 0; i < disjointSets.Count; i++)
            {
                for (var j = 0; j < disjointSets.Count; j++)
                {
                    var merge = false;
                    if (i == j)
                        continue;
                    foreach (var location1 in disjointSets[i])
                    {
                        foreach (var location2 in disjointSets[j])
                        {
                            if (MapLocation.GetDistance(location1, location2) > MaxDistanceInConvexHull)
                                continue;
                            merge = true;
                            break;
                        }
                        if (merge)
                            break;
                    }
                    if (!merge)
                        continue;
                    disjointSets[i] = disjointSets[i].Union(disjointSets[j]);
                    disjointSets.RemoveAt(j);
                    if (i > j)
                        --i;
                    --j;
                }
            }

            foreach (var disjointSet in disjointSets)
                convexHulls.Add(new ConvexHull(disjointSet));

            return new ConvexHullSet(convexHulls);
        }

        public static int GetCreditTradeValuePercent(Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            var value = 1;
            if (IsInFinancialTrouble(who))
                value++;
            return (100 * value);
        }

        public static int GetFleetDanger(Fleet fleet, int range, bool testMoves, bool anyDanger)
        {
            return GetSectorDanger(fleet.Owner, fleet.Sector, range, testMoves);
        }

        public static int GetSectorDanger(Civilization who, Sector sector, int range, bool testMoves)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            if (sector == null)
                throw new ArgumentNullException("sector");

            var map = GameContext.Current.Universe.Map;

            var count = 0;
            var borderDanger = 0;

            if (range < 0)
                range = DangerRange;

            for (var dX = -range; dX < range; dX++)
            {
                for (var dY = -range; dY < range; dY++)
                {
                    var loopSector = map[sector.Location.X + dX, sector.Location.Y + dY];
                    if (loopSector == null)
                        continue;

                    var distance = MapLocation.GetDistance(sector.Location, loopSector.Location);

                    if (DiplomacyHelper.AreAtWar(who, loopSector.Owner))
                    {
                        if (distance <= 2)
                            borderDanger++;
                    }

                    foreach (var fleet in GameContext.Current.Universe.FindAt<Fleet>(loopSector.Location))
                    {
                        if (!DiplomacyHelper.AreAtWar(who, fleet.Owner))
                            continue;

                        if (!fleet.IsCombatant)
                            continue;

                        var fleetView = FleetView.Create(who, fleet);
                        if (!fleetView.IsPresenceKnown)
                            continue;

                        if (UnitAI.CanEnterSector(sector, fleet.Owner) && (!testMoves || (fleet.Speed >= distance)))
                            ++count;
                    }
                }
            }

            if (IsHuman(who))
                count += borderDanger;

            return count;
        }

        public static bool IsHuman(Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            return PlayerContext.Current.IsHumanPlayer(who);
        }

        public static bool IsInFinancialTrouble(Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            try
            {
                var civManager = GameContext.Current.CivilizationManagers[who];
                //TODO: Might this be better checking if the civilization isn't in a negative balance?
                if ((civManager != null) && (civManager.Credits.LastChange < 0))
                    return true;
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }

            return false;
        }
        #endregion
    }
}
