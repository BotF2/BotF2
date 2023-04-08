// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Scripting.Events
{
    [Serializable]
    public class EarthquakeEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 100000;

        [NonSerialized]
        private List<BuildProject> _affectedProjects;

        public EarthquakeEvent()
        {
            _affectedProjects = new List<BuildProject>();
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
                    .GroupBy(o => o.OwnerID);

                foreach (IGrouping<int, Colony> group in targetGroups)
                {
                    List<Colony> productionCenters = group.ToList();

                    Colony target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("target.Name: {0}", target.Name);

                    _affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project)
                        .ToList();

                    foreach (BuildProject affectedProject in _affectedProjects)
                    {
                        GameLog.Client.GameData.DebugFormat("affectedProject: {0}", affectedProject.Description);
                    }

                    Entities.Civilization targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    int population = target.Population.CurrentValue;
                    int health = target.Health.CurrentValue;

                    OnUnitTargeted(target);

                    _ = target.Morale.AdjustCurrent(-5);

                    // Population
                    //Don't reduce the population if it is already low
                    if (population >= 65)
                    {
                        _ = target.Population.AdjustCurrent(-5);
                    }
                    target.Population.UpdateAndReset();
                    _ = target.Health.AdjustCurrent(-(health / 6));
                    target.Health.UpdateAndReset();

                    // Facilities
                    int removeFood = 1; // If you have food 4 or more then take out 1
                    if (target.GetTotalFacilities(ProductionCategory.Food) < 4)
                    {
                        removeFood = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Food, removeFood);

                    int removeIndustry = 2;  // If you have industry 8 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Industry) < 8)
                    {
                        removeIndustry = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Industry, removeIndustry);

                    int removeEnergy = 1; ;  // If you have energy 6 or more then take out 1
                    if (target.GetTotalFacilities(ProductionCategory.Energy) < 6)
                    {
                        removeEnergy = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Energy, removeEnergy);

                    int removeResearch = 1;   // If you have research 4 or more then take out 1
                    if (target.GetTotalFacilities(ProductionCategory.Research) < 4)
                    {
                        removeResearch = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Research, removeResearch);

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    civManager?.SitRepEntries.Add(new EarthquakeSitRepEntry(civManager.Civilization, target));

                    GameContext.Current.Universe.UpdateSectors();
                }
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

            if (_affectedProjects != null)
            {
                _affectedProjects.ForEach(p => p.IsPaused = false);
                _affectedProjects.Clear();
            }
        }
    }
}
