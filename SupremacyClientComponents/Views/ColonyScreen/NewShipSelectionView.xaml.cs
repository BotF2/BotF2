using Microsoft.Practices.Composite.Presentation.Commands;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Universe;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;

namespace Supremacy.Client.Views
{
    public partial class NewShipSelectionView
    {
        private string _builderKey;
      

        public NewShipSelectionView(ShipyardBuildSlot buildSlot)
        {
            InitializeComponent();

            BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard)
                                        .OrderByDescending(s => s.BuildDesign.BuildCost)
                                        .ToArray();

            BuildProjectList.ItemsSource = shipList;

            _builderKey = shipList[0].Builder.Key; // set up for the switch in ShipInfoByEmpire

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

        #region ShipInfoByEmpire Property
        public string ShipInfoByEmpire
        {
            get
            {
                string imagePath = "vfs:///Resources/UI/Default/Ship_Functions.png";
                switch (_builderKey)
                {
                    case "FEDERATION":
                        imagePath = "vfs:///Resources/UI/Federation/ColonyScreen/Ship_Functions.png";
                        break;
                    case "KLINGONS":
                        imagePath = "vfs:///Resources/UI/Klingons/ColonyScreen/Ship_Functions.png";
                        break;
                    case "ROMULANS":
                        imagePath = "vfs:///Resources/UI/Borg/ColonyScreen/Ship_Functions.png";
                        break;
                    case "DOMINION":
                        imagePath = "vfs:///Resources/UI/Dominion/ColonyScreen/Ship_Functions.png";
                        break;
                    case "TERRANEMPIRE":
                        imagePath = "vfs:///Resources/UI/TerranEmpire/ColonyScreen/Ship_Functions.png";
                        break;
                    case "BORG":
                        imagePath = "vfs:///Resources/UI/Borg/ColonyScreen/Ship_Functions.png";
                        break;
                    case "CARDASSIANS":
                        imagePath = "vfs:///Resources/UI/Cardassians/ColonyScreen/Ship_Functions.png";
                        break;
                    default:
                        imagePath = "vfs:///Resources/UI/Default/Ship_Functions.png";
                        break;
                }
                return imagePath;
            }
            set
            {
                if (SelectedBuildProject == null)
                {
                    var property = DependencyProperty.Register(
                         "SelectedBuildProject",
                         typeof(ShipBuildProject),
                         typeof(NewShipSelectionView),
                         new PropertyMetadata());
                    var project = (ShipBuildProject)GetValue(property);
                    _builderKey = project.Builder.Key;
                    _builderKey = value;
                }
                SelectedBuildProject.Builder.Key = value;
            }
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
