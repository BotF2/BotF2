using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
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
        //Civilization _civLocalPlayer = DesignTimeObjects.CivilizationManager.Civilization;
        Civilization _spiedOneCiv = DesignTimeObjects.SpiedCivOne.Civilization;
        Civilization _spiedTwoCiv = DesignTimeObjects.SpiedCivTwo.Civilization;
        Civilization _spiedThreeCiv = DesignTimeObjects.SpiedCivThree.Civilization;
        Civilization _spiedFourCiv = DesignTimeObjects.SpiedCivFour.Civilization;
        Civilization _spiedFiveCiv = DesignTimeObjects.SpiedCivFive.Civilization;
        Civilization _spiedSixCiv = DesignTimeObjects.SpiedCivSix.Civilization;
        public AssetsScreen()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;

            BlameNoOne1.IsChecked = true;
            BlameNoOne2.IsChecked = true;
            BlameNoOne3.IsChecked = true;
            BlameNoOne4.IsChecked = true;
            BlameNoOne5.IsChecked = true;
            BlameNoOne6.IsChecked = true;

            LoadInsignia();
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _civLocalPlayer = AppContext.LocalPlayerEmpire.Civilization;
            if (IsVisible)
            {
                ResumeAnimations();
                GameLog.Client.UI.DebugFormat("begin of checking visible");

                if (!AssetsHelper.IsSpiedOne(_spiedOneCiv) || _spiedOneCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedOneCiv checking visible .... ");
                    EmpireExpanderOne.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (_spiedOneCiv != _civLocalPlayer)
                    {
                        EmpireExpanderOne.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedOneCiv))
                            SabotageEnergyOne.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedOneCiv))
                            SabotageFoodOne.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedOneCiv))
                            SabotageIndustryOne.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedOneCiv))
                            StealResearchOne.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedOneCiv))
                            StealCreditsOne.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedTwo(_spiedTwoCiv) || _spiedTwoCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedTwoCiv checking visible .... ");
                    EmpireExpanderTwo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (_spiedTwoCiv != _civLocalPlayer)
                    {
                        EmpireExpanderTwo.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedTwoCiv))
                            SabotageEnergyTwo.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedTwoCiv))
                            SabotageFoodTwo.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedTwoCiv))
                            SabotageIndustryTwo.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedTwoCiv))
                            StealResearchTwo.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedTwoCiv))
                            StealCreditsTwo.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedThree(_spiedThreeCiv) || _spiedThreeCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedThreeCiv checking visible .... ");
                    EmpireExpanderThree.Visibility = Visibility.Collapsed;

                }
                else
                {
                    if (_spiedThreeCiv != _civLocalPlayer)
                    {
                        EmpireExpanderThree.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedThreeCiv))
                            SabotageEnergyThree.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedThreeCiv))
                            SabotageFoodThree.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedThreeCiv))
                            SabotageIndustryThree.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedThreeCiv))
                            StealResearchThree.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedThreeCiv))
                            StealCreditsThree.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedFour(_spiedFourCiv) || _spiedFourCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedFourCiv checking visible .... ");
                    EmpireExpanderFour.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (_spiedFourCiv != _civLocalPlayer)
                    {
                        EmpireExpanderFour.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedFourCiv))
                            SabotageEnergyFour.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedFourCiv))
                            SabotageFoodFour.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedFourCiv))
                            SabotageIndustryFour.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedFourCiv))
                            StealResearchFour.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedFourCiv))
                            StealCreditsFour.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedFive(_spiedFiveCiv) || _spiedFiveCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedFiveCiv checking visible .... ");
                    EmpireExpanderFive.Visibility = Visibility.Collapsed;
                }

                else
                {
                    if (_spiedFiveCiv != _civLocalPlayer)
                    {
                        EmpireExpanderFive.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedFiveCiv))
                            SabotageEnergyFive.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedFiveCiv))
                            SabotageFoodFive.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedFiveCiv))
                            SabotageIndustryFive.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedFiveCiv))
                            StealResearchFive.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedFiveCiv))
                            StealCreditsFive.Visibility = Visibility.Visible;
                    }
                }
                if (!AssetsHelper.IsSpiedSix(_spiedSixCiv) || _spiedSixCiv == _civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("SpiedSixCiv checking visible .... ");

                    EmpireExpanderSix.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (_spiedSixCiv != _civLocalPlayer)

                    {
                        EmpireExpanderSix.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageEnergy(_spiedSixCiv))
                            SabotageEnergySix.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageFood(_spiedSixCiv))
                            SabotageFoodSix.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeSabotageIndustry(_spiedSixCiv))
                            SabotageIndustrySix.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealResearch(_spiedSixCiv))
                            StealResearchSix.Visibility = Visibility.Visible;
                        if (IntelHelper.SeeStealCredits(_spiedSixCiv))
                            StealCreditsSix.Visibility = Visibility.Visible;
                    }
                }
                GameLog.Client.UI.DebugFormat("end  of checking visible");

                //var allEmpireCivManagers = DesignTimeObjects.SpiedCivMangers; // all CivilizationManagers in game, and in CivID numerical sequence, but Local CivilizationManager substitued for not in game CiviliationManagers

                // IEnumerable<CivilizationManager> distinctCivManagers = allEmpireCivManagers.Distinct();
                List<CivilizationManager> spyableCivManagers = new List<CivilizationManager>();

                var shortList = GameContext.Current.CivilizationManagers; // only CivilizationMangers in game and in CivID numerical sequence
                foreach (var manager in shortList)
                {
                    if (manager != DesignTimeObjects.LocalCivManager) // not the local player
                    {
                        spyableCivManagers.Add(manager);
                    }
                }


                var localPlayer = DesignTimeObjects.LocalCivManager.Civilization;

                Dictionary<int, Civilization> empireCivsDictionary = new Dictionary<int, Civilization>();
                List<Civilization> empireCivsList = new List<Civilization>();
        
                int counting = 0;
                foreach (var civManager in spyableCivManagers)
                {
                    empireCivsDictionary.Add(civManager.CivilizationID, civManager.Civilization); //dictionary of civs that can be spied on with key set to CivID
                    empireCivsList.Add(civManager.Civilization); // list of civs that can be spied on by local player and in CivID sequence
                    GameLog.Client.UI.DebugFormat("Add civ = {0} to blame dictionary at key ={1}", civManager.Civilization.Key, civManager.CivilizationID);
                    GameLog.Client.UI.DebugFormat("Add civ = {0} to blame list at index ={1}", civManager.Civilization.Key, counting);
                    counting++;
                }

                GameLog.Client.UI.DebugFormat("FED: begin of checking BLAME visible");

                if (empireCivsDictionary.Keys.Contains(0) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[0]].IsContactMade())
                {
                    BlameFederation2.Visibility = Visibility.Visible;
                    BlameFederation3.Visibility = Visibility.Visible;
                    BlameFederation4.Visibility = Visibility.Visible;
                    BlameFederation5.Visibility = Visibility.Visible;
                    BlameFederation6.Visibility = Visibility.Visible;
                }
                GameLog.Client.UI.DebugFormat("FED: end   of checking BLAME visible");

                if (empireCivsDictionary.Keys.Contains(1) && 
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[1]].IsContactMade())
                {
                    if (empireCivsDictionary[1] == empireCivsList[0]) // if the Terran Empire (key =1) is in the first index (first expander spy report)
                    {
                        BlameTerranEmpire2.Visibility = Visibility.Visible;
                        BlameTerranEmpire3.Visibility = Visibility.Visible;
                        BlameTerranEmpire4.Visibility = Visibility.Visible;
                        BlameTerranEmpire5.Visibility = Visibility.Visible;
                        BlameTerranEmpire6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameTerranEmpire1.Visibility = Visibility.Visible;
                        BlameTerranEmpire3.Visibility = Visibility.Visible;
                        BlameTerranEmpire4.Visibility = Visibility.Visible;
                        BlameTerranEmpire5.Visibility = Visibility.Visible;
                        BlameTerranEmpire6.Visibility = Visibility.Visible;
                    }
                }
                if (empireCivsDictionary.Keys.Contains(2) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[2]].IsContactMade())
                {
                    if (empireCivsDictionary[2] == empireCivsList[0])
                    {
                        BlameRomulans2.Visibility = Visibility.Visible;
                        BlameRomulans3.Visibility = Visibility.Visible;
                        BlameRomulans4.Visibility = Visibility.Visible;
                        BlameRomulans5.Visibility = Visibility.Visible;
                        BlameRomulans6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[2] == empireCivsList[1])
                    {
                        BlameRomulans1.Visibility = Visibility.Visible;
                        BlameRomulans3.Visibility = Visibility.Visible;
                        BlameRomulans4.Visibility = Visibility.Visible;
                        BlameRomulans5.Visibility = Visibility.Visible;
                        BlameRomulans6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameRomulans1.Visibility = Visibility.Visible;
                        BlameRomulans2.Visibility = Visibility.Visible;
                        BlameRomulans4.Visibility = Visibility.Visible;
                        BlameRomulans5.Visibility = Visibility.Visible;
                        BlameRomulans6.Visibility = Visibility.Visible;
                    }
                }
                if (empireCivsDictionary.Keys.Contains(3) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[3]].IsContactMade())
                {
                    if (empireCivsDictionary[3] == empireCivsList[0])
                    {
                        BlameKlingons2.Visibility = Visibility.Visible;
                        BlameKlingons3.Visibility = Visibility.Visible;
                        BlameKlingons4.Visibility = Visibility.Visible;
                        BlameKlingons5.Visibility = Visibility.Visible;
                        BlameKlingons6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[3] == empireCivsList[1])
                    {
                        BlameKlingons1.Visibility = Visibility.Visible;
                        BlameKlingons3.Visibility = Visibility.Visible;
                        BlameKlingons4.Visibility = Visibility.Visible;
                        BlameKlingons5.Visibility = Visibility.Visible;
                        BlameKlingons6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[3] == empireCivsList[2])
                    {
                        BlameKlingons1.Visibility = Visibility.Visible;
                        BlameKlingons2.Visibility = Visibility.Visible;
                        BlameKlingons4.Visibility = Visibility.Visible;
                        BlameKlingons5.Visibility = Visibility.Visible;
                        BlameKlingons6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameKlingons1.Visibility = Visibility.Visible;
                        BlameKlingons2.Visibility = Visibility.Visible;
                        BlameKlingons3.Visibility = Visibility.Visible;
                        BlameKlingons5.Visibility = Visibility.Visible;
                        BlameKlingons6.Visibility = Visibility.Visible;
                    }
                }
                GameLog.Client.UI.DebugFormat("CARD: begin of checking BLAME visible");
                if (empireCivsDictionary.Keys.Contains(4) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[4]].IsContactMade()) // && sevenCivs[4].Key != "CARDASSIANS")
                {
                    if (empireCivsDictionary[4] == empireCivsList[0])
                    {
                        BlameCardassians2.Visibility = Visibility.Visible;
                        BlameCardassians3.Visibility = Visibility.Visible;
                        BlameCardassians4.Visibility = Visibility.Visible;
                        BlameCardassians5.Visibility = Visibility.Visible;
                        BlameCardassians6.Visibility = Visibility.Visible;
                    }
                    else if (empireCivsDictionary[4] == empireCivsList[1])
                    {
                        BlameCardassians1.Visibility = Visibility.Visible;
                        BlameCardassians3.Visibility = Visibility.Visible;
                        BlameCardassians4.Visibility = Visibility.Visible;
                        BlameCardassians5.Visibility = Visibility.Visible;
                        BlameCardassians6.Visibility = Visibility.Visible;
                    }
                    else if (empireCivsDictionary[4] == empireCivsList[2])
                    {
                        BlameCardassians1.Visibility = Visibility.Visible;
                        BlameCardassians2.Visibility = Visibility.Visible;
                        BlameCardassians4.Visibility = Visibility.Visible;
                        BlameCardassians5.Visibility = Visibility.Visible;
                        BlameCardassians6.Visibility = Visibility.Visible;
                    }
                    else if (empireCivsDictionary[4] == empireCivsList[3])
                    {
                        BlameCardassians1.Visibility = Visibility.Visible;
                        BlameCardassians2.Visibility = Visibility.Visible;
                        BlameCardassians3.Visibility = Visibility.Visible;
                        BlameCardassians5.Visibility = Visibility.Visible;
                        BlameCardassians6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameCardassians1.Visibility = Visibility.Visible;
                        BlameCardassians2.Visibility = Visibility.Visible;
                        BlameCardassians3.Visibility = Visibility.Visible;
                        BlameCardassians4.Visibility = Visibility.Visible;
                        BlameCardassians6.Visibility = Visibility.Visible;
                    }
                }
                GameLog.Client.UI.DebugFormat("CARD: end of checking BLAME visible");
                if (empireCivsDictionary.Keys.Contains(5) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[5]].IsContactMade())
                {
                    if (empireCivsDictionary[5] == empireCivsList[0])
                    {
                        BlameDominion2.Visibility = Visibility.Visible;
                        BlameDominion3.Visibility = Visibility.Visible;
                        BlameDominion4.Visibility = Visibility.Visible;
                        BlameDominion5.Visibility = Visibility.Visible;
                        BlameDominion6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[5] == empireCivsList[1])
                    {
                        BlameDominion1.Visibility = Visibility.Visible;
                        BlameDominion3.Visibility = Visibility.Visible;
                        BlameDominion4.Visibility = Visibility.Visible;
                        BlameDominion5.Visibility = Visibility.Visible;
                        BlameDominion6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[5] == empireCivsList[2])
                    {
                        BlameDominion1.Visibility = Visibility.Visible;
                        BlameDominion2.Visibility = Visibility.Visible;
                        BlameDominion4.Visibility = Visibility.Visible;
                        BlameDominion5.Visibility = Visibility.Visible;
                        BlameDominion6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[5] == empireCivsList[3])
                    {
                        BlameDominion1.Visibility = Visibility.Visible;
                        BlameDominion2.Visibility = Visibility.Visible;
                        BlameDominion3.Visibility = Visibility.Visible;
                        BlameDominion5.Visibility = Visibility.Visible;
                        BlameDominion6.Visibility = Visibility.Visible;
                       // GameLog.Client.UI.DebugFormat("****************** Dictionary key 5 ={0} List item 4 ={1}", empireCivsDictionary[5], empireCivsList[4]);
                    }
                    if (empireCivsDictionary[5] == empireCivsList[4])
                    {
                        BlameDominion1.Visibility = Visibility.Visible;
                        BlameDominion2.Visibility = Visibility.Visible;
                        BlameDominion3.Visibility = Visibility.Visible;
                        BlameDominion4.Visibility = Visibility.Visible;
                        BlameDominion6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameDominion1.Visibility = Visibility.Visible;
                        BlameDominion2.Visibility = Visibility.Visible;
                        BlameDominion3.Visibility = Visibility.Visible;
                        BlameDominion4.Visibility = Visibility.Visible;
                        BlameDominion5.Visibility = Visibility.Visible;
                    }
                }

                if (empireCivsDictionary.Keys.Contains(6) &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[6]].IsContactMade())
                {
                    //if (empireCivsDictionary[6] == empireCivsList[0])
                    //{
                    //    BlameBorg2.Visibility = Visibility.Visible;
                    //    BlameBorg3.Visibility = Visibility.Visible;
                    //    BlameBorg4.Visibility = Visibility.Visible;
                    //    BlameBorg5.Visibility = Visibility.Visible;
                    //    BlameBorg6.Visibility = Visibility.Visible;
                    //}
                    if (empireCivsDictionary[6] == empireCivsList[1])
                    {
                        BlameBorg1.Visibility = Visibility.Visible;
                        BlameBorg3.Visibility = Visibility.Visible;
                        BlameBorg4.Visibility = Visibility.Visible;
                        BlameBorg5.Visibility = Visibility.Visible;
                        BlameBorg6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[6] == empireCivsList[2])
                    {
                        BlameBorg1.Visibility = Visibility.Visible;
                        BlameBorg2.Visibility = Visibility.Visible;
                        BlameBorg4.Visibility = Visibility.Visible;
                        BlameBorg5.Visibility = Visibility.Visible;
                        BlameBorg6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[6] == empireCivsList[3])
                    {
                        BlameBorg1.Visibility = Visibility.Visible;
                        BlameBorg2.Visibility = Visibility.Visible;
                        BlameBorg3.Visibility = Visibility.Visible;
                        BlameBorg5.Visibility = Visibility.Visible;
                        BlameBorg6.Visibility = Visibility.Visible;
                    }
                    if (empireCivsDictionary[6] == empireCivsList[4])
                    {
                        BlameBorg1.Visibility = Visibility.Visible;
                        BlameBorg2.Visibility = Visibility.Visible;
                        BlameBorg3.Visibility = Visibility.Visible;
                        BlameBorg4.Visibility = Visibility.Visible;
                        BlameBorg6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BlameBorg1.Visibility = Visibility.Visible;
                        BlameBorg2.Visibility = Visibility.Visible;
                        BlameBorg3.Visibility = Visibility.Visible;
                        BlameBorg4.Visibility = Visibility.Visible;
                        BlameBorg5.Visibility = Visibility.Visible;
                    }
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
        private void OnCreditsOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnCreditsTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnCreditsThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnCreditsFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnCreditsFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnCreditsSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnResearchOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
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
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        #endregion
        private void LoadInsignia()
        {
            GameLog.Client.UI.DebugFormat("Loading Insignias/FEDERATION.png and more");
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
            GameLog.Client.UI.DebugFormat("Loading Insignias is finished");

            List<int> CivIDs = new List<int>();
            if (AssetsScreenPresentationModel.SpiedOneCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedOneCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedTwoCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedTwoCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedThreeCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedThreeCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedFourCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFourCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedFiveCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFiveCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedSixCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedSixCiv.CivID);
            GameLog.Client.UI.DebugFormat("Adding SpiedCiv is finished");

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
            GameLog.Client.UI.DebugFormat("Insignia is finished");
        }      
    }
}