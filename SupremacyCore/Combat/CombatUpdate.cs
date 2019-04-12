// CombatUpdate.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Game;
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
        // protected Dictionary<string, int> _combatPartyStrengths; // string in key of civ and int is total fire power of civ
        private bool _standoff;
        private MapLocation _location;
        private IList<CombatAssets> _friendlyAssets;
        private IList<CombatAssets> _hostileAssets;
        private List<CombatAssets> _otherAssetsDinamic;
        private List<Object> _civList;
        private List<Object> _civShortNameList;
        private string civAndFirePower = " ";
        private int _friendlyEmpireStrength;
        private int _allHostileEmpireStrength;
        // private int _otherCivStrength;
        //private Dictionary<Civilization, int> _dictionaryOtherCivStrengths;
       

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
            _otherAssetsDinamic = HostileAssets.ToList();
            
        }

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

                        //  (cs.Source.OrbitalDesign.PrimaryWeapon.Damage * cs.Source.OrbitalDesign.PrimaryWeapon.Count)
                        //+ (cs.Source.OrbitalDesign.SecondaryWeapon.Damage * cs.Source.OrbitalDesign.SecondaryWeapon.Count);


                        // works well
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
                        //  (fa.Station.FirePower  fa.Source.OrbitalDesign.PrimaryWeapon.Damage * cs.Source.OrbitalDesign.PrimaryWeapon.Count)
                        //+ (cs.Source.OrbitalDesign.SecondaryWeapon.Damage * cs.Source.OrbitalDesign.SecondaryWeapon.Count);

                        // works well
                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0}  - in total now {1}",
                        //    ha.Station.Name, _hostileEmpireStrength); 
                    }
                }
                return _allHostileEmpireStrength;
            }
        }

        public string OtherCivKey1
        {
            get
            {
                var asset = _otherAssetsDinamic.FirstOrDefault();
                var civOwner = asset.Owner;
                var civKey = asset.Owner.Key;
                List<Object> civList = new List<Object>();
                foreach(var ha in _otherAssetsDinamic)
                {
                    civList.Add(ha.Owner);
                    civList.Distinct().ToList();
                }
                civList.Remove(civOwner);
                _civList = civList.ToList();
                return civKey;
            }
        }

        public string OtherCivKey2
        {
            get
            {
                if (_civList.FirstOrDefault() != null)
                {
                    var civ = _civList.FirstOrDefault();
                    _civList.Remove(civ);
                    var civKey = civ.ToString();

                    return civKey;
                }
                else { return null; }
                //string civi = "Romulans";
                //return civi;
            }
        }

        public Object OtherCivKey3
        {
            get
            {
                if (_civList.FirstOrDefault() != null)
                {
                    var civ = _civList.FirstOrDefault();
                    _civList.Remove(civ);
                    var civKey = civ.ToString();

                    return civKey;
                }
                else { return null; }
                //string civi = "Klingons";
                //return civi;
            }
        }

        public Object OtherCivKey4
        {
            get
            {
                if (_civList.FirstOrDefault() != null)
                {
                    var civ = _civList.FirstOrDefault();
                    _civList.Remove(civ);
                    var civKey = civ.ToString();

                    return civKey;
                }
                else { return null; }

                //string civi = "Cardassians";
                //return civi;
            }
        }

        public string CivsAndFirePowers1
        {
            get
            {
                var asset = _otherAssetsDinamic.FirstOrDefault();
                var civOwner = asset.Owner;
   
                List<Object> civList = new List<Object>();
                foreach (var ha in _otherAssetsDinamic)
                {
                    civList.Add(ha.Owner);
                    civList.Distinct().ToList();
                }
                civList.Remove(civOwner);
                _civShortNameList = civList.ToList();
                string civShortName;
                int otherCivStrength = 0;
                var _otherAssetsLocal = _otherAssetsDinamic.ToList();
            
                civShortName = asset.Owner.ShortName;

                foreach (var ha in _otherAssetsDinamic)
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
                civAndFirePower = civShortName + " Firepower " + otherCivStrength.ToString();
                GameLog.Core.CombatDetails.DebugFormat("A civilization with firepower {0}", civAndFirePower);
                return civAndFirePower;
            }
        }

        public string CivsAndFirePowers2
        {
            get
            {
                if (_civShortNameList.FirstOrDefault() != null)
                {
                    var civ = _civShortNameList.FirstOrDefault();
                    _civShortNameList.Remove(civ);
                    var civKey = civ.ToString();
                    return civKey;

                }
                else { return null; }
                
                //string civi = "Romulan FirePower 1234";
                //return civi; 

            }
        }
        public string CivsAndFirePowers3
        {
            get
            {
                if (_civShortNameList.FirstOrDefault() != null)
                {
                    var civ = _civShortNameList.FirstOrDefault();
                    _civShortNameList.Remove(civ);
                    var civKey = civ.ToString();
                    return civKey;
                }
                else { return null; }
                //string civi = "Klingon FirePower 1234";
                //return civi;

            }
        }

        public string CivsAndFirePowers4
        {
            get
            {
                if (_civShortNameList.FirstOrDefault() != null)
                {
                    var civ = _civShortNameList.FirstOrDefault();
                    _civShortNameList.Remove(civ);
                    var civKey = civ.ToString();
                    return civKey;
                }
                else { return null; }
                //string civi = "Cardassian FirePower 1234";
                //return civi;
            }
        }

        public string GetOtherCivAndFirePower()
        {
            if (_civShortNameList != null && _otherAssetsDinamic != null)
            {
                string civ;
                int otherCivStrength = 0;
                var _otherAssetsLocal = _otherAssetsDinamic.ToList();
                var asset = _otherAssetsLocal.Distinct().FirstOrDefault();
                civ = asset.Owner.ShortName;

                foreach (var ha in _otherAssetsDinamic)
                {
                    _civShortNameList.Remove(civ);
                    foreach (var cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civ == cs.Owner.ShortName)
                        {
                            otherCivStrength += cs.FirePower;
                            _otherAssetsLocal.Remove(ha);
                        }
                    }
                    foreach (var ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        if (civ == ncs.Owner.ShortName)
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

                    civAndFirePower = civ + " Firepower " + otherCivStrength.ToString();
                    GameLog.Core.CombatDetails.DebugFormat("A civilization with firepower {0}", civAndFirePower);
                }
                return civAndFirePower;
            }
            else { return null; }
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

                //if (_combatUpdateTraceLocally == true)
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
