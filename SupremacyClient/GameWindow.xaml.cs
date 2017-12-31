//// GameWindow.xaml.cs
////
//// Copyright (c) 2007-2009 Mike Strobel
////
//// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
//// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
////
//// All other rights reserved.

//using System;
//using System.ComponentModel;
//using System.Configuration;
//using System.Diagnostics;
//using System.IO;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Threading;

//using Microsoft.Practices.Composite.Presentation.Commands;
//using Microsoft.Practices.Composite.Presentation.Events;
//using Microsoft.Practices.Composite.Regions;
//using Microsoft.Practices.Unity;

//using Supremacy.Annotations;
//using Supremacy.Client.Commands;
//using Supremacy.Client.Dialogs;
//using Supremacy.Client.Events;
//using Supremacy.Client.Properties;
//using Supremacy.Client.Services;
//using Supremacy.Client.Views;
//using Supremacy.Game;
//using Supremacy.Types;
//using Supremacy.UI;

//using Wintellect;

//using System.Linq;

//namespace Supremacy.Client
//{
//    /// <summary>
//    /// Interaction logic for GameWindow.xaml
//    /// </summary>
//    [Obsolete("Replaced by ClientWindow.")]
//    public sealed partial class GameWindow : IGameWindow
//    {
//        #region Static Members
//        public static readonly RoutedCommand EscapeCommand;
//        public static readonly RoutedCommand ConsoleCommand;

//        private static GameWindow s_current;
//        private static Cursor s_defaultCursor;

//        public static GameWindow Current
//        {
//            get
//            {
//                if (s_current == null)
//                    s_current = Application.Current.MainWindow as GameWindow;
//                return s_current;
//            }
//            private set { s_current = value; }
//        }

//        static GameWindow()
//        {
//            EscapeCommand = new RoutedCommand("Escape", typeof(GameWindow));
//            ConsoleCommand = new RoutedCommand("Console", typeof(GameWindow));
//            CursorProperty.OverrideMetadata(
//                typeof(Window),
//                new FrameworkPropertyMetadata(
//                    new Cursor(Environment.CurrentDirectory + @"\Resources\Cursors\cursor.cur")));
//        }
//        #endregion

//        #region Constants
//        private const double MaxScreenWidth = 1600;
//        #endregion

//        #region Fields

//        private readonly object _waitCursorLock = new object();

//        private readonly IRegionManager _regionManager;
//        private readonly IRegionViewRegistry _regionViewRegistry;
//        private readonly INavigationService _navigationService;
//        //private readonly ClientSettingsWindow _settingsWindow;
//        private readonly LoadGameDialog _loadGameWindow;
//        private readonly SaveGameDialog _saveGameWindow;
//        private readonly StatusWindow _statusWindow;
//        private readonly MultiplayerSetupScreen _mpSetupScreen;
//        private readonly INavigationCommandsProxy _navigationCommands;

//        private readonly DelegateCommand<object> _endTurnCommand;
//        private readonly DelegateCommand<TargetSelectionArgs> _showTargetSelectionDialogCommand;

//#pragma warning disable 219
//        private CombatWindow _combatWindow;
//#pragma warning restore 219
//        private int _waitCursorCount;
//        private bool _settingsLoaded;
//        private Window _summaryWindow;
//        #endregion

//        #region Properties
//        public IUnityContainer Container { get; private set; }
//        public MenuScreen MenuScreen { get; private set; }
//        public GalaxyScreenView GalaxyScreen { get; private set; }
//        //public SystemScreen SystemScreen { get; private set; }
//        public ResearchScreen ResearchScreen { get; private set; }
//        public AffairsScreen AffairsScreen { get; private set; }
//        public DiplomacyScreen DiplomacyScreen { get; private set; }

//        #endregion

//        #region Constructors
//        public GameWindow(
//            [NotNull] IUnityContainer container,
//            [NotNull] IRegionManager regionManager,
//            [NotNull] IRegionViewRegistry regionViewRegistry,
//            [NotNull] INavigationService navigationService,
//            [NotNull] INavigationCommandsProxy navigationCommands)
//        {
//            if (container == null)
//                throw new ArgumentNullException("container");
//            if (regionManager == null)
//                throw new ArgumentNullException("regionManager");
//            if (regionViewRegistry == null)
//                throw new ArgumentNullException("regionViewRegistry");
//            if (navigationService == null)
//                throw new ArgumentNullException("navigationService");
//            if (navigationCommands == null)
//                throw new ArgumentNullException("navigationCommands");

//            _regionManager = regionManager;
//            _regionViewRegistry = regionViewRegistry;
//            _navigationService = navigationService;
//            _navigationCommands = navigationCommands;

//            Container = container;
//            Current = this;

//            _waitCursorCount = 0;
//            PresentationTraceSources.SetTraceLevel(this, ClientApp.CmdLineArgs.TraceLevel);

//            InitializeComponent();

//            //_screenStack = new GameScreenStack();
//            //_screenStack.CurrentScreenChanged += ScreenStack_CurrentScreenChanged;

//            //RegionManager.SetRegionName(_screenStack, ClientRegions.GameScreens);

