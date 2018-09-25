// GameEngine.cs
//
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
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
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
        #endregion

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
                    scriptedEvent.OnTurnPhaseStarted(game, phase);
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
                    scriptedEvent.OnTurnPhaseFinished(game, phase);
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

            HashSet<Fleet> fleets;

            GameContext.PushThreadContext(game);
            try
            {
                var eventsToRemove = game.ScriptedEvents.Where(o => !o.CanExecute).ToList();
                foreach (var eventToRemove in eventsToRemove)
                    game.ScriptedEvents.Remove(eventToRemove);

                //If we've reached turn 20, start running scripted events
                if (GameContext.Current.TurnNumber >= 20)
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

            OnTurnPhaseChanged(game, TurnPhase.PreTurnOperations);
            GameContext.PushThreadContext(game);
            try { DoPreTurnOperations(game); }       
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PreTurnOperations);

            OnTurnPhaseChanged(game, TurnPhase.FleetMovement);
            GameContext.PushThreadContext(game);
            try { DoFleetMovement(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.FleetMovement);

            OnTurnPhaseChanged(game, TurnPhase.Diplomacy);
            GameContext.PushThreadContext(game);
            try { DoDiplomacy(); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Diplomacy);

            OnTurnPhaseChanged(game, TurnPhase.Combat);
            GameContext.PushThreadContext(game);
            try { DoCombat(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Combat);

            OnTurnPhaseChanged(game, TurnPhase.PopulationGrowth);
            GameContext.PushThreadContext(game);
            try { DoPopulation(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PopulationGrowth);

            OnTurnPhaseChanged(game, TurnPhase.Research);
            GameContext.PushThreadContext(game);
            try { DoResearch(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Research);

            OnTurnPhaseChanged(game, TurnPhase.Scrapping);
            GameContext.PushThreadContext(game);
            try { DoScrapping(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Scrapping);

            OnTurnPhaseChanged(game, TurnPhase.Maintenance);
            GameContext.PushThreadContext(game);
            try { DoMaintenance(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Maintenance);

            OnTurnPhaseChanged(game, TurnPhase.Production);
            GameContext.PushThreadContext(game);
            try { DoProduction(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Production);

            OnTurnPhaseChanged(game, TurnPhase.ShipProduction);
            GameContext.PushThreadContext(game);
            try { DoShipProduction(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.ShipProduction);

            OnTurnPhaseChanged(game, TurnPhase.Trade);
            GameContext.PushThreadContext(game);
            try { DoTrade(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Trade);

            OnTurnPhaseChanged(game, TurnPhase.Intelligence);
            GameContext.PushThreadContext(game);
            try { DoIntelligence(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Intelligence);

            OnTurnPhaseChanged(game, TurnPhase.Morale);
            GameContext.PushThreadContext(game);
            try { DoMorale(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.Morale);

            OnTurnPhaseChanged(game, TurnPhase.MapUpdates);
            GameContext.PushThreadContext(game);
            try { DoMapUpdates(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.MapUpdates);

            OnTurnPhaseChanged(game, TurnPhase.PostTurnOperations);
            GameContext.PushThreadContext(game);
            try { DoPostTurnOperations(game); }
            finally { GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(game, TurnPhase.PostTurnOperations);

            OnTurnPhaseChanged(game, TurnPhase.SendUpdates);

            GameContext.PushThreadContext(game);
            try
            {
                foreach (var scriptedEvent in game.ScriptedEvents)
                    scriptedEvent.OnTurnFinished(game);
            }
            finally { GameContext.PopThreadContext(); }

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

            ParallelForEach(objects, item =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    item.Reset();
                }
                catch (Exception e)
                {
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
                try
                {
                    civManager.SitRepEntries.Clear();
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
            var errors = new System.Collections.Concurrent.ConcurrentStack<Exception>();

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
            GameLog.Core.General.InfoFormat("Options: GalaxySize = {0} ({1} x {2})", GameContext.Current.Options.GalaxySize, GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);
            GameLog.Core.General.InfoFormat("Options: GalaxyShape = {0}", GameContext.Current.Options.GalaxyShape);
            GameLog.Core.General.InfoFormat("Options: StarDensity = {0}", GameContext.Current.Options.StarDensity);
            GameLog.Core.General.InfoFormat("Options: PlanetDensity = {0}", GameContext.Current.Options.PlanetDensity);
            GameLog.Core.General.InfoFormat("Options: StartingTechLevel = {0}", GameContext.Current.Options.StartingTechLevel);
            GameLog.Core.General.InfoFormat("Options: MinorRaceFrequency = {0}", GameContext.Current.Options.MinorRaceFrequency);
            GameLog.Core.General.InfoFormat("Options: FederationPlayable = {0}", GameContext.Current.Options.FederationPlayable);
            GameLog.Core.General.InfoFormat("Options: RomulanPlayable = {0}", GameContext.Current.Options.RomulanPlayable);
            GameLog.Core.General.InfoFormat("Options: KlingonPlayable = {0}", GameContext.Current.Options.KlingonPlayable);
            GameLog.Core.General.InfoFormat("Options: CardassianPlayable = {0}", GameContext.Current.Options.CardassianPlayable);
            GameLog.Core.General.InfoFormat("Options: DominionPlayable = {0}", GameContext.Current.Options.DominionPlayable);
            GameLog.Core.General.InfoFormat("Options: BorgPlayable = {0}", GameContext.Current.Options.BorgPlayable);
            GameLog.Core.General.InfoFormat("Options: TerranEmpirePlayable = {0}", GameContext.Current.Options.TerranEmpirePlayable);

            game.TurnNumber = 1;
        }
        #endregion

        #region DoFleetMovement() Method
        private void DoFleetMovement(GameContext game)
        {
            var allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();

            foreach (var fleet in allFleets)
            {
                if (fleet.Order == null)
                    continue;

                if (fleet.Order.IsComplete)
                {
                    var completedOrder = fleet.Order;
                    fleet.Order.OnOrderCompleted();
                    /* 
                     * It is possible that invoking OnCompletedOrder() on the completed
                     * order caused a new (or prior) order to be issued.  In this case,
                     * we let that order stand.  Otherwise, we revert to the default order.
                     */
                    if (fleet.Order == completedOrder)
                        fleet.SetOrder(fleet.GetDefaultOrder());
                }
                else if (!fleet.Order.IsValidOrder(fleet))
                {
                    var cancelledOrder = fleet.Order;
                    fleet.Order.OnOrderCancelled();
                    if (fleet.Order == cancelledOrder)
                        fleet.SetOrder(fleet.GetDefaultOrder());
                }
            }

            foreach (var fleet in allFleets)
            {
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

                    if (!fleet.Order.IsComplete)
                        continue;

                    var completedOrder = fleet.Order;

                    fleet.Order.OnOrderCompleted();

                    /* 
                     * It is possible that invoking OnCompletedOrder() caused a new (or prior)
                     * order to be issued.  In this case, we let that order stand.  Otherwise,
                     * we revert to the default order.
                     */
                    if (fleet.Order == completedOrder)
                        fleet.SetOrder(fleet.GetDefaultOrder());
                }

                if (fleet.IsStranded && !fleet.IsFleetInFuelRange())
                {
                    if (fleet.IsRouteLocked)
                        fleet.UnlockRoute();

                    fleet.SetRoute(TravelRoute.Empty);
                }

                if (fleet.Order.IsComplete)
                {
                    var completedOrder = fleet.Order;

                    fleet.Order.OnOrderCompleted();
                    
                    /* 
                     * It is possible that invoking OnCompletedOrder() caused a new (or prior)
                     * order to be issued.  In this case, we let that order stand.  Otherwise,
                     * we revert to the default order.
                     */
                    if (fleet.Order == completedOrder)
                        fleet.SetOrder(fleet.GetDefaultOrder());
                }
                else if (!fleet.Order.IsValidOrder(fleet))
                {
                    fleet.SetOrder(fleet.GetDefaultOrder());
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
                var diplomat = Diplomat.Get(civ1);

                foreach (var civ2 in GameContext.Current.Civilizations)
                {
                    if (civ1 == civ2)
                        continue;

                    var foreignPower = diplomat.GetForeignPower(civ2);

                    switch (foreignPower.PendingAction)
                    {
                        case PendingDiplomacyAction.AcceptProposal:
                            if (foreignPower.LastProposalReceived != null)
                                AcceptProposalVisitor.Visit(foreignPower.LastProposalReceived);
                            break;
                        case PendingDiplomacyAction.RejectProposal:
                            if (foreignPower.LastProposalReceived != null)
                                RejectProposalVisitor.Visit(foreignPower.LastProposalReceived);                            
                            break;
                    }

                    foreignPower.PendingAction = PendingDiplomacyAction.None;
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

        #region DoCombat() Method
        void DoCombat(GameContext game)
        {
            var combatLocations = new HashSet<MapLocation>();
            var invasionLocations = new HashSet<MapLocation>();
            var combats = new List<List<CombatAssets>>();
            var invasions = new List<InvasionArena>();

            foreach (var fleet in GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
            {
                if (!combatLocations.Contains(fleet.Location))
                {
                    var assets = CombatHelper.GetCombatAssets(fleet.Location);
                    if (assets.Count > 1)
                    {
                        combats.Add(assets);
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

                        if (colony.Name == "Companion" && colony.Owner.ShortName.ToString() == "Intro")
                        {
                            colony.Population.AdjustCurrent(-1 * colony.Population.CurrentValue);
                            colony.Population.UpdateAndReset();
                            colony.Reset();
                        }

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

                        /*
                         * TODO: This is disabled for now until I can figure out how to remove players/end a game.
                         * We need to find out whether the player has any more colonies, and switch government etc
                         * to that. Failing that, if they have a colony ship, let them continue
                        if (colony.Population.CurrentValue == 0)
                        {
                            civManager.SitRepEntries.Add(new PopulationDiedSitRepEntry(civ, colony));
                            colony.Destroy();
                            return;
                        }*/
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
                GameLog.Core.General.DebugFormat("{0} {1} is conducting research in {2}...",
                    scienceShip.ObjectID, scienceShip.Name, scienceShip.Sector);

                try
                {
                    var owner = GameContext.Current.CivilizationManagers[scienceShip.Owner];
                    var starType = scienceShip.Sector.System.StarType;

                    int researchGained = (int)(scienceShip.ShipDesign.ScanStrength * scienceShip.ShipDesign.ScienceAbility);
                    GameLog.Core.General.DebugFormat("Base research gained for {0} {1} is {2}",
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
                    }

                    GameContext.Current.CivilizationManagers[scienceShip.Owner].Research.UpdateResearch(researchGained);

                    GameLog.Core.General.DebugFormat("{0} {1} gained {2} research points for {3} by studying the {4} in {5}",
                        scienceShip.ObjectID, scienceShip.Name, researchGained, owner, starType, scienceShip.Sector);

                    GameContext.Current.CivilizationManagers[owner].SitRepEntries.Add(new ScienceShipResearchGainedSitRepEntry(owner.Civilization, scienceShip, researchGained, starType));
                }
                catch (Exception e)
                {
                    GameLog.Core.General.ErrorFormat(string.Format("There was a problem conducting research for {0} {1}",
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
                    civManager.Credits.AdjustCurrent(colonies.Sum(c => c.TaxCredits));
                    civManager.Resources.Deuterium.AdjustCurrent(colonies.Sum(c => c.NetDeuterium));
                    civManager.Resources.Dilithium.AdjustCurrent(colonies.Sum(c => c.NetDilithium));
                    civManager.Resources.RawMaterials.AdjustCurrent(colonies.Sum(c => c.NetRawMaterials));

                    //Get the resources available for the civilization
                    ResourceValueCollection totalResourcesAvailable = new ResourceValueCollection();
                    totalResourcesAvailable[ResourceType.Deuterium] = civManager.Resources.Deuterium.CurrentValue;
                    totalResourcesAvailable[ResourceType.Dilithium] = civManager.Resources.Dilithium.CurrentValue;
                    totalResourcesAvailable[ResourceType.RawMaterials] = civManager.Resources.RawMaterials.CurrentValue;

                    /* 
                     * Shuffle the colonies so they are processed in random order.  This
                     * will help prevent the same colonies from getting priority when
                     * the global stockpiles are low.
                     */
                    colonies.RandomizeInPlace();

                    /* Iterate through each colony */
                    foreach (Colony colony in colonies)
                    {
                        GameLog.Core.Production.DebugFormat("DoProduction for Colony {0} ({1} with credits = {2})", colony.Name, civ.Name, civManager.Credits);

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
                                continue;
                            }

                            if (colony.BuildSlots[0].Project.IsPaused) { }
                                //Not sure how to handle this

                            GameLog.Core.Production.DebugFormat("Resources available for {0} before construction of {1} on {2}: Deuterium={3}, Dilithium={4}, RawMaterials={5}",
                                civ.Name,
                                colony.BuildSlots[0].Project.BuildDesign.Name,
                                colony.Name,
                                civManager.Resources.Deuterium.CurrentValue,
                                civManager.Resources.Dilithium.CurrentValue,
                                civManager.Resources.RawMaterials.CurrentValue);

                            //Try to finish the projects
                            if (colony.BuildSlots[0].Project.IsRushed)
                            {
                                // Rushing a project should have no impact on the industry of colony (since it's all been paid for)
                                int tmpIndustry = colony.BuildSlots[0].Project.GetCurrentIndustryCost();
                                ResourceValueCollection tmpResources = new ResourceValueCollection();
                                tmpResources[ResourceType.Deuterium] = 999999;
                                tmpResources[ResourceType.Dilithium] = 999999;
                                tmpResources[ResourceType.RawMaterials] = 999999;
                                civManager.Credits.AdjustCurrent(-tmpIndustry);
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
                    GameLog.Core.ShipProduction.DebugFormat(string.Format("DoShipProduction failed for {0}", civ.Name), e);
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

        #region DoIntelligence() Method
        void DoIntelligence(GameContext game)
        {
            ParallelForEach(GameContext.Current.Civilizations, civ =>
            {
                GameContext.PushThreadContext(game);
                try
                {
                    if (!civ.IsEmpire)
                        return;

                    var attackingEmpire = GameContext.Current.CivilizationManagers[civ.CivID];

                    int attackIntelligence = 201;   // avoid bug by deviding by zero  - later it's shrink by 200

                    //Get a list of all viable target empire
                    var targets = GameContext.Current.Civilizations
                        //All empires
                        .Where(t => t.IsEmpire)
                        //That aren't themselves
                        .Where(t => t.CivID != civ.CivID)
                        //That they have actually met
                        .Where(t => DiplomacyHelper.IsContactMade(civ, t));

                    GameLog.Core.Intel.DebugFormat("empires without the own one & contact made: Available targets = {0}", targets.Count());

                    //Double check that we have viable targets
                    if (targets.Count() == 0)
                        return;

                    //Select one at random
                    CivilizationManager targetEmpire = GameContext.Current.CivilizationManagers[targets.RandomElement()];
                    GameLog.Core.Intel.DebugFormat("targetEmpire = {0}", targetEmpire.Civilization.Name);

                    //Randomly pick one of their colonies to attack
                    Colony targetColony = targetEmpire.Colonies.RandomElement();

                    GameLog.Core.Intel.DebugFormat("targetColony = {0}", targetColony.Name);

                    //200 intelligience is not taken in to account for attacks
                    if (attackingEmpire.TotalIntelligence > 200)
                    {
                        attackIntelligence = attackingEmpire.TotalIntelligence - 200;
                    }

                    int defenseIntelligence = targetEmpire.TotalIntelligence;
                    if (defenseIntelligence < 1)   // avoid bug by devided by zero
                        defenseIntelligence = 1;

                    //Get the ratio of the attacking power to defending power
                    int ratio = attackIntelligence / defenseIntelligence;

                    //We need at least a ratio of greater than 1 to attack
                    if (ratio < 1)
                    {
                        GameLog.Core.Intel.DebugFormat("{0} doesn't have enough attacking intelligence to make an attack against {1} - {2} vs {3}",
                            attackingEmpire.Civilization.Name, targetEmpire.Civilization.Name, attackIntelligence, defenseIntelligence);
                        return;
                    }

                    //For each 1 ratio, the attacking empire has a chance of performing an action, or failing, to a maximum of 4
                    int attempts;
                    if (ratio > 4)
                    {
                        attempts = 4;
                    }
                    else
                    {
                        attempts = ratio;
                    }

                    for (int i = 0; i < attempts; i++)
                    {
                        int action = RandomHelper.Roll(30);

                        if (action < 9)
                        {
                            /*
                             * Adjust morale
                             */
                            //-2 morale at target colony
                            targetColony.Morale.AdjustCurrent(-2);
                            targetColony.Morale.UpdateAndReset();
                            GameLog.Core.Intel.DebugFormat("Morale at {0} reduced by 2 to {1}", targetColony.Name, targetColony.Morale.CurrentValue);

                            //-1 morale at target home colony
                            targetEmpire.HomeColony.Morale.AdjustCurrent(-1);
                            targetEmpire.HomeColony.Morale.UpdateAndReset();
                            GameLog.Core.Intel.DebugFormat("Morale on {0} reduced by 1 to {1}", targetEmpire.HomeColony.Name, targetEmpire.HomeColony.Morale.CurrentValue);

                            //-1 morale at target seat of government
                            targetEmpire.SeatOfGovernment.Morale.AdjustCurrent(-1);
                            targetEmpire.SeatOfGovernment.Morale.UpdateAndReset();
                            GameLog.Core.Intel.DebugFormat("Morale on {0} reduced by 1 to {1}", targetEmpire.SeatOfGovernment.Name, targetEmpire.SeatOfGovernment.Morale.CurrentValue);

                            //Morale +1 to attacker HomeColony
                            attackingEmpire.HomeColony.Morale.AdjustCurrent(+1);
                            attackingEmpire.HomeColony.Morale.UpdateAndReset();
                            GameLog.Core.Intel.DebugFormat("Morale on {0} increased by 1 to {1}", attackingEmpire.HomeColony.Name, attackingEmpire.HomeColony.Morale.CurrentValue);
                        }

                        //Steal Money
                        if (action == 1)
                        {
                            //If we're going for their main planet, target the central treasury
                            if ((targetColony == targetEmpire.HomeColony) || (targetColony == targetEmpire.SeatOfGovernment))
                            {
                                if (targetEmpire.Credits.CurrentValue > 0)
                                {
                                    int stolenCredits = RandomHelper.Roll(targetEmpire.Credits.CurrentValue);

                                    targetEmpire.Credits.AdjustCurrent(-1 * stolenCredits);
                                    targetEmpire.Credits.UpdateAndReset();
                                    attackingEmpire.Credits.AdjustCurrent(stolenCredits);
                                    attackingEmpire.Credits.UpdateAndReset();

                                    GameLog.Core.Intel.DebugFormat("{0} stole {1} credits from the {2} treasury", attackingEmpire.Civilization.Name, stolenCredits, targetEmpire.Civilization.Name);

                                    targetEmpire.SitRepEntries.Add(new CreditsStolenTargetSitRepEntry(targetEmpire.Civilization, targetColony, stolenCredits));
                                    attackingEmpire.SitRepEntries.Add(new CreditsStolenAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, stolenCredits));
                                }
                            }
                            else //Otherwise siphon credits from trade route
                            {
                                if ((targetColony.CreditsFromTrade.CurrentValue > 0))
                                {
                                    int stolenCredits = RandomHelper.Roll(targetColony.CreditsFromTrade.CurrentValue);
                                    targetColony.CreditsFromTrade.AdjustCurrent(stolenCredits * -1);
                                    targetColony.CreditsFromTrade.UpdateAndReset();
                                    attackingEmpire.Credits.AdjustCurrent(stolenCredits);
                                    attackingEmpire.Credits.UpdateAndReset();

                                    GameLog.Core.Intel.DebugFormat("{0} stole {1} credits from the trade routes on {2}", attackingEmpire.Civilization.Name, stolenCredits, targetColony.Name);

                                    targetEmpire.SitRepEntries.Add(new TradeRouteCreditsStolenTargetSitRepEntry(targetEmpire.Civilization, targetColony, stolenCredits));
                                    attackingEmpire.SitRepEntries.Add(new TradeRouteCreditsStolenAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, stolenCredits));
                                }
                            }
                        }

                        //Target their food reserves
                        else if (action == 2)
                        {
                            if (targetColony.FoodReserves.CurrentValue > 0)
                            {
                                int destroyedFoodReserves = RandomHelper.Roll(targetColony.FoodReserves.CurrentValue);

                                targetColony.FoodReserves.AdjustCurrent(destroyedFoodReserves * -1);
                                targetColony.FoodReserves.UpdateAndReset();

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} food at {2}", attackingEmpire.Civilization.Name, destroyedFoodReserves, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new FoodReservesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, destroyedFoodReserves));
                                attackingEmpire.SitRepEntries.Add(new FoodReservesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, destroyedFoodReserves));
                            }
                        }

                        //Target their food production
                        else if (action == 3)
                        {
                            if (targetColony.GetTotalFacilities(ProductionCategory.Food) > 0)
                            {
                                int destroyedFoodFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Food));
                                targetColony.RemoveFacilities(ProductionCategory.Food, destroyedFoodFacilities);

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} food faciliities at {2}", attackingEmpire.Civilization.Name, destroyedFoodFacilities, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Food, destroyedFoodFacilities));
                                attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Food, destroyedFoodFacilities));
                            }
                        }

                        //Target industrial production
                        else if (action == 4)
                        {
                            if (targetColony.GetTotalFacilities(ProductionCategory.Industry) > 0)
                            {
                                int destroyedIndustrialFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Industry));
                                targetColony.RemoveFacilities(ProductionCategory.Industry, destroyedIndustrialFacilities);

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} industrial faciliities at {2}", attackingEmpire.Civilization.Name, destroyedIndustrialFacilities, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Industry, destroyedIndustrialFacilities));
                                attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Industry, destroyedIndustrialFacilities));
                            }
                        }

                        //Target energy production
                        else if (action == 5)
                        {
                            if (targetColony.GetTotalFacilities(ProductionCategory.Energy) > 0)
                            {
                                int destroyedEnergyFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Energy));
                                targetColony.RemoveFacilities(ProductionCategory.Energy, destroyedEnergyFacilities);

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} energy faciliities at {2}", attackingEmpire.Civilization.Name, destroyedEnergyFacilities, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Energy, destroyedEnergyFacilities));
                                attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Energy, destroyedEnergyFacilities));
                            }
                        }

                        //Target research facilities
                        else if (action == 6)
                        {
                            if (targetColony.GetTotalFacilities(ProductionCategory.Research) > 0)
                            {
                                int destroyedResearchFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Research));
                                targetColony.RemoveFacilities(ProductionCategory.Research, destroyedResearchFacilities);

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} research facilities at {2}", attackingEmpire.Civilization.Name, destroyedResearchFacilities, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Research, destroyedResearchFacilities));
                                attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Research, destroyedResearchFacilities));
                            }
                        }

                        //Target intel facilities
                        else if (action == 7)
                        {
                            if (targetColony.GetTotalFacilities(ProductionCategory.Intelligence) > 0)
                            {
                                int destroyedIntelFacilities = RandomHelper.Roll(targetColony.GetTotalFacilities(ProductionCategory.Intelligence));
                                targetColony.RemoveFacilities(ProductionCategory.Intelligence, destroyedIntelFacilities);

                                GameLog.Core.Intel.DebugFormat("{0} destroyed {1} intelligence facilities at {2}", attackingEmpire.Civilization.Name, destroyedIntelFacilities, targetColony.Name);

                                targetEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedTargetSitRepEntry(targetEmpire.Civilization, targetColony, ProductionCategory.Intelligence, destroyedIntelFacilities));
                                attackingEmpire.SitRepEntries.Add(new ProductionFacilitiesDestroyedAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, ProductionCategory.Intelligence, destroyedIntelFacilities));
                            }
                        }

                        //Target planetary defenses
                        else if (action == 8)
                        {
                            int destroyedOrbitalBatteries = 0;
                            int shieldStrengthLost = 0;

                            if (targetColony.OrbitalBatteries.Count > 1)
                            {
                                destroyedOrbitalBatteries = RandomHelper.Roll(targetColony.OrbitalBatteries.Count);
                                targetColony.RemoveOrbitalBatteries(destroyedOrbitalBatteries);
                            }

                            if (targetColony.ShieldStrength.CurrentValue > 0) {
                                shieldStrengthLost = RandomHelper.Roll(targetColony.ShieldStrength.CurrentValue);
                                targetColony.ShieldStrength.AdjustCurrent(-1 * shieldStrengthLost);
                                targetColony.ShieldStrength.UpdateAndReset();
                            }

                            GameLog.Core.Intel.DebugFormat("{0} destroyed {1} orbital batteries and removed {2} strength from planetary shields at {3}",
                                attackingEmpire.Civilization.Name, destroyedOrbitalBatteries, shieldStrengthLost, targetColony.Name);

                            targetEmpire.SitRepEntries.Add(new PlanetaryDefenceAttackTargetSitRepEntry(targetEmpire.Civilization, targetColony, destroyedOrbitalBatteries, shieldStrengthLost));
                            attackingEmpire.SitRepEntries.Add(new PlanetaryDefenceAttackAttackerSitRepEntry(attackingEmpire.Civilization, targetColony, destroyedOrbitalBatteries, shieldStrengthLost));
                        }

                        //Other possibilties...
                        //Destroy orbiting space station
                        //Assasination
                        //Target individual buildings
                        //Bombing
                        //Target research (destroy)
                        //Target research (steal)

                        //Attack failed
                        //else
                        //{
                        //    targetEmpire.SitRepEntries.Add(new IntelDefenceSucceededSitRepEntry(targetEmpire.Civilization, targetColony));
                        //    attackingEmpire.SitRepEntries.Add(new IntelAttackFailedSitRepEntry(attackingEmpire.Civilization, targetColony));
                        //}
                    }
                }
                catch (Exception e)
                {
                    GameLog.Core.Intel.ErrorFormat(string.Format("DoIntelligience failed for {0}", civ.Name), e);
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            });
        }
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
                                route.Credits = (int)((sourceMod * route.SourceColony.Population.CurrentValue) +
                                                    (targetMod * route.TargetColony.Population.CurrentValue));
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
                            colony.CreditsFromTrade.AdjustCurrent(route.Credits);

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
                 * is properly reflected.  Do the same for the personnel pool
                 * and the credit treasury.
                 */
                civManager.Resources.UpdateAndReset();
                civManager.Credits.UpdateAndReset();
                civManager.OnTurnFinished();
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
