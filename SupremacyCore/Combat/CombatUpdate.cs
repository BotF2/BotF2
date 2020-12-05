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
        private int _otherCivStrength;
        private List<object> _civList;
        private List<string> _civShortNameList;
        private List<string> _civFirePowerList;
        private List<Civilization> _civStatusList;
        private int _friendlyEmpireStrength;
        private int _allHostileEmpireStrength;

        public CombatUpdate(int combatId, int roundNumber, bool standoff, Civilization owner, MapLocation location, IList<CombatAssets> friendlyAssets, IList<CombatAssets> hostileAssets)
        {
            GameLog.Core.Combat.DebugFormat("combatId = {0}, roundNumber = {1}, standoff = {2}, " +
                "Civilization owner = {3}, location = {4}, friendlyAssetsCount = {5}, hostileAssetsCount = {6}",
                combatId
                , roundNumber
                , standoff
                , owner.CivID
                , location.ToString()
                , friendlyAssets.Count
                , hostileAssets.Count
                );

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            bool yesStandoff;
            if (hostileAssets.Count == 0)
            {
                var changeSides = friendlyAssets.Last();
                friendlyAssets.Remove(changeSides);
                hostileAssets.Add(changeSides);
                yesStandoff = true;
            }
            else yesStandoff = standoff;
            CombatID = combatId;
            RoundNumber = roundNumber;
            IsStandoff = yesStandoff;
            OwnerID = owner.CivID;
            Location = location;
            FriendlyAssets = friendlyAssets ?? throw new ArgumentNullException(nameof(friendlyAssets));
            HostileAssets = hostileAssets ?? throw new ArgumentNullException(nameof(hostileAssets));
        }
        #region Properties for total fire power of the friends and Others (hostiles)
        public int FriendlyEmpireStrength
        {
            get
            {
                foreach (CombatAssets fa in FriendlyAssets)
                {
                    foreach (CombatUnit cs in fa.CombatShips)   // only combat ships
                    {
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );

                       GameLog.Core.CombatDetails.DebugFormat("adding _friendlyEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                            cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.Firepower, _friendlyEmpireStrength);
                    }

                    // Update X 25 june 2019 Added this foreach for noncombatships because other empires has it too, i considered the noncombatships weapons to be missing, so i inserted them
                    foreach (CombatUnit ncs in fa.NonCombatShips)   // only NonCombat ships 
                    {
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );
                    }

                    if (fa.Station != null)
                    {
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + fa.Station.Firepower)
                                + Convert.ToDouble(fa.Station.ShieldStrength + fa.Station.HullStrength)
                                );

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
                foreach (CombatAssets ha in HostileAssets)
                {
                    foreach (CombatUnit cs in ha.CombatShips)   // only combat ships
                    {
                        // _allHostileEmpireStrength += cs.FirePower;
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                        //    cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.FirePower, _hostileEmpireStrength);
                    }

                    foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );
                    }
                    if (ha.Station != null)
                    {
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        //_allHostileEmpireStrength += ha.Station.FirePower;
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + ha.Station.Firepower)
                                + Convert.ToDouble(ha.Station.ShieldStrength + ha.Station.HullStrength)
                                );

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
                CombatAssets asset = HostileAssets.FirstOrDefault();
                Civilization civOwner = asset.Owner;
                string civKey = asset.Owner.Key;
                List<object> civList = new List<object>();
                foreach (CombatAssets ha in HostileAssets)
                {
                    civList.Add(ha.Owner);
                    _ = civList.Distinct().ToList();
                }
                _ = civList.Remove(civOwner);
                _civList = civList.ToList();
                return civKey;
            }
        }

        public string OtherCivInsignia2 => GetOthersInsignia();

        public string OtherCivInsignia3 => GetOthersInsignia();

        public string OtherCivInsignia4 => GetOthersInsignia();
        #endregion

        public string GetOthersInsignia()
        {
            if (_civList.Count > 0)
            {
                object civ = _civList.FirstOrDefault();
                _ = _civList.Remove(civ);
                Civilization aCiv =(Civilization)civ;
                return aCiv.Key;
            }
            return "BlackInsignia";
        }

        #region other Civ name

        public string CivName1
        {
            get
            {
                CombatAssets asset = HostileAssets.FirstOrDefault();
                string civShortName = asset.Owner.ShortName;
                List<string> civNameList = new List<string>();
                foreach (CombatAssets ha in HostileAssets)
                {
                    civNameList.Add(ha.Owner.ShortName);
                    _ = civNameList.Distinct().ToList();
                }
                _ = civNameList.Remove(civShortName);
                _civFirePowerList = civNameList.ToList();
                return civShortName;
            }
        }

        public string CivName2 => GetOthersName();

        public string CivName3 => GetOthersName();

        public string CivName4 => GetOthersName();
        #endregion

        public string GetOthersName()
        {
            if (_civFirePowerList.Count > 0)
            {
                string civShortName = _civFirePowerList.FirstOrDefault();
                List<string> civNameList = new List<string>();
                foreach (string name in _civFirePowerList)
                {
                    civNameList.Add(name);
                    _ = civNameList.Distinct().ToList();
                }
                _ = civNameList.Remove(civShortName);
                _civFirePowerList = civNameList.ToList();
                return civShortName;
            }

            return null;
        }

        #region other Civ Status

        public string CivStatus1
        {
            get
            {
                CombatAssets asset = HostileAssets.FirstOrDefault();
                Civilization currentOwner = asset.Owner;
                List<Civilization> civOwner = new List<Civilization>();

                string _targetCiv1Status = GameContext.Current.DiplomacyData[Owner, asset.Owner].Status.ToString();
                //GameLog.Core.CombatDetails.DebugFormat("Status Target 1: Status = {2} for Owner = {0} vs others = {1}", 
                //Owner, asset.Owner, _targetCiv1Status);

                _ = new List<string>();  // list of Status
                foreach (CombatAssets ha in HostileAssets)
                {
                    civOwner.Add(ha.Owner);
                    _ = civOwner.Distinct().ToList();
                }
                _ = civOwner.Remove(currentOwner);

                _civStatusList = civOwner.ToList();
                return string.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + " " + ReturnTextOfStatus(_targetCiv1Status);
            }
        }

        public string CivStatus2 => GetStatusToOthers();

        public string CivStatus3 => GetStatusToOthers();

        public string CivStatus4 => GetStatusToOthers();
        #endregion

        public string GetStatusToOthers()
        {
            if (_civStatusList.Count > 0)
            {
                Civilization currentCiv = _civStatusList.FirstOrDefault();
                string _targetCiv1Status = GameContext.Current.DiplomacyData[Owner, currentCiv].Status.ToString();
                List<Civilization> civStatusList = new List<Civilization>();
                foreach (Civilization civilization in _civStatusList)
                {
                    civStatusList.Add(civilization);
                    _ = civStatusList.Distinct().ToList();
                }
                GameLog.Core.CombatDetails.DebugFormat("_targetCiv1Status = {0}", _targetCiv1Status);
                _ = civStatusList.Remove(currentCiv);
                _civStatusList = civStatusList.ToList();
                return string.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + " " + ReturnTextOfStatus(_targetCiv1Status);
            }

            return null;
        }

        private string ReturnTextOfStatus(string status)

        {
            switch ((ForeignPowerStatus)Enum.Parse(typeof(ForeignPowerStatus), status))
            {
                case ForeignPowerStatus.NoContact:
                    return "First Contact";
                case ForeignPowerStatus.CounterpartyIsSubjugated:
                    return "Subjugated";
                case ForeignPowerStatus.AtWar:
                    return "War";
                case ForeignPowerStatus.CounterpartyIsUnreachable:
                    return "Undefined";
                default:
                    return status;
            }
        }

        #region Properties for civ firepowers
        public string CivFirePowers1
        {
            get
            {
                CombatAssets asset = HostileAssets.FirstOrDefault();
                string civShortName = asset.Owner.ShortName;
                List<string> civNameList = new List<string>();
                foreach (CombatAssets ha in HostileAssets)
                {
                    civNameList.Add(ha.Owner.ShortName);
                    _ = civNameList.Distinct().ToList();
                }
                _ = civNameList.Remove(civShortName);
                _civShortNameList = civNameList.ToList();

                //int otherCivStrength = 0;
                List<CombatAssets> _otherAssetsLocal = HostileAssets.ToList();

                foreach (CombatAssets ha in HostileAssets)
                {
                    foreach (CombatUnit cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civShortName == cs.Owner.ShortName)
                        {
                            // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                            _otherCivStrength = Convert.ToInt32(Convert.ToDouble(_otherCivStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }
                    foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        if (civShortName == ncs.Owner.ShortName)
                        {
                            // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                            _otherCivStrength = Convert.ToInt32(Convert.ToDouble(_otherCivStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                        _otherCivStrength = _otherCivStrength + ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                        _ = _otherAssetsLocal.Remove(ha);
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("A civilization with firepower {0}", _otherCivStrength);
                return _otherCivStrength.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
            }
        }

        public string CivFirePowers2 => GetOthersFirePower();
        public string CivFirePowers3 => GetOthersFirePower();

        public string CivFirePowers4 => GetOthersFirePower();
        #endregion of proerties for civilizations firepowers

        public string GetOthersFirePower()
        {
            if (_civShortNameList.Count > 0)
            {
                string civShortName = _civShortNameList.FirstOrDefault();
                List<string> civNameList = new List<string>();
                foreach (string name in _civShortNameList)
                {
                    civNameList.Add(name);
                    _ = civNameList.Distinct().ToList();
                }
                _ = civNameList.Remove(civShortName);
                _civShortNameList = civNameList.ToList();

                int otherCivStrength = 0;
                List<CombatAssets> _otherAssetsLocal = HostileAssets.ToList();

                foreach (CombatAssets ha in HostileAssets)
                {
                    foreach (CombatUnit cs in ha.CombatShips)   // only combat ships
                    {
                        if (civShortName == cs.Owner.ShortName)
                        {
                            // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                            otherCivStrength = Convert.ToInt32(Convert.ToDouble(otherCivStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }
                    foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships
                    {
                        if (civShortName == ncs.Owner.ShortName)
                        {
                            // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                            otherCivStrength = Convert.ToInt32(Convert.ToDouble(otherCivStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        // UPDATE X 25 June 2019: Do total strenght instead of just firepower
                        otherCivStrength = otherCivStrength + ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                        _ = _otherAssetsLocal.Remove(ha);
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("A civilization with CombatPower: {0}", otherCivStrength);
                return otherCivStrength.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
            }

            return null;
        }

        public string TargetCiv1Status(Civilization us, Civilization others)
        {
            string _targetCiv1Status = GameContext.Current.DiplomacyData[us, others].Status.ToString();
            GameLog.Core.CombatDetails.DebugFormat("Status Target 1: Us = {0}, Status = {2}, others = {1}", us, others, _targetCiv1Status);

            return _targetCiv1Status;
        }

        public int CombatID { get; }

        public int RoundNumber { get; }

        public int OwnerID { get; }

        public MapLocation Location { get; }

        public Sector Sector => GameContext.Current.Universe.Map[Location];

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public IList<CombatAssets> FriendlyAssets { get; }

        public IList<CombatAssets> HostileAssets { get; }

        public bool IsStandoff { get; }
        public bool CombatUpdate_IsCombatOver // This bool opens and closes the 'close' button and the combat order buttons
        {
            get
            {
                if (IsStandoff)
                {
                    return true;
                }
                // CHANGE X
                int friendlyAssets = 0;
                int hostileAssets = 0;
                int currentCivStrength = 0;

                foreach (CombatAssets asset in FriendlyAssets)
                {
                    if (asset.HasSurvivingAssets)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(assets.CombatShips.Count)={0}", asset.CombatShips.Count);
                        friendlyAssets++;
                    }
                    //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", cs.Owner.Key, civ.Owner.Key);
                    foreach (CombatUnit ship in asset.CombatShips)
                    {
                        currentCivStrength += ship.Firepower;
                        //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                        //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                    if (asset.Station != null)
                    {
                        currentCivStrength += asset.Station.Firepower;
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(Amount)={0} and otherCivStrength ={1}", friendlyAssets, _otherCivStrength);
                if (friendlyAssets == 0 || _otherCivStrength == 0)// currentCivStrength == 0)
                {
                    GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                    return true;
                }

                foreach (CombatAssets asset in HostileAssets)
                {
                    if (asset.HasSurvivingAssets)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets(assets.CombatShips.Count)={0}", asset.CombatShips.Count);
                        hostileAssets++;
                    }
                    foreach (CombatUnit ship in asset.CombatShips)
                    {
                        currentCivStrength += ship.Firepower;
                        //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                        //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                    if (asset.Station != null)
                    {
                        currentCivStrength += asset.Station.Firepower;
                    }
                }

                if (hostileAssets == 0 || _otherCivStrength == 0)//currentCivStrength == 0)
                {
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                    return true;
                }

                return hostileAssets == 0;
            }
        }
    }
}

