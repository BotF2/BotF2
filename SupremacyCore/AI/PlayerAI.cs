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
//using System.Linq;
using Obtics.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
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

        #region DoTurn from GameEngine
        public static void DoTurn(Civilization targetCiv)
        {
            if (targetCiv.IsEmpire && targetCiv.CivID != 6 && targetCiv.SpiedCivList != null)
            {
                List<Civilization> spyingCivs = (List<Civilization>)GameContext.Current.Civilizations.Where(o => o.IsEmpire && o.CivID != 6).ToList();

                foreach (Civilization spyingCiv in spyingCivs)
                {
                    if (targetCiv.SpiedCivList.Contains(spyingCiv))
                    {
                        if (DiplomacyHelper.AreAtWar(spyingCiv, targetCiv))
                        {
                            DoSpySabotageMission(spyingCiv, targetCiv);                         
                        }
                        //else if (DiplomacyHelper.AreAllied(spyingCiv, targetCiv) || DiplomacyHelper.AreFriendly(spyingCiv, targetCiv))
                        //{
                        //    // do things
                        //}
                        else if (DiplomacyHelper.AreNeutral(spyingCiv, targetCiv))
                        {
                            if (spyingCiv.Traits.Contains(CivTraits.Hostile.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Subversive.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Warlike.ToString()))
                            {
                                if (RandomHelper.Random(3) == 0)
                                    DoSpySabotageMission(spyingCiv, targetCiv);
                                else IntelHelper.SabotageStealResearch(spyingCiv, targetCiv, "No one");
                            }
                        }
                    }
                }
            }
            if (targetCiv.IsEmpire && !targetCiv.IsHuman && GameContext.Current.TurnNumber > 5)
            {
                var possibleTotalWarCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList();
                foreach (Civilization possibleTotalWarCiv in possibleTotalWarCivs)
                {
                    var diplomat = Diplomat.Get(targetCiv);
                    ForeignPower foreignPower = diplomat.GetForeignPower(possibleTotalWarCiv);
                    if (DiplomacyHelper.AreAtWar(possibleTotalWarCiv, targetCiv))
                    {
                        var maintenaceValue = GameContext.Current.CivilizationManagers[possibleTotalWarCiv].MaintenanceCostLastTurn;
                        if (maintenaceValue < GameContext.Current.CivilizationManagers[targetCiv].MaintenanceCostLastTurn * 1.2 && possibleTotalWarCiv.TotalWarCivilization == null)
                        {
                            //foreignPower.BeginTotalWar(); // if there already is total war by the target civ a new one will not be created over in ForeignPower.cs
                            possibleTotalWarCiv.TotalWarCivilization = targetCiv;
                            GameLog.Client.AI.DebugFormat("{0} set as TOTALWAR!!! by {1} ", targetCiv.Name, possibleTotalWarCiv.Name);
                        }
                        else
                        {
                        possibleTotalWarCiv.TotalWarCivilization = null;
                            //foreignPower.EndTotalWar();
                        }
                    }
                    else possibleTotalWarCiv.TotalWarCivilization = null; // if civs are no longer at war then total war ends.
                }
            }
        }
        #endregion

        public static void DoSpySabotageMission(Civilization spyingCiv, Civilization targetCiv)
        {
            int decide = RandomHelper.Random(5);
            switch (decide)
            {
                case 0:
                    {
                        IntelHelper.SabotageEnergy(spyingCiv, targetCiv, "No one");
                        break;
                    }
                case 1:
                    {
                        IntelHelper.SabotageFood(spyingCiv, targetCiv, "No one");
                        break;
                    }
                case 2:
                    {
                        IntelHelper.SabotageIndustry(spyingCiv, targetCiv, "No one");
                        break;
                    }
                case 3:
                    {
                        IntelHelper.SabotageStealCredits(spyingCiv, targetCiv, "No one");
                        break;
                    }
                case 4:
                    {
                        IntelHelper.SabotageStealResearch(spyingCiv, targetCiv, "No one");
                        break;
                    }
                default:
                    break;
            }
        }

        public static ConvexHullSet CreateDesiredBorders(Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            SectorMap map = GameContext.Current.Universe.Map;
            SectorClaimGrid sectorClaims = GameContext.Current.SectorClaims;
            List<IEnumerable<MapLocation>> disjointSets = new List<IEnumerable<MapLocation>>();
            List<ConvexHull> convexHulls = new List<ConvexHull>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapLocation location = new MapLocation(x, y);
                    if (!sectorClaims.IsClaimedByCiv(location, civ))
                    {
                        continue;
                    }

                    List<MapLocation> disjointSet = new List<MapLocation>(1) { location };
                    disjointSets.Add(disjointSet);
                }
            }

            for (int i = 0; i < disjointSets.Count; i++)
            {
                for (int j = 0; j < disjointSets.Count; j++)
                {
                    bool merge = false;
                    if (i == j)
                    {
                        continue;
                    }

                    foreach (MapLocation location1 in disjointSets[i])
                    {
                        foreach (MapLocation location2 in disjointSets[j])
                        {
                            if (MapLocation.GetDistance(location1, location2) > MaxDistanceInConvexHull)
                            {
                                continue;
                            }

                            merge = true;
                            break;
                        }
                        if (merge)
                        {
                            break;
                        }
                    }
                    if (!merge)
                    {
                        continue;
                    }

                    disjointSets[i] = disjointSets[i].Union(disjointSets[j]);
                    disjointSets.RemoveAt(j);
                    if (i > j)
                    {
                        --i;
                    }

                    --j;
                }
            }

            foreach (IEnumerable<MapLocation> disjointSet in disjointSets)
            {
                convexHulls.Add(new ConvexHull(disjointSet));
            }

            return new ConvexHullSet(convexHulls);
        }

        public static int GetCreditTradeValuePercent(Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException(nameof(who));
            }

            int value = 1;
            if (IsInFinancialTrouble(who))
            {
                value++;
            }

            return 100 * value;
        }

        public static int GetFleetDanger(Fleet fleet, int range,  bool anyDanger) // bool testMoves,
        {
            return GetSectorDanger(fleet.Owner, fleet.Sector, range); //, testMoves);
        }

        public static int GetSectorDanger(Civilization who, Sector sector, int range) //, bool testMoves)
        {
            if (who == null)
            {
                throw new ArgumentNullException(nameof(who));
            }

            if (sector == null)
            {
                throw new ArgumentNullException(nameof(sector));
            }

            SectorMap map = GameContext.Current.Universe.Map;

            int count = 0;
            int borderDanger = 0;

            if (range < 0)
            {
                range = DangerRange; 
            }

            for (int dX = -range; dX < range; dX++)
            {
                for (int dY = -range; dY < range; dY++)
                {
                    Sector loopSector = map[sector.Location.X + dX, sector.Location.Y + dY];
                    if (loopSector == null || loopSector.Owner == null)
                    {
                        continue;
                    }
                    var distance = MapLocation.GetDistance(sector.Location, loopSector.Location);

                    if (DiplomacyHelper.AreAtWar(who, loopSector.Owner) && distance <= 2)
                    {
                        borderDanger++;
                    }

                    foreach (Fleet fleet in GameContext.Current.Universe.FindAt<Fleet>(loopSector.Location))
                    {
                        if (!DiplomacyHelper.AreAtWar(who, fleet.Owner))
                        {
                            continue;
                        }

                        if (!fleet.IsCombatant)
                        {
                            continue;
                        }

                        FleetView fleetView = FleetView.Create(who, fleet);
                        if (!fleetView.IsPresenceKnown)
                        {
                            continue;
                        }

                        if (DiplomacyHelper.IsTravelAllowed(fleet.Owner, sector) && ( fleet.Speed >= distance)) // || !testMoves ||
                        {
                            ++count;
                        }
                    }
                }
            }

            if (!IsHuman(who))
            {
                count += borderDanger;
            }
           // GameLog.Client.AI.DebugFormat("* Sector Danger ={0}",count);
           // count = 20;
            return count;
        }

        public static bool IsHuman(Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException(nameof(who));
            }

            return PlayerContext.Current.IsHumanPlayer(who);
        }

        public static bool IsInFinancialTrouble(Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException(nameof(who));
            }

            try
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[who];
                //TODO: Might this be better checking if the civilization isn't in a negative balance?
                if (civManager?.Credits.LastChange < 0)
                {
                    return true;
                }
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
