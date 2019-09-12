﻿// Copyright (c) 2009 Mike Strobel
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
    public class AsteroidImpactEvent : UnitScopedEvent<Colony>
    {
        private bool _productionFinished;
        private bool _shipProductionFinished;
        private int _occurrenceChance = 200;

        [NonSerialized]
        private List<BuildProject> _affectedProjects;

        public AsteroidImpactEvent()
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
            _shipProductionFinished = false; // turn off production for this turn
        }

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber >=80)
            {
                var affectedCivs = game.Civilizations
                    .Where(
                        o => o.IsEmpire &&
                             o.IsHuman &&
                             RandomHelper.Chance(_occurrenceChance));
                    //.ToList();

                var targetGroups = affectedCivs
                    .Where(CanTargetCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (var group in targetGroups) 
                {
                    var productionCenters = group.ToList();

                    var target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("target.Name: {0}", target.Name);

                    var _affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project)
                        .ToList();
                        //;

                    foreach (var affectedProject in _affectedProjects)
                    {
                        GameLog.Client.GameData.DebugFormat("affectedProject: {0}", affectedProject.Description);
                    }

                    var targetCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    var population = target.Population.CurrentValue;
                    var health = target.Health.CurrentValue;

                    List<Building> tmpBuildings = new List<Building>(target.Buildings.Count);
                    tmpBuildings.AddRange(target.Buildings.ToList());
                    tmpBuildings.ForEach(o => target.RemoveBuilding(o));
                    tmpBuildings.ForEach(o => o.ObjectID = -1);

                    OnUnitTargeted(target);

                    target.Population.AdjustCurrent(-population / 5);
                    target.Population.UpdateAndReset();
                    target.Health.AdjustCurrent(-(health / 5));
                    //GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.UpdateAndReset();

                    int removeFood = 2; // If you have food 4 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Food) < 4)
                        removeFood = 0;
                    target.RemoveFacilities(ProductionCategory.Food, removeFood);

                    int removeIndustry = 4;  // If you have industry 8 or more then take out 4
                    if (target.GetTotalFacilities(ProductionCategory.Industry) < 8)
                        removeIndustry = 0;
                    target.RemoveFacilities(ProductionCategory.Industry, removeIndustry);

                    int removeEnergy = 2; ;  // If you have energy 6 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Energy) < 6)
                        removeEnergy = 0;
                    target.RemoveFacilities(ProductionCategory.Energy, removeEnergy);

                    int removeResearch = 2;   // If you have research 4 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Research) < 4)
                        removeResearch = 0;
                    target.RemoveFacilities(ProductionCategory.Research, removeResearch);

                    int removeIntelligence = 3;   // If you have intel 4 or more than take out 3
                    if (target.GetTotalFacilities(ProductionCategory.Intelligence) < 4)
                        removeIntelligence = 0;
                    target.RemoveFacilities(ProductionCategory.Intelligence, removeIntelligence);

                    int removeOrbitalBatteries = 10;  // if you have 11 or more orbital batteries take out 10
                    if (target.OrbitalBatteries.Count <= 11)
                        removeOrbitalBatteries = 0;
                    target.RemoveOrbitalBatteries(removeOrbitalBatteries);

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetCiv.CivID];
                    if (civManager != null)
                        civManager.SitRepEntries.Add(new AsteroidImpactSitRepEntry(civManager.Civilization, target));

                    GameContext.Current.Universe.UpdateSectors();
                    return;
                }

                foreach (var affectedProject in _affectedProjects)
                    affectedProject.IsPaused = false;

                _affectedProjects.Clear();
            }
        }
    }
}

