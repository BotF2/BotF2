// StarSystem.cs
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

using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.IO.Serialization;
using Supremacy.Types;

namespace Supremacy.Universe
{
    /// <summary>
    /// Represents a star system.
    /// </summary>
    [Serializable]
    public sealed class StarSystem : UniverseObject
    {
        #region Constants
        public const int MaxPlanetsPerSystem = 10;
        #endregion

        #region Fields
        private SystemBonus _bonuses;
        [NonSerialized]
        private Colony _colony;
        private ArrayWrapper<Planet> _planets;
        private StarType _starType;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StarSystem"/> class.
        /// </summary>
        public StarSystem()
        {
            _planets = new ArrayWrapper<Planet>();
            _colony = null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.StarSystem; }
        }

        /// <summary>
        /// Gets or sets a system's bonuses.
        /// </summary>
        /// <value>The bonuses.</value>
        public SystemBonus Bonuses
        {
            get { return _bonuses; }
            set { _bonuses = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a Dilithium bonus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has Dilithium bonus; otherwise, <c>false</c>.
        /// </value>
        public bool HasDilithiumBonus
        {
            get { return ((_bonuses & SystemBonus.Dilithium) == SystemBonus.Dilithium); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a Raw Materials bonus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has a Raw Materials bonus; otherwise, <c>false</c>.
        /// </value>
        public bool HasRawMaterialsBonus
        {
            get { return ((_bonuses & SystemBonus.RawMaterials) == SystemBonus.RawMaterials); }
        }

        /// <summary>
        /// Gets or sets the design of the star.
        /// </summary>
        /// <value>The design of the star.</value>
        public StarType StarType
        {
            get { return _starType; }
            set
            {
                _starType = value;
                OnPropertyChanged("StarType");
            }
        }

        /// <summary>
        /// Gets the planets.
        /// </summary>
        /// <value>The planets.</value>
        public IIndexedCollection<Planet> Planets
        {
            get { return _planets; }
        }

        /// <summary>
        /// Gets the planets in reversed order.
        /// </summary>
        /// <value>The planets in reversed order.</value>
        public IEnumerable<Planet> ReversedPlanets
        {
            get { return _planets.Reverse(); }
        }

        /// <summary>
        /// Gets or sets the colony present in this <see cref="StarSystem"/>.
        /// </summary>
        /// <value>The colony.</value>
        public Colony Colony
        {
            get { return _colony; }
            set { _colony = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="StarSystem"/> is inhabited.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="StarSystem"/> is inhabited; otherwise, <c>false</c>.
        /// </value>
        public bool IsInhabited
        {
            get { return (HasColony && (Colony.Population.CurrentValue > 0)); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="StarSystem"/> has colony.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="StarSystem"/> has colony; otherwise, <c>false</c>.
        /// </value>
        public bool HasColony
        {
            get { return (_colony != null); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a system bonus.
        /// </summary>
        /// <param name="bonus">The bonus to add.</param>
        public void AddBonus(SystemBonus bonus)
        {
            _bonuses |= bonus;
        }

        /// <summary>
        /// Gets the overall growth rate of the <see cref="StarSystem"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design.</param>
        /// <returns>The growth rate.</returns>
        public Percentage GetGrowthRate(PlanetType homePlanetType)
        {
            int maxPop = GetMaxPopulation(homePlanetType);
            Percentage growthRate = 0.0f;
            if (maxPop > 0)
            {
                foreach (Planet planet in Planets)
                {
                    growthRate += planet.GetGrowthRate(homePlanetType)
                        * ((float)planet.GetMaxPopulation(homePlanetType) / maxPop);
                }
            }
            return growthRate;
        }

        /// <summary>
        /// Gets the overall growth rate of the <see cref="StarSystem"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>The growth rate.</returns>
        public Percentage GetGrowthRate(Race race)
        {
            if (race == null)
                throw new ArgumentNullException("race");
            return GetGrowthRate(race.HomePlanetType);
        }

        /// <summary>
        /// Gets the maximum population of the <see cref="StarSystem"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design.</param>
        /// <returns>The maximum population.</returns>
        public int GetMaxPopulation(PlanetType homePlanetType)
        {
            int result = 0;
            foreach (Planet planet in Planets)
                result += planet.GetMaxPopulation(homePlanetType);
            return result;
        }

        /// <summary>
        /// Gets the maximum population of the <see cref="StarSystem"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>The maximum population.</returns>
        public int GetMaxPopulation(Race race)
        {
            if (race == null)
                throw new ArgumentNullException("race");
            return GetMaxPopulation(race.HomePlanetType);
        }

        /// <summary>
        /// Determines whether a system has the specified bonus.
        /// </summary>
        /// <param name="bonus">The bonus.</param>
        /// <returns>
        /// <c>true</c> if the system has the specified bonus; otherwise, <c>false</c>.
        /// </returns>
        public bool HasBonus(SystemBonus bonus)
        {
            return ((_bonuses & bonus) == bonus);
        }

        /// <summary>
        /// Determines whether this <see cref="StarSystem"/> is habitable for the specified race.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="StarSystem"/> is habitable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHabitable(Race race)
        {
            var habitablePlanetTypes = race.HabitablePlanetTypes;

            foreach (var planet in _planets)
            {
                if (habitablePlanetTypes[planet.PlanetType] && planet.IsHabitable(race.HomePlanetType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this <see cref="StarSystem"/> is habitable for the specified race.
        /// </summary>
        /// <param name="homePlanetType">The home planet design of the race.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="StarSystem"/> is habitable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHabitable(PlanetType homePlanetType)
        {
            foreach (var planet in _planets)
            {
                if (planet.IsHabitable(homePlanetType))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a system bonus.
        /// </summary>
        /// <param name="bonus">The bonus to remove.</param>
        public void RemoveBonus(SystemBonus bonus)
        {
            _bonuses &= ~bonus;
        }

        /// <summary>
        /// Adds a planet to this <see cref="StarSystem"/>.
        /// </summary>
        /// <param name="planet">The planet.</param>
        internal void AddPlanet(Planet planet)
        {
            if (_planets.Contains(planet))
                return;

            var planets = new Planet[_planets.Count + 1];

            _planets.CopyTo(planets);
            planets[planets.Length - 1] = planet;
            _planets = new ArrayWrapper<Planet>(planets);
        }

        /// <summary>
        /// Adds planets to this <see cref="StarSystem"/>.
        /// </summary>
        /// <param name="planets">The planets.</param>
        internal void AddPlanets(IEnumerable<Planet> planets)
        {
            if (planets == null)
                throw new ArgumentNullException("planets");

            var allPlanets = new List<Planet>(_planets);

            allPlanets.AddRange(planets);
            _planets = new ArrayWrapper<Planet>(allPlanets.ToArray());
        }

        /// <summary>
        /// Determines whether this <see cref="StarSystem"/> contains a <see cref="Planet"/> of
        /// the specified design.
        /// </summary>
        /// <param name="planetType">Type of the planet.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="StarSystem"/> contains a <see cref="Planet"/> of
        /// the specified design; otherwise, <c>false</c>.
        /// </returns>
        internal bool ContainsPlanetType(PlanetType planetType)
        {
            return _planets.Any(planet => planet.PlanetType == planetType);
        }

        /// <summary>
        /// Removes a planet from this <see cref="StarSystem"/>.
        /// </summary>
        /// <param name="planet">The planet.</param>
        internal void RemovePlanet(Planet planet)
        {
            if (!_planets.Contains(planet))
                return;

            var planetList = new List<Planet>(_planets);
            planetList.Remove(planet);
            _planets = new ArrayWrapper<Planet>(planetList.ToArray());
        }
        #endregion

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.Write((byte)_bonuses);
            //writer.WriteObject(_colony);
            writer.WriteOptimized(_planets.Values);
            writer.Write((byte)_starType);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);
            _bonuses = (SystemBonus)reader.ReadByte();
            //_colony = reader.Read<Colony>();
            _planets = new ArrayWrapper<Planet>((Planet[])reader.ReadOptimizedObjectArray(typeof(Planet)));
            _starType = (StarType)reader.ReadByte();
        }

        // needed here until it is be solved (problem with generating Gamma wormhole and so...
        public MapLocation? WormholeDestination { get; set; }
    }
}