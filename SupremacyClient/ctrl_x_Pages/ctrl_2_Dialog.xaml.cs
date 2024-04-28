// <!-- File:ctrl_2_Dialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Supremacy.Client.Views;
using Supremacy.Encyclopedia;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Utility;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;
using System.Windows.Navigation;

namespace Supremacy.Client
{
    [TemplatePart(Name = "PART_ResearchFieldItemsHost", Type = typeof(Border))]
    [TemplatePart(Name = "PART_ResearchMatrixHost", Type = typeof(Border))]
    [TemplatePart(Name = "PART_ApplicationDetailsHost", Type = typeof(Border))]
    [TemplatePart(Name = "PART_EncyclopediaEntries", Type = typeof(TreeView))]
    [TemplatePart(Name = "PART_SearchText", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_EncyclopediaViewer", Type = typeof(FlowDocumentScrollViewer))]
    //[TemplatePart(Name = "PART_EncyclopediaViewer", Type = typeof(FlowDocumentScrollViewer))]
    /// <summary>
    /// Interaction logic for 2_Dialog.xaml.cs
    /// </summary>
    public partial class CTRL_2_Dialog
    {
        //public ResearchScreen([NotNull] IUnityContainer container) : base(container)
        //{
        //    _researchFieldGrid = new Grid();
        //    _researchMatrixGrid = new Grid();
        //    _selectedApplication = null;

        //    LoadEncyclopediaEntries();

        //    SetValue(Grid.IsSharedSizeScopeProperty, true);

        //    ResourceDictionary themeResources;

        //    if (ThemeHelper.TryLoadThemeResources(out themeResources))
        //        Resources.MergedDictionaries.Add(themeResources);
        //}

        //[TemplatePart(Name = "PART_EncyclopediaViewer", Type = typeof(FlowDocumentScrollViewer))]
        //private Border _researchFieldItemsControl;
        //private Border _researchMatrixHost;
        //private Border _applicationDetailsHost;
        //private readonly Grid _researchFieldGrid;
        //private readonly Grid _researchMatrixGrid;
        //private DependencyObject _selectedApplication;
        private TreeView _encyclopediaEntryListView;
        private TextBox _searchText;
        private FlowDocumentScrollViewer _encyclopediaViewer;
        //private readonly IClientContext AppContext;//_app;
        public CTRL_2_Dialog()
        {
            InitializeComponent();
            LoadEncyclopediaEntries();
            OnApplyTemplate();
            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();

            _ = InputBindings.Add(
                new KeyBinding(
                    GenericCommands.CancelCommand,
                    Key.Escape,
                    ModifierKeys.None));

            _ = InputBindings.Add(
                new KeyBinding(
                    GenericCommands.AcceptCommand,
                    Key.Enter,
                    ModifierKeys.None));

            _ = CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.CancelCommand,
                    OnGenericCommandsCancelCommandExecuted));

            _ = CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.AcceptCommand,
                    OnGenericCommandsAcceptCommandExecuted));

            GameLog.Client.UIDetails.DebugFormat("F07-Dialog initialized");

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _encyclopediaEntryListView = new TreeView();
            //if (_encyclopediaEntryListView != null)
            //{
            //    _encyclopediaEntryListView.SelectedItemChanged -=
            //        EncyclopediaEntryListView_SelectedItemChanged;
            //}
            if (_searchText != null)
            {
                _searchText.TextChanged -= OnSearchTextChanged;
            }

            //_researchFieldItemsControl = GetTemplateChild("PART_ResearchFieldItemsHost") as Border;
            //_researchMatrixHost = GetTemplateChild("PART_ResearchMatrixHost") as Border;
            //_applicationDetailsHost = GetTemplateChild("PART_ApplicationDetailsHost") as Border;
            //_encyclopediaEntryListView = GetTemplateChild("PART_EncyclopediaEntries") as TreeView;
            _searchText = GetTemplateChild("PART_SearchText") as TextBox;
            //_encyclopediaViewer = GetTemplateChild("PART_EncyclopediaViewer") as FlowDocumentScrollViewer;
            _encyclopediaViewer = new FlowDocumentScrollViewer();

