// File:IntelHelper.cs
using Microsoft.Practices.ServiceLocation;
using Supremacy.Client;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using Supremacy.Orbitals;
using System.Linq;

namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static Civilization _newSpyCiv;
        private static List<Civilization> _spiedList = new List<Civilization>();
        //private static List<Civilization> _localSpiedList = new List<Civilization>();
        private static int _defenseAccumulatedIntelInt;
        private static int _attackAccumulatedIntelInt;
        public static List<Civilization> _spyingCiv_0_List;
        public static List<Civilization> _spyingCiv_1_List;
        public static List<Civilization> _spyingCiv_2_List;
        public static List<Civilization> _spyingCiv_3_List;
        public static List<Civilization> _spyingCiv_4_List;
        public static List<Civilization> _spyingCiv_5_List;
        public static List<Civilization> _spyingCiv_6_List;
        public static Dictionary<Civilization, string> _blamedCiv;

        public static bool _showNetwork_0 = false;
        public static bool _showNetwork_1 = false;
        public static bool _showNetwork_2 = false;
        public static bool _showNetwork_3 = false;
        public static bool _showNetwork_4 = false;
        public static bool _showNetwork_5 = false;
        public static bool _showNetwork_6 = false;

        public static string Attack_ED_stuff_before = "";
        public static string Attack_ED_stuff_after = "";
        public static string Attack_ING_stuff_before = "";
        public static string Attack_ING_stuff_after = "";
        public static string Attack_ED_IntelPoints_before = "";
        public static string Attack_ED_IntelPoints_after = "";
        public static string Attack_ING_IntelPoints_before = "";
        public static string Attack_ING_IntelPoints_after = "";
        public static string Attack_ED_IntelPointCosts = "";
        public static string Attack_ING_IntelPointCosts = "";

        public static List<SitRepEntry> SitReps_Temp { get; set; } = new List<SitRepEntry>();
        public static UniverseObjectList<Colony> NewSpiedColonies { get; private set; }
        public static Civilization NewSpyCiv => _newSpyCiv;
        public static Civilization NewTargetCiv { get; private set; }

        public static CivilizationManager LocalCivManager { get; private set; }
        public static int DefenseAccumulatedInteInt
        {
            get
            {
                _defenseAccumulatedIntelInt = GameContext.Current.CivilizationManagers[LocalCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue;
                return _defenseAccumulatedIntelInt;
            }
        }
        public static int AttackingAccumulatedInteInt
        {
            get
            {
                _attackAccumulatedIntelInt = GameContext.Current.CivilizationManagers[LocalCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue;
                return _attackAccumulatedIntelInt;
            }
        }
        public static bool ShowNetwork_0 => _showNetwork_0;
        public static bool ShowNetwork_1 => _showNetwork_1;
        public static bool ShowNetwork_2 => _showNetwork_2;
        public static bool ShowNetwork_3 => _showNetwork_3;
        public static bool ShowNetwork_4 => _showNetwork_4;
        public static bool ShowNetwork_5 => _showNetwork_5;
        public static bool ShowNetwork_6 => _showNetwork_6;

        public static List<Civilization> SpyingCiv_0_List => _spyingCiv_0_List;
        public static List<Civilization> SpyingCiv_1_List => _spyingCiv_1_List;
        public static List<Civilization> SpyingCiv_2_List => _spyingCiv_2_List;
        public static List<Civilization> SpyingCiv_3_List => _spyingCiv_3_List;
        public static List<Civilization> SpyingCiv_4_List => _spyingCiv_4_List;
        public static List<Civilization> SpyingCiv_5_List => _spyingCiv_5_List;

        /// <summary>
        /// Using the civ manager as a param from AssetsScreen. Hope this is the local machine local player
        /// </summary>
        /// <param name="civManager"></param>
        /// <returns></returns>
        public static CivilizationManager GetLocalCiv(CivilizationManager civManager)
        {
            LocalCivManager = civManager;
            return civManager;
        }

        public static void ShowSpyNetwork(Civilization civ)
        {
            switch (civ.CivID)
            {
                case 0:
                    _showNetwork_0 = true;
                    break;
                case 1:
                    _showNetwork_1 = true;
                    break;
                case 2:
                    _showNetwork_2 = true;
                    break;
                case 3:
                    _showNetwork_3 = true;
                    break;
                case 4:
                    _showNetwork_4 = true;
                    break;
                case 5:
                    _showNetwork_5 = true;
                    break;
                case 6:
                    _showNetwork_6 = true;
                    break;
            }
        }
        public static void SendXSpiedY(Civilization spyCiv, Civilization spiedCiv, UniverseObjectList<Colony> colonies)
        {
            GameLog.Core.UI.DebugFormat("**** New spyciv = {0} spying on = {1}", spyCiv.Key, spiedCiv.Key);
            if (spyCiv == null)
            {
                throw new ArgumentNullException("spyCiv");
            }

            if (spiedCiv == null)
            {
                throw new ArgumentNullException("spiedCiv");
            }

            if (LocalCivManager.Civilization == spyCiv)
            {
                ShowSpyNetwork(spiedCiv);
            }

            _spiedList.Clear();
            _newSpyCiv = spyCiv;
            NewTargetCiv = spiedCiv;
            NewSpiedColonies = colonies;
            List<Civilization> newList = new List<Civilization> { spiedCiv };
            switch (spyCiv.CivID)
            {
                case 0:
                    if (_spyingCiv_0_List == null)
                    {
                        _spyingCiv_0_List = newList;
                    }
                    else
                    {
                        _spyingCiv_0_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_0_List);
                    break;
                case 1:
                    if (_spyingCiv_1_List == null)
                    {
                        _spyingCiv_1_List = newList;
                    }
                    else
                    {
                        _spyingCiv_1_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_1_List);
                    break;
                case 2:
                    if (_spyingCiv_2_List == null)
                    {
                        _spyingCiv_2_List = newList;
                    }
                    else
                    {
                        _spyingCiv_2_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_2_List);
                    break;
                case 3:
                    if (_spyingCiv_3_List == null)
                    {
                        _spyingCiv_3_List = newList;
                    }
                    else
                    {
                        _spyingCiv_3_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_3_List);
                    break;
                case 4:
                    if (_spyingCiv_4_List == null)
                    {
                        _spyingCiv_4_List = newList;
                    }
                    else
                    {
                        _spyingCiv_4_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_4_List);
                    break;
                case 5:
                    if (_spyingCiv_5_List == null)
                    {
                        _spyingCiv_5_List = newList;
                    }
                    else
                    {
                        _spyingCiv_5_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_5_List);
                    break;
                case 6:
                    if (_spyingCiv_6_List == null)
                    {
                        _spyingCiv_6_List = newList;
                    }
                    else
                    {
                        _spyingCiv_6_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_6_List);
                    break;
            }
            GameLog.Client.UI.DebugFormat("********* end of sending spied list to CM **********");
        }

        #region Espionage Methods
        public static string Blame(Civilization localCivAttacker, Civilization Attacked, string blamed, int chance)
        {
            //GameLog.Client.Diplomacy.DebugFormat("  ");
            if (RandomHelper.Chance(chance))
            {

                IList<Civilization> allContacts = DiplomacyHelper.GetCivilizationsHavingContact(localCivAttacker);
                _ = allContacts.Remove(Attacked);   // attacked civ itself

                List<Civilization> minors = new List<Civilization>();
                foreach (Civilization civ in allContacts)
                {
                    if (!civ.IsEmpire)
                    {
                        minors.Add(civ);
                    }
                }
                foreach (Civilization civ in minors)
                {
                    _ = allContacts.Remove(civ);
                }

                IList<Civilization> otherCivs = allContacts;

                Civilization oneCiv = otherCivs.RandomElementOrDefault();
                Civilization nextCiv = oneCiv ?? localCivAttacker;
                blamed = nextCiv == localCivAttacker
                    ? RandomHelper.Chance(2)
                        ? ResourceManager.GetString("SITREP_SABOTAGE_TERRORISTS")
                        : ResourceManager.GetString("SITREP_SABOTAGE_NO_ONE")
                    : nextCiv.ShortName;
            }
            return blamed;
        }

        public static void SabotageStealCredits(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            // coming from Buttons in each of the six expanders
            //CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            //CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            SendStatementOrder _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.StealCredits, Tone.Enraged, blamed, GameContext.Current.TurnNumber))
            {
                Owner = attackingCiv
            };
            GameLog.Core.DiplomacyDetails.DebugFormat("Create Statement for StealCredits: " + Environment.NewLine
                + "sender = {0} *vs* Recipient = {1}:   StatementType = {2} Tone ={3}, blamed = {4}"
                                , attackingCiv, attackedCiv, _sendOrder.Statement.ToString(), _sendOrder.Statement.Tone.ToString(), blamed + Environment.NewLine);
            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
            _ = ServiceLocator.Current.GetInstance<IPlayerOrderService>().Orders;  // just for Break point controlling
        }


        public static void SabotageStealCreditsExecute(Civilization attackingCiv, Civilization attackedCiv, string blamed, int ratio)
        {

            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Colony colony = attackedCivManager.SeatOfGovernment.Sector.System.Colony;

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int ratioLevel = -1;

            GameLog.Core.Intel.DebugFormat("**** StealCredits, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            if (attackingCiv == null)
            {
                return;
            }

            if (attackedCiv == null)
            {
                return;
            }

            ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            _ = int.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
            {
                defenseIntelligence = 2;
            }


            //Effect of steal // value needed for SitRep

            _ = int.TryParse(attackedCivManager.Credits.ToString(), out int stolenCredits);
            //int attackedCreditsBefore = stolenCredits;

            if (stolenCredits < 100)  // they have not enough credits worth stealing, especially avoid negative stuff !!!
            {
                stolenCredits = -2;
                goto stolenCreditsIsMinusOne;
            }

            if (ratio < 2 || attackMeter.CurrentValue < 10)  // 
            {
                stolenCredits = -1;  // failed
                blamed = Blame(attackingCiv, attackedCiv, blamed, 3);
                goto stolenCreditsIsMinusOne;
            }

            stolenCredits = stolenCredits / 100 * 3;  // default 3 percent

            if (!RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 5)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenCredits *= 3; // 2 percent of their TOTAL Credits - not just income
                blamed = Blame(attackingCiv, attackedCiv, blamed, 4);
                ratioLevel = 1;
            }
            if (ratio > 15 && !RandomHelper.Chance(3) && attackedCivManager.Treasury.CurrentLevel > 20) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                if (!RandomHelper.Chance(2))
                {
                    stolenCredits *= 3;
                    blamed = Blame(attackingCiv, attackedCiv, blamed, 6);
                    ratioLevel = 2;
                }
            }
            if (ratio > 30 && !RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenCredits *= 2;
                blamed = Blame(attackingCiv, attackedCiv, blamed, 10);
                ratioLevel = 3;
            }


            GameLog.Core.Intel.DebugFormat("**** CREDITS, The attack_ING Spy Civ={0} the attack_ED civ={1}", attackingCiv.Key, attackedCiv.Key); // this ouput only...
                                                                                                                                                 // only, if it runs into calculation (not 'not enough credits' or 'not enough attacking power')  

            Attack_ED_stuff_before = GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue.ToString();
            _ = GameContext.Current.CivilizationManagers[attackedCiv].Credits.AdjustCurrent(stolenCredits * -1);
            GameContext.Current.CivilizationManagers[attackedCiv].Credits.UpdateAndReset();
            Attack_ED_stuff_after = GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue.ToString();


            Attack_ING_stuff_before = GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue.ToString();
            _ = GameContext.Current.CivilizationManagers[attackingCiv].Credits.AdjustCurrent(stolenCredits);
            GameContext.Current.CivilizationManagers[attackingCiv].Credits.UpdateAndReset();
            Attack_ING_stuff_after = GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue.ToString();


            // DEFENSE CIV - Intel Points

            Attack_ED_IntelPoints_before = defenseMeter.CurrentValue.ToString();
            _ = defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            Attack_ED_IntelPointCosts = (defenseIntelligence / 4).ToString();
            Attack_ED_IntelPoints_after = defenseMeter.CurrentValue.ToString();



        //  ATTACKING CIV - Intel Points

        stolenCreditsIsMinusOne:;   // pushing buttons makes always 'intel costs'

            Attack_ING_IntelPoints_before = attackMeter.CurrentValue.ToString();
            _ = attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            Attack_ING_IntelPointCosts = (defenseIntelligence / 2).ToString();
            Attack_ING_IntelPoints_after = attackMeter.CurrentValue.ToString();


            string attack_ED_before = "attackED  = " + attackedCiv.Key + " BEFORE: Credits: " + Attack_ED_stuff_before + " stolen= " + stolenCredits + " - IP: "
                + Attack_ED_IntelPoints_before;
            string attack_ED_after = "attackED  = " + attackedCiv.Key + " AFTER : Credits: " + Attack_ED_stuff_after + " stolen= " + stolenCredits + " - IP: "
                + Attack_ED_IntelPoints_after + " IPcosts= " + Attack_ED_IntelPointCosts;
            string attack_ING_before = "attackING = " + attackingCiv.Key + " BEFORE: Credits: " + Attack_ING_stuff_before + " stolen= " + stolenCredits + " - IP: "
                + Attack_ING_IntelPoints_before;
            string attack_ING_after = "attackING = " + attackingCiv.Key + " AFTER : Credits: " + Attack_ING_stuff_after + " stolen= " + stolenCredits + " - IP: "
                + Attack_ING_IntelPoints_after + " IPcosts= " + Attack_ING_IntelPointCosts;

            GameLog.Core.Intel.DebugFormat(attack_ED_before);
            GameLog.Core.Intel.DebugFormat(attack_ED_after);
            GameLog.Core.Intel.DebugFormat(attack_ING_before);
            GameLog.Core.Intel.DebugFormat(attack_ING_after);



            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED");

            _ = int.TryParse(GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue.ToString(), out int newCreditsAttacked);

            GameLog.Core.Intel.DebugFormat("{0}: Stolen Credits from {1}:  >>> {2} Credits, {3} Blamed", attackingCiv.Key, attackedCiv.Key, stolenCredits, blamed);

            // Sitreps   attack*ed* and attack*ing*
            attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                   attackingCiv, attackedCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, ratioLevel));

            attackingCivManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, ratioLevel));
        }

        public static void SabotageStealResearch(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            // coming from Buttons in each of the six expanders
            //CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            //CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            SendStatementOrder _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.StealResearch, Tone.Enraged, blamed, GameContext.Current.TurnNumber))
            {
                Owner = attackingCiv
            };

            GameLog.Core.Diplomacy.DebugFormat("Create Statement for StealResearch: " + Environment.NewLine
                    + "sender = {0} *vs* Recipient = {1}: StatementType = {2}, tone ={3} blamed = {4}"
                    , attackingCiv, attackedCiv, _sendOrder.Statement.ToString(), _sendOrder.Statement.Tone.ToString(), blamed + Environment.NewLine);

            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);

            System.Collections.ObjectModel.ReadOnlyCollection<Order> diploOrders = ServiceLocator.Current.GetInstance<IPlayerOrderService>().Orders;  // just for Break point controlling
        }

        public static void SabotageStealResearchExecute(Civilization attackingCiv, Civilization attackedCiv, string blamed, int ratio)
        {
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Colony colony = attackedCivManager.SeatOfGovernment.Sector.System.Colony;
            //GameContext.Current.CivilizationManagers[attackedCiv].UpDateBlamedCiv(_blamedCiv);

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int ratioLevel = -1;

            GameLog.Core.Test.DebugFormat("**** StealResearch, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            if (attackingCiv == null)
            {
                return;
            }

            if (attackedCiv == null)
            {
                return;
            }

            if (colony == null)
            {
                return;
            }

            ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            _ = int.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
            {
                defenseIntelligence = 2;
            }


            //Effect of steal // value needed for SitRep
            // calculation stolen research points depended on defenders stuff

            _ = int.TryParse(attackedCivManager.Research.CumulativePoints.ToString(), out int stolenResearchPoints);
            int attackedResearchCumulativePoints = stolenResearchPoints;

            if (stolenResearchPoints < 100)
            {
                stolenResearchPoints = -2;
                goto stolenResearchPointsIsMinusOne;
            }

            if (ratio < 2 || attackMeter.CurrentValue < 10)
            {
                stolenResearchPoints = -1;  // -2 for a differenz
                blamed = Blame(attackingCiv, attackedCiv, blamed, 3);
                goto stolenResearchPointsIsMinusOne;
            }

            stolenResearchPoints = stolenResearchPoints / 100 * 1; // default JUST 1 percent

            if (ratio > 2 && !RandomHelper.Chance(2)) // (Cumulative is meter) && attackedCivManager.Research.CumulativePoints > 10)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenResearchPoints *= 2;  // 2 percent, but base is CumulativePoints, so all research points ever yielded
                blamed = Blame(attackingCiv, attackedCiv, blamed, 4);
                ratioLevel = 1;
            }
            if (ratio > 10 && !RandomHelper.Chance(4))// && attackedCivManager.Treasury.CurrentLevel > 40) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenResearchPoints *= 3;
                blamed = Blame(attackingCiv, attackedCiv, blamed, 6);
                ratioLevel = 2;
            }
            if (ratio > 20 && !RandomHelper.Chance(8))// && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenResearchPoints *= 2;
                blamed = Blame(attackingCiv, attackedCiv, blamed, 10);
                ratioLevel = 3;
            }

            // stuff 

            Meter stuff = GameContext.Current.CivilizationManagers[attackedCiv].Research.CumulativePoints;

            Attack_ED_stuff_before = stuff.CurrentValue.ToString();
            _ = stuff.AdjustCurrent(stolenResearchPoints * -1);
            stuff.UpdateAndReset();
            Attack_ED_stuff_after = GameContext.Current.CivilizationManagers[attackedCiv].Research.CumulativePoints.CurrentValue.ToString();

            stuff = GameContext.Current.CivilizationManagers[attackingCiv].Research.CumulativePoints;
            Attack_ING_stuff_before = stuff.CurrentValue.ToString();
            _ = stuff.AdjustCurrent(stolenResearchPoints);
            stuff.UpdateAndReset();
            Attack_ING_stuff_after = stuff.CurrentValue.ToString();


            // DEFENSE CIV - Intel Points

            Attack_ED_IntelPoints_before = defenseMeter.CurrentValue.ToString();
            _ = defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            Attack_ED_IntelPointCosts = (defenseIntelligence / 4).ToString();
            Attack_ED_IntelPoints_after = defenseMeter.CurrentValue.ToString();



        //  ATTACKING CIV - Intel Points

        stolenResearchPointsIsMinusOne:;   // pushing buttons makes always 'intel costs'

            Attack_ING_IntelPoints_before = attackMeter.CurrentValue.ToString();
            _ = attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            Attack_ING_IntelPointCosts = (defenseIntelligence / 2).ToString();
            Attack_ING_IntelPoints_after = attackMeter.CurrentValue.ToString();

            string attack_ED_before = "attackED  = " + attackedCiv.Key + " BEFORE: Research: " + Attack_ED_stuff_before + " stolen= " + stolenResearchPoints + " - IP: "
                + Attack_ED_IntelPoints_before;
            string attack_ED_after = "attackED  = " + attackedCiv.Key + " AFTER : Research: " + Attack_ED_stuff_after + " stolen= " + stolenResearchPoints + " - IP: "
                + Attack_ED_IntelPoints_after + " IPcosts= " + Attack_ED_IntelPointCosts;
            string attack_ING_before = "attackING = " + attackingCiv.Key + " BEFORE: Research: " + Attack_ING_stuff_before + " stolen= " + stolenResearchPoints + " - IP: "
                + Attack_ING_IntelPoints_before;
            string attack_ING_after = "attackING = " + attackingCiv.Key + " AFTER : Research: " + Attack_ING_stuff_after + " stolen= " + stolenResearchPoints + " - IP: "
                + Attack_ING_IntelPoints_after + " IPcosts= " + Attack_ING_IntelPointCosts;

            GameLog.Core.Intel.DebugFormat(attack_ED_before);
            GameLog.Core.Intel.DebugFormat(attack_ED_after);
            GameLog.Core.Intel.DebugFormat(attack_ING_before);
            GameLog.Core.Intel.DebugFormat(attack_ING_after);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED");

            GameLog.Core.Intel.DebugFormat("Research stolen at {0}: {1} stolenResearchPoints, {2} blamed ******", colony.Name, stolenResearchPoints, blamed);

            _ = int.TryParse(attackedCivManager.Research.CumulativePoints.ToString(), out int newResearchCumulative);

            // Sitreps   attack*ed* and attack*ing*
            attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                   attackingCiv, attackedCiv, colony, affectedField, stolenResearchPoints, newResearchCumulative, blamed, ratioLevel));

            attackingCivManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, stolenResearchPoints, newResearchCumulative, blamed, ratioLevel));
        }

        public static void SabotageFood(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            // coming from Buttons in each of the six expanders
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            SendStatementOrder _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.SabotageFood, Tone.Enraged, blamed, GameContext.Current.TurnNumber))
            {
                Owner = attackingCiv
            };

            GameLog.Core.Diplomacy.DebugFormat("Create Statement for SabotageFood: " + Environment.NewLine
                    + "StatementType = {2} (Sender=) {0} *vs* {1} (Recipient), blamed = {3}, Tone = {4}"
                    , attackingCiv, attackedCiv, _sendOrder.Statement.ToString(), blamed, _sendOrder.Statement.Tone.ToString());

            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
        }

        public static void SabotageFoodExecute(Civilization attackingCiv, Civilization attackedCiv, string blamed, int ratio)
        {
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Colony colony = attackedCivManager.SeatOfGovernment.Sector.System.Colony;

            Meter defenseMeter = attackedCivManager.TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int ratioLevel = -1;

            int removeFoodFacilities = -2;  // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
            {
                return;
            }

            if (attackedCiv == null)
            {
                return;
            }

            if (colony == null)
            {
                return;
            }

            bool ownedByPlayer = colony.OwnerID == attackingCiv.CivID;
            if (ownedByPlayer)
            {
                return;
            }

            ratio = GetIntelRatio(attackedCivManager, attackingCivManager);

            if (ratio < 2 || attackMeter.CurrentValue < 10)
            {
                removeFoodFacilities = -1;
                goto NoActionFood;
            }

            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Food={3} out of facilities={4}, in total={5}",
                colony.Owner, colony.Name, colony.Location,
                colony.NetFood,
                colony.GetActiveFacilities(ProductionCategory.Food),
                colony.GetTotalFacilities(ProductionCategory.Food));
            GameLog.Core.Intel.DebugFormat("Sabotage Food to {0}: TotalFoodFacilities before={1}",
                colony.Name, colony.GetTotalFacilities(ProductionCategory.Food));

            //Effect of sabotage // value needed for SitRep
            //if ratio > 1 than remove one more  FoodFacility
            if (ratio > 1 /*&& !RandomHelper.Chance(2) */&& colony.GetTotalFacilities(ProductionCategory.Food) > 1)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 3);
                ratioLevel = 1;
            }

            //if ratio > 2 than remove one more  FoodFacility
            if (ratio > 20 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Food) > 3)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //  2 and one from before
                colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 4);
                ratioLevel = 2;
            }

            // if ratio > 3 than remove one more  FoodFacility
            if (ratio > 40 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Food) > 5)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 6);
                ratioLevel = 3;
            }

            //// stuff 

            //var stuff = defenseMeter;

            //Attack_ED_stuff_before = stuff.CurrentValue.ToString();
            //stuff.AdjustCurrent(defenseIntelligence / 4 * -1);
            //stuff.UpdateAndReset();
            //Attack_ED_stuff_after = stuff.CurrentValue.ToString();

            //stuff = GameContext.Current.CivilizationManagers[attackingCiv].Research.CumulativePoints;
            //Attack_ING_stuff_before = stuff.CurrentValue.ToString();
            //stuff.AdjustCurrent(defenseIntelligence / 2);
            //stuff.UpdateAndReset();
            //Attack_ING_stuff_after = stuff.CurrentValue.ToString();


            // DEFENSE CIV - Intel Points

            Attack_ED_IntelPoints_before = defenseMeter.CurrentValue.ToString();
            _ = defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            Attack_ED_IntelPointCosts = (defenseIntelligence / 4).ToString();
            Attack_ED_IntelPoints_after = defenseMeter.CurrentValue.ToString();



        //  ATTACKING CIV - Intel Points

        NoActionFood:;   // pushing buttons makes always 'intel costs'

            Attack_ING_IntelPoints_before = attackMeter.CurrentValue.ToString();
            _ = attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            Attack_ING_IntelPointCosts = (defenseIntelligence / 2).ToString();
            Attack_ING_IntelPoints_after = attackMeter.CurrentValue.ToString();


            Attack_ED_stuff_after = colony.GetTotalFacilities(ProductionCategory.Food).ToString();
            Attack_ED_stuff_before = (colony.GetTotalFacilities(ProductionCategory.Food) + removeFoodFacilities).ToString();
            string attack_ED_before = "attackED  = " + attackedCiv.Key + " BEFORE: FOOD: " + Attack_ED_stuff_before + " removed= " + removeFoodFacilities + " - IP: "
                + Attack_ED_IntelPoints_before;
            string attack_ED_after = "attackED  = " + attackedCiv.Key + " AFTER : FOOD: " + Attack_ED_stuff_after + " removed= " + removeFoodFacilities + " - IP: "
                + Attack_ED_IntelPoints_after + " IPcosts= " + Attack_ED_IntelPointCosts;
            string attack_ING_before = "attackING = " + attackingCiv.Key + " BEFORE: FOOD:                 - IP: "
                + Attack_ING_IntelPoints_before;
            string attack_ING_after = "attackING = " + attackingCiv.Key + " AFTER : FOOD:                  - IP: "
                + Attack_ING_IntelPoints_after + " IPcosts= " + Attack_ING_IntelPointCosts;

            GameLog.Core.Intel.DebugFormat(attack_ED_before);
            GameLog.Core.Intel.DebugFormat(attack_ED_after);
            GameLog.Core.Intel.DebugFormat(attack_ING_before);
            GameLog.Core.Intel.DebugFormat(attack_ING_after);

            GameLog.Core.Intel.DebugFormat("Sabotage Food at {0}: TotalFoodFacilities after={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Food), blamed);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD");

            // Sitreps   attack*ed* and attack*ing*
            attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                    attackingCiv, attackedCiv, colony, affectedField, removeFoodFacilities, colony.GetTotalFacilities(ProductionCategory.Food), blamed, ratioLevel));

            attackingCivManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeFoodFacilities, colony.GetTotalFacilities(ProductionCategory.Food), blamed, ratioLevel));
        }

        public static void SabotageEnergy(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            // coming from Buttons in each of the six expanders
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            SendStatementOrder _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.SabotageEnergy, Tone.Enraged, blamed, GameContext.Current.TurnNumber))
            {
                Owner = attackingCiv
            };
            GameLog.Core.Diplomacy.DebugFormat("Create Statement for SabotageEnergy: " + Environment.NewLine
                + "sender = {0} *vs* Recipient = {1}: StatementType = {2}, blamed = {3} tone ={4}"
                                , attackingCiv, attackedCiv, _sendOrder.Statement.ToString(), blamed, _sendOrder.Statement.Tone.ToString());

            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
        }

        public static void SabotageEnergyExecute(Civilization attackingCiv, Civilization attackedCiv, string blamed, int ratio)
        {
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Colony colony = attackedCivManager.SeatOfGovernment.Sector.System.Colony;
            Meter defenseMeter = GameContext.Current.CivilizationManagers[colony.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int ratioLevel = -1;
            int removeEnergyFacilities = -2;
            int defenseIntelligence = -2;

            if (attackingCiv == null)
            {
                return;
            }

            if (attackedCiv == null)
            {
                return;
            }

            if (colony == null)
            {
                return;
            }

            bool ownedByPlayer = colony.OwnerID == attackingCiv.CivID;
            if (ownedByPlayer)
            {
                return;
            }

            ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            if (ratio < 2)
            {
                removeEnergyFacilities = -1;
                goto NoActionEnergy;
            }

            _ = int.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
            {
                defenseIntelligence = 2;
            }

            //Effect of sabotage // value needed for SitRep
            GameLog.Core.Intel.DebugFormat("**** Before Sabotage Energy at {0}: TotalEnergyFacilities before={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy), blamed);
            //if ratio > 1 than remove one more  EnergyFacility
            if (ratio > 10 /*&& RandomHelper.Chance(4)*/ && colony.GetTotalFacilities(ProductionCategory.Energy) > 3)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 3);
                ratioLevel = 1;
            }

            //if ratio > 2 than remove one more  EnergyFacility
            if (ratio > 40 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Energy) > 4)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities += 1;  //  2 and one from before
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 4);
                ratioLevel = 2;
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (ratio > 80 && !RandomHelper.Chance(2) && colony.GetTotalFacilities(ProductionCategory.Energy) > 5)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 6);
                ratioLevel = 3;
            }

            // DEFENSE CIV - Intel Points

            Attack_ED_IntelPoints_before = defenseMeter.CurrentValue.ToString();
            _ = defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            Attack_ED_IntelPointCosts = (defenseIntelligence / 4).ToString();
            Attack_ED_IntelPoints_after = defenseMeter.CurrentValue.ToString();



        //  ATTACKING CIV - Intel Points

        NoActionEnergy:;   // pushing buttons makes always 'intel costs'

            Attack_ING_IntelPoints_before = attackMeter.CurrentValue.ToString();
            _ = attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            Attack_ING_IntelPointCosts = (defenseIntelligence / 2).ToString();
            Attack_ING_IntelPoints_after = attackMeter.CurrentValue.ToString();


            Attack_ED_stuff_after = colony.GetTotalFacilities(ProductionCategory.Energy).ToString();
            Attack_ED_stuff_before = (colony.GetTotalFacilities(ProductionCategory.Energy) + removeEnergyFacilities).ToString();
            string attack_ED_before = "attackED  = " + attackedCiv.Key + " BEFORE: ENERGY: " + Attack_ED_stuff_before + " removed= " + removeEnergyFacilities + " - IP: "
                + Attack_ED_IntelPoints_before;
            string attack_ED_after = "attackED  = " + attackedCiv.Key + " AFTER : ENERGY: " + Attack_ED_stuff_after + " removed= " + removeEnergyFacilities + " - IP: "
                + Attack_ED_IntelPoints_after + " IPcosts= " + Attack_ED_IntelPointCosts;
            string attack_ING_before = "attackING = " + attackingCiv.Key + " BEFORE: ENERGY:                    - IP: "
                + Attack_ING_IntelPoints_before;
            string attack_ING_after = "attackING = " + attackingCiv.Key + " AFTER : ENERGY:                    - IP: "
                + Attack_ING_IntelPoints_after + " IPcosts= " + Attack_ING_IntelPointCosts;

            GameLog.Core.Intel.DebugFormat(attack_ED_before);
            GameLog.Core.Intel.DebugFormat(attack_ED_after);
            GameLog.Core.Intel.DebugFormat(attack_ING_before);
            GameLog.Core.Intel.DebugFormat(attack_ING_after);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY");

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy), blamed);

            // Sitreps   attack*ed* and attack*ing*
            attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, ratioLevel));

            attackingCivManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                    attackingCiv, attackedCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, ratioLevel));
        }

        public static void SabotageIndustry(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            // coming from Buttons in each of the six expanders
            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            SendStatementOrder _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.SabotageIndustry, Tone.Enraged, blamed, GameContext.Current.TurnNumber))
            {
                Owner = attackingCiv
            };
            GameLog.Core.DiplomacyDetails.DebugFormat("Create Statement for SabotageIndustry...");
            //GameLog.Core.DiplomacyDetails.DebugFormat("Create Statement for SabotageIndustry: " + Environment.NewLine
            //    + "sender = {0} *vs* Recipient = {1}: StatementType = {2}, Tone ={3}, blamed = {4}"
            //                    , attackingCiv, attackedCiv, _sendOrder.Statement.StatementType.ToString(),
            //                    _sendOrder.Statement.Tone.ToString(), blamed + Environment.NewLine);
            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
        }

        public static void SabotageIndustryExecute(Civilization attackingCiv, Civilization attackedCiv, string blamed, int ratio)
        {

            CivilizationManager attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            CivilizationManager attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Colony colony = attackedCivManager.SeatOfGovernment.Sector.System.Colony;
            //GameContext.Current.CivilizationManagers[attackedCiv].UpDateBlamedCiv(_blamedCiv);

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int ratioLevel = -1;

            GameLog.Core.Intel.DebugFormat("**** Sabotage Industry, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            int removeIndustryFacilities = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
            {
                return;
            }

            if (attackedCiv == null)
            {
                return;
            }

            if (colony == null)
            {
                return;
            }

            ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            if (ratio < 2)
            {
                removeIndustryFacilities = -1;
                goto NoActionIndustry;
            }

            _ = int.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
            {
                defenseIntelligence = 2;
            }

            //Effect of sabotage // value needed for SitRep
            //if ratio > 1 than remove one more  IndustryFacility
            if (ratio > 10 /*&& !RandomHelper.Chance(2)*/ && colony.GetTotalFacilities(ProductionCategory.Industry) > 3)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 3);
                ratioLevel = 1;
            }

            //if ratio > 2 than remove one more  IndustryFacility
            if (ratio > 20 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Industry) > 4)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //  2 and one from before
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 4);
                ratioLevel = 2;
            }

            // if ratio > 3 than remove one more  IndustryFacility
            if (ratio > 30 && !RandomHelper.Chance(6) && colony.GetTotalFacilities(ProductionCategory.Industry) > 5)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = Blame(attackingCiv, attackedCiv, blamed, 6);
                ratioLevel = 3;
            }

            // DEFENSE CIV - Intel Points

            Attack_ED_IntelPoints_before = defenseMeter.CurrentValue.ToString();
            _ = defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            Attack_ED_IntelPointCosts = (defenseIntelligence / 4).ToString();
            Attack_ED_IntelPoints_after = defenseMeter.CurrentValue.ToString();



        //  ATTACKING CIV - Intel Points

        NoActionIndustry:;   // pushing buttons makes always 'intel costs'

            Attack_ING_IntelPoints_before = attackMeter.CurrentValue.ToString();
            _ = attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            Attack_ING_IntelPointCosts = (defenseIntelligence / 2).ToString();
            Attack_ING_IntelPoints_after = attackMeter.CurrentValue.ToString();

            Attack_ED_stuff_after = colony.GetTotalFacilities(ProductionCategory.Industry).ToString();
            Attack_ED_stuff_before = (colony.GetTotalFacilities(ProductionCategory.Industry) + removeIndustryFacilities).ToString();
            string attack_ED_before = "attackED  = " + attackedCiv.Key + " BEFORE: INDUSTRY: " + Attack_ED_stuff_before + " removed= " + removeIndustryFacilities + " - IP: "
                + Attack_ED_IntelPoints_before;
            string attack_ED_after = "attackED  = " + attackedCiv.Key + " AFTER : INDUSTRY: " + Attack_ED_stuff_after + " removed= " + removeIndustryFacilities + " - IP: "
                + Attack_ED_IntelPoints_after + " IPcosts= " + Attack_ED_IntelPointCosts;
            string attack_ING_before = "attackING = " + attackingCiv.Key + " BEFORE: INDUSTRY:                    - IP: "
                + Attack_ING_IntelPoints_before;
            string attack_ING_after = "attackING = " + attackingCiv.Key + " AFTER : INDUSTRY:                    - IP: "
                + Attack_ING_IntelPoints_after + " IPcosts= " + Attack_ING_IntelPointCosts;

            GameLog.Core.Intel.DebugFormat(attack_ED_before);
            GameLog.Core.Intel.DebugFormat(attack_ED_after);
            GameLog.Core.Intel.DebugFormat(attack_ING_before);
            GameLog.Core.Intel.DebugFormat(attack_ING_after);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY");

            GameLog.Core.Intel.DebugFormat("Sabotage Industry at {0}: TotalIndustryFacilities after={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Industry), blamed);

            // Sitreps   attack*ed* and attack*ing*
            attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                    attackingCiv, attackedCiv, colony, affectedField, removeIndustryFacilities, colony.GetTotalFacilities(ProductionCategory.Industry), blamed, ratioLevel));

            attackingCivManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeIndustryFacilities, colony.GetTotalFacilities(ProductionCategory.Industry), blamed, ratioLevel));
        }
        public static int GetIntelRatio(CivilizationManager attackedCivManager, CivilizationManager attackingCivManager)
        {
            bool isSpyShipInHomeSystem = IsSpyShipInHomeSystem(attackedCivManager, attackingCivManager);
            bool daBorg = attackedCivManager.Civilization.Key == "Borg";
            _ = int.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
            {
                defenseIntelligence = 2;
            }

            _ = int.TryParse(GameContext.Current.CivilizationManagers[attackingCivManager].TotalIntelligenceAttackingAccumulated.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
            {
                attackingIntelligence = 1;
            }

            attackingIntelligence = 1 * attackingIntelligence;// multiplied with 1000 - just for increase attacking Intelligence

            int ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
            {
                ratio = 1;
            }

            GameLog.Core.Intel.DebugFormat("Intelligence attacking = {0}, defense = {1}, Ratio = {2}", attackingIntelligence, defenseIntelligence, ratio);
            if (RandomHelper.Chance(4))
            {
                ratio += 5;
            }

            if (RandomHelper.Chance(10))
            {
                ratio += 20;
            }

            if (daBorg)
            {
                ratio += 5;
            }

            if (isSpyShipInHomeSystem)
            {
                ratio += 10;
            }

            return ratio;
        }

        public static bool IsSpyShipInHomeSystem(CivilizationManager attackedCivManager, CivilizationManager attackingCivManager)
        {
            Colony attackedHomeSystemLocation = attackedCivManager.SeatOfGovernment;
            IEnumerable<Fleet> fleetsAtSetOfGovernment = attackedHomeSystemLocation.Sector.GetFleets();
            int attackingSpyShips = fleetsAtSetOfGovernment.Where(s => s.IsSpy && s.Owner == attackingCivManager.Civilization).ToList().Count();//Where(x => x.ShipType.ToString() == "Spy")).ToList();

            if (attackingSpyShips > 0)
            {
                return true;
            }
            return false;
        }
    }
    #endregion

}
