// File:CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

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
        private readonly string _sectorString;
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
        protected Dictionary<string, int> _civDurabilities; // string in key of civ and int is total fire power of civ

        [NonSerialized]
        private string _text;
        private string _combatEngine_full_Report;
        private bool _combatWriteDirectly;
        private readonly string _destroyedString = "";
        private readonly string _escapedString = "";
        private readonly string _combatString = "";
        private readonly string _nonCombatString = "";
        private readonly string newline = Environment.NewLine;
        private readonly string blank = " ";
        private readonly List<Civilization> _friendlyCivs = new List<Civilization> { };
        private readonly List<Civilization> _otherCivs = new List<Civilization> { };

        public bool BattelInOwnTerritory { get; set; }

        public int TotalFirepower { get; set; }

        protected List<Civilization> FriendlyCivs => _friendlyCivs; // test 
        protected List<Civilization> OtherCivs => _otherCivs;  // test 

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
                if (_roundNumber > 1)
                {
                    //_text = _combat_Automated_full_Report += newline + _text;
                    Console.WriteLine(newline + "Step_3667:; _combatEngine_full_Report > " /*+ newline */
                            + _combatEngine_full_Report + " > end of _combatEngine_full_Report"
                            + newline + newline
                            + "Step_3668:;  > _combat_Automated_full_Report"
                            + Combat_Automated_full_Report + "............. > end of _combat_Automated_full_Report"
                            + newline
                            );

                    return true;
                }

                //GameLog.Core.Combat.DebugFormat("_roundNumber = {0}", _roundNumber);
                //GameLog.Core.Combat.DebugFormat("_allSidesStandDown ={0}, IsCombatOver ={1} as HasSurvivingAssets ", _allSidesStandDown, (_assets.Count(assets => assets.HasSurvivingAssets) <= 1));
                if (_allSidesStandDown)
                {
                    Console.WriteLine(newline + "Step_3665:; _combatEngine_full_Report" + newline
                            + _combatEngine_full_Report + " > end of _combatEngine_full_Report");

                    return true;
                }

                if (_roundNumber > 1)
                {
                    //_text = _combat_Automated_full_Report += newline + _text;
                    Console.WriteLine(newline + "Step_3667:; _combatEngine_full_Report > " /*+ newline */
                            + _combatEngine_full_Report + " > end of _combatEngine_full_Report"
                            + newline + newline
                            + "Step_3668:;  > _combat_Automated_full_Report"
                            + Combat_Automated_full_Report + "............. > end of _combat_Automated_full_Report"
                            + newline
                            );

                    return true;
                }

                //return false;

                int counter = 0;
            TryAgain:

                try
                {
                    return _assets.Count(assets => assets.HasSurvivingAssets) <= 1; //count assets less than or equal one for true/false
                }
                catch (Exception e)
                {
                    _text = "Step_3398:; We changed _assets while counting, error message " + e;
                    Console.WriteLine(_text);
                    //_combat_Automated_full_Report += newline + _text;
                    GameLog.Core.Combat.WarnFormat(_text);
                    System.Threading.Thread.Sleep(1000); // wait for a second
                    if (counter > 2)
                    {
                        return true;
                    }

                    counter++;
                    goto TryAgain;
                    //throw;
                }

                //how ever:
                //return true;
                return false;

            } // end of get{}

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
            NotifyCombatEndedCallback combatEndedCallback,
            string combat_Automated_full_Report = null)
        {
            _combatWriteDirectly = true;

            _text = /*newline + */"Step_3012:; protected CombatEngine...";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += newline + _text;

            _running = false;
            _runningTargetOne = false;
            _runningTargetTwo = false;
            _allSidesStandDown = false;
            _text += " "; // just placeholder to avoid a "is never used"
            _combatString += " "; // just placeholder to avoid a "is never used"
            _destroyedString += " "; // just placeholder to avoid a "is never used"
            _escapedString += " "; // just placeholder to avoid a "is never used"
            _nonCombatString += " "; // just placeholder to avoid a "is never used"
            _text += " "; // just placeholder to avoid a "is never used"
            CombatID = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _updateCallback = updateCallback ?? throw new ArgumentNullException(nameof(updateCallback));
            _combatEndedCallback = combatEndedCallback ?? throw new ArgumentNullException(nameof(combatEndedCallback));
            _orders = new Dictionary<int, CombatOrders>(); // in CombatOrders class there is the _orders dictionary of int object orbital id and value enum combat order. Here int is OwnerID
            _empireStrengths = new Dictionary<string, int>();
            _civDurabilities = new Dictionary<string, int>();
            _targetOneByCiv = new Dictionary<int, CombatTargetPrimaries>();
            _targetTwoByCiv = new Dictionary<int, CombatTargetSecondaries>();
            SyncLock = _orders;
            SyncLockTargetOnes = _targetOneByCiv;
            SyncLockTargetTwos = _targetTwoByCiv;
            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _text += newline + _destroyedString + _escapedString + _combatString + _nonCombatString;  // dummy

            _sectorString = GameEngine.LocationString(_assets[0].Location.ToString()) + " > ";

            _text = "Step_3017:; " + _sectorString + "_combatId = " + CombatID + ", _roundNumber = " + _roundNumber; //, _targetOneByCiv = {2}, _targetOneByCiv = {3}"
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += newline + _text;
            GameLog.Core.CombatDetails.DebugFormat(_text);

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

            //Combat_Automated_full_Report = combat_Automated_full_Report;
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
                    _text = "Step_3078:; adding orders in dictionary for civ.ID " + orders.OwnerID + " > " + _orders[orders.OwnerID].ToString();
                    Console.WriteLine(_text);
                    //_combatEngine_full_Report += newline + _text;
                    //GameLog.Core.CombatDetails.DebugFormat(_text);
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

                //RunningTargetOne = true; // 2025-02-08

                _assets.ForEach(a => a.CombatID = CombatID); // assign combatID for each asset _assets

                //CalculateEmpireStrengths();

                _text = "Step_3193:; _roundNumber = " + _roundNumber
                    + "; AllSidesStandDown() = " + AllSidesStandDown()
                    + "; IsCombatOver = " + IsCombatOver
                    ;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += newline + _text;
                GameLog.Core.CombatDetails.DebugFormat(_text);
                //_text = "Step_3001: _roundNumber = {0}, AllSidesStandDown() = {1}, IsCombatOver ={2}", _roundNumber, AllSidesStandDown(), IsCombatOver);

                RechargeWeapons();
                ResolveCombatRoundCore(); // call to AutomatedCombatEngine's CombatResolveCombatRoundCore

                if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                {
                    PerformAssimilation();
                }
                _text = "Step_3194:; " + _sectorString + "_combatId = " + CombatID + " > ResolveCombatRound - at PerformRetreat";
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += newline + _text;
                //GameLog.Core.CombatDetails.DebugFormat(_text);

                PerformRetreat();

                _text = "Step_3196:; " + _sectorString + "_combatId = " + CombatID + " > ResolveCombatRound - at UpdateOrbitals";
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += newline + _text;
                GameLog.Core.CombatDetails.DebugFormat(_text);

                UpdateOrbitals();

                _text = "Step_3199:; " + _sectorString + "_combatId = " + CombatID + " > If IsCombatOver = " + IsCombatOver
                    + " > then increment round number " + _roundNumber
                    ;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += newline + _text;
                GameLog.Core.CombatDetails.DebugFormat(_text);
                //_text = "Step_3001: If IsCombatOver  = {0} then increment round number {1} to {2}", IsCombatOver, _roundNumber, _roundNumber + 1);

                if (!IsCombatOver)
                {
                    //_text = "Step_3001: incrementing - round number {0} to {1}", _roundNumber, _roundNumber + 1);
                    _roundNumber++;
                }
                _orders.Clear();
            }

            SendUpdates();

            //_text = "Step_3001: ResolveCombatRound Sent SendUpdates then call RemoveDefeatedPlayers()");
            RemoveDefeatedPlayers();

            //_roundNumber++; // 2025-02-08

            RunningTargetOne = false;
            RunningTargetTwo = false;
            Running = false;
            //_text = "Step_3001: IsCombatOver ={0} for AsychHelper", IsCombatOver);
            if (IsCombatOver)
            {
                _text = "Step_3090:; now IsCombatOver = TRUE so invoked AsyncHelper" + blank;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += newline + _text;
                //GameLog.Core.CombatDetails.DebugFormat(_text);

                AsyncHelper.Invoke(_combatEndedCallback, this);
            }
            _targetTwoByCiv.Clear();
            _targetOneByCiv.Clear();
        }

        public bool AllSidesStandDown() // ??? do we no longer care what the orders are - no longer have a second chance at setting orders?
        {
            //_text = "Step_3001: Now AllsideStandDown ={0} based on combat orders", AllSidesStandDown());
            foreach (CombatAssets civAssets in _assets)
            {
                // Combat ships
                if (civAssets.CombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    _text = "Step_3091:; Combat ships - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += newline + _text;
                    GameLog.Core.CombatDetails.DebugFormat(_text);
                    return false;
                }
                // Non-combat ships
                if (civAssets.NonCombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    _text = "Step_3092:; NON Combat ships - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += newline + _text;
                    GameLog.Core.CombatDetails.DebugFormat(_text);

                    return false;
                }
                // Station
                if ((civAssets.Station != null) && (GetCombatOrder(civAssets.Station.Source) == CombatOrder.Engage || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Transports || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Rush || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Formation))
                {
                    _text = "Step_3093:; Station - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += newline + _text;
                    GameLog.Core.CombatDetails.DebugFormat(_text);

                    return false;
                }
            }
            _text = "Step_3098:; AllSidesStandDown is true";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += newline + _text;
            GameLog.Core.CombatDetails.DebugFormat(_text);
            return true;
        }

        public void SendInitialUpdate()
        {

            _text = "Step_3007:; Called SendInitalUpdate to now call SendUpdates()";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += newline + _text;
            //GameLog.Core.CombatDetails.DebugFormat(_text);

            SendUpdates();
        }

        protected void SendUpdates()
        {
            _text = "Step_3010:; SendUpdates"; // Step_3020 is enough
            //if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += newline + _text;

            foreach (CombatAssets playerAsset in _assets) // _assets is list of current player (friend) assets so one list for our friends, friend's and other's asset are in asset (not _assets)
            {
                Civilization owner = playerAsset.Owner;
                List<CombatAssets> friendlyAssets = new List<CombatAssets>();
                List<CombatAssets> hostileAssets = new List<CombatAssets>();

                Universe.MapLocation _location = _assets.First().Location;

                friendlyAssets.Add(playerAsset); // on each looping arbitrary one side or the other is 'friendly' for combatwindow right and left side
                //foreach (CombatAssets asset in _assets)
                //{
                //    GameLog.Core.Combat.DebugFormat("asset of {0} in sector", asset.Owner.Key);
                //}
                _text = "Step_3020:; " + _sectorString + "SendUpdates for current friendlyAssets for " + playerAsset.Owner.Key;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += newline + _text; // > less output !!
                //GameLog.Core.CombatDetails.DebugFormat(_text);

                foreach (CombatAssets civAsset in _assets.Distinct().ToList())
                {
                    //_text = "Step_3001: beginning calculating empireStrengths for {0}", //, current value =  for {0} {1} ({2}) = {3}", civ.Owner.Key);

                    int _currentCivStrength = 0;
                    int _civDurability = 0;

                    foreach (CombatAssets cs in _assets)  // only combat ships
                    {
                        //_text = "Step_3001: calculating empireStrengths"
                        //    + " for Ship.Owner=" + cs.Owner.Key
                        //    + " and Empire= " + civAsset.Owner.Key
                        //    ;
                        //if (_combatWriteDirectly) Console.WriteLine(_text);
                        //_combatEngine_full_Report += newline + _text;

                        if (cs.Owner.Key == civAsset.Owner.Key)
                        {
                            //_text = "Step_3001: calculating empireStrengths for Ship.Owner = {0} and Empire {1}", cs.Owner.Key, civ.Owner.ToString());

                            foreach (CombatUnit ship in cs.CombatShips)
                            {
                                _currentCivStrength += ship.Firepower;
                                _civDurability += CalculateStrength_CombatShip_in_CombatEngine(ship);
                                //_text = "Step_3001: added Firepower into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                            }

                            foreach (CombatUnit ship in cs.NonCombatShips)
                            {
                                //_currentCivStrength += ship.Firepower;
                                _civDurability += CalculateStrength_CombatShip_in_CombatEngine(ship);
                                //_text = "Step_3001: added Firepower into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                            }

                            if (cs.Station != null)
                            {
                                _currentCivStrength += cs.Station.Firepower;
                                _civDurability += cs.Station.Firepower + cs.Station.HullStrength + cs.Station.ShieldStrength;
                            }

                            if (_empireStrengths.Any(e => e.Key.ToString() == cs.Owner.Name.ToString()))
                            {
                                ;
                                // already exist
                            }
                            else
                            {
                                try { 
                                _empireStrengths.Add(civAsset.Owner.ToString(), _currentCivStrength);
                                _civDurabilities.Add(civAsset.Owner.ToString(), _civDurability);
                            } catch { }
                            }
                        }
                    }
                    //to often
                    //_text = "Step_3030:; " + _sectorString + "SendUpdates: _currentCivStrength = " + _currentCivStrength + " for " + civAsset.Owner.Key;
                    //if (_combatWriteDirectly) Console.WriteLine(_text);
                    //_combatEngine_full_Report += newline + _text;
                    //GameLog.Core.CombatDetails.DebugFormat(_text);
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

                AsyncHelper.Invoke(_updateCallback, this, update);

                // to often
                //_text = "Step_3041:; new CombatUpdate for " + owner;
                //if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += newline + _text;
                // sends data back to combat window

                // ToDo>try to build in here the Sitreps
                //if (playerAsset.DestroyedShips.Count > 0)
                //    _destroyedString = ", some ships got destroyed"
                //        ;
                //if (playerAsset.EscapedShips.Count > 0)
                //    _escapedString = ", some ships escaped"
                //        ;
                //if (playerAsset.CombatShips.Count > 0)
                //    _combatString = ", combat ships survived"
                //        ;
                //if (playerAsset.NonCombatShips.Count > 0)
                //    _nonCombatString = ", non-combat ships survived"
                //        ;

                //_text = 
                //    //playerAsset.Location.ToString()
                //    //+ " > Combat result: " 
                //    /*+ */"Strength " + update.FriendlyEmpireStrength + " vs " + update.AllHostileEmpireStrength
                //    + _escapedString
                //    + _destroyedString
                //    ;

                //GameContext.Current.CivilizationManagers[owner].SitRepEntries.Add(new ReportEntry_CoS(owner, _location, _text, "", "", SitRepPriority.Red));


                //AsyncHelper.Invoke(_updateCallback, this, update);

                // to often
                //_text = "Step_3048:; _updateCallback for " + owner;
                //if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += newline + _text;
            }
        }

        private int CalculateStrength_CombatShip_in_CombatEngine(CombatUnit cs)
        {
            // UPDATE X 25 June 2019: Do total strength instead of just firepower
            return Convert.ToInt32(Convert.ToDouble(cs.Firepower)
                    + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                    * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        }

        //private int CalculateStrength_CombatShip_in_CombatEngine(CombatUnit cs)
        //{
        //    // UPDATE X 25 June 2019: Do total strength instead of just firepower
        //    return Convert.ToInt32(Convert.ToDouble(cs.Firepower)
        //            + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
        //            * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        //}


        /// <summary>
        /// Remove the assets of defeated players from the combat
        /// </summary>
        private void RemoveDefeatedPlayers()
        {
            // CHANGE X
            for (int i = 0; i < _assets.Count; i++)
            {
                _text = "Step_3048:; " + _sectorString + "Surviving assets for " + _assets[i].Owner.Key + ": " + _assets[i].HasSurvivingAssets;
                Console.WriteLine(_text);
                GameLog.Core.CombatDetails.DebugFormat(_text);

                if (!_assets[i].HasSurvivingAssets)
                {
                    _text = "Step_3049:; " + _sectorString + "remove defeated assets for Player " + _assets[i].Owner.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.CombatDetails.DebugFormat(_text);

                    _assets.RemoveAt(i--);
                }
            }
            _text = "Step_3009:; --------------------";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += newline + _text;
            GameLog.Core.CombatDetails.DebugFormat(_text);
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
                _text = "Step_3053:; " + _sectorString
                    + " > " + GameEngine.Do_5_Digit(empire.Value.ToString())
                    + " = Strength for " + empire.Key + ": ";
                // Detailed_Log(_text);
                Console.WriteLine(_text);
                GameLog.Core.CombatDetails.DebugFormat(_text);
                //Civilization civ = GameContext.Current.Civilizations.First(c => c.Name == "Borg");
                //GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(new ReportEntry_CoS(civ, _assets.First().Location, _text, "", "", SitRepPriority.Red));
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
                    newfleet.SetOrder(FleetOrders.IdleOrder.Create());
                    //if (newfleet.Order == null)
                    //{
                    //    newfleet.SetOrder(FleetOrders.AvoidOrder.Create());
                    //}
                    ship.IsAssimilated = true;
                    ship.Scrap = false;
                    newfleet.Name = "Assimilated Assets";
                    GameContext.Current.CivilizationManagers[borg].Research.UpdateResearch(gainedResearchPoints);

                    _text = "Step_3021:; " + ship.Location
                        + " > Ship assimilated: " + ship.ObjectID + " * " + ship.Name + " * ( " + ship.Design + " )";

                    // Detailed_Log(_text);
                    Console.WriteLine(_text);
                    GameLog.Core.CombatDetails.DebugFormat(_text);
                    //Detailed_Log(_sectorString + "Assimilated Assets: {0} {1}, Owner = {2}, OwnerID = {3}, Fleet.OwnerID = {4}, Order = {5} gainedResearchPoints ={6}",
                    //    ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order, gainedResearchPoints);

                    GameContext.Current.CivilizationManagers[assimilatedCiv].SitRepEntries.Add(new ReportEntry_CoS(assimilatedCiv, ship.Location, _text, "", "", SitRepPriority.Red));
                    //GameContext.Current.CivilizationManagers[assimilatedCiv].SitRepEntries.Add(new ShipAssimilatedSitRepEntry(assimilatedCiv, ship.Location, _text));

                    //for Borg only: 
                    _text += ": We gained " + gainedResearchPoints + " research points.";
                    GameContext.Current.CivilizationManagers[borg].SitRepEntries.Add(new ReportEntry_CoS(borg, ship.Location, _text, "", "", SitRepPriority.Green));
                    //GameContext.Current.CivilizationManagers[borg].SitRepEntries.Add(new ShipAssimilatedSitRepEntry(borg, ship.Location, _text));

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
                _text = "Step_3093:; " + _sectorString + "_combatId = " + CombatID + " > PerformRetreat begins";
                Console.WriteLine(_text);
                //GameLog.Core.CombatDetails.DebugFormat(_text);

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
                            _text = "Step_3025:; " + _sectorString + "PerformRetreat: retreating "
                                + ((Ship)shipStats.Source).Fleet.ObjectID + " " + ((Ship)shipStats.Source).Fleet.Name
                                + " to " + destination.Location.ToString();
                            Console.WriteLine(_text);
                            GameLog.Core.CombatDetails.DebugFormat(_text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _text = "Step_3027:; " + _sectorString + "##### Problem at PerformRetreat" + Environment.NewLine + e;
                Console.WriteLine(_text);
                GameLog.Core.CombatDetails.DebugFormat(_text);
                //((Ship)shipStats.Source).Fleet.ObjectID, ((Ship)shipStats.Source).Fleet.Name, destination.Location.ToString(), e);
            }
        }

        /// <summary>
        /// Returns the <see cref="CombatAssets"/> that belong to the given <see cref="Civilization"/>
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

            if (!source.IsMobile) _localOrder = CombatOrder.Engage;
            if (source.HullStrength.CurrentValue < source.HullStrength.Maximum / 5)  // Hull below 20%
                _localOrder = CombatOrder.Retreat;
            if (source.IsCombatant) return CombatOrder.Engage; // 
            if (source.Sector.Owner == source.Owner) return CombatOrder.Hail; // Non-Combatant ships

            //try

            //{

            //    Detailed_Log(_sectorString + "Try Get Order for " + source.ObjectID + blank + source.Name + blank + source.Design.Name);
            //    //if(_orders[source.OwnerID].GetOrder(source) == CombatOrder.)

            //    _localOrder = _orders[source.OwnerID].GetOrder(source);
            //    //_localOrder = _orders[_orders.Count-1].GetOrder(source);
            //    Detailed_Log(_sectorString + "Got Order for " + source.ObjectID + blank + source.Name + blank + source.Design.Name
            //        + " -> order = " + _orders[source.OwnerID].GetOrder(source));
            //    return _localOrder; // this is the class CombatOrder.BORG (or FEDERATION or.....) that comes from public GetCombatOrder() in CombatOrders.cs
            //}
            //catch //(Exception e)
            //{
            //    //if (source.Owner.IsHuman == false)
            //    //{
            //    //    // Gamelog works but makes no sense ... or "nothing to win" with this error message (Example: Scout, before Engage, here fails 
            //    Detailed_Log(_sectorString + "Returning Engage due to > Unable to get order for " + source.ObjectID + blank + source.Name + blank + source.Design.Name + blank + source.Owner.Name
            //        /*+ newline + e*/);
            //    _localOrder = CombatOrder.Engage;
            //    //}
            //    //GameLog.LogException(e);
            //}

            //Detailed_Log(_sectorString + "Setting order for " + source.ObjectID + blank + source.Name + blank + source.Design.Name 
            //    + " (Owner= " + source.Owner.Name + ") > " + _localOrder.ToString());
            return _localOrder; //CombatOrder.Engage; // not set to retreat because easy retreat in automatedCE will take ship out of combat by default
        }

        //private void Detailed_Log(string _rep)
        //{
        //    Console.WriteLine(_rep);
        //    GameLog.Core.CombatDetails.DebugFormat(_rep);
        //}

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
                _text = "Step_3021:; "
                    + _sectorString + "GetTargetOne = " + _targetOneByCiv[source.OwnerID].GetTargetOne(source)
                    + " ( 888 = only return fire, 777 = no target)"
                    ;//if (targetCiv == null)  
                Console.WriteLine(_text);
                GameLog.Core.CombatDetails.DebugFormat(_text);
                //if(source !=null)
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
                _text = "Step_3022:; " + _sectorString + "GetTargetTwo = " + _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);
                Console.WriteLine(_text);
                GameLog.Core.CombatDetails.DebugFormat(_text);
                return _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);
            }
            else
            {
                return CombatHelper.GetDefaultHoldFireCiv();
            }
        }

        protected abstract void ResolveCombatRoundCore();
        protected string Combat_Automated_full_Report => _combatEngine_full_Report;


    }
}

