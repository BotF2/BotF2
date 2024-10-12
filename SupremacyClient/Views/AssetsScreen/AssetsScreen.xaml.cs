﻿//File: AssetsScreen.xaml.cs
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
        private readonly CivilizationManager _localCivManager;
        //private IntelUpdate _update;

        // order dictionary is located in IntelOrders.cs constructor, store orders in core of host?

        private string _blameWho_0 = "No one is blamed";
        private string _blameWho_1 = "No one is blamed";
        private string _blameWho_2 = "No one is blamed";
        private string _blameWho_3 = "No one is blamed";
        private string _blameWho_4 = "No one is blamed";
        private string _blameWho_5 = "No one is blamed";
        private string _blameWho_6 = "No one is blamed";

//#pragma warning disable IDE0044 // Add readonly modifier
        private RadioButton[] _radioButton_0;

        private RadioButton[] _radioButton_1;
        private RadioButton[] _radioButton_2;
        private RadioButton[] _radioButton_3;
        private RadioButton[] _radioButton_4;
        private RadioButton[] _radioButton_5;
        private RadioButton[] _radioButton_6;
//#pragma warning restore IDE0044 // Add readonly modifier

#pragma warning disable IDE0052 // Remove unread private members
        readonly Civilization _spiedCiv_0 = DesignTimeObjects.SpiedCiv_0.Civilization;

        readonly Civilization _spiedCiv_1 = DesignTimeObjects.SpiedCiv_1.Civilization;
        readonly Civilization _spiedCiv_2 = DesignTimeObjects.SpiedCiv_2.Civilization;
        readonly Civilization _spiedCiv_3 = DesignTimeObjects.SpiedCiv_3.Civilization;
        readonly Civilization _spiedCiv_4 = DesignTimeObjects.SpiedCiv_4.Civilization;
        readonly Civilization _spiedCiv_5 = DesignTimeObjects.SpiedCiv_5.Civilization;
        readonly Civilization _spiedCiv_6 = DesignTimeObjects.SpiedCiv_6.Civilization;
