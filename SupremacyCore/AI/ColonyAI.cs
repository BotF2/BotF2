// File:ColonyAI.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using Obtics.Collections;
//using Obtics.Collections;
using Supremacy.Annotations;
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
        //private const int NOT_USED_NumScouts = 1; // not used at the moment
        //private const int ColonyShipEveryTurns = 2;
        //private const int NOT_USED_ColonyShipEveryTurnsMinor = 5; // not used at the moment
        //private const int NOT_USED_MaxMinorColonyCount = 3;  // not used at the moment

        //private static int neededColonizer;
        //private const int MaxEmpireColonyCount = 999; // currently not used



        [NonSerialized]

        private static string _owner_col;
        private static string _name_col;
        private static string _colony_full_Report;
        private static bool _colonyAIControlled = false;
        private static BuildProject _itemToBuild;
        private static BuildProject _itemToBuild_Facility; // Food, Industry etc
        private static bool _writeDirectly_Colony = true;
        private static bool _shipOrderIsDone;

        public static void DoTurn([NotNull] Civilization _civ)
        {
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];
            //string _col_location = ClientApp.;
            string _newline = Environment.NewLine;

            _writeDirectly_Colony = true;
            string _text = _newline + "Step_1101:; ColonyAI.cs > Do_0_Turn_Unit begins... for > " + _civ.Key
                + ": Deu=" + _civM.Resources.Deuterium.CurrentValue
                + ", Dur=" + _civM.Resources.Duranium.CurrentValue
                + ", Dil=" + _civM.Resources.Dilithium.CurrentValue
                + " > " + DateTime.Now + ", Console-Output= " + _writeDirectly_Colony.ToString()
                ;
            //if (_writeDirectly_Colony) 
            Console.WriteLine(_text);
            string _civ_text_ColonyAI = _text;


            //int _required_Energy = 50 + (50 * _civM.AverageTechLevel);

            // foreach _colony
            foreach (Colony _colony in GameContext.Current.Universe.FindOwned<Colony>(_civ.CivID))
            {
                _writeDirectly_Colony = true;
                try
                {
                    // checkcolony

                    _name_col = _colony.Name; _text += " "; // dummy - please keep
                    _owner_col = _colony.Owner.Key;
                    //string _net_industry_text = GameEngine.Do_x_Digit_String( 4, _colony.Industry_Net.ToString());

                    _colony_full_Report = _civ_text_ColonyAI; // _newline; // new one for each _colony

                    _itemToBuild = null;
                    _itemToBuild_Facility = null;

                    var _all_Build_Projects = TechTreeHelper.GetBuildProjects(_colony);

                    Print_all_Build_Projects(_all_Build_Projects); // print to Debug _output = console



                    //_text = _newline + "Step_1103:; " + GameEngine.LocationString(_colony.Location.ToString()) + " * " + _name_col + " (ID=" + _colony.ObjectID
                    //    + ") * > Handling _colony... > "
                    //    + " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues >"
                    //    ;

                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += _newline + _text;


                    //if (MaxEmpireColonyCount == 999)
                    //    _text = "";//nothing - just for dummy



                    //if (_colony.BuildQueue.Count > 0)
                    //{
                    //    _text = "Step_1104:; " + GameEngine.LocationString(_colony.Location.ToString()) + " "
                    //            + _name_col + " ; " + _owner_col
                    //            + "; Handling.. "
                    //            + "; already building; " + _colony.BuildQueue[0].Project.Description

                    //    ;


                    //    for (int i = 0; i < _colony.BuildQueue.Count; i++)
                    //    {
                    //        _text += _newline + "Step_1431:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //            + " > " + _name_col + " ; " + _owner_col
                    //            + ", BuildQueue # " + i + " > " + _colony.BuildQueue[0].Description
                    //            + ", needs " + _colony.BuildQueue[0].TurnsRemaining + " Turns "

                    //            //+ _newline
                    //            ;
                    //    }

                    //}
                    //else
                    //{
                    //    _text = "Step_1432:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //            + " > " + _name_col + " ; " + _owner_col
                    //            + " > BuildQueue is empty BEFORE Handling..."

                    //            ;
                    //}


                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += _newline + _text;


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


                    if (_colony.Owner.IsHuman)
                    {
                        Print_Colony_Owner_IsHuman(_colony);  // prints Step_1102 notification only

                        //Debugger.Break();

                        //_colonyAIControlled = false;
                        //_colonyAIControlled = /*true;*/

                        //_text = /*_newline +*/ "Step_1102:; " + GameEngine.LocationString(_colony.Location.ToString()) + " * " + _name_col + " " + _owner_col
                        //    + " * > AIcontrolled= " + _colonyAIControlled // + " ) > Handling _colony"
                        //    + "; BuildQueue.Count= " + _colony.BuildQueue.Count // + " ) > Handling _colony"
                        //                                                       //+ " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues "
                        //    ;
                        //if (_writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += _newline + _text;
                    }



                    // which Colony > see Console Output

                    // next_Check / set Breakpoint
                    //_colony.ProcessQueue();
                    Colony_Step_01_Check_Population(_colony);  // + Colony_Step_03_Handle_Energy_Production(_colony); // done inside Colony_Step_01_Check_Population
                    Colony_Step_05_Handle_Food_Production(_colony);

                    //// just for info
                    //var most_expensive_Project_Available = TechTreeHelper
                    //    .GetBuildProjects(_colony).ToList()
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

                    //    _text = "Step_2235:; " + GameEngine.LocationString(_colony.Location.ToString()) + " MOST Expensive Available"
                    //                + " on; " + _name_col
                    //                + "; " + _owner_col

                    //                + "; costs= " + _itemToBuild.IndustryRemaining
                    //                + "; Turns needed: " + _itemToBuild.TurnsRemaining
                    //                + "; for " + _itemToBuild.BuildDesign.Key
                    //                ;
                    //    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;
                    //}

                    if (_colony.BuildQueue.Count > 0)
                    {
                        if (_colony.Owner.IsHuman)
                        {
                            //Debugger.Break();
                        }

                        Colony_Step_10_Build_Queue_Clean(_colony);
                    }


                    //_text = /*_newline + */"Step_1418:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //        + " > " + _name_col + " ; " + _owner_col
                    //        + " > _colony.BuildQueue.Count= " + _colony.BuildQueue.Count
                    //        //+ "ID= " + _designID_string
                    //        //+ " = " + _available_item.BuildDesign
                    //        //+ ", calc by maxPop= " + _colony.Population_Max / 100
                    //        ;
                    //for (int i = 0; i < _colony.BuildQueue.Count; i++)
                    //{
                    //    _text += _newline + "Step_1419:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                    //        + " > " + _name_col + " ; " + _owner_col 
                    //        + " > Buildqueue " + i + " > " + _colony.BuildQueue[i].Description;
                    //}

                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += _newline + _text;

                    Build_Queue_Print(_colony);

                    if (_colony.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    int _buildDuration = 0; // just measure the duration (in Turns) to decide next build order
                    foreach (var proj in _colony.BuildQueue)
                    {
                        _buildDuration += proj.TurnsRemaining;
                    }


                    //checkcolproduction
                    if (_colony.BuildQueue.Count < 3)  // ColonyAI ..foreach _colony
                    {
                        // next_Check / set Breakpoint
                        if (_colonyAIControlled)  // not for human player
                        {
                            //if (_colony.Owner.IsHuman) { Debugger.Break(); }

                            Colony_Step_40_Build_for_LaborPool(_colony, _civ); // this first > if free labors, build facilities (no new upgrades!)
                            Colony_Step_50_Handle_Upgrades(_colony, _civ);
                            Colony_Step_60_Handle_Basic_Structures(_colony, _civ); // older code > Bunker Network, Extractors etc.
                            Colony_Step_65_Handle_Buildings(_colony, _civ);
                            Colony_Step_70_Handle_Additional_Structures(_colony, _civ);
                            Colony_Step_75_CheckFor_All_Build_Projects(_colony);

                            Colony_Step_80_Handle_Flex_Production(_colony, _civ); // older code
                            Colony_Step_85_Handle_Build_Anything(_colony, _civ);
                            Colony_Step_92_ClearUpColony(_colony, _civ);
                        }

                        //if (_writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += _newline + _text;

                        _text = "Step_2349:; " + GameEngine.LocationString(_colony.Location.ToString())
                                + " Pop= " + _colony.Population + " of " + _colony.Population_Max
                                + ", Active: Food= " + _colony.Facilities_Active1_Food + " of " + _colony.Facilities_Active1_Food
                                + ", Ind= " + _colony.Facilities_Active2_Industry + " of " + _colony.Facilities_Active2_Industry
                                + ", En= " + _colony.Facilities_Active3_Energy + " of " + _colony.Facilities_Active3_Energy
                                + ", Res= " + _colony.Facilities_Active4_Research + " of " + _colony.Facilities_Active4_Research
                                + ", Int= " + _colony.Facilities_Active5_Intelligence + " of " + _colony.Facilities_Active5_Intelligence
                                + ", Pool= " + _colony.GetAvailableLabor() / 10
                                + " for " + _name_col
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;





                        _colony.ProcessQueue();

                        if (_colonyAIControlled)  // not for human player
                        {
                            Handle_Buy_Build(_colony, _civ);
                            Handle_Industry_Production(_colony);
                            //Handle_Research_Distribution(_colony);
                        }


                        Handle_Labors(_colony); // fills up (if possible): Industry - Research - Intelligence - Fodd (Energy is done before)

                        Handle_Food_Labors_UNDONE(_colony); // in case too much food is produced

                        Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);
                        //_text = "Step_2351:; " + GameEngine.LocationString(_colony.Location.ToString())
                        //        + " Pop= " + _colony.Population + " of max " + _colony.Population_Max
                        //        + ", Active: Food= " + _colony.Facilities_Active1_Food + " of " + _colony.Facilities_Active1_Food
                        //        + ", Ind= " + _colony.Facilities_Active2_Industry + " of " + _colony.Facilities_Active2_Industry
                        //        + ", En= " + _colony.Facilities_Active3_Energy + " of " + _colony.Facilities_Active3_Energy
                        //        + ", Res= " + _colony.Facilities_Active4_Research + " of " + _colony.Facilities_Active4_Research
                        //        + ", Int= " + _colony.Facilities_Active5_Intelligence + " of " + _colony.Facilities_Active5_Intelligence
                        //        + ", Pool= " + _colony.GetAvailableLabor() / 10
                        //        + " for " + _name_col
                        //        ;
                        //if (_writeDirectly_Colony) Console.WriteLine(_text);
                        //_colony_full_Report += _newline + _text;

                        if (_colony.BuildQueue.Count > 0) // not to often 
                        {
                            //Build_Queue_Print(_colony); 
                        }
                        else
                        {
                            _text = "Step_1432:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                                    + " > " + _name_col + " ; " + _owner_col
                                    + " > BuildQueue is empty BEFORE Handling..."
                                    ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                        }


                        int count = 0;
                        foreach (BuildQueueItem buildQueueItem in _colony.BuildQueue) // just > Console.WriteLine
                        {
                            _text = "Step_1206:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                                + "; needs " + GameEngine.Do_x_Digit_String(2, buildQueueItem.Project.TurnsRemaining.ToString()) + " turns "
                                + "; buildQueueItem # " + count + " = " + buildQueueItem.Description

                                    //+ buildQueueItem.Description
                                    ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                            //GameLog.Client.ProductionDetails.DebugFormat(_text);
                            count++;
                        }

                        if (_colony.BuildQueue.Count > 0) // not to often 
                        {
                            //Build_Queue_Print(_colony); 
                        }
                        else
                        {
                            _text = "Step_1433:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                                    + " > " + _name_col + " ; " + _owner_col
                                    + " > BuildQueue is empty BEFORE Handling..."
                                    ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                        }

                        if (_colonyAIControlled)  // not for human player
                        {
                            Handle_Buy_Build(_colony, _civ);
                            Handle_Industry_Production(_colony);
                            //Handle_Research_Distribution(_colony);

                            _colony.ProcessQueue();

                            //if (_colony.Owner.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}

                            Handle_Labors_for_Nothing_to_Build(_colony);
                        }

                        if (_colony.Shipyard != null)
                        {
                            //if (_colony.Owner.IsHuman)
                            //{
                            //    //Debugger.Break();
                            //}

                            //CheckFor_OFF_ShipProduction(_colony);
                            if (/*_colony.Shipyard.BuildSlots != null && */!PlayerAI.IsInFinancialTrouble_BelowMinus2000(_colony.Owner))
                            {
                                Handle_Ship_Production(_colony, _colony.Owner);//, _listPrioShipBuild_tmp);
                                                                             //    old
                                                                             //    if (_civ.IsEmpire) { HandleShipProductionEmpire(_colony, _civ); }
                                                                             //    else { HandleShipProductionMinor(_colony, _civ); }
                            }
                            else
                            {
                                _text = GameEngine.LocationString(_colony.Location.ToString())
                                    + " " + _colony.Name
                                    + " > Empire is in financial problems and can not afford ShipBuilding ( Limit is -2000 )"
                                    ;
                                _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civM.Civilization, _colony, _text, _text, "", SitRepPriority.RedYellow));

                                if (_writeDirectly_Colony) Console.WriteLine("Step_1426:; " + _text);
                                _colony_full_Report += _newline + "Step_1426:; " + _text;
                            }
                        }
                        else
                        {
                            _text = "Step_1437:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                                    + " > has no Shipyard"
                                            ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }

                    } // end of foreach _colony
                    //}
                }// end of try
                catch (Exception e)
                {

                    _text = "Step_1105:; ##################### Problem at ColonyAI.Do_0_Turn_Unit ..." + _colony.Name + _newline + e;
                    //if (_writeDirectly_Colony) 
                    Console.WriteLine(_text);
                    Debugger.Break();
                }// end of catch



                _text = "Step_1107:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony is done..................";
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;


                // Colony_Full_Report
                //Console.WriteLine(_newline + "Step_1103:; Colony_Full_Report > " // Output
                //    + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col + " " + _owner_col
                //    /*+ _newline*/ + _colony_full_Report
                //     + "end of > Colony_Full_Report"
                //     + _newline);


            }// end of 
            _text = "Step_1109:; Finish of ColonyAI.Do_0_Turn_Unit ";
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            //Console.WriteLine(_newline + _newline + "_colony_full_Report" + _newline + _newline + _colony_full_Report + _newline + "End of _colony_full_Report");

        } // End of Main "Do Turn"


        private static void Colony_Step_80_Handle_Flex_Production(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text = /*_newline + */"Step_1218:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Check for Colony_Step_80_Handle_Flex_Production (older code): "
                    + ", BuildQueue.Count= " + _colony.BuildQueue.Count
                    + ", _colony.AvailableLabor= " + _colony.AvailableLabor
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            //if (_colony.Owner.IsHuman && _colony.Name == "Sol")
            //{
            //    Debugger.Break();
            //}

            if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = _colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                {
                    // if Ind +2 < Research + Intel > build one research
                    if (_colony.GetTotalFacilities(ProductionCategory.Industry) + 2 <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                        _text = "Step_1242:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > ordered one more facility for > Industry"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;
                    }
                    else
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Research))));
                        _text = "Step_1244:; " + GameEngine.LocationString(_colony.Location.ToString())
                            + " > Colony_Step_80_Handle_Flex_Production on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > ordered one more facility for > Research"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        if (_colony.GetTotalFacilities(ProductionCategory.Intelligence) + 1 < (_colony.GetTotalFacilities(ProductionCategory.Research) + 2) / 2)
                        {
                            // As well build Intel  > about half of Research
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Intelligence))));
                            _text = "Step_1245:; " + GameEngine.LocationString(_colony.Location.ToString())
                                + " > Colony_Step_80_Handle_Flex_Production on "
                                    + _name_col + ", Owner= " + _owner_col
                                    + " > ordered one more facility for > Intelligence"
                                    ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    }
                }
            }

            //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2)
            //{
            //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(_colony);
            //}
        }

        private static void Colony_Step_75_CheckFor_All_Build_Projects(Colony _colony)
        {
            var _all_Build_Projects = TechTreeHelper.GetBuildProjects(_colony);
            string _text;
            ProductionCategory _available_item_Category;

            Print_all_Build_Projects(_all_Build_Projects); // print to Debug _output = console

            foreach (var _available_item in _all_Build_Projects)
            {

                _text = "Step_1430:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                        + " > " + _name_col + " " + _owner_col
                        + " > _available_item= " + _available_item.BuildDesign
                        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

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
                    case "SOLAR_ARRAY": // this is mostly needed
                    case "WIND_TURBINES": // this is mostly needed
                    case "CHARGE_COLLECTORS": // this is mostly needed
                    case "THERMAL_TETHER": // this is mostly needed
                    case "HEALTH_CORE": // this is mostly needed
                    case "IMMUNOLOGY_CORE": // this is mostly needed
                    case "SUBSPACE_SCANNER": // not so important to build first

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
                        if (_itemToBuild_Facility == null) CheckFor_1_Food_Facility(_colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Industry:
                        if (_itemToBuild_Facility == null) CheckFor_2_Industry_Facility(_colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Energy:
                        continue;
                    case ProductionCategory.Research:
                        if (_itemToBuild_Facility == null) CheckFor_4_Research_Facility(_colony, _available_item, _available_item_Category);
                        continue;
                    case ProductionCategory.Intelligence:
                        if (_itemToBuild_Facility == null) CheckFor_5_Intelligence_Facility(_colony, _available_item, _available_item_Category);
                        continue;
                        //default:
                        //    break;
                }


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
                    foreach (var item in _colony.BuildQueue)
                    {
                        if (item.Project.BuildDesign != _available_item.BuildDesign)
                        {
                            _itemToBuild_Facility = _available_item; // build instead of a facility
                        }
                    }
                }


                if (/*_itemToBuild_Facility == null && */_available_item.BuildDesign.Key.Contains("Battery"))
                {
                    // each 8 turns one more OrbBat is fine
                    if (_colony.OrbitalBatteries_Total < GameContext.Current.TurnNumber / 8 && _colony.OrbitalBatteries_Total < 49)
                    {
                        _itemToBuild_Facility = _available_item;
                        _itemToBuild = _available_item;
                    }
                }
                // if nothing yet build Battery or next: better...


                if (_itemToBuild_Facility != null && _colonyAIControlled)
                {
                    _itemToBuild = _itemToBuild_Facility;
                }

                _text = "Step_1439:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Colony_Step_75_CheckFor_All_Build_Projects on; "*/
                                 + " > " + _name_col + " ; " + _owner_col;
                if (_itemToBuild != null)
                {
                    _text += " > _itemToBuild= " + _itemToBuild
                                     + "; IndustryRemaining= " + _itemToBuild.IndustryRemaining
                                     + "; TurnsRemaining= " + _itemToBuild.TurnsRemaining
                                     ;
                }
                else
                {
                    _text += " > no _itemToBuild, BuildQueue.Count= " + _colony.BuildQueue.Count;
                    //Build_Queue_Print(_colony);
                }

                if (_writeDirectly_Colony) Console.WriteLine(_text);
            }

            //_text = "Step_1109:; Finish of ColonyAI.Do_0_Turn_Unit ";
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            ////Console.WriteLine(_newline + _newline + "_colony_full_Report" + _newline + _newline + _colony_full_Report + _newline + "End of _colony_full_Report");

        } // End of Do_0_Turn_Unit

        private static void CheckFor_OFF_ShipProduction(Colony _colony)
        {
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_colony.Owner.CivID];

            //_civM.ShipBuildOrdered_Check();

            string _newline = Environment.NewLine;
            //string _text;

            //// Ship Building
            //int _shipNeeded_colony = _civM.Z_Ship_Colony_Needed;// - _civM.Z_Ship_Colony_Available - _civM.Z_Ship_Colony_Ordered;
            //int _civM.Z_Ship_Construction_Needed = _civM.Z_Ship_Construction_Needed;// - _civM.Z_Ship_Construction_Available - _civM.Z_Ship_Construction_Ordered;
            //int _civM.Z_Ship_Medical_Needed = _civM.Z_Ship_Medical_Needed;// - _civM.Z_Ship_Medical_Available - _civM.Z_ShipMedicalOrdered;
            //int _civM.Z_Ship_Spy_Needed = _civM.Z_Ship_Spy_Needed;// - _civM.Z_Ship_Spy_Available - _civM.Z_ShipSpyOrdered;
            //int _civM.Z_Ship_Diplomatic_Needed = _civM.Z_Ship_Diplomatic_Needed;// - _civM.Z_Ship_Diplomatic_Available - _civM.Z_ShipDiplomaticOrdered;
            //int _civM.Z_Ship_Science_Needed = _civM.Z_Ship_Science_Needed;// - _civM.Z_Ship_Science_Available - _civM.Z_ShipScienceOrdered;
            //int _civM.Z_Ship_Scout_Needed = _civM.Z_Ship_Scout_Needed;// - _civM.Z_Ship_Scout_Available - _civM.Z_ShipScoutOrdered;
            //int _civM.Z_Ship_FastAttack_Needed = _civM.Z_Ship_FastAttack_Available;// - _civM.Z_Ship_FastAttack_Available - _civM.Z_ShipFastAttackOrdered;
            ////int _shipNeeded_destroyer = _civM. - _civM.Z_ShipDestroyerAvailable - _civM.Z_ShipDestroyerOrdered;
            //int _civM.Z_Ship_Cruiser_Needed = _civM.Z_Ship_Cruiser_Needed;// - _civM.Z_Ship_Cruiser_Available - _civM.Z_ShipCruiserOrdered;
            //int _civM.Z_Ship_StrikeCruiser_Needed = _civM.Z_Ship_StrikeCruiser_Needed;// - _civM.Z_Ship_StrikeCruiser_Available - _civM.Z_ShipStrikeCruiserOrdered;
            //int _civM.Z_Ship_HeavyCruiser_Needed = _civM.Z_Ship_HeavyCruiser_Needed;// - _civM.Z_Ship_HeavyCruiser_Available - _civM.Z_ShipHeavyCruiserOrdered;
            //int _civM.Z_Ship_Command_Needed = _civM.Z_Ship_Command_Needed;// - _civM.Z_ShipCommandAvailable - _civM.Z_ShipCommandOrdered;
            //int _civM.Z_Ship_Transport_Needed = _civM.Z_Ship_Transport_Needed;// - _civM.Z_Ship_Transport_Available - _civM.Z_ShipTransportOrdered;



            Dictionary<ShipType, Tuple<int, string>> _listPrioShipBuild_tmp = new Dictionary<ShipType, Tuple<int, string>>();
            //Dictionary<ShipType, int> _listPrioShipBuild = new Dictionary<ShipType, int>();

            //Tuple<int, string> _add = new Tuple<int, string>(_civM.Z_Ship_Colony_Needed * -1, "Colony");
            //if (_civM.Z_Ship_Colony_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Colony, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Construction_Needed * -1, "Construction");
            //if (_civM.Z_Ship_Construction_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Construction, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Medical_Needed * -1, "Medical");
            //if (_civM.Z_Ship_Medical_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Medical, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Spy_Needed * -1, "Spy");
            //if (_civM.Z_Ship_Spy_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Spy, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Diplomatic_Needed * -1, "Diplomatic");
            //if (_civM.Z_Ship_Diplomatic_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Diplomatic, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Science_Needed * -1, "Science");
            //if (_civM.Z_Ship_Science_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Science, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Scout_Needed * -1, "Scout");
            //if (_civM.Z_Ship_Scout_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Scout, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_FastAttack_Needed * -1, "FastAttack");
            //if (_civM.Z_Ship_FastAttack_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.FastAttack, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Cruiser_Needed * -1, "Cruiser");
            //if (_civM.Z_Ship_Cruiser_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Cruiser, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_StrikeCruiser_Needed * -1, "StrikeCruiser");
            //if (_civM.Z_Ship_StrikeCruiser_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.StrikeCruiser, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_HeavyCruiser_Needed * -1, "HeavyCruiser");
            //if (_civM.Z_Ship_HeavyCruiser_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.HeavyCruiser, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Command_Needed * -1, "Command");
            //if (_civM.Z_Ship_Command_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Command, _add);

            //_add = new Tuple<int, string>(_civM.Z_Ship_Transport_Needed * -1, "Transport");
            //if (_civM.Z_Ship_Transport_Needed > 0) _listPrioShipBuild_tmp.Add(ShipType.Transport, _add);


            //Dictionary<ShipType,int> _listPrioShipBuild = _listPrioShipBuild_tmp.OrderByDescending(_l => _l.Value).ToList(); // doesn't sort
            //_listPrioShipBuild = _listPrioShipBuild_tmp.OrderByDescending(_l => _l.Value).ToList(); // doesn't sort
            //var sortedDict = from entry in _listPrioShipBuild_tmp orderby entry.Value ascending select entry;

            bool checkForShipProduction = true;
            //bool checkForShipProduction = false;
            if (checkForShipProduction && _colony.Owner.IsHuman)
            {
                //Debugger.Break();
            }


            //if (_colony.Shipyard.BuildSlots != null && !PlayerAI.IsInFinancialTrouble_BelowMinus2000(_colony.Owner))
            //{
            //    Handle_Ship_Production(_colony, _colony.Owner);//, _listPrioShipBuild_tmp);
            //    //    old
            //    //    if (_civ.IsEmpire) { HandleShipProductionEmpire(_colony, _civ); }
            //    //    else { HandleShipProductionMinor(_colony, _civ); }
            //}
            //else
            //{
            //    _text = GameEngine.LocationString(_colony.Location.ToString())
            //        + " " + _colony.Name
            //        + " > Empire is in financial problems and can not afford ShipBuilding ( Limit is -2000 )"
            //        ;
            //    _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civM.Civilization, _colony, _text, _text, "", SitRepPriority.RedYellow));

            //    if (_writeDirectly_Colony) Console.WriteLine("Step_1426:; " + _text);
            //    _colony_full_Report += _newline + "Step_1426:; " + _text;
            //}
        }

        private static void CheckFor_5_Intelligence_Facility(Colony _colony, BuildProject _available_item, ProductionCategory _available_item_Category)
        {
            string _newline = Environment.NewLine;
            string _text;

            int _intelligencePerPop = _colony.Population_Max / 100;
            //int _researchEachPop = 50;
            //int _researchCalc = _colony.Population_Max / _researchEachPop;

            _text = "Step_1425:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " > Check for Intelligence > "
                              + "current " + _colony.Facilities_Total5_Intelligence
                     + ", calc by maxPop= (max) " + _intelligencePerPop
                     ;

            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            if (_colony.Owner.IsHuman /*&& _colony.Name == "Sol"*/)
            {
                //Debugger.Break();
            }

            if (_colony.Facilities_Total5_Intelligence - 2 < _intelligencePerPop) // each 100 pop = 1 intel = 10%
            {
                _itemToBuild_Facility = _available_item;
            }

            if (_colony.Facilities_Total5_Intelligence /*+ 1*/ > _intelligencePerPop) // x over calc is ok
            {
                _colony.RemoveFacility(ProductionCategory.Intelligence);
                _text = "Step_1435:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Check for Intelligence > "
                        + "current " + _colony.Facilities_Total5_Intelligence
                        + ", calc by maxPop= (max) " + _intelligencePerPop
                        + " > removed ONE facility "
                        ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                // do NOT while because Total is not updated > we do one per turn
                if (_colony.Facilities_Total5_Intelligence - 1 > _intelligencePerPop)
                {
                    _colony.RemoveFacility(ProductionCategory.Intelligence);
                    _text = "Step_1437:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                            + " > " + _name_col + " ; " + _owner_col
                            + " > Check for Intelligence > "
                            + "current " + _colony.Facilities_Total5_Intelligence
                            + ", calc by maxPop= (max) " + _intelligencePerPop
                            + " > removed an amount of facilities "
                            ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }
            }
        }

        private static void CheckFor_4_Research_Facility(Colony _colony, BuildProject _available_item, ProductionCategory _available_item_Category)
        {
            int _researchEachPop = 60;
            int _researchCalc = _colony.Population_Max / _researchEachPop;
            string _newline = Environment.NewLine;
            string _text;

            _text = "Step_1464:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Research on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " > Check for Research > "
                     + "current " + _colony.Facilities_Total4_Research
                     + ", calc by maxPop= (max) " + _researchCalc
                     ;

            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            //if (_colony.Owner.IsHuman)
            //{
            //    Debugger.Break();
            //}

            if (_colony.Facilities_Total4_Research - 1 < _researchCalc) // each x pop = 1 research = 20%
            {
                _itemToBuild_Facility = _available_item;
            }

            if (_colony.Facilities_Total4_Research /*+ 1*/ > _researchCalc) // 1 over calc is ok
            {
                if (_colony.Population.CurrentValue + 70 > _colony.Population_Max)
                {
                    _colony.RemoveFacility(ProductionCategory.Research);
                    _text = "Step_1434:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Research on; "*/
                        + " > " + _name_col + " ; " + _owner_col
                        + " >  Check for Research > "
                        + "current " + _colony.Facilities_Total4_Research
                        + ", calc by maxPop= (max) " + _researchCalc
                        + " > removed ONE facility "
                        ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }
            }
        }

        private static void Print_Colony_Owner_IsHuman(Colony _colony)
        {
            _colonyAIControlled = GameEngine.IsPlayer_AIControllend();
            string _text;
            _text = Environment.NewLine + "Step_1102:; " + GameEngine.LocationString(_colony.Location.ToString()) + " *** " + _name_col + " " + _owner_col
                + " * > AIcontrolled= " + _colonyAIControlled // + " ) > Handling _colony"
                + "; BuildQueue.Count= " + _colony.BuildQueue.Count // + " ) > Handling _colony"
                                                                    //+ " Energy > Food > B_Structures > Buildings > Add_Str > Upgrades > BuildQueues "
                ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += Environment.NewLine + _text;
        }

        private static void CheckFor_2_Industry_Facility(Colony _colony, BuildProject _available_item, ProductionCategory _available_item_Category)
        {
            int _industryEachPop = _colony.Population_Max / 25;

            string _newline = Environment.NewLine;
            string _text;
            //ProductionCategory.Industry
            //if (/*_itemToBuild_Facility == null && */_available_item_Category == ProductionCategory.Industry)
            //{
            _text = "Step_1422:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                 + " > " + _name_col + " ; " + _owner_col
                 + " > Check for Industry"
                          + ", current " + _colony.Facilities_Total2_Industry
                 + ", calc by maxPop= (max) " + _industryEachPop
                 ;

            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            //if (_colony.Owner.IsHuman) { Debugger.Break(); }

            if (_colony.Facilities_Total2_Industry + 1 < _industryEachPop) // each 20 pop = 1 industry = 50%
            {
                _itemToBuild_Facility = _available_item;
            }
            else
            {
                _colony.RemoveFacility(ProductionCategory.Industry);
                _text = "Step_1522:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " RemoveFacility(ProductionCategory.Industry); "*/
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Check for Industry "
                        + "; current " + _colony.Facilities_Total2_Industry
                        + ", calc by maxPop= (max) " + _industryEachPop
                        + " > removed ONE facility "
                        ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }
            //}
        }
        private static void CheckFor_1_Food_Facility(Colony _colony, BuildProject _available_item, ProductionCategory _available_item_Category)
        {
            //try
            //{

            //int _foodEachPop = 50;
            //int _foodCalc = _colony.Population_Max / _researchEachPop;

            string _text = "Step_1421:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                     + " > " + _name_col + " ; " + _owner_col
                     + " >  Check for Food > "
                              + "current _colony.Food_Net= " + _colony.Food_Net
                     //+ _colony.Facilities_Active1_Food
                     + ", maxPop= " + _colony.Population_Max

            ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();  // checkforfood_removing a facility
            }

            //bool _build_for_food = true;

            //if (_colony.FoodReserves.CurrentValue > 500)
            //{
            //    _build_for_food = false;
            //}

            // if food is minus and all are active
            if (_colony.FoodReserves.CurrentValue < 500 && _colony.Food_Net < 0 && _colony.Facilities_Active1_Food + 1 > _colony.Facilities_Total1_Food)
            {
                //_build_for_food = true;
                _itemToBuild_Facility = _available_item;
            }

            //if (_build_for_food == true)
            //{
            //    _itemToBuild_Facility = _available_item;
            //}


            // if food is plus and active 3 and total 6 
            if (_colony.Food_Net > 10 && _colony.Facilities_Active1_Food + 1 > _colony.Facilities_Total1_Food + 2)
            {
                //_colony.RemoveFacility(ProductionCategory.Food); // no scratch for food facilities
                _text = "Step_1434:; INACTIVATE > " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Research on; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Check for Food > "
                    + "current " + _colony.Facilities_Total1_Food
                    //+ ", calc by maxPop= (max) " + _researchCalc
                    + " > removed ONE facility (INACTIVE)"
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += Environment.NewLine + _text;
            }
        }



        //private static void Handle_Research_Distribution(Colony _colony) // this 
        //{
        //    _text = "Step_1631:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
        //            + " > " + _name_col + " ; " + _owner_col
        //            + ", Handle_Research_Distribution " //+ i /*+ " of " + _total*/ + " > " + _colony.BuildQueue[i].Description

        //            ;
        //    if (_writeDirectly_Colony) Console.WriteLine(_text);
        //    _colony_full_Report += _newline + _text;

        //}

        private static void Colony_Step_10_Build_Queue_Clean(Colony _colony)
        {
            string _itemToCheck = "";
            string _text;


            List<BuildProject> _BuildQueueItemsTo_Remove = new List<BuildProject>();
            //int _total = _colony.BuildQueue.Count;

            for (int i = 0; i < _colony.BuildQueue.Count; i++)
            {
                if (i + 1 > _colony.BuildQueue.Count)
                    continue;

                _text = "Step_1431:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > BuildQueue of " + _colony.BuildQueue.Count + " > # " + i + " > " + _colony.BuildQueue[i].Description
                    //+ " > BuildQueue # " + i + " > " + _colony.BuildQueue[i].Description
                    + " (Colony_Step_10_Build_Queue_Clean) "

                    ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += _newline + _text;

                _itemToCheck = _colony.BuildQueue[i].Project.ToString();

                bool isFacility = _colony.BuildQueue[i].Project is ProductionFacilityBuildProject;

                if (!isFacility && _itemToCheck != "ShieldGenerator")
                {
                    if (_colony.BuildSlots[0].HasProject && _colony.BuildSlots[0].Project.BuildDesign.ToString() == _itemToCheck && _colony.BuildQueue.Count > 0)
                    {
                        _BuildQueueItemsTo_Remove.Add(_colony.BuildQueue[i].Project);
                    }

                    foreach (var _existing in _colony.Buildings)
                    {
                        if (_existing.BuildingDesign.ToString() == _itemToCheck && _colony.BuildQueue.Count > 0)
                            _BuildQueueItemsTo_Remove.Add(_colony.BuildQueue[i].Project);
                    }
                }
                _BuildQueueItemsTo_Remove = _BuildQueueItemsTo_Remove.Distinct().ToList();
            }

            foreach (var item in _BuildQueueItemsTo_Remove)
            {
                string _newline = Environment.NewLine;
                _text = "Step_1439:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Colony_Step_10_Build_Queue_Clean; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + " > _BuildQueueItemsTo_Remove " + " > " + item.Description

                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                for (int i = 0; i < _colony.BuildQueue.Count; i++)
                //{

                //}
                //for (int i = _colony.BuildQueue.Count; i < 0; i--)
                {
                    //Console.WriteLine(i); // just_i
                    if (_colony.BuildQueue[i].Project.ToString() == item.ToString())
                    {
                        _colony.BuildQueue.Remove(_colony.BuildQueue[i]);
                    }

                }

                //if (_colony.BuildQueue[0].Project.Contains(item))
                //{

                //}

                //_colony.BuildQueue.Remove(item);
            }
        }

        private static void Build_Queue_Print(Colony _colony)
        {
            int _total = _colony.BuildQueue.Count;
            string _text;
            for (int i = 0; i < _total; i++)
            {
                _text = "Step_1411:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + " > BuildQueue of " + _total + " > # " + i + " > " + _colony.BuildQueue[i].Description

                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += Environment.NewLine + _text;
            }
        }

        private static void Print_all_Build_Projects(IList<BuildProject> all_Build_Projects)
        {
            int count = 0;
            string _all_Build_Projects_Text = "";
            string _newline = Environment.NewLine;
            string _text;

            foreach (var item in all_Build_Projects)
            {
                string _designID_string = GameEngine.Do_x_Digit_String(4, item.BuildDesign.DesignID.ToString());

                //_text = /*_newline + */"Step_1420:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
                //                + " > " + _name_col + " ; " + _owner_col
                //                + " > _available_item or Upgrade  "

                //                + " = " + item.BuildDesign
                //                //+ ", calc by maxPop= (max) " + _colony.Population_Max / 100
                //                ;
                _text = "Step_1420:; " + GameEngine.LocationString(item.ProductionCenter.Location.ToString())
                    + " > " + _name_col
                    + " ; " + _owner_col
                    + "; IsUpgrade= " + GameEngine.BoolString_x5(item.IsUpgrade.ToString())
                    + "; BCost=;" + GameEngine.Do_x_Digit_String(5, item.BuildDesign.BuildCost.ToString())
                    + " ; TurnsNeeded=;" + GameEngine.Do_x_Digit_String(2, item.TurnsRemaining.ToString()) // not avaible
                    + " ; OPTIONS_to_Build_on #;" + GameEngine.Do_x_Digit_String(2, count.ToString())
                    + " ; ID= " + _designID_string
                    + " ; " + item.BuildDesign.ToString()

                    //+ " > OPTIONS to Build incl. Upgrades > "
                    //+ "; Morale=; " + _colony.Morale
                    //+ "; Industry_Net=;" + _colony.Industry_Net
                    ;
                count++;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += _newline + _text;
                _all_Build_Projects_Text += _newline + _text;
            } // end of _output

            // _all_Build_Projects_Text

            //if (_writeDirectly_Colony) Console.WriteLine("# Begin of _all_Build_Projects_Text" /*+ _newline */
            //    + _all_Build_Projects_Text + _newline + "# End of _all_Build_Projects_Text");

            _colony_full_Report += _newline + _all_Build_Projects_Text;
        }

        //private static void CheckBuildQueueContent(Colony _colony)
        //{
        //    _text = /*_newline + */"Step_1418:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
        //            + " > " + _name_col + " ; " + _owner_col
        //            + " > Count for BuildQueue= " + _colony.BuildQueue.Count
        //            //+ "ID= " + _designID_string
        //            //+ " = " + _available_item.BuildDesign
        //            //+ ", calc by maxPop= " + _colony.Population_Max / 100
        //            ;
        //        Build_Queue_Print(_colony);
        //    //for (int i = 0; i < _colony.BuildQueue.Count; i++)
        //    //{
        //    //    _text += _newline + "Step_1419:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Food on; "*/
        //    //        + " > " + _name_col + " ; " + _owner_col
        //    //        + " > Buildqueue # " + i + " > " + _colony.BuildQueue[i].Description;
        //    //}

        //    //if (_writeDirectly_Colony) Console.WriteLine(_text);
        //    //_colony_full_Report += _newline + _text;
        //}


        // do we need this still or is it double
        private static void SetFacility(Colony _colony, ProductionCategory _category, int _netProd, double _output, IEnumerable<ProductionCategory> _otherCategories)
        {
            double reserveFacility = Math.Floor(_netProd / _output);
            reserveFacility = Math.Max(reserveFacility, -(_colony.TotalFacilities[_category].Value - _colony.GetActiveFacilities(_category)));
            reserveFacility = Math.Min(reserveFacility, _colony.GetActiveFacilities(_category));
            int labors = _colony.GetAvailableLabor() / _colony.GetFacilityType(_category).LaborCost;
            while (reserveFacility < 0 && labors > 0)
            {
                _ = _colony.Facility_Activate(_category);
                reserveFacility++;
                labors--;
            }
            foreach (ProductionCategory c in _otherCategories)
            {
                while (reserveFacility < 0 && _colony.GetActiveFacilities(c) > 0)
                {
                    _ = _colony.Facility_Deactivate(c);
                    _ = _colony.Facility_Activate(_category);
                    reserveFacility++;
                }
            }

            // deactivate not needed
            for (int i = 0; i < reserveFacility; i++)
            {
                _ = _colony.Facility_Deactivate(_category);
            }
        }

        private static void Colony_Step_01_Check_Population(Colony _colony)
        {
            string _text;
            string _check_colony = GetCheckColony();
            if (_colony.Name == _check_colony)
            {
                //Debugger.Break();
            }

            if (_colony.Owner.IsHuman)
            {
                return;
            }

            int _popAvailable = _colony.Population.CurrentValue / 10;


            int _tmp_Active1_Food = _colony.Facilities_Active1_Food;
            int _tmp_Active2_Industry = _colony.Facilities_Active2_Industry;
            int _tmp_Active3_Energy = _colony.Facilities_Active3_Energy;
            int _tmp_Active4_Research = _colony.Facilities_Active4_Research;
            int _tmp_Active5_Intelligence = _colony.Facilities_Active5_Intelligence;
            int _tmp_Active_All = _tmp_Active1_Food + _tmp_Active2_Industry + _tmp_Active3_Energy + _tmp_Active4_Research + _tmp_Active5_Intelligence;

            int _laborPool = _colony.GetAvailableLabor() / 10;

            //Print_Labors(_colony, _laborPool, _popAvailable);

            while (_colony.Facility_Deactivate(ProductionCategory.Industry)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Research)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Intelligence)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Food)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Energy)) { }

            _laborPool = _colony.GetAvailableLabor() / 10;

            //Print_Labors(_colony, _laborPool, _popAvailable); // + " All-De-Activated !!


            //checked what's going on here setting _popAvai to Zero'
            // >> in some situations (SystemAssault or AsteroidImpact) > pop shrinked heavily and..
            // outputs have to be adapted


            // re-populate energy first

            int _energy_facilities = _colony.Facilities_Total3_Energy;
            if (_colony.Owner.IsHuman)
            {
                _text = "let the payer decide itself";
                _energy_facilities = _tmp_Active3_Energy;
            }

            for (int i = 0; i < _energy_facilities; i++)
            {
                while (_popAvailable + _laborPool > 0 && _colony.Facilities_Active3_Energy < _colony.Facilities_Total3_Energy)  // later another one is added if possible
                {
                    _colony.Facility_Activate(ProductionCategory.Energy);
                    _popAvailable -= 1;
                }
            }
            // Check if we need all pop on energy
            // CheckEnergy // next_Check / set Breakpoint

            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            Colony_Step_03_Handle_Energy_Production(_colony);

            //while (_colony.Energy_Net - _colony.GetFacilityType(ProductionCategory.Energy).UnitOutput > 0)  // later another one is added if possible
            //{
            //    _colony.Facility_Deactivate(ProductionCategory.Energy);
            //    _popAvailable += 1;
            //}

            // Food 1
            //while (_popAvailable > 0 && _colony.FoodReserves.CurrentValue > 1000 && _colony.Food_Net < -50)
            // CheckFood
            while (_popAvailable > 0 && _colony.Food_Net < -50)
            {
                _text = "Step_2347:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Pop= " + _popAvailable
                        + ", Active: Food= " + _colony.GetActiveFacilities(ProductionCategory.Food)
                        + ", Food_Net= " + _colony.Food_Net
                        //+ ", En= " + _colony.GetActiveFacilities(ProductionCategory.Energy)
                        //+ ", Res= " + _colony.GetActiveFacilities(ProductionCategory.Research)
                        //+ ", Int= " + _colony.GetActiveFacilities(ProductionCategory.Intelligence)
                        //+ ", Pool= " + _laborPool
                        + " for " + _name_col
                        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += _newline + _text;

                _colony.Facility_Activate(ProductionCategory.Food);
                _popAvailable -= 1;
            }

            // Food 2 > try to activate another Food one
            if (_popAvailable > 0 && _colony.FoodReserves.CurrentValue < 500)
            {
                _colony.Facility_Activate(ProductionCategory.Food);
                _popAvailable -= 1;

                //_text = "Step_2348:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Pop= " + _popAvailable
                //        + ", Active: Food= " + _colony.GetActiveFacilities(ProductionCategory.Food)
                //        + ", Food_Net= " + _colony.Food_Net
                //        + ", Reserve= " + _colony.FoodReserves.CurrentValue
                //        + " for " + _name_col
                //        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += _newline + _text;
            }


            // fill in facilities like _tmp = before
            while (_popAvailable > 0 && _colony.Facilities_Active4_Research < _tmp_Active4_Research)
            {
                _colony.Facility_Activate(ProductionCategory.Research);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && _colony.Facilities_Active5_Intelligence < _tmp_Active5_Intelligence)
            {
                _colony.Facility_Activate(ProductionCategory.Intelligence);
                _popAvailable -= 1;
            }

            // fill up more pop into available facilities
            while (_popAvailable > 0 && _colony.Facilities_Active2_Industry < _colony.Facilities_Total2_Industry)
            {
                _colony.Facility_Activate(ProductionCategory.Industry);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && _colony.Facilities_Active4_Research < _colony.Facilities_Total4_Research)
            {
                _colony.Facility_Activate(ProductionCategory.Research);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && _colony.Facilities_Active5_Intelligence < _colony.Facilities_Total5_Intelligence)
            {
                _colony.Facility_Activate(ProductionCategory.Intelligence);
                _popAvailable -= 1;
            }

            while (_popAvailable > 0 && _colony.Facilities_Active1_Food < _colony.Facilities_Total1_Food)
            {
                _colony.Facility_Activate(ProductionCategory.Food);
                _popAvailable -= 1;
            }

            // don't fill up energy - put to labor pool instead

            //while (_popAvailable > 1 && _colony.Facilities_Active3_Energy < _colony.Facilities_Total3_Energy)
            //{
            //    _colony.Facility_Activate(ProductionCategory.Energy);
            //    _popAvailable -= 1;
            //}

            _laborPool = _colony.GetAvailableLabor() / 10;

            //Print_Labors(_colony, _laborPool, _popAvailable);

            //_text = "Step_2346:; " + GameEngine.LocationString(_colony.Location.ToString())
            //        + " > Pool= " + _laborPool
            //            + " vs " + _popAvailable // should be zero

            //        + " ,Active: Food= " + _colony.GetActiveFacilities(ProductionCategory.Food)
            //        + " of " + _colony.GetTotalFacilities(ProductionCategory.Food)
            //        + ", Ind= " + _colony.GetActiveFacilities(ProductionCategory.Industry)
            //        + " of " + _colony.GetTotalFacilities(ProductionCategory.Industry)
            //        + ", En= " + _colony.GetActiveFacilities(ProductionCategory.Energy)
            //        + " of " + _colony.GetTotalFacilities(ProductionCategory.Energy)
            //        + ", Res= " + _colony.GetActiveFacilities(ProductionCategory.Research)
            //        + " of " + _colony.GetTotalFacilities(ProductionCategory.Research)
            //        + ", Int= " + _colony.GetActiveFacilities(ProductionCategory.Intelligence)
            //        + " of " + _colony.GetTotalFacilities(ProductionCategory.Intelligence)

            //        + " for " + _name_col
            //        + ", Pop now " + _colony.Population
            //        + " max " + _colony.Population_Max
            //        //+ " (Checking Population ...DONE)"
            //        ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            while (_colony.Facility_Activate(ProductionCategory.Industry)) { }
            while (_colony.Facility_Activate(ProductionCategory.Research)) { }
            while (_colony.Facility_Activate(ProductionCategory.Intelligence)) { }
            while (_colony.Facility_Activate(ProductionCategory.Food)) { }
            // next_Check / set Breakpoint
        }

        private static string GetCheckColony()
        {
            string _text = "return nothing";
            //return "return nothing";
            return "Ventax";
        }

        private static void Print_Labors(Colony _colony, int _laborPool, int _popAvailable)
        {
            string _newline = Environment.NewLine;
            string _text;
            _text = "Step_2346:; " + GameEngine.LocationString(_colony.Location.ToString())
        + " > Pool= " + GameEngine.Do_x_Digit_String(2, _laborPool.ToString())
            + " vs " + GameEngine.Do_x_Digit_String(2, _popAvailable.ToString()) // should be zero

        + " ,Active: Food= " + GameEngine.Do_x_Digit_String(2, _colony.GetActiveFacilities(ProductionCategory.Food).ToString())
        + " of " + GameEngine.Do_x_Digit_String(2, _colony.GetTotalFacilities(ProductionCategory.Food).ToString())
        + ", Ind= " + GameEngine.Do_x_Digit_String(2, _colony.GetActiveFacilities(ProductionCategory.Industry).ToString())
        + " of " + GameEngine.Do_x_Digit_String(2, _colony.GetTotalFacilities(ProductionCategory.Industry).ToString())
        + ", En= " + GameEngine.Do_x_Digit_String(2, _colony.GetActiveFacilities(ProductionCategory.Energy).ToString())
        + " of " + GameEngine.Do_x_Digit_String(2, _colony.GetTotalFacilities(ProductionCategory.Energy).ToString())
        + ", Res= " + GameEngine.Do_x_Digit_String(2, _colony.GetActiveFacilities(ProductionCategory.Research).ToString())
        + " of " + GameEngine.Do_x_Digit_String(2, _colony.GetTotalFacilities(ProductionCategory.Research).ToString())
        + ", Int= " + GameEngine.Do_x_Digit_String(2, _colony.GetActiveFacilities(ProductionCategory.Intelligence).ToString())
        + " of " + GameEngine.Do_x_Digit_String(2, _colony.GetTotalFacilities(ProductionCategory.Intelligence).ToString())

        + " for " + _name_col
        + ", Pop now " + GameEngine.Do_x_Digit_String(3, _colony.Population.ToString())
        + " max " + GameEngine.Do_x_Digit_String(3, _colony.Population_Max.ToString())
        //+ " (Checking Population ...DONE)"
        ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;
        }

        private static void Colony_Step_03_Handle_Energy_Production(Colony _colony)
        {
            string _text;
            _text = "complete no AI controlled ?";
            if (_colony.Owner.IsHuman)  // complete no AI controlled ?
            {
                return;
            }

            string _newline = Environment.NewLine;

            _text = "Step_1229:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Handle ENERGY on; "
                    + _name_col + " ; " + _owner_col
                    ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            double _energyOutput = _colony.GetFacilityType(ProductionCategory.Energy).UnitOutput * (1.0 + _colony.GetProductionModifier(ProductionCategory.Energy).Efficiency);
            List<Buildings.Building> _offlineBuilding = _colony.Buildings.Where(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0).ToList();
            List<OrbitalBattery> _orbBatteries = _colony.OrbitalBatteries.ToList();
            List<ShipyardBuildSlot> _offlineShipyardSlots = _colony.Shipyard == null ? new List<ShipyardBuildSlot>() : _colony.Shipyard.BuildSlots.Where(s => !s.IsActive).ToList();
            int _energy_net = _colony.Energy_Net - _offlineBuilding.Sum(b => b.BuildingDesign.EnergyCost) - _offlineShipyardSlots.Sum(s => s.Shipyard.ShipyardDesign.BuildSlotEnergyCost);

            while (_colony.Facility_Deactivate(ProductionCategory.Industry)) { } // take it from industry and...

            while (_colony.Facility_Activate(ProductionCategory.Energy)) { } // turn on all energy facilities

            int _o = 0;
            foreach (var orb in _orbBatteries)
            {

                if (_o < 5)  // 4 aktive Orb are enough
                {

                    orb.IsActive = true;
                    _o += 1;
                }
                else
                {
                    orb.IsActive = false;
                }


            }

            _text = "Step_1248:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col + " ; " + _owner_col
                + " > OrbBat: active " + _colony.OrbitalBatteries_Active + " of " + _colony.OrbitalBatteries_Total
                + ", and  > _offlineBuildings: " + _offlineBuilding.Count
                + ", _offlineShipyardSlots: " + _offlineShipyardSlots.Count
                ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            // next_Check / set Breakpoint
            // turn things on
            foreach (Buildings.Building building in _offlineBuilding)
            {
                _ = _colony.Building_Activate(building);
            }

            foreach (ShipyardBuildSlot slot in _offlineShipyardSlots)
            {
                _ = _colony.ShipyardBuildSlot_Activate(slot);
            }

            ProductionFacilityDesign facilityType = _colony.GetFacilityType(ProductionCategory.Energy);
            // next_Check / set Breakpoint



            if ((_colony.Buildings.Any(b => !b.IsActive && b.BuildingDesign.EnergyCost > 0)
                || (_colony.Shipyard?.BuildSlots.Any(s => !s.IsActive) == true)) && !_colony.IsBuilding(facilityType))
            {
                if (_colonyAIControlled)
                {
                _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, facilityType)));
                }


                _text = "Step_1247:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col + " ; " + _owner_col + " > Handle ENERGY "
                    + " > added 1 ENERGY Facility Build Order"
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }

            //SetFacility(_colony, ProductionCategory.Energy, _energy_net, _energyOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry, ProductionCategory.Food });


            // do NOT do a while here > Energy_Net is not updated > so we do it > one per turn
            if (_colony.Energy_Net/* - (int)_energyOutput*/ > (int)_energyOutput)  // later another one is added if possible
            {
                _colony.Facility_Deactivate(ProductionCategory.Energy);
                //_popAvailable += 1;
            }

            // do it 3times
            if (_colony.Energy_Net /*- (int)_energyOutput*/ > (int)_energyOutput)  // later another one is added if possible
            {
                _colony.Facility_Deactivate(ProductionCategory.Energy);
                //_popAvailable += 1;
            }

            if (_colony.Energy_Net /*- (int)_energyOutput*/ > (int)_energyOutput)  // later another one is added if possible
            {
                _colony.Facility_Deactivate(ProductionCategory.Energy);
                //_popAvailable += 1;
            }

            if (_colony.Energy_Net /*- (int)_energyOutput*/ > (int)_energyOutput)  // later another one is added if possible
            {
                _colony.Facility_Deactivate(ProductionCategory.Energy);
                //_popAvailable += 1;
            }

            if (_colony.Energy_Net - (3 * (int)_energyOutput) > (int)_energyOutput)
            {
                _colony.RemoveFacility(ProductionCategory.Energy);
                _text = "Step_1434:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Energy on; "*/
                    + " > " + _name_col + " ; " + _owner_col
                    + " >  Check for Energy > "
                    + "current " + _colony.Facilities_Total3_Energy
                    //+ ", calc by maxPop= (max) " + _researchCalc
                    + " > removed ONE facility "
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }

            var _activeOrbBat = new List<OrbitalBattery>();
            foreach (var orb in _colony.OrbitalBatteries)
            {
                if (_colony.Energy_Net < 1 && orb.IsActive)
                {
                    //orb.s
                }
            }
            //}
            //while (_colony.Energy_Net < 1)
            //{
            //    for 
            //}


            // labors might be unchanced
            //Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);

            _text = "Step_1259:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Handle ENERGY on "
                    + _name_col + " " + _owner_col
                    + " >>> _energy_net= " + _colony.Energy_Net
                    + " > OrbBat: active " + _colony.OrbitalBatteries_Active + " of " + _colony.OrbitalBatteries_Total
                    + ", _offlineBuilding= " + _offlineBuilding.Where(b => b.IsActive == false).Count()
                    + ", _offlineShipyardSlots= " + _offlineShipyardSlots.Count
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();
            }

        } // End of Colony_Step_03_Handle_Energy_Production(Colony _colony)

        private static void Colony_Step_05_Handle_Food_Production(Colony _colony)
        {
            string _newline = Environment.NewLine;
            string _text = GetCheckColony();
            if (_colony.Name == _text)
            {
                Console.WriteLine(_text);
                //Debugger.Break();
            }

            if (_colony.Owner.IsHuman)
            {
                return;
            }

            double foodOutput = _colony.GetFacilityType(ProductionCategory.Food).UnitOutput * (1.0 + _colony.GetProductionModifier(ProductionCategory.Food).Efficiency);
            //double neededFood = _colony.Food_Net + _colony.FoodReserves.CurrentValue - (10 * foodOutput);
            double neededFood = _colony.Population.CurrentValue - foodOutput;

            //SetFacility(_colony, ProductionCategory.Food, (int)neededFood, foodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research, ProductionCategory.Industry });

            double maxFoodProduction = _colony.GetProductionModifier(ProductionCategory.Food).Bonus + (_colony.GetTotalFacilities(ProductionCategory.Food) * foodOutput);

            _text = "Step_1220:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_05_Handle_Food_Production on; "
                    + _name_col + " ; " + _owner_col
                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + _colony.Population
                    + "; Food_Net= " + _colony.Food_Net
                    + ", Reserve= " + _colony.FoodReserves.CurrentValue
                    //+ " > no Upgrade INDUSTRY"
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            _text = "build another food facility ??";
            ProductionFacilityDesign facilityType = _colony.GetFacilityType(ProductionCategory.Food);
            if (_colony.GetUnusedFacilities(ProductionCategory.Food) < 4
                && _colony.Food_Net < 15 
                && _colony.FoodReserves.CurrentValue + 1 / _colony.Population.CurrentValue + 1 < 5 
                && !_colony.IsBuilding(facilityType))
            {
                _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, facilityType)));
                _text = "Step_1228:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col + " " + _owner_col + " > Handle_FOOD_Production "
                    + " > #### added 1 FOOD Facility Build Order"
                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + _colony.Population
                    + "; of " + _colony.Population_Max
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }

            if (!_colony.Owner.IsHuman && _colony.GetUnusedFacilities(ProductionCategory.Food) > 3)
            {
                _colony.RemoveFacility(ProductionCategory.Food);
                _text = "Step_1226:; " + GameEngine.LocationString(_colony.Location.ToString()) /*+ " Check for Intelligence on; "*/
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Check for FOOD > "
                        + "current " + _colony.Facilities_Total1_Food
                        //+ ", calc by maxPop= (max) " + _intelligencePerPop
                        + " > removed ONE facility "
                        ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }

            _text = "Step_1222:; " + GameEngine.LocationString(_colony.Location.ToString())
                + " > Colony_Step_05_Handle_Food_Production is DONE; "
                    + _name_col + " ; " + _owner_col

                    + "; neededFood= " + (int)neededFood
                    + "; maxFoodProduction= " + (int)maxFoodProduction
                    + "; for Pop= " + _colony.Population
                    + "; Food_Net= " + _colony.Food_Net
                    + ", Reserve= " + _colony.FoodReserves.CurrentValue
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;
            // next_Check / set Breakpoint
        }

        private static void Handle_Industry_Production(Colony _colony)
        {
            double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * _colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue) * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
            int maxProdFacility = Math.Min(_colony.TotalFacilities[ProductionCategory.Industry].Value,
                (_colony.GetAvailableLabor() / _colony.GetFacilityType(ProductionCategory.Industry).LaborCost)
                + _colony.ActiveFacilities[ProductionCategory.Intelligence].Value
                + _colony.ActiveFacilities[ProductionCategory.Research].Value
                + _colony.ActiveFacilities[ProductionCategory.Industry].Value);
            int industryNeeded = _colony.BuildSlots.Where(s => s.Project != null).Select(s => s.Project.IsRushed ? 0 : s.Project.GetCurrentIndustryCost()).Sum();
            int turnsNeeded = industryNeeded == 0 ? 0 : (int)Math.Ceiling(industryNeeded / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));
            double facilityNeeded = turnsNeeded == 0 ? 0 : Math.Truncate(((industryNeeded / turnsNeeded) - _colony.GetProductionModifier(ProductionCategory.Industry).Bonus) / prodOutput);
            double _industry_Net = -(facilityNeeded - _colony.ActiveFacilities[ProductionCategory.Industry].Value) * prodOutput;

            //Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);

            SetFacility(_colony, ProductionCategory.Industry, (int)_industry_Net, prodOutput, new[] { ProductionCategory.Intelligence, ProductionCategory.Research });

            Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);
        }

        private static void Handle_Labors(Colony _colony)
        {


            while (_colony.Facility_Activate(ProductionCategory.Research)) { }
            while (_colony.Facility_Activate(ProductionCategory.Intelligence)) { }
            while (_colony.Facility_Activate(ProductionCategory.Industry)) { }
            while (_colony.Facility_Activate(ProductionCategory.Food)) { }
            //_text = "Step_2348:; " + GameEngine.LocationString(_colony.Location.ToString())
            //        + " Pop= " + _colony.Population.CurrentValue
            //        + ", Active: Food= " + _colony.Facilities_Active1_Food
            //        + ", Ind= " + _colony.Facilities_Active2_Industry
            //        + ", En= " + _colony.Facilities_Active3_Energy
            //        + ", Res= " + _colony.Facilities_Active4_Research
            //        + ", Int= " + _colony.Facilities_Active5_Intelligence
            //        + ", Pool= " + _colony.AvailableLabor
            //        + " for " + _name_col
            //        ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            //if (boolCheckColonyProduction)
            //{
            //Debugger.Break();
            //}

        }

        private static void Handle_Food_Labors_UNDONE(Colony _colony) // in case too much food is produced
        {
        }

        private static void Handle_Labors_for_Nothing_to_Build(Colony _colony)
        {
            if (_colony.Owner.IsHuman)
            {
                return;
            }

            //if (_colony.Owner.IsHuman && _colony.Name == "Sol")
            //{
            //    Debugger.Break();
            //}

            //if (!_colony.BuildSlots[0].HasProject)  // at Buy=Rush BuildSlot always has a _proj

            //Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);

            // we don't touch energy and food because they are "special controlled"
            //while (_colony.Facility_Deactivate(ProductionCategory.Food)) { }
            //while (_colony.Facility_Activate(ProductionCategory.Food)) { }

            // take away all labors 
            while (_colony.Facility_Deactivate(ProductionCategory.Industry)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Research)) { }
            while (_colony.Facility_Deactivate(ProductionCategory.Intelligence)) { }

            _colony.Facility_Activate(ProductionCategory.Industry); // Activate at least one industry (e.g. new _colony)

            _colony.Facility_Activate(ProductionCategory.Research); // Activate up to 2 Research
            _colony.Facility_Activate(ProductionCategory.Research); // Activate up to 2 Research

            _colony.Facility_Activate(ProductionCategory.Intelligence); // Activate up to 2 Intelligence
            _colony.Facility_Activate(ProductionCategory.Intelligence); // Activate up to 2 Intelligence

            while (_colony.Facility_Activate(ProductionCategory.Industry)) { } // Activate all industry

            _colony.Facility_Deactivate(ProductionCategory.Industry); // Deactivate 2 Industry
            //_colony.Facility_Deactivate(ProductionCategory.Industry); // Deactivate 2 Industry
            //_colony.Facility_Deactivate(ProductionCategory.Industry); // Deactivate 2 Industry

            _colony.Facility_Activate(ProductionCategory.Industry); // Activate at least one industry (e.g. new _colony)


            while (_colony.Facility_Activate(ProductionCategory.Research)) { } // Activate all 
            while (_colony.Facility_Activate(ProductionCategory.Intelligence)) { } // Activate all 
            while (_colony.Facility_Activate(ProductionCategory.Industry)) { } // Activate all industry


            //Print_Labors(_colony, _colony.AvailableLabor, _colony.AvailableLabor / 10);

            //_text = "Step_2348:; " + GameEngine.LocationString(_colony.Location.ToString())
            //        + " Pop= " + _colony.Population.CurrentValue
            //        + ", Active: Food= " + _colony.Facilities_Active1_Food
            //        + ", Ind= " + _colony.Facilities_Active2_Industry
            //        + ", En= " + _colony.Facilities_Active3_Energy
            //        + ", Res= " + _colony.Facilities_Active4_Research
            //        + ", Int= " + _colony.Facilities_Active5_Intelligence
            //        + ", Pool= " + _colony.AvailableLabor
            //        + " for " + _name_col
            //        ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            //}

            //if (boolCheckColonyProduction)
            //{
            //if (_colony.Owner.IsHuman)
            //Debugger.Break();
            //}
        }

        private static void Colony_Step_65_Handle_Buildings(Colony _colony, Civilization _civ)
        {
            bool _checkHandleBuildings = true;
            string _newline = Environment.NewLine;
            string _text;
            _text = _checkHandleBuildings.ToString(); // dummy - please keep
                                                      //bool _checkHandleBuildings = false;
            if (_name_col == "Nadra")
            {
                Debugger.Break();
            }


            if (_colony.Shipyard == null)
            {
                BuildProject project = TechTreeHelper.GetBuildProjects(_colony).FirstOrDefault(bp => bp.BuildDesign is ShipyardDesign);
                if (_colony == GameContext.Current.Universe.HomeColonyLookup[_civ] && project != null && !_colony.IsBuilding(project.BuildDesign))
                {
                    _colony.BuildQueue.Add(new BuildQueueItem(project));
                }
            }

            _colony.ProcessQueue();

            //if (boolCheckColonyProduction)
            //    _text = ""; // just for breakpoint

            if (/*_colony.BuildSlots.All(t => t.Project == null) &&*/ _colony.BuildQueue.Count < 2)
            {
                _text = "Step_1202:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _colony.Name
                    + " > Colony_Step_65_Handle_Buildings: "
                    //+ "Credits.Current= " + _civM.Credits.CurrentValue
                    //+ ", Costs= " + _cost
                    //+ ", _industryNeeded= " + _industryNeeded
                    //+ ", prodOutput= " + prodOutput.ToString()
                    //+ ", _turnsNeeded= " + _turnsNeeded
                    //+ " > IsRushed for " + s.Project
                    + " on " + _name_col + " " + _owner_col
                ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                //if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
                if (_colony.BuildQueue.Count < 2)
                {
                    //INDUSTRY 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry };
                    int flexLabors = _colony.GetAvailableLabor() - 30; // flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                    if (flexLabors > -21)  // 2 more facilites as available labors
                    {
                        _text = "Step_1205:; " + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col
                            + " ; " + _owner_col
                            + " > Colony_Step_65_Handle_Buildings on INDUSTRY at "

                            + " > " + _colony.GetAvailableLabor() + " labors available"
                            ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        int totalInd = _colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && _colony.GetTotalFacilities(ProductionCategory.Industry) <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1205:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_65_Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                        else
                        {
                            //than Research
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1246:; " + GameEngine.LocationString(_colony.Location.ToString())
                                + " > " + _name_col
                                + " ; " + _owner_col
                                + " Colony_Step_65_Handle_Buildings on "
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    }
                }

                if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
                {
                    //FOOD 
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Food };
                    int flexLabors = _colony.GetAvailableLabor() /*- 30*/; // flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                    //if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                    if (flexLabors > 4)  // 2 more facilites as available labors, 10 labors = 1 facility
                    {
                        _text = "Step_1204:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_65_Handle_Buildings on INDUSTRY at "
                            + _name_col + " " + _owner_col
                            + " > " + _colony.GetAvailableLabor() + " labors available"
                            ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        int totalInd = _colony.GetTotalFacilities(ProductionCategory.Industry);
                        if (totalInd < 4 && _colony.GetTotalFacilities(ProductionCategory.Industry) <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1255:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_65_Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                        else
                        {
                            //than Research
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1256:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_65_Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Industry Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    }
                }

                if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
                {
                    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                    int flexLabors = _colony.GetAvailableLabor() /*- 30*/; // flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                    //if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                    if (flexLabors > 4)  // 2 more facilites as available labors, 10 labors = 1 facility
                    {
                        _text = "Step_1274:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_65_Handle_Buildings on "
                            + _name_col + " " + _owner_col
                            + " " + _colony.GetAvailableLabor() + " > flexLabors available"
                            ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        if (_colony.GetTotalFacilities(ProductionCategory.Industry) <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                        {
                            //Industry
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                            _text = "Step_1275:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_65_Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                        else
                        {
                            //than Research
                            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Research))));
                            _text = "Step_1276:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_65_Handle_Buildings on "
                                + _name_col + " " + _owner_col
                                + " > added 1 Research Facility Build Order"
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    }
                }

                if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
                {
                    // Industry Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIndustryProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Industry));

                    if (upgradeIndustryProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(upgradeIndustryProject));
                    }
                    else
                    {
                        _text = "Step_1216:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > "
                                + _name_col + " > " + _owner_col
                                + " > Colony_Step_65_Handle_Buildings on  > no Upgrade INDUSTRY"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;
                    }
                }

                ////structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(_colony)
                //    .OfType<StructureBuildProject>()
                //    .Where(p =>
                //            p.GetCurrentIndustryCost() > 0
                //            && EnumHelper
                //                .GetValues<ResourceType>()
                //                .Where(availableResources.ContainsKey)
                //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                //{
                //    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //}
            }

            if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2)
            {
                double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * _colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                    * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

                Dictionary<ResourceType, int> availableResources = _civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used); // availableResources
                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(_colony)
                    .OfType<StructureBuildProject>()
                    .Where(p =>
                            p.GetCurrentIndustryCost() > 0
                            && EnumHelper
                                .GetValues<ResourceType>()
                                .Where(availableResources.ContainsKey)
                                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                {
                    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                }
            }

            if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
            {
                List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                int flexLabors = _colony.GetAvailableLabor() /*- 30*/; // flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                                                                       //if (flexLabors > -21)  // 2 more facilites as available labors, 10 labors = 1 facility
                if (flexLabors > 4)  // 2 more facilites as available labors, 10 labors = 1 facility
                {
                    if (_colony.GetTotalFacilities(ProductionCategory.Industry) <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                    }
                    else
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Research))));
                    }
                }
            }

            if (_colony.BuildSlots.All(t => t.Project == null) && _colony.BuildQueue.Count < 2)
            {
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(_colony);


                foreach (var item in projects)
                {
                    _text = "Step_1211:; " + GameEngine.LocationString(_colony.Location.ToString())
                        + " > " + _name_col /*+ " " + item.Project.Location*/

                        //+ "Credits.Current= " + _civM.Credits.CurrentValue
                        + ", Costs= "
                        + ", _industryNeeded= " + item.IndustryRemaining
                        //+ ", prodOutput= " + prodOutput.ToString()
                        + ", _turnsNeeded= " + item.TurnsRemaining
                        //+ " > IsRushed for " + s.Project
                        + " Available = " + item.BuildDesign
                    ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }
            }
        }

        private static void Colony_Step_60_Handle_Basic_Structures(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text;
            _text = "Step_1231:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Colony_Step_60_Handle_Basic_Structures > BuildQueue.Count= " + _colony.BuildQueue.Count
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text + _newline;


            _colony.ProcessQueue();

            if (_colony.BuildQueue.Count > 0)
            {
                _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                        + " > Colony_Step_60_Handle_Basic_Structures > already building >; " + _colony.BuildQueue[0].Description;
                ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += /*_newline + */_text;
            }

            if (_colony.BuildQueue.Count < 2) // Colony_Step_60_Handle_Basic_Structures
            {
                double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (_colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + _colony.Morale
                //    + "; UnitOutput=;" + _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                Dictionary<ResourceType, int> availableResources = _civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used); // availableResources

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1249:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + " ; for; " + item.Key
                //        //+ "; Industry_Net=;" + _colony.Industry_Net
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += _newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(_colony).ToList()
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
                //    _text = "Step_1265:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);

                //}

                //structureProject
                StructureBuildProject structureProject = TechTreeHelper
                    .GetBuildProjects(_colony)
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


                //_text = "Step_1236:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; Industry_Net=;" + _colony.Industry_Net
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1258:; " + GameEngine.LocationString(_colony.Location.ToString()) //+ " > Colony_Step_60_Handle_Basic_Structures"
                        + " > " + _name_col
                        + " ; " + _owner_col
                        + " > Morale=; " + _colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; Industry_Net=;" + _colony.Industry_Net
                        + "; BuildQueue.Count=; " + _colony.BuildQueue.Count
                        + "; Colony_Step_60_Handle_Basic_Structures=;" + _toBuildText

                    //+ "; MathCeiling=; " + Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)_civM.Credits.CurrentValue / 4;

                if (structureProject != null && (int)structureProject.BuildDesign.BuildCost < (_civM.Credits.CurrentValue / 4))
                {
                    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                    _text = "Step_1269:; " + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col
                            + " ; " + _owner_col
                            + "; Morale=; " + _colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; Industry_Net=;" + _colony.Industry_Net
                            + "; > Added to Build=;" + structureProject.BuildDesign.ToString()

                            ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }


                //                if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2)  //2023-11-11
                //                {
                //                    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(_colony);
                //                    _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString()) 
                //                            + " > " + _name_col
                //                            + "; " + _owner_col
                //+ " Added to Build"
                //                            + "; Morale=; " + _colony.Morale
                //                            + "; prodOutput=;" + prodOutput
                //                            + "; ToBuildonSlots.Count=;" + projects.Count

                //                            ;
                //                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //                    _colony_full_Report += _newline + _text + _newline;

                //                    //int count = 0;
                //                    //foreach (var item in projects) // Colony_Step_60_Handle_Basic_Structures
                //                    //{
                //                        Print_all_Build_Projects(projects);
                //                        //_text = "Step_1235:; " + GameEngine.LocationString(_colony.Location.ToString()) 
                //                        //    + " > " + _name_col
                //                        //    + "; " + _owner_col
                //                        //    + " OPTIONS to Build > "
                //                        //    + "; Morale=; " + _colony.Morale
                //                        //    + "; Industry_Net=;" + _colony.Industry_Net
                //                        //    + "; BCost=;" + GameEngine.Do_x_Digit_String( 5, item.BuildDesign.BuildCost.ToString())
                //                        //    + "; OPTIONS_to_Build_on #;" + count
                //                        //    + "; " + item.BuildDesign.ToString()

                //                        //    ;
                //                        //count++;
                //                        //if (_writeDirectly_Colony) Console.WriteLine(_text);
                //                        //_colony_full_Report += _newline + _text;
                //                    //}
                //                }
            }
        }

        private static void Colony_Step_92_ClearUpColony(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text;
            _text = "Step_1231:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                    + " > Colony_Step_92_ClearUpColony > BuildQueue.Count= " + _colony.BuildQueue.Count
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text + _newline;

            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];


            //_colony.ProcessQueue();

            if (_colony.BuildQueue.Count > 0)
            {
                _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " > " + _name_col + " ; " + _owner_col
                        + " > Colony_Step_92_ClearUpColony > already building >; " + _colony.BuildQueue[0].Description;
                ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += /*_newline + */_text;
            }

            CheckFor_2_Industry_Facility(_colony, null, ProductionCategory.Industry);
            CheckFor_4_Research_Facility(_colony, null, ProductionCategory.Research);
            CheckFor_5_Intelligence_Facility(_colony, null, ProductionCategory.Intelligence);

            //CheckFor_2_Industry_Facility(_colony, null, ProductionCategory.Industry);



        }


        private static void Colony_Step_40_Build_for_LaborPool(Colony colony, Civilization civ)
        {
            string _newline = Environment.NewLine;
            string _text;
            ProductionCategory _available_item_Category;

            _text = "Step_1266:; " + GameEngine.LocationString(colony.Location.ToString())

                    + " > " + _name_col + " ; " + _owner_col
                    + " > Colony_Step_40_Build_for_LaborPool on; "
                    + ", BuildQueue.Count= " + colony.BuildQueue.Count
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += /*_newline + */_text + _newline;


            colony.ProcessQueue();

            int _laborAvailable = colony.AvailableLabor - 2;

            //if (_colony.BuildQueue.Count > 0) // already building
            //{
            //    _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_40_Build_for_LaborPool on; "
            //            + _name_col + " ; " + _owner_col
            //            + "; already building >; " + _colony.BuildQueue[0].Description;
            //    ;
            //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    _colony_full_Report += /*_newline + */_text;
            //}

            if (_laborAvailable > 0 && colony.BuildQueue.Count < 4) // Colony_Step_60_Handle_Basic_Structures
            {
                if (colony.Owner.IsHuman)
                {
                    //Debugger.Break();
                }

                double prodOutput = colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + _colony.Morale
                //    + "; UnitOutput=;" + _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager _civM = GameContext.Current.CivilizationManagers[civ];

                Dictionary<ResourceType, int> availableResources = _civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used);

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1249:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + " ; for; " + item.Key
                //        //+ "; Industry_Net=;" + _colony.Industry_Net
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += _newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(_colony).ToList()
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
                //    _text = "Step_1265:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);

                //}

                //structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(_colony)
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


                //_text = "Step_1236:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; Industry_Net=;" + _colony.Industry_Net
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1237:; " + GameEngine.LocationString(colony.Location.ToString())
                        + " > " + _name_col
                        + " ; " + _owner_col
                        + " > Colony_Step_40_Build_for_LaborPool"
                        + "; Morale=; " + colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; Industry_Net=;" + colony.Industry_Net
                        //+ "; ToBuild=;" + _toBuildText
                        + "; BuildQueue.Count=" + colony.BuildQueue.Count
                    //+ "; MathCeiling=; " + Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)_civM.Credits.CurrentValue / 4;

                //if (structureProject != null && (int)structureProject.BuildDesign.BuildCost < (_civM.Credits.CurrentValue / 4))
                //{
                //    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //    _text = "Step_1269:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Added to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            + "; prodOutput=;" + prodOutput
                //            + "; Industry_Net=;" + _colony.Industry_Net
                //            + "; ToBuild=;" + structureProject.BuildDesign.ToString()

                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += _newline + _text;
                //}


                //                if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2)  //2023-11-11
                //                {
                IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(colony);


                //                    _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString()) 
                //                            + " > " + _name_col
                //                            + "; " + _owner_col
                //+ " Added to Build"
                //                            + "; Morale=; " + _colony.Morale
                //                            + "; prodOutput=;" + prodOutput
                //                            + "; ToBuildonSlots.Count=;" + projects.Count

                //                            ;
                //                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //                    _colony_full_Report += _newline + _text + _newline;

                //                    //int count = 0;
                foreach (var _available_item in projects) // Colony_Step_60_Handle_Basic_Structures
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
                            CheckFor_1_Food_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Industry:
                            CheckFor_2_Industry_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Energy:
                            //CheckFor_Energy_Facility(_colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Research:
                            CheckFor_4_Research_Facility(colony, _available_item, _available_item_Category);
                            break;
                        case ProductionCategory.Intelligence:
                            CheckFor_5_Intelligence_Facility(colony, _available_item, _available_item_Category);
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
                        _text = /*_newline + */"Step_1299:; " + GameEngine.LocationString(colony.Location.ToString())
                                + " > " + _name_col + " ; " + _owner_col
                            + " > Added= " + _available_item.BuildDesign

                                + " > by using ** Build_for_Labor ** " /*+ _colony.BuildQueue.Count*/
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;
                    }

                SkipFacilities:;

                    //                        Print_all_Build_Projects(projects);
                    //                        //_text = "Step_1235:; " + GameEngine.LocationString(_colony.Location.ToString()) 
                    //                        //    + " > " + _name_col
                    //                        //    + "; " + _owner_col
                    //                        //    + " OPTIONS to Build > "
                    //                        //    + "; Morale=; " + _colony.Morale
                    //                        //    + "; Industry_Net=;" + _colony.Industry_Net
                    //                        //    + "; BCost=;" + GameEngine.Do_x_Digit_String( 5, item.BuildDesign.BuildCost.ToString())
                    //                        //    + "; OPTIONS_to_Build_on #;" + count
                    //                        //    + "; " + item.BuildDesign.ToString()

                    //                        //    ;
                    //                        //count++;
                    //                        //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //                        //_colony_full_Report += _newline + _text;
                    //                    //}

                }
            }
        }

        private static void Colony_Step_70_Handle_Additional_Structures(Colony _colony, Civilization _civ)  // unneccesary but still in
        {
            string _newline = Environment.NewLine;
            string _text;

            // Build a shipyard ?
            if (_colony.Shipyard == null)
            {
                BuildProject project = TechTreeHelper.GetBuildProjects(_colony).FirstOrDefault(bp => bp.BuildDesign is ShipyardDesign);
                if (_colony == GameContext.Current.Universe.HomeColonyLookup[_civ] && project != null && !_colony.IsBuilding(project.BuildDesign))
                {
                    _colony.BuildQueue.Add(new BuildQueueItem(project));
                }
            }

            _colony.ProcessQueue();

            if (/*_colony.BuildSlots.All(t => t.Project == null) && */_colony.BuildQueue.Count < 2) // Handle_Additional_Structure
            {
                _text = "Step_1249:; " + GameEngine.LocationString(_colony.Location.ToString())
                        + " > " + _name_col + " ; " + _owner_col
                        + " > Colony_Step_70_Handle_Additional_Structures... "
                        ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                if (_colony.Owner.IsHuman)
                {
                    //Debugger.Break();
                }

                //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)
                //{
                //    // Industry Upgrade ?
                //    ProductionFacilityUpgradeProject upgrade_IndustryProject = TechTreeHelper
                //        .GetBuildProjects(_colony)
                //        .OfType<ProductionFacilityUpgradeProject>()
                //        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Industry));

                //    if (upgrade_IndustryProject == null)
                //    {
                //        _text = "Step_1203:; " + GameEngine.LocationString(_colony.Location.ToString())
                //                + " Colony_Step_65_Handle_Buildings on; "
                //                + _name_col + " " + _owner_col
                //                + " > no Upgrade INDUSTRY"
                //                ;
                //        if (_writeDirectly_Colony) Console.WriteLine(_text);
                //        _colony_full_Report += _newline + _text;

                //    }
                //    else
                //    {
                //        _colony.BuildQueue.Add(new BuildQueueItem(upgrade_IndustryProject));
                //    }
                //}

                ////structureProject
                //StructureBuildProject structureProject = TechTreeHelper
                //    .GetBuildProjects(_colony)
                //    .OfType<StructureBuildProject>()
                //    .Where(p =>
                //            p.GetCurrentIndustryCost() > 0
                //            && EnumHelper
                //                .GetValues<ResourceType>()
                //                .Where(availableResources.ContainsKey)
                //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                //{
                //    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                //}

                int _buildDuration = 0; // just measure the duration (in Turns) to decide next build order
                foreach (var proj in _colony.BuildQueue)
                {
                    _buildDuration += proj.TurnsRemaining;
                }

                if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2) // Colony_Step_70_Handle_Additional_Structures
                {
                    double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                        * _colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue)
                        * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

                    Dictionary<ResourceType, int> availableResources = _civM.Colonies
                        .SelectMany(c => c.BuildSlots)
                        .Where(os => os.Project != null)
                        .Select(os => os.Project)
                        .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                        .GroupBy(r => r.Resource)
                        .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                        .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used);

                    //structureProject
                    StructureBuildProject structureProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<StructureBuildProject>()
                        .Where(p =>
                                p.GetCurrentIndustryCost() > 0
                                && EnumHelper
                                    .GetValues<ResourceType>()
                                    .Where(availableResources.ContainsKey)
                                    .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                        .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                    if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                        _text = /*_newline + */"Step_1297:; " + GameEngine.LocationString(_colony.Location.ToString())
                                + " > " + _name_col + " " + _owner_col
                            + " Added= " + structureProject.BuildDesign

                                + " > by using ** Colony_Step_70_Handle_Additional_Structures ** " /*+ _colony.BuildQueue.Count*/
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;
                    }
                }

                //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)
                //{
                //    List<ProductionCategory> flexProduction = new List<ProductionCategory> { ProductionCategory.Industry, ProductionCategory.Research, ProductionCategory.Intelligence };
                //    int flexLabors = _colony.GetAvailableLabor() + flexProduction.Sum(c => _colony.GetFacilityType(c).LaborCost * _colony.GetActiveFacilities(c));
                //    if (flexLabors > 0)
                //    {
                //        if (_colony.GetTotalFacilities(ProductionCategory.Industry) <= _colony.GetTotalFacilities(ProductionCategory.Research) + _colony.GetTotalFacilities(ProductionCategory.Intelligence))
                //        {
                //            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Industry))));
                //        }
                //        else
                //        {
                //            _colony.BuildQueue.Add(new BuildQueueItem(new ProductionFacilityBuildProject(_colony, _colony.GetFacilityType(ProductionCategory.Research))));
                //        }
                //    }
                //}

                //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)
                //{
                //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(_colony);
                //}
            }
        }
        // ----------------------------------------------------

        private static void Colony_Step_50_Handle_Upgrades(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text;
            _text = /*_newline + */"Step_1207:; "
                            + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col + " ; " + _owner_col
                    + " > Colony_Step_50_Handle_Upgrades: "
                    + ", BuildQueue.Count= " + _colony.BuildQueue.Count
                    + ", _colony.AvailableLabor= " + _colony.AvailableLabor
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text;

            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            //_colony.ProcessQueue();

            if (_colony.AvailableLabor > 0)
            {
                goto NoUpgradesDueToLaborPool;
            }



            //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 1 && _colony.AvailableLabor < 1)

            int _buildDuration = 0; // just measure the duration (in Turns) to decide next build order
            foreach (var proj in _colony.BuildQueue)
            {
                _buildDuration += proj.TurnsRemaining;
            }

            // Queue < 1 for not blocking the queue for a long time
            if (_buildDuration < 1 && _colony.AvailableLabor < 1)
            {
                try
                {
                    _text = "Step_1208:; "
                            + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col + " ; " + _owner_col
                            + " > Colony_Step_50_Handle_Upgrades "
                            ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;


                    //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)
                    //{
                    // Industry Upgrade ?
                    var _all = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .Where(bp => bp.IsUpgrade)
                        ;

                    ProductionFacilityUpgradeProject _productionFacilityUpgradeProject = null;
                    int _turnsNeeded = 8;

                    foreach (var item in _all)
                    {
                        _text = "Step_3558:; "
                            + GameEngine.LocationString(item.Location.ToString())
                            + " > " + item.Builder.Race.Name
                            + " > available = " + item.Description
                            ;
                        Console.WriteLine(_text);
                        //Debugger.Break();


                        if (item.FacilityDesign.Category.ToString() == ProductionCategory.Industry.ToString())
                        {
                            _productionFacilityUpgradeProject = item;
                            break; // = stop foreach
                        }

                        if (item.TurnsRemaining < _turnsNeeded || _colony.BuildQueue.IsEmpty() == true)
                        {
                            _productionFacilityUpgradeProject = item;
                            _turnsNeeded = item.TurnsRemaining;
                        }



                    }

                    if (_productionFacilityUpgradeProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(_productionFacilityUpgradeProject));

                        _text = "Step_1222:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > Upgrade > INDUSTRY"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        //Debugger.Break();
                    }

                    if (_colony.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    // seems not to work !!!
                    //ProductionFacilityUpgradeProject upgrade_IndustryProject = TechTreeHelper
                    //    .GetBuildProjects(_colony)
                    //    .OfType<ProductionFacilityUpgradeProject>()
                    //    //.Where(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Industry))
                    //    //.FirstOrDefault(bp => bp.IsUpgrade                 // ONLY !! checking the first one and not the others
                    //    //&& bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Industry)
                    //    ;

                    var _food_exists = _colony.GetFacilityType(ProductionCategory.Food);
                    var _industry_exists = _colony.GetFacilityType(ProductionCategory.Industry);
                    var _energy_exists = _colony.GetFacilityType(ProductionCategory.Energy);
                    var _research_exists = _colony.GetFacilityType(ProductionCategory.Research);
                    var _intelligence_exists = _colony.GetFacilityType(ProductionCategory.Intelligence);

                    //ProductionFacilityUpgradeProject upgrade_IndustryProject = TechTreeHelper
                    //    .GetBuildProjects(_colony)
                    //    .OfType<ProductionFacilityUpgradeProject>()
                    //    .FirstOrDefault(bp => bp.Isup))
                    //    ;

                    //upgrade_IndustryProject = TechTreeHelper
                    //    .GetBuildProjects(_colony)
                    //    .OfType<ProductionFacilityUpgradeProject>()
                    //    //.Where(o => o.IsUpgrade)
                    //    .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Industry));
                    ////.Where(bp => bp.IsUpgrade && bp.BuildDesign.EncyclopediaCategory.ToString() == Encyclopedia.EncyclopediaCategory.Facilites.ToString());

                    //if (upgrade_IndustryProject != null)
                    //{
                    //    _colony.BuildQueue.Add(new BuildQueueItem(upgrade_IndustryProject));
                    //}
                    //else
                    //{
                    //    _text = "Step_1222:; " + GameEngine.LocationString(_colony.Location.ToString())
                    //        + " > Colony_Step_50_Handle_Upgrades on "
                    //            + _name_col + ", Owner= " + _owner_col
                    //            + " > no Upgrade > INDUSTRY"
                    //            ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;
                    //}

                    // Food Upgrade ?
                    ProductionFacilityUpgradeProject upgradeFoodProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Food));

                    if (upgradeFoodProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(upgradeFoodProject));

                        _text = "Step_1221:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > Upgrade > FOOD"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        Debugger.Break();
                    }
                    else
                    {

                    }
                    // End of Food Upgrade

                    // Energy Upgrade ?
                    ProductionFacilityUpgradeProject upgradeEnergyProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Energy));

                    if (upgradeEnergyProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(upgradeEnergyProject));

                        _text = "Step_1223:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > Upgrade > ENERGY"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        Debugger.Break();
                    }
                    else
                    {

                    }
                    // End of Energy Upgrade


                    // Research Upgrade ?
                    ProductionFacilityUpgradeProject upgradeResearchProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Research));

                    if (upgradeResearchProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(upgradeResearchProject));

                        _text = "Step_1224:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > Upgrade > RESEARCH"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        Debugger.Break();
                    }
                    else
                    {

                    }
                    // End of Research Upgrade


                    // Intelligence Upgrade ?
                    ProductionFacilityUpgradeProject upgradeIntelligenceProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<ProductionFacilityUpgradeProject>()
                        .FirstOrDefault(bp => bp.FacilityDesign == _colony.GetFacilityType(ProductionCategory.Intelligence));

                    if (upgradeIntelligenceProject != null)
                    {
                        _colony.BuildQueue.Add(new BuildQueueItem(upgradeIntelligenceProject));

                        _text = "Step_1225:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                                + _name_col + ", Owner= " + _owner_col
                                + " > Upgrade > INTELLIGENCE"
                                ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        Debugger.Break();
                    }
                    else
                    {

                    }
                    // End of Intelligence Upgrade

                    //}
                }
                catch
                {
                    _text = "Step_1229:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Colony_Step_50_Handle_Upgrades on "
                            + _name_col + ", Owner= " + _owner_col
                            + " > CRASH"
                            ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;

                    Debugger.Break();

                    ////structureProject
                    //StructureBuildProject structureProject = TechTreeHelper
                    //    .GetBuildProjects(_colony)
                    //    .OfType<StructureBuildProject>()
                    //    .Where(p =>
                    //            p.GetCurrentIndustryCost() > 0
                    //            && EnumHelper
                    //                .GetValues<ResourceType>()
                    //                .Where(availableResources.ContainsKey)
                    //                .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    //    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();
                    //if (structureProject != null && Math.Ceiling(structureProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 5.0)
                    //{
                    //    _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                    //}
                }

                if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)
                {
                    double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                        * (_colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                        * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                    Dictionary<ResourceType, int> availableResources = _civM.Colonies
                        .SelectMany(c => c.BuildSlots)
                        .Where(os => os.Project != null)
                        .Select(os => os.Project)
                        .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                        .GroupBy(r => r.Resource)
                        .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                        .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used);

                    //structureProject
                    StructureBuildProject structureProject = TechTreeHelper
                        .GetBuildProjects(_colony)
                        .OfType<StructureBuildProject>()
                        .Where(p =>
                                p.GetCurrentIndustryCost() > 0
                                && EnumHelper
                                    .GetValues<ResourceType>()
                                    .Where(availableResources.ContainsKey)
                                    .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                        .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();

                    if (_colony.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    // doubled
                    //foreach (var item in TechTreeHelper
                    //    .GetBuildProjects(_colony)
                    //    .OfType<StructureBuildProject>())
                    //{
                    //    _text = "Step_1226:; > " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                    //        + " structureProject: available "
                    //        + ", _turnsNeeded= " + GameEngine.Do_x_Digit_String( 2, item.TurnsRemaining.ToString())
                    //        + ", industryRemaining= " + GameEngine.Do_x_Digit_String( 4, item.IndustryRemaining.ToString())
                    //        + ", Industry_Net= " + _net_industry_text
                    //        + ", item= " + item.BuildDesign

                    //    //+ ", Costs= " + _cost

                    //    //+ ", Industry_Net= " + item.BuildDesign.BuildCost.

                    //    //+ "; Credits.Current= " + _civM.Credits.CurrentValue
                    //    //+ " > IsRushed for " + s.Project
                    //    //+ " on " + _name_col + " " + s.Project.Location
                    //    ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;

                    //    if (boolCheckColonyProduction)
                    //        _text = ""; // just for breakpoint
                    //}

                    var _all = TechTreeHelper
    .GetBuildProjects(_colony)
    .OfType<ProductionFacilityUpgradeProject>()
    .Where(bp => bp.IsUpgrade).ToList()
    ;

                    //structureProject = _all[0].ProductionCenter.pro;



                    if (structureProject != null) //&&
                    {
                        if (_colony.Owner.IsHuman)
                        {
                            //Debugger.Break();
                        }

                        double _turns_needed = 99;

                        try
                        {
                            // searching for DivideByZeroException

                            if (structureProject != null && structureProject.TurnsRemaining > 0)
                            {
                                _turns_needed = structureProject.TurnsRemaining;
                            }

                            //_turns_needed = Math.Ceiling(structureProject.GetCurrentIndustryCost()
                            //                        // _colony.GetProductionModifier(ProductionCategory.Industry).Bonus  // does a DivideByZeroException
                            //                        + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput));
                        }
                        catch
                        {
                            Debugger.Break();
                        }

                        _text = "Step_1227:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                                + " > structureProject: available > "
                                + structureProject.TurnsRemaining + " _turnsNeeded for"
                                + " " + structureProject.BuildDesign

                                //+ ", Costs= " + _cost
                                + ", industryRemaining= " + structureProject.IndustryRemaining
                                + ", Industry_Net= " + GameEngine.Do_x_Digit_String(4, _colony.Industry_Net.ToString())
                        //+ ", Industry_Net= " + item.BuildDesign.BuildCost.

                        //+ "; Credits.Current= " + _civM.Credits.CurrentValue
                        //+ " > Math.Ceiling= " + Math.Ceiling(structureProject.GetCurrentIndustryCost()
                        //                        / _colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        //                        + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))
                        //+ " on " + _name_col + " " + GameEngine.LocationString(_colony.Location.ToString())
                        //+ "  ..( max. 8)"
                        ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        //if (Math.Ceiling(structureProject.GetCurrentIndustryCost()
                        //    / _colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        //    + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))

                        if (_turns_needed < 2.0)
                        {
                            _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                            goto StructureProjectAdded;
                        }

                        if (_buildDuration < 2 && _turns_needed < 5.0) // not more as 3 turns
                        {
                            _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                            goto StructureProjectAdded;
                        }

                        if (_buildDuration < 2 && _turns_needed < 19.0) // not more as 18 turns
                        {
                            _colony.BuildQueue.Add(new BuildQueueItem(structureProject));
                            _text = "Step_1279:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                                    + " > structureProject: available > "
                                    + structureProject.TurnsRemaining + " _turnsNeeded for"
                                    + " " + structureProject.BuildDesign

                                    //+ ", Costs= " + _cost
                                    + ", industryRemaining= " + structureProject.IndustryRemaining
                                    + ", Industry_Net= " + GameEngine.Do_x_Digit_String(4, _colony.Industry_Net.ToString())

                                    + "  ..( max. 18)"
                                //+ ", Industry_Net= " + item.BuildDesign.BuildCost.

                                //+ "; Credits.Current= " + _civM.Credits.CurrentValue
                                //+ " > Math.Ceiling= " + Math.Ceiling(structureProject.GetCurrentIndustryCost()
                                //                        / _colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                                //                        + (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))
                                //+ " on " + _name_col + " " + GameEngine.LocationString(_colony.Location.ToString())
                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    StructureProjectAdded:;
                    }

                }

            }
        NoUpgradesDueToLaborPool:;
        }

        private static void Colony_Step_85_Handle_Build_Anything(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text;
            _text = "Step_1261:; " + GameEngine.LocationString(_colony.Location.ToString())
                + " > " + _name_col + " ; " + _owner_col
                + " > Colony_Step_85_Handle_Build_Anything on; "

                    + ", BuildQueue.Count= " + _colony.BuildQueue.Count
                    ;
            if (_writeDirectly_Colony) Console.WriteLine(_text);
            _colony_full_Report += _newline + _text; // not necessary

            //if (_colony.Owner.IsHuman && _colony.Name == "Sol")
            //{
            //    Debugger.Break();
            //}


            //_colony.ProcessQueue();

            //if (_colony.BuildQueue.Count > 0)
            //{
            //    //Build_Queue_Print(_colony); // not to often

            //    //_text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString()) + "; " + _name_col + " ; " + _owner_col + " Colony_Step_85_Handle_Build_Anything on; "

            //    //        + "; already bulding >; " + _colony.BuildQueue[0].Description;
            //    //;
            //    //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //_colony_full_Report += _newline + _text; // not necessary
            //}

            _colony.ProcessQueue();

            int _buildDuration = 0; // just measure the duration (in Turns) to decide next build order
            foreach (var proj in _colony.BuildQueue)
            {
                _buildDuration += proj.TurnsRemaining;
            }

            if (_buildDuration < 2) //Colony_Step_85_Handle_Build_Anything
            {
                double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                    * (_colony.Morale.CurrentValue / (0.5f * MoraleHelper.MaxValue))
                    * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);

                // no needed
                //_text = "Step_1232:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //    + " on; " + _name_col
                //    + "; " + _owner_col
                //    + "; Morale=; " + _colony.Morale
                //    + "; UnitOutput=;" + _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput
                //    + "; prodOutput=;" + prodOutput
                //;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

                Dictionary<ResourceType, int> availableResources = _civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os.Project != null)
                    .Select(os => os.Project)
                    .SelectMany(p => EnumHelper.GetValues<ResourceType>().Select(r => new { Resource = r, Cost = p.GetCurrentResourceCost(r) }))
                    .GroupBy(r => r.Resource)
                    .Select(g => new { Resource = g.Key, Used = g.Sum(r => r.Cost) })
                    .ToDictionary(r => r.Resource, r => _civM.Resources[r.Resource].CurrentValue - r.Used);

                //foreach (var item in availableResources)
                //{
                //    _text = "Step_1255:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_85_Handle_Build_Anything"
                //            + " on; " + _name_col
                //            + "; " + _owner_col

                //            + "; StockpileGLOBAL=; " + item.Value
                //            + "; for; " + item.Key
                //        //+ "; Industry_Net=;" + _colony.Industry_Net
                //        //+ "; ToBuild=;" + _toBuildText

                //        ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += _newline + _text;
                //}

                //// just for info // doubled now 
                //var structureProject_Available = TechTreeHelper
                //    .GetBuildProjects(_colony).ToList()
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
                //    _text = "Step_1275:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Available"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; " + item.BuildDesign.Key
                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);

                //}

                //structureProject
                StructureBuildProject anyProject = TechTreeHelper.GetBuildProjects(_colony)
                    .OfType<StructureBuildProject>().Where(p => p.GetCurrentIndustryCost() > 0
                    && EnumHelper.GetValues<ResourceType>().Where(availableResources.ContainsKey).All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault();

                //anyProject
                var anyProject_Available = TechTreeHelper
                    .GetBuildProjects(_colony).ToList()
                    //.OfType<StructureBuildProject>()
                    //.Where(p =>
                    //        p.GetCurrentIndustryCost() > 0
                    //        && EnumHelper
                    //            .GetValues<ResourceType>()
                    //            .Where(availableResources.ContainsKey)
                    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                    .OrderBy(p => p.BuildDesign.BuildCost).ToList();
                ;

                OrbitalBatteryBuildProject obProject = TechTreeHelper.GetBuildProjects(_colony)
                    .OfType<OrbitalBatteryBuildProject>().FirstOrDefault();

                //foreach (var item in anyProject_Available)
                //{
                //    if (item.BuildDesign.Key.Contains("Battery"))
                //        {
                //        OrbitalBatteryBuildProject obProject = TechTreeHelper.GetBuildProjects(_colony)
                //    .OfType<OrbitalBatteryBuildProject>().FirstOrDefault();
                //    }
                //}



                //foreach (BuildProject item in most_expensive_Project_Available)
                //{
                //    var _type = most_expensive_Project_Available.GetType();

                //    _text = "Step_1233:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //                + " on; " + _name_col
                //                + "; " + _owner_col
                //                + "; Morale=; " + _colony.Morale
                //                //+ "; prodOutput=;" + prodOutput // per unit
                //                + "; Industry_Net=;" + _colony.Industry_Net
                //                + "; Available=;" + item.BuildDesign

                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);

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
                //else
                if (obProject != null)
                {
                    _toBuildText = obProject.BuildDesign.ToString();
                    if (/*_itemToBuild_Facility == null && */obProject.BuildDesign.Key.Contains("BATTERY"))
                    {
                        // each 6 turns one more OrbBat is fine
                        if (_colony.OrbitalBatteries_Total < GameContext.Current.TurnNumber / 6)
                        {
                            _itemToBuild_Facility = obProject;
                            _itemToBuild = obProject;
                        }
                    }
                    // if nothing yet build Battery or next: better...
                }

                if (anyProject != null && obProject == null)
                {
                    _text = "Step_1289:; " + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col
                            + " ; " + _owner_col
                        + " > Colony_Step_85_Handle_Build_Anything"
                            + "; Morale=; " + _colony.Morale
                            //+ "; prodOutput=;" + prodOutput // per unit
                            + "; Industry_Net=;" + _colony.Industry_Net
                            + "; ToBuild=;" + _toBuildText
                            + "; BuildQueue.Count= " + _colony.BuildQueue.Count
                        //+ "; MathCeiling=; " + Math.Ceiling(anyProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                        //        * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                        ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }


                //_text = "Step_1236:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Colony_Step_60_Handle_Basic_Structures"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            //+ "; prodOutput=;" + prodOutput // per unit
                //            + "; Industry_Net=;" + _colony.Industry_Net
                //            + "; ToBuild=;" + _toBuildText

                //        ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                _text = "Step_1239:; " + GameEngine.LocationString(_colony.Location.ToString()) //+ " > Colony_Step_85_Handle_Build_Anything"
                        + " > " + _name_col
                        + " ; " + _owner_col
                        + " ; Morale=; " + _colony.Morale
                        //+ "; prodOutput=;" + prodOutput // per unit
                        + "; Industry_Net=;" + _colony.Industry_Net
                        + "; Colony_Step_85_Handle_Build_Anything=;" + _toBuildText
                        + "; BuildQueue.Count=" + _colony.BuildQueue.Count
                    //+ "; MathCeiling=; " + Math.Ceiling(anyProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                    //        * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput)))
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                //if (anyProject != null && Math.Ceiling(anyProject.GetCurrentIndustryCost() / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus
                //    * (_colony.TotalFacilities[ProductionCategory.Industry].Value * prodOutput))) <= 9999.0)  // now 9999.0 instead of 5.0 > puts something on ..
                //    

                //.. put some on build list for buy option
                //int _credits = (int)_civM.Credits.CurrentValue / 4;

                if (_itemToBuild != null
                    && _itemToBuild.BuildDesign.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Facilites)
                {
                    //if (_colony.Owner.IsHuman) { Debugger.Break(); }

                    ProductionCategory _itemToBuild_Category = GameContext.Current.TechDatabase.ProductionFacilityDesigns[_itemToBuild.BuildDesign.DesignID].Category;
                    switch (_itemToBuild_Category)
                    {
                        case ProductionCategory.Industry:
                            CheckFor_2_Industry_Facility(_colony, _itemToBuild, ProductionCategory.Industry);
                            break;
                        case ProductionCategory.Research:
                            CheckFor_4_Research_Facility(_colony, _itemToBuild, ProductionCategory.Research);
                            break;
                        case ProductionCategory.Intelligence:
                            CheckFor_5_Intelligence_Facility(_colony, _itemToBuild, ProductionCategory.Intelligence);
                            break;
                        case ProductionCategory.Food:
                            CheckFor_1_Food_Facility(_colony, _itemToBuild, ProductionCategory.Food);
                            break;
                    }

                }

                //if (_colony.Owner.IsHuman) { Debugger.Break(); }

                if (anyProject == null && _itemToBuild != null)// && (int)anyProject.BuildDesign.BuildCost < (_civM.Credits.CurrentValue / 2))
                {
                    _colony.BuildQueue.Add(new BuildQueueItem(_itemToBuild));

                    _text = "Step_1247:; " + GameEngine.LocationString(_colony.Location.ToString()) //+ " Added to Build"
                            + " > " + _name_col
                            + " ; " + _owner_col
                            + " ; Morale=; " + _colony.Morale
                            + "; prodOutput=;" + prodOutput
                            + "; Industry_Net=;" + _colony.Industry_Net
                            + "; Added_to_Build=;" + _itemToBuild.BuildDesign.ToString()

                            ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;
                }

                // doubled
                //if (_buildDuration < 2)
                //{
                //    _colony.BuildQueue.Add(new BuildQueueItem(obProject));
                //}



                //if (/*_colony.BuildSlots.All(t => t.Project == null) && */_buildDuration < 2)  //2023-11-11
                //{
                //    IList<BuildProject> projects = TechTreeHelper.GetBuildProjects(_colony);
                //    _text = "Step_1234:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Added to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            + "; prodOutput=;" + prodOutput
                //            + "; ToBuildonSlots.Count=;" + projects.Count

                //            ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _colony_full_Report += _newline + _text;

                //    int count = 0;
                //    foreach (var item in projects)
                //    {

                //        _text = "Step_1246:; " + GameEngine.LocationString(_colony.Location.ToString()) + " OPTIONS to Build"
                //            + " on; " + _name_col
                //            + "; " + _owner_col
                //            + "; Morale=; " + _colony.Morale
                //            + "; Industry_Net=;" + _colony.Industry_Net
                //            + "; BCost=;" + GameEngine.Do_x_Digit_String( 5, item.BuildDesign.BuildCost.ToString())
                //            + "; OPTIONS_to_Build_on #;" + count
                //            + "; " + item.BuildDesign.ToString()

                //            ;
                //        count++;
                //        if (_writeDirectly_Colony) Console.WriteLine(_text);
                //        _colony_full_Report += _newline + _text;
                //    }
                //}
            }
        }
        //End of Colony_Step_85_Handle_Build_Anything

        private static void Handle_Buy_Build(Colony _colony, Civilization _civ)
        {
            string _newline = Environment.NewLine;
            string _text;

            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

            _colony.BuildSlots.Where(s => s.Project?.IsRushed == false).ToList().ForEach(s =>
            {
                List<BuildProject> otherProjects = _civM.Colonies
                    .SelectMany(c => c.BuildSlots)
                    .Where(os => os != s && os.Project != null)
                    .Select(os => os.Project)
                    .Where(p => p.GetTimeEstimate() <= 1 || p.IsRushed)
                    .ToList();

                // what does this help? ...costs for the next projects??
                //int _cost = otherProjects
                //    .Where(p => p.IsRushed)
                //    .Select(p => p.GetTotalCreditsCost())
                //    .DefaultIfEmpty()
                //    .Sum();

                int _cost = s.Project.GetTotalCreditsCost();  // we take max half of the credits

                //if ((_civM.Credits.CurrentValue - (_cost * 0.2)) > s.Project.GetTotalCreditsCost())
                if ((_civM.Credits.CurrentValue > _cost * 2 && _civM.Credits.CurrentValue > 1000))
                {
                    double prodOutput = _colony.GetFacilityType(ProductionCategory.Industry).UnitOutput * (_colony.Morale.CurrentValue
                        / (0.5f * MoraleHelper.MaxValue)) * (1.0 + _colony.GetProductionModifier(ProductionCategory.Industry).Efficiency);
                    int maxProdFacility = Math.Min(_colony.TotalFacilities[ProductionCategory.Industry].Value, (_colony.GetAvailableLabor() / _colony.GetFacilityType(ProductionCategory.Industry).LaborCost)
                        + _colony.ActiveFacilities[ProductionCategory.Intelligence].Value + _colony.ActiveFacilities[ProductionCategory.Research].Value + _colony.ActiveFacilities[ProductionCategory.Industry].Value);
                    int _industryNeeded = _colony.BuildSlots.Where(bs => bs.Project != null)
                        .Select(bs => bs.Project.IsRushed ? 0 : bs.Project.GetCurrentIndustryCost()).Sum();
                    int _turnsNeeded = _industryNeeded == 0 ? 0 : (int)Math.Ceiling(_industryNeeded / (_colony.GetProductionModifier(ProductionCategory.Industry).Bonus + (maxProdFacility * prodOutput)));

                    bool _buyQuick = false;
                    if (_cost < _civM.Credits.CurrentValue / 4 && _turnsNeeded > 4)
                    {
                        _buyQuick = true;
                    }

                    if (_buyQuick == true || _turnsNeeded > 1 && _turnsNeeded < 3 || _cost < 600)  // we buy when turnsNeede = 2 or _cost less than 600
                    {
                        _text = "Step_1210:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > Handle_Buy_Build: "
                            + "Credits.Current= " + _civM.Credits.CurrentValue
                            + ", Costs= " + _cost
                            + ", _industryNeeded= " + _industryNeeded
                            + ", prodOutput= " + (int)prodOutput
                            + ", _turnsNeeded= " + _turnsNeeded
                            + " > IsRushed for " + s.Project
                            + " on " + _name_col + " " + s.Project.Location
                        ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        s.Project.IsRushed = true;

                        _text = GameEngine.LocationString(_colony.Location.ToString())
                            + " " + _colony.Name
                            + " > Order to buy of " + s.Project.BuildDesign

                            + " ( turns needed: " + _turnsNeeded 
                            + ", costs: " + _cost + " ) "
                            + ", Credits: " + _civM.Credits.CurrentValue + " ) "
                            ;
                        _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civM.Civilization, _colony, _text, _text, "", SitRepPriority.Gray));

                        if (_writeDirectly_Colony) Console.WriteLine("Step_1422:; " + _text);
                        _colony_full_Report += _newline + "Step_1426:; " + _text;

                        //while (_colony.Facility_Deactivate(ProductionCategory.Industry)) { }  ??
                    }
                }
            });
            Handle_Labors_for_Nothing_to_Build(_colony);
        }

        private static void Handle_Ship_Production(Colony _colony, Civilization _civ) //, Dictionary<ShipType, Tuple<int, string>> _listPrioShipBuild)
        {
            //bool bool_is_human = GameEngine.IsPlayer_AIControllend;
            if (_civ.IsHuman)
                return;

            if (_colony.Shipyard == null) { return; }

            if (_colony.Shipyard.BuildQueue.Count > 1)
            { goto ProcessQueue; }

            IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            IList<BuildProject> projects = potentialProjects; // projects identical with _potentialProjects

            Sector homeSector = GameContext.Current.CivilizationManagers[_civ].SeatOfGovernment.Sector;
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

            List<ShipDesign> shipDesigns = GameContext.Current.TechTrees[_colony.OwnerID].ShipDesigns.ToList();

            string _newline = Environment.NewLine;
            string _text = /*_newline + */"Step_5780:; " + GameEngine.LocationString(_colony.Location.ToString()) + " ShipProduction > " + _name_col;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_colony_full_Report += _newline + _text;

            ShipType _neededShipType = ShipType.FastAttack; // default
            string _neededShipTypeText = "FastAttack";

            //ShipType _neededShipType;

            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            //_neededShipType = ShipType.Construction; // here more code to do

            if (_civM._neededShiptypesList != null && _civM._neededShiptypesList.Count > 1)
            {
                _neededShipTypeText = _civM._neededShiptypesList[1].ToString();
                _civM._neededShiptypesList.RemoveAt(1);
                _neededShipType = (ShipType)Enum.Parse(typeof(ShipType), _neededShipTypeText);
                //return neededShipType;// = (ShipType)Enum.Parse(typeof(ShipType), _neededShipTypeText);
            }



            BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
            if (newProject != null)
            {
                _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));
                _text = "Step_5381:; " //ShipProduction "
                    + GameEngine.LocationString(_colony.Location.ToString())
                    + " - " + _owner_col
                    //+ ""


                    + " > ShipProduction: Added Ship- _proj..." + newProject.BuildDesign

                    ;
                if (_writeDirectly_Colony)
                    Console.WriteLine(_text);
            }


            if (PlayerAI.IsInFinancialTrouble_BelowMinus2000(_colony.Owner) && _colony.Owner.Key != "BORG")
            {
                _text = "Step_5783:; "
                            + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col
                            + " ; " + _owner_col
                            + " > ShipProduction > PlayerAI.IsInFinancialTrouble_BelowMinus2000 "
                           ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                goto ProcessQueue;
                //nothing;  > _shipTypeToBuild is never "!= null"
            }

            //bool boolCheckShipProduction = true;
            //bool checkForShipProduction = true;
            //bool _shipOrderIsDone = false;

            //ShipType _shipTypeToBuild = ShipType.FastAttack; // just a dummy

            //ShipType _neededShipType = ShipType.FastAttack; // default
            //string _neededShipTypeText = "FastAttack";

            //bool _bool_listPrioShipBuild_Empty = false;

            //if (_colony.Shipyard.BuildQueue.Count > 1) 
            //      { goto ProcessQueue; }

            //IList<BuildProject> _potentialProjects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            //IList<BuildProject> projects = _potentialProjects; // projects identical with _potentialProjects

            //Sector homeSector = GameContext.Current.CivilizationManagers[_civ].SeatOfGovernment.Sector;
            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

            //List<ShipDesign> _shipDesigns = GameContext.Current.TechTrees[_colony.OwnerID].ShipDesigns.ToList();
            //List<Fleet> _fleets = GameContext.Current.Universe.FindOwned<Fleet>(_civ).ToList();
            //List<Fleet> homeFleets = homeSector.GetOwnedFleets(_civ).ToList();

            //var sortedDict = from entry in _listPrioShipBuild orderby entry.Value ascending select entry.Key;

            foreach (BuildProject _proj in potentialProjects)  // find Prio
            {
                _text = "Step_1213:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Handle_Ship_Production: "
                        + "potential < _proj.Description= " + _proj.Description
                        ;
                //if (_writeDirectly_Colony) 
                Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;
            }


            if (_colony.Owner.IsHuman)
            {
                //Debugger.Break();  // next: checking each project is one is "needed" and "avaible"
            }

            try
            {


                // just listing available projects
                foreach (BuildProject _proj in projects)
                {
                    _text = "Step_5781:; "
                            + GameEngine.LocationString(_colony.Location.ToString())
                            + " > " + _name_col
                            + " ; " + _owner_col
                            + " > ShipProduction"

                        + " (needs " + GameEngine.Do_x_Digit_String(2, _proj.TurnsRemaining.ToString()) + " Turns)"
                        + ": available= " + _proj.BuildDesign
                        ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    _colony_full_Report += _newline + _text;




                    // Ship production
                    //_potentialProjects.Add(_proj); - this is already populated

                    // this works by the sorting out of the data file 'TechObj_6_Ships.xml'
                    // if 2nd is transport > it will check for Transport and block the BuildQueue with it

                    //if (_proj.Description.Contains("COLONY")) CheckFor_ColonyShip(_colony, _civM, ShipType.Colony, _proj);
                    //if (_proj.Description.Contains("MEDICAL")) CheckFor_BuildShip(_colony, _civM, ShipType.Medical, _proj);
                    //if (_proj.Description.Contains("SPY")) CheckFor_BuildShip(_colony, _civM, ShipType.Spy, _proj);
                    //if (_proj.Description.Contains("DIPLOMATIC")) CheckFor_BuildShip(_colony, _civM, ShipType.Diplomatic, _proj);

                    //if (_proj.Description.Contains("COMMAND")) CheckFor_BuildShip(_colony, _civM, ShipType.Command, _proj);
                    //if (_proj.Description.Contains("CRUISER")) CheckFor_BuildShip(_colony, _civM, ShipType.Command, _proj); // includes Heavy and StrikeCruiser
                    //if (_proj.Description.Contains("DESTROYER")) CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //if (_proj.Description.Contains("FRIGATE")) CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //if (_proj.Description.Contains("SCOUT")) CheckFor_BuildShip(_colony, _civM, ShipType.Scout, _proj);
                    //if (_proj.Description.Contains("SCIENCE")) CheckFor_BuildShip(_colony, _civM, ShipType.Science, _proj);

                    //if (_proj.Description.Contains("TRANSPORT")) CheckFor_BuildShip(_colony, _civM, ShipType.Transport, _proj);
                    //if (_proj.Description.Contains("CONSTRUCTION")) CheckFor_BuildShip(_colony, _civM, ShipType.Construction, _proj);

                    //}

                    #region oldstuff
                    //Dictionary<int, ShipType> _listPrioShipBuild_tmp = new Dictionary<int, ShipType>();
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Colony_Needed, ShipType.Colony);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Construction_Needed, ShipType.Construction);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Medical_Needed, ShipType.Medical);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Spy_Needed, ShipType.Spy);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Diplomatic_Needed, ShipType.Diplomatic);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Science_Needed, ShipType.Science);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Scout_Needed, ShipType.Scout);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_FastAttack_Needed, ShipType.FastAttack);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Cruiser_Needed, ShipType.Cruiser);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_StrikeCruiser_Needed, ShipType.StrikeCruiser);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_HeavyCruiser_Needed, ShipType.HeavyCruiser);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Command_Needed, ShipType.Command);
                    //_listPrioShipBuild_tmp.Add(_civM.Z_Ship_Transport_Needed, ShipType.Transport);

                    //int _gp = GameContext.Current.TurnNumber / 10;

                    //// first value is basic requirement
                    //int _civM.Z_Ship_Colony_Needed = 1 + _civM.Z_Ship_Colony_Needed;// - _civM.Z_Ship_Colony_Available - _civM.Z_Ship_Colony_Ordered;
                    //int _civM.Z_Ship_Construction_Needed = 1 + _civM.Z_Ship_Construction_Needed;// - _civM.Z_Ship_Construction_Available - _civM.Z_Ship_Construction_Ordered;
                    //int _civM.Z_Ship_Medical_Needed = 1 + _civM.Z_Ship_Medical_Needed;// - _civM.Z_Ship_Medical_Available - _civM.Z_ShipMedicalOrdered;
                    //int _civM.Z_Ship_Spy_Needed = 0 + _civM.Z_Ship_Spy_Needed;// - _civM.Z_Ship_Spy_Available - _civM.Z_ShipSpyOrdered;
                    //int _civM.Z_Ship_Diplomatic_Needed = _gp + _civM.Z_Ship_Diplomatic_Needed;// - _civM.Z_Ship_Diplomatic_Available - _civM.Z_ShipDiplomaticOrdered;
                    //int _civM.Z_Ship_Science_Needed = 1 + _civM.Z_Ship_Science_Needed;// - _civM.Z_Ship_Science_Available - _civM.Z_ShipScienceOrdered;
                    //int _civM.Z_Ship_Scout_Needed = 1 + _civM.Z_Ship_Scout_Needed;// - _civM.Z_Ship_Scout_Available - _civM.Z_ShipScoutOrdered;
                    //int _civM.Z_Ship_FastAttack_Needed = 1 + _civM.Z_Ship_FastAttack_Available;// - _civM.Z_Ship_FastAttack_Available - _civM.Z_ShipFastAttackOrdered;
                    ////int _shipNeeded_destroyer = _civM. - _civM.Z_ShipDestroyerAvailable - _civM.Z_ShipDestroyerOrdered;
                    //int _civM.Z_Ship_Cruiser_Needed = 1 + _civM.Z_Ship_Cruiser_Needed;// - _civM.Z_Ship_Cruiser_Available - _civM.Z_ShipCruiserOrdered;

                    //int _civM.Z_Ship_StrikeCruiser_Needed = _gp + _civM.Z_Ship_StrikeCruiser_Needed;// - _civM.Z_Ship_StrikeCruiser_Available - _civM.Z_ShipStrikeCruiserOrdered;
                    //int _civM.Z_Ship_HeavyCruiser_Needed = _gp + _civM.Z_Ship_HeavyCruiser_Needed;// - _civM.Z_Ship_HeavyCruiser_Available - _civM.Z_ShipHeavyCruiserOrdered;
                    //int _civM.Z_Ship_Command_Needed = _gp + _civM.Z_Ship_Command_Needed;// - _civM.Z_ShipCommandAvailable - _civM.Z_ShipCommandOrdered;
                    //int _civM.Z_Ship_Transport_Needed = _gp + _civM.Z_Ship_Transport_Needed;// - _civM.Z_Ship_Transport_Available - _civM.Z_ShipTransportOrdered;

                    //if (GameContext.Current.TurnNumber < 9)
                    //{ _civM.Z_Ship_Colony_Needed = +10; }

                    //int _ship_Total_Needed =
                    //      _civM.Z_Ship_Colony_Needed
                    //    + _civM.Z_Ship_Construction_Needed
                    //    + _civM.Z_Ship_Medical_Needed
                    //    + _civM.Z_Ship_Spy_Needed
                    //    + _civM.Z_Ship_Diplomatic_Needed
                    //    + _civM.Z_Ship_Science_Needed
                    //    + _civM.Z_Ship_Scout_Needed
                    //    + _civM.Z_Ship_FastAttack_Needed
                    //    + _civM.Z_Ship_Cruiser_Needed
                    //    + _civM.Z_Ship_StrikeCruiser_Needed
                    //    + _civM.Z_Ship_HeavyCruiser_Needed
                    //    + _civM.Z_Ship_Command_Needed
                    //    + _civM.Z_Ship_Transport_Needed
                    //    ;
                    //ggg


                    //_text = "Step_5785:; "
                    //            + GameEngine.LocationString(_colony.Location.ToString())
                    //            + " > " + _name_col
                    //            + " ; " + _owner_col
                    //            + " > ShipProduction"
                    //            + " > _listPrioShipBuild.Count= " + _listPrioShipBuild.Count
                    //    ;
                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += _newline + _text;

                    //if (_listPrioShipBuild.Count == 0)
                    //{
                    //    //
                    //    //_text = _bool_listPrioShipBuild_Empty.ToString(); // dummy, just keep

                    //    //_bool_listPrioShipBuild_Empty = true;
                    //}
                    //else
                    //{
                    //if (_listPrioShipBuild.Count > 1)
                    //{
                    //    _listPrioShipBuild.OrderByDescending(_l => _l.Value);
                    //}

                    //_text = _listPrioShipBuild[0].Item2.ToString(); // > this crashes on Item2
                    #endregion oldstuff

                    //if (_civ.Key.Contains("Botha"))
                    //{
                    //    //Debugger.Break();
                    //}


                    //if (_civM._neededShiptypesList != null && _civM._neededShiptypesList.Count > 1)
                    //{
                    //    _neededShipTypeText = _civM._neededShiptypesList[1].ToString();
                    //    _civM._neededShiptypesList.RemoveAt(1);
                    //}

                    //_neededShipType = _listPrioShipBuild.;
                    //switch (_neededShipTypeText)
                    //{
                    //    case "COLONY":
                    //        _neededShipType = ShipType.Colony;
                    //        break;
                    //    case "CONSTRUCTION":
                    //        _neededShipType = ShipType.Construction;
                    //        break;
                    //    case "MEDICAL":
                    //        _neededShipType = ShipType.Medical;
                    //        break;
                    //    case "TRANSPORT":
                    //        _neededShipType = ShipType.Transport;
                    //        break;
                    //    case "SPY":
                    //        _neededShipType = ShipType.Spy;
                    //        break;
                    //    case "DIPLOMATIC":
                    //        _neededShipType = ShipType.Science;
                    //        break;
                    //    case "SCIENCE":
                    //        _neededShipType = ShipType.Colony;
                    //        break;
                    //    case "SCOUT":
                    //        _neededShipType = ShipType.Scout;
                    //        break;
                    //    case "FASTATTACK":
                    //        _neededShipType = ShipType.FastAttack;
                    //        break;
                    //    case "CRUISER":
                    //        _neededShipType = ShipType.Cruiser;
                    //        break;
                    //    case "HEAVYCRUISER":
                    //        _neededShipType = ShipType.HeavyCruiser;
                    //        break;
                    //    case "STRIKECRUISER":
                    //        _neededShipType = ShipType.StrikeCruiser;
                    //        break;
                    //    case "COMMAND":
                    //        _neededShipType = ShipType.Command;
                    //        break;
                    //        //default":
                    //        //    break;
                    //}


                    if (GameContext.Current.TurnNumber < 15)
                    {
                        _neededShipType = ShipType.Construction;
                        _neededShipTypeText = "Construction";
                    }

                    if (GameContext.Current.TurnNumber < 7)
                    {
                        _neededShipType = ShipType.Colony;
                        _neededShipTypeText = "Colony";
                    }


                    if (GameContext.Current.TurnNumber < 14 && _colony.Owner.Key == "BORG")
                    {
                        _neededShipType = ShipType.Construction;
                        _neededShipTypeText = "Construction";
                    }


                    // Already checked before but here to hover the count
                    if (_colony.Shipyard.BuildQueue.Count > 0) { goto ProcessQueue; }

                    _shipOrderIsDone = false;

                    // 
                    if (_civM._neededShiptypesList != null)
                    {


                        foreach (var item in _civM._neededShiptypesList)
                        {
                            if (item.ToString() == "dummy")
                            {
                                // nothing
                            }
                            else
                            {
                                _text = "Step_7766:; " + _civM.Civilization
                                    + " >  _neededShiptypesList= " + item.ToString()
                                    //+ " for " + _civ
                                    ;
                                Console.WriteLine(_text);
                            }
                        }
                    }

                    //check _listPrioShipBuild2

                    //var _listPrioShipBuild2 = new List<ShipType>();// _listPrioShipBuild2 is out of the options if first list is empty


                    //foreach (BuildProject _proj in _potentialProjects)  // find Prio
                    //{
                    //    _text = "Step_1213:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Handle_Ship_Production: "
                    //            + "potential < _proj.Description= " + _proj.Description
                    //            ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;
                    //}

                    //foreach (BuildProject _proj in _potentialProjects)  // find Prio
                    //{
                    //    //if (checkForShipProduction)
                    //    //    _text = ""; /*just for breakpoint*/

                    if (_colony.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    //    if (_colony.Shipyard.BuildQueue.Count > 1)
                    //    {
                    //        continue;
                    //    }

                    if (potentialProjects.Count == 1) // build the only existing option
                    {
                        // just use Proj[0], ShipType.Medical = DUMMY
                        _text = "Step_1209:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Only this one available > "
                            + potentialProjects[0].BuildDesign

                            ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                        _colony_full_Report += _newline + _text;

                        BuildShipType(_colony, _civM, ShipType.Medical); //, potentialProjects[0]);
                        _shipOrderIsDone = true;
                        goto ProcessQueue;
                    }


                    //_text = "Step_1213:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Handle_Ship_Production: "
                    //        + "potential < _proj.Description= " + _proj.Description
                    //        ;
                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_colony_full_Report += _newline + _text;

                    //if (_colony.Owner.IsHuman)
                    //{
                    //    Debugger.Break();
                    //}


                    // above we had already selected a "neededshiptype"
                    if (_shipOrderIsDone == false)
                    {
                        switch (_neededShipTypeText)
                        {
                            case "Colony":
                                _neededShipType = ShipType.Colony;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Colony, _proj);
                                break;
                            case "Construction":
                                _neededShipType = ShipType.Construction;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Construction, _proj);
                                break;
                            case "MedicalL":
                                _neededShipType = ShipType.Medical;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Medical, _proj);
                                break;
                            case "Transport":
                                _neededShipType = ShipType.Transport;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Transport, _proj);
                                break;
                            case "Spy":
                                _neededShipType = ShipType.Spy;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Spy, _proj);
                                break;
                            case "Diplomatic":
                                _neededShipType = ShipType.Science;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Science, _proj);
                                break;
                            case "Science":
                                _neededShipType = ShipType.Science;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Science, _proj);
                                break;
                            case "Scout":
                                _neededShipType = ShipType.Scout;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Scout, _proj);
                                break;
                            case "FastAttack":
                                _neededShipType = ShipType.FastAttack;
                                CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                                break;
                            case "CRUISER":
                                _neededShipType = ShipType.Cruiser;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Cruiser, _proj);
                                break;
                            case "HEAVYCRUISER":
                                _neededShipType = ShipType.HeavyCruiser;
                                CheckFor_BuildShip(_colony, _civM, ShipType.HeavyCruiser, _proj);
                                break;
                            case "STRIKECRUISER":
                                _neededShipType = ShipType.StrikeCruiser;
                                CheckFor_BuildShip(_colony, _civM, ShipType.StrikeCruiser, _proj);
                                break;
                            case "COMMAND":
                                _neededShipType = ShipType.Command;
                                CheckFor_BuildShip(_colony, _civM, ShipType.Command, _proj);
                                break;
                        }
                    }
                    //if (_civM._neededShiptypesList.Contains("COMBATANT"))
                    //{
                    //    if (_proj.Description.Contains("COMMAND"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.Command, _proj);
                    //    if (_proj.Description.Contains("CRUISER"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.Cruiser, _proj); // includes Heavy and StrikeCruiser
                    //    if (_proj.Description.Contains("DESTROYER"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //    if (_proj.Description.Contains("FRIGATE"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //    if (_proj.Description.Contains("FIGHTER"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //    if (_proj.Description.Contains("SURVEYOR"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //    if (_proj.Description.Contains("RAIDER"))// && _civM._neededShiptypesList.Contains("COMBATANT")) //COMMAND"))
                    //        CheckFor_BuildShip(_colony, _civM, ShipType.FastAttack, _proj);
                    //}

                    //if (_proj.Description.Contains("SCOUT") && _civM._neededShiptypesList.Contains("SCOUT")) //COMMAND"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Scout, _proj);
                    //if (_proj.Description.Contains("SCIENCE") && _civM._neededShiptypesList.Contains("SCIENCE"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Science, _proj);
                    //if (_proj.Description.Contains("TRANSPORT") && _civM._neededShiptypesList.Contains("TRANSPORT"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Transport, _proj);
                    //if (_proj.Description.Contains("CONSTRUCTION") && _civM._neededShiptypesList.Contains("CONSTRUCTION"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Construction, _proj);
                    //if (_proj.Description.Contains("MEDICAL") && _civM._neededShiptypesList.Contains("MEDICAL"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Medical, _proj);
                    //if (_proj.Description.Contains("SPY") && _civM._neededShiptypesList.Contains("SPY"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Spy, _proj);
                    //if (_proj.Description.Contains("DIPLOMATIC") && _civM._neededShiptypesList.Contains("DIPLOMATIC"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Diplomatic, _proj);
                    //if (_proj.Description.Contains("COLONY") && _civM._neededShiptypesList.Contains("COLONY"))
                    //    CheckFor_BuildShip(_colony, _civM, ShipType.Colony, _proj);

                    //_text = _shipOrderIsDone.ToString();

                    //if (_proj.Description.Contains("COLONY") && _civM.Z_Ship_Colony_Needed > 0)
                    //{
                    //    _text = "Step_1210:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Handle_Buy_Build: "
                    //            + "_civM.Z_Ship_Colony_Needed= " + _civM.Z_Ship_Colony_Needed
                    //        //+ ", Costs= " + _cost
                    //        //+ ", _industryNeeded= " + _industryNeeded
                    //        //+ ", prodOutput= " + prodOutput.ToString()
                    //        //+ ", _turnsNeeded= " + _turnsNeeded
                    //        //+ " > IsRushed for " + s.Project
                    //        //+ " on " + _name_col + " " + s.Project.Location
                    //        ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;

                    //    if (_colony.Owner.IsHuman)
                    //    {
                    //        Debugger.Break();
                    //    }

                    //    CheckFor_ColonyShip(_colony, _civM, ShipType.Colony, _proj);
                    //}



                    // _colony ships done above or better in > CheckFor_ColonyShip

                    //foreach (var item in _civM._neededShiptypesList)
                    //{
                    //    if (item.ToString() == "dummy")
                    //    {
                    //        // nothing
                    //    }
                    //    else
                    //    {
                    //        _text = _civM.Civilization
                    //            + " _neededShiptypesList= " + item.ToString()
                    //            ;
                    //        Console.WriteLine(_text);
                    //    }
                    //}

                    //if (_colony.Owner.IsHuman)
                    //{
                    //    Debugger.Break();
                    //}





                    //if (_ship_Total_Needed < 1) // no ship is needed
                    //{

                    //if (_civM.Civilization.IsHuman)
                    //{
                    //    Debugger.Break();
                    //}


                    //if (_shipOrderIsDone == false && _proj.Description.Contains("COMMAND"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Command, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}

                    //if (_shipOrderIsDone == false && _proj.Description.Contains("CRUISER"))
                    //{
                    //    if (_shipOrderIsDone = false && _proj.Description.Contains("HEAVY_CRUISER"))
                    //    {
                    //        if (_proj.TurnsRemaining < 10)
                    //        {
                    //            BuildShipType(_colony, _civM, ShipType.HeavyCruiser, _proj);
                    //        }
                    //        //_listPrioShipBuild2.Add(ShipType.Command); 
                    //        _shipOrderIsDone = true;
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.HeavyCruiser); _shipOrderIsDone = true;


                    //    if (_shipOrderIsDone == false && _proj.Description.Contains("STRIKE_CRUISER"))
                    //    {
                    //        if (_proj.TurnsRemaining < 10)
                    //        {
                    //            BuildShipType(_colony, _civM, ShipType.StrikeCruiser, _proj);
                    //        }
                    //        //_listPrioShipBuild2.Add(ShipType.Command); 
                    //        //_shipOrderIsDone = true;
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.StrikeCruiser); _shipOrderIsDone = true;
                    //    //}

                    //    if (_shipOrderIsDone == false && _proj.Description.Contains("CRUISER"))
                    //    {
                    //        if (_proj.TurnsRemaining < 10)
                    //        {
                    //            BuildShipType(_colony, _civM, ShipType.Cruiser, _proj);
                    //        }
                    //        //_listPrioShipBuild2.Add(ShipType.Command); 
                    //        _shipOrderIsDone = true;
                    //    }
                    //}


                    //if (_shipOrderIsDone == false && _proj.Description.Contains("DESTROYER"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.FastAttack, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}

                    //if (_shipOrderIsDone == false && _proj.Description.Contains("FRIGATE"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.FastAttack, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    ////_listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
                    //if (_shipOrderIsDone == false && _proj.Description.Contains("FIGHTER"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.FastAttack, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    ////_listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
                    //if (_shipOrderIsDone == false && _proj.Description.Contains("SURVEYOR"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.FastAttack, _proj);

                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    ////_listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
                    //if (_shipOrderIsDone == false && _proj.Description.Contains("RAIDER"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.FastAttack, _proj);

                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    ////_listPrioShipBuild2.Add(ShipType.FastAttack); _shipOrderIsDone = true; }
                    //if (_shipOrderIsDone == false && _proj.Description.Contains("SCOUT"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Scout, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    ////_listPrioShipBuild2.Add(ShipType.Scout); _shipOrderIsDone = true; }



                    //if (_proj.Description.Contains("SCIENCE")) { _listPrioShipBuild2.Add(ShipType.Science); _shipOrderIsDone = true; }
                    //if (_proj.Description.Contains("MEDICAL")) { _listPrioShipBuild2.Add(ShipType.Medical); _shipOrderIsDone = true; }
                    //if (_proj.Description.Contains("COLONY")) { _listPrioShipBuild2.Add(ShipType.Colony); _shipOrderIsDone = true; }




                    //if (_proj.Description.Contains("CONSTRUCTION")) { _listPrioShipBuild2.Add(ShipType.Construction); _shipOrderIsDone = true; }
                    //if (_shipOrderIsDone = false && _proj.Description.Contains("CONSTRUCTION"))
                    //{
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Command, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}

                    //    if (_proj.Description.Contains("TRANSPORT")) {
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Command, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    //_listPrioShipBuild2.Add(ShipType.Transport); _shipOrderIsDone = true; }
                    //    if (_proj.Description.Contains("DIPOMATIC")) {
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Command, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    //_listPrioShipBuild2.Add(ShipType.Diplomatic); _shipOrderIsDone = true; }
                    //    if (_proj.Description.Contains("SPY")) {
                    //    if (_proj.TurnsRemaining < 10)
                    //    {
                    //        BuildShipType(_colony, _civM, ShipType.Command, _proj);
                    //    }
                    //    //_listPrioShipBuild2.Add(ShipType.Command); 
                    //    _shipOrderIsDone = true;
                    //}
                    //_listPrioShipBuild2.Add(ShipType.Spy); _shipOrderIsDone = true; }

                    //needed_ShipType_1 = ShipType.;
                    //break; 

                    //if (_proj.Description.Contains("CRUISER")) {
                    //    _listPrioShipBuild2.Add(ShipType.Cruiser); 
                    //    //needed_ShipType_1 = ShipType.Cruiser; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("DESTROYER")) {
                    //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
                    //    //needed_ShipType_1 = ShipType.FastAttack; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("FRIGATE")) {
                    //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
                    //    //needed_ShipType_1 = ShipType.FastAttack; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("FIGHTER")) {
                    //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
                    //    //needed_ShipType_1 = ShipType.FastAttack; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("SURVEYOR")) {
                    //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
                    //    //needed_ShipType_1 = ShipType.FastAttack; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("RAIDER")) {
                    //    _listPrioShipBuild2.Add(ShipType.FastAttack); 
                    //    //needed_ShipType_1 = ShipType.FastAttack; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("SCOUT")) {
                    //    _listPrioShipBuild2.Add(ShipType.Scout); 
                    //    //needed_ShipType_1 = ShipType.Scout; 
                    //    //break; 
                    //}
                    //if (_proj.Description.Contains("SCIENCE")) {
                    //    _listPrioShipBuild2.Add(ShipType.Science); 
                    //    //needed_ShipType_1 = ShipType.Science; 
                    //    //break; 
                    //}
                    //}
                    //checked Map.txt and JNAI Surveyor
                    //#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                    //if (_shipTypeToBuild == null || PlayerAI.IsInFinancialTrouble_BelowMinus2000(_colony.Owner))
                    //{
                    //    goto ProcessQueue;
                    //    //nothing;  > _shipTypeToBuild is never "!= null"
                    //}
                    //else
                    //{
                    //    BuildShipType(_colony, _civM, _shipTypeToBuild, _proj);  // _proj is relevant here !!!
                    //}
                    //#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'


                    //}

                    //doubled
                    //int xy = 0;
                    //foreach (var item in _colony.Shipyard.BuildQueue)
                    //{
                    //    xy += 1;
                    //    _text = _text = "Step_5787:; " + GameEngine.LocationString(_colony.Location.ToString())
                    //        + " ShipProduction-Queue # " + xy
                    //        + " - " + _owner_col

                    //        + " (needs " + item.TurnsRemaining + " Turns)"
                    //        + ": in queue= " + item.Project.BuildDesign
                    //        ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    _colony_full_Report += _newline + _text;
                    //}

                    //if (boolCheckShipProduction)
                    //{
                    //    //Debugger.Break();
                    //}


                    if (_colony.Shipyard.BuildQueue.Count > 1)
                    {
                        goto ProcessQueue;
                    } // only for Colony Ships we try to order 2 ones


                    //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));

                    //CheckFor_SystemsToColonizeProject(_colony);
                    //int _shipcolonyNeeded = _civM.Z_Ship_Colony_Needed - _civM.Z_Ship_Colony_Available - _civM.Z_Ship_Colony_Ordered;
                    //if (_shipcolonyNeeded > 0)
                    //{
                    //    needed_ShipType_1 = ShipType.Colony;
                    //    BuildShipType(_colony, _civM, needed_ShipType_1, _proj);
                    //    if (_shipcolonyNeeded > 2) 
                    //    { 
                    //        needed_ShipType_2 = ShipType.Colony; 
                    //        BuildShipType(_colony, _civM, needed_ShipType_2, _proj);
                    //        goto ProcessQueue;
                    //    }
                    //}

                    //_text = "Step_5782:; ShipProduction_2"
                    //        + " at " + GameEngine.LocationString(_colony.Location.ToString())
                    //        + " - " + _owner_col
                    //        + ": ColonyShips: Available= " + _civM.Z_Ship_Colony_Available
                    //        + ", Needed= " + _civM.Z_Ship_Colony_Needed
                    //        + ", Ordered= " + _civM.Z_Ship_Colony_Ordered

                    //        ;
                    //if (_writeDirectly_Colony) Console.WriteLine(_text);

                    //BuildShipType(_colony, _civM, needed_ShipType_1, _proj); needed_1_done = true;
                    //BuildShipType(_colony, _civM, needed_ShipType_2, _proj); needed_2_done = true;

                    //if (_colony.Shipyard.BuildQueue.Count > 1) { goto ProcessQueue; } // only for Colony Ships we try to order 2 ones
                    //else...
                    //_civM.Z_Ship_Spy_Needed = 2; // always 2 Medical needed as storage
                    //int _shipMedicalNeeded = _civM.Z_Ship_Medical_Needed - _civM.Z_Ship_Medical_Available - _civM.Z_ShipMedicalOrdered;

                    // Medical
                    //while (_colony.Shipyard.BuildQueue.Count > 1)
                    //{
                    //    _civM.Z_Ship_Medical_Needed = 2; // always 2 Medical needed as storage
                    //    int _shipMedicalNeeded = _civM.Z_Ship_Medical_Needed - _civM.Z_Ship_Medical_Available - _civM.Z_ShipMedicalOrdered;
                    //    if(_shipMedicalNeeded > 0) BuildShipType(_colony, _civM, ShipType.Medical, _proj);
                    //}

                    //// Spy
                    //while (_colony.Shipyard.BuildQueue.Count > 1)
                    //{
                    //    _civM.Z_Ship_Spy_Needed = 1; // always 2 Medical needed as storage
                    //    int _ShipSpyNeeded = _civM.Z_Ship_Spy_Needed - _civM.Z_Ship_Spy_Available - _civM.Z_ShipSpyOrdered;
                    //    if (_ShipSpyNeeded > 0) BuildShipType(_colony, _civM, ShipType.Spy, _proj);
                    //}

                    //// Spy
                    //while (_colony.Shipyard.BuildQueue.Count > 1)
                    //{
                    //    _civM.Z_Ship_Spy_Needed = 1; // always 2 Medical needed as storage
                    //    int _ShipSpyNeeded = _civM.Z_Ship_Spy_Needed - _civM.Z_Ship_Spy_Available - _civM.Z_ShipSpyOrdered;
                    //    if (_ShipSpyNeeded > 0) BuildShipType(_colony, _civM, ShipType.Spy, _proj);
                    //}



                    //if (_civM.Z_Ship_Colony_Needed > _civM.Z_Ship_Colony_Available && _colony.Sector.GetOwnedFleets(_civ).All(o => !o.IsColonizer) &&
                    //    !_shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
                    //if (needed_ShipType_1 == ShipType.Colony) BuildShipType(_colony, _civM, needed_ShipType_1);
                    //if (needed_ShipType_2 == ShipType.Colony) BuildShipType(_colony, _civM, needed_ShipType_2);
                    //{
                    //    BuildShipType(_colony, _civM);
                    //    //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    //    //if (_proj != null)
                    //    //{
                    //    //    _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //    //    _text = "Step_5383:; ShipProduction "
                    //    //        + " at " + GameEngine.LocationString(_colony.Location.ToString())
                    //    //        + " > " + _name_col
                    //    //        + " - " + _owner_col
                    //    //        + ": Added Colonizer _proj..." + _proj.BuildDesign
                    //    //        ;
                    //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);

                    //    //    _civM.Z_Ship_Colony_Ordered += 1;
                    //    //}
                    //}


                    // Construction
                    //if (_civM.Z_Ship_Construction_Available < 2 &&
                    //    _colony.Sector.GetOwnedFleets(_civ).All(o => !o.IsConstructor) &&
                    //    !_shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(_colony.Shipyard.IsBuilding))
                    //{
                    //    //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    //    if (_proj != null)
                    //    {
                    //        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //        _text = "Step_5384:; ShipProduction "
                    //            + " at " + GameEngine.LocationString(_colony.Location.ToString())
                    //            + " - " + _owner_col
                    //            + ": Added Construction ship _proj..." + _proj.BuildDesign

                    //            ;
                    //        if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    }
                    //}


                    // Military
                    //Fleet defenseFleet = homeSector.GetOwnedFleets(_civ).FirstOrDefault(o => o.AITypeUnit == AITypeUnit.SystemDefense);
                    //if ((defenseFleet?.HasCommandShip != true) &&
                    //    homeFleets.All(o => !o.HasCommandShip) &&
                    //    !_shipDesigns.Where(o => o.ShipType == ShipType.Command).Any(_colony.Shipyard.IsBuilding))
                    //{
                    //    //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Command && p.BuildDesign == d));
                    //    if (_proj != null)
                    //    {
                    //        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //    }
                    //}
                    //if ((defenseFleet == null || defenseFleet.Ships.Count < 5) &&
                    //    homeFleets.Where(o => o.IsBattleFleet).Sum(o => o.Ships.Count) < 5 &&
                    //    !_shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(_colony.Shipyard.IsBuilding))
                    //{
                    //    //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                    //    if (_proj != null)
                    //    {
                    //        _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    //    }
                    //    if (_proj != null)
                    //    {
                    //        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //    }
                    //}

                    //// Exploration - HomeSector has Starting Scouts
                    //if (!_shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(_colony.Shipyard.IsBuilding))
                    //{
                    //    for (int i = _fleets.Count(o => o.IsScout); i < NOT_USED_NumScouts; i++)
                    //    {
                    //        //BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
                    //        if (_proj != null)
                    //        {
                    //            _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //        }
                    //    }
                    //}


                    //} // end of HomeSector

                    // all Colonies - build _colony ships
                    //        if (GameContext.Current.Universe.FindOwned<Colony>(_civ).Count < MaxEmpireColonyCount &&
                    //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
                    //!_shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
                    //        {
                    //            BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    //            if (_proj != null)
                    //            {
                    //                _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                    //            }
                    //        }

                    // not HomeSector or especially SeatOfGovernment
                    //if (_colony.Sector != homeSector && _colony.Shipyard != null)
                    //if (_colony.Shipyard != null && _colony.Shipyard.BuildQueue.Count == 0)
                    //{
                    //    _text = "Step_5360:; " + GameEngine.LocationString(_colony.Location.ToString()) //+ " next: check for ShipProduction - not at HomeSector: "
                    //        + " " + _colony.Shipyard.Design

                    //        + " at " + _name_col
                    //        + ", Owner= " + _owner_col
                    //        + " - here no ship is building - maybe on the next code"
                    //        ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    //CheckFor_SystemsToColonizeProject(_colony);
                    //}


                    // this builds a colonizer - why only colonizer ?
                    if (_colony.Shipyard.BuildSlots.Any(t => t.Project == null) && _colony.Shipyard.BuildQueue.Count == 0)
                    {
                        //if (_colony.Owner.IsHuman)
                        //{
                        //    Debugger.Break();
                        //}

                        IList<BuildProject> projects2 = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
                        //foreach (BuildProject _proj in projects2)
                        //{
                        //    _text = "ShipProduction at HomeSector: "
                        //        + " at " + GameEngine.LocationString(_colony.Location.ToString())
                        //        + " - " + _owner_col
                        //        + ": available= " + _proj.BuildDesign

                        //        ;
                        //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                        //}

                        //if (boolCheckShipProduction)
                        //    _text = ""; // just for breakpoint

                        //if (_neededShipType == ShipType.Medical)  // medical is set as default
                        //{
                        //    if (_listPrioShipBuild2.Contains(ShipType.Colony)) _neededShipType = ShipType.Colony;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Medical)) _neededShipType = ShipType.Medical;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Spy)) _neededShipType = ShipType.Spy;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Diplomatic)) _neededShipType = ShipType.Diplomatic;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Construction)) _neededShipType = ShipType.Construction;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Transport)) _neededShipType = ShipType.Transport;

                        //    if (_listPrioShipBuild2.Contains(ShipType.Scout)) _neededShipType = ShipType.Scout;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Science)) _neededShipType = ShipType.Science;
                        //    if (_listPrioShipBuild2.Contains(ShipType.FastAttack)) _neededShipType = ShipType.FastAttack;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Cruiser)) _neededShipType = ShipType.Cruiser;
                        //    if (_listPrioShipBuild2.Contains(ShipType.HeavyCruiser)) _neededShipType = ShipType.HeavyCruiser;
                        //    if (_listPrioShipBuild2.Contains(ShipType.StrikeCruiser)) _neededShipType = ShipType.StrikeCruiser;
                        //    if (_listPrioShipBuild2.Contains(ShipType.Command)) _neededShipType = ShipType.Command;
                        //}




                        //_neededShipType = ShipType.Construction; // here more code to do
                        //if (needed_ShipType_1 != null)
                        //    _neededShipType = needed_ShipType_1;

                        if (_colony.Owner.IsHuman)
                        {
                            //Debugger.Break();
                        }


                        //BuildProject newProject = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
                        if (newProject != null && _colony.Shipyard.BuildQueue.Count < 1)
                        {
                            _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));

                            _text = "Step_5388:; "
                                + GameEngine.LocationString(_colony.Location.ToString())
                                + " > " + _name_col
                                + " ; " + _owner_col
                                + " > ShipProduction"

                                + ": Added Construction _proj..." + newProject.BuildDesign

                                ;
                            if (_writeDirectly_Colony) Console.WriteLine(_text);
                            _colony_full_Report += _newline + _text;
                        }
                    }



                    //foreach (var item in _colony.Shipyard.BuildQueue)
                    //{
                    //    _text = "Step_5387:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                    //        + ", ShipProduction > " + item.Project.BuildDesign
                    //        + ", TurnsRemaining= " + item.Project.TurnsRemaining


                    //        ;
                    //    if (_writeDirectly_Colony) Console.WriteLine(_text);

                }
            }
            catch (Exception e)
            {
                _text = "Step_5387:; " + e;
                Console.WriteLine(_text);
                Debugger.Break();
            }

        ProcessQueue:;
            _colony.Shipyard.ProcessQueue();
        }

        //private static ShipType GetNeededShiptype(CivilizationManager _civM, out ShipType neededShipType)
        //{
        //    string _neededShipTypeText = _civM._neededShiptypesList[1].ToString();
        //    _civM._neededShiptypesList.RemoveAt(1);
        //    neededShipType = (ShipType)Enum.Parse(typeof(ShipType), _neededShipTypeText);
        //    return neededShipType;// = (ShipType)Enum.Parse(typeof(ShipType), _neededShipTypeText);
        //}

        //}
        //}


        //private static void PopulateEmptyPrioShipBuild(CivilizationManager _civM, ShipType _shiptype)
        //{
        //    if (_bool_listPrioShipBuild_Empty)
        //    {
        //        _civM._listPrioShipBuild.Add(_shiptype, 1);
        //    }
        //}

        private static void CheckFor_ColonyShip(Colony _colony, CivilizationManager _civM, ShipType _shipType, BuildProject _project)
        {
            string _newline = Environment.NewLine;
            string _text;
            bool boolCheckShipProduction = true;
            //bool boolCheckShipProduction = true;
            ShipType needed_ShipType_1 = new ShipType();
            ShipType needed_ShipType_2 = new ShipType(); // in case two slots are free in one turn

            //if (boolCheckShipProduction)
            if (_colony.Owner.IsHuman)
            {
                Debugger.Break();
            }


            //CheckFor_SystemsToColonizeProject(_colony);

            //int _shipcolonyNeeded = _civM.Z_Ship_Colony_Needed - _civM.Z_Ship_Colony_Available - _civM.Z_Ship_Colony_Ordered;
            if (_civM.Z_Ship_Colony_Needed > 0)
            {
                needed_ShipType_1 = ShipType.Colony;
                BuildShipType(_colony, _civM, needed_ShipType_1); //, _project);
                if (_civM.Z_Ship_Colony_Needed > 2)
                {
                    needed_ShipType_2 = ShipType.Colony;
                    BuildShipType(_colony, _civM, needed_ShipType_2); //, _project);
                    //goto ProcessQueue;
                }
            }

            _text = "Step_5793:; " + GameEngine.LocationString(_colony.Location.ToString()) + " ShipProduction"
                    + " at " + _name_col
                    //+ " - " + _owner_col
                    + ": ColonyShips: Available= " + _civM.Z_Ship_Colony_Available
                    + ", Needed= " + _civM.Z_Ship_Colony_Needed
                    + ", Ordered= " + _civM.Z_Ship_Colony_Ordered

                    ;
            _colony_full_Report += _newline + _text;
            if (boolCheckShipProduction)
            {
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                //_colony_full_Report += _newline + _text;
            }


            //BuildShipType(_colony, _civM, needed_ShipType_1, _proj); //needed_1_done = true;
            //BuildShipType(_colony, _civM, needed_ShipType_2, _proj); //needed_2_done = true;
        }

        private static void CheckFor_BuildShip(Colony _colony, CivilizationManager _civM, ShipType _shipType, BuildProject _project)
        {
            string _newline = Environment.NewLine;
            string _text;

            if (_colony.Shipyard == null) { return; }
            if (_colony.Shipyard.BuildQueue.Count > 1)
            {
                return;
            }

            //switch (_shipType)
            //{
            //    case ShipType.Colony:
            //        //if (_needed)
            //        _civM.Z_Ship_Colony_Ordered += 1;
            //        break;
            //    case ShipType.Construction:
            //        _civM.Z_Ship_Construction_Ordered += 1;
            //        break;
            //    case ShipType.Medical:
            //        _civM.Z_ShipMedicalOrdered += 1;
            //        break;
            //    case ShipType.Transport:
            //        _civM.Z_ShipTransportOrdered += 1;
            //        break;
            //    case ShipType.Spy:
            //        _civM.Z_ShipSpyOrdered += 1;
            //        break;
            //    case ShipType.Diplomatic:
            //        _civM.Z_ShipDiplomaticOrdered += 1;
            //        break;
            //    case ShipType.Science:
            //        _civM.Z_ShipScienceOrdered += 1;
            //        break;
            //    case ShipType.Scout:
            //        _civM.Z_ShipScoutOrdered += 1;
            //        break;
            //    case ShipType.FastAttack:
            //        _civM.Z_ShipFastAttackOrdered += 1;
            //        break;
            //    case ShipType.Cruiser:
            //        _civM.Z_ShipCruiserOrdered += 1;
            //        break;
            //    case ShipType.HeavyCruiser:
            //        _civM.Z_ShipHeavyCruiserOrdered += 1;
            //        break;
            //    case ShipType.StrikeCruiser:
            //        _civM.Z_ShipStrikeCruiserOrdered += 1;
            //        break;
            //    case ShipType.Command:
            //        _civM.Z_ShipCommandOrdered += 1;
            //        break;
            //    default:
            //        break;
            //}

            if (_project.TurnsRemaining < 10)
            {


                BuildShipType(_colony, _civM, _shipType); //, _project); //needed_1_done = true;
                _text = "Step_5784:; " + GameEngine.LocationString(_colony.Location.ToString()) + " ShipProduction"
                        + " at " + _name_col
                        + ", Owner= " + _owner_col
                        + " > ordered ShipBuilding-Tpye: " + _shipType
                        //+ ", Needed= " + _civM.Z_Ship_Colony_Needed
                        //+ ", Ordered= " + _civM.Z_Ship_Colony_Ordered

                        ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += _newline + _text;

                _shipOrderIsDone = true;

                //BuildShipType(_colony, _civM, needed_ShipType_2, _proj); //needed_2_done = true;
            }
        }

        private static void BuildShipType(Colony _colony, CivilizationManager _civM, ShipType _shipType) //, BuildProject _proj)
        {
            //string _newline = Environment.NewLine;
            //string _text;
            //if (_shipType == null)
            //    return;

            // Medical is a placeholder if only one is available
            //if (_shipType == ShipType.Medical)
            //{
            //    _text = "Step_5381:; just to be notified"
            //        ;
            //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //}

            IList<BuildProject> _potentialProjects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            List<ShipDesign> _shipDesigns = GameContext.Current.TechTrees[_colony.OwnerID].ShipDesigns.ToList();

            BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == _shipType && p.BuildDesign == d));
            if (_proj != null)
            {
                _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                string _text = "Step_5383:; " + GameEngine.LocationString(_colony.Location.ToString())
                    + " BuildShipType > ShipProduction at"
                    + " > " + _name_col
                    + ", Owner= " + _owner_col
                    + ": Added Ship Build _proj..." + _proj.BuildDesign
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                _colony_full_Report += Environment.NewLine + _text;
            }
        }


        //TODO: Move ship production out of _colony AI. It requires a greater oversight than just a single _colony
        //TODO: Is there any need for separate functions for empires and minor races? > 2024: I guess: no !!!
        //TODO: Break these functions up into smaller chunks
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
        private static void Handle_Shipx_OFF_ProductionEmpire(Colony _colony, Civilization _civ)
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079 // Remove unnecessary suppression

        {
            if (_colony.Shipyard == null)
            {
                return;
            }
            string _text;

            //>Check ShipProduction

            IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            List<ShipDesign> shipDesigns = GameContext.Current.TechTrees[_colony.OwnerID].ShipDesigns.ToList();
            List<Fleet> fleets = GameContext.Current.Universe.FindOwned<Fleet>(_civ).ToList();
            Sector homeSector = GameContext.Current.CivilizationManagers[_civ].SeatOfGovernment.Sector;
            List<Fleet> homeFleets = homeSector.GetOwnedFleets(_civ).ToList();


            IList<BuildProject> projects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            //foreach (BuildProject _proj in projects)
            //{
            //    _text = "ShipProduction_2"
            //        + " at " + GameEngine.LocationString(_colony.Location.ToString())
            //        + " - " + _owner_col
            //        + ": available= " + _proj.BuildDesign

            //        ;
            //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //}

            if (_colony.Sector == homeSector)
            {
                _text = "Step_5380:; ShipProduction at " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                    //+ " - Not Habited: Habitation= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);

                // Colonization
                //neededColonizer = 0;

                //CheckFor_SystemsToColonizeProject(_colony);

                int _colonizerAvailable = GameContext.Current.Universe.FindOwned<Fleet>(_colony.Owner).Where(f => f.IsColonizer).Count();

                //if (neededColonizer > _colonizerAvailable)
                //{
                //    neededColonizer -= 1;
                //    need1Colonizer = true;
                //}



                ////if (GameContext.Current.Universe.FindOwned<Colony>(_civ).Count < MaxEmpireColonyCount &&
                ////    //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
                ////    //need1Colonizer &&
                ////    !_shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
                //if (need1Colonizer && _colony.Sector.GetOwnedFleets(_civ).All(o => !o.IsColonizer) &&
                //    !_shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
                //{
                //    BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                //    if (_proj != null)
                //    {
                //        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                //        _text = "Step_5384: ShipProduction "
                //            + " at " + GameEngine.LocationString(_colony.Location.ToString())
                //            + " > " + _name_col
                //            + " - " + _owner_col
                //            + ": Added Colonizer _proj..." + _proj.BuildDesign

                //            ;
                //        if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    }
                //}


                // Construction
                if (_colony.Sector.Station == null &&
                    _colony.Sector.GetOwnedFleets(_civ).All(o => !o.IsConstructor) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        _text = "Step_5386:; ShipProduction "
                            + " at " + GameEngine.LocationString(_colony.Location.ToString())
                            + " - " + _owner_col
                            + ": Added Construction ship _proj..." + project.BuildDesign

                            ;
                        if (_writeDirectly_Colony) Console.WriteLine(_text);
                    }
                }


                // Military
                Fleet defenseFleet = homeSector.GetOwnedFleets(_civ).FirstOrDefault(o => o.AITypeUnit == UnitAIType.SystemDefense);
                if ((defenseFleet?.HasCommandShip != true) &&
                    homeFleets.All(o => !o.HasCommandShip) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Command).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Command && p.BuildDesign == d));
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                if ((defenseFleet == null || defenseFleet.Ships.Count < 5) &&
                    homeFleets.Where(o => o.IsBattleFleet).Sum(o => o.Ships.Count) < 5 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                    if (project != null)
                    {
                        project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    }
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }

                // Exploration - HomeSector has Starting Scouts
                //if (!_shipDesigns.Where(o => o.ShipType == ShipType.Scout).Any(_colony.Shipyard.IsBuilding))
                //{
                //    for (int i = _fleets.Count(o => o.IsScout); i < NOT_USED_NumScouts; i++)
                //    {
                //        BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Scout && p.BuildDesign == d));
                //        if (_proj != null)
                //        {
                //            _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
                //        }
                //    }
                //}


            } // end of HomeSector

            // all Colonies - build _colony ships
            //        if (GameContext.Current.Universe.FindOwned<Colony>(_civ).Count < MaxEmpireColonyCount &&
            //GameContext.Current.TurnNumber % ColonyShipEveryTurns == 0 &&
            //!_shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
            //        {
            //            BuildProject _proj = _potentialProjects.LastOrDefault(p => _shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
            //            if (_proj != null)
            //            {
            //                _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(_proj));
            //            }
            //        }

            // not HomeSector or especially SeatOfGovernment
            if (_colony.Sector != homeSector && _colony.Shipyard != null)
            {
                _text = "Step_5390:; " + GameEngine.LocationString(_colony.Location.ToString()) + "next: check for ShipProduction - not at HomeSector: "
                    + _colony.Shipyard.Design
                    + " - " + _owner_col
                    + " at " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                    + " - here no ship is building - maybe on the next code"
                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
                //CheckFor_SystemsToColonizeProject(_colony);
            }


            // this builds a colonizer - why only colonizer ?
            if (_colony.Shipyard.BuildSlots.Any(t => t.Project == null) && _colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects2 = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
                //foreach (BuildProject _proj in projects2)
                //{
                //    _text = "ShipProduction at HomeSector: "
                //        + " at " + GameEngine.LocationString(_colony.Location.ToString())
                //        + " - " + _owner_col
                //        + ": available= " + _proj.BuildDesign

                //        ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //}

                ShipType _neededShipType;

                _neededShipType = ShipType.Construction; // here more code to do

                BuildProject newProject = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == _neededShipType && p.BuildDesign == d));
                if (newProject != null)
                {
                    _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(newProject));
                    _text = "Step_5386:; ShipProduction "
                        + " at " + GameEngine.LocationString(_colony.Location.ToString())
                        + " - " + _owner_col
                        + ": Added Colonizer _proj..." + newProject.BuildDesign

                        ;
                    if (_writeDirectly_Colony) Console.WriteLine(_text);
                }
            }

            foreach (var item in _colony.Shipyard.BuildQueue)
            {
                _text = "Step_5387:; " + GameEngine.LocationString(_colony.Location.ToString()) + " > " + _name_col
                    + ", ShipProduction > " + item.Project.BuildDesign
                    + ", TurnsRemaining= " + item.Project.TurnsRemaining


                    ;
                if (_writeDirectly_Colony) Console.WriteLine(_text);
            }

            _colony.Shipyard.ProcessQueue();
        }

        //private static void CheckFor_SystemsToColonizeProject(Colony _colony)
        //{
        //    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_colony.Owner.CivID];
        //    // need a fleet for getting a range for IsSectorWithinFuelRange
        //    Fleet fleet = GameContext.Current.Universe.FindOwned<Fleet>(_colony.Owner).Where(f => f.IsColonizer).FirstOrDefault();
        //    if (fleet == null)
        //        return;

        //    _text = "Step_5393:; " + GameEngine.LocationString(fleet.Location.ToString()) + " using " + fleet.Ships[0].ObjectID + " " + fleet.Ships[0].Design + " > CheckFor_SystemsToColonizeProject..."
        //            //+ " - Not Habited: Habitation Aim= "
        //            //+ item.HasColony
        //            //+ " at " + item.Location
        //            //+ " - " + item.Owner
        //            ;
        //    if (_writeDirectly_Colony) Console.WriteLine(_text);

        //    var possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
        //        .Where(c => c.Sector != null && c.IsInhabited == false && c.IsHabitable(_colony.Owner.Race) == true
        //        && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet) && DiplomacyHelper.IsTravelAllowed(_colony.Owner, c.Sector)) /*&& mapData.IsScanned(c.Location)*/
        //        //&& mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
        //        //)//Where other science ship is not already going
        //        //.Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location))
        //        .ToList();

        //    foreach (var item in possibleSystems)
        //    {
        //        string _ownerText = "No Owner";
        //        if (item.Owner != null)
        //        {
        //            _ownerText = item.Owner.Key;
        //        }
        //        _text = "Step_5396:; " + GameEngine.LocationString(_colony.Location.ToString()) + " Check for possible Colony at " + _name_col
        //            + " - possible: " + possibleSystems.Count
        //            + " - inhabited ? > " + item.IsInhabited //" for HasColony"


        //            + " > at " + GameEngine.LocationString(item.Location.ToString())
        //            + " - " + _ownerText
        //            ;
        //        if (_writeDirectly_Colony) Console.WriteLine(_text);
        //    }
        //    //neededColonizer = possibleSystems.Count;
        //    _civM.z_ShipColonyNeeded = possibleSystems.Count - 1;  // set _civM.z_ShipColonyNeeded
        //}

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
        private static void Handle_Shipx_OFF_ProductionMinor(Colony _colony, Civilization _civ)

