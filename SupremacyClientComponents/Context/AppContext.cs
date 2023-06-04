// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;

using Supremacy.Client.Events;
using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.Client.Audio;
using Supremacy.Utility;

namespace Supremacy.Client.Context
{
    public interface IAppContext : IClientContext
    {
        MusicLibrary DefaultMusicLibrary { get; }
        MusicLibrary ThemeMusicLibrary { get; }

        //int  ASpecialWidth1 { get; }
        //int ASpecialHeight1 { get; }
    }

    public class AppContext : IAppContext, IDisposable
    {
        #region Fields
        private const string DefaultMusicLibraryPath = "Resources/Specific_Empires_UI/Default/MusicPacks.xml";

        //private readonly int ASpecialWidth1 = 576;
        //private readonly int ASpecialHeight1 = 480;

        private readonly ReaderWriterLockSlim _accessLock;
        private readonly Dispatcher _dispatcher;
        private readonly KeyedCollectionBase<int, IPlayer> _allPlayers;

        private IGameContext _currentGame;
        private bool _isConnected;
        private bool _isDisposed;
        private bool _isSinglePlayerGame;
        private readonly bool _isBorgPlayable = false;
        private readonly bool _isTerranEmpirePlayable = false;
        private readonly bool _isFederationPlayable = false;
        private readonly bool _isRomulanPlayable = false;
        private readonly bool _isKlingonPlayable = false;
        private readonly bool _isCardassianPlayable = false;
        private readonly bool _isDominionPlayable = false;
        public bool _audioTrace;
        private bool _isGameHost;
        private bool _isGameEnding;
        private bool _isGameInPlay;
        private bool _isTurnFinished;
        private IPlayer _localPlayer;
        private IEnumerable<IPlayer> _remotePlayers;
        private ILobbyData _lobbyData;
//#pragma warning disable IDE0044 // Add readonly modifier
        private MusicLibrary _themeMusicLibrary = new MusicLibrary();
//#pragma warning restore IDE0044 // Add readonly modifier

        #endregion

        #region Properties
        public MusicLibrary DefaultMusicLibrary { get; } = new MusicLibrary();

        public MusicLibrary ThemeMusicLibrary => _themeMusicLibrary;

        //public int ASpecialWidth1
        //{
        //    get { return 576; }
        //}

        //public int ASpecialHeight1
        //{
        //    get { return 480; }
        //}
        #endregion

        #region Constructors and Finalizers
        public AppContext()
        {
            _accessLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _dispatcher = Dispatcher.CurrentDispatcher;
            _allPlayers = new KeyedCollectionBase<int, IPlayer>(o => o.PlayerID);
            DefaultMusicLibrary.Load(DefaultMusicLibraryPath);
            _audioTrace = false;    // just tracing audio into Log.txt
            HookEventHandlers();
        }
        #endregion

        #region IAppContext Implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public and Protected Methods
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        #region Public and Protected Methods
        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
            {
                return;
            }

            try
            {
                UnhookEventHandlers();
            }
            finally
            {
                _isDisposed = true;
            }
        }
        #endregion

