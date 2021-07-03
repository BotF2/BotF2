// File:SitRepDialog.xaml.cs  
using Supremacy.Client.Commands;
using Supremacy.Client.Controls;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace Supremacy.Client.Dialogs
{
    /// <summary>
    /// Interaction logic for SitRepDialog.xaml
    /// </summary>
    public partial class SitRepDialog
    {
        private SitRepCategory _visibleCategories;
        private IEnumerable<SitRepEntry> _sitRepEntries;
        private string _previousSitRepCommentText;
        //private readonly static Dictionary<int, string> _SitRepComments = new Dictionary<int, string>(); // { { "98", false } };


        public SitRepDialog()
        {
            InitializeComponent();

            SitRepCategory visibleCategories = SitRepDialogSettings.GetVisibleCategories(ClientSettings.Current);

            EnumStringConverter enumStringConverter = new EnumStringConverter();

            foreach (SitRepCategory category in EnumHelper.GetValues<SitRepCategory>())
            {
                MenuItem menuItem = new MenuItem
                {
                    StaysOpenOnClick = true,
                    IsCheckable = true,
                    IsChecked = (visibleCategories & category) == category,
                    Tag = category
                };

                _ = menuItem.SetBinding(
                    HeaderedItemsControl.HeaderProperty,
                    new Binding
                    {
                        Source = "SitRepCategory." + category,
                        Converter = enumStringConverter,
                        BindsDirectlyToSource = true
                    });

                menuItem.Checked += OnFilterItemIsCheckedChanged;
                menuItem.Unchecked += OnFilterItemIsCheckedChanged;

                // deactivated - normally not used by Players   
                _ = FilterMenu.Items.Add(menuItem);
            }

            _visibleCategories = visibleCategories;

            Loaded += OnLoaded;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!(GetTemplateChild("SitRepComment") is TextBox sitRepCommentText))
            {
                return;
            }

            sitRepCommentText.LostFocus += SitRepCommentText_OnLostFocus;
            sitRepCommentText.GotFocus += SitRepCommentText_OnGotFocus;
            sitRepCommentText.TextChanged += SitRepCommentText_OnTextChanged;

        }

        private static void SitRepCommentText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(e.Source is TextBox sitRepCommentText))
            {
                return;
            }

            BindingExpression bindingExpression = sitRepCommentText.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            if (bindingExpression == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(sitRepCommentText.Text))
            {
                bindingExpression.UpdateSource();
            }
        }


        private void SitRepCommentText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is TextBox sitRepCommentText))
            {
                return;
            }

            _previousSitRepCommentText = sitRepCommentText.Text;
        }
        private void SitRepCommentText_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string previousText = _previousSitRepCommentText;
            _previousSitRepCommentText = null;
            if ((!(e.Source is TextBox sitRepCommentText)) || string.Equals(sitRepCommentText.Text, previousText))
            {
                return;
            }

            if (!(DataContext is SitRepEntry entry))
            {
                return;
            }

            entry.SitRepComment = string.IsNullOrEmpty(sitRepCommentText.Text.Trim()) ? null : sitRepCommentText.Text;
            //ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(new SetObjectNameOrder(entry, entry.ClassName));
        }

        public void ShowIfAnyVisibleEntries()
        {
            if (IsOpen || (ItemsView.Items.Count == 0))
            {
                return;
            }

            Show();
        }

        private void OnLoaded(object @object, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            // deactivated - normally not used by Players   
            UpdateCategoryFilter();
        }

        public SitRepCategory VisibleCategories
        {
            get => _visibleCategories;
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
            {
                return;
            }

            // deactivated - normally not used by Players   
            SitRepCategory visibleCategories = FilterMenu.Items
                .OfType<MenuItem>()
                .Where(menuItem => menuItem.IsChecked)
                .Aggregate<MenuItem, SitRepCategory>(0, (current, menuItem) => current | (SitRepCategory)menuItem.Tag);

            List<SitRepPriority> visiblePriorities = new List<SitRepPriority>();

            if (GreenCheck.IsChecked.HasValue && GreenCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Green);
            }

            if (OrangeCheck.IsChecked.HasValue && OrangeCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Orange);
            }

            if (RedCheck.IsChecked.HasValue && RedCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Red);
            }

            if (BlueCheck.IsChecked.HasValue && BlueCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Blue);
            }

            if (GrayCheck.IsChecked.HasValue && GrayCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Gray);
            }

            if (PurpleCheck.IsChecked.HasValue && PurpleCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Purple);
            }

            if (PinkCheck.IsChecked.HasValue && PinkCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Pink);
            }

            if (BrownCheck.IsChecked.HasValue && BrownCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Brown);
            }

            if (AquaCheck.IsChecked.HasValue && AquaCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Aqua);
            }

            ItemsView.Items.Filter = o => ((SitRepEntry)o).Categories/* & visibleCategories*/ != 0 &&
                                               visiblePriorities.Contains(((SitRepEntry)o).Priority);
            ItemsView.Items.Refresh();
        }

        public IEnumerable<SitRepEntry> SitRepEntries
        {
            get
            {
                //_sitRepEntries.OrderBy(_sitRepEntries, SitRepPriority);

                _sitRepEntries = _sitRepEntries.OrderByDescending(x => x.Priority).ToList();
                //foreach (var item in _sitRepEntries)
                //{
                //    item.
                //}
                return _sitRepEntries;
            }
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
            {
                return;
            }
            // deactivated - normally not used by Players   
            UpdateCategoryFilter();
        }

        // deactivated - normally not used by Players   
        private void UpdateCategoryFilter()
        {
            // deactivated - normally not used by Players   
            SitRepCategory visibleCategories = FilterMenu.Items
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
            if (ItemsView.SelectedItem is SitRepEntry selection)
            {
                switch (selection.Action)
                {
                    case SitRepAction.ShowScienceScreen:
                        Close();
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ScienceScreen); // F4
                        break;

                    case SitRepAction.ViewColony:
                        Close();
                        GalaxyScreenCommands.SelectSector.Execute((selection.ActionTarget as Colony).Sector); // F2
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                        //Refresh the screen on an easy way
                        //SendKeys.SendWait("{F5}");
                        //SendKeys.SendWait("{F2}");
                        break;

                    case SitRepAction.ShowDiploScreen:
                        Close();
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.DiplomacyScreen);  // F3
                        break;

                    case SitRepAction.ShowIntelScreen:
                        Close();
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.IntelScreen); // F5
                        break;

                    case SitRepAction.CenterOnSector:
                        Close();
                        Sector sector = selection.ActionTarget as Sector;
                        GalaxyScreenCommands.SelectSector.Execute(sector);
                        GalaxyScreenCommands.CenterOnSector.Execute(sector);
                        break;

                    case SitRepAction.SelectTaskForce:
                        Close();
                        Fleet fleet = selection.ActionTarget as Fleet;
                        GalaxyScreenCommands.SelectSector.Execute(fleet.Sector);
                        GalaxyScreenCommands.CenterOnSector.Execute(fleet.Sector);
                        GalaxyScreenCommands.SelectTaskForce.Execute(fleet);
                        break;

                }
            }
        }

        // deactivated - normally not used by Players   
        private void OnFilterButtonClick(object sender, ExecuteRoutedEventArgs e)
        {
            FilterMenu.IsOpen = !FilterMenu.IsOpen;
        }
    }
}
