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

        //private List<object> _civList;
        private List<string> _civShortNameList;
        //private List<string> _civFirePowerList;
        private List<Civilization> _civStatusList;
        //private List<Civilization> FriendlyCivs = new List<Civilization>(); // dummy - just a test if this is helpful to avoid BindingExpression path error:
        //private List<Civilization> OtherCivs = new List<Civilization>();    // dummy - just a test if this is helpful to avoid BindingExpression path error:
        private int _friendlyEmpireStrength;
        private int _allHostileEmpireStrength;
        [NonSerialized]
        private object _sectorString;
        private string _text;
        //private int _otherCivStrength = 0;

        //private Dictionary<string, int> civStrengthValues;
        private string _newline = Environment.NewLine;
        private string _civName1;
        private string _civName2;
        private string _civName3;
        private string _civName4;
        private string _civInsigniaOther1;
        private string _civInsigniaOther2;
        private string _civInsigniaOther3;
        private string _civInsigniaOther4;
        //private int _civFirePowers1;
        //private int _civFirePowers2;
        //private int _civFirePowers3;
        //private int _civFirePowers4;
        private string _civFirePowers1Text;
        private string _civFirePowers2Text;
        private string _civFirePowers3Text;
        private string _civFirePowers4Text;
        //private bool _anyAsset;
#pragma warning disable IDE0052 // Remove unread private members
        private string _combatText_CombatUpdate;
