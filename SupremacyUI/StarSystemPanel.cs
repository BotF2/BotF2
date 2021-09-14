// StarSystemPanel.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Client;
using Supremacy.Client.Events;
using Supremacy.Game;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Xna;

using System.Linq;
using Supremacy.Client.Context;

namespace Supremacy.UI
{
    public sealed class StarSystemPanel : Control
    {
        #region Constants
        private const double PlanetBonusIconSize = 20;
        private const double SystemBonusIconSize = 24;
        #endregion

        #region Static Members

        private static IAppContext s_appContext;
        private static IResourceManager s_resourceManager;

        #endregion

        #region Constants
        private static readonly CachedBitmap DilithiumBonusImage;
        private static readonly CachedBitmap EnergyBonusImage;
        private static readonly CachedBitmap FoodBonusImage;
        private static readonly CachedBitmap NebulaImage;
        private static readonly CachedBitmap DuraniumBonusImage;
        public static readonly DependencyProperty ShowStatsProperty;
        #endregion

        #region Constructors and Finalizers
        static StarSystemPanel()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(SystemPanel), new FrameworkPropertyMetadata(typeof(SystemPanel)));

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(StarSystemPanel),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            ShowStatsProperty = DependencyProperty.Register(
                "ShowStats",
                typeof(bool),
                typeof(StarSystemPanel),
                new PropertyMetadata(
                    true,
                    ShowStatsChangedCallback));

