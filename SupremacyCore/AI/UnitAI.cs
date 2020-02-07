// UnitAI.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
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
        public static void DoTurn([NotNull] Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            foreach (var fleet in GameContext.Current.Universe.FindOwned<Fleet>(civ))
            {
                GameLog.Core.AI.DebugFormat("Processing Fleet {0} in {1}...", fleet.ObjectID, fleet.Location);

                //Make sure all fleets are cloaked
                foreach (var ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    GameLog.Core.AI.DebugFormat("Cloaking {0} {1}", ship.Name, ship.ObjectID);
                    ship.IsCloaked = true;
                }

                //If the fleet can't move, we're limited in our options
                if (!fleet.CanMove)
                    continue;

                //Set scouts to permanently explore
                if (fleet.IsScout)
                {
                    if (fleet.Activity == UnitActivity.NoActivity)
                    {
                        fleet.SetOrder(new ExploreOrder());
                        fleet.UnitAIType = UnitAIType.Explorer;
                        fleet.Activity = UnitActivity.Mission;
                        GameLog.Core.AI.DebugFormat("Ordering Scout fleet {0} to explore", fleet.ObjectID, fleet.Location);
                    }
                }

                if (fleet.IsColonizer)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        StarSystem bestSystemToColonize;
                        //Do we have a system to colonize?
                        if (GetBestSystemToColonize(fleet, out bestSystemToColonize))
                        {
                            //Are we there?
                            if (fleet.Sector == bestSystemToColonize.Sector)
                            {
                                //Colonize
                                fleet.SetOrder(new ColonizeOrder());
                                fleet.UnitAIType = UnitAIType.Colonizer;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering colonizer fleet {0} in {1} to colonize", fleet.ObjectID, fleet.Location);
                            }
                            else
                            {
                                //Head to the system
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemToColonize.Sector }));
                                fleet.UnitAIType = UnitAIType.Colonizer;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering colonizer fleet {0} to {1}", fleet.ObjectID, bestSystemToColonize);
                            }
                        }
                        else
                        {
                            GameLog.Core.AI.DebugFormat("Nothing to do for colonizer fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                if (fleet.IsConstructor)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete) {
                        Sector bestSectorForStation;
                        if (GetBestSectorForStation(fleet, out bestSectorForStation))
                        {
                            if (fleet.Sector == bestSectorForStation)
                            {
                                //Build the station
                                var order = new BuildStationOrder();
                                order.BuildProject = order.FindTargets(fleet).Cast<StationBuildProject>().LastOrDefault(o => o.StationDesign.IsCombatant);
                                if (order.BuildProject != null && order.CanAssignOrder(fleet))
                                {
                                    fleet.SetOrder(order);
                                    fleet.Activity = UnitActivity.Mission;
                                }
                                GameLog.Core.AI.DebugFormat("Ordering constructor fleet {0} to build station", fleet.ObjectID);
                            } 
                            else
                            {
                                //Head to the system
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSectorForStation }));
                                fleet.UnitAIType = UnitAIType.Constructor;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering constructor fleet {0} to {1}", fleet.ObjectID, bestSectorForStation);
                            }
                        }
                        else
                        {
                            GameLog.Core.AI.DebugFormat("Nothing to do for constructor fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                //TODO: Refactor battle fleet
                if (fleet.IsBattleFleet)
                {
                    GameLog.Core.AI.DebugFormat("## IsBattleFleet ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                    var defenseFleet = GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                    if (fleet.Activity == UnitActivity.NoActivity && defenseFleet == null)
                    {
                        fleet.UnitAIType = UnitAIType.SystemDefense;
                        fleet.Activity = UnitActivity.Hold;
                        GameLog.Core.AI.DebugFormat("## IsBattleFleet - on SystemDefence ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                    }
                    else if (fleet.Activity == UnitActivity.NoActivity && fleet.Ships.Count == 1 && fleet.Sector == defenseFleet.Sector)
                    {
                        var ship = fleet.Ships[0];
                        GameLog.Core.AI.DebugFormat("## IsBattleFleet - on SystemDefence - Ship added ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                        fleet.RemoveShip(ship);
                        defenseFleet.AddShip(ship);
                    }
                }

                if (fleet.IsMedical)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        Colony bestSystemForMedical;
                        if (GetBestColonyForMedical(fleet, out bestSystemForMedical))
                        {
                            if (bestSystemForMedical.Location == fleet.Location)
                            {
                                //Colonize
                                fleet.SetOrder(new MedicalOrder());
                                fleet.UnitAIType = UnitAIType.Medical;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering medical fleet {0} in {1} to treat the population", fleet.ObjectID, fleet.Location);
                            }
                            else
                            {
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForMedical.Sector }));
                                fleet.UnitAIType = UnitAIType.Medical;
                                fleet.Activity = UnitActivity.Mission;
                                GameLog.Core.AI.DebugFormat("Ordering medical fleet {0} to {1}", fleet.ObjectID, bestSystemForMedical);
                            }
                        }
                        else
                        {
                            GameLog.Core.AI.DebugFormat("Nothing to do for medical fleet {0}", fleet.ObjectID);
                        }
                    }
                }

                //TODO
                if (fleet.IsDiplomatic)
                {

                }

                //TODO
                if (fleet.IsSpy)
                {
                    //if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    //{
                    //    Colony bestSystemForSpying;
                    //    if (GetBestColonyForSpying(fleet, out bestSystemForSpying))
                    //    {
                    //        if (bestSystemForSpying.Location == fleet.Location)
                    //        {
                    //            //Spy
                    //            fleet.SetOrder(new SabotageOrder());
                    //            fleet.UnitAIType = UnitAIType.Spy;
                    //            fleet.Activity = UnitActivity.Mission;
                    //            GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} in {1} to sabotage the system", fleet.ObjectID, fleet.Location);
                    //        }
                    //        else
                    //        {
                    //            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForSpying.Sector }));
                    //            fleet.UnitAIType = UnitAIType.Medical;
                    //            fleet.Activity = UnitActivity.Mission;
                    //            GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} to {1}", fleet.ObjectID, bestSystemForSpying);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        GameLog.Core.AI.DebugFormat("Nothing to do for spy fleet {0}", fleet.ObjectID);
                    //    }
                    //}
                }

                //TODO
                if (fleet.IsScience)
                {

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
                throw new ArgumentNullException("fleet");

            var colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(o => o.IsColonizer);

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            var mapData = civManager.MapData;

            //Get a list of all systems that we can colonise
            var systems = GameContext.Current.Universe.Find<StarSystem>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                //That isn't owned by another civilization
                .Where(s => !s.IsOwned || s.Owner == fleet.Owner)
                //That isn't currently inhabited or have a current colony
                .Where(s => !s.IsInhabited || !s.HasColony)
                //That's actually inhabitable
                .Where(s => s.IsHabitable(fleet.Owner.Race))
                //That are in fuel range
                .Where(s => FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet))
                //That doesn't have potential enemies in it
                .Where(s => GameContext.Current.Universe.FindAt<Orbital>(s.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner)))
                //Where a ship isn't heading there already
                .Where(s => colonizerFleets.Any(f => f.Route.Waypoints.LastOrDefault() == s.Location))
                //Where a ship isn't there and colonizing
                .Where(s => colonizerFleets.Where(f => (f.Location == s.Location)).Any(f => f.Order is ColonizeOrder))
                .ToList();

            if (systems.Count() == 0)
            {
                result = null;
                return false;
            }

            systems.Sort((a, b) =>
                (GetColonizeValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetColonizeValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = systems[systems.Count() - 1];
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
                throw new ArgumentNullException("system");
            if (civ == null)
                throw new ArgumentNullException("civ");

            //Alter this to alter priority
            int DilithiumBonusValue = 20;
            int DuraniumBonusValue = 20;

            float value = 0;

            if (system.HasDilithiumBonus)
                value += DilithiumBonusValue;

            if (system.HasRawMaterialsBonus)
                value += DuraniumBonusValue;

            value += system.GetMaxPopulation(civ.Race) * system.GetGrowthRate(civ.Race);
            GameLog.Core.AI.DebugFormat("Colonize value for {0} is {1}", system, value);
            return value;
        }

        /*
         * Exploration
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
                throw new ArgumentNullException("sector");
            if (civ == null)
                throw new ArgumentNullException("civ");

            //These values are the priority of each item
            int UnscannedSectorValue = 500;
            int UnexploredSectorValue = 200;
            int HasStarSystemValue = 300;
            int InitiatesFirstContactValue = 200;

            int value = 0;

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];
            CivilizationMapData mapData = civManager.MapData;

            //Unscanned
            if (!mapData.IsScanned(sector.Location))
                value += UnscannedSectorValue;

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
            if (sector.System != null && sector.System.HasColony && (sector.System.Colony.Owner != civ)  && !DiplomacyHelper.IsContactMade(sector.Owner, civ))
                value += InitiatesFirstContactValue;

            GameLog.Core.AI.DebugFormat("Explore priority for {0} is {1}", sector, value);
            return value;
        }

        /// <summary>
        /// Gets the best <see cref="Sector"/> to explore for the given <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static bool GetBestSectorToExplore(Fleet fleet, out Sector sector)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            List<Fleet> ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(f => f.CanMove && f != fleet && !f.Route.IsEmpty).ToList();

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            var mapData = civManager.MapData;

            var starsToExplore = GameContext.Current.Universe.Find<StarSystem>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location))
                //No point exploring our own space
                .Where(s => !s.IsOwned || (s.Owner != fleet.Owner))
                //Where we can enter the sector
                .Where(s => DiplomacyHelper.IsTravelAllowed(fleet.Owner, s.Sector))
                //Where is in fuel range of the ship
                .Where(s => FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet))
                //Where no fleets are already heading there or through there
                .Where(s => !ownFleets.Any(f => f.Route.Waypoints.Any(wp => s.Location == wp)))
                .ToList();

            if (starsToExplore.Count() == 0)
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
         * Station
         */

        /// <summary>
        /// Determines how valuable a <see cref="Station"/> would be
        /// in a given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetStationValue(Sector sector, Civilization civ)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            if (civ == null)
                throw new ArgumentNullException("civ");

            int HomeSystemValue = 2000;
            int SeatOfGovernmentValue = 2000;

            int value = 0;

            var civManager = GameContext.Current.CivilizationManagers[civ];

            if ((sector.System != null) && sector.System.HasColony)
            {
                value += sector.System.Colony.ColonyValue();
                if (sector.System == civManager.HomeSystem)
                    value += HomeSystemValue;

                if (sector.System == civManager.SeatOfGovernment)
                    value += SeatOfGovernmentValue;

            }

            GameLog.Core.AI.DebugFormat("Station value for {0} is {1}", sector, value);
            return value;

        }

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
                throw new ArgumentNullException("fleet");

            var constructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(f => f.IsConstructor);

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            var mapData = civManager.MapData;

            var possibleSectors = GameContext.Current.Universe.Find<StarSystem>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                //That isn't owned by an opposition
                .Where(s => (s.Owner == null) || (s.Owner == fleet.Owner))
                //That's within fuel range of the ship
                .Where(s => FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet))
                //That hasn't got a station already
                .Where(s => s.Sector.Station == null)
                //Where a ship isn't heading there already
                .Where(s => constructorFleets.Any(f => f.Route.Waypoints.LastOrDefault() == s.Location))
                //Where one isn't under construction
                .Where(s => !constructorFleets.Where(f => f.Location == s.Location).Any(f => f.Order is BuildStationOrder))
                .ToList();

            if (possibleSectors.Count() == 0)
            {
                result = null;
                return false;
            }

            possibleSectors.Sort((a, b) => 
                GetStationValue(a.Sector, fleet.Owner)
                .CompareTo(GetStationValue(b.Sector, fleet.Owner)));
            result = possibleSectors[possibleSectors.Count() - 1].Sector;
            return true;
        }

        /*
         * Medical
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
                throw new ArgumentNullException("fleet");

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            var mapData = civManager.MapData;

            var possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet))
                //Where we can enter the sector
                .Where(c => DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector))
                //Where there aren't any hostiles
                .Where(c => GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner)))
                //Where they aren't at war
                .Where(c => !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                .ToList();

            if (possibleColonies.Count() == 0)
            {
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) => 
                (GetMedicalValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetMedicalValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count() - 1];
            return true;
        }

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
                throw new ArgumentNullException("colony");
            if (civ == null)
                throw new ArgumentNullException("civ");

            //Tweak these to set priorities
            int OwnColonyPriority = 100;
            int AlliedColonyPriority = 15;
            int FriendlyColonyPriority = 10;
            int NeutralColonyPriority = 5;

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

            value += (100 - colony.Health.CurrentValue);
            GameLog.Core.AI.DebugFormat("Medical value for {0} is {1})", colony, value);
            return value;
        }

        /*
         * Spying
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
                throw new ArgumentNullException("colony");

            if (civ == null)
                throw new ArgumentNullException("civ");

            int EnemyColonyPriority = 50;
            int NeutralColonyPriority = 25;
            int FriendlyColonyPriority = 10;

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

            GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", colony, value);
            return value;

        }

        /// <summary>
        /// Gets the best <see cref="Colony"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyForSpying(Fleet fleet, out Colony result)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            var mapData = civManager.MapData;
            var spyShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsSpy);

            var possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //That isn't owned by us
                .Where(c => c.Owner != fleet.Owner)
                //We need to know about it (no cheating)
                .Where(c => mapData.IsScanned(c.Location) && mapData.IsExplored(c.Location))
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet))
                //Where there isn't a spy ship already there or heading there
                .Where(c => !spyShips.Any(f => f.Location == c.Location))
                .Where(c => !spyShips.Any(f => f.Route.Waypoints.LastOrDefault() == c.Location))
                .ToList();

            if (possibleColonies.Count() == 0)
            {
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetSpyingValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetSpyingValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count() - 1];
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
                throw new ArgumentNullException("fleet");
            if (targetSector == null)
                throw new ArgumentNullException("targetSector");

            var distance = MapLocation.GetDistance(targetSector.Location, GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem.Location);
            return 1 / (distance + 1);
        }
    }
}
