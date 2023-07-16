// File:SupremacyService.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Client.Commands;
using Supremacy.Client.Services;
using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Messaging;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Text;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Input;
using Scheduler = System.Concurrency.Scheduler;

namespace Supremacy.WCF
{
    internal delegate void DropPlayerDelegate(Player player);

    internal delegate void NotifyChatMessageReceivedDelegate(int senderId, string message, int recipientId);

    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.Single,
        IgnoreExtensionDataObject = true,
        UseSynchronizationContext = false)]

    public class SupremacyService : ISupremacyService
    {
        #region Fields

        //private static readonly Lazy<GameLog> _log = new Lazy<GameLog>(() => GameLog.Server);

        private readonly object _aiAsyncLock;
        private readonly Dictionary<Player, PlayerOrdersMessage> _playerOrders;
        private readonly ServerPlayerInfoCollection _playerInfo;
        private readonly IGameErrorService _errorService;
        private readonly IScheduler _scheduler;
        private readonly IScheduler _threadPoolScheduler;
        private GameInitData _gameInitData;
        private IAsyncResult _aiAsyncResult;
        private CombatEngine _combatEngine;
        //private IntelEngine _intelEngine;
        private InvasionEngine _invasionEngine;
        private GameContext _game;
        //private Civilization _alreadyDidCivAsAI;
        private bool _isGameStarted;
        private bool _isGameEnding;
        private int _isProcessingTurn;
        private GameEngine _gameEngine;
        private IDisposable _heartbeat;
        private string _text;
        #endregion

        #region Constructors
        public SupremacyService()
        {
            _aiAsyncLock = new object();
            _errorService = ServiceLocator.Current.GetInstance<IGameErrorService>();
            _playerInfo = new ServerPlayerInfoCollection();
            _playerOrders = new Dictionary<Player, PlayerOrdersMessage>();
            LobbyData = new LobbyData
            {
                Players = _playerInfo.Select(o => o.Player).ToArray()
            };
            _scheduler = new EventLoopScheduler("ServerEventLoop").AsGameScheduler(() => _game);

            PlayerContext.Current = new PlayerContext(
                new DelegatingIndexedCollection<Player, ServerPlayerInfoCollection>(
                    _playerInfo,
                    collection => collection.Select(o => o.Player),
                    collection => collection.Count,
                    (collection, index) => collection[index].Player));

            _threadPoolScheduler = Scheduler.ThreadPool.AsGameScheduler(() => _game);
        }
        #endregion

        #region Properties

        internal static ISupremacyCallback Callback => OperationContext.Current.GetCallbackChannel<ISupremacyCallback>();

        internal Player CurrentPlayer
        {
            get
            {
                ServerPlayerInfo playerInfo = _playerInfo.FromSessionId(OperationContext.Current.SessionId);
                if (playerInfo != null)
                {
                    return playerInfo.Player;
                }

                return null;
            }
        }

        internal ServerPlayerInfo CurrentPlayerInfo
        {
            get
            {
                OperationContext operationContext = OperationContext.Current;
                if (operationContext == null)
                {
                    return null;
                }

                return _playerInfo.FromSessionId(operationContext.SessionId);
            }
        }

        internal ServiceHost Host { get; set; }

        internal LobbyData LobbyData { get; }
        #endregion

        #region Methods
        internal void DoStartGame()
        {
            if (_isGameStarted)
            {
                return;
            }

            _isGameStarted = true;

            try
            {
                if (_playerInfo.Count > 1)
                {
                    SendLobbyUpdate();
                }

                lock (_playerInfo.SyncRoot)
                {
                    foreach (ServerPlayerInfo playerInfo in _playerInfo)
                    {
                        Player player = playerInfo.Player;

                        _ = ((Action)playerInfo.Callback.NotifyGameStarting)
                            .ToAsync(playerInfo.Scheduler)()
                            .Subscribe(
                                _ => { },
                                e => DropPlayer(player));
                    }
                }

                if (_gameEngine == null)
                {
                    _gameEngine = new GameEngine();
                    _gameEngine.CombatOccurring += OnCombatOccurring;
                    _gameEngine.InvasionOccurring += OnInvasionOccurring;
                }

                if (_game == null)
                {
                    if ((_gameInitData != null) &&
                        ((_gameInitData.GameType == GameType.SinglePlayerLoad) || (_gameInitData.GameType == GameType.MultiplayerLoad)))
                    {
                        //NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
                        if (!SavedGameManager.LoadGame(_gameInitData.SaveGameFileName, out SavedGameHeader header, out _game, out DateTime timestamp))
                        {
                            _text = "Step_4987: Loading failed - end game";
                            Console.WriteLine(_text);
                            EndGame();
                            
                            return;
                        }
                    }
                    else
                    {
                        _game = GameContext.Create(LobbyData.GameOptions, LobbyData.IsMultiplayerGame);
                        GameContext.PushThreadContext(_game);
                        try
                        {
                            _gameEngine.DoPreGameSetup(_game);
                        }
                        finally
                        {
                            _ = GameContext.PopThreadContext();
                        }
                    }
                }

                ClientTextDatabase textDatabase = ClientTextDatabase.Load(
                    ResourceManager.GetResourcePath("Resources\\Data\\TextDatabase.xml"));

                lock (_playerInfo.SyncRoot)
                {
                    foreach (ServerPlayerInfo playerInfo in Enumerable.ToArray(_playerInfo))
                    {
                        try
                        {
                            if (playerInfo.Callback == null)
                            {
                                continue;
                            }

                            Player player = playerInfo.Player;
                            GameStartMessage message = new GameStartMessage(GameStartData.Create(_game, player, textDatabase));

                            _ = ((Action<GameStartMessage>)playerInfo.Callback.NotifyGameStarted)
                                .ToAsync(_scheduler)(message)
                                .Subscribe(
                                    _ => { },
                                    e => DropPlayerAsync(player));
                            
                            //_navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
                        }
                        catch
                        {
                            if (playerInfo.Player.IsGameHost)
                            {
                                throw;
                            }

                            DropPlayerAsync(playerInfo.Player);
                        }
                        //_navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
                    }
                }
            }
            catch (SupremacyException e)
            {
                //SendKeys.SendWait("^e"); // Error.txt  
                //Thread.Sleep(1000);
                SendKeys.SendWait("^l"); // Log.txt
                Thread.Sleep(1000);

                _ = MessageBox.Show("Step_0098: An error occurred while starting a new game - please retry or change Settings like Galaxy Size.");
                GameLog.Server.General.Error("An error occurred while starting a new game.", e);
                //_errorService.HandleError(e);

                _errorService.HandleError(e);
            }
            catch (Exception e)
            {
                //SendKeys.SendWait("^e"); // Error.txt  
                //Thread.Sleep(1000);
                SendKeys.SendWait("^l"); // Log.txt
                Thread.Sleep(1000);
                _ = MessageBox.Show("Step_0099: An error occurred while starting a new game  - please retry or change Settings like Galaxy Size.");
                GameLog.Server.General.Error("An error occurred while starting a new game.", e);

            }
            
            //_navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
        }

        internal void DropPlayer()
        {
            ServerPlayerInfo player = CurrentPlayerInfo;
            if (player == null)
            {
                return;
            }

            DropPlayer(player);
        }

        internal void DropPlayer(Player player)
        {
            if (_playerInfo.TryGetValue(player, out ServerPlayerInfo playerInfo))
            {
                return;
            }

            DropPlayer(playerInfo);
        }

        internal void DropPlayer(ServerPlayerInfo playerInfo)
        {
            Player player = playerInfo.Player;

            lock (_playerInfo.SyncRoot)
            {
                if (!_playerInfo.Remove(player))
                {
                    return;
                }
            }

            try
            {
                ClearChannelClosingHandling(playerInfo.Session.Channel);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.Server.General.Error(e);
            }

            if (playerInfo.Session.Channel.State == CommunicationState.Opened)
            {
                _ = ((Action)playerInfo.Callback.NotifyDisconnected)
                    .ToAsync(_scheduler)()
                    .Subscribe(
                        _ => { },
                        e => { });
            }


            _ = ((Action)playerInfo.Session.Channel.Close)
                .ToAsync(_scheduler)()
                .Subscribe(
                    _ => { },
                    e => { });

            OnPlayerExited(player);

            if (_isGameEnding)
            {
                return;
            }

            if (_playerInfo.Count > 0)
            {
                if (!TryProcessTurn() && (_combatEngine != null) && _combatEngine.Ready)
                {
                    _ = ((Action<CombatEngine>)TryResumeCombat).ToAsync()(_combatEngine);
                }
            }
            else
            {
                new Task(EndGame).Start();
            }
        }

        internal void DropPlayerAsync(Player player)
        {
            _ = ((Action<Player>)DropPlayer).ToAsync(_scheduler)(player);
        }

        internal void DropPlayerAsync(ServerPlayerInfo player)
        {
            _ = ((Action<ServerPlayerInfo>)DropPlayer).ToAsync(_scheduler)(player);
        }

        internal void EndGame()
        {
            if (!_isGameStarted)
            {
                return;
            }

            _isGameStarted = false;
            _isGameEnding = true;

            try
            {
                lock (_playerInfo.SyncRoot)
                {
                    while (_playerInfo.Count > 0)
                    {
                        DropPlayer(_playerInfo[0]);
                    }
                }
            }
            finally
            {
                _isGameEnding = false;
                _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
                _game = null;
            }

            StopHeartbeat();
        }

        internal void EnsureChannelClosingHandling()
        {
            OperationContext.Current.Channel.Closing += OnChannelClosing;
            OperationContext.Current.Channel.Faulted += OnChannelClosing;
        }

        internal void ClearChannelClosingHandling(IContextChannel contextChannel)
        {
            if (contextChannel == null)
            {
                return;
            }

            contextChannel.Closing -= OnChannelClosing;
            contextChannel.Faulted -= OnChannelClosing;
        }

        internal void EnsurePlayer()
        {
            if (CurrentPlayerInfo == null)
            {
                throw new SupremacyException(
                    "You are not a valid player.",
                    SupremacyExceptionAction.Disconnect);
            }
        }

        internal Player EstablishPlayer(string playerName)
        {
            Player player;

            lock (_playerInfo.SyncRoot)
            {
                player = new Player { PlayerID = _playerInfo.Count };

                player.Name = !string.IsNullOrWhiteSpace(playerName) ? playerName : "Player " + player.PlayerID;

                OperationContext session = OperationContext.Current;

                ServerPlayerInfo playerInfo = new ServerPlayerInfo(
                    player,
                    session.GetCallbackChannel<ISupremacyCallback>(),
                    session,
                    _scheduler);

                EnsureChannelClosingHandling();

                _playerInfo.Add(playerInfo);
                LobbyData.Players = _playerInfo.Select(o => o.Player).ToArray();
            }

            OnPlayerJoined(player);

            return player;
        }

        internal Player GetPlayerByEmpire(Civilization empire)
        {
            if (empire == null)
            {
                return null;
            }

            return GetPlayerByEmpireId(empire.CivID);
        }

        internal Player GetPlayerByEmpireId(int empireId)
        {
            lock (_playerInfo.SyncRoot)
            {
                return _playerInfo.Select(o => o.Player).FirstOrDefault(o => o.EmpireID == empireId);
            }
        }

        internal Player GetPlayerById(int playerId)
        {
            ServerPlayerInfo playerInfo = _playerInfo.FromPlayerId(playerId);
            if (playerInfo != null)
            {
                return playerInfo.Player;
            }

            return null;
        }

        internal ISupremacyCallback GetPlayerCallback(Player player)
        {
            if (_playerInfo.TryGetValue(player, out ServerPlayerInfo playerInfo))
            {
                return playerInfo.Callback;
            }

            return null;
        }

        internal void OnTurnPhaseChanged(TurnPhase phase)
        {
            SendTurnPhaseChangedNotifications(phase);
            _ = new AutoResetEvent(false).WaitOne(100, true);
        }

        internal async void ProcessTurn()
        {
            try
            {
                await SendAllTurnEndedNotificationsAsync().ConfigureAwait(false);
                _ = new AutoResetEvent(false).WaitOne(100, true);

                Player gameHost = GetPlayerById(0);
                List<Order> orders; // fleet orders, not combat orders
                List<Civilization> autoTurnCivs;

                lock (_playerOrders)
                {
                    orders = _playerOrders.Values.SelectMany(v => v.Orders).ToList();
                    autoTurnCivs = _playerOrders.Where(po => po.Value.AutoTurn).Select(po => _game.Civilizations[po.Key.EmpireID]).ToList();
                    _playerOrders.Clear();
                }

                foreach (Order order in orders)
                {
                    //GameLog.Core.Test.DebugFormat("", order.);
                    order.Execute(_game);
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                OnTurnPhaseChanged(TurnPhase.WaitOnAIPlayers);

                lock (_aiAsyncLock)
                {
                    Action<GameContext, List<Civilization>> doAiPlayers = _gameEngine.DoAIPlayers;
                    _aiAsyncResult = doAiPlayers.BeginInvoke(
                        _game, autoTurnCivs,
                        delegate (IAsyncResult result)
                        {
                            lock (_aiAsyncLock)
                            {
                                _ = Interlocked.Exchange(ref _aiAsyncResult, null);
                            }
                            try
                            {
                                doAiPlayers.EndInvoke(result);
                            }
                            catch (Exception e) //ToDo: Just log or additional handling necessary?
                            {
                                GameLog.Server.General.Error(e);
                            }
                        },
                        null);

                }

                GameLog.Server.GeneralDetails.InfoFormat("AI processing time: {0}", stopwatch.Elapsed);

                stopwatch.Restart();
            OH:
                try
                {
                    await DoTurnCore().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    GameLog.Core.GeneralDetails.DebugFormat("Hit await, ************** issue #398 *******************");
                    Thread.Sleep(0050);
                    goto OH;
                }


                GameLog.Server.GeneralDetails.InfoFormat("Turn processing time: {0}", stopwatch.Elapsed);

                Task autoSaveTask = null;

                if (gameHost != null)
                {
                    autoSaveTask = Task.Run(
                        () =>
                        {
                            Stopwatch autoSaveStopwatch = Stopwatch.StartNew();

                            GameContext.PushThreadContext(_game);

                            try { _ = SavedGameManager.AutoSave(gameHost, LobbyData); }
                            finally { _ = GameContext.PopThreadContext(); }
                        });
                }

                stopwatch.Restart();

                OnTurnPhaseChanged(TurnPhase.SendUpdates);

                Task[] batchOperation;

                lock (_playerInfo.SyncRoot)
                {
                    batchOperation = _playerInfo.Select(SendEndOfTurnUpdateAsync).ToArray();
                }

                await Task.WhenAll(batchOperation).ConfigureAwait(false);

                if (autoSaveTask != null)
                {
                    await autoSaveTask.ConfigureAwait(false);
                }

                await SendTurnFinishedNotificationsAsync().ConfigureAwait(false);
            }
            finally
            {
                _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
            }

            // Just in case the AI task completed before resetting _isProcessingTurn to 0.
            _ = TryProcessTurn();
        }

        private async Task DoTurnCore()
        {
            TaskCompletionSource<Unit> tcs = new TaskCompletionSource<Unit>();

            _gameEngine.TurnPhaseChanged += OnGameEngineTurnPhaseChanged;

            GameContext gameContext = _game;

            _ = Observable
                .ToAsync(() => _gameEngine.DoTurn(gameContext), _threadPoolScheduler)()
                .Subscribe(tcs.SetResult, tcs.SetException);

            _ = await tcs.Task;

            _gameEngine.TurnPhaseChanged -= OnGameEngineTurnPhaseChanged;
        }

        private async Task SendEndOfTurnUpdateAsync(ServerPlayerInfo playerInfo)
        {
            ISupremacyCallback callback = playerInfo.Callback;
            if (callback == null)
            {
                return;
            }

            Player player = playerInfo.Player;
            GameUpdateMessage message = new GameUpdateMessage(GameUpdateData.Create(_game, player));
            TaskCompletionSource<Unit> tcs = new TaskCompletionSource<Unit>();

            _text = "Step_0575: doing SendEndOfTurnUpdateAsync for " + player.Empire.Key;
            Console.WriteLine(_text);
            GameLog.Core.GameDataDetails.DebugFormat(_text);

            //GameLog.Server.GameDataDetails.DebugFormat("doing SendEndOfTurnUpdateAsync for {0}", player.Empire.Key);

            IDisposable subscription = Observable
                .ToAsync(() => callback.NotifyGameDataUpdated(message), _scheduler)()
                .Subscribe(tcs.SetResult, tcs.SetException);

            try
            {
                _ = await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                GameLog.Server.General.Error(
                    string.Format(
                        "An error occurred while publishing an end of turn update to player {0}",
                        player.Name),
                    e);

                DropPlayerAsync(player);
            }

            subscription.Dispose();
        }

        private async Task SendTurnFinishedNotificationsAsync()
        {
            List<Task> subTasks = new List<Task>();

            lock (_playerInfo.SyncRoot)
            {
                for (int i = 0; i < _playerInfo.Count; i++)
                {
                    ServerPlayerInfo playerInfo = _playerInfo[i];

                    ISupremacyCallback callback = playerInfo.Callback;
                    if (callback == null)
                    {
                        continue;
                    }

                    TaskCompletionSource<Unit> tcs = new TaskCompletionSource<Unit>();

                    _ = Observable
                        .ToAsync(callback.NotifyTurnFinished, _scheduler)()
                        .Subscribe(
                            tcs.SetResult,
                            e =>
                            {
                                tcs.SetException(e);
                                DropPlayerAsync(playerInfo.Player);
                            });

                    subTasks.Add(tcs.Task);
                }
            }

            await Task.WhenAll(subTasks).ConfigureAwait(false);
        }

        private async Task SendAllTurnEndedNotificationsAsync()
        {
            List<Task> subTasks = new List<Task>();

            lock (_playerInfo.SyncRoot)
            {
                foreach (ServerPlayerInfo playerInfo in _playerInfo)
                {
                    ISupremacyCallback callback = playerInfo.Callback;
                    if (callback == null)
                    {
                        continue;
                    }

                    TaskCompletionSource<Unit> tcs = new TaskCompletionSource<Unit>();

                    ServerPlayerInfo info = playerInfo;
                    _ = Observable
                        .ToAsync(callback.NotifyAllTurnEnded, _scheduler)()
                        .Subscribe(
                            tcs.SetResult,
                            e =>
                            {
                                tcs.SetException(e);
                                DropPlayerAsync(info.Player);
                            });

                    subTasks.Add(tcs.Task);
                }
            }

            await Task.WhenAll(subTasks).ConfigureAwait(false);
        }

        internal void SendLobbyUpdate()
        {
            lock (_playerInfo.SyncRoot)
            {
                foreach (ServerPlayerInfo playerInfo in _playerInfo)
                {
                    Player player = playerInfo.Player;
                    LobbyData lobbyData = LobbyData;

                    _ = ((Action<LobbyData>)playerInfo.Callback.NotifyLobbyUpdated)
                        .ToAsync(playerInfo.Scheduler)(lobbyData)
                        .Subscribe(
                            _ => { },
                            e => DropPlayerAsync(player));
                }
            }
        }

        internal bool TryProcessTurn()
        {
            lock (_playerInfo.SyncRoot)
            {
                if (Interlocked.CompareExchange(ref _isProcessingTurn, 1, 0) != 0)
                {
                    return false;
                }

                if (_playerInfo.Count == 0)
                {
                    _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
                    return false;
                }

                lock (_aiAsyncLock)
                {
                    if (_aiAsyncResult != null)
                    {
                        _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
                        return false;
                    }
                }

                try
                {
                    bool anyOutstandingOrders = _playerInfo
                        .Select(playerInfo => playerInfo.Player)
                        .Any(player => !_playerOrders.ContainsKey(player) || _playerOrders[player] == null);

                    if (anyOutstandingOrders)
                    {
                        _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
                        return false;
                    }
                }
                catch
                {
                    _ = Interlocked.Exchange(ref _isProcessingTurn, 0);
                    return false;
                }

            }

            _ = _threadPoolScheduler.Schedule(ProcessTurn);

            return true;
        }

        internal void Terminate()
        {
            if (_isGameStarted)
            {
                _isGameEnding = true;
                EndGame();
                return;
            }

            while (true)
            {
                lock (_playerInfo.SyncRoot)
                {
                    if (_playerInfo.Count == 0)
                    {
                        break;
                    }

                    DropPlayer(_playerInfo[0].Player);
                }
            }
        }

        private void OnGameEngineTurnPhaseChanged(TurnPhase phase)
        {
            OnTurnPhaseChanged(phase);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void OnAITaskCompleted()
#pragma warning restore IDE0051 // Remove unused private members
        {
            lock (_aiAsyncLock)  // is this used anyway ??? ...reported by VS: it is not used
            {
                _aiAsyncResult = null;
                _ = Observable.ToAsync(
                    delegate
                    {
                        OnTurnPhaseChanged(TurnPhase.WaitOnPlayers);
                        _ = TryProcessTurn();
                    },
                    _scheduler)();
            }
        }

        private void OnChannelClosing(object sender, EventArgs e)
        {
            if (!(sender is IContextChannel channel))
            {
                return;
            }

            ClearChannelClosingHandling(channel);

            ServerPlayerInfo playerInfo = _playerInfo.FromSessionId(channel.InputSession.Id);
            if (playerInfo == null)
            {
                return;
            }

            DropPlayer(playerInfo.Player);
        }

        private void OnPlayerExited(Player player)
        {
            lock (_playerInfo.SyncRoot)
            {
                InvasionArena currentInvasion = _invasionEngine?.InvasionArena;
                if (currentInvasion != null &&
                    currentInvasion.InvaderID == player.EmpireID)
                {
                    // TODO: Deal with players who drop in the middle of combat or invasions.
                }
                foreach (ServerPlayerInfo otherPlayerInfo in _playerInfo)
                {
                    Player otherPlayer = otherPlayerInfo.Player;
                    if (otherPlayer == player)
                    {
                        continue;
                    }

                    ISupremacyCallback callback = otherPlayerInfo.Callback;
                    if (callback == null)
                    {
                        continue;
                    }
                    _ = ((Action)
                   (() =>
                   {
                       callback.NotifyPlayerExited(player);
                       callback.NotifyLobbyUpdated(LobbyData);
                   }))
                      .ToAsync(_scheduler)()
                      .Subscribe(
                          _ => { },
                          e => DropPlayerAsync(otherPlayer));
                }
            }
        }

        private void OnPlayerJoined([NotNull] Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException("player");
            }

            ISupremacyCallback callback = GetPlayerCallback(player);
            LobbyData lobbyData = LobbyData;

            if (callback != null)
            {
                try
                {
                    callback.NotifyOnJoin(player, lobbyData);
                }
                catch
                {
                    DropPlayerAsync(player);
                    return;
                }
            }

            lock (_playerInfo.SyncRoot)
            {
                foreach (ServerPlayerInfo otherPlayer in _playerInfo.Where(o => o.Player != player))
                {
                    ServerPlayerInfo otherPlayerCopy = otherPlayer;
                    if (otherPlayerCopy.Callback == null)
                    {
                        continue;
                    }
                    _ = ((Action)
                   (() =>
                   {
                       otherPlayerCopy.Callback.NotifyPlayerJoined(player);
                       otherPlayerCopy.Callback.NotifyLobbyUpdated(lobbyData);
                   }))
                      .ToAsync(_scheduler)()
                      .Subscribe(
                          _ => { },
                          e => DropPlayerAsync(otherPlayerCopy.Player));
                }
            }
        }

        private void SendChatMessageCallback(int senderId, string message, int recipientId)
        {
            ServerPlayerInfo sender = _playerInfo.FromPlayerId(senderId);
            if (sender == null)
            {
                return;
            }

            ServerPlayerInfo[] recipients;

            if (recipientId < 0)
            {
                lock (_playerInfo.SyncRoot)
                {
                    recipients = new ServerPlayerInfo[_playerInfo.Count];
                    _playerInfo.CopyTo(recipients, 0);
                }
            }
            else
            {
                ServerPlayerInfo recipient = _playerInfo.FromPlayerId(recipientId);
                recipients = recipient != null ? (new[] { recipient, sender }) : (new[] { sender });
            }

            foreach (ServerPlayerInfo recipient in recipients)
            {
                ServerPlayerInfo recipientCopy = recipient;

                _ = ((Action<int, string, int>)recipient.Callback.NotifyChatMessageReceived)
                    .ToAsync(recipient.Scheduler)(senderId, message, recipientId)
                    .Subscribe(
                        _ => { },
                        e => DropPlayerAsync(recipientCopy));
            }
        }

        private void SendTurnPhaseChangedNotifications(TurnPhase phase)
        {
            lock (_playerInfo.SyncRoot)
            {
                foreach (ServerPlayerInfo playerInfo in _playerInfo)
                {
                    ISupremacyCallback callback = playerInfo.Callback;
                    if (callback == null)
                    {
                        continue;
                    }

                    ServerPlayerInfo playerInfoCopy = playerInfo;

                    _ = ((Action<TurnPhase>)playerInfoCopy.Callback.NotifyTurnProgressChanged)
                        .ToAsync(playerInfo.Scheduler)(phase)
                        .Subscribe(
                            _ => { },
                            e => DropPlayerAsync(playerInfoCopy.Player));
                }
            }
        }

        ~SupremacyService()
        {
            PlayerContext.Current = null;
        }
        #endregion

        #region ISupremacyService Members
        public void AssignPlayerSlot(int slotId, int playerId)
        {
            EnsurePlayer();

            lock (_playerInfo.SyncRoot)
            {
                PlayerSlot slot = LobbyData.Slots[slotId];
                if (slot.IsFrozen)
                {
                    return;
                }

                if ((!slot.IsVacant ||
                     playerId != CurrentPlayer.PlayerID) &&
                    !CurrentPlayer.IsGameHost)
                {
                    return;
                }

                if (playerId >= _playerInfo.Count)
                {
                    return;
                }

                if (playerId == Player.UnassignedPlayerID)
                {
                    ClearPlayerSlot(slotId);
                    return;
                }

                if (playerId == Player.ComputerPlayerID)
                {
                    if (!CurrentPlayer.IsGameHost)
                    {
                        return;
                    }

                    slot.Player = Player.Computer;
                    slot.Status = SlotStatus.Computer;
                    slot.Claim = SlotClaim.Assigned;
                }
                else
                {
                    Player assignedPlayer = GetPlayerById(playerId);

                    assignedPlayer.EmpireID = slot.EmpireID;

                    slot.Player = assignedPlayer;
                    slot.Status = SlotStatus.Taken;
                    slot.Claim = SlotClaim.Assigned;

                    PlayerSlot oldSlot = LobbyData.Slots
                        .Where(o => o != slot)
.FirstOrDefault(otherSlot => otherSlot.Player == assignedPlayer);

                    if (oldSlot != null)
                    {
                        ClearPlayerSlot(oldSlot.SlotID);
                    }
                }
            }

            _ = Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public int GetNewObjectID()
        {
            return _game.GenerateID();
        }

        public void SaveGameDeleteManualSaved()
        {
            _ = SavedGameManager.SaveGameDeleteManualSaved();
        }

        public void SaveGame(string fileName)
        {
            EnsurePlayer();

            ServerPlayerInfo currentPlayerInfo = CurrentPlayerInfo;
            if (currentPlayerInfo == null || !currentPlayerInfo.Player.IsGameHost)
            {
                return;
            }

            try
            {
                _ = SavedGameManager.SaveGame(
                    fileName,
                    _game,
                    CurrentPlayer,
                    LobbyData);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.Server.General.Error(e);
            }
        }

        public void StartGame()
        {
            if (_isGameStarted)
            {
                return;
            }

            lock (_playerInfo.SyncRoot)
            {
                LobbyData.Players = _playerInfo.ToPlayerArray();

                bool playerAssignmentsChanged = false;
                List<Player> unassignedPlayers = LobbyData.Players.Except(LobbyData.Slots.Select(o => o.Player)).ToList();
                if (unassignedPlayers.Count != 0)
                {
                    List<PlayerSlot> vacantSlots = LobbyData.Slots.Where(o => o.IsVacant).ToList();

                    foreach (Player unassignedPlayer in unassignedPlayers)
                    {
                        if (vacantSlots.Count == 0)
                        {
                            DropPlayer(unassignedPlayer);
                        }

                        vacantSlots[0].Player = unassignedPlayer;
                        vacantSlots[0].Status = SlotStatus.Taken;
                        vacantSlots[0].Claim = SlotClaim.Assigned;
                        unassignedPlayer.EmpireID = vacantSlots[0].EmpireID;
                        vacantSlots.RemoveAt(0);
                        playerAssignmentsChanged = true;
                    }
                }

                foreach (PlayerSlot slot in LobbyData.Slots.Where(
                    slot => slot.Status == SlotStatus.Open &&
                            slot.Claim == SlotClaim.Unassigned))
                {
                    slot.Status = SlotStatus.Computer;
                    slot.Claim = SlotClaim.Assigned;
                }

                LobbyData.GameOptions.Freeze();

                if (playerAssignmentsChanged)
                {
                    SendLobbyUpdate();
                }
            }

            new Task(DoStartGame).Start();
        }

        public HostGameResult HostGame([NotNull] GameInitData initData, out Player localPlayer, out LobbyData lobbyData)
        {
            _gameInitData = initData ?? throw new ArgumentNullException("initData");

            localPlayer = null;
            lobbyData = null;

            GameMod mod = null;

            if ((initData.GameType == GameType.SinglePlayerLoad) || (initData.GameType == GameType.MultiplayerLoad))
            {
                SavedGameHeader header = SavedGameManager.LoadSavedGameHeader(initData.SaveGameFileName);
                Console.WriteLine("Step_0286: loading SavedGameHeader from " + initData.SaveGameFileName);
                if (header == null)
                {
                    return HostGameResult.LoadGameFailure;
                }

                initData.Options.Freeze();
            }
            else
            {
                Guid modId = initData.Options.ModID;
                if (!Equals(modId, Guid.Empty))
                {
                    mod = GameModLoader.FindMods().Where(o => o.UniqueIdentifier == modId).FirstOrDefault();
                }
            }

            LobbyData.IsMultiplayerGame = initData.IsMultiplayerGame;
            LobbyData.GameOptions = initData.Options;
            LobbyData.Empires = initData.EmpireNames;

            LobbyData.GameMod = mod;

            localPlayer = EstablishPlayer(initData.LocalPlayerName);

            int empireCount = initData.EmpireIDs.Length;

            LobbyData.Players = new[] { localPlayer };
            LobbyData.Slots = new PlayerSlot[empireCount];

            lobbyData = LobbyData;

            for (int i = 0; i < empireCount; i++)
            {
                PlayerSlot slot = new PlayerSlot
                {

                    SlotID = i,
                    EmpireID = initData.EmpireIDs[i],
                    EmpireName = initData.EmpireNames[i],
                    Status = initData.SlotStatus[i],
                    Claim = initData.SlotClaims[i]

                };

                LobbyData.Slots[i] = slot;

                if (slot.IsFrozen)
                {
                    continue;
                }

                if (slot.Status == SlotStatus.Closed)
                {
                    slot.Close();
                }
            }

            if (initData.GameType != GameType.MultiplayerNew)
            {
                PlayerSlot playerSlot = LobbyData.Slots.FirstOrDefault(o => o.EmpireID == initData.LocalPlayerEmpireID) ??
                                 LobbyData.Slots[0];
                AssignPlayerSlot(playerSlot.SlotID, localPlayer.PlayerID);
            }

            if (!initData.IsMultiplayerGame)
            {
                _ = ((Action)StartGame)
                    .ToAsync(_scheduler)()
                    .Subscribe(
                    _ => { },
                    e => GameLog.Server.General.Error("Error occurred while starting game.", e));
            }

            return HostGameResult.Success;

        }

        public JoinGameResult JoinGame(string playerName, out Player localPlayer, out LobbyData lobbyData)
        {
            lock (_playerInfo.SyncRoot)
            {
                if (_isGameStarted)
                {
                    localPlayer = null;
                    lobbyData = null;
                    return JoinGameResult.GameAlreadyStarted;
                }

                if (_playerInfo.Count >= LobbyData.Slots.Length)
                {
                    localPlayer = null;
                    lobbyData = null;
                    return JoinGameResult.GameIsFull;
                }

                localPlayer = EstablishPlayer(playerName);
                lobbyData = LobbyData;
                return JoinGameResult.Success;
            }
        }

        public void Disconnect()
        {
            try
            {
                ClearChannelClosingHandling(OperationContext.Current.Channel);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.Server.General.Error(e);
            }

            DropPlayer();
        }

        public void SendChatMessage(string message, int recipientId)
        {
            EnsurePlayer();

            _ = ((Action<int, string, int>)SendChatMessageCallback).ToAsync(_scheduler)(
                CurrentPlayer.PlayerID,
                message,
                recipientId);
        }

        public void EndTurn(PlayerOrdersMessage orders)
        {
            EnsurePlayer();

            Player currentPlayer = CurrentPlayer;

            _playerOrders[currentPlayer] = orders;


            lock (_playerInfo.SyncRoot)
            {
                foreach (ServerPlayerInfo playerInfo in _playerInfo)
                {
                    ISupremacyCallback callback = playerInfo.Callback;
                    if (callback == null)
                    {
                        continue;
                    }

                    ServerPlayerInfo playerInfoCopy = playerInfo;

                    _ = ((Action<int>)callback.NotifyPlayerFinishedTurn)
                        .ToAsync(_scheduler)(currentPlayer.EmpireID)
                        .Subscribe(
                            _ => { },
                            e => DropPlayer(playerInfoCopy.Player));
                }
            }

            _ = TryProcessTurn();
        }

        public void UpdateGameOptions(GameOptions options)
        {
            if (_isGameStarted)
            {
                return;
            }

            lock (_playerInfo.SyncRoot)
            {
                LobbyData.GameOptions = options;
            }

            _ = Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void UpdateEmpireSelection(int playerId, int empireId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                ServerPlayerInfo currentPlayerInfo = CurrentPlayerInfo;

                if (!currentPlayerInfo.Player.IsGameHost &&
                    playerId != currentPlayerInfo.Player.PlayerID)
                {
                    return;
                }

                if (playerId >= _playerInfo.Count)
                {
                    return;
                }

                currentPlayerInfo.Player.EmpireID = empireId;

                foreach (ServerPlayerInfo playerInfo in _playerInfo)
                {
                    if (playerInfo != currentPlayerInfo &&
                        playerInfo.Player.EmpireID == empireId)
                    {
                        playerInfo.Player.EmpireID = Player.InvalidEmpireID;
                    }
                }
            }

            _ = Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void ClearPlayerSlot(int slotId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                PlayerSlot slot = LobbyData.Slots[slotId];
                if (slot.IsFrozen)
                {
                    return;
                }

                Player currentPlayer = CurrentPlayer;
                if (slot.IsClosed)
                {
                    if (!currentPlayer.IsGameHost)
                    {
                        return;
                    }
                }
                else if (!slot.IsVacant &&
                         !currentPlayer.IsGameHost &&
                         slot.Player != currentPlayer)
                {
                    return;
                }

                slot.Clear();
            }

            _ = Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void ClosePlayerSlot(int slotId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                PlayerSlot slot = LobbyData.Slots[slotId];
                if (slot.IsFrozen || slot.IsClosed)
                {
                    return;
                }

                Player currentPlayer = CurrentPlayer;
                if (!currentPlayer.IsGameHost)
                {
                    return;
                }

                slot.Close();
            }

            _ = Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        #region Combat
        private void NotifyCombatEndedCallback(CombatEngine engine)
        {
            //Console.WriteLine("Step_3004: " + _combatEngine._assets[0].Sector.Location + "> OnCombatOccurring ... populating _combatEngine ");
            if (_combatEngine != null)
            {
                _combatEngine = null;
                _gameEngine.NotifyCombatFinished();
            }
        }


        private void OnCombatOccurring(List<CombatAssets> assets)
        {
            Console.WriteLine("Step_3004: " + assets[0].Sector.Location + " > OnCombatOccurring ... populating _combatEngine ");
            _combatEngine = new AutomatedCombatEngine(
                assets,
                SendCombatUpdateCallback,
                NotifyCombatEndedCallback);
            _combatEngine.SendInitialUpdate();
        }

        public void SendCombatOrders(CombatOrders orders)
        {
            try
            {
                if (_combatEngine == null || orders == null)
                {
                    return;
                }

                // just a test .... but "Resolving Combat" never finished
                //if (orders.CombatID == -1)
                //    orders.CombatID = GameContext.Current.GenerateID();

                lock (_combatEngine.SyncLock)
                {
                    try
                    {
                        if (orders.CombatID != -1 && _combatEngine != null)
                        {
                            _combatEngine.SubmitOrders(orders);
                        }
                    }
                    catch { GameLog.Client.CombatDetails.DebugFormat("Problem with null in SubmitOrders(orders)"); }

                    if (_combatEngine != null && _combatEngine.Ready)
                    {
                        TryResumeCombat(_combatEngine);
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Server.Combat.DebugFormat("null reference old closed issue #164 {0} appears not to crash code", orders.ToString());
                GameLog.Server.Combat.Error(e);
            }
        }

        public void SendCombatTarget1(CombatTargetPrimaries target1)
        {
            try
            {
                if (_combatEngine == null || target1 == null)
                {
                    return;
                }

                lock (_combatEngine.SyncLockTargetOnes)
                {
                    _combatEngine.SubmitTargetOnes(target1);

                }
            }
            catch (Exception e)
            {
                GameLog.Server.Combat.DebugFormat("SendCombatTargetOnes null reference issue #164 {0}", target1.ToString());
                GameLog.Server.Combat.Error(e);
            }
        }

        public void SendCombatTarget2(CombatTargetSecondaries target2)
        {
            try
            {
                if (_combatEngine == null || target2 == null)
                {
                    return;
                }

                lock (_combatEngine.SyncLockTargetTwos)
                {
                    _combatEngine.SubmitTargetTwos(target2);

                }
            }
            catch (Exception e)
            {
                GameLog.Server.Combat.DebugFormat("SendCombatTargetTwos null reference issue #164 {0}", target2.ToString());
                GameLog.Server.Combat.Error(e);
            }
        }
        //public void SendIntelOrders(IntelOrders intelOrders)
        //{
        //    GameLog.Server.Intel.DebugFormat("NEXT: trying to do SendIntelOrders ...");
        //    try
        //    {
        //        if (_intelEngine == null || intelOrders == null)
        //            return;

        //        GameLog.Server.Intel.DebugFormat("trying to do SendIntelOrders ...");
        //        //lock (_combatEngine.SyncLockTargetTwos)
        //        //{
        //        _intelEngine.SubmitIntelOrders(intelOrders);
        //        GameLog.Server.Intel.DebugFormat("done Submit for  SendIntelOrders ...");
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        GameLog.Server.Intel.DebugFormat("SendIntelOrders null {0}", intelOrders.ToString());
        //        GameLog.Server.Intel.Error(e);
        //    }
        //}
        private void SendCombatUpdateCallback(CombatEngine engine, CombatUpdate update)
        {
            _text = "Step_3009: SendCombatUpdateCallbac...";
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);

            GameContext.PushThreadContext(_game);

            ServerPlayerInfo player = _playerInfo.FromEmpireId(update.OwnerID);
            if (player != null)
            {
                ISupremacyCallback callback = player.Callback;
                callback?.NotifyCombatUpdate(update);
            }
            //No proper CombatAI, so just for now fake some orders
            else if (!engine.IsCombatOver && !update.Owner.IsHuman)
            {
                // works   GameLog.Server.Combat.DebugFormat("Generating fake order for {0}", update.Owner.Name);
                CombatAssets ownerAssets = update.FriendlyAssets.FirstOrDefault(friendlyAssets => friendlyAssets.Owner == update.Owner);
                CombatAssets enemyAssets = update.HostileAssets.FirstOrDefault(hostileAssets => hostileAssets.Owner != update.Owner);

                if (ownerAssets == null)
                {
                    return;
                }

                Civilization _target = new Civilization
                {
                    ShortName = "Only Return Fire",
                    CivID = 888,
                    Key = "Only Return Fire"
                }; // The AI generates a dummy target for non-human player civ

                CombatOrder blanketOrder = CombatOrder.Engage;
                Civilization blanketTargetOne = _target;
                Civilization blanketTargetTwo = _target;

                //int countStation = 0;
                //if (enemyAssets != null)
                //{
                //    if (enemyAssets.Station != null)
                //    {
                //        countStation = 2;  // counting value for Station = 2 ships
                //        GameLog.Core.Combat.DebugFormat("generated blanketOrder = {3} for {0} (Count friendly = {1} vs {2})",
                //        ownerAssets.Owner, enemyAssets.CombatShips.Count + countStation, ownerAssets.CombatShips.Count + 1, blanketOrder);
                //    }
                //}

                SendCombatTarget1(CombatHelper.GenerateBlanketTargetPrimary(ownerAssets, blanketTargetOne));
                SendCombatTarget2(CombatHelper.GenerateBlanketTargetSecondary(ownerAssets, blanketTargetTwo));
                SendCombatOrders(CombatHelper.GenerateBlanketOrders(ownerAssets, blanketOrder)); // Sending of the order

            }


            _ = GameContext.PopThreadContext();
        }

        private void TryResumeCombat(CombatEngine engine)
        {
            if (engine != null)
            {
                GameContext.PushThreadContext(_game);
                try
                {
                    lock (engine.SyncLock)
                    {
                        if (engine.Ready)
                        {
                            engine.ResolveCombatRound();
                        }
                    }

                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.Server.Combat.Error(e);
                }

                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }
        }
        #endregion

        #region Invasion
        private void NotifyInvasionEndedCallback(InvasionEngine engine)
        {
            Player player = GetPlayerByEmpire(engine.InvasionArena.Invader);
            if (player == null)
            {
                if (_invasionEngine == engine)
                {
                    _invasionEngine = null;
                    _gameEngine.NotifyCombatFinished();
                }
            }
            else if (player.IsHumanPlayer) //this is bool based on return (_playerId >= GameHostID) GameHostID is always 0    
            {
                return;
            }
            else if (_invasionEngine == engine)
            {
                _invasionEngine = null;
                _gameEngine.NotifyCombatFinished();
            }
        }

        public void NotifyInvasionScreenReady()
        {
            InvasionEngine invasionEngine = _invasionEngine;
            if (invasionEngine == null)
            {
                return;
            }

            GameContext game = _game;
            if (game != null)
            {
                GameContext.PushThreadContext(game);
            }

            try
            {
                Player player = GetPlayerByEmpire(invasionEngine.InvasionArena.Invader);

                if (Equals(player, CurrentPlayer))
                {
                    _invasionEngine = null;
                    _gameEngine.NotifyCombatFinished();
                }
            }
            finally
            {
                if (game != null)
                {
                    _ = GameContext.Pop();
                }
            }
        }

        private void OnInvasionOccurring(InvasionArena invasionArena)
        {
            Console.WriteLine("Step_3004: OnInvasionOccurring ... populating _invasionEngine");

            if (invasionArena.LatelyDoneInTurn > GameContext.Current.TurnNumber)
            {
                return;
            }
            else
            {
                invasionArena.LatelyDoneInTurn = GameContext.Current.TurnNumber + 1;
                _invasionEngine = new InvasionEngine(SendInvasionUpdateCallback, NotifyInvasionEndedCallback);
                _scheduler.Schedule(() => _invasionEngine.BeginInvasion(invasionArena));
            }

            //_invasionEngine = new InvasionEngine(SendInvasionUpdateCallback, NotifyInvasionEndedCallback);
            //_scheduler.Schedule(() => _invasionEngine.BeginInvasion(invasionArena));

            //_combatEngine = new AutomatedCombatEngine(
            //    assets,
            //    SendCombatUpdateCallback,
            //    NotifyCombatEndedCallback);
            //_combatEngine.SendInitialUpdate();
        }

        //private void OnInvasionOccurring(InvasionArena invasionArena)
        //{
        //    Console.WriteLine("Step_3004: " + invasionArena.Colony.Location + " " + invasionArena.Colony.Name + " > OnInvasionOccurring ... populating _invasionEngine");
        //    bool doneOnceAlready = false;
        //    if (!invasionArena.Invader.IsHuman && doneOnceAlready == false)
        //    {

        //        if (_alreadyDidCivAsAI == null || _alreadyDidCivAsAI != invasionArena.Invader)
        //        {
        //            _alreadyDidCivAsAI = invasionArena.Invader;
        //            GameLog.Client.SystemAssaultDetails.DebugFormat("_alreadyDidCivAsAI = {0}", invasionArena.Invader.Key);
        //            if (_invasionEngine == null)
        //            {
        //                _invasionEngine = new InvasionEngine(SendInvasionUpdateCallback, NotifyInvasionEndedCallback);
        //            }

        //            _ = _scheduler.Schedule(() => _invasionEngine.BeginInvasion(invasionArena));
        //            doneOnceAlready = true;
        //        }
        //    }
        //    else if (doneOnceAlready == false)
        //    {
        //        if (_invasionEngine == null)
        //        {
        //            _invasionEngine = new InvasionEngine(SendInvasionUpdateCallback, NotifyInvasionEndedCallback);
        //        }

        //        if (_invasionEngine != null)
        //        {
        //            try
        //            {
        //                _ = _scheduler.Schedule(() => _invasionEngine.BeginInvasion(invasionArena));
        //            }
        //            catch (Exception)
        //            {
        //                _text =
        //                    "SystemAssault doesn't work - "
        //                    + invasionArena.Colony.Name
        //                    + invasionArena.Colony.Location
        //                    ;
        //                Console.WriteLine(_text);
        //                //throw;
        //            }

        //        }

        //        doneOnceAlready = true;
        //    }
        //}

        public void SendInvasionOrders(InvasionOrders orders)
        {
            if (_invasionEngine == null || orders == null)
            {
                return;
            }

            _ = _threadPoolScheduler.Schedule(() => _invasionEngine.SubmitOrders(orders));
        }

        private void SendInvasionUpdateCallback(InvasionEngine engine, InvasionArena update)
        {
            ServerPlayerInfo player = _playerInfo.FromEmpireId(update.InvaderID);
            if (player != null)
            {
                ISupremacyCallback callback = player.Callback;
                callback?.NotifyInvasionUpdate(update);
            }
            else if (update.Status == InvasionStatus.InProgress)
            {
                List<InvasionOrbital> transports =
                    (
                        from o in update.InvadingUnits.OfType<InvasionOrbital>()
                        where !o.IsDestroyed
                        let ship = o.Source as Ship
                        where ship != null && ship.ShipType == ShipType.Transport
                        select o
                    ).ToList();

                if (transports.Count == 0)
                {
                    SendInvasionOrders(new InvasionOrders(update.InvasionID, InvasionAction.StandDown, InvasionTargetingStrategy.Balanced));
                }
                else
                {
                    SendInvasionOrders(new InvasionOrders(update.InvasionID, InvasionAction.LandTroops, InvasionTargetingStrategy.Balanced));
                }
            }
        }
        #endregion

        #region Ping
        private void PingClients()
        {
            ServerPlayerInfo[] players = _playerInfo.ToArray();

            foreach (ServerPlayerInfo player in players)
            {
                ServerPlayerInfo playerCopy = player;

                _ = ((Action<ServerPlayerInfo>)PingPlayer)
                    .ToAsync(player.Scheduler)(player)
                    .Subscribe(
                    _ => { },
                    e => DropPlayer(playerCopy));
            }
        }

        private void PingPlayer([NotNull] ServerPlayerInfo player)
        {
            if (player == null)
            {
                throw new ArgumentNullException("player");
            }

            try { player.Callback.Ping(); }
            catch { DropPlayer(player); }
        }

        public void Pong(int pingId) { }

        internal void StartHeartbeat()
        {
            _heartbeat = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15))
                .Select(_ => ((Action)PingClients).ToAsync(_threadPoolScheduler)())
                .Subscribe();
        }

        internal void StopHeartbeat()
        {
            IDisposable heartbeat = Interlocked.Exchange(ref _heartbeat, null);
            heartbeat?.Dispose();
        }
        #endregion
        #endregion
    }
}
