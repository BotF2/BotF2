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

        //public IPlayer SpiedOnePlayer
        //{
        //    get { return PlayerContext.Current.Players[1]; }
        //}

        public ILobbyData LobbyData
        {
            get { return _lobbyData; }
        }

        public CivilizationManager LocalPlayerEmpire
        {
            get { return GameContext.Current.CivilizationManagers[LocalPlayer.EmpireID]; }
        }
        //public CivilizationManager SpyEmpire
        //{
        //    get { return DesignTimeObjects.GetSpiedCivilizationOne(); }
        //}
        public CivilizationManager SpiedOneEmpire
        {
            get
            {
                return DesignTimeObjects.GetSpiedCivilizationOne();
            }//GameContext.Current.CivilizationManagers[DesignTimeObjects.GetSpiedCivilizationOne().CivilizationID]; }
        }

        public CivilizationManager SpiedTwoEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationTwo(); }
        }

        //public CivilizationManager SpiedThreeEmpire
        //{
        //    get { return DesignTimeObjects.GetSpiedCivilizationThree(); }
        //}

        //public CivilizationManager SpiedFourEmpire
        //{
        //    get { return DesignTimeObjects.GetSpiedCivilizationFour(); }
        //}

        //public CivilizationManager SpiedFiveEmpire
        //{
        //    get { return DesignTimeObjects.GetSpiedCivilizationFive(); }
        //}

        //public CivilizationManager SpiedSixEmpire
        //{
        //    get { return DesignTimeObjects.GetSpiedCivilizationSix(); }
        //}

        // turn off once we are using CivilizationManagersMap
        //public CivilizationManager SpyEmpire
        //{
        //    get { return GameContext.Current.CivilizationManagers[DesignTimeObjects.SpyCivilizationManager.CivilizationID]; }
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
        //private static readonly Dictionary<int, CivilizationManager> _spiedCivManagerDictionary;
        private static CivilizationManagerMap _otherMajorEmpires;
        static DesignTimeObjects()
        {
            _otherMajorEmpires = GetSpiedCivMangerMap();
            //_spiedCivManagerDictionary = GetSpiedCivMangerDictionary();

            //IntelHelper.SendLocalPlayer(CivilizationManager);
            //IntelHelper.SendSpiedCivOne(GetSpiedCivilizationOne());
            //IntelHelper.SendSpiedCivTwo(GetSpiedCivilizationTwo());
            //IntelHelper.SendSpiedCivThree(GetSpiedCivilizationThree());
            //IntelHelper.SendSpiedCivFour(GetSpiedCivilizationFour());
            //IntelHelper.SendSpiedCivFive(GetSpiedCivilizationFive());
            //IntelHelper.SendSpiedCivSix(GetSpiedCivilizationSix());
            //IntelHelper.SendSpyCiv(GetSpiedCivilizationOne());
        }

        public static CivilizationManager CivilizationManager
        {
            get { return DesignTimeAppContext.Instance.LocalPlayerEmpire; }
        }

        //public static CivilizationManager SpyCivilizationManager
        //{
        //    get { return GetSpiedCivilizationOne(); }
        //}

        public static Colony Colony
        {
            get
            {
                return DesignTimeAppContext.Instance.LocalPlayerEmpire.HomeColony;
            }
        }
        public static Colony SpiedOneColony
        {
            get { return GetSpiedCivilizationOne().HomeColony; }
        }

        public static Colony SpiedTwoColony
        {
            get { return GetSpiedCivilizationTwo().HomeColony; }
        }
        //public static Colony SpiedThreeColony
        //{
        //    get { return GetSpiedCivilizationThree().HomeColony; }
        //}
        //public static Colony SpiedFourColony
        //{
        //    get { return GetSpiedCivilizationFour().HomeColony; }
        //}
        //public static Colony SpiedFiveColony
        //{
        //    get { return GetSpiedCivilizationFive().HomeColony; }
        //}
        //public static Colony SpiedSixColony
        //{
        //    get { return GetSpiedCivilizationSix().HomeColony; }
        //}
        public static IEnumerable<Colony> Colonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies); }
        }
        public static IEnumerable<Colony> SpiedOneColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationOne().CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedTwoColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationTwo().CivilizationID); }
        }
        //public static IEnumerable<Colony> SpiedThreeColonies
        //{
        //    get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationThree().CivilizationID); }
        //}
        //public static IEnumerable<Colony> SpiedFourColonies
        //{
        //    get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationFour().CivilizationID); }
        //}
        //public static IEnumerable<Colony> SpiedFiveColonies
        //{
        //    get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationFive().CivilizationID); }
        //}
        //public static IEnumerable<Colony> SpiedSixColonies
        //{
        //    get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationSix().CivilizationID); }
        //}

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
        private static CivilizationManagerMap GetSpiedCivMangerMap()
        {        
            var allCivManagers = GameContext.Current.CivilizationManagers;
            CivilizationManagerMap otherManagers = new CivilizationManagerMap();
            foreach (var allCiv in allCivManagers)
            {
                otherManagers.Add(allCiv);
            }
            foreach (var aCivManager in allCivManagers)
            {
                if (aCivManager.CivilizationID > 6 || aCivManager.CivilizationID == DesignTimeAppContext.Instance.LocalPlayer.CivID)
                {
                    otherManagers.Remove(aCivManager);
                    //GameLog.Core.Intel.DebugFormat("Civ Manager removed {0}", aCivManager.Civilization.Name);
                }
            }
            return otherManagers;//_spiedCivManagerDictionary; // hope we get all major civs that are not local player
        }
        public static CivilizationManager GetSpiedCivilizationOne()
        {
            var civ = GetSpiedCivMangerMap().FirstOrDefault();
            _otherMajorEmpires.Remove(civ);
            return civ;
        }
        public static CivilizationManager GetSpiedCivilizationTwo()
        {
            var civ = GetSpiedCivMangerMap().FirstOrDefault();
            _otherMajorEmpires.Remove(civ);
            return civ;
        }
        //public static CivilizationManager GetSpiedCivilizationThree()
        //{
        //    return SpiedCivMangerDictionary[2]; 
        //}
        //public static CivilizationManager GetSpiedCivilizationFour()
        //{
        //    return SpiedCivMangerDictionary[3];
        //}
        //public static CivilizationManager GetSpiedCivilizationFive()
        //{
        //    return SpiedCivMangerDictionary[4]; 
        //}
        //public static CivilizationManager GetSpiedCivilizationSix()
        //{
        //    return SpiedCivMangerDictionary[5];
        //}
    }
}