//            //_mpSetupScreen = new MultiplayerSetupScreen();
//            //_statusWindow = new StatusWindow();
//            //_combatWindow = new CombatWindow();
//            //_settingsWindow = new ClientSettingsWindow();
//            //_loadGameWindow = new LoadGameDialog();
//            //_saveGameWindow = new SaveGameDialog();

//            //MenuScreen = new MenuScreen();
//            //_screenStack.AddScreen(MenuScreen);
//            _regionViewRegistry.RegisterViewWithRegion(
//                ClientRegions.GameScreens,
//                StandardGameScreens.MenuScreen,
//                () => MenuScreen);
//            //_screenStack.FallbackScreen = MenuScreen;
//            //_screenStack.CurrentScreen = MenuScreen;

//            this.Loaded += GameWindow_Loaded;
//            this.Closing += GameWindow_Closing;
//            this.Closed += GameWindow_Closed;
//            this.SizeChanged += GameWindow_SizeChanged;
//            this.LocationChanged += GameWindow_LocationChanged;

//            #region Composite Events
//            _endTurnCommand = new DelegateCommand<object>(
//                GalaxyScreen_TurnCommand_Executed,
//                GalaxyScreen_TurnCommand_CanExecute);
//            ClientCommands.EndTurn.RegisterCommand(_endTurnCommand);

//            _showTargetSelectionDialogCommand = new DelegateCommand<TargetSelectionArgs>(
//                ExecuteShowTargetSelectionDialogCommand);
//            GalaxyScreenCommands.ShowTargetSelectionDialog.RegisterCommand(_showTargetSelectionDialogCommand);

//            ClientEvents.TurnStarted.Subscribe(
//                args => _endTurnCommand.IsActive = true,
//                ThreadOption.UIThread);

//            ClientEvents.TurnEnded.Subscribe(
//                gameContext => _endTurnCommand.IsActive = false,
//                ThreadOption.UIThread);

//            //_navigationCommands.NavigateToColony.RegisterCommand(
//            //    new DelegateCommand<Colony>(
//            //        delegate(Colony colony)
//            //        {
//            //            if ((colony == null) || !Equals(colony.OwnerID, this.AppContext.LocalPlayer.LocalPlayerEmpireID))
//            //                return;
//            //            this.SystemScreen.Colony = colony;
//            //            _regionManager.Regions[ClientRegions.GameScreens].Activate(this.SystemScreen);
//            //        }));

//            if (s_defaultCursor == null)
//            {
//                s_defaultCursor = new Cursor(
//                    Path.Combine(
//                        Environment.CurrentDirectory,
//                        @"Resources\Cursors\cursor.cur"));
//            }
//            #endregion

//            this.Cursor = s_defaultCursor;

//            #region Game Window Command Bindings
//            CommandBindings.Add(
//                new CommandBinding(
//                    EscapeCommand,
//                    GameWindow_EscapeCommand_Executed));
//            CommandBindings.Add(
//                new CommandBinding(
//                    ConsoleCommand,
//                    GameWindow_ConsoleCommand_Executed));
//            #endregion

//            #region Context Menu Command Bindings
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.MainCommand,
//                    GameContextMenu_MainCommand_Executed,
//                    GameContextMenu_MainCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.SystemCommand,
//                    GameContextMenu_SystemCommand_Executed,
//                    GameContextMenu_SystemCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.ScienceCommand,
//                    GameContextMenu_ScienceCommand_Executed,
//                    GameContextMenu_ScienceCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.DiplomacyCommand,
//                    GameContextMenu_DiplomacyCommand_Executed,
//                    GameContextMenu_DiplomacyCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.MenuCommand,
//                    GameContextMenu_MenuCommand_Executed,
//                    GameContextMenu_MenuCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameContextMenu.AffairsCommand,
//                    GameContextMenu_AffairsCommand_Executed,
//                    GameContextMenu_AffairsCommand_CanExecute));
//            #endregion

//            #region Menu Screen Command Bindings
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.SinglePlayerCommand,
//                    MenuScreen_SinglePlayerCommand_Executed,
//                    MenuScreen_SinglePlayerCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.MultiplayerCommand,
//                    MenuScreen_MultiplayerCommand_Executed,
//                    MenuScreen_MultiplayerCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.ContinueCommand,
//                    MenuScreen_ContinueCommand_Executed,
//                    MenuScreen_ContinueCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.OptionsCommand,
//                    MenuScreen_OptionsCommand_Executed,
//                    MenuScreen_OptionsCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.CreditsCommand,
//                    MenuScreen_CreditsCommand_Executed,
//                    MenuScreen_CreditsCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.LoadGame,
//                    MenuScreen_LoadGameCommand_Executed,
//                    MenuScreen_LoadGameCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.SaveGame,
//                    MenuScreen_SaveGameCommand_Executed,
//                    MenuScreen_SaveGameCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.RetireCommand,
//                    MenuScreen_RetireCommand_Executed,
//                    MenuScreen_RetireCommand_CanExecute));
//            CommandBindings.Add(
//                new CommandBinding(
//                    MenuScreen.ExitCommand,
//                    MenuScreen_ExitCommand_Executed,
//                    MenuScreen_ExitCommand_CanExecute));
//            #endregion

