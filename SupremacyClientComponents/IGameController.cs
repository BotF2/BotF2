// File:IGameController.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Controls;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Client.Services;
using Supremacy.Client.Views;
using Supremacy.Combat;
using Supremacy.Game;

using System.Linq;
using Supremacy.Client.Context;
using Supremacy.Utility;
using System.IO;
using System.Reflection;
using Supremacy.Resources;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

namespace Supremacy.Client
{
    public interface IGameController
    {
        event EventHandler Terminated;

        void RunLocal([NotNull] GameInitData initData);
        void RunRemote([NotNull] string playerName, [NotNull] string remoteHost);
        void Terminate();
    }

    public class GameController : IGameController, IDisposable
    {
        public const string LocalPlayerName = "Player";

        private readonly IUnityContainer _container;
        private readonly INavigationService _navigationService;
        private readonly IGameWindow _gameWindow;
        private readonly SitRepDialog _sitRepDialog;
        //private readonly ShipOverview _shipOverview;
        private readonly IAppContext _appContext;
        private readonly IPlayerOrderService _playerOrderService;
        private readonly Dictionary<EventBase, SubscriptionToken> _eventSubscriptionTokens;
        private readonly List<IPresenter> _screenPresenters;
        private readonly DelegateCommand<object> _endTurnCommand;
        private readonly DelegateCommand<object> _showEndOfTurnSummaryCommand;
        //private readonly DelegateCommand<object> _showShipOverviewCommand;
        private readonly Dispatcher _dispatcher;
        private IDisposable _connectWaitCursorHandle;
        private IDisposable _gameStartWaitCursorHandle;
        private IDisposable _turnWaitCursorHandle;
        private IGameClient _client;
        private IGameServer _server;
        private GameOptions _gameOptions;
        private bool _suppressClientEvents;
        private bool _isServerLocal;
        private bool _lobbyScreenShown;
        private bool _isDisposed;
        private bool _firstTurnStarted;
        private string _text;
        private int _lastOneDone;
        private string _contentHistoryFile = "";
        private readonly string newline = Environment.NewLine;

        public GameController(
            [NotNull] IUnityContainer container,
            [NotNull] INavigationService navigationService,
            [NotNull] IGameWindow gameWindow,
            [NotNull] IRegionManager regionManager,
            [NotNull] IRegionViewRegistry regionViewRegistry,
            [NotNull] IAppContext appContext,
            [NotNull] IGameClient client,
            [NotNull] IPlayerOrderService playerOrderService)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }

            if (regionViewRegistry == null)
            {
                throw new ArgumentNullException("regionViewRegistry");
            }

            _container = container ?? throw new ArgumentNullException("container");
            _navigationService = navigationService ?? throw new ArgumentNullException("navigationService");
            _gameWindow = gameWindow ?? throw new ArgumentNullException("gameWindow");
            _sitRepDialog = container.Resolve<SitRepDialog>();
            //_shipOverview = container.Resolve<ShipOverview>();
            _appContext = appContext ?? throw new ArgumentNullException("appContext");
            _client = client ?? throw new ArgumentNullException("client");
            _playerOrderService = playerOrderService ?? throw new ArgumentNullException("playerOrderService");
            _endTurnCommand = new DelegateCommand<object>(ExecuteTurnCommand) { IsActive = false };
            _showEndOfTurnSummaryCommand = new DelegateCommand<object>(ExecuteShowEndOfTurnSummaryCommand) { IsActive = true };
            //_showShipOverviewCommand = new DelegateCommand<object>(ExecuteShowShipOverviewCommand) { IsActive = true };
            _eventSubscriptionTokens = new Dictionary<EventBase, SubscriptionToken>();
            _screenPresenters = new List<IPresenter>();
            _playerOrderService.ClearOrders();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void ExecuteShowEndOfTurnSummaryCommand(object obj)
        {
            ShowSummary(true);
        }

