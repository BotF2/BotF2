using System.Collections.Generic;
// UniverseManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;

using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Data;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Universe
{
    /// <summary>
    /// Manages all of the objects in a game universe and the map of the universe.
    /// </summary>
    /// <remarks>
    /// The 'Find...' methods in this class will only evaluate objects of the specified design parameter.
    /// </remarks>
    [Serializable]
    public class UniverseManager : IOwnedDataSerializableAndRecreatable
    {
        #region Static Members
        private static readonly TableMap s_tables;

        /// <summary>
        /// Gets the tables in the UniverseTables set.
        /// </summary>
        /// <value>The tables.</value>
        public static TableMap Tables
        {
            get { return s_tables; }
        }

        /// <summary>
        /// Initializes the <see cref="UniverseManager"/> class.
        /// </summary>
        static UniverseManager()
        {
            s_tables = TableMap.ReadFromFile(
                ResourceManager.GetResourcePath("Resources/Tables/UniverseTables.txt"));
        }
        #endregion

        private SectorMap _map;
        private UniverseObjectSet _objects;
        private GameObjectLookupCollection<Civilization, Colony> _homeColonyLookup;

        /// <summary>
        /// Gets the map of the game universe.
        /// </summary>
        /// <value>The map.</value>
        public SectorMap Map
        {
            get { return _map; }
        }

        /// <summary>
        /// Gets or sets the objects in the game universe.
        /// </summary>
        /// <value>The objects.</value>
        public UniverseObjectSet Objects
        {
            get { return _objects; }
            internal set { _objects = value; }
        }

        public GameObjectLookupCollection<Civilization, Colony> HomeColonyLookup
        {
            get { return _homeColonyLookup; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniverseManager"/> class.
        /// </summary>
        /// <param name="mapSize">Size of the map.</param>
        public UniverseManager(Dimension mapSize)
        {
            _map = new SectorMap(mapSize.Width, mapSize.Height);
            _objects = new UniverseObjectSet();
            _homeColonyLookup = new GameObjectLookupCollection<Civilization, Colony>(
                civilization => civilization.CivID,
                colony => colony.OriginalOwner,
                colony => colony.ObjectID,
                id => _objects[id] as Colony);
            //GameLog.Client.GameData.DebugFormat("Map: {0} to {1}",_map.Width, _map.Height);
        }

        /// <summary>
        /// Gets the object with the specified id.
        /// </summary>
        /// <typeparam name="T">The design of object.</typeparam>
        /// <param name="objectId">The object id.</param>
        /// <returns>The object with the specified id.</returns>
        public T Get<T>(int objectId) where T : UniverseObject
        {
            return _objects[objectId] as T;
        }

        /// <summary>
        /// Finds all objects of the specified design for which the given
        /// <see cref="Predicate&lt;T&gt;"/> evaluates <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The objects.</returns>
        public HashSet<T> Find<T>(Func<T, bool> predicate)
            where T : UniverseObject
        {
            return _objects.OfType<T>().Where(predicate).ToHashSet();
        }

        public HashSet<UniverseObject> Find(UniverseObjectType objectType)
        {
            return _objects.Where(o => o.ObjectType == objectType).ToHashSet();
        }

        public HashSet<T> Find<T>(UniverseObjectType objectType)
            where T : UniverseObject
        {
            return _objects.Where(o => o.ObjectType == objectType).OfType<T>().ToHashSet();
        }

        public HashSet<T> Find<T>(UniverseObjectType objectType, Expression<Func<T, bool>> predicate)
            where T : UniverseObject
        {
            var itemParameter = Expression.Parameter(typeof(UniverseObject), "o");
            return _objects.Where(
                Expression.Lambda<Func<UniverseObject, bool>>(
                    Expression.AndAlso(
                        Expression.AndAlso(
                            Expression.Equal(
                                Expression.Property(itemParameter, "ObjectType"),
                                Expression.Constant(objectType, typeof(UniverseObjectType))),
                            Expression.TypeIs(itemParameter, typeof(T))),
                        Expression.Invoke(predicate, Expression.TypeAs(itemParameter, typeof(T)))),
                    itemParameter))
                .Cast<T>()
                .ToHashSet();
        }

        /// <summary>
        /// Finds all objects of the specified design owned by the specified <see cref="Civilization"/>.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <param name="civilization">The owner.</param>
        /// <returns>The objects.</returns>
        public HashSet<T> FindOwned<T>(Civilization civilization)
            where T : UniverseObject
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");
            var items = from item in _objects
                        where (item.OwnerID == civilization.CivID)
                        select item;
            return items.OfType<T>().ToHashSet();
        }

        /// <summary>
        /// Finds all objects of the specified design owned by the specified <see cref="Civilization"/>.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <param name="civilizationId">The owner ID.</param>
        /// <returns>The objects.</returns>
        public HashSet<T> FindOwned<T>(int civilizationId)
            where T : UniverseObject
        {
            var items = from item in _objects
                        where (item.OwnerID == civilizationId)
                        select item;
            return items.OfType<T>().ToHashSet();
        }

        /// <summary>
        /// Finds all objects of the specified design at a specific location.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <param name="location">The location.</param>
        /// <returns>The objects.</returns>
        public HashSet<T> FindAt<T>(MapLocation location)
            where T : UniverseObject
        {
            var items = from item in _objects
                        where (item.Location == location)
                        select item;
            return items.OfType<T>().ToHashSet();
        }

        /// <summary>
        /// Finds all objects of the specified design at a specific location for which the
        /// given <see cref="Predicate&lt;T&gt;"/> evaluates <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <param name="location">The location.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The objects.</returns>
        public HashSet<T> FindAt<T>(MapLocation location, Expression<Func<T, bool>> predicate)
            where T : UniverseObject
        {
            var itemParameter = Expression.Parameter(typeof(UniverseObject), "o");
            return _objects
                .AsQueryable()
                .Where(
                    Expression.Lambda<Func<UniverseObject, bool>>(
                        Expression.AndAlso(
                            Expression.AndAlso(
                                Expression.Equal(
                                    Expression.Property(itemParameter, "Location"),
                                    Expression.Constant(location, typeof(MapLocation))),
                                Expression.TypeIs(itemParameter, typeof(T))),
                            Expression.Invoke(predicate, Expression.TypeAs(itemParameter, typeof(T)))),
                        itemParameter))
                    .Cast<T>()
                    .ToHashSet();
        }

        /// <summary>
        /// Finds all objects of the specified design.
        /// </summary>
        /// <typeparam name="T">The design of objects to return.</typeparam>
        /// <returns>The objects.</returns>
        public HashSet<T> Find<T>()
            where T : UniverseObject
        {
            return _objects.OfType<T>().ToHashSet();
        }

        /// <summary>
        /// Finds the first object of the specified design for which the given
        /// <see cref="Predicate&lt;T&gt;"/> evaluates <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The design of object to return.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The object.</returns>
        public T FindFirst<T>(Func<T, bool> predicate)
            where T : UniverseObject
        {
            return _objects.OfType<T>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Finds the object of the specified design nearest to the specified location.
        /// </summary>
        /// <typeparam name="T">The design of object to return.</typeparam>
        /// <param name="source">The source location.</param>
        /// <returns>The object.</returns>
        public T FindNearest<T>(MapLocation source)
            where T : UniverseObject
        {
            return FindNearest<T>(source, true);
        }

        /// <summary>
        /// Finds the object of the specified design nearest to the specified location.
        /// </summary>
        /// <typeparam name="T">The design of object to return.</typeparam>
        /// <param name="source">The source location.</param>
        /// <param name="includeSource">Whether or not objects in the source location should be considered.</param>
        /// <returns>The object.</returns>
        public T FindNearest<T>(MapLocation source, bool includeSource)
            where T : UniverseObject
        {
            return FindNearest(
                source,
                (T o) => true,
                includeSource);
        }

        public T FindNearestOwned<T>(MapLocation source, Civilization owner, bool includeSource)
            where T : UniverseObject
        {
            return FindNearestOwned(source, owner, (Expression<Func<T, bool>>)(o => true), includeSource);
        }

        public T FindNearestOwned<T>(MapLocation source, Civilization owner)
            where T : UniverseObject
        {
            return FindNearestOwned(source, owner, (Expression<Func<T, bool>>)(o => true), true);
        }

        public T FindNearestOwned<T>(MapLocation source, Civilization owner, Expression<Func<T, bool>> predicate, bool includeSource = true)
            where T : UniverseObject
        {
            var ownerId = (owner != null) ? owner.CivID : Civilization.InvalidID;
            return _objects
                .Where(o => o.OwnerID == ownerId)
                .OfType<T>()
                .Where(o => includeSource || o.Location != source)
                .MinElement(o => MapLocation.GetDistance(source, o.Location));
        }

        /// <summary>
        /// Finds the first object of the specified design nearest to the specified location
        /// for which the given <see cref="Predicate&lt;T&gt;"/> evaluates <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The design of object to return.</typeparam>
        /// <param name="source">The source location.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="includeSource">Whether or not objects in the source location should be considered.</param>
        /// <returns>The object.</returns>
        public T FindNearest<T>(MapLocation source, Func<T, bool> predicate, bool includeSource = true)
            where T : UniverseObject
        {
            return (from o in _objects
                    where includeSource || o.Location != source
                    select o)
                   .OfType<T>()
                   .Where(predicate)
                   .MinElement(o => MapLocation.GetDistance(source, o.Location));
        }

        /// <summary>
        /// Gets a list of object IDs for all objects of the specified design.
        /// </summary>
        /// <typeparam name="T">The design of object.</typeparam>
        /// <returns>The ovbject IDs.</returns>
        public HashSet<GameObjectID> FindObjectIDs<T>()
            where T : UniverseObject
        {
            return _objects.OfType<T>().Select(o => o.ObjectID).ToHashSet();
        }

        /// <summary>
        /// Scraps all of the facilities at a <see cref="Colony"/> that have been marked for scrapping.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool ScrapNonStructures(Colony colony)
        {
            if (colony == null)
                return false;

            var civManager = GameContext.Current.CivilizationManagers[colony.OwnerID];
            if (civManager == null)
                return false;

            foreach (ProductionCategory pc in EnumUtilities.GetValues<ProductionCategory>())
            {
                int numFacilities = colony.GetScrappedFacilities(pc);
                if (numFacilities == 0)
                    continue;
                ProductionFacilityDesign design = colony.GetFacilityType(pc);
                if (design == null)
                    continue;
                int credits;
                ResourceValueCollection resources;
                double modifier = 1.0 + colony.ScrapBonus;
                design.GetScrapReturn(out credits, out resources);
                credits = Math.Min(design.BuildCost, (int)Math.Floor(modifier * credits));
                foreach (ResourceType resource in EnumUtilities.GetValues<ResourceType>())
                {
                    resources[resource] = Math.Min(
                        design.BuildResourceCosts[resource],
                        (int)Math.Floor(modifier * resources[resource]));
                        civManager.Resources[resource].AdjustCurrent(resources[resource]);
                }
                civManager.Credits.AdjustCurrent(credits);
            }

            colony.ScrapNonStructures();

            return true;
        }

        /// <summary>
        /// Scraps the specified object.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Scrap(TechObject target)
        {
            if (target == null)
                return false;

            if (!target.IsOwned)
                return Destroy(target);

            var civManager = GameContext.Current.CivilizationManagers[target.OwnerID];
            if (civManager == null)
                return Destroy(target);

            int credits;
            ResourceValueCollection resources;

            target.Design.GetScrapReturn(out credits, out resources);

            var targetSystem = target.Sector.System;
            
            if (targetSystem != null &&
                targetSystem.HasColony)
            {
                var baseReclaim = (double)credits / target.Design.BuildCost;
                
                var totalReclaim = (
                                       from bonus in civManager.GlobalBonuses
                                       where bonus.BonusType == BonusType.PercentScrapping
                                       select bonus
                                   )
                    .Aggregate(baseReclaim, (b, o) => baseReclaim + (0.01 * o.Amount));

                totalReclaim = Math.Min(totalReclaim, 1.0);

                credits = Math.Min(target.Design.BuildCost, (int)Math.Floor(totalReclaim * credits));
                
                foreach (var resource in EnumHelper.GetValues<ResourceType>())
                {
                    //if (resource == ResourceType.Personnel)
                    //    continue;
                    resources[resource] = Math.Min(
                        target.Design.BuildResourceCosts[resource],
                        (int)Math.Floor(totalReclaim * resources[resource]));
                }
                
                civManager.Credits.AdjustCurrent(credits);
            }

            if (Destroy(target))
            {
                foreach (ResourceType resource in EnumUtilities.GetValues<ResourceType>())
                {
                    //if (resource == ResourceType.Personnel)
                    //    civManager.Personnel[PersonnelCategory.Officers].AdjustCurrent(resources[resource]);
                    //else
                    civManager.Resources[resource].AdjustCurrent(resources[resource]);
                }
            
                return true;
            }

            return false;
        }

        /// <summary>
        /// Destroys the specified object.
        /// </summary>
        /// <param name="objectId">The ID of the object.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Destroy(int objectId)
        {
            return Destroy(_objects[objectId]);
        }

        /// <summary>
        /// Destroys the specified object.
        /// </summary>
        /// <param name="item">The object.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Destroy(UniverseObject item)
        {
            if (item == null)
                return false;

            if (item is Ship)
            {
                var ship = item as Ship;
                var fleet = ship.Fleet;

                fleet.RemoveShip(ship);

                if (fleet.Ships.Count == 0)
                {
                    Destroy(fleet);
                }
            }

            else if (item is Fleet)
            {
                var fleet = item as Fleet;
                if (fleet.Ships.Count > 0)
                {
                    // I'll admit, this is a stupid way of doing this, but I've done it this way
                    // to avoid a possible bug due to the ship destroyer calling destroy fleet when
                    // a fleet has no ships left in it, which would result in two calls to the fleet
                    // destroyer
                    //
                    // Instead, spin each ship out to it's own fleet, and let the ship destroyer take care of it
                    for (int i = 0; i < fleet.Ships.Count; i++) {
                        var newFleet = fleet.Ships[i].CreateFleet();
                        Destroy(newFleet.Ships[0]);
                    }
                }
            }

            else if (item is StarSystem)
            { 
                var system = item as StarSystem;
                if (system.IsInhabited)
                {
                    Destroy(system.Colony);
                }
            }

            else if (item is Colony)
            {
                var colony = item as Colony;
                var colonyOwner = colony.Owner;
                if (colonyOwner != null)
                {
                    var civManager = GameContext.Current.CivilizationManagers[colonyOwner];
                    if (civManager != null)
                    {
                        if (civManager.HomeColony == colony)
                        {
                            civManager.HomeColony = null;
                        }
                        civManager.Colonies.Remove(colony);
                    }

                    var ownerHomeColony = _homeColonyLookup[colonyOwner];
                    if (ownerHomeColony == colony)
                    {
                        _homeColonyLookup.Remove(ownerHomeColony);
                    }

                    colony.System.Owner = null;
                }

                colony.Population.Reset(0);
                colony.Morale.Reset(0);

                List<Building> tmpBuildings = new List<Building>(colony.Buildings.Count);
                tmpBuildings.AddRange(colony.Buildings.ToList());
                tmpBuildings.ForEach(o => Destroy(o));

                List<OrbitalBattery> tmpOBs = new List<OrbitalBattery>(colony.OrbitalBatteries.Count);
                tmpOBs.AddRange(colony.OrbitalBatteries.ToList());
                tmpOBs.ForEach(o => Destroy(o));

                colony.BuildQueue.Clear();
                colony.BuildSlots.ForEach(o => o.Project = null);
                
                colony.TradeRoutes.ForEach(o => o.TargetColony = null);

                if (colony.Shipyard != null)
                {
                    Destroy(colony.Shipyard);
                }
            }

            else if (item is Building)
            {
                var building = item as Building;
                building.Sector.System.Colony.RemoveBuilding(building);
            }

            else if (item is Shipyard)
            {
                var shipyard = item as Shipyard;
                if (Equals(shipyard.Sector.System.Colony.Shipyard, shipyard))
                {
                    shipyard.Sector.System.Colony.Shipyard = null;
                }
            }

            else if (item is Station)
            {
                var station = item as Station;
                station.Sector.Station = null;
            }

            else if (item is OrbitalBattery)
            {
                var orbitalBattery = item as OrbitalBattery;
                orbitalBattery.Sector.System.Colony.OnOrbitalBatteryDestroyed(orbitalBattery);
            }

            _objects.Remove(item);
            item.ObjectID = GameObjectID.InvalidID;
            return true;
        }

        /// <summary>
        /// Updates references lost during reserialization.
        /// </summary>
        internal void OnDeserialized()
        {
            UpdateSectors();
            
            foreach (var item in _objects)
            {
                item.OnDeserialized();

                var ship = item as Ship;
                if (ship == null)
                    continue;

                var fleet = ship.Fleet;
                if ((fleet != null) && !fleet.Ships.Contains(ship))
                    fleet.AddShipInternal(ship);
            }

            var colonies = _objects.OfType<Colony>();
            var systemLocationLookup = _objects.OfType<StarSystem>().ToLookup(o => o.Location);
            var buildingLocationLookup = _objects.OfType<Building>().ToLookup(o => o.Location);

            GameLog.Core.GalaxyGenerator.DebugFormat("Deserialized: item=Colony;Location;Owner;Name;Population");
            foreach (var colony in colonies)
            {
                var system = systemLocationLookup[colony.Location].FirstOrDefault();
                if (system == null)
                    continue;

                system.Colony = colony;
                colony.BuildingsInternal.Clear();
                GameLog.Core.GalaxyGenerator.DebugFormat("Deserialized: item=Colony;{0};{1};{2};{3};", colony.Location, colony.Owner, colony.Name, colony.Population);

                foreach (var building in buildingLocationLookup[colony.Location])
                    colony.BuildingsInternal.Add(building);
            }
        }

        /// <summary>
        /// Updates the sectors in the <see cref="Map"/>.
        /// </summary>
        internal void UpdateSectors()
        {
            _map.Reset();

            foreach (var station in Find<Station>())
                _map[station.Location].Station = station;

            foreach (var system in Find<StarSystem>())
                _map[system.Location].System = system;            
        }

    	public void SerializeOwnedData(SerializationWriter writer, object context)
    	{
            writer.Write((byte)_map.Width);
            writer.Write((byte)_map.Height);
            _objects.SerializeOwnedData(writer, context);
            _homeColonyLookup.SerializeOwnedData(writer, context);
    	}

    	public void DeserializeOwnedData(SerializationReader reader, object context)
    	{
            _objects = new UniverseObjectSet();
            _map = new SectorMap(reader.ReadByte(), reader.ReadByte());
            _homeColonyLookup = new GameObjectLookupCollection<Civilization, Colony>(
                civilization => civilization.CivID,
                colony => colony.OriginalOwner,
                colony => colony.ObjectID,
                id => _objects[id] as Colony);
    	    _objects.DeserializeOwnedData(reader, context);
            _homeColonyLookup.DeserializeOwnedData(reader, context);
    	}
    }
}