//            #region Galaxy Screen Command Bindings
//            //CommandBindings.Add(new CommandBinding(
//            //                        Client.GalaxyScreen.TurnCommand,
//            //                        GalaxyScreen_TurnCommand_Executed,
//            //                        GalaxyScreen_TurnCommand_CanExecute));
//            //ClientCommands.EndTurn.RegisterCommand(Client.GalaxyScreen.TurnCommand);
//            CommandBindings.Add(
//                new CommandBinding(
//                    ClientCommands.ShowEndOfTurnSummary,
//                    GalaxyScreen_SummaryCommand_Executed,
//                    GalaxyScreen_SummaryCommand_CanExecute));
//            #endregion

//            #region Single Player Game Command Bindings
//            CommandBindings.Add(
//                new CommandBinding(
//                    GameCommands.BeginSinglePlayerGame,
//                    GameCommands_BeginSinglePlayerGameCommand_Executed));

//            CommandBindings.Add(
//                new CommandBinding(
//                    GameCommands.SaveGame,
//                    GameCommands_SaveSinglePlayerGameCommand_Executed,
//                    GameCommands_SaveSinglePlayerGameCommand_CanExecute));

//            CommandBindings.Add(
//                new CommandBinding(
//                    GameCommands.LoadGame,
//                    GameCommands_LoadSinglePlayerGameCommand_Executed));
//            #endregion

//            #region Keyboard Input Bindings
//            InputBindings.Add(
//                new KeyBinding(
//                    _navigationCommands.ActivateScreen,
//                    new KeyGesture(Key.F1, ModifierKeys.None))
//                {
//                    CommandParameter = StandardGameScreens.GalaxyScreen
//                });
//            InputBindings.Add(
//                new KeyBinding(
//                    _navigationCommands.ActivateScreen,
//                    new KeyGesture(Key.F2, ModifierKeys.None))
//                {
//                    CommandParameter = StandardGameScreens.ColonyScreen
//                });
//            InputBindings.Add(
//                new KeyBinding(
//                    _navigationCommands.ActivateScreen,
//                    new KeyGesture(Key.F3, ModifierKeys.None))
//                {
//                    CommandParameter = StandardGameScreens.DiplomacyScreen
//                });
//            InputBindings.Add(
//                new KeyBinding(
//                    _navigationCommands.ActivateScreen,
//                    new KeyGesture(Key.F4, ModifierKeys.None))
//                {
//                    CommandParameter = StandardGameScreens.ScienceScreen
//                });
//            InputBindings.Add(
//                new KeyBinding(
//                    _navigationCommands.ActivateScreen,
//                    new KeyGesture(Key.F5, ModifierKeys.None))
//                {
//                    CommandParameter = StandardGameScreens.PersonnelScreen
//                });
//            InputBindings.Add(
//                new KeyBinding(
//                    EscapeCommand,
//                    new KeyGesture(Key.Escape, ModifierKeys.None)));
//            InputBindings.Add(
//                new KeyBinding(
//                    ConsoleCommand,
//                    new KeyGesture(Key.OemTilde)));
//            #endregion

//            //PresentationTraceSources.SetTraceLevel(_settingsWindow, PresentationTraceLevel.High);

//            this.ContextMenu = new GameContextMenu
//                               {
//                                   CustomPopupPlacementCallback = ContextMenuPlacementCallback
//                               };

//            Settings.Default.SettingsLoaded += SettingsLoaded;
//            Settings.Default.SettingsSaving += SettingsSaving;

//            Settings.Default.Reload();

//            SetFullScreenMode(Settings.Default.FullScreenMode);

//            base.Width = Settings.Default.WindowWidth;
//            base.Height = Settings.Default.WindowHeight;
//            base.Left = ((SystemParameters.WorkArea.Width - base.Width) / 2);
//            base.Top = ((SystemParameters.WorkArea.Height - base.Height) / 2);

//            //base.Content = _screenStack;

//            StarSystemPanel.PreloadImages();
//        }

//        private static void ExecuteShowTargetSelectionDialogCommand(TargetSelectionArgs args)
//        {
//            args.Result = TargetSelectionDialog.Show(
//                args.TargetList.Cast<object>(),
//                args.TargetDisplayMember,
//                args.Prompt);
//        }

//        private void GameWindow_LocationChanged(object sender, EventArgs e)
//        {
//            CenterOwnedWindows();
//        }

//        private void CenterOwnedWindows()
//        {
//            foreach (Window window in OwnedWindows)
//            {
//                window.Left = this.Left + (this.ActualWidth - window.ActualWidth) / 2;
//                window.Top = this.Top + (this.ActualHeight - window.ActualHeight) / 2;
//            }
//        }
//        #endregion

//        #region Settings Event Handlers
//        private void SettingsLoaded(object sender, SettingsLoadedEventArgs e)
//        {
//            if (!_settingsLoaded)
//            {
//                _settingsLoaded = true;

//                Dispatcher.Invoke(
//                    DispatcherPriority.Normal,
//                    (Action)ApplySettings);