        //private void ExecuteShowShipOverviewCommand(object obj)
        //{
        //    ShowShipOverview(true);
        //}

        private static void ExecuteTurnCommand(object obj)
        {
            ClientEvents.TurnEnded.Publish(ClientEventArgs.Default);
        }

        #region Implementation of IGameController
        public event EventHandler Terminated;

        public void RunLocal([NotNull] GameInitData initData)
        {
            if (initData == null)
            {
                throw new ArgumentNullException("initData");
            }

            CheckDisposed();

            _isServerLocal = true;

            _dispatcher.Invoke(
                SetConnectWaitCursor,
                DispatcherPriority.Normal);

            HookCommandAndEventHandlers();

            try
            {
                if (initData.Options == null)
                {
                    initData.Options = _container.Resolve<GameOptions>();
                }

                _gameOptions = initData.Options;

                StartServer(initData.IsMultiplayerGame);

                Connect(() => _client.HostAndConnect(initData, "localhost"));
            }
            catch
            {
                UnhookCommandAndEventHandlers();

                _dispatcher.Invoke(
                    ClearWaitCursors,
                    DispatcherPriority.Normal);

                throw;
            }

        }

        private void SetConnectWaitCursor()
        {
            IDisposable handle = _gameWindow.EnterWaitCursorScope();

            if (Interlocked.CompareExchange(ref _connectWaitCursorHandle, handle, null) != null)
            {
                handle.Dispose();
            }
        }

        private void UnhookCommandAndEventHandlers()
        {
            ClientCommands.EndTurn.UnregisterCommand(_endTurnCommand);
            ClientCommands.ShowEndOfTurnSummary.UnregisterCommand(_showEndOfTurnSummaryCommand);
            //ClientCommands.ShowShipOverview.UnregisterCommand(_showShipOverviewCommand);
            ClientEvents.InvasionUpdateReceived.Unsubscribe(OnInvasionUpdateReceived);

            lock (_eventSubscriptionTokens)
            {
                foreach (EventBase subscribedEvent in _eventSubscriptionTokens.Keys)
                {
                    subscribedEvent.Unsubscribe(_eventSubscriptionTokens[subscribedEvent]);
                }

                _eventSubscriptionTokens.Clear();
            }
        }

        private void OnLocalPlayerJoined(LocalPlayerJoinedEventArgs args)
        {
            if (_lobbyScreenShown)
            {
                return;
            }

            if (_eventSubscriptionTokens.TryGetValue(ClientEvents.LocalPlayerJoined, out SubscriptionToken subscriptionToken))
            {
                ClientEvents.LocalPlayerJoined.Unsubscribe(subscriptionToken);
            }

            if (!_appContext.IsSinglePlayerGame)
            {
                _ = _navigationService.ActivateScreen(StandardGameScreens.MultiplayerLobby);
            }

            _lobbyScreenShown = true;

            ClearConnectWaitCursor();
        }

        private void OnTerminated()
        {
            Terminated?.Invoke(this, EventArgs.Empty);
        }

        private void HookCommandAndEventHandlers()
        {
            ClientCommands.EndTurn.RegisterCommand(_endTurnCommand);
            ClientCommands.ShowEndOfTurnSummary.RegisterCommand(_showEndOfTurnSummaryCommand);
            //ClientCommands.ShowShipOverview.RegisterCommand(_showShipOverviewCommand);
            _ = ClientEvents.InvasionUpdateReceived.Subscribe(OnInvasionUpdateReceived, ThreadOption.UIThread);

            lock (_eventSubscriptionTokens)
            {
                SubscriptionToken subscriptionToken = ClientEvents.LocalPlayerJoined.Subscribe(
                    OnLocalPlayerJoined,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.LocalPlayerJoined] = subscriptionToken;

                subscriptionToken = ClientEvents.GameStarting.Subscribe(
                    OnGameStarting,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.GameStarting] = subscriptionToken;

                subscriptionToken = ClientEvents.GameStarted.Subscribe(
                    OnGameStarted,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.GameStarted] = subscriptionToken;

                subscriptionToken = ClientEvents.GameEnded.Subscribe(
                    OnGameEnded,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.GameEnded] = subscriptionToken;

                subscriptionToken = ClientEvents.TurnStarted.Subscribe(
                    OnTurnStarted,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.TurnStarted] = subscriptionToken;

                subscriptionToken = ClientEvents.TurnEnded.Subscribe(
                    OnTurnEnded,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.TurnEnded] = subscriptionToken;

                subscriptionToken = ClientEvents.AllTurnEnded.Subscribe(
                    OnAllTurnEnded,
                    ThreadOption.UIThread);

                _eventSubscriptionTokens[ClientEvents.AllTurnEnded] = subscriptionToken;
            }
        }

