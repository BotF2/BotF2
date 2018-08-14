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
                    .Where(
                        o => o.IsEmpire &&
                             o.IsHuman &&
                             RandomHelper.Chance(_occurrenceChance))
                    .ToList();

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

                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Morale.AdjustCurrent(+5);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Morale.UpdateAndReset();

                    game.CivilizationManagers[targetCiv].SitRepEntries.Add(
                        new ScriptedEventSitRepEntry(
                            new ScriptedEventSitRepEntryData(
                                targetCiv,
                                "TERRORISTS_CAPTURED_HEADER_TEXT",
                                "TERRORISTS_CAPTURED_SUMMARY_TEXT",
                                "TERRORISTS_CAPTURED_DETAIL_TEXT",
                                "vfs:///Resources/Images/ScriptedEvents/TerroristsCaptured.png",
                                "vfs:///Resources/SoundFX/ScriptedEvents/EventGenerell.wma",
                                () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));
                }

                return;
            }
        }
    }
}
