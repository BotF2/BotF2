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
    public class TerroristsCaptured : UnitScopedEvent<Colony>
    {
        private int _occurrenceChance = 100;

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

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations)
            {

                var affectedCivs = game.Civilizations
                    .Where(o =>
                        o.IsEmpire &&
                        o.IsHuman &&
                        RandomHelper.Chance(_occurrenceChance));

                var targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c))
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (var group in targetGroups)
                {

                    var productionCenters = group.ToList();

                    var target = productionCenters[RandomProvider.Next(productionCenters.Count)];

                    if (target.Owner.Name == "Borg") // Borg do not have terrorists
                        return;

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    OnUnitTargeted(target);

                    target.Morale.AdjustCurrent(+3);
                    target.Morale.UpdateAndReset();

                    var civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    if (civManager != null)
                    {
                        civManager.SitRepEntries.Add(new TerroristsCapturedSitRepEntry(civManager.Civilization, target.Name));
                    }
                }
            }
        }
    }
}
