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
    public class TerroristBombingOfShipProductionEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 200;

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

        public TerroristBombingOfShipProductionEvent()
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
                    GameLog.Client.GameData.DebugFormat("TerroristBombShipyards.cs: target.Name: {0}", target.Name);

                    if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion Nebula" || target.Name == "Romulus" || target.Name == "Borg Nebula")
                        return;

                    var affectedProjects = target.BuildSlots
                    .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                    .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                    .Select(o => o.Project);

                    foreach (var affectedProject in affectedProjects)
                    {
                        GameLog.Client.GameData.DebugFormat("TerroristBombShipyards.cs: affectedProject: {0}", affectedProject.Description);

                        AffectedProjects.Add(affectedProject);
                    }

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;

                    if (target.Shipyard != null)
                    {
                        GameLog.Client.GameData.DebugFormat("TerroristBombShipyards.cs: {0} Shipyard: {1}, affectedProject: {2}", target.Name, target.Shipyard.Name, target.Shipyard.BuildSlots.Count);
                        List<ShipyardBuildSlot> tmpShipyards = new List<ShipyardBuildSlot>(target.Shipyard.BuildSlots.Count);
                        tmpShipyards.AddRange(target.Shipyard.BuildSlots.ToList());
                        tmpShipyards.ForEach(o => target.DeactivateShipyardBuildSlot(o));
                        tmpShipyards.ForEach(o => GameLog.Client.GameData.DebugFormat("TerroristBombShipyards.cs: affectedProject: {0}", target.Shipyard.BuildSlots.Count));
                        tmpShipyards.ForEach(o => target.Shipyard.BuildQueue.Clear());
                        tmpShipyards.ForEach(o => o.Shipyard.ObjectID = GameObjectID.InvalidID);
                        game.CivilizationManagers[targetCiv].SitRepEntries.Add(
                                new ScriptedEventSitRepEntry(
                                 new ScriptedEventSitRepEntryData(
                                   targetCiv,
                                    "TERRORIST_BOMBING_OF_SHIP_PRODUCTION_HEADER_TEXT",
                                    "TERRORIST_BOMBING_OF_SHIP_PRODUCTION_SUMMARY_TEXT",
                                    "TERRORIST_BOMBING_OF_SHIP_PRODUCTION_DETAIL_TEXT",
                                    "vfs:///Resources/Images/ScriptedEvents/TerroristBombingOfShipProduction.png",
                                    "vfs:///Resources/SoundFX/ScriptedEvents/TerroristBombing.mp3",
                                    () => GameContext.Current.Universe.Get<Colony>(targetColonyId).Name)));
                    }

                    OnUnitTargeted(target);

                    GameContext.Current.Universe.UpdateSectors();

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
