// File:GameContext.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private int _nextObjectId;// = 0; //2025-06-14
        private int _turnnumber = 0;
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

        //[NonSerialized]
        //public string _text;
        //public readonly string _newline = Environment.NewLine;

        //private bool _bool_Fac_Count_Active;
        #endregion Fields

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            string _summary_write_serialized = "";
            string _newline = Environment.NewLine;
            string _text;

            bool _write_serialized = true;
            //bool _write_serialized = false;



            _text = "Step_3701: --------------------------------------------------";
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);

            _text = "Step_3702:; ########### Serialising GameContext...";
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);

            writer.Write(IsMultiplayerGame);
            _text = "Step_3707:; write > IsMultiplayerGame= " + IsMultiplayerGame;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);

            writer.WriteOptimized(_nextObjectId);
            _text = "Step_3712:; write > _nextObjectId= " + IsMultiplayerGame;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteOptimized((ushort)_turnnumber);
            _text = "Step_3713:; write > _turnnumber= " + _turnnumber;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_options);
            _text = "Step_3714:; write > _options= xx";// + IsMultiplayerGame;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_gameMod);
            _text = "Step_3715:; write > _gameMod= " + _gameMod;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_civilizations);
            _text = "Step_3716:; write > _civilizations...";// + IsMultiplayerGame;
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_civManagers);
            _text = "Step_3717:; write > _civManagers...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_races);
            _text = "Step_3718:; write > _races...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_universe);
            _text = "Step_3721:; write > _universe...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_techDatabase);
            _text = "Step_3725:; write > _techDatabase...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_researchMatrix);
            _text = "Step_3727:; write > _researchMatrix...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_sectorClaims);
            _text = "Step_3731:; write > _sectorClaims...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_techTrees);
            _text = "Step_3735:; write > _techTrees...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_diplomacyData);
            _text = "Step_3737:; write > _diplomacyData...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_agreementMatrix);
            _text = "Step_3741:; write > _agreementMatrix...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_diplomats);
            _text = "Step_3745:; write > _diplomats...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_strategyDatabase);
            _text = "Step_3747:; write > _strategyDatabase...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_scriptedEvents);
            _text = "Step_3751:; write > _scriptedEvents...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);


            writer.WriteObject(_diplomacyDatabase);
            _text = "Step_3755:; write > _diplomacyDatabase...";// 
            if (_write_serialized) Console.WriteLine(_text);
            _summary_write_serialized += _newline + _text;
            GameLog.Core.SaveLoad.DebugFormat(_text);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            PushThreadContext(this);
            string _text;

            try
            {
                GameLog.Core.SaveLoad.DebugFormat("Step_3600: --------------------------------------------------");
                _text = "Step_3600:; ########### Deserialising GameContext...";
                Console.WriteLine(_text);
                GameLog.Core.SaveLoad.DebugFormat(_text);

                IsMultiplayerGame = reader.ReadBoolean();
                _text = "Step_3610:; already read  IsMultiplayerGame..... > " + IsMultiplayerGame.ToString();
                Console.WriteLine(_text);
                GameLog.Core.SaveLoad.DebugFormat("Step_3611: IsMultiplayerGame = {0}", IsMultiplayerGame);

                _nextObjectId = reader.ReadOptimizedInt32();
                _text = "Step_3630:; already read  _nextObjectId..... > " + _nextObjectId;
                Console.WriteLine(_text);

                _turnnumber = reader.ReadOptimizedUInt16();
                _text = "Step_3640:; already read _turnnumber..... > " + _turnnumber;
                Console.WriteLine(_text);
                GameLog.Core.SaveLoad.DebugFormat("Step_3641: _turnnumber = {0}", _turnnumber);

                _options = reader.Read<GameOptions>();
                _text = "Step_3660:; already read _options.....";
                Console.WriteLine(_text);

                _gameMod = reader.Read<GameMod>();
                _text = "Step_3680:; already read _gameMod.....";
                Console.WriteLine(_text);

                _civilizations = reader.Read<CivDatabase>();
                _text = "Step_3710:; already read _civilizations..... > " + _civilizations.Count;
                Console.WriteLine(_text);
                // CivDatabase = basic Civs (like ShortName etc)

                _civManagers = reader.Read<CivilizationManagerMap>();
                _text = "Step_3740:; already read _civManagers..... > " + _civManagers.Count;
                Console.WriteLine(_text);
                // _civManagers = basic Civs (like ShortName etc)

                _races = reader.Read<RaceDatabase>();
                _text = "Step_3780:; already read _races..... > " + _races.Count;
                Console.WriteLine(_text);

                Console.WriteLine("Step_4447:; Print of List of systems from saved game is turned off - use ALT+M at Map > \\lib\\_MapData.txt");
                _universe = reader.Read<UniverseManager>();
                _text = "Step_3810:; already read _universe.....";
                Console.WriteLine(_text);
                //GameLog.Core.SaveLoad.DebugFormat(_text);

                _techDatabase = reader.Read<TechDatabase>();
                _text = "Step_3910:; already read _techDatabase.....";
                Console.WriteLine(_text);

                _researchMatrix = reader.Read<ResearchMatrix>();
                _text = "Step_3920:; already read _researchMatrix.....";
                Console.WriteLine(_text);

                _sectorClaims = reader.Read<SectorClaimGrid>();
                _text = "Step_3930:; already read _sectorClaims.....";
                Console.WriteLine(_text);

                _techTrees = reader.Read<TechTreeMap>();
                _text = "Step_3940:; already read _techTrees.....";
                Console.WriteLine(_text);

                _diplomacyData = reader.Read<CivilizationPairedMap<IDiplomacyData>>();
                _text = "Step_3950:; already read _diplomacyData.....";
                Console.WriteLine(_text);

                _agreementMatrix = reader.Read<AgreementMatrix>();
                _text = "Step_3960:; already read _agreementMatrix.....";
                Console.WriteLine(_text);

                _diplomats = reader.Read<CivilizationKeyedMap<Diplomat>>();
                _text = "Step_3970:; already read _diplomats.....";
                Console.WriteLine(_text);

                _strategyDatabase = reader.Read<StrategyDatabase>();
                _text = "Step_3980:; already read _strategyDatabase.....";
                Console.WriteLine(_text);

                _scriptedEvents = reader.Read<ICollection<ScriptedEvent>>();
                _text = "Step_3985:; already read _scriptedEvents.....";
                Console.WriteLine(_text);

                _diplomacyDatabase = reader.Read<DiplomacyDatabase>();
                _text = "Step_3990:; already read _diplomacyDatabase.....";
                Console.WriteLine(_text);

                Report_DiplomacyData();

                FixupDiplomacyData();
                _text = "Step_3995:; already done > FixupDiplomacyData().....";
                Console.WriteLine(_text);
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
            foreach (CivilizationManager _civM_1 in _civManagers)
            {
                Civilization _civ1 = _civM_1.Civilization;
                Diplomat _diplomat = _diplomats[_civ1];

                foreach (CivilizationManager _civM_2 in _civManagers)
                {
                    Civilization _civ2 = _civM_2.Civilization;
                    if (_civ1 == _civ2)
                    {
                        continue;
                    }

                    diplomacyData.Add(_civ1, _civ2, _diplomat.GetData(_civ2));
                }
            }

            //Report_DiplomacyData();



            _diplomacyData = diplomacyData;

            //Report_DiplomacyData();

        }

        public static void Report_DiplomacyData()
        {
            CivilizationPairedMap<IDiplomacyData> diplomacyData = new CivilizationPairedMap<IDiplomacyData>();
            CivilizationKeyedMap<Diplomat> _diplomats = GameContext.Current._diplomats;

            AgreementMatrix agreementMatrix = GameContext.Current.AgreementMatrix;
            var _active_agreements = new List<(int ID_1, int ID_2, string Treaty)>();
            //_active_agreements.Add((0, 999, "x"));


            //string _active_agreements_text = "";
            string agreementText = "";

            foreach (var item in GameContext.Current.AgreementMatrix)
            {
                string _proposal = item.Proposal.Clauses[0].ClauseType.ToString();
                _active_agreements.Add((item.SenderID, item.RecipientID, _proposal));
            }

            foreach (CivilizationManager _civM_1 in GameContext.Current._civManagers)
            {
                Civilization _civ1 = _civM_1.Civilization;
                Diplomat _diplomat = _diplomats[_civ1];

                foreach (CivilizationManager _civM_2 in GameContext.Current._civManagers)
                {
                    Civilization _civ2 = _civM_2.Civilization;
                    if (_civ1 == _civ2)
                    {
                        continue;
                    }

                    diplomacyData.Add(_civ1, _civ2, _diplomat.GetData(_civ2));

                    //_active_agreements = agreementMatrix[_civ1.CivID, _civ2.CivID];

                }
            }

            string _text_diplomacyData = "Step_1777:; no diplomacyData yet";

            // going through civ managers better reflects which civ got spawned
            //foreach (CivilizationManager _civM_1 in GameContext.Current._civManagers)
            //{
            //    Civilization _civ1 = _civM_1.Civilization;
            //    Diplomat _diplomat = _diplomats[_civ1];

            //foreach (CivilizationManager _civM_2 in GameContext.Current._civManagers)
            //{
            foreach (var item in diplomacyData)
            {
                agreementText = "";
                agreementText = string.Join("", _active_agreements
                    .Where(x => x.ID_1 == item.OwnerID)
                    .Select(x => x.Treaty)
                    .ToList());

                agreementText += string.Join("", _active_agreements
                        .Where(x => x.ID_2 == item.OwnerID)
                        .Select(x => x.Treaty)
                        .ToList());


                if (item.Status != ForeignPowerStatus.NoContact)
                {
                    // works but we want to have the agreementmatrix to find out the active treaties
                    //Diplomat _diplomat = _diplomats[item.CounterpartyID];
                    //Civilization _civ1 = GameContext.Current.CivilizationManagers[item.OwnerID].Civilization;
                    //ForeignPower foreignPower = _diplomat.GetForeignPower(_civ1);

                    //_agreement_text = _active_agreements.Where(_active_agreements.TryFindFirstItem == item.OwnerID).tolist();

                    agreementText = agreementText.Replace("TreatyOpenBordersTreatyOpenBordersTreatyOpenBorders",
                        "TreatyOpenBorders");
                    agreementText = agreementText.Replace("TreatyOpenBordersTreatyOpenBorders",
                        "TreatyOpenBorders");

                    Console.WriteLine("Step_1774:; " + agreementText);


                    var _sb = new StringBuilder();
                    _sb.Append("Step_1777:; ");
                    //_sb.Append(" for ");

                    _sb.Append(GameEngine.Do_x_String(15, GameContext.Current.CivilizationManagers[item.OwnerID].Civilization.ToString()));
                    //_sb.Append("= ");


                    _sb.Append(" vs  ");
                    //_foreignPower.CounterpartyDiplomacyData.Status;
                    //ForeignPowerStatus.coun

                    _sb.Append(GameEngine.Do_x_String(15, GameContext.Current.CivilizationManagers[item.CounterpartyID].Civilization.ToString()));
                    _sb.Append(" > ");
                    _sb.Append(GameEngine.Do_x_String(20, item.Status.ToString()));
                    _sb.Append(" ");
                    _sb.Append(GameEngine.Do_x_String(25, agreementText));
                    _sb.Append(" > R= ");
                    _sb.Append(GameEngine.Do_x_Digit_String(4, item.Regard.ToString()));
                    _sb.Append(" > T= ");
                    _sb.Append(GameEngine.Do_x_Digit_String(4, item.Trust.ToString()));

                    _sb.Append(" > FirePowerSpace: ");
                    _sb.Append(GameEngine.Do_x_Digit_String(5, 
                        GameContext.Current.CivilizationManagers[item.OwnerID].FirePowerSpace.ToString()));
                    _sb.Append(" vs ");
                    _sb.Append(GameEngine.Do_x_Digit_String(5, 
                        GameContext.Current.CivilizationManagers[item.CounterpartyID].FirePowerSpace.ToString()));
                    //_sb.Append(" > ContactDuration= ");
                    //_sb.Append(GameEngine.Do_x_Digit_String(3, item.ContactDuration.ToString()));
                    //_sb.Append(" > LastStatusChange= ");
                    //_sb.Append(GameEngine.Do_x_Digit_String(3, item.TurnsSinceLastStatusChange.ToString()));

                    _sb.Append(Environment.NewLine);

                    _text_diplomacyData += _sb.ToString();// + "/r/n";
                                                          //Console.WriteLine(_sb.ToString());
                                                          //    }
                                                          //}
                }
            }
            Console.WriteLine(DateTime.Now + Environment.NewLine + _text_diplomacyData);

            string _path_Resources_Data_Addon = ResourceManager.GetResourcePath(".\\Resources\\Data\\Addon"); // "_diplomacyData.txt"
            string _file = Path.Combine(_path_Resources_Data_Addon, "_diplomacyData.txt"); // by ALT+M at GalaxyMap
            if (!string.IsNullOrEmpty(_file))
            {
                StreamWriter streamWriter = new StreamWriter(_file);
                streamWriter.WriteLine(_text_diplomacyData);
                streamWriter.Close();
                _text_diplomacyData = "Step_1778:; output of _diplomacyData.txt done to " + _file;
                //if (writeDirectly)
                Console.WriteLine(_text_diplomacyData);
            }

        }


        public bool IsMultiplayerGame { get; internal set; }

        public void LoadStrings([NotNull] ITextDatabase textDatabase)
        {
            if (textDatabase == null)
            {
                throw new ArgumentNullException("textDatabase");
            }
            string _text = "";

            ITextDatabaseTable<ITechObjectTextDatabaseEntry> techObjectTable = textDatabase.GetTable<ITechObjectTextDatabaseEntry>(); //Does this every get any data?????

            // outdated .. maybe somewhere else >> _text = "Step_0932:; TextDatabase ..next > Exception thrown: 'System.Xml.XmlException' in System.Xml.dll but it works";
            Console.WriteLine(_text);

            foreach (TechObjectDesign design in _techDatabase)
            {
                _text = "Step_0933:; TextDatabase Key= " + design.Key
                    + ", Name= " + design.Name
                    + ", Description= " + design.Description
                    ;
                //Console.WriteLine(_text);
                ///GameLog.Client.GameInitData.DebugFormat("THE design Key ={0}; Name ={1}; Description ={2}", design.Key, design.Name, design.Description);
                // This is Orbital Batteries Only!!! 


                // Exception thrown: 'System.Xml.XmlException' in System.Xml.dll but it works
                //_text = "Step_0933:; TextDatabase ..next > Exception thrown: 'System.Xml.XmlException' in System.Xml.dll but it works";
                //Console.WriteLine(_text);

                if (LocalizedTextDatabase.Instance.Groups.TryGetValue(new TechObjectTextGroupKey(design.Key), out LocalizedTextGroup localizedText))
                {
                    //GameLog.Client.GameInitData.DebugFormat("###### textDatabase localizedTest = {0} {1} {2} {3} {4}",
                    //    localizedText.DefaultEntry, localizedText.DefaultLocalText, localizedText.Entries, localizedText.Key, design.Key );
                    design.LocalizedText = localizedText;
                    continue;
                }

                // this populates 'entry'
                if (!techObjectTable.TryGetEntry(design.Key, out ITextDatabaseEntry<ITechObjectTextDatabaseEntry> entry))
                {
                    continue;
                }

                // tries to find a Local, maybe German one
                design.TextDatabaseEntry = entry.GetLocalizedEntry(ResourceManager.CurrentLocale);
                //_text = "Step_0934:; TextDatabase Key = " + design.Key
                //        + ", Name= " + design.Name
                //        + ", Description= " + design.Description
                //        ;
                //Console.WriteLine(_text);
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
            string _text = "Step_4001:; " + DateTime.Now + " ------------------------------ BEGIN OF TURN " + TurnNumber + " ------------------------------";
            Console.WriteLine(_text);
            GameLog.Client.General.InfoFormat(_text);
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
            get => _turnnumber;
            set
            {
                if (Equals(_turnnumber, value))
                {
                    return;
                }

                _turnnumber = value;
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
        //private string _text;


        [ThreadStatic]
        private static Stack<GameContext> _threadStack;


        private static Stack<GameContext> ThreadStack  // not worth to monitor ... just creating a new one if necessary
        {
            get
            {
                if (_threadStack == null)// > not, otherwise Stack Overflow || _threadStack.Count == 0)
                {
                    _threadStack = new Stack<GameContext>();
                }

                //if (_threadStack.Count > 1)
                //{
                //    Debugger.Break();
                //}

                return _threadStack;
            }
        }

        /// <summary>
        /// Pushes the specified context onto the stack for the current thread only.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void PushThreadContext(GameContext context)
        {
            // to often !
            //Console.WriteLine("Step_0567:; " + DateTime.Now + " > PushThreadContext(GameContext context) !  "
            //    //+ ", _civM= " + context.CivilizationManagers.Count
            //    );
            if (context == null)
            {
                Debugger.Break();
            }
            ThreadStack.Push(context); // PushThreadContext(GameContext context)
        }

        /// <summary>
        /// Pops a context off the top of the thread-specific stack.
        /// </summary>
        /// <returns>The popped context, or <c>null</c> if the stack is empty.</returns>
        public static GameContext PopThreadContext()
        {
            string _text = "";
            if (!ThreadStack.TryPop(out GameContext result))
            {
                _text = "Step_0568:; " + DateTime.Now + " > PopThreadContext > GameContext: "
                    + "result.CivilizationManagers.Count=" + result.CivilizationManagers.Count
                    ;
                Console.WriteLine(_text);
                //Console.WriteLine("Step_0568:; " + DateTime.Now + " ####### PopThreadContext(GameContext context) !!!!  No Context = no game running anymore " );
                return result;
            }

            int _count = -1;
            //try
            //{
            if (result != null && result.CivilizationManagers != null)
            {
                _count = result.CivilizationManagers.Count;
            }

            //} catch { }

            //_text = "Step_0569:; " + DateTime.Now + " > GameContext: "
            //        + "result.CivilizationManagers.Count=" + _count
            //        ;
            //Console.WriteLine(_text);


            //Console.WriteLine("Step_0568:; " + DateTime.Now + " ####### PopThreadContext(GameContext context) !!!!  No Context = no game running anymore ");
            //Debugger.Break();

            if (result == null)
            {
                return null;
            }
            else
            {
                return result;
            }


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

                    //EmpireModifierRecurringBalancing = EmpireModifierRecurringBalancing.No,
                    EmpireModifierRecurringBalancing = EmpireModifierRecurringBalancing.Run,
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

                while (homeColony.Facility_Deactivate(ProductionCategory.Industry))
                {
                    if (!homeColony.Facility_Activate(ProductionCategory.Energy))
                    {
                        break;
                    }
                }

                while (homeColony.OrbitalBattery_Activate())
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

                bool _bool_Step_0879_FAILED = false;
                if (ThreadStack.Count > 0 && ThreadStack.TryPeek(out GameContext context))
                {
                    return context;
                }

                if (_bool_Step_0879_FAILED == false)
                {
                    //string _text = "Step_0879: No ThreadContext = no GameContext anymore ";
                    //Console.WriteLine(_text);
                    ////GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                    //_bool_Step_0879_FAILED = true;
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
                string _text = "Err_0001:; Problem while creating a new game context" + e;
                Console.WriteLine(_text);
                GameLog.Core.General.Error("Err_0001:; Problem while creating a new game context", e);

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
            string _text = "Step_3003:; " + DateTime.Now + " > GameContext Initialize...";
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);

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
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("UnitRecurrencePeriod", " Unit-Recur.");
                        _eventOptionsGameLogText = _eventOptionsGameLogText.Replace("OccurrenceChance", " Occur.");

                        _scriptedEventGameLogText = scriptedEvent.GetType().ToString();
                        _scriptedEventGameLogText = _scriptedEventGameLogText.Replace("Supremacy.Scripting.Events.", "");

                        _text = "Step_1334:; Scripted Event loaded - Options from file: "
                            + _eventOptionsGameLogText + " for " + _scriptedEventGameLogText;
                        Console.WriteLine(_text);
                        GameLog.Client.Events.InfoFormat(_text);

                        _eventOptionsGameLogText = "";
                        _scriptedEventGameLogText = "";
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(
                            string.Format(
                                "Step_1335:; Error initializing scripted event \"{0}\".",
                                eventDefinition.Description),
                            e);
                    }
                }

                _text = "Step_1287:; NEXT:GenerateGalaxy...";
                Console.WriteLine(_text);

                GalaxyGenerator.GenerateGalaxy(this);

                _text = "Step_1288:; " + DateTime.Now + " > Galaxy generated...";
                Console.WriteLine(_text);
                //GameLog.Core.GalaxyGeneratorDetails.DebugFormat(_text);

                TechTree.LoadTechTrees(this);

                // Prep up the settings for initial homeworlds
                HomeSystemsDatabase homeSystemDatabase = HomeSystemsDatabase.Load();

                bool _bool_Fac_Count_Active = false;
                //bool _bool_Fac_Count_Active = true;

                foreach (CivilizationManager civManager in _civManagers)
                {
                    foreach (Colony colony in civManager.Colonies)
                    {
                        //_text = "Generating HomeSystems... > " + colony.Name;
                        //Console.WriteLine(_text);
                        ////GameLog.Core.GalaxyGeneratorDetails.DebugFormat(_text);

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
                        int _laborAvailable = colony.Population.CurrentValue / 10;

                        int _more_by_start_level = 0;
                        switch (this.Options.StartingTechLevel)
                        {
                            case StartingTechLevel.Early:
                                break;
                            case StartingTechLevel.Developed:
                                _more_by_start_level = 2;
                                break;
                            case StartingTechLevel.Sophisticated:
                                _more_by_start_level = 3;
                                break;
                            case StartingTechLevel.Advanced:
                                _more_by_start_level = 4;
                                break;
                            case StartingTechLevel.Supreme:
                                _more_by_start_level = 5;
                                break;
                        }


                        //_text = "Adjusting facilities if necessary...";
                        //Console.WriteLine(_text);

                        bool _use_value_from_XML = true;

                        if (!colony.Owner.IsEmpire)
                        {
                            _use_value_from_XML = false;
                            _text = "OFFLINE > _use_value_from_XML = false;";
                        }


                        //_bool_Fac_Count_Active = false;

                        if (_bool_Fac_Count_Active == false)
                        {
                            _text = "Step_1312:; ####### From HomeSystems.xml > Facilities (Count/Active) is ignored...";
                            Console.WriteLine(_text);
                            GameLog.Client.GalaxyGenerator.InfoFormat(_text);
                            _bool_Fac_Count_Active = true; // just do once
                        }

                        int facilitiesRequired = 0;
                        int _additional_facilities = 0;
                        // readjust production facilities if needed
                        if (homeSystemDescriptor.FoodPF != null)
                        {
                            TechDatabase db = Current.TechDatabase;

                            ProductionFacilityDesign foodFacility = db.ProductionFacilityDesigns[db.DesignIdMap[homeSystemDescriptor.FoodPF.DesignType]];



                            if (foodFacility != null)
                            {


                                // Start by clearing already existing facilities
                                colony.RemoveFacilities(ProductionCategory.Food, colony.GetTotalFacilities(ProductionCategory.Food));
                                colony.SetFacilityType(ProductionCategory.Food, null);


                                // have a look why some minors (low populated) generate 255 facilities
                                // Create new one
                                colony.SetFacilityType(ProductionCategory.Food, foodFacility);

                                int pop = colony.Population.CurrentValue;
                                float growth = colony.System.GetGrowthRate(colony.Inhabitants);
                                if (pop == colony.Population_Max)
                                {
                                    growth = 0.0f;
                                }

                                int foodNeeded = (int)(pop * (1 + (3 * growth)));
                                /* should take into account planetary food bonuses */
                                facilitiesRequired = foodNeeded / (foodFacility.UnitOutput + 1);

                                if (_use_value_from_XML && homeSystemDescriptor.FoodPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.FoodPF.Count;
                                }
                                else
                                {
                                    _additional_facilities = 2;
                                }

                                if (facilitiesRequired > 19)
                                {
                                    _text = "Step_1312:; ####### From HomeSystems.xml > facilitiesRequired= " + facilitiesRequired;
                                    Console.WriteLine(_text);
                                }




                                colony.AddFacilities(ProductionCategory.Food, facilitiesRequired + _additional_facilities);
                                _additional_facilities = 0;

                                //if (_use_value_from_XML && homeSystemDescriptor.FoodPF.Active != -1.0f)
                                //{
                                //    facilitiesRequired = Math.Min((int)homeSystemDescriptor.FoodPF.Active, colony.GetTotalFacilities(ProductionCategory.Food));
                                //}

                                //colony.AddFacilities(ProductionCategory.Food, facilitiesRequired + 2);

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.Facility_Activate(ProductionCategory.Food);
                                    _laborAvailable -= 1;
                                }
                            }
                        }

                        _text = "EnergyPF";
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

                                //int energyNeeded = colony.GetEnergyUsage();

                                facilitiesRequired = 2;
                                //int _additional_energy = 0;


                                if (_use_value_from_XML && homeSystemDescriptor.EnergyPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.EnergyPF.Count;
                                }
                                else
                                {
                                    _additional_facilities = 4 + _more_by_start_level;
                                }

                                colony.AddFacilities(ProductionCategory.Energy, facilitiesRequired + _additional_facilities);
                                _additional_facilities = 0;

                                //if (_use_value_from_XML && homeSystemDescriptor.EnergyPF.Active != -1.0f)
                                //{
                                //    facilitiesRequired = Math.Min((int)homeSystemDescriptor.EnergyPF.Active, colony.GetTotalFacilities(ProductionCategory.Energy));
                                //}

                                //colony.AddFacilities(ProductionCategory.Energy, facilitiesRequired + _additional_energy);

                                for (int i = 0; i < facilitiesRequired + 2; i++)
                                {
                                    _ = colony.Facility_Activate(ProductionCategory.Energy);
                                    _laborAvailable -= 1;
                                }
                            }
                        }

                        _text = "IndustryPF";
                        //IndustryPF
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

                                facilitiesRequired = _laborAvailable;

                                // facilitiesRequired.Value is reduce each time as well !!
                                if (facilitiesRequired > 4) facilitiesRequired -= 2; // 3 to industry, 1 to research, 1 to intelligence
                                if (facilitiesRequired > 6) facilitiesRequired -= 2; //               +1 to research, +1 to intelligence
                                if (facilitiesRequired > 8) facilitiesRequired -= 2;
                                if (facilitiesRequired > 10) facilitiesRequired -= 2;
                                if (facilitiesRequired > 12) facilitiesRequired -= 2;


                                if (_use_value_from_XML && homeSystemDescriptor.IndustryPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.IndustryPF.Count;
                                }
                                else
                                {
                                    _additional_facilities = 1;
                                }

                                colony.AddFacilities(ProductionCategory.Industry, facilitiesRequired + _additional_facilities);
                                _additional_facilities = 0;

                                //if (_use_value_from_XML && homeSystemDescriptor.IndustryPF.Active != -1.0f)
                                //{
                                //    facilitiesRequired = Math.Min((int)homeSystemDescriptor.IndustryPF.Active, colony.GetTotalFacilities(ProductionCategory.Industry));
                                //}

                                //colony.AddFacilities(ProductionCategory.Industry, facilitiesRequired + 1);

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.Facility_Activate(ProductionCategory.Industry);
                                    _laborAvailable -= 1;
                                }
                            }
                        }

                        _text = "IntelligencePF";
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

                                facilitiesRequired = _laborAvailable / 2;

                                if (_use_value_from_XML && homeSystemDescriptor.IntelligencePF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.IntelligencePF.Count;
                                }
                                else
                                {
                                    _additional_facilities = 0;
                                }

                                colony.AddFacilities(ProductionCategory.Intelligence, facilitiesRequired + _additional_facilities);
                                _additional_facilities = 0;

                                //if (_use_value_from_XML && homeSystemDescriptor.IntelligencePF.Active != -1.0f)
                                //{
                                //    facilitiesRequired = Math.Min((int)homeSystemDescriptor.IntelligencePF.Active, colony.GetTotalFacilities(ProductionCategory.Intelligence));
                                //}

                                //colony.AddFacilities(ProductionCategory.Intelligence, facilitiesRequired);

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.Facility_Activate(ProductionCategory.Intelligence);
                                    _laborAvailable -= 1;
                                }
                            }
                        }

                        _text = "ResearchPF";
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

                                facilitiesRequired = _laborAvailable;

                                if (_use_value_from_XML && homeSystemDescriptor.ResearchPF.Count != -1.0f)
                                {
                                    facilitiesRequired = (int)homeSystemDescriptor.ResearchPF.Count;
                                }
                                else
                                {
                                    _additional_facilities = 0;
                                }

                                colony.AddFacilities(ProductionCategory.Research, facilitiesRequired + _additional_facilities);
                                _additional_facilities = 0;

                                //if (_use_value_from_XML && homeSystemDescriptor.ResearchPF.Active != -1.0f)
                                //{
                                //    facilitiesRequired = Math.Min((int)homeSystemDescriptor.ResearchPF.Active, colony.GetTotalFacilities(ProductionCategory.Research));
                                //}

                                //colony.AddFacilities(ProductionCategory.Research, facilitiesRequired);

                                for (int i = 0; i < facilitiesRequired; i++)
                                {
                                    _ = colony.Facility_Activate(ProductionCategory.Research);
                                    //_laborAvailable -= 1;
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
                                    _ = colony.Building_Activate(instance as Building);
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
                                        _ = colony.ShipyardBuildSlot_Activate(buildSlot);
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
                                    _ = colony.OrbitalBattery_Activate();
                                }
                            }
                        }
                    }
                }
                _text = "Step_4002:; Starting items are done!";
                Console.WriteLine(_text);
                GameLog.Core.General.InfoFormat(_text);

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
                _text = "Step_4502:; SeatOfGovernment ensured...";
                Console.WriteLine(_text);
                GameLog.Core.General.InfoFormat(_text);

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
