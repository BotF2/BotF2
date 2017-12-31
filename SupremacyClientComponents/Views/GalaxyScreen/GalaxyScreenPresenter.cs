// GalaxyScreenPresenter.cs
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
using System.Windows;

using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Utility;

using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Client.Services;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Universe;

using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;

namespace Supremacy.Client.Views
{
    public class GalaxyScreenPresenter : GameScreenPresenterBase<GalaxyScreenPresentationModel, IGalaxyScreenView>, IGalaxyScreenPresenter
    {
        #region Fields
        private readonly DelegateCommand<RedeployShipCommandArgs> _addShipToTaskForceCommand;
        //private readonly DelegateCommand<RedeployIntelCommandArgs> _addIntelToIntelForceCommand;
        private readonly DelegateCommand<TradeRoute> _cancelTradeRouteCommand;
        private readonly DelegateCommand<Pair<FleetView, FleetOrder>> _issueTaskForceOrderCommand;
        //private readonly DelegateCommand<Pair<IntelFleetView, FleetOrder>> _issueIntelForceOrderCommand;
        private readonly DelegateCommand<RedeployShipCommandArgs> _removeShipFromTaskForceCommand;
        private readonly DelegateCommand<GalaxyScreenInputMode> _setInputModeCommand;
        private readonly DelegateCommand<GalaxyScreenOverviewMode> _setOverviewModeCommand;
        private readonly DelegateCommand<FleetView> _toggleTaskForceCloakCommand;
        private readonly DelegateCommand<FleetView> _toggleTaskForceCamouflageCommand;
        //private readonly DelegateCommand<IntelFleetView> _toggleIntelForceCloakCommand;
        private readonly DelegateCommand<ICheckableCommandParameter> _scrapCommand;
        private readonly List<IDisposable> _channelSubscriptions;
        private INavigationService _navigationService = null;
        private ISoundPlayer _soundPlayer = null;
        #endregion

