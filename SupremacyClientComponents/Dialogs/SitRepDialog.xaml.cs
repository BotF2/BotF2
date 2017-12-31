using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Supremacy.Client.Controls;
using Supremacy.Game;
using Supremacy.Utility;

using System.Linq;
using Supremacy.Universe;
using Supremacy.Orbitals;
using Supremacy.Client.Commands;

namespace Supremacy.Client.Dialogs
{
    /// <summary>
    /// Interaction logic for SitRepDialog.xaml
    /// </summary>
    public partial class SitRepDialog
    {
        private SitRepCategory _visibleCategories;
        private IEnumerable<SitRepEntry> _sitRepEntries;

        public SitRepDialog()
        {
            InitializeComponent();

            var visibleCategories = SitRepDialogSettings.GetVisibleCategories(ClientSettings.Current);

            var enumStringConverter = new EnumStringConverter();

            foreach (var category in EnumHelper.GetValues<SitRepCategory>())
            {
                var menuItem = new MenuItem
                               {
                                   StaysOpenOnClick = true,
                                   IsCheckable = true,
                                   IsChecked = (visibleCategories & category) == category,
                                   Tag = category
                               };

                menuItem.SetBinding(
                    HeaderedItemsControl.HeaderProperty,
                    new Binding
                    {
                        Source = "SitRepCategory." + category,
                        Converter = enumStringConverter,
                        BindsDirectlyToSource = true
                    });

                menuItem.Checked += OnFilterItemIsCheckedChanged;
                menuItem.Unchecked += OnFilterItemIsCheckedChanged;

                FilterMenu.Items.Add(menuItem);
            }

            _visibleCategories = visibleCategories;

            Loaded += OnLoaded;
        }

        public void ShowIfAnyVisibleEntries()
        {
            if (IsOpen || (ItemsView.Items.Count == 0))
                return;
            Show();
        }

        private void OnLoaded(object @object, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            UpdateCategoryFilter();
        }

        public SitRepCategory VisibleCategories
        {
            get { return _visibleCategories; }
            set
            {
                _visibleCategories = value;
                SitRepDialogSettings.SetVisibleCategories(ClientSettings.Current, value);
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (SitRepEntries == null)
                return;

            var visibleCategories = FilterMenu.Items
                .OfType<MenuItem>()
                .Where(menuItem => menuItem.IsChecked)
                .Aggregate<MenuItem, SitRepCategory>(0, (current, menuItem) => current | (SitRepCategory)menuItem.Tag);

            var visiblePriorities = new List<SitRepPriority>();

            if (GreenCheck.IsChecked.HasValue && GreenCheck.IsChecked.Value)
                visiblePriorities.Add(SitRepPriority.Green);
            if (YellowCheck.IsChecked.HasValue && YellowCheck.IsChecked.Value)
                visiblePriorities.Add(SitRepPriority.Yellow);
            if (RedCheck.IsChecked.HasValue && RedCheck.IsChecked.Value)
                visiblePriorities.Add(SitRepPriority.Red);
            if (BlueCheck.IsChecked.HasValue && BlueCheck.IsChecked.Value)
                visiblePriorities.Add(SitRepPriority.Special);

            ItemsView.Items.Filter = o => (((SitRepEntry)o).Categories & visibleCategories) != 0 &&
                                               visiblePriorities.Contains(((SitRepEntry)o).Priority);
            ItemsView.Items.Refresh();
        }

        public IEnumerable<SitRepEntry> SitRepEntries
        {
            get { return _sitRepEntries; }
            set
            {
                _sitRepEntries = value;
                ItemsView.ItemsSource = value;
                ApplyFilter();
            }
        }

        private void OnFilterItemIsCheckedChanged(object @object, RoutedEventArgs routedEventArgs)
        {
            if (!IsLoaded)
                return;
            UpdateCategoryFilter();
        }

        private void UpdateCategoryFilter()
        {
            var visibleCategories = FilterMenu.Items
                .OfType<MenuItem>()
                .Where(menuItem => menuItem.IsChecked)
                .Aggregate<MenuItem, SitRepCategory>(0, (current, menuItem) => current | (SitRepCategory)menuItem.Tag);

            VisibleCategories = visibleCategories;
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnSitRepEntryDoubleClick(object sender, RoutedEventArgs e)
        {
            SitRepEntry selection = ItemsView.SelectedItem as SitRepEntry;
            if (selection != null)
            {
                if (selection is ResearchCompleteSitRepEntry)
                {
                    Close();
                    NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ScienceScreen);
                }
                else if (selection is NewColonySitRepEntry)
                {
                    Close();
                    GalaxyScreenCommands.SelectSector.Execute((selection as NewColonySitRepEntry).Colony.Sector);
                    NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                }
                else if (selection is BuildQueueEmptySitRepEntry)
                {
                    Close();
                    GalaxyScreenCommands.SelectSector.Execute((selection as BuildQueueEmptySitRepEntry).Colony.Sector);
                    NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                }
                else if (selection is ItemBuiltSitRepEntry)
                {
                    ItemBuiltSitRepEntry entry = (ItemBuiltSitRepEntry)selection;
                    Sector sector = GameContext.Current.Universe.Map[entry.Location];
                    if ((entry.ItemType is ShipDesign) || (entry.ItemType is StationDesign))
                    {
                        Close();
                        GalaxyScreenCommands.SelectSector.Execute(sector);
                        GalaxyScreenCommands.CenterOnSector.Execute(sector);

                        // TODO: Could be nice to also automatically select the new ship/station??
                    }
                    else if ((sector.System != null) 
                        && sector.System.HasColony
                        && (sector.System.Colony.Owner == entry.Owner))
                    {
                        Close();
                        GalaxyScreenCommands.SelectSector.Execute(sector);
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                    }
                }
                else if (selection is StarvationSitRepEntry)
                {
                    StarvationSitRepEntry entry = (StarvationSitRepEntry)selection;
                    if ((entry.System != null)
                        && entry.System.HasColony
                        && (entry.System.Colony.Owner == entry.Owner))
                    {
                        Close();
                        GalaxyScreenCommands.SelectSector.Execute(entry.System.Colony.Sector);
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                    }
                }
                else if (selection is FirstContactSitRepEntry)
                {
                    FirstContactSitRepEntry sitRep = selection as FirstContactSitRepEntry;

                    Close();
                    GalaxyScreenCommands.SelectSector.Execute(sitRep.Sector);
                    GalaxyScreenCommands.CenterOnSector.Execute(sitRep.Sector);
                }
            }
        }

        private void OnFilterButtonClick(object sender, ExecuteRoutedEventArgs e)
        {
            FilterMenu.IsOpen = !FilterMenu.IsOpen;
        }
    }
}
