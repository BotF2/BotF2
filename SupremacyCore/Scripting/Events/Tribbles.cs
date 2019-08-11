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
    public class TribblesEvent : UnitScopedEvent<Colony>
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

        private List<Building> _affectedBuildings;
        protected List<Building> AffectedBuildings
        {
            get
            {
                if (_affectedBuildings == null)
                    _affectedBuildings = new List<Building>();
                return _affectedBuildings;
            }
        }

        public TribblesEvent()
        {
            _affectedProjects = new List<BuildProject>();
            _affectedBuildings = new List<Building>();
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
            _shipProductionFinished = false; // turn off production for this turn
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
                    GameLog.Client.GameData.DebugFormat("Tribbles.cs: target.Name: {0}", target.Name);
                    GameLog.Client.GameData.DebugFormat("Tribbles.cs:ProductionOutput(ProductionCategory.Food): {0}", target.GetProductionOutput(ProductionCategory.Food));

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;

                    List<Building> tmpBuildings = new List<Building>(target.Buildings.Count);
                    tmpBuildings.AddRange(target.Buildings.ToList());
                    tmpBuildings.ForEach(o => target.DeactivateFacility(ProductionCategory.Food));

                    GameLog.Client.GameData.DebugFormat("Tribbles.cs: target.FoodReserves before : {0}", target.FoodReserves);

                    target.FoodReserves.AdjustCurrent(-1 * target.FoodReserves.CurrentValue);
                    target.FoodReserves.UpdateAndReset();
                    GameLog.Client.GameData.DebugFormat("Tribbles.cs: target.FoodReserves after : {0}", target.FoodReserves);
                    
                    target.DeactivateFacility(ProductionCategory.Food);

                    OnUnitTargeted(target);

                    //game.CivilizationManagers[targetCiv].SitRepEntries.Add(
                    //    new ScriptedEventSitRepEntry(
                    //        new ScriptedEventSitRepEntryData(
                    //            targetCiv,
                    //            "TRIBBLES_HEADER_TEXT",
                    //            "TRIBBLES_SUMMARY_TEXT",
                    //            "TRIBBLES_DETAIL_TEXT",
                    //            "vfs:///Resources/Images/ScriptedEvents/Tribbles.png",
                    //            "vfs:///Resources/SoundFX/ScriptedEvents/Tribbles.mp3",
                    //            () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));

                    GameContext.Current.Universe.UpdateSectors();

                    return;
                }

                if (phase == TurnPhase.Production)
                    _productionFinished = true; // turn production back on
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
}
