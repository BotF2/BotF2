// HumanClient.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Combat;

namespace Supremacy.Client
{
    internal delegate void SendChatMessageDelegate(string message, int recipientId);

    internal delegate void SendCombatOrdersDelegate(CombatOrders orders);

    internal delegate void FinishTurnDelegate();

/*
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class HumanClient : ClientBase, ISupremacyCallback
    {
        #region Static Members
        private const string ServiceDomainName = "Supremacy Service Domain";

        private static bool Initialized;
        private static HumanClient Instance;

        static HumanClient()
        {
            if (!Initialized)
            {
                Initialize();
            }
        }

        public static new HumanClient Current
        {
            get { return ClientBase.Current as HumanClient; }
            set { ClientBase.Current = value; }
        }

        public static void Initialize()
        {
            if (!Initialized)
            {
                Instance = new HumanClient();
                ClientBase.Current = Instance;
                Initialized = true;
            }
        }
        #endregion

        #region Fields
        private readonly object _serviceLock;

        private GameContext _game;
        private bool _isGameHost;
        private bool _isSinglePlayerGame;
        private bool _isTurnFinished;
        private LobbyData _lobbyData;
        private bool _isServiceLoaded;
        private ServiceClient _serviceClient;
        private AppDomain _serviceDomain;
        private SupremacyServiceHost _serviceHost;
        private Player _localPlayer;
        private IList<Player> _players;
        private TurnPhase _turnPhase;
        #endregion

        #region Constructors
        public HumanClient()
        {
            _serviceLock = new object();
        }
        #endregion

        public Dispatcher Dispatcher
        {
            get { return Application.Current.Dispatcher; }
        }

        #region Properties
        public override GameContext Game
        {
            get { return _game; }
            protected set
            {
                _game = value;
                OnPropertyChanged("Game");
                OnPropertyChanged("IsGameInPlay");
            }
        }

        public override CivilizationManager CivilizationManager
        {
            get
            {
                GameContext game = GameContext.Current;
                if (game != null)
                {
                    Player localPlayer = LocalPlayer;
                    if (localPlayer != null)
                    {
                        return game.CivilizationManagers[localPlayer.LocalPlayerEmpireID];
                    }
                }
                return null;
            }
            // ReSharper disable ValueParameterNotUsed
            protected internal set { OnPropertyChanged("CivilizationManager"); }
            // ReSharper restore ValueParameterNotUsed
        }

        public override Player LocalPlayer
        {
            get { return _localPlayer; }
            protected set
            {
                _localPlayer = value;
                OnPropertyChanged("LocalPlayer");
            }
        }

        public override LobbyData LobbyData
        {
            get { return _lobbyData; }
            protected set
            {
                _lobbyData = value;
                OnPropertyChanged("LobbyData");
            }
        }

        public override bool IsConnected
        {
            get
            {
                if (_serviceClient != null)
                {
                    return (_serviceClient.State == CommunicationState.Opened);
                }
                return false;
            }
        }

        public override bool IsGameHost
        {
            get { return _isGameHost; }
        }

        public override bool IsGameInPlay
        {
            get { return (_game != null); }
        }

        public override bool IsTurnFinished
        {
            get { return IsGameInPlay && _isTurnFinished; }
        }

        public override bool IsSinglePlayerGame
        {
            get { return _isSinglePlayerGame; }
        }

        public override IList<Player> Players
        {
            get { return _players; }
            protected set
            {
                _players = value;
                OnPropertyChanged("Players");
            }
        }

        public override TurnPhase TurnPhase
        {
            get { return _turnPhase; }
            protected set
            {
                _turnPhase = value;
                OnPropertyChanged("TurnPhase");
            }
        }
        #endregion

        #region Methods
        public override void Alert(string message)
        {
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.Send,
                (SetterFunction<string>)ShowAlert,
                message);
        }

        private static void ShowAlert(string message)
        {
            MessageDialog.Show(
                "Attention",
                message,
                MessageDialogButtons.Ok);
        }

        protected Player GetPlayerFromID(int playerId)
        {
            if (playerId < 0)
            {
                return null;
            }

            if (IsGameInPlay)
            {
                if (Players != null)
                {
                    foreach (Player player in Players)
                    {
                        if (player.PlayerID == playerId)
                            return player;
                    }
                }
            }
            if ((LobbyData != null)
                && (playerId >= 0)
                    && (playerId < LobbyData.Players.Length))
            {
                foreach (Player player in LobbyData.Players)
                {
                    if (player.PlayerID == playerId)
                        return player;
                }
            }
            return null;
        }

        protected void CreateRemoteClient(IPAddress address)
        {
            EnsureNoClient();

            _serviceClient = new ServiceClient(
                new InstanceContext(this),
                "",
                "net.tcp://" + address + ":4455/SupremacyService");
        }

        private void OnServiceClientChannelFaulted(object sender, EventArgs e)
        {
            OnConnectionBroken();
            Disconnect();
        }

        protected void CreateLocalClient()
        {
            EnsureNoClient();

            _serviceClient = new ServiceClient(
                new InstanceContext(this),
                "LocalEndpoint");
        }

        protected void EnsureNotConnected()
        {
            bool wasConnected = IsConnected;

            if (_serviceClient != null)
            {
                wasConnected = true;
                try
                {
                    if ((_serviceClient.InnerChannel.State == CommunicationState.Opening)
                        || (_serviceClient.InnerChannel.State == CommunicationState.Opened))
                    {
                        _serviceClient.Disconnect();
                    }
                    if (_serviceClient.State != CommunicationState.Faulted)
                        _serviceClient.Close();
                }
                catch {}
            }

            if (_isGameHost)
            {
                EnsureNoServerHost();
                _isGameHost = false;
                OnPropertyChanged("IsGameHost");
            }

            EnsureNoClient();

            if (_isTurnFinished)
            {
                _isTurnFinished = false;
                OnPropertyChanged("IsTurnFinished");
            }

            if (_isSinglePlayerGame)
            {
                _isSinglePlayerGame = false;
                OnPropertyChanged("IsSinglePlayerGame");
            }

            if (wasConnected)
            {
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("IsGameHost");
                OnPropertyChanged("IsGameInPlay");
                OnDisconnected();
            }

            LocalPlayer = null;
            CivilizationManager = null;
            LobbyData = null;
            Players = null;

            if (Game != null)
            {
                GameContext.CheckAndPop(Game);
                Game = null;
                GC.Collect();
            }
        }

        protected void EnsureNoClient()
        {
            if (_serviceClient != null)
            {
                try
                {
                    if ((_serviceClient.State != CommunicationState.Closed)
                        && (_serviceClient.State != CommunicationState.Closing))
                    {
                        _serviceClient.Close();
                    }
                }
                catch {}
                finally
                {
                    _serviceClient = null;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        protected void EnsureNoServerHost()
        {
            bool doGarbageCollect = false;

            lock (_serviceLock)
            {
                var serviceHost = Interlocked.Exchange(ref _serviceHost, null);
                if (serviceHost != null)
                {
                    try
                    {
                        serviceHost.StopService();
                    }
                    catch {}
                    doGarbageCollect = true;
                }

                //var serviceDomain = Interlocked.CompareExchange(ref _serviceDomain, null, null);
                //if (serviceDomain != null)
                //{
                //    try
                //    {
                //        AppDomain.Unload(serviceDomain);
                //    }
                //    catch {}
                //    doGarbageCollect = true;
                //}

                _isServiceLoaded = false;
            }

            if (doGarbageCollect)
            {
                GC.Collect();
            }
        }

        public override void LoadSinglePlayerGame(string fileName)
        {
            HostGame("Player", fileName, default(GameOptions), true);
        }

        public override void HostSinglePlayerGame(GameOptions options, int empireId)
        {
            HostGame("Player", null, options, empireId, true);
        }

        private void HostGame(string playerName, string fileName, GameOptions options, int empireId, bool isSinglePlayerGame)
        {
            HostGameResult result;

            EnsureNoClient();
            EnsureNoServerHost();

            EnsureServerHost();

            _serviceHost.StartService(isSinglePlayerGame);

            try
            {
                if (isSinglePlayerGame)
                {
                    CreateLocalClient();
                }
                else
                {
                    CreateRemoteClient(IPAddress.Loopback);
                }
                _serviceClient.Open();
                _serviceClient.InnerChannel.Faulted += OnServiceClientChannelFaulted;
                _serviceClient.InnerDuplexChannel.Faulted += OnServiceClientChannelFaulted;
            }
            catch
            {
                EnsureNoServerHost();
            }

            if (fileName != null)
            {
                result = _serviceClient.LoadSinglePlayerGame(fileName, out _localPlayer);
            }
            else
            {
                if (isSinglePlayerGame)
                {
                    result = _serviceClient.HostSinglePlayerGame(
                        options ?? GameOptionsManager.LoadDefaults(),
                        empireId,
                        out _localPlayer);
                }
                else
                {
                    result = _serviceClient.HostGame(playerName, out _localPlayer, out _lobbyData);
                }
            }

            switch (result)
            {
                case HostGameResult.Success:
                    List<Player> players = new List<Player>
                                           {
                                               _localPlayer
                                           };
                    Players = players;
                    _isGameHost = true;
                    OnPropertyChanged("LocalPlayer");
                    if (isSinglePlayerGame)
                    {
                        _isSinglePlayerGame = true;
                        OnPropertyChanged("IsSinglePlayerGame");
                    }
                    else
                    {
                        OnPropertyChanged("LobbyData");
                    }
                    OnConnected();
                    OnLobbyUpdated(_lobbyData);
                    OnPlayerJoined(_localPlayer);
                    break;
                default:
                    EnsureNoClient();
                    EnsureNoServerHost();
                    Alert("Connection Failed");
                    break;
            }
        }

        public override void HostGame(string playerName)
        {
            HostGame(playerName, null, null, false);
        }

        protected void EnsureServerHost()
        {
            lock (_serviceLock)
            {
                if (!_isServiceLoaded)
                {
                    try
                    {
                        if (Interlocked.CompareExchange(ref _serviceDomain, null, null) == null)
                        {
                            var serviceDomain = AppDomain.CreateDomain(
                                ServiceDomainName,
                                Assembly.GetExecutingAssembly().Evidence,
                                AppDomain.CurrentDomain.SetupInformation);
                            serviceDomain.Load("SupremacyNative");
                            serviceDomain.Load("SupremacyCore");
                            serviceDomain.Load("SupremacyService");
                            serviceDomain.UnhandledException += (sender, e) =>
                                                                {
                                                                    if (
                                                                        !(e.ExceptionObject is
                                                                          AppDomainUnloadedException))
                                                                    {
                                                                        ClientApp.HandleError(
                                                                            e.ExceptionObject as Exception);
                                                                    }
                                                                };
                            Interlocked.CompareExchange(ref _serviceDomain, serviceDomain, null);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new SupremacyException(
                            "Failed to create application domain for Supremacy Service", e);
                    }

                    try
                    {
                        var serviceHost = (SupremacyServiceHost)_serviceDomain.CreateInstanceAndUnwrap(
                            "SupremacyService",
                            "Supremacy.WCF.SupremacyServiceHost");
                        Interlocked.CompareExchange(ref _serviceHost, serviceHost, null);
                        _isServiceLoaded = true;
                    }
                    catch (Exception e)
                    {
                        throw new SupremacyException("Failed to Supremacy Service Host", e);
                    }
                }
            }
        }

        protected void HostGame(string playerName, string fileName, GameOptions options, bool isSinglePlayerGame)
        {
            HostGame(playerName, fileName, options, 0, isSinglePlayerGame);
        }

        public override void JoinGame(string playerName, IPAddress server)
        {
            try
            {
                EnsureNotConnected();
                EnsureNoClient();
                EnsureNoServerHost();
                CreateRemoteClient(server);
                _serviceClient.Open();
                _isSinglePlayerGame = false;
            }
            catch (Exception e)
            {
                Logger.WriteError(e.Message);
                EnsureNoClient();
                throw;
            }

            JoinGameResult result = _serviceClient.JoinGame(playerName, out _localPlayer, out _lobbyData);
            switch (result)
            {
                case JoinGameResult.Success:
                    OnPropertyChanged("LocalPlayer");
                    OnPropertyChanged("LobbyData");
                    OnConnected();
                    OnLobbyUpdated(_lobbyData);
                    break;
                default:
                    EnsureNoClient();
                    Alert("Connection Failed (" + result + ")");
                    break;
            }
        }

        public override void FinishTurn()
        {
            if (IsGameInPlay && !IsTurnFinished)
            {
                _isTurnFinished = true;
                OnPropertyChanged("IsTurnFinished");
                OnPlayerTurnFinished();
                ClientEvents.TurnEnded.Publish(ClientEventArgs.Default);
                AsyncHelper.Invoke((FinishTurnDelegate)DoFinishTurn);
            }
        }

        private void DoFinishTurn()
        {
            _serviceClient.EndTurn(new PlayerOrdersMessage(Orders));
            ClearOrders();
        }

        public override void UpdateGameOptions(GameOptions options)
        {
            if (IsConnected && IsGameHost && !IsGameInPlay && !IsSinglePlayerGame)
            {
                _serviceClient.UpdateGameOptions(options);
            }
        }

        public override void UpdateEmpireSelection(int playerId, int empireId)
        {
            if (IsConnected && !IsGameInPlay && !IsSinglePlayerGame)
            {
                _serviceClient.UpdateEmpireSelection(playerId, empireId);
            }
        }

        public override void Disconnect()
        {
            EnsureNotConnected();
            EnsureNoServerHost();
        }

        public override void StartGame()
        {
            if (IsConnected && !IsGameInPlay)
            {
                _serviceClient.StartGame();
            }
        }

        public override void EndGame()
        {
            Disconnect();
        }

        public override void SendChatMessage(string message)
        {
            AsyncHelper.Invoke(
                (SendChatMessageDelegate)_serviceClient.SendChatMessage,
                message,
                -1);
        }

        public override void SendCombatOrders(CombatOrders orders)
        {
            if (orders == null)
                throw new ArgumentNullException("orders");
            if (_serviceClient != null)
            {
                AsyncHelper.Invoke(
                    (SendCombatOrdersDelegate)_serviceClient.SendCombatOrders,
                    orders);
            }
        }

        public override void SendChatMessage(Player recipient, string message)
        {
            if (recipient == null)
            {
                SendChatMessage(message);
            }
            else
            {
                AsyncHelper.Invoke(
                    (SendChatMessageDelegate)_serviceClient.SendChatMessage,
                    message,
                    recipient.PlayerID);
            }
        }

        public override int GetNewObjectID()
        {
            if (IsConnected)
            {
                if (IsGameInPlay)
                {
                    return _serviceClient.GetNewObjectID();
                }
                throw new InvalidOperationException("Game is not in play");
            }
            throw new InvalidOperationException("Not connected");
        }

        public override void SaveGame(string fileName)
        {
            if (IsConnected && IsGameInPlay && IsGameHost)
            {
                _serviceClient.SaveGame(fileName);
            }
        }
        #endregion

        #region ISupremacyCallback Members
        public void NotifyOnJoin(Player localPlayer, LobbyData lobbyData)
        {
            _localPlayer = localPlayer;
            _lobbyData = lobbyData;
        }

        public void NotifyPlayerJoined(Player player)
        {
            if (Players != null)
                Players.Add(player);
            OnPlayerJoined(player);
        }

        public void NotifyPlayerExited(Player player)
        {
            if (Players != null)
                Players.Remove(player);
            if (IsGameInPlay)
                Alert(player.Name + " has left the game.");
            OnPlayerExited(player);
            if (player.IsGameHost)
                Disconnect();
        }

        public void NotifyGameDataUpdated(GameUpdateMessage updateMessage)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                (Action<GameUpdateMessage>)ProcessGameUpdate,
                updateMessage);
        }

        private void ProcessGameUpdate(GameUpdateMessage updateMessage)
        {
            //updateMessage.Data.UpdateLocalGame();
            OnPropertyChanged("Game");
            OnPropertyChanged("CivilizationManager");
        }

        public void NotifyTurnProgressChanged(TurnPhase phase)
        {
            TurnPhase = phase;
            OnTurnPhaseChanged(phase);
        }

        public void NotifyChatMessageReceived(int senderId, string message, int recipientId)
        {
            Player sender = GetPlayerFromID(senderId);
            if ((sender != null) && (message != null))
            {
                ChatMessage chatMessage = new ChatMessage(
                    sender,
                    message,
                    GetPlayerFromID(recipientId));
                OnChatMessageReceived(chatMessage);
            }
        }

        public void NotifyGameStarting()
        {
            OnGameStarting();
        }

        public void NotifyGameStarted(GameStartMessage startMessage)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                (SetterFunction<GameStartMessage>)HandleGameStarted,
                startMessage);
        }

        private void HandleGameStarted(GameStartMessage startMessage)
        {
            GameStartData startData = startMessage.Data;
            if (Game != null)
            {
                GameContext.CheckAndPop(Game);
            }
            Game = startData.CreateLocalGame();
            GameContext.Push(Game);
            OnGameStarted();
            //ClientEvents.GameStarted.Publish(new DataEventArgs<GameStartData>(startData));
            //ClientEvents.TurnStarted.Publish(new GameContextEventArgs(_appContext, _appContext.CurrentGame));
        }

        public void NotifyTurnFinished()
        {
            _isTurnFinished = false;
            OnPropertyChanged("IsTurnFinished");
            OnTurnFinished();
            //ClientEvents.TurnStarted.Publish(ClientEventArgs.Default);
        }

        public void Ping()
        {
            if (_serviceClient != null)
            {
                try
                {
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        (Action<int>)_serviceClient.Pong,
                        0);
                }
                catch (FaultException) {}
            }
        }

        public void NotifyCombatUpdate(CombatUpdate update)
        {
            OnCombatUpdate(update);
        }

        public void NotifyLobbyUpdated(LobbyData lobbyData)
        {
            LobbyData = lobbyData;
            Players = new List<Player>(lobbyData.Players);
            LocalPlayer = GetPlayerFromID(LocalPlayer.PlayerID);
            OnLobbyUpdated(lobbyData);
        }

        public void NotifyDisconnected()
        {
            EnsureNotConnected();
            EnsureNoClient();
            EnsureNoServerHost();
            Alert("Disconnected");
        }
        #endregion
    }
*/
}