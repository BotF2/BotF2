// TerroristCaptured.cs 
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

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
    public class TerroristsCaptured : UnitScopedEvent<Colony>
    {
        private int _occurrenceChance = 100;

        [NonSerialized]
        private string _text;


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

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations)
            {

                IEnumerable<Entities.Civilization> affectedCivs = game.Civilizations
                    .Where(o =>
                        o.IsEmpire &&
                        o.IsHuman &&
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

                    if (target.Owner.Name == "Borg") // Borg do not have terrorists
                    {
                        return;
                    }

                    Entities.Civilization targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    OnUnitTargeted(target);

                    _ = target.Morale.AdjustCurrent(+3);
                    target.Morale.UpdateAndReset();

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    _text = target.Location + " " + target.Name + " > ";
                    civManager?.SitRepEntries.Add(new ReportEntry_ShowColony(civManager.Civilization, target
                        , _text + ResourceManager.GetString("TERRORISTS_CAPTURED_HEADER_TEXT")
                        , _text + ResourceManager.GetString("TERRORISTS_CAPTURED_DETAIL_TEXT")
                        , "ScriptedEvents/TerroristsCaptured.png", SitRepPriority.RedYellow));

                    //civManager?.SitRepEntries.Add(new TerroristsCapturedSitRepEntry(civManager.Civilization, target));
                }
            }
        }
    }
}