//                if (Settings.Default.CheckForUpdatesOnStartup && ClientApp.Current.CanCheckForUpdates)
//                    ClientApp.Current.CheckForUpdates();
//            }
//        }

//        private void SettingsSaving(object sender, CancelEventArgs e)
//        {
//            Dispatcher.Invoke(
//                DispatcherPriority.Normal,
//                (Action)ApplySettings);
//        }
//        #endregion

//        #region Single Player Game Command Handlers
//        private void GameCommands_BeginSinglePlayerGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            GameOptions options;
//            if (GameContext.Current != null)
//            {
//                DestroyGameScreens();
//                GameContext.Pop();
//            }
//            if (e.Parameter is GameOptions)
//            {
//                options = (GameOptions)e.Parameter;
//            }
//            else
//            {
//                options = GameOptionsManager.LoadDefaults();
//            }
//            GameContext.Push(GameContext.Create(options));
//            CreateGameScreens();
//            MenuScreen.ContinueCommand.Execute(null, this);
//        }

//        private static void GameCommands_SaveSinglePlayerGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            //this.AppContext.SaveGame(
//            //    (e.Parameter != null)
//            //        ? e.Parameter.ToString()
//            //        : DateTime.Now.ToLongDateString());
//        }

//        private void GameCommands_SaveSinglePlayerGameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (this.AppContext.IsGameInPlay && this.AppContext.IsGameHost);
//        }

//        private void GameCommands_LoadSinglePlayerGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            //var saveGame = e.Parameter as SavedGameHeader;
//            //if (saveGame == null)
//            //    return;
//            //ForceWaitCursor();
//            //HideScreenStack();
//            //InvalidateVisual();
//            //try
//            //{
//            //    Dispatcher.BeginInvoke(
//            //        DispatcherPriority.Normal,
//            //        (Function)(() => this.AppContext.LoadSinglePlayerGame(saveGame.FileName)));
//            //}
//            //catch (Exception ex)
//            //{
//            //    ClearWaitCursor();
//            //    MessageDialog.Show("Error", ex.Message, MessageDialogButton.Ok);
//            //    ShowScreenStack();
//            //}
//        }
//        #endregion

//        #region Window Event Handlers
//        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
//        {
//            base.OnRenderSizeChanged(sizeInfo);

//            if (sizeInfo.NewSize.Width > MaxScreenWidth)
//            {
//                var scale = (sizeInfo.NewSize.Width / MaxScreenWidth);
//                this.LayoutTransform = GameScreensRegion.LayoutTransform = new ScaleTransform(scale, scale);
//            }
//            else
//            {
//                this.LayoutTransform = GameScreensRegion.LayoutTransform = null;
//            }
//        }

//        private void GameWindow_Loaded(object sender, RoutedEventArgs e)
//        {
//#if !NOMUSIC
//            if (!ClientApp.CmdLineArgs.NoMusic)
//            {
//                AudioEngine.Initialize();
//                AudioEngine.LoadMusic(MusicPack.LoadDefault());
//                //SwitchMusicTrack(MusicTrack.TitleScreen);
//                AudioEngine.Start();
//            }
//#endif

//            _mpSetupScreen.Owner = this;
//            //_settingsWindow.Owner = this;
//            _loadGameWindow.Owner = this;
//            _saveGameWindow.Owner = this;

//            ShowMenuScreen();

//            CommandManager.InvalidateRequerySuggested();
//        }

//        private static void GameWindow_Closing(object sender, CancelEventArgs e)
//        {
//            Settings.Default.Save();
//#if !NOMUSIC
//            if (!ClientApp.CmdLineArgs.NoMusic)
//                AudioEngine.Stop();
//#endif
//        }

//        private static void GameWindow_Closed(object sender, EventArgs e)
//        {
//            //try
//            //{
//            //    if (this.AppContext.IsConnected)
//            //        this.AppContext.Disconnect();
//            //}
//            //catch {}
//            //finally
//            //{
//            //    if (!ClientApp.IsShuttingDown)
//            //        ClientApp.Current.Shutdown();
//            //}
//        }

//        private void GameWindow_SizeChanged(object sender, SizeChangedEventArgs e)
//        {
//            if (IsVisible && (WindowState == WindowState.Normal))
//            {
//                if (e.NewSize.Width != Settings.Default.WindowWidth)
//                    Settings.Default.WindowWidth = e.NewSize.Width;
//                if (e.NewSize.Height != Settings.Default.WindowHeight)
//                    Settings.Default.WindowHeight = e.NewSize.Height;
//            }
//            CenterOwnedWindows();
//        }
//        #endregion

//        #region Game Window Command Handlers
//        private void GameWindow_EscapeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            if (_regionManager.Regions[ClientRegions.GameScreens].ActiveViews.Contains(MenuScreen))
//                return;
//            if (!_regionManager.Regions[ClientRegions.GameScreens].ActiveViews.Contains(GalaxyScreen))
//                ShowGalaxyScreen();
//            else
//                ShowMenuScreen();
//            //if (_screenStack.CurrentScreen == MenuScreen)
//            //    return;
//            //if (_screenStack.CurrentScreen != GalaxyScreen)
//            //    ShowGalaxyScreen();
//            //else
//            //    ShowMenuScreen();
//        }

