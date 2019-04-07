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
using Supremacy.Universe;
using System.ComponentModel;
using System.Threading;
using Supremacy.Game;
using System.Collections.ObjectModel;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for CombatWindow.xaml
    /// </summary>

    public partial class CombatWindow
    {
        private CombatUpdate _update;
        private CombatAssets _playerAssets;
        private List<Civilization> _otherCivs; // this collection populates UI with 'other' civilizations found in the sector
        private List<Civilization> _friendlyCivs; // players civ and fight along side civs if any, can this replace _shooterCivilizations1 and 2?
        //private bool _playerIsShooting = true; // do we need this? figure this out from whoIsShootingWhom...
        private List<Civilization> _shooterCivilizations1; // players civ and fight along side civs for Prime targets
        private List<Civilization> _shooterCivilizations2; // players civ and fight along side civs for Secondary targets
        private Civilization _targetCivilzation1; // player-selected civ to attack 
        private Civilization _targetCivilzation2; // secondary player-selected civ to attack
        private Dictionary<Civilization, Civilization> _whoIsShootingWhomFirst;
        private Dictionary<Civilization, Civilization> _whoIsShootingWhomSecond;
        //protected int _friendlyEmpireStrength;
        //public string _friendlyEmpireStrengthString = "444";
        private IAppContext _appContext;
        //private readonly object choiceTextBlock;

        //private object choiceTextBloack;

        #region Constructors
        //public CombatWindow(bool playerIsShooting)
        //{
        //    _playerIsShooting = playerIsShooting;
        //}
        // how to do a constuctor for a field that is a collection like _whoIsShootingWhomFirst
        #endregion

        #region Properties
        public Dictionary<Civilization, Civilization> WhoIsShootingWhomFirst // WhoIsShootingWhom[Civilization] returns the target Civilization try catch(KeyNotFoundException)
        {
            get
            {
                return _whoIsShootingWhomFirst;
            }
            set
            {
                _whoIsShootingWhomFirst = value;
            }
        }

        public Dictionary<Civilization, Civilization> WhoIsShootingWhomSecond // WhoIsShootingWhom[Civilization] returns the target Civilization try catch(KeyNotFoundException)
        {

            get
            {

                return _whoIsShootingWhomSecond;
            }
            set
            {
                _whoIsShootingWhomSecond = value;
            }
        }

        public List<Civilization> OtherCivs
        {
            get
            {
                //null ref crash GameLog.Core.Combat.DebugFormat("OtherCivs - GET: _otherCivs = {0}", _otherCivs.ToString());
                return _otherCivs;
            }
            set
            {
                //null ref crash GameLog.Core.Combat.DebugFormat("OtherCivs - SET: _otherCivs = {0}", value.ToString());
                _otherCivs = value;
            }
        }

        public List<Civilization> FriendlyCivs // Do we really need this as a Property?
        {
            get
            {
                Civilization dummy = new Civilization();
                dummy.Race = DUMMIES;
                dummy.ShortName = "Dummy";
                _friendlyCivs.Add(dummy);
                return _friendlyCivs;
            }
            set
            {
                _friendlyCivs = value;
            }
        }

        public Civilization TargetCivilization1  // does this need to be a public property? keep it private as the field?
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("TargetCivilization - GET: _otherCivsKeys = {0}", _targetCivilzation1.ToString());
                return _targetCivilzation1;
            }
            set
            {

                _targetCivilzation1 = value;
                GameLog.Core.Combat.DebugFormat("TargetCivilization - SET: _otherCivsKeys = {0}", _targetCivilzation1.ToString());
            }
        }

        public Civilization TargetCivilization2 // does this need to be a public property? keep it private as the field?
        {
            get
            {
                //null ref crash GameLog.Core.Combat.DebugFormat("SecondaryTargetCivilization - GET: _otherCivsKeys = {0}", _secondTargetCivilzation.ToString());
                return _targetCivilzation2;
            }
            set
            {
                _targetCivilzation2 = value;
                //null ref crash GameLog.Core.Combat.DebugFormat("SecondaryTargetCivilization - GET: _otherCivsKeys = {0}", _secondTargetCivilzation.ToString());
            }
        }

        public Race ROMULANS { get; private set; }
        public Race DUMMIES { get; private set; }
        #endregion

        public CombatWindow()
        {
            InitializeComponent();

            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            ClientEvents.CombatUpdateReceived.Subscribe(OnCombatUpdateReceived, ThreadOption.UIThread);
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
            // the other civilizations summary
            FriendCivilizationsItems.ItemTemplate = civFriendTemplate;

            FriendCivilizationsItems.DataContext = _friendlyCivs;

            DataTemplate civTemplate = TryFindResource("OthersTreeSummaryTemplate") as DataTemplate;
            // the other civilizations summary
            OtherCivilizationsSummaryItem1.ItemTemplate = civTemplate;

            OtherCivilizationsSummaryItem1.DataContext = _otherCivs; // ListBox data context set to OtherCivs, or so I hope
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
            OtherCivilizationsSummaryItem1.Items.Clear();
            FriendCivilizationsItems.Items.Clear();

            GameLog.Core.Combat.DebugFormat("cleared all ClearUnitTrees");

        }

        private void PopulateUnitTrees()
        {
            ClearUnitTrees();

            // turn off the dummy civilization when testing is over
            Civilization rom = new Civilization();
            rom.Race = ROMULANS;
            rom.ShortName = "Romulans";
            Civilization dummy = new Civilization();
            dummy.Race = DUMMIES;
            dummy.ShortName = "Dummy";

            foreach (CombatAssets friendlyAssets in _update.FriendlyAssets)
            {
               // int friendlyAssetsFirepower = 0;
                var shootingPlayerCivs = new List<Civilization>();

                if (friendlyAssets.Station != null)
                {
                    FriendlyStationItem.Header = friendlyAssets.Station;
                    shootingPlayerCivs.Add(friendlyAssets.Station.Owner);
                    //friendlyAssetsFirepower += friendlyAssets.Station.FirePower;
                }
                if (friendlyAssets.CombatShips != null)
                {
                    foreach (CombatUnit shipStats in friendlyAssets.CombatShips)
                    {
                        FriendlyCombatantItems.Items.Add(shipStats);
                        shootingPlayerCivs.Add(shipStats.Owner);
                        //friendlyAssetsFirepower += shipStats.FirePower;
                    }
                }
                if (friendlyAssets.NonCombatShips != null)
                {
                    foreach (CombatUnit shipStats in friendlyAssets.NonCombatShips)
                    {
                        FriendlyNonCombatantItems.Items.Add(shipStats);
                        shootingPlayerCivs.Add(shipStats.Owner);
                       // friendlyAssetsFirepower += shipStats.FirePower;
                    }
                }
                foreach (CombatUnit shipStats in friendlyAssets.DestroyedShips)
                {
                    FriendlyDestroyedItems.Items.Add(shipStats);
                    //shootingPlayerCivs.Add(shipStats.Owner);
                }

                foreach (CombatUnit shipStats in friendlyAssets.AssimilatedShips)
                {
                    FriendlyAssimilatedItems.Items.Add(shipStats);
                }

                foreach (CombatUnit shipStats in friendlyAssets.EscapedShips)
                {
                    FriendlyEscapedItems.Items.Add(shipStats);
                  //  shootingPlayerCivs.Add(shipStats.Owner);
                }
                shootingPlayerCivs.Add(dummy);
                shootingPlayerCivs =shootingPlayerCivs.Distinct().ToList();
                _friendlyCivs = shootingPlayerCivs;
                _shooterCivilizations1 = shootingPlayerCivs;
                _shooterCivilizations2 = shootingPlayerCivs;
                foreach (Civilization Friend in _friendlyCivs)
                {
                    FriendCivilizationsItems.Items.Add(Friend); // a template for rach other civ
                    GameLog.Core.Combat.DebugFormat("_otherCivs containing = {0}", Friend.ShortName);
                }
                //_friendlyEmpireStrength = friendlyAssetsFirepower;
            }

            /* Hostile (others) Assets */
            foreach (CombatAssets hostileAssets in _update.HostileAssets)
            {


                var otherCivs = new List<Civilization>();
           
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
                }
                otherCivs.Add(rom);
               // otherCivs.Add(dummy);
                _otherCivs = otherCivs.Distinct().ToList(); // adding Civilizations of the others into the field _otherCivs
              
                foreach (Civilization Other in _otherCivs)
                {
                    OtherCivilizationsSummaryItem1.Items.Add(Other); // a template for rach other civ
                    GameLog.Core.Combat.DebugFormat("_otherCivs containing = {0}", Other.ShortName);
                    
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

            OtherCivilizationsSummaryItem1.Visibility = OtherCivilizationsSummaryItem1.HasItems ? Visibility.Visible : Visibility.Collapsed;
            FriendCivilizationsItems.Visibility = FriendCivilizationsItems.HasItems ? Visibility.Visible : Visibility.Collapsed;
        }
        private void SelectControl()
        {
            
        }
        private void TargetButton1_Click(object sender, RoutedEventArgs e)
        {
           
             RadioButton cmd = sender as RadioButton;
            //choiceTextBlock.Text = "Seleected" + cmd.GroupName + ": " + cmd.Name;
            if (cmd.DataContext is Civilization) // && Civilizations != null)
            {
               // Civilization dummy = new Civilization();
                Civilization theTargetedCiv = (Civilization)cmd.DataContext;
                _targetCivilzation1 = theTargetedCiv;
                Dictionary<Civilization, Civilization> myShooters = new Dictionary<Civilization, Civilization>();
              //  myShooters.Add(dummy, dummy);
                {
                    var shootist = _shooterCivilizations1.FirstOrDefault();

                    myShooters.Add(shootist, theTargetedCiv);
                    _whoIsShootingWhomFirst = myShooters;
                    if (myShooters.Count > 1) { _shooterCivilizations1.Remove(shootist); }
                    if(_shooterCivilizations1 != null)
                    {
                        foreach (Civilization shooter in _shooterCivilizations1)
                        {
                            try
                            { 
                               _whoIsShootingWhomFirst.Add(shooter, theTargetedCiv);
                                
                            }
                            catch (ArgumentException)
                            {
                                GameLog.Core.Combat.DebugFormat("Could not add civilization {0} to _whoIsShootingWhomFirst for target {1}", shooter.ShortName, theTargetedCiv.ShortName);
                            }
                        }
                    }                   
                }
                OtherCivs.Remove(theTargetedCiv);
            }
        }

        private void TargetButton2_Click(object sender, RoutedEventArgs e)
        {

            RadioButton cmd = (RadioButton)sender;
            if (cmd.DataContext is Civilization) // && Civilizations != null)
            {
               // Civilization dummy = new Civilization();
                Civilization theTargetedCiv = (Civilization)cmd.DataContext;
                _targetCivilzation2 = theTargetedCiv;
                Dictionary<Civilization, Civilization> myShooters = new Dictionary<Civilization, Civilization>();
               // myShooters.Add(dummy, dummy);
                {
                    var shootist = _shooterCivilizations2.FirstOrDefault();

                    myShooters.Add(shootist, theTargetedCiv);
                    _whoIsShootingWhomSecond = myShooters;
                    if (myShooters.Count > 1) { _shooterCivilizations1.Remove(shootist); }
                    if (_shooterCivilizations2 != null)
                    {
                        foreach (Civilization shooter in _shooterCivilizations2)
                        {
                            try
                            {
                                _whoIsShootingWhomSecond.Add(shooter, theTargetedCiv);

                            }
                            catch (ArgumentException)
                            {
                                GameLog.Core.Combat.DebugFormat("Could not add civilization {0} to _whoIsShootingWhomSecond for target {1}", shooter.ShortName, theTargetedCiv.ShortName);
                            }
                        }
                    }
                }
                
                OtherCivs.Remove(theTargetedCiv);
            }
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
            {
                order = CombatOrder.Hail;
                //_playerIsShooting = false;
            }

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
