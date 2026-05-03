// File:GameEngine.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using C5;
using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Data;
using Supremacy.Diplomacy;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace Supremacy.Game
{
    /// <summary>
    /// The turnnumber processing engine used in the _game.
    /// </summary>
    public partial class GameEngine 
    {
        public static bool IsPlayer_AIControllend() // string = orientated left
        {
            //return true;
            return false;
        }

        //public int _tn = 0;  // internal use // Turn Number

        public readonly List<CivValue> CivValueList = new List<CivValue>();
        public readonly List<CivRank> CivRankList = new List<CivRank>();

        //public int AAASpecialWidth1;  // resize images in Game to a player's setting outside
        //public int AAASpecialHeight1;



        #region Public Members

        /// <summary>
        /// Occurs when the current turnnumber phase has changed.
        /// </summary>
        public event TurnPhaseEventHandler TurnPhaseChanged;

        /// <summary>
        /// Occurs when the current turnnumber phase has finished.
        /// </summary>
        public event TurnPhaseEventHandler TurnPhaseFinished;

        /// <summary>
        /// Occurs when _combat is starting.
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
        /// Blocks the execution of the turnnumber processing engine while waiting on players
        /// to submit _combat orders.
        /// </summary>
        private readonly ManualResetEvent CombatReset = new ManualResetEvent(false);
        #endregion

        #region OnTurnPhaseChanged() Method
        /// <summary>
        /// Raises the <see cref="TurnPhaseChanged"/> event.
        /// </summary>
        /// <param name="game">The current _game.</param>
        /// <param name="_phase">The current turnnumber _phase.</param>
        private void OnTurnPhaseChanged(GameContext _game, TurnPhase _phase)
        {
            if (_phase != TurnPhase.SendUpdates)
            {
                foreach (Scripting.ScriptedEvent scriptedEvent in _game.ScriptedEvents)
                {
                    //if (GameContext.Current.TurnNumber >= 1)
                    scriptedEvent.OnTurnPhaseStarted(_game, _phase);
                }
            }

            TurnPhaseChanged?.Invoke(_phase);
        }
        #endregion

        #region OnTurnPhaseFinished() Method
        /// <summary>
        /// Raises the <see cref="TurnPhaseChanged"/> event.
        /// </summary>
        /// /// <param name="_game">The current _game.</param>
        /// <param name="_phase">The turnnumber _phase that just finished.</param>
        private void OnTurnPhaseFinished(GameContext _game, TurnPhase _phase)
        {
            if (_phase != TurnPhase.SendUpdates)
            {
                foreach (Scripting.ScriptedEvent scriptedEvent in _game.ScriptedEvents)
                {
                    if (GameContext.Current.TurnNumber > 2)
                    {
                        scriptedEvent.OnTurnPhaseFinished(_game, _phase);
                    }
                }
            }

            TurnPhaseFinished?.Invoke(_phase);
        }
        #endregion

        #region OnFleetLocationChanged() Method
        /// <summary>
        /// Raises the <see cref="FleetLocationChanged"/> event.
        /// </summary>
        /// <param name="fleet">A Fleet whose Location just changed.</param>
        private void OnFleetLocationChanged(Fleet fleet)
        {
            FleetLocationChanged?.Invoke(this, new ParameterEventArgs<Fleet>(fleet));
        }
        #endregion

        #region DoTurn() Method
        /// <summary>
        /// Perform turnnumber processing for the specified _game context.
        /// </summary>
        /// <param name="_game">The _game context.</param>
        public void DoTurn([NotNull] GameContext _game)
        {
            string _text;
            bool _gamelog_bool = false;
            bool _writeDirectly = true;

            if (_game == null)
            {
                _text = "Game might have broken due to long lack of activity";

                _text = "There is NOT a _game anymore - maybe due to a long lack of Activity"
                        + Environment.NewLine + Environment.NewLine + "*** Please restart the _game !"
                        //+ _newline + _newline + "*** or rename the fake file 'XNA31_ok_OFF.info' to 'XNA31_ok.info'"
                        //+ _newline + _newline + "For Coders: Make sure you have fill the \\Resources folder"
                        ;
                _ = MessageBox.Show(_text, "WARNING", MessageBoxButton.OK);
                throw new ArgumentNullException("_game");
            }

            _text = "Step_0705:; " + DateTime.Now + " ...Do_C_Turn_UnitAI in GameEngine.cs  ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            HashSet<Fleet> _fleets;

            GameContext.PushThreadContext(_game);

            try { Do_10_ScriptedEvents(_game); }
            finally { _ = GameContext.PopThreadContext(); }

            //try  // Scripted Events ...
            //{
            //    _text = "Step_0710:; ...Scripted Events > beginning from Turn x on ...";
            //    if (_writeDirectly) Console.WriteLine(_text);
            //    if (_gamelog_bool)
            //        GameLog.Core.GeneralDetails.DebugFormat(_text);

            //    List<Scripting.ScriptedEvent> eventsToRemove = _game.ScriptedEvents.Where(o => !o.CanExecute).ToList();
            //    foreach (Scripting.ScriptedEvent eventToRemove in eventsToRemove)
            //    {
            //        _ = _game.ScriptedEvents.Remove(eventToRemove);
            //    }

            //    //Update If we've reached turnnumber x, start running scripted events
            //    if (GameContext.Current.TurnNumber >= 1) // Scripted Events ... from Turn x on
            //    {
            //        foreach (Scripting.ScriptedEvent scriptedEvent in _game.ScriptedEvents)
            //        {
            //            scriptedEvent.OnTurnStarted(_game);
            //        }
            //    }

            //    _fleets = _game.Universe.Find<Fleet>();

            //    foreach (Fleet _fleet in _fleets)
            //    {
            //        _fleet.LocationChanged += HandleFleetLocationChanged;
            //    }
            //}
            //finally { _ = GameContext.PopThreadContext(); }


            _text = "Step_0715:; " + DateTime.Now + " > next > Do_11_PreTurnOperations...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.PreTurnOperations);
            GameContext.PushThreadContext(_game);

            try { Do_11_PreTurnOperations(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.PreTurnOperations);


            //_text = "Step_0720: ...beginning SpyOperations...";
            //if (_writeDirectly) Console.WriteLine(_text);
            //GameLog.Core.GeneralDetails.DebugFormat(_text);

            //OnTurnPhaseChanged(_game, TurnPhase.SpyOperations);
            //GameContext.PushThreadContext(_game);
            //try { DoSpyOperations(); } //?? do we need _game in the constructor ??
            //finally { GameContext.PopThreadContext(); }
            //OnTurnPhaseFinished(_game, TurnPhase.SpyOperations);


            _text = "Step_0725:; " + DateTime.Now + " > next > Do_12_Fleet_Handling...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.FleetMovement);
            GameContext.PushThreadContext(_game);

            try { Do_12_FleetHandling(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.FleetMovement);


            // DoSabotage is part of DiplomatAI - we just Sabotage non-Allies

            //_text = "Step_0728:; next > Sabotage...";
            //if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            //try { DoSabotage(_game); }
            //finally { _ = GameContext.PopThreadContext(); }
            //OnTurnPhaseFinished(_game, TurnPhase.Sabotage);


            _text = "Step_0730:; " + DateTime.Now + " > next > Do_13_Diplomacy...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Diplomacy);
            GameContext.PushThreadContext(_game);
            try { Do_13_Diplomacy(); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Diplomacy);


            _text = "Step_0735:; " + DateTime.Now + " > next >  Do_15_Combat...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Combat);
            GameContext.PushThreadContext(_game);
            try { Do_15_Combat(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Combat);


            _text = "Step_0740:; " + DateTime.Now + " > next > Do_16_PopulationGrowth ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.PopulationGrowth);
            GameContext.PushThreadContext(_game);
            try { Do_16_Population(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.PopulationGrowth);


            _text = "Step_0745:; " + DateTime.Now + " > next > Do_17_Research ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Research);
            GameContext.PushThreadContext(_game);
            try { Do_17_Research(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Research);


            _text = "Step_0750:; " + DateTime.Now + " > next > Do_18_Scrapping ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Scrapping);
            GameContext.PushThreadContext(_game);
            try { Do_18_Scrapping(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Scrapping);


            _text = "Step_0755:; " + DateTime.Now + " > next > Do_19_Maintenance ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Maintenance);
            GameContext.PushThreadContext(_game);
            try { Do_19_Maintenance(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Maintenance);


            _text = "Step_0760:; " + DateTime.Now + " > next > Do_20_ShipProduction ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.ShipProduction);
            GameContext.PushThreadContext(_game);
            try { Do_20_ShipProduction(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.ShipProduction);


            // test 2022-07-17 Production after ShipProduction
            _text = "Step_0765:; " + DateTime.Now + " > next > Do_21_Production ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Production);
            GameContext.PushThreadContext(_game);
            try { Do_21_Production(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Production);


            _text = "Step_0770:; " + DateTime.Now + " > next > Do_22_Trade ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Trade);
            GameContext.PushThreadContext(_game);
            try { Do_22_Trade(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Trade);

            //GameLog.Core.GeneralDetails.DebugFormat("...beginning Intelligence...");

            //OnTurnPhaseChanged(_game, TurnPhase.Intelligence);
            //GameContext.PushThreadContext(_game);
            //try { DoIntelligence(_game); }
            //finally { GameContext.PopThreadContext(); }
            //OnTurnPhaseFinished(_game, TurnPhase.Intelligence);


            _text = "Step_0775:; " + DateTime.Now + " > next > Do_23_Morale ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.Morale);
            GameContext.PushThreadContext(_game);
            try { Do_23_Morale(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.Morale);


            _text = "Step_0780:; " + DateTime.Now + " > next > Do_24_MapUpdates ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.MapUpdates);
            GameContext.PushThreadContext(_game);
            try { Do_24_MapUpdates(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.MapUpdates);


            _text = "Step_0785:; " + DateTime.Now + " > next > Do_25_PostTurnOperations ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            OnTurnPhaseChanged(_game, TurnPhase.PostTurnOperations);
            GameContext.PushThreadContext(_game);
            try { Do_25_PostTurnOperations(_game); }
            finally { _ = GameContext.PopThreadContext(); }
            OnTurnPhaseFinished(_game, TurnPhase.PostTurnOperations);


            _text = "Step_0790:; " + DateTime.Now + " > next > SendUpdates ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);
            OnTurnPhaseChanged(_game, TurnPhase.SendUpdates);


            _text = "Step_0795:; " + DateTime.Now + " > next > PushThreadContext ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);
            GameContext.PushThreadContext(_game);
            try
            {
                foreach (Scripting.ScriptedEvent scriptedEvent in _game.ScriptedEvents)
                {
                    //if (GameContext.Current.TurnNumber >= 50)
                    scriptedEvent.OnTurnFinished(_game);
                }

            }
            finally { _ = GameContext.PopThreadContext(); }



            _text = "Step_0797:; " + DateTime.Now + " > next > HandleFleetLocationChanged ...";
            if (_writeDirectly) Console.WriteLine(_text);
            //if (_gamelog_bool)
            //    GameLog.Core.GeneralDetails.DebugFormat(_text);

            _fleets = _game.Universe.Find<Fleet>();
            foreach (Fleet fleet in _fleets)
            {
                fleet.LocationChanged -= HandleFleetLocationChanged;
            }
        }

        #endregion DoTurn

        #region DoScriptedEvents() Method

        private void Do_10_ScriptedEvents(GameContext _game)
        {
            bool _writeDirectly = true;

            string _text = "Step_0710:; ...Scripted Events > beginning from Turn x on ...";
            if (_writeDirectly) Console.WriteLine(_text);
            bool _gamelog_bool = false;
            if (_gamelog_bool)
                GameLog.Core.GeneralDetails.DebugFormat(_text);

            List<Scripting.ScriptedEvent> eventsToRemove = _game.ScriptedEvents.Where(o => !o.CanExecute).ToList();
            foreach (Scripting.ScriptedEvent eventToRemove in eventsToRemove)
            {
                _ = _game.ScriptedEvents.Remove(eventToRemove);
            }

            //Update If we've reached turnnumber x, start running scripted events
            if (GameContext.Current.TurnNumber >= 1) // Scripted Events ... from Turn x on
            {
                foreach (Scripting.ScriptedEvent scriptedEvent in _game.ScriptedEvents)
                {
                    scriptedEvent.OnTurnStarted(_game);
                }
            }

            HashSet<Fleet> _fleets = _game.Universe.Find<Fleet>();

            //_fleets = _game.Universe.Find<Fleet>();

            foreach (Fleet fleet in _fleets)
            {
                fleet.LocationChanged += HandleFleetLocationChanged;
            }
        }
        #endregion DoScriptedEvents() Method



        #region HandleFleetLocationChanged() Method
        private void HandleFleetLocationChanged(object sender, EventArgs e)
        {
            Fleet fleet = sender as Fleet;
            if (fleet != null)
            {
                OnFleetLocationChanged(fleet);
            }
        }
        #endregion
        private static List<T> Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[j];
                list[j] = list[i];
                list[i] = tmp;
            }
            return list;
        }
        //#region DoPreTurnOperations() Method
        private void Do_11_PreTurnOperations(GameContext _game)
        {
            // these here > foreach....
            HashSet<CivilizationManager> _civManagers = GameContext.Current.CivilizationManagers.ToHashSet();
            //HashSet<Fleet> _fleets = _objects.OfType<Fleet>().ToHashSet();

            string _text;
            string _pre_turn_text = "";
            string _newline = Environment.NewLine;


            Reset_Items(_game);

            foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers)
            {
                _civM.SitRepEntries.Clear();
            }
            //{
            //    //GameContext.PushThreadContext(_game);

            //}

            ConcurrentStack<Exception> errors = new ConcurrentStack<Exception>();

            CivilizationKeyedMap<Diplomat> diplomatCheck = _game.Diplomats;

            Console.WriteLine("Step_1152:; ...Do_11_PreTurnOperations");

            HashSet<Orbital> _all_orbitals = _game.Universe.Find<Orbital>();
            foreach (Orbital item in _all_orbitals)
            {
                item.Fire_Power_Orbital = OrbitalHelper.Fire_power_calculated(item);
                _text = "Step_1051:; _fleet.Order?.OnTurnBeginning() for= "
                    + UnitAI.CreateShipText(item as Ship, out string _fleet_text)
                //+ " ( station? )"
                //+ " > _fleet.Order?.OnTurnBeginning()"
                //+ " - Name= " + _civM.Civilization
                //+ _newline
                ;
                _pre_turn_text += _newline + _text;
                //Console.WriteLine(_text);

                //                _item.Order?.OnTurnBeginning();
                //SitReps_for_Fleets(_fleet);
            }

            //Console.WriteLine("Step_1154:; resetting items...");
            ////_ = ParallelForEach(objects, _item =>
            //foreach (var item in objects)
            //{
            //    //GameContext.PushThreadContext(_game);
            //    // GameLog.Core.General.DebugFormat("next _item will be: ID = {0}, Name = {1}", _item.ObjectID, _item.Name);
            //    try
            //    {
            //        // GameLog.Core.General.DebugFormat("_item: ID = {0}, Name = {1} is trying to reset", _item.ObjectID, _item.Name);
            //        item.Reset();
            //        // works well but gives hidden info
            //        // GameLog.Core.General.DebugFormat("_item: ID = {0}, Name = {1} is successfully resetted", _item.ObjectID, _item.Name);
            //    }
            //    catch (Exception e)
            //    {
            //        _text = "Step_1156:; ### Object = _item.Reset() at Do_11_PreTurnOperations > crashed for ID= " + item.ObjectID
            //            + " - Name= " + item.Name
            //            ;
            //        Console.WriteLine(_text);
            //        GameLog.Core.General.ErrorFormat(_text);
            //        Debugger.Break();
            //        _errors.Push(e);
            //    }
            //    finally
            //    {

            //        GameContext.PushThreadContext(_game);
            //    }
            //}
            //// jump over and break here
            ////Debugger.Break();
            ////;
            ////});


            //if (!_errors.IsEmpty)
            //{
            //    foreach (var err in _errors)
            //    {
            //        _text = "Step_1157:; ERROR > " + err.Message + err.StackTrace;
            //        Console.WriteLine(_text);
            //        GameLog.Core.General.DebugFormat(_text);

            //        Debugger.Break();
            //    }

            //    //throw new AggregateException(_errors);   // avoid crashes = stopps for player as much as possible !
            //}

            //_errors.Clear();

            // PreTurn CivManagers
            foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers)
            {
                //lock (_civM)
                //{


                bool _bool_is_human = _civM.Civilization.IsHuman;

                if (_civM.Civilization.Key == "ZALKONIANS")
                {
                    //Debugger.Break();
                }
                //_text = "Step_1151:; Pre_Turn For= " + _civM.Civilization
                //    //+ " - Name= " + _civM.Civilization
                //    + _newline
                //    ;
                //Console.WriteLine(_text);
                //GameContext.PushThreadContext(_game);
                //_civM.SitRepEntries.Clear();



                Civ_Fire_Power_Space(_civM);

                //_text = "Step_1151:;"
                //    //+ " > Pre_Turn For= * " + _civM.Civilization
                //    + " * > Fire_Power_Space= " + _civM.FirePowerSpace
                //;

                //Console.WriteLine(_text);

                if (_bool_is_human == true)
                {
                    _text = "Debugger.Break();";
                    //Debugger.Break();
                }
                // _civM.FirePowerSpace
                //List<Ship> _combatant_ships = GameContext.Current.Universe.FindOwned<Ship>(_civM.Civilization)
                //    .Where(s => s.IsCombatant).ToList();
                //int _int = 0;
                //foreach (Ship ship in _combatant_ships)
                //{
                //    _int += ship.Fire_Power_Ship;
                //    //
                //    //Debugger.Break();
                //}
                //_civM.FirePowerSpace = _int;

                // _civM.Locations_To_Explore


                //List<Fleet> _ownFleets = new List<Fleet>();
                //try
                //{
                //List<Fleet> _ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization)
                //    .Where(f => f.CanMove /*&& !f.Route.IsEmpty*/).ToList();
                //}
                //catch (Exception e)
                //{
                //    //if (_fleet != null)
                //    //{
                //    //    _text = "Step_2211:; "
                //    //        + "_fleet.ObjectId= " + _fleet.ObjectID
                //    //        + "" + _fleet.Name
                //    //        + "" + _fleet.ClassName
                //    //        + " > error=" + e
                //    //        ;
                //    //    Console.WriteLine(_text);
                //    Debugger.Break();
                //    Console.WriteLine(e);
                //    //GameLog.Client.General.ErrorFormat("_fleet.ObjectId ={0} {1} {2} error ={3}", _fleet.ObjectID, _fleet.Name, _fleet.ClassName, e);
                //}




                //    List<MapLocation> _possible_locations_to_explore = new List<MapLocation>(); // locations need less storage
                //    List<Sector> _possible_sectors_to_explore = new List<Sector>();
                //    int _radius = 2;
                //Expand_Radius:;

                Find_Locations_To_Explore(_civM);

                //Find_Locations_To_NOT_Build_Stations(_civM);

                Find_Locations_To_Build_Stations(_civM);

                //_text = "Step_1152:;"
                _text = "Step_1154:;"
                    + " Locations_To_Explore.Count= " + GameEngine.Do_x_Digit_String(2, _civM.Locations_To_Explore.Count.ToString())
                    + ", Locations_To_Build_Stations.Count= " + GameEngine.Do_x_Digit_String(2, _civM.Locations_To_Build_Stations.Count.ToString())
                    + ", Locations_To_NOT_Build_Stations.Count= " + GameEngine.Do_x_Digit_String(2, _civM.Locations_To_NOT_Build_Stations.Count.ToString())
                    + " for= " + _civM.Civilization + " > Fire_Power_Space=" + _civM.FirePowerSpace;
                //Console.WriteLine(_text);
                _pre_turn_text += _newline + _text;

                if (_bool_is_human == true)
                {
                    //Debugger.Break();
                }
                //}
                //}
                //    List<MapLocation> _prior_objects_to_build_stations = new List<MapLocation>();
                //    List<MapLocation> _possible_locations_to_NOT_build_stations = new List<MapLocation>();
                //    List<Sector> _check_sectors = new List<Sector>();

                //    _civM.Locations_To_Build_Stations = null;
                //    _radius = 2;

                //    if (_civM.Locations_To_NOT_Build_Stations != null)
                //    {
                //        _possible_locations_to_NOT_build_stations = _civM.Locations_To_NOT_Build_Stations;
                //        Report_Locations_To_NOT_Build_Stations(_civM);  // list out of _civM
                //    }

                //    List<Fleet> _ownConstructorFleets = new List<Fleet>();
                //    _ownConstructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization).Where(f => f.IsConstructor).ToList();

                //    if (_ownConstructorFleets.Count < 1)
                //    {
                //        goto No_construction_ships_at_all;
                //    }

                //Expand_Radius_For_Build_Stations:;

                //    // find new solution

                //    _check_sectors = MapHelper.GetSectorsWithinRadius(_civM.HomeSystem.Sector, _radius).ToList();
                //    foreach (var _sector in _check_sectors)
                //    {
                //        Report_Sector(_sector);

                //        //bool _bool_is_human = _civM.Civilization.IsHuman;
                //        if (_bool_is_human == true)
                //        {
                //            //Debugger.Break();
                //        }

                //        if (_civM.Locations_To_NOT_Build_Stations != null
                //        && _civM.Locations_To_NOT_Build_Stations.Contains(_sector.Location))
                //        {
                //            continue;
                //        }

                //        if (DiplomacyHelper.IsTravelAllowed(_civM.Civilization, _sector) == false)
                //        {
                //            continue;
                //        }

                //        //bool _bool_is_human = _civM.Civilization.IsHuman;
                //        if (_bool_is_human == true)
                //        {
                //            Debugger.Break();
                //        }

                //        if (_sector.System != null
                //            && _sector.System.StarType == StarType.BlackHole
                //            && _sector.System.StarType == StarType.XRayPulsar
                //            && _sector.System.StarType == StarType.NeutronStar
                //            && _sector.System.StarType == StarType.Quasar
                //            && _sector.System.StarType == StarType.RadioPulsar
                //            )
                //        {
                //            if (!_possible_locations_to_NOT_build_stations.Contains(_sector.Location))
                //            {
                //            _possible_locations_to_NOT_build_stations.Add(_sector.Location); // added to *locations*  !!
                //            }

                //            //_possible_locations_to_NOT_build_stations.Distinct();
                //        }

                //        _text = _civM.MapData.GetFuelRange(_sector.Location).ToString();
                //        if (_civM.MapData.GetFuelRange(_sector.Location) < 1)
                //        {
                //            if (!_possible_locations_to_NOT_build_stations.Contains(_sector.Location))
                //            {
                //                _possible_locations_to_NOT_build_stations.Add(_sector.Location); // added to *locations*  !!
                //            }
                //        }
                //        else
                //        {
                //            if (!_prior_objects_to_build_stations.Contains(_sector.Location))
                //            {
                //                _prior_objects_to_build_stations.Add(_sector.Location); // added to *locations*  !!
                //            }
                //        }


                //        if (_sector.Owner != null && _sector.System.Owner != _civM.Civilization)
                //        {
                //            _possible_locations_to_NOT_build_stations.Add(_sector.Location); // nothing
                //        }
                //        else
                //        {
                //            if (!_possible_locations_to_NOT_build_stations.Contains(_sector.Location))
                //            {
                //                _possible_locations_to_NOT_build_stations.Add(_sector.Location); // added to *locations*  !!
                //            }
                //        }

                //        //CivilizationMapData _civMapData = GetMapDataForCivilization(_civM.Civilization);


                //        if (!_possible_locations_to_NOT_build_stations.Contains(_sector.Location))
                //        {
                //            _possible_locations_to_NOT_build_stations.Add(_sector.Location); // added to *locations*  !!
                //        }



                //        _civM.Locations_To_NOT_Build_Stations = _possible_locations_to_NOT_build_stations;
                //        //continue;

                //        _i = 0;
                //        foreach (var _item in _possible_locations_to_NOT_build_stations)
                //        {
                //            _i += 1;
                //            Console.WriteLine("Step_6662:; GameEngine > possible_locations_to_NOT_build_stations= " + _i + " > " + _item + " for " + _civM.Civilization);
                //        }

                //        //bool _bool_is_human = _civM.Civilization.IsHuman;
                //        if (_bool_is_human == true)
                //        {
                //            Debugger.Break();
                //        }

                //        List<Fleet> _ownConstructionFleets = new List<Fleet>();
                //        foreach (var _item in _ownFleets)
                //        {
                //            if (_item.IsConstructor)
                //            {
                //                _ownConstructionFleets.Add(_item);
                //                _ownConstructionFleets.Distinct();

                //            }
                //        }


                //        if (_ownConstructionFleets.Any(f => f.Route.Waypoints.Any(wp => _sector.Location == wp)))
                //        {
                //            continue;
                //        }

                //        if (!_possible_locations_to_NOT_build_stations.Contains(_sector.Location))
                //        {
                //            _prior_objects_to_build_stations.Add(_sector.Location);
                //        }

                //        _prior_objects_to_build_stations.Distinct();
                //    }

                //    if (_check_sectors == null || _check_sectors.Count == 0)
                //    {
                //        _check_sectors.Add(_civM.HomeSystem.Sector);
                //    }

                //    _i = 0;
                //    //Report_possible_locations_to_explore(_possible_locations_to_explore);
                //    foreach (var _item in _check_sectors)
                //    {
                //        _i += 1;
                //        Console.WriteLine("Step_6665:; GameEngine > possible_locations_to_build_stations= " + _i + " > " + _item + " for " + _civM.Civilization);
                //    }


                //    Dictionary<MapLocation, int> _sectorValues = new Dictionary<MapLocation, int>();
                //    string _text_sectorValues = "";

                //    int _xloc = GameContext.Current.Universe.Map.Width / 2;
                //    int _yloc = GameContext.Current.Universe.Map.Height / 2;

                //    MapLocation _center = new MapLocation(_xloc, _yloc);


                //    if (_radius < 100 && _check_sectors.Count < 9)
                //    {
                //        _radius += 1;
                //        goto Expand_Radius_For_Build_Stations;
                //    }
                //    else
                //    {

                //        foreach (var _sector in _check_sectors)
                //        {
                //            int _distance_from_center = MapLocation.GetDistance(_sector.Location, _center);
                //            int _distance_from_home = MapLocation.GetDistance(_sector.Location, _civM.HomeSystem.Location);
                //            int _distance_from_stranded = -1;
                //            if (_civM.StrandedShipsSector != null)
                //            {
                //                if (_ownFleets.Any(_f => _f.IsStranded)) // is still stranded
                //                {
                //                    _distance_from_stranded += 20000 + MapLocation.GetDistance(_sector.Location, _civM.StrandedShipsSector.Location) * 1000;
                //                }
                //                else
                //                {
                //                    _civM.StrandedShipsSector = null;
                //                }
                //            }

                //            int _val = 40000 - (_distance_from_center * 1000);
                //            _val += 80000 - (_distance_from_home * 1000);
                //            _val += _distance_from_stranded;

                //            const int SystemSectorValue = 2500;
                //            //const int StrandedShipSectorValue = 50000;
                //            //const int PastFuelRange = 30000;     // FuelRange is above
                //            //const int DistanceFactor = 3000; // was 100

                //            if (_sector.System != null)
                //            {
                //                _val += SystemSectorValue;

                //                if (_sector.System.HasColony)
                //                {
                //                    _val += SystemSectorValue; // another 'value'
                //                }
                //            }

                //            //if (_sector)

                //            _sectorValues.Add(_sector.Location, _val);
                //            //_sectorValues.Distinct();
                //            ////_text_sectorValues += _newline + "Step_3361:; " + _fleet.Owner + ": " + _sector.Location + ", val= " + _val
                //            // //_sectorValues.OrderBy(_y => _y.Value)/*.ToList()*/;
                //            // var _sortedDict = _sectorValues
                //            //            .OrderBy(kv => kv.Value)
                //            //            .ToDictionary(kv => kv.Key, kv => kv.Value);


                //            //_prior_objects_to_build_stations = _sectorValues.Keys.ToList();
                //            //    ;
                //        }

                //        _sectorValues.Distinct();
                //        //_text_sectorValues += _newline + "Step_3361:; " + _fleet.Owner + ": " + _sector.Location + ", val= " + _val
                //        //_sectorValues.OrderBy(_y => _y.Value)/*.ToList()*/;
                //        var _sortedDict = _sectorValues
                //                   .OrderByDescending(kv => kv.Value)
                //                   .ToDictionary(kv => kv.Key, kv => kv.Value);


                //        _prior_objects_to_build_stations = _sortedDict.Keys.ToList();

                //        String _values = "";
                //        foreach (var _item in _sortedDict)
                //        {
                //            _values += "Step_3367:; sorted _prior_objects_to_build_stations= " 
                //                + GameEngine.LocationString(_item.Key.ToString())
                //                + ": Value= " + _item.Value
                //                + " for " + _civM.Civilization
                //                + _newline
                //                ;
                //        }
                //        Console.WriteLine(_values);


                //        //_sectorValues = 
                //        //_sectorValues.OrderBy(_y => _y.Value)/*.ToList()*/;
                //        foreach (var _item in _prior_objects_to_build_stations)
                //        {
                //            //Console.WriteLine("Step_3365:; sorted possible_locations_to_build_stations= " + _item + " for " + _civM.Civilization);
                //        }

                //        if (_bool_is_human)
                //        {
                //            //Debugger.Break();
                //        }

                //        _civM.Locations_To_Build_Stations = _prior_objects_to_build_stations;

                //        UnitAI.Clear_Locations_To_Build_Stations_in_UnitAI(_civM); // clear in UnitAI too





                //        //No_construction_ships_at_all:;
                //    }

                //}
                //No_construction_ships_at_all:;

                // _fleet.Order?.OnTurnBeginning();
                //            HashSet<Fleet> _all_fleets = _game.Universe.Find<Fleet>();
                //            foreach (Fleet _fleet in _all_fleets)
                //            {
                //                _text = "Step_1051:; _fleet.Order?.OnTurnBeginning() for= " + UnitAI.CreateUpdateFleetText(_fleet, out string _fleet_text)
                ////+ " > _fleet.Order?.OnTurnBeginning()"
                ////+ " - Name= " + _civM.Civilization
                ////+ _newline
                //;
                //                Console.WriteLine(_text);

                //                _fleet.Order?.OnTurnBeginning();
                //                //SitReps_for_Fleets(_fleet);
                //            }
                //if (_fleet == null)
                //{
                //    Debugger.Break();
                //    continue;
                //}

                //_fleet.Order?.OnTurnBeginning();

                //foreach (Ship ship in _fleet.Ships)
                //{
                //    //if (!_fleet.Route.IsEmpty) 
                //    _text = GameEngine.LocationString(ship.Location.ToString()) + " > Ship ";
                //    string _design = ship.DesignName + "  ";
                //    while (_design.Length < 28)
                //    {
                //        _design += "_";
                //    }


                //    _text += GameEngine.Do_x_Digit_String( 5, (ship.ObjectID.ToString()) + ": " /*+ " < " */+ _design + " - Maint. " + ship.Design.MaintenanceCost + " > * " + " " + ship.Name + "  * > ";
                //    _text += " " + _fleet.Order;
                //    if (!_fleet.Route.IsEmpty)
                //    {
                //        MapLocation _aim = _fleet.Route.Waypoints.LastOrDefault();
                //        Sector _aimSector = GameContext.Current.Universe.Map[_aim];
                //        _fleet.Order = FleetOrders.TravelOrder.Create();
                //        //GameContext.Current.Universe.Find<MapLocation>().TryFindFirstItem(o => o == _aim, out Sector _aimSector);
                //        _text += " # going to " + _aim.ToString() + " named " + _aimSector.Name/* + " (PostTurnOps)"*/;
                //    }

                //    // is here something to do ? 2024-12-29

                //    //if (_fleet.Route.IsEmpty)
                //    //{
                //    //    if (_fleet.Order == FleetOrders.EngageOrder)
                //    //    {
                //    //        _fleet.SetOrder(FleetOrders.IdleOrder);
                //    //    }
                //    //}

                //    CivilizationManager _civM = GameContext.Current.CivilizationManagers[ship.OwnerID];
                //    CivilizationManager PlayerCivManager = GameContext.Current.CivilizationManagers[0];  // Federation - can be changed

                //    // only own civilization
                //    Console.WriteLine("Step_3583:; Turn " + GameContext.Current.TurnNumber + " > " + _text);
                //    //GameLog.Core.Combat.DebugFormat("Step_3282: " + _text);

                //    _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, ship.Location, _text, "", "", SitRepPriority.Pink));

                //    // all ships shown
                //    //PlayerCivManager.SitRepEntries.Add(new ShipStatusSitRepEntry(PlayerCivManager.Civilization, ship.Location, _rep));
                //} // end of each ship
                //}

                //HashSet<Fleet> _fleets = _game.Universe.Find<Fleet>();

                //foreach (Fleet _fleet in _fleets)
                //{
                //    if (_fleet == null)
                //    {
                //        Debugger.Break();
                //        continue;
                //    }

                //    // that'_sector too much Console.Writeline and why PreTurn ?
                //    //if (_fleet.ObjectID == -1)
                //    //{
                //    //    // for example construction ship destroyed after build is finished ?
                //    //    //Debugger.Break();
                //    //}
                //    //else
                //    //{
                //    //    //_text = "Step_1151:; trying > " + _fleet.ObjectID + " "+ _fleet.Name;
                //    //    //if (_writeDirectly) Console.WriteLine(_text);

                //    //    _text += "Step_1157:; " + UnitAI.CreateUpdateFleetText(_fleet, out string _fleetText)
                //    //        ;
                //    //    //if (_writeDirectly) Console.WriteLine(_text);
                //    //    _allFleetsReport += _newline + _text;
                //    //}
                //    //if (_writeDirectly) Console.WriteLine("Step_1158:; begin of > _allFleetsReport:" + _newline 
                //    //    + _allFleetsReport + _newline
                //    //    + "end of > _allFleetsReport" 
                //    //    );


                //}


                //Debugger.Break();

                //_ = ParallelForEach(_civManagers, _civM_1 =>
                //  {

                //    //GameContext.PushThreadContext(_game);
                //    //_civM.SitRepEntries.Clear();
                //    try
                //{
                //    //_civM_1.SitRepEntries.Clear();

                //    try
                //        {
                //            List<SitRepEntry> civSitReps = IntelHelper.SitReps_Temp.Where(o => o.Owner == _civM.Civilization).ToList();

                //            if (civSitReps.Count > 0)
                //            {
                //                foreach (SitRepEntry entry in civSitReps)
                //                {
                //                    _civM.SitRepEntries.Add(entry);
                //                }
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            _text = "Step_1159:; ERROR > " + e.Message;
                //            Console.WriteLine(_text);
                //            GameLog.Client.General.ErrorFormat(_text);
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        _errors.Push(e);
                //        _text = "Step_1158:; ERROR > " + e.Message;
                //        Console.WriteLine(_text);
                //        //GameLog.Client.General.ErrorFormat("SitRepEntries clear error ={0}", e);
                //    }
                //    finally
                //    {
                //        _text = "Step_1152:; Foreach _civM is done for > " + _civM.Civilization.Key;
                //        Console.WriteLine(_text);
                //        //GameLog.Client.General.DebugFormat(_text);

                //        //_ = GameContext.PopThreadContext();
                //    }

                //});



                //IntelHelper.SitReps_Temp.Clear();

                //turnnumber = GameContext.Current.TurnNumber;

                //if (!_errors.IsEmpty)
                //{
                //    Debugger.Break();
                //    throw new AggregateException(innerExceptions: _errors);
                //}
                //else
                //{
                //    _ = GameContext.PopThreadContext();
                //}

                // This block is not guaranteed to be safe for parallel execution.
                //GameContext.PushThreadContext(_game);

                //string _allFleetsReport = "";


            }

            Console.WriteLine(_pre_turn_text + _newline + "End of Do_11_PreTurn");

            _text = "Debugger.Break();";
            //Debugger.Break();
            //_text = "End of Do_11_PreTurn";
        }

        private void Find_Locations_To_Build_Stations(CivilizationManager _civM)
        {
            string _text = _civM.Civilization.Key;

            int _radius = 3 + GameContext.Current.TurnNumber / 10;

            List<MapLocation> _locations_to_NOT_build_stations = new List<MapLocation>();
            List<MapLocation> _available_locations_to_build_stations = new List<MapLocation>();

            bool _bool_is_human = _civM.Civilization.IsHuman;

            List<Sector> _check_sectors = MapHelper.GetSectorsWithinRadius(_civM.HomeSystem.Sector, _radius).ToList();

            foreach (var _sector in _check_sectors)
            {

                if (_sector.IsOwned && _sector.Owner.CivID == _civM.CivilizationID)
                //&& _sector.System == null
                {
                    _locations_to_NOT_build_stations.Add(_sector.Location);
                    if (_bool_is_human)
                        Report_Sector_Message(_sector, " > no building > isOwned by ourself");
                    continue;
                }

                //if (!_sector.IsOwned
                //        && _sector.System == null)
                //{
                //    _locations_to_NOT_build_stations.Add(_sector.Location);
                //}

                if (_sector.System != null
                    && _sector.System.StarType == StarType.BlackHole)
                {
                    _locations_to_NOT_build_stations.Add(_sector.Location);
                    if (_bool_is_human)
                        Report_Sector_Message(_sector, " > no building > BlackHole  ");
                    break;
                }

                if (_sector.Station != null)
                {
                    _locations_to_NOT_build_stations.Add(_sector.Location);
                    if (_bool_is_human)
                        Report_Sector_Message(_sector, " > no building > has already a station  ");
                    continue;
                }

                if (_sector.System != null && _sector.System.Colony != null
                    && _sector.System.Colony.Owner.IsEmpire) // Ownership can change !
                {
                    _locations_to_NOT_build_stations.Add(_sector.Location);
                    if (_bool_is_human)
                        Report_Sector_Message(_sector, " > no building > has an Empire's colony  ");
                    continue;
                }

                if (_bool_is_human == true)
                {
                    //Report_Sector(_sector);

                    //Debugger.Break();
                }

                if (!_civM.Locations_To_NOT_Build_Stations.Contains(_sector.Location))
                {
                    _available_locations_to_build_stations.Add(_sector.Location);
                    if (_bool_is_human)
                        Report_Sector_Message(_sector, " > available for building  ");
                }




                _locations_to_NOT_build_stations = _locations_to_NOT_build_stations.Distinct().ToList();

            }

            _available_locations_to_build_stations = _available_locations_to_build_stations.Distinct().ToList();
            //_civM.Locations_To_Build_Stations = _available_locations_to_build_stations;

            //if (_bool_is_human)
            //    Report_Locations_To_Build_Stations(_civM);

            //_text = "Step_6681:; GameEngine > _available_locations_to_build_station"
            //        + "  > found > " + _available_locations_to_build_stations.Count
            //        + " for " + _civM.Civilization
            //        ;

            //Console.WriteLine(_text);

            //Debugger.Break();

            //List<Station> _own_stations = new List<Station>();
            List<Station> _own_stations = GameContext.Current.Universe.FindOwned<Station>(_civM.Civilization).ToList();

            foreach (var _item in _own_stations)
            {
                _check_sectors = MapHelper.GetSectorsWithinRadius(_item.Sector, 2).ToList();

                foreach (var _sector in _check_sectors)
                {
                    _locations_to_NOT_build_stations.Add(_sector.Location);
                    Report_Sector_Message(_sector, " no building > next to an own station");
                }
            }

            _locations_to_NOT_build_stations = _locations_to_NOT_build_stations.Distinct().ToList();

            _civM.Locations_To_NOT_Build_Stations = _locations_to_NOT_build_stations;

            //_civM.Locations_To_NOT_Build_Stations = _locations_to_NOT_build_stations;

            if (_bool_is_human == true)
            {
                Report_Locations_To_NOT_Build_Stations(_civM);

                GameEngine.DummyCodeComment("Debugger.Break();");
                //Debugger.Break();
            }

            // Merge and populate _civM
            //foreach (var item in _civM.Locations_To_Build_Stations)
            //{
            //    _available_locations_to_build_stations.Add(item);
            //}
            List <MapLocation> _TMP_available_locations_to_build_stations = _available_locations_to_build_stations.Distinct().ToList();

            var _random = new Random();
            //_TMP_available_locations_to_build_stations = 
            List<MapLocation> _TMP2_available_locations_to_build_stations = Shuffle(_TMP_available_locations_to_build_stations);
            //_random.shuffle
            //_random.Shuffle(_TMP_available_locations_to_build_stations).;//.Shuffle();
            //_TMP_available_locations_to_build_stations.

            _civM.Locations_To_Build_Stations.Clear();



            foreach (var _location in _TMP_available_locations_to_build_stations)
            {
                _text = _location.ToString();

                if (!_locations_to_NOT_build_stations.Contains(_location))
                {
                    _civM.Locations_To_Build_Stations.Add(_location);
                }
            }
            Report_Locations_To_Build_Stations(_civM, "finally");
        }

        private void Report_Sector_Message(Sector _sector, string _message)
        {
            if (_sector == null) { return; }

            string _text = "Step_6661:; ..checking > " + SectorString(_sector, out string _sector_text) + _message;

            Console.WriteLine(_text);
        }

        private void Report_Locations_To_NOT_Build_Stations(CivilizationManager _civM)
        {
            string _text = "";

            if (_civM.Locations_To_NOT_Build_Stations == null || _civM.Locations_To_NOT_Build_Stations.Count == 0)
            {
                _text += "Step_6664:; GameEngine > _possible_locations_to_Not_Build_Stations= empty or Count= 0 " + " for " + _civM.Civilization;
                Console.WriteLine(_text);
                return;
            }

            int _count = 0;
            foreach (var item in _civM.Locations_To_NOT_Build_Stations)
            {
                _count += 1;
                _text += "Step_6665:; GameEngine > _possible_locations_to_NOT_Build_Stations= "
                                        + " # " + _count + " > "
                    + GameEngine.LocationString(item.ToString()) + " for " + _civM.Civilization
                    + Environment.NewLine

                    ;
            }
            Console.WriteLine(_text);
        }

        //    private void Find_Locations_To_NOT_Build_Stations(CivilizationManager _civM)
        //    {
        //        List<Fleet> _ownConstructorFleets = new List<Fleet>();
        //        //return; > 
        //        string _text = "Step_6666:; GameEngine > Find_Locations_To_NOT_Build_Stations .. starts .." + " for " + _civM.Civilization;
        //        Console.WriteLine(_text);

        //        _ownConstructorFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization).Where(f => f.IsConstructor).ToList();

        //        List<MapLocation> _possible_locations_to_NOT_build_stations = new List<MapLocation>();
        //        _possible_locations_to_NOT_build_stations.Add(_civM.HomeSystem.Location);
        //        _civM.Locations_To_Build_Stations = _possible_locations_to_NOT_build_stations;

        //        //Debugger.Break();

        //        if (_ownConstructorFleets.Count == 0)
        //        {
        //            return;
        //        }

        //        List<MapLocation> _prior_objects_to_build_stations = new List<MapLocation>();
        //        List<MapLocation> _possible_locations_to_build_stations = new List<MapLocation>();  // unused ??


        //        // _ownfleets for checking stranded ships
        //        List<Fleet> _ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization).Where(f => f.CanMove /*&& !f.Route.IsEmpty*/).ToList();

        //        Sector _home_sector = _civM.HomeSystem.Sector;

        //        List<Sector> _availableSectors = new List<Sector> { _home_sector };
        //        List<Sector> _maybeSectors = new List<Sector> { _home_sector };


        //        _civM.Locations_To_Build_Stations = null;
        //        int _i = 0;
        //        int _radius = 3;
        //        bool _bool_is_human = _civM.Civilization.IsHuman;
        //        //string _text = "";
        //        string _newline = Environment.NewLine;

        //        //Sector _homeSector = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Sector;

        //        MapLocation _home_location = _civM.HomeSystem.Location;
        //        int _home_x = _civM.HomeSystem.Location.X;
        //        int _home_y = _civM.HomeSystem.Location.Y;
        //        Quadrant _home_quadrant = _civM.HomeSystem.Quadrant;
        //        int _width_direction = 0;
        //        int _height_direction = 0;

        //        int _map_width = GameContext.Current.Universe.Map.Width;
        //        int _map_heigth = GameContext.Current.Universe.Map.Height;

        //        MapLocation _center = new MapLocation(_map_width / 2, _map_heigth / 2);

        //        switch (_home_quadrant)
        //        {
        //            case Quadrant.Gamma:
        //                _width_direction = 5;
        //                _height_direction = 5;
        //                break;

        //            case Quadrant.Delta:
        //                _width_direction = -5;
        //                _height_direction = 5;
        //                break;

        //            case Quadrant.Alpha:
        //                _width_direction = 5;
        //                _height_direction = -5;
        //                break;

        //            case Quadrant.Beta:
        //                _width_direction = -5;
        //                _height_direction = -5;
        //                break;

        //        }
        //        int _new_x = _home_x + _width_direction;
        //        int _new_y = _home_y + _height_direction;
        //        MapLocation _new_loc = new MapLocation(_new_x, _new_y);

        //        _text = "Step_6671:; GameEngine > _possible_locations_to_Not_Build_Stations > checking > nt= 0 "
        //            + "  > checking " + _new_loc.ToString()
        //            + " for " + _civM.Civilization
        //            ;
        //        Console.WriteLine(_text);

        //        //int _step = 5; // each fifth sector one station


        //        List<UniverseObject> _objects_around_home = GameContext.Current.Universe.Objects
        //            .Where(s => s.Location != null
        //            && s.ObjectType != UniverseObjectType.Ship
        //            && s.ObjectType != UniverseObjectType.Fleet
        //            //&& s.ObjectType != UniverseObjectType.StarSystem.bla
        //            && s.Sector.Station == null
        //            //&& s.Sector.Owner == null  > not own sectors =  just a few objects left in "not own sectors"
        //            && MapLocation.GetDistance(_home_location, s.Location) < 5 + (GameContext.Current.TurnNumber / 4)
        //            //&& GetDistanceTo(_center, s.Location) < xloc - 3 // but if there is a system + BORG don't colonize... > ToDo 2025-03-16
        //            )  // do not build at the edge of the map
        //               //&& GetDistanceTo(_center, s.Location) < lengthThirdMap)
        //            .ToList();

        //        _text = "Step_6674:; GameEngine > _possible_locations_to_Not_Build_Stations"
        //            + " > checking > " + _objects_around_home.Count + " objects around home location " + _home_location.ToString()
        //            + " for " + _civM.Civilization
        //            ;
        //        Console.WriteLine(_text);

        //        int _distance_to_home = 4;

        //        //Debugger.Break();

        //        if (_bool_is_human == true)
        //        {
        //            Debugger.Break();
        //        }


        //    Expand_distance_to_home:;

        //        //List<Object> _blackholes = new List<object>();
        //        //_i = 0;
        //        foreach (var _obj in _objects_around_home)
        //        {
        //            if (_obj.Sector != null && _obj.Sector.System != null)
        //            {
        //                switch (_obj.Sector.System.StarType)
        //                {
        //                    //case StarType.White:
        //                    //    break;
        //                    //case StarType.Blue:
        //                    //    break;
        //                    //case StarType.Yellow:
        //                    //    break;
        //                    //case StarType.Orange:
        //                    //    break;
        //                    //case StarType.Red:
        //                    //    break;
        //                    //case StarType.Nebula:
        //                    //    break;
        //                    //case StarType.Wormhole:
        //                    //    break;
        //                    //case StarType.NeutronStar:
        //                    //    break;
        //                    //case StarType.RadioPulsar:
        //                    //    break;
        //                    //case StarType.XRayPulsar:
        //                    //    break;
        //                    //case StarType.Quasar:
        //                    //    break;
        //                    case StarType.BlackHole:
        //                        //_blackholes.Add(_obj); // and than ??? // already 
        //                        continue;
        //                        //break;
        //                        //default:
        //                        //    break;
        //                }
        //            }

        //            Report_Sector(_obj.Sector);

        //            if (_bool_is_human == true)
        //            {
        //                Report_Sector(_obj.Sector);
        //            }
        //            //Report_Sector(_obj.Sector);
        //            //_distance_to_home = MapLocation.GetDistance(_home_location, _obj.Location);
        //            if (!_prior_objects_to_build_stations.Contains(_obj.Location)
        //                && MapLocation.GetDistance(_home_location, _obj.Location) < _distance_to_home)
        //            {
        //                if (_obj.Sector != null && _obj.Sector.System != null && _obj.Sector.System.StarType != StarType.BlackHole)
        //                {
        //                    continue;
        //                }

        //                if (_obj.Sector.System.HasColony && !_obj.Sector.System.Colony.Owner.IsEmpire)
        //                {
        //                    continue;
        //                }

        //                _prior_objects_to_build_stations.Add(_obj.Location);
        //                if (_bool_is_human == true)
        //                {
        //                    Report_Sector(_obj.Sector);
        //                    //Debugger.Break();
        //                }
        //            }
        //        }
        //        _distance_to_home += 1;
        //        _prior_objects_to_build_stations.Distinct();


        //        _text = "Step_6671:; GameEngine > _possible_locations_to_Not_Build_Stations"
        //+ "  > _distance_to_home= " + _distance_to_home
        //+ "  > found > " + _prior_objects_to_build_stations.Count
        //+ " for " + _civM.Civilization
        //;
        //        Console.WriteLine(_text);
        //        Debugger.Break();



        //        //if (_prior_objects_to_build_stations.Count < 4)
        //        //{
        //        //    //Debugger.Break();
        //        //    goto Expand_distance_to_home;
        //        //}




        //        //_prior_objects_to_build_stations.Add(_new_loc);
        //        _prior_objects_to_build_stations.Distinct();

        //        if (_prior_objects_to_build_stations.Count > 0)
        //        {
        //            _civM.Locations_To_Build_Stations = _prior_objects_to_build_stations;
        //        }

        //        Report_Locations_To_Build_Stations(_civM);

        //        if (_bool_is_human)
        //        {
        //            //Debugger.Break();
        //        }


        //        //Clear_Locations_To_Build_Stations(_civM);

        //        if (_civM.Locations_To_Build_Stations.Count < 5)
        //        {
        //            //Locations_To_Build_Stations_ADD_by_Grid(_civM);

        //            for (int i = 1; i < 9; i++)
        //            {
        //                _new_x = _home_x + (_width_direction * i);
        //                if (_new_x < 0 || _new_x > _map_width)
        //                {
        //                    return;
        //                }
        //                _new_y = _home_y + (_height_direction * i);
        //                if (_new_y < 0 || _new_y > _map_heigth)
        //                {
        //                    return;
        //                }

        //                _new_loc = new MapLocation(_new_x, _new_y);
        //                Sector _check_sector = new Sector(_new_loc);

        //                List<Sector> _grid_sectors = MapHelper.GetSectorsWithinRadius(_check_sector, 3).ToList();
        //                List<Sector> _provided_sectors = new List<Sector> { _home_sector };

        //                foreach (var item in _grid_sectors)
        //                {
        //                    _provided_sectors.Distinct();

        //                    if (_provided_sectors.Contains(item))
        //                    {
        //                        continue;
        //                    }

        //                    if (item.Station == null)
        //                    {
        //                        continue;
        //                    }

        //                    if (item.Station.OwnerID == _civM.CivilizationID)
        //                    {
        //                        _provided_sectors = MapHelper.GetSectorsWithinRadius(_check_sector, 3).ToList();
        //                        _provided_sectors.Distinct();
        //                    }

        //                    if (item.System != null && item.System.StarType != StarType.BlackHole)
        //                    {
        //                        if (item.System.Colony != null && !item.System.Colony.Owner.IsEmpire
        //                            && !_provided_sectors.Contains(item))
        //                        {
        //                            _civM.Locations_To_Build_Stations.Add(item.Location);
        //                        }
        //                    }

        //                    if (item.System == null && _provided_sectors.Contains(item))
        //                    {
        //                        _civM.Locations_To_Build_Stations.Add(item.Location);
        //                        break; // only select one sector in this area
        //                    }
        //                }
        //            }


        //        }
        //    }

        private void Report_Locations_To_Build_Stations(CivilizationManager _civM, string _message)
        {
            if (_civM.Locations_To_Build_Stations == null)
            {
                return;
            }
            string _text = "";
            int _count = 0;
            foreach (var item in _civM.Locations_To_Build_Stations)
            {
                _count += 1;
                _text += "Step_6665:; possible_locations_to_build_stations= "
                                        + " # " + GameEngine.Do_x_Digit_String(2, _count.ToString()) + " > "
                    + GameEngine.LocationString(item.ToString()) + " for " + _civM.Civilization
                    + " > " + _message
                    + Environment.NewLine

                    ;
            }
            Console.WriteLine(_text);
        }

        //private void Report_Sector(Sector _sector)
        //{
        //    if (_sector == null)
        //    {
        //        return;
        //    }


        //    string _text = "Step_6663:; ..checking > " + SectorString(_sector, out string _sector_text);

        //    Console.WriteLine(_text);
        //}

        private string SectorString(Sector _sector, out string _sector_text)
        {
            _sector_text = "";
            string _star_type = "";
            if (_sector == null)
            {
                return "no _sector available";
            }



            _sector_text = LocationString(_sector.Location.ToString())
                //+ " > Sector " + _sector.Name


                ;

            if (_sector.Owner != null)
            {
                _sector_text += " > Owner= " + _sector.Owner;
            }
            else
            {
                _sector_text += " > Owner= NOBODY";
            }

            if (_sector.System != null)
            {



                if (_sector.System.Colony != null)
                {
                    _sector_text += " ,Colony= " + _sector.System.Colony.Name;
                }
                else
                {
                    _sector_text += " ,Colony= NO";
                }
                _star_type = " ( " + _sector.System.StarType + " )";
            }

            _sector_text += " for Sector= " + _sector.Name + _star_type;

            return _sector_text;
        }

        private void Find_Locations_To_Explore(CivilizationManager _civM)
        {
            List<MapLocation> _possible_locations_to_explore = new List<MapLocation>(); // locations need less storage
            List<Sector> _possible_sectors_to_explore = new List<Sector>();
            int _radius = 2;
            List<Fleet> _ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization).Where(f => f.CanMove /*&& !f.Route.IsEmpty*/).ToList();

        Expand_Radius:;

            _possible_sectors_to_explore = MapHelper.GetSectorsWithinRadius(_civM.HomeSystem.Sector, _radius).ToList();
            foreach (var _sector in _possible_sectors_to_explore)
            {
                //Console.WriteLine("Step_6665:; possible_locations_to_explore= " + _i + " > " + _item + " for " + _civM.Civilization);
                if (_civM.MapData.IsExplored(_sector.Location))
                {
                    continue;
                }

                if (DiplomacyHelper.IsTravelAllowed(_civM.Civilization, _sector) == false)
                {
                    continue;
                }

                if (_sector.System != null
                    && _sector.System.StarType == StarType.BlackHole
                    //&& _sector.System.StarType != StarType.XRayPulsar
                    //&& _sector.System.StarType != StarType.NeutronStar
                    //&& _sector.System.StarType != StarType.Quasar
                    //&& _sector.System.StarType != StarType.RadioPulsar
                    //&& _sector.System.StarType != StarType.Wormhole
                    )
                {
                    continue;
                }

                foreach (var item in _ownFleets)
                {
                    if (item.Route != null && item.Route.Waypoints.Count > 0 && item.Route.Waypoints.Last() == _sector.Location)
                    {
                        continue;
                    }
                }
                // old 
                //if (_ownFleets.Any(f => f.Route.Waypoints.Any(wp => _sector.Location == wp)))
                //{
                //    continue;
                //}

                // else
                _possible_locations_to_explore.Add(_sector.Location);
                _possible_locations_to_explore = _possible_locations_to_explore.Distinct().ToList();
            }

            //int _i = 0;



            if (_possible_locations_to_explore.Count < 9)
            {
                _radius += 1;
                goto Expand_Radius;
            }
            else
            {
                _possible_locations_to_explore = _possible_locations_to_explore.Distinct().ToList();
                _civM.Locations_To_Explore = _possible_locations_to_explore;
            }

            //Report_possible_locations_to_explore(_possible_locations_to_explore);
            //foreach (var _item in _possible_locations_to_explore)
            //{
            //    _i += 1;
            //    Console.WriteLine("Step_6663:; possible_locations_to_explore= " + _i + " > " 
            //        +GameEngine.LocationString(_item.ToString()) + " for " + _civM.Civilization);
            //}

            //sectors_to_build_stations

            Report_Locations_To_Explore(_civM);  // list out of _civM
        }

        private void Report_Locations_To_Explore(CivilizationManager _civM)
        {
            if (_civM.Locations_To_Explore == null)
            {
                return;
            }
            string _text = "";
            int _count = 0;
            foreach (var item in _civM.Locations_To_Explore)
            {
                _count += 1;
                _text += "Step_6663:; possible_locations_to_explore= "
                                        + " # " + _count + " > "
                    + GameEngine.LocationString(item.ToString()) + " for " + _civM.Civilization
                    + Environment.NewLine

                    ;
            }
        }

        private void Civ_Fire_Power_Space(CivilizationManager _civM)
        {
            List<Ship> _combatant_ships = GameContext.Current.Universe.FindOwned<Ship>(_civM.Civilization)
                    .Where(s => s.IsCombatant).ToList();

            int _int = 0;
            foreach (Ship ship in _combatant_ships)
            {
                //int _int_ship = ship.ShipDesign.PrimaryWeapon.Damage
                //                + ship.ShipDesign.SecondaryWeapon.Damage;


                _int += ship.Fire_Power_Orbital;
                //_int += ship.Fire_power_calculated();


                if (_civM.Civilization.IsHuman)
                {
                    //Debugger.Break();
                }
                //ship.Fire_power_calculated = _int_ship;
            }
            _civM.FirePowerSpace = _int;
            //_civM.FirePowerSpace = _combatant_ships.Fire_Power_Orbital.Sum();


            //if (_civM.Civilization.IsHuman)
            //{
            //Debugger.Break();
            //}

        }

        //private void Reset_Items(HashSet<UniverseObject> objects)
        private void Reset_Items(GameContext _game)
        {
            HashSet<UniverseObject> _objects = GameContext.Current.Universe.Objects.ToHashSet();

            ConcurrentStack<Exception> _errors = new ConcurrentStack<Exception>();
            string _text = "";

            Console.WriteLine("Step_1154:; resetting items...");
            //_ = ParallelForEach(_objects, _item =>
            foreach (var item in _objects)
            {
                //GameContext.PushThreadContext(_game);
                // GameLog.Core.General.DebugFormat("next _item will be: ID = {0}, Name = {1}", _item.ObjectID, _item.Name);
                try
                {
                    // GameLog.Core.General.DebugFormat("_item: ID = {0}, Name = {1} is trying to reset", _item.ObjectID, _item.Name);
                    item.Reset();
                    // works well but gives hidden info
                    // GameLog.Core.General.DebugFormat("_item: ID = {0}, Name = {1} is successfully resetted", _item.ObjectID, _item.Name);
                }
                catch (Exception e)
                {
                    _text = "Step_1156:; ### Object = _item.Reset() at Do_11_PreTurnOperations > crashed for ID= " + item.ObjectID
                        + " - Name= " + item.Name
                        ;
                    Console.WriteLine(_text);
                    GameLog.Core.General.ErrorFormat(_text);
                    Debugger.Break();
                    _errors.Push(e);
                }
                finally
                {

                    GameContext.PushThreadContext(_game);
                }
            }
            // jump over and break here
            //Debugger.Break();
            //;
            //});


            if (!_errors.IsEmpty)
            {
                foreach (var err in _errors)
                {
                    _text = "Step_1157:; ERROR > " + err.Message + err.StackTrace;
                    Console.WriteLine(_text);
                    GameLog.Core.General.DebugFormat(_text);

                    Debugger.Break();
                }

                //throw new AggregateException(_errors);   // avoid crashes = stopps for player as much as possible !
            }

            _errors.Clear();

            foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers)
            {
                //GameContext.PushThreadContext(_game);
                _civM.SitRepEntries.Clear();
            }

            HashSet<Fleet> allFleets = _game.Universe.Find<Fleet>();
            foreach (Fleet fleet in allFleets)
            {
                fleet.Order?.OnTurnBeginning();
                //SitReps_for_Fleets(_fleet);
            }
            //if (_fleet == null)
            //{
            //    Debugger.Break();
            //    continue;
            //}

            //_fleet.Order?.OnTurnBeginning();

            //foreach (Ship ship in _fleet.Ships)
            //{
            //    //if (!_fleet.Route.IsEmpty) 
            //    _text = GameEngine.LocationString(ship.Location.ToString()) + " > Ship ";
            //    string _design = ship.DesignName + "  ";
            //    while (_design.Length < 28)
            //    {
            //        _design += "_";
            //    }


            //    _text += GameEngine.Do_x_Digit_String( 5, (ship.ObjectID.ToString()) + ": " /*+ " < " */+ _design + " - Maint. " + ship.Design.MaintenanceCost + " > * " + " " + ship.Name + "  * > ";
            //    _text += " " + _fleet.Order;
            //    if (!_fleet.Route.IsEmpty)
            //    {
            //        MapLocation _aim = _fleet.Route.Waypoints.LastOrDefault();
            //        Sector _aimSector = GameContext.Current.Universe.Map[_aim];
            //        _fleet.Order = FleetOrders.TravelOrder.Create();
            //        //GameContext.Current.Universe.Find<MapLocation>().TryFindFirstItem(o => o == _aim, out Sector _aimSector);
            //        _text += " # going to " + _aim.ToString() + " named " + _aimSector.Name/* + " (PostTurnOps)"*/;
            //    }

            //    // is here something to do ? 2024-12-29

            //    //if (_fleet.Route.IsEmpty)
            //    //{
            //    //    if (_fleet.Order == FleetOrders.EngageOrder)
            //    //    {
            //    //        _fleet.SetOrder(FleetOrders.IdleOrder);
            //    //    }
            //    //}

            //    CivilizationManager _civM = GameContext.Current.CivilizationManagers[ship.OwnerID];
            //    CivilizationManager PlayerCivManager = GameContext.Current.CivilizationManagers[0];  // Federation - can be changed

            //    // only own civilization
            //    Console.WriteLine("Step_3583:; Turn " + GameContext.Current.TurnNumber + " > " + _text);
            //    //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

            //    _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, ship.Location, _text, "", "", SitRepPriority.Pink));

            //    // all ships shown
            //    //PlayerCivManager.SitRepEntries.Add(new ShipStatusSitRepEntry(PlayerCivManager.Civilization, ship.Location, _rep));
            //} // end of each ship
            //}

            //HashSet<Fleet> _fleets = _game.Universe.Find<Fleet>();

            //foreach (Fleet _fleet in _fleets)
            //{
            //    if (_fleet == null)
            //    {
            //        Debugger.Break();
            //        continue;
            //    }

            //    // that's too much Console.Writeline and why PreTurn ?
            //    //if (_fleet.ObjectID == -1)
            //    //{
            //    //    // for example construction ship destroyed after build is finished ?
            //    //    //Debugger.Break();
            //    //}
            //    //else
            //    //{
            //    //    //_text = "Step_1151:; trying > " + _fleet.ObjectID + " "+ _fleet.Name;
            //    //    //if (_writeDirectly) Console.WriteLine(_text);

            //    //    _text += "Step_1157:; " + UnitAI.CreateUpdateFleetText(_fleet, out string _fleetText)
            //    //        ;
            //    //    //if (_writeDirectly) Console.WriteLine(_text);
            //    //    _allFleetsReport += _newline + _text;
            //    //}
            //    //if (_writeDirectly) Console.WriteLine("Step_1158:; begin of > _allFleetsReport:" + _newline 
            //    //    + _allFleetsReport + _newline
            //    //    + "end of > _allFleetsReport" 
            //    //    );


            //}


            //Debugger.Break();

            //_ = ParallelForEach(_civManagers, _civM_1 =>
            //  {
            foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers) // foreachCivilizationManager
            {
                //GameContext.PushThreadContext(_game);
                //_civM.SitRepEntries.Clear();
                try
                {
                    //_civM_1.SitRepEntries.Clear();

                    try
                    {
                        List<SitRepEntry> civSitReps = IntelHelper.SitReps_Temp.Where(o => o.Owner == _civM.Civilization).ToList();

                        if (civSitReps.Count > 0)
                        {
                            foreach (SitRepEntry entry in civSitReps)
                            {
                                _civM.SitRepEntries.Add(entry);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _text = "Step_1159:; ERROR > " + e.Message;
                        Console.WriteLine(_text);
                        GameLog.Client.General.ErrorFormat(_text);
                    }
                }
                catch (Exception e)
                {
                    _errors.Push(e);
                    _text = "Step_1158:; ERROR > " + e.Message;
                    Console.WriteLine(_text);
                    //GameLog.Client.General.ErrorFormat("SitRepEntries clear error ={0}", e);
                }
                finally
                {
                    _text = "Step_1152:; Foreach _civM is done for > " + _civM.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.General.DebugFormat(_text);

                    _ = GameContext.PopThreadContext();
                }
                //});
            }
            ;

            //IntelHelper.SitReps_Temp.Clear();

            //turnnumber = GameContext.Current.TurnNumber;

            if (!_errors.IsEmpty)
            {
                throw new AggregateException(innerExceptions: _errors);
            }

            // This block is not guaranteed to be safe for parallel execution.
            //GameContext.PushThreadContext(_game);

            //string _allFleetsReport = "";


        }

        private void SitReps_for_Fleets(Fleet fleet)
        {
            string _text;

            if (fleet == null)
            {
                Debugger.Break();
                return;
            }

            //fleet.Order?.OnTurnBeginning();

            foreach (Ship ship in fleet.Ships)
            {
                //if (!_fleet.Route.IsEmpty) 
                _text = GameEngine.LocationString(ship.Location.ToString()) + " > Ship ";
                string _design = ship.DesignName + "  ";
                while (_design.Length < 28)
                {
                    _design += "_";
                }


                _text += GameEngine.Do_x_Digit_String(5, ship.ObjectID.ToString())
                    + ": " /*+ " < " */
                    + _design
                    + " - Maint. " + ship.Design.MaintenanceCost
                    + " > * " + " " + ship.Name + "  * > "
                    ;
                _text += " " + fleet.Order;
                if (!fleet.Route.IsEmpty)
                {
                    MapLocation _aim = fleet.Route.Waypoints.LastOrDefault();
                    Sector _aimSector = GameContext.Current.Universe.Map[_aim];
                    fleet.Order = FleetOrders.TravelOrder.Create();
                    //GameContext.Current.Universe.Find<MapLocation>().TryFindFirstItem(o => o == _aim, out Sector _aimSector);
                    _text += " # going to " + _aim.ToString() + " named " + _aimSector.Name/* + " (PostTurnOps)"*/;
                }

                // is here something to do ? 2024-12-29

                //if (_fleet.Route.IsEmpty)
                //{
                //    if (_fleet.Order == FleetOrders.EngageOrder)
                //    {
                //        _fleet.SetOrder(FleetOrders.IdleOrder);
                //    }
                //}

                CivilizationManager civManager = GameContext.Current.CivilizationManagers[ship.OwnerID];
                CivilizationManager PlayerCivManager = GameContext.Current.CivilizationManagers[0];  // Federation - can be changed

                // only own civilization
                Console.WriteLine("Step_3583:; Turn " + GameContext.Current.TurnNumber + " > " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

                civManager.SitRepEntries.Add(new ReportEntry_CoS(civManager.Civilization, ship.Location, _text, "", "", SitRepPriority.Pink));

                // all ships shown
                //PlayerCivManager.SitRepEntries.Add(new ShipStatusSitRepEntry(PlayerCivManager.Civilization, ship.Location, _rep));
            } // end of each ship
        }


        #region DoPreGameSetup() Method
        public void Do_04_PreGameSetup(GameContext _game)
        {
            ConcurrentStack<Exception> errors = new ConcurrentStack<Exception>();

            string _text;
            bool _writeDirectly = true;

            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            //  {
            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                GameContext.PushThreadContext(_game);
                try
                {
                    if (!GameContext.Current.CivilizationManagers.Contains(civ.CivID))
                    {

                        GameContext.Current.CivilizationManagers.Add(new CivilizationManager(_game, civ));
                        GameLog.Core.General.DebugFormat("New _civ added: {0}", civ.Name);
                        MapLocation _loc = GameContext.Current.CivilizationManagers[civ.CivID].HomeSystem.Location;
                        GameContext.Current.CivilizationManagers[civ.CivID].SitRepEntries.Add(
                            new ReportEntry_CoS(civ, _loc, "HomeSystem placed at " + _loc, "", "", SitRepPriority.Gray));
                        // generates a needed first SitRep in initialize SitRepCommentTextBox
                    }
                }
                catch (Exception e)
                {
                    errors.Push(e);
                }
                _ = GameContext.PopThreadContext();
                //});
            }
            ;

            if (!errors.IsEmpty)
            {
                throw new AggregateException(errors);
            }

            Do_24_MapUpdates(_game);

            //GameLog.Print("GameVersion = {0}", GameContext.Current.GameMod.Version);
            GameLog.Core.General.InfoFormat("Step_0900: Options: ---------------------------");
            GameLog.Core.General.InfoFormat("Step_0903: Options:GalaxySize = {0} ({1} x {2})", GameContext.Current.Options.GalaxySize, GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0906: Options:GalaxyShape = {0}", GameContext.Current.Options.GalaxyShape);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0913: Options:StarDensity = {0}", GameContext.Current.Options.StarDensity);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0916: Options:PlanetDensity = {0}", GameContext.Current.Options.PlanetDensity);
            GameLog.Core.General.InfoFormat("Step_0920: Options:StartingTechLevel = {0}", GameContext.Current.Options.StartingTechLevel);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0923: Options:MinorRaceFrequency = {0}", GameContext.Current.Options.MinorRaceFrequency);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0926: Options:GalaxyCanon = {0}", GameContext.Current.Options.GalaxyCanon);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0930: Options:---------------------------");
            GameLog.Core.GeneralDetails.DebugFormat("Step_0933: Options:FederationPlayable = {0}", GameContext.Current.Options.FederationPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0936: Options:RomulanPlayable = {0}", GameContext.Current.Options.RomulanPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0940: Options:KlingonPlayable = {0}", GameContext.Current.Options.KlingonPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0943: Options:CardassianPlayable = {0}", GameContext.Current.Options.CardassianPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0946: Options:DominionPlayable = {0}", GameContext.Current.Options.DominionPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0950: Options:BorgPlayable = {0}", GameContext.Current.Options.BorgPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0953: Options:TerranEmpirePlayable = {0}", GameContext.Current.Options.TerranEmpirePlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0956: Options:---------------------------");
            GameLog.Core.GeneralDetails.DebugFormat("Step_0960: Options:FederationModifier = {0}", GameContext.Current.Options.FederationModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0963: Options:RomulanModifier = {0}", GameContext.Current.Options.RomulanModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0966: Options:KlingonModifier = {0}", GameContext.Current.Options.KlingonModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0970: Options:CardassianModifier = {0}", GameContext.Current.Options.CardassianModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0973: Options:DominionModifier = {0}", GameContext.Current.Options.DominionModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0976: Options:BorgModifier = {0}", GameContext.Current.Options.BorgModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0980: Options:TerranEmpireModifier = {0}", GameContext.Current.Options.TerranEmpireModifier);

            GameLog.Core.GeneralDetails.DebugFormat("Step_0983: Options:EmpireModifierRecurringBalancing = {0}", GameContext.Current.Options.EmpireModifierRecurringBalancing);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0986: Options:GamePace = {0}", GameContext.Current.Options.GamePace);
            GameLog.Core.GeneralDetails.DebugFormat("Step_0990: Options:TurnTimer = {0}", GameContext.Current.Options.TurnTimerEnum);

            Table ToolTipImageSizeTable = GameContext.Current.Tables.UniverseTables["Sizes"];
            //AAASpecialWidth1 = (int)Number.ParseSingle(ToolTipImageSizeTable["Width"][0]);
            //AAASpecialHeight1 = (int)Number.ParseSingle(ToolTipImageSizeTable["Height"][0]);
            //string _text = "AAASpecialWidth1=" + AAASpecialWidth1 + " x " + "AAASpecialHeight1=" + AAASpecialHeight1;
            //if (_writeDirectly) Console.WriteLine(_text);
            //GameLog.Core.GeneralDetails.DebugFormat(_text);

            Table BuyModTable = GameContext.Current.Tables.GameOptionTables["BuyModifier"];
            int _buyMod = (int)Number.ParseSingle(BuyModTable["BuyMod"][0]);
            Table TaxModTable = GameContext.Current.Tables.GameOptionTables["TaxModifier"];
            int _taxMod = (int)Number.ParseSingle(TaxModTable["TaxMod"][0]);
            // 3rd one is Maintenance, but no change for this at the moment




            /* With StrengthModifier it is possible to increase some stuff or to decrease */
            /* default value is 1.0 - range shall be 0.1 to 1.9 */
            /* all modifier are working in generell, not race-speficic */
            Table strengthTable = GameContext.Current.Tables.GameOptionTables["StrengthModifier"];
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

            //string _newline = _newline;

            _text = "Step_0995:; StrengthModifier: (might be not used)"
                //+ _newline +
                //+ EspionageMod + " for EspionageMod" + _newline
                //+ SabotageMod + " for SabotageMod" + _newline
                //+ InternalSecurityMod + " for InternalSecurityMod" + _newline
                //+ ShipProductionMod + " for ShipProductionMod" + _newline
                //+ ScienceSpeedMod + " for ScienceSpeedMod" + _newline
                //+ MinorPowerMod + " for MinorPowerMod" + _newline
                //+ MiningMod + " for MiningMod" + _newline
                //+ CreditsMod + " for CreditsMod" + _newline
                //+ DiplomacyTrustMod + " for DiplomacyTrustMod" + _newline
                //+ DiplomacyRegardMod + " for DiplomacyRegardMod" + _newline
                //+ FoodProductionMod + " for FoodProductionMod" + _newline
                //+ RaidingMod + " for RaidingMod" + _newline
                //+ ShipVisibilityMod + " for ShipVisibilityMod" + _newline
                //+ StationsStrenghtMod + " for StationsStrenghtMod" + _newline
                //+ OrbitalBatteryStrenghtMod + " for OrbitalBatteryStrenghtMod" + _newline
                //+ TroopTransportStrenghtMod + " for TroopTransportStrenghtMod" + _newline
                //+ ColonyTroopStrenghtMod + " for ColonyTroopStrenghtMod" + _newline
                ;

            if (_writeDirectly) Console.WriteLine(_text);
            //GameLog.Core.GameInitDataDetails.DebugFormat(_text);

            //doesn't work'
            //_text = "Step_0997: Window-Size" + (Frame).w + " x " + System.Windows.Window.Size + ", Normal or Maximized? =" + Window.WindowStateProperty;
            //if (_writeDirectly) Console.WriteLine(_text);
            //GameLog.Core.General.DebugFormat(_text);

            _game.TurnNumber = 1;
            //_tn = 1;

        }
        #endregion

        #region DoFleetMovement() Method
        private void Do_12_FleetHandling(GameContext _game)
        {
            //#pragma warning disable IDE0059 // Unnecessary assignment of a value
            //int turnnumber = _game.TurnNumber;  // Dummy, do not remove
            //                                   //#pragma warning restore IDE0059 // Unnecessary assignment of a value
            List<Fleet> _allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();

            int fuelNeeded;
            string _newline = Environment.NewLine;
            string _text = "";
            string _text2 = "";
            bool _writeDirectly = true;


            _text = "Step_6001:; _allFleets.Count = " + _allFleets.Count;
            if (_writeDirectly) Console.WriteLine(_text);
            string _allFleets_report = _text + _newline;

            //GameLog.Core.GalaxyGeneratorDetails.DebugFormat(_text);

            foreach (var _civ in GameContext.Current.Civilizations)
            {
                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];
                List<Fleet> strandedFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civ).Where(o => o.IsStranded).ToList();

                List<Sector> sectorList = new List<Sector>();

                sectorList = UnitAI.FindStrandedShipSectors(_civ);

                bool _strandedShipsSectorStillRelevant = false;

                Sector _tmpStrandedShipsSector = _civM.HomeSystem.Sector;

                foreach (var _fleet in strandedFleets)
                {
                    //UnitAI.CreateUpdateFleetText(_fleet, out string _fleetText);
                    _text = _fleet.Location
                        //+ "   " + _fleet.ObjectID
                        + " > " + _fleet.Name
                        + " ( " + _fleet.Ships[0].DesignName + " )"
                    //+ " > " + _fleet.des
                    //GameEngine.LocationString(_fleet.Location.ToString())
                    //    + " > " +
                    //UnitAI.CreateUpdateFleetText(_fleet, out string _fleetText)
                    + " is stranded there..."

                        ;
                    _civM.SitRepEntries.Add(new ReportEntry_CoS(_civ, _fleet.Location, _text, "", "", SitRepPriority.Yellow));

                    if (_civM.StrandedShipsSector.Location.ToString() == _fleet.Sector.Location.ToString())
                    {
                        _strandedShipsSectorStillRelevant = true;
                    }


                }

                if (_strandedShipsSectorStillRelevant == false && _tmpStrandedShipsSector != null)
                {
                    _civM.StrandedShipsSector = _tmpStrandedShipsSector;

                    //_text =
                    //        _civM.StrandedShipsSector.Location
                    //        + " > = new StrandedShipsSector"

                    //            ;
                    //_civM.SitRepEntries.Add(new ReportEntry_CoS(_civ, _civM.StrandedShipsSector.Location, _text, "", "", SitRepPriority.Yellow));


                }
            }



            foreach (Fleet _fleet in _allFleets)
            {

                int shipNum = _fleet.Ships.Count();
                //GameEngine.LocationString(_colony.Location.ToString()) = GameEngine.LocationString(_fleet.Location.ToString());

                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner];

                string _singleShipDesign;
                if (shipNum == 1)
                    _singleShipDesign = _fleet.Ships[0].ShipDesign.ToString();
                else
                    _singleShipDesign = _fleet.Ships.Count() + " ships ";

                string _fleetAim = "None";
                string _fleetRouteSteps = "0";

                if (_fleet.Route != null && !_fleet.Route.IsEmpty)
                {
                    _fleetAim = _fleet.Route.Steps.Last().ToString();
                    _fleetRouteSteps = _fleet.Route.Steps.Count.ToString();

                    if (_fleetRouteSteps == "0")
                    {
                        _text =
                            "Step_6090:; Fleet " + _fleet.ObjectID
                            + " will arrive it's aim soon";
                        ;
                        //if (_writeDirectly) Console.WriteLine(_text);
                        _allFleets_report += _text + _newline;
                        _fleet.Activity = UnitActivity.Hold;
                    }
                }




                _text = "Step_6002:; "
                    + "Turn " + GameContext.Current.TurnNumber
                    + " > " + _fleet.Owner.Name
                    + " " + _singleShipDesign
                    + " > " + _fleet.ObjectID
                    + " " + _fleet.Name

                    + " at " + _fleet.Location

                    + ", Type " + _fleet.AITypeUnit
                    + ", " + _fleet.Activity
                    //+ " since Turn " + _fleet.ActivityStart
                    //+ ", Duration = " + _fleet.ActivityDuration
                    + ", AIM > " + _fleetAim
                    + ", OLD Order > " + _fleet.Order.OrderName
                    + ", RouteSteps = " + _fleetRouteSteps

                    ;
                //if (_writeDirectly) Console.WriteLine(_text);
                _allFleets_report += _text + _newline;
                //GameLog.Core.GalaxyGeneratorDetails.DebugFormat(_text);

                // works - _output for each single ship
                //if (_fleet.Route.Steps.Count() > 0 && shipNum > 1)
                //{
                //    for (int i = 0; i < shipNum; i++)
                //    {
                //        _text = "Step_6003:; Turn " + _game.TurnNumber + " >>> "
                //            + _fleet.Location;// + " > # Ship " + i /*+ " of Fleet"*/;

                //        Ship ship = _fleet.Ships[i];
                //        _text += ", Waypoints= " + _fleet.Route.Waypoints.Count + ", Steps= " + _fleet.Route.Steps.Count;
                //        _text += ", Order= " + _fleet.Order; //+ ", Steps= " + _fleet.Route.Steps.Count
                //        _text += " for Ship # "+ i + " = "  + ship.ObjectID + " " + ship.Name + " - " + ship.Design.Key;



                //        if (_writeDirectly) Console.WriteLine(_text);
                //        _allFleets_report += _text + _newline;
                //        //GameLog.Core.AIDetails.DebugFormat(_text);
                //    }
                //}


                // doubled info
                //if (_fleet.Activity == UnitActivity.Mission)
                //{
                //    _text =
                //        "Step_6094: Fleet " + _fleet.ObjectID
                //        + " > Activity = " + _fleet.Activity
                //        + " > AITypeUnit = " + _fleet.AITypeUnit
                //    ;
                //    if (_writeDirectly) Console.WriteLine(_text);
                //}

                //if (_fleet.AITypeUnit == AITypeUnit.Reserve)
                //{
                //    GameLog.Client.AIDetails.DebugFormat("*** Turn {0}: Reserve,  Owner = {1} Fleet location ={2}, AITypeUnit ={3}, UnitActivity ={4} Actibvity Duration ={5} Activity Start ={6}",
                //        turnnumber, _fleet.Owner.Name, _fleet.Location, _fleet.AITypeUnit, _fleet.Activity, _fleet.ActivityDuration, _fleet.ActivityStart);
                //}


                if (_fleet.Route.Steps.Count() > 0 && shipNum > 1)
                {
                    //GameLog.Client.AIDetails.DebugFormat("Step_6002: # {0} ships inside _fleet, {1} Waypoints to go, first step = {2}", shipNum, _fleet.Route.Waypoints.Count, _fleet.Route.Steps[0]);
                    for (int i = 0; i < shipNum; i++)
                    {
                        _text = "Step_6005:; # doubled # Fleet# Ship " + i;
                        Ship ship = _fleet.Ships[i];
                        _text += " = " + ship.ObjectID + " " + ship.Name + " - " + ship.Design.Key;
                        //if (_writeDirectly) Console.WriteLine(_text);
                        //GameLog.Core.AIDetails.DebugFormat(_text);
                    }
                }

                //If the _fleet is stranded and out of fuel range, it can't move
                if (_fleet.IsStranded && !_fleet.IsFleetInFuelRange())
                {
                    if (_fleet.IsRouteLocked)
                    {
                        _fleet.UnlockRoute();
                    }

                    _fleet.SetRoute(TravelRoute.Empty);

                    if (_fleet.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    if (_civM.StrandedShipsSector.Location.ToString() == _civM.HomeSystem.Sector.Location.ToString()
                        && _civM.StrandedShipsSector.Location.ToString() != _fleet.Sector.Location.ToString())
                    {
                        _civM.StrandedShipsSector = _fleet.Sector;
                        _text = GameEngine.LocationString(_fleet.Location.ToString())
                            + " > a stranded ship was reported"  // that's a cheat > only StrandedShipsSector is reported, not every sector/ship
                            ;
                        _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, "", "", SitRepPriority.Pink));
                    }

                    //if (_fleet.HasConstructionShip)
                    //{
                    //    _fleet.Order = FleetOrders.BuildStationOrder;
                    //}
                }

                //if (_fleet.AITypeUnit != AITypeUnit.SystemDefense) // && _fleet.AITypeUnit == AITypeUnit.Attack)
                //{
                //    //GameLog.Client.AIDetails.DebugFormat("*** Turn {0}: {2} {1} >5 and Not SystemDefence, Unit: AIType={3}, Activity={4}, Duration={5}, Start ={6}",
                //    //    GameContext.Current.TurnNumber, _fleet.Owner.Name, _fleet.Location, _fleet.AITypeUnit, _fleet.Activity, _fleet.ActivityDuration, _fleet.ActivityStart);

                //    _fleet.Activity = UnitActivity.Mission;
                //    //_fleet.AITypeUnit = AITypeUnit.Explorer;
                //}




                //int fuelNeeded;
                int fuelRange = _civM.MapData.GetFuelRange(_fleet.Location);

                /*
                 * If the _fleet is within fueling range, then try to top off the reserves of
                 * each ship in the _fleet.  We do this now in case a ship is out of fuel, but
                 * is now within fueling range, thus ensuring the ship will be able to move.
                 */
                if (!_fleet.IsInTow && (_fleet.Range >= fuelRange))
                {
                    foreach (Ship ship in _fleet.Ships)
                    {
                        fuelNeeded = ship.FuelReserve.Maximum - ship.FuelReserve.CurrentValue;

                        if (fuelNeeded > 0)
                        {
                            _ = ship.FuelReserve.AdjustCurrent(
                                _civM.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));
                            // Deuterium is for fuel into ships
                            _text = "Step_6006:; " + LocationString(ship.Location.ToString()) + " > Deuterium filled in: " + fuelNeeded

                                + " ;for; " + ship.ObjectID
                                + " ; " + ship.Name
                                + " ;for; " + ship.Design

                                ;
                            if (_writeDirectly) Console.WriteLine(_text);
                        }
                    }
                }

                //Move the ships along their route
                for (int i = 0; i < _fleet.Speed; i++)
                {
                    if (_fleet.MoveAlongRoute())
                    {
                        _fleet.AdjustCrewExperience(5);
                    }
                    else
                    {
                        if (i == 0)
                        {
                            _fleet.AdjustCrewExperience(1);
                        }

                        fuelNeeded = _fleet.Ships.Count;

                        _ = _civM.Resources[ResourceType.Deuterium].AdjustCurrent(fuelNeeded);
                        _civM.Resources[ResourceType.Deuterium].UpdateAndReset();
                        // Deuterium is for fuel into ships
                        //_text = "Fleet: Deuterium filled in: " + fuelNeeded

                        //    + ";for;" + _fleet.ObjectID
                        //    + "; " + _fleet.Name
                        //    + ";for;" + _fleet.Ships.Count

                        //    ;
                        //if (_writeDirectly) Console.WriteLine(_text);


                        //Destroy ships due to financial problems
                        if (_civM.DestroyOfShipOrdered == false && GameContext.Current.TurnNumber > 9)
                            if (_civM.MaintenanceCostLastTurn > _civM.TaxIncome * 5
                            || _civM.Credits.CurrentValue + 1000 < (100 * _civM.AverageTechLevel))
                            {
                                if (_fleet.Owner.Key == "BORG") { continue; }

                                Ship ship = _fleet.Ships[0];
                                //ship.Destroy();
                                _civM.DestroyOfShipOrdered = true;

                                string _objectIDText = ship.ObjectID.ToString() + " "; if (_objectIDText == "-1") _objectIDText = "";


                                _text2 = /*_objectIDText + blank*/ "* " + ship.Name + "* ( " + ship.ShipType + " ) ";
                                // {0} > Ship {1} was destroyed for keeping credit costs low.
                                _text = string.Format(ResourceManager.GetString("SITREP_SHIP_DESTROYED_DUE_TO_LOW_CREDITS"), GameEngine.LocationString(_fleet.Location.ToString()), _text2);
                                //_text = "Empty? " + _text;
                                Console.WriteLine("Step_4118:; Turn " + GameContext.Current.TurnNumber + " " + _text);
                                //GameLog.Client.ShipsDetails.DebugFormat("shipDestroyed {0} Ship(s) went down a Black hole {1} {2}", shipsDestroyed, _fleet.Owner.Key, _fleet.Location);

                                _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, "", "", SitRepPriority.RedYellow));
                            }
                        //checked for maintenance cost especially for destroyed ships
                        //        destroy just one ship per turnnumber

                        //break;
                    }

                    fuelRange = _civM.MapData.GetFuelRange(_fleet.Location);

                    foreach (Ship ship in _fleet.Ships)
                    {
                        /*
                         * For each ship in the _fleet, deplete the fuel reserves by a 1 unit
                         * of Deuterium.  Then, if the _fleet is within fueling range, attempt
                         * to replenish that unit from the global stockpile.
                         */
                        fuelNeeded = ship.FuelReserve.AdjustCurrent(-1);  // old: 1 

                        //testing deuterium

                        if (_fleet.Range >= fuelRange)
                        {
                            _ = ship.FuelReserve.AdjustCurrent(
                                _civM.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));
                            ship.FuelReserve.UpdateAndReset();

                            //_text = "Step_3037:; _civM_1 Deuterium CurrentValue= "
                            //    + _civM_1.Resources[ResourceType.Deuterium].CurrentValue
                            //    + " for " + _civM_1.Civilization.Key
                            //    ;
                            //if (boolCheckDeuterium)
                            //    if (_writeDirectly) Console.WriteLine(_text);
                            //GameLog.Client.General.InfoFormat(_text);
                        }

                    }
                    _civM.Resources[ResourceType.Deuterium].UpdateAndReset();

                    bool boolCheckDeuterium = false;
                    if (boolCheckDeuterium)
                    {
                        _text = "Step_3038:; _civM_1 Deuterium CurrentValue= "
                            + _civM.Resources[ResourceType.Deuterium].CurrentValue
                            ;
                        if (_writeDirectly) Console.WriteLine(_text);
                        //GameLog.Client.General.InfoFormat(_text);
                    }

                }

                // Blackhole
                if (_fleet.Sector.System != null && (_fleet.Sector.System.StarType == StarType.BlackHole))
                {
                    int shipsDamaged = 0;
                    int shipsDestroyed = 0;

                    if (_fleet.Ships != null) // Update FixBlackholeCrash (hopefully) 2 March 2019
                    {

                        foreach (Ship ship in _fleet.Ships)
                        {
                            int damage = RandomHelper.Roll(ship.HullStrength.CurrentValue);
                            if (damage >= ship.HullStrength.CurrentValue)
                            {
                                _text = _fleet.Location
                                    + " > Ship " + ship.ObjectID
                                    + " * " + ship.Name
                                    + " * ( " + ship.DesignName
                                    + " ) got destroyed by a Black Hole"
                                    ;
                                Console.WriteLine(_text);
                                _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, _text, "", SitRepPriority.RedYellow));
                                shipsDestroyed++;
                                ship.Destroy();

                            }
                            else
                            {
                                shipsDamaged++;
                                _ = ship.HullStrength.AdjustCurrent(-damage);

                                _text = _fleet.Location
                                        + " > Ship " + ship.ObjectID
                                        + " * " + ship.Name
                                        + " * ( " + ship.DesignName
                                        + " ) > got damaged by a Black Hole"
                                        + " > Hull at " + ship.HullStrength.CurrentValue + "%"
                                        ;
                                Console.WriteLine(_text);
                                _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, _text, "", SitRepPriority.Yellow));


                            }

                            if (_fleet.Ships != null) // 2nd try to fix blackhole 3 march 2019
                            {
                                break;
                            }
                        }

                    }


                    if ((shipsDamaged > 0) || (shipsDestroyed > 0))
                    {
                        _text = string.Format(ResourceManager.GetString("SITREP_BLACK_HOLE_ENCOUNTER"), _fleet.Location, shipsDestroyed, shipsDamaged);

                        //GameLog.Client.ShipsDetails.DebugFormat("shipDestroyed {0} Ship(s) went down a Black hole {1} {2}", shipsDestroyed, _fleet.Owner.Key, _fleet.Location);
                        //if (_writeDirectly) Console.WriteLine(_text);
                        _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, _text, "", SitRepPriority.Blue));
                    }
                } // End of Black Hole

                //if (_fleet.Route != null && _fleet.Location == _fleet.Route.Waypoints[_fleet.Route.Length])

                if (_fleet.Owner.IsHuman && _fleet.Route != null && _fleetAim != "None" && _fleet.Route.Length < 1)
                {
                    _text = GameEngine.LocationString(_fleet.Location.ToString())
                        + " > " + _fleet.Ships[0].Design
                        + " > "
                        //+ _fleet.ObjectID
                        //+ ":  " 
                        + _fleet.Name
                        + " > aim location is reached - please check for further orders"
                        //+ " ( old order = " + _fleet.Order + " ) "
                        ;
                    //FleetOrder _oldOrder = _fleet.Order;
                    _fleet.Order = FleetOrders.IdleOrder;

                    //GameLog.Client.ShipsDetails.DebugFormat("shipDestroyed {0} Ship(s) went down a Black hole {1} {2}", shipsDestroyed, _fleet.Owner.Key, _fleet.Location);

                    _civM.SitRepEntries.Add(new ReportEntry_CoS(_fleet.Owner, _fleet.Location, _text, _text, "", SitRepPriority.Green));
                    //Console.WriteLine("Step_6007:; Turn " + turnnumber + " > " + _text);
                }

            }
            _allFleets_report += _text + _newline;
            Console.WriteLine(_newline + "Step_6016:; ## _allFleets_report " + _newline + _allFleets_report + "End of _allFleets_report, Count = " + _allFleets.Count + _newline);

            // please do not include into the next
            foreach (Fleet _fleet in _allFleets)
            {
                _fleet.Order?.OnTurnBeginning();

                SitReps_for_Fleets(_fleet);


            }

            HashSet<Station> allStations = GameContext.Current.Universe.Find<Station>(UniverseObjectType.Station);
            foreach (Station station in allStations)
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[station.OwnerID];

                _text = GameEngine.LocationString(station.Location.ToString()) + " > Station " + station.ObjectID
                    + ": " + station.Design
                    + " ___ - Maint. " + station.Design.MaintenanceCost
                    + " > * " /*+ station.ObjectID + " " */+ station.Name
                    + " *"  /*( Maint." + station.Design.MaintenanceCost + " )"*/
                    + " > since Turn " + station.TurnCreated /*+ " )"*/
                    ;
                Console.WriteLine("Step_3482:; " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

                civManager.SitRepEntries.Add(new ReportEntry_CoS(civManager.Civilization, station.Location, _text, "", ""
                    , SitRepPriority.Gray));
            }
            _text = "End of Do_12_FleetHandling";
        }
        #endregion

        #region DoDiplomacy() Method
        private void Do_13_Diplomacy()
        {
            string _text;


            // FIRST: Pending Actions
            foreach (Civilization _civ1 in GameContext.Current.Civilizations)
            {
                _text = "Step_1351:; Do_13_Diplomacy for > " + _civ1;
                //Console.WriteLine(_text);

                string _diploStatusText = "";
                _diploStatusText += " " + _diploStatusText; // dummy - please keep
                string _newline = Environment.NewLine;

                CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];
                Diplomat _diplomatCiv1 = Diplomat.Get(_civ1);
                _civM_1.Assault_Value_Defense_and_Distance = 999993;

                Report_SomeSectors(_civ1, _civM_1); // reports for _civ1: Sectors for Accumulate, SystemAttack, TargetCiv

                if (!_civ1.IsHuman)
                {
                    DiplomacyHelper.AcceptingRejecting(_civ1);
                }

                Diplomacy_1_Basics(_civ1, _civM_1);  // e.g. Status = NoContact, War etc...




                // Second: Schedule delivery of outbound messages  Including Statementreceived
                _text = "Step_3091:; NEXT: *Second* Outgoing";
                //if (_combatWriteDirectly)
                //{
                //    //Console.WriteLine(_text);
                //}
                //GameLog.Core.DiplomacyDetails.DebugFormat(_text);
                //foreach (Civilization _civ_1 in GameContext.Current.Civilizations)
                //{
                //Diplomat _diplomatCiv1 = Diplomat.Get(_civ1);
                //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ_1];

                foreach (Civilization _civ2 in GameContext.Current.Civilizations)
                {
                    if (_civ1 == _civ2) { continue; }

                    ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(_civ2);
                    CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

                    Diplomacy_9_ConsoleWriteline(_civ1, _civ2);

                    if (_diplomatCiv2.StatementReceived != null)//  Second.1 = StatementReceived
                    {
                        Diplomacy_Statement_Received(_civ1, _civ2);
                    }

                    Diplomacy_Proposal_Sent(_civ1, _civ2);//  Second.2 = proposalSent

                    Diplomacy_Statement_Sent(_civ1, _civ2);//  Second.3 = statementSent

                    Diplomacy_Response_Sent(_civ1, _civ2);//  Second.4 = responseSent

                    //_civM_1.TargetCivList.Add(_civ2);
                }
                //}


                // Third: Fulfill agreement obligations
                foreach (IAgreement agreement in GameContext.Current.AgreementMatrix) { AgreementFulfillmentVisitor.Visit(agreement); }


                //_civM_1.TargetCivList.Add(_civ2);

                //if (_civM_1.Assault_TargetCiv == null)
                //{

                if (_civ1.IsHuman)
                {
                    //Debugger.Break();
                }

                if (_civM_1.Assault_Location != null && _civM_1.Assault_Accumulate_Location_1.ToString() != "(0, 0)")
                {
                    Civilization _civ2 = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID].Civilization;
                    //_text += "SystemAssault Location 1 = " + _civM_1.Assault_Accumulate_Location_1 + ", ";
                    _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1
                        , "Assault_Location= " + _civM_1.Assault_Location, "", "", SitRepPriority.Purple));
                    // no center on Target
                    //_civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID].HomeSystem.Location
                    //    , "SystemAssault TargetCiv = " + _civM_1.Assault_TargetCiv, "", "", SitRepPriority.Purple));

                    //Diplomat _foreignPowerCiv2 = Diplomat.Get(_civ2);
                    ForeignPower _foreignPowerCiv2 = _diplomatCiv1.GetForeignPower(_civ2);
                    ForeignPowerStatus _foreignPowerStatus = _diplomatCiv1.GetForeignPower(_civ2).DiplomacyData.Status;

                    if (_foreignPowerStatus != ForeignPowerStatus.AtWar)
                    {
                        _foreignPowerCiv2.DeclareWar();
                        CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID];
                        UnitAI.GetBestSystemFor_AccumulateFor_SystemAttack(_civM_2.HomeSystem.Sector, _civM_1.HomeSystem.Sector, out Sector _sector);
                        _civM_1.Assault_Accumulate_Sector_1 = _sector;

                        string _systemAssaultSector_Text = "Assault_Accumulate_Sector_1 is set to Sector " + _sector.ToString();
                        _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _sector.Location, _systemAssaultSector_Text, "", "", SitRepPriority.Purple));

                        string _systemAssaultLocation_Text = "Assault_Accumulate_Location_1 is set to Location " + _sector.Location.ToString();
                        _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _sector.Location, _systemAssaultLocation_Text, "", "", SitRepPriority.Purple));


                        int _defense = 10; // 

                        if (_civM_2.HomeSystem.Sector.Station != null)
                        {
                            _defense += _civM_2.HomeSystem.Sector.Station.Fire_Power_Orbital / 200;  // station only half
                            _defense += _civM_2.HomeSystem.Colony.Population.CurrentValue;  // 
                            if (_civM_2.HomeSystem.Colony.OrbitalBatteries.Count > 0)
                            {
                                _defense += _civM_2.HomeSystem.Colony.OrbitalBatteries.Count
                                                 * (_civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                                                 * _civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                                                 + (_civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                                                 * _civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage)

                                                 ;  // 


                            }
                            _civM_1.Assault_DefenseValue = _defense;
                        }

                    }


                }
                else
                {
                    if (_civM_1.Assault_TargetCiv != null)
                    {
                        string _targetCivtext = "Assault_TargetCiv is set to= " + _civM_1.Assault_TargetCiv.Name
                            + " at " + _civM_1.HomeSystem.Location
                            ;
                        _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _targetCivtext, "", "", SitRepPriority.Gray));

                        if (_civM_1.Assault_Accumulate_Sector_1 != null)
                        {
                            CivilizationManager _civM_temp = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv];
                            UnitAI.GetBestSystemFor_AccumulateFor_SystemAttack(_civM_temp.HomeSystem.Sector, _civM_1.HomeSystem.Sector, out Sector _sector);
                            _civM_1.Assault_Accumulate_Sector_1 = _sector;
                            string _systemAssaultSector_Text = "Assault_Accumulate_Sector_1 is set to Sector " + _sector.ToString();
                            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _systemAssaultSector_Text, "", "", SitRepPriority.Purple));

                            string _systemAssaultLocation_Text = "Assault_Accumulate_Location_1 is set to Location " + _sector.Location.ToString();
                            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _systemAssaultLocation_Text, "", "", SitRepPriority.Purple));
                        }
                        else
                        {

                            string _noTargetCivtext = "Not set: Assault_Accumulate_Location_1";
                            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _noTargetCivtext, "", "", SitRepPriority.Gray));

                        }
                    }
                } // end of else


                //if (_writeDirectly) Console.WriteLine("Step_7727:; _diplomacyBasicsSummary_Text="
                //    + _diplomacyBasicsSummary_Text + _newline
                //    + "End of _diplomacyBasicsSummary_Text" + _newline

                //    );

                //_diplomacyBasicsSummary_Text = "";

                if (_civM_1.Assault_TargetCiv != null)
                {
                    _text = "Step_7729:; Assault_TargetCiv= " + _civM_1.Assault_TargetCiv.Name
                            + " at " + _civM_1.HomeSystem.Location
                            + " for " + _civM_1.Civilization.Key
                            ;
                    //if (_writeDirectly)
                    Console.WriteLine(_text);
                }



            } // End of foreach civ1
        } // End of Do_13_Diplo

        private void Diplomacy_1_Basics(Civilization _civ1, CivilizationManager _civM_1)
        {
            string _text;
            string _diplomacyBasicsSummary_Text = "";
            string _newline = Environment.NewLine;
            int _targetDistance = 99;
            _civM_1.Assault_Value_Defense_and_Distance = 9999997;
            bool _writeDirectly = true;

            //Dictionary<Civilization, int> _possibleTargetCivs = new Dictionary<Civilization, int >(); // for Assault or better SystemAssault
            List<Civilization> _possibleTargetCivs = new List<Civilization>(); // for Assault or better SystemAssault

            foreach (Civilization _civ2 in GameContext.Current.Civilizations)
            {
                #region DiplomacyBasics
                if (_civ1 == _civ2)
                {
                    continue;
                }

                Diplomat _diplomat1 = Diplomat.Get(_civ1);
                //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];

                ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
                CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

                ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

                if (_foreignPowerStatus == ForeignPowerStatus.NoContact)
                {
                    continue;
                }

                //if (_foreignPowerStatus != ForeignPowerStatus.NoContact)
                //{
                _text = _newline + _newline + "Step_7731:; Do_13_Diplomacy >>>>>>>>>>>>>>>>>>>>>>>>>> " + _civ1
                    + " ; Status > " + _foreignPowerStatus
                    + " ; to ; " + _civ2

                    ;
                if (_writeDirectly)
                    Console.WriteLine(_text);
                _diplomacyBasicsSummary_Text += _newline + _text;


                int _regard = _diplomatForeignPower_Civ2.DiplomacyData.Regard.CurrentValue;
                int _trust = _diplomatForeignPower_Civ2.DiplomacyData.Trust.CurrentValue;


                // Find Assault_TargetCiv
                if (_civ1.IsHuman)
                {
                    //Debugger.Break();
                }

                _possibleTargetCivs.Add(_civ2);
                _possibleTargetCivs = _possibleTargetCivs.Distinct().ToList();


                //_text = "Step_7742:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 
                //    + "; > Regard =;" + _regard + "; > Trust =;" + _trust;
                //if (_writeDirectly) Console.WriteLine(_text);
                //////_text = "Step_7744:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Trust =;" + _trust;
                //////if (_writeDirectly) Console.WriteLine(_text);

                if (_foreignPowerStatus == ForeignPowerStatus.Affiliated)
                {
                    //_text = "Step_7750:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Affiliated";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    if (_regard < 850)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3); // 2 each turnnumber
                    if (_trust < 800)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 4);
                    //_text = 
                }

                if (_foreignPowerStatus == ForeignPowerStatus.Allied)
                {
                    //_text = "Step_7760:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Allied";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    if (_regard < 850)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 800)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

                }

                if (_foreignPowerStatus == ForeignPowerStatus.Friendly)  // Open Borders
                {
                    //_text = "Step_7770:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Friendly";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    if (_regard < 650)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);

                }


                if (_foreignPowerStatus == ForeignPowerStatus.Peace)
                {
                    //_text = "Step_7780:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Peace";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    //if (_regard < 850)
                    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

                }

                if (_foreignPowerStatus == ForeignPowerStatus.Neutral)
                {
                    //_text = "Step_7710:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Neutral";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    if (_regard < 650)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);

                }

                //Debugger.Break()
                //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }


                //AtWar
                if (_foreignPowerStatus == ForeignPowerStatus.AtWar)
                {
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    _text = "Step_7732:; Do_13_Diplomacy > " + _civ1 + "; vs ; " + _civ2 + "; > AtWar";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _diplomacyBasicsSummary_Text += _newline + _text;

                    //List<Civilization> _possibleTargetCivs = new List<Civilization>();


                    //// Find Assault_TargetCiv
                    //if (_civ1.IsHuman)
                    //{
                    //    //Debugger.Break();
                    //}

                    //_possibleTargetCivs.Add(_civ2);
                    //_possibleTargetCivs.Distinct();

                }

                // Find new TargetCiv
                List<Civilization> _target_help_list = new List<Civilization>() { _civ1 };

                if (_civ1.IsHuman)
                {
                    //Debugger.Break(); // see below
                }
                //_target_help_list.Add(_civ1);
                if (_civM_1.TargetCivList == null
                    || _civM_1.TargetCivList.Count == 0)
                {
                    _civM_1.TargetCivList = _target_help_list;
                }
                else
                {
                    _civM_1.TargetCivList.AddRange(_possibleTargetCivs);
                    _target_help_list = _civM_1.TargetCivList;
                    //_target_help_list = _target_help_list;
                    _civM_1.TargetCivList = _target_help_list.Distinct().ToList();

                    //Debugger.Break()
                    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

                }


                //if (_possibleTargetCivs.Count > 0)
                //{
                //    //_civM_1.TargetCivList = _possibleTargetCivs;
                //    _civM_1.TargetCivList.AddRange(_possibleTargetCivs);
                //}


                //_civM_1.TargetList_Update(_target_help_list);
                //_civM_1.TargetList_Update(_civ1);

                //Array<Civilization, int distance, int defense_value> _targetColoniesLocations = new Array<Civilization, int, int>(); // find out the nearest one

                //Dictionary<int, int> ColonyTargetValues = new Dictionary<int, int>(); // find out the nearest one
                //ColonyTargetValues.Add(99, 999999);  // avoid an empty Dictionary

                Dictionary<MapLocation, ColonyTargetValues> _targetColoniesLocations = new Dictionary<MapLocation, ColonyTargetValues>(); // find out the nearest one
                //var _colonyTargetValues = new ColonyTargetValues(99, 999999);
                //_targetColoniesLocations.Add(_civM_1.HomeColony, 99 ,999999);  // avoid an empty Dictionary

                _targetColoniesLocations.Add(_civM_1.HomeColony.Location, new ColonyTargetValues(99, 999999));
                //_targetColoniesLocations[colonyB] = new ColonyTargetValues(200, 60);
                int _lowest_targetDistance = 99;

                MapLocation _new_assault_location = new MapLocation();
                _civM_1.Assault_Value_Defense_and_Distance = 999996;
                int _target_fire_power = 0;
                int _last_target_fire_power = 999995;
                int _new_target_fire_power = 999994;
                string _all_attack_location_text = "";
                int _next_target_fire_power = 999991;
                int _lowest_defense_value = 999991;
                //int _targetDistance = 99;

                if (_civ1.IsHuman)
                {
                    //Debugger.Break();
                }

                if (_civM_1.TargetCivList != null && _civM_1.TargetCivList.Count > 0)
                {
                    foreach (var _item in _civM_1.TargetCivList)
                    {
                        MapLocation _loc_1 = _civM_1.HomeSystem.Location;

                        var _civ2Colonies = _civM_2.Colonies.ToList();
                        //_lowest_targetDistance = 99;

                        foreach (var _colony in _civ2Colonies)
                        {
                            MapLocation _loc_2 = _colony.Location;
                            int _distance = (int)Math.Sqrt((int)Math.Pow(_loc_1.X - _loc_2.X, 2)
                                + (int)Math.Pow(_loc_1.Y - _loc_2.Y, 2));
                            int _defense_value = Colony.DefenseValue(_colony); // minimum defense value
                            //_defense_value = UnitAI.
                            if (_loc_2 == _loc_1)
                            {
                                _distance += 50;
                            }
                            if (!_targetColoniesLocations.ContainsKey(_colony.Location))
                            {
                                if (GameContext.Current.CivilizationManagers[_item.CivID].SeatOfGovernment != null)  // subjageted
                                {
                                    _targetColoniesLocations.Add(_colony.Location, new ColonyTargetValues(_distance, _defense_value));
                                }
                                //_targetColoniesLocations.Add(_civM_1.HomeColony.Location, new ColonyTargetValues(99, 999999));
                            }



                        }
                        //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }
                    }

                    if (_civ1.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    //int _minValue = 98;


                    //MapLocation _new_assault_location = new MapLocation();


                    foreach (var item in _targetColoniesLocations)
                    {
                        //if (item.Value < _minValue)
                        //{
                        //    _minValue = item.Value;

                        //string _distance_text = "";


                        if (_regard < 600)
                        {
                            //_targetDistance = MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location);

                            //if (_targetDistance < MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location))
                            //{
                            _targetDistance = MapLocation.GetDistance(_civM_1.HomeSystem.Location, item.Key);
                            //_civM_1.Assault_TargetCiv = GameContext.Current.CivilizationManagers[item.Key].Civilization;

                            if (_targetDistance > 0 && _targetDistance < _lowest_targetDistance)
                            {
                                _text = "Step_7720:; Do_13_Diplomacy > "
                                        /*+ "_targetDistance= "*/ + _civM_1.Civilization.Key
                                        + " > _targetDistance= " + _targetDistance
                                        + ", _lowest_targetDistance= " + _lowest_targetDistance
                                            ;
                                //if (_writeDirectly)
                                //Console.WriteLine(_text);
                                //_all_attack_location_text += _newline + _text;

                                if (_civM_1.Civilization.IsHuman)
                                {
                                    //Debugger.Break();
                                }

                            }


                            if (_targetDistance > 0 && _targetDistance < _lowest_targetDistance)
                            {
                                _lowest_targetDistance = _targetDistance;
                                LocationFirePower(item.Key, out _target_fire_power);

                                List<Colony> _colony_there = GameContext.Current.Universe.Find<Colony>()//(_civ).ToList()
                                                            .Where(a => a.Location.ToString() == item.ToString())
                                                            .ToList()
                                                            ;

                                Colony _colony_local = null;

                                //Debugger.Break();

                                //try
                                //{
                                if (_colony_there.Count > 0)
                                {
                                    _colony_local = _colony_there[0];
                                }

                                //}
                                //catch
                                //{
                                //    Debugger.Break(); // no _colony is this sector > just a station ?
                                //}


                                if (_colony_local != null)
                                {
                                    //_target_fire_power += _colony_there[0].Population.CurrentValue;
                                    _target_fire_power += Colony.DefenseValue(_colony_local);
                                }

                                _next_target_fire_power = _target_fire_power + ((_targetDistance * _targetDistance) * 100);

                                _text = "Step_7726:; Do_13_Diplomacy > "
                                        + "_civM_1.Assault_Location"

                                        + " for >>> "
                                        + _civ1 + " at " + GameEngine.LocationString(_civM_1.HomeSystem.Location.ToString())

                                        + " possible  > "
                                        + " Colony= " + GameEngine.LocationString(_new_assault_location.ToString())
                                        + " "
                                        + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                                        + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _target_fire_power.ToString())

                                        //+ "   ; _regard= " + _regard
                                        + "   ; Attack= " + _civM_1.Assault_Attack_Value
                                        + "   ; Distance= " + GameEngine.Do_x_Digit_String(2, _targetDistance.ToString())
                                        + "   ; _next_target_fire_power= " + GameEngine.Do_x_Digit_String(5, _next_target_fire_power.ToString())
                                        + "   ; AssVal_Defense+Dist= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_Value_Defense_and_Distance.ToString())
                                            //+ "XXXXX >"
                                            //+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                                            //+ " for "
                                            //+ _civ1 + " at " + LocationString(_civM_1.HomeSystem.Location.ToString()) + "   ; vs ; "
                                            //+ _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/
                                            ////+ "   ; _regard= " + _regard
                                            //+ "; Colony= " + item.Key
                                            //+ "; _target_fire_power= " + GameEngine.Do_x_Digit_String( 5, (_target_fire_power.ToString())
                                            ////+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                                            ////+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                                            ;
                                //if (_writeDirectly)
                                //Console.WriteLine(_text);
                                _all_attack_location_text += _newline + _text;
                                //_distance_text += /*_newline +*/ _text;


                            }



                            //Console.WriteLine(_all_attack_location_text + " > from Step_7722");

                            //if (_civ1.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}
                            ////}


                            //Console.WriteLine(_all_attack_location_text + " > from Step_7723");

                            //if (_civ1.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}


                            _next_target_fire_power = _target_fire_power + ((_targetDistance + 1) * 100); // gives a 100 basic value

                            if (_next_target_fire_power < _last_target_fire_power)
                            {
                                _new_assault_location = item.Key;
                                _lowest_defense_value = _next_target_fire_power;

                                _last_target_fire_power = _lowest_defense_value;
                            }
                        }


                    }

                    //Console.WriteLine(_all_attack_location_text + " > from Step_7724"); // see below

                    if (_civ1.IsHuman)
                    {
                        //Debugger.Break();
                    }

                }


                if (/*_civM_1.Assault_Location != _new_assault_location && */_new_target_fire_power < _civM_1.Assault_Value_Defense_and_Distance)
                {


                    _text = "Step_7717:; Do_13_Diplomacy > "
                            + "_civM_1.Assault_Location"

                            + " for >>> "
                            + _civ1 + " at " + LocationString(_civM_1.HomeSystem.Location.ToString())

                            + " possible  > "
                            + " Colony= " + GameEngine.LocationString(_new_assault_location.ToString())
                            + " "
                            + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                            + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_DefenseValue.ToString())

                            //+ "   ; _regard= " + _regard
                            + "   ; Attack= " + _civM_1.Assault_Attack_Value //GameEngine.Do_x_Digit_String(5, _civM_1.Assault_AttackValue.ToString())
                                                                             //+ "  ; Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())

                                //+ "_civM_1.Assault_Location possible  >"
                                ////+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                                //+ " for "
                                //+ _civ1 + " at " + LocationString(_civM_1.HomeSystem.Location.ToString()) + "   ; vs ; "
                                //+ _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/
                                //+ " for Colony= " + _new_assault_location
                                ////+ "   ; _regard= " + _regard

                                //+ "; _target_fire_power= " + _target_fire_power
                                //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                                //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                                ;
                    //if (_writeDirectly) 
                    //Console.WriteLine(_text);
                    //_all_attack_location_text += _newline + _text;


                    _civM_1.Assault_Location = _new_assault_location;
                    _civM_1.Assault_Value_Defense_and_Distance = _new_target_fire_power;

                    _last_target_fire_power = _new_target_fire_power;

                    if (_civ1.IsHuman)
                    {
                        //Debugger.Break();
                    }

                }


                //Console.WriteLine(_all_attack_location_text + "        > from Step_7724");


                //if (_writeDirectly)
                //{
                //    Console.WriteLine(_text + "            > from Step_7727");
                //}

                if (_civ1.IsHuman)
                {
                    //Debugger.Break();
                }

                //if (_writeDirectly) Console.WriteLine("Step_7727:; _diplomacyBasicsSummary_Text="
                //    + _diplomacyBasicsSummary_Text + _newline
                //    + "End of _diplomacyBasicsSummary_Text" + _newline

                //    );
                //_diplomacyBasicsSummary_Text = "";

                string _atWarText = "";

                if (_foreignPowerStatus == ForeignPowerStatus.AtWar
                        && _civM_1.TargetCivList != null
                        && _civM_1.TargetCivList.Count > 0)
                {

                    //_targetColoniesLocations = new Dictionary<Civilization, int>(); // find out the nearest one
                    //_targetColoniesLocations.Add(_civ1, 99);  // avoid an empty Dictionary


                    //foreach (var _item in _civM_1.TargetCivList)
                    //{
                    //    MapLocation _loc_1 = _civM_1.HomeSystem.Location;
                    //    MapLocation _loc_2 = GameContext.Current.CivilizationManagers[_item.CivID].HomeSystem.Location;
                    //    int _distance = (int)Math.Sqrt((int)Math.Pow(_loc_1.X - _loc_2.X, 2)
                    //        + (int)Math.Pow(_loc_1.Y - _loc_2.Y, 2));
                    //    if (_loc_2 == _loc_1)
                    //    {
                    //        _distance += 50;
                    //    }
                    //    if (!_targetColoniesLocations.ContainsKey(_item))
                    //    {
                    //        if (GameContext.Current.CivilizationManagers[_item.CivID].SeatOfGovernment != null)  // subjageted
                    //        {
                    //            _targetColoniesLocations.Add(_item, _distance);
                    //        }

                    //    }
                    //    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }
                    //}

                    //Debugger.Break()
                    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

                    //var _nearest_target = _targetColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

                    //_civM_1.Assault_TargetCiv = _nearest_target;
                    //_text = "Step_7737:; Do_13_Diplomacy > "
                    //    + "AtWar > nearest Target: "
                    //    + _civ1 + " at " + _civM_1.HomeSystem.Location + "; vs ; "
                    //    + _civ2 + " at " + _civM_2.HomeSystem.Location
                    //    //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location
                    //    + ", Distance= " + MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location)
                    //    //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                    //    ;
                    //if (_writeDirectly) Console.WriteLine(_text);



                    if (_civ1.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    //int _minValue = 98;
                    //foreach (var item in _targetColoniesLocations)
                    //{
                    //    if (item.Value < _minValue)
                    //    {
                    //        _minValue = item.Value;
                    //        _civM_1.Assault_TargetCiv = item.Key;

                    //        _text = "Step_7736:; Do_13_Diplomacy > "
                    //                    + "AtWar >"
                    //                    + " Distance= " + GameEngine.Do_x_Digit_String(MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location).ToString())
                    //                    + " for "
                    //                    + _civ1 + " at " + _civM_1.HomeSystem.Location + "    ; vs ; "
                    //                    + _civ2 + " at " + _civM_2.HomeSystem.Location

                    //                    + "; Assault_TargetCiv= " + _civM_1.Assault_TargetCiv
                    //                    //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                    //                    //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                    //                    ;
                    //        if (_writeDirectly) Console.WriteLine(_text);
                    //        _diplomacyBasicsSummary_Text += _newline + _text;

                    //        //if (_civ1.IsHuman) { Debugger.Break(); }

                    //    }
                    //}


                    _text = "Step_7737:; Do_13_Diplomacy > "

                            + _civ1 + " at " + _civM_1.HomeSystem.Location /*+ "; vs ; "*/
                            //+ " nearest Target: "
                            + "; AtWar > Distance= * " + GameEngine.Do_x_Digit_String(2, MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location).ToString())
                            + " * for " + _civ2 + " at " + _civM_2.HomeSystem.Location
                            + " <<<<<<<<<<<<<<<<<<"
                            ;
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _diplomacyBasicsSummary_Text += _newline + _text;
                    _atWarText = _text;

                    if (_civ1.IsHuman)
                    {
                        //Debugger.Break(); 
                    }


                    //Console.WriteLine(_newline + "Step_7734:; Begin of _diplomacyBasicsSummary_Text" + /*_newline + */_diplomacyBasicsSummary_Text + _newline + "End of _diplomacyBasicsSummary_Text" + _newline);

                    //if (_civ1.IsHuman) { Debugger.Break(); }

                    if (_civ1.IsHuman
                        //&& _foreignPowerStatus != ForeignPowerStatus.NoContact
                        && _foreignPowerStatus != ForeignPowerStatus.AtWar)
                    {
                        if (_atWarText == "") _atWarText = "Step_7738:; Do_13_Diplomacy > with nobody for " + _civ1;
                        if (_writeDirectly) Console.WriteLine(_atWarText);
                        //Debugger.Break();
                    }
                }


                Console.WriteLine(_all_attack_location_text + "        > from Step_7724 = _all_attack_location_text");



                _text = "Step_7718:; Do_13_Diplomacy > "
                            + "_civM_1.Assault_Location"
                            //+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                            + " for >>> "
                            + _civ1 + " at " + GameEngine.LocationString(_civM_1.HomeSystem.Location.ToString())

                            + " possible  > "
                            + " Colony= " + GameEngine.LocationString(_new_assault_location.ToString())
                            + " "
                            + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                            + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_DefenseValue.ToString())

                            ////+ "   ; _regard= " + _regard
                            + "   ; Attack= " + _civM_1.Assault_Attack_Value //GameEngine.Do_x_Digit_String(5, _civM_1.Assault_AttackValue.ToString())
                            + "   ; Distance= " + GameEngine.Do_x_Digit_String(2, MapLocation.GetDistance(_civM_1.HomeSystem.Location, _new_assault_location).ToString())

                            //+ "; _target_fire_power= " + _target_fire_power
                            //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                            //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                            ;
                //if (_writeDirectly) 
                //Console.WriteLine(_text);


                if (_civ1.IsHuman)
                {
                    //Debugger.Break();
                }


                //var _nearest_target = _targetColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                //// doubled))
                //if (l.Value < r.Value)
                //{
                //    _civM_1.Assault_TargetCiv = _targetColoniesLocations.Aggregate((l, r) => l).Key;
                //}
                //else
                //{
                //    _civM_1.Assault_TargetCiv = _targetColoniesLocations.Aggregate((l, r) => r).Key;
                //}





                //Borg                    
                //if (_itIsBorg == true)
                ////{
                //if (_civ1.CivID == 6 || _civ1.Key == "BORG")
                //{
                //    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
                //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                //    continue;
                //}
                //if (_civ2.CivID == 6 || _civ2.Key == "BORG")
                //{
                //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                //    continue;
                //}
                //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
                ////if (_writeDirectly) Console.WriteLine(_text);
                ////continue;
                ////}

                //Console.WriteLine(_newline + "Step_7734:; Begin of _diplomacyBasicsSummary_Text" + _newline + _diplomacyBasicsSummary_Text + _newline + "End of _diplomacyBasicsSummary_Text" + _newline);




                //DoBorgDiploApply(_civ1, _civ2); // not worth
                ////Borg                    
                if (_civ1.CivID == 6 || _civ1.Key == "BORG")
                {
                    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                    continue;
                }
                if (_civ2.CivID == 6 || _civ2.Key == "BORG")
                {
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                    continue;
                }
                //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
                ////if (_writeDirectly) Console.WriteLine(_text);
                //continue;
                //}


                Diplomat diplomat2 = Diplomat.Get(_civ2);
                string _Contact = "";
                if (_diplomat1.GetForeignPower(_civ2).DiplomacyData.Status == ForeignPowerStatus.NoContact ||
                    diplomat2.GetForeignPower(_civ1).DiplomacyData.Status == ForeignPowerStatus.NoContact)
                {
                    _Contact = " > No Contact !";
                    //_text = "Step_7710:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > NoContact";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.DiplomacyDetails.DebugFormat("DiplomacyData.Status = NoContact for {0} vs {1}", _civ1, _civ2);
                    continue;
                }

                //if (_civ1.Key == "FEDERATION" || _civ2.Key == "FEDERATION")
                //{
                //    _checkRace = true;
                //    _text = "Step_7702:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                //        + " > " + _foreignPowerStatus
                //        + "" + _Contact
                //        ;
                //    //if (_writeDirectly)
                //    Console.WriteLine(_text);

                //    Debugger.Break();
                //}
                #endregion DiplomacyBasics





                //DiplomacyDoStatus(_civ1, _civ2);  // like War


                Diplomacy_1_PendingActions(_civ1, _civ2);


                var _diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];

                string _diplomat1_Location_String = "(Empire)";
                if (!_diplomat1.Owner.IsEmpire && _diplomat1.SeatOfGovernment != null)
                    _diplomat1_Location_String = _diplomat1.SeatOfGovernment.Location.ToString();

                // SitRep for all
                if (_foreignPowerStatus != ForeignPowerStatus.OwnerIsSubjugated)
                {
                    _text = "Relation > "
                        + "Regard: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Regard.CurrentValue.ToString())
                        + ", Trust: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Trust.CurrentValue.ToString())

                        + " > " + _foreignPowerStatus

                        + " vs " + _diplomat1.Owner
                        + " " + _diplomat1_Location_String
                        ;
                    // too much info
                    //Console.WriteLine("Step_7429:; " + _text + "; Turn " + GameContext.Current.TurnNumber + ";SR for " + _civ2.Name);

                    GameContext.Current.CivilizationManagers[_civ2].SitRepEntries.Add(
                        new ReportEntry_ShowDiplo(_civ2, _text, "", "", SitRepPriority.BlueDark));

                }

                //string _testCiv = "FEDERATION";
                string _testCiv = "BORG";
                if (_civ1.Key == _testCiv || _civ2.Key == _testCiv)
                {
                    //_checkRace = true;
                    _text = "Step_7702:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                        + " > " + _foreignPowerStatus
                        + "" + _Contact
                        + _newline
                        ;
                    //if (_writeDirectly)
                    Console.WriteLine(_text);
                    _diplomacyBasicsSummary_Text += _text;

                    //Debugger.Break();
                }

                Console.WriteLine(_all_attack_location_text + " > from Step_7703 = _all_attack_location_text");

                //Debugger.Break();
            }


        }

        public int LocationFirePower(MapLocation _loc, out int _location_fire_power)
        {
            _location_fire_power = 10;


            Sector _sector = new Sector(_loc);

            IList<string> _involved_civs = new List<string>();
            //_involved_civs.Add("Dummy");


            // atm all fleets are counted for firepower, even own ones and not involved ones

            List<Fleet> _all_fleets_here = GameContext.Current.Universe.Find<Fleet>()//(_civ).ToList()
            .Where(a => a.Location.ToString() == _loc.ToString())
            .ToList()
            ;



            foreach (var item in _all_fleets_here)
            {
                _location_fire_power += item.Fire_power_fleet();

                if (!_involved_civs.Contains(item.Owner.ToString()))
                {
                    _involved_civs.Add(item.Owner.Key);
                }
            }

            if (_sector.Station != null)// && _sector.Station.OwnerID != this._civ)
            {
                _location_fire_power += _sector.Station.Fire_Power_Orbital;
            }

            if (_sector.System != null && _sector.System.Colony != null)// && _sector.Station.OwnerID != this._civ)
            {
                _location_fire_power += Colony.DefenseValue(_sector.System.Colony);


            }


            //var _ships_in_location = GetShipsAtLocation(_loc);
            return _location_fire_power;
        }

        private void Report_SomeSectors(Civilization _civ1, CivilizationManager _civM_1)
        {
            string _text;

            if (_civ1.IsEmpire)
            {
                _text = ResourceManager.GetString("DOUBLE_CLICK_FOR_GOING_THERE"); // for going there

                if (_civM_1.AccumulateLocation != null && _civM_1.AccumulateLocation.ToString() != "(0, 0)")
                {
                    //_text += "Accumulate Sector = " + _civM_1.AccumulateLocation + ", ";
                    _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _civM_1.AccumulateLocation,
                        "Accumulate Sector = " + _civM_1.AccumulateLocation + _text, "", "", SitRepPriority.Purple));
                }



                if (_civM_1.Assault_Accumulate_Location_1 != null && _civM_1.Assault_Accumulate_Location_1.ToString() != "(0, 0)")
                {
                    //_text += "SystemAssault Location 1 = " + _civM_1.Assault_Accumulate_Location_1 + ", ";
                    _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _civM_1.Assault_Accumulate_Location_1
                        , "Assault Accumulate Location 1 = " + _civM_1.Assault_Accumulate_Location_1 + _text, "", "", SitRepPriority.Purple));
                }

                //if (_civM_1.SystemAssault_Accumulate_Location_2 != null && _civM_1.SystemAssault_Accumulate_Location_2.ToString() != "(0, 0)")
                //{
                //    //_text += "SystemAssault Location 2 = " + _civM_1.SystemAssault_Accumulate_Location_2 + ", ";
                //    _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _civM_1.SystemAssault_Accumulate_Location_2
                //        , "SystemAssault Accumulate Location 2 = " + _civM_1.SystemAssault_Accumulate_Location_2 + _text, "", "", SitRepPriority.Purple));
                //}

                if (_civM_1.StrandedShipsSector.Location != _civM_1.HomeSystem.Sector.Location && _civM_1.StrandedShipsSector != null && _civM_1.StrandedShipsSector.ToString() != "(0, 0)")
                {
                    //_text += "SystemAssault Location 2 = " + _civM_1.SystemAssault_Accumulate_Location_2 + ", ";
                    _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _civM_1.StrandedShipsSector.Location
                        , "StrandedShipsSector = " + _civM_1.StrandedShipsSector + _text, "", "", SitRepPriority.Purple));
                }

                //if (_text != "") // separated lines allow a double click and so a markup in the _map
                //{
                //    _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1
                //    , _text, "", "", SitRepPriority.Purple));
                //}
            }
        }


        //private void DoBorgDiploApply(Civilization civ1, Civilization civ2)
        //{
        //    //Diplomat _diplomat1 = Diplomat.Get(_civ1);
        //    //ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
        //    ////Borg                    
        //    //if (_civ1.CivID == 6 || _civ1.Key == "BORG")
        //    //{
        //    //    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
        //    //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
        //    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

        //    //    //continue;
        //    //}
        //    //if (_civ2.CivID == 6 || _civ2.Key == "BORG")
        //    //{
        //    //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
        //    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

        //    //    //continue;
        //    //}
        //    //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
        //    //if (_writeDirectly) Console.WriteLine(_text);
        //}

        private void Diplomacy_Response_Sent(Civilization _civ_1, Civilization civ2)
        {
            Diplomat diplomat1 = Diplomat.Get(_civ_1);

            ForeignPower _diplomatCiv2 = diplomat1.GetForeignPower(civ2);
            string _text;
            bool _writeDirectly = true;

            IResponse responseSent = _diplomatCiv2.ResponseSent;
            if (responseSent != null)
            {
                _diplomatCiv2.CounterpartyForeignPower.ResponseReceived = responseSent; // cross over response sent to response received
                _text =
                    _diplomatCiv2.Owner.Key
                    + " sent Response " + _diplomatCiv2.ResponseSent.Proposal.ToString()
                    + " to " + _diplomatCiv2.Counterparty.Key
                    ;
                if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("{0} sent Response {1} to {2}"
                //    , _diplomatForeignPower_Civ2.Owner.Key, _diplomatForeignPower_Civ2.ResponseSent.Proposal.ToString(), _diplomatForeignPower_Civ2.Counterparty.Key);
                _diplomatCiv2.LastResponseSent = responseSent;
                _text =
                        /*_diplomatForeignPower_Civ2.Owner.Key
                        + */" Response Sent stored in LastResponseSent " + _diplomatCiv2.ResponseSent.ToString()
                        ;
                if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("Response Sent stored in LastResponseSent, {0}", _diplomatForeignPower_Civ2.ResponseSent.ToString());
                _diplomatCiv2.ResponseSent = null;

                if (responseSent.ResponseType != ResponseType.NoResponse &&
                    !(responseSent.ResponseType == ResponseType.Accept && responseSent.Proposal.IsGift()))
                {
                    if (_civ_1.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[_civ_1].SitRepEntries.Add(new DiplomaticSitRepEntry(_civ_1, responseSent));
                    }

                    if (civ2.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
                    }
                }
                else if (responseSent.ResponseType != ResponseType.NoResponse && responseSent.ResponseType == ResponseType.Reject)
                {
                    if (_civ_1.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[_civ_1].SitRepEntries.Add(new DiplomaticSitRepEntry(_civ_1, responseSent));
                    }

                    if (civ2.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
                    }
                }
            }
            else
            {
                _diplomatCiv2.CounterpartyForeignPower.ResponseReceived = null;
            }
        }

        private void Diplomacy_Statement_Sent(Civilization civ1, Civilization civ2)
        {
            string _text;
            bool _writeDirectly = true;

            Diplomat _diplomatCiv1 = Diplomat.Get(civ1);
            CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[civ1];

            ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(civ2);
            CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[civ2];

            ForeignPowerStatus _foreignPowerStatus = _diplomatCiv1.GetForeignPower(civ2).DiplomacyData.Status;

            Statement statementSent = _diplomatCiv2.StatementSent;
            if (statementSent != null)
            {
                // StatementSent becomes counterparty StatementReceived
                _diplomatCiv2.CounterpartyForeignPower.StatementReceived = statementSent;
                _text = "Step_8236:; ProposalSent becomes "
                        + "; _diplomatForeignPower_Civ2.Owner= " + _diplomatCiv2.CounterpartyForeignPower.Owner.Key
                        + "; got StatementReceived= " + Enum.GetName(typeof(StatementType), statementSent.StatementType)
                        + "; from= " + statementSent.Sender.Key
                        ;
                if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("_diplomatForeignPower_Civ2.Owner {0} got StatementReceived {1} from {2}"
                //    , _diplomatForeignPower_Civ2.CounterpartyForeignPower.Owner.Key
                //    , Enum.GetName(typeof(StatementType), statementSent.StatementType)
                //    , statementSent.Sender.Key);
                _diplomatCiv2.LastStatementSent = statementSent;
                _diplomatCiv2.StatementSent = null;

                //GameLog.Core.DiplomacyDetails.DebugFormat("_diplomatForeignPower_Civ2.Owner = {0}", _diplomatForeignPower_Civ2.Owner.Key);
                //GameLog.Core.DiplomacyDetails.DebugFormat("CounterpartyForeignPower.Owner = {0}", _diplomatForeignPower_Civ2.CounterpartyForeignPower.Owner.Key);

                bool _doDeclareWar = false;

                if (_civM_1.Assault_Location != null && _foreignPowerStatus != ForeignPowerStatus.AtWar)
                {
                    _doDeclareWar = true;
                }


                if (statementSent.StatementType == StatementType.WarDeclaration)
                {
                    _doDeclareWar = true;

                }

                if (_doDeclareWar == true)
                {
                    _diplomatCiv2.DeclareWar();
                }
            }
            else
            {
                _diplomatCiv2.CounterpartyForeignPower.StatementReceived = null;
            }
        }

        private void Diplomacy_Proposal_Sent(Civilization civ1, Civilization civ2)
        {
            Diplomat _diplomatCiv1 = Diplomat.Get(civ1);
            ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(civ2);
            string _text;
            bool _writeDirectly = true;
            //  Second.2 = proposalSent
            IProposal proposalSent = _diplomatCiv2.ProposalSent;
            if (proposalSent != null)
            {
                _diplomatCiv2.CounterpartyForeignPower.ProposalReceived = proposalSent;
                _diplomatCiv2.LastProposalSent = proposalSent;
                _diplomatCiv2.ProposalSent = null;
                _text = "Step_8234:; "
                     + _diplomatCiv2.LastProposalSent.Clauses[0].ClauseType.ToString() + " (ProposalReceived)"
                    + "; from Owner= " + _diplomatCiv2.Owner.ToString()
                    + "; to=; " + _diplomatCiv2.Counterparty.ToString()
                    + " (ProposalSent)"
                    ;
                if (_writeDirectly) Console.WriteLine(_text);

                //GameLog.Client.DiplomacyDetails.DebugFormat("** ProposalSent becomes Counterparty ProposalReceived [{0}], Counterparty = {1}, Owner = {2}"
                //    , _diplomatForeignPower_Civ2.LastProposalSent.Clauses[0].ClauseType.ToString(), _diplomatForeignPower_Civ2.Counterparty.ToString(), _diplomatForeignPower_Civ2.Owner.ToString()); ;

                if (civ1.IsEmpire)
                {
                    GameContext.Current.CivilizationManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, proposalSent));
                }

                if (civ2.IsEmpire)
                {
                    GameContext.Current.CivilizationManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, proposalSent));
                }
            }
            else
            {
                _diplomatCiv2.CounterpartyForeignPower.ProposalReceived = null;
            }
        }

        private void Diplomacy_9_ConsoleWriteline(Civilization civ1, Civilization civ2)
        {
            Diplomat _diplomatCiv1 = Diplomat.Get(civ1);
            ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(civ2);
            string _newline = Environment.NewLine;
            string _text = "";

            #region Gamelogs
            if (_diplomatCiv2.ProposalReceived != null)
            {
                _text += _newline + "ProposalReceived: "
                          + _diplomatCiv2.ProposalReceived.Sender + " to "
                          + _diplomatCiv2.ProposalReceived.Recipient + ": > "
                          + _diplomatCiv2.ProposalReceived.Clauses.ToString()
                          + _newline;
            }

            if (_diplomatCiv2.ProposalSent != null)
            {
                _text += _newline + "ProposalSent: "
                          + _diplomatCiv2.ProposalSent.Sender + " to "
                          + _diplomatCiv2.ProposalSent.Recipient + ": > "
                          + _diplomatCiv2.ProposalSent.Clauses.ToString()
                          + _newline;
            }

            if (_diplomatCiv2.ResponseReceived != null)
            {
                _text += _newline + "ResponseReceived: "
                          + _diplomatCiv2.ResponseReceived.Sender + " to "
                          + _diplomatCiv2.ResponseReceived.Recipient + ": > "
                          + _diplomatCiv2.ResponseReceived.ResponseType.ToString()
                          + _newline;
            }

            if (_diplomatCiv2.ResponseSent != null)
            {
                _text += _newline + "ResponseSent: "
                          + _diplomatCiv2.ResponseSent.Sender + " to "
                          + _diplomatCiv2.ResponseSent.Recipient + ": > "
                          + _diplomatCiv2.ResponseSent.ResponseType.ToString()
                          + _newline;
            }

            if (_diplomatCiv2.StatementReceived != null)  // in SinglePlayer you'll never get this "received" because you are always the playing SENDER unitl AI sends
            {

                //string parameterString = _diplomatForeignPower_Civ2.StatementSent.Parameter.ToString() ?? "";

                _text += _newline + "StatementReceived: "
                          + _diplomatCiv2.StatementReceived.Sender + " to "
                          + _diplomatCiv2.StatementReceived.Recipient + ": > "
                          + ", Parameter = " //+ parameterString
                          + Enum.GetName(typeof(StatementType), _diplomatCiv2.StatementReceived.StatementType)
                          + _newline
                          ;
            }
            if (_diplomatCiv2.StatementSent != null)  // in SinglePlayer you'll never get this "received" because you are always the playing SENDER unitl AI sends
            {

                //string parameterString = _diplomatForeignPower_Civ2.StatementSent.Parameter.ToString() ?? "";

                _text += _newline + "StatementSent: "
                          + _diplomatCiv2.StatementSent.Sender + " to "
                          + _diplomatCiv2.StatementSent.Recipient + ": > "
                          + ", Parameter = " //+ parameterString
                          + _newline
                          ;
            }

            // GameLog.Core.Diplomacy.DebugFormat("------------------------------------------");
            //GameLog.Core.DiplomacyDetails.DebugFormat("received a 'Sabotage'-Diplomacy-Statement, Tone = {0}", _diplomatForeignPower_Civ2.StatementReceived.Tone.ToString());

            if (_text.Length > 44)  // not only the entry phrase...
            {
                GameLog.Core.DiplomacyDetails.DebugFormat(_text);
            }

            _text = "what's next + ";

            if (_diplomatCiv2.StatementSent != null)
            {
                _text += _newline + "(relevant is just the receive on HOSTING side.... StatementSent: "
                            + _diplomatCiv2.StatementSent.Sender + " vs "
                            + _diplomatCiv2.StatementSent.Recipient + ": > "
                            + _diplomatCiv2.StatementSent.StatementType.ToString()
                            + ", Parameter = " //+ parameterString
                            + _newline;
            }

            if (_diplomatCiv2.PendingAction != PendingDiplomacyAction.None)
            {
                _text += _newline + "PendingAction: "
                            //+ _diplomatForeignPower_Civ2.PendingAction + " vs "
                            //+ _diplomatForeignPower_Civ2.PendingAction.Recipient
                            + _diplomatCiv2.PendingAction.ToString()
                            + _newline;
            }

            if (_text != "what's next + ")
            {
                GameLog.Core.DiplomacyDetails.DebugFormat(_text);
            }
            #endregion Gamelogs
            //}
        }

        private void DiplomacyDoStatus(Civilization _civ1, Civilization _civ2)
        {
            // empty > included into Do_13_Diplomacy()


            //    Diplomat _diplomat1 = Diplomat.Get(_civ1);
            //    CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];
            //    //Diplomat _diplomat1 = Diplomat.Get(_civ1);

            //    ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
            //    CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];
            //    ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

            //    List<Civilization> _possibleTargetCivs = new List<Civilization>();

            //    string _diplomacyBasicsSummary_Text = "";

            //    //Debugger.Break()
            //    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }


            //    //AtWar
            //    if (_foreignPowerStatus == ForeignPowerStatus.AtWar)
            //    {


            //        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
            //        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
            //        _text = "Step_7732:; Do_13_Diplomacy > " + _civ1 + "; vs ; " + _civ2 + "; > AtWar";
            //        //if (_writeDirectly) Console.WriteLine(_text);

            //        //List<Civilization> _possibleTargetCivs = new List<Civilization>();

            //        //if (_civ1.IsHuman)
            //        //{
            //        //    Debugger.Break();
            //        //}

            //        //_civM_1.TargetCivList.AddRange(_civ2.);
            //        //_civM_1.TargetCivList.Distinct();
            //        _possibleTargetCivs.Add(_civ2);
            //        _possibleTargetCivs.Distinct();

            //    }
            //    List<Civilization> _target_help_list = new List<Civilization>() { _civ1 };
            //    //_target_help_list.Add(_civ1);
            //    if (GameContext.Current.CivilizationManagers[_civ1].TargetCivList == null
            //        || GameContext.Current.CivilizationManagers[_civ1].TargetCivList.Count == 0)
            //    {
            //        GameContext.Current.CivilizationManagers[_civ1].TargetCivList = _target_help_list;
            //    }
            //    else
            //    {
            //        GameContext.Current.CivilizationManagers[_civ1].TargetCivList.AddRange(_possibleTargetCivs);
            //        _target_help_list = GameContext.Current.CivilizationManagers[_civ1].TargetCivList;
            //        //_target_help_list = _target_help_list;
            //        GameContext.Current.CivilizationManagers[_civ1].TargetCivList = _target_help_list.Distinct().ToList();

            //        //Debugger.Break()
            //        //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

            //    }


            //    //if (_possibleTargetCivs.Count > 0)
            //    //{
            //    //    //GameContext.Current.CivilizationManagers[_civ1].TargetCivList = _possibleTargetCivs;
            //    //    GameContext.Current.CivilizationManagers[_civ1].TargetCivList.AddRange(_possibleTargetCivs);
            //    //}


            //    //GameContext.Current.CivilizationManagers[_civ1].TargetList_Update(_target_help_list);
            //    //GameContext.Current.CivilizationManagers[_civ1].TargetList_Update(_civ1);

            //    string _atWarText = "";

            //    if (_foreignPowerStatus == ForeignPowerStatus.AtWar
            //            && _civM_1.TargetCivList != null
            //            && _civM_1.TargetCivList.Count > 0)
            //    {

            //        _targetColoniesLocations = new Dictionary<Civilization, int>();
            //        _targetColoniesLocations.Add(_civ1, 99);


            //        foreach (var _item in _civM_1.TargetCivList)
            //        {
            //            MapLocation _loc_1 = _civM_1.HomeSystem.Location;
            //            MapLocation _loc_2 = GameContext.Current.CivilizationManagers[_item.CivID].HomeSystem.Location;
            //            int _distance = (int)Math.Sqrt((int)Math.Pow(_loc_1.X - _loc_2.X, 2)
            //                + (int)Math.Pow(_loc_1.Y - _loc_2.Y, 2));
            //            if (_loc_2 == _loc_1)
            //            {
            //                _distance += 50;
            //            }
            //            if (!_targetColoniesLocations.ContainsKey(_item))
            //            {
            //                _targetColoniesLocations.Add(_item, _distance);
            //            }
            //            //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }
            //        }

            //        //Debugger.Break()
            //        //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

            //        //var _nearest_target = _targetColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

            //        //_civM_1.Assault_TargetCiv = _nearest_target;
            //        //_text = "Step_7737:; Do_13_Diplomacy > "
            //        //    + "AtWar > nearest Target: "
            //        //    + _civ1 + " at " + _civM_1.HomeSystem.Location + "; vs ; "
            //        //    + _civ2 + " at " + _civM_2.HomeSystem.Location
            //        //    //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location
            //        //    + ", Distance= " + MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location)
            //        //    //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

            //        //    ;
            //        //if (_writeDirectly) Console.WriteLine(_text);

            //        //if (_civ1.IsHuman) { Debugger.Break(); }



            //        int _minValue = 98;
            //        foreach (var _item in _targetColoniesLocations)
            //        {
            //            if (_item.Value < _minValue)
            //            {
            //                _minValue = _item.Value;
            //                _civM_1.Assault_TargetCiv = _item.Key;

            //                _text = "Step_7736:; Do_13_Diplomacy > "
            //                            + "AtWar > nearest Target: "
            //                            + _civ1 + " at " + _civM_1.HomeSystem.Location + "; vs ; "
            //                            + _civ2 + " at " + _civM_2.HomeSystem.Location
            //                            //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location
            //                            + ", Distance= " + MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location)
            //                            //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

            //                            ;
            //                if (_writeDirectly) Console.WriteLine(_text);
            //                _diplomacyBasicsSummary_Text += _newline + _text;

            //                //if (_civ1.IsHuman) { Debugger.Break(); }

            //            }
            //        }

            //        _text = "Step_7737:; Do_13_Diplomacy > "

            //                + _civ1 + " at " + _civM_1.HomeSystem.Location /*+ "; vs ; "*/
            //                //+ " nearest Target: "
            //                + "; AtWar > Distance= " + MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location)
            //                + " for " + _civ2 + " at " + _civM_2.HomeSystem.Location
            //                ;
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        _atWarText = _text;



            //        if (_civ1.IsHuman) { Debugger.Break(); }

            //    }

            //        Console.WriteLine(_newline + "Step_7732:; Begin of _diplomacyBasicsSummary_Text" + /*_newline + */_diplomacyBasicsSummary_Text + _newline + "End of _diplomacyBasicsSummary_Text" + _newline);


            //    if (_civ1.IsHuman 
            //        && _foreignPowerStatus != ForeignPowerStatus.NoContact 
            //        && _foreignPowerStatus != ForeignPowerStatus.AtWar)
            //    {
            //        if (_atWarText == "") _atWarText = "Step_7738:; Do_13_Diplomacy > with nobody for " + _civ1;
            //        if (_writeDirectly) Console.WriteLine(_atWarText);
            //        //Debugger.Break();
            //    }


            //    //var _nearest_target = _targetColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
            //    //// doubled))
            //    //if (l.Value < r.Value)
            //    //{
            //    //    _civM_1.Assault_TargetCiv = _targetColoniesLocations.Aggregate((l, r) => l).Key;
            //    //}
            //    //else
            //    //{
            //    //    _civM_1.Assault_TargetCiv = _targetColoniesLocations.Aggregate((l, r) => r).Key;
            //    //}



            //    int _regard = _diplomatForeignPower_Civ2.DiplomacyData.Regard.CurrentValue;
            //    int _trust = _diplomatForeignPower_Civ2.DiplomacyData.Trust.CurrentValue;

            //    //_text = "Step_7742:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 
            //    //    + "; > Regard =;" + _regard + "; > Trust =;" + _trust;
            //    //if (_writeDirectly) Console.WriteLine(_text);
            //    //////_text = "Step_7744:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Trust =;" + _trust;
            //    //////if (_writeDirectly) Console.WriteLine(_text);

            //    if (_foreignPowerStatus == ForeignPowerStatus.Affiliated)
            //    {
            //        //_text = "Step_7750:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Affiliated";
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        if (_regard < 850)
            //            DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3); // 2 each turnnumber
            //        if (_trust < 800)
            //            DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 4);
            //        //_text = 
            //    }

            //    if (_foreignPowerStatus == ForeignPowerStatus.Allied)
            //    {
            //        //_text = "Step_7760:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Allied";
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        if (_regard < 850)
            //            DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
            //        if (_trust < 800)
            //            DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

            //    }

            //    if (_foreignPowerStatus == ForeignPowerStatus.Friendly)  // Open Borders
            //    {
            //        //_text = "Step_7770:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Friendly";
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        if (_regard < 650)
            //            DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
            //        if (_trust < 600)
            //            DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);

            //    }


            //    if (_foreignPowerStatus == ForeignPowerStatus.Peace)
            //    {
            //        //_text = "Step_7780:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Peace";
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        //if (_regard < 850)
            //        //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
            //        if (_trust < 600)
            //            DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

            //    }

            //    if (_foreignPowerStatus == ForeignPowerStatus.Neutral)
            //    {
            //        //_text = "Step_7710:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Neutral";
            //        //if (_writeDirectly) Console.WriteLine(_text);
            //        if (_regard < 650)
            //            DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
            //        if (_trust < 600)
            //            DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);

            //    }

            //    //Borg                    
            //    //if (_itIsBorg == true)
            //    ////{
            //    //if (_civ1.CivID == 6 || _civ1.Key == "BORG")
            //    //{
            //    //    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
            //    //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
            //    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

            //    //    continue;
            //    //}
            //    //if (_civ2.CivID == 6 || _civ2.Key == "BORG")
            //    //{
            //    //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
            //    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

            //    //    continue;
            //    //}
            //    //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
            //    ////if (_writeDirectly) Console.WriteLine(_text);
            //    ////continue;
            //    ////}
        }

        private void Diplomacy_1_PendingActions(Civilization civ1, Civilization civ2)
        {
            Diplomat diplomat1 = Diplomat.Get(civ1);

            ForeignPower _diplomatCiv2 = diplomat1.GetForeignPower(civ2);

            string _text;

            //GameLog.Core.DiplomacyDetails.DebugFormat("---------------------------------------");
            //GameLog.Core.DiplomacyDetails.DebugFormat("_foreignPowerStatus = {2} for {0} vs {1}", _civ1, _civ2, _foreignPowerStatus.ToString());



            if (_diplomatCiv2.PendingAction.ToString() != "None")
            {
                _text = "Step_7721:; Do_13_Diplomacy > * " + civ1.Key + " * vs * " + civ2.Key
                        + ": PendingAction > Status= >>> " + _diplomatCiv2.PendingAction.ToString()
                        ;
                //if (_writeDirectly)
                Console.WriteLine(_text);
            }

            //if (_checkRace) Debugger.Break();



            switch (_diplomatCiv2.PendingAction)
            {


                case PendingDiplomacyAction.AcceptProposal:
                    {
                        _text = "Step_7722:; Do_13_Diplomacy > * " + civ1.Key + " * vs * " + civ2.Key
                            + ", Accept Status=" + _diplomatCiv2.PendingAction.ToString()
                            ;
                        //if (_writeDirectly)
                        Console.WriteLine(_text);

                        Debugger.Break();
                        //GameLog.Core.DiplomacyDetails.DebugFormat(_text);

                        if (_diplomatCiv2.ProposalReceived != null)
                        {
                            _ = AcceptProposalVisitor.Visit(_diplomatCiv2.ProposalReceived);
                        }

                        _diplomatCiv2.LastProposalReceived = _diplomatCiv2.ProposalReceived;
                        _diplomatCiv2.ProposalReceived = null;
                        break;
                    }

                case PendingDiplomacyAction.RejectProposal:
                    {
                        _text = "Step_7724:; Do_13_Diplomacy > * " + civ1.Key + " * vs * " + civ2.Key
                                + ", Reject Status=" + _diplomatCiv2.PendingAction.ToString()
                                ;
                        //if (_writeDirectly)
                        Console.WriteLine(_text);

                        Debugger.Break();
                        //GameLog.Core.DiplomacyDetails.DebugFormat(_text);

                        if (_diplomatCiv2.ProposalReceived != null)
                        {
                            RejectProposalVisitor.Visit(_diplomatCiv2.ProposalReceived);
                        }

                        _diplomatCiv2.LastProposalReceived = _diplomatCiv2.ProposalReceived;
                        _diplomatCiv2.ProposalReceived = null;
                        break;
                    }
                default:  // case None
                    break;
            }
            //GameLog.Core.DiplomacyDetails.DebugFormat("Next: _diplomatForeignPower_Civ2.PendingAction = NONE for {0} vs {1}, status {2}, pending {3}", _diplomatForeignPower_Civ2.Owner, _diplomatForeignPower_Civ2.Counterparty, _foreignPowerStatus.ToString(), _diplomatForeignPower_Civ2.PendingAction.ToString());
            _diplomatCiv2.PendingAction = PendingDiplomacyAction.None;

            // Ships gets new owner on joining empire - _colonies are done in AccpetPropsalVisitor
            if (civ1.IsEmpire && !civ2.IsEmpire && civ1.Key != "Borg")
            {
                Diplomat currentDiplomat = Diplomat.Get(civ1);
                if (currentDiplomat.GetForeignPower(civ2).DiplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember)
                {
                    //_text = "Searching for Crash: _objectsCiv2";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    List<UniverseObject> _objectsCiv2 = GameContext.Current.Universe.Objects.Where(s => s.Owner == civ2)
                            .Where(s => s.ObjectType == UniverseObjectType.Ship).ToList();
                    foreach (UniverseObject minorsObject in _objectsCiv2)
                    {
                        if (minorsObject.Owner == civ2)
                        {
                            CivilizationManager targetMinor = GameContext.Current.CivilizationManagers[civ2];
                            Colony minorCivHome = targetMinor.HomeColony;
                            int gainedResearchPoints = minorCivHome.Research_Net;
                            Ship ship = (Ship)minorsObject;
                            ship.Owner = civ1;
                            Fleet newfleet = ship.CreateFleet();
                            newfleet.Owner = civ1;
                            newfleet.SetOrder(FleetOrders.IdleOrder.Create());
                            if (newfleet.Order == null)
                            {
                                newfleet.SetOrder(FleetOrders.IdleOrder.Create());
                            }
                            ship.Scrap = false;
                            GameContext.Current.CivilizationManagers[civ1].Research.UpdateResearch(gainedResearchPoints);

                            //GameLog.Core.Ships.DebugFormat("Ship Joined:{0} {1}, Owner {2}, OwnerID {3}, Fleet.OwnerID {4}, Order {5} _fleet name {6} gainedResearchPoints {7}",
                            //        ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order, newfleet.Name, gainedResearchPoints);
                        }
                    }
                }




            }  // foreach _civ2
        }

        private void Diplomacy_Statement_Received(Civilization civ1, Civilization civ2)
        {
            Diplomat diplomat1 = Diplomat.Get(civ1);

            ForeignPower _diplomatCiv2 = diplomat1.GetForeignPower(civ2);
            //ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

            switch (_diplomatCiv2.StatementReceived.StatementType)
            {
                case StatementType.StealCredits:
                    if (civ2.CivID > civ1.CivID)
                    {
                        IntelHelper.SabotageStealCreditsExecute(civ2, civ1, _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    }

                    break;
                case StatementType.StealResearch:
                    if (civ2.CivID > civ1.CivID)
                    {
                        IntelHelper.SabotageStealResearchExecute(civ2, civ1, _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    }

                    break;
                case StatementType.SabotageFood:
                    if (civ2.CivID > civ1.CivID)
                    {
                        IntelHelper.SabotageFoodExecute(civ2, civ1, _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    }

                    break;
                case StatementType.SabotageIndustry:
                    if (civ2.CivID > civ1.CivID)
                    {
                        IntelHelper.SabotageIndustryExecute(civ2, civ1, _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    }

                    break;
                case StatementType.SabotageEnergy:
                    if (civ2.CivID > civ1.CivID)
                    {
                        IntelHelper.SabotageEnergyExecute(civ2, civ1, _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    }

                    break;

                case StatementType.T01: // read statement type off of _diplomatForeignPower_Civ2 and send it to accept - reject dictionary
                case StatementType.T02:
                case StatementType.T03:
                case StatementType.T04:
                case StatementType.T05:
                case StatementType.T10:
                case StatementType.T12:
                case StatementType.T13:
                case StatementType.T14:
                case StatementType.T15:
                case StatementType.T20:
                case StatementType.T21:
                case StatementType.T23:
                case StatementType.T24:
                case StatementType.T25:
                case StatementType.T30:
                case StatementType.T31:
                case StatementType.T32:
                case StatementType.T34:
                case StatementType.T35:
                case StatementType.T40:
                case StatementType.T41:
                case StatementType.T42:
                case StatementType.T43:
                case StatementType.T45:
                case StatementType.T50:
                case StatementType.T51:
                case StatementType.T52:
                case StatementType.T53:
                case StatementType.T54:
                case StatementType.F01:
                case StatementType.F02:
                case StatementType.F03:
                case StatementType.F04:
                case StatementType.F05:
                case StatementType.F10:
                case StatementType.F12:
                case StatementType.F13:
                case StatementType.F14:
                case StatementType.F15:
                case StatementType.F20:
                case StatementType.F21:
                case StatementType.F23:
                case StatementType.F24:
                case StatementType.F25:
                case StatementType.F30:
                case StatementType.F31:
                case StatementType.F32:
                case StatementType.F34:
                case StatementType.F35:
                case StatementType.F40:
                case StatementType.F41:
                case StatementType.F42:
                case StatementType.F43:
                case StatementType.F45:
                case StatementType.F50:
                case StatementType.F51:
                case StatementType.F52:
                case StatementType.F53:
                case StatementType.F54:
                    {
                        GameLog.Core.DiplomacyDetails.DebugFormat("Statement sent for Dictionary Entery {0} _diplomatForeignPower_Civ2 Counterparty {1}, Owner {2}",
                            Enum.GetName(typeof(StatementType), _diplomatCiv2.StatementReceived.StatementType),
                            _diplomatCiv2.Counterparty.Key,
                            _diplomatCiv2.Owner.Key);

                        DiplomacyHelper.SpecificCivAcceptingRejecting(_diplomatCiv2.StatementReceived.StatementType); // act on statement to accept reject
                        break;
                    }
                case StatementType.WarPact:
                case StatementType.CommendWar:
                case StatementType.DenounceWar:
                case StatementType.WarDeclaration:
                    break;
                default:
                    break;
            }
            //else
            //{
            //else

            ////  Second.1 = StatementReceived
            if (_diplomatCiv2.StatementReceived == null && _diplomatCiv2.LastStatementReceived != null)
            {
                switch (_diplomatCiv2.LastStatementReceived.StatementType)
                {
                    case StatementType.StealCredits:
                        IntelHelper.SabotageStealCreditsExecute(civ2, civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.StealResearch:
                        IntelHelper.SabotageStealResearchExecute(civ2, civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageFood:
                        IntelHelper.SabotageFoodExecute(civ2, civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageIndustry:
                        IntelHelper.SabotageIndustryExecute(civ2, civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageEnergy:
                        IntelHelper.SabotageEnergyExecute(civ2, civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    //    GameLog.Core.DiplomacyDetails.DebugFormat("LastStatementReceived Statement Type = {0} _diplomatForeignPower_Civ2 counterparyt {1}, owner {2}",
                    //        Enum.GetName(typeof(StatementType), _diplomatForeignPower_Civ2.LastStatementReceived.StatementType),
                    //        _diplomatForeignPower_Civ2.Counterparty.Key,
                    //        _diplomatForeignPower_Civ2.Owner.Key);
                    //    //DiplomacyHelper.AcceptRejectDictionaryFromStatement(_diplomatForeignPower_Civ2.LastStatementReceived);
                    //    DiplomacyHelper.SpecificCivAcceptingRejecting(_diplomatForeignPower_Civ2.LastStatementReceived.StatementType);
                    //    break;
                    case StatementType.CommendWar:
                    case StatementType.DenounceWar:
                    case StatementType.WarDeclaration:
                        break;
                    default:
                        break;
                }
                //}
            }
        }

        #endregion

        #region DoCombat() Method
        void Do_15_Combat(GameContext _game)
        {
            string _text = "Step_8009:; ---- Do_15_Combat --------------------";
            Console.WriteLine(_text);

            HashSet<MapLocation> _combatLocations = new HashSet<MapLocation>();
            HashSet<MapLocation> _invasionLocations = new HashSet<MapLocation>();
            List<List<CombatAssets>> _combats = new List<List<CombatAssets>>();
            List<InvasionArena> _invasions = new List<InvasionArena>();
            List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                //.Where(_f => _f.Location == _combatLocations || )
                .ToList();

            foreach (Fleet _fleet in _fleetsAtLocation)
            {
                if (!_combatLocations.Contains(_fleet.Location))
                {
                    List<CombatAssets> _assets = CombatHelper.GetCombatAssets(_fleet.Location); // part of altering collection while using from GameEngine 216 and CombatHelper.cs line 58
                    List<Civilization> _fleetsOwners = _fleetsAtLocation
                            .Select(o => o.Owner)
                            .Distinct()
                            .ToList();

                    if (_assets.Count > 1 && _fleetsOwners.Count > 1)
                    {
                        foreach (Fleet nextFleet in _fleetsAtLocation)
                        {
                            if (_fleet.Owner == nextFleet.Owner ||
                                CombatHelper.WillFightAlongside(_fleet.Owner, nextFleet.Owner) ||
                                !CombatHelper.WillEngage(_fleet.Owner, nextFleet.Owner))
                            {
                                continue;
                            }
                        }

                        _combats.Add(_assets); // we add all the ships at this location if there is any _combat. Combat decides who is in and on what side
                        _ = _combatLocations.Add(_fleet.Location);
                    }
                }

                if (!_invasionLocations.Contains(_fleet.Location))
                {
                    if (_fleet.Sector.System != null)
                    {
                        if (_fleet.Order is AssaultSystemOrder)
                        {
                            _invasions.Add(new InvasionArena(_fleet.Sector.System.Colony, _fleet.Owner));
                            _ = _invasionLocations.Add(_fleet.Location);
                        }
                    }
                    //else
                    //{
                    //    _text = "Step_8006:; No Invasion available due to no system at " + _fleet.Location + blank + _fleet.Name;
                    //    if (_writeDirectly) Console.WriteLine(_text);
                    //    //GameLog.Core.SystemAssault.InfoFormat(_text);
                    //}
                }
            }

            foreach (List<CombatAssets> combat in _combats)
            {
                _ = CombatReset.Reset();
                _text = "Step_8011:; ---- COMBAT OCCURED GameEngine --------------------";
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);
                OnCombatOccurring(combat);
                _ = CombatReset.WaitOne();

            }

            foreach (InvasionArena invasion in _invasions)
            {
                _ = CombatReset.Reset();
                _text = "Step_8021:; ---- INVASION OCCURED GameEngine --------------------";
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);
                OnInvasionOccurring(invasion);
                if (invasion.Invader.IsHuman)
                {
                    _ = CombatReset.WaitOne();
                }
            }

            List<Fleet> invadingFleets = _invasions
                .SelectMany(o => o.InvadingUnits)
                .OfType<InvasionOrbital>()
                .Where(o => !o.IsDestroyed)
                .Select(o => o.Source)
                .OfType<Ship>()
                .Select(o => o.Fleet)
                .Distinct()
                .ToList();

            foreach (Fleet invadingFleet in invadingFleets)
            {
                if (invadingFleet.Order is AssaultSystemOrder assaultOrder && !assaultOrder.IsValidOrder(invadingFleet))
                {
                    invadingFleet.SetOrder(invadingFleet.GetDefaultOrder());
                }
            }

            //_ = ParallelForEach(GameContext.Current.Universe.Find<Colony>(), c =>
            foreach (var c in GameContext.Current.Universe.Find<Colony>())
            {
                GameContext.PushThreadContext(_game);
                try { c.RefreshShielding(true); }
                finally { _ = GameContext.PopThreadContext(); }
            }
            ;
            //});
        }
        #endregion

        #region DoPopulation() Method
        void Do_16_Population(GameContext _game)
        {
            string _turnnumber = GameContext.Current.TurnNumber.ToString();
            string _text;
            bool _writeDirectly = true;
            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            foreach (var _civ in GameContext.Current.Civilizations)
            {
                GameContext.PushThreadContext(_game);
                try
                {
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                    _civM.TotalPopulation.Reset();

                    //_text = "";

                    foreach (Colony _colony in _civM.Colonies)
                    {
                        _colony.Population.Maximum = _colony.Population_Max;

                        int popChange = 0;
                        int foodDeficit;

                        _ = _colony.FoodReserves.AdjustCurrent(_colony.GetProductionOutput(ProductionCategory.Food));
                        foodDeficit = Math.Min(_colony.FoodReserves.CurrentValue - _colony.Population.CurrentValue, 0);
                        _ = _colony.FoodReserves.AdjustCurrent(-1 * _colony.Population.CurrentValue);
                        _colony.FoodReserves.UpdateAndReset();

                        /*
                         * If there is not enough food to feed the population, we need to kill off some of the
                         * population due to starvation.  Otherwise, we increase the population according to the
                         * growth rate if we did not suffer a loss due to starvation during the previous turnnumber.
                         * We want to ensure that there is a 1-turnnumber period between population loss and recovery.
                         */
                        //if (_colony.Name == "Ledos")
                        //    ; // ddd;

                        Percentage growthRate = _colony.GrowthRate;


                        if (foodDeficit < 0)
                        {
                            popChange = -(int)Math.Floor(0.1 * Math.Sqrt(Math.Abs(_colony.Population.CurrentValue * foodDeficit)));
                            _text = string.Format(ResourceManager.GetString("SITREP_STARVATION"), _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Red));
                            //_civM_1.SitRepEntries.Add(new StarvationSitRepEntry(_civ, _colony));

                            _colony.Health.AdjustCurrent(-3);
                            _colony.Health.UpdateAndReset();

                            _colony.Morale.AdjustCurrent(-2);
                            _colony.Morale.UpdateAndReset();
                        }
                        else
                        {
                            // minimum growth of 1.0, otherwise minors and even Majors stays and begin value e.g. 16 (not getting more!!)
                            popChange = (int)Math.Ceiling(1 + growthRate * _colony.Population.CurrentValue);  // minimum growth of 1.0

                        }

                        if (popChange < 0 && growthRate < 0 && GameContext.Current.TurnNumber > 2)
                        {
                            _text = string.Format(ResourceManager.GetString("SITREP_POPULATION_DYING"), _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Red));
                            //_civM_1.SitRepEntries.Add(new PopulationDyingSitRepEntry(_civ, _colony));

                            _colony.Health.AdjustCurrent(-2);
                            _colony.Health.UpdateAndReset();

                            _colony.Morale.AdjustCurrent(-3);
                            _colony.Morale.UpdateAndReset();

                        }

                        if (popChange > 12) // popGrowth limited to 12
                        {
                            popChange = 12;
                        }

                        int newPopulation = _colony.Population.AdjustCurrent(popChange);

                        // TODO: We need to figure out how to deal with a civilization having no _colonies
                        // and no _colony ships
                        if (_colony.Population.CurrentValue == 0)
                        {
                            _text = "Step_3277:; " + GameEngine.LocationString(_colony.Location.ToString())
                            + " " + _colony.Name
                            + " > Population have died from illness, and the _colony has been lost.";

                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Red));
                            //_civM_1.SitRepEntries.Add(new PopulationDiedSitRepEntry(_colony.Owner, _colony.Sector.Location, _note));
                            _colony.Destroy();
                            _civM.EnsureSeatOfGovernment();
                            return;
                        }
                        _colony.Population.UpdateAndReset();
                        _ = _civM.TotalPopulation.AdjustCurrent(_colony.Population.CurrentValue);


                        int newLabors = _colony.GetAvailableLabor() / 10;
                        int curPop = _colony.Population.CurrentValue;
                        int maxPop = _colony.Population_Max;

                        if (popChange > 0 && newLabors > 0)
                        {
                            while (newLabors > 0 && _colony.Facilities_Total2_Industry > _colony.Facilities_Active2_Industry)
                            {
                                _ = _colony.Facility_Activate(ProductionCategory.Industry);
                                newLabors -= 1;
                                _text = GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                + " > Population growing (now " + curPop + " max. " + maxPop
                                + " ) - one labor unit was sent to Industry Production"
                                //+ " at " + 
                                ;

                                //_civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));


                                Console.WriteLine("Step_3281:; Turn " + _turnnumber + ": " + _text);

                                //GameLog.Core.CombatDetails.DebugFormat("Step_3281:; " + _text);

                            }
                        }

                        if (popChange > 0 && newLabors > 0)
                        {
                            while (newLabors > 0 && _colony.Facilities_Total4_Research > _colony.Facilities_Active4_Research)
                            {
                                _ = _colony.Facility_Activate(ProductionCategory.Research);
                                newLabors -= 1;
                                _text = GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                + " > Population growing (now " + curPop + " max. " + maxPop
                                + " ) - one labor unit was sent to Research Facility"
                                //+ " at " + GameEngine.LocationString(_colony.Location.ToString()) + blank + _colony.Name
                                ;


                                //_civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));
                                Console.WriteLine("Step_3282:; Turn " + _turnnumber + ": " + _text);
                                //GameLog.Core.CombatDetails.DebugFormat("Step_3282:; " + _text);
                            }
                        }

                        if (popChange > 0 && newLabors > 0)
                        {
                            while (newLabors > 0 && _colony.Facilities_Total5_Intelligence > _colony.Facilities_Active5_Intelligence)
                            {
                                _ = _colony.Facility_Activate(ProductionCategory.Intelligence);
                                newLabors -= 1;
                                _text = GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                + " > Population growing (now " + curPop + " max. " + maxPop
                                  + " ) - one labor unit was sent to Intelligence Facility"
                                  //+ " at " + GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                  ;

                                //_civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));
                                Console.WriteLine("Step_3283:; Turn " + _turnnumber + ": " + _text);
                                //GameLog.Core.CombatDetails.DebugFormat("Step_3283:; " + _text);
                            }
                        }

                        if (popChange > 0 && newLabors > 0)
                        {
                            while (newLabors > 0 && _colony.Facilities_Total3_Energy > _colony.Facilities_Active3_Energy)
                            {
                                _ = _colony.Facility_Activate(ProductionCategory.Energy);
                                newLabors -= 1;
                                _text = GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                + " > Population growing (now " + curPop + " max. " + maxPop
                                    + " ) - one labor unit was sent to Energy Production"
                                    //+ " at " + GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                    ;

                                //_civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));
                                Console.WriteLine("Step_3284:; Turn " + _turnnumber + ": " + _text);
                            }
                        }

                        if (popChange > 0 && newLabors > 0)
                        {
                            while (newLabors > 0 && _colony.Facilities_Total1_Food > _colony.Facilities_Active1_Food)
                            {
                                _ = _colony.Facility_Activate(ProductionCategory.Food);
                                newLabors -= 1;
                                _text = GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                + " > Population growing (now " + curPop + " max. " + maxPop
                                + " ) - one labor unit was sent to Food Production"
                                //+ " at " + GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name
                                ;

                                //_civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));
                                Console.WriteLine("Step_3285:; Turn " + _turnnumber + ": " + _text);
                            }
                        }

                        int availableLaborUnits = _colony.GetAvailableLabor() / 10;
                        _text = GameEngine.LocationString(_colony.Location.ToString()) /*+ " " + _colony.Name*/
                        + " > Labor Pool: " + availableLaborUnits
                        + " - Food: " + _colony.Facilities_Active1_Food + " / " + _colony.Facilities_Total1_Food
                        + " - Industry: " + _colony.Facilities_Active2_Industry + " / " + _colony.Facilities_Total2_Industry
                        + " - Energy: " + _colony.Facilities_Active3_Energy + " / " + _colony.Facilities_Total3_Energy
                        + " - Research: " + _colony.Facilities_Active4_Research + " / " + _colony.Facilities_Total4_Research
                        + " - Intel: " + _colony.Facilities_Active5_Intelligence + " / " + _colony.Facilities_Total5_Intelligence
                        + " - Pop: " + _colony.Population.CurrentValue + " / " + _colony.Population_Max
                        + "  for " + _colony.Name
                        ;


                        if (_civM.Civilization.CivID == _colony.Owner.CivID)
                        {
                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Brown));
                        }
                        //Console.WriteLine("Step_3287:; Turn " + _turnnumber + ": " + _text);


                        if (_colony.Population.CurrentValue < _colony.Population.Maximum)
                        {
                            ProductionFacilityDesign foodFacilityType = _colony.GetFacilityType(ProductionCategory.Food);
                            if ((foodFacilityType != null) && (_colony.GetAvailableLabor() >= foodFacilityType.LaborCost))
                            {
                                int popInThreeTurns = Math.Min(_colony.Population.Maximum,
                                    (int)(newPopulation * (1 + _colony.GrowthRate) * (1 + _colony.GrowthRate) * (1 + _colony.GrowthRate)));
                                while (popInThreeTurns > _colony.GetProductionOutput(ProductionCategory.Food))
                                {
                                    if (!_colony.Facility_Activate(ProductionCategory.Food))
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        while (_colony.Facility_Activate(ProductionCategory.Industry))
                        {
                            continue;
                        }


                        int healthBonus = (from building in _colony.Buildings
                                           where building.IsActive
                                           from bonus in building.BuildingDesign.Bonuses
                                           where bonus.BonusType == BonusType.PercentPopulationHealth
                                           select bonus.Amount).Sum();
                        //int _healthPlus = _colony.Health; * healthBonus;

                        if (healthBonus > 0)
                        {
                            healthBonus = 1 + (healthBonus / 10);
                            if (healthBonus < 1)
                                healthBonus = 1;
                            _colony.Health.AdjustCurrent(healthBonus);
                            _colony.Health.UpdateAndReset();
                        }

                    }

                    _civM.EnsureSeatOfGovernment();
                }
                catch (Exception e)
                {
                    _text = "Step_3285:; Exception on Do_23_Morale";
                    if (_writeDirectly) Console.WriteLine(_text);
                    GameLog.Core.General.ErrorFormat(_text);
                    GameLog.Core.General.Error(e);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }
            ;
        }
        #endregion

        #region DoResearch() Method
        private void Do_17_Research(GameContext _game)
        {
            string _newline = Environment.NewLine;
            string _text;
            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            //  {
            foreach (Civilization _civ in GameContext.Current.Civilizations)
            {
                if (_civ == null)
                    continue;



                GameContext.PushThreadContext(_game);
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[_civ.CivID];

                //if (_civM_1 == null)
                //    goto NoCivM;

                try
                {
                    //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ.CivID];
                    if (civManager == null)
                        goto NoCivM;

                    int _rp = 2 + civManager.Colonies.Sum(c => c.GetProductionOutput(ProductionCategory.Research));



                    //if (GameContext.Current.TurnNumber / 2 == (float)GameContext.Current.TurnNumber / 2)
                    //{
                    //if (!_civM_1.Civilization.IsHuman) 
                    if (civManager.Civilization.IsHuman)
                    {
                        civManager.Research.Distributions[0].SetValueInternal(0.16f);
                        civManager.Research.Distributions[1].SetValueInternal(0.19f); // these 3 are more important
                        civManager.Research.Distributions[2].SetValueInternal(0.20f);
                        civManager.Research.Distributions[3].SetValueInternal(0.18f);
                        civManager.Research.Distributions[4].SetValueInternal(0.14f);
                        civManager.Research.Distributions[5].SetValueInternal(0.13f);
                    }


                    IEnumerable<Ship> scienceShips = _game.Universe.Find<Ship>(UniverseObjectType.Ship)
                        .Where(s => s.OwnerID == civManager.CivilizationID
                        && s.ShipType == ShipType.Science).ToList();

                    foreach (var item in scienceShips)
                    {
                        try
                        {
                            _rp += ScienceShipsGainResearch(item);
                        }
                        catch
                        {

                        }

                    }

                    _text = _newline + "Step_8766:; " + _civ.Name + " > Research.UpdateResearch"
                        + " with RP= " + _rp
                        + ", before= " + civManager.Research.CumulativePoints

                        ;
                    //if (_writeDirectly) Console.WriteLine(_text);



                    civManager.Research.UpdateResearch(_rp);

                    _text = /*_newline + */"Step_8767:; " + _civ.Name + " > Research.UpdateResearch"
                        + " with RP= " + _rp
                        + ", after= " + civManager.Research.CumulativePoints

                        ;
                    //if (_writeDirectly) Console.WriteLine(_text);

                NoCivM:;
                    //Console.WriteLine("Step_8768:; ---");
                }
                catch (Exception e)
                {
                    _text = "Step_8769: Error on Do_17_Research for " + _civ.Name;
                    //if (_writeDirectly) 
                    Console.WriteLine(_text);
                    GameLog.Core.General.ErrorFormat(_text);
                    GameLog.Core.General.Error(string.Format("Do_17_Research failed for {0}", _civ.Name), e);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }

                //};
                //}
                //catch (Exception e)
                //{
                //    // Check TechObj for correct (formatted) values for ScienceAbility and ScanStrength
                //    GameLog.Core.Research.ErrorFormat(string.Format("##### There was a problem conducting research"), // for {0} {1}",
                //          //scienceShip.ObjectID, scienceShip.Name),
                //          e);
                //}
                //finally
                //{
                //_ = GameContext.PopThreadContext();
                //}
            }
            ;
        }

        private int ScienceShipsGainResearch(Ship scienceShip)
        {
            string _newline = Environment.NewLine;
            string _text;

            //always problems with Science ships because so after it Research breaks
            try
            {

                //_text = "Step_8765:; Do_17_Research... First: each science ship";
                //Console.WriteLine(/*"Step_8765:; " + */_text);

                //IEnumerable<Ship> scienceShips = _game.Universe.Find<Ship>(UniverseObjectType.Ship).Where(s => s.ShipType == ShipType.Science).ToList();

                ////_ = ParallelForEach(scienceShips, scienceShip =>
                //foreach (var scienceShip in scienceShips)
                //{
                //GameContext.PushThreadContext(_game);
                if (scienceShip.Sector.System == null)
                {
                    return 0;
                }
                //GameLog.Core.Research.DebugFormat("{0} {1} is conducting research in {2}...",
                //    scienceShip.ObjectID, scienceShip.Name, scienceShip.Sector);


                CivilizationManager owner = GameContext.Current.CivilizationManagers[scienceShip.Owner];
                StarType starType = scienceShip.Sector.System.StarType;
                if (scienceShip.Location == owner.HomeSystem.Location)
                {
                    return 0;
                    //return;
                }

                int researchGained = (int)(scienceShip.ShipDesign.ScanStrength * scienceShip.ShipDesign.ScienceAbility) + 10;
                researchGained += 1;

                // works GameLog.Core.Research.DebugFormat("Turn {3}: Base research gained for {0} {1} is {2}",
                //scienceShip.ObjectID, scienceShip.Name, researchGained, GameContext.Current.TurnNumber);
                //_text = 
                string _starType = "";
                switch (starType)
                {
                    case StarType.Nebula:
                        researchGained *= 20;  // multiplied with 5
                        _starType = ResourceManager.GetString("STAR_TYPE_NEBULA");
                        break;
                    //// 10 Points for...
                    case StarType.Blue:
                        researchGained *= 4;
                        _starType = ResourceManager.GetString("STAR_TYPE_BLUE");
                        break;
                    case StarType.Orange:
                        researchGained *= 4;
                        _starType = ResourceManager.GetString("STAR_TYPE_ORANGE");
                        break;
                    case StarType.Red:
                        researchGained *= 4;
                        _starType = ResourceManager.GetString("STAR_TYPE_RED");
                        break;
                    case StarType.White:
                        researchGained *= 4;
                        _starType = ResourceManager.GetString("STAR_TYPE_WHITE");
                        break;
                    case StarType.Yellow:
                        researchGained *= 4;
                        _starType = ResourceManager.GetString("STAR_TYPE_Yellow");
                        break;

                    // 15 points for ...
                    case StarType.XRayPulsar:
                        researchGained *= 15;
                        _starType = ResourceManager.GetString("STAR_TYPE_XRAYPULSAR");
                        break;
                    case StarType.RadioPulsar:
                        researchGained *= 15;
                        _starType = ResourceManager.GetString("STAR_TYPE_QUASAR");
                        break;
                    case StarType.NeutronStar:
                        researchGained *= 15;
                        _starType = ResourceManager.GetString("STAR_TYPE_NEUTRONSTAR");
                        break;

                    // 20 points for ...
                    case StarType.BlackHole:
                        researchGained *= 20;
                        _starType = ResourceManager.GetString("STAR_TYPE_BLACK_HOLE");
                        break;
                    case StarType.Quasar:
                        researchGained *= 20;
                        _starType = ResourceManager.GetString("STAR_TYPE_QUASAR");
                        break;

                    // 30 points for ...
                    case StarType.Wormhole:
                        researchGained *= 30;
                        _starType = ResourceManager.GetString("STAR_TYPE_WORMHOLE");
                        break;

                    default:
                        researchGained = 1;
                        break;
                }

                //int gained = 13 - GameContext.Current.CivilizationManagers[scienceShip.Owner].AverageTechLevel;

                //researchGained += 20;  // base value for bigger impact on lower techlevel

                //GameContext.Current.CivilizationManagers[scienceShip.Owner].Research.UpdateResearch(researchGained);
                //_ = GameContext.PopThreadContext();


                //works   GameLog.Core.Research.DebugFormat("{0} {1} gained {2} research points for {3} by studying the {4} in {5}",
                //    scienceShip.ObjectID, scienceShip.Name, researchGained, owner.Civilization.Key, starType, scienceShip.Sector);

                if (researchGained < 2)
                {
                    _text = string.Format(ResourceManager.GetString("SITREP_RESEARCH_SCIENCE_SHIP_RESULT_UNKNOWN"));
                }
                else
                {
                    _text = string.Format(ResourceManager.GetString("SITREP_RESEARCH_SCIENCE_SHIP"),
                  scienceShip.Sector.Location, scienceShip.Name, scienceShip.ObjectID, researchGained, _starType);
                    //{0} > Science Ship {2} {1} gained {3} research points studying this {4}.
                }

                GameContext.Current.CivilizationManagers[owner].SitRepEntries.Add(new
                        ReportEntry_CoS(owner.Civilization, scienceShip.Location, _text, "", "", SitRepPriority.Gray));
                //              GameContext.Current.CivilizationManagers[owner].SitRepEntries.Add(new
                //ScienceShipResearchGainedSitRepEntry(owner.Civilization, scienceShip, researchGained));
                Console.WriteLine("Step_8765:; " + _text + "; Research:CurrentValue= " + owner.Research.CumulativePoints.CurrentValue);

                return researchGained;


            }
            catch (Exception ex)
            {
                _text = "Problem at DoResearchForScienceShip";
                Console.WriteLine("Step_8766:; " + _text + _newline + ex);

                return 0;
            }

            //End of foreach ScienceShip


        }
        //            catch (Exception e)
        //            {
        //                // Check TechObj for correct (formatted) values for ScienceAbility and ScanStrength
        //                GameLog.Core.Research.ErrorFormat(string.Format("##### There was a problem conducting research"), // for {0} {1}",
        //                                                                                                                  //scienceShip.ObjectID, scienceShip.Name),
        //                      e);
        //return 0;
        //            }

        #endregion

        #region DoMapUpdates() Method
        private void Do_24_MapUpdates(GameContext _game)
        {
            Do_24a_SectorClaims(_game);

            GameContext.PushThreadContext(_game);
            string _text;

            SectorMap _map = _game.Universe.Map;

            Task<int[,]> interference = new Task<int[,]>(() =>
            {
                int[,] array = new int[_map.Width, _map.Height];

                GameContext.PushThreadContext(_game);
                try
                {
                    foreach (StarSystem starSystem in _game.Universe.Find(UniverseObjectType.StarSystem).Cast<StarSystem>())
                    {
                        StarHelper.ApplySensorInterference(array, starSystem);
                    }
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }

                return array;
            });

            interference.Start();

            var _col = GameContext.Current.Universe.Objects.OfType<Colony>().ToArray();
            //string _col_name;
            List<string> _col_name_list = new List<string>();

            //for (int i = 0; i < _col.Count(); i++)
            //{
            //    if (_col.Any().ToString() == _col)
            //}

            foreach (Colony col in _col)
            {
                string to_check = col.Location.ToString();
                bool exists = _col_name_list.Any(n => n.ToString() == to_check);
                if (exists)
                {
                    _text = "Step_3459:; ### Problem (doubled) with " + to_check;

                    //if (_writeDirectly_Colony) 
                    Console.WriteLine(_text);
                    col.Name += " I";
                    //_colony_full_Report += _text + _newline;
                }
            }


            //_ = ParallelForEach(_game.Civilizations, _civ =>
            //  {



            foreach (Civilization civ in GameContext.Current.Civilizations)
            {


                GameContext.PushThreadContext(_game);
                try
                {
                    HashSet<MapLocation> fuelLocations = new HashSet<MapLocation>();
                    CivilizationManager civManager = _game.CivilizationManagers[civ];
                    CivilizationMapData mapData = civManager.MapData;

                    mapData.ResetScanStrengthAndFuelRange();
                    //_fleets
                    foreach (Fleet fleet in _game.Universe.FindOwned<Fleet>(civ))
                    {
                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from FLEET {0} {1} ({2}) at {3}, ScanStrength = {4}, Range = {5}", _fleet.ObjectID, _fleet.Name, 
                        //    _fleet.Owner, _fleet.Location, _fleet.ScanStrength, _fleet.SensorRange);
                        mapData.UpgradeScanStrength(
                              fleet.Location,
                              fleet.ScanStrength,
                              fleet.SensorRange,
                              0,
                              1);
                    }
                    /*stations */
                    foreach (Station station in _game.Universe.FindOwned<Station>(civ))
                    {
                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from STATION {0} {1} ({2}) at {3}, ScanStrength = {4}, Range = {5}", station.ObjectID, station.Name, 
                        //    station.Owner, station.Location, station.StationDesign.ScanStrength, station.StationDesign.SensorRange);
                        mapData.UpgradeScanStrength(
                              station.Location,
                              station.StationDesign.ScanStrength,
                              station.StationDesign.SensorRange,
                              0,
                              1);

                        _ = fuelLocations.Add(station.Location);
                        /* stations of other civs we can use to travel */
                        foreach (Civilization whoElse in _game.Civilizations)
                        {
                            List<Civilization> aggreableCivs = (from Civilization in GameContext.Current.Civilizations
                                                                where GameContext.Current.AgreementMatrix.IsAgreementActive(civ, whoElse, ClauseType.TreatyDefensiveAlliance) ||
                                                                      GameContext.Current.AgreementMatrix.IsAgreementActive(civ, whoElse, ClauseType.TreatyFullAlliance) ||
                                                                      GameContext.Current.AgreementMatrix.IsAgreementActive(civ, whoElse, ClauseType.TreatyAffiliation)
                                                                select whoElse).ToList();
                            if (aggreableCivs != null)
                            {
                                foreach (Civilization who in aggreableCivs)
                                {
                                    foreach (Station anotherStation in _game.Universe.FindOwned<Station>(who))
                                    {
                                        if (anotherStation != null)
                                        {
                                            _ = fuelLocations.Add(anotherStation.Location);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (Colony colony in civManager.Colonies)
                    {
                        int scanModifier = 0;

                        IEnumerable<int> scanBonuses = colony.Buildings
                            .Where(o => o.IsActive)
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => o.BonusType == BonusType.ScanRange)
                            .Select(o => o.Amount);

                        if (scanBonuses.Any())
                        {
                            scanModifier = scanBonuses.Max();
                        }

                        //GameLog.Core.MapData.DebugFormat("UpgradeScanStrength from COLONY {0} {1} ({2}) at  {3}, ScanStrength = {4}, Range = {5}", _colony.ObjectID, _colony.Name, 
                        //    _colony.Owner, GameEngine.LocationString(_colony.Location.ToString()), 1 + scanModifier, 1 + scanModifier);  
                        mapData.UpgradeScanStrength(
                              colony.Location,
                              1 + scanModifier,
                              1 + scanModifier,
                              0,
                              1);

                        if (colony.Shipyard != null)
                        {
                            _ = fuelLocations.Add(colony.Location);
                        }
                    }

                    for (int x = 0; x < _map.Width; x++)
                    {
                        for (int y = 0; y < _map.Height; y++)
                        {
                            Sector sector = _map[x, y];

                            foreach (MapLocation fuelLocation in fuelLocations)
                            {
                                mapData.UpgradeFuelRange(
                                    sector.Location,
                                    MapLocation.GetDistance(fuelLocation, sector.Location));
                            }
                        }
                    }

                    mapData.ApplyScanInterference(interference.Result);
                }
                catch (Exception e)
                {
                    GameLog.Core.General.ErrorFormat(string.Format("DoMapUpdate failed for {0}",
                        civ.Name),
                        e);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
                //});
            }
            ;
        }
        #endregion

        #region DoSectorClaims() Method
        private void Do_24a_SectorClaims(GameContext _game)
        {
            SectorMap _map = _game.Universe.Map;
            SectorClaimGrid _sectorClaims = _game.SectorClaims;

            _sectorClaims.ClearClaims();

            //_ = ParallelForEach(GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList(), _civ =>
            foreach (var _civ in GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList())
            {

                //}
                //  {
                GameContext.PushThreadContext(_game);
                try
                {
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

                    foreach (Colony _colony in _civM.Colonies)
                    {
                        int minX = _colony.Location.X;
                        int minY = _colony.Location.Y;
                        int maxX = _colony.Location.X;
                        int maxY = _colony.Location.Y;
                        int radius = Math.Min(_colony.Population.CurrentValue / 100, 3);

                        minX = Math.Max(0, minX - radius);
                        minY = Math.Max(0, minY - radius);
                        maxX = Math.Min(_map.Width - 1, maxX + radius);
                        maxY = Math.Min(_map.Height - 1, maxY + radius);

                        for (int x = minX; x <= maxX; x++)
                        {
                            for (int y = minY; y <= maxY; y++)
                            {
                                MapLocation location = new MapLocation(x, y);

                                int claimWeight = _colony.Population.CurrentValue / (MapLocation.GetDistance(location, _colony.Location) + 1);

                                if (claimWeight <= 0)
                                {
                                    continue;
                                }

                                lock (_sectorClaims)
                                {
                                    _sectorClaims.AddClaim(location, _civ, claimWeight);
                                }

                                _civM.MapData.SetScanned(location, true);
                                /* look for ships in violation of Non_Agression (no go into others space) treaty */
                                foreach (Civilization whoElse in GameContext.Current.Civilizations)
                                {
                                    //if (whoElse == _civ)
                                    //    continue;
                                    if (GameContext.Current.AgreementMatrix.IsAgreementActive(_civ, whoElse, ClauseType.TreatyNonAggression))
                                    {
                                        GameLog.Core.DiplomacyDetails.DebugFormat("*******Looking for NonAggression Treaties*******");
                                        List<Fleet> whosFleets = GameContext.Current.Universe.Find<Fleet>().Where(o => o.Owner == whoElse).ToList();
                                        foreach (Fleet fleet in whosFleets)
                                        {
                                            if (_sectorClaims.GetOwner(fleet.Location) == _civ)
                                            {
                                                GameLog.Core.DiplomacyDetails.DebugFormat("Got NonAggression Treaty for {0} vs {1}, trying for regard trust change and canel treaties", _civ.Key, whoElse.Key);
                                                DiplomacyHelper.ApplyRegardChange(_civ, whoElse, -200);
                                                DiplomacyHelper.ApplyTrustChange(_civ, whoElse, -200);
                                                //var activeAgreements = GameContext.Current.AgreementMatrix[_civ.CivID, whoElse.CivID];
                                                /* cancel all agreements */
                                                //while (activeAgreements.Count > 0)
                                                //{
                                                //    BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);
                                                //}
                                                /* sitrep for canceling all agreements */
                                                //if (_civ.IsEmpire)
                                                //{
                                                //    _civM_1.SitRepEntries.Add(new ViolateTreatySitRepEntry(_civ, whoElse));
                                                //    //_civM_1.SitRepEntries.Add(new ViolateTreatySitRepEntry(whoElse, _civ));
                                                //}
                                                ForeignPower foreignPower = new ForeignPower(_civ, whoElse);
                                                foreignPower.ViolateNonAggression(whoElse);
                                                ForeignPower otherForeignPower = new ForeignPower(whoElse, _civ);
                                                otherForeignPower.ViolateNonAggression(whoElse);
                                            }
                                        }
                                    }
                                    //GameLog.Core.MapData.DebugFormat("{0} (Colony owner: {1}): SetScanned to -> True ", location.ToString(), _colony.Owner);
                                }
                            }
                        }
                    }

                    if (_civ.IsHuman)
                    {
                        _civM.DesiredBorders = new ConvexHullSet(Enumerable.Empty<ConvexHull>());
                    }
                    //PlayerAI.CreateDesiredBorders(_civ);

                }
                catch (Exception e)
                {
                    GameLog.Core.General.ErrorFormat(string.Format("Do_24a_SectorClaims failed for {0}",
                        _civ.Name),
                        e);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }
        }
        #endregion

        #region DoScrapping() Method
        void Do_18_Scrapping(GameContext _game)
        {
            GameContext priorThreadContext = GameContext.ThreadContext;
            GameContext.PushThreadContext(_game);
            try
            {
                foreach (TechObject scrappedObject in _game.Universe.Find<TechObject>().Where(o => o.Scrap))
                {
                    _ = _game.Universe.Scrap(scrappedObject);
                }

                IEnumerable<Colony> colonies = _game.Civilizations
                    .Select(o => _game.CivilizationManagers[o.CivID])
                    .SelectMany(o => o.Colonies);

                foreach (Colony colony in colonies)
                {
                    _ = _game.Universe.ScrapNonStructures(colony);
                }
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }
        }
        #endregion

        #region DoMaintenance() Method
        private void Do_19_Maintenance(GameContext _game)
        {
            string _newline = Environment.NewLine;
            string _text;
            bool _writeDirectly = true;
            //int turn = _game.TurnNumber;
            //_ = turn + 0; // dummy to avoid an unused for turnnumber or _game

            string _civMaintanceText = "";

            foreach (Civilization _civ in GameContext.Current.Civilizations)
            {
                CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];
                foreach (Colony _colony in _civM.Colonies)
                {
                    int _energyPF_unused = _colony.Facilities_Total3_Energy - _colony.GetActiveFacilities(ProductionCategory.Energy);
                    //GameLog.Core.EnergyDetails.DebugFormat(" Turn {0}: {1} Energy Facilities unused at {2} {3} {4} "
                    //    , turnnumber
                    //    , _energyPF_unused
                    //    , _colony.Name
                    //    , _colony.Location
                    //    , _colony.Owner
                    //    );

                    if (_colony.Energy_Net < 0 && _energyPF_unused > 0)
                    {
                        _colony.HandlePF();  // for energy shortage try to increase energy
                    }

                    int _shutdowned = _colony.EnsureEnergyForBuildings();

                    if (_shutdowned > 0)
                    {
                        _text = "Step_3887:; Turn " + GameContext.Current.TurnNumber + " > "
                                + " Energy Shutdown for " + _shutdowned
                                //+ item.ObjectID + " "
                                //+ item.Name + " "
                                + " at " + GameEngine.LocationString(_colony.Location.ToString())
                                + " " + _colony.Name
                                + " " + _colony.Owner

                                ;
                        if (_writeDirectly) Console.WriteLine(_text);
                        _civMaintanceText += _text + _newline;
                        GameLog.Core.EnergyDetails.DebugFormat(_text);
                    }
                }

                int _civMaintance = 0;
                //string _civMaintanceText = "";
                foreach (TechObject item in GameContext.Current.Universe.FindOwned<TechObject>(_civ))
                {

                    if (item.Design.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Ships || item.Design.EncyclopediaCategory == Encyclopedia.EncyclopediaCategory.Stations)
                    {
                        _civMaintance += item.Design.MaintenanceCost;

                        //works
                        _text = "Step_3880:; Turn " + GameContext.Current.TurnNumber + " > "
                            + item.Design.MaintenanceCost + " MaintenanceCost for "
                            + item.ObjectID + " "
                            + item.Name + " "
                            + item.Design + " at "
                            + item.Location + " "
                            + item.Owner
                            ;
                        //if (_writeDirectly) Console.WriteLine(_text);
                        _civMaintanceText += _text + _newline;
                        //GameLog.Core.Production.DebugFormat(_text);
                    }

                    // works
                    //if (_item.Design.MaintenanceCost > 0)
                    //    GameLog.Core.Credits.DebugFormat("Turn {0}: {4} MaintenanceCost for {1} {3} {2} at {5} {6}"
                    //        , GameContext.Current.TurnNumber
                    //        , _item.ObjectID
                    //        , _item.Name
                    //        , _item.Design
                    //    , _item.Design.MaintenanceCost
                    //    , _item.Location
                    //        , _item.Owner
                    //    );
                }

                _ = _civM.Credits.AdjustCurrent(_civMaintance * -1);

                // write > _civMaintanceText
                if (_civM.Civilization.IsHuman)
                {
                    _text = "Step_3286:; Credits > _civMaintance= " + _civMaintance + " for " + _civ.Name;
                    if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.Production.DebugFormat(_text);
                    // --------------

                    Console.WriteLine("Step_3287:; no _output of _civMaintanceText");

                    //Console.WriteLine("Step_3287:; # begin of _civMaintanceText" + _newline + _civMaintanceText
                    //        /*+ _newline*/ + "Step_3287:; # end of _civMaintanceText");

                    //Debugger.Break();
                }

                _civM.MaintenanceCostLastTurn = _civMaintance;
                //_text = _item.Location
                //    + " > BuildProject costs " + _creditsCosts
                //    + " just for reducing credits..."
                //    ;
                //if (_writeDirectly) Console.WriteLine(_text);

                // works, values part of Log of CivsAndRaces
                //GameLog.Core.Credits.DebugFormat("Turn {0}: {3} _civMaintenanceCost for _civ {1} {2} "
                //    , GameContext.Current.TurnNumber
                //    , _civ.CivID
                //    , _civ.Key
                //    , _civMaintance
                //    );

                //foreach station > deuterium ?


            }
        }
        #endregion

        #region DoProduction() Method
        private void Do_21_Production(GameContext _game)
        {
            /*
             * Break down production by civilization.  We want to use resources
             * from both the _colonies and the global reserves, so this is the
             * sensible way to do it.
             */
            int _civsToDo = 0; // for Debug
            _civsToDo = GameContext.Current.Civilizations.Count;
            string _creditsText;
            string _turnNumber = GameContext.Current.TurnNumber.ToString();
            string _newline = Environment.NewLine;
            string _text;
            bool _writeDirectly = true;

            foreach (Civilization _civ in GameContext.Current.Civilizations)
            {
                GameContext.PushThreadContext(_game);

                if (_civ == null)
                    continue;



                //GameLog.Core.Production.DebugFormat("#####################################################");
                //string _gameTurnNumber = GameContext.Current.TurnNumber.ToString();
                _text = "------------------------------------------------------------------------------";
                if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Core.Production.DebugFormat(_text);

                int civOfCivs = GameContext.Current.Civilizations.Count - _civsToDo + 1;

                _text = "Step_4150:; Turn " + GameContext.Current.TurnNumber
                    + ": ################ Do_21_Production for Civs (" + _civsToDo
                    + " to do): ####### > " + _civ.Name
                    //+ " - CivID = " + _civ.CivID
                    + " - " + civOfCivs + " of " + GameContext.Current.Civilizations.Count
                    ;
                Console.WriteLine(_newline + _text);
                //GameLog.Core.Production.DebugFormat(_text);

                _civsToDo -= 1;

                try
                {
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                    if (_civM.SeatOfGovernment == null) // _civ might be sugjected
                    {
                        _text = "Step_4153:; Turn " + GameContext.Current.TurnNumber
                                + ": ################ Do_21_Production for Civs (" + _civsToDo
                                + " to do): ####### > " + _civ.Name
                                //+ " - CivID = " + _civ.CivID
                                + " - " + civOfCivs + " of " + GameContext.Current.Civilizations.Count
                                + " > Civ might be subjected"
                                ;
                        Console.WriteLine(_newline + _text);
                        //GameLog.Core.Production.DebugFormat(_text);
                        continue;
                    }

                    List<Colony> _colonies = new List<Colony>(_civM.Colonies);

                    /*
                     * Update the civilization's treasury and resource stockpile to include anything that was
                     * generated by this _colony. *EVERYTHING* must be add to the global pool prior to checking
                     * for negative and possibly blocking production.
                     */
                    int _newCredits = _colonies.Sum(c => c.TaxCredits);
                    int _newIntelligenceDefense = _colonies.Sum(c => c.Intelligence_Net) * 3 / 10; // 30 % into Defense
                    int _newIntelligenceAttacking = _colonies.Sum(c => c.Intelligence_Net) * 7 / 10; // 70 % into AttackingAccumulation
                    int _newDeuterium = _colonies.Sum(c => c.Deuterium_Net);
                    int _newDilithium = _colonies.Sum(c => c.Dilithium_Net);
                    int _newDuranium = _colonies.Sum(c => c.Duranium_Net);

                    _text = "Yields Empire: "
                        + "Dilithium= " + _newDilithium
                        + ", Deuterium= " + _newDeuterium
                        + ", Duranium= " + _newDuranium
                        ;
                    _civM.SitRepEntries.Add(new ReportEntry_NoAction(_civ, _text, _text, "", SitRepPriority.Gray));

                    // AI gets an advantage
                    if (!_civ.IsHuman)
                        _newCredits *= 2;

                    // Minors get an advantage of 4
                    if (!_civ.IsEmpire)
                        _newCredits *= 2;

                    _ = _civM.Credits.AdjustCurrent(_newCredits);
                    _ = _civM.TotalIntelligenceDefenseAccumulated.AdjustCurrent(_newIntelligenceDefense);
                    _ = _civM.TotalIntelligenceAttackingAccumulated.AdjustCurrent(_newIntelligenceAttacking);
                    _ = _civM.Resources.Deuterium.AdjustCurrent(_newDeuterium);
                    _ = _civM.Resources.Dilithium.AdjustCurrent(_newDilithium);
                    _ = _civM.Resources.Duranium.AdjustCurrent(_newDuranium);

                    _creditsText = _civM
                        + ": Tax= " + _newCredits;

                    _text = "Empire Intelligence Points: "
                            + "Production > " + _civM.TotalIntelligenceProduction
                            + ", available for Attack (press F5) > " + _civM.TotalIntelligenceAttackingAccumulated
                            + ", accum. Defense > " + _civM.TotalIntelligenceDefenseAccumulated
                            + " for " + _civM.Civilization.Name
                            ;
                    _civM.SitRepEntries.Add(new ReportEntry_Show_F5(_civ, _civM.Colonies[0], _text, _text, "", SitRepPriority.Purple));
                    Console.WriteLine("Step_4118:; Turn " + _turnNumber
                        + ": " + _text);
                    //GameLog.Core.ProductionDetails.DebugFormat(_text);

                    _text = "Step_4120:; Turn " + GameContext.Current.TurnNumber + ": "
                        + _civM.Credits.LastChange + " last change, "
                        + _newCredits + " TaxCredits, "
                        + _newDeuterium + " Deut, "
                        + _newDuranium + " Dur, "
                        + _newDilithium + " Dil, "
                        + _newIntelligenceDefense + " IDef, "
                        + _newIntelligenceAttacking + " IAtt, "
                        + "added from all _colonies to " + _civM.Civilization
                        ;
                    if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.ProductionDetails.DebugFormat(_text);

                    //GameLog.Client.ProductionDetails.DebugFormat("Turn {3}: TotalIntelDefenseAccumulated = {1}, TotalIntelAccumulated = {2} for {0}",
                    //    _civM_1.Civilization.Key,
                    //    _civM_1.TotalIntelligenceDefenseAccumulated.CurrentValue,
                    //    _civM_1.TotalIntelligenceAttackingAccumulated.CurrentValue
                    //    , GameContext.Current.TurnNumber
                    //    );

                    //Get the resources available for the civilization
                    ResourceValueCollection totalResourcesAvailable = new ResourceValueCollection
                    {
                        [ResourceType.Deuterium] = _civM.Resources.Deuterium.CurrentValue,
                        [ResourceType.Dilithium] = _civM.Resources.Dilithium.CurrentValue,
                        [ResourceType.Duranium] = _civM.Resources.Duranium.CurrentValue
                    };


                    _text = "Step_4140:; Turn " + GameContext.Current.TurnNumber + ": "
                        + _civM.Credits.LastChange + " last change, "
                        + _civM.Credits.CurrentValue + " Credits, "
                        + _civM.Resources.Deuterium.CurrentValue + " Deut, "
                        + _civM.Resources.Duranium.CurrentValue + " Dur, "
                        + _civM.Resources.Dilithium.CurrentValue + " Dil, "
                        + _civM.TotalIntelligenceDefenseAccumulated + " IDef, "
                        + _civM.TotalIntelligenceAttackingAccumulated + " IAtt, "
                        + "available in TOTAL for " + _civM.Civilization
                        ;
                    if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.ProductionDetails.DebugFormat(_text);

                    /* 
                        * Shuffle the _colonies so they are processed in random order.  This
                        * will help prevent the same _colonies from getting priority when
                        * the global stockpiles are low.
                        */
                    _colonies.RandomizeInPlace();

                    int _coloniesToDo = 0;
                    _coloniesToDo = _colonies.Count;



                    /* Iterate through each _colony */
                    foreach (Colony _colony in _colonies)
                    {
                        //GameEngine.LocationString(_colony.Location.ToString()) = GameEngine.LocationString(_colony.Location.ToString());

                        //foreach (var orb in _colony.OrbitalBatteries)
                        //{
                        //    if (orb.IsActive)
                        //        orb.
                        //    OnPropertyChanged("OrbitalBatteries_Active");
                        //}

                        //_colony.DeOrbitalBatteries_Active.all = 0;
                        //OnPropertyChanged("OrbitalBatteries_Active");

                        _colony.HandlePF();

                        // if morale < 90 and too much credits available, transform credits into morale compensation (on a _colony base)

                        _ = int.TryParse(_colony.Morale.ToString(), out int _morale);
                        _ = int.TryParse(_civM.TotalPopulation.ToString(), out int _pop);
                        bool higherMorale = false;
                        if (_morale < 90)
                        {
                            int _credits2morale = _colony.CreditsEmpire / _pop / 10 * _civM.AverageTechLevel;
                            if (_credits2morale > 100)
                            {
                                _colony.Morale.AdjustCurrent(+1);
                                higherMorale = true;
                            }
                            if (_credits2morale > 200)
                            {
                                _colony.Morale.AdjustCurrent(+1); // another plus 1
                                higherMorale = true;
                            }

                            if (higherMorale)
                            {


                                _text = "Step_4150:; Morale below 90 (actual " + _morale
                                    + " but enough credits available ( " + _colony.CreditsEmpire
                                    + " ) > leads to higher morale"

                                    ;

                                if (_writeDirectly) Console.WriteLine(_text);
                                GameLog.Core.InfoText.DebugFormat(_text);
                                _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Gray));

                                _colony.Morale.UpdateAndReset();
                            }

                        }

                        GameLog.Core.Production.DebugFormat("--------------------------------------------------------------");

                        _text = "Step_4160:; --------------------------------------------------------------" + _newline
                            + "Step_4160:; Turn " + GameContext.Current.TurnNumber
                            + ": " + GameEngine.LocationString(_colony.Location.ToString())
                            + "; " + _civ.Key
                            + " undone _colonies = " + _coloniesToDo
                            + ", last change, " + _civM.Credits.LastChange
                            + ", Credits = " + _civM.Credits
                            + " - Do_21_Production for Colony " + _colony.Name
                            + " (Maint. " + _civM.MaintenanceCostLastTurn  // Shipyard yes ??
                            + " )"
                            ;
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.ProductionDetails.DebugFormat(_text);


                        _coloniesToDo -= 1;  // counting down ... do not double minus like '-= -1'

                        string prevProject = "";
                        // Checking Production

                        //See if there is actually anything to build for this _colony
                        if (!_colony.BuildSlots[0].HasProject && _colony.BuildQueue.IsEmpty())
                        {
                            //_ = _civM_1.Credits.AdjustCurrent(-10);  // 10 Credits consume for doing nothing
                            //_text =
                            //    "Turn " + GameContext.Current.TurnNumber
                            //    + ": Planetary build queue is empty: " + _colony.Name
                            //    + " (" + _civ.Name
                            //    + ") - 10 credits less..."
                            //    ;
                            //Console.WriteLine(GameEngine.LocationString(_colony.Location.ToString()) + ": " + _text);
                            //GameLog.Core.Production.DebugFormat(_text);

                            _text = string.Format(ResourceManager.GetString("SITREP_PLANETARY_BUILD_QUEUE_EMPTY"),
                                _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Orange));
                            _text = "Step_4165:; Turn " + GameContext.Current.TurnNumber + ": " + _text;
                            if (_writeDirectly) Console.WriteLine(_text);

                            continue;
                        }

                        if (!_colony.IsProductionAutomated)
                        {
                            _colony.ClearBuildPrioritiesAndConsolidate();
                        }

                        /* We want to capture the _industry _output available at the _colony. */
                        int _industry = _colony.Industry_Net;

                        //int _shipProduction;  // reducing _industry by 1 / 6 per active _shipyard _slot (max 90%)
                        //if (_colony.Shipyard != null)
                        //{

                        //    //foreach
                        //    List<ShipyardBuildSlot> activatedBuildSlot = _colony.Shipyard.BuildSlots
                        //        .Where(o => o.IsActive).ToList();
                        //        // && !o.HasProject).FirstOrDefault(ShipyardBuildSlot_Deactivate);

                        //    foreach(var _slot in activatedBuildSlot)
                        //    {
                        //        if (_slot.Project != null) _shipProduction += (_industry / 6);
                        //    }

                        //}

                        //_industry -= _shipProduction; // fresh conquered don't have full _industry for 


                        // AI gets an advantage
                        if (!_colony.Owner.IsHuman)
                            _industry *= 2;

                        // Minors get an advantage of 4
                        if (!_colony.Owner.IsEmpire)
                            _industry *= 2;


                        string _currProject = "";

                        int _colonyBuildProject_SameTurn = 0;

                        if (_colony.BuildQueue.IsEmpty())
                        {
                            _text = string.Format(ResourceManager.GetString("SITREP_PLANETARY_BUILD_QUEUE_EMPTY"),
                                _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));

                            _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Orange));

                            _text = "Step_4162:; Turn " + GameContext.Current.TurnNumber + "; " + _text;
                            if (_writeDirectly) Console.WriteLine(_text);
                            //GameLog.Core.Production.DebugFormat(_text);

                            //Colony_Step_65_Handle_Buildings in ColonyAI.cs via Do_0_Turn_Unit
                            //    if (!_colony.Owner.IsHuman && _colony.AvailableLabor > 20)
                            //    {
                            //        if (_colony.FoodReserves < 500)
                            //            _colony.BuildQueue.Add(ProductionFacilityBuildProject)
                            //}

                        }

                        //Start going through the queue
                        while ((_industry > 0) && ((!_colony.BuildQueue.IsEmpty()) || _colony.BuildSlots[0].HasProject))
                        {
                            _colonyBuildProject_SameTurn += 1;
                            //Move the top of the queue in to the build _slot
                            if (!_colony.BuildSlots[0].HasProject)
                            {
                                _colony.ProcessQueue();
                            }

                            _currProject = _colony.Name + ": " + _colony.BuildSlots[0].Project.BuildDesign.Name;
                            if (_currProject == prevProject)
                            {
                                /*//breakpoint*/
                                GameLog.Core.Production.DebugFormat("_currProject == prevProject")
                                    ;
                            }

                            if (_colonyBuildProject_SameTurn > 6)
                                break;


                            //Check to see if the _colony has reached the limit for this building
                            if (TechTreeHelper.IsBuildLimitReached(_colony, _colony.BuildSlots[0].Project.BuildDesign))
                            {
                                GameLog.Core.Production.WarnFormat("Removing {0} from queue on {1} ({2}) - Build Limit Reached", _colony.BuildSlots[0].Project.BuildDesign.Name, _colony.Name, _civ.Name);
                                _colony.BuildSlots[0].Project.Cancel();
                                break;
                            }

                            if (_colony.BuildSlots[0].Project.IsPaused) { break; }
                            //TODO: Not sure how to handle this > break

                            //string _constructionAim = _colony.BuildSlots[0].Project.BuildDesign.Name;

                            _text =
                                "Step_4210:; Turn " + GameContext.Current.TurnNumber
                                + "; " + GameEngine.LocationString(_colony.Location.ToString())
                                + "; " + _colony.Name
                                + "; " + _civ.Name
                                + "; Income TradeR= " + _colony.CreditsFromTrade
                                + "; Tax= " + _colony.TaxCredits
                                + "; last change= " + _civM.Credits.LastChange
                                + "; Deu= " + totalResourcesAvailable[ResourceType.Deuterium]
                                + "; Dur= " + totalResourcesAvailable[ResourceType.Duranium]
                                + "; Dil= " + totalResourcesAvailable[ResourceType.Dilithium]

                                + "; avail. before construct of; " + _colony.BuildSlots[0].Project.BuildDesign.Name
                                //+ " on " + _colony.Name
                                ;
                            if (_writeDirectly) Console.WriteLine(_text);
                            //GameLog.Core.Production.DebugFormat(_text);
                            //GameLog.Core.ProductionDetails.DebugFormat(_newline + "       Turn {8}: Income TradeRoute={4}, Tax={3}, Deuterium={5}, Dilithium={6}, Duranium={7} available for {0} before construction of {1} on {2}" + _newline,
                            //    _civ.Name,
                            //    _colony.BuildSlots[0].Project.BuildDesign.Name,
                            //    _colony.Name,
                            //    _colony.TaxCredits,
                            //    _colony.CreditsFromTrade,
                            //    totalResourcesAvailable[ResourceType.Deuterium],
                            //    totalResourcesAvailable[ResourceType.Dilithium],
                            //    totalResourcesAvailable[ResourceType.Duranium]
                            //    , GameContext.Current.TurnNumber
                            //    );

                            //if (_colony.BuildSlots[0].Project.BuildDesign.Name == "SUBSPACE_JAMMER" && _colony.OwnerID == 0)
                            //    /*Breakpoint*/
                            //    ;
                            //Try to finish the projects
                            if (_colony.BuildSlots[0].Project.IsRushed)
                            {
                                // Rushing a project should have no impact on the _industry of _colony (since it's all been paid for)
                                int tmpIndustry = 3 * _colony.BuildSlots[0].Project.GetCurrentIndustryCost();

                                ResourceValueCollection tmpResources = new ResourceValueCollection
                                {
                                    [ResourceType.Deuterium] = 999999,
                                    [ResourceType.Dilithium] = 999999,
                                    [ResourceType.Duranium] = 999999
                                };

                                int _creditsCosts = _colony.BuildSlots[0].Project.GetTotalCreditsCost();
                                if (_colony.Owner.IsHuman)
                                    _creditsCosts = _creditsCosts * 3 * -1;
                                else
                                    _creditsCosts = _creditsCosts * 1 * -1;

                                _ = _civM.Credits.AdjustCurrent(_creditsCosts);

                                _colony.BuildSlots[0].Project.Advance(ref tmpIndustry, tmpResources);

                                _text = "Step_4230:;"
                                    + " Turn " + GameContext.Current.TurnNumber
                                    + "; " + GameEngine.LocationString(_colony.Location.ToString())
                                    + " BUY: "
                                    + _creditsCosts + " credits applied to "
                                    + _colony.BuildSlots[0].Project.BuildDesign.Name + " on "
                                    + _colony.Name + " ( "
                                    + _civ.Name + " ) "
                                    //+ _civM_1.Credits.LastChange + " last change "
                                    ;
                                if (_writeDirectly) Console.WriteLine(_text);
                                //GameLog.Core.ProductionDetails.DebugFormat(_text);


                            }
                            else
                            {
                                ResourceValueCollection totalResourcesBefore = totalResourcesAvailable.Clone();

                                //cheat (necessary for never ending build projects)
                                if (_industry < 10)
                                {
                                    _industry = 10;
                                }

                                // destroyes last change
                                //int _creditsCosts = 1;
                                //if (_industry > 100)
                                //    _creditsCosts = _industry / 100;
                                //_ = _civM_1.Credits.AdjustCurrent(_creditsCosts);  // each build project has small credit costs
                                //_text = GameEngine.LocationString(_colony.Location.ToString())
                                //    + " > BuildProject costs " + _creditsCosts
                                //    + " just for reducing credits..."
                                //    ;
                                //if (_writeDirectly) Console.WriteLine(_text);
                                //_civM_1.Credits.UpdateAndReset();


                                _colony.BuildSlots[0].Project.Advance(ref _industry, totalResourcesAvailable);

                                int _deuteriumUsed = totalResourcesBefore[ResourceType.Deuterium];
                                int _deuteriumavailable = totalResourcesAvailable[ResourceType.Deuterium];

                                //Figure out how what resources have been used
                                int deuteriumUsed = totalResourcesBefore[ResourceType.Deuterium] - totalResourcesAvailable[ResourceType.Deuterium];
                                int dilithiumUsed = totalResourcesBefore[ResourceType.Dilithium] - totalResourcesAvailable[ResourceType.Dilithium];
                                int duraniumUsed = totalResourcesBefore[ResourceType.Duranium] - totalResourcesAvailable[ResourceType.Duranium];

                                _text = _newline
                                    + "Step_4250:;    Turn " + GameContext.Current.TurnNumber
                                    + ": passing=" + _colonyBuildProject_SameTurn
                                    + _industry + " _industry, "
                                    + deuteriumUsed + " deuterium, "
                                    + dilithiumUsed + " dilithium, "
                                    + duraniumUsed + " duranium applied to project "
                                    + _colony.BuildSlots[0].Project
                                    + " on " + _colony + " " + GameEngine.LocationString(_colony.Location.ToString()) + ";"
                                    + _colony.BuildSlots[0].Project.PercentComplete + " percent done"
                                    ;

                                //_newline + "   Turn {5}: passing={6}, {7} _industry, {0} deuterium, {1} dilithium, {2} duranium applied to project {3} on {4} {9}, {8} percent done" + _newline
                                //, deuteriumUsed, dilithiumUsed, duraniumUsed
                                //, _colony.BuildSlots[0].Project, _colony, GameContext.Current.TurnNumber
                                //, _colonyBuildProject_SameTurn, _industry, _colony.BuildSlots[0].Project.PercentComplete
                                //, GameEngine.LocationString(_colony.Location.ToString())
                                //);
                                //if (_writeDirectly) Console.WriteLine(_text);
                                //GameLog.Core.ProductionDetails.DebugFormat(_text);

                                if (_colony.BuildSlots[0].Project.PercentComplete < 0.01)
                                {
                                    _text = ""
                                        + GameEngine.LocationString(_colony.Location.ToString()) + " " + _colony.Name + " > "
                                        + _colony.BuildSlots[0].Project.BuildDesign.LocalizedName + " - "
                                        + _colony.BuildSlots[0].Project.PercentComplete + " done";

                                    _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_colony.Owner, _colony, _text, _text, "", SitRepPriority.Gray));
                                    Console.WriteLine("Step_4270:; Turn " + _turnNumber + ": " + _text);
                                    //_civM_1.SitRepEntries.Add(new BuildProjectStatusSitRepEntry(_colony.Owner, GameEngine.LocationString(_colony.Location.ToString()), _note, "", "", SitRepPriority.Gray));
                                }

                                _ = _civM.Resources.Deuterium.AdjustCurrent(-1 * deuteriumUsed);
                                _ = _civM.Resources.Dilithium.AdjustCurrent(-1 * dilithiumUsed);
                                _ = _civM.Resources.Duranium.AdjustCurrent(-1 * duraniumUsed);

                                if (_colonyBuildProject_SameTurn > 4)
                                {
                                    _text = GameEngine.LocationString(_colony.Location.ToString())

                                        + " ; " + _colony.Name
                                        + " ; " + _civ.Name
                                        + " forced to be finished > " + _colony.BuildSlots[0].Project.BuildDesign.Name
                                        + " > Construction of " + _colony.BuildSlots[0].Project.BuildDesign.Name
                                        ;
                                    GameLog.Core.ProductionDetails.DebugFormat(_newline + "   Turn {3}: Construction of {0} forced to be finished on {1} ({2})" + _newline
                                       , _colony.BuildSlots[0].Project.BuildDesign.Name, _colony.Name, _civ.Name, GameContext.Current.TurnNumber);
                                    _colony.BuildSlots[0].Project.Finish();
                                    _colony.BuildSlots[0].Project = null;
                                    _colonyBuildProject_SameTurn = 0;
                                    continue;
                                }
                            }

                            if (_colony.BuildSlots[0].Project.IsCompleted)
                            {
                                GameLog.Core.ProductionDetails.DebugFormat(_newline + "   Turn {3}: ############### FINISHED: Construction of {0} finished on {1} ({2})" + _newline
                                    , _colony.BuildSlots[0].Project.BuildDesign.Name, _colony.Name, _civ.Name, GameContext.Current.TurnNumber);
                                _colony.BuildSlots[0].Project.Finish();
                                _colony.BuildSlots[0].Project = null;
                                continue;
                            }
                            //GameLog.Core.Production.DebugFormat(string.Format("Turn {0}: Do_21_Production DONE for {1} ({2})" + _newline + "-----",
                            //    GameContext.Current.TurnNumber, _colony.Name, _civ.Name));
                            //// continue as well if not finish
                            //break;
                        }

                        if (/*!_colony.BuildSlots[0].HasProject && */_colony.BuildQueue.IsEmpty())
                        {
                            //    //_civM_1.SitRepEntries.Add(new BuildQueueEmptySitRepEntry(_civ, _colony, false));
                            //    _text = string.Format(ResourceManager.GetString("SITREP_SHIPYARD_BUILD_QUEUE_EMPTY"),
                            //        _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            //    //if (_writeDirectly) Console.WriteLine(_text);
                            //    //GameLog.Core.Production.DebugFormat(_text);

                            //    //? string.Format(
                            //    //    ResourceManager.GetString("SITREP_SHIPYARD_BUILD_QUEUE_EMPTY"),
                            //    //    Colony.Name, GameEngine.LocationString(_colony.Location.ToString()))
                            //    //: string.Format(
                            //    //ResourceManager.GetString("SITREP_PLANETARY_BUILD_QUEUE_EMPTY"),
                            //    //Colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            //    _civM_1.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Orange));
                            //    //_civM_1.SitRepEntries.Add(new BuildQueueEmptySitRepEntry(_civ, _colony, false));
                        }
                        else
                        {
                            //go on 
                            GameLog.Core.ProductionDetails.DebugFormat(string.Format("Turn {0}: Do_21_Production - BuildQueue*s* not empty for {1} ({2})" + _newline + "-----",
                        GameContext.Current.TurnNumber, _colony.Name, _civ.Name));
                        }
                        // above SitRep added if _colony is finished and empty



                        GameLog.Core.ProductionDetails.DebugFormat(string.Format("Turn {0}: Do_21_Production DONE for {1} ({2})" + _newline + "-----",
                        GameContext.Current.TurnNumber, _colony.Name, _civ.Name));
                        // continue as well if not finish
                        if (_colony.Shipyard != null)
                        {
                            for (int i = 0; i < _colony.BuildSlots.Count; i++)
                            {
                                //_ = _civM_1.Credits.AdjustCurrent(-10);  // for each ship yard build _slot
                                //_text = GameEngine.LocationString(_colony.Location.ToString())
                                //    + " > Shipyard Slot costs 10 credits "
                                //    + " just for reducing credits..."
                                //    ;
                                //if (_writeDirectly) Console.WriteLine(_text);
                                //_civM_1.Credits.UpdateAndReset();  // ..does this crash the credit calculation ??

                                if (_colony.Shipyard.BuildSlots[i].IsActive && !_colony.Shipyard.BuildSlots[i].HasProject)
                                {

                                    _text = string.Format(ResourceManager.GetString("SITREP_SHIPYARD_BUILD_QUEUE_EMPTY"),
                                        _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));

                                    //GameLog.Core.Production.DebugFormat(_text);
                                    _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Crimson));
                                    Console.WriteLine("Step_4289:; Turn " + _turnNumber + "; " + _text);
                                }
                            }
                        }
                        _text = "Step_4290:; Turn " + _turnNumber
                            + "; " + GameEngine.LocationString(_colony.Location.ToString())
                            + "; " + _colony.Name
                            + "; " + _civ.Name
                            + "; Do_21_Production done"
                            //+ "; trying ### > " + _colony.BuildSlots[0].Project.BuildDesign.Name
                            ;
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.Production.DebugFormat(_text);

                        //_constructionAim = "";
                        continue;

                    }// end for each _civ
                }
                catch (Exception e)
                {
                    _text = "Step_4291:; Do_21_Production failed for "
                        + _civ.Name
                    //+ ", trying " + _colony.BuildSlots[0].Project.BuildDesign.Name
                    + _newline + e
                        ;
                    //if (_writeDirectly) 
                    Console.WriteLine(_text);
                    GameLog.Core.Production.Error(_text);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }

        }
        #endregion

        #region DoShipProduction() Method
        private void Do_20_ShipProduction(GameContext _game)
        {

            /*
             * Break down production by civilization.  We want to use resources
             * from both the _colonies and the global reserves, so this is the
             * sensible way to do it.
             */

            string _newline = Environment.NewLine;
            string _text;
            bool _writeDirectly = true;

            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            //  {

            foreach (Civilization _civ in GameContext.Current.Civilizations)
            {
                //if (_civ.Key == "BORG")
                //{
                //    Debugger.Break();
                //}

                GameContext.PushThreadContext(_game);

                try
                {
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];
                    //Get all _colonies with a _shipyard
                    List<Colony> _colonies = _civM.Colonies.Where(c => c.Shipyard != null).ToList();
                    /* 
                     * Shuffle the _colonies so they are processed in random order. This
                     * will help prevent the same _colonies from getting priority when
                     * the global stockpiles are low.
                     */
                    _colonies.RandomizeInPlace();

                    //Get the resources available for the civilization
                    ResourceValueCollection totalResourcesAvailable = new ResourceValueCollection
                    {
                        [ResourceType.Deuterium] = _civM.Resources.Deuterium.CurrentValue,
                        [ResourceType.Dilithium] = _civM.Resources.Dilithium.CurrentValue,
                        [ResourceType.Duranium] = _civM.Resources.Duranium.CurrentValue
                    };

                    foreach (Colony _colony in _colonies)
                    {
                        Shipyard _shipyard = _colony.Shipyard;
                        IList<BuildQueueItem> queue = _shipyard.BuildQueue;

                        //int colonyHealth = Int32.TryParse(_colony.Health.ToString(), out int _health);

                        if (!_colony.Population.IsMaximized && _colony.GrowthRate == 0) // && Int32.TryParse(_colony.Health.ToString(), out int _health) != 100)
                        {
                            _text = string.Format(ResourceManager.GetString("SITREP_GROWTH_BY_HEALTH_UNKNOWN_COLONY_TEXT"), _colony.Name, GameEngine.LocationString(_colony.Location.ToString()));
                            if (GameContext.Current.TurnNumber > 4)
                                _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text, _text, "", SitRepPriority.Yellow));
                            //_civM_1.SitRepEntries.Add(new GrowthByHealthSitRepEntry(_civ, _colony));
                        }

                        // >Check ShipProduction


                        List<ShipyardBuildSlot> _buildSlots = _colony.Shipyard.BuildSlots.Where(s => s.IsActive && !s.OnHold).ToList();
                        foreach (ShipyardBuildSlot _slot in _buildSlots)
                        {
                            //int _ratioIndustry;

                            _text = "Step_4732:; "
                                    + GameEngine.LocationString(_colony.Location.ToString())
                                    + " " + _colony.Name
                                    + " > " + _civ.Name
                                    + " > Do_20_ShipProduction:"
                                    + " Deu= " + totalResourcesAvailable[ResourceType.Deuterium]
                                    + ", Dur= " + totalResourcesAvailable[ResourceType.Duranium]
                                    + ", Dil= " + totalResourcesAvailable[ResourceType.Dilithium]

                                    ;
                            //if (_writeDirectly) Console.WriteLine(_text);

                            //Debugger.Break();

                            // GameLog is making trouble
                            /*GameLog.Core.ShipProduction.DebugFormat("Resources available for {0} before construction of {1} on {2}: Deuterium={3}, Dilithium={4}, Duranium={5}",
                                _civ.Name,
                                _slot.Project.BuildDesign.Name,
                                _colony.Name,
                                totalResourcesAvailable[ResourceType.Deuterium],
                                totalResourcesAvailable[ResourceType.Dilithium],
                                totalResourcesAvailable[ResourceType.Duranium]);
                             */

                            int _ratioIndustryForShipProduction = 0;

                            // active 25 of total 50 = 50 %;
                            if (_colony.Facilities_Total2_Industry > 0)
                            {
                                _ratioIndustryForShipProduction = (100 * _colony.Facilities_Active2_Industry / _colony.Facilities_Total2_Industry) + 1;
                            }

                            int _output = _shipyard.GetBuildOutput(_slot.SlotID) / 100 * _ratioIndustryForShipProduction;
                            while ((_slot.HasProject || !_shipyard.BuildQueue.IsEmpty()) && (_output > 0))
                            {
                                // checking ShipProduction
                                if (!_slot.HasProject)
                                {
                                    //_slot.ProcessQueue();
                                    _shipyard.ProcessQueue();
                                }

                                if (!_slot.HasProject && _shipyard.BuildQueue.IsEmpty())
                                {
                                    string _text1 = GameEngine.LocationString(_colony.Location.ToString())  // needs a new _text here !!!!
                                          + " " + _colony.Name
                                          + " > Shipyard-Slot " + _slot.SlotID
                                          + ":  nothing to do..."
                                          //+ "  is " + _slot.Project.PercentComplete
                                          //+ " complete "
                                          ;

                                    _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text1, _text1, "", SitRepPriority.Gray));

                                    Console.WriteLine("Step_4734:; " + _text1);
                                    //GameLog.Core.ShipProductionDetails.DebugFormat("Nothing to do for Shipyard Slot {0} on {1} ({2})",
                                    //    _slot.SlotID,
                                    //    _colony.Name,
                                    //    _civ.Name);
                                    continue;
                                }

                                ResourceValueCollection totalResourcesBefore = totalResourcesAvailable.Clone();
                                _slot.Project.Advance(ref _output, totalResourcesAvailable);

                                //Figure out how what resources have been used
                                int deuteriumUsed = totalResourcesBefore[ResourceType.Deuterium] - totalResourcesAvailable[ResourceType.Deuterium];
                                int dilithiumUsed = totalResourcesBefore[ResourceType.Dilithium] - totalResourcesAvailable[ResourceType.Dilithium];
                                int duraniumUsed = totalResourcesBefore[ResourceType.Duranium] - totalResourcesAvailable[ResourceType.Duranium];
                                _ = _civM.Resources.Deuterium.AdjustCurrent(-1 * deuteriumUsed);
                                _ = _civM.Resources.Dilithium.AdjustCurrent(-1 * dilithiumUsed);
                                _ = _civM.Resources.Duranium.AdjustCurrent(-1 * duraniumUsed);

                                //_text = 
                                //if (_writeDirectly) Console.WriteLine(_text);

                                //GameLog.Core.ShipProductionDetails.DebugFormat(/*_newline + "       */"Turn {5}: {0} de, {2} du, {1} di applied on {4} ({6}) to {3} " /*+ _newline*/,
                                //    deuteriumUsed, dilithiumUsed, duraniumUsed, _slot.Project, _colony, GameContext.Current.TurnNumber, _colony.Owner);

                                string _text2 = GameEngine.LocationString(_colony.Location.ToString())  // needs a new _text here !!!!
                                + " " + _colony.Name
                                + " > Shipyard-Slot " + _slot.SlotID
                                + " > has " + _slot.Project.ProductionCenter.GetBuildOutput(_slot.SlotID)
                                + " industry output:  " + _slot.Project.BuildDesign
                                + "  is " + _slot.Project.PercentComplete
                                + " complete - "
                                + " used: Dur=" + duraniumUsed
                                + ", Dil=" + dilithiumUsed
                                + ", Deu=" + deuteriumUsed
                                ;

                                _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civ, _colony, _text2, _text2, "", SitRepPriority.GrayDark));

                                Console.WriteLine("Step_4736:; " + _text2 + " (SR)");

                                //if (_civ.Key == "DOMINION")
                                //{
                                //    Debugger.Break();
                                //}

                                if (_slot.Project.IsCompleted)
                                {
                                    _text = "Step_4738:; "
                                            + GameEngine.LocationString(_colony.Location.ToString())
                                            + " " + _colony.Name
                                            + " > " + _civ.Name
                                            + " > Do_20_ShipProduction > Slot= " + _slot.SlotID
                                            + " > " + _slot.Project.BuildDesign

                                            + " is finished"

                                            ;
                                    if (_writeDirectly) Console.WriteLine(_text);

                                    //GameLog.Core.ShipProductionDetails.DebugFormat("Turn {4}: {0} in Shipyard Slot {1} on {2} ({3}) is finished",
                                    //    _slot.Project.BuildDesign,
                                    //    _slot.SlotID,
                                    //    _colony.Name,
                                    //    _civ.Name
                                    //    , GameContext.Current.TurnNumber
                                    //    );
                                    _slot.Project.Finish();
                                    _slot.Project = null;
                                }
                                else
                                {
                                    //if there is a gap for DURANIUM than code would go into never ending loop without the break
                                    //break; >> break kills next Civs
                                    break;
                                    //continue;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _text = "Step_4739:; "
                        //+ GameEngine.LocationString(_colony.Location.ToString())
                        //+ " > " + _colony.Name
                        + " > " + _civ.Name
                        + " Do_20_ShipProduction failed for  " + _civ.Name
                        + _newline + e.Message
                        ;
                    if (_writeDirectly) Console.WriteLine(_text);

                    GameLog.Core.ShipProduction.Error(string.Format("Do_20_ShipProduction failed for {0}", _civ.Name), e);
                    Debugger.Break();
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }
            ;
            //;
        }
        #endregion

        #region DoMorale() Method
        void Do_23_Morale(GameContext _game)
        {
            ConcurrentStack<Exception> _errors = new ConcurrentStack<Exception>();
            string _text;
            bool _writeDirectly = true;
            List<int> _moraleBuildingsID = new List<int>();

            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            //  {
            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                //GameContext.PushThreadContext(_game);
                try
                {
                    int globalMorale = 0;
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[civ.CivID];

                    /* Calculate any empire-wide morale bonuses */
                    foreach (Bonus bonus in _civM.GlobalBonuses)
                    {
                        if (bonus.BonusType == BonusType.MoraleEmpireWide)
                        {
                            globalMorale += bonus.Amount;
                        }
                        // EmpireWide Morale > see CivHistory
                        //_text = "EmpireWide Morale = "
                        //GameLog.Core.Production.DebugFormat(_text);
                    }

                    int _creditsLowerLimit = -5000 + (-3 * _civM.TaxIncome);
                    int _incomeLowerLimit = -1000 - (-2 * _civM.TaxIncome);
                    bool _creditsSitRep = false;

                    _text = "Step_5410:; Turn " + GameContext.Current.TurnNumber
                        + ": _textCreditsLastChange = "
                        + Do_x_Digit_String(5, _civM.Credits.LastChange.ToString())
                        + "  for " + _civM.Civilization.Key
                        ;

                    //Console.WriteLine(_text);


                    if (_civM.Credits.CurrentValue < _creditsLowerLimit)
                    {
                        //globalMorale -= 1;
                        _creditsSitRep = true;
                    }

                    if (_civM.Credits.LastChange < _incomeLowerLimit && _civM.Credits.CurrentValue < _incomeLowerLimit)
                    {
                        //globalMorale -= 1;
                        _creditsSitRep = true;
                    }

                    if (GameContext.Current.TurnNumber > 9 && _creditsSitRep == true)
                    {
                        if (_civM.Civilization.Key != "BORG")
                        {
                            globalMorale -= 1;
                            _text = "Empire: Morale decreased due to deficit of credits"
                            + ": OneTurnLimit= " + _incomeLowerLimit
                            + " (actual " + _civM.Credits.CurrentChange
                            + " )"
                            + "or by TreasuryLimit= " + _creditsLowerLimit
                            + " (actual " + _civM.Credits.CurrentValue + " )"
                            ;
                            Console.WriteLine("Step_5420:; " + _civM.Civilization.Key + ": " + _text);
                            _civM.SitRepEntries.Add(new ReportEntry_NoAction(_civM.Civilization, _text, "", "", SitRepPriority.RedYellow));
                        }
                    }

                    //if (_civM_1.Civilization.Key == "BORG")
                    //{
                    //    _civM_1.
                    //}

                    /* Iterate through each _colony. */
                    foreach (Colony colony in _civM.Colonies)
                    {
                        /* Add the empire-wide morale adjustments. */
                        _ = colony.Morale.AdjustCurrent(globalMorale);

                        if (colony.OriginalOwner != colony.Owner)
                            _ = colony.Morale.AdjustCurrent(-1); // TODO: Malus for Subjageted

                        /* Add any morale bonuses from active buildings at the _colony. */
                        int colonyBonus = (from building in colony.Buildings
                                           where building.IsActive
                                           from bonus in building.BuildingDesign.Bonuses
                                           where bonus.BonusType == BonusType.Morale
                                           select bonus.Amount).Sum();

                        _ = colony.Morale.AdjustCurrent(colonyBonus);

                        // slow down Morale above 120
                        if (colony.Morale.CurrentValue > 120)
                        {
                            _ = colony.Morale.AdjustCurrent(-1);

                            // slow * more * down Morale above 130
                            if (colony.Morale.CurrentValue > 150)
                            {
                                _ = colony.Morale.AdjustCurrent(-1);
                            }
                        }

                        if (colony.Morale.CurrentValue < 70)
                        {
                            _ = colony.Morale.AdjustCurrent(1);

                            _moraleBuildingsID = (List<int>)(from building in colony.Buildings
                                                             where building.IsActive
                                                             from bonus in building.BuildingDesign.Bonuses
                                                             where bonus.BonusType == BonusType.Morale
                                                             select building.ObjectID).ToList()
                                               ;

                            foreach (var objID in _moraleBuildingsID)
                            {
                                var b = GameContext.Current.Universe.Objects[objID] as Building;
                                b.IsActive = false;
                                _text = "Step_5456:; "
                                + b.Location
                                  + b.Sector.Name
                                  + " > "
                                  + b.Name
                                  + "was de-activated due to low morale level"
                                  ;
                                if (_writeDirectly) Console.WriteLine(_text);
                                _civM.SitRepEntries.Add(new ReportEntry_ShowColony(_civM.Civilization, colony, _text, _text, "", SitRepPriority.Red));

                                //if (b.)
                                //{

                                //}
                            }

                        }

                        if (colony.Morale.CurrentValue < 50)
                        {
                            _ = colony.Morale.AdjustCurrent(1);  // another 1 
                        }

                        if (colony.Morale.CurrentValue < 25)
                        {
                            _ = colony.Morale.AdjustCurrent(1);  // another 1 
                        }

                        /*
                         * If morale has not changed in this _colony for any reason, then we will
                         * cause the morale level to drift towards the founding civilization's
                         * base morale level.
                         */
                        if (colony.Morale.CurrentChange == 0)
                        {
                            int drift = 0;
                            Civilization originalCiv = colony.OriginalOwner;

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

                            _ = colony.Morale.AdjustCurrent(drift);
                        }

                        // Health below 50 means morale -1
                        if (colony.Health.CurrentValue < 50)
                            _ = colony.Morale.AdjustCurrent(-1);

                        // limited by health value
                        int moraleByHealth = (colony.Health.CurrentValue * 2) - 10;
                        if (moraleByHealth < 30) moraleByHealth = 30;
                        if (colony.Morale.CurrentValue > moraleByHealth)
                            _ = colony.Morale.AdjustCurrent((colony.Morale.CurrentValue - moraleByHealth) * -1);

                        // lowest level for AI-controlled _colonies
                        if (!colony.Owner.IsHuman && colony.Morale.CurrentValue < 80)
                            colony.Morale.AdjustCurrent(80 - colony.Morale.CurrentValue);

                        if (_civM.Civilization.Key == "BORG")
                        {
                            colony.Morale.CurrentValue = 101;
                        }

                        colony.Morale.UpdateAndReset();
                    }
                }
                catch (Exception e)
                {
                    _text = "Step_5480:; Exception on Do_23_Morale";
                    if (_writeDirectly) Console.WriteLine(_text);
                    GameLog.Core.General.ErrorFormat(_text);
                    _errors.Push(e);
                }
                finally
                {
                    _ = GameContext.PopThreadContext();
                }
            }
            ;
            //});

            if (!_errors.IsEmpty)
            {
                throw new AggregateException(_errors);
            }
        }
        #endregion

        #region DoTrade() Method
        void Do_22_Trade(GameContext _game)
        {
            Table popReqTable = GameContext.Current.Tables.GameOptionTables["TradeRoutePopReq"];
            Table popModTable = GameContext.Current.Tables.GameOptionTables["TradeRoutePopMultipliers"];

            float sourceMod = Number.ParseSingle(popModTable["Source"][0]);
            float targetMod = Number.ParseSingle(popModTable["Target"][0]);

            string _newline = Environment.NewLine;
            string _text2 = "";
            string _text;
            bool _writeDirectly = true;

            //_ = ParallelForEach(GameContext.Current.Civilizations, _civ =>
            foreach (Civilization _civ in GameContext.Current.Civilizations)
            {
                GameContext.PushThreadContext(_game);
                try
                {
                    int popForTradeRoute;
                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                    /*
                     * See what the minimum population level is for a new trade route for the
                     * current civilization.  If one is not specified, use the default.
                     */
                    popForTradeRoute = popReqTable[_civM.Civilization.Key] != null
                        ? Number.ParseInt32(popReqTable[_civM.Civilization.Key][0])
                        : Number.ParseInt32(popReqTable[0][0]);

                    HashSet<Colony> colonies = GameContext.Current.Universe.FindOwned<Colony>(_civ);

                    /* Iterate through each _colony... */
                    foreach (Colony _colony in colonies)
                    {
                        /*
                         * For each established trade route, ensure that the target _colony is
                         * a valid choice.  If it isn't, break it.  Otherwise, calculate the
                         * revised credit total.
                         */
                        foreach (TradeRoute route in _colony.TradeRoutes)
                        {
                            if (!route.IsValidTargetColony(route.TargetColony))
                            {
                                route.TargetColony = null;
                            }
                            /* do not appear to need this as treaties are already checked some place else? */
                            //if (route.TargetColony != null && route.TargetColony.Owner != null)
                            //{
                            //    var targetCiv = route.TargetColony.Owner;
                            //    if (!GameContext.Current.AgreementMatrix.IsAgreementActive(_civ, targetCiv, ClauseType.TreatyDefensiveAlliance) &&
                            //    !GameContext.Current.AgreementMatrix.IsAgreementActive(_civ, targetCiv, ClauseType.TreatyFullAlliance) &&
                            //    !GameContext.Current.AgreementMatrix.IsAgreementActive(_civ, targetCiv, ClauseType.TreatyAffiliation) &&
                            //    !GameContext.Current.AgreementMatrix.IsAgreementActive(_civ, targetCiv, ClauseType.TreatyOpenBorders))
                            //    {
                            //        GameLog.Core.DiplomacyDetails.DebugFormat("!!! NO TRADE ROUTE because no treaty {0} vs {1}", _civ, targetCiv);
                            //        route.TargetColony = null;
                            //    }
                            //}
                            if (route.TargetColony != null)
                            {
                                int sourceIndustry = route.SourceColony.Industry_Net + 1;  // avoiding a zero
                                int targetIndustry = route.TargetColony.Industry_Net + 1;

                                route.Credits = 4 * ((int)((sourceMod * sourceIndustry) + (targetMod * targetIndustry)));  // old 10 *

                            }
                        }

                        /*
                         * Calculate how many trade routes the _colony is allowed to have.
                         * Take into consideration any routes added by building bonuses.
                         */
                        int tradeRoutes = _colony.Population.CurrentValue / popForTradeRoute;

                        tradeRoutes += _colony.Buildings
                            .Where(o => o.IsActive)
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => o.BonusType == BonusType.TradeRoutes)
                            .Sum(o => o.Amount);

                        /*
                         * If the _colony doesn't have as many trade routes as it should, then
                         * we need to add some more.
                         */
                        if (tradeRoutes > _colony.TradeRoutes.Count)
                        {
                            int tradeRouteDeficit = tradeRoutes - _colony.TradeRoutes.Count;
                            for (int i = 0; i < tradeRouteDeficit; i++)
                            {
                                _colony.TradeRoutes.Add(new TradeRoute(_colony));
                            }
                        }

                        /*
                         * If the _colony has too many trade routes, we need to remove some.
                         * To be generous, we sort them in order of credits generated so that
                         * we remove the least valuable routes.
                         */
                        else if (tradeRoutes < _colony.TradeRoutes.Count)
                        {
                            TradeRoute[] extraTradeRoutes = _colony.TradeRoutes
                                .OrderByDescending(o => o.Credits)
                                .SkipWhile((o, i) => i < tradeRoutes)
                                .ToArray();
                            foreach (TradeRoute extraTradeRoute in extraTradeRoutes)
                            {
                                _ = _colony.TradeRoutes.Remove(extraTradeRoute);
                            }
                        }

                        //_text = "Trade-Credits -----------------------" + _newline;
                        //if (_writeDirectly) Console.WriteLine(_text);

                        /*
                         * Iterate through the remaining trade routes and deposit the credit
                         * income into the civilization's treasury.
                         */
                        foreach (TradeRoute route in _colony.TradeRoutes)
                        {
                            _ = _colony.CreditsFromTrade.AdjustCurrent(route.Credits);
                            //GameLog.Core.TradeRoutes.DebugFormat("trade route {0}, route is assigned ={1}", route.SourceColony.Owner, route.IsAssigned);
                            if (!route.IsAssigned) // && _civM_1.SitRepEntries.Any(s=>s.Categories.ToString() == "SpecialEvent"))
                            {
                                //works   GameLog.Core.TradeRoutes.DebugFormat("trade route for {0}, credit {1}=0 should add sitRep", route.SourceColony.Owner, route.SourceColony.CreditsFromTrade.BaseValue);
                                // text: There is an unassigned trade route
                                _text = string.Format(ResourceManager.GetString("SITREP_UNASSIGNED_TRADE_ROUTE"), _colony, GameEngine.LocationString(_colony.Location.ToString()));
                                //_civM_1.SitRepEntries.Add(new UnassignedTradeRoute(route));
                                //Console.WriteLine("SR:; " + _text);
                                //if (_text != null && _text != "" && _text != " ")
                                _civM.SitRepEntries.Add(new ReportEntry_CoS(_colony.Owner, _colony.Location, _text, _text, "", SitRepPriority.Crimson));
                                //_text = "";
                            }
                            else
                            {
                                _text2 = "" /*"Step_5610:; "*/
                                + route.SourceColony.Location
                                + " " + route.SourceColony.Name
                                + " > Income " + route.Credits
                                + " Credits out of trade route to " + route.TargetColony.Name
                                + " " + route.TargetColony.Location
                                ;
                                Console.WriteLine("Step_5610:; " + _text2);

                                if (_text2 != null && _text2 != "" && _text2 != " ")
                                    _civM.SitRepEntries.Add(new ReportEntry_CoS(route.SourceColony.Owner, route.SourceColony.Location, _text2, _text2, "", SitRepPriority.Brown));

                            }
                        }
                        /*
                         * Apply all "+% Trade Income" and "+% Credits" bonuses at this _colony.
                         */
                        int tradeBonuses = (int)_colony.ActiveBuildings
                            .SelectMany(o => o.BuildingDesign.Bonuses)
                            .Where(o => (o.BonusType == BonusType.PercentTradeIncome) || (o.BonusType == BonusType.PercentCredits))
                            .Sum(o => 0.01f * o.Amount);

                        _ = _colony.CreditsFromTrade.AdjustCurrent(tradeBonuses);
                        _ = _civM.Credits.AdjustCurrent(_colony.CreditsFromTrade.CurrentValue);

                        if (tradeBonuses > 0)
                        {
                            _text = "Credits > CreditsFromTrade=" + tradeBonuses;
                            if (_writeDirectly) Console.WriteLine(_text);
                            //GameLog.Core.Production.DebugFormat(_text);
                        }

                        _colony.ResetCreditsFromTrade();
                    }

                    /* 
                     * Apply all global "+% Total Credits" bonuses for the civilization.  At present, we have now
                     * completed all adjustments to the civilization's treasury for this turnnumber.  If that changes in
                     * the future, we may need to move this operation.
                     */
                    int globalBonusAdjustment = (int)(0.01f * _civM.GlobalBonuses
                        .Where(o => o.BonusType == BonusType.PercentTotalCredits)
                        .Sum(o => o.Amount));
                    _ = _civM.Credits.AdjustCurrent(globalBonusAdjustment);

                    if (globalBonusAdjustment > 0)
                    {
                        _text = "Credits > globalBonusAdjustment=" + globalBonusAdjustment;
                        if (_writeDirectly) Console.WriteLine(_text);
                        //GameLog.Core.Production.DebugFormat(_text);
                    }


                    //theCatch:;
                }
                catch (Exception e)
                {
                    _text = "Do_22_Trade failed " + _newline + e;
                    if (_writeDirectly) Console.WriteLine(_text);
                    GameLog.Core.ProductionDetails.DebugFormat(_text);
                    //e);
                }

                finally
                {
                    _ = GameContext.PopThreadContext();
                }

            }
            ;
        }
        #endregion

        #region DoPostTurnOperations() Method
        void Do_25_PostTurnOperations(GameContext game)
        {
            string _text;
            string _newline = Environment.NewLine;
            string _turnnumber = GameContext.Current.TurnNumber.ToString();
            //int turn = _game.TurnNumber;  // Dummy, do not remove
            //DiplomacyHelper.ClearAcceptRejectDictionary(); do this for older turns?
            //IntelHelper.ExecuteIntelOrders(); // now update results of spy operations on host computer, steal and sabotage, remove production facilities, just before we end the turnnumber

            //GameContext.Current.CivilizationManagers[attackedCiv].Credits.AdjustCurrent(stolenCredits * -1);
            //GameContext.Current.CivilizationManagers[attackedCiv].Credits.UpdateAndReset();
            HashSet<Orbital> destroyedOrbitals = GameContext.Current.Universe.Find<Orbital>(o => o.HullStrength.IsMinimized);
            HashSet<Fleet> allFleets = GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet);

            List<CivRank> CivRankList = new List<CivRank>();

            int _globalMorale = 0;

            foreach (Orbital orbital in destroyedOrbitals)
            {
                _ = GameContext.Current.Universe.Destroy(orbital);
            }

            foreach (Fleet fleet in allFleets)
            {
                if (fleet.Ships.Count == 0)
                {
                    fleet.Order?.OnOrderCancelled();
                    _ = GameContext.Current.Universe.Destroy(fleet);
                }
                else fleet.Order?.OnTurnEnding();
            }


            // foreach _civM in GameContext.Current.CivilizationManagers
            foreach (CivilizationManager civManager in GameContext.Current.CivilizationManagers)
            {

                IEnumerable<Ship> allCivShips = GameContext.Current.Universe.Find<Ship>(UniverseObjectType.Ship).Where(o => o.OwnerID == civManager.CivilizationID);

                string civValueShipSummary2 = /*"(" + _civM_1.CivilizationID + "> */"LT-ShipSum2 > "; //All;" + allCivShips.Count();
                string civValueShipSummary1 = /*"(" + _civM_1.CivilizationID + "> */"LT-ShipSum1 > "; //All;" + allCivShips.Count();  // more civil ships
                int _count = 0;
                int _fp = 0;
                int _fpAll = 0;

                IEnumerable<Ship> commandShips = allCivShips.Where(o => o.ShipType == ShipType.Command);
                _count = commandShips.Count();
                if (_count > 0)
                {
                    _fp = commandShips.LastOrDefault().Fire_Power_Orbital * _count;
                    civValueShipSummary2 += "Command " + _count + "x (FP: " + _fp + " ), "; _fpAll += _fp; // if _count = 0 don't show, there are nothing 
                }


                IEnumerable<Ship> cruiserShips = allCivShips.Where(o => o.ShipType == ShipType.Cruiser || o.ShipType == ShipType.HeavyCruiser || o.ShipType == ShipType.StrikeCruiser);
                //civValueShipSummary2 += ";Cru;" + _count = cruiserShips.Count();
                _count = cruiserShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = cruiserShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary2 += "Cruiser " + _count + "x (FP: " + _fp + " )"; _fpAll += _fp; // if _count = 0 >>> show 0 


                IEnumerable<Ship> fastAttackShips = allCivShips.Where(o => o.ShipType == ShipType.FastAttack);
                //civValueShipSummary2 += ";Att;" + _count = fastAttackShips.Count();
                _count = fastAttackShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = fastAttackShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary2 += ", Attack " + _count + "x (FP: " + _fp + " )"; _fpAll += _fp;

                IEnumerable<Ship> scoutShips = allCivShips.Where(o => o.ShipType == ShipType.Scout);
                //civValueShipSummary2 += ";Sco;" + _count = scoutShips.Count();
                _count = scoutShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = scoutShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary2 += ", Scouts " + _count + "x (FP: " + _fp + " )"; _fpAll += _fp;

                IEnumerable<Ship> scienceShips = allCivShips.Where(o => o.ShipType == ShipType.Science);
                //civValueShipSummary2 += ";Sci;" + _count = scienceShips.Count();
                _count = scienceShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = scienceShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                // civValueShipSummary1 ... first line, no semi colon 

                _text = "Science 0x (FP: 0 )";
                if (_count > 0)
                {
                    _text = "Science " + _count + "x (FP: " + _fp + " )";
                }

                civValueShipSummary1 += _text;

                _fpAll += _fp;

                IEnumerable<Ship> spyShips = allCivShips.Where(o => o.ShipType == ShipType.Spy);
                //civValueShipSummary2 += ";Spy;" + _count = spyShips.Count();
                _count = spyShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = spyShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary1 += ", Spy " + _count + "x ";// (FP: " + _fp + ")";

                IEnumerable<Ship> diplomaticShips = allCivShips.Where(o => o.ShipType == ShipType.Diplomatic);
                //civValueShipSummary2 += ";Dip;" + _count = diplomaticShips.Count();
                _count = diplomaticShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = diplomaticShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary1 += ", Diplo " + _count + "x ";// (FP: " + _fp + ")";

                IEnumerable<Ship> medicalShips = allCivShips.Where(o => o.ShipType == ShipType.Medical);
                //civValueShipSummary2 += ";Med;" + _count = medicalShips.Count();
                _count = medicalShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = medicalShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                civValueShipSummary1 += ", Medical " + _count + "x ";// (FP: " + _fp + ")";

                IEnumerable<Ship> transportShips = allCivShips.Where(o => o.ShipType == ShipType.Transport);
                //civValueShipSummary2 += ";Tra;" + _count = transportShips.Count();
                _count = transportShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = transportShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                if (_count > 0)
                {
                    civValueShipSummary1 += ", Transport " + _count + "x ";// (FP: " + _fp + ")";
                }

                IEnumerable<Ship> constructionShips = allCivShips.Where(o => o.ShipType == ShipType.Construction);
                //civValueShipSummary2 += ";Con;" + _count = constructionShips.Count();
                _count = constructionShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = constructionShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                if (_count > 0)
                {
                    civValueShipSummary1 += ", Construction " + _count + "x ";// (FP: " + _fp + ")"; 
                }

                IEnumerable<Ship> colonyShips = allCivShips.Where(o => o.ShipType == ShipType.Colony);
                //civValueShipSummary2 += ";Col;" + _count = colonyShips.Count();
                _count = colonyShips.Count();
                _fp = 0;
                if (_count > 0)
                {
                    _fp = colonyShips.LastOrDefault().Fire_Power_Orbital * _count;
                }
                if (_count > 0)
                {
                    civValueShipSummary1 += ", Colony " + _count + "x ";// (FP: " + _fp + ")";
                }

                civValueShipSummary2 += " - Ships: " + allCivShips.Count() + " - Fire Power Total: " + _fpAll;

                civManager.SitRepEntries.Add(new ReportEntry_ShowGalaxy(civManager.Civilization, civValueShipSummary1, "", "", SitRepPriority.Gray));
                civManager.SitRepEntries.Add(new ReportEntry_ShowGalaxy(civManager.Civilization, civValueShipSummary2, "", "", SitRepPriority.Gray));

            }

            foreach (CivValue civ in CivValueList)
            {
                _text = /*_newline +*/ "CivValueList: " /*+ _civ.AA_CIV_ID*/;
                //_text += _civ.CIV_KEY;
                _text += ";Pop;" + civ.TOT_POP;
                _text += ";MOR;" + civ.MOR;
                _text += ";Cred; " + civ.CRED;
                _text += ";Maint; " + civ.MAINT;

                _text += ";ID;" + civ.AA_CIV_ID;
                _text += ";" + civ.CIV_KEY;
                //_text += _newline + "   " + civValueShipSummary2;
                //_text += _newline + "   " + civValueShipSummary1;


                //if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Core.CivsAndRacesDetails.DebugFormat(_text);

            }

            //HashSet<Station> allStations = GameContext.Current.Universe.Find<Station>(UniverseObjectType.Station);
            //foreach (Station station in allStations)
            //{
            //    CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[station.OwnerID];

            //    string _text = station.Location + " > Station " + station.ObjectID + ": Maint. " + station.Design.MaintenanceCost; ;
            //    _text += " > * " /*+ station.ObjectID + " " */+ station.Name + " *"  /*( Maint." + station.Design.MaintenanceCost + " )"*/;
            //    _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civM_1.Civilization, station.Location, _text, "", "", SitRepPriority.Pink));
            //}

            //foreach (Fleet _fleet in _allFleets)
            //{
            //    foreach (Ship ship in _fleet.Ships)
            //    {
            //        //if (!_fleet.Route.IsEmpty) 
            //        string _text = ship.Location + " > Ship ";
            //        string _design = ship.DesignName + "  ";
            //        while (_design.Length < 25)
            //        {
            //            _design += "_";
            //        }


            //        _text += ship.ObjectID + ": " /*+ " < " */+ _design + " - Maint. " + ship.Design.MaintenanceCost + " > * " + " " + ship.Name + "  * > ";
            //        _text += " " + _fleet.Order;
            //        if (!_fleet.Route.IsEmpty)
            //        {
            //            MapLocation _aim = _fleet.Route.Waypoints.LastOrDefault();
            //            Sector _aimSector = GameContext.Current.Universe.Map[_aim];
            //            //GameContext.Current.Universe.Find<MapLocation>().TryFindFirstItem(o => o == _aim, out Sector _aimSector);
            //            _text += " # on the way to " + _aim.ToString() + " named " + _aimSector.Name;
            //        }

            //        CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[ship.OwnerID];
            //        CivilizationManager PlayerCivManager = GameContext.Current.CivilizationManagers[0];  // Federation - can be changed

            //        // only own civilization
            //        _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civM_1.Civilization, ship.Location, _text, "", "", SitRepPriority.Pink));

            //        // all ships shown
            //        //PlayerCivManager.SitRepEntries.Add(new ShipStatusSitRepEntry(PlayerCivManager.Civilization, ship.Location, _text));
            //    } // end of each ship
            //}

            //HashSet<CivilizationManager> _civManagers = GameContext.Current.CivilizationManagers.ToHashSet();
            //List<CivRank> rankLists = new List<CivRank>();
            foreach (CivilizationManager civ in GameContext.Current.CivilizationManagers)
            {
                _ = int.TryParse(civ.Credits.ToString(), out int _cred);
                _ = int.TryParse(civ.MaintenanceCostLastTurn.ToString(), out int _maint);
                _ = int.TryParse(civ.Research.CumulativePoints.ToString(), out int _research);
                _ = int.TryParse(civ.TotalIntelligenceAttackingAccumulated.ToString(), out int _intelAttack);
                CivRankList.Add(new CivRank(civ.Civilization.Key, _cred, _maint, _research, _intelAttack));

                //);

            }


            int _r_Credits_BestValue;
            int _r_Credits_Average_5;
            int _r_Maint_BestValue;
            int _r_Maint_Average_5;
            int _r_Research_BestValue;
            int _r_Research_Average_5;
            int _r_IntelAttack_BestValue;
            int _r_IntelAttack_Average_5;

            //PrintCivRank(CivRankList);
            // Ranking Credits
            List<CivRank> r_credList = CivRankList.OrderByDescending(o => o.R_CRED).ToList();
            PrintCivRank(r_credList);
            _r_Credits_BestValue = r_credList[0].R_CRED;
            if (r_credList.Count > 6)
            {
                _r_Credits_Average_5 = r_credList[1].R_CRED  // first = [0] already know, so average of place 2 - 6
                    + r_credList[2].R_CRED
                    + r_credList[3].R_CRED
                    + r_credList[4].R_CRED
                    + r_credList[5].R_CRED
                    ;
                _r_Credits_Average_5 /= 5;
            }
            else
            {
                _r_Credits_Average_5 = 99999;
            }
            r_credList.Clear();

            // Ranking Maintenance = Military Power
            List<CivRank> r_maintList = CivRankList.OrderByDescending(o => o.R_MAINT).ToList();
            PrintCivRank(r_maintList);
            _r_Maint_BestValue = r_maintList[0].R_MAINT;
            if (r_maintList.Count > 6)
            {
                _r_Maint_Average_5 = r_maintList[1].R_MAINT  // first = [0] already know, so average of place 2 - 6
                    + r_maintList[2].R_MAINT
                    + r_maintList[3].R_MAINT
                    + r_maintList[4].R_MAINT
                    + r_maintList[5].R_MAINT
                    ;
                _r_Maint_Average_5 /= 5;
            }
            else
            {
                _r_Maint_Average_5 = 99999;
            }
            r_maintList.Clear();

            // Ranking Research Total Points
            List<CivRank> r_researchList = CivRankList.OrderByDescending(o => o.R_RESEARCH).ToList();
            PrintCivRank(r_researchList);
            _r_Research_BestValue = r_researchList[0].R_RESEARCH;
            if (r_researchList.Count > 6)
            {
                _r_Research_Average_5 = r_researchList[1].R_RESEARCH  // first = [0] already know, so average of place 2 - 6
                    + r_researchList[2].R_RESEARCH
                    + r_researchList[3].R_RESEARCH
                    + r_researchList[4].R_RESEARCH
                    + r_researchList[5].R_RESEARCH
                    ;
                _r_Research_Average_5 /= 5;
            }
            else
            {
                _r_Research_Average_5 = 99999;
            }
            r_researchList.Clear();

            // Ranking Intel Attack Power = 70% of Intel Production
            List<CivRank> r_intelAttackList = CivRankList.OrderByDescending(o => o.R_INTEL_ATTACK).ToList();
            PrintCivRank(r_intelAttackList);
            _r_IntelAttack_BestValue = r_intelAttackList[0].R_INTEL_ATTACK;
            if (r_intelAttackList.Count > 6)
            {
                _r_IntelAttack_Average_5 = r_intelAttackList[1].R_INTEL_ATTACK  // first = [0] already know, so average of place 2 - 6
                    + r_intelAttackList[2].R_INTEL_ATTACK
                    + r_intelAttackList[3].R_INTEL_ATTACK
                    + r_intelAttackList[4].R_INTEL_ATTACK
                    + r_intelAttackList[5].R_INTEL_ATTACK
                    ;
                _r_IntelAttack_Average_5 /= 5;
            }
            else
            {
                _r_IntelAttack_Average_5 = 99999;
            }
            r_intelAttackList.Clear();

            //var allCivs = GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet);

            string _allRanking_Intelligence = "";
            foreach (CivilizationManager _civM in GameContext.Current.CivilizationManagers)
            {
                /*
                 * Reset the resource stockpile meters now that we have finished
                 * production for the each civilization.  This will update the
                 * last base value of the meters so that the net change this turnnumber
                 * is properly reflected.  Do the same for the credit treasury.
                 */
                _civM.Resources.UpdateAndReset();
                _civM.Credits.UpdateAndReset();

                //if (_civM_1.)

                _civM.OnTurnBeginn();
                _civM.OnTurnFinished();

                //string civID_and_Turn_Text = _civM_1.civID_and_Turn); 


                _ = int.TryParse(_civM.TotalPopulation.ToString(), out int _totalPopulation);
                _ = int.TryParse(_civM.TotalValue.ToString(), out int _totalValue);
                _ = int.TryParse(_civM.Research.CumulativePoints.ToString(), out int _totalResearch);
                _ = int.TryParse(_civM.TotalIntelligenceProduction.ToString(), out int _totalIProd);
                _ = int.TryParse(_civM.TotalIntelligenceDefenseAccumulated.ToString(), out int _totalIDef);
                _ = int.TryParse(_civM.TotalIntelligenceAttackingAccumulated.ToString(), out int _totalIAtt);

                r_credList = CivRankList.OrderByDescending(o => o.R_CRED).ToList();
                PrintCivRank(r_credList);
                int _rankingCreditsPositon = 1 + r_credList.FindIndex(o => o.CIV_KEY == _civM.Civilization.Key); // +1: 1 = Place 2

                r_maintList = CivRankList.OrderByDescending(o => o.R_MAINT).ToList();
                PrintCivRank(r_maintList);
                int _rankingMaintPositon = 1 + r_maintList.FindIndex(o => o.CIV_KEY == _civM.Civilization.Key); // +1: 1 = Place 2

                r_researchList = CivRankList.OrderByDescending(o => o.R_RESEARCH).ToList();
                PrintCivRank(r_researchList);
                int _rankingResearchPositon = 1 + r_researchList.FindIndex(o => o.CIV_KEY == _civM.Civilization.Key); // +1: 1 = Place 2

                r_intelAttackList = CivRankList.OrderByDescending(o => o.R_INTEL_ATTACK).ToList();
                PrintCivRank(r_intelAttackList);
                int _rankingIntelAttackPositon = 1 + r_intelAttackList.FindIndex(o => o.CIV_KEY == _civM.Civilization.Key); // +1: 1 = Place 2

                //CivHistory = logging civs value from turnnumber 1 (and once making curves like in BotF)

                //CivValue for comparing civs in current turnnumber
                AddCivValue(
                    _civM.CivilizationID
                    , _civM.Civilization.Key
                    , _totalPopulation
                    , _civM.AverageMorale
                    , _totalValue
                    , _civM.Credits.CurrentValue
                    , _civM.MaintenanceCostLastTurn
                    , _totalResearch
                    , _civM.TotalIntelligenceProduction
                    , _rankingCreditsPositon
                    , _rankingMaintPositon
                    //, _rankingResearchPositon
                    //, _rankingIntelAttackPositon
                    );

                _text = "Empire"
                    //+ _civM_1.Civilization.Key
                    + ": Col>" + _civM.Colonies.Count
                    + ", Pop>" + _totalPopulation
                    + "/ Mor>" + _civM.AverageMorale
                    //+ "+" + _civM_1.Credits.CurrentChange

                    + ", Dil> " + _civM.Resources.Dilithium.CurrentValue
                    + " /" + _civM.Resources.Dilithium.LastChange
                    + ", Deu> " + _civM.Resources.Deuterium.CurrentValue
                    + " /" + _civM.Resources.Deuterium.LastChange
                    + ", Dur> " + _civM.Resources.Duranium.CurrentValue
                    + " /" + _civM.Resources.Duranium.LastChange

                    + ", Res> " + _civM.Research.CumulativePoints.LastChange
                    + ", Int> " + _civM.TotalIntelligenceProduction

                    + " , Maint. " + _civM.MaintenanceCostLastTurn
                    + ", Credits> " + _civM.Credits.CurrentValue
                    + " / " + _civM.Credits.LastChange

                    + " "
                    + " for " + _civM.Civilization.Key
                    ;


                //_civM_1.SitRepEntries.Add(new ReportOutput_Purple_CoS_SitRepEntry(_civM_1.Civilization, _civM_1.HomeSystem.Location, _text));
                _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, _civM.HomeSystem.Location, _text, "", "", SitRepPriority.Purple));
                Console.WriteLine("Step_4111:; Turn " + _turnnumber + ": " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

                foreach (Colony col in _civM.Colonies)
                {
                    _text = GameEngine.LocationString(col.Location.ToString()) //+ " " "Colony"
                                                                               //+ _civM_1.Civilization.Key
                                                                               //+ " Colony" /*+ col.Name*/
                    + ": Pop> " + col.Population + " /G " + col.GrowthRate
                    + " /H " + col.Health
                    + " /Mor " + col.Morale

                    + ", Dil> " + col.Dilithium_Net
                    + ", Deu> " + col.Deuterium_Net
                    + ", Dur> " + col.Duranium_Net

                    + ", Res> " + col.Research_Net
                    + ", Int> " + col.Intelligence_Net

                    + ", Ind> " + col.Industry_Net
                    + ", En> " + col.Energy_Net


                    + ", Food> " + col.FoodReserves
                    + " / " + col.FoodReserves.LastChange

                    + "  for " + col.Name
                    ;

                    _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, col.Location, _text, "", "", SitRepPriority.Brown));
                    Console.WriteLine("Step_4112:; Turn " + _turnnumber + " > " + _text);
                    //GameLog.Core.CombatDetails.DebugFormat("Step_4112: " + _text);
                }

                foreach (Bonus bonus in _civM.GlobalBonuses)
                {
                    if (bonus.BonusType == BonusType.MoraleEmpireWide)
                    {
                        _globalMorale += bonus.Amount;
                    }
                    // EmpireWide Morale > see CivHistory
                    //_text = "EmpireWide Morale = "
                    //GameLog.Core.Production.DebugFormat(_text);
                }

                _civM.ZZ_AddCivHist(_civM.CivilizationID
                    , _civM.Civilization.Key

                    , _civM.Credits.CurrentValue
                    , _civM.Credits.LastChange
                    , _civM.MaintenanceCostLastTurn
                    , _civM.Colonies.Count
                    , _totalPopulation
                    , _civM.AverageMorale
                    , _globalMorale
                    , _civM.Resources.Dilithium.CurrentValue
                    , _civM.Resources.Deuterium.CurrentValue
                    , _civM.Resources.Duranium.CurrentValue
                    , _totalValue
                    , _totalResearch
                    , _civM.TotalIntelligenceProduction
                    , _totalIDef
                    , _totalIAtt
                    , _rankingCreditsPositon
                    , _rankingMaintPositon
                    , _rankingResearchPositon
                    , _rankingIntelAttackPositon

                    //, _civM_1.MaintenanceCostLastTurn
                    //, _civM_1.MaintenanceCostLastTurn
                    //, _civM_1.MaintenanceCostLastTurn

                    //, _civM_1.SitRepEntries.ToString()
                    );

                //string _owner = "";
                //if (_civM_1.HomeSystem.Owner != null)
                //{
                //    _owner += _owner; // dummy - do not remove
                //    _owner = _civM_1.HomeSystem.Owner.Key;
                //}

                // works - just for DEBUG  // optimized for CSV-Export (CopyPaste)

                //GameLog.Core.CivsAndRacesDetails.DebugFormat(_newline + "   Turn {0};Col:;{1};Pop:;{2};Morale:;{3};IntelProd;{9};IDef;{11};IAtt;{12};Maint;{10};
                //Credits;{4};Change;{5};Research;{6};Dil;{14};Deut;{15};Dur;{16};{7};for;{8};{13};Owner;{17}" + _newline

                _text =
                      //_newline + "   " + 
                      "Turn:," + GameContext.Current.TurnNumber

                    + ",Research," + _civM.Research.CumulativePoints

                    + ",IntelProd," + _civM.TotalIntelligenceProduction

                    + ",IDef," + _civM.TotalIntelligenceDefenseAccumulated
                    + ",IAtt," + _civM.TotalIntelligenceAttackingAccumulated

                    + ",Dil," + _civM.Resources.Dilithium.CurrentValue
                    + ",Deut," + _civM.Resources.Deuterium.CurrentValue
                    + ",Dur," + _civM.Resources.Duranium.CurrentValue
                    + ",Morale:," + _civM.AverageMorale
                    + ",MoraleGlobal:," + _globalMorale
                    + ",Col:, " + _civM.Colonies.Count
                    + ",Pop:, " + _civM.TotalPopulation

                    + ",Credits, " + _civM.Credits.CurrentValue
                    //+ ",Change," + _civM_1.Credits.CurrentChange  // always 0
                    + ",LT, " + _civM.Credits.LastChange
                    + ",Maint, " + _civM.MaintenanceCostLastTurn

                    + ",for," /*+ _civM_1.Civilization.CivilizationType + ","*/
                    + "," + _civM.Civilization.Key
                    + "," + _civM.CivilizationID
                    //+ _newline
                    ;

                // set back to zero
                _globalMorale = 0;

                Console.WriteLine("Step_3282:; Turn " + _turnnumber + ": " + _text);
                //GameLog.Core.CivsAndRacesDetails.DebugFormat(_text);

                PrintCivRank(CivRankList);
                _text = "Ranking: Credits > " + _civM.Civilization.Name
                    + " = * " + _rankingCreditsPositon
                    + " * = " + _civM.Credits
                    + "  -  Rivals: " + _r_Credits_Average_5
                    + "  -  Best: " + _r_Credits_BestValue

                    ;
                Console.WriteLine("Step_3582:; Turn " + _turnnumber + ": " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3582: " + _text);

                // due to AI has Credit Advantage, no Ranking for Credits reported
                //_civM_1.SitRepEntries.Add(new Report_NoAction(_civM_1.Civilization, _text, "", "", SitRepPriority.Aqua));

                _text = "Ranking: Maint. > " + _civM.Civilization.Name
                    + " = * " + _rankingMaintPositon
                    + " * = " + _civM.MaintenanceCostLastTurn
                    + "  -  Rivals: " + _r_Maint_Average_5
                    + "  -  Best: " + _r_Maint_BestValue

                    ;


                _civM.SitRepEntries.Add(new ReportEntry_NoAction(_civM.Civilization, _text, "", "", SitRepPriority.Aqua));
                Console.WriteLine("Step_3682:; Turn " + _turnnumber + ": " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3682: " + _text);


                _text = "Ranking: Research > " + _civM.Civilization.Name
                    + " = * " + _rankingResearchPositon
                    + " * = " + _civM.Research.CumulativePoints
                    + "  -  Rivals: " + _r_Research_Average_5
                    + "  -  Best: " + _r_Research_BestValue

                    ;


                _civM.SitRepEntries.Add(new ReportEntry_NoAction(_civM.Civilization, _text, "", "", SitRepPriority.Aqua));
                Console.WriteLine("Step_3782:; Turn " + _turnnumber + ": " + _text);
                //GameLog.Core.CombatDetails.DebugFormat("Step_3782:; " + _text);

                _text = "Ranking: Intelligence > " + _civM.Civilization.Name
                    + " = * " + _rankingIntelAttackPositon
                    + " * = " + _civM.TotalIntelligenceAttackingAccumulated
                    + "  -  Rivals: " + _r_IntelAttack_Average_5
                    + "  -  Best: " + _r_IntelAttack_BestValue

                    ;
                Console.WriteLine("Step_3882:; Turn " + _turnnumber + ": " + _text);
                _allRanking_Intelligence += _newline + _text;
                //GameLog.Core.CombatDetails.DebugFormat("Step_3882: " + _text);

                _civM.SitRepEntries.Add(new ReportEntry_NoAction(_civM.Civilization, _text, "", "", SitRepPriority.Aqua));
            }
            Console.WriteLine("Step_3883:; begin of _allRanking_Intelligence"
                + _newline + _allRanking_Intelligence
                + _newline + "end of _allRanking_Intelligence");


            //        foreach (CivilizationManager _civM_1 in GameContext.Current.CivilizationManagers)
            //        {
            //            _text = "Ranking: You"
            //+ _civM_1._civHist_List.
            //;
            //            _civM_1.SitRepEntries.Add(new Report_NoAction(_civM_1.Civilization, _text, "", "", SitRepPriority.BlueDark));
            //        }
            CivRankList.Clear();

            //HashSet<Station> allStations = GameContext.Current.Universe.Find<Station>(UniverseObjectType.Station);
            //foreach (Station station in allStations)
            //{
            //    CivilizationManager _civM = GameContext.Current.CivilizationManagers[station.OwnerID];

            //    _text = GameEngine.LocationString(station.Location.ToString()) + " > Station " + station.ObjectID
            //        + ": " + station.Design
            //        + " ___ - Maint. " + station.Design.MaintenanceCost
            //        + " > * " /*+ station.ObjectID + " " */+ station.Name
            //        + " *"  /*( Maint." + station.Design.MaintenanceCost + " )"*/
            //        + " > since Turn " + station.TurnCreated /*+ " )"*/
            //        ;
            //    Console.WriteLine("Step_3482:; " + _text);
            //    //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

            //    _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, station.Location, _text, "", "", SitRepPriority.Pink));
            //}

            //foreach (Fleet _fleet in allFleets)
            //{
            //    foreach (Ship ship in _fleet.Ships)
            //    {
            //        //if (!_fleet.Route.IsEmpty) 
            //        _text = GameEngine.LocationString(ship.Location.ToString()) + " > Ship ";
            //        string _design = ship.DesignName + "  ";
            //        while (_design.Length < 28)
            //        {
            //            _design += "_";
            //        }


            //        _text += GameEngine.Do_x_Digit_String( 5, (ship.ObjectID.ToString()) + ": " /*+ " < " */+ _design + " - Maint. " + ship.Design.MaintenanceCost + " > * " + " " + ship.Name + "  * > ";
            //        _text += " " + _fleet.Order;
            //        if (!_fleet.Route.IsEmpty)
            //        {
            //            MapLocation _aim = _fleet.Route.Waypoints.LastOrDefault();
            //            Sector _aimSector = GameContext.Current.Universe.Map[_aim];
            //            _fleet.Order = FleetOrders.TravelOrder.Create();
            //            //GameContext.Current.Universe.Find<MapLocation>().TryFindFirstItem(o => o == _aim, out Sector _aimSector);
            //            _text += " # going to " + _aim.ToString() + " named " + _aimSector.Name/* + " (PostTurnOps)"*/;
            //        }

            //        // is here something to do ? 2024-12-29

            //        //if (_fleet.Route.IsEmpty)
            //        //{
            //        //    if (_fleet.Order == FleetOrders.EngageOrder)
            //        //    {
            //        //        _fleet.SetOrder(FleetOrders.IdleOrder);
            //        //    }
            //        //}

            //        CivilizationManager _civM = GameContext.Current.CivilizationManagers[ship.OwnerID];
            //        CivilizationManager PlayerCivManager = GameContext.Current.CivilizationManagers[0];  // Federation - can be changed

            //        // only own civilization
            //        Console.WriteLine("Step_3583:; Turn " + GameContext.Current.TurnNumber + " > " + _text);
            //        //GameLog.Core.CombatDetails.DebugFormat("Step_3282: " + _text);

            //        _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, ship.Location, _text, "", "", SitRepPriority.Pink));

            //        // all ships shown
            //        //PlayerCivManager.SitRepEntries.Add(new ShipStatusSitRepEntry(PlayerCivManager.Civilization, ship.Location, _rep));
            //    } // end of each ship
            //}

            GameContext.Current.TurnNumber++;
            //_tn = GameContext.Current.TurnNumber;
        }

        //private void PrintCivRank(List<CivRank> civRankList)
        //{
        //    throw new NotImplementedException();
        //}

        private void PrintCivRank(List<CivRank> list)
        {
            _ = list.Count.ToString(); // dummy
                                       // not necassary at the moment
                                       //Console.WriteLine(_newline);
                                       //foreach (var _item in list)
                                       //{
                                       //    //    _text = "CivRankList;"
                                       //    //        + _item.CIV_KEY
                                       //    //        + ";" + _item.R_CRED
                                       //    //        + ";" + _item.R_MAINT
                                       //    //        + ";" + _item.R_RESEARCH
                                       //    //        + ";" + _item.R_INTEL_ATTACK
                                       //    //        ;
                                       //    //    if (_writeDirectly) Console.WriteLine(_text);

            //}
        }
        #endregion

        #region DoAIPlayers() Method ... called from SupremacyService.cs
        public void DoAIPlayers(object _gameContext, List<Civilization> _autoTurnCiv)
        {
            string _text;
            string _newline = Environment.NewLine;
            bool _writeDirectly = true;

            ConcurrentStack<Exception> _errors = new ConcurrentStack<Exception>();
            if (!(_gameContext is GameContext _game))
            {
                throw new ArgumentException("_gameContext must be a valid GameContext instance");
            }


            GameContext.PushThreadContext(_game);

            try
            {
                foreach (var _civ in _game.Civilizations)
                {
                    _text = _newline + "Step_9773:; >>>>>>>>>>>  DoAIPlayers for " + _civ.Key + "   " + DateTime.Now
                        + "  called from > SupremacyService.cs";
                    if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.General.Error(e);

                    GameContext.PushThreadContext(_game);

                    CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];

                    //if (_civ.IsHuman)
                    //{
                    //    Debugger.Break();
                    //}


                    //try
                    //{
                    if (_civ.IsHuman && _autoTurnCiv.Count > 0 && !_autoTurnCiv.Contains(_civ))
                    {
                        //continue; //return;
                        _text = "Step_9789:; #### AI-Ship Production as well for human player for TEST-Purpose";
                        if (_writeDirectly) Console.WriteLine(_text);
                    }

                    //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];
                    _civM.DesiredBorders = PlayerAI.CreateDesiredBorders(_civ);

                    //if (_civ.Key == "BORG")
                    //{
                    //    Debugger.Break();
                    //}


                    try
                    {
                        _text = "Step_9781:; next > DiplomatAI.Do_0_Turn_Unit(_civ);";
                        if (_writeDirectly) Console.WriteLine(_text);
                        DiplomatAI.DoTurn(_civ);
                    }
                    catch (Exception e)
                    {
                        _text = "Step_9782:; #### problem at DiplomatAI.Do_0_Turn_Unit" + _newline + e.ToString();
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.General.Error(e);

                        Debugger.Break();
                    }




                    try // ColonyAI.Do_0_Turn_Unit(_civ);
                    {
                        if (DiplomacyHelper.IsIndependent(_civ))
                        {
                            _text = "Step_9781:; next > ColonyAI.Do_0_Turn_Unit(_civ);";
                            if (_writeDirectly) Console.WriteLine(_text);
                            ColonyAI.DoTurn(_civ);
                        }
                    }
                    catch (Exception e)
                    {
                        _text = "Step_9782:; #### problem at DoAIPlayers" + _newline + e.ToString();
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.General.Error(e);

                        Debugger.Break();
                    }

                    try // PlayerAI.Do_0_Turn_Unit(_civ);
                    {
                        if (DiplomacyHelper.IsIndependent(_civ))
                        {
                            _text = "Step_9783:; next > ColonyAI.Do_0_Turn_Unit(_civ);";
                            if (_writeDirectly) Console.WriteLine(_text);


                            PlayerAI.DoTurn(_civ);
                        }
                    }
                    catch (Exception e)
                    {
                        _text = "Step_9784:; #### problem at DoAIPlayers" + _newline + e.ToString();
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.General.Error(e);

                        Debugger.Break();
                    }

                    try // UnitAI.Do_0_Turn_Unit(_civ);
                    {
                        if (DiplomacyHelper.IsIndependent(_civ))
                        {
                            _text = "Step_9785:; next > ColonyAI.Do_0_Turn_Unit(_civ);";
                            if (_writeDirectly) Console.WriteLine(_text);

                            UnitAI.Do_0_Turn_Unit(_civ);
                        }
                    }
                    catch (Exception e)
                    {
                        _text = "Step_9786:; #### problem at DoAIPlayers" + _newline + e.ToString();
                        if (_writeDirectly) Console.WriteLine(_text);
                        GameLog.Core.General.Error(e);

                        Debugger.Break();
                    }

                    //}
                    //catch (Exception e)
                    //{
                    //    _text = "Step_9777:; #### problem at DoAIPlayers" + _newline + e.ToString();
                    //    if (_writeDirectly) Console.WriteLine(_text);
                    //    _errors.Push(e);
                    ////    }
                    ////        finally
                    ////{
                    ////    _ = GameContext.PopThreadContext();
                    ////}

                }
            }


            catch (Exception e)
            {
                _text = "Step_9793:; #### problem at DoAIPlayers" + _newline + e.ToString();
                if (_writeDirectly) Console.WriteLine(_text);
                GameLog.Core.General.Error(e);

                Debugger.Break();
                //}
            }
            finally
            {
                _ = GameContext.PopThreadContext();

                //_soundPlayer = new SoundPlayer("Resources/SoundFX/NewTurn.wav");
                //{
                //    if (File.Exists("Resources/SoundFX/NewTurn.ogg"))
                //    {
                //        SoundPlayer.PlayFile("Resources/SoundFX/NewTurn.ogg");
                //    }
                //}

                if (!_errors.IsEmpty)
                {
                    _text = "Step_5489:; Errors not empty ";
                    if (_writeDirectly) Console.WriteLine(_text);
                    GameLog.Core.CombatDetails.DebugFormat(_text);
                    //
                    Debugger.Break();
                    //throw new AggregateException(_errors);
                }
            }
        }
        #endregion



        #region OnCombatOccurring() Method
        /// <summary>
        /// Raises the <see cref="CombatOccurring"/> event.
        /// </summary>
        /// <param name="_combat">The _combat _assets.</param>
        private void OnCombatOccurring(List<CombatAssets> _combat)
        {
            if (_combat == null)
            {
                throw new ArgumentNullException("_combat");
            }

            bool _writeDirectly = true;

            string _text = "Red Alert at " + _combat[0].Location /*+ " - involved:"*/;
            for (int i = 0; i < _combat.Count(); i++)
            {

                CivilizationManager civManager = GameContext.Current.CivilizationManagers[_combat[i].OwnerID];
                _text += " > " + civManager.Civilization.ShortName + ": ";

                if (_combat[i].CombatShips != null)
                {
                    _text += _combat[i].CombatShips.Count + " armed ship";
                    if (_combat[i].CombatShips.Count > 1)
                        _text += ResourceManager.GetString("PLURAL_S"); // plural s
                }

                if (_combat[i].Station != null)
                {
                    _text += " + 1 Station";
                }
                ;
            }

            // second loop to inform any party 
            for (int i = 0; i < _combat.Count(); i++)
            {
                //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_combat[i].OwnerID];
                //GameContext.Current.CivilizationManagers[_combat[i].OwnerID].SitRepEntries.Add(new ReportOutput_RedYellow_CoS_SitRepEntry(_combat[i].Owner, _combat[i].Location, _text));

                GameContext.Current.CivilizationManagers[_combat[i].OwnerID].SitRepEntries.Add(new ReportEntry_CoS(_combat[i].Owner, _combat[i].Location, _text, "", "", SitRepPriority.RedYellow));
            }

            _text = "Step_0877:; GameEngins.cs > " + _text;
            if (_writeDirectly) Console.WriteLine(_text);

            CombatOccurring?.Invoke(_combat);
            //_text = "Step_0877:; xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx check why Combat screen doesn't close";
            //if (_writeDirectly) Console.WriteLine(_text);


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
            {
                throw new ArgumentNullException("invasionArena");
            }

            InvasionOccurring?.Invoke(invasionArena);
        }
        #endregion

        #region NotifyCombatFinished() Method
        /// <summary>
        /// Resets the _combat wait handle.
        /// </summary>
        public void NotifyCombatFinished()
        {
            _ = CombatReset.Set();
        }
        #endregion



        private static ParallelLoopResult ParallelForEach<TSource>(
            [NotNull] IEnumerable<TSource> source,
            [NotNull] Action<TSource> body)
        {
            return Parallel.ForEach(
                source,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 4 },
                body);
        }

        public void AddCivValue(int civID, string civKey, int TotPop, int Mor, int TotVal, int cred, int maint, int TotRes, int IProd, int r_cred, int r_maint)
        {
            CivValue civValueNew = new CivValue(
                civID
                , civKey
                , TotPop
                , Mor
                , TotVal
                , cred
                , maint
                , TotRes
                , IProd
                , r_cred
                , r_maint
                );
            CivValueList.Add(civValueNew);
        }

        public void AddCivRank(/*int civID, */string civKey/*, int TotPop, int Mor, int TotVal, int cred, int TotRes, int IProd*/, int r_cred, int r_maint, int r_research, int r_intel_attack)
        {
            CivRank civRankNew = new CivRank(
                //civID
                //, 
                civKey
                //, TotPop
                //, Mor
                //, TotVal
                //, cred
                , r_cred
                , r_maint
                , r_research
                , r_intel_attack
                //, TotRes
                //, IProd

                );
            CivRankList.Add(civRankNew);
        }

        public static string DummyCodeComment(string _v) // string = orientated left
        {
            return _v;
        }

        public static string DummyText(string _v) // string = orientated left
        {
            return _v;
        }

        public static string Do_x_String(int _how_many, string _v) // string = orientated left
        {
            //string _out_text = _v.ToString();
            while (_v.Length < _how_many)
            {
                _v = _v + " ";
            }
            return _v;
        }

        public static bool IsCivM_Human_Player(CivilizationManager _civM) // string = orientated left
        {
            if (_civM.Civilization.IsHuman)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static string Do_x_Digit_String(int _how_many, string _v) // digit = orientated right
        {
            while (_v.Length < _how_many)
            {
                _v = " " + _v;
            }
            return _v;
        }

        public static CivilizationManager Get_civM(Civilization civ) //, out CivilizationManager civM)
        {

            CivilizationManager civM = GameContext.Current.CivilizationManagers[civ.CivID];
            return civM;
        }

        public static string LocationString(string _in_text)//, out string _out_text) // changes 1 numeric to 2 numeric
        {
            string _out_text = _in_text/*.ToString()*/;

            string aT = "";
            string bT = "";


            if (_out_text.Length != 8)
            {
                int intComma = _out_text.IndexOf(',');
                aT = _out_text.Substring(1, intComma - 1);
                bT = _out_text.Substring(intComma + 2, 2);

                if (aT.Length == 1) aT = " " + aT;

                bT = bT.Replace(")", "");
                if (bT.Length == 1)
                    bT = " " + bT;

                _out_text = "(" + aT + ", " + bT + ")";
            }

            return _out_text;
        }


        public static string BoolString_x5(string _in_text)//, out string _out_text) // changes 1 numeric to 2 numeric
        {
            if (_in_text == "True") { _in_text = " True"; }

            return _in_text;
        }



        public static string GetTimeString()
        {
            var time = DateTime.Now;
            string Year = time.Year.ToString(); Year = CheckDateString(Year);
            string Month = time.Month.ToString(); Month = CheckDateString(Month);
            string Day = time.Day.ToString(); Day = CheckDateString(Day);
            string Hour = time.Hour.ToString(); Hour = CheckDateString(Hour);
            string Minute = time.Minute.ToString(); Minute = CheckDateString(Minute);
            string Second = time.Second.ToString(); Second = CheckDateString(Second);
            return "Output_" + Year + "_" + Month + "_" + Day + "-" + Hour + "_" + Minute + "_" + Second;
        }
        public static string CheckDateString(string _string)
        {
            if (_string.Length == 1)
                _string = "0" + _string;

            return _string;
        }

        private class ColonyTargetValues
        {
            public ColonyTargetValues(int distance, int DefenseValue)
            {
            }

            //public int Distance { get; set; }
            //public int DefenseValue { get; set; }
        }
    }





    //public void GetAcceptReject(ForeignPower _diplomatForeignPower_Civ2)
    //{
    //    if (_diplomatForeignPower_Civ2.PendingAction == PendingDiplomacyAction.AcceptProposal)
    //        AcceptProposalVisitor.Visit(_diplomatForeignPower_Civ2.LastProposalReceived);
    //    else RejectProposalVisitor.Visit(_diplomatForeignPower_Civ2.LastProposalReceived); 
    //}
}

/// <summary>
/// Defines the turnnumber processing phases used by the _game engine.
/// </summary>
public enum TurnPhase : byte
{
    WaitOnPlayers = 0,
    PreTurnOperations,
    Sabotage,
    // SpyOperations,
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
    // Intelligence,
    Morale,
    MapUpdates,
    PostTurnOperations,
    SendUpdates,
    Diplomacy,
    WaitOnAIPlayers
}

/// <summary>
/// Delegate used for event handlers related to changes in the current turnnumber phase.
/// </summary>
public delegate void TurnPhaseEventHandler(TurnPhase phase);

/// <summary>
/// Delegate used for event handlers related to the initiation of combat.
/// </summary>
public delegate void CombatEventHandler(List<CombatAssets> assets);

/// <summary>
/// Delegate used for event handlers related to the initiation of system _invasions.
/// </summary>
public delegate void InvasionEventHandler(InvasionArena invasionArena);

