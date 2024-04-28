// File:ColonyAI.cs
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
using System.Windows;

namespace Supremacy.AI
{
    public static class ColonyAI
    {
        private const int NumScouts = 1;
        //private const int ColonyShipEveryTurns = 2;
        private const int ColonyShipEveryTurnsMinor = 5;
        private const int MaxMinorColonyCount = 3;

        //private static int neededColonizer;
        private const int MaxEmpireColonyCount = 999; // currently not used

        static ShipType needed_ShipType_1 = new ShipType();
        static ShipType needed_ShipType_2 = new ShipType();// in case two slots are free in one turn

        [NonSerialized]
        private static string _text;
        private static string newline = Environment.NewLine;
        private static bool boolCheckShipProduction = true;
        private static bool _bool_listPrioShipBuild_Empty = false;
        private static bool _shipOrderIsDone;

        //private static bool needed_1_done;
        //private static bool needed_2_done;
        private static readonly string blank = " ";


        public static void DoTurn([NotNull] Civilization civ)
        {
            _text = "Step_1101:; ColonyAI.DoTurn begins...";
            Console.WriteLine(_text);

            CivilizationManager civM = GameContext.Current.CivilizationManagers[civ.CivID];

            foreach (Colony colony in GameContext.Current.Universe.FindOwned<Colony>(civ.CivID))
            {
                _text += newline;// dummy, please keep
                _text = "Step_1102:; " + colony.Location + "; " + colony.Name + " > Handling colony"
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
                            + "; already building; " + colony.BuildQueue[0].Project.Description;
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

                int _shipNeeded_colony = civM.ShipColonyNeeded - civM.ShipColonyAvailable - civM.ShipColonyOrdered;
                int _shipNeeded_construction = civM.ShipConstructionNeeded - civM.ShipConstructionAvailable - civM.ShipConstructionOrdered;
                int _shipNeeded_medical = civM.ShipMedicalNeeded - civM.ShipMedicalAvailable - civM.ShipMedicalOrdered;
                int _shipNeeded_spy = civM.ShipSpyNeeded - civM.ShipSpyAvailable - civM.ShipSpyOrdered;
                int _shipNeeded_diplomatic = civM.ShipDiplomaticNeeded - civM.ShipDiplomaticAvailable - civM.ShipDiplomaticOrdered;
                int _shipNeeded_science = civM.ShipScienceNeeded - civM.ShipScienceAvailable - civM.ShipScienceOrdered;
                int _shipNeeded_scout = civM.ShipScoutNeeded - civM.ShipScoutAvailable - civM.ShipScoutOrdered;
                int _shipNeeded_fastattack = civM.ShipFastAttackAvailable - civM.ShipFastAttackAvailable - civM.ShipFastAttackOrdered;
                //int _shipNeeded_destroyer = civM. - civM.ShipDestroyerAvailable - civM.ShipDestroyerOrdered;
                int _shipNeeded_cruiser = civM.ShipCruiserNeeded - civM.ShipCruiserAvailable - civM.ShipCruiserOrdered;
                int _shipNeeded_strikecruiser = civM.ShipStrikeCruiserNeeded - civM.ShipStrikeCruiserAvailable - civM.ShipStrikeCruiserOrdered;
                int _shipNeeded_heavycruiser = civM.ShipHeavyCruiserNeeded - civM.ShipHeavyCruiserAvailable - civM.ShipHeavyCruiserOrdered;
                int _shipNeeded_command = civM.ShipCommandNeeded - civM.ShipCommandAvailable - civM.ShipCommandOrdered;
                int _shipNeeded_transport = civM.ShipTransportNeeded - civM.ShipTransportAvailable - civM.ShipTransportOrdered;



                Dictionary<ShipType, Tuple<int, string>> _listPrioShipBuild_tmp = new Dictionary<ShipType, Tuple<int, string>>();
                //Dictionary<ShipType, int> _listPrioShipBuild = new Dictionary<ShipType, int>();

                Tuple<int, string> _add = new Tuple<int, string>(_shipNeeded_colony * -1, "Colony");
                if (_shipNeeded_colony > 0) _listPrioShipBuild_tmp.Add(ShipType.Colony, _add);

                _add = new Tuple<int, string>(_shipNeeded_construction * -1, "Construction");
                if (_shipNeeded_construction > 0) _listPrioShipBuild_tmp.Add(ShipType.Construction, _add);

                _add = new Tuple<int, string>(_shipNeeded_medical * -1, "Medical");
                if (_shipNeeded_medical > 0) _listPrioShipBuild_tmp.Add(ShipType.Medical, _add);

                _add = new Tuple<int, string>(_shipNeeded_spy * -1, "Spy");
                if (_shipNeeded_spy > 0) _listPrioShipBuild_tmp.Add(ShipType.Spy, _add);

                _add = new Tuple<int, string>(_shipNeeded_diplomatic * -1, "Diplomatic");
                if (_shipNeeded_diplomatic > 0) _listPrioShipBuild_tmp.Add(ShipType.Diplomatic, _add);

                _add = new Tuple<int, string>(_shipNeeded_science * -1, "Science");
                if (_shipNeeded_science > 0) _listPrioShipBuild_tmp.Add(ShipType.Science, _add);

                _add = new Tuple<int, string>(_shipNeeded_scout * -1, "Scout");
                if (_shipNeeded_scout > 0) _listPrioShipBuild_tmp.Add(ShipType.Scout, _add);

                _add = new Tuple<int, string>(_shipNeeded_fastattack * -1, "FastAttack");
                if (_shipNeeded_fastattack > 0) _listPrioShipBuild_tmp.Add(ShipType.FastAttack, _add);

                _add = new Tuple<int, string>(_shipNeeded_cruiser * -1, "Cruiser");
                if (_shipNeeded_cruiser > 0) _listPrioShipBuild_tmp.Add(ShipType.Cruiser, _add);

                _add = new Tuple<int, string>(_shipNeeded_strikecruiser * -1, "StrikeCruiser");
                if (_shipNeeded_strikecruiser > 0) _listPrioShipBuild_tmp.Add(ShipType.StrikeCruiser, _add);

                _add = new Tuple<int, string>(_shipNeeded_heavycruiser * -1, "HeavyCruiser");
                if (_shipNeeded_heavycruiser > 0) _listPrioShipBuild_tmp.Add(ShipType.HeavyCruiser, _add);

                _add = new Tuple<int, string>(_shipNeeded_command * -1, "Command");
                if (_shipNeeded_command > 0) _listPrioShipBuild_tmp.Add(ShipType.Command, _add);

                _add = new Tuple<int, string>(_shipNeeded_transport * -1, "Transport");
                if (_shipNeeded_transport > 0) _listPrioShipBuild_tmp.Add(ShipType.Transport, _add);


                //Dictionary<ShipType,int> _listPrioShipBuild = _listPrioShipBuild_tmp.OrderByDescending(_l => _l.Value).ToList(); // doesn't sort
                //_listPrioShipBuild = _listPrioShipBuild_tmp.OrderByDescending(_l => _l.Value).ToList(); // doesn't sort
                //var sortedDict = from entry in _listPrioShipBuild_tmp orderby entry.Value ascending select entry;

                bool checkForShipProduction = true;
                //bool checkForShipProduction = false;
                if (checkForShipProduction)
                    _text = ""; /*just for breakpoint*/

        HandleShipProduction(colony, civ, _listPrioShipBuild_tmp);
        //if (!PlayerAI.IsInFinancialTrouble(civ))
        //{
        //    if (civ.IsEmpire)
        //    {
        //        HandleShipProductionEmpire(colony, civ);
        //    }
        //    else
        //    {
        //        HandleShipProductionMinor(colony, civ);
        //    }
        //}

    }
    _text = "Step_1103:; ColonyAI.DoTurn is done..."

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
        _text = "Step_1228:; HandleFoodProduction on "
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
    if (colony.Name == "Nadra")
    {
        _text = ""; // just for breakpoint
    }


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
                    _text = "Step_1205:; HandleBuildings on "
                        + colony.Name + " " + colony.Owner
                        + " > added 1 Industry Facility Build Order"
                        ;
                    Console.WriteLine(_text);
                }
                else
                {
                    //than Research
                    colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                    _text = "Step_1206:; HandleBuildings on "
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
                    _text = "Step_1205:; HandleBuildings on "
                        + colony.Name + " " + colony.Owner
                        + " > added 1 Research Facility Build Order"
                        ;
                    Console.WriteLine(_text);
                }
                else
                {
                    //than Research
                    colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                    _text = "Step_1206:; HandleBuildings on "
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
                _text = "Step_1204:; HandleBuildings on "
                    + colony.Name + " " + colony.Owner
                    + " " + colony.GetAvailableLabor() + " > flexLabors available"
                    ;
                Console.WriteLine(_text);
                if (colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                {
                    //Industry
                    colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                    _text = "Step_1205:; HandleBuildings on "
                        + colony.Name + " " + colony.Owner
                        + " > added 1 Research Facility Build Order"
                        ;
                    Console.WriteLine(_text);
                }
                else
                {
                    //than Research
                    colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                    _text = "Step_1206:; HandleBuildings on "
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
                _text = "Step_1213:; HandleBuildings on "
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
                    + "; OPTIONS_to_Build_on #;" + count
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

private static void HandleShipProduction(Colony colony, Civilization civ, Dictionary<ShipType, Tuple<int, string>> _listPrioShipBuild)
{
    bool checkForShipProduction = true;
    _shipOrderIsDone = false;

    if (colony.Shipyard == null) { return; }
    if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; }

    _text = "Step_5780:; " + colony.Location + " ShipProduction > " + colony.Name;
    Console.WriteLine(_text);

    IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
    List<ShipDesign> shipDesigns = GameContext.Current.TechTrees[colony.OwnerID].ShipDesigns.ToList();
    List<Fleet> fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList();
    Sector homeSector = GameContext.Current.CivilizationManagers[civ].SeatOfGovernment.Sector;
    List<Fleet> homeFleets = homeSector.GetOwnedFleets(civ).ToList();
    CivilizationManager civM = GameContext.Current.CivilizationManagers[civ.CivID];

    //projects identical with potentialProjects
    IList<BuildProject> projects = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);

    var sortedDict = from entry in _listPrioShipBuild orderby entry.Value ascending select entry.Key;

    foreach (BuildProject proj in projects)
    {
        _text = "Step_5781:; " + colony.Location
            + " ShipProduction"
            + " - " + colony.Owner

            + " (needs " + proj.TurnsRemaining + " Turns)"
            + ": available= " + proj.BuildDesign
            ;
        Console.WriteLine(_text);

        // Ship production
        //potentialProjects.Add(proj); - this is already populated

        // this works by the sorting out of the data file 'TechObj_6_Ships.xml'
        // if 2nd is transport > it will check for Transport and block the BuildQueue with it

        //if (proj.Description.Contains("COLONY")) CheckForColonizer(colony, civM, ShipType.Colony, proj);
        //if (proj.Description.Contains("MEDICAL")) CheckForBuild(colony, civM, ShipType.Medical, proj);
        //if (proj.Description.Contains("SPY")) CheckForBuild(colony, civM, ShipType.Spy, proj);
        //if (proj.Description.Contains("DIPLOMATIC")) CheckForBuild(colony, civM, ShipType.Diplomatic, proj);

        //if (proj.Description.Contains("COMMAND")) CheckForBuild(colony, civM, ShipType.Command, proj);
        //if (proj.Description.Contains("CRUISER")) CheckForBuild(colony, civM, ShipType.Command, proj); // includes Heavy and StrikeCruiser
        //if (proj.Description.Contains("DESTROYER")) CheckForBuild(colony, civM, ShipType.FastAttack, proj);
        //if (proj.Description.Contains("FRIGATE")) CheckForBuild(colony, civM, ShipType.FastAttack, proj);
        //if (proj.Description.Contains("SCOUT")) CheckForBuild(colony, civM, ShipType.Scout, proj);
        //if (proj.Description.Contains("SCIENCE")) CheckForBuild(colony, civM, ShipType.Science, proj);

        //if (proj.Description.Contains("TRANSPORT")) CheckForBuild(colony, civM, ShipType.Transport, proj);
        //if (proj.Description.Contains("CONSTRUCTION")) CheckForBuild(colony, civM, ShipType.Construction, proj);

    }

    //Dictionary<int, ShipType> _listPrioShipBuild_tmp = new Dictionary<int, ShipType>();
    //_listPrioShipBuild_tmp.Add(_shipNeeded_colony, ShipType.Colony);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_construction, ShipType.Construction);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_medical, ShipType.Medical);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_spy, ShipType.Spy);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_diplomatic, ShipType.Diplomatic);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_science, ShipType.Science);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_scout, ShipType.Scout);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_fastattack, ShipType.FastAttack);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_cruiser, ShipType.Cruiser);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_strikecruiser, ShipType.StrikeCruiser);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_heavycruiser, ShipType.HeavyCruiser);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_command, ShipType.Command);
    //_listPrioShipBuild_tmp.Add(_shipNeeded_transport, ShipType.Transport);

    // first value is basic requirement
    int _shipNeeded_colony = 1 + civM.ShipColonyNeeded - civM.ShipColonyAvailable - civM.ShipColonyOrdered;
    int _shipNeeded_construction = 1 + civM.ShipConstructionNeeded - civM.ShipConstructionAvailable - civM.ShipConstructionOrdered;
    int _shipNeeded_medical = 2 + civM.ShipMedicalNeeded - civM.ShipMedicalAvailable - civM.ShipMedicalOrdered;
    int _shipNeeded_spy = 1 + civM.ShipSpyNeeded - civM.ShipSpyAvailable - civM.ShipSpyOrdered;
    int _shipNeeded_diplomatic = 1 + civM.ShipDiplomaticNeeded - civM.ShipDiplomaticAvailable - civM.ShipDiplomaticOrdered;
    int _shipNeeded_science = 1 + civM.ShipScienceNeeded - civM.ShipScienceAvailable - civM.ShipScienceOrdered;
    int _shipNeeded_scout = 1 + civM.ShipScoutNeeded - civM.ShipScoutAvailable - civM.ShipScoutOrdered;
    int _shipNeeded_fastattack = 3 + civM.ShipFastAttackAvailable - civM.ShipFastAttackAvailable - civM.ShipFastAttackOrdered;
    //int _shipNeeded_destroyer = civM. - civM.ShipDestroyerAvailable - civM.ShipDestroyerOrdered;
    int _shipNeeded_cruiser = 2 + civM.ShipCruiserNeeded - civM.ShipCruiserAvailable - civM.ShipCruiserOrdered;
    int _shipNeeded_strikecruiser = 1 + civM.ShipStrikeCruiserNeeded - civM.ShipStrikeCruiserAvailable - civM.ShipStrikeCruiserOrdered;
    int _shipNeeded_heavycruiser = 1 + civM.ShipHeavyCruiserNeeded - civM.ShipHeavyCruiserAvailable - civM.ShipHeavyCruiserOrdered;
    int _shipNeeded_command = 2 + civM.ShipCommandNeeded - civM.ShipCommandAvailable - civM.ShipCommandOrdered;
    int _shipNeeded_transport = 2 + civM.ShipTransportNeeded - civM.ShipTransportAvailable - civM.ShipTransportOrdered;

    int _ship_Total_Needed =
          _shipNeeded_colony
        + _shipNeeded_construction
        + _shipNeeded_medical
        + _shipNeeded_spy
        + _shipNeeded_diplomatic
        + _shipNeeded_science
        + _shipNeeded_scout
        + _shipNeeded_fastattack
        + _shipNeeded_cruiser
        + _shipNeeded_strikecruiser
        + _shipNeeded_heavycruiser
        + _shipNeeded_command
        + _shipNeeded_transport
        ;
    //ggg
    ShipType _neededShipType = ShipType.Medical; // default

    if (_listPrioShipBuild.Count == 0)
    {
        _text = _bool_listPrioShipBuild_Empty.ToString(); // dummy, just keep
        _bool_listPrioShipBuild_Empty = true;
    }
    else
    {
        _text = "Step_5782:; " + colony.Location
        + " ShipProduction"
        + " - " + colony.Owner
        + " > _listPrioShipBuild.Count= " + _listPrioShipBuild.Count
        ;
        Console.WriteLine(_text);
        _listPrioShipBuild.OrderByDescending(_l => _l.Value);
        _text = _listPrioShipBuild[0].Item2.ToString();

                if (civ.Key.Contains("Botha"))
                    _text += _text; // just for breakpoint

        //_neededShipType = _listPrioShipBuild.;
        switch (_listPrioShipBuild[0].Item2)
        {
            case "Colony":
                _neededShipType = ShipType.Colony;
                break;
            case "Construction":
                _neededShipType = ShipType.Construction;
                break;
            case "Medical":
                _neededShipType = ShipType.Medical;
                break;
            case "Transport":
                _neededShipType = ShipType.Transport;
                break;
            case "Spy":
                _neededShipType = ShipType.Spy;
                break;
            case "Diplomatic":
                _neededShipType = ShipType.Science;
                break;
            case "Science":
                _neededShipType = ShipType.Colony;
                break;
            case "Scout":
                _neededShipType = ShipType.Scout;
                break;
            case "FastAttack":
                _neededShipType = ShipType.FastAttack;
                break;
            case "Cruiser":
                _neededShipType = ShipType.Cruiser;
                break;
            case "HeavyCruiser":
                _neededShipType = ShipType.HeavyCruiser;
                break;
            case "StrikeCruiser":
                _neededShipType = ShipType.StrikeCruiser;
                break;
            case "Command":
                _neededShipType = ShipType.Command;
                break;
                //default":
                //    break;
        }

        BuildShipType(colony, civM, _neededShipType, potentialProjects[0]);
        goto ProcessQueue;

    }



    //_neededShipType = ShipType.Construction; // here more code to do


    if (checkForShipProduction)
        _text = ""; /*just for breakpoint*/

    // Already checked before but here to hover the count
    if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; }


    var _listPrioShipBuild2 = new List<ShipType>();// _listPrioShipBuild2 is out of the options if first list is empty
    foreach (BuildProject proj in potentialProjects)  // find Prio
    {
        //if (checkForShipProduction)
        //    _text = ""; /*just for breakpoint*/

        if (potentialProjects.Count == 1)
        {
            BuildShipType(colony, civM, ShipType.Medical, potentialProjects[0]);
            goto ProcessQueue;
        }

        //if (_bool_listPrioShipBuild_Empty)
        //{
        //    _listPrioShipBuild.Add(proj.BuildDesign.DesignID, 1);
        //}
        if (_shipOrderIsDone == false)
        {

            if (proj.Description.Contains("COLONY") /*&& _shipNeeded_colony > 0*/)
            {
                CheckForColonizer(colony, civM, ShipType.Colony, proj);
            }


            if (proj.Description.Contains("COMMAND") /*&& _shipNeeded_command > 0*/)
                CheckForBuild(colony, civM, ShipType.Command, proj);
            if (proj.Description.Contains("CRUISER") /*&& _shipNeeded_cruiser > 0*/)
                CheckForBuild(colony, civM, ShipType.Cruiser, proj); // includes Heavy and StrikeCruiser
            if (proj.Description.Contains("DESTROYER") /*&& _shipNeeded_fastattack > 0*/)
                CheckForBuild(colony, civM, ShipType.FastAttack, proj);
            if (proj.Description.Contains("FRIGATE") /*&& _shipNeeded_fastattack > 0*/)
                CheckForBuild(colony, civM, ShipType.FastAttack, proj);
            if (proj.Description.Contains("FIGHTER") /*&& _shipNeeded_fastattack > 0*/)
                CheckForBuild(colony, civM, ShipType.FastAttack, proj);
            if (proj.Description.Contains("SURVEYOR") /*&& _shipNeeded_fastattack > 0*/)
                CheckForBuild(colony, civM, ShipType.FastAttack, proj);
            if (proj.Description.Contains("RAIDER") /*&& _shipNeeded_fastattack > 0*/)
                CheckForBuild(colony, civM, ShipType.FastAttack, proj);
            if (proj.Description.Contains("SCOUT") /*&& _shipNeeded_scout > 0*/)
                CheckForBuild(colony, civM, ShipType.Scout, proj);
            if (proj.Description.Contains("SCIENCE") /*&& _shipNeeded_science > 0*/)
                CheckForBuild(colony, civM, ShipType.Science, proj);


            if (proj.Description.Contains("TRANSPORT") /*&& _shipNeeded_transport > 0*/)
                CheckForBuild(colony, civM, ShipType.Transport, proj);
            if (proj.Description.Contains("CONSTRUCTION") /*&& _shipNeeded_construction > 0*/)
                CheckForBuild(colony, civM, ShipType.Construction, proj);
            if (proj.Description.Contains("MEDICAL") /*&& _shipNeeded_medical > 0*/)
                CheckForBuild(colony, civM, ShipType.Medical, proj);
            if (proj.Description.Contains("SPY") /*&& _shipNeeded_spy > 0*/)
                CheckForBuild(colony, civM, ShipType.Spy, proj);
            if (proj.Description.Contains("DIPLOMATIC") /*&& _shipNeeded_diplomatic > 0*/)
                CheckForBuild(colony, civM, ShipType.Diplomatic, proj);


            //if (_ship_Total_Needed < 1) // no ship is needed
            //{
            if (proj.Description.Contains("COMMAND")) { _listPrioShipBuild2.Add(ShipType.Command); _shipOrderIsDone = true; }

            if (proj.Description.Contains("DESTROYER")) { _listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
            if (proj.Description.Contains("FRIGATE")) { _listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
            if (proj.Description.Contains("FIGHTER")) { _listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
            if (proj.Description.Contains("SURVEYOR")) { _listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
            if (proj.Description.Contains("RAIDER")) { _listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
            if (proj.Description.Contains("SCOUT")) { _listPrioShipBuild2.Add(ShipType.Scout); _shipOrderIsDone = true; }
            if (proj.Description.Contains("SCIENCE")) { _listPrioShipBuild2.Add(ShipType.Science); _shipOrderIsDone = true; }
            if (proj.Description.Contains("MEDICAL")) { _listPrioShipBuild2.Add(ShipType.Medical); _shipOrderIsDone = true; }
            if (proj.Description.Contains("COLONY")) { _listPrioShipBuild2.Add(ShipType.Colony); _shipOrderIsDone = true; }
            if (proj.Description.Contains("CONSTRUCTION")) { _listPrioShipBuild2.Add(ShipType.Construction); _shipOrderIsDone = true; }
            if (proj.Description.Contains("TRANSPORT")) { _listPrioShipBuild2.Add(ShipType.Transport); _shipOrderIsDone = true; }
            if (proj.Description.Contains("DIPOMATIC")) { _listPrioShipBuild2.Add(ShipType.Diplomatic); _shipOrderIsDone = true; }
            if (proj.Description.Contains("SPY")) { _listPrioShipBuild2.Add(ShipType.Spy); _shipOrderIsDone = true; }
            if (proj.Description.Contains("CRUISER"))
            {
                if (proj.Description.Contains("HEAVY_CRUISER"))
                {
                    _listPrioShipBuild2.Add(ShipType.HeavyCruiser); _shipOrderIsDone = true;
                }

                if (proj.Description.Contains("STRIKE_CRUISER"))
                {
                    _listPrioShipBuild2.Add(ShipType.StrikeCruiser); _shipOrderIsDone = true;
                }

                if (proj.Description.Contains("CRUISER"))
                {
                    _listPrioShipBuild2.Add(ShipType.Cruiser); _shipOrderIsDone = true;
                }
            }
        }
        //needed_ShipType_1 = ShipType.;
        //break; 

        //if (proj.Description.Contains("CRUISER")) {
        //    _listPrioShipBuild2.Add(ShipType.Cruiser); 
        //    //needed_ShipType_1 = ShipType.Cruiser; 
        //    //break; 
        //}
        //if (proj.Description.Contains("DESTROYER")) {
        //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
        //    //needed_ShipType_1 = ShipType.FastAttack; 
        //    //break; 
        //}
        //if (proj.Description.Contains("FRIGATE")) {
        //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
        //    //needed_ShipType_1 = ShipType.FastAttack; 
        //    //break; 
        //}
        //if (proj.Description.Contains("FIGHTER")) {
        //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
        //    //needed_ShipType_1 = ShipType.FastAttack; 
        //    //break; 
        //}
        //if (proj.Description.Contains("SURVEYOR")) {
        //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
        //    //needed_ShipType_1 = ShipType.FastAttack; 
        //    //break; 
        //}
        //if (proj.Description.Contains("RAIDER")) {
        //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
        //    //needed_ShipType_1 = ShipType.FastAttack; 
        //    //break; 
        //}
        //if (proj.Description.Contains("SCOUT")) {
        //    _listPrioShipBuild2.Add(ShipType.Scout); 
        //    //needed_ShipType_1 = ShipType.Scout; 
        //    //break; 
        //}
        //if (proj.Description.Contains("SCIENCE")) {
        //    _listPrioShipBuild2.Add(ShipType.Science); 
        //    //needed_ShipType_1 = ShipType.Science; 
        //    //break; 
        //}
        //}
        //checked Map.txt and JNAI Surveyor



    }

    int xy = 0;
    foreach (var item in colony.Shipyard.BuildQueue)
    {
        xy += 1;
        _text = _text = "Step_5787:; " + colony.Location
            + " ShipProduction-Queue # " + xy
            + " - " + colony.Owner

            + " (needs " + item.TurnsRemaining + " Turns)"
            + ": in queue= " + item.Project.BuildDesign
            ;
        Console.WriteLine(_text);
    }

    if (boolCheckShipProduction)
        _text = "do a breakpoint here";

    if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; } // only for Colony Ships we try to order 2 ones


    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));

    //CheckForSystemsToColonizeProject(colony);
    //int _shipcolonyNeeded = civM.ShipColonyNeeded - civM.ShipColonyAvailable - civM.ShipColonyOrdered;
    //if (_shipcolonyNeeded > 0)
    //{
    //    needed_ShipType_1 = ShipType.Colony;
    //    BuildShipType(colony, civM, needed_ShipType_1, project);
    //    if (_shipcolonyNeeded > 2) 
    //    { 
    //        needed_ShipType_2 = ShipType.Colony; 
    //        BuildShipType(colony, civM, needed_ShipType_2, project);
    //        goto ProcessQueue;
    //    }
    //}

    //_text = "Step_5782:; ShipProduction_2"
    //        + " at " + colony.Location
    //        + " - " + colony.Owner
    //        + ": ColonyShips: Available= " + civM.ShipColonyAvailable
    //        + ", Needed= " + civM.ShipColonyNeeded
    //        + ", Ordered= " + civM.ShipColonyOrdered

    //        ;
    //Console.WriteLine(_text);

    //BuildShipType(colony, civM, needed_ShipType_1, project); needed_1_done = true;
    //BuildShipType(colony, civM, needed_ShipType_2, project); needed_2_done = true;

    //if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; } // only for Colony Ships we try to order 2 ones
    //else...
    //civM.ShipSpyNeeded = 2; // always 2 Medical needed as storage
    //int _shipMedicalNeeded = civM.ShipMedicalNeeded - civM.ShipMedicalAvailable - civM.ShipMedicalOrdered;

    // Medical
    //while (colony.Shipyard.BuildQueue.Count > 1)
    //{
    //    civM.ShipMedicalNeeded = 2; // always 2 Medical needed as storage
    //    int _shipMedicalNeeded = civM.ShipMedicalNeeded - civM.ShipMedicalAvailable - civM.ShipMedicalOrdered;
    //    if(_shipMedicalNeeded > 0) BuildShipType(colony, civM, ShipType.Medical, project);
    //}

    //// Spy
    //while (colony.Shipyard.BuildQueue.Count > 1)
    //{
    //    civM.ShipSpyNeeded = 1; // always 2 Medical needed as storage
    //    int _ShipSpyNeeded = civM.ShipSpyNeeded - civM.ShipSpyAvailable - civM.ShipSpyOrdered;
    //    if (_ShipSpyNeeded > 0) BuildShipType(colony, civM, ShipType.Spy, project);
    //}

    //// Spy
    //while (colony.Shipyard.BuildQueue.Count > 1)
    //{
    //    civM.ShipSpyNeeded = 1; // always 2 Medical needed as storage
    //    int _ShipSpyNeeded = civM.ShipSpyNeeded - civM.ShipSpyAvailable - civM.ShipSpyOrdered;
    //    if (_ShipSpyNeeded > 0) BuildShipType(colony, civM, ShipType.Spy, project);
    //}



    //if (civM.ShipColonyNeeded > civM.ShipColonyAvailable && colony.Sector.GetOwnedFleets(civ).All(o => !o.IsColonizer) &&
    //    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
    //if (needed_ShipType_1 == ShipType.Colony) BuildShipType(colony, civM, needed_ShipType_1);
    //if (needed_ShipType_2 == ShipType.Colony) BuildShipType(colony, civM, needed_ShipType_2);
    //{
    //    BuildShipType(colony, civM);
    //    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
    //    //if (project != null)
    //    //{
    //    //    colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
    //    //    _text = "Step_5383:; ShipProduction "
    //    //        + " at " + colony.Location
    //    //        + " " + colony.Name
    //    //        + " - " + colony.Owner
    //    //        + ": Added Colonizer project..." + project.BuildDesign
    //    //        ;
    //    //    Console.WriteLine(_text);

    //    //    civM.ShipColonyOrdered += 1;
    //    //}
    //}


    // Construction
    //if (civM.ShipConstructionAvailable < 2 &&
    //    colony.Sector.GetOwnedFleets(civ).All(o => !o.IsConstructor) &&
    //    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(colony.Shipyard.IsBuilding))
    //{
    //    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
    //    if (project != null)
    //    {
    //        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
    //        _text = "Step_5384:; ShipProduction "
    //            + " at " + colony.Location
    //            + " - " + colony.Owner
    //            + ": Added Construction ship project..." + project.BuildDesign

    //            ;
    //        Console.WriteLine(_text);
    //    }
    //}


    // Military
    //Fleet defenseFleet = homeSector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
    //if ((defenseFleet?.HasCommandShip != true) &&
    //    homeFleets.All(o => !o.HasCommandShip) &&
    //    !shipDesigns.Where(o => o.ShipType == ShipType.Command).Any(colony.Shipyard.IsBuilding))
    //{
    //    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Command && p.BuildDesign == d));
    //    if (project != null)
    //    {
    //        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
    //    }
    //}
    //if ((defenseFleet == null || defenseFleet.Ships.Count < 5) &&
    //    homeFleets.Where(o => o.IsBattleFleet).Sum(o => o.Ships.Count) < 5 &&
    //    !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(colony.Shipyard.IsBuilding))
    //{
    //    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
    //    if (project != null)
    //    {
    //        project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
    //    }
    //    if (project != null)
    //    {
    //        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
    //    }
    //}

    //// Exploration - HomeSector has Starting Scouts
    //if (!shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(colony.Shipyard.IsBuilding))
    //{
    //    for (int i = fleets.Count(o => o.IsScout); i < NumScouts; i++)
    //    {
    //        //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
    //        if (project != null)
    //        {
    //            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
    //        }
    //    }
    //}


    //} // end of HomeSector

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

    // not HomeSector or especially SeatOfGovernment
    //if (colony.Sector != homeSector && colony.Shipyard != null)
    //if (colony.Shipyard != null && colony.Shipyard.BuildQueue.Count == 0)
    //{
    //    _text = "Step_5360:; " + colony.Location //+ " next: check for ShipProduction - not at HomeSector: "
    //        + " " + colony.Shipyard.Design

    //        + " at " + colony.Name
    //        + ", Owner= " + colony.Owner
    //        + " - here no ship is building - maybe on the next code"
    //        ;
    //    Console.WriteLine(_text);
    //    //CheckForSystemsToColonizeProject(colony);
    //}


    // this builds a colonizer - why only colonizer ?
    if (colony.Shipyard.BuildSlots.Any(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
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



        if (_neededShipType != ShipType.Colony)
        {
            if (_listPrioShipBuild2.Contains(ShipType.Colony)) _neededShipType = ShipType.Colony;
            if (_listPrioShipBuild2.Contains(ShipType.Medical)) _neededShipType = ShipType.Medical;
            if (_listPrioShipBuild2.Contains(ShipType.Spy)) _neededShipType = ShipType.Spy;
            if (_listPrioShipBuild2.Contains(ShipType.Diplomatic)) _neededShipType = ShipType.Diplomatic;
            if (_listPrioShipBuild2.Contains(ShipType.Construction)) _neededShipType = ShipType.Construction;
            if (_listPrioShipBuild2.Contains(ShipType.Transport)) _neededShipType = ShipType.Transport;

            if (_listPrioShipBuild2.Contains(ShipType.Scout)) _neededShipType = ShipType.Scout;
            if (_listPrioShipBuild2.Contains(ShipType.Science)) _neededShipType = ShipType.Science;
            if (_listPrioShipBuild2.Contains(ShipType.FastAttack)) _neededShipType = ShipType.FastAttack;
            if (_listPrioShipBuild2.Contains(ShipType.Cruiser)) _neededShipType = ShipType.Cruiser;
            if (_listPrioShipBuild2.Contains(ShipType.HeavyCruiser)) _neededShipType = ShipType.HeavyCruiser;
            if (_listPrioShipBuild2.Contains(ShipType.StrikeCruiser)) _neededShipType = ShipType.StrikeCruiser;
            if (_listPrioShipBuild2.Contains(ShipType.Command)) _neededShipType = ShipType.Command;
        }




        //_neededShipType = ShipType.Construction; // here more code to do
        //if (needed_ShipType_1 != null)
        //    _neededShipType = needed_ShipType_1;

        BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
        if (newProject != null)
        {
            colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));

            _text = "Step_5388:; "
                + colony.Location
                + " > ShipProduction "
                + " - " + colony.Owner
                + ": Added Construction project..." + newProject.BuildDesign

                ;
            Console.WriteLine(_text);
        }
    }



//foreach (var item in colony.Shipyard.BuildQueue)
//{
//    _text = "Step_5387:; " + colony.Location + " " + colony.Name
//        + ", ShipProduction > " + item.Project.BuildDesign
//        + ", TurnsRemaining= " + item.Project.TurnsRemaining


//        ;
//    Console.WriteLine(_text);
//}

ProcessQueue:;
    colony.Shipyard.ProcessQueue();
}

//private static void PopulateEmptyPrioShipBuild(CivilizationManager civM, ShipType _shiptype)
//{
//    if (_bool_listPrioShipBuild_Empty)
//    {
//        civM._listPrioShipBuild.Add(_shiptype, 1);
//    }
//}

private static void CheckForColonizer(Colony colony, CivilizationManager civM, ShipType shipType, BuildProject project)
{
    if (boolCheckShipProduction)
        _text = "do a breakpoint here";
    CheckForSystemsToColonizeProject(colony);
    int _shipcolonyNeeded = civM.ShipColonyNeeded - civM.ShipColonyAvailable - civM.ShipColonyOrdered;
    if (_shipcolonyNeeded > 0)
    {
        needed_ShipType_1 = ShipType.Colony;
        BuildShipType(colony, civM, needed_ShipType_1, project);
        if (_shipcolonyNeeded > 2)
        {
            needed_ShipType_2 = ShipType.Colony;
            BuildShipType(colony, civM, needed_ShipType_2, project);
            //goto ProcessQueue;
        }
    }

    _text = "Step_5793:; " + colony.Location + " ShipProduction"
            + " at " + colony.Name
            //+ " - " + colony.Owner
            + ": ColonyShips: Available= " + civM.ShipColonyAvailable
            + ", Needed= " + civM.ShipColonyNeeded
            + ", Ordered= " + civM.ShipColonyOrdered

            ;
    if (boolCheckShipProduction)
    {
        Console.WriteLine(_text);
    }


    //BuildShipType(colony, civM, needed_ShipType_1, project); //needed_1_done = true;
    //BuildShipType(colony, civM, needed_ShipType_2, project); //needed_2_done = true;
}

private static void CheckForBuild(Colony colony, CivilizationManager civM, ShipType shipType, BuildProject project)
{
    if (colony.Shipyard == null) { return; }
    if (colony.Shipyard.BuildQueue.Count > 1)
    {
        return;
    }

    //if (_) { }

    switch (shipType)
    {
        case ShipType.Colony:
            //if (_needed)
            civM.ShipColonyOrdered += 1;
            break;
        case ShipType.Construction:
            civM.ShipConstructionOrdered += 1;
            break;
        case ShipType.Medical:
            civM.ShipMedicalOrdered += 1;
            break;
        case ShipType.Transport:
            civM.ShipTransportOrdered += 1;
            break;
        case ShipType.Spy:
            civM.ShipSpyOrdered += 1;
            break;
        case ShipType.Diplomatic:
            civM.ShipDiplomaticOrdered += 1;
            break;
        case ShipType.Science:
            civM.ShipScienceOrdered += 1;
            break;
        case ShipType.Scout:
            civM.ShipScoutOrdered += 1;
            break;
        case ShipType.FastAttack:
            civM.ShipFastAttackOrdered += 1;
            break;
        case ShipType.Cruiser:
            civM.ShipCruiserOrdered += 1;
            break;
        case ShipType.HeavyCruiser:
            civM.ShipHeavyCruiserOrdered += 1;
            break;
        case ShipType.StrikeCruiser:
            civM.ShipStrikeCruiserOrdered += 1;
            break;
        case ShipType.Command:
            civM.ShipCommandOrdered += 1;
            break;
        default:
            break;
    }
    BuildShipType(colony, civM, shipType, project); //needed_1_done = true;
    _text = "Step_5782:; " + colony.Location + " ShipProduction"
            + " at " + colony.Name
            + ", Owner= " + colony.Owner
            + " > ordered ShipBuilding-Tpye: " + shipType
            //+ ", Needed= " + civM.ShipColonyNeeded
            //+ ", Ordered= " + civM.ShipColonyOrdered

            ;
    Console.WriteLine(_text);

    //BuildShipType(colony, civM, needed_ShipType_2, project); //needed_2_done = true;
}

private static void BuildShipType(Colony colony, CivilizationManager civM, ShipType shipType, BuildProject project)
{
    //if (shipType == null)
    //    return;

    // Medical is a placeholder if only one is available
    //if (shipType == ShipType.Medical)
    //{
    //    _text = "Step_5381:; just to be notified"
    //        ;
    //    Console.WriteLine(_text);
    //}


    //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
    if (project != null)
    {
        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
        _text = "Step_5383:; " + colony.Location
            + " ShipProduction at"
            + " " + colony.Name
            + ", Owner= " + colony.Owner
            + ": Added Ship Build project..." + project.BuildDesign
            ;
        Console.WriteLine(_text);

        switch (shipType)
        {
            case ShipType.Colony:
                civM.ShipColonyOrdered += 1;
                break;
            case ShipType.Construction:
                civM.ShipConstructionOrdered += 1;
                break;
            case ShipType.Medical:
                civM.ShipMedicalOrdered += 1;
                break;
            case ShipType.Transport:
                civM.ShipTransportOrdered += 1;
                break;
            case ShipType.Spy:
                civM.ShipSpyOrdered += 1;
                break;
            case ShipType.Diplomatic:
                civM.ShipDiplomaticOrdered += 1;
                break;
            case ShipType.Science:
                civM.ShipScienceOrdered += 1;
                break;
            case ShipType.Scout:
                civM.ShipScoutOrdered += 1;
                break;
            case ShipType.FastAttack:
                civM.ShipFastAttackOrdered += 1;
                break;
            case ShipType.Cruiser:
                civM.ShipCruiserOrdered += 1;
                break;
            case ShipType.HeavyCruiser:
                civM.ShipHeavyCruiserOrdered += 1;
                break;
            case ShipType.StrikeCruiser:
                civM.ShipStrikeCruiserOrdered += 1;
                break;
            case ShipType.Command:
                civM.ShipCommandOrdered += 1;
                break;
            default:
                break;
        }
    }
}


//TODO: Move ship production out of colony AI. It requires a greater oversight than just a single colony
//TODO: Is there any need for separate functions for empires and minor races? > 2024: I guess: no !!!
//TODO: Break these functions up into smaller chunks
#pragma warning disable IDE0051 // Remove unused private members
private static void HandleShipx_OFF_ProductionEmpire(Colony colony, Civilization civ)
#pragma warning restore IDE0051 // Remove unused private members
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

        // Colonization
        //neededColonizer = 0;

        //CheckForSystemsToColonizeProject(colony);

        int _colonizerAvailable = GameContext.Current.Universe.FindOwned<Fleet>(colony.Owner).Where(f => f.IsColonizer).Count();

        //if (neededColonizer > _colonizerAvailable)
        //{
        //    neededColonizer -= 1;
        //    need1Colonizer = true;
        //}



        ////if (GameContext.Current.Universe.FindOwned<Colony>(civ).Count < MaxEmpireColonyCount &&
        ////    //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
        ////    //need1Colonizer &&
        ////    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
        //if (need1Colonizer && colony.Sector.GetOwnedFleets(civ).All(o => !o.IsColonizer) &&
        //    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(colony.Shipyard.IsBuilding))
        //{
        //    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
        //    if (project != null)
        //    {
        //        colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
        //        _text = "Step_5384: ShipProduction "
        //            + " at " + colony.Location
        //            + " " + colony.Name
        //            + " - " + colony.Owner
        //            + ": Added Colonizer project..." + project.BuildDesign

        //            ;
        //        Console.WriteLine(_text);
        //    }
        //}


        // Construction
        if (colony.Sector.Station == null &&
            colony.Sector.GetOwnedFleets(civ).All(o => !o.IsConstructor) &&
            !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(colony.Shipyard.IsBuilding))
        {
            BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
            if (project != null)
            {
                colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                _text = "Step_5386:; ShipProduction "
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

    // not HomeSector or especially SeatOfGovernment
    if (colony.Sector != homeSector && colony.Shipyard != null)
    {
        _text = "Step_5390:; " + colony.Location + "next: check for ShipProduction - not at HomeSector: "
            + colony.Shipyard.Design
            + " - " + colony.Owner
            + " at " + colony.Location + " " + colony.Name
            + " - here no ship is building - maybe on the next code"
            ;
        Console.WriteLine(_text);
        //CheckForSystemsToColonizeProject(colony);
    }


    // this builds a colonizer - why only colonizer ?
    if (colony.Shipyard.BuildSlots.Any(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
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

        ShipType _neededShipType;

        _neededShipType = ShipType.Construction; // here more code to do

        BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
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
        _text = "Step_5387:; " + colony.Location + " " + colony.Name
            + ", ShipProduction > " + item.Project.BuildDesign
            + ", TurnsRemaining= " + item.Project.TurnsRemaining


            ;
        Console.WriteLine(_text);
    }

    colony.Shipyard.ProcessQueue();
}

private static void CheckForSystemsToColonizeProject(Colony colony)
{
    CivilizationManager civM = GameContext.Current.CivilizationManagers[colony.Owner.CivID];
    // need a fleet for getting a range for IsSectorWithinFuelRange
    Fleet fleet = GameContext.Current.Universe.FindOwned<Fleet>(colony.Owner).Where(f => f.IsColonizer).FirstOrDefault();
    if (fleet == null)
        return;

    _text = "Step_5393:; " + fleet.Location + " using " + fleet.Ships[0].ObjectID + " " + fleet.Ships[0].Design + " > CheckForSystemsToColonizeProject..."
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

    //neededColonizer = possibleSystems.Count;
    civM.ShipColonyNeeded = possibleSystems.Count;

    foreach (var item in possibleSystems)
    {
        _text = "Step_5396:; " + colony.Location + " ShipProduction at " + colony.Name
            + " - possible: " + possibleSystems.Count
            + " - inhabited ? > " + item.HasColony //" for HasColony"


            + " > at " + item.Location
            + " - " + item.Owner
            ;
        Console.WriteLine(_text);
    }
}

#pragma warning disable IDE0051 // Remove unused private members
private static void HandleShipx_OFF_ProductionMinor(Colony colony, Civilization civ)
#pragma warning restore IDE0051 // Remove unused private members
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