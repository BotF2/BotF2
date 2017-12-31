using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;

using Supremacy.Economy;
using Supremacy.Tech;
using Wintellect.PowerCollections;
using System.Collections.Generic;

namespace Supremacy.Client.Views
{
    public partial class NewShipSelectionView
    {
        public NewShipSelectionView(ShipyardBuildSlot buildSlot)
        {
            InitializeComponent();

            IList<BuildProject> shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard);

            BuildProject[] shipListArray = Algorithms.Sort(shipList.AsEnumerable<BuildProject>(),
                new Comparison<BuildProject>(
                    delegate(BuildProject a, BuildProject b) { return a.BuildDesign.BuildCost.CompareTo(b.BuildDesign.BuildCost) * -1 /* to reverse the order */; }));

            BuildProjectList.ItemsSource = shipListArray;

            SetBinding(
                SelectedBuildProjectProperty,
                new Binding
                {
                    Source = BuildProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });
        }

        #region SelectedBuildProject Property
        public static readonly DependencyProperty SelectedBuildProjectProperty = DependencyProperty.Register(
            "SelectedBuildProject",
            typeof(ShipBuildProject),
            typeof(NewShipSelectionView),
            new PropertyMetadata());

        public ShipBuildProject SelectedBuildProject
        {
            get { return (ShipBuildProject)GetValue(SelectedBuildProjectProperty); }
            set { SetValue(SelectedBuildProjectProperty, value); }
        }
        #endregion

        #region AdditionalContent Property
        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register(
            "AdditionalContent",
            typeof(object),
            typeof(NewShipSelectionView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public object AdditionalContent
        {
            get { return GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }
        #endregion

        private void CanExecuteAcceptCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (SelectedBuildProject != null);
        }

        private void ExecuteAcceptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBuildProject == null)
                return;
            DialogResult = true;
        }

        private void OnBuildProjectListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            if (source == null)
                return;

            var contanier = source.FindVisualAncestorByType<ListBoxItem>();
            if (contanier == null)
                return;

            DialogResult = true;
        }
    }
}
