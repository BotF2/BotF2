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
using Supremacy.Orbitals;
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
        
        private List<object> _civList;
        private List<string> _civShortNameList;
        private List<string> _civFirePowerList;
        private List<Civilization> _civStatusList;
        private int _friendlyEmpireStrength;
        private int _allHostileEmpireStrength;
        [NonSerialized]
        private object _sectorString;
        private string _text;
        private int _otherCivStrength = 0;
        public CombatUpdate(int combatId, int roundNumber, bool standoff, Civilization owner, MapLocation location, IList<CombatAssets> friendlyAssets, IList<CombatAssets> hostileAssets)
        {

            //GameLog.Core.CombatDetails.DebugFormat("combatId = {0}, roundNumber = {1}, standoff = {2}, " +
            //    "Civilization owner = {3}, location = {4}, friendlyAssetsCount = {5}, hostileAssetsCount = {6}",
            _text = "Step_3199:; "
                + location.ToString()
                + " > ### combatId " + combatId
                + " Round " + roundNumber
                + ", standoff= " + standoff
                + ", civ= " + owner.CivID

                + ", CIVs friendly= " + friendlyAssets.Count
                + " vs hostile= " + hostileAssets.Count
                ;
            Console.WriteLine(_text);
            GameLog.Core.CombatDetails.DebugFormat(_text);

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            bool yesStandoff;
            if (hostileAssets.Count == 0)
            {
                CombatAssets changeSides = friendlyAssets.Last();
                _ = friendlyAssets.Remove(changeSides);
                hostileAssets.Add(changeSides);
                yesStandoff = true;
            }
            else
            {
                yesStandoff = standoff;
            }

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
                    Civilization civ = GameContext.Current.Civilizations.First();
                    _sectorString = FriendlyAssets.First().Location;

                    // Update X 25 june 2019 Added this foreach for noncombatships because other empires has it too, i considered the noncombatships weapons to be missing, so i inserted them
                    foreach (CombatUnit ncs in fa.NonCombatShips)   // only NonCombat ships 
                    {
                        civ = fa.NonCombatShips.First().Owner;
                        // Update X 25 june 2019 Total Strength instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );
                    }

                    foreach (CombatUnit cs in fa.CombatShips)   // only combat ships
                    {
                        civ = fa.CombatShips.First().Owner;
                        // Update X 25 june 2019 Total Strength instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );

                        GameLog.Core.CombatDetails.DebugFormat("adding _friendlyEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                             cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.Firepower, _friendlyEmpireStrength);
                    }



                    if (fa.Station != null)
                    {
                        civ = fa.Station.Owner;
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _friendlyEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_friendlyEmpireStrength + fa.Station.Firepower)
                                + Convert.ToDouble(fa.Station.ShieldStrength + fa.Station.HullStrength)
                                );

                        GameLog.Core.CombatDetails.DebugFormat("adding _friendlyEmpireStrength for {0}  - in total now {1}",
                            fa.Station.Name, _friendlyEmpireStrength); // fa.Source.Name, fa.Source.Design, _friendlyEmpireStrength);
                    }
                    //Civilization civ = GameContext.Current.Civilizations.First(c => c.Name == "Borg");
                    _text = _sectorString + " > Combat Durability Friendly Assets = " + _friendlyEmpireStrength;
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(new ReportEntry_CoS(civ, FriendlyAssets.First().Location, _text, "", "", SitRepPriority.Red));
                }
                //GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add

                return _friendlyEmpireStrength;
            }
        }

        public int AllHostileEmpireStrength
        {
            get
            {
                foreach (CombatAssets ha in HostileAssets)
                {
                    Civilization civ = GameContext.Current.Civilizations.First();
                    _sectorString = HostileAssets.First().Location;

                    foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        civ = ha.NonCombatShips.First().Owner;
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );
                    }

                    foreach (CombatUnit cs in ha.CombatShips)   // only combat ships
                    {
                        civ = ha.CombatShips.First().Owner;
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


                    if (ha.Station != null)
                    {
                        civ = ha.Station.Owner;
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        //_allHostileEmpireStrength += ha.Station.FirePower;
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + ha.Station.Firepower)
                                + Convert.ToDouble(ha.Station.ShieldStrength + ha.Station.HullStrength)
                                );

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0}  - in total now {1}",
                        //    ha.Station.Name, _hostileEmpireStrength); 
                    }
                    _text = _sectorString + " > Combat Durability Hostile Assets = " + _allHostileEmpireStrength;
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(new ReportEntry_CoS(civ, HostileAssets.First().Location, _text, "", "", SitRepPriority.Red));
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
                Civilization aCiv = (Civilization)civ;
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

        //public int CivFirePowers1 => GetOthersFirePower();

        public int CivFirePowers1
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

                int _otherCivStrength = 0;
                List<CombatAssets> _otherAssetsLocal = HostileAssets.ToList();

                foreach (CombatAssets ha in HostileAssets)
                {
                    foreach (CombatUnit cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civShortName == cs.Owner.ShortName)
                        {
                            // UPDATE X 25 June 2019: Do total strength instead of just firepower
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
                            // UPDATE X 25 June 2019: Do total strength instead of just firepower
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
                //works
                //_text = "Step_7345:; A civilization with firepower " + _otherCivStrength;
                //Console.WriteLine(_text);

                //GameLog.Core.CombatDetails.DebugFormat(_text);
                return _otherCivStrength; //.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
            }
        }

        public string CivFirePowers1Text
        {
            get
            {
                string val = "";
                if (CivFirePowers1 < 1) 
                    val = "0";
                else 
                    val = CivFirePowers1.ToString();

                return "Durability: " + val;
            }
        }

        public int CivFirePowers2 => GetOthersFirePower();

        public string CivFirePowers2Text
        {
            get {
                if (CivFirePowers2 < 1) return "";
                else
                return GetOthersFirePower().ToString(); 
            }
        }
        public int CivFirePowers3 => GetOthersFirePower();

        public string CivFirePowers3Text
        {
            get
            {
                if (CivFirePowers3 < 1) return "";
                else
                    return GetOthersFirePower().ToString();
            }
        }

        public int CivFirePowers4 => GetOthersFirePower();
        public string CivFirePowers4Text
        {
            get
            {
                if (CivFirePowers4 < 1) return "";
                else
                    return GetOthersFirePower().ToString();
            }
        }
        #endregion of properties for civilizations firepowers

        public int GetOthersFirePower()
        {
            if (_civShortNameList != null && _civShortNameList.Count > 0)
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
                            // UPDATE X 25 June 2019: Do total strength instead of just firepower
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
                            // UPDATE X 25 June 2019: Do total strength instead of just firepower
                            otherCivStrength = Convert.ToInt32(Convert.ToDouble(otherCivStrength + ncs.Firepower)
                                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        // UPDATE X 25 June 2019: Do total strength instead of just firepower
                        otherCivStrength = otherCivStrength + ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                        _ = _otherAssetsLocal.Remove(ha);
                    }
                }
                GameLog.Core.CombatDetails.DebugFormat("A civilization with CombatPower: {0}", otherCivStrength);
                return otherCivStrength; //.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
            }

            return 0;
        }

        public string TargetCiv1Status(Civilization us, Civilization others)
        {
            string _targetCiv1Status = GameContext.Current.DiplomacyData[us, others].Status.ToString();
            _text = "Step_3476: Status Target 1:"
                + "; Us = " + us
                + "; others = " + others
                + "; _targetCiv1Status = " + _targetCiv1Status
                ;
            Console.WriteLine(_text);
            GameLog.Core.CombatDetails.DebugFormat(_text);

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

                _text = "Step_3380:; Combat-Result ? ";
                Console.WriteLine(_text);

                foreach (CombatAssets asset in FriendlyAssets)
                {
                    if (asset.HasSurvivingAssets)
                    {
                        _text = "Step_3381:; Combat: friendlyAssets(assets.CombatShips.Count)=; " + asset.CombatShips.Count;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(assets.CombatShips.Count)={0}", asset.CombatShips.Count);
                        friendlyAssets++;
                    }
                    //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", cs.Owner.Key, civ.Owner.Key);
                    foreach (CombatUnit ship in asset.CombatShips)
                    {
                        currentCivStrength += ship.Firepower;
                        _text = "Step_3383:; Combat: added Firepower into; " + ship.Owner.Key
                            + "; for; " + ship.Source.ObjectID
                            + "; " + ship.Source.Name
                            + "; " + ship.Source.Design
                            + "; " + ship.Source.FirePower
                            ;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                        //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                    if (asset.Station != null)
                    {
                        currentCivStrength += asset.Station.Firepower;
                        _text = "Step_3385:; Combat: added Firepower into; " + asset.Station.Owner.Key
                                + "; for; " + asset.Station.Source.ObjectID
                                + "; " + asset.Station.Source.Name
                                + "; " + asset.Station.Source.Design
                                + "; " + asset.Station.Source.FirePower
                                ;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                        //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                }
                _text = "Step_3389:; Combat: friendlyAssets(Amount)=;"
                        + "; for; " + friendlyAssets
                        + "; " + _otherCivStrength
                        ;
                Console.WriteLine(_text);
                //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(Amount)={0} and otherCivStrength ={1}", friendlyAssets, _otherCivStrength);
                if (friendlyAssets == 0 || _otherCivStrength == 0)// currentCivStrength == 0)
                {
                    _text = "Step_3381:; Combat: friendlyAssets (number of involved entities)=; " + friendlyAssets;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                    return true;
                }

                foreach (CombatAssets asset in HostileAssets)
                {
                    if (asset.HasSurvivingAssets)
                    {
                        _text = "Step_3391:; Combat: hostileAssets(assets.CombatShips.Count)=; " + asset.CombatShips.Count;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets(assets.CombatShips.Count)={0}", asset.CombatShips.Count);
                        hostileAssets++;
                    }
                    foreach (CombatUnit ship in asset.CombatShips)
                    {
                        currentCivStrength += ship.Firepower;
                        _text = "Step_3393:; Combat: added Firepower into; " + ship.Owner.Key
                                + "; for; " + ship.Source.ObjectID
                                + "; " + ship.Source.Name
                                + "; " + ship.Source.Design
                                + "; " + ship.Source.FirePower
                                ;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                        //    civ.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                    if (asset.Station != null)
                    {
                        currentCivStrength += asset.Station.Firepower;
                        _text = "Step_3395:; Combat: added Firepower into; " + asset.Station.Owner.Key
                                + "; for; " + asset.Station.Source.ObjectID
                                + "; " + asset.Station.Source.Name
                                + "; " + asset.Station.Source.Design
                                + "; " + asset.Station.Source.FirePower
                                ;
                        Console.WriteLine(_text);
                    }
                }

                if (hostileAssets == 0 || _otherCivStrength == 0)//currentCivStrength == 0)
                {
                    _text = "Step_3397; Combat: hostileAssets (number of involved entities)= " + hostileAssets;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                    return true;
                }

                return hostileAssets == 0;
            }
        }
    }
}

