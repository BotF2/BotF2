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
        private static List<EspionageAlreadyPressed> alreadyPressedList = new List<EspionageAlreadyPressed>();
        private static Dictionary<Civilization, List<Civilization>> _spiedDictionary = new Dictionary<Civilization, List<Civilization>>();
        private static List<Civilization> _spiedList = new List<Civilization>();

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
        public static Dictionary<Civilization, List<Civilization>> SpiedDictionary
        {
            get { return _spiedDictionary; }
        }
        public static void SendXSpiedY(Civilization spyCiv, Civilization spiedCiv, UniverseObjectList<Colony> colonies)
        { GameLog.Core.UI.DebugFormat("IntelHelper SendXSpiedY at line 35");
            if (spyCiv == null)
                throw new ArgumentNullException("spyCiv");
            if (spiedCiv == null)
                throw new ArgumentNullException("spiedCiv");
            _spiedList.Clear();
            _newSpyCiv = spyCiv;
            _newTargetCiv = spiedCiv;
            _newSpiedColonies = colonies;
            try
            {

                _spiedDictionary[spyCiv].Add(spiedCiv);
            }
            catch
            {
                _spiedDictionary[spyCiv] = new List<Civilization> { spiedCiv };

            }
        }
        #region Espionage Methods
        public static bool SeeStealCredits(Civilization spied, string whenAsked)
        {
            bool seeIt = true;
            if (whenAsked == "NotClicked")
            {
                //var attackedCivManager = GameContext.Current.CivilizationManagers[spied];

                //int ratio = GetIntelRatio(attackedCivManager);
                //if (ratio > 1)
                //    seeIt = true;
                return seeIt;
            }
            else { seeIt = false; }
            return seeIt;

        }

        public static bool SeeStealResearch(Civilization spied)
        {
            bool seeIt = false;
            var attackedCivManager = GameContext.Current.CivilizationManagers[spied];

            int ratio = GetIntelRatio(attackedCivManager);
            if (ratio > 1)
                seeIt = true;
            return seeIt;
        }
        public static bool SeeSabotageFood(Civilization spied)
        {
            bool seeIt = false;
            var attackedCivManager = GameContext.Current.CivilizationManagers[spied];

            int ratio = GetIntelRatio(attackedCivManager);
            if (ratio > 1)
                seeIt = true;
            return seeIt;
        }
        public static bool SeeSabotageIndustry(Civilization spied)
        {
            bool seeIt = false;
            var attackedCivManager = GameContext.Current.CivilizationManagers[spied];

            int ratio = GetIntelRatio(attackedCivManager);
            if (ratio > 1)
                seeIt = true;
            return seeIt;
        }
        public static bool SeeSabotageEnergy(Civilization spied)
        {
            bool seeIt = false;
            var attackedCivManager = GameContext.Current.CivilizationManagers[spied];

            int ratio = GetIntelRatio(attackedCivManager);
            if (ratio > 1)
                seeIt = true;
            return seeIt;
        }
      
        public static void StealCredits(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;

            //   #### we are using incoming text for blamed
            //switch (blamed)
            //{
            //    default:
            //        blamed = ResourceManager.GetString("NOBODY_TO_BLAME");
            //        break;
            //}

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Credits", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("alreadyPressedList-Entry: {0},{1},{2},", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of steal // value needed for SitRep
            //int removeChredits = 0;
            Int32.TryParse(attackedCivManager.Credits.ToString(), out int stolenCredits);
            int attackedCreditsBefore = stolenCredits;


            if (stolenCredits < 100)  // they have not enough credits being worth to be sabotaged, especially avoid negative stuff !!!
            {
                stolenCredits = -1;
                goto stolenCreditsIsMinusOne;
            }

            stolenCredits = stolenCredits / 100 * 3;

            if (!RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 5)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                SeeStealCredits(_newTargetCiv, "Clicked");
                stolenCredits = stolenCredits * 2; // 2 percent of their TOTAL Credits - not just income
            }
            if (ratio > 10 && !RandomHelper.Chance(3) && attackedCivManager.Treasury.CurrentLevel > 20) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                if (!RandomHelper.Chance(3))
                {
                    SeeStealCredits(_newTargetCiv, "Clicked");
                    stolenCredits = stolenCredits * 3;
                }
            }
            if (ratio > 20 && !RandomHelper.Chance(2) && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                SeeStealCredits(_newTargetCiv, "Clicked");
                stolenCredits = stolenCredits * 2;
            }

            GameLog.Core.Intel.DebugFormat("Credits system.Owner BEFORE from {0}:  >>> {1} Credits", system.Owner, GameContext.Current.CivilizationManagers[system.Owner].Credits.CurrentValue);
            // result 
            GameContext.Current.CivilizationManagers[system.Owner].Credits.AdjustCurrent(stolenCredits * -1);
            GameContext.Current.CivilizationManagers[system.Owner].Credits.UpdateAndReset();
            GameLog.Core.Intel.DebugFormat("Credits system.Owner AFTER from {0}:  >>> {1} Credits", system.Owner, GameContext.Current.CivilizationManagers[system.Owner].Credits.CurrentValue);


            GameLog.Core.Intel.DebugFormat("Credits SPY CIV BEFORE from {0}:  >>> {1} Credits", 
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key, 
                GameContext.Current.CivilizationManagers[_newSpyCiv].Credits.CurrentValue);

            GameContext.Current.CivilizationManagers[_newSpyCiv].Credits.AdjustCurrent(stolenCredits);
            GameContext.Current.CivilizationManagers[_newSpyCiv].Credits.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat("Credits SPY CIV AFTER from {0}:  >>> {1} Credits",
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[_newSpyCiv].Credits.CurrentValue);


            // costs on both sides for the attack
            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            stolenCreditsIsMinusOne:;  // in this case give SitRep: nothing to gain

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED");

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Credits.CurrentValue.ToString(), out int newCreditsAttacked);

            GameLog.Core.Intel.DebugFormat("Stolen Credits from {0}:  >>> {1} Credits", system.Name, stolenCredits);

            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackedCiv"));
            attackingCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, stolenCredits, newCreditsAttacked, blamed, "attackingCiv"));

        }
        public static void StealResearch(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;
            Meter stolenResearchPoints;

            switch (blamed)
            {
                default:
                    blamed = ResourceManager.GetString("NOBODY_TO_BLAME");
                    break;
            }

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Research", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("now pressed: {2}, but alreadyPressedList-Entry: {0},{1}", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of steal // value needed for SitRep

            // calculation stolen research points depended on defenders stuff

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Research.CumulativePoints.ToString(), out int researchPoints);
            int attackedResearchCumulativePoints = researchPoints;

            // ToDO: do it like credits... start with base value of 2%, and then random chance and ratio to increase this....
            //    ... instead of DEcreasing it once and once again.. and again...

            if (ratio > 1 && !RandomHelper.Chance(2)) // (Cumulative is meter) && attackedCivManager.Research.CumulativePoints > 10)// Credit everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                // ToDo add to local player              
                researchPoints = researchPoints / 100 * 2;  // 3 percent, but base is CumulativePoints, so all research points ever yielded
            }
            if (ratio > 2 && !RandomHelper.Chance(4))// && attackedCivManager.Treasury.CurrentLevel > 40) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                researchPoints = researchPoints / 100 * 4;  
            }
            if (ratio > 3 && !RandomHelper.Chance(8))// && attackedCivManager.Treasury.CurrentLevel > 100) // Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                researchPoints = researchPoints / 100 * 6;  
            }


            GameLog.Core.Intel.DebugFormat("Research SPY CIV ** BEFORE ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.CumulativePoints);

            // result
            if (researchPoints > 0)
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.UpdateResearch(researchPoints);


            GameLog.Core.Intel.DebugFormat("Research SPY CIV ** AFTER ** from {0}:  >>> {1} Research",
                GameContext.Current.CivilizationManagers[_newSpyCiv].Civilization.Key,
                GameContext.Current.CivilizationManagers[_newSpyCiv].Research.CumulativePoints);


            // handling intelligence points for attack / defence
            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();


            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED"); 

            GameLog.Core.Intel.DebugFormat("Research stolen at {0}: {1} ResearchPoints", system.Name, researchPoints);

            Int32.TryParse(GameContext.Current.CivilizationManagers[system.Owner].Research.CumulativePoints.ToString(), out int newResearchCumulative);

            attackingCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, researchPoints,
                newResearchCumulative, blamed, "attackingCiv"));
            // no info to attacked civ
        }
        public static void SabotageFood(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;

            switch (blamed)
            {
                default:
                    blamed = ResourceManager.GetString("NOBODY_TO_BLAME");
                    break;
            }

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Food", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("now pressed: {2}, but alreadyPressedList-Entry: {0},{1}", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }
            //int ratio = -1;
            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Food={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.NetFood,
                system.Colony.GetActiveFacilities(ProductionCategory.Food),
                system.Colony.GetTotalFacilities(ProductionCategory.Food));
            GameLog.Core.Intel.DebugFormat("Sabotage Food to {0}: TotalFoodFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Food));

            //Effect of sabotage // value needed for SitRep
            int removeFoodFacilities = 0;

            //if ratio > 1 than remove one more  FoodFacility
            if (ratio > 1 && !RandomHelper.Chance(2) && colony.GetTotalFacilities(ProductionCategory.Food) > 1)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            //if ratio > 2 than remove one more  FoodFacility
            if (ratio > 2 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 2)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            // if ratio > 3 than remove one more  FoodFacility
            if (ratio > 3 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 3)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Food, 2);
            }

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD"); 

            GameLog.Core.Intel.DebugFormat("Sabotage Food at {0}: TotalFoodFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Food));

            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed, "attackedCiv"));
            
            attackingCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food), blamed, "attackingCiv"));
        }
        public static void SabotageEnergy(Colony colony, Civilization attackedCiv, string blamed)
        {
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;

            switch (blamed)
            {
                default:
                    blamed = ResourceManager.GetString("NOBODY_TO_BLAME");
                    break;
            }

            // avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0)
                if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber)
                    alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCiv.ToString() + ";Energy", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("now pressed: {2}, but alreadyPressedList-Entry: {0},{1}", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;

            int ratio = GetIntelRatio(attackedCivManager);

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of sabotage // value needed for SitRep
            int removeEnergyFacilities = 0;

            //if ratio > 1 than remove one more  EnergyFacility
            if (ratio > 1 && RandomHelper.Chance(4) && colony.GetTotalFacilities(ProductionCategory.Energy) > 5)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            //if ratio > 2 than remove one more  EnergyFacility
            if (ratio > 2 && !RandomHelper.Chance(2) && system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (ratio > 3 && !RandomHelper.Chance(2) && system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 2);
            }

            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY"); 

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Energy));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackedCiv"));
            attackingCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy), blamed, "attackingCiv"));

        }

        public static void SabotageIndustry(Colony colony, Civilization attackedCiv, string blamed)
        {
            //GameLog.Core.Intel.DebugFormat("##### Sabotage Industry not implemented yet");
            GameLog.Core.UI.DebugFormat("IntelHelper SabotageIndustry at line 390");

            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            var attackingCivManager = GameContext.Current.CivilizationManagers[_newSpyCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[_newSpyCiv].TotalIntelligenceAttackingAccumulated;

            switch (blamed)
            {
                default:
                    blamed = ResourceManager.GetString("NOBODY_TO_BLAME");
                    break;
            }

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Industry", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("now pressed: {2}, but alreadyPressedList-Entry: {0},{1}", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }

            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;
            int ratio = GetIntelRatio(attackedCivManager);

            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            //Effect of sabotage // value needed for SitRep
            int removeIndustryFacilities = 0;

            //if ratio > 1 than remove one more  IndustryFacility
            if (ratio > 1 && !RandomHelper.Chance(2) && colony.GetTotalFacilities(ProductionCategory.Industry) > 1)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            //if ratio > 2 than remove one more  IndustryFacility
            if (ratio > 2 && !RandomHelper.Chance(4) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 2)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            // if ratio > 3 than remove one more  IndustryFacility
            if (ratio > 3 && !RandomHelper.Chance(6) && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 3)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            string affectedField = ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY");

            GameLog.Core.Intel.DebugFormat("Sabotage Industry at {0}: TotalIndustryFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Industry));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackedCiv"));
            attackingCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(
                attackedCiv, system.Colony, affectedField, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry), blamed, "attackingCiv"));

        }
        public static int GetIntelRatio(CivilizationManager attackedCivManager)
        {
            int ratio = -1;
            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceProduction.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1000 * attackingIntelligence;// just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence; 
            if (ratio < 2)
                ratio = 1;

            return ratio;
        }
    }
        #endregion

    public class EspionageAlreadyPressed
    {

        public string alreadyPressedEntry;
        public int turnNumber;

        public EspionageAlreadyPressed(string _alreadyPressedEntry, int _turnNumber)
        {
            alreadyPressedEntry = _alreadyPressedEntry;
            turnNumber = _turnNumber;
        }
    }
    
}