        private void OnInvasionUpdateReceived(ClientDataEventArgs<InvasionArena> e)
        {
            ViewModelPresenter<SystemAssaultScreenViewModel, ISystemAssaultScreenView> presenter = _container.Resolve<ViewModelPresenter<SystemAssaultScreenViewModel, ISystemAssaultScreenView>>();
            if (presenter.Model.IsRunning)
            {
                return;
            }

            presenter.Model.ProcessUpdate(e.Value);
            presenter.Run();
        }

        private void OnGameEnded(EventArgs t)
        {
            Terminate();
        }

        private void OnTurnEnded(EventArgs t)
        {
            _endTurnCommand.IsActive = false;
        }

        private void OnAllTurnEnded(EventArgs t)
        {
            SetTurnWaitCursor();
        }

        private void OnGameStarting(EventArgs t)
        {
            SetGameStartWaitCursor();
        }

        private void SetGameStartWaitCursor()
        {
            IDisposable handle = _gameWindow.EnterWaitCursorScope();
            if (Interlocked.CompareExchange(ref _gameStartWaitCursorHandle, handle, null) != null)
            {
                handle.Dispose();
            }
        }

        private void SetTurnWaitCursor()
        {
            IDisposable handle = _gameWindow.EnterWaitCursorScope();
            if (Interlocked.CompareExchange(ref _turnWaitCursorHandle, handle, null) != null)
            {
                handle.Dispose();
            }
        }

        private void OnTurnStarted(EventArgs args)
        {
            IGameContext currentGame = _appContext.CurrentGame;
            if (currentGame == null)
            {
                return;
            }
            ProcessSitRepEntries();

            ClientEvents.ScreenRefreshRequired.Publish(ClientEventArgs.Default);

            if (!_firstTurnStarted)
            {
                _firstTurnStarted = true;
                _ = _navigationService.ActivateScreen(StandardGameScreens.GalaxyScreen);
                ClearGameStartWaitCursor();
            }

            foreach (IInfoCardSubject infoCardSubject in InfoCardService.Current.InfoCards.Select(o => o.Subject).Where(o => o != null))
            {
                infoCardSubject.RefreshData();
            }

            ClearTurnWaitCursor();

            _endTurnCommand.IsActive = true;

            //ProcessSitRepEntries();
        }

