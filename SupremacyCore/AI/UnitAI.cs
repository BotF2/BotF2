// UnitAI.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.Pathfinding;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.AI
{

    public static class UnitAI
    {
        private static List<bool> _inRange = new List<bool> { true }; // fleets of civ out of range of target homeworld
        private static Dictionary<int, List<bool>> _rangesByCiv = new Dictionary<int, List<bool>> { { 0, _inRange }, { 1, _inRange }, { 2, _inRange }, { 3, _inRange }, { 4, _inRange }, { 5, _inRange }, { 6, _inRange } };
        private static Dictionary<int, bool> TotalWarNextTurn = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false }, { 4, false }, { 5, false }, { 6, false } };
        private static bool _totalWar = false;
        private static int _currentTurn = 0;

        public static Dictionary<int, List<bool>> RangesByCiv // check here for all ships in range for distance of total-war-target-civ home-world from your home-world
        {
            get { return _rangesByCiv; }
            set { _rangesByCiv = value; }
        }
        //public static Dictionary<int, int> OnlyOnceATurn { get { return _onlyOnceATurn; } set { _onlyOnceATurn = value; } }
        public static void DoTurn([NotNull] Civilization civ)
        {
            if (_currentTurn != GameContext.Current.TurnNumber)
            {
                _currentTurn = GameContext.Current.TurnNumber;
                List<bool> _inRange = new List<bool> { true };
                RangesByCiv = new Dictionary<int, List<bool>> { { 0, _inRange }, { 1, _inRange }, { 2, _inRange }, { 3, _inRange }, { 4, _inRange }, { 5, _inRange }, { 6, _inRange } };
            }
            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            foreach (Fleet fleet in GameContext.Current.Universe.FindOwned<Fleet>(civ))
            {
                StarSystem homeSystem = GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem;
                //Make sure all fleets are cloaked
                foreach (Ship ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    //GameLog.Core.AI.DebugFormat("Cloaking {0} {1}", ship.Name, ship.ObjectID);
                    ship.IsCloaked = true;
                }

                //If the fleet can't move, we're limited in our options
                if (!fleet.CanMove)
                {
                    continue;
                }
                if (civ.TotalWarCivilization != null && RangesByCiv.ContainsKey(fleet.OwnerID))
                    _totalWar = !RangesByCiv[fleet.OwnerID].Contains(false); // set local total war (send attacking fleet) condition from last turn for the civ of this current fleet.

                // GameLog.Core.AI.DebugFormat("Turn {2}: Processing Fleet {0} in {1}...", fleet.ObjectID, fleet.Location, GameContext.Current.TurnNumber);

                if (_totalWar && fleet.UnitAIType != UnitAIType.Attack)
                {
                    StarSystem othersHomeSystem = GameContext.Current.CivilizationManagers[civ.TotalWarCivilization].HomeSystem;

                    IEnumerable<Fleet> attackWarShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsBattleFleet
                        || s.IsFastAttack || s.IsScout).ToList();
                    if (attackWarShips != null)
                    {
                        Fleet attackFleet = new Fleet();
                        foreach (Fleet testRangeFleet in attackWarShips)
                        {
                            if (!FleetHelper.IsSectorWithinFuelRange(othersHomeSystem.Sector, testRangeFleet))
                            {
                                List<bool> listOfOne = new List<bool> { false }; // found a ship that cannot get there
                                _rangesByCiv[fleet.OwnerID].AddRange(listOfOne);
                            }
                        }

                        if (!_rangesByCiv[fleet.ObjectID].Contains(false))
                        {
                            if (fleet.Ships.Count() == 1)
                                    attackFleet = fleet;
                            else if (fleet.Ships.Count() != 0)
                            {
                                for (int i = 0; i < fleet.Ships.Count; i++)
                                {
                                    // put next fleet into attack fleet
                                    Ship ship = fleet.Ships[0];
                                    // GameLog.Core.AI.DebugFormat("## Attackfleet - Ship added ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                                    fleet.RemoveShip(ship);
                                    attackFleet.AddShip(ship);
                                }
                            }
                        }
                        IEnumerable<Fleet> homeWarShips = attackWarShips.Where(s => s.Location == homeSystem.Location).ToList();

                        if (homeWarShips.Count() + 1 >= attackWarShips.Count())
                        {
                            attackFleet.SetOrder(new EngageOrder());
                            attackFleet.UnitAIType = UnitAIType.Attack;
                            attackFleet.Activity = UnitActivity.Mission;
                            attackFleet.SetRoute(AStar.FindPath(attackFleet, PathOptions.SafeTerritory, null, new List<Sector> { othersHomeSystem.Sector }));
                        }
                    }
                }

                //Set scouts to permanently explore

                if ((fleet.IsScout) && fleet.Activity == UnitActivity.NoActivity)
                {
                    if (!_totalWar)
                    {
                        fleet.SetOrder(new ExploreOrder());
                        fleet.UnitAIType = UnitAIType.Explorer;
                        fleet.Activity = UnitActivity.Mission;
                        // GameLog.Core.AI.DebugFormat("Ordering Scout & FastAttack {0} to explore from {1}", fleet.ClassName, fleet.Location);
                    }

                    else if (_totalWar)
                    {
                        // StarSystem homeSystem = GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem;
                        fleet.SetOrder(new AvoidOrder());
                        fleet.UnitAIType = UnitAIType.Attack;
                        fleet.Activity = UnitActivity.NoActivity;
                        if (fleet.Location != homeSystem.Location)
                        {
                            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { homeSystem.Sector }));
                        }
                    }
                }

                if (fleet.IsColonizer || fleet.UnitAIType == UnitAIType.Colonizer)
                {
                    if ((fleet.Activity == UnitActivity.NoActivity || fleet.Activity == UnitActivity.Mission || fleet.Route.IsEmpty || fleet.Order.IsComplete))
                    {
                        // only left over escort without colonyship
                        if (fleet.Ships.Count() == 1 && fleet.Ships[0].ShipType >= ShipType.FastAttack && fleet.UnitAIType == UnitAIType.Colonizer)
                        {
                            fleet.UnitAIType = UnitAIType.Reserve;
                            fleet.Activity = UnitActivity.NoActivity;
                            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { homeSystem.Sector }));
                        }
                        //Can we find a system to colonize?
                        else if (GetBestSystemToColonize(fleet, out StarSystem bestSystemToColonize))
                        {
                            //Are we there yet?
                            if (fleet.Sector == bestSystemToColonize.Sector && systemNotAlreadyTaken(fleet))
                            {             
                                // We are there
                                fleet.Route.Clear();
                                fleet.SetOrder(new ColonizeOrder());
                                fleet.UnitAIType = UnitAIType.Colonizer;
                                fleet.Activity = UnitActivity.Hold;
                                if (fleet.Ships.Count() > 1)
                                    RemoveEscortShips(fleet, ShipType.Colony);
                                // GameLog.Core.AI.DebugFormat("Ordering colonizer fleet {0} at {1} to colonize", fleet.Sector.Name, fleet.Location);
                            }
                            else if (bestSystemToColonize != null)
                            {
                                if (fleet.Route == null || fleet.Route.IsEmpty)
                                {
                                    //Head to the system                                
                                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemToColonize.Sector }));
                                    fleet.UnitAIType = UnitAIType.Colonizer;
                                    fleet.Activity = UnitActivity.Mission;
                                    if (!fleet.Ships.Where(s => s.ShipType >= ShipType.FastAttack).Any())
                                        GetFleetEscort(fleet, bestSystemToColonize.Sector);
                                }
                                // GameLog.Core.AI.DebugFormat("Ordering {0} colonizer {1} to go to {2} {3}", fleet.Owner, fleet.Name, bestSystemToColonize.Name, bestSystemToColonize.Location);
                            }
                        }
                        //else
                        //{
                        //    //GameLog.Core.AI.DebugFormat("Nothing to do for colonizer fleet {0}", fleet.ObjectID);
                        //}
                    }
                }

                if (fleet.IsConstructor || fleet.UnitAIType == UnitAIType.Constructor)
                {
                    if (fleet.UnitAIType == UnitAIType.Constructor
                        && !fleet.Ships.Any(x => x.ShipType == ShipType.Construction)
                        && !fleet.Sector.GetOwnedFleets(fleet.Owner).Where(o => o.IsConstructor).Any()) // constructor fleet with no constructor left                  
                    {
                        fleet.UnitAIType = UnitAIType.Reserve;
                        fleet.Activity = UnitActivity.NoActivity;
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { homeSystem.Sector }));
                    }
                    else if (fleet.IsStranded && fleet.Activity != UnitActivity.BuildStation && systemNotAlreadyTaken(fleet)) // stranded construction ship builds station
                    {
                        BuidStation(fleet);
                    }
                    else if (fleet.Activity == UnitActivity.NoActivity || (fleet.Route.IsEmpty && fleet.Activity != UnitActivity.BuildStation)
                        || fleet.Order.IsComplete) 
                    {
                        if (fleet.Route.IsEmpty && fleet.Activity == UnitActivity.Mission && systemNotAlreadyTaken(fleet))
                        {
                            BuidStation(fleet);
                        }
                        else if (GetBestSectorForStation(fleet, out Sector bestSectorForStation)
                            && fleet.Activity != UnitActivity.BuildStation)
                            //&& fleet.Activity != UnitActivity.Mission)
                        {  
                            if (fleet.Sector == bestSectorForStation && systemNotAlreadyTaken(fleet))
                            {
                                BuidStation(fleet);
                                //GameLog.Core.AI.DebugFormat("Ordering constructor fleet {0} to build station at {1}, {2}", fleet.ObjectID, fleet.Location, fleet.Sector.Name);
                            }
                            else if (fleet.Route == null || fleet.Route.IsEmpty)
                            {                                   
                                //go to the sector location
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSectorForStation }));
                                fleet.UnitAIType = UnitAIType.Constructor;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering constructor fleet {0} to {1}, {2}", fleet.ObjectID, bestSectorForStation.Location, bestSectorForStation.Name);
                                if (!fleet.Ships.Where(s => s.ShipType >= ShipType.FastAttack).Any())
                                    GetFleetEscort(fleet, bestSectorForStation);
                            }
                        }
                        //else
                        //{
                        //    GameLog.Core.AI.DebugFormat("Nothing to do for constructor fleet {0}", fleet.ObjectID);
                        //}
                    }                  
                }

                if (fleet.IsBattleFleet || (fleet.IsFastAttack)) // && fleet.Activity == UnitActivity.NoActivity))
                {
                    if (!_totalWar)
                    {
                        // GameLog.Core.AI.DebugFormat("## NOT Total War, fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                        Fleet defenseFleet = new Fleet();
                        if (GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense) == null)
                        {
                            defenseFleet = fleet;
                            if (fleet.Activity == UnitActivity.NoActivity)
                            {
                                fleet.UnitAIType = UnitAIType.SystemDefense;
                                fleet.Activity = UnitActivity.Hold;
                                //GameLog.Core.AI.DebugFormat("## first SystemDefence fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                            }
                        }
                        else
                        {
                            defenseFleet = GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);

                            if (fleet.Activity == UnitActivity.NoActivity && fleet.Sector == defenseFleet.Sector)
                            {
                                for (int i = 0; i < fleet.Ships.Count; i++)
                                {
                                    Ship ship = fleet.Ships[0];
                                    // GameLog.Core.AI.DebugFormat("## Ship added to systemDefence fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                                    fleet.RemoveShip(ship);
                                    defenseFleet.AddShip(ship);
                                }
                            }
                        }
                        if (fleet.UnitAIType == UnitAIType.Escort
                            && fleet.Sector != GameContext.Current.Universe.HomeColonyLookup[civ].Sector
                            && !fleet.Sector.GetOwnedFleets(fleet.Owner).Where(o => o.IsConstructor || o.IsColonizer).Any()
                             ) // find escort to send to next job
                        {
                            fleet.UnitAIType = UnitAIType.Reserve;
                            fleet.Activity = UnitActivity.NoActivity;
                        }
                    }
                    //else if (_totalWar)
                    //{

                    //    //GameLog.Core.AI.DebugFormat("## IS TOTAL WAR,  fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                    //    Fleet attackFleet = new Fleet();
                    //    if (GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.Attack) == null)
                    //    {
                    //        attackFleet = fleet;
                    //        if (fleet.Activity == UnitActivity.NoActivity || fleet.Activity == UnitActivity.Hold)
                    //        {
                    //            fleet.UnitAIType = UnitAIType.SystemDefense;
                    //            fleet.Activity = UnitActivity.Hold;
                    //            // GameLog.Core.AI.DebugFormat("## first Attack fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        attackFleet = GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.Attack);

                    //        if (fleet.Activity == UnitActivity.NoActivity && fleet.Sector == attackFleet.Sector)
                    //        {
                    //            for (int i = 0; i < fleet.Ships.Count; i++)
                    //            {
                    //                Ship ship = fleet.Ships[0];
                    //                // GameLog.Core.AI.DebugFormat("## Ship added to attack fleet ={0}, {1}, {2}, {3}, {4}, {5}", fleet.Name, fleet.Owner, fleet.Order, fleet.UnitAIType, fleet.Activity, fleet.Location);
                    //                fleet.RemoveShip(ship);
                    //                attackFleet.AddShip(ship);
                    //            }
                    //        }
                    //    }
                    //}
                }

                if (fleet.IsMedical)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        if (GetBestColonyForMedical(fleet, out Colony bestSystemForMedical))
                        {
                            if (bestSystemForMedical != null && bestSystemForMedical.Location == fleet.Location)
                            {
                                //Colony medical treatment
                                fleet.SetOrder(new MedicalOrder());
                                fleet.UnitAIType = UnitAIType.Medical;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AI.DebugFormat("Ordering medical fleet {0} in {1} to treat the population", fleet.ObjectID, fleet.Location);
                            }
                            else if (bestSystemForMedical != null)
                            {
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForMedical.Sector }));
                                fleet.UnitAIType = UnitAIType.Medical;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AI.DebugFormat("Ordering medical fleet {0} to {1}", fleet.ObjectID, bestSystemForMedical);
                            }
                        }
                        else
                        {
                            //GameLog.Core.AI.DebugFormat("Nothing to do for medical fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                if (fleet.IsDiplomatic)
                {
                    // Send diplomatic ship and influence order, but what do we do with race traits and diplomacy?
                    if (fleet.Owner.Traits.Contains(CivTraits.Peaceful.ToString()) || fleet.Owner.Traits.Contains(CivTraits.Kindness.ToString()))
                    {

                    }
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        Colony bestSystemForDiplomacy;
                        if (GetBestColonyForDiplomacy(fleet, out bestSystemForDiplomacy))
                        {
                            if (bestSystemForDiplomacy.OwnerID < 6)
                            {
                                if (bestSystemForDiplomacy.Location == fleet.Location)
                                {
                                    fleet.SetOrder(new InfluenceOrder());
                                    fleet.UnitAIType = UnitAIType.Diplomatic;
                                    fleet.Activity = UnitActivity.Mission;
                                    // GameLog.Core.AI.DebugFormat("Ordering diplomacy fleet {0} in {1} to influence", fleet.ObjectID, fleet.Location);
                                }
                                else
                                {
                                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForDiplomacy.Sector }));
                                    fleet.UnitAIType = UnitAIType.Diplomatic;
                                    fleet.Activity = UnitActivity.Mission;
                                    //  GameLog.Core.AI.DebugFormat("Ordering diplomacy fleet {0} to {1}", fleet.ObjectID, bestSystemForDiplomacy);
                                }
                            }
                        }
                        else
                        {
                            //  GameLog.Core.AI.DebugFormat("Nothing to do for diplomacy fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                if (fleet.IsSpy) // install spy network
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        Colony bestSystemForSpying;
                        if (GetBestColonyForSpying(fleet, out bestSystemForSpying))
                        {
                            if (bestSystemForSpying != null && bestSystemForSpying.OwnerID < 6)
                            {
                                bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForSpying.Owner, fleet.Owner);
                                if (!hasOurSpyNetwork)
                                {
                                    if (bestSystemForSpying.Location == fleet.Location)
                                    {
                                        fleet.SetOrder(new SpyOnOrder()); // install spy network
                                        //fleet.UnitAIType = UnitAIType.NoUnitAI;
                                        fleet.Activity = UnitActivity.NoActivity;
                                        // GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} in {1} to install spy network", fleet.ObjectID, fleet.Location);
                                    }
                                    else
                                    {
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForSpying.Sector }));
                                        fleet.UnitAIType = UnitAIType.Spy;
                                        fleet.Activity = UnitActivity.Mission;
                                        // GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} to {1}", fleet.ObjectID, bestSystemForSpying);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // GameLog.Core.AI.DebugFormat("Nothing to do for spy fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                if (fleet.IsScience)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        StarSystem bestSystemForScience;
                        if (GetBestSystemForScience(fleet, out bestSystemForScience))
                        {
                            if (bestSystemForScience != null)
                            {
                                bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForScience.Owner, fleet.Owner);
                                if (!hasOurSpyNetwork)
                                {
                                    if (bestSystemForScience.Location == fleet.Location)
                                    {
                                        fleet.SetOrder(new AvoidOrder());
                                        fleet.UnitAIType = UnitAIType.Science;
                                        fleet.Activity = UnitActivity.Mission;
                                        // GameLog.Core.AI.DebugFormat("Science fleet {0} at Research location {1}", fleet.ObjectID, fleet.Location);
                                    }
                                    else
                                    {
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForScience.Sector }));
                                        fleet.UnitAIType = UnitAIType.Science;
                                        fleet.Activity = UnitActivity.Mission;
                                        // GameLog.Core.AI.DebugFormat("Ordering science fleet {0} to {1}", fleet.ObjectID, bestSystemForScience);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //GameLog.Core.AI.DebugFormat("Nothing to do for science fleet {0}", fleet.ObjectID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check IsTotalWar Dictionary for  
        ///  a fleet that cannot get to target home world
        /// </summary>
        /// <returns></returns>
        public static void SetForTotalWarNextTurn()
        {
            List<bool> listBools;
            for (int i = 0; i < 7; i++)
            {
                if (_rangesByCiv.TryGetValue(i, out listBools))
                {
                    if (!listBools.Contains(false))
                    {
                        TotalWarNextTurn.Remove(i);
                        TotalWarNextTurn.Add(i, true);
                    }
                    else if (listBools.Contains(false))
                    {
                        TotalWarNextTurn.Remove(i);
                        TotalWarNextTurn.Add(i, false);
                    }
                }
            }
        }
        /*
         * Colonization
         */

        /// <summary>
        /// Get the best <see cref="StarSystem"/> for the given <see cref="Fleet"/>
        /// to colonize
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSystemToColonize(Fleet fleet, out StarSystem result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            List<Fleet> colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsColonizer || o.multiFleetHasAColonizer).ToList();
            var otherFleets = colonizerFleets.Where(o => o != fleet).ToList(); // other colony ships

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            //Get a list of all systems that we can colonise
            List<StarSystem> systems = GameContext.Current.Universe.Find<StarSystem>()
            //We need to know about it (no cheating)
            .Where(r => mapData.IsScanned(r.Location) && mapData.IsExplored(r.Location))
            .Where(s => !s.IsOwned || s.Owner == fleet.Owner)
            .Where(t => !t.IsInhabited && !t.HasColony)
            .Where(u => u.IsHabitable(fleet.Owner.Race))
            .Where(v => v.StarType != StarType.RadioPulsar && v.StarType != StarType.NeutronStar)
            .Where(w => FleetHelper.IsSectorWithinFuelRange(w.Sector, fleet))
            .Where(x => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is ColonizeOrder)
            //.Where(y => GameContext.Current.Universe.FindAt<Orbital>(y.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, y.Owner))
            )
            //That isn't owned by another civilization
            //That isn't currently inhabited or have a current colony
            //That's actually inhabitable
            //That are in fuel range
            //That doesn't have potential enemies in it
            //Where a ship isn't heading there already
            //Where a ship isn't there and colonizing
            .ToList();

            if (systems.Count == 0)
            {
                result = null;
                return false;
            }
            List<StarSystem> enemySystems = new List<StarSystem>() { systems.FirstOrDefault() };
            StarSystem placeholder = enemySystems.FirstOrDefault();
            foreach (StarSystem system in systems)
            {
                if (system.Owner != null && GameContext.Current.Universe.FindAt<Orbital>(system.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, system.Owner)))
                {
                    enemySystems.Add(system);
                }
            }
            foreach (StarSystem removeSystem in enemySystems)
            {
                systems.Remove(removeSystem);
            }
            //foreach (var system in systems)
            //{
            //    GameLog.Client.AI.DebugFormat("System ={0}, {1} Colony? ={2} owner ={3}, Habitable? ={4} starType {5} for {6}"
            //        , system.Name, system.Location, system.HasColony, system.Owner, system.IsHabitable(fleet.Owner.Race), system.StarType, fleet.Owner);
            //}
            if (systems.Count == 0)
            {
                result = null;
                return false;
            }

            var sortResults = from system in systems
                              orderby GetColonizeValue(system, fleet.Owner)
                              select system;

            result = sortResults.Last();
            //GameLog.Client.AI.DebugFormat("Best System for {0}, star ={1}, {2} {3}, value ={4}", fleet.Owner, result.Name, result.StarType, result.Location, GetColonizeValue(result, fleet.Owner));
            return true;
        }

        /// <summary>
        /// Determines how valuable colonizing a particular <see cref="StarSystem"/>
        /// will be for a <see cref="Civilization"/>
        /// </summary>
        /// <param name="system"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static float GetColonizeValue(StarSystem system, Civilization civ)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            //Alter this to alter priority
            const int DilithiumBonusValue = 20;
            const int DuraniumBonusValue = 20;

            float value = 0;

            if (system.HasDilithiumBonus)
            {
                value += DilithiumBonusValue;
            }

            if (system.HasRawMaterialsBonus)
            {
                value += DuraniumBonusValue;
            }

            value += system.GetMaxPopulation(civ.Race) * system.GetGrowthRate(civ.Race);
            //GameLog.Core.AI.DebugFormat("Colonize value for {0} is {1} for {2}", system, value, civ.Name);
            return value;
        }

        /// <summary>
        /// Get combat ship <see cref="Fleet"/> to protect colonyship
        /// </summary>
        /// <param name="colonyFleet"></param>
        /// <param name="colonySector"></param>
        /// <returns></returns>
        public static void GetFleetEscort(Fleet fleetToFollow, Sector finalSector) 
        {
            if ( finalSector == null)
            {
                return;
            }
            List<Fleet> escortFleets = GameContext.Current.Universe.HomeColonyLookup[fleetToFollow.Owner].Sector.GetOwnedFleets(fleetToFollow.Owner)
                .Where(b => b.Sector == GameContext.Current.Universe.HomeColonyLookup[fleetToFollow.Owner].Sector)
                .Where(a => a.Ships.Count() > 1).ToList();
            List<Fleet> loanEscorts = GameContext.Current.Universe.FindOwned<Fleet>(fleetToFollow.Owner)
                .Where(n => n.Sector != GameContext.Current.Universe.HomeColonyLookup[fleetToFollow.Owner].Sector)
                .Where(o => o.UnitAIType == UnitAIType.Reserve).ToList();
            if (loanEscorts.Count > 0)
            {
                escortFleets.AddRange(loanEscorts);
            }
            
            foreach (var fleet in escortFleets)
            {
                if (fleet.Ships.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser || o.ShipType == ShipType.HeavyCruiser
                    && fleet.Owner == fleetToFollow.Owner
                    && fleet.CanMove && fleet.ClassName != "UNKNOWN"
                    && fleet.Ships[0].ObjectID > 1).Any())
                    //&& !fleet.Ships.Where(o => o.ShipType < ShipType.FastAttack).Any())
                {
                    fleet.Ships.Sort((x, y) => y.ShipType.CompareTo(x.ShipType));
                    
                    int extraShips = fleet.Ships.Count() -1;
                    if (extraShips > 0)   
                    for (int i = 0; i < extraShips; i++)
                    {
                        Ship ship = fleet.Ships.Last();
                        fleet.RemoveShip(ship);
                    }
                    Ship lastShip = fleet.Ships[0];
                    if (lastShip.ObjectID != -1)
                        fleetToFollow.AddShip(lastShip);
                //fleet.SetRouteInternal(AStar.FindPath(fleetToFollow, PathOptions.SafeTerritory, null, new List<Sector> { finalSector }));                   

                GameLog.Core.AI.DebugFormat("ESCORT ={0} {1} unitAIType {2} activity {3} sector ={4} for ship ={5} {6} step count ={7}"
                    ,fleet.Owner, fleet.ClassName, fleet.UnitAIType, fleet.Activity, finalSector.Name, fleetToFollow.Owner, fleetToFollow.Name, fleetToFollow.Route.Steps.Count);
                return;                   
                }
            }
        }

        private static void RemoveEscortShips(Fleet fleet, ShipType type)
        {
            int shipCount = fleet.Ships.Count();
            for (int i = 0; i < shipCount; i++)
            {
                Ship ship = fleet.Ships[i];
                if (ship.ShipType != type)
                {
                    Fleet newFleet = new Fleet();
                    fleet.RemoveShip(ship);
                    newFleet.AddShip(ship);
                    //newFleet.Route.Clear();
                    newFleet.UnitAIType = UnitAIType.Reserve;
                    newFleet.Activity = UnitActivity.NoActivity;
                    
                }
            }            
        }

        /*
         * Exploration value
         */

        /// <summary>
        /// Determines how valuable it will be for the given <see cref="Fleet"/>
        /// to explore the given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetExploreValue(Sector sector, Civilization civ)
        {
            if (sector == null)
            {
                throw new ArgumentNullException(nameof(sector));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            //These values are the priority of each item
            const int UnscannedSectorValue = 500;
            const int UnexploredSectorValue = 200;
            const int HasStarSystemValue = 300;
            const int InitiatesFirstContactValue = 200;

            int value = 0;

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];
            CivilizationMapData mapData = civManager.MapData;

            //Unscanned
            if (!mapData.IsScanned(sector.Location))
            {
                value += UnscannedSectorValue;
            }

            //Unexplored
            if (!mapData.IsExplored(sector.Location))
            {
                value += UnexploredSectorValue;
                //Unexplored star system
                if (sector.System != null)
                {
                    value += HasStarSystemValue;
                }
            }

            //First contact
            if (sector.System?.HasColony == true && (sector.System.Colony.Owner != civ) && !DiplomacyHelper.IsContactMade(sector.Owner, civ))
            {
                value += InitiatesFirstContactValue;
            }

            //GameLog.Core.AI.DebugFormat("Explore priority for {0} is {1}", sector, value);
            return value;
        }

        /*
       * Explor best sector
       */

        /// <summary>
        /// Gets the best <see cref="Sector"/> to explore for the given <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static bool GetBestSectorToExplore(Fleet fleet, out Sector sector)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            List<Fleet> ownFleets = new List<Fleet>();
            try
            {
                ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(f => f.CanMove && f != fleet && !f.Route.IsEmpty).ToList();
            }

            catch (Exception e)
            {
                //GameLog.Client.General.Error(e);
                //  GameLog.Client.General.ErrorFormat("fleet.ObjectId ={0} {1} {2} error ={3}", fleet.ObjectID, fleet.Name, fleet.ClassName, e);
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            List<StarSystem> starsToExplore = new List<StarSystem>();
            if (fleet.Owner != null)
            {
                starsToExplore = GameContext.Current.Universe.Find<StarSystem>()
                    //We need to know about it (no cheating)
                    .Where(s => mapData.IsScanned(s.Location) && (!s.IsOwned || (s.Owner != fleet.Owner))
                    && DiplomacyHelper.IsTravelAllowed(fleet.Owner, s.Sector)
                    && FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet)
                    && !ownFleets.Any(f => f.Route.Waypoints.Any(wp => s.Location == wp)))
                    //No point exploring our own space
                    //Where we can enter the sector
                    //Where is in fuel range of the ship
                    //Where no fleets are already heading there or through there
                    .ToList();
            }

            if (starsToExplore.Count == 0)
            {
                sector = null;
                return false;
            }

            starsToExplore.Sort((a, b) =>
                (GetExploreValue(a.Sector, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetExploreValue(b.Sector, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            sector = starsToExplore[starsToExplore.Count() - 1].Sector;
            return true;
        }

        /*
         * NoOneElseTakingSystem
         */
        public static bool systemNotAlreadyTaken(Fleet fleet)
        {
            bool noOtherColony = !GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Colony && o.Owner != fleet.Owner).Any();
            bool noStation = !GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Station).Any();
            if (noOtherColony && noStation)
                return true;
            return false;
        }

        /*
        * Station value
        */

        /// <summary>
        /// Determines how valuable a <see cref="Station"/> would be
        /// in a given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetStationValue(Sector sector, Fleet fleet, List<UniverseObject> universeObjects)
        {

            if (sector == null)
            {
                throw new ArgumentNullException(nameof(sector));
            }
            //if (fleet.Owner.Key == "BORG")
            //{
            //    var borgHomeSector = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Sector;
            //    if (sector == borgHomeSector && sector.Station == null)
            //    {
            //        if(useOnlyOnce == 0)
            //        {
            //            useOnlyOnce = 1;
            //            return 5000;
            //        }
            //    }
            //}
            if (sector.Station != null
                || sector.GetFleets().Where(o => o.UnitAIType == UnitAIType.Constructor) != null && sector.GetFleets().Where(o => o.UnitAIType == UnitAIType.Constructor).Any()
                || sector.System != null && (sector.System.StarType == StarType.BlackHole || sector.System.StarType == StarType.NeutronStar)
                || (sector.Owner != null && sector.Owner.CivID <= 6)
                || sector.GetNeighbors().Where(o => o.Owner != null && o.Owner.CivID <= 6).Any()) //(sector.GetNeighbors().Where(o => o.Owner != null).Any() &&
            {
                return -4000;
            }
            //IEnumerable<Fleet> constructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
            //    .Where(f => f.IsConstructor);

            //var otherFleets = constructorFleets.Where(o => o != fleet).ToList();

            ////Get a list of all sectors to build
            //List<UniverseObject> okObject = GameContext.Current.Universe.Find<UniverseObject>()
            //    .Where(x => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is BuildStationOrder))
            //    .ToList();

            int value = 1;
            var something = universeObjects.Where(o => o.Sector == sector);
            if (something != null)
            {
                const int SystemSectorValue = 1500;
                const int StrandedShipSectorValue = 2500;
                const int PastFuelRange = 1000;
                const int GreatestDistance = 500;

                if (!FleetHelper.IsSectorWithinFuelRange(sector, fleet))
                {
                    value += PastFuelRange;
                }

                if ((sector.System != null)
                    && (sector.System.Owner == null || sector.System.OwnerID > 6))
                {
                    value += SystemSectorValue;
                    if (sector.System.StarType == StarType.Blue
                        || sector.System.StarType == StarType.Orange
                        || sector.System.StarType == StarType.Red
                        || sector.System.StarType == StarType.White
                        || sector.System.StarType == StarType.Yellow
                        || sector.System.StarType == StarType.Wormhole)
                    {
                        value += SystemSectorValue;
                    }
                }
                Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Sector;

                try
                { var furthestObject = GameContext.Current.Universe.FindFurthestObject<UniverseObject>(homeSector.Location, fleet.Owner, universeObjects);
                    if (furthestObject.Sector == sector)
                    {
                        value += GreatestDistance;
                    }
                }
                catch { GameLog.Client.AI.DebugFormat("unable to get furthest object form home world for station value"); }

                List<Sector> strandedShipSectors = FindStrandedShipSectors(fleet.Owner);
                if (strandedShipSectors.Count > 0)
                {
                    if (strandedShipSectors.Contains(sector))
                    {
                        value += StrandedShipSectorValue;
                    }
                }
            }
            //int randomInt = RandomHelper.Random(10);
            //GameLog.Core.AI.DebugFormat("Station at {0} has value {1}", sector.Location, (value + randomInt));

            return value; // + randomInt;

        }

        public static void BuidStation(Fleet fleet)
        {
            BuildStationOrder order = new BuildStationOrder();
            order.BuildProject = order.FindTargets(fleet).Cast<StationBuildProject>().LastOrDefault(o => o.StationDesign.IsCombatant);
            if (order.BuildProject != null && order.CanAssignOrder(fleet))
            {
                fleet.SetOrder(order);
                fleet.UnitAIType = UnitAIType.Constructor;
                fleet.Activity = UnitActivity.BuildStation;
                if (!fleet.Sector.GetOwnedFleets(fleet.Owner).Where(o => o.IsBattleFleet || o.IsFastAttack).Any()
                    && !fleet.Ships.Where(s => s.ShipType >= ShipType.FastAttack).Any())
                {
                    GetFleetEscort(fleet, fleet.Sector);
                }
                // GameLog.Core.AI.DebugFormat("Stranded constructor fleet {0} order {1}", fleet.Name, fleet.Order.OrderName);
            }
        }

        /*
        * Station best sector
        */

        /// <summary>
        /// Returns the best possible <see cref="Sector"/> for a given <see cref="Fleet"/>
        /// to build a <see cref="Station"/> in
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSectorForStation(Fleet fleet, out Sector result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            List<Fleet> constructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(f => f.IsConstructor || f.multiFleetHasAConstructor).ToList();

            //List<Fleet> otherFleets = constructorFleets.Where(o => o != fleet).ToList();
            //List<UniverseObject> objectsWhereOthersAreGoing = new List<UniverseObject>();
            ////Get a list object (sectors) where other consttuctors are going
            //if (otherFleets.Count > 0)
            //{
            //    objectsWhereOthersAreGoing = GameContext.Current.Universe.Find<UniverseObject>()
            //        .Where(x => otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is BuildStationOrder))
            //        .ToList();
            //}

            int halfMapWidthX = GameContext.Current.Universe.Map.Width / 2;
            int halfMapHeightY = GameContext.Current.Universe.Map.Height / 2;
            int thirdMapWidthX = GameContext.Current.Universe.Map.Width / 3;
            int thirdMapHeightY = GameContext.Current.Universe.Map.Height / 3;
            int quarterMapWidthX = GameContext.Current.Universe.Map.Width / 4;
            int quarterMapHeightY = GameContext.Current.Universe.Map.Height / 4;

            // var stationLocation = _station.Where(oGameContext.Current.Universe.Objects.(UniverseObjectType.Station).
            switch (fleet.Owner.Key)
            {
                case "BORG":
                    {
                        int borgX = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X;
                        int borgXDelta = Math.Abs(GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X - halfMapWidthX)/4;
                        int borgY = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y;
                        int borgYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y)/4;

                        var borgHomeLocation = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location;
                        var objectsAlongCenterAxis = GameContext.Current.Universe.Objects
                            .Where(s => s.Location != null
                            && s.Sector.Station == null
                            && (s.Location.X >= halfMapWidthX + borgXDelta && s.Location.X <= borgX )
                            && (s.Location.Y <= Math.Abs(halfMapHeightY - borgYDelta) && s.Location.Y >= borgY + borgYDelta))
                            //&& s.Location == borgHomeLocation)                         
                            .ToList();
                        //if (objectsWhereOthersAreGoing != null && objectsWhereOthersAreGoing.Count() > 0)
                        //{
                        //    foreach (UniverseObject universeObject in objectsWhereOthersAreGoing)
                        //    {
                        //        if (objectsAlongCenterAxis.Contains(universeObject))
                        //        {
                        //            objectsAlongCenterAxis.Remove(universeObject);
                        //        }
                        //    }
                        //}
                       // objectsAlongCenterAxis.RemoveRange(objectWhereOthersAreGoing);
                        if (objectsAlongCenterAxis.Count == 0)
                        {
                            result = null;
                            return false;
                        }
                        // GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), fleet.Owner.Key);

                        objectsAlongCenterAxis.Sort((a, b) =>
                            GetStationValue(a.Sector, fleet, objectsAlongCenterAxis)
                            .CompareTo(GetStationValue(b.Sector, fleet, objectsAlongCenterAxis)));
                        result = objectsAlongCenterAxis[objectsAlongCenterAxis.Count - 1].Sector;
                        // GameLog.Core.AI.DebugFormat("Borg station selected sector = {0} {1}", result.Location, result.Name);
                        return true;
                    }

                case "DOMINION":
                    {
                        int domX = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X;
                        int domXDelta = Math.Abs(halfMapWidthX - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X) / 4;
                        int domY = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y;
                        int domYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y) / 4;

                        var objectsAlongCenterAxis = GameContext.Current.Universe.Objects
                           // .Where(c => !FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet))
                            .Where(s => s.Location != null
                            && s.Sector.Station == null
                            && (s.Location.X <= Math.Abs(halfMapWidthX - domXDelta) && s.Location.X > domX)
                            && (s.Location.Y <= halfMapHeightY - domYDelta && s.Location.Y > domY))
                            // find a list of objects in some sector around Dom side of galactic center
                            .ToList();
                        //if (objectsWhereOthersAreGoing != null && objectsWhereOthersAreGoing.Count() > 0)
                        //{
                        //    foreach (UniverseObject universeObject in objectsWhereOthersAreGoing)
                        //    {
                        //        if (objectsAlongCenterAxis.Contains(universeObject))
                        //        {
                        //            objectsAlongCenterAxis.Remove(universeObject);
                        //        }
                        //    }
                        //}

                        if (objectsAlongCenterAxis.Count == 0)
                        {
                            result = null;
                            return false;
                        }
                        //GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), fleet.Owner.Key);

                        objectsAlongCenterAxis.Sort((a, b) =>
                            GetStationValue(a.Sector, fleet, objectsAlongCenterAxis)
                            .CompareTo(GetStationValue(b.Sector, fleet, objectsAlongCenterAxis)));
                        result = objectsAlongCenterAxis[objectsAlongCenterAxis.Count - 1].Sector;
                        // GameLog.Core.AI.DebugFormat("Dominion station selected sector = {0} {1}", result.Location, result.Name);
                        return true;
                    }
                case "KLINGON":
                case "TERRANEMPIRE":
                case "FEDERATION":
                case "ROMULANS":
                case "CARDASSIANS":
                    {
                        //var furthestObject = GameContext.Current.Universe.FindFurthestObject<UniverseObject>(homeSector.Location, fleet.Owner);
                        Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Sector;

                        var objectsAroundHome = GameContext.Current.Universe.Objects 
                            .Where(s => s.Location != null
                            && s.Sector.Station == null
                            && !s.CanMove
                            && (((s.Location.X <= homeSector.Location.X + thirdMapWidthX && s.Location.X >= homeSector.Location.X + quarterMapWidthX)
                            || (s.Location.X >= Math.Abs(homeSector.Location.X - thirdMapWidthX) && s.Location.X <= Math.Abs(homeSector.Location.X - quarterMapWidthX))
                            && (s.Location.Y >= Math.Abs(homeSector.Location.Y - quarterMapHeightY) && s.Location.Y <= homeSector.Location.Y + quarterMapHeightY))
                            || ((s.Location.Y <= Math.Abs(homeSector.Location.Y - quarterMapHeightY) && s.Location.Y >= Math.Abs(homeSector.Location.Y - thirdMapHeightY))
                            || (s.Location.Y <= homeSector.Location.Y + thirdMapHeightY && s.Location.Y >= homeSector.Location.Y + quarterMapHeightY)
                            && (s.Location.X >= Math.Abs(homeSector.Location.X - quarterMapWidthX) && s.Location.X <= homeSector.Location.X + quarterMapWidthX))))
                            .ToList();
                        if (objectsAroundHome.Count == 0)
                        {
                            result = null;
                            return false;
                        }
                        //if (objectsWhereOthersAreGoing != null && objectsWhereOthersAreGoing.Count() > 0)
                        //{
                        //    foreach (UniverseObject universeObject in objectsWhereOthersAreGoing)
                        //    {
                        //        if (objectsAroundHome.Contains(universeObject))
                        //        {
                        //            objectsAroundHome.Remove(universeObject);
                        //        }
                        //    }
                        //}

                        // GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAroundHome.Count(), fleet.Owner.Key);
                        objectsAroundHome.Sort((a, b) =>
                                      GetStationValue(a.Sector, fleet, objectsAroundHome)
                                      .CompareTo(GetStationValue(b.Sector, fleet, objectsAroundHome)));

                        result = objectsAroundHome[objectsAroundHome.Count - 1].Sector;
                        //GameLog.Core.AI.DebugFormat("{0} station selected sector = {1} {2}", fleet.Owner.Key ,result.Location, result.Name);
                        return true;
                    }
                default:
                    result = null;
                    //GameLog.Core.AI.DebugFormat("{0} no sector for station", fleet.Owner.Key);
                    return false; // could not find sector for station
            }
        }

        /*
        * Medical value
        */

        /// <summary>
        /// Determines the value of a <see cref="Civilization"/> providing
        /// medical services to a <see cref="Colony"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetMedicalValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            //Tweak these to set priorities
            const int OwnColonyPriority = 100;
            const int AlliedColonyPriority = 15;
            const int FriendlyColonyPriority = 10;
            const int NeutralColonyPriority = 5;

            int value = 0;

            if (colony.Owner == civ)
            {
                value += OwnColonyPriority;
            }
            else if (DiplomacyHelper.AreAllied(colony.Owner, civ))
            {
                value += AlliedColonyPriority;
            }
            else if (DiplomacyHelper.AreFriendly(colony.Owner, civ))
            {
                value += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(colony.Owner, civ))
            {
                value += NeutralColonyPriority;
            }

            value += 100 - colony.Health.CurrentValue;
            // GameLog.Core.AI.DebugFormat("Medical value for {0} is {1})", colony, value);
            return value;
        }

        /*
        * Medical best colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyForMedical(Fleet fleet, out Colony result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> medicalShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsMedical);
            List<Colony> possibleColonies = new List<Colony>();
            if (fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                && !medicalShips.Any(f => f.Location == c.Location)
                && !medicalShips.Any(f => f.Route.Waypoints.LastOrDefault() == c.Location)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                //Where we can enter the sector
                //Where there aren't any hostiles
                //Where they aren't at war
                .ToList();
            }

            if (possibleColonies.Count == 0)
            {
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetMedicalValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetMedicalValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            return true;
        }

        /*
        * Diplomacy value
        */

        /// <summary>
        /// Determines the value diplomacy <see cref="Colony"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetDiplomaticValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            Civilization otherCiv = colony.Owner;
            if (colony.System.Name != otherCiv.HomeSystemName)
                return 0;
            var diplomat = Diplomat.Get(civ);

            if (otherCiv.CivID == civ.CivID)
                return 0;
            if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
                return 0;

            var foreignPower = diplomat.GetForeignPower(otherCiv);
            var otherdiplomat = Diplomat.Get(otherCiv);
            ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
            if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember || otherForeignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
                return 0;

            #region Foriegn Traits List

            string traitsOfForeignCiv = otherCiv.Traits;
            string[] foreignTraits = traitsOfForeignCiv.Split(',');

            #endregion

            #region The Civ's Traits List

            string traitsOfCiv = civ.Traits;
            string[] theCivTraits = traitsOfCiv.Split(',');

            #endregion

            // traits in common relative to the number of triats a civilization has
            var commonTraitItems = foreignTraits.Intersect(theCivTraits);

            int countCommon = 0;
            foreach (string aString in commonTraitItems)
            {
                countCommon++;
            }

            int[] countArray = new int[] { foreignTraits.Length, theCivTraits.Length };
            int fewestTotalTraits = countArray.Min();

            int similarTraits = (countCommon * 10 / fewestTotalTraits);

            const int EnemyColonyPriority = 0;
            const int NeutralColonyPriority = 10;
            const int FriendlyColonyPriority = 5;

            int value = similarTraits;

            if (DiplomacyHelper.AreAllied(otherCiv, civ) || DiplomacyHelper.AreFriendly(otherCiv, civ))
            {
                similarTraits += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(otherCiv, civ))
            {
                similarTraits += NeutralColonyPriority;
            }
            else if (DiplomacyHelper.AreAtWar(otherCiv, civ))
            {
                similarTraits += EnemyColonyPriority;
            }

            GameLog.Core.AI.DebugFormat("diplomacy value for {0} belonging to {1} is {2} to the {3}", colony.Name, otherCiv.Key, value, civ.Key);
            return similarTraits;
        }

        /*
        * Diplomacy best colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyForDiplomacy(Fleet fleet, out Colony result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> diplomaticShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsDiplomatic);
            List<Colony> possibleColonies = new List<Colony>();
            if (fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location)
                && mapData.IsExplored(s.Location)
                && s.Owner != fleet.Owner)
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                //&& !diplomaticShips.Any(f => f.Location == c.Location)
                //&& !diplomaticShips.Any(f => f.Route.Waypoints.LastOrDefault() == c.Location)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                //Where we can enter the sector
                //Where there aren't any hostiles
                //Where they aren't at war
                .ToList();
            }

            if (possibleColonies.Count == 0)
            {
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetDiplomaticValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetDiplomaticValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            return true;
        }

        /*
         * Spying value
         */

        /// <summary>
        /// Determines the value of spying on a <see cref="Colony"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetSpyingValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }
            Civilization otherCiv = colony.Owner;
            if (colony.System.Name == otherCiv.HomeSystemName)
                return 1000;
            if (otherCiv.CivID == civ.CivID)
                return 0;
            if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
                return 0;

            const int EnemyColonyPriority = 50;
            const int NeutralColonyPriority = 25;
            const int FriendlyColonyPriority = 10;

            int value = 0;

            if (DiplomacyHelper.AreAllied(colony.Owner, civ) || DiplomacyHelper.AreFriendly(colony.Owner, civ))
            {
                value += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(colony.Owner, civ))
            {
                value += NeutralColonyPriority;
            }
            else if (DiplomacyHelper.AreAtWar(colony.Owner, civ))
            {
                value += EnemyColonyPriority;
            }

           // GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", colony, value);
            return value;

        }

        /*
        * Science value
        */

        /// <summary>
        /// Determines the value of science on a <see cref="StarSystem"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="system"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetScienceValue(StarSystem system, Civilization civ) // civ is fleet.Owner
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }
            if (system.Owner != null && system.Owner != civ)
            {
                Civilization otherCiv = system.Owner;
                if (DiplomacyHelper.AreAtWar(system.Owner, civ))
                    return 0;
            }
            const int StarTypeNebula = 5;
            const int StarTypeColor = 10;
            const int StarTypeMoreFun = 15;
            const int StarTypeBlackHoleQuasar = 20;
            const int StarTypeWormhole = 30;
            int value = 0;

            if (system.StarType == StarType.Nebula)
            {
                value += StarTypeNebula;
            }
            else if (system.StarType == StarType.Blue ||
                system.StarType == StarType.Orange ||
                system.StarType == StarType.Red ||
                system.StarType == StarType.White ||
                system.StarType == StarType.Yellow)
            {
                value += StarTypeColor;
            }
            else if (system.StarType == StarType.XRayPulsar ||
                system.StarType == StarType.RadioPulsar ||
                system.StarType == StarType.NeutronStar)
            {
                value += StarTypeMoreFun;
            }
            else if (system.StarType == StarType.Quasar || system.StarType == StarType.BlackHole)
            {
                value += StarTypeBlackHoleQuasar;
            }
            else if (system.StarType == StarType.Wormhole)
            {
                value += StarTypeWormhole;
            }

            // GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", system, value);
            return value;
        }

        /*
        / Spy best colony
        */

        /// <summary>
        /// Gets the best <see cref="Colony"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// 
        public static bool GetBestColonyForSpying(Fleet fleet, out Colony result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> spyShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsSpy);
            List<Colony> possibleColonies = new List<Colony>();
            if (fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //That isn't owned by us
                .Where(c => c.Owner != fleet.Owner && mapData.IsScanned(c.Location) && c.Owner.IsEmpire
                && mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                && CheckForSpyNetwork(c.Owner, fleet.Owner) == false
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector))
                //&& !spyShips.Any(f => f.Location == c.Location)
                //&& !spyShips.Any(f => f.Route.Waypoints.LastOrDefault() == c.Location))
                //We need to know about it (no cheating)
                //In fuel range
                //Where there isn't a spy ship already there or heading there
                .ToList();
            }

            if (possibleColonies.Count == 0)
            {
                // GameLog.Client.AI.DebugFormat("Damn, no Home System of Empire found, possible colonies = {0}", possibleColonies.Count());
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetSpyingValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetSpyingValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            // GameLog.Client.AI.DebugFormat("Yippy, System of Empire found!, possible spied colony = {0}", possibleColonies.FirstOrDefault().Name);
            return true;
        }

        /*
        / Science best system
        */

        /// <summary>
        /// Gets the best <see cref="StarSystem"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSystemForScience(Fleet fleet, out StarSystem result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> scienceShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsScience);
            List<StarSystem> possibleSystems = new List<StarSystem>();
            if (fleet.Owner != null)
            {
                possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
                //That isn't owned by us
                .Where(c => c.Sector != null && mapData.IsScanned(c.Location)
                && mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                )
                //&& !scienceShips.Any(f => f.Location == c.Location)
                //&& !scienceShips.Any(f => f.Route.Waypoints.LastOrDefault() == c.Location))
                //We need to know about it (no cheating)
                //In fuel range
                //Where there isn't a science ship already there or heading there
                .ToList();
            }
            if (possibleSystems.Contains(civManager.HomeSystem))
            {
                possibleSystems.Remove(civManager.HomeSystem);
            }
            //foreach (var system in possibleSystems)
            //{
            //    if (system.Owner != null && system.Owner.IsEmpire && GameContext.Current.CivilizationManagers[system.Owner].HomeSystem == system )
            //    {
            //        possibleSystems.Remove(system);
            //    }
            //}
            if (possibleSystems.Count == 0)
            {
                //  GameLog.Client.AI.DebugFormat("Damn, no Science System of Empire found, possible colonies = {0}", possibleSystems.Count());
                result = null;
                return false;
            }

            possibleSystems.Sort((a, b) =>
                (GetScienceValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetScienceValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleSystems[possibleSystems.Count - 1];
            //  GameLog.Client.AI.DebugFormat("Yippy, Science System found!, possible  = {0}", possibleSystems.FirstOrDefault().Name);
            return true;
        }

        /// <summary>
        /// Provides a modifier to prioritise targets that are closer the home system
        /// of the <see cref="Civilization"/> that owns the <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="targetSector"></param>
        /// <returns></returns>
        public static float HomeSystemDistanceModifier(Fleet fleet, Sector targetSector)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            if (targetSector == null)
            {
                throw new ArgumentNullException(nameof(targetSector));
            }

            int distance = MapLocation.GetDistance(targetSector.Location, GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem.Location);
            return 1 / (distance + 1);
        }
        public static bool CheckForSpyNetwork(Civilization civSpied, Civilization civSpying)
        {
            if (civSpied == null)
            {
                return false;
            }
            List<Civilization> spiedCivs = new List<Civilization>();
            switch (civSpied.CivID)
            {
                case 0:
                    spiedCivs = IntelHelper.SpyingCiv_0_List;
                    break;
                case 1:
                    spiedCivs = IntelHelper.SpyingCiv_1_List;
                    break;
                case 2:
                    spiedCivs = IntelHelper.SpyingCiv_2_List;
                    break;
                case 3:
                    spiedCivs = IntelHelper.SpyingCiv_3_List;
                    break;
                case 4:
                    spiedCivs = IntelHelper.SpyingCiv_4_List;
                    break;
                case 5:
                    spiedCivs = IntelHelper.SpyingCiv_5_List;
                    break;
                    //case 6:
                    //default:
                    //    return true;
            }
            if (spiedCivs != null && spiedCivs.Contains(civSpying))
                return true;
            return false;

        }
        private static List<Sector> FindStrandedShipSectors(Civilization civ)
        {
            List<Fleet> strandedFleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).Where(o => o.IsStranded).ToList();
            List<Sector> sectorList = new List<Sector>();
            foreach (Sector sector in sectorList)
            {
                sectorList.Add(sector);
            }
            return sectorList;
        }
    }
}
