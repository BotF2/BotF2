using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
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
        {   GameLog.Core.UI.DebugFormat("IntelHelper SendXSpiedY at line 35");
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
                _spiedDictionary[spyCiv] = new List<Civilization> {spiedCiv};

            }
            //foreach (var aSpyCivKey in _spiedDictionary.Keys)
            //{
            //    foreach (var aSpiedCivValue in _spiedDictionary[aSpyCivKey])
            //    {
            //        GameLog.Client.UI.DebugFormat("********* Dictionary Key ={1} spied Civ ={0}", aSpiedCivValue.Key, aSpyCivKey.Key);
            //    }
            //}
  
        }
        #region Espionage Methods

        public static void SabotageEnergy(Colony colony, Civilization attackedCiv)
        {
            GameLog.Core.UI.DebugFormat("IntelHelper SabotageEnergy at line 76");
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[attackedCiv];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceAttackingAccumulated;

            // avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0)
                if(alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber)
                    alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCiv.ToString() + ";Energy", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("alreadyPressedList-Entry: {0},{1},{2},", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                    GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                    return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }


            int ratio = -1;
            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;


              Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;
                
            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceProduction.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)  
                attackingIntelligence = 1;
                    
            attackingIntelligence = 1000 * attackingIntelligence;// just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
                ratio = 1;   // we start with sabotage with ratio more than one, not before

            //GameLog.Core.Intel.DebugFormat("Sabotage Energy to {0}: defense={1}, attacking={2}, ratio={3}",
            //    system.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) is SABOTAGED by {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
                system.Owner, system.Name, NewSpyCiv.Name, defenseIntelligence, attackingIntelligence, ratio);


            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Energy={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.NetEnergy,
                system.Colony.GetActiveFacilities(ProductionCategory.Energy),
                system.Colony.GetTotalFacilities(ProductionCategory.Energy));
            GameLog.Core.Intel.DebugFormat("Sabotage Energy to {0}: TotalEnergyFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Energy));

            //Effect of sabotage // value needed for SitRep
            int removeEnergyFacilities = 0;                  

            //if ratio > 1 than remove one more  EnergyFacility
            if (ratio > 1 && colony.GetTotalFacilities(ProductionCategory.Energy) > 1)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            //if ratio > 2 than remove one more  EnergyFacility
            if (ratio > 2 && system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (ratio > 3 && system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 3)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 1);
            }


            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat("Sabotage Energy at {0}: TotalEnergyFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Energy));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(attackedCiv, system.Colony, removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy)));

        }

        public static void SabotageFood(Colony colony, Civilization attackedCiv)
        {
            GameLog.Core.UI.DebugFormat("IntelHelper SabotageFood at line 180");
            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceDefenseAccumulated;

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Food", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("alreadyPressedList-Entry: {0},{1},{2},", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }


            int ratio = -1;
            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;


            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceProduction.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1000 * attackingIntelligence;// just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
                ratio = 1;   // we start with sabotage with ratio more than one, not before

            //GameLog.Core.Intel.DebugFormat("Sabotage Energy to {0}: defense={1}, attacking={2}, ratio={3}",
            //    system.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) is SABOTAGED by {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
                system.Owner, system.Name, attackedCiv.Name, defenseIntelligence, attackingIntelligence, ratio);


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
            if (ratio > 1 && colony.GetTotalFacilities(ProductionCategory.Food) > 1)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            //if ratio > 2 than remove one more  FoodFacility
            if (ratio > 2 && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 2)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
            }

            // if ratio > 3 than remove one more  FoodFacility
            if (ratio > 3 && system.Colony.GetTotalFacilities(ProductionCategory.Food) > 3)// Food: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeFoodFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Food, 1);
            }


            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat("Sabotage Food at {0}: TotalFoodFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Food));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(attackedCiv, system.Colony, removeFoodFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Food)));

        }

        public static void StealResearch(Colony colony, Civilization attackedCiv)
        {
            //GameLog.Core.Intel.DebugFormat("##### StealResearch not implemented yet");
            GameLog.Core.UI.DebugFormat("IntelHelper SabotageResearch at line 285");

            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceDefenseAccumulated;

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Research", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("alreadyPressedList-Entry: {0},{1},{2},", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }


            int ratio = -1;
            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;


            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceProduction.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1000 * attackingIntelligence;// just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
                ratio = 1;   // we start with sabotage with ratio more than one, not before

            //GameLog.Core.Intel.DebugFormat("Sabotage Energy to {0}: defense={1}, attacking={2}, ratio={3}",
            //    system.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) is SABOTAGED by {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
                system.Owner, system.Name, attackedCiv.Name, defenseIntelligence, attackingIntelligence, ratio);


            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Research={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.NetResearch,
                system.Colony.GetActiveFacilities(ProductionCategory.Research),
                system.Colony.GetTotalFacilities(ProductionCategory.Research));
            GameLog.Core.Intel.DebugFormat("Sabotage Research to {0}: TotalResearchFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Research));

            //Effect of sabotage // value needed for SitRep
            int removeResearchFacilities = 0;

            //if ratio > 1 than remove one more  ResearchFacility
            if (ratio > 1 && colony.GetTotalFacilities(ProductionCategory.Research) > 1)// Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeResearchFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Research, 1);
            }

            //if ratio > 2 than remove one more  ResearchFacility
            if (ratio > 2 && system.Colony.GetTotalFacilities(ProductionCategory.Research) > 2)// Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeResearchFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Research, 1);
            }

            // if ratio > 3 than remove one more  ResearchFacility
            if (ratio > 3 && system.Colony.GetTotalFacilities(ProductionCategory.Research) > 3)// Research: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeResearchFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Research, 1);
            }


            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat("Sabotage Research at {0}: TotalResearchFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Research));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(attackedCiv, system.Colony, removeResearchFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Research)));

        }
        public static void SabotageIndustry(Colony colony, Civilization attackedCiv)
        {
            //GameLog.Core.Intel.DebugFormat("##### Sabotage Industry not implemented yet");
            GameLog.Core.UI.DebugFormat("IntelHelper SabotageIndustry at line 390");

            var system = colony.System;
            var attackedCivManager = GameContext.Current.CivilizationManagers[colony.System.Owner];
            Meter defenseMeter = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceDefenseAccumulated;
            Meter attackMeter = GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceDefenseAccumulated;

            // stuff to avoid doing Sabotage multiple times if buttons are pressed multiple time
            if (alreadyPressedList.Count > 0) if (alreadyPressedList[0].turnNumber < GameContext.Current.TurnNumber) alreadyPressedList.Clear(); // clear old list from previous turns
            EspionageAlreadyPressed pressedNew = new EspionageAlreadyPressed(NewSpyCiv.ToString() + " VS " + attackedCivManager.Civilization.ToString() + ";Industry", GameContext.Current.TurnNumber);

            int apINT = -1;
            apINT = alreadyPressedList.FindIndex(item => item.alreadyPressedEntry == pressedNew.alreadyPressedEntry);
            if (apINT > -1)
            {
                GameLog.Client.Intel.DebugFormat("alreadyPressedList-Entry: {0},{1}", alreadyPressedList[apINT].turnNumber, alreadyPressedList[apINT].alreadyPressedEntry, pressedNew.alreadyPressedEntry);
                GameLog.Client.Intel.DebugFormat("this sabotage button was pressed before in this turn ... nothing happens...");
                return;
            }
            else
            {
                alreadyPressedList.Add(pressedNew);
            }


            int ratio = -1;
            if (NewSpyCiv == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == NewSpyCiv.CivID);
            if (ownedByPlayer)
                return;


            Int32.TryParse(attackedCivManager.TotalIntelligenceDefenseAccumulated.ToString(), out int defenseIntelligence);  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            Int32.TryParse(GameContext.Current.CivilizationManagers[NewSpyCiv].TotalIntelligenceProduction.ToString(), out int attackingIntelligence);  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            attackingIntelligence = 1000 * attackingIntelligence;// just for increase attacking Intelligence

            ratio = attackingIntelligence / defenseIntelligence;
            if (ratio < 2)
                ratio = 1;   // we start with sabotage with ratio more than one, not before

            //GameLog.Core.Intel.DebugFormat("Sabotage Industry to {0}: defense={1}, attacking={2}, ratio={3}",
            //    system.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) is SABOTAGED by {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
                system.Owner, system.Name, attackedCiv.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("{1} ({0}) at {2} (sabotaged): Industry={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.NetIndustry,
                system.Colony.GetActiveFacilities(ProductionCategory.Industry),
                system.Colony.GetTotalFacilities(ProductionCategory.Industry));
            GameLog.Core.Intel.DebugFormat("Sabotage Industry to {0}: TotalIndustryFacilities before={1}",
                system.Name, colony.GetTotalFacilities(ProductionCategory.Industry));

            //Effect of sabotage // value needed for SitRep
            int removeIndustryFacilities = 0;

            //if ratio > 1 than remove one more  IndustryFacility
            if (ratio > 1 && colony.GetTotalFacilities(ProductionCategory.Industry) > 1)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 1;
                colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            //if ratio > 2 than remove one more  IndustryFacility
            if (ratio > 2 && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 2)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 2;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            // if ratio > 3 than remove one more  IndustryFacility
            if (ratio > 3 && system.Colony.GetTotalFacilities(ProductionCategory.Industry) > 3)// Industry: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeIndustryFacilities = 3;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Industry, 1);
            }

            defenseMeter.AdjustCurrent(defenseIntelligence / 3 * -1);
            defenseMeter.UpdateAndReset();
            attackMeter.AdjustCurrent(defenseIntelligence / 2); // devided by two, it's more than on defense side
            attackMeter.UpdateAndReset();

            GameLog.Core.Intel.DebugFormat("Sabotage Industry at {0}: TotalIndustryFacilities after={1}", system.Name, colony.GetTotalFacilities(ProductionCategory.Industry));
            attackedCivManager.SitRepEntries.Add(new NewSabotageSitRepEntry(attackedCiv, system.Colony, removeIndustryFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Industry)));

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
}
