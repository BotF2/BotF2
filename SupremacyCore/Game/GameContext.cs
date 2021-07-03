// GameContext.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.IO.Serialization;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Tech;
using Supremacy.Text;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Supremacy.Game
{
    public interface IGameContext : IOwnedDataSerializableAndRecreatable
    {
        bool IsMultiplayerGame { get; }

        void LoadStrings([NotNull] ITextDatabase textDatabase);

        ICollection<ScriptedEvent> ScriptedEvents { get; }

        /// <summary>
        /// Gets the game data tables.
        /// </summary>
        /// <value>The tables.</value>
        GameTables Tables { get; }

        /// <summary>
        /// Gets the game options with which this instance was created.
        /// </summary>
        /// <value>The options.</value>
        GameOptions Options { get; }

        /// <summary>
        /// Gets or sets the current game mod.
        /// </summary>
        /// <value>The current game mod, or <c>null</c> if no mod is loaded.</value>
        GameMod GameMod { get; }

        /// <summary>
        /// Gets the civilizations in the current game.
        /// </summary>
        /// <value>The civilizations.</value>
        CivDatabase Civilizations { get; }

        /// <summary>
        /// Gets the civilization managers for the civilizations in the current game.
        /// </summary>
        /// <value>The civilization managers.</value>
        /// <remarks>
        /// In the server-side instance, the returned collection contains managers
        /// for every civilization in the current game.  The client-side instance
        /// contains only the manager for the local player's civilization.
        /// </remarks>
        CivilizationManagerMap CivilizationManagers { get; }

        /// <summary>
        /// Gets the races in the current game.
        /// </summary>
        /// <value>The races.</value>
        RaceDatabase Races { get; }

        /// <summary>
        /// Gets the universe manager for the current game.
        /// </summary>
        /// <value>The universe manager.</value>
        UniverseManager Universe { get; }

        /// <summary>
        /// Gets the tech database for the current game.
        /// </summary>
        /// <value>The tech database.</value>
        TechDatabase TechDatabase { get; }

        /// <summary>
        /// Gets or sets the turn number for the current game.
        /// </summary>
        /// <value>The turn number.</value>
        int TurnNumber { get; set; }

        /// <summary>
        /// Gets the research matrix for the current game.
        /// </summary>
        /// <value>The research matrix.</value>
        ResearchMatrix ResearchMatrix { get; }

        /// <summary> Do we still need this? are we no longer trying to make intel like ResearchMatrix (IntelMatrix), ResearchPool (IntelPool)
        /// Gets the intel matrix for the current game.
        /// </summary>
        /// <value>The research matrix.</value>
        //IntelMatrix IntelMatrix { get; }

        /// <summary>
        /// Gets the map of sector claims for the current game.
        /// </summary>
        /// <value>The map of sector claims.</value>
        SectorClaimGrid SectorClaims { get; }

        TechTreeMap TechTrees { get; }

        StrategyDatabase StrategyDatabase { get; }

        /// <summary>
        /// Gets a double-keyed map of the diplomacy data for every pair of civilizations for the current game.
        /// </summary>
        /// <value>The diplomacy data map.</value>
        CivilizationPairedMap<IDiplomacyData> DiplomacyData { get; }

        /// <summary>
        /// Gets the <see cref="Civilization"/>-to-<see cref="Diplomat"/> map for the current game.
        /// </summary>
        /// <value>The <see cref="Civilization"/>-to-<see cref="Diplomat"/> map.</value>
        CivilizationKeyedMap<Diplomat> Diplomats { get; }

        /// <summary>
        /// Generates a new object ID for use in the current game.
        /// </summary>
        /// <returns>The object ID.</returns>
        int GenerateID();

        AgreementMatrix AgreementMatrix { get; }
    }

    /// <summary>
    /// Holds all data pertaining to a specific game instance.  The class also
    /// functions as a static stack of class instances, with the <see cref="Current"/>
    /// property pointing to the instance at the top of the stack.
    /// </summary>
    [Serializable]
    public sealed class GameContext : IGameContext
    {
        #region Instance Members
        #region Fields
        private int _nextObjectId;
        private int _turnNumber = 0;
        private GameOptions _options;
        private GameMod _gameMod;
        private CivDatabase _civilizations;
        private CivilizationManagerMap _civManagers;
        private RaceDatabase _races;
        private UniverseManager _universe;
        private TechDatabase _techDatabase;
        [NonSerialized]
        private GameTables _tables;
        private ResearchMatrix _researchMatrix;
        private SectorClaimGrid _sectorClaims;
        private TechTreeMap _techTrees;
        private CivilizationPairedMap<IDiplomacyData> _diplomacyData;
        private AgreementMatrix _agreementMatrix;
        private CivilizationKeyedMap<Diplomat> _diplomats;
        private StrategyDatabase _strategyDatabase;
        private ICollection<ScriptedEvent> _scriptedEvents;
        private DiplomacyDatabase _diplomacyDatabase;
        #endregion

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(IsMultiplayerGame);
            writer.WriteOptimized(_nextObjectId);
            writer.WriteOptimized((ushort)_turnNumber);
            writer.WriteObject(_options);
            writer.WriteObject(_gameMod);
            writer.WriteObject(_civilizations);
            writer.WriteObject(_civManagers);
            writer.WriteObject(_races);
            writer.WriteObject(_universe);
            writer.WriteObject(_techDatabase);
            writer.WriteObject(_researchMatrix);
            writer.WriteObject(_sectorClaims);
            writer.WriteObject(_techTrees);
            writer.WriteObject(_diplomacyData);
            writer.WriteObject(_agreementMatrix);
            writer.WriteObject(_diplomats);
            writer.WriteObject(_strategyDatabase);
            writer.WriteObject(_scriptedEvents);
            writer.WriteObject(_diplomacyDatabase);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            PushThreadContext(this);

            try
            {
                GameLog.Core.SaveLoad.DebugFormat("--------------------------------------------------");
                GameLog.Core.SaveLoad.DebugFormat("########### Deserialising GameContext...");
                IsMultiplayerGame = reader.ReadBoolean();
                GameLog.Core.SaveLoad.DebugFormat("IsMultiplayerGame = {0}", IsMultiplayerGame);
                _nextObjectId = reader.ReadOptimizedInt32();
                _turnNumber = reader.ReadOptimizedUInt16();
                GameLog.Core.SaveLoad.DebugFormat("_turnNumber = {0}", _turnNumber);
                _options = reader.Read<GameOptions>();
                _gameMod = reader.Read<GameMod>();
                _civilizations = reader.Read<CivDatabase>();
                // CivDatabase = basic Civs (like ShortName etc)
                _civManagers = reader.Read<CivilizationManagerMap>();
                // _civManagers = basic Civs (like ShortName etc)
                _races = reader.Read<RaceDatabase>();
                _universe = reader.Read<UniverseManager>();
                GameLog.Core.SaveLoad.DebugFormat("reading _universe.....");
                _techDatabase = reader.Read<TechDatabase>();
                _researchMatrix = reader.Read<ResearchMatrix>();
                _sectorClaims = reader.Read<SectorClaimGrid>();
                _techTrees = reader.Read<TechTreeMap>();
                _diplomacyData = reader.Read<CivilizationPairedMap<IDiplomacyData>>();
                _agreementMatrix = reader.Read<AgreementMatrix>();
                _diplomats = reader.Read<CivilizationKeyedMap<Diplomat>>();
                _strategyDatabase = reader.Read<StrategyDatabase>();
                _scriptedEvents = reader.Read<ICollection<ScriptedEvent>>();
                _diplomacyDatabase = reader.Read<DiplomacyDatabase>();


                FixupDiplomacyData();
            }
            finally
            {
                _ = PopThreadContext();
            }
        }

        private void FixupDiplomacyData()
        {
            CivilizationPairedMap<IDiplomacyData> diplomacyData = new CivilizationPairedMap<IDiplomacyData>();

            // going through civ managers better reflects which civ got spawned
            foreach (CivilizationManager civMgr1 in _civManagers)
            {
                Civilization civ1 = civMgr1.Civilization;
                Diplomat diplomat = _diplomats[civ1];

                foreach (CivilizationManager civMgr2 in _civManagers)
                {
                    Civilization civ2 = civMgr2.Civilization;
                    if (civ1 == civ2)
                    {
                        continue;
                    }

                    diplomacyData.Add(civ1, civ2, diplomat.GetData(civ2));
                }
            }

            _diplomacyData = diplomacyData;
        }

        public bool IsMultiplayerGame { get; internal set; }

        public void LoadStrings([NotNull] ITextDatabase textDatabase)
        {
            if (textDatabase == null)
            {
                throw new ArgumentNullException("textDatabase");
            }

            ITextDatabaseTable<ITechObjectTextDatabaseEntry> techObjectTable = textDatabase.GetTable<ITechObjectTextDatabaseEntry>(); //Does this every get any data?????

            foreach (TechObjectDesign design in _techDatabase)
            {
                ///GameLog.Client.GameInitData.DebugFormat("THE design Key ={0}; Name ={1}; Description ={2}", design.Key, design.Name, design.Description);
                        // This is Orbital Batteries Only!!! 
                if (LocalizedTextDatabase.Instance.Groups.TryGetValue(new TechObjectTextGroupKey(design.Key), out LocalizedTextGroup localizedText))
                {
                    //GameLog.Client.GameInitData.DebugFormat("###### textDatabase localizedTest = {0} {1} {2} {3} {4}",
                    //    localizedText.DefaultEntry, localizedText.DefaultLocalText, localizedText.Entries, localizedText.Key, design.Key );
                    design.LocalizedText = localizedText;
                    continue;
                }
                if (!techObjectTable.TryGetEntry(design.Key, out ITextDatabaseEntry<ITechObjectTextDatabaseEntry> entry))
                {
                    continue;
                }

                design.TextDatabaseEntry = entry.GetLocalizedEntry(ResourceManager.CurrentLocale);
                //GameLog.Client.GameInitData.DebugFormat("THE ^^TextDatabaseEntry ={0} {1}", design.TextDatabaseEntry.Name, design.TextDatabaseEntry.Description);
            }
        }

        #region Properties
        /// <summary>
        /// Gets the game data tables.
        /// </summary>
        /// <value>The tables.</value>
        public GameTables Tables
        {
            get => _tables;
            internal set => _tables = value;
        }

        /// <summary>
        /// Gets the game options with which this instance was created.
        /// </summary>
        /// <value>The options.</value>
        public GameOptions Options
        {
            get => _options;
            internal set => _options = value;
        }

        /// <summary>
        /// Gets or sets the current game mod.
        /// </summary>
        /// <value>The current game mod, or <c>null</c> if no mod is loaded.</value>
        public GameMod GameMod
        {
            get => _gameMod;
            set => _gameMod = value;
        }

        /// <summary>
        /// Gets the civilizations in the current game.
        /// </summary>
        /// <value>The civilizations.</value>
        public CivDatabase Civilizations
        {
            get => _civilizations;
            internal set => _civilizations = value;
        }

        /// <summary>
        /// Gets the civilization managers for the civilizations in the current game.
        /// </summary>
        /// <value>The civilization managers.</value>
        /// <remarks>
        /// In the server-side instance, the returned collection contains managers
        /// for every civilization in the current game.  The client-side instance
        /// contains only the manager for the local player's civilization.
        /// </remarks>
        public CivilizationManagerMap CivilizationManagers
        {
            get => _civManagers;
            internal set => _civManagers = value;
        }

        /// <summary>
        /// Gets the races in the current game.
        /// </summary>
        /// <value>The races.</value>
        public RaceDatabase Races
        {
            get => _races;
            internal set => _races = value;
        }

        /// <summary>
        /// Gets the universe manager for the current game.
        /// </summary>
        /// <value>The universe manager.</value>
        public UniverseManager Universe
        {
            get => _universe;
            internal set => _universe = value;
        }

        /// <summary>
        /// Gets the tech database for the current game.
        /// </summary>
        /// <value>The tech database.</value>
        public TechDatabase TechDatabase
        {
            get => _techDatabase;
            internal set => _techDatabase = value;
        }

        public ICollection<ScriptedEvent> ScriptedEvents
        {
            get => _scriptedEvents;
            internal set => _scriptedEvents = value;
        }

        public DiplomacyDatabase DiplomacyDatabase
        {
            get => _diplomacyDatabase;
            internal set => _diplomacyDatabase = value;
        }

        public event EventHandler TurnNumberChanged;

        private void OnTurnNumberChanged()
        {
            GameLog.Client.General.InfoFormat("------------------------------ TURN {0} ------------------------------", TurnNumber);
            TurnNumberChanged?.Invoke(this, EventArgs.Empty);

            if (!IsMultiplayerGame)
            {
                // doesn't work - plan is to give output to Log.txt: Credits and more out of own Empire Info
                //var civ = Current.Civilizations["FEDERATION"] ?? Current.Civilizations.FirstOrDefault(o => o.IsEmpire);
                //GameLog.Client.GameData.DebugFormat("Player.GameHostID: {0}", civ.Name);
            }
        }

        /// <summary>
        /// Gets or sets the turn number for the current game.
        /// </summary>
        /// <value>The turn number.</value>
        public int TurnNumber
        {
            get => _turnNumber;
            set
            {
                if (Equals(_turnNumber, value))
                {
                    return;
                }

                _turnNumber = value;
                OnTurnNumberChanged();
            }
        }

        /// <summary>
        /// Gets the research matrix for the current game.
        /// </summary>
        /// <value>The research matrix.</value>
        public ResearchMatrix ResearchMatrix
        {
            get => _researchMatrix;
            internal set => _researchMatrix = value;
        }

        /// <summary> Do we still need this matrix part of intel??? not making intel like research anymore?
        /// Gets the intel matrix for the current game.
        /// </summary>
        /// <value>The intel matrix.</value>
        //public IntelMatrix IntelMatrix
        //{
        //    get { return _intelMatrix; }
        //    internal set { _intelMatrix = value; }
        //}

        /// <summary>
        /// Gets the map of sector claims for the current game.
        /// </summary>
        /// <value>The map of sector claims.</value>
        public SectorClaimGrid SectorClaims
        {
            get => _sectorClaims;
            internal set => _sectorClaims = value;
        }

        public TechTreeMap TechTrees
        {
            get => _techTrees;
            internal set => _techTrees = value;
        }

        public StrategyDatabase StrategyDatabase
        {
            get => _strategyDatabase;
            internal set => _strategyDatabase = value;
        }

        /// <summary>
        /// Gets a double-keyed map of the diplomacy data for every pair of civilizations for the current game.
        /// </summary>
        /// <value>The diplomacy data map.</value>
        public CivilizationPairedMap<IDiplomacyData> DiplomacyData
        {
            get => _diplomacyData;
            internal set => _diplomacyData = value;
        }

        public AgreementMatrix AgreementMatrix
        {
            get => _agreementMatrix;
            internal set => _agreementMatrix = value;
        }

        /// <summary>
        /// Gets the <see cref="Civilization"/>-to-<see cref="Diplomat"/> map for the current game.
        /// </summary>
        /// <value>The <see cref="Civilization"/>-to-<see cref="Diplomat"/> map.</value>
        public CivilizationKeyedMap<Diplomat> Diplomats
        {
            get => _diplomats;
            internal set => _diplomats = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext"/> class.
        /// </summary>
        public GameContext() { }
        #endregion

        #region Methods
        /// <summary>
        /// Generates a new object ID for use in the current game.
        /// </summary>
        /// <returns>The object ID.</returns>
        public int GenerateID()
        {
            return _nextObjectId++;
        }
        #endregion
        #endregion

        #region Static Members
        private static readonly ConcurrentStack<GameContext> _stack = new ConcurrentStack<GameContext>();

        [ThreadStatic]
        private static Stack<GameContext> _threadStack;

        private static Stack<GameContext> ThreadStack
        {
            get
            {
                if (_threadStack == null)
                {
                    _threadStack = new Stack<GameContext>();
                }

                return _threadStack;
            }
        }

        /// <summary>
        /// Pushes the specified context onto the stack for the current thread only.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void PushThreadContext(GameContext context)
        {
            ThreadStack.Push(context);
        }

        /// <summary>
        /// Pops a context off the top of the thread-specific stack.
        /// </summary>
        /// <returns>The popped context, or <c>null</c> if the stack is empty.</returns>
        public static GameContext PopThreadContext()
        {

            if (!ThreadStack.TryPop(out GameContext result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the <see cref="GameContext"/> currently at the top of the
        /// stack matches <paramref name="context"/> and performs a Pop()
        /// only if it was pushed by the current thread.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public static bool CheckAndPop(GameContext context)
        {
            if (!_stack.TryPeek(out GameContext top))
            {
                return false;
            }
            if (top != context)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pops a context off the top of the stack.
        /// </summary>
        /// <returns>The popped context, or <c>null</c> if the stack is empty.</returns>
        public static GameContext Pop()
        {
            if (!_stack.TryPop(out GameContext result))
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Gets the context at the top of the stack.
        /// </summary>
        /// <returns>The context.</returns>
        public static GameContext Peek()
        {
            if (!_stack.TryPeek(out GameContext result))
            {
                return null;
            }

            return result;
        }

        private static readonly Lazy<bool> _isInDesignMode = new Lazy<bool>(() => DesignerProperties.GetIsInDesignMode(new DependencyObject()), false);

        /// <summary>
        /// Gets the context at the top of the stack.
        /// </summary>
        /// <value>The context.</value>
        public static GameContext Current
        {
            get
            {
                GameContext gameContext = ThreadContext ?? Peek();
                if (gameContext != null)
                {
                    // keep this for next time we have to check Game Context
                    //foreach (var civManager in gameContext.CivilizationManagers)
                    //{
                    //    //    if (civManager.Civilization.IsEmpire)
                    //    //        continue;
                    //    if (civManager.CivilizationID != 4) // only Cardassians
                    //        continue;

                    //bool output = false;

                    //    string _gameLogText = "Civ= " + civManager.CivilizationID +"  :";//  .Civilization.Key;
                    //                                                                     //string _gameLogText = "Hello";
                    //if (civManager.IntelOrdersGoingToHost != null)
                    //{
                    //    _gameLogText += civManager.IntelOrdersGoingToHost.Count + " for civManager.IntelOrdersGoingToHost,  ";
                    //    if (civManager.IntelOrdersGoingToHost.Count > 0)
                    //        output = true;
                    //}
                    //if (civManager.IntelOrdersIncomingToHost != null)
                    //{
                    //    _gameLogText += civManager.IntelOrdersIncomingToHost.Count + " for civManager.IntelOrdersIncomingToHost";
                    //    if(civManager.IntelOrdersIncomingToHost.Count > 0)
                    //        output = true;
                    //}
                    ////    // same for civ, not for CivManager
                    ////    if (civManager.Civilization.IntelOrdersGoingToHost != null)
                    ////        _gameLogText += "civ.IntelOrdersGoingToHost={1} , " + civManager.Civilization.IntelOrdersIncomingToHost.Count;
                    ////    if (civManager.Civilization.IntelOrdersIncomingToHost != null)
                    ////        _gameLogText += "civ.IntelOrdersIncomingToHost={1} , " + civManager.Civilization.IntelOrdersIncomingToHost.Count;

                    //if (output == true)
                    //GameLog.Core.Test.DebugFormat(_gameLogText);

                    //}


                    return gameContext;
                }

                if (_isInDesignMode.Value)
                {
                    gameContext = CreateDesignTimeGameContext();
                }

                return gameContext;
            }
        }

        private static GameContext CreateDesignTimeGameContext()
        {
            GameContext gameContext = Create(
                new GameOptions
                {
                    GalaxySize = GalaxySize.Tiny,
                    GalaxyShape = GalaxyShape.Irregular,
                    GalaxyCanon = GalaxyCanon.Canon,
                    StartingTechLevel = StartingTechLevel.Developed,

                    FederationPlayable = EmpirePlayable.Yes,
                    RomulanPlayable = EmpirePlayable.Yes,
                    KlingonPlayable = EmpirePlayable.Yes,
                    CardassianPlayable = EmpirePlayable.Yes,
                    DominionPlayable = EmpirePlayable.Yes,
                    BorgPlayable = EmpirePlayable.No,
                    TerranEmpirePlayable = EmpirePlayable.No,


                    FederationModifier = EmpireModifier.Standard,
                    RomulanModifier = EmpireModifier.Standard,
                    KlingonModifier = EmpireModifier.Standard,
                    CardassianModifier = EmpireModifier.Standard,
                    DominionModifier = EmpireModifier.Standard,
                    BorgModifier = EmpireModifier.Standard,
                    TerranEmpireModifier = EmpireModifier.Standard,

                    EmpireModifierRecurringBalancing = EmpireModifierRecurringBalancing.No,
                    GamePace = GamePace.Normal,
                    TurnTimerEnum = TurnTimerEnum.Unlimited,
                },
                false);

            _stack.Push(gameContext);

            gameContext.TurnNumber = 1;
            Civilization civ = gameContext.Civilizations["FEDERATION"] ?? gameContext.Civilizations.FirstOrDefault(o => o.IsEmpire);
            GameLog.Client.GameData.DebugFormat("civ={0}, type={1}", civ.Name, civ.CivilizationType);

            CivilizationManager civManager = gameContext.CivilizationManagers[civ];
            Colony homeColony = civManager.HomeColony;

            ShipyardDesign shipyardDesign = TechTreeHelper.GetBuildProjects(homeColony).Select(o => o.BuildDesign).OfType<ShipyardDesign>().FirstOrDefault();

            if (shipyardDesign != null)
            {
                if (shipyardDesign.TrySpawn(homeColony.Location, civ, out TechObject spawnedInstance))
                {
                    Shipyard shipyard = homeColony.Shipyard;
                    IList<BuildProject> shipBuildProjects = TechTreeHelper.GetShipyardBuildProjects(shipyard);

                    for (int i = 0; i <= shipyard.BuildSlots.Count && shipBuildProjects.Count != 0; i++)
                    {
                        GameLog.Core.ShipProduction.DebugFormat("shipBuildProjects[0].Description = {0}", shipBuildProjects[0].Description);
                        shipyard.BuildQueue.Add(new BuildQueueItem(shipBuildProjects[0]));
                        shipBuildProjects.RemoveAt(0);
                    }

                    shipyard.ProcessQueue();
                }
            }

            BuildingDesign windTurbines = gameContext.TechDatabase["WIND_TURBINES"] as BuildingDesign;
            if (windTurbines != null)
            {
                _ = windTurbines.TrySpawn(homeColony.Location, homeColony.Owner, out TechObject spawnedInstance);
            }

            BuildingDesign chargeCollectors = gameContext.TechDatabase["CHARGE_COLLECTORS"] as BuildingDesign;
            if (chargeCollectors != null)
            {
                _ = chargeCollectors.TrySpawn(homeColony.Location, homeColony.Owner, out TechObject spawnedInstance);
            }

            OrbitalBatteryDesign batteryDesign = gameContext.TechDatabase["FED_ORBITAL_BATTERY_I"] as OrbitalBatteryDesign;
            if (batteryDesign != null)
            {
                homeColony.OrbitalBatteryDesign = batteryDesign;
                homeColony.AddOrbitalBatteries(5);

                while (homeColony.DeactivateFacility(ProductionCategory.Industry))
                {
                    if (!homeColony.ActivateFacility(ProductionCategory.Energy))
                    {
                        break;
                    }
                }

                while (homeColony.ActivateOrbitalBattery())
                {
                    continue;
                }
            }

            IEnumerable<BuildProject> buildProjects = TechTreeHelper.GetBuildProjects(homeColony).Take(3);

            foreach (BuildProject buildProject in buildProjects)
            {
                homeColony.BuildQueue.Add(new BuildQueueItem(buildProject));
            }

            homeColony.ProcessQueue();

            _ = gameContext._diplomacyData.GetValuesForOwner(civ).ForEach(
                o =>
                {
                    Civilization counterparty = gameContext.Civilizations[o.CounterpartyID];

                    DiplomacyHelper.EnsureContact(
                        civ,
                        counterparty,
                        gameContext.CivilizationManagers[o.CounterpartyID].HomeColony.Location);
                });

            return gameContext;
        }

        public static GameContext ThreadContext
        {
            get
            {
                if (ThreadStack.TryPeek(out GameContext context))
                {
                    return context;
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a new instance using the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="isMultiplayerGame">Specifies if the game is multiplayer.</param>
        /// <returns>The new instance.</returns>
        public static GameContext Create(GameOptions options, bool isMultiplayerGame)
        {
            try
            {
                return new GameContext(options, isMultiplayerGame);
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error("Problem while creating a new game context", e);
                return new GameContext(options, isMultiplayerGame);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GameContext"/> class
        /// using the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        private GameContext(GameOptions options, bool isMultiplayerGame)
        {
            _options = options;
            IsMultiplayerGame = isMultiplayerGame;
            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            PushThreadContext(this);
            try
            {
                _gameMod = GameModLoader.GetModFromCommandLine();
                _races = RaceDatabase.Load();
                GameLog.Client.GameData.DebugFormat("Races loaded");
                _civilizations = CivDatabase.Load();
                GameLog.Client.GameData.DebugFormat("Civilizations loaded");
                _civManagers = new CivilizationManagerMap();
                _tables = GameTables.Load();
                GameLog.Client.GameData.DebugFormat("Tables loaded");
                _techDatabase = TechDatabase.Load();
                GameLog.Client.GameData.DebugFormat("TechDatabase loaded");
                _researchMatrix = ResearchMatrix.Load();
                GameLog.Client.GameData.DebugFormat("ResearchMatrix loaded");
                //_intelMatrix = IntelMatrix.Load();
                //GameLog.Client.GameData.DebugFormat("IntelMatrix loaded");
                _techTrees = new TechTreeMap();
                GameLog.Client.GameData.DebugFormat("TechTree loaded");
                _strategyDatabase = StrategyDatabase.Load();
                _scriptedEvents = new List<ScriptedEvent>();
                _diplomacyDatabase = DiplomacyDatabase.Load();
                _agreementMatrix = new AgreementMatrix();

                ScriptedEventDatabase scriptedEventDatabase = ScriptedEventDatabase.Load();

                string _eventOptionsGameLogText = "";
                string _scriptedEventGameLogText = "";

                foreach (EventDefinition eventDefinition in scriptedEventDatabase)
                {

                    string eventId = eventDefinition.EventID;
                    Type eventType = eventDefinition.EventType;

                    if (string.IsNullOrWhiteSpace(eventId) || eventType == null)
                    {
                        continue;
                    }

                    try
                    {
                        ScriptedEvent scriptedEvent = (ScriptedEvent)Activator.CreateInstance(eventType);

                        scriptedEvent.Initialize(eventId, eventDefinition.Options);

                        _scriptedEvents.Add(scriptedEvent);

                        foreach (KeyValuePair<string, object> _eventOption in eventDefinition.Options)
                        {
                            _eventOptionsGameLogText += _eventOption.Key + "=" + _eventOption.Value + ",";
                        }
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("MinTurnsBetweenExecutions", "TurnDist.");
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("CivilizationRecurrencePeriod", "CivRecur.");
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("UnitRecurrencePeriod", "Unit-Recur.");
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("OccurrenceChance", "Occur.");

                        _scriptedEventGameLogText = scriptedEvent.GetType().ToString();
                        _scriptedEventGameLogText = _scriptedEventGameLogText.Replace("Supremacy.Scripting.Events.", "");

                        GameLog.Client.EventsDetails.InfoFormat("Scripted Event loaded - Options from file: " + _eventOptionsGameLogText + " for {0}", _scriptedEventGameLogText);
                        _eventOptionsGameLogText = "";
                        _scriptedEventGameLogText = "";
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(
                            string.Format(
                                "Error initializing scripted event \"{0}\".",
                                eventDefinition.Description),
                            e);
                    }
                }

                GalaxyGenerator.GenerateGalaxy(this);
                GameLog.Client.GameData.DebugFormat("Galaxy generated...");

                TechTree.LoadTechTrees(this);

                // Prep up the settings for initial homeworlds
                HomeSystemsDatabase homeSystemDatabase = HomeSystemsDatabase.Load();

                foreach (CivilizationManager civManager in _civManagers)
                {
                    foreach (Colony colony in civManager.Colonies)
                    {
                        // get the home system settings
                        Civilization civ = colony.Owner;

                        StarSystemDescriptor homeSystemDescriptor = homeSystemDatabase.ContainsKey(civ.Key)
                                                       ? homeSystemDatabase[civ.Key]
                                                       : GalaxyGenerator.GenerateHomeSystem(civ);

                        // adjust starting population
                        if (homeSystemDescriptor.PopulationRatio != -1.0f)
                        {
                            colony.Population.CurrentValue = (int)(colony.Population.Maximum * homeSystemDescriptor.PopulationRatio);
                        }

                        // adjust starting credits
                        if (homeSystemDescriptor.Credits != -1.0f)
                        {
                            civManager.Credits.CurrentValue = (int)homeSystemDescriptor.Credits;
                            civManager.Credits.UpdateAndReset();
                            civManager.Credits.SaveCurrentAndResetToBase();
                        }

                        // adjust starting resources
                        if (homeSystemDescriptor.Deuterium != -1.0f)
                        {
                            civManager.Resources.Deuterium.CurrentValue = (int)homeSystemDescriptor.Deuterium;
                            civManager.Resources.Deuterium.UpdateAndReset();
                            civManager.Resources.Deuterium.SaveCurrentAndResetToBase();
                        }

                        if (homeSystemDescriptor.Dilithium != -1.0f)
                        {
                            civManager.Resources.Dilithium.CurrentValue = (int)homeSystemDescriptor.Dilithium;
                            civManager.Resources.Dilithium.UpdateAndReset();
                            civManager.Resources.Dilithium.SaveCurrentAndResetToBase();
                        }

                        if (homeSystemDescriptor.Duranium != -1.0f)
                        {
                            civManager.Resources.Duranium.CurrentValue = (int)homeSystemDescriptor.Duranium;
                            civManager.Resources.Duranium.UpdateAndReset();
                            civManager.Resources.Duranium.SaveCurrentAndResetToBase();
                        }

                        if (homeSystemDescriptor.Food != -1.0f)
                        {
                            colony.FoodReserves.CurrentValue = (int)homeSystemDescriptor.Food;
                            colony.FoodReserves.UpdateAndReset();
                            colony.FoodReserves.SaveCurrentAndResetToBase();
                        }

                        if (homeSystemDescriptor.Morale != -1.0f)
                        {
                            colony.Morale.CurrentValue = (int)homeSystemDescriptor.Morale;
                            colony.Morale.UpdateAndReset();
                            colony.Morale.SaveCurrentAndResetToBase();
                        }

                        ColonyBuilder.Build(colony);
                        _ = civManager.TotalPopulation.AdjustCurrent(colony.Population.CurrentValue);

                        // readjust production facilities if needed
                        if (homeSystemDescriptor.FoodPF != null)
                        {
                            TechDatabase db = Current.TechDatabase;

                            //foreach (var item in db)
                            //{
                            //    //GameLog.Client.GameData.DebugFormat("item = {0}", item.Key);
                            //}


                            ProductionFacilityDesign foodFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.FoodPF.DesignType]];

                            if (foodFacility != null)
                            {
                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Food, colony.GetTotalFacilities(ProductionCategory.Food));
                                colony.SetFacilityType(ProductionCategory.Food, null);

                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Food, foodFacility);

                                int pop = colony.Population.CurrentValue;
                                float growth = colony.System.GetGrowthRate(colony.Inhabitants);
                                if (pop == colony.MaxPopulation)
                                {
                                    growth = 0.0f;
                                }

                                int foodNeeded = (int)(pop * (1 + (3 * growth)));
                                /* should take into account planetary food bonuses */
                                int facilitiesRequired = foodNeeded / (foodFacility.UnitOutput + 1) + 1;

                                if (homeSystemDescriptor.FoodPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.FoodPF.Count;
                                }

                                colony.AddFacilities(ProductionCategory.Food, facilitiesRequired);

                                if (homeSystemDescriptor.FoodPF.Active != -1.0f)
                                {
                                    facilitiesRequired = Math.Min((int)homeSystemDescriptor.FoodPF.Active, colony.GetTotalFacilities(ProductionCategory.Food));
                                }

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.ActivateFacility(ProductionCategory.Food);
                                }
                            }
                        }

                        if (homeSystemDescriptor.IndustryPF != null)
                        {
                            TechDatabase db = Current.TechDatabase;
                            ProductionFacilityDesign industryFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.IndustryPF.DesignType]];
                            if (industryFacility != null)
                            {
                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Industry, colony.GetTotalFacilities(ProductionCategory.Industry));
                                colony.SetFacilityType(ProductionCategory.Industry, null);

                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Industry, industryFacility);

                                int facilitiesRequired = (colony.Population.CurrentValue / industryFacility.LaborCost) + 1;

                                if (homeSystemDescriptor.IndustryPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.IndustryPF.Count;
                                }

                                colony.AddFacilities(ProductionCategory.Industry, facilitiesRequired);

                                if (homeSystemDescriptor.IndustryPF.Active != -1.0f)
                                {
                                    facilitiesRequired = Math.Min((int)homeSystemDescriptor.IndustryPF.Active, colony.GetTotalFacilities(ProductionCategory.Industry));
                                }

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.ActivateFacility(ProductionCategory.Industry);
                                }
                            }
                        }

                        if (homeSystemDescriptor.EnergyPF != null)
                        {
                            TechDatabase db = Current.TechDatabase;
                            ProductionFacilityDesign energyFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.EnergyPF.DesignType]];
                            if (energyFacility != null)
                            {
                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Energy, colony.GetTotalFacilities(ProductionCategory.Energy));
                                colony.SetFacilityType(ProductionCategory.Energy, null);

                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Energy, energyFacility);

                                int facilitiesRequired = (colony.Population.CurrentValue / energyFacility.LaborCost) + 1;

                                if (homeSystemDescriptor.EnergyPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.EnergyPF.Count;
                                }

                                colony.AddFacilities(ProductionCategory.Energy, facilitiesRequired);

                                if (homeSystemDescriptor.EnergyPF.Active != -1.0f)
                                {
                                    facilitiesRequired = Math.Min((int)homeSystemDescriptor.EnergyPF.Active, colony.GetTotalFacilities(ProductionCategory.Energy));
                                }

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.ActivateFacility(ProductionCategory.Energy);
                                }
                            }
                        }

                        if (homeSystemDescriptor.ResearchPF != null)
                        {
                            TechDatabase db = Current.TechDatabase;
                            ProductionFacilityDesign researchFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.ResearchPF.DesignType]];
                            if (researchFacility != null)
                            {
                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Research, colony.GetTotalFacilities(ProductionCategory.Research));
                                colony.SetFacilityType(ProductionCategory.Research, null);

                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Research, researchFacility);

                                int facilitiesRequired = (colony.Population.CurrentValue / researchFacility.LaborCost) + 1;

                                if (homeSystemDescriptor.ResearchPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.ResearchPF.Count;
                                }

                                colony.AddFacilities(ProductionCategory.Research, facilitiesRequired);

                                if (homeSystemDescriptor.ResearchPF.Active != -1.0f)
                                {
                                    facilitiesRequired = Math.Min((int)homeSystemDescriptor.ResearchPF.Active, colony.GetTotalFacilities(ProductionCategory.Research));
                                }

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.ActivateFacility(ProductionCategory.Research);
                                }
                            }
                        }

                        if (homeSystemDescriptor.IntelligencePF != null)
                        {
                            TechDatabase db = Current.TechDatabase;
                            ProductionFacilityDesign intelligenceFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.IntelligencePF.DesignType]];
                            if (intelligenceFacility != null)
                            {
                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Intelligence, colony.GetTotalFacilities(ProductionCategory.Intelligence));
                                colony.SetFacilityType(ProductionCategory.Intelligence, null);

                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Intelligence, intelligenceFacility);

                                int facilitiesRequired = (colony.Population.CurrentValue / intelligenceFacility.LaborCost) + 1;

                                if (homeSystemDescriptor.IntelligencePF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.IntelligencePF.Count;
                                }

                                colony.AddFacilities(ProductionCategory.Intelligence, facilitiesRequired);

                                if (homeSystemDescriptor.IntelligencePF.Active != -1.0f)
                                {
                                    facilitiesRequired = Math.Min((int)homeSystemDescriptor.IntelligencePF.Active, colony.GetTotalFacilities(ProductionCategory.Intelligence));
                                }

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.ActivateFacility(ProductionCategory.Intelligence);
                                }
                            }
                        }

                        // Spawn starting objects

                        // Starting Building, process first so that energy providing buildings can be spawned to feed other buildings and shipyards
                        foreach (string building in homeSystemDescriptor.StartingBuildings)
                        {
                            if (Current.TechDatabase.DesignIdMap.ContainsKey(building))
                            {
                                int buildingDesign = Current.TechDatabase.DesignIdMap[building];

                                _ = Current.TechDatabase.BuildingDesigns[buildingDesign].TrySpawn(colony.Location, colony.Owner, out TechObject instance);
                                //GameLog.Client.GameData.DebugFormat("Starting Buildings: buildingDesign={0}, {1}", buildingDesign, building);
                                if (instance != null)
                                {
                                    _ = colony.ActivateBuilding(instance as Building);
                                }
                            }
                        }

                        // Starting Shipyards
                        foreach (string shipyard in homeSystemDescriptor.StartingShipyards)
                        {
                            if (Current.TechDatabase.DesignIdMap.ContainsKey(shipyard))
                            {
                                int shipyardDesign = Current.TechDatabase.DesignIdMap[shipyard];

                                _ = Current.TechDatabase.ShipyardDesigns[shipyardDesign].TrySpawn(colony.Location, colony.Owner, out TechObject instance);
                                //GameLog.Client.GameData.DebugFormat("Starting Shipyards: shipyardDesign={0}, {1}", shipyardDesign, shipyard);
                                if (instance != null)
                                {
                                    Shipyard newShipyard = instance as Shipyard;
                                    foreach (ShipyardBuildSlot buildSlot in newShipyard.BuildSlots)
                                    {
                                        _ = colony.ActivateShipyardBuildSlot(buildSlot);
                                    }
                                }
                            }
                        }

                        // Starting Ships
                        foreach (string ship in homeSystemDescriptor.StartingShips)
                        {
                            if (Current.TechDatabase.DesignIdMap.ContainsKey(ship))
                            {
                                int shipDesign = Current.TechDatabase.DesignIdMap[ship];

                                _ = Current.TechDatabase.ShipDesigns[shipDesign].TrySpawn(colony.Location, colony.Owner, out TechObject instance);
                            }
                        }

                        // Starting Outposts
                        foreach (string outpost in homeSystemDescriptor.StartingOutposts)
                        {
                            if (Current.TechDatabase.DesignIdMap.ContainsKey(outpost))
                            {
                                int outpostDesign = Current.TechDatabase.DesignIdMap[outpost];
                                //GameLog.Client.GameData.DebugFormat("Starting Outposts: outpostDesign={0}, {1}", outpostDesign, outpost);
                                _ = Current.TechDatabase.StationDesigns[outpostDesign].TrySpawn(colony.Location, colony.Owner, out TechObject instance);
                            }
                        }

                        // Orbital Batteries
                        foreach (string OB in homeSystemDescriptor.StartingOrbitalBatteries)
                        {
                            if (Current.TechDatabase.DesignIdMap.ContainsKey(OB))
                            {
                                int OBDesign = Current.TechDatabase.DesignIdMap[OB];

                                _ = Current.TechDatabase.OrbitalBatteryDesigns[OBDesign].TrySpawn(colony.Location, colony.Owner, out TechObject instance);
                                if (instance != null)
                                {
                                    _ = colony.ActivateOrbitalBattery();
                                }
                            }
                        }
                    }
                }
                GameLog.Core.General.InfoFormat("Starting items are done!");
                _sectorClaims = new SectorClaimGrid();
                _diplomats = new CivilizationKeyedMap<Diplomat>(o => o.OwnerID);

                foreach (CivilizationManager civManager in _civManagers)
                {
                    if (civManager.Civilization.CivilizationType != CivilizationType.NotInGameRace)
                    {
                        _diplomats.Add(new Diplomat(civManager.Civilization));
                        //_diplomats.Add(new List<IntelHelper.NewIntelOrders>());
                        civManager.EnsureSeatOfGovernment();
                    }
                }
                GameLog.Core.General.InfoFormat("SeatOfGovernment ensured...");

                _ = _diplomats.ForEach(d => d.EnsureForeignPowers());

                FixupDiplomacyData();

            }
            finally
            {
                _ = PopThreadContext();
            }
        }

        /// <summary>
        /// Update references lost during reserialization.
        /// </summary>
        internal void OnDeserialized()
        {
            bool needsPush = !ReferenceEquals(Current, this);
            if (needsPush)
            {
                PushThreadContext(this);
            }

            try
            {
                _universe.OnDeserialized();
            }
            finally
            {
                if (needsPush)
                {
                    _ = PopThreadContext();
                }
            }
        }
    }
}
