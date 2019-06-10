// CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
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

        public readonly object SyncLock;
        public readonly object SyncLockTargetOnes;
        public readonly object SyncLockTargetTwos;
        protected const double BaseChanceToRetreat = 0.50;
        protected const double BaseChanceToAssimilate = 0.05;
        protected const double BaseChanceToRushFormation = 0.50;
        protected readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        protected readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips;
        protected List<Tuple<CombatUnit, CombatWeapon[]>> _combatShipsTemp; // Update xyz declare temp array done
        protected Tuple<CombatUnit, CombatWeapon[]> _combatStation;
        protected readonly Dictionary<int, Civilization> _targetOneData;
        private readonly int _combatId;
        private int _zeroFirePowers;

        protected int _roundNumber;
        private bool _running;
        private bool _runningTargetOne;
        private bool _runningTargetTwo;
        private bool _allSidesStandDown;
        private bool _ready;
        protected readonly List<CombatAssets> _assets;
        private readonly SendCombatUpdateCallback _updateCallback;
        private readonly NotifyCombatEndedCallback _combatEndedCallback;
        private readonly Dictionary<int, CombatOrders> _orders; // locked to evaluate one civ at a time for combat order, key is OwnerID int
        private readonly Dictionary<int, CombatTargetPrimaries> _targetOneByCiv; // like _orders
        private readonly Dictionary<int, CombatTargetSecondaries> _targetTwoByCiv; 
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

        protected bool Running
        {
            get
            {
                lock (SyncLock)
                {
                    return _running;
                }
            }
            private set
            {
                lock (SyncLock)
                {
                    _running = value;
                    if (_running)
                        _ready = false;
                }
            }
        }

        protected bool RunningTargetOne
        {
            get
            {
                lock (SyncLockTargetOnes)
                {
                    return _runningTargetOne;
                }
            }
            private set
            {
                lock (SyncLockTargetOnes)
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
                lock (SyncLockTargetTwos)
                {
                    return _runningTargetTwo;
                }
            }
            private set
            {
                lock (SyncLockTargetTwos)
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
                if (_roundNumber > 1)
                    return true;
                else
                    return false;
 
                //if (_allSidesStandDown)
                //{
                //    return true;
                //}
                ////friendlyAssets = assets.CombatShips.Count() + assets.NonCombatShips.Count();
                ////if (assets.Station != null)
                ////    friendlyAssets += 1;
                //var countAssets = _assets.Count(); // assets means list of civilizations in combat so .Count is number of civs 
                //var remainingAssets = _assets.Count(assets => assets.HasSurvivingAssets);
                //GameLog.Core.Test.DebugFormat("remaining assets {0} of {1} assets & round number {2} & round >5 for IsCombatOver = true? {3} IsCombatOver",
                //    remainingAssets, countAssets, _roundNumber, (_roundNumber > 3));

                //if (_roundNumber > 6 || remainingAssets <= 1)
                //{
                //    return true;
                //    // for testing, up to 7 rounds or one or zero civs left. This use to be if (remainingAssets <= 1) return true but this was when there had been only 2 sides
                //}
                //GameLog.Core.Test.DebugFormat("remaining assets {0} of {1} assets & round number {2} & round >5 for IsCombatOver = true? {3}",
                //    remainingAssets, countAssets, _roundNumber, (_roundNumber > 3));
                //return false;
                //int friendlyAssets = 0; // ships and stations
                //int hostileAssets = 0;

                //foreach (CombatAssets assets in _friendlyAssets)
                //{
                //    if (assets.HasSurvivingAssets)
                //    {
                //        friendlyAssets = assets.CombatShips.Count() + assets.NonCombatShips.Count();
                //        if (assets.Station != null)
                //            friendlyAssets += 1;
                //        GameLog.Core.Combat.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                //    }
                //}
                ////GameLog.Print("Combat: friendlyAssets(Amount)={0}", friendlyAssets);
                //if (friendlyAssets == 0)
                //{
                //    return true;
                //}

                //foreach (CombatAssets assets in _hostileAssets)
                //{
                //    if (assets.HasSurvivingAssets)
                //    {
                //        hostileAssets = assets.CombatShips.Count() + assets.NonCombatShips.Count();
                //        if (assets.Station != null)
                //            hostileAssets += 1;
                //    }
                //}

                //if (hostileAssets == 0)
                //{
                //    //GameLog.Core.Combat.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                //    return true;
                //}

                //return (hostileAssets == 0 || friendlyAssets == 0);

            }
        }

        public bool Ready
        {
            get
            {
                lock (SyncLock)
                {
                    if (Running || IsCombatOver) // RunningTargetOne || RunningTargetTwo)
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

            _running = false;
            _runningTargetOne = false;
            _runningTargetTwo = false;
            _allSidesStandDown = false;
            _combatId = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _zeroFirePowers = 0;
            //_friendlyAssets = new List<CombatAssets>();
            //_hostileAssets = new List<CombatAssets>();
            _assets = assets;
            _updateCallback = updateCallback;
            _combatEndedCallback = combatEndedCallback;
            _orders = new Dictionary<int, CombatOrders>();
            _targetOneByCiv = new Dictionary<int, CombatTargetPrimaries>();
            _targetTwoByCiv = new Dictionary<int, CombatTargetSecondaries>();
            SyncLock = _orders;
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
            lock (SyncLock) //Lock is the keyword in C# that will ensure one thread is executing a piece of code at one time.
            {
                if (!_orders.ContainsKey(orders.OwnerID))
                {
                    _orders[orders.OwnerID] = orders;
                }

                var outstandingOrders = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_orders)
                {
                    foreach (var civKey in _orders.Keys)
                    {
                        outstandingOrders.Remove(civKey);
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
                    foreach (var civKey in _targetOneByCiv.Keys)
                    {
                        outstandingTargets.Remove(civKey);
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
                Running = true;

                _assets.ForEach(a => a.CombatID = _combatId); // assign combatID for each asset
                CalculateEmpireStrengths();

                if ((_roundNumber > 1) || !AllSidesStandDown())
                {
                    RechargeWeapons();
                    ResolveCombatRoundCore(); // call to AutomatedCombatEngine's CombatResolveCombatRoundCore
                }
                if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                {
                    PerformAssimilation();
                }
                GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before PerformRetreat");
                PerformRetreat();

                GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before UpdateOrbitals");
                UpdateOrbitals();

                if (!IsCombatOver)
                {
                    _roundNumber++;
                }
                _targetTwoByCiv.Clear();
                _targetOneByCiv.Clear();
                _orders.Clear();
            }

            SendUpdates();

            GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - before RemoveDefeatedPlayers");
            RemoveDefeatedPlayers();

            Running = false;
            RunningTargetOne = false;
            RunningTargetTwo = false;

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
                if (civAssets.CombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    return false;
                }

                // Non-combat ships
                if (civAssets.NonCombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    return false;
                }

                // Station
                if ((civAssets.Station != null) && (GetCombatOrder(civAssets.Station.Source) == CombatOrder.Engage || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Transports || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Rush || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Formation))
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
                GameLog.Core.Test.DebugFormat("Combat is turned off");
                AsyncHelper.Invoke(_combatEndedCallback, this);
                return;

            }
            //foreach (var combatent in _combatShips) // now search for destroyed ships
            //{
            //    if (combatent.Item1.IsDestroyed)
            //    {
            //        GameLog.Core.Combat.DebugFormat("Opposition {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
            //        if (combatent.Item1.Source is Ship)
            //        {
            //            var Assets = GetAssets(combatent.Item1.Owner);
            //            if (Assets != null)
            //            {
            //                GameLog.Core.Combat.DebugFormat("Name of Owner = {0}, Assets.CombatShips{1}, Assets.NonCobatShips{2}", Assets.Owner.Name, Assets.CombatShips.Count, Assets.NonCombatShips.Count);

            //                if (!Assets.DestroyedShips.Contains(combatent.Item1))
            //                {
            //                    Assets.DestroyedShips.Add(combatent.Item1);
            //                }
            //                if (combatent.Item1.Source.IsCombatant)
            //                {
            //                    Assets.CombatShips.Remove(combatent.Item1);
            //                }
            //                else
            //                {
            //                    Assets.NonCombatShips.Remove(combatent.Item1);
            //                }
            //            }
            //            else
            //                GameLog.Core.Combat.DebugFormat("Assets Null");

            //        }
            //        continue;
            //    }
            //}
            foreach (var playerAsset in _assets) // _assets is list of assets so one list for our friends and for others
            {
                var owner = playerAsset.Owner;
                var friendlyAssets = new List<CombatAssets>();
                var hostileAssets = new List<CombatAssets>();
                var empireStrengths = new Dictionary<Civilization, int>();

                friendlyAssets.Add(playerAsset); // arbitrary one side or the other is 'friendly' from when there were only two sides in combat

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
                    GameLog.Core.CombatDetails.DebugFormat("######  CombatID: {0} - CivForEmpireStrength for Empire {1} = {2}", _combatId, civ.Key, civ.Value);
                }


                foreach (var otherAsset in _assets)
                {
                    if (otherAsset == playerAsset)
                        continue;
                    if (CombatHelper.WillFightAlongside(owner, otherAsset.Owner))
                    {
                        friendlyAssets.Add(otherAsset);
                        friendlyAssets.Distinct().ToList();
                    }
                    else
                    {
                        hostileAssets.Add(otherAsset);
                        hostileAssets.Distinct().ToList();
                    }
                }

                foreach (var firePower in empireStrengths)
                {
                    if (firePower.Value == 0)
                    {
                        _zeroFirePowers += 1;
                    }
                }

                if (friendlyAssets.Count() == 0 || hostileAssets.Count() == 0 || _zeroFirePowers >= 1)//(_empireStrengths != null && _empireStrengths.All(e => e.Value == 0)))                
                {
                    _allSidesStandDown = true;
                    AsyncHelper.Invoke(_combatEndedCallback, this);
                    // if hostileAssets = 0 then don't show a combat window and send a "combatEnded"
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
                // sends data back to combat window
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
            //if (oppositionIsInFormation || oppositonIsHailing || oppsoitionIsRetreating)
            //    return true;

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
        protected CombatOrder GetCombatOrder(Orbital source)
        {
            var _localOrder = new CombatOrder();
            _localOrder = CombatOrder.Engage;
            try
            {
                GameLog.Core.Combat.DebugFormat("Try Get Order for {0} owner {1}: -> order = {2}", source, source.Owner, _orders[source.OwnerID].GetOrder(source));
                _localOrder = _orders[source.OwnerID].GetOrder(source);
                return _localOrder; // this is the class CombatOrder.BORG (or FEDERATION or.....) that comes from public GetCombatOrder() in CombatOrders.cs
            }
            catch //(Exception e)
            {
                if (source.Owner.IsHuman == false)
                    GameLog.Core.Combat.ErrorFormat("Unable to get order for {0} {1} ({2}) Owner: {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
                //GameLog.LogException(e);
            }

            GameLog.Core.CombatDetails.DebugFormat("Setting order for {0} {1} ({2}) owner ={3} order={4}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name, _localOrder.ToString());
            return _localOrder; //CombatOrder.Engage; // not set to retreat because easy retreat in automatedCE will take ship out of combat by default
        }

        protected Civilization GetTargetOne(Orbital source)
        {
            bool foundBorg = (GameContext.Current.Civilizations.Where(sc => sc.ShortName == "Borg").Select(sc => sc).ToList().Any()); // any borg here?
            var _targetOne = new Civilization();
            try
            {                                                                                                                   //if (targetCiv == null)                                                                                                                                                                                                                                                                                                        //if(source !=null)
                _targetOne = _targetOneByCiv[source.OwnerID].GetTargetOne(source);

                if (_targetOne.CivID == -1)
                {
                    //if (foundBorg)
                        _targetOne = CombatHelper.GetBorgCiv();
                    //else
                    // _targetOne = CombatHelper.GetHoldFireCiv();                  
                }
                GameLog.Core.Test.DebugFormat("Try Get target one for {0} owner {1}: target = {2}", source, source.Owner, _targetOne.Name);
                return _targetOne;
            }
            catch // (Exception e)
            {
                //GameLog.Core.Combat.DebugFormat("Unable to get target one for {0} {1} ({2}) Owner {3}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
                _targetOne = CombatHelper.GetHoldFireCiv();
                GameLog.Core.Combat.DebugFormat("Setting target for {0} {1} ({2}) owner: {3} TARGET = {4}",
                    source.ObjectID, source.Name, source.Design.Name, source.Owner.Name, _targetOne.Name);
                ////GameLog.LogException(e);
            }
            GameLog.Core.Test.DebugFormat("Got Target One returning GetTargetOne:{0}, Key = {1}", _targetOne.CivID, _targetOne.Key);
            return _targetOne;
        }
        protected Civilization GetTargetTwo(Orbital source)
        {
            bool foundBorg = (GameContext.Current.Civilizations.Where(sc => sc.ShortName == "Borg").Select(sc => sc).ToList().Any()); // any borg here?
            var _targetTwo = new Civilization();
            try
            {
                _targetTwo = _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);
                if (_targetTwo.CivID == -1)
                {
                    if (foundBorg)
                        _targetTwo = CombatHelper.GetBorgCiv();
                    else
                    _targetTwo = CombatHelper.GetHoldFireCiv();
                }

            }
            catch // (Exception e)
            {
                //GameLog.Core.Test.ErrorFormat("Unable to get target Two for source {0} owner {1}, {2} default to Borg {2}", source, source.Owner, _targetTwo.ToString());
                _targetTwo = CombatHelper.GetHoldFireCiv();
                //GameLog.LogException(e);
            }
            return _targetTwo;
        }

        protected abstract void ResolveCombatRoundCore();

    }
}

