using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Universe;
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
            //SabotageEngeryOne.Visibility = Visibility.Collapsed;
            //SabotageEngeryTwo.Visibility = Visibility.Collapsed;
            //SabotageEngeryThere.Visibility = Visibility.Collapsed;
            SabotageEngeryFour.Visibility = Visibility.Collapsed;
            //SabotageEngeryFive.Visibility = Visibility.Collapsed;
            //SabotageEngerySix.Visibility = Visibility.Collapsed;
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
                    SabotageEngeryFour.Visibility = Visibility.Visible;
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
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        #endregion
        public void SabotageEnergy(Colony colony, Civilization civ)
        {
            var system = colony.System;
            var spyEmpire = IntelHelper.NewSpyCiv;
            if (spyEmpire == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == spyEmpire.CivID);
            if (ownedByPlayer)
                return;

            //private static void CreateSabotage(Civilization civ, StarSystem system)
            //{
            //var sabotagedCiv = GameContext.Current.CivilizationManagers[colony.Owner].Colonies;
            //var civManager = GameContext.Current.CivilizationManagers[civ.Key];

            int defenseIntelligence = GameContext.Current.CivilizationManagers[colony.System.Owner].TotalIntelligence + 1;  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //int attackingIntelligence = GameContext.Current.CivilizationManagers[civ].TotalIntelligence + 1;  // TotalIntelligence of attacked civ
            //if (attackingIntelligence - 1 < 0.1)
            // var   attackingIntelligence = 100 * ;

            //int ratio = attackingIntelligence / defenseIntelligence;
            ////max ratio for no exceeding gaining points
            //if (ratio > 10)
            int ratio = 2;

            //GameLog.Core.Intel.DebugFormat("owner= {0}, system= {1} is SABOTAGED by civ= {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
            //    system.Owner, system.Name, civ.Name, defenseIntelligence, attackingIntelligence, ratio);


            GameLog.Core.Intel.DebugFormat("Owner= {0}, system= {1} at {2} (sabotaged): Energy=? out of facilities={3}, in total={4}",
                system.Owner, system.Name, system.Location,
                //colony.GetEnergyUsage(),
                system.Colony.GetActiveFacilities(ProductionCategory.Energy),
                system.Colony.GetTotalFacilities(ProductionCategory.Energy));
            GameLog.Core.Intel.DebugFormat("Sabotage Energy to {0}: TotalEnergyFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Energy));

            //Effect of sabatoge
            int removeEnergyFacilities = 0;
            if (colony.GetTotalFacilities(ProductionCategory.Energy) > 1 && ratio > 1)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            //if ratio > 2 than remove one more  EnergyFacility
            //if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 2 && ratio > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            //{
            //    removeEnergyFacilities = 3;  //  2 and one from before
            //    system.Colony.RemoveFacilities(ProductionCategory.Energy, 2);
            //}

            // if ratio > 3 than remove one more  EnergyFacility
            //if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 3 && ratio > 3)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            //{
            //    removeEnergyFacilities = 6;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
            //    system.Colony.RemoveFacilities(ProductionCategory.Energy, 3);
            //}

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Energy));
            // civManager.SitRepEntries.Add(new NewSabotageSitRepEntry(civ, system.Colony, removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy)));

        }
    } 
}