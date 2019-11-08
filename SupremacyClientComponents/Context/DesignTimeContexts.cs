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

        public CivilizationManager SpiedOneEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationOne(); } 
        }

        public CivilizationManager SpiedTwoEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationTwo(); }
        }

        public CivilizationManager SpiedThreeEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationThree(); }
        }

        public CivilizationManager SpiedFourEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationFour(); }
        }

        public CivilizationManager SpiedFiveEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationFive(); }
        }

        public CivilizationManager SpiedSixEmpire
        {
            get { return DesignTimeObjects.GetSpiedCivilizationSix(); }
        }

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
        private static List<CivilizationManager> _otherMajorEmpires;
        static DesignTimeObjects()
        {
            _otherMajorEmpires = SpiedCivManagers();
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
        public static Colony SpiedOneColony
        {
            get { return GetSpiedCivilizationOne().HomeColony; }
        }

        public static Colony SpiedTwoColony
        {
            get { return GetSpiedCivilizationTwo().HomeColony; }
        }
        public static Colony SpiedThreeColony
        {
            get { return GetSpiedCivilizationThree().HomeColony; }
        }
        public static Colony SpiedFourColony
        {
            get { return GetSpiedCivilizationFour().HomeColony; }
        }
        public static Colony SpiedFiveColony
        {
            get { return GetSpiedCivilizationFive().HomeColony; }
        }
        public static Colony SpiedSixColony
        {
            get { return GetSpiedCivilizationSix().HomeColony; }
        }
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
        public static IEnumerable<Colony> SpiedThreeColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationThree().CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedFourColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationFour().CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedFiveColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationFive().CivilizationID); }
        }
        public static IEnumerable<Colony> SpiedSixColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == DesignTimeObjects.GetSpiedCivilizationSix().CivilizationID); }
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
            var allCivs = GameContext.Current.CivilizationManagers;
            var localCiv = GameContext.Current.CivilizationManagers[DesignTimeAppContext.Instance.LocalPlayerEmpire.CivilizationID];
            List<CivilizationManager> spiedCivMangers = new List<CivilizationManager>();

            Civilization DummyCiv = new Civilization();          
            CivilizationManager DummyManager= new CivilizationManager(GameContext.Current, DummyCiv);

            foreach (var aCiv in allCivs)
            {
                if (aCiv.Civilization.IsEmpire)
                    spiedCivMangers.Add(aCiv);
            }
            spiedCivMangers.Remove(localCiv);

            while (6 - spiedCivMangers.Count > 0)
            {
                spiedCivMangers.Add(DummyManager);
            }

            return spiedCivMangers;
        }
        public static CivilizationManager GetSpiedCivilizationOne()
        {
            IntelHelper.SendSpiedCivOne(_otherMajorEmpires[0]);
            return _otherMajorEmpires[0];
        }
        public static CivilizationManager GetSpiedCivilizationTwo()
        {
            IntelHelper.SendSpiedCivTwo(_otherMajorEmpires[1]);
            return _otherMajorEmpires[1];
        }
        public static CivilizationManager GetSpiedCivilizationThree()
        {
            IntelHelper.SendSpiedCivThree(_otherMajorEmpires[2]);
            return _otherMajorEmpires[2];
        }
        public static CivilizationManager GetSpiedCivilizationFour()
        {
            IntelHelper.SendSpiedCivFour(_otherMajorEmpires[3]);
            return _otherMajorEmpires[3];
        }
        public static CivilizationManager GetSpiedCivilizationFive()
        {
            IntelHelper.SendSpiedCivFive(_otherMajorEmpires[4]);
            return _otherMajorEmpires[4];
        }
        public static CivilizationManager GetSpiedCivilizationSix()
        {
            IntelHelper.SendSpiedCivSix(_otherMajorEmpires[5]);
            return _otherMajorEmpires[5];
        }
    }
}