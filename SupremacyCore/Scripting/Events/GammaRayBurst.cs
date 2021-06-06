// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Scripting.Events
{
    [Serializable]
    public class GammaRayBurstEvent : UnitScopedEvent<Colony>
    {
        private int _occurrenceChance = 200;
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

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber > 65)
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
                    GameLog.Core.Events.DebugFormat("target.Name: {0}", target.Name);
                    if (GameContext.Current.TurnNumber < 290)
                    {
                        if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                        {
                            return;
                        }
                    }

                    Entities.Civilization targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    int population = target.Population.CurrentValue;
                    int health = target.Health.CurrentValue;

                    GameLog.Core.Events.DebugFormat("Colony = {0}, population before = {1}, health before = {2}", targetColonyId, population, health);

                    if (game.Universe.FindOwned<Colony>(targetCiv).Count > 2)
                    {
                        GameLog.Core.Events.DebugFormat("colony amount > 1 for: {0}", target.Name);
                    }

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    if (civManager != null)
                    {
                        civManager.SitRepEntries.Add(new GammaRayBurstSitRepEntry(civManager.Civilization, target));
                    }

                    GameLog.Core.Events.DebugFormat("HomeSystemName is: {0}", target.Name);
                    target.Population.AdjustCurrent(-population / 3 * 2);
                    target.Population.UpdateAndReset();
                    target.Health.AdjustCurrent(-health / 3 * 2);
                    target.Health.UpdateAndReset();

                    GameLog.Core.Events.DebugFormat("Colony = {0}, population after = {1}, health after = {2}", targetColonyId, target.Population.CurrentValue, target.Health.CurrentValue);

                    GameContext.Current.Universe.UpdateSectors();
                }
            }
        }
    }
}