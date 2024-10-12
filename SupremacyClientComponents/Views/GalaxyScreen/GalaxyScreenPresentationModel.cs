// File:GalaxyScreenPresentationModel.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Entities;
using System.Windows.Media.Imaging;
using System.IO;
using Supremacy.Resources;
using Supremacy.Diplomacy;
using Supremacy.Client.Context;
using Supremacy.Game;
using Supremacy.Combat;

namespace Supremacy.Client.Views
{
    public class StationPresentationModel : PresentationModelBase
    {
        private Sector _sector = null;
        private Civilization _playerCiv = null;
        private string _stationStatus = "";
        private bool _stationScrapVisibility;

        public Sector Sector
        {
            get => _sector;
            set
            {
                _sector = value;
                Update();
            }
        }

        public Civilization PlayerCiv
        {
            get
            {
                _playerCiv = AppContext.LocalPlayer.Empire;
                return _playerCiv;
            }
            set
            {
                _playerCiv = value;
                Update();
            }
        }

        public string StationName { get; private set; } = "";

        public string StationStatus => _stationStatus;

        public BitmapImage StationImage { get; private set; } = null;

        public bool StationScrapVisibility
        {
            get
            {
                _stationScrapVisibility = Sector != null && Sector.Owner == PlayerCiv;
                return _stationScrapVisibility;
            }
        }


        public StationPresentationModel([NotNull] IAppContext appContext)
         : base(appContext)
        {

        }

        private void Update()
        {
            StationImage = null;
            StationName = "Unknown in station";
            _stationStatus = "";
            if (Sector == null || PlayerCiv == null)
            {
                return;
            }

            if (Sector.Station == null)
            {
                return;
            }

            string imagePath = "";

            if ((Sector.Station.Owner == PlayerCiv) || DiplomacyHelper.IsContactMade(PlayerCiv, Sector.Station.Owner))
            {
                StationName = Sector.Station.Name;
                _stationStatus = "Operational";
                imagePath = Sector.Station.Design.Image;
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                imagePath = "Resources/Images/Insignias/__unknown.png";
            }

            if (!Uri.TryCreate(imagePath, UriKind.Absolute, out Uri uri))
            {
                string tmpPath = ResourceManager.GetResourcePath(imagePath);
                if (!File.Exists(tmpPath))
                {
                    tmpPath = ResourceManager.GetResourcePath(@"Resources\Images\__image_missing.png");
                    if (!File.Exists(tmpPath))
                    {
                        return;
                    }
                }

                uri = ResourceManager.GetResourceUri(tmpPath);
            }
            StationImage = ImageCache.Current.Get(uri);
        }

        //#pragma warning disable IDE0051 // Remove unused private members
        //        private BitmapImage GetImage(string insigniaPath)
        //#pragma warning restore IDE0051 // Remove unused private members
        //        {
        //            Uri imageUri;
        //            //var imagePath = insigniaPath.ToLowerInvariant();

        //            if (File.Exists(ResourceManager.GetResourcePath(insigniaPath)))
        //                imageUri = ResourceManager.GetResourceUri(insigniaPath);
        //            else
        //                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");

        //            return ImageCache.Current.Get(imageUri);
        //        }
    }

    public class GalaxyScreenPresentationModel : PresentationModelBase
    {
        #region Fields
        private IEnumerable<Ship> _availableShips;
        private GalaxyScreenInputMode _inputMode;
        private GalaxyScreenOverviewMode _overviewMode;
        private Sector _selectedSector;
        private Sector _hoveredSector;
        private string _selectedSectorAllegiance;
        private string _selectedSectorInhabitants;
        private Ship _selectedShip;
        private ShipView _selectedShipInTaskForce;
        private IEnumerable<ShipView> _selectedShipsInTaskForce;
        private Ship _selectedShipResolved;
        private FleetViewWrapper _selectedTaskForce;
        private TradeRoute _selectedTradeRoute;
        private IEnumerable<FleetViewWrapper> _taskForces;
        private IEnumerable<FleetViewWrapper> _localPlayerTaskForces;
        private IEnumerable<FleetViewWrapper> _otherVisibleTaskForces;
        private IEnumerable<FleetViewWrapper> _iSpyTaskForces;
        private IEnumerable<TradeRoute> _tradeRoutes;
        private readonly EmpirePlayerStatusCollection _empirePlayers;
        #endregion

