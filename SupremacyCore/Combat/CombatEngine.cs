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

        private readonly int _combatId;
        private int _roundNumber;
        private bool _running;
        private bool _allSidesStandDown;
        private bool _ready;
        private readonly IList<CombatAssets> _assets;
        private readonly SendCombatUpdateCallback _updateCallback;
        private readonly NotifyCombatEndedCallback _combatEndedCallback;
        private readonly Dictionary<int, CombatOrders> _orders;

        private bool _combatEngineTraceLocally = false;    // turn to true if you want

        protected int CombatID
        {
            get { return _combatId; }
        }

        protected int RoundNumber
        {
            get { return _roundNumber;  }
        }

        protected IList<CombatAssets> Assets
        {
            get { return _assets; }
        }

        protected Dictionary<int, CombatOrders> Orders
        {
            get { return _orders; }
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
                    return true;
                //if (_assets.Count(assets => assets.CombatShips.);
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
            IList<CombatAssets> assets,
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
        }

        public void SubmitOrders(CombatOrders orders)
        {
            lock (SyncLock)
            {
                if (!_orders.ContainsKey(orders.OwnerID))
                    _orders[orders.OwnerID] = orders;

                var outstandingOrders = Assets.Select(assets => assets.OwnerID).ToList();

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

                foreach (var asset in _assets)
                    asset.CombatID = _combatId;

                if ((RoundNumber > 1) || !CheckAllSidesStandDown())
                    ResolveCombatRoundCore();

                UpdateOrbitals();

                PerformAssimilation();

                PerformRetreat();

                isCombatOver = IsCombatOver;

                if (!isCombatOver)
                    _roundNumber++;

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
            var result = true;

            foreach (var civAssets in Assets)
            {
                //if (_combatEngineTraceLocally == true)
                    //GameLog.Print("civAssets: {0}", civAssets.);

                if (civAssets.CombatShips.Select(unit => GetOrder(unit.Source)).Any(order => order == CombatOrder.Engage || order == CombatOrder.Rush || order == CombatOrder.Transports ))
                {
                    result = false;
                }
                if (!result) { break; }

                if (civAssets.NonCombatShips.Any(unit => GetOrder(unit.Source) == CombatOrder.Engage || GetOrder(unit.Source) == CombatOrder.Rush || GetOrder(unit.Source) == CombatOrder.Transports))
                {
                    result = false;
                }
                if (!result) { break; }

                // Station
                if (civAssets.Station == null)
                {
                    continue;
                }

                if (GetOrder(civAssets.Station.Source) == CombatOrder.Engage || GetOrder(civAssets.Station.Source) == CombatOrder.Transports)
                {
                    continue;
                }

                //if (civAssets.CombatShips.Select(unit => GetOrder(unit.Source)).Any(order => order == CombatOrder.Rush))
                //{
                //    result = false;
                //}
                //result = false;
                //break;

            }
            //foreach (var ship in this.Assets)
            //{

            //    if (ship.NonCombatShips.Any(ShipType => ShipType.IsCamouflaged))
            //    return true;
            //    break;
            //}

                _allSidesStandDown = result;
            // this game is confussing      GameLog.Client.GameData.DebugFormat("Combat: allSidesStandDown={0}", result);

            return result;
        }

        public void SendInitialUpdate()
        {
            SendUpdates();
        }

        private void SendUpdates()
        {
            foreach (var playerAssets in _assets)
            {
                var owner = playerAssets.Owner;
                var friendlyAssets = new List<CombatAssets>();
                var assimilatedAssets = new List<CombatAssets>();
                var hostileAssets = new List<CombatAssets>();

                friendlyAssets.Add(playerAssets);

                foreach (var otherAssets in _assets)
                {
                    if (otherAssets == playerAssets)
                        continue;
                    if (CombatHelper.WillEngage(owner, otherAssets.Owner))
                    {
                        hostileAssets.Add(otherAssets);
                        // works 
                        //GameLog.Print("Combat: otherAssets of Owner {0}", otherAssets.Owner);
                    }
                    else
                    {
                        friendlyAssets.Add(otherAssets);
                        if (_combatEngineTraceLocally)
                            GameLog.Print("Combat: add other asset to friendly assets, otherAssets.Owner= {0}", otherAssets.Owner);
                    } 

                }

                //new stuff
                if (hostileAssets.Count == 0)
                {
                    //if (_combatEngineTraceLocally == true)   // next GameLog please always into Log.txt
                        GameLog.Print("Combat: hostileAssets.Count == 0, no combat will be shown due to missing enemy");
                    _allSidesStandDown = true;
                    AsyncHelper.Invoke(_combatEndedCallback, this);   // if hostileAssets = 0 then don't show a combat window and send a "combatEnded"
                    break;
                    //return;
                    //continue;
                }

                //GameLog.Client.GameData.DebugFormat("Combat: new CombatUpdate");
                var update = new CombatUpdate(
                    _combatId,
                    _roundNumber,
                    _allSidesStandDown,
                    owner,
                    playerAssets.Location,
                    friendlyAssets,
                    hostileAssets);

                //GameLog.Client.GameData.DebugFormat("CombatUpdate: Location={4} ## ID={0} Turn={1} ##  {2}_to_AllSideStandDown, AmountFriendlyUnits={5}, AmountHostileUnits={6} Owner= ## {3}",

                if (_combatEngineTraceLocally)
                    GameLog.Print("CombatUpdate: Location={4} ## ID={0} Turn={1} ##  {2}_to_AllSideStandDown, Owner = ## {3}",
                    _combatId,
                    _roundNumber,
                    _allSidesStandDown,
                    owner,
                    playerAssets.Location
                    //friendlyAssets.Count,  // doesn't work, always counting 1 friendly (race), maybe 2   (e.g. Fed+Rom)
                    //hostileAssets.Count    // doesn't work, mostly counting 1 hostile (race)             (e.g. Kling+Dom)
                    );

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
                    _assets.RemoveAt(i--);
            }
        }

        private void UpdateOrbitals()
        {
            foreach (CombatAssets assets in Assets)
            {
                assets.UpdateAllSources();
                //if (_combatEngineTraceLocally == true)
                // doesn't work      GameLog.Client.GameData.DebugFormat("Combat: CombatUnits={0}", assets.CombatUnits);
            }
        }

        private void PerformAssimilation()
        {
            foreach (var assets in Assets)
            {
                if (assets.Owner.Name == "Borg")
                    break;

                for (var i = 0; i < assets.CombatShips.Count; i++)
                {
                    if (assets.CombatShips[i].IsAssimilated)
                    {
                        assets.AssimilatedShips.Add(assets.CombatShips[i]);
                        assets.CombatShips.RemoveAt(i--);
                    }
                }

                for (var i = 0; i < assets.NonCombatShips.Count; i++)
                {
                    if (assets.NonCombatShips[i].IsAssimilated)
                    {
                        assets.AssimilatedShips.Add(assets.NonCombatShips[i]);
                        assets.NonCombatShips.RemoveAt(i--);
                    }
                }
                
                if (assets.AssimilatedShips.Count == 0)
                    continue;

                //var destination = CalculateRetreatDestination(assets);

                //if (destination == null)
                //    continue;

                foreach (var shipStats in assets.AssimilatedShips)
                {
                    var _ship = ((Ship)shipStats.Source);

                    _ship.Fleet.OwnerID = 7;
                    _ship.Fleet.SetOrder(_ship.Fleet.GetDefaultOrder());
                    _ship.Scrap = false;
                    GameLog.Print("Fleet={0}, FleetID ={1}, FleetOrder ={2}, FleetOwner ={3}, FleetOwnerID ={4}, ShipName={5}, ShipID={6}",
                        _ship.Fleet.Name, _ship.Fleet.ObjectID, _ship.Fleet.Order.ToString(), _ship.Fleet.Owner.Name, _ship.Fleet.OwnerID.ToString(), _ship.Name, _ship.ObjectID);
                }
            }
        }

        private void PerformRetreat()
        {
            

            foreach (var assets in Assets)
            {
                //if (assets.AssimilatedShips.First == true)
                //    break;

                for (var i = 0; i < assets.CombatShips.Count; i++)
                {
                    if (assets.CombatShips[i].Owner.Name == "Borg")
                        break;

                    if (GetOrder(assets.CombatShips[i].Source) != CombatOrder.Retreat)
                        continue;
                    assets.EscapedShips.Add(assets.CombatShips[i]);
                    assets.CombatShips.RemoveAt(i--);
                }

                for (var i = 0; i < assets.NonCombatShips.Count; i++)
                {
                    if (assets.NonCombatShips[i].Owner.Name == "Borg")
                        break;

                    if (GetOrder(assets.NonCombatShips[i].Source) != CombatOrder.Retreat)
                        continue;
                    assets.EscapedShips.Add(assets.NonCombatShips[i]);
                    assets.NonCombatShips.RemoveAt(i--);
                }

                if (assets.EscapedShips.Count == 0)
                    continue;
                
                var destination = CalculateRetreatDestination(assets);

                if (destination == null)
                    continue;

                foreach (var shipStats in assets.EscapedShips)
                    ((Ship)shipStats.Source).Fleet.Location = destination.Location;
            }
        }

        private static Sector CalculateRetreatDestination(CombatAssets assets)
        {
            var nearestFriendlySystem = GameContext.Current.Universe.FindNearestOwned<Colony>(
                assets.Location,
                assets.Owner);

            var sectors =
                (
                    from s in assets.Sector.GetNeighbors()
                    let distance = MapLocation.GetDistance(s.Location, nearestFriendlySystem.Location)
                    let hostileOrbitals = GameContext.Current.Universe.FindAt<Orbital>(s.Location).Where(o => o.OwnerID != assets.OwnerID && o.IsCombatant)
                    let hostileOrbitalPower = hostileOrbitals.Sum(o => CalculateOrbitalPower(o))
                    orderby hostileOrbitalPower ascending, distance descending
                    select s
                );

            return sectors.FirstOrDefault();
        }

        private static int CalculateOrbitalPower(Orbital orbital)
        {
            return (orbital.OrbitalDesign.PrimaryWeapon.Damage * orbital.OrbitalDesign.PrimaryWeapon.Count) +
                   (orbital.OrbitalDesign.SecondaryWeapon.Damage * orbital.OrbitalDesign.SecondaryWeapon.Count);
        }

        protected CombatAssets GetAssets(Civilization owner)
        {
            return Assets.FirstOrDefault(assets => assets.Owner == owner);
        }

        protected CombatOrder GetOrder(Orbital source)
        {
            try
            {
                //GameLog.Print("source.ObjectID {0}, source.Name {1}", source.ObjectID, source.Name);
                GameLog.Print("source.ObjectID {0}, source.Name {1}, Order {2}", source.ObjectID, source.Name, Orders[source.OwnerID].GetOrder(source));
                return Orders[source.OwnerID].GetOrder(source);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.Print("Problem for GetOrder for source.ObjectID {0}, source.Name {1}", source.ObjectID, source.Name);
                GameLog.LogException(e);
            }

            GameLog.Print("source.ObjectID {0}, source.Name {1}, returning RETREAT is backup order {2}", source.ObjectID, source.Name, Orders[source.OwnerID].GetOrder(source));
            return CombatOrder.Retreat;
        }

        protected abstract void ResolveCombatRoundCore();
    }
}
