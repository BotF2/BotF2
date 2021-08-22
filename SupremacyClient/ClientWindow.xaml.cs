using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Client.Views;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;
using ColorToneEffect = Microsoft.Expression.Media.Effects.ColorToneEffect;
using Disposer = Supremacy.Types.Disposer;
using Expression = System.Linq.Expressions.Expression;
using Path = System.IO.Path;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : IGameWindow
    {
        private const double MaxUnscaledScreenWidth = 1600;

        public static readonly RoutedCommand ToggleFullScreenModeCommand = new RoutedCommand("ToggleFullScreenMode", typeof(ClientWindow));
        public static readonly RoutedCommand CollectGarbageCommand = new RoutedCommand("CollectGarbage", typeof(ClientWindow));

        private static readonly Func<double, double> PixelToPoint;

#pragma warning disable IDE0052 // Remove unread private members
        private readonly IClientApplication _app;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly IAppContext _appContext;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly object _waitCursorLock;
        private readonly Cursor _defaultCursor;
        private readonly StateScope _settingsChangeScope;
        private readonly IAudioEngine _audioEngine;
        private readonly IMusicPlayer _musicPlayer;
        private readonly ISoundPlayer _soundPlayer;

        private int _waitCursorCount;
        private bool _isClosing;
        private bool _exitInProgress;
        private double _scaleFactor;

        static ClientWindow()
        {
            MethodInfo convertPixelMethod = typeof(SystemParameters).GetMethod(
                "ConvertPixel",
                BindingFlags.Static | BindingFlags.NonPublic);

            System.Linq.Expressions.ParameterExpression pixelParameter = Expression.Parameter(typeof(double), "pixel");

            PixelToPoint = Expression.Lambda<Func<double, double>>(
                Expression.Call(
                    convertPixelMethod,
                    Expression.Convert(pixelParameter, typeof(int))),
                pixelParameter).Compile();
        }

        public ClientWindow(
            [NotNull] IClientApplication app,
            [NotNull] IAppContext appContext,
            [NotNull] IAudioEngine audioEngine,
            [NotNull] IMusicPlayer musicPlayer,
            [NotNull] ISoundPlayer soundPlayer,
            [NotNull] IEventAggregator eventAggregator,
            [NotNull] INavigationCommandsProxy navigationCommands)
        {
            _app = app ?? throw new ArgumentNullException("app");
            _appContext = appContext ?? throw new ArgumentNullException("appContext");
            _audioEngine = audioEngine ?? throw new ArgumentNullException("audioEngine");
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException("musicPlayer");
            _soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException("eventAggregator");
            _navigationCommands = navigationCommands ?? throw new ArgumentNullException("navigationCommands");
            _waitCursorLock = new object();
            _settingsChangeScope = new StateScope();

            _defaultCursor = new Cursor(Path.Combine(Environment.CurrentDirectory, @"Resources\Images\UI\Shell\cursor.cur"));


            InitializeComponent();

            /*
             * Officially, we only support video resolutions of 1024x768 and up.  However, considering
             * 1280x720 is one of the standard High Definition resolutions, we will adjust our minimum
             * size constraints to accomodate it.
             */

            if (SystemParameters.PrimaryScreenWidth != 1280d ||
                (SystemParameters.PrimaryScreenHeight != 720d))
            {
                MinHeight = 720;
                Height = 720;
            }


            Cursor = _defaultCursor;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;

            _ = _eventAggregator.GetEvent<TurnStartedEvent>().Subscribe(OnTurnStarted, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<GameStartedEvent>().Subscribe(OnGameStarted, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<GameEndedEvent>().Subscribe(OnGameEnded, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<GameEndingEvent>().Subscribe(OnGameEnding, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<ClientDisconnectedEvent>().Subscribe(OnClientDisconnected, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<GameEndedEvent>().Subscribe(OnGameEnded, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<AllTurnEndedEvent>().Subscribe(OnAllTurnEnded, ThreadOption.UIThread);
            _ = _eventAggregator.GetEvent<ChatMessageReceivedEvent>().Subscribe(OnChatMessageReceived, ThreadOption.UIThread);

            ModelessDialogsRegion.SelectionChanged += OnModelessDialogsRegionSelectionChanged;
            ModalDialogsRegion.SelectionChanged += OnModalDialogsRegionSelectionChanged;

            ClientSettings.Current.EnableAntiAliasingChanged += OnEnableAntiAliasingSettingsChanged;

            ApplyAntiAliasingSettings();

            _ = InputBindings.Add(new KeyBinding(CollectGarbageCommand, new KeyGesture(Key.G, ModifierKeys.Control)));
            _ = InputBindings.Add(new KeyBinding(ToggleFullScreenModeCommand, Key.Enter, ModifierKeys.Alt));


            _ = InputBindings.Add(new KeyBinding(ClientCommands.EscapeCommand, new KeyGesture(Key.Escape, ModifierKeys.None))); // Escape


            _ = InputBindings.Add(new KeyBinding(_navigationCommands.ActivateScreen, new KeyGesture(Key.F1, ModifierKeys.None))
            { CommandParameter = StandardGameScreens.GalaxyScreen });
            _ = InputBindings.Add(new KeyBinding(_navigationCommands.ActivateScreen, new KeyGesture(Key.F2, ModifierKeys.None))
            { CommandParameter = StandardGameScreens.ColonyScreen });
            _ = InputBindings.Add(new KeyBinding(_navigationCommands.ActivateScreen, new KeyGesture(Key.F3, ModifierKeys.None))
            { CommandParameter = StandardGameScreens.ScienceScreen });
            _ = InputBindings.Add(new KeyBinding(_navigationCommands.ActivateScreen, new KeyGesture(Key.F4, ModifierKeys.None))
            { CommandParameter = StandardGameScreens.DiplomacyScreen });
            _ = InputBindings.Add(new KeyBinding(_navigationCommands.ActivateScreen, new KeyGesture(Key.F5, ModifierKeys.None))
            { CommandParameter = StandardGameScreens.IntelScreen });

            _ = InputBindings.Add(new KeyBinding(ClientCommands.F06_Command, Key.F6, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F07_Command, Key.F7, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F08_Command, Key.F8, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F09_Command, Key.F9, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F10_Command, Key.F10, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F11_Command, Key.F11, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.F12_Command, Key.F12, ModifierKeys.None));// new for lift Fog of War  = ALT + F

            
                        //                                                      CheatMenu uses CTRL + C

            _ = InputBindings.Add(new KeyBinding(ClientCommands.ErrorTxtCommand, Key.E, ModifierKeys.Control));
            //InputBindings.Add(new KeyBinding(CollectGarbageCommand, new KeyGesture(Key.G, ModifierKeys.Control))); - see above - from Mike
            _ = InputBindings.Add(new KeyBinding(ClientCommands.ShowPlayersHistoryFileCommand, Key.H, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.ShowEndOfTurnSummary, Key.I, ModifierKeys.Control));
            //InputBindings.Add(new KeyBinding(ClientCommands.ShowShipOverview, Key.D1, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.LogTxtCommand, Key.L, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.OptionsCommand, Key.O, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.TracesCommand, Key.P, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.SaveGame, Key.S, ModifierKeys.Control)); // CRTL+S makes saved file "_manual_save"
            //_ = InputBindings.Add(new KeyBinding(ClientCommands.SaveGameDeleteManualSaved, Key.S, ModifierKeys.Alt)); // deletes file from CRTL+S "_manual_save"
            //InputBindings.Add(new KeyBinding(ClientCommands.ShowSaveGameDialog, Key.S, ModifierKeys.Alt));// ALT+S shows SaveGameDialog    // does not work yet
            _ = InputBindings.Add(new KeyBinding(ClientCommands.EndTurn, Key.T, ModifierKeys.Control));

            _ = InputBindings.Add(new KeyBinding(ClientCommands.FakeCommand, Key.F11, ModifierKeys.Control));


            // Modifier "Shift" - not supported by some unknown reason


            // Modifier "ALT"
            _ = InputBindings.Add(new KeyBinding(ClientCommands.AutoTurnCommand, Key.A, ModifierKeys.Alt));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.ShowAllHistoryFileCommand, Key.H, ModifierKeys.Alt));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.SaveGameDeleteManualSaved, Key.S, ModifierKeys.Alt)); // deletes file from CRTL+S "_manual_save"

            _ = InputBindings.Add(new KeyBinding(ClientCommands.ShowSettingsFileCommand, Key.X, ModifierKeys.Alt));

            //CommandBindings
            _ = CommandBindings.Add(
                new CommandBinding(
                    ClientCommands.EscapeCommand,
                    ExecuteEscapeCommand));

            _ = CommandBindings.Add(
                new CommandBinding(
                    ToggleFullScreenModeCommand,
                    (s, e) => ToggleFullScreenMode()));

            _ = CommandBindings.Add(
                new CommandBinding(
                    CollectGarbageCommand,
                    (s, e) =>
                    {
                        Process process = Process.GetCurrentProcess();
                        long workingSet = process.WorkingSet64;
                        long heapSize = GC.GetTotalMemory(false);

                        string _text = "Forcing garbage collection...";
                        //Console.WriteLine(_text); GameLog.Client.General.Info(_text);

                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();

                        process.Refresh();

                        _text = "Garbage Collection: Process= " + process.ProcessName
                            + ", ProcessID= " + process.Id
                            + ", Threads= " + process.Threads.Count.ToString()
                            + ", Memory= " + process.PagedMemorySize64
                            //+ ", Memory= " + process.PagedSystemMemorySize64
                            //+ ", Memory= " + process.PrivateMemorySize64
                            //+ ", Memory= " + process.VirtualMemorySize64
                            //+ ", TIMEtotal= " + process.TotalProcessorTime
                            //+ ", Memory= " + process.VirtualMemorySize64

                            ;
                        //works  MessageBox.Show(_text, process.ProcessName + " ID: " + process.Id);
                        Console.WriteLine(_text); GameLog.Client.General.Info(_text);  // Garbage Collection

                        // old code
                        GameLog.Client.General.InfoFormat(
                            "[working set [{0:#,#,} K -> {1:#,#,} K], managed heap [{2:#,#,} K -> {3:#,#,} K]]",
                            workingSet,
                            process.WorkingSet64,
                            heapSize,
                            GC.GetTotalMemory(false));

                        GameLog.Client.General.DebugFormat(
                            "[working set [{0:#,#,} K -> {1:#,#,} K], managed heap [{2:#,#,} K -> {3:#,#,} K]]",
                            workingSet,
                            process.WorkingSet64,
                            heapSize,
                            GC.GetTotalMemory(false));
                    }));

            _ = CommandBindings.Add(
                new CommandBinding(
                    ClientCommands.AutoTurnCommand,
                    (s, e) =>
                    {
                        IPlayerOrderService service = ServiceLocator.Current.GetInstance<IPlayerOrderService>();
                        service.AutoTurn = !service.AutoTurn;
                        if (service.AutoTurn)
                        {
                            ClientCommands.EndTurn.Execute(null);
                        }
                    }));

            ClientSettings settings = ClientSettings.Current;

            Width = settings.ClientWindowWidth;
            Height = settings.ClientWindowHeight;

            CheckFullScreenSettings();
        }

        private void OnEnableAntiAliasingSettingsChanged(object sender, PropertyChangedRoutedEventArgs<bool> args)
        {
            if (Dispatcher.CheckAccess())
            {
                ApplyAntiAliasingSettings();
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, (Action)ApplyAntiAliasingSettings);
            }
        }

        private void ApplyAntiAliasingSettings()
        {
            if (ClientSettings.Current.EnableAntiAliasing)
            {
                ContentPanel.ClearValue(RenderOptions.EdgeModeProperty);
            }
            else
            {
                RenderOptions.SetEdgeMode(ContentPanel, EdgeMode.Aliased);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            using (_settingsChangeScope.Enter())
            {
                SaveWindowDimensions();
            }
        }

        private void SaveWindowDimensions()
        {
            if (RestoreBounds.IsEmpty)
            {
                return;
            }

            ClientSettings.Current.ClientWindowWidth = RestoreBounds.Width;
            ClientSettings.Current.ClientWindowHeight = RestoreBounds.Height;
            ClientSettings.Current.Save();
        }

        private void CheckFullScreenSettings()
        {
            if (_settingsChangeScope.IsWithin)
            {
                return;
            }

            RenderOptions.SetBitmapScalingMode(
                ContentPanel,
                ClientSettings.Current.EnableHighQualityScaling
                    ? BitmapScalingMode.HighQuality
                    : BitmapScalingMode.LowQuality);
            SetFullScreenMode(ClientSettings.Current.EnableFullScreenMode);
        }

        private void OnViewActivating(ViewActivatingEventArgs e)
        {
            if (!_appContext.IsConnected)
            {
                return;
            }

            e.Cancel = IsDialogOpen() && !(e.View is ISystemAssaultScreenView);
        }

        private void OnLoaded(object @object, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            ClientSettings.Current.Saved += (s, e) => CheckFullScreenSettings();
            ClientSettings.Current.Loaded += (s, e) => CheckFullScreenSettings();

            _ = _eventAggregator.GetEvent<ViewActivatingEvent>().Subscribe(OnViewActivating, ThreadOption.PublisherThread);

            _audioEngine.Volume = (float)ClientSettings.Current.MasterVolume;
            _musicPlayer.Volume = (float)ClientSettings.Current.MusicVolume;
            _soundPlayer.Volume = (float)ClientSettings.Current.FXVolume;
            ClientSettings.Current.MasterVolumeChanged += (s, e) => _audioEngine.Volume = (float)e.NewValue;
            ClientSettings.Current.MusicVolumeChanged += (s, e) => _musicPlayer.Volume = (float)e.NewValue;
            ClientSettings.Current.FXVolumeChanged += (s, e) => _soundPlayer.Volume = (float)e.NewValue;
            _audioEngine.Start();
            _musicPlayer.PlayMode = PlaybackMode.Sequential | PlaybackMode.Fade;

            _musicPlayer.SwitchMusic("DefaultMusic");
            _musicPlayer.Play();
        }

        private void OnModelessDialogsRegionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckScreenFocus();
        }

        private bool IsDialogOpen()
        {
            return (ModalDialogsRegion.ActiveDialog != null) || (ModelessDialogsRegion.ActiveDialog != null);
        }

        private void CheckScreenFocus()
        {
            if (IsDialogOpen())
            {
                return;
            }

            Control currentScreen = GameScreensRegion.CurrentScreen;
            _ = currentScreen != null ? currentScreen.Focus() : GameScreensRegion.Focus();
        }

        private void ExecuteEscapeCommand(object sender, ExecutedRoutedEventArgs args)
        {
            if (IsDialogOpen())
            {
                return;
            }

            if (GameScreensRegion.CurrentScreen is IGalaxyScreenView)
            {
                _navigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);
            }
            else if (!(GameScreensRegion.CurrentScreen is MenuScreen))
            {
                _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            }
        }

        protected void ToggleFullScreenMode()
        {
            SetFullScreenMode(WindowState != WindowState.Maximized);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_settingsChangeScope.IsWithin)
            {
                return availableSize;
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (_settingsChangeScope.IsWithin)
            {
                return arrangeBounds;
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        protected void SetFullScreenMode(bool enableFullScreenMode)
        {
            using (_settingsChangeScope.Enter())
            {
                using (Dispatcher.DisableProcessing())
                {
                    if (enableFullScreenMode)
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            return;
                        }

                        ResizeMode = ResizeMode.CanMinimize;
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Maximized;
                        ClientSettings.Current.EnableFullScreenMode = true;
                        ClientSettings.Current.Save();
                    }
                    else
                    {
                        if (WindowState == WindowState.Normal)
                        {
                            return;
                        }

                        CenterToScreen();
                        ResizeMode = ResizeMode.CanResize;
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Normal;
                        ClientSettings.Current.EnableFullScreenMode = false;
                        ClientSettings.Current.Save();
                    }
                }
            }
            InvalidateMeasure();
        }

        protected void CenterToScreen()
        {
            Rect workArea = SystemParameters.WorkArea;
            Rect windowSize = RestoreBounds;

            if (double.IsInfinity(windowSize.Width) || double.IsInfinity(windowSize.Height))
            {
                windowSize = new Rect(0, 0, Width, Height);
            }

            Left = (workArea.Width - windowSize.Width) / 2;
            Top = (workArea.Height - windowSize.Height) / 2;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.NewSize.Width > MaxUnscaledScreenWidth)
            {
                _scaleFactor = sizeInfo.NewSize.Width / MaxUnscaledScreenWidth;

                ClientProperties.SetScaleFactor(this, _scaleFactor);

                LayoutTransform = ContentPanel.LayoutTransform = new ScaleTransform(_scaleFactor, _scaleFactor, 0.5, 0.5);
            }
            else
            {
                _scaleFactor = 1.0;

                ClearValue(ClientProperties.ScaleFactorProperty);

                LayoutTransform = ContentPanel.LayoutTransform = null;

            }
        }

        private void OnModalDialogsRegionSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            bool enableShaders = RenderCapability.IsPixelShaderVersionSupported(2, 0);

            if (ModalDialogsRegion.ActiveDialog == null)
            {
                GameScreensRegionBorder.ClearValue(OpacityProperty);
                GameScreensRegionBorder.ClearValue(EffectProperty);
                CheckScreenFocus();
            }
            else if (enableShaders)
            {
                GameScreensRegionBorder.Effect = new ColorToneEffect
                {
                    DarkColor = Color.FromScRgb(1.0f, 0.1f, 0.075f, 0.125f),
                    LightColor = Color.FromScRgb(1.0f, 0.4f, 0.3f, 0.5f),
                    Desaturation = .8,
                    ToneAmount = 0
                };
            }
            else
            {
                GameScreensRegionBorder.Opacity = 0.4;
            }
        }

        private void OnClientDisconnected(ClientDataEventArgs<ClientDisconnectReason> obj)
        {
            if (_isClosing)
            {
                _exitInProgress = true;
            }
        }

        private void OnGameEnding(ClientEventArgs obj)
        {
            if (_isClosing)
            {
                _exitInProgress = true;
            }
        }

        private void OnGameEnded(ClientEventArgs obj)
        {
            if (_isClosing)
            {
                _exitInProgress = true;
            }

            ClearValue(ContextMenuProperty);
        }

        private void OnGameStarted(ClientDataEventArgs<GameStartData> obj)
        {
            ContextMenu = new GameContextMenu { CustomPopupPlacementCallback = ContextMenuPlacementCallback };
        }

        private void OnTurnStarted(ClientEventArgs e)
        {
            _soundPlayer.PlayFile("Resources/SoundFX/NewTurn.ogg");
        }

        private void OnChatMessageReceived(ClientDataEventArgs<ChatMessage> e)
        {
            if (e.Value == null || !_appContext.IsGameInPlay)
            {
                return;
            }

            if (ReferenceEquals(e.Value.Sender, _appContext.LocalPlayer))
            {
                return;
            }

            _soundPlayer.PlayFile("Resources/SoundFX/ChatMessage.ogg");
        }

        private void OnAllTurnEnded(ClientEventArgs obj)
        {
            if (!(GameScreensRegion.CurrentScreen is MenuScreen || GameScreensRegion.CurrentScreen is IGalaxyScreenView))
            {
                _navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel)
            {
                return;
            }

            _isClosing = true;

            try
            {
                ClientCommands.Exit.Execute(true);
            }
            catch
            {
                _exitInProgress = true;
            }

            if (_exitInProgress)
            {
                return;
            }

            _isClosing = false;
            e.Cancel = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            SaveWindowDimensions();
            ClientSettings.Current.Save();
            _audioEngine.Dispose();
        }

        private CustomPopupPlacement[] ContextMenuPlacementCallback(
            Size popupSize,

            Size targetSize,

            Point offset)
        {
            double dpiConversion = PixelToPoint(1.0);
            Point mouse = Mouse.GetPosition(this);

            if (ContextMenu != null)
            {
                popupSize = (Size)((Vector)ContextMenu.RenderSize * _scaleFactor);
            }

            targetSize = (Size)((Vector)ContentPanel.RenderSize * _scaleFactor);

            Point point = new Point(
                Math.Max(
                    0,
                    Math.Min(
                        mouse.X - popupSize.Width / 2,
                        targetSize.Width - popupSize.Width)) / dpiConversion,
                Math.Max(
                    0,
                    Math.Min(
                        mouse.Y - popupSize.Height / 2,
                        targetSize.Height - popupSize.Height)) / dpiConversion);

            return new[] { new CustomPopupPlacement { Point = point } };
        }

        public void ForceWaitCursor()
        {
            lock (_waitCursorLock)
            {
                _waitCursorCount++;
                Cursor = Cursors.Wait;
            }
            Mouse.UpdateCursor();
        }

        public void ClearWaitCursor()
        {
            lock (_waitCursorLock)
            {
                if (_waitCursorCount > 0)
                {
                    _waitCursorCount--;
                }

                if (_waitCursorCount == 0)
                {
                    Cursor = _defaultCursor;
                }
            }
            Mouse.UpdateCursor();
        }

        #region Implementation of IGameWindow
        public IDisposable EnterWaitCursorScope()
        {
            ForceWaitCursor();
            return new Disposer(ClearWaitCursor);
        }
        #endregion
    }
}
