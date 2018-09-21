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
using Supremacy.Resources;
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
        private int _localPlayerEmpireID;
        private string _localPlayerName;
        private int[] _empireIDs;
        private string[] _empireNames;
        private string _gameName;
        private GameType _gameType;
        private GameOptions _options;
        private string _saveGameFilename;
        private SlotClaim[] _slotClaims;
        private SlotStatus[] _slotStatus;
        #endregion

        protected GameInitData() {}

        public static GameInitData CreateSinglePlayerGame([NotNull] GameOptions options, int localPlayerEmpireID)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            GameLog.Client.GameData.DebugFormat("GameInitData.cs: CreateSinglePlayerGame: SinglePlayerGameName={0}, localPlayerEmpireID={1}, SinglePlayerName={2}", 
                                                        SinglePlayerGameName, localPlayerEmpireID, SinglePlayerName);

            var initData = new GameInitData
                           {
                               GameName = SinglePlayerGameName,
                               Options = options,
                               GameType = GameType.SinglePlayerNew,
                               LocalPlayerEmpireID = localPlayerEmpireID,
                               LocalPlayerName = SinglePlayerName,
                           };

            initData.PopulateEmpires();

            var empireCount = initData.EmpireIDs.Length;  // does not count Empires turned into ExpandingPower, but we need that amount too. 

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

            return initData;
        }

        public static GameInitData CreateMultiplayerGame([NotNull] GameOptions options, [NotNull] string localPlayerName)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (localPlayerName == null)
                throw new ArgumentNullException("localPlayerName");

            var initData = new GameInitData
                           {
                               Options = options,
                               GameType = GameType.MultiplayerNew,
                               LocalPlayerEmpireID = GameObjectID.InvalidID,
                               LocalPlayerName = localPlayerName,
                           };

            initData.PopulateEmpires();

            var empireCount = initData.EmpireIDs.Length;

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
                throw new ArgumentNullException("savedGameHeader");
            GameLog.Core.General.DebugFormat("CreateFromSavedGame: {0}", savedGameHeader.FileName);

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
        public bool IsMultiplayerGame
        {
            get { return ((GameType == GameType.MultiplayerNew) || (GameType == GameType.MultiplayerLoad)); }
        }

        public int LocalPlayerEmpireID
        {
            get { return _localPlayerEmpireID; }
            set
            {
                _localPlayerEmpireID = value;
                OnPropertyChanged("LocalPlayerEmpireID");
                GameLog.Core.General.DebugFormat("LocalPlayerEmpireID (beginning from 0): {0}", _localPlayerEmpireID);
            }
        }

        public string LocalPlayerName
        {
            get { return _localPlayerName; }
            set
            {
                _localPlayerName = value;
                OnPropertyChanged("LocalPlayerName");
            }
        }

        public int[] EmpireIDs
        {
            get { return _empireIDs; }
            set
            {
                _empireIDs = value;
                OnPropertyChanged("EmpireIDs");
            }
        }

        public string[] EmpireNames
        {
            get { return _empireNames; }
            set
            {
                _empireNames = value;
                OnPropertyChanged("EmpireNames");
                //GameLog.Client.GameData.DebugFormat("GameInitData.cs: _empireNames: {0}", _empireNames);
            }
        }

        public string GameName
        {
            get { return _gameName; }
            set
            {
                _gameName = value;
                OnPropertyChanged("GameName");
            }
        }

        public GameType GameType
        {
            get { return _gameType; }
            set
            {
                _gameType = value;
                OnPropertyChanged("GameType");
                OnPropertyChanged("IsMultiplayerGame");
            }
        }

        public GameOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;
                OnPropertyChanged("Options");
            }
        }

        public string SaveGameFileName
        {
            get { return _saveGameFilename; }
            set
            {
                _saveGameFilename = value;
                OnPropertyChanged("SaveGameFileName");
            }
        }

        public SlotClaim[] SlotClaims
        {
            get { return _slotClaims; }
            set
            {
                _slotClaims = value;
                OnPropertyChanged("SlotClaims");
            }
        }

        public SlotStatus[] SlotStatus
        {
            get { return _slotStatus; }
            set
            {
                _slotStatus = value;
                OnPropertyChanged("SlotStatus");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        [field : NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public and Protected Methods
        protected void PopulateEmpires()
        {
            GameContext.PushThreadContext(new GameContext());

            try
            {
                var empires = CivDatabase.Load().Where(o => o.IsEmpire).Select(o => new { o.Name, o.CivID });

                EmpireIDs = empires.Select(o => (int)o.CivID).ToArray();
                EmpireNames = empires.Select(o => o.Name).ToArray();
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}