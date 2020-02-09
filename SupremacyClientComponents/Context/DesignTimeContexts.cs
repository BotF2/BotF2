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
        private static List<CivilizationManager> _availableCivManagers;
        private static CivilizationManager _spiedCivDummy;
        //private static Dictionary<Civilization, List<Civilization>> _spyDictionary = new Dictionary<Civilization, List<Civilization>>();
        private static bool _subedZero = false; // Is the race not in the game? We substitue the host civ for missing civ and then _subedZero is true and Federation not in game.
        private static bool _subedOne = false;
        private static bool _subedTwo = false;
        private static bool _subedThree = false;
        private static bool _subedFour = false;
        private static bool _subedFive = false;
        private static bool _subedSix = false;

        /// <summary>
        /// Host Civilization Manager has been used as a substitute for a civ not in the game
        /// In this case Federation is not in game, CivID Zero
        /// </summary>
        public static bool SubedZero
        {
            get { return _subedZero; }
        }
        public static bool SubedOne
        {
            get { return _subedOne; }
        }
        public static bool SubedTwo
        {
            get { return _subedTwo; }
        }
        public static bool SubedThree
        {
            get { return _subedThree; }
        }
        public static bool SubedFour
        {
            get { return _subedFour; }
        }
        public static bool SubedFive
        {
            get { return _subedFive; }
        }
        public static bool SubedSix
        {
            get { return _subedSix; }
        }
        #region  Constuctor
        static DesignTimeObjects() 
        {
            _availableCivManagers = GameContext.Current.CivilizationManagers.Where(s => s.Civilization.IsEmpire).ToList();
        }
        #endregion
        public static CivilizationManager SpiedCivZero
        {
            get
            {
                _spiedCivDummy = CivilizationManager; // use host here as substitute, catch it later
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[0]; // if Fed is not in game this line fails to the catch and Host CivManager is left in as subsitute civManager
                }
                catch
                {
                    _subedZero = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivOne
        {
            get
            {
                _spiedCivDummy = CivilizationManager; // use host here as substitute, catch it later 
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[1]; // if Terran is not in game this line fails to the catch and Host CivManager is left in as subsitute civManager
                }
                catch
                {
                    _subedOne = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }

        public static CivilizationManager SpiedCivTwo
        {
            get
            {
                _spiedCivDummy = CivilizationManager; 
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[2];
                }
                catch
                {
                    _subedTwo = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivThree
        {
            get
            {
                _spiedCivDummy = CivilizationManager; 
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[3];
                }
                catch
                {
                    _subedThree = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFour
        {
            get
            {
                _spiedCivDummy = CivilizationManager;
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[4];
                }
                catch
                {
                    _subedFour = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFive
        {
            get
            {
                _spiedCivDummy = CivilizationManager;
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[5];
                }
                catch
                {
                    _subedFive = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivSix
        {
            get
            {
                _spiedCivDummy = CivilizationManager;
                try
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[6];
                }
                catch
                {
                    _subedSix = true;
                    _availableCivManagers.Remove(_spiedCivDummy);
                }
                return _spiedCivDummy;
            }
        }
        public static List<CivilizationManager> AvailableCivManagers
        {
            get { return _availableCivManagers; }
        }

        /// <summary>
        /// This is the Host Civilization Manager, see IntelHelper.localCivManager for civ manager in multiplayer
        /// Info on multiplayer civ manager is from AssetsScreen.xaml.cs so hope this works for multiplayer local machine
        /// </summary>
        public static CivilizationManager CivilizationManager 
        {
            get { return DesignTimeAppContext.Instance.LocalPlayerEmpire; }
        }

        /// <summary>
        /// This is the Host home colony, see IntelHelper.localCivManager for civ manager / colonies in multiplayer
        /// </summary>
        public static Colony Colony
        {
            get
            {
                return DesignTimeAppContext.Instance.LocalPlayerEmpire.HomeColony;
            }
        }
        /// <summary>
        /// This is the Host home colony, see IntelHelper.localCivManager for civ manager / colonies in multiplayer
        /// </summary>
        public static IEnumerable<Colony> Colonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies); }
        }
        public static IEnumerable<Colony> SpiedZeroColonies
        {
            get { return GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivZero.CivilizationID); }
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
    }
}