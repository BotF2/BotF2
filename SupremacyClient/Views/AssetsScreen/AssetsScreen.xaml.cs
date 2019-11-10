using Supremacy.Client.Context;
using Supremacy.Diplomacy;
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
       //private int[] _restSpiedColonyVisible = new int[] { 0,1,2,3,4,5,6 };
        public AssetsScreen()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;

            if ((AssetsScreenPresentationModel.SpiedOneCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedOneCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderOne.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderOne.Visibility = Visibility.Collapsed; }

            if ((AssetsScreenPresentationModel.SpiedTwoCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedTwoCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderTwo.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderTwo.Visibility = Visibility.Collapsed; }

            if ((AssetsScreenPresentationModel.SpiedThreeCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedThreeCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderThree.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderThree.Visibility = Visibility.Collapsed; }

            if ((AssetsScreenPresentationModel.SpiedFourCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedFourCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderFour.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderFour.Visibility = Visibility.Collapsed; }

            if ((AssetsScreenPresentationModel.SpiedFiveCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedFiveCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderFive.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderFive.Visibility = Visibility.Collapsed; }

            if ((AssetsScreenPresentationModel.SpiedSixCivName != "Empty") && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedSixCiv, AssetsScreenPresentationModel.Local))
            {
                EmpireExpanderSix.Visibility = Visibility.Visible;
            }
            else { EmpireExpanderSix.Visibility = Visibility.Collapsed; }

        }


        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ResumeAnimations();

                var local = AssetsScreenPresentationModel.Local;
                bool oneNoDummy = AssetsScreenPresentationModel.SpiedOneCivName != "Empty";
                bool twoNoDummy = AssetsScreenPresentationModel.SpiedTwoCivName != "Empty";
                bool threeNoDummy = AssetsScreenPresentationModel.SpiedThreeCivName != "Empty";
                bool fourNoDummy = AssetsScreenPresentationModel.SpiedFourCivName != "Empty";
                bool fiveNoDummy = AssetsScreenPresentationModel.SpiedFiveCivName != "Empty";
                bool sixNoDummy = AssetsScreenPresentationModel.SpiedSixCivName != "Empty";

                if (oneNoDummy && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedOneCiv, local)) //_restSpiedColonyVisible[0] != 8 && 
                {
                    EmpireExpanderOne.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[0] = 8;
                }
                if (twoNoDummy && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedTwoCiv, local)) //_restSpiedColonyVisible[1] != 9 &&
                {
                    EmpireExpanderTwo.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[1] = 9;
                }
                if (threeNoDummy && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedThreeCiv, local)) //_restSpiedColonyVisible[2] != 10 &&
                {
                    EmpireExpanderThree.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[2] = 10;
                }
                if (fourNoDummy &&DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedFourCiv, local)) //_restSpiedColonyVisible[3] != 11 &&
                {
                    EmpireExpanderFour.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[3] = 11;
                }
                if (fiveNoDummy && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedFiveCiv, local)) //_restSpiedColonyVisible[4] != 12 &&
                {
                    EmpireExpanderFive.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[4] = 12;
                }
                if (sixNoDummy && DiplomacyHelper.IsContactMade(AssetsScreenPresentationModel.SpiedSixCiv, local)) //_restSpiedColonyVisible[5] != 13 &&
                {
                    EmpireExpanderSix.Visibility = Visibility.Visible;
                    //_restSpiedColonyVisible[5] = 13;
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
    } 
}