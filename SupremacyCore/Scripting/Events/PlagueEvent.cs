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
    public class PlagueEvent : UnitScopedEvent<Colony>
    {

        private int _occurrenceChance = 200;
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
            if (phase == TurnPhase.PreTurnOperations  && GameContext.Current.TurnNumber >30)
            {
                var affectedCivs = game.Civilizations
                    .Where(
                        o => o.IsEmpire &&
                             o.IsHuman &&
                             RandomHelper.Chance(_occurrenceChance))
                    .ToList();

                var targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (var group in targetGroups)
                {
                    var productionCenters = group.ToList();

                    var target = productionCenters[RandomProvider.Next(productionCenters.Count)];

                    if (GameContext.Current.TurnNumber < 400)
                    {
                        if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                            return;
                    }                    
                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;
                    var health = target.Health.CurrentValue;

                    GameLog.Core.Events.DebugFormat("Colony = {0}, population before = {1}, health before = {2}", targetColonyId, population, health);

                    if (game.Universe.FindOwned<Colony>(targetCiv).Count > 1)
                        GameLog.Client.GameData.DebugFormat("colony amount > 1 for: {0}", target.Name);

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    if (civManager != null)
                        civManager.SitRepEntries.Add(new PlaqueSitRepEntry(civManager.Civilization, target.Name));

                    // OLD

                    //game.CivilizationManagers[targetCiv].SitRepEntries.Add
                    //    (new ScriptedEventSitRepEntry(new ScriptedEventSitRepEntryData(
                    //    targetCiv,
                    //        "PLAGUE_HEADER_TEXT",
                    //        "PLAGUE_SUMMARY_TEXT",
                    //        "PLAGUE_DETAIL_TEXT",
                    //        "vfs:///Resources/Images/ScriptedEvents/Plague.png",
                    //        "vfs:///Resources/SoundFX/ScriptedEvents/Plague.mp3",
                    //            () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));

                    GameLog.Client.GameData.DebugFormat("HomeSystemName is: {0}", target.Name);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.AdjustCurrent(- (population/3));
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.UpdateAndReset();
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.AdjustCurrent(- (health/2));
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.UpdateAndReset();

                    GameLog.Core.Events.DebugFormat("Colony = {0}, population after = {1}, health after = {2}", targetColonyId, GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.CurrentValue, GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.CurrentValue);

                    GameContext.Current.Universe.UpdateSectors();

                    return;
                }
            }
        }
    }
}