            if (_encyclopediaEntryListView != null)
            {
                _encyclopediaEntryListView.SelectedItemChanged +=
                    EncyclopediaEntryListView_SelectedItemChanged;
                LoadEncyclopediaEntries();
            }
            if (_encyclopediaViewer != null)
            {
                _encyclopediaViewer.Document = null;
            }
            if (_searchText != null)
            {
                _searchText.TextChanged += OnSearchTextChanged;
            }
        }

        private void LoadEncyclopediaEntries()
        {
            //int playerCivId = 0;
            //var playerCiv = GameContext.Current.CivilizationManagers[playerCivId].Civilization;
            CivilizationManager civManager;// = new Civilization("DUMMY");
            TechTree techTree = new TechTree();

            //techTree.Merge(civManager.TechTree);

            if (GameContext.Current == null)
                return;
            else
                civManager = GameContext.Current.CivilizationManagers[0];


            foreach (Entities.Civilization civ in GameContext.Current.Civilizations)
            {
                //if (DiplomacyHelper.IsMember(civ, playerCiv))
                techTree.Merge(GameContext.Current.TechTrees[civ]);
            }

            IOrderedEnumerable<IGrouping<EncyclopediaCategory, IEncyclopediaEntry>> groups = (
                             from civ in GameContext.Current.Civilizations
                                 //let diplomacyStatus = DiplomacyHelper.GetForeignPowerStatus(playerCiv, civ)
                                 //where (diplomacyStatus != ForeignPowerStatus.NoContact) || (civ.CivID == playerCivId)
                             let raceEntry = civ.Race as IEncyclopediaEntry
                             where raceEntry != null
                             select raceEntry
                         )
                .Concat(

                    from design in techTree
                    where TechTreeHelper.MeetsTechLevels(civManager, design)
                    let designEntry = design as IEncyclopediaEntry
                    where designEntry != null
                    select designEntry
                )
                .OrderBy(o => o.EncyclopediaHeading)
                .GroupBy(o => o.EncyclopediaCategory)
                .OrderBy(o => o.Key);

            //_raceGroup = new GroupItem("Text") ;
            //groups.AddRange(_raceGroup as IEncyclopediaEntry);

            Style groupStyle = new Style(
                typeof(TreeViewItem),
                Application.Current.FindResource(typeof(TreeViewItem)) as Style);
            Style itemStyle = new Style(
                typeof(TreeViewItem),
                Application.Current.FindResource(typeof(TreeViewItem)) as Style);

            groupStyle.Triggers.Add(
                new Trigger { Property = ItemsControl.HasItemsProperty, Value = false });
            ((Trigger)groupStyle.Triggers[0]).Setters.Add(
                new Setter(
                    VisibilityProperty,
                    Visibility.Collapsed));

            itemStyle.Setters.Add(
                new Setter(
                    ForegroundProperty,
                    new DynamicResourceExtension("DefaultTextBrush")));
            itemStyle.Setters.Add(
                new Setter(
                    HeaderedContentControl.HeaderProperty,
                    new Binding("EncyclopediaHeading")));

            groupStyle.Seal();
            itemStyle.Seal();

            if (_encyclopediaEntryListView == null)
            {
                return;
            }

            _encyclopediaEntryListView.Items.Clear();

            foreach (IGrouping<EncyclopediaCategory, IEncyclopediaEntry> item in groups)
            {
                //item.Key
                GameLog.Client.Research.DebugFormat("F07_Tree Item = {0}", item.Key);
            }

            foreach (IGrouping<EncyclopediaCategory, IEncyclopediaEntry> group in groups)
            {
                TreeViewItem groupItem = new TreeViewItem();
                ICollectionView entriesView = CollectionViewSource.GetDefaultView(group);
                entriesView.Filter = FilterEncyclopediaEntry;
                groupItem.Style = groupStyle;
                groupItem.SetResourceReference(
                    ForegroundProperty,
                    "HeaderTextBrush");
                groupItem.Resources.Add(typeof(TreeViewItem), itemStyle);
                groupItem.Header = group.Key;
                groupItem.ItemsSource = entriesView;
                groupItem.IsExpanded = true;
                _ = _encyclopediaEntryListView.Items.Add(groupItem);
                //GameLog.Client.Research.DebugFormat("");
            }
        }

        private void DoLink(object sender, RequestNavigateEventArgs e)
        {
            //Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private bool FilterEncyclopediaEntry(object value)
        {
            string searchText = string.Empty;

            if (!(value is IEncyclopediaEntry entry))
            {
                return false;
            }

            if (_searchText != null)
            {
                searchText = _searchText.Text.Trim();
            }

            if (searchText == string.Empty)
            {
                return true;
            }

            string[] words = searchText.Split(
                new[] { ' ', ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                string lcWord = word.ToLowerInvariant();
                return entry.EncyclopediaHeading.ToLowerInvariant().Contains(lcWord)
                        || entry.EncyclopediaText.ToLowerInvariant().Contains(lcWord);
            }

            return false;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)RefreshEncyclopediaEntries);
        }

        private void RefreshEncyclopediaEntries()
        {
            if (_encyclopediaEntryListView == null)
            {
                return;
            }

            System.Collections.Generic.IEnumerable<ICollectionView> groupViews = (from groupItem in _encyclopediaEntryListView.Items.OfType<TreeViewItem>()
                                                                                  select groupItem.ItemsSource).OfType<ICollectionView>();

            foreach (ICollectionView groupView in groupViews)
            {
                groupView.Refresh();
            }
        }

        private void EncyclopediaEntryListView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((_encyclopediaViewer != null)
                && (_encyclopediaEntryListView.SelectedItem != null)
                && (_encyclopediaEntryListView.SelectedItem is IEncyclopediaEntry entry))
            {
                _encyclopediaViewer.Document = GenerateEncyclopediaDocument(
                    entry);
            }
        }


        private FlowDocument GenerateEncyclopediaDocument(IEncyclopediaEntry entry)
        {
            if (entry == null)
            {
                return new FlowDocument();
            }

            TechObjectDesign design = entry as TechObjectDesign;
            FlowDocument doc = new FlowDocument();
            EncyclopediaImageConverter imageConverter = new EncyclopediaImageConverter();
            ResearchFieldImageConverter fiendImageConverter = new ResearchFieldImageConverter();

            Run headerRun = new Run(entry.EncyclopediaHeading);
            Paragraph headerBlock = new Paragraph(headerRun)
            {
                FontFamily = FindResource(ClientResources.DefaultFontFamilyKey) as FontFamily,
                FontSize = 16d * 96d / 72d,
                Foreground = FindResource(ClientResources.HeaderTextForegroundBrushKey) as Brush
            };

            doc.Blocks.Add(headerBlock);

            doc.FontFamily = FindResource(ClientResources.DefaultFontFamilyKey) as FontFamily;
            doc.FontSize = 12d * 96d / 72d;
            doc.Foreground = FindResource(ClientResources.DefaultTextForegroundBrushKey) as Brush;
            doc.TextAlignment = TextAlignment.Left;

            // EncyclopediaImage
            Border image = new Border();

            System.Collections.Generic.List<Paragraph> paragraphs = TextHelper.TrimParagraphs(entry.EncyclopediaText).Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries).Select(o => new Paragraph(new Run(o))).ToList();

            Paragraph firstParagraph = paragraphs.FirstOrDefault();
            if (firstParagraph == null)
            {
                firstParagraph = new Paragraph();
                doc.Blocks.Add(firstParagraph);
            }

            if (imageConverter.Convert(
                entry.EncyclopediaImage,
                typeof(BitmapImage),
                null,
                null) is BitmapImage imageSource)
            {
                double imageWidth = imageSource.Width;
                double imageHeight = imageSource.Height;

                double imageRatio = imageWidth / imageHeight;
                if (imageRatio >= 1.0)
                {
                    imageWidth = Math.Max(200, Math.Min(imageWidth, 270));
                    imageHeight = imageWidth / imageRatio;
                }
                else
                {
                    imageHeight = Math.Max(200, Math.Min(imageHeight, 270));
                    imageWidth = imageHeight * imageRatio;
                }

                image.Width = imageWidth;
                image.Height = imageHeight;
                image.BorderBrush = Brushes.White;
                image.BorderThickness = new Thickness(2.0);
                image.CornerRadius = new CornerRadius(14.0);
                image.Background = new ImageBrush(imageSource) { Stretch = Stretch.UniformToFill };

                Thickness imageMargin = new Thickness(14, 0, 0, 14);
                Floater imageFloater = new Floater
                {
                    Blocks = { new BlockUIContainer(image) },
                    Margin = imageMargin,
                    Width = image.Width,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Padding = new Thickness(0)
                };

                if (firstParagraph.Inlines.Any())
                {
                    firstParagraph.Inlines.InsertBefore(firstParagraph.Inlines.First(), imageFloater);
                }
                else
                {
                    firstParagraph.Inlines.Add(imageFloater);
                }
            }

            doc.Blocks.AddRange(paragraphs);

            if (design != null)
            {
                ContentControl statsControl = new ContentControl
                {
                    Margin = new Thickness(0, 14, 0, 0),
                    Width = 320,
                    Content = new TechObjectDesignViewModel
                    {
                        Design = design,
                        Civilization = GameContext.Current.CivilizationManagers[0].Civilization
                    },
                    Style = FindResource("TechObjectInfoPanelStyle") as Style
                };



                Paragraph statsBlock = new Paragraph(new InlineUIContainer(statsControl))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0)
                };

                doc.Blocks.Add(statsBlock);

                Table techTable = new Table();
                techTable.RowGroups.Add(new TableRowGroup());
                techTable.RowGroups[0].Rows.Add(new TableRow());
                foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields)
                {
                    TechCategory techCategory = field.TechCategory;
                    TableColumn column = new TableColumn();
                    Border techIcon = new Border();
                    TextBlock techTextShadow = new TextBlock { Effect = new BlurEffect { Radius = 6 } };
                    TextBlock techText = new TextBlock();

                    if (design.TechRequirements[techCategory] < 1)
                    {
                        techIcon.Opacity = 0.25;
                    }

                    ImageBrush imageBrush = new ImageBrush(
                        fiendImageConverter.Convert(field, typeof(BitmapImage), null, null)
                        as ImageSource)
                    { Stretch = Stretch.Uniform };




                    techIcon.Width = 54;
                    techIcon.Height = 45;
                    techIcon.Padding = new Thickness(4);
                    techIcon.BorderBrush = Brushes.White;
                    techIcon.BorderThickness = new Thickness(2.0);
                    techIcon.CornerRadius = new CornerRadius(7.0);
                    techIcon.Background = imageBrush;

                    techTextShadow.Text = design.TechRequirements[techCategory].ToString();
                    techTextShadow.Foreground = Brushes.Black;
                    techTextShadow.SetResourceReference(TextBlock.FontFamilyProperty, ClientResources.DefaultFontFamilyKey);
                    techTextShadow.FontWeight = FontWeights.Bold;
                    techTextShadow.FontSize = 16 * (96d / 72d);
                    techTextShadow.HorizontalAlignment = HorizontalAlignment.Right;
                    techTextShadow.VerticalAlignment = VerticalAlignment.Bottom;

                    techText.Text = design.TechRequirements[techCategory].ToString();
                    techText.Foreground = Brushes.White;
                    techText.SetResourceReference(TextBlock.FontFamilyProperty, ClientResources.DefaultFontFamilyKey);
                    techText.FontWeight = FontWeights.Normal;
                    techText.FontSize = 16 * (96d / 72d);
                    techText.HorizontalAlignment = HorizontalAlignment.Right;
                    techText.VerticalAlignment = VerticalAlignment.Bottom;

                    techIcon.Child = new Grid { Children = { techTextShadow, techText } };
                    techIcon.ToolTip = string.Format(
                        "{0} Level {1}",
                        ResourceManager.GetString(field.Name),
                        design.TechRequirements[techCategory]);

                    techIcon.UseLayoutRounding = true;
                    techIcon.CacheMode = new BitmapCache { SnapsToDevicePixels = true };

                    _ = BindingOperations.SetBinding(
                        techIcon.CacheMode,
                        BitmapCache.RenderAtScaleProperty,
                        new Binding
                        {
                            Source = Application.Current.MainWindow,
                            Path = new PropertyPath(ClientProperties.ScaleFactorProperty),
                            Mode = BindingMode.OneWay
                        });

                    BlockUIContainer techIconContainer = new BlockUIContainer(techIcon);

                    techTable.Columns.Add(column);
                    techTable.RowGroups[0].Rows[0].Cells.Add(new TableCell(techIconContainer));
                }

                techTable.ClearFloaters = WrapDirection.Both;
                techTable.Margin = new Thickness(0, 14, 0, 0);
                techTable.CellSpacing = 7.0;

                doc.Blocks.Add(techTable);
            }

            return doc;
        }


        private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.Reload();
            Close();
        }

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            SaveChangesAndHide();
        }

        private void SaveChangesAndHide()
        {
            ClientSettings.Current.Save();
            Close();
        }

        private void OnGenericCommandsTracesSetAllwithoutDetailsCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.TracesAudio = true;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }

        private void OnGenericCommandsTracesSetSomeCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.TracesAudio = false;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }

        private void OnGenericCommandsTracesSetNoneCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            //ClientSettings.Traces_ClearAllProperty();
            ClientSettings.Current.TracesAudio = false;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }
    }
}