// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

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
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;

namespace Supremacy.Client.Views
{
    public class GalaxyScreenPresenter : GameScreenPresenterBase<GalaxyScreenPresentationModel, IGalaxyScreenView>, IGalaxyScreenPresenter
    {
        #region Fields
        private readonly DelegateCommand<RedeployShipCommandArgs> _addShipToTaskForceCommand;
        private readonly DelegateCommand<TradeRoute> _cancelTradeRouteCommand;
        private readonly DelegateCommand<Pair<FleetView, FleetOrder>> _issueTaskForceOrderCommand;
        private readonly DelegateCommand<RedeployShipCommandArgs> _removeShipFromTaskForceCommand;
        private readonly DelegateCommand<GalaxyScreenInputMode> _setInputModeCommand;
        private readonly DelegateCommand<GalaxyScreenOverviewMode> _setOverviewModeCommand;
        private readonly DelegateCommand<FleetView> _toggleTaskForceCloakCommand;
        private readonly DelegateCommand<FleetView> _toggleTaskForceCamouflageCommand;
        private readonly DelegateCommand<ICheckableCommandParameter> _scrapCommand;
        private readonly List<IDisposable> _channelSubscriptions;
        private readonly INavigationService _navigationService = null;
        private readonly ISoundPlayer _soundPlayer = null;
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
            {
                throw new ArgumentNullException("container");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

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

            _navigationService = navigationService ?? throw new ArgumentNullException("navigationService");

            _soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");

            ISoundPlayer dummy = _soundPlayer;
        }
        #endregion

        #region Public and Protected Methods
        protected override string ViewName => StandardGameScreens.GalaxyScreen;

        protected override void InvalidateCommands()
        {
            _addShipToTaskForceCommand.RaiseCanExecuteChanged();
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
            _issueTaskForceOrderCommand.RaiseCanExecuteChanged();
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
            Model.SelectedTaskForceChanged += OnSelectedTaskForceChanged;
            Model.AvailableShipsChanged += OnAvailableShipsChanged;
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

            _ = ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
            _ = ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded, ThreadOption.UIThread);
            _ = ClientEvents.LobbyUpdated.Subscribe(OnLobbyUpdated, ThreadOption.UIThread);
            _ = PlayerActionEvents.FleetRouteUpdated.Subscribe(OnFleetRouteUpdated, ThreadOption.UIThread);
            _ = GameEvents.TradeRouteEstablished.Subscribe(OnTradeRouteChanged, ThreadOption.UIThread);
            _ = GameEvents.TradeRouteCancelled.Subscribe(OnTradeRouteChanged, ThreadOption.UIThread);

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
            PlayerActionEvents.FleetRouteUpdated.Unsubscribe(OnFleetRouteUpdated);
            GameEvents.TradeRouteEstablished.Unsubscribe(OnTradeRouteChanged);
            GameEvents.TradeRouteCancelled.Unsubscribe(OnTradeRouteChanged);

            foreach (IDisposable subscription in _channelSubscriptions)
            {
                subscription.Dispose();
            }

            _channelSubscriptions.Clear();

            base.UnregisterCommandAndEventHandlers();
        }
        #endregion

        #region Private Methods
        private bool CanExecuteAddShipToTaskForceCommand(RedeployShipCommandArgs args)
        {
            if (args == null)
            {
                return false;
            }

            if (args.Ship.OwnerID != AppContext.LocalPlayer.EmpireID)
            {
                return false;
            }

            if (Model.SelectedTaskForce == null)
            {
                return false;
            }

            if (!Model.SelectedTaskForce.View.Ships.Any(o => Equals(o.Source, o)))
            {
                return false;
            }

            return true;
        }

        private bool CanExecuteRemoveShipFromTaskForceCommand(RedeployShipCommandArgs args)
        {
            if (args == null)
            {
                return false;
            }

            if (args.Ship.OwnerID != AppContext.LocalPlayer.EmpireID)
            {
                return false;
            }

            if (Model.SelectedTaskForce == null)
            {
                return false;
            }

            if (!Model.SelectedTaskForce.View.Ships.Any(o => Equals(o.Source, o)))
            {
                return false;
            }

            return true;
        }

