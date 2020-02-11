using Supremacy.Collections;
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
        //private static Dictionary<Civilization, List<Civilization>> _spiedDictionary = new Dictionary<Civilization, List<Civilization>>();
        private static List<Civilization> _spiedList = new List<Civilization>();
        private static List<Civilization> _localSpiedList = new List<Civilization>();
        private static Dictionary<Civilization, int> _defenseDictionary = new Dictionary<Civilization, int>();
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
   
        public static Dictionary<Civilization, int> DefenceDictionary
        {
            get { return _defenseDictionary; }
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
        public static bool CheckingInstallSpy_0()
        {
            return _installingSpy_0;
        }
        public static void SendXSpiedY(Civilization spyCiv, Civilization spiedCiv, UniverseObjectList<Colony> colonies)
        {
            _installingSpy_0 = true;
            GameLog.Core.UI.DebugFormat("**** New spyciv = {0} spying on = {1}",spyCiv.Key, spiedCiv.Key);
            if (spyCiv == null)
                throw new ArgumentNullException("spyCiv");
            if (spiedCiv == null)
                throw new ArgumentNullException("spiedCiv");
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
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_0_List);
                    }
                    break;
                case 1:
                    if (_spyingCiv_1_List == null)
                        _spyingCiv_1_List = newList;
                    else
                    {
                        _spyingCiv_1_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_1_List);
                    }
                    break;
                case 2:
                    if (_spyingCiv_2_List == null)
                        _spyingCiv_2_List = newList;
                    else
                    {
                        _spyingCiv_2_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_2_List);
                    }
                    break;
                case 3:
                    if (_spyingCiv_3_List == null)
                        _spyingCiv_3_List = newList;
                    else
                    {
                        _spyingCiv_3_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_3_List);
                    }
                    break;
                case 4:
                    if (_spyingCiv_4_List == null)
                        _spyingCiv_4_List = newList;
                    else
                    {
                        _spyingCiv_4_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_4_List);
                    }
                    break;
                case 5:
                    if (_spyingCiv_5_List == null)
                        _spyingCiv_5_List = newList;
                    else
                    {
                        _spyingCiv_5_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_5_List);
                    }
                    break;
                case 6:
                    if (_spyingCiv_6_List == null)
                        _spyingCiv_6_List = newList;
                    else
                    {
                        _spyingCiv_6_List.Add(spiedCiv);
                        GameContext.Current.CivilizationManagers[spyCiv].UpDateSpiedList(_spyingCiv_6_List);
                    }
                    break;
            }
            GameLog.Client.UI.DebugFormat("********* end of sending spied list to CM **********");
            PopulateDefence();
        }
        private static void PopulateDefence()
        {
            _defenseDictionary.Clear();
            foreach (var spiedCiv in _spiedList)
            {
                int defenseInt = 0;      
                var spiedCivManager = GameContext.Current.CivilizationManagers[spiedCiv];
                Int32.TryParse(spiedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseInt);
                _defenseDictionary.Add(spiedCiv, defenseInt);
            }
        }
        
        #region Espionage Methods

        public static void StealCredits(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];

            var attackingCivManager = GameContext.Current.CivilizationManagers[attackingCiv];

            Meter defenseMeter = GameContext.Current.CivilizationManagers[attackedCiv].TotalIntelligenceDefenseAccumulated;
            //var attackMeter = AssetsScreenPresentationModel.UpdateAttackingAccumulated(attackingCiv);
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int stolenCredits = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (blamed == "No one" || blamed == "Terrorists")
            {
                blamed = attackingCivManager.Civilization.ShortName;
                if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_Terrorists";
                }
                else if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_No one";
                }
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);
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
                goto stolenCreditsIsMinusOne;
            }

            stolenCredits = stolenCredits / 100 * 3;  // default 3 percent

            if (!RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 5)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                //SeeStealCredits(_newTargetCiv, "Clicked");
                stolenCredits = stolenCredits * 2; // 2 percent of their TOTAL Credits - not just income
            }
            if (ratio > 10 && !RandomHelper.Chance(3) && attackedCivManager.Treasury.CurrentLevel > 20) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                if (!RandomHelper.Chance(3))
                {
                    //SeeStealCredits(_newTargetCiv, "Clicked");
                    stolenCredits = stolenCredits * 3;
                }
            }
            if (ratio > 20 && !RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
               // SeeStealCredits(_newTargetCiv, "Clicked");
                stolenCredits = stolenCredits * 2;
            }
            GameLog.Core.UI.DebugFormat("**** CREDITS, The attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.UI.DebugFormat("BEFORE: attackED civ={0}: {1} Credits", attackedCiv.Key, GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue);

            GameContext.Current.CivilizationManagers[attackedCiv].Credits.AdjustCurrent(stolenCredits * -1);
            GameContext.Current.CivilizationManagers[attackedCiv].Credits.UpdateAndReset();

            GameLog.Core.UI.DebugFormat(" AFTER: attackED civ={0}: {1} Credits", attackedCiv.Key, GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue);
            GameLog.Core.UI.DebugFormat("attacing accumulated ={0}", _attackAccumulatedIntelInt);

            GameLog.Core.UI.DebugFormat("BEFORE: attackING civ={0}: {1} Credits", attackingCiv.Key, GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue);

            GameContext.Current.CivilizationManagers[attackingCiv].Credits.AdjustCurrent(stolenCredits);
            GameContext.Current.CivilizationManagers[attackingCiv].Credits.UpdateAndReset();

            GameLog.Core.UI.DebugFormat(" AFTER: attakING civ={0}: {1} Credits", attackingCiv.Key, GameContext.Current.CivilizationManagers[attackingCiv].Credits.CurrentValue);
            GameLog.Core.UI.DebugFormat("attacing accumulated ={0}", _attackAccumulatedIntelInt);

            // DEFENSE

            GameLog.Core.UI.DebugFormat("** DEFENSE METER INT, attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.UI.DebugFormat("BEFORE: attackED civ={0}: defense meter {1}, accumulated ={2}",
                    attackedCiv.Key, defenseMeter.CurrentValue, DefenseAccumulatedInteInt);
            GameLog.Core.UI.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);
            //GameLog.Core.UI.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
            //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);

            defenseMeter.AdjustCurrent(defenseIntelligence /4 * -1);
            defenseMeter.UpdateAndReset();

            GameLog.Core.UI.DebugFormat(" AFTER: attackED civ={0}: defense meter {1}, accumulated ={2}",
                    attackedCiv.Key, defenseMeter.CurrentValue, DefenseAccumulatedInteInt);
            GameLog.Core.UI.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);
        //GameLog.Core.UI.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
        //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue);

        //  ATTACKING

        stolenCreditsIsMinusOne:;   // pushing buttons makes 'intel costs'

            GameLog.Core.UI.DebugFormat("** ATTACK METER INT attakING Spy Civ={0} the attackED civ={1}", attackingCiv.Key, attackedCiv.Key);
            GameLog.Core.UI.DebugFormat("BEFORE: attackED civ={0}: attackED meter {1}, accumulated ={2}",
                    attackedCiv.Key, attackMeter.CurrentValue, AttackingAccumulatedInteInt);
            GameLog.Core.UI.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);
            //GameLog.Core.UI.DebugFormat("Before: attackING civ={0}, local civ={1}, GameContest * accululated ={1}",
            //    attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].Credits.CurrentValue);

            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // divided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.UI.DebugFormat(" AFTER: attackED civ={0}: attackED meter {1}, accumulated ={2}",
                    attackedCiv.Key, attackMeter.CurrentValue, AttackingAccumulatedInteInt);
            GameLog.Core.UI.DebugFormat(" After: attackING civ={0}, local civ={1}, GameContest * accululated ={2}",
                attackingCiv.Key, _localCivManager.Civilization.Key, GameContext.Current.CivilizationManagers[_localCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED");

            Int32.TryParse(GameContext.Current.CivilizationManagers[attackedCiv].Credits.CurrentValue.ToString(), out int newCreditsAttacked);

            GameLog.Core.UI.DebugFormat("Stolen Credits from {0}:  >>> {1} Credits", colony.Name, stolenCredits);

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                attackedCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackedCiv"));
            //attackedCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    attackedCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackedCiv"));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                attackingCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackingCiv"));
            //attackingCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    _newSpyCiv, colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackingCiv"));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
        }
        public static void StealResearch(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;
            int stolenResearchPoints = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (blamed == "No one" || blamed == "Terrorists")
            {
                blamed = attackingCivManager.Civilization.ShortName;
                if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_Terrorists";
                }
                else if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_No one";
                }
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);
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
                goto stolenResearchPointsIsMinusOne;
            }

            stolenResearchPoints = stolenResearchPoints / 100 * 1; // default JUST 1 percent

            //if (ratio < 2) stolenResearchPoints = 0; // ratio = 1 or less: no success 

            if (ratio > 1 && !RandomHelper.Chance(2)) // (Cumulative is meter) && attackedCivManager.Research.CumulativePoints > 10)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                // ToDo add to local player           
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 2;  // 2 percent, but base is CumulativePoints, so all research points ever yielded
            }
            if (ratio > 10 && !RandomHelper.Chance(4))// && attackedCivManager.Treasury.CurrentLevel > 40) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 3;  
            }
            if (ratio > 20 && !RandomHelper.Chance(8))// && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                //SeeStealResearch(_newTargetCiv, "Clicked");
                stolenResearchPoints = stolenResearchPoints * 2;  
            }



            GameLog.Core.Intel.DebugFormat("Research ** BEFORE ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.CumulativePoints);

            // result   // only attackingCiv = _newSpyCiv is getting a plus of research points
            if (stolenResearchPoints > 0)
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.UpdateResearch(stolenResearchPoints);


            GameLog.Core.Intel.DebugFormat("Research ** AFTER ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.CumulativePoints);


            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence /4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////

            stolenResearchPointsIsMinusOne:;  // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED"); 

            GameLog.Core.Intel.DebugFormat("Research stolen at {0}: {1} stolenResearchPoints", system.Name, stolenResearchPoints);

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Research.CumulativePoints.ToString(), out int newResearchCumulative);

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(_newSpyCiv, system.Colony, 
                    affectedField, stolenResearchPoints,
                    newResearchCumulative, blamed, "attackingCiv"));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;

            // no info to attacked civ
        }
        public static void SabotageFood(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;
            int removeFoodFacilities = -2;  // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (blamed == "No one" || blamed == "Terrorists")
            {
                blamed = "bl_" + attackingCivManager.Civilization.ShortName;
                if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_Terrorists";
                }
                else if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_No one";
                }
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);

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
            }

            //if ratio > 2 than remove one more  FoodFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 2)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            // if ratio > 3 than remove one more  FoodFacility
            if (ratio > 20 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 3)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence /4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionFood:;   // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            GameLog.Core.Intel.DebugFormat("Sabotage Food at {0}: TotalFoodFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Food));

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD");

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed, "attackedCiv"));

            //attackedCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    attackedCiv, system.Colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed, "attackedCiv"));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                _newSpyCiv, colony, affectedField, removeFoodFacilities, colony.GetTotalFacilities(ProductionCategory.Food), blamed, "attackingCiv"));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
        }
        public static void SabotageEnergy(Colony colony, Civilization attackedCiv, string blamed)
        {
            //var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[colony.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;
            int removeEnergyFacilities = -2;
            int defenseIntelligence = -2;

            if (blamed == "No one" || blamed == "Terrorists")
            {
                blamed = attackingCivManager.Civilization.ShortName;
                if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_Terrorists";
                }
                else if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_No one";
                }
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);
            if (ratio < 2)
            {
                removeEnergyFacilities = -1;
                goto NoActionEnergy;
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of sabotage // value needed for SitRep

            //if ratio > 1 than remove one more  EnergyFacility
            if (ratio > 1 /*&& RandomHelper.Chance(4)*/ && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            //if ratio > 2 than remove one more  EnergyFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities += 1;  //  2 and one from before
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (ratio > 20 && !RandomHelper.Chance(2) && colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }
            //_removedEnergyFacilities = removeEnergyFacilities;

            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence /4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionEnergy:;  // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);



            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY"); 

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}", colony.Name, colony.GetTotalFacilities(ProductionCategory.Energy));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                attackedCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackedCiv"));
            //attackedCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    attackedCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackedCiv"));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                _newSpyCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackingCiv"));
            //attackingCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    _newSpyCiv, colony, affectedField, removeEnergyFacilities, colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackingCiv"));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt = newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
        }
        public static void SabotageIndustry(Colony colony, Civilization attackedCiv, string blamed)
        {
            //GameLog.Core.Intel.DebugFormat("##### Sabotage Industry not implemented yet");
            //GameLog.Core.UI.DebugFormat("IntelHelper SabotageIndustry at line 390");

            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;
            int removeIndustryFacilities = -2; // -1 = failed, -2 = not worth
            int defenseIntelligence = -2;

            if (blamed == "No one" || blamed == "Terrorists")
            {
                blamed = attackingCivManager.Civilization.ShortName;
                if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_Terrorists";
                }
                else if (!RandomHelper.Chance(4))
                {
                    blamed = "bl_No one";
                }
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;
            int ratio = GetIntelRatio(attackedCivManager);
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
            }

            //if ratio > 2 than remove one more  IndustryFacility
            if (ratio > 10 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 2)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            // if ratio > 3 than remove one more  IndustryFacility
            if (ratio > 20 && !RandomHelper.Chance(6) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 3)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities += 1;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            // handling intelligence points for attack / defence  //////////////////////////7
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);
            defenseMeter.AdjustCurrent(defenseIntelligence /4 * -1);
            defenseMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("defenseMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[attackedCiv].Civilization.Key,
                    defenseMeter.CurrentValue);

            //////////////////////////////////////////7

            NoActionIndustry:;   // pushing buttons makes 'intel costs'

            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** BEFORE ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);
            attackMeter.AdjustCurrent(defenseIntelligence / 2 * -1); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("attackMeter.Adjust ** AFTER ** from {0}:  >>> {1} intelligence points",
                    GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                    attackMeter.CurrentValue);

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY");

            GameLog.Core.Intel.DebugFormat("Sabotage Industry at {0}: TotalIndustryFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Industry));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackedCiv"));
            //attackedCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    attackedCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackedCiv"));

            _sitReps_Temp.Add(new NewSabotageSitRepEntry(
                _newSpyCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackingCiv"));
            //attackingCivManager.SitRepEntries_Temp.Add(new NewSabotageSitRepEntry(
            //    _newSpyCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackingCiv"));

            int newDefenseIntelligence = 0;
            Int32.TryParse(defenseMeter.CurrentValue.ToString(), out newDefenseIntelligence);
            _defenseAccumulatedIntelInt += newDefenseIntelligence;

            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _attackAccumulatedIntelInt = newAttackIntelligence;
        }
        public static int GetIntelRatio(CivilizationManager attackedCivManager)
        {
            int ratio = -1;
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceAttackingAccumulated.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1 * attackingIntelligence;// multiplied with 1000 - just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence; 
            if (ratio < 2)
                ratio = 1;

            GameLog.Core.Intel.DebugFormat("Intelligence attacking = {0}, defense = {1}, Ratio = {2}", attackingIntelligence, defenseIntelligence, ratio);

            return ratio;
        }
    }
        #endregion
}
