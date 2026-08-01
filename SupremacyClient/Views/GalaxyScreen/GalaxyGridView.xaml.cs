// File:GalaxyGridView.xaml.cs
//using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Input;
using Supremacy.Client.Views.GalaxyScreen;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Supremacy.Client.Views
{
    public partial class GalaxyGridView : IWeakEventListener
    {
        private readonly IUnityContainer _container;
        private readonly IAppContext _appContext;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly DelegateCommand<object> _revealMapCommand;
        private readonly DelegateCommand<object> _outputMapCommand;
        private readonly DelegateCommand<object> _outputMapSectorInfoCommand;
        private readonly DelegateCommand<object> _cheatMenuCommand;
        private readonly DelegateCommand<object> _f12_ScreenCommand;
        private readonly DelegateCommand<object> _f11_ScreenCommand;
        private readonly DelegateCommand<object> _f10_ScreenCommand;
        private readonly DelegateCommand<object> _f09_ScreenCommand;
        private readonly DelegateCommand<object> _f08_ScreenCommand;
        private readonly DelegateCommand<object> _f07_ScreenCommand;
        private readonly DelegateCommand<object> _f06_ScreenCommand;
        //private readonly string _path_Resources_Data_Addon = Path.Combine(ResourceManager.GetResourcePath(".\\Resources\\Data\\Addon");

        //[NonSerialized]
        //private readonly string _newline = Environment.NewLine;
        //private readonly string _text_all_out;
        //private string _restriction_text;
        //private string _ownerText;
        //private bool writeDirectly = true;
        //#pragma warning disable IDE0051 // Remove unused private members
        //private readonly IMusicPlayer _musicPlayer;
        //private readonly ISoundPlayer _soundPlayer;
        //#pragma warning restore IDE0051 // Remove unused private members


        #region Constructors and Finalizers
        public GalaxyGridView([NotNull] IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException("container");
            _appContext = _container.Resolve<IAppContext>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            InitializeComponent();

            bool writeDirectly = true;
            string _text = "";
            string _newline = Environment.NewLine;

            _text = "Step_2101:; GalaxyGridView Initialize...";
            if (writeDirectly)
                Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);

            Loaded += delegate
                           {
                               GalaxyGrid.Update();
                               GalaxyGrid.SelectedSector = _appContext.LocalPlayerEmpire.SeatOfGovernment.Sector;
                               GalaxyGrid.CenterOnSelectedSector();
                           };
            Unloaded += OnUnloaded;

            GalaxyGrid.SectorDoubleClicked += OnSectorDoubleClicked;

            PropertyChangedEventManager.AddListener(_appContext, this, "LocalPlayerEmpire");

            _revealMapCommand = new DelegateCommand<object>(ExecuteRevealMapCommand);
            _outputMapCommand = new DelegateCommand<object>(ExecuteOutputMapCommand);
            _outputMapSectorInfoCommand = new DelegateCommand<object>(ExecuteOutputMapSectorInfoCommand);
            _cheatMenuCommand = new DelegateCommand<object>(ExecuteCheatMenuCommand);
            _f12_ScreenCommand = new DelegateCommand<object>(Execute_f12_ScreenCommand);
            _f11_ScreenCommand = new DelegateCommand<object>(Execute_f11_ScreenCommand);
            _f10_ScreenCommand = new DelegateCommand<object>(Execute_f10_ScreenCommand);
            _f09_ScreenCommand = new DelegateCommand<object>(Execute_f09_ScreenCommand);
            _f08_ScreenCommand = new DelegateCommand<object>(Execute_f08_ScreenCommand);
            _f07_ScreenCommand = new DelegateCommand<object>(Execute_f07_ScreenCommand);
            _f06_ScreenCommand = new DelegateCommand<object>(Execute_f06_ScreenCommand);

            DebugCommands.RevealMap.RegisterCommand(_revealMapCommand);
            DebugCommands.OutputMap.RegisterCommand(_outputMapCommand);
            DebugCommands.OutputMapSectorInfo.RegisterCommand(_outputMapSectorInfoCommand);
            DebugCommands.CheatMenu.RegisterCommand(_cheatMenuCommand);
            DebugCommands.F12_Screen.RegisterCommand(_f12_ScreenCommand);
            DebugCommands.F11_Screen.RegisterCommand(_f11_ScreenCommand);
            DebugCommands.F10_Screen.RegisterCommand(_f10_ScreenCommand);
            DebugCommands.F09_Screen.RegisterCommand(_f09_ScreenCommand);
            DebugCommands.F08_Screen.RegisterCommand(_f08_ScreenCommand);
            DebugCommands.F07_Screen.RegisterCommand(_f07_ScreenCommand);
            DebugCommands.F06_Screen.RegisterCommand(_f06_ScreenCommand);

            _text = "Step_2102:; GalaxyGridView Initialize done...";
            if (writeDirectly)
                Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            GalaxyGrid.SectorDoubleClicked -= OnSectorDoubleClicked;
            Content = null;
            GalaxyGrid = null;
            DebugCommands.RevealMap.UnregisterCommand(_revealMapCommand);
            DebugCommands.OutputMap.UnregisterCommand(_outputMapCommand);
            DebugCommands.OutputMapSectorInfo.UnregisterCommand(_outputMapSectorInfoCommand);
            DebugCommands.CheatMenu.UnregisterCommand(_cheatMenuCommand);
            DebugCommands.F12_Screen.UnregisterCommand(_f12_ScreenCommand);
            DebugCommands.F11_Screen.UnregisterCommand(_f11_ScreenCommand);
            DebugCommands.F10_Screen.UnregisterCommand(_f10_ScreenCommand);
            DebugCommands.F09_Screen.UnregisterCommand(_f09_ScreenCommand);
            DebugCommands.F08_Screen.UnregisterCommand(_f08_ScreenCommand);
            DebugCommands.F07_Screen.UnregisterCommand(_f07_ScreenCommand);
            DebugCommands.F06_Screen.UnregisterCommand(_f06_ScreenCommand);

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
        }

        private void OnLocalPlayerEmpireChanged()
        {
            if (!_appContext.IsGameInPlay || _appContext.IsGameEnding)
            {
                return;
            }

            CivilizationManager localPlayerEmpire = _appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
            {
                return;
            }
        }

        private void OnSectorDoubleClicked(Sector sector)
        {
            if ((sector == null) || (sector.System == null))
            {
                return;
            }

            Colony colony = sector.System.Colony;
            if (colony == null)
            {
                return;
            }

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
        }

        private void ExecuteRevealMapCommand(object t)
        {
            if (!_appContext.IsSinglePlayerGame)
            {
                return;
            }

            SectorMap map = _appContext.CurrentGame.Universe.Map;
            Entities.Civilization playerCiv = _appContext.LocalPlayer.Empire;
            CivilizationMapData mapData = _appContext.LocalPlayerEmpire.MapData;

            string _text = "";

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapLocation loc = new MapLocation(x, y);
                    mapData.SetExplored(loc, true);
                    mapData.SetScanStrength(loc, 99);
                }
            }

            Diplomat diplomat = Diplomat.Get(playerCiv);

            foreach (Entities.Civilization civ in GameContext.Current.Civilizations)
            {
                if (civ == playerCiv)
                {
                    continue;
                }

                //_text_all_out = "Step_4354:; Un-Fog of War not available... " ;
                //if (writeDirectly) 
                //    Console.WriteLine(_text_all_out);


                if (diplomat.GetForeignPower(civ).DiplomacyData.Status == ForeignPowerStatus.NoContact)
                {
                    diplomat.GetForeignPower(civ).DiplomacyData.Status = ForeignPowerStatus.Neutral;
                    //diplomat.GetForeignPower(civ).DiplomacyData.ContactTurn = 999999;   // ships are not visible yet
                }
            }
            GalaxyGrid.Update();
        }

        private void ExecuteOutputMapSectorInfoCommand(object t)
        {
            string _timeString = GameEngine.GetTimeString();
            string _newline = Environment.NewLine;

            string _sector_text = "";
            string _system_text = "( no system ) ";
            string _colony_text = "( no colony ) ";
            string _orbBat_text = "";
            //string _shipnames_text = "( no ships ) ";
            //string _station_text = "( no station ) ";


            Sector _sector = GalaxyGrid.SelectedSector;
            MapLocation _loc = _sector.Location;

            if (_sector.System != null)
            {
                _system_text = " * " + _sector.System.Name + " * ( " + _sector.System.StarType + " ) ";

                if (_sector.System.HasColony)
                {
                    _colony_text = GetInfoText_Colony(_sector.System.Colony);
                    //_orbBat_text = "( no Orbital Batteries)";

                }
            }


            IEnumerable<Ship> ships = GameContext.Current.Universe.Objects.OfType<Ship>()
                .Where(s => s.Location == _loc)
                .OrderBy(s => s.OwnerID);
            //_shipnames_text = GetInfoText_Ships(ships);  // MapSectorInfo

            //IEnumerable<Station> station = GameContext.Current.Universe.Objects.OfType<Station>()
            //    .Where(s => s.Location == _loc)
            //    .OrderBy(s => s.OwnerID);
            //_station_text = GetInfoText_Station(_sector.Station);  // MapSectorInfo

            //string _dialogHeadline = "-------------------------- * Sector Info * -------------------------- " /*+ _timeString*/;
            string _dialogHeadline = "" /*+ _timeString*/;

            _sector_text = "Sector Location " + GameEngine.LocationString(_loc.ToString())
                    + " " + _system_text
                    + " ...  ( " + _timeString + " ) " + _newline /*+ _newline*/

                    + "----------------------------------------------------------------------------------------------------------------" + _newline

                //+ "Sector > " + _system_text + _newline + _newline

                /*+ "> Station: "*/ + GetInfoText_Station(_sector.Station) + _newline /*+ _newline*/

                + "> Ships: " + ships.Count() + GetInfoText_Ships(ships) + _newline /*+ _newline*/

                + "> Colony: " + _colony_text + _newline
                + _newline
                ;

            MessageDialogResult result = MessageDialog.Show(_dialogHeadline,
                ""
                //_timeString + _newline + _newline
                + "Sector Location " + GameEngine.LocationString(_loc.ToString())
                    + " " + _system_text
                    + " ...  ( " + _timeString + " ) " + _newline /*+ _newline*/

                    + "----------------------------------------------------------------------------------------------------------------" + _newline

                //+ "Sector > " + _system_text + _newline + _newline

                /*+ "> Station: "*/ + GetInfoText_Station(_sector.Station) + _newline /*+ _newline*/

                + "> Ships: " + ships.Count() + GetInfoText_Ships(ships) + _newline /*+ _newline*/

                + "> Colony: " + _colony_text + _newline + _newline
                    , MessageDialogButtons.Ok);

            _sector_text = "Step_4552:; > " + _dialogHeadline + _newline + _sector_text;

            GameContext.Output_File(".\\Resources\\Data\\Addon", "_sectorData.txt", _sector_text);

            Console.WriteLine(_sector_text);



            //if (result == MessageDialogResult.No)
            //{
            //    _ = EndGame(false);
            //}

        }

        private string GetInfoText_Ships(IEnumerable<Ship> ships)
        {
            string _shipsInfo = "";
            string _ownerText = "";
            string _newline = Environment.NewLine;

            foreach (Ship item in ships)
            {
                if (item.Owner.Key != _ownerText)
                    _shipsInfo += _newline /*+ _newline*/;
                _ownerText = item.Owner.Key;

                _shipsInfo += "" //"Step_4381:"
                                 //+ "; " + GameEngine.LocationString(item.Location.ToString())
                                 //+ "; Ship"

                        /*+ "; "*/ + item.Owner.Key
                        + " > Hull=" + item.HullStrength
                        + ", Sh=" + item.ShieldStrength
                        //+ "; Cloak=;" + item.CloakStrength
                        //+ "; Camo=;" + item.CamouflagedStrength
                        //+ "; Camo=;" + item.
                        + ", Fuel=" + item.FuelReserve
                        + " > " + item.Design.Key


                        + " > " + item.ObjectID
                        //+ " " + item.Design

                        + " " + item.Name

                        //+ "; Crew=;" + item.Crew
                        //+ "; Exp=;" + item.ExperiencePercent


                        //+ "; since Turn;" + item.TurnCreated
                        + _newline
                                                    //+ _newline
                                                    ;
                //Console.WriteLine("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                //if (writeDirectly) Console.WriteLine(_text_all_out);
                //GameLog.Core.SaveLoadDetails.DebugFormat("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
            }


            //_shipsInfo += _newline;

            return _shipsInfo;
        }

        private string GetInfoText_Station(Station item)
        {
            string _info = "( no station )";
            if (item == null)
            {
                return _info;
            }

            _info = "" //"Step_4381:"
                       //+ "; " + GameEngine.LocationString(item.Location.ToString())
                       //+ "; Ship"
                    + item.Design
                    ///*+ "; "*/ + item.Owner.Key
                    + " > " + item.ObjectID
                    + " " + item.Name
                    //+ " ( " 

                    //+ " ) "


                    //+ "; Crew=;" + item.Crew
                    //+ "; Exp=;" + item.ExperiencePercent
                    + ", Hull=" + item.HullStrength
                    + ", Sh=" + item.ShieldStrength
                    ;
            return _info;
        }

        private string GetInfoText_Colony(Colony item)
        {
            string _info = "( no colony )";
            //string _ownerText = "";
            //if (item.Owner.Key != _ownerText)
            //    _info += _newline + _newline;
            //_ownerText = item.Owner.Key;
            string _text = "";
            string _newline = Environment.NewLine;

            int pf = item.Facilities_Total1_Food
                    + item.Facilities_Total2_Industry
                    + item.Facilities_Total3_Energy
                    + item.Facilities_Total4_Research
                    + item.Facilities_Total5_Intelligence
                    ;

            string _buildings_Text = "";

            foreach (var _building in item.Buildings)
            {
                _buildings_Text += ""
                        + "Building"
                        + " since Turn;" + GameEngine.Do_x_Digit_String(3, _building.TurnCreated.ToString())
                        + ", " + (_building.IsActive ? "__Active" : "Inactive").ToString()
                        + "; " + _building.ObjectID
                        + "; " + _building.Design
                        + _newline;
            }
            if (_buildings_Text == "") _buildings_Text = "( no buildings )";


            string _orbBat_text = "no Orbital Batteries";

            if (item.OrbitalBatteries.Count > 0)
            {
                _orbBat_text = item.OrbitalBatteryDesign
                    + ": " + item.OrbitalBatteries_Active
                    + " of " + item.OrbitalBatteries_Total + " active"
                    //+ " ( " + item.OrbitalBatteryDesign + " ) "
                    ;
            }

            string _stockpile_Text = ""//"Step_7604:; " + _col
                    + "Stockpile Deu= " + item.Deuterium_Net
                    + ", Dur= " + item.Duranium_Net
                    + ", Dil= " + item.Dilithium_Net
                    + ", Credits= " + item.CreditsEmpire
                    //+ " ) "
                    //+ _newline
                    ;


            string _proj_Text = "> available Build Projects ( " + _stockpile_Text + " ) " + _newline;

            var Project_Available = TechTreeHelper
    .GetBuildProjects(item).ToList()
    //.OfType<StructureBuildProject>()
    //.Where(p =>
    //        p.GetCurrentIndustryCost() > 0
    //        && EnumHelper
    //            .GetValues<ResourceType>()
    //            .Where(availableResources.ContainsKey)
    //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
    //.OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault()
    ;
            foreach (var item2 in Project_Available)
            {
                _proj_Text += //"Step_4235:; " + colony + ";  Available to build"
                              //+ " on; " + item.Name
                              //+ "; " + item.Owner
                        ""

                        + "costs " + GameEngine.Do_x_Digit_String(5, item2.GetCurrentIndustryCost().ToString())
                        + " aka " + GameEngine.Do_x_Digit_String(2, item2.TurnsRemaining.ToString()) + " turns"
                        + " > " + item2.BuildDesign.Key
                        + " (Deu=" + item2.GetCurrentResourceCost(ResourceType.Deuterium)
                        + ", Dur=" + item2.GetCurrentResourceCost(ResourceType.Duranium)
                        + ", Dil=" + item2.GetCurrentResourceCost(ResourceType.Dilithium) + " )"
                        + _newline
                        ;
                //if (writeDirectly) Console.WriteLine(_text_all_out);

            }


            string _building_text = "";
            if (item.BuildSlots[0].HasProject)
            {
                _building_text += "" //"Step_7607:; "
                                     //+ _col
                        + "now BUILDING > " + item.BuildSlots[0].Project.BuildDesign
                        + " > needs " + item.BuildSlots[0].Project.TurnsRemaining + " turns or a BUY"

                        + _newline;
            }
            else
            {
                _building_text += "" //"Step_7602:; "
                                     //+ _col
                    + "now building > * NOTHING * or just finished this turn" + _newline;
            }


            string _buildQueue_Text = "";//"""Build Queue" + _newline;
            // not necessary
            //_text_all_out += "Step_7609:; "
            //        + _col
            //        + ";  " + item.BuildQueue.Count + " for System-BuildQueue.Count " + _newline;
            foreach (BuildQueueItem buildQueueItem in item.BuildQueue)
            {
                _buildQueue_Text += "" //"Step_7608:; "
                                       //+ _col
                    + "BUILD QUEUE  > " + buildQueueItem.Description + " > Turns needed= " + buildQueueItem.TurnsRemaining.ToString()
                    //+ " ; for; " + 

                    //+ slot.Project.Location
                    //+ " > Slot= " + slot.SlotID
                    //+ " at " + slot.Shipyard.Name
                    //+ " " + 
                    //+ " > " + _percent
                    //+ " done for " + _design
                    + _newline;
                //if (writeDirectly) Console.WriteLine(_text_all_out);
                //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
            }
            //if (writeDirectly) Console.WriteLine(_text_all_out);


            string _slots_Text = "";
            if (item.Shipyard != null)
            {
                foreach (var slot in item.Shipyard.BuildSlots)
                {
                    //try
                    //{
                    //foreach (ShipyardBuildSlot slot in shipyard)
                    //{
                    string _design = "nothing";
                    string _percent = "0 %";
                    if (slot.Project != null && slot.Project.BuildDesign != null)
                    {
                        _design = slot.Project.BuildDesign.Key.ToString();
                        _percent = slot.Project.PercentComplete.ToString();
                    }

                    if (_percent != "0 %")
                    {
                        _slots_Text += "" //"Step_7603:; " + _col //+ slot.Shipyard.Location
                            + "Shipyard-Slot= " + slot.SlotID

                            + " "
                            + " > " + _percent
                            + " done for " + _design
                            + "  at  " + slot.Shipyard.Name
                            + _newline;
                        //if (writeDirectly) Console.WriteLine(_text_all_out);
                        //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                    }
                    else
                    {
                        _slots_Text += "" // "Step_7607:; " + _col //+ slot.Shipyard.Location
                            + "Shipyard-Slot= " + slot.SlotID  // crashes with a StackOverFlow
                                                               //+ " at " + slot.Shipyard.Name
                            + " "
                            + " > " + _percent
                            + " done for " + _design
                            + _newline;
                        //if (writeDirectly) Console.WriteLine(_text_all_out);
                        //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                    }
                }
            }

            //
            // Combine full text
            //

            _info = "" //"Step_4381:"
                       //+ "; " + GameEngine.LocationString(item.Location.ToString())
                       //+ "; Ship"


        + " " + item.ObjectID
        + " " + item.Name

        + " - " + item.Owner.Key/* + " ) "*/
            //+ ", Defense= " + item.System.Colony.de
            //+ "; " + GameEngine.LocationString(item.Location.ToString())
            //+ "; " + item.ObjectID
            //+ ";Colony"
            //+ "; " + item.Name
            //+ "; " + item.Owner
            + _newline
            /*+ ", "*/ + "Facilities total= " + pf
            + ", Population > " + item.Population
            + " of max " + item.Population_Max
            + ", Morale= " + item.Morale
            + ", Credits = " + item.CreditsEmpire

            + _newline

            //+ ";fac      Food > " 
            + item.Facilities_Active1_Food + " of " + item.Facilities_Total1_Food + " Food"
            + " Food" + ", Food Reserves= " + item.FoodReserves + _newline
            + item.Facilities_Active2_Industry + " of " + item.Facilities_Total2_Industry + " Industry" + _newline
            + item.Facilities_Active3_Energy + " of " + item.Facilities_Total3_Energy + " Energy ( " + _orbBat_text + " ) " + _newline
            + item.Facilities_Active4_Research + " of " + item.Facilities_Total4_Research + " Research" + _newline
            + item.Facilities_Active5_Intelligence + " of " + item.Facilities_Total5_Intelligence + " Intelligence" + _newline
            + "----" + _newline
//+ _newline

+ _building_text //+ _newline 
+ _buildQueue_Text //+ _newline 
+ _slots_Text //+ _newline 
+ _proj_Text + _newline
            + _buildings_Text //+ _newline

            //+ _newline
            ;

            //Console.WriteLine("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
            //if (writeDirectly) Console.WriteLine(_text_all_out);
            //GameLog.Core.SaveLoadDetails.DebugFormat("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
            //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
            //}


            //_info += _newline;

            return _info;
        }

        private void ExecuteOutputMapCommand(object t)
        {


            //if (!_appContext.IsSinglePlayerGame)
            //{
            //    return;
            //}
            //string _path_Lib = ResourceManager.GetResourcePath(".\\lib");

            System.Media.SoundPlayer _wav_player = new System.Media.SoundPlayer("Resources/SoundFX/sound002.wav");
            _wav_player.Play();

            string _path_Resources_Data_Addon = ResourceManager.GetResourcePath(".\\Resources\\Data\\Addon");
            string _timeString = GameEngine.GetTimeString();

            string _shipnames_text = "";
            string _station_names_text = "";

            string _text_all_out = "";
            string _text = "";
            string _file = "";
            string _newline = Environment.NewLine;
            bool writeDirectly = true;
            bool bool_output = true;

            //Report_DiplomacyData();
            Supremacy.Game.GameContext.Report_DiplomacyData();


            // is doubled
            //foreach (var _civ1 in GameContext.Current.Civilizations)
            //{
            //    foreach (var _civ2 in GameContext.Current.Civilizations)
            //    {
            //        DiplomacyHelper.ShouldTheyGoToWar(_civ1, _civ2);
            //        _text = "this reports some data";
            //    }
            //} 

            // MapData

            SectorMap _map = _appContext.CurrentGame.Universe.Map;

            _text_all_out = _timeString + "_Turn_" + _appContext.CurrentGame.TurnNumber + ".txt" + _newline;
            _text_all_out += "** Example:  MAP Location (2,5) = line 5, column 2 ** use CTRL+F for searching... ** ...before half width ('|') add some few minus **   "
                + _newline
                + _newline
                + "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + _newline
                + _newline
                ;
            int yhalf = _map.Height / 2;
            int xhalf = _map.Width / 2;

            for (int y = 0; y < _map.Height; y++)
            {
                if (y < 10) _text_all_out += " ";  // 1 to 9 getting a blank before

                if (y == yhalf) _text_all_out += "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + _newline;
                _text_all_out += y + ":  ";
                for (int x = 0; x < _map.Width; x++)
                {
                    if (x == xhalf) _text_all_out += "| ";
                    string owner = ".";
                    if (_map[x, y].Owner != null)
                    {
                        owner = _map[x, y].Owner.CivID.ToString();
                        if (_map[x, y].Owner.CivID > 6) owner = "M"; // Minor
                    }

                    string type = ".";
                    if (_map[x, y].System != null)
                    {
                        type = _map[x, y].System.StarType.ToString().Substring(0, 1);
                        if (_map[x, y].System.StarType == StarType.BlackHole) type = "b";
                        if (_map[x, y].System.StarType == StarType.NeutronStar) type = "n";
                        //if (_map[x, y].System.StarType == StarType.Quasar) type = "Q";
                        if (_map[x, y].System.StarType == StarType.RadioPulsar) type = "r";
                        if (_map[x, y].System.StarType == StarType.XRayPulsar) type = "x";
                        if (_map[x, y].System.StarType == StarType.Wormhole) type = "w";
                    }
                    _text_all_out += owner + type + " ";
                    //if (writeDirectly) Console.WriteLine(_text_all_out);
                }
                _text_all_out += _newline;
                //if (writeDirectly) Console.WriteLine(_text_all_out);
            }

            _text_all_out +=
                _newline + "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + _newline
                + _newline
                + "1st character:                                     2nd character: StarSystem" + _newline
                + "   0 = Federation                                     B = Blue star" + _newline//" + _newline
                + "   1 = Terrans                                        O = Orange star" + _newline//" + _newline
                + "   2 = Romulans                                       N = Nebula" + _newline//" + _newline
                + "   3 = Klingons                                       R = Red star" + _newline//" + _newline
                + "   4 = Cardassian                                     Y = Yellow star" + _newline//" + _newline
                + "   5 = Dominion                                       W = White star" + _newline//" + _newline
                + "   6 = Borg                                           B = Blue star" + _newline//" + _newline
                + _newline /*+ _newline*/
                + "   M = Minor                                          b = black hole" + _newline
                + "                                                      n = Neutron star" + _newline
                + "                                                      Q = Quasar" + _newline
                + "                                                      r = Radio Pulsar" + _newline
                + "                                                      w = worm hole" + _newline
                + "                                                      x = x-ray Pulsar" + _newline
                + _newline
                ;


            if (GameContext.Current != null)
            {

                foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers)
                {
                    string _x_text = _civM.HomeColony.Location.X.ToString();
                    string _y_text = _civM.HomeColony.Location.Y.ToString();
                    _text_all_out += _civM.Civilization.HomeQuadrant + "-Quadrant"
                        + " ; " + GameEngine.Do_x_String(19, _civM.Civilization.Key).ToString()
                        + " ; " + GameEngine.Do_x_Digit_String(2, _x_text)
                        + " ; " + GameEngine.Do_x_Digit_String(2, _y_text)

                        + " ; " + _civM.Civilization.HomeSystemName
                        + " ; " + _civM.Civilization
                        + _newline;
                    _text_all_out = _text_all_out.Replace("Beta-Quadrant", "Beta -Quadrant");
                }

                _text_all_out += _newline;

                IEnumerable<StarSystem> otherSystems = GameContext.Current.Universe.Objects.OfType<StarSystem>();
                foreach (StarSystem _sec in otherSystems)
                {
                    string _owner;
                    if (_sec.Owner == null)
                    {
                        _owner = "---- No Owner -----";
                    }
                    else
                    {
                        _owner = GameEngine.Do_x_String(19, _sec.Owner.Key);
                    }

                    string _x_text = GameEngine.Do_x_Digit_String(2, _sec.Location.X.ToString());
                    string _y_text = GameEngine.Do_x_Digit_String(2, _sec.Location.Y.ToString());
                    _text_all_out += "--------------"//Quadrant"
                        + " ; " + _owner
                        + " ; " + _x_text
                        + " ; " + _y_text
                        + " ; " + _sec.Name
                        + " ; " + _sec.StarType

                        + _newline;
                    //_text_all_out = _text_all_out.Replace("Beta-Quadrant", "Beta -Quadrant");


                }

                //DiplomacyHelper.rep

                IEnumerable<Colony> colonies = GameContext.Current.Universe.Objects.OfType<Colony>();
                foreach (Colony item in colonies)
                {
                    String _col =
                        /*";Colony;" */
                        /*"; " + */GameEngine.LocationString(item.Location.ToString())
                        + "; " + item.ObjectID + ";Colony"
                        + ";" + item.Name
                        + ";" + item.Owner

                        ;

                    int pf = item.Facilities_Total1_Food
                            + item.Facilities_Total2_Industry
                            + item.Facilities_Total3_Energy
                            + item.Facilities_Total4_Research
                            + item.Facilities_Total5_Intelligence

                        ;

                    _text_all_out += _newline + _timeString + _newline
                        + "Step_4363:"
                        + "; " + GameEngine.LocationString(item.Location.ToString())
                        + "; " + item.ObjectID
                        + ";Colony"
                        + "; " + item.Name
                        + "; " + item.Owner
                        + ";pop;" + item.Population
                        + ";max; " + item.Population_Max
                        + ";Fac; " + pf


                        + ";mor;" + item.Morale
                        + ";FoodR;" + item.FoodReserves
                        + ";facF;" + item.Facilities_Active1_Food + ";of; " + item.Facilities_Total1_Food
                        + "; facI;" + item.Facilities_Active2_Industry + ";of; " + item.Facilities_Total2_Industry
                        + "; facE;" + item.Facilities_Active3_Energy + ";of; " + item.Facilities_Total3_Energy
                        + "; facR;" + item.Facilities_Active4_Research + ";of; " + item.Facilities_Total4_Research
                        + "; facI;" + item.Facilities_Active5_Intelligence + ";of; " + item.Facilities_Total5_Intelligence


                                                    //+ ";since Turn;" + item.TurnCreated

                                                    + _newline;
                    //if (writeDirectly) Console.WriteLine(_text_all_out);
                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);

                    ILookup<MapLocation, StarSystem> systemLocationLookup = GameContext.Current.Universe.Objects.OfType<StarSystem>().ToLookup(o => o.Location);

                    //_text_all_out += _newline;
                    //ILookup<MapLocation, GameObject> gameObjectLocationLookup = GameContext.Current.Universe.Objects.OfType<GameObject>().ToLookup(o => o.GetType() != Type.EmptyTypes);
                    //int pf = item.Facilities_Total1_Food
                    //        + item.Facilities_Total2_Industry
                    //        + item.Facilities_Total3_Energy
                    //        + item.Facilities_Total4_Research
                    //        + item.Facilities_Total5_Intelligence

                    //    ;
                    //foreach (var item2 in pf)
                    //{
                    //_text_all_out += "Step_4366:; "
                    //    + _col
                    //    //+ "; " + pf.IsActive + "_for_Active"
                    //    //+ "; Building"
                    //    //+ "; " + pf.ObjectID
                    //    + "; TotalFacilities=; " + pf

                    //    //+ "; since Turn;" + pf.TurnCreated

                    //    + _newline;

                    //if (writeDirectly) Console.WriteLine(_text_all_out);

                    //}

                    //for (int i = 0;i < item.TotalFacilities.Count(); i++)
                    //{

                    //}

                    //_text_all_out += _newline;
                    ILookup<MapLocation, Building> buildingLocationLookup = GameContext.Current.Universe.Objects.OfType<Building>().ToLookup(o => o.Location);
                    foreach (Building building in buildingLocationLookup[item.Location])
                    {
                        _text_all_out += "Step_4367:; "
                            + _col
                            + "; " + building.IsActive + "_for_Active"
                            + "; since Turn;" + GameEngine.Do_x_Digit_String(3, building.TurnCreated.ToString())
                            + "; Building"
                            + "; " + building.ObjectID
                            + "; " + building.Design



                            + _newline;

                        //if (writeDirectly) Console.WriteLine(_text_all_out);

                    }

                    // just for info
                    var Project_Available = TechTreeHelper
                        .GetBuildProjects(item).ToList()
                        //.OfType<StructureBuildProject>()
                        //.Where(p =>
                        //        p.GetCurrentIndustryCost() > 0
                        //        && EnumHelper
                        //            .GetValues<ResourceType>()
                        //            .Where(availableResources.ContainsKey)
                        //            .All(r => availableResources[r] >= p.GetCurrentResourceCost(r)))
                        //.OrderBy(p => p.BuildDesign.BuildCost).FirstOrDefault()
                        ;
                    foreach (var item2 in Project_Available)
                    {
                        _text_all_out += "Step_4235:; " + _col + ";  Available to build"
                                //+ " on; " + item.Name
                                //+ "; " + item.Owner

                                + "; needs " + GameEngine.Do_x_Digit_String(2, item2.TurnsRemaining.ToString()) + " turns"
                                + "; costs= " + GameEngine.Do_x_Digit_String(5, item2.GetCurrentIndustryCost().ToString())
                                + "; " + item2.BuildDesign.Key
                                + "; and Deu=" + item2.GetCurrentResourceCost(ResourceType.Deuterium)
                                + "; Dur=" + item2.GetCurrentResourceCost(ResourceType.Duranium)
                                + "; Dil=" + item2.GetCurrentResourceCost(ResourceType.Dilithium)
                                + _newline
                                ;
                        //if (writeDirectly) Console.WriteLine(_text_all_out);

                    }

                    _text_all_out += "Step_7604:; " + _col

                            + "; Stockpile Deu= " + item.Deuterium_Net
                            + "; Dur= " + item.Duranium_Net
                            + "; Dil= " + item.Dilithium_Net
                            + "; Credits= " + item.CreditsEmpire
                            + _newline;

                    _text_all_out += "Step_4369:"
                            + "; " + GameEngine.LocationString(item.Location.ToString())
                            + "; " + item.ObjectID
                            + ";Colony"
                            + "; " + item.Name
                            + "; " + item.Owner
                            + ";pop;" + item.Population
                            + ";max; " + item.Population_Max
                            + ";Fac; " + pf

                            + ";mor;" + item.Morale
                            + ";FoodR;" + item.FoodReserves
                            + ";facF;" + item.Facilities_Active1_Food + ";of; " + item.Facilities_Total1_Food
                            + "; facI;" + item.Facilities_Active2_Industry + ";of; " + item.Facilities_Total2_Industry
                            + "; facE;" + item.Facilities_Active3_Energy + ";of; " + item.Facilities_Total3_Energy
                            + "; facR;" + item.Facilities_Active4_Research + ";of; " + item.Facilities_Total4_Research
                            + "; facI;" + item.Facilities_Active5_Intelligence + ";of; " + item.Facilities_Total5_Intelligence

                            //+ ";since Turn;" + item.TurnCreated
                            + _newline;

                    if (item.BuildSlots[0].HasProject)
                    {
                        _text_all_out += "Step_7607:; "
                                + _col
                                + "; IS BUILDING > " + item.BuildSlots[0].Project.BuildDesign
                                + " > needs " + item.BuildSlots[0].Project.TurnsRemaining + " turns or a BUY"

                                + _newline;
                    }
                    else
                    {
                        _text_all_out += "Step_7602:; "
                            + _col
                            + "; is _building > * NOTHING * or just finished this turn" + _newline;
                    }

                    // not necessary
                    //_text_all_out += "Step_7609:; "
                    //        + _col
                    //        + ";  " + item.BuildQueue.Count + " for System-BuildQueue.Count " + _newline;
                    foreach (BuildQueueItem buildQueueItem in item.BuildQueue)
                    {
                        _text_all_out += "Step_7608:; "
                            + _col
                            + "; BUILDQUEUE  > " + buildQueueItem.Description + " > Turns needed= " + buildQueueItem.TurnsRemaining.ToString()
                            //+ " ; for; " + 

                            //+ slot.Project.Location
                            //+ " > Slot= " + slot.SlotID
                            //+ " at " + slot.Shipyard.Name
                            //+ " " + 
                            //+ " > " + _percent
                            //+ " done for " + _design
                            + _newline;
                        //if (writeDirectly) Console.WriteLine(_text_all_out);
                        //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                    }
                    //if (writeDirectly) Console.WriteLine(_text_all_out);

                    if (item.Shipyard != null)
                    {
                        foreach (var slot in item.Shipyard.BuildSlots)
                        {
                            try
                            {
                                //foreach (ShipyardBuildSlot slot in shipyard)
                                //{
                                string _design = "nothing";
                                string _percent = "0 %";
                                if (slot.Project != null && slot.Project.BuildDesign != null)
                                {
                                    _design = slot.Project.BuildDesign.ToString();
                                    _percent = slot.Project.PercentComplete.ToString();
                                }

                                if (_percent != "0 %")
                                {
                                    _text_all_out += "Step_7603:; " + _col //+ slot.Shipyard.Location
                                        + "; Slot= " + slot.SlotID

                                        + " "
                                        + " > " + _percent
                                        + " done for " + _design
                                        + " at " + slot.Shipyard.Name
                                        + _newline;
                                    //if (writeDirectly) Console.WriteLine(_text_all_out);
                                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                                }
                                else
                                {
                                    _text_all_out += "Step_7607:; " + _col //+ slot.Shipyard.Location
                                        + "; Slot= " + slot.SlotID  // crashes with a StackOverFlow
                                                                    //+ " at " + slot.Shipyard.Name
                                        + " "
                                        + " > " + _percent
                                        + " done for " + _design
                                        + _newline;
                                    //if (writeDirectly) Console.WriteLine(_text_all_out);
                                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                                }
                            }
                            catch
                            {
                                _text_all_out += "Step_7609: Serialize failed"
                                     //+ slot.Project.Location
                                     //+ " > Slot= " + slot.SlotID
                                     //+ " at " + slot.Shipyard.Name
                                     //+ " " + 
                                     //+ " > " + _percent
                                     //+ " done for " + _design
                                     + _newline;
                                //if (writeDirectly) Console.WriteLine(_text_all_out);
                                //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                            }
                            ;
                        }
                        //foreach (BuildQueueItem buildQueueItem in colony.BuildQueue)
                    }
                    //foreach (BuildQueueItem buildQueueItem in colony.BuildQueue)
                    //{

                    //}


                }


                if (writeDirectly)
                    Console.WriteLine(_text_all_out);
                _text_all_out += _newline;



                IEnumerable<Ship> ships = GameContext.Current.Universe.Objects.OfType<Ship>().OrderBy(s => s.OwnerID);

                string _shipsInfo_text = GetInfoText_Ships(ships);  // Alt+M = all MapData

                //foreach (Ship item in ships)
                //{
                //    if (item.Owner.Key != _ownerText)
                //        _text_all_out += _newline + _newline;
                //    _ownerText = item.Owner.Key;

                //    _text_all_out += "Step_4381:"
                //            + "; " + GameEngine.LocationString(item.Location.ToString())
                //            + "; Ship"

                //            + "; " + item.Owner.Key
                //            + "; " + item.ObjectID
                //            + "; " + item.Design
                //            + "; " + item.Name

                //            + "; Crew=;" + item.Crew
                //            + "; Exp=;" + item.ExperiencePercent
                //            + "; Hull=;" + item.HullStrength
                //            + "; Sh=;" + item.ShieldStrength
                //            + "; Cloak=;" + item.CloakStrength
                //            + "; Camo=;" + item.CamouflagedStrength
                //            //+ "; Camo=;" + item.
                //            + "; Fuel=;" + item.FuelReserve

                //            + "; since Turn;" + item.TurnCreated

                //                                        + _newline;
                //    //Console.WriteLine("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                //    //if (writeDirectly) Console.WriteLine(_text_all_out);
                //    //GameLog.Core.SaveLoadDetails.DebugFormat("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                //    //GameLog.Core.SaveLoadDetails.DebugFormat(_text_all_out);
                //}


                _text_all_out += _timeString + _newline;

                IEnumerable<Station> stations = GameContext.Current.Universe.Objects.OfType<Station>();
                foreach (Station item in stations)
                {
                    _text_all_out += "Step_4392:"
                            + "; " + GameEngine.LocationString(item.Location.ToString())
                            + "; Station"

                            + "; " + item.Owner
                            + "; " + item.ObjectID
                            + "; " + item.Design
                            + "; " + item.Name

                            + "; Crew=;" + item.Crew
                            + "; Exp=;" + item.ExperiencePercent
                            + "; Hull=;" + item.HullStrength
                            + "; Sh=;" + item.ShieldStrength
                            //+ "; Cloak=;" + item.CloakStrength
                            //+ "; Camo=;" + item.CamouflagedStrength
                            //+ "; Camo=;" + item.
                            //+ "; Fuel=;" + item.FuelReserve

                            + "; since Turn;" + item.TurnCreated

                                                        + _newline;
                }

                _text_all_out += _timeString + _newline;

                IEnumerable<Shipyard> shipyards = GameContext.Current.Universe.Objects.OfType<Shipyard>();
                foreach (Shipyard item in shipyards)
                {
                    _text_all_out += "Step_4391:"
                            + "; " + GameEngine.LocationString(item.Location.ToString())
                            + "; Shipyard"

                            + "; " + item.Owner
                            + "; " + item.ObjectID
                            + "; " + item.Design
                            + "; " + item.Name

                            //+ "; Crew=;" + item.
                            //+ "; Exp=;" + item.ExperiencePercent
                            //+ "; Hull=;" + item.HullStrength
                            //+ "; Sh=;" + item.ShieldStrength
                            //+ "; Cloak=;" + item.CloakStrength
                            //+ "; Camo=;" + item.CamouflagedStrength
                            //+ "; Camo=;" + item.
                            //+ "; Fuel=;" + item.FuelReserve

                            + "; since Turn;" + item.TurnCreated

                                                        + _newline;
                }

                _text_all_out += _timeString + _newline;

                var races = GameContext.Current.Races.ToList();
                foreach (var item in races)
                {
                    _text_all_out += "Step_4396:"
                            //+ "; " + item.
                            + "; Race"

                            //+ "; " + item.Owner
                            //+ "; " + item.ObjectID
                            //+ "; " + item.Design
                            + "; " + item.Key

                                                        + "; HomePlanet=;" + item.HomePlanetType
                                                        + "; Eff=;" + item.GroundCombatEffectiveness
                                                        //+ "; Hull=;" + item.HullStrength
                                                        //+ "; Sh=;" + item.ShieldStrength
                                                        //+ "; Cloak=;" + item.CloakStrength
                                                        //+ "; Camo=;" + item.CamouflagedStrength
                                                        //+ "; Camo=;" + item.
                                                        //+ "; Fuel=;" + item.FuelReserve

                                                        //+ "; since Turn;" + item.TurnCreated

                                                        + _newline;
                }

                //var events = null;
                if (GameContext.Current.ScriptedEvents != null)
                {
                    _text_all_out += "Step_4356:; Events following..." + _newline;
                    var events = GameContext.Current.ScriptedEvents.ToList();
                    foreach (var item in events)
                    {
                        _text_all_out += "Step_4356:"
                                //+ "; " + item.
                                + "; Events"

                                //+ "; " + item.Owner
                                //+ "; " + item.ObjectID
                                //+ "; " + item.Design
                                + "; " + item.EventID

                                                            + "; Last=;" + item.LastExecution
                                                            //+ "; Eff=;" + item.GroundCombatEffectiveness
                                                            //+ "; Hull=;" + item.HullStrength
                                                            //+ "; Sh=;" + item.ShieldStrength
                                                            //+ "; Cloak=;" + item.CloakStrength
                                                            //+ "; Camo=;" + item.CamouflagedStrength
                                                            //+ "; Camo=;" + item.
                                                            //+ "; Fuel=;" + item.FuelReserve

                                                            //+ "; since Turn;" + item.TurnCreated

                                                            + _newline;
                    }
                }

                _text_all_out += _timeString + _newline;

                var civs = GameContext.Current.Civilizations.ToList();
                foreach (Civilization item in civs)
                {
                    _text_all_out += "Step_4346:"
                            //+ "; " + ClientApp.Current.LocationString(item.Location.ToString())
                            + "; Civ"
                            + "; " + item.Name
                            + "; " + item.CivilizationType
                            + "; " + item.CivID
                            //+ "; " + item.Design


                            + "; Race=;" + item.Race.Name
                            + "; " + item.HomeQuadrant
                            + "; " + item.HomeSystemName
                            + "; " + item.Color
                            + "; " + item.Traits
                                                        + "; Mor=;" + item.MoraleDriftRate
                                                        //+ "; Camo=;" + item.
                                                        //+ "; Fuel=;" + item.FuelReserve

                                                        //+ "; since Turn;" + item.TurnCreated

                                                        + _newline;
                }

                _text_all_out += _timeString + _newline;

                var civMans = GameContext.Current.CivilizationManagers.ToList();
                foreach (CivilizationManager item in civMans)
                {
                    _text_all_out += "Step_4348:"
                            //+ "; " + ClientApp.Current.LocationString(item.Location.ToString())
                            + "; CivMan"
                            + "; " + item.Civilization
                            + "; ID=" + item.CivilizationID


                                                        + _newline;
                }

                _text_all_out += _newline;

                //var civMans = GameContext.Current.ScriptedEvents;
                if (GameContext.Current.ScriptedEvents != null)
                {
                    foreach (var item in GameContext.Current.ScriptedEvents)
                    {
                        _text_all_out += "Step_4349:"
                                //+ "; " + ClientApp.Current.LocationString(item.Location.ToString())
                                + "; CivMan"
                                + "; " + item.EventID
                                                            //+ "; " + item.Civilization

                                                            + _newline;
                    }
                }

                _text_all_out += _newline;

                //IEnumerable<ShipDesign> bd = GameContext.Current.TechDatabase.Select(i => GameContext.Current.TechDatabase[i] as ShipDesign);
                //if (GameContext.Current.TechDatabase)
                //{
                //    foreach (var item in GameContext.Current.TechDatabase)
                //{
                bool _first_line_ship_names = true;
                bool first_stationname = true;
                bool first_orbbat = true;
                bool first_pf = true;
                bool first_buildings = true;
                bool first_shipyards = true;
                bool first_stations = true;
                bool first_ships = true;




                foreach (var item in GameContext.Current.TechDatabase) //.Where(i => i. GameContext.Current.TechDatabase[i] as ShipDesign))

                {
                    string tdb_text = "Step_4347:";
                    //string _shipnames_text = "";

                    tdb_text += "; " + item.DesignID;
                    tdb_text += "; " + item.Key;

                    tdb_text += " ;" + item.TechRequirements[Tech.TechCategory.BioTech];
                    tdb_text += ";" + item.TechRequirements[Tech.TechCategory.Computers];
                    tdb_text += ";" + item.TechRequirements[Tech.TechCategory.Construction];
                    tdb_text += ";" + item.TechRequirements[Tech.TechCategory.Energy];
                    tdb_text += ";" + item.TechRequirements[Tech.TechCategory.Propulsion];
                    tdb_text += ";" + item.TechRequirements[Tech.TechCategory.Weapons];
                    tdb_text += ";" + item.IsUniversallyAvailable;

                    tdb_text += ";" + item.BuildCost;
                    tdb_text += ";" + item.Duranium;
                    tdb_text += ";" + item.MaintenanceCost;
                    tdb_text += ";" + item.PopulationHealth;
                    tdb_text += ";" + item.EncyclopediaCategory;
                    tdb_text += ";" + item.ObsoletedDesigns.Count;
                    tdb_text += ";" + item.UpgradableDesigns.Count;
                    ;



                    //Batteries
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Batteries)
                    {

                        if (first_orbbat)
                        {
                            _text_all_out += "Step_4341: ---------------" + _newline;
                            _text_all_out += "Step_4341:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Sc%;SP;SR;HULL;SH;ShR;W1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + _newline;
                            first_orbbat = false;
                        }

                        OrbitalBatteryDesign spec = item as OrbitalBatteryDesign;
                        tdb_text += ";" + spec.ScienceAbility;
                        tdb_text += ";" + spec.ScanStrength;
                        tdb_text += ";" + spec.SensorRange;
                        tdb_text += ";" + spec.HullStrength;
                        tdb_text += ";" + spec.ShieldStrength;
                        tdb_text += ";" + spec.ShieldRechargeRate;

                        if (spec.PrimaryWeapon != null)
                        {
                            tdb_text += ";" + spec.PrimaryWeaponName;
                            tdb_text += ";" + spec.PrimaryWeapon.Count;
                            tdb_text += ";" + spec.PrimaryWeapon.Damage;
                            tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        }

                        if (spec.SecondaryWeapon != null)
                        {
                            tdb_text += ";" + spec.SecondaryWeaponName;
                            tdb_text += ";" + spec.SecondaryWeapon.Count;
                            tdb_text += ";" + spec.SecondaryWeapon.Damage;
                            //tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        }

                        //tdb_text += ";" + item.s;
                    }

                    //ProductionFacilityDesign
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Facilites)
                    {

                        if (first_pf)
                        {
                            _text_all_out += "Step_4342: ---------------" + _timeString + _newline;
                            _text_all_out += "Step_4342:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;LA;FIELD;OUTP;COMMENT" + _newline;
                            first_pf = false;
                        }

                        ProductionFacilityDesign spec = item as ProductionFacilityDesign;
                        //tdb_text += ";" + spec.Duranium;
                        tdb_text += ";" + spec.LaborCost;
                        tdb_text += ";" + spec.Category;
                        tdb_text += ";" + spec.UnitOutput;
                        //tdb_text += ";" + spec.HullStrength;
                        //tdb_text += ";" + spec.ShieldStrength;
                        //tdb_text += ";" + spec.ShieldRechargeRate;

                        //if (spec.Bonuses != null)
                        //{
                        //    foreach (var bonus in spec.Bonuses)
                        //    {
                        //        tdb_text += ";" + bonus.BonusType;
                        //        tdb_text += ";" + bonus.Amount;
                        //        //tdb_text += ";" + spec.PrimaryWeapon.Damage;
                        //        //tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        //    }


                        //}

                        ////var re = spec.Restriction.get
                        //if (spec.Restriction != null)
                        //{
                        //    //foreach (var re in spec.Restriction.)
                        //    //{
                        //    tdb_text += ";Restrictions";
                        //    //tdb_text += ";" + re.Amount;
                        //    //tdb_text += ";" + spec.PrimaryWeapon.Damage;
                        //    //tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        //    //}
                        //}


                    }

                    //Buildings.BuildingDesign
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Buildings)
                    {

                        if (first_buildings)
                        {
                            tdb_text += "Step_4343: ---------------" + _timeString + _newline;
                            tdb_text += "Step_4343:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;EN;R1;R2;R3;R4;Bo1;B1V;Bo2;B2V;COMMENT" + _newline;
                            first_buildings = false;
                        }

                        Buildings.BuildingDesign spec = item as Buildings.BuildingDesign;
                        tdb_text += ";" + spec.EnergyCost;



                        //Restriction
                        string _restriction_text = ";";

                        if (spec.Restriction.ToString() != null)
                        {


                            if (spec.Restriction.ToString().Contains("HomeSystem")) _restriction_text += "HoS;"; else _restriction_text += ";";
                            if (spec.Restriction.ToString().Contains("OnePerEmpire")) _restriction_text += "OneE;"; else _restriction_text += ";";
                            if (spec.Restriction.ToString().Contains("OnePerSystem")) _restriction_text += "OneS;"; else _restriction_text += ";";
                            //if (spec.Restriction.ToString() == "HomeSystem") _restriction_text += "HoS;"; else _restriction_text += ";" ;
                            //if (spec.Restriction.ToString() == "HomeSystem") _restriction_text += "HoS;"; else _restriction_text += ";" ;
                            //if (spec.Restriction.ToString() == "HomeSystem") _restriction_text += "HoS;"; else _restriction_text += ";" ;
                        }
                        tdb_text += _restriction_text;

                        if (spec.Bonuses != null)
                        {
                            foreach (var bonus in spec.Bonuses)
                            {
                                tdb_text += ";" + bonus.BonusType;
                                tdb_text += ";" + bonus.Amount;
                            }
                        }
                    }

                    //ShipyardDesign
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Shipyards)
                    {

                        if (first_shipyards)
                        {
                            _text_all_out += "Step_4344: ---------------" + _timeString + _newline;
                            _text_all_out += "Step_4344:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;EN;SL;OUTP;OUTPm;TYPE;maxLvl;COMMENT" + _newline;
                            first_shipyards = false;
                        }

                        ShipyardDesign spec = item as ShipyardDesign;
                        tdb_text += ";" + spec.BuildSlotEnergyCost;
                        tdb_text += ";" + spec.BuildSlots;
                        tdb_text += ";" + spec.BuildSlotOutput;
                        tdb_text += ";" + spec.BuildSlotMaxOutput;
                        tdb_text += ";" + spec.BuildSlotOutputType;

                        tdb_text += ";" + spec.MaxBuildTechLevel;

                    }

                    //StationDesign
                    string _text_stations = "Step_4345: _stations_text >  (or no output)";

                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Stations)
                    {
                        if (first_stations)
                        {
                            _text_stations += "Step_4345: ---------------" + _timeString + _newline;
                            _text_stations += "Step_4345:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Sc%;SP;SR;HULL;SH;ShR;W1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + _newline;
                            first_stations = false;
                        }

                        StationDesign spec = item as StationDesign;
                        _text_stations += ";" + spec.ScienceAbility;
                        _text_stations += ";" + spec.ScanStrength;
                        _text_stations += ";" + spec.SensorRange;
                        _text_stations += ";" + spec.HullStrength;
                        _text_stations += ";" + spec.ShieldStrength;
                        _text_stations += ";" + spec.ShieldRechargeRate;

                        if (spec.PrimaryWeapon != null)
                        {
                            _text_stations += ";" + spec.PrimaryWeaponName;
                            _text_stations += ";" + spec.PrimaryWeapon.Count;
                            _text_stations += ";" + spec.PrimaryWeapon.Damage;
                            _text_stations += ";" + spec.PrimaryWeapon.Refire;
                        }

                        if (spec.SecondaryWeapon != null)
                        {
                            _text_stations += ";" + spec.SecondaryWeaponName;
                            _text_stations += ";" + spec.SecondaryWeapon.Count;
                            _text_stations += ";" + spec.SecondaryWeapon.Damage;
                        }



                        //StationNames
                        _station_names_text += "Step_4359: no output for  _station_names_text";
                        //if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Stations)
                        //{

                        //    if (first_stationname)
                        //    {
                        //        _station_names_text += "Step_4349: ---------------" + _timeString + _newline;
                        //        _station_names_text += DateTime.Now + ";COUNT;KEY;NAMES;COMMENT" + _newline;
                        //        first_stationname = false;
                        //    }

                        //    StationDesign spec2 = item as StationDesign;

                        //    tdb_text += ";" + spec.Dilithium;
                        //    tdb_text += ";" + spec.Speed;
                        //    tdb_text += ";" + spec.Range;
                        //    tdb_text += ";" + spec.FuelCapacity;
                        //    tdb_text += ";" + spec.Maneuverability;
                        //    tdb_text += ";" + spec.WorkCapacity;

                        //    tdb_text += ";" + spec.ScienceAbility;
                        //    tdb_text += ";" + spec.ScanStrength;
                        //    tdb_text += ";" + spec.SensorRange;
                        //    tdb_text += ";" + spec.HullStrength;
                        //    tdb_text += ";" + spec.ShieldStrength;
                        //    tdb_text += ";" + spec.ShieldRechargeRate;

                        //    tdb_text += ";" + spec.CloakStrength;
                        //    tdb_text += ";" + spec.CamouflagedStrength;

                        //    tdb_text += ";" + spec.StationType;
                        //    tdb_text += ";" + spec.ClassName;

                        //    int count = spec2.PossibleNames.Count;

                        //    foreach (var name in spec2.PossibleNames)
                        //    {
                        //        _station_names_text += "Step_4359:"
                        //            + ";" + count
                        //            + ";" + item.Key

                        //            + " ;" + name.Key
                        //            + _newline;
                        //    }

                        //ToDo: test it on this place of code
                        //_file = Path.Combine(_path_Resources_Data_Addon, "_StationNames.csv"); // by ALT+M at GalaxyMap
                        //if (!string.IsNullOrEmpty(_file))
                        //{
                        //    StreamWriter streamWriter = new StreamWriter(_file);
                        //    streamWriter.Write(_station_names_text);
                        //    streamWriter.Close();
                        //    _text_stations = "output of _StationNames done to " + _file;
                        //    if (writeDirectly)
                        //        Console.WriteLine(_text_stations);
                        //}

                        //}

                    }
                    tdb_text += _text_stations;
                    bool_output = true; // true again


                    //ShipDesign
                    string _text_ships = "### output for > ships  (or no output)";
                    bool_output = true;
                    //bool_output = false;
                    if (bool_output == true && item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Ships)
                    {

                        if (first_ships)
                        {
                            _text_ships += "Step_4346: ---------------" + _timeString + _newline;
                            _text_ships += "Step_4346:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Dil;Spe;Ra;Fu;Man;WO;Sc%;SP;SR;"
                                                            + "HULL;SH;ShR;Cl;Ca;TYPE;CLASSNAME;N_+_Sp;W1=Weapon 1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + _newline;
                            first_ships = false;
                        }

                        ShipDesign spec = item as ShipDesign;

                        _text_ships += ";" + spec.Dilithium;
                        _text_ships += ";" + spec.Speed;
                        _text_ships += ";" + spec.Range;
                        _text_ships += ";" + spec.FuelCapacity;
                        _text_ships += ";" + spec.Maneuverability;
                        _text_ships += ";" + spec.WorkCapacity;

                        _text_ships += ";" + spec.ScienceAbility;
                        _text_ships += ";" + spec.ScanStrength;
                        _text_ships += ";" + spec.SensorRange;
                        _text_ships += ";" + spec.HullStrength;
                        _text_ships += ";" + spec.ShieldStrength;
                        _text_ships += ";" + spec.ShieldRechargeRate;

                        _text_ships += ";" + spec.CloakStrength;
                        _text_ships += ";" + spec.CamouflagedStrength;

                        _text_ships += ";" + spec.ShipType;
                        _text_ships += ";" + spec.ClassName;
                        _text_ships += ";" + spec.EncyclopediaHeading;

                        if (spec.PrimaryWeapon != null)
                        {
                            _text_ships += ";" + spec.PrimaryWeaponName;
                            _text_ships += ";" + spec.PrimaryWeapon.Count;
                            _text_ships += ";" + spec.PrimaryWeapon.Damage;
                            _text_ships += ";" + spec.PrimaryWeapon.Refire;
                        }

                        if (spec.SecondaryWeapon != null)
                        {
                            _text_ships += ";" + spec.SecondaryWeaponName;
                            _text_ships += ";" + spec.SecondaryWeapon.Count;
                            _text_ships += ";" + spec.SecondaryWeapon.Damage;
                        }

                        //tdb_text += ";" + spec.ShipType;
                        //tdb_text += ";" + spec.ClassName;

                        //tdb_text += ";" + item.s;
                    }
                    tdb_text += _text_ships;
                    bool_output = true; // true again


                    // Ship Names
                    _shipnames_text = "### output for > ship_names  ( or no output )";
                    bool_output = false;

                    //ShipNames
                    if (bool_output == true && item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Ships)
                    {

                        if (_first_line_ship_names)
                        {
                            _shipnames_text += "Step_4349: ---------------" + _timeString + _newline;
                            _shipnames_text += DateTime.Now + ";COUNT;KEY;NAMES;COMMENT" + _newline;
                            _first_line_ship_names = false;
                        }

                        ShipDesign spec = item as ShipDesign;

                        int count = spec.PossibleNames.Count;

                        foreach (var name in spec.PossibleNames)
                        {
                            _shipnames_text += "Step_4359:"
                                + ";" + count
                                + ";" + item.Key

                                + " ;" + name.Key
                                + _newline;
                            Console.WriteLine(_shipnames_text);
                        }
                        //Console.WriteLine(_shipnames_text);


                        //_shipnames_text = DateTime.Now + ";COUNT;KEY;NAMES;COMMENT   >>> no output" + _newline;
                        //Console.WriteLine(_shipnames_text);

                        string _comment_inside_code = "No _ShipNames.csv because there is already a > _TechObj - 6 - Ships_NAMES_List(autoCreated).csv";
                        //_file = Path.Combine(_path_Resources_Data_Addon, "zz_ShipNames.csv");
                        //if (!string.IsNullOrEmpty(_file))
                        //{
                        //    StreamWriter streamWriter = new StreamWriter(_file);
                        //    streamWriter.Write(_shipnames_text);
                        //    streamWriter.Close();
                        //    _text_all_out = "output of _ShipNames done to " + _file;
                        //    if (writeDirectly)
                        //        Console.WriteLine(_text_all_out);
                        //}


                        //if (spec.PossibleNames != null)
                        //{
                        //    tdb_text += ";" + spec.PrimaryWeaponName;
                        //    tdb_text += ";" + spec.PrimaryWeapon.Count;
                        //    tdb_text += ";" + spec.PrimaryWeapon.Damage;
                        //    tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        //}

                        //if (spec.SecondaryWeapon != null)
                        //{
                        //    tdb_text += ";" + spec.SecondaryWeaponName;
                        //    tdb_text += ";" + spec.SecondaryWeapon.Count;
                        //    tdb_text += ";" + spec.SecondaryWeapon.Damage;
                        //    //tdb_text += ";" + spec.PrimaryWeapon.Refire;
                        //}

                        //tdb_text += ";" + spec.ShipType;
                        //tdb_text += ";" + spec.ClassName;

                        //tdb_text += ";" + item.s;
                    }

                    _text_all_out += tdb_text + _newline;

                }



                _text_all_out += _shipnames_text;
                //}

            }

            if (writeDirectly) 
                Console.WriteLine(_text_all_out);   // Output here as well






            _file = Path.Combine(_path_Resources_Data_Addon, "_MapData.txt");  // by ALT+M at GalaxyMap
            //string _file = Path.Combine(AppContext., "_MapData.txt");
            if (!string.IsNullOrEmpty(_file)/* && File.Exists(_file)*/)
            {
                StreamWriter streamWriter = new StreamWriter(_file);
                streamWriter.Write(_text_all_out);
                streamWriter.Close();
                _text_all_out = "output of _MapData done to " + _file;
                if (writeDirectly) 
                    Console.WriteLine(_text_all_out);
            }



            //_also do for more output-files
            //TechDatabase.Load();  // don't do this at the moment

            GameContext.Report_DiplomacyData();

            /*System.Media.SoundPlayer */_wav_player = new System.Media.SoundPlayer("Resources/SoundFX/sound001.wav");
            _wav_player.Play();

        }

        private void ExecuteCheatMenuCommand(object t)
        {

            // to do: just check whether IsHumanPlayer more than one (whenever SP is started by MP-Screen)
            //if (PlayerContext.Current.Players.Count)
            //    if (PlayerContext.Current.Players.Contains)
            if (!_appContext.IsSinglePlayerGame)
            {
                _ = MessageDialog.Show("Cheat Menu is not available in MultiPlayer", "INFO", MessageDialogButtons.Ok);
                return;
            }

            CheatMenu cheatMenu = new CheatMenu(_appContext);
            _ = cheatMenu.ShowDialog();
        }

        private void Execute_f12_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f12_Screen = new GameInfoScreen(_appContext);
            _ = _f12_Screen.ShowDialog();
        }
        private void Execute_f11_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f11_Screen = new GameInfoScreen(_appContext);
            _ = _f11_Screen.ShowDialog();
        }
        private void Execute_f10_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f10_Screen = new GameInfoScreen(_appContext);
            _ = _f10_Screen.ShowDialog();
        }

        private void Execute_f09_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            //var _f09_Screen = new GameInfoScreen(_appContext);
            //_f09_Screen.ShowDialog();
            GameInfoScreen GameInfoScreen = new GameInfoScreen(_appContext);
            _ = GameInfoScreen.ShowDialog();
        }

        private void Execute_f08_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            ColonyInfoScreen _f08_Screen = new ColonyInfoScreen(_appContext);
            _ = _f08_Screen.ShowDialog();
        }

        private void Execute_f07_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f07_Screen = new GameInfoScreen(_appContext);
            _ = _f07_Screen.ShowDialog();
        }

        private void Execute_f06_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            ColorInfoScreen _f06_Screen = new ColorInfoScreen(_appContext);
            _ = _f06_Screen.ShowDialog();
        }
        #endregion

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(sender is IAppContext))
            {
                return false;
            }

            if (!(e is PropertyChangedEventArgs propertyChangedEventArgs))
            {
                return false;
            }

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LocalPlayerEmpire":
                    OnLocalPlayerEmpireChanged();
                    break;
            }

            return true;
        }
    }
}