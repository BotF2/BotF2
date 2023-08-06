// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Buildings;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Scripting.Events
{
    [Serializable]
    public class TribblesEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 100;

        [NonSerialized]
        private string _text;

        public TribblesEvent()
        {
        }

        public override bool CanExecute => _occurrenceChance > 0 && base.CanExecute;

        protected override void InitializeOverride(IDictionary<string, object> options)
        {

            if (options.TryGetValue("OccurrenceChance", out object value))
            {
                try
                {
                    _occurrenceChance = Convert.ToInt32(value);
                }
                catch
                {
                    GameLog.Client.GameData.ErrorFormat(
                        "Invalid OccurrenceChance value for event '{0}': {1}",
                        EventID,
                        value);
                }
            }
        }

        protected override void OnTurnStartedOverride(GameContext game)
        {
            _productionFinished = false;
            _shipProductionFinished = false; // turn off production for this turn
        }

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations)
            {
                IEnumerable<Entities.Civilization> affectedCivs = game.Civilizations
                    .Where(c =>
                        c.IsEmpire &&
                        c.IsHuman &&
                        RandomHelper.Chance(_occurrenceChance));

                IEnumerable<IGrouping<int, Colony>> targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(c => c.OwnerID);

                foreach (IGrouping<int, Colony> group in targetGroups)
                {
                    List<Colony> productionCenters = group.ToList();

                    Colony target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("target.Name: {0}", target.Name);
                    GameLog.Client.GameData.DebugFormat("ProductionOutput(ProductionCategory.Food): {0}", target.GetProductionOutput(ProductionCategory.Food));

                    Entities.Civilization targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    int population = target.Population.CurrentValue;

                    List<Building> tmpBuildings = new List<Building>(target.Buildings.Count);
                    tmpBuildings.AddRange(target.Buildings);
                    tmpBuildings.ForEach(o => target.DeactivateFacility(ProductionCategory.Food));

                    GameLog.Client.GameData.DebugFormat("target.FoodReserves before : {0}", target.FoodReserves);

                    _ = target.FoodReserves.AdjustCurrent(-1 * target.FoodReserves.CurrentValue);
                    target.FoodReserves.UpdateAndReset();
                    GameLog.Client.GameData.DebugFormat("target.FoodReserves after : {0}", target.FoodReserves);

                    _ = target.DeactivateFacility(ProductionCategory.Food);

                    OnUnitTargeted(target);

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    //civManager?.SitRepEntries.Add(new TribblesSitRepEntry(civManager.Civilization, target));


                    _text = target.Location + " " + target.Name + " > ";
                    civManager?.SitRepEntries.Add(new ReportEntry_ShowColony(civManager.Civilization, target
                        , _text + ResourceManager.GetString("TRIBBLES_HEADER_TEXT")
                        , _text + ResourceManager.GetString("TRIBBLES_DETAIL_TEXT")
                        , "ScriptedEvents/Tribbles.png", SitRepPriority.RedYellow));
                    //civManager?.SitRepEntries.Add(new ReportEntry_ShowColony(civManager.Civilization, target));
        //                    public override string DetailText => string.Format(ResourceManager.GetString("TRIBBLES_DETAIL_TEXT"), Colony.Name, Colony.Location);
        //public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Tribbles.png";

        GameContext.Current.Universe.UpdateSectors();
                }

                return;
            }

            else if (phase == TurnPhase.Production)
            {
                _productionFinished = true; // turn production back on
            }
            else if (phase == TurnPhase.ShipProduction)
            {
                _shipProductionFinished = true;
            }

            if (!_productionFinished || !_shipProductionFinished)
            {
                return;
            }
        }
    }
}
