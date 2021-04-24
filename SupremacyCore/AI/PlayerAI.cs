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

            //var possibleInvadeMinorCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire == false).ToList();

            if (Civ.IsEmpire && GameContext.Current.TurnNumber > 5)
            {
                if (Civ.Traits.Contains("Warlike") || DiplomacyHelper.IsAtWar(Civ))
                {
                    if (Civ.TargetCivilization != null)
                    {
                        TurnOffTargetCiv(Civ);
                    }
                    else if (Civ.TargetCivilization == null) //AI empire so look for invasion conditions
                    {
                        FindTargetCiv(Civ);
                    }
                }
            }
        }
        #endregion

        //public static void AssimilateSystem(Colony colony)
        //{
        //    // Resistance is futile, assimilate da system
        //    int chanceToAssimilate = RandomHelper.Random(100);
        //    if (true) //(chanceToAssimilate <= 5)
        //    {
        //        Civilization borgy = GameContext.Current.CivilizationManagers[6].Civilization; 
        //        //var borg = GameContext.Current.Civilizations.Where(c => c.Key == "BORG").FirstOrDefault();
        //    Civilization assimilatedCiv = colony.Owner;
        //    CivilizationManager targetEmpire = GameContext.Current.CivilizationManagers[assimilatedCiv];
        //    Universe.Colony assimiltedCivHome = targetEmpire.HomeColony;
        //    int gainedResearchPoints = assimiltedCivHome.NetResearch;
        //    //Universe.Sector destination = CombatHelper.CalculateRetreatDestination(assets);
        //    //Ship ship = (Ship)assimilatedShip.Source;
        //    colony.Owner = borgy;
        //            //ship.Owner = borg;
        //            //Fleet newfleet = ship.CreateFleet();
        //            //newfleet.Location = destination.Location;
        //            //newfleet.Owner = borg;
        //            //newfleet.SetOrder(FleetOrders.EngageOrder.Create());
        //            //if (newfleet.Order == null)
        //            //{
        //            //    newfleet.SetOrder(FleetOrders.AvoidOrder.Create());
        //            //}
        //            //ship.IsAssimilated = true;
        //            //ship.Scrap = false;
        //            //newfleet.Name = "Assimilated Assets";
        //    GameContext.Current.CivilizationManagers[colony.Owner].Research.UpdateResearch(gainedResearchPoints);
        //    }
        //}
        public static void TurnOffTargetCiv(Civilization aCiv)
        {
            int civFirePower = CalculateFirePower(aCiv);
            int targetFirePower = CalculateFirePower(aCiv.TargetCivilization);
            if (civFirePower < targetFirePower)
            {
                aCiv.TargetCivilization = null;
            }
            else if (IsCivDefeated(aCiv.TargetCivilization))
            {
                aCiv.TargetCivilization = null;
                // break down the fleet in UnitAI
            }
        }
        public static void FindTargetCiv(Civilization daCiv)
        {
            var possibleInvasionCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList();
            foreach (Civilization possibleInvasionCiv in possibleInvasionCivs)
            {
                Double lastRange = 999;

                if (DiplomacyHelper.IsContactMade(daCiv, possibleInvasionCiv)
                    && !GameContext.Current.CivilizationManagers[possibleInvasionCiv].IsHomeColonyDestroyed
                    && !DiplomacyHelper.AreAllied(possibleInvasionCiv, daCiv)
                    && possibleInvasionCiv.TargetCivilization == null)
                {
                    MapLocation empire = GameContext.Current.CivilizationManagers[possibleInvasionCiv].HomeSystem.Location;
                    MapLocation ai = GameContext.Current.CivilizationManagers[daCiv].HomeSystem.Location;
                    Double curretRange = Math.Sqrt(Math.Pow((empire.X - ai.X), 2) + Math.Pow((empire.Y - ai.Y), 2));

                    //var maintenaceValue = GameContext.Current.CivilizationManagers[possibleInvasionCiv].MaintenanceCostLastTurn;
                    int civFirePower = CalculateFirePower(daCiv);
                    int targetFirePower = CalculateFirePower(possibleInvasionCiv);
                    if (possibleInvasionCiv.TargetCivilization == null && targetFirePower * 1.2 < civFirePower)
                    {
                        if (curretRange < lastRange)
                        {
                            if (UnitAI.CanAllShipsGetThere(daCiv, possibleInvasionCiv))
                            {
                                daCiv.TargetCivilization = possibleInvasionCiv;
                                lastRange = curretRange;
                                if (!DiplomacyHelper.AreAtWar(daCiv, daCiv.TargetCivilization))
                                {
                                    GameLog.Core.AI.DebugFormat("Declare War {0} on {1}", daCiv.Name, daCiv.TargetCivilization.Name);
                                    var diplomat = Diplomat.Get(daCiv);
                                    ForeignPower foreignPower = diplomat.GetForeignPower(daCiv.TargetCivilization);
                                    foreignPower.DeclareWar();
                                }
                                GameLog.Client.AI.DebugFormat("{0} set Invasion! on {1} ", daCiv.Name, daCiv.TargetCivilization.Name);
                            }
                        }
                    }
                    else daCiv.TargetCivilization = null;
                }
            }
        }
        public static bool IsCivDefeated(Civilization undefeatedCiv)
        {
            bool stillViable = false;
            if (GameContext.Current.CivilizationManagers[undefeatedCiv].MaintenanceCostLastTurn == 0
                    || !GameContext.Current.CivilizationManagers[undefeatedCiv].ControlsHomeSystem
                    || GameContext.Current.CivilizationManagers[undefeatedCiv].IsHomeColonyDestroyed
                    || GameContext.Current.CivilizationManagers[undefeatedCiv].TotalPopulation.IsMinimized)
                stillViable = true;
            return stillViable;
        }

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

        public static int GetFleetDanger(Fleet fleet, int range, bool anyDanger) // bool testMoves,
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
                        foreach (var ship in fleet.Ships)
                        {
                            if (ship.Owner != null)
                                fleet.Owner = ship.Owner;
                        }
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
                        //if ((sector.Location.X >= 0 || sector.Location.X <= map.Width) && (sector.Location.Y >= 0 || sector.Location.Y <= map.Height))
                        //{

                        if (fleet.Owner != null && DiplomacyHelper.IsTravelAllowed(fleet.Owner, sector) && (fleet.Speed >= distance)) // || !testMoves ||
                        {
                            ++count;
                        }
                        //}
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
        private static int CalculateFirePower(Civilization civ)
        {
            int firePower = 0;
            foreach (Fleet civFleet in GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList())
            {
                foreach (Ship ship in civFleet.Ships.Where(s => s.ShipType >= ShipType.Scout || s.ShipType == ShipType.Transport).ToList())
                {
                    firePower += ship.Firepower();
                    // GameLog.Client.AI.DebugFormat("A ship all attack ships {0} location ={1}", ship.Name, ship.Location );
                }
            }
            return firePower;
        }
        #endregion
    }
}
