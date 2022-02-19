// File:CombatWindow.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Commands;
using Supremacy.Client.Events;
using Supremacy.Combat;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Client.Context;
using System.Media;
using System.IO;
using Supremacy.Utility;
using System.Linq;
using System.Collections.Generic;
using Supremacy.Entities;
using Supremacy.Game;
using System;

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
        private readonly Civilization _onlyFireIfFiredAppone;
        private Civilization _theTargeted1Civ;
        private Civilization _theTargeted2Civ;
        private string _text;
        private readonly IAppContext _appContext;


        public CombatWindow()
        {
            InitializeComponent();

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

            DataTemplate civTemplate = TryFindResource("OthersTreeSummaryTemplate") as DataTemplate;
            // other civilizations summary for targeting
            OtherCivilizationsSummaryItem1.ItemTemplate = civTemplate;

            OtherCivilizationsSummaryItem1.DataContext = _otherCivs; // ListBox data context set to OtherCivs

            _onlyFireIfFiredAppone = new Civilization
            {
                //_onlyFireIfFiredAppone.ShortName = "Only Return Fire";
                ShortName = ResourceManager.GetString("ONLY_RETURN_FIRE"),
                CivID = 888,
                Key = "Only Return Fire"
            };
            // The click of "Only Return Fire" radio button by human player
            // _theTargeted1Civ = new Civilization();
            _theTargeted1Civ = _onlyFireIfFiredAppone;
            // _theTargeted2Civ = new Civilization();
            _theTargeted2Civ = _onlyFireIfFiredAppone;

        }

        private void OnCombatUpdateReceived(DataEventArgs<CombatUpdate> args)
        {
            HandleCombatUpdate(args.Value);
        }

        private void HandleCombatUpdate(CombatUpdate update)
        {
            _update = update;
            string _text = _update.CombatID + ": " + "Combat at " + _update.Location
                + " > " + _update.FriendlyAssets.Count() + " on our side - "
                + _update.HostileAssets.Count() + " hostile "
                ;

            List<CivilizationManager> _civs = new List<CivilizationManager>();


            foreach (CombatAssets assets in update.FriendlyAssets)
            {
                if (assets.Owner == _appContext.LocalPlayer.Empire)
                {
                    _playerAssets = assets;
                    _playerAssets.CombatID = _update.CombatID;
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


            DataContext = _update;

            if (update.CombatUpdate_IsCombatOver)
            {

                if (_update.IsStandoff)
                {
                    _text = string.Format(ResourceManager.GetString("COMBAT_STANDOFF"));
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + _text;
                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_STANDOFF"),
                        _update.Sector.Name);
                    _text += _text + " - no winner";

                    CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text, "", "", SitRepPriority.Red));

                    //playerCivManager.SitRepEntries.Add(new CombatSummarySitRepEntry(playerCivManager.Civilization, _update.Sector.Location,
                    //    string.Format(ResourceManager.GetString("COMBAT_TEXT_STANDOFF"), _update.Sector.Name)));

                }
                else if (_playerAssets.HasSurvivingAssets)
                {
                    _text = string.Format(ResourceManager.GetString("COMBAT_VICTORY"));
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + string.Format(ResourceManager.GetString("COMBAT_VICTORY"));
                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_VICTORY"),
                        _update.Sector.Name);
                    _text += _text + " - we were victorious !";

                    CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text, "", "", SitRepPriority.Red));
                }
                else
                {
                    _text = string.Format(ResourceManager.GetString("COMBAT_DEFEAT"));
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + " >>  "
                        + string.Format(ResourceManager.GetString("COMBAT_DEFEAT"));
                    SubHeaderText.Text = string.Format(
                        ResourceManager.GetString("COMBAT_TEXT_DEFEAT"),
                        _update.Sector.Name);
                    _text += _text + " - we were not victorious !";

                    CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
                    playerCivManager.SitRepEntries.Add(new ReportEntry_CoS(playerCivManager.Civilization, _update.Sector.Location, _text, "", "", SitRepPriority.Red));
                }
            }
            else
            {
                HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER"); // + ": "
                                                                              //+ String.Format(ResourceManager.GetString("COMBAT_ROUND"), _update.RoundNumber);
                SubHeaderText.Text = string.Format(
                    ResourceManager.GetString("COMBAT_TEXT_ENCOUNTER"),
                    _update.Sector.Name);
                SoundPlayer soundPlayer = new SoundPlayer("Resources/SoundFX/REDALERT.wav");
                {
                    if (File.Exists("Resources/SoundFX/REDALERT.wav"))
                    {
                        soundPlayer.Play();
                    }
                }
            }
            SubHeader2Text.Text = string.Format(
                ResourceManager.GetString("COMBAT_TEXT_DURABILITY"),
                _update.Sector.Name);

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

            if (!IsVisible)
            {
                _ = Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NullableBoolFunction(ShowDialog));
            }
        }

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

            GameLog.Core.CombatDetails.DebugFormat("cleared all ClearUnitTrees");

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
            _ = OtherCivilizationsSummaryItem1.Items.Add(_onlyFireIfFiredAppone);
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
            _theTargeted1Civ = (Civilization)radioButton1.DataContext;
            if (_theTargeted1Civ.ShortName == "Only Return Fire" && _theTargeted2Civ.ShortName == "Only Return Fire")
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
            TransportsButton.IsEnabled = _update.HostileAssets.Any(ha => ha.CombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _theTargeted1Civ) || (ncs.Owner == _theTargeted2Civ))))
                || _update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _theTargeted1Civ) || (ncs.Owner == _theTargeted2Civ))));

            //GameLog.Core.CombatDetails.DebugFormat("Secondary Target is set to theTargetCiv = {0}", _theTargeted2Civ.ShortName);
            GameLog.Core.CombatDetails.DebugFormat("Primary Target is set to theTargetCiv = {0}", _theTargeted1Civ.ShortName); //theTargeted1Civ);

        }

        private void TargetButton2_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton2 = (RadioButton)sender;
            _theTargeted2Civ = (Civilization)radioButton2.DataContext;
            if (_theTargeted1Civ.ShortName == "Only Return Fire" && _theTargeted2Civ.ShortName == "Only Return Fire")
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
            TransportsButton.IsEnabled = _update.HostileAssets.Any(ha => ha.CombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _theTargeted1Civ) || (ncs.Owner == _theTargeted2Civ))))
               || _update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => (ncs.Source.OrbitalDesign.ShipType == "Transport") && ((ncs.Owner == _theTargeted1Civ) || (ncs.Owner == _theTargeted2Civ))));

            GameLog.Core.CombatDetails.DebugFormat("Secondary Target is set to theTargetCiv = {0}", _theTargeted2Civ.ShortName);
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
            }

            if (sender == EscapeButton)
            {
                order = CombatOrder.Retreat;
                DialogResult = true;
                Close();
            }

            _text = _playerAssets.Location + " > Combat at " + _playerAssets.Sector + " > " + order + " button was clicked by player";
            Console.WriteLine(_text);
            GameLog.Client.Combat.DebugFormat(_text);

            CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_appContext.LocalPlayer.CivID];
            playerCivManager.SitRepEntries.Add(new Report_NoAction(playerCivManager.Civilization, _text, "", "", SitRepPriority.Red));


            UpperButtonsPanel.IsEnabled = false;
            LowerButtonsPanel.IsEnabled = false;
            // send targets before order - order updates
            ClientCommands.SendCombatTarget1.Execute(CombatHelper.GenerateBlanketTargetPrimary(_playerAssets, _theTargeted1Civ));
            ClientCommands.SendCombatTarget2.Execute(CombatHelper.GenerateBlanketTargetSecondary(_playerAssets, _theTargeted2Civ));
            ClientCommands.SendCombatOrders.Execute(CombatHelper.GenerateBlanketOrders(_playerAssets, order));
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            //_combatWindowVisible = false;
        }

        //private void OnEscapeButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = true;
        //    this.Close();
        //}

    }
}