        private void ProcessSitRepEntries()
        {
            _text = "ProcessSitRepEntries...";
            Console.WriteLine(_text);
            GameLog.Core.General.DebugFormat(_text);

            if (_appContext.LocalPlayerEmpire.SitRepEntries.Count <= 0) // || _appContext.LocalPlayerEmpire.SitRepEntries.Count > 7)
            {
                return;
            }

            bool _showDetailDialog = false;
            if (ClientSettings.Current != null && ClientSettings.Current.EnableSitRepDetailsScreen == true)
                _showDetailDialog = true;

            List<SitRepEntry> _sitRepsWithDetails = _appContext.LocalPlayerEmpire.SitRepEntries.Where(o => o.HasDetails).ToList();

            //foreach (SitRepEntry sitRepEntry in _appContext.LocalPlayerEmpire.SitRepEntries) // getting an out of range for this collection
            foreach (SitRepEntry sitRepEntry in _sitRepsWithDetails)
            {
                if (_showDetailDialog == true && sitRepEntry != null)
                {
                    // got a null ref from this gamelog. Are we missing a sitRepEntry.SummaryText?
                    // GameLog.Client.General.DebugFormat("###################### SUMMARY: {0}", sitRepEntry.SummaryText);

                    if (/*(ClientSettings.Current != null) && */sitRepEntry.HasDetails/* && ClientSettings.Current.EnableSitRepDetailsScreen*/)   // only show Detail_Dialog if also CombatScreen are shown (if not, a quicker game is possible)
                    {
                        SitRepDetailDialog.Show(sitRepEntry);
                    }
                }
            }

            _text = "ProcessSitRepEntries... done ";
            Console.WriteLine(_text);
            GameLog.Core.General.DebugFormat(_text);

            ShowSummary(false);
        }

        private void ShowSummary(bool showIfEmpty)
        {
            if (!_appContext.IsGameInPlay)
            {
                return;
            }

            SendKeys.SendWait("{F1}");  // shows Map

            _text = "ShowSummary...";
            Console.WriteLine(_text);
            GameLog.Core.GeneralDetails.DebugFormat(_text);

            _sitRepDialog.SitRepEntries = _appContext.LocalPlayerEmpire.SitRepEntries;


            IPlayerOrderService service = ServiceLocator.Current.GetInstance<IPlayerOrderService>();

            if (showIfEmpty)
            {
                _sitRepDialog.Show();
            }
            else if (!service.AutoTurn)
            {
                // works but doubled
                if (ClientSettings.Current.EnableSummaryScreen == true)   // only show SUMMARY if active (if not, a quicker game is possible)
                {
                    //GameLog.Client.GeneralDetails.DebugFormat("################ Setting EnableSummaryScreen = {0} - SUMMARY not shown at false - just click manually to SUMMARY if you want", ClientSettings.Current.EnableCombatScreen.ToString());
                    _sitRepDialog.ShowIfAnyVisibleEntries();
                }
            }

            //SendKeys.SendWait("{F1}");

            _text = "ShowSummary... before storing";
            Console.WriteLine(_text);
            GameLog.Core.GeneralDetails.DebugFormat(_text);

            //string _lastOneDone;
            if (GameContext.Current.TurnNumber > _lastOneDone)
            {
              
                _text = "";
                foreach (SitRepEntry item in _sitRepDialog.SitRepEntries)
                {
                    //string _prio = item.Priority.ToString();
                    //while (_prio.Length < 10)  // length 10 for better reading
                    //{
                    //    _prio += " ";
                    //}

                    _text += newline + "Turn;" + GameContext.Current.TurnNumber
                        //+ ";" + _prio
                        + ";" + item.SummaryText
                        //+ newline
                        ;
                }
                GameLog.Core.SitReps.InfoFormat(_text);

                _text = "SaveSUMMARY_TXT... offline - takes to long time";
                Console.WriteLine(_text);
                GameLog.Core.GeneralDetails.DebugFormat(_text);
                //SaveSUMMARY_TXT(_text);
                _lastOneDone = GameContext.Current.TurnNumber;
                //// \lib\_SUMMARY.txt
                //string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //string file = appDir + "\\lib\\" + "_SUMMARY.txt";

                ////if (!File.Exists(file))
                ////{
                ////    //streamWriter;
                //    StreamWriter streamWriter = new StreamWriter(file);
                //    streamWriter.WriteLine(_text);
                //    streamWriter.Close();
                ////}
            }
            _text = "ShowSummary... DONE";
            Console.WriteLine(_text);
            GameLog.Core.GeneralDetails.DebugFormat(_text);


        }