//        private static void GameWindow_ConsoleCommand_Executed(object sender, ExecutedRoutedEventArgs e) {}
//        #endregion

//        #region Game Screen Stack Event Handlers
//        private void ScreenStack_CurrentScreenChanged(object sender, EventArgs e)
//        {
//            HideSummary();
//        }

//        #endregion

//        #region Context Menu Command Handlers
//        private void GameContextMenu_MainCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (GalaxyScreen != null);
//        }

//        public void GameContextMenu_MainCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            if (GalaxyScreen != null)
//                ShowGalaxyScreen();
//        }

//        private static void GameContextMenu_MenuCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = true;
//        }

//        public void GameContextMenu_MenuCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            ShowMenuScreen();
//        }

//        private void GameContextMenu_SystemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            //e.CanExecute = (SystemScreen != null);
//        }

//        private void GameContextMenu_SystemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            //if ((GalaxyScreen == null) || (SystemScreen == null))
//            //    return;
//            //if ((GalaxyScreen.Model.SelectedSector != null)
//            //    && (GalaxyScreen.Model.SelectedSector.System != null)
//            //    && GalaxyScreen.Model.SelectedSector.System.HasColony
//            //    && (GalaxyScreen.Model.SelectedSector.System.Colony.OwnerID.Equals(this.AppContext.LocalPlayerEmpire.CivilizationID)))
//            //{
//            //    SystemScreen.Colony = GalaxyScreen.Model.SelectedSector.System.Colony;
//            //}
//            //else
//            //{
//            //    SystemScreen.Colony = this.AppContext.LocalPlayerEmpire.HomeColony;
//            //}
//            ShowSystemScreen();
//            e.Handled = true;
//        }

//        private void GameContextMenu_ScienceCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (ResearchScreen != null);
//        }

//        private void GameContextMenu_ScienceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            if (ResearchScreen == null)
//                return;
//            ShowResearchScreen();
//            e.Handled = true;
//        }

//        private void GameContextMenu_DiplomacyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (DiplomacyScreen != null);
//        }

//        private void GameContextMenu_DiplomacyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            if (DiplomacyScreen == null)
//                return;
//            ShowDiplomacyScreen();
//            e.Handled = true;
//        }

//        private void GameContextMenu_AffairsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (AffairsScreen != null);
//        }

//        private void GameContextMenu_AffairsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            if (AffairsScreen == null)
//                return;
//            ShowAffairsScreen();
//            e.Handled = true;
//        }
//        #endregion

//        #region Context Menu Placement Callback
//        private CustomPopupPlacement[] ContextMenuPlacementCallback(
//            Size popupSize,
//            Size targetSize,
//            Point offset)
//        {
//            var mouse = Mouse.GetPosition(this);
//            return new[]
//                   {
//                       new CustomPopupPlacement
//                       {
//                           Point = new Point(
//                               Math.Max(
//                                   0,
//                                   Math.Min(
//                                       mouse.X - popupSize.Width / 2,
//                                       this.ActualWidth - popupSize.Width)),
//                               Math.Max(
//                                   0,
//                                   Math.Min(
//                                       mouse.Y - popupSize.Height / 2,
//                                       this.ActualHeight - popupSize.Height)))
//                       }
//                   };
//        }
//        #endregion

//        #region Menu Screen Command Handlers
//        private void MenuScreen_SinglePlayerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = !this.AppContext.IsConnected;
//        }

//        private void MenuScreen_SinglePlayerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            //var startScreen = new SinglePlayerStartScreen { Owner = this };
//            //startScreen.ShowDialog();

//            //if (!startScreen.DialogResult.HasValue || !startScreen.DialogResult.Value)
//            //    return;

//            //ForceWaitCursor();
//            //HideScreenStack();
//            //InvalidateVisual();
           
//            //try
//            //{
//            //    this.AppContext.HostSinglePlayerGame(startScreen.Options, startScreen.LocalPlayerEmpireID);
//            //}
//            //catch (Exception ex)
//            //{
//            //    ClearWaitCursor();
//            //    MessageDialog.Show("Error", ex.Message, MessageDialogButton.Ok);
//            //    ShowScreenStack();
//            //}
//        }

//        private void MenuScreen_MultiplayerCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = !this.AppContext.IsConnected;
//        }

//        private void MenuScreen_MultiplayerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            _mpSetupScreen.ShowDialog();
//        }

//        private void MenuScreen_ContinueCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = this.AppContext.IsGameInPlay;
//        }

//        private void MenuScreen_ContinueCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            ShowGalaxyScreen();
//        }

//        private void MenuScreen_OptionsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = true;
//        }

//        private void MenuScreen_OptionsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            //_settingsWindow.Show();
//        }

//        private void MenuScreen_CreditsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = true;
//        }

//        private static void MenuScreen_CreditsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            var content = Application.LoadComponent(
//                new Uri(
//                    "/SupremacyClient;Component/Resources/Credits.xaml",
//                    UriKind.RelativeOrAbsolute));
//            MessageDialog.Show(
//                content,
//                MessageDialogButtons.Ok);
//        }