        private void ExecuteAddShipToTaskForceCommand(RedeployShipCommandArgs args)
        {
            PlayerOperations.RedeployShip(args.Ship, args.TargetFleet);

            RefreshTaskForceList();

            Model.SelectedTaskForce = null;

            if (Model.InputMode == GalaxyScreenInputMode.RedeployShips)
            {
                return;
            }

            Model.SelectedTaskForce = Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, args.Ship.Fleet));
        }

        private void ExecuteRemoveShipFromTaskForceCommand(RedeployShipCommandArgs args)
        {
            Fleet selectedTaskForce = args.Ship.Fleet;
            if (selectedTaskForce == null)
            {
                return;
            }

            PlayerOperations.RedeployShip(args.Ship);

            RefreshTaskForceList();

            Model.SelectedTaskForce = null;

            if (Model.InputMode == GalaxyScreenInputMode.RedeployShips)
            {
                return;
            }

            Model.SelectedTaskForce = selectedTaskForce.Ships.Any()
                ? Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, selectedTaskForce))
                : Model.TaskForces.FirstOrDefault(o => Equals(o.View.Source, args.Ship.Fleet));
        }

        private void ExecuteCancelTradeRouteCommand(TradeRoute tradeRoute)
        {
            if ((tradeRoute == null) || !tradeRoute.IsAssigned)
            {
                return;
            }

            tradeRoute.TargetColony = null;
            GameEvents.TradeRouteCancelled.Publish(tradeRoute);

            PlayerOrderService.AddOrder(new SetTradeRouteOrder(tradeRoute));
            Model.SelectedTradeRoute = null;
        }

        private static bool CanExecuteScrapCommand(ICheckableCommandParameter args)
        {
            if (args == null)
            {
                return false;
            }

            if (args is ScrapCommandArgs scrapCommandArgs)
            {
                return scrapCommandArgs.Objects.Any();
            }

            TechObject techObject = args.InnerParameter as TechObject;
            if (techObject != null)
            {
                return true;
            }

            if (args.InnerParameter is IEnumerable<TechObject> techObjects)
            {
                return techObjects.Any();
            }

            return false;
        }

        private static void ExecuteScrapCommand(ICheckableCommandParameter args)
        {
            if (args == null)
            {
                return;
            }

            bool scrap = !args.IsChecked.HasValue || args.IsChecked.Value;

            IEnumerable<TechObject> techObjects = args.InnerParameter as IEnumerable<TechObject>;
            if (techObjects == null)
            {
                if (args is ScrapCommandArgs scrapCommandArgs)
                {
                    techObjects = scrapCommandArgs.Objects;
                }
                else
                {
                    TechObject techObject = args.InnerParameter as TechObject;
                    if (techObject != null)
                    {
                        techObjects = new[] { techObject };
                    }
                }
            }

            if ((techObjects == null) || !techObjects.Any())
            {
                return;
            }

            PlayerOperations.Scrap(
                scrap,
                techObjects);
        }

        private void ExecuteIssueTaskForceOrderCommand(Pair<FleetView, FleetOrder> p)
        {
            FleetOrder order = p.Second;
            if (order == null)
            {
                return;
            }

            FleetView fleetView = p.First;
            bool updateTaskForces = false;
            if (fleetView == null)
            {
                return;
            }

            if ((fleetView != null) && (fleetView.Source != null))
            {
                if (order.IsTargetRequired(fleetView.Source))
                {
                    object target = TargetSelectionDialog.Show(
                        order.FindTargets(fleetView.Source),
                        order.TargetDisplayMember,
                        order.OrderName);

                    if (target != null)
                    {
                        FleetOrder currentOrder = fleetView.Source.Order;
                        if (currentOrder != null && currentOrder is TowOrder)
                        {
                            updateTaskForces = true;
                        }

                        if (currentOrder != null && currentOrder is WormholeOrder)
                        {
                            updateTaskForces = true;
                        }

                        order.Target = target;
                        PlayerOperations.SetFleetOrder(fleetView.Source, order);
                    }
                }
                else
                {
                    FleetOrder currentOrder = fleetView.Source.Order;
                    if (currentOrder != null && currentOrder is TowOrder)
                    {
                        updateTaskForces = true;
                    }

                    if (currentOrder != null && currentOrder is WormholeOrder)
                    {
                        updateTaskForces = true;
                    }

                    PlayerOperations.SetFleetOrder(fleetView.Source, order);


                    if (order != null && order is AssaultSystemOrder)
                    {

                        Fleet fleet = fleetView.Source;

                        StarSystem system = GameContext.Current.Universe.Map[fleet.Location].System;

                        if (!DiplomacyHelper.AreAtWar(system.Colony.Owner, fleet.Owner))
                        {

                            _ = _navigationService.ActivateScreen(StandardGameScreens.DiplomacyScreen);

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
            {
                RefreshTaskForceList();
            }
        }

        private void ExecuteToggleTaskForceCloakCommand(FleetView fleetView)
        {
            if ((fleetView == null) || (fleetView.Source == null))
            {
                return;
            }

            fleetView.Source.IsCloaked = !fleetView.Source.IsCloaked;

            PlayerOrderService.AddOrder(new CloakFleetOrder(fleetView.Source));
            Model.SelectedTaskForce = null;
        }
        private void ExecuteToggleTaskForceCamouflageCommand(FleetView fleetView)
        {
            if ((fleetView == null) || (fleetView.Source == null))
            {
                return;
            }

            fleetView.Source.IsCamouflaged = !fleetView.Source.IsCamouflaged;

            PlayerOrderService.AddOrder(new CamouflageFleetOrder(fleetView.Source));
            Model.SelectedTaskForce = null;
        }

        private IEnumerable<FleetViewWrapper> GenerateFleetViews(MapLocation location)
        {
            if (!AppContext.LocalPlayerEmpire.MapData.IsScanned(location))
            {
                return null;
            }

            IList<FleetView> fleets = AppContext.CurrentGame.Universe.FindAt<Fleet>(location)
                .Where(o => o.IsVisible)
                .Select(o => FleetView.Create(AppContext.LocalPlayerEmpire.Civilization, o))
                .ToList();

            List<FleetViewWrapper> wrapperList = new List<FleetViewWrapper>();
            foreach (FleetView fleet in fleets)
            {
                wrapperList.Add(new FleetViewWrapper(fleet));
            }
            return wrapperList;
        }

        private void OnAvailableShipsChanged(object sender, EventArgs e)
        {
            IEnumerable<Ship> availableShips = Model.AvailableShips;
            Ship selectedShip = Model.SelectedShip;
            if ((availableShips == null) ||
                ((selectedShip != null) && !availableShips.Any(o => Equals(o.ObjectID, selectedShip.ObjectID))))
            {
                Model.SelectedShip = null;
            }
        }

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
                    Model.TradeRoutes = Enumerable.Empty<TradeRoute>();
                    Model.SelectedTaskForce = null;
                    Model.SelectedShipInTaskForce = null;
                    Model.SelectedShip = null;
                    break;
            }

            Model.SelectedShip = null;
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
            Sector selectedSector = Model.SelectedSector;
            if (selectedSector == null)
            {
                Model.TaskForces = Enumerable.Empty<FleetViewWrapper>();
                Model.TradeRoutes = Enumerable.Empty<TradeRoute>();
                Model.SelectedSectorAllegiance = null;
                Model.SelectedSectorInhabitants = null;
            }
            else
            {
                StarSystem starSystem = selectedSector.System;
                CivilizationManager playerEmpire = AppContext.LocalPlayerEmpire;
                Colony colony = null;
                string selectedSectorAllegiance = null;
                string selectedSectorInhabitants = null;
                IEnumerable<TradeRoute> tradeRoutes = null;

                if (starSystem != null)
                {
                    colony = starSystem.Colony;
                    if (colony != null && colony.OwnerID == playerEmpire.CivilizationID)
                    {
                        tradeRoutes = colony.TradeRoutes;
                    }
                }

                if (tradeRoutes == null)
                {
                    tradeRoutes = Enumerable.Empty<TradeRoute>();
                }

                Civilization owner = GetPerceivedSectorOwner(selectedSector);
                if (owner != null)
                {
                    if (playerEmpire.MapData.IsExplored(selectedSector.Location))
                    {
                        selectedSectorInhabitants = owner.ShortName;
                    }

                    selectedSectorAllegiance = owner.ShortName;
                }

                Model.TradeRoutes = tradeRoutes;
                Model.TaskForces = GenerateFleetViews(selectedSector.Location);
                Model.GeneratePlayerTaskForces(playerEmpire.Civilization);
                Model.SelectedSectorAllegiance = selectedSectorAllegiance;
                Model.SelectedSectorInhabitants = selectedSectorInhabitants;

                Microsoft.Practices.Composite.Regions.IRegion planetsViewRegion = CompositeRegionManager.GetRegionManager((DependencyObject)View).Regions[CommonGameScreenRegions.PlanetsView];
                planetsViewRegion.Context = selectedSector;

                if ((colony != null) && Equals(colony.OwnerID, playerEmpire.CivilizationID))
                {
                    GalaxyScreenCommands.SelectSector.Execute(colony.Sector);
                }
            }
        }

        private Civilization GetPerceivedSectorOwner(Sector sector)
        {
            Civilization owner = null;
            Civilization localPlayerEmpire = AppContext.LocalPlayer.Empire;
            CivilizationManager localPlayerEmpireManager = AppContext.LocalPlayerEmpire;

            StarSystem system = sector.System;
            if ((system != null) && system.IsOwned)
            {
                owner = system.Owner;
            }
            else
            {
                Station station = sector.Station;
                if ((station != null) && station.IsOwned)
                {
                    owner = station.Owner;
                }
            }

            if ((owner != null) &&
                (localPlayerEmpireManager.MapData.IsExplored(sector.Location) ||
                 Equals(owner, localPlayerEmpire) ||
                 DiplomacyHelper.IsContactMade(owner, localPlayerEmpire)))
            {
                return owner;
            }

            SectorClaimGrid claims = AppContext.CurrentGame.SectorClaims;
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

        private void UpdateShipViews()
        {
            FleetViewWrapper selectedTaskForce = Model.SelectedTaskForce;
            IEnumerable<Ship> availableShips;
            ShipView selectedShipInTaskForce = null;

            if (selectedTaskForce == null)
            {
                availableShips = Enumerable.Empty<Ship>();
                // doesn't help much
                // GameLog.Client.Intel.DebugFormat("SelectedTaskForce is null. availableShips is Empty ={0}", availableShips);
            }

            else if (Model.OverviewMode == GalaxyScreenOverviewMode.Economic)
            {
                availableShips = Enumerable.Empty<Ship>();
                Model.SelectedTaskForce = null;
            }
            else
            {
                Sector sector = Model.SelectedSector;

                if (sector == null)
                {
                    availableShips = Enumerable.Empty<Ship>();
                }
                else
                {
                    IEnumerable<Ship> ownedShipsAtLocation = AppContext.CurrentGame.Universe.FindAt<Ship>(Model.SelectedSector.Location)
                        .Where(s => s.OwnerID == AppContext.LocalPlayer.EmpireID);

                    //foreach (var ownedShip in ownedShipsAtLocation)
                    //GameLog.Client.Intel.DebugFormat("local player ownedship.Name = {0}", ownedShip.Name);

                    availableShips = ownedShipsAtLocation.Where(
                        ship => !selectedTaskForce.View.Ships.Any(o => Equals(o.Source, ship))); //?? your ships that are not selected, just available

                    //foreach (var availableShip in availableShips)
                    //GameLog.Client.Intel.DebugFormat("availableShip.Name = {0}", availableShip.Name);
                }

                Ship selectedShip = Model.SelectedShip;

                if ((selectedShip != null) && selectedTaskForce.View.Ships.Select(o => o.Source).Contains(selectedShip))
                {
                    selectedShipInTaskForce = selectedTaskForce.View.Ships.FirstOrDefault(o => o.Source == selectedShip);

                    GameLog.Client.IntelDetails.DebugFormat("Contains(selectedShip) - selectedShipInTaskForce = {0}", selectedTaskForce.View.Ships.Count);
                }
                else
                {
                    selectedShipInTaskForce = Model.SelectedShipInTaskForce;
                    GameLog.Client.IntelDetails.DebugFormat("ELSE ... selectedShipInTaskForce = {0}", selectedTaskForce.View.Ships.Count);

                    if (!selectedTaskForce.View.Ships.Contains(selectedShipInTaskForce))
                    {
                        //GameLog.Client.IntelDetails.DebugFormat("selectedTaskForce.View.Ships.Contains(selectedShipInTaskForce) is FALSE - count = {0}", selectedTaskForce.View.Ships.Count);
                        selectedShipInTaskForce = null;
                    }
                }
            }

            Model.AvailableShips = availableShips;

            Model.SelectedShipInTaskForce = selectedShipInTaskForce;

            if (selectedTaskForce != null)
            {
                GameLog.Client.IntelDetails.DebugFormat("selectedShipInTaskForceLISTVIEW = {0} (for own: only the first one is shown in detail view = System Panel because one single ship in the fleet) ",
                        selectedTaskForce.View.Ships.Count);
            }
        }

        private void OnSelectedTradeRouteChanged(object sender, EventArgs e)
        {
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
        }

        private void OnTaskForcesChanged(object sender, EventArgs e)
        {
            IEnumerable<FleetViewWrapper> taskForces = Model.TaskForces;
            FleetViewWrapper selectedTaskForce = Model.SelectedTaskForce;
            Model.SelectedTaskForce = taskForces?.FirstOrDefault(o => Equals(o, selectedTaskForce));

            Model.GeneratePlayerTaskForces(AppContext.LocalPlayerEmpire.Civilization);
        }

        private void OnTradeRouteChanged(TradeRoute tradeRoute)
        {
            _cancelTradeRouteCommand.RaiseCanExecuteChanged();
        }

        private void OnTradeRoutesChanged(object sender, EventArgs e)
        {
            IEnumerable<TradeRoute> tradeRoutes = Model.TradeRoutes;
            TradeRoute selectedTradeRoute = Model.SelectedTradeRoute;
            Model.SelectedTradeRoute = tradeRoutes?.FirstOrDefault(o => Equals(o, selectedTradeRoute));
        }

        private void OnTurnStarted()
        {
            if (Model.SelectedSector == null)
            {
                Model.SelectedSector = AppContext.LocalPlayerEmpire.SeatOfGovernment.Sector;
            }
            else
            {
                MapLocation currentLocation = Model.SelectedSector.Location;
                Model.SelectedSectorInternal = null;
                Model.SelectedSectorInternal = AppContext.CurrentGame.Universe.Map[currentLocation];
            }

            Model.EmpirePlayers.ClearPlayerReadiness();
            Model.EmpirePlayers.UpdateRelationshipStatus();

            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);

            RefreshTaskForceList();
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
            Sector selectedSector = Model.SelectedSector;
            if (selectedSector == null)
            {
                Model.TaskForces = Enumerable.Empty<FleetViewWrapper>();
                return;
            }
            Model.TaskForces = GenerateFleetViews(selectedSector.Location);
        }
        #endregion
    }
}