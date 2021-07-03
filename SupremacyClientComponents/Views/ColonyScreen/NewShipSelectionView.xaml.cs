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


    public partial class NewShipSelectionView
    {
        public NewShipSelectionView(ShipyardBuildSlot buildSlot)
        {
            InitializeComponent();

            BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard)
                                        .OrderBy(s => s.BuildDesign.Key)
                                        .ToArray();

            BuildProjectList.ItemsSource = shipList;

            _ = SetBinding(
                SelectedBuildProjectProperty,
                new Binding
                {
                    Source = BuildProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });

            if (BuildProjectList.Items.Count > 0)
            {
                BuildProjectList.SelectedIndex = 0;  // to display SHIP_INFO_TEXT just at screen opening
            }
        }

        #region SelectedBuildProject Property
        public static readonly DependencyProperty SelectedBuildProjectProperty = DependencyProperty.Register(
            "SelectedBuildProject",
            typeof(ShipBuildProject),
            typeof(NewShipSelectionView),
            new PropertyMetadata());

        public ShipBuildProject SelectedBuildProject // Change in this is seen at ColonyScreenPresenter inside of ExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            get => (ShipBuildProject)GetValue(SelectedBuildProjectProperty);
            set => SetValue(SelectedBuildProjectProperty, value);
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
            get => GetValue(AdditionalContentProperty);
            set => SetValue(AdditionalContentProperty, value);
        }
        #endregion

        public string ShipFunctionPath => "vfs:///Resources/UI/" + Context.DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization.Key + "/ColonyScreen/Ship_Functions.png";

        public int SpecialWidth1 => Context.DesignTimeAppContext.Instance.ASpecialWidth1;// ActualWidthProperty;
        public int SpecialHeight1 => Context.DesignTimeAppContext.Instance.ASpecialHeight1;

        private void CanExecuteAcceptCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedBuildProject != null;
        }

        private void ExecuteAcceptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBuildProject == null)
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
