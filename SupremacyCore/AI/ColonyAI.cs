// File:ColonyAI.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using Obtics.Collections;
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
using System.Diagnostics;
using System.Linq;

namespace Supremacy.AI
{
    public static class ColonyAI
    {
        private const int NumScouts = 1;
        //private const int ColonyShipEveryTurns = 2;
        private const int ColonyShipEveryTurnsMinor = 5;
        private const int MaxMinorColonyCount = 3;

        //private static int neededColonizer;
        //private const int MaxEmpireColonyCount = 999; // currently not used

        static ShipType needed_ShipType_1 = new ShipType();
        static ShipType needed_ShipType_2 = new ShipType();// in case two slots are free in one turn

        [NonSerialized]
        private static string _text;
        //private static string GameEngine.LocationString(colony.Location.ToString());
        private static string _owner_col;
        private static string _name_col;
        private static string _net_industry_text;
        private static string _colony_full_Report;
        private static string newline = Environment.NewLine;
        private static bool boolCheckShipProduction = true;
        private static bool boolCheckColonyProduction = true;
        private static bool _bool_listPrioShipBuild_Empty = false;
        private static bool _colonyAIControlled = false;
        private static bool _shipOrderIsDone = false;
        private static BuildProject _itemToBuild;
        private static BuildProject _itemToBuild_Facility; // Food, Industry etc
        private static bool writeDirectly_Colony = true;
        private static string _civ_text_ColonyAI;
        private static ProductionCategory _available_item_Category;


        //private static bool needed_1_done;
        //private static bool needed_2_done;
        private static readonly string blank = " ";


        public static void DoTurn([NotNull] Civilization civ)
        {
            CivilizationManager civM = GameContext.Current.CivilizationManagers[civ.CivID];
            //string _col_location = ClientApp.;

            _text = newline + "Step_1101:; ColonyAI.DoTurn begins... for > " + civ.Key
                + ": Deu=" + civM.Resources.Deuterium.CurrentValue
                + ", Dur=" + civM.Resources.Duranium.CurrentValue
                + ", Dil=" + civM.Resources.Dilithium.CurrentValue
                ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _civ_text_ColonyAI = _text;


            //int _required_Energy = 50 + (50 * civM.AverageTechLevel);

            // foreach colony
            foreach (Colony colony in GameContext.Current.Universe.FindOwned<Colony>(civ.CivID))
            {
                try
                {
                    // checkcolony

                    _name_col = colony.Name;
                    _owner_col = colony.Owner.Key;
                    _net_industry_text = GameEngine.Do_4_Digit(colony.NetIndustry.ToString());

                    _colony_full_Report = _civ_text_ColonyAI; // newline; // new one for each colony

                    _itemToBuild = null;
                    _itemToBuild_Facility = null;

                    var _all_Build_Projects = TechTreeHelper.GetBuildProjects(colony);

                    Print_all_Build_Projects(_all_Build_Projects); // print to Debug output = console

                    _text += blank; // dummy - please keep

                    //_text = newline + "Step_1103:; " + GameEngine.LocationString(colony.Location.ToString()) + " * " + _name_col + " (ID=" + colony.ObjectID
                    //    + ") * > Handling colony... > "
                    //    + " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues >"
                    //    ;

                    //if (writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += newline + _text;


                    //if (MaxEmpireColonyCount == 999)
                    //    _text = "";//nothing - just for dummy



                    //if (colony.BuildQueue.Count > 0)
                    //{
                    //    _text = "Step_1104:; " + GameEngine.LocationString(colony.Location.ToString()) + blank
                    //            + _name_col + " ; " + _owner_col
                    //            + "; Handling.. "
                    //            + "; already building; " + colony.BuildQueue[0].Project.Description

                    //    ;


                    //    for (int i = 0; i < colony.BuildQueue.Count; i++)
                    //    {
                    //        _text += newline + "Step_1431:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //            + " " + _name_col + " ; " + _owner_col
                    //            + ", BuildQueue # " + i + " > " + colony.BuildQueue[0].Description
                    //            + ", needs " + colony.BuildQueue[0].TurnsRemaining + " Turns "

                    //            //+ newline
                    //            ;
                    //    }

                    //}
                    //else
                    //{
                    //    _text = "Step_1432:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //            + " " + _name_col + " ; " + _owner_col
                    //            + " > BuildQueue is empty BEFORE Handling..."

                    //            ;
                    //}


                    //if (writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += newline + _text;


                    //if (boolCheckColonyProduction)
                    //    _text = ""; // just for breakpoint

                    // checkcolony
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 1 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 2 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 3 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 4 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 5 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 6 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 7 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 8 // "CheckCardassia"
                    //if (_name_col == "Portas") Debugger.Break(); // Colony 9 // "CheckCardassia"


                    if (colony.Owner.IsHuman)
                    {
                        Print_Colony_Owner_IsHuman(colony);
                        //_colonyAIControlled = false;
                        //_colonyAIControlled = true;
                        //_text = /*newline +*/ "Step_1102:; " + GameEngine.LocationString(colony.Location.ToString()) + " * " + _name_col + " " + _owner_col
                        //    + " * > AIcontrolled= " + _colonyAIControlled // + " ) > Handling colony"
                        //    + "; BuildQueue.Count= " + colony.BuildQueue.Count // + " ) > Handling colony"
                        //                                                       //+ " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues "
                        //    ;
                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += newline + _text;
                    }

                    if (boolCheckColonyProduction)
                        _text = ""; // just for breakpoint

                    if (_name_col == "Portas") // Colony 1 // "CheckCardassia"
                        _text = ""; // just for breakpoint

                    //if (_name_col == "Terra")  // Colony 2
                    //    _text = ""; // just for breakpoint

                    //if (_name_col == "B'Omar")  // Colony 3
                    //    _text = ""; // just for breakpoint

                    //if (_name_col == "Qo'noS")  // Colony 4
                    //    _text = ""; // just for breakpoint

                    // which Colony > see Console Output

                    // next_Check / set Breakpoint
                    colony.ProcessQueue();
                    CheckPopulation(colony);  // + Handle_Energy_Production(colony); // done inside CheckPopulation
                    Handle_Food_Production(colony);

                    //// just for info
                    //var most_expensive_Project_Available = TechTreeHelper
                    //    .GetBuildProjects(colony).ToList()
                    //    //.OfType<StructureBuildProject>()
                    //    //.Where(p =>
                    //    //        p.GetCurrentIndustryCost() > 0
                    //    //        && EnumHelper
                    //    //            .GetValues<ResourceType>()
                    //    //            .Where(availableResources.ContainsKey)
                    //    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    //    .OrderBy(p => p.BuildDesign.BuildCost).LastOrDefault()

                    //    ;
                    ////foreach (var item in most_expensive_Project_Available)
                    ////{
                    ////var item = null;
                    //if (most_expensive_Project_Available != null)
                    //{
                    //    _itemToBuild = most_expensive_Project_Available;

                    //    //_itemToBuild = item; // preset: the most expensive

                    //    _text = "Step_2235:; " + GameEngine.LocationString(colony.Location.ToString()) + " MOST Expensive Available"
                    //                + " on; " + _name_col
                    //                + "; " + _owner_col

                    //                + "; costs= " + _itemToBuild.IndustryRemaining
                    //                + "; Turns needed: " + _itemToBuild.TurnsRemaining
                    //                + "; for " + _itemToBuild.BuildDesign.Key
                    //                ;
                    //    //if (writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += newline + _text;
                    //}

                    if (colony.BuildQueue.Count > 0) Build_Queue_Clean(colony);

                    Build_Queue_Print(colony);

                    //_text = /*newline + */"Step_1418:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //        + " > " + _name_col + " ; " + _owner_col
                    //        + " > colony.BuildQueue.Count= " + colony.BuildQueue.Count
                    //        //+ "ID= " + _designID_string
                    //        //+ " = " + _available_item.BuildDesign
                    //        //+ ", calc by maxPop= " + colony.Population_Max / 100
                    //        ;
                    //for (int i = 0; i < colony.BuildQueue.Count; i++)
                    //{
                    //    _text += newline + "Step_1419:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //        + " > " + _name_col + " ; " + _owner_col 
                    //        + " > Buildqueue " + i + " > " + colony.BuildQueue[i].Description;
                    //}

                    //if (writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += newline + _text;

                    if (colony.BuildQueue.Count < 2)  // ColonyAI ..foreach colony
                    {
                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += newline + _text;

                        //_text = "Step_2349:; " + GameEngine.LocationString(colony.Location.ToString())
                        //        + " Pop= " + colony.Population + " of " + colony.Population_Max
                        //        + ", Active: Food= " + colony.Facilities_Active1_Food + " of " + colony.Facilities_Active1_Food
                        //        + ", Ind= " + colony.Facilities_Active2_Industry + " of " + colony.Facilities_Active2_Industry
                        //        + ", En= " + colony.Facilities_Active3_Energy + " of " + colony.Facilities_Active3_Energy
                        //        + ", Res= " + colony.Facilities_Active4_Research + " of " + colony.Facilities_Active4_Research
                        //        + ", Int= " + colony.Facilities_Active5_Intelligence + " of " + colony.Facilities_Active5_Intelligence
                        //        + ", Pool= " + colony.GetAvailableLabor() / 10
                        //        + " for " + _name_col
                        //        ;
                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += newline + _text;



                        // next_Check / set Breakpoint
                        if (_colonyAIControlled)  // not for human player
                        {
                            Build_for_LaborPool(colony, civ); // this first
                            Handle_Upgrades(colony, civ);
                            Handle_Basic_Structures(colony, civ);
                            Handle_Buildings(colony, civ);
                            Handle_Additional_Structures(colony, civ);
                            Checkfor_All_Build_Projects(colony);
                            Handle_Build_Anything(colony, civ);
                            Handle_Flex_Production(colony, civ); // older code


                        }

                        colony.ProcessQueue();

                        //if (_colonyAIControlled)  // not for human player
                        //{
                        //    Handle_Buy_Build(colony, civ);
                        //    Handle_Industry_Production(colony);
                        //    //Handle_Research_Distribution(colony);
                        //}


                        Handle_Labors(colony); // fills up (if possible): Industry - Research - Intelligence - Fodd (Energy is done before)

                        Print_Labors(colony, colony.AvailableLabor, colony.AvailableLabor / 10);
                        //_text = "Step_2351:; " + GameEngine.LocationString(colony.Location.ToString())
                        //        + " Pop= " + colony.Population + " of max " + colony.Population_Max
                        //        + ", Active: Food= " + colony.Facilities_Active1_Food + " of " + colony.Facilities_Active1_Food
                        //        + ", Ind= " + colony.Facilities_Active2_Industry + " of " + colony.Facilities_Active2_Industry
                        //        + ", En= " + colony.Facilities_Active3_Energy + " of " + colony.Facilities_Active3_Energy
                        //        + ", Res= " + colony.Facilities_Active4_Research + " of " + colony.Facilities_Active4_Research
                        //        + ", Int= " + colony.Facilities_Active5_Intelligence + " of " + colony.Facilities_Active5_Intelligence
                        //        + ", Pool= " + colony.GetAvailableLabor() / 10
                        //        + " for " + _name_col
                        //        ;
                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += newline + _text;

                        if (colony.BuildQueue.Count > 0) // not to often 
                        {
                            //Build_Queue_Print(colony); 
                        }
                        else
                        {
                            _text = "Step_1432:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                                    + " > " + _name_col + " ; " + _owner_col
                                    + " > BuildQueue is empty BEFORE Handling..."
                                    ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                        }


                        int count = 0;
                        foreach (BuildQueueItem buildQueueItem in colony.BuildQueue) // just > Console.WriteLine
                        {
                            _text = "Step_1206:; " + GameEngine.LocationString(colony.Location.ToString()) + " > " + _name_col
                                + "; needs " + GameEngine.Do_2_Digit(buildQueueItem.Project.TurnsRemaining.ToString()) + " turns "
                                + "; buildQueueItem # " + count + " = " + buildQueueItem.Description

                                    //+ buildQueueItem.Description
                                    ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                            //GameLog.Client.ProductionDetails.DebugFormat(_text);
                            count++;
                        }

                        if (colony.BuildQueue.Count > 0) // not to often 
                        {
                            //Build_Queue_Print(colony); 
                        }
                        else
                        {
                            _text = "Step_1433:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                                    + " > " + _name_col + " ; " + _owner_col
                                    + " > BuildQueue is empty BEFORE Handling..."
                                    ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                        }

                        if (_colonyAIControlled)  // not for human player
                        {
                            Handle_Buy_Build(colony, civ);
                            Handle_Industry_Production(colony);
                            //Handle_Research_Distribution(colony);

                            Handle_Nothing_to_Build(colony);
                        }

                        if (colony.Shipyard != null)
                        {
                            CheckFor_ShipProduction(colony);
                        }
                        else
                        {
                            _text = "Step_1437:; " + GameEngine.LocationString(colony.Location.ToString()) + " > " + _name_col
                                    + " > has no Shipyard"
                                            ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }

                    } // end of foreach colony
                    //}
                }// end of try
                catch
                {
                    _text = "Step_1105:; ### Problem at ColonyAI.DoTurn ..." + colony.Name;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                }// end of catch



                _text = "Step_1107:; " + GameEngine.LocationString(colony.Location.ToString()) + " Colony is done..................";
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                Console.WriteLine(newline + "Colony_Full_Report > " // Output
                    + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col + " " + _owner_col
                    /*+ newline*/ + _colony_full_Report
                     + "end of > Colony_Full_Report"
                     + newline);
            }// end of 
            _text = "Step_1109:; Finish of ColonyAI.DoTurn ";
            if (writeDirectly_Colony) Console.WriteLine(_text);
            //Console.WriteLine(newline + newline + "_colony_full_Report" + newline + newline + _colony_full_Report + newline + "End of _colony_full_Report");
        
        } // End of Main "Do Turn"


