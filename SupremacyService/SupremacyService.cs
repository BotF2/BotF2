// SupremacyService.cs
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
using Supremacy.Client.Input;
using Supremacy.Client.Services;
using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Messaging;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Scripting;
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

        private static readonly Lazy<GameLog> _log = new Lazy<GameLog>(() => GameLog.Server);

        private readonly object _aiAsyncLock;
        private readonly LobbyData _lobbyData;
        private readonly Dictionary<Player, PlayerOrdersMessage> _playerOrders;
        private readonly ServerPlayerInfoCollection _playerInfo;
        private readonly IGameErrorService _errorService;
        private readonly IScheduler _scheduler;
        private readonly IScheduler _threadPoolScheduler;
        private readonly DelegateCommand<string> _consoleCommand;
        private GameInitData _gameInitData;
        private IAsyncResult _aiAsyncResult;
        private CombatEngine _combatEngine;
        private InvasionEngine _invasionEngine;
        private GameContext _game;
        private bool _isGameStarted;
        private bool _isGameEnding;
        private int _isProcessingTurn;
        private GameEngine _gameEngine;
        private IDisposable _heartbeat;
        #endregion

        #region Constructors
        public SupremacyService()
        {
            _aiAsyncLock = new object();
            _errorService = ServiceLocator.Current.GetInstance<IGameErrorService>();
            _playerInfo = new ServerPlayerInfoCollection();
            _playerOrders = new Dictionary<Player, PlayerOrdersMessage>();
            _lobbyData = new LobbyData
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

            _consoleCommand = new DelegateCommand<string>(ExecuteConsoleCommand);
            _threadPoolScheduler = Scheduler.ThreadPool.AsGameScheduler(() => _game);
        }

        private void ExecuteConsoleCommand(string s)
        {
            try
            {
                var script = new ScriptExpression(false)
                {
                    Parameters = new ScriptParameters(new ScriptParameter("$game", typeof(GameContext)), new ScriptParameter("$gc", typeof(Action))),
                    ScriptCode = s
                };

                if (!script.CompileScript())
                    return;

                GameContext.PushThreadContext(_game);

                try
                {
                    var result = script.Evaluate(
                        new RuntimeScriptParameters
                        {
                            new RuntimeScriptParameter(script.Parameters[0], _game),
                            new RuntimeScriptParameter(
                                script.Parameters[1],
                                (Action)(() =>
                                         {
                                             var memoryBeforeCollection = GC.GetTotalMemory(false);

                                             GCHelper.Collect();

                                             Channel.Publish(
                                                 new ConsoleEvent(
                                                     string.Format(
                                                         "Garbage collected [{0:#,#,} kb -> {1:#,#,} kb]",
                                                         memoryBeforeCollection,
                                                         GC.GetTotalMemory(false))));
                                         }))
                        });

                    Channel.Publish(new ConsoleEvent(result));
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            }
            catch (Exception e)
            {
                GameLog.Server.General.Error(e);
            }
        }

        #endregion

        #region Properties
        internal static GameLog Log
        {
            get { return _log.Value; }
        }

        internal static ISupremacyCallback Callback
        {
            get { return OperationContext.Current.GetCallbackChannel<ISupremacyCallback>(); }
        }

        internal Player CurrentPlayer
        {
            get
            {
                var playerInfo = _playerInfo.FromSessionId(OperationContext.Current.SessionId);
                if (playerInfo != null)
                    return playerInfo.Player;
                return null;
            }
        }

        internal ServerPlayerInfo CurrentPlayerInfo
        {
            get
            {
                var operationContext = OperationContext.Current;
                if (operationContext == null)
                    return null;

                return _playerInfo.FromSessionId(operationContext.SessionId);
            }
        }

        internal ServiceHost Host { get; set; }

        internal LobbyData LobbyData
        {
            get { return _lobbyData; }
        }
        #endregion

        #region Methods
        internal void DoStartGame()
        {
            if (_isGameStarted)
                return;

            _isGameStarted = true;

            try
            {
                ClientCommands.ConsoleCommand.RegisterCommand(_consoleCommand);

                if (_playerInfo.Count > 1)
                    SendLobbyUpdate();

                lock (_playerInfo.SyncRoot)
                {
                    foreach (var playerInfo in _playerInfo)
                    {
                        var player = playerInfo.Player;

                        ((Action)playerInfo.Callback.NotifyGameStarting)
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
                        if (!SavedGameManager.LoadGame(_gameInitData.SaveGameFileName, out SavedGameHeader header, out _game, out DateTime timestamp))
                        {
                            EndGame();
                            return;
                        }
                    }
                    else
                    {
                        _game = GameContext.Create(_lobbyData.GameOptions, _lobbyData.IsMultiplayerGame);
                        GameContext.PushThreadContext(_game);
                        try
                        {
                            _gameEngine.DoPreGameSetup(_game);
                        }
                        finally
                        {
                            GameContext.PopThreadContext();
                        }
                    }
                }

                var textDatabase = ClientTextDatabase.Load(
                    ResourceManager.GetResourcePath("Resources\\Data\\TextDatabase.xml"));

                lock (_playerInfo.SyncRoot)
                {
                    foreach (var playerInfo in Enumerable.ToArray(_playerInfo))
                    {
                        try
                        {
                            if (playerInfo.Callback == null)
                                continue;

                            var player = playerInfo.Player;
                            var message = new GameStartMessage(GameStartData.Create(_game, player, textDatabase));

                            ((Action<GameStartMessage>)playerInfo.Callback.NotifyGameStarted)
                                .ToAsync(_scheduler)(message)
                                .Subscribe(
                                    _ => { },
                                    e => DropPlayerAsync(player));
                        }
                        catch
                        {
                            if (playerInfo.Player.IsGameHost)
                                throw;
                            DropPlayerAsync(playerInfo.Player);
                        }
                    }
                }
            }
            catch (SupremacyException e)
            {
                Log.General.Error("An error occurred while starting a new game.", e);
                _errorService.HandleError(e);
            }
            catch (Exception e)
            {
                Log.General.Error("An error occurred while starting a new game.", e);
            }
        }

        internal void DropPlayer()
        {
            var player = CurrentPlayerInfo;
            if (player == null)
                return;
            DropPlayer(player);
        }

        internal void DropPlayer(Player player)
        {
            ServerPlayerInfo playerInfo;

            if (_playerInfo.TryGetValue(player, out playerInfo))
                return;

            DropPlayer(playerInfo);
        }

        internal void DropPlayer(ServerPlayerInfo playerInfo)
        {
            var player = playerInfo.Player;

            lock (_playerInfo.SyncRoot)
            {
                if (!_playerInfo.Remove(player))
                    return;
            }

            try
            {
                ClearChannelClosingHandling(playerInfo.Session.Channel);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            if (playerInfo.Session.Channel.State == CommunicationState.Opened)
            {
                ((Action)playerInfo.Callback.NotifyDisconnected)
                    .ToAsync(_scheduler)()
                    .Subscribe(
                        _ => { },
                        e => { });
            }


            ((Action)playerInfo.Session.Channel.Close)
                .ToAsync(_scheduler)()
                .Subscribe(
                    _ => { },
                    e => { });

            OnPlayerExited(player);

            if (_isGameEnding)
                return;

            if (_playerInfo.Count > 0)
            {
                if (!TryProcessTurn() && (_combatEngine != null) && _combatEngine.Ready)
                    ((Action<CombatEngine>)TryResumeCombat).ToAsync()(_combatEngine);
            }
            else
            {
                new Task(EndGame).Start();
            }
        }

        internal void DropPlayerAsync(Player player)
        {
            ((Action<Player>)DropPlayer).ToAsync(_scheduler)(player);
        }

        internal void DropPlayerAsync(ServerPlayerInfo player)
        {
            ((Action<ServerPlayerInfo>)DropPlayer).ToAsync(_scheduler)(player);
        }

        internal void EndGame()
        {
            if (!_isGameStarted)
                return;

            _isGameStarted = false;
            _isGameEnding = true;

            try
            {
                ClientCommands.ConsoleCommand.UnregisterCommand(_consoleCommand);

                lock (_playerInfo.SyncRoot)
                {
                    while (_playerInfo.Count > 0)
                        DropPlayer(_playerInfo[0]);
                }
            }
            finally
            {
                _isGameEnding = false;
                Interlocked.Exchange(ref _isProcessingTurn, 0);
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
                return;
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

                if (!string.IsNullOrWhiteSpace(playerName))
                    player.Name = playerName;
                else
                    player.Name = "Player " + player.PlayerID;

                var session = OperationContext.Current;

                var playerInfo = new ServerPlayerInfo(
                    player,
                    session.GetCallbackChannel<ISupremacyCallback>(),
                    session,
                    _scheduler);

                EnsureChannelClosingHandling();

                _playerInfo.Add(playerInfo);
                _lobbyData.Players = _playerInfo.Select(o => o.Player).ToArray();
            }

            OnPlayerJoined(player);

            return player;
        }

        internal Player GetPlayerByEmpire(Civilization empire)
        {
            if (empire == null)
                return null;

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
            var playerInfo = _playerInfo.FromPlayerId(playerId);
            if (playerInfo != null)
                return playerInfo.Player;
            return null;
        }

        internal ISupremacyCallback GetPlayerCallback(Player player)
        {
            ServerPlayerInfo playerInfo;

            if (_playerInfo.TryGetValue(player, out playerInfo))
                return playerInfo.Callback;

            return null;
        }

        internal void OnTurnPhaseChanged(TurnPhase phase)
        {
            SendTurnPhaseChangedNotifications(phase);
            new AutoResetEvent(false).WaitOne(100, true);
        }

        internal async void ProcessTurn()
        {
            try
            {
                await SendAllTurnEndedNotificationsAsync().ConfigureAwait(false);
                new AutoResetEvent(false).WaitOne(100, true);

                Player gameHost = GetPlayerById(0);
                List<Order> orders;
                List<Civilization> autoTurnCivs;

                lock (_playerOrders)
                {
                    orders = _playerOrders.Values.SelectMany(v => v.Orders).ToList();
                    autoTurnCivs = _playerOrders.Where(po => po.Value.AutoTurn).Select(po => _game.Civilizations[po.Key.EmpireID]).ToList();
                    _playerOrders.Clear();
                }

                foreach (var order in orders)
                {
                    order.Execute(_game);
                }

                var stopwatch = Stopwatch.StartNew();

                OnTurnPhaseChanged(TurnPhase.WaitOnAIPlayers);

                lock (_aiAsyncLock)
                {
                    var doAiPlayers = (Action<GameContext, List<Civilization>>)_gameEngine.DoAIPlayers;
                    _aiAsyncResult = doAiPlayers.BeginInvoke(
                        _game, autoTurnCivs,
                        delegate (IAsyncResult result)
                        {
                            lock (_aiAsyncLock)
                            {
                                Interlocked.Exchange(ref _aiAsyncResult, null);
                            }
                            try
                            {
                                doAiPlayers.EndInvoke(result);
                            }
                            catch (Exception e) //ToDo: Just log or additional handling necessary?
                            {
                                GameLog.LogException(e);
                            }
                        },
                        null);
                }

                GameLog.Server.General.DebugFormat("AI processing time: {0}", stopwatch.Elapsed);

                stopwatch.Restart();

                await DoTurnCore().ConfigureAwait(false);

                GameLog.Server.General.DebugFormat("Turn processing time: {0}", stopwatch.Elapsed);

                Task autoSaveTask = null;

                if (gameHost != null)
                {
                    autoSaveTask = Task.Run(
                        () =>
                        {
                            var autoSaveStopwatch = Stopwatch.StartNew();

                            GameContext.PushThreadContext(_game);

                            try { SavedGameManager.AutoSave(gameHost, _lobbyData); }
                            finally { GameContext.PopThreadContext(); }
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
                    await autoSaveTask.ConfigureAwait(false);

                await SendTurnFinishedNotificationsAsync().ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessingTurn, 0);
            }

            // Just in case the AI task completed before resetting _isProcessingTurn to 0.
            TryProcessTurn();
        }

        private async Task DoTurnCore()
        {
            var tcs = new TaskCompletionSource<Unit>();

            _gameEngine.TurnPhaseChanged += OnGameEngineTurnPhaseChanged;

            var gameContext = _game;

            Observable
                .ToAsync(() => _gameEngine.DoTurn(gameContext), _threadPoolScheduler)()
                .Subscribe(tcs.SetResult, tcs.SetException);

            await tcs.Task;

            _gameEngine.TurnPhaseChanged -= OnGameEngineTurnPhaseChanged;
        }

        private async Task SendEndOfTurnUpdateAsync(ServerPlayerInfo playerInfo)
        {
            var callback = playerInfo.Callback;
            if (callback == null)
                return;

            var player = playerInfo.Player;
            var message = new GameUpdateMessage(GameUpdateData.Create(_game, player));
            var tcs = new TaskCompletionSource<Unit>();

            var subscription = Observable
                .ToAsync(() => callback.NotifyGameDataUpdated(message), _scheduler)()
                .Subscribe(tcs.SetResult, tcs.SetException);

            try
            {
                await tcs.Task.ConfigureAwait(false);
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
            var subTasks = new List<Task>();

            lock (_playerInfo.SyncRoot)
            {
                for (var i = 0; i < _playerInfo.Count; i++)
                {
                    var playerInfo = _playerInfo[i];

                    var callback = playerInfo.Callback;
                    if (callback == null)
                        continue;

                    var tcs = new TaskCompletionSource<Unit>();

                    Observable
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
            var subTasks = new List<Task>();

            lock (_playerInfo.SyncRoot)
            {
                foreach (var playerInfo in _playerInfo)
                {
                    var callback = playerInfo.Callback;
                    if (callback == null)
                        continue;

                    var tcs = new TaskCompletionSource<Unit>();

                    ServerPlayerInfo info = playerInfo;
                    Observable
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
                foreach (var playerInfo in _playerInfo)
                {
                    var player = playerInfo.Player;
                    var lobbyData = LobbyData;

                    ((Action<LobbyData>)playerInfo.Callback.NotifyLobbyUpdated)
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
                    return false;

                if (_playerInfo.Count == 0)
                {
                    Interlocked.Exchange(ref _isProcessingTurn, 0);
                    return false;
                }

                lock (_aiAsyncLock)
                {
                    if (_aiAsyncResult != null)
                    {
                        Interlocked.Exchange(ref _isProcessingTurn, 0);
                        return false;
                    }
                }

                try
                {
                    var anyOutstandingOrders = _playerInfo
                        .Select(playerInfo => playerInfo.Player)
                        .Any(player => !_playerOrders.ContainsKey(player) || _playerOrders[player] == null);

                    if (anyOutstandingOrders)
                    {
                        Interlocked.Exchange(ref _isProcessingTurn, 0);
                        return false;
                    }
                }
                catch
                {
                    Interlocked.Exchange(ref _isProcessingTurn, 0);
                    return false;
                }
            }

            _threadPoolScheduler.Schedule(ProcessTurn);

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
                        break;

                    DropPlayer(_playerInfo[0].Player);
                }
            }
        }

        private void OnGameEngineTurnPhaseChanged(TurnPhase phase)
        {
            OnTurnPhaseChanged(phase);
        }

        private void OnAITaskCompleted()
        {
            lock (_aiAsyncLock)
            {
                _aiAsyncResult = null;
                Observable.ToAsync(
                    delegate
                    {
                        OnTurnPhaseChanged(TurnPhase.WaitOnPlayers);
                        TryProcessTurn();
                    },
                    _scheduler)();
            }
        }

        private void OnChannelClosing(object sender, EventArgs e)
        {
            var channel = sender as IContextChannel;
            if (channel == null)
                return;

            ClearChannelClosingHandling(channel);

            var playerInfo = _playerInfo.FromSessionId(channel.InputSession.Id);
            if (playerInfo == null)
                return;

            DropPlayer(playerInfo.Player);
        }

        private void OnPlayerExited(Player player)
        {
            lock (_playerInfo.SyncRoot)
            {
                var currentInvasion = _invasionEngine != null ? _invasionEngine.InvasionArena : null;
                if (currentInvasion != null &&
                    currentInvasion.InvaderID == player.EmpireID)
                {
                    // TODO: Deal with players who drop in the middle of combat or invasions.
                }
                foreach (var otherPlayerInfo in _playerInfo)
                {
                    var otherPlayer = otherPlayerInfo.Player;
                    if (otherPlayer == player)
                        continue;

                    var callback = otherPlayerInfo.Callback;
                    if (callback == null)
                        continue;

                    ((Action)
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
                throw new ArgumentNullException("player");

            var callback = GetPlayerCallback(player);
            var lobbyData = LobbyData;

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
                foreach (var otherPlayer in _playerInfo.Where(o => o.Player != player))
                {
                    var otherPlayerCopy = otherPlayer;
                    if (otherPlayerCopy.Callback == null)
                        continue;

                    ((Action)
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
            var sender = _playerInfo.FromPlayerId(senderId);
            if (sender == null)
                return;

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
                var recipient = _playerInfo.FromPlayerId(recipientId);
                if (recipient != null)
                    recipients = new[] { recipient, sender };
                else
                    recipients = new[] { sender };
            }

            foreach (var recipient in recipients)
            {
                var recipientCopy = recipient;

                ((Action<int, string, int>)recipient.Callback.NotifyChatMessageReceived)
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
                foreach (var playerInfo in _playerInfo)
                {
                    var callback = playerInfo.Callback;
                    if (callback == null)
                        continue;

                    var playerInfoCopy = playerInfo;

                    ((Action<TurnPhase>)playerInfoCopy.Callback.NotifyTurnProgressChanged)
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
                var slot = _lobbyData.Slots[slotId];
                if (slot.IsFrozen)
                    return;

                if ((!slot.IsVacant ||
                     playerId != CurrentPlayer.PlayerID) &&
                    !CurrentPlayer.IsGameHost)
                {
                    return;
                }

                if (playerId >= _playerInfo.Count)
                    return;

                if (playerId == Player.UnassignedPlayerID)
                {
                    ClearPlayerSlot(slotId);
                    return;
                }

                if (playerId == Player.ComputerPlayerID)
                {
                    if (!CurrentPlayer.IsGameHost)
                        return;

                    slot.Player = Player.Computer;
                    slot.Status = SlotStatus.Computer;
                    slot.Claim = SlotClaim.Assigned;
                }
                else
                {
                    var assignedPlayer = GetPlayerById(playerId);

                    assignedPlayer.EmpireID = slot.EmpireID;

                    slot.Player = assignedPlayer;
                    slot.Status = SlotStatus.Taken;
                    slot.Claim = SlotClaim.Assigned;

                    var oldSlot = _lobbyData.Slots
                        .Where(o => o != slot)
                        .Where(otherSlot => otherSlot.Player == assignedPlayer)
                        .FirstOrDefault();

                    if (oldSlot != null)
                        ClearPlayerSlot(oldSlot.SlotID);
                }
            }

            Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public int GetNewObjectID()
        {
            return _game.GenerateID();
        }

        public void SaveGame(string fileName)
        {
            EnsurePlayer();

            var currentPlayerInfo = CurrentPlayerInfo;
            if (currentPlayerInfo == null || !currentPlayerInfo.Player.IsGameHost)
                return;

            try
            {
                SavedGameManager.SaveGame(
                    fileName,
                    _game,
                    CurrentPlayer,
                    _lobbyData);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        public void StartGame()
        {
            if (_isGameStarted)
                return;

            lock (_playerInfo.SyncRoot)
            {
                _lobbyData.Players = _playerInfo.ToPlayerArray();

                var playerAssignmentsChanged = false;
                var unassignedPlayers = _lobbyData.Players.Except(_lobbyData.Slots.Select(o => o.Player)).ToList();
                if (unassignedPlayers.Count != 0)
                {
                    var vacantSlots = _lobbyData.Slots.Where(o => o.IsVacant).ToList();

                    foreach (var unassignedPlayer in unassignedPlayers)
                    {
                        if (vacantSlots.Count == 0)
                            DropPlayer(unassignedPlayer);

                        vacantSlots[0].Player = unassignedPlayer;
                        vacantSlots[0].Status = SlotStatus.Taken;
                        vacantSlots[0].Claim = SlotClaim.Assigned;
                        unassignedPlayer.EmpireID = vacantSlots[0].EmpireID;
                        vacantSlots.RemoveAt(0);
                        playerAssignmentsChanged = true;
                    }
                }

                foreach (var slot in _lobbyData.Slots.Where(
                    slot => slot.Status == SlotStatus.Open &&
                            slot.Claim == SlotClaim.Unassigned))
                {
                    slot.Status = SlotStatus.Computer;
                    slot.Claim = SlotClaim.Assigned;
                }

                _lobbyData.GameOptions.Freeze();

                if (playerAssignmentsChanged)
                    SendLobbyUpdate();
            }

            new Task(DoStartGame).Start();
        }

        public HostGameResult HostGame([NotNull] GameInitData initData, out Player localPlayer, out LobbyData lobbyData)
        {
            if (initData == null)
                throw new ArgumentNullException("initData");

            _gameInitData = initData;

            localPlayer = null;
            lobbyData = null;

            GameMod mod = null;

            if ((initData.GameType == GameType.SinglePlayerLoad) || (initData.GameType == GameType.MultiplayerLoad))
            {
                var header = SavedGameManager.LoadSavedGameHeader(initData.SaveGameFileName);
                if (header == null)
                    return HostGameResult.LoadGameFailure;
                initData.Options.Freeze();
            }
            else
            {
                var modId = initData.Options.ModID;
                if (!Equals(modId, Guid.Empty))
                    mod = GameModLoader.FindMods().Where(o => o.UniqueIdentifier == modId).FirstOrDefault();
            }

            _lobbyData.IsMultiplayerGame = initData.IsMultiplayerGame;
            _lobbyData.GameOptions = initData.Options;
            _lobbyData.Empires = initData.EmpireNames;

            _lobbyData.GameMod = mod;

            localPlayer = EstablishPlayer(initData.LocalPlayerName);

            var empireCount = initData.EmpireIDs.Length;

            _lobbyData.Players = new[] { localPlayer };
            _lobbyData.Slots = new PlayerSlot[empireCount];

            lobbyData = _lobbyData;

            for (int i = 0; i < empireCount; i++)
            {
                var slot = new PlayerSlot
                {

                    SlotID = i,
                    EmpireID = initData.EmpireIDs[i],
                    EmpireName = initData.EmpireNames[i],
                    Status = initData.SlotStatus[i],
                    Claim = initData.SlotClaims[i]

                };

                _lobbyData.Slots[i] = slot;

                if (slot.IsFrozen)
                    continue;

                if (slot.Status == SlotStatus.Closed)
                    slot.Close();
            }

            if (initData.GameType != GameType.MultiplayerNew)
            {
                var playerSlot = _lobbyData.Slots.FirstOrDefault(o => o.EmpireID == initData.LocalPlayerEmpireID) ??
                                 _lobbyData.Slots[0];
                AssignPlayerSlot(playerSlot.SlotID, localPlayer.PlayerID);
            }

            if (!initData.IsMultiplayerGame)
            {
                ((Action)StartGame)
                    .ToAsync(_scheduler)()
                    .Subscribe(
                    _ => { },
                    e => Log.General.Error("Error occurred while starting game.", e));
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

                if (_playerInfo.Count >= _lobbyData.Slots.Length)
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
                GameLog.LogException(e);
            }

            DropPlayer();
        }

        public void SendChatMessage(string message, int recipientId)
        {
            EnsurePlayer();

            ((Action<int, string, int>)SendChatMessageCallback).ToAsync(_scheduler)(
                CurrentPlayer.PlayerID,
                message,
                recipientId);
        }

        public void EndTurn(PlayerOrdersMessage orders)
        {
            EnsurePlayer();

            var currentPlayer = CurrentPlayer;

            _playerOrders[currentPlayer] = orders;

            lock (_playerInfo.SyncRoot)
            {
                foreach (var playerInfo in _playerInfo)
                {
                    var callback = playerInfo.Callback;
                    if (callback == null)
                        continue;

                    var playerInfoCopy = playerInfo;

                    ((Action<GameObjectID>)callback.NotifyPlayerFinishedTurn)
                        .ToAsync(_scheduler)(currentPlayer.EmpireID)
                        .Subscribe(
                            _ => { },
                            e => DropPlayer(playerInfoCopy.Player));
                }
            }

            TryProcessTurn();
        }

        public void UpdateGameOptions(GameOptions options)
        {
            if (_isGameStarted)
                return;

            lock (_playerInfo.SyncRoot)
            {
                _lobbyData.GameOptions = options;
            }

            Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void UpdateEmpireSelection(int playerId, int empireId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                var currentPlayerInfo = CurrentPlayerInfo;

                if (!currentPlayerInfo.Player.IsGameHost &&
                    playerId != currentPlayerInfo.Player.PlayerID)
                {
                    return;
                }

                if (playerId >= _playerInfo.Count)
                    return;

                currentPlayerInfo.Player.EmpireID = empireId;

                foreach (var playerInfo in _playerInfo)
                {
                    if (playerInfo != currentPlayerInfo &&
                        playerInfo.Player.EmpireID == empireId)
                    {
                        playerInfo.Player.EmpireID = Player.InvalidEmpireID;
                    }
                }
            }

            Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void ClearPlayerSlot(int slotId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                var slot = _lobbyData.Slots[slotId];
                if (slot.IsFrozen)
                    return;

                var currentPlayer = CurrentPlayer;
                if (slot.IsClosed)
                {
                    if (!currentPlayer.IsGameHost)
                        return;
                }
                else if (!slot.IsVacant &&
                         !currentPlayer.IsGameHost &&
                         slot.Player != currentPlayer)
                {
                    return;
                }

                slot.Clear();
            }

            Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        public void ClosePlayerSlot(int slotId)
        {
            lock (_playerInfo.SyncRoot)
            {
                EnsurePlayer();

                var slot = _lobbyData.Slots[slotId];
                if (slot.IsFrozen || slot.IsClosed)
                    return;

                var currentPlayer = CurrentPlayer;
                if (!currentPlayer.IsGameHost)
                    return;

                slot.Close();
            }

            Observable.ToAsync(SendLobbyUpdate, _scheduler)();
        }

        #region Combat
        private void NotifyCombatEndedCallback(CombatEngine engine)
        {
            if (_combatEngine != null)
            {
                _combatEngine = null;
                _gameEngine.NotifyCombatFinished();
            }
        }

        private void OnCombatOccurring(List<CombatAssets> assets)
        {
            _combatEngine = new AutomatedCombatEngine(
                assets,
                SendCombatUpdateCallback,
                NotifyCombatEndedCallback);
            _combatEngine.SendInitialUpdate();
        }

        public void SendCombatOrders(CombatOrders orders)
        {
            if (_combatEngine == null || orders == null)
                return;

            lock (_combatEngine.SyncLock)
            {
                _combatEngine.SubmitOrders(orders);

                if (_combatEngine.Ready)
                    TryResumeCombat(_combatEngine);
            }
        }

        private void SendCombatUpdateCallback(CombatEngine engine, CombatUpdate update)
        {
            GameContext.PushThreadContext(_game);

            var player = _playerInfo.FromEmpireId(update.OwnerID);
            if (player != null)
            {
                var callback = player.Callback;
                if (callback != null)
                {
                    callback.NotifyCombatUpdate(update);
                }
            }

            //No proper CombatAI, so just for now fake some orders
            else if (!engine.IsCombatOver && !update.Owner.IsHuman)
            {
                GameLog.Print("Generating fake order for {0}", update.Owner.Name);
                var ownerAssets = update.FriendlyAssets.FirstOrDefault(friendlyAssets => friendlyAssets.Owner == update.Owner);
                if (ownerAssets == null)
                {
                    return;
                }
                if (update.RoundNumber == 1)
                {
                    SendCombatOrders(CombatHelper.GenerateBlanketOrders(ownerAssets, CombatOrder.Hail));
                }
                else
                {
                    SendCombatOrders(CombatHelper.GenerateBlanketOrders(ownerAssets, CombatOrder.Engage));
                }
            }

            GameContext.PopThreadContext();
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
                            engine.ResolveCombatRound();
                    }
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            }
        }
        #endregion

        #region Invasion
        private void NotifyInvasionEndedCallback(InvasionEngine engine)
        {
            var player = GetPlayerByEmpire(engine.InvasionArena.Invader);
            if (player.IsHumanPlayer)
                return;

            if (_invasionEngine == engine)
            {
                _invasionEngine = null;
                _gameEngine.NotifyCombatFinished();
            }
        }

        public void NotifyInvasionScreenReady()
        {
            var invasionEngine = _invasionEngine;
            if (invasionEngine == null)
                return;

            var game = _game;
            if (game != null)
                GameContext.PushThreadContext(game);

            try
            {
                var player = GetPlayerByEmpire(invasionEngine.InvasionArena.Invader);

                if (Equals(player, CurrentPlayer))
                {
                    _invasionEngine = null;
                    _gameEngine.NotifyCombatFinished();
                }
            }
            finally
            {
                if (game != null)
                    GameContext.Pop();
            }
        }

        private void OnInvasionOccurring(InvasionArena invasionArena)
        {
            if (_invasionEngine == null)
                _invasionEngine = new InvasionEngine(SendInvasionUpdateCallback, NotifyInvasionEndedCallback);

            _scheduler.Schedule(() => _invasionEngine.BeginInvasion(invasionArena));
        }

        public void SendInvasionOrders(InvasionOrders orders)
        {
            if (_invasionEngine == null || orders == null)
                return;

            _threadPoolScheduler.Schedule(() => _invasionEngine.SubmitOrders(orders));
        }

        private void SendInvasionUpdateCallback(InvasionEngine engine, InvasionArena update)
        {
            var player = _playerInfo.FromEmpireId(update.InvaderID);
            if (player != null)
            {
                var callback = player.Callback;
                if (callback != null)
                    callback.NotifyInvasionUpdate(update);
            }
            else if (update.Status == InvasionStatus.InProgress)
            {
                var transports =
                    (
                        from o in update.InvadingUnits.OfType<InvasionOrbital>()
                        where !o.IsDestroyed
                        let ship = o.Source as Ship
                        where ship != null && ship.ShipType == ShipType.Transport
                        select o
                    ).ToList();

                if (transports.Count == 0)
                    SendInvasionOrders(new InvasionOrders(update.InvasionID, InvasionAction.StandDown, InvasionTargetingStrategy.Balanced));
                else
                    SendInvasionOrders(new InvasionOrders(update.InvasionID, InvasionAction.LandTroops, InvasionTargetingStrategy.Balanced));
            }
        }
        #endregion

        #region Ping
        private void PingClients()
        {
            var players = _playerInfo.ToArray();

            foreach (var player in players)
            {
                var playerCopy = player;

                ((Action<ServerPlayerInfo>)PingPlayer)
                    .ToAsync(player.Scheduler)(player)
                    .Subscribe(
                    _ => { },
                    e => DropPlayer(playerCopy));
            }
        }

        private void PingPlayer([NotNull] ServerPlayerInfo player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

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
            var heartbeat = Interlocked.Exchange(ref _heartbeat, null);
            if (heartbeat != null)
                heartbeat.Dispose();
        }
        #endregion
        #endregion
    }
}