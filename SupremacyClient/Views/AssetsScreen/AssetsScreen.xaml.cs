using Supremacy.Client.Context;
//using Supremacy.Diplomacy;
//using Supremacy.Economy;
//using Supremacy.Entities;
//using Supremacy.Game;
using Supremacy.Intelligence;
//using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for AssetsScreen.xaml
    /// </summary>
    public partial class AssetsScreen : IAssetsScreenView
    {
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
            StealResearchFour.Visibility = Visibility.Collapsed;


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
                    StealResearchFour.Visibility = Visibility.Visible;
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
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }

    } 
}