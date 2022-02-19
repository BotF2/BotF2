//File:NewShipQueueSelectionView.xaml.cs
using Supremacy.Economy;
using Supremacy.Tech;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Supremacy.Client.Views
{


    public partial class NewShipQueueSelectionView
    {
        public NewShipQueueSelectionView(ShipyardBuildSlot buildSlot)
        {
            InitializeComponent();

            BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard)
                                        .OrderBy(s => s.BuildDesign.Key)
                                        .ToArray();

            BuildSlotQueueProjectList.ItemsSource = shipList;

            _ = SetBinding(
                SelectedBuildSlotQueueProjectProperty,
                new Binding
                {
                    Source = BuildSlotQueueProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });

            if (BuildSlotQueueProjectList.Items.Count > 0)
            {
                BuildSlotQueueProjectList.SelectedIndex = 0;  // to display SHIP_INFO_TEXT just at screen opening
            }
        }

        #region SelectedBuildProject Property
        public static readonly DependencyProperty SelectedBuildSlotQueueProjectProperty = DependencyProperty.Register(
            "SelectedBuildSlotQueueProject",
            typeof(ShipBuildProject),
            typeof(NewShipQueueSelectionView),
            new PropertyMetadata());

        public ShipBuildProject SelectedBuildSlotQueueProject
        {
            get => (ShipBuildProject)GetValue(SelectedBuildSlotQueueProjectProperty);
            set => SetValue(SelectedBuildSlotQueueProjectProperty, value);
        }
        #endregion

        #region AdditionalQueueContent Property
        public static readonly DependencyProperty AdditionalQueueContentProperty = DependencyProperty.Register(
            "AdditionalQueueContent",
            typeof(object),
            typeof(NewShipQueueSelectionView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public object AdditionalQueueContent
        {
            get => GetValue(AdditionalQueueContentProperty);
            set => SetValue(AdditionalQueueContentProperty, value);
        }
        #endregion

        public ShipBuildProject BuildOneMoreProject
        {
            set => SetValue(SelectedBuildSlotQueueProjectProperty, value);
            //set => 
            //    ExecuteAddShipBuildProjectCommand(SelectedBuildSlotQueueProject.Builder, SelectedBuildSlotQueueProject.BuildDesign);

        }

        public string ShipQueueFunctionPath => "vfs:///Resources/Specific_Empires_UI/" + Context.DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization.Key + "/ColonyScreen/Ship_Functions.png";

        private void CanExecuteAcceptCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedBuildSlotQueueProject != null;
        }

        private void ExecuteAcceptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBuildSlotQueueProject == null)
            {
                return;
            }

            DialogResult = true;
        }

        private void OnBuildProjectListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is DependencyObject source))
            {
                return;
            }

            ListBoxItem contanier = source.FindVisualAncestorByType<ListBoxItem>();
            if (contanier == null)
            {
                return;
            }

            DialogResult = true;
        }
    }
}
