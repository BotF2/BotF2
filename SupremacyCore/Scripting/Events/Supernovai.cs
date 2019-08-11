﻿// Copyright (c) 2009 Mike Strobel
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
    public class SupernovaiEvent : UnitScopedEvent<Colony>
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

                    if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                        return;

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;
                    var health = target.Health.CurrentValue;

                    if (game.Universe.FindOwned<Colony>(targetCiv).Count > 4) // only when many colonies are there
                        GameLog.Client.GameData.DebugFormat("SupernovaiEvents.cs: colony amount > 1 for: {0}", target.Name);

                    //game.CivilizationManagers[targetCiv].SitRepEntries.Add
                    //    (new ScriptedEventSitRepEntry(new ScriptedEventSitRepEntryData(
                    //    targetCiv,
                    //        "SUPERNOVA_I_HEADER_TEXT",
                    //        "SUPERNOVA_I_SUMMARY_TEXT",
                    //        "SUPERNOVA_I_DETAIL_TEXT",
                    //        "vfs:///Resources/Images/ScriptedEvents/Supernovai.png",
                    //        "vfs:///Resources/SoundFX/ScriptedEvents/Supernovai.wav",
                    //            () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));

                    GameLog.Client.GameData.DebugFormat("SupernovaiEvents.cs: HomeSystemName is: {0}", target.Name);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.AdjustCurrent( - population/6 * 3);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.UpdateAndReset();
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.AdjustCurrent(-health/ 5);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.UpdateAndReset();

                    GameContext.Current.Universe.UpdateSectors();

                    return;
                }
            }
        }
    }
}