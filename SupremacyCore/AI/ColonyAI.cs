// ColonyAI.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.AI
{
    public static class ColonyAI
    {
        private const int NumScouts = 2;
        private const int ColonyShipEveryTurns = 20;
        private const int ColonyShipEveryTurnsMinor = 30;
        private const int MaxMinorColonyCount = 3;
        private const int MaxEmpireColonyCount = 6;

        public static void DoTurn([NotNull] Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            foreach (var colony in GameContext.Current.Universe.FindOwned<Colony>(civ.CivID))
            {
                HandleEnergyProduction(colony);
                HandleFoodProduction(colony);
                HandleBuildings(colony, civ);
                colony.ProcessQueue();
                HandleBuyBuild(colony, civ);
                HandleIndustryProduction(colony);
                HandleLabors(colony);
                if (!PlayerAI.IsInFinancialTrouble(civ))
                {
                    if (civ.IsEmpire)
                        HandleShipProductionEmpire(colony, civ);
                    else
                        HandleShipProductionMinor(colony, civ);
                }
            }
        }

        private static void SetFacility(Colony colony, ProductionCategory category, int netProd, double output, IEnumerable<ProductionCategory> otherCategories)
        {
            var reserveFacility = Math.Floor(netProd / output);
            reserveFacility = Math.Max(reserveFacility, -(colony.TotalFacilities[category].Value - colony.GetActiveFacilities(category)));
            reserveFacility = Math.Min(reserveFacility, colony.GetActiveFacilities(category));
            var labors = colony.GetAvailableLabor() / colony.GetFacilityType(category).LaborCost;
            while (reserveFacility < 0 && labors > 0)
            {
                colony.ActivateFacility(category);
                reserveFacility++;
                labors--;
            }
            foreach (var c in otherCategories)
            {
                while (reserveFacility < 0 && colony.GetActiveFacilities(c) > 0)
                {
                    colony.DeactivateFacility(c);
                    colony.ActivateFacility(category);
                    reserveFacility++;
                }
            }

            // deactivate not needed
            for (int i = 0; i < reserveFacility; i++)
            {
                colony.DeactivateFacility(category);
            }
        }

        private static void HandleEnergyProduction(Colony colony)
        {
            var energyOutput = colony.GetFacilityType(ProductionCategory.Energy).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Energy).Efficiency);
            var offlineBuilding = colony.Buildings.Where(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0).ToList();
            var offlineShipyardSlots = colony.Shipyard == null ? new List<ShipyardBuildSlot>() : colony.Shipyard.BuildSlots.Where(s => !s.IsActive).ToList();
            var netEnergy = colony.NetEnergy - offlineBuilding.Sum(b => b.BuildingDesign.EnergyCost) - offlineShipyardSlots.Sum(s => s.Shipyard.ShipyardDesign.BuildSlotEnergyCost);
            SetFacility(colony, ProductionCategory.Energy, netEnergy, energyOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry, ProductionCategory.Food });

            // turn things on
            foreach (var building in offlineBuilding)
            {
                colony.ActivateBuilding(building);
            }
            foreach (var slot in offlineShipyardSlots)
            {
                colony.ActivateShipyardBuildSlot(slot);
            }

            var facilityType = colony.GetFacilityType(ProductionCategory.Energy);
            if ((colony.Buildings.Any(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0)  || colony.Shipyard != null && colony.Shipyard.BuildSlots.Any(s => !s.IsActive)) && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));
            }
        }

        private static void HandleFoodProduction(Colony colony)
        {
            var foodOutput = colony.GetFacilityType(ProductionCategory.Food).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Food).Efficiency);
            var neededFood = colony.NetFood + colony.FoodReserves.CurrentValue - 10 * foodOutput;
            SetFacility(colony, ProductionCategory.Food, (int)neededFood, foodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry });
            neededFood = colony.Population.CurrentValue + foodOutput;
            var maxFoodProduction = colony.GetProductionModifier(ProductionCategory.Food).Bonus + colony.GetTotalFacilities(ProductionCategory.Food) * foodOutput;
            var facilityType = colony.GetFacilityType(ProductionCategory.Food);
            if (maxFoodProduction < neededFood && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));
            }
        }

        private static void HandleIndustryProduction(Colony colony)
        {
            var prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
            var maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
            var industryNeeded = colony.BuildSlots.Where(s => s.Project != null).Select(s => s.Project.IsRushed ? 0 : s.Project.GetCurrentIndustryCost()).Sum();
            var turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + maxProdFacility * prodOutput));
            var facilityNeeded = turnsNeeded == 0 ? 0 : Math.Truncate((industryNeeded / turnsNeeded - colony.GetProductionModifier(ProductionCategory.Industry).Bonus) / prodOutput);
            var netIndustry = -(facilityNeeded - colony.ActiveFacilities[ProductionCategory.Industry].Value) * prodOutput;
            SetFacility(colony, ProductionCategory.Industry, (int)netIndustry, prodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research });
        }

        private static void HandleLabors(Colony colony)
        {
            while (colony.ActivateFacility(ProductionCategory.Research)) { }
            while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.ActivateFacility(ProductionCategory.Food)) { }
            while (colony.ActivateFacility(ProductionCategory.Industry)) { }
        }

        private static void HandleBuildings(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
            {
                var project = TechTreeHelper.GetBuildProjects(colony).FirstOrDefault(bp => bp.BuildDesign is ShipyardDesign);
                if (colony == GameContext.Current.Universe.HomeColonyLookup[civ] && project != null && !colony.IsBuilding(project.BuildDesign))
                {
                    colony.BuildQueue.Add(new BuildQueueItem(project));
                }
            }

            colony.ProcessQueue();

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                var prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
                var manager = GameContext.Current.CivilizationManagers[civ];
                var availableResources = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);

                var structureProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)) <= 5.0)
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                }
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                var upgradeIndustryProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<ProductionFacilityUpgradeProject>()
                    .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));
                if (upgradeIndustryProject != null)
                {
                    colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                }
            }


            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                var flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                var flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                if (flexLabors > 0)
                {
                    if (colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                    }
                    else
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                    }
                }
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                var projects = TechTreeHelper.GetBuildProjects(colony);
            }
        }

        private static void HandleBuyBuild(Colony colony, Civilization civ)
        {
            CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];
            colony.BuildSlots.Where(s => s.Project != null && !s.Project.IsRushed).ToList().ForEach(s =>
            {
                var otherProjects = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os != s && os.Project != null)
                    .Select(os => os.Project)
                    .Where(p => p.GetTimeEstimate() <= 1 || p.IsRushed)
                    .ToList();

                var availableResources = otherProjects
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);

                var cost = otherProjects
                    .Where(p => p.IsRushed)
                    .Select(p => p.GetCurrentIndustryCost())
                    .DefaultIfEmpty()
                    .Sum();

                if (EnumHelper.GetValues<ResourceType>().Where(availableResources.ContainsKey).All(r => availableResources[r] >= s.Project.GetCurrentResourceCost(r))
                    && ((manager.Credits.CurrentValue - cost) * 0.2 > s.Project.GetCurrentIndustryCost()))
                {
                    var prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
                    var maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
                    var industryNeeded = colony.BuildSlots.Where(bs => bs.Project != null).Select(bs => bs.Project.IsRushed ? 0 : bs.Project.GetCurrentIndustryCost()).Sum();
                    var turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + maxProdFacility * prodOutput));
                    if (turnsNeeded > 0)
                    {
                        s.Project.IsRushed = true;
                        while (colony.DeactivateFacility(ProductionCategory.Industry)) {}
                    }
                }
            });
        }

        private static void HandleShipProductionEmpire(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
                return;

            var potentialProjects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            var shipDesigns = GameContext.Current.TechTrees[colony.OwnerID].ShipDesigns.ToList();
            var fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList();
            var homeSector = GameContext.Current.Universe.HomeColonyLookup[civ].Sector;
            var homeFleets = homeSector.GetOwnedFleets(civ).ToList();

            if (colony.Sector == homeSector)
            {
                // Exploration
                if (!shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(colony.Shipyard.IsBuilding))
                {
                    for (int i = fleets.Count(o => o.IsScout); i < NumScouts; i++)
                    {
                        var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
                        if (project != null)
                        {
                            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        }
                    }
                }
                // Colonization
                if (GameContext.Current.Universe.FindOwned<Colony>(civ).Count < MaxEmpireColonyCount && 
                    GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Construction
                if (colony.Sector.Station == null &&
                    colony.Sector.GetOwnedFleets(civ).All(o => !o.IsConstructor) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Military
                var defenseFleet = homeSector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                if ((defenseFleet == null || !defenseFleet.HasCommandShip) &&
                    homeFleets.All(o => !o.HasCommandShip) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Command).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Command && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                if ((defenseFleet == null || defenseFleet.Ships.Count < 5) &&
                    homeFleets.Where(o => o.IsBattleFleet).Sum(o => o.Ships.Count) < 5 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                    if (project != null)
                    {
                        project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    }
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
            }
            if (colony.Shipyard.BuildSlots.All(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                var projects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            }

            colony.Shipyard.ProcessQueue();
        }

        private static void HandleShipProductionMinor(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
                return;

            var potentialProjects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            var shipDesigns = GameContext.Current.TechTrees[colony.OwnerID].ShipDesigns.ToArray();
            //var fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ);
            var homeSector = GameContext.Current.Universe.HomeColonyLookup[civ].Sector;

            if (colony.Sector == homeSector)
            {
                // Exploration
                //if (!shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(colony.Shipyard.IsBuilding))
                //{
                //    for (int i = fleets.Count(o => o.IsScout); i < NumScouts; i++)
                //    {
                //        var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
                //        if (project != null)
                //        {
                //            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                //        }
                //    }
                //}
                // Colonization
                if (civ.CivilizationType == CivilizationType.ExpandingPower &&
                    GameContext.Current.Universe.FindOwned<Colony>(civ).Count < MaxMinorColonyCount &&
                    GameContext.Current.TurnNumber % ColonyShipEveryTurnsMinor == 0 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Construction
                if (civ.CivilizationType == CivilizationType.ExpandingPower &&
                    colony.Sector.Station == null &&
                    colony.Sector.GetOwnedFleets(civ).All(o => !o.IsConstructor) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(colony.Shipyard.IsBuilding))
                {
                    var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Military
                var defenseFleet = homeSector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                if (civ.CivilizationType != CivilizationType.MinorPower)
                {
                    if ((defenseFleet == null || defenseFleet.Ships.Count < 2) &&
                        !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(colony.Shipyard.IsBuilding))
                    {
                        var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                        if (project != null)
                        {
                            project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                        }
                        if (project != null)
                        {
                            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        }
                    }
                }
                else
                {
                    if ((defenseFleet == null || defenseFleet.Ships.Count < 2) &&
                        !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack).Any(colony.Shipyard.IsBuilding))
                    {
                        var project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                        if (project != null)
                        {
                            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        }
                    }
                }
            }

            if (colony.Shipyard.BuildSlots.All(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                var projects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            }

            colony.Shipyard.ProcessQueue();
         }    
    }
}