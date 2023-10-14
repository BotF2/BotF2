using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Input;
using Supremacy.Client.Views.GalaxyScreen;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
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
        private readonly DelegateCommand<object> _cheatMenuCommand;
        private readonly DelegateCommand<object> _f12_ScreenCommand;
        private readonly DelegateCommand<object> _f11_ScreenCommand;
        private readonly DelegateCommand<object> _f10_ScreenCommand;
        private readonly DelegateCommand<object> _f09_ScreenCommand;
        private readonly DelegateCommand<object> _f08_ScreenCommand;
        private readonly DelegateCommand<object> _f07_ScreenCommand;
        private readonly DelegateCommand<object> _f06_ScreenCommand;
        private readonly string newline = Environment.NewLine;
        private readonly string _text;
        private string restriction_text;

        #region Constructors and Finalizers
        public GalaxyGridView([NotNull] IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException("container");
            _appContext = _container.Resolve<IAppContext>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            InitializeComponent();

            _text = "Step_2100: GalaxyGridView Initialize...";
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
            DebugCommands.CheatMenu.RegisterCommand(_cheatMenuCommand);
            DebugCommands.F12_Screen.RegisterCommand(_f12_ScreenCommand);
            DebugCommands.F11_Screen.RegisterCommand(_f11_ScreenCommand);
            DebugCommands.F10_Screen.RegisterCommand(_f10_ScreenCommand);
            DebugCommands.F09_Screen.RegisterCommand(_f09_ScreenCommand);
            DebugCommands.F08_Screen.RegisterCommand(_f08_ScreenCommand);
            DebugCommands.F07_Screen.RegisterCommand(_f07_ScreenCommand);
            DebugCommands.F06_Screen.RegisterCommand(_f06_ScreenCommand);

            _text = "Step_2101: GalaxyGridView Initialize done...";
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

                if (diplomat.GetForeignPower(civ).DiplomacyData.Status == ForeignPowerStatus.NoContact)
                {
                    diplomat.GetForeignPower(civ).DiplomacyData.Status = ForeignPowerStatus.Neutral;
                    //diplomat.GetForeignPower(civ).DiplomacyData.ContactTurn = 999999;   // ships are not visible yet
                }
            }
            GalaxyGrid.Update();
        }

        private void ExecuteOutputMapCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //{
            //    return;
            //}
            var time = DateTime.Now;
            string Year = time.Year.ToString(); Year = CheckDateString(Year);
            string Month = time.Month.ToString(); Month = CheckDateString(Month);
            string Day = time.Day.ToString(); Day = CheckDateString(Day);
            string Hour = time.Hour.ToString(); Hour = CheckDateString(Hour);
            string Minute = time.Minute.ToString(); Minute = CheckDateString(Minute);
            string Second = time.Second.ToString(); Second = CheckDateString(Second);
            string timeString = "Output_" + Year + "_" + Month + "_" + Day + "-" + Hour + "_" + Minute + "_" + Second + ".txt";

            string shipnames_text = "";
            string stationnames_text = "";

            SectorMap map = _appContext.CurrentGame.Universe.Map;

            string _text = timeString + newline;
            _text += "** Example:  MAP Location (2,5) = line 5, column 2 ** use CTRL+F for searching... ** ...before half width ('|') add some few minus **   "
                + newline
                + newline
                + "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + newline
                + newline
                ;
            int yhalf = map.Height / 2;
            int xhalf = map.Width / 2;

            for (int y = 0; y < map.Height; y++)
            {
                if (y < 10) _text += " ";  // 1 to 9 getting a blank before

                if (y == yhalf) _text += "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + newline;
                _text += y + ":  ";
                for (int x = 0; x < map.Width; x++)
                {
                    if (x == xhalf) _text += "| ";
                    string owner = ".";
                    if (map[x, y].Owner != null)
                    {
                        owner = map[x, y].Owner.CivID.ToString();
                        if (map[x, y].Owner.CivID > 6) owner = "M"; // Minor
                    }

                    string type = ".";
                    if (map[x, y].System != null)
                    {
                        type = map[x, y].System.StarType.ToString().Substring(0, 1);
                        if (map[x, y].System.StarType == StarType.BlackHole) type = "b";
                        if (map[x, y].System.StarType == StarType.NeutronStar) type = "n";
                        //if (map[x, y].System.StarType == StarType.Quasar) type = "Q";
                        if (map[x, y].System.StarType == StarType.RadioPulsar) type = "r";
                        if (map[x, y].System.StarType == StarType.XRayPulsar) type = "x";
                        if (map[x, y].System.StarType == StarType.Wormhole) type = "w";
                    }
                    _text += owner + type + " ";
                    //Console.WriteLine(_text);
                }
                _text += newline;
                //Console.WriteLine(_text);
            }

            _text +=
                newline + "------0--------------5-------------10-------------15-------------20-------------25-------------30-------------35-------------40-------------45-------------50-------------55----------59" + newline
                + newline
                + "1st character:                                     2nd character: StarSystem" + newline
                + "   0 = Federation                                     B = Blue star" + newline//" + newline
                + "   1 = Terrans                                        O = Orange star" + newline//" + newline
                + "   2 = Romulans                                       N = Nebula" + newline//" + newline
                + "   3 = Klingons                                       R = Red star" + newline//" + newline
                + "   4 = Cardassian                                     Y = Yellow star" + newline//" + newline
                + "   5 = Dominion                                       W = White star" + newline//" + newline
                + "   6 = Borg                                           B = Blue star" + newline//" + newline
                + newline /*+ newline*/
                + "   M = Minor                                          b = black hole" + newline
                + "                                                      n = Neutron star" + newline
                + "                                                      Q = Quasar" + newline
                + "                                                      r = Radio Pulsar" + newline
                + "                                                      w = worm hole" + newline
                + "                                                      x = x-ray Pulsar" + newline
                //+ "2nd character: StarSystem" + newline
                //+ "   B = Blue star" + newline
                //+ "   O = Orange star" + newline
                //+ "   N = Nebula" + newline
                //+ "   R = Red star" + newline
                //+ "   Y = Yellow star" + newline
                //+ "   W = White star" + newline
                //+ newline
                ;


            if (GameContext.Current != null)
            {

                foreach (CivilizationManager civManager in GameContext.Current.CivilizationManagers)
                {
                    _text += civManager.HomeColony.Location.X
                        + " ; " + civManager.HomeColony.Location.Y
                        + " ; " + civManager.Civilization.HomeSystemName
                        + " ; " + civManager.Civilization.HomeQuadrant + "-Quadrant"
                        + " ;" + civManager.Civilization

                        + newline + newline;
                }

                IEnumerable<Colony> colonies = GameContext.Current.Universe.Objects.OfType<Colony>();
                foreach (Colony item in colonies)
                {
                    String _col =
                        /*";Colony;" */
                        /*"; " + */item.Location
                        + ";" + item.Name
                        + ";" + item.Owner
                        + ";Colony;"
                        ;

                    _text += newline
                        + "Step_4361:"
                        + "; " + item.Location
                        + "; " + item.ObjectID
                        + ";Colony"
                        + ";" + item.Name
                        + ";" + item.Owner
                        + ";pop;" + item.Population
                        + ";max;" + item.MaxPopulation


                        + ";mor;" + item.Morale
                        + ";FoodR;" + item.FoodReserves
                        + ";facF;" + item.Facilities_Active1_Food + ";of;" + item.Facilities_Total1_Food
                        + ";facI;" + item.Facilities_Active2_Industry + ";of;" + item.Facilities_Total2_Industry
                        + ";facE;" + item.Facilities_Active3_Energy + ";of;" + item.Facilities_Total3_Energy
                        + ";facR;" + item.Facilities_Active4_Research + ";of;" + item.Facilities_Total4_Research
                        + ";facI;" + item.Facilities_Active5_Intelligence + ";of;" + item.Facilities_Total5_Intelligence


                        + ";since Turn;" + item.TurnCreated

                                                    + newline;
                    //Console.WriteLine(_text);
                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text);

                    ILookup<MapLocation, StarSystem> systemLocationLookup = GameContext.Current.Universe.Objects.OfType<StarSystem>().ToLookup(o => o.Location);


                    //_text += newline;
                    ILookup<MapLocation, Building> buildingLocationLookup = GameContext.Current.Universe.Objects.OfType<Building>().ToLookup(o => o.Location);
                    foreach (Building building in buildingLocationLookup[item.Location])
                    {
                        _text += "Step_4366:; "
                            + _col
                            + "; Building"
                            + "; " + building.ObjectID
                            + "; " + building.Design
                            + ";" + building.IsActive + "_for_Active"
                            + "; since Turn;" + building.TurnCreated

                            + newline;

                        //Console.WriteLine(_text);

                    }

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
                                    _text += "Step_7603:; " + slot.Shipyard.Location
                                        + " > Slot= " + slot.SlotID

                                        + " "
                                        + " > " + _percent
                                        + " done for " + _design
                                        + " at " + slot.Shipyard.Name
                                        + newline;
                                    //Console.WriteLine(_text);
                                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                                }
                                else
                                {
                                    _text += "Step_7607:; " + slot.Shipyard.Location
                                        + " > Slot= " + slot.SlotID  // crashes with a StackOverFlow
                                                                     //+ " at " + slot.Shipyard.Name
                                        + " "
                                        + " > " + _percent
                                        + " done for " + _design
                                        + newline;
                                    //Console.WriteLine(_text);
                                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                                }

                                //}
                            }
                            catch
                            {
                                _text += "Step_7609: Serialize failed"
                                     //+ slot.Project.Location
                                     //+ " > Slot= " + slot.SlotID
                                     //+ " at " + slot.Shipyard.Name
                                     //+ " " + 
                                     //+ " > " + _percent
                                     //+ " done for " + _design
                                     + newline;
                                //Console.WriteLine(_text);
                                //GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                            };
                        }
                    }


                }

                _text += newline;



                IEnumerable<Ship> ships = GameContext.Current.Universe.Objects.OfType<Ship>();
                foreach (Ship item in ships)
                {
                    _text += "Step_4381:"
                            + "; " + item.Location
                            + "; Ship"

                            + "; " + item.Owner
                            + "; " + item.ObjectID
                            + "; " + item.Design
                            + "; " + item.Name

                            + "; Crew=;" + item.Crew
                            + "; Exp=;" + item.ExperiencePercent
                            + "; Hull=;" + item.HullStrength
                            + "; Sh=;" + item.ShieldStrength
                            + "; Cloak=;" + item.CloakStrength
                            + "; Camo=;" + item.CamouflagedStrength
                            //+ "; Camo=;" + item.
                            + "; Fuel=;" + item.FuelReserve

                            + "; since Turn;" + item.TurnCreated

                                                        + newline;
                    //Console.WriteLine("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                    //Console.WriteLine(_text);
                    //GameLog.Core.SaveLoadDetails.DebugFormat("Step_4381: Ship_Output is ongoing to nowhere :-) ... ");
                    //GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                }


                _text += newline;

                IEnumerable<Station> stations = GameContext.Current.Universe.Objects.OfType<Station>();
                foreach (Station item in stations)
                {
                    _text += "Step_4391:"
                            + "; " + item.Location
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

                                                        + newline;
                }

                _text += newline;

                IEnumerable<Shipyard> shipyards = GameContext.Current.Universe.Objects.OfType<Shipyard>();
                foreach (Shipyard item in shipyards)
                {
                    _text += "Step_4391:"
                            + "; " + item.Location
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

                                                        + newline;
                }

                _text += newline;

                var races = GameContext.Current.Races.ToList();
                foreach (var item in races)
                {
                    _text += "Step_4396:"
                            //+ "; " + item.
                            + "; Race"

                            //+ "; " + item.Owner
                            //+ "; " + item.ObjectID
                            //+ "; " + item.Design
                            + "; " + item.Key

                                                        + "; HomePlanet=;" + item.HomePlanetType
                                                        + "; Eff=;" + item.CombatEffectiveness
                                                        //+ "; Hull=;" + item.HullStrength
                                                        //+ "; Sh=;" + item.ShieldStrength
                                                        //+ "; Cloak=;" + item.CloakStrength
                                                        //+ "; Camo=;" + item.CamouflagedStrength
                                                        //+ "; Camo=;" + item.
                                                        //+ "; Fuel=;" + item.FuelReserve

                                                        //+ "; since Turn;" + item.TurnCreated

                                                        + newline;
                }

                //var events = null;
                if (GameContext.Current.ScriptedEvents != null)
                {
                    _text += "Step_4356:; Events following..." + newline;
                    var events = GameContext.Current.ScriptedEvents.ToList();
                    foreach (var item in events)
                    {
                        _text += "Step_4356:"
                                //+ "; " + item.
                                + "; Events"

                                //+ "; " + item.Owner
                                //+ "; " + item.ObjectID
                                //+ "; " + item.Design
                                + "; " + item.EventID

                                                            + "; Last=;" + item.LastExecution
                                                            //+ "; Eff=;" + item.CombatEffectiveness
                                                            //+ "; Hull=;" + item.HullStrength
                                                            //+ "; Sh=;" + item.ShieldStrength
                                                            //+ "; Cloak=;" + item.CloakStrength
                                                            //+ "; Camo=;" + item.CamouflagedStrength
                                                            //+ "; Camo=;" + item.
                                                            //+ "; Fuel=;" + item.FuelReserve

                                                            //+ "; since Turn;" + item.TurnCreated

                                                            + newline;
                    }
                }

                _text += newline;

                var civs = GameContext.Current.Civilizations.ToList();
                foreach (Civilization item in civs)
                {
                    _text += "Step_4346:"
                            //+ "; " + item.Location
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

                                                        + newline;
                }

                _text += newline;

                var civMans = GameContext.Current.CivilizationManagers.ToList();
                foreach (CivilizationManager item in civMans)
                {
                    _text += "Step_4348:"
                            //+ "; " + item.Location
                            + "; CivMan"
                            + "; " + item.Civilization
                            + "; ID=" + item.CivilizationID


                                                        + newline;
                }

                _text += newline;

                //var civMans = GameContext.Current.ScriptedEvents;
                if (GameContext.Current.ScriptedEvents != null)
                {
                    foreach (var item in GameContext.Current.ScriptedEvents)
                    {
                        _text += "Step_4349:"
                                //+ "; " + item.Location
                                + "; CivMan"
                                + "; " + item.EventID
                                                            //+ "; " + item.Civilization

                                                            + newline;
                    }
                }

                _text += newline;

                //IEnumerable<ShipDesign> bd = GameContext.Current.TechDatabase.Select(i => GameContext.Current.TechDatabase[i] as ShipDesign);
                //if (GameContext.Current.TechDatabase)
                //{
                //    foreach (var item in GameContext.Current.TechDatabase)
                //{
                bool first_shipname = true;
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
                    //string shipnames_text = "";

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
                    //tdb_text += ";" + item.des;




                    //+ "; " + item.DesignID
                    //+ "; " + item.Name;
                    ;



                    //Batteries
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Batteries)
                    {

                        if (first_orbbat)
                        {
                            _text += "Step_4341: ---------------" + newline;
                            _text += "Step_4341:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Sc%;SP;SR;HULL;SH;ShR;W1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + newline;
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
                            _text += "Step_4342: ---------------" + newline;
                            _text += "Step_4342:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;LA;FIELD;OUTP;COMMENT" + newline;
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
                            _text += "Step_4343: ---------------" + newline;
                            _text += "Step_4343:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;EN;R1;R2;R3;R4;Bo1;B1V;Bo2;B2V;COMMENT" + newline;
                            first_buildings = false;
                        }

                        Buildings.BuildingDesign spec = item as Buildings.BuildingDesign;
                        tdb_text += ";" + spec.EnergyCost;

                        //Restriction
                        if (spec.Restriction.ToString() != null)
                        {
                            restriction_text = ";";

                            if (spec.Restriction.ToString().Contains("HomeSystem")) restriction_text += "HoS;"; else restriction_text += ";";
                            if (spec.Restriction.ToString().Contains("OnePerEmpire")) restriction_text += "OneE;"; else restriction_text += ";";
                            if (spec.Restriction.ToString().Contains("OnePerSystem")) restriction_text += "OneS;"; else restriction_text += ";";
                            //if (spec.Restriction.ToString() == "HomeSystem") restriction_text += "HoS;"; else restriction_text += ";" ;
                            //if (spec.Restriction.ToString() == "HomeSystem") restriction_text += "HoS;"; else restriction_text += ";" ;
                            //if (spec.Restriction.ToString() == "HomeSystem") restriction_text += "HoS;"; else restriction_text += ";" ;
                        }
                        tdb_text += restriction_text;

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
                            _text += "Step_4344: ---------------" + newline;
                            _text += "Step_4344:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;EN;SL;OUTP;TYPE;OUTPm;maxLvl;COMMENT" + newline;
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
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Stations)
                    {

                        if (first_stations)
                        {
                            _text += "Step_4345: ---------------" + newline;
                            _text += "Step_4345:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Sc%;SP;SR;HULL;SH;ShR;W1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + newline;
                            first_stations = false;
                        }

                        StationDesign spec = item as StationDesign;
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

                        //StationNames
                        if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Stations)
                        {

                            if (first_stationname)
                            {
                                //stationnames_text += "Step_4349: ---------------" + newline;
                                stationnames_text += "Step_4359:;COUNT;KEY;NAMES;COMMENT" + newline;
                                first_stationname = false;
                            }

                            StationDesign spec2 = item as StationDesign;

                            //tdb_text += ";" + spec.Dilithium;
                            //tdb_text += ";" + spec.Speed;
                            //tdb_text += ";" + spec.Range;
                            //tdb_text += ";" + spec.FuelCapacity;
                            //tdb_text += ";" + spec.Maneuverability;
                            //tdb_text += ";" + spec.WorkCapacity;

                            //tdb_text += ";" + spec.ScienceAbility;
                            //tdb_text += ";" + spec.ScanStrength;
                            //tdb_text += ";" + spec.SensorRange;
                            //tdb_text += ";" + spec.HullStrength;
                            //tdb_text += ";" + spec.ShieldStrength;
                            //tdb_text += ";" + spec.ShieldRechargeRate;

                            //tdb_text += ";" + spec.CloakStrength;
                            //tdb_text += ";" + spec.CamouflagedStrength;

                            //tdb_text += ";" + spec.StationType;
                            //tdb_text += ";" + spec.ClassName;

                            int count = spec2.PossibleNames.Count;

                            foreach (var name in spec2.PossibleNames)
                            {
                                stationnames_text += "Step_4359:"
                                    + ";" + count
                                    + ";" + item.Key

                                    + " ;" + name.Key
                                    + newline;
                            }

                        }
                    }

                    //ShipDesign
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Ships)
                    {

                        if (first_ships)
                        {
                            _text += "Step_4346: ---------------" + newline;
                            _text += "Step_4346:;ID;KEY;BIO;CP;CS;EN;PR;WP;UNIVE;BCo;DUR;MA;PoH;CATE;Obs;Up;Dil;Spe;Ra;Fu;Man;WO;Sc%;SP;SR;"
                                                            + "HULL;SH;ShR;Cl;Ca;TYPE;CLASSNAME;N_+_Sp;W1=Weapon 1;W1C;W1D;W1R;W2;W2C;W2D;COMMENT" + newline;
                            first_ships = false;
                        }

                        ShipDesign spec = item as ShipDesign;

                        tdb_text += ";" + spec.Dilithium;
                        tdb_text += ";" + spec.Speed;
                        tdb_text += ";" + spec.Range;
                        tdb_text += ";" + spec.FuelCapacity;
                        tdb_text += ";" + spec.Maneuverability;
                        tdb_text += ";" + spec.WorkCapacity;

                        tdb_text += ";" + spec.ScienceAbility;
                        tdb_text += ";" + spec.ScanStrength;
                        tdb_text += ";" + spec.SensorRange;
                        tdb_text += ";" + spec.HullStrength;
                        tdb_text += ";" + spec.ShieldStrength;
                        tdb_text += ";" + spec.ShieldRechargeRate;

                        tdb_text += ";" + spec.CloakStrength;
                        tdb_text += ";" + spec.CamouflagedStrength;

                        tdb_text += ";" + spec.ShipType;
                        tdb_text += ";" + spec.ClassName;
                        tdb_text += ";" + spec.EncyclopediaHeading;

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

                        //tdb_text += ";" + spec.ShipType;
                        //tdb_text += ";" + spec.ClassName;

                        //tdb_text += ";" + item.s;
                    }

                    //ShipNames
                    if (item.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Ships)
                    {

                        if (first_shipname)
                        {
                            //shipnames_text += "Step_4349: ---------------" + newline;
                            shipnames_text += "Step_4359:;COUNT;KEY;NAMES;COMMENT" + newline;
                            first_shipname = false;
                        }

                        ShipDesign spec = item as ShipDesign;

                        //tdb_text += ";" + spec.Dilithium;
                        //tdb_text += ";" + spec.Speed;
                        //tdb_text += ";" + spec.Range;
                        //tdb_text += ";" + spec.FuelCapacity;
                        //tdb_text += ";" + spec.Maneuverability;
                        //tdb_text += ";" + spec.WorkCapacity;

                        //tdb_text += ";" + spec.ScienceAbility;
                        //tdb_text += ";" + spec.ScanStrength;
                        //tdb_text += ";" + spec.SensorRange;
                        //tdb_text += ";" + spec.HullStrength;
                        //tdb_text += ";" + spec.ShieldStrength;
                        //tdb_text += ";" + spec.ShieldRechargeRate;

                        //tdb_text += ";" + spec.CloakStrength;
                        //tdb_text += ";" + spec.CamouflagedStrength;

                        //tdb_text += ";" + spec.ShipType;
                        //tdb_text += ";" + spec.ClassName;

                        int count = spec.PossibleNames.Count;

                        foreach (var name in spec.PossibleNames)
                        {
                            shipnames_text += "Step_4359:"
                                + ";" + count
                                + ";" + item.Key

                                + " ;" + name.Key
                                + newline;
                        }


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


                    _text += tdb_text + newline;





                }
                //_text += shipnames_text;
                //}

            }

            Console.WriteLine(_text);   // Output here as well

            string file = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "_MapData.txt");
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.Write(_text);
                streamWriter.Close();
                _text = "output of _MapData done to " + file;
                Console.WriteLine(_text);
            }

            file = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "_ShipNames.csv");
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.Write(shipnames_text);
                streamWriter.Close();
                _text = "output of _ShipNames done to " + file;
                Console.WriteLine(_text);
            }

            file = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "_StationNames.csv");
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.Write(stationnames_text);
                streamWriter.Close();
                _text = "output of _StationNames done to " + file;
                Console.WriteLine(_text);
            }
        }

        private static string CheckDateString(string _string)
        {
            if (_string.Length == 1)
                _string = "0" + _string;

            return _string;
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