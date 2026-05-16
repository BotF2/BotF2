// File:CombatWindow.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Combat;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for CombatWindow.xaml
    /// </summary>

    public partial class CombatWindow
    {
        private CombatUpdate _update;
        private CombatAssets _playerAssets;
        private CombatAssets _otherAssets;
        private List<Civilization> _otherCivs; // this collection populates UI with 'other' civilizations found in the sector
        private List<Civilization> _friendlyCivs; // players civ and fight along side civs if any    
                                                  //#pragma warning disable IDE0052 // Remove unread private members
                                                  //private List<Civilization> OtherCivs = new List<Civilization> { }; // just a dummy to avoid: Error: 40 : BindingExpression path error:
                                                  //private List<Civilization> FriendlyCivs = new List<Civilization> { }; // just a dummy to avoid: Error: 40 : BindingExpression path error:  
                                                  //#pragma warning restore IDE0052 // Remove unread private members
        private readonly Civilization _onlyFireIfFiredApp_1;
        private readonly Civilization _onlyFireIfFiredApp_2;
        private Civilization _targeted_civ_1;
        private Civilization _targeted_civ_2;

        private readonly IAppContext _appContext;

        //[NonSerialized]
        //private string _text_combatWindow;
        //private string _newline = Environment.NewLine;
        //private int _otherFirePower;

        public List<Civilization> FriendlyCivs { get; private set; }
        public List<Civilization> OtherCivs { get; private set; }

        public Civilization Targeted1Civ { get; private set; }
        public Civilization Targeted2Civ { get; private set; }

        public CombatWindow()
        {
            InitializeComponent();

            _targeted_civ_1 = null;
            _targeted_civ_2 = null;

            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            _ = ClientEvents.CombatUpdateReceived.Subscribe(OnCombatUpdateReceived, ThreadOption.UIThread);
            DataTemplate itemTemplate = TryFindResource("AssetsTreeItemTemplate") as DataTemplate;

            FriendlyStationItem.HeaderTemplate = itemTemplate;
            FriendlyCombatantItems.ItemTemplate = itemTemplate;
            FriendlyNonCombatantItems.ItemTemplate = itemTemplate;
            FriendlyDestroyedItems.ItemTemplate = itemTemplate;
            FriendlyAssimilatedItems.ItemTemplate = itemTemplate;
            FriendlyEscapedItems.ItemTemplate = itemTemplate;
            HostileStationItem.HeaderTemplate = itemTemplate;
            HostileCombatantItems.ItemTemplate = itemTemplate;
            HostileNonCombatantItems.ItemTemplate = itemTemplate;
            HostileDestroyedItems.ItemTemplate = itemTemplate;
            HostileAssimilatedItems.ItemTemplate = itemTemplate;
            HostileEscapedItems.ItemTemplate = itemTemplate;

            DataTemplate civFriendTemplate = TryFindResource("FriendTreeTemplate") as DataTemplate;

            // friend civilizations summary
            FriendCivilizationsItems.ItemTemplate = civFriendTemplate;

            FriendCivilizationsItems.DataContext = _friendlyCivs;



            FriendlyCivs = _friendlyCivs; // just a dummy to avoid: Error: 40 : BindingExpression path error:

            DataTemplate civTemplate = TryFindResource("OthersTreeSummaryTemplate") as DataTemplate;
            // other civilizations summary for targeting
            OtherCivilizationsSummaryItem1.ItemTemplate = civTemplate;

            OtherCivilizationsSummaryItem1.DataContext = _otherCivs; // ListBox data context set to OtherCivs

            //OtherCivs = _otherCivs; // just a dummy to avoid: Error: 40 : BindingExpression path error:

            _onlyFireIfFiredApp_1 = new Civilization
            {
                //_onlyFireIfFiredApp_1.ShortName = "Only Return Fire";
                ShortName = ResourceManager.GetString("ONLY_RETURN_FIRE")
                ,
                CivID = 888
                ,
                Key = "Only Return Fire"
                //TargetCiv1Status = "",
                //TargetCiv2Status = ""

            };

            //_targeted_civ_1 = (Civilization)radioButton1.DataContext;
            //_targeted_civ_1 = this.

            try
            {
                if (_otherCivs != null)
                {
                _targeted_civ_1 = GameContext.Current.CivilizationManagers[_otherCivs.FirstOrDefault().CivID].Civilization;
                }

                //Debugger.Break();
            }
            catch
            {
                Debugger.Break();
            }



            // be careful for activated ... does the game proceed into the next turn ??
            _onlyFireIfFiredApp_2 = new Civilization
            {
                //_onlyFireIfFiredApp_1.ShortName = "Only Return Fire";
                ShortName = ResourceManager.GetString("ONLY_RETURN_FIRE")
    ,
                CivID = 888
    ,
                Key = "Only Return Fire"
                //TargetCiv1Status = "",
                //TargetCiv2Status = ""

            };
            // The click of "Only Return Fire" radio button by human player
            // _targeted_civ_1 = new Civilization();
            _targeted_civ_1 = _onlyFireIfFiredApp_1;
            // _targeted_civ_2 = new Civilization();
            _targeted_civ_2 = _onlyFireIfFiredApp_2;

        }

        private void OnCombatUpdateReceived(DataEventArgs<CombatUpdate> args)
        {
            HandleCombatUpdate(args.Value);
        }


        private void HandleCombatUpdate(CombatUpdate update)
        {
            _update = update;
            //_text_combatWindow = _newline; // dummy - just keep

            string _text_combatWindow = "Step_6787:; "
                + "CombatID=" + _update.CombatID + ": " + "Red Alert at "
                + _update.Location
                + " > " + _update.FriendlyAssets.Count() + " on our side - "
                + _update.HostileAssets.Count() + " hostile "
                ;
            Console.WriteLine(_text_combatWindow);
            //GameLog.Client.EventsDetails.DebugFormat(_text_combatWindow);



            //List<CivilizationManager> _civs = new List<CivilizationManager>();
            CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];


            foreach (CombatAssets assets in update.FriendlyAssets)
            {
                if (assets.Owner == _appContext.LocalPlayer.Empire)
                {
                    _playerAssets = assets;
                    _playerAssets.CombatID = _update.CombatID;
                    // CombatAssets.CombatID has an internal setter; avoid setting it from the client assembly.
                    // The combat ID should already be set by the source of the update. Do not attempt to assign here.
                    break;
                }
                else
                {
                    _otherAssets = assets;
                }
            }

            if (_playerAssets == null)
            {
                _playerAssets = update.FriendlyAssets[0];
            }
            if (_otherAssets == null) // && update != null && update.HostileAssets.Count() >0)
            {
                _otherAssets = update.HostileAssets[0];
            }

            //FriendlyCivs.Add(update.FriendlyAssets[0].Owner); // test 

            //OtherCivs.Add(update.HostileAssets[0].Owner); // test 


            //for (int i = 0; i < 4; i++)
            //{

            //}

            DataContext = _update;

            SubHeader2Text.Text = string.Format(ResourceManager.GetString("COMBAT_TEXT_DURABILITY"));

            //, _update.Sector.Name);

            // as long as (relevant) hostileassets is not null
            if (update.CombatUpdate_IsCombatOver)  // Combat is over
            {

                DialogResult = true;
                Close();
                // -----------------------------------------------------
                //if (update.CombatUpdate_IsCombatOver)
                //{
                string _newline = Environment.NewLine;
                //string _newline = Environment.NewLine;

                Civilization _localPlayer = _appContext.LocalPlayer.Empire;
                MapLocation _loc = update.Location;




                //string _civ2 = "";
                //if (update.CivName2 != "")
                //{
                //    _civ2 = update.CivName2;
                //}

                //string _civ3 = "";
                //if (update.CivName3 != "")
                //{
                //    _civ3 = update.CivName3;
                //}

                //string _civ4 = "";
                //if (update.CivName4 != "")
                //{
                //    _civ4 = update.CivName4;
                //}
                
                //string _allFriendlyAssetsText = "";


                //string _allHostileAssetsText = "";


                string _resultHeaderText = _update.Owner.ToString().ToUpper() + ":  --- RESULT for  > Combat" // at = " + update.CombatID
                                                                                                                      //+ " Round= " + update.RoundNumber
                    + " at " + update.Location.ToString() 
                    + " ---"
    
                    /*+ _newline*/
                    //+ " > Durability " + update.FriendlyEmpireStrength + " vs " + update.AllHostileEmpireStrength
                    ; // + _newline

                //string _resultText =
                ////_update.Owner.ToString().ToUpper() + " > Combat" // at = " + update.CombatID
                ////                              //+ " Round= " + update.RoundNumber
                ////    + " at " + update.Location.ToString() /*+ _newline*/
                ////    + " > Durability " + update.FriendlyEmpireStrength + " vs " + update.AllHostileEmpireStrength + _newline
                //    "----------------------------------------------------------------------------------------------------------------"
                ////    //+ update.CivFirePowers1 + " = " + update.CivFirePowers1Text + _newline
                ////    //+ _newline + _update.Owner + _newline
                ////    //+ _update.CivName1 + _newline
                //    + _allFriendlyAssetsText + _newline
                //    + _newline
                //    + _allHostileAssetsText + _newline
                //    ;

                // No Output, no MessageBox here !!

            //    MessageDialogResult _result = MessageDialog.Show(_resultHeaderText
            //        , _resultText, MessageDialogButtons.Ok);

            //Again:;
            //    if (_result != MessageDialogResult.Ok)
            //    {
            //        Thread.Sleep(100);
            //        goto Again;
            //    }



                string _combatText = "Red Alert at " + _update.Sector.Location + " > ";
                if (_update.IsStandoff)
                {
                    _combatText += string.Format(ResourceManager.GetString("COMBAT_STANDOFF"));
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + _text_combatWindow;
                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_STANDOFF"),
                        _update.Sector.Name);
                    _text_combatWindow += _combatText + " - no winner";

                    //CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text_combatWindow, "", "", SitRepPriority.Red));

                    //playerCivManager.SitRepEntries.Add(new CombatSummarySitRepEntry(playerCivManager.Civilization, _update.Sector.Location,
                    //    string.Format(ResourceManager.GetString("COMBAT_TEXT_STANDOFF"), _update.Sector.Name)));
                    Console.WriteLine("Step_6884:; " + _text_combatWindow);


                }
                else if (_playerAssets.HasSurvivingAssets || _playerAssets.EscapedShips.Count > 0)
                {
                    _combatText += string.Format(ResourceManager.GetString("COMBAT_VICTORY"));

                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + string.Format(ResourceManager.GetString("COMBAT_VICTORY"));

                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_VICTORY"),
                        _update.Sector.Name);

                    _text_combatWindow = _combatText + " - we were victorious !";

                    //CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text_combatWindow, "", "", SitRepPriority.Red));
                    Console.WriteLine("Step_6886:; " + _text_combatWindow);
                }
                else
                {
                    _combatText += string.Format(ResourceManager.GetString("COMBAT_DEFEAT"));

                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + string.Format(ResourceManager.GetString("COMBAT_DEFEAT"));

                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_DEFEAT"),
                        _update.Sector.Name);

                    _text_combatWindow = _combatText + " - we were not victorious !";




                    //CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text_combatWindow, "", "", SitRepPriority.Red));
                    Console.WriteLine("Step_6888:; " + _text_combatWindow);
                }

            }
            else // combat is not over
            {
                HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER"); // + ": "
                                                                              //+ String.Format(ResourceManager.GetString("COMBAT_ROUND"), _update.RoundNumber);
                SubHeaderText.Text = string.Format(
                    ResourceManager.GetString("COMBAT_TEXT_ENCOUNTER"),
                    _update.Sector.Name);

                SoundPlayer soundPlayer = new SoundPlayer("Resources/SoundFX/REDALERT.wav");
                {
                    if (File.Exists("Resources/SoundFX/REDALERT.wav") && ClientSettings.Current.EnableSoundRedAlert)
                    {
                        soundPlayer.Play();
                    }
                }
            }
            //SubHeader2Text.Text = string.Format(
            //    ResourceManager.GetString("COMBAT_TEXT_DURABILITY"),
            //    _update.Sector.Name);

            //int _otherFirePower = 0;



            //if (CivFirePowers2 != 0) _otherFirePower += CivFirePowers2;
            //if (update.CivFirePowers3 != 0) _otherFirePower += update.CivFirePowers3;
            //if (update.CivFirePowers4 != 0) _otherFirePower += update.CivFirePowers4;

            //update.GetCivFirePowers
            _text_combatWindow =  GameEngine.LocationString(update.Location.ToString()) + " Red Alert > "
                //+ " > our Fire_power_calculated: " + update.CivFirePowers1
                //+ " vs " + update.CivFirePowers2
                //+ " + " + update.CivFirePowers3
                //+ " + " + update.CivFirePowers4

                //+ _text_combatWindow 
                //+ _newline
                ;

            //CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
            playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text_combatWindow, "", "", SitRepPriority.Red));

            _text_combatWindow = "Step_3456:; CombatID= " + update.CombatID + ": " + _text_combatWindow;
            Console.WriteLine(_text_combatWindow);
            //_combat_full_Report += _text_combatWindow + _newline;

            PopulateUnitTrees();

            //We need combat assets to be able to engage
            EngageButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.Station != null));
            //We need combat assets to be able to rush the opposition
            RushButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count > 0);
            //There needs to be transports in the opposition to be able to target them
            TransportsButton.IsEnabled = _update.HostileAssets.Any(h => h.IsTransport);
            //We need at least 3 ships to create a formation
            FormationButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count >= 3);
            //We need assets to be able to retreat
            RetreatButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count > 0 || fa.NonCombatShips.Count > 0); // && fa.Owner != fa.Sector.Station.Owner);
            //Can hail

            HailButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count > 0 || fa.NonCombatShips.Count > 0 || fa.Station != null); //(update.RoundNumber == 1);

            UpperButtonsPanel.Visibility = update.CombatUpdate_IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            LowerButtonsPanel.Visibility = update.CombatUpdate_IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            CloseButton.Visibility = update.CombatUpdate_IsCombatOver ? Visibility.Visible : Visibility.Collapsed;
            UpperButtonsPanel.IsEnabled = true;
            LowerButtonsPanel.IsEnabled = true;



            if (!IsVisible && update.RoundNumber == 1)
            {
                _ = Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NullableBoolFunction(ShowDialog));
            }


        }

        //private string CreateShipText(CombatUnit _ship, out string _shipText)
        //{
        //    _shipText = Environment.NewLine +
        //            "ICH: " + _ship.HullIntegrity
        //            + ", S: " + _ship.ShieldIntegrity
        //            //+ " f." + _ship.Owner.ShortName
        //            + " Ship " + GameEngine.Do_x_Digit_String(5, _ship.Source.ObjectID.ToString())
        //            + " - " + _ship.Source.OrbitalDesign.Key
        //            + " - " + _ship.Source.Name;
        //    return _shipText;
        //}

        private void ClearUnitTrees()
        {

            FriendlyStationItem.Header = null;
            FriendlyCombatantItems.Items.Clear();
            FriendlyNonCombatantItems.Items.Clear();
            FriendlyDestroyedItems.Items.Clear();
            FriendlyAssimilatedItems.Items.Clear();
            FriendlyEscapedItems.Items.Clear();
            HostileStationItem.Header = null;
            HostileCombatantItems.Items.Clear();
            HostileNonCombatantItems.Items.Clear();
            HostileDestroyedItems.Items.Clear();
            HostileAssimilatedItems.Items.Clear();
            HostileEscapedItems.Items.Clear();
            OtherCivilizationsSummaryItem1.Items.Clear();
            FriendCivilizationsItems.Items.Clear();

            //GameLog.Core.CombatDetails.DebugFormat("cleared all ClearUnitTrees");

        }

        private void PopulateUnitTrees()
        {
            ClearUnitTrees();
            foreach (CombatAssets friendlyAssets in _update.FriendlyAssets)
            {

                List<Civilization> shootingPlayerCivs = new List<Civilization>();

                if (friendlyAssets.Station != null)
                {
                    FriendlyStationItem.Header = friendlyAssets.Station;
                    shootingPlayerCivs.Add(friendlyAssets.Station.Owner);

                }
                if (friendlyAssets.CombatShips != null)
                {
                    foreach (CombatUnit shipStats in friendlyAssets.CombatShips)
                    {
                        _ = FriendlyCombatantItems.Items.Add(shipStats);
                        shootingPlayerCivs.Add(shipStats.Owner);

                    }
                }
                if (friendlyAssets.NonCombatShips != null)
                {
                    foreach (CombatUnit shipStats in friendlyAssets.NonCombatShips)
                    {
                        _ = FriendlyNonCombatantItems.Items.Add(shipStats);
                        shootingPlayerCivs.Add(shipStats.Owner);

                    }
                }
                foreach (CombatUnit shipStats in friendlyAssets.DestroyedShips)
                {
                    _ = FriendlyDestroyedItems.Items.Add(shipStats);

                }

                foreach (CombatUnit shipStats in friendlyAssets.AssimilatedShips)
                {
                    _ = FriendlyAssimilatedItems.Items.Add(shipStats);
                }

                foreach (CombatUnit shipStats in friendlyAssets.EscapedShips)
                {
                    _ = FriendlyEscapedItems.Items.Add(shipStats);

                }

                shootingPlayerCivs = shootingPlayerCivs.Distinct().ToList();
                _friendlyCivs = shootingPlayerCivs;
                ;
                foreach (Civilization Friend in _friendlyCivs)
                {
                    _ = FriendCivilizationsItems.Items.Add(Friend); // a template for rach other civ
                    GameLog.Core.Combat.DebugFormat("_friendlyCivs containing = {0}", Friend.ShortName);
                }

            }

            /* Hostile (others) Assets */
            foreach (CombatAssets hostileAssets in _update.HostileAssets)
            {

                List<Civilization> otherCivs = new List<Civilization>();

                if (hostileAssets.Station != null)
                {
                    HostileStationItem.Header = hostileAssets.Station;
                    otherCivs.Add(hostileAssets.Station.Owner);

                }
                foreach (CombatUnit shipStats in hostileAssets.CombatShips)
                {
                    _ = HostileCombatantItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.NonCombatShips)
                {
                    _ = HostileNonCombatantItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.EscapedShips)
                {
                    _ = HostileEscapedItems.Items.Add(shipStats);

                }
                foreach (CombatUnit shipStats in hostileAssets.DestroyedShips)
                {
                    _ = HostileDestroyedItems.Items.Add(shipStats);

                }
                foreach (CombatUnit shipStats in hostileAssets.AssimilatedShips)
                {
                    _ = HostileAssimilatedItems.Items.Add(shipStats);
                }
                _otherCivs = otherCivs.Distinct().ToList(); // adding Civilizations of the others into the field (a list) _otherCivs

                foreach (Civilization Other in _otherCivs)
                {
                    _ = OtherCivilizationsSummaryItem1.Items.Add(Other); // a template for rach other civ
                    GameLog.Core.Combat.DebugFormat("_otherCivs containing = {0}", Other.ShortName);

                }

            }
            _ = OtherCivilizationsSummaryItem1.Items.Add(_onlyFireIfFiredApp_1);
            ShowHideUnitTrees();
        }

        private void ShowHideUnitTrees()
        {
            FriendlyStationItem.Visibility = FriendlyStationItem.HasHeader ? Visibility.Visible : Visibility.Collapsed;
            FriendlyCombatantItems.Header = FriendlyCombatantItems.HasItems ? ResourceManager.GetString("COMBAT_COMBATANT_UNITS") : null;
            FriendlyCombatantItems.Visibility = FriendlyCombatantItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendlyNonCombatantItems.Header = FriendlyNonCombatantItems.HasItems ? ResourceManager.GetString("COMBAT_NON-COMBATANT_UNITS") : null;
            FriendlyNonCombatantItems.Visibility = FriendlyNonCombatantItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendlyDestroyedItems.Header = FriendlyDestroyedItems.HasItems ? ResourceManager.GetString("COMBAT_DESTROYED_UNITS") : null;
            FriendlyDestroyedItems.Visibility = FriendlyDestroyedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendlyAssimilatedItems.Header = FriendlyAssimilatedItems.HasItems ? ResourceManager.GetString("COMBAT_ASSIMILATED_UNITS") : null;
            FriendlyAssimilatedItems.Visibility = FriendlyAssimilatedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendlyEscapedItems.Header = FriendlyEscapedItems.HasItems ? ResourceManager.GetString("COMBAT_ESCAPED_UNITS") : null;
            FriendlyEscapedItems.Visibility = FriendlyEscapedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            HostileStationItem.Visibility = HostileStationItem.HasHeader ? Visibility.Visible : Visibility.Collapsed;
            HostileCombatantItems.Header = HostileCombatantItems.HasItems ? ResourceManager.GetString("COMBAT_COMBATANT_UNITS") : null;
            HostileCombatantItems.Visibility = HostileCombatantItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            HostileNonCombatantItems.Header = HostileNonCombatantItems.HasItems ? ResourceManager.GetString("COMBAT_NON-COMBATANT_UNITS") : null;
            HostileNonCombatantItems.Visibility = HostileNonCombatantItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            HostileDestroyedItems.Header = HostileDestroyedItems.HasItems ? ResourceManager.GetString("COMBAT_DESTROYED_UNITS") : null;
            HostileDestroyedItems.Visibility = HostileDestroyedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            HostileAssimilatedItems.Header = HostileAssimilatedItems.HasItems ? ResourceManager.GetString("COMBAT_ASSIMILATED_UNITS") : null;
            HostileAssimilatedItems.Visibility = HostileAssimilatedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
            HostileEscapedItems.Header = HostileEscapedItems.HasItems ? ResourceManager.GetString("COMBAT_ESCAPED_UNITS") : null;
            HostileEscapedItems.Visibility = HostileEscapedItems.HasItems ? Visibility.Visible : Visibility.Collapsed;

            OtherCivilizationsSummaryItem1.Visibility = OtherCivilizationsSummaryItem1.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendCivilizationsItems.Visibility = FriendCivilizationsItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
        }
        private void TargetButton1_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton1 = (RadioButton)sender;
            _targeted_civ_1 = (Civilization)radioButton1.DataContext;
            if (_targeted_civ_1.ShortName == "Only Return Fire" && _targeted_civ_2.ShortName == "Only Return Fire")
            {
                EngageButton.IsEnabled = false;
                RushButton.IsEnabled = false;
                TransportsButton.IsEnabled = false;

                //radioButton1.i = true;  // pres-set the first button to FIRE
            }
            else
            {
                EngageButton.IsEnabled = true;
                RushButton.IsEnabled = true;
                TransportsButton.IsEnabled = true;
            }
            TransportsButton.IsEnabled = _update.HostileAssets.Any(ha => ha.CombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _targeted_civ_1) || (ncs.Owner == _targeted_civ_2))))
                || _update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _targeted_civ_1) || (ncs.Owner == _targeted_civ_2))));

            //GameLog.Core.CombatDetails.DebugFormat("Secondary Target is set to theTargetCiv = {0}", _targeted_civ_2.ShortName);
            string _text_combatWindow = "Step_5486:; Primary Target is set to .. > " + _targeted_civ_1.ShortName;
            Console.WriteLine(_text_combatWindow);
            //GameLog.Core.CombatDetails.DebugFormat(_text_combatWindow); //theTargeted1Civ);

        }

        private void TargetButton2_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton2 = (RadioButton)sender;
            _targeted_civ_2 = (Civilization)radioButton2.DataContext;
            if (_targeted_civ_1.ShortName == "Only Return Fire" && _targeted_civ_2.ShortName == "Only Return Fire")
            {
                EngageButton.IsEnabled = false;
                RushButton.IsEnabled = false;
                TransportsButton.IsEnabled = false;
            }
            else
            {
                EngageButton.IsEnabled = true;
                RushButton.IsEnabled = true;
                TransportsButton.IsEnabled = true;
            }
            TransportsButton.IsEnabled = _update.HostileAssets.Any(ha => ha.CombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _targeted_civ_1) || (ncs.Owner == _targeted_civ_2))))
               || _update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _targeted_civ_1) || (ncs.Owner == _targeted_civ_2))));

            string _text_combatWindow = "Step_5487:; Secondary Target is set to .. > " + _targeted_civ_2.ShortName;
            Console.WriteLine(_text_combatWindow);
            //GameLog.Core.CombatDetails.DebugFormat(_text_combatWindow);
            //GameLog.Core.CombatDetails.DebugFormat("Secondary Target is set to theTargetCiv = {0}", _targeted_civ_2.ShortName);
        }

        private void OnOrderButtonClicked(object sender, RoutedEventArgs e)
        {
            CombatOrder order = CombatOrder.Retreat;
            if (sender == EngageButton)
            {
                order = CombatOrder.Engage;
            }

            if (sender == TransportsButton)
            {
                order = CombatOrder.Transports;
            }

            if (sender == FormationButton)
            {
                order = CombatOrder.Formation;
            }

            if (sender == RushButton)
            {
                order = CombatOrder.Rush;
            }

            if (sender == HailButton)
            {
                order = CombatOrder.Hail;
                //DialogResult = true;
                //Close();
            }

            if (sender == EscapeButton)
            {
                order = CombatOrder.Retreat;
                //DialogResult = true;
                //Close();
            }

            string _text_combatWindow = /*"###########################" +*/
                GameEngine.LocationString(_playerAssets.Location.ToString()) + " > Red Alert at " + _playerAssets.Sector
                + " > Target 1: " + _targeted_civ_1.Name + ", 2: " + _targeted_civ_2.Name
                + " > Player's choice: " + order /*+ " button was clicked by player "*/
                ;

            CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
            playerCivManager.SitRepEntries.Add(new ReportEntry_NoAction(playerCivManager.Civilization
                , _text_combatWindow, "", "", SitRepPriority.Yellow));

            _text_combatWindow = "Step_5389:; " + _text_combatWindow;
            Console.WriteLine(_text_combatWindow);
            //GameLog.Client.Combat.DebugFormat(_text_combatWindow_combatWindow);


            UpperButtonsPanel.IsEnabled = false;
            LowerButtonsPanel.IsEnabled = false;
            // send targets before order - order updates
            ClientCommands.SendCombatTarget1.Execute(CombatHelper.GenerateBlanketTargetPrimary(_playerAssets, _targeted_civ_1));
            ClientCommands.SendCombatTarget2.Execute(CombatHelper.GenerateBlanketTargetSecondary(_playerAssets, _targeted_civ_2));
            ClientCommands.SendCombatOrders.Execute(CombatHelper.GenerateBlanketOrders(_playerAssets, order));

            DialogResult = true;
            Close();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            //_combatWindowVisible = false;
        }

        //private void OnEscapeButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = true;
        //    this.Close();
        //}

    }
}