        #region Events
        public event EventHandler AvailableShipsChanged;
        public event EventHandler InputModeChanged;
        public event EventHandler OverviewModeChanged;
        public event EventHandler SelectedSectorAllegianceChanged;
        public event EventHandler SelectedSectorChanged;
        public event EventHandler HoveredSectorChanged;
        public event EventHandler SelectedSectorInhabitantsChanged;
        public event EventHandler SelectedShipChanged;
        public event EventHandler SelectedShipInTaskForceChanged;
        public event EventHandler SelectedShipsInTaskForceChanged;
        public event EventHandler SelectedTaskForceChanged;
        public event EventHandler TaskForcesChanged;
        public event EventHandler LocalPlayerTaskForcesChanged;
        public event EventHandler VisibleTaskForcesChanged;
        public event EventHandler SelectedTradeRouteChanged;
        public event EventHandler TradeRoutesChanged;
        public event EventHandler SelectedSectorStationChanged;
        #endregion

        #region Constructors and Finalizers
        public GalaxyScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext)
        {
            _empirePlayers = new EmpirePlayerStatusCollection();

            _empirePlayers.AddRange(
                from civ in appContext.CurrentGame.Civilizations
                where civ.IsEmpire
                select new EmpirePlayerStatus(appContext, civ)
                {
                    Player = appContext.Players.FirstOrDefault(o => o.EmpireID == civ.CivID)
                }
                );

            SelectedSectorStation = new StationPresentationModel(appContext);
        }
        #endregion

        #region Properties and Indexers
        public IEmpirePlayerStatusCollection EmpirePlayers => _empirePlayers;

        public IEnumerable<Ship> AvailableShips
        {
            get => _availableShips;
            set
            {
                if (Equals(_availableShips, value))
                {
                    return;
                }

                _availableShips = value;
                OnAvailableShipsChanged();
            }
        }

        public GalaxyScreenInputMode InputMode
        {
            get => _inputMode;
            set
            {
                if (Equals(_inputMode, value))
                {
                    return;
                }

                _inputMode = value;
                OnInputModeChanged();
            }
        }

        public GalaxyScreenOverviewMode OverviewMode
        {
            get => _overviewMode;
            set
            {
                if (Equals(_overviewMode, value))
                {
                    return;
                }

                _overviewMode = value;
                OnOverviewModeChanged();
            }
        }

        public Sector SelectedSector
        {
            get => _selectedSector;
            set
            {
                if (Equals(_selectedSector, value))
                {
                    return;
                }
                _selectedSector = value;
                OnSelectedSectorChanged();

                SelectedSectorStation.Sector = _selectedSector;
                // _stationScrapVisibility = _selectedSectorStation.ScrapVisibility;
                OnSelectedSectorStationChanged();
            }
        }

        public StationPresentationModel SelectedSectorStation { get; }

        public Sector HoveredSector
        {
            get => _hoveredSector;
            set
            {
                if (Equals(_hoveredSector, value))
                {
                    return;
                }

                _hoveredSector = value;
                OnHoveredSectorChanged();
            }
        }

        internal Sector SelectedSectorInternal
        {
            get => _selectedSector;
            set
            {
                _selectedSector = value;
                OnSelectedSectorChanged();

                SelectedSectorStation.Sector = _selectedSector;
                OnSelectedSectorStationChanged();
            }
        }

        public string SelectedSectorAllegiance
        {
            get => _selectedSectorAllegiance;
            set
            {
                if (Equals(_selectedSectorAllegiance, value))
                {
                    return;
                }

                _selectedSectorAllegiance = value;
                OnSelectedSectorAllegianceChanged();
            }
        }

        public string SelectedSectorInhabitants
        {
            get => _selectedSectorInhabitants;
            set
            {
                if (Equals(_selectedSectorInhabitants, value))
                {
                    return;
                }

                _selectedSectorInhabitants = value;
                OnSelectedSectorInhabitantsChanged();
            }
        }

