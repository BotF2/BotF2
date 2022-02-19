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
            SelectedColonyChanged?.Invoke(this, new PropertyChangedRoutedEventArgs<Colony>(oldValue, newValue));
            OnPropertyChanged("SelectedColony");
        }

        public bool AddShipToQueue => true;

        public Colony SelectedColony
        {
            get => _selectedColony;
            set
            {
                Colony oldValue = _selectedColony;
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
            ColoniesChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("Colonies");
        }

        public IEnumerable<Colony> Colonies
        {
            get => _colonies;
            set
            {
                if (Equals(_colonies, value))
                {
                    return;
                }

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
            SelectedPlanetaryBuildProjectChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("SelectedPlanetaryBuildProject");
        }

        public BuildProject SelectedPlanetaryBuildProject
        {
            get => _selectedPlanetaryBuildProject;
            set
            {
                if (Equals(_selectedPlanetaryBuildProject, value))
                {
                    return;
                }

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
            SelectedShipyardBuildProjectChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("SelectedShipyardBuildProject");
        }

        public BuildProject SelectedShipyardBuildProject
        {
            get => _selectedShipyardBuildProject;
            set
            {
                if (Equals(_selectedShipyardBuildProject, value))
                {
                    return;
                }

                _selectedShipyardBuildProject = value;
                OnSelectedShipyardBuildProjectChanged();
            }
        }
        #endregion

        //#region SelectedBuildSlotQueueProject Property
        //private BuildProject _selectedBuildSlotQueueProject;

        //public event EventHandler SelectedBuildSlotQueueProjectChanged;

        //private void OnSelectedBuildSlotQueueProjectChanged()
        //{
        //    var handler = SelectedBuildSlotQueueProjectChanged;
        //    if (handler != null)
        //        handler(this, EventArgs.Empty);
        //    OnPropertyChanged("SelectedBuildSlotQueueProject");
        //}

        //public BuildProject SelectedBuildSlotQueueProject
        //{
        //    get { return _selectedBuildSlotQueueProject; }
        //    set
        //    {
        //        if (Equals(_selectedBuildSlotQueueProject, value))
        //            return;
        //        _selectedBuildSlotQueueProject = value;
        //        OnSelectedBuildSlotQueueProjectChanged();
        //    }
        //}
        //#endregion

        #region SelectedShipyardBuildSlot Property
        private ShipyardBuildSlot _selectedShipyardBuildSlot;

        public event EventHandler SelectedShipyardBuildSlotChanged;

        private void OnSelectedShipyardBuildSlotChanged()
        {
            SelectedShipyardBuildSlotChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("SelectedShipyardBuildSlot");
        }

        public ShipyardBuildSlot SelectedShipyardBuildSlot
        {
            get => _selectedShipyardBuildSlot;
            set
            {
                if (Equals(_selectedShipyardBuildSlot, value))
                {
                    return;
                }

                _selectedShipyardBuildSlot = value;
                OnSelectedShipyardBuildSlotChanged();
            }
        }
        #endregion

        #region PlanetaryBuildProjects Property
        private IEnumerable<BuildProject> _planetaryBuildProjects;

        public event EventHandler PlanetaryBuildProjectsChanged;

        private void OnPlanetaryBuildProjectsChanged()
        {
            PlanetaryBuildProjectsChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("PlanetaryBuildProjects");
        }

        public IEnumerable<BuildProject> PlanetaryBuildProjects
        {
            get => _planetaryBuildProjects;
            set
            {
                if (Equals(_planetaryBuildProjects, value))
                {
                    return;
                }

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
            ShipyardBuildProjectsChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ShipyardBuildProjects");
        }

        public IEnumerable<BuildProject> ShipyardBuildProjects
        {
            get => _shipyardBuildProjects;
            set
            {
                if (Equals(_shipyardBuildProjects, value))
                {
                    return;
                }

                _shipyardBuildProjects = value;
                OnShipyardBuildProjectsChanged();
            }
        }
        #endregion

        //#region BuildSlotQueueProjects Property
        //private IEnumerable<BuildProject> _buildSlotQueueProjects;

        //public event EventHandler BuildSlotQueueProjectsChanged;

        //private void OnBuildSlotQueueProjectsChanged()
        //{
        //    var handler = BuildSlotQueueProjectsChanged;
        //    if (handler != null)
        //        handler(this, EventArgs.Empty);
        //    OnPropertyChanged("BuildSlotQueueProjects");
        //}

        //public IEnumerable<BuildProject> BuildSlotQueueProjects
        //{
        //    get { return _buildSlotQueueProjects; }
        //    set
        //    {
        //        if (Equals(_buildSlotQueueProjects, value))
        //            return;
        //        _buildSlotQueueProjects = value;
        //        OnBuildSlotQueueProjectsChanged();
        //    }
        //}
        //#endregion

        #region AddToPlanetaryBuildQueue Command
        public event EventHandler AddToPlanetaryBuildQueueCommandChanged;

        private ICommand _addToPlanetaryBuildQueueCommand;

        private void OnAddToPlanetaryBuildQueueCommandChanged()
        {
            AddToPlanetaryBuildQueueCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("AddToPlanetaryBuildQueueCommand");
        }

        public ICommand AddToPlanetaryBuildQueueCommand
        {
            get => _addToPlanetaryBuildQueueCommand;
            set
            {
                if (Equals(_addToPlanetaryBuildQueueCommand, value))
                {
                    return;
                }

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
            RemoveFromPlanetaryBuildQueueCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("RemoveFromPlanetaryBuildQueueCommand");
        }

        public ICommand RemoveFromPlanetaryBuildQueueCommand
        {
            get => _removeFromPlanetaryBuildQueueCommand;
            set
            {
                if (Equals(_removeFromPlanetaryBuildQueueCommand, value))
                {
                    return;
                }

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
            AddToShipyardBuildQueueCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("AddToShipyardBuildQueueCommand");
        }

        public ICommand AddToShipyardBuildQueueCommand
        {
            get => _addToShipyardBuildQueueCommand;
            set
            {
                if (Equals(_addToShipyardBuildQueueCommand, value))
                {
                    return;
                }

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
            RemoveFromShipyardBuildQueueCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("RemoveFromShipyardBuildQueueCommand");
        }

        public ICommand RemoveFromShipyardBuildQueueCommand
        {
            get => _removeFromShipyardBuildQueueCommand;
            set
            {
                if (Equals(_removeFromShipyardBuildQueueCommand, value))
                {
                    return;
                }

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
            CancelBuildProjectCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("CancelBuildProjectCommand");
        }

        private void OnBuyBuildProjectCommandChanged()
        {
            BuyBuildProjectCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("BuyBuildProjectCommand");
        }

        public ICommand CancelBuildProjectCommand
        {
            get => _cancelBuildProjectCommand;
            set
            {
                if (Equals(_cancelBuildProjectCommand, value))
                {
                    return;
                }

                _cancelBuildProjectCommand = value;
                OnCancelBuildProjectCommandChanged();
            }
        }

        public ICommand BuyBuildProjectCommand
        {
            get => _buyBuildProjectCommand;
            set
            {
                if (Equals(_buyBuildProjectCommand, value))
                {
                    return;
                }

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
            ActivateFacilityCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ActivateFacilityCommand");
        }

        public ICommand ActivateFacilityCommand
        {
            get => _activateFacilityCommand;
            set
            {
                if (Equals(_activateFacilityCommand, value))
                {
                    return;
                }

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
            DeactivateFacilityCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("DeactivateFacilityCommand");
        }

        public ICommand DeactivateFacilityCommand
        {
            get => _deactivateFacilityCommand;
            set
            {
                if (Equals(_deactivateFacilityCommand, value))
                {
                    return;
                }

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
            ScrapFacilityCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ScrapFacilityCommand");
        }

        public ICommand ScrapFacilityCommand
        {
            get => _scrapFacilityCommand;
            set
            {
                if (Equals(_scrapFacilityCommand, value))
                {
                    return;
                }

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
            UnscrapFacilityCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("UnscrapFacilityCommand");
        }

        public ICommand UnscrapFacilityCommand
        {
            get => _unscrapFacilityCommand;
            set
            {
                if (Equals(_unscrapFacilityCommand, value))
                {
                    return;
                }

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
            ToggleBuildingScrapCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ToggleBuildingScrapCommand");
        }

        public ICommand ToggleBuildingScrapCommand
        {
            get => _toggleBuildingScrapCommand;
            set
            {
                if (Equals(_toggleBuildingScrapCommand, value))
                {
                    return;
                }

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
            ToggleBuildingIsActiveCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ToggleBuildingIsActiveCommand");
        }

        public ICommand ToggleBuildingIsActiveCommand
        {
            get => _toggleBuildingIsActiveCommand;
            set
            {
                if (Equals(_toggleBuildingIsActiveCommand, value))
                {
                    return;
                }

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
            ToggleShipyardBuildSlotCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ToggleShipyardBuildSlotCommand");
        }

        public ICommand ToggleShipyardBuildSlotCommand
        {
            get => _toggleShipyardBuildSlotCommand;
            set
            {
                if (Equals(_toggleShipyardBuildSlotCommand, value))
                {
                    return;
                }

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
            SelectShipBuildProjectCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("SelectShipBuildProjectCommand");
        }

        public ICommand SelectShipBuildProjectCommand
        {
            get => _selectShipBuildProjectCommand;
            set
            {
                if (Equals(_selectShipBuildProjectCommand, value))
                {
                    return;
                }

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
            ActivateOrbitalBatteryCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("ActivateOrbitalBatteryCommand");
        }

        public ICommand ActivateOrbitalBatteryCommand
        {
            get => _activateOrbitalBatteryCommand;
            set
            {
                if (Equals(_activateOrbitalBatteryCommand, value))
                {
                    return;
                }

                _activateOrbitalBatteryCommand = value;
                OnActivateOrbitalBatteryCommandChanged();
            }
        }
        #endregion

        #region DeactivateOrbitalBatteryCommand Command
        private ICommand _deactivateOrbitalBatteryCommand;

        public event EventHandler DeactivateOrbitalBatteryCommandChanged;

        private void OnDeactivateOrbitalBatteryCommandChanged()
        {
            DeactivateOrbitalBatteryCommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged("DeactivateOrbitalBatteryCommand");
        }

        public ICommand DeactivateOrbitalBatteryCommand
        {
            get => _deactivateOrbitalBatteryCommand;
            set
            {
                if (Equals(_deactivateOrbitalBatteryCommand, value))
                {
                    return;
                }

                _deactivateOrbitalBatteryCommand = value;
                OnDeactivateOrbitalBatteryCommandChanged();
            }
        }
        #endregion

        #region MaxActiveOrbitalBatteries Property

        [field: NonSerialized]
        public event EventHandler MaxActiveOrbitalBatteriesChanged;

        private int _maxActiveOrbitalBatteries;

        public int MaxActiveOrbitalBatteries
        {
            get => _maxActiveOrbitalBatteries;
            set
            {
                if (Equals(value, _maxActiveOrbitalBatteries))
                {
                    return;
                }

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

        #region ActiveFoodFacilites Property

        [field: NonSerialized]
        public event EventHandler ActiveFoodFacilitesChanged;

        private int _activeFoodFacilites;

        public int ActiveFoodFacilites
        {
            get => _activeFoodFacilites;
            set
            {
                if (Equals(value, _activeFoodFacilites))
                {
                    return;
                }

                _activeFoodFacilites = value;

                OnActiveFoodFacilitesChanged();
            }
        }

        protected virtual void OnActiveFoodFacilitesChanged()
        {
            ActiveFoodFacilitesChanged.Raise(this);
            OnPropertyChanged("ActiveFoodFacilites");
        }

        #endregion

        #region ActiveIndustryFacilites Property

        [field: NonSerialized]
        public event EventHandler ActiveIndustryFacilitesChanged;

        private int _activeIndustryFacilites;

        public int ActiveIndustryFacilites
        {
            get => _activeIndustryFacilites;
            set
            {
                if (Equals(value, _activeIndustryFacilites))
                {
                    return;
                }

                _activeIndustryFacilites = value;

                OnActiveIndustryFacilitesChanged();
            }
        }

        protected virtual void OnActiveIndustryFacilitesChanged()
        {
            ActiveIndustryFacilitesChanged.Raise(this);
            OnPropertyChanged("ActiveIndustryFacilites");
        }

        #endregion

        #region ActiveEnergyFacilites Property

        [field: NonSerialized]
        public event EventHandler ActiveEnergyFacilitesChanged;

        private int _activeEnergyFacilites;

        public int ActiveEnergyFacilites
        {
            get => _activeEnergyFacilites;
            set
            {
                if (Equals(value, _activeEnergyFacilites))
                {
                    return;
                }

                _activeEnergyFacilites = value;

                OnActiveEnergyFacilitesChanged();
            }
        }

        protected virtual void OnActiveEnergyFacilitesChanged()
        {
            ActiveEnergyFacilitesChanged.Raise(this);
            OnPropertyChanged("ActiveEnergyFacilites");
        }

        #endregion

        #region ActiveResearchFacilites Property

        [field: NonSerialized]
        public event EventHandler ActiveResearchFacilitesChanged;

        private int _activeResearchFacilites;

        public int ActiveResearchFacilites
        {
            get => _activeResearchFacilites;
            set
            {
                if (Equals(value, _activeResearchFacilites))
                {
                    return;
                }

                _activeResearchFacilites = value;

                OnActiveResearchFacilitesChanged();
            }
        }

        protected virtual void OnActiveResearchFacilitesChanged()
        {
            ActiveResearchFacilitesChanged.Raise(this);
            OnPropertyChanged("ActiveResearchFacilites");
        }

        #endregion

        #region ActiveIntelligenceFacilites Property

        [field: NonSerialized]
        public event EventHandler ActiveIntelligenceFacilitesChanged;

        private int _activeIntelligenceFacilites;

        public int ActiveIntelligenceFacilites
        {
            get => _activeIntelligenceFacilites;
            set
            {
                if (Equals(value, _activeIntelligenceFacilites))
                {
                    return;
                }

                _activeIntelligenceFacilites = value;

                OnActiveIntelligenceFacilitesChanged();
            }
        }

        protected virtual void OnActiveIntelligenceFacilitesChanged()
        {
            ActiveIntelligenceFacilitesChanged.Raise(this);
            OnPropertyChanged("ActiveIntelligenceFacilites");
        }

        #endregion

        #region ActiveOrbitalBatteries Property

        [field: NonSerialized]
        public event EventHandler ActiveOrbitalBatteriesChanged;

        private int _activeOrbitalBatteries;

        public int ActiveOrbitalBatteries
        {
            get => _activeOrbitalBatteries;
            set
            {
                if (Equals(value, _activeOrbitalBatteries))
                {
                    return;
                }

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
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
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