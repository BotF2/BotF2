// CombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    public delegate void SendCombatUpdateCallback(CombatEngine engine, CombatUpdate update);
    public delegate void NotifyCombatEndedCallback(CombatEngine engine);

    public abstract class CombatEngine
    {
        public readonly object SyncLock;
        protected const double BaseChanceToRetreat = 0.75;
        protected const double BaseChanceToAssimilate = 0.20;
        protected const double BaseChanceToRushFormation = 0.50;
        protected readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        protected readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips;
        protected Tuple<CombatUnit, CombatWeapon[]> _combatStation;
        private readonly int _combatId;
        private int _roundNumber;
        private bool _running;
        private bool _allSidesStandDown;
        private bool _ready;
        protected readonly List<CombatAssets> _assets;
        private readonly SendCombatUpdateCallback _updateCallback;
        private readonly NotifyCombatEndedCallback _combatEndedCallback;
        private readonly Dictionary<int, CombatOrders> _orders;

        protected bool _traceCombatEngine = true;

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
                lock (SyncLock)
                {
                    if (Running || IsCombatOver)
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
                throw new ArgumentNullException("assets");
            if (updateCallback == null)
                throw new ArgumentNullException("updateCallback");
            if (combatEndedCallback == null)
                throw new ArgumentNullException("combatEndedCallback");

            _running = false;
            _allSidesStandDown = false;
            _combatId = GameContext.Current.GenerateID();
            _roundNumber = 1;
            _assets = assets;
            _updateCallback = updateCallback;
            _combatEndedCallback = combatEndedCallback;
            _orders = new Dictionary<int, CombatOrders>();

            SyncLock = _orders;

            // TODO: This looks like a waste of time, table is not going to change from one combat to the other
            // Consider creating the table once instead
            var accuracyTable = GameContext.Current.Tables.ShipTables["AccuracyModifiers"];
            _experienceAccuracy = new Dictionary<ExperienceRank, double>();
            foreach (ExperienceRank rank in EnumUtilities.GetValues<ExperienceRank>())
            {
                double modifier;
                if (Double.TryParse(accuracyTable[rank.ToString()][0], out modifier))
                {
                    _experienceAccuracy[rank] = modifier;
                }
                else
                    _experienceAccuracy[rank] = 0.75;
            }
            ///////////////////

            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();

            foreach (CombatAssets civAssets in _assets)
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
            lock (SyncLock)
            {
                if (!_orders.ContainsKey(orders.OwnerID))
                    _orders[orders.OwnerID] = orders;

                var outstandingOrders = _assets.Select(assets => assets.OwnerID).ToList();

                lock (_orders)
                {
                    foreach (var civId in _orders.Keys)
                        outstandingOrders.Remove(civId);

                    if (outstandingOrders.Count == 0)
                        _ready = true;
                }
            }
        }

        public void ResolveCombatRound()
        {
            bool isCombatOver;

            lock (_orders)
            {
                Running = true;

                _assets.ForEach(a => a.CombatID = _combatId);

                _allSidesStandDown = CheckAllSidesStandDown();
                if ((_roundNumber > 1) || _allSidesStandDown)
                {
                    ResolveCombatRoundCore();
                }
                if (GameContext.Current.Options.BorgPlayable == EmpirePlayable.Yes)
                {
                    PerformAssimilation();
                }           
                PerformRetreat();
                UpdateOrbitals();

                isCombatOver = IsCombatOver;

                if (!isCombatOver)
                {
                    _roundNumber++;
                }

                _orders.Clear();
            }

            SendUpdates();

            RemoveDefeatedPlayers();

            Running = false;

            if (isCombatOver)
            {
                AsyncHelper.Invoke(_combatEndedCallback, this);
            }
        }

        private bool CheckAllSidesStandDown()
        {
            foreach (var civAssets in _assets)
            {
                // Combat ships
                if (civAssets.CombatShips.Select(unit => GetOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports ||  order == CombatOrder.Formation))
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
            foreach (var playerAsset in _assets)
            {
                var owner = playerAsset.Owner;
                var friendlyAssets = new List<CombatAssets>();
                var hostileAssets = new List<CombatAssets>();

                friendlyAssets.Add(playerAsset);

                foreach (var otherAsset in _assets)
                    //var otherOwner = otherAsset.Owner;
                {
                    if (otherAsset == playerAsset)
                        continue;
                    if (CombatHelper.WillEngage(owner, otherAsset.Owner))
                    {
                        hostileAssets.Add(otherAsset);
                    }
                    //if (owner.Name == "Borg" && otherAsset.Owner.Name == "Borg")
                    //{
                    //    hostileAssets.Remove(otherAsset);
                    //}
                    else
                    {
                        friendlyAssets.Add(otherAsset);
                        if (_traceCombatEngine)
                        {
                            GameLog.Print("Combat: add other asset to friendly assets, otherAsset.Owner= {0}", otherAsset.Owner);
                        }
                    } 

                }

                if (hostileAssets.Count == 0)
                {
                        GameLog.Print("Combat: hostileAssets.Count == 0, no combat will be shown due to missing enemy");
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
                    hostileAssets);

                if (_traceCombatEngine)
                {
                    GameLog.Print("CombatUpdate: Location={4} ## ID={0} Turn={1} ##  {2} to AllSideStandDown, Owner = ## {3}",
                        _combatId,
                        _roundNumber,
                        _allSidesStandDown,
                        owner,
                        playerAsset.Location
                    );
                }

                if (GameContext.Current.Options.GalaxyShape.ToString() == "Cluster-not-now")   // correct value is "Cluster" - just remove "-not-now" to disable Combats (done! and) shown
                {
                    GameLog.Print("GameContext.Current.Options.GalaxyShape = {0}", GameContext.Current.Options.GalaxyShape);
                    GameLog.Print("Combat is turned off");
                    AsyncHelper.Invoke(_combatEndedCallback, this);   // if hostileAssets = 0 then don't show a combat window and send a "combatEnded"
                    break;
                }

                AsyncHelper.Invoke(_updateCallback, this, update);
            }
        }

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
        /// Performs the assimilation of ships that have been assimilated
        /// </summary>
        private void PerformAssimilation()
        {
            var borgCivID = GameContext.Current.Civilizations.First(c => c.Name == "Borg").CivID;
            var borgCivilization = GameContext.Current.Civilizations.First(c => c.Name == "Borg");
            var borgShipPrefix = borgCivilization.ShipPrefix;

            foreach (var assets in _assets)
            {
                foreach (var assimilatedShip in assets.AssimilatedShips)
                {
                    var _ship = (Ship)assimilatedShip.Source;
                    _ship.OwnerID = borgCivID;
                    _ship.Fleet.OwnerID = borgCivID;
                    _ship.Fleet.SetOrder(FleetOrders.EngageOrder.Create());
                    if (_ship.Fleet.Order == null)
                        _ship.Fleet.SetOrder(FleetOrders.AvoidOrder.Create());
                    _ship.IsAssimilated = true;
                    _ship.Scrap = false;
                    _ship.Fleet.Name = "Assimilated Assets";
                    _ship.Fleet.Owner = borgCivilization;
                   
                    GameLog.Print("Assismilated Assets: {0} {1}, Owner = {2}, OwnerID = {3}, Fleet.OwnerID = {4}, Order = {5}", 
                        _ship.ObjectID, _ship.Name, _ship.Owner, _ship.OwnerID, _ship.Fleet.OwnerID, _ship.Fleet.Order);
                }
            }
        }

        /// <summary>
        /// Calculates the best sector for the given <see cref="CombatAssets"/> to retreat to
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        private Sector CalculateRetreatDestination(CombatAssets assets)
        {
            var nearestFriendlySystem = GameContext.Current.Universe.FindNearestOwned<Colony>(
                assets.Location,
                assets.Owner);

            var sectors =
                (
                    from s in assets.Sector.GetNeighbors()
                    let distance = MapLocation.GetDistance(s.Location, nearestFriendlySystem.Location)
                    let hostileOrbitals = GameContext.Current.Universe.FindAt<Orbital>(s.Location).Where(o => o.OwnerID != assets.OwnerID && o.IsCombatant)
                    let hostileOrbitalPower = hostileOrbitals.Sum(o => CombatHelper.CalculateOrbitalPower(o))
                    orderby hostileOrbitalPower ascending, distance descending
                    select s
                );

            return sectors.FirstOrDefault();
        }

        /// <summary>
        /// Moves ships that have escaped to their destinations
        /// </summary>
        private void PerformRetreat()
        {
            foreach (var assets in _assets)
            {               
                var destination = CalculateRetreatDestination(assets);

                if (destination == null)
                    continue;

                foreach (var shipStats in assets.EscapedShips)
                    ((Ship)shipStats.Source).Fleet.Location = destination.Location;
            }
        }

        /// <summary>
        /// Returns the <see cref="CombatAssets"/> that belong to the given <see cref="Civilization"/>
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
                return _orders[source.OwnerID].GetOrder(source);
            }
            catch (Exception e)
            {
                GameLog.Print("Unable to get order for {0}", source.Name);
                GameLog.LogException(e);
            }

            if (_traceCombatEngine == true)
            {
                GameLog.Print("Setting Retreat as fallback order for {0}", source.Name);
            }
            return CombatOrder.Retreat;
        }

        protected abstract void ResolveCombatRoundCore();
    }
}
