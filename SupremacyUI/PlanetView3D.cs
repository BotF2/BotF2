// PlanetView3D.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Client;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.UI
{
    public enum LightSourceDirection
    {
        Right,
        Left,
        Front
    }

    public sealed class PlanetView3D : FrameworkElement, IAnimationsHost
    {
        #region Fields

        private static readonly GameLog _log;

        private static readonly Dictionary<PlanetType, List<CachedBitmap>> _atmospheres;
        private static readonly Dictionary<string, CachedBitmap> _customAtmospheres;
        private static readonly Dictionary<string, CachedBitmap> _customMaterials;
        private static readonly Dictionary<PlanetType, List<CachedBitmap>> _materials;
        private static readonly Dictionary<MoonShape, CachedBitmap> _moonImages;
        private static readonly string _planetSizeTypeFormat;

        private AnimationClock _animationClock;
        private readonly DoubleAnimation _axisAnimation;
        private readonly AxisAngleRotation3D _axisRotation;
        private readonly OrthographicCamera _camera;
        private readonly RotateTransform3D _cameraAngle;

        private readonly Grid _grid;
        private readonly TextBlock _label;
        private readonly ModelVisual3D _light;
        private readonly ModelVisual3D _model;
        private readonly StackPanel _moons;
        private readonly Image _overlay;
        private readonly RotateTransform3D _rotation;
        private readonly Sphere _sphere;
        private readonly ToolTip _toolTip;

        private readonly Transform3DGroup _transforms;
        private readonly Viewport3D _viewport;

        private IAppContext _appContext;
        private DiffuseMaterial _material;
        private string _toolTipText;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty PlanetMarginProperty = DependencyProperty.RegisterAttached(
            "PlanetMargin",
            typeof(Thickness),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                new Thickness(0),
                FrameworkPropertyMetadataOptions.Inherits));


        public static readonly DependencyProperty PlanetProperty = DependencyProperty.Register(
            "Planet",
            typeof(Planet),
            typeof(PlanetView3D),
            new PropertyMetadata(
                null,
                (o, args) => ((PlanetView3D)o).RebuildUI()));

        public static readonly DependencyProperty StarSystemProperty = DependencyProperty.RegisterAttached(
            "StarSystem",
            typeof(StarSystem),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits,
                (o, args) =>
                 {
                     if (o is PlanetView3D planetView)
                     {
                         planetView.RebuildUI();
                     }
                 }));

        public static readonly DependencyProperty BaseDimensionProperty = DependencyProperty.RegisterAttached(
            "BaseDimension",
            typeof(double),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                96d,
                FrameworkPropertyMetadataOptions.Inherits,
                OnImportantPropertyChanged));

        public static readonly DependencyProperty RotationDurationProperty = DependencyProperty.RegisterAttached(
            "RotationDuration",
            typeof(Duration),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                new Duration(TimeSpan.FromSeconds(120)),
                FrameworkPropertyMetadataOptions.Inherits,
                (o, args) =>
                {
                    if (!(o is PlanetView3D planetView))
                    {
                        return;
                    }

                    bool wasRunning = planetView._axisRotation.HasAnimatedProperties &&
                                     planetView._animationClock.Controller == null &&
                                     planetView._animationClock.CurrentState == ClockState.Active;

                    planetView.StopAnimations();

                    BindingExpression bindingExpression = BindingOperations.GetBindingExpression(planetView._axisAnimation, Timeline.DurationProperty);
                    if (bindingExpression != null)
                    {
                        bindingExpression.UpdateTarget();
                    }

                    planetView._animationClock = planetView._axisAnimation.CreateClock();

                    if (wasRunning)
                    {
                        planetView.ResumeAnimations();
                    }
                }));

        public static readonly DependencyProperty DimensionOverrideProperty = DependencyProperty.RegisterAttached(
            "DimensionOverride",
            typeof(double),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                double.NaN,
                FrameworkPropertyMetadataOptions.Inherits,
                OnImportantPropertyChanged));

        public static readonly DependencyProperty ShowMoonsProperty = DependencyProperty.RegisterAttached(
            "ShowMoons",
            typeof(bool),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.Inherits,
                OnImportantPropertyChanged));

        public static readonly DependencyProperty ShowPlanetTypeLabelsProperty = DependencyProperty.RegisterAttached(
            "ShowPlanetTypeLabels",
            typeof(bool),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.Inherits,
                OnImportantPropertyChanged));

        public static readonly DependencyProperty LightSourceDirectionProperty = DependencyProperty.RegisterAttached(
            "LightSourceDirection",
            typeof(LightSourceDirection),
            typeof(PlanetView3D),
            new FrameworkPropertyMetadata(
                LightSourceDirection.Right,
                FrameworkPropertyMetadataOptions.Inherits,
                OnImportantPropertyChanged));

        private static void OnImportantPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlanetView3D planetView)
            {
                planetView.RebuildUI();
            }
        }

        public static StarSystem GetStarSystem(DependencyObject target)
        {
            return target.GetValue(StarSystemProperty) as StarSystem;
        }

        public static void SetStarSystem(DependencyObject target, StarSystem value)
        {
            target.SetValue(StarSystemProperty, value);
        }

        public static Thickness GetPlanetMargin(DependencyObject target)
        {
            return (Thickness)target.GetValue(PlanetMarginProperty);
        }

        public static void SetPlanetMargin(DependencyObject target, Thickness value)
        {
            target.SetValue(PlanetMarginProperty, value);
        }

        public static void SetBaseDimension(DependencyObject target, double value)
        {
            target.SetValue(BaseDimensionProperty, value);
        }

        public static double GetBaseDimension(DependencyObject target)
        {
            return (double)target.GetValue(BaseDimensionProperty);
        }

        public static void SetRotationDuration(DependencyObject target, Duration value)
        {
            target.SetValue(RotationDurationProperty, value);
        }

        public static Duration GetRotationDuration(DependencyObject target)
        {
            return (Duration)target.GetValue(RotationDurationProperty);
        }

        public static void SetDimensionOverride(DependencyObject target, double value)
        {
            target.SetValue(DimensionOverrideProperty, value);
        }

        public static double GetDimensionOverride(DependencyObject target)
        {
            return (double)target.GetValue(DimensionOverrideProperty);
        }

        public static void SetShowMoons(DependencyObject target, bool value)
        {
            target.SetValue(ShowMoonsProperty, value);
        }

        public static bool GetShowMoons(DependencyObject target)
        {
            return (bool)target.GetValue(ShowMoonsProperty);
        }

        public static void SetShowPlanetTypeLabels(DependencyObject target, bool value)
        {
            target.SetValue(ShowPlanetTypeLabelsProperty, value);
        }

        public static bool GetShowPlanetTypeLabels(DependencyObject target)
        {
            return (bool)target.GetValue(ShowPlanetTypeLabelsProperty);
        }

        public static void SetLightSourceDirection(DependencyObject target, LightSourceDirection value)
        {
            target.SetValue(LightSourceDirectionProperty, value);
        }

        public static LightSourceDirection GetLightSourceDirection(DependencyObject target)
        {
            return (LightSourceDirection)target.GetValue(LightSourceDirectionProperty);
        }

        #endregion

        #region Constructors

        static PlanetView3D()
        {
            FocusVisualStyleProperty.OverrideMetadata(
                typeof(PlanetView3D),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            _log = GameLog.GetLog(typeof(PlanetView3D));

            _customMaterials = new Dictionary<string, CachedBitmap>();
            _customAtmospheres = new Dictionary<string, CachedBitmap>();
            _materials = new Dictionary<PlanetType, List<CachedBitmap>>();
            _atmospheres = new Dictionary<PlanetType, List<CachedBitmap>>();
            _moonImages = new Dictionary<MoonShape, CachedBitmap>();
            
            try { _planetSizeTypeFormat = ResourceManager.GetString("PLANET_SIZE_TYPE_FORMAT"); }
            catch { _planetSizeTypeFormat = "{0} {1}"; }

            try
            {
                XDocument customPlanetsXml = XDocument.Load(ResourceManager.GetResourcePath(@"Resources\Data\CustomPlanets.xml"));

                var customPlanets = from customPlanet in customPlanetsXml.Elements("CustomPlanets").Elements("CustomPlanet")
                                    let name = (string)customPlanet.Attribute("Name")
                                    let imageSource = new CachedBitmap(
                                        new BitmapImage(
                                            ResourceManager.GetResourceUri(
                                                "Resources/Images/UI/Planets/" + (string)customPlanet.Attribute("Texture"))),
                                        BitmapCreateOptions.None,
                                        BitmapCacheOption.OnLoad)
                                    let atmosphereSource = new CachedBitmap(
                                        new BitmapImage(
                                            ResourceManager.GetResourceUri(
                                                "Resources/Images/UI/Atmospheres/" + (string)customPlanet.Attribute("Atmosphere"))),
                                        BitmapCreateOptions.None,
                                        BitmapCacheOption.OnLoad)
                                    select new { Name = name, ImageSource = imageSource, AtmosphereSource = atmosphereSource };

                foreach (var customPlanet in customPlanets)
                {
                    if (customPlanet.ImageSource != null && customPlanet.ImageSource.CanFreeze)
                    {
                        customPlanet.ImageSource.Freeze();
                    }

                    if (customPlanet.AtmosphereSource != null && customPlanet.AtmosphereSource.CanFreeze)
                    {
                        customPlanet.AtmosphereSource.Freeze();
                    }

                    _customMaterials.Add(customPlanet.Name, customPlanet.ImageSource);
                    _customAtmospheres.Add(customPlanet.Name, customPlanet.AtmosphereSource);
                }
            }
            catch (Exception e)
            {
                _log.GameData.Error(
                   "Error processing CustomPlanets.xml",
                   e);
            }

            foreach (PlanetType type in EnumHelper.GetValues<PlanetType>())
            {
                if (type == PlanetType.Asteroids)
                {
                    continue;
                }

                _materials[type] = new List<CachedBitmap>();
                _atmospheres[type] = new List<CachedBitmap>();

                for (int i = 0; i < 3; i++)
                {
                    CachedBitmap cachedBitmap = new CachedBitmap(
                        new BitmapImage(
                            ResourceManager.GetResourceUri(
                                string.Format(
                                    "Resources/Images/UI/Planets/{0}{1:00}.png",
                                    type,
                                    i + 1))),
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);

                    cachedBitmap.Freeze();

                    _materials[type].Insert(i, cachedBitmap);

                    cachedBitmap = new CachedBitmap(
                        new BitmapImage(
                            ResourceManager.GetResourceUri(
                                string.Format(
                                    "Resources/Images/UI/Atmospheres/{0}{1:00}.png",
                                    type,
                                    i + 1))),
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);

                    cachedBitmap.Freeze();

                    _atmospheres[type].Insert(i, cachedBitmap);
                }
            }

            foreach (MoonShape shape in EnumHelper.GetValues<MoonShape>())
            {
                _moonImages[shape] = new CachedBitmap(
                    new BitmapImage(
                        ResourceManager.GetResourceUri(
                            string.Format("Resources/Images/UI/Planets/Moons/{0}.png", shape))),
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);

                _moonImages[shape].Freeze();
            }
        }

        public PlanetView3D()
        {
            _sphere = new Sphere();
            _model = new ModelVisual3D();
            _grid = new Grid();
            _viewport = new Viewport3D();
            _overlay = new Image();
            _label = new TextBlock();
            _toolTip = new ToolTip();
            _transforms = new Transform3DGroup();
            _axisRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            _rotation = new RotateTransform3D(_axisRotation, new Point3D(0, 0, 0));
            _transforms.Children.Add(_rotation);

            _light = new ModelVisual3D
                     {
                         Content = new DirectionalLight(
                             Colors.White,
                             new Vector3D(0.85, -0.15, 1))
                     };

            _viewport.ClipToBounds = false;
            _viewport.IsHitTestVisible = false;
            _viewport.Children.Add(_model);
            _viewport.Children.Add(_light);

            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1d, GridUnitType.Auto) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1d, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1d, GridUnitType.Auto) });
            _grid.VerticalAlignment = VerticalAlignment.Center;

            _label.TextAlignment = TextAlignment.Center;
            _label.Margin = new Thickness(2);
            _label.FontSize = 10.0;
            _label.FontFamily = new FontFamily("Resources/Fonts/#Calibri");

            _moons = new StackPanel
            {
                Height = 16,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _moons.SetValue(Grid.RowProperty, 0);

            _viewport.SetValue(Grid.RowProperty, 1);
            _overlay.SetValue(Grid.RowProperty, 1);
            _label.SetValue(Grid.RowProperty, 2);

            _cameraAngle = new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), 15),
                new Point3D(0, 0, 0));

            _camera = new OrthographicCamera(
                new Point3D(0, 0, -1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 1, 0),
                (double)OrthographicCamera.WidthProperty.DefaultMetadata.DefaultValue)
                      {
                          NearPlaneDistance = (-1),
                          FarPlaneDistance = 1,
                          Transform = _cameraAngle
                      };

            _camera.Freeze();

            _viewport.Camera = _camera;

            _axisAnimation = new DoubleAnimation
                             {
                                 From = 0,
                                 To = 360,
                                 RepeatBehavior = RepeatBehavior.Forever
                             };

            _ = BindingOperations.SetBinding(
                _axisAnimation,
                Timeline.DurationProperty,
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(RotationDurationProperty),
                    Mode = BindingMode.OneWay
                });

            _animationClock = _axisAnimation.CreateClock();

            AddVisualChild(_grid);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimations();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ClientSettings.Current.EnableAnimation)  // only animate then
            {
                StartAnimations();
            }
        }

        #region Properties

        public Thickness PlanetMargin
        {
            get => (Thickness)GetValue(PlanetMarginProperty);
            set => SetValue(PlanetMarginProperty, value);
        }

        public Duration RotationDuration
        {
            get => (Duration)GetValue(RotationDurationProperty);
            set => SetValue(RotationDurationProperty, value);
        }

        public double BaseDimension
        {
            get => (double)GetValue(BaseDimensionProperty);
            set => SetValue(BaseDimensionProperty, value);
        }

        public double DimensionOverride
        {
            get => (double)GetValue(DimensionOverrideProperty);
            set => SetValue(DimensionOverrideProperty, value);
        }

        public bool ShowMoons
        {
            get => (bool)GetValue(ShowMoonsProperty);
            set => SetValue(ShowMoonsProperty, value);
        }
        public bool ShowPlanetTypeLabels
        {
            get => (bool)GetValue(ShowPlanetTypeLabelsProperty);
            set => SetValue(ShowPlanetTypeLabelsProperty, value);
        }

        public Planet Planet
        {
            get => GetValue(PlanetProperty) as Planet;
            set => SetValue(PlanetProperty, value);
        }

        public StarSystem StarSystem
        {
            get => GetValue(StarSystemProperty) as StarSystem;
            set => SetValue(StarSystemProperty, value);
        }

        public LightSourceDirection LightSourceDirection
        {
            get => GetLightSourceDirection(this);
            set => SetLightSourceDirection(this, value);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private IAppContext AppContext
        {
            get
            {
                IAppContext appContext = _appContext;
                if (appContext == null)
                {
                    appContext = Designer.IsInDesignMode
                        ? (_appContext = DesignTimeAppContext.Instance)
                        : (_appContext = ServiceLocator.Current.GetInstance<IAppContext>());
                }
                return appContext;
            }
        }
        // ReSharper restore MemberCanBeMadeStatic.Local

        protected override int VisualChildrenCount => 1;

        #endregion

        #region IAnimationsHost Members

        public void PauseAnimations()
        {
            if (!_animationClock.IsPaused && _animationClock.Controller != null)
            {
                _animationClock.Controller.Pause();
            }
        }

        public void ResumeAnimations()
        {
            if (_animationClock.Controller == null)
            {
                return;
            }

            if (_animationClock.IsPaused)
            {
                _animationClock.Controller.Resume();
            }
            else
                if (ClientSettings.Current.EnableAnimation)
            {
                StartAnimations();
            }
        }

        public void StopAnimations()
        {
            if (_animationClock.Controller != null)
            {
                _animationClock.Controller.Stop();
                _animationClock.Controller.Remove();
            }

            if (_axisAnimation != null)
            {
                _axisRotation.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, null);
            }
        }

        public void StartAnimations()
        {
            if (_axisAnimation != null &&
                !_axisRotation.HasAnimatedProperties)
            {
                _axisRotation.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, _animationClock);
            }

            if (_animationClock.Controller != null &&
                _animationClock.CurrentState != ClockState.Active)
            {
                _animationClock.Controller.Begin();
            }
        }

        #endregion

        #region Methods

        private void RebuildUI()
        {
            _grid.Children.Clear();
            _moons.Children.Clear();
            _overlay.ClearValue(Image.SourceProperty);
            _toolTip.ClearValue(ContentControl.ContentProperty);
            _label.ClearValue(ContentControl.ContentProperty);

            Planet planet = Planet;
            StarSystem system = StarSystem;

            if (planet == null || system == null)
            {
                return;
            }

            bool overrideDimension = !DoubleUtil.IsNaN(DimensionOverride);
            double dimension = overrideDimension ? DimensionOverride : BaseDimension;
            bool customTexture = false;

            Entities.Race targetRace = system.HasColony
                                 ? system.Colony.Inhabitants
                                 : AppContext.LocalPlayerEmpire.Civilization.Race;

            PlanetEnvironment environment = planet.GetEnvironment(targetRace);
            int maxPopulation = planet.GetMaxPopulation(targetRace);
            Types.Percentage growthRate = planet.GetGrowthRate(targetRace);

            if (!overrideDimension)
            {
                switch (planet.PlanetSize)
                {
                    case PlanetSize.Large:
                        dimension = (2d / 3d) * BaseDimension;
                        break;
                    case PlanetSize.Medium:
                        dimension = .5d * BaseDimension;
                        break;
                    case PlanetSize.Small:
                        dimension = .375d * BaseDimension;
                        break;
                    case PlanetSize.Tiny:
                        dimension = .25d * BaseDimension;
                        break;
                    case PlanetSize.Giant:
                        dimension = (5d / 6d) * BaseDimension;
                        break;
                }
            }

            _viewport.Width = dimension;
            _viewport.Height = dimension;
            _overlay.Width = _viewport.Width + 2;
            _overlay.Height = _viewport.Height + 2;

            _toolTipText = planet.Name;

            if (planet.PlanetType == PlanetType.GasGiant)
            {
                _toolTipText += "\n";
                _toolTipText += ResourceManager.GetString("PLANET_TYPE_GASGIANT");
            }
            else if (planet.PlanetType == PlanetType.Crystalline)
            {
                _toolTipText += "\n";
                _toolTipText += ResourceManager.GetString("PLANET_TYPE_CRYSTALLINE");
            }
            else
            {
                _toolTipText += "\n";
                _toolTipText += string.Format(
                    _planetSizeTypeFormat,
                    ResourceManager.GetString("PLANET_SIZE_" + planet.PlanetSize.ToString().ToUpper()),
                    ResourceManager.GetString("PLANET_TYPE_" + planet.PlanetType.ToString().ToUpper()));
            }

            _toolTipText += "\n";

            if (environment == PlanetEnvironment.Uninhabitable)
            {
                _toolTipText += ResourceManager.GetString(
                    "PLANET_ENVIRONMENT_" + environment.ToString().ToUpper());
            }
            else
            {
                _toolTipText += ResourceManager.GetString("PLANET_ENVIRONMENT") + ": ";
                _toolTipText += ResourceManager.GetString(
                    "PLANET_ENVIRONMENT_" + environment.ToString().ToUpper());
                _toolTipText += "\n" + ResourceManager.GetString("PLANET_MAX_POPULATION")
                    + ": " + maxPopulation;
                _toolTipText += "\n" + ResourceManager.GetString("PLANET_GROWTH_RATE")
                    + ": " + growthRate;
            }

            _toolTip.Content = _toolTipText;
            _overlay.ToolTip = _toolTip;

            _label.Foreground =
                ((planet.PlanetType == PlanetType.Crystalline)
                    || (planet.PlanetType == PlanetType.GasGiant)
                        || (planet.PlanetType == PlanetType.Demon))
                    ? Brushes.Blue : Brushes.Green;

            switch (planet.PlanetType)
            {
                case PlanetType.Barren:
                    _label.Text = "J";
                    break;
                case PlanetType.Desert:
                    _label.Text = "G";
                    break;
                case PlanetType.Crystalline:
                    _label.Text = "D";
                    break;
                case PlanetType.GasGiant:
                    _label.Text = "B";
                    break;
                case PlanetType.Volcanic:
                    _label.Text = "K";
                    break;
                case PlanetType.Oceanic:
                    _label.Text = "O";
                    break;
                case PlanetType.Rogue:
                    _label.Text = "R";
                    break;
                case PlanetType.Demon:
                    _label.Text = "Y";
                    break;
                case PlanetType.Jungle:
                    _label.Text = "L";
                    break;
                case PlanetType.Terran:
                    _label.Text = "M";
                    break;
                case PlanetType.Arctic:
                    _label.Text = "P";
                    break;
                default:
                    _label.Text = "";
                    _label.Visibility = Visibility.Collapsed;
                    break;
            }

            if (_customMaterials.ContainsKey(planet.Name))
            {
                customTexture = true;
                _material = new DiffuseMaterial(
                    new ImageBrush(_customMaterials[planet.Name]));
            }
            else if (_materials.ContainsKey(planet.PlanetType))
            {
                if (_materials[planet.PlanetType].Count > planet.Variation)
                {
                    _material = new DiffuseMaterial(new ImageBrush(
                                                        _materials[planet.PlanetType][planet.Variation]));
                }
                else
                {
                    if (_materials[planet.PlanetType].Count > 0)
                    {
                        _material = new DiffuseMaterial(
                            new ImageBrush(_materials[planet.PlanetType][0]));
                    }
                }
            }

            CachedBitmap atmosphereImage = null;

            if (!customTexture ||
                !_customAtmospheres.TryGetValue(planet.Name, out atmosphereImage))
            {

                if (_atmospheres.TryGetValue(planet.PlanetType, out List<CachedBitmap> atmospheres) &&
                    atmospheres.Count > planet.Variation)
                {
                    atmosphereImage = atmospheres[planet.Variation];
                }
                else if (_atmospheres.TryGetValue(PlanetType.Barren, out atmospheres) &&
                        atmospheres.Count > 0)
                {
                    atmosphereImage = _atmospheres[PlanetType.Barren][0];
                }
            }

            _overlay.Source = atmosphereImage;
            _overlay.RenderTransformOrigin = new Point(0.5, 0.5);

            if (LightSourceDirection == LightSourceDirection.Left)
            {
                _overlay.ClearValue(VisibilityProperty);
                _overlay.RenderTransform = new ScaleTransform(-1d, 1d);
                
                _light.Content = new DirectionalLight(
                    Colors.White,
                    new Vector3D(-0.85, -0.15, 1));
            }
            else if (LightSourceDirection == LightSourceDirection.Right)
            {
                _overlay.ClearValue(VisibilityProperty);
                _overlay.ClearValue(RenderTransformProperty);

                _light.Content = new DirectionalLight(
                    Colors.White,
                    new Vector3D(0.85, -0.15, 1));
            }
            else
            {
                _overlay.Visibility = Visibility.Hidden;
                _overlay.ClearValue(RenderTransformProperty);

                _light.Content = new DirectionalLight(
                    Colors.White,
                    new Vector3D(0, -0.5, 1));
            }

            if (_material == null)
            {
                _material = new DiffuseMaterial(Brushes.White);
            }

            _material.AmbientColor = Color.FromScRgb(1.0f, 0.3f, 0.3f, 0.3f);
            _material.Color = Color.FromScRgb(1.0f, 0.7f, 0.7f, 0.7f);

            _model.Content = new GeometryModel3D(_sphere.Mesh, _material);
            _model.Transform = _transforms;

            if (ShowMoons)
            {
                foreach (MoonType moon in planet.Moons)
                {
                    int size;
                    ToolTip moonToolTip = new ToolTip();
                    Image moonImage = new Image { Source = _moonImages[moon.GetShape()] };

                    switch (moon.GetSize())
                    {
                        case MoonSize.Small:
                            size = 8;
                            moonToolTip.Content = ResourceManager.GetString("SMALL_MOON_STRING");
                            break;
                        case MoonSize.Medium:
                            size = 12;
                            moonToolTip.Content = ResourceManager.GetString("MEDIUM_MOON_STRING");
                            break;
                        default:
                            size = 16;
                            moonToolTip.Content = ResourceManager.GetString("LARGE_MOON_STRING");
                            break;
                    }

                    moonImage.Width = moonImage.Height = size;
                    moonImage.VerticalAlignment = VerticalAlignment.Center;
                    moonImage.Margin = new Thickness(1, 0, 1, 0);
                    moonImage.ToolTip = moonToolTip;

                    _ = _moons.Children.Add(moonImage);
                }
            }

            _ = _grid.Children.Add(_viewport);
            _ = _grid.Children.Add(_overlay);

            if (ShowPlanetTypeLabels)
            {
                _ = _grid.Children.Add(_label);
            }

            if (ShowMoons)
            {
                _ = _grid.Children.Add(_moons);
            }
        }

        public static void PreloadImages() { }

        public void Rotate()
        {
            _axisRotation.Angle--;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _grid.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _grid;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _grid.Measure(availableSize);
            return _grid.DesiredSize;
        }

        #endregion
    }

    internal class Sphere
    {
        #region Fields
        private bool _changed;
        private int _latitude;
        private int _longitude;
        private MeshGeometry3D _mesh;
        private double _radius;
        #endregion

        #region Constructors
        public Sphere()
        {
            _latitude = 24;
            _longitude = 48;
            _radius = 1.0;
            _changed = true;
        }
        #endregion

        #region Properties
        public int Latitude
        {
            set
            {
                if (value != _latitude)
                {
                    _latitude = value;
                    _changed = true;
                }
            }
        }

        public int Longitude
        {
            set
            {
                if (value != _longitude)
                {
                    _longitude = value;
                    _changed = true;
                }
            }
        }

        public double Radius
        {
            set
            {
                if (value != _radius)
                {
                    _radius = value;
                    _changed = true;
                }
            }
        }

        public MeshGeometry3D Mesh
        {
            get
            {
                if (_changed)
                {
                    CreateMesh();
                    _changed = false;
                }

                return _mesh;
            }
        }
        #endregion

        #region Methods
        private void CreateMesh()
        {
            _mesh = new MeshGeometry3D();

            double latTheta = 0.0;
            double latDeltaTheta = Math.PI / _latitude;
            double lonDeltaTheta = 2.0 * Math.PI / _longitude;

            Point3D origin = new Point3D(0, 0, 0);

            // Order of vertex creation:
            //  - For each latitude strip (y := [+radius,-radius] by -increment)
            //      - start at (-x,y,0)
            //      - For each longitude line (CCW about +y ... meaning +y points out of the paper)
            //          - generate vertex for latitude-longitude intersection

            // So if you have a 2x1 texture applied to this sphere:
            //      +---+---+
            //      | A | B |
            //      +---+---+
            // A camera pointing down -z with up = +y will see the "A" half of the texture.
            // "A" is considered to be the front of the sphere.

            for (int lat = 0; lat <= _latitude; lat++)
            {
                double v = (double)lat / _latitude;
                double y = _radius * Math.Cos(latTheta);
                double r = _radius * Math.Sin(latTheta);

                if (lat == _latitude - 1)
                {
                    latTheta = Math.PI; // Close the gap in case of precision error
                }
                else
                {
                    latTheta += latDeltaTheta;
                }

                double lonTheta = Math.PI;

                for (int lon = 0; lon <= _longitude + 1; lon++)
                {
                    double u = (double)lon / _longitude;
                    double x = r * Math.Cos(lonTheta);
                    double z = r * Math.Sin(lonTheta);
                    if (lon == _longitude - 1)
                    {
                        lonTheta = Math.PI; // Close the gap in case of precision error
                    }
                    else
                    {
                        lonTheta -= lonDeltaTheta;
                    }

                    Point3D p = new Point3D(x, y, z);
                    Vector3D norm = p - origin;

                    _mesh.Positions.Add(p);
                    _mesh.Normals.Add(norm);
                    _mesh.TextureCoordinates.Add(new Point(u, v));

                    if (lat != 0 && lon != 0)
                    {
                        // The loop just created the bottom right vertex (lat * (longitude + 1) + lon)
                        //  (the +1 comes because of the extra vertex on the seam)
                        // We only create panels when we're at the bottom-right vertex
                        //  (bottom-left, top-right, top-left have all been created by now)
                        //
                        //          +-----------+ x - (longitude + 1)
                        //          |           |
                        //          |           |
                        //      x-1 +-----------+ x

                        int bottomRight = (lat * (_longitude + 1)) + lon;
                        int bottomLeft = bottomRight - 1;
                        int topRight = bottomRight - (_longitude + 1);
                        int topLeft = topRight - 1;

                        // Wind counter-clockwise
                        _mesh.TriangleIndices.Add(bottomLeft);
                        _mesh.TriangleIndices.Add(topRight);
                        _mesh.TriangleIndices.Add(topLeft);

                        _mesh.TriangleIndices.Add(bottomRight);
                        _mesh.TriangleIndices.Add(topRight);
                        _mesh.TriangleIndices.Add(bottomLeft);
                    }
                }
            }
        }
        #endregion
    }

    public class PlanetItemTemplateSelector : DataTemplateSelector
    {
        public static readonly PlanetItemTemplateSelector Instance = new PlanetItemTemplateSelector();

        private static readonly DataTemplate AsteroidsTemplate;
        private static readonly DataTemplate PlanetTemplate;

        public double BaseDimension { get; set; } = 96d;

        static PlanetItemTemplateSelector()
        {
            FrameworkElementFactory asteroids = new FrameworkElementFactory(typeof(AsteroidsView));
            FrameworkElementFactory planet = new FrameworkElementFactory(typeof(PlanetView3D));

            planet.SetBinding(
                PlanetView3D.PlanetProperty,
                new Binding
                {
                    Mode = BindingMode.OneWay
                });

            planet.SetBinding(
                FrameworkElement.MarginProperty,
                new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath(PlanetView3D.PlanetMarginProperty),
                    Mode = BindingMode.OneWay
                });


            asteroids.SetBinding(
                FrameworkElement.WidthProperty,
                new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath(PlanetView3D.BaseDimensionProperty),
                    Mode = BindingMode.OneWay
                });

            asteroids.SetBinding(
                FrameworkElement.HeightProperty,
                new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath(PlanetView3D.BaseDimensionProperty),
                    Mode = BindingMode.OneWay
                });

            asteroids.SetBinding(
                FrameworkElement.MarginProperty,
                new Binding
                {
                    RelativeSource = RelativeSource.Self,
                    Path = new PropertyPath(PlanetView3D.PlanetMarginProperty),
                    Mode = BindingMode.OneWay
                });

            AsteroidsTemplate = new DataTemplate(typeof(Planet))
                                {
                                    VisualTree = asteroids
                                };

            PlanetTemplate = new DataTemplate(typeof(Planet))
                             {
                                 VisualTree = planet
                             };
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return !(item is Planet planet)
                ? base.SelectTemplate(item, container)
                : planet.PlanetType == PlanetType.Asteroids ? AsteroidsTemplate : PlanetTemplate;
        }
    }
}