// ColonyAI.cs
//
// Copyright (c) 2008 Mike Strobel
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
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.AI
{
    public static class ColonyAI
    {
        private const int NumScouts = 1;
        //private const int ColonyShipEveryTurns = 2;
        private const int ColonyShipEveryTurnsMinor = 5;
        private const int MaxMinorColonyCount = 3;

        //private static string _text;
        private static int neededColonizer;
        private const int MaxEmpireColonyCount = 999; // currently not used

        private static bool need1Colonizer;
        [NonSerialized]
        private static string _text;
        private static readonly string blank = " ";


        public static void DoTurn([NotNull] Civilization civ)
        {
            _text = "Step_1100:; ColonyAI.DoTurn begins..."

                    ;
            Console.WriteLine(_text);

            if (civ == null)
            {
                need1Colonizer = false;// dummy > just keep
                _text = need1Colonizer.ToString();
                throw new ArgumentNullException(nameof(civ));
            }


            foreach (Colony colony in GameContext.Current.Universe.FindOwned<Colony>(civ.CivID))
            {
                _text = "Step_1100:; " + colony.Location + "; " + colony.Name + " > Handling colony"
                    + " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues >"
                    ;
                _text += blank; // dummy - please keep
                Console.WriteLine(_text);

                if (MaxEmpireColonyCount == 999)
                    _text = "";//nothing - just for dummy

                if (colony.BuildQueue.Count > 0)
                {
                    _text = "Step_1104:; HandleBasicStructures on; "
                            + colony.Name + "; " + colony.Owner
                            + "; already bulding; " + colony.BuildQueue[0].Project.Description;
                    ;
                    Console.WriteLine(_text);
                }
                else
                {
                    _text = "Step_1106:; HandleBasicStructures on; "
                            + colony.Name + "; " + colony.Owner
                            + "; BuildQueue empty";
                    ;
                    Console.WriteLine(_text);
                }

                CheckPopulation(colony);
                HandleEnergyProduction(colony);
                HandleFoodProduction(colony);
                HandleBasicStructures(colony, civ);
                HandleBuildings(colony, civ);
                HandleAdditionalStructures(colony, civ);
                HandleUpgrades(colony, civ);
                colony.ProcessQueue();
                HandleBuyBuild(colony, civ);
                HandleIndustryProduction(colony);
                HandleLabors(colony);
                if (!PlayerAI.IsInFinancialTrouble(civ))
                {
                    if (civ.IsEmpire)
                    {
                        HandleShipProductionEmpire(colony, civ);
                    }
                    else
                    {
                        HandleShipProductionMinor(colony, civ);
                    }
                }
            }
            _text = "Step_1100:; ColonyAI.DoTurn is done..."

        ;
            Console.WriteLine(_text);
        }

        private static void SetFacility(Colony colony, ProductionCategory category, int netProd, double output, IEnumerable<ProductionCategory> otherCategories)
        {
            double reserveFacility = Math.Floor(netProd / output);
            reserveFacility = Math.Max(reserveFacility, -(colony.TotalFacilities[category].Value - colony.GetActiveFacilities(category)));
            reserveFacility = Math.Min(reserveFacility, colony.GetActiveFacilities(category));
            int labors = colony.GetAvailableLabor() / colony.GetFacilityType(category).LaborCost;
            while (reserveFacility < 0 && labors > 0)
            {
                _ = colony.ActivateFacility(category);
                reserveFacility++;
                labors--;
            }
            foreach (ProductionCategory c in otherCategories)
            {
                while (reserveFacility < 0 && colony.GetActiveFacilities(c) > 0)
                {
                    _ = colony.DeactivateFacility(c);
                    _ = colony.ActivateFacility(category);
                    reserveFacility++;
                }
            }

            // deactivate not needed
            for (int i = 0; i < reserveFacility; i++)
            {
                _ = colony.DeactivateFacility(category);
            }
        }

        private static void CheckPopulation(Colony colony)
        {
            int _popAvailable = colony.Population.CurrentValue / 10;

            int _tmp_Active1_Food = colony.Facilities_Active1_Food;
            int _tmp_Active2_Industry = colony.Facilities_Active2_Industry;
            int _tmp_Active3_Energy = colony.Facilities_Active3_Energy;
            int _tmp_Active4_Research = colony.Facilities_Active4_Research;
            int _tmp_Active5_Intelligence = colony.Facilities_Active5_Intelligence;

            int _laborPool = colony.GetAvailableLabor() / 10;

            _text = "Step_2345:; Pop= " + _popAvailable
                + ", Active: Food= " + _tmp_Active1_Food
                + ", Ind= " + _tmp_Active2_Industry
                + ", En= " + _tmp_Active3_Energy
                + ", Res= " + _tmp_Active4_Research
                + ", Int= " + _tmp_Active5_Intelligence
                + ", Pool= " + _laborPool
                + " for " + colony.Name
                ;
            Console.WriteLine(_text);

            while (colony.DeactivateFacility(ProductionCategory.Industry)) { }
            while (colony.DeactivateFacility(ProductionCategory.Research)) { }
            while (colony.DeactivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.DeactivateFacility(ProductionCategory.Food)) { }
            while (colony.DeactivateFacility(ProductionCategory.Energy)) { }

            _laborPool = colony.GetAvailableLabor() / 10;

            _text = "Step_2347:; Pop= " + _popAvailable
                    + ", Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
                    + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
                    + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
                    + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
                    + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
                    + ", Pool= " + _laborPool
                    + " for " + colony.Name
                    ;
            Console.WriteLine(_text);

            //checked what's going on here setting _popAvai to Zero'
        

            for (int i = 0; i < colony.Facilities_Total3_Energy; i++)
            {
                while (_popAvailable > colony.Facilities_Total3_Energy)
                {
                    colony.ActivateFacility(ProductionCategory.Energy);
                    _popAvailable -= 1;
                }
            }
            HandleEnergyProduction(colony);


            while (_popAvailable > 0 && colony.FoodReserves.CurrentValue > 1000 && colony.NetFood < -10)
            {
                colony.ActivateFacility(ProductionCategory.Food);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active4_Research < _tmp_Active4_Research)
            {
                colony.ActivateFacility(ProductionCategory.Research);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active5_Intelligence < _tmp_Active5_Intelligence)
            {
                colony.ActivateFacility(ProductionCategory.Intelligence);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active2_Industry < colony.Facilities_Total2_Industry)
            {
                colony.ActivateFacility(ProductionCategory.Industry);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active4_Research < colony.Facilities_Total4_Research)
            {
                colony.ActivateFacility(ProductionCategory.Research);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active5_Intelligence < colony.Facilities_Total5_Intelligence)
            {
                colony.ActivateFacility(ProductionCategory.Intelligence);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && colony.Facilities_Active1_Food < colony.Facilities_Total1_Food)
            {
                colony.ActivateFacility(ProductionCategory.Food);
                _popAvailable -= 1;
            }

            // don't fill up energy - put to labor pool instead

            //while (_popAvailable > 1 && colony.Facilities_Active3_Energy < colony.Facilities_Total3_Energy)
            //{
            //    colony.ActivateFacility(ProductionCategory.Energy);
            //    _popAvailable -= 1;
            //}

            _laborPool = colony.GetAvailableLabor() / 10;

            _text = "Step_2348:; Pop= " + _popAvailable
                    + ", Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
                    + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
                    + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
                    + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
                    + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
                    + ", Pool= " + _laborPool
                    + " for " + colony.Name
                    ;
            Console.WriteLine(_text);

            //while (colony.ActivateFacility(ProductionCategory.Industry)) { }
            //while (colony.ActivateFacility(ProductionCategory.Research)) { }
            //while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
            //while (colony.ActivateFacility(ProductionCategory.Food)) { }

        }

        private static void HandleEnergyProduction(Colony colony)
        {
            _text = "Step_1218:; Handle ENERGY on; "
                    + colony.Name + "; " + colony.Owner
                    //+ " > no Upgrade INDUSTRY"
                    ;
            Console.WriteLine(_text);

            double energyOutput = colony.GetFacilityType(ProductionCategory.Energy).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Energy).Efficiency);
            List<Buildings.Building> offlineBuilding = colony.Buildings.Where(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0).ToList();
            List<ShipyardBuildSlot> offlineShipyardSlots = colony.Shipyard == null ? new List<ShipyardBuildSlot>() : colony.Shipyard.BuildSlots.Where(s => !s.IsActive).ToList();
            int netEnergy = colony.NetEnergy - offlineBuilding.Sum(b => b.BuildingDesign.EnergyCost) - offlineShipyardSlots.Sum(s => s.Shipyard.ShipyardDesign.BuildSlotEnergyCost);
            SetFacility(colony, ProductionCategory.Energy, netEnergy, energyOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry, ProductionCategory.Food });

            // turn things on
            foreach (Buildings.Building building in offlineBuilding)
            {
                _ = colony.ActivateBuilding(building);
            }
            foreach (ShipyardBuildSlot slot in offlineShipyardSlots)
            {
                _ = colony.ActivateShipyardBuildSlot(slot);
            }

            ProductionFacilityDesign facilityType = colony.GetFacilityType(ProductionCategory.Energy);
            if ((colony.Buildings.Any(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0) || (colony.Shipyard?.BuildSlots.Any(s => !s.IsActive) == true)) && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));
                _text = "Step_1219:; HandleBuildings on "
                    + colony.Name + " " + colony.Owner
                    + " > added 1 ENERGY Facility Build Order"
                    ;
                Console.WriteLine(_text);
            }
        }

        private static void HandleFoodProduction(Colony colony)
        {
            _text = "Step_1220:; Handle FOOD on; "
                    + colony.Name + "; " + colony.Owner
                    //+ " > no Upgrade INDUSTRY"
                    ;
            Console.WriteLine(_text);

            double foodOutput = colony.GetFacilityType(ProductionCategory.Food).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Food).Efficiency);
            double neededFood = colony.NetFood + colony.FoodReserves.CurrentValue - (10 * foodOutput);
            SetFacility(colony, ProductionCategory.Food, (int)neededFood, foodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry });
            neededFood = colony.Population.CurrentValue + foodOutput;
            double maxFoodProduction = colony.GetProductionModifier(ProductionCategory.Food).Bonus + (colony.GetTotalFacilities(ProductionCategory.Food) * foodOutput);
            ProductionFacilityDesign facilityType = colony.GetFacilityType(ProductionCategory.Food);
            if (maxFoodProduction < neededFood && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));
                _text = "Step_1228: HandleFoodProduction on "
                    + colony.Name + " " + colony.Owner
                    + " > added 1 FOOD Facility Build Order"
                    ;
                Console.WriteLine(_text);
            }
        }

        private static void HandleIndustryProduction(Colony colony)
        {
            double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
            int maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, (colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost) + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
            int industryNeeded = colony.BuildSlots.Where(s => s.Project != null).Select(s => s.Project.IsRushed ? 0 : s.Project.GetCurrentIndustryCost()).Sum();
            int turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));
            double facilityNeeded = turnsNeeded == 0 ? 0 : Math.Truncate(((industryNeeded / turnsNeeded) - colony.GetProductionModifier(ProductionCategory.Industry).Bonus) / prodOutput);
            double netIndustry = -(facilityNeeded - colony.ActiveFacilities[ProductionCategory.Industry].Value) * prodOutput;
            SetFacility(colony, ProductionCategory.Industry, (int)netIndustry, prodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research });
        }

        private static void HandleLabors(Colony colony)
        {
            while (colony.ActivateFacility(ProductionCategory.Industry)) { }
            while (colony.ActivateFacility(ProductionCategory.Research)) { }
            while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.ActivateFacility(ProductionCategory.Food)) { }

        }

        private static void HandleBuildings(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
            {
                BuildProject project = TechTreeHelper.GetBuildProjects(colony).FirstOrDefault(bp => bp.BuildDesign is ShipyardDesign);
                if (colony == GameContext.Current.Universe.HomeColonyLookup[civ] && project != null && !colony.IsBuilding(project.BuildDesign))
                {
                    colony.BuildQueue.Add(new BuildQueueItem(project));
                }
            }

            colony.ProcessQueue();

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                _text = "Step_1202:; HandleBuildings: "
                    //+ "Credits.Current= " + manager.Credits.CurrentValue
                    //+ ", Costs= " + cost
                    //+ ", industryNeeded= " + industryNeeded
                    //+ ", prodOutput= " + prodOutput.ToString()
                    //+ ", turnsNeeded= " + turnsNeeded
                    //+ " > IsRushed for " + s.Project
                    + " on " + colony.Name + " " + colony.Owner
                ;
                Console.WriteLine(_text);

                //if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                if (colony.BuildQueue.Count == 0)
                {
                    //INDUSTRY 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry };
                    int flexLabors = colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > -21)  // 2 more facilites as available labors
                    {
                        _text = "Step_1204:; HandleBuildings on INDUSTRY at "
                            + colony.Name + " " + colony.Owner
                            + " > " + colony.GetAvailableLabor() + " labors available"
                            ;
                        Console.WriteLine(_text);

                        int totalInd = colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1205: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Industry Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1206: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Industry Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                {
                    //FOOD 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Food };
                    int flexLabors = colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                    {
                        _text = "Step_1204: HandleBuildings on INDUSTRY at "
                            + colony.Name + " " + colony.Owner
                            + " > " + colony.GetAvailableLabor() + " labors available"
                            ;
                        Console.WriteLine(_text);

                        int totalInd = colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1205: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Research Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1206: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Industry Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                {
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                    int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > 0)
                    {
                        _text = "Step_1204: HandleBuildings on "
                            + colony.Name + " " + colony.Owner
                            + " " + colony.GetAvailableLabor() + " > flexLabors available"
                            ;
                        Console.WriteLine(_text);
                        if (colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1205: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Research Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                            _text = "Step_1206: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > added 1 Research Facility Build Order"
                                ;
                            Console.WriteLine(_text);
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                {
                    // Industry Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));
                    if (upgradeIndustryProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                    }
                    else
                    {
                        _text = "Step_1203: HandleBuildings on "
                                + colony.Name + " " + colony.Owner
                                + " > no Upgrade INDUSTRY"
                                ;
                        Console.WriteLine(_text);
                    }
                }

                ////structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(colony)
                //    .OfType<StructureBuildProject>()
                //    .Where(p =>
                //            p.GetCurrentIndustryCost() > 0
                //            && EnumHelper
                //                .GetValues<ResourceType>()
                //                .Where(availableResources.ContainsKey)
                //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                //{
                //    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //}
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);
                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                }
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
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
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
            }
        }

        private static void HandleBasicStructures(Colony colony, Civilization civ)
        {
            _text = "Step_1231:; HandleBasicStructures on; "
                    + "" + colony.Name + "; " + colony.Owner
                    ;
            Console.WriteLine(_text);


            colony.ProcessQueue();

            if (colony.BuildQueue.Count > 0)
            {
                _text = "Step_1234:; HandleBasicStructures on; "
                        + colony.Name + "; " + colony.Owner
                        + "; already bulding >; " + colony.BuildQueue[0].Description;
                ;
                Console.WriteLine(_text);
            }

            if (colony.BuildQueue.Count == 0)
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                _text = "Step_1232:; HandleBasicStructures: "
                    + " on; " + colony.Name
                    + "; " + colony.Owner
                    + "; Morale=; " + colony.Morale
                    + "; prodOutput=;" + prodOutput
                ;
                Console.WriteLine(_text);



                CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);
                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();

                string _toBuildText = "no structureProject to build";
                if (structureProject != null)
                {
                    _toBuildText = structureProject.BuildDesign.ToString();
                }


                _text = "Step_1235:; HandleBasicStructures: "
                            + " on; " + colony.Name
                            + "; " + colony.Owner
                            + "; Morale=; " + colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; ToBuild=;" + _toBuildText

                        ;
                Console.WriteLine(_text);

                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9.0)  // 2023-11-11 9.0 instead of 5.0
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                    _text = "Step_1236:; Added to Build"
                            + " on; " + colony.Name
                            + "; " + colony.Owner
                            + "; Morale=; " + colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; ToBuild=;" + structureProject.BuildDesign.ToString()

                            ;
                    Console.WriteLine(_text);
                }


                if (colony.BuildSlots.All(t => t.Project == null) /*&& colony.BuildQueue.Count == 0*/)  //2023-11-11
                {
                    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
                    _text = "Step_1234:; Added to Build"
                            + " on; " + colony.Name
                            + "; " + colony.Owner
                            + "; Morale=; " + colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; ToBuildonSlots.Count=;" + projects.Count

                            ;
                    Console.WriteLine(_text);
                    int count = 0;
                    foreach (var item in projects)
                    {

                        _text = "Step_1235:; OPTIONS to Build"
                            + " on; " + colony.Name
                            + "; " + colony.Owner
                            + "; Morale=; " + colony.Morale
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; BCost=;" + item.BuildDesign.BuildCost
                            + "; OPTIONStoBuildon #;" + count
                            + "; " + item.BuildDesign.ToString()

                            ;
                        count++;
                        Console.WriteLine(_text);
                    }
                }
            }
        }

        private static void HandleAdditionalStructures(Colony colony, Civilization civ)
        {
            // Build a shipyard ?
            if (colony.Shipyard == null)
            {
                BuildProject project = TechTreeHelper.GetBuildProjects(colony).FirstOrDefault(bp => bp.BuildDesign is ShipyardDesign);
                if (colony == GameContext.Current.Universe.HomeColonyLookup[civ] && project != null && !colony.IsBuilding(project.BuildDesign))
                {
                    colony.BuildQueue.Add(new BuildQueueItem(project));
                }
            }

            colony.ProcessQueue();

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                _text = "Step_1208: HandleAdditionalStructures: "
                        + " on " + colony.Name + " " + colony.Owner
                        ;
                Console.WriteLine(_text);


                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                {
                    // Industry Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));
                    if (upgradeIndustryProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                    }
                    else
                    {
                        _text = "Step_1203:; HandleBuildings on; "
                                + colony.Name + " " + colony.Owner
                                + " > no Upgrade INDUSTRY"
                                ;
                        Console.WriteLine(_text);
                    }
                }

                ////structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(colony)
                //    .OfType<StructureBuildProject>()
                //    .Where(p =>
                //            p.GetCurrentIndustryCost() > 0
                //            && EnumHelper
                //                .GetValues<ResourceType>()
                //                .Where(availableResources.ContainsKey)
                //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                //{
                //    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //}
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);
                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                }
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
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
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
            }
        }

        private static void HandleUpgrades(Colony colony, Civilization civ)
        {

            colony.ProcessQueue();

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                _text = "Step_1208: HandleUpgrades: "
                        + " on " + colony.Name + " " + colony.Owner
                        ;
                Console.WriteLine(_text);


                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
                {
                    // Industry Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));
                    if (upgradeIndustryProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                    }
                    else
                    {
                        _text = "Step_1209: HandleUpgrades on "
                                + colony.Name + " " + colony.Owner
                                + " > no Upgrade INDUSTRY"
                                ;
                        Console.WriteLine(_text);
                    }
                }

                ////structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(colony)
                //    .OfType<StructureBuildProject>()
                //    .Where(p =>
                //            p.GetCurrentIndustryCost() > 0
                //            && EnumHelper
                //                .GetValues<ResourceType>()
                //                .Where(availableResources.ContainsKey)
                //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                //{
                //    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //}
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => manager.Resources[r.Resource].CurrentValue - r.Used);
                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                }
            }

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count == 0)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
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
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
            }
        }

        private static void HandleBuyBuild(Colony colony, Civilization civ)
        {
            CivilizationManager manager = GameContext.Current.CivilizationManagers[civ];
            colony.BuildSlots.Where(s => s.Project?.IsRushed == false).ToList().ForEach(s =>
            {
                List<BuildProject> otherProjects = manager.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os != s && os.Project != null)
                    .Select(os => os.Project)
                    .Where(p => p.GetTimeEstimate() <= 1 || p.IsRushed)
                    .ToList();

                // what does this help? ...costs for the next projects??
                //int cost = otherProjects
                //    .Where(p => p.IsRushed)
                //    .Select(p => p.GetTotalCreditsCost())
                //    .DefaultIfEmpty()
                //    .Sum();

                int cost = s.Project.GetTotalCreditsCost() * 2;  // we take max half of the credits

                //if ((manager.Credits.CurrentValue - (cost * 0.2)) > s.Project.GetTotalCreditsCost())
                if ((manager.Credits.CurrentValue > cost))
                {
                    double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * colony.Morale.CurrentValue
                        / (0.5f * MoraleHelper.MaxValue) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
                    int maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, (colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost)
                        + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
                    int industryNeeded = colony.BuildSlots.Where(bs => bs.Project != null)
                        .Select(bs => bs.Project.IsRushed ? 0 : bs.Project.GetCurrentIndustryCost()).Sum();
                    int turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));


                    if (turnsNeeded > 1 && turnsNeeded < 3)  // we buy when turnsNeede = 2
                    {
                        _text = "Step_1210:; HandleBuyBuild: "
                            + "Credits.Current= " + manager.Credits.CurrentValue
                            + ", Costs= " + cost
                            + ", industryNeeded= " + industryNeeded
                            + ", prodOutput= " + prodOutput.ToString()
                            + ", turnsNeeded= " + turnsNeeded
                            + " > IsRushed for " + s.Project
                            + " on " + colony.Name + " " + s.Project.Location
                        ;
                        Console.WriteLine(_text);

                        s.Project.IsRushed = true;
                        while (colony.DeactivateFacility(ProductionCategory.Industry)) { }
                    }
                }
            });
        }

        //TODO: Move ship production out of colony AI. It requires a greater oversight than just a single colony
        //TODO: Is there any need for separate functions for empires and minor races?
        //TODO: Break these functions up into smaller chunks
        private static void HandleShipProductionEmpire(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
            {
                return;
            }

            //>Check ShipProduction

            IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            List<ShipDesign> shipDesigns = GameContext.Current.TechTrees[colony.OwnerID].ShipDesigns.ToList();
            List<Fleet> fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList();
            Sector homeSector = GameContext.Current.CivilizationManagers[civ].SeatOfGovernment.Sector;
            List<Fleet> homeFleets = homeSector.GetOwnedFleets(civ).ToList();


            IList<BuildProject> projects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            //foreach (BuildProject project in projects)
            //{
            //    _text = "ShipProduction_2"
            //        + " at " + colony.Location
            //        + " - " + colony.Owner
            //        + ": available= " + project.BuildDesign

            //        ;
            //    Console.WriteLine(_text);
            //}

            if (colony.Sector == homeSector)
            {
                _text = "Step_5380:; ShipProduction at " + colony.Location + " " + colony.Name
                    //+ " - Not Habited: Habitation= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
                Console.WriteLine(_text);

                neededColonizer = 0;

                CheckForColonizerBuildProject(colony);

                if (neededColonizer > 1)
                {
                    neededColonizer -= 1;
                    need1Colonizer = true;
                }


                // Colonization
                //if (GameContext.Current.Universe.FindOwned<Colony>(civ).Count < MaxEmpireColonyCount &&
                //    //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
                //    //need1Colonizer &&
                //    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
                if (need1Colonizer && colony.Sector.GetOwnedFleets(civ).All(o => !o.IsColonizer) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        _text = "Step_5384: ShipProduction "
                            + " at " + colony.Location
                            + " " + colony.Name
                            + " - " + colony.Owner
                            + ": Added Colonizer project..." + project.BuildDesign

                            ;
                        Console.WriteLine(_text);
                    }
                }

                // Construction
                if (colony.Sector.Station == null &&
                    colony.Sector.GetOwnedFleets(civ).All(o => !o.IsConstructor) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        _text = "Step_5386: ShipProduction "
                            + " at " + colony.Location
                            + " - " + colony.Owner
                            + ": Added Construction ship project..." + project.BuildDesign

                            ;
                        Console.WriteLine(_text);
                    }
                }

                // Military
                Fleet defenseFleet = homeSector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                if ((defenseFleet?.HasCommandShip != true) &&
                    homeFleets.All(o => !o.HasCommandShip) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Command).Any(colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Command && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                if ((defenseFleet == null || defenseFleet.Ships.Count < 5) &&
                    homeFleets.Where(o => o.IsBattleFleet).Sum(o => o.Ships.Count) < 5 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                    if (project != null)
                    {
                        project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    }
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }

                // Exploration - HomeSector has Starting Scouts
                if (!shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(colony.Shipyard.IsBuilding))
                {
                    for (int i = fleets.Count(o => o.IsScout); i < NumScouts; i++)
                    {
                        BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
                        if (project != null)
                        {
                            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        }
                    }
                }
            } // end of HomeSector

            // all Colonies - build colony ships
            //        if (GameContext.Current.Universe.FindOwned<Colony>(civ).Count < MaxEmpireColonyCount &&
            //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
            //!shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
            //        {
            //            BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
            //            if (project != null)
            //            {
            //                colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
            //            }
            //        }

            if (colony.Sector != homeSector && colony.Shipyard != null)
            {
                _text = "Step_5390:; next: check for ShipProduction - not at HomeSector: "
                    + colony.Shipyard.Design
                    + " at " + colony.Location
                    + " - " + colony.Owner
                    ;
                Console.WriteLine(_text);
                CheckForColonizerBuildProject(colony);
            }

            if (colony.Shipyard.BuildSlots.All(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects2 = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
                //foreach (BuildProject project in projects2)
                //{
                //    _text = "ShipProduction at HomeSector: "
                //        + " at " + colony.Location
                //        + " - " + colony.Owner
                //        + ": available= " + project.BuildDesign

                //        ;
                //    Console.WriteLine(_text);
                //}

                BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                if (newProject != null)
                {
                    colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));
                    _text = "Step_5386:; ShipProduction "
                        + " at " + colony.Location
                        + " - " + colony.Owner
                        + ": Added Colonizer project..." + newProject.BuildDesign

                        ;
                    Console.WriteLine(_text);
                }
            }

            foreach (var item in colony.Shipyard.BuildQueue)
            {
                _text = "Step_5387:; " + colony.Location
                    + ", ShipProduction > " + item.Project.BuildDesign
                    + ", TurnsRemaining= " + item.Project.TurnsRemaining


                    ;
                Console.WriteLine(_text);
            }

            colony.Shipyard.ProcessQueue();
        }

        private static void CheckForColonizerBuildProject(Colony colony)
        {
            // need a fleet for getting a range for IsSectorWithinFuelRange
            Fleet fleet = GameContext.Current.Universe.FindOwned<Fleet>(colony.Owner).Where(f => f.IsColonizer).FirstOrDefault();
            if (fleet == null)
                return;

            _text = "Step_5393:; CheckForColonizerBuildProject - using " + fleet.Location + " " + fleet.Ships[0].Design
                    //+ " - Not Habited: Habitation Aim= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
            Console.WriteLine(_text);

            var possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
                .Where(c => c.Sector != null && c.IsInhabited == false && c.IsHabitable(colony.Owner.Race) == true
                && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet) && DiplomacyHelper.IsTravelAllowed(colony.Owner, c.Sector)) /*&& mapData.IsScanned(c.Location)*/
                //&& mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //)//Where other science ship is not already going
                //.Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location))
                .ToList();

            neededColonizer = possibleSystems.Count;

            foreach (var item in possibleSystems)
            {
                _text = "Step_5396:; ShipProduction at " + colony.Location + " " + colony.Name
                    + " - possible: " + possibleSystems.Count
                    + " - Not Habited: Habitation Aim= "
                    + item.HasColony
                    + " at " + item.Location
                    + " - " + item.Owner
                    ;
                Console.WriteLine(_text);
            }
        }

        private static void HandleShipProductionMinor(Colony colony, Civilization civ)
        {
            if (colony.Shipyard == null)
            {
                return;
            }

            IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            ShipDesign[] shipDesigns = GameContext.Current.TechTrees[colony.OwnerID].ShipDesigns.ToArray();
            //var fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ);
            Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[civ].Sector;

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
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
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
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Military
                Fleet defenseFleet = homeSector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                if (civ.CivilizationType != CivilizationType.MinorPower)
                {
                    if ((defenseFleet == null || defenseFleet.Ships.Count < 2) &&
                        !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(colony.Shipyard.IsBuilding))
                    {
                        BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
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
                else if ((defenseFleet == null || defenseFleet.Ships.Count < 2) && !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack).Any(colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    if (project != null)
                    {
                        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
            }

            if (colony.Shipyard.BuildSlots.All(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
            }

            colony.Shipyard.ProcessQueue();
        }
    }
}