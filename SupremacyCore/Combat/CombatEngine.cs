// File:CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using Supremacy.Client;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Windows;

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
        protected Dictionary<int, int> _empireStrengths; // civID and int is total fire power of civ
        protected Dictionary<int, int> _civDurabilities; // cidID and int is total fire power of civ

        [NonSerialized]
        //private string _text;
        private string _combatEngine_full_Report;
        //private bool _combatWriteDirectly;
        //private readonly string _destroyedString = "";
        //private readonly string _escapedString = "";
        //private readonly string _combatString = "";
        //private readonly string _nonCombatString = "";
        //private readonly string _newline = Environment.NewLine;

        //private readonly List<Civilization> _friendlyCivs = new List<Civilization> { };
        //private readonly List<Civilization> _otherCivs = new List<Civilization> { };

        //public bool BattelInOwnTerritory { get; set; }

        public int TotalFirepower { get; set; }

        //protected List<Civilization> FriendlyCivs => _friendlyCivs; // test 
        //protected List<Civilization> OtherCivs => _otherCivs;  // test 

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
                string _newline = Environment.NewLine;
                if (_roundNumber > 1)
                {
                    //_text = _combat_Automated_full_Report += _newline + _text;

                    // much too often
                    //Console.WriteLine(_newline + "Step_3667:; _combatEngine_full_Report > " /*+ _newline */
                    //        + _combatEngine_full_Report + " > end of _combatEngine_full_Report"
                    //        + _newline + _newline
                    //        + "Step_3668:;  > _combat_Automated_full_Report"
                    //        + Combat_Automated_full_Report + "............. > end of _combat_Automated_full_Report"
                    //        + _newline
                    //        );

                    return true;
                }

                //GameLog.Core.Combat.DebugFormat("_roundNumber = {0}", _roundNumber);
                //GameLog.Core.Combat.DebugFormat("_allSidesStandDown ={0}, IsCombatOver ={1} as HasSurvivingAssets ", _allSidesStandDown, (_assets.Count(assets => assets.HasSurvivingAssets) <= 1));
                if (_allSidesStandDown)
                {
                    Console.WriteLine(_newline + "Step_3665:; _combatEngine_full_Report" + _newline
                            + _combatEngine_full_Report + " > end of _combatEngine_full_Report");

                    return true;
                }

                if (_roundNumber > 1)
                {
                    //_text = _combat_Automated_full_Report += _newline + _text;
                    Console.WriteLine(_newline + "Step_3667:; _combatEngine_full_Report > " /*+ _newline */
                            + _combatEngine_full_Report + " > end of _combatEngine_full_Report"
                            + _newline + _newline
                            + "Step_3668:;  > _combat_Automated_full_Report"
                            + Combat_Automated_full_Report + "............. > end of _combat_Automated_full_Report"
                            + _newline
                            );

                    return true;
                }

                return false;

                //    int counter = 0;
                //TryAgain:

                //    try
                //    {
                //        return _assets.Count(assets => assets.HasSurvivingAssets) <= 1; //count assets less than or equal one for true/false
                //    }
                //    catch (Exception e)
                //    {
                //        _text = "Step_3398:; We changed _assets while counting, error message " + e;
                //        Console.WriteLine(_text);
                //        //_combat_Automated_full_Report += _newline + _text;
                //        GameLog.Core.Combat.WarnFormat(_text);
                //        System.Threading.Thread.Sleep(1000); // wait for a second
                //        if (counter > 2)
                //        {
                //            return true;
                //        }

                //        counter++;
                //        goto TryAgain;
                //        //throw;
                //    }

                //    //how ever:
                //    //return true;
                //    return false;

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
            string combReport = null)
        {
            bool _combatWriteDirectly = true;
            string _newline = Environment.NewLine;

            string _text = /*_newline + */"Step_3012:; protected CombatEngine...";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += _newline + _text;

            _running = false;
            _runningTargetOne = false;
            _runningTargetTwo = false;
            _allSidesStandDown = false;
            //_text += " "; // just placeholder to avoid a "is never used"
            string _combatString = " "; // just placeholder to avoid a "is never used"
            string _destroyedString = " "; // just placeholder to avoid a "is never used"
            string _escapedString = " "; // just placeholder to avoid a "is never used"
            string _nonCombatString = " "; // just placeholder to avoid a "is never used"
            //string _text = " "; // just placeholder to avoid a "is never used"
            CombatID = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _updateCallback = updateCallback ?? throw new ArgumentNullException(nameof(updateCallback));
            _combatEndedCallback = combatEndedCallback ?? throw new ArgumentNullException(nameof(combatEndedCallback));
            _orders = new Dictionary<int, CombatOrders>(); // in CombatOrders class there is the _orders dictionary of int object orbital id and value enum combat order. Here int is OwnerID
            _empireStrengths = new Dictionary<int, int>();
            //Dictionary<string, int> 
            _civDurabilities = new Dictionary<int, int>();
            _targetOneByCiv = new Dictionary<int, CombatTargetPrimaries>();
            _targetTwoByCiv = new Dictionary<int, CombatTargetSecondaries>();
            SyncLock = _orders;
            SyncLockTargetOnes = _targetOneByCiv;
            SyncLockTargetTwos = _targetTwoByCiv;
            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _text += _newline + _destroyedString + _escapedString + _combatString + _nonCombatString;  // dummy

            _sectorString = GameEngine.LocationString(_assets[0].Location.ToString())/* + " > "*/;

            _text = "Step_3017:; " + _sectorString + "_combatId = " + CombatID + ", _roundNumber = " + _roundNumber; //, _targetOneByCiv = {2}, _targetOneByCiv = {3}"
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += _newline + _text;
            //GameLog.Core.Combat.DebugFormat(_text);

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

            //Combat_Automated_full_Report = combReport;
        }

        public void SubmitOrders(CombatOrders orders)
        {
            string _text = "";
            lock (SyncLock) //Lock is the keyword in C# that will ensure one thread is executing a piece of code at one time.
            {
                _orders[888] = orders;
                _orders[999] = orders;
                _orders[777] = orders;
                if (!_orders.ContainsKey(orders.OwnerID))
                {
                    _orders[orders.OwnerID] = orders;
                    _text = "Step_3078:; adding orders in dictionary for civ.ID " + orders.OwnerID + " > " + _orders[orders.OwnerID].ToString();

                    // not available or: _orders_orders[orders.OwnerID].Values._orders.Value = Retreat


                    Console.WriteLine(_text);
                    //_combatEngine_full_Report += _newline + _text;
                    //GameLog.Core.Combat.DebugFormat(_text);
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
            string _text = "";
            string _newline = Environment.NewLine;
            bool _combatWriteDirectly = true;

            foreach (var item in _orders)
            {
                string _orders = "";
                foreach (var it in item.Value)
                {
                    _orders += item.Key + /*it._orders.Key +*/ " - " + it + " / "/*+ it.Value + it.*/;

                }

                _text += "Step_7791:; CombatID= " 
                    + CombatID
                    //+ Combat.
                    //+ " " + item.Key
                    + " " + _orders
                    + _newline
                    ;
            }
            Console.WriteLine(_text);

            lock (_orders)
            {
                Running = true;

                //RunningTargetOne = true; // 2025-02-08

                _assets.ForEach(a => a.CombatID = CombatID); // assign combatID for each asset _assets

                CalculateEmpireStrengths();

                _text = "Step_3194:; _roundNumber = " + _roundNumber
                    + "; AllSidesStandDown() = " + AllSidesStandDown()
                    + "; IsCombatOver = " + IsCombatOver
                    ;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);
                //_text = "Step_3001: _roundNumber = {0}, AllSidesStandDown() = {1}, IsCombatOver ={2}", _roundNumber, AllSidesStandDown(), IsCombatOver);

                RechargeWeapons();

                ResolveCombatRoundCore(); // call to AutomatedCombatEngine's CombatResolveCombatRoundCore

                if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                {
                    PerformAssimilation();
                }


                        _text = "Step_3194:; " + _sectorString + "_combatId = " + CombatID + " > ResolveCombatRound - at PerformRetreat";
                        if (_combatWriteDirectly) Console.WriteLine(_text);
                        _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);

                PerformRetreat();

                        _text = "Step_3196:; " + _sectorString + "_combatId = " + CombatID + " > ResolveCombatRound - at UpdateOrbitals";
                        if (_combatWriteDirectly) Console.WriteLine(_text);
                        _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);

                UpdateOrbitals();

                        _text = "Step_3197:; " + _sectorString + "_combatId = " + CombatID + " > If IsCombatOver = " + IsCombatOver
                            + " > then increment round number " + _roundNumber
                            ;
                //if (_combatWriteDirectly) Console.WriteLine(_text);
                        _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);
                //_text = "Step_3001: If IsCombatOver  = {0} then increment round number {1} to {2}", IsCombatOver, _roundNumber, _roundNumber + 1);

                _text = "Step_3198:; _roundNumber = " + _roundNumber
                    + "; AllSidesStandDown() = " + AllSidesStandDown()
                    + "; IsCombatOver = " + IsCombatOver
                    ;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);

                _roundNumber++;

                if (!IsCombatOver)
                {
                    //_text = "Step_3001: incrementing - round number {0} to {1}", _roundNumber, _roundNumber + 1);
                    _roundNumber++;
                }
                _orders.Clear();
            }

            SendUpdates();  // each each turn

            

            DoSitRepsAboutCombat(_assets);

            //_text = "Step_3001: ResolveCombatRound Sent SendUpdates then call RemoveDefeatedPlayers()");
            RemoveDefeatedPlayers();

            //_roundNumber++; // 2025-02-08

            RunningTargetOne = true;
            RunningTargetTwo = false;
            Running = false;
            //_text = "Step_3001: IsCombatOver ={0} for AsychHelper", IsCombatOver);
            if (IsCombatOver)
            {
                _text = "Step_3090:; now IsCombatOver = TRUE so invoked AsyncHelper" + " ";
                if (_combatWriteDirectly) Console.WriteLine(_text);
                _combatEngine_full_Report += _newline + _text;
                //GameLog.Core.Combat.DebugFormat(_text);

                AsyncHelper.Invoke(_combatEndedCallback, this);
            }
            _targetTwoByCiv.Clear();
            _targetOneByCiv.Clear();
        }

        private void DoSitRepsAboutCombat(List<CombatAssets> _assets)//, List<Civilization> _friendlyCivs)
        {
            string _text = "Step_3097:; begin DoSitRepsAboutCombat..";
            bool _combatWriteDirectly = true;

            if (_combatWriteDirectly) Console.WriteLine(_text);

            //if (!_allreadySitRepDONE && _leftSideCiv.Key == "BORG")
            //{

            //    _allreadySitRepDONE = true;

            if (_assets[0] == null)
            {
                return;
            }

            if (_assets[1] == null)
            {
                return;
            }

            //if (_assets[2] != null)
            //{
            //    Debugger.Break();
            //}

            MapLocation _loc = _assets[0].Location;
            //List<Civilization> _leftSideCivs = _assets[0].Owner.ToList();
            Civilization _leftSideCiv = _assets[0].Owner;
            Civilization _rightSideCiv = _assets[1].Owner;
            CivilizationManager _leftSideCivM = GameContext.Current.CivilizationManagers[_leftSideCiv.CivID];
            CivilizationManager _rightSideCivM = GameContext.Current.CivilizationManagers[_rightSideCiv.CivID];

            List<CombatAssets> _leftSideAssets = new List<CombatAssets>();
            List<CombatAssets> _rightSideAssets = new List<CombatAssets>();

            if (_leftSideCiv.IsHuman)
            {
                //Debugger.Break();
            }


            foreach (var item in _assets)
            {
                if (item.OwnerID == _leftSideCiv.CivID)
                {
                    _leftSideAssets.Add(item);
                }
                else
                {
                    _rightSideAssets.Add(item);
                }
            }


            string _allFriendlyAssetsText = "";
            foreach (var item in _leftSideAssets)
            {
                if (item.Station != null)
                {
                    string _ownerString = item.Station.Owner.ShortName;
                    if (_ownerString.Length > 15)
                    {
                        _ownerString = _ownerString.Substring(0, 14) + ".";
                    }

                    while (_ownerString.Length < 16)
                    {
                        _ownerString += " ";
                    }
                    _allFriendlyAssetsText =
                        /*_newline + */"Red Alert at " + item.Sector.Location
                        + " > " + _ownerString
                        + " > Hull= " + item.Station.HullIntegrity
                        + ", Shields= " + item.Station.ShieldIntegrity

                        + " > STATION > " + item.Station.Name

                        //+ _newline
                        ;
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));

                }

                if (item.DestroyedShips.Count > 0) { /*_allFriendlyAssetsText += _newline + "> DESTROYED friendly ships: " + _newline*/; }
                foreach (var _ship in item.DestroyedShips)
                {
                    _allFriendlyAssetsText = CreateShipText(_ship, out string _shipText) + " > destroyed";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Red));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Red));
                }

                if (item.EscapedShips.Count > 0) { /*_allFriendlyAssetsText += _newline + "> ESCAPED friendly ships: " + _newline*/; }
                foreach (var _ship in item.EscapedShips)
                {
                    _allFriendlyAssetsText = CreateShipText(_ship, out string _shipText);// + " > escaped";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                }

                if (item.AssimilatedShips.Count > 0) { /*_allFriendlyAssetsText += _newline + "> ASSIMILATED friendly ships: " + _newline*/; }
                foreach (var _ship in item.AssimilatedShips)
                {
                    _allFriendlyAssetsText = CreateShipText(_ship, out string _shipText);// + " > assimilated";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                }

                if (item.CombatShips.Count > 0) { /*_allFriendlyAssetsText += _newline + "> CombatShips Friendly: " + _newline*/; }
                foreach (var _ship in item.CombatShips)
                {
                    _allFriendlyAssetsText = CreateShipText(_ship, out string _shipText) + " > alive";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                }

                //if (item.NonCombatShips.Count > 0) { _allFriendlyAssetsText += _newline + "> NonCombatShips Friendly: "/* + _newline*/; }
                foreach (var _ship in item.NonCombatShips)
                {
                    _allFriendlyAssetsText = CreateShipText(_ship, out string _shipText) + " > alive";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allFriendlyAssetsText, "", "", SitRepPriority.Yellow));
                }
            }

            string _allHostileAssetsText = "";
            foreach (var item in _rightSideAssets)
            {
                if (item.Station != null)
                {
                    string _ownerString = item.Owner.ShortName;
                    if (_ownerString.Length > 15)
                    {
                        _ownerString = _ownerString.Substring(0, 14) + ".";
                    }

                    while (_ownerString.Length < 16)
                    {
                        _ownerString += " ";
                    }

                    _allHostileAssetsText =
                        /*_newline + */"Red Alert at " + item.Sector.Location
                        + " > " + _ownerString

                        + " > Hull= " + item.Station.HullIntegrity
                        + ", Shields= " + item.Station.ShieldIntegrity
                        +" > STATION > " + item.Station.Name

                        ;
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                }

                //if (item.DestroyedShips.Count > 0) { _allHostileAssetsText += _newline + "> DESTROYED Hostile Ships: "/* + _newline*/; }
                foreach (var _ship in item.DestroyedShips)
                {
                    _allHostileAssetsText = CreateShipText(_ship, out string _shipText) + " > destroyed";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Red));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Red));
                }

                //if (item.EscapedShips.Count > 0) { _allHostileAssetsText += _newline + "> ESCAPED Hostile Ships : "/* + _newline*/; }
                foreach (var _ship in item.EscapedShips)
                {
                    _allHostileAssetsText = CreateShipText(_ship, out string _shipText) + " > escaped";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                }

                //if (item.AssimilatedShips.Count > 0) { _allHostileAssetsText += _newline + "> ASSIMILATED Hostile Ships : "/* + _newline*/; }
                foreach (var _ship in item.AssimilatedShips)
                {
                    _allHostileAssetsText = CreateShipText(_ship, out string _shipText) + " > assimilated";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                }

                //if (item.CombatShips.Count > 0) { _allHostileAssetsText += _newline + "> CombatShips Hostile: "/* + _newline*/; }
                foreach (var _ship in item.CombatShips)
                {
                    _allHostileAssetsText = CreateShipText(_ship, out string _shipText) + " > alive";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                }

                //if (item.NonCombatShips.Count > 0) { _allHostileAssetsText += _newline + "> NonCombatShips Hostile: "/* + _newline*/; }
                foreach (var _ship in item.NonCombatShips)
                {
                    _allHostileAssetsText = CreateShipText(_ship, out string _shipText) + " > alive";
                    _leftSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                    _rightSideCivM.SitRepEntries.Add(new ReportEntry_CoS(_rightSideCiv, _loc, _allHostileAssetsText, "", "", SitRepPriority.Yellow));
                }
            }
            _text = "Step_309e:; end DoSitRepsAboutCombat..";
            if (_combatWriteDirectly) Console.WriteLine(_text);
        }


        public bool AllSidesStandDown() // ??? do we no longer care what the orders are - no longer have a second chance at setting orders?
        {
            string _text = "";
            //_text = "Step_3001: Now AllsideStandDown ={0} based on combat orders", AllSidesStandDown());
            string _newline = Environment.NewLine;
            bool _combatWriteDirectly = true;

            foreach (CombatAssets civAssets in _assets)
            {
                // Combat ships
                if (civAssets.CombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    _text = "Step_3091:; Combat ships - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += _newline + _text;
                    GameLog.Core.Combat.DebugFormat(_text);
                    return false;
                }
                // Non-combat ships
                if (civAssets.NonCombatShips.Select(unit => GetCombatOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports || order == CombatOrder.Formation))
                {
                    _text = "Step_3092:; NON Combat ships - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += _newline + _text;
                    GameLog.Core.Combat.DebugFormat(_text);

                    return false;
                }
                // Station
                if ((civAssets.Station != null) && (GetCombatOrder(civAssets.Station.Source) == CombatOrder.Engage || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Transports || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Rush || GetCombatOrder(civAssets.Station.Source) == CombatOrder.Formation))
                {
                    _text = "Step_3093:; Station - AllSidesStandDown is false";
                    if (_combatWriteDirectly) Console.WriteLine(_text);
                    _combatEngine_full_Report += _newline + _text;
                    GameLog.Core.Combat.DebugFormat(_text);

                    return false;
                }
            }
            _text = "Step_3098:; AllSidesStandDown is true";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += _newline + _text;
            GameLog.Core.Combat.DebugFormat(_text);
            return true;
        }

        public void SendInitialUpdate()
        {
            string _text = "Step_3007:; cEngine > Called SendInitalUpdate to now call SendUpdates()";
            bool _combatWriteDirectly = true;

            if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += _newline + _text;
            //GameLog.Core.Combat.DebugFormat(_text);

            SendUpdates(); // SendInitialUpdate
        }

        protected void SendUpdates()
        {
            string _text = "Step_3010:; cEngine > SendUpdates"; // Step_3020 is enough
            bool _combatWriteDirectly = true;

            if (_combatWriteDirectly) Console.WriteLine(_text);
            //_combatEngine_full_Report += _newline + _text;
            //bool _allreadySitRepDONE = false;

            foreach (CombatAssets _leftSideAssets in _assets) // _assets is list of current player (friend) assets so one list for our friends, friend's and other's asset are in asset (not _assets)
            {
                Civilization _leftSideCiv = _leftSideAssets.Owner;
                CivilizationManager _leftSideCivM = GameContext.Current.CivilizationManagers[_leftSideCiv.CivID];
                string _newline = Environment.NewLine;

                if (_leftSideCiv.IsHuman)
                {
                    //Debugger.Break();
                }

                List<CombatAssets> _friendlyAssets = new List<CombatAssets>();
                List<CombatAssets> _hostileAssets = new List<CombatAssets>();

                Universe.MapLocation _loc = _assets.First().Location;

                _friendlyAssets.Add(_leftSideAssets); // on each looping arbitrary one side or the other is 'friendly' for combatwindow right and left side
                                                      //foreach (CombatAssets asset in _assets)
                                                      //{
                                                      //    GameLog.Core.Combat.DebugFormat("asset of {0} in sector", asset.Owner.Key);
                                                      //}
                _text = "Step_3020:; " + _sectorString + " cEngine > SendUpdates for current _leftSideAssets = " + _leftSideAssets.Owner.Key;
                if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += _newline + _text; // > less output !!
                //GameLog.Core.Combat.DebugFormat(_text);

                int _civDurability = 0;

                //goto AfterCalculate_CivStrength_and_Durability;

                foreach (CombatAssets civAsset in _assets.Distinct().ToList())
                {
                    _text = "Step_3002:; beginning calculating empireStrengths for " 
                        + _leftSideCiv.Key 
                        //+ "{0} , current value =  for {0} {1} ({2}) = {3}", )
                        
                        ;
                    //if (_combatWriteDirectly) Console.WriteLine(_text);
                    //_combatEngine_full_Report += _newline + _text;

                    int _currentCivStrength = 0;
                    _civDurability = 0;

                    foreach (CombatAssets cs in _assets)  // only combat ships
                    {
                        //_text = "Step_3001: calculating empireStrengths"
                        //    + " for Ship.Owner=" + cs.Owner.Key
                        //    + " and Empire= " + civAsset.Owner.Key
                        //    ;
                        //if (_combatWriteDirectly) Console.WriteLine(_text);
                        //_combatEngine_full_Report += _newline + _text;

                        // for all > calculate EmpireStrength etc.
                        if (cs.Owner.Key == civAsset.Owner.Key)
                        {
                            //_text = "Step_3001: calculating empireStrengths for Ship.Owner = {0} and Empire {1}", cs.Owner.Key, civ.Owner.ToString());

                            foreach (CombatUnit ship in cs.CombatShips)
                            {
                                _currentCivStrength += ship.Firepower;
                                _civDurability += CalculateStrength_CombatShip_in_CombatEngine(ship);
                                //_text = "Step_3001: added Fire_power_calculated into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.Fire_Power_Ship);
                            }

                            foreach (CombatUnit ship in cs.NonCombatShips)
                            {
                                //_currentCivStrength += ship.Fire_power_calculated;
                                _civDurability += CalculateStrength_CombatShip_in_CombatEngine(ship);
                                //_text = "Step_3001: added Fire_power_calculated into {0} for {1} {2} ({3}) = {4}",
                                //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.Fire_Power_Ship);
                            }

                            if (cs.Station != null)
                            {
                                _currentCivStrength += cs.Station.Firepower;
                                _civDurability += cs.Station.Firepower + cs.Station.HullStrength + cs.Station.ShieldStrength;
                            }

                            if (_empireStrengths.Any(e => e.Key == cs.Owner.CivID))
                            {
                                ;
                                // already exist
                            }
                            else
                            {
                                try
                                {

                                    _empireStrengths.Add(civAsset.Owner.CivID, _currentCivStrength);
                                    _civDurabilities.Add(civAsset.Owner.CivID, _civDurability);
                                }
                                catch 
                                {
                                    
                                }
                            }
                        }
                    }
                    //to often
                    //_text = "Step_3030:; " + _sectorString + "SendUpdates: _currentCivStrength = " + _currentCivStrength + " for " + civAsset.Owner.Key;
                    //if (_combatWriteDirectly) Console.WriteLine(_text);
                    //_combatEngine_full_Report += _newline + _text;
                    //GameLog.Core.Combat.DebugFormat(_text);
                    
                    //_leftSideAssets = _leftSideAssets;

                    

                    //string _civ2 = "";
                    //        if (update.CivName2 != "")
                    //        {
                    //            _civ2 = update.CivName2;
                    //        }

                    //        string _civ3 = "";
                    //        if (update.CivName3 != "")
                    //        {
                    //            _civ3 = update.CivName3;
                    //        }

                    //        string _civ4 = "";
                    //        if (update.CivName4 != "")
                    //        {
                    //            _civ4 = update.CivName4;
                    //        }

                    //bool _allreadySitRepDONE = false;
                }


                foreach (CombatAssets _rightsideAssets in _assets) // _assets is all combat assest in sector while "_rightsideAssets" is not of type "friendly" first asset
                {
                    if (_rightsideAssets == _leftSideAssets)  // otherwise it is _rightSideAssets  !!
                    {
                        continue;
                    }

                    if (CombatHelper.WillFightAlongside(_leftSideCiv, _rightsideAssets.Owner))
                    {
                        _friendlyAssets.Add(_rightsideAssets);
                        _ = _friendlyAssets.Distinct().ToList();
                        //GameLog.Core.Combat.DebugFormat("asset of {0} added to friendlies", _rightsideAssets.Owner.Key);
                    }
                    else
                    {
                        _hostileAssets.Add(_rightsideAssets);
                        _ = _hostileAssets.Distinct().ToList();
                        //GameLog.Core.Combat.DebugFormat("asset for {0} added to hostilies", _rightsideAssets.Owner.Key);
                    }
                }

            //AfterCalculate_CivStrength_and_Durability:;

                List<CombatAssets> leftOutAssets = new List<CombatAssets>();
                foreach (CombatAssets missedAsset1 in _assets)
                {
                    if (!_friendlyAssets.Contains(missedAsset1) && !_hostileAssets.Contains(missedAsset1))
                    {
                        leftOutAssets.Add(missedAsset1);
                    }
                }

                foreach (CombatAssets friendlyAsset in _friendlyAssets)
                {
                    foreach (CombatAssets missedAsset2 in leftOutAssets)
                    {
                        IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[missedAsset2.Owner, friendlyAsset.Owner];
                        if (diplomacyData.Status == ForeignPowerStatus.OwnerIsMember ||
                            diplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember ||
                            diplomacyData.Status == ForeignPowerStatus.Allied)
                        {
                            _friendlyAssets.Add(missedAsset2);
                        }
                    }
                }

                //_allSidesStandDown = true; // cheat

                CombatUpdate update = new CombatUpdate(
                    CombatID,
                    _roundNumber,
                    _allSidesStandDown,
                    _leftSideCiv,
                    _leftSideAssets.Location,
                    _friendlyAssets,
                    _hostileAssets
                    );

                //AsyncHelper.Invoke(_updateCallback, this, update);

                // to often
                //_text = "Step_3041:; new CombatUpdate for " + _leftSideCiv;
                //if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += _newline + _text;
                // sends data back to combat window

                // ToDo>try to build in here the Sitreps
                //if (_leftSideAssets.DestroyedShips.Count > 0)
                //    _destroyedString = ", some ships got destroyed"
                //        ;
                //if (_leftSideAssets.EscapedShips.Count > 0)
                //    _escapedString = ", some ships escaped"
                //        ;
                //if (_leftSideAssets.CombatShips.Count > 0)
                //    _combatString = ", combat ships survived"
                //        ;
                //if (_leftSideAssets.NonCombatShips.Count > 0)
                //    _nonCombatString = ", non-combat ships survived"
                //        ;

                //_text = 
                //    //_leftSideAssets.Location.ToString()
                //    /*+ */"Strength " + update.FriendlyEmpireStrength + " vs " + update.AllHostileEmpireStrength
                //    + _escapedString
                //    + _destroyedString
                //    ;

                //GameContext.Current.CivilizationManagers[_leftSideCiv].SitRepEntries.Add(new ReportEntry_CoS(_leftSideCiv, _loc, _text, "", "", SitRepPriority.Red));


                AsyncHelper.Invoke(_updateCallback, this, update);

                // to often
                //_text = "Step_3048:; _updateCallback for " + _leftSideCiv;
                //if (_combatWriteDirectly) Console.WriteLine(_text);
                //_combatEngine_full_Report += _newline + _text;
            }
        }

        private string CreateShipText(CombatUnit _ship, out string _shipText)
        {
            string _ownerString = _ship.Owner.ShortName;
            if (_ownerString.Length > 15)
            {
                _ownerString = _ownerString.Substring(0,14) + ".";
            }

            while (_ownerString.Length < 16)
            {
                _ownerString += " ";
            }

            _shipText = /*Environment.NewLine +*/
                "Red Alert at " + _ship.Source.Location
        + " > " + _ownerString
        + " > H: " + GameEngine.Do_x_Digit_String( 3, _ship.HullIntegrity.ToString())
        + ", S: " + GameEngine.Do_x_Digit_String( 3, _ship.ShieldIntegrity.ToString())
        + " - " + _ship.Source.OrbitalDesign.Key

        + " > " + GameEngine.Do_x_Digit_String( 5, _ship.Source.ObjectID.ToString())

        + " - " + _ship.Source.Name /*+ " > "*/;
            return _shipText;
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
        //    return Convert.ToInt32(Convert.ToDouble(cs.Fire_power_calculated)
        //            + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
        //            * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        //}


        /// <summary>
        /// Remove the assets of defeated players from the combat
        /// </summary>
        private void RemoveDefeatedPlayers()
        {
            string _text = "";
            bool _combatWriteDirectly = true;
            // CHANGE X
            for (int i = 0; i < _assets.Count; i++)
            {
                _text = "Step_3048:; " + _sectorString + " > Surviving assets for " + _assets[i].Owner.Key + ": " + _assets[i].HasSurvivingAssets;
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);

                if (!_assets[i].HasSurvivingAssets)
                {
                    _text = "Step_3049:; " + _sectorString + " > remove defeated assets for Player " + _assets[i].Owner.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Combat.DebugFormat(_text);

                    _assets.RemoveAt(i--);
                }
            }
            _text = "Step_3009:; --------------------";
            if (_combatWriteDirectly) Console.WriteLine(_text);
            _combatEngine_full_Report += Environment.NewLine + _text;
            //GameLog.Core.Combat.DebugFormat(_text);
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
            _empireStrengths = new Dictionary<int, int>();
            string _text = "";
            foreach (Tuple<CombatUnit, CombatWeapon[]> combatShip in _combatShips)
            {
                if (!_empireStrengths.ContainsKey(combatShip.Item1.Owner.CivID))
                {
                    _empireStrengths[combatShip.Item1.Owner.CivID] = 0;
                }
                _empireStrengths[combatShip.Item1.Owner.CivID] += combatShip.Item1.Source.Fire_power_calculated();
            }
            if (_combatStation != null)
            {
                if (!_empireStrengths.ContainsKey(_combatStation.Item1.Owner.CivID))
                {
                    _empireStrengths[_combatStation.Item1.Owner.CivID] = 0;
                }
                _empireStrengths[_combatStation.Item1.Owner.CivID] += _combatStation.Item1.Source.Fire_power_calculated();
            }

            foreach (KeyValuePair<int, int> empire in _empireStrengths)
            {
                _text = "Step_3053:; " + _sectorString
                    + " > " + GameEngine.Do_x_Digit_String( 5, empire.Value.ToString())
                    + " = Strength for civID " + empire.Key;
                // Detailed_Log(_text);
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);
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
                    int gainedResearchPoints = assimiltedCivHome.Research_Net;
                    Universe.Sector destination = CombatHelper.CalculateRetreatDestination(assets);
                    Ship ship = (Ship)assimilatedShip.Source;
                    ship.Owner = borg;
                    Fleet newfleet = ship.CreateFleet();
                    newfleet.Location = destination.Location;
                    newfleet.Owner = borg;
                    newfleet.SetOrder(FleetOrders.IdleOrder.Create()); // AssimilatedShips
                                                                       //if (newfleet.Order == null)
                                                                       //{
                                                                       //    newfleet.SetOrder(FleetOrders.AvoidOrder.Create());
                                                                       //}
                    ship.IsAssimilated = true;
                    ship.Scrap = false;
                    newfleet.Name = "Assimilated Assets";
                    GameContext.Current.CivilizationManagers[borg].Research.UpdateResearch(gainedResearchPoints);

                    string _text = /*"Step_3021:; " + */ship.Location
                        + " > Ship assimilated: " + ship.ObjectID + " * " + ship.Name + " * ( " + ship.Design + " )";

                    // Detailed_Log(_text);
                    Console.WriteLine("Step_3021:;" + _text);
                    //GameLog.Core.Combat.DebugFormat("Step_3021:;" + _text);
                    //Detailed_Log(_sectorString + "Assimilated Assets: {0} {1}, Owner = {2}, OwnerID = {3}, Fleet.OwnerID = {4}, Order = {5} gainedResearchPoints ={6}",
                    //    ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order, gainedResearchPoints);

                    GameContext.Current.CivilizationManagers[assimilatedCiv].SitRepEntries.Add(new ReportEntry_CoS(assimilatedCiv, ship.Location, _text, "", "", SitRepPriority.Red));
                    //GameContext.Current.CivilizationManagers[assimilatedCiv].SitRepEntries.Add(new ShipAssimilatedSitRepEntry(assimilatedCiv, ship.Location, _text));

                    //for Borg only: 
                    _text += ": We gained " + gainedResearchPoints + " research points";
                    GameContext.Current.CivilizationManagers[borg].SitRepEntries.Add(new ReportEntry_CoS(borg, ship.Location, _text, "", "", SitRepPriority.Yellow));
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
                string _text = "Step_3093:; " + _sectorString + "_combatId = " + CombatID + " > PerformRetreat begins";
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);

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
                            //GameLog.Core.Combat.DebugFormat(_text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string _text = "Step_3027:; " + _sectorString + "##### Problem at PerformRetreat" + Environment.NewLine + e;
                Console.WriteLine(_text);
                GameLog.Core.Combat.DebugFormat(_text);
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

            //    Detailed_Log(_sectorString + "Try Get Order for " + source.ObjectID + " " + source.Name + " " + source.Design.Name);
            //    //if(_orders[source.OwnerID].GetOrder(source) == CombatOrder.)

            //    _localOrder = _orders[source.OwnerID].GetOrder(source);
            //    //_localOrder = _orders[_orders.Count-1].GetOrder(source);
            //    Detailed_Log(_sectorString + "Got Order for " + source.ObjectID + " " + source.Name + " " + source.Design.Name
            //        + " -> order = " + _orders[source.OwnerID].GetOrder(source));
            //    return _localOrder; // this is the class CombatOrder.BORG (or FEDERATION or.....) that comes from public GetCombatOrder() in CombatOrders.cs
            //}
            //catch //(Exception e)
            //{
            //    //if (source.Owner.IsHuman == false)
            //    //{
            //    //    // Gamelog works but makes no sense ... or "nothing to win" with this error message (Example: Scout, before Engage, here fails 
            //    Detailed_Log(_sectorString + "Returning Engage due to > Unable to get order for " + source.ObjectID + " " + source.Name + " " + source.Design.Name + " " + source.Owner.Name
            //        /*+ _newline + e*/);
            //    _localOrder = CombatOrder.Engage;
            //    //}
            //    //GameLog.LogException(e);
            //}

            //Detailed_Log(_sectorString + "Setting order for " + source.ObjectID + " " + source.Name + " " + source.Design.Name 
            //    + " (Owner= " + source.Owner.Name + ") > " + _localOrder.ToString());
            return _localOrder; //CombatOrder.Engage; // not set to retreat because easy retreat in automatedCE will take ship out of combat by default
        }

        //private void Detailed_Log(string _rep)
        //{
        //    Console.WriteLine(_rep);
        //    GameLog.Core.Combat.DebugFormat(_rep);
        //}

        //public void SetCombatOrder(Orbital source, CombatOrder order)
        //{
        //    Dictionary<int, CombatOrder> dictionary = new Dictionary<int, CombatOrder>();
        //    dictionary.Add( source.ObjectID, order);
        //    _orders[source.OwnerID].Add(source.OwnerID, dictionary);
        //}

        protected Civilization GetTargetOne(Orbital source)
        {
            string _text = "";
            if (_targetOneByCiv.Keys.Contains(source.OwnerID))
            {
                _text = "Step_3021:; "
                    + _sectorString + " > GetTargetOne = " + _targetOneByCiv[source.OwnerID].GetTargetOne(source)
                    + " ( 888 = only return fire, 777 = no target)"
                    ;//if (targetCiv == null)  
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);
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
                string _text = "Step_3022:; " + _sectorString + " > GetTargetTwo = " + _targetTwoByCiv[source.OwnerID].GetTargetTwo(source);
                Console.WriteLine(_text);
                //GameLog.Core.Combat.DebugFormat(_text);
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

