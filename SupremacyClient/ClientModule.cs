// File:ClientModule.cs
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
using System.Collections.Generic;

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
        private readonly ClientTracesDialog _tracesDialog;
        private readonly F06_Dialog _f06_Dialog;
        //private readonly F07_Dialog _encyclopedia_Dialog;
        private readonly F07_Dialog _f07_Dialog;
        private readonly F08_Dialog _f08_Dialog;
        private readonly F09_Dialog _f09_Dialog;
        private readonly F10_Dialog _f10_Dialog;
        private readonly F11_Dialog _f11_Dialog;
        private readonly F12_Dialog _f12_Dialog;
        private readonly FakeDialog _fakeDialog;

        private readonly DelegateCommand<object> _optionsCommand;
        private readonly DelegateCommand<object> _tracesCommand;
        private readonly DelegateCommand<object> _f06_Command;
        private readonly DelegateCommand<object> _f07_Command;
        private readonly DelegateCommand<object> _f08_Command;
        private readonly DelegateCommand<object> _f09_Command;
        private readonly DelegateCommand<object> _f10_Command;
        private readonly DelegateCommand<object> _f11_Command;
        private readonly DelegateCommand<object> _f12_Command;

        private readonly DelegateCommand<object> _s0_Command;   // start Single Player Empire 0
        private readonly DelegateCommand<object> _s1_Command;
        private readonly DelegateCommand<object> _s2_Command;
        private readonly DelegateCommand<object> _s3_Command;
        private readonly DelegateCommand<object> _s4_Command;
        private readonly DelegateCommand<object> _s5_Command;
        private readonly DelegateCommand<object> _s6_Command;

        private readonly DelegateCommand<object> _fakeCommand;
        private readonly DelegateCommand<object> _logTxtCommand;
        private readonly DelegateCommand<object> _errorTxtCommand;
        private readonly DelegateCommand<object> _startSinglePlayerGameCommand;
        private readonly DelegateCommand<object> _continueGameCommand;
        private readonly DelegateCommand<bool> _endGameCommand;
        private readonly DelegateCommand<SavedGameHeader> _loadGameCommand;
        private readonly DelegateCommand<object> _showCreditsDialogCommand;
        private readonly DelegateCommand<object> _showSettingsFileCommand;
        private readonly DelegateCommand<MultiplayerConnectParameters> _joinMultiplayerGameCommand;
        private readonly DelegateCommand<string> _hostMultiplayerGameCommand;
        private readonly DelegateCommand<bool> _exitCommand;

        private string localEmpire = "";
        private int startTechLvl = -1;

        private bool _isExiting;
        private IGameController _gameController;
        private string _text;
        private readonly string newline = Environment.NewLine;
        //private string _trueText;
        //private string _falseText;
        //private string _restText;
        //private string truesText;
        private string _resultText;
        //private string falseText;
        //private string restText;
        //private int _truelength;
        //private int _length;
        //private Dictionary<int, string, string, string> _array;
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
            _app = app ?? throw new ArgumentNullException("app");
            _container = container ?? throw new ArgumentNullException("container");
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            _regionViewRegistry = regionViewRegistry ?? throw new ArgumentNullException("regionViewRegistry");
            _dispatcherService = dispatcherService ?? throw new ArgumentNullException("dispatcherService");
            _errorService = errorService ?? throw new ArgumentNullException("errorService");
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException("musicPlayer");
            _soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");

            _appContext = _container.Resolve<IAppContext>();
            _regionManager = _container.Resolve<IRegionManager>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            _optionsDialog = new ClientOptionsDialog();
            _optionsCommand = new DelegateCommand<object>(
                ExecuteOptionsCommand);

            _tracesDialog = new ClientTracesDialog();
            _tracesCommand = new DelegateCommand<object>(
                ExecuteTracesCommand);

            _f06_Dialog = new F06_Dialog();
            _f06_Command = new DelegateCommand<object>(
                Execute_f06_Command);

            _f07_Dialog = new F07_Dialog();
            _f07_Command = new DelegateCommand<object>(
                Execute_f07_Command);

            _f08_Dialog = new F08_Dialog();
            _f08_Command = new DelegateCommand<object>(
                Execute_f08_Command);

            _f09_Dialog = new F09_Dialog();
            _f09_Command = new DelegateCommand<object>(
                Execute_f09_Command);

            _f10_Dialog = new F10_Dialog();
            _f10_Command = new DelegateCommand<object>(
                Execute_f10_Command);

            _f11_Dialog = new F11_Dialog();
            _f11_Command = new DelegateCommand<object>(
                Execute_f11_Command);

            _f12_Dialog = new F12_Dialog();
            _f12_Command = new DelegateCommand<object>(
                Execute_f12_Command);

            _s0_Command = new DelegateCommand<object>(Execute_s0_Command); // start Single Player Empire 0
            _s1_Command = new DelegateCommand<object>(Execute_s1_Command);
            _s2_Command = new DelegateCommand<object>(Execute_s2_Command);
            _s3_Command = new DelegateCommand<object>(Execute_s3_Command);
            _s4_Command = new DelegateCommand<object>(Execute_s4_Command);
            _s5_Command = new DelegateCommand<object>(Execute_s5_Command);
            _s6_Command = new DelegateCommand<object>(Execute_s6_Command);

            _fakeDialog = new FakeDialog();
            _fakeCommand = new DelegateCommand<object>(ExecuteFakeCommand);

            _logTxtCommand = new DelegateCommand<object>(ExecuteLogTxtCommand);
            _errorTxtCommand = new DelegateCommand<object>(ExecuteErrorTxtCommand);

            _startSinglePlayerGameCommand = new DelegateCommand<object>(ExecuteStartSinglePlayerGameCommand);
            _continueGameCommand = new DelegateCommand<object>(ExecuteContinueGameCommand);
            _endGameCommand = new DelegateCommand<bool>(ExecuteEndGameCommand);
            _exitCommand = new DelegateCommand<bool>(ExecuteExitCommand);
            _loadGameCommand = new DelegateCommand<SavedGameHeader>(ExecuteLoadGameCommand);
            _showCreditsDialogCommand = new DelegateCommand<object>(ExecuteShowCreditsDialogCommand);
            _showSettingsFileCommand = new DelegateCommand<object>(ExecuteShowSettingsFileCommand);
            _joinMultiplayerGameCommand = new DelegateCommand<MultiplayerConnectParameters>(ExecuteJoinMultiplayerGameCommand);
            _hostMultiplayerGameCommand = new DelegateCommand<string>(ExecuteHostMultiplayerGameCommand);
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

            //MessageDialog.Show("Please have a look to Credits.xaml !", MessageDialogButtons.Close);

            string file = "Credits_for_Rise_of_the_UFP.pdf";
            try
            {
                if (System.IO.File.Exists(file))
                    System.Diagnostics.Process.Start(file);
            }
            catch
            {
                MessageDialog.Show("Please have a look to Credits.xaml !", MessageDialogButtons.Close);
            }
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

            startTechLvl = GetStartTechLvl(initData.Options.StartingTechLevel.ToString());
            localEmpire = GetLocalEmpireShortage(initData.LocalPlayerEmpireID, out string localempire);
        }

        private void ExecuteOptionsCommand(object obj) { _optionsDialog.ShowDialog();}
        private void ExecuteTracesCommand(object obj) { _tracesDialog.ShowDialog();}

        private void Execute_f06_Command(object obj) { _f06_Dialog.ShowDialog(); }
        private void Execute_f07_Command(object obj) { _f07_Dialog.ShowDialog(); }
        private void Execute_f08_Command(object obj) { _f08_Dialog.ShowDialog();}
        private void Execute_f09_Command(object obj) { _f09_Dialog.ShowDialog();}
        private void Execute_f10_Command(object obj) { _f10_Dialog.ShowDialog();}
        private void Execute_f11_Command(object obj) { _f11_Dialog.ShowDialog();}
        private void Execute_f12_Command(object obj) { _f12_Dialog.ShowDialog();}

        private void Execute_s0_Command(object obj) { ExecuteSP_DirectlyGameCommand(0);}
        private void Execute_s1_Command(object obj) { ExecuteSP_DirectlyGameCommand(1);}
        private void Execute_s2_Command(object obj) { ExecuteSP_DirectlyGameCommand(2);}
        private void Execute_s3_Command(object obj) { ExecuteSP_DirectlyGameCommand(3);}
        private void Execute_s4_Command(object obj) { ExecuteSP_DirectlyGameCommand(4);}
        private void Execute_s5_Command(object obj) { ExecuteSP_DirectlyGameCommand(5);}
        private void Execute_s6_Command(object obj) { ExecuteSP_DirectlyGameCommand(6); }

        private void ExecuteFakeCommand(object obj) { _fakeDialog.ShowDialog(); }
        private void ExecuteLogTxtCommand(object obj)
        {
            var logFile = Path.Combine(
                ResourceManager.GetResourcePath(""),
                "Log.txt");

            if (!string.IsNullOrEmpty(logFile) && File.Exists(logFile))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = logFile
                };

                try
                {
                    Process.Start(processStartInfo);
                }
                catch 
                {
                    MessageBox.Show("Could not load Log.txt");
                }
            }
        }

        private void ExecuteErrorTxtCommand(object obj)
        {
            var errorFile = Path.Combine(ResourceManager.GetResourcePath(""),"Error.txt");

            if (!string.IsNullOrEmpty(errorFile) && File.Exists(errorFile))
            {
                double fileSize = new FileInfo(errorFile).Length;
                if (fileSize == 0) { MessageBox.Show("Error.txt is empty - nothing to load"); return;}
                if (fileSize < 0) { MessageBox.Show("Could not load Error.txt");return;}

                ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = errorFile };

                try { _ = Process.Start(processStartInfo);}
                catch { MessageBox.Show("Could not load Error.txt");}
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

        private void ExecuteShowSettingsFileCommand(object obj)
        {
            var file = Path.Combine(
                ResourceManager.GetResourcePath(""),
                "SupremacyClient..Settings.xaml");
            file = file.Replace(".\\", "");
            //string _text1;

            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                var stream = new FileStream(
                    file,
                    FileMode.Open,
                    FileAccess.Read);

                _text = "";

                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("---------------");

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        //Console.WriteLine(line);
                        _text += line;
                    }

                }
                //stream.Close;
            }

            var coll = _text.Split(' ');
            var _trues = new List<String>(); /*_trues.Clear();*/
            var _false = new List<String>(); /*_false.Clear();*/
            var _rest = new List<String>(); /*_rest.Clear();*/
            //_array = new Dictionary<int, string, string, string>();

            foreach (var item in coll)
                {
                    Console.WriteLine(item);
                    if (item.Contains("True")) { _trues.Add(item); }// += item + newline;}
                    if (item.Contains("False")) { _false.Add(item); }
                    if (!item.Contains("True") && !item.Contains("False")) { _rest.Add(item); }
                }

            //_resultText = "";
            //int columnsize = 40;
            //_length = _trues.Count;
            //if (_false.Count > _length) _length = _false.Count;
            //if (_rest.Count > _length) _length = _rest.Count;

            //for (int i = 0; i < _length; i++)
            //{
            //    truesText = "";
            //    falseText = "";
            //    restText = "";
            //    try { 
            //    if (_trues[i] != null) truesText = _trues[i];

            //        while (truesText.Length < columnsize) { truesText += " ";}

            //    if (_false[i] != null) falseText = _false[i];
            //        while (falseText.Length < columnsize) { falseText += " "; }
            //        if (_rest[i] != null) restText = _rest[i];
            //    } catch { 
            //        // ss
            //    }

            //_resultText += truesText + falseText + restText + newline;
            //}
            _resultText = "CONTENT OF SupremacyClient..Settings.xaml "  + DateTime.Now + newline;

            _resultText += newline + "VALUES" + newline + "======" + newline;
            foreach (var item in _rest) { _resultText += item + newline; }

            _resultText += newline + "TRUE" + newline + "====" + newline;
            foreach (var item in _trues) { _resultText += item + newline; }

            _resultText += newline + "FALSE" + newline + "=====" + newline;
            foreach (var item in _false) { _resultText += item + newline; }

            //_resultText += newline + "REST" + newline + "====" + newline;
            //foreach (var item in _rest) { _resultText += item + newline; }
            
            _resultText += newline + newline;

            StreamWriter streamWriter = new StreamWriter(file+".txt");
                streamWriter.Write(_resultText);
                streamWriter.Close();

                var _file = Path.Combine(ResourceManager.GetResourcePath(""), file+ ".txt");
                if (!string.IsNullOrEmpty(_file) && File.Exists(_file))
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo { UseShellExecute = true, FileName = _file };

                    try { _ = Process.Start(processStartInfo); }
                    catch { MessageBox.Show("Could not load Text-File about Settings"); }
                }


                //var result = MessageDialog.Show(_resultText, MessageDialogButtons.YesNo);
                //MessageBox.Show(_resultText);
                //MessageBox.Show(_trueText);
                //MessageBox.Show(_falseText);
                //MessageBox.Show(_restText);
            
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
            _soundPlayer.PlayFile("Resources/SoundFX/MenuScreen.ogg");
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
            _container.RegisterType<IGameController, GameController>(new TransientLifetimeManager());
            //_container.RegisterInstance<IScriptService>(new ScriptService());*/

            _container.RegisterType<StatusWindow>(new ContainerControlledLifetimeManager());
            _container.RegisterInstance(new CombatWindow());

            _container.RegisterType<GalaxyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ColonyScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<DiplomacyScreenViewModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ScienceScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<AssetsScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<EncyclopediaScreenPresentationModel>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<SystemAssaultScreenViewModel>(new ContainerControlledLifetimeManager());

            //_container.RegisterType<ISinglePlayerScreen, SinglePlayerScreen>(new ExternallyControlledLifetimeManager());

            _container.RegisterType<IGalaxyScreenView, GalaxyScreenView>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IColonyScreenView, ColonyScreenView>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<INewDiplomacyScreenView, NewDiplomacyScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IScienceScreenView, ResearchScreen>(new ExternallyControlledLifetimeManager());
           // _container.RegisterType<IIntelScreenView, IntelScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IAssetsScreenView, AssetsScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IEncyclopediaScreenView, EncyclopediaScreen>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<ILobbyScreenView, MultiplayerLobby>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISystemAssaultScreenView, SystemAssaultScreen>(new ContainerControlledLifetimeManager());

            _container.RegisterType<IGalaxyScreenPresenter, GalaxyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IColonyScreenPresenter, ColonyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IDiplomacyScreenPresenter, DiplomacyScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IScienceScreenPresenter, ScienceScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IAssetsScreenPresenter, AssetsScreenPresenter>(new ExternallyControlledLifetimeManager());
            //_container.RegisterType<IScienceScreenPresenter, EncyclodepiaScreenPresenter>(new ExternallyControlledLifetimeManager());
            _container.RegisterType<IEncyclopediaScreenPresenter, EncyclopediaScreenPresenter>(new ExternallyControlledLifetimeManager());

            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MenuScreen, typeof(MenuScreen));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.MultiplayerLobby, typeof(ILobbyScreenView));

            // first is first shown in Options
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(SecondOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(TracesOptionsPage));   // moved into own Dialog
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.TracesPages, typeof(TracesOptionsPage));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.FakeDialog, typeof(FakeDialog));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AudioOptionsPage));   // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GraphicsOptionsPage));  // remove outcomment to be shown again
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(GeneralOptionsPage));
            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.OptionsPages, typeof(AllOptionsPage));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_1));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_2));
            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.f10_Pages, typeof(F10_Tab_3));

            // _regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.SpyList, typeof(SpyListView)); // keep it simple for now
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.EmpireOverview, typeof(EmpireInfoView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.EmpireResources, typeof(EmpireResourcesView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.GalaxyGrid, typeof(GalaxyGridView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.TradeRouteList, typeof(TradeRouteListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.TaskForceList, typeof(TaskForceListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.AssignedShipList, typeof(AssignedShipListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.AvailableShipList, typeof(AvailableShipListView));
            //_regionViewRegistry.RegisterViewWithRegion(AssetsScreenRegions.ShipStats, typeof(ShipInfoPanel));

            _regionViewRegistry.RegisterViewWithRegion(CommonGameScreenRegions.PlanetsView, typeof(StarSystemPanel));
           // _regionViewRegistry.RegisterViewWithRegion(CommonGameScreenRegions.SpyListView, typeof(SpyListView));

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
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildList, ColonyScreenRegions.ShipyardBuildList, typeof(ColonyShipyardBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, ColonyScreenRegions.SelectedPlanetaryBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildQueue, ColonyScreenRegions.ShipyardBuildQueue, typeof(ColonyShipyardBuildQueueView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.ShipyardBuildList, ColonyScreenRegions.ShipyardBuildList, typeof(ColonyShipyardBuildListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.SelectedShipyardBuildProjectInfo, ColonyScreenRegions.SelectedShipyardBuildProjectInfo, typeof(ColonyBuildProjectInfoView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.StructureList, ColonyScreenRegions.StructureList, typeof(ColonyStructureListView));
            _regionViewRegistry.RegisterViewWithRegion(ColonyScreenRegions.HandlingList, ColonyScreenRegions.HandlingList, typeof(ColonyHandlingListView));
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
            statusWindow.Header = " ***     Loading Game . . .      ***  "; // +Environment.NewLine;


            statusWindow.Content = Environment.NewLine
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "For more information on game play please read the manual."
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "Star Trek and all related marks, logos and characters are solely owned by CBS Studios Inc."
            + Environment.NewLine + "This fan production is not endorsed by, sponsored by, nor affiliated with CBS, Paramount Pictures, or"
            + Environment.NewLine + "any other Star Trek franchise, and is a non-commercial fan-made game intended for recreational use."
            + Environment.NewLine + "No commercial exhibition or distribution is permitted. No alleged independent rights will be asserted"
            + Environment.NewLine + "against CBS or Paramount Pictures."
            + Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------"
            + Environment.NewLine + "This work is licensed under the Creative Commons"
            + Environment.NewLine + "Attribution - NonCommercial - ShareAlike 4.0 International (CC BY - NC - SA 4.0)"
            ;

            //string techlvl = "3";
            //var options = localEmpireID;
            //string techlvl = startTechLvl;
            //string techlvl = _appContext.LobbyData.GameOptions.StartingTechLevel.ToString();
            //string empireID = _appContext.LocalPlayerEmpire.Civilization.Key.Substring(3, 0);

            string introTextCase;  // SinglePlayerGame working
            //string introTextCase = "empty_introTextCase";


            introTextCase = localEmpire + startTechLvl;  // startTechLvl = -1 shown
            if (startTechLvl == -1)
                introTextCase = _resourceManager.GetString("GAME_START_INFO_LOADING_GAME");
                    //"...history from the saved game continues ... let's see what the future will bring...";

            try
            {
                if (_appContext.RemotePlayers != null)
                    introTextCase = _resourceManager.GetString("GAME_START_INFO_MP_JOINER_LOADING_GAME");
                //"...Competition to Supremacy of Galaxy begins... join and let your empire raise ...";
            } catch { }

            if (_appContext.IsGameHost == true)
                introTextCase = _resourceManager.GetString("GAME_START_INFO_MP_HOSTER_LOADING_GAME");
            //"...Competition to Supremacy of Galaxy begins... let your empire raise and lead others ...";



            GameLog.Client.GameInitData.DebugFormat("introTextCase = {0}", introTextCase);
            //string introTextCase = "FED1"; 
            string introText = Environment.NewLine;
            //+ "----------------------------------------------------------------------------------------------------------------------------------------------"
            //+ Environment.NewLine;
            try
            {
                introText += _resourceManager.GetString(introTextCase);
            } catch { introText = "";  }

            statusWindow.Content = introText + statusWindow.Content + Environment.NewLine;



            // Hints screen will not show for host of a multiplayer game so is excluded here, the host cannot progress to the loaded game.
            // to do line 425 to 435 add hint for people new to game

            //if (_appContext.IsSinglePlayerGame == false)   // see below, depending on Length out of en.txt or later on OPTION
            {
                var _hints = _resourceManager.GetString("LOADING_GAME_HINTS");
                
                if (_hints.Length > 0)   // later: make additional OPTION to show hints or not
                {
                    _ = MessageDialog.Show(statusWindow.Content = _resourceManager.GetString("LOADING_GAME_HINTS"),
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
            _tracesCommand.IsActive = true;
            _f06_Command.IsActive = true;
            _f07_Command.IsActive = true;
            _f08_Command.IsActive = true;
            _f09_Command.IsActive = true;
            _f10_Command.IsActive = true;
            _f11_Command.IsActive = true;
            _f12_Command.IsActive = true;
            _s0_Command.IsActive = true;
            _s1_Command.IsActive = true;
            _s2_Command.IsActive = true;
            _s3_Command.IsActive = true;
            _s4_Command.IsActive = true;
            _s5_Command.IsActive = true;
            _s6_Command.IsActive = true;
            _fakeCommand.IsActive = true;
            _logTxtCommand.IsActive = true;
            _errorTxtCommand.IsActive = true;
            _showCreditsDialogCommand.IsActive = true;
            _showSettingsFileCommand.IsActive = true;
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
                if (_appContext.LocalPlayer.Empire.Key == "FEDERATION")
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
                {
                    MessageBox.Show("Empire is set to NOT-Playable - falling back to Default - Please restart, Select Single Player Menu and set Empire Playable to YES");
                    LoadDefaultTheme();
                }
                    
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

            ThemeShipyard = theme;

            _app.LoadThemeResourcesShipyard(ThemeShipyard);

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
            ClientCommands.TracesCommand.RegisterCommand(_tracesCommand);
            ClientCommands.F06_Command.RegisterCommand(_f06_Command);
            ClientCommands.F07_Command.RegisterCommand(_f07_Command);
            ClientCommands.F08_Command.RegisterCommand(_f08_Command);
            ClientCommands.F09_Command.RegisterCommand(_f09_Command);
            ClientCommands.F10_Command.RegisterCommand(_f10_Command);
            ClientCommands.F11_Command.RegisterCommand(_f11_Command);
            ClientCommands.F12_Command.RegisterCommand(_f12_Command);

            ClientCommands.S0_Command.RegisterCommand(_s0_Command);
            ClientCommands.S1_Command.RegisterCommand(_s1_Command);
            ClientCommands.S2_Command.RegisterCommand(_s2_Command);
            ClientCommands.S3_Command.RegisterCommand(_s3_Command);
            ClientCommands.S4_Command.RegisterCommand(_s4_Command);
            ClientCommands.S5_Command.RegisterCommand(_s5_Command);
            ClientCommands.S6_Command.RegisterCommand(_s6_Command);

            ClientCommands.FakeCommand.RegisterCommand(_fakeCommand);
            ClientCommands.LogTxtCommand.RegisterCommand(_logTxtCommand);
            ClientCommands.ErrorTxtCommand.RegisterCommand(_errorTxtCommand);
            ClientCommands.StartSinglePlayerGame.RegisterCommand(_startSinglePlayerGameCommand);
            ClientCommands.ContinueGame.RegisterCommand(_continueGameCommand);
            ClientCommands.EndGame.RegisterCommand(_endGameCommand);
            ClientCommands.JoinMultiplayerGame.RegisterCommand(_joinMultiplayerGameCommand);
            ClientCommands.HostMultiplayerGame.RegisterCommand(_hostMultiplayerGameCommand);
            ClientCommands.LoadGame.RegisterCommand(_loadGameCommand);
            ClientCommands.ShowCreditsDialog.RegisterCommand(_showCreditsDialogCommand);
            ClientCommands.ShowSettingsFileCommand.RegisterCommand(_showSettingsFileCommand);
            ClientCommands.Exit.RegisterCommand(_exitCommand);
        }
        private void ExecuteSP_DirectlyGameCommand(int _id)
        {
            if (_appContext.IsGameInPlay) return;

            var startScreen = new SinglePlayerStartScreen(_soundPlayer);
            var options = startScreen.Options;

            switch (_id)
            {
                case 0: options.FederationPlayable = EmpirePlayable.Yes; break;
                case 1: options.TerranEmpirePlayable = EmpirePlayable.Yes; break;
                case 2: options.RomulanPlayable = EmpirePlayable.Yes; break;
                case 3: options.KlingonPlayable = EmpirePlayable.Yes; break;
                case 4: options.CardassianPlayable = EmpirePlayable.Yes; break;
                case 5: options.DominionPlayable = EmpirePlayable.Yes; break;
                case 6: options.BorgPlayable = EmpirePlayable.Yes; break;
                default:
                    break;
            }
            

            var initData = GameInitData.CreateSinglePlayerGame(startScreen.Options, _id);
            localEmpire = GetLocalEmpireShortage(_id, out string localempire);
            startTechLvl = GetStartTechLvl(startScreen.Options.StartingTechLevel.ToString());

            RunGameController(gameController => gameController.RunLocal(initData), false);
        }

        private void ExecuteStartSinglePlayerGameCommand(object parameter)
        {
            if (Interlocked.CompareExchange(ref _gameController, null, null) != null)
                return;

            LoadDefaultTheme();

            var startScreen = new SinglePlayerStartScreen(_soundPlayer);
            var options = startScreen.Options;

            var dialogResult = startScreen.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
                return;

            var initData = GameInitData.CreateSinglePlayerGame(options, startScreen.EmpireID);

            localEmpire = GetLocalEmpireShortage(startScreen.EmpireID, out string localempire);
            startTechLvl = GetStartTechLvl(startScreen.Options.StartingTechLevel.ToString());

            RunGameController(gameController => gameController.RunLocal(initData), false);

            // activate following for switching to using MP-Screen as well for SP
            //var initData = GameInitData.CreateMultiplayerGame(GameOptionsManager.LoadDefaults(), "LOCAL PLAYER");
            //RunGameController(gameController => gameController.RunLocal(initData), true);
        }

        private int GetStartTechLvl(string startTechLvlText)
        {
            switch (startTechLvlText)
            {
                case "Early": startTechLvl = 1; break;
                case "Developed": startTechLvl = 2; break;
                case "Sophisticated": startTechLvl = 3; break;
                case "Advanced": startTechLvl = 4; break;
                case "Supreme": startTechLvl = 5; break;
                default:
                    startTechLvl = 1;
                    break;
            }
            return startTechLvl;
        }

        private string GetLocalEmpireShortage(int empireID, out string localEmpire)
        {
            switch (empireID)
            {
                case 0: localEmpire = "FED"; break;
                case 1: localEmpire = "TER"; break;
                case 2: localEmpire = "ROM"; break;
                case 3: localEmpire = "KLI"; break;
                case 4: localEmpire = "CAR"; break;
                case 5: localEmpire = "DOM"; break;
                case 6: localEmpire = "BOR"; break;
                default:
                    localEmpire = "FED";
                    break;
            }
            return localEmpire;
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

            //ShowLoadingScreen();  // additional showing

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
            if (!(sender is IGameController gameController))
                return;
            gameController.Terminated -= OnGameControllerTerminated;
            Interlocked.CompareExchange(ref _gameController, null, gameController);
            _app.DoEvents();
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            UpdateCommands();
        }

        public string ThemeShipyard { get; set; }
    }
}