        #region Constructors and Finalizers
        public GalaxyScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] GalaxyScreenPresentationModel model,
            [NotNull] IGalaxyScreenView view,
            [NotNull] INavigationService navigationService,
            [NotNull] ISoundPlayer soundPlayer)
            : base(container, model, view)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (model == null)
                throw new ArgumentNullException("model");
            if (view == null)
                throw new ArgumentNullException("view");
            if (navigationService == null)
                throw new ArgumentNullException("navigationService");
            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");

            _channelSubscriptions = new List<IDisposable>();

            _setInputModeCommand = new DelegateCommand<GalaxyScreenInputMode>(mode => Model.InputMode = mode);

            _setOverviewModeCommand = new DelegateCommand<GalaxyScreenOverviewMode>(mode => Model.OverviewMode = mode);

            _addShipToTaskForceCommand = new DelegateCommand<RedeployShipCommandArgs>(
                ExecuteAddShipToTaskForceCommand,
                CanExecuteAddShipToTaskForceCommand);

            _removeShipFromTaskForceCommand = new DelegateCommand<RedeployShipCommandArgs>(
                ExecuteRemoveShipFromTaskForceCommand,
                CanExecuteRemoveShipFromTaskForceCommand);

            _issueTaskForceOrderCommand = new DelegateCommand<Pair<FleetView, FleetOrder>>(
                ExecuteIssueTaskForceOrderCommand);

            _toggleTaskForceCloakCommand = new DelegateCommand<FleetView>(
                ExecuteToggleTaskForceCloakCommand);

            _toggleTaskForceCamouflageCommand = new DelegateCommand<FleetView>(
                ExecuteToggleTaskForceCamouflageCommand);

            _cancelTradeRouteCommand = new DelegateCommand<TradeRoute>(
                ExecuteCancelTradeRouteCommand,
                tradeRoute => (tradeRoute != null) && tradeRoute.IsAssigned);

            _scrapCommand = new DelegateCommand<ICheckableCommandParameter>(
                ExecuteScrapCommand,
                CanExecuteScrapCommand);

            _navigationService = navigationService;

            _soundPlayer = soundPlayer;
        }
        #endregion

        #region Public and Protected Methods
        protected override string ViewName
        {
            get { return StandardGameScreens.GalaxyScreen; }
        }

        protected override void InvalidateCommands()
        {
            _addShipToTaskForceCommand.RaiseCanExecuteChanged();
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
            _issueTaskForceOrderCommand.RaiseCanExecuteChanged();
            ////_issueIntelForceOrderCommand.RaiseCanExecuteChanged();
            _removeShipFromTaskForceCommand.RaiseCanExecuteChanged();
            _setInputModeCommand.RaiseCanExecuteChanged();
            _setOverviewModeCommand.RaiseCanExecuteChanged();
            _toggleTaskForceCloakCommand.RaiseCanExecuteChanged();
            _toggleTaskForceCamouflageCommand.RaiseCanExecuteChanged();
            _scrapCommand.RaiseCanExecuteChanged();
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            Model.InputModeChanged += OnInputModeChanged;
            Model.OverviewModeChanged += OnOverviewModeChanged;
            Model.SelectedSectorChanged += OnSelectedSectorChanged;
            Model.TaskForcesChanged += OnTaskForcesChanged;
            //this.Model.IntelForcesChanged += OnIntelForcesChanged;
            Model.SelectedTaskForceChanged += OnSelectedTaskForceChanged;
            //this.Model.SelectedIntelForceChanged += OnSelectedIntelForceChanged;
            Model.AvailableShipsChanged += OnAvailableShipsChanged;
            //this.Model.AvailableIntelsChanged += OnAvailableIntelsChanged;
            Model.TradeRoutesChanged += OnTradeRoutesChanged;
            Model.SelectedShipChanged += OnSelectedShipChanged;
            Model.SelectedTradeRouteChanged += OnSelectedTradeRouteChanged;

            GalaxyScreenCommands.SetInputMode.RegisterCommand(_setInputModeCommand);
            GalaxyScreenCommands.SetOverviewMode.RegisterCommand(_setOverviewModeCommand);
            GalaxyScreenCommands.IssueTaskForceOrder.RegisterCommand(_issueTaskForceOrderCommand);
            GalaxyScreenCommands.ToggleTaskForceCloak.RegisterCommand(_toggleTaskForceCloakCommand);
            GalaxyScreenCommands.ToggleTaskForceCamouflage.RegisterCommand(_toggleTaskForceCamouflageCommand);
            GalaxyScreenCommands.CancelTradeRoute.RegisterCommand(_cancelTradeRouteCommand);
            GalaxyScreenCommands.AddShipToTaskForce.RegisterCommand(_addShipToTaskForceCommand);
            GalaxyScreenCommands.RemoveShipFromTaskForce.RegisterCommand(_removeShipFromTaskForceCommand);
            GalaxyScreenCommands.Scrap.RegisterCommand(_scrapCommand);

            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
            ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded, ThreadOption.UIThread);
            ClientEvents.LobbyUpdated.Subscribe(OnLobbyUpdated, ThreadOption.UIThread);
            //ClientEvents.PlayerTurnFinished.Subscribe(OnPlayerTurnFinished, ThreadOption.UIThread);
            PlayerActionEvents.FleetRouteUpdated.Subscribe(OnFleetRouteUpdated, ThreadOption.UIThread);
            GameEvents.TradeRouteEstablished.Subscribe(OnTradeRouteChanged, ThreadOption.UIThread);
            GameEvents.TradeRouteCancelled.Subscribe(OnTradeRouteChanged, ThreadOption.UIThread);

            _channelSubscriptions.Add(
                Channel<PlayerTurnFinishedMessage>.Public.Subscribe(
                    message => Model.EmpirePlayers.UpdatePlayerReadiness(message.Player),
                    threadOption: ChannelThreadOption.UIThread));

            _channelSubscriptions.Add(
                Channel<TurnStartedMessage>.Public.Subscribe(
                    message => OnTurnStarted(),
                    threadOption: ChannelThreadOption.UIThread));

            base.RegisterCommandAndEventHandlers();
        }

        protected override void UnregisterCommandAndEventHandlers()
        {
            Model.InputModeChanged -= OnInputModeChanged;
            Model.OverviewModeChanged -= OnOverviewModeChanged;
            Model.SelectedSectorChanged -= OnSelectedSectorChanged;
            Model.TaskForcesChanged -= OnTaskForcesChanged;
            Model.SelectedTaskForceChanged -= OnSelectedTaskForceChanged;
            Model.AvailableShipsChanged -= OnAvailableShipsChanged;
            //this.Model.AvailableIntelsChanged -= OnAvailableIntelsChanged;
            Model.TradeRoutesChanged -= OnTradeRoutesChanged;
            Model.SelectedShipChanged -= OnSelectedShipChanged;
            Model.SelectedTradeRouteChanged -= OnSelectedTradeRouteChanged;

            GalaxyScreenCommands.SetInputMode.UnregisterCommand(_setInputModeCommand);
            GalaxyScreenCommands.SetOverviewMode.UnregisterCommand(_setOverviewModeCommand);
            GalaxyScreenCommands.IssueTaskForceOrder.UnregisterCommand(_issueTaskForceOrderCommand);
            GalaxyScreenCommands.ToggleTaskForceCloak.UnregisterCommand(_toggleTaskForceCloakCommand);
            GalaxyScreenCommands.ToggleTaskForceCamouflage.UnregisterCommand(_toggleTaskForceCamouflageCommand);
            GalaxyScreenCommands.CancelTradeRoute.UnregisterCommand(_cancelTradeRouteCommand);
            GalaxyScreenCommands.AddShipToTaskForce.UnregisterCommand(_addShipToTaskForceCommand);
            GalaxyScreenCommands.RemoveShipFromTaskForce.UnregisterCommand(_removeShipFromTaskForceCommand);
            GalaxyScreenCommands.Scrap.UnregisterCommand(_scrapCommand);

            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
            ClientEvents.AllTurnEnded.Unsubscribe(OnAllTurnEnded);
            ClientEvents.LobbyUpdated.Unsubscribe(OnLobbyUpdated);
            //ClientEvents.PlayerTurnFinished.Unsubscribe(OnPlayerTurnFinished);
            PlayerActionEvents.FleetRouteUpdated.Unsubscribe(OnFleetRouteUpdated);
            GameEvents.TradeRouteEstablished.Unsubscribe(OnTradeRouteChanged);
            GameEvents.TradeRouteCancelled.Unsubscribe(OnTradeRouteChanged);

            foreach (var subscription in _channelSubscriptions)
                subscription.Dispose();

            _channelSubscriptions.Clear();

            base.UnregisterCommandAndEventHandlers();
        }
        #endregion

        #region Private Methods
        private bool CanExecuteAddShipToTaskForceCommand(RedeployShipCommandArgs args)
        {
            //if (this.Model.InputMode != GalaxyScreenInputMode.RedeployShips)
            //    return false;
            if (args == null)
                return false;
            if (args.Ship.OwnerID != AppContext.LocalPlayer.EmpireID)
                return false;
            if (Model.SelectedTaskForce == null)
                return false;
            if (!Model.SelectedTaskForce.View.Ships.Any(o => Equals(o.Source, o)))
                return false;
            return true;
        }

        private bool CanExecuteRemoveShipFromTaskForceCommand(RedeployShipCommandArgs args)
        {
            //if (this.Model.InputMode != GalaxyScreenInputMode.RedeployShips)
            //    return false;
            if (args == null)
                return false;
            if (args.Ship.OwnerID != AppContext.LocalPlayer.EmpireID)
                return false;
            if (Model.SelectedTaskForce == null)
                return false;
            if (!Model.SelectedTaskForce.View.Ships.Any(o => Equals(o.Source, o)))
                return false;
            return true;
        }

        private void ExecuteAddShipToTaskForceCommand(RedeployShipCommandArgs args)
        {
            PlayerOperations.RedeployShip(args.Ship, args.TargetFleet);
            
            RefreshTaskForceList();

            //RefreshIntelForceList();

            Model.SelectedTaskForce = null;

            if (Model.InputMode == GalaxyScreenInputMode.RedeployShips)
                return;

            Model.SelectedTaskForce = Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, args.Ship.Fleet));
        }

        private void ExecuteRemoveShipFromTaskForceCommand(RedeployShipCommandArgs args)
        {
            var selectedTaskForce = args.Ship.Fleet;
            if (selectedTaskForce == null)
                return;

            PlayerOperations.RedeployShip(args.Ship);

            RefreshTaskForceList();

            //RefreshIntelForceList();

            Model.SelectedTaskForce = null;

            if (Model.InputMode == GalaxyScreenInputMode.RedeployShips)
                return;

            if (selectedTaskForce.Ships.Any())
                Model.SelectedTaskForce = Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, selectedTaskForce));
            else
                Model.SelectedTaskForce = Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, args.Ship.Fleet));
        }

        private void ExecuteCancelTradeRouteCommand(TradeRoute tradeRoute)
        {
            if ((tradeRoute == null) || !tradeRoute.IsAssigned)
                return;

            tradeRoute.TargetColony = null;
            GameEvents.TradeRouteCancelled.Publish(tradeRoute);

            PlayerOrderService.AddOrder(new SetTradeRouteOrder(tradeRoute));
            Model.SelectedTradeRoute = null;
        }

        private static bool CanExecuteScrapCommand(ICheckableCommandParameter args)
        {
            if (args == null)
                return false;

            var scrapCommandArgs = args as ScrapCommandArgs;
            if (scrapCommandArgs != null)
                return scrapCommandArgs.Objects.Any();

            var techObject = args.InnerParameter as TechObject;
            if (techObject != null)
                return true;

            var techObjects = args.InnerParameter as IEnumerable<TechObject>;
            if (techObjects != null)
                return techObjects.Any();

            return false;
        }

        private static void ExecuteScrapCommand(ICheckableCommandParameter args)
        {
            if (args == null)
                return;

            var scrap = !args.IsChecked.HasValue || args.IsChecked.Value;

            var techObjects = args.InnerParameter as IEnumerable<TechObject>;
            if (techObjects == null)
            {
                var scrapCommandArgs = args as ScrapCommandArgs;
                if (scrapCommandArgs != null)
                {
                    techObjects = scrapCommandArgs.Objects;
                }
                else
                {
                    var techObject = args.InnerParameter as TechObject;
                    if (techObject != null)
                        techObjects = new[] { techObject };
                }
            }

            if ((techObjects == null) || !techObjects.Any())
                return;

            PlayerOperations.Scrap(
                scrap,
                techObjects);
        }

        private void ExecuteIssueTaskForceOrderCommand(Pair<FleetView, FleetOrder> p)
        {
            var order = p.Second;
            if (order == null)
                return;

            var fleetView = p.First;
            var updateTaskForces = false;
            if (fleetView == null)
                return;
            if ((fleetView != null) && (fleetView.Source != null))
            {
                if (order.IsTargetRequired(fleetView.Source))
                {
                    var target = TargetSelectionDialog.Show(
                        order.FindTargets(fleetView.Source),
                        order.TargetDisplayMember,
                        order.OrderName);

                    if (target != null)
                    {
                        var currentOrder = fleetView.Source.Order;
                        if (currentOrder != null && currentOrder is TowOrder)
                            updateTaskForces = true;

                        if (currentOrder != null && currentOrder is WormholeOrder)
                            updateTaskForces = true;

                        order.Target = target;
                        PlayerOperations.SetFleetOrder(fleetView.Source, order);
                        //_soundPlayer.PlayAny("TaskForceOrders");
                    }
                }
                else
                {
                    var currentOrder = fleetView.Source.Order;
                    if (currentOrder != null && currentOrder is TowOrder)
                        updateTaskForces = true;

                    if (currentOrder != null && currentOrder is WormholeOrder)
                        updateTaskForces = true;

                    PlayerOperations.SetFleetOrder(fleetView.Source, order);
                    //_soundPlayer.PlayAny("TaskForceOrders");


                    if (order != null && order is AssaultSystemOrder)
                    {

                        var fleet = fleetView.Source;

                        var system = GameContext.Current.Universe.Map[fleet.Location].System;

                        if (!DiplomacyHelper.AreAtWar(system.Colony.Owner, fleet.Owner))
                        {

                            _navigationService.ActivateScreen(StandardGameScreens.DiplomacyScreen);

                            // TODO:
                            // next: Message Screeen: if you want to assault then declare war first, if you want to 

                            // old: 
                            // if Order = AssaultSystemOrder and not at war... then open Diplomacy
                            // a) you can't assault because you doesn't have declared war
                            // b) assaulting would mean you declared 

                        }
                    }
                }
            }

            Model.SelectedTaskForce = null;

            if (updateTaskForces)
                RefreshTaskForceList();

            //RefreshIntelForceList();
        }

        private void ExecuteToggleTaskForceCloakCommand(FleetView fleetView)
        {
            if ((fleetView == null) || (fleetView.Source == null))
                return;

            fleetView.Source.IsCloaked = !fleetView.Source.IsCloaked;

            PlayerOrderService.AddOrder(new CloakFleetOrder(fleetView.Source));
            Model.SelectedTaskForce = null;
        }
        private void ExecuteToggleTaskForceCamouflageCommand(FleetView fleetView)
        {
            if ((fleetView == null) || (fleetView.Source == null))
                return;

            fleetView.Source.IsCamouflaged = !fleetView.Source.IsCamouflaged;

            PlayerOrderService.AddOrder(new CamouflageFleetOrder(fleetView.Source));
            Model.SelectedTaskForce = null;
        }
        private IEnumerable<FleetViewWrapper> GenerateFleetViews(MapLocation location)
        {
            //GameLog.Print("LocalPlayerEmpire Homesystem = {0} ", AppContext.LocalPlayerEmpire.HomeSystem );
            if (!AppContext.LocalPlayerEmpire.MapData.IsScanned(location))
                return null;

            IList<FleetView> fleets = AppContext.CurrentGame.Universe.FindAt<Fleet>(location)
                .Where(o => o.IsVisible)
                .Select(o => FleetView.Create(AppContext.LocalPlayerEmpire.Civilization, o))
                .ToList();

            List<FleetViewWrapper> wrapperList = new List<FleetViewWrapper>();
            foreach (FleetView fleet in fleets)
                wrapperList.Add(new FleetViewWrapper(fleet));
            return wrapperList;
        }

        //private IEnumerable<IntelFleetViewWrapper> GenerateIntelFleetViews(MapLocation location)
        //{
        //    if (!base.AppContext.LocalPlayerEmpire.MapData.IsScanned(location))
        //        return null;

        //    IList<IntelFleetView> intelfleets = base.AppContext.CurrentGame.Universe.FindAt<IntelFleet>(location)
        //        .Where(o => o.IsVisible)
        //        .Select(o => IntelFleetView.Create(this.AppContext.LocalPlayerEmpire.Civilization, o))
        //        .ToList();

        //    List<IntelFleetViewWrapper> wrapperList = new List<IntelFleetViewWrapper>();
        //    foreach (IntelFleetView intelfleet in intelfleets)
        //        wrapperList.Add(new IntelFleetViewWrapper(intelfleet));
        //    return wrapperList;
        //}

        private void OnAvailableShipsChanged(object sender, EventArgs e)
        {
            var availableShips = Model.AvailableShips;
            var selectedShip = Model.SelectedShip;
            if ((availableShips == null) ||
                ((selectedShip != null) && !availableShips.Any(o => Equals(o.ObjectID, selectedShip.ObjectID))))
            {
                Model.SelectedShip = null;
            }
        }

        //private void OnAvailableIntelsChanged(object sender, EventArgs e)
        //{
        //    var availableIntels = this.Model.AvailableIntels;
        //    var selectedIntel = this.Model.SelectedIntel;
        //    if ((availableIntels == null) ||
        //        ((selectedIntel != null) && !availableIntels.Any(o => Equals(o.ObjectID, selectedIntel.ObjectID))))
        //    {
        //        this.Model.SelectedIntel = null;
        //    }
        //}

        private void OnFleetRouteUpdated(Fleet fleet)
        {
            Model.SelectedTaskForce = null;
        }

        private void OnInputModeChanged(object sender, EventArgs e)
        {
            switch (Model.InputMode)
            {
                case GalaxyScreenInputMode.Normal:
                    Model.AvailableShips = Enumerable.Empty<Ship>();
                    //this.Model.AvailableIntels = Enumerable.Empty<Intel>();
                    Model.SelectedTaskForce = null;
                    //this.Model.SelectedIntelForce = null;
                    Model.SelectedShipInTaskForce = null;
                    //this.Model.SelectedIntelInIntelForce = null;
                    Model.SelectedShip = null;
                    //this.Model.SelectedIntel = null;
                    break;
            }

            Model.SelectedShip = null;
            //this.Model.SelectedIntel = null;
            Model.SelectedTradeRoute = null;

            _setInputModeCommand.RaiseCanExecuteChanged();
            _setOverviewModeCommand.RaiseCanExecuteChanged();
        }

        private void OnOverviewModeChanged(object sender, EventArgs e)
        {
            if (Model.OverviewMode == GalaxyScreenOverviewMode.Economic)
            {
                Model.SelectedTaskForce = null;
                if (Model.InputMode != GalaxyScreenInputMode.Normal)
                {
                    Model.InputMode = GalaxyScreenInputMode.Normal;
                    return;
                }
            }
            else
            {
                Model.SelectedTradeRoute = null;
            }

            _setInputModeCommand.RaiseCanExecuteChanged();
            _setOverviewModeCommand.RaiseCanExecuteChanged();
        }

        private void OnSelectedSectorChanged(object sender, EventArgs e)
        {
            var selectedSector = Model.SelectedSector;
            if (selectedSector == null)
            {
                Model.TaskForces = Enumerable.Empty<FleetViewWrapper>();
                Model.TradeRoutes = Enumerable.Empty<TradeRoute>();
                Model.SelectedSectorAllegiance = null;
                Model.SelectedSectorInhabitants = null;
            }
            else
            {
                var starSystem = selectedSector.System;
                var playerEmpire = AppContext.LocalPlayerEmpire;
                Colony colony = null;
                string selectedSectorAllegiance = null;
                string selectedSectorInhabitants = null;
                IEnumerable<TradeRoute> tradeRoutes = null;

                if (starSystem != null)
                {
                    colony = starSystem.Colony;
                    if (colony != null && colony.OwnerID == playerEmpire.CivilizationID)
                        tradeRoutes = colony.TradeRoutes;
                }

                if (tradeRoutes == null)
                    tradeRoutes = Enumerable.Empty<TradeRoute>();

                var owner = GetPerceivedSectorOwner(selectedSector);
                if (owner != null)
                {
                    if (playerEmpire.MapData.IsExplored(selectedSector.Location))
                        selectedSectorInhabitants = owner.ShortName;
                    selectedSectorAllegiance = owner.ShortName;
                }

                Model.TradeRoutes = tradeRoutes;
                Model.TaskForces = GenerateFleetViews(selectedSector.Location);
                //this.Model.IntelForces = GenerateIntelFleetViews(selectedSector.Location);
                Model.GeneratePlayerTaskForces(playerEmpire.Civilization);
                Model.SelectedSectorAllegiance = selectedSectorAllegiance;
                Model.SelectedSectorInhabitants = selectedSectorInhabitants;

                var planetsViewRegion = CompositeRegionManager.GetRegionManager((DependencyObject)View).Regions[CommonGameScreenRegions.PlanetsView];
                planetsViewRegion.Context = selectedSector;

                if ((colony != null) && Equals(colony.OwnerID, playerEmpire.CivilizationID))
                    GalaxyScreenCommands.SelectSector.Execute(colony.Sector);
            }
        }

        private Civilization GetPerceivedSectorOwner(Sector sector)
        {
            var owner = (Civilization)null;
            var localPlayerEmpire = AppContext.LocalPlayer.Empire;
            var localPlayerEmpireManager = AppContext.LocalPlayerEmpire;

            var system = sector.System;
            if ((system != null) && system.IsOwned)
            {
                owner = system.Owner;
            }
            else
            {
                var station = sector.Station;
                if ((station != null) && station.IsOwned)
                    owner = station.Owner;
            }

            if ((owner != null) &&
                (localPlayerEmpireManager.MapData.IsExplored(sector.Location) ||
                 Equals(owner, localPlayerEmpire) ||
                 DiplomacyHelper.IsContactMade(owner, localPlayerEmpire)))
            {
                return owner;
            }

            var claims = AppContext.CurrentGame.SectorClaims;
            return claims.GetPerceivedOwner(sector.Location, localPlayerEmpire);
        }

        private void OnSelectedShipChanged(object sender, EventArgs e)
        {
            _addShipToTaskForceCommand.RaiseCanExecuteChanged();
            _removeShipFromTaskForceCommand.RaiseCanExecuteChanged();
        }

        private void OnSelectedTaskForceChanged(object sender, EventArgs e)
        {
            UpdateShipViews();
        }
        //private void OnSelectedIntelForceChanged(object sender, EventArgs e)
        //{
        //    UpdateIntelViews();
        //}

        private void UpdateShipViews()
        {
            var selectedTaskForce = Model.SelectedTaskForce;
            IEnumerable<Ship> availableShips;
            ShipView selectedShipInTaskForce = null;

            if (selectedTaskForce == null)
            {
                availableShips = Enumerable.Empty<Ship>();
            }
            else if (Model.OverviewMode == GalaxyScreenOverviewMode.Economic)
            {
                availableShips = Enumerable.Empty<Ship>();
                Model.SelectedTaskForce = null;
            }
            else
            {
                var sector = Model.SelectedSector;

                if (sector == null)
                {
                    availableShips = Enumerable.Empty<Ship>();
                }
                else
                {
                    var ownedShipsAtLocation = AppContext.CurrentGame.Universe.FindAt<Ship>(
                        Model.SelectedSector.Location,
                        ship => ship.OwnerID == AppContext.LocalPlayer.EmpireID);
                    availableShips = ownedShipsAtLocation.Where(
                        ship => !selectedTaskForce.View.Ships.Any(o => Equals(o.Source, ship)));
                }

                var selectedShip = Model.SelectedShip;

                if ((selectedShip != null) && selectedTaskForce.View.Ships.Select(o => o.Source).Contains(selectedShip))
                {
                    selectedShipInTaskForce = selectedTaskForce.View.Ships.FirstOrDefault(o => o.Source == selectedShip);
                }
                else
                {
                    selectedShipInTaskForce = Model.SelectedShipInTaskForce;

                    if (!selectedTaskForce.View.Ships.Contains(selectedShipInTaskForce))
                    {
                        selectedShipInTaskForce = null;
                    }
                }
            }
            
            Model.AvailableShips = availableShips;

            Model.SelectedShipInTaskForce = selectedShipInTaskForce;
        }
        //private void UpdateIntelViews()
        //{
        //    var selectedIntelForce = this.Model.SelectedIntelForce;
        //    IEnumerable<Intel> availableIntels;
        //    IntelView selectedIntelInIntelForce = null;

        //    if (selectedIntelForce == null)
        //    {
        //        availableIntels = Enumerable.Empty<Intel>();
        //    }
        //    else if (this.Model.OverviewMode == GalaxyScreenOverviewMode.Economic)
        //    {
        //        availableIntels = Enumerable.Empty<Intel>();
        //        this.Model.SelectedIntelForce = null;
        //    }
        //    else
        //    {
        //        var sector = this.Model.SelectedSector;

        //        if (sector == null)
        //        {
        //            availableIntels = Enumerable.Empty<Intel>();
        //        }
        //        else
        //        {
        //            var ownedIntelsAtLocation = this.AppContext.CurrentGame.Universe.FindAt<Intel>(
        //                this.Model.SelectedSector.Location,
        //               intel => intel.OwnerID == this.AppContext.LocalPlayer.EmpireID);
        //            availableIntels = ownedIntelsAtLocation.Where(
        //                intel => !selectedIntelForce.View.Intels.Any(o => Equals(o.Source, intel)));
        //        }

        //        var selectedIntel = this.Model.SelectedIntel;

        //        if ((selectedIntel != null) && selectedIntelForce.View.Intels.Select(o => o.Source).Contains(selectedIntel))
        //        {
        //            selectedIntelInIntelForce = selectedIntelForce.View.Intels.FirstOrDefault(o => o.Source == selectedIntel);
        //        }
        //        else
        //        {
        //            selectedIntelInIntelForce = this.Model.SelectedIntelInIntelForce;

        //            if (!selectedIntelForce.View.Intels.Contains(selectedIntelInIntelForce))
        //            {
        //                selectedIntelInIntelForce = null;
        //            }
        //        }
        //    }

        //    this.Model.AvailableIntels = availableIntels;

        //    this.Model.SelectedIntelInIntelForce = selectedIntelInIntelForce;
        //}
        private void OnSelectedTradeRouteChanged(object sender, EventArgs e)
        {
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
        }

        private void OnTaskForcesChanged(object sender, EventArgs e)
        {
            var taskForces = Model.TaskForces;
            var selectedTaskForce = Model.SelectedTaskForce;
            Model.SelectedTaskForce = (taskForces == null)
                                           ? null
                                           : taskForces.FirstOrDefault(o => Equals(o, selectedTaskForce));

            Model.GeneratePlayerTaskForces(AppContext.LocalPlayerEmpire.Civilization);
        }
        //private void OnIntelForcesChanged(object sender, EventArgs e)
        //{
        //    var intelForces = this.Model.IntelForces;
        //    var selectedIntelForce = this.Model.SelectedIntelForce;
        //    //this.Model.SelectedIntelForce = (intelForces == null)
        //                                   ? null
        //                                   : intelForces.FirstOrDefault(o => Equals(o, selectedIntelForce));

        //    this.Model.GeneratePlayerTaskForces(base.AppContext.LocalPlayerEmpire.Civilization);
        //}

        private void OnTradeRouteChanged(TradeRoute tradeRoute)
        {
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
        }

        private void OnTradeRoutesChanged(object sender, EventArgs e)
        {
            var tradeRoutes = Model.TradeRoutes;
            var selectedTradeRoute = Model.SelectedTradeRoute;
            Model.SelectedTradeRoute = (tradeRoutes == null)
                                            ? null
                                            : tradeRoutes.FirstOrDefault(o => Equals(o, selectedTradeRoute));
        }

        private void OnTurnStarted()
        {
            if (Model.SelectedSector == null)
            {
                Model.SelectedSector = AppContext.LocalPlayerEmpire.SeatOfGovernment.Sector;
            }
            else
            {
                var currentLocation = Model.SelectedSector.Location;
                Model.SelectedSectorInternal = null;
                Model.SelectedSectorInternal = AppContext.CurrentGame.Universe.Map[currentLocation];
            }

            Model.EmpirePlayers.ClearPlayerReadiness();
            Model.EmpirePlayers.UpdateRelationshipStatus();

            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);

            RefreshTaskForceList();

            //RefreshIntelForceList();
        }

        private void OnAllTurnEnded(ClientEventArgs obj)
        {
            Model.InputMode = GalaxyScreenInputMode.Normal;
            Model.SelectedTaskForce = null;
            Model.SelectedTradeRoute = null;
        }

        private void OnTurnStarted(ClientEventArgs obj)
        {
            if (PlayerOrderService.AutoTurn)
            {
                ClientCommands.EndTurn.Execute(null);
            }
        }

        private void OnLobbyUpdated(ClientDataEventArgs<ILobbyData> args)
        {
            Model.EmpirePlayers.Update(args.Value);
        }

        private void RefreshTaskForceList()
        {
            var selectedSector = Model.SelectedSector;
            if (selectedSector == null)
            {
                Model.TaskForces = Enumerable.Empty<FleetViewWrapper>();
                return;
            }
            Model.TaskForces = GenerateFleetViews(selectedSector.Location);
        }
        //private void RefreshIntelForceList()
        //{
        //    var selectedSector = this.Model.SelectedSector;
        //    if (selectedSector == null)
        //    {
        //        this.Model.IntelForces = Enumerable.Empty<IntelFleetViewWrapper>();
        //        return;
        //    }
        //    this.Model.IntelForces = GenerateIntelFleetViews(selectedSector.Location);
        //}
        #endregion
    }
}