// GalaxyScreenPresentationModel.cs
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
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class StationPresentationModel
    {
        private Sector _sector = null;
        private Civilization _playerCiv = null;

        private string _stationName = "";
        private string _stationStatus = "";
        private BitmapImage _stationImage = null;

        public Sector Sector
        {
            get { return _sector; }
            set 
            { 
                _sector = value;
                Update();
            }
        }

        public Civilization PlayerCiv
        {
            get { return _playerCiv; }
            set 
            { 
                _playerCiv = value;
                Update();
            }
        }

        public string StationName
        {
            get { return _stationName; }
        }

        public string StationStatus
        {
            get { return _stationStatus; }
        }

        public BitmapImage StationImage
        {
            get { return _stationImage; }
        }

        private void Update()
        {
            _stationImage = null;
            _stationName = "Unknown";
            _stationStatus = "";

            if (Sector == null || PlayerCiv == null)
                return;

            if (Sector.Station == null)
                return;

            string imagePath = "";

            if ((Sector.Station.Owner == PlayerCiv) || DiplomacyHelper.IsContactMade(PlayerCiv, Sector.Station.Owner))
            {
                _stationName = Sector.Station.Name;
                _stationStatus = "Operational";
                imagePath = Sector.Station.Design.Image;
            }

            if (string.IsNullOrEmpty(imagePath))
                imagePath = "Resources/Images/Insignias/__unknown.png";

            Uri uri;
            if (!Uri.TryCreate(imagePath, UriKind.Absolute, out uri))
            {
                string tmpPath = ResourceManager.GetResourcePath(imagePath);
                if (!File.Exists(tmpPath))
                {
                    tmpPath = ResourceManager.GetResourcePath(@"Resources\Images\__image_missing.png");
                    if (!File.Exists(tmpPath))
                        return;
                }

                uri = ResourceManager.GetResourceUri(tmpPath);
            }
            _stationImage = ImageCache.Current.Get(uri);
        }

        private BitmapImage GetImage(string insigniaPath)
        {
            Uri imageUri;
            var imagePath = insigniaPath.ToLowerInvariant();

            if (File.Exists(ResourceManager.GetResourcePath(insigniaPath)))
                imageUri = ResourceManager.GetResourceUri(insigniaPath);
            else
                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");

            return ImageCache.Current.Get(imageUri);
        }
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
        private IEnumerable<TradeRoute> _tradeRoutes;
        private readonly EmpirePlayerStatusCollection _empirePlayers;
        private StationPresentationModel _selectedSectorStation;
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

            _selectedSectorStation = new StationPresentationModel();
        }
        #endregion

        #region Properties and Indexers
        public IEmpirePlayerStatusCollection EmpirePlayers
        {
            get { return _empirePlayers; }
        }

        public IEnumerable<Ship> AvailableShips
        {
            get { return _availableShips; }
            set
            {
                if (Equals(_availableShips, value))
                    return;
                _availableShips = value;
                OnAvailableShipsChanged();
            }
        }

        public GalaxyScreenInputMode InputMode
        {
            get { return _inputMode; }
            set
            {
                if (Equals(_inputMode, value))
                    return;
                _inputMode = value;
                OnInputModeChanged();
            }
        }

        public GalaxyScreenOverviewMode OverviewMode
        {
            get { return _overviewMode; }
            set
            {
                if (Equals(_overviewMode, value))
                    return;
                _overviewMode = value;
                OnOverviewModeChanged();
            }
        }

        public Sector SelectedSector
        {
            get { return _selectedSector; }
            set
            {
                if (Equals(_selectedSector, value))
                    return;
                _selectedSector = value;
                OnSelectedSectorChanged();

                _selectedSectorStation.Sector = _selectedSector;
                OnSelectedSectorStationChanged();
            }
        }

        public StationPresentationModel SelectedSectorStation
        {
            get { return _selectedSectorStation; }
        }

        public Sector HoveredSector
        {
            get { return _hoveredSector; }
            set
            {
                if (Equals(_hoveredSector, value))
                    return;
                _hoveredSector = value;
                OnHoveredSectorChanged();
            }
        }

        internal Sector SelectedSectorInternal
        {
            get { return _selectedSector; }
            set
            {
                _selectedSector = value;
                OnSelectedSectorChanged();

                _selectedSectorStation.Sector = _selectedSector;
                OnSelectedSectorStationChanged();
            }
        }

        public string SelectedSectorAllegiance
        {
            get { return _selectedSectorAllegiance; }
            set
            {
                if (Equals(_selectedSectorAllegiance, value))
                    return;
                _selectedSectorAllegiance = value;
                OnSelectedSectorAllegianceChanged();
            }
        }

        public string SelectedSectorInhabitants
        {
            get { return _selectedSectorInhabitants; }
            set
            {
                if (Equals(_selectedSectorInhabitants, value))
                    return;
                _selectedSectorInhabitants = value;
                OnSelectedSectorInhabitantsChanged();
            }
        }

        public Ship SelectedShip
        {
            get
            {
                Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                    _selectedShipResolved = null;
                    _selectedShip = null;

                return _selectedShipResolved ?? _selectedShip;  
            }
            set
            {
                if (Equals(_selectedShip, value) && Equals(_selectedShipResolved, value))
                    return;
                _selectedShip = value;
                _selectedShipResolved = value;
                OnSelectedShipChanged();
            }
        }

        public ShipView SelectedShipInTaskForce
        {
            get
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //     return null; // Enumerable.Empty<ShipView>();

                return _selectedShipInTaskForce;
            }
            set
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if ( _selectedSector != null &&
                //     _selectedSector.Station != null &&
                //     _selectedSector.Station.Owner != localPlayer)
                //    return;

                ////_selectedShipInTaskForce = value;
                //OnSelectedShipInTaskForceChanged();
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    return;            

                if (Equals(_selectedShipInTaskForce, value))
                    return;

                _selectedShipInTaskForce = value;
                OnSelectedShipInTaskForceChanged();

                if ((_selectedShipInTaskForce == null) || !_selectedShipInTaskForce.IsOwned)
                    return;
                //if (_selectedShipInTaskForce.Source.Name.Contains("BLOCKED"))
                //    return;
                _selectedShipResolved = _selectedShipInTaskForce.Source;
                OnSelectedShipChanged();
            }
        }

        public IEnumerable<ShipView> SelectedShipsInTaskForce 
        {
            get
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    _selectedShipsInTaskForce = null; // Enumerable.Empty<ShipView>();

                 return _selectedShipsInTaskForce ?? Enumerable.Empty<ShipView>();
            }
            set
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    _selectedShipsInTaskForce = null;

                if (Equals(_selectedShipInTaskForce, value))
                    return;

                _selectedShipsInTaskForce = value;
                OnSelectedShipsInTaskForceChanged();
            }
        }

        public FleetViewWrapper SelectedTaskForce
        {
            get { return _selectedTaskForce; }
            set
            {
                //StationPresentationModel station = new StationPresentationModel();
                //   Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (
                //    _selectedSector != null && 
                //    _selectedSector.Station != null &&
                //    _selectedSector.Station.Owner != localPlayer) 
                //    return;
                if (Equals(_selectedTaskForce, value))
                    return;
                _selectedTaskForce = value;
                OnSelectedTaskForceChanged();
            }
        }

        public TradeRoute SelectedTradeRoute
        {
            get { return _selectedTradeRoute; }
            set
            {
                if (Equals(_selectedTradeRoute, value))
                    return;
                _selectedTradeRoute = value;
                OnSelectedTradeRouteChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> TaskForces
        {
            get { return _taskForces; }
            set
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    _taskForces = null;

                if (Equals(_taskForces, value))
                    return;
                _taskForces = value;
                OnTaskForcesChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> LocalPlayerTaskForces
        {
            get { return _localPlayerTaskForces; }
            set
            {
                if (Equals(_localPlayerTaskForces, value))
                    return;
                _localPlayerTaskForces = value;
                OnLocalPlayerTaskForcesChanged();
            }
        }

        public IEnumerable<FleetViewWrapper> VisibleTaskForces
        {
            get 
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    _otherVisibleTaskForces = null; // Enumerable.Empty<ShipView>();

                if (_localPlayerTaskForces == null)
                    return _otherVisibleTaskForces;
                return _localPlayerTaskForces.Union(_otherVisibleTaskForces);
            }
            set
            {
                //Civilization localPlayer = AppContext.LocalPlayerEmpire.Civilization;
                //if (_selectedShip != null && _selectedSector != null && DiplomacyHelper.IsScanBlocked(localPlayer, _selectedSector))
                //    _otherVisibleTaskForces = null; // Enumerable.Empty<ShipView>();

                if (Equals(_otherVisibleTaskForces, value))
                    return;
                _otherVisibleTaskForces = value;
                OnVisibleTaskForcesChanged();
            }
        }

        public void GeneratePlayerTaskForces(Civilization playerCiv)
        {
            _selectedSectorStation.PlayerCiv = playerCiv;

            var mapData = AppContext.LocalPlayerEmpire.MapData;

            List<FleetViewWrapper> playerList = new List<FleetViewWrapper>();
            List<FleetViewWrapper> otherVisibleList = new List<FleetViewWrapper>();

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
                    else if (mapData.GetScanStrength(fleetView.View.Source.Location) > 0)
                    {
                        if (!DiplomacyHelper.IsContactMade(playerCiv, fleetView.View.Source.Owner))
                        {
                            fleetView.IsUnknown = true;
                            fleetView.InsigniaImage = GetInsigniaImage("Resources/Images/Insignias/__unknown.png");
                            count++;
                       
                        }
                        else if (SelectedSector.System != null &&
                                SelectedSector.Owner != null &&
                                SelectedSector.System.Colony != null &&
                                GameContext.Current.Universe.HomeColonyLookup[SelectedSector.Owner] == SelectedSector.System.Colony &&
                                DiplomacyHelper.IsScanBlocked(playerCiv, fleetView.View.Source.Sector))
                        { 
                            GameLog.Client.Intel.DebugFormat("local playerCiv ={0},. fleet Owner ={1}, counter ={2}, scanblock ={3}, home colony ={4}",
                                playerCiv, fleetView.View.Source.Owner, count, DiplomacyHelper.IsScanBlocked(playerCiv, fleetView.View.Source.Sector), GameContext.Current.Universe.HomeColonyLookup[SelectedSector.Owner].Name);
                     
                            if (!DiplomacyHelper.AreAtWar(playerCiv, SelectedSector.Owner)) // && DiplomacyHelper.IsScanBlocked(playerCiv, fleetView.View.Source.Sector))
                            {
                                //GameLog.Core.Combat.DebugFormat("Home Colony found = {0}, Not at war ={1}", GameContext.Current.Universe.HomeColonyLookup[SelectedSector.Owner] == SelectedSector.System.Colony,!DiplomacyHelper.AreAtWar(playerCiv, SelectedSector.Owner));

                                fleetView.IsUnScannable = true;
                                fleetView.InsigniaImage = GetInsigniaImage("Resources/Images/Insignias/_Pirates.png");
                                count++;
                                GameLog.Client.Intel.DebugFormat("IsUnScannable was True so got Insignia Pirate & count++ ={0}", count );
                            }
                            else fleetView.InsigniaImage = GetInsigniaImage(fleetView.View.Source.Owner.InsigniaPath);
                        }
                        else fleetView.InsigniaImage = GetInsigniaImage(fleetView.View.Source.Owner.InsigniaPath);
                        //if (count <= 1)
                        //{
                        //    otherVisibleList.Add(fleetView);
                        //    GameLog.Client.Intel.DebugFormat("otherVisibleList count ={0}", otherVisibleList.Count);
                        //}
                    }
                    if (count <= 1)
                    {
                        otherVisibleList.Add(fleetView);
                        GameLog.Client.Intel.DebugFormat("otherVisibleList count ={0}", otherVisibleList.Count);
                    }
                }
            }

            LocalPlayerTaskForces = playerList;
            VisibleTaskForces = otherVisibleList;
        }

        public BitmapImage GetInsigniaImage(string insigniaPath)
        {
            Uri imageUri;
            var imagePath = insigniaPath.ToLowerInvariant();

            if (File.Exists(ResourceManager.GetResourcePath(insigniaPath)))
                imageUri = ResourceManager.GetResourceUri(insigniaPath);
            else
                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");

            return ImageCache.Current.Get(imageUri);
        }

        public IEnumerable<TradeRoute> TradeRoutes
        {
            get { return _tradeRoutes; }
            set
            {
                if (Equals(_tradeRoutes, value))
                    return;
                _tradeRoutes = value;
                OnTradeRoutesChanged();
            }
        }
        #endregion

        #region Private Methods
        private void OnAvailableShipsChanged()
        {
            var handler = AvailableShipsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
        private void OnInputModeChanged()
        {
            var handler = InputModeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnOverviewModeChanged()
        {
            var handler = OverviewModeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedSectorAllegianceChanged()
        {
            var handler = SelectedSectorAllegianceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedSectorChanged()
        {
            var handler = SelectedSectorChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnHoveredSectorChanged()
        {
            var handler = HoveredSectorChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedSectorInhabitantsChanged()
        {
            var handler = SelectedSectorInhabitantsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedSectorStationChanged()
        {
            var handler = SelectedSectorStationChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedShipChanged()
        {
            var handler = SelectedShipChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
        private void OnSelectedShipInTaskForceChanged()
        {
            var handler = SelectedShipInTaskForceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedShipsInTaskForceChanged()
        {
            var handler = SelectedShipsInTaskForceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedTaskForceChanged()
        {
            var handler = SelectedTaskForceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnSelectedTradeRouteChanged()
        {
            var handler = SelectedTradeRouteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnTaskForcesChanged()
        {
            var handler = TaskForcesChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnLocalPlayerTaskForcesChanged()
        {
            var handler = LocalPlayerTaskForcesChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnVisibleTaskForcesChanged()
        {
            var handler = VisibleTaskForcesChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
        private void OnTradeRoutesChanged()
        {
            var handler = TradeRoutesChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }
}