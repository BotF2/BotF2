// File:DesignTimeContexts.cs

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

//#pragma warning disable IDE0044 // Add readonly modifier
        private MusicLibrary _themeMusicLibrary = new MusicLibrary();
//#pragma warning restore IDE0044 // Add readonly modifier
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
#pragma warning disable CS0067 // The event 'DesignTimeAppContext.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'DesignTimeAppContext.PropertyChanged' is never used


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
        private static bool _subed_6 = false;
        [NonSerialized]
        private static string _text;

        /// <summary>
        /// Host Civilization Manager has been used as a substitute for a civ not in the game
        /// In this case Federation is not in game, CivID Zero
        /// </summary>
        public static bool Subed_0 { get; private set; } = false;
        public static bool Subed_1 { get; private set; } = false;
        public static bool Subed_2 { get; private set; } = false;
        public static bool Subed_3 { get; private set; } = false;
        public static bool Subed_4 { get; private set; } = false;
        public static bool Subed_5 { get; private set; } = false;
        public static bool Subed_6 => _subed_6;
        #region  Constuctor
        static DesignTimeObjects()
        {
            AvailableCivManagers = GameContext.Current.CivilizationManagers.Where(s => s.Civilization.IsEmpire).ToList();
        }
        #endregion
        public static CivilizationManager SpiedCiv_0
        {
            get
            {
                CivilizationManager isZeroNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 0);
                if (isZeroNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 0);
                    //GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_0 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_0 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_0 = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCiv_1
        {
            get
            {
                CivilizationManager isOneNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 1);
                if (isOneNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 1);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_1 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_1 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_1 = true;
                }
                return _spiedCivDummy;
            }
        }

        public static CivilizationManager SpiedCiv_2
        {
            get
            {
                CivilizationManager isTwoNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 2);
                if (isTwoNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 2);
                    //  GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_2 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    //  GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_2 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_2 = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCiv_3
        {
            get
            {
                CivilizationManager isThreeNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 3);
                if (isThreeNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 3);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_3 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_3 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_3 = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCiv_4
        {
            get
            {
                CivilizationManager isFourNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 4);
                if (isFourNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 4);
                    //  GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_4 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_4 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_4 = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCiv_5
        {
            get
            {
                CivilizationManager isFiveNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 5);
                if (isFiveNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 5);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_5 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_5 = {0}", _spiedCivDummy.Civilization.Key);
                    Subed_5 = true;
                }
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCiv_6
        {
            get
            {
                CivilizationManager isSixNull = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 6);
                if (isSixNull != null)
                {
                    _spiedCivDummy = GameContext.Current.CivilizationManagers.FirstOrDefault(s => s.CivilizationID == 6);
                    // GameLog.Client.Test.DebugFormat("## Playable SpiedCiv_6 = {0}", _spiedCivDummy.Civilization.Key);
                }
                else
                {
                    _spiedCivDummy = CivilizationManager;
                    // GameLog.Client.Test.DebugFormat("## Substitution SpiedCiv_6 = {0}", _spiedCivDummy.Civilization.Key);
                    _subed_6 = true;
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
        public static IEnumerable<Colony> SpiedZeroColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_0.CivilizationID);
        public static IEnumerable<Colony> SpiedOneColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_1.CivilizationID);
        public static IEnumerable<Colony> SpiedTwoColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_2.CivilizationID);
        public static IEnumerable<Colony> SpiedThreeColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_3.CivilizationID);
        public static IEnumerable<Colony> SpiedFourColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_4.CivilizationID);
        public static IEnumerable<Colony> SpiedFiveColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_5.CivilizationID);
        public static IEnumerable<Colony> SpiedSixColonies => GameContext.Current.CivilizationManagers.SelectMany(o => o.Colonies).Where(o => o.OwnerID == SpiedCiv_6.CivilizationID);
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