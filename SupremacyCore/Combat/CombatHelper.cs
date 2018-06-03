// CombatHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    public static class CombatHelper
    {
        public static int CalculateOrbitalPower(Orbital orbital)
        {
            return (orbital.OrbitalDesign.PrimaryWeapon.Damage * orbital.OrbitalDesign.PrimaryWeapon.Count) +
                   (orbital.OrbitalDesign.SecondaryWeapon.Damage * orbital.OrbitalDesign.SecondaryWeapon.Count);
        }

        public static List<CombatAssets> GetCombatAssets(MapLocation location)
        {
            var assets = new Dictionary<Civilization, CombatAssets>();
            var results = new List<CombatAssets>();
            var sector = GameContext.Current.Universe.Map[location];

            var engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).Where(f => !f.IsCamouflaged).ToList();

            if ((engagingFleets.Count == 0) && (sector.Station == null))
            {
                return results;
            }

            foreach (var fleet in engagingFleets)
            {
                if (!assets.ContainsKey(fleet.Owner))
                {
                    assets[fleet.Owner] = new CombatAssets(fleet.Owner, location);
                }

                foreach (var ship in fleet.Ships)
                {
                    if (ship.IsCombatant)
                    {
                        assets[fleet.Owner].CombatShips.Add(new CombatUnit(ship));
                    }
                    else
                    {
                        assets[fleet.Owner].NonCombatShips.Add(new CombatUnit(ship));
                    }
                }
            }

            if (sector.Station != null)
            {
                var owner = sector.Station.Owner;

                if (!assets.ContainsKey(owner))
                    assets[owner] = new CombatAssets(owner, location);

                assets[owner].Station = new CombatUnit(sector.Station);
            }

            results.AddRange(assets.Values);

            return results;
        }

        public static bool WillEngage(Civilization firstCiv, Civilization secondCiv)
        {
            bool _willEngageTracing = false; // turn true if you want

            if (firstCiv == null)
                throw new ArgumentNullException("firstCiv");
            if (secondCiv == null)
                throw new ArgumentNullException("secondCiv");

            if (firstCiv == secondCiv)
                return false;
            //if (firstCiv.Race.ToString() != "Borg" && secondCiv.)
            //{ }
            var diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
                return false;

            if (_willEngageTracing == true)
                GameLog.Print("diplomacyData.Status={2} for {0} vs {1}, ", firstCiv, secondCiv, diplomacyData.Status.ToString());

            switch (diplomacyData.Status)
            {
                case ForeignPowerStatus.Affiliated:
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.Friendly:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                case ForeignPowerStatus.Peace:
                    return false;
            }

            return true;
        }

        public static bool WillFightAlongside(Civilization firstCiv, Civilization secondCiv)
        {
            if (firstCiv == null)
                throw new ArgumentNullException("firstCiv");
            if (secondCiv == null)
                throw new ArgumentNullException("secondCiv");

            var diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
                return false;

            switch (diplomacyData.Status)
            {
                case ForeignPowerStatus.Affiliated:
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
            var owner = assets.Owner;
            var orders = new CombatOrders(owner, assets.CombatID);

            bool _generateBlanketOrdersTracing = true;

            foreach (var ship in assets.CombatShips)  // CombatShips
            {
                orders.SetOrder(ship.Source, order);
                if (_generateBlanketOrdersTracing == true && order != CombatOrder.Hail) // reduces lines especially on starting (all ships starting with Hail)
                {
                    GameLog.Print("{0} {1} is ordered to {2}", ship.Source.ObjectID, ship.Source.Name, order);
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
                    GameLog.Print("{0} {1} is ordered to {2}", ship.Source.ObjectID, ship.Source.Name, order);
                }
                //orders.SetOrder(ship.Source, (order == CombatOrder.Rush) ? CombatOrder.Standby : order);
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                orders.SetOrder(assets.Station.Source, (order == CombatOrder.Retreat) ? CombatOrder.Engage : order);
                if (_generateBlanketOrdersTracing == true)
                {
                    GameLog.Print("{0} is ordered to {1}", assets.Station.Source, order);
                }
            }

            return orders;
        }

        public static double ComputeCombatOrderMultiplier(AutomatedCombatEngine automatedCombatEngine) // considering a way to get combat orders into outcome 
        {
            //if (colony == null)
                return 0;

            //GameLog.Print("GroundCombat?: Colony={0}, ComputeGroundDefenseMultiplier={1}",
            //    colony.Name,
            //    Math.Max(
            //    0.1,
            //    1.0 + (0.01 * colony.Buildings
            //                       .Where(o => o.IsActive)
            //                       .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
            //                       .Sum(b => b.Amount))));

            //return Math.Max(
            //    0.1,
            //    1.0 + (0.01 * colony.Buildings
            //                       .Where(o => o.IsActive)
            //                       .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
            //                       .Sum(b => b.Amount)));
        }

        public static double ComputeGroundDefenseMultiplier(Colony colony)
        {
            if (colony == null)
                return 0;

            GameLog.Print("GroundCombat?: Colony={0}, ComputeGroundDefenseMultiplier={1}",
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

            GameLog.Print("Colony = {5}: raceMod = {0}, weaponTechMod = {1}, localGroundCombatMod = {2}, population = {3}, result of GroundCombatStrength (in total) = {4} ", raceMod, weaponTechMod, localGroundCombatMod, population, result, colony.Name);

            return (int)result;
        }
    }
}
