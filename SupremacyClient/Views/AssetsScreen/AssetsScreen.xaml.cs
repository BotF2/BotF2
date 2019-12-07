using Supremacy.Client.Context;
using Supremacy.Intelligence;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;

using System.Windows.Media.Imaging;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for AssetsScreen.xaml
    /// </summary>
    public partial class AssetsScreen : IAssetsScreenView
    {
        private List<int> _loopingCivIDs;
        public AssetsScreen()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
            EmpireExpanderOne.Visibility = Visibility.Collapsed;
            EmpireExpanderTwo.Visibility = Visibility.Collapsed;
            EmpireExpanderThree.Visibility = Visibility.Collapsed;
            EmpireExpanderFour.Visibility = Visibility.Collapsed;
            EmpireExpanderFive.Visibility = Visibility.Collapsed;
            EmpireExpanderSix.Visibility = Visibility.Collapsed;
            SabotageEnergyOne.Visibility = Visibility.Collapsed;
            SabotageEnergyTwo.Visibility = Visibility.Collapsed;
            SabotageEnergyThree.Visibility = Visibility.Collapsed;
            SabotageEnergyFour.Visibility = Visibility.Collapsed;
            SabotageEnergyFive.Visibility = Visibility.Collapsed;
            SabotageEnergySix.Visibility = Visibility.Collapsed;
            SabotageFoodOne.Visibility = Visibility.Collapsed;
            SabotageFoodTwo.Visibility = Visibility.Collapsed;
            SabotageFoodThree.Visibility = Visibility.Collapsed;
            SabotageFoodFour.Visibility = Visibility.Collapsed;
            SabotageFoodFive.Visibility = Visibility.Collapsed;
            SabotageFoodSix.Visibility = Visibility.Collapsed;
            SabotageIndustryOne.Visibility = Visibility.Collapsed;
            SabotageIndustryTwo.Visibility = Visibility.Collapsed;
            SabotageIndustryThree.Visibility = Visibility.Collapsed;
            SabotageIndustryFour.Visibility = Visibility.Collapsed;
            SabotageIndustryFive.Visibility = Visibility.Collapsed;
            SabotageIndustrySix.Visibility = Visibility.Collapsed;
            StealResearchOne.Visibility = Visibility.Collapsed;
            StealResearchTwo.Visibility = Visibility.Collapsed;
            StealResearchThree.Visibility = Visibility.Collapsed;
            StealResearchFour.Visibility = Visibility.Collapsed;
            StealResearchFive.Visibility = Visibility.Collapsed;
            StealResearchSix.Visibility = Visibility.Collapsed;
            BlameNoOne.Visibility = Visibility.Visible;
            BlameNoOne.IsChecked = true;
            BlameTerrorists.Visibility = Visibility.Collapsed;
            BlameFederation.Visibility = Visibility.Collapsed;
            BlameTerranEmpire.Visibility = Visibility.Collapsed;
            BlameRomulans.Visibility = Visibility.Collapsed;
            BlameKlingons.Visibility = Visibility.Collapsed;
            BlameCardassians.Visibility = Visibility.Collapsed;
            BlameDominion.Visibility = Visibility.Collapsed;
            BlameBorg.Visibility = Visibility.Collapsed;
            LoadInsignia();
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ResumeAnimations();
                bool oneNoDummy = false;
                bool twoNoDummy = false;
                bool threeNoDummy = false;
                bool fourNoDummy = false;
                bool fiveNoDummy = false;
                bool sixNoDummy = false;

                var oneCivManager = DesignTimeObjects.GetSpiedCivilizationOne();
                if (oneCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != oneCivManager)
                {
                    oneNoDummy = true;
                }
                var twoCivManager = DesignTimeObjects.GetSpiedCivilizationTwo();
                if (twoCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != twoCivManager)
                {
                    twoNoDummy = true;
                }
                var threeCivManager = DesignTimeObjects.GetSpiedCivilizationThree();
                if (threeCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != threeCivManager)
                {
                    threeNoDummy = true;
                }
                var fourCivManager = DesignTimeObjects.GetSpiedCivilizationFour();
                if (fourCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != fourCivManager)
                {
                    fourNoDummy = true;
                }
                var fiveCivManager = DesignTimeObjects.GetSpiedCivilizationFive();
                if (fiveCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != fourCivManager)
                {
                    fiveNoDummy = true;
                }
                var sixCivManager = DesignTimeObjects.GetSpiedCivilizationSix();
                if (sixCivManager.CivilizationID != -1 && DesignTimeObjects.CivilizationManager != sixCivManager)
                {
                    sixNoDummy = true;
                }

                if (oneNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedOneCiv))
                {
                    EmpireExpanderOne.Visibility = Visibility.Visible;
                }
                if (twoNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedTwoCiv))
                {
                    EmpireExpanderTwo.Visibility = Visibility.Visible;
                }
                if (threeNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedThreeCiv))
                {
                    EmpireExpanderThree.Visibility = Visibility.Visible;
                }
                if (fourNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedFourCiv))
                {
                    EmpireExpanderFour.Visibility = Visibility.Visible;
                }
                if (fiveNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedFiveCiv))
                {
                    EmpireExpanderFive.Visibility = Visibility.Visible;
                }
                if (sixNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedSixCiv))
                {
                    EmpireExpanderSix.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergyOne.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergyTwo.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergyThree.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergyFour.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergyFive.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageEnergySix.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodOne.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodTwo.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodThree.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodFour.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodFive.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageFoodSix.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustryOne.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustryTwo.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustryThree.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustryFour.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustryFive.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    SabotageIndustrySix.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchOne.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchTwo.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchThree.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchFour.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchFive.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    StealResearchSix.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameTerrorists.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameFederation.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameTerranEmpire.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameRomulans.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameKlingons.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameCardassians.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameDominion.Visibility = Visibility.Visible;
                }
                if (true)
                {
                    BlameBorg.Visibility = Visibility.Visible;
                }
            }
            else
                PauseAnimations();
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }

        protected void PauseAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.PauseAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        protected void ResumeAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.ResumeAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        protected void StopAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.StopAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        #region Implementation of IActiveAware

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value == _isActive)
                    return;

                _isActive = value;

                IsActiveChanged.Raise(this);
            }
        }

        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IGameScreenView<AssetsScreenPresentationModel>

        public IAppContext AppContext { get; set; }

        public AssetsScreenPresentationModel Model
        {
            get { return DataContext as AssetsScreenPresentationModel; }
            set { DataContext = value; }
        }

        public void OnCreated() { }

        public void OnDestroyed()
        {
            StopAnimations();
        }
        #endregion
        #region OnButtonClicks
        private void OnEnergyOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnEnergyTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnEnergyThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnEnergyFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnEnergyFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnEnergySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        private void OnFoodOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnFoodTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnFoodThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnFoodFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnFoodFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnFoodSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        private void OnResearchOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        #endregion
        private void LoadInsignia()
        {
            BitmapImage insigniaFed = new BitmapImage();
            var uriFed = new Uri("vfs:///Resources/Images/Insignias/FEDERATION.png");
            insigniaFed.BeginInit();
            insigniaFed.UriSource = uriFed;
            insigniaFed.EndInit();

            BitmapImage insigniaTerran = new BitmapImage();
            var uriTerran = new Uri("vfs:///Resources/Images/Insignias/TERRANEMPIRE.png");
            insigniaTerran.BeginInit();
            insigniaTerran.UriSource = uriTerran;
            insigniaTerran.EndInit();

            BitmapImage insigniaRom = new BitmapImage();
            var uriRom = new Uri("vfs:///Resources/Images/Insignias/ROMULANS.png");
            insigniaRom.BeginInit();
            insigniaRom.UriSource = uriRom;
            insigniaRom.EndInit();

            BitmapImage insigniaKling = new BitmapImage();
            var uriKling = new Uri("vfs:///Resources/Images/Insignias/KLINGONS.png");
            insigniaKling.BeginInit();
            insigniaKling.UriSource = uriKling;
            insigniaKling.EndInit();

            BitmapImage insigniaCard = new BitmapImage();
            var uriCard = new Uri("vfs:///Resources/Images/Insignias/CARDASSIANS.png");
            insigniaCard.BeginInit();
            insigniaCard.UriSource = uriCard;
            insigniaCard.EndInit();

            BitmapImage insigniaDom = new BitmapImage();
            var uriDom = new Uri("vfs:///Resources/Images/Insignias/DOMINION.png");
            insigniaDom.BeginInit();
            insigniaDom.UriSource = uriDom;
            insigniaDom.EndInit();

            BitmapImage insigniaBorg = new BitmapImage();
            var uriBorg = new Uri("vfs:///Resources/Images/Insignias/BORG.png");
            insigniaBorg.BeginInit();
            insigniaBorg.UriSource = uriBorg;
            insigniaBorg.EndInit();

            List<int> CivIDs = new List<int>();
            if (AssetsScreenPresentationModel.SpiedOneCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedOneCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedTwoCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedTwoCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedOneCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedThreeCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedOneCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFourCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedFiveCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFiveCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedSixCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedSixCiv.CivID);

        
            if (CivIDs.Count >= 1)
            {
                switch (CivIDs[0])
                {
                    case 0:
                        InsigniaOne.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaOne.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaOne.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaOne.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaOne.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaOne.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaOne.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 2)
            {
                switch (CivIDs[1])
                {
                    case 0:
                        InsigniaTwo.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaTwo.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaTwo.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaTwo.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaTwo.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaTwo.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaTwo.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 3)
            {
                switch (CivIDs[2])
                {
                    case 0:
                        InsigniaThree.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaThree.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaThree.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaThree.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaThree.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaThree.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaThree.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 4)
            {
                switch (CivIDs[3])
                {
                    case 0:
                        InsigniaFour.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaFour.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaFour.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaFour.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaFour.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaFour.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaFour.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 5)
            {
                switch (CivIDs[4])
                {
                    case 0:
                        InsigniaFive.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaFive.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaFive.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaFive.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaFive.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaFive.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaFive.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 6)
            {
                switch (CivIDs[5])
                {
                    case 0:
                        InsigniaSix.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaSix.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaSix.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaSix.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaSix.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaSix.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaSix.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
        }      
    }
}