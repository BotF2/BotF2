// CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Combat
{
    public delegate void SendCombatUpdateCallback(CombatEngine engine, CombatUpdate update);
    public delegate void NotifyCombatEndedCallback(CombatEngine engine);

    public abstract class CombatEngine
    {

        private bool _battleInOwnTerritory;
        private int _totalFirepower; // looks like _empireStrenths dictionary below
        private double _favorTheBoldMalus;
        private int _fleetAsCommandshipBonus;
        private bool _has20PlusPercentFastAttack;

        public readonly object SyncLockOrders;
        public readonly object SyncLockTargetOnes;
        public readonly object SyncLockTargetTwos;
        protected const double BaseChanceToRetreat = 0.50;
        protected const double BaseChanceToAssimilate = 0.05;
        protected const double BaseChanceToRushFormation = 0.50;
        protected readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        protected readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips;
        protected List<Tuple<CombatUnit, CombatWeapon[]>> _combatShipsTemp; // Update xyz declare temp array done
        protected Tuple<CombatUnit, CombatWeapon[]> _combatStation;
        private readonly int _combatId;
        protected int _roundNumber;
        private bool _runningOrders;
        private bool _runningTargetOne;
        private bool _runningTargetTwo;
        private bool _allSidesStandDown;
        private bool _ready;
        protected readonly List<CombatAssets> _assets;
        private readonly SendCombatUpdateCallback _updateCallback;
        private readonly NotifyCombatEndedCallback _combatEndedCallback;
        private readonly Dictionary<int, CombatOrders> _orders; // locked to evaluate one civ at a time for combat order, key is OwnerID int
        private readonly Dictionary<int, CombatTargetPrimaries> _targetOneByCiv; // like _orders
        private readonly Dictionary<int, CombatTargetSecondaries> _targetTwoByCiv; // like _orders
        protected Dictionary<string, int> _empireStrengths; // string in key of civ and int is total fire power of civ


        public bool BattelInOwnTerritory
        {
            get { return _battleInOwnTerritory; }
            set { _battleInOwnTerritory = value; }
        }


        public int TotalFirepower
        {
            get
            {
                return _totalFirepower;
            }
            set
            {
                _totalFirepower = value;
            }
        }

        public double FavorTheBoldMalus
        {
            get { return _favorTheBoldMalus; }
            set { _favorTheBoldMalus = value; }
        }


        public int FleetAsCommandshipBonus
        {
            get { return _fleetAsCommandshipBonus; }
            set { _fleetAsCommandshipBonus = value; }
        }

        public bool Has20PlusPercentFastAttack
        {
            get { return _has20PlusPercentFastAttack; }
            set { _has20PlusPercentFastAttack = value; }
        }


        public Dictionary<string, int> EmpireStrengths
        {
            get
            {
                GameLog.Core.Combat.DebugFormat("GET EmpireStrengths = {0}", _empireStrengths.ToString());
                return _empireStrengths;
            }
            set
            {
                GameLog.Core.Combat.DebugFormat("SET EmpireStrengths = {0}", value.ToString());
                _empireStrengths = value;
            }
        }

        protected int CombatID
        {
            get { return _combatId; }
        }

        protected bool RunningOrders
        {
            get
            {
                lock (SyncLockOrders)
                {
                    return _runningOrders;
                }
            }
            private set
            {
                lock (SyncLockOrders)
                {
                    _runningOrders = value;
                    if (_runningOrders)
                        _ready = false;
                }
            }
        }

        protected bool RunningTargetOne
        {
            get
            {
                lock (SyncLockOrders)
                {
                    return _runningTargetOne;
                }
            }
            private set
            {
                lock (SyncLockOrders)
                {
                    _runningTargetOne = value;
                    if (_runningTargetOne)
                        _ready = false;
                }
            }
        }

        protected bool RunningTargetTwo
        {
            get
            {
                lock (SyncLockOrders)
                {
                    return _runningTargetTwo;
                }
            }
            private set
            {
                lock (SyncLockOrders)
                {
                    _runningTargetTwo = value;
                    if (_runningTargetTwo)
                        _ready = false;
                }
            }
        }

        public bool IsCombatOver
        {
            get
            {
                if (_allSidesStandDown)
                {
                    return true;
                }
                return (_assets.Count(assets => assets.HasSurvivingAssets) <= 1);
            }
        }

        public bool Ready
        {
            get
            {
                lock (SyncLockOrders)
                {
                    if (RunningOrders || IsCombatOver) // RunningTargetOne || RunningTargetTwo)
                        return false;
                    return _ready;
                }
            }
        }


        protected CombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets");
            }
            if (updateCallback == null)
            {
                throw new ArgumentNullException("updateCallback");
            }
            if (combatEndedCallback == null)
            {
                throw new ArgumentNullException("combatEndedCallback");
            }

            _runningOrders = false;
            _runningTargetOne = false;
            _runningTargetTwo = false;
            _allSidesStandDown = false;
            _combatId = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _assets = assets;
            _updateCallback = updateCallback;
            _combatEndedCallback = combatEndedCallback;
            _orders = new Dictionary<int, CombatOrders>();
            _targetOneByCiv = new Dictionary<int, CombatTargetPrimaries>();
            _targetTwoByCiv = new Dictionary<int, CombatTargetSecondaries>();
            SyncLockOrders = _orders;
            SyncLockTargetOnes = _targetOneByCiv;
            SyncLockTargetTwos = _targetTwoByCiv;

            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();

            foreach (CombatAssets civAssets in _assets.ToList())
            {
                if (civAssets.Station != null)
                {
                    _combatStation = new Tuple<CombatUnit, CombatWeapon[]>(
                        civAssets.Station,
                        CombatWeapon.CreateWeapons(civAssets.Station.Source));

                }
                foreach (CombatUnit shipStats in civAssets.CombatShips)
                {
                    _combatShips.Add(new Tuple<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));

                }
                foreach (CombatUnit shipStats in civAssets.NonCombatShips)
                {
                    _combatShips.Add(new Tuple<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));


                }

            }
        }


        public void SubmitOrders(CombatOrders orders)
        {
            lock (SyncLockOrders) //Lock is the keyword in C# that will ensure one thread is executing a piece of code at one time.
            {
                if (!_orders.ContainsKey(orders.OwnerID))
                {
                    _orders[orders.OwnerID] = orders;
                }

                var outstandingOrders = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_orders)
                {
                    foreach (var civId in _orders.Keys)
                    {
                        outstandingOrders.Remove(civId);
                    }

                    if (outstandingOrders.Count == 0)
                    {
                        _ready = true;
                    }
                }
            }
        }

        public void SubmitTargetOnes(CombatTargetPrimaries targets)
        {
            lock (SyncLockTargetOnes) //Lock is the keyword in C# that will ensure one thread is executing a piece of code at one time.
            {
                if (!_targetOneByCiv.ContainsKey(targets.OwnerID))
                {
                    _targetOneByCiv[targets.OwnerID] = targets;
                }

                var outstandingTargets = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_targetOneByCiv)
                {
                    foreach (var civId in _targetOneByCiv.Keys)
                    {
                        outstandingTargets.Remove(civId);
                    }

                    if (outstandingTargets.Count == 0)
                    {
                        _ready = true;
                    }
                }
            }
        }

        public void SubmitTargetTwos(CombatTargetSecondaries targets)
        {
            lock (SyncLockTargetTwos) //Lock is the keyword in C# that will ensure one thread is executing a piece of code at one time.
            {
                if (!_targetTwoByCiv.ContainsKey(targets.OwnerID))
                {
                    _targetTwoByCiv[targets.OwnerID] = targets;
                }

                var outstandingTargets = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_targetTwoByCiv)
                {
                    foreach (var civId in _targetTwoByCiv.Keys)
                    {
                        outstandingTargets.Remove(civId);
                    }

                    if (outstandingTargets.Count == 0)
                    {
                        _ready = true;
                    }
                }
            }
        }

        public void ResolveCombatRound()
        {
            lock (_orders)
            {
                //RunningOrders = true;
                //lock (_targetOneByCiv)
                //{
                //    RunningTargetOne = true;
                //    lock (_targetTwoByCiv)
                //    {
                        GameLog.Core.Test.DebugFormat("ResolveCombatRound");

                        RunningTargetTwo = true;


                        _assets.ForEach(a => a.CombatID = _combatId);
                        CalculateEmpireStrengths();

                        if ((_roundNumber > 1) || !AllSidesStandDown())
                        {
                            RechargeWeapons();
                            ResolveCombatRoundCore();
                        }
                        if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                        {
                            PerformAssimilation();
                        }
                        GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before PerformRetreat");
                        PerformRetreat();

                        GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before UpdateOrbitals");
                        UpdateOrbitals();

                        //if (!IsCombatOver)
                        //{
                        //    _roundNumber++;
                        //}
                //        _targetTwoByCiv.Clear();
                //    }
                //    _targetOneByCiv.Clear();
                //}

                _orders.Clear();
                if (!IsCombatOver)
                {
                    _roundNumber++;
                }
            }

            SendUpdates();

            GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before RemoveDefeatedPlayers");
            RemoveDefeatedPlayers();

            RunningOrders = false;
            //RunningTargetOne = false;
            //RunningTargetTwo = false;

            if (IsCombatOver)
            {
                GameLog.Core.Test.DebugFormat("ResolveCombatRound - IsCombatOver = TRUE");
                AsyncHelper.Invoke(_combatEndedCallback, this);
            }
        }

        private bool AllSidesStandDown()
        {
            foreach (var civAssets in _assets)
            {
                // Combat ships
                if (civAssets.CombatShips.Select(unit => GetOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    return false;
                }

                // Non-combat ships
                if (civAssets.NonCombatShips.Select(unit => GetOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    return false;
                }

                // Station
                if ((civAssets.Station != null) && (GetOrder(civAssets.Station.Source) == CombatOrder.Engage || GetOrder(civAssets.Station.Source) == CombatOrder.Transports || GetOrder(civAssets.Station.Source) == CombatOrder.Rush || GetOrder(civAssets.Station.Source) == CombatOrder.Formation))
                {
                    return false;
                }

            }

            return true;
        }

        public void SendInitialUpdate()
        {
            SendUpdates();
        }

        private void SendUpdates()
        {
            if (GameContext.Current.Options.GalaxyShape.ToString() == "Cluster-not-now")   // correct value is "Cluster" - just remove "-not-now" to disable Combats (done! and) shown
            {
                GameLog.Core.Combat.Info("Combat is turned off");
                AsyncHelper.Invoke(_combatEndedCallback, this);
                return;

            }
            foreach (var playerAsset in _assets)
            {
                var owner = playerAsset.Owner;
                var friendlyAssets = new List<CombatAssets>();
                var hostileAssets = new List<CombatAssets>();
                var empireStrengths = new Dictionary<Civilization, int>();

                friendlyAssets.Add(playerAsset);

                var CivForEmpireStrength = _assets.Distinct().ToList();
                foreach (var civ in CivForEmpireStrength)
                {
                    //GameLog.Core.Combat.DebugFormat("beginning calculating empireStrengths for {0}", //, current value =  for {0} {1} ({2}) = {3}", civ.Owner.Key);

                    int currentEmpireStrength = 0;

                    foreach (var cs in _assets)  // only combat ships
                    {
                        //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", cs.Owner.Key, civ.Owner.Key);
                        if (cs.Owner.Key == civ.Owner.Key)
                        {
                            //GameLog.Core.Combat.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire {1}", cs.Owner.Key, civ.Owner.ToString());

                            foreach (var ship in cs.CombatShips)
                            {
                                currentEmpireStrength += ship.FirePower;
                                //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                            }

                            if (cs.Station != null)
                                currentEmpireStrength += cs.Station.FirePower;

                        }
                    }

                    empireStrengths.Add(civ.Owner, currentEmpireStrength);
                }

                // just for controlling
                foreach (var civ in empireStrengths)   // the dictionary contains the values
                {
                    GameLog.Core.CombatDetails.DebugFormat("CombatID: {0} - CivForEmpireStrength for Empire {1} = {2}", _combatId, civ.Key, civ.Value);
                }


                foreach (var otherAsset in _assets)
                {
                    if (otherAsset == playerAsset)
                        continue;
                    if (CombatHelper.WillEngage(owner, otherAsset.Owner))
                    {
                        hostileAssets.Add(otherAsset);
                    }
                    else
                    {
                        friendlyAssets.Add(otherAsset);
                    }
                }
                if ((friendlyAssets.Count == 0 || hostileAssets.Count == 0) || (_empireStrengths != null && _empireStrengths.All(e => e.Value == 0)))
                {
                    _allSidesStandDown = true;
                    AsyncHelper.Invoke(_combatEndedCallback, this);   // if hostileAssets = 0 then don't show a combat window and send a "combatEnded"
                    break;
                }


                var update = new CombatUpdate(
                    _combatId,
                    _roundNumber,
                    _allSidesStandDown,
                    owner,
                    playerAsset.Location,
                    friendlyAssets,
                    hostileAssets
                    );

                AsyncHelper.Invoke(_updateCallback, this, update);
            }
        }

        /// <summary>
        /// Remove the assets of defeated players from the combat
        /// </summary>
        private void RemoveDefeatedPlayers()
        {
            for (int i = 0; i < _assets.Count; i++)
            {
                if (!_assets[i].HasSurvivingAssets)
                {
                    _assets.RemoveAt(i--);
                }
            }
        }

        private void UpdateOrbitals()
        {
            _assets.ForEach(a => a.UpdateAllSources());
        }

        /// <summary>
        /// Recharges the weapons of all combat ships
        /// </summary>
        protected void RechargeWeapons()
        {
            if (_combatStation != null)
            {
                foreach (var weapon in _combatStation.Item2)
                {
                    weapon.Recharge();
                }
            }
            foreach (var combatShip in _combatShips)
            {
                foreach (var weapon in combatShip.Item2)
                {
                    weapon.Recharge();
                }
            }
        }

        /// <summary>
        /// Calculates the total strength of each side involved in the combat
        /// </summary>
        /// <returns></returns>
        private void CalculateEmpireStrengths()
        {
            _empireStrengths = new Dictionary<string, int>();
            foreach (var combatShip in _combatShips)
            {
                if (!_empireStrengths.ContainsKey(combatShip.Item1.Owner.Key))
                {
                    _empireStrengths[combatShip.Item1.Owner.Key] = 0;
                }
                _empireStrengths[combatShip.Item1.Owner.Key] += combatShip.Item1.Source.Firepower();
            }
            if (_combatStation != null)
            {
                if (!_empireStrengths.ContainsKey(_combatStation.Item1.Owner.Key))
                {
                    _empireStrengths[_combatStation.Item1.Owner.Key] = 0;
                }
                _empireStrengths[_combatStation.Item1.Owner.Key] += _combatStation.Item1.Source.Firepower();
            }

            foreach (var empire in _empireStrengths)
            {
                GameLog.Core.Combat.DebugFormat("Strength for {0} = {1}", empire.Key, empire.Value);
                //makes crash !!   _empireStrengths.Add(empire.Key, empire.Value);
            }
        }

        /// <summary>
        /// Determines whether the given ship is able to successfully retreat
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        protected bool WasRetreateSuccessful(CombatUnit unit, bool oppositionIsRushing, bool oppositionIsInFormation, bool oppositionIsEngage, bool oppositonIsHailing, bool oppsoitionIsRetreating, bool oppsoitionIsRaidTransports, int weaponRatio)
        {
            int chanceToRetreat = RandomHelper.Random(100);
            int retreatChanceModifier = 0;

            GameLog.Core.Combat.DebugFormat("Calculating retreat for {0} {1}", unit.Source.ObjectID, unit.Source.Name);

            if (oppositionIsInFormation || oppositonIsHailing || oppsoitionIsRetreating) // If you go into formation or hailing or Retreating you are not in position to stop the opposition from retreating                   
            {
                GameLog.Core.Combat.DebugFormat("{0} {1} successfully retreated - opposition was in formation", unit.Source.ObjectID, unit.Source.Name);
                return true;
            }

            if (weaponRatio > 6) // if you outgun the retreater they are less likely to get away
            {
                retreatChanceModifier = -10;
                GameLog.Core.Combat.DebugFormat("Weapon ratio was {0}. - Modifier was {1}", weaponRatio, retreatChanceModifier);
            }
            //else if (weaponRatio > 3)
            //{
            //    retreatChanceModifier = -20;
            //    GameLog.Core.Combat.DebugFormat("Weapon ratio was {0}. -20 modifier", weaponRatio);
            //}
            //else if (weaponRatio > 1)
            //{
            //    retreatChanceModifier = -10;
            //    GameLog.Core.Combat.DebugFormat("Weapon ratio was {0}. -10 modifier", weaponRatio);
            //}
            //else
            //{
            //    retreatChanceModifier = 0;
            //    GameLog.Core.Combat.DebugFormat("Weapon ratio was {0}. 0 modifier", weaponRatio);
            //}
            if (oppositionIsEngage)
            {
                retreatChanceModifier += 15;
                GameLog.Core.Combat.DebugFormat("Opposition is Engage. +15 modifier (now {0})", retreatChanceModifier);
            }

            if (oppositionIsRushing || oppsoitionIsRaidTransports) // if you rush the retreater they are less likely to get away
            {
                retreatChanceModifier += -10;
                GameLog.Core.Combat.DebugFormat("Opposition is rushing. -10 modifier (now {0})", retreatChanceModifier);
            }
            if (_roundNumber > 2)
            {
                retreatChanceModifier += 25;
                GameLog.Core.Combat.DebugFormat("If round is 3 or more. +25 to modifier (now {0})", retreatChanceModifier);
            }

            if (chanceToRetreat <= (BaseChanceToRetreat * 100) + retreatChanceModifier)
            {
                GameLog.Core.Combat.DebugFormat("{0} {1} succesfully retreated", unit.Source.ObjectID, unit.Source.Name);
                return true;
            }
            else
            {
                GameLog.Core.Combat.DebugFormat("{0} {1} failed to retreat", unit.Source.ObjectID, unit.Source.Name);
                return false;
            }
        }

        /// <summary>
        /// Performs the assimilation of ships that have been assimilated
        /// </summary>
        private void PerformAssimilation()
        {
            Civilization borg = GameContext.Current.Civilizations.First(c => c.Name == "Borg");

            foreach (var assets in _assets)
            {
                foreach (var assimilatedShip in assets.AssimilatedShips)
                {
                    var destination = CombatHelper.CalculateRetreatDestination(assets);
                    var ship = (Ship)assimilatedShip.Source;
                    ship.Owner = borg;
                    var newfleet = ship.CreateFleet();
                    newfleet.Location = destination.Location;
                    newfleet.Owner = borg;

                    newfleet.SetOrder(FleetOrders.EngageOrder.Create());
                    if (newfleet.Order == null)
                    {
                        newfleet.SetOrder(FleetOrders.AvoidOrder.Create());
                    }
                    ship.IsAssimilated = true;
                    ship.Scrap = false;
                    newfleet.Name = "Assimilated Assets";

                    GameLog.Core.Combat.DebugFormat("Assimilated Assets: {0} {1}, Owner = {2}, OwnerID = {3}, Fleet.OwnerID = {4}, Order = {5}",
                        ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order);
                }
            }
        }

        /// <summary>
        /// Moves ships that have escaped to their destinations
        /// </summary>
        protected void PerformRetreat()
        {
            try
            {
                GameLog.Core.Combat.DebugFormat("PerformRetreat begins");
                foreach (var assets in _assets)
                {
                    var destination = CombatHelper.CalculateRetreatDestination(assets);

                    if (destination == null)
                    {
                        continue;
                    }

                    foreach (var shipStats in assets.EscapedShips)
                    {
                        ((Ship)shipStats.Source).Fleet.Location = destination.Location;
                        GameLog.Core.Combat.DebugFormat("PerformRetreat: {0} {1} retreats to {2}",
                            ((Ship)shipStats.Source).Fleet.ObjectID, ((Ship)shipStats.Source).Fleet.Name, destination.Location.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Core.Combat.DebugFormat("##### Problem at PerformRetreat" + Environment.NewLine + "{0}", e);
                //((Ship)shipStats.Source).Fleet.ObjectID, ((Ship)shipStats.Source).Fleet.Name, destination.Location.ToString(), e);
            }
        }

        /// <summary>
        /// Returns the <see cref="CombatAssets"/> that belong to the given <see cref="Entities.Civilization"/>
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        protected CombatAssets GetAssets(Civilization owner)
        {
            return _assets.FirstOrDefault(a => a.Owner == owner);
        }

        /// <summary>
        /// Gets the order assigned to the given <see cref="Orbital"/>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected CombatOrder GetOrder(Orbital source)
        {
            try
            {
                //GameLog.Core.Test.DebugFormat("Get Order for {0} owner {1}: -> order = {2}", source, source.Owner, _orders[source.OwnerID].GetOrder(source));
                return _orders[source.OwnerID].GetOrder(source);
            }
            catch //(Exception e)
            {
                if (source.Owner.IsHuman == false)
                    GameLog.Core.Combat.ErrorFormat("Unable to get order for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
                //GameLog.LogException(e);
            }

            GameLog.Core.Combat.DebugFormat("Setting Retreat as fallback order for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
            return CombatOrder.Retreat;
        }

        Civilization borg = new Civilization("BORG");
        protected Civilization GetTargetOne(Orbital source)
        {
            if (_targetOneByCiv == null || _targetOneByCiv.Count() == 0)
            {
                GameLog.Core.Combat.DebugFormat("_targetOneByCiv is null ... returning borg");
                return borg;
            }

            try
            {
                //GameLog.Core.Test.DebugFormat("GetTargetOne for source = {0} {1} {2} is Targetting -> {3}",
                //    source.Owner, source.ObjectID, source.Name, _targetOneByCiv[source.OwnerID].GetTargetOne(source));
                return _targetOneByCiv[source.OwnerID].GetTargetOne(source);
            }
            catch (Exception e)
            {
                 
                GameLog.Core.Test.ErrorFormat("Unable to get target one for source {0} owner {1}, {2}", source, source.Owner, e);
                //GameLog.LogException(e);
            }



            GameLog.Core.Combat.DebugFormat("Setting Borg as fallback target one for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
            //return _targetOneByCiv[source.CivID].GetTargetOne(source);
            return borg;
        }

        protected Civilization GetTargetTwo(Orbital source)
        {
            try
            {
                GameLog.Core.Combat.DebugFormat("Setting Borg as target two for CivID = {0}, Name {1}, {2}", source, source.Name, source);
                return _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);

            }
            catch //(Exception e)
            {
                //GameLog.Core.Combat.ErrorFormat("Unable to get order for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
                //GameLog.LogException(e);
            }

            //GameLog.Core.Combat.DebugFormat("Setting Borg as fallback target two for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
            return borg;
        }

        protected abstract void ResolveCombatRoundCore();
    }
}

