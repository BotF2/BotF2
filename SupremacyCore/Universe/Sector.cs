// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Data;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Supremacy.Universe
{
    /// <summary>
    /// Defines the cardinal map directions used to differentiate neighboring
    /// <see cref="Sector"/>s in a <see cref="SectorMap"/>.
    /// </summary>
    public enum MapDirection : byte
    {
        North = 0,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    /// <summary>
    /// Defines a galactic sector.
    /// </summary>
    [Serializable]
    public class Sector : IEquatable<Sector>, INotifyPropertyChanged
    {
        private MapLocation _location;

        [NonSerialized]
        private Lazy<StarSystem> _system;

        [NonSerialized]
        private Lazy<Station> _station;

        /// <summary>
        /// Gets the map location of this <see cref="Sector"/>.
        /// </summary>
        /// <value>The map location.</value>
        public MapLocation Location
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the system located in this <see cref="Sector"/>.
        /// </summary>
        /// <value>The system.</value>
        public StarSystem System
        {
            get { return _system.Value; }
            internal set
            {
                _system = new Lazy<StarSystem>(() => value);

                OnPropertyChanged("System");
                OnPropertyChanged("Name");
                OnPropertyChanged("Owner");
                OnPropertyChanged("IsOwned");
            }
        }

        /// <summary>
        /// Gets the station located in this <see cref="Sector"/>.
        /// </summary>
        /// <value>The station.</value>
        public Station Station
        {
            get { return _station.Value; }
            internal set
            {
                _station = new Lazy<Station>(() => value);

                OnPropertyChanged("Station");
                OnPropertyChanged("Owner");
                OnPropertyChanged("IsOwned");
            }
        }

        public int TradeRouteIndicator
        {
            get
            {
                if (System == null || System.Colony == null)
                    return 99;

                Table popReqTable = GameContext.Current.Tables.ResourceTables["TradeRoutePopReq"];
                Table popModTable = GameContext.Current.Tables.ResourceTables["TradeRoutePopMultipliers"];

                int popForTradeRoute;

                var civManager = GameContext.Current.CivilizationManagers[Owner.CivID];

                /*
                 * See what the minimum population level is for a new trade route for the
                 * current civilization.  If one is not specified, use the default.
                 */
                if (popReqTable[civManager.Civilization.Key] != null)
                    popForTradeRoute = Number.ParseInt32(popReqTable[civManager.Civilization.Key][0]);
                else
                    popForTradeRoute = Number.ParseInt32(popReqTable[0][0]);

                int possibleTradeRoutes = System.Colony.Population.CurrentValue / popForTradeRoute;

                return possibleTradeRoutes;

            }
        }

        /// <summary>
        /// Gets the name of this <see cref="Sector"/>.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                var system = System;
                if (system != null)
                    return system.Name;

                return string.Format("({0}, {1})", _location.X, _location.Y);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Sector"/> is owned.
        /// </summary>
        /// <value><c>true</c> if this <see cref="Sector"/> is owned; otherwise, <c>false</c>.</value>
        public bool IsOwned
        {
            get { return (Owner != null); }
        }

        /// <summary>
        /// Gets the owner of this <see cref="Sector"/>.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner
        {
            get { return this.GetOwner(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sector"/> class.
        /// </summary>
        /// <param name="location">The map location.</param>
        public Sector(MapLocation location)
        {
            _location = location;
            _system = new Lazy<StarSystem>(FindSystem, LazyThreadSafetyMode.PublicationOnly);
            _station = new Lazy<Station>(FindStation, LazyThreadSafetyMode.PublicationOnly);
        }

        private StarSystem FindSystem()
        {
            return GameContext.Current.Universe.FindFirst<StarSystem>(o => o.Location == _location);
        }

        private Station FindStation()
        {
            return GameContext.Current.Universe.FindFirst<Station>(o => o.Location == _location);
        }

        public void Reset()
        {
            _system = new Lazy<StarSystem>(FindSystem, LazyThreadSafetyMode.PublicationOnly);
            _station = new Lazy<Station>(FindStation, LazyThreadSafetyMode.PublicationOnly);

            OnPropertyChanged("System");
            OnPropertyChanged("Station");
            OnPropertyChanged("Name");
            OnPropertyChanged("Owner");
            OnPropertyChanged("IsOwned");
        }

        /// <summary>
        /// Gets the <see cref="Sector"/>s neighboring this <see cref="Sector"/>.
        /// </summary>
        /// <returns>The neighboring <see cref="Sector"/>s.</returns>
        public IIndexedEnumerable<Sector> GetNeighbors()
        {
            var mapDirections = EnumHelper.GetValues<MapDirection>();
            var neighbors = new Sector[mapDirections.Length];
            
            var count = 0;

            mapDirections
                .Select(GetNeighbor)
                .Where(neighbor => neighbor != null)
                .ForEach(
                    (sector, i) =>
                    {
                        neighbors[i] = sector;
                        ++count;
                    });

            return new ArrayWrapper<Sector>(neighbors, 0, count);
        }

        /// <summary>
        /// Gets the adjacent <see cref="Sector"/> for a given <see cref="MapDirection"/>.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>The adjacent <see cref="Sector"/>.</returns>
        public Sector GetNeighbor(MapDirection direction)
        {
            int dx, dy;

            switch (direction)
            {
                case MapDirection.NorthEast:
                    dx = 1;
                    dy = -1;
                    break;
                case MapDirection.East:
                    dx = 1;
                    dy = 0;
                    break;
                case MapDirection.SouthEast:
                    dx = 1;
                    dy = 1;
                    break;
                case MapDirection.South:
                    dx = 0;
                    dy = 1;
                    break;
                case MapDirection.SouthWest:
                    dx = -1;
                    dy = 1;
                    break;
                case MapDirection.West:
                    dx = -1;
                    dy = 0;
                    break;
                case MapDirection.NorthWest:
                    dx = -1;
                    dy = -1;
                    break;
                default:
                    dx = 0;
                    dy = -1;
                    break;
            }

            if (_location.X + dx < 0 ||
                _location.X + dx > GameContext.Current.Universe.Map.Width ||
                _location.Y + dy < 0 ||
                _location.Y + dy > GameContext.Current.Universe.Map.Height)
            {
                return null;
            }

            var location = new MapLocation(
                _location.X + dx,
                _location.Y + dy);

            return GameContext.Current.Universe.Map[location];
        }

        /// <summary>
        /// Gets the location of the <see cref="Sector"/> with the specified ID.
        /// </summary>
        /// <param name="sectorId">The ID of the <see cref="Sector"/>.</param>
        /// <returns>The location of the <see cref="Sector"/>.</returns>
        public static MapLocation SectorToLocation(int sectorId)
        {
            return new MapLocation(sectorId >> 8, sectorId & 0xFF);
        }

        /// <summary>
        /// Equalses the specified sector.
        /// </summary>
        /// <param name="sector">The sector.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="sector"/> is equal to this <see cref="Sector"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        
        public virtual bool Equals(Sector sector)
        {
            if (sector is null)
                return false;

            return (sector._location == _location); //&& sector._station == _station && sector._system == _system);
        }

        /// <summary>
        /// Returns a <see cref="Sector"/> that represents the current <see cref="Sector"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Sector"/> that represents the current <see cref="Sector"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Sector a, Sector b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return (a._location == b._location);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Sector a, Sector b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if ((a is null) || (b is null))
                return true;
            return (a._location != b._location);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="Sector"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current<see cref="Sector"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="Sector"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as Sector);
        }

    
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Sector"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override int GetHashCode()
        {
            return _location.GetHashCode();
        }

        #region Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public static class SectorExtensions
    {
        public static Civilization GetOwner([NotNull] this Sector sector)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");

            var system = sector.System;
            if (system != null)
            {
                var colony = system.Colony;
                if (colony != null)
                    return colony.Owner;
            }

            var station = sector.Station;
            if (station != null)
                return station.Owner;

            return null;
        }

        public static IEnumerable<Fleet> GetFleets([NotNull] this Sector sector)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");

            return GameContext.Current.Universe.FindAt<Fleet>(sector.Location);
        }

        public static IEnumerable<Fleet> GetOwnedFleets([NotNull] this Sector sector, [NotNull] Civilization civ)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            if (civ == null)
                throw new ArgumentNullException("civ");

            return GameContext.Current.Universe.FindAt<Fleet>(sector.Location)
                .Where(f => f.Owner == civ);
        }
    }
}
