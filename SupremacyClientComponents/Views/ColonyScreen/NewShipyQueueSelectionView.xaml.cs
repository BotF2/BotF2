using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Utility;
using Supremacy.Client;
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

            SetBinding(
                SelectedBuildSlotQueueProjectProperty,
                new Binding
                {
                    Source = BuildSlotQueueProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });

            if (BuildSlotQueueProjectList.Items.Count > 0)
                BuildSlotQueueProjectList.SelectedIndex = 0;  // to display SHIP_INFO_TEXT just at screen opening
        }

        #region SelectedBuildProject Property
        public static readonly DependencyProperty SelectedBuildSlotQueueProjectProperty = DependencyProperty.Register(
            "SelectedBuildSlotQueueProject",
            typeof(ShipBuildProject),
            typeof(NewShipQueueSelectionView),
            new PropertyMetadata());

        public ShipBuildProject SelectedBuildSlotQueueProject
        {
            get { return (ShipBuildProject)GetValue(SelectedBuildSlotQueueProjectProperty); }
            set { SetValue(SelectedBuildSlotQueueProjectProperty, value); }
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
            get { return GetValue(AdditionalQueueContentProperty); }
            set { SetValue(AdditionalQueueContentProperty, value); }
        }
        #endregion

        public string ShipQueueFunctionPath
        {
            get
            {
                return "vfs:///Resources/UI/" + Context.DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization.Key + "/ColonyScreen/Ship_Functions.png";
            }
        }

        private void CanExecuteAcceptCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (SelectedBuildSlotQueueProject != null);
        }

        private void ExecuteAcceptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBuildSlotQueueProject == null)
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
