// File:CombatHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Supremacy.Combat
{
    public static class CombatHelper
    {
        public static int CivFirePowers1 { get; private set; }

        public static int CivFirePowers2 { get; private set; }
        public static int CivFirePowers3 { get; private set; }
        public static int CivFirePowers4 { get; private set; }

        /// <summary>
        /// Calculates the best sector for the given <see cref="CombatAssets"/> to retreat to
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        public static Sector CalculateRetreatDestination(CombatAssets assets)
        {
            Colony nearestFriendlySystem = GameContext.Current.Universe.FindNearestOwned<Colony>(
                assets.Location,
                assets.Owner);

            IEnumerable<Sector> sectors =

                    from s in assets.Sector.GetNeighbors()
                    let distance = MapLocation.GetDistance(s.Location, nearestFriendlySystem.Location)
                    let hostileOrbitals = GameContext.Current.Universe.FindAt<Orbital>(s.Location).Where(o => o.OwnerID != assets.OwnerID && o.IsCombatant)
                    let hostileOrbitalPower = hostileOrbitals.Sum(o => o.Fire_Power_Orbital)
                    orderby hostileOrbitalPower ascending, distance descending
                    select s
                ;

            return sectors.FirstOrDefault();
        }

        public static string CivName_GetOthers(List<CombatAssets> _hostileAssets)
        {
            //if (_civFirePowerList.Count > 0)
            //{
            //this.
            //    string _civ_short_name = _civFirePowerList.FirstOrDefault();
            List<string> _civNameList = new List<string>();
            List<string> _civStatusList = new List<string>();
            List<Civilization> _civList = new List<Civilization>();
            foreach (var item in _hostileAssets)
            {
                _civNameList.Add(item.Owner.Name);
                _civList.Add(GameContext.Current.Civilizations[item.Owner.CivID]);
            }

            _civList = _civList.Distinct().ToList();
            _civNameList = _civNameList.Distinct().ToList();

            foreach (var item in _civList)
            {
                Diplomat _diplomat = Diplomat.Get(item);
                string _status = _diplomat.GetForeignPower(GameContext.Current.Civilizations[item.CivID]).DiplomacyData.Status.ToString();
                //ForeignPower _foreignPower = _diplomat.GetForeignPower(item.Owner);

                _civStatusList.Add(_status);
            }
            //_civStatusList = _civStatusList.Distinct().ToList();


            string _civName1 = "";
            string _civName2 = "";
            string _civName3 = "";
            string _civName4 = "";

            string _civStatus2 = "";
            string _civStatus3 = "";
            string _civStatus4 = "";

            string _civInsigniaOther1 = "BlackInsignia";
            string _civInsigniaOther2 = "BlackInsignia";
            string _civInsigniaOther3 = "BlackInsignia";
            string _civInsigniaOther4 = "BlackInsignia";

            for (int i = 0; i < _civNameList.Count; i++)
            {
                if (_civNameList[i] != null)
                {
                    if (i == 0) { _civName1 = _civNameList[i]; _civInsigniaOther1 = _civNameList[i]; _civInsigniaOther1 = _civInsigniaOther1.Replace(" ", ""); }
                    if (i == 1)
                    {
                        _civName2 = _civNameList[i];
                        _civInsigniaOther2 = _civNameList[i]; _civInsigniaOther2 = _civInsigniaOther2.Replace(" ", "");
                        _civStatus2 = _civStatusList[i - 1];
                        //CivStatus2 = _civStatus2;
                    }
                    if (i == 2)
                    {
                        _civName3 = _civNameList[i];
                        _civInsigniaOther3 = _civNameList[i - 1]; _civInsigniaOther3 = _civInsigniaOther3.Replace(" ", "");
                        //_civStatus3 = CivStatus3;
                    }
                    if (i == 3)
                    {
                        _civName4 = _civNameList[i];
                        _civInsigniaOther4 = _civNameList[i - 1]; _civInsigniaOther4 = _civInsigniaOther4.Replace(" ", "");
                        //_civStatus4 = CivStatus4;
                    }
                }
            }
            //_ = _civ_name_list.Remove(_civ_short_name);
            //_civFirePowerList = _civNameList.ToList();
            //return _civ_short_name;
            //}

            return null;
        }

        public static string CivFirePowerText_GetOthers(List<CombatAssets> _hostileAssets) //string _civName)
        {
            string _text = "";
            //private string _civName1;
            string _civName1 = "";
            string CivName1 = "";
            //private string _civName2;
            //private 
            string _civName2 = "";
            string CivName2 = "";
            //private string _civName3;
            //private 
            string _civName3 = "";
            string CivName3 = "";
            //private string _civName4;
            //private 
            string _civName4 = "";
            string CivName4 = "";
            //private string _civInsigniaOther1;
            //private 
            string _civInsigniaOther1 = "";
            string CivInsigniaOther1 = "";
            //private string _civInsigniaOther2;
            //private 
            string _civInsigniaOther2 = "";
            string CivInsigniaOther2 = "";
            //private string _civInsigniaOther3;
            //private 
            string _civInsigniaOther3 = "";
            string CivInsigniaOther3 = "";
            //private string _civInsigniaOther4;
            //private 
            string _civInsigniaOther4 = "";
            string CivInsigniaOther4 = "";
            //private int _civFirePowers1;
            //private int _civFirePowers2;
            //private int _civFirePowers3;
            //private int _civFirePowers4;
            //private string _civFirePowers1Text;
            //private 
            string _civFirePowers1Text = "";
            string CivFirePowers1Text = "";
            //private string _civFirePowers2Text;
            //private 
            string _civFirePowers2Text = "";
            string CivFirePowers2Text = "";
            //private string _civFirePowers3Text;
            //private 
            string _civFirePowers3Text = "";
            string CivFirePowers3Text = "";
            //private string _civFirePowers4Text;
            //private 
            string _civFirePowers4Text = "";
            string CivFirePowers4Text = "";
            //private string _civStatus2;
            //private 
            string _civStatus2 = "";
            string CivStatus2 = "";
            //private string _civStatus3;
            //private 
            string _civStatus3 = "";
            string CivStatus3 = "";
            //private string _civStatus4;
            //private 
            string _civStatus4 = "";
            string CivStatus4 = "";
            //List<CombatAssets> _h_Assets = _hostileAssets.to;
            //if (_civName == null)
            //{
            //    Console.WriteLine("Step_8880:; "
            //        + "" + GameEngine.LocationString(Location.ToString())
            //        + " > CombatID=" + CombatID
            //        + " > checking Durability > _civName= null");
            //    return "";
            //}
            //else
            //{
            //    Console.WriteLine("Step_8886:; "
            //        + "" + GameEngine.LocationString(Location.ToString())
            //        + " > CombatID=" + CombatID
            //        + " > checking Durability > _civName=" + _civName + this.CombatID);
            //}


            //CombatAssets _h_assets = HostileAssets.FirstOrDefault();
            //string _civ_short_name = _h_assets.Owner.ShortName;
            Dictionary<string, int> civStrengthValues = new Dictionary<string, int>();
            ////protected Dictionary<string, int>
            ////civStrengthValues = new Dictionary<string, int>();
            List<string> civNameList = new List<string>();
            foreach (CombatAssets ha in _hostileAssets)
            {
                //if (_ha.Owner.ShortName == CivFirePowers1Text)
                //    continue;

                civNameList.Add(ha.Owner.ShortName);
            }
            civNameList = civNameList.Distinct().ToList();
            //return "";
            //}

            //    //if (!civStrengthValues.ContainsKey(_ha.Owner.ShortName))
            //    //{
            //    //    civStrengthValues.Add(_ha.Owner.ShortName, 0);
            //    //}
            //}
            //_ = _civ_name_list.Remove(_civ_short_name);
            //_civShortNameList = _civ_name_list.ToList();

            //foreach (var item in _civ_name_list)
            //{
            //    if (!_civ_name_list.Contains(item))
            //    {
            //        civStrengthValues.Add(item, 0);
            //    }

            //}

            int _right_side_Strength = 0; // this is needed

            //List<CombatAssets> _otherAssetsLocal = _hostileAssets.ToList();

            foreach (var _civName in civNameList)
            {
                bool _anyAsset = false;

                //for (int i = 0; i < HostileAssets.Count; i++)
                //{
                //}

                int i = 0;

                //foreach (CombatAssets _ha in HostileAssets)
                //{
                //if (_ha.Owner.ShortName == CivFirePowers1Text)
                //    continue;

                foreach (CombatUnit _cs in _hostileAssets[i].CombatShips)   // only combat ships 
                {
                    if (_civName == _cs.Owner.ShortName)
                    {
                        //Debugger.Break();
                        _right_side_Strength += CalculateStrength_CombatShip_in_CombatHelper(_cs);
                        //civStrengthValues[_cs.Owner.ToString()] += CalculateStrength_CombatShip_in_CombatHelper(_cs);
                        _anyAsset = true;
                        //_ = _otherAssetsLocal.Remove(_ha);
                    }
                }

                foreach (CombatUnit _ncs in _hostileAssets[i].NonCombatShips)   // only NonCombat ships 
                {
                    civStrengthValues[_ncs.Owner.ToString()] += Convert.ToInt32(Convert.ToDouble(_right_side_Strength + _ncs.Firepower)
                            + (Convert.ToDouble(_ncs.ShieldStrength + _ncs.HullStrength)
                            * (1 + (Convert.ToDouble(_ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                    if (_civName == _ncs.Owner.ShortName)
                    {
                        Debugger.Break();
                        _right_side_Strength += Convert.ToInt32(Convert.ToDouble(_right_side_Strength + _ncs.Firepower)
                            + (Convert.ToDouble(_ncs.ShieldStrength + _ncs.HullStrength)
                            * (1 + (Convert.ToDouble(_ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));


                        //// UPDATE X 25 June 2019: Do total strength instead of just firepower
                        //_right_side_Strength = Convert.ToInt32(Convert.ToDouble(_right_side_Strength + _ncs.Firepower)
                        //    + (Convert.ToDouble(_ncs.ShieldStrength + _ncs.HullStrength)
                        //    * (1 + (Convert.ToDouble(_ncs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
                        _anyAsset = true;
                        //_ = _otherAssetsLocal.Remove(_ha);
                    }
                }

                if (_hostileAssets[i].Station != null)  //  station
                {
                    if (_civName == _hostileAssets[i].Station.Owner.ShortName)
                    {
                        //civStrengthValues[_ha.Station.Owner.ToString()] += CalculateStrength_Station_in_CombatUpdate(_ha.Station);
                        Debugger.Break();
                        //_right_side_Strength += CalculateStrength_Station_in_CombatUpdate(_hostileAssets[i].Station);
                        _anyAsset = true;
                        //// UPDATE X 25 June 2019: Do total strenght instead of just firepower
                        //_right_side_Strength += _ha.Station.Firepower + _ha.Station.HullStrength + _ha.Station.ShieldStrength;
                    }
                    //_ = _otherAssetsLocal.Remove(_ha);
                }
                //}

                if (_anyAsset == true)
                {
                    _text = string.Format(ResourceManager.GetString("COMBAT_POWER")) + ": " /*+ _right_side_Strength.ToString()*/;
                    //Console.WriteLine("Step_8881:; "
                    //    + "" + GameEngine.LocationString(_hostileAssets[0].Location.ToString())
                    //    + " cUpda > " + _civName
                    //    + " > " + _text + _right_side_Strength.ToString()
                    //    );

                    CivName1 = civNameList[0];
                    CivInsigniaOther1 = CivName1;

                    if (civNameList.Count > 1)
                    {
                        CivName2 = civNameList[1];
                        CivInsigniaOther2 = CivName2;
                    }

                    if (civNameList.Count > 2)
                    {
                        CivName3 = civNameList[2];
                        CivInsigniaOther3 = CivName3;
                    }

                    if (civNameList.Count > 3)
                    {
                        CivName4 = civNameList[3];
                        CivInsigniaOther4 = CivName4;
                    }




                    //_civFirePowers_x_Text
                    if (_civName == CivName1 && CivInsigniaOther1 != "BlackInsignia")
                        _civFirePowers1Text = _text + _right_side_Strength.ToString();
                    if (_civName == CivName2 && CivInsigniaOther2 != "BlackInsignia")
                        _civFirePowers2Text = _text + (_right_side_Strength - CivFirePowers1).ToString();
                    if (_civName == CivName3 && CivInsigniaOther3 != "BlackInsignia")
                        _civFirePowers3Text = _text + (_right_side_Strength - CivFirePowers1 - CivFirePowers2).ToString();
                    if (_civName == CivName4 && CivInsigniaOther4 != "BlackInsignia")
                        _civFirePowers4Text = _text + (_right_side_Strength - CivFirePowers1 - CivFirePowers2 - CivFirePowers3).ToString();

                    _anyAsset = false;

                    _right_side_Strength = 0; // reset to 0
                    return "xyz";
                }
                else
                {
                    //_text = string.Format(ResourceManager.GetString("COMBAT_POWER")) + ": " + _right_side_Strength.ToString();
                    Console.WriteLine("Step_8882:; cUpda" + _civName + " > no Durability necessary in this one - no visible Combat Screen");
                    return "abc";
                }

                //return "ofg";

            }
            ////end of froeach _civName
            //Console.WriteLine("Step_8883:; " + _civName + " > no Durability necessary");
            return "def"; // return "" if there is no more civ
                          //}
        }


        private static int CalculateStrength_CombatShip_in_CombatHelper(CombatUnit cs)
        {
            // UPDATE X 25 June 2019: Do total strength instead of just firepower
            return Convert.ToInt32(Convert.ToDouble(cs.Firepower)
                    + (Convert.ToDouble(cs.ShieldStrength + cs.HullStrength)
                    * (1 + (Convert.ToDouble(cs.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))));
        }


        public static List<CombatAssets> GetCombatAssets(MapLocation location)
        {
            Dictionary<Civilization, CombatAssets> assets = new Dictionary<Civilization, CombatAssets>();
            List<CombatAssets> results = new List<CombatAssets>();
            Dictionary<Civilization, CombatUnit> units = new Dictionary<Civilization, CombatUnit>();
            Sector sector = GameContext.Current.Universe.Map[location];
            List<Fleet> engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            TakeSidesAssets ExposedAssets = new TakeSidesAssets(location); // part of an altering of collection while using, in TakeSidesAssets.cs line 34 and to GameEngine,cs line 1288
            int maxOppostionScanStrength = ExposedAssets.MaxOppositionScanStrengh;
            List<Fleet> oppositionFleets = ExposedAssets.OppositionFleets;

            if ((oppositionFleets.Count == 0) && (sector.Station == null))
            {
                return results;
            }
            else
            {
                IEnumerable<Ship> _ships = from p in engagingFleets.SelectMany(l => l.Ships) select p;

                foreach (Ship ship in _ships.Distinct().ToList())
                {
                    CombatUnit unit = new CombatUnit(ship);
                    // works   GameLog.Core.Combat.DebugFormat("maxOppostionScanStrength =  {0}", maxOppostionScanStrength);
                    // GameLog.Core.Combat.DebugFormat("!ship! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);

                    // seems to be no difference between ship and unit
                    //GameLog.Core.Combat.DebugFormat("!unit! {0} {1} ({2}) at {3} is Camouflaged {4}, Cloaked {5}",
                    //    ship.ObjectID, ship.Name, ship.DesignName, ship.Location.ToString(), ship.IsCamouflaged, ship.IsCloaked);
                    if (ship.IsCamouflaged && (unit.CamouflagedStrength >= maxOppostionScanStrength))
                    {
                        //if (ship.ShipType.ToString() == "Spy" || ship.ShipType.ToString() == "Diplomatic")
                        //{
                        //    if (Ships.Where(sh => sh.OwnerID != 6).Any())
                        //        continue;
                        //}

                        continue; // skip over ships camaouflaged better than best scan strength
                    }
                    if (sector.System != null && ship.Owner != sector.Owner && sector.Owner != null && sector.System.Colony != null && GameContext.Current.Universe.HomeColonyLookup[sector.Owner] == sector.System.Colony && !DiplomacyHelper.Status_AtWar(ship.Owner, sector.Owner))
                    {
                        //GameLog.Core.Combat.DebugFormat("Home Colony = {0}, Not at war ={1}",
                        //GameContext.Current.Universe.HomeColonyLookup[sector.Owner] == sector.System.Colony, !DiplomacyHelper.Status_AtWar(ship.Owner, sector.Owner));

                        continue; // for home worlds you need to declare war to get combat
                    }
                    if (!assets.ContainsKey(ship.Owner))
                    {
                        assets[ship.Owner] = new CombatAssets(ship.Owner, location);
                    }
                    if (ship.IsCombatant)
                    {
                        assets[ship.Owner].CombatShips.Add(new CombatUnit(ship));

                        if (ship.IsCamouflaged)
                        {
                            unit.Decamouflage();
                            ship.IsCamouflaged = false; // do we need an updater here to unit.Decamouflage() reset ship.IsCamouflaged? - so far it does not appear to do this in the GameLog below.

                            // is this SitRep still relevant than change it to new ReportEntry_CoS
                            //GameContext.Current.CivilizationManagers[ship.Owner].SitRepEntries.Add(new DeCamouflagedSitRepEntry(ship, maxOppostionScanStrength));

                            GameLog.Core.Combat.DebugFormat("CombatShip Decamouflage - max scan ={0}, unit Camouflage = {1} for {2} {3} {4} at {5} Is Camouflaged? {6}",
                                maxOppostionScanStrength, unit.CamouflagedStrength, unit.Source.ObjectID, unit.Source.Name, unit.Source.Design, location.ToString(), ship.IsCamouflaged.ToString());
                        }
                    }
                    else
                    {
                        assets[ship.Owner].NonCombatShips.Add(new CombatUnit(ship));

                        if (ship.IsCamouflaged)
                        {
                            unit.Decamouflage();
                            ship.IsCamouflaged = false;

                            // is this SitRep still relevant than change it to new ReportEntry_CoS
                            //GameContext.Current.CivilizationManagers[ship.Owner].SitRepEntries.Add(new DeCamouflagedSitRepEntry(ship, maxOppostionScanStrength));

                            GameLog.Core.Combat.DebugFormat("NonCombatShip - max scan ={0}, unit Camouflage ={1} for{2} {3} {4} at {5}",
                                    maxOppostionScanStrength, unit.CamouflagedStrength, unit.Source.ObjectID, unit.Source.Name, unit.Source.Design, location.ToString());
                        }
                    }
                }
            }

            if (sector.Station != null)
            {
                // needed once again for avoiding crash while finish a station build
                if (sector.Station.TurnCreated == GameContext.Current.TurnNumber || sector.Station.TurnCreated == 1)
                {
                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) just build in turn {3} and NOT taking part in this combat for avoid crashes on *uncomplete* stations"
                        , sector.Station.ObjectID, sector.Station.Name, sector.Station.Design, sector.Station.TurnCreated);
                    goto DoNotIncludeStationsNotFullyBuilded;
                }
                Civilization owner = sector.Station.Owner;

                if (!assets.ContainsKey(owner))
                {
                    assets[owner] = new CombatAssets(owner, location);
                }

                assets[owner].Station = new CombatUnit(sector.Station);

            DoNotIncludeStationsNotFullyBuilded:;
            }

            results.AddRange(assets.Values);

            return results;
        }

        public static List<CombatAssets> GetCamouflageAssets(MapLocation location)
        {
            Dictionary<Civilization, CombatAssets> assets = new Dictionary<Civilization, CombatAssets>();
            List<CombatAssets> results = new List<CombatAssets>();
            //var units = new Dictionary<Civilization, CombatUnit>();
            //var sector = GameContext.Current.Universe.Map[location];
            List<Fleet> engagingFleets = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            TakeSidesAssets ExposedAssets = new TakeSidesAssets(location);
            //var maxOppostionScanStrength = ExposedAssets.MaxOppositionScanStrengh;
            //var oppositionFleets = ExposedAssets.OppositionFleets;

            //if (sector.Station == null)
            //{
            //    return results;
            //}

            //else
            //{
            IEnumerable<Ship> _ships = from p in engagingFleets.SelectMany(l => l.Ships) select p;

            foreach (Ship ship in _ships.Distinct().ToList())
            {
                CombatUnit unit = new CombatUnit(ship);

                if (!ship.IsCamouflaged) // && (unit.CamouflagedStrength >= maxOppostionScanStrength))
                {
                    continue; // skip over ships not camaouflaged better than best scan strength
                }
                if (!assets.ContainsKey(ship.Owner))
                {
                    assets[ship.Owner] = new CombatAssets(ship.Owner, location);
                }
                if (ship.IsCombatant)
                {
                    assets[ship.Owner].CombatShips.Add(new CombatUnit(ship));
                }
                else
                {
                    assets[ship.Owner].NonCombatShips.Add(new CombatUnit(ship));
                }
                //}
            }

            results.AddRange(assets.Values);

            return results;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s will enage
        /// each other in combat
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool WillEngage(Civilization firstCiv, Civilization secondCiv)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }
            if (firstCiv == secondCiv)
            {
                return false;
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                GameLog.Core.Combat.DebugFormat("no diplomacyData !! - WillEngage = FALSE");
                return true;
            }

            switch (diplomacyData.Status) // see WillFightAlongside below
            {
                case ForeignPowerStatus.Self:
                //case ForeignPowerStatus.Peace:
                case ForeignPowerStatus.Friendly:
                case ForeignPowerStatus.Affiliated:
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s is not at war
        /// each other in combat
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool AreNotAtWar(Civilization firstCiv, Civilization secondCiv)
        {
            bool notWar = true;
            if (firstCiv == null)
            {
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }
            if (firstCiv == secondCiv)
            {
                notWar = true;
            }
            // do not call GetTargetOne or Two here!, use in ChoseTarget
            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                //GameLog.Core.Combat.DebugFormat("no diplomacyData !! - WillEngage = FALSE");
                return true;
            }
            else if (diplomacyData.Status == ForeignPowerStatus.AtWar)
            {
                return false;
            }

            return notWar;
        }

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/>s will
        /// fight alongside each other
        /// </summary>
        /// <param name="firstCiv"></param>
        /// <param name="secondCiv"></param>
        /// <returns></returns>
        public static bool WillFightAlongside(Civilization firstCiv, Civilization secondCiv)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException(nameof(firstCiv));
            }
            if (secondCiv == null)
            {
                throw new ArgumentNullException(nameof(secondCiv));
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[firstCiv, secondCiv];
            if (diplomacyData == null)
            {
                return false;
            }
            if (firstCiv != secondCiv)
            {
                switch (diplomacyData.Status)
                {
                    case ForeignPowerStatus.Affiliated:
                    case ForeignPowerStatus.Allied:
                    case ForeignPowerStatus.OwnerIsMember:
                    case ForeignPowerStatus.CounterpartyIsMember:
                        return true;
                }
            }
            // TODO: How should we handle war partners?

            return false;
        }

        public static CombatOrders GenerateBlanketOrders(CombatAssets assets, CombatOrder order)
        {
            const bool _generateBlanketOrdersTracing = true;
            Civilization owner = assets.Owner;
            CombatOrders orders = new CombatOrders(owner, assets.CombatID);
            string _text = "";

            foreach (CombatUnit ship in assets.CombatShips)  // CombatShips
            {
                orders.SetOrder(ship.Source, order);

                if (_generateBlanketOrdersTracing)// && order != CombatOrder.Hail) // reduces lines especially on starting (all ships starting with Hail)
                {
                    _text = "Step_3561:; "
                        + "" + GameEngine.LocationString(ship.Source.Location.ToString())
                        + " > cHelper     CombUnits > "
                        + order + " for "

                        + " ( " + ship.Source.Design
                        + " ) "
                        + ship.Source.ObjectID
                        + " ) " + ship.Source.Name


                        ;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("{0} {1} {2} is ordered to {3}",
                    //    ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                }
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                orders.SetOrder(ship.Source, (order == CombatOrder.Engage) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Rush) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Transports) ? CombatOrder.Standby : order);
                orders.SetOrder(ship.Source, (order == CombatOrder.Formation) ? CombatOrder.Standby : order);
                if (_generateBlanketOrdersTracing)// && order != CombatOrder.Hail) // reduces lines especially on starting (all ships starting with Hail)
                {
                    _text = "Step_3562:; "
                        + "" + GameEngine.LocationString(ship.Source.Location.ToString())
                        + " > cHelper Non-CombUnits > "
                        + order + " for "
                        + ship.Source.ObjectID
                        + " " + ship.Source.Name
                        + " ( " + ship.Source.Design
                        + " ) "

                        ;
                    Console.WriteLine(_text);
                    //GameLog.Core.CombatDetails.DebugFormat("{0} {1} {2} is ordered to {3}",
                    //    ship.Source.ObjectID, ship.Source.Name, ship.Source.Design, order);
                }
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                orders.SetOrder(assets.Station.Source, (order == CombatOrder.Retreat) ? CombatOrder.Engage : order);
                //if (_generateBlanketOrdersTracing == true)
                //{
                //    //GameLog.Core.Combat.DebugFormat("{0} is ordered to {1}", assets.Station.Source, order);
                //}
            }

            //foreach (var _order in orders)
            //{
            //    _text = _order.ToString()
            //        //_order.ToString()
            //        ;
            //    Console.WriteLine(_text);
            //}

            return orders;
        }

        public static CombatTargetPrimaries GenerateBlanketTargetPrimary(CombatAssets assets, Civilization target) // the orbital and it's target civ from combat window
        {
            Civilization owner = assets.Owner;
            CombatTargetPrimaries targetOne = new CombatTargetPrimaries(owner, assets.CombatID);
            string _text = "";

            foreach (CombatUnit ship in assets.CombatShips)  // all CombatShips  of civ should get this target in the CombatTargetPrimaries dictionary
            {
                if (target.CivID == -1 || target == null)
                {
                    targetOne.SetTargetOneCiv(ship.Source, GetDefault_OnlyReturnFireCiv_888());
                    _text = "Step_5632:; CombatAsset ship = " + ship.Owner.Key
                        + "" + ship.Description
                        + ", DummyTarget= " + GetDefault_OnlyReturnFireCiv_888()

                        ;

                    GameLog.Core.CombatDetails.DebugFormat("CombatAsset ship = {0} {1} Dummy Target = {2}", ship.Description, ship.Owner.Key, GetDefault_OnlyReturnFireCiv_888().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(ship.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("Combat ship = {0} {1} real Target = {2}", ship.Description, ship.Owner.Key, target.Key);
                }
                //GameLog.Core.CombatDetails.DebugFormat("Combat Ship  {0}: target = {2}", ship.Name, ship.Owner, target.Key);
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                if (target.CivID == -1)
                {
                    targetOne.SetTargetOneCiv(ship.Source, GetDefault_OnlyReturnFireCiv_888());
                    GameLog.Core.CombatDetails.DebugFormat("NonCombat ship = {0} {1} Dummy Target = {2}", ship.Description, ship.Owner.Key, GetDefault_OnlyReturnFireCiv_888().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(ship.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("NonCombat ship = {0} {1} Real Target = {2}", ship.Description, ship.Owner.Key, target.Key);
                }
            }

            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                if (target.CivID == -1)
                {
                    targetOne.SetTargetOneCiv(assets.Station.Source, GetDefault_OnlyReturnFireCiv_888());
                    GameLog.Core.CombatDetails.DebugFormat("Station = {0} {1} Dummy Target = {2}", assets.Station.Description, assets.Station.Owner.Key, GetDefault_OnlyReturnFireCiv_888().Key);
                }
                else
                {
                    targetOne.SetTargetOneCiv(assets.Station.Source, target);
                    GameLog.Core.CombatDetails.DebugFormat("Station {0} {1} with Real target = {2}", assets.Station.Name, assets.Station.Owner.Key, target.Key);
                }
            }
            return targetOne;
        }

        public static CombatTargetSecondaries GenerateBlanketTargetSecondary(CombatAssets assets, Civilization target)
        {
            Civilization owner = assets.Owner;
            CombatTargetSecondaries targetTwo = new CombatTargetSecondaries(owner, assets.CombatID);

            foreach (CombatUnit ship in assets.CombatShips)  // all CombatShips get target
            {
                if (target.CivID == -1 || target == null) // UPDATE X 04 july 2019 manualy re-do update from ken, to fix targetTwo bug
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, GetDefault_OnlyReturnFireCiv_888());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, target);
                }

                GameLog.Core.CombatDetails.DebugFormat("Combat Ship {0} with target = {2}", ship.Name, ship.Owner, target.Key);
            }

            foreach (CombatUnit ship in assets.NonCombatShips) // NonCombatShips (decided by carrying weapons)
            {
                if (target.CivID == -1)
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, GetDefault_OnlyReturnFireCiv_888());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(ship.Source, target);
                }
            }
            if (assets.Station != null && assets.Station.Owner == owner)  // Station (only one per Sector possible)
            {
                if (target.CivID == -1)
                {
                    targetTwo.SetTargetTwoCiv(assets.Station.Source, GetDefault_OnlyReturnFireCiv_888());
                }
                else
                {
                    targetTwo.SetTargetTwoCiv(assets.Station.Source, target);
                }
            }
            return targetTwo;
        }

        public static double ComputeGroundDefenseMultiplier(Colony colony)
        {
            if (colony == null)
            {
                return 0;
            }

            GameLog.Core.SystemAssaultDetails.DebugFormat("ComputeGroundDefenseMultiplier...");
            //GameLog.Core.SystemAssaultDetails.DebugFormat("Colony={0}, ComputeGroundDefenseMultiplier={1}",
            //    colony.Name,
            //    Math.Max(
            //    0.1,
            //    1.0 + (0.01 * colony.Buildings
            //                       .Where(o => o.IsActive)
            //                       .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
            //                       .Sum(b => b.Amount))));

            return Math.Max(
                0.1,
                1.0 + (0.01 * colony.Buildings
                                   .Where(o => o.IsActive)
                                   .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundDefense))
                                   .Sum(b => b.Amount)));
        }

        public static int ComputeGroundCombatStrength(Civilization civ, MapLocation location, int population)
        {
            StarSystem system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
            {
                return 0;
            }

            Colony colony = system.Colony;
            if (colony == null)
            {
                return 0;
            }

            int localGroundCombatBonus = 0;

            if (colony.OwnerID == civ.CivID)
            {
                localGroundCombatBonus = colony.Buildings
                    .Where(o => o.IsActive)
                    .SelectMany(b => b.BuildingDesign.GetBonuses(BonusType.PercentGroundCombat))
                    .Sum(b => b.Amount);
            }

            double raceMod = Math.Max(0.1, Math.Min(2.0, civ.Race.GroundCombatEffectiveness));
            double weaponTechMod = 1.0 + (0.1 * GameContext.Current.CivilizationManagers[civ].Research.GetTechLevel(TechCategory.Weapons));
            double localGroundCombatMod = 1.0 + (0.01 * localGroundCombatBonus);

            double result = population * weaponTechMod * raceMod * localGroundCombatMod;

            GameLog.Core.SystemAssaultDetails.DebugFormat("Colony = {5}: raceMod = {0}, weaponTechMod = {1}, localGroundCombatMod = {2}, population = {3}, result of GroundCombatStrength (in total) = {4} ", raceMod, weaponTechMod, localGroundCombatMod, population, result, colony.Name);

            return (int)result;
        }

        public static Civilization GetDefault_OnlyReturnFireCiv_888()
        {
            // The 'never clicked a target button' target civilization for a human player so was it a hail order or an engage order?

            return new Civilization
            {
                ShortName = "Only Return Fire"
                ,
                CivID = 888 // CHANGE X PROBLEM this 778 will always be used for anyones TargetTWO. Bug.
                ,
                Key = "Only Return Fire"
                //,TargetCiv1Status = ""
            };
        }




    }
}


