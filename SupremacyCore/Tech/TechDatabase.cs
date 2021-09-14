// File:TechDatabase.cs
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
using System.Linq;
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
        private static string _text;
        private static readonly string newline = Environment.NewLine;

        /// <summary>
        /// Gets the <see cref="TechObjectDesign"/> with the specified design id.
        /// </summary>
        /// <value>The <see cref="TechObjectDesign"/> with the specified design id.</value>
        public TechObjectDesign this[int designId]
        {
            get
            {
                if (_productionFacilityDesigns.Contains(designId))
                {
                    return _productionFacilityDesigns[designId];
                }

                if (_buildingDesigns.Contains(designId))
                {
                    return _buildingDesigns[designId];
                }

                if (_shipyardDesigns.Contains(designId))
                {
                    return _shipyardDesigns[designId];
                }

                if (_shipDesigns.Contains(designId))
                {
                    return _shipDesigns[designId];
                }

                if (_stationDesigns.Contains(designId))
                {
                    return _stationDesigns[designId];
                }

                if (_orbitalBatteryDesigns.Contains(designId))
                {
                    return _orbitalBatteryDesigns[designId];
                }

                return null;
            }
        }

        public TechObjectDesign this[string key]
        {
            get
            {
                if (DesignIdMap.ContainsKey(key))
                {
                    return this[DesignIdMap[key]];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the subset of building designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The building designs.</value>
        public TechObjectDesignMap<BuildingDesign> BuildingDesigns => _buildingDesigns;

        /// <summary>
        /// Gets the subset of shipyard designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The shipyard designs.</value>
        public TechObjectDesignMap<ShipyardDesign> ShipyardDesigns => _shipyardDesigns;

        /// <summary>
        /// Gets the subset of ship designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The ship designs.</value>
        public TechObjectDesignMap<ShipDesign> ShipDesigns => _shipDesigns;

        /// <summary>
        /// Gets the subset of station designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The station designs.</value>
        public TechObjectDesignMap<StationDesign> StationDesigns => _stationDesigns;

        /// <summary>
        /// Gets the subset of facility designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The facility designs.</value>
        public TechObjectDesignMap<ProductionFacilityDesign> ProductionFacilityDesigns => _productionFacilityDesigns;

        /// <summary>
        /// Gets the subset of orbital battery designs in this <see cref="TechDatabase"/>.
        /// </summary>
        /// <value>The orbital battery designs.</value>
        public TechObjectDesignMap<OrbitalBatteryDesign> OrbitalBatteryDesigns => _orbitalBatteryDesigns;

        /// <summary>
        /// Gets the dictionary that maps the designs' unique keys to design IDs.
        /// </summary>
        /// <value>The design ID map.</value>
        public IDictionary<string, int> DesignIdMap => _designIdMap;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0054:Use '++' operator", Justification = "<Pending>")]
        public static TechDatabase Load()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            TechDatabase db = new TechDatabase();
            XmlDocument xmlDoc = new XmlDocument();
            Dictionary<string, int> designIdMap = new Dictionary<string, int>();

            _ = schemas.Add(
                "Supremacy:Supremacy.xsd",
                ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
            _ = schemas.Add(
                "Supremacy:TechObjectDatabase.xsd",
                ResourceManager.GetResourcePath("Resources/Data/TechObjectDatabase.xsd"));

            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
            xmlDoc.Schemas.Add(schemas);
            xmlDoc.Validate(ValidateXml);

            // ProductionFacilities

            XmlElement xmlFacilities = xmlDoc.DocumentElement["ProductionFacilities"];

            foreach (XmlElement xmlFacility in xmlFacilities.GetElementsByTagName("ProductionFacility"))
            {
                ProductionFacilityDesign facility = new ProductionFacilityDesign(xmlFacility)
                {
                    DesignID = db.GetNewDesignID()
                };
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
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
                        {
                            db.ProductionFacilityDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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
            XmlElement xmlBatteries = xmlDoc.DocumentElement["OrbitalBatteries"];

            foreach (XmlElement xmlBattery in xmlBatteries.GetElementsByTagName("OrbitalBattery"))
            {
                OrbitalBatteryDesign battery = new OrbitalBatteryDesign(xmlBattery) { DesignID = db.GetNewDesignID() };
                designIdMap[battery.Key] = battery.DesignID;
                //GameLog works
                //GameLog.Client.GameData.DebugFormat("TechDatabase.cs: battery.DesignID={0}, {1}", battery.DesignID, battery.LocalizedName);
                db.OrbitalBatteryDesigns.Add(battery);
            }

            foreach (XmlElement xmlBattery in xmlBatteries.GetElementsByTagName("OrbitalBattery"))
            {
                string sourceKey = xmlBattery.GetAttribute("Key");
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
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
                        {
                            db.OrbitalBatteryDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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
                BuildingDesign building = new BuildingDesign(xmlBuilding)
                {
                    DesignID = db.GetNewDesignID()
                };
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
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
                        {
                            db.BuildingDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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
                ShipyardDesign shipyard = new ShipyardDesign(xmlShipyard)
                {
                    DesignID = db.GetNewDesignID()
                };
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
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
                        {
                            db.ShipyardDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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
            string lastSuccessfullyLoadedShipDesign = "";
            int successfullyLoadedShipDesignCounter = 0;
            foreach (XmlElement xmlShip in xmlShips.GetElementsByTagName("Ship"))
            {
                lastSuccessfullyLoadedShipDesign = xmlShip.Name;
                successfullyLoadedShipDesignCounter += 1;

                ShipDesign ship = new ShipDesign(xmlShip)
                {
                    DesignID = db.GetNewDesignID()
                };
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
                        foreach (XmlElement xmlPrereq in xmlEquivPrereq.GetElementsByTagName("Prerequisite"))
                        {
                            string prereqKey = xmlPrereq.InnerText.Trim();
                            if (designIdMap.ContainsKey(prereqKey))
                            {
                                equivPrereqs.Add(db[designIdMap[prereqKey]]);
                            }
                        }
                        if (equivPrereqs.Count > 0)
                        {
                            db.ShipDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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
            GameLog.Core.XMLCheck.InfoFormat("lastSuccessfullyLoadedShipDesign = {0}", lastSuccessfullyLoadedShipDesign);
            //if (lastSuccessfullyLoadedShipDesign == "MAQUIS")
            GameLog.Client.General.InfoFormat("{0} of successfullyLoadedShipDesign (once 394 were fine)", successfullyLoadedShipDesignCounter);

            /************
             * Stations *
             ************/
            XmlElement xmlStations = xmlDoc.DocumentElement["SpaceStations"];
            foreach (XmlElement xmlStation in xmlStations.GetElementsByTagName("SpaceStation"))
            {
                StationDesign station = new StationDesign(xmlStation)
                {
                    DesignID = db.GetNewDesignID()
                };
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
                        PrerequisiteGroup equivPrereqs = new PrerequisiteGroup();
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
                        {
                            db.StationDesigns[designIdMap[sourceKey]].Prerequisites.Add(equivPrereqs);
                        }
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

            //if (ClientSettings.TracesXML2CSV == true)

            if (_traceTechObjectDatabase == true)
            {
                string pathOutputFile = "./lib/";  // instead of ./Resources/Data/
                string separator = ";";
                string line = "";
                StreamWriter streamWriter;

                string file = pathOutputFile + "test-Output.txt";
                streamWriter = new StreamWriter(file);
                streamWriter.Close();

                string strHeader = "";  // first line of output files

                #region ProductionFacilities_To_CSV
                //if (GameLog.Core.GameInitData.IsDebugEnabled == true)
                //{
                try // avoid hang up if this file is opened by another program 
                {
                    file = pathOutputFile + "_TechObj-1-ProdFac_List(autoCreated).csv";



                    if (file == null)
                    {
                        goto WriterClose;
                    }
                    Console.WriteLine("writing {0}", file);
                    streamWriter = new StreamWriter(file);

                    strHeader =    // Head line
                        "CE_ProductionFacility" + separator +
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

                        "CE_LaborCost" + separator +
                        "CE_Category" + separator +
                        "CE_UnitOutput" + separator +

                        // just placeholder
                        //"CE_Bonus" + separator +  // no bonus for ProdFac, because Type_1_Dilithium is not used - the Dilithium >BUILDING=Structure< is in usage
                        //"CE_Restrictions" + separator +   // no buildcondition for ProdFac, because Type_1_Dilithium is not used - the Dilithium >BUILDING=Structure<s is in usage
                        "CE_Prerequisites" + separator +
                        "CE_ObsoletedItems" + separator +
                        "CE_UpgradeOptions" + separator +
                        "CE_Image"
                        ;

                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    string category = "";
                    foreach (ProductionFacilityDesign PF in db.ProductionFacilityDesigns)   // each shipyard
                    {
                        //App.DoEvents();  // for avoid error after 60 seconds

                        if (PF.Category > 0)
                        {
                            category = PF.Category.ToString();
                        }

                        if (PF.Category == 0)
                        {
                            category = PF.Category.ToString();
                            if (PF.Key.Contains("DILITHIUM"))
                            {
                                category = "Dilithium";
                            }

                            if (PF.Key.Contains("DEUTERIUM"))
                            {
                                category = "Deuterium";
                            }

                            if (PF.Key.Contains("RAWMATERIALS"))
                            {
                                category = "Duranium";
                            }
                        }

                        //doesn't work
                        string imageString = "";
                        //if (PF.Image != null)
                        //    {
                        //        try { imageString = PF.Image; } catch { }
                        //    }

                        string obsDesign = "";
                        foreach (TechObjectDesign obsolete in PF.ObsoletedDesigns)
                        {
                            obsDesign += obsolete.Key + ",";
                        }
                        //GameLog.Core.GameData.DebugFormat("{0} has obsolete designs = {1} ", PF.Key, obsDesign);


                        string prerequisitesCollection = "";
                        foreach (PrerequisiteGroup prereq in PF.Prerequisites)
                        {
                            foreach (TechObjectDesign item in prereq)
                            {
                                prerequisitesCollection += prereq.FirstOrDefault().Key + ",";
                            }
                        }
                        //GameLog.Core.GameData.DebugFormat("{0} has prerequisites = {1} ", PF.Key, prerequisitesCollection);


                        string upgradeDesign = "";
                        foreach (TechObjectDesign upgrade in PF.UpgradableDesigns)
                        {
                            upgradeDesign += upgrade.Key + ",";
                        }
                        //GameLog.Core.GameData.DebugFormat("{0} has upgrade designs = {1} ", PF.Key, upgradeDesign);


                        //string bonusCollection = "";
                        ////string _bon = GameContext.Current.TechDatabase.ProductionFacilityDesigns.[PF.DesignID].UnitOutput.;
                        //foreach (var _bonus in PF.UnitOutput())
                        //{
                        //    bonusCollection += upgrade.Key + ",";
                        //}
                        //bonusCollection = "Bonus for {0}" + PF.Key;
                        //GameLog.Core.Txxt.DebugFormat("{0} has bonusCollection = {1} ", PF.Key, bonusCollection);



                        //string _buildcondition = "";   // no buildcondition for ProdFac, because Type_1_Dilithium is not used - the Dilithium >BUILDING=Structure<s is in usage
                        //_buildcondition = "BuildCondition for " + PF.Key;
                        //// following doesn't work yet
                        //try { 
                        //    foreach (var buildCond in PF.BuildCondition.ScriptCode)
                        //    {
                        //        _buildcondition += buildCond.ToString() + ",";
                        //    }
                        //    GameLog.Core.Txxest.DebugFormat("{0} has _buildcondition = {1} ", PF.Key, _buildcondition);
                        //}
                        //catch
                        //{
                        //    _buildcondition = "BuildCondition for " + PF.Key;
                        //}


                        line =
                        "ProductionFacility" + separator +
                        PF.Key + separator +


                        //<TechRequirements>
                        "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                           // after GoogleSheet-Export: replace...
                                           // </Weapons> by </Weapons></TechRequirements>
                                           // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                        PF.TechRequirements[TechCategory.BioTech] + separator +
                        PF.TechRequirements[TechCategory.Computers] + separator +
                        PF.TechRequirements[TechCategory.Construction] + separator +
                        PF.TechRequirements[TechCategory.Energy] + separator +
                        PF.TechRequirements[TechCategory.Propulsion] + separator +
                        PF.TechRequirements[TechCategory.Weapons] + separator +

                        PF.BuildCost + separator +
                        PF.IsUniversallyAvailable + separator +
                        PF.LaborCost + separator +
                        category + separator +
                        PF.UnitOutput.ToString() + separator +

                        // just placeholders
                        //"Bonus for " + PF.Key + separator +
                        //bonusCollection + separator +       // no bonus for ProdFac, because Type_1_Dilithium is not used - the Dilithium >BUILDING=Structure<s is in usage

                        //"Restrictions for " + PF.Key + separator +
                        //_buildcondition + separator +       // no buildcondition for ProdFac, because Type_1_Dilithium is not used - the Dilithium >BUILDING=Structure<s is in usage


                        // + separator +  // doesn't work

                        //"Prerequisites for " + PF.Key + separator +
                        prerequisitesCollection + separator +

                        //"ObsoletedItems for " + PF.Key + separator +
                        obsDesign + separator +

                        //"UpgradeOptions for " + PF.Key                                                
                        upgradeDesign + separator +

                        imageString
                        ;

                        //Console.WriteLine("{0}", line);

                        streamWriter.WriteLine(line);
                    }
                } //end of Try
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }
                //}// End of GameLog.Core.GameInitData.IsDebugEnabled


                #endregion ProductionFacilities_To_CSV
                // End of ProductionFacilities


                #region Buildings_To_CSV
                //if (GameLog.Core.GameInitData.IsDebugEnabled == true)
                //{
                try // avoid hang up if this file is opened by another program 
                {
                    file = pathOutputFile + "_TechObj-2-Buildings_List(autoCreated).csv";



                    if (file == null)
                    {
                        goto WriterClose;
                    }
                    Console.WriteLine("writing {0}", file);
                    streamWriter = new StreamWriter(file);

                    strHeader =    // Head line
                        "CE_Building" + separator +
                        "ATT_Key" + separator +

                        "CE_B1_Amount" + separator +
                        "CE_Bonus1" + separator +

                        "CE_B2_Amount" + separator +
                        "CE_Bonus2" + separator +

                        "CE_B3_Amount" + separator +
                        "CE_Bonus3" + separator +


                        // no more than 3 bonuses, but prepared for 5
                        //"CE_Bonus4" + separator +
                        //"CE_B4_Amount" + separator +

                        //"CE_Bonus5" + separator +
                        //"CE_B5_Amount" + separator +


                        "CE_TechRequirements" + separator +
                        "CE_BioTech" + separator +
                        "CE_Computers" + separator +
                        "CE_Construction" + separator +
                        "CE_Energy" + separator +
                        "CE_Propulsion" + separator +
                        "CE_Weapons" + separator +
                        "CE_BuildCost" + separator +
                        "CE_IsUniversallyAvailable" + separator +

                        "CE_EnergyCost" + separator +
                        //"CE_Category" + separator +

                        // just placeholder

                        "CE_Restriction1" + separator +
                        "CE_Restriction2" + separator +
                        "CE_Restriction3" + separator +
                        "CE_Restriction4" + separator +
                        "CE_Restriction5" //+ separator +
                        ;

                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    //string Restriction = "";

                    //string bonusType = "";
                    string bonustype1 = "";
                    string bonustype2 = "";
                    string bonustype3 = "";
                    //string bonustype4 = "";
                    //string bonustype5 = "";

                    string bonusAmount1 = "";
                    string bonusAmount2 = "";
                    string bonusAmount3 = "";
                    //string bonusAmount4 = "";
                    //string bonusAmount5 = "";


                    foreach (BuildingDesign B in db.BuildingDesigns)   // each shipyard
                    {
                        //App.DoEvents();  // for avoid error after 60 seconds

                        int i = 0;

                        foreach (Bonus bonus in B.Bonuses)
                        {
                            i = i + 1;  // first "bonus 1" then bonus 2

                            string bonusType = bonus.BonusType.ToString();
                            string bonusAmount = bonus.Amount.ToString();

                            switch (i)
                            {
                                case 1: bonustype1 = bonusType; bonusAmount1 = bonusAmount; break;
                                case 2: bonustype2 = bonusType; bonusAmount2 = bonusAmount; break;
                                case 3: bonustype3 = bonusType; bonusAmount3 = bonusAmount; break;
                                    //case 4: bonustype4 = bonusType; bonusAmount4 = bonusAmount; break;
                                    //case 5: bonustype5 = bonusType; bonusAmount5 = bonusAmount; break;
                            }

                        }

                        string Restriction = "";
                        // Restriction: put into one string with including semicolon, out just at the end (otherwise split and count)

                        // often
                        //if (B.HasRestriction(BuildRestriction.None)) { Restriction += "None;"; }  // delivers wrong result
                        if (B.HasRestriction(BuildRestriction.OnePerSystem)) { Restriction += "OnePerSystem;"; }
                        if (B.HasRestriction(BuildRestriction.OnePerEmpire)) { Restriction += "OnePerEmpire;"; }
                        if (B.HasRestriction(BuildRestriction.HomeSystem)) { Restriction += "HomeSystem;"; }


                        if (B.HasRestriction(BuildRestriction.ArcticPlanet)) { Restriction += "ArcticPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.Asteroids)) { Restriction += "Asteroids;"; }
                        if (B.HasRestriction(BuildRestriction.BarrenPlanet)) { Restriction += "BarrenPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.BlueStar)) { Restriction += "BlueStar;"; }
                        if (B.HasRestriction(BuildRestriction.ConqueredSystem)) { Restriction += "ConqueredSystem;"; }
                        if (B.HasRestriction(BuildRestriction.CrystallinePlanet)) { Restriction += "CrystallinePlanet;"; }
                        if (B.HasRestriction(BuildRestriction.DemonPlanet)) { Restriction += "DemonPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.DesertPlanet)) { Restriction += "DesertPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.DilithiumBonus)) { Restriction += "DilithiumBonus;"; }
                        if (B.HasRestriction(BuildRestriction.GasGiant)) { Restriction += "GasGiant;"; }
                        if (B.HasRestriction(BuildRestriction.JunglePlanet)) { Restriction += "JunglePlanet;"; }
                        if (B.HasRestriction(BuildRestriction.MemberSystem)) { Restriction += "MemberSystem;"; }
                        if (B.HasRestriction(BuildRestriction.Moons)) { Restriction += "Moons;"; }
                        if (B.HasRestriction(BuildRestriction.NativeSystem)) { Restriction += "NativeSystem;"; }
                        if (B.HasRestriction(BuildRestriction.Nebula)) { Restriction += "Nebula;"; }
                        if (B.HasRestriction(BuildRestriction.NonNativeSystem)) { Restriction += "NonNativeSystem;"; }
                        if (B.HasRestriction(BuildRestriction.OceanicPlanet)) { Restriction += "OceanicPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.OnePer100MaxPopUnits)) { Restriction += "OnePer100MaxPopUnits;"; }
                        if (B.HasRestriction(BuildRestriction.OrangeStar)) { Restriction += "OrangeStar;"; }
                        if (B.HasRestriction(BuildRestriction.DuraniumBonus)) { Restriction += "DuraniumBonus;"; }
                        if (B.HasRestriction(BuildRestriction.RedStar)) { Restriction += "RedStar;"; }
                        if (B.HasRestriction(BuildRestriction.RoguePlanet)) { Restriction += "RoguePlanet;"; }
                        if (B.HasRestriction(BuildRestriction.TerranPlanet)) { Restriction += "TerranPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.VolcanicPlanet)) { Restriction += "VolcanicPlanet;"; }
                        if (B.HasRestriction(BuildRestriction.WhiteStar)) { Restriction += "WhiteStar;"; }
                        if (B.HasRestriction(BuildRestriction.YellowStar)) { Restriction += "YellowStar;"; }

                        if (B.HasRestriction(BuildRestriction.GreenStar)) { Restriction += "Green Star (no Green Stars in Universe!);"; }



                        line =
                        "Building" + separator +
                        B.Key + separator +
                        //B.Image + separator +

                        bonusAmount1 + separator +
                        bonustype1 + separator +

                        bonusAmount2 + separator +
                        bonustype2 + separator +

                        bonusAmount3 + separator +
                        bonustype3 + separator +

                        //shipyard.DesignID + separator +   // not useful for current working
                        //shipyard.ShipType + separator +  // moved down for current working
                        //shipyard.ClassName + separator +  // moved down for current working
                        //shipyard.Key;   // just for testing

                        //<TechRequirements>
                        "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                           // after GoogleSheet-Export: replace...
                                           // </Weapons> by </Weapons></TechRequirements>
                                           // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                        B.TechRequirements[TechCategory.BioTech] + separator +
                        B.TechRequirements[TechCategory.Computers] + separator +
                        B.TechRequirements[TechCategory.Construction] + separator +
                        B.TechRequirements[TechCategory.Energy] + separator +
                        B.TechRequirements[TechCategory.Propulsion] + separator +
                        B.TechRequirements[TechCategory.Weapons] + separator +

                        B.BuildCost + separator +
                        B.IsUniversallyAvailable + separator +

                        B.EnergyCost + separator +

                        Restriction // for " + B.Key //+ separator +
                                    //"Prerequisites for " + B.Key + separator +
                                    //"ObsoletedItems for " + B.Key + separator +
                                    //"UpgradeOptions for " + B.Key
                        ;

                        //Console.WriteLine("{0}", line);

                        streamWriter.WriteLine(line);

                        //clear strings for next buildingng
                        bonustype1 = ""; bonusAmount1 = "";
                        bonustype2 = ""; bonusAmount2 = "";
                        bonustype3 = ""; bonusAmount3 = "";
                        //bonustype4 = ""; bonusAmount4 = "";
                        //bonustype5 = ""; bonusAmount5 = "";

                        Restriction = "";
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }
                //} end of GameLog.Core.GameInitData.IsDebugEnabled


                #endregion Buildings_To_CSV
                // End of Buildings_To_CSV

                #region PossibleShipNames_To_CSV
                try // avoid hang up if this file is opened by another program 
                {
                    // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                    file = pathOutputFile + "_TechObj-6-Ships_NAMES_List(autoCreated).csv";

                    if (file == null)
                    {
                        goto WriterClose;
                    }

                    streamWriter = new StreamWriter(file);

                    Console.WriteLine("writing {0}", file);



                    strHeader =    // Head line
                        "CE_Ship" + separator +
                                            "CE_PossibleNames";
                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    foreach (ShipDesign item in db.ShipDesigns)   // each item
                    {
                        _text = "";
                        foreach (KeyValuePair<string, int> _pair in item.PossibleNames)
                        {
                            _text += _pair.Key + ";" + item.Name + newline;
                        }
                        streamWriter.WriteLine(_text + "next");
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }
                // End of PShipNames
                #endregion PossibleShipNames_To_CSV;

                #region SHIPS_to_CSV and ShipData.txt for UnityEngine
                try // avoid hang up if this file is opened by another program 
                {
                    // Ships    
                    file = pathOutputFile + "_TechObj-6-Ships_List(autoCreated).csv";   //Console.WriteLine("writing {0}", file);

                    if (file == null)
                    {
                        goto WriterClose;
                    }

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
                        "CE_Duranium" + separator +
                        "CE_MaintenanceCost" + separator +

                        "CE_HullStrength" + separator +
                        "CE_PopulationHealth" + separator +
                        "CE_IsUniversallyAvailable" + separator +

                        "CE_Crew" + separator +    // it's Crew
                        "CE_ScienceAbility" + separator +
                        "CE_ScanPower" + separator +
                        "CE_SensorRange" + separator +
                        "CE_HullStrength" + separator +
                        "CE_ShieldStrength" + separator +
                        "CE_ShieldRecharge" + separator +

                        "CE_ShipType" + separator +

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

                        "CE_ClassName" + separator +
                        //"CE_PrimaryWeaponName" + separator + // not useful for current working
                        "CE_Beam Count" + separator +
                        "CE_Refire" + separator +           // there is a need to export this first  (btw. first refire rate and out of that: damage)
                                                            //"CE_PrimaryWeapon.Refire" + separator +
                        "CE_Damage" + separator +


                        //"CE_SecondaryWeaponName" + separator + // not useful for current working
                        "CE_Torpedo Count" + separator +
                        "CE_Damage" + separator +
                        "FirePower" + separator +

                        "CE_ObsoletedDesigns" + separator +  // for real it's ObsoletedItems
                        "CE_UpgradableDesigns" + separator +   // for real it's UpgradeOptions
                        "CE_PossibleNames"
                        ;
                    //"CE_SecondaryWeapon.Damage" + separator +



                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    foreach (ShipDesign item in db.ShipDesigns)   // each item
                    {
                        int _firepower = (item.PrimaryWeapon.Count * item.PrimaryWeapon.Damage)
                                            + (item.SecondaryWeapon.Count * item.SecondaryWeapon.Damage);
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
                            item.Duranium + separator +
                            item.MaintenanceCost + separator +
                            item.HullStrength + separator +
                            item.PopulationHealth + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export
                            item.IsUniversallyAvailable + separator +



                            item.CrewSize + separator +
                            item.ScienceAbility + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export
                            item.ScanStrength + separator +
                            item.SensorRange + separator +
                            item.HullStrength + separator +
                            item.ShieldStrength + separator +
                            item.ShieldRechargeRate + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export

                            item.ShipType + separator +


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


                            item.ClassName + separator +
                            //"Beam" + separator + // item.PrimaryWeaponName doesn't work  // not useful for current working
                            item.PrimaryWeapon.Count + separator +
                            item.PrimaryWeapon.Refire + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export // first refire !!
                            item.PrimaryWeapon.Damage + separator +

                            //"Torpedo" + separator + // item.SecondaryWeaponName doesn't work // not useful for current working
                            item.SecondaryWeapon.Count + separator +
                            item.SecondaryWeapon.Damage + separator +

                            _firepower + separator +


                             // <ObsoletedItems>  // new trying ... just insert Key ... don't forget to change "II" -> "I" and as well "III" to "II"  and more
                             "ObsoletedItems" + item.Key + separator +

                             //item.ObsoletedDesigns.FirstIndexOf(item) + separator +  // not working fine
                             //"<ObsoletedItems> + newline + " +                 // not helpful
                             //"<ObsoletedItem></ObsoletedItem>" +// not helpful
                             //" + newline + </ObsoletedItems>" +                 // not helpful
                             //separator +

                             //<UpgradeOptions>  // new trying.... justing take the key and add a "I"
                             "UpgradeOptions" + item.Key + separator +
                            //item.UpgradableDesigns.FirstIndexOf(item) + separator +  // not working fine
                            //"<UpgradeOptions> + newline + " +                // not helpful
                            //"<UpgradeOption></UpgradeOption> + " +// not helpful
                            //separator +



                            // Possibles ShipNames
                            //"<ShipNames> + newline + " +                // not helpful
                            //"<ShipName></ShipName>" +// not helpful
                            //" + newline + </ShipNames>" +                 // not helpful
                            "PossibleShipNames" + item.Key
                            ;

                        streamWriter.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }

                // End of Ships
                #endregion SHIPS_to_CSV

                #region SHIPS_to_ShipData.csv (UnityEngine)
                try // avoid hang up if this file is opened by another program 
                {
                    // Ships    
                    file = pathOutputFile + "ShipData.csv";   //Console.WriteLine("writing {0}", file);

                    if (file == null)
                    {
                        goto WriterClose;
                    }

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
                        "CE_Duranium" + separator +
                        "CE_MaintenanceCost" + separator +

                        "CE_HullStrength" + separator +
                        "CE_PopulationHealth" + separator +
                        "CE_IsUniversallyAvailable" + separator +

                        "CE_Crew" + separator +    // it's Crew
                        "CE_ScienceAbility" + separator +
                        "CE_ScanPower" + separator +
                        "CE_SensorRange" + separator +
                        "CE_HullStrength" + separator +
                        "CE_ShieldStrength" + separator +
                        "CE_ShieldRecharge" + separator +

                        "CE_ShipType" + separator +

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

                        "CE_ClassName" + separator +
                        //"CE_PrimaryWeaponName" + separator + // not useful for current working
                        "CE_Beam Count" + separator +
                        "CE_Refire" + separator +           // there is a need to export this first  (btw. first refire rate and out of that: damage)
                                                            //"CE_PrimaryWeapon.Refire" + separator +
                        "CE_Damage" + separator +


                        //"CE_SecondaryWeaponName" + separator + // not useful for current working
                        "CE_Torpedo Count" + separator +
                        "CE_Damage" + separator +
                        "FirePower" + separator +

                        "CE_ObsoletedDesigns" + separator +  // for real it's ObsoletedItems
                        "CE_UpgradableDesigns" + separator +   // for real it's UpgradeOptions
                        "CE_PossibleNames"
                        ;
                    //"CE_SecondaryWeapon.Damage" + separator +



                    ////////////streamWriter.WriteLine(strHeader);
                    // End of head line


                    foreach (ShipDesign item in db.ShipDesigns)   // each item
                    {

                        int _beamDamage = 0;
                        //int _beamRefire = 1;
                        int _torpedoDamage = 0;

                        int _firepower = (item.PrimaryWeapon.Count * item.PrimaryWeapon.Damage)
                                            + (item.SecondaryWeapon.Count * item.SecondaryWeapon.Damage);

                        if (item.PrimaryWeapon != null)
                        {
                            //_ = int.TryParse(item.PrimaryWeapon.Refire.ToString(), out _beamRefire);
                            _beamDamage = item.PrimaryWeapon.Count * item.PrimaryWeapon.Damage; // * _beamRefire;
                        }

                        if (item.SecondaryWeapon != null)
                        {
                            _torpedoDamage = item.SecondaryWeapon.Count * item.SecondaryWeapon.Damage;
                        }

                        line =
                            //"Ship" + separator +
                            item.Key + "(Clone)" + separator +
                            item.ShieldStrength + separator +
                            item.HullStrength + separator +
                            _beamDamage + separator +
                            _torpedoDamage/* + separator */
                            ;
                            ////item.DesignID + separator +   // not useful for current working
                            ////item.ShipType + separator +  // moved down for current working
                            ////item.ClassName + separator +  // moved down for current working
                            ////item.Key;   // just for testing

                            ////<TechRequirements>
                            //"xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                            //                   // after GoogleSheet-Export: replace...
                            //                   // </Weapons> by </Weapons></TechRequirements>
                            //                   // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                            ////"<Biotech>" + separator +                // not helpful
                            //item.TechRequirements[TechCategory.BioTech] + separator +
                            ////"</Biotech>" + separator +                 // not helpful
                            ////"<Computers>" + separator +                 // not helpful
                            //item.TechRequirements[TechCategory.Computers] + separator +
                            ////"</Computers>" + separator +                // not helpful
                            ////"<Construction>" + separator +                 // not helpful
                            //item.TechRequirements[TechCategory.Construction] + separator +
                            ////"</Construction>" + separator +                // not helpful
                            ////"<Energy>" + separator +                 // not helpful
                            //item.TechRequirements[TechCategory.Energy] + separator +
                            ////"</Energy>" + separator +                // not helpful
                            ////"<Propulsion>" + separator +                 // not helpful
                            //item.TechRequirements[TechCategory.Propulsion] + separator +
                            ////"</Propulsion>" + separator +                // not helpful
                            ////"<Weapons>" + separator +                 // not helpful
                            //item.TechRequirements[TechCategory.Weapons] + separator +
                            ////"</Weapons>" + separator +                // not helpful

                            //item.BuildCost + separator +
                            //item.Duranium + separator +
                            //item.MaintenanceCost + separator +
                            //item.HullStrength + separator +
                            //item.PopulationHealth + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export
                            //item.IsUniversallyAvailable + separator +



                            //item.CrewSize + separator +
                            //item.ScienceAbility + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export
                            //item.ScanStrength + separator +
                            //item.SensorRange + separator +
                            //item.HullStrength + separator +
                            //item.ShieldStrength + separator +
                            //item.ShieldRechargeRate + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export

                            //item.ShipType + separator +


                            //item.Dilithium + separator +
                            //item.CloakStrength + separator +
                            //item.CamouflagedStrength + separator +
                            //item.Range + separator +
                            //item.Speed + separator +
                            //item.FuelCapacity + separator +
                            //item.Maneuverability + separator +
                            //item.EvacuationLimit + separator +
                            //item.WorkCapacity + separator +

                            //item.InterceptAbility + "percent" + separator +  // percent bust be replaced after GoogleSheet-Export


                            //item.ClassName + separator +
                            ////"Beam" + separator + // item.PrimaryWeaponName doesn't work  // not useful for current working
                            //item.PrimaryWeapon.Count + separator +
                            //item.PrimaryWeapon.Refire + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export // first refire !!
                            //item.PrimaryWeapon.Damage + separator +

                            ////"Torpedo" + separator + // item.SecondaryWeaponName doesn't work // not useful for current working
                            //item.SecondaryWeapon.Count + separator +
                            //item.SecondaryWeapon.Damage + separator +

                            //_firepower + separator +


                            // // <ObsoletedItems>  // new trying ... just insert Key ... don't forget to change "II" -> "I" and as well "III" to "II"  and more
                            // "ObsoletedItems" + item.Key + separator +

                            // //item.ObsoletedDesigns.FirstIndexOf(item) + separator +  // not working fine
                            // //"<ObsoletedItems> + newline + " +                 // not helpful
                            // //"<ObsoletedItem></ObsoletedItem>" +// not helpful
                            // //" + newline + </ObsoletedItems>" +                 // not helpful
                            // //separator +

                            // //<UpgradeOptions>  // new trying.... justing take the key and add a "I"
                            // "UpgradeOptions" + item.Key + separator +
                            ////item.UpgradableDesigns.FirstIndexOf(item) + separator +  // not working fine
                            ////"<UpgradeOptions> + newline + " +                // not helpful
                            ////"<UpgradeOption></UpgradeOption> + " +// not helpful
                            ////separator +



                            //// Possibles ShipNames
                            ////"<ShipNames> + newline + " +                // not helpful
                            ////"<ShipName></ShipName>" +// not helpful
                            ////" + newline + </ShipNames>" +                 // not helpful
                            //"PossibleShipNames" + item.Key
                            ;

                        streamWriter.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }

                // End of Ships
                #endregion SHIPS_to_ShipData.txt

                #region Shipyards_To_CSV
                try // avoid hang up if this file is opened by another program 
                {
                    // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                    file = pathOutputFile + "_TechObj-4-Shipyards_List(autoCreated).csv";
                    //Console.WriteLine("writing {0}", file);

                    if (file == null)
                    {
                        goto WriterClose;
                    }

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

                        //"CE_EnergyCosts_not_used_anymore?" + separator +
                        "CE_BuildSlots" + separator +
                        "CE_BuildSlotMaxOutput" + separator +
                        "CE_BuildSlotOutputType" + separator +
                        "CE_BuildSlotOutput" + separator +
                        "CE_BuildSlotEnergyCost" + separator +
                        "CE_MaxBuildTechLevel" + separator +

                        "CE_Restrictions" + separator +
                        "CE_Prerequisites" + separator +
                        "CE_ObsoletedItems" + separator +
                        "CE_UpgradeOptions" + separator
                        ;

                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    foreach (ShipyardDesign shipyard in db.ShipyardDesigns)   // each shipyard
                    {
                        string obsDesign = "";
                        foreach (TechObjectDesign obsolete in shipyard.ObsoletedDesigns)
                        {
                            obsDesign += obsolete.Key + ",";
                        }
                        //GameLog.Core.Texxst.DebugFormat("{0} has obsolete designs = {1} ", shipyard.Key, obsDesign);


                        string prerequisitesCollection = "";
                        foreach (PrerequisiteGroup prereq in shipyard.Prerequisites)
                        {
                            foreach (TechObjectDesign item in prereq)
                            {
                                prerequisitesCollection += prereq.FirstOrDefault().Key + ",";
                            }
                        }
                        //GameLog.Core.Texxst.DebugFormat("{0} has prerequisites = {1} ", shipyard.Key, prerequisitesCollection);


                        string upgradeDesign = "";
                        foreach (TechObjectDesign upgrade in shipyard.UpgradableDesigns)
                        {
                            upgradeDesign += upgrade.Key + ",";
                        }
                        //GameLog.Core.Txxest.DebugFormat("{0} has upgrade designs = {1} ", shipyard.Key, upgradeDesign);

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

                        //"EnergyCost_not_used_anymore?" + separator +

                        shipyard.BuildSlots + separator +
                        shipyard.BuildSlotMaxOutput + separator +
                        shipyard.BuildSlotOutputType + separator +
                        shipyard.BuildSlotOutput + separator +
                        shipyard.BuildSlotEnergyCost + separator +
                        shipyard.MaxBuildTechLevel + separator +
                        shipyard.Restriction + separator +

                        prerequisitesCollection + separator +
                        //"Prerequisites for " + shipyard.Key + separator +
                        obsDesign + separator +
                        //"ObsoletedItems for " + shipyard.Key + separator +
                        upgradeDesign + separator +
                        //"UpgradeOptions for " + shipyard.Key
                        separator // emtpy colomn
                        ;

                        streamWriter.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }

                // End of Shipyards
                #endregion Shipyards_To_CSV


                #region Stations_To_CSV
                try // avoid hang up if this file is opened by another program 
                {
                    // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                    file = pathOutputFile + "_TechObj-5-Stations_List(autoCreated).csv";
                    Console.WriteLine("writing {0}", file);

                    if (file == null)
                    {
                        goto WriterClose;
                    }

                    streamWriter = new StreamWriter(file);

                    strHeader =    // Head line
                        "CE_Station" + separator +
                        "ATT_Key" + separator +

                        "CE_TechRequirements" + separator +
                        "CE_BioTech" + separator +
                        "CE_Computers" + separator +
                        "CE_Construction" + separator +
                        "CE_Energy" + separator +
                        "CE_Propulsion" + separator +
                        "CE_Weapons" + separator +
                        "CE_BuildCost" + separator +
                        "CE_Duranium" + separator +
                        "CE_MaintanceCost" + separator +
                        "CE_Crew" + separator +
                        "CE_IsUniversallyAvailable" + separator +

                        "CE_ScienceAbility" + separator +
                        "CE_ScanPower" + separator +
                        //"CE_EnergyCosts_not_used_anymore?" + separator +
                        "CE_SensorRange" + separator +
                        "CE_HullStrength" + separator +
                        "CE_ShieldStrength" + separator +
                        "CE_ShieldRecharge" + separator +

                        "CE_Beam" + separator +
                        "CE_Beam Count" + separator +
                        "CE_Damage" + separator +
                        "CE_Refire" + separator +           // there is a need to export this first  (btw. first refire rate and out of that: damage)

                        "CE_Torpedo" + separator +
                        "CE_Torpedo Count" + separator +
                        "CE_Damage" + separator +

                        "CE_RepairSlots" + separator +
                        "CE_RepairCapacity" + separator +

                        // just placeholders at the moment for > other/outside replacements
                        "CE_Prerequisites" + separator +
                        "CE_ObsoletedItems" + separator +
                        "CE_UpgradeOptions" + separator +

                        "CE_StationNames"
                        ;

                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    foreach (StationDesign station in db.StationDesigns)   // each shipyard
                    {
                        if (station.Key == "ROM_STARBASE_I")  // just for testing - any problems for ROM_I or II ??
                        {
                            GameLog.Core.GameData.DebugFormat("{0} testing ", station.Key);
                        }

                        string obsDesign = "";
                        foreach (TechObjectDesign obsolete in station.ObsoletedDesigns)
                        {
                            obsDesign += obsolete.Key + ",";
                        }
                        //GameLog.Core.Texxst.DebugFormat("{0} has obsolete designs = {1} ", station.Key, obsDesign);


                        string prerequisitesCollection = "";
                        foreach (PrerequisiteGroup prereq in station.Prerequisites)
                        {
                            foreach (TechObjectDesign item in prereq)
                            {
                                prerequisitesCollection += prereq.FirstOrDefault().Key + ",";
                            }
                        }
                        //GameLog.Core.Texxst.DebugFormat("{0} has prerequisites = {1} ", station.Key, prerequisitesCollection);


                        string upgradeDesign = "";
                        foreach (TechObjectDesign upgrade in station.UpgradableDesigns)
                        {
                            upgradeDesign += upgrade.Key + ",";
                        }
                        //GameLog.Core.Tesxxt.DebugFormat("{0} has upgrade designs = {1} ", station.Key, upgradeDesign);

                        //string possibleNames = "";
                        //foreach (var possName in db.xxxx.)  // didn't find a way for station names
                        //{
                        //    possibleNames += possName.Key + ",";
                        //}
                        //GameLog.Core.Texxst.DebugFormat("{0} has upgrade designs = {1} ", station.Key, upgradeDesign);

                        // --------------------------
                        line =
                        "Station" + separator +
                        station.Key + separator +
                        //station.DesignID + separator +   // not useful for current working
                        //station.ShipType + separator +  // moved down for current working
                        //station.ClassName + separator +  // moved down for current working
                        //station.Key;   // just for testing

                        //<TechRequirements>
                        "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                           // after GoogleSheet-Export: replace...
                                           // </Weapons> by </Weapons></TechRequirements>
                                           // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                        //"<Biotech>" + separator +                // not helpful
                        station.TechRequirements[TechCategory.BioTech] + separator +
                        //"</Biotech>" + separator +                 // not helpful
                        //"<Computers>" + separator +                 // not helpful
                        station.TechRequirements[TechCategory.Computers] + separator +
                        //"</Computers>" + separator +                // not helpful
                        //"<Construction>" + separator +                 // not helpful
                        station.TechRequirements[TechCategory.Construction] + separator +
                        //"</Construction>" + separator +                // not helpful
                        //"<Energy>" + separator +                 // not helpful
                        station.TechRequirements[TechCategory.Energy] + separator +
                        //"</Energy>" + separator +                // not helpful
                        //"<Propulsion>" + separator +                 // not helpful
                        station.TechRequirements[TechCategory.Propulsion] + separator +
                        //"</Propulsion>" + separator +                // not helpful
                        //"<Weapons>" + separator +                 // not helpful
                        station.TechRequirements[TechCategory.Weapons] + separator +
                        //"</Weapons>" + separator +                // not helpful


                        station.BuildCost + separator +
                        station.Duranium + separator +
                        station.MaintenanceCost + separator +
                        station.CrewSize + separator +
                        station.IsUniversallyAvailable + separator +


                        //"EnergyCost_not_used_anymore?" + separator +

                        station.ScienceAbility + separator +
                        station.ScanStrength + separator +  // equal to ScanPower
                        station.SensorRange + separator +

                        station.HullStrength + separator +
                        station.ShieldStrength + separator +
                        station.ShieldRechargeRate + separator +

                        "Beam" + separator + // item.PrimaryWeaponName doesn't work  // not useful for current working
                        station.PrimaryWeapon.Count + separator +
                        station.PrimaryWeapon.Damage + separator +
                        station.PrimaryWeapon.Refire /*+ "percent"*/ + separator +   // percent bust be replaced after GoogleSheet-Export // first refire !!


                        "Torpedo" + separator + // item.SecondaryWeaponName doesn't work // not useful for current working
                        station.SecondaryWeapon.Count + separator +
                        station.SecondaryWeapon.Damage + separator +

                        station.BuildSlots + separator +
                        station.BuildOutput + separator +

                        // just placeholders at the moment for > other/outside replacements
                        prerequisitesCollection + separator +
                        //"Prerequisites for " + station.Key + separator +
                        obsDesign + separator +
                        //"ObsoletedItems for " + station.Key + separator +
                        upgradeDesign + separator +
                        //"UpgradeOptions for " + station.Key + separator +
                        //possibleNames + separator +
                        "PossibleStationNames" + station.Key + separator +

                        separator;  // ends with an empty column


                        streamWriter.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }

                // End of Stations
                #endregion Stations_To_CSV


                #region OrbBat_To_CSV
                try // avoid hang up if this file is opened by another program 
                {
                    // PossibleShipNames   // at the moment not working because I didn't found a way to read the dictionary
                    file = pathOutputFile + "_TechObj-3-OrbBat_List(autoCreated).csv";
                    Console.WriteLine("writing {0}", file);

                    if (file == null)
                    {
                        goto WriterClose;
                    }

                    streamWriter = new StreamWriter(file);

                    strHeader =    // Head line
                        "CE_OrbitalBattery" + separator +
                        "ATT_Key" + separator +

                        "CE_TechRequirements" + separator +
                        "CE_BioTech" + separator +
                        "CE_Computers" + separator +
                        "CE_Construction" + separator +
                        "CE_Energy" + separator +
                        "CE_Propulsion" + separator +
                        "CE_Weapons" + separator +

                        "CE_BuildCost" + separator +
                        "CE_Duranium" + separator +
                        "CE_MaintanceCost" + separator +
                        "CE_UnitEnergyCost" + separator +
                        "CE_IsUniversallyAvailable" + separator +

                        "CE_ScienceAbility" + separator +
                        "CE_ScanPower" + separator +
                        //"CE_EnergyCosts_not_used_anymore?" + separator +
                        "CE_SensorRange" + separator +
                        "CE_HullStrength" + separator +
                        "CE_ShieldStrength" + separator +
                        "CE_ShieldRecharge" + separator +

                        "CE_Beam" + separator +
                        "CE_Beam Count" + separator +
                        "CE_Damage" + separator +
                        "CE_Refire" + separator +           // there is a need to export this first  (btw. first refire rate and out of that: damage)

                        "CE_Torpedo" + separator +
                        "CE_Torpedo Count" + separator +
                        "CE_Damage" + separator +

                        //"CE_RepairSlots" + separator +
                        //"CE_RepairCapacity" + separator +

                        // just placeholders at the moment for > other/outside replacements
                        //"CE_Prerequisites" + separator +
                        "CE_ObsoletedItems" + separator +
                        "CE_UpgradeOptions" //+ separator +

                        //"CE_StationNames"
                        ;

                    streamWriter.WriteLine(strHeader);
                    // End of head line

                    foreach (OrbitalBatteryDesign ob in db.OrbitalBatteryDesigns)   // each shipyard
                    {
                        string obsDesign = "";
                        foreach (TechObjectDesign obsolete in ob.ObsoletedDesigns)
                        {
                            obsDesign += obsolete.Key + ",";
                        }
                        //GameLog.Core.CombatDetails.DebugFormat("{0} has obsolete designs = {1} ", ob.Key, obsDesign);


                        //string prerequisitesCollection = "";
                        //foreach (var prereq in shipyard.Prerequisites)
                        //{
                        //    foreach (var item in prereq)
                        //    {
                        //        prerequisitesCollection += prereq.FirstOrDefault().Key + ",";
                        //    }
                        //}
                        ////GameLog.Core.GameData.DebugFormat("{0} has prerequisites = {1} ", shipyard.Key, prerequisitesCollection);


                        string upgradeDesign = "";
                        foreach (TechObjectDesign upgrade in ob.UpgradableDesigns)
                        {
                            upgradeDesign += upgrade.Key + ",";
                        }


                        line =
                        "OrbitalBattery" + separator +
                        ob.Key + separator +
                        //ob.DesignID + separator +   // not useful for current working
                        //ob.ShipType + separator +  // moved down for current working
                        //ob.ClassName + separator +  // moved down for current working
                        //ob.Key;   // just for testing

                        //<TechRequirements>
                        "xx" + separator + // needs to be empty for "<TechRequirements></TechRequirements>" + separator +  
                                           // after GoogleSheet-Export: replace...
                                           // </Weapons> by </Weapons></TechRequirements>
                                           // and <TechRequirements></TechRequirements> by just a beginning <TechRequirements>

                        //"<Biotech>" + separator +                // not helpful
                        ob.TechRequirements[TechCategory.BioTech] + separator +
                        //"</Biotech>" + separator +                 // not helpful
                        //"<Computers>" + separator +                 // not helpful
                        ob.TechRequirements[TechCategory.Computers] + separator +
                        //"</Computers>" + separator +                // not helpful
                        //"<Construction>" + separator +                 // not helpful
                        ob.TechRequirements[TechCategory.Construction] + separator +
                        //"</Construction>" + separator +                // not helpful
                        //"<Energy>" + separator +                 // not helpful
                        ob.TechRequirements[TechCategory.Energy] + separator +
                        //"</Energy>" + separator +                // not helpful
                        //"<Propulsion>" + separator +                 // not helpful
                        ob.TechRequirements[TechCategory.Propulsion] + separator +
                        //"</Propulsion>" + separator +                // not helpful
                        //"<Weapons>" + separator +                 // not helpful
                        ob.TechRequirements[TechCategory.Weapons] + separator +
                        //"</Weapons>" + separator +                // not helpful


                        ob.BuildCost + separator +
                        ob.Duranium + separator +
                        ob.MaintenanceCost + separator +
                        ob.UnitEnergyCost + separator +
                        ob.IsUniversallyAvailable + separator +


                        //"EnergyCost_not_used_anymore?" + separator +

                        ob.ScienceAbility + separator +
                        ob.ScanStrength + separator +  // equal to ScanPower
                        ob.SensorRange + separator +

                        ob.HullStrength + separator +
                        ob.ShieldStrength + separator +
                        ob.ShieldRechargeRate + separator +

                        "Beam" + separator + // item.PrimaryWeaponName doesn't work  // not useful for current working
                        ob.PrimaryWeapon.Count + separator +
                        ob.PrimaryWeapon.Damage + separator +
                        ob.PrimaryWeapon.Refire + "percent" + separator +   // percent bust be replaced after GoogleSheet-Export // first refire !!


                        "Torpedo" + separator + // item.SecondaryWeaponName doesn't work // not useful for current working
                        ob.SecondaryWeapon.Count + separator +
                        ob.SecondaryWeapon.Damage + separator +

                        //ob.BuildSlots + separator +
                        //ob.BuildOutput + separator +

                        // just placeholders at the moment for > other/outside replacements
                        //"Prerequisites for " + ob.Key + separator +

                        obsDesign + separator +
                        //"ObsoletedItems for " + ob.Key + separator +

                        upgradeDesign + separator +
                        //"UpgradeOptions for " + ob.Key + separator +

                        //"PossibleOrbBatNames" + ob.Key + separator +

                        separator;  // ends with an empty column


                        streamWriter.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    _text = "Cannot write ... " + file + e;
                    GameLog.Core.GameData.ErrorFormat(_text);
                }


                #endregion OrbBat_To_CSV
                // End of OrbBat


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
        /// <param name="e">The <see cref="ValidationEventArgs"/> instance containing the event data.</param>
        private static void ValidateXml(object sender, ValidationEventArgs e)
        {
            XmlHelper.ValidateXml(XmlFilePath, e);
        }

        /// <summary>
        /// Saves this <see cref="TechDatabase"/> to XML.
        /// </summary>
        public void Save()
        {
            string path = Path.Combine(Environment.CurrentDirectory, XmlFilePath);
            Save(path);
        }

        /// <summary>
        /// Saves this <see cref="TechDatabase"/> to XML.
        /// </summary>
        /// <param name="fileName">The filename to save to.</param>
        public void Save(string fileName)
        {
            if (fileName == null) { throw new ArgumentNullException("fileName"); }

            using (StreamWriter writer = new StreamWriter(fileName)) { Save(writer); }
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
                    _ = xmlDoc.AppendChild(xmlRoot);
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
                        _ = groupElement.AppendChild(designElement);
                    }
                    _ = xmlRoot.AppendChild(groupElement);
                }

                if (_orbitalBatteryDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("OrbitalBatteries");
                    foreach (OrbitalBatteryDesign design in _orbitalBatteryDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("OrbitalBattery");
                        design.AppendXml(designElement);
                        _ = groupElement.AppendChild(designElement);
                    }
                    _ = xmlRoot.AppendChild(groupElement);
                }

                if (_buildingDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("Buildings");
                    foreach (BuildingDesign design in _buildingDesigns)
                    {
                        //if (design is ShipyardDesign)
                        //    continue;


                        XmlElement designElement = xmlDoc.CreateElement("Building");
                        GameLog.Client.GameData.DebugFormat("designBuildings={0}, {1}", design, designElement);
                        design.AppendXml(designElement);
                        _ = groupElement.AppendChild(designElement);
                    }
                    _ = xmlRoot.AppendChild(groupElement);
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
                            _ = groupElement.AppendChild(designElement);
                        }
                    }
                    if (groupElement.ChildNodes.Count > 0)
                    {
                        _ = xmlRoot.AppendChild(groupElement);
                    }
                }

                if (_stationDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("SpaceStations");
                    foreach (StationDesign design in _stationDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("SpaceStation");
                        design.AppendXml(designElement);
                        _ = groupElement.AppendChild(designElement);
                    }
                    _ = xmlRoot.AppendChild(groupElement);
                }

                if (_shipDesigns.Count > 0)
                {
                    XmlElement groupElement = xmlDoc.CreateElement("Ships");
                    foreach (ShipDesign design in _shipDesigns)
                    {
                        XmlElement designElement = xmlDoc.CreateElement("Ship");
                        design.AppendXml(designElement);
                        _ = groupElement.AppendChild(designElement);
                    }
                    _ = xmlRoot.AppendChild(groupElement);
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
            foreach (ProductionFacilityDesign design in _productionFacilityDesigns)
            {
                yield return design;
            }

            foreach (BuildingDesign design in _buildingDesigns)
            {
                yield return design;
            }

            foreach (ShipyardDesign design in _shipyardDesigns)
            {
                yield return design;
            }

            foreach (ShipDesign design in _shipDesigns)
            {
                yield return design;
            }

            foreach (StationDesign design in _stationDesigns)
            {
                yield return design;
            }

            foreach (OrbitalBatteryDesign design in _orbitalBatteryDesigns)
            {
                yield return design;
            }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0038:Use pattern matching", Justification = "<Pending>")]
        public void Add(TechObjectDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException("design");
            }

            if (design is BuildingDesign)
            {
                _buildingDesigns.Add((BuildingDesign)design);
            }
            else if (design is ShipyardDesign)
            {
                _shipyardDesigns.Add((ShipyardDesign)design);
            }
            else if (design is ProductionFacilityDesign)
            {
                _productionFacilityDesigns.Add((ProductionFacilityDesign)design);
            }
            else if (design is ShipDesign)
            {
                _shipDesigns.Add((ShipDesign)design);
            }
            else if (design is StationDesign)
            {
                _stationDesigns.Add((StationDesign)design);
            }
            else if (design is OrbitalBatteryDesign)
            {
                _orbitalBatteryDesigns.Add((OrbitalBatteryDesign)design);
            }

            if (!DesignIdMap.ContainsKey(design.Key))
            {
                DesignIdMap[design.Key] = design.DesignID;
            }
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
                _ = DesignIdMap.Remove(design.Key);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0038:Use pattern matching", Justification = "<Pending>")]
        private bool DoRemove(TechObjectDesign design)
        {
            if (design is BuildingDesign)
            {
                return _buildingDesigns.Remove((BuildingDesign)design);
            }

            if (design is ShipyardDesign)
            {
                return _shipyardDesigns.Remove((ShipyardDesign)design);
            }

            if (design is ProductionFacilityDesign)
            {
                return _productionFacilityDesigns.Remove((ProductionFacilityDesign)design);
            }

            if (design is ShipDesign)
            {
                return _shipDesigns.Remove((ShipDesign)design);
            }

            if (design is StationDesign)
            {
                return _stationDesigns.Remove((StationDesign)design);
            }

            if (design is OrbitalBatteryDesign)
            {
                _ = _orbitalBatteryDesigns.Remove((OrbitalBatteryDesign)design);
            }

            return false;
        }

        #region IDeserializationCallback Members
        public void OnDeserialization(object sender)
        {
            _designIdMap = new Dictionary<string, int>();
            _ = this.ForEach(o => _designIdMap[o.Key] = o.DesignID);
        }
        #endregion
    }
}
