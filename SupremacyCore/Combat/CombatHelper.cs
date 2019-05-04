// CombatHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Combat
{
    public static class CombatHelper
    {
        /// <summary>
        /// Calculates the best sector for the given <see cref="CombatAssets"/> to retreat to
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        public static Sector CalculateRetreatDestination(CombatAssets assets)
        {
            var nearestFriendlySystem = GameContext.Current.Universe.FindNearestOwned<Colony>(
                assets.Location,
                assets.Owner);

            var sectors =
                (
                    from s in assets.Sector.GetNeighbors()
                    let distance = MapLocation.GetDistance(s.Location, nearestFriendlySystem.Location)
                    let hostileOrbitals = GameContext.Current.Universe.FindAt<Orbital>(s.Location).Where(o => o.OwnerID != assets.OwnerID && o.IsCombatant)
                    let hostileOrbitalPower = hostileOrbitals.Sum(o => o.Firepower())
                    orderby hostileOrbitalPower ascending, distance descending
                    select s
                );

            return sectors.FirstOrDefault();
        }

        public static List<CombatAssets> GetCombatAssets(MapLocation location)
        {
            var assets = new Dictionary<Civilization, CombatAssets>();
            var results = new List<CombatAssets>();
            var units = new Dictionary<Civilization, CombatUnit>();
            var sector = GameContext.Current.Universe.Map[location];
            var engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            TakeSidesAssets ExposedAssets = new TakeSidesAssets(location);
            var maxOppostionScanStrength = ExposedAssets.MaxOppositionScanStrengh;
            var oppositionFleets = ExposedAssets.OppositionFleets;

            if ((oppositionFleets.Count == 0) && (sector.Station == null))
            {
                return results;
            }

            else
            {
                var _ships = from p in engagingFleets.SelectMany(l => l.Ships) select p;

                var Ships = _ships.Distinct().ToList();

                foreach (var ship in Ships)
                {
                    CombatUnit unit = new CombatUnit(ship);
                    // works   GameLog.Core.Combat.DebugFormat("maxOppostionScanStrength =  {0}", maxOppostionScanStrength);
                    // GameLog.Core.Combat.DebugFormat("!ship! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);

                    // seems to be no difference between ship and unit
                    //GameLog.Core.Combat.DebugFormat("!unit! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);
                    if ((ship.IsCamouflaged) && (unit.CamouflagedStrength >= maxOppostionScanStrength))
                    {
                        continue; // skip over ships camaouflaged better than best scan strength
                    }
                    if (!assets.ContainsKey(ship.Owner))
                    {
                        assets[ship.Owner] = new CombatAssets(ship.Owner, location);
                    }
                    if (ship.IsCombatant)
                    {
                        assets[ship.Owner].CombatShips.Add(new CombatUnit(ship));

                        if (ship.IsCamouflaged)
                        {
                            unit.Decamouflage();
                            ship.IsCamouflaged = false; // do we need an updater here to unit.Decamouflage() reset ship.IsCamouflaged? - so far it does not appear to do this in the GameLog below.

                            GameContext.Current.CivilizationManagers[ship.Owner].SitRepEntries.Add(new DeCamouflagedSitRepEntry(ship, maxOppostionScanStrength));
                            GameLog.Core.Combat.DebugFormat("CombatShip Decamouflage - max scan ={0}, unit Camouflage = {1} for {2} {3} {4} at {5} Is Camouflaged? {6}",
                                maxOppostionScanStrength, unit.CamouflagedStrength, unit.Source.ObjectID, unit.Source.Name, unit.Source.Design, location.ToString(), ship.IsCamouflaged.ToString());
                        }

                    }
                    else
                    {
                        assets[ship.Owner].NonCombatShips.Add(new CombatUnit(ship));

                        if (ship.IsCamouflaged)
                        {
                            unit.Decamouflage();
                            ship.IsCamouflaged = false;

                            GameContext.Current.CivilizationManagers[ship.Owner].SitRepEntries.Add(new DeCamouflagedSitRepEntry(ship, maxOppostionScanStrength));
                            GameLog.Core.Combat.DebugFormat("NonCombatShip - max scan ={0}, unit Camouflage ={1} for{2} {3} {4} at {5}",
                                    maxOppostionScanStrength, unit.CamouflagedStrength, unit.Source.ObjectID, unit.Source.Name, unit.Source.Design, location.ToString());
                        }

                    }
                }
            }
            if (sector.Station != null)
            {
                var owner = sector.Station.Owner;

                if (!assets.ContainsKey(owner))
                {
                    assets[owner] = new CombatAssets(owner, location);
                }

                assets[owner].Station = new CombatUnit(sector.Station);
            }

            results.AddRange(assets.Values);

            return results;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s will enage
        /// each other in combat
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool WillEngage(Civilization firstCiv, Civilization secondCiv)
        {

            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }
            if (firstCiv == secondCiv)
            {
                return false;
            }
            // do not call GetTargetOne or Two here!, use in ChoseTarget
            var diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                GameLog.Core.Combat.DebugFormat("no diplomacyData !! - WillEngage = FALSE");
                return false;
            }

            switch (diplomacyData.Status) // see WillFightAlongside below
            {
                //case ForeignPowerStatus.Peace:
                //case ForeignPowerStatus.Friendly:
                //case ForeignPowerStatus.Affiliated:  //try this diplomatic level for not opening the combat window
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s will
        /// fight alongside each other
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool WillFightAlongside(Civilization firstCiv, Civilization secondCiv)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            var diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                return false;
            }

            switch (diplomacyData.Status)
            {
                //case ForeignPowerStatus.Affiliated:
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                    return true;
            }

            // TODO: How should we handle war partners?

            return false;
        }

        public static CombatOrders GenerateBlanketOrders(CombatAssets assets, CombatOrder order)
        {
            bool _generateBlanketOrdersTracing = true;
            var owner = assets.Owner;
            var orders = new CombatOrders(owner, assets.CombatID);

            foreach (var ship in assets.CombatShips)  // CombatShips
            {
                orders.SetOrder(ship.Source, order);

                if (_generateBlanketOrdersTracing == true && order != CombatOrder.Hail) // reduces lines especially on starting (all ships starting with Hail)
                {
                    GameLog.Core.CombatDetails.DebugFormat("{0} {1} ({2}) is ordered to {3}, primary target civ ={4}, secondary target civ ={5}",
                        ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                }
            }

            foreach (var ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                orders.SetOrder(ship.Source, (order == CombatOrder.Engage) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Rush) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Transports) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Formation) ? CombatOrder.Standby : order);
                if (_generateBlanketOrdersTracing == true && order != CombatOrder.Hail)  // reduces lines especially on starting (all ships starting with Hail)
                {
                    //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) is ordered to {3}", ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                }
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                orders.SetOrder(assets.Station.Source, (order == CombatOrder.Retreat) ? CombatOrder.Engage : order);
                if (_generateBlanketOrdersTracing == true)
                {
                    //GameLog.Core.Combat.DebugFormat("{0} is ordered to {1}", assets.Station.Source, order);
                }
            }

            return orders;
        }

        public static CombatTargetPrimaries GenerateTargetPrimary(CombatAssets assets, Civilization target)
        {

            //bool _generateTargetPrimariesTracing = true; 
            var owner = assets.Owner;
            var targetOne = new CombatTargetPrimaries(owner, assets.CombatID);
            //Civilization borg = new Civilization("BORG");
            //Civilization cards = new Civilization("CARDASSIANS");
            //Civilization terrans = new Civilization("TERRANEMPIRE");

            if (target == null)
            {
                GameLog.Core.Test.DebugFormat("GenerateTargetPrimary: target == null #######");
                return targetOne;
            }

            foreach (var ship in assets.CombatShips)  // CombatShips
            {


                //GameLog.Core.Test.DebugFormat("GenerateTargetPrimary: Combat Ship {0} with target = {1}", ship.Name, ship.Owner, target.Name.ToString());
                //targetOne.SetTargetOne(ship.Source, (target == cards) ? cards : target);
                targetOne.SetTargetOne(ship.Source, target);
                GameLog.Core.Test.DebugFormat("GenerateTargetPrimary: Combat Ship {1} - {0} with target = {2}", ship.Name, ship.Owner, target.Key);
                //, target.Name.ToString()
            }

            foreach (var ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                targetOne.SetTargetOne(ship.Source, target);

                GameLog.Core.Test.DebugFormat("GenerateTargetPrimary: Non Combat Ship {0} with target = {1}", ship.Name, target.Key);
                //targetOne.SetTargetOne(ship.Source, (target == cards) ? cards : target);
                //targetOne.SetTargetOne(ship.Source, (target == terrans) ? terrans : target);
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                targetOne.SetTargetOne(assets.Station.Source, target);
                GameLog.Core.Test.DebugFormat("GenerateTargetPrimary: Station {0} with target = {1}", assets.Station.Name, target.Key);
            }

            //GameLog.Core.Test.DebugFormat("GenerateTargetPrimary targets Onwer = {0}, (shooting)Assets.Owner ={1}, target civ = {2}",
            //    targetOne.Owner, owner, target);
            return targetOne;
        }

        public static CombatTargetSecondaries GenerateTargetSecondary(CombatAssets assets, Civilization target)
        {
            // bool _generateTargetSecondaryTracing = true;
            var owner = assets.Owner;

            var targetTwo = new CombatTargetSecondaries(owner, assets.CombatID);
            foreach (var ship in assets.CombatShips)  // CombatShips
            {
                targetTwo.SetTargetTwo(ship.Source, target);
                //targetTwo.Distinct().ToList();
            }

            foreach (var ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                targetTwo.SetTargetTwo(ship.Source, target);
                //targetTwo.Distinct().ToList();
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                targetTwo.SetTargetTwo(assets.Station.Source, target);
                //targetTwo.Distinct().ToList();
            }
            return targetTwo;
        }

        public static double ComputeGroundDefenseMultiplier(Colony colony)
        {
            if (colony == null)
                return 0;

            GameLog.Core.Combat.DebugFormat("Colony={0}, ComputeGroundDefenseMultiplier={1}",
                colony.Name,
                Math.Max(
                0.1,
                1.0 + (0.01 * colony.Buildings
                                   .Where(o => o.IsActive)
                                   .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
                                   .Sum(b => b.Amount))));

            return Math.Max(
                0.1,
                1.0 + (0.01 * colony.Buildings
                                   .Where(o => o.IsActive)
                                   .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
                                   .Sum(b => b.Amount)));
        }

        public static int ComputeGroundCombatStrength(Civilization civ, MapLocation location, int population)
        {
            var system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
                return 0;

            var colony = system.Colony;
            if (colony == null)
                return 0;

            var localGroundCombatBonus = 0;

            if (colony.OwnerID == civ.CivID)
            {
                localGroundCombatBonus = colony.Buildings
                    .Where(o => o.IsActive)
                    .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundCombat))
                    .Sum(b => b.Amount);
            }

            var raceMod = Math.Max(0.1, Math.Min(2.0, civ.Race.CombatEffectiveness));
            var weaponTechMod = 1.0 + (0.1 * GameContext.Current.CivilizationManagers[civ].Research.GetTechLevel(TechCategory.Weapons));
            var localGroundCombatMod = 1.0 + (0.01 * localGroundCombatBonus);

            var result = population * weaponTechMod * raceMod * localGroundCombatMod;

            GameLog.Core.Combat.DebugFormat("Colony = {5}: raceMod = {0}, weaponTechMod = {1}, localGroundCombatMod = {2}, population = {3}, result of GroundCombatStrength (in total) = {4} ", raceMod, weaponTechMod, localGroundCombatMod, population, result, colony.Name);

            return (int)result;
        }

        internal static object GenerateTargetSecondary(CombatAssets playerAssets, object theTargetCiv)
        {
            throw new NotImplementedException();
        }
    }
}
