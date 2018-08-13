// TechDatabase.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Tech
{
    /// <summary>
    /// Represents the database of all tech objects available in the game.
    /// </summary>
    [Serializable]
    public sealed class TechDatabase : IEnumerable<TechObjectDesign>, IDeserializationCallback
    {
        private const string XmlFilePath = "Resources/Data/TechObjectDatabase.xml";

        private readonly TechObjectDesignMap<BuildingDesign> _buildingDesigns;
        private readonly TechObjectDesignMap<ShipDesign> _shipDesigns;
        private readonly TechObjectDesignMap<StationDesign> _stationDesigns;
        private readonly TechObjectDesignMap<ProductionFacilityDesign> _productionFacilityDesigns;
        private readonly TechObjectDesignMap<OrbitalBatteryDesign> _orbitalBatteryDesigns;
        private readonly TechObjectDesignMap<ShipyardDesign> _shipyardDesigns;
        private int _nextDesignId;

        [NonSerialized]
        private Dictionary<string, int> _designIdMap;

        /// <summary>
        /// Gets the <see cref="Supremacy.Tech.TechObjectDesign"/> with the specified design id.
        /// </summary>
        /// <value>The <see cref="Supremacy.Tech.TechObjectDesign"/> with the specified design id.</value>
        public TechObjectDesign this[int designId]
        {
            get
            {
                if (_productionFacilityDesigns.Contains(designId))
                    return _productionFacilityDesigns[designId];
                if (_buildingDesigns.Contains(designId))
                    return _buildingDesigns[designId];
                if (_shipyardDesigns.Contains(designId))
                    return _shipyardDesigns[designId];
                if (_shipDesigns.Contains(designId))
                    return _shipDesigns[designId];
                if (_stationDesigns.Contains(designId))
                    return _stationDesigns[designId];
                if (_orbitalBatteryDesigns.Contains(designId))
                    return _orbitalBatteryDesigns[designId];
                return null;
            }
        }

        public TechObjectDesign this[string key]
        {
            get
            {
                if (DesignIdMap.ContainsKey(key))
                    return this[DesignIdMap[key]];
                return null;
            }
        }

        /// <summary>
        /// Gets the subset of building designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The building designs.</value>
        public TechObjectDesignMap<BuildingDesign> BuildingDesigns
        {
            get { return _buildingDesigns; }
        }

        /// <summary>
        /// Gets the subset of shipyard designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The shipyard designs.</value>
        public TechObjectDesignMap<ShipyardDesign> ShipyardDesigns
        {
            get { return _shipyardDesigns; }
        }

        /// <summary>
        /// Gets the subset of ship designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The ship designs.</value>
        public TechObjectDesignMap<ShipDesign> ShipDesigns
        {
            get { return _shipDesigns; }
        }

        /// <summary>
        /// Gets the subset of station designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The station designs.</value>
        public TechObjectDesignMap<StationDesign> StationDesigns
        {
            get { return _stationDesigns; }
        }

        /// <summary>
        /// Gets the subset of facility designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The facility designs.</value>
        public TechObjectDesignMap<ProductionFacilityDesign> ProductionFacilityDesigns
        {
            get { return _productionFacilityDesigns; }
        }

        /// <summary>
        /// Gets the subset of orbital battery designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The orbital battery designs.</value>
        public TechObjectDesignMap<OrbitalBatteryDesign> OrbitalBatteryDesigns
        {
            get { return _orbitalBatteryDesigns; }
        }

        /// <summary>
        /// Gets the dictionary that maps the designs' unique keys to design IDs.
        /// </summary>
        /// <value>The design ID map.</value>
        public IDictionary<string, int> DesignIdMap
        {
            get { return _designIdMap; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechDatabase"/> class.
        /// </summary>
        public TechDatabase()
        {
            _buildingDesigns = new TechObjectDesignMap<BuildingDesign>();
            _shipyardDesigns = new TechObjectDesignMap<ShipyardDesign>();
            _shipDesigns = new TechObjectDesignMap<ShipDesign>();
            _stationDesigns = new TechObjectDesignMap<StationDesign>();
            _productionFacilityDesigns = new TechObjectDesignMap<ProductionFacilityDesign>();
            _orbitalBatteryDesigns = new TechObjectDesignMap<OrbitalBatteryDesign>();
            _designIdMap = new Dictionary<string, int>();
        }

        /// <summary>
        /// Generates a new design ID.
        /// </summary>
        /// <returns>The design ID.</returns>
        public int GetNewDesignID()
        {
            return _nextDesignId++;
        }

        /// <summary>
        /// Loads the tech database from XML.
        /// </summary>
        /// <returns>The tech database.</returns>
        public static TechDatabase Load()
        {
            var schemas = new XmlSchemaSet();
            var db = new TechDatabase();
            var xmlDoc = new XmlDocument();
            var designIdMap = new Dictionary<string, int>();

            schemas.Add(
                "Supremacy:Supremacy.xsd",
                ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
            schemas.Add(
                "Supremacy:TechObjectDatabase.xsd",
                ResourceManager.GetResourcePath("Resources/Data/TechObjectDatabase.xsd"));

            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
            xmlDoc.Schemas.Add(schemas);
            xmlDoc.Validate(ValidateXml);

            // ProductionFacilities

            XmlElement xmlFacilities = xmlDoc.DocumentElement["ProductionFacilities"];

            foreach (XmlElement xmlFacility in xmlFacilities.GetElementsByTagName("ProductionFacility"))
            {
                ProductionFacilityDesign facility = new ProductionFacilityDesign(xmlFacility);
                facility.DesignID = db.GetNewDesignID();
                designIdMap[facility.Key] = facility.DesignID;
                db.ProductionFacilityDesigns.Add(facility);
            }
            foreach (XmlElement xmlFacility in xmlFacilities.GetElementsByTagName("ProductionFacility"))
            {
                string sourceKey = xmlFacility.GetAttribute("Key");
                if (xmlFacility["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in 
                        xmlFacility["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.ProductionFacilityDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.ProductionFacilityDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.ProductionFacilityDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlFacility["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlFacility["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey)
                                && db.ProductionFacilityDesigns.Contains(designIdMap[prereqKey]))
                            {
                                equivPrereqs.Add(db.ProductionFacilityDesigns[designIdMap[prereqKey]]);                                
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.ProductionFacilityDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlFacility["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlFacility["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey) 
                            && db.ProductionFacilityDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.ProductionFacilityDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.ProductionFacilityDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }
            }

            // OrbitalBatteries
            // w00t
            var xmlBatteries = xmlDoc.DocumentElement["OrbitalBatteries"];

            foreach (XmlElement xmlBattery in xmlBatteries.GetElementsByTagName("OrbitalBattery"))
            {
                var battery = new OrbitalBatteryDesign(xmlBattery) { DesignID = db.GetNewDesignID() };
                designIdMap[battery.Key] = battery.DesignID;
                //GameLog works
                //GameLog.Client.GameData.DebugFormat("TechDatabase.cs: battery.DesignID={0}, {1}", battery.DesignID, battery.LocalizedName);
                db.OrbitalBatteryDesigns.Add(battery);
            }
            
            foreach (XmlElement xmlBattery in xmlBatteries.GetElementsByTagName("OrbitalBattery"))
            {
                var sourceKey = xmlBattery.GetAttribute("Key");
                if (xmlBattery["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in
                        xmlBattery["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.OrbitalBatteryDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.OrbitalBatteryDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.OrbitalBatteryDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlBattery["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlBattery["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey)
                                && db.OrbitalBatteryDesigns.Contains(designIdMap[prereqKey]))
                            {
                                equivPrereqs.Add(db.OrbitalBatteryDesigns[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.OrbitalBatteryDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlBattery["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlBattery["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey)
                            && db.OrbitalBatteryDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.OrbitalBatteryDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.OrbitalBatteryDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }
            }


            // Buildings

            XmlElement xmlBuildings = xmlDoc.DocumentElement["Buildings"];
            foreach (XmlElement xmlBuilding in xmlBuildings.GetElementsByTagName("Building"))
            {
                BuildingDesign building = new BuildingDesign(xmlBuilding);
                building.DesignID = db.GetNewDesignID();
                designIdMap[building.Key] = building.DesignID;
                //GameLog works
                //GameLog.Client.GameData.DebugFormat("TechDatabase.cs: building.DesignID={0}, {1}", building.DesignID, building.LocalizedName);
                db.BuildingDesigns.Add(building);
            }
            foreach (XmlElement xmlBuilding in xmlBuildings.GetElementsByTagName("Building"))
            {
                string sourceKey = xmlBuilding.GetAttribute("Key");
                if (xmlBuilding["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in
                        xmlBuilding["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.BuildingDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.BuildingDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.BuildingDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlBuilding["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlBuilding["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey)
                                && db.BuildingDesigns.Contains(designIdMap[prereqKey]))
                            {
                                equivPrereqs.Add(db.BuildingDesigns[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.BuildingDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlBuilding["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlBuilding["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey)
                            && db.BuildingDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.BuildingDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.BuildingDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }
            }

            // Shipyards

            XmlElement xmlShipyards = xmlDoc.DocumentElement["Shipyards"];
            foreach (XmlElement xmlShipyard in xmlShipyards.GetElementsByTagName("Shipyard"))
            {
                ShipyardDesign shipyard = new ShipyardDesign(xmlShipyard);
                shipyard.DesignID = db.GetNewDesignID();
                designIdMap[shipyard.Key] = shipyard.DesignID;
                //GameLog works
                //GameLog.Client.GameData.DebugFormat("TechDatabase.cs: shipyard.DesignID={0}, {1}", shipyard.DesignID, shipyard.LocalizedName);
                db.ShipyardDesigns.Add(shipyard);
            }
            foreach (XmlElement xmlShipyard in xmlShipyards.GetElementsByTagName("Shipyard"))
            {
                string sourceKey = xmlShipyard.GetAttribute("Key");
                if (xmlShipyard["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in
                        xmlShipyard["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.ShipyardDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.ShipyardDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.ShipyardDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlShipyard["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlShipyard["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey)
                                && db.ShipyardDesigns.Contains(designIdMap[prereqKey]))
                            {
                                equivPrereqs.Add(db.ShipyardDesigns[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.ShipyardDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlShipyard["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlShipyard["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey)
                            && db.ShipyardDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.ShipyardDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.ShipyardDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }

            }

            /*********
             * Ships *
             *********/
            XmlElement xmlShips = xmlDoc.DocumentElement["Ships"];
            foreach (XmlElement xmlShip in xmlShips.GetElementsByTagName("Ship"))
            {
                ShipDesign ship = new ShipDesign(xmlShip);
                ship.DesignID = db.GetNewDesignID();
                designIdMap[ship.Key] = ship.DesignID;
                db.ShipDesigns.Add(ship);
            }
            foreach (XmlElement xmlShip in xmlShips.GetElementsByTagName("Ship"))
            {
                string sourceKey = xmlShip.GetAttribute("Key");
                if (xmlShip["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in
                        xmlShip["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.ShipDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.ShipDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.ShipDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlShip["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlShip["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey))
                            {
                                equivPrereqs.Add(db[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.ShipDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlShip["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlShip["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey)
                            && db.ShipDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.ShipDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.ShipDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }
            }

            /************
             * Stations *
             ************/
            XmlElement xmlStations = xmlDoc.DocumentElement["SpaceStations"];
            foreach (XmlElement xmlStation in xmlStations.GetElementsByTagName("SpaceStation"))
            {
                StationDesign station = new StationDesign(xmlStation);
                station.DesignID = db.GetNewDesignID();
                designIdMap[station.Key] = station.DesignID;
                db.StationDesigns.Add(station);
            }
            foreach (XmlElement xmlStation in xmlStations.GetElementsByTagName("SpaceStation"))
            {
                string sourceKey = xmlStation.GetAttribute("Key");
                if (xmlStation["ObsoletedItems"] != null)
                {
                    foreach (XmlElement xmlObsoleted in
                        xmlStation["ObsoletedItems"].GetElementsByTagName("ObsoletedItem"))
                    {
                        string obsoletedKey = xmlObsoleted.InnerText.Trim();
                        if (designIdMap.ContainsKey(obsoletedKey)
                            && db.StationDesigns.Contains(designIdMap[obsoletedKey]))
                        {
                            db.StationDesigns[designIdMap[sourceKey]].ObsoletedDesigns.Add(
                                db.StationDesigns[designIdMap[obsoletedKey]]);
                        }
                    }
                }
                if (xmlStation["Prerequisites"] != null)
                {
                    foreach (XmlElement xmlEquivPrereq in
                        xmlStation["Prerequisites"].GetElementsByTagName("EquivalentPrerequisites"))
                    {
                        var equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey)
                                && db.StationDesigns.Contains(designIdMap[prereqKey]))
                            {
                                equivPrereqs.Add(db.StationDesigns[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                            db.StationDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                    }
                }
                if (xmlStation["UpgradeOptions"] != null)
                {
                    foreach (XmlElement xmlUpgrade in
                        xmlStation["UpgradeOptions"].GetElementsByTagName("UpgradeOption"))
                    {
                        string upgradeKey = xmlUpgrade.InnerText.Trim();
                        if (designIdMap.ContainsKey(upgradeKey)
                            && db.StationDesigns.Contains(designIdMap[upgradeKey]))
                        {
                            db.StationDesigns[designIdMap[sourceKey]].UpgradableDesigns.Add(
                                db.StationDesigns[designIdMap[upgradeKey]]);
                        }
                    }
                }
            }


            bool _traceTechObjectDatabase = true;  // file is writen while starting a game -> Federation -> Start


            if (_traceTechObjectDatabase == true)
            {
                var separator = ";";
                var line = "";
                StreamWriter streamWriter;
                var file = "";
                String strHeader = "";  // first line of output files

                #region PossibleShipNames_To_CSV
                // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                file = "FromTechObj-ShipNames_(autoCreated).csv";
                Console.WriteLine("writing {0}", file);

                if (file == null)
                    goto WriterClose;

                streamWriter = new StreamWriter(file);

                strHeader =    // Head line
                    "CE_Ship" + separator +
                                        "CE_PossibleNames";
                streamWriter.WriteLine(strHeader);
                // End of head line

                foreach (var item in db.ShipDesigns)   // each item
                {
                    //item.Name._po
                    //private Dictionary<string, int> _possibleNames;
                    //if (item._possibleNames.count > 0)
                    //            foreach (_ShipName in item.)
                    //                line =
                    //                    item.Key + separator +
                    //                    item._possibleShips.count ;
                    line = "Placeholder";
                    streamWriter.WriteLine(line);
                }
                // End of ShipNames
                #endregion;

                #region SHIPS_to_CSV
                // Ships    
                file = "FromTechObj-Ships_(autoCreated).csv";
                Console.WriteLine("writing {0}", file);

                if (file == null)
                    goto WriterClose;

                streamWriter = new StreamWriter(file);

                strHeader =    // Head line
                    "CE_Ship" + separator +
                    "ATT_Key" + separator +
                    //"CE_DesignID" + separator +   // not useful for current working
                    //"CE_ShipType" + separator +  // moved down for current working
                    //"CE_ClassName" + separator +  // moved down for current working
                    "CE_TechRequirements" + separator +
                    "CE_BioTech" + separator +
                    "CE_Computers" + separator +
                    "CE_Construction" + separator +
                    "CE_Energy" + separator +
                    "CE_Propulsion" + separator +
                    "CE_Weapons" + separator +

                    "CE_BuildCost" + separator +
                    "CE_RawMaterials" + separator +
                    "CE_MaintenanceCost" + separator +

                    "CE_HullStrength" + separator +
                    "CE_PopulationHealth" + separator +
                    "CE_IsUniversallyAvailable" + separator +
                    "CE_ObsoletedDesigns" + separator +  // for real it's ObsoletedItems
                    "CE_UpgradableDesigns" + separator +   // for real it's UpgradeOptions
                    "CE_Crew" + separator +    // it's Crew
                    "CE_ScienceAbility" + separator +
                    "CE_ScanPower" + separator +
                    "CE_SensorRange" + separator +
                    "CE_HullStrength" + separator +
                    "CE_ShieldStrength" + separator +
                    "CE_ShieldRecharge" + separator +

                    "CE_ShipType" + separator +
                    "CE_ClassName" + separator +
                    "CE_Dilithium" + separator +
                    "CE_CloakStrength" + separator +
                    "CE_CamouflagedStrength" + separator +
                    "CE_Range" + separator +
                    "CE_Speed" + separator +
                    "CE_FuelReserve" + separator +
                    "CE_Maneuverability" + separator +
                    "CE_EvacuationLimit" + separator +
                    "CE_WorkCapacity" + separator +

                    "CE_InterceptAbility" + separator +
                    "CE_PossibleNames" + separator +

                    //"CE_PrimaryWeaponName" + separator + // not useful for current working
                    "CE_Beam Count" + separator +
                    //"CE_PrimaryWeapon.Count" + separator +

                    "CE_Refire" + separator +           // there is a need to export this first  (btw. first refire rate and out of that: damage)
                    //"CE_PrimaryWeapon.Refire" + separator +

                    "CE_Damage" + separator +
                    //"CE_PrimaryWeapon.Damage" + separator +


                    //"CE_SecondaryWeaponName" + separator + // not useful for current working
                    "CE_Torpedo Count" + separator +
                    //"CE_SecondaryWeapon.Count" + separator +
                    "CE_Damage";
                    //"CE_SecondaryWeapon.Damage" + separator +


                streamWriter.WriteLine(strHeader);
                // End of head line
                
                foreach (var item in db.ShipDesigns)   // each item
                {
                    line = 
                        "Ship" + separator +
                        item.Key + separator +
                        //item.DesignID + separator +   // not useful for current working
                        //item.ShipType + separator +  // moved down for current working
                        //item.ClassName + separator +  // moved down for current working
                        //item.Key;   // just for testing

                        //<TechRequirements>
                        "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                           // after GoogleSheet-Export: replace...
                                           // </Weapons> by </Weapons></TechRequirements>
                                           // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                        //"<Biotech>" + separator +                // not helpful
                        item.TechRequirements[TechCategory.BioTech] + separator +
                        //"</Biotech>" + separator +                 // not helpful
                        //"<Computers>" + separator +                 // not helpful
                        item.TechRequirements[TechCategory.Computers] + separator +
                        //"</Computers>" + separator +                // not helpful
                        //"<Construction>" + separator +                 // not helpful
                        item.TechRequirements[TechCategory.Construction] + separator +
                        //"</Construction>" + separator +                // not helpful
                        //"<Energy>" + separator +                 // not helpful
                        item.TechRequirements[TechCategory.Energy] + separator +
                        //"</Energy>" + separator +                // not helpful
                        //"<Propulsion>" + separator +                 // not helpful
                        item.TechRequirements[TechCategory.Propulsion] + separator +
                        //"</Propulsion>" + separator +                // not helpful
                        //"<Weapons>" + separator +                 // not helpful
                        item.TechRequirements[TechCategory.Weapons] + separator +
                        //"</Weapons>" + separator +                // not helpful

                        item.BuildCost + separator +
                        item.RawMaterials + separator +
                        item.MaintenanceCost + separator +
                        item.HullStrength + separator +
                        item.PopulationHealth + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export
                        item.IsUniversallyAvailable + separator +

                        // <ObsoletedItems>  // new trying ... just insert Key ... don't forget to change "II" -> "I" and as well "III" to "II"  and more
                        item.Key + separator +

                        //item.ObsoletedDesigns.FirstIndexOf(item) + separator +  // not working fine
                        //"<ObsoletedItems> + newline + " +                 // not helpful
                        //"<ObsoletedItem></ObsoletedItem>" +// not helpful
                        //" + newline + </ObsoletedItems>" +                 // not helpful
                        //separator +

                        //<UpgradeOptions>  // new trying.... justing take the key and add a "I"
                        item.Key + "I" + separator +
                        //item.UpgradableDesigns.FirstIndexOf(item) + separator +  // not working fine
                        //"<UpgradeOptions> + newline + " +                // not helpful
                        //"<UpgradeOption></UpgradeOption> + " +// not helpful
                        //separator +

                        // if UpgrodeOption is "SHIPI" (I at the end) -> remove manually

                        item.CrewSize + separator +
                        item.ScienceAbility + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export
                        item.ScanStrength + separator +
                        item.SensorRange + separator +
                        item.HullStrength + separator +
                        item.ShieldStrength + separator +
                        item.ShieldRechargeRate + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export
                        
                        item.ShipType + separator +
                        item.ClassName + separator +

                        item.Dilithium + separator +
                        item.CloakStrength + separator +
                        item.CamouflagedStrength + separator +
                        item.Range + separator +
                        item.Speed + separator +
                        item.FuelCapacity + separator +
                        item.Maneuverability + separator +
                        item.EvacuationLimit + separator +
                        item.WorkCapacity + separator +

                        item.InterceptAbility + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export

                        // Possibles ShipNames
                        //"<ShipNames> + newline + " +                // not helpful
                        //"<ShipName></ShipName>" +// not helpful
                        //" + newline + </ShipNames>" +                 // not helpful
                        "PossibleShipNames" + item.Key + separator +   // doubled ??
                        

                        //"Beam" + separator + // item.PrimaryWeaponName doesn't work  // not useful for current working
                        item.PrimaryWeapon.Count + separator +
                        item.PrimaryWeapon.Refire + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export // first refire !!
                        item.PrimaryWeapon.Damage + separator +

                        //"Torpedo" + separator + // item.SecondaryWeaponName doesn't work // not useful for current working
                        item.SecondaryWeapon.Count + separator +
                        item.SecondaryWeapon.Damage;
                        
                    streamWriter.WriteLine(line);
                }
                // End of Ships
                #endregion

                #region Shipyards_To_CSV
                // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                file = "FromTechObj-Shipyards_(autoCreated).csv";
                Console.WriteLine("writing {0}", file);

                if (file == null)
                    goto WriterClose;

                streamWriter = new StreamWriter(file);

                strHeader =    // Head line
                    "CE_Shipyard" + separator +
                    "ATT_Key" + separator +

                    "CE_TechRequirements" + separator +
                    "CE_BioTech" + separator +
                    "CE_Computers" + separator +
                    "CE_Construction" + separator +
                    "CE_Energy" + separator +
                    "CE_Propulsion" + separator +
                    "CE_Weapons" + separator +
                    "CE_BuildCost" + separator +
                    "CE_IsUniversallyAvailable" + separator +
                    "CE_Prerequisites" + separator +
                    "CE_ObsoletedItems" + separator +
                    "CE_UpgradeOptions" + separator +
                    "CE_Restrictions" + separator +
                    //"CE_EnergyCosts_not_used_anymore?" + separator +
                    "CE_BuildSlots" + separator +
                    "CE_BuildSlotMaxOutput" + separator +
                    "CE_BuildSlotOutputType" + separator +
                    "CE_BuildSlotOutput" + separator +
                    "CE_BuildSlotEnergyCost" + separator +
                    "CE_MaxBuildTechLevel";

                streamWriter.WriteLine(strHeader);
                // End of head line

                foreach (var shipyard in db.ShipyardDesigns)   // each shipyard
                {
                    line = 
                    "Shipyard" + separator +
                    shipyard.Key + separator +
                    //shipyard.DesignID + separator +   // not useful for current working
                    //shipyard.ShipType + separator +  // moved down for current working
                    //shipyard.ClassName + separator +  // moved down for current working
                    //shipyard.Key;   // just for testing

                    //<TechRequirements>
                    "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                       // after GoogleSheet-Export: replace...
                                       // </Weapons> by </Weapons></TechRequirements>
                                       // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                    //"<Biotech>" + separator +                // not helpful
                    shipyard.TechRequirements[TechCategory.BioTech] + separator +
                    //"</Biotech>" + separator +                 // not helpful
                    //"<Computers>" + separator +                 // not helpful
                    shipyard.TechRequirements[TechCategory.Computers] + separator +
                    //"</Computers>" + separator +                // not helpful
                    //"<Construction>" + separator +                 // not helpful
                    shipyard.TechRequirements[TechCategory.Construction] + separator +
                    //"</Construction>" + separator +                // not helpful
                    //"<Energy>" + separator +                 // not helpful
                    shipyard.TechRequirements[TechCategory.Energy] + separator +
                    //"</Energy>" + separator +                // not helpful
                    //"<Propulsion>" + separator +                 // not helpful
                    shipyard.TechRequirements[TechCategory.Propulsion] + separator +
                    //"</Propulsion>" + separator +                // not helpful
                    //"<Weapons>" + separator +                 // not helpful
                    shipyard.TechRequirements[TechCategory.Weapons] + separator +
                    //"</Weapons>" + separator +                // not helpful


                    shipyard.BuildCost + separator +
                    shipyard.IsUniversallyAvailable + separator +
                    "Prerequisites for " + shipyard.Key + separator +
                    "ObsoletedItems for " + shipyard.Key + separator +
                    "UpgradeOptions for " + shipyard.Key + separator +
                    //"EnergyCost_not_used_anymore?" + separator +
                    shipyard.Restriction + separator +
                    shipyard.BuildSlots + separator +
                    shipyard.BuildSlotMaxOutput + separator +
                    shipyard.BuildSlotOutputType + separator +
                    shipyard.BuildSlotOutput + separator +
                    shipyard.BuildSlotEnergyCost + separator +
                    shipyard.MaxBuildTechLevel;

                streamWriter.WriteLine(line);
                }
                // End of Shipyards
                #endregion;

                // End of Autocreated files 
                streamWriter.Close();
                WriterClose:;
            }
            

            db._designIdMap = designIdMap;

            //using (var gameDatabase = new SupremacyDatabase())
            //{
            //    foreach (var design in db)
            //    {
            //        design.ObjectString = gameDatabase.FindObjectString(design.Key);
            //        design.Compact();
            //    }
            //}

            return db;
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        private static void ValidateXml(object sender, ValidationEventArgs e)
        {
            XmlHelper.ValidateXml(XmlFilePath, e);
        }

        /// <summary>
        /// Saves this <see cref="TechDatabase"/> to XML.
        /// </summary>
        public void Save()
        {
            string path = Path.Combine(
                Environment.CurrentDirectory,
                XmlFilePath);
            Save(path);
        }

        /// <summary>
        /// Saves this <see cref="TechDatabase"/> to XML.
        /// </summary>
        /// <param name="fileName">The filename to save to.</param>
        public void Save(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                Save(writer);
            }
        }

        /// <summary>
        /// Saves this <see cref="TechDatabase"/> to XML.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to use for output.</param>
        public void Save(TextWriter writer)
        {
            using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlRoot;

                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 2;
                xmlWriter.IndentChar = ' ';

                if (!xmlDoc.HasChildNodes)
                {
                    xmlRoot = xmlDoc.CreateElement("TechObjectDatabase");
                    xmlDoc.AppendChild(xmlRoot);
                }
                else
                {
                    xmlRoot = xmlDoc.DocumentElement;
                }

                if (_productionFacilityDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("ProductionFacilities");
                    foreach (ProductionFacilityDesign design in _productionFacilityDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("ProductionFacility");
                        design.AppendXml(designElement);
                        groupElement.AppendChild(designElement);
                    }
                    xmlRoot.AppendChild(groupElement);
                }

                if (_orbitalBatteryDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("OrbitalBatteries");
                    foreach (var design in _orbitalBatteryDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("OrbitalBattery");
                        design.AppendXml(designElement);
                        groupElement.AppendChild(designElement);
                    }
                    xmlRoot.AppendChild(groupElement);
                }

                if (_buildingDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("Buildings");
                    foreach (BuildingDesign design in _buildingDesigns)
                    {
                        if (design is ShipyardDesign)
                            continue;


                        XmlElement designElement = xmlDoc.CreateElement("Building");
                  GameLog.Client.GameData.DebugFormat("designBuildings={0}, {1}", design, designElement);
                        design.AppendXml(designElement);
                        groupElement.AppendChild(designElement);
                    }
                    xmlRoot.AppendChild(groupElement);
                }

                if (_shipyardDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("Shipyards");
                    foreach (ShipyardDesign design in _shipyardDesigns)
                    {
                        ShipyardDesign shipyardDesign = design;
                        if (shipyardDesign != null)
                        {
                            XmlElement designElement = xmlDoc.CreateElement("Shipyard");
                            shipyardDesign.AppendXml(designElement);
                            groupElement.AppendChild(designElement);
                        }
                    }
                    if (groupElement.ChildNodes.Count > 0)
                        xmlRoot.AppendChild(groupElement);
                }

                if (_stationDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("SpaceStations");
                    foreach (StationDesign design in _stationDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("SpaceStation");
                        design.AppendXml(designElement);
                        groupElement.AppendChild(designElement);
                    }
                    xmlRoot.AppendChild(groupElement);
                }

                if (_shipDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("Ships");
                    foreach (ShipDesign design in _shipDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("Ship");
                        design.AppendXml(designElement);
                        groupElement.AppendChild(designElement);
                    }
                    xmlRoot.AppendChild(groupElement);
                }

                xmlDoc.WriteTo(xmlWriter);
                
            }
        }

        #region IEnumerable<TechObjectDesign> Members
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TechObjectDesign> GetEnumerator()
        {
            foreach (var design in _productionFacilityDesigns)
                yield return design;
            foreach (var design in _buildingDesigns)
                yield return design;
            foreach (var design in _shipyardDesigns)
                yield return design;
            foreach (var design in _shipDesigns)
                yield return design;
            foreach (var design in _stationDesigns)
                yield return design;
            foreach (var design in _orbitalBatteryDesigns)
                yield return design;
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Adds the specified <see cref="TechObjectDesign"/> to this <see cref="TechDatabase"/>.
        /// </summary>
        /// <param name="design">The <see cref="TechObjectDesign"/> to add.</param>
        public void Add(TechObjectDesign design)
        {
            if (design == null)
                throw new ArgumentNullException("design");

            if (design is BuildingDesign)
                _buildingDesigns.Add((BuildingDesign)design);
            else if (design is ShipyardDesign)
                _shipyardDesigns.Add((ShipyardDesign)design);
            else if (design is ProductionFacilityDesign)
                _productionFacilityDesigns.Add((ProductionFacilityDesign)design);
            else if (design is ShipDesign)
                _shipDesigns.Add((ShipDesign)design);
            else if (design is StationDesign)
                _stationDesigns.Add((StationDesign)design);
            else if (design is OrbitalBatteryDesign)
                _orbitalBatteryDesigns.Add((OrbitalBatteryDesign)design);

            if (!DesignIdMap.ContainsKey(design.Key))
                DesignIdMap[design.Key] = design.DesignID;
        }

        /// <summary>
        /// Removes the specified <see cref="TechObjectDesign"/> from this <see cref="TechDatabase"/>.
        /// </summary>
        /// <param name="design">The <see cref="TechObjectDesign"/> to remove.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Remove(TechObjectDesign design)
        {
            if (DoRemove(design))
            {
                DesignIdMap.Remove(design.Key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _designIdMap.Clear();
            _buildingDesigns.Clear();
            _shipyardDesigns.Clear();
            _productionFacilityDesigns.Clear();
            _shipDesigns.Clear();
            _stationDesigns.Clear();
            _orbitalBatteryDesigns.Clear();
        }

        private bool DoRemove(TechObjectDesign design)
        {
            if (design is BuildingDesign)
                return _buildingDesigns.Remove((BuildingDesign)design);
            if (design is ShipyardDesign)
                return _shipyardDesigns.Remove((ShipyardDesign)design);
            if (design is ProductionFacilityDesign)
                return _productionFacilityDesigns.Remove((ProductionFacilityDesign)design);
            if (design is ShipDesign)
                return _shipDesigns.Remove((ShipDesign)design);
            if (design is StationDesign)
                return _stationDesigns.Remove((StationDesign)design);
            if (design is OrbitalBatteryDesign)
                _orbitalBatteryDesigns.Remove((OrbitalBatteryDesign)design);
            return false;
        }

        #region IDeserializationCallback Members
        public void OnDeserialization(object sender)
        {
            _designIdMap = new Dictionary<string, int>();
            this.ForEach(o => _designIdMap[o.Key] = o.DesignID);
        }
        #endregion
    }
}
