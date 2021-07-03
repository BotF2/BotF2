// File:CivilizationManager.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;



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
        private readonly Meter _totalResearch;
        private readonly Treasury _treasury;
        private int _maintenanceCostLastTurn;
        private readonly UniverseObjectList<Colony> _colonies;
        private List<Civilization> _spiedCivList;
        public List<CivHistory> _civHist_List = new List<CivHistory>();

        private int _homeColonyId;
        private List<int> _IntelIDs;
        private MapLocation? _homeColonyLocation;
        private int _seatOfGovernmentId = -1;
        private readonly Meter _totalIntelligenceAttackingAccumulated;
        private readonly Meter _totalIntelligenceDefenseAccumulated;
        private string _text;
        //private readonly IPlayer _localPlayer;
        //private readonly AppContext _appContext;

        #endregion Fields


        #region Constructors
        [Serializable]
        public class CivHistory
        {
            public string CivIDHistAndTurn;
            public int CivIDHist;
            public string CivKeyHist;
            public int CreditsHist;
            public int ColoniesHist;
            public int PopulationHist;
            public int MaintenanceHist;
            public int ResearchHist;
            public CivHistory
                (
                string civIDHistAndTurn  // Index of civID and Turn
                , int civIDHist   // just civID
                , string civKeyHist
                , int creditsHist
                , int coloniesHist
                , int populationHist
                , int maintenanceHist
                , int researchHist
                )
            {
                CivIDHistAndTurn = civIDHistAndTurn;
                CivIDHist = civIDHist;
                CivKeyHist = civKeyHist;
                CreditsHist = creditsHist;
                ColoniesHist = coloniesHist;
                PopulationHist = populationHist;
                MaintenanceHist = maintenanceHist;
                ResearchHist = researchHist;
            }
        }

        public void AddCivHist(int civIDHist, string civKeyHist, int creditsHist, int coloniesHist, int populationHist, int maintenanceHist, int researchHist)
        {
            string _tn;
            _tn = GameContext.Current.TurnNumber.ToString();
            string civIDHistAndTurn = civIDHist + "-" + _tn;
            CivHistory civHist_New = new CivHistory(
                civIDHistAndTurn

                , civIDHist
                , civKeyHist
                , creditsHist
                , coloniesHist
                , populationHist
                , maintenanceHist
                , researchHist
                //, blank, blank, blank, blank, blank, blank, blank  // 11
                );

            if (_civHist_List != null)
            {
                _civHist_List.Add(civHist_New);
                foreach (CivHistory item in _civHist_List)
                {
                    GameLog.Core.CivsAndRacesDetails.DebugFormat("Turn;{0};CivID+Turn;{1};{2};{3};Research;{8};Col;{5};Pop;{6};Credits;{4};Maint;{7}"
                        , _tn

                        , item.CivIDHistAndTurn
                        , item.CivIDHist
                        , item.CivKeyHist

                        , item.CreditsHist
                        , item.ColoniesHist
                        , item.PopulationHist
                        , item.MaintenanceHist

                        , item.ResearchHist
                        );
                }
            }

        }
        //private AppContext _appContext => _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        public CivilizationManager()
        {
            _credits = new Meter(5000, Meter.MinValue, Meter.MaxValue);
            _treasury = new Treasury(5000);
            _maintenanceCostLastTurn = 0;
            _resources = new ResourcePool();
            _colonies = new UniverseObjectList<Colony>();

            _globalBonuses = new List<Bonus>();
            _mapData = new CivilizationMapData(
                GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);

            _totalPopulation = new Meter();
            _totalPopulation.PropertyChanged += OnTotalPopulationPropertyChanged;

            _totalResearch = new Meter();
            _totalResearch.PropertyChanged += OnTotalResearchPropertyChanged;

            _totalIntelligenceAttackingAccumulated = new Meter(0, 0, Meter.MaxValue);
            _totalIntelligenceAttackingAccumulated.PropertyChanged += OnTotalIntelligenceAttackingAccumulatedPropertyChanged;
            _totalIntelligenceDefenseAccumulated = new Meter(0, 0, Meter.MaxValue);
            _totalIntelligenceDefenseAccumulated.PropertyChanged += OnTotalIntelligenceDefenseAccumulatedPropertyChanged;

            _sitRepEntries = new List<SitRepEntry>();
            _spiedCivList = new List<Civilization>();
            _civHist_List = new List<CivHistory>();

            _resources.Deuterium.BaseValue = 100;
            _resources.Deuterium.Reset();
            _resources.Dilithium.BaseValue = 10;
            _resources.Dilithium.Reset();
            _resources.Duranium.BaseValue = 1000;
            _resources.Duranium.Reset();
            _resources.UpdateAndReset();

            //_stealCreditsSpyOperation = new List<StealCredits>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="civilization">The civilization.</param>
        public CivilizationManager(IGameContext game, Civilization civilization) : this()
        {
            if (civilization == null)
            {
                throw new ArgumentNullException("civilization");
            }

            _civId = civilization.CivID;
            _research = new ResearchPool(civilization, game.ResearchMatrix);
        }

        //public CivilizationManager(List<StealCredits> stealCreditsSpyOperation)
        //{
        //    _stealCreditsSpyOperation = stealCreditsSpyOperation;
        //}
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets the civilization ID.
        /// </summary>
        /// <value>The civilization ID.</value>
        public int CivilizationID => _civId;

        /// <summary>
        /// Gets the civilization.
        /// </summary>
        /// <value>The civilization.</value>
        public Civilization Civilization => GameContext.Current.Civilizations[_civId];

        /// <summary>
        /// Gets the total population of all the civilization's colonies.
        /// </summary>
        /// <value>The total population.</value>
        public Meter TotalPopulation => _totalPopulation;

        /// <summary>
        /// Gets the total research of all the civilization's colonies.
        /// </summary>
        /// <value>The total population.</value>
        public Meter TotalResearch => _totalResearch;

        /// <summary>
        /// Gets the credits in the civilization's treasury.
        /// </summary>
        /// <value>The credits.</value>
        public Meter Credits => _credits;

        /// <summary>
        /// Gets the civilization's treasury.
        /// </summary>
        public Treasury Treasury => _treasury;

        /// <summary>
        /// Gets the civilization's MaintenanceCostLastTurn.
        /// </summary>
        public int MaintenanceCostLastTurn
        {
            get => _maintenanceCostLastTurn;
            set => _maintenanceCostLastTurn = value;
        }



        /// <summary>
        /// Gets the civilization's resource pool.
        /// </summary>
        /// <value>The resource pool.</value>
        [NotNull]
        public ResourcePool Resources => _resources;

        /// <summary>
        /// Gets the civilization's research pool.
        /// </summary>
        /// <value>The research pool.</value>
        [NotNull]
        public ResearchPool Research => _research;

        /// <summary>
        /// Gets a list of the civilization's colonies.
        /// </summary>
        /// <value>The colonies.</value>
        [NotNull]
        public UniverseObjectList<Colony> Colonies => _colonies;

        [NotNull]
        public Colony SeatOfGovernment
        {
            get
            {
                if (_seatOfGovernmentId == -1)
                {
                    return null;
                }

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
            get
            {
                foreach (SitRepEntry rep in _sitRepEntries)
                {
                    int _player = 789;

                    if (LocalPlayer != null)
                    {
                        _player = LocalPlayer.CivID;
                    }

                    CivilizationManager playerCivManager = GameContext.Current.CivilizationManagers[_player];
                    if (playerCivManager != null && rep.Owner.ToString() == playerCivManager.ToString())
                    {
                        _text = "SitRep Turn "
                            + GameContext.Current.TurnNumber
                            + " Cat= " + rep.Categories
                            + " " + rep.Priority
                            + " Action= " + rep.Action
                            + " for " + rep.Owner
                            + ":" + Environment.NewLine
                            + "                    SitRep: " + rep.SummaryText
                            + " Cat= " + rep.Categories
                            + Environment.NewLine
                            ;

                        Console.WriteLine(_text);
                        GameLog.Core.SitReps.DebugFormat("SitRep Turn {4} Cat={2} Action {3} for {1}:" + Environment.NewLine + // splitted in 2 lines for better reading
                            "                    SitRep: {0}" + Environment.NewLine, rep.SummaryText, rep.Owner, rep.Categories, rep.Action, GameContext.Current.TurnNumber);

                    }

                }
                return _sitRepEntries;
            }
        }

        public List<Civilization> SpiedCivList => _spiedCivList;


        /// <summary>
        /// Gets the average morale of all the civilization's colonies.
        /// </summary>
        /// <value>The average morale.</value>
        public int AverageMorale
        {
            get
            {
                int totalPopulation = _totalPopulation.CurrentValue;
                double totalMorale = Colonies.Sum(colony => colony.Morale.CurrentValue * (1d / totalPopulation * colony.Population.CurrentValue));
                return (int)totalMorale;
            }
        }

        /// <summary>
        /// Gets the sum intelligence generated by all the colonies
        /// </summary>
        /// <value>The total intelligence.</value>
        public int TotalIntelligenceProduction
        {
            get
            {
                int baseIntel = Colonies.Sum(colony => colony.NetIntelligence) + _globalBonuses.Where(b => b.BonusType == BonusType.Intelligence).Sum(b => b.Amount);
                foreach (Bonus bonus in _globalBonuses.Where(b => b.BonusType == BonusType.PercentTotalIntelligence))
                {
                    baseIntel *= bonus.Amount;
                }
                //works   GameLog.Client.Intel.DebugFormat("TotalIntelProduction = {0}", baseIntel);
                return baseIntel;
            }
        }
        public Meter TotalIntelligenceAttackingAccumulated
        {
            get
            {
                Meter updateMeter = _totalIntelligenceAttackingAccumulated;

                if (_totalIntelligenceAttackingAccumulated.CurrentValue == 0)
                {
                    updateMeter.CurrentValue = TotalIntelligenceProduction;
                }
                //works   GameLog.Client.Intel.DebugFormat("TotalIntelAttackingAccumulated = {0}", updateMeter.CurrentValue);
                return updateMeter;
            }
        }

        public Meter TotalIntelligenceDefenseAccumulated
        {
            get
            {
                Meter updateMeter = _totalIntelligenceDefenseAccumulated;
                //works   GameLog.Client.Intel.DebugFormat("TotalIntelDefenseAccumulated = {0}", updateMeter.CurrentValue);
                if (_totalIntelligenceDefenseAccumulated.CurrentValue == 0)
                {
                    updateMeter.CurrentValue = TotalIntelligenceProduction;
                }
                return _totalIntelligenceDefenseAccumulated;
            }
        }

        public bool ControlsHomeSystem
        {
            get
            {
                StarSystem homeSystem = HomeSystem;
                if (homeSystem == null)
                {
                    return false;
                }

                return homeSystem.OwnerID == CivilizationID;
            }
        }

        public bool IsHomeColonyDestroyed
        {
            get
            {
                StarSystem homeSystem = HomeSystem;
                if (homeSystem == null)
                {
                    return false;
                }

                Colony colony = homeSystem.Colony;
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
            get => GameContext.Current.Universe.Get<Colony>(_homeColonyId);
            internal set
            {
                _homeColonyId = (value != null) ? value.ObjectID : -1;

                if (value != null)
                {
                    _homeColonyLocation = value.Location;
                }
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
                if (!_homeColonyLocation.HasValue)
                {
                    return null;
                }

                return GameContext.Current.Universe.Map[_homeColonyLocation.Value].System;
            }
        }

        /// <summary>
        /// Gets the civilization's tech tree.
        /// </summary>
        /// <value>The tech tree.</value>
        public TechTree TechTree
        {
            get => GameContext.Current.TechTrees[_civId];
            internal set => GameContext.Current.TechTrees[_civId] = value;
        }

        /// <summary>
        /// Gets the civilization's global bonuses.
        /// </summary>
        /// <value>The global bonuses.</value>
        public IList<Bonus> GlobalBonuses => _globalBonuses;

        /// <summary>
        /// Gets the map data for the civilization.
        /// </summary>
        /// <value>The map data.</value>
        public CivilizationMapData MapData => _mapData;

        /// <summary>
        /// Gets the desired borders for the civilization.
        /// </summary>
        /// <value>The desired borders.</value>
        public ConvexHullSet DesiredBorders { get; internal set; }
        #endregion

        #region Methods

        public void UpDateSpiedList(List<Civilization> civList)
        {
            _spiedCivList.AddRange(civList);
            //foreach (var item in civList)
            //{
            //    GameLog.Client.Intel.DebugFormat("Updated the spied list = {0}", item);
            //}
        }

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
            Data.Table moraleTable = GameContext.Current.Tables.MoraleTables["MoraleEventResults"];
            if (moraleTable == null)
            {
                return;
            }

            const float multiplier = 1.0f;

            string tableValue = moraleTable[eventType.ToString()][_civId] ??
                             moraleTable[eventType.ToString()][0];

            if (tableValue == null)
            {
                return;
            }

            if (!int.TryParse(tableValue, out int change))
            {
                return;
            }

            foreach (Colony colony in Colonies)
            {
                _ = colony.Morale.AdjustCurrent((int)(multiplier * change));
            }
        }

        /// <summary>
        /// Compacts this instance for serialization.
        /// </summary>
        public void Compact()
        {
            _colonies.TrimExcess();
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
            Colony seatOfGovernment = SeatOfGovernment;
            if (seatOfGovernment == null || seatOfGovernment.OwnerID != CivilizationID)
            {
                MapLocation? homeColonyLocation = _homeColonyLocation;

                Func<Colony, double> rankHueristic = (Func<Colony, double>)
                                    (c =>
                                     {
                                         if (!homeColonyLocation.HasValue)
                                         {
                                             return 1d;
                                         }

                                         double distanceFactor = Math.Min(
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
                {
                    _seatOfGovernmentId = seatOfGovernment.ObjectID;
                }
                else
                {
                    _seatOfGovernmentId = -1;
                }
            }

            Diplomacy.Diplomat diplomat = GameContext.Current.Diplomats[_civId];
            if (diplomat != null)
            {
                diplomat.SeatOfGovernment = seatOfGovernment;
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the TotalPopulation property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnTotalPopulationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("AverageMorale");
            }
        }
        private void OnTotalResearchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("AverageMorale");
            }
        }
        private void OnInstallingSpyNetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("InstallingSpyNetwork");
            }
        }
        private void OnTotalIntelligenceAttackingAccumulatedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GameLog.Client.Intel.DebugFormat("OnTotalIntelAttackingAccumulated sender ={0} property changed ={1}", sender.ToString(), e.PropertyName.ToString());
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            }
        }

        private void OnTotalIntelligenceDefenseAccumulatedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GameLog.Client.Intel.DebugFormat("OnTotalIntelDefenceAccumulated sender ={0} property changed ={1}", sender.ToString(), e.PropertyName.ToString());
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Static Accessors

        public static CivilizationManager For([NotNull] Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("civ");
            }

            return GameContext.Current.CivilizationManagers[civ];
        }

        public static CivilizationManager For([NotNull] string civKey)
        {
            if (civKey == null)
            {
                throw new ArgumentNullException("civKey");
            }

            return GameContext.Current.CivilizationManagers[civKey];
        }

        public static CivilizationManager For(int civId)
        {
            return GameContext.Current.CivilizationManagers[civId];
        }

        #endregion

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID => _civId;

        public List<int> IntelIDs { get => _IntelIDs; set => _IntelIDs = value; }
        public object AppContextProperty { get; private set; }
        public Civilization LocalPlayer { get; private set; }


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
            : base(keyRetriever) { }

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
                {
                    throw new ArgumentNullException("civilization");
                }

                _ = TryGetValue(civilization.CivID, out TValue value);
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
                _ = TryGetValue(civKey, out TValue value);
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
            if (GameContext.Current.Civilizations.TryGetValue(civKey, out Civilization civ))
            {
                if (civ != null)
                {
                    value = this[civ.CivID];
                    return true;
                }
            }
            value = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default;
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
            : base(o => o.CivilizationID) { }

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
            : base(o => o.ObjectID) { }

        #endregion
    }

}