        #region Private Methods
        private void HookEventHandlers()
        {
            _ = ClientEvents.ClientConnected.Subscribe(OnClientConnected);
            _ = ClientEvents.ClientDisconnected.Subscribe(OnClientConnectionClosed);
            _ = ClientEvents.ClientConnectionFailed.Subscribe(OnClientConnectionClosed, ThreadOption.UIThread);
            _ = ClientEvents.LocalPlayerJoined.Subscribe(OnLocalPlayerJoined, ThreadOption.PublisherThread);
            _ = ClientEvents.LobbyUpdated.Subscribe(OnLobbyUpdated);
            _ = ClientEvents.GameStarted.Subscribe(OnGameStarted, ThreadOption.PublisherThread);
            _ = ClientEvents.GameUpdateDataReceived.Subscribe(OnGameUpdateDataReceived, ThreadOption.UIThread);
            _ = ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
            _ = ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded);
            _ = ClientEvents.GameEnding.Subscribe(OnGameEnding);
            _ = ClientEvents.GameEnded.Subscribe(OnGameEnded, ThreadOption.UIThread);
        }

        private void OnGameUpdateDataReceived(ClientDataEventArgs<GameUpdateData> args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                args.Value.UpdateLocalGame((GameContext)_currentGame);
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("LocalPlayerEmpire");
        }

        private void OnGameEnding(ClientEventArgs obj)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _isGameEnding = true;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("IsGameEnding");
        }

        private void OnLocalPlayerJoined(LocalPlayerJoinedEventArgs args)
        {
            IPlayer localPlayer = args.Player;

            if (localPlayer == null)
            {
                return;
            }

            _accessLock.EnterWriteLock();
            try
            {
                _lobbyData = args.LobbyData;
                _localPlayer = localPlayer;
                _isSinglePlayerGame = !_lobbyData.IsMultiplayerGame;
                _isGameHost = localPlayer.IsGameHost;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }

            OnPropertyChanged("LocalPlayer");
            OnPropertyChanged("IsGameHost");
            OnPropertyChanged("IsSinglePlayerGame");
            OnPropertyChanged("LobbyData");
        }

        private void UnhookEventHandlers()
        {
            ClientEvents.ClientConnected.Unsubscribe(OnClientConnected);
            ClientEvents.ClientDisconnected.Unsubscribe(OnClientConnectionClosed);
            ClientEvents.ClientConnectionFailed.Unsubscribe(OnClientConnectionClosed);
            ClientEvents.LocalPlayerJoined.Unsubscribe(OnLocalPlayerJoined);
            ClientEvents.LobbyUpdated.Unsubscribe(OnLobbyUpdated);
            ClientEvents.GameStarted.Unsubscribe(OnGameStarted);
            ClientEvents.GameUpdateDataReceived.Unsubscribe(OnGameUpdateDataReceived);
            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
            ClientEvents.AllTurnEnded.Unsubscribe(OnAllTurnEnded);
            ClientEvents.GameEnded.Unsubscribe(OnGameEnded);
        }
        #endregion

        #endregion

        #region Private Methods
        private void ClearGameData()
        {
            _accessLock.EnterWriteLock();
            try
            {
                if (_currentGame != null)
                {
                    _ = _dispatcher.Invoke(
                        (Func<GameContext, bool>)GameContext.CheckAndPop,
                        DispatcherPriority.Send,
                        _currentGame);
                }

                _isGameInPlay = false;
                _isGameHost = false;
                _isGameEnding = false;
                _localPlayer = null;
                _isSinglePlayerGame = false;
                _allPlayers.Clear();
                _remotePlayers = Enumerable.Empty<IPlayer>();
                _currentGame = null;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }

            OnPropertyChanged("IsGameInPlay");
            OnPropertyChanged("IsGameHost");
            OnPropertyChanged("IsGameEnding");
            OnPropertyChanged("CurrentGame");
            OnPropertyChanged("LocalPlayer");
            OnPropertyChanged("LocalPlayerEmpire");
            OnPropertyChanged("RemotePlayers");
        }

        private void OnClientConnected(ClientConnectedEventArgs args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _isConnected = true;
                _isGameHost = args.IsServerLocal;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("IsGameHost");
        }

        private void OnClientConnectionClosed(EventArgs args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _isConnected = false;
                _isGameHost = false;
                ClearGameData();
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("IsGameHost");
        }

        private void OnGameEnded(EventArgs args)
        {
            ClearGameData();
            OnPropertyChanged("IsGameInPlay");
        }

        private void OnGameStarted(ClientDataEventArgs<GameStartData> args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _currentGame = args.Value.CreateLocalGame();
                _ = _dispatcher.Invoke(
                    (Action<GameContext>)GameContext.PushThreadContext,
                    DispatcherPriority.Send,
                    _currentGame);
                _isGameInPlay = true;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("CurrentGame");
            OnPropertyChanged("IsGameInPlay");
            OnPropertyChanged("LocalPlayerEmpire");
        }

        private void OnLobbyUpdated(DataEventArgs<ILobbyData> args)
        {
            ILobbyData lobbyData = args.Value;
            if (lobbyData != null)
            {
                _accessLock.EnterUpgradeableReadLock();
                try
                {
                    _lobbyData = lobbyData;
                    _isSinglePlayerGame = !lobbyData.IsMultiplayerGame;
                    if (_localPlayer != null)
                    {
                        _accessLock.EnterWriteLock();
                        try
                        {
                            _localPlayer = lobbyData.Players.Where(o => o.PlayerID == _localPlayer.PlayerID).FirstOrDefault();
                            _remotePlayers = lobbyData.Players.Cast<IPlayer>().Where(o => o != _localPlayer);
                            _allPlayers.Clear();
                            _allPlayers.AddRange(lobbyData.Players);
                        }
                        finally
                        {
                            _accessLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _accessLock.ExitUpgradeableReadLock();
                }
            }
            OnPropertyChanged("LocalPlayer");
            OnPropertyChanged("LocalPlayerEmpire");
            OnPropertyChanged("RemotePlayers");
            OnPropertyChanged("IsSinglePlayerGame");
        }

        private void OnAllTurnEnded(EventArgs args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _isTurnFinished = true;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("IsTurnFinished");
        }

        private void OnTurnStarted(EventArgs args)
        {
            _accessLock.EnterWriteLock();
            try
            {
                _isTurnFinished = false;
            }
            finally
            {
                _accessLock.ExitWriteLock();
            }
            OnPropertyChanged("IsTurnFinished");
            OnPropertyChanged("LocalPlayerEmpire");
        }
        #endregion

        #region Implementation of IAppContext
        public bool IsConnected
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isConnected;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsGameHost
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isGameHost;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsGameInPlay
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isGameInPlay;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsGameEnding
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isGameEnding;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsSinglePlayerGame
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isSinglePlayerGame;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }


        public bool IsFederationPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isFederationPlayable={0}", _isFederationPlayable);
                    return _isFederationPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsRomulanPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isRomulanPlayable={0}", _isRomulanPlayable);
                    return _isRomulanPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsKlingonPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isKlingonPlayable={0}", _isKlingonPlayable);
                    return _isKlingonPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsCardassianPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isCardassianPlayable={0}", _isCardassianPlayable);
                    return _isCardassianPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsDominionPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isDominionPlayable={0}", _isDominionPlayable);
                    return _isDominionPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsBorgPlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isBorgPlayable={0}", _isBorgPlayable);
                    return _isBorgPlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsTerranEmpirePlayable
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    GameLog.Client.GameData.DebugFormat("AppContext.cs: isTerranEmpirePlayable={0}", _isTerranEmpirePlayable);
                    return _isTerranEmpirePlayable;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public IPlayer LocalPlayer
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _localPlayer;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public ILobbyData LobbyData
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _lobbyData;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public IKeyedCollection<int, IPlayer> Players
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _allPlayers;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public IEnumerable<IPlayer> RemotePlayers
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _remotePlayers;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public CivilizationManager LocalPlayerEmpire
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    if ((_localPlayer == null) || (_currentGame == null))
                    {
                        return null;
                    }

                    return _currentGame.CivilizationManagers[_localPlayer.EmpireID];
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public IGameContext CurrentGame
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _currentGame;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }

        public bool IsTurnFinished
        {
            get
            {
                _accessLock.EnterReadLock();
                try
                {
                    return _isTurnFinished;
                }
                finally
                {
                    _accessLock.ExitReadLock();
                }
            }
        }
        #endregion
    }
}