        public Ship SelectedShip
        {
            get => _selectedShipResolved ?? _selectedShip;
            set
            {
                if (Equals(_selectedShip, value) && Equals(_selectedShipResolved, value))
                {
                    return;
                }

                _selectedShip = value;
                _selectedShipResolved = value;
                OnSelectedShipChanged();
            }
        }

        public ShipView SelectedShipInTaskForce
        {
            get => _selectedShipInTaskForce;
            set
            {
                if (Equals(_selectedShipInTaskForce, value))
                {
                    return;
                }

                _selectedShipInTaskForce = value;
                OnSelectedShipInTaskForceChanged();

                if ((_selectedShipInTaskForce == null) || !_selectedShipInTaskForce.IsOwned)
                {
                    return;
                }

                _selectedShipResolved = _selectedShipInTaskForce.Source;
                OnSelectedShipChanged();
            }
        }

        public IEnumerable<ShipView> SelectedShipsInTaskForce
        {
            get
            {
                Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                {
                    _selectedShipsInTaskForce = null; // Enumerable.Empty<ShipView>();
                }

                return _selectedShipsInTaskForce ?? Enumerable.Empty<ShipView>();
            }
            set
            {

                if (Equals(_selectedShipInTaskForce, value))
                {
                    return;
                }

                _selectedShipsInTaskForce = value;
                OnSelectedShipsInTaskForceChanged();
            }
        }