//        private void MenuScreen_LoadGameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = !this.AppContext.IsConnected;
//        }

//        private void MenuScreen_LoadGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            _loadGameWindow.Show();
//        }

//        private void MenuScreen_SaveGameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = (this.AppContext.IsGameInPlay && this.AppContext.IsGameHost);
//        }

//        private void MenuScreen_SaveGameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            _saveGameWindow.Show();
//        }

//        private void MenuScreen_RetireCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = this.AppContext.IsGameInPlay;
//        }

//        private static void MenuScreen_RetireCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            ClientCommands.EndGame.Execute(true);
//        }

//        private void MenuScreen_ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            if (IsChildDialogOpen())
//            {
//                e.CanExecute = false;
//                return;
//            }
//            e.CanExecute = true;
//        }

//        private static void MenuScreen_ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            ClientApp.Current.Shutdown();
//        }
//        #endregion

//        #region Galaxy Screen Command Handlers
//        private void GalaxyScreen_TurnCommand_Executed(object parameter)
//        {
//            HideSummary();
//            GalaxyScreen.Model.InputMode = GalaxyScreenInputMode.Normal;
//            if (!_statusWindow.IsVisible)
//                _statusWindow.Show();
//            ClientCommands.EndTurn.Execute(null);
//        }

//        private bool GalaxyScreen_TurnCommand_CanExecute(object parameter)
//        {
//            return this.AppContext.IsGameInPlay && !this.AppContext.IsTurnFinished;
//        }

//        private void GalaxyScreen_SummaryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
//        {
//            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Function(ShowSummary));
//        }

//        private void GalaxyScreen_SummaryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
//        {
//            e.CanExecute = (this.AppContext.IsGameInPlay && !this.AppContext.IsTurnFinished);
//        }
//        #endregion

//        #region Game Screen Initialization
//        private void CreateGameScreens()
//        {
//            //GalaxyScreen = new GalaxyScreen();
//            var presenter = this.Container.Resolve<IGalaxyScreenPresenter>();
//            presenter.Run();
//            //GalaxyScreen = (GalaxyScreenView)presenter.View;
//            ////_screenStack.AddScreen(GalaxyScreen);
//            //SystemScreen = new SystemScreen();
//            ////_screenStack.AddScreen(SystemScreen);
//            //DiplomacyScreen = new DiplomacyScreen();
//            ////_screenStack.AddScreen(DiplomacyScreen);
//            //ResearchScreen = new ResearchScreen();
//            ////_screenStack.AddScreen(ResearchScreen);
//            //AffairsScreen = new AffairsScreen();
//            ////_screenStack.AddScreen(AffairsScreen);

//            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.GalaxyScreen, () => GalaxyScreen);
//            //_regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.ColonyScreen, () => SystemScreen);
//            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.DiplomacyScreen, () => DiplomacyScreen);
//            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.ScienceScreen, () => ResearchScreen);
//            _regionViewRegistry.RegisterViewWithRegion(ClientRegions.GameScreens, StandardGameScreens.PersonnelScreen, () => AffairsScreen);

//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireOverview, typeof(EmpireInfoView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.EmpireResources, typeof(EmpireResourcesView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.GalaxyGrid, typeof(GalaxyGridView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TradeRouteList, typeof(TradeRouteListView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.TaskForceList, typeof(TaskForceListView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AssignedShipList, typeof(AssignedShipListView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.AvailableShipList, typeof(AvailableShipListView));
//            _regionViewRegistry.RegisterViewWithRegion(GalaxyScreenRegions.ShipStats, typeof(ShipInfoPanel));
//        }

//        private void DestroyGameScreens()
//        {
//            if (AffairsScreen != null)
//            {
//                if (_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(AffairsScreen))
//                    _regionManager.Regions[ClientRegions.GameScreens].Remove(AffairsScreen);
//                AffairsScreen.ClearValue(TemplateProperty);
//                AffairsScreen = null;
//            }
//            if (DiplomacyScreen != null)
//            {
//                if (_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(DiplomacyScreen))
//                    _regionManager.Regions[ClientRegions.GameScreens].Remove(DiplomacyScreen);
//                DiplomacyScreen.ClearValue(TemplateProperty);
//                DiplomacyScreen = null;
//            }
//            if (ResearchScreen != null)
//            {
//                if (_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(ResearchScreen)) 
//                    _regionManager.Regions[ClientRegions.GameScreens].Remove(ResearchScreen);
//                ResearchScreen.ClearValue(TemplateProperty);
//                ResearchScreen = null;
//            }
//            //if (SystemScreen != null)
//            //{
//            //    if (_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(SystemScreen)) 
//            //        _regionManager.Regions[ClientRegions.GameScreens].Remove(SystemScreen);
//            //    SystemScreen.ClearValue(TemplateProperty);
//            //    SystemScreen = null;
//            //}
//            if (GalaxyScreen != null)
//            {
//                if (_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(GalaxyScreen)) 
//                    _regionManager.Regions[ClientRegions.GameScreens].Remove(GalaxyScreen);
//                GalaxyScreen.ClearValue(TemplateProperty);
//                GalaxyScreen = null;
//            }
//            ClientApp.ReloadResources();
//        }
//        #endregion

