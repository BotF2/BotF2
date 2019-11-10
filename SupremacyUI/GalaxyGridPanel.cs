// GalaxyGridPanel.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Client.Views;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Pathfinding;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Supremacy.UI
{
    public sealed class GalaxyGridPanel : Control, IScrollInfo, IAnimationsHost
    {
        #region Constants
        public const double SectorSize = 72;
        internal const double FleetIconSize = 20.0;
        internal const double FleetIconSpacing = 3.0;
        private const double MaxScaleFactor = 2.0;
        private const double ZoomIncrement = 0.10;
        private const double StarNameFontSize = 12.0;
        private const double MinVisibleStarNameFontSize = 8.0;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty OptionsProperty;
        public static readonly DependencyProperty UseAnimatedStarsProperty;
        public static readonly DependencyProperty UseAnimationProperty;
        public static readonly DependencyProperty UseCombatScreenProperty;
        public static readonly DependencyProperty SelectedFleetProperty;
        public static readonly DependencyProperty SelectedSectorProperty;
        private static readonly DependencyPropertyKey HoveredSectorPropertyKey;
        public static readonly DependencyProperty HoveredSectorProperty;
        public static readonly DependencyProperty SelectedSectorAllegianceProperty;
        public static readonly DependencyProperty SelectedTradeRouteProperty;
        #endregion

        #region Fields
        private static readonly Dictionary<int, Pen> s_borderPens;
        private static readonly Dictionary<int, Brush> s_colonyNameBrushes;
        private static readonly BitmapImage s_defaultFleetIcon;
        private static readonly BitmapImage s_multiFleetIcon;
        private static readonly LinearGradientBrush s_disputedSectorFill;
        private static readonly Dictionary<int, SolidColorBrush> s_empireFills;
        private static readonly Dictionary<int, SolidColorBrush> s_colonyFills;
        private static readonly Dictionary<int, BitmapImage> s_fleetIcons;
        private static readonly Brush s_fogOfWarBrush;
        private static readonly Pen s_minorRaceBorderPen;
        private static readonly SolidColorBrush s_minorRaceFill;
        private static readonly Pen s_routePen;
        private static readonly Pen s_routePenWarning;
        private static readonly Dictionary<int, Pen> s_scanPens;
        private static readonly Dictionary<StarType, BitmapImage> s_starImages;
        private static readonly Typeface s_textTypeface;
        private static readonly Pen s_tradeRouteInvalidPen;
        private static readonly Pen s_tradeRouteSetPen;
        private static readonly Pen s_tradeRouteValidPen;
        private static readonly BitmapImage s_unknownFleetIcon;

        private static IAppContext s_appContext;
        private static IPlayerOrderService s_playerOrderService;

        private readonly DrawingVisual _backdrop;
        private readonly DrawingVisual _borderLines;
        private readonly VisualCollection _children;
        private readonly DrawingVisual _composite;
        private readonly SectorMap _galaxy;
        private readonly DrawingVisual _routeLines;
        private readonly Dictionary<Fleet, Visual> _routePaths;
        private readonly ScaleTransform _scale;
        private readonly DrawingVisual _sectors;
        private readonly DrawingVisual _selectRect;
        private readonly DrawingVisual _shipRange;
        private readonly DrawingVisual _starNames;
        private readonly DrawingVisual _tradeLines;
        private readonly TranslateTransform _translation;
        private readonly List<Sector> _waypoints;
        private readonly List<FleetIconAdorner> _fleetIconAdorners;
        private readonly Canvas _fleetIconCanvas = new Canvas();
        private readonly DispatcherTimer _autoScrollTimer;
        private Dictionary<StarType, ImageBrush> _starBrushes;
        private GuidelineSet _guides;
        private GuidelineSet _halfGuides;
        private GalaxyGridInputMode _inputMode;
        private bool _isDragScrollInProgress;
        private double _lastOffsetRequestX = -1;
        private double _lastOffsetRequestY = -1;
        private Pen _scanPen;
        private Sector _lastSector;
        private TravelRoute _newRoute;
        private Visual _newRouteEta;
        private Visual _newRoutePath;
        private Visual _newTradeLine;
        private ScrollData _scrollData;
        private Point _scrollStartOffset;
        private Point _scrollStartPoint;
        private List<Clock> _animationClocks;
        private readonly DelegateCommand<Sector> _centerOnSectorCommand;
        private readonly DelegateCommand<Sector> _selectSectorCommand;
        private readonly DelegateCommand<object> _zoomInCommand;
        private readonly DelegateCommand<object> _zoomOutCommand;

        private GalaxyScreenPresentationModel _screenModel;
        private IObservable<Sector> _hoveredSector;
        private IDisposable _hoveredSectorSubscription;
        private ISoundPlayer _soundPlayer = null;
        #endregion

        #region Events
        public event DependencyPropertyChangedEventHandler<GalaxyViewOptions> OptionsChanged;
        public event DependencyPropertyChangedEventHandler<Fleet> SelectedFleetChanged;
        public event DependencyPropertyChangedEventHandler<TradeRoute> SelectedTradeRouteChanged;
        public event DependencyPropertyChangedEventHandler<Sector> SelectedSectorChanged;
        public event SectorEventHandler SectorDoubleClicked;
        #endregion

        #region Constructors
        static GalaxyGridPanel()
        {

            s_fleetIcons = new Dictionary<int, BitmapImage>();
            s_empireFills = new Dictionary<int, SolidColorBrush>();
            s_colonyFills = new Dictionary<int, SolidColorBrush>();
            s_starImages = new Dictionary<StarType, BitmapImage>();
            s_scanPens = new Dictionary<int, Pen>();
            s_colonyNameBrushes = new Dictionary<int, Brush>();
            s_borderPens = new Dictionary<int, Pen>();

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            //Set up defaults and ones that don't change depending on civilization
            //Scans
            var scanBrush = new SolidColorBrush(Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
            scanBrush.Freeze();

            //Ship Routes
            s_routePen = new Pen(Brushes.White, 3.0)
            {
                DashStyle = DashStyles.Dot,
                EndLineCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                LineJoin = PenLineJoin.Bevel
            };
            s_routePen.Freeze();
            s_routePenWarning = new Pen(Brushes.Red, 3.0)
            {
                DashStyle = DashStyles.Dot,
                EndLineCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                LineJoin = PenLineJoin.Bevel
            };
            s_routePenWarning.Freeze();

            //Trade Routes
            s_tradeRouteValidPen = new Pen(Brushes.Green, 1.0);
            s_tradeRouteValidPen.Freeze();
            s_tradeRouteInvalidPen = new Pen(Brushes.Red, 1.0);
            s_tradeRouteInvalidPen.Freeze();
            s_tradeRouteSetPen = new Pen(Brushes.Tan, 1.0)
            {
                DashStyle = new DashStyle(new double[] { 0, 4 }, 0)
            };
            s_tradeRouteSetPen.Freeze();

            //Fleet Icons
            s_defaultFleetIcon = LoadFleetIcon(
                ResourceManager.GetResourceUri("Resources/Images/Insignias/__default.png"));
            s_unknownFleetIcon = LoadFleetIcon(
                ResourceManager.GetResourceUri("Resources/Images/Insignias/__unknown.png"));
            s_multiFleetIcon = LoadFleetIcon(
                ResourceManager.GetResourceUri("Resources/Images/Insignias/__multi_fleet_indicator.png"));

            //Minor Race Fill
            s_minorRaceFill = new SolidColorBrush(Color.FromArgb(63, 127, 127, 127));
            s_minorRaceFill.Freeze();

            //Minor Race Borders
            var minorRaceBorderPenBrush = new SolidColorBrush(
                Color.FromArgb(255, 127, 127, 127));
            minorRaceBorderPenBrush.Freeze();
            s_minorRaceBorderPen = new Pen(minorRaceBorderPenBrush, 2.0);
            s_minorRaceBorderPen.Freeze();

            //Fog of War
            var fogOfWarColor = Color.FromArgb(0x66, 0x33, 0x33, 0x33);
            s_fogOfWarBrush = new SolidColorBrush(fogOfWarColor);
            s_fogOfWarBrush.Freeze();

            //Load empire specific ones
            //Instead of loading the civilizations from the gamecontext,
            //load them straight from the db, as this panel is only constructed once.
            //Failure to do so will cause crashes when starting a second game
            foreach (var civ in MasterResources.CivDB)
            {
                var color = (Color) ColorConverter.ConvertFromString(civ.Color);
                var textColor = Color.Add(color, Colors.Gray);

                //Scan
                scanBrush = new SolidColorBrush(Avalon.Windows.Utility.ColorHelpers.Lighten(color, 0.67f))
                {
                    Opacity = 0.5
                };
                scanBrush.Freeze();
                s_scanPens[civ.CivID] = new Pen(scanBrush, 1.0);
                s_scanPens[civ.CivID].Freeze();

                //Empire fills
                color.A = 31;
                //Don't load if white as this screws up disputed space
                if (civ.Color != "White")
                {
                    s_empireFills[civ.CivID] = new SolidColorBrush(color);
                    s_empireFills[civ.CivID].Freeze();
                }

                //Colonies
                color.A = 63;
                s_colonyFills[civ.CivID] = new SolidColorBrush(color);
                s_colonyFills[civ.CivID].Freeze();
                s_colonyNameBrushes[civ.CivID] = new SolidColorBrush(textColor);
                s_colonyNameBrushes[civ.CivID].Freeze();

                //Borders
                var borderBrush = new SolidColorBrush(color);
                borderBrush.Freeze();
                s_borderPens[civ.CivID] = new Pen(borderBrush, 2.0);
                s_borderPens[civ.CivID].Freeze();

                //Fleet icons
                var iconPath = "Resources/Images/Insignias/" + civ.Key.ToLower() + ".png";
                if (File.Exists(ResourceManager.GetResourcePath(iconPath)))
                {
                    s_fleetIcons[civ.CivID] = LoadFleetIcon(ResourceManager.GetResourceUri(iconPath));
                }
                else
                {
                    s_fleetIcons[civ.CivID] = s_defaultFleetIcon;
                }
            }
            
            //Disputed
            s_disputedSectorFill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            var uniqueEmpireFills = s_empireFills.Values.Distinct().ToList();
            var stepOffset = 1.0 / uniqueEmpireFills.Count / 2;
            var i = 0;
            foreach (var empireBrush in uniqueEmpireFills)
            {
                s_disputedSectorFill.GradientStops.Add(
                    new GradientStop(empireBrush.Color,
                                     stepOffset * i));
                s_disputedSectorFill.GradientStops.Add(
                    new GradientStop(empireBrush.Color,
                                     0.5 + stepOffset * i));
                i++;
            }
            s_disputedSectorFill.Freeze();


            foreach (StarType type in EnumUtilities.GetValues<StarType>())
            {
                s_starImages[type] = new BitmapImage(
                    ResourceManager.GetResourceUri(string.Format("Resources/Images/Stars/Map/{0}.png", type)));
            }

            s_textTypeface = new Typeface(
                new FontFamily("#Resources/Fonts/Calibri"),
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Normal);

            OptionsProperty = DependencyProperty.Register(
                "Options",
                typeof(GalaxyViewOptions),
                typeof(GalaxyGridPanel),
                new PropertyMetadata(GalaxyViewOptions.Default, OptionsChangedCallback));
            SelectedFleetProperty = DependencyProperty.Register(
                "SelectedFleet",
                typeof(Fleet),
                typeof(GalaxyGridPanel),
                new PropertyMetadata(
                    null,
                    SelectedFleetChangedCallback,
                    SelectedFleetCoerceValueCallback));
            SelectedTradeRouteProperty = DependencyProperty.Register(
                "SelectedTradeRoute",
                typeof(TradeRoute),
                typeof(GalaxyGridPanel),
                new PropertyMetadata(
                    null,
                    SelectedTradeRouteChangedCallback,
                    SelectedTradeRouteCoerceValueCallback));
            SelectedSectorProperty = DependencyProperty.Register(
                "SelectedSector",
                typeof(Sector),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    SelectedSectorChangedCallback));
            HoveredSectorPropertyKey = DependencyProperty.RegisterReadOnly(
                "HoveredSector",
                typeof(Sector),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(null));
            SelectedSectorAllegianceProperty = DependencyProperty.Register(
                "SelectedSectorAllegiance",
                typeof(string),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    String.Empty,
                    FrameworkPropertyMetadataOptions.None,
                    null,
                    CoerceSelectedSectorAllegiance));
            UseAnimatedStarsProperty = DependencyProperty.Register(
                "UseAnimatedStars",
                typeof(bool),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    true,
                    UseAnimatedStarsChangedCallback));
            UseAnimationProperty = DependencyProperty.Register(
                "UseAnimation",
                typeof(bool),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    true,
                    UseAnimationChangedCallback));
            UseCombatScreenProperty = DependencyProperty.Register(
                "UseCombatScreen",
                typeof(bool),
                typeof(GalaxyGridPanel),
                new FrameworkPropertyMetadata(
                    true,
                    UseCombatScreenChangedCallback));
        }

        public GalaxyGridPanel()
            : this(GameContext.Current.Universe.Map, ServiceLocator.Current.GetInstance<ISoundPlayer>()) { }

        public GalaxyGridPanel(SectorMap galaxy, [NotNull] ISoundPlayer soundPlayer)
        {
            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");
            _soundPlayer = soundPlayer;

            if (galaxy == null)
                throw new ArgumentNullException("galaxy");

            InputBindings.Add(
                new KeyBinding(
                    GalaxyScreenCommands.MapZoomIn,
                    Key.Add,
                    ModifierKeys.None));

            InputBindings.Add(
                new KeyBinding(
                    GalaxyScreenCommands.MapZoomOut,
                    Key.Subtract,
                    ModifierKeys.None));

            CommandBindings.Add(
                new CommandBinding(
                    GalaxyScreenCommands.MapZoomIn,
                    (sender, args) => ZoomIn()));

            CommandBindings.Add(
                new CommandBinding(
                    GalaxyScreenCommands.MapZoomOut,
                    (sender, args) => ZoomOut()));

            _fleetIconAdorners = new List<FleetIconAdorner>();
            _centerOnSectorCommand = new DelegateCommand<Sector>(ExecuteCenterOnSectorCommand);
            _zoomInCommand = new DelegateCommand<object>(ExecuteZoomInCommand);
            _zoomOutCommand = new DelegateCommand<object>(ExecuteZoomOutCommand);
            _selectSectorCommand = new DelegateCommand<Sector>(ExecuteSelectSectorCommand);

            GalaxyScreenCommands.CenterOnSector.RegisterCommand(_centerOnSectorCommand);
            GalaxyScreenCommands.MapZoomIn.RegisterCommand(_zoomInCommand);
            GalaxyScreenCommands.MapZoomOut.RegisterCommand(_zoomOutCommand);
            GalaxyScreenCommands.SelectSector.RegisterCommand(_selectSectorCommand);

            ClientEvents.ScreenRefreshRequired.Subscribe(OnScreenRefreshRequired, ThreadOption.UIThread);

            _autoScrollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.2d) };
            _autoScrollTimer.Tick += OnAutoScrollTimerTick;

            var transforms = new TransformGroup();

            _translation = new TranslateTransform();
            _scale = new ScaleTransform();

            transforms.Children.Add(_translation);
            transforms.Children.Add(_scale);

            RenderTransform = transforms;

            Focusable = true;
            ClipToBounds = false;

            _galaxy = galaxy;
            _inputMode = GalaxyGridInputMode.Default;
            InputModeOnFirstClick = GalaxyGridInputMode.Default;
            _lastSector = null;
            _waypoints = new List<Sector>();
            _routePaths = new Dictionary<Fleet, Visual>();
            _newRoute = null;
            _newRoutePath = null;
            _newTradeLine = null;
            _newRouteEta = null;
            _backdrop = new DrawingVisual();
            _children = new VisualCollection(this);
            _composite = new DrawingVisual();
            _selectRect = new DrawingVisual();
            _borderLines = new DrawingVisual();
            _shipRange = new DrawingVisual();
            _sectors = new DrawingVisual();
            _starNames = new DrawingVisual();
            _routeLines = new DrawingVisual();
            _tradeLines = new DrawingVisual();
            _routeLines.Opacity = 0.50;
            _shipRange.Opacity = 0;

            CreateGuides();
            CreateBackdrop();

            _children.Add(_composite);
            _children.Add(_selectRect);
            _children.Add(_shipRange);
            _children.Add(_routeLines);
            _children.Add(_tradeLines);
            _children.Add(_starNames);

            OptionsChanged += GalaxyGridPanel_OptionsChanged;
            SelectedFleetChanged += GalaxyGridPanel_SelectedFleetChanged;
            SelectedTradeRouteChanged += GalaxyGridPanel_SelectedTradeRouteChanged;
            SizeChanged += GalaxyGridPanel_SizeChanged;
            Unloaded += OnUnloaded;
            Loaded += OnLoaded;

            SetBinding(
                UseAnimatedStarsProperty,
                new Binding
                {
                    Source = ClientSettings.Current,
                    Path = new PropertyPath(ClientSettings.EnableStarMapAnimationsProperty),
                    Mode = BindingMode.OneWay
                });

            SetBinding(
                UseAnimationProperty,
                new Binding
                {
                    Source = ClientSettings.Current,
                    Path = new PropertyPath(ClientSettings.EnableAnimationProperty),
                    Mode = BindingMode.OneWay
                });

            SetBinding(
                UseCombatScreenProperty,
                new Binding
                {
                    Source = ClientSettings.Current,
                    Path = new PropertyPath(ClientSettings.EnableCombatScreenProperty),
                    Mode = BindingMode.OneWay
                });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GameLog.Client.General.InfoFormat("GalaxyGridPanel.cs: OnLoading is beginning...");
            var galaxyScreen = this.FindVisualAncestorByType<GalaxyScreenView>();
            if (galaxyScreen != null)
                _screenModel = galaxyScreen.Model;

            if (_screenModel == null)
                return;

            _screenModel.SelectedTaskForceChanged += OnScreenModelSelectedTaskForceChanged;
            _screenModel.SelectedSectorChanged += OnScreenModelSelectedSectorChanged;

            if (_hoveredSector == null)
                _hoveredSector = HoveredSectorAsObservable();

            _hoveredSectorSubscription = _hoveredSector.Subscribe(
                sector =>
                {
                    Dispatcher.VerifyAccess();
                    if (_screenModel != null)
                        _screenModel.HoveredSector = sector;
                });
        }

        private IObservable<Sector> HoveredSectorAsObservable()
        {
            var mouseMove = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                eventHandler => new MouseEventHandler(eventHandler),
                handler => MouseMove += handler,
                handler => MouseMove -= handler)
                .ObserveOn(Scheduler.Immediate)
                .Select(@event => PointToSector(@event.EventArgs.GetPosition(this)));

            var mouseLeave = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                eventHandler => new MouseEventHandler(eventHandler),
                handler => MouseLeave += handler,
                handler => MouseLeave -= handler)
                .ObserveOn(Scheduler.Immediate)
                .Select(_ => (Sector)null);

            return mouseMove.Merge(mouseLeave);
        }

        private void ExecuteZoomInCommand(object obj)
        {
            ZoomIn();
        }

        private void ExecuteZoomOutCommand(object obj)
        {
            ZoomOut();
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            if (_screenModel != null)
            {
                _screenModel.SelectedTaskForceChanged -= OnScreenModelSelectedTaskForceChanged;
                _screenModel.SelectedSectorChanged -= OnScreenModelSelectedSectorChanged;
                _screenModel = null;
            }

            StopAnimations();

            _children.Clear();

            if (_hoveredSectorSubscription != null)
            {
                _hoveredSectorSubscription.Dispose();
                _hoveredSectorSubscription = null;
            }

            GalaxyScreenCommands.CenterOnSector.UnregisterCommand(_centerOnSectorCommand);
            GalaxyScreenCommands.MapZoomIn.UnregisterCommand(_zoomInCommand);
            GalaxyScreenCommands.MapZoomOut.UnregisterCommand(_zoomOutCommand);
            GalaxyScreenCommands.SelectSector.UnregisterCommand(_selectSectorCommand);
            ClientEvents.ScreenRefreshRequired.Unsubscribe(OnScreenRefreshRequired);
        }

        private void OnScreenModelSelectedTaskForceChanged(object sender, EventArgs e)
        {
            if (_screenModel == null)
                return;

            var selectedTaskForce = _screenModel.SelectedTaskForce;

            if (Equals(selectedTaskForce, SelectedFleet))
                return;

            if (selectedTaskForce == null)
                SelectedFleet = null;
            else
                SelectedFleet = selectedTaskForce.View.Source;
        }

        private void OnScreenModelSelectedSectorChanged(object sender, EventArgs e)
        {
            if (_screenModel == null)
                return;

            if (Equals(_screenModel.SelectedSector, SelectedSector))
                return;

            SelectedSector = _screenModel.SelectedSector;
        }

        private void OnScreenRefreshRequired(ClientEventArgs obj)
        {
            Update();
        }
        #endregion

        #region Properties
        private static IAppContext AppContext
        {
            get
            {
                if (s_appContext == null)
                    s_appContext = ServiceLocator.Current.GetInstance<IAppContext>();
                return s_appContext;
            }
        }

        private static IPlayerOrderService PlayerOrderService
        {
            get
            {
                if (s_playerOrderService == null)
                    s_playerOrderService = ServiceLocator.Current.GetInstance<IPlayerOrderService>();
                return s_playerOrderService;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        private bool IsScrolling
        {
            get
            {
                if (_scrollData != null)
                    return (_scrollData.ScrollOwner != null);
                return false;
            }
        }

        private GalaxyGridInputMode InputMode
        {
            get { return _inputMode; }
        }

        private GalaxyGridInputMode InputModeOnFirstClick { get; set; }

        public Sector SelectedSector
        {
            get { return GetValue(SelectedSectorProperty) as Sector; }
            set { SetValue(SelectedSectorProperty, value); }
        }
        
        public Sector HoveredSector
        {
            get { return GetValue(HoveredSectorProperty) as Sector; }
            private set { SetValue(HoveredSectorPropertyKey, value); }
        }

        public string SelectedSectorAllegiance
        {
            get { return GetValue(SelectedSectorAllegianceProperty) as string; }
            set { SetValue(SelectedSectorAllegianceProperty, value); }
        }

        public TradeRoute SelectedTradeRoute
        {
            get { return GetValue(SelectedTradeRouteProperty) as TradeRoute; }
            set { SetValue(SelectedTradeRouteProperty, value); }
        }

        public Civilization PlayerCivilization
        {
            get
            {
                var playerEmpire = AppContext.LocalPlayerEmpire;
                if (playerEmpire == null)
                    return null;
                return playerEmpire.Civilization;
            }
        }

        public GalaxyViewOptions Options
        {
            get { return (GalaxyViewOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public Fleet SelectedFleet
        {
            get { return GetValue(SelectedFleetProperty) as Fleet; }
            set { SetValue(SelectedFleetProperty, value); }
        }

        public bool UseAnimatedStars
        {
            get { return (bool)GetValue(UseAnimatedStarsProperty); }
            set { SetValue(UseAnimatedStarsProperty, value); }
        }

        public bool UseAnimation
        {
            get { return (bool)GetValue(UseAnimationProperty); }
            set { SetValue(UseAnimationProperty, value); }
        }
        public bool UseCombatScreen
        {
            get { return (bool)GetValue(UseCombatScreenProperty); }
            set { SetValue(UseCombatScreenProperty, value); }
        }

        public double ScaleFactor
        {
            get { return _scale.ScaleX; }
        }

        private double MinScaleFactor
        {
            get
            {
                return Math.Max(ViewportWidth / (SectorSize * _galaxy.Width),
                                ViewportHeight / (SectorSize * _galaxy.Height));
            }
        }

        public bool CanZoomOut
        {
            get { return (ScaleFactor > MinScaleFactor); }
        }

        public bool CanZoomIn
        {
            get { return (ScaleFactor < MaxScaleFactor); }
        }
        #endregion

        #region Methods
        private static BitmapImage LoadFleetIcon(Uri uri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.DecodePixelHeight = (int)(FleetIconSize * MaxScaleFactor);
            image.UriSource = uri;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public void PauseAnimations()
        {
            if (_animationClocks == null)
                return;

            foreach (var clock in _animationClocks)
            {
                if (!clock.IsPaused && clock.Controller != null)
                    clock.Controller.Pause();
            }

            _fleetIconAdorners.ForEach(o => o.PauseAnimations());
        }

        public void ResumeAnimations()
        {
            if (_animationClocks == null || !UseAnimatedStars)
                return;

            foreach (var clock in _animationClocks)
            {
                if (clock.IsPaused && clock.Controller != null)
                    clock.Controller.Resume();
            }

            _fleetIconAdorners.ForEach(o => o.ResumeAnimations());
        }

        void IAnimationsHost.StopAnimations()
        {
            StopAnimations();
        }

        private void StopAnimations()
        {
            if (_animationClocks == null)
                return;

            foreach (var clockController in _animationClocks.Select(o => o.Controller).Where(o => o!= null))
            {
                clockController.Stop();
                clockController.Remove();
            }

            _fleetIconAdorners.ForEach(o => o.StopAnimations());
        }

        private static Visual BuildRouteETA(Fleet fleet, TravelRoute route)
        {
            if ((fleet == null) || (fleet.Ships.Count == 0) || (fleet.Speed == 0))
                return null;
            var eta = (route.Length / fleet.Speed) + (((route.Length % fleet.Speed) == 0) ? 0 : 1);
            var visual = new DrawingVisual();
            var text = new FormattedText(
                eta.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                s_textTypeface,
                StarNameFontSize,
                Brushes.White,
                VisualTreeHelper.GetDpi(visual).PixelsPerDip);

            using (var dc = visual.RenderOpen())
            {
                const int padding = 3;
                var midpoint = GetSectorMidpoint(route.Steps[route.Length - 1]);
                var border = new Rect(
                    midpoint.X - text.Width / 2 - padding,
                    midpoint.Y - text.Height / 2 - padding,
                    text.Width + (padding * 2),
                    text.Height + (padding * 2));
                dc.DrawRectangle(Brushes.Black, new Pen(Brushes.White, 1.0), border);
                midpoint.Offset(-text.Width / 2, -text.Height / 2);
                dc.DrawText(text, midpoint);
            }
            return visual;
        }

        private static Visual BuildRoutePath(Fleet fleet, TravelRoute route)
        {
            var visual = new DrawingVisual();
            var geometry = new StreamGeometry();
            var isInRange = true;

            using (var context = geometry.Open())
            {
                context.BeginFigure(GetSectorMidpoint(fleet.Location), false, false);
                foreach (var location in route.Steps)
                {
                    context.LineTo(GetSectorMidpoint(location), true, false);
                    if (isInRange && !IsSectorInRange(fleet, location))
                        isInRange = false;
                }
            }

            geometry.Freeze();

            using (var dc = visual.RenderOpen())
            {
                dc.DrawGeometry(
                    null,
                    isInRange ? s_routePen : s_routePenWarning,
                    geometry);
            }

            return visual;
        }

        private static Point GetSectorMidpoint(MapLocation location)
        {
            return new Point((location.X * SectorSize) + (0.5 * SectorSize),
                             (location.Y * SectorSize) + (0.5 * SectorSize));
        }

        private static FormattedText GetStarText(StarSystem system, Civilization playerCiv)
        {
            var owner = system.Owner;
            var brush = (system.HasColony && owner.IsEmpire && (DiplomacyHelper.IsContactMade(owner, playerCiv) || (owner == playerCiv)))
                            ? s_colonyNameBrushes[system.OwnerID]
                            : Brushes.White;
            string nameText;
            switch (system.StarType)
            {
                case StarType.Nebula:
                    nameText = string.Format(
                        ResourceManager.GetString("NEBULA_NAME_FORMAT"),
                        system.Name);
                    break;
                case StarType.Wormhole:
                    nameText = string.Format(
                        ResourceManager.GetString("WORMHOLE_NAME_FORMAT"),
                        system.Name);
                    break;
                default:
                    nameText = system.Name;
                    break;
            }

            FormattedText starName = new FormattedText(
                nameText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                s_textTypeface,
                StarNameFontSize,
                brush)
                {
                    MaxTextWidth = (SectorSize - 6),
                    TextAlignment = TextAlignment.Center,
                    MaxLineCount = 2,
                    Trimming = TextTrimming.CharacterEllipsis,
                    LineHeight = StarNameFontSize
                };
    //        //var starName = new FormattedText(
                //nameText,
                //CultureInfo.CurrentCulture,
                //FlowDirection.LeftToRight,
                //s_textTypeface,
                //StarNameFontSize,
                //brush,
                //VisualTreeHelper.GetDpi(brush).PixelsPerDip)    <- this line or "this" doesn't work. Without this line there is a warning (nothing more)
                //        {
                //            MaxTextWidth = (SectorSize - 6),
                //            TextAlignment = TextAlignment.Center,
                //            MaxLineCount = 2,
                //            Trimming = TextTrimming.CharacterEllipsis,
                //            LineHeight = StarNameFontSize
                //        };
            return starName;
        }

        private static bool IsExplored(MapLocation location)
        {
            var playerEmpire = AppContext.LocalPlayerEmpire;
            if (playerEmpire == null)
                return false;

            //if (playerEmpire.MapData.IsExplored(location) == true)
            //{
            //    GameLog.Core.GameData.InfoFormat("location = {0}, playerEmpire = {1}, IsScanned = {2}, IsEXPLORED = {3}",
            //        location, playerEmpire.Civilization.Name,
            //        playerEmpire.MapData.IsScanned(location), playerEmpire.MapData.IsExplored(location)
            //        );
            //}

            return playerEmpire.MapData.IsExplored(location);
            //return true;  //just for testing
        }

        private static bool IsScanned(MapLocation location)
        {
            var playerEmpire = AppContext.LocalPlayerEmpire;
            if (playerEmpire == null)
                return false;

            //if (playerEmpire.MapData.IsScanned(location) == true)
            //{
            //    GameLog.Core.GameData.InfoFormat("location = {0}, playerEmpire = {1}, IsSCANNED = {2}, Strength = {3}, IsExplored = {4}",
            //        location, playerEmpire.Civilization.Name,
            //        playerEmpire.MapData.IsScanned(location),
            //        playerEmpire.MapData.GetScanStrength(location),
            //        playerEmpire.MapData.IsExplored(location)
            //        );
            //}

            //playerEmpire.MapData.IsExplored(playerEmpire.HomeSystem.Location) = true;
            //playerEmpire.MapData.IsExplored = true;

            return playerEmpire.MapData.IsScanned(location);
            //return true;  //just for testing
        }

        private static bool IsSectorInRange(Fleet fleet, MapLocation location)
        {
            var playerEmpire = AppContext.LocalPlayerEmpire;
            if (playerEmpire == null)
                return false;
            return (playerEmpire.MapData.GetFuelRange(location) <= fleet.Range);
        }

        private static bool IsStarNameVisible(StarSystem starSystem)
        {
            return ((starSystem != null) && 
                    IsExplored(starSystem.Location) &&
                    starSystem.StarType <= StarType.Wormhole);
        }

        private static bool IsValidTradeEndpoint(TradeRoute route, Colony colony)
        {
            if (colony == null)
                return false;
            return route.IsValidTargetColony(colony);
        }

        private static void OptionsChangedCallback(DependencyObject source,
                                                   DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if ((view == null) || (view.OptionsChanged == null))
                return;
            view.OptionsChanged(
                source,
                new DependencyPropertyChangedEventArgs<GalaxyViewOptions>(e));
        }

        private static void ResetScrolling(GalaxyGridPanel element)
        {
            element.InvalidateMeasure();
            if (element.IsScrolling)
                element._scrollData.ClearLayout();
        }

        private static void SelectedFleetChangedCallback(DependencyObject source,
                                                         DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if ((view == null) || (view.SelectedFleetChanged == null))
                return;
            view.SelectedFleetChanged(
                source,
                new DependencyPropertyChangedEventArgs<Fleet>(e));
        }

        private static object SelectedFleetCoerceValueCallback(
            DependencyObject source,
            object value)
        {
            var grid = source as GalaxyGridPanel;
            var fleet = value as Fleet;
            if ((grid == null) || (fleet == null))
                return null;
            if (fleet.Owner != grid.PlayerCivilization)
                return null;
            return value;
        }

        private static object CoerceSelectedSectorAllegiance(DependencyObject d, object value)
        {
            var source = d as GalaxyGridPanel;
            try
            {
                if (source != null)
                {
                    if (source.SelectedSector == null)
                        return null;
                    
                    var owner = GetPerceivedSectorOwner(source.SelectedSector);
                    if (owner != null)
                        return owner.ShortName;
                    
                    if (IsScanned(source.SelectedSector.Location))
                        return ResourceManager.GetString("SECTOR_NO_OWNER");
                }
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return null;
        }

        private static void SelectedSectorChangedCallback(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if (view == null)
                return;
            if (view.SelectedSectorChanged != null)
            {
                view.SelectedSectorChanged(
                    source,
                    new DependencyPropertyChangedEventArgs<Sector>(e));
            }
            view.CoerceValue(SelectedSectorAllegianceProperty);
            view.UpdateSelection();
        }

        private static void SelectedTradeRouteChangedCallback(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if ((view == null) || (view.SelectedTradeRouteChanged == null))
                return;
            view.SelectedTradeRouteChanged(
                source,
                new DependencyPropertyChangedEventArgs<TradeRoute>(e));
        }

        private static void UseAnimatedStarsChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if (view == null)
                return;

            view.Update(true);
        }

        private static void UseAnimationChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if (view == null)
                return;

            view.Update(true);
        }

        private static void UseCombatScreenChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var view = source as GalaxyGridPanel;
            if (view == null)
                return;

            view.Update(true);
        }

        private static object SelectedTradeRouteCoerceValueCallback(
            DependencyObject source, object value)
        {
            var grid = source as GalaxyGridPanel;
            
            var route = value as TradeRoute;
            if ((grid == null) || (route == null))
                return null;
            if (route.SourceColony.Owner != grid.PlayerCivilization)
                return null;
            return value;
        }

        private void ExecuteSelectSectorCommand(Sector sector)
        {
            if (sector == null)
                return;
            SelectedSector = sector;
        }

        private void ExecuteCenterOnSectorCommand(Sector sector)
        {
            if ((sector == null) || (ScrollOwner == null))
                return;
            AutoScrollToSector(sector);
        }

        public void CenterOnSelectedSector()
        {
            ExecuteCenterOnSectorCommand(SelectedSector);
        }

        public void SetHorizontalOffset(double offset, bool snapToGrid)
        {
            EnsureScrollData();

            if (snapToGrid)
            {
                var sectorSize = ExtentWidth / _galaxy.Width;
                var offsetX = Math.Floor(offset / sectorSize) * sectorSize;

                if (_lastOffsetRequestX < 0)
                    _lastOffsetRequestX = HorizontalOffset;

                if ((offset % sectorSize) > 1)
                    offsetX += sectorSize;

                // If scrolling right...
                if (offset > _lastOffsetRequestX)
                    offsetX -= (ViewportWidth % sectorSize);

                _lastOffsetRequestX = offset;

                offset = offsetX;
            }
            else
                _lastOffsetRequestY = offset;

            if (offset < 0 || ViewportWidth >= ExtentWidth)
                offset = 0;
            else
            {
                if (offset + ViewportWidth >= ExtentWidth)
                    offset = ExtentWidth - ViewportWidth;
            }

            _scrollData.Offset.X = offset;
            _translation.X = -offset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public void SetVerticalOffset(double offset, bool snapToGrid)
        {
            EnsureScrollData();

            if (snapToGrid)
            {
                var sectorSize = ExtentHeight / _galaxy.Height;
                var offsetY = Math.Floor(offset / sectorSize) * sectorSize;

                if (_lastOffsetRequestY < 0)
                    _lastOffsetRequestY = VerticalOffset;

                if ((offset % sectorSize) > 1)
                    offsetY += sectorSize;

                // If scrolling down...
                if (offset > _lastOffsetRequestY)
                    offsetY -= (ViewportHeight % sectorSize);

                _lastOffsetRequestY = offset;

                offset = offsetY;
            }
            else
                _lastOffsetRequestY = offset;

            if (offset < 0 || ViewportHeight >= ExtentHeight)
                offset = 0;
            else
            {
                if (offset + ViewportHeight >= ExtentHeight)
                    offset = ExtentHeight - ViewportHeight;
            }

            _scrollData.Offset.Y = offset;
            _translation.Y = -offset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public void Update()
        {
            Update(true);
        }

        public void Update(bool updateSectors)
        {
            if (!UseAnimatedStars)
                PauseAnimations();

            _children.Clear();

            ClearRouteData();
            
            if (updateSectors)
            {
                UpdateSectors();
                UpdateBorders();
            }
            
            UpdateRoutes();
            UpdateTradeLines();
            UpdateSelection();
            
            if (updateSectors)
                Composite();

            _children.Add(_composite);
            _children.Add(_sectors);
            _children.Add(_selectRect);
            _children.Add(_shipRange);
            _children.Add(_routeLines);
            _children.Add(_tradeLines);
            _children.Add(_starNames);
            _children.Add(_fleetIconCanvas);
        }

        public void ZoomIn()
        {
            ZoomIn(false);
        }

        private Point? GetZoomOrigin(bool zoomAroundMouse)
        {
            Point? zoomAroundPoint = null;
            if (zoomAroundMouse)
            {
                zoomAroundPoint = Mouse.GetPosition((IInputElement)ScrollOwner ?? this);
            }
            else if (SelectedSector != null)
            {
                var selectedSectorMidpoint = GetSectorMidpoint(SelectedSector.Location);
                if (ScrollOwner != null)
                    selectedSectorMidpoint = TransformToVisual(ScrollOwner).Transform(selectedSectorMidpoint);
                zoomAroundPoint = selectedSectorMidpoint;
            }
            return zoomAroundPoint;
        }

        public void ZoomIn(bool zoomAroundMouse)
        {
            ZoomIn(GetZoomOrigin(zoomAroundMouse));
        }

        public void ZoomOut()
        {
            ZoomOut(false);
        }

        public void ZoomOut(bool zoomAroundMouse)
        {
            ZoomOut(GetZoomOrigin(zoomAroundMouse));
        }

        public void ZoomIn(Point? zoomAroundPoint)
        {
            if (!CanZoomIn)
                return;
            var scaleFactor = ScaleFactor;
            if (scaleFactor % ZoomIncrement != 0)
                scaleFactor = Math.Round(scaleFactor, 1);
            scaleFactor += ZoomIncrement;
            if (scaleFactor > MaxScaleFactor)
                scaleFactor = MaxScaleFactor;
            SetScaleFactor(scaleFactor, zoomAroundPoint);
        }

        public void ZoomOut(Point? zoomAroundPoint)
        {
            if (!CanZoomOut)
                return;
            var scaleFactor = ScaleFactor;
            if (scaleFactor % ZoomIncrement != 0)
                scaleFactor = Math.Round(scaleFactor, 1);
            scaleFactor -= ZoomIncrement;
            if (scaleFactor < MinScaleFactor)
                scaleFactor = MinScaleFactor;
            SetScaleFactor(scaleFactor, zoomAroundPoint);
        }

        private Visual BuildTradeLine(TradeRoute route, Point endPoint, bool isNew)
        {
            var visual = new DrawingVisual();
            var geometry = new StreamGeometry();
            Pen pen;
            var isValid = false;

            var startPoint = GetSectorMidpoint(route.SourceColony.Location);

            if (isNew)
            {
                var endSector = PointToSector(endPoint);
                if ((endSector.System != null) && endSector.System.HasColony)
                {
                    endPoint = GetSectorMidpoint(endSector.Location);
                    if (IsValidTradeEndpoint(route, endSector.System.Colony))
                        isValid = true;
                }
                pen = isValid ? s_tradeRouteValidPen : s_tradeRouteInvalidPen;
            }
            else
            {
                endPoint = GetSectorMidpoint(route.TargetColony.Location);
                pen = s_tradeRouteSetPen;
            }

            using (var context = geometry.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.LineTo(endPoint, true, false);
            }

            geometry.Freeze();

            using (var dc = visual.RenderOpen())
            {
                dc.PushGuidelineSet(_halfGuides);
                dc.DrawGeometry(null, pen, geometry);
            }

            return visual;
        }

        private void ClearRouteData()
        {
            if (_newRoutePath != null)
                _children.Remove(_newRoutePath);
            if (_newRouteEta != null)
                _children.Remove(_newRouteEta);
            _newRoute = null;
            _newRoutePath = null;
            _newRouteEta = null;
            _lastSector = null;
            _waypoints.Clear();
        }

        private void Composite()
        {
            using (var drawingContext = _composite.RenderOpen())
            {
                drawingContext.DrawDrawing(_backdrop.Drawing);
                drawingContext.DrawDrawing(_borderLines.Drawing);
            }
            if (_composite.Drawing != null)
                _composite.Drawing.Freeze();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (VisualClip == null)
                return null;
            return new PointHitTestResult(
                VisualClip.Bounds.Contains(hitTestParameters.HitPoint) ? this : null,
                hitTestParameters.HitPoint);
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return new GeometryHitTestResult(
                hitTestParameters.HitGeometry.Bounds.IntersectsWith(VisualClip.Bounds) ? this : null,
                IntersectionDetail.Intersects);
        }

        private void CreateBackdrop()
        {
            var map = GameContext.Current.Universe.Map;
            var gridColor = Color.FromArgb(0x5F, 0x3F, 0x3F, 0x3F);
            var gridBrush = new SolidColorBrush(gridColor);
            gridBrush.Freeze();
            var gridPen = new Pen(gridBrush, 1.0);
            gridPen.Freeze();
            var axisPen = new Pen(Brushes.White, 1.0);
            axisPen.Freeze();
            using (var dc = _backdrop.RenderOpen())
            {
                dc.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    new Rect(0, 0, map.Width * SectorSize,
                             map.Height * SectorSize));
                dc.PushGuidelineSet(_guides);
                for (var x = 0; x < map.Width; x++)
                {
                    dc.DrawLine(
                        gridPen,
                        new Point(x * SectorSize, 0),
                        new Point(x * SectorSize,
                                  map.Height * SectorSize));
                }
                for (var y = 0; y < map.Height; y++)
                {
                    dc.DrawLine(
                        gridPen,
                        new Point(0, y * SectorSize),
                        new Point(map.Width * SectorSize,
                                  y * SectorSize));
                }
                // ReSharper disable PossibleLossOfFraction
                dc.DrawLine(
                    axisPen,
                    new Point((map.Width / 2) * SectorSize, 0),
                    new Point((map.Width / 2) * SectorSize,
                              map.Height * SectorSize));
                dc.DrawLine(
                    axisPen,
                    new Point(0, (map.Height / 2) * SectorSize),
                    new Point(map.Width * SectorSize,
                              (map.Height / 2) * SectorSize));
                // ReSharper restore PossibleLossOfFraction
            }
            if (_backdrop.Drawing != null)
                _backdrop.Drawing.Freeze();
        }

        private void CreateGuides()
        {
            _guides = new GuidelineSet();
            _halfGuides = new GuidelineSet();
            for (var x = 0; x <= SectorMap.MaxWidth; x++)
            {
                _guides.GuidelinesX.Add(x * SectorSize - 0.5);
                _halfGuides.GuidelinesX.Add(x * SectorSize - SectorSize / 2 - 0.5);
            }
            for (var y = 0; y <= SectorMap.MaxHeight; y++)
            {
                _guides.GuidelinesY.Add(y * SectorSize - 0.5);
                _halfGuides.GuidelinesY.Add(y * SectorSize - SectorSize / 2 - 0.5);
            }
            _guides.Freeze();
            _halfGuides.Freeze();
        }

        private void EnsureScrollData()
        {
            if (_scrollData == null)
                _scrollData = new ScrollData();
        }

        private double GetSectorSize()
        {
            return (SectorSize * ScaleFactor);
        }

        private Sector PointToSector(Point p)
        {
            var location = new MapLocation(
                Math.Min((int)(p.X / SectorSize), _galaxy.Width - 1),
                Math.Min((int)(p.Y / SectorSize), _galaxy.Height - 1));
            return _galaxy[location];
        }

        private void SetInputMode(GalaxyGridInputMode newInputMode)
        {
            var oldInputMode = _inputMode;
            if (oldInputMode == newInputMode)
                return;

            _inputMode = newInputMode;

            switch (oldInputMode)
            {
                case GalaxyGridInputMode.FleetMovement:
                    GalaxyScreenCommands.SelectTaskForce.Execute(null);
                    ClearRouteData();
                    ReleaseMouseCapture();
                    if (SelectedFleet != null)
                        PlayerActionEvents.FleetRouteUpdated.Publish(SelectedFleet);
                    break;
                case GalaxyGridInputMode.TradeRoute:
                    SelectedTradeRoute = null;
                    _newTradeLine = null;
                    ReleaseMouseCapture();
                    break;
                default:
                    break;
            }

            Update(false);
        }

        private void SetScaleFactor(double scaleFactor, Point? relativePoint)
        {
            // this works.
            var lastScaleFactor = ScaleFactor;
            var minScaleFactor = MinScaleFactor;
            var scaleRatio = scaleFactor / lastScaleFactor;

            if (scaleFactor < minScaleFactor)
                scaleFactor = minScaleFactor;
            else if (scaleFactor > MaxScaleFactor)
                scaleFactor = MaxScaleFactor;

            var transforms = new TransformGroup();

            var lastCenterPoint = new Point
            {
                X = (HorizontalOffset + ViewportWidth / 2),
                Y = (VerticalOffset + ViewportHeight / 2)
            };

            if (relativePoint.HasValue)
            {
                var p = relativePoint.Value;
                lastCenterPoint.X -= ((p.X - (ViewportWidth / 2)) * (1 - scaleRatio)) / scaleRatio;
                lastCenterPoint.Y -= ((p.Y - (ViewportHeight / 2)) * (1 - scaleRatio)) / scaleRatio;
            }

            lastCenterPoint.X /= lastScaleFactor;
            lastCenterPoint.Y /= lastScaleFactor;

            _scale.ScaleX = scaleFactor;
            _scale.ScaleY = scaleFactor;

            transforms.Children.Add(_scale);
            transforms.Children.Add(_translation);

            var effectiveFontSize = TransformToVisual(Application.Current.MainWindow)
                                        .TransformBounds(new Rect(new Size(0.0, StarNameFontSize)))
                                        .Height;

            _starNames.Opacity = (effectiveFontSize >= MinVisibleStarNameFontSize) ? 1.0 : 0.0;

            RenderTransform = transforms;

            UpdateLayout();

            if (relativePoint.HasValue)
                AutoCenterOnPoint(lastCenterPoint, false);
        }

        private void UpdateBorders()
        {
            var galaxy = GameContext.Current.Universe.Map;
            var dc = _borderLines.RenderOpen();
            
            dc.PushGuidelineSet(_guides);
            
            for (var x = 0; x < galaxy.Width; x++)
            {
                for (var y = 0; y < galaxy.Height; y++)
                {
                    var sector = galaxy[x, y];
                    
                    if (!IsScanned(sector.Location))
                        continue;
                    
                    var owner = GetPerceivedSectorOwner(sector);
                    if (owner == null)
                        continue;

                    var borderPen = (owner.IsEmpire)
                                        ? s_borderPens[owner.CivID]
                                        : s_minorRaceBorderPen;

                    var neighbor = sector.GetNeighbor(MapDirection.West);

                    if (neighbor == null ||
                        owner != GetPerceivedSectorOwner(neighbor))
                    {
                        dc.DrawLine(
                            borderPen,
                            new Point(SectorSize * x,
                                      SectorSize * y),
                            new Point(SectorSize * x,
                                      SectorSize * (y + 1)));
                    }

                    neighbor = sector.GetNeighbor(MapDirection.North);

                    if (neighbor == null ||
                        owner != GetPerceivedSectorOwner(neighbor))
                    {
                        dc.DrawLine(
                            borderPen,
                            new Point(SectorSize * x,
                                      SectorSize * y),
                            new Point(SectorSize * (x + 1),
                                      SectorSize * y));
                    }

                   neighbor = sector.GetNeighbor(MapDirection.East);

                    if (neighbor == null ||
                        owner != GetPerceivedSectorOwner(neighbor))
                    {
                        dc.DrawLine(
                            borderPen,
                            new Point(SectorSize * (x + 1),
                                      SectorSize * y),
                            new Point(SectorSize * (x + 1),
                                      SectorSize * (y + 1)));
                    }

                    neighbor = sector.GetNeighbor(MapDirection.South);

                    if (neighbor == null ||
                        owner != GetPerceivedSectorOwner(neighbor))
                    {
                        dc.DrawLine(
                            borderPen,
                            new Point(SectorSize * x,
                                      SectorSize * (y + 1)),
                            new Point(SectorSize * (x + 1),
                                      SectorSize * (y + 1)));
                    }
                }
            }

            dc.Close();
        }

        private static Civilization GetPerceivedSectorOwner(Sector sector)
        {
            var owner = (Civilization)null;
            var localPlayerEmpire = AppContext.LocalPlayer.Empire;
            var localPlayerEmpireManager = AppContext.LocalPlayerEmpire;
            var claims = AppContext.CurrentGame.SectorClaims;
            var system = sector.System;

            if (system != null && system.IsOwned)
            {
                owner = system.Owner;
            }
            else
            {
                var station = sector.Station;
                if ((station != null) && station.IsOwned)
                    owner = station.Owner;
            }

            if (owner == null)
                return claims.GetPerceivedOwner(sector.Location, localPlayerEmpire);

            if (localPlayerEmpireManager.MapData.IsExplored(sector.Location) ||
                Equals(owner, localPlayerEmpire) ||
                DiplomacyHelper.IsContactMade(owner, localPlayerEmpire))
            {
                return owner;
            }

            return claims.GetPerceivedOwner(sector.Location, localPlayerEmpire);
        }

        private void UpdateRoutes()
        {
            _routeLines.Children.Clear();
            _routePaths.Clear();
            if (PlayerCivilization == null)
                return;
            foreach (var fleet in GameContext.Current.Universe.FindOwned<Fleet>(PlayerCivilization))
            {
                var route = fleet.Route;
                if (!route.IsEmpty)
                {
                    var routePath = BuildRoutePath(fleet, route);
                    _routePaths[fleet] = routePath;
                    _routeLines.Children.Add(routePath);
                }
            }
        }

        private static Color SetAlpha(Color c, byte alpha)
        {
            var copy = c;
            copy.A = alpha;
            return copy;
        }

        private Pen GetScanPen(Civilization civ)
        {
            if (_scanPen != null)
                return _scanPen;

            var color = Avalon.Windows.Utility.ColorHelpers.Lighten(
                (Color)ColorConverter.ConvertFromString(civ.Color),
                0.67f);

            var scanBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0,0),
                EndPoint = new Point(SectorSize, SectorSize * 2),
                SpreadMethod = GradientSpreadMethod.Reflect,
                MappingMode = BrushMappingMode.Absolute,
                GradientStops =
                    {                                        
                        new GradientStop(SetAlpha(color, 0x66), 0.0),
                        new GradientStop(SetAlpha(color, 0x33), 0.2),
                        new GradientStop(SetAlpha(color, 0x66), 0.3),
                        new GradientStop(SetAlpha(color, 0xcc), 0.4),
                        new GradientStop(SetAlpha(color, 0x66), 0.6),
                        new GradientStop(SetAlpha(color, 0x33), 0.7),
                        new GradientStop(SetAlpha(color, 0x66), 0.8),
                        new GradientStop(SetAlpha(color, 0xaa), 1.0),
                    }
            };

            var transform = new TranslateTransform(0d, 0d);

            var animationX = new DoubleAnimation(
                0d,
                scanBrush.EndPoint.X * 2,
                new Duration(
                    TimeSpan.FromSeconds(7.5)))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };

            var animationY = new DoubleAnimation(
                0d,
                scanBrush.EndPoint.Y * 2,
                new Duration(
                    TimeSpan.FromSeconds(7.5)))
                    {
                        RepeatBehavior = RepeatBehavior.Forever
                    };

            var clockX = animationX.CreateClock();
            var clockY = animationY.CreateClock();

            transform.ApplyAnimationClock(
                TranslateTransform.XProperty,
                clockX);

            transform.ApplyAnimationClock(
                TranslateTransform.YProperty,
                clockY);

            scanBrush.Transform = transform;

            _scanPen = new Pen(scanBrush, 1.5);

            if (_animationClocks == null)
                _animationClocks = new List<Clock>();

            _animationClocks.Add(clockX);
            _animationClocks.Add(clockY);

            return _scanPen;
        }

        private void UpdateSectors()
        {
            var playerCiv = PlayerCivilization;
            var scanPen = GetScanPen(playerCiv);
            var map = GameContext.Current.Universe.Map;
            var sectorClaims = GameContext.Current.SectorClaims;
            var mapData = AppContext.LocalPlayerEmpire.MapData;

            var fleetLookup = (from item in GameContext.Current.Universe.Objects
                               where item.ObjectType == UniverseObjectType.Fleet
                               select (Fleet)item).ToLookup(o => o.Location,
                                                            o => FleetView.Create(playerCiv, o));

            _sectors.Children.Clear();
            _starNames.Children.Clear();
            _fleetIconCanvas.Children.Clear();

            while (_fleetIconAdorners.Count != 0)
            {
                _fleetIconAdorners[0].Dispose();
                _fleetIconAdorners.RemoveAt(0);
            }

            using (DrawingContext dc = _sectors.RenderOpen(), dcStarNames = _starNames.RenderOpen())
            {
                var scanLines = new StreamGeometry();
                var slc = scanLines.Open();

                dc.PushGuidelineSet(_guides);
                for (var x = 0; x < map.Width; x++)
                {
                    for (var y = 0; y < map.Height; y++)
                    {
                        var isContactMadeWithOwner = false;
                        var sector = map[x, y];
                        var system = sector.System;
                        var owner = GetPerceivedSectorOwner(sector);
                        var location = sector.Location;

                        /*******************
                         * DRAW FOG OF WAR *
                         *******************/
                        if (!mapData.IsScanned(location))
                        {
                            dc.DrawRectangle(
                                s_fogOfWarBrush,
                                null,
                                new Rect(new Point(SectorSize * location.X,
                                                   SectorSize * location.Y),
                                         new Point(SectorSize * (location.X + 1),
                                                   SectorSize * (location.Y + 1))));
                            continue;
                        }

                        if (owner != null)
                            isContactMadeWithOwner = true;

                        /**************************************
                         * DRAW SECTOR FILL FOR OWNED SECTORS *
                         **************************************/
                        if (((system == null) || !system.HasColony) && sectorClaims.IsDisputed(location, playerCiv))
                        {
                            dc.DrawRectangle(
                                s_disputedSectorFill,
                                null,
                                new Rect(new Point(SectorSize * location.X,
                                                   SectorSize * location.Y),
                                         new Point(SectorSize * (location.X + 1),
                                                   SectorSize * (location.Y + 1))));
                        }
                        else if (isContactMadeWithOwner)
                        {

                            SolidColorBrush sectorBrush;
                            if (owner.CivilizationType == CivilizationType.Empire)
                            {
                                sectorBrush = s_colonyFills[owner.CivID];
                            }
                            else
                            {
                                sectorBrush = s_minorRaceFill;
                            }

                            dc.DrawRectangle(
                                sectorBrush,
                                null,
                                new Rect(
                                    new Point(
                                        SectorSize * location.X,
                                        SectorSize * location.Y),
                                    new Point(
                                        SectorSize * (location.X + 1),
                                        SectorSize * (location.Y + 1))));
                        }

                        var scanStrength = mapData.GetScanStrength(location);

                        /*************************
                         * DRAW SCAN RANGE LINES *
                         *************************/

                        if ((scanStrength > 0) &&
                            ((location.X == (map.Width - 1)) || (mapData.GetScanStrength(sector.GetNeighbor(MapDirection.East).Location) <= 0)))
                        {
                            slc.BeginFigure(new Point(SectorSize * (location.X + 1),
                                                     SectorSize * location.Y),
                                           false, false);
                            slc.LineTo(new Point(SectorSize * (location.X + 1),
                                                SectorSize * (location.Y + 1)), true,
                                      false);
                        }
                        if (scanStrength > 0)
                        {
                            slc.BeginFigure(new Point(SectorSize * location.X,
                                          SectorSize * location.Y),
                                           false, false);
                            slc.LineTo(new Point(SectorSize * location.X,
                                          SectorSize * (location.Y + 1)), true,
                                      false);
                            slc.BeginFigure(new Point(SectorSize * location.X,
                                          SectorSize * location.Y),
                                           false, false);
                            slc.LineTo(new Point(SectorSize * (location.X + 1),
                                          SectorSize * location.Y), true,
                                      false);
                        }
                        if ((scanStrength > 0)
                            && ((location.Y == (map.Height - 1))
                                || (mapData.GetScanStrength(sector.GetNeighbor(MapDirection.South).Location) <= 0)))
                        {
                            slc.BeginFigure(new Point(SectorSize * location.X,
                                          SectorSize * (location.Y + 1)),
                                           false, false);
                            slc.LineTo(new Point(SectorSize * (location.X + 1),
                                          SectorSize * (location.Y + 1)), true,
                                      false);
                        }
                        /***************************
                         * DRAW STARS & STAR NAMES *
                         ***************************/
                        if (system != null)
                        {
                            var p = new Point(SectorSize * location.X,
                                                SectorSize * location.Y);
                            double topMargin = 4;
                            if (mapData.IsScanned(location))
                            {
                                if (UseAnimatedStars)
                                {
                                    if (_starBrushes == null)
                                    {
                                        _starBrushes = new Dictionary<StarType, ImageBrush>();
                                    }
                                    if (_animationClocks == null)
                                    {
                                        _animationClocks = new List<Clock>();
                                    }
                                    if (!_starBrushes.ContainsKey(sector.System.StarType))
                                    {
                                        AnimationClock clock;
                                        var brush = new ImageBrush(s_starImages[sector.System.StarType]);
                                        RenderOptions.SetCacheInvalidationThresholdMinimum(brush, 0.5);
                                        RenderOptions.SetCacheInvalidationThresholdMaximum(brush, 2.0);
                                        RenderOptions.SetCachingHint(brush, CachingHint.Cache);
                                        if (sector.System.StarType == StarType.Nebula)
                                        {
                                            var opacityAnim = new DoubleAnimation(
                                                1.0, 0.5, new Duration(new TimeSpan(0, 0, 3)))
                                            {
                                                AutoReverse = true,
                                                RepeatBehavior = RepeatBehavior.Forever
                                            };
                                            clock = opacityAnim.CreateClock();
                                            _animationClocks.Add(clock);
                                            if (!UseAnimatedStars && clock.Controller != null)
                                                clock.Controller.Pause();
                                            brush.ApplyAnimationClock(Brush.OpacityProperty, clock);
                                        }
                                        else
                                        {
                                            if (StarHelper.SupportsPlanets(sector.System))
                                            {
                                                var grow = new ScaleTransform(1.0, 1.0, 0.5, 0.5);
                                                var growAnimation = new DoubleAnimation(
                                                    0.85, 1.25, new Duration(new TimeSpan(0, 0, 2)))
                                                {
                                                    RepeatBehavior = RepeatBehavior.Forever,
                                                    AutoReverse = true,
                                                    AccelerationRatio = 1.0
                                                };
                                                clock = growAnimation.CreateClock();
                                                if (clock.Controller != null)
                                                {
                                                    clock.Controller.Seek(
                                                        new TimeSpan(
                                                            0,
                                                            0,
                                                            0,
                                                            0,
                                                            500 * (int)sector.System.StarType),
                                                        TimeSeekOrigin.BeginTime);
                                                }
                                                _animationClocks.Add(clock);
                                                grow.ApplyAnimationClock(ScaleTransform.ScaleXProperty, clock);
                                                grow.ApplyAnimationClock(ScaleTransform.ScaleYProperty, clock);
                                                brush.RelativeTransform = grow;
                                            }
                                        }
                                        _starBrushes.Add(sector.System.StarType, brush);
                                    }
                                    dc.DrawRectangle(_starBrushes[sector.System.StarType], null, new Rect(p, new Size(SectorSize, SectorSize)));
                                }
                                else
                                {
                                    if (_starBrushes != null)
                                    {
                                        _starBrushes.Clear();
                                        _starBrushes = null;
                                    }
                                    if (_animationClocks != null)
                                    {
                                        _animationClocks.Clear();
                                        _animationClocks = null;
                                    }
                                    dc.DrawImage(s_starImages[system.StarType],
                                                 new Rect(p, new Size(SectorSize, SectorSize)));
                                }

                                if (IsStarNameVisible(system))
                                {
                                    var starName = GetStarText(system, playerCiv);
                                    if (starName.Height > starName.LineHeight)
                                        topMargin = -2;
                                    dcStarNames.DrawText(
                                        starName,
                                        new Point(p.X + 3,
                                                  p.Y + Math.Round(SectorSize * 2.0 / 3.0) + topMargin));
                                }
                            }
                        }
                        /*************************
                         * DRAW FLEET INDICATORS *
                         *************************/
                        var fleetsAtLocation = fleetLookup[location]
                            .Where(f => f.IsPresenceKnown)
                            .GroupBy(f => f.Source.Owner)
                            .ToList();

                        const double maxIconsWidth = (SectorSize * MaxScaleFactor) + FleetIconSpacing;

                        var consumedWidth = 0d;

                        while (fleetsAtLocation.Count != 0)
                        {
                            FleetIconAdorner adorner;

                            if (consumedWidth + FleetIconSize > maxIconsWidth)
                            {
                                adorner = new FleetIconAdorner(
                                    playerCiv,
                                    location,
                                    fleetsAtLocation.Select(o => o.Key).ToArray());

                                fleetsAtLocation.Clear();
                            }
                            else
                            {
                                adorner = new FleetIconAdorner(
                                    playerCiv,
                                    location,
                                    new[] { fleetsAtLocation[0].Key });

                                fleetsAtLocation.RemoveAt(0);
                            }

                            Canvas.SetLeft(
                                adorner,
                                (location.X * SectorSize) + FleetIconSpacing + consumedWidth);

                            Canvas.SetTop(
                                adorner,
                                (location.Y * SectorSize) + FleetIconSpacing);

                            _fleetIconCanvas.Children.Add(adorner);
                            _fleetIconAdorners.Add(adorner);

                            consumedWidth += (FleetIconSize + FleetIconSpacing);
                        }

                        /***************************
                         * DRAW STATION INDICATORS *
                         ***************************/
                        var station = sector.Station;
                        if ((station != null) &&
                            ((station.Owner == playerCiv) || (scanStrength > 0)))
                        {
                            int brushId = station.OwnerID;
                            Pen sPen;
                            if ((station.Owner != playerCiv) && !DiplomacyHelper.IsContactMade(station.Owner, playerCiv)) {
                                sPen = new Pen(Brushes.White, 1.0);
                            } else {
                                sPen = new Pen(s_colonyNameBrushes[brushId], 1.0);
                            }
                            var sText = new FormattedText(
                                "S",
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                s_textTypeface,
                                16.0,
                                Brushes.White,
                                VisualTreeHelper.GetDpi(this).PixelsPerDip);
                            var sGeom = sText.BuildGeometry(
                                new Point(SectorSize * location.X + 5,
                                            SectorSize * (location.Y + 1) - 5
                                                - sText.Height + (sText.Height - sText.Baseline)));
                            dc.DrawGeometry(Brushes.White, sPen, sGeom);
                        }

                        /***************************
                         * DRAW TradeRoute INDICATORS *
                         ***************************/

                        if (sector.TradeRouteIndicator != 99 && sector.TradeRouteIndicator != 0)
                        {
                            if (!mapData.IsScanned(location))
                            {
                                // my plan is: 
                                // yellow "2": 2 TradeRoutes done, 2 possible
                                // green "1": 1 TradeRoutes done, 2 possible -> one available

                                Pen tPen;

                                tPen = new Pen(Brushes.Green, 1.0);

                                int tradeRouteAvailable = sector.TradeRouteIndicator - sector.System.Colony.TradeRoutes.Count();
                                GameLog.Core.TradeRoutes.DebugFormat("TradeRoutes for Sector {0}: Available: {1}, Possible: {2}, Used: {3}", sector.Location,  tradeRouteAvailable, sector.TradeRouteIndicator, sector.System.Colony.TradeRoutes.Count());

                                var tText = new FormattedText(
                                    "T:" + sector.TradeRouteIndicator.ToString(),
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    s_textTypeface,
                                    12.0,
                                    Brushes.White,
                                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                                var tGeom = tText.BuildGeometry(
                                    new Point(SectorSize * location.X + 50,
                                                SectorSize * (location.Y + 1) - 40
                                                    - tText.Height + (tText.Height - tText.Baseline)));
                                dc.DrawGeometry(Brushes.White, tPen, tGeom);
                            }
                        }
                    }
                }
                slc.Close();
                dc.DrawGeometry(null, scanPen, scanLines);
            }
        }

        [NotNull]
        internal static BitmapImage GetFleetIcon(Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (s_fleetIcons.ContainsKey(civ.CivID))
                return s_fleetIcons[civ.CivID];
            return s_defaultFleetIcon;
        }

        [NotNull]
        internal static BitmapImage GetMultiFleetIcon()
        {
            return s_multiFleetIcon;
        }

        [NotNull]
        internal static BitmapImage GetUnknownFleetIcon()
        {
            return s_unknownFleetIcon;
        }

        private void UpdateSelection()
        {
            using (var selectRect = _selectRect.RenderOpen())
            {
                if (SelectedSector != null)
                {
                    selectRect.DrawRectangle(
                        null,
                        new Pen(Brushes.White, 2.0),
                        new Rect(
                            new Point(
                                SelectedSector.Location.X * SectorSize + 2.0,
                                SelectedSector.Location.Y * SectorSize + 2.0),
                            new Size(SectorSize - 3, SectorSize - 3)));
                }
                selectRect.Close();
            }
        }

        private void UpdateTradeLines()
        {
            _tradeLines.Children.Clear();
            if (PlayerCivilization == null)
                return;
            foreach (var colony in GameContext.Current.Universe.FindOwned<Colony>(PlayerCivilization))
            {
                foreach (var route in colony.TradeRoutes)
                {
                    if (route.TargetColony != null)
                        _tradeLines.Children.Add(BuildTradeLine(route, default(Point), false));
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (IsScrolling)
            {
                var extent = new Size(
                    GetSectorSize() * _galaxy.Width,
                    GetSectorSize() * _galaxy.Height);
                if (extent != _scrollData.Extent)
                {
                    _scrollData.Extent = extent;
                    if (_scrollData.ScrollOwner != null)
                        _scrollData.ScrollOwner.InvalidateScrollInfo();
                }
                if (finalSize != _scrollData.Viewport)
                {
                    _scrollData.Viewport = finalSize;
                    if (_scrollData.ScrollOwner != null)
                        _scrollData.ScrollOwner.InvalidateScrollInfo();
                }
                finalSize = new Size(
                    Math.Min(extent.Width, finalSize.Width),
                    Math.Min(extent.Height, finalSize.Height));
            }
            _fleetIconCanvas.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (IsScrolling)
            {
                var extent = new Size(
                    GetSectorSize() * _galaxy.Width,
                    GetSectorSize() * _galaxy.Height);
                if (extent != _scrollData.Extent)
                {
                    if (_scrollData.ScrollOwner != null)
                        _scrollData.ScrollOwner.InvalidateScrollInfo();
                }
                if (availableSize != _scrollData.Viewport)
                {
                    _scrollData.Viewport = availableSize;
                    if (_scrollData.ScrollOwner != null)
                        _scrollData.ScrollOwner.InvalidateScrollInfo();
                }
                return new Size(Math.Min(extent.Width, availableSize.Width),
                                Math.Min(extent.Height, availableSize.Height));
            }
            return new Size(_galaxy.Width * GetSectorSize(),
                            _galaxy.Height * GetSectorSize());
        }
        #endregion

        #region Mouse Handlers
        [Flags]
        private enum ScreenEdges
        {
            None = 0x00,
            Top = 0x01,
            Left = 0x02,
            Right = 0x04,
            Bottom = 0x08
        }

        private ScreenEdges GetMouseScreenEdges()
        {
            const double edgeWidth = 16d;

            var edges = ScreenEdges.None;
            var position = Mouse.GetPosition(this);

            var leftEdge = _scrollData.Offset.X / ScaleFactor;
            var topEdge = _scrollData.Offset.Y / ScaleFactor;
            var rightEdge = leftEdge + _scrollData.Viewport.Width / ScaleFactor;
            var bottomEdge = topEdge + _scrollData.Viewport.Height / ScaleFactor;

            var leftThreshold = leftEdge + edgeWidth;
            var topThreshold = topEdge + edgeWidth;
            var rightThreshold = rightEdge - edgeWidth;
            var bottomThreshold = bottomEdge - edgeWidth;

            if (position.X >= leftEdge && position.X <= leftThreshold)
                edges |= ScreenEdges.Left;
            else if (position.X <= rightEdge && position.X >= rightThreshold)
                edges |= ScreenEdges.Right;

            if (position.Y >= topEdge && position.Y <= topThreshold)
                edges |= ScreenEdges.Top;
            else if (position.Y <= bottomEdge && position.Y >= bottomThreshold)
                edges |= ScreenEdges.Bottom;

            return edges;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            SetInputMode(GalaxyGridInputMode.Default);
            _autoScrollTimer.IsEnabled = false;
        }

        private void OnAutoScrollTimerTick(object sender, EventArgs eventArgs)
        {
            var edges = GetMouseScreenEdges();

            if ((edges & ScreenEdges.Left) == ScreenEdges.Left)
                LineLeft();
            else if ((edges & ScreenEdges.Right) == ScreenEdges.Right)
                LineRight();

            if ((edges & ScreenEdges.Top) == ScreenEdges.Top)
                LineUp();
            else if ((edges & ScreenEdges.Bottom) == ScreenEdges.Bottom)
                LineDown();

            _autoScrollTimer.Interval = TimeSpan.FromSeconds(0.2d);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if ((InputModeOnFirstClick == GalaxyGridInputMode.Default)
                && (InputMode == GalaxyGridInputMode.Default))
            {
                if (ScrollOwner != null)
                {
                    AutoScrollToSector(SelectedSector);
                    e.Handled = true;
                }
                if ((SelectedSector != null)
                    && (SelectedSector.System != null)
                    && SelectedSector.System.HasColony
                    && (SelectedSector.System.Colony.Owner == AppContext.LocalPlayer.Empire))
                {
                    OnSectorDoubleClicked(SelectedSector);
                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Handled)
                return;
            Keyboard.Focus(this);
            if ((InputMode != GalaxyGridInputMode.Default) &&
                (InputHitTest(e.GetPosition(this)) == null))
            {
                /*
                 * If the player clicks the mouse anywhere outside of the grid while we are in fleet
                 * movement mode or setting a trade route, we abort the operation and go back to normal
                 * input mode.
                 */
                SetInputMode(GalaxyGridInputMode.Default);
                return;
            }
            if (e.ClickCount == 1)
            {
                InputModeOnFirstClick = InputMode;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var clickSector = PointToSector(e.GetPosition(this));
                if (_inputMode == GalaxyGridInputMode.FleetMovement)
                {
                    var fleet = SelectedFleet;
                    if ((fleet != null) && (_newRoute != null))
                    {
                        if ((_waypoints.Count == 0) || (_waypoints[_waypoints.Count - 1] != clickSector))
                        {
                            if (_newRoutePath != null)
                                _children.Remove(_newRoutePath);
                            if (_newRouteEta != null)
                                _children.Remove(_newRouteEta);
                            _waypoints.Add(clickSector);
                            _newRoute = AStar.FindPath(SelectedFleet, _waypoints);
                            if (!_newRoute.IsEmpty)
                            {
                                _newRoutePath = BuildRoutePath(SelectedFleet, _newRoute);
                                _children.Add(_newRoutePath);
                                _newRouteEta = BuildRouteETA(SelectedFleet, _newRoute);
                                if (_newRouteEta != null)
                                    _children.Add(_newRouteEta);
                            }
                        }
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            var setRoute = true;
                            if (!_newRoute.IsEmpty && fleet.Order.IsCancelledOnRouteChange)
                            {
                                var confirmResult = MessageDialog.Show(
                                    ResourceManager.GetString("CONFIRM_NEW_ROUTE_HEADER"),
                                    String.Format(
                                        ResourceManager.GetString("CONFIRM_NEW_ROUTE_MESSAGE"),
                                        fleet.Order.OrderName),
                                    MessageDialogButtons.YesNo);
                                if (confirmResult != MessageDialogResult.Yes)
                                    setRoute = false;
                            }
                            if (setRoute)
                            {
                                fleet.SetRoute(_newRoute);
                                PlayerOrderService.AddOrder(new SetFleetRouteOrder(fleet));
                                PlayerActionEvents.FleetRouteUpdated.Publish(fleet);
                            }
                            SetInputMode(GalaxyGridInputMode.Default);
                        }
                        return;
                    }
                }
                else if (_inputMode == GalaxyGridInputMode.TradeRoute)
                {
                    var route = SelectedTradeRoute;
                    var targetChanged = false;
                    if ((clickSector.System != null)
                        && IsValidTradeEndpoint(route, clickSector.System.Colony))
                    {
                        targetChanged = true;
                        route.TargetColony = clickSector.System.Colony;
                        GameEvents.TradeRouteEstablished.Publish(route);
                    }
                    else if (route.IsAssigned)
                    {
                        targetChanged = true;
                        route.TargetColony = null;
                        GameEvents.TradeRouteCancelled.Publish(route);
                    }
                    if (targetChanged)
                    {
                        PlayerOrderService.AddOrder(new SetTradeRouteOrder(route));
                    }
                    SetInputMode(GalaxyGridInputMode.Default);
                }
                else
                {
                    SelectedSector = clickSector;
                    UpdateSelection();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_inputMode == GalaxyGridInputMode.Default)
                return;

            var hoverSector = PointToSector(e.GetPosition(this));

            if (_inputMode == GalaxyGridInputMode.FleetMovement)
            {
                var fleet = SelectedFleet;

                if (fleet == null)
                {
                    _inputMode = GalaxyGridInputMode.Default;
                    return;
                }

                if (_routePaths.ContainsKey(fleet))
                    _routeLines.Children.Remove(_routePaths[fleet]);

                if ((_waypoints.Count == 0) || (hoverSector != _lastSector))
                {
                    var newWaypoints = new List<Sector>(_waypoints)
                                       {
                                           hoverSector
                                       };
                    _newRoute = AStar.FindPath(SelectedFleet, newWaypoints);
                    if (_newRoutePath != null)
                        _children.Remove(_newRoutePath);
                    _newRoutePath = BuildRoutePath(SelectedFleet, _newRoute);
                    _children.Add(_newRoutePath);
                    if (_newRouteEta != null)
                        _children.Remove(_newRouteEta);
                    if (_newRoute.Length > 0)
                    {
                        _newRouteEta = BuildRouteETA(SelectedFleet, _newRoute);
                        if (_newRouteEta != null)
                            _children.Add(_newRouteEta);
                    }
                }
            }
            else if (_inputMode == GalaxyGridInputMode.TradeRoute)
            {
                if (_newTradeLine != null)
                    _tradeLines.Children.Remove(_newTradeLine);
                _newTradeLine = BuildTradeLine(SelectedTradeRoute, e.GetPosition(this), true);
                _tradeLines.Children.Add(_newTradeLine);
            }
            _lastSector = hoverSector;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if ((SelectedFleet == null)
                && (e.LeftButton == MouseButtonState.Pressed)
                && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                // Save starting point, used later when determining how much to scroll.
                _isDragScrollInProgress = true;
                _scrollStartPoint = e.GetPosition(ScrollOwner);
                _scrollStartOffset.X = HorizontalOffset;
                _scrollStartOffset.Y = VerticalOffset;

                // Update the cursor if can scroll or not.
                if ((_scrollData.Extent.Width > _scrollData.Viewport.Width)
                    || (_scrollData.Extent.Height > _scrollData.Viewport.Height))
                    Cursor = Cursors.ScrollAll;

                CaptureMouse();

                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_isDragScrollInProgress)
            {
                // Get the new scroll position.
                var point = e.GetPosition(ScrollOwner);

                // Determine the new amount to scroll.
                var delta = new Point(
                    (point.X > _scrollStartPoint.X)
                        ? -(point.X - _scrollStartPoint.X)
                        : (_scrollStartPoint.X - point.X),
                    (point.Y > _scrollStartPoint.Y)
                        ? -(point.Y - _scrollStartPoint.Y)
                        : (_scrollStartPoint.Y - point.Y));

                // Scroll to the new position.
                SetHorizontalOffset(_scrollStartOffset.X + delta.X);
                SetVerticalOffset(_scrollStartOffset.Y + delta.Y);

                e.Handled = true;
            }
            else if (_inputMode != GalaxyGridInputMode.Default &&
                     !_autoScrollTimer.IsEnabled)
            {
                _autoScrollTimer.Interval = TimeSpan.FromSeconds(1d);
                _autoScrollTimer.IsEnabled = GetMouseScreenEdges() != ScreenEdges.None;
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (_isDragScrollInProgress)
            {
                ClearValue(CursorProperty);
                ReleaseMouseCapture();
                _isDragScrollInProgress = false;
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn(true);
            else if (e.Delta < 0)
                ZoomOut(true);
            e.Handled = true;
        }
        #endregion

        #region Event Handlers
        private void AutoCenterOnPoint(Point offset, bool animate)
        {
            offset.X *= ScaleFactor;
            offset.Y *= ScaleFactor;

            offset.X = offset.X - ViewportWidth / 2;
            offset.Y = offset.Y - ViewportHeight / 2;

            if (offset.X < 0)
                offset.X = 0;
            else if ((offset.X + ViewportWidth) > ExtentWidth)
                offset.X = ExtentWidth - ViewportWidth;

            if (offset.Y < 0)
                offset.Y = 0;
            else if ((offset.Y + ViewportHeight) > ExtentHeight)
                offset.Y = ExtentHeight - ViewportHeight;

            _scrollData.Offset.X = offset.X;
            _scrollData.Offset.Y = offset.Y;

            ScrollOwner.InvalidateScrollInfo();

            if (animate)
            {
                var xAnim = new DoubleAnimation(
                    _translation.X,
                    -_scrollData.Offset.X,
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    FillBehavior.Stop)
                                        {
                                            AccelerationRatio = 0.5,
                                            DecelerationRatio = 0.5
                                        };

                var yAnim = new DoubleAnimation(
                    _translation.Y,
                    -_scrollData.Offset.Y,
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    FillBehavior.Stop)
                                        {
                                            AccelerationRatio = 0.5,
                                            DecelerationRatio = 0.5
                                        };

                _translation.BeginAnimation(TranslateTransform.XProperty, xAnim);
                _translation.BeginAnimation(TranslateTransform.YProperty, yAnim);
            }

            _translation.X = -_scrollData.Offset.X;
            _translation.Y = -_scrollData.Offset.Y;
        }

        private void AutoScrollToSector(Sector sector)
        {
            var offset = GetSectorMidpoint(sector.Location);
            AutoCenterOnPoint(offset, true);
        }

        private void GalaxyGridPanel_OptionsChanged(object sender, DependencyPropertyChangedEventArgs<GalaxyViewOptions> e)
        {
            _starNames.Opacity = Options.StarNames ? 1.0 : 0.0;
            Composite();
        }

        private void GalaxyGridPanel_SelectedFleetChanged(object sender, DependencyPropertyChangedEventArgs<Fleet> e)
        {
            if ((e.NewValue != null) && !e.NewValue.IsStranded)
            {
                SetInputMode(GalaxyGridInputMode.FleetMovement);
                if (!Focus() && (Parent is Control))
                    ((Control)Parent).Focus();
            }
            else if (_inputMode == GalaxyGridInputMode.FleetMovement)
                SetInputMode(GalaxyGridInputMode.Default);
            _shipRange.Opacity = 0;
            _shipRange.Children.Clear();
            if (e.NewValue != null)
            {
                using (var dc = _shipRange.RenderOpen())
                {
                    var pen = new Pen(Brushes.Yellow, 2.0);
                    var fleet = e.NewValue;
                    const int lineLength = (int)SectorSize;
                    pen.DashStyle = DashStyles.Dash;
                    for (var x = 0; x < _galaxy.Width; x++)
                    {
                        for (var y = 0; y < _galaxy.Height; y++)
                        {
                            if (IsSectorInRange(fleet, new MapLocation(x, y)))
                            {
                                if ((x == 0)
                                    || !IsSectorInRange(fleet, new MapLocation(x - 1, y)))
                                {
                                    dc.DrawLine(
                                        pen,
                                        new Point(x * lineLength,
                                                  y * lineLength),
                                        new Point(x * lineLength,
                                                  y * lineLength + lineLength));
                                }
                                if ((y == 0)
                                    || !IsSectorInRange(fleet, new MapLocation(x, y - 1)))
                                {
                                    dc.DrawLine(
                                        pen,
                                        new Point(x * lineLength,
                                                  y * lineLength),
                                        new Point(x * lineLength + lineLength,
                                                  y * lineLength));
                                }
                                if ((x == (_galaxy.Width - 1))
                                    || !IsSectorInRange(fleet, new MapLocation(x + 1, y)))
                                {
                                    dc.DrawLine(
                                        pen,
                                        new Point(x * lineLength + lineLength,
                                                  y * lineLength),
                                        new Point(x * lineLength + lineLength,
                                                  y * lineLength + lineLength));
                                }
                                if ((y == (_galaxy.Height - 1))
                                    || !IsSectorInRange(fleet, new MapLocation(x, y + 1)))
                                {
                                    dc.DrawLine(
                                        pen,
                                        new Point(x * lineLength,
                                                  y * lineLength + lineLength),
                                        new Point(x * lineLength + lineLength,
                                                  y * lineLength + lineLength));
                                }
                            }
                        }
                    }
                }
                _shipRange.Opacity = 1;
            }
        }

        private void GalaxyGridPanel_SelectedTradeRouteChanged(object sender, DependencyPropertyChangedEventArgs<TradeRoute> e)
        {
            if (e.NewValue != null)
            {
                SetInputMode(GalaxyGridInputMode.TradeRoute);
                if (!Focus() && (Parent is Control))
                    ((Control)Parent).Focus();
            }
            else if (_inputMode == GalaxyGridInputMode.TradeRoute)
                SetInputMode(GalaxyGridInputMode.Default);
        }

        private void GalaxyGridPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetScaleFactor(ScaleFactor, null);
        }

        private void OnSectorDoubleClicked(Sector sector)
        {
            if ((sector != null) && (SectorDoubleClicked != null))
                SectorDoubleClicked(sector);
        }
        #endregion

        #region IScrollInfo Members
        public bool CanHorizontallyScroll
        {
            get
            {
                if (_scrollData == null)
                    return false;
                return _scrollData.AllowHorizontal;
            }
            set
            {
                EnsureScrollData();
                if (_scrollData.AllowHorizontal != value)
                {
                    _scrollData.AllowHorizontal = value;
                    InvalidateMeasure();
                }
            }
        }

        public bool CanVerticallyScroll
        {
            get
            {
                if (_scrollData == null)
                    return false;
                return _scrollData.AllowVertical;
            }
            set
            {
                EnsureScrollData();
                if (_scrollData.AllowVertical != value)
                {
                    _scrollData.AllowVertical = value;
                    InvalidateMeasure();
                }
            }
        }

        public double ExtentHeight
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Extent.Height;
            }
        }

        public double ExtentWidth
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Extent.Width;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Offset.X;
            }
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + GetSectorSize() + _scale.ScaleY, true);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - GetSectorSize() - _scale.ScaleX, true);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + GetSectorSize() + _scale.ScaleX, true);
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - GetSectorSize() - _scale.ScaleY, true);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        public void MouseWheelDown()
        {
            LineDown();
        }

        public void MouseWheelLeft()
        {
            LineLeft();
        }

        public void MouseWheelRight()
        {
            LineRight();
        }

        public void MouseWheelUp()
        {
            LineUp();
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight, true);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth, true);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth, true);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight, true);
        }

        public ScrollViewer ScrollOwner
        {
            get
            {
                EnsureScrollData();
                return _scrollData.ScrollOwner;
            }
            set
            {
                EnsureScrollData();
                if (value != _scrollData.ScrollOwner)
                {
                    ResetScrolling(this);
                    _scrollData.ScrollOwner = value;
                }
            }
        }

        public void SetVerticalOffset(double offset)
        {
            SetVerticalOffset(offset, false);
        }

        public void SetHorizontalOffset(double offset)
        {
            SetHorizontalOffset(offset, false);
        }

        public double VerticalOffset
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Offset.Y;
            }
        }

        public double ViewportHeight
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Viewport.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                if (_scrollData == null)
                    return 0;
                return _scrollData.Viewport.Width;
            }
        }
        #endregion

        #region ScrollData Class
        private class ScrollData
        {
            #region Fields
            internal bool AllowHorizontal;
            internal bool AllowVertical;
            internal Size Extent;
            internal Vector Offset;
            internal ScrollViewer ScrollOwner;
            internal Size Viewport;
            #endregion

            #region Methods
            internal void ClearLayout()
            {
                Offset = new Vector();
                Extent = new Size();
                Viewport = new Size();
            }
            #endregion
        }
        #endregion

        #region GalaxyGridInputMode Enumeration
        public enum GalaxyGridInputMode
        {
            Default,
            FleetMovement,
            TradeRoute
        }
        #endregion

        #region GalaxyViewOptions Class
        public struct GalaxyViewOptions
        {
            #region Fields
            public static readonly GalaxyViewOptions Default;
            #endregion

            #region Constructors
            static GalaxyViewOptions()
            {
                Default = new GalaxyViewOptions
                {
                    Fleets = true,
                    TradeRoutes = true,
                    StarNames = true
                };
            }
            #endregion

            #region Properties
            public bool Fleets { get; set; }
            public bool StarNames { get; set; }
            public bool TradeRoutes { get; set; }
            #endregion
        }
        #endregion
    }
}