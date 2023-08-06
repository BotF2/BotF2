// GameInitData.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Utility;
using System;
using System.ComponentModel;
using System.Linq;

namespace Supremacy.Game
{
    [Serializable]
    public class GameInitData : INotifyPropertyChanged
    {
        protected const string SinglePlayerName = "Player";
        protected const string SinglePlayerGameName = "Single Player Game";

        #region Fields
        public int _localPlayerEmpireID;
        private string _localPlayerName;
        private int[] _empireIDs;
        private string[] _empireNames;
        private string _gameName;
        private GameType _gameType;
        private GameOptions _options;
        private string _saveGameFilename;
        private SlotClaim[] _slotClaims;
        private SlotStatus[] _slotStatus;

        private static string _text;
        private readonly string newline = Environment.NewLine;
        //private int _count;
        #endregion

        protected GameInitData() { }




    public static GameInitData CreateSinglePlayerGame([NotNull] GameOptions options, int localPlayerEmpireID)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _text = "Step_1201: CreateSinglePlayerGame: "
                + "SP-GameName=" + SinglePlayerGameName
                + ", SP-Name=" + SinglePlayerName
                + ", localPlayerEmpireID=" + localPlayerEmpireID
                ;
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);
            //GameLog.Client.GameData.DebugFormat("CreateSinglePlayerGame: SP-GameName={0}, SP-Name={2}, localPlayerEmpireID={1}",
            //                                            SinglePlayerGameName, localPlayerEmpireID, SinglePlayerName);

            GameInitData initData = new GameInitData
            {
                GameName = SinglePlayerGameName,
                Options = options,
                GameType = GameType.SinglePlayerNew,
                LocalPlayerEmpireID = localPlayerEmpireID,
                LocalPlayerName = SinglePlayerName,
            };

