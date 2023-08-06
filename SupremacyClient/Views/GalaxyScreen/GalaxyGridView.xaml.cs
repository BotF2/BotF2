﻿using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Input;
using Supremacy.Client.Views.GalaxyScreen;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
//using Obtics.Collections;
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
                        + ";facF;" + item.ActiveFoodFacilities + ";of;" + item.TotalFoodFacilities
                        + ";facI;" + item.ActiveIndustryFacilities + ";of;" + item.TotalIndustryFacilities
                        + ";facE;" + item.ActiveEnergyFacilities + ";of;" + item.TotalEnergyFacilities
                        + ";facR;" + item.ActiveResearchFacilities + ";of;" + item.TotalResearchFacilities
                        + ";facI;" + item.ActiveIntelligenceFacilities + ";of;" + item.TotalIntelligenceFacilities


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
                if (GameContext.Current.ScriptedEvents != null) {
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

            }

            Console.WriteLine(_text);   // Output here as well

            string file = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "MapData.txt");
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                StreamWriter streamWriter = new StreamWriter(file);
                streamWriter.Write(_text);
                streamWriter.Close();
                _text = "output of MapData done to " + file;
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