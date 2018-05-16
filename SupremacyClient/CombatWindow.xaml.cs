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

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for CombatWindow.xaml
    /// </summary>

    public partial class CombatWindow
    {
        private CombatUpdate _update;
        private CombatAssets _playerAssets;
        private IAppContext _appContext;

        bool _tracingCombatWindow = false;   // turn true if you want
        //bool _tracingCombatWindow = true;   // turn true if you want

        public CombatWindow()
        {
            InitializeComponent();
            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            ClientEvents.CombatUpdateReceived.Subscribe(OnCombatUpdateReceived, ThreadOption.UIThread);
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
                HeaderText.Text = ResourceManager.GetString("COMBAT_HEADER") + ": "
                    + String.Format(ResourceManager.GetString("COMBAT_ROUND"), _update.RoundNumber);
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

            //We need combat assets to be able to engage
            EngageButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.Station != null));
            //We need combat assets to be able to rush the opposition
            RushButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count > 0);
            //There needs to be transports in the opposition to be able to target them
            TransportsButton.IsEnabled = _update.HostileAssets.Any(ha => ha.NonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Transport"));
            //We need at least 3 ships to create a formation
            FormationButton.IsEnabled = _update.FriendlyAssets.Any(fa => fa.CombatShips.Count >= 3);
            //We need assets to be able to retreat
            RetreatButton.IsEnabled = _update.FriendlyAssets.Any(fa => (fa.CombatShips.Count > 0) || (fa.NonCombatShips.Count > 0) || (fa.Station != null));
            //Can only hail on the first round
            HailButton.IsEnabled = (update.RoundNumber == 1);

            ButtonsPanel0.Visibility = update.IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            ButtonsPanel1.Visibility = update.IsCombatOver ? Visibility.Collapsed : Visibility.Visible;
            CloseButton.Visibility = update.IsCombatOver ? Visibility.Visible : Visibility.Collapsed;
            ButtonsPanel0.IsEnabled = true;
            ButtonsPanel1.IsEnabled = true;

            if (!IsVisible)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NullableBoolFunction(ShowDialog));
        }

        private void PopulateUnitTrees()
        {
            DataTemplate itemTemplate = TryFindResource("AssetTreeItemTemplate") as DataTemplate;

            TreeViewItem combatantItems = new TreeViewItem();
            TreeViewItem nonCombatantItems = new TreeViewItem();
            TreeViewItem escapedItems = new TreeViewItem();
            TreeViewItem destroyedItems = new TreeViewItem();
            TreeViewItem assimilatedItems = new TreeViewItem();

            combatantItems.Header = "Combatant Units";
            nonCombatantItems.Header = "Non-Combatant Units";
            escapedItems.Header = "Escaped Units";
            destroyedItems.Header = "Destroyed Units";
            assimilatedItems.Header = "Assimilated Units";

            combatantItems.IsExpanded = true;
            nonCombatantItems.IsExpanded = true;
            escapedItems.IsExpanded = true;
            destroyedItems.IsExpanded = true;
            assimilatedItems.IsExpanded = true;

            combatantItems.ItemTemplate = itemTemplate;
            nonCombatantItems.ItemTemplate = itemTemplate;
            escapedItems.ItemTemplate = itemTemplate;
            destroyedItems.ItemTemplate = itemTemplate;
            assimilatedItems.ItemTemplate = itemTemplate;

            FriendlyAssetsTree.Items.Clear();

            foreach (CombatAssets friendlyAssets in _update.FriendlyAssets)
            {
                if (friendlyAssets.Station != null)
                {
                    TreeViewItem stationItem = new TreeViewItem();
                    stationItem.Header = friendlyAssets.Station;
                    stationItem.HeaderTemplate = itemTemplate;
                    FriendlyAssetsTree.Items.Add(stationItem);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit: ShieldIntegry={0}, HullIntegry={1}, Name={2}", friendlyAssets.Station.ShieldIntegrity, friendlyAssets.Station.HullIntegrity, friendlyAssets.Station.Name);
                }

                foreach (CombatUnit shipStats in friendlyAssets.CombatShips)
                {
                    combatantItems.Items.Add(shipStats);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit-combatantItems: ShieldIntegry={0}, HullIntegry={1}, Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                }

                foreach (CombatUnit shipStats in friendlyAssets.NonCombatShips)
                {
                    nonCombatantItems.Items.Add(shipStats);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit-nonCombatantItems: ShieldIntegry={0}, HullIntegry={1}, Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                }

                foreach (CombatUnit shipStats in friendlyAssets.EscapedShips)
                {
                    escapedItems.Items.Add(shipStats);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit-escapedItems: ShieldIntegry={0}, HullIntegry={1}, Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                }

                foreach (CombatUnit shipStats in friendlyAssets.DestroyedShips)
                {
                    destroyedItems.Items.Add(shipStats);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit-destroyedItems: ShieldIntegry={0}, HullIntegry={1}, Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                }

                foreach (CombatUnit shipStats in friendlyAssets.AssimilatedShips)
                {
                    assimilatedItems.Items.Add(shipStats);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("FriendlyUnit-assimilatedItems: ShieldIntegry={0}, HullIntegry={1}, Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                }
            }

            if (combatantItems.Items.Count > 0)
                FriendlyAssetsTree.Items.Add(combatantItems);
            if (nonCombatantItems.Items.Count > 0)
                FriendlyAssetsTree.Items.Add(nonCombatantItems);
            if (escapedItems.Items.Count > 0)
                FriendlyAssetsTree.Items.Add(escapedItems);
            if (destroyedItems.Items.Count > 0)
                FriendlyAssetsTree.Items.Add(destroyedItems);
            if (assimilatedItems.Items.Count > 0)
                FriendlyAssetsTree.Items.Add(assimilatedItems);

            combatantItems = new TreeViewItem();
            nonCombatantItems = new TreeViewItem();
            escapedItems = new TreeViewItem();
            destroyedItems = new TreeViewItem();
            assimilatedItems = new TreeViewItem();

            combatantItems.Header = "Combatant Units";
            nonCombatantItems.Header = "Non-Combatant Units";
            escapedItems.Header = "Escaped Units";
            destroyedItems.Header = "Destroyed Units";
            assimilatedItems.Header = "Assimilated Units";

            combatantItems.IsExpanded = true;
            nonCombatantItems.IsExpanded = true;
            escapedItems.IsExpanded = true;
            destroyedItems.IsExpanded = true;
            assimilatedItems.IsExpanded = true;

            combatantItems.ItemTemplate = itemTemplate;
            nonCombatantItems.ItemTemplate = itemTemplate;
            escapedItems.ItemTemplate = itemTemplate;
            destroyedItems.ItemTemplate = itemTemplate;
            assimilatedItems.ItemTemplate = itemTemplate;

            HostileAssetsTree.Items.Clear();

            foreach (CombatAssets hostileAssets in _update.HostileAssets)
            {
                if (hostileAssets.Station != null)
                {
                    TreeViewItem stationItem = new TreeViewItem();
                    stationItem.Header = hostileAssets.Station;
                    stationItem.HeaderTemplate = itemTemplate;
                    HostileAssetsTree.Items.Add(stationItem);

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("HostileUnit: ShieldIntegry={0}, HullIntegry={1}, Name={2}", hostileAssets.Station.ShieldIntegrity, hostileAssets.Station.HullIntegrity, hostileAssets.Station.Name);
                }
                foreach (CombatUnit shipStats in hostileAssets.CombatShips)
                {

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("HostileUnit -combatantItems: ShieldIntegry={0}, HullIntegry={1}, Name= {2}, Owner Name={3} ", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name, shipStats.Owner.Name);
                    combatantItems.Items.Add(shipStats);
                }
                foreach (CombatUnit shipStats in hostileAssets.NonCombatShips)
                {

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("HostileUnit -nonCombatantItems: NonCombat: ShieldIntegry={0}, HullIntegry={1}, NonCombatShip-Name={2}", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                    nonCombatantItems.Items.Add(shipStats);
                }
                foreach (CombatUnit shipStats in hostileAssets.EscapedShips)
                {

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("HostileUnit -escapedItems: ShieldIntegry={0}, HullIntegry={1}, Name={2} ", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                    escapedItems.Items.Add(shipStats);
                }
                foreach (CombatUnit shipStats in hostileAssets.DestroyedShips)
                {

                    if (_tracingCombatWindow)
                        GameLog.Client.GameData.DebugFormat("HostileUnit -destroyedItems: ShieldIntegry={0}, HullIntegry={1}, Name={2} ", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                    destroyedItems.Items.Add(shipStats);
                }

                // Assimilated items only shown at Borg side
                //foreach (CombatUnit shipStats in hostileAssets.AssimilatedShips)
                //{
                //    //GameLog.Client.GameData.DebugFormat("HostileUnit : ShieldIntegry={0}, HullIntegry={1}, Name={2} ", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name);
                //    //assimilatedItems.Items.Add(shipStats);

                //    if (_tracingCombatWindow == true)
                //        GameLog.Client.GameData.DebugFormat("HostileUnit assimilatedItems: ShieldIntegry={0}, HullIntegry={1}, Name= {2}, Owner Name={3} ", shipStats.ShieldIntegrity, shipStats.HullIntegrity, shipStats.Name, shipStats.Owner.Name);
                //    assimilatedItems.Items.Add(shipStats);
                //}
            }

            if (combatantItems.Items.Count > 0)
                HostileAssetsTree.Items.Add(combatantItems);
            if (nonCombatantItems.Items.Count > 0)
                HostileAssetsTree.Items.Add(nonCombatantItems);
            if (escapedItems.Items.Count > 0)
                HostileAssetsTree.Items.Add(escapedItems);
            if (destroyedItems.Items.Count > 0)
                HostileAssetsTree.Items.Add(destroyedItems);
            if (assimilatedItems.Items.Count > 0)
                HostileAssetsTree.Items.Add(assimilatedItems);
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

            GameLog.Print("OnOrderButtonClicked:  Combat Window: Order Button clicked by Player = {1}", sender, order);

            ButtonsPanel0.IsEnabled = false;
            ButtonsPanel1.IsEnabled = false;
            ClientCommands.SendCombatOrders.Execute(CombatHelper.GenerateBlanketOrders(_playerAssets, order));
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            //base.DialogResult = true;
            DialogResult = true;
        }
    }
}