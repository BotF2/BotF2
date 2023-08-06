// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Resources;
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
        private string _text;

        public ReligiousHolidayEvent()
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
            _shipProductionFinished = false;
        }

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber >= 30)
            {
                IEnumerable<Entities.Civilization> affectedCivs = game.Civilizations
                    .Where(c => c.IsEmpire &&
                        c.IsHuman &&
                        RandomHelper.Chance(_occurrenceChance));

                IEnumerable<IGrouping<int, Colony>> targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c))
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (IGrouping<int, Colony> group in targetGroups)
                {
                    List<Colony> productionCenters = group.ToList();

                    Colony target = productionCenters[RandomProvider.Next(productionCenters.Count)];

                    // Borg do not have religious holidays
                    if (target.Owner.Name == "Borg")
                    {
                        return;
                    }

                    _affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project)
                        .ToList();


                    _affectedProjects.ForEach(p => p.IsPaused = true);

                    Entities.Civilization targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;

                    OnUnitTargeted(target);

                    _ = target.Morale.AdjustCurrent(+5);
                    target.Morale.UpdateAndReset();

                    //CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];

                    //civManager?.SitRepEntries.Add(new ReligiousHolidaySitRepEntry(civManager.Civilization, target));
                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];

                    _text = target.Location + " " + target.Name + " > ";
                    civManager?.SitRepEntries.Add(new ReportEntry_ShowColony(civManager.Civilization, target
                        , _text + ResourceManager.GetString("RELIGIOUS_HOLIDAY_HEADER_TEXT")
                        , _text + ResourceManager.GetString("RELIGIOUS_HOLIDAY_DETAIL_TEXT")
                        , "ScriptedEvents/ReligiousHoliday.png", SitRepPriority.RedYellow));

                    //                    public override string DetailText => string.Format(ResourceManager.GetString("RELIGIOUS_HOLIDAY_DETAIL_TEXT"), Colony.Name, Colony.Location);
                    //public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/ReligiousHoliday.png";
                }

                return;
            }

            else if (phase == TurnPhase.Production)
            {
                _productionFinished = true;
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