// File:SystemProductionPanel.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Supremacy.Client;
using Supremacy.Client.Views;
using Supremacy.Economy;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.UI
{
    public sealed class SystemProductionPanel : Control
    {
        #region Delegates

        public delegate void SliderChangedEventHandler(object sender, SliderChangedEventArgs e);

        #endregion

        #region Fields

        public static readonly RoutedCommand ScrapFacilityCommand;

        private readonly VisualCollection _children;

        private readonly Grid _grid;
        private readonly UnitActivationBar _laborBar;
        private readonly TextBlock _laborPoolText;
        private readonly UnitActivationGroup _sliderGroup;

        private readonly TextBlock _energyActiveText;
        private readonly TextBlock _energyFacilityText;
        private readonly ImageBrush _energyImage;
        private readonly Border _energyImageBorder;
        private readonly Border _energyImageBorder_TOOLTIP;
        private readonly TextBlock _energyOutputText;
        private readonly TextBlock _energyScrapText;
        private readonly UnitActivationBar _energySlider;

        private readonly TextBlock _foodActiveText;
        private readonly TextBlock _foodFacilityText;
        private readonly ImageBrush _foodImage;
        private readonly Border _foodImageBorder;
        private readonly Border _foodImageBorder_TOOLTIP;
        private readonly TextBlock _foodOutputText;
        private readonly TextBlock _foodScrapText;
        private readonly UnitActivationBar _foodSlider;

        private readonly TextBlock _industryActiveText;
        private readonly TextBlock _industryFacilityText;
        private readonly ImageBrush _industryImage;
        private readonly Border _industryImageBorder;
        private readonly Border _industryImageBorder_TOOLTIP;
        private readonly TextBlock _industryOutputText;
        private readonly TextBlock _industryScrapText;
        private readonly UnitActivationBar _industrySlider;

        private readonly TextBlock _researchActiveText;
        private readonly TextBlock _researchFacilityText;
        private readonly ImageBrush _researchImage;
        private readonly Border _researchImageBorder;
        private readonly Border _researchImageBorder_TOOLTIP;
        private readonly TextBlock _researchOutputText;
        private readonly TextBlock _researchScrapText;
        private readonly UnitActivationBar _researchSlider;

        private readonly TextBlock _intelligenceActiveText;
        private readonly TextBlock _intelligenceFacilityText;
        private readonly ImageBrush _intelligenceImage;
        private readonly Border _intelligenceImageBorder;
        private readonly Border _intelligenceImageBorder_TOOLTIP;
        private readonly TextBlock _intelligenceOutputText;
        private readonly TextBlock _intelligenceScrapText;
        private readonly UnitActivationBar _intelligenceSlider;
        [NonSerialized]
        private string _text;
        #endregion

        #region Events

        public event SliderChangedEventHandler SliderChanged;

        #endregion

        #region Constructors

        static SystemProductionPanel()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SystemProductionPanel),
                new FrameworkPropertyMetadata(typeof(SystemProductionPanel)));

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(SystemProductionPanel),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            ScrapFacilityCommand = new RoutedCommand(
                "ScrapFacility",
                typeof(SystemProductionPanel));
        }

        public SystemProductionPanel()
        {
            const int rowSpacing = 10;  // ProdPanel was 14
            const int colSpacing = 6;  // ProdPanel was 14

            const int imageWidth = 62;  // ProdPanel was 100
            const int imageHeight = 62;  // ProdPanel was 77

            DataContextChanged += OnDataContextChanged;

            Brush headerBrush = FindResource("HeaderTextBrush") as Brush ?? Foreground;
            Brush paragraphBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground;

            SetResourceReference(
                FontFamilyProperty,
                ClientResources.DefaultFontFamilyKey);

            _children = new VisualCollection(this);

            _grid = new Grid
            {
                Width = 580
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.ColumnDefinitions.Add(new ColumnDefinition());

            _grid.ColumnDefinitions[0].Width = new GridLength(1.0, GridUnitType.Auto);
            _grid.ColumnDefinitions[1].Width = new GridLength(1.0, GridUnitType.Auto);
            _grid.ColumnDefinitions[2].Width = new GridLength(1.0, GridUnitType.Star);  // was .Star
            _grid.ColumnDefinitions[3].Width = new GridLength(1.0, GridUnitType.Auto);

            _grid.RowDefinitions.Add(new RowDefinition());
            _grid.RowDefinitions.Add(new RowDefinition());
            _grid.RowDefinitions.Add(new RowDefinition());
            _grid.RowDefinitions.Add(new RowDefinition());
            _grid.RowDefinitions.Add(new RowDefinition());
            _grid.RowDefinitions.Add(new RowDefinition());

            _grid.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Auto);
            _grid.RowDefinitions[1].Height = new GridLength(1.0, GridUnitType.Auto);
            _grid.RowDefinitions[2].Height = new GridLength(1.0, GridUnitType.Auto);
            _grid.RowDefinitions[3].Height = new GridLength(1.0, GridUnitType.Auto);
            _grid.RowDefinitions[4].Height = new GridLength(1.0, GridUnitType.Auto);
            _grid.RowDefinitions[5].Height = new GridLength(1.0, GridUnitType.Star);

            _sliderGroup = new UnitActivationGroup();

            _foodSlider = new UnitActivationBar();
            _industrySlider = new UnitActivationBar();
            _energySlider = new UnitActivationBar();
            _researchSlider = new UnitActivationBar();
            _intelligenceSlider = new UnitActivationBar();

            _foodSlider.HorizontalAlignment = HorizontalAlignment.Stretch;
            _industrySlider.HorizontalAlignment = HorizontalAlignment.Stretch;
            _energySlider.HorizontalAlignment = HorizontalAlignment.Stretch;
            _researchSlider.HorizontalAlignment = HorizontalAlignment.Stretch;
            _intelligenceSlider.HorizontalAlignment = HorizontalAlignment.Stretch;

            _foodSlider.Margin = new Thickness(0, 1, 0, 0);
            _industrySlider.Margin = new Thickness(0, 1, 0, 0);
            _energySlider.Margin = new Thickness(0, 1, 0, 0);
            _researchSlider.Margin = new Thickness(0, 1, 0, 0);
            _intelligenceSlider.Margin = new Thickness(0, 1, 0, 0);

            #region Arrange Visual Children

            _foodImage = new ImageBrush();
            _industryImage = new ImageBrush();
            _energyImage = new ImageBrush();
            _researchImage = new ImageBrush();
            _intelligenceImage = new ImageBrush();

            _foodFacilityText = new TextBlock();
            _industryFacilityText = new TextBlock();
            _energyFacilityText = new TextBlock();
            _researchFacilityText = new TextBlock();
            _intelligenceFacilityText = new TextBlock();

            _foodScrapText = new TextBlock();
            _industryScrapText = new TextBlock();
            _energyScrapText = new TextBlock();
            _researchScrapText = new TextBlock();
            _intelligenceScrapText = new TextBlock();

            _foodActiveText = new TextBlock();
            _industryActiveText = new TextBlock();
            _energyActiveText = new TextBlock();
            _researchActiveText = new TextBlock();
            _intelligenceActiveText = new TextBlock();

            _foodFacilityText.Foreground = paragraphBrush;
            _industryFacilityText.Foreground = paragraphBrush;
            _energyFacilityText.Foreground = paragraphBrush;
            _researchFacilityText.Foreground = paragraphBrush;
            _intelligenceFacilityText.Foreground = paragraphBrush;

            _foodScrapText.Foreground = Brushes.Red;
            _industryScrapText.Foreground = Brushes.Red;
            _energyScrapText.Foreground = Brushes.Red;
            _researchScrapText.Foreground = Brushes.Red;
            _intelligenceScrapText.Foreground = Brushes.Red;

            _foodActiveText.Foreground = paragraphBrush;
            _industryActiveText.Foreground = paragraphBrush;
            _energyActiveText.Foreground = paragraphBrush;
            _researchActiveText.Foreground = paragraphBrush;
            _intelligenceActiveText.Foreground = paragraphBrush;

            _foodOutputText = new TextBlock();
            _industryOutputText = new TextBlock();
            _energyOutputText = new TextBlock();
            _researchOutputText = new TextBlock();
            _intelligenceOutputText = new TextBlock();

            _foodOutputText.Foreground = headerBrush;
            _industryOutputText.Foreground = headerBrush;
            _energyOutputText.Foreground = headerBrush;
            _researchOutputText.Foreground = headerBrush;
            _intelligenceOutputText.Foreground = headerBrush;

            _foodOutputText.MinWidth = 100;
            _industryOutputText.MinWidth = 100;
            _energyOutputText.MinWidth = 100;
            _researchOutputText.MinWidth = 100;
            _intelligenceOutputText.MinWidth = 100;

            _foodFacilityText.VerticalAlignment = VerticalAlignment.Top;
            _industryFacilityText.VerticalAlignment = VerticalAlignment.Top;
            _energyFacilityText.VerticalAlignment = VerticalAlignment.Top;
            _researchFacilityText.VerticalAlignment = VerticalAlignment.Top;
            _intelligenceFacilityText.VerticalAlignment = VerticalAlignment.Top;

            _foodOutputText.VerticalAlignment = VerticalAlignment.Bottom;
            _industryOutputText.VerticalAlignment = VerticalAlignment.Bottom;
            _energyOutputText.VerticalAlignment = VerticalAlignment.Bottom;
            _researchOutputText.VerticalAlignment = VerticalAlignment.Bottom;
            _intelligenceOutputText.VerticalAlignment = VerticalAlignment.Bottom;

            _foodFacilityText.HorizontalAlignment = HorizontalAlignment.Left;
            _industryFacilityText.HorizontalAlignment = HorizontalAlignment.Left;
            _energyFacilityText.HorizontalAlignment = HorizontalAlignment.Left;
            _researchFacilityText.HorizontalAlignment = HorizontalAlignment.Left;
            _intelligenceFacilityText.HorizontalAlignment = HorizontalAlignment.Left;

            _foodOutputText.HorizontalAlignment = HorizontalAlignment.Left;
            _industryOutputText.HorizontalAlignment = HorizontalAlignment.Left;
            _energyOutputText.HorizontalAlignment = HorizontalAlignment.Left;
            _researchOutputText.HorizontalAlignment = HorizontalAlignment.Left;
            _intelligenceOutputText.HorizontalAlignment = HorizontalAlignment.Left;

            _foodFacilityText.Margin = new Thickness(colSpacing, rowSpacing, colSpacing, 0);
            _industryFacilityText.Margin = new Thickness(colSpacing, rowSpacing, colSpacing, 0);
            _energyFacilityText.Margin = new Thickness(colSpacing, rowSpacing, colSpacing, 0);
            _researchFacilityText.Margin = new Thickness(colSpacing, rowSpacing, colSpacing, 0);
            _intelligenceFacilityText.Margin = new Thickness(colSpacing, rowSpacing, colSpacing, 0);

            _foodOutputText.Margin = new Thickness(colSpacing, rowSpacing, 0, 0);
            _industryOutputText.Margin = new Thickness(colSpacing, rowSpacing, 0, 0);
            _energyOutputText.Margin = new Thickness(colSpacing, rowSpacing, 0, 0);
            _researchOutputText.Margin = new Thickness(colSpacing, rowSpacing, 0, 0);
            _intelligenceOutputText.Margin = new Thickness(colSpacing, rowSpacing, 0, 0);

            _foodFacilityText.SetValue(Grid.ColumnProperty, 1);
            _industryFacilityText.SetValue(Grid.ColumnProperty, 1);
            _energyFacilityText.SetValue(Grid.ColumnProperty, 1);
            _researchFacilityText.SetValue(Grid.ColumnProperty, 1);
            _intelligenceFacilityText.SetValue(Grid.ColumnProperty, 1);

            _foodFacilityText.SetValue(Grid.RowProperty, 0);
            _industryFacilityText.SetValue(Grid.RowProperty, 1);
            _energyFacilityText.SetValue(Grid.RowProperty, 2);
            _researchFacilityText.SetValue(Grid.RowProperty, 3);
            _intelligenceFacilityText.SetValue(Grid.RowProperty, 4);

            _foodOutputText.SetValue(Grid.ColumnProperty, 1);
            _industryOutputText.SetValue(Grid.ColumnProperty, 1);
            _energyOutputText.SetValue(Grid.ColumnProperty, 1);
            _researchOutputText.SetValue(Grid.ColumnProperty, 1);
            _intelligenceOutputText.SetValue(Grid.ColumnProperty, 1);

            _foodOutputText.SetValue(Grid.RowProperty, 0);
            _industryOutputText.SetValue(Grid.RowProperty, 1);
            _energyOutputText.SetValue(Grid.RowProperty, 2);
            _researchOutputText.SetValue(Grid.RowProperty, 3);
            _intelligenceOutputText.SetValue(Grid.RowProperty, 4);

            _ = _grid.Children.Add(_foodFacilityText);
            _ = _grid.Children.Add(_industryFacilityText);
            _ = _grid.Children.Add(_energyFacilityText);
            _ = _grid.Children.Add(_researchFacilityText);
            _ = _grid.Children.Add(_intelligenceFacilityText);

            _ = _grid.Children.Add(_foodOutputText);
            _ = _grid.Children.Add(_industryOutputText);
            _ = _grid.Children.Add(_energyOutputText);
            _ = _grid.Children.Add(_researchOutputText);
            _ = _grid.Children.Add(_intelligenceOutputText);

            /* FOOD IMAGE */
            _foodImageBorder = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(2.0),
                CornerRadius = new CornerRadius(2.0),
                MinWidth = imageWidth,
                MinHeight = imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _foodImage
            };
            _foodImageBorder_TOOLTIP = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(0.0),
                CornerRadius = new CornerRadius(0.0),
                MinWidth = 8 * imageWidth,
                MinHeight = 8 * imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _foodImage
            };
            _foodImageBorder.ToolTip = _foodImageBorder_TOOLTIP;
            _foodImageBorder.SetValue(Grid.ColumnProperty, 0);
            _foodImageBorder.SetValue(Grid.RowProperty, 0);
            _foodImageBorder.PreviewMouseDown += ImageBorder_PreviewMouseDown;
            _foodImageBorder.PreviewMouseUp += ImageBorder_PreviewMouseUp;
            _ = _grid.Children.Add(_foodImageBorder);

            /* INDUSTRY IMAGE */
            _industryImageBorder = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(2.0),
                CornerRadius = new CornerRadius(2.0),
                MinWidth = imageWidth,
                MinHeight = imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _industryImage
            };
            _industryImageBorder_TOOLTIP = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(0.0),
                CornerRadius = new CornerRadius(0.0),
                MinWidth = 8 * imageWidth,
                MinHeight = 8 * imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _industryImage
            };
            _industryImageBorder.ToolTip = _industryImageBorder_TOOLTIP;
            _industryImageBorder.SetValue(Grid.ColumnProperty, 0);
            _industryImageBorder.SetValue(Grid.RowProperty, 1);
            _industryImageBorder.PreviewMouseDown += ImageBorder_PreviewMouseDown;
            _industryImageBorder.PreviewMouseUp += ImageBorder_PreviewMouseUp;
            _ = _grid.Children.Add(_industryImageBorder);

            /* ENERGY IMAGE */
            _energyImageBorder = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(2.0),
                CornerRadius = new CornerRadius(2.0),
                MinWidth = imageWidth,
                MinHeight = imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _energyImage
            };
            _energyImageBorder_TOOLTIP = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(0.0),
                CornerRadius = new CornerRadius(0.0),
                MinWidth = 8 * imageWidth,
                MinHeight = 8 * imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _energyImage
            };
            _energyImageBorder.ToolTip = _energyImageBorder_TOOLTIP;
            _energyImageBorder.SetValue(Grid.ColumnProperty, 0);
            _energyImageBorder.SetValue(Grid.RowProperty, 2);
            _energyImageBorder.PreviewMouseDown += ImageBorder_PreviewMouseDown;
            _energyImageBorder.PreviewMouseUp += ImageBorder_PreviewMouseUp;
            _ = _grid.Children.Add(_energyImageBorder);

            /* RESEARCH IMAGE */
            _researchImageBorder = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(2.0),
                CornerRadius = new CornerRadius(2.0),
                MinWidth = imageWidth,
                MinHeight = imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _researchImage
            };
            _researchImageBorder_TOOLTIP = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(0.0),
                CornerRadius = new CornerRadius(0.0),
                MinWidth = 8 * imageWidth,
                MinHeight = 8 * imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _researchImage
            };
            _researchImageBorder.ToolTip = _researchImageBorder_TOOLTIP;
            _researchImageBorder.SetValue(Grid.ColumnProperty, 0);
            _researchImageBorder.SetValue(Grid.RowProperty, 3);
            _researchImageBorder.PreviewMouseDown += ImageBorder_PreviewMouseDown;
            _researchImageBorder.PreviewMouseUp += ImageBorder_PreviewMouseUp;
            _ = _grid.Children.Add(_researchImageBorder);

            /* INTELLIGENCE IMAGE */
            _intelligenceImageBorder = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(2.0),
                CornerRadius = new CornerRadius(2.0),
                MinWidth = imageWidth,
                MinHeight = imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _intelligenceImage
            };
            _intelligenceImageBorder_TOOLTIP = new Border
            {
                BorderBrush = FindResource("DefaultTextBrush") as Brush ?? Foreground,
                BorderThickness = new Thickness(0.0),
                CornerRadius = new CornerRadius(0.0),
                MinWidth = 8 * imageWidth,
                MinHeight = 8 * imageHeight,
                Margin = new Thickness(0, rowSpacing, 0, 0),
                Background = _intelligenceImage
            };
            _intelligenceImageBorder.ToolTip = _intelligenceImageBorder_TOOLTIP;
            _intelligenceImageBorder.SetValue(Grid.ColumnProperty, 0);
            _intelligenceImageBorder.SetValue(Grid.RowProperty, 4);
            _intelligenceImageBorder.PreviewMouseDown += ImageBorder_PreviewMouseDown;
            _intelligenceImageBorder.PreviewMouseUp += ImageBorder_PreviewMouseUp;
            _ = _grid.Children.Add(_intelligenceImageBorder);

            _foodSlider.SetValue(Grid.ColumnProperty, 2);
            _industrySlider.SetValue(Grid.ColumnProperty, 2);
            _energySlider.SetValue(Grid.ColumnProperty, 2);
            _researchSlider.SetValue(Grid.ColumnProperty, 2);
            _intelligenceSlider.SetValue(Grid.ColumnProperty, 2);

            _foodSlider.VerticalAlignment = VerticalAlignment.Center;
            _industrySlider.VerticalAlignment = VerticalAlignment.Center;
            _energySlider.VerticalAlignment = VerticalAlignment.Center;
            _researchSlider.VerticalAlignment = VerticalAlignment.Center;
            _intelligenceSlider.VerticalAlignment = VerticalAlignment.Center;

            _foodSlider.MinHeight = 28;
            _industrySlider.MinHeight = 28;
            _energySlider.MinHeight = 28;
            _researchSlider.MinHeight = 28;
            _intelligenceSlider.MinHeight = 28;

            _foodSlider.SetValue(Grid.RowProperty, 0);
            _industrySlider.SetValue(Grid.RowProperty, 1);
            _energySlider.SetValue(Grid.RowProperty, 2);
            _researchSlider.SetValue(Grid.RowProperty, 3);
            _intelligenceSlider.SetValue(Grid.RowProperty, 4);

            _foodSlider.IsEnabled = true;
            _industrySlider.IsEnabled = true;
            _energySlider.IsEnabled = true;
            _researchSlider.IsEnabled = true;
            _intelligenceSlider.IsEnabled = true;

            _ = _grid.Children.Add(_foodSlider);
            _ = _grid.Children.Add(_industrySlider);
            _ = _grid.Children.Add(_energySlider);
            _ = _grid.Children.Add(_researchSlider);
            _ = _grid.Children.Add(_intelligenceSlider);

            _laborBar = new UnitActivationBar
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            _laborBar.SetValue(Grid.ColumnProperty, 1);
            _laborBar.SetValue(Grid.ColumnSpanProperty, 4);
            _laborBar.SetValue(Grid.RowProperty, 5);
            _laborBar.VerticalAlignment = VerticalAlignment.Top;
            _laborBar.Margin = new Thickness(0, rowSpacing * 2, 0, rowSpacing);
            _laborBar.Height = 28;
            _laborBar.IsReadOnly = true;
            _laborBar.IsEnabled = true;
            _laborBar.Foreground = Brushes.Red;

            _laborPoolText = new TextBlock();
            _laborPoolText.SetValue(Grid.ColumnProperty, 0);
            _laborPoolText.SetValue(Grid.RowProperty, 5);
            _laborPoolText.VerticalAlignment = VerticalAlignment.Top;
            _laborPoolText.HorizontalAlignment = HorizontalAlignment.Right;
            _laborPoolText.Margin = new Thickness(0, rowSpacing * 2, colSpacing, rowSpacing);
            _laborPoolText.FontSize = 20;
            _laborPoolText.Text = string.Format(ResourceManager.GetString("Labor_Pool"));
            _laborPoolText.Foreground = headerBrush;

            _ = _grid.Children.Add(_laborBar);
            _ = _grid.Children.Add(_laborPoolText);

            #endregion

            _ = _children.Add(_grid);

            _sliderGroup.PoolBar = _laborBar;
            //_laborBar.Foreground = "";
            _sliderGroup.FreePoolSizeChanged += SliderGroup_FreePoolSizeChanged;
            _sliderGroup.PoolSizeChanged += SliderGroup_PoolSizeChanged;

            _sliderGroup.Children.Add(_foodSlider);
            _sliderGroup.Children.Add(_industrySlider);
            _sliderGroup.Children.Add(_energySlider);
            _sliderGroup.Children.Add(_researchSlider);
            _sliderGroup.Children.Add(_intelligenceSlider);

            _ = CommandBindings.Add(
                new CommandBinding(
                    ScrapFacilityCommand,
                    ScrapFacilityCommand_Executed));
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ColonyScreenPresentationModel model)
            {
                model.SelectedColonyChanged -= OnColonyChanged;
            }

            model = e.NewValue as ColonyScreenPresentationModel;

            if (model == null)
            {
                return;
            }

            model.SelectedColonyChanged += OnColonyChanged;

            if (model.SelectedColony != null)
            {
                Reset();
            }
        }

        private void OnColonyChanged(object sender, EventArgs e)
        {
            Reset();
        }

        #endregion

        #region Event Handlers

        private void ImageBorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right)
            {
                return;
            }

            if ((sender == _foodImageBorder) ||
                (sender == _industryImageBorder) ||
                (sender == _energyImageBorder) ||
                (sender == _researchImageBorder) ||
                (sender == _intelligenceImageBorder)
                )
            {
                e.Handled = true;
            }
        }

        private void ImageBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Colony colony = Colony;
            if (colony == null)
            {
                return;
            }

            ColonyScreenPresentationModel model = Model;
            if (model == null)
            {
                return;
            }

            int delta = 0;
            ProductionCategory? category = null;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                delta = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                    || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                    ? -1
                    : 1;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                delta = -1;
            }

            if (sender == _foodImageBorder)
            {
                category = ProductionCategory.Food;
            }
            else if (sender == _industryImageBorder)
            {
                category = ProductionCategory.Industry;
            }
            else if (sender == _energyImageBorder)
            {
                category = ProductionCategory.Energy;
            }
            else if (sender == _researchImageBorder)
            {
                category = ProductionCategory.Research;
            }
            else if (sender == _intelligenceImageBorder)
            {
                category = ProductionCategory.Intelligence;
            }

            if (!category.HasValue)
            {
                return;
            }

            ICommand command = (delta < 0) ? Model.UnscrapFacilityCommand : Model.ScrapFacilityCommand;
            if ((command == null) || !command.CanExecute(category.Value))
            {
                return;
            }

            command.Execute(category.Value);

            e.Handled = true;

            UpdateImages();
        }

        private static void ScrapFacilityCommand_Executed(object sender, ExecutedRoutedEventArgs e) { }

        private void Slider_ActiveUnitsChanged(object sender, DependencyPropertyChangedEventArgs<int> e)
        {
            //GameLog.Client.UI.DebugFormat("slider_ActiveUnitsChanged...");
            Colony colony = Colony;
            if (colony == null)
            {
                return;
            }


            if ((!(sender is UnitActivationBar slider)) || (slider == _laborBar))
            {
                return;
            }

            int delta = Math.Abs(e.NewValue - e.OldValue);
            if (delta != 0)
            {
                int i;
                bool activate = e.NewValue > e.OldValue;
                ProductionCategory category = default;
                TextBlock outputText = null;
                TextBlock facilityText = null;
                TextBlock activeText = null;

                if (slider == _foodSlider)
                {
                    category = ProductionCategory.Food;
                    outputText = _foodOutputText;
                    activeText = _foodActiveText;
                    facilityText = _foodFacilityText;
                }
                else if (slider == _industrySlider)
                {
                    category = ProductionCategory.Industry;
                    outputText = _industryOutputText;
                    activeText = _industryActiveText;
                    facilityText = _industryFacilityText;
                }
                else if (slider == _energySlider)
                {
                    category = ProductionCategory.Energy;
                    outputText = _energyOutputText;
                    activeText = _energyActiveText;
                    facilityText = _energyFacilityText;
                }
                else if (slider == _researchSlider)
                {
                    category = ProductionCategory.Research;
                    outputText = _researchOutputText;
                    activeText = _researchActiveText;
                    facilityText = _researchFacilityText;
                }
                else if (slider == _intelligenceSlider)
                {
                    category = ProductionCategory.Intelligence;
                    outputText = _intelligenceOutputText;
                    activeText = _intelligenceActiveText;
                    facilityText = _intelligenceFacilityText;
                }

                _text = "Step_7102: " + colony.Location + ": old= " + e.OldValue + ", new= " + e.NewValue + " for slider " + category;
                Console.WriteLine(_text);
                GameLog.Client.GameData.DebugFormat(_text);

                for (i = 1; i <= delta; i++) // one by one
                {
                    if (activate)
                    {
                        ICommand activateCommand = Model.ActivateFacilityCommand;
                        if ((activateCommand != null) && activateCommand.CanExecute(category))
                        {
                            activateCommand.Execute(category);
                        }
                        //_text = "Step_2101: slider_ActiveUnitsChanged... category " + category + " IN-CREASED " + delta;
                        //Console.WriteLine(_text);
                        //GameLog.Client.ProductionDetails.DebugFormat(_text);
                    }
                    else
                    {
                        ICommand deactivateCommand = Model.DeactivateFacilityCommand;
                        if ((deactivateCommand != null) && deactivateCommand.CanExecute(category))
                        {
                            deactivateCommand.Execute(category);
                        }
                        //_text = "Step_2102: slider_ActiveUnitsChanged... category " + category + " DE-CREASED " + delta;
                        //Console.WriteLine(_text);
                        //GameLog.Client.ProductionDetails.DebugFormat(_text);
                    }
                }


                if ((activeText != null) && (facilityText != null))
                {
                    activeText.Text = string.Format(
                        ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                        colony.GetActiveFacilities(category),
                        colony.GetTotalFacilities(category));
                    activeText.Inlines.FirstInline.Foreground = activeText.Foreground;
                    facilityText.Inlines.Clear();
                    facilityText.Text = ResourceManager.GetString(
                        colony.GetFacilityType(category).Name);
                    facilityText.Inlines.Add(new LineBreak());
                    facilityText.Inlines.Add(activeText.Inlines.FirstInline);
                }

                if (outputText != null)
                {
                    outputText.Text = string.Format(
                        "Output {1}:  {0}",
                        colony.GetProductionOutput(category),
                        category);
                }

                slider.ActiveUnits = colony.GetActiveFacilities(category);

                SliderChanged?.Invoke(this, new SliderChangedEventArgs(category));
                slider.IsEnabled = true;
            }
        }

        private void SliderGroup_FreePoolSizeChanged(object sender, EventArgs e)
        {
            _laborBar.ActiveUnits = _sliderGroup.FreePoolSize / _laborBar.UnitCost;
            _foodSlider.IsEnabled = true;
            _industrySlider.IsEnabled = true;
            _energySlider.IsEnabled = true;
            _researchSlider.IsEnabled = true;
            _intelligenceSlider.IsEnabled = true;
        }

        private void SliderGroup_PoolSizeChanged(object sender, EventArgs e)
        {
            _laborBar.Units = _sliderGroup.PoolSize / _laborBar.UnitCost;
            _foodSlider.IsEnabled = true;
            _industrySlider.IsEnabled = true;
            _energySlider.IsEnabled = true;
            _researchSlider.IsEnabled = true;
            _intelligenceSlider.IsEnabled = true;
        }

        #endregion

        #region Properties

        public ColonyScreenPresentationModel Model => DataContext as ColonyScreenPresentationModel;

        public Colony Colony
        {
            get
            {
                ColonyScreenPresentationModel model = Model;
                return model?.SelectedColony;
            }
        }

        #endregion

        #region Methods

        public void Reset()
        {
            UpdateImages();
            ResetSliders();
            _foodSlider.IsEnabled = true;
            _industrySlider.IsEnabled = true;
            _energySlider.IsEnabled = true;
            _researchSlider.IsEnabled = true;
            _intelligenceSlider.IsEnabled = true;
        }

        private void ResetSliders()
        {
            Colony colony = Colony;
            if (colony == null)
            {
                _sliderGroup.ResetPool(0);
                _foodSlider.Units = 0;
                _industrySlider.Units = 0;
                _energySlider.Units = 0;
                _researchSlider.Units = 0;
                _intelligenceSlider.Units = 0;
            }
            else
            {
                _foodSlider.ActiveUnitsChanged -= Slider_ActiveUnitsChanged;
                _industrySlider.ActiveUnitsChanged -= Slider_ActiveUnitsChanged;
                _energySlider.ActiveUnitsChanged -= Slider_ActiveUnitsChanged;
                _researchSlider.ActiveUnitsChanged -= Slider_ActiveUnitsChanged;
                _intelligenceSlider.ActiveUnitsChanged -= Slider_ActiveUnitsChanged;

                _foodSlider.UnitCost = colony.GetFacilityType(ProductionCategory.Food).LaborCost;
                _industrySlider.UnitCost = colony.GetFacilityType(ProductionCategory.Industry).LaborCost;
                _energySlider.UnitCost = colony.GetFacilityType(ProductionCategory.Energy).LaborCost;
                _researchSlider.UnitCost = colony.GetFacilityType(ProductionCategory.Research).LaborCost;
                _intelligenceSlider.UnitCost = colony.GetFacilityType(ProductionCategory.Intelligence).LaborCost;

                _foodSlider.Units = colony.GetTotalFacilities(ProductionCategory.Food);
                _industrySlider.Units = colony.GetTotalFacilities(ProductionCategory.Industry);
                _energySlider.Units = colony.GetTotalFacilities(ProductionCategory.Energy);
                _researchSlider.Units = colony.GetTotalFacilities(ProductionCategory.Research);
                _intelligenceSlider.Units = colony.GetTotalFacilities(ProductionCategory.Intelligence);

                _sliderGroup.ResetPool(colony.Population.CurrentValue);

                int LaborPool = colony.GetAvailableLabor() / 10;

                // now a SitRep
                //GameLog.Client.Production.DebugFormat("for {8}" + Environment.NewLine + "   Turn {7}: Pop={0},Food={1},Ind={2},Energy={3},Research={4},Intel={5},FreePoolSize={6} for {8} ({9})",
                //    colony.Population.CurrentValue,
                //    colony.GetActiveFacilities(ProductionCategory.Food),
                //    colony.GetActiveFacilities(ProductionCategory.Industry),
                //    colony.GetActiveFacilities(ProductionCategory.Energy),
                //    colony.GetActiveFacilities(ProductionCategory.Research),
                //    colony.GetActiveFacilities(ProductionCategory.Intelligence),
                //    LaborPool,
                //    GameContext.Current.TurnNumber,
                //    Name,
                //    colony.Name,
                //    colony.Location
                //    );
                /*_laborBar.ActiveUnits doesn't work */

                _foodSlider.ActiveUnits = colony.GetActiveFacilities(ProductionCategory.Food);
                _industrySlider.ActiveUnits = colony.GetActiveFacilities(ProductionCategory.Industry);
                _energySlider.ActiveUnits = colony.GetActiveFacilities(ProductionCategory.Energy);
                _researchSlider.ActiveUnits = colony.GetActiveFacilities(ProductionCategory.Research);
                _intelligenceSlider.ActiveUnits = colony.GetActiveFacilities(ProductionCategory.Intelligence);

                _foodSlider.ActiveUnitsChanged += Slider_ActiveUnitsChanged;
                _industrySlider.ActiveUnitsChanged += Slider_ActiveUnitsChanged;
                _energySlider.ActiveUnitsChanged += Slider_ActiveUnitsChanged;
                _researchSlider.ActiveUnitsChanged += Slider_ActiveUnitsChanged;
                _intelligenceSlider.ActiveUnitsChanged += Slider_ActiveUnitsChanged;

                _foodOutputText.Text = string.Format(
                    "Output {1}:  {0}",
                    colony.GetProductionOutput(ProductionCategory.Food),
                    ResourceManager.GetString(
                        "PRODUCTION_CATEGORY_"
                        + ProductionCategory.Food.ToString().ToUpperInvariant()));
                _industryOutputText.Text = string.Format(
                    "Output {1}:  {0}",
                    colony.GetProductionOutput(ProductionCategory.Industry),
                    ResourceManager.GetString(
                        "PRODUCTION_CATEGORY_"
                        + ProductionCategory.Industry.ToString().ToUpperInvariant()));
                _energyOutputText.Text = string.Format(
                    "Output {1}:  {0}",
                    colony.GetProductionOutput(ProductionCategory.Energy),
                    ResourceManager.GetString(
                        "PRODUCTION_CATEGORY_"
                        + ProductionCategory.Energy.ToString().ToUpperInvariant()));
                _researchOutputText.Text = string.Format(
                    "Output {1}:  {0}",
                    colony.GetProductionOutput(ProductionCategory.Research),
                    ResourceManager.GetString(
                        "PRODUCTION_CATEGORY_"
                        + ProductionCategory.Research.ToString().ToUpperInvariant()));
                _intelligenceOutputText.Text = string.Format(
                    "Output {1}:  {0}",
                    colony.GetProductionOutput(ProductionCategory.Intelligence),
                    ResourceManager.GetString(
                        "PRODUCTION_CATEGORY_"
                        + ProductionCategory.Intelligence.ToString().ToUpperInvariant()));
            }

            _foodSlider.InvalidateVisual();
            _industrySlider.InvalidateVisual();
            _energySlider.InvalidateVisual();
            _researchSlider.InvalidateVisual();
            _intelligenceSlider.InvalidateVisual();
            _laborBar.InvalidateVisual();

            _foodSlider.IsEnabled = true;
            _industrySlider.IsEnabled = true;
            _energySlider.IsEnabled = true;
            _researchSlider.IsEnabled = true;
            _intelligenceSlider.IsEnabled = true;
        }

        private void UpdateImages()
        {
            Colony colony = Colony;
            if (colony == null)
            {
                _foodImage.ImageSource = TechObjectImageConverter.Convert("");
                _industryImage.ImageSource = TechObjectImageConverter.Convert("");
                _energyImage.ImageSource = TechObjectImageConverter.Convert("");
                _researchImage.ImageSource = TechObjectImageConverter.Convert("");
                _intelligenceImage.ImageSource = TechObjectImageConverter.Convert("");

                _foodFacilityText.Text = "";
                _industryFacilityText.Text = "";
                _energyFacilityText.Text = "";
                _researchFacilityText.Text = "";
                _intelligenceFacilityText.Text = "";

                Visibility = Visibility.Hidden;
            }
            else
            {
                _foodImage.ImageSource = TechObjectImageConverter.Convert(
                    colony.GetFacilityType(ProductionCategory.Food).Image);
                _industryImage.ImageSource = TechObjectImageConverter.Convert(
                    colony.GetFacilityType(ProductionCategory.Industry).Image);
                _energyImage.ImageSource = TechObjectImageConverter.Convert(
                    colony.GetFacilityType(ProductionCategory.Energy).Image);
                _researchImage.ImageSource = TechObjectImageConverter.Convert(
                    colony.GetFacilityType(ProductionCategory.Research).Image);
                _intelligenceImage.ImageSource = TechObjectImageConverter.Convert(
                    colony.GetFacilityType(ProductionCategory.Intelligence).Image);

                _foodFacilityText.Inlines.Clear();
                _industryFacilityText.Inlines.Clear();
                _energyFacilityText.Inlines.Clear();
                _researchFacilityText.Inlines.Clear();
                _intelligenceFacilityText.Inlines.Clear();

                _foodActiveText.Text = string.Format(
                    ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                    colony.GetActiveFacilities(ProductionCategory.Food),
                    colony.GetTotalFacilities(ProductionCategory.Food),
                    _foodOutputText.Text);
                _industryActiveText.Text = string.Format(
                    ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                    colony.GetActiveFacilities(ProductionCategory.Industry),
                    colony.GetTotalFacilities(ProductionCategory.Industry),
                    _industryOutputText.Text);
                _energyActiveText.Text = string.Format(
                    ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                    colony.GetActiveFacilities(ProductionCategory.Energy),
                    colony.GetTotalFacilities(ProductionCategory.Energy),
                    _energyOutputText.Text);
                _researchActiveText.Text = string.Format(
                    ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                    colony.GetActiveFacilities(ProductionCategory.Research),
                    colony.GetTotalFacilities(ProductionCategory.Research),
                    _researchOutputText.Text);
                _intelligenceActiveText.Text = string.Format(
                    ResourceManager.GetString("ACTIVE_FACILITIES_FORMAT_STRING"),
                    colony.GetActiveFacilities(ProductionCategory.Intelligence),
                    colony.GetTotalFacilities(ProductionCategory.Intelligence),
                    _intelligenceOutputText.Text);

                _foodFacilityText.Inlines.Clear();
                _industryFacilityText.Inlines.Clear();
                _energyFacilityText.Inlines.Clear();
                _researchFacilityText.Inlines.Clear();
                _intelligenceFacilityText.Inlines.Clear();

                _foodFacilityText.Text = ResourceManager.GetString(
                    colony.GetFacilityType(ProductionCategory.Food).Name);
                _industryFacilityText.Text = ResourceManager.GetString(
                    colony.GetFacilityType(ProductionCategory.Industry).Name);
                _energyFacilityText.Text = ResourceManager.GetString(
                    colony.GetFacilityType(ProductionCategory.Energy).Name);
                _researchFacilityText.Text = ResourceManager.GetString(
                    colony.GetFacilityType(ProductionCategory.Research).Name);
                _intelligenceFacilityText.Text = ResourceManager.GetString(
                    colony.GetFacilityType(ProductionCategory.Intelligence).Name);

                _foodActiveText.Inlines.FirstInline.Foreground = _foodActiveText.Foreground;
                _industryActiveText.Inlines.FirstInline.Foreground = _industryActiveText.Foreground;
                _energyActiveText.Inlines.FirstInline.Foreground = _energyActiveText.Foreground;
                _researchActiveText.Inlines.FirstInline.Foreground = _researchActiveText.Foreground;
                _intelligenceActiveText.Inlines.FirstInline.Foreground = _intelligenceActiveText.Foreground;

                _foodActiveText.FontSize = 10d;
                _industryActiveText.FontSize = 10d;
                _energyActiveText.FontSize = 10d;
                _researchActiveText.FontSize = 10d;
                _intelligenceActiveText.FontSize = 10d;

                _foodFacilityText.Inlines.Add(new LineBreak());
                _foodFacilityText.Inlines.Add(_foodActiveText.Inlines.FirstInline);
                _industryFacilityText.Inlines.Add(new LineBreak());
                _industryFacilityText.Inlines.Add(_industryActiveText.Inlines.FirstInline);
                _energyFacilityText.Inlines.Add(new LineBreak());
                _energyFacilityText.Inlines.Add(_energyActiveText.Inlines.FirstInline);
                _researchFacilityText.Inlines.Add(new LineBreak());
                _researchFacilityText.Inlines.Add(_researchActiveText.Inlines.FirstInline);
                _intelligenceFacilityText.Inlines.Add(new LineBreak());
                _intelligenceFacilityText.Inlines.Add(_intelligenceActiveText.Inlines.FirstInline);

                if (colony.GetScrappedFacilities(ProductionCategory.Food) > 0)
                {
                    _foodScrapText.Text = " " + string.Format(
                        ResourceManager.GetString("FACILITY_SCRAP_FORMAT_STRING"),
                        colony.GetScrappedFacilities(ProductionCategory.Food));
                    _foodScrapText.Inlines.FirstInline.Foreground = Brushes.Red;
                    _foodFacilityText.Inlines.Add(_foodScrapText.Inlines.FirstInline);
                }
                if (colony.GetScrappedFacilities(ProductionCategory.Industry) > 0)
                {
                    _industryScrapText.Text = " " + string.Format(
                        ResourceManager.GetString("FACILITY_SCRAP_FORMAT_STRING"),
                        colony.GetScrappedFacilities(ProductionCategory.Industry));
                    _industryScrapText.Inlines.FirstInline.Foreground = Brushes.Red;
                    _industryFacilityText.Inlines.Add(_industryScrapText.Inlines.FirstInline);
                }
                if (colony.GetScrappedFacilities(ProductionCategory.Energy) > 0)
                {
                    _energyScrapText.Text = " " + string.Format(
                        ResourceManager.GetString("FACILITY_SCRAP_FORMAT_STRING"),
                        colony.GetScrappedFacilities(ProductionCategory.Energy));
                    _energyScrapText.Inlines.FirstInline.Foreground = Brushes.Red;
                    _energyFacilityText.Inlines.Add(_energyScrapText.Inlines.FirstInline);
                }
                if (colony.GetScrappedFacilities(ProductionCategory.Research) > 0)
                {
                    _researchScrapText.Text = " " + string.Format(
                        ResourceManager.GetString("FACILITY_SCRAP_FORMAT_STRING"),
                        colony.GetScrappedFacilities(ProductionCategory.Research));
                    _researchScrapText.Inlines.FirstInline.Foreground = Brushes.Red;
                    _researchFacilityText.Inlines.Add(_researchScrapText.Inlines.FirstInline);
                }
                if (colony.GetScrappedFacilities(ProductionCategory.Intelligence) > 0)
                {
                    _intelligenceScrapText.Text = " " + string.Format(
                        ResourceManager.GetString("FACILITY_SCRAP_FORMAT_STRING"),
                        colony.GetScrappedFacilities(ProductionCategory.Intelligence));
                    _intelligenceScrapText.Inlines.FirstInline.Foreground = Brushes.Red;
                    _intelligenceFacilityText.Inlines.Add(_intelligenceScrapText.Inlines.FirstInline);
                }
                Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Control Member Overrides

        #region Properties

        protected override int VisualChildrenCount => _children.Count;

        #endregion

        #region Methods

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        #endregion

        #endregion

        #region SliderChangedEventArgs Type

        public class SliderChangedEventArgs : EventArgs
        {
            #region Fields


            #endregion

            #region Constructors

            public SliderChangedEventArgs(ProductionCategory productionCategory)
            {
                ProductionCategory = productionCategory;
            }

            #endregion

            #region Properties

            public ProductionCategory ProductionCategory { get; }

            #endregion
        }

        #endregion
    }
}