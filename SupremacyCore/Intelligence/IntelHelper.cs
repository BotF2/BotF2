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
using System.Collections;
using System.Collections.Generic;

namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static Civilization _newTargetCiv;
        private static Civilization _newSpyCiv;
        private static UniverseObjectList<Colony> _newSpiedColonies;
        private static List<Civilization> _spiedList = new List<Civilization>();
        private static List<Civilization> _localSpiedList = new List<Civilization>();
        private static List<SitRepEntry> _sitReps_Temp = new List<SitRepEntry>();
        private static int _defenseAccumulatedIntelInt;
        private static int _attackAccumulatedIntelInt;
        private static CivilizationManager _localCivManager;
        public static List<Civilization> _spyingCiv_0_List;
        public static List<Civilization> _spyingCiv_1_List;
        public static List<Civilization> _spyingCiv_2_List;
        public static List<Civilization> _spyingCiv_3_List;
        public static List<Civilization> _spyingCiv_4_List;
        public static List<Civilization> _spyingCiv_5_List;
        public static List<Civilization> _spyingCiv_6_List;
        public static Dictionary<Civilization, string> _blamedCiv;
        public static List<NewIntelOrders> _local_IntelOrders = new List<NewIntelOrders>();
        public static bool _showNetwork_0 = false;
        public static bool _showNetwork_1 = false;
        public static bool _showNetwork_2 = false;
        public static bool _showNetwork_3 = false;
        public static bool _showNetwork_4 = false;
        public static bool _showNetwork_5 = false;
        public static bool _showNetwork_6 = false;
       // private static Dictionary<int, IntelOrdersStealCredits> _intelStealCreditDictionary;
       // private static List<KeyValuePair<int, IntelOrdersStealCredits>> _intelStealCreditList;

        public static List<SitRepEntry> SitReps_Temp
        {
            get { return _sitReps_Temp; }
            set { _sitReps_Temp = value; }
        }
        public static UniverseObjectList<Colony> NewSpiedColonies
        {
            get { return _newSpiedColonies; }
        }
        public static Civilization NewSpyCiv
        {
            get { return _newSpyCiv; }
        }
        public static Civilization NewTargetCiv
        {
            get { return _newTargetCiv; }
        }

        public static CivilizationManager LocalCivManager
        {
            get { return _localCivManager; }
        }
        public static int DefenseAccumulatedInteInt
        {
            get
            {
                _defenseAccumulatedIntelInt = GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue;
                return _defenseAccumulatedIntelInt;
            }
        }
        public static int AttackingAccumulatedInteInt
        {
            get
            {
                _attackAccumulatedIntelInt = GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue;
                return _attackAccumulatedIntelInt;
            }
        }
        public static bool ShowNetwork_0
        {
            get { return _showNetwork_0; }
        }
        public static bool ShowNetwork_1
        {
            get { return _showNetwork_1; }
        }
        public static bool ShowNetwork_2
        {
            get { return _showNetwork_2; }
        }
        public static bool ShowNetwork_3
        {
            get { return _showNetwork_3; }
        }
        public static bool ShowNetwork_4
        {
            get { return _showNetwork_4; }
        }
        public static bool ShowNetwork_5
        {
            get { return _showNetwork_5; }
        }
        public static bool ShowNetwork_6
        {
            get { return _showNetwork_6; }
        }
        //public static Dictionary<int, IntelOrdersStealCredits> IntelStealCreditsDictionary
        //{
        //    get { return _intelStealCreditDictionary; }
        //}

        /// <summary>
        /// Using the civ manager as a param from AssetsScreen. Hope this is the local machine local player
        /// </summary>
        /// <param name="civManager"></param>
        /// <returns></returns>
        public static CivilizationManager GetLocalCiv(CivilizationManager civManager)
        {
            _localCivManager = civManager;
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
                throw new ArgumentNullException("spyCiv");
            if (spiedCiv == null)
                throw new ArgumentNullException("spiedCiv");
            if (_localCivManager.Civilization == spyCiv)
                ShowSpyNetwork(spiedCiv);
            _spiedList.Clear();
            _newSpyCiv = spyCiv;
            _newTargetCiv = spiedCiv;
            _newSpiedColonies = colonies;
            var newList = new List<Civilization> { spiedCiv };
            switch (spyCiv.CivID)
            {
                case 0:
                    if (_spyingCiv_0_List == null)
                        _spyingCiv_0_List = newList;
                    else
                    {
                        _spyingCiv_0_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_0_List);
                    break;
                case 1:
                    if (_spyingCiv_1_List == null)
                        _spyingCiv_1_List = newList;
                    else
                    {
                        _spyingCiv_1_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_1_List);
                    break;
                case 2:
                    if (_spyingCiv_2_List == null)
                        _spyingCiv_2_List = newList;
                    else
                    {
                        _spyingCiv_2_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_2_List);
                    break;
                case 3:
                    if (_spyingCiv_3_List == null)
                        _spyingCiv_3_List = newList;
                    else
                    {
                        _spyingCiv_3_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_3_List);
                    break;
                case 4:
                    if (_spyingCiv_4_List == null)
                        _spyingCiv_4_List = newList;
                    else
                    {
                        _spyingCiv_4_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_4_List);
                    break;
                case 5:
                    if (_spyingCiv_5_List == null)
                        _spyingCiv_5_List = newList;
                    else
                    {
                        _spyingCiv_5_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_5_List);
                    break;
                case 6:
                    if (_spyingCiv_6_List == null)
                        _spyingCiv_6_List = newList;
                    else
                    {
                        _spyingCiv_6_List.Add(spiedCiv);
                    }
                    GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_6_List);
                    break;
            }
            GameLog.Client.UI.DebugFormat("********* end of sending spied list to CM **********");
           // PopulateDefence();
        }

        #region Espionage Methods
        public static string Blame(Civilization localCivAttacker, string blamed, int chance)
        {
            if (!RandomHelper.Chance(chance))
                return localCivAttacker.ShortName;
            return blamed;
        }

        // coming from Buttons in each of the six expanders
        public static void StealCredits(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];

            
            var _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.DenounceRelationship, Tone.Indignant, blamed));
            _sendOrder.Owner = attackingCiv;
            GameLog.Core.Diplomacy.DebugFormat("Create Statement for Stealing Credits sender = {0} *vs* Recipient = {1}: Tone = {2}  StatementType = {3}, blamed = {4}"
                                , attackingCiv, attackedCiv, "Tone.Indignant", "DenounceRelationship", blamed);
            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);

            var diploOrders = ServiceLocator.Current.GetInstance<IPlayerOrderService>().Orders;  // just for Break point controlling

            //var playerDiplomat = Diplomat.Get(attackingCiv.CivID);            
            
            //IntelHelper.NewIntelOrders order = new NewIntelOrders(attackingCiv.CivID, attackedCiv.CivID, "StealCredits", blamed);

            //var statementType = DiplomacyScreenViewModel.ElementTypeToStatementType(_elements[0].ElementType);
            //if (statementType == StatementType.NoStatement)
            //    return null;
            //if (statementType != StatementType.NoStatement)
            //    GameLog.Core.Diplomacy.DebugFormat("Create Statement sender = {0} *vs* rRecipient = {1}: Tone = {2}  StatementType = {3} ", _sender, _recipient, _tone, statementType.ToString());

            //return new Statement(attackingCiv, attackedCiv, StatementType.DenounceRelationship, Tone.Indignant);
            //var _sendOrder = new SendStatementOrder(new Statement(attackingCiv, attackedCiv, StatementType.DenounceRelationship, Tone.Indignant));


            //IntelOrdersStealCredits stealCredit = new IntelOrdersStealCredits(attackingCiv, attackedCiv, blamed);
            //GameLog.Core.Intel.DebugFormat("** Class StealCredits = {0} vs {1} blamedd = {2}", attackingCiv.Key, attackedCiv.Key, blamed);

            //attackingCivManager.UpdateIntelOrdersGoingToHost(order);

            //if (attackingCivManager.IntelOrdersGoingToHost == null)
            //{
            //    var itemList = new List<IntelHelper.NewIntelOrders>();
            //    //attackingCivManager.IntelOrdersGoingToHost = itemList;
            //    //itemList.Add(new IntelHelper.NewIntelOrders(5, 6, "StealCredits", "0"));
            //    itemList.Add(order);
            //    //attackingCivManager.IntelOrdersGoingToHost.AddRange(itemList);
            //    //attackingCivManager.IntelOrdersGoingToHost. = new List<IntelHelper.NewIntelOrders>({ 5, 6, "StealCredits", "0" });
            //    attackingCivManager.IntelOrdersGoingToHost = itemList;
            //}
            //else
            //{
            //    attackingCivManager.IntelOrdersGoingToHost.Add(order);
            //}
            




            //var listIntelOrders = playerDiplomat.IntelOrdersGoingToHost;

            //listIntelOrders.Add(order);
            //playerDiplomat.UpdateIntelOrderList(listIntelOrders);
            


        }

        // from DoPreTurnOperations in GameEngine, only do it at this time //   Done at HOST !!!!!
        public static void ExecuteIntelIncomingOrders()  //(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {

            //GameLog.Core.Intel.DebugFormat("doing ExecuteIntelIncomingOrders...");

            var civs = GameContext.Current.CivilizationManagers;

            var empiresDoingIntel = new List<CivilizationManager>();

            foreach (var civ in civs)
            {
                if (civ.Civilization.IsEmpire)
                    empiresDoingIntel.Add(civ);
            }

            var _completeListofIntelOrders = new List<IntelHelper.NewIntelOrders>();

            foreach (var empire in empiresDoingIntel)
            {
                var _diplomat = Diplomat.Get(empire);
                //GameLog.Core.Intel.DebugFormat("checking from {0}: for Dipolmat.IntelOrdersGoingToHost... # = {1}"/*no counting"*/, empire.Civilization.Key, _diplomat.IntelOrdersGoingToHost.Count); //, empire.Civilization.IntelOrdersGoingToHost.Count);
                if (empire.CivilizationID == 4)
                {
                    //GameLog.Core.Intel.DebugFormat("Before add intelOrders CivKey = {0}: Count Intel Orders = {1}", empire.Civilization.Key, Diplomat.Get(empire).IntelOrdersGoingToHost.Count);
                    //if (Diplomat.Get(empire).IntelOrdersGoingToHost.Count > 0)
                    //{
                    //    //_completeListofIntelOrders.AddRange(empire.IntelOrdersGoingToHost);
                    //    _completeListofIntelOrders.AddRange(Diplomat.Get(empire).IntelOrdersGoingToHost);
                    //    //GameLog.Core.Intel.DebugFormat("add to CivKey {0} Intel OrderToHost Count = {1}", empire.Civilization.Key, Diplomat.Get(empire).IntelOrdersGoingToHost.Count);
                    //}
                }
            }

            //works
            //foreach (var item in _completeListofIntelOrders)
            //{
            //    GameLog.Core.Intel.DebugFormat("_completeListofIntelOrders-Entry: {0} from {1} against {2], blamed={3}", item.Intel_Order, item.AttackingCivID, item.AttackedCivID, item.Intel_Order_Blamed);
            //}

            foreach (var order in _completeListofIntelOrders)
                {

                    //if (order.AttackingCivID == 999)
                    //{
                    //    GameLog.Core.Intel.DebugFormat("Creating fake Incoming Order... (ROM vs DOM)"); // Incoming: {2} for {0} VS {1}", attacking.Civilization.Key,
                    //    order.AttackedCivID = 5;
                    //    order.AttackingCivID = 3;
                    //    order.Intel_Order = "StealCredits";
                    //    order.Intel_Order_Blamed = "bl_Federation";
                    //    //order._attackedCivID = 5;
                    //    //order._attackingCivID = 3;
                    //    //order._intel_Order = "StealCreditsUL";
                    //    //order._intel_Order_Blamed = "bl_FederationUL";
                    //}

                    //GameLog.Core.Intel.DebugFormat("Incoming: {2} for {0} VS {1}", attacking.Civilization.Key,
                    //                                        attacked.Civilization.Key,
                    //                                         order.Intel_Order,
                    //                                         order.Intel_Order_Blamed);

                    var attacking = GameContext.Current.CivilizationManagers[2].Civilization;
                    var attacked = GameContext.Current.CivilizationManagers[5].Civilization;
                    //GameLog.Core.Intel.DebugFormat("Incoming: {2} for {0} VS {1}", attacking.Key,
                    //                                                                attacked.Key,
                    //                                                                 order.Intel_Order,
                    //                                                                 order.Intel_Order_Blamed);
                    switch (order.Intel_Order)
                    {
                        case "StealCredits":
                            ExecuteStealCredits(attacking, attacked, "_bla_Terrorists");
                            break;
                        default:
                            break;
                    }

                //    }
                //}
                //GameLog.Core.Intel.DebugFormat("", empire.IntelOrdersGoingToHost.);
            }
        }

        //IntelOrders.SetNewIntelOrders(); /*just for fun*/

        //IntelOrders.SetNewIntelOrders..add(_intelOrder);
        //.SetIntelOrder(attackingCiv.CivID.ToString(), IntelOrder.StealCredits.ToString());  // we are sending to host: ID of local civ = local player, + a dictionary, who is attacked with what (e.g. StealCredits)

        //IntelOrders.

        public static void ExecuteStealCredits(Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;

            GameLog.Core.Intel.DebugFormat("**** StealCredits, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);

            int stolenCredits = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
                return;
            if (attackedCiv == null)
                return;
            //if (colony == null)
            //    return;

            int ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of steal // value needed for SitRep
            //int removeChredits = 0;
            Int32.TryParse(attackedCivManager.Credits.ToString(), out stolenCredits);
            int attackedCreditsBefore = stolenCredits;

            if (stolenCredits < 100)  // they have not enough credits worth stealing, especially avoid negative stuff !!!
            {
                stolenCredits = -2;
                goto stolenCreditsIsMinusOne;
            }

            if (ratio < 2 || attackMeter.CurrentValue < 10)  // 
            {
                stolenCredits = -1;  // failed
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
                goto stolenCreditsIsMinusOne;
            }

            stolenCredits = stolenCredits / 100 * 3;  // default 3 percent

            if (!RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 5)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                stolenCredits = stolenCredits * 3; // 2 percent of their TOTAL Credits - not just income
                blamed = IntelHelper.Blame(attackingCiv, blamed, 3);
            }
            if (ratio > 10 && !RandomHelper.Chance(3) && attackedCivManager.Treasury.CurrentLevel > 20) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                if (!RandomHelper.Chance(2))
                {
                    stolenCredits = stolenCredits * 3;
                    blamed = IntelHelper.Blame(attackingCiv, blamed, 4);
                }
            }
            if (ratio > 20 && !RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                // SeeStealCredits(_newTargetCiv, "Clicked");
                stolenCredits = stolenCredits * 2;
                blamed = IntelHelper.Blame(attackingCiv, blamed, 5);

            }
            GameLog.Core.Intel.DebugFormat("**** CREDITS, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.Intel.DebugFormat("BEFORE: attackED civ={0}: {1} Credits", attackedCiv.Key, GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue);

            //GameEngine engine = new GameEngine();
            //engine.SendStealCreditsData(attackedCiv, attackingCiv, stolenCredits);
            //GameContext.Current.CivilizationManagers[attackedCiv].Credits.AdjustCurrent(stolenCredits * -1);
            // GameContext.Current.CivilizationManagers[attackedCiv].Credits.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat(" AFTER: attackED civ={0}: {1} Credits", attackedCiv.Key, GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue);
            GameLog.Core.Intel.DebugFormat("attacing accumulated ={0}", _attackAccumulatedIntelInt);

            GameLog.Core.Intel.DebugFormat("BEFORE: attackING civ={0}: {1} Credits", attackingCiv.Key, GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue);

            GameContext.Current.CivilizationManagers[attackingCiv].Credits.AdjustCurrent(stolenCredits);
            GameContext.Current.CivilizationManagers[attackingCiv].Credits.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat(" AFTER: attakING civ={0}: {1} Credits, {2} Blamed *******", attackingCiv.Key, GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue, blamed);
            GameLog.Core.Intel.DebugFormat("attacing accumulated ={0}", _attackAccumulatedIntelInt);

            // DEFENSE

            GameLog.Core.Intel.DebugFormat("** DEFENSE METER INT, attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.Intel.DebugFormat("BEFORE: attackED civ={0}: defense meter {1}, accumulated ={2}",
                    attackedCiv.Key, defenseMeter.CurrentValue, DefenseAccumulatedInteInt);
            GameLog.Core.Intel.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);
            //GameLog.Core.Intel.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
            //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);

            defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat(" AFTER: attackED civ={0}: defense meter {1}, accumulated ={2}",
                attackedCiv.Key, defenseMeter.CurrentValue, DefenseAccumulatedInteInt);
            GameLog.Core.Intel.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);
            //GameLog.Core.Intel.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
            //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);

            //  ATTACKING

            stolenCreditsIsMinusOne:;   // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("** ATTACK METER INT attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.Intel.DebugFormat("BEFORE: attackED civ={0}: attackED meter {1}, accumulated ={2}",
                    attackedCiv.Key, attackMeter.CurrentValue, AttackingAccumulatedInteInt);
            GameLog.Core.Intel.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);
            //GameLog.Core.Intel.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
            //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].Credits.CurrentValue);

            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat(" AFTER: attackED civ={0}: attackED meter {1}, accumulated ={2}",
                    attackedCiv.Key, attackMeter.CurrentValue, AttackingAccumulatedInteInt);
            GameLog.Core.Intel.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED");

            Int32.TryParse(GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue.ToString(), out int newCreditsAttacked);

            GameLog.Core.Intel.DebugFormat("Stolen Credits from {0}:  >>> {1} Credits, {2} Blamed", attackedCiv.Key, stolenCredits, blamed);

            /* only SitRep when local player is attacked for now
            _sitReps_Temp.Add(new NewSabotagingSitRepEntry(
                   attackingCiv, attackedCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed));
                   */
            //*********** Want spy operation clicks coming back to host the same as it does for combat, combatupdat and now intelupdate


            //_sitReps_Temp.Add(new NewSabotagedSitRepEntry(
            //        attackedCiv, attackingCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;

            // UpdatingBlame(attackingCiv, attackedCiv, blamed);
            // EndofStealCredits:;

        }
        public static void StealResearch(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            //GameContext.Current.CivilizationManagers[attackedCiv].UpDateBlamedCiv(_blamedCiv);

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;

            GameLog.Core.Test.DebugFormat("**** StealResearch, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);

            int stolenResearchPoints = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
                return;
            if (attackedCiv == null)
                return;
            if (colony == null)
                return;

            int ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of steal // value needed for SitRep

            // calculation stolen research points depended on defenders stuff

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Research.CumulativePoints.ToString(), out stolenResearchPoints);
            int attackedResearchCumulativePoints = stolenResearchPoints;

            if (stolenResearchPoints < 100)
            {
                stolenResearchPoints = -2;
                goto stolenResearchPointsIsMinusOne;
            }

            if (ratio < 2 || attackMeter.CurrentValue < 10)
            {
                stolenResearchPoints = -1;  // -2 for a differenz
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
                goto stolenResearchPointsIsMinusOne;
            }

            stolenResearchPoints = stolenResearchPoints / 100 * 1; // default JUST 1 percent

            //if (ratio < 2) stolenResearchPoints = 0; // ratio = 1 or less: no success 

            if (ratio > 1 && !RandomHelper.Chance(2)) // (Cumulative is meter) && attackedCivManager.Research.CumulativePoints > 10)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                // ToDo add to local player           
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 2;  // 2 percent, but base is CumulativePoints, so all research points ever yielded
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
            }
            if (ratio > 10 && !RandomHelper.Chance(4))// && attackedCivManager.Treasury.CurrentLevel > 40) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 3;
                blamed = IntelHelper.Blame(attackingCiv, blamed, 3);
            }
            if (ratio > 20 && !RandomHelper.Chance(8))// && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 2;
                blamed = IntelHelper.Blame(attackingCiv, blamed, 4);
            }

            GameLog.Core.Intel.DebugFormat("Research ** BEFORE ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[attackingCiv].Research.CumulativePoints);

            // result   // only attackingCiv = attackingCiv is getting a plus of research points
            if (stolenResearchPoints > 0)
                GameContext.Current.CivilizationManagers[attackingCiv].Research.UpdateResearch(stolenResearchPoints);

            GameLog.Core.Intel.DebugFormat("Research ** AFTER ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[attackingCiv].Research.CumulativePoints);

            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////

            stolenResearchPointsIsMinusOne:;  // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED");

            GameLog.Core.Intel.DebugFormat("Research stolen at {0}: {1} stolenResearchPoints, {2} blamed ******", system.Name, stolenResearchPoints, blamed);

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Research.CumulativePoints.ToString(), out int newResearchCumulative);

            //_sitReps_Temp.Add(new NewSabotagingSitRepEntry(
            //       attackingCiv, attackedCiv, colony, affectedField, stolenResearchPoints, newResearchCumulative, blamed));

            _sitReps_Temp.Add(new NewSabotagedSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, stolenResearchPoints, newResearchCumulative, blamed));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;

        }
        public static void SabotageFood(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int removeFoodFacilities = -2;  // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
                return;
            if (attackedCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == attackingCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager, attackingCivManager);

            if (ratio < 2 || attackMeter.CurrentValue < 10)
            {
                removeFoodFacilities = -1;
                goto NoActionFood;
            }

            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Food={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.NetFood,
                system.Colony.GetActiveFacilities(ProductionCategory.Food),
                system.Colony.GetTotalFacilities(ProductionCategory.Food));
            GameLog.Core.Intel.DebugFormat("Sabotage Food to {0}: TotalFoodFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Food));

            //Effect of sabotage // value needed for SitRep


            //if ratio > 1 than remove one more  FoodFacility
            if (ratio > 1 /*&& !RandomHelper.Chance(2) */&& colony.GetTotalFacilities(ProductionCategory.Food) > 1)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
            }

            //if ratio > 2 than remove one more  FoodFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 2)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 3);
            }

            // if ratio > 3 than remove one more  FoodFacility
            if (ratio > 20 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 3)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 4);
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionFood:;   // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            GameLog.Core.Intel.DebugFormat("Sabotage Food at {0}: TotalFoodFacilities after={1}, {2} blamed", system.Name, colony.GetTotalFacilities(ProductionCategory.Food), blamed);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD");

            //_sitReps_Temp.Add(new NewSabotagingSitRepEntry(
            //        attackingCiv, attackedCiv, colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed));

            _sitReps_Temp.Add(new NewSabotagedSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
            //UpdatingBlame(attackingCiv, attackedCiv, blamed);
        }
        public static void SabotageEnergy(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[colony.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int removeEnergyFacilities = -2;
            int defenseIntelligence = -2;

            if (attackingCiv == null)
                return;
            if (attackedCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == attackingCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            if (ratio < 2)
            {
                removeEnergyFacilities = -1;
                goto NoActionEnergy;
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of sabotage // value needed for SitRep
            GameLog.Core.Intel.DebugFormat("**** Before Sabotage Energy at {0}: TotalEnergyFacilities before={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy), blamed);
            //if ratio > 1 than remove one more  EnergyFacility
            if (ratio > 1 /*&& RandomHelper.Chance(4)*/ && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
            }

            //if ratio > 2 than remove one more  EnergyFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities += 1;  //  2 and one from before
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 3);
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (ratio > 20 && !RandomHelper.Chance(2) && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 4);
            }
            //_removedEnergyFacilities = removeEnergyFacilities;
            GameLog.Core.Intel.DebugFormat("**** After Sabotage Energy at {0}: TotalEnergyFacilities after={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy), blamed);
            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionEnergy:;  // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);


            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY");

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}, {2} blamed", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy), blamed);

            //_sitReps_Temp.Add(new NewSabotagingSitRepEntry(
            //        attackingCiv, attackedCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed));

            _sitReps_Temp.Add(new NewSabotagedSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;

            // UpdatingBlame(attackingCiv, attackedCiv, blamed);
        }
        public static void SabotageIndustry(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];
            //GameContext.Current.CivilizationManagers[attackedCiv].UpDateBlamedCiv(_blamedCiv);

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;

            GameLog.Core.Test.DebugFormat("**** Sabotage Industry, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            int removeIndustryFacilities = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (attackingCiv == null)
                return;
            if (attackedCiv == null)
                return;

            if (colony == null)
                return;

            int ratio = GetIntelRatio(attackedCivManager, attackingCivManager);
            if (ratio < 2)
            {
                removeIndustryFacilities = -1;
                goto NoActionIndustry;
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of sabotage // value needed for SitRep

            //if ratio > 1 than remove one more  IndustryFacility
            if (ratio > 1 /*&& !RandomHelper.Chance(2)*/ && colony.GetTotalFacilities(ProductionCategory.Industry) > 1)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 2);
            }

            //if ratio > 2 than remove one more  IndustryFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 2)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 3);
            }

            // if ratio > 3 than remove one more  IndustryFacility
            if (ratio > 20 && !RandomHelper.Chance(6) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 3)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
                blamed = IntelHelper.Blame(attackingCiv, blamed, 4);
            }

            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence / 4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionIndustry:;   // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackingCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY");

            GameLog.Core.Intel.DebugFormat("Sabotage Industry at {0}: TotalIndustryFacilities after={1}, {2} blamed", system.Name, colony.GetTotalFacilities(ProductionCategory.Industry), blamed);

            //_sitReps_Temp.Add(new NewSabotagingSitRepEntry(
            //        attackingCiv, attackedCiv, colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed));

            _sitReps_Temp.Add(new NewSabotagedSitRepEntry(
                    attackedCiv, attackingCiv, colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt += newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
            //  UpdatingBlame(attackingCiv, attackedCiv, blamed);
        }
        public static int GetIntelRatio(CivilizationManager attackedCivManager, CivilizationManager attackingCivManager)
        {
            int ratio = -1;
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[attackingCivManager].TotalIntelligenceAttackingAccumulated.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1 * attackingIntelligence;// multiplied with 1000 - just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
                ratio = 1;

            GameLog.Core.Intel.DebugFormat("Intelligence attacking = {0}, defense = {1}, Ratio = {2}", attackingIntelligence, defenseIntelligence, ratio);

            return ratio;
        }

        [Serializable]
        public class NewIntelOrders //(ICivilization civilization)  //(int, int, string)
        {
            private int _attackingCivID; // = 999;
            private int _attackedCivID; //= 999;
            private string _intel_Order; // = "Dummy_Intel_Order";
            private string _intel_Order_Blamed; // = "Dummy_Intel_Order_Blamed";

            //private int sCiv;
            //private int aCiv;
            //private string order;

            //public NewIntelOrders(int sCiv, int aCiv, string order)
            //{
            //    this.sCiv = sCiv;
            //    this.aCiv = aCiv;
            //    this.order = order;
            //}


            public NewIntelOrders(int attackingCivID, int attackedCivID, string intelOrder, string intelOrderBlamed)
            {
                _attackingCivID = attackingCivID;
                _attackedCivID = attackedCivID;
                _intel_Order = intelOrder;
                _intel_Order_Blamed = intelOrderBlamed;
            }

            public int AttackingCivID {
                get
                {
                    //var _DummyattackingCivID = 999;
                    //if (_attackingCivID == null)
                    //    _attackingCivID = 999;

                    return _attackingCivID; 
                }
                set
                {
                    if (_attackingCivID != 999)
                        _attackingCivID = value; 
                }
            }

            public int AttackedCivID
            {
                get
                {
                    //var _DummyattackedCivID = 999;
                    //if (_attackedCivID == null)
                    //    _attackedCivID = 999;

                    return _attackedCivID;
                }
                set
                {
                    if (_attackedCivID != 999)
                        _attackedCivID = value;
                }
            }
            public string Intel_Order 
            {
                get
                {
                    //var _DummyattackedCivID = 999;
                    //if (_intel_Order == null)
                    //    _intel_Order = "Dummy_Intel_Order was null";

                    return _intel_Order;
                }
                set
                {
                    //if (_intel_Order != null)
                        _intel_Order = value;
                }
            }

            public string Intel_Order_Blamed
            {
                get
                {
                    //var _DummyattackedCivID = 999;
                    //if (_intel_Order_Blamed == null)
                    //    _intel_Order_Blamed = "Dummy_Intel_Order_Blamed was null";

                    return _intel_Order_Blamed;
                }
                set
                {
                    //if (_intel_Order_Blamed != null)
                        _intel_Order_Blamed = value;
                }
            }

        }
        //public void CreateNewIntelOrders(int sCiv, int aCiv, string order)
        //    {
        //    var _newOrder = new NewIntelOrders(sCiv, aCiv, order);
        //    }
    }
    #endregion
}
