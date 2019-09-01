// Copyright(c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Buildings;
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
    public class MajorAsteroidImpact : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 100;

        [NonSerialized]
        private List<BuildProject> _affectedProjects;

        public MajorAsteroidImpact()
        {
            _affectedProjects = new List<BuildProject>();
        }

        public override bool CanExecute
        {
            get { return _occurrenceChance > 0 && base.CanExecute; }
        }

        protected override void InitializeOverride(IDictionary<string, object> options)
        {
            object value;

            if (options.TryGetValue("OccurrenceChance", out value))
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
            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber > 72)
            {
                var affectedCivs = game.Civilizations
                    .Where(c =>
                        c.IsEmpire &&
                        c.IsHuman &&
                        RandomHelper.Chance(_occurrenceChance));

                var targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (var group in targetGroups)
                {
                    var productionCenters = group.ToList();

                    var target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("target.Name: {0}", target.Name);

                    if (GameContext.Current.TurnNumber < 200)
                    {
                        if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                        {
                            return;
                        }
                    }

                    _affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project)
                        .ToList();

                    foreach (var affectedProject in _affectedProjects)
                    {
                        GameLog.Client.GameData.DebugFormat("affectedProject: {0}", affectedProject.Description);
                    }

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;
                    var health = target.Health.CurrentValue;

                    OnUnitTargeted(target);

                    // Population
                    target.Population.AdjustCurrent(-population / 5 * 3);
                    target.Population.UpdateAndReset();
                    target.Health.AdjustCurrent(-health / 5);
                    target.Health.UpdateAndReset();

                    // Facilities
                    int removeFood = target.GetTotalFacilities(ProductionCategory.Food) - 6; // Food: remaining everything up to 6
                    if (removeFood < 7)
                    {
                        removeFood = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Food, removeFood);

                    int removeIndustry = target.GetTotalFacilities(ProductionCategory.Industry) - 5; // Industry: remaining everything up to 5
                    if (removeIndustry < 6)
                    {
                        removeIndustry = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Industry, removeIndustry);

                    int removeEnergy = target.GetTotalFacilities(ProductionCategory.Energy) - 2;  // Energy: remaining everything up to 2
                    if (removeEnergy < 3)
                    {
                        removeEnergy = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Energy, removeEnergy);

                    int removeResearch = target.GetTotalFacilities(ProductionCategory.Research) - 3;  // Research: remaining everything up to 3
                    if (removeResearch < 4)
                    {
                        removeResearch = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Research, removeResearch);

                    int removeIntelligence = target.GetTotalFacilities(ProductionCategory.Intelligence) - 3;  // Research: remaining everything up to 3
                    if (removeIntelligence < 4)
                    {
                        removeIntelligence = 0;
                    }
                    target.RemoveFacilities(ProductionCategory.Intelligence, removeIntelligence); // Intelligence: remaining everything up to 0

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    if (civManager != null)
                    {
                        civManager.SitRepEntries.Add(new MajorAsteroidImpactSitRepEntry(civManager.Civilization, target.Name));
                    }

                    target.Population.UpdateAndReset();

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

            _affectedProjects.ForEach(p => p.IsPaused = false);
            _affectedProjects.Clear();
        }
    }
}