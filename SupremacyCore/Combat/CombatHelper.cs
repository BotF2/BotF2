// File:CombatHelper.cs
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
            Colony nearestFriendlySystem = GameContext.Current.Universe.FindNearestOwned<Colony>(
                assets.Location,
                assets.Owner);

            IEnumerable<Sector> sectors =
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
            Dictionary<Civilization, CombatAssets> assets = new Dictionary<Civilization, CombatAssets>();
            List<CombatAssets> results = new List<CombatAssets>();
            Dictionary<Civilization, CombatUnit> units = new Dictionary<Civilization, CombatUnit>();
            Sector sector = GameContext.Current.Universe.Map[location];
            List<Fleet> engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            TakeSidesAssets ExposedAssets = new TakeSidesAssets(location); // part of an altering of collection while using, in TakeSidesAssets.cs line 34 and to GameEngine,cs line 1288
            int maxOppostionScanStrength = ExposedAssets.MaxOppositionScanStrengh;
            List<Fleet> oppositionFleets = ExposedAssets.OppositionFleets;

            if ((oppositionFleets.Count == 0) && (sector.Station == null))
            {
                return results;
            }
            else
            {
                IEnumerable<Ship> _ships = from p in engagingFleets.SelectMany(l => l.Ships) select p;

                foreach (Ship ship in _ships.Distinct().ToList())
                {
                    CombatUnit unit = new CombatUnit(ship);
                    // works   GameLog.Core.Combat.DebugFormat("maxOppostionScanStrength =  {0}", maxOppostionScanStrength);
                    // GameLog.Core.Combat.DebugFormat("!ship! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);

                    // seems to be no difference between ship and unit
                    //GameLog.Core.Combat.DebugFormat("!unit! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);
                    if (ship.IsCamouflaged && (unit.CamouflagedStrength >= maxOppostionScanStrength))
                    {
                        //if (ship.ShipType.ToString() == "Spy" || ship.ShipType.ToString() == "Diplomatic")
                        //{
                        //    if (Ships.Where(sh => sh.OwnerID != 6).Any())
                        //        continue;
                        //}

                        continue; // skip over ships camaouflaged better than best scan strength
                    }
                    if (sector.System != null && ship.Owner != sector.Owner && sector.Owner != null && sector.System.Colony != null && GameContext.Current.Universe.HomeColonyLookup[sector.Owner] == sector.System.Colony && !DiplomacyHelper.AreAtWar(ship.Owner, sector.Owner))
                    {
                        //GameLog.Core.Combat.DebugFormat("Home Colony = {0}, Not at war ={1}",
                            //GameContext.Current.Universe.HomeColonyLookup[sector.Owner] == sector.System.Colony, !DiplomacyHelper.AreAtWar(ship.Owner, sector.Owner));
                        
                        continue; // for home worlds you need to declare war to get combat
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
                // needed once again for avoiding crash while finish a station build
                if (sector.Station.TurnCreated == GameContext.Current.TurnNumber || sector.Station.TurnCreated == 1)
                {
                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) just build in turn {3} and NOT taking part in this combat for avoid crashes on *uncomplete* stations"
                        , sector.Station.ObjectID, sector.Station.Name, sector.Station.Design, sector.Station.TurnCreated);
                    goto DoNotIncludeStationsNotFullyBuilded;
                }
                Civilization owner = sector.Station.Owner;

                if (!assets.ContainsKey(owner))
                {
                    assets[owner] = new CombatAssets(owner, location);
                }

                assets[owner].Station = new CombatUnit(sector.Station);

                DoNotIncludeStationsNotFullyBuilded:;
            }

            results.AddRange(assets.Values);

            return results;
        }

        public static List<CombatAssets> GetCamouflageAssets(MapLocation location)
        {
            Dictionary<Civilization, CombatAssets> assets = new Dictionary<Civilization, CombatAssets>();
            List<CombatAssets> results = new List<CombatAssets>();
            //var units = new Dictionary<Civilization, CombatUnit>();
            //var sector = GameContext.Current.Universe.Map[location];
            List<Fleet> engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            TakeSidesAssets ExposedAssets = new TakeSidesAssets(location);
            //var maxOppostionScanStrength = ExposedAssets.MaxOppositionScanStrengh;
            //var oppositionFleets = ExposedAssets.OppositionFleets;

            //if (sector.Station == null)
            //{
            //    return results;
            //}

            //else
            //{
            IEnumerable<Ship> _ships = from p in engagingFleets.SelectMany(l => l.Ships) select p;

            foreach (Ship ship in _ships.Distinct().ToList())
            {
                CombatUnit unit = new CombatUnit(ship);

                if (!ship.IsCamouflaged) // && (unit.CamouflagedStrength >= maxOppostionScanStrength))
                {
                    continue; // skip over ships not camaouflaged better than best scan strength
                }
                if (!assets.ContainsKey(ship.Owner))
                {
                    assets[ship.Owner] = new CombatAssets(ship.Owner, location);
                }
                if (ship.IsCombatant)
                {
                    assets[ship.Owner].CombatShips.Add(new CombatUnit(ship));
                }
                else
                {
                    assets[ship.Owner].NonCombatShips.Add(new CombatUnit(ship));
                }
                //}
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
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }
            if (firstCiv == secondCiv)
            {
                return false;
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                GameLog.Core.Combat.DebugFormat("no diplomacyData !! - WillEngage = FALSE");
                return true;
            }

            switch (diplomacyData.Status) // see WillFightAlongside below
            {
                case ForeignPowerStatus.Self:
                //case ForeignPowerStatus.Peace:
                case ForeignPowerStatus.Friendly:
                case ForeignPowerStatus.Affiliated:
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s is not at war
        /// each other in combat
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool AreNotAtWar(Civilization firstCiv, Civilization secondCiv)
        {
            bool notWar = true;
            if (firstCiv == null)
            {
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }
            if (firstCiv == secondCiv)
            {
                notWar = true;
            }
            // do not call GetTargetOne or Two here!, use in ChoseTarget
            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                //GameLog.Core.Combat.DebugFormat("no diplomacyData !! - WillEngage = FALSE");
                return true;
            }
            else if (diplomacyData.Status == ForeignPowerStatus.AtWar)
            {
                return false;
            }

            return notWar;
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
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                return false;
            }
            if (firstCiv != secondCiv)
            {
                switch (diplomacyData.Status)
                {
                    case ForeignPowerStatus.Affiliated:
                    case ForeignPowerStatus.Allied:
                    case ForeignPowerStatus.OwnerIsMember:
                    case ForeignPowerStatus.CounterpartyIsMember:
                        return true;
                }
            }
            // TODO: How should we handle war partners?

            return false;
        }

        public static CombatOrders GenerateBlanketOrders(CombatAssets assets, CombatOrder order)
        {
            const bool _generateBlanketOrdersTracing = true;
            Civilization owner = assets.Owner;
            CombatOrders orders = new CombatOrders(owner, assets.CombatID);

            foreach (CombatUnit ship in assets.CombatShips)  // CombatShips
            {
                orders.SetOrder(ship.Source, order);

                if (_generateBlanketOrdersTracing && order != CombatOrder.Hail) // reduces lines especially on starting (all ships starting with Hail)
                {
                    GameLog.Core.CombatDetails.DebugFormat("{0} {1} {2} is ordered to {3}",
                        ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                }
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                orders.SetOrder(ship.Source, (order == CombatOrder.Engage) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Rush) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Transports) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Formation) ? CombatOrder.Standby : order);
                //if (_generateBlanketOrdersTracing == true && order != CombatOrder.Hail)  // reduces lines especially on starting (all ships starting with Hail)
                //{
                //    //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) is ordered to {3}", ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                //}
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                orders.SetOrder(assets.Station.Source, (order == CombatOrder.Retreat) ? CombatOrder.Engage : order);
                //if (_generateBlanketOrdersTracing == true)
                //{
                //    //GameLog.Core.Combat.DebugFormat("{0} is ordered to {1}", assets.Station.Source, order);
                //}
            }

            return orders;
        }

        public static CombatTargetPrimaries GenerateBlanketTargetPrimary(CombatAssets assets, Civilization target) // the orbital and it's target civ from combat window
        {
            Civilization owner = assets.Owner;
            CombatTargetPrimaries targetOne = new CombatTargetPrimaries(owner, assets.CombatID);

            foreach (CombatUnit ship in assets.CombatShips)  // all CombatShips  of civ should get this target in the CombatTargetPrimaries dictionary
            {
                if (target.CivID == -1 || target == null)
                {
                    targetOne.SetTargetOneCiv(ship.Source, GetDefaultHoldFireCiv());
                    GameLog.Core.CombatDetails.DebugFormat("CombatAsset ship = {0} {1} Dummy Target = {2}", ship.Description, ship.Owner.Key, GetDefaultHoldFireCiv().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(ship.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("Combat ship = {0} {1} real Target = {2}", ship.Description, ship.Owner.Key, target.Key);
                }
                //GameLog.Core.CombatDetails.DebugFormat("Combat Ship  {0}: target = {2}", ship.Name, ship.Owner, target.Key);
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                if (target.CivID == -1)
                {
                    targetOne.SetTargetOneCiv(ship.Source, GetDefaultHoldFireCiv());
                    GameLog.Core.CombatDetails.DebugFormat("NonCombat ship = {0} {1} Dummy Target = {2}", ship.Description, ship.Owner.Key, GetDefaultHoldFireCiv().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(ship.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("NonCombat ship = {0} {1} Real Target = {2}", ship.Description, ship.Owner.Key, target.Key);
                }
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                if (target.CivID == -1)
                {
                    targetOne.SetTargetOneCiv(assets.Station.Source, GetDefaultHoldFireCiv());
                    GameLog.Core.CombatDetails.DebugFormat("Station = {0} {1} Dummy Target = {2}", assets.Station.Description, assets.Station.Owner.Key, GetDefaultHoldFireCiv().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(assets.Station.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("Station {0} {1} with Real target = {2}", assets.Station.Name, assets.Station.Owner.Key, target.Key);
                }
            }
            return targetOne;
        }

        public static CombatTargetSecondaries GenerateBlanketTargetSecondary(CombatAssets assets, Civilization target)
        {
            Civilization owner = assets.Owner;
            CombatTargetSecondaries targetTwo = new CombatTargetSecondaries(owner, assets.CombatID);

            foreach (CombatUnit ship in assets.CombatShips)  // all CombatShips get target
            {
                if (target.CivID == -1 || target == null) // UPDATE X 04 july 2019 manualy re-do update from ken, to fix targetTwo bug
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, GetDefaultHoldFireCiv());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, target);
                }

                GameLog.Core.CombatDetails.DebugFormat("Combat Ship {0} with target = {2}", ship.Name, ship.Owner, target.Key);
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                if (target.CivID == -1)
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, GetDefaultHoldFireCiv());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, target);
                }
            }
            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                if (target.CivID == -1)
                {
                    targetTwo.SetTargetTwoCiv(assets.Station.Source, GetDefaultHoldFireCiv());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(assets.Station.Source, target);
                }
            }
            return targetTwo;
        }

        public static double ComputeGroundDefenseMultiplier(Colony colony)
        {
            if (colony == null)
            {
                return 0;
            }

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
            StarSystem system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
            {
                return 0;
            }

            Colony colony = system.Colony;
            if (colony == null)
            {
                return 0;
            }

            int localGroundCombatBonus = 0;

            if (colony.OwnerID == civ.CivID)
            {
                localGroundCombatBonus = colony.Buildings
                    .Where(o => o.IsActive)
                    .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundCombat))
                    .Sum(b => b.Amount);
            }

            double raceMod = Math.Max(0.1, Math.Min(2.0, civ.Race.CombatEffectiveness));
            double weaponTechMod = 1.0 + (0.1 * GameContext.Current.CivilizationManagers[civ].Research.GetTechLevel(TechCategory.Weapons));
            double localGroundCombatMod = 1.0 + (0.01 * localGroundCombatBonus);

            double result = population * weaponTechMod * raceMod * localGroundCombatMod;

            GameLog.Core.Combat.DebugFormat("Colony = {5}: raceMod = {0}, weaponTechMod = {1}, localGroundCombatMod = {2}, population = {3}, result of GroundCombatStrength (in total) = {4} ", raceMod, weaponTechMod, localGroundCombatMod, population, result, colony.Name);

            return (int)result;
        }

        public static Civilization GetDefaultHoldFireCiv()
        {
             // The 'never clicked a target button' target civilizaiton for a human player so was it a hail order or an engage order?

            return new Civilization
            {
                ShortName = "Only Return Fire",
                CivID = 888, // CHANGE X PROBLEM this 778 will always be used for anyones TargetTWO. Bug.
                Key = "Only Return Fire"
            };
        }
    }
}

