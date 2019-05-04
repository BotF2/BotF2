// GameClient.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Concurrency;
using System.Net;
using System.ServiceModel;
using System.Threading;

using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity.Utility;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Combat;
using Supremacy.Game;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.WCF;

using System.Linq;

using Scheduler = Supremacy.Threading.Scheduler;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameClientCallback : ISupremacyCallback, IDisposable
    {
        private readonly IAppContext _appContext;
        private readonly AutoResetEvent _gameUpdateWaitHandle;
        private readonly ManualResetEvent _localPlayerJoinedWaitHandle;
        private readonly IScheduler _scheduler;

        internal IGameClient Client { get; set; }

        public GameClientCallback([NotNull] IAppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");

            _appContext = appContext;
            _gameUpdateWaitHandle = new AutoResetEvent(false);
            _localPlayerJoinedWaitHandle = new ManualResetEvent(false);
            _scheduler = Scheduler.ClientEventLoop;
        }

        #region Implementation of ISupremacyCallback
        public void NotifyOnJoin(Player localPlayer, LobbyData lobbyData)
        {
            Observable.ToAsync(
                () =>
                {
                    ClientEvents.LocalPlayerJoined.Publish(new LocalPlayerJoinedEventArgs(localPlayer, lobbyData));
                    _localPlayerJoinedWaitHandle.Set();
                    ClientEvents.PlayerJoined.Publish(new ClientDataEventArgs<IPlayer>(localPlayer));
                },
                _scheduler)();
            
            //ClientEvents.LobbyUpdated.Publish(new ClientDataEventArgs<ILobbyData>(lobbyData));
        }

        public void NotifyPlayerJoined(Player player)
        {
            Observable.ToAsync(
                () => ClientEvents.PlayerJoined.Publish(new ClientDataEventArgs<IPlayer>(player)),
                _scheduler)();
        }

        public void NotifyPlayerExited(Player player)
        {
            Observable.ToAsync(
                () => ClientEvents.PlayerExited.Publish(new ClientDataEventArgs<IPlayer>(player)),
                _scheduler)();
        }

        public void NotifyGameStarting()
        {
            Observable.ToAsync(
                () => ClientEvents.GameStarting.Publish(ClientEventArgs.Default),
                _scheduler)();
        }

        public void NotifyGameStarted(GameStartMessage startMessage)
        {
            Observable.ToAsync(
                () =>
                {
                    ClientEvents.GameStarted.Publish(new ClientDataEventArgs<GameStartData>(startMessage.Data));
                    ClientEvents.TurnStarted.Publish(new GameContextEventArgs(_appContext, _appContext.CurrentGame));
                    Channel.Publish(new TurnStartedMessage());
                },
                _scheduler)();
        }

        public void NotifyTurnProgressChanged(TurnPhase phase)
        {
            Observable.ToAsync(
                () =>
                {
                    Channel.Publish(new TurnProgressChangedMessage(phase));
                    ClientEvents.TurnPhaseChanged.Publish(new ClientDataEventArgs<TurnPhase>(phase));
                },
                _scheduler)();
        }

        public void NotifyGameDataUpdated(GameUpdateMessage updateMessage)
        {
            Observable.ToAsync(
                () => ClientEvents.GameUpdateDataReceived.Publish(new ClientDataEventArgs<GameUpdateData>(updateMessage.Data)),
                _scheduler)();
        }

        public void NotifyAllTurnEnded()
        {
            Observable.ToAsync(
                () => ClientEvents.AllTurnEnded.Publish(new GameContextEventArgs(_appContext, _appContext.CurrentGame)),
                _scheduler)();
        }

        public void NotifyTurnFinished()
        {
            Observable.ToAsync(
                () =>
                {
                    Channel.Publish(new TurnStartedMessage());
                    ClientEvents.TurnStarted.Publish(new GameContextEventArgs(_appContext, _appContext.CurrentGame));
                },
                _scheduler)();
        }

        public void NotifyChatMessageReceived(int senderId, string message, int recipientId)
        {
            Observable.ToAsync(
                () =>
                {
                    var sender = GetPlayerFromID(senderId);
                    if (sender == null)
                        return;
                    
                    var recipient = GetPlayerFromID(recipientId);
                    
                    ClientEvents.ChatMessageReceived.Publish(
                        new ClientDataEventArgs<ChatMessage>(
                            new ChatMessage(sender, message, recipient)));
                },
                _scheduler)();
        }

        private IPlayer GetPlayerFromID(int senderId)
        {
            var localPlayer = _appContext.LocalPlayer;
            if ((localPlayer != null) && (localPlayer.PlayerID == senderId))
                return localPlayer;
            return _appContext.RemotePlayers.FirstOrDefault(o => o.PlayerID == senderId);
        }

        public void NotifyLobbyUpdated(LobbyData lobbyData)
        {
            Observable.ToAsync(
                () => ClientEvents.LobbyUpdated.Publish(new ClientDataEventArgs<ILobbyData>(lobbyData)),
                _scheduler)();
        }

        public void NotifyDisconnected()
        {
            Observable.ToAsync(
                () =>
                {
                    var client = Client;
                    if (client != null)
                        client.Disconnect();
                },
                _scheduler)();
        }

        public void Ping()
        {
            Observable.ToAsync(
                () => ClientEvents.ServerHeartbeat.Publish(ClientEventArgs.Default),
                _scheduler)();
        }

        public void NotifyCombatUpdate(CombatUpdate update)
        {
            Observable.ToAsync(
                () => ClientEvents.CombatUpdateReceived.Publish(new ClientDataEventArgs<CombatUpdate>(update)),
                _scheduler)();
        }

        public void NotifyInvasionUpdate(InvasionArena update)
        {
            Observable.ToAsync(
                () => ClientEvents.InvasionUpdateReceived.Publish(new ClientDataEventArgs<InvasionArena>(update)),
                _scheduler)();
        }

        public void NotifyPlayerFinishedTurn(int empireId)
        {
            Observable.ToAsync(
                () =>
                {
                    var player = _appContext.Players.FirstOrDefault(o => o.EmpireID == empireId);
                    if (player == null)
                        return;
                    Channel.Publish(new PlayerTurnFinishedMessage(player), true);
                    ClientEvents.PlayerTurnFinished.Publish(new ClientDataEventArgs<IPlayer>(player));
                },
                _scheduler)();
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            Client = null;
            _localPlayerJoinedWaitHandle.Set();
            _gameUpdateWaitHandle.Set();
        }
        #endregion
    }

    internal class GameClient : IGameClient, IDisposable
    {
        protected const string LocalEndpointConfigurationName = "LocalEndpoint";
        protected const string LocalEndpointAddress = "net.pipe://localhost/SupremacyService/Local";
        protected const string RemoteAddressFormat = "net.tcp://{0}:4455/SupremacyService";

        private readonly ILoggerFacade _logger;
        private readonly ISupremacyCallback _clientCallback;
        private readonly IPlayerOrderService _playerOrderService;
        //private readonly IPlayerTarget1Service _playerTarget1Service;
        //private readonly IPlayerTarget2Service _playerTarget2Service;
        private readonly object _clientLock;
        private readonly object _eventLock;
        private readonly IScheduler _scheduler;
        private readonly DelegateCommand<ChatMessage> _sendChatMessageCommand;
        private readonly DelegateCommand<CombatOrders> _sendCombatOrdersCommand;
        private readonly DelegateCommand<CombatTargetPrimaries> _sendCombatTarget1Command;
        private readonly DelegateCommand<CombatTargetSecondaries> _sendCombatTarget2Command;
        private readonly DelegateCommand<InvasionOrders> _sendInvasionOrdersCommand;
        private readonly DelegateCommand<object> _endInvasionCommand;
        private readonly DelegateCommand<string> _saveGameCommand;
        private readonly DelegateCommand<Pair<int, int>> _assignPlayerSlotCommand;
        private readonly DelegateCommand<int> _clearPlayerSlotCommand;
        private readonly DelegateCommand<int> _closePlayerSlotCommand;
        private readonly DelegateCommand<object> _startMultiplayerGameCommand;
        private readonly DelegateCommand<GameOptions> _sendUpdatedGameOptionsCommand;

        private ServiceClient _serviceClient;
        private ClientDisconnectReason? _disconnectReason;
        private bool _eventAndCommandHandlersHooked;
        private bool _connectionEstablished;
        private bool _isConnected;
        private bool _isDisconnecting;
        private bool _isDisposed;

        public GameClient(
            [NotNull] ILoggerFacade logger, 
            [NotNull] ISupremacyCallback clientCallback, 
            [NotNull] IPlayerOrderService playerOrderService)
            //[NotNull] IPlayerTarget1Service playerTarget1Service,
            //[NotNull] IPlayerTarget2Service playerTarget2Service)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (clientCallback == null)
                throw new ArgumentNullException("clientCallback");
            if (playerOrderService == null)
                throw new ArgumentNullException("playerOrderService");
            //if (playerTarget1Service == null)
            //    throw new ArgumentNullException("playerTarget1Service");
            //if (playerTarget2Service == null)
            //    throw new ArgumentNullException("playerTarget2Service");

            _clientLock = new object();
            _eventLock = new object();
            _playerOrderService = playerOrderService;
            //_playerTarget1Service = playerTarget1Service;
            //_playerTarget2Service = playerTarget2Service;
            _logger = logger;
            _clientCallback = clientCallback;
            _scheduler = Scheduler.ClientEventLoop;

            ((GameClientCallback)_clientCallback).Client = this;

            _sendChatMessageCommand = new DelegateCommand<ChatMessage>(ExecuteSendChatMessageCommand) { IsActive = true };
            _sendCombatOrdersCommand = new DelegateCommand<CombatOrders>(ExecuteSendCombatOrdersCommand) { IsActive = true };
            _sendCombatTarget1Command = new DelegateCommand<CombatTargetPrimaries>(ExecuteSendCombatTarget1Command) { IsActive = true };
            _sendCombatTarget2Command = new DelegateCommand<CombatTargetSecondaries>(ExecuteSendCombatTarget2Command) { IsActive = true };
            _sendInvasionOrdersCommand = new DelegateCommand<InvasionOrders>(ExecuteSendInvasionOrdersCommand) { IsActive = true };
            _endInvasionCommand = new DelegateCommand<object>(ExecuteEndInvasionCommand) { IsActive = true };
            _saveGameCommand = new DelegateCommand<string>(ExecuteSaveGameCommand) { IsActive = false };
            _assignPlayerSlotCommand = new DelegateCommand<Pair<int, int>>(ExecuteAssignPlayerSlotCommand) { IsActive = true };
            _clearPlayerSlotCommand = new DelegateCommand<int>(ExecuteClearPlayerSlotCmmand) { IsActive = true };
            _closePlayerSlotCommand = new DelegateCommand<int>(ExecuteClosePlayerSlotCmmand) { IsActive = true };
            _startMultiplayerGameCommand = new DelegateCommand<object>(ExecuteStartMultiplayerGameCommand) { IsActive = true };
            _sendUpdatedGameOptionsCommand = new DelegateCommand<GameOptions>(ExecuteSendUpdatedGameOptionsCommand) { IsActive = true };
        }

        private void ExecuteSendUpdatedGameOptionsCommand(GameOptions options)
        {
            ExecuteRemoteCommand(() => _serviceClient.UpdateGameOptions(options));
        }

        private void ExecuteStartMultiplayerGameCommand(object obj)
        {
            ExecuteRemoteCommand(() => _serviceClient.StartGame());
        }

        private void ExecuteClearPlayerSlotCmmand(int slotId)
        {
            ExecuteRemoteCommand(() => _serviceClient.ClearPlayerSlot(slotId));
        }

        private void ExecuteClosePlayerSlotCmmand(int slotId)
        {
            ExecuteRemoteCommand(() => _serviceClient.ClosePlayerSlot(slotId));
        }

        private void ExecuteAssignPlayerSlotCommand(Pair<int, int> slotAndPlayerId)
        {
            ExecuteRemoteCommand(() => _serviceClient.AssignPlayerSlot(slotAndPlayerId.First, slotAndPlayerId.Second));
        }

        protected void ExecuteRemoteCommand(Action remoteCommand) {
            ServiceClient serviceClient;

            lock (_clientLock)
            {
                if (!_isConnected)
                    return;
                serviceClient = _serviceClient;
            }

            if (serviceClient == null)
                return;

            try
            {
                remoteCommand();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        private void ExecuteSaveGameCommand(string fileName)
        {
            ExecuteRemoteCommand(() => _serviceClient.SaveGame(fileName));
        }

        #region Implementation of IGameClient
        public event Action<ClientEventArgs> Connected;
        public event Action<ClientDataEventArgs<ClientDisconnectReason>> Disconnected;

        public bool IsConnected
        {
            get
            {
                lock (_clientLock)
                {
                    return _isConnected;
                }
            }
        }

        public void Connect([NotNull] string playerName, [NotNull] IPAddress remoteServerAddress)
        {
            if (playerName == null)
                throw new ArgumentNullException("playerName");
            if (remoteServerAddress == null)
                throw new ArgumentNullException("remoteServerAddress");

            CheckDisposed();

            try
            {
                _serviceClient = new ServiceClient(
                        new InstanceContext(_clientCallback),
                        String.Empty,
                        string.Format(RemoteAddressFormat, remoteServerAddress));

                _serviceClient.InnerChannel.Closed += OnChannelClosed;
                _serviceClient.InnerDuplexChannel.Closed += OnChannelClosed;
                _serviceClient.InnerChannel.Faulted += OnChannelFaulted;
                _serviceClient.InnerDuplexChannel.Faulted += OnChannelFaulted;

                lock (_clientLock)
                {
                    //_serviceClient.Open();
                    _connectionEstablished = true;
                    _isConnected = true;
                }

                OnConnected();

                var operationFailed = false;

                var joinGameResult = _serviceClient.JoinGame(playerName, out Player localPlayer, out LobbyData lobbyData);
                switch (joinGameResult)
                {
                    case JoinGameResult.Success:
                        Observable.ToAsync(
                            () => ClientEvents.LobbyUpdated.Publish(new ClientDataEventArgs<ILobbyData>(lobbyData)),
                            _scheduler)();
                        break;
                    case JoinGameResult.GameIsFull:
                        _disconnectReason = ClientDisconnectReason.GameIsFull;
                        operationFailed = true;
                        break;
                    case JoinGameResult.GameAlreadyStarted:
                        _disconnectReason = ClientDisconnectReason.GameAlreadyStarted;
                        operationFailed = true;
                        break;
                    case JoinGameResult.VersionMismatch:
                        _disconnectReason = ClientDisconnectReason.VersionMismatch;
                        operationFailed = true;
                        break;
                    default:
                    case JoinGameResult.ConnectionFailure:
                        _disconnectReason = ClientDisconnectReason.UnknownFailure;
                        operationFailed = true;
                        break;
                }

                if (operationFailed)
                    Disconnect();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        protected void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("GameClient");
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            lock (_clientLock)
            {
                if (!_connectionEstablished)
                    return;
            }
            if (_isDisconnecting)
                return;
            if (!_disconnectReason.HasValue)
                _disconnectReason = ClientDisconnectReason.ConnectionClosed;
            Disconnect();
        }

        public void HostAndConnect(GameInitData initData, IPAddress remoteServerAddress)
        {
            if (initData == null)
                throw new ArgumentNullException("initData");
            if (remoteServerAddress == null)
                throw new ArgumentNullException("remoteServerAddress");

            CheckDisposed();

            try
            {
                _serviceClient = new ServiceClient(
                        new InstanceContext(_clientCallback),
                        LocalEndpointConfigurationName);

                _serviceClient.InnerChannel.Closed += OnChannelClosed;
                _serviceClient.InnerDuplexChannel.Closed += OnChannelClosed;
                _serviceClient.InnerChannel.Faulted += OnChannelFaulted;
                _serviceClient.InnerDuplexChannel.Faulted += OnChannelFaulted;

                lock (_clientLock)
                {
                    _connectionEstablished = true;
                    _isConnected = true;
                }

                OnConnected();

                bool operationFailed = false;

                var hostGameResult = _serviceClient.HostGame(initData, out Player localPlayer, out LobbyData lobbyData);

                switch (hostGameResult)
                {
                    case HostGameResult.Success:
                        _saveGameCommand.IsActive = true;
                        Observable.ToAsync(
                            () => ClientEvents.LobbyUpdated.Publish(new ClientDataEventArgs<ILobbyData>(lobbyData)),
                            _scheduler)();
                        break;
                    case HostGameResult.LoadGameFailure:
                        _disconnectReason = ClientDisconnectReason.LoadGameFailure;
                        operationFailed = true;
                        break;
                    case HostGameResult.ChannelFaultFailure:
                        _disconnectReason = ClientDisconnectReason.LocalServiceFailure;
                        operationFailed = true;
                        break;
                    default:
                    case HostGameResult.ServiceAlreadyRunning:
                    case HostGameResult.UnknownFailure:
                        _disconnectReason = ClientDisconnectReason.UnknownFailure;
                        operationFailed = true;
                        break;
                }

                if (operationFailed)
                    Disconnect();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Disconnect()
        {
            Dispose();
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisconnecting = true;

            UnhookCommandAndEventHandlers();

            try
            {
                ServiceClient serviceClient;

                lock (_clientLock)
                {
                    if (_isConnected && !_disconnectReason.HasValue)
                        _disconnectReason = ClientDisconnectReason.Disconnected;
                    _isConnected = false;
                    serviceClient = Interlocked.Exchange(ref _serviceClient, null);
                }

                if (serviceClient != null)
                {
                    try
                    {
                        serviceClient.InnerChannel.Closed -= OnChannelClosed;
                        serviceClient.InnerDuplexChannel.Closed -= OnChannelClosed;
                        serviceClient.InnerChannel.Faulted -= OnChannelFaulted;
                        serviceClient.InnerDuplexChannel.Faulted -= OnChannelFaulted;
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.General.Error(e);
                    }

            if (!serviceClient.IsClosed)
                    {
                        try
                        {
                            serviceClient.Disconnect();
                        }
                        catch (Exception e)
                        {
                            GameLog.Client.General.Error(e);
                        }

                        try
                        {
                            serviceClient.Close();
                        }
                        catch(Exception e)
                        {
                            GameLog.Client.General.Error(e);
                        }
                    }
                    
                    if (_connectionEstablished)
                        OnDisconnected();
                }

                ((GameClientCallback)_clientCallback).Dispose();
            }
            finally
            {
                _isDisposed = true;
            }
        }
        #endregion

        private void OnConnected()
        {
            HookCommandAndEventHandlers();

            var handler = Connected;
            if (handler != null)
                handler(ClientEventArgs.Default);
        }

        private void HookCommandAndEventHandlers()
        {
            lock (_eventLock)
            {
                if (_eventAndCommandHandlersHooked)
                    return;

                ClientCommands.SendChatMessage.RegisterCommand(_sendChatMessageCommand);
                ClientCommands.SendCombatOrders.RegisterCommand(_sendCombatOrdersCommand);
                ClientCommands.SendCombatTarget1.RegisterCommand(_sendCombatTarget1Command);
                ClientCommands.SendCombatTarget2.RegisterCommand(_sendCombatTarget2Command);
                ClientCommands.SendInvasionOrders.RegisterCommand(_sendInvasionOrdersCommand);
                ClientCommands.EndInvasion.RegisterCommand(_endInvasionCommand);
                ClientCommands.SaveGame.RegisterCommand(_saveGameCommand);
                ClientCommands.AssignPlayerSlot.RegisterCommand(_assignPlayerSlotCommand);
                ClientCommands.ClearPlayerSlot.RegisterCommand(_clearPlayerSlotCommand);
                ClientCommands.ClosePlayerSlot.RegisterCommand(_closePlayerSlotCommand);
                ClientCommands.StartMultiplayerGame.RegisterCommand(_startMultiplayerGameCommand);
                ClientCommands.SendUpdatedGameOptions.RegisterCommand(_sendUpdatedGameOptionsCommand);

                ClientEvents.TurnEnded.Subscribe(OnTurnEnded, ThreadOption.BackgroundThread);
                ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded, ThreadOption.BackgroundThread);
                ClientEvents.ServerHeartbeat.Subscribe(OnServerHeartbeat, ThreadOption.BackgroundThread);
                ClientEvents.GameObjectIDRequested.Subscribe(OnGameObjectIDRequested, ThreadOption.BackgroundThread);

                _eventAndCommandHandlersHooked = true;
            }
        }

        private void OnGameObjectIDRequested(GameObjectIDRequestEventArgs args)
        {
            if (args == null)
                return;
            ExecuteRemoteCommand(() => args.Value = _serviceClient.GetNewObjectID());
            args.WaitHandle.Set();
        }

        private void ExecuteSendCombatOrdersCommand(CombatOrders orders)
        {
            ExecuteRemoteCommand(() => _serviceClient.SendCombatOrders(orders));
        }

        private void ExecuteSendCombatTarget1Command(CombatTargetPrimaries target1)
        {
            ExecuteRemoteCommand(() => _serviceClient.SendCombatTarget1(target1));
        }

        private void ExecuteSendCombatTarget2Command(CombatTargetSecondaries target2)
        {
            ExecuteRemoteCommand(() => _serviceClient.SendCombatTarget2(target2));
        }

        private void ExecuteSendInvasionOrdersCommand(InvasionOrders orders)
        {
            ExecuteRemoteCommand(() => _serviceClient.SendInvasionOrders(orders));
        }

        private void ExecuteEndInvasionCommand(object _)
        {
            ExecuteRemoteCommand(() => _serviceClient.NotifyInvasionScreenReady());
        }

        private void ExecuteSendChatMessageCommand(ChatMessage message)
        {
            ExecuteRemoteCommand(
                () => _serviceClient.SendChatMessage(
                          message.Message,
                          message.IsGlobalMessage ? -1 : message.Recipient.PlayerID));
        }

        private void OnTurnEnded(EventArgs args)
        {
            //var soundPlayer = new SoundPlayer("Resources/SoundFX/Turn.wav");
            //{
            //    if (File.Exists("Resources/SoundFX/Turn.wav"));
            //    soundPlayer.Play();
            //}  

            PlayerOrderServiceOnOrdersChanged(null, null);
            _playerOrderService.OrdersChanged += PlayerOrderServiceOnOrdersChanged;
            //PlayerTarget1ServiceOnTarget1Changed(null, null);
            //_playerTarget1Service.Target1Changed += PlayerTarget1ServiceOnTarget1Changed;
            //PlayerTarget2ServiceOnTarget2Changed(null, null);
            //_playerTarget2Service.Target2Changed += PlayerTarget2ServiceOnTarget2Changed;
        }

        private void PlayerTarget1ServiceOnTarget1Changed(object sender, EventArgs eventArgs)
        {
            ServiceClient serviceClient;
            lock (_clientLock)
            {
                if (!_isConnected)
                    return;
                serviceClient = Interlocked.CompareExchange(ref _serviceClient, null, null);
            }
            if (serviceClient == null)
                return;

            //try
            //{
            //    var message = new PlayerTarget1Message(_playerTarget1Service.Target1, _playerTarget1Service.AutoTurnTarget1);
            //    //serviceClient.EndTurn(message);
            //}
            //catch (Exception e)
            //{
            //    _logger.Log(
            //        "Exception occurred while submitting end-of-turn target1: " + e.Message,
            //        Category.Exception,
            //        Priority.High);
            //    throw;
            //}
        }

        private void PlayerTarget2ServiceOnTarget2Changed(object sender, EventArgs eventArgs)
        {
            ServiceClient serviceClient;
            lock (_clientLock)
            {
                if (!_isConnected)
                    return;
                serviceClient = Interlocked.CompareExchange(ref _serviceClient, null, null);
            }
            if (serviceClient == null)
                return;

            //try
            //{
            //    var message = new PlayerTarget2Message(_playerTarget2Service.Target2, _playerTarget2Service.AutoTurnTarget2);
            //    //serviceClient.EndTurn(message);
            //}

            //catch (Exception e)
            //{
            //    _logger.Log(
            //        "Exception occurred while submitting end-of-turn target1: " + e.Message,
            //        Category.Exception,
            //        Priority.High);
            //    throw;
            //}
        }

        private void PlayerOrderServiceOnOrdersChanged(object sender, EventArgs eventArgs)
        {
            ServiceClient serviceClient;
            lock (_clientLock)
            {
                if (!_isConnected)
                    return;
                serviceClient = Interlocked.CompareExchange(ref _serviceClient, null, null);
            }
            if (serviceClient == null)
                return;

            try
            {
                var messageOrder = new PlayerOrdersMessage(_playerOrderService.Orders, _playerOrderService.AutoTurn);
                //var messageTarget1 = new PlayerTarget1Message(_playerTarget1Service.Target1, _playerTarget1Service.AutoTurnTarget1);
                //var messageTarget2 = new PlayerTarget2Message(_playerTarget2Service.Target2, _playerTarget2Service.AutoTurnTarget2);

                serviceClient.EndTurn(messageOrder);
                //serviceClient.EndTurn(messageTarget1);
                //serviceClient.EndTurn(messageTarget2);
            }
            catch (Exception e)
            {
                _logger.Log(
                    "Exception occurred while submitting end-of-turn orders: " + e.Message,
                    Category.Exception,
                    Priority.High);
                throw;
            }
        }




        private void OnAllTurnEnded(EventArgs args)
        {
            _playerOrderService.OrdersChanged -= PlayerOrderServiceOnOrdersChanged;
            _playerOrderService.ClearOrders();
            //_playerTarget1Service.Target1Changed -= PlayerTarget1ServiceOnTarget1Changed;
            //_playerTarget1Service.ClearTarget1();
            //_playerTarget2Service.Target2Changed -= PlayerTarget2ServiceOnTarget2Changed;
            //_playerTarget2Service.ClearTarget2();
        }


        private void UnhookCommandAndEventHandlers()
        {
            lock (_eventLock)
            {
                if (!_eventAndCommandHandlersHooked)
                    return;

                _eventAndCommandHandlersHooked = false;

                ClientCommands.SendChatMessage.UnregisterCommand(_sendChatMessageCommand);
                ClientCommands.SendCombatOrders.UnregisterCommand(_sendCombatOrdersCommand);
                ClientCommands.SendCombatTarget1.UnregisterCommand(_sendCombatTarget1Command);
                ClientCommands.SendCombatTarget2.UnregisterCommand(_sendCombatTarget2Command);
                ClientCommands.SendInvasionOrders.UnregisterCommand(_sendInvasionOrdersCommand);
                ClientCommands.EndInvasion.UnregisterCommand(_endInvasionCommand);
                ClientCommands.SaveGame.UnregisterCommand(_saveGameCommand);
                ClientCommands.AssignPlayerSlot.UnregisterCommand(_assignPlayerSlotCommand);
                ClientCommands.ClearPlayerSlot.UnregisterCommand(_clearPlayerSlotCommand);
                ClientCommands.ClosePlayerSlot.UnregisterCommand(_closePlayerSlotCommand);
                ClientCommands.StartMultiplayerGame.UnregisterCommand(_startMultiplayerGameCommand);
                ClientCommands.SendUpdatedGameOptions.UnregisterCommand(_sendUpdatedGameOptionsCommand);

                ClientEvents.TurnEnded.Unsubscribe(OnTurnEnded);
                ClientEvents.AllTurnEnded.Unsubscribe(OnAllTurnEnded);
                ClientEvents.ServerHeartbeat.Unsubscribe(OnServerHeartbeat);
                ClientEvents.GameObjectIDRequested.Unsubscribe(OnGameObjectIDRequested);
            }
        }

        private void OnServerHeartbeat(EventArgs args)
        {
            ServiceClient serviceClient;

            lock (_clientLock)
            {
                if (!_isConnected)
                    return;
                serviceClient = _serviceClient;
            }

            if (serviceClient == null)
                return;

            try { serviceClient.Pong(0); }
            catch (Exception e)
            {
                lock (_clientLock)
                {
                    if (!_isConnected)
                        return;
                    _logger.Log(
                        "Exception occurred while responding to service heartbeat: " + e.Message,
                        Category.Warn,
                        Priority.Low);
                }
            }
        }

        private void OnDisconnected()
        {
            UnhookCommandAndEventHandlers();

            if (!_disconnectReason.HasValue)
            {
                _disconnectReason = ClientDisconnectReason.ConnectionBroken;
            }

            var handler = Disconnected;
            if (handler != null)
                handler(new ClientDataEventArgs<ClientDisconnectReason>(_disconnectReason.Value));
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            lock (_clientLock)
            {
                if (!_connectionEstablished)
                    return;
                _isConnected = false;
            }

            if (!_disconnectReason.HasValue)
                _disconnectReason = ClientDisconnectReason.ConnectionBroken;

            UnhookCommandAndEventHandlers();

            Dispose();
        }
    }
}