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


    public partial class NewShipSelectionView
    {
        public NewShipSelectionView(ShipyardBuildSlot buildSlot)
        {
            InitializeComponent();

            BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard)
                                        .OrderBy(s => s.BuildDesign.Key)
                                        .ToArray();

            BuildProjectList.ItemsSource = shipList;

            SetBinding(
                SelectedBuildProjectProperty,
                new Binding
                {
                    Source = BuildProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });

            if (BuildProjectList.Items.Count > 0)  
                BuildProjectList.SelectedIndex = 0;  // to display SHIP_INFO_TEXT just at screen opening
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

        public string ShipFunctionPath 
        {
            get
            {
                return "vfs:///Resources/UI/" + Context.DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization.Key + "/ColonyScreen/Ship_Functions.png";
            }
        }

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