//        #region Screen Control Methods
//        internal void HideScreenStack()
//        {
//            this.GameScreensRegion.Visibility = Visibility.Hidden;
//            //if (_screenStack != null)
//            //    _screenStack.Visibility = Visibility.Hidden;
//        }

//        internal void ShowScreenStack()
//        {
//            if (this.GameScreensRegion.IsVisible)
//                return;
//            this.GameScreensRegion.Visibility = Visibility.Visible;
//            //if (_screenStack == null)
//            //    return;
//            //_screenStack.Visibility = Visibility.Visible;
//            //this.Content = _screenStack;
//        }

//        internal void ShowMenuScreen()
//        {
//            if (MenuScreen == null)
//                return;
//            if (ShowScreen(MenuScreen))
//            {
//                MenuScreen.RefreshScreen();
//                InvalidateVisual();
//                //SwitchMusicTrack(MusicTrack.TitleScreen);
//            }
//        }

//        private bool ShowScreen(Control screen)
//        {
//            if (!_regionManager.Regions[ClientRegions.GameScreens].Views.Contains(screen))
//                return false;
//            _regionManager.Regions[ClientRegions.GameScreens].Activate(screen);
//            return true;
//            //lock (_screenStack)
//            //{
//            //    if (_screenStack.CurrentScreen != null)
//            //    {
//            //        if (!(screen is PriorityGameScreen))
//            //        {
//            //            var args = new CancelEventArgs(false);
//            //            //_screenStack.CurrentScreen.OnClosing(this, args);
//            //            if (args.Cancel)
//            //                return false;
//            //        }
//            //    }
//            //    _screenStack.CurrentScreen = screen;
//            //    CommandManager.InvalidateRequerySuggested();
//            //    return true;
//            //}
//        }

//        internal void ShowGalaxyScreen()
//        {
//            if (GalaxyScreen == null)
//                return;
//            if (_navigationService.ActivateScreen(StandardGameScreens.GalaxyScreen))
//            {
//                //SwitchMusicTrack(MusicTrack.GalaxyScreen);
//            }
//        }

//        internal void ShowSystemScreen()
//        {
//            //if (SystemScreen == null)
//            //    return;
//            //if (!_navigationService.ActivateScreen(StandardGameScreens.ColonyScreen))
//            //    return;
//            //if (SystemScreen.ProductionPanel != null)
//            //    SystemScreen.BringDescendantIntoView(SystemScreen.ProductionPanel);
//            //SwitchMusicTrack(MusicTrack.SystemScreen);
//        }

//        internal void ShowDiplomacyScreen()
//        {
//            if (DiplomacyScreen == null)
//                return;
//            //if (_navigationService.ActivateScreen(StandardGameScreens.DiplomacyScreen))
//            //    SwitchMusicTrack(MusicTrack.DiplomacyScreen);
//        }

//        internal void ShowResearchScreen()
//        {
//            if (ResearchScreen == null)
//                return;
//            //if (_navigationService.ActivateScreen(StandardGameScreens.ScienceScreen))
//            //    SwitchMusicTrack(MusicTrack.ResearchScreen);
//        }

//        internal void ShowAffairsScreen()
//        {
//            if (AffairsScreen == null)
//                return;
//            //if (_navigationService.ActivateScreen(StandardGameScreens.PersonnelScreen))
//            //    SwitchMusicTrack(MusicTrack.IntelligenceScreen);
//        }

//        #endregion

//        #region Mouse Control Methods
//        public void ForceWaitCursor()
//        {
//            lock (_waitCursorLock)
//            {
//                _waitCursorCount++;
//                Mouse.OverrideCursor = Cursors.Wait;
//            }
//            Mouse.UpdateCursor();
//        }

//        public void ClearWaitCursor()
//        {
//            lock (_waitCursorLock)
//            {
//                if (_waitCursorCount > 0)
//                    _waitCursorCount--;
//                if (_waitCursorCount == 0)
//                    Mouse.OverrideCursor = s_defaultCursor;
//            }
//            Mouse.UpdateCursor();
//        }
//        #endregion

//        #region Miscellaneous Methods
//        internal void ShowSummary()
//        {
//            if (!this.AppContext.IsGameInPlay || this.AppContext.IsTurnFinished)
//                return;
//            MessageDialog.Show(
//                "Summary",
//                new SitRepListView(),
//                MessageDialogButtons.Ok);
//        }

//        internal void HideSummary()
//        {
//            if (_summaryWindow == null)
//                return;
//            try
//            {
//                _summaryWindow.Close();
//            }
//            catch {}
//            finally
//            {
//                _summaryWindow = null;
//            }
//        }

//        private void ShowSitRepDetails()
//        {
//            if (this.AppContext.LocalPlayerEmpire.SitRepEntries.Count <= 0)
//                return;
//            var showSummary = false;
//            foreach (var sitRepEntry in this.AppContext.LocalPlayerEmpire.SitRepEntries)
//            {
//                if (sitRepEntry.IsPriority && !sitRepEntry.HasDetails)
//                    showSummary = true;
//                if (sitRepEntry.HasDetails)
//                {
//                    MessageDialog.Show(
//                        null,
//                        sitRepEntry,
//                        MessageDialogButtons.Ok);
//                }
//            }
//            if (showSummary)
//                ShowSummary();
//        }

