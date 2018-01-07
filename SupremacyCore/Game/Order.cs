// Order.cs
// 
// Copyright (c) 2011 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Orbitals;
using Supremacy.Personnel;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// The base class for all game orders issued by human players.
    /// </summary>
    [Serializable]
    public abstract class Order
    {
        [NonSerialized] private GameContext _executionContext;
        [NonSerialized] private bool _isExecuted;
        private short _ownerId;

        /// <summary>
        /// Gets or sets the owner ID.
        /// </summary>
        /// <value>The owner ID.</value>
        public int OwnerID
        {
            get { return _ownerId; }
            set
            {
                if (value < 0)
                    value = Civilization.InvalidID;
                _ownerId = (short)value;
            }
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner
        {
            get
            {
                if (_ownerId == Civilization.InvalidID)
                    return null;
                if (_executionContext != null)
                    return _executionContext.Civilizations[_ownerId];
                return GameContext.Current.Civilizations[_ownerId];
            }
            set
            {
                if (value == null)
                {
                    _ownerId = (short)Civilization.InvalidID;
                    return;
                }
                _ownerId = (short)value.CivID;
            }
        }

        /// <summary>
        /// Gets the execution context.
        /// </summary>
        /// <value>The execution context.</value>
        protected GameContext ExecutionContext
        {
            get { return _executionContext; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Order"/> has been executed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Order"/> has been executed; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecuted
        {
            get { return _isExecuted; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        protected Order(Civilization owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            _ownerId = (short)owner.CivID;
            _executionContext = null;
            _isExecuted = false;
        }

        /// <summary>
        /// When overridden by a subclass, this method performs the actual execution of an <see cref="Order"/>.
        /// </summary>
        /// <returns><c>true</c> if execution was successful; otherwise, <c>false</c>.</returns>
        public abstract bool DoExecute();

        /// <summary>
        /// When overridden by a subclass, this method undoes the execution of an <see cref="Order"/>.
        /// </summary>
        /// <returns><c>true</c> if undo was successful; otherwise, <c>false</c>.</returns>
        public virtual bool DoUndo()
        {
            return false;
        }

        /// <summary>
        /// Determines whether this <see cref="Order"/> overrides the specified <see cref="Order"/>.
        /// </summary>
        /// <param name="otherOrder">The other <see cref="Order"/>.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="Order"/> overrides <paramref name="otherOrder"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is used to determine if whether a previously submitted <see cref="Order"/> can be
        /// scrapped in favor of this <see cref="Order"/>.
        /// </remarks>
        public virtual bool Overrides(Order otherOrder)
        {
            return false;
        }

        /// <summary>
        /// Execute this <see cref="Order"/> on the client.
        /// </summary>
        public void ClientExecute()
        {
            Execute(GameContext.Current, false);
        }

        /// <summary>
        /// Execute this <see cref="Order"/> on the server.
        /// </summary>
        public void Execute()
        {
            Execute(GameContext.Current);
        }

        /// <summary>
        /// Execute this <see cref="Order"/> on the server.
        /// </summary>
        /// <param name="game">The execution context.</param>
        public void Execute(GameContext game)
        {
            Execute(game, true);
        }

        /// <summary>
        /// Execute this <see cref="Order"/> on the server.
        /// </summary>
        /// <param name="game">The execution context.</param>
        /// <param name="setExecuted">Whether to mark this <see cref="Order"/> as executed.</param>
        private void Execute(GameContext game, bool setExecuted)
        {
            if (_isExecuted)
                return;

            lock (this)
            {
                _executionContext = game;

                GameContext.PushThreadContext(_executionContext);

                try
                {
                    if (DoExecute() && setExecuted)
                        _isExecuted = true;
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            }
        }

        /// <summary>
        /// Undo this <see cref="Order"/> on the server.
        /// </summary>
        public void Undo()
        {
            if (!_isExecuted)
                return;
            lock (this)
            {
                GameContext.PushThreadContext(_executionContext);
                try
                {
                    if (DoUndo())
                    {
                        _isExecuted = false;
                        _executionContext = null;
                    }
                }
                finally
                {
                    GameContext.PopThreadContext();
                }
            }
        }
    }

    [Serializable]
    public sealed class CreateFleetOrder : Order
    {
        private readonly int _fleetId;
        private readonly MapLocation _location;

        public CreateFleetOrder(Civilization owner, int fleetId, MapLocation location)
            : base(owner)
        {
            _fleetId = fleetId;
            _location = location;
        }

        public CreateFleetOrder(Civilization owner, Fleet fleet)
            : base(owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _location = fleet.Location;
        }

        public CreateFleetOrder(Fleet fleet)
            : base(fleet.Owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _location = fleet.Location;
        }

        public override bool DoExecute()
        {
            if (GameContext.Current.Universe.Objects[_fleetId] != null)
                return false;

            var fleet = new Fleet
                        {
                            ObjectID = _fleetId,
                            OwnerID = OwnerID,
                            Location = _location
                        };

            GameContext.Current.Universe.Objects.Add(fleet);

            return true;
        }
    }

    [Serializable]
    public sealed class RedeployShipOrder : Order
    {
        private readonly int _shipId;
        private readonly int _targetFleetId;

        public RedeployShipOrder(Civilization owner, Ship ship)
            : this(owner, ship, ship.Fleet) {}

        public RedeployShipOrder(Civilization owner, Ship ship, Fleet targetFleet)
            : base(owner)
        {
            if (ship == null)
                throw new ArgumentNullException("ship");
            if (targetFleet == null)
                throw new ArgumentNullException("targetFleet");
            _shipId = ship.ObjectID;
            _targetFleetId = targetFleet.ObjectID;
        }

        public RedeployShipOrder(Ship ship)
            : this(ship, ship.Fleet) {}

        public RedeployShipOrder(Ship ship, Fleet targetFleet)
            : base(ship.Owner)
        {
            if (ship == null)
                throw new ArgumentNullException("ship");
            if (targetFleet == null)
                throw new ArgumentNullException("targetFleet");
            _shipId = ship.ObjectID;
            _targetFleetId = targetFleet.ObjectID;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as RedeployShipOrder;

            if (otherOrder == null)
                return false;

            return ((otherOrder._shipId == _shipId)
                    && (otherOrder._targetFleetId == _targetFleetId));
        }
        public override bool DoExecute()
        {
            var ship = GameContext.Current.Universe.Objects[_shipId] as Ship;
            var targetFleet = GameContext.Current.Universe.Objects[_targetFleetId] as Fleet;

            if ((ship == null) || (targetFleet == null))
                return false;

            ship.Fleet = targetFleet;
            return true;
        }
    }

    //[Serializable]
    //    public sealed class RedeployIntelOrder : Order
    //    {
    //        private readonly int _intelId;
    //        private readonly int _targetFleetId;

    //        public RedeployIntelOrder(Civilization owner, Intel intel)
    //            : this(owner, intel, intel.Fleet) { }

    //        public RedeployIntelOrder(Civilization owner, Intel intel, Fleet targetFleet)
    //            : base(owner)
    //        {
    //            if (intel == null)
    //                throw new ArgumentNullException("intel");
    //            if (targetFleet == null)
    //                throw new ArgumentNullException("targetFleet");
    //            _intelId = intel.ObjectID;
    //            _targetFleetId = targetFleet.ObjectID;
    //        }

    //        public RedeployIntelOrder(Intel intel)
    //            : this(intel, intel.Fleet) { }

    //        public RedeployIntelOrder(Intel intel, Fleet targetFleet)
    //            : base(intel.Owner)
    //        {
    //            if (intel == null)
    //                throw new ArgumentNullException("intel");
    //            if (targetFleet == null)
    //                throw new ArgumentNullException("targetFleet");
    //            _intelId = intel.ObjectID;
    //            _targetFleetId = targetFleet.ObjectID;
    //        }

    //        public override bool Overrides(Order o)
    //        {
    //            var otherOrder = o as RedeployIntelOrder;

    //            if (otherOrder == null)
    //                return false;

    //            return ((otherOrder._intelId == this._intelId)
    //                    && (otherOrder._targetFleetId == this._targetFleetId));
    //        }
    //        public override bool DoExecute()
    //    {
    //        var intel = GameContext.Current.Universe.Objects[_intelId] as Intel;
    //        var targetFleet = GameContext.Current.Universe.Objects[_targetFleetId] as Fleet;

    //        if ((intel == null) || (targetFleet == null))
    //            return false;

    //        intel.Fleet = targetFleet;
    //        return true;
    //    }
    //}

    //[Serializable]
    //    public sealed class RedeployIntelOrder : Order
    //    {
    //        private readonly int _intelId;
    //        private readonly int _targetFleetId;

    //        public RedeployIntelOrder(Civilization owner, Intel intel)
    //            : this(owner, intel, intel.Fleet) { }

    //        public RedeployIntelOrder(Civilization owner, Intel intel, Fleet targetFleet)
    //            : base(owner)
    //        {
    //            if (intel == null)
    //                throw new ArgumentNullException("intel");
    //            if (targetFleet == null)
    //                throw new ArgumentNullException("targetFleet");
    //            _intelId = intel.ObjectID;
    //            _targetFleetId = targetFleet.ObjectID;
    //        }

    //        public RedeployIntelOrder(Intel intel)
    //            : this(intel, intel.Fleet) { }

    //        public RedeployIntelOrder(Intel intel, Fleet targetFleet)
    //            : base(intel.Owner)
    //        {
    //            if (intel == null)
    //                throw new ArgumentNullException("intel");
    //            if (targetFleet == null)
    //                throw new ArgumentNullException("targetFleet");
    //            _intelId = intel.ObjectID;
    //            _targetFleetId = targetFleet.ObjectID;
    //        }

    //        public override bool Overrides(Order o)
    //        {
    //            var otherOrder = o as RedeployIntelOrder;

    //            if (otherOrder == null)
    //                return false;

    //            return ((otherOrder._intelId == this._intelId)
    //                    && (otherOrder._targetFleetId == this._targetFleetId));
    //        }
    //        public override bool DoExecute()
    //    {
    //        var intel = GameContext.Current.Universe.Objects[_intelId] as Intel;
    //        var targetFleet = GameContext.Current.Universe.Objects[_targetFleetId] as Fleet;

    //        if ((intel == null) || (targetFleet == null))
    //            return false;

    //        intel.Fleet = targetFleet;
    //        return true;
    //    }
    //}

    [Serializable]
    public sealed class SetColonyProductionOrder : Order
    {
        private int _colonyId;
        private int[] _activeFacilities;

        public SetColonyProductionOrder(Colony colony)
            : base(colony.Owner)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            Initialize(colony);
        }

        public SetColonyProductionOrder(Civilization owner, Colony colony)
            : base(owner)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            Initialize(colony);
        }

        private void Initialize(Colony colony)
        {
            _colonyId = colony.ObjectID;

            var productionCategories = EnumUtilities.GetValues<ProductionCategory>();

            _activeFacilities = new int[productionCategories.Count];

            foreach (var category in productionCategories)
                _activeFacilities[(int)category] = colony.GetActiveFacilities(category);
        }

        public override bool DoExecute()
        {
            var colony = GameContext.Current.Universe.Objects[_colonyId] as Colony;

            if (colony == null)
                return false;

            foreach (ProductionCategory category in EnumUtilities.GetValues<ProductionCategory>())
            {
                while (colony.DeactivateFacility(category))
                    continue;
            }

            foreach (ProductionCategory category in EnumUtilities.GetValues<ProductionCategory>())
            {
                var facilitiesToActivate = _activeFacilities[(int)category];
                while (facilitiesToActivate-- > 0)
                    colony.ActivateFacility(category);
            }

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetColonyProductionOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._colonyId == _colonyId);
        }
    }

    [Serializable]
    public sealed class SetPersonnelDistributionOrder : Order
    {
        private readonly DistributionGroup<PersonnelCategory> _values;

        public SetPersonnelDistributionOrder(Civilization owner, DistributionGroup<PersonnelCategory> values)
            : base(owner)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            _values = values;
        }

        public override bool DoExecute()
        {
            var civManager = ExecutionContext.CivilizationManagers[OwnerID];

            if (civManager == null)
                return false;

            foreach (PersonnelCategory category in EnumUtilities.GetValues<PersonnelCategory>())
                civManager.Personnel.Distribution[category].IsLocked = false;

            foreach (PersonnelCategory category in EnumUtilities.GetValues<PersonnelCategory>())
            {
                civManager.Personnel.Distribution[category].Value = _values[category].Value;
                civManager.Personnel.Distribution[category].IsLocked = true;
            }

            foreach (PersonnelCategory category in EnumUtilities.GetValues<PersonnelCategory>())
                civManager.Personnel.Distribution[category].IsLocked = _values[category].IsLocked;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetPersonnelDistributionOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder.OwnerID == OwnerID);
        }
    }

    [Serializable]
    public sealed class UpdateProductionOrder : Order
    {
        private int _sourceId;
        private BuildSlot[] _slots;
        private IList<BuildQueueItem> _buildQueue;

        public UpdateProductionOrder(Civilization owner, IProductionCenter source)
            : base(owner)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Initialize(source);
        }

        public UpdateProductionOrder(IProductionCenter source)
            : base(source.Owner)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Initialize(source);
        }

        private void Initialize(IProductionCenter source)
        {
            _sourceId = source.ObjectID;
            _slots = source.BuildSlots.ToArray();
            _buildQueue = source.BuildQueue;
        }

        public override bool DoExecute()
        {
            var source = GameContext.Current.Universe.Objects[_sourceId] as IProductionCenter;

            if (source == null)
                return false;

            for (var i = 0; i < _slots.Length && i < source.BuildSlots.Count; i++)
            {
                source.BuildSlots[i].Project = _slots[i].Project;
                source.BuildSlots[i].Priority = _slots[i].Priority;
            }

            source.BuildQueue.Clear();
            source.BuildQueue.AddRange(_buildQueue);

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as UpdateProductionOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._sourceId == _sourceId);
        }
    }

    [Serializable]
    public sealed class RushProductionOrder : Order
    {
        private int _sourceId;

        public RushProductionOrder(Civilization owner, IProductionCenter source)
            : base(owner)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Initialize(source);
        }

        public RushProductionOrder(IProductionCenter source)
            : base(source.Owner)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Initialize(source);
        }

        private void Initialize(IProductionCenter source)
        {
            _sourceId = source.ObjectID;
        }

        public override bool DoExecute()
        {
            var source = GameContext.Current.Universe.Objects[_sourceId] as IProductionCenter;
            if (source == null)
                return false;

            if ((source.BuildSlots.Count > 0) && (source.BuildSlots[0].Project != null))
                source.BuildSlots[0].Project.IsRushed = true;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as RushProductionOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._sourceId == _sourceId);
        }
    }

    [Serializable]
    public sealed class UpdateBuildingOrder : Order
    {
        private int _buildingId;
        private bool _scrap;
        private bool _isActive;

        public UpdateBuildingOrder(Building building)
            : base(building.Owner)
        {
            if (building == null)
                throw new ArgumentNullException("building");
            Initialize(building);
        }

        private void Initialize(Building building)
        {
            _buildingId = building.ObjectID;
            _scrap = building.Scrap;
            _isActive = building.IsActive;
        }

        public override bool DoExecute()
        {
            var building = GameContext.Current.Universe.Objects[_buildingId] as Building;

            if (building == null)
                return false;

            building.Scrap = _scrap;

            if (_isActive != building.IsActive)
            {
                if (_isActive)
                {
                    building.Sector.System.Colony.ActivateBuilding(building);
                }
                else
                {
                    building.Sector.System.Colony.DeactivateBuilding(building);
                }
            }

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as UpdateBuildingOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._buildingId == _buildingId);
        }
    }

    [Serializable]
    public sealed class UpdateOrbitalBatteriesOrder : Order
    {
        private readonly int _colonyId;
        private readonly int _activeCount;

        public UpdateOrbitalBatteriesOrder(Colony colony)
            : base(colony.Owner)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            _colonyId = colony.ObjectID;
            _activeCount = colony.ActiveOrbitalBatteries;
        }

        public override bool DoExecute()
        {
            var colony = GameContext.Current.Universe.Objects[_colonyId] as Colony;
            if (colony == null)
                return false;

            var activeCountDifference = _activeCount - colony.ActiveOrbitalBatteries;

            while (activeCountDifference != 0)
            {
                if (activeCountDifference > 0)
                {
                    if (colony.ActivateOrbitalBattery())
                        --activeCountDifference;
                    else
                        break;
                }
                else
                {
                    if (colony.DeactivateOrbitalBattery())
                        ++activeCountDifference;
                    else
                        break;
                }
            }

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as UpdateOrbitalBatteriesOrder;
            if (otherOrder == null)
                return false;

            return (otherOrder._colonyId == _colonyId);
        }
    }

    [Serializable]
    public sealed class ToggleShipyardBuildSlotOrder : Order
    {
        private readonly int _shipyardId;
        private readonly int _slotId;
        private readonly bool _isActive;

        public ToggleShipyardBuildSlotOrder(ShipyardBuildSlot buildSlot)
            : base(Guard.ArgumentNotNull(buildSlot, "buildSlot").Shipyard.Owner)
        {
            _shipyardId = buildSlot.Shipyard.ObjectID;
            _slotId = buildSlot.SlotID;
            _isActive = buildSlot.IsActive;
        }

        public override bool DoExecute()
        {
            var shipyard = GameContext.Current.Universe.Objects[_shipyardId] as Shipyard;
            if (shipyard == null)
                return false;

            if (_slotId >= shipyard.BuildSlots.Count)
                return false;

            var buildSlot = shipyard.BuildSlots[_slotId];

            if (_isActive)
                return shipyard.Sector.System.Colony.ActivateShipyardBuildSlot(buildSlot);

            return shipyard.Sector.System.Colony.DeactivateShipyardBuildSlot(buildSlot);
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as ToggleShipyardBuildSlotOrder;
            if (otherOrder == null)
                return false;

            return otherOrder._shipyardId == _shipyardId &&
                   otherOrder._slotId == _slotId;
        }
    }

    [Serializable]
    public sealed class SetShipyardBuildSlotProjectOrder : Order
    {
        private readonly int _shipyardId;
        private readonly int _slotId;
        private readonly BuildProject _project;

        public SetShipyardBuildSlotProjectOrder(ShipyardBuildSlot buildSlot)
            : base(Guard.ArgumentNotNull(buildSlot, "buildSlot").Shipyard.Owner)
        {
            _shipyardId = buildSlot.Shipyard.ObjectID;
            _slotId = buildSlot.SlotID;
            _project = buildSlot.Project;
        }

        public override bool DoExecute()
        {
            var shipyard = GameContext.Current.Universe.Objects[_shipyardId] as Shipyard;
            if (shipyard == null)
                return false;

            if (_slotId >= shipyard.BuildSlots.Count)
                return false;

            var buildSlot = shipyard.BuildSlots[_slotId];

            if (buildSlot.HasProject)
            {
                if (buildSlot.Project == _project)
                    return true;

                if (!buildSlot.Project.IsCancelled)
                    buildSlot.Project.Cancel();

                buildSlot.Project = null;
            }

            buildSlot.Project = _project;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetShipyardBuildSlotProjectOrder;
            if (otherOrder == null)
                return false;

            return otherOrder._shipyardId == _shipyardId &&
                   otherOrder._slotId == _slotId;
        }
    }

    [Serializable]
    public sealed class SetFleetRouteOrder : Order
    {
        private readonly int _fleetId;
        private readonly TravelRoute _route;

        public SetFleetRouteOrder(Civilization owner, Fleet fleet)
            : base(owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _route = fleet.Route;
        }

        public SetFleetRouteOrder(Fleet fleet)
            : base(fleet.Owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _route = fleet.Route;
        }

        public override bool DoExecute()
        {
            var fleet = GameContext.Current.Universe.Objects[_fleetId] as Fleet;

            if (fleet == null)
                return false;

            fleet.SetRoute(_route);

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetFleetRouteOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._fleetId == _fleetId);
        }
    }

    [Serializable]
    public sealed class SetFleetOrderOrder : Order
    {
        private readonly int _fleetId;
        private readonly FleetOrder _order;

        public SetFleetOrderOrder(Civilization owner, Fleet fleet)
            : base(owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _order = fleet.Order;
        }

        public SetFleetOrderOrder(Fleet fleet)
            : base(fleet.Owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _order = fleet.Order;
        }

        public override bool DoExecute()
        {
            var fleet = GameContext.Current.Universe.Objects[_fleetId] as Fleet;

            if (fleet == null)
                return false;

            fleet.SetOrder(_order);
            _order.UpdateReferences();

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetFleetOrderOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._fleetId == _fleetId);
        }
    }

    [Serializable]
    public sealed class SetTradeRouteOrder : Order
    {
        private readonly int _colonyId;
        private readonly int _routeId;
        private readonly int _targetId;

        public SetTradeRouteOrder(TradeRoute route)
            : base(route.SourceColony.Owner)
        {
            if (route == null)
                throw new ArgumentNullException("route");
            if (route.SourceColony == null)
                throw new ArgumentException("TradeRoute cannot have null SourceColony");
            _colonyId = route.SourceColony.ObjectID;
            _routeId = route.SourceColony.TradeRoutes.IndexOf(route);
            _targetId = route.IsAssigned ? route.TargetColony.ObjectID : GameObjectID.InvalidID;
        }

        public override bool DoExecute()
        {
            var colony = GameContext.Current.Universe.Objects[_colonyId] as Colony;

            if (colony == null)
                return false;
            if (_routeId >= colony.TradeRoutes.Count)
                return false;

            var route = colony.TradeRoutes[_routeId];

            if (_targetId == GameObjectID.InvalidID)
                route.TargetColony = null;
            else
                route.TargetColony = GameContext.Current.Universe.Objects[_targetId] as Colony;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as SetTradeRouteOrder;

            if (otherOrder == null)
                return false;

            return otherOrder._colonyId == _colonyId &&
                   otherOrder._routeId == _routeId;
        }
    }

    [Serializable]
    public sealed class CloakFleetOrder : Order
    {
        private readonly int _fleetId;
        private readonly bool _cloaked;
        //private readonly bool _camouflaged;

        public CloakFleetOrder(Fleet fleet)
            : base(fleet.Owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _cloaked = fleet.IsCloaked;
        }

        public override bool DoExecute()
        {
            var fleet = GameContext.Current.Universe.Objects[_fleetId] as Fleet;

            if (fleet == null)
                return false;

            fleet.IsCloaked = _cloaked;
            //fleet.IsCamouflaged = _camouflaged;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as CloakFleetOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._fleetId == _fleetId);
        }
    }
    [Serializable]
    public sealed class CamouflageFleetOrder : Order
    {
        private readonly int _fleetId;
        private readonly bool _camouflaged;

        public CamouflageFleetOrder(Fleet fleet)
            : base(fleet.Owner)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            _fleetId = fleet.ObjectID;
            _camouflaged = fleet.IsCamouflaged;
        }

        public override bool DoExecute()
        {
            var fleet = GameContext.Current.Universe.Objects[_fleetId] as Fleet;

            if (fleet == null)
                return false;

            fleet.IsCamouflaged = _camouflaged;

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as CamouflageFleetOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._fleetId == _fleetId);
        }
    }
    [Serializable]
    public sealed class FacilityScrapOrder : Order
    {
        private readonly int _colonyId;
        private readonly int[] _scrappedFacilities;

        public FacilityScrapOrder(Colony colony)
            : base(colony.Owner)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            
            _colonyId = colony.ObjectID;
            
            var productionCategories = EnumUtilities.GetValues<ProductionCategory>();
            
            _scrappedFacilities = new int[productionCategories.Count];
            
            foreach (var category in productionCategories)
                _scrappedFacilities[(int)category] = colony.GetScrappedFacilities(category);
        }

        public override bool DoExecute()
        {
            var colony = GameContext.Current.Universe.Objects[_colonyId] as Colony;

            if (colony == null)
                return false;

            foreach (var category in EnumUtilities.GetValues<ProductionCategory>())
                colony.SetScrappedFacilities(category, _scrappedFacilities[(int)category]);

            return true;
        }

        public override bool Overrides(Order o)
        {
            var otherOrder = o as FacilityScrapOrder;

            if (otherOrder == null)
                return false;

            return (otherOrder._colonyId == _colonyId);
        }
    }

    [Serializable]
    public sealed class ScrapOrder : Order
    {
        private readonly bool _scrap;
        private readonly int _targetId;

        public TechObject Target
        {
            get { return GameContext.Current.Universe.Get<TechObject>(_targetId); }
        }

        public ScrapOrder(TechObject target)
            : this(true, target) {}

        public ScrapOrder(bool scrap, TechObject target)
            : base(target.Owner)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            _scrap = scrap;
            _targetId = target.ObjectID;
        }

        public override bool DoExecute()
        {
            var target = Target;
            if (target == null)
                return false;
            target.Scrap = _scrap;
            return true;
        }

        public override bool Overrides(Order otherOrder)
        {
            var otherScrapOrder = otherOrder as ScrapOrder;
            if (otherScrapOrder == null)
                return false;
            return (otherScrapOrder._targetId == _targetId);
        }
    }

    [Serializable]
    public sealed class SetObjectNameOrder : Order
    {
        private readonly int _targetId;
        private readonly string _name;

        public UniverseObject Target
        {
            get { return GameContext.Current.Universe.Get<UniverseObject>(_targetId); }
        }

        public SetObjectNameOrder([NotNull] UniverseObject target, string name)
            : base(target.Owner)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            _targetId = target.ObjectID;
            _name = name;
        }

        public override bool DoExecute()
        {
            var target = Target;
            if (target != null)
            {
                target.Name = _name;
                return true;
            }
            return false;
        }

        public override bool Overrides(Order otherOrder)
        {
            var order = otherOrder as SetObjectNameOrder;
            if (order == null)
                return false;
            if (order._targetId == _targetId)
                return true;
            return false;
        }
    }

    [Serializable]
    public sealed class UpdateResearchOrder : Order
    {
        private readonly Percentage[] _values;
        private readonly bool[] _locked;

        public UpdateResearchOrder(Civilization owner)
            : base(owner)
        {
            var civManager = GameContext.Current.CivilizationManagers[owner];
            if (civManager == null)
            {
                throw new InvalidOperationException(
                    "Cannot access CivilizationManager for owner");
            }
            _values = new Percentage[GameContext.Current.ResearchMatrix.Fields.Count];
            _locked = new bool[GameContext.Current.ResearchMatrix.Fields.Count];
            for (var i = 0; i < GameContext.Current.ResearchMatrix.Fields.Count; i++)
            {
                _locked[i] = civManager.Research.Distributions[i].IsLocked;
                _values[i] = civManager.Research.Distributions[i].Value;
            }
        }

        public override bool Overrides(Order o)
        {
            return (o is UpdateResearchOrder);
        }

        public override bool DoExecute()
        {
            var civManager = GameContext.Current.CivilizationManagers[Owner];
            if (civManager != null)
            {
                for (var i = 0; i < GameContext.Current.ResearchMatrix.Fields.Count; i++)
                {
                    civManager.Research.Distributions[i].IsLocked = _locked[i];
                    civManager.Research.Distributions[i].SetValueInternal(_values[i]);
                }
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public sealed class SendProposalOrder : Order
    {
        private readonly IProposal _proposal;

        public IProposal Proposal
        {
            get { return _proposal; }
        }

        public SendProposalOrder([NotNull] IProposal proposal)
            : base(proposal.Sender)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            _proposal = proposal;
        }

        public override bool DoExecute()
        {
            var diplomat = Diplomat.Get(OwnerID);
            if (diplomat == null)
                return false;

            var foreignPower = diplomat.GetForeignPower(_proposal.Recipient);
            if (foreignPower == null)
                return false;

            foreignPower.ProposalSent = _proposal;
            //foreignPower.CounterpartyForeignPower.ProposalReceived = _proposal;

            return true;
        }
    }

    [Serializable]
    public sealed class SendStatementOrder : Order
    {
        private readonly Statement _statement;

        public Statement Statement
        {
            get { return _statement; }
        }

        public SendStatementOrder([NotNull] Statement statement)
            : base(statement.Sender)
        {
            if (statement == null)
                throw new ArgumentNullException("statement");

            _statement = statement;
        }

        public override bool DoExecute()
        {
            var diplomat = Diplomat.Get(OwnerID);
            if (diplomat == null)
                return false;

            var foreignPower = diplomat.GetForeignPower(_statement.Recipient);
            if (foreignPower == null)
                return false;

            foreignPower.StatementSent = _statement;

            return true;
        }
    }

    [Serializable]
    public sealed class AssignDiplomaticEnvoyOrder : Order
    {
        private readonly GameObjectID _agentId;
        private readonly GameObjectID _counterpartyId;

        public Agent Agent
        {
            get
            {
                Agent agent;

                var civilizationManager = GameContext.Current.CivilizationManagers[OwnerID];
                if (civilizationManager != null &&
                    civilizationManager.AgentPool.CurrentAgents.TryGetValue(_agentId, out agent))
                {
                    return agent;
                }

                return null;
            }
        }

        public ForeignPower ForeignPower
        {
            get
            {
                var diplomat = Diplomat.Get(OwnerID);
                if (diplomat == null)
                    return null;

                return diplomat.GetForeignPower(GameContext.Current.Civilizations[_counterpartyId]);
            }
        }

        public AssignDiplomaticEnvoyOrder([NotNull] Agent agent, [NotNull] ForeignPower foreignPower)
            : base(Guard.ArgumentNotNull(agent, "agent").Owner)
        {
            if (foreignPower == null)
                throw new ArgumentNullException("foreignPower");

            _agentId = agent.ObjectID;
            _counterpartyId = foreignPower.CounterpartyID;
        }

        public override bool DoExecute()
        {
            var agent = Agent;
            if (agent == null)
                return false;

            var foreignPower = ForeignPower;
            if (foreignPower == null)
                return false;

            var mission = new DiplomaticEnvoyMission(
                Owner,
                ForeignPower.Counterparty,
                Agent.CurrentLocation ?? CivilizationManager.For(OwnerID).SeatOfGovernment.Location);

            return mission.Assign(agent);
        }
    }

    [Serializable]
    public sealed class CancelAgentMissionOrder : Order
    {
        private readonly GameObjectID _agentId;

        [NonSerialized] private MissionPhase _originalPhase;

        public Agent Agent
        {
            get
            {
                Agent agent;

                var civilizationManager = GameContext.Current.CivilizationManagers[OwnerID];
                if (civilizationManager != null &&
                    civilizationManager.AgentPool.CurrentAgents.TryGetValue(_agentId, out agent))
                {
                    return agent;
                }

                return null;
            }
        }

        public CancelAgentMissionOrder([NotNull] Agent agent)
            : base(Guard.ArgumentNotNull(agent, "agent").Owner)
        {
            _agentId = agent.ObjectID;
        }

        public override bool DoExecute()
        {
            var agent = Agent;
            if (agent == null)
                return false;

            var mission = agent.Mission;
            if (mission == null || mission is NullMission)
                return false;

            _originalPhase = mission.CurrentPhase;

            return mission.Cancel();
        }

        public override bool DoUndo()
        {
            if (_originalPhase == null || Agent == null || Agent.Mission == null || !Agent.Mission.TransitionToPhase(_originalPhase))
                return false;

            _originalPhase = null;

            return true;
        }
    }

    [Serializable]
    public class GiveCreditsOrder : Order
    {
        protected int _amount = 0;

        public GiveCreditsOrder(CivilizationManager owner, int amount)
            : base(owner.Civilization)
        {
            _amount = amount;
        }

        public override bool DoExecute()
        {
            var civManager = ExecutionContext.CivilizationManagers[OwnerID];
            if (civManager == null)
                return false;

            civManager.Credits.AdjustCurrent(_amount);

            return true;
        }
    }

    [Serializable]
    public class GiveResourceOrder : GiveCreditsOrder
    {
        private ResourceType _resourceType = ResourceType.Deuterium;

        public GiveResourceOrder(CivilizationManager owner, ResourceType resType, int amount)
            : base(owner, amount)
        {
            _resourceType = resType;
        }

        public override bool DoExecute()
        {
            var civManager = ExecutionContext.CivilizationManagers[OwnerID];
            if (civManager == null)
                return false;

            civManager.Resources[_resourceType].AdjustCurrent(_amount);
            return true;
        }
    }
}
