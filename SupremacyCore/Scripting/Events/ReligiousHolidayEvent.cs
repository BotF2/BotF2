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
    public class ReligiousHolidayEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 100;

        [NonSerialized]
        private List<BuildProject> _affectedProjects;

        protected List<BuildProject> AffectedProjects
        {
            get
            {
                if (_affectedProjects == null)
                    _affectedProjects = new List<BuildProject>();
                return _affectedProjects;
            }
        }

        public ReligiousHolidayEvent()
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
            _shipProductionFinished = false;
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

                    if (target.Owner.Name == "Borg") // Borg do not have religious holidays
                        return;

                   // if (target.Name == "Omarion")
                     //   return;

                    var affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project);

                    foreach (var affectedProject in affectedProjects)
                    {
                        affectedProject.IsPaused = true;
                        AffectedProjects.Add(affectedProject);
                    }

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;

                    OnUnitTargeted(target);

                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Morale.AdjustCurrent(+5);
                    GameContext.Current.Universe.Get<Colony>(targetColonyId).Morale.UpdateAndReset();
                    GameLog.Core.Diplomacy.DebugFormat("target colony ID ={0}", targetColonyId);
                    game.CivilizationManagers[targetCiv].SitRepEntries.Add(
                        new ScriptedEventSitRepEntry(                                                       
                                "\"RELIGIOUS_HOLIDAY_HEADER_TEXT\"",
                                "\"RELIGIOUS_HOLIDAY_SUMMARY_TEXT\"",
                                "\"RELIGIOUS_HOLIDAY_DETAIL_TEXT\"",
                                "vfs:///Resources/Images/ScriptedEvents/ReligiousHoliday.png",
                                "vfs:///Resources/SoundFX/ScriptedEvents/ReligiousHoliday.wma",
                                GameContext.Current.Universe.Get<Colony>(targetColonyId).Name));
                }

                return;
            }

            if (phase == TurnPhase.Production)
                _productionFinished = true;
            else if (phase == TurnPhase.ShipProduction)
                _shipProductionFinished = true;

            if (!_productionFinished || !_shipProductionFinished)
                return;

            foreach (var affectedProject in AffectedProjects)
                affectedProject.IsPaused = false;

            AffectedProjects.Clear();
        }
    }
}