        private static void Handle_Flex_Production(Colony colony, Civilization civ)
        {
            _text = /*newline + */"Step_1209:; " + GameEngine.LocationString(colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Check for Handle_Flex_Production (older code): "
                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    + ", colony.AvailableLabor= " + colony.AvailableLabor
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;

            if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                if (flexLabors > 0)
                {
                    // if Ind +2 < Research + Intel > build one research
                    if (colony.GetTotalFacilities(ProductionCategory.Industry) + 2 <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                        _text = "Step_1242:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > ordered one more facility for > Industry"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                    else
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                        _text = "Step_1244:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > ordered one more facility for > Research"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;

                        if (colony.GetTotalFacilities(ProductionCategory.Intelligence) + 1 < (colony.GetTotalFacilities(ProductionCategory.Research) + 2) / 2)
                        {
                            // As well build Intel  > about half of Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Intelligence))));
                            _text = "Step_1245:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                    + _name_col + ", Owner= " + _owner_col
                                    + " > ordered one more facility for > Intelligence"
                                    ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                    }
                }
            }

            //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
            //{
            //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
            //}
        }

        private static void Checkfor_All_Build_Projects(Colony colony)
        {
            var _all_Build_Projects = TechTreeHelper.GetBuildProjects(colony);

            Print_all_Build_Projects(_all_Build_Projects); // print to Debug output = console

            foreach (var _available_item in _all_Build_Projects)
            {

                _text = "Step_1430:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                        + " > " + _name_col + " " + _owner_col
                        + " > _available_item= " + _available_item.BuildDesign
                        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                // next_Check / set Breakpoint
                switch (_available_item.BuildDesign.Key) // Do NOT Build due to Morale MINUS
                {
                    case "CARD_CENTRAL_HOSPITAL":
                    case "CARD_LABOUR_CAMP":
                    case "CARD_STRIP_MINING_OPERATION":
                    case "KLING_MINING_PRISON":
                    case "AKRITIRIAN_PRISON_SATELLITE":
                    case "MALON_HEAVY_RECYCLING_PLANT":
                    case "MOKRA_WORKSHOPS":
                    case "QUARREN_RECRUITMENT_COMPOUND":
                    case "RAKHARI_MINISTRY_OF_JUSTICE":
                    case "TZENKETHI_THE_AUTARCHS_STRONGHOLD":
                    case "VISSIAN_COGENITOR_FOUNDATION":
                        //case "CARD_CENTRAL_HOSPITAL":
                        //case "CARD_CENTRAL_HOSPITAL":
                        continue;
                        //default:
                        //;
                }

                // Build as a prior
                switch (_available_item.BuildDesign.Key)
                {
                    //case "SUBSPACE_SCANNER": // not so important to build first
                    case "SOLAR_ARRAY": // this is mostly needed
                    case "WIND_TURBINES": // this is mostly needed
                    case "CHARGE_COLLECTORS": // this is mostly needed
                    case "THERMAL_TETHER": // this is mostly needed
                    case "HEALTH_CORE": // this is mostly needed
                    case "IMMUNOLOGY_CORE": // this is mostly needed

                        _itemToBuild_Facility = _available_item;
                        break;
                        //default:
                        //;
                }

                if (_available_item.Description.Contains("SHIPYARD"))
                {
                    _itemToBuild_Facility = _available_item;
                }



                try
                {                                // important, otherwise there are crashes

                    if (_available_item.BuildDesign.EncyclopediaCategory != Encyclopedia.EncyclopediaCategory.Facilites)
                    {
                        _available_item_Category = ProductionCategory.Intelligence; // just as a dummy
                        goto SkipFacilities_1;
                    }
                    else
                    {
                        _available_item_Category = GameContext.Current.TechDatabase.ProductionFacilityDesigns[_available_item.BuildDesign.DesignID].Category;
                    }
                }
                catch
                {
                    _available_item_Category = ProductionCategory.Intelligence;
                }

                switch (_available_item_Category)
                {
                    case ProductionCategory.Food:
                        if (_itemToBuild_Facility == null) CheckFor_Food_Facility(colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Industry:
                        if (_itemToBuild_Facility == null) CheckFor_Industry_Facility(colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Energy:
                        continue;
                    case ProductionCategory.Research:
                        if (_itemToBuild_Facility == null) CheckFor_Research_Facility(colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Intelligence:
                        if (_itemToBuild_Facility == null) CheckFor_Intelligence_Facility(colony, _available_item, _available_item_Category);
                        continue;
                        //default:
                        //    break;
                }

            //ProductionCategory.Food
            //if (_itemToBuild_Facility == null && _available_item_Category == ProductionCategory.Food)
            //{
            //    CheckFor_Food_Facility(colony, _available_item, _available_item_Category);
            //}


            ////ProductionCategory.Industry
            ////int _required_Energy = 50 + (50 * civM.AverageTechLevel);
            //if (_itemToBuild_Facility == null && _available_item_Category == ProductionCategory.Industry)
            //{
            //    CheckFor_Industry_Facility(colony, _available_item, _available_item_Category);

            //}


            ////ProductionCategory.Research
            //if (_itemToBuild_Facility == null && _available_item_Category == ProductionCategory.Research)
            //{
            //    CheckFor_Research_Facility(colony, _available_item, _available_item_Category);
            //    //_text = "Step_1424:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Research on; "*/
            //    //         + " > " + _name_col + " ; " + _owner_col
            //    //         + " >  Check for Research > "
            //    //         + "current " + colony.Facilities_Total4_Research
            //    //         + ", calc by maxPop= " + colony.Population_Max / 50
            //    //         ;

            //    //if (writeDirectly_Colony) Console.WriteLine(_text);
            //    //_colony_full_Report += newline + _text;

            //    //if (colony.Facilities_Total4_Research < colony.Population_Max / 50) // each 50 pop = 1 research = 20%
            //    //{
            //    //    _itemToBuild_Facility = _available_item;
            //    //}
            //    //else
            //    //{
            //    //    colony.RemoveFacility(ProductionCategory.Research);
            //    //    _text = "Step_1434:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
            //    //        + " " + _name_col + " ; " + _owner_col
            //    //        + " >  Check for Research > "
            //    //        + "current " + colony.Facilities_Total4_Research
            //    //        + ", calc by maxPop= " + colony.Population_Max / 100
            //    //        + " > removed ONE facility "
            //    //        ;
            //    //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //    //    _colony_full_Report += newline + _text;
            //    //}
            //}

            ////ProductionCategory.Intelligence
            //if (_itemToBuild_Facility == null && _available_item_Category == ProductionCategory.Intelligence)
            //{
            //    CheckFor_Intelligence_Facility(colony, _available_item, _available_item_Category);
            //    //_text = "Step_1425:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
            //    //     + " > " + _name_col + " ; " + _owner_col
            //    //     + " >  Check for Intelligence > "
            //    //              + "current " + colony.Facilities_Total5_Intelligence
            //    //     + ", calc by maxPop= " + colony.Population_Max / 100
            //    //     ;

            //    //if (writeDirectly_Colony) Console.WriteLine(_text);
            //    //_colony_full_Report += newline + _text;

            //    //if (colony.Facilities_Total5_Intelligence < colony.Population_Max / 100) // each 100 pop = 1 intel = 10%
            //    //{
            //    //    _itemToBuild_Facility = _available_item;
            //    //}
            //    //else
            //    //{
            //    //    colony.RemoveFacility(ProductionCategory.Intelligence);
            //    //    _text = "Step_1435:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
            //    //            + " " + _name_col + " ; " + _owner_col
            //    //            + " >  Check for Intelligence > "
            //    //            + "current " + colony.Facilities_Total5_Intelligence
            //    //            + ", calc by maxPop= " + colony.Population_Max / 100
            //    //            + " > removed ONE facility "
            //    //            ;
            //    //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //    //    _colony_full_Report += newline + _text;
            //    //}
            //}

            SkipFacilities_1:

                // Build as a Prio
                if (_available_item.BuildDesign.Key.Contains("SOLAR_ARRAY")
                    || _available_item.BuildDesign.Key.Contains("WIND_TURBINES")
                    || _available_item.BuildDesign.Key.Contains("CHARGE_COLLECTORS")
                    || _available_item.BuildDesign.Key.Contains("THERMAL_TETHER")
                    || _available_item.BuildDesign.Key.Contains("HEALTH_CORE")
                    || _available_item.BuildDesign.Key.Contains("IMMUNOLOGY_CORE")
                    || _available_item.BuildDesign.Key.Contains("SHIPYARD")
                    || _available_item.BuildDesign.Key.Contains("MOON_HABITATION")
                    || _available_item.BuildDesign.Key.Contains("DEUTERIUM_EXTRACTOR")
                    || _available_item.BuildDesign.Key.Contains("DURANIUM_MINE")
                    || _available_item.BuildDesign.Key.Contains("DILITHIUM_REFINERY")
                    || _available_item.BuildDesign.Key.Contains("AQUATIC_DEUTERIUM_PLANT")

                    )
                {
                    foreach (var item in colony.BuildQueue)
                    {
                        if (item.Project.BuildDesign != _available_item.BuildDesign)
                        {
                            _itemToBuild_Facility = _available_item; // build instead of a facility
                        }
                    }
                }

                //if (_itemToBuild_Facility == null && _available_item.BuildDesign.Key.Contains("Battery"))
                //{
                //    _itemToBuild_Facility = _available_item;
                //} // if nothing yet build Battery or next: better...


                if (_itemToBuild_Facility != null && _colonyAIControlled)
                {
                    _itemToBuild = _itemToBuild_Facility;
                }

                _text = "Step_1439:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Checkfor_All_Build_Projects on; "*/
                                 + " > " + _name_col + "; " + _owner_col;
                if (_itemToBuild != null)
                {
                    _text += " > _itemToBuild= " + _itemToBuild
                                     + "; IndustryRemaining= " + _itemToBuild.IndustryRemaining
                                     + "; TurnsRemaining= " + _itemToBuild.TurnsRemaining
                                     ;
                }
                else
                {
                    _text += " > no _itemToBuild, BuildQueue.Count= " + colony.BuildQueue.Count;
                    //Build_Queue_Print(colony);
                }

                if (writeDirectly_Colony) Console.WriteLine(_text);
            }

            //_text = "Step_1109:; Finish of ColonyAI.DoTurn ";
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            ////Console.WriteLine(newline + newline + "_colony_full_Report" + newline + newline + _colony_full_Report + newline + "End of _colony_full_Report");

        } // End of DoTurn

        private static void CheckFor_ShipProduction(Colony colony)
        {
            CivilizationManager civM = GameContext.Current.CivilizationManagers[colony.Owner];

            civM.ShipsOrdered_Check();

            // Ship Building
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

            Handle_Ship_Production(colony, colony.Owner, _listPrioShipBuild_tmp);
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

        private static void CheckFor_Intelligence_Facility(Colony colony, BuildProject available_item, ProductionCategory available_item_Category)
        {
            int _intelligencePerPop = colony.Population_Max / 100;

            _text = "Step_1425:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " > Check for Intelligence > "
                              + "current " + colony.Facilities_Total5_Intelligence
                     + ", calc by maxPop= (max) " + _intelligencePerPop
                     ;

            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;

            if (colony.Facilities_Total5_Intelligence - 2 < _intelligencePerPop) // each 100 pop = 1 intel = 10%
            {
                _itemToBuild_Facility = available_item;
            }

            if (colony.Facilities_Total5_Intelligence + 1 > _intelligencePerPop) // x over calc is ok
            {
                colony.RemoveFacility(ProductionCategory.Intelligence);
                _text = "Step_1435:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Check for Intelligence > "
                        + "current " + colony.Facilities_Total5_Intelligence
                        + ", calc by maxPop= (max) " + _intelligencePerPop
                        + " > removed ONE facility "
                        ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                // do NOT while because Total is not updated > we do one per turn
                if (colony.Facilities_Total5_Intelligence - 3 > _intelligencePerPop)
                {
                    colony.RemoveFacility(ProductionCategory.Intelligence);
                    _text = "Step_1437:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                            + " > " + _name_col + " ; " + _owner_col
                            + " > Check for Intelligence > "
                            + "current " + colony.Facilities_Total5_Intelligence
                            + ", calc by maxPop= (max) " + _intelligencePerPop
                            + " > removed an amount of facilities "
                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }
            }
        }

        private static void CheckFor_Research_Facility(Colony colony, BuildProject available_item, ProductionCategory available_item_Category)
        {
            int _researchEachPop = 50;
            int _researchCalc = colony.Population_Max / _researchEachPop;

            _text = "Step_1424:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Research on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " >  Check for Research > "
                     + "current " + colony.Facilities_Total4_Research
                     + ", calc by maxPop= (max) " + _researchCalc
                     ;

            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

            if (colony.Facilities_Total4_Research - 2 < _researchCalc) // each 50 pop = 1 research = 20%
            {
                _itemToBuild_Facility = available_item;
            }

            if (colony.Facilities_Total4_Research + 1 > _researchCalc) // 1 over calc is ok
            {
                if (colony.Population.CurrentValue + 70 > colony.Population_Max)
                {
                    colony.RemoveFacility(ProductionCategory.Research);
                    _text = "Step_1434:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Research on; "*/
                        + " > " + _name_col + "; " + _owner_col
                        + " >  Check for Research > "
                        + "current " + colony.Facilities_Total4_Research
                        + ", calc by maxPop= (max) " + _researchCalc
                        + " > removed ONE facility "
                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }
            }
        }

        private static void Print_Colony_Owner_IsHuman(Colony colony)
        {
            _colonyAIControlled = true;
            _text = /*newline +*/ "Step_1102:; " + GameEngine.LocationString(colony.Location.ToString()) + " *** " + _name_col + " " + _owner_col
                + " * > AIcontrolled= " + _colonyAIControlled // + " ) > Handling colony"
                + "; BuildQueue.Count= " + colony.BuildQueue.Count // + " ) > Handling colony"
                                                                   //+ " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues "
                ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;
        }

        private static void CheckFor_Industry_Facility(Colony colony, BuildProject available_item, ProductionCategory available_item_Category)
        {
            //ProductionCategory.Industry
            if (_itemToBuild_Facility == null && available_item_Category == ProductionCategory.Industry)
            {
                _text = "Step_1422:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                     + " > " + _name_col + "; " + _owner_col
                     + " >  Check for Industry"
                              + ", current " + colony.Facilities_Total2_Industry
                     + ", calc by maxPop= (max) " + colony.Population_Max / 20
                     ;

                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;


                if (colony.Facilities_Total2_Industry - 3 < colony.Population_Max / 20) // each 20 pop = 1 industry = 50%
                {
                    _itemToBuild_Facility = available_item;
                }
                else
                {
                    colony.RemoveFacility(ProductionCategory.Industry);
                    _text = "Step_1522:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " RemoveFacility(ProductionCategory.Industry); "*/
                            + " > " + _name_col + "; " + _owner_col
                            + " >  Check for Industry "
                            + "; current " + colony.Facilities_Total2_Industry
                            + ", calc by maxPop= (max) " + colony.Population_Max / 20
                            + " > removed ONE facility "
                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }
            }
        }
        private static void CheckFor_Food_Facility(Colony colony, BuildProject available_item, ProductionCategory available_item_Category)
        {
            //try
            //{
            _text = "Step_1421:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " >  Check for Food > "
                              + "current colony.NetFood= " + colony.NetFood
                     //+ colony.Facilities_Active1_Food
                     + ", maxPop= " + colony.Population_Max

            ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

            // if food is minus and all are active
            if (colony.NetFood < 0 && colony.Facilities_Active1_Food + 1 > colony.Facilities_Total1_Food)
            {
                _itemToBuild_Facility = available_item;
            }
            //else
            //{
            //    //colony.RemoveFacility(ProductionCategory.Food); // no scratch for food facilities
            //}
        }

        //private static void Handle_Research_Distribution(Colony colony) // this 
        //{
        //    _text = "Step_1631:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
        //            + " > " + _name_col + " ; " + _owner_col
        //            + ", Handle_Research_Distribution " //+ i /*+ " of " + _total*/ + " > " + colony.BuildQueue[i].Description

        //            ;
        //    if (writeDirectly_Colony) Console.WriteLine(_text);
        //    _colony_full_Report += newline + _text;

        //}

        private static void Build_Queue_Clean(Colony colony)
        {
            string _itemToCheck = "";
            int _total = colony.BuildQueue.Count;
            List<BuildProject> _BuildQueueItemsTo_Remove = new List<BuildProject>();
            for (int i = 0; i < _total; i++)
            {
                if (i +1 > colony.BuildQueue.Count)
                    continue;

                _text = "Step_1431:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Build_Queue_Clean; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + ", BuildQueue " + " # " + i + " > " + colony.BuildQueue[i].Description

                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                _itemToCheck = colony.BuildQueue[i].Project.ToString();

                bool isFacility = colony.BuildQueue[i].Project is ProductionFacilityBuildProject;

                if (!isFacility && _itemToCheck != "ShieldGenerator")
                {
                    if (colony.BuildSlots[0].HasProject && colony.BuildSlots[0].Project.BuildDesign.ToString() == _itemToCheck && colony.BuildQueue.Count > 0)
                    {
                        _BuildQueueItemsTo_Remove.Add(colony.BuildQueue[i].Project);
                    }

                    foreach (var _existing in colony.Buildings)
                    {
                        if (_existing.BuildingDesign.ToString() == _itemToCheck && colony.BuildQueue.Count > 0)
                            _BuildQueueItemsTo_Remove.Add(colony.BuildQueue[i].Project);
                    }
                }
                _BuildQueueItemsTo_Remove.Distinct();  // this seems not to work ??
            }

            foreach (var item in _BuildQueueItemsTo_Remove)
            {
                _text = "Step_1439:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Build_Queue_Clean; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + ", _BuildQueueItemsTo_Remove "  + " > " + item.Description

                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                for (int i = 0; i < colony.BuildQueue.Count; i++)
                //{

                //}
                //for (int i = colony.BuildQueue.Count; i < 0; i--)
                {
                    //Console.WriteLine(i); // just_i
                    if (colony.BuildQueue[i].Project.ToString() == item.ToString())
                    {
                        colony.BuildQueue.Remove(colony.BuildQueue[i]);
                    }

                }

                //if (colony.BuildQueue[0].Project.Contains(item))
                //{

                //}

                //colony.BuildQueue.Remove(item);
            }
        }

        private static void Build_Queue_Print(Colony colony)
        {
            int _total = colony.BuildQueue.Count;
            for (int i = 0; i < _total; i++)
            {
                _text = "Step_1411:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + ", BuildQueue " + _total + " > # " + i + " > " + colony.BuildQueue[i].Description

                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;
            }
        }

        private static void Print_all_Build_Projects(IList<BuildProject> all_Build_Projects)
        {
            int count = 0;
            string _all_Build_Projects_Text = "";
            foreach (var item in all_Build_Projects)
            {
                string _designID_string = GameEngine.Do_3_Digit(item.BuildDesign.DesignID.ToString());

                //_text = /*newline + */"Step_1420:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
                //                + " > " + _name_col + " ; " + _owner_col
                //                + " > _available_item or Upgrade  "

                //                + " = " + item.BuildDesign
                //                //+ ", calc by maxPop= (max) " + colony.Population_Max / 100
                //                ;
                _text = "Step_1420:; " + GameEngine.LocationString(item.ProductionCenter.Location.ToString())
                    + " > " + _name_col
                    + "; " + _owner_col
                    + " OPTIONS to Build incl. Upgrades > "
                    //+ "; Morale=; " + colony.Morale
                    //+ "; NetIndustry=;" + colony.NetIndustry
                    + "; BCost=;" + GameEngine.Do_5_Digit(item.BuildDesign.BuildCost.ToString())
                    + "; TurnsNeeded=;" + GameEngine.Do_2_Digit(item.TurnsRemaining.ToString()) // not avaible
                    + "; OPTIONS_to_Build_on #;" + GameEngine.Do_2_Digit(count.ToString())
                    + "; ID= " + _designID_string
                    + "; " + item.BuildDesign.ToString()

                    ;
                count++;
                //if (writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += newline + _text;
                _all_Build_Projects_Text += newline + _text;
            } // end of output
            if (writeDirectly_Colony) Console.WriteLine(_all_Build_Projects_Text);
            _colony_full_Report += newline + _all_Build_Projects_Text;
        }

        //private static void CheckBuildQueueContent(Colony colony)
        //{
        //    _text = /*newline + */"Step_1418:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
        //            + " > " + _name_col + " ; " + _owner_col
        //            + " > Count for BuildQueue= " + colony.BuildQueue.Count
        //            //+ "ID= " + _designID_string
        //            //+ " = " + _available_item.BuildDesign
        //            //+ ", calc by maxPop= " + colony.Population_Max / 100
        //            ;
        //        Build_Queue_Print(colony);
        //    //for (int i = 0; i < colony.BuildQueue.Count; i++)
        //    //{
        //    //    _text += newline + "Step_1419:; " + GameEngine.LocationString(colony.Location.ToString()) /*+ " Check for Food on; "*/
        //    //        + " > " + _name_col + " ; " + _owner_col
        //    //        + " > Buildqueue # " + i + " > " + colony.BuildQueue[i].Description;
        //    //}

        //    //if (writeDirectly_Colony) Console.WriteLine(_text);
        //    //_colony_full_Report += newline + _text;
        //}


        // do we need this still or is it double
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
            int _tmp_Active_All = _tmp_Active1_Food + _tmp_Active2_Industry + _tmp_Active3_Energy + _tmp_Active4_Research + _tmp_Active5_Intelligence;

            int _laborPool = colony.GetAvailableLabor() / 10;

            Print_Labors(colony, _laborPool, _popAvailable);

            //_text = "Step_2345:; " + GameEngine.LocationString(GameEngine.LocationString(colony.Location.ToString()).ToString())
            //        //+ "Turn " + GameContext.Current.TurnNumber
            //        + " > Pool= " + _laborPool
            //            + " vs " + _popAvailable // should be zero

            //        + " ,Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Food)
            //        + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Industry)
            //        + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Energy)
            //        + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Research)
            //        + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Intelligence)
            //    + " for " + _name_col
            //    + " (Checking Population)"
            //    ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;


            while (colony.DeactivateFacility(ProductionCategory.Industry)) { }
            while (colony.DeactivateFacility(ProductionCategory.Research)) { }
            while (colony.DeactivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.DeactivateFacility(ProductionCategory.Food)) { }
            while (colony.DeactivateFacility(ProductionCategory.Energy)) { }

            _laborPool = colony.GetAvailableLabor() / 10;

            // works
            //_text = "Step_2347:; " + GameEngine.LocationString(colony.Location.ToString()) + " All-De-Activated !!, Pop= " + _popAvailable
            //        + ", Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
            //        + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
            //        + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
            //        + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
            //        + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
            //        + ", Pool= " + _laborPool
            //        + " for " + _name_col
            //        ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;
            // -------


            //checked what's going on here setting _popAvai to Zero'
            // >> in some situations (SystemAssault or AsteroidImpact) > pop shrinked heavily and..
            // outputs have to be adapted


            // re-populate energy first
            for (int i = 0; i < colony.Facilities_Total3_Energy; i++)
            {
                while (_popAvailable + _laborPool > 0 && colony.Facilities_Active3_Energy < colony.Facilities_Total3_Energy)  // later another one is added if possible
                {
                    colony.ActivateFacility(ProductionCategory.Energy);
                    _popAvailable -= 1;
                }
            }
            // Check if we need all pop on energy
            // CheckEnergy // next_Check / set Breakpoint

            Handle_Energy_Production(colony);

            //while (colony.NetEnergy - colony.GetFacilityType(ProductionCategory.Energy).UnitOutput > 0)  // later another one is added if possible
            //{
            //    colony.DeactivateFacility(ProductionCategory.Energy);
            //    _popAvailable += 1;
            //}

            // Food 1
            //while (_popAvailable > 0 && colony.FoodReserves.CurrentValue > 1000 && colony.NetFood < -50)
            // CheckFood
            while (_popAvailable > 0 && colony.NetFood < -50)
            {
                _text = "Step_2347:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Pop= " + _popAvailable
                        + ", Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
                        + ", NetFood= " + colony.NetFood
                        //+ ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
                        //+ ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
                        //+ ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
                        //+ ", Pool= " + _laborPool
                        + " for " + _name_col
                        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                colony.ActivateFacility(ProductionCategory.Food);
                _popAvailable -= 1;
            }

            // Food 2 > try to activate another Food one
            if (_popAvailable > 0 && colony.FoodReserves.CurrentValue < 500)
            {
                colony.ActivateFacility(ProductionCategory.Food);
                _popAvailable -= 1;

                //_text = "Step_2348:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Pop= " + _popAvailable
                //        + ", Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
                //        + ", NetFood= " + colony.NetFood
                //        + ", Reserve= " + colony.FoodReserves.CurrentValue
                //        + " for " + _name_col
                //        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += newline + _text;
            }


            // fill in facilities like _tmp = before
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

            // fill up more pop into available facilities
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

            Print_Labors(colony, _laborPool, _popAvailable);

            //_text = "Step_2346:; " + GameEngine.LocationString(colony.Location.ToString())
            //        + " > Pool= " + _laborPool
            //            + " vs " + _popAvailable // should be zero

            //        + " ,Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Food)
            //        + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Industry)
            //        + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Energy)
            //        + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Research)
            //        + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
            //        + " of " + colony.GetTotalFacilities(ProductionCategory.Intelligence)

            //        + " for " + _name_col
            //        + ", Pop now " + colony.Population
            //        + " max " + colony.Population_Max
            //        //+ " (Checking Population ...DONE)"
            //        ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

            while (colony.ActivateFacility(ProductionCategory.Industry)) { }
            while (colony.ActivateFacility(ProductionCategory.Research)) { }
            while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.ActivateFacility(ProductionCategory.Food)) { }
            // next_Check / set Breakpoint
        }

        private static void Print_Labors(Colony colony, int _laborPool, int _popAvailable)
        {
            _text = "Step_2346:; " + GameEngine.LocationString(colony.Location.ToString())
        + " > Pool= " + _laborPool
            + " vs " + GameEngine.Do_2_Digit(_popAvailable.ToString()) // should be zero

        + " ,Active: Food= " + colony.GetActiveFacilities(ProductionCategory.Food)
        + " of " + colony.GetTotalFacilities(ProductionCategory.Food)
        + ", Ind= " + colony.GetActiveFacilities(ProductionCategory.Industry)
        + " of " + colony.GetTotalFacilities(ProductionCategory.Industry)
        + ", En= " + colony.GetActiveFacilities(ProductionCategory.Energy)
        + " of " + colony.GetTotalFacilities(ProductionCategory.Energy)
        + ", Res= " + colony.GetActiveFacilities(ProductionCategory.Research)
        + " of " + colony.GetTotalFacilities(ProductionCategory.Research)
        + ", Int= " + colony.GetActiveFacilities(ProductionCategory.Intelligence)
        + " of " + colony.GetTotalFacilities(ProductionCategory.Intelligence)

        + " for " + _name_col
        + ", Pop now " + colony.Population
        + " max " + colony.Population_Max
        //+ " (Checking Population ...DONE)"
        ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;
        }

        private static void Handle_Energy_Production(Colony colony)
        {
            _text = "Step_1229:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle ENERGY on; "
                    + _name_col + " ; " + _owner_col
                    ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

            double energyOutput = colony.GetFacilityType(ProductionCategory.Energy).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Energy).Efficiency);
            List<Buildings.Building> offlineBuilding = colony.Buildings.Where(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0).ToList();
            List<ShipyardBuildSlot> offlineShipyardSlots = colony.Shipyard == null ? new List<ShipyardBuildSlot>() : colony.Shipyard.BuildSlots.Where(s => !s.IsActive).ToList();
            int netEnergy = colony.NetEnergy - offlineBuilding.Sum(b => b.BuildingDesign.EnergyCost) - offlineShipyardSlots.Sum(s => s.Shipyard.ShipyardDesign.BuildSlotEnergyCost);

            while (colony.DeactivateFacility(ProductionCategory.Industry)) { } // take it from industry and...

            while (colony.ActivateFacility(ProductionCategory.Energy)) { } // turn on all energy facilities

            // next_Check / set Breakpoint
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
            // next_Check / set Breakpoint
            if ((colony.Buildings.Any(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0)
                || (colony.Shipyard?.BuildSlots.Any(s => !s.IsActive) == true)) && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));

                _text = "Step_1248:; " + GameEngine.LocationString(colony.Location.ToString()) + blank + _name_col + " " + _owner_col + " Handle ENERGY "


                    + " > added 1 ENERGY Facility Build Order"
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;
            }

