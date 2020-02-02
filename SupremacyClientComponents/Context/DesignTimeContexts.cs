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
        //static List<CivilizationManager> managerList;
        ////static List<CivilizationManager> _allManagersList;
        //static CivilizationManager _spyingCivManager;  // syping, but in mulitplayer maybe different to local one
        static CivilizationManager _spiedCivDummy;
        private static Dictionary<Civilization, List<Civilization>> _spyDictionary = new Dictionary<Civilization, List<Civilization>>();
        private static bool _subedZero = false; // Is the race not in the game? We substitue the host civ for missing civ and then _subedZero is true and Federation not in game.
        private static bool _subedOne = false;
        private static bool _subedTwo = false;
        private static bool _subedThree = false;
        private static bool _subedFour = false;
        private static bool _subedFive = false;
        private static bool _subedSix = false;

        public static bool SubedZero
        {
            get { return _subedZero; }
        }
        /// <summary>
        /// Host Civilization Manager has been used as a substitute for a civ not in the game
        /// In this case Federation is not in game, CivID Zero
        /// </summary>
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

        //private static List<CivilizationManager> spyableCivManagers;

        //static DesignTimeObjects()
        //{
        //    managerList = SpyableCivManagers();
        //}
        //public static List<CivilizationManager> SpiedCivMangers
        //{ 
        //    get { return managerList; }

        //}
        //public static List<CivilizationManager> allCivManagers
        //{
        //    get { return _allManagersList; }
        //    set
        //    { 
        //        allCivManagers = managerList;
        //        allCivManagers.Add(LocalCivManager);
        //        allCivManagers = value;
        //    }
        //}

        public static CivilizationManager SpiedCivZero
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_0_DummyList.Contains(GameContext.Current.CivilizationManagers[0].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[0];
                }
                catch { _subedZero = true; }

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[0];

                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivOne
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_1_DummyList.Contains(GameContext.Current.CivilizationManagers[1].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[1];
                }
                catch { _subedOne = true; }

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[1];

                return _spiedCivDummy;
            }
        }

        public static CivilizationManager SpiedCivTwo
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_2_DummyList.Contains(GameContext.Current.CivilizationManagers[2].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[2];
                }
                catch { _subedTwo = true; }

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[2];
                
                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivThree
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_3_DummyList.Contains(GameContext.Current.CivilizationManagers[3].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[3];
                }
                catch { _subedThree = true; }


                //foreach (var item in _spiedCiv_3_DummyList)
                //{
                //    //GameLog.Core.UI.DebugFormat("_spiedCivThreeDummyList-Entry = {0}", item.Key);
                //}

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[3];

                ////GameLog.Core.UI.DebugFormat("_spiedCivThree (FINALLY) = {0}", _spiedCivDummy.Civilization.Key);

                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFour
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_4_DummyList.Contains(GameContext.Current.CivilizationManagers[4].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[4];
                }
                catch { _subedFour = true; }


                //for (int i = 0; i < _spiedCiv_4_DummyList.Count; i++)
                //{
                //    GameLog.Core.UI.DebugFormat("{0} is spying to > _spiedCiv_4_DummyList-Entry = {1}", IntelHelper.LocalCivManager.Civilization.Key, _spiedCiv_4_DummyList[i].Key);
                //}


                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[4];

                //GameLog.Core.UI.DebugFormat("_spiedCiv_4_ (FINALLY) = {0}", _spiedCivDummy.Civilization.Key);

                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivFive
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_5_DummyList.Contains(GameContext.Current.CivilizationManagers[5].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[5];
                }
                catch { _subedFive = true; }

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[5];

                return _spiedCivDummy;
            }
        }
        public static CivilizationManager SpiedCivSix
        {
            get
            {
                _spiedCivDummy = IntelHelper.LocalCivManager;
                try
                {
                    if (IntelHelper._spyingCiv_6_DummyList.Contains(GameContext.Current.CivilizationManagers[6].Civilization))
                        _spiedCivDummy = GameContext.Current.CivilizationManagers[6];
                }
                catch { _subedSix = true; }

                if (_spiedCivDummy == null)
                    _spiedCivDummy = GameContext.Current.CivilizationManagers[6];

                return _spiedCivDummy;
            }


        }
        //public static CivilizationManager SpyingCivManager
        //{
        //    get { return _spyingCivManager; }
        //    set
        //    {
        //        _spyingCivManager = value;
        //    }
        //}
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
        //private static List<CivilizationManager> SpyableCivManagers()
        //{
        //    var LocalCivManager = DesignTimeAppContext.Instance.LocalPlayerEmpire;
        //    var CivManagers = GameContext.Current.CivilizationManagers.Where(o => o.Civilization.IsEmpire).ToList();

        //    try
        //    {
        //        if (CivManagers[0].Civilization.Key!= null && CivManagers[0].Civilization.Key != "FEDERATION") 
        //            CivManagers.Insert(0, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(0, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[1].Civilization.Key != null && CivManagers[1].Civilization.Key != "TERRANEMPIRE")
        //            CivManagers.Insert(1, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(1, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[2].Civilization.Key != null && CivManagers[2].Civilization.Key != "ROMULANS")
        //            CivManagers.Insert(2, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(2, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[3].Civilization.Key != null && CivManagers[3].Civilization.Key != "KLINGONS")
        //            CivManagers.Insert(3, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(3, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[4].Civilization.Key != null && CivManagers[4].Civilization.Key != "CARDASSIANS")
        //            CivManagers.Insert(4, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(4, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[5].Civilization.Key != null && CivManagers[5].Civilization.Key != "DOMINION")
        //            CivManagers.Insert(5, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(5, LocalCivManager);
        //    }

        //    try
        //    {
        //        if (CivManagers[6].Civilization.Key != null && CivManagers[6].Civilization.Key != "BORG")
        //            CivManagers.Insert(6, LocalCivManager);
        //    }
        //    catch
        //    {
        //        CivManagers.Insert(6, LocalCivManager);
        //    }

            //GameLogOutCivMan(CivManagers);

        //    CivManagers.Remove(LocalCivManager);
        //    CivManagers.OrderBy(o => o.CivilizationID);

        //    GameLog.Client.UI.DebugFormat("--------------------");
        //    foreach (var civ in CivManagers)
        //    {
        //        GameLog.Client.UI.DebugFormat("civManagers contains {0} {1}", civ.CivilizationID, civ.Civilization.Key);
        //    }

        //    return CivManagers;
        //}

        //public static CivilizationManager GetCivLocalPlayer()
        //{
        //    return DesignTimeAppContext.Instance.LocalPlayerEmpire;
        //}
    }
}