            _text = "Step_1202: PopulateEmpires... "
            ;
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);

            initData.PopulateEmpires();

            int empireCount = initData.EmpireIDs.Length;  // does not count Empires turned into ExpandingPower, but we need that amount too. 

            //maybe works now......empireCount = 8; // hardcoded value, depending on defined empires in Civilizations.xaml

            //if (empireCount < 1)
            //    empireCount = 2;

            //GameLog.Client.GameData.DebugFormat("GameInitData.cs: CreateSinglePlayerGame: empireCount={0}", empireCount);

            initData.SlotClaims = new SlotClaim[empireCount];
            initData.SlotStatus = new SlotStatus[empireCount];

            initData.SlotClaims[0] = SlotClaim.Assigned;
            initData.SlotStatus[0] = Game.SlotStatus.Taken;

            for (int i = 1; i < empireCount; i++)
            {
                initData.SlotClaims[i] = SlotClaim.Unassigned;
                initData.SlotStatus[i] = Game.SlotStatus.Open;
            }
            _text = "Step_1203: Returning initData... ";
            Console.WriteLine(_text);
            GameLog.Client.GameData.DebugFormat(_text);
            return initData;
        }

        public static GameInitData CreateMultiplayerGame([NotNull] GameOptions options, [NotNull] string localPlayerName)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (localPlayerName == null)
            {
                throw new ArgumentNullException("localPlayerName");
            }

            GameInitData initData = new GameInitData
            {
                Options = options,
                GameType = GameType.MultiplayerNew,
                LocalPlayerEmpireID = -1,
                LocalPlayerName = localPlayerName,
            };

            initData.PopulateEmpires();

            int empireCount = initData.EmpireIDs.Length;

            initData.SlotClaims = new SlotClaim[empireCount];
            initData.SlotStatus = new SlotStatus[empireCount];

            for (int i = 0; i < empireCount; i++)
            {
                initData.SlotClaims[i] = SlotClaim.Unassigned;
                initData.SlotStatus[i] = Game.SlotStatus.Open;
            }

            GameLog.Client.GameData.DebugFormat("GameInitData.cs: CreateMultiplayerGame: LocalPlayerName={0}, LocalPlayerEmpireID={1}",
                initData.LocalPlayerName, initData.LocalPlayerEmpireID);

            return initData;
        }

        public static GameInitData CreateFromSavedGame([NotNull] SavedGameHeader savedGameHeader)
        {
            if (savedGameHeader == null)
            {
                throw new ArgumentNullException("savedGameHeader");
            }

            DateTime _time = DateTime.Now;
            GameLog.Core.GeneralDetails.DebugFormat("CreateFromSavedGame: {0}", savedGameHeader.FileName);  // GameLog always ... Core.General

            GameLog.Core.General.InfoFormat("Deserialized: savedGameHeader;FileName;{0}", savedGameHeader.FileName);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;LocalPlayerEmpireID;{0}", savedGameHeader.LocalPlayerEmpireID);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;LocalPlayerName;{0}", savedGameHeader.LocalPlayerName);
            foreach (int empire in savedGameHeader.EmpireIDs)
            {
                GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;EmpireNames;{0}", empire);
            }
            foreach (string empireName in savedGameHeader.EmpireNames)
            {
                GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;EmpireNames;{0}", empireName);
            }

            GameLog.Core.General.InfoFormat("Deserialized: savedGameHeader;Options - GalaxySize;{0}", savedGameHeader.Options.GalaxySize);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - GalaxyShape;{0}", savedGameHeader.Options.GalaxyShape);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - StarDensity;{0}", savedGameHeader.Options.StarDensity);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - PlanetDensity;{0}", savedGameHeader.Options.PlanetDensity);
            GameLog.Core.General.InfoFormat("Deserialized: savedGameHeader;Options - StartingTechLevel (once);{0}", savedGameHeader.Options.StartingTechLevel);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - MinorRaceFrequency;{0}", savedGameHeader.Options.MinorRaceFrequency);

            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};FederationPlayable;", savedGameHeader.Options.FederationPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};RomulanPlayable", savedGameHeader.Options.RomulanPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};KlingonPlayable", savedGameHeader.Options.KlingonPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};CardassianPlayable", savedGameHeader.Options.CardassianPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};DominionPlayable", savedGameHeader.Options.DominionPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};BorgPlayable", savedGameHeader.Options.BorgPlayable);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - {0};TerranEmpirePlayable", savedGameHeader.Options.TerranEmpirePlayable);

            GameLog.Core.GeneralDetails.DebugFormat("Options: FederationModifier = {0}", savedGameHeader.Options.FederationModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: RomulanModifier = {0}", savedGameHeader.Options.RomulanModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: KlingonModifier = {0}", savedGameHeader.Options.KlingonModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: CardassianModifier = {0}", savedGameHeader.Options.CardassianModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: DominionModifier = {0}", savedGameHeader.Options.DominionModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: BorgModifier = {0}", savedGameHeader.Options.BorgModifier);
            GameLog.Core.GeneralDetails.DebugFormat("Options: TerranEmpireModifier = {0}", savedGameHeader.Options.TerranEmpireModifier);

            GameLog.Core.GeneralDetails.DebugFormat("Options: EmpireModifierRecurringBalancing = {0}", savedGameHeader.Options.EmpireModifierRecurringBalancing);
            GameLog.Core.GeneralDetails.DebugFormat("Options: GamePace = {0}", savedGameHeader.Options.GamePace);
            GameLog.Core.GeneralDetails.DebugFormat("Options: TurnTimer = {0}", savedGameHeader.Options.TurnTimerEnum);

            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - UseHomeQuadrants;{0}", savedGameHeader.Options.UseHomeQuadrants);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - TurnTimer;{0}", savedGameHeader.Options.TurnTimer);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - CombatTimer;{0}", savedGameHeader.Options.CombatTimer);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - AIMode    ;{0}", savedGameHeader.Options.AIMode);
            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Options - AITakeover;{0}", savedGameHeader.Options.AITakeover);
            // not useful GameLog.Core.General.InfoFormat("Deserialized: savedGameHeader;Options - ModID     ;{0}", savedGameHeader.Options.ModID);

            foreach (SlotClaim slotClaim in savedGameHeader.SlotClaims)
            {
                GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;SlotClaims;{0}", slotClaim);
            }
            foreach (SlotStatus slotStatus in savedGameHeader.SlotStatus)
            {
                GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;SlotClaims;{0}", slotStatus);
            }

            GameLog.Core.GeneralDetails.DebugFormat("Deserialized: savedGameHeader;Single or MultiplayerGame;{0}", savedGameHeader.IsMultiplayerGame ? GameType.MultiplayerLoad : GameType.SinglePlayerLoad);

            GameLog.Client.SaveLoad.DebugFormat("Step_9811: Loading Time = {0}", DateTime.Now - _time);
            Console.WriteLine("Step_9811: Loading Time = {0}", DateTime.Now - _time);



            return new GameInitData
            {
                LocalPlayerEmpireID = savedGameHeader.LocalPlayerEmpireID,
                LocalPlayerName = savedGameHeader.LocalPlayerName,
                EmpireIDs = savedGameHeader.EmpireIDs,
                EmpireNames = savedGameHeader.EmpireNames,
                Options = savedGameHeader.Options,
                SaveGameFileName = savedGameHeader.FileName,
                SlotClaims = savedGameHeader.SlotClaims,
                SlotStatus = savedGameHeader.SlotStatus,
                GameType = savedGameHeader.IsMultiplayerGame ? GameType.MultiplayerLoad : GameType.SinglePlayerLoad
            };

        }

        #region Properties and Indexers
        public bool IsMultiplayerGame => (GameType == GameType.MultiplayerNew) || (GameType == GameType.MultiplayerLoad);

        public int LocalPlayerEmpireID
        {
            get => _localPlayerEmpireID;
            set
            {
                _localPlayerEmpireID = value;
                OnPropertyChanged("LocalPlayerEmpireID");
                GameLog.Core.General.InfoFormat("Step_0287: LocalPlayerEmpireID (beginning from 0): {0}", _localPlayerEmpireID);
                _text += value + ";;LocalPlayerEmpireID;" + newline;
            }
        }

        public string LocalPlayerName
        {
            get => _localPlayerName;
            set
            {
                _localPlayerName = value;
                OnPropertyChanged("LocalPlayerName");
                _text += value + ";;LocalPlayerName;" + newline;
            }
        }

        public int[] EmpireIDs
        {
            get => _empireIDs;
            set
            {
                _empireIDs = value;
                OnPropertyChanged("EmpireIDs");
                //_count = 0;
                //foreach (var item in _empireIDs)
                //{

                //    _loadGameText += value[_count] + ";;EmpireIDs;" +_count + newline;
                //    _count += 1;
                //}
                
            }
        }

        public string[] EmpireNames
        {
            get => _empireNames;
            set
            {
                _empireNames = value;
                OnPropertyChanged("EmpireNames");
                //GameLog.Client.GameData.DebugFormat("GameInitData.cs: _empireNames: {0}", _empireNames);
                // nonsense
                //_count = 0;
                //foreach (var item in _empireNames)
                //{
                //    _loadGameText += value[_count] + ";;EmpireNames;" +_count + newline;
                //    _count += 1;
                //}
                //Console.WriteLine("Step 222: " + _loadGameText);
            }
        }

        public string GameName
        {
            get => _gameName;
            set
            {
                _gameName = value;
                OnPropertyChanged("GameName");
                _text += value + ";;GameName;" + newline;
            }
        }

        public GameType GameType
        {
            get => _gameType;
            set
            {
                _gameType = value;
                OnPropertyChanged("GameType");
                OnPropertyChanged("IsMultiplayerGame");
                _text += value + ";;GameType" + newline;
                _text += value + ";;IsMultiplayerGame" + newline;
            }
        }

        public GameOptions Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged("Options");
                //foreach (var item in value)
                //{
                //    //_loadGameText += item.value + ";;Options:" + item + newline;
                _text = _options.ToString();
                _text += "GameOptions are set...";
                //}
                Console.WriteLine("Step_1200: " + _text);
            }
        }

        public string SaveGameFileName
        {
            get => _saveGameFilename;
            set
            {
                _saveGameFilename = value;
                OnPropertyChanged("SaveGameFileName");
            }
        }

        public SlotClaim[] SlotClaims
        {
            get => _slotClaims;
            set
            {
                _slotClaims = value;
                OnPropertyChanged("SlotClaims");
            }
        }

        public SlotStatus[] SlotStatus
        {
            get => _slotStatus;
            set
            {
                _slotStatus = value;
                OnPropertyChanged("SlotStatus");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public and Protected Methods
        protected void PopulateEmpires()
        {
            GameContext.PushThreadContext(new GameContext());

            try
            {
                var empires = CivDatabase.Load().Where(o => o.IsEmpire).Select(o => new { o.Name, o.CivID });

                EmpireIDs = empires.Select(o => o.CivID).ToArray();
                EmpireNames = empires.Select(o => o.Name).ToArray();
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}