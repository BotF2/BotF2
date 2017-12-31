using System;
using System.ComponentModel;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Client.Context;
using Supremacy.Utility;
//using Supremacy.Client.Audio;

namespace Supremacy.Client.Views
{
    public interface IEmpirePlayerStatusCollection : IIndexedCollection<EmpirePlayerStatus>
    {
        void Update([NotNull] ILobbyData lobbyData);
        void UpdatePlayerReadiness([NotNull] IPlayer player);
        void ClearPlayerReadiness();
        void UpdateRelationshipStatus();
    }

    internal class EmpirePlayerStatusCollection : KeyedCollectionBase<Civilization, EmpirePlayerStatus>, IEmpirePlayerStatusCollection
    {
        public EmpirePlayerStatusCollection()
            : base(o => o.Empire) {}

        public void Update([NotNull] ILobbyData lobbyData)
        {
            if (lobbyData == null)
                throw new ArgumentNullException("lobbyData");

            foreach (var player in lobbyData.Players)
            { 
                this[player.Empire].Player = player;
            GameLog.Print("PlayerID = {0}, Name = {1}, EmpireID = {2}, Empire = {3}", player.PlayerID, player.Name, player.EmpireID, player.Empire );
            
            }
            UpdateRelationshipStatus();
        }

        public void UpdatePlayerReadiness([NotNull] IPlayer player)
        {
           
            if (player == null)
                throw new ArgumentNullException("player");

            this[player.Empire].IsReady = true;
//Plays a "click" sound when turn button is pressed. Sound plays on all machines in Multiplayer.
            //var soundPlayer = new SoundPlayer("Resources/SoundFX/ChatMessage.wav");
            //{
            //    if (File.Exists("Resources/SoundFX/ChatMessage.wav"));
            //    soundPlayer.Play();
            //}  
        }
        
        public void ClearPlayerReadiness()
        {
            foreach (var status in this)
                status.IsReady = false;
        }

        public void UpdateRelationshipStatus()
        {
            this.ForEach(o => o.UpdateRelationshipStatus());
        }
    }

    public class EmpirePlayerStatus : INotifyPropertyChanged
    {
        private readonly IAppContext _appContext;
        private readonly int _empireId;
        private int _playerId = Game.Player.ComputerPlayerID;

        public EmpirePlayerStatus([NotNull] IAppContext appContext, [NotNull] Civilization empire)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");
            if (empire == null)
                throw new ArgumentNullException("empire");
            if (!empire.IsEmpire)
                throw new ArgumentException(@"Civilization must be an empire.", "empire");

            _appContext = appContext;
            _empireId = empire.CivID;
        }

        #region Empire Property
        public Civilization Empire
        {
            get { return _appContext.CurrentGame.Civilizations[_empireId]; }
        }
        #endregion

        #region Player Property
        public IPlayer Player
        {
            get
            {
                if (_playerId < Game.Player.GameHostID)
                {
                    return new Player
                           {
                               EmpireID = _empireId,
                               PlayerID = Game.Player.ComputerPlayerID
                           };
                }

                return _appContext.Players[_playerId];
            }
            set
            {
                if (value == null)
                    _playerId = Game.Player.UnassignedPlayerID;
                else
                    _playerId = value.PlayerID;

                OnPropertyChanged("Player");

                if (!Player.IsHumanPlayer)
                    IsReady = true;

                UpdateRelationshipStatus();
            }
        }
        #endregion

        #region IsReady Property
        private bool _isReady;

        public bool IsReady
        {
            get { return !Player.IsHumanPlayer || _isReady; }
            set
            {
                _isReady = value;

                OnPropertyChanged("IsReady");
            }
        }
        #endregion

        #region DiplomacyStatus Property

        private ForeignPowerStatus _diplomacyStatus;

        public ForeignPowerStatus DiplomacyStatus
        {
            get { return _diplomacyStatus; }
            set
            {
                if (value == _diplomacyStatus)
                    return;

                _diplomacyStatus = value;

                OnPropertyChanged("DiplomacyStatus");
            }
        }

        #endregion

        public void UpdateRelationshipStatus()
        {
            var localPlayer = _appContext.LocalPlayer;
            DiplomacyStatus = DiplomacyHelper.GetForeignPowerStatus(localPlayer.Empire, Empire);
        }

        #region Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}