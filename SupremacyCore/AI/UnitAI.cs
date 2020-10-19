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
        public static void DoTurn([NotNull] Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            foreach (Fleet fleet in GameContext.Current.Universe.FindOwned<Fleet>(civ))
            {
                GameLog.Core.AI.DebugFormat("Turn {2}: Processing Fleet {0} in {1}...", fleet.ObjectID, fleet.Location, GameContext.Current.TurnNumber);

                //Make sure all fleets are cloaked
                foreach (Ship ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    GameLog.Core.AI.DebugFormat("Cloaking {0} {1}", ship.Name, ship.ObjectID);
                    ship.IsCloaked = true;
                }

                //If the fleet can't move, we're limited in our options
                if (!fleet.CanMove)
                {
                    continue;
                }

                //Set scouts to permanently explore
                if ((fleet.IsScout || fleet.IsFastAttack) && fleet.Activity == UnitActivity.NoActivity)
                {
                    fleet.SetOrder(new ExploreOrder());
                    fleet.UnitAIType = UnitAIType.Explorer;
                    fleet.Activity = UnitActivity.Mission;
                    GameLog.Core.AI.DebugFormat("Ordering Scout & FastAttack {0} to explore from {1}", fleet.ClassName, fleet.Location);
                }

                if (fleet.IsColonizer)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        //Do we have a system to colonize?
                        if (GetBestSystemToColonize(fleet, out StarSystem bestSystemToColonize))
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
                    if (fleet.IsStranded) // stranded construction ship builds station
                    {
                        BuildStationOrder order = new BuildStationOrder();
                        order.BuildProject = order.FindTargets(fleet).Cast<StationBuildProject>().LastOrDefault(o => o.StationDesign.IsCombatant);
                        if (order.BuildProject != null && order.CanAssignOrder(fleet))
                        {
                            fleet.SetOrder(order);
                            fleet.Activity = UnitActivity.Mission;
                        }
                        GameLog.Core.AI.DebugFormat("Stranded constructor fleet {0} order {1}", fleet.Name, fleet.Order.OrderName);
                    }
                    else if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        if (GetBestSectorForStation(fleet, out Sector bestSectorForStation))
                        {
                            if (fleet.Sector == bestSectorForStation)
                            {
                                //Build the station
                                BuildStationOrder order = new BuildStationOrder();
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
                    Fleet defenseFleet = new Fleet();
                    if (GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense) == null)                      
                            defenseFleet = fleet;

                    defenseFleet = GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                    if (fleet.Activity == UnitActivity.NoActivity && defenseFleet == null)
                    {
                        fleet.UnitAIType = UnitAIType.SystemDefense;
                        fleet.Activity = UnitActivity.Hold;
                        GameLog.Core.AI.DebugFormat("## IsBattleFleet - on SystemDefence ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                    }
                    else if (fleet.Activity == UnitActivity.NoActivity && fleet.Ships.Count == 1 && fleet.Sector == defenseFleet.Sector)
                    {
                        Ship ship = fleet.Ships[0];
                        GameLog.Core.AI.DebugFormat("## IsBattleFleet - on SystemDefence - Ship added ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                        fleet.RemoveShip(ship);
                        defenseFleet.AddShip(ship);
                    }
                }

                if (fleet.IsMedical)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        if (GetBestColonyForMedical(fleet, out Colony bestSystemForMedical))
                        {
                            if (bestSystemForMedical.Location == fleet.Location)
                            {
                                //Colony medical treatment
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
                                        GameLog.Core.AI.DebugFormat("Ordering diplomacy fleet {0} in {1} to influence", fleet.ObjectID, fleet.Location);
                                    }
                                    else
                                    {
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForDiplomacy.Sector }));
                                        fleet.UnitAIType = UnitAIType.Diplomatic;
                                        fleet.Activity = UnitActivity.Mission;
                                        GameLog.Core.AI.DebugFormat("Ordering diplomacy fleet {0} to {1}", fleet.ObjectID, bestSystemForDiplomacy);
                                    }
                            }
                        }
                        else
                        {
                            GameLog.Core.AI.DebugFormat("Nothing to do for diplomacy fleet {0}", fleet.ObjectID);
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
                            if (bestSystemForSpying.OwnerID < 6)
                            { 
                                bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForSpying.Owner, fleet.Owner);
                                if (!hasOurSpyNetwork)
                                {
                                    if (bestSystemForSpying.Location == fleet.Location)
                                    {
                                        fleet.SetOrder(new SpyOnOrder()); // install spy network
                                        fleet.UnitAIType = UnitAIType.Spy;
                                        fleet.Activity = UnitActivity.Mission;
                                        GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} in {1} to install spy network", fleet.ObjectID, fleet.Location);
                                    }
                                    else
                                    {
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystemForSpying.Sector }));
                                        fleet.UnitAIType = UnitAIType.Spy;
                                        fleet.Activity = UnitActivity.Mission;
                                        GameLog.Core.AI.DebugFormat("Ordering spy fleet {0} to {1}", fleet.ObjectID, bestSystemForSpying);
                                    }
                                }
                            }
                        }
                        else
                        {
                            GameLog.Core.AI.DebugFormat("Nothing to do for spy fleet {0}", fleet.ObjectID);
                        }
                    }
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
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            IEnumerable<Fleet> colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(o => o.IsColonizer);

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            //Get a list of all systems that we can colonise
            List<StarSystem> systems = GameContext.Current.Universe.Find<StarSystem>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location)
                && (!s.IsOwned || s.Owner == fleet.Owner) && (!s.IsInhabited || !s.HasColony)
                && s.IsHabitable(fleet.Owner.Race) && FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet)
                && GameContext.Current.Universe.FindAt<Orbital>(s.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && colonizerFleets.Any(f => f.Route.Waypoints.LastOrDefault() == s.Location) && colonizerFleets.Any(f => (f.Location == s.Location)
                && f.Order is ColonizeOrder))
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

            systems.Sort((a, b) =>
                (GetColonizeValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetColonizeValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = systems[systems.Count - 1];
            GameLog.Client.AI.DebugFormat("{0} found a system to colonize at {1} {2}", fleet.Owner.Key, result.Location, result.Name);
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
            GameLog.Core.AI.DebugFormat("Colonize value for {0} is {1}", system, value);
            return value;
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
                GameLog.Client.General.ErrorFormat("fleet.ObjectId ={0} {1} {2} error ={3}",fleet.ObjectID, fleet.Name, fleet.ClassName, e );
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            List<StarSystem> starsToExplore = GameContext.Current.Universe.Find<StarSystem>()
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
         * Station value
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
            {
                throw new ArgumentNullException(nameof(sector));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            const int HomeSystemValue = 2000;
            const int SeatOfGovernmentValue = 2000;
            const int StrandedShipSectorValue = 2000;

            int value = 0;

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];

            if ((sector.System?.HasColony == true))
            {
                value += sector.System.Colony.ColonyValue();
                if (sector.System == civManager.HomeSystem)
                {
                    value += HomeSystemValue;
                }

                if (sector.System == civManager.SeatOfGovernment)
                {
                    value += SeatOfGovernmentValue;
                }
            }
            List<Sector> strandedShipSectors = FindStrandedShipSectors(civ);
            if (strandedShipSectors.Count > 0)
            {
                if (strandedShipSectors.Contains(sector))
                {
                    value += StrandedShipSectorValue;
                }
            }

            GameLog.Core.AI.DebugFormat("Station value for {0} is {1}", sector, value);
            return value;
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

            IEnumerable<Fleet> constructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(f => f.IsConstructor);

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            List<StarSystem> possibleSectors = GameContext.Current.Universe.Find<StarSystem>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location)
                && ((s.Owner == null) || (s.Owner == fleet.Owner)) && FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet)
                && s.Sector.Station == null && constructorFleets.Any(f => f.Route.Waypoints.LastOrDefault() == s.Location)
                && !constructorFleets.Where(f => f.Location == s.Location).Any(f => f.Order is BuildStationOrder))
                //That isn't owned by an opposition
                //That's within fuel range of the ship
                //That hasn't got a station already
                //Where a ship isn't heading there already
                //Where one isn't under construction
                .ToList();
            //List<Sector> possibleSectors = GameContext.Current.Universe.Find<Sector>()
                //.Where(s => s.)
            if (possibleSectors.Count == 0)
            {
                result = null;
                return false;
            }

            possibleSectors.Sort((a, b) =>
                GetStationValue(a.Sector, fleet.Owner)
                .CompareTo(GetStationValue(b.Sector, fleet.Owner)));
            result = possibleSectors[possibleSectors.Count - 1].Sector;
            return true;
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
            GameLog.Core.AI.DebugFormat("Medical value for {0} is {1})", colony, value);
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

            List<Colony> possibleColonies = GameContext.Current.Universe.Find<Colony>()
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

            List<Colony> possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
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
            if (colony.System.Name != otherCiv.HomeSystemName)
                return 0;
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

            GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", colony, value);
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
        public static bool GetBestColonyForSpying(Fleet fleet, out Colony result)
        {
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> spyShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsSpy);

            List<Colony> possibleColonies = GameContext.Current.Universe.Find<Colony>()
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

            if (possibleColonies.Count == 0)
            {
                GameLog.Client.AI.DebugFormat("Damn, no Home System of Empire found, possible colonies = {0}", possibleColonies.Count());
                result = null;
                return false;                
            }

            possibleColonies.Sort((a, b) =>
                (GetSpyingValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetSpyingValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            GameLog.Client.AI.DebugFormat("Yippy, Home System of Empire found!, possible colonies = {0}", possibleColonies.FirstOrDefault().Name);
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
            foreach(Sector sector in sectorList)
            {
                sectorList.Add(sector);
            }
            return sectorList;
        }
    }
}
