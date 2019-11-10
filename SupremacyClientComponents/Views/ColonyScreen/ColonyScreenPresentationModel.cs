// ColonyScreenPresentationModel.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using Supremacy.Economy;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public class ColonyScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        private static ColonyScreenPresentationModel _designInstance;

        public static ColonyScreenPresentationModel DesignInstance
        {
            get
            {
                if (_designInstance == null)
                {
                    _designInstance = new ColonyScreenPresentationModel(DesignTimeAppContext.Instance)
                                      {
                                          SelectedColony = DesignTimeObjects.Colony
                                      };
                }
                return _designInstance;
            }
        }

        [InjectionConstructor]
        public ColonyScreenPresentationModel(IAppContext appContext)
            : base(appContext) { }

        #region Colony Property
        public event EventHandler SelectedColonyChanged;

        private Colony _selectedColony;

        private void OnSelectedColonyChanged(Colony oldValue, Colony newValue)
        {
            var handler = SelectedColonyChanged;
            if (handler != null)
                handler(this, new PropertyChangedRoutedEventArgs<Colony>(oldValue, newValue));
            OnPropertyChanged("SelectedColony");
        }

        public Colony SelectedColony
        {
            get { return _selectedColony; }
            set
            {
                var oldValue = _selectedColony;
                _selectedColony = value;
                OnSelectedColonyChanged(oldValue, value);
            }
        }
        #endregion

        #region Colonies Property
        public event EventHandler ColoniesChanged;

        private IEnumerable<Colony> _colonies;

        private void OnColoniesChanged()
        {
            var handler = ColoniesChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("Colonies");
        }

        public IEnumerable<Colony> Colonies
        {
            get { return _colonies; }
            set
            {
                if (Equals(_colonies, value))
                    return;
                _colonies = value;
                OnColoniesChanged();
            }
        }
        #endregion

        #region SelectedPlanetaryBuildProject Property
        private BuildProject _selectedPlanetaryBuildProject;

        public event EventHandler SelectedPlanetaryBuildProjectChanged;

        private void OnSelectedPlanetaryBuildProjectChanged()
        {
            var handler = SelectedPlanetaryBuildProjectChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("SelectedPlanetaryBuildProject");
        }

        public BuildProject SelectedPlanetaryBuildProject
        {
            get { return _selectedPlanetaryBuildProject; }
            set
            {
                if (Equals(_selectedPlanetaryBuildProject, value))
                    return;
                _selectedPlanetaryBuildProject = value;
                OnSelectedPlanetaryBuildProjectChanged();
            }
        }
        #endregion

        #region SelectedShipyardBuildProject Property
        private BuildProject _selectedShipyardBuildProject;

        public event EventHandler SelectedShipyardBuildProjectChanged;

        private void OnSelectedShipyardBuildProjectChanged()
        {
            var handler = SelectedShipyardBuildProjectChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("SelectedShipyardBuildProject");
        }

        public BuildProject SelectedShipyardBuildProject
        {
            get { return _selectedShipyardBuildProject; }
            set
            {
                if (Equals(_selectedShipyardBuildProject, value))
                    return;
                _selectedShipyardBuildProject = value;
                OnSelectedShipyardBuildProjectChanged();
            }
        }
        #endregion

        #region PlanetaryBuildProjects Property
        private IEnumerable<BuildProject> _planetaryBuildProjects;

        public event EventHandler PlanetaryBuildProjectsChanged;

        private void OnPlanetaryBuildProjectsChanged()
        {
            var handler = PlanetaryBuildProjectsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("PlanetaryBuildProjects");
        }

        public IEnumerable<BuildProject> PlanetaryBuildProjects
        {
            get { return _planetaryBuildProjects; }
            set
            {
                if (Equals(_planetaryBuildProjects, value))
                    return;
                _planetaryBuildProjects = value;
                OnPlanetaryBuildProjectsChanged();
            }
        }
        #endregion

        #region ShipyardBuildProjects Property
        private IEnumerable<BuildProject> _shipyardBuildProjects;

        public event EventHandler ShipyardBuildProjectsChanged;

        private void OnShipyardBuildProjectsChanged()
        {
            var handler = ShipyardBuildProjectsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ShipyardBuildProjects");
        }

        public IEnumerable<BuildProject> ShipyardBuildProjects
        {
            get { return _shipyardBuildProjects; }
            set
            {
                if (Equals(_shipyardBuildProjects, value))
                    return;
                _shipyardBuildProjects = value;
                OnShipyardBuildProjectsChanged();
            }
        }
        #endregion

        #region AddToPlanetaryBuildQueue Command
        public event EventHandler AddToPlanetaryBuildQueueCommandChanged;

        private ICommand _addToPlanetaryBuildQueueCommand;

        private void OnAddToPlanetaryBuildQueueCommandChanged()
        {
            var handler = AddToPlanetaryBuildQueueCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("AddToPlanetaryBuildQueueCommand");
        }

        public ICommand AddToPlanetaryBuildQueueCommand
        {
            get { return _addToPlanetaryBuildQueueCommand; }
            set
            {
                if (Equals(_addToPlanetaryBuildQueueCommand, value))
                    return;
                _addToPlanetaryBuildQueueCommand = value;
                OnAddToPlanetaryBuildQueueCommandChanged();
            }
        }
        #endregion

        #region RemoveFromPlanetaryBuildQueue Command
        public event EventHandler RemoveFromPlanetaryBuildQueueCommandChanged;

        private ICommand _removeFromPlanetaryBuildQueueCommand;

        private void OnRemoveFromPlanetaryBuildQueueCommandChanged()
        {
            var handler = RemoveFromPlanetaryBuildQueueCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("RemoveFromPlanetaryBuildQueueCommand");
        }

        public ICommand RemoveFromPlanetaryBuildQueueCommand
        {
            get { return _removeFromPlanetaryBuildQueueCommand; }
            set
            {
                if (Equals(_removeFromPlanetaryBuildQueueCommand, value))
                    return;
                _removeFromPlanetaryBuildQueueCommand = value;
                OnRemoveFromPlanetaryBuildQueueCommandChanged();
            }
        }
        #endregion

        #region AddToShipyardBuildQueue Command
        public event EventHandler AddToShipyardBuildQueueCommandChanged;

        private ICommand _addToShipyardBuildQueueCommand;

        private void OnAddToShipyardBuildQueueCommandChanged()
        {
            var handler = AddToShipyardBuildQueueCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("AddToShipyardBuildQueueCommand");
        }

        public ICommand AddToShipyardBuildQueueCommand
        {
            get { return _addToShipyardBuildQueueCommand; }
            set
            {
                if (Equals(_addToShipyardBuildQueueCommand, value))
                    return;
                _addToShipyardBuildQueueCommand = value;
                OnAddToShipyardBuildQueueCommandChanged();
            }
        }
        #endregion

        #region RemoveFromShipyardBuildQueue Command
        public event EventHandler RemoveFromShipyardBuildQueueCommandChanged;

        private ICommand _removeFromShipyardBuildQueueCommand;

        private void OnRemoveFromShipyardBuildQueueCommandChanged()
        {
            var handler = RemoveFromShipyardBuildQueueCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("RemoveFromShipyardBuildQueueCommand");
        }

        public ICommand RemoveFromShipyardBuildQueueCommand
        {
            get { return _removeFromShipyardBuildQueueCommand; }
            set
            {
                if (Equals(_removeFromShipyardBuildQueueCommand, value))
                    return;
                _removeFromShipyardBuildQueueCommand = value;
                OnRemoveFromShipyardBuildQueueCommandChanged();
            }
        }
        #endregion

        #region CancelBuildProject Command
        public event EventHandler CancelBuildProjectCommandChanged;
        public event EventHandler BuyBuildProjectCommandChanged;

        private ICommand _cancelBuildProjectCommand;
        private ICommand _buyBuildProjectCommand;

        private void OnCancelBuildProjectCommandChanged()
        {
            var handler = CancelBuildProjectCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("CancelBuildProjectCommand");
        }

        private void OnBuyBuildProjectCommandChanged()
        {
            var handler = BuyBuildProjectCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("BuyBuildProjectCommand");
        }

        public ICommand CancelBuildProjectCommand
        {
            get { return _cancelBuildProjectCommand; }
            set
            {
                if (Equals(_cancelBuildProjectCommand, value))
                    return;
                _cancelBuildProjectCommand = value;
                OnCancelBuildProjectCommandChanged();
            }
        }

        public ICommand BuyBuildProjectCommand
        {
            get { return _buyBuildProjectCommand; }
            set
            {
                if (Equals(_buyBuildProjectCommand, value))
                    return;
                _buyBuildProjectCommand = value;
                OnBuyBuildProjectCommandChanged();
            }
        }
        #endregion

        #region ActivateFacility Command
        private ICommand _activateFacilityCommand;

        public event EventHandler ActivateFacilityCommandChanged;

        private void OnActivateFacilityCommandChanged()
        {
            var handler = ActivateFacilityCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ActivateFacilityCommand");
        }

        public ICommand ActivateFacilityCommand
        {
            get { return _activateFacilityCommand; }
            set
            {
                if (Equals(_activateFacilityCommand, value))
                    return;
                _activateFacilityCommand = value;
                OnActivateFacilityCommandChanged();
            }
        }
        #endregion

        #region DeactivateFacility Command
        private ICommand _deactivateFacilityCommand;

        public event EventHandler DeactivateFacilityCommandChanged;

        private void OnDeactivateFacilityCommandChanged()
        {
            var handler = DeactivateFacilityCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("DeactivateFacilityCommand");
        }

        public ICommand DeactivateFacilityCommand
        {
            get { return _deactivateFacilityCommand; }
            set
            {
                if (Equals(_deactivateFacilityCommand, value))
                    return;
                _deactivateFacilityCommand = value;
                OnDeactivateFacilityCommandChanged();
            }
        }
        #endregion

        #region ScrapFacility Command
        private ICommand _scrapFacilityCommand;

        public event EventHandler ScrapFacilityCommandChanged;

        private void OnScrapFacilityCommandChanged()
        {
            var handler = ScrapFacilityCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ScrapFacilityCommand");
        }

        public ICommand ScrapFacilityCommand
        {
            get { return _scrapFacilityCommand; }
            set
            {
                if (Equals(_scrapFacilityCommand, value))
                    return;
                _scrapFacilityCommand = value;
                OnScrapFacilityCommandChanged();
            }
        }
        #endregion

        #region UnscrapFacility Command
        private ICommand _unscrapFacilityCommand;

        public event EventHandler UnscrapFacilityCommandChanged;

        private void OnUnscrapFacilityCommandChanged()
        {
            var handler = UnscrapFacilityCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("UnscrapFacilityCommand");
        }

        public ICommand UnscrapFacilityCommand
        {
            get { return _unscrapFacilityCommand; }
            set
            {
                if (Equals(_unscrapFacilityCommand, value))
                    return;
                _unscrapFacilityCommand = value;
                OnUnscrapFacilityCommandChanged();
            }
        }
        #endregion

        #region ToggleBuildingScrap Command
        private ICommand _toggleBuildingScrapCommand;

        public event EventHandler ToggleBuildingScrapCommandChanged;

        private void OnToggleBuildingScrapCommandChanged()
        {
            var handler = ToggleBuildingScrapCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ToggleBuildingScrapCommand");
        }

        public ICommand ToggleBuildingScrapCommand
        {
            get { return _toggleBuildingScrapCommand; }
            set
            {
                if (Equals(_toggleBuildingScrapCommand, value))
                    return;
                _toggleBuildingScrapCommand = value;
                OnToggleBuildingScrapCommandChanged();
            }
        }
        #endregion

        #region ToggleBuildingIsActive Command
        private ICommand _toggleBuildingIsActiveCommand;

        public event EventHandler ToggleBuildingIsActiveCommandChanged;

        private void OnToggleBuildingIsActiveCommandChanged()
        {
            var handler = ToggleBuildingIsActiveCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ToggleBuildingIsActiveCommand");
        }

        public ICommand ToggleBuildingIsActiveCommand
        {
            get { return _toggleBuildingIsActiveCommand; }
            set
            {
                if (Equals(_toggleBuildingIsActiveCommand, value))
                    return;
                _toggleBuildingIsActiveCommand = value;
                OnToggleBuildingIsActiveCommandChanged();
            }
        }
        #endregion

        #region ToggleShipyardBuildSlot Command
        private ICommand _toggleShipyardBuildSlotCommand;

        public event EventHandler ToggleShipyardBuildSlotCommandChanged;

        private void OnToggleShipyardBuildSlotCommandChanged()
        {
            var handler = ToggleShipyardBuildSlotCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ToggleShipyardBuildSlotCommand");
        }

        public ICommand ToggleShipyardBuildSlotCommand
        {
            get { return _toggleShipyardBuildSlotCommand; }
            set
            {
                if (Equals(_toggleShipyardBuildSlotCommand, value))
                    return;
                _toggleShipyardBuildSlotCommand = value;
                OnToggleShipyardBuildSlotCommandChanged();
            }
        }
        #endregion

        #region SelectShipBuildProjectCommand Command
        private ICommand _selectShipBuildProjectCommand;

        public event EventHandler SelectShipBuildProjectCommandChanged;

        private void OnSelectShipBuildProjectCommandChanged()
        {
            var handler = SelectShipBuildProjectCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("SelectShipBuildProjectCommand");
        }

        public ICommand SelectShipBuildProjectCommand
        {
            get { return _selectShipBuildProjectCommand; }
            set
            {
                if (Equals(_selectShipBuildProjectCommand, value))
                    return;
                _selectShipBuildProjectCommand = value;
                OnSelectShipBuildProjectCommandChanged();
            }
        }
        #endregion

        #region ActivateOrbitalBatteryCommand Command
        private ICommand _activateOrbitalBatteryCommand;

        public event EventHandler ActivateOrbitalBatteryCommandChanged;

        private void OnActivateOrbitalBatteryCommandChanged()
        {
            var handler = ActivateOrbitalBatteryCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("ActivateOrbitalBatteryCommand");
        }

        public ICommand ActivateOrbitalBatteryCommand
        {
            get { return _activateOrbitalBatteryCommand; }
            set
            {
                if (Equals(_activateOrbitalBatteryCommand, value))
                    return;
                _activateOrbitalBatteryCommand = value;
                OnActivateOrbitalBatteryCommandChanged();
            }
        }
        #endregion

        #region DedeactivateOrbitalBatteryCommand Command
        private ICommand _deactivateOrbitalBatteryCommand;

        public event EventHandler DedeactivateOrbitalBatteryCommandChanged;

        private void OnDedeactivateOrbitalBatteryCommandChanged()
        {
            var handler = DedeactivateOrbitalBatteryCommandChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
            OnPropertyChanged("DedeactivateOrbitalBatteryCommand");
        }

        public ICommand DedeactivateOrbitalBatteryCommand
        {
            get { return _deactivateOrbitalBatteryCommand; }
            set
            {
                if (Equals(_deactivateOrbitalBatteryCommand, value))
                    return;
                _deactivateOrbitalBatteryCommand = value;
                OnDedeactivateOrbitalBatteryCommandChanged();
            }
        }
        #endregion

        #region MaxActiveOrbitalBatteries Property

        [field: NonSerialized]
        public event EventHandler MaxActiveOrbitalBatteriesChanged;

        private int _maxActiveOrbitalBatteries;

        public int MaxActiveOrbitalBatteries
        {
            get { return _maxActiveOrbitalBatteries; }
            set
            {
                if (Equals(value, _maxActiveOrbitalBatteries))
                    return;

                _maxActiveOrbitalBatteries = value;

                OnMaxActiveOrbitalBatteriesChanged();
            }
        }

        protected virtual void OnMaxActiveOrbitalBatteriesChanged()
        {
            MaxActiveOrbitalBatteriesChanged.Raise(this);
            OnPropertyChanged("MaxActiveOrbitalBatteries");
        }

        #endregion

        #region ActiveOrbitalBatteries Property

        [field: NonSerialized]
        public event EventHandler ActiveOrbitalBatteriesChanged;

        private int _activeOrbitalBatteries;

        public int ActiveOrbitalBatteries
        {
            get { return _activeOrbitalBatteries; }
            set
            {
                if (Equals(value, _activeOrbitalBatteries))
                    return;

                _activeOrbitalBatteries = value;

                OnActiveOrbitalBatteriesChanged();
            }
        }

        protected virtual void OnActiveOrbitalBatteriesChanged()
        {
            ActiveOrbitalBatteriesChanged.Raise(this);
            OnPropertyChanged("ActiveOrbitalBatteries");
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

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

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }
}