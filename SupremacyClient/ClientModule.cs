// ClientModule.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Client.OptionsPages;
using Supremacy.Client.Services;
using Supremacy.Client.Views;
using Supremacy.Client.Views.DiplomacyScreen;
using Supremacy.Game;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Types;
using Supremacy.UI;
using Supremacy.Utility;
using Supremacy.WCF;
using System;
using System.Concurrency;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace Supremacy.Client
{
    public class ClientModule : IModule
    {
        public const string ModuleName = "Supremacy.Client.ClientModule";

        #region Fields
        private const string MusicThemeBasePath = "Resources/UI";
        private const string MusicPackFileName = "MusicPacks.xml";

        private readonly IClientApplication _app;
        private readonly IUnityContainer _container;
        private readonly IResourceManager _resourceManager;
        private readonly IRegionManager _regionManager;
        private readonly IRegionViewRegistry _regionViewRegistry;
        private readonly IDispatcherService _dispatcherService;
        private readonly IGameErrorService _errorService;
        private readonly IAppContext _appContext;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly IMusicPlayer _musicPlayer;
        private readonly ISoundPlayer _soundPlayer;

        private readonly ClientOptionsDialog _optionsDialog;

        private readonly DelegateCommand<object> _optionsCommand;
        private readonly DelegateCommand<object> _logTxtCommand;
        private readonly DelegateCommand<object> _errorTxtCommand;
        private readonly DelegateCommand<object> _startSinglePlayerGameCommand;
        private readonly DelegateCommand<object> _continueGameCommand;
        private readonly DelegateCommand<bool> _endGameCommand;
        private readonly DelegateCommand<SavedGameHeader> _loadGameCommand;
        private readonly DelegateCommand<object> _showCreditsDialogCommand;
        private readonly DelegateCommand<MultiplayerConnectParameters> _joinMultiplayerGameCommand;
        private readonly DelegateCommand<string> _hostMultiplayerGameCommand;
        private readonly DelegateCommand<bool> _exitCommand;

        private bool _isExiting;
        private IGameController _gameController;
        #endregion

        #region Constructor & Lifetime
        public ClientModule(
            [NotNull] IClientApplication app,
            [NotNull] IUnityContainer container,
            [NotNull] IResourceManager resourceManager,
            [NotNull] IRegionViewRegistry regionViewRegistry,
            [NotNull] IDispatcherService dispatcherService,
            [NotNull] IGameErrorService errorService,
            [NotNull] IMusicPlayer musicPlayer,
            [NotNull] ISoundPlayer soundPlayer)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            if (container == null)
                throw new ArgumentNullException("container");
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");
            if (regionViewRegistry == null)
                throw new ArgumentNullException("regionViewRegistry");
            if (dispatcherService == null)
                throw new ArgumentNullException("dispatcherService");
            if (errorService == null)
                throw new ArgumentNullException("errorService");
            if (musicPlayer == null)
                throw new ArgumentNullException("musicPlayer");
            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");

            _app = app;
            _container = container;
            _resourceManager = resourceManager;
            _regionViewRegistry = regionViewRegistry;
            _dispatcherService = dispatcherService;
            _errorService = errorService;
            _musicPlayer = musicPlayer;
            _soundPlayer = soundPlayer;

            _appContext = _container.Resolve<IAppContext>();
            _regionManager = _container.Resolve<IRegionManager>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            _optionsDialog = new ClientOptionsDialog();

            _optionsCommand = new DelegateCommand<object>(
                ExecuteOptionsCommand);

            _logTxtCommand = new DelegateCommand<object>(
                ExecuteLogTxtCommand);

            _errorTxtCommand = new DelegateCommand<object>(
                ExecuteErrorTxtCommand);

            _startSinglePlayerGameCommand = new DelegateCommand<object>(
                ExecuteStartSinglePlayerGameCommand);

            _continueGameCommand = new DelegateCommand<object>(
                ExecuteContinueGameCommand);

            _endGameCommand = new DelegateCommand<bool>(
                ExecuteEndGameCommand);

            _exitCommand = new DelegateCommand<bool>(
                ExecuteExitCommand);

            _loadGameCommand = new DelegateCommand<SavedGameHeader>(
                ExecuteLoadGameCommand);

            _showCreditsDialogCommand = new DelegateCommand<object>(
                ExecuteShowCreditsDialogCommand);

            _joinMultiplayerGameCommand = new DelegateCommand<MultiplayerConnectParameters>(
                ExecuteJoinMultiplayerGameCommand);

            _hostMultiplayerGameCommand = new DelegateCommand<string>(
                ExecuteHostMultiplayerGameCommand);
        }
        #endregion

        #region Commands
        private static void ExecuteShowCreditsDialogCommand(object parameter)
        {

            // makes a crash, maybe since Data was moved to \bin

            //var creditsPage = Application.LoadComponent(
            //    new Uri(
            //        "/SupremacyClient;Component/Resources/Credits.xaml",
            //        UriKind.RelativeOrAbsolute));

            //MessageDialog.Show(creditsPage, MessageDialogButtons.Close);

            var result = MessageDialog.Show("Please have a look to Credits.xaml !", MessageDialogButtons.Close);
        }

        private void ExecuteHostMultiplayerGameCommand(string playerName)
        {
            var initData = GameInitData.CreateMultiplayerGame(GameOptionsManager.LoadDefaults(), playerName);
            RunGameController(gameController => gameController.RunLocal(initData), true);
        }

        private void ExecuteJoinMultiplayerGameCommand(MultiplayerConnectParameters parameters)
        {
            RunGameController(gameController => gameController.RunRemote(parameters.PlayerName, parameters.RemoteHost), true);
        }

        private void ExecuteLoadGameCommand(SavedGameHeader header)
        {
            var initData = GameInitData.CreateFromSavedGame(header);
            GameLog.Client.General.Debug("doing ExecuteLoadGameCommand ...");
            RunGameController(gameController => gameController.RunLocal(initData), initData.IsMultiplayerGame);
            GameLog.Client.General.Debug("doing gameController.RunLocal(initData) ...");

        }

        private void ExecuteOptionsCommand(object obj)
        {
            _optionsDialog.ShowDialog();
        }

        private void ExecuteLogTxtCommand(object obj)
        {
            var folder = ResourceManager.GetResourcePath("");
            var logfile = Path.Combine(
                folder,
                "Log.txt");
            // maybe not working ->    GameLog.Client.General.DebugFormat("Logfile at {0}", logfile);

            if (logfile != null) //&& resourceFile.Exists)
            {
                var p = new Process();
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = logfile;
                p.StartInfo = pi;
                try
                {
                    p.Start();
                }
                catch 
                {
                    MessageBox.Show("Could not load Log.txt");
                }
            }
        }

        private void ExecuteErrorTxtCommand(object obj)
        {
            var folder = ResourceManager.GetResourcePath("");
            var errorfile = Path.Combine(
                folder,
                "Error.txt");
            // maybe not working ->    GameLog.Client.General.DebugFormat("Logfile at {0}", errorfile);

            double fileSize = new FileInfo(errorfile).Length;
            if (File.Exists(errorfile) && fileSize > 0)
            {
                var p = new Process();
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = errorfile;
                p.StartInfo = pi;
                    
                try
                {
                    p.Start();
                }
                catch
                {
                    MessageBox.Show("Could not load Error.txt");
                }
            }
        }

        private void ExecuteContinueGameCommand(object obj)
        {
            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
        }

        private void ExecuteExitCommand(bool showConfirmation)
        {
            Exit(showConfirmation);
        }

        private void ExecuteEndGameCommand(bool showConfirmation)
        {
            EndGame(showConfirmation);
        }
        #endregion

        #region Methods
        private void Exit(bool showConfirmation)
        {
            if (_isExiting)
                return;

            _isExiting = true;
            try
            {
                if (!EndGame(showConfirmation))
                    return;
            }
            finally
            {
                _isExiting = false;
            }
            Application.Current.Shutdown();
        }

        private bool EndGame(bool showConfirmation)
        {
            if (showConfirmation && (_appContext.IsGameInPlay || _appContext.IsGameInPlay))
            {
                var result = MessageDialog.Show(
                    _isExiting ? "Confirm Exit" : "Confirm Quit",
                    "Are you sure you want to " + (_isExiting ? "exit?" : "quit?"),
                    MessageDialogButtons.YesNo);
                if (result != MessageDialogResult.Yes)
                    return false;
            }

            var gameController = Interlocked.CompareExchange(ref _gameController, null, null);

            if (gameController == null)
                return true;

            gameController.Terminate();

            // when current game is terminated, go back to main menu music
            _appContext.ThemeMusicLibrary.Clear();
            _musicPlayer.SwitchMusic("DefaultMusic");

            GameLog.Client.General.Info("Game was exited");

            return true;
        }
        #endregion

        #region Implementation of IModule
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Initialize()
        {
            GameLog.Client.General.InfoFormat("Initializing... !");
            RegisterViewsAndServices();
            RegisterEventHandlers();
            RegisterCommandHandlers();
            UpdateCommands();

            UIHelpers.IsAutomaticBrowserLaunchEnabled = true;

            if (AutoLoadSavedGame())
                return;

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);
            GameLog.Client.General.InfoFormat("MenuScreen activated... ");
            _soundPlayer.PlayFile("Resources/SoundFX/MenuScreen.wav");
        }

        private bool AutoLoadSavedGame()
        {
            var savedGameFile = _app.CommandLineArguments.SavedGame;

            if (string.IsNullOrWhiteSpace(savedGameFile))
                return false;

            try
            {
                var header = SavedGameManager.LoadSavedGameHeader(savedGameFile);
                if (header != null)
                {
                    ClientCommands.LoadGame.Execute(header);
                    return true;
                }
            }
            catch (Exception exception)
            {
                GameLog.Client.General.Error(
                    string.Format(@"Error loading saved game '{0}'.", savedGameFile),
                    exception);
            }

            return false;
        }

        private void RegisterViewsAndServices()
        {
            _container.RegisterInstance(GameOptionsManager.LoadDefaults());

            _container.RegisterType<IScheduler, EventLoopScheduler>(new ContainerControlledLifetimeManager());
            _container.RegisterType<INavigationService, NavigationService>(new ContainerControlledLifetimeManager());

            _container.Resolve<INavigationService>();

            _container.RegisterType<IGameObjectIDService, GameObjectIDService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISupremacyCallback, GameClientCallback>(new TransientLifetimeManager());
            _container.RegisterType<IGameClient, GameClient>(new TransientLifetimeManager());
            _container.RegisterType<IGameServer, GameServer>(new TransientLifetimeManager());
            _container.RegisterType<IPlayerOrderService, PlayerOrderService>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IPlayerTarget1Service, PlayerTarget1Service>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IPlayerTarget2Service, PlayerTarget2Service>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IGameController, GameController>(new TransientLifetimeManager());
            //_container.RegisterInstance<IScriptService>(new ScriptService());*/

            _container.RegisterType<StatusWindow>(new ContainerControlledLifetimeManager());
            _container.RegisterInstance(new CombatWindow());

            _container.RegisterType<GalaxyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ColonyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<DiplomacyScreenViewModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ScienceScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<AssetsScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<SystemAssaultScreenViewModel>(new ContainerControlledLifetimeManager());

            _container.RegisterType<IGalaxyScreenView, GalaxyScreenView>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IColonyScreenView, ColonyScreenView>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<INewDiplomacyScreenView, NewDiplomacyScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IScienceScreenView, ResearchScreen>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IIntelScreenView, IntelScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IAssetsScreenView, AssetsScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ILobbyScreenView, MultiplayerLobby>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISystemAssaultScreenView, SystemAssaultScreen>(new ContainerControlledLifetimeManager());

            _container.RegisterType<IGalaxyScreenPresenter, GalaxyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IColonyScreenPresenter, ColonyScreenPresenter>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IDiplomacyScreenPresenter, DiplomacyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IScienceScreenPresenter, ScienceScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IAssetsScreenPresenter, AssetsScreenPresenter>(new ExternallyControlledLifetimeManager());

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MenuScreen, typeof(MenuScreen));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MultiplayerLobby, typeof(ILobbyScreenView));

            // first is first shown in Options
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(SecondOptionsPage));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(TracesOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AudioOptionsPage));   // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GraphicsOptionsPage));  // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GeneralOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));

            _regionViewRegistry.RegisterViewWithRegion(CommonGameScreenRegions.PlanetsView, typeof(StarSystemPanel));

            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireOverview, typeof(EmpireInfoView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireResources, typeof(EmpireResourcesView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.GalaxyGrid, typeof(GalaxyGridView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TradeRouteList, typeof(TradeRouteListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TaskForceList, typeof(TaskForceListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AssignedShipList, typeof(AssignedShipListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AvailableShipList, typeof(AvailableShipListView));
            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.ShipStats, typeof(ShipInfoPanel));

            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ColonyInfo, ColonyScreenRegions.ColonyInfo, typeof(ColonyInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ProductionManagement, ColonyScreenRegions.ProductionManagement, typeof(SystemProductionPanel));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.PlanetaryBuildQueue, ColonyScreenRegions.PlanetaryBuildQueue, typeof(ColonyPlanetaryBuildQueueView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.PlanetaryBuildList, ColonyScreenRegions.PlanetaryBuildList, typeof(ColonyPlanetaryBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildQueue, ColonyScreenRegions.ShipyardBuildQueue, typeof(ColonyShipyardBuildQueueView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildList, ColonyScreenRegions.ShipyardBuildList, typeof(ColonyShipyardBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedShipyardBuildProjectInfo, ColonyScreenRegions.SelectedShipyardBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.StructureList, ColonyScreenRegions.StructureList, typeof(ColonyStructureListView));
        }

        private void RegisterEventHandlers()
        {
            ClientEvents.ClientConnectionFailed.Subscribe(OnClientConnectionFailed, ThreadOption.UIThread);
            ClientEvents.ClientDisconnected.Subscribe(OnClientDisconnected, ThreadOption.UIThread);
            ClientEvents.GameStarted.Subscribe(OnGameStarted, ThreadOption.UIThread);
            ClientEvents.GameStarting.Subscribe(OnGameStarting, ThreadOption.UIThread);
            ClientEvents.GameEnding.Subscribe(OnGameEnding, ThreadOption.UIThread);
            ClientEvents.ClientConnected.Subscribe(OnClientConnected, ThreadOption.BackgroundThread);
            ClientEvents.LocalPlayerJoined.Subscribe(OnLocalPlayerJoined, ThreadOption.UIThread);
            ClientEvents.PlayerExited.Subscribe(OnPlayerExited, ThreadOption.UIThread);

            Channel<GameSavedMessage>.Public
                .ObserveOn(Scheduler.ThreadPool)
                .Subscribe(_ => ShellIntegration.UpdateJumpList());
        }

        private void OnPlayerExited(ClientDataEventArgs<IPlayer> args)
        {
            var player = args.Value;

            if (!_appContext.IsGameInPlay)
                return;

            if (Equals(player, _appContext.LocalPlayer))
                return;

            var remainingPlayers = _appContext.RemotePlayers.Where(o => !Equals(o, player));
            if (!remainingPlayers.Any())
            {
                var result = MessageDialog.Show(
                    _resourceManager.GetString("PLAYER_EXITED_MESSAGE_HEADER"),
                    _resourceManager.GetStringFormat("LAST_PLAYER_EXITED_MESSAGE_CONTENT", player.Name),
                    MessageDialogButtons.YesNo);
                if (result == MessageDialogResult.No)
                    EndGame(false);
            }
            else
            {
                MessageDialog.Show(
                    _resourceManager.GetString("PLAYER_EXITED_MESSAGE_HEADER"),
                    _resourceManager.GetStringFormat("PLAYER_EXITED_MESSAGE_CONTENT", player.Name),
                    MessageDialogButtons.Ok);
            }
        }

        private void OnLocalPlayerJoined(LocalPlayerJoinedEventArgs args)
        {
            if (!_appContext.IsSinglePlayerGame)
                ClearStatusWindow();
        }

        private void OnGameStarting(ClientEventArgs obj)
        {
            if (!_appContext.IsSinglePlayerGame)
                ShowLoadingScreen();
        }

        private void ShowLoadingScreen()
        {
            var statusWindow = _container.Resolve<StatusWindow>();
            //statusWindow.Header = _resourceManager.GetString("LOADING_GAME_MESSAGE");
            statusWindow.Header = " ***     Loading Game . . .      ***  " +Environment.NewLine;
                //"----------------------------"; // + Environment.NewLine +

                statusWindow.Content =
                //"----------------------------" + Environment.NewLine +
                "- For more information on game play please read the manual." + Environment.NewLine +
                "----------------------------" + Environment.NewLine +

                "Star Trek and it's related images and characters are solely owned " + Environment.NewLine +
                "by CBS Studios and Paramount Pictures. " + Environment.NewLine +
                "----------------------------" + Environment.NewLine +
                "This fan game is not endorsed or affiliated with them. " + Environment.NewLine +
                "It is a non-commercial, free, unfunded, amateur game for " + Environment.NewLine +
                "recreational use only. No commercial exhibition is permitted.";



            // Hints screen will not show for host of a multiplayer game so is excluded here, the host cannot progress to the loaded game.
            // to do line 425 to 435 add hint for people new to game

            //if (_appContext.IsSinglePlayerGame == false)   // see below, depending on Length out of en.txt or later on OPTION
            {
                var _hints = _resourceManager.GetString("LOADING_GAME_HINTS");
                
                if (_hints.Length > 0)   // later: make additional OPTION to show hints or not
                {
                    var result = MessageDialog.Show(statusWindow.Content = _resourceManager.GetString("LOADING_GAME_HINTS"),
                                MessageDialogButtons.Ok);
                    //"Remember:" + Environment.NewLine +
                    //    "- Right mouse click in the game to see the Panel Access Menu." + Environment.NewLine +
                    //    "- Before assaulting a system declare war in an earlier turn." + Environment.NewLine +
                    //    "- In Diplomacy select a race then click the Outbox to declare war. " + Environment.NewLine +
                    //    "- To conquer a system assault it with transport ships in your taskforce.",
                }
            }

            //statusWindow.Content = null;
            statusWindow.Show();

            var gameScreensRegion = _container.Resolve<IRegionManager>().Regions[ClientRegions.GameScreens];
            gameScreensRegion.Deactivate(gameScreensRegion.GetView(StandardGameScreens.MenuScreen));
            gameScreensRegion.Deactivate(gameScreensRegion.GetView(StandardGameScreens.MultiplayerLobby));
        }

        private void OnGameEnding(ClientEventArgs obj)
        {
            UpdateCommands();
        }

        private void OnClientConnected(ClientConnectedEventArgs obj)
        {
            UpdateCommands();
        }

        private void UpdateCommands()
        {
            bool isConnected = _appContext.IsConnected;
            bool isGameEnding = _appContext.IsGameEnding;
            bool isGameInPlay = _appContext.IsGameInPlay;
            bool gameControllerExists = (Interlocked.CompareExchange(ref _gameController, null, null) != null);

            _optionsCommand.IsActive = true;
            _logTxtCommand.IsActive = true;
            _errorTxtCommand.IsActive = true;
            _showCreditsDialogCommand.IsActive = true;
            _startSinglePlayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _joinMultiplayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _hostMultiplayerGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _loadGameCommand.IsActive = !isConnected && !isGameEnding && !gameControllerExists;
            _continueGameCommand.IsActive = isGameInPlay;
            _endGameCommand.IsActive = isConnected && !isGameEnding;
        }

        private void OnGameStarted(ClientDataEventArgs<GameStartData> obj)
        {
            UpdateCommands();

            if (_appContext.IsGameInPlay)
            {
                if (_appContext.LocalPlayer.Empire.Key == "INTRO")
                    LoadTheme("Intro");
                else if (_appContext.LocalPlayer.Empire.Key == "FEDERATION")
                    LoadTheme("Federation");
                else if (_appContext.LocalPlayer.Empire.Key == "ROMULANS")
                    LoadTheme("Romulans");
                else if (_appContext.LocalPlayer.Empire.Key == "KLINGONS")
                    LoadTheme("Klingons");
                else if (_appContext.LocalPlayer.Empire.Key == "CARDASSIANS")
                    LoadTheme("Cardassians");
                else if (_appContext.LocalPlayer.Empire.Key == "DOMINION")
                    LoadTheme("Dominion");
                else if (_appContext.LocalPlayer.Empire.Key == "BORG")
                    LoadTheme("Borg");
                else if (_appContext.LocalPlayer.Empire.Key == "TERRANEMPIRE")
                    LoadTheme("TerranEmpire");
                else
                    LoadDefaultTheme();
            }
        }

        public void LoadDefaultTheme()
        {
            _app.LoadDefaultResources();
        }

        public void LoadTheme(string theme)
        {
            //works: GameLog.Client.GameData.DebugFormat("ClientModule.cs: UI-Theme={0} (or default), EmpireID={1}", theme, _appContext.LocalPlayer.EmpireID);

            if (!_app.LoadThemeResources(theme))
                _app.LoadDefaultResources();

            themeShipyard = theme;

            _app.LoadThemeResourcesShipyard(themeShipyard);

            // load theme music
            _appContext.ThemeMusicLibrary.Load(Path.Combine(MusicThemeBasePath, theme, MusicPackFileName));
            _musicPlayer.SwitchMusic("DefaultMusic");
        }

        private void OnClientConnectionFailed(EventArgs args)
        {
            ClearStatusWindow();

            MessageDialog.Show(
                _resourceManager.GetString("CLIENT_CONNECTION_FAILURE_HEADER"),
                _resourceManager.GetString("CLIENT_CONNECTION_FAILURE_MESSAGE"),
                MessageDialogButtons.Ok);

            var gameController = Interlocked.Exchange(ref _gameController, null);
            if (gameController == null)
                return;

            try
            {
                gameController.Terminate();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            UpdateCommands();
        }

        protected void ClearStatusWindow()
        {
            var statusWindow = _container.Resolve<StatusWindow>();
            if ((statusWindow != null) && statusWindow.IsOpen)
                statusWindow.Close();
        }

        private void OnClientDisconnected(DataEventArgs<ClientDisconnectReason> args)
        {
            ClearStatusWindow();

            var gameController = Interlocked.Exchange(ref _gameController, null);
            if (gameController == null)
                return;

            string disconnectMessage = null;

            switch (args.Value)
            {
                case ClientDisconnectReason.ConnectionBroken:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_CONNECTION_BROKEN");
                    break;
                case ClientDisconnectReason.GameAlreadyStarted:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_GAME_ALREADY_STARTED");
                    break;
                case ClientDisconnectReason.GameIsFull:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_GAME_IS_FULL");
                    break;
                case ClientDisconnectReason.VersionMismatch:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_VERSION_MISMATCH");
                    break;
                case ClientDisconnectReason.ConnectionClosed:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_CONNECTION_CLOSED");
                    break;
                case ClientDisconnectReason.LoadGameFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_LOAD_GAME_FAILURE");
                    break;
                case ClientDisconnectReason.LocalServiceFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_LOCAL_SERVICE_FAILURE");
                    break;
                case ClientDisconnectReason.UnknownFailure:
                    disconnectMessage = _resourceManager.GetString("CLIENT_DISCONNECT_MESSAGE_UNKNOWN_FAILURE");
                    break;
            }

            if (disconnectMessage != null)
            {
                MessageDialog.Show(
                    _resourceManager.GetString("CLIENT_DISCONNECT_HEADER"),
                    disconnectMessage,
                    MessageDialogButtons.Ok);
            }

            try
            {
                _navigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);
            }
            finally
            {
                try
                {
                    gameController.Terminate();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }

            UpdateCommands();

            //AsyncHelper.Invoke((Action)GC.Collect);
        }
        #endregion

        private void RegisterCommandHandlers()
        {
            ClientCommands.OptionsCommand.RegisterCommand(_optionsCommand);
            ClientCommands.LogTxtCommand.RegisterCommand(_logTxtCommand);
            ClientCommands.ErrorTxtCommand.RegisterCommand(_errorTxtCommand);
            ClientCommands.StartSinglePlayerGame.RegisterCommand(_startSinglePlayerGameCommand);
            ClientCommands.ContinueGame.RegisterCommand(_continueGameCommand);
            ClientCommands.EndGame.RegisterCommand(_endGameCommand);
            ClientCommands.JoinMultiplayerGame.RegisterCommand(_joinMultiplayerGameCommand);
            ClientCommands.HostMultiplayerGame.RegisterCommand(_hostMultiplayerGameCommand);
            ClientCommands.LoadGame.RegisterCommand(_loadGameCommand);
            ClientCommands.ShowCreditsDialog.RegisterCommand(_showCreditsDialogCommand);
            ClientCommands.Exit.RegisterCommand(_exitCommand);
        }

        private void ExecuteStartSinglePlayerGameCommand(object parameter)
        {
            if (Interlocked.CompareExchange(ref _gameController, null, null) != null)
                return;

            LoadDefaultTheme();

            var startScreen = new SinglePlayerStartScreen(_soundPlayer);

            // deactivate following completely for switching to using MP-Screen as well for SP
            var dialogResult = startScreen.ShowDialog();

            if (!dialogResult.HasValue || !dialogResult.Value)
                return;

            var initData = GameInitData.CreateSinglePlayerGame(startScreen.Options, startScreen.EmpireID);

            //if (startScreen.EmpireID = 5)
            //    var initData = GameInitData.CreateSinglePlayerGame(startScreen.Options, themeID);

            RunGameController(gameController => gameController.RunLocal(initData), false);

            // activate following for switching to using MP-Screen as well for SP
            //var initData = GameInitData.CreateMultiplayerGame(GameOptionsManager.LoadDefaults(), "LOCAL PLAYER");
            //RunGameController(gameController => gameController.RunLocal(initData), true);
        }

        private void RunGameController(Action<IGameController> runDelegate, bool remoteConnection)
        {
            if (Interlocked.CompareExchange(ref _gameController, null, null) != null)
                return;

            try
            {
                _gameController = ResolveGameController();

                if (remoteConnection)
                    ShowConnectingScreen();
                else
                    ShowLoadingScreen();

                runDelegate.BeginInvoke(
                    _gameController,
                    delegate(IAsyncResult result)
                    {
                        try
                        {
                            runDelegate.EndInvoke(result);
                            //GameLog.Print("trying runDelegate.EndInvoke");
                        }
                        catch (SupremacyException e)
                        {
                            GameLog.Client.General.Error("runDelegate.EndInvoke failed", e);
                            Interlocked.Exchange(ref _gameController, null);
                            _dispatcherService.InvokeAsync((Action)ClearStatusWindow);
                            _errorService.HandleError(e);
                            _dispatcherService.InvokeAsync((Action)ActivateMenuScreen);
                        }
                    },
                    null);
            }
            catch (SupremacyException e)
            {
                GameLog.Client.General.Error("ResolveGameController failed", e);
                ClearStatusWindow();
                _errorService.HandleError(e);
                Interlocked.Exchange(ref _gameController, null);
                ActivateMenuScreen();
            }

        }

        private void ShowConnectingScreen()
        {
            var statusWindow = _container.Resolve<StatusWindow>();
            statusWindow.Header = "Connecting";
            statusWindow.Content = null;
            statusWindow.Show();

        }

        protected void DeactivateMenuScreen()
        {
            var region = _regionManager.Regions[ClientRegions.GameScreens];
            if (region == null)
                return;

            var menuScreen = region.GetView(StandardGameScreens.MenuScreen);
            if (menuScreen == null)
                return;

            region.Deactivate(menuScreen);
        }

        protected void ActivateMenuScreen()
        {
            var region = _regionManager.Regions[ClientRegions.GameScreens];
            if (region == null)
                return;

            var menuScreen = region.GetView(StandardGameScreens.MenuScreen);
            if (menuScreen == null)
                return;

            region.Activate(menuScreen);
        }

        private IGameController ResolveGameController()
        {
            GCHelper.Collect();

            var gameController = _container.Resolve<IGameController>();
            if (gameController == null)
                throw new SupremacyException("A game controller could not be created.");

            if (Interlocked.CompareExchange(ref _gameController, gameController, null) != null)
                return _gameController;

            gameController.Terminated += OnGameControllerTerminated;
            return gameController;
        }

        private void OnGameControllerTerminated(object sender, EventArgs args)
        {
            var gameController = sender as IGameController;
            if (gameController == null)
                return;
            gameController.Terminated -= OnGameControllerTerminated;
            Interlocked.CompareExchange(ref _gameController, null, gameController);
            _app.DoEvents();
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            UpdateCommands();
        }

        public string themeShipyard { get; set; }
    }
}