//File: AssetsScreen.xaml.cs
using Avalon.Windows.Annotations;
using Microsoft.Practices.Unity;

using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Types;
using Supremacy.Utility;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;


namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for AssetsScreen.xaml
    /// </summary>
    public partial class AssetsScreen : IAssetsScreenView, IWeakEventListener
    {
        private readonly IUnityContainer _container;
        private readonly IAppContext _appContext;
        private CivilizationManager _localCivManager;
        //private IntelUpdate _update;

        // order dictionary is located in IntelOrders.cs constructor, store orders in core of host?

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

        protected int _totalIntelligenceProduction;
        protected int _totalIntelligenceDefenseAccumulated;
        protected int _totalIntelligenceAttackingAccumulated;


        #region Properties for AssestsScreen

        public Meter UpdateAttackingAccumulated(Civilization attackingCiv)
        {
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            return attackMeter;
        }
        protected virtual void FillUpDefense()
        {
            CivilizationManager civ = GameContext.Current.CivilizationManagers[DesignTimeObjects.CivilizationManager.Civilization];
            civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
            civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
            civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
            civ.TotalIntelligenceDefenseAccumulated.UpdateAndReset();
            //OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            //OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            //OnPropertyChanged("TotalIntelligenceProduction");

        }
        #endregion 
        public AssetsScreen([NotNull] IUnityContainer container)
        {
            GameLog.Client.UI.DebugFormat("AssetsScreen - InitializeComponent();");

            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
            _appContext = _container.Resolve<IAppContext>();
            _localCivManager = _appContext.LocalPlayerEmpire;
            InitializeComponent();
            PropertyChangedEventManager.AddListener(_appContext, this, "LocalPlayerEmpire");
            IntelHelper.GetLocalCiv(_localCivManager);
            // ClientEvents.IntelUpdateReceived.Subscribe(OnIntelUpdateReceived, ThreadOption.UIThread);
            // DataTemplate itemTemplate = TryFindResource("AssetsTreeItemTemplate") as DataTemplate;

            //for (int i = 0; i < this.; i++)
            //{

            //}


            IsVisibleChanged += OnIsVisibleChanged;

            _radioButtonZero = new RadioButton[] { BlameNoOne0, Terrorists0, Federation0, TerranEmpire0, Romulans0, Klingons0, Cardassians0, Dominion0, Borg0 };
            //just put them in the order so you can use item 1,2,3,4
            for (int i = 0; i < _radioButtonZero.Length; i++)
            {
                _radioButtonZero[i].Tag = i; //set your item number into tag property here (1,2,3,4)
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButtonZero[i].Name);
            }
            _radioButtonOne = new RadioButton[] { BlameNoOne1, Terrorists1, Federation1, TerranEmpire1, Romulans1, Klingons1, Cardassians1, Dominion1, Borg1 };
            //just put them in the order so you can use Critera 1,2,3,4
            for (int i = 0; i < _radioButtonOne.Length; i++)
            {
                _radioButtonOne[i].Tag = i;
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButton[i].Name);
            }
            _radioButtonTwo = new RadioButton[] { BlameNoOne2, Terrorists2, Federation2, TerranEmpire2, Romulans2, Klingons2, Cardassians2, Dominion2, Borg2 };
            for (int i = 0; i < _radioButtonTwo.Length; i++)
            {
                _radioButtonTwo[i].Tag = i;
            }
            _radioButtonThree = new RadioButton[] { BlameNoOne3, Terrorists3, Federation3, TerranEmpire3, Romulans3, Klingons3, Cardassians3, Dominion3, Borg3 };
            for (int i = 0; i < _radioButtonThree.Length; i++)
            {
                _radioButtonThree[i].Tag = i;
            }
            _radioButtonFour = new RadioButton[] { BlameNoOne4, Terrorists4, Federation4, TerranEmpire4, Romulans4, Klingons4, Cardassians4, Dominion4, Borg4 };
            for (int i = 0; i < _radioButtonFour.Length; i++)
            {
                _radioButtonFour[i].Tag = i;
            }
            _radioButtonFive = new RadioButton[] { BlameNoOne5, Terrorists5, Federation5, TerranEmpire5, Romulans5, Klingons5, Cardassians5, Dominion5, Borg5 };
            for (int i = 0; i < _radioButtonFive.Length; i++)
            {
                _radioButtonFive[i].Tag = i;
            }
            _radioButtonSix = new RadioButton[] { BlameNoOne6, Terrorists6, Federation6, TerranEmpire6, Romulans6, Klingons6, Cardassians6, Dominion6, Borg6 };
            for (int i = 0; i < _radioButtonSix.Length; i++)
            {
                _radioButtonSix[i].Tag = i;
            }
            BlameNoOne0.IsChecked = true;
            BlameNoOne1.IsChecked = true;
            BlameNoOne2.IsChecked = true;
            BlameNoOne3.IsChecked = true;
            BlameNoOne4.IsChecked = true;
            BlameNoOne5.IsChecked = true;
            BlameNoOne6.IsChecked = true;
        }
        //private void Terrorists()
        //{
        //    Civilization Terrorists = new Civilization();
        //    CivilizationManager TerroristsManager = new CivilizationManager(GameContext.Current, Terrorists);
        //    Terrorists.Key = "Terrorists";
        //    Terrorists.ShortName = "Terrorists_ShortName";

        //    if (GameContext.Current.TurnNumber > 2)
        //    {
        //        var availableCivManagers = DesignTimeObjects.AvailableCivManagers; //GameContext.Current.CivilizationManagers.Where(o => o.Civilization.IsEmpire).ToList();

        //        foreach (var civManager in availableCivManagers)
        //        {
        //            switch (civManager.Civilization.Key)
        //            {
        //                case "BORG":
        //                    if (RandomHelper.Chance(95))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[6];
        //                    break;
        //                case "KLINGONS":
        //                    if (RandomHelper.Chance(80))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[3];
        //                    break;
        //                case "ROMULANS":
        //                    if (RandomHelper.Chance(75))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[2];
        //                    break;
        //                case "FEDERATION":
        //                    if (RandomHelper.Chance(70))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[0];
        //                    break;
        //                case "CARDASSIANS":
        //                    if (RandomHelper.Chance(65))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[4];
        //                    break;
        //                case "DOMINION":
        //                    if (RandomHelper.Chance(60))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[5];
        //                    break;
        //                case "TERRANEMPIRE":
        //                    if (RandomHelper.Chance(50))
        //                        TerroristsManager = GameContext.Current.CivilizationManagers[1];
        //                    break;
        //            }
        //        }
        //        if (Terrorists.ShortName != "Terrorists_ShortName")
        //            FindTarget(TerroristsManager);
        //    }
        //}

        //private void FindTarget(CivilizationManager civManager)
        //{
        //    GameLog.Client.UI.DebugFormat(" ********** terrorists targeet civ = {0} ************ ", civManager.Civilization.Key);

        //    Random random = new Random();
        //    int choseTheTarget = random.Next(0, 4);
        //    var Civs = GameContext.Current.CivilizationManagers.Where(o => o.Civilization.IsEmpire).ToList();
        //    var luckyCiv = Civs.OrderBy(s => random.Next()).First();
        //    switch (choseTheTarget)
        //    {
        //        case 0:
        //            IntelHelper.StealCredits(civManager.SeatOfGovernment, civManager.Civilization, luckyCiv.Civilization, "Terrorists");
        //            break;
        //        case 1:
        //            IntelHelper.StealResearch(civManager.SeatOfGovernment, civManager.Civilization, luckyCiv.Civilization, "Terrorists");
        //            break;
        //        case 2:
        //            IntelHelper.SabotageEnergy(civManager.SeatOfGovernment, civManager.Civilization, luckyCiv.Civilization, "Terrorists");
        //            break;
        //        case 3:
        //            IntelHelper.SabotageFood(civManager.SeatOfGovernment, civManager.Civilization, luckyCiv.Civilization, "Terrorists");
        //            break;
        //        case 4:
        //            IntelHelper.SabotageIndustry(civManager.SeatOfGovernment, civManager.Civilization, luckyCiv.Civilization, "Terrorists");
        //            break;
        //        default:
        //            break;
        //    }
        //}
        private void OnLocalPlayerEmpireChanged()
        {
            if (!_appContext.IsGameInPlay || _appContext.IsGameEnding)
                return;

            CivilizationManager localPlayerEmpire = _appContext.LocalPlayerEmpire;
            //works  GameLog.Client.UI.DebugFormat("AssetsScreen local player ={0}", localPlayerEmpire.Civilization.Key);
            if (localPlayerEmpire == null)
                return;
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            Civilization _civLocalPlayer = _appContext.LocalPlayer.Empire;

            if (IsVisible)
            {
                //ResumeAnimations();
                GameLog.Client.UI.DebugFormat("*********** begin of checking visible ***********");

                // GameLog.Client.UI.DebugFormat("SpiedZeroCiv checking visible .... _spiedOneCiv = {0}, _civLocalPlayer = {1}", _spiedZeroCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedZero(_civLocalPlayer) || IntelHelper.ShowNetwork_0)
                {
                    EmpireExpanderZero.Visibility = Visibility.Visible;
                    SabotageEnergyZero.Visibility = Visibility.Visible;
                    SabotageFoodZero.Visibility = Visibility.Visible;
                    SabotageIndustryZero.Visibility = Visibility.Visible;
                    StealResearchZero.Visibility = Visibility.Visible;
                    StealCreditsZero.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedOneCiv checking visible .... _spiedOneCiv = {0}, _civLocalPlayer = {1}", _spiedOneCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedOne(_civLocalPlayer) || IntelHelper.ShowNetwork_1)
                {
                    EmpireExpanderOne.Visibility = Visibility.Visible;
                    SabotageEnergyOne.Visibility = Visibility.Visible;
                    SabotageFoodOne.Visibility = Visibility.Visible;
                    SabotageIndustryOne.Visibility = Visibility.Visible;
                    StealResearchOne.Visibility = Visibility.Visible;
                    StealCreditsOne.Visibility = Visibility.Visible;
                }
                // GameLog.Client.UI.DebugFormat("SpiedTwoCiv checking visible .... _spiedTwoCiv = {0}, _civLocalPlayer = {1}", _spiedTwoCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedTwo(_civLocalPlayer) || IntelHelper.ShowNetwork_2)
                {
                    EmpireExpanderTwo.Visibility = Visibility.Visible;
                    SabotageEnergyTwo.Visibility = Visibility.Visible;
                    SabotageFoodTwo.Visibility = Visibility.Visible;
                    SabotageIndustryTwo.Visibility = Visibility.Visible;
                    StealResearchTwo.Visibility = Visibility.Visible;
                    StealCreditsTwo.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedThreeCiv checking visible .... _spiedThreeCiv = {0}, _civLocalPlayer = {1}", _spiedThreeCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedThree(_civLocalPlayer) || IntelHelper.ShowNetwork_3)
                {
                    EmpireExpanderThree.Visibility = Visibility.Visible;
                    SabotageEnergyThree.Visibility = Visibility.Visible;
                    SabotageFoodThree.Visibility = Visibility.Visible;
                    SabotageIndustryThree.Visibility = Visibility.Visible;
                    StealResearchThree.Visibility = Visibility.Visible;
                    StealCreditsThree.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedCiv cFourhecking visible .... _spiedFourCiv = {0}, _civLocalPlayer = {1}", _spiedFourCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedFour(_civLocalPlayer) || IntelHelper.ShowNetwork_4)
                {
                    EmpireExpanderFour.Visibility = Visibility.Visible;
                    SabotageEnergyFour.Visibility = Visibility.Visible;
                    SabotageFoodFour.Visibility = Visibility.Visible;
                    SabotageIndustryFour.Visibility = Visibility.Visible;
                    StealResearchFour.Visibility = Visibility.Visible;
                    StealCreditsFour.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedFiveCiv checking visible .... _spiedFiveCiv = {0}, _civLocalPlayer = {1}", _spiedFiveCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedFive(_civLocalPlayer) || IntelHelper.ShowNetwork_5)
                {
                    EmpireExpanderFive.Visibility = Visibility.Visible;
                    SabotageEnergyFive.Visibility = Visibility.Visible;
                    SabotageFoodFive.Visibility = Visibility.Visible;
                    SabotageIndustryFive.Visibility = Visibility.Visible;
                    StealResearchFive.Visibility = Visibility.Visible;
                    StealCreditsFive.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedSixCiv checking visible .... _spiedSixCiv = {0}, _civLocalPlayer = {1}", _spiedSixCiv, _civLocalPlayer);
                if (AssetsHelper.IsSpiedSix(_civLocalPlayer) || IntelHelper.ShowNetwork_6)
                {
                    EmpireExpanderSix.Visibility = Visibility.Visible;
                    SabotageEnergySix.Visibility = Visibility.Visible;
                    SabotageFoodSix.Visibility = Visibility.Visible;
                    SabotageIndustrySix.Visibility = Visibility.Visible;
                    StealResearchSix.Visibility = Visibility.Visible;
                    StealCreditsSix.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("end  of checking visible");

                // GameLog.Client.UI.DebugFormat("_civLocalPlayer = {0}", _civLocalPlayer.Key);

                Diplomat diplomat1 = Diplomat.Get(GameContext.Current.CivilizationManagers[_civLocalPlayer.CivID]);
                int empireCount = GameContext.Current.Civilizations.Where(o => o.IsEmpire).Count();
                List<Civilization> empireCivsList = GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList();
                List<int> empireIdList = new List<int>();
                foreach (Civilization empireCiv in empireCivsList)
                {
                    empireIdList.Add(empireCiv.CivID);
                }

                //for (int i = 0; i < empireCount; i++)
                foreach (int empireID in empireIdList)

                {
                    if (empireID == _civLocalPlayer.CivID)
                        continue;

                    ForeignPower ForeignPower = diplomat1.GetForeignPower(GameContext.Current.CivilizationManagers[empireID]);
                    bool _checkedVisibleForSabotagePending = true;

                    //_checkedVisibleForSabotagePending = CheckingVisibityForSabotagePending(diplomat1, ForeignPower);

                    //if (ForeignPower.LastStatementSent != null)
                    if (diplomat1.GetLastStatementSent(ForeignPower) != null)
                    {
                        int _statementSentInTurn = diplomat1.GetLastStatementSent(ForeignPower).TurnSent;

                        if (_statementSentInTurn == 99999)
                            _statementSentInTurn = 1;
                        //switch (ForeignPower.LastStatementSent.StatementType)
                        if (GameContext.Current.TurnNumber < _statementSentInTurn + 2)
                            switch (diplomat1.GetLastStatementSent(ForeignPower).StatementType)
                            {
                                case StatementType.StealCredits:
                                case StatementType.StealResearch:
                                case StatementType.SabotageFood:
                                case StatementType.SabotageIndustry:
                                case StatementType.SabotageEnergy:
                                    _checkedVisibleForSabotagePending = false;
                                    //_visibleForSabotagePending = false;
                                    break;
                                case StatementType.CommendWar:
                                case StatementType.DenounceWar:
                                case StatementType.WarDeclaration:
                                    break;
                                default:
                                    break;
                            }
                    }
                    //_checkedVisibleForSabotagePending = _visibleForSabotagePending;

                    // just for testing      _checkedVisibleForSabotagePending = true;
                    if (_checkedVisibleForSabotagePending == false)
                        switch (empireID)
                        {
                            case 0:
                                Close_0_SabotageButtons();
                                break;
                            case 1:
                                Close_1_SabotageButtons();
                                break;
                            case 2:
                                Close_2_SabotageButtons();
                                break;
                            case 3:
                                Close_3_SabotageButtons();
                                break;
                            case 4:
                                Close_4_SabotageButtons();
                                break;
                            case 5:
                                Close_5_SabotageButtons();
                                break;
                            case 6:
                                Close_6_SabotageButtons();
                                break;
                            default: break;
                        }
                }

                Dictionary<int, Civilization> empireCivsDictionary = new Dictionary<int, Civilization>();

                foreach (Civilization civ in empireCivsList)
                {
                    empireCivsDictionary.Add(civ.CivID, civ); //dictionary of civs that can be spied on with key set to CivID
                    //GameLog.Client.UI.DebugFormat("Add civ = {0} to blame dictionary at key ={1}", civManager.Civilization.Key, civManager.CivilizationID);
                    // GameLog.Client.UI.DebugFormat("Add civ.Key = {0} to blame list at index ={1}", civManager.Civilization.Key, counting);
                }
                //GameLog.Client.UI.DebugFormat("FED: begin of checking BLAME visible");

                if (empireCivsDictionary.Keys.Contains(0) && _civLocalPlayer.CivID != 0 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[0]].IsContactMade())
                {
                    BlameFederation1.Visibility = Visibility.Visible;
                    BlameFederation2.Visibility = Visibility.Visible;
                    BlameFederation3.Visibility = Visibility.Visible;
                    BlameFederation4.Visibility = Visibility.Visible;
                    BlameFederation5.Visibility = Visibility.Visible;
                    BlameFederation6.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("FED: end   of checking BLAME visible");

                if (empireCivsDictionary.Keys.Contains(1) && _civLocalPlayer.CivID != 1 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[1]].IsContactMade())
                {
                    BlameTerranEmpire0.Visibility = Visibility.Visible;
                    BlameTerranEmpire2.Visibility = Visibility.Visible;
                    BlameTerranEmpire3.Visibility = Visibility.Visible;
                    BlameTerranEmpire4.Visibility = Visibility.Visible;
                    BlameTerranEmpire5.Visibility = Visibility.Visible;
                    BlameTerranEmpire6.Visibility = Visibility.Visible;
                }
                if (empireCivsDictionary.Keys.Contains(2) && _civLocalPlayer.CivID != 2 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[2]].IsContactMade())
                {
                    BlameRomulans0.Visibility = Visibility.Visible;
                    BlameRomulans1.Visibility = Visibility.Visible;
                    BlameRomulans3.Visibility = Visibility.Visible;
                    BlameRomulans4.Visibility = Visibility.Visible;
                    BlameRomulans5.Visibility = Visibility.Visible;
                    BlameRomulans6.Visibility = Visibility.Visible;
                }
                if (empireCivsDictionary.Keys.Contains(3) && _civLocalPlayer.CivID != 3 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[3]].IsContactMade())
                {
                    BlameKlingons0.Visibility = Visibility.Visible;
                    BlameKlingons1.Visibility = Visibility.Visible;
                    BlameKlingons2.Visibility = Visibility.Visible;
                    BlameKlingons4.Visibility = Visibility.Visible;
                    BlameKlingons5.Visibility = Visibility.Visible;
                    BlameKlingons6.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("CARD: begin of checking BLAME visible");
                if (empireCivsDictionary.Keys.Contains(4) && _civLocalPlayer.CivID != 4 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[4]].IsContactMade()) // && sevenCivs[4].Key != "CARDASSIANS")
                {
                    BlameCardassians0.Visibility = Visibility.Visible;
                    BlameCardassians1.Visibility = Visibility.Visible;
                    BlameCardassians2.Visibility = Visibility.Visible;
                    BlameCardassians3.Visibility = Visibility.Visible;
                    BlameCardassians5.Visibility = Visibility.Visible;
                    BlameCardassians6.Visibility = Visibility.Visible;
                }

                if (empireCivsDictionary.Keys.Contains(5) && _civLocalPlayer.CivID != 5 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[5]].IsContactMade())
                {
                    BlameDominion0.Visibility = Visibility.Visible;
                    BlameDominion1.Visibility = Visibility.Visible;
                    BlameDominion2.Visibility = Visibility.Visible;
                    BlameDominion3.Visibility = Visibility.Visible;
                    BlameDominion4.Visibility = Visibility.Visible;
                    BlameDominion6.Visibility = Visibility.Visible;
                }

                if (empireCivsDictionary.Keys.Contains(6) && _civLocalPlayer.CivID != 6 &&
                    GameContext.Current.DiplomacyData[_civLocalPlayer, empireCivsDictionary[6]].IsContactMade())
                {
                    BlameBorg0.Visibility = Visibility.Visible;
                    BlameBorg1.Visibility = Visibility.Visible;
                    BlameBorg2.Visibility = Visibility.Visible;
                    BlameBorg3.Visibility = Visibility.Visible;
                    BlameBorg4.Visibility = Visibility.Visible;
                    BlameBorg5.Visibility = Visibility.Visible;
                }
            }
            else
                PauseAnimations();
        }

        private bool CheckingVisibityForSabotagePending(Diplomat diplomat1, ForeignPower foreignPower)
        {
            bool _visibleForSabotagePending = true;  // more often it is useful to have it visible !
            if (foreignPower.LastStatementSent != null)
            {
                switch (foreignPower.LastStatementSent.StatementType)
                {
                    case StatementType.StealCredits:
                    case StatementType.StealResearch:
                    case StatementType.SabotageFood:
                    case StatementType.SabotageIndustry:
                    case StatementType.SabotageEnergy:
                        _visibleForSabotagePending = false;
                        break;
                    case StatementType.CommendWar:
                    case StatementType.DenounceWar:
                    case StatementType.WarDeclaration:
                        break;
                    default:
                        break;
                }

            }
            return _visibleForSabotagePending;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }

        protected void PauseAnimations()
        {
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
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
        // do we need this in the AssetsScreen???  we should keep it, if we bring back suns and planets
        protected void ResumeAnimations()
        {
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
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
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
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
                    _blameWhoZero = "Romulans";
                }
                if (Klingons0.IsChecked == true)
                {
                    _blameWhoZero = "Klingons";
                }
                if (Cardassians0.IsChecked == true)
                {
                    _blameWhoZero = "Cardassians";
                }
                if (Dominion0.IsChecked == true)
                {
                    _blameWhoZero = "Dominion";
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
                    _blameWhoOne = "Romulans";
                }
                if (Klingons1.IsChecked == true)
                {
                    _blameWhoOne = "Klingons";
                }
                if (Cardassians1.IsChecked == true)
                {
                    _blameWhoOne = "Cardassians";
                }
                if (Dominion1.IsChecked == true)
                {
                    _blameWhoOne = "Dominion";
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
                    _blameWhoTwo = "Romulans";
                }
                if (Klingons2.IsChecked == true)
                {
                    _blameWhoTwo = "Klingons";
                }
                if (Cardassians2.IsChecked == true)
                {
                    _blameWhoTwo = "Cardassians";
                }
                if (Dominion2.IsChecked == true)
                {
                    _blameWhoTwo = "Dominion";
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
                    _blameWhoThree = "Romulans";
                }
                if (Klingons3.IsChecked == true)
                {
                    _blameWhoThree = "Klingons";
                }
                if (Cardassians3.IsChecked == true)
                {
                    _blameWhoThree = "Cardassians";
                }
                if (Dominion3.IsChecked == true)
                {
                    _blameWhoThree = "Dominion";
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
                    _blameWhoFour = "Romulans";
                }
                if (Klingons4.IsChecked == true)
                {
                    _blameWhoFour = "Klingons";
                }
                if (Cardassians4.IsChecked == true)
                {
                    _blameWhoFour = "Cardassians";
                }
                if (Dominion4.IsChecked == true)
                {
                    _blameWhoFour = "Dominion";
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
                    _blameWhoFive = "Romulans";
                }
                if (Klingons5.IsChecked == true)
                {
                    _blameWhoFive = "Klingons";
                }
                if (Cardassians5.IsChecked == true)
                {
                    _blameWhoFive = "Cardassians";
                }
                if (Dominion5.IsChecked == true)
                {
                    _blameWhoFive = "Dominion";
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
                    _blameWhoSix = "Romulans";
                }
                if (Klingons6.IsChecked == true)
                {
                    _blameWhoSix = "Klingons";
                }
                if (Cardassians6.IsChecked == true)
                {
                    _blameWhoSix = "Cardassians";
                }
                if (Dominion6.IsChecked == true)
                {
                    _blameWhoSix = "Dominion";
                }
                if (Borg6.IsChecked == true)
                {
                    _blameWhoSix = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Six ############### Blame Sting ={0}", _blameWhoSix);
            }
        }
        private void Close_0_SabotageButtons()
        {
            StealCreditsZero.Visibility = Visibility.Collapsed;
            StealResearchZero.Visibility = Visibility.Collapsed;
            SabotageEnergyZero.Visibility = Visibility.Collapsed;
            SabotageFoodZero.Visibility = Visibility.Collapsed;
            SabotageIndustryZero.Visibility = Visibility.Collapsed;
        }
        private void Close_1_SabotageButtons()
        {
            StealCreditsZero.Visibility = Visibility.Collapsed;
            StealResearchZero.Visibility = Visibility.Collapsed;
            SabotageEnergyZero.Visibility = Visibility.Collapsed;
            SabotageFoodZero.Visibility = Visibility.Collapsed;
            SabotageIndustryZero.Visibility = Visibility.Collapsed;


            StealCreditsOne.Visibility = Visibility.Collapsed;
            StealResearchOne.Visibility = Visibility.Collapsed;
            SabotageEnergyOne.Visibility = Visibility.Collapsed;
            SabotageFoodOne.Visibility = Visibility.Collapsed;
            SabotageIndustryOne.Visibility = Visibility.Collapsed;
        }
        private void Close_2_SabotageButtons()
        {
            StealCreditsTwo.Visibility = Visibility.Collapsed;
            StealResearchTwo.Visibility = Visibility.Collapsed;
            SabotageEnergyTwo.Visibility = Visibility.Collapsed;
            SabotageFoodTwo.Visibility = Visibility.Collapsed;
            SabotageIndustryTwo.Visibility = Visibility.Collapsed;
        }
        private void Close_3_SabotageButtons()
        {
            StealCreditsThree.Visibility = Visibility.Collapsed;
            StealResearchThree.Visibility = Visibility.Collapsed;
            SabotageEnergyThree.Visibility = Visibility.Collapsed;
            SabotageFoodThree.Visibility = Visibility.Collapsed;
            SabotageIndustryThree.Visibility = Visibility.Collapsed;
        }
        private void Close_4_SabotageButtons()
        {
            StealCreditsFour.Visibility = Visibility.Collapsed;
            StealResearchFour.Visibility = Visibility.Collapsed;
            SabotageEnergyFour.Visibility = Visibility.Collapsed;
            SabotageFoodFour.Visibility = Visibility.Collapsed;
            SabotageIndustryFour.Visibility = Visibility.Collapsed;
        }
        private void Close_5_SabotageButtons()
        {
            StealCreditsFive.Visibility = Visibility.Collapsed;
            StealResearchFive.Visibility = Visibility.Collapsed;
            SabotageEnergyFive.Visibility = Visibility.Collapsed;
            SabotageFoodFive.Visibility = Visibility.Collapsed;
            SabotageIndustryFive.Visibility = Visibility.Collapsed;
        }
        private void Close_6_SabotageButtons()
        {
            StealCreditsSix.Visibility = Visibility.Collapsed;
            StealResearchSix.Visibility = Visibility.Collapsed;
            SabotageEnergySix.Visibility = Visibility.Collapsed;
            SabotageFoodSix.Visibility = Visibility.Collapsed;
            SabotageIndustrySix.Visibility = Visibility.Collapsed;
        }

        private void OnCreditsZeroClick(object sender, RoutedEventArgs e) // we are using attacking spy civ as peramiter here in Creidt only so far
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero); // blame);
            Close_0_SabotageButtons();
        }
        private void OnCreditsOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            Close_1_SabotageButtons();
        }
        private void OnCreditsTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            Close_2_SabotageButtons();
        }
        private void OnCreditsThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            Close_3_SabotageButtons();
        }
        private void OnCreditsFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            Close_4_SabotageButtons();
        }
        private void OnCreditsFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            Close_5_SabotageButtons();
        }
        private void OnCreditsSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            Close_6_SabotageButtons();
        }
        private void OnResearchZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            Close_0_SabotageButtons();
        }
        private void OnResearchOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            Close_1_SabotageButtons();
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            Close_2_SabotageButtons();
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            Close_3_SabotageButtons();
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            Close_4_SabotageButtons();
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            Close_5_SabotageButtons();
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            Close_6_SabotageButtons();
        }
        private void OnEnergyZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero); //, out removedEnergyFacilities);
            Close_0_SabotageButtons();
        }
        private void OnEnergyOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne); //, out removedEnergyFacilities);
            Close_1_SabotageButtons();
        }
        private void OnEnergyTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            Close_2_SabotageButtons();
        }
        private void OnEnergyThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            Close_3_SabotageButtons();
        }
        private void OnEnergyFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            Close_4_SabotageButtons();
        }
        private void OnEnergyFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            Close_5_SabotageButtons();
        }
        private void OnEnergySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            Close_6_SabotageButtons();
        }
        private void OnFoodZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            Close_0_SabotageButtons();
        }
        private void OnFoodOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            Close_1_SabotageButtons();
        }
        private void OnFoodTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            Close_2_SabotageButtons();
        }
        private void OnFoodThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            Close_3_SabotageButtons();
        }
        private void OnFoodFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            Close_4_SabotageButtons();
        }
        private void OnFoodFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            Close_5_SabotageButtons();
        }
        private void OnFoodSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            Close_6_SabotageButtons();
        }
        private void OnIndustryZeroClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedZeroCiv, _blameWhoZero);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedZeroCiv = {1}",
                AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.SpiedZeroCiv.Key);
            Close_0_SabotageButtons();
        }
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedOneCiv, _blameWhoOne);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedOneCiv = {1}", AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.SpiedOneCiv.Key);
            Close_1_SabotageButtons();
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedTwoCiv, _blameWhoTwo);
            Close_2_SabotageButtons();
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedThreeCiv, _blameWhoThree);
            Close_3_SabotageButtons();
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFourCiv, _blameWhoFour);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedFourCiv = {1}", AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.SpiedFourCiv.Key);
            Close_4_SabotageButtons();
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedFiveCiv, _blameWhoFive);
            Close_5_SabotageButtons();
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.SpiedSixCiv, _blameWhoSix);
            Close_6_SabotageButtons();
        }
        #endregion
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            IAppContext appContext = sender as IAppContext;

            if (appContext == null)
                return false;

            PropertyChangedEventArgs propertyChangedEventArgs = e as PropertyChangedEventArgs;
            if (propertyChangedEventArgs == null)
                return false;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LocalPlayerEmpire":
                    OnLocalPlayerEmpireChanged();
                    break;
            }
            GameLog.Client.UI.DebugFormat("AssetsScreen receives sender=(whole GameContext)");  // sender.ToString doesn't work
            return true;
        }
    }
}