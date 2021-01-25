// File:PlayerAI.cs
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
        public static void DoTurn(Civilization Civ)  
        {
            if (Civ.IsEmpire && Civ.CivID != 6 && Civ.SpiedCivList != null)  // Spy
            {
                List<Civilization> spyingCivs = (List<Civilization>)GameContext.Current.Civilizations.Where(o => o.IsEmpire && o.CivID != 6).ToList();

                foreach (Civilization spyingCiv in spyingCivs)
                {
                    if (Civ.SpiedCivList.Contains(spyingCiv))
                    {
                        if (DiplomacyHelper.AreAtWar(spyingCiv, Civ))
                        {
                            DoSpySabotageMission(spyingCiv, Civ);                         
                        }
                        //else if (DiplomacyHelper.AreAllied(spyingCiv, Civ) || DiplomacyHelper.AreFriendly(spyingCiv, Civ))
                        //{
                        //    // do things
                        //}
                        else if (DiplomacyHelper.AreNeutral(spyingCiv, Civ))
                        {
                            if (spyingCiv.Traits.Contains(CivTraits.Hostile.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Subversive.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Warlike.ToString()))
                            {
                                if (RandomHelper.Random(3) == 0)
                                    DoSpySabotageMission(spyingCiv, Civ);
                                else IntelHelper.SabotageStealResearch(spyingCiv, Civ, "No one");
                            }
                        }
                    }
                }
            }
            if (Civ.TotalWarCivilization != null)
            {
                Civ.InvasionMinorCiv = null;
                if (GameContext.Current.CivilizationManagers[Civ.TotalWarCivilization].MaintenanceCostLastTurn == 0
                    || !GameContext.Current.CivilizationManagers[Civ.TotalWarCivilization].ControlsHomeSystem
                    || GameContext.Current.CivilizationManagers[Civ.TotalWarCivilization].IsHomeColonyDestroyed
                    || GameContext.Current.CivilizationManagers[Civ.TotalWarCivilization].TotalPopulation.IsMinimized)
                {
                    Civ.TotalWarCivilization = null;
                }
            }
            if (Civ.InvasionMinorCiv != null)
            {
                if (GameContext.Current.CivilizationManagers[Civ.InvasionMinorCiv].MaintenanceCostLastTurn == 0
                    || !GameContext.Current.CivilizationManagers[Civ.InvasionMinorCiv].ControlsHomeSystem
                    || GameContext.Current.CivilizationManagers[Civ.InvasionMinorCiv].IsHomeColonyDestroyed
                    || GameContext.Current.CivilizationManagers[Civ.InvasionMinorCiv].TotalPopulation.IsMinimized)
                {
                    Civ.InvasionMinorCiv = null;
                }
            }
            if (Civ.IsEmpire && Civ.TotalWarCivilization == null && Civ.InvasionMinorCiv == null
                && !Civ.IsHuman && GameContext.Current.TurnNumber > 5) //AI empire so look for total war conditions
            {
                var possibleTotalWarCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList();
                foreach (Civilization possibleTotalWarCiv in possibleTotalWarCivs)
                {
                    Double lastRange = 999;
                    
                    if (DiplomacyHelper.AreAtWar(possibleTotalWarCiv, Civ)
                        && !GameContext.Current.CivilizationManagers[possibleTotalWarCiv].IsHomeColonyDestroyed)
                    {
                        MapLocation empire = GameContext.Current.CivilizationManagers[possibleTotalWarCiv].HomeSystem.Location;
                        MapLocation ai = GameContext.Current.CivilizationManagers[Civ].HomeSystem.Location;
                        Double curretRange = Math.Sqrt(Math.Pow((empire.X - ai.X), 2) + Math.Pow((empire.Y - ai.Y), 2));
                        var maintenaceValue = GameContext.Current.CivilizationManagers[possibleTotalWarCiv].MaintenanceCostLastTurn;
                        if (maintenaceValue * 1.2 < GameContext.Current.CivilizationManagers[Civ].MaintenanceCostLastTurn
                            && possibleTotalWarCiv.TotalWarCivilization == null
                            && possibleTotalWarCiv.InvasionMinorCiv == null)
                        {
                            if (curretRange < lastRange)
                            {
                                if (UnitAI.CanAllShipsGetThere(Civ, possibleTotalWarCiv))
                                {
                                    Civ.TotalWarCivilization = possibleTotalWarCiv;
                                    lastRange = curretRange;
                                    GameLog.Client.AI.DebugFormat("{0} set as TOTALWAR!!! by {1} ", Civ.Name, possibleTotalWarCiv.Name);
                                }
                            }
                        }
                        else  Civ.TotalWarCivilization = null;
                    }
                }
            }
            
            if (Civ.IsEmpire && !Civ.IsHuman && Civ.Traits.Contains("Warlike")
                && Civ.TotalWarCivilization == null
                && Civ.InvasionMinorCiv == null
                && GameContext.Current.TurnNumber > 2) // AI empire is warlike so look for minors
            {
                var possibleInvadeMinorCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire == false).ToList();
                if (possibleInvadeMinorCivs != null && possibleInvadeMinorCivs.Count > 0)
                {
                    Double lastRange= 999;
                    foreach (Civilization possibleInvadeMinorCiv in possibleInvadeMinorCivs)
                    {  
                        if (DiplomacyHelper.IsContactMade(Civ, possibleInvadeMinorCiv)
                            && GameContext.Current.CivilizationManagers[possibleInvadeMinorCiv].HomeSystem.Owner != Civ
                            && !DiplomacyHelper.AreAllied(possibleInvadeMinorCiv, Civ))
                        {
                            MapLocation minor = GameContext.Current.CivilizationManagers[possibleInvadeMinorCiv].HomeSystem.Location;
                            MapLocation ai = GameContext.Current.CivilizationManagers[Civ].HomeSystem.Location;
                            Double curretRange = Math.Sqrt(Math.Pow((minor.X - ai.X), 2) + Math.Pow((minor.Y - ai.Y), 2));
                            var maintenaceValue = GameContext.Current.CivilizationManagers[possibleInvadeMinorCiv].MaintenanceCostLastTurn;
                            if (maintenaceValue * 1.2 < GameContext.Current.CivilizationManagers[Civ].MaintenanceCostLastTurn && possibleInvadeMinorCiv.InvasionMinorCiv == null)
                            {
                                if (curretRange < lastRange)
                                {
                                    if (UnitAI.CanAllShipsGetThere(Civ, possibleInvadeMinorCiv))
                                    {
                                        if (GameContext.Current.CivilizationManagers[possibleInvadeMinorCiv].HomeSystem.Owner != Civ)
                                        {
                                            Civ.InvasionMinorCiv = possibleInvadeMinorCiv;
                                            lastRange = curretRange;
                                            GameLog.Client.AI.DebugFormat("{0} set as new INVASION target by {1} ", possibleInvadeMinorCiv.Name, Civ.Name);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Civ.InvasionMinorCiv = null;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        public static void DoSpySabotageMission(Civilization spyingCiv, Civilization Civ)
        {
            int decide = RandomHelper.Random(5);
            switch (decide)
            {
                case 0:
                    {
                        IntelHelper.SabotageEnergy(spyingCiv, Civ, "No one");
                        break;
                    }
                case 1:
                    {
                        IntelHelper.SabotageFood(spyingCiv, Civ, "No one");
                        break;
                    }
                case 2:
                    {
                        IntelHelper.SabotageIndustry(spyingCiv, Civ, "No one");
                        break;
                    }
                case 3:
                    {
                        IntelHelper.SabotageStealCredits(spyingCiv, Civ, "No one");
                        break;
                    }
                case 4:
                    {
                        IntelHelper.SabotageStealResearch(spyingCiv, Civ, "No one");
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
