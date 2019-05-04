// CombatUpdate.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Combat
{
    [Serializable]
    public class CombatUpdate
    {
        private int _combatId;
        private int _roundNumber;
        private int _ownerId;

        private bool _standoff;
        private MapLocation _location;
        private IList<CombatAssets> _friendlyAssets;
        private IList<CombatAssets> _hostileAssets;
        //private Dictionary<Civilization, Civilization> _willTarget;
        private List<Object> _civList;
        private List<string> _civShortNameList;
        private List<string> _civFirePowerList;
        private List<Civilization> _civStatusList;
        private int _friendlyEmpireStrength;
        private int _allHostileEmpireStrength;

        public CombatUpdate(int combatId, int roundNumber, bool standoff, Civilization owner, MapLocation location, IList<CombatAssets> friendlyAssets, IList<CombatAssets> hostileAssets)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (friendlyAssets == null)
                throw new ArgumentNullException("friendlyAssets");
            if (hostileAssets == null)
                throw new ArgumentNullException("hostileAssets");

            _combatId = combatId;
            _roundNumber = roundNumber;
            _standoff = standoff;
            _ownerId = owner.CivID;
            _location = location;
            _friendlyAssets = friendlyAssets;
            _hostileAssets = hostileAssets;

        }
        #region Properties for total fire power of the friends and Others (hostiles)
        public int FriendlyEmpireStrength
        {

            get
            {
                foreach (var fa in FriendlyAssets)
                {

                    foreach (var cs in fa.CombatShips)   // only combat ships 
                    {
                        _friendlyEmpireStrength += cs.FirePower;

                        GameLog.Core.CombatDetails.DebugFormat("adding _friendlyEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                            cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.FirePower, _friendlyEmpireStrength);
                    }

                    if (fa.Station != null)
                    {
                        _friendlyEmpireStrength += fa.Station.FirePower;

                        GameLog.Core.CombatDetails.DebugFormat("adding _friendlyEmpireStrength for {0}  - in total now {1}",
                            fa.Station.Name, _friendlyEmpireStrength); // fa.Source.Name, fa.Source.Design, _friendlyEmpireStrength);
                    }
                }

                return _friendlyEmpireStrength;
            }
        }

        public int AllHostileEmpireStrength
        {
            get
            {
                foreach (var ha in HostileAssets)
                {

                    foreach (var cs in ha.CombatShips)   // only combat ships 
                    {
                        _allHostileEmpireStrength += cs.FirePower;

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                        //    cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.FirePower, _hostileEmpireStrength);
                    }

                    foreach (var ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        _allHostileEmpireStrength += ncs.FirePower;
                    }
                    if (ha.Station != null)
                    {
                        _allHostileEmpireStrength += ha.Station.FirePower;

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0}  - in total now {1}",
                        //    ha.Station.Name, _hostileEmpireStrength); 
                    }
                }
                return _allHostileEmpireStrength;
            }
        }
        #endregion

        #region Properties for other civilization insignias
        public string OtherCivInsignia1
        {
            get
            {
                var asset = _hostileAssets.FirstOrDefault();
                var civOwner = asset.Owner;
                var civKey = asset.Owner.Key;
                List<Object> civList = new List<Object>();
                foreach (var ha in _hostileAssets)
                {
                    civList.Add(ha.Owner);
                    civList.Distinct().ToList();
                }
                civList.Remove(civOwner);
                _civList = civList.ToList();
                return civKey;
            }
        }

        public string OtherCivInsignia2
        {
            get
            {
                return GetOthersInsignia();
            }
        }

        public string OtherCivInsignia3
        {
            get
            {
                return GetOthersInsignia();
            }
        }

        public string OtherCivInsignia4
        {
            get
            {
                return GetOthersInsignia();
            }
        }
        #endregion

        public string GetOthersInsignia()
        {
            if (_civList.FirstOrDefault() != null)
            {
                var civ = _civList.FirstOrDefault();
                _civList.Remove(civ);
                var civKey = civ.ToString();

                return civKey;
            }
            string civi = "BlackInsignia";
            return civi;
        }

        #region other Civ name

        public string CivName1
        {
            get
            {
                var asset = _hostileAssets.FirstOrDefault();
                var civShortName = asset.Owner.ShortName;
                List<string> civNameList = new List<string>();
                foreach (var ha in _hostileAssets)
                {
                    civNameList.Add(ha.Owner.ShortName);
                    civNameList.Distinct().ToList();
                }
                civNameList.Remove(civShortName);
                _civFirePowerList = civNameList.ToList();
                return civShortName;
            }

        }

        public string CivName2
        {
            get
            {
                return GetOthersName();
            }

        }

        public string CivName3
        {
            get
            {
                return GetOthersName();
            }

        }

        public string CivName4
        {
            get
            {
                return GetOthersName();
            }

        }
        #endregion

        public string GetOthersName()
        {
            if (_civFirePowerList.FirstOrDefault() != null)
            {
                var civShortName = _civFirePowerList.FirstOrDefault();
                List<string> civNameList = new List<string>();
                foreach (var name in _civFirePowerList)
                {
                    civNameList.Add(name);
                    civNameList.Distinct().ToList();
                }
                civNameList.Remove(civShortName);
                _civFirePowerList = civNameList.ToList();
                return civShortName;
            }
            else { return null; }
        }

        #region other Civ Status

        public string CivStatus1
        {
            get
            {
                var asset = _hostileAssets.FirstOrDefault();
                var currentOwner = asset.Owner;
                List<Civilization> civOwner = new List<Civilization>();

                var _targetCiv1Status = GameContext.Current.DiplomacyData[Owner, asset.Owner].Status.ToString();
                //GameLog.Core.Combat.DebugFormat("Status Target 1: Status = {2} for Owner = {0} vs others = {1}", 
                //Owner, asset.Owner, _targetCiv1Status);

                List<string> civStatusList = new List<string>();  // list of Status
                foreach (var ha in _hostileAssets)
                {
                    civOwner.Add(ha.Owner);
                    civOwner.Distinct().ToList();

                }
                civOwner.Remove(currentOwner);

                _civStatusList = civOwner.ToList();
                return String.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + " " + ReturnTextOfStatus(_targetCiv1Status);
            }

        }

        public string CivStatus2
        {
            get
            {
                return GetStatusToOthers();
            }

        }

        public string CivStatus3
        {
            get
            {
                return GetStatusToOthers();
            }

        }

        public string CivStatus4
        {
            get
            {
                return GetStatusToOthers();
            }

        }
        #endregion

        public string GetStatusToOthers()
        {
            if (_civStatusList.FirstOrDefault() != null)
            {
                var currentCiv = _civStatusList.FirstOrDefault();
                var _targetCiv1Status = GameContext.Current.DiplomacyData[Owner, currentCiv].Status.ToString();
                List<Civilization> civStatusList = new List<Civilization>();
                foreach (var civilization in _civStatusList)
                {
                    civStatusList.Add(civilization);
                    civStatusList.Distinct().ToList();
                }
                GameLog.Core.Combat.DebugFormat("_targetCiv1Status = {0}", _targetCiv1Status);
                civStatusList.Remove(currentCiv);
                _civStatusList = civStatusList.ToList();
                return String.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + " " + ReturnTextOfStatus(_targetCiv1Status);
            }
            else { return null; }
        }


        private string ReturnTextOfStatus(string status)

        {
            var enumStatus = (ForeignPowerStatus)Enum.Parse(typeof(ForeignPowerStatus), status);

            string returnStatus = " ";

            switch (enumStatus)
            {
                case ForeignPowerStatus.NoContact:
                    returnStatus = "First Contact";
                    break;
                case ForeignPowerStatus.CounterpartyIsSubjugated:
                    returnStatus = "Subjugated";
                    break;
                case ForeignPowerStatus.AtWar:
                    returnStatus = "War";
                    break;
                case ForeignPowerStatus.CounterpartyIsUnreachable:
                    returnStatus = "Undefined";
                    break;
                default:
                    returnStatus = status;
                    break;
            }

            return returnStatus;
        }

        #region Properties for civ firepowers
        public string CivFirePowers1
        {
            get
            {
                var asset = _hostileAssets.FirstOrDefault();
                var civShortName = asset.Owner.ShortName;
                List<string> civNameList = new List<string>();
                foreach (var ha in _hostileAssets)
                {
                    civNameList.Add(ha.Owner.ShortName);
                    civNameList.Distinct().ToList();
                }
                civNameList.Remove(civShortName);
                _civShortNameList = civNameList.ToList();

                int otherCivStrength = 0;
                var _otherAssetsLocal = _hostileAssets.ToList();

                foreach (var ha in _hostileAssets)
                {

                    foreach (var cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civShortName == cs.Owner.ShortName)
                        {
                            otherCivStrength += cs.FirePower;
                            _otherAssetsLocal.Remove(ha);
                        }
                    }
                    foreach (var ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        if (civShortName == ncs.Owner.ShortName)
                        {
                            otherCivStrength += ncs.FirePower;
                            _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        otherCivStrength += ha.Station.FirePower;
                        _otherAssetsLocal.Remove(ha);
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("A civilization with firepower {0}", otherCivStrength);
                return otherCivStrength.ToString() + " " + String.Format(ResourceManager.GetString("COMBAT_POWER"));
            }
        }

        public string CivFirePowers2
        {
            get
            {
                return GetOthersFirePower();
            }
        }
        public string CivFirePowers3
        {
            get
            {
                return GetOthersFirePower();
            }
        }

        public string CivFirePowers4
        {
            get
            {
                return GetOthersFirePower();
            }
        }
        #endregion of proerties for civilizations firepowers

        public string GetOthersFirePower()
        {
            if (_civShortNameList.FirstOrDefault() != null)
            {
                var civShortName = _civShortNameList.FirstOrDefault();
                List<string> civNameList = new List<string>();
                foreach (var name in _civShortNameList)
                {
                    civNameList.Add(name);
                    civNameList.Distinct().ToList();
                }
                civNameList.Remove(civShortName);
                _civShortNameList = civNameList.ToList();

                int otherCivStrength = 0;
                var _otherAssetsLocal = _hostileAssets.ToList();

                foreach (var ha in _hostileAssets)
                {

                    foreach (var cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civShortName == cs.Owner.ShortName)
                        {
                            otherCivStrength += cs.FirePower;
                            _otherAssetsLocal.Remove(ha);
                        }
                    }
                    foreach (var ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        if (civShortName == ncs.Owner.ShortName)
                        {
                            otherCivStrength += ncs.FirePower;
                            _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        otherCivStrength += ha.Station.FirePower;
                        _otherAssetsLocal.Remove(ha);
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("A civilization with firepower {0}", otherCivStrength);
                return otherCivStrength.ToString() + " " + String.Format(ResourceManager.GetString("COMBAT_POWER"));
            }
            else { return null; }

        }

        public string TargetCiv1Status(Civilization us, Civilization others)
        {
            //get {
            //var us = _playerAssets.Owner;

            var _targetCiv1Status = GameContext.Current.DiplomacyData[us, others].Status.ToString();
            GameLog.Core.Combat.DebugFormat("Status Target 1: Us = {0}, Status = {2}, others = {1}", us, others, _targetCiv1Status);

            //foreach (var civs in _otherCivs)
            //{
            //    var diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            //}
            return _targetCiv1Status;
            //}
        }

        public int CombatID
        {
            get { return _combatId; }
        }

        public int RoundNumber
        {
            get { return _roundNumber; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public MapLocation Location
        {
            get { return _location; }
        }

        public Sector Sector
        {
            get { return GameContext.Current.Universe.Map[Location]; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public IList<CombatAssets> FriendlyAssets
        {
            get
            {

                return _friendlyAssets;
            }
        }

        public IList<CombatAssets> HostileAssets
        {
            get { return _hostileAssets; }
        }

        public bool IsStandoff
        {
            get { return _standoff; }
        }

        public bool IsCombatOver
        {
            get
            {
                if (_standoff)
                    return true;

                int friendlyAssets = 0;
                int hostileAssets = 0;
                
                foreach (CombatAssets assets in FriendlyAssets)
                {
                    if (assets.HasSurvivingAssets)
                    {
                        //GameLog.Core.Combat.DebugFormat("Combat: friendlyAssets(assets.CombatShips.Count)={0}", assets.CombatShips.Count);
                        friendlyAssets++;
                    }
                }

                //GameLog.Print("Combat: friendlyAssets(Amount)={0}", friendlyAssets);

                if (friendlyAssets == 0)
                {
                    GameLog.Core.Combat.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                    return true;
                }

                foreach (CombatAssets assets in HostileAssets)
                {
                    if (assets.HasSurvivingAssets)
                    {
                        //GameLog.Core.Combat.DebugFormat("Combat: hostileAssets(assets.CombatShips.Count)={0}", assets.CombatShips.Count);
                        hostileAssets++;
                    }
                }

                if (hostileAssets == 0)
                {
                    //GameLog.Core.Combat.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                    return true;
                }

                return (hostileAssets == 0);
            }
        }
    }
}
