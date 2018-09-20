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
        bool m_traceGammaRayBurst = false;
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
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (var group in targetGroups)
                {
                    var productionCenters = group.ToList();

                    var target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("GammaRayBurstEvents.cs: target.Name: {0}", target.Name);

                    if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion Nebula" || target.Name == "Romulus" || target.Name == "Borg Nebula")
                        return;

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;
                    var health = target.Health.CurrentValue;

                    if (m_traceGammaRayBurst)
                    {
                        GameLog.Print("Colony = {0}, population before = {1}, health before = {2}", targetColonyId, population, health);
                    }
                    if (game.Universe.FindOwned<Colony>(targetCiv).Count > 1)
                        GameLog.Client.GameData.DebugFormat("GammaRayBurstEvents.cs: colony amount > 1 for: {0}", target.Name);

                    game.CivilizationManagers[targetCiv].SitRepEntries.Add
                        (new ScriptedEventSitRepEntry(new ScriptedEventSitRepEntryData(
                        targetCiv,
                            "GAMMA_RAY_BURST_HEADER_TEXT",
                            "GAMMA_RAY_BURST_SUMMARY_TEXT",
                            "GAMMA_RAY_BURST_DETAIL_TEXT",
                            "vfs:///Resources/Images/ScriptedEvents/GammaRayBurst.png",
                            "vfs:///Resources/SoundFX/ScriptedEvents/GammaRayBurst.mp3",
                                () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));
               // see CivStringDatabase.xml for text

                    GameLog.Client.GameData.DebugFormat("GammaRayBurstEvents.cs: HomeSystemName is: {0}", target.Name);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.AdjustCurrent(- population/3 * 2);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.UpdateAndReset();
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.AdjustCurrent(-health/3 * 2);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.UpdateAndReset();

                    if (m_traceGammaRayBurst)
                    {
                        GameLog.Print("Colony = {0}, population after = {1}, health after = {2}", targetColonyId, GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.CurrentValue, GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.CurrentValue);
                    }

                    GameContext.Current.Universe.UpdateSectors();

                    return;
                }
            }
        }
    }
}