        private void SaveSUMMARY_TXT(string _text)
        {
            _text += " "; // dummy - please keep
            _text = "SaveSUMMARY_TXT...";
            Console.WriteLine(_text);
            GameLog.Core.GeneralDetails.DebugFormat(_text);
            if (GameContext.Current == null)
            {
                return;
            }

            string file = Path.Combine(
                ResourceManager.GetResourcePath(".\\lib"),
                "_SUMMARY");
        //file = file.Replace(".\\", "");
        //string _text1;
        //_text = "";

        nextTry:
            try
            {
                StreamWriter streamWriter = new StreamWriter(file + ".csv");
                streamWriter.Write(_text);
                streamWriter.Close();
                //Thread.Sleep(500);
            }
            catch
            {
                //string _ask = 
                MessageDialogResult result = MessageDialog.Show(
                    ResourceManager.GetString("FILE_ALREADY_IN_USAGE"),
                    ResourceManager.GetString("FILE_ALREADY_IN_USAGE")
                    + " " + file
                    + " " + ResourceManager.GetString("RETRY_QUESTION"),
                    MessageDialogButtons.YesNo);
                if (result == MessageDialogResult.Yes)
                {
                    goto nextTry;
                }
            }


            //finally
            file += ".txt";
            StreamWriter streamWriter2 = new StreamWriter(file);
            streamWriter2.Write(_text);
            streamWriter2.Close();

            bool autoOpenSummaryTxt = false;
            if (autoOpenSummaryTxt)
            {
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    _ = new FileStream(
                        file,
                        FileMode.Open,
                        FileAccess.Read);

                    //string _file = Path.Combine(ResourceManager.GetResourcePath(""), file + ".txt");
                    if (!string.IsNullOrEmpty(file) && File.Exists(file))
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = file };

                        try { _ = Process.Start(processStartInfo); }
                        catch { _ = System.Windows.MessageBox.Show("Could not load Text-File about SUMMARY"); }
                    }
                }

                //Thread.Sleep(1500);
                string fileCSV_BAT = Path.Combine(
                    ResourceManager.GetResourcePath(".\\lib"),
                    "_SUMMARY.bat");
                if (!string.IsNullOrEmpty(fileCSV_BAT) && File.Exists(fileCSV_BAT))
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = fileCSV_BAT };

                    try { _ = Process.Start(processStartInfo); }
                    catch { _ = System.Windows.MessageBox.Show("Could not load Text-File about SUMMARY"); }
                }
            }
            // end of autoOpenSummaryTxt

            file = Path.Combine(
                ResourceManager.GetResourcePath(".\\lib"),
                "_SUMMARY.hist");

            if (GameContext.Current.TurnNumber == 1)
            {
                if (File.Exists(file))
                {
                    _contentHistoryFile = "NEW started..." 
                        + "-" + GameContext.Current.Options.StartingTechLevel
                        + "-" + GameContext.Current.Options.GalaxySize
                        //+ "-" + GameContext.Current.Options.
                        ;
                }
            }
            else
            {
                ReadPlayersHistoryFile(file);
                _contentHistoryFile += _text;

                ////////if(GameContext.Current.
                try
                {
                    StreamWriter streamWriter3 = new StreamWriter(file);
                    streamWriter3.Write(_contentHistoryFile);
                    streamWriter3.Close();
                }
                catch (Exception)
                {

                    _ = System.Windows.MessageBox.Show(file + " is already in usage");
                }

            }




        }

        public void ReadPlayersHistoryFile(string file)
        {
            _text = "ReadPlayersHistoryFile...";
            Console.WriteLine(_text);
            GameLog.Core.GeneralDetails.DebugFormat(_text);

            _contentHistoryFile = "";

            if (!File.Exists(file))
            {
                return;
            }

            using (StreamReader reader = new StreamReader(file))
            {
                Console.WriteLine("---------------");
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    _contentHistoryFile += line + newline;
                }
                reader.Close();
            }
        }

        private void ClearTurnWaitCursor()
        {
            IDisposable handle = Interlocked.Exchange(ref _turnWaitCursorHandle, null);
            if (handle != null)
            {
                handle.Dispose();
            }
        }

        private void ClearWaitCursors()
        {
            ClearTurnWaitCursor();
            ClearGameStartWaitCursor();
            ClearConnectWaitCursor();
        }

        private void ClearConnectWaitCursor()
        {
            IDisposable handle = Interlocked.Exchange(ref _connectWaitCursorHandle, null);
            if (handle != null)
            {
                handle.Dispose();
            }
        }

        private void ClearGameStartWaitCursor()
        {
            IDisposable handle = Interlocked.Exchange(ref _gameStartWaitCursorHandle, null);
            if (handle != null)
            {
                handle.Dispose();
            }
        }

        private void OnGameStarted(DataEventArgs<GameStartData> args)
        {
            CreatePresenters();
        }

        private void CreatePresenters()
        {
            List<IPresenter> initializedPresenters = new List<IPresenter>();

            GameLog.Client.UIDetails.DebugFormat("BEGINNING: CreatePresenters");

            try
            {
                _screenPresenters.Add(_container.Resolve<IGalaxyScreenPresenter>());
                GameLog.Client.UIDetails.DebugFormat("DONE: IGalaxyScreenPresenter");  // F1-Screen

                _screenPresenters.Add(_container.Resolve<IColonyScreenPresenter>());
                GameLog.Client.UIDetails.DebugFormat("DONE: IColonyScreenPresenter");  // F2-Screen

                _screenPresenters.Add(_container.Resolve<ViewModelPresenter<DiplomacyScreenViewModel, INewDiplomacyScreenView>>());
                GameLog.Client.UIDetails.DebugFormat("DONE: INewDiplomacyScreenView");  // F3-Screen

                _screenPresenters.Add(_container.Resolve<IScienceScreenPresenter>());
                GameLog.Client.UIDetails.DebugFormat("DONE: IScienceScreenPresenter");  // F4-Screen

                _screenPresenters.Add(_container.Resolve<IAssetsScreenPresenter>());
                GameLog.Client.UIDetails.DebugFormat("DONE: IAssetsScreenPresenter");  // F5-Screen

                // XXXXX  not realized yet
                //_screenPresenters.Add(_container.Resolve<IEncyclopediaScreenPresenter>());
                //GameLog.Client.UI.DebugFormat("DONE: IEncyclopediaScreenPresenter");    // F7-Screen

                foreach (IPresenter presenter in _screenPresenters)
                {
                    try
                    {
                        presenter.Run();
                        initializedPresenters.Add(presenter);
                        GameLog.Client.UIDetails.DebugFormat("DONE: {0}", presenter.ToString());
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.UI.Error(string.Format("###### problem with {0}",
                            presenter.ToString()),
                            e);
                        throw;
                    }
                }
            }
            catch
            {
                _screenPresenters.Clear();
                foreach (IPresenter presenter in initializedPresenters)
                {
                    try
                    {
                        presenter.Terminate();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.General.Error(string.Format("###### problem with {0}",
                            presenter.ToString()),
                            e);
                    }
                }
                throw;
            }
        }

        protected void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("GameClient");
            }
        }

        protected void StartServer(bool allowRemoteConnections)
        {
            CheckDisposed();
            try
            {
                _server = _container.Resolve<IGameServer>();
                _server.Faulted += OnServerFaulted;
                _server.Start(_gameOptions, allowRemoteConnections);
            }
            catch
            {
                StopServer();
                ClientEvents.ServerInitializationFailed.Publish(new ClientEventArgs(_appContext));
                throw;
            }
        }

        private void OnServerFaulted(EventArgs t)
        {
            try
            {
                Disconnect();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            try
            {
                Dispose();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        protected void StopServer()
        {
            IGameServer server = Interlocked.Exchange(ref _server, null);
            if (server == null)
            {
                return;
            }

            server.Faulted -= OnServerFaulted;
            if (server.IsRunning)
            {
                try
                {
                    server.Stop();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }

            try
            {
                server.Dispose();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        private void Connect(Action connectAction)
        {
            if (connectAction == null)
            {
                throw new ArgumentNullException("connectAction");
            }

            IGameClient client = Interlocked.CompareExchange(ref _client, null, null);
            if (client == null)
            {
                return;
            }

            HookClientEventHandlers(client);

            try
            {
                connectAction();
            }
            catch
            {
                UnhookClientEventHandlers(client);
                UnhookCommandAndEventHandlers();
                _dispatcher.Invoke(
                    ClearWaitCursors,
                    DispatcherPriority.Normal);
                _ = Interlocked.Exchange(ref _client, null);
                ClientEvents.ClientConnectionFailed.Publish(ClientEventArgs.Default);
                Terminate();
            }
        }

        protected void HookClientEventHandlers(IGameClient client)
        {
            if (client == null)
            {
                return;
            }

            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
        }

        protected void UnhookClientEventHandlers(IGameClient client)
        {
            if (client == null)
            {
                return;
            }

            client.Connected -= OnClientConnected;
            client.Disconnected -= OnClientDisconnected;
        }


        private void OnClientDisconnected(ClientDataEventArgs<ClientDisconnectReason> args)
        {
            if (_suppressClientEvents)
            {
                return;
            }

            _suppressClientEvents = true;
            ClientEvents.ClientDisconnected.Publish(args);
        }

        private void OnClientConnected(EventArgs args)
        {
            if (_suppressClientEvents)
            {
                return;
            }

            ClientEvents.ClientConnected.Publish(new ClientConnectedEventArgs(_isServerLocal));
        }


        private void Disconnect()
        {
            IGameClient client = Interlocked.Exchange(ref _client, null);
            if (client == null)
            {
                return;
            }

            UnhookClientEventHandlers(client);

            try
            {
                client.Disconnect();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            if (!_suppressClientEvents)
            {
                ClientEvents.ClientDisconnected.Publish(new ClientDataEventArgs<ClientDisconnectReason>(ClientDisconnectReason.Disconnected));
            }
        }

        public void RunRemote([NotNull] string playerName, [NotNull] string remoteHost)
        {
            if (playerName == null)
            {
                throw new ArgumentNullException("playerName");
            }

            if (remoteHost == null)
            {
                throw new ArgumentNullException("remoteHost");
            }

            CheckDisposed();

            _dispatcher.Invoke(
                    SetConnectWaitCursor,
                    DispatcherPriority.Normal);

            HookCommandAndEventHandlers();

            try
            {
                try
                {
                    Connect(() => _client.Connect(playerName, remoteHost));
                }
                catch
                {
                    try
                    {
                        Terminate();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.General.Error(e);
                    }

                    ClientEvents.ClientConnectionFailed.Publish(ClientEventArgs.Default);
                }
            }
            catch
            {
                UnhookCommandAndEventHandlers();
                _dispatcher.Invoke(
                    ClearWaitCursors,
                    DispatcherPriority.Normal);
                throw;
            }
        }

        public void Terminate()
        {
            Dispose();
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_appContext.IsGameInPlay)
            {
                try
                {
                    ClientEvents.GameEnding.Publish(ClientEventArgs.Default);
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }

            _ = _navigationService.ActivateScreen(StandardGameScreens.MenuScreen);

            ClearScreenViews();

            try
            {
                Disconnect();
                StopServer();
            }
            finally
            {
                _dispatcher.Invoke(ClearWaitCursors);
            }

            UnhookCommandAndEventHandlers();

            _ = _dispatcher.BeginInvoke((Action)OnTerminated);
        }

        private void ClearScreenViews()
        {
            foreach (IPresenter presenter in _screenPresenters)
            {
                try
                {
                    presenter.Terminate();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
            _screenPresenters.Clear();
        }
        #endregion
    }
}