#pragma warning restore IDE0052 // Remove unread private members

        protected int _totalIntelligenceProduction;
        protected int _totalIntelligenceDefenseAccumulated;
        protected int _totalIntelligenceAttackingAccumulated;


        #region Properties for AssestsScreen

        public Meter UpdateAttackingAccumulated(Civilization attackingCiv)
        {
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            _ = int.TryParse(attackMeter.CurrentValue.ToString(), out int newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            return attackMeter;
        }
        protected virtual void FillUpDefense()
        {
            CivilizationManager civ = GameContext.Current.CivilizationManagers[DesignTimeObjects.CivilizationManager.Civilization];
            _ = civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
            civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
            _ = civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
            civ.TotalIntelligenceDefenseAccumulated.UpdateAndReset();
            //OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            //OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            //OnPropertyChanged("TotalIntelligenceProduction");

        }
        #endregion 
        public AssetsScreen([NotNull] IUnityContainer container)
        {
            GameLog.Client.UIDetails.DebugFormat("AssetsScreen - InitializeComponent();");
            _container = container ?? throw new ArgumentNullException("container");
            _appContext = _container.Resolve<IAppContext>();
            _localCivManager = _appContext.LocalPlayerEmpire;
            InitializeComponent();
            PropertyChangedEventManager.AddListener(_appContext, this, "LocalPlayerEmpire");
            _ = IntelHelper.GetLocalCiv(_localCivManager);
            // ClientEvents.IntelUpdateReceived.Subscribe(OnIntelUpdateReceived, ThreadOption.UIThread);
            // DataTemplate itemTemplate = TryFindResource("AssetsTreeItemTemplate") as DataTemplate;

            //for (int i = 0; i < this.; i++)
            //{

            //}


            IsVisibleChanged += OnIsVisibleChanged;

            _radioButton_0 = new RadioButton[] { BlameNoOne0, Terrorists0, Federation0, TerranEmpire0, Romulans0, Klingons0, Cardassians0, Dominion0, Borg0 };
            //just put them in the order so you can use item 1,2,3,4
            for (int i = 0; i < _radioButton_0.Length; i++)
            {
                _radioButton_0[i].Tag = i; //set your item number into tag property here (1,2,3,4)
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButton_0[i].Name);
            }
            _radioButton_1 = new RadioButton[] { BlameNoOne1, Terrorists1, Federation1, TerranEmpire1, Romulans1, Klingons1, Cardassians1, Dominion1, Borg1 };
            //just put them in the order so you can use Critera 1,2,3,4
            for (int i = 0; i < _radioButton_1.Length; i++)
            {
                _radioButton_1[i].Tag = i;
                //GameLog.Client.UI.DebugFormat("radio button loaded into array {0}", _radioButton[i].Name);
            }
            _radioButton_2 = new RadioButton[] { BlameNoOne2, Terrorists2, Federation2, TerranEmpire2, Romulans2, Klingons2, Cardassians2, Dominion2, Borg2 };
            for (int i = 0; i < _radioButton_2.Length; i++)
            {
                _radioButton_2[i].Tag = i;
            }
            _radioButton_3 = new RadioButton[] { BlameNoOne3, Terrorists3, Federation3, TerranEmpire3, Romulans3, Klingons3, Cardassians3, Dominion3, Borg3 };
            for (int i = 0; i < _radioButton_3.Length; i++)
            {
                _radioButton_3[i].Tag = i;
            }
            _radioButton_4 = new RadioButton[] { BlameNoOne4, Terrorists4, Federation4, TerranEmpire4, Romulans4, Klingons4, Cardassians4, Dominion4, Borg4 };
            for (int i = 0; i < _radioButton_4.Length; i++)
            {
                _radioButton_4[i].Tag = i;
            }
            _radioButton_5 = new RadioButton[] { BlameNoOne5, Terrorists5, Federation5, TerranEmpire5, Romulans5, Klingons5, Cardassians5, Dominion5, Borg5 };
            for (int i = 0; i < _radioButton_5.Length; i++)
            {
                _radioButton_5[i].Tag = i;
            }
            _radioButton_6 = new RadioButton[] { BlameNoOne6, Terrorists6, Federation6, TerranEmpire6, Romulans6, Klingons6, Cardassians6, Dominion6, Borg6 };
            for (int i = 0; i < _radioButton_6.Length; i++)
            {
                _radioButton_6[i].Tag = i;
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
            {
                return;
            }

            CivilizationManager localPlayerEmpire = _appContext.LocalPlayerEmpire;
            //works  GameLog.Client.UI.DebugFormat("AssetsScreen local player ={0}", localPlayerEmpire.Civilization.Key);
            if (localPlayerEmpire == null)
            {
                return;
            }
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            Civilization _civLocalPlayer = _appContext.LocalPlayer.Empire;

            if (IsVisible)
            {
                //ResumeAnimations();
                GameLog.Client.UIDetails.DebugFormat("*********** begin of checking visible ***********");

                // GameLog.Client.UI.DebugFormat("Spied_0_Civ checking visible .... _spiedCiv_1 = {0}, _civLocalPlayer = {1}", _spiedCiv_0, _civLocalPlayer);
                if (AssetsHelper.IsSpied_0_(_civLocalPlayer) || IntelHelper.ShowNetwork_0)
                {
                    EmpireExpander_0.Visibility = Visibility.Visible;
                    SabotageEnergyZero.Visibility = Visibility.Visible;
                    SabotageFoodZero.Visibility = Visibility.Visible;
                    SabotageIndustryZero.Visibility = Visibility.Visible;
                    StealResearchZero.Visibility = Visibility.Visible;
                    StealCreditsZero.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("Spied_1_Civ checking visible .... _spiedCiv_1 = {0}, _civLocalPlayer = {1}", _spiedCiv_1, _civLocalPlayer);
                if (AssetsHelper.IsSpied_1_(_civLocalPlayer) || IntelHelper.ShowNetwork_1)
                {
                    EmpireExpander_1.Visibility = Visibility.Visible;
                    SabotageEnergyOne.Visibility = Visibility.Visible;
                    SabotageFoodOne.Visibility = Visibility.Visible;
                    SabotageIndustryOne.Visibility = Visibility.Visible;
                    StealResearchOne.Visibility = Visibility.Visible;
                    StealCreditsOne.Visibility = Visibility.Visible;
                }
                // GameLog.Client.UI.DebugFormat("Spied_2_Civ checking visible .... _spiedCiv_2 = {0}, _civLocalPlayer = {1}", _spiedCiv_2, _civLocalPlayer);
                if (AssetsHelper.IsSpied_2_(_civLocalPlayer) || IntelHelper.ShowNetwork_2)
                {
                    EmpireExpander_2.Visibility = Visibility.Visible;
                    SabotageEnergyTwo.Visibility = Visibility.Visible;
                    SabotageFoodTwo.Visibility = Visibility.Visible;
                    SabotageIndustryTwo.Visibility = Visibility.Visible;
                    StealResearchTwo.Visibility = Visibility.Visible;
                    StealCreditsTwo.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("Spied_3_Civ checking visible .... _spiedCiv_3 = {0}, _civLocalPlayer = {1}", _spiedCiv_3, _civLocalPlayer);
                if (AssetsHelper.IsSpied_3_(_civLocalPlayer) || IntelHelper.ShowNetwork_3)
                {
                    EmpireExpander_3.Visibility = Visibility.Visible;
                    SabotageEnergyThree.Visibility = Visibility.Visible;
                    SabotageFoodThree.Visibility = Visibility.Visible;
                    SabotageIndustryThree.Visibility = Visibility.Visible;
                    StealResearchThree.Visibility = Visibility.Visible;
                    StealCreditsThree.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("SpiedCiv cFourhecking visible .... _spiedCiv_4 = {0}, _civLocalPlayer = {1}", _spiedCiv_4, _civLocalPlayer);
                if (AssetsHelper.IsSpied_4_(_civLocalPlayer) || IntelHelper.ShowNetwork_4)
                {
                    EmpireExpander_4.Visibility = Visibility.Visible;
                    SabotageEnergyFour.Visibility = Visibility.Visible;
                    SabotageFoodFour.Visibility = Visibility.Visible;
                    SabotageIndustryFour.Visibility = Visibility.Visible;
                    StealResearchFour.Visibility = Visibility.Visible;
                    StealCreditsFour.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("Spied_5_Civ checking visible .... _spiedCiv_5 = {0}, _civLocalPlayer = {1}", _spiedCiv_5, _civLocalPlayer);
                if (AssetsHelper.IsSpied_5_(_civLocalPlayer) || IntelHelper.ShowNetwork_5)
                {
                    EmpireExpander_5.Visibility = Visibility.Visible;
                    SabotageEnergyFive.Visibility = Visibility.Visible;
                    SabotageFoodFive.Visibility = Visibility.Visible;
                    SabotageIndustryFive.Visibility = Visibility.Visible;
                    StealResearchFive.Visibility = Visibility.Visible;
                    StealCreditsFive.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("Spied_6_Civ checking visible .... _spiedCiv_6 = {0}, _civLocalPlayer = {1}", _spiedCiv_6, _civLocalPlayer);
                if (AssetsHelper.IsSpied_6_(_civLocalPlayer) || IntelHelper.ShowNetwork_6)
                {
                    EmpireExpander_6.Visibility = Visibility.Visible;
                    SabotageEnergySix.Visibility = Visibility.Visible;
                    SabotageFoodSix.Visibility = Visibility.Visible;
                    SabotageIndustrySix.Visibility = Visibility.Visible;
                    StealResearchSix.Visibility = Visibility.Visible;
                    StealCreditsSix.Visibility = Visibility.Visible;
                }
                //GameLog.Client.UI.DebugFormat("end  of checking visible");

                // GameLog.Client.UI.DebugFormat("_civLocalPlayer = {0}", _civLocalPlayer.Key);

                Diplomat diplomat1 = Diplomat.Get(GameContext.Current.CivilizationManagers[_civLocalPlayer.CivID]);
                int empireCount = GameContext.Current.Civilizations.Count(o => o.IsEmpire);
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
                    {
                        continue;
                    }

                    ForeignPower ForeignPower = diplomat1.GetForeignPower(GameContext.Current.CivilizationManagers[empireID]);
                    bool _checkedVisibleForSabotagePending = true;

                    _checkedVisibleForSabotagePending = CheckingVisibityForSabotagePending(diplomat1, ForeignPower);

                    //if (ForeignPower.LastStatementSent != null)
                    if (diplomat1.GetLastStatementSent(ForeignPower) != null)
                    {
                        int _statementSentInTurn = diplomat1.GetLastStatementSent(ForeignPower).TurnSent;

                        if (_statementSentInTurn == 99999)
                        {
                            _statementSentInTurn = 1;
                        }
                        //switch (ForeignPower.LastStatementSent.StatementType)
                        if (GameContext.Current.TurnNumber < _statementSentInTurn + 2)
                        {
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
                    }
                    //_checkedVisibleForSabotagePending = _visibleForSabotagePending;

                    // just for testing      _checkedVisibleForSabotagePending = true;
                    if (_checkedVisibleForSabotagePending == false)
                    {
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
            {
                PauseAnimations();
            }
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
            get => _isActive;
            set
            {
                if (value == _isActive)
                {
                    return;
                }

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
            get => DataContext as AssetsScreenPresentationModel;
            set => DataContext = value;
        }

        public void OnCreated() { }

        public void OnDestroyed()
        {
            StopAnimations();
        }
        #endregion
        #region OnButtonClicks
        private void OnBlameButtons_0_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne0.IsChecked == true)
                {
                    _blameWho_0 = "No one";
                }
                if (Terrorists0.IsChecked == true)
                {
                    _blameWho_0 = "Terrorists";
                }
                if (Federation0.IsChecked == true)
                {
                    _blameWho_0 = "Federation";
                }
                if (TerranEmpire0.IsChecked == true)
                {
                    _blameWho_0 = "TerranEmpire";
                }
                if (Romulans0.IsChecked == true)
                {
                    _blameWho_0 = "Romulans";
                }
                if (Klingons0.IsChecked == true)
                {
                    _blameWho_0 = "Klingons";
                }
                if (Cardassians0.IsChecked == true)
                {
                    _blameWho_0 = "Cardassians";
                }
                if (Dominion0.IsChecked == true)
                {
                    _blameWho_0 = "Dominion";
                }
                if (Borg0.IsChecked == true)
                {
                    _blameWho_0 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Zero %$%$###$%$$#@ Blame Sting ={0}", _blameWho_0);
            }
        }
        private void OnBlameButtons_1_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne1.IsChecked == true)
                {
                    _blameWho_1 = "No one";
                }
                if (Terrorists1.IsChecked == true)
                {
                    _blameWho_1 = "Terrorists";
                }
                if (Federation1.IsChecked == true)
                {
                    _blameWho_1 = "Federation";
                }
                if (TerranEmpire1.IsChecked == true)
                {
                    _blameWho_1 = "TerranEmpire";
                }
                if (Romulans1.IsChecked == true)
                {
                    _blameWho_1 = "Romulans";
                }
                if (Klingons1.IsChecked == true)
                {
                    _blameWho_1 = "Klingons";
                }
                if (Cardassians1.IsChecked == true)
                {
                    _blameWho_1 = "Cardassians";
                }
                if (Dominion1.IsChecked == true)
                {
                    _blameWho_1 = "Dominion";
                }
                if (Borg1.IsChecked == true)
                {
                    _blameWho_1 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander One %$%$###$%$$#@ Blame Sting ={0}", _blameWho_1);
            }
        }

        private void OnBlameButtons_2_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne2.IsChecked == true)
                {
                    _blameWho_2 = "No one";
                }
                if (Terrorists2.IsChecked == true)
                {
                    _blameWho_2 = "Terrorists";
                }
                if (Federation2.IsChecked == true)
                {
                    _blameWho_2 = "Federation";
                }
                if (TerranEmpire2.IsChecked == true)
                {
                    _blameWho_2 = "TerranEmpire";
                }
                if (Romulans2.IsChecked == true)
                {
                    _blameWho_2 = "Romulans";
                }
                if (Klingons2.IsChecked == true)
                {
                    _blameWho_2 = "Klingons";
                }
                if (Cardassians2.IsChecked == true)
                {
                    _blameWho_2 = "Cardassians";
                }
                if (Dominion2.IsChecked == true)
                {
                    _blameWho_2 = "Dominion";
                }
                if (Borg2.IsChecked == true)
                {
                    _blameWho_2 = "Borg";
                }
                // GameLog.Client.UI.DebugFormat("Expander Two ############### Blame Sting ={0}", _blameWho_2);
            }
        }
        private void OnBlameButtons_3_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne3.IsChecked == true)
                {
                    _blameWho_3 = "No one";
                }
                if (Terrorists3.IsChecked == true)
                {
                    _blameWho_3 = "Terrorists";
                }
                if (Federation3.IsChecked == true)
                {
                    _blameWho_3 = "Federation";
                }
                if (TerranEmpire3.IsChecked == true)
                {
                    _blameWho_3 = "TerranEmpire";
                }
                if (Romulans3.IsChecked == true)
                {
                    _blameWho_3 = "Romulans";
                }
                if (Klingons3.IsChecked == true)
                {
                    _blameWho_3 = "Klingons";
                }
                if (Cardassians3.IsChecked == true)
                {
                    _blameWho_3 = "Cardassians";
                }
                if (Dominion3.IsChecked == true)
                {
                    _blameWho_3 = "Dominion";
                }
                if (Borg3.IsChecked == true)
                {
                    _blameWho_3 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Three ############### Blame Sting ={0}", _blameWho_3);
            }
        }
        private void OnBlameButtons_4_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne4.IsChecked == true)
                {
                    _blameWho_4 = "No one";
                }
                if (Terrorists4.IsChecked == true)
                {
                    _blameWho_4 = "Terrorists";
                }
                if (Federation4.IsChecked == true)
                {
                    _blameWho_4 = "Federation";
                }
                if (TerranEmpire4.IsChecked == true)
                {
                    _blameWho_4 = "TerranEmpire";
                }
                if (Romulans4.IsChecked == true)
                {
                    _blameWho_4 = "Romulans";
                }
                if (Klingons4.IsChecked == true)
                {
                    _blameWho_4 = "Klingons";
                }
                if (Cardassians4.IsChecked == true)
                {
                    _blameWho_4 = "Cardassians";
                }
                if (Dominion4.IsChecked == true)
                {
                    _blameWho_4 = "Dominion";
                }
                if (Borg4.IsChecked == true)
                {
                    _blameWho_4 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Four ############### Blame Sting ={0}", _blameWho_4);
            }
        }
        private void OnBlameButtons_5_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne5.IsChecked == true)
                {
                    _blameWho_5 = "No one";
                }
                if (Terrorists5.IsChecked == true)
                {
                    _blameWho_5 = "Terrorists";
                }
                if (Federation5.IsChecked == true)
                {
                    _blameWho_5 = "Federation";
                }
                if (TerranEmpire5.IsChecked == true)
                {
                    _blameWho_5 = "TerranEmpire";
                }
                if (Romulans5.IsChecked == true)
                {
                    _blameWho_5 = "Romulans";
                }
                if (Klingons5.IsChecked == true)
                {
                    _blameWho_5 = "Klingons";
                }
                if (Cardassians5.IsChecked == true)
                {
                    _blameWho_5 = "Cardassians";
                }
                if (Dominion5.IsChecked == true)
                {
                    _blameWho_5 = "Dominion";
                }
                if (Borg5.IsChecked == true)
                {
                    _blameWho_5 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Five ############### Blame Sting ={0}", _blameWho_5);
            }
        }
        private void OnBlameButtons_6_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                if (BlameNoOne6.IsChecked == true)
                {
                    _blameWho_6 = "No one";
                }
                if (Terrorists6.IsChecked == true)
                {
                    _blameWho_6 = "Terrorists";
                }
                if (Federation6.IsChecked == true)
                {
                    _blameWho_6 = "Federation";
                }
                if (TerranEmpire6.IsChecked == true)
                {
                    _blameWho_6 = "TerranEmpire";
                }
                if (Romulans6.IsChecked == true)
                {
                    _blameWho_6 = "Romulans";
                }
                if (Klingons6.IsChecked == true)
                {
                    _blameWho_6 = "Klingons";
                }
                if (Cardassians6.IsChecked == true)
                {
                    _blameWho_6 = "Cardassians";
                }
                if (Dominion6.IsChecked == true)
                {
                    _blameWho_6 = "Dominion";
                }
                if (Borg6.IsChecked == true)
                {
                    _blameWho_6 = "Borg";
                }
                //GameLog.Client.UI.DebugFormat("Expander Six ############### Blame Sting ={0}", _blameWho_6);
            }
        }
        private void Close_0_SabotageButtons()
        {
            StealCreditsZero.Visibility = Visibility.Collapsed;
            StealResearchZero.Visibility = Visibility.Collapsed;
            SabotageEnergyZero.Visibility = Visibility.Collapsed;
            SabotageFoodZero.Visibility = Visibility.Collapsed;
            SabotageIndustryZero.Visibility = Visibility.Collapsed;
            //EmpireExpander_0.IsExpanded = false;
            EmpireExpander_0.IsExpanded = true;
        }
        private void Close_1_SabotageButtons()
        {
            StealCreditsOne.Visibility = Visibility.Collapsed;
            StealResearchOne.Visibility = Visibility.Collapsed;
            SabotageEnergyOne.Visibility = Visibility.Collapsed;
            SabotageFoodOne.Visibility = Visibility.Collapsed;
            SabotageIndustryOne.Visibility = Visibility.Collapsed;
            //EmpireExpander_1.IsExpanded = false;
            EmpireExpander_1.IsExpanded = true;
        }
        private void Close_2_SabotageButtons()
        {
            StealCreditsTwo.Visibility = Visibility.Collapsed;
            StealResearchTwo.Visibility = Visibility.Collapsed;
            SabotageEnergyTwo.Visibility = Visibility.Collapsed;
            SabotageFoodTwo.Visibility = Visibility.Collapsed;
            SabotageIndustryTwo.Visibility = Visibility.Collapsed;
            //EmpireExpander_2.IsExpanded = false;
            EmpireExpander_2.IsExpanded = true;
        }
        private void Close_3_SabotageButtons()
        {
            StealCreditsThree.Visibility = Visibility.Collapsed;
            StealResearchThree.Visibility = Visibility.Collapsed;
            SabotageEnergyThree.Visibility = Visibility.Collapsed;
            SabotageFoodThree.Visibility = Visibility.Collapsed;
            SabotageIndustryThree.Visibility = Visibility.Collapsed;
            //EmpireExpander_3.IsExpanded = false;
            EmpireExpander_3.IsExpanded = true;
        }
        private void Close_4_SabotageButtons()
        {
            StealCreditsFour.Visibility = Visibility.Collapsed;
            StealResearchFour.Visibility = Visibility.Collapsed;
            SabotageEnergyFour.Visibility = Visibility.Collapsed;
            SabotageFoodFour.Visibility = Visibility.Collapsed;
            SabotageIndustryFour.Visibility = Visibility.Collapsed;
            //EmpireExpander_4.IsExpanded = false;
            EmpireExpander_4.IsExpanded = true;
        }
        private void Close_5_SabotageButtons()
        {
            StealCreditsFive.Visibility = Visibility.Collapsed;
            StealResearchFive.Visibility = Visibility.Collapsed;
            SabotageEnergyFive.Visibility = Visibility.Collapsed;
            SabotageFoodFive.Visibility = Visibility.Collapsed;
            SabotageIndustryFive.Visibility = Visibility.Collapsed;
            //EmpireExpander_5.IsExpanded = false;
            EmpireExpander_5.IsExpanded = true;
        }
        private void Close_6_SabotageButtons()
        {
            StealCreditsSix.Visibility = Visibility.Collapsed;
            StealResearchSix.Visibility = Visibility.Collapsed;
            SabotageEnergySix.Visibility = Visibility.Collapsed;
            SabotageFoodSix.Visibility = Visibility.Collapsed;
            SabotageIndustrySix.Visibility = Visibility.Collapsed;
            //EmpireExpander_6.IsExpanded = false;
            EmpireExpander_6.IsExpanded = true;
        }

        private void OnCredits_0_Click(object sender, RoutedEventArgs e) // we are using attacking spy civ as peramiter here in Creidt only so far
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_0_Civ, _blameWho_0); // blame);
            Close_0_SabotageButtons();
        }
        private void OnCredits_1_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_1_Civ, _blameWho_1);
            Close_1_SabotageButtons();
        }
        private void OnCredits_2_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_2_Civ, _blameWho_2);
            Close_2_SabotageButtons();
        }
        private void OnCredits_3_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_3_Civ, _blameWho_3);
            Close_3_SabotageButtons();
        }
        private void OnCredits_4_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_4_Civ, _blameWho_4);
            Close_4_SabotageButtons();
        }
        private void OnCredits_5_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_5_Civ, _blameWho_5);
            Close_5_SabotageButtons();
        }
        private void OnCredits_6_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealCredits(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_6_Civ, _blameWho_6);
            Close_6_SabotageButtons();
        }
        private void OnResearch_0_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_0_Civ, _blameWho_0);
            Close_0_SabotageButtons();
        }
        private void OnResearch_1_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_1_Civ, _blameWho_1);
            Close_1_SabotageButtons();
        }
        private void OnResearch_2_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_2_Civ, _blameWho_2);
            Close_2_SabotageButtons();
        }
        private void OnResearch_3_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_3_Civ, _blameWho_3);
            Close_3_SabotageButtons();
        }
        private void OnResearch_4_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_4_Civ, _blameWho_4);
            Close_4_SabotageButtons();
        }
        private void OnResearch_5_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_5_Civ, _blameWho_5);
            Close_5_SabotageButtons();
        }
        private void OnResearch_6_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageStealResearch(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_6_Civ, _blameWho_6);
            Close_6_SabotageButtons();
        }
        private void OnEnergy_0_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_0_Civ, _blameWho_0); //, out removedEnergyFacilities);
            Close_0_SabotageButtons();
        }
        private void OnEnergy_1_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_1_Civ, _blameWho_1); //, out removedEnergyFacilities);
            Close_1_SabotageButtons();
        }
        private void OnEnergy_2_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_2_Civ, _blameWho_2);
            Close_2_SabotageButtons();
        }
        private void OnEnergy_3_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_3_Civ, _blameWho_3);
            Close_3_SabotageButtons();
        }
        private void OnEnergy_4_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_4_Civ, _blameWho_4);
            Close_4_SabotageButtons();
        }
        private void OnEnergy_5_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_5_Civ, _blameWho_5);
            Close_5_SabotageButtons();
        }
        private void OnEnergy_6_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_6_Civ, _blameWho_6);
            Close_6_SabotageButtons();
        }
        private void OnFood_0_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_0_Civ, _blameWho_0);
            Close_0_SabotageButtons();
        }
        private void OnFood_1_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_1_Civ, _blameWho_1);
            Close_1_SabotageButtons();
        }
        private void OnFood_2_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_2_Civ, _blameWho_2);
            Close_2_SabotageButtons();
        }
        private void OnFood_3_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_3_Civ, _blameWho_3);
            Close_3_SabotageButtons();
        }
        private void OnFood_4_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_4_Civ, _blameWho_4);
            Close_4_SabotageButtons();
        }
        private void OnFood_5_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_5_Civ, _blameWho_5);
            Close_5_SabotageButtons();
        }
        private void OnFood_6_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_6_Civ, _blameWho_6);
            Close_6_SabotageButtons();
        }
        private void OnIndustry_0_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_0_Civ, _blameWho_0);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedZeroCiv = {1}",
                AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.Spied_0_Civ.Key);
            Close_0_SabotageButtons();
        }
        private void OnIndustry_1_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_1_Civ, _blameWho_1);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedOneCiv = {1}", AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.Spied_1_Civ.Key);
            Close_1_SabotageButtons();
        }
        private void OnIndustry_2_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_2_Civ, _blameWho_2);
            Close_2_SabotageButtons();
        }
        private void OnIndustry_3_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_3_Civ, _blameWho_3);
            Close_3_SabotageButtons();
        }
        private void OnIndustry_4_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_4_Civ, _blameWho_4);
            GameLog.Client.Intel.DebugFormat("LocalCiv ={0} spiedFourCiv = {1}", AssetsScreenPresentationModel.LocalCiv.Key, AssetsScreenPresentationModel.Spied_4_Civ.Key);
            Close_4_SabotageButtons();
        }
        private void OnIndustry_5_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_5_Civ, _blameWho_5);
            Close_5_SabotageButtons();
        }
        private void OnIndustry_6_Click(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(AssetsScreenPresentationModel.LocalCiv, AssetsScreenPresentationModel.Spied_6_Civ, _blameWho_6);
            Close_6_SabotageButtons();
        }
        #endregion
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(sender is IAppContext))
            {
                return false;
            }

            if (!(e is PropertyChangedEventArgs propertyChangedEventArgs))
            {
                return false;
            }

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LocalPlayerEmpire":
                    OnLocalPlayerEmpireChanged();
                    break;
            }
            GameLog.Client.UIDetails.DebugFormat("AssetsScreen receives sender=(whole GameContext)");  // sender.ToString doesn't work
            return true;
        }
    }
}