// File:CivTechTree.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Buildings;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Supremacy.Tech
{
    //private static IClientContext _clientContext;

    /// <summary>
    /// Represents the technology tree of a civilization.
    /// </summary>
    [Serializable]
    public class TechTree : IEnumerable<TechObjectDesign>
    {
        private const string XmlFilePath = "Resources/Data/TechTrees.xml";

        private readonly HashSet<int> _buildingDesigns;
        private readonly HashSet<int> _shipyardDesigns;
        private readonly HashSet<int> _shipDesigns;
        private readonly HashSet<int> _stationDesigns;
        private readonly HashSet<int> _productionFacilityDesigns;
        private readonly HashSet<int> _orbitalBatteryDesigns;
        private static string _text;
        private static string line;
        private static readonly string separator = ";";
        private static string file;


        //private IClientContext _clientContext;


        /// <summary>
        /// Gets the subset of building designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The building designs.</value>
        public IEnumerable<BuildingDesign> BuildingDesigns => _buildingDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<BuildingDesign>();

        /// <summary>
        /// Gets the subset of shipyard designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The shipyard designs.</value>
        public IEnumerable<ShipyardDesign> ShipyardDesigns => _shipyardDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<ShipyardDesign>();

        /// <summary>
        /// Gets the subset of ship designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The ship designs.</value>
        public IEnumerable<ShipDesign> ShipDesigns => _shipDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<ShipDesign>();

        /// <summary>
        /// Gets the subset of station designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The station designs.</value>
        public IEnumerable<StationDesign> StationDesigns => _stationDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<StationDesign>();

        /// <summary>
        /// Gets the subset of facility designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The facility designs.</value>
        public IEnumerable<ProductionFacilityDesign> ProductionFacilityDesigns => _productionFacilityDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<ProductionFacilityDesign>();

        /// <summary>
        /// Gets the subset of orbital battery designs in this <see cref="TechTree"/>.
        /// </summary>
        /// <value>The orbital battery designs.</value>
        public IEnumerable<OrbitalBatteryDesign> OrbitalBatteryDesigns => _orbitalBatteryDesigns
                    .Select(i => GameContext.Current.TechDatabase[i])
                    .OfType<OrbitalBatteryDesign>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="TechTree"/> is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="TechTree"/> is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => _buildingDesigns.Count == 0 &&
                       _shipyardDesigns.Count == 0 &&
                       _shipDesigns.Count == 0 &&
                       _stationDesigns.Count == 0 &&
                       _productionFacilityDesigns.Count == 0 &&
                       _orbitalBatteryDesigns.Count == 0;

        //public int localPlayerID { get => _clientContext.LocalPlayer.CivID; private set => 0; }

        //public int LocalPlayerEmpireID { get => _localPlayerEmpireID; }

        /// <summary>
        /// Determines whether this <see cref="TechTree"/> contains the specified <see cref="TechObjectDesign"/>.
        /// </summary>
        /// <param name="design">The design.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="TechTree"/> contains the specified <see cref="TechObjectDesign"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TechObjectDesign design)
        {
            if (design == null)
            {
                return false;
            }

            ProductionFacilityDesign pfDesign = design as ProductionFacilityDesign;
            if (pfDesign != null && _productionFacilityDesigns.Contains(pfDesign.DesignID))
            {
                return true;
            }

            BuildingDesign bDesign = design as BuildingDesign;
            if (bDesign != null && _buildingDesigns.Contains(bDesign.DesignID))
            {
                return true;
            }

            ShipyardDesign syDesign = design as ShipyardDesign;
            if (syDesign != null && _shipyardDesigns.Contains(syDesign.DesignID))
            {
                return true;
            }

            ShipDesign shDesign = design as ShipDesign;
            if (shDesign != null && _shipDesigns.Contains(shDesign.DesignID))
            {
                return true;
            }

            StationDesign stDesign = design as StationDesign;
            if (stDesign != null && _stationDesigns.Contains(stDesign.DesignID))
            {
                return true;
            }

            OrbitalBatteryDesign obDesign = design as OrbitalBatteryDesign;
            if (obDesign != null && _orbitalBatteryDesigns.Contains(obDesign.DesignID))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechTree"/> class.
        /// </summary>
        public TechTree()
        {
            _buildingDesigns = new HashSet<int>();
            _shipyardDesigns = new HashSet<int>();
            _shipDesigns = new HashSet<int>();
            _stationDesigns = new HashSet<int>();
            _productionFacilityDesigns = new HashSet<int>();
            _orbitalBatteryDesigns = new HashSet<int>();
        }

        /// <summary>
        /// Merges the specified <see cref="TechTree"/> with this <see cref="TechTree"/>.
        /// </summary>
        /// <param name="tree">The <see cref="TechTree"/> to merge with this <see cref="TechTree"/>.</param>
        /// <remarks>
        /// Any designs contained in the specified <see cref="TechTree"/> that are not already present
        /// are added to this <see cref="TechTree"/>.
        /// </remarks>
        public void Merge(TechTree tree)
        {
            TechDatabase techDatabase = GameContext.Current.TechDatabase;
            ILookup<Tuple<ProductionCategory, int>, IGrouping<Tuple<ProductionCategory, int>
                , ProductionFacilityDesign>> nativeFacilities =
                (from id in _productionFacilityDesigns
                 let design = techDatabase.ProductionFacilityDesigns[id]
                 group design by new Tuple<ProductionCategory, int>(
                     design.Category,
                     GetMaxTechLevel(design))).ToLookup(o => o.Key, o => o);

            _productionFacilityDesigns.UnionWith(
                tree.ProductionFacilityDesigns
                    .Where(o => !nativeFacilities[new Tuple<ProductionCategory, int>(o.Category, GetMaxTechLevel(o))].Any())
                    .Select(o => o.DesignID));

            _buildingDesigns.UnionWith(tree.BuildingDesigns.Select(o => o.DesignID));
            _shipyardDesigns.UnionWith(tree.ShipyardDesigns.Select(o => o.DesignID));
            _shipDesigns.UnionWith(tree.ShipDesigns.Select(o => o.DesignID));
            _stationDesigns.UnionWith(tree.StationDesigns.Select(o => o.DesignID));
            _orbitalBatteryDesigns.UnionWith(tree.OrbitalBatteryDesigns.Select(o => o.DesignID));
        }

        //private static IClientContext ClientContext
        //{
        //    get
        //    {
        //        if (_clientContext == null)
        //        {
        //            //_clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
        //            _clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
        //        }

        //        return _clientContext;
        //    }
        //}

        private static int GetMaxTechLevel(IBuildable design)
        {
            return design.TechRequirements.Max(r => r.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechTree"/> class from XML data.
        /// </summary>
        /// <param name="xmlElement">The XML element.</param>
        internal TechTree(XmlNode xmlElement) : this()
        {
            TechDatabase db = GameContext.Current.TechDatabase;
            //LocalPlayer = ClientContext.LocalPlayerEmpire

            XmlElement xmlRoot = xmlElement["ProductionFacilities"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlFacility in xmlRoot.GetElementsByTagName("ProductionFacility").Cast<XmlElement>())
                {
                    string designKey = xmlFacility.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    ProductionFacilityDesign design = db.ProductionFacilityDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {
                        _ = _productionFacilityDesigns.Add(design.DesignID);
                    }
                }
            }

            xmlRoot = xmlElement["Buildings"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlBuilding in xmlRoot.GetElementsByTagName("Building").Cast<XmlElement>())
                {
                    string designKey = xmlBuilding.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    BuildingDesign design = db.BuildingDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {

                        ///*var playerID = */int.TryParse(ClientConnectionState.AssignedID.ToString(), out int playerID);
                        //    //_clientContext.LocalPlayer.CivID;
                        //if (playerID != 6)
                        //{
                        _ = _buildingDesigns.Add(design.DesignID);
                        //}
                        //else
                        //{
                        //    if (!design.IsUniversallyAvailable)
                        //        _ = _buildingDesigns.Add(design.DesignID);
                        //}
                    }
                }
            }

            xmlRoot = xmlElement["Shipyards"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlShipyard in xmlRoot.GetElementsByTagName("Shipyard").Cast<XmlElement>())
                {
                    string designKey = xmlShipyard.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    ShipyardDesign design = db.ShipyardDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {
                        _ = _shipyardDesigns.Add(design.DesignID);
                    }
                }
            }

            xmlRoot = xmlElement["Ships"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlShip in xmlRoot.GetElementsByTagName("Ship").Cast<XmlElement>())
                {
                    string designKey = xmlShip.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    ShipDesign design = db.ShipDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {
                        _ = _shipDesigns.Add(design.DesignID);
                    }
                }
            }

            xmlRoot = xmlElement["SpaceStations"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlStation in xmlRoot.GetElementsByTagName("SpaceStation").Cast<XmlElement>())
                {
                    string designKey = xmlStation.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    StationDesign design = db.StationDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {
                        _ = _stationDesigns.Add(design.DesignID);
                    }
                }
            }

            xmlRoot = xmlElement["OrbitalBatteries"];
            if (xmlRoot != null)
            {
                foreach (XmlElement xmlBattery in xmlRoot.GetElementsByTagName("OrbitalBattery").Cast<XmlElement>())
                {
                    string designKey = xmlBattery.InnerText.Trim();
                    if (!db.DesignIdMap.ContainsKey(designKey))
                    {
                        continue;
                    }

                    OrbitalBatteryDesign design = db.OrbitalBatteryDesigns[db.DesignIdMap[designKey]];
                    if ((design != null) && !design.IsUniversallyAvailable)
                    {
                        _ = _orbitalBatteryDesigns.Add(design.DesignID);
                    }
                }
            }
        }

        /// <summary>
        /// Loads all of the tech trees for the specified <see cref="GameContext"/>.
        /// </summary>
        /// <param name="game">The game context.</param>
        public static void LoadTechTrees(GameContext game)
        {
            GameContext oldThreadContext = GameContext.ThreadContext;
            GameContext.PushThreadContext(game);
            try
            {
                TechDatabase db = game.TechDatabase;
                XmlDocument xmlDoc = new XmlDocument();
                XmlSchemaSet schemas = new XmlSchemaSet();

                _ = schemas.Add("Supremacy:Supremacy.xsd",
                            ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
                _ = schemas.Add("Supremacy:TechTrees.xsd",
                            ResourceManager.GetResourcePath("Resources/Data/TechTrees.xsd"));

                xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
                xmlDoc.Schemas.Add(schemas);
                xmlDoc.Validate(ValidateXml);

                TechTree defaultTechTree = new TechTree();
                foreach (TechObjectDesign design in db)
                {
                    if (design.IsUniversallyAvailable)
                    {
                        defaultTechTree.Add(design);
                    }
                }

                game.TechTrees.Default = defaultTechTree;


                // CSV_Output

                bool _bool_TechTrees_List = false;
                //file = pathOutputFile + "_TechTrees_List(autoCreated).csv";

                if (_bool_TechTrees_List)
                {

                    string pathOutputFile = "./lib/";  // instead of ./Resources/Data/
                    string separator = ";";
                    string line = "";
                    StreamWriter streamWriter;
                    string file = pathOutputFile + "test-Output.txt";
                    streamWriter = new StreamWriter(file);
                    streamWriter.Close();
                    string strHeader = "";  // first line of output files

                    file = pathOutputFile + "_TechTrees_List(autoCreated).csv";

                    Console.WriteLine("Step_9120: writing {0}", file); // _TechTrees_List(autoCreated).csv
                    file = null; // quick offline

                    if (file == null)
                    {
                        goto WriterClose;
                    }

                    streamWriter = new StreamWriter(file);

                    strHeader = "CIV;KIND;KEY;IsUniversal";  // Head line

                    streamWriter.WriteLine(strHeader);

                    // End of head line

                    foreach (TechObjectDesign design in db.ProductionFacilityDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "ProdFac" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }
                    foreach (TechObjectDesign design in db.BuildingDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "Building" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }
                    foreach (TechObjectDesign design in db.ShipDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "Ship" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }
                    foreach (TechObjectDesign design in db.ShipyardDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "Shipyard" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }
                    foreach (TechObjectDesign design in db.StationDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "Station" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }
                    foreach (TechObjectDesign design in db.OrbitalBatteryDesigns)
                    {
                        if (design.IsUniversallyAvailable)
                        {
                            line = "All" + separator + "OrbitalBattery" + separator + design.Key + separator + "Universal";
                            streamWriter.WriteLine(line);
                        }
                    }

                    _text = "written: _TechTrees_List(autoCreated).csv";
                    Console.WriteLine(_text);

                    WriterClose:;
                    streamWriter.Close();
                }


                // GameStuff
                game.TechTrees.Default = defaultTechTree;



                if (xmlDoc.DocumentElement != null)
                {


                    foreach (XmlElement xmlTree in xmlDoc.DocumentElement.GetElementsByTagName("TechTree").Cast<XmlElement>())
                    {
                        // first CSV_Output ... necessary to get all races incl. minors

                        // second: Game Stuff

                        #region TechTrees_To_CSV

                        try
                        {
                            //var civManager = game.CivilizationManagers[xmlTree.GetAttribute("Civilization")];
                            //if (civManager == null)
                            //    continue;
                            TechTree techTree = new TechTree(xmlTree);
                            string pathOutputFile = "./lib/";  // instead of ./Resources/Data/
                            file = pathOutputFile + "_TechTrees_List(autoCreated).csv";
                            StreamWriter streamWriter = new StreamWriter(file);

                            bool _streamWriterWorks = false;

                            bool _traceTechTrees = false;
                            if (_traceTechTrees == true)
                            {
                                string owner = xmlTree.GetAttribute("Civilization");
                                string category = "";

                                foreach (ProductionFacilityDesign item in techTree.ProductionFacilityDesigns)
                                {
                                    category = "ProdFac";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + "ProdFac" + separator + item.ToString() + separator + isUniversal;
                                    Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks) streamWriter.WriteLine(line);
                                }

                                foreach (BuildingDesign item in techTree.BuildingDesigns)
                                {
                                    category = "Building";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + "Building" + separator + item.ToString() + separator + isUniversal;
                                    Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks) streamWriter.WriteLine(line);
                                }
                                foreach (ShipDesign item in techTree.ShipDesigns)
                                {
                                    category = "Ship";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + "Ship" + separator + item.ToString() + separator + isUniversal;
                                    //Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks) streamWriter.WriteLine(line);
                                }
                                foreach (ShipyardDesign item in techTree.ShipyardDesigns)
                                {
                                    category = "Shipyards";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + "Shipyards" + separator + item.ToString() + separator + isUniversal;
                                    //Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks) streamWriter.WriteLine(line);
                                }
                                foreach (StationDesign item in techTree.StationDesigns)
                                {
                                    category = "Station";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + "Station" + separator + item.ToString() + separator + isUniversal;
                                    //Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks)
                                        streamWriter.WriteLine(line);
                                }
                                foreach (OrbitalBatteryDesign item in techTree.OrbitalBatteryDesigns)
                                {
                                    category = "OrbitalBattery";
                                    string isUniversal = item.IsUniversallyAvailable.ToString();
                                    //GameLog.Core.GameData.DebugFormat("{0}; {1}", owner, item.ToString());

                                    line = owner + separator + category + separator + item.ToString() + separator + isUniversal;
                                    //Console.WriteLine("{0}", line);
                                    if (_streamWriterWorks)
                                        streamWriter.WriteLine(line);
                                }
                            }
                            streamWriter.Close();
                        }
                        catch (Exception e)
                        {
                            _text = "Cannot write ... " + file + e;
                            GameLog.Core.GameData.ErrorFormat(_text);
                        }


                        // End of TechTrees_To_CSV
                        #endregion TechTrees_To_CSV



                        // Game Stuff
                        // its quite possible that the tech tree we are trying to load doesn't have a corresponding CivManager.
                        // If the civilization is not part of the current game, then we don't need it's data
                        try
                        {
                            CivilizationManager civManager = game.CivilizationManagers[xmlTree.GetAttribute("Civilization")];
                            if (civManager == null)
                            {
                                continue;
                            }

                            TechTree techTree = new TechTree(xmlTree);
                            techTree.Merge(defaultTechTree);
                            civManager.TechTree = techTree;
                        }
                        catch (Exception e)
                        {
                            GameLog.Core.GameData.DebugFormat("TechTree exception {0} {1}", e.Message, e.StackTrace);
                        }
                    

                    //WriterClose:;
                    //streamWriter.Close();
                }
                //WriterClose2:;
            }
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }
        }

        /// <summary>
        /// Adds the specified design to this <see cref="TechTree"/>.
        /// </summary>
        /// <param name="design">The design to add.</param>
        public void Add(TechObjectDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException("design");
            }

            if (design is BuildingDesign)
            {
                _ = _buildingDesigns.Add(design.DesignID);
            }
            else if (design is ShipyardDesign)
            {
                _ = _shipyardDesigns.Add(design.DesignID);
            }
            else if (design is ProductionFacilityDesign)
            {
                _ = _productionFacilityDesigns.Add(design.DesignID);
            }
            else if (design is ShipDesign)
            {
                _ = _shipDesigns.Add(design.DesignID);
            }
            else if (design is StationDesign)
            {
                _ = _stationDesigns.Add(design.DesignID);
            }
            else if (design is OrbitalBatteryDesign)
            {
                _ = _orbitalBatteryDesigns.Add(design.DesignID);
            }
        }

        /// <summary>
        /// Adds the specified designs to this <see cref="TechTree"/>.
        /// </summary>
        /// <param name="designs">The designs to add.</param>
        public void AddMany(IEnumerable<TechObjectDesign> designs)
        {
            if (designs == null)
            {
                throw new ArgumentNullException("designs");
            }

            foreach (TechObjectDesign design in designs)
            {
                Add(design);
            }
        }

        /// <summary>
        /// Removes the specified design from this <see cref="TechTree"/>.
        /// </summary>
        /// <param name="design">The design to remove.</param>
        /// <returns></returns>
        public bool Remove(TechObjectDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException("design");
            }

            if (design is BuildingDesign)
            {
                return _buildingDesigns.Remove(design.DesignID);
            }

            if (design is ShipyardDesign)
            {
                return _shipyardDesigns.Remove(design.DesignID);
            }

            if (design is ProductionFacilityDesign)
            {
                return _productionFacilityDesigns.Remove(design.DesignID);
            }

            if (design is ShipDesign)
            {
                return _shipDesigns.Remove(design.DesignID);
            }

            if (design is StationDesign)
            {
                return _stationDesigns.Remove(design.DesignID);
            }

            if (design is OrbitalBatteryDesign)
            {
                return _orbitalBatteryDesigns.Remove(design.DesignID);
            }

            return false;
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

        #region IEnumerable<TechObjectDesign> Members
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<TechObjectDesign> GetEnumerator()
        {
            foreach (ProductionFacilityDesign design in _productionFacilityDesigns.Select(i => GameContext.Current.TechDatabase[i] as ProductionFacilityDesign))
            {
                yield return design;
            }

            foreach (BuildingDesign design in _buildingDesigns.Select(i => GameContext.Current.TechDatabase[i] as BuildingDesign))
            {
                yield return design;
            }

            foreach (ShipyardDesign design in _shipyardDesigns.Select(i => GameContext.Current.TechDatabase[i] as ShipyardDesign))
            {
                yield return design;
            }

            foreach (ShipDesign design in _shipDesigns.Select(i => GameContext.Current.TechDatabase[i] as ShipDesign))
            {
                yield return design;
            }

            foreach (StationDesign design in _stationDesigns.Select(i => GameContext.Current.TechDatabase[i] as StationDesign))
            {
                yield return design;
            }

            foreach (OrbitalBatteryDesign design in _orbitalBatteryDesigns.Select(i => GameContext.Current.TechDatabase[i] as OrbitalBatteryDesign))
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

        public void ClearBuildingDesigns()
        {
            _buildingDesigns.Clear();
        }

        public void ClearShipyardDesigns()
        {
            _shipyardDesigns.Clear();
        }

        public void ClearStationDesigns()
        {
            _stationDesigns.Clear();
        }

        public void ClearShipDesigns()
        {
            _shipDesigns.Clear();
        }

        public void ClearProductionFacilityDesigns()
        {
            _productionFacilityDesigns.Clear();
        }

        public void ClearOrbitalBatteryDesigns()
        {
            _orbitalBatteryDesigns.Clear();
        }
    }
}
