using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.Intelligence;
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
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ResumeAnimations();

                //var local = AssetsScreenPresentationModel.Local;
                bool oneNoDummy = DesignTimeObjects.GetSpiedCivilizationOne().Civilization.CivID != -1; //CivilizationManager.Civilization.CivID != -1; //AssetsScreenPresentationModel.SpiedOneCivName != "Empty"; //IntelHelper.SpiedOneCivManager.Civilization.Name != "Empty";
                bool twoNoDummy = DesignTimeObjects.CivilizationManager.Civilization.CivID != -1; //AssetsScreenPresentationModel.SpiedTwoCivName != "Empty";
                bool threeNoDummy = DesignTimeObjects.CivilizationManager.Civilization.CivID != -1; //AssetsScreenPresentationModel.SpiedThreeCivName != "Empty";
                bool fourNoDummy = AssetsScreenPresentationModel.SpiedFourCivName != "Empty"; //DesignTimeAppContext.Instance.SpiedThreeEmpire.Civilization.Name != "Empty";
                bool fiveNoDummy = AssetsScreenPresentationModel.SpiedFiveCivName != "Empty";
                bool sixNoDummy = AssetsScreenPresentationModel.SpiedSixCivName != "Empty";

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
                    EmpireExpanderFour.Visibility = Visibility.Visible;;
                }
                if (fiveNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedFiveCiv))
                {
                    EmpireExpanderFive.Visibility = Visibility.Visible;
                }
                if (sixNoDummy && AssetsHelper.IsSpiedOn(AssetsScreenPresentationModel.SpiedSixCiv))
                {
                    EmpireExpanderSix.Visibility = Visibility.Visible;
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

        public void OnCreated() {}

        public void OnDestroyed()
        {
            StopAnimations();
        }

        #endregion
    } 
}