#pragma warning restore IDE0052 // Remove unread private members

        public CombatUpdate(int combatId, int roundNumber, bool standoff, Civilization owner, MapLocation location
            , IList<CombatAssets> friendlyAssets, IList<CombatAssets> hostileAssets)
        {


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

            //GameLog.Core.CombatDetails.DebugFormat("combatId = {0}, roundNumber = {1}, standoff = {2}, " +
            //    "Civilization owner = {3}, location = {4}, friendlyAssetsCount = {5}, hostileAssetsCount = {6}",
            _text = _newline; // dummy - please keep
            _text = "Step_3199:; "
                + GameEngine.LocationString(location.ToString())
                + " > ### combatId " + combatId
                + " Round " + roundNumber
                + ", standoff= " + standoff
                + ", civID= " + owner.CivID

                + ", CIVs friendly= " + friendlyAssets.Count
                + " vs hostile= " + hostileAssets.Count
                ;
            Console.WriteLine(_text);
            _combatText_CombatUpdate += _text;
            //GameLog.Core.CombatDetails.DebugFormat(_text);

            CivName_GetOthers(hostileAssets.ToList());
            CivFirePowerText_GetOthers(hostileAssets.ToList());

            //var _list_1 = FriendlyCivs;
            //var _list_2 = OtherCivs;


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
                    //Civilization pair = GameContext.Current.Civilizations.First(c => c.Name == "Borg");
                    _text = _sectorString + " > Combat Durability Friendly Assets = " + _friendlyEmpireStrength;
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(new ReportEntry_CoS(civ, FriendlyAssets.First().Location, _text, "", "", SitRepPriority.Red));
                }
                //GameContext.Current.CivilizationManagers[pair].SitRepEntries.Add

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
                        // _allHostileEmpireStrength += _cs.FirePower;
                        // Update X 25 june 2019 Total Strenght instead of just Firepower
                        _allHostileEmpireStrength = Convert.ToInt32(
                                Convert.ToDouble(_allHostileEmpireStrength + cs.Firepower)
                                + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                                * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100)))
                                );

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                        //    _cs.Source.ObjectID, _cs.Source.Name, _cs.Source.Design, _cs.FirePower, _hostileEmpireStrength);
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
        public string CivInsigniaOther1 => _civInsigniaOther1; // 
        //{
        //    get
        //    {
        //        CombatAssets asset = HostileAssets.FirstOrDefault();
        //        Civilization civOwner = asset.Owner;
        //        string civKey = asset.Owner.Key;
        //        List<object> civList = new List<object>();
        //        foreach (CombatAssets ha in HostileAssets)
        //        {
        //            civList.Add(ha.Owner);
        //            _ = civList.Distinct().ToList();
        //        }
        //        _ = civList.Remove(civOwner);
        //        _civList = civList.ToList();
        //        return civKey;
        //    }
        //}

        public string CivInsigniaOther2 => _civInsigniaOther2; // CivInsignias_GetOthers();

        public string CivInsigniaOther3 => _civInsigniaOther3; // CivInsignias_GetOthers();

        public string CivInsigniaOther4 => _civInsigniaOther4; // CivInsignias_GetOthers();
        #endregion

        //public string CivInsignias_GetOthers()
        //{
        //    if (_civList.Count > 0)
        //    {
        //        object civ = _civList.FirstOrDefault();
        //        _ = _civList.Remove(civ);
        //        Civilization aCiv = (Civilization)civ;
        //        return aCiv.Key;
        //    }
        //    return "BlackInsignia";
        //}

        #region other Civ name

        public string CivName1 => _civName1; // CivName_GetOthers();
        //{
        //    get
        //    {
        //        try
        //        {
        //            _civName1 = _hostileAssets[0].Owner.ToString();
        //        }
        //        catch
        //        {
        //            _civName1 = "xyz";
        //        }
        //        return _civName1;
        //    }
        //}
        //        CombatAssets _h_assets = HostileAssets.FirstOrDefault();
        //        string civShortName = _h_assets.Owner.ShortName;
        //        List<string> civNameList = new List<string>();
        //        foreach (CombatAssets ha in HostileAssets)
        //        {
        //            civNameList.Add(ha.Owner.ShortName);
        //            _ = civNameList.Distinct().ToList();
        //        }
        //        //foreach (var item in civNameList)
        //        //{
        //        ////OtherCivs = civNameList.ToList();
        //        //}
        //        //_ = civNameList.Remove(civShortName);
        //        _civFirePowerList = civNameList.ToList();

        //        //CivFirePowerText_GetOthers(civShortName);


        //        return civNameList[0];
        //    }
        //}

        public string CivName2 => _civName2; // CivName_GetOthers();

        public string CivName3 => _civName3; // CivName_GetOthers();

        public string CivName4 => _civName4; // CivName_GetOthers();
        #endregion

        public string CivName_GetOthers(List<CombatAssets> _hostileAssets)
        {
            //if (_civFirePowerList.Count > 0)
            //{
            //this.
            //    string civShortName = _civFirePowerList.FirstOrDefault();
            List<string> _civNameList = new List<string>();
            foreach (var item in _hostileAssets)
            {
                _civNameList.Add(item.Owner.Name);

            }
            _civNameList.Distinct().ToList();

            _civName1 = "";
            _civName2 = "";
            _civName3 = "";
            _civName4 = "";

            _civInsigniaOther1 = "BlackInsignia";
            _civInsigniaOther2 = "BlackInsignia";
            _civInsigniaOther3 = "BlackInsignia";
            _civInsigniaOther4 = "BlackInsignia";

            for (int i = 0; i < _civNameList.Count; i++)
            {
                if (_civNameList[i] != null)
                {
                    if (i == 0) { _civName1 = _civNameList[i]; _civInsigniaOther1 = _civNameList[i]; _civInsigniaOther1 = _civInsigniaOther1.Replace(" ", ""); }
                    if (i == 1) { _civName2 = _civNameList[i]; _civInsigniaOther2 = _civNameList[i]; _civInsigniaOther2 = _civInsigniaOther2.Replace(" ", ""); }
                    if (i == 2) { _civName3 = _civNameList[i]; _civInsigniaOther3 = _civNameList[i]; _civInsigniaOther3 = _civInsigniaOther3.Replace(" ", ""); }
                    if (i == 3) { _civName4 = _civNameList[i]; _civInsigniaOther4 = _civNameList[i]; _civInsigniaOther4 = _civInsigniaOther4.Replace(" ",""); }
                }
            }
            //_ = civNameList.Remove(civShortName);
            //_civFirePowerList = _civNameList.ToList();
            //return civShortName;
            //}

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
                //Owner, _h_assets.Owner, _targetCiv1Status);

                //_ = new List<string>();  // list of Status
                foreach (CombatAssets ha in HostileAssets)
                {
                    civOwner.Add(ha.Owner);
                    _ = civOwner.Distinct().ToList();
                }
                _ = civOwner.Remove(currentOwner);

                _civStatusList = civOwner.ToList();
                return string.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + ": " + ReturnTextOfStatus(_targetCiv1Status);
            }
        }

        public string CivStatus2 => CivStatus_GetOthers();

        public string CivStatus3 => CivStatus_GetOthers();

        public string CivStatus4 => CivStatus_GetOthers();
        #endregion

        public string CivStatus_GetOthers()
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
                return string.Format(ResourceManager.GetString("COMBAT_STATUS_WORD")) + ": " + ReturnTextOfStatus(_targetCiv1Status);
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

        //public int CivFirePowers1 => CivFirePower_GetOthers();

        public int CivFirePowers1
        {
            get
            {

                CombatAssets asset = HostileAssets.FirstOrDefault();
                string civShortName = asset.Owner.ShortName;
                //Dictionary<string, int> civStrengthValues;// = new Dictionary<string, int>();
                //protected Dictionary<string, int>
                //civStrengthValues = new Dictionary<string, int>();
                List<string> civNameList = new List<string>();
                foreach (CombatAssets ha in HostileAssets)
                {
                    civNameList.Add(ha.Owner.ShortName);
                    _ = civNameList.Distinct().ToList();

                    //if (!civStrengthValues.ContainsKey(ha.Owner.ShortName))
                    //{
                    //    civStrengthValues.Add(ha.Owner.ShortName, 0);
                    //}
                }
                _ = civNameList.Remove(civShortName);
                _civShortNameList = civNameList.ToList();

                //foreach (var item in civNameList)
                //{
                //    if (!civNameList.Contains(item))
                //    {
                //        civStrengthValues.Add(item, 0);
                //    }

                //}

                //#pragma warning disable CS0219 // Variable is assigned but its value is never used
                int _otherCivStrength = 0; // this is needed
                                           //#pragma warning restore CS0219 // Variable is assigned but its value is never used
                List<CombatAssets> _otherAssetsLocal = HostileAssets.ToList();

                foreach (CombatAssets ha in HostileAssets)
                {
                    foreach (CombatUnit _cs in ha.CombatShips)   // only combat ships 
                    {
                        if (civShortName == _cs.Owner.ShortName)
                        {
                            _otherCivStrength += CalculateStrength_CombatShip_in_CombatUpdate(_cs);
                            //civStrengthValues[_cs.Owner.ToString()] += CalculateStrength_CombatShip_in_CombatUpdate(_cs);

                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }

                    foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships 
                    {
                        //civStrengthValues[ncs.Owner.ToString()] += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
                        if (civShortName == ncs.Owner.ShortName)
                        {
                            _otherCivStrength += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
                            //// UPDATE X 25 June 2019: Do total strength instead of just firepower
                            //_otherCivStrength = Convert.ToInt32(Convert.ToDouble(_otherCivStrength + ncs.Firepower)
                            //    + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                            //    * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                            _ = _otherAssetsLocal.Remove(ha);
                        }
                    }

                    if (ha.Station != null)  //  station
                    {
                        if (civShortName == ha.Station.Owner.ShortName)
                        {
                            //civStrengthValues[ha.Station.Owner.ToString()] += CalculateStrength_Station_in_CombatUpdate(ha.Station);
                            _otherCivStrength += ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                            //// UPDATE X 25 June 2019: Do total strenght instead of just firepower
                            //_otherCivStrength += ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                        }
                        _ = _otherAssetsLocal.Remove(ha);
                    }
                }


                //if (CivName2)

                //    _otherCivStrength = civStrengthValues.Where(c => c.Key == CivName1).
                //var otherCivStrength = civStrengthValues[CivName1].va

                //works
                //_text = "Step_7345:; A civilization with firepower " + _otherCivStrength;
                //Console.WriteLine(_text);

                //GameLog.Core.CombatDetails.DebugFormat(_text);
                return 0; //.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
            }
        }


        private int CalculateStrength_Station_in_CombatUpdate(CombatUnit station)
        {
            // UPDATE X 25 June 2019: Do total strenght instead of just firepower
            return station.Firepower + station.HullStrength + station.ShieldStrength;
        }

        private int CalculateStrength_Ship_NonCombat_in_CombatUpdate(CombatUnit ncs)
        {
            // UPDATE X 25 June 2019: Do total strength instead of just firepower
            return Convert.ToInt32(Convert.ToDouble(ncs.Firepower)
                + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        }

        private int CalculateStrength_CombatShip_in_CombatUpdate(CombatUnit cs)
        {
            // UPDATE X 25 June 2019: Do total strength instead of just firepower
            return Convert.ToInt32(Convert.ToDouble(cs.Firepower)
                    + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                    * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        }


        public string CivFirePowers2Text => _civFirePowers2Text;
        public string CivFirePowers3Text => _civFirePowers3Text;
        public string CivFirePowers4Text => _civFirePowers4Text;
        public string CivFirePowers1Text => _civFirePowers1Text;
        //{
        //    get
        //    {
        //        CombatAssets _h_assets = HostileAssets.FirstOrDefault();
        //        string civShortName = _h_assets.Owner.ShortName;
        //        List<string> civNameList = new List<string>();

        //        int _civ1Strength = 0; // this is needed

        //        List<CombatAssets> _otherAssetsLocal = HostileAssets.ToList();

        //        foreach (CombatAssets ha in HostileAssets)
        //        {
        //            foreach (CombatUnit _cs in ha.CombatShips)   // only combat ships 
        //            {
        //                if (civShortName == _cs.Owner.ShortName)
        //                {
        //                    _civ1Strength += CalculateStrength_CombatShip_in_CombatUpdate(_cs);
        //                    //civStrengthValues[_cs.Owner.ToString()] += CalculateStrength_CombatShip_in_CombatUpdate(_cs);

        //                    _ = _otherAssetsLocal.Remove(ha);
        //                }
        //            }

        //            foreach (CombatUnit ncs in ha.NonCombatShips)   // only NonCombat ships 
        //            {
        //                //civStrengthValues[ncs.Owner.ToString()] += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
        //                if (civShortName == ncs.Owner.ShortName)
        //                {
        //                    _civ1Strength += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
        //                    //// UPDATE X 25 June 2019: Do total strength instead of just firepower
        //                    //_otherCivStrength = Convert.ToInt32(Convert.ToDouble(_otherCivStrength + ncs.Firepower)
        //                    //    + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
        //                    //    * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        //                    _ = _otherAssetsLocal.Remove(ha);
        //                }
        //            }

        //            if (ha.Station != null)  //  station
        //            {
        //                if (civShortName == ha.Station.Owner.ShortName)
        //                {
        //                    //civStrengthValues[ha.Station.Owner.ToString()] += CalculateStrength_Station_in_CombatUpdate(ha.Station);
        //                    _civ1Strength += CalculateStrength_Station_in_CombatUpdate(ha.Station);
        //                    //// UPDATE X 25 June 2019: Do total strenght instead of just firepower
        //                    //_otherCivStrength += ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
        //                }
        //                _ = _otherAssetsLocal.Remove(ha);
        //            }
        //            return string.Format(ResourceManager.GetString("COMBAT_POWER") + ": " + _civ1Strength);
        //        }

        //        //works
        //        //_text = "Step_7345:; A civilization with firepower " + _otherCivStrength;
        //        //Console.WriteLine(_text);
        //        //GameLog.Core.CombatDetails.DebugFormat(_text);

        //        return ""; //.ToString("N0") + " " + string.Format(ResourceManager.GetString("COMBAT_POWER"));
        //    }
        //}


        public int CivFirePowers2 => CivFirePower_GetOthers();

        //public string CivFirePowers2Text => CivFirePowerText_GetOthers(CivName2);

        public string CivFirePowerText_GetOthers(List<CombatAssets> _hostileAssets) //string civName)
        {
            //List<CombatAssets> _h_Assets = _hostileAssets.to;
            //if (civName == null)
            //{
            //    Console.WriteLine("Step_8880:; "
            //        + "" + GameEngine.LocationString(Location.ToString())
            //        + " > CombatID=" + CombatID
            //        + " > checking Durability > civName= null");
            //    return "";
            //}
            //else
            //{
            //    Console.WriteLine("Step_8886:; "
            //        + "" + GameEngine.LocationString(Location.ToString())
            //        + " > CombatID=" + CombatID
            //        + " > checking Durability > civName=" + civName + this.CombatID);
            //}


            //CombatAssets _h_assets = HostileAssets.FirstOrDefault();
            //string civShortName = _h_assets.Owner.ShortName;
            ////Dictionary<string, int> civStrengthValues;// = new Dictionary<string, int>();
            ////protected Dictionary<string, int>
            ////civStrengthValues = new Dictionary<string, int>();
            List<string> civNameList = new List<string>();
            foreach (CombatAssets ha in _hostileAssets)
            {
                //if (ha.Owner.ShortName == CivFirePowers1Text)
                //    continue;

                civNameList.Add(ha.Owner.ShortName);
            }
            _ = civNameList.Distinct().ToList();
            //return "";
            //}

            //    //if (!civStrengthValues.ContainsKey(ha.Owner.ShortName))
            //    //{
            //    //    civStrengthValues.Add(ha.Owner.ShortName, 0);
            //    //}
            //}
            //_ = civNameList.Remove(civShortName);
            //_civShortNameList = civNameList.ToList();

            //foreach (var item in civNameList)
            //{
            //    if (!civNameList.Contains(item))
            //    {
            //        civStrengthValues.Add(item, 0);
            //    }

            //}

            int _otherCivStrength = 0; // this is needed

            //List<CombatAssets> _otherAssetsLocal = _hostileAssets.ToList();

            foreach (var civName in civNameList)
            {
                bool _anyAsset = false;

                //for (int i = 0; i < HostileAssets.Count; i++)
                //{
                //}

                int i = 0;

                //foreach (CombatAssets ha in HostileAssets)
                //{
                //if (ha.Owner.ShortName == CivFirePowers1Text)
                //    continue;

                foreach (CombatUnit _cs in _hostileAssets[i].CombatShips)   // only combat ships 
                {
                    if (civName == _cs.Owner.ShortName)
                    {
                        _otherCivStrength += CalculateStrength_CombatShip_in_CombatUpdate(_cs);
                        //civStrengthValues[_cs.Owner.ToString()] += CalculateStrength_CombatShip_in_CombatUpdate(_cs);
                        _anyAsset = true;
                        //_ = _otherAssetsLocal.Remove(ha);
                    }
                }

                foreach (CombatUnit ncs in _hostileAssets[i].NonCombatShips)   // only NonCombat ships 
                {
                    //civStrengthValues[ncs.Owner.ToString()] += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
                    if (civName == ncs.Owner.ShortName)
                    {
                        _otherCivStrength += CalculateStrength_Ship_NonCombat_in_CombatUpdate(ncs);
                        //// UPDATE X 25 June 2019: Do total strength instead of just firepower
                        //_otherCivStrength = Convert.ToInt32(Convert.ToDouble(_otherCivStrength + ncs.Firepower)
                        //    + (Convert.ToDouble(ncs.ShieldStrength + ncs.HullStrength)
                        //    * (1 + (Convert.ToDouble(ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                        _anyAsset = true;
                        //_ = _otherAssetsLocal.Remove(ha);
                    }
                }

                if (_hostileAssets[i].Station != null)  //  station
                {
                    if (civName == _hostileAssets[i].Station.Owner.ShortName)
                    {
                        //civStrengthValues[ha.Station.Owner.ToString()] += CalculateStrength_Station_in_CombatUpdate(ha.Station);
                        _otherCivStrength += CalculateStrength_Station_in_CombatUpdate(_hostileAssets[i].Station);
                        _anyAsset = true;
                        //// UPDATE X 25 June 2019: Do total strenght instead of just firepower
                        //_otherCivStrength += ha.Station.Firepower + ha.Station.HullStrength + ha.Station.ShieldStrength;
                    }
                    //_ = _otherAssetsLocal.Remove(ha);
                }
                //}

                if (_anyAsset == true)
                {
                    _text = string.Format(ResourceManager.GetString("COMBAT_POWER")) + ": " + _otherCivStrength.ToString();
                    Console.WriteLine("Step_8881:; "
                        + "" + GameEngine.LocationString(_hostileAssets[0].Location.ToString())
                        + " > " + civName + " > " + _text)
                        ;

                    if (civName == CivName1 && CivInsigniaOther1 != "BlackInsignia")
                        _civFirePowers1Text = _text;
                    if (civName == CivName2 && CivInsigniaOther2 != "BlackInsignia")
                        _civFirePowers2Text = (_otherCivStrength - CivFirePowers1).ToString();
                    if (civName == CivName3 && CivInsigniaOther3 != "BlackInsignia")
                        _civFirePowers3Text = (_otherCivStrength - CivFirePowers1 - CivFirePowers2).ToString();
                    if (civName == CivName4 && CivInsigniaOther4 != "BlackInsignia")
                        _civFirePowers4Text = (_otherCivStrength - CivFirePowers1 - CivFirePowers2 - CivFirePowers3).ToString();

                    _anyAsset = false;

                    _otherCivStrength = 0; // reset to 0
                    return "xyz";
                }
                else
                {
                    //_text = string.Format(ResourceManager.GetString("COMBAT_POWER")) + ": " + _otherCivStrength.ToString();
                    Console.WriteLine("Step_8882:; " + civName + " > no Durability necessary in this one");
                    return "abc";
                }

                //return "ofg";

            }
            ////end of froeach civName
            //Console.WriteLine("Step_8883:; " + civName + " > no Durability necessary");
            return "def"; // return "" if there is no more civ
                          //}
        }


        //{
        //    get
        //    {
        //        string val = ResourceManager.GetString("COMBAT_POWER") + ": ";
        //        if (CivFirePowers2 == 0)
        //            val += "";
        //        else
        //            val += CivFirePowers2.ToString();

        //        if (CivName2 == null)
        //            val = "";

        //        return val;
        //    }
        //}
        public int CivFirePowers3 => CivFirePower_GetOthers();

        //public string CivFirePowers3Text => CivFirePowerText_GetOthers(CivName3);

        //public string CivFirePowers3Text
        //{
        //    get
        //    {
        //        string val = ResourceManager.GetString("COMBAT_POWER") + ": ";
        //        if (CivFirePowers3 == 0)
        //            val += "";
        //        else
        //            val += CivFirePowers3.ToString();

        //        if (CivName3 == null)
        //            val = "";

        //        return val;
        //    }
        //}

        public int CivFirePowers4 => CivFirePower_GetOthers();

        //public string CivFirePowers4Text => CivFirePowerText_GetOthers(CivName4);
        //public string CivFirePowers4Text
        //{
        //    get
        //    {
        //        string val = ResourceManager.GetString("COMBAT_POWER") + ": ";
        //        if (CivFirePowers4 == 0)
        //            val += "";
        //        else
        //            val += CivFirePowers4.ToString();

        //        if (CivName4 == null)
        //            val = "";

        //        return val;
        //    }
        //}
        #endregion of properties for civilizations firepowers

        public int CivFirePower_GetOthers()
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
        public IList<Civilization> FriendlyCivs { get; }
        public IList<Civilization> OtherCivs { get; }

        public bool IsStandoff { get; }
        public bool CombatUpdate_IsCombatOver // This bool opens and closes the 'close' button and the combat order buttons
        {
            get
            {
                //CivName_GetOthers();
                //CivFirePowerText_GetOthers();

                //currentCivStrength from FriendlyAssets
                GetCurrentCivStrength(FriendlyAssets);
                GetCurrentCivStrength(HostileAssets);

                _text = "Step_3381:; Combat: > Result ? ";
                Console.WriteLine(_text);

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
                        _text = "Step_3382:; Combat: friendlyAssets(_assets.CombatShips.Count)=; " + asset.CombatShips.Count;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(_assets.CombatShips.Count)={0}", _h_assets.CombatShips.Count);
                        friendlyAssets++;
                    }
                    //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", _cs.Owner.Key, pair.Owner.Key);
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
                        //    pair.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
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
                        //    pair.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }
                }
                _text = "Step_3388:; Combat: friendlyAssets(Amount)="
                        + "; for; " + friendlyAssets
                        //+ "; " + _otherCivStrength
                        ;
                Console.WriteLine(_text);
                //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets(Amount)={0} and otherCivStrength ={1}", friendlyAssets, _otherCivStrength);
                if (friendlyAssets == 0) // || _otherCivStrength == 0)// currentCivStrength == 0)
                {
                    _text = "Step_3391:; Combat: friendlyAssets (number of involved entities)=; " + friendlyAssets;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                    return true;
                }



                //this.CivFirePowers1Text

                //currentCivStrength from HostileAssets
                foreach (CombatAssets asset in HostileAssets)
                {
                    if (asset.HasSurvivingAssets)
                    {
                        _text = "Step_3392:; Combat: hostileAssets(_assets.CombatShips.Count)=; " + asset.CombatShips.Count;
                        Console.WriteLine(_text);
                        //GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets(_assets.CombatShips.Count)={0}", _h_assets.CombatShips.Count);
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
                        //    pair.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                    }

                    if (asset.Station != null)
                    {
                        currentCivStrength += asset.Station.Firepower;
                        _text = "Step_3395:; Combat: added Firepower into; " + asset.Station.Owner.Key
                                + "; for; " + asset.Station.Source.ObjectID
                                + "; " + asset.Station.Source.Name
                                + "; " + asset.Station.Source.Design
                                + "; FirePower=" + asset.Station.Source.FirePower
                                ;
                        Console.WriteLine(_text);
                    }
                }

                if (hostileAssets == 0) // && _otherCivStrength == 0)//currentCivStrength == 0)
                {
                    _text = "Step_3397:; Combat: hostileAssets (number of involved entities)= " + hostileAssets
                        + ", hostileAssets = 0"
                        ;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                    return true;
                }

                //return true;
                return hostileAssets == 0;
            }
        }
        //}

        private void GetCurrentCivStrength(IList<CombatAssets> _assets)
        {
            //int _assetCount;
            int _currentCivStrength = 0;
            foreach (CombatAssets asset in _assets)
            {
                if (asset.HasSurvivingAssets)
                {
                    _text = "Step_3382:; Combat: _assets(_assets.CombatShips.Count)=; " + asset.CombatShips.Count;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("Combat: _assets(_assets.CombatShips.Count)={0}", _h_assets.CombatShips.Count);
                    //_assets++;
                }
                //GameLog.Core.CombatDetails.DebugFormat("calculating empireStrengths for Ship.Owner = {0} and Empire = {1}", _cs.Owner.Key, pair.Owner.Key);
                foreach (CombatUnit ship in asset.CombatShips)
                {
                    _currentCivStrength += ship.Firepower;
                    _text = "Step_3383:; Combat: added Firepower into; " + ship.Owner.Key
                        + "; for; " + ship.Source.ObjectID
                        + "; " + ship.Source.Name
                        + "; " + ship.Source.Design
                        + "; " + ship.Source.FirePower
                        ;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                    //    pair.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                }
                if (asset.Station != null)
                {
                    _currentCivStrength += asset.Station.Firepower;
                    _text = "Step_3385:; Combat: added Firepower into; " + asset.Station.Owner.Key
                            + "; for; " + asset.Station.Source.ObjectID
                            + "; " + asset.Station.Source.Name
                            + "; " + asset.Station.Source.Design
                            + "; " + asset.Station.Source.FirePower
                            ;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("added Firepower into {0} for {1} {2} ({3}) = {4}",
                    //    pair.Owner.Key, ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, ship.FirePower);
                }
            }
            _text = "Step_3389:; Combat: _assets(Amount)="
                    + "; for; " + _assets.Count
                    //+ "; " + _otherCivStrength
                    ;
            Console.WriteLine(_text);
            //GameLog.Core.CombatDetails.DebugFormat("Combat: _assets(Amount)={0} and otherCivStrength ={1}", _assets, _otherCivStrength);
            //if (_assets == 0) // || _otherCivStrength == 0)// currentCivStrength == 0)
            //{
            //    _text = "Step_3391:; Combat: _assets (number of involved entities)=; " + _assets;
            //    Console.WriteLine(_text);
            //    //GameLog.Core.CombatDetails.DebugFormat("Combat: _assets (number of involved entities)={0}", _assets);
            //    return true;
            //}
            //return _currentCivStrength;
        }
    }
}