//        private IAppContext _appContext;
//        private IAppContext AppContext
//        {
//            get
//            {
//                if (_appContext == null)
//                    _appContext = this.Container.Resolve<IAppContext>();
//                return _appContext;
//            }
//        }

//        private void ApplySettings()
//        {
//            Dispatcher.BeginInvoke(
//                DispatcherPriority.Normal,
//                new SetterFunction<bool>(SetFullScreenMode),
//                Settings.Default.FullScreenMode);
         
//            if (!ClientApp.CmdLineArgs.NoMusic)
//                AudioEngine.MaxVolume = (float)Settings.Default.MusicVolume;
            
//            //if (GalaxyScreen != null)
//            //    GalaxyScreen.GalaxyGrid.UseAnimatedStars = Settings.Default.UseAnimatedStars;
            
//            if (Settings.Default.UseHighQualityScaling)
//            {
//                SetValue(
//                    RenderOptions.BitmapScalingModeProperty,
//                    BitmapScalingMode.HighQuality);
//            }
//            else
//            {
//                SetValue(
//                    RenderOptions.BitmapScalingModeProperty,
//                    BitmapScalingMode.LowQuality);
//            }
//        }

//        private void SetFullScreenMode(bool value)
//        {
//            if (value == (base.WindowStyle == WindowStyle.None))
//                return;

//            if (value)
//            {
//                base.ResizeMode = ResizeMode.NoResize;
//                base.WindowStyle = WindowStyle.None;
//                base.WindowState = WindowState.Maximized;
//                base.ClearValue(WidthProperty);
//                base.ClearValue(HeightProperty);
//                base.ClearValue(LeftProperty);
//                base.ClearValue(TopProperty);
//            }
//            else
//            {
//                base.WindowState = WindowState.Normal;
//                base.Width = Settings.Default.WindowWidth;
//                base.Height = Settings.Default.WindowHeight;
//                base.WindowStartupLocation = WindowStartupLocation.CenterScreen;
//                base.WindowStyle = WindowStyle.ThreeDBorderWindow;
//                base.ResizeMode = ResizeMode.CanResize;
//                base.Left = ((SystemParameters.WorkArea.Width - base.ActualWidth) / 2);
//                base.Top = ((SystemParameters.WorkArea.Height - base.ActualHeight) / 2);
//            }

//            foreach (Window ownedWindow in base.OwnedWindows)
//            {
//                if (ownedWindow.IsLoaded)
//                {
//                    ownedWindow.Left = base.Left + ((base.ActualWidth - ownedWindow.ActualWidth) / 2);
//                    ownedWindow.Top = base.Top + ((base.ActualHeight - ownedWindow.ActualHeight) / 2);
//                }
//                else
//                {
//                    ownedWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
//                }
//            }
//        }

//        private bool IsChildDialogOpen()
//        {
//            foreach (Window ownedWindow in base.OwnedWindows)
//            {
//                if (ownedWindow.IsVisible)
//                    return true;
//            }
//            return false;
//        }

//        //private static void SwitchMusicTrack(MusicTrack musicTrack)
//        //{
//        //    if (!ClientApp.CmdLineArgs.NoMusic)
//        //        AudioEngine.SwitchMusicTrack(musicTrack);
//        //}
//        #endregion

//        //private FrameworkElement _lastVisibleScreen;
//        //private void GameScreensRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        //{
//        //    var oldItem = e.RemovedItems.OfType<FrameworkElement>().FirstOrDefault() ?? _lastVisibleScreen;
            
//        //    _lastVisibleScreen = oldItem;

//        //    if (oldItem == null)
//        //        return;

//        //    var newItem = e.AddedItems.OfType<FrameworkElement>().FirstOrDefault();
//        //    if (newItem == null)
//        //        return;

//        //    _lastVisibleScreen = null;

//        //    var brush = new VisualBrush(oldItem)
//        //                {
//        //                    Viewbox = new Rect(0, 0, oldItem.ActualWidth, oldItem.ActualHeight),
//        //                    ViewboxUnits = BrushMappingMode.Absolute,
//        //                    AutoLayoutContent = false
//        //                };

//        //    var da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(2.0)), FillBehavior.HoldEnd)
//        //             {
//        //                 AccelerationRatio = 0.5,
//        //                 DecelerationRatio = 0.5
//        //             };

//        //    da.Completed += delegate { newItem.Effect = null; };

//        //    var effect = new RadialBlurTransitionEffect { OldImage = brush };
//        //    effect.BeginAnimation(TransitionEffect.ProgressProperty, da);

//        //    newItem.Effect = effect;
//        //}

//        #region Implementation of IGameWindow
//        public IDisposable EnterWaitCursorScope()
//        {
//            ForceWaitCursor();
//            return new Disposer(ClearWaitCursor);
//        }
//        #endregion
//    }
//}