        public FleetViewWrapper SelectedTaskForce
        {
            get
            {
                if (_selectedSector != null && _selectedTaskForce != null)
                {

                    if (_selectedTaskForce.View != null && _selectedTaskForce.View.Ships != null && _selectedTaskForce.View.Ships.FirstOrDefault().Source != null)
                    {
                        Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                        Civilization owner = _selectedTaskForce.View.Ships.FirstOrDefault().Source.Owner;
                        if (owner != localPlayer && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                        {
                            _selectedTaskForce = null;
                        }
                    }
                }
                return _selectedTaskForce;
            }
            set
            {
                if (Equals(_selectedTaskForce, value))
                {
                    return;
                }

                _selectedTaskForce = value;
                OnSelectedTaskForceChanged();
            }
        }

        public TradeRoute SelectedTradeRoute
        {
            get => _selectedTradeRoute;
            set
            {
                if (Equals(_selectedTradeRoute, value))
                {
                    return;
                }

                _selectedTradeRoute = value;
                OnSelectedTradeRouteChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> TaskForces
        {
            get => _taskForces;
            set
            {
                if (Equals(_taskForces, value))
                {
                    return;
                }

                _taskForces = value;
                OnTaskForcesChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> LocalPlayerTaskForces
        {
            get => _localPlayerTaskForces;
            set
            {
                if (Equals(_localPlayerTaskForces, value))
                {
                    return;
                }

                _localPlayerTaskForces = value;
                OnLocalPlayerTaskForcesChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> VisibleTaskForces
        {
            get
            {
                if (_localPlayerTaskForces == null)
                {
                    return _otherVisibleTaskForces;
                }

                return _localPlayerTaskForces.Union(_otherVisibleTaskForces);
            }
            set
            {
                if (Equals(_otherVisibleTaskForces, value))
                {
                    return;
                }

                _otherVisibleTaskForces = value;
                OnVisibleTaskForcesChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> ISpyTaskForces
        {
            get => _iSpyTaskForces;  // do we need to union this to _localPlayerTaskFoces like above?
            set
            {
                if (Equals(_iSpyTaskForces, value))
                {
                    return;
                }

                _iSpyTaskForces = value;
                OnISpyTaskForcesChanged();
            }
        }

        public void GeneratePlayerTaskForces(Civilization playerCiv)
        {
            CivilizationMapData mapData = AppContext.LocalPlayerEmpire.MapData;
            List<FleetViewWrapper> playerList = new List<FleetViewWrapper>();
            List<FleetViewWrapper> otherVisibleList = new List<FleetViewWrapper>();
            List<FleetViewWrapper> iSpyList = new List<FleetViewWrapper>();

            if (TaskForces != null)
            {
                int count = 0;
                foreach (FleetViewWrapper fleetView in TaskForces)
                {
                    if (fleetView.View.Source.Owner == playerCiv)
                    {
                        fleetView.InsigniaImage = GetInsigniaImage(playerCiv.InsigniaPath);
                        playerList.Add(fleetView);
                    }
                    else if (SelectedSector.Station != null
                        && DiplomacyHelper.IsScanBlocked(playerCiv, fleetView.View.Source.Sector))
                    {
                        //works
                        //GameLog.Client.Intel.DebugFormat("local playerCiv = {0},. fleet Owner = {1}, counter = {2}, scanblock = {3}",

                        MapLocation location = SelectedSector.Station.Location;

                        if ((!DiplomacyHelper.AreAtWar(playerCiv, SelectedSector.Owner)
                            && !CombatHelper.WillFightAlongside(playerCiv, SelectedSector.Owner))
                            || (DiplomacyHelper.AreAtWar(playerCiv, SelectedSector.Owner) && fleetView.View.Source.Sector == SelectedSector.Station.Sector))
                        {
                            fleetView.IsUnScannable = true;
                            fleetView.InsigniaImage = GetInsigniaImage("Resources/Images/Insignias/_ScanBlock.png");
                            count++;
                            // works
                            // GameLog.Client.Intel.DebugFormat("IsUnScannable was True so got Insignia _ScanBlock & count++ ={0}", count);
                            iSpyList.Add(fleetView);
                        }
                        else
                        {
                            fleetView.InsigniaImage = GetInsigniaImage(fleetView.View.Source.Owner.InsigniaPath);
                        }
                    }
                    else if (mapData.GetScanStrength(fleetView.View.Source.Location) > 0)
                    {
                        if (!DiplomacyHelper.IsContactMade(playerCiv, fleetView.View.Source.Owner))
                        {
                            fleetView.IsUnknown = true;
                            fleetView.InsigniaImage = GetInsigniaImage("Resources/Images/Insignias/__unknown.png");
                            count++;

                        }
                        else
                        {
                            fleetView.InsigniaImage = GetInsigniaImage(fleetView.View.Source.Owner.InsigniaPath);
                        }
                    }

                    if (count <= 1)
                    {
                        otherVisibleList.Add(fleetView);
                        //GameLog.Client.Intel.DebugFormat("otherVisibleList count ={0}", otherVisibleList.Count);
                    }
                }
            }

            LocalPlayerTaskForces = playerList;
            VisibleTaskForces = otherVisibleList;
            ISpyTaskForces = iSpyList;
        }

        public BitmapImage GetInsigniaImage(string insigniaPath)
        {
            Uri imageUri = File.Exists(ResourceManager.GetResourcePath(insigniaPath))
                ? ResourceManager.GetResourceUri(insigniaPath)
                : ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");
            //var imagePath =   insigniaPath.ToLowerInvariant();


            return ImageCache.Current.Get(imageUri);
        }

        public IEnumerable<TradeRoute> TradeRoutes
        {
            get => _tradeRoutes;
            set
            {
                if (Equals(_tradeRoutes, value))
                {
                    return;
                }

                _tradeRoutes = value;
                OnTradeRoutesChanged();
            }
        }
        #endregion

        #region Private Methods

        private void OnAvailableShipsChanged()
        {
            AvailableShipsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnInputModeChanged()
        {
            InputModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnOverviewModeChanged()
        {
            OverviewModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedSectorAllegianceChanged()
        {
            SelectedSectorAllegianceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedSectorChanged()
        {
            SelectedSectorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnHoveredSectorChanged()
        {
            HoveredSectorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedSectorInhabitantsChanged()
        {
            SelectedSectorInhabitantsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedSectorStationChanged()
        {
            SelectedSectorStationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedShipChanged()
        {
            SelectedShipChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedShipInTaskForceChanged()
        {
            SelectedShipInTaskForceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedShipsInTaskForceChanged()
        {
            SelectedShipsInTaskForceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedTaskForceChanged()
        {
            SelectedTaskForceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedTradeRouteChanged()
        {
            SelectedTradeRouteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskForcesChanged()
        {
            TaskForcesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnLocalPlayerTaskForcesChanged()
        {
            LocalPlayerTaskForcesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnVisibleTaskForcesChanged()
        {
            VisibleTaskForcesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnISpyTaskForcesChanged()
        {
            VisibleTaskForcesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTradeRoutesChanged()
        {
            TradeRoutesChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}