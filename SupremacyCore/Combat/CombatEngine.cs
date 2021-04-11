// File:CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Diplomacy;
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
        public readonly object SyncLock;
        public readonly object SyncLockTargetOnes;
        public readonly object SyncLockTargetTwos;
        protected const double BaseChanceToRetreat = 0.50;
        protected const double BaseChanceToAssimilate = 0.05;
        protected const double BaseChanceToRushFormation = 0.50;
        protected readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        protected readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips; // ships and stations
        protected List<Tuple<CombatUnit, CombatWeapon[]>> _combatShipsTemp; // Update xyz declare temp array done
        protected Tuple<CombatUnit, CombatWeapon[]> _combatStation;
        protected readonly Dictionary<int, Civilization> _targetOneData;
        protected int _roundNumber;
        private bool _running;
        private bool _runningTargetOne;
        private bool _runningTargetTwo;
        private readonly bool _allSidesStandDown;
        private bool _ready;
        protected readonly List<CombatAssets> _assets;
        private readonly SendCombatUpdateCallback _updateCallback;
        private readonly NotifyCombatEndedCallback _combatEndedCallback;
        private readonly Dictionary<int, CombatOrders> _orders; // locked to evaluate one civ at a time for combat order, key is OwnerID int
        private readonly Dictionary<int, CombatTargetPrimaries> _targetOneByCiv; // like _orders
        private readonly Dictionary<int, CombatTargetSecondaries> _targetTwoByCiv;
        protected Dictionary<string, int> _empireStrengths; // string in key of civ and int is total fire power of civ

        public bool BattelInOwnTerritory { get; set; }

        public int TotalFirepower { get; set; }

        protected int CombatID { get; }

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
                    {
                        _ready = false;
                    }
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
                    {
                        _ready = false;
                    }
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
                    {
                        _ready = false;
                    }
                }
            }
        }

        public bool IsCombatOver
        {
            get
            {
                //GameLog.Core.Combat.DebugFormat("_roundNumber = {0}", _roundNumber);
                //GameLog.Core.Combat.DebugFormat("_allSidesStandDown ={0}, IsCombatOver ={1} as HasSurvivingAssets ", _allSidesStandDown, (_assets.Count(assets => assets.HasSurvivingAssets) <= 1));
                if (_allSidesStandDown)
                {
                    return true;
                }

                if (_roundNumber > 1)
                {
                    return true;
                }

                int coutner = 0;
            TryAgain:

                try
                {
                    return _assets.Count(assets => assets.HasSurvivingAssets) <= 1; //count assets less than or equal one for true/false
                }
                catch (Exception e)
                {
                    GameLog.Core.Combat.WarnFormat("We changed _assets while counting, error message {0}", e);
                    System.Threading.Thread.Sleep(1000); // wait for a second
                    if (coutner > 2)
                    {
                        return true;
                    }

                    coutner++;
                    goto TryAgain;
                    //throw;
                }
            }
        }

        public bool Ready
        {
            get
            {
                lock (SyncLock)
                {
                    if (Running || IsCombatOver) // RunningTargetOne || RunningTargetTwo)
                    {
                        return false;
                    }

                    return _ready;
                }
            }
        }

        protected CombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
        {
            _running = false;
            _runningTargetOne = false;
            _runningTargetTwo = false;
            _allSidesStandDown = false;
            CombatID = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _updateCallback = updateCallback ?? throw new ArgumentNullException(nameof(updateCallback));
            _combatEndedCallback = combatEndedCallback ?? throw new ArgumentNullException(nameof(combatEndedCallback));
            _orders = new Dictionary<int, CombatOrders>(); // in CombatOrders class there is the _orders dictionary of int object orbital id and value enum combat order. Here int is OwnerID
            _empireStrengths = new Dictionary<string, int>();
            _targetOneByCiv = new Dictionary<int, CombatTargetPrimaries>();
            _targetTwoByCiv = new Dictionary<int, CombatTargetSecondaries>();
            SyncLock = _orders;
            SyncLockTargetOnes = _targetOneByCiv;
            SyncLockTargetTwos = _targetTwoByCiv;
            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();

            GameLog.Core.Combat.DebugFormat("_combatId = {0}, _roundNumber = {1}" //, _targetOneByCiv = {2}, _targetOneByCiv = {3}"
                , CombatID
                , _roundNumber
                );

            foreach (CombatAssets civAssets in _assets.ToList())
            {
                if (civAssets.Station?.Source != null) // new build stations have no source
                {
                    _combatStation = new Tuple<CombatUnit, CombatWeapon[]>(
                        civAssets.Station,
                        CombatWeapon.CreateWeapons(civAssets.Station.Source));
                }
                foreach (CombatUnit shipStats in civAssets.CombatShips.ToList())
                {
                    _combatShips.Add(new Tuple<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));
                }
                foreach (CombatUnit shipStats in civAssets.NonCombatShips.ToList())
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
                _orders[888] = orders;
                _orders[999] = orders;
                _orders[777] = orders;
                if (!_orders.ContainsKey(orders.OwnerID))
                {
                    _orders[orders.OwnerID] = orders;
                    GameLog.Core.CombatDetails.DebugFormat("adding orders in dictionary for ID {0}", orders.OwnerID);
                }

                List<int> outstandingOrders = _assets.Select(assets => assets.OwnerID).ToList(); // list of OwnerIDs, ints
                List<int> dummyIDs = new List<int>
                {
                    777, // was set to 775
                    888,
                    999
                };
                outstandingOrders.AddRange(dummyIDs);

                lock (_orders)
                {
                    foreach (int civKey in _orders.Keys)
                    {
                        _ = outstandingOrders.Remove(civKey);
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

                List<int> outstandingTargets = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_targetOneByCiv)
                {
                    foreach (int civKey in _targetOneByCiv.Keys)
                    {
                        _ = outstandingTargets.Remove(civKey);
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

                List<int> outstandingTargets = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_targetTwoByCiv)
                {
                    foreach (int civId in _targetTwoByCiv.Keys)
                    {
                        _ = outstandingTargets.Remove(civId);
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

                _assets.ForEach(a => a.CombatID = CombatID); // assign combatID for each asset _assets
                CalculateEmpireStrengths();
                GameLog.Core.Combat.DebugFormat("_roundNumber = {0}, AllSidesStandDown() = {1}, IsCombatOver ={2}", _roundNumber, AllSidesStandDown(), IsCombatOver);
                RechargeWeapons();
                ResolveCombatRoundCore(); // call to AutomatedCombatEngine's CombatResolveCombatRoundCore

                if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                {
                    PerformAssimilation();
                }
                GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - at PerformRetreat");
                PerformRetreat();

                GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound - at UpdateOrbitals");
                UpdateOrbitals();
                GameLog.Core.CombatDetails.DebugFormat("If IsCombatOver  = {0} then increment round number {1} to {2}", IsCombatOver, _roundNumber, _roundNumber + 1);
                if (!IsCombatOver)
                {
                    //GameLog.Core.CombatDetails.DebugFormat("incrementing - round number {0} to {1}", _roundNumber, _roundNumber + 1);
                    _roundNumber++;
                }
                _orders.Clear();
            }

            SendUpdates();

           //GameLog.Core.CombatDetails.DebugFormat("ResolveCombatRound Sent SendUpdates then call RemoveDefeatedPlayers()");
            RemoveDefeatedPlayers();

            RunningTargetOne = false;
            RunningTargetTwo = false;
            Running = false;
            //GameLog.Core.CombatDetails.DebugFormat("IsCombatOver ={0} for AsychHelper", IsCombatOver);
            if (IsCombatOver)
            {
                GameLog.Core.CombatDetails.DebugFormat("now IsCombatOver = TRUE so invoked AsyncHelper");
                AsyncHelper.Invoke(_combatEndedCallback, this);
            }
            _targetTwoByCiv.Clear();
            _targetOneByCiv.Clear();
        }

        public bool AllSidesStandDown() // ??? do we no longer care what the orders are - no longer have a second chance at setting orders?
        {
            //GameLog.Core.CombatDetails.DebugFormat("Now AllsideStandDown ={0} based on combat orders", AllSidesStandDown());
            foreach (CombatAssets civAssets in _assets)
            {
                // Combat ships
                if (civAssets.CombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    GameLog.Core.CombatDetails.DebugFormat("Combat ships - AllSidesStandDown is false");
                    return false;
                }
                // Non-combat ships
                if (civAssets.NonCombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    GameLog.Core.CombatDetails.DebugFormat("NON Combat ships - AllSidesStandDown is false");
                    return false;
                }
                // Station
                if ((civAssets.Station != null) && (GetCombatOrder(civAssets.Station.Source) == CombatOrder.Engage || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Transports || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Rush || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Formation))
                {
                    GameLog.Core.CombatDetails.DebugFormat("Station - AllSidesStandDown is false");
                    return false;
                }
            }
            GameLog.Core.CombatDetails.DebugFormat("AllSidesStandDown is true");
            return true;
        }

        public void SendInitialUpdate()
        {
            GameLog.Core.Combat.DebugFormat("Called SendInitalUpdate to now call SendUpdates()");
            SendUpdates();
        }

        protected void SendUpdates()
        {
            foreach (CombatAssets playerAsset in _assets) // _assets is list of current player (friend) assets so one list for our friends, friend's and other's asset are in asset (not _assets)
            {
                Civilization owner = playerAsset.Owner;
                List<CombatAssets> friendlyAssets = new List<CombatAssets>();
                List<CombatAssets> hostileAssets = new List<CombatAssets>();

                friendlyAssets.Add(playerAsset); // on each looping arbitrary one side or the other is 'friendly' for combatwindow right and left side
                //foreach (CombatAssets asset in _assets)
                //{
                //    GameLog.Core.Combat.DebugFormat("asset of {0} in sector", asset.Owner.Key);
                //}
                GameLog.Core.CombatDetails.DebugFormat("Current or first asset from {0} for current friendlyAssets", playerAsset.Owner.Key);
                foreach (CombatAssets civAsset in _assets.Distinct().ToList())
                {
                    //GameLog.Core.CombatDetails.DebugFormat("beginning calculating empireStrengths for {0}", //, current value =  for {0} {1} ({2}) = {3}", civ.Owner.Key);

                    int currentEmpireStrength = 0;

                    foreach (CombatAssets cs in _assets)  // only combat ships
                    {
                        //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", cs.Owner.Key, civ.Owner.Key);
                        if (cs.Owner.Key == civAsset.Owner.Key)
                        {
                            //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire {1}", cs.Owner.Key, civ.Owner.ToString());

                            foreach (CombatUnit ship in cs.CombatShips)
                            {
                                currentEmpireStrength += ship.Firepower;
                                //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                            }

                            if (cs.Station != null)
                            {
                                currentEmpireStrength += cs.Station.Firepower;
                            }

                            if (!_empireStrengths.Any(e => e.Key == cs.Owner.ToString()))
                            {
                                _empireStrengths.Add(civAsset.Owner.ToString(), currentEmpireStrength);
                            }
                        }
                    }
                    GameLog.Core.CombatDetails.DebugFormat("for = {0} currentEmpireStrength = {1}", civAsset.Owner.Key, currentEmpireStrength);
                }
                foreach (CombatAssets otherAsset in _assets) // _assets is all combat assest in sector while "otherAsset" is not of type "friendly" first asset
                {
                    if (otherAsset == playerAsset)
                    {
                        continue;
                    }

                    if (CombatHelper.WillFightAlongside(owner, otherAsset.Owner))
                    {
                        friendlyAssets.Add(otherAsset);
                        _ = friendlyAssets.Distinct().ToList();
                        //GameLog.Core.Combat.DebugFormat("asset of {0} added to friendlies", otherAsset.Owner.Key);
                    }
                    else
                    {
                        hostileAssets.Add(otherAsset);
                        _ = hostileAssets.Distinct().ToList();
                        //GameLog.Core.Combat.DebugFormat("asset for {0} added to hostilies", otherAsset.Owner.Key);
                    }
                }
                List<CombatAssets> leftOutAssets = new List<CombatAssets>();
                foreach (CombatAssets missedAsset1 in _assets)
                {
                    if (!friendlyAssets.Contains(missedAsset1) && !hostileAssets.Contains(missedAsset1))
                    {
                        leftOutAssets.Add(missedAsset1);
                    }
                }
                foreach (CombatAssets friendlyAsset in friendlyAssets)
                {
                    foreach (CombatAssets missedAsset2 in leftOutAssets)
                    {
                        IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[missedAsset2.Owner, friendlyAsset.Owner];
                        if (diplomacyData.Status == ForeignPowerStatus.OwnerIsMember ||
                            diplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember ||
                            diplomacyData.Status == ForeignPowerStatus.Allied)
                        {
                            friendlyAssets.Add(missedAsset2);
                        }
                    }
                }
                CombatUpdate update = new CombatUpdate(
                    CombatID,
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
            // CHANGE X
            for (int i = 0; i < _assets.Count; i++)
            {
                GameLog.Core.CombatDetails.DebugFormat("Surviving assets in sector {0}? ={1}", _assets[i].Owner.Key, _assets[i].HasSurvivingAssets);
                if (!_assets[i].HasSurvivingAssets)
                {
                    GameLog.Core.Combat.DebugFormat("remove defeated Player {0} asset from sector", _assets[i].Owner.Key);
                    _assets.RemoveAt(i--);
                }
            }
            GameLog.Core.CombatDetails.DebugFormat("--------------------");
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
                foreach (CombatWeapon weapon in _combatStation.Item2)
                {
                    weapon.Recharge();
                }
            }
            foreach (Tuple<CombatUnit, CombatWeapon[]> combatShip in _combatShips)
            {
                foreach (CombatWeapon weapon in combatShip.Item2)
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
            foreach (Tuple<CombatUnit, CombatWeapon[]> combatShip in _combatShips)
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

            foreach (KeyValuePair<string, int> empire in _empireStrengths)
            {
                GameLog.Core.Combat.DebugFormat("Strength for {0} = {1}", empire.Key, empire.Value);
                //makes crash !!   _empireStrengths.Add(empire.Key, empire.Value);
            }
        }

        /// <summary>
        /// Performs the assimilation of ships that have been assimilated
        /// </summary>
        private void PerformAssimilation()
        {
            Civilization borg = GameContext.Current.Civilizations.First(c => c.Name == "Borg");

            foreach (CombatAssets assets in _assets)
            {
                foreach (CombatUnit assimilatedShip in assets.AssimilatedShips)
                {
                    Civilization assimilatedCiv = assimilatedShip.Owner;
                    CivilizationManager targetEmpire = GameContext.Current.CivilizationManagers[assimilatedCiv];
                    Universe.Colony assimiltedCivHome = targetEmpire.HomeColony;
                    int gainedResearchPoints = assimiltedCivHome.NetResearch;
                    Universe.Sector destination = CombatHelper.CalculateRetreatDestination(assets);
                    Ship ship = (Ship)assimilatedShip.Source;
                    ship.Owner = borg;
                    Fleet newfleet = ship.CreateFleet();
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
                    GameContext.Current.CivilizationManagers[borg].Research.UpdateResearch(gainedResearchPoints);

                    string _report = "Ship " + ship.ObjectID + " assimilated: " + ship.Name + " (" + ship.Design + "). We gained " + gainedResearchPoints + " research points.";

                    GameLog.Core.Combat.DebugFormat("Assimilated Assets: {0} {1}, Owner = {2}, OwnerID = {3}, Fleet.OwnerID = {4}, Order = {5} gainedResearchPoints ={6}",
                        ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order, gainedResearchPoints);

                    GameContext.Current.CivilizationManagers[borg].SitRepEntries.Add(new ShipAssimilatedSitRepEntry( borg, ship.Location, _report));
                }
            }
        }

        /// <summary>
        /// Moves ships that have escaped to their destinations
        /// </summary>
        protected void PerformRetreat()
        {
            try // CHANGE X
            {
                GameLog.Core.Combat.DebugFormat("PerformRetreat begins");
                foreach (CombatAssets assets in _assets)
                {
                    Universe.Sector destination = CombatHelper.CalculateRetreatDestination(assets);

                    if (destination == null)
                    {
                        continue;
                    }
                    if (assets.EscapedShips.Count() > 0)
                    {
                        foreach (CombatUnit shipStats in assets.EscapedShips)
                        {
                            ((Ship)shipStats.Source).Fleet.Location = destination.Location;
                            GameLog.Core.Combat.DebugFormat("PerformRetreat: {0} {1} retreats to {2}",
                                ((Ship)shipStats.Source).Fleet.ObjectID, ((Ship)shipStats.Source).Fleet.Name, destination.Location.ToString());
                        }
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
            return _assets.Find(a => a.Owner == owner);
        }

        /// <summary>
        /// Gets the order assigned to the given <see cref="Orbital"/>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected CombatOrder GetCombatOrder(Orbital source)
        {
            CombatOrder _localOrder = CombatOrder.Engage;
            try
            {
                GameLog.Core.CombatDetails.DebugFormat("Try Get Order for {0} {1} {2}", source.ObjectID, source.Name, source.Design.Name);
                //if(_orders[source.OwnerID].GetOrder(source) == CombatOrder.)

                _localOrder = _orders[source.OwnerID].GetOrder(source);
                GameLog.Core.CombatDetails.DebugFormat("Got Order for {0} {1} {2}: -> order = {3}", source.ObjectID, source.Name, source.Design.Name, _orders[source.OwnerID].GetOrder(source));
                return _localOrder; // this is the class CombatOrder.BORG (or FEDERATION or.....) that comes from public GetCombatOrder() in CombatOrders.cs
            }
            catch //(Exception e)
            {
                //if (source.Owner.IsHuman == false)
                //{
                //    // Gamelog works but makes no sense ... or "nothing to win" with this error message (Example: Scout, before Engage, here fails 
                //    //GameLog.Core.CombatDetails.ErrorFormat("Unable to get order for {2} Owner: {3} Ship {0} {1}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name);
                //}
                //GameLog.LogException(e);
            }

            GameLog.Core.CombatDetails.DebugFormat("Setting order for {0} {1} ({2}) owner ={3} order={4}", source.ObjectID, source.Name, source.Design.Name, source.Owner.Name, _localOrder.ToString());
            return _localOrder; //CombatOrder.Engage; // not set to retreat because easy retreat in automatedCE will take ship out of combat by default
        }

        //public void SetCombatOrder(Orbital source, CombatOrder order)
        //{
        //    Dictionary<int, CombatOrder> dictionary = new Dictionary<int, CombatOrder>();
        //    dictionary.Add( source.ObjectID, order);
        //    _orders[source.OwnerID].Add(source.OwnerID, dictionary);
        //}

        protected Civilization GetTargetOne(Orbital source)
        {
            if (_targetOneByCiv.Keys.Contains(source.OwnerID))
            {
                GameLog.Core.CombatDetails.DebugFormat("GetTargetOne ={0}", _targetOneByCiv[source.OwnerID].GetTargetOne(source));//if (targetCiv == null)                                                                                                                                                                                                                                                                                                        //if(source !=null)
                return _targetOneByCiv[source.OwnerID].GetTargetOne(source);
            }
            else
            {
                return CombatHelper.GetDefaultHoldFireCiv();
            }
        }
        protected Civilization GetTargetTwo(Orbital source)
        {
            if (_targetTwoByCiv.Keys.Contains(source.OwnerID))
            {
                GameLog.Core.CombatDetails.DebugFormat("GetTargetTwo ={0}", _targetTwoByCiv[source.OwnerID].GetTargetTwo(source));
                return _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);
            }
            else
            {
                return CombatHelper.GetDefaultHoldFireCiv();
            }
        }

        protected abstract void ResolveCombatRoundCore();
    }
}

