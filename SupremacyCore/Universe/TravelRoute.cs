// TravelRoute.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.IO.Serialization;
using Supremacy.Orbitals;

namespace Supremacy.Universe
{
    /// <summary>
    /// Represents a travel plan for a <see cref="Fleet"/>.
    /// </summary>
    [Serializable]
    public sealed class TravelRoute : IOwnedDataSerializableAndRecreatable, ICloneable
    {
        /// <summary>
        /// An empty <see cref="TravelRoute"/>.
        /// </summary>
        public static readonly TravelRoute Empty = new TravelRoute(new MapLocation[0]);

        private List<MapLocation> _waypoints;
        private List<MapLocation> _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelRoute"/> class.
        /// </summary>
        public TravelRoute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelRoute"/> class.
        /// </summary>
        /// <param name="waypoints">The waypoints.</param>
        public TravelRoute(IEnumerable<MapLocation> waypoints) 
        {
            _path = new List<MapLocation>();
            _waypoints = new List<MapLocation>(waypoints);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelRoute"/> class.
        /// </summary>
        /// <param name="waypoints">The waypoints.</param>
        public TravelRoute(IEnumerable<Sector> waypoints)
        {
            _path = new List<MapLocation>();
            _waypoints = new List<MapLocation>();
            foreach (Sector sector in waypoints)
                _waypoints.Add(sector.Location);
			_waypoints.TrimExcess();
        }

        /// <summary>
        /// Gets the waypoints of a <see cref="TravelRoute"/>.
        /// </summary>
        /// <value>The waypoints.</value>
        public IList<MapLocation> Waypoints
        {
            get { return _waypoints.AsReadOnly(); }
        }

        /// <summary>
        /// Gets all of the individual steps comprising a <see cref="TravelRoute"/>.
        /// </summary>
        /// <value>The steps.</value>
        /// <remarks>
        /// This value may be regenerated due to changes in universe "terrain",
        /// supply range, and empire border openness.  In such an event, the
        /// original waypoints will be respected, but the path between them may
        /// change.
        /// </remarks>
        public IList<MapLocation> Steps
        {
            get { return _path.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TravelRoute"/> is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return (_path.Count == 0); }
        }

        /// <summary>
        /// Gets the length of this <see cref="TravelRoute"/>.
        /// </summary>
        /// <value>The length.</value>
        public int Length
        {
            get { return _path.Count; }
        }

        /// <summary>
        /// Clears this <see cref="TravelRoute"/>.
        /// </summary>
        public void Clear()
        {
            _path.Clear();
        }

        /// <summary>
        /// Removes and returns the next <see cref="MapLocation"/> from <see cref="Steps"/>.
        /// </summary>
        /// <returns>The next step.</returns>
        public MapLocation Pop()
        {
            MapLocation result = _path[0];
            _path.RemoveAt(0);
            if ((_waypoints.Count > 0) && (_waypoints[0] == result))
                _waypoints.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// Adds a new <see cref="MapLocation"/> to <see cref="Steps"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        public void Push(MapLocation location)
        {
            _path.Add(location);
        }

        /// <summary>
        /// Compacts the <see cref="Steps"/> list.
        /// </summary>
        public void Compact()
        {
            _path.TrimExcess();
        }

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Clones this <see cref="TravelRoute"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public TravelRoute Clone()
        {
            TravelRoute clone = new TravelRoute();
            clone._path.AddRange(_path);
            return clone;
        }
        #endregion

    	public void SerializeOwnedData(SerializationWriter writer, object context)
    	{
    		writer.Write(_path);
			writer.Write(_waypoints);
    	}

    	public void DeserializeOwnedData(SerializationReader reader, object context)
    	{
    		_path = reader.ReadList<MapLocation>();
			_waypoints = reader.ReadList<MapLocation>();
    	}
    }
}
