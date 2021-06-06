// Copyright (c) 2009 Mike Strobel
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
    public class TerroristBombingOfShipProductionEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 200;

        [NonSerialized]
        private List<BuildProject> _affectedProjects;

        public TerroristBombingOfShipProductionEvent()
        {
            _affectedProjects = new List<BuildProject>();
        }

        public override bool CanExecute => _occurrenceChance > 0 && base.CanExecute;

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
            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber > 55)
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

                    //Don't target home systems
                    if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                    {
                        return;
                    }

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

                    if (target.Shipyard != null)
                    {
                        GameLog.Client.GameData.DebugFormat("{0} Shipyard: {1}, affectedProject: {2}", target.Name, target.Shipyard.Name, target.Shipyard.BuildSlots.Count);
                        List<ShipyardBuildSlot> tmpShipyards = new List<ShipyardBuildSlot>(target.Shipyard.BuildSlots.Count);
                        tmpShipyards.AddRange(target.Shipyard.BuildSlots.ToList());
                        tmpShipyards.ForEach(o => target.DeactivateShipyardBuildSlot(o));
                        tmpShipyards.ForEach(o => GameLog.Client.GameData.DebugFormat("affectedProject: {0}", target.Shipyard.BuildSlots.Count));
                        tmpShipyards.ForEach(o => target.Shipyard.BuildQueue.Clear());
                        tmpShipyards.ForEach(o => o.Shipyard.ObjectID = -1);

                        CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                        if (civManager != null)
                        {
                            civManager.SitRepEntries.Add(new TerroristBombingOfShipProductionSitRepEntry(civManager.Civilization, target));
                        }

                    }

                    OnUnitTargeted(target);

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

            if (_affectedProjects != null)
            {
                _affectedProjects.ForEach(p => p.IsPaused = false);
                _affectedProjects.Clear();
            }
        }
    }
}