            NebulaImage = new CachedBitmap(
                new BitmapImage(
                    new Uri(
                        "Resources/Images/UI/Stars/Nebula.png",
                        UriKind.Relative)),
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            NebulaImage.Freeze();

            FoodBonusImage = new CachedBitmap(
                new BitmapImage(
                    new Uri(
                        "Resources/Images/UI/ScreenIcons/food.png",
                        UriKind.Relative)),
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            FoodBonusImage.Freeze();

            EnergyBonusImage = new CachedBitmap(
                new BitmapImage(
                    new Uri(
                        "Resources/Images/UI/ScreenIcons/energy_bonus.png",
                        UriKind.Relative)),
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            EnergyBonusImage.Freeze();

            DilithiumBonusImage = new CachedBitmap(
                new BitmapImage(
                    new Uri(
                        "Resources/Images/UI/ScreenIcons/dilithium.png",
                        UriKind.Relative)),
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            DilithiumBonusImage.Freeze();

            DuraniumBonusImage = new CachedBitmap(
                new BitmapImage(
                    new Uri(
                        "Resources/Images/UI/ScreenIcons/rawmaterials.png",
                        UriKind.Relative)),
                BitmapCreateOptions.None,
                BitmapCacheOption.OnLoad);
            DuraniumBonusImage.Freeze();
        }
        #endregion

        #region Public and Protected Methods
        public static void PreloadImages()
        {
            PlanetView3D.PreloadImages();
            AsteroidsView.PreloadImages();
        }
        #endregion

        #region Private Methods
        private static void ShowStatsChangedCallback(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            if (source is StarSystemPanel panel)
            {
                panel.Refresh();
            }
        }
        #endregion

        #region Fields
        private readonly Grid _grid;
        private readonly ObservableObject<object> _regionContext;
        private readonly DelegatingWeakPropertyChangedListener _regionContextChangeHandler;

        #endregion

        #region Constructors and Finalizers
        public StarSystemPanel()
        {
            _grid = new Grid();
            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
            _grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            SetResourceReference(
                FontFamilyProperty,
                ClientResources.DefaultFontFamilyKey);

            AddLogicalChild(_grid);
            AddVisualChild(_grid);

            _ = ClientEvents.GameEnding.Subscribe(OnGameEnding, ThreadOption.UIThread);

            _regionContextChangeHandler = new DelegatingWeakPropertyChangedListener(OnRegionContextChanged);

            _regionContext = RegionContext.GetObservableContext(this);

            DataContext = _regionContext;

            PropertyChangedEventManager.AddListener(
                _regionContext,
                _regionContextChangeHandler,
                "Value");

            _ = Channel<TurnStartedMessage>.Public.ObserveOnDispatcher().Subscribe(_ => Refresh());
        }

        private void OnGameEnding(ClientEventArgs e)
        {
            _regionContext.ClearValue(ObservableObject<object>.ValueProperty);
        }

        private void OnRegionContextChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }
        #endregion

        #region Properties and Indexers

        private IAppContext AppContext
        {
            get
            {
                if (s_appContext == null)
                {
                    s_appContext = ServiceLocator.Current.GetInstance<IAppContext>();
                }

                return s_appContext;
            }
        }

        private IResourceManager ResourceManager
        {
            get
            {
                if (s_resourceManager == null)
                {
                    s_resourceManager = ServiceLocator.Current.GetInstance<IResourceManager>();
                }

                return s_resourceManager;
            }
        }
        public Sector Sector => _regionContext.Value as Sector;

        public bool ShowStats
        {
            get => (bool)GetValue(ShowStatsProperty);
            set => SetValue(ShowStatsProperty, value);
        }

        public StarSystem System
        {
            get
            {
                Sector sector = Sector;
                return sector?.System;
            }
        }

        protected override int VisualChildrenCount => 1;
        #endregion

        #region Public and Protected Methods
        public void Refresh()
        {
            StarSystem system = System;
            //GameLog.Client.UI.DebugFormat("trying to refreshing PlanetView for {0}", system.Name);
            _grid.Children.Clear();

            if (ShowStats)
            {
                DisplayStats(system);
            }
            //GameLog.Client.UI.DebugFormat("refreshing StarSystemPanel");
            if ((system != null) && IsScanned(system.Sector) && StarHelper.SupportsPlanets(system)) // && IsScanned(system.Sector) && StarHelper.SupportsPlanets(system))
            {
                //GameLog.Client.UI.DebugFormat("refreshing PlanetView");
                DisplayVisuals(system);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _grid;
        }
        #endregion

        #region Private Methods
        private void DisplayStats(StarSystem system)
        {
            StackPanel statsPanel = new StackPanel();
            FontSizeConverter fontSize = new FontSizeConverter();
            TextBlock name = new TextBlock();
            TextBlock details = new TextBlock();

            name.FontFamily = FontFamily;
            name.FontSize = (double)fontSize.ConvertFrom("14pt");
            name.Foreground = Brushes.LightBlue;
            name.Margin = new Thickness(0, 0, 0, 2);

            details.FontFamily = FontFamily;
            details.FontSize = (double)fontSize.ConvertFrom("12pt");
            details.Foreground = Brushes.White;
            details.TextWrapping = TextWrapping.Wrap;
            details.TextTrimming = TextTrimming.WordEllipsis;

            statsPanel.SetValue(Grid.ColumnProperty, 0);
            statsPanel.Orientation = Orientation.Vertical;
            statsPanel.Margin = new Thickness(0, 0, 14, 0);
            statsPanel.CanHorizontallyScroll = false;
            if (!double.IsNaN(ActualWidth))
            {
                statsPanel.MaxWidth = ActualWidth;
            }

            _ = statsPanel.Children.Add(name);

            if (!IsScanned(Sector))
            {
                name.Text = ResourceManager.GetString("UNKNOWN_SECTOR");
            }
            else if ((system != null) && (system.StarType == StarType.Nebula) && (!IsExplored(Sector) || system.Planets.Count == 0))
            {
                name.Text = ResourceManager.GetString("STAR_TYPE_NEBULA");
                details.Text = ResourceManager.GetString("STAR_TYPE_NEBULA_DESCRIPTION");
                _ = statsPanel.Children.Add(details);
            }
            else if ((system != null) && !IsExplored(Sector) && StarHelper.SupportsPlanets(system))
            {
                name.Text = ResourceManager.GetString("UNEXPLORED_SYSTEM");
            }
            else if (system != null)
            {
                switch (system.StarType)
                {
                    case StarType.BlackHole:
                        name.Text = ResourceManager.GetString("STAR_TYPE_BLACKHOLE");
                        details.Text = ResourceManager.GetString("STAR_TYPE_BLACKHOLE_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;
                    case StarType.Wormhole:
                        name.Text = string.Format(ResourceManager.GetString("WORMHOLE_NAME_FORMAT"),
                            system.Name);
                        details.Text = ResourceManager.GetString("STAR_TYPE_WORMHOLE_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;
                    case StarType.Quasar:
                        name.Text = ResourceManager.GetString("STAR_TYPE_QUASAR");
                        details.Text = ResourceManager.GetString("STAR_TYPE_QUASAR_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;

                    case StarType.NeutronStar:
                        name.Text = ResourceManager.GetString("STAR_TYPE_NEUTRONSTAR");
                        details.Text = ResourceManager.GetString("STAR_TYPE_NEUTRONSTAR_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;

                    case StarType.RadioPulsar:
                        name.Text = ResourceManager.GetString("STAR_TYPE_RADIOPULSAR");
                        details.Text = ResourceManager.GetString("STAR_TYPE_RADIOPULSAR_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;

                    case StarType.XRayPulsar:
                        name.Text = ResourceManager.GetString("STAR_TYPE_XRAYPULSAR");
                        details.Text = ResourceManager.GetString("STAR_TYPE_XRAYPULSAR_DESCRIPTION");
                        _ = statsPanel.Children.Add(details);
                        break;

                    default:
                        TextBlock morale = new TextBlock();
                        TextBlock population = new TextBlock();
                        TextBlock growth = new TextBlock();
                        TextBlock health = new TextBlock();
                        TextBlock race = new TextBlock();
                        TextBlock orbitals = new TextBlock();

                        morale.FontFamily = FontFamily;
                        population.FontFamily = FontFamily;
                        growth.FontFamily = FontFamily;
                        health.FontFamily = FontFamily;
                        race.FontFamily = FontFamily;
                        orbitals.FontFamily = FontFamily;

                        morale.FontSize = population.FontSize
                                        = growth.FontSize
                                        = health.FontSize
                                        = race.FontSize
                                        = orbitals.FontSize
                                        = (double)fontSize.ConvertFrom("11pt");

                        morale.Foreground = population.Foreground
                                            = growth.Foreground
                                            = health.Foreground
                                            = race.Foreground
                                            = orbitals.Foreground
                                            = Brushes.Beige;

                        race.Foreground = Brushes.Aquamarine;
                        population.Foreground = Brushes.Goldenrod;
                        growth.Foreground = Brushes.LightGreen;
                        health.Foreground = Brushes.LightBlue;
                        orbitals.Foreground = Brushes.LightGray;
                        morale.Foreground = Brushes.YellowGreen;

                        name.Text = system.StarType == StarType.Nebula
                            ? string.Format(
                                ResourceManager.GetString("NEBULA_NAME_FORMAT"),
                                system.Name)
                            : system.Name;

                        if (system.HasColony)
                        {
                            morale.Text = string.Format("{0}: {1}",
                                ResourceManager.GetString("MORALE"), system.Colony.Morale.CurrentValue);
                            population.Text = string.Format("{0}: {1:#,##0} of {2:#,##0}",
                                ResourceManager.GetString("SYSTEM_POPULATION"),
                                system.Colony.Population.CurrentValue, system.Colony.MaxPopulation);
                            growth.Text = string.Format("{0}: {1:0.#}%",
                                ResourceManager.GetString("SYSTEM_GROWTH_RATE"), system.Colony.GrowthRate * 100);
                            race.Text = string.Format("{0}: {1}",
                                ResourceManager.GetString("SYSTEM_INHABITANTS"), system.Colony.Inhabitants.PluralName);
                            Percentage populationHealth = system.Colony.Health.PercentFilled;
                            health.Text = string.Format("{0}: {1:0.#}%",
                                ResourceManager.GetString("SYSTEM_HEALTH"), populationHealth * 100);

                            morale.ToolTip = ResourceManager.GetString("MORALE_TOOLTIP");
                            growth.ToolTip = ResourceManager.GetString("SYSTEM_GROWTH_RATE_TOOLTIP");
                            health.ToolTip = ResourceManager.GetString("SYSTEM_HEALTH_TOOLTIP");
                            orbitals.ToolTip = ResourceManager.GetString("SYSTEM_SHIELDS_TOOLTIP");

                            _ = orbitals.SetBinding(
                                TextBlock.TextProperty,
                                new MultiBinding
                                {
                                    StringFormat = string.Format("{0}: {{0}} / {{1}}", ResourceManager.GetString("SYSTEM_SHIELDS")),
                                    Bindings =
                                        {
                                            new Binding
                                            {
                                                Source = system.Colony,
                                                Path = new PropertyPath("ShieldStrength.CurrentValue")
                                            },
                                            new Binding
                                            {
                                                Source = system.Colony,
                                                Path = new PropertyPath("ShieldStrength.Maximum")
                                            }
                                        }

                                });
                        }
                        else
                        {
                            race.Text = ResourceManager.GetString("SYSTEM_UNINHABITED");
                            population.Text = string.Format("{0}: {1:#,##0}",
                                ResourceManager.GetString("SYSTEM_MAX_POPULATION"), system.GetMaxPopulation(AppContext.LocalPlayerEmpire.Civilization.Race));
                            growth.Text = string.Format("{0}: {1:0.#}%",
                                ResourceManager.GetString("SYSTEM_GROWTH_RATE"), system.GetGrowthRate(AppContext.LocalPlayerEmpire.Civilization.Race) * 100);
                            BindingOperations.ClearBinding(orbitals, TextBlock.TextProperty);
                        }

                        _ = statsPanel.Children.Add(race);
                        _ = statsPanel.Children.Add(population);
                        _ = statsPanel.Children.Add(growth);
                        if (system.HasColony)
                        {
                            _ = statsPanel.Children.Add(health);
                            _ = statsPanel.Children.Add(morale);
                            _ = statsPanel.Children.Add(orbitals);
                        }

                        break;
                }
            }
            else
            {
                name.Text = ResourceManager.GetString("EMPTY_SPACE");
            }

            _ = _grid.Children.Add(statsPanel);
        }

        private void DisplayVisuals(StarSystem system)
        {
            //GameLog.Client.UI.DebugFormat("DisplayVisuals for {0} {1}", system.Location, system.Name);
            Grid starContainer = new Grid();
            Viewbox view = new Viewbox();
            StackPanel visuals = new StackPanel();

            starContainer.Margin = new Thickness(14, 0, 0, 0);
            starContainer.MaxHeight = 128;
            starContainer.MaxWidth = 128;
            starContainer.VerticalAlignment = VerticalAlignment.Center;
            starContainer.HorizontalAlignment = HorizontalAlignment.Right;

            view.HorizontalAlignment = HorizontalAlignment.Right;

            visuals.Orientation = Orientation.Horizontal;
            visuals.HorizontalAlignment = HorizontalAlignment.Right;
            visuals.VerticalAlignment = VerticalAlignment.Center;

            if (IsExplored(Sector))
            {
                //GameLog.Client.UI.DebugFormat("{0} {1} is explored and planets are displayed", Sector.Location, Sector.Name);
                foreach (Planet planet in system.Planets.Reverse())
                {
                    //GameLog.Client.UI.DebugFormat("planet {0}", planet.Name);
                    if (planet.PlanetType == PlanetType.Asteroids)
                    {
                        AsteroidsView asteroids = new AsteroidsView { Margin = new Thickness(0, 0, 14, 0) };
                        _ = visuals.Children.Add(asteroids);
                    }
                    else
                    {
                        Grid planetContainer = new Grid();
                        PlanetView3D planet3d = new PlanetView3D
                        {
                            Planet = planet,
                            StarSystem = system
                        };
                        planetContainer.Margin = new Thickness(0, 0, 14, 0);
                        planetContainer.HorizontalAlignment = HorizontalAlignment.Right;
                        planetContainer.VerticalAlignment = VerticalAlignment.Center;
                        _ = planetContainer.Children.Add(planet3d);
                        if (planet.HasFoodBonus)
                        {
                            Image bonusIcon = new Image
                            {
                                Source = TryFindResource("Images:Resources:Food") as ImageSource ?? FoodBonusImage,
                                Width = PlanetBonusIconSize,
                                Height = PlanetBonusIconSize,
                                Margin = new Thickness(
                                    0,
                                    0,
                                    -PlanetBonusIconSize / 2,
                                    PlanetBonusIconSize),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                ToolTip = GameContext.Current.Tables.EnumTables
                                    [typeof(PlanetBonus).Name]
                                    [PlanetBonus.Food.ToString()][0]
                            };
                            _ = planetContainer.Children.Add(bonusIcon);
                        }
                        else if (planet.HasEnergyBonus)
                        {
                            Image bonusIcon = new Image
                            {
                                Source = EnergyBonusImage,
                                Width = PlanetBonusIconSize,
                                Height = PlanetBonusIconSize,
                                Margin = new Thickness(
                                    0,
                                    0,
                                    -PlanetBonusIconSize / 2,
                                    PlanetBonusIconSize),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                ToolTip = GameContext.Current.Tables.EnumTables
                                    [typeof(PlanetBonus).Name]
                                    [PlanetBonus.Energy.ToString()][0]
                            };
                            _ = planetContainer.Children.Add(bonusIcon);
                        }
                        //GameLog.Client.UI.DebugFormat("trying add planetContainer {0}", planetContainer.Name);
                        _ = visuals.Children.Add(planetContainer);
                    }
                }
            }

            FrameworkElement star;
            string starToolTip = "";

            if (system.StarType == StarType.Nebula)
            {
                Image starImage = new Image { Source = NebulaImage };
                star = starImage;
            }
            else
            {
                star = new SunView3DRenderer { StarType = system.StarType };
            }

            if (system.StarType == StarType.Nebula)
            {
                if (IsExplored(Sector))
                {
                    starToolTip = string.Format(
                        ResourceManager.GetString("NEBULA_NAME_FORMAT"),
                        system.Name);
                }
            }
            else
            {
                if (IsExplored(Sector))
                {
                    starToolTip = system.Name + "\n";
                }

                starToolTip += ResourceManager.GetString(
                    "STAR_TYPE_" + system.StarType.ToString().ToUpper());
            }

            starToolTip += "\n" + string.Format(
                                      ResourceManager.GetString("QUADRANT_FORMAT"),
                                      ResourceManager.GetString(
                                          "QUADRANT_" + system.Quadrant.ToString().ToUpperInvariant()),
                                      ResourceManager.GetString("QUADRANT"));

            //GameLog.Client.UI.DebugFormat("trying add starContainer {0}", starContainer.Name);
            _ = starContainer.Children.Add(star);

            if (IsExplored(system.Sector))
            {
                if (system.HasDuraniumBonus)
                {
                    Image bonusIcon = new Image
                    {
                        Source = DuraniumBonusImage,
                        Width = SystemBonusIconSize,
                        Height = SystemBonusIconSize,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        ToolTip = GameContext.Current.Tables.EnumTables
                            [typeof(SystemBonus).Name]
                            [SystemBonus.Duranium.ToString()][0]
                    };
                    _ = starContainer.Children.Add(bonusIcon);
                }
                if (system.HasDilithiumBonus)
                {
                    Image bonusIcon = new Image
                    {
                        Source = DilithiumBonusImage,
                        Width = SystemBonusIconSize,
                        Height = SystemBonusIconSize,
                        Margin = new Thickness(
                            system.HasDuraniumBonus ? SystemBonusIconSize : 0,
                            0,
                            0,
                            0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        ToolTip = GameContext.Current.Tables.EnumTables
                            [typeof(SystemBonus).Name]
                            [SystemBonus.Dilithium.ToString()][0]
                    };
                    _ = starContainer.Children.Add(bonusIcon);
                }
            }

            star.ToolTip = starToolTip;

            _ = visuals.Children.Add(starContainer);

            view.SetValue(Grid.ColumnProperty, 1);
            view.Stretch = Stretch.Uniform;
            view.StretchDirection = StretchDirection.DownOnly;
            view.Child = visuals;
            //GameLog.Client.UI.DebugFormat("trying add view {0}", view.Name);
            _ = _grid.Children.Add(view);
        }

        private bool IsExplored(Sector sector)
        {
            return sector != null && AppContext.LocalPlayerEmpire.MapData.IsExplored(sector.Location);
        }

        private bool IsScanned(Sector sector)
        {
            return sector != null && AppContext.LocalPlayerEmpire.MapData.IsScanned(sector.Location);
        }
        #endregion
    }
}