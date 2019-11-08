using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using Supremacy.Utility;
using Supremacy.Client.Context;

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

            if (AssetsScreenPresentationModel.SpiedOneCivName != "Empty")
            {
                EmpireExpanderOne.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderOne.Visibility = Visibility.Collapsed; }

            if (AssetsScreenPresentationModel.SpiedTwoCivName != "Empty")
            {
                EmpireExpanderTwo.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderTwo.Visibility = Visibility.Collapsed; }

            if (AssetsScreenPresentationModel.SpiedThreeCivName != "Empty")
            {
                EmpireExpanderThree.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderThree.Visibility = Visibility.Collapsed; }

            if (AssetsScreenPresentationModel.SpiedFourCivName != "Empty")
            {
                EmpireExpanderFour.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderFour.Visibility = Visibility.Collapsed; }

            if (AssetsScreenPresentationModel.SpiedFiveCivName != "Empty")
            {
                EmpireExpanderFive.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderFive.Visibility = Visibility.Collapsed; }

            if (AssetsScreenPresentationModel.SpiedSixCivName != "Empty")
            {
                EmpireExpanderSix.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderSix.Visibility = Visibility.Collapsed; }
        }


        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                ResumeAnimations();
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