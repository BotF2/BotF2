using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for AssetsScreen.xaml
    /// </summary>
    public partial class AssetsScreen : IAssetsScreenView
    {
        private string _blameWhoZero = "No one";
        private string _blameWhoOne = "No one";
        private string _blameWhoTwo = "No one";
        private string _blameWhoThree = "No one";
        private string _blameWhoFour = "No one";
        private string _blameWhoFive = "No one";
        private string _blameWhoSix = "No one";

        private RadioButton[] _radioButtonZero;
        private RadioButton[] _radioButtonOne;
        private RadioButton[] _radioButtonTwo;
        private RadioButton[] _radioButtonThree;
        private RadioButton[] _radioButtonFour;
        private RadioButton[] _radioButtonFive;
        private RadioButton[] _radioButtonSix;

        Civilization _spiedZeroCiv = DesignTimeObjects.SpiedCivZero.Civilization;
        Civilization _spiedOneCiv = DesignTimeObjects.SpiedCivOne.Civilization;
        Civilization _spiedTwoCiv = DesignTimeObjects.SpiedCivTwo.Civilization;
        Civilization _spiedThreeCiv = DesignTimeObjects.SpiedCivThree.Civilization;
        Civilization _spiedFourCiv = DesignTimeObjects.SpiedCivFour.Civilization;
        Civilization _spiedFiveCiv = DesignTimeObjects.SpiedCivFive.Civilization;
        Civilization _spiedSixCiv = DesignTimeObjects.SpiedCivSix.Civilization;
        //public string BlameWhoOne
        //{
        //    get { return _blameWhoOne; }
        //    set
        //    {
        //        value = _blameWhoOne;
        //    }
        //}

        public AssetsScreen()
        {

            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;

            _radioButtonZero = new RadioButton[] { BlameNoOne0, Terrorists0, Federation0, TerranEmpire0, Romulans0, Klingons0, Cardassians0, Dominion0, Borg0 };
            //just put them in the order so you can use Critera 1,2,3,4
            for (int i = 0; i < _radioButtonZero.Length; i++)
            {
                _radioButtonZero[i].Tag = i; //set your critera number into tag property here (1,2,3,4)
                //_radioButton[i]. += new EventHandler(OnBlameButtonsOneClick);
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButton[i].Name);
            }
            _radioButtonOne = new RadioButton[] { BlameNoOne1, Terrorists1, Federation1, TerranEmpire1, Romulans1, Klingons1, Cardassians1, Dominion1, Borg1 };
            //just put them in the order so you can use Critera 1,2,3,4
            for (int i = 0; i < _radioButtonOne.Length; i++)
            {
                _radioButtonOne[i].Tag = i; //set your critera number into tag property here (1,2,3,4)
                //_radioButton[i]. += new EventHandler(OnBlameButtonsOneClick);
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButton[i].Name);
            }
            _radioButtonTwo = new RadioButton[] { BlameNoOne2, Terrorists2, Federation2, TerranEmpire2, Romulans2, Klingons2, Cardassians2, Dominion2, Borg2 };
            for (int i = 0; i < _radioButtonTwo.Length; i++)
            {
                _radioButtonTwo[i].Tag = i; //set your critera number into tag property here (0,1,2,3,4... )
            }
            _radioButtonThree = new RadioButton[] { BlameNoOne3, Terrorists3, Federation3, TerranEmpire3, Romulans3, Klingons3, Cardassians3, Dominion3, Borg3 };
            for (int i = 0; i < _radioButtonThree.Length; i++)
            {
                _radioButtonThree[i].Tag = i; //set your critera number into tag property here (0,1,2,3,4... )
            }
            _radioButtonFour = new RadioButton[] { BlameNoOne4, Terrorists4, Federation4, TerranEmpire4, Romulans4, Klingons4, Cardassians4, Dominion4, Borg4 };
            for (int i = 0; i < _radioButtonFour.Length; i++)
            {
                _radioButtonFour[i].Tag = i; //set your critera number into tag property here (0,1,2,3,4... )
            }
            _radioButtonFive = new RadioButton[] { BlameNoOne5, Terrorists5, Federation5, TerranEmpire5, Romulans5, Klingons5, Cardassians5, Dominion5, Borg5 };
            for (int i = 0; i < _radioButtonFive.Length; i++)
            {
                _radioButtonFive[i].Tag = i; //set your critera number into tag property here (0,1,2,3,4... )
            }
            _radioButtonSix = new RadioButton[] { BlameNoOne6, Terrorists6, Federation6, TerranEmpire6, Romulans6, Klingons6, Cardassians6, Dominion6, Borg6 };
            for (int i = 0; i < _radioButtonSix.Length; i++)
            {
                _radioButtonSix[i].Tag = i; //set your critera number into tag property here (0,1,2,3,4... )
            }
            BlameNoOne0.IsChecked = true;
            BlameNoOne1.IsChecked = true;
            BlameNoOne2.IsChecked = true;
            BlameNoOne3.IsChecked = true;
            BlameNoOne4.IsChecked = true;
            BlameNoOne5.IsChecked = true;
            BlameNoOne6.IsChecked = true;

            //LoadInsignia();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _civLocalPlayer = DesignTimeObjects.CivilizationManager.Civilization;//AppContext.LocalPlayerEmpire.Civilization;
            if (IsVisible)
            {
                ResumeAnimations();
                GameLog.Client.UI.DebugFormat("begin of checking visible");
                if (!AssetsHelper.IsSpiedZero(_spiedZeroCiv) || _spiedZeroCiv == _civLocalPlayer)
                {
                    EmpireExpanderZero.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (_spiedZeroCiv != _civLocalPlayer)
                    {
                        SabotageEnergyZero.Visibility = Visibility.Visible;
                        SabotageFoodZero.Visibility = Visibility.Visible;
                        SabotageIndustryZero.Visibility = Visibility.Visible;
                        StealResearchZero.Visibility = Visibility.Visible;
                        StealCreditsZero.Visibility = Visibility.Visible;
                    }
                }
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
                            SabotageEnergyOne.Visibility = Visibility.Visible;
                            SabotageFoodOne.Visibility = Visibility.Visible;
                            SabotageIndustryOne.Visibility = Visibility.Visible;
                            StealResearchOne.Visibility = Visibility.Visible;
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
                            SabotageEnergyTwo.Visibility = Visibility.Visible;
                            SabotageFoodTwo.Visibility = Visibility.Visible;
                            SabotageIndustryTwo.Visibility = Visibility.Visible;
                            StealResearchTwo.Visibility = Visibility.Visible;
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
                            SabotageEnergyThree.Visibility = Visibility.Visible;
                            SabotageFoodThree.Visibility = Visibility.Visible;
                            SabotageIndustryThree.Visibility = Visibility.Visible;
                            StealResearchThree.Visibility = Visibility.Visible;
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
                            SabotageEnergyFour.Visibility = Visibility.Visible;
                            SabotageFoodFour.Visibility = Visibility.Visible;
                            SabotageIndustryFour.Visibility = Visibility.Visible;
                            StealResearchFour.Visibility = Visibility.Visible;
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
                            SabotageEnergyFive.Visibility = Visibility.Visible;
                            SabotageFoodFive.Visibility = Visibility.Visible;
                            SabotageIndustryFive.Visibility = Visibility.Visible;
                            StealResearchFive.Visibility = Visibility.Visible;
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
                            SabotageEnergySix.Visibility = Visibility.Visible;
                            SabotageFoodSix.Visibility = Visibility.Visible;
                            SabotageIndustrySix.Visibility = Visibility.Visible;
                            StealResearchSix.Visibility = Visibility.Visible;
                            StealCreditsSix.Visibility = Visibility.Visible;
                    }
                }
               //GameLog.Client.UI.DebugFormat("end  of checking visible");

                List<CivilizationManager> spyableCivManagers = new List<CivilizationManager>();

                var shortList = GameContext.Current.CivilizationManagers; // only CivilizationMangers in game and in CivID numerical sequence
                foreach (var manager in shortList)
                {
                    if (manager.Civilization.IsEmpire && manager != DesignTimeObjects.CivilizationManager) // not the local player
                    {
                        spyableCivManagers.Add(manager);
                    }
                }

                Dictionary<int, Civilization> empireCivsDictionary = new Dictionary<int, Civilization>();
                List<Civilization> empireCivsList = new List<Civilization>();
        
                int counting = 0;
                foreach (var civManager in spyableCivManagers)
                {
                    empireCivsDictionary.Add(civManager.CivilizationID, civManager.Civilization); //dictionary of civs that can be spied on with key set to CivID
                    empireCivsList.Add(civManager.Civilization); // list of civs that can be spied on by local player and in CivID sequence
                    GameLog.Client.UI.DebugFormat("Add civ = {0} to blame dictionary at key ={1}", civManager.Civilization.Key, civManager.CivilizationID);
                    GameLog.Client.UI.DebugFormat("Add civ.Key = {0} to blame list at index ={1}", civManager.Civilization.Key, counting);
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
        private void OnBlameButtonsZeroClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne0.IsChecked == true)
                {
                    _blameWhoZero = "No one";
                }
                if (Terrorists0.IsChecked == true)
                {
                    _blameWhoZero = "Terrorists";
                }
                if (Federation0.IsChecked == true)
                {
                    _blameWhoZero = "Federation";
                }
                if (TerranEmpire0.IsChecked == true)
                {
                    _blameWhoZero = "TerranEmpire";
                }
                if (Romulans0.IsChecked == true)
                {
                    _blameWhoZero = "Romulnas";
                }
                if (Klingons0.IsChecked == true)
                {
                    _blameWhoZero = "Klingons";
                }
                if (Cardassians0.IsChecked == true)
                {
                    _blameWhoZero = "Cardassians";
                }
                if (Borg0.IsChecked == true)
                {
                    _blameWhoZero = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Zero %$%$###$%$$#@ Blame Sting ={0}", _blameWhoZero);
            }
        }
        private void OnBlameButtonsOneClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne1.IsChecked == true)
                {
                    _blameWhoOne = "No one"; 
                }
                if (Terrorists1.IsChecked == true)
                {
                    _blameWhoOne = "Terrorists"; 
                }
                if (Federation1.IsChecked == true)
                {
                    _blameWhoOne = "Federation";
                }
                if (TerranEmpire1.IsChecked == true)
                {
                    _blameWhoOne = "TerranEmpire";
                }
                if (Romulans1.IsChecked == true)
                {
                    _blameWhoOne = "Romulnas";
                }
                if (Klingons1.IsChecked == true)
                {
                    _blameWhoOne = "Klingons";
                }
                if (Cardassians1.IsChecked == true)
                {
                    _blameWhoOne = "Cardassians";
                }
                if (Borg1.IsChecked == true)
                {
                    _blameWhoOne = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander One %$%$###$%$$#@ Blame Sting ={0}", _blameWhoOne);
            }
        }
         
        private void OnBlameButtonsTwoClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne2.IsChecked == true)
                {
                    _blameWhoTwo = "No one"; 
                }
                if (Terrorists2.IsChecked == true)
                {
                    _blameWhoTwo = "Terrorists"; 
                }
                if (Federation2.IsChecked == true)
                {
                    _blameWhoTwo = "Federation";
                }
                if (TerranEmpire2.IsChecked == true)
                {
                    _blameWhoTwo = "TerranEmpire";
                }
                if (Romulans2.IsChecked == true)
                {
                    _blameWhoTwo = "Romulnas";
                }
                if (Klingons2.IsChecked == true)
                {
                    _blameWhoTwo = "Klingons";
                }
                if (Cardassians2.IsChecked == true)
                {
                    _blameWhoTwo = "Cardassians";
                }
                if (Borg2.IsChecked == true)
                {
                    _blameWhoTwo = "Borg";
                }
               // GameLog.Client.UI.DebugFormat("Expander Two ############### Blame Sting ={0}", _blameWhoTwo);
            }
        }
        private void OnBlameButtonsThreeClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne3.IsChecked == true)
                {
                    _blameWhoThree = "No one"; 
                }
                if (Terrorists3.IsChecked == true)
                {
                    _blameWhoThree = "Terrorists"; 
                }
                if (Federation3.IsChecked == true)
                {
                    _blameWhoThree = "Federation";
                }
                if (TerranEmpire3.IsChecked == true)
                {
                    _blameWhoThree = "TerranEmpire";
                }
                if (Romulans3.IsChecked == true)
                {
                    _blameWhoThree = "Romulnas";
                }
                if (Klingons3.IsChecked == true)
                {
                    _blameWhoThree = "Klingons";
                }
                if (Cardassians3.IsChecked == true)
                {
                    _blameWhoThree = "Cardassians";
                }
                if (Borg3.IsChecked == true)
                {
                    _blameWhoThree = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Three ############### Blame Sting ={0}", _blameWhoThree);
            }
        }
        private void OnBlameButtonsFourClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne4.IsChecked == true)
                {
                    _blameWhoFour = "No one";
                }
                if (Terrorists4.IsChecked == true)
                {
                    _blameWhoFour = "Terrorists";
                }
                if (Federation4.IsChecked == true)
                {
                    _blameWhoFour = "Federation";
                }
                if (TerranEmpire4.IsChecked == true)
                {
                    _blameWhoFour = "TerranEmpire";
                }
                if (Romulans4.IsChecked == true)
                {
                    _blameWhoFour = "Romulnas";
                }
                if (Klingons4.IsChecked == true)
                {
                    _blameWhoFour = "Klingons";
                }
                if (Cardassians4.IsChecked == true)
                {
                    _blameWhoFour = "Cardassians";
                }
                if (Borg4.IsChecked == true)
                {
                    _blameWhoFour = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Four ############### Blame Sting ={0}", _blameWhoFour);
            }
        }
        private void OnBlameButtonsFiveClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne5.IsChecked == true)
                {
                    _blameWhoFive = "No one";
                }
                if (Terrorists5.IsChecked == true)
                {
                    _blameWhoFive = "Terrorists";
                }
                if (Federation5.IsChecked == true)
                {
                    _blameWhoFive = "Federation";
                }
                if (TerranEmpire5.IsChecked == true)
                {
                    _blameWhoFive = "TerranEmpire";
                }
                if (Romulans5.IsChecked == true)
                {
                    _blameWhoFive = "Romulnas";
                }
                if (Klingons5.IsChecked == true)
                {
                    _blameWhoFive = "Klingons";
                }
                if (Cardassians5.IsChecked == true)
                {
                    _blameWhoFive = "Cardassians";
                }
                if (Borg5.IsChecked == true)
                {
                    _blameWhoFive = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Five ############### Blame Sting ={0}", _blameWhoFive);
            }
        }
        private void OnBlameButtonsSixClick(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (BlameNoOne6.IsChecked == true)
                {
                    _blameWhoSix = "No one";
                }
                if (Terrorists6.IsChecked == true)
                {
                    _blameWhoSix = "Terrorists";
                }
                if (Federation6.IsChecked == true)
                {
                    _blameWhoSix = "Federation";
                }
                if (TerranEmpire6.IsChecked == true)
                {
                    _blameWhoSix = "TerranEmpire";
                }
                if (Romulans6.IsChecked == true)
                {
                    _blameWhoSix = "Romulnas";
                }
                if (Klingons6.IsChecked == true)
                {
                    _blameWhoSix = "Klingons";
                }
                if (Cardassians6.IsChecked == true)
                {
                    _blameWhoSix = "Cardassians";
                }
                if (Borg6.IsChecked == true)
                {
                    _blameWhoSix = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Six ############### Blame Sting ={0}", _blameWhoSix);
            }
        }
        private void OnCreditsZeroClick(object sender, RoutedEventArgs e) // we are using attacking spy civ as peramiter here in Creidt only so far
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            StealCreditsZero.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            StealCreditsOne.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            StealCreditsTwo.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            StealCreditsThree.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            StealCreditsFour.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            StealCreditsFive.Visibility = Visibility.Collapsed;
        }
        private void OnCreditsSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealCredits(IntelHelper.NewSpiedColonies.FirstOrDefault(), IntelHelper.NewSpyCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            StealCreditsSix.Visibility = Visibility.Collapsed;
        }
        private void OnResearchZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            StealResearchZero.Visibility = Visibility.Collapsed;
        }
        private void OnResearchOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            StealResearchOne.Visibility = Visibility.Collapsed;
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            StealResearchTwo.Visibility = Visibility.Collapsed;
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            StealResearchThree.Visibility = Visibility.Collapsed;
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            StealResearchFour.Visibility = Visibility.Collapsed;
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            StealResearchFive.Visibility = Visibility.Collapsed;
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            StealResearchSix.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero); //, out removedEnergyFacilities);
            SabotageEnergyZero.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne); //, out removedEnergyFacilities);
            SabotageEnergyOne.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            SabotageEnergyTwo.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            SabotageEnergyThree.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            SabotageEnergyFour.Visibility = Visibility.Collapsed;
        }
        private void OnEnergyFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            SabotageEnergyFive.Visibility = Visibility.Collapsed;
        }
        private void OnEnergySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            SabotageEnergySix.Visibility = Visibility.Collapsed;
        }
        private void OnFoodZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            SabotageFoodZero.Visibility = Visibility.Collapsed;
        }
        private void OnFoodOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            SabotageFoodOne.Visibility = Visibility.Collapsed;
        }
        private void OnFoodTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            SabotageFoodTwo.Visibility = Visibility.Collapsed;
        }
        private void OnFoodThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            SabotageFoodThree.Visibility = Visibility.Collapsed;
        }
        private void OnFoodFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            SabotageFoodFour.Visibility = Visibility.Collapsed;
        }
        private void OnFoodFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            SabotageFoodFive.Visibility = Visibility.Collapsed;
        }
        private void OnFoodSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            SabotageFoodSix.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            SabotageIndustryZero.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            SabotageIndustryOne.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            SabotageIndustryTwo.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            SabotageIndustryThree.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            SabotageIndustryFour.Visibility = Visibility.Collapsed;
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            SabotageIndustryFive.Visibility = Visibility.Collapsed;
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            SabotageIndustrySix.Visibility = Visibility.Collapsed;
        }
        #endregion
        //private void LoadInsignia()
        //{
        //    GameLog.Client.UI.DebugFormat("Loading Insignias/FEDERATION.png and more");
        //    BitmapImage insigniaFed = new BitmapImage();
        //    var uriFed = new Uri("vfs:///Resources/Images/Insignias/FEDERATION.png");
        //    insigniaFed.BeginInit();
        //    insigniaFed.UriSource = uriFed;
        //    insigniaFed.EndInit();

        //    BitmapImage insigniaTerran = new BitmapImage();
        //    var uriTerran = new Uri("vfs:///Resources/Images/Insignias/TERRANEMPIRE.png");
        //    insigniaTerran.BeginInit();
        //    insigniaTerran.UriSource = uriTerran;
        //    insigniaTerran.EndInit();

        //    BitmapImage insigniaRom = new BitmapImage();
        //    var uriRom = new Uri("vfs:///Resources/Images/Insignias/ROMULANS.png");
        //    insigniaRom.BeginInit();
        //    insigniaRom.UriSource = uriRom;
        //    insigniaRom.EndInit();

        //    BitmapImage insigniaKling = new BitmapImage();
        //    var uriKling = new Uri("vfs:///Resources/Images/Insignias/KLINGONS.png");
        //    insigniaKling.BeginInit();
        //    insigniaKling.UriSource = uriKling;
        //    insigniaKling.EndInit();

        //    BitmapImage insigniaCard = new BitmapImage();
        //    var uriCard = new Uri("vfs:///Resources/Images/Insignias/CARDASSIANS.png");
        //    insigniaCard.BeginInit();
        //    insigniaCard.UriSource = uriCard;
        //    insigniaCard.EndInit();

        //    BitmapImage insigniaDom = new BitmapImage();
        //    var uriDom = new Uri("vfs:///Resources/Images/Insignias/DOMINION.png");
        //    insigniaDom.BeginInit();
        //    insigniaDom.UriSource = uriDom;
        //    insigniaDom.EndInit();

        //    BitmapImage insigniaBorg = new BitmapImage();
        //    var uriBorg = new Uri("vfs:///Resources/Images/Insignias/BORG.png");
        //    insigniaBorg.BeginInit();
        //    insigniaBorg.UriSource = uriBorg;
        //    insigniaBorg.EndInit();
        //    GameLog.Client.UI.DebugFormat("Loading Insignias is finished");

        //    List<int> CivIDs = new List<int>();
        //    //if (AssetsScreenPresentationModel.SpiedZeroCiv != null)
        //    //    CivIDs.Add(AssetsScreenPresentationModel.SpiedZeroCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedOneCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedOneCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedTwoCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedTwoCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedThreeCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedThreeCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedFourCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedFourCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedFiveCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedFiveCiv.CivID);
        //    if (AssetsScreenPresentationModel.SpiedSixCiv != null)
        //        CivIDs.Add(AssetsScreenPresentationModel.SpiedSixCiv.CivID);
        //    GameLog.Client.UI.DebugFormat("Adding SpiedCiv is finished");

        //    if (CivIDs.Count >= 1)
        //    {
        //        switch (CivIDs[0])
        //        {
        //            case 0:
        //                InsigniaOne.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaOne.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaOne.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaOne.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaOne.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaOne.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaOne.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    if (CivIDs.Count >= 2)
        //    {
        //        switch (CivIDs[1])
        //        {
        //            case 0:
        //                InsigniaTwo.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaTwo.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaTwo.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaTwo.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaTwo.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaTwo.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaTwo.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    if (CivIDs.Count >= 3)
        //    {
        //        switch (CivIDs[2])
        //        {
        //            case 0:
        //                InsigniaThree.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaThree.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaThree.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaThree.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaThree.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaThree.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaThree.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    if (CivIDs.Count >= 4)
        //    {
        //        switch (CivIDs[3])
        //        {
        //            case 0:
        //                InsigniaFour.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaFour.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaFour.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaFour.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaFour.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaFour.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaFour.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    if (CivIDs.Count >= 5)
        //    {
        //        switch (CivIDs[4])
        //        {
        //            case 0:
        //                InsigniaFive.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaFive.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaFive.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaFive.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaFive.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaFive.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaFive.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    if (CivIDs.Count >= 6)
        //    {
        //        switch (CivIDs[5])
        //        {
        //            case 0:
        //                InsigniaSix.Source = insigniaFed;
        //                break;
        //            case 1:
        //                InsigniaSix.Source = insigniaTerran;
        //                break;
        //            case 2:
        //                InsigniaSix.Source = insigniaRom;
        //                break;
        //            case 3:
        //                InsigniaSix.Source = insigniaKling;
        //                break;
        //            case 4:
        //                InsigniaSix.Source = insigniaCard;
        //                break;
        //            case 5:
        //                InsigniaSix.Source = insigniaDom;
        //                break;
        //            case 6:
        //                InsigniaSix.Source = insigniaBorg;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
            //GameLog.Client.UI.DebugFormat("Insignia is finished");
        //}      
    }
}