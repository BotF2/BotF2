// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Data;
using Supremacy.Diplomacy;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.SpyOperations;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Supremacy.Game
{
    /// <summary>
    /// Defines the turn processing phases used by the game engine.
    /// </summary>
    public enum TurnPhase : byte
    {
        WaitOnPlayers = 0,
        PreTurnOperations,
        SpyOperations,
        ResetObjects,
        FleetMovement,
        Combat,
        PopulationGrowth,
        Research,
        Scrapping,
        Maintenance,
        ShipProduction,
        Production,
        Trade,
        Intelligence,
        Morale,
        MapUpdates,
        PostTurnOperations,
        SendUpdates,
        Diplomacy,
        WaitOnAIPlayers
    }

    /// <summary>
    /// Delegate used for event handlers related to changes in the current turn phase.
    /// </summary>
    public delegate void TurnPhaseEventHandler(TurnPhase phase);

    /// <summary>
    /// Delegate used for event handlers related to the initiation of combat.
    /// </summary>
    public delegate void CombatEventHandler(List<CombatAssets> assets);

    /// <summary>
    /// Delegate used for event handlers related to the initiation of system invasions.
    /// </summary>
    public delegate void InvasionEventHandler(InvasionArena invasionArena);

    /// <summary>
    /// The turn processing engine used in the game.
    /// </summary>
    public class GameEngine
    {
        private Civilization _spyAttacking;

        private Civilization _spyAttacked;

        private int _spyCredits;


        #region Public Members

        /// <summary>
        /// Occurs when the current turn phase has changed.
        /// </summary>
        public event TurnPhaseEventHandler TurnPhaseChanged;

        /// <summary>
        /// Occurs when the current turn phase has finished.
        /// </summary>
        public event TurnPhaseEventHandler TurnPhaseFinished;

        /// <summary>
        /// Occurs when combat is starting.
        /// </summary>
        public event CombatEventHandler CombatOccurring;

        /// <summary>
        /// Occurs when an invasion is starting.
        /// </summary>
        public event InvasionEventHandler InvasionOccurring;

        /// <summary>
        /// Occurs when a Fleet moves to a new location.
        /// </summary>
        public event EventHandler<ParameterEventArgs<Fleet>> FleetLocationChanged;

        public object GameContent { get; private set; }
        public object AppContext { get; private set; }
        //public Civilization SpyAttacking { get => _spyAttacking; set => _spyAttacking = value; }
        //public Civilization SpyAttacked { get => _spyAttacked; set => _spyAttacked = value; }
        //public int SpyCredits { get => spyCredits; set => spyCredits = value; }
        #endregion
        public void SendStealCreditsData(Civilization attacking, Civilization attacked, int credits)
        {
            _spyAttacking = attacking;
            _spyAttacked = attacked;
            _spyCredits = credits;
        }
        #region Private Members
        /// <summary>
        /// Blocks the execution of the turn processing engine while waiting on players
        /// to submit combat orders.
        /// </summary>
        private readonly ManualResetEvent CombatReset = new ManualResetEvent(false);
        #endregion

        #region OnTurnPhaseChanged() Method
        /// <summary>
        /// Raises the <see cref="TurnPhaseChanged"/> event.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="phase">The current turn phase.</param>
        private void OnTurnPhaseChanged(GameContext game, TurnPhase phase)
        {
            if (phase != TurnPhase.SendUpdates)
            {
                foreach (var scriptedEvent in game.ScriptedEvents)
                {
                    //if (GameContext.Current.TurnNumber >= 1)
                        scriptedEvent.OnTurnPhaseStarted(game, phase);
                }
            }

            var handler = TurnPhaseChanged;
            if (handler != null)
                handler(phase);
        }
        #endregion

        #region OnTurnPhaseFinished() Method
        /// <summary>
        /// Raises the <see cref="TurnPhaseChanged"/> event.
        /// </summary>
        /// /// <param name="game">The current game.</param>
        /// <param name="phase">The turn phase that just finished.</param>
        private void OnTurnPhaseFinished(GameContext game, TurnPhase phase)
        {
            if (phase != TurnPhase.SendUpdates)
            {
                foreach (var scriptedEvent in game.ScriptedEvents)
                {
                    //if (GameContext.Current.TurnNumber >= 50)
                        scriptedEvent.OnTurnPhaseFinished(game, phase);
                }
            }

            var handler = TurnPhaseFinished;
            if (handler != null)
                handler(phase);
        }
        #endregion

        #region OnFleetLocationChanged() Method
        /// <summary>
        /// Raises the <see cref="FleetLocationChanged"/> event.
        /// </summary>
        /// <param name="fleet">A Fleet whose Location just changed.</param>
        private void OnFleetLocationChanged(Fleet fleet)
        {
            var handler = FleetLocationChanged;
            if (handler != null)
                handler(this, new ParameterEventArgs<Fleet>(fleet));
        }
        #endregion

        #region DoTurn() Method
        /// <summary>
        /// Perform turn processing for the specified game context.
        /// </summary>
        /// <param name="game">The game context.</param>
        public void DoTurn([NotNull] GameContext game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            GameLog.Core.Events.DebugFormat("...beginning DoTurn...");

            HashSet<Fleet> fleets;

            GameContext.PushThreadContext(game);
            try
            {
                var eventsToRemove = game.ScriptedEvents.Where(o => !o.CanExecute).ToList();
                foreach (var eventToRemove in eventsToRemove)
                    game.ScriptedEvents.Remove(eventToRemove);

                //Update If we've reached turn x, start running scripted events
                if (GameContext.Current.TurnNumber >= 1)
                {
                    foreach (var scriptedEvent in game.ScriptedEvents)
                    {
                        scriptedEvent.OnTurnStarted(game);
                    }
                }

                fleets = game.Universe.Find<Fleet>();

                foreach (var fleet in fleets)
                    fleet.LocationChanged += HandleFleetLocationChanged;
            }
            finally { GameContext.PopThreadContext(); }

            GameLog.Core.Events.DebugFormat("...beginning PreTurnOperations...");

            OnTurnPhaseChanged(game, TurnPhase.PreTurnOperations);
            GameContext.PushThreadContext(game);
            try { DoPreTurnOperations(game); }       
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PreTurnOperations);

            GameLog.Core.Events.DebugFormat("...beginning SpyOperations...");

            //OnTurnPhaseChanged(game, TurnPhase.SpyOperations);
            //GameContext.PushThreadContext(game);
            //try { DoSpyOperations(); } //?? do we need game in the constructor ??
            //finally { GameContext.PopThreadContext(); }
            //OnTurnPhaseFinished(game, TurnPhase.SpyOperations);

            GameLog.Core.Events.DebugFormat("...beginning FleetMovement...");

            OnTurnPhaseChanged(game, TurnPhase.FleetMovement);
            GameContext.PushThreadContext(game);
            try { DoFleetMovement(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.FleetMovement);

            GameLog.Core.Events.DebugFormat("...beginning Diplomacy...");

            OnTurnPhaseChanged(game, TurnPhase.Diplomacy);
            GameContext.PushThreadContext(game);
            try { DoDiplomacy(); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Diplomacy);

            GameLog.Core.Events.DebugFormat("...beginning Combat...");

            OnTurnPhaseChanged(game, TurnPhase.Combat);
            GameContext.PushThreadContext(game);
            try { DoCombat(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Combat);

            GameLog.Core.Events.DebugFormat("...beginning PopulationGrowth...");

            OnTurnPhaseChanged(game, TurnPhase.PopulationGrowth);
            GameContext.PushThreadContext(game);
            try { DoPopulation(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PopulationGrowth);

            GameLog.Core.Events.DebugFormat("...beginning Research...");

            OnTurnPhaseChanged(game, TurnPhase.Research);
            GameContext.PushThreadContext(game);
            try { DoResearch(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Research);

            GameLog.Core.Events.DebugFormat("...beginning Scrapping...");

            OnTurnPhaseChanged(game, TurnPhase.Scrapping);
            GameContext.PushThreadContext(game);
            try { DoScrapping(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Scrapping);

            GameLog.Core.Events.DebugFormat("...beginning Maintenance...");

            OnTurnPhaseChanged(game, TurnPhase.Maintenance);
            GameContext.PushThreadContext(game);
            try { DoMaintenance(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Maintenance);

            GameLog.Core.Events.DebugFormat("...beginning Production...");

            OnTurnPhaseChanged(game, TurnPhase.Production);
            GameContext.PushThreadContext(game);
            try { DoProduction(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Production);

            GameLog.Core.Events.DebugFormat("...beginning ShipProduction...");

            OnTurnPhaseChanged(game, TurnPhase.ShipProduction);
            GameContext.PushThreadContext(game);
            try { DoShipProduction(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.ShipProduction);

            GameLog.Core.Events.DebugFormat("...beginning Trade...");

            OnTurnPhaseChanged(game, TurnPhase.Trade);
            GameContext.PushThreadContext(game);
            try { DoTrade(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Trade);

            //GameLog.Core.Events.DebugFormat("...beginning Intelligence...");

            //OnTurnPhaseChanged(game, TurnPhase.Intelligence);
            //GameContext.PushThreadContext(game);
            //try { DoIntelligence(game); }
            //finally { GameContext.PopThreadContext(); }
            //OnTurnPhaseFinished(game, TurnPhase.Intelligence);

            GameLog.Core.Events.DebugFormat("...beginning Morale...");

            OnTurnPhaseChanged(game, TurnPhase.Morale);
            GameContext.PushThreadContext(game);
            try { DoMorale(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Morale);

            GameLog.Core.Events.DebugFormat("...beginning MapUpdates...");

            OnTurnPhaseChanged(game, TurnPhase.MapUpdates);
            GameContext.PushThreadContext(game);
            try { DoMapUpdates(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.MapUpdates);

            GameLog.Core.Events.DebugFormat("...beginning PostTurnOperations...");

            OnTurnPhaseChanged(game, TurnPhase.PostTurnOperations);
            GameContext.PushThreadContext(game);
            try { DoPostTurnOperations(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PostTurnOperations);

            GameLog.Core.Events.DebugFormat("...beginning SendUpdates...");

            OnTurnPhaseChanged(game, TurnPhase.SendUpdates);

            GameLog.Core.Events.DebugFormat("...beginning PushThreadContext...");

            GameContext.PushThreadContext(game);
            try
            {
                foreach (var scriptedEvent in game.ScriptedEvents)
                {
                    //if (GameContext.Current.TurnNumber >= 50)
                        scriptedEvent.OnTurnFinished(game);
                }
                   
            }
            finally { GameContext.PopThreadContext(); }


            GameLog.Core.Events.DebugFormat("...HandleFleetLocationChanged...");


            foreach (var fleet in fleets)
                fleet.LocationChanged -= HandleFleetLocationChanged;
        }
        #endregion

        #region HandleFleetLocationChanged() Method
        private void HandleFleetLocationChanged(object sender, EventArgs e)
        {
            var fleet = sender as Fleet;
            if (fleet != null)
                OnFleetLocationChanged(fleet);
        }
        #endregion

        #region DoPreTurnOperations() Method
        private void DoPreTurnOperations(GameContext game)
        {
            
            var objects = GameContext.Current.Universe.Objects.ToHashSet();
            var civManagers = GameContext.Current.CivilizationManagers.ToHashSet();
            var fleets = objects.OfType<Fleet>().ToHashSet();
            var errors = new System.Collections.Concurrent.ConcurrentStack<Exception>();

            IntelHelper.ExecuteIntelIncomingOrders();

            GameLog.Core.General.DebugFormat("resetting items...");
            ParallelForEach(objects, item =>
            {
                GameContext.PushThreadContext(game);
                // GameLog.Core.General.DebugFormat("next item will be: ID = {0}, Name = {1}", item.ObjectID, item.Name);
                try
                {
                    // GameLog.Core.General.DebugFormat("item: ID = {0}, Name = {1} is trying to reset", item.ObjectID, item.Name);
                    item.Reset();
                    // works well but gives hidden info
                    // GameLog.Core.General.DebugFormat("item: ID = {0}, Name = {1} is successfully resetted", item.ObjectID, item.Name);
                }
                catch (Exception e)
                {
                    GameLog.Core.General.ErrorFormat("***** catch error e item: ID = {0}, Name = {1}", item.ObjectID, item.Name);
                    errors.Push(e);
                }
                finally
                {
                    GameContext.PushThreadContext(game);
                }
            });

            if (!errors.IsEmpty)
                throw new AggregateException(errors);

            errors.Clear();



            ParallelForEach(civManagers, civManager =>
            {
                GameContext.PushThreadContext(game);
                civManager.SitRepEntries.Clear();
                try
                {
                    civManager.SitRepEntries.Clear();

                    try
                    {
                        var civSitReps = IntelHelper.SitReps_Temp.Where(o => o.Owner == civManager.Civilization).ToList();
                        foreach (var entry in civSitReps)
                        {
                            civManager.SitRepEntries.Add(entry);
                        }
                    }
                    catch { }
                }
                catch (Exception e)
                {
                    errors.Push(e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });

            IntelHelper.SitReps_Temp.Clear();

            if (!errors.IsEmpty)
                throw new AggregateException(innerExceptions: errors);

            // This block is not guaranteed to be safe for parallel execution.
            GameContext.PushThreadContext(game);
            foreach (Fleet fleet in fleets)
            {
                if (fleet.Order != null)
                {
                    fleet.Order.OnTurnBeginning();
                }
            }
        }
        #endregion

        #region DoPreGameSetup() Method
        public void DoPreGameSetup(GameContext game)
        {
            var errors = new ConcurrentStack<Exception>();

            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    if (!GameContext.Current.CivilizationManagers.Contains(civ.CivID))
                    {

                        GameContext.Current.CivilizationManagers.Add(new CivilizationManager(game, civ));
                        GameLog.Core.General.DebugFormat("New civ added: {0}", civ.Name);
                    }
                }
                catch (Exception e)
                {
                    errors.Push(e);
                }
                GameContext.PopThreadContext();
            });

            if (!errors.IsEmpty)
                throw new AggregateException(errors);

            DoMapUpdates(game);

            //GameLog.Print("GameVersion = {0}", GameContext.Current.GameMod.Version);
            GameLog.Core.General.InfoFormat("Options: ---------------------------");
            GameLog.Core.General.InfoFormat("Options: GalaxySize = {0} ({1} x {2})", GameContext.Current.Options.GalaxySize, GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);
            GameLog.Core.General.InfoFormat("Options: GalaxyShape = {0}", GameContext.Current.Options.GalaxyShape);
            GameLog.Core.General.InfoFormat("Options: StarDensity = {0}", GameContext.Current.Options.StarDensity);
            GameLog.Core.General.InfoFormat("Options: PlanetDensity = {0}", GameContext.Current.Options.PlanetDensity);
            GameLog.Core.General.InfoFormat("Options: StartingTechLevel = {0}", GameContext.Current.Options.StartingTechLevel);
            GameLog.Core.General.InfoFormat("Options: MinorRaceFrequency = {0}", GameContext.Current.Options.MinorRaceFrequency);
            GameLog.Core.General.InfoFormat("Options: GalaxyCanon = {0}", GameContext.Current.Options.GalaxyCanon);
            GameLog.Core.General.InfoFormat("Options: ---------------------------");
            GameLog.Core.General.InfoFormat("Options: FederationPlayable = {0}", GameContext.Current.Options.FederationPlayable);
            GameLog.Core.General.InfoFormat("Options: RomulanPlayable = {0}", GameContext.Current.Options.RomulanPlayable);
            GameLog.Core.General.InfoFormat("Options: KlingonPlayable = {0}", GameContext.Current.Options.KlingonPlayable);
            GameLog.Core.General.InfoFormat("Options: CardassianPlayable = {0}", GameContext.Current.Options.CardassianPlayable);
            GameLog.Core.General.InfoFormat("Options: DominionPlayable = {0}", GameContext.Current.Options.DominionPlayable);
            GameLog.Core.General.InfoFormat("Options: BorgPlayable = {0}", GameContext.Current.Options.BorgPlayable);
            GameLog.Core.General.InfoFormat("Options: TerranEmpirePlayable = {0}", GameContext.Current.Options.TerranEmpirePlayable);
            GameLog.Core.General.InfoFormat("Options: ---------------------------");
            GameLog.Core.General.InfoFormat("Options: FederationModifier = {0}", GameContext.Current.Options.FederationModifier);
            GameLog.Core.General.InfoFormat("Options: RomulanModifier = {0}", GameContext.Current.Options.RomulanModifier);
            GameLog.Core.General.InfoFormat("Options: KlingonModifier = {0}", GameContext.Current.Options.KlingonModifier);
            GameLog.Core.General.InfoFormat("Options: CardassianModifier = {0}", GameContext.Current.Options.CardassianModifier);
            GameLog.Core.General.InfoFormat("Options: DominionModifier = {0}", GameContext.Current.Options.DominionModifier);
            GameLog.Core.General.InfoFormat("Options: BorgModifier = {0}", GameContext.Current.Options.BorgModifier);
            GameLog.Core.General.InfoFormat("Options: TerranEmpireModifier = {0}", GameContext.Current.Options.TerranEmpireModifier);

            GameLog.Core.General.InfoFormat("Options: EmpireModifierRecurringBalancing = {0}", GameContext.Current.Options.EmpireModifierRecurringBalancing);
            GameLog.Core.General.InfoFormat("Options: GamePace = {0}", GameContext.Current.Options.GamePace);
            GameLog.Core.General.InfoFormat("Options: TurnTimer = {0}", GameContext.Current.Options.TurnTimerEnum);

            /* With StrengthModifier it is possible to increase some stuff or to decrease */
            /* default value is 1.0 - range shall be 0.1 to 1.9 */
            /* all modifier are working in generell, not race-speficic */
            Table strengthTable = GameContext.Current.Tables.StrengthTables["StrengthModifier"];
            float EspionageMod = Number.ParseSingle(strengthTable["EspionageMod"][0]);
            float SabotageMod = Number.ParseSingle(strengthTable["SabotageMod"][0]);
            float InternalSecurityMod = Number.ParseSingle(strengthTable["InternalSecurityMod"][0]);
            float ShipProductionMod = Number.ParseSingle(strengthTable["ShipProductionMod"][0]);
            float ScienceSpeedMod = Number.ParseSingle(strengthTable["ScienceSpeedMod"][0]);
            float MinorPowerMod = Number.ParseSingle(strengthTable["MinorPowerMod"][0]);
            float MiningMod = Number.ParseSingle(strengthTable["MiningMod"][0]);
            float CreditsMod = Number.ParseSingle(strengthTable["CreditsMod"][0]);
            float DiplomacyTrustMod = Number.ParseSingle(strengthTable["DiplomacyTrustMod"][0]);
            float DiplomacyRegardMod = Number.ParseSingle(strengthTable["DiplomacyRegardMod"][0]);
            float FoodProductionMod = Number.ParseSingle(strengthTable["FoodProductionMod"][0]);
            float RaidingMod = Number.ParseSingle(strengthTable["RaidingMod"][0]);
            float ShipVisibilityMod = Number.ParseSingle(strengthTable["ShipVisibilityMod"][0]);
            float StationsStrenghtMod = Number.ParseSingle(strengthTable["StationsStrenghtMod"][0]);
            float OrbitalBatteryStrenghtMod = Number.ParseSingle(strengthTable["OrbitalBatteryStrenghtMod"][0]);
            float TroopTransportStrenghtMod = Number.ParseSingle(strengthTable["TroopTransportStrenghtMod"][0]);
            float ColonyTroopStrenghtMod = Number.ParseSingle(strengthTable["ColonyTroopStrenghtMod"][0]);

            var newline = Environment.NewLine;
            GameLog.Core.GameInitData.DebugFormat("StrengthModifier: " + newline +
                "EspionageMod = {0}" + newline +
                "SabotageMod = {1}" + newline + 
                "InternalSecurityMod = {2}" + newline +
                "ShipProductionMod = {3}" + newline +
                "ScienceSpeedMod = {4}" + newline +
                "MinorPowerMod = {5}" + newline +
                "MiningMod = {6}" + newline +
                "CreditsMod = {7}" + newline + 
                "DiplomacyTrustMod = {8}" + newline +
                "DiplomacyRegardMod = {9}" + newline +
                "FoodProductionMod = {10}" + newline +
                "RaidingMod = {11}" + newline +
                "ShipVisibilityMod = {12}" + newline +
                "StationsStrenghtMod = {13}" + newline +
                "OrbitalBatteryStrenghtMod = {14}" + newline +
                "TroopTransportStrenghtMod = {15}" + newline +
                "ColonyTroopStrenghtMod = {16}" + newline 
                , EspionageMod
                , SabotageMod
                , InternalSecurityMod
                , ShipProductionMod
                , ScienceSpeedMod
                , MinorPowerMod
                , MiningMod
                , CreditsMod
                , DiplomacyTrustMod
                , DiplomacyRegardMod
                , FoodProductionMod
                , RaidingMod
                , ShipVisibilityMod
                , StationsStrenghtMod
                , OrbitalBatteryStrenghtMod
                , TroopTransportStrenghtMod
                , ColonyTroopStrenghtMod
                );



            game.TurnNumber = 1;
        }
        #endregion

        #region DoFleetMovement() Method
        private void DoFleetMovement(GameContext game)
        {
            var allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();

            foreach (var fleet in allFleets)
            {
                //If the fleet is stranded and out of fuel range, it can't move
                if (fleet.IsStranded && !fleet.IsFleetInFuelRange())
                {
                    if (fleet.IsRouteLocked)
                        fleet.UnlockRoute();

                    fleet.SetRoute(TravelRoute.Empty);
                }

                var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
                int fuelNeeded;
                var fuelRange = civManager.MapData.GetFuelRange(fleet.Location);
                
                /*
                 * If the fleet is within fueling range, then try to top off the reserves of
                 * each ship in the fleet.  We do this now in case a ship is out of fuel, but
                 * is now within fueling range, thus ensuring the ship will be able to move.
                 */
                if (!fleet.IsInTow && (fleet.Range >= fuelRange))
                {
                    foreach (var ship in fleet.Ships)
                    {
                        fuelNeeded = ship.FuelReserve.Maximum - ship.FuelReserve.CurrentValue;

                        ship.FuelReserve.AdjustCurrent(
                            civManager.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));
                    }
                }

                //Move the ships along their route
                for (var i = 0; i < fleet.Speed; i++)
                {
                    if (fleet.MoveAlongRoute())
                    {
                        fleet.AdjustCrewExperience(5);
                    }
                    else
                    {
                        if (i == 0)
                            fleet.AdjustCrewExperience(1);
                        break;
                    }

                    fuelRange = civManager.MapData.GetFuelRange(fleet.Location);

                    foreach (var ship in fleet.Ships)
                    {
                        /*
                         * For each ship in the fleet, deplete the fuel reserves by a 1 unit
                         * of Deuterium.  Then, if the fleet is within fueling range, attempt
                         * to replenish that unit from the global stockpile.
                         */
                        fuelNeeded = ship.FuelReserve.AdjustCurrent(-1);

                        if (fleet.Range >= fuelRange)
                        {
                            ship.FuelReserve.AdjustCurrent(
                                civManager.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));
                        }
                        //ship.FuelReserve.UpdateAndReset();
                    }
                }

                if (fleet.Sector.System != null && (fleet.Sector.System.StarType == StarType.BlackHole))
                {
                    int shipsDamaged = 0;
                    int shipsDestroyed = 0;



                    if (fleet.Ships != null) // Update FixBlackholeCrash (hopefully) 2 March 2019
                    {
                        
                        foreach (var ship in fleet.Ships)
                        {
                            int damage = RandomHelper.Roll(ship.HullStrength.CurrentValue);
                            if (damage >= ship.HullStrength.CurrentValue)
                            {
                                shipsDestroyed++;
                                ship.Destroy();
                            }
                            else
                            {
                                shipsDamaged++;
                                ship.HullStrength.AdjustCurrent(-damage);
                            }
                            if (fleet.Ships != null) // 2nd try to fix blackhole 3 march 2019
                                break;
                        }

                    }
                

                    if ((shipsDamaged > 0) || (shipsDestroyed > 0))
                    {
                        civManager.SitRepEntries.Add(new BlackHoleEncounterSitRepEntry(fleet.Owner, fleet.Location, shipsDamaged, shipsDestroyed));
                    }
                }
            }
        }
        #endregion

        #region DoDiplomacy() Method
        private void DoDiplomacy()
        {
            /*
             * Process pending actions.
             */
            foreach (var civ1 in GameContext.Current.Civilizations)
            {
                foreach (var civ2 in GameContext.Current.Civilizations)
                {
                    if (civ1 == civ2)
                        continue;

                    if (civ1.CivID == 6 || civ1.Key == "BORG")
                    {
                        //GameLog.Core.Diplomacy.DebugFormat("civ1 = {0}, civ2 = {1}, foreignPower = {2}, foreignPowerStatus = {3}", civ1, civ2, foreignPower, foreignPowerStatus);
                        continue; // Borg don't accept anything
                    }
                    if (civ2.CivID == 6 || civ2.Key == "BORG")
                    {
                        continue; // Borg don't accept anything
                    }
                    var diplomat1 = Diplomat.Get(civ1);
                    var diplomat2 = Diplomat.Get(civ2);
                    if (diplomat1.GetForeignPower(civ2).DiplomacyData.Status == Diplomacy.ForeignPowerStatus.NoContact ||
                        diplomat2.GetForeignPower(civ1).DiplomacyData.Status == Diplomacy.ForeignPowerStatus.NoContact)
                        {
                            continue;
                        }
                    if (!civ2.IsEmpire && civ1.IsEmpire) // only a minor vs a major
                    {
                        foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
                        {
                            if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ1 && aCiv != civ2)
                            {
                                //
                                GameLog.Core.Diplomacy.DebugFormat("I** civ1= {2} civ2 = {3} aCiv = {0} status = {1}", aCiv, Diplomat.Get(aCiv).GetForeignPower(civ2).DiplomacyData.Status.ToString(), civ1.Key, civ2.Key);
                                var diplomatOther = Diplomat.Get(aCiv);
                                var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ2).DiplomacyData.Status;
                                if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.CounterpartyIsMember) // || otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.OwnerIsMember)
                                {
                                    continue;
                                }
                            }
                        }
                          
                    }
                    if (!civ1.IsEmpire && civ2.IsEmpire)
                    {
                        foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
                        {
                            if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ2 && aCiv != civ1)
                            {
                                var diplomatOther = Diplomat.Get(aCiv);
                                var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ1).DiplomacyData.Status;
                                if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.CounterpartyIsMember || otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.OwnerIsMember)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    if (civ2.IsEmpire && civ2.IsHuman && civ1.IsEmpire) // only a minor vs a major
                    {
                        foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
                        {
                            if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ1 && aCiv != civ2)
                            {
                               // GameLog.Client.Test.DebugFormat("C** civ1= {2} civ2 = {3} aCiv = {0} status = {1}", aCiv, Diplomat.Get(aCiv).GetForeignPower(civ2).DiplomacyData.Status.ToString(), civ1.Key, civ2.Key);
                                var diplomatOther = Diplomat.Get(aCiv);
                                var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ2).DiplomacyData.Status;
                                if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.Allied) 
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    if (civ1.IsEmpire && civ1.IsHuman && civ2.IsEmpire) // only a minor vs a major
                    {
                        foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
                        {
                            if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ2 && aCiv != civ1)
                            {
                                var diplomatOther = Diplomat.Get(aCiv);
                                var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ1).DiplomacyData.Status;
                                if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.Allied)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    var ForeignPower = diplomat1.GetForeignPower(civ2);
                    var ForeignPowerStatus = diplomat1.GetForeignPower(civ2).DiplomacyData.Status;
                    GameLog.Core.Diplomacy.DebugFormat("---------------------------------------");
                    GameLog.Core.Diplomacy.DebugFormat("foreignPowerStatus = {2} for {0} vs {1}", civ1, civ2, ForeignPowerStatus);

                    switch (ForeignPower.PendingAction)
                    {
                        case PendingDiplomacyAction.AcceptProposal:
                                            GameLog.Core.Diplomacy.DebugFormat("AcceptProposal = {2} for {0} vs {1}, pending {3}", civ1, civ2, ForeignPowerStatus, ForeignPower.PendingAction.ToString());
                            if (ForeignPower.LastProposalReceived != null)
                                        AcceptProposalVisitor.Visit(ForeignPower.LastProposalReceived);
                            break;
                        case PendingDiplomacyAction.RejectProposal:
                                            GameLog.Core.Diplomacy.DebugFormat("RejectProposal = {2} for {0} vs {1}, pending {3}", civ1, civ2, ForeignPowerStatus, ForeignPower.PendingAction.ToString());
                            if (ForeignPower.LastProposalReceived != null)
                                        RejectProposalVisitor.Visit(ForeignPower.LastProposalReceived);                            
                            break;
                    }

                    ForeignPower.PendingAction = PendingDiplomacyAction.None;
                }
            }

            var civManagers = GameContext.Current.CivilizationManagers;

            /*
             * Schedule delivery of outbound messages
             */
            foreach (var civ1 in GameContext.Current.Civilizations)
            {
                var diplomat = Diplomat.Get(civ1);

                foreach (var civ2 in GameContext.Current.Civilizations)
                {
                    if (civ1 == civ2)
                        continue;

                    var foreignPower = diplomat.GetForeignPower(civ2);

                    var proposalSent = foreignPower.ProposalSent;
                    if (proposalSent != null)
                    {
                        foreignPower.CounterpartyForeignPower.ProposalReceived = proposalSent;
                        foreignPower.LastProposalSent = proposalSent;
                        foreignPower.ProposalSent = null;

                        if (civ1.IsEmpire)
                            civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, proposalSent));

                        if (civ2.IsEmpire)
                            civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, proposalSent));
                    }
                    else
                    {
                        foreignPower.CounterpartyForeignPower.ProposalReceived = null;
                    }

                    var statementSent = foreignPower.StatementSent;
                    if (statementSent != null)
                    {
                        foreignPower.CounterpartyForeignPower.StatementReceived = statementSent;
                        foreignPower.LastStatementSent = statementSent;
                        foreignPower.StatementSent = null;

                        if (statementSent.StatementType == StatementType.WarDeclaration)
                            foreignPower.DeclareWar();
                    }
                    else
                    {
                        foreignPower.CounterpartyForeignPower.StatementReceived = null;
                    }

                    var responseSent = foreignPower.ResponseSent;
                    if (responseSent != null)
                    {
                        foreignPower.CounterpartyForeignPower.ResponseReceived = responseSent;
                        foreignPower.LastResponseSent = responseSent;
                        foreignPower.ResponseSent = null;

                        if (responseSent.ResponseType != ResponseType.NoResponse &&
                            !(responseSent.ResponseType == ResponseType.Accept && responseSent.Proposal.IsGift()))
                        {
                            if (civ1.IsEmpire)
                                civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, responseSent));

                            if (civ2.IsEmpire)
                                civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
                        }
                    }
                    else
                    {
                        foreignPower.CounterpartyForeignPower.ResponseReceived = null;
                    }
                }
            }

            /*
             * Fulfull agreement obligations
             */
            foreach (var agreement in GameContext.Current.AgreementMatrix)
                AgreementFulfillmentVisitor.Visit(agreement);
        }
        #endregion

        #region DoSpyOperations() Method - move this to end of turn after the orders were given. Results will show up in the next turn


        //private void DoSpyOperations()
        //{
        //    /*
        //     * Process pending actions.... collected ...like SabotageEnergy or SabotageCredits
        //     */
        //    foreach (var attacker in GameContext.Current.Civilizations)
        //    {
        //        if (attacker.IsEmpire)
        //        {
        //            foreach (var victim in GameContext.Current.Civilizations)
        //            {
        //                if (attacker == victim || !victim.IsEmpire)
        //                    continue;

        //                // minor races are out, only empire1 vs empire2

        //                //var Spy_2_Power = victim;
        //                var attacker1 = Diplomat.Get(attacker);
        //                //var Spy_2_Power = new Spy_2_Power

        //                ////var ForeignPowerStatus = diplomat1.GetForeignPower(civ2).DiplomacyData.Status;

        //                //switch (Spy_2_Power.PendingAction)
        //                //{
        //                //    case SpyActionExecute.DoItSo:
        //                //        if (Spy_2_Power.LastProposalReceived != null)
        //                //            AcceptProposalVisitor.Visit(ForeignPower.LastProposalReceived);
        //                //        break;
        //                //    //case PendingDiplomacyAction.RejectProposal:
        //                //    //    if (ForeignPower.LastProposalReceived != null)
        //                //    //        RejectProposalVisitor.Visit(ForeignPower.LastProposalReceived);
        //                //    //    break;
        //                //}

        //                GameLog.Core.Intel.DebugFormat("DoSpyOperations....doing the operations {0} VS {1}", attacker, victim);


        //                Colony _seat = GameContext.Current.CivilizationManagers[victim].SeatOfGovernment;

        //                //IntelHelper.StealCredits(_seat, attacker, victim, "Terrorists"); // ?? Do we need to save blame string in CivilizationManager

        //                //Spy_2_Power.PendingSpyAction = SpyActionExecute.Done;



        //                //ForeignPower.PendingAction = PendingDiplomacyAction.None;


        //                //        if (civ1.CivID == 6 || civ1.Key == "BORG")
        //                //        {
        //                //            //GameLog.Core.Diplomacy.DebugFormat("civ1 = {0}, civ2 = {1}, foreignPower = {2}, foreignPowerStatus = {3}", civ1, civ2, foreignPower, foreignPowerStatus);
        //                //            continue; // Borg don't accept anything
        //                //        }
        //                //        if (civ2.CivID == 6 || civ2.Key == "BORG")
        //                //        {
        //                //            continue; // Borg don't accept anything
        //                //        }
        //                //        var diplomat1 = Diplomat.Get(civ1);
        //                //        var diplomat2 = Diplomat.Get(civ2);
        //                //        if (diplomat1.GetForeignPower(civ2).DiplomacyData.Status == Diplomacy.ForeignPowerStatus.NoContact ||
        //                //            diplomat2.GetForeignPower(civ1).DiplomacyData.Status == Diplomacy.ForeignPowerStatus.NoContact)
        //                //        {
        //                //            continue;
        //                //        }
        //                //        if (!civ2.IsEmpire && civ1.IsEmpire) // only a minor vs a major
        //                //        {
        //                //            foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
        //                //            {
        //                //                if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ1 && aCiv != civ2)
        //                //                {
        //                //                    //
        //                //                    GameLog.Core.Diplomacy.DebugFormat("I** civ1= {2} civ2 = {3} aCiv = {0} status = {1}", aCiv, Diplomat.Get(aCiv).GetForeignPower(civ2).DiplomacyData.Status.ToString(), civ1.Key, civ2.Key);
        //                //                    var diplomatOther = Diplomat.Get(aCiv);
        //                //                    var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ2).DiplomacyData.Status;
        //                //                    if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.CounterpartyIsMember) // || otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.OwnerIsMember)
        //                //                    {
        //                //                        continue;
        //                //                    }
        //                //                }
        //                //            }

        //                //        }
        //                //        if (!civ1.IsEmpire && civ2.IsEmpire)
        //                //        {
        //                //            foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
        //                //            {
        //                //                if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ2 && aCiv != civ1)
        //                //                {
        //                //                    var diplomatOther = Diplomat.Get(aCiv);
        //                //                    var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ1).DiplomacyData.Status;
        //                //                    if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.CounterpartyIsMember || otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.OwnerIsMember)
        //                //                    {
        //                //                        continue;
        //                //                    }
        //                //                }
        //                //            }
        //                //        }
        //                //        if (civ2.IsEmpire && civ2.IsHuman && civ1.IsEmpire) // only a minor vs a major
        //                //        {
        //                //            foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
        //                //            {
        //                //                if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ1 && aCiv != civ2)
        //                //                {
        //                //                    // GameLog.Client.Test.DebugFormat("C** civ1= {2} civ2 = {3} aCiv = {0} status = {1}", aCiv, Diplomat.Get(aCiv).GetForeignPower(civ2).DiplomacyData.Status.ToString(), civ1.Key, civ2.Key);
        //                //                    var diplomatOther = Diplomat.Get(aCiv);
        //                //                    var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ2).DiplomacyData.Status;
        //                //                    if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.Allied)
        //                //                    {
        //                //                        continue;
        //                //                    }
        //                //                }
        //                //            }
        //                //        }
        //                //        if (civ1.IsEmpire && civ1.IsHuman && civ2.IsEmpire) // only a minor vs a major
        //                //        {
        //                //            foreach (Civilization aCiv in GameContext.Current.Civilizations) // not already a member with other empire
        //                //            {
        //                //                if (aCiv.IsEmpire && aCiv.CivID != 6 && aCiv != civ2 && aCiv != civ1)
        //                //                {
        //                //                    var diplomatOther = Diplomat.Get(aCiv);
        //                //                    var otherForeignPowerStatus = diplomatOther.GetForeignPower(civ1).DiplomacyData.Status;
        //                //                    if (otherForeignPowerStatus == Diplomacy.ForeignPowerStatus.Allied)
        //                //                    {
        //                //                        continue;
        //                //                    }
        //                //                }
        //                //            }
        //                //        }
        //                //        var ForeignPower = diplomat1.GetForeignPower(civ2);
        //                //        var ForeignPowerStatus = diplomat1.GetForeignPower(civ2).DiplomacyData.Status;
        //                //        GameLog.Core.Diplomacy.DebugFormat("---------------------------------------");
        //                //        GameLog.Core.Diplomacy.DebugFormat("foreignPowerStatus = {2} for {0} vs {1}", civ1, civ2, ForeignPowerStatus);

        //                //        switch (ForeignPower.PendingAction)
        //                //        {
        //                //            case PendingDiplomacyAction.AcceptProposal:
        //                //                if (ForeignPower.LastProposalReceived != null)
        //                //                    AcceptProposalVisitor.Visit(ForeignPower.LastProposalReceived);
        //                //                break;
        //                //            case PendingDiplomacyAction.RejectProposal:
        //                //                if (ForeignPower.LastProposalReceived != null)
        //                //                    RejectProposalVisitor.Visit(ForeignPower.LastProposalReceived);
        //                //                break;
        //                //        }

        //                //        ForeignPower.PendingAction = PendingDiplomacyAction.None;
        //                //    }
        //            }
        //        }
        //        //var civManagers = GameContext.Current.CivilizationManagers;

        //        ///*
        //        // * Schedule delivery of outbound messages
        //        // */
        //        //foreach (var civ1 in GameContext.Current.Civilizations)
        //        //{
        //        //    var diplomat = Diplomat.Get(civ1);

        //        //    foreach (var civ2 in GameContext.Current.Civilizations)
        //        //    {
        //        //        if (civ1 == civ2)
        //        //            continue;

        //        //        var foreignPower = diplomat.GetForeignPower(civ2);

        //        //        var proposalSent = foreignPower.ProposalSent;
        //        //        if (proposalSent != null)
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.ProposalReceived = proposalSent;
        //        //            foreignPower.LastProposalSent = proposalSent;
        //        //            foreignPower.ProposalSent = null;

        //        //            if (civ1.IsEmpire)
        //        //                civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, proposalSent));

        //        //            if (civ2.IsEmpire)
        //        //                civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, proposalSent));
        //        //        }
        //        //        else
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.ProposalReceived = null;
        //        //        }

        //        //        var statementSent = foreignPower.StatementSent;
        //        //        if (statementSent != null)
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.StatementReceived = statementSent;
        //        //            foreignPower.LastStatementSent = statementSent;
        //        //            foreignPower.StatementSent = null;

        //        //            if (statementSent.StatementType == StatementType.WarDeclaration)
        //        //                foreignPower.DeclareWar();
        //        //        }
        //        //        else
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.StatementReceived = null;
        //        //        }

        //        //        var responseSent = foreignPower.ResponseSent;
        //        //        if (responseSent != null)
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.ResponseReceived = responseSent;
        //        //            foreignPower.LastResponseSent = responseSent;
        //        //            foreignPower.ResponseSent = null;

        //        //            if (responseSent.ResponseType != ResponseType.NoResponse &&
        //        //                !(responseSent.ResponseType == ResponseType.Accept && responseSent.Proposal.IsGift()))
        //        //            {
        //        //                if (civ1.IsEmpire)
        //        //                    civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, responseSent));

        //        //                if (civ2.IsEmpire)
        //        //                    civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            foreignPower.CounterpartyForeignPower.ResponseReceived = null;
        //        //        }
        //        //}
        //    }

        //        ///*
        //        // * Fulfull agreement obligations
        //        // */
        //        //foreach (var agreement in GameContext.Current.AgreementMatrix)
        //        //    AgreementFulfillmentVisitor.Visit(agreement);
        //    }
        #endregion SpyOperations

        #region DoCombat() Method
        void DoCombat(GameContext game)
        {
            var combatLocations = new HashSet<MapLocation>();
            var invasionLocations = new HashSet<MapLocation>();
            var combats = new List<List<CombatAssets>>();
            var invasions = new List<InvasionArena>();
            var fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet)).ToList();
  
            foreach (var fleet in fleetsAtLocation)
            {
                if (!combatLocations.Contains(fleet.Location))
                {
                        var assets = CombatHelper.GetCombatAssets(fleet.Location);
                        var fleetsOwners = fleetsAtLocation
                            .Select(o => o.Owner)
                            .Distinct()
                            .ToList();

                    if (assets.Count > 1 && fleetsOwners.Count > 1)
                    {
                        foreach (var nextFleet in fleetsAtLocation)
                            if (fleet.Owner == nextFleet.Owner ||
                                CombatHelper.WillFightAlongside(fleet.Owner, nextFleet.Owner) ||
                                !CombatHelper.WillEngage(fleet.Owner, nextFleet.Owner))
                                continue;
                        combats.Add(assets); // we add all the ships at this location if there is any combat. Combat decides who is in and on what side
                        combatLocations.Add(fleet.Location);                     
                    }
                }
                if (!invasionLocations.Contains(fleet.Location))
                {
                    if (fleet.Order is AssaultSystemOrder)
                    {
                        invasions.Add(new InvasionArena(fleet.Sector.System.Colony, fleet.Owner));
                        invasionLocations.Add(fleet.Location);
                    }
                }
            }

            foreach (var combat in combats)
            {
                CombatReset.Reset();
                GameLog.Core.Combat.DebugFormat("---- COMBAT OCCURED GameEngine --------------------");
                OnCombatOccurring(combat);
                CombatReset.WaitOne();
            }

            foreach (var invasion in invasions)
            {
                CombatReset.Reset();
                OnInvasionOccurring(invasion);
                CombatReset.WaitOne();
            }

            var invadingFleets = invasions
                .SelectMany(o => o.InvadingUnits)
                .OfType<InvasionOrbital>()
                .Where(o => !o.IsDestroyed)
                .Select(o => o.Source)
                .OfType<Ship>()
                .Select(o => o.Fleet)
                .Distinct()
                .ToList();

            foreach (var invadingFleet in invadingFleets)
            {
                var assaultOrder = invadingFleet.Order as AssaultSystemOrder;
                if (assaultOrder != null && !assaultOrder.IsValidOrder(invadingFleet))
                    invadingFleet.SetOrder(invadingFleet.GetDefaultOrder());
            }

            ParallelForEach(GameContext.Current.Universe.Find<Colony>(), c =>
            {
                GameContext.PushThreadContext(game);
                try { c.RefreshShielding(true); }
                finally { GameContext.PopThreadContext(); }
            });
        }
        #endregion
    
        #region DoPopulation() Method
        void DoPopulation(GameContext game)
        {
            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ.CivID];

                    civManager.TotalPopulation.Reset();

                    foreach (Colony colony in civManager.Colonies)
                    {
                        colony.Population.Maximum = colony.MaxPopulation;

                        int popChange = 0;
                        int foodDeficit;

                        colony.FoodReserves.AdjustCurrent(colony.GetProductionOutput(ProductionCategory.Food));
                        foodDeficit = Math.Min(colony.FoodReserves.CurrentValue - colony.Population.CurrentValue, 0);
                        colony.FoodReserves.AdjustCurrent(-1 * colony.Population.CurrentValue);
                        colony.FoodReserves.UpdateAndReset();

                        /*
                         * If there is not enough food to feed the population, we need to kill off some of the
                         * population due to starvation.  Otherwise, we increase the population according to the
                         * growth rate iff we did not suffer a loss due to starvation during the previous turn.
                         * We want to ensure that there is a 1-turn period between population loss and recovery.
                         */
                        var growthRate = colony.GrowthRate;
                        if (foodDeficit < 0)
                        {
                            popChange = -(int)Math.Floor(0.1 * Math.Sqrt(Math.Abs(colony.Population.CurrentValue * foodDeficit)));
                            civManager.SitRepEntries.Add(new StarvationSitRepEntry(civ, colony));
                        }
                        else
                        {                         
                            popChange = (int)Math.Ceiling(growthRate * colony.Population.CurrentValue);
                        }

                        if (growthRate < 0)
                        {
                            civManager.SitRepEntries.Add(new PopulationDyingSitRepEntry(civ, colony));
                        }

                        int newPopulation = colony.Population.AdjustCurrent(popChange);

                        // TODO: We need to figure out how to deal with a civilization having no colonies
                        // and no colony ships
                        if (colony.Population.CurrentValue == 0)
                        {
                            civManager.SitRepEntries.Add(new PopulationDiedSitRepEntry(civ, colony));
                            colony.Destroy();
                            civManager.EnsureSeatOfGovernment();
                            return;
                        }
                        colony.Population.UpdateAndReset();
                        civManager.TotalPopulation.AdjustCurrent(colony.Population.CurrentValue);

                        if (newPopulation < colony.Population.Maximum)
                        {
                            var foodFacilityType = colony.GetFacilityType(ProductionCategory.Food);
                            if ((foodFacilityType != null) && (colony.GetAvailableLabor() >= foodFacilityType.LaborCost))
                            {
                                int popInThreeTurns = Math.Min(colony.Population.Maximum,
                                    (int)(newPopulation * (1 + colony.GrowthRate) * (1 + colony.GrowthRate) * (1 + colony.GrowthRate)));
                                while (popInThreeTurns > colony.GetProductionOutput(ProductionCategory.Food))
                                {
                                    if (!colony.ActivateFacility(ProductionCategory.Food))
                                        break;
                                }
                            }
                        }

                        while (colony.ActivateFacility(ProductionCategory.Industry))
                            continue;
                    }

                    civManager.EnsureSeatOfGovernment();
                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoResearch() Method
        private void DoResearch(GameContext game)
        {
            var scienceShips = game.Universe.Find<Ship>(UniverseObjectType.Ship).Where(s => s.ShipType == ShipType.Science);
            ParallelForEach(scienceShips, scienceShip =>
            {
                GameContext.PushThreadContext(game);
                if (scienceShip.Sector.System == null)
                {
                    return;
                }
                //GameLog.Core.Research.DebugFormat("{0} {1} is conducting research in {2}...",
                //    scienceShip.ObjectID, scienceShip.Name, scienceShip.Sector);

                try
                {
                    var owner = GameContext.Current.CivilizationManagers[scienceShip.Owner];
                    var starType = scienceShip.Sector.System.StarType;
                    if (scienceShip.Location == owner.HomeSystem.Location)
                        return;

                    int researchGained = (int)(scienceShip.ShipDesign.ScanStrength * scienceShip.ShipDesign.ScienceAbility);
                    GameLog.Core.Research.DebugFormat("Base research gained for {0} {1} is {2}",
                        scienceShip.ObjectID, scienceShip.Name, researchGained);

                    switch (starType)
                    {
                        case StarType.Blue:
                        case StarType.Orange:
                        case StarType.Red:
                        case StarType.White:
                        case StarType.Yellow:
                            researchGained = researchGained * 10;
                            break;
                        case StarType.BlackHole:
                            researchGained = researchGained * 20;
                            break;
                        case StarType.Nebula:
                            researchGained = researchGained * 10;
                            break;
                        case StarType.NeutronStar:
                            researchGained = researchGained * 10;
                            break;
                        case StarType.Quasar:
                            researchGained = researchGained * 10;
                            break;
                        case StarType.RadioPulsar:
                            researchGained = researchGained * 10;
                            break;
                        case StarType.Wormhole:
                            researchGained = researchGained * 30;
                            break;
                        case StarType.XRayPulsar:
                            researchGained = researchGained * 10;
                            break;
                        default:
                            researchGained = 1;
                            break;
                    }

                    GameContext.Current.CivilizationManagers[scienceShip.Owner].Research.UpdateResearch(researchGained);


                    GameLog.Core.Research.DebugFormat("{0} {1} gained {2} research points for {3} by studying the {4} in {5}",
                        scienceShip.ObjectID, scienceShip.Name, researchGained, owner.Civilization.Key, starType, scienceShip.Sector);

                    GameContext.Current.CivilizationManagers[owner].SitRepEntries.Add(new ScienceShipResearchGainedSitRepEntry(owner.Civilization, scienceShip, researchGained));
                }
                catch (Exception e)
                {
                    // Check TechObj for correct (formatted) values for ScienceAbility and ScanStrength
                    GameLog.Core.Research.ErrorFormat(string.Format("##### There was a problem conducting research for {0} {1}",
                        scienceShip.ObjectID, scienceShip.Name),
                        e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });

            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    var civManager = GameContext.Current.CivilizationManagers[civ.CivID];
                    civManager.Research.UpdateResearch(
                        civManager.Colonies.Sum(c => c.GetProductionOutput(ProductionCategory.Research)));

                }
                catch (Exception e)
                {
                    GameLog.Core.General.Error(string.Format("DoResearch failed for {0}",
                        civ.Name),
                        e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoMapUpdates() Method
        private void DoMapUpdates(GameContext game)
        {
            DoSectorClaims(game);

            GameContext.PushThreadContext(game);

            var map = game.Universe.Map;
            
            var interference = new Task<int[,]>(() =>
            {
                var array = new int[map.Width, map.Height];

                GameContext.PushThreadContext(game);
                try
                {
                    foreach (StarSystem starSystem in game.Universe.Find(UniverseObjectType.StarSystem))
                        StarHelper.ApplySensorInterference(array, starSystem);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }

                return array;
            });

            interference.Start();
            
            ParallelForEach(game.Civilizations, civ => 
            {
                GameContext.PushThreadContext(game);
                try
                {
                    var fuelLocations = new HashSet<MapLocation>();
                    var civManager = game.CivilizationManagers[civ];
                    var mapData = civManager.MapData;

                    mapData.ResetScanStrengthAndFuelRange();
                    //fleets
                    foreach (var fleet in game.Universe.FindOwned<Fleet>(civ))
                    {
                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from FLEET {0} {1} ({2}) at {3}, ScanStrength = {4}, Range = {5}", fleet.ObjectID, fleet.Name, 
                        //    fleet.Owner, fleet.Location, fleet.ScanStrength, fleet.SensorRange);
                        mapData.UpgradeScanStrength(
                            fleet.Location,
                            fleet.ScanStrength,
                            fleet.SensorRange,
                            0,
                            1);
                    }
                    //stations
                    foreach (var station in game.Universe.FindOwned<Station>(civ))
                    {
                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from STATION {0} {1} ({2}) at {3}, ScanStrength = {4}, Range = {5}", station.ObjectID, station.Name, 
                        //    station.Owner, station.Location, station.StationDesign.ScanStrength, station.StationDesign.SensorRange);
                        mapData.UpgradeScanStrength(
                            station.Location,
                            station.StationDesign.ScanStrength,
                            station.StationDesign.SensorRange,
                            0,
                            1);

                        fuelLocations.Add(station.Location);
                    }

                    foreach (var colony in civManager.Colonies)
                    {
                        int scanModifier = 0;
                            
                        var scanBonuses = colony.Buildings
                            .Where(o => o.IsActive)
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => o.BonusType == BonusType.ScanRange)
                            .Select(o => o.Amount);

                        if (scanBonuses.Any())
                            scanModifier = scanBonuses.Max();

                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from COLONY {0} {1} ({2}) at  {3}, ScanStrength = {4}, Range = {5}", colony.ObjectID, colony.Name, 
                        //    colony.Owner, colony.Location, 1 + scanModifier, 1 + scanModifier);  
                        mapData.UpgradeScanStrength(
                            colony.Location,
                            1 + scanModifier,
                            1 + scanModifier,
                            0,
                            1);

                        if (colony.Shipyard != null)
                            fuelLocations.Add(colony.Location);
                    }

                    for (var x = 0; x < map.Width; x++)
                    {
                        for (var y = 0; y < map.Height; y++)
                        {
                            var sector = map[x, y];

                            foreach (var fuelLocation in fuelLocations)
                            {    
                                mapData.UpgradeFuelRange(
                                    sector.Location,
                                    MapLocation.GetDistance(fuelLocation, sector.Location));
                            }
                        }
                    }

                    mapData.ApplyScanInterference(interference.Result);
                }
                catch(Exception e)
                {
                    GameLog.Core.General.ErrorFormat(string.Format("DoMapUpdate failed for {0}",
                        civ.Name),
                        e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoSectorClaims() Method
        private void DoSectorClaims(GameContext game)
        {
            var map = game.Universe.Map;
            var sectorClaims = game.SectorClaims;

            sectorClaims.ClearClaims();

            ParallelForEach(GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList(), civ => {
                GameContext.PushThreadContext(game);
                try
                {
                    var civManager = GameContext.Current.CivilizationManagers[civ];

                    foreach (var colony in civManager.Colonies)
                    {
                        var minX = colony.Location.X;
                        var minY = colony.Location.Y;
                        var maxX = colony.Location.X;
                        var maxY = colony.Location.Y;
                        var radius = Math.Min(colony.Population.CurrentValue / 100, 3);

                        minX = Math.Max(0, minX - radius);
                        minY = Math.Max(0, minY - radius);
                        maxX = Math.Min(map.Width - 1, maxX + radius);
                        maxY = Math.Min(map.Height - 1, maxY + radius);

                        for (var x = minX; x <= maxX; x++)
                        {
                            for (var y = minY; y <= maxY; y++)
                            {
                                var location = new MapLocation(x, y);

                                var claimWeight = colony.Population.CurrentValue / (MapLocation.GetDistance(location, colony.Location) + 1);

                                if (claimWeight <= 0)
                                    continue;

                                lock (sectorClaims)
                                    sectorClaims.AddClaim(location, civ, claimWeight);

                                civManager.MapData.SetScanned(location, true);
                                //GameLog.Core.MapData.DebugFormat("{0} (Colony owner: {1}): SetScanned to -> True ", location.ToString(), colony.Owner);
                            }
                        }
                    }

                    if (civ.IsHuman)
                        civManager.DesiredBorders = new ConvexHullSet(Enumerable.Empty<ConvexHull>());
                    //PlayerAI.CreateDesiredBorders(civ);

                }
                catch (Exception e)
                {
                    GameLog.Core.General.ErrorFormat(string.Format("DoSectorClaims failed for {0}",
                        civ.Name),
                        e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoScrapping() Method
        void DoScrapping(GameContext game)
        {
            var priorThreadContext = GameContext.ThreadContext;
            GameContext.PushThreadContext(game);
            try
            {
                foreach (var scrappedObject in game.Universe.Find<TechObject>().Where(o => o.Scrap))
                    game.Universe.Scrap(scrappedObject);

                var colonies = game.Civilizations
                    .Select(o => game.CivilizationManagers[o.CivID])
                    .SelectMany(o => o.Colonies);

                foreach (var colony in colonies)
                    game.Universe.ScrapNonStructures(colony);
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }
        #endregion

        #region DoMaintenance() Method
        private void DoMaintenance(GameContext game)
        {
            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];
                foreach (Colony colony in civManager.Colonies)
                {
                    colony.EnsureEnergyForBuildings();
                }

                foreach (TechObject item in GameContext.Current.Universe.FindOwned<TechObject>(civ))
                {
                    civManager.Credits.AdjustCurrent(-item.Design.MaintenanceCost);
                }
            }
        }
        #endregion

        #region DoProduction() Method
        void DoProduction(GameContext game)
        {
            /*
             * Break down production by civilization.  We want to use resources
             * from both the colonies and the global reserves, so this is the
             * sensible way to do it.
             */
            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameLog.Core.Production.DebugFormat("#####################################################");
                GameLog.Core.Production.DebugFormat("DoProduction for Civilization {0}", civ.Name);

                GameContext.PushThreadContext(game);
                try
                {
                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ.CivID];
                    var colonies = new List<Colony>(civManager.Colonies);

                    /*
                     * Update the civilization's treasury and resource stockpile to include anything that was
                     * generated by this colony. *EVERYTHING* must be add to the global pool prior to checking
                     * for negative and possibly blocking production.
                     */
                    int newCredits = colonies.Sum(c => c.TaxCredits);
                    int newIntelligenceDefense = colonies.Sum(c => c.NetIntelligence) * 3 / 10; // 30 % into Defense
                    int newIntelligenceAttacking = colonies.Sum(c => c.NetIntelligence) * 7 / 10; // 70 % into AttackingAccumulation
                    int newDeuterium = colonies.Sum(c => c.NetDeuterium);
                    int newDilithium = colonies.Sum(c => c.NetDilithium);
                    int newRawMaterials = colonies.Sum(c => c.NetRawMaterials);

                    civManager.Credits.AdjustCurrent(newCredits);
                    civManager.TotalIntelligenceDefenseAccumulated.AdjustCurrent(newIntelligenceDefense); 
                    civManager.TotalIntelligenceAttackingAccumulated.AdjustCurrent(newIntelligenceAttacking);
                    civManager.Resources.Deuterium.AdjustCurrent(newDeuterium);
                    civManager.Resources.Dilithium.AdjustCurrent(newDilithium);
                    civManager.Resources.RawMaterials.AdjustCurrent(newRawMaterials);

                    GameLog.Core.Production.DebugFormat("{0} credits, {1} deuterium, {2} dilithium, {3} raw materials added from all colonies to {4} ",
                        newCredits, newDeuterium, newDilithium, newRawMaterials, civManager.Civilization);
                    GameLog.Client.UI.DebugFormat("Civ Manager ={0} TotalIntelDefenseAccumulated ={1}, TotalIntelAccumulated ={2}",
                        civManager.Civilization.Key,
                        civManager.TotalIntelligenceDefenseAccumulated.CurrentValue,
                        civManager.TotalIntelligenceAttackingAccumulated.CurrentValue);
                    //Get the resources available for the civilization
                    ResourceValueCollection totalResourcesAvailable = new ResourceValueCollection();
                    totalResourcesAvailable[ResourceType.Deuterium] = civManager.Resources.Deuterium.CurrentValue;
                    totalResourcesAvailable[ResourceType.Dilithium] = civManager.Resources.Dilithium.CurrentValue;
                    totalResourcesAvailable[ResourceType.RawMaterials] = civManager.Resources.RawMaterials.CurrentValue;

                    GameLog.Core.Production.DebugFormat("{0} credits, {1} deuterium, {2} dilithium, {3} raw materials available in total for {4}"
                        , civManager.Credits.CurrentValue
                        , civManager.Resources.Deuterium.CurrentValue
                        , civManager.Resources.Dilithium.CurrentValue
                        , civManager.Resources.RawMaterials.CurrentValue
                        , civManager.Civilization);

                    /* 
                     * Shuffle the colonies so they are processed in random order.  This
                     * will help prevent the same colonies from getting priority when
                     * the global stockpiles are low.
                     */
                    colonies.RandomizeInPlace();

                    /* Iterate through each colony */
                    foreach (Colony colony in colonies)
                    {
                        GameLog.Core.Production.DebugFormat("--------------------------------------------------------------");
                        GameLog.Core.Production.DebugFormat("DoProduction for Colony {0}", colony.Name, civ.Name, civManager.Credits);

                        //See if there is actually anything to build for this colony
                        if (!colony.BuildSlots[0].HasProject && colony.BuildQueue.IsEmpty())
                        {
                            GameLog.Core.Production.DebugFormat("Nothing to do for Colony {0} ({1})", colony.Name, civ.Name);
                            continue;
                        }

                        if (!colony.IsProductionAutomated)
                            colony.ClearBuildPrioritiesAndConsolidate();

                        /* We want to capture the industry output available at the colony. */
                        int industry = colony.NetIndustry;

                        //Start going through the queue
                        while ((industry > 0) && ((!colony.BuildQueue.IsEmpty()) || colony.BuildSlots[0].HasProject))
                        {
                            //Move the top of the queue in to the build slot
                            if (!colony.BuildSlots[0].HasProject)
                            {
                                colony.ProcessQueue();
                            }

                            //Check to see if the colony has reached the limit for this building
                            if (TechTreeHelper.IsBuildLimitReached(colony, colony.BuildSlots[0].Project.BuildDesign))
                            {
                                GameLog.Core.Production.WarnFormat("Removing {0} from queue on {1} ({2}) - Build Limit Reached", colony.BuildSlots[0].Project.BuildDesign.Name, colony.Name, civ.Name);
                                colony.BuildSlots[0].Project.Cancel();
                                break;
                            }

                            if (colony.BuildSlots[0].Project.IsPaused) { }
                                //TODO: Not sure how to handle this

                            GameLog.Core.Production.DebugFormat("Deuterium={5}, Dilithium={6}, RawMaterials={7} available for {0} before construction of {1} on {2} - Income Tax = {3}, Income TradeRoute = {4}: ",
                                civ.Name,
                                colony.BuildSlots[0].Project.BuildDesign.Name,
                                colony.Name,
                                colony.TaxCredits,
                                colony.CreditsFromTrade,
                                totalResourcesAvailable[ResourceType.Deuterium],
                                totalResourcesAvailable[ResourceType.Dilithium],
                                totalResourcesAvailable[ResourceType.RawMaterials]);

                            //Try to finish the projects
                            if (colony.BuildSlots[0].Project.IsRushed)
                            {
                                // Rushing a project should have no impact on the industry of colony (since it's all been paid for)
                                int tmpIndustry = colony.BuildSlots[0].Project.GetCurrentIndustryCost();
                                ResourceValueCollection tmpResources = new ResourceValueCollection();
                                tmpResources[ResourceType.Deuterium] = 999999;
                                tmpResources[ResourceType.Dilithium] = 999999;
                                tmpResources[ResourceType.RawMaterials] = 999999;
                                civManager.Credits.AdjustCurrent(colony.BuildSlots[0].Project.GetTotalCreditsCost());
                                colony.BuildSlots[0].Project.Advance(ref tmpIndustry, tmpResources);
                                GameLog.Core.Production.DebugFormat("{0} credits applied to {1} on {2} ({3})",
                                    tmpIndustry,
                                    colony.BuildSlots[0].Project.BuildDesign.Name,
                                    colony.Name,
                                    civ.Name);
                            }
                            else
                            {
                                ResourceValueCollection totalResourcesBefore = totalResourcesAvailable.Clone();
                                colony.BuildSlots[0].Project.Advance(ref industry, totalResourcesAvailable);

                                //Figure out how what resources have been used
                                int deuteriumUsed = totalResourcesBefore[ResourceType.Deuterium] - totalResourcesAvailable[ResourceType.Deuterium];
                                int dilithiumUsed = totalResourcesBefore[ResourceType.Dilithium] - totalResourcesAvailable[ResourceType.Dilithium];
                                int rawMaterialsUsed = totalResourcesBefore[ResourceType.RawMaterials] - totalResourcesAvailable[ResourceType.RawMaterials];

                                GameLog.Core.ShipProduction.DebugFormat("{0} deuterium, {1} dilithium, {2} raw materials applied to {3} on {4}",
                                    deuteriumUsed, dilithiumUsed, rawMaterialsUsed, colony.BuildSlots[0].Project, colony);

                                civManager.Resources.Deuterium.AdjustCurrent(-1 * deuteriumUsed);
                                civManager.Resources.Dilithium.AdjustCurrent(-1 * dilithiumUsed);
                                civManager.Resources.RawMaterials.AdjustCurrent(-1 * rawMaterialsUsed);
                            }

                            if (colony.BuildSlots[0].Project.IsCompleted)
                            {
                                GameLog.Core.Production.DebugFormat("Construction of {0} finished on {1} ({2})", colony.BuildSlots[0].Project.BuildDesign.Name, colony.Name, civ.Name);
                                colony.BuildSlots[0].Project.Finish();
                                colony.BuildSlots[0].Project = null;
                                continue;
                            }
                        }

                        if (!colony.BuildSlots[0].HasProject && colony.BuildQueue.IsEmpty())
                            civManager.SitRepEntries.Add(new BuildQueueEmptySitRepEntry(civ, colony, false));
                    }
                }
                catch (Exception e)
                {
                    GameLog.Core.Production.Error(string.Format("DoProduction failed for {0}", civ.Name), e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoShipProduction() Method
        private void DoShipProduction(GameContext game)
        {
            /*
             * Break down production by civilization.  We want to use resources
             * from both the colonies and the global reserves, so this is the
             * sensible way to do it.
             */
            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    var civManager = GameContext.Current.CivilizationManagers[civ.CivID];
                    //Get all colonies with a shipyard
                    var colonies = civManager.Colonies.Where(c => c.Shipyard != null).ToList();
                    /* 
                     * Shuffle the colonies so they are processed in random order. This
                     * will help prevent the same colonies from getting priority when
                     * the global stockpiles are low.
                     */
                    colonies.RandomizeInPlace();

                    //Get the resources available for the civilization
                    ResourceValueCollection totalResourcesAvailable = new ResourceValueCollection();
                    totalResourcesAvailable[ResourceType.Deuterium] = civManager.Resources.Deuterium.CurrentValue;
                    totalResourcesAvailable[ResourceType.Dilithium] = civManager.Resources.Dilithium.CurrentValue;
                    totalResourcesAvailable[ResourceType.RawMaterials] = civManager.Resources.RawMaterials.CurrentValue;

                    foreach (var colony in colonies)
                    {
                        var shipyard = colony.Shipyard;
                        var queue = shipyard.BuildQueue;

                        List<ShipyardBuildSlot> buildSlots = colony.Shipyard.BuildSlots.Where(s => s.IsActive && !s.OnHold).ToList();
                        foreach (ShipyardBuildSlot slot in buildSlots)
                        {

                            // GameLog is making trouble
                            /*GameLog.Core.ShipProduction.DebugFormat("Resources available for {0} before construction of {1} on {2}: Deuterium={3}, Dilithium={4}, RawMaterials={5}",
                                civ.Name,
                                slot.Project.BuildDesign.Name,
                                colony.Name,
                                totalResourcesAvailable[ResourceType.Deuterium],
                                totalResourcesAvailable[ResourceType.Dilithium],
                                totalResourcesAvailable[ResourceType.RawMaterials]);
                             */

                            int output = shipyard.GetBuildOutput(slot.SlotID);
                            while ((slot.HasProject || !shipyard.BuildQueue.IsEmpty()) && (output > 0))
                            {
                                if (!slot.HasProject)
                                {
                                    shipyard.ProcessQueue();
                                }
                                if (!slot.HasProject && shipyard.BuildQueue.IsEmpty())
                                {
                                    GameLog.Core.ShipProduction.DebugFormat("Nothing to do for Shipyard Slot {0} on {1} ({2})",
                                        slot.SlotID,
                                        colony.Name,
                                        civ.Name);
                                    continue;
                                }

                                ResourceValueCollection totalResourcesBefore = totalResourcesAvailable.Clone();
                                slot.Project.Advance(ref output, totalResourcesAvailable);

                                //Figure out how what resources have been used
                                int deuteriumUsed = totalResourcesBefore[ResourceType.Deuterium] - totalResourcesAvailable[ResourceType.Deuterium];
                                int dilithiumUsed = totalResourcesBefore[ResourceType.Dilithium] - totalResourcesAvailable[ResourceType.Dilithium];
                                int rawMaterialsUsed = totalResourcesBefore[ResourceType.RawMaterials] - totalResourcesAvailable[ResourceType.RawMaterials];
                                civManager.Resources.Deuterium.AdjustCurrent(-1 * deuteriumUsed);
                                civManager.Resources.Dilithium.AdjustCurrent(-1 * dilithiumUsed);
                                civManager.Resources.RawMaterials.AdjustCurrent(-1 * rawMaterialsUsed);

                                GameLog.Core.ShipProduction.DebugFormat("{0} deuterium, {1} dilithium, {2} raw materials applied to {3} on {4}",
                                    deuteriumUsed, dilithiumUsed, rawMaterialsUsed, slot.Project, colony);

                                if (slot.Project.IsCompleted)
                                {
                                    GameLog.Core.ShipProduction.DebugFormat("{0} in Shipyard Slot {1} on {2} ({3}) is finished",
                                        slot.Project.BuildDesign,
                                        slot.SlotID,
                                        colony.Name,
                                        civ.Name);
                                    slot.Project.Finish();
                                    slot.Project = null;
                                }
                                else
                                {
                                    //if there is a gap for raw materials than code would go into never ending loop without the break
                                    break;
                                }
                            }
                        }
                    }
                }        
                catch (Exception e)
                {
                    GameLog.Core.ShipProduction.Error(string.Format("DoShipProduction failed for {0}", civ.Name), e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoMorale() Method
        void DoMorale(GameContext game)
        {
            var errors = new System.Collections.Concurrent.ConcurrentStack<Exception>();

            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    var globalMorale = 0;
                    var civManager = GameContext.Current.CivilizationManagers[civ.CivID];

                    /* Calculate any empire-wide morale bonuses */
                    foreach (var bonus in civManager.GlobalBonuses)
                    {
                        if (bonus.BonusType == BonusType.MoraleEmpireWide)
                            globalMorale += bonus.Amount;
                    }

                    /* Iterate through each colony. */
                    foreach (var colony in civManager.Colonies)
                    {
                        /* Add the empire-wide morale adjustments. */
                        colony.Morale.AdjustCurrent(globalMorale);

                        /* Add any morale bonuses from active buildings at the colony. */
                        var colonyBonus = (from building in colony.Buildings
                                            where building.IsActive
                                            from bonus in building.BuildingDesign.Bonuses
                                            where bonus.BonusType == BonusType.Morale
                                            select bonus.Amount).Sum();

                        colony.Morale.AdjustCurrent(colonyBonus);

                        /*
                         * If morale has not changed in this colony for any reason, then we will
                         * cause the morale level to drift towards the founding civilization's
                         * base morale level.
                         */
                        if (colony.Morale.CurrentChange == 0)
                        {
                            int drift = 0;
                            var originalCiv = colony.OriginalOwner;

                            //We're below the base, so drift up to it
                            if (colony.Morale.CurrentValue < originalCiv.BaseMoraleLevel)
                            {
                                drift = originalCiv.MoraleDriftRate;
                            }
                            //We're above the base, so drift down to it
                            else if (colony.Morale.CurrentValue > originalCiv.BaseMoraleLevel)
                            {
                                drift = -originalCiv.MoraleDriftRate;
                            }

                            colony.Morale.AdjustCurrent(drift);
                        }

                        colony.Morale.UpdateAndReset();
                    }
                }
                catch (Exception e)
                {
                    errors.Push(e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });

            if (!errors.IsEmpty)
                throw new AggregateException(errors);
        }
        #endregion

        #region DoIntelligence() Method // move this to post turn operations, see results next turn

        //void DoIntelligence(GameContext game)
        //{
        //    var innateDefense = 200;
        //    var chanceOfAttemptSucceeding = 100;
        //    var minProductionFacilitiesToLeave = 5;

        //    ParallelForEach(GameContext.Current.Civilizations, civ =>
        //    {
        //        GameContext.PushThreadContext(game);
        //        try
        //        {
        //            if (!civ.IsEmpire)
        //                return;

        //            var attackingEmpire = GameContext.Current.CivilizationManagers[civ.CivID];
        //            if (attackingEmpire.TotalIntelligenceProduction <= 0)
        //            {
        //                GameLog.Core.Intel.DebugFormat("{0} has no intel power so cannot attack");
        //                return;
        //            }

        //            //Get a list of all viable target empire
        //            var targets = GameContext.Current.Civilizations
        //                //All empires
        //                .Where(t => t.IsEmpire)
        //                //That aren't themselves
        //                .Where(t => t.CivID != civ.CivID)
        //                //That they have actually met
        //                .Where(t => DiplomacyHelper.IsContactMade(civ, t));


        //            GameLog.Core.Intel.DebugFormat("Available intel targets for {0}: {1}", civ.Name, targets.Count());

        //            //Double check that we have viable targets
        //            if (targets.Count() == 0)
        //                return;

        //            // ToDo: let player chose a target
        //            //Select one at random
        //            CivilizationManager targetEmpire = GameContext.Current.CivilizationManagers[targets.RandomElement()];
        //            GameLog.Core.Intel.DebugFormat("{0} is targeting empire {1}...", civ.Name, targetEmpire.Civilization.Name);

        //            //Randomly pick one of their colonies to attack
        //            Colony targetColony = targetEmpire.Colonies.RandomElement();

        //            GameLog.Core.Intel.DebugFormat("{0} is targeting colony {1}...", civ.Name, targetColony.Name);

        //            int defenseIntelligence = targetEmpire.TotalIntelligenceProduction + innateDefense;
        //            int attackIntelligience = attackingEmpire.TotalIntelligenceProduction;

        //            //Get the ratio of the attacking power to defending power
        //            int ratio = attackIntelligience / defenseIntelligence;

        //            //We need at least a ratio of greater than 1 to attack
        //            if (ratio < 1)
        //            {
        //                GameLog.Core.Intel.DebugFormat("{0} doesn't have enough attacking intelligence to make an attack against {1} - {2} vs {3}",
        //                    attackingEmpire.Civilization.Name, targetEmpire.Civilization.Name, attackIntelligience, defenseIntelligence);
        //                return;
        //            }

        //            //For each 1 ratio, the attacking empire has a chance of performing an action, or failing, to a maximum of 4
        //            int attempts;
        //            if (ratio > 4)
        //            {
        //                attempts = 4;
        //            }
        //            else
        //            {
        //                attempts = ratio;
        //            }

        //            for (int i = 0; i < attempts; i++)
        //            {
        //                int action = RandomHelper.Roll(chanceOfAttemptSucceeding);

        //                if (action < 9)
        //                {
        //                    /*
        //                     * Adjust morale
        //                     */
        //                    //-2 morale at target colony
        //                    targetColony.Morale.AdjustCurrent(-2);
        //                    targetColony.Morale.UpdateAndReset();
        //                    GameLog.Core.Intel.DebugFormat("Morale at {0} reduced by 2 to {1}", targetColony.Name, targetColony.Morale.CurrentValue);

        //                    //-1 morale at target home colony
        //                    targetEmpire.HomeColony.Morale.AdjustCurrent(-1);
        //                    targetEmpire.HomeColony.Morale.UpdateAndReset();
        //                    GameLog.Core.Intel.DebugFormat("Morale on {0} reduced by 1 to {1}", targetEmpire.HomeColony.Name, targetEmpire.HomeColony.Morale.CurrentValue);

        //                    //-1 morale at target seat of government
        //                    targetEmpire.SeatOfGovernment.Morale.AdjustCurrent(-1);
        //                    targetEmpire.SeatOfGovernment.Morale.UpdateAndReset();
        //                    GameLog.Core.Intel.DebugFormat("Morale on {0} reduced by 1 to {1}", targetEmpire.SeatOfGovernment.Name, targetEmpire.SeatOfGovernment.Morale.CurrentValue);

        //                    //Morale +1 to attacker HomeColony
        //                    attackingEmpire.HomeColony.Morale.AdjustCurrent(+1);
        //                    attackingEmpire.HomeColony.Morale.UpdateAndReset();
        //                    GameLog.Core.Intel.DebugFormat("Morale on {0} increased by 1 to {1}", attackingEmpire.HomeColony.Name, attackingEmpire.HomeColony.Morale.CurrentValue);
        //                }

        //                //Steal Money
        //                if (action == 1)
        //                {
        //                    //If we're going for their main planet, target the central treasury
        //                    if ((targetColony == targetEmpire.HomeColony) || (targetColony == targetEmpire.SeatOfGovernment))
        //                    {
        //                        if (targetEmpire.Credits.CurrentValue > 0)
        //                        {
        //                            int stolenCredits = RandomHelper.Roll(targetEmpire.Credits.CurrentValue);

        //                            targetEmpire.Credits.AdjustCurrent(-1 * stolenCredits);
        //                            targetEmpire.Credits.UpdateAndReset();
        //                            attackingEmpire.Credits.AdjustCurrent(stolenCredits);
        //                            attackingEmpire.Credits.UpdateAndReset();

        //                            GameLog.Core.Intel.DebugFormat("{0} stole {1} credits from the {2} treasury", attackingEmpire.Civilization.Name, stolenCredits, targetEmpire.Civilization.Name);

        //                            targetEmpire.SitRepEntries.Add(new CreditsStolenTargetSitRepEntry(targetEmpire.Civilization, targetColony, stolenCredits));
        //                            attackingEmpire.SitRepEntries.Add(new CreditsStolenAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, stolenCredits));
        //                        }
        //                    }
        //                    else //Otherwise siphon credits from trade route
        //                    {
        //                        if (targetColony.CreditsFromTrade.CurrentValue > 0)
        //                        {
        //                            int stolenCredits = RandomHelper.Roll(targetColony.CreditsFromTrade.CurrentValue);
        //                            targetColony.CreditsFromTrade.AdjustCurrent(stolenCredits * -1);
        //                            targetColony.CreditsFromTrade.UpdateAndReset();
        //                            attackingEmpire.Credits.AdjustCurrent(stolenCredits);
        //                            attackingEmpire.Credits.UpdateAndReset();

        //                            GameLog.Core.Intel.DebugFormat("{0} stole {1} credits from the trade routes on {2}", attackingEmpire.Civilization.Name, stolenCredits, targetColony.Name);

        //                            targetEmpire.SitRepEntries.Add(new TradeRouteCreditsStolenTargetSitRepEntry(targetEmpire.Civilization, targetColony, stolenCredits));
        //                            attackingEmpire.SitRepEntries.Add(new TradeRouteCreditsStolenAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, stolenCredits));
        //                        }
        //                    }
        //                }

        //                //Target their food reserves
        //                else if (action == 2)
        //                {
        //                    if (targetColony.FoodReserves.CurrentValue > 0)
        //                    {
        //                        int destroyedFoodReserves = RandomHelper.Roll(targetColony.FoodReserves.CurrentValue);

        //                        targetColony.FoodReserves.AdjustCurrent(destroyedFoodReserves * -1);
        //                        targetColony.FoodReserves.UpdateAndReset();

        //                        GameLog.Core.Intel.DebugFormat("{0} destroyed {1} food at {2}", attackingEmpire.Civilization.Name, destroyedFoodReserves, targetColony.Name);

        //                        targetEmpire.SitRepEntries.Add(new FoodReservesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, destroyedFoodReserves));
        //                        attackingEmpire.SitRepEntries.Add(new FoodReservesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, destroyedFoodReserves));
        //                    }
        //                }

        //                //Target their food production
        //                else if (action == 3)
        //                {
        //                    if (targetColony.GetTotalFacilities(ProductionCategory.Food) <= minProductionFacilitiesToLeave)
        //                    {
        //                        continue;
        //                    }

        //                    int destroyedFoodFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Food) - minProductionFacilitiesToLeave);
        //                    targetColony.RemoveFacilities(ProductionCategory.Food, destroyedFoodFacilities);

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} food faciliities at {2}", attackingEmpire.Civilization.Name, destroyedFoodFacilities, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Food, destroyedFoodFacilities));
        //                    attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Food, destroyedFoodFacilities));
        //                }

        //                //Target industrial production
        //                else if (action == 4)
        //                {
        //                    if (targetColony.GetTotalFacilities(ProductionCategory.Industry) <= minProductionFacilitiesToLeave)
        //                    {
        //                        continue;
        //                    }

        //                    int destroyedIndustrialFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Industry) - minProductionFacilitiesToLeave);
        //                    targetColony.RemoveFacilities(ProductionCategory.Industry, destroyedIndustrialFacilities);

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} industrial faciliities at {2}", attackingEmpire.Civilization.Name, destroyedIndustrialFacilities, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Industry, destroyedIndustrialFacilities));
        //                    attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Industry, destroyedIndustrialFacilities));
        //                }

        //                //Target energy production
        //                else if (action == 5)
        //                {
        //                    if (targetColony.GetTotalFacilities(ProductionCategory.Energy) <= minProductionFacilitiesToLeave)
        //                    {
        //                        continue;
        //                    }

        //                    int destroyedEnergyFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Energy) - minProductionFacilitiesToLeave);
        //                    targetColony.RemoveFacilities(ProductionCategory.Energy, destroyedEnergyFacilities);

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} energy faciliities at {2}", attackingEmpire.Civilization.Name, destroyedEnergyFacilities, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Energy, destroyedEnergyFacilities));
        //                    attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Energy, destroyedEnergyFacilities));
        //                }

        //                //Target research facilities
        //                else if (action == 6)
        //                {
        //                    if (targetColony.GetTotalFacilities(ProductionCategory.Research) <= minProductionFacilitiesToLeave)
        //                    {
        //                        continue;
        //                    }

        //                    int destroyedResearchFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Research) - minProductionFacilitiesToLeave);
        //                    targetColony.RemoveFacilities(ProductionCategory.Research, destroyedResearchFacilities);

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} research facilities at {2}", attackingEmpire.Civilization.Name, destroyedResearchFacilities, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Research, destroyedResearchFacilities));
        //                    attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Research, destroyedResearchFacilities));
        //                }

        //                //Target intel facilities
        //                else if (action == 7)
        //                {
        //                    if (targetColony.GetTotalFacilities(ProductionCategory.Intelligence) <= minProductionFacilitiesToLeave)
        //                    {
        //                        continue;
        //                    }

        //                    int destroyedIntelFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Intelligence) - minProductionFacilitiesToLeave);
        //                    targetColony.RemoveFacilities(ProductionCategory.Intelligence, destroyedIntelFacilities);

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} intelligence facilities at {2}", attackingEmpire.Civilization.Name, destroyedIntelFacilities, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Intelligence, destroyedIntelFacilities));
        //                    attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Intelligence, destroyedIntelFacilities));
        //                }

        //                //Target planetary defenses
        //                else if (action == 8)
        //                {
        //                    int destroyedOrbitalBatteries = 0;
        //                    int shieldStrengthLost = 0;

        //                    if (targetColony.OrbitalBatteries.Count > 1)
        //                    {
        //                        destroyedOrbitalBatteries = RandomHelper.Roll(targetColony.OrbitalBatteries.Count);
        //                        targetColony.RemoveOrbitalBatteries(destroyedOrbitalBatteries);
        //                    }

        //                    if (targetColony.ShieldStrength.CurrentValue > 0) {
        //                        shieldStrengthLost = RandomHelper.Roll(targetColony.ShieldStrength.CurrentValue);
        //                        targetColony.ShieldStrength.AdjustCurrent(-1 * shieldStrengthLost);
        //                        targetColony.ShieldStrength.UpdateAndReset();
        //                    }

        //                    GameLog.Core.Intel.DebugFormat("{0} destroyed {1} orbital batteries and removed {2} strength from planetary shields at {3}",
        //                        attackingEmpire.Civilization.Name, destroyedOrbitalBatteries, shieldStrengthLost, targetColony.Name);

        //                    targetEmpire.SitRepEntries.Add(new PlanetaryDefenceAttackTargetSitRepEntry(targetEmpire.Civilization, targetColony, destroyedOrbitalBatteries, shieldStrengthLost));
        //                    attackingEmpire.SitRepEntries.Add(new PlanetaryDefenceAttackAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, destroyedOrbitalBatteries, shieldStrengthLost));
        //                }

        //                //Other possibilties...
        //                //Uncover who attacked us and blaming others 
        //                //Destroy orbiting space station
        //                //Assasination
        //                //Target individual buildings
        //                //Bombing
        //                //Target research (destroy)
        //                //Target research (steal)

        //                //Attack failed
        //                //else
        //                //{
        //                //    targetEmpire.SitRepEntries.Add(new IntelDefenceSucceededSitRepEntry(targetEmpire.Civilization, targetColony));
        //                //    attackingEmpire.SitRepEntries.Add(new IntelAttackFailedSitRepEntry(attackingEmpire.Civilization, targetColony));
        //                //}
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            GameLog.Core.Intel.ErrorFormat(string.Format("DoIntelligience failed for {0}", civ.Name), e);
        //        }
        //        finally
        //        {
        //            GameContext.PopThreadContext();
        //        }
        //    });
        //}
        #endregion

        #region DoTrade() Method
        void DoTrade(GameContext game)
        {
            Table popReqTable = GameContext.Current.Tables.ResourceTables["TradeRoutePopReq"];
            Table popModTable = GameContext.Current.Tables.ResourceTables["TradeRoutePopMultipliers"];

            float sourceMod = Number.ParseSingle(popModTable["Source"][0]);
            float targetMod = Number.ParseSingle(popModTable["Target"][0]);

            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    int popForTradeRoute;
                    var civManager = GameContext.Current.CivilizationManagers[civ.CivID];

                    /*
                     * See what the minimum population level is for a new trade route for the
                     * current civilization.  If one is not specified, use the default.
                     */
                    if (popReqTable[civManager.Civilization.Key] != null)
                        popForTradeRoute = Number.ParseInt32(popReqTable[civManager.Civilization.Key][0]);
                    else
                        popForTradeRoute = Number.ParseInt32(popReqTable[0][0]);

                    var colonies = GameContext.Current.Universe.FindOwned<Colony>(civ);

                    /* Iterate through each colony... */
                    foreach (var colony in colonies)
                    {
                        /*
                         * For each established trade route, ensure that the target colony is
                         * a valid choice.  If it isn't, break it.  Otherwise, calculate the
                         * revised credit total.
                         */
                        foreach (var route in colony.TradeRoutes)
                        {
                            if (!route.IsValidTargetColony(route.TargetColony))
                            {
                                route.TargetColony = null;
                            }
                            if (route.TargetColony != null)
                            {
                            int sourceIndustry = route.SourceColony.NetIndustry + 1;  // avoiding a zero
                            int targetIndustry = route.TargetColony.NetIndustry + 1;

                            route.Credits = 10 * (int)((sourceMod * sourceIndustry) + (targetMod * targetIndustry));

                            }
                        }
                            
                        /*
                         * Calculate how many trade routes the colony is allowed to have.
                         * Take into consideration any routes added by building bonuses.
                         */
                        int tradeRoutes = colony.Population.CurrentValue / popForTradeRoute;
                            
                        tradeRoutes += colony.Buildings
                            .Where(o => o.IsActive)
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => o.BonusType == BonusType.TradeRoutes)
                            .Sum(o => o.Amount);

                        /*
                         * If the colony doesn't have as many trade routes as it should, then
                         * we need to add some more.
                         */
                        if (tradeRoutes > colony.TradeRoutes.Count)
                        {
                            int tradeRouteDeficit = tradeRoutes - colony.TradeRoutes.Count;
                            for (int i = 0; i < tradeRouteDeficit; i++)
                                colony.TradeRoutes.Add(new TradeRoute(colony));
                        }
                            
                        /*
                         * If the colony has too many trade routes, we need to remove some.
                         * To be generous, we sort them in order of credits generated so that
                         * we remove the least valuable routes.
                         */
                        else if (tradeRoutes < colony.TradeRoutes.Count)
                        {
                            var extraTradeRoutes = colony.TradeRoutes
                                .OrderByDescending(o => o.Credits)
                                .SkipWhile((o, i) => i < tradeRoutes)
                                .ToArray();
                            foreach (var extraTradeRoute in extraTradeRoutes)
                                colony.TradeRoutes.Remove(extraTradeRoute);
                        }

                        /*
                         * Iterate through the remaining trade routes and deposit the credit
                         * income into the civilization's treasury.
                         */
                        foreach (var route in colony.TradeRoutes)
                        {
                            colony.CreditsFromTrade.AdjustCurrent(route.Credits);
                            GameLog.Core.TradeRoutes.DebugFormat("trade route {0}, route is assigned ={1}", route.SourceColony.Owner, route.IsAssigned);
                            if (!route.IsAssigned) // && civManager.SitRepEntries.Any(s=>s.Categories.ToString() == "SpecialEvent"))
                            {
                                GameLog.Core.TradeRoutes.DebugFormat("trade route for {0}, credti {1}=0 should add sitRep", route.SourceColony.Owner, route.SourceColony.CreditsFromTrade.BaseValue);
                                civManager.SitRepEntries.Add(new UnassignedTradeRoute(route));
                            }
                        }
                        /*
                         * Apply all "+% Trade Income" and "+% Credits" bonuses at this colony.
                         */
                        var tradeBonuses = (int)colony.ActiveBuildings
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => ((o.BonusType == BonusType.PercentTradeIncome) || (o.BonusType == BonusType.PercentCredits)))
                            .Sum(o => 0.01f * o.Amount);

                        colony.CreditsFromTrade.AdjustCurrent(tradeBonuses);
                        civManager.Credits.AdjustCurrent(colony.CreditsFromTrade.CurrentValue);
                        colony.ResetCreditsFromTrade();
                    }
                        
                    /* 
                     * Apply all global "+% Total Credits" bonuses for the civilization.  At present, we have now
                     * completed all adjustments to the civilization's treasury for this turn.  If that changes in
                     * the future, we may need to move this operation.
                     */
                    var globalBonusAdjustment = (int)(0.01f * civManager.GlobalBonuses
                        .Where(o => o.BonusType == BonusType.PercentTotalCredits)
                        .Sum(o => o.Amount));
                    civManager.Credits.AdjustCurrent(globalBonusAdjustment);
                }
                catch (Exception e)
                {
                    GameLog.Core.General.DebugFormat(string.Format("DoTrade failed for {0}",
                        civ.Name),
                        e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
        #endregion

        #region DoPostTurnOperations() Method
        private void DoPostTurnOperations(GameContext game)
        {
            //IntelHelper.ExecuteIntelOrders(); // now update results of spy operations on host computer, steal and sabotage, remove production facilities, just before we end the turn

            //GameContext.Current.CivilizationManagers[attackedCiv].Credits.AdjustCurrent(stolenCredits * -1);
            //GameContext.Current.CivilizationManagers[attackedCiv].Credits.UpdateAndReset();
            var destroyedOrbitals = GameContext.Current.Universe.Find<Orbital>(o => o.HullStrength.IsMinimized);
            var allFleets = GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet);

            foreach (var orbital in destroyedOrbitals)
            {
                GameContext.Current.CivilizationManagers[orbital.OwnerID].SitRepEntries.Add(
                    new OrbitalDestroyedSitRepEntry(orbital));
                GameContext.Current.Universe.Destroy(orbital);
            }

            foreach (var fleet in allFleets)
            {
                if (fleet.Ships.Count == 0)
                {
                    if (fleet.Order != null)
                        fleet.Order.OnOrderCancelled();
                    GameContext.Current.Universe.Destroy(fleet);

                }
                else if (fleet.Order != null)
                {
                    fleet.Order.OnTurnEnding();
                }
            }

            foreach (var civManager in GameContext.Current.CivilizationManagers)
            {
                /*
                 * Reset the resource stockpile meters now that we have finished
                 * production for the each civilization.  This will update the
                 * last base value of the meters so that the net change this turn
                 * is properly reflected.  Do the same for the credit treasury.
                 */
                civManager.Resources.UpdateAndReset();
                civManager.Credits.UpdateAndReset();
                //civManager.IntelOrdersGoingToHost.Count;
                civManager.OnTurnFinished();

                // works - just for DEBUG  // optimized for CSV-Export (CopyPaste)
                GameLog.Core.CivsAndRaces.DebugFormat(";Col:;{1};Pop:;{2};Morale:;{3};Credits;{4};Change;{5};Research;{6};{7};for;{0}"
                    , civManager.Civilization.Key
                    , civManager.Colonies.Count
                    , civManager.TotalPopulation
                    , civManager.AverageMorale  
                    , civManager.Credits.CurrentValue
                    , civManager.Credits.CurrentChange

                    , civManager.Research.CumulativePoints
                    , civManager.Civilization.CivilizationType
                    //, civManager.Civilization.IntelOrdersGoingToHost.Count
                    //, civManager.Treasury.GrossIncome  // ;Treasury;{7}  // doesn't work, maybe it's just done with Credits !
                    //, civManager.Treasury.Maintenance  // ;Maint;{8}

                    );
            }

            GameContext.Current.TurnNumber++;
        }
        #endregion

        #region DoAIPlayers() Method
        public void DoAIPlayers(object gameContext, List<Civilization> autoTurnCiv)
        {
            var errors = new System.Collections.Concurrent.ConcurrentStack<Exception>();
            var game = gameContext as GameContext;
            if (game == null)
                throw new ArgumentException("gameContext must be a valid GameContext instance");

            GameContext.PushThreadContext(game);

            try
            {
                ParallelForEach(game.Civilizations, civ =>
                {
                    GameContext.PushThreadContext(game);

                    try
                    {
                        if (civ.IsHuman && !autoTurnCiv.Contains(civ))
                            return;

                        var civManager = GameContext.Current.CivilizationManagers[civ];
                        civManager.DesiredBorders = PlayerAI.CreateDesiredBorders(civ);

                        DiplomatAI.DoTurn(civ);
                        try
                        {
                            if (DiplomacyHelper.IsIndependent(civ))
                            {
                                ColonyAI.DoTurn(civ);
                                UnitAI.DoTurn(civ);
                            }
                        }
                        catch (Exception e)
                        {
                            GameLog.Core.General.Error(e);
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Push(e);
                    }
                    finally
                    {
                        GameContext.PopThreadContext();
                    }
                });
            }
            finally
            {
                GameContext.PopThreadContext();
            }

            if (!errors.IsEmpty)
            {
                //throw new AggregateException(errors);
            }
        }
        #endregion

        #region OnCombatOccurring() Method
        /// <summary>
        /// Raises the <see cref="CombatOccurring"/> event.
        /// </summary>
        /// <param name="combat">The combat assets.</param>
        private void OnCombatOccurring(List<CombatAssets> combat)
        {
            if (combat == null)
                throw new ArgumentNullException("combat");

            var handler = CombatOccurring;
            if (handler != null)
                handler(combat);
        }
        #endregion

        #region OnInvasionOccurring() Method
        /// <summary>
        /// 
        /// Raises the <see cref="InvasionOccurring"/> event.
        /// </summary>
        /// <param name="invasionArena">The invasion arena.</param>
        private void OnInvasionOccurring(InvasionArena invasionArena)
        {
            if (invasionArena == null)
                throw new ArgumentNullException("invasionArena");

            var handler = InvasionOccurring;
            if (handler != null)
                handler(invasionArena);
        }
        #endregion

        #region NotifyCombatFinished() Method
        /// <summary>
        /// Resets the combat wait handle.
        /// </summary>
        public void NotifyCombatFinished()
        {
            CombatReset.Set();
        }
        #endregion

        // ReSharper disable UnusedMethodReturnValue.Local
        private static ParallelLoopResult ParallelForEach<TSource>(
            [NotNull] IEnumerable<TSource> source,
            [NotNull] Action<TSource> body)
        {
            return Parallel.ForEach(
                source,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 4 },
                body);
        }
        // ReSharper restore UnusedMethodReturnValue.Local
    }
}
