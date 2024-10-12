﻿// File:SitRepDialog.xaml.cs  
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Controls;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Forms;

namespace Supremacy.Client.Dialogs
{
    /// <summary>
    /// Interaction logic for SitRepDialog.xaml
    /// </summary>
    public partial class SitRepDialog
    {
        private SitRepCategory _visibleCategories;
        private IEnumerable<SitRepEntry> _sitRepEntries;
        private string _previoussitRepCommentTextBox;
        private readonly IMusicPlayer _musicPlayer;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ISoundPlayer _soundPlayer;
#pragma warning restore IDE0052 // Remove unread private members

        public SitRepDialog([NotNull] IMusicPlayer musicPlayer,
            [NotNull] ISoundPlayer soundPlayer)
        {
            InitializeComponent();

            _musicPlayer = musicPlayer ?? throw new ArgumentNullException("musicPlayer");
            _soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");

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


        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    //TextBox sitRepCommentTextBox;


        //    //sitRepCommentTextBox.LostFocus += SitRepCommentTextBox_OnLostFocus;
        //    //sitRepCommentTextBox.GotFocus += SitRepCommentTextBox_OnGotFocus;
        //    //sitRepCommentTextBox.TextChanged += SitRepCommentTextBox_OnTextChanged;

        //}

        private static void SitRepCommentTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(e.Source is TextBox sitRepCommentTextBox))
            {
                return;
            }

            BindingExpression bindingExpression = sitRepCommentTextBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            if (bindingExpression == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(sitRepCommentTextBox.Text))
            {
                bindingExpression.UpdateSource();
            }
        }


        private void SitRepCommentTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is TextBox sitRepCommentTextBox))
            {
                return;
            }

            _previoussitRepCommentTextBox = sitRepCommentTextBox.Text;
        }
        private void SitRepCommentTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string previousText = _previoussitRepCommentTextBox;
            _previoussitRepCommentTextBox = null;
            if ((!(e.Source is TextBox sitRepCommentTextBox)) || string.Equals(sitRepCommentTextBox.Text, previousText))
            {
                return;
            }

            if (!(DataContext is SitRepEntry entry))
            {
                return;
            }

            entry.SitRepComment = string.IsNullOrEmpty(sitRepCommentTextBox.Text.Trim()) ? null : sitRepCommentTextBox.Text;
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

            if (CrimsonCheck.IsChecked.HasValue && CrimsonCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Crimson);
            }

            if (YellowCheck.IsChecked.HasValue && YellowCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.Yellow);
            }

            if (BlueDarkCheck.IsChecked.HasValue && BlueDarkCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.BlueDark);
            }

            if (RedYellowCheck.IsChecked.HasValue && RedYellowCheck.IsChecked.Value)
            {
                visiblePriorities.Add(SitRepPriority.RedYellow);
            }

            ItemsView.Items.Filter = o => ((SitRepEntry)o).Categories/* & visibleCategories*/ != 0 &&
                                               visiblePriorities.Contains(((SitRepEntry)o).Priority);
            ItemsView.Items.Refresh();

        }

        public IEnumerable<SitRepEntry> SitRepEntries
        {
            get
            {
                _sitRepEntries = _sitRepEntries.OrderBy(x => x.SummaryText);

                //_sitRepEntries = _sitRepEntries.OrderBy(x => x.SummaryText).T/*oL*/ist();
                //foreach (var item in _sitRepEntries)
                //{
                //    item.
                //}
                return _sitRepEntries;
            }
            set
            {
                _sitRepEntries = value;
                //_sitRepEntries.OrderByDescending(x => x.SummaryText).ToList();
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
                .Aggregate<MenuItem, SitRepCategory>(0, (current, menuItem) => current | (SitRepCategory)menuItem.Tag)
                ;

            //SitRepAction visibleCategories2 = FilterMenu.Items
            //    .OfType<MenuItem>()
            //    .Where(menuItem => menuItem.IsChecked)
            //    .Aggregate<MenuItem, SitRepAction>(0, (current, menuItem) => current | (SitRepAction)menuItem.Tag)
            //    ;

            VisibleCategories = visibleCategories;
        }



        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {

            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            Close();
            if (_musicPlayer != null)
            {
            _musicPlayer.SwitchMusic("DefaultMusic");
            }

        }

        private void OnMapButtonClick(object sender, ExecuteRoutedEventArgs e)
        {

            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            Close();
            if (_musicPlayer != null)
            {
                _musicPlayer.SwitchMusic("DefaultMusic");
            }

            System.Windows.Forms.SendKeys.SendWait("{F1}"); // avoid blank background and go to Map
        }

        private void OnSitRepEntrySelected(object sender, RoutedEventArgs e)
        {
            //var entry = this.ItemsView.SelectedItem;
            //entry.
            //var entry = GetTemplateChild("Itemsview");
            //entry.   entry.GetValue(SitRepComment.DONE);//   SitRepDone.Read);
            if (ItemsView.SelectedItem is SitRepEntry selection)
            {
                selection.SitRepComment = selection.SitRepComment == "" ? "X" : "";
                //Console.WriteLine("Step_8888: Changed SitRepComment to x or blank");
            }
            ItemsView.Items.Refresh();
            //UpdateCategoryFilter();
            ////OnLoaded();
            //ApplyFilter();

        }

        private void OnSitRepEntryDoubleClick(object sender, RoutedEventArgs e)
        {
            if (ItemsView.SelectedItem is SitRepEntry selection)
            {
                selection.SitRepComment = selection.SitRepComment == "" ? "X" : "";
                //Console.WriteLine("Step_8887: Changed SitRepComment to x or blank");

                switch (selection.Action)
                {
                    case SitRepAction.ShowGalaxyScreen: // F1
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F1_ScreenMusic");
                        }
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen); // F1
                        break;

                    case SitRepAction.ShowColony: // F2
                        Close();
                        GalaxyScreenCommands.SelectSector.Execute((selection.ActionTarget as Colony).Sector); // F2
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
                        //Refresh the screen on an easy way
                        //SendKeys.SendWait("{F5}");
                        //SendKeys.SendWait("{F2}");
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F2_ScreenMusic");
                        }
                        break;

                    case SitRepAction.ShowScienceScreen: // F3
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F3_ScreenMusic");
                        }
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.ScienceScreen); // F3
                        break;

                    case SitRepAction.ShowDiploScreen: // F4
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F4_ScreenMusic");
                        }
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.DiplomacyScreen);  // F4
                        break;

                    case SitRepAction.ShowIntelScreen: // F5
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F5_ScreenMusic");
                        }
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.IntelScreen); // F5
                        break;

                    case SitRepAction.CenterOnSector:
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F1_ScreenMusic");
                        }
                        NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen); // F1
                        Sector sector = selection.ActionTarget as Sector;
                        GalaxyScreenCommands.SelectSector.Execute(sector);
                        GalaxyScreenCommands.CenterOnSector.Execute(sector);
                        break;

                    case SitRepAction.SelectTaskForce:
                        Close();
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("F1_ScreenMusic");
                        }
                        Fleet fleet = selection.ActionTarget as Fleet;
                        GalaxyScreenCommands.SelectSector.Execute(fleet.Sector);
                        GalaxyScreenCommands.CenterOnSector.Execute(fleet.Sector);
                        GalaxyScreenCommands.SelectTaskForce.Execute(fleet);
                        break;

                    case SitRepAction.None:
                    default:
                        if (_musicPlayer != null)
                        {
                            _musicPlayer.SwitchMusic("DefaultMusic");
                        }
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
