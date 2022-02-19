using Supremacy.Client.Audio;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
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

#pragma warning disable IDE0044 // Add readonly modifier
        private MusicLibrary _themeMusicLibrary = new MusicLibrary();
#pragma warning restore IDE0044 // Add readonly modifier
        #endregion

        #region Properties
        public static DesignTimeAppContext Instance => _instance.Value;

        public MusicLibrary DefaultMusicLibrary { get; } = new MusicLibrary();

        public MusicLibrary ThemeMusicLibrary => _themeMusicLibrary;
        public int ASpecialWidth1 => 576;
        public int ASpecialHeight1 => 480;
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

        public IGameContext CurrentGame => GameContext.Current;

        public bool IsConnected => true;

        public bool IsGameHost => true;

        public bool IsGameInPlay => true;

        public bool IsGameEnding => false;

        public bool IsSinglePlayerGame => true;

        public bool IsFederationPlayable => true;


        public bool IsRomulanPlayable => true;


        public bool IsKlingonPlayable => true;


        public bool IsCardassianPlayable => true;


        public bool IsDominionPlayable => true;

        public bool IsBorgPlayable => true;

        public bool IsTerranEmpirePlayable => true;

        public IPlayer LocalPlayer => PlayerContext.Current.Players[0];

        public ILobbyData LobbyData => _lobbyData;

        public CivilizationManager LocalPlayerEmpire => GameContext.Current.CivilizationManagers[LocalPlayer.EmpireID];

        public IEnumerable<IPlayer> RemotePlayers => Enumerable.Empty<IPlayer>();

        public IKeyedCollection<int, IPlayer> Players => _players;

        public bool IsTurnFinished => false;

        #endregion
    }
    public static class DesignTimeObjects
    {
        private static CivilizationManager _spiedCivDummy;
        private static bool _subedSix = false;
        private static string _text;

        /// <summary>
        /// Host Civilization Manager has been used as a substitute for a civ not in the game
        /// In this case Federation is not in game, CivID Zero
        /// </summary>
        public static bool SubedZero { get; private set; } = false;
        public static bool SubedOne { get; private set; } = false;
        public static bool SubedTwo { get; private set; } = false;
        public static bool SubedThree { get; private set; } = false;
        public static bool SubedFour { get; private set; } = false;
        public static bool SubedFive { get; private set; } = false;
        public static bool SubedSix => _subedSix;
        #region  Constuctor
        static DesignTimeObjects()
        {
            AvailableCivManagers = GameContext.Current.CivilizationManagers.Where(s => s.Civilization.IsEmpire).ToList();
        }
        #endregion
        public static CivilizationManager SpiedCivZero
        {
            get
            {
                CivilizationManager isZeroNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 0);
                if (isZeroNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 0);
                    //GameLog.Client.Test.DebugFormat("## Playable SpiedCivZero = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //GameLog.Client.Test.DebugFormat("## Substitution SpiedCivZero = {0}", _spiedCivDummy.Civilization.Key);
                    SubedZero = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivOne
        {
            get
            {
                CivilizationManager isOneNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 1);
                if (isOneNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 1);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCivOne = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //GameLog.Client.Test.DebugFormat("## Substitution SpiedCivOne = {0}", _spiedCivDummy.Civilization.Key);
                    SubedOne = true;
                }
                return _spiedCivDummy;
            }
        }

        public static CivilizationManager SpiedCivTwo
        {
            get
            {
                CivilizationManager isTwoNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 2);
                if (isTwoNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 2);
                    //  GameLog.Client.Test.DebugFormat("## Playable SpiedCivTwo = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //  GameLog.Client.Test.DebugFormat("## Substitution SpiedCivTwo = {0}", _spiedCivDummy.Civilization.Key);
                    SubedTwo = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivThree
        {
            get
            {
                CivilizationManager isThreeNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 3);
                if (isThreeNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 3);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCivThree = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCivThree = {0}", _spiedCivDummy.Civilization.Key);
                    SubedThree = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFour
        {
            get
            {
                CivilizationManager isFourNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 4);
                if (isFourNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 4);
                    //  GameLog.Client.Test.DebugFormat("## Playable SpiedCivFour = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCivFour = {0}", _spiedCivDummy.Civilization.Key);
                    SubedFour = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFive
        {
            get
            {
                CivilizationManager isFiveNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 5);
                if (isFiveNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 5);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCivFive = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCivFive = {0}", _spiedCivDummy.Civilization.Key);
                    SubedFive = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivSix
        {
            get
            {
                CivilizationManager isSixNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 6);
                if (isSixNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 6);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCivSix = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCivSix = {0}", _spiedCivDummy.Civilization.Key);
                    _subedSix = true;
                }
                return _spiedCivDummy;
            }
        }
        public static List<CivilizationManager> AvailableCivManagers { get; private set; }

        /// <summary>
        /// This is the Host Civilization Manager, see IntelHelper.localCivManager for civ manager in multiplayer
        /// Info on multiplayer civ manager is from AssetsScreen.xaml.cs so hope this works for multiplayer local machine
        /// </summary>
        public static CivilizationManager CivilizationManager => DesignTimeAppContext.Instance.LocalPlayerEmpire;

        /// <summary>
        /// This is the Host home colony, see IntelHelper.localCivManager for civ manager / colonies in multiplayer
        /// </summary>
        public static Colony Colony => DesignTimeAppContext.Instance.LocalPlayerEmpire.HomeColony;
        /// <summary>
        /// This is the Host home colony, see IntelHelper.localCivManager for civ manager / colonies in multiplayer
        /// </summary>
        public static IEnumerable<Colony> Colonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies);
        public static IEnumerable<Colony> SpiedZeroColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivZero.CivilizationID);
        public static IEnumerable<Colony> SpiedOneColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivOne.CivilizationID);
        public static IEnumerable<Colony> SpiedTwoColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivTwo.CivilizationID);
        public static IEnumerable<Colony> SpiedThreeColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivThree.CivilizationID);
        public static IEnumerable<Colony> SpiedFourColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivFour.CivilizationID);
        public static IEnumerable<Colony> SpiedFiveColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivFive.CivilizationID);
        public static IEnumerable<Colony> SpiedSixColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCivSix.CivilizationID);
        public static IEnumerable<StarSystem> StarSystems => GameContext.Current.Universe.Find<StarSystem>();

        public static IEnumerable<StarSystem> ControlledSystems
        {
            get
            {
                SectorClaimGrid claims = GameContext.Current.SectorClaims;
                Civilization owner = CivilizationManager.Civilization;
                _text = "Search for ControlledSystems";
                Console.WriteLine(_text);
                return GameContext.Current.Universe.Find(UniverseObjectType.StarSystem).Cast<StarSystem>().Where(s => claims.GetPerceivedOwner(s.Location, owner) == owner);
            }
        }
    }
}