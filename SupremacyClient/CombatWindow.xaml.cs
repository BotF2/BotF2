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
        private Civilization _primeTargetCivilzation; // primary player-selected civ to attack
        private Civilization _secondTargetCivilzation; // secondary player-selected civ to attack
        private Dictionary<string, CombatUnit> _ourFiendlyCombatUnits; // do I need combat unit or combat assets here?
        private Dictionary<string, CombatUnit> _othersCombatUnits;
        //protected int _friendlyEmpireStrength;
        //public string _friendlyEmpireStrengthString = "444";
        private IAppContext _appContext;

        public List<Civilization> OtherCivs
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("OtherCivs - GET: _otherCivs = {0}", _otherCivs.ToString());
                return _otherCivs;
            }
            set
            {
                GameLog.Core.Combat.DebugFormat("OtherCivs - SET: _otherCivs = {0}", value.ToString());
                _otherCivs = value;
            }
        }

        public List<string> OtherCivsKeys
        {
            get
            {
                //GameLog.Core.Combat.DebugFormat("OtherCivsKeys - GET: _otherCivsKeys = {0}", _otherCivsKeys.ToString());
                return _otherCivsKeys;
            }
            set
            {
                //GameLog.Core.Combat.DebugFormat("OtherCivsKeys - SET: _otherCivsKeys = {0}", value.ToString());
                _otherCivsKeys = value;
            }
           
        }
        //List<string> ListOtherCivsKeys = OtherCivsKeys;

        public Civilization PrimaryTargetCivilization
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("PrimaryTargetCivilization - GET: _otherCivsKeys = {0}", _primeTargetCivilzation.ToString());
                return _primeTargetCivilzation;
            }
            set
            {

                _primeTargetCivilzation = value;
                GameLog.Core.Combat.DebugFormat("PrimaryTargetCivilization - SET: _otherCivsKeys = {0}", _primeTargetCivilzation.ToString());
            }
        }

        public Civilization SecondaryTargetCivilization
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("SecondaryTargetCivilization - GET: _otherCivsKeys = {0}", _secondTargetCivilzation.ToString());
                return _secondTargetCivilzation;
            }
            set
            {
                _secondTargetCivilzation = value;
                GameLog.Core.Combat.DebugFormat("SecondaryTargetCivilization - GET: _otherCivsKeys = {0}", _secondTargetCivilzation.ToString());
            }
        }

      
        public CombatWindow()
        {
            InitializeComponent();
          
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

            DataTemplate civDropDownTemplate = TryFindResource("CivDropDownTemplate") as DataTemplate;

            //OtherCivilizationsHeaderDropDown.HeaderTemplate = civDropDownTemplate;
           // OtherCivilizationsDropDown1.ItemTemplate = civDropDownTemplate;
            //OtherCivilizationsDropDown2.ItemTemplate = civDropDownTemplate;

            //DataTemplate civDropDownTemplate2 = TryFindResource("CivDropDownTemplate") as DataTemplate;

            ////OtherCivilizationsHeaderDropDown.HeaderTemplate = civDropDownTemplate;
            //OtherCivilizationsDropDown2.ItemTemplate = civDropDownTemplate2;

            //ComboBoxItem civDropDownTemplate = TryFindResource("CivTreeDropDownTemplate") as ComboBoxItem;
            //PrimaryTargetCivilizationItem.ItemTemplate = civDropDownTemplate;
            //SecondaryTargetListItem = civDropDownTemplate;

            //OtherCivsKeys = _otherCivsKeys;
            


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

            //if (_playerAssets.Owner.IsEmpire && _playerAssets.Owner.Key == "Mike")
            //{
            //    _friendlyEmpireStrength = _update.EmpireStrengths[_appContext.LocalPlayer.Empire.Key.ToString()];
            //    GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);
            //}


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
            //OtherCivilizationsDropDown2.Clear();
            OtherCivilizationsSummaryItem.Items.Clear();
            GameLog.Core.Combat.DebugFormat("cleared all ClearUnitTrees");


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

                // Add each Civilization key from otherCivs and creating foo civilizations for testing
                foreach (Civilization civ in otherCivs)
                {
                    //otherCivKeys.Add("TRUMPS");
                    otherCivKeys.Add(civ.Key);
                    otherCivKeys.Add("CLINTONS");
                    // works but not neccessary   GameLog.Core.Combat.DebugFormat("adding into otherCivs: civ.key = {0}", civ.Key);
                }

                _otherCivsKeys = otherCivKeys.Distinct().ToList();
                OtherCivsKeys = _otherCivsKeys;
                _otherCivs = otherCivs;
                 //OtherCivs = _otherCivs;
                var _OtherCivsKeys = _otherCivsKeys;

                foreach (string Other in _otherCivsKeys)
                {
                    OtherCivilizationsSummaryItem.Items.Add(Other);
                    GameLog.Core.Combat.DebugFormat("_otherCivsKeys containing = {0}", Other.ToString());

                    //PrimaryTargetDropDown.Add(Other);
                    //OtherCivilizationsHeaderDropDown.Header.Add(Other);

                    //OtherCivilizationsDropDown1.Items.Add(Other);

                   // OtherCivilizationsDropDown2.Items.Add(Other);
                    GameLog.Core.Combat.DebugFormat("_otherCivsKeys: Added to OtherCivilizationsDropDown2.Items: Other = {0}", Other.ToString());
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
           // OtherCivilizationsHeaderDropDown.Visibility = OtherCivilizationsHeaderDropDown.HasHeader ? Visibility.Visible : Visibility.Collapsed;
           // OtherCivilizationsDropDown.Header = OtherCivilizationsDropDown.HasItems ? ResourceManager.GetString("COMBAT_CIVILIZATIONS") : null;
           //OtherCivilizationsDropDown1.Visibility = OtherCivilizationsDropDown1.HasItems ? Visibility.Visible : Visibility.Collapsed;
           // OtherCivilizationsDropDown2.Visibility = OtherCivilizationsDropDown2.HasItems ? Visibility.Visible : Visibility.Collapsed;

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

    public class OtherCivsKey : INotifyPropertyChanged
    {
        private string _otherCivKey = "Reg";
        private IAppContext _appContext;

        //private List<String> _otherCivsKeys;
        //private static OtherCivsKey _designInstance;

        //public static OtherCivsKey DesignInstance
        //{
        //    get
        //    {
        //        if (_designInstance == null)
        //        {
        //            _designInstance = new OtherCivsKey(DesignTimeAppContext.Instance)
        //            {

        //                //SelectedOtherCivsKey = DesignTimeObjects.OtherCivsKey
        //            };
        //            GameLog.Core.Combat.DebugFormat("OtherCivsKey DesignInstance - GET: _designInstance = {0}", _designInstance.ToString());
        //        }
        //        return _designInstance;
        //    }
        //}

        #region OtherCivsKey Property
        public event EventHandler SelectedOtherCivsKeyChanged;

        private OtherCivsKey _selectedOtherCivsKey;

        private void OnSelectedOtherCivsKeyChanged(OtherCivsKey oldValue, OtherCivsKey newValue)
        {
            var handler = SelectedOtherCivsKeyChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<OtherCivsKey>(oldValue, newValue));

            GameLog.Core.Combat.DebugFormat("SelectedOtherCivsKeyChanged...");

            OnPropertyChanged("SelectedOtherCivsKey");
        }

        public OtherCivsKey SelectedOtherCivsKey
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("SelectedOtherCivsKey - GET: _selectedOtherCivsKey = {0}", _selectedOtherCivsKey.ToString());
                return _selectedOtherCivsKey;
            }
            set
            {
                var oldValue = _selectedOtherCivsKey;
                _selectedOtherCivsKey = value;
                GameLog.Core.Combat.DebugFormat("SelectedOtherCivsKey - SET: oldvalue = {0}, _selectedOtherCivsKey NEW = {1}", oldValue.ToString(), _selectedOtherCivsKey.ToString());
                OnSelectedOtherCivsKeyChanged(oldValue, value);
            }
        }

        #endregion
        #region OtherCivsKeys Property
        public event EventHandler OtherCivsKeysChanged;

        private IEnumerable<OtherCivsKey> _otherCivsKeys;
        private DesignTimeAppContext instance;

        private void OnOtherCivsKeysChanged()
        {
            var handler = OtherCivsKeysChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            GameLog.Core.Combat.DebugFormat("OnOtherCivsKeysChanged...");

            OnPropertyChanged("OtherCivsKeys");
        }

        public IEnumerable<OtherCivsKey> OtherCivsKeys
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("OtherCivsKeys: GET _OtherCivsKeys = {0}", _otherCivsKeys);
                return _otherCivsKeys;
            }
            set
            {
                if (Equals(_otherCivsKeys, value))
                    return;
                _otherCivsKeys = value;
                GameLog.Core.Combat.DebugFormat("OtherCivsKeys: SET _OtherCivsKeys = {0}", _otherCivsKeys);
                OnOtherCivsKeysChanged();
            }
        }
        #endregion

        //public override sealed Entities
        //{
        //    get { return Universe.Entities; }
        //}

        public string Name
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("GET  Name for Combat = {0}", _otherCivKey);
                return _otherCivKey;
            }
            set
            {
                GameLog.Core.Combat.DebugFormat("SET  Name for Combat = {0}", _otherCivKey);
                _otherCivKey = value;
            }
        }

        public OtherCivsKey(DesignTimeAppContext instance)
        {
            GameLog.Core.Combat.DebugFormat("OtherCivsKey(DesignTimeAppContext instance)....");
            this.instance = instance;
        }

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    GameLog.Core.Combat.DebugFormat("PropertyChangedEventHandler -ADD");
                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    GameLog.Core.Combat.DebugFormat("PropertyChangedEventHandler -REMOVE");
                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            GameLog.Core.Combat.DebugFormat("OnPropertyChanged");
            _propertyChanged.Raise(this, propertyName);
        }
    }
}

