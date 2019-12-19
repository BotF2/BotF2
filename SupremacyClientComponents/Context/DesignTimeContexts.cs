using Microsoft.Practices.Unity;
using Supremacy.Client.Audio;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Supremacy.Client.Context
{
    public class DesignTimeAppContext : IAppContext
    {
        #region Fields
        private static readonly Lazy<DesignTimeAppContext> _instance = new Lazy<DesignTimeAppContext>(false);

        private readonly LobbyData _lobbyData = null;
        private readonly KeyedCollectionBase<int, IPlayer> _players = null;
        private MusicLibrary _defaultMusicLibrary = new MusicLibrary();
        private MusicLibrary _themeMusicLibrary = new MusicLibrary();
        #endregion

        #region Properties
        public static DesignTimeAppContext Instance
        {
            get { return _instance.Value; }
        }

        public MusicLibrary DefaultMusicLibrary
        {
            get { return _defaultMusicLibrary; }
        }

        public MusicLibrary ThemeMusicLibrary
        {
            get { return _themeMusicLibrary; }
        }
        #endregion

        #region Construction & Lifetime
        public DesignTimeAppContext()
        {
            if (PlayerContext.Current == null ||
                PlayerContext.Current.Players.Count == 0)
            {
                PlayerContext.Current = new PlayerContext(
                    new ArrayWrapper<Player>(
                        new[]
                        {
                            new Player
                            {
                                EmpireID = GameContext.Current.Civilizations.FirstOrDefault(o => o.IsEmpire).CivID,
                                Name = "Local Player",
                                PlayerID = Player.GameHostID
                            }
                        }));
            }

            _lobbyData = new LobbyData
                         {
                             Empires = GameContext.Current.Civilizations.Where(o => o.IsEmpire).Select(o => o.Key).ToArray(),
                             GameOptions = GameContext.Current.Options,
                             Players = PlayerContext.Current.Players.ToArray(),
                             Slots = new[]
                                     {
                                         new PlayerSlot
                                         {
                                             Claim = SlotClaim.Assigned,
                                             EmpireID = PlayerContext.Current.Players[0].EmpireID,
                                             EmpireName = PlayerContext.Current.Players[0].Empire.Key,
                                             IsClosed = false,
                                             Player = PlayerContext.Current.Players[0],
                                             SlotID = 0,
                                             Status = SlotStatus.Taken
                                         }
                                     }
                         };

            _players = new KeyedCollectionBase<int, IPlayer>(o => o.PlayerID)
                       {
                           PlayerContext.Current.Players[0]
                       };
        }
        #endregion

        #region Implementation of INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IClientContext

        public IGameContext CurrentGame
        {
            get { return GameContext.Current; }
        }

        public bool IsConnected
        {
            get { return true; }
        }

        public bool IsGameHost
        {
            get { return true; }
        }

        public bool IsGameInPlay
        {
            get { return true; }
        }

        public bool IsGameEnding
        {
            get { return false; }
        }

        public bool IsSinglePlayerGame
        {
            get { return true; }
        }

        public bool IsFederationPlayable
        {
            get { return true; }
        }


        public bool IsRomulanPlayable
        {
            get { return true; }
        }


        public bool IsKlingonPlayable
        {
            get { return true; }
        }


        public bool IsCardassianPlayable
        {
            get { return true; }
        }


        public bool IsDominionPlayable
        {
            get { return true; }
        }

        public bool IsBorgPlayable
        {
            get { return true; }
        }

        public bool IsTerranEmpirePlayable
        {
            get { return true; }
        }

        public IPlayer LocalPlayer
        {
            get { return PlayerContext.Current.Players[0]; }
        }

        public ILobbyData LobbyData
        {
            get { return _lobbyData; }
        }

        public CivilizationManager LocalPlayerEmpire
        {
            get { return GameContext.Current.CivilizationManagers[LocalPlayer.EmpireID]; }
        }

        //public CivilizationManager SpiedOneEmpire
        //{
        //    get { return SpiedCivEmpires[0]; }
        //}

        //public CivilizationManager SpiedTwoEmpire
        //{
        //    get { return SpiedCivEmpires[1]; }
        //}

        //public CivilizationManager SpiedThreeEmpire
        //{
        //    get { return SpiedCivEmpires[2]; }
        //}

        //public CivilizationManager SpiedFourEmpire
        //{
        //    get { return SpiedCivEmpires[3]; }
        //}

        //public CivilizationManager SpiedFiveEmpire
        //{
        //    get { return SpiedCivEmpires[4]; }
        //}

        //public CivilizationManager SpiedSixEmpire
        //{
        //    get { return SpiedCivEmpires[5]; }
        //}

        public IEnumerable<IPlayer> RemotePlayers
        {
            get { return Enumerable.Empty<IPlayer>(); }
        }

        public IKeyedCollection<int, IPlayer> Players
        {
            get { return _players; }
        }

        public bool IsTurnFinished
        {
            get { return false; }
        }

        #endregion
    }
    public static class DesignTimeObjects
    {

        //static DesignTimeObjects()
        //{
        //    //SpiedCivManagers();
        //    //GetSpiedCivilizationOne();
        //    //GetSpiedCivilizationTwo();
        //    //GetSpiedCivilizationThree();
        //    //GetSpiedCivilizationFour();
        //    //GetSpiedCivilizationFive();
        //    //GetSpiedCivilizationSix();
        //}
        public static List<CivilizationManager> SpiedCivMangers
        { 
            get { return SpiedCivManagers(); }

        }
        public static CivilizationManager SpiedCivOne
        {
            get { return SpiedCivMangers[0]; }

        }
        public static CivilizationManager SpiedCivTwo
        {
            get { return SpiedCivMangers[1]; }

        }
        public static CivilizationManager SpiedCivThree
        {
            get { return SpiedCivMangers[2]; }

        }
        public static CivilizationManager SpiedCivFour
        {
            get { return SpiedCivMangers[3]; }

        }
        public static CivilizationManager SpiedCivFive
        {
            get { return SpiedCivMangers[4]; }

        }
        public static CivilizationManager SpiedCivSix
        {
            get { return SpiedCivMangers[5]; }

        }

        public static CivilizationManager CivilizationManager
        {
            get { return DesignTimeAppContext.Instance.LocalPlayerEmpire; }
        }

        public static Colony Colony
        {
            get
            {
                return DesignTimeAppContext.Instance.LocalPlayerEmpire.HomeColony;
            }
        }

        public static IEnumerable<Colony> Colonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies); }
        }
        public static IEnumerable<Colony> SpiedOneColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivOne.CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedTwoColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivTwo.CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedThreeColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivThree.CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedFourColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivFour.CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedFiveColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivFive.CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedSixColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivSix.CivilizationID); }
        }

        public static IEnumerable<StarSystem> StarSystems
        {
            get { return GameContext.Current.Universe.Find<StarSystem>(); }
        }
        public static IEnumerable<StarSystem> ControlledSystems
        {
            get
            {
                var claims = GameContext.Current.SectorClaims;
                var owner = CivilizationManager.Civilization;
                return GameContext.Current.Universe.Find(UniverseObjectType.StarSystem).Cast<StarSystem>().Where(s => claims.GetPerceivedOwner(s.Location, owner) == owner);
            }
        }
        private static List<CivilizationManager> SpiedCivManagers()
        {
            List<CivilizationManager> spiedCivManagers = new List<CivilizationManager>();

            spiedCivManagers.Clear();

            int _civIDinGame = -1;

            try { _civIDinGame = GameContext.Current.CivilizationManagers[6].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[5].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[4].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[3].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[2].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[1].CivilizationID; } catch { _civIDinGame = 0; }
            if (_civIDinGame != -1) try { _civIDinGame = GameContext.Current.CivilizationManagers[0].CivilizationID; } catch { _civIDinGame = 1; }

            GameLog.Core.Intel.DebugFormat("_civIDinGame: {0} is available", _civIDinGame);

            //while (spiedCivManagers.Count < 7)
            //{
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[0]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[1]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[2]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[3]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[4]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
                try { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[5]); } catch { spiedCivManagers.Add(GameContext.Current.CivilizationManagers[_civIDinGame]); }
            //}
            GameLog.Core.Intel.DebugFormat("spiedCivManagers 0 - 5 is populated");
            return spiedCivManagers;
        }
        public static CivilizationManager GetCivLocalPlayer()
        {
            return DesignTimeAppContext.Instance.LocalPlayerEmpire;
        }
    }
}