#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079 // Remove unnecessary suppression
        {
            if (_colony.Shipyard == null)
            {
                return;
            }

            IList<BuildProject> potentialProjects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            ShipDesign[] shipDesigns = GameContext.Current.TechTrees[_colony.OwnerID].ShipDesigns.ToArray();
            //var _fleets = GameContext.Current.Universe.FindOwned<Fleet>(_civ);
            Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[_civ].Sector;

            if (_colony.Sector == homeSector)
            {

                // Colonization
                if (_civ.CivilizationType == CivilizationType.ExpandingPower &&
                    //GameContext.Current.Universe.FindOwned<Colony>(_civ).Count < NOT_USED_MaxMinorColonyCount &&
                    //GameContext.Current.TurnNumber % NOT_USED_ColonyShipEveryTurnsMinor == 0 &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Colony).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Colony && p.BuildDesign == d));
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Construction
                if (_civ.CivilizationType == CivilizationType.ExpandingPower &&
                    _colony.Sector.Station == null &&
                    _colony.Sector.GetOwnedFleets(_civ).All(o => !o.IsConstructor) &&
                    !shipDesigns.Where(o => o.ShipType == ShipType.Construction).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Construction && p.BuildDesign == d));
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
                // Military
                Fleet defenseFleet = homeSector.GetOwnedFleets(_civ).FirstOrDefault(o => o.AITypeUnit == UnitAIType.SystemDefense);
                if (_civ.CivilizationType != CivilizationType.MinorPower)
                {
                    if ((defenseFleet == null || defenseFleet.Ships.Count < 2) &&
                        !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack || o.ShipType == ShipType.Cruiser).Any(_colony.Shipyard.IsBuilding))
                    {
                        BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.Cruiser && p.BuildDesign == d));
                        if (project != null)
                        {
                            project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                        }
                        if (project != null)
                        {
                            _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                        }
                    }
                }
                else if ((defenseFleet == null || defenseFleet.Ships.Count < 2) && !shipDesigns.Where(o => o.ShipType == ShipType.FastAttack).Any(_colony.Shipyard.IsBuilding))
                {
                    BuildProject project = potentialProjects.LastOrDefault(p => shipDesigns.Any(d => d.ShipType == ShipType.FastAttack && p.BuildDesign == d));
                    if (project != null)
                    {
                        _colony.Shipyard.BuildQueue.Add(new BuildQueueItem(project));
                    }
                }
            }

            if (_colony.Shipyard.BuildSlots.All(t => t.Project == null) && _colony.Shipyard.BuildQueue.Count == 0)
            {
                IList<BuildProject> projects = TechTreeHelper.GetShipyardBuildProjects(_colony.Shipyard);
            }

            _colony.Shipyard.ProcessQueue();
        }
    }
}