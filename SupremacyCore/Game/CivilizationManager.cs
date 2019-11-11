// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Supremacy.Orbitals;

namespace Supremacy.Game
{
    /// <summary>
    /// Contains data and logic for managing an individual <see cref="Civilization"/> and its assets.
    /// </summary>
    [Serializable]
    public class CivilizationManager : INotifyPropertyChanged, ICivIdentity
    {
        #region Fields
        private readonly int _civId;
        private readonly Meter _credits;
        private readonly List<Bonus> _globalBonuses;
        private readonly CivilizationMapData _mapData;
        private readonly ResearchPool _research;
        private readonly ResourcePool _resources;
        private readonly List<SitRepEntry> _sitRepEntries;
        private readonly Meter _totalPopulation;
        private readonly Treasury _treasury;
        private readonly UniverseObjectList<Colony> _colonies;
        private Dictionary<Civilization, List<Colony>> _infiltratedColonies;
        private int _homeColonyId;
        private MapLocation? _homeColonyLocation;
        private int _seatOfGovernmentId = -1;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        private CivilizationManager()
        {
            _credits = new Meter(5000, Meter.MinValue, Meter.MaxValue);
            _treasury = new Treasury(5000);
            _resources = new ResourcePool();
            _colonies = new UniverseObjectList<Colony>();

            _globalBonuses = new List<Bonus>();
            _mapData = new CivilizationMapData(
                GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);

            _totalPopulation = new Meter();
            _totalPopulation.PropertyChanged += OnTotalPopulationPropertyChanged;

            _sitRepEntries = new List<SitRepEntry>();

            _resources.Deuterium.BaseValue = 100;
            _resources.Deuterium.Reset();
            _resources.Dilithium.BaseValue = 10;
            _resources.Dilithium.Reset();
            _resources.RawMaterials.BaseValue = 1000;
            _resources.RawMaterials.Reset();
            _resources.UpdateAndReset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="civilization">The civilization.</param>
        public CivilizationManager(IGameContext game, Civilization civilization) : this()
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");

            _civId = civilization.CivID;
            _research = new ResearchPool(civilization, game.ResearchMatrix);
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets the civilization ID.
        /// </summary>
        /// <value>The civilization ID.</value>
        public int CivilizationID
        {
            get { return _civId; }
        }

        /// <summary>
        /// Gets the civilization.
        /// </summary>
        /// <value>The civilization.</value>
        public Civilization Civilization
        {
            get { return GameContext.Current.Civilizations[_civId]; }
        }

        /// <summary>
        /// Gets the total population of all the civilization's colonies.
        /// </summary>
        /// <value>The total population.</value>
        public Meter TotalPopulation
        {
            get { return _totalPopulation; }
        }

        /// <summary>
        /// Gets the credits in the civilization's treasury.
        /// </summary>
        /// <value>The credits.</value>
        public Meter Credits
        {
            get { return _credits; }
        }

        /// <summary>
        /// Gets the civilization's treasury.
        /// </summary>
        public Treasury Treasury
        {
            get { return _treasury; }
        }

        /// <summary>
        /// Gets the civilization's resource pool.
        /// </summary>
        /// <value>The resource pool.</value>
        [NotNull]
        public ResourcePool Resources
        {
            get { return _resources; }
        }

        /// <summary>
        /// Gets the civilization's research pool.
        /// </summary>
        /// <value>The research pool.</value>
        [NotNull]
        public ResearchPool Research
        {
            get { return _research; }
        }

        /// <summary>
        /// Gets a list of the civilization's colonies.
        /// </summary>
        /// <value>The colonies.</value>
        [NotNull]
        public UniverseObjectList<Colony> Colonies
        {
            get { return _colonies; }
        }

        ///// <summary>
        ///// Gets a list of the spied civilization's colonies.
        ///// </summary>
        ///// <value>The colonies.</value>
        //[NotNull]
        //public UniverseObjectList<Colony> SpyColonies
        //{
        //    get { return _spyColonies; }
        //}

        /// <summary>
        /// Gets a list of the civilization's infiltrated colonies.
        /// </summary>
        /// <value>The infiltrated colonies.</value>
        [NotNull]
        public Dictionary<Civilization, List<Colony>> InfiltratedColonies
        {
            get { return _infiltratedColonies; }
            // set{ alksdjf = value}
        }

        public Colony SeatOfGovernment
        {
            get
            {
                if (_seatOfGovernmentId == -1)
                    return null;

                return GameContext.Current.Universe.Objects[_seatOfGovernmentId] as Colony;
            }
        }

        /// <summary>
        /// Gets the list of SitRep entries for the civilization.
        /// </summary>
        /// <value>The SitRep entries.</value>
        [NotNull]
        public IList<SitRepEntry> SitRepEntries
        {
            get { return _sitRepEntries; }
        }

        /// <summary>
        /// Gets the average morale of all the civilization's colonies.
        /// </summary>
        /// <value>The average morale.</value>
        public int AverageMorale
        {
            get
            {
                var totalPopulation = _totalPopulation.CurrentValue;
                var totalMorale = Colonies.Sum(colony => colony.Morale.CurrentValue * ((1d / totalPopulation) * colony.Population.CurrentValue));
                return (int)totalMorale;
            }
        }

        /// <summary>
        /// Gets the sum intelligence generated by all the colonies
        /// </summary>
        /// <value>The total intelligence.</value>
        public int TotalIntelligence
        {
            get
            {
                var baseIntel = Colonies.Sum(colony => colony.NetIntelligence) + _globalBonuses.Where(b => b.BonusType == BonusType.Intelligence).Sum(b => b.Amount);
                foreach (var bonus in _globalBonuses.Where(b => b.BonusType == BonusType.PercentTotalIntelligence))
                {
                    baseIntel *= bonus.Amount;
                }
                return baseIntel;
            }
        }

        public bool ControlsHomeSystem
        {
            get
            {
                var homeSystem = HomeSystem;
                if (homeSystem == null)
                    return false;
                return homeSystem.OwnerID == CivilizationID;
            }
        }

        public bool IsHomeColonyDestroyed
        {
            get
            {
                var homeSystem = HomeSystem;
                if (homeSystem == null)
                    return false;

                var colony = homeSystem.Colony;
                return colony == null ||
                       colony.ObjectID != _homeColonyId;
            }
        }

        /// <summary>
        /// Gets the civilization's home colony.
        /// </summary>
        /// <value>The home colony.</value>
        public Colony HomeColony
        {
            get { return GameContext.Current.Universe.Get<Colony>(_homeColonyId); }
            internal set
            {
                _homeColonyId = (value != null) ? value.ObjectID : -1;
                
                if (value != null)
                    _homeColonyLocation = value.Location;
            }
        }

        /// <summary>
        /// Gets the civilization's home system.
        /// </summary>
        /// <value>The home system.</value>
        public StarSystem HomeSystem
        {
            get
            {
                if(!_homeColonyLocation.HasValue)
                    return null;
                return GameContext.Current.Universe.Map[_homeColonyLocation.Value].System;
            }
        }

        /// <summary>
        /// Gets the civilization's tech tree.
        /// </summary>
        /// <value>The tech tree.</value>
        public TechTree TechTree
        {
            get { return GameContext.Current.TechTrees[_civId]; }
            internal set { GameContext.Current.TechTrees[_civId] = value; }
        }

        /// <summary>
        /// Gets the civilization's global bonuses.
        /// </summary>
        /// <value>The global bonuses.</value>
        public IList<Bonus> GlobalBonuses
        {
            get { return _globalBonuses; }
        }

        /// <summary>
        /// Gets the map data for the civilization.
        /// </summary>
        /// <value>The map data.</value>
        public CivilizationMapData MapData
        {
            get { return _mapData; }
        }

        /// <summary>
        /// Gets the desired borders for the civilization.
        /// </summary>
        /// <value>The desired borders.</value>
        public ConvexHullSet DesiredBorders { get; internal set; }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the specified morale event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        public void ApplyMoraleEvent(MoraleEvent eventType)
        {
            ApplyMoraleEvent(eventType, SeatOfGovernment.Location);
        }

        /// <summary>
        /// Applies the specified morale event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="location">The location at which the event occurred.</param>
        public void ApplyMoraleEvent(MoraleEvent eventType, MapLocation location)
        {
            var moraleTable = GameContext.Current.Tables.MoraleTables["MoraleEventResults"];
            if (moraleTable == null)
                return;

            const float multiplier = 1.0f;

            var tableValue = moraleTable[eventType.ToString()][_civId] ??
                             moraleTable[eventType.ToString()][0];
            
            if (tableValue == null)
                return;

            int change;

            if (!int.TryParse(tableValue, out change))
                return;

            foreach (var colony in Colonies)
            {
                colony.Morale.AdjustCurrent((int)(multiplier * change));
            }
        }

        /// <summary>
        /// Compacts this instance for serialization.
        /// </summary>
        public void Compact()
        {
            _colonies.TrimExcess();
            //_infiltratedColonies.TrimExcess();
            _globalBonuses.TrimExcess();
            _sitRepEntries.TrimExcess();
        }

        /// <summary>
        /// Called when the current game turn is finished.
        /// </summary>
        public void OnTurnFinished()
        {
            OnPropertyChanged("AverageMorale");
        }

        public void EnsureSeatOfGovernment()
        {
            var seatOfGovernment = SeatOfGovernment;
            if (seatOfGovernment == null || seatOfGovernment.OwnerID != CivilizationID)
            {
                var homeColonyLocation = _homeColonyLocation;

                var rankHueristic = (Func<Colony, double>)
                                    (c =>
                                     {
                                         if (!homeColonyLocation.HasValue)
                                             return 1d;

                                         var distanceFactor = Math.Min(
                                             0.2,
                                             Math.Max(
                                                 1d,
                                                 2d / MapLocation.GetDistance(c.Location, homeColonyLocation.Value)));

                                         return c.ColonyValue() * distanceFactor;
                                     });

                seatOfGovernment = (
                                       from c in Colonies
                                       where c.OwnerID == CivilizationID
                                       orderby rankHueristic(c) descending
                                       select c
                                   ).FirstOrDefault();

                if (seatOfGovernment != null)
                    _seatOfGovernmentId = seatOfGovernment.ObjectID;
                else
                    _seatOfGovernmentId = -1;
            }

            var diplomat = GameContext.Current.Diplomats[_civId];
            if (diplomat != null)
                diplomat.SeatOfGovernment = seatOfGovernment;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the TotalPopulation property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnTotalPopulationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
                OnPropertyChanged("AverageMorale");
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field : NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Static Accessors

        public static CivilizationManager For([NotNull] Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            return GameContext.Current.CivilizationManagers[civ];
        }

        public static CivilizationManager For([NotNull] string civKey)
        {
            if (civKey == null)
                throw new ArgumentNullException("civKey");
            return GameContext.Current.CivilizationManagers[civKey];
        }

        public static CivilizationManager For(int civId)
        {
            return GameContext.Current.CivilizationManagers[civId];
        }

        #endregion

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID
        {
            get { return _civId; }
        }

        #endregion
    }

    /// <summary>
    /// A collection of <typeparamref name="TValue"/> instances, keyed by
    /// civilization ID and indexed by civilization ID or civilization.
    /// </summary>
    [Serializable]
    public class CivilizationKeyedMap<TValue> : KeyedCollectionBase<int, TValue>
    {
        #region Constructors

        public CivilizationKeyedMap(Func<TValue, int> keyRetriever)
            : base(keyRetriever) {}

        #endregion

        #region Properties and Indexers

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified civilization.
        /// </summary>
        /// <value>The <typeparamref name="TValue"/>.</value>
        public TValue this[ICivIdentity civilization]
        {
            get
            {
                if (civilization == null)
                    throw new ArgumentNullException("civilization");
                TValue value;
                TryGetValue(civilization.CivID, out value);
                return value;
            }
        }

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified civilization.
        /// </summary>
        /// <value>The <typeparamref name="TValue"/>.</value>
        public TValue this[string civKey]
        {
            get
            {
                TValue value;
                TryGetValue(civKey, out value);
                return value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to get the <typeparamref name="TValue"/> for the Civilization with the key <paramref name="civKey"/>.
        /// </summary>
        /// <param name="civKey">The Civilization's key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c></returns>
        public bool TryGetValue(string civKey, out TValue value)
        {
            Civilization civ;
            if (GameContext.Current.Civilizations.TryGetValue(civKey, out civ))
            {
                if (civ != null)
                {
                    value = this[civ.CivID];
                    return true;
                }
            }
            value = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default(TValue);
            return false;
        }

        #endregion
    }

    /// <summary>
    /// A collection of <see cref="CivilizationManager"/> instances, keyed by
    /// civilization ID and indexed by civilization ID or civilization.
    /// </summary>
    [Serializable]
    public class CivilizationManagerMap : CivilizationKeyedMap<CivilizationManager>
    {
        #region Constructors

        public CivilizationManagerMap()
            : base(o => o.CivilizationID) {}

        #endregion
    }

    /// <summary>
    /// A collection of Colonies keyed by Object ID.
    /// </summary>
    [Serializable]
    public sealed class ColonyMap : KeyedCollectionBase<int, Colony>
    {
        #region Constructors

        public ColonyMap()
            : base(o => o.ObjectID) {}

        #endregion
    }
}