// File:PlayerAI.cs
// 
// Copyright (c) 2011-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using Obtics.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Supremacy.AI
{
    public static class PlayerAI
    {
        #region Constants

        private const int DangerRange = 4;
        private const int MaxDistanceInConvexHull = 3;

        [NonSerialized]
        private static string _text;
        //private static string blank = " ";

        #endregion

        #region Methods

        #region DoTurn from GameEngine
        public static void DoTurn(Civilization _civ)
        {
            _text = "\r\nStep_1131:; PlayerAI begins... for CivID " + _civ.CivID + " " + _civ.Key
                    ;
            Console.WriteLine(_text);

            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

            if (_civ.IsEmpire && _civ.CivID != 6 && _civ.SpiedCivList != null)  // Spy
            {
                _text = "Step_1132:; PlayerAI.Do_0_Turn_Unit ...SpiedCivList is NOT null ... for CivID "
                    + _civ.CivID + " " + _civ.Key
                    ;
                Console.WriteLine(_text);

                List<Civilization> spyingCivs = (List<Civilization>)GameContext.Current.Civilizations.Where(o => o.IsEmpire && o.CivID != 6).ToList();

                foreach (Civilization spyingCiv in spyingCivs)
                {
                    if (_civ.SpiedCivList.Contains(spyingCiv))
                    {
                        if (DiplomacyHelper.Status_AtWar(spyingCiv, _civ))
                        {
                            DoSpySabotageMission(spyingCiv, _civ);
                        }
                        //else if (DiplomacyHelper.AreAllied(spyingCiv, _civ) || DiplomacyHelper.AreFriendly(spyingCiv, _civ))
                        //{
                        //    // do things
                        //}
                        else if (DiplomacyHelper.Status_Neutral(spyingCiv, _civ))
                        {
                            if (spyingCiv.Traits.Contains(CivTraits.Hostile.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Subversive.ToString())
                                || spyingCiv.Traits.Contains(CivTraits.Warlike.ToString()))
                            {
                                if (RandomHelper.Random(3) == 0)
                                {
                                    DoSpySabotageMission(spyingCiv, _civ);
                                }
                                else
                                {
                                    IntelHelper.SabotageStealResearch(spyingCiv, _civ, "No one");
                                }
                            }
                        }
                    }
                }
            }

            //var possibleInvadeMinorCivs = GameContext.Current.Civilizations.Where(o => o.IsEmpire == false).ToList();

            if (_civ.IsEmpire && GameContext.Current.TurnNumber > 5)
            {
                _text = "Step_1139:; Assault_Location= " + _civM.Assault_Location.ToString()
                        ;
                Console.WriteLine(_text);

                if (_civ.IsHuman)
                {
                    //Debugger.Break();
                }

                //if (_civM.Assault_TargetCiv != null && GameContext.Current.Civilizations[_civM.Assault_TargetCiv.CivID] != null)
                if (_civM.Assault_Location != null && _civM.Assault_Accumulate_Location_1 != null 
                    && _civM.Assault_Location.ToString() != "(0, 0)" && _civM.Assault_Accumulate_Location_1.ToString() != "(0, 0)")
                {
                    //TargetCiv_CheckFirePower(_civM);  // check every turn for a better target
                    int civ_fire_Power_Accumulate_Location = Calculate_fire_power_ships_and_station(_civM.Assault_Accumulate_Location_1); // better: Colony.DefenseValue + Ships + Stations
                    int targetFirePower = Calculate_fire_power_ships_and_station(_civM.Assault_Location);

                    _text = "Step_1154:; "
                        /*+ "civ_fire_Power_Accumulate_Location= "*/ + _civM.Civilization.Key + " at " + _civM.Assault_Accumulate_Location_1
                        + " -civ_fire_Power_Accumulate_Location= * " + civ_fire_Power_Accumulate_Location
                        + " vs " + targetFirePower + " * targetcivFirePower at " + _civM.Assault_Location
                        ;
                    Console.WriteLine(_text);

                    if (_civ.IsHuman)
                    {
                        Debugger.Break();  // PlayerAI.DoTurn
                    }

                    if (civ_fire_Power_Accumulate_Location < targetFirePower)
                    {
                        _civM.Assault_Location = _civM.HomeSystem.Location;
                    }
                    else if (IsCivDefeated(_civM.Assault_TargetCiv))
                    {
                        _civM.Assault_Location = _civM.HomeSystem.Location;
                        // break down the fleet in UnitAI
                    }

                    if (civ_fire_Power_Accumulate_Location > targetFirePower)
                    {
                        // nothing ;  // assault + system_assault
                    }
                }

                //if it is AtWar, both sides can attack
                if (/*_civ.Traits.Contains("Warlike") || */DiplomacyHelper.IsAtWar(_civ))
                {
                    //if (_civM.Assault_TargetCiv == null)
                    //{
                    //    TargetCiv_CheckFirePower(_civ);
                    //}
                    //else 
                    //if (_civM.Assault_TargetCiv == null) //AI empire so look for invasion conditions
                    //{
                    if (_civM.Assault_Location == null || _civM.Assault_Location.ToString() == "(0, 0)")
                    {
                        TargetCiv_Find(_civM); // check every turn for a better target
                    }
                    //}
                }
            }
            _text = "Step_1133:; PlayerAI is done..."
                ;
            Console.WriteLine(_text);
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
        //    int gainedResearchPoints = assimiltedCivHome.Research_Net;
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
        public static void TargetCiv_CheckFirePower(CivilizationManager _civM)
        {
            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

            int civFirePower = Calculate_fire_power_ships_and_station(_civM.Assault_Location);
            int targetFirePower = Calculate_fire_power_ships_and_station(_civM.Assault_Location);
            if (civFirePower < targetFirePower)
            {
                _civM.Assault_Location = _civM.HomeSystem.Location;
            }
            else if (IsCivDefeated(_civM.Assault_TargetCiv))
            {
                _civM.Assault_Location = _civM.HomeSystem.Location;
                // break down the fleet in UnitAI
            }
        }
        public static void TargetCiv_Find(CivilizationManager _civM_1)
        {
            IList<Civilization> possibleCivs = GameContext.Current.Civilizations.ToList();
            //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1.CivID];
            Civilization _civ1 = _civM_1.Civilization;

            foreach (Civilization _invasionCiv in possibleCivs)
            {
                //if (_invasionCiv.IsHuman)
                //{
                //    Debugger.Break();
                //}

                // even peaceful empires need a target to grow 
                //if (!_civ1.Traits.Contains("Warlike")) // && _invasionCiv.CivID > 6)  // not warlike and a minor so skip
                //{
                //    continue;
                //}

                CivilizationManager _invasionCivM = GameContext.Current.CivilizationManagers[_invasionCiv.CivID];

                double lastRange = 999;

                if (DiplomacyHelper.IsContactMade(_civ1, _invasionCiv)
                    && !GameContext.Current.CivilizationManagers[_invasionCiv].IsHomeColonyDestroyed
                    && !DiplomacyHelper.AreAllied(_invasionCiv, _civ1)
                    && !DiplomacyHelper.IsMember(_invasionCiv, _civ1)
                    && _invasionCivM.Assault_TargetCiv == null
                    && _civ1 != _invasionCiv)
                {
                    MapLocation empire = GameContext.Current.CivilizationManagers[_invasionCiv].HomeSystem.Location;
                    MapLocation ai = GameContext.Current.CivilizationManagers[_civ1].HomeSystem.Location;
                    double curretRange = Math.Sqrt(Math.Pow(empire.X - ai.X, 2) + Math.Pow(empire.Y - ai.Y, 2));

                    int civFirePower = Calculate_fire_power_ships_and_station(_invasionCivM.Assault_Location);
                    int targetFirePower = Calculate_fire_power_ships_and_station(_invasionCivM.Assault_Location);
                    if (_invasionCivM.Assault_TargetCiv == null && targetFirePower * 1.1 < civFirePower)
                    {
                        if (curretRange < lastRange)
                        {
                            if (UnitAI.CanAllShipsGetThere(_civ1, _invasionCiv))
                            {

                                _civM_1.Assault_TargetCiv = _invasionCiv;
                                lastRange = curretRange;
                                if (!DiplomacyHelper.Status_AtWar(_civ1, _civM_1.Assault_TargetCiv))
                                {
                                    _text = "Step_3321:; Declare War " + _civ1.Name + " on " + _civM_1.Assault_TargetCiv.Name
                                        ;
                                    Console.WriteLine(_text);
                                    //GameLog.Core.AI.DebugFormat("Declare War {0} on {1}", _civ1.Name, _civM_1.Assault_TargetCiv.Name);
                                    Diplomat diplomat = Diplomat.Get(_civ1);
                                    ForeignPower foreignPower = diplomat.GetForeignPower(_civM_1.Assault_TargetCiv);
                                    foreignPower.DeclareWar();
                                }
                                _text = "Step_3322:; " + _civ1.Name + " set Invasion! on " + _civM_1.Assault_TargetCiv.Name
                                           ;
                                Console.WriteLine(_text);
                                //GameLog.Client.AI.DebugFormat("{0} set Invasion! on {1} ", _civ1.Name, _civM_1.Assault_TargetCiv.Name);
                            }
                        }
                    }
                    else
                    {
                        _civM_1.Assault_TargetCiv = null;
                    }
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
            {
                stillViable = true;
            }

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
                _text = "C5 Nuget de-installed";
                //convexHulls.Add(new ConvexHull(disjointSet));
            }

            return new ConvexHullSet(convexHulls);
        }

        public static int GetCreditTradeValuePercent(Civilization _civ)
        {
            // is this used ??
            Debugger.Break();

            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
            }

            int value = 1;
            if (IsInFinancialTrouble_BelowMinus2000(_civ))
            {
                value++;
            }

            return 100 * value;
        }

        public static int GetFleetDanger(Fleet fleet, int range, bool anyDanger) // bool testMoves,
        {
            return GetSectorDanger(fleet.Owner, fleet.Sector, range); //, testMoves);
        }

        public static int GetSectorDanger(Civilization _civ, Sector sector, int range) //, bool testMoves)
        {
            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
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

                    int distance = MapLocation.GetDistance(sector.Location, loopSector.Location);

                    if (DiplomacyHelper.Status_AtWar(_civ, loopSector.Owner) && distance <= 2)
                    {
                        borderDanger++;
                    }

                    foreach (Fleet fleet in GameContext.Current.Universe.FindAt<Fleet>(loopSector.Location))
                    {
                        foreach (Ship ship in fleet.Ships)
                        {
                            if (ship.Owner != null)
                            {
                                fleet.Owner = ship.Owner;
                            }
                        }
                        if (!DiplomacyHelper.Status_AtWar(_civ, fleet.Owner))
                        {
                            continue;
                        }

                        if (!fleet.IsCombatant)
                        {
                            continue;
                        }

                        FleetView fleetView = FleetView.Create(_civ, fleet);
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

            if (!IsHuman(_civ))
            {
                count += borderDanger;
            }
            // GameLog.Client.AI.DebugFormat("* Sector Danger ={0}",count);
            // count = 20;
            return count;
        }

        public static bool IsHuman(Civilization _civ)
        {
            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
            }

            return PlayerContext.Current.IsHumanPlayer(_civ);
        }

        public static bool IsInFinancialTrouble_BelowMinus2000(Civilization _civ)
        {
            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
            }

            //try
            //{
            CivilizationManager civManager = GameContext.Current.CivilizationManagers[_civ];

            if (civManager?.Credits.CurrentValue < -2000)
            {
                return true;
            }
            //}
            //catch (Exception e)
            //{
            //    GameLog.Core.General.Error(e);
            //}

            return false;
        }

        private static int Calculate_fire_power_ships_and_station(MapLocation _location)
        {
            int firePower = 0;
            _text = "Step_1141:; Calculate_fire_power_ships_and_station > for location= " + _location.ToString()
                    ;
            //Console.WriteLine(_text);
            
            if (_location == null || _location.ToString() == "(0, 0)")
            {
                return 0;
            }

            Sector _sector = new Sector(_location);

            //Supremacy.Orbitals.Fleet.
                IList<Fleet> _fleets_at_location = GameContext.Current.Universe.FindAt<Fleet>(_location).ToList();
            //.Where(_f => _f.Location == _location)
            //.ToList()
            //; 

            _text = "Step_1133:; _location= " + _location.ToString()
                + " - _fleets_at_location.Count= " + _fleets_at_location.Count
                ;
            Console.WriteLine(_text);

            foreach (Fleet civFleet in _fleets_at_location)
                //.Where(_f => _f.Location == _location)
                //.ToList())
            {
                foreach (Ship ship in civFleet.Ships.ToList())
                {
                    firePower += ship.Fire_Power_Orbital;

                    _text = "Step_1147:; " + UnitAI.CreateShipText(ship, out string shiptext) + " > FirePow= " + firePower;
                    Console.WriteLine(_text);
                    
                    // GameLog.Client.AI.DebugFormat("A ship all attack ships {0} location ={1}", ship.Name, ship.Location );
                }
            }

            if (_sector.Station != null)
            {
                firePower += _sector.Station.Fire_Power_Orbital;
            }

            return firePower;
        }
        #endregion
    }
}
