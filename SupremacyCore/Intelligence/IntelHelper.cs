using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;


namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static Civilization _newTargetCiv;
        private static Civilization _newSpyCiv;
        private static UniverseObjectList<Colony> _newSpiedColonies;

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

        public static void SendXSpiedY(Civilization spyCiv, Civilization spiedCiv, UniverseObjectList<Colony> colonies)
        {      
            if (spyCiv == null)
                throw new ArgumentNullException("spyCiv");
            if (spiedCiv == null)  
                throw new ArgumentNullException("spiedCiv");

            _newSpyCiv = spyCiv;
            _newTargetCiv = spiedCiv;
            _newSpiedColonies = colonies;
        }
        #region Espionage Methods
        public static void SabotageEnergy(Colony colony, Civilization civ)
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
        #endregion
    }
}
