// CombatWindow.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
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

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for CombatWindow.xaml
    /// </summary>

    public partial class CombatWindow
    {
        private CombatUpdate _update;
        private CombatAssets _playerAssets;
        private List<String> _otherCivsKeys;
        private List<Civilization> _otherCivs;
        private Civilization _primeTargetOftheCivilzation; // primary player-selected civ to attack
        private Civilization _secondTargetOftheCivilzation; // secondary player-selected civ to attack
        private Dictionary<string, CombatUnit> _ourFiendlyCombatUnits; // do I need combat unit or combat assets here?
        private Dictionary<string, CombatUnit> _othersCombatUnits;
        //protected int _friendlyEmpireStrength;
        //public string _friendlyEmpireStrengthString = "444";
        private IAppContext _appContext;

        public List<Civilization> OtherCivs
        {
            get { return _otherCivs; }
            set { _otherCivs = value; }
        }

        public List<string> OtherCivsKeys
        {
            get { return _otherCivsKeys; }
            set { _otherCivsKeys = value; }
        }

        //public string FriendlyEmpireStrengthString
        //{
        //    get
        //    {
        //        GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);

        //        return "100";
        //        //return _friendlyEmpireStrengthString;
        //        //return _friendlyEmpireStrength;
        //    }

        //}

        //public int FriendlyEmpireStrength
        //{
        //    get
        //    {
        //        GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);
        //        return 100;
        //        //return _friendlyEmpireStrength;
        //    }
        //    set
        //    {
        //        _friendlyEmpireStrength = 123;
        //        //_friendlyEmpireStrength = value;
        //    }
        //}

        public CombatWindow()
        {
            InitializeComponent();
            DataContext = this;
            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            ClientEvents.CombatUpdateReceived.Subscribe(OnCombatUpdateReceived, ThreadOption.UIThread);
            DataTemplate itemTemplate = TryFindResource("FriendTreeItemTemplate") as DataTemplate;

            FriendlyStationItem.HeaderTemplate = itemTemplate;
            FriendlyCombatantItems.ItemTemplate = itemTemplate;
            FriendlyNonCombatantItems.ItemTemplate = itemTemplate;
            FriendlyDestroyedItems.ItemTemplate = itemTemplate;
            FriendlyAssimilatedItems.ItemTemplate = itemTemplate;
            FriendlyEscapedItems.ItemTemplate = itemTemplate;

            DataTemplate civHeaderTemplate = TryFindResource("OthersTreeSummaryTemplate") as DataTemplate;
            // the other civilizations summary
       
            OtherCivilizationsSummaryItem.ItemTemplate = civHeaderTemplate;

            DataTemplate civItemTemplate = TryFindResource("OthersTreeItemTemplate") as DataTemplate;
            // the items in each civilization of the others

            HostileStationItem.HeaderTemplate = civItemTemplate;
            HostileCombatantItems.ItemTemplate = civItemTemplate;
            HostileNonCombatantItems.ItemTemplate = civItemTemplate;
            HostileDestroyedItems.ItemTemplate = civItemTemplate;
            HostileAssimilatedItems.ItemTemplate = civItemTemplate;
            HostileEscapedItems.ItemTemplate = civItemTemplate;

            //DataTemplate civDropDownTemplate = TryFindResource("CivDropDownTemplate") as DataTemplate;

            //OtherCivilizationsHeaderDropDown.HeaderTemplate = civDropDownTemplate;
            //OtherCivilizationsDropDown.ItemTemplate = civDropDownTemplate;
            ComboBoxItem civDropDownTemplate = TryFindResource("CivTreeDropDownTemplate") as ComboBoxItem;
            PrimaryTargetDropDown = civDropDownTemplate;
            SecondaryTargetDropDown = civDropDownTemplate;

            OtherCivsKeys = _otherCivsKeys;
            
            //FriendlyEmpireStrength = _friendlyEmpireStrength;

        }

        private void OnCombatUpdateReceived(DataEventArgs<CombatUpdate> args)
        {
            HandleCombatUpdate(args.Value);
        }

        private void HandleCombatUpdate(CombatUpdate update)
        {
            _update = update;
            foreach (CombatAssets assets in update.FriendlyAssets)
            {
                if (assets.Owner == _appContext.LocalPlayer.Empire)
                {
                    _playerAssets = assets;
                    break;
                }
            }
            if (_playerAssets == null)
            {
                _playerAssets = update.FriendlyAssets[0];
            }

            DataContext = _update;

            if (update.IsCombatOver)
            {
                //if (update.RoundNumber == 6) // add fitting text, for when the battle is over Roundnumber must be 1 more then in the forced reatreat/combat ending.
                //{
                //    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + ": "
                //        + String.Format(ResourceManager.GetString("COMBAT_STANDOFF"));
                //    SubHeaderText.Text = String.Format(
                //        ResourceManager.GetString("COMBAT_TEXT_LONG_BATTLE_OVER"),
                //        _update.Sector.Name);
                //} 


                if (_update.IsStandoff)
                {
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + ": "
                        + String.Format(ResourceManager.GetString("COMBAT_STANDOFF"));
                    SubHeaderText.Text = String.Format(
                        ResourceManager.GetString("COMBAT_TEXT_STANDOFF"),
                        _update.Sector.Name);
                }
                else if (_playerAssets.HasSurvivingAssets)
                {
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + ": "
                        + String.Format(ResourceManager.GetString("COMBAT_VICTORY"));
                    SubHeaderText.Text = String.Format(
                        ResourceManager.GetString("COMBAT_TEXT_VICTORY"),
                        _update.Sector.Name);
                }
                else
                {
                    HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + ": "
                        + String.Format(ResourceManager.GetString("COMBAT_DEFEAT"));
                    SubHeaderText.Text = String.Format(
                        ResourceManager.GetString("COMBAT_TEXT_DEFEAT"),
                        _update.Sector.Name);
                }
            }
            else
            {
                HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER"); // + ": "
                                                                              //+ String.Format(ResourceManager.GetString("COMBAT_ROUND"), _update.RoundNumber);
                SubHeaderText.Text = String.Format(
                    ResourceManager.GetString("COMBAT_TEXT_ENCOUNTER"),
                    _update.Sector.Name);
                var soundPlayer = new SoundPlayer("Resources/SoundFX/REDALERT.wav");
                {
                    if (File.Exists("Resources/SoundFX/REDALERT.wav"))
                        soundPlayer.Play();
                }
            }

            //_friendlyEmpireStrength = 100;

            //if (_playerAssets.Owner.IsEmpire && _playerAssets.Owner.Key == "Mike")
            //{
            //    _friendlyEmpireStrength = _update.EmpireStrengths[_appContext.LocalPlayer.Empire.Key.ToString()];
            //    GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);
            //}

            //string FriendlyEmpireStrengthString = _friendlyEmpireStrength.ToString();

            PopulateUnitTrees();

            //var path = civEmblem.Source.ToString();

            //We need combat assets to be able to engage
            EngageButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.Station != null));
            //We need combat assets to be able to rush the opposition
            RushButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count > 0);
            //There needs to be transports in the opposition to be able to target them
            TransportsButton.IsEnabled = (_update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Transport"))) ||
                (_update.HostileAssets.Any(ha => ha.CombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Transport"))); // klingon transports are combat ships
            //We need at least 3 ships to create a formation
            FormationButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count >= 3);
            //We need assets to be able to retreat
            RetreatButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.NonCombatShips.Count > 0) || (fa.Station != null));
            //Can hail
            HailButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.Station != null)); //(update.RoundNumber == 1);

            UpperButtonsPanel.Visibility = update.IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            LowerButtonsPanel.Visibility = update.IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            CloseButton.Visibility = update.IsCombatOver ? Visibility.Visible : Visibility.Collapsed;
            UpperButtonsPanel.IsEnabled = true;
            LowerButtonsPanel.IsEnabled = true;

            if (!IsVisible)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NullableBoolFunction(ShowDialog));
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
            //OtherCivilizationsHeaderDropDown.Header = null;
            //OtherCivilizationsDropDown.Clear();
            OtherCivilizationsSummaryItem.Items.Clear();

        }

        private void PopulateUnitTrees()
        {
            ClearUnitTrees();


            foreach (CombatAssets friendlyAssets in _update.FriendlyAssets)
            {
                int friendlyAssetsFirepower = 0;
                // string friendCiv; 
                if (friendlyAssets.Station != null)
                {
                    FriendlyStationItem.Header = friendlyAssets.Station;
                    //friendCiv = friendlyAssets.Station.Owner.Key;
                    friendlyAssetsFirepower += friendlyAssets.Station.FirePower;
                }

                foreach (CombatUnit shipStats in friendlyAssets.CombatShips)
                {
                    FriendlyCombatantItems.Items.Add(shipStats);
                    //friendCiv = friendlyAssets.Owner.Key;
                    friendlyAssetsFirepower += shipStats.FirePower;
                }

                foreach (CombatUnit shipStats in friendlyAssets.NonCombatShips)
                {
                    FriendlyNonCombatantItems.Items.Add(shipStats);
                    //friendCiv = friendlyAssets.Owner.Key;
                    friendlyAssetsFirepower += shipStats.FirePower;
                }

                foreach (CombatUnit shipStats in friendlyAssets.DestroyedShips)
                {
                    FriendlyDestroyedItems.Items.Add(shipStats);
                }

                foreach (CombatUnit shipStats in friendlyAssets.AssimilatedShips)
                {
                    FriendlyAssimilatedItems.Items.Add(shipStats);
                }

                foreach (CombatUnit shipStats in friendlyAssets.EscapedShips)
                {
                    FriendlyEscapedItems.Items.Add(shipStats);
                }
                //_friendlyEmpireStrength = friendlyAssetsFirepower;
            }

            /* Hostile (others) Assets */
            foreach (CombatAssets hostileAssets in _update.HostileAssets)
            {
                var otherCivs = new List<Civilization>();
                var otherCivKeys = new List<string>();
                if (hostileAssets.Station != null)
                {
                    HostileStationItem.Header = hostileAssets.Station;
                    otherCivs.Add(hostileAssets.Station.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.CombatShips)
                {
                    HostileCombatantItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.NonCombatShips)
                {
                    HostileNonCombatantItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.EscapedShips)
                {
                    HostileEscapedItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.DestroyedShips)
                {
                    HostileDestroyedItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (CombatUnit shipStats in hostileAssets.AssimilatedShips)
                {
                    HostileAssimilatedItems.Items.Add(shipStats);
                    otherCivs.Add(shipStats.Owner);
                }
                foreach (Civilization civ in otherCivs)
                {
                    otherCivKeys.Add("TRUMPS");
                    otherCivKeys.Add(civ.Key);
                    otherCivKeys.Add("CLINTONS");
                }

                _otherCivsKeys = otherCivKeys.Distinct().ToList();
                OtherCivsKeys = _otherCivsKeys;
                _otherCivs = otherCivs;
                OtherCivs = _otherCivs;

                foreach (string Other in _otherCivsKeys)
                {
                    OtherCivilizationsSummaryItem.Items.Add(Other);

                    //OtherCivilizationsDropDown.Add(Other);
                    //OtherCivilizationsHeaderDropDown.Header.Add(Other);
                }
            }

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
  
           OtherCivilizationsSummaryItem.Visibility = OtherCivilizationsSummaryItem.HasItems ? Visibility.Visible : Visibility.Collapsed;
               //OtherCivilizationsHeaderDropDown.Visibility = OtherCivilizationsHeaderDropDown.HasHeader ? Visibility.Visible : Visibility.Collapsed;
                //OtherCivilizationsDropDown.Header = OtherCivilizationsDropDown.HasItems ? ResourceManager.GetString("COMBAT_CIVILIZATIONS") : null;
                //OtherCivilizationsDropDown.Visibility = OtherCivilizationsDropDown.HasItems ? Visibility.Visible : Visibility.Collapsed;
         
        }

        private void OnOrderButtonClicked(object sender, RoutedEventArgs e)
        {
            CombatOrder order = CombatOrder.Retreat;
            if (sender == EngageButton)
                order = CombatOrder.Engage;
            if (sender == TransportsButton)
                order = CombatOrder.Transports;
            if (sender == FormationButton)
                order = CombatOrder.Formation;
            if (sender == RushButton)
                order = CombatOrder.Rush;
            if (sender == HailButton)
                order = CombatOrder.Hail;

            GameLog.Client.General.DebugFormat("{0} button clicked by player", order);

            UpperButtonsPanel.IsEnabled = false;
            LowerButtonsPanel.IsEnabled = false;
            ClientCommands.SendCombatOrders.Execute(CombatHelper.GenerateBlanketOrders(_playerAssets, order));
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            //base.DialogResult = true;
            DialogResult = true;
        }
    }
}