            SetFacility(colony, ProductionCategory.Energy, netEnergy, energyOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry, ProductionCategory.Food });


            // do NOT do a while here > NetEnergy is not updated > so we do it > one per turn
            if (colony.NetEnergy - (int)energyOutput > 0)  // later another one is added if possible
            {
                colony.DeactivateFacility(ProductionCategory.Energy);
                //_popAvailable += 1;
            }


            // labors might be unchanced
            Print_Labors(colony, colony.AvailableLabor, colony.AvailableLabor / 10);

            _text = "Step_1259:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle ENERGY on "
                    + _name_col + " " + _owner_col
                    + " >>> netEnergy= " + netEnergy
                    + ", offlineBuilding= " + offlineBuilding.Where(b => b.IsActive == false).Count()
                    + ", offlineShipyardSlots= " + offlineShipyardSlots.Count
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;



        } // End of Handle_Energy_Production(Colony colony)

        private static void Handle_Food_Production(Colony colony)
        {


            double foodOutput = colony.GetFacilityType(ProductionCategory.Food).UnitOutput * (1.0 + colony.GetProductionModifier(ProductionCategory.Food).Efficiency);
            //double neededFood = colony.NetFood + colony.FoodReserves.CurrentValue - (10 * foodOutput);
            double neededFood = colony.Population.CurrentValue - foodOutput;

            //SetFacility(colony, ProductionCategory.Food, (int)neededFood, foodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry });

            double maxFoodProduction = colony.GetProductionModifier(ProductionCategory.Food).Bonus + (colony.GetTotalFacilities(ProductionCategory.Food) * foodOutput);

            _text = "Step_1220:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle FOOD      on; "
                    + _name_col + " ; " + _owner_col
                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + colony.Population
                    + "; NetFood= " + colony.NetFood
                    + ", Reserve= " + colony.FoodReserves.CurrentValue
                    //+ " > no Upgrade INDUSTRY"
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

            ProductionFacilityDesign facilityType = colony.GetFacilityType(ProductionCategory.Food);
            if (colony.NetFood < 15 && colony.FoodReserves.CurrentValue + 1 / colony.Population.CurrentValue + 1 < 5 && !colony.IsBuilding(facilityType))
            {
                colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, facilityType)));
                _text = "Step_1228:; " + GameEngine.LocationString(colony.Location.ToString()) + blank + _name_col + " " + _owner_col + " > Handle_FOOD_Production "
                    + " > #### added 1 FOOD Facility Build Order"
                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + colony.Population
                    + "; of " + colony.Population_Max
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;
            }
            _text = "Step_1222:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle FOOD is DONE; "
                    + _name_col + " ; " + _owner_col

                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + colony.Population
                    + "; NetFood= " + colony.NetFood
                    + ", Reserve= " + colony.FoodReserves.CurrentValue
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;
            // next_Check / set Breakpoint
        }

        private static void Handle_Industry_Production(Colony colony)
        {
            double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
            int maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, (colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost) + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
            int industryNeeded = colony.BuildSlots.Where(s => s.Project != null).Select(s => s.Project.IsRushed ? 0 : s.Project.GetCurrentIndustryCost()).Sum();
            int turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));
            double facilityNeeded = turnsNeeded == 0 ? 0 : Math.Truncate(((industryNeeded / turnsNeeded) - colony.GetProductionModifier(ProductionCategory.Industry).Bonus) / prodOutput);
            double netIndustry = -(facilityNeeded - colony.ActiveFacilities[ProductionCategory.Industry].Value) * prodOutput;
            //SetFacility(colony, ProductionCategory.Industry, (int)netIndustry, prodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research });
        }

        private static void Handle_Labors(Colony colony)
        {
            while (colony.ActivateFacility(ProductionCategory.Industry)) { }
            while (colony.ActivateFacility(ProductionCategory.Research)) { }
            while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
            while (colony.ActivateFacility(ProductionCategory.Food)) { }
            //_text = "Step_2348:; " + GameEngine.LocationString(colony.Location.ToString())
            //        + " Pop= " + colony.Population.CurrentValue
            //        + ", Active: Food= " + colony.Facilities_Active1_Food
            //        + ", Ind= " + colony.Facilities_Active2_Industry
            //        + ", En= " + colony.Facilities_Active3_Energy
            //        + ", Res= " + colony.Facilities_Active4_Research
            //        + ", Int= " + colony.Facilities_Active5_Intelligence
            //        + ", Pool= " + colony.AvailableLabor
            //        + " for " + _name_col
            //        ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;
            if (boolCheckColonyProduction)
            {
                //Debugger.Break();
            }

        }

        private static void Handle_Nothing_to_Build(Colony colony)
        {
            // we don't touch energy
            if (!colony.BuildSlots[0].HasProject)
            {



                // take away all labors 
                while (colony.DeactivateFacility(ProductionCategory.Industry)) { }
                while (colony.DeactivateFacility(ProductionCategory.Research)) { }
                while (colony.DeactivateFacility(ProductionCategory.Intelligence)) { }
                while (colony.DeactivateFacility(ProductionCategory.Food)) { }


                // re-populate, but Industry last

                while (colony.ActivateFacility(ProductionCategory.Research)) { }
                while (colony.ActivateFacility(ProductionCategory.Intelligence)) { }
                while (colony.ActivateFacility(ProductionCategory.Food)) { }
                while (colony.ActivateFacility(ProductionCategory.Industry)) { }
                //_text = "Step_2348:; " + GameEngine.LocationString(colony.Location.ToString())
                //        + " Pop= " + colony.Population.CurrentValue
                //        + ", Active: Food= " + colony.Facilities_Active1_Food
                //        + ", Ind= " + colony.Facilities_Active2_Industry
                //        + ", En= " + colony.Facilities_Active3_Energy
                //        + ", Res= " + colony.Facilities_Active4_Research
                //        + ", Int= " + colony.Facilities_Active5_Intelligence
                //        + ", Pool= " + colony.AvailableLabor
                //        + " for " + _name_col
                //        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += newline + _text;

            }

            if (boolCheckColonyProduction)
            {
                //Debugger.Break();
            }
        }

        private static void Handle_Buildings(Colony colony, Civilization civ)
        {
            bool _checkHandleBuildings = true;
            _text = _checkHandleBuildings.ToString(); // dummy - please keep
                                                      //bool _checkHandleBuildings = false;
            if (_name_col == "Nadra")
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

            if (boolCheckColonyProduction)
                _text = ""; // just for breakpoint

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
            {
                _text = "Step_1202:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings: "
                    //+ "Credits.Current= " + civM.Credits.CurrentValue
                    //+ ", Costs= " + cost
                    //+ ", industryNeeded= " + industryNeeded
                    //+ ", prodOutput= " + prodOutput.ToString()
                    //+ ", turnsNeeded= " + turnsNeeded
                    //+ " > IsRushed for " + s.Project
                    + " on " + _name_col + " " + _owner_col
                ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                //if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
                if (colony.BuildQueue.Count < 2)
                {
                    //INDUSTRY 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry };
                    int flexLabors = colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > -21)  // 2 more facilites as available labors
                    {
                        _text = "Step_1204:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on INDUSTRY at "
                            + _name_col + " " + _owner_col
                            + " > " + colony.GetAvailableLabor() + " labors available"
                            ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;

                        int totalInd = colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1205:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1246:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
                {
                    //FOOD 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Food };
                    int flexLabors = colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                    {
                        _text = "Step_1204:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on INDUSTRY at "
                            + _name_col + " " + _owner_col
                            + " > " + colony.GetAvailableLabor() + " labors available"
                            ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;

                        int totalInd = colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1255:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1256:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
                {
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                    int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                    if (flexLabors > 0)
                    {
                        _text = "Step_1274:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                            + _name_col + " " + _owner_col
                            + " " + colony.GetAvailableLabor() + " > flexLabors available"
                            ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                        if (colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1275:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                        else
                        {
                            //than Research
                            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                            _text = "Step_1276:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                    }
                }

                if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
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
                        _text = "Step_1213:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > no Upgrade INDUSTRY"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
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

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
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

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
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

            if (colony.BuildSlots.All(t => t.Project == null) && colony.BuildQueue.Count < 2)
            {
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);


                foreach (var item in projects)
                {
                    _text = "Step_1210:; " + GameEngine.LocationString(colony.Location.ToString()) + " Available =  "
                        //+ "Credits.Current= " + civM.Credits.CurrentValue
                        + ", Costs= " + item.BuildDesign
                        + ", industryNeeded= " + item.IndustryRemaining
                        //+ ", prodOutput= " + prodOutput.ToString()
                        + ", turnsNeeded= " + item.TurnsRemaining
                        //+ " > IsRushed for " + s.Project
                        + " on " + _name_col /*+ " " + item.Project.Location*/
                    ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }
            }
        }

        private static void Handle_Basic_Structures(Colony colony, Civilization civ)
        {
            _text = "Step_1231:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Basic_Structures on; "
                    + "" + _name_col + " ; " + _owner_col
                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text + newline;


            colony.ProcessQueue();

            if (colony.BuildQueue.Count > 0)
            {
                _text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Basic_Structures on; "
                        + _name_col + " ; " + _owner_col
                        + "; already building >; " + colony.BuildQueue[0].Description;
                ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += /*newline + */_text;
            }

            if (colony.BuildQueue.Count < 2) // Handle_Basic_Structures
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + colony.Morale
                //    + "; UnitOutput=;" + colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager civM = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => civM.Resources[r.Resource].CurrentValue - r.Used);

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1249:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + " ; for; " + item.Key
                //        //+ "; NetIndustry=;" + colony.NetIndustry
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(colony).ToList()
                //    //.OfType<StructureBuildProject>()
                //    //.Where(p =>
                //    //        p.GetCurrentIndustryCost() > 0
                //    //        && EnumHelper
                //    //            .GetValues<ResourceType>()
                //    //            .Where(availableResources.ContainsKey)
                //    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    //.OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault()
                //    ;
                //foreach (var item in structureProject_Available)
                //{
                //    _text = "Step_1265:; " + GameEngine.LocationString(colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);

                //}

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

                string _toBuildText = " > no StructureProject to build";
                if (structureProject != null)
                {
                    _toBuildText = structureProject.BuildDesign.ToString();
                }


                //_text = "Step_1236:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; NetIndustry=;" + colony.NetIndustry
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1257:; " + GameEngine.LocationString(colony.Location.ToString()) //+ " > Handle_Basic_Structures"
                        + " > " + _name_col
                        + " ; " + _owner_col
                        + "; Morale=; " + colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; NetIndustry=;" + colony.NetIndustry
                        + "; Handle_Basic_Structures=;" + _toBuildText
                        + "; BuildQueue.Count=" + colony.BuildQueue.Count
                    //+ "; MathCeiling=; " + Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)civM.Credits.CurrentValue / 4;

                if (structureProject != null && (int)structureProject.BuildDesign.BuildCost < (civM.Credits.CurrentValue / 4))
                {
                    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                    _text = "Step_1269:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Added to Build"
                            + " on; " + _name_col
                            + "; " + _owner_col
                            + "; Morale=; " + colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; ToBuild=;" + structureProject.BuildDesign.ToString()

                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }


                //                if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)  //2023-11-11
                //                {
                //                    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
                //                    _text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) 
                //                            + " > " + _name_col
                //                            + "; " + _owner_col
                //+ " Added to Build"
                //                            + "; Morale=; " + colony.Morale
                //                            + "; prodOutput=;" + prodOutput
                //                            + "; ToBuildonSlots.Count=;" + projects.Count

                //                            ;
                //                    if (writeDirectly_Colony) Console.WriteLine(_text);
                //                    _colony_full_Report += newline + _text + newline;

                //                    //int count = 0;
                //                    //foreach (var item in projects) // Handle_Basic_Structures
                //                    //{
                //                        Print_all_Build_Projects(projects);
                //                        //_text = "Step_1235:; " + GameEngine.LocationString(colony.Location.ToString()) 
                //                        //    + " > " + _name_col
                //                        //    + "; " + _owner_col
                //                        //    + " OPTIONS to Build > "
                //                        //    + "; Morale=; " + colony.Morale
                //                        //    + "; NetIndustry=;" + colony.NetIndustry
                //                        //    + "; BCost=;" + GameEngine.Do_5_Digit(item.BuildDesign.BuildCost.ToString())
                //                        //    + "; OPTIONS_to_Build_on #;" + count
                //                        //    + "; " + item.BuildDesign.ToString()

                //                        //    ;
                //                        //count++;
                //                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                //                        //_colony_full_Report += newline + _text;
                //                    //}
                //                }
            }
        }

        private static void Build_for_LaborPool(Colony colony, Civilization civ)
        {
            _text = "Step_1266:; " + GameEngine.LocationString(colony.Location.ToString())

                    + " > " + _name_col + " ; " + _owner_col
                    + " > Build_for_LaborPool on; "
                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += /*newline + */_text + newline;


            colony.ProcessQueue();

            int _laborAvailable = colony.AvailableLabor;

            //if (colony.BuildQueue.Count > 0) // already building
            //{
            //    _text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Build_for_LaborPool on; "
            //            + _name_col + " ; " + _owner_col
            //            + "; already building >; " + colony.BuildQueue[0].Description;
            //    ;
            //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //    _colony_full_Report += /*newline + */_text;
            //}

            if (_laborAvailable > 0 && colony.BuildQueue.Count < 4) // Handle_Basic_Structures
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + colony.Morale
                //    + "; UnitOutput=;" + colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager civM = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => civM.Resources[r.Resource].CurrentValue - r.Used);

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1249:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + " ; for; " + item.Key
                //        //+ "; NetIndustry=;" + colony.NetIndustry
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(colony).ToList()
                //    //.OfType<StructureBuildProject>()
                //    //.Where(p =>
                //    //        p.GetCurrentIndustryCost() > 0
                //    //        && EnumHelper
                //    //            .GetValues<ResourceType>()
                //    //            .Where(availableResources.ContainsKey)
                //    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    //.OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault()
                //    ;
                //foreach (var item in structureProject_Available)
                //{
                //    _text = "Step_1265:; " + GameEngine.LocationString(colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);

                //}

                //structureProject
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

                //string _toBuildText = " > no StructureProject to build";
                //if (structureProject != null)
                //{
                //    _toBuildText = structureProject.BuildDesign.ToString();
                //}


                //_text = "Step_1236:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; NetIndustry=;" + colony.NetIndustry
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1237:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Build_for_LaborPool"
                        + " on; " + _name_col
                        + "; " + _owner_col
                        + "; Morale=; " + colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; NetIndustry=;" + colony.NetIndustry
                        //+ "; ToBuild=;" + _toBuildText
                        + "; BuildQueue.Count=" + colony.BuildQueue.Count
                    //+ "; MathCeiling=; " + Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)civM.Credits.CurrentValue / 4;

                //if (structureProject != null && (int)structureProject.BuildDesign.BuildCost < (civM.Credits.CurrentValue / 4))
                //{
                //    colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //    _text = "Step_1269:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Added to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            + "; prodOutput=;" + prodOutput
                //            + "; NetIndustry=;" + colony.NetIndustry
                //            + "; ToBuild=;" + structureProject.BuildDesign.ToString()

                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += newline + _text;
                //}


                //                if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)  //2023-11-11
                //                {
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);


                //                    _text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) 
                //                            + " > " + _name_col
                //                            + "; " + _owner_col
                //+ " Added to Build"
                //                            + "; Morale=; " + colony.Morale
                //                            + "; prodOutput=;" + prodOutput
                //                            + "; ToBuildonSlots.Count=;" + projects.Count

                //                            ;
                //                    if (writeDirectly_Colony) Console.WriteLine(_text);
                //                    _colony_full_Report += newline + _text + newline;

                //                    //int count = 0;
                foreach (var _available_item in projects) // Handle_Basic_Structures
                {
                    if (_available_item.BuildDesign.EncyclopediaCategory != Encyclopedia.EncyclopediaCategory.Facilites)
                    {
                        _available_item_Category = ProductionCategory.Intelligence; // just as a dummy
                        goto SkipFacilities;
                    }

                    try
                    {
                        _available_item_Category = GameContext.Current.TechDatabase.ProductionFacilityDesigns[_available_item.BuildDesign.DesignID].Category;
                    }
                    catch
                    {
                        _available_item_Category = ProductionCategory.Intelligence; // just as a dummy
                    }

                    if (_available_item.IsUpgrade)
                    {
                        continue;
                    }



                    switch (_available_item_Category)
                    {
                        case ProductionCategory.Food:
                            CheckFor_Food_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Industry:
                            CheckFor_Industry_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Energy:
                            //CheckFor_Energy_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Research:
                            CheckFor_Research_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Intelligence:
                            CheckFor_Intelligence_Facility(colony, _available_item, _available_item_Category);
                            break;
                        default:
                            break;
                    }



                    if (_available_item != null && _laborAvailable > 0)
                    {

                        colony.BuildQueue.Add(new BuildQueueItem(_available_item));

                        if (_available_item_Category == ProductionCategory.Industry)
                        {
                            colony.BuildQueue.Add(new BuildQueueItem(_available_item)); // Industry: add 2 ones in one steps, otherwise too less Industry is built
                        }

                        _laborAvailable -= 1;
                        _text = /*newline + */"Step_1299:; " + GameEngine.LocationString(colony.Location.ToString())
                                + " > " + _name_col + " " + _owner_col
                            + " Added= " + _available_item.BuildDesign

                                + " > by using ** Build_for_Labor ** " /*+ colony.BuildQueue.Count*/
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }

                SkipFacilities:;

                    //                        Print_all_Build_Projects(projects);
                    //                        //_text = "Step_1235:; " + GameEngine.LocationString(colony.Location.ToString()) 
                    //                        //    + " > " + _name_col
                    //                        //    + "; " + _owner_col
                    //                        //    + " OPTIONS to Build > "
                    //                        //    + "; Morale=; " + colony.Morale
                    //                        //    + "; NetIndustry=;" + colony.NetIndustry
                    //                        //    + "; BCost=;" + GameEngine.Do_5_Digit(item.BuildDesign.BuildCost.ToString())
                    //                        //    + "; OPTIONS_to_Build_on #;" + count
                    //                        //    + "; " + item.BuildDesign.ToString()

                    //                        //    ;
                    //                        //count++;
                    //                        //if (writeDirectly_Colony) Console.WriteLine(_text);
                    //                        //_colony_full_Report += newline + _text;
                    //                    //}

                }
            }
        }

        private static void Handle_Additional_Structures(Colony colony, Civilization civ)  // unneccesary but still in
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

            if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2) // Handle_Additional_Structure
            {
                _text = "Step_1238:; " + GameEngine.LocationString(colony.Location.ToString())
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Handle_Additional_Structures... "
                        ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
                //{
                //    // Industry Upgrade ?
                //    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                //        .GetBuildProjects(colony)
                //        .OfType<ProductionFacilityUpgradeProject>()
                //        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));

                //    if (upgradeIndustryProject == null)
                //    {
                //        _text = "Step_1203:; " + GameEngine.LocationString(colony.Location.ToString())
                //                + " Handle_Buildings on; "
                //                + _name_col + " " + _owner_col
                //                + " > no Upgrade INDUSTRY"
                //                ;
                //        if (writeDirectly_Colony) Console.WriteLine(_text);
                //        _colony_full_Report += newline + _text;

                //    }
                //    else
                //    {
                //        colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                //    }
                //}

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


                if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2) // Handle_Additional_Structures
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
                        _text = /*newline + */"Step_1297:; " + GameEngine.LocationString(colony.Location.ToString())
                                + " > " + _name_col + " " + _owner_col
                            + " Added= " + structureProject.BuildDesign

                                + " > by using ** Handle_Additional_Structures ** " /*+ colony.BuildQueue.Count*/
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                }

                //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
                //{
                //    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                //    int flexLabors = colony.GetAvailableLabor() + flexProduction.Sum(c => colony.GetFacilityType(c).LaborCost * colony.GetActiveFacilities(c));
                //    if (flexLabors > 0)
                //    {
                //        if (colony.GetTotalFacilities(ProductionCategory.Industry) <= colony.GetTotalFacilities(ProductionCategory.Research) + colony.GetTotalFacilities(ProductionCategory.Intelligence))
                //        {
                //            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Industry))));
                //        }
                //        else
                //        {
                //            colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(colony, colony.GetFacilityType(ProductionCategory.Research))));
                //        }
                //    }
                //}

                //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
                //{
                //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
                //}
            }
        }
        // ----------------------------------------------------

        private static void Handle_Upgrades(Colony colony, Civilization civ)
        {
            _text = /*newline + */"Step_1207:; " + GameEngine.LocationString(colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Check for Handle_Upgrades: "
                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    + ", colony.AvailableLabor= " + colony.AvailableLabor
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;

            colony.ProcessQueue();

            if (colony.AvailableLabor > 0)
            {
                goto NoUpgradesDueToLaborPool;
            }

            if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2 && colony.AvailableLabor < 1)
            {
                try
                {
                    _text = "Step_1208:; " + GameEngine.LocationString(colony.Location.ToString())
                            + " > " + _name_col + " ; " + _owner_col
                            + " > Handle_Upgrades "
                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;


                    //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
                    //{
                    // Industry Upgrade ?
                    var _all = TechTreeHelper
                        .GetBuildProjects(colony)
                        //.OfType<ProductionFacilityUpgradeProject>()
                        ;

                    // seems not to work !!!
                    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.IsUpgrade && bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Industry));

                    var _food_exists = colony.GetFacilityType(ProductionCategory.Food);
                    var _industry_exists = colony.GetFacilityType(ProductionCategory.Industry);
                    var _energy_exists = colony.GetFacilityType(ProductionCategory.Energy);
                    var _research_exists = colony.GetFacilityType(ProductionCategory.Research);
                    var _intelligence_exists = colony.GetFacilityType(ProductionCategory.Intelligence);

                    //ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                    //    .GetBuildProjects(colony)
                    //    .OfType<ProductionFacilityUpgradeProject>()
                    //    .FirstOrDefault(bp => bp.Isup))
                    //    ;

                    //ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                    //    .GetBuildProjects(colony)
                    //    .Where(o => o.IsUpgrade && o.BuildDesign.)
                    //    .(bp => bp.IsUpgrade && bp.BuildDesign.EncyclopediaCategory.ToString() == Encyclopedia.EncyclopediaCategory.Facilites.ToString());

                    if (upgradeIndustryProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                    }
                    else
                    {
                        _text = "Step_1222:; " + GameEngine.LocationString(colony.Location.ToString())
                            + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > no Upgrade > INDUSTRY"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }

                    // Food Upgrade ?
                    ProductionFacilityUpgradeProject upgradeFoodProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Food));

                    if (upgradeFoodProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeFoodProject));
                    }
                    else
                    {
                        _text = "Step_1221:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > no Upgrade > FOOD"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                    // End of Food Upgrade

                    // Energy Upgrade ?
                    ProductionFacilityUpgradeProject upgradeEnergyProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Energy));

                    if (upgradeEnergyProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeEnergyProject));
                    }
                    else
                    {
                        _text = "Step_1223:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > no Upgrade > ENERGY"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                    // End of Energy Upgrade


                    // Research Upgrade ?
                    ProductionFacilityUpgradeProject upgradeResearchProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Research));

                    if (upgradeResearchProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeResearchProject));
                    }
                    else
                    {
                        _text = "Step_1224:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > no Upgrade > RESEARCH"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                    // End of Research Upgrade


                    // Intelligence Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIntelligenceProject = TechTreeHelper
                        .GetBuildProjects(colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == colony.GetFacilityType(ProductionCategory.Intelligence));

                    if (upgradeIntelligenceProject != null)
                    {
                        colony.BuildQueue.Add(new BuildQueueItem(upgradeIntelligenceProject));
                    }
                    else
                    {
                        _text = "Step_1225:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > no Upgrade > INTELLIGENCE"
                                ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;
                    }
                    // End of Intelligence Upgrade

                    //}
                }
                catch
                {
                    _text = "Step_1229:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Upgrades on "
                            + _name_col + ", Owner= " + _owner_col
                            + " > CRASH"
                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;

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

                if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)
                {
                    double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                        * (colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
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

                    // doubled
                    //foreach (var item in TechTreeHelper
                    //    .GetBuildProjects(colony)
                    //    .OfType<StructureBuildProject>())
                    //{
                    //    _text = "Step_1226:; > " + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col
                    //        + " structureProject: available "
                    //        + ", turnsNeeded= " + GameEngine.Do_2_Digit(item.TurnsRemaining.ToString())
                    //        + ", industryRemaining= " + GameEngine.Do_4_Digit(item.IndustryRemaining.ToString())
                    //        + ", NetIndustry= " + _net_industry_text
                    //        + ", item= " + item.BuildDesign

                    //    //+ ", Costs= " + cost

                    //    //+ ", NetIndustry= " + item.BuildDesign.BuildCost.

                    //    //+ "; Credits.Current= " + civM.Credits.CurrentValue
                    //    //+ " > IsRushed for " + s.Project
                    //    //+ " on " + _name_col + " " + s.Project.Location
                    //    ;
                    //    if (writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += newline + _text;

                    //    if (boolCheckColonyProduction)
                    //        _text = ""; // just for breakpoint
                    //}

                    if (structureProject != null) //&&
                    {
                        double _turns_needed = 99;

                        try
                        {
                            // searching for DivideByZeroException

                            _turns_needed = structureProject.TurnsRemaining;

                            //_turns_needed = Math.Ceiling(structureProject.GetCurrentIndustryCost()
                            //                        // colony.GetProductionModifier(ProductionCategory.Industry).Bonus  // does a DivideByZeroException
                            //                        + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput));
                        }
                        catch
                        {

                        }

                        _text = "Step_1227:; " + GameEngine.LocationString(colony.Location.ToString()) + " > " + _name_col
                                + " structureProject: available > "
                                + structureProject.TurnsRemaining + " turnsNeeded for"
                                + " " + structureProject.BuildDesign

                                //+ ", Costs= " + cost
                                + ", industryRemaining= " + structureProject.IndustryRemaining
                                + ", NetIndustry= " + _net_industry_text
                            //+ ", NetIndustry= " + item.BuildDesign.BuildCost.

                            //+ "; Credits.Current= " + civM.Credits.CurrentValue
                            //+ " > Math.Ceiling= " + Math.Ceiling(structureProject.GetCurrentIndustryCost()
                            //                        / colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                            //                        + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))
                            //+ " on " + _name_col + " " + GameEngine.LocationString(colony.Location.ToString())
                            //+ "  ..( max. 8)"
                            ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;

                        //if (Math.Ceiling(structureProject.GetCurrentIndustryCost()
                        //    / colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        //    + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))

                        if (_turns_needed < 2.0)
                        {
                            colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                        }

                        if (colony.BuildQueue.Count < 2 && _turns_needed < 5.0) // not more as 3 turns
                        {
                            colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                        }

                        if (colony.BuildQueue.Count < 2 && _turns_needed < 19.0) // not more as 18 turns
                        {
                            colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                            _text = "Step_1279:; " + GameEngine.LocationString(colony.Location.ToString()) + " > " + _name_col
                                    + " > structureProject: available > "
                                    + structureProject.TurnsRemaining + " turnsNeeded for"
                                    + " " + structureProject.BuildDesign

                                    //+ ", Costs= " + cost
                                    + ", industryRemaining= " + structureProject.IndustryRemaining
                                    + ", NetIndustry= " + GameEngine.Do_4_Digit(colony.NetIndustry.ToString())

                                    + "  ..( max. 18)"
                                //+ ", NetIndustry= " + item.BuildDesign.BuildCost.

                                //+ "; Credits.Current= " + civM.Credits.CurrentValue
                                //+ " > Math.Ceiling= " + Math.Ceiling(structureProject.GetCurrentIndustryCost()
                                //                        / colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                                //                        + (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))
                                //+ " on " + _name_col + " " + GameEngine.LocationString(colony.Location.ToString())
                                ;
                            if (writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += newline + _text;
                        }
                    }

                }

            }
        NoUpgradesDueToLaborPool:;
        }

        private static void Handle_Build_Anything(Colony colony, Civilization civ)
        {
            _text = "Step_1261:; " + GameEngine.LocationString(colony.Location.ToString()) + " > " + _name_col + "; " + _owner_col
                + " > Handle_Build_Anything on; "

                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text; // not necessary


            //colony.ProcessQueue();

            if (colony.BuildQueue.Count > 0)
            {
                //Build_Queue_Print(colony); // not to often

                //_text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) + "; " + _name_col + " ; " + _owner_col + " Handle_Build_Anything on; "

                //        + "; already bulding >; " + colony.BuildQueue[0].Description;
                //;
                //if (writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += newline + _text; // not necessary
            }

            colony.ProcessQueue();


            if (colony.BuildQueue.Count < 2) //Handle_Build_Anything
            {
                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + colony.Morale
                //    + "; UnitOutput=;" + colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager civM = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => civM.Resources[r.Resource].CurrentValue - r.Used);

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1255:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Build_Anything"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + "; for; " + item.Key
                //        //+ "; NetIndustry=;" + colony.NetIndustry
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(colony).ToList()
                //    //.OfType<StructureBuildProject>()
                //    //.Where(p =>
                //    //        p.GetCurrentIndustryCost() > 0
                //    //        && EnumHelper
                //    //            .GetValues<ResourceType>()
                //    //            .Where(availableResources.ContainsKey)
                //    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    //.OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault()
                //    ;
                //foreach (var item in structureProject_Available)
                //{
                //    _text = "Step_1275:; " + GameEngine.LocationString(colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);

                //}

                //structureProject
                StructureBuildProject anyProject = TechTreeHelper.GetBuildProjects(colony)
                    .OfType<StructureBuildProject>().Where(p => p.GetCurrentIndustryCost() > 0
                    && EnumHelper.GetValues<ResourceType>().Where(availableResources.ContainsKey).All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();

                //anyProject
                var anyProject_Available = TechTreeHelper
                    .GetBuildProjects(colony).ToList()
                    //.OfType<StructureBuildProject>()
                    //.Where(p =>
                    //        p.GetCurrentIndustryCost() > 0
                    //        && EnumHelper
                    //            .GetValues<ResourceType>()
                    //            .Where(availableResources.ContainsKey)
                    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).ToList();
                ;



                //foreach (BuildProject item in most_expensive_Project_Available)
                //{
                //    var _type = most_expensive_Project_Available.GetType();

                //    _text = "Step_1233:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //                + " on; " + _name_col
                //                + "; " + _owner_col
                //                + "; Morale=; " + colony.Morale
                //                //+ "; prodOutput=;" + prodOutput // per unit
                //                + "; NetIndustry=;" + colony.NetIndustry
                //                + "; Available=;" + item.BuildDesign

                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);

                //    //switch (_type)
                //    //{
                //    //        //case StructureBuildProject: 
                //    //        case _type == Supremacy.Economy.ProductionFacilityBuildProject:
                //    //            break;
                //    //    default:
                //    //        break;
                //    //}
                //}



                string _toBuildText = " > no anyProject to build";
                if (anyProject != null)
                {
                    _toBuildText = anyProject.BuildDesign.ToString();
                }
                else
                {
                    _text = "Step_1289:; " + GameEngine.LocationString(colony.Location.ToString())
                            + " > " + _name_col
                            + "; " + _owner_col
                        + " > Handle_Build_Anything"
                            + "; Morale=; " + colony.Morale
                            //+ "; prodOutput=;" + prodOutput // per unit
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; ToBuild=;" + _toBuildText
                            + "; BuildQueue.Count= " + colony.BuildQueue.Count
                        //+ "; MathCeiling=; " + Math.Ceiling(anyProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        //        * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }


                //_text = "Step_1236:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; NetIndustry=;" + colony.NetIndustry
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1239:; " + GameEngine.LocationString(colony.Location.ToString()) //+ " > Handle_Build_Anything"
                        + " > " + _name_col
                        + "; " + _owner_col
                        + "; Morale=; " + colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; NetIndustry=;" + colony.NetIndustry
                        + "; Handle_Build_Anything=;" + _toBuildText
                        + "; BuildQueue.Count=" + colony.BuildQueue.Count
                    //+ "; MathCeiling=; " + Math.Ceiling(anyProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

                //if (anyProject != null && Math.Ceiling(anyProject.GetCurrentIndustryCost() / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)civM.Credits.CurrentValue / 4;

                if (anyProject == null && _itemToBuild != null)// && (int)anyProject.BuildDesign.BuildCost < (civM.Credits.CurrentValue / 2))
                {
                    colony.BuildQueue.Add(new BuildQueueItem(_itemToBuild));
                    _text = "Step_1247:; " + GameEngine.LocationString(colony.Location.ToString()) //+ " Added to Build"
                            + " > " + _name_col
                            + "; " + _owner_col
                            + "; Morale=; " + colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; NetIndustry=;" + colony.NetIndustry
                            + "; Added_to_Build=;" + _itemToBuild.BuildDesign.ToString()

                            ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }


                //if (/*colony.BuildSlots.All(t => t.Project == null) && */colony.BuildQueue.Count < 2)  //2023-11-11
                //{
                //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);
                //    _text = "Step_1234:; " + GameEngine.LocationString(colony.Location.ToString()) + " Added to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            + "; prodOutput=;" + prodOutput
                //            + "; ToBuildonSlots.Count=;" + projects.Count

                //            ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += newline + _text;

                //    int count = 0;
                //    foreach (var item in projects)
                //    {

                //        _text = "Step_1246:; " + GameEngine.LocationString(colony.Location.ToString()) + " OPTIONS to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + colony.Morale
                //            + "; NetIndustry=;" + colony.NetIndustry
                //            + "; BCost=;" + GameEngine.Do_5_Digit(item.BuildDesign.BuildCost.ToString())
                //            + "; OPTIONS_to_Build_on #;" + count
                //            + "; " + item.BuildDesign.ToString()

                //            ;
                //        count++;
                //        if (writeDirectly_Colony) Console.WriteLine(_text);
                //        _colony_full_Report += newline + _text;
                //    }
                //}
            }
        }
        //End of Handle_Build_Anything

        private static void Handle_Buy_Build(Colony colony, Civilization civ)
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

                //if ((civM.Credits.CurrentValue - (cost * 0.2)) > s.Project.GetTotalCreditsCost())
                if ((manager.Credits.CurrentValue > cost))
                {
                    double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * (colony.Morale.CurrentValue
                        / (0.5f * MoraleHelper.MaxValue)) * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
                    int maxProdFacility = Math.Min(colony.TotalFacilities[ProductionCategory.Industry].Value, (colony.GetAvailableLabor() / colony.GetFacilityType(ProductionCategory.Industry).LaborCost)
                        + colony.ActiveFacilities[ProductionCategory.Intelligence].Value + colony.ActiveFacilities[ProductionCategory.Research].Value + colony.ActiveFacilities[ProductionCategory.Industry].Value);
                    int industryNeeded = colony.BuildSlots.Where(bs => bs.Project != null)
                        .Select(bs => bs.Project.IsRushed ? 0 : bs.Project.GetCurrentIndustryCost()).Sum();
                    int turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));


                    if (turnsNeeded > 1 && turnsNeeded < 3)  // we buy when turnsNeede = 2
                    {
                        _text = "Step_1210:; " + GameEngine.LocationString(colony.Location.ToString()) + " > Handle_Buy_Build: "
                            + "Credits.Current= " + manager.Credits.CurrentValue
                            + ", Costs= " + cost
                            + ", industryNeeded= " + industryNeeded
                            + ", prodOutput= " + (int)prodOutput
                            + ", turnsNeeded= " + turnsNeeded
                            + " > IsRushed for " + s.Project
                            + " on " + _name_col + " " + s.Project.Location
                        ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += newline + _text;

                        s.Project.IsRushed = true;
                        //while (colony.DeactivateFacility(ProductionCategory.Industry)) { }  ??
                    }
                }
            });
        }

        private static void Handle_Ship_Production(Colony colony, Civilization civ, Dictionary<ShipType, Tuple<int, string>> _listPrioShipBuild)
        {
            bool checkForShipProduction = true;
            _shipOrderIsDone = false;

            if (colony.Shipyard == null) { return; }
            if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; }

            _text = /*newline + */"Step_5780:; " + GameEngine.LocationString(colony.Location.ToString()) + " ShipProduction > " + _name_col;
            //if (writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += newline + _text;

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
                _text = "Step_5781:; " + GameEngine.LocationString(colony.Location.ToString())
                    + " ShipProduction"
                    + " - " + _owner_col

                    + " (needs " + GameEngine.Do_2_Digit(proj.TurnsRemaining.ToString()) + " Turns)"
                    + ": available= " + proj.BuildDesign
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;

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

            int _gp = GameContext.Current.TurnNumber / 10;

            // first value is basic requirement
            int _shipNeeded_colony = 1 + civM.ShipColonyNeeded - civM.ShipColonyAvailable - civM.ShipColonyOrdered;
            int _shipNeeded_construction = 1 + civM.ShipConstructionNeeded - civM.ShipConstructionAvailable - civM.ShipConstructionOrdered;
            int _shipNeeded_medical = 1 + civM.ShipMedicalNeeded - civM.ShipMedicalAvailable - civM.ShipMedicalOrdered;
            int _shipNeeded_spy = 1 + civM.ShipSpyNeeded - civM.ShipSpyAvailable - civM.ShipSpyOrdered;
            int _shipNeeded_diplomatic = _gp + civM.ShipDiplomaticNeeded - civM.ShipDiplomaticAvailable - civM.ShipDiplomaticOrdered;
            int _shipNeeded_science = 1 + civM.ShipScienceNeeded - civM.ShipScienceAvailable - civM.ShipScienceOrdered;
            int _shipNeeded_scout = 1 + civM.ShipScoutNeeded - civM.ShipScoutAvailable - civM.ShipScoutOrdered;
            int _shipNeeded_fastattack = 1 + civM.ShipFastAttackAvailable - civM.ShipFastAttackAvailable - civM.ShipFastAttackOrdered;
            //int _shipNeeded_destroyer = civM. - civM.ShipDestroyerAvailable - civM.ShipDestroyerOrdered;
            int _shipNeeded_cruiser = 1 + civM.ShipCruiserNeeded - civM.ShipCruiserAvailable - civM.ShipCruiserOrdered;

            int _shipNeeded_strikecruiser = _gp + civM.ShipStrikeCruiserNeeded - civM.ShipStrikeCruiserAvailable - civM.ShipStrikeCruiserOrdered;
            int _shipNeeded_heavycruiser = _gp + civM.ShipHeavyCruiserNeeded - civM.ShipHeavyCruiserAvailable - civM.ShipHeavyCruiserOrdered;
            int _shipNeeded_command = _gp + civM.ShipCommandNeeded - civM.ShipCommandAvailable - civM.ShipCommandOrdered;
            int _shipNeeded_transport = _gp + civM.ShipTransportNeeded - civM.ShipTransportAvailable - civM.ShipTransportOrdered;

            if (GameContext.Current.TurnNumber < 9)
            { _shipNeeded_colony = +10; }

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

            _text = "Step_5785:; " + GameEngine.LocationString(colony.Location.ToString())
                + " ShipProduction"
                + " - " + _name_col
                + " - " + _owner_col
                + " > _listPrioShipBuild.Count= " + _listPrioShipBuild.Count
                ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;

            if (_listPrioShipBuild.Count == 0)
            {
                //
                _text = _bool_listPrioShipBuild_Empty.ToString(); // dummy, just keep

                _bool_listPrioShipBuild_Empty = true;
            }
            else
            {
                _listPrioShipBuild.OrderByDescending(_l => _l.Value);
                _text = _listPrioShipBuild[0].Item2.ToString();

                if (civ.Key.Contains("Botha"))
                    _text += _text + newline; // just for breakpoint

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

                if (GameContext.Current.TurnNumber < 7)
                {
                    _neededShipType = ShipType.Colony;
                }

                if (GameContext.Current.TurnNumber < 15)
                {
                    _neededShipType = ShipType.Construction;
                }

                if (GameContext.Current.TurnNumber < 20 && colony.Owner.Key == "BORG")
                {
                    _neededShipType = ShipType.Construction;
                }


                BuildShipType(colony, civM, _neededShipType, potentialProjects[0]);
                goto ProcessQueue;

            } // end if prioList != null



            //_neededShipType = ShipType.Construction; // here more code to do


            if (checkForShipProduction)
                _text = ""; /*just for breakpoint*/

            // Already checked before but here to hover the count
            if (colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; }

            //check _listPrioShipBuild2

            var _listPrioShipBuild2 = new List<ShipType>();// _listPrioShipBuild2 is out of the options if first list is empty
            foreach (BuildProject proj in potentialProjects)  // find Prio
            {
                //if (checkForShipProduction)
                //    _text = ""; /*just for breakpoint*/

                if (potentialProjects.Count == 1)
                {
                    // just use Proj[0], ShipType.Medical = DUMMY
                    _text = "Step_1209:; " + GameEngine.LocationString(colony.Location.ToString()) + " Only this one available > "
                        + potentialProjects[0].BuildDesign

                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;

                    BuildShipType(colony, civM, ShipType.Medical, potentialProjects[0]);
                    _shipOrderIsDone = true;
                    goto ProcessQueue;
                }

                //if (_bool_listPrioShipBuild_Empty)
                //{
                //    _listPrioShipBuild.Add(proj.BuildDesign.DesignID, 1);
                //}

                //if (_shipOrderIsDone == false)
                //{

                if (proj.Description.Contains("COLONY") && _shipNeeded_colony > 0)
                {
                    _text = "Step_1210:; " + GameEngine.LocationString(colony.Location.ToString()) + " Handle_Buy_Build: "
                            + "_shipNeeded_colony= " + _shipNeeded_colony
                        //+ ", Costs= " + cost
                        //+ ", industryNeeded= " + industryNeeded
                        //+ ", prodOutput= " + prodOutput.ToString()
                        //+ ", turnsNeeded= " + turnsNeeded
                        //+ " > IsRushed for " + s.Project
                        //+ " on " + _name_col + " " + s.Project.Location
                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;

                    CheckForColonizer(colony, civM, ShipType.Colony, proj);
                }


                if (proj.Description.Contains("COMMAND") && _shipNeeded_command > 0)
                    CheckForBuild(colony, civM, ShipType.Command, proj);
                if (proj.Description.Contains("CRUISER") && _shipNeeded_cruiser > 0)
                    CheckForBuild(colony, civM, ShipType.Cruiser, proj); // includes Heavy and StrikeCruiser
                if (proj.Description.Contains("DESTROYER") && _shipNeeded_fastattack > 0)
                    CheckForBuild(colony, civM, ShipType.FastAttack, proj);
                if (proj.Description.Contains("FRIGATE") && _shipNeeded_fastattack > 0)
                    CheckForBuild(colony, civM, ShipType.FastAttack, proj);
                if (proj.Description.Contains("FIGHTER") && _shipNeeded_fastattack > 0)
                    CheckForBuild(colony, civM, ShipType.FastAttack, proj);
                if (proj.Description.Contains("SURVEYOR") && _shipNeeded_fastattack > 0)
                    CheckForBuild(colony, civM, ShipType.FastAttack, proj);
                if (proj.Description.Contains("RAIDER") && _shipNeeded_fastattack > 0)
                    CheckForBuild(colony, civM, ShipType.FastAttack, proj);
                if (proj.Description.Contains("SCOUT") && _shipNeeded_scout > 0)
                    CheckForBuild(colony, civM, ShipType.Scout, proj);
                if (proj.Description.Contains("SCIENCE") && _shipNeeded_science > 0)
                    CheckForBuild(colony, civM, ShipType.Science, proj);


                if (proj.Description.Contains("TRANSPORT") && _shipNeeded_transport > 0)
                    CheckForBuild(colony, civM, ShipType.Transport, proj);
                if (proj.Description.Contains("CONSTRUCTION") && _shipNeeded_construction > 0)
                    CheckForBuild(colony, civM, ShipType.Construction, proj);
                if (proj.Description.Contains("MEDICAL") && _shipNeeded_medical > 0)
                    CheckForBuild(colony, civM, ShipType.Medical, proj);
                if (proj.Description.Contains("SPY") && _shipNeeded_spy > 0)
                    CheckForBuild(colony, civM, ShipType.Spy, proj);
                if (proj.Description.Contains("DIPLOMATIC") && _shipNeeded_diplomatic > 0)
                    CheckForBuild(colony, civM, ShipType.Diplomatic, proj);

                _text = _shipOrderIsDone.ToString();

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



            //}

            //doubled
            //int xy = 0;
            //foreach (var item in colony.Shipyard.BuildQueue)
            //{
            //    xy += 1;
            //    _text = _text = "Step_5787:; " + GameEngine.LocationString(colony.Location.ToString())
            //        + " ShipProduction-Queue # " + xy
            //        + " - " + _owner_col

            //        + " (needs " + item.TurnsRemaining + " Turns)"
            //        + ": in queue= " + item.Project.BuildDesign
            //        ;
            //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //    _colony_full_Report += newline + _text;
            //}

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
            //        + " at " + GameEngine.LocationString(colony.Location.ToString())
            //        + " - " + _owner_col
            //        + ": ColonyShips: Available= " + civM.ShipColonyAvailable
            //        + ", Needed= " + civM.ShipColonyNeeded
            //        + ", Ordered= " + civM.ShipColonyOrdered

            //        ;
            //if (writeDirectly_Colony) Console.WriteLine(_text);

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
            //    //        + " at " + GameEngine.LocationString(colony.Location.ToString())
            //    //        + " " + _name_col
            //    //        + " - " + _owner_col
            //    //        + ": Added Colonizer project..." + project.BuildDesign
            //    //        ;
            //    //    if (writeDirectly_Colony) Console.WriteLine(_text);

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
            //            + " at " + GameEngine.LocationString(colony.Location.ToString())
            //            + " - " + _owner_col
            //            + ": Added Construction ship project..." + project.BuildDesign

            //            ;
            //        if (writeDirectly_Colony) Console.WriteLine(_text);
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
            //    _text = "Step_5360:; " + GameEngine.LocationString(colony.Location.ToString()) //+ " next: check for ShipProduction - not at HomeSector: "
            //        + " " + colony.Shipyard.Design

            //        + " at " + _name_col
            //        + ", Owner= " + _owner_col
            //        + " - here no ship is building - maybe on the next code"
            //        ;
            //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //    //CheckForSystemsToColonizeProject(colony);
            //}


            // this builds a colonizer - why only colonizer ?
            if (colony.Shipyard.BuildSlots.Any(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects2 = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
                //foreach (BuildProject project in projects2)
                //{
                //    _text = "ShipProduction at HomeSector: "
                //        + " at " + GameEngine.LocationString(colony.Location.ToString())
                //        + " - " + _owner_col
                //        + ": available= " + project.BuildDesign

                //        ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //}

                if (boolCheckShipProduction)
                    _text = ""; // just for breakpoint

                if (_neededShipType == ShipType.Medical)  // medical is set as default
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
                        + GameEngine.LocationString(colony.Location.ToString())
                        + " > ShipProduction "
                        + " - " + _owner_col
                        + ": Added Construction project..." + newProject.BuildDesign

                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += newline + _text;
                }
            }



        //foreach (var item in colony.Shipyard.BuildQueue)
        //{
        //    _text = "Step_5387:; " + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col
        //        + ", ShipProduction > " + item.Project.BuildDesign
        //        + ", TurnsRemaining= " + item.Project.TurnsRemaining


        //        ;
        //    if (writeDirectly_Colony) Console.WriteLine(_text);
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

            _text = "Step_5793:; " + GameEngine.LocationString(colony.Location.ToString()) + " ShipProduction"
                    + " at " + _name_col
                    //+ " - " + _owner_col
                    + ": ColonyShips: Available= " + civM.ShipColonyAvailable
                    + ", Needed= " + civM.ShipColonyNeeded
                    + ", Ordered= " + civM.ShipColonyOrdered

                    ;
            _colony_full_Report += newline + _text;
            if (boolCheckShipProduction)
            {
                if (writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += newline + _text;
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
            _text = "Step_5784:; " + GameEngine.LocationString(colony.Location.ToString()) + " ShipProduction"
                    + " at " + _name_col
                    + ", Owner= " + _owner_col
                    + " > ordered ShipBuilding-Tpye: " + shipType
                    //+ ", Needed= " + civM.ShipColonyNeeded
                    //+ ", Ordered= " + civM.ShipColonyOrdered

                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += newline + _text;

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
            //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //}


            //BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
            if (project != null)
            {
                colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                _text = "Step_5383:; " + GameEngine.LocationString(colony.Location.ToString())
                    + " BuildShipType > ShipProduction at"
                    + " " + _name_col
                    + ", Owner= " + _owner_col
                    + ": Added Ship Build project..." + project.BuildDesign
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += newline + _text;
            }
        }


        //TODO: Move ship production out of colony AI. It requires a greater oversight than just a single colony
        //TODO: Is there any need for separate functions for empires and minor races? > 2024: I guess: no !!!
        //TODO: Break these functions up into smaller chunks
#pragma warning disable IDE0051 // Remove unused private members
        private static void Handle_Shipx_OFF_ProductionEmpire(Colony colony, Civilization civ)
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
            //        + " at " + GameEngine.LocationString(colony.Location.ToString())
            //        + " - " + _owner_col
            //        + ": available= " + project.BuildDesign

            //        ;
            //    if (writeDirectly_Colony) Console.WriteLine(_text);
            //}

            if (colony.Sector == homeSector)
            {
                _text = "Step_5380:; ShipProduction at " + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col
                    //+ " - Not Habited: Habitation= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);

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
                //            + " at " + GameEngine.LocationString(colony.Location.ToString())
                //            + " " + _name_col
                //            + " - " + _owner_col
                //            + ": Added Colonizer project..." + project.BuildDesign

                //            ;
                //        if (writeDirectly_Colony) Console.WriteLine(_text);
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
                            + " at " + GameEngine.LocationString(colony.Location.ToString())
                            + " - " + _owner_col
                            + ": Added Construction ship project..." + project.BuildDesign

                            ;
                        if (writeDirectly_Colony) Console.WriteLine(_text);
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
                _text = "Step_5390:; " + GameEngine.LocationString(colony.Location.ToString()) + "next: check for ShipProduction - not at HomeSector: "
                    + colony.Shipyard.Design
                    + " - " + _owner_col
                    + " at " + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col
                    + " - here no ship is building - maybe on the next code"
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
                //CheckForSystemsToColonizeProject(colony);
            }


            // this builds a colonizer - why only colonizer ?
            if (colony.Shipyard.BuildSlots.Any(t => t.Project == null) && colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects2 = TechTreeHelper.GetShipyardBuildProjects(colony.Shipyard);
                //foreach (BuildProject project in projects2)
                //{
                //    _text = "ShipProduction at HomeSector: "
                //        + " at " + GameEngine.LocationString(colony.Location.ToString())
                //        + " - " + _owner_col
                //        + ": available= " + project.BuildDesign

                //        ;
                //    if (writeDirectly_Colony) Console.WriteLine(_text);
                //}

                ShipType _neededShipType;

                _neededShipType = ShipType.Construction; // here more code to do

                BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
                if (newProject != null)
                {
                    colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));
                    _text = "Step_5386:; ShipProduction "
                        + " at " + GameEngine.LocationString(colony.Location.ToString())
                        + " - " + _owner_col
                        + ": Added Colonizer project..." + newProject.BuildDesign

                        ;
                    if (writeDirectly_Colony) Console.WriteLine(_text);
                }
            }

            foreach (var item in colony.Shipyard.BuildQueue)
            {
                _text = "Step_5387:; " + GameEngine.LocationString(colony.Location.ToString()) + " " + _name_col
                    + ", ShipProduction > " + item.Project.BuildDesign
                    + ", TurnsRemaining= " + item.Project.TurnsRemaining


                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
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

            _text = "Step_5393:; " + GameEngine.LocationString(fleet.Location.ToString()) + " using " + fleet.Ships[0].ObjectID + " " + fleet.Ships[0].Design + " > CheckForSystemsToColonizeProject..."
                    //+ " - Not Habited: Habitation Aim= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
            if (writeDirectly_Colony) Console.WriteLine(_text);

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
                string _ownerText = "No Owner";
                if (item.Owner != null)
                {
                    _ownerText = item.Owner.Key;
                }
                _text = "Step_5396:; " + GameEngine.LocationString(colony.Location.ToString()) + " ShipProduction at " + _name_col
                    + " - possible: " + possibleSystems.Count
                    + " - inhabited ? > " + item.HasColony //" for HasColony"


                    + " > at " + GameEngine.LocationString(item.Location.ToString())
                    + " - " + _ownerText
                    ;
                if (writeDirectly_Colony) Console.WriteLine(_text);
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static void Handle_Shipx_OFF_ProductionMinor(Colony colony, Civilization civ)
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