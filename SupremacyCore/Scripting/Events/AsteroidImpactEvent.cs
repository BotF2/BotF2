// File:AsteroidImpactEvent.cs
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Buildings;
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
    public class AsteroidImpactEvent : UnitScopedEvent<Colony>
    {
        //private bool _productionFinished;         // this is necassary !!!
        //private bool _shipProductionFinished;     // this is necassary !!!

        private int _occurrenceChance = 200;

        //[NonSerialized]
        //private string _text;

        //#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
        //private List<BuildProject> _affectedProjects;
        //#pragma warning restore IDE0044 // Modifizierer "readonly" hinzufügen


        public AsteroidImpactEvent()
        {
            List<BuildProject> _affectedProjects = new List<BuildProject>();

            //keep the following to avoid error messages while "Build"
            //_text += _productionFinished.ToString() + _text;
            //_text += _shipProductionFinished.ToString();
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
            //_productionFinished = false;
            //_shipProductionFinished = false; // turn off production for this turn

            //if (_shipProductionFinished == true && _productionFinished == true) { }; // dummy
        }

        protected override void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase)
        {
            string _text = "";
            List<BuildProject> _affectedProjects = new List<BuildProject>();

            if (phase == TurnPhase.PreTurnOperations && GameContext.Current.TurnNumber > 20)  // before 80
            {
                IEnumerable<Entities.Civilization> affectedCivs = game.Civilizations
                    .Where(
                        o => o.IsEmpire &&
                             o.IsHuman &&
                             RandomHelper.Chance(_occurrenceChance));
                //.ToList();

                IEnumerable<IGrouping<int, Colony>> targetGroups = affectedCivs
                    .Where(CanTargetEventCivilization)
                    .SelectMany(c => game.Universe.FindOwned<Colony>(c)) // finds colony to affect in the civiliation's empire
                    .Where(CanTargetUnit)
                    .GroupBy(o => o.OwnerID);

                foreach (IGrouping<int, Colony> group in targetGroups)
                {
                    List<Colony> productionCenters = group.ToList();
                    _affectedProjects = new List<BuildProject>();

                    Colony target = productionCenters[RandomProvider.Next(productionCenters.Count)];
                    GameLog.Client.GameData.DebugFormat("target.Name: {0}", target.Name);

                    if (GameContext.Current.TurnNumber < 150)  // impacts on HomeWorlds are hard !!!!
                    {
                        if (target.Name == "Sol" || target.Name == "Terra" || target.Name == "Cardassia" || target.Name == "Qo'nos" || target.Name == "Omarion" || target.Name == "Romulus" || target.Name == "Borg")
                        {
                            return;
                        }
                    }

                    _affectedProjects = target.BuildSlots
                        .Concat((target.Shipyard != null) ? target.Shipyard.BuildSlots : Enumerable.Empty<BuildSlot>())
                        .Where(o => o.HasProject && !o.Project.IsPaused && !o.Project.IsCancelled)
                        .Select(o => o.Project)
                        .ToList();
                    //;

                    foreach (BuildProject affectedProject in _affectedProjects)
                    {
                        _text = "Step_7222:; " + GameEngine.LocationString(target.Location.ToString())
                            + " > Turn " + GameContext.Current.TurnNumber
                            + " > Earthquake - affectedProject= " + affectedProject.Description;
                        Console.WriteLine(_text);
                        GameLog.Client.GameData.DebugFormat(_text);
                    }

                    Entities.Civilization targetEventCiv = target.Owner;
                    int targetColonyId = target.ObjectID;
                    int population = target.Population.CurrentValue;
                    int health = target.Health.CurrentValue;

                    List<Building> tmpBuildings = new List<Building>(target.Buildings.Count);
                    tmpBuildings.AddRange(target.Buildings.ToList());
                    tmpBuildings.ForEach(o => target.RemoveBuilding(o));
                    tmpBuildings.ForEach(o => o.ObjectID = -1);

                    OnUnitTargeted(target);

                    _ = target.Population.AdjustCurrent(-population / 5);
                    target.Population.UpdateAndReset();
                    _ = target.Health.AdjustCurrent(-(health / 5));
                    target.Health.UpdateAndReset();
                    //GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.UpdateAndReset();

                    int removeFood = 2; // If you have food 4 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Food) < 4)
                    {
                        removeFood = 0;
                    }

                    target.RemoveFacilities(ProductionCategory.Food, removeFood);
                    for (int i = 0; i < target.GetActiveFacilities(ProductionCategory.Food); i++)
                    {
                        target.Facility_Deactivate(ProductionCategory.Food);
                        target.Facility_Activate(ProductionCategory.Food);
                    }

                    int removeIndustry = 4;  // If you have industry 8 or more then take out 4
                    if (target.GetTotalFacilities(ProductionCategory.Industry) < 8)
                    {
                        removeIndustry = 0;
                    }

                    target.RemoveFacilities(ProductionCategory.Industry, removeIndustry);
                    for (int i = 0; i < target.GetActiveFacilities(ProductionCategory.Industry); i++)
                    {
                        target.Facility_Deactivate(ProductionCategory.Industry);
                        target.Facility_Activate(ProductionCategory.Industry);
                    }

                    int removeEnergy = 2; ;  // If you have energy 6 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Energy) < 6)
                    {
                        removeEnergy = 0;
                    }

                    target.RemoveFacilities(ProductionCategory.Energy, removeEnergy);
                    for (int i = 0; i < target.GetActiveFacilities(ProductionCategory.Energy); i++)
                    {
                        target.Facility_Deactivate(ProductionCategory.Energy);
                        target.Facility_Activate(ProductionCategory.Energy);
                    }

                    int removeResearch = 2;   // If you have research 4 or more then take out 2
                    if (target.GetTotalFacilities(ProductionCategory.Research) < 4)
                    {
                        removeResearch = 0;
                    }

                    target.RemoveFacilities(ProductionCategory.Research, removeResearch);
                    for (int i = 0; i < target.GetActiveFacilities(ProductionCategory.Research); i++)
                    {
                        target.Facility_Deactivate(ProductionCategory.Research);
                        target.Facility_Activate(ProductionCategory.Research);
                    }

                    int removeIntelligence = 3;   // If you have intel 4 or more than take out 3
                    if (target.GetTotalFacilities(ProductionCategory.Intelligence) < 4)
                    {
                        removeIntelligence = 0;
                    }

                    target.RemoveFacilities(ProductionCategory.Intelligence, removeIntelligence);
                    for (int i = 0; i < target.GetActiveFacilities(ProductionCategory.Intelligence); i++)
                    {
                        target.Facility_Deactivate(ProductionCategory.Intelligence);
                        target.Facility_Activate(ProductionCategory.Intelligence);
                    }


                    int removeOrbitalBatteries = 10;  // if you have 11 or more orbital batteries take out 10
                    if (target.OrbitalBatteries.Count <= 11)
                    {
                        removeOrbitalBatteries = 0;
                    }

                    target.RemoveOrbitalBatteries(removeOrbitalBatteries);

                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[targetEventCiv.CivID];
                    //civManager?.SitRepEntries.Add(new AsteroidImpactSitRepEntry(civManager.Civilization, target));
                    _text = target.Location + " " + target.Name + " > ";
                    civManager?.SitRepEntries.Add(new ReportEntry_ShowColony(civManager.Civilization, target
                        , _text + ResourceManager.GetString("ASTEROID_IMPACT_HEADER_TEXT")
                        , _text + ResourceManager.GetString("ASTEROID_IMPACT_DETAIL_TEXT")
                        , "ScriptedEvents/AsteroidImpact.png", SitRepPriority.RedYellow));
                    //    public override string SummaryText => string.Format(ResourceManager.GetString("ASTEROID_IMPACT_SUMMARY_TEXT"), Colony.Name, Colony.Location);
                    //    public override string SitRepComment { get; set; }
                    //    public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/AsteroidImpact.png";

                    _text = "Step_5492:; Turn " + GameContext.Current.TurnNumber + ": " + target.Location + " " + target.Name
                        + " > AsteroidImpact (Event). Down: Population " + -population / 3 * 2 + ", Health " + -health / 3 * 2;
                    Console.WriteLine(_text);
                    //GameLog.Core.Events.DebugFormat(_text);

                    GameContext.Current.Universe.UpdateSectors();
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
}

