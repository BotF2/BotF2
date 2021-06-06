// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Schema;

namespace Supremacy.Universe
{
    [Serializable]
    public class HomeSystemsDatabase : Dictionary<string, StarSystemDescriptor>
    {
        private const string XmlFilePath = "Resources/Data/HomeSystems.xml";

        private static void ValidateXml(object sender, ValidationEventArgs e)
        {
            XmlHelper.ValidateXml(XmlFilePath, e);
        }

        public static HomeSystemsDatabase Load()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            HomeSystemsDatabase db = new HomeSystemsDatabase();
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement xmlRoot;

            schemas.Add("Supremacy:Supremacy.xsd",
                        ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
            schemas.Add("Supremacy:TechObjectDatabase.xsd",
                        ResourceManager.GetResourcePath("Resources/Data/TechObjectDatabase.xsd"));

            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
            xmlDoc.Schemas.Add(schemas);
            xmlDoc.Validate(ValidateXml);

            xmlRoot = xmlDoc.DocumentElement;

            var separator = ";";
            //var line = "";
            StreamWriter streamWriter;
            StreamWriter streamWriter2;
            var pathOutputFile = "./Resources/Data/";  // instead of ./Resources/Data/
            var file = "./lib/test-FromHomeSystems.txt";
            var file2 = "./lib/test2-FromHomeSystems.txt";
            streamWriter = new StreamWriter(file);
            streamWriter2 = new StreamWriter(file2);
            streamWriter.Close();
            streamWriter2.Close();
            String strHeader = "";  // first line of output files
            String strLine = "";   // each civ gets one line
            String strLine2 = "";   // each civ gets one line


            try // avoid hang up if this file is opened by another program 
            {
                // better //  file = "./From_HomeSystemsXML_(autoCreated).csv";
                file = pathOutputFile + "z_FromHomeSystemsXML_(autoCreated).csv";

                Console.WriteLine("writing {0}", file);

                if (file == null)
                    goto WriterCloseHomeSystemsXML;


                streamWriter = new StreamWriter(file);

                strHeader =    // Head line
                    "Civilization" + separator +
                    "StarName" + separator +

                    "StarType" + separator +
                    "Inhabitants" + separator +
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



                file2 = pathOutputFile + "z_FromHomeSystemsXML_StartingLevel_(autoCreated).csv";

                Console.WriteLine("writing {0}", file2);

                if (file2 == null)
                    goto WriterCloseHomeSystemsXML;


                streamWriter2 = new StreamWriter(file2);

                string strHeader2 =    // Head line
                    "Civilization" + separator +
                    "StarName" + separator +

                    "StarType" + separator +
                    "Inhabitants" + separator +
                    "CE_Computers" + separator +
                    "CE_Construction" + separator +
                    //"CE_Energy" + separator +
                    //"CE_Propulsion" + separator +
                    //"CE_Weapons" + separator +
                    //"CE_BuildCost" + separator +
                    //"CE_IsUniversallyAvailable" + separator +
                    //"CE_Prerequisites" + separator +
                    //"CE_ObsoletedItems" + separator +
                    //"CE_UpgradeOptions" + separator +
                    //"CE_Restrictions" + separator +
                    ////"CE_EnergyCosts_not_used_anymore?" + separator +
                    //"CE_BuildSlots" + separator +
                    //"CE_BuildSlotMaxOutput" + separator +
                    //"CE_BuildSlotOutputType" + separator +
                    //"CE_BuildSlotOutput" + separator +
                    //"CE_BuildSlotEnergyCost" + separator +
                    "CE_MaxBuildTechLevel";

                foreach (XmlElement homeSystemElement in xmlRoot.GetElementsByTagName("HomeSystem"))
                {
                    //GameLog.Core.XMLCheck.DebugFormat("HomeSystems CIV = {0}", homeSystemElement.GetAttribute("Civilization").Trim().ToUpperInvariant());

                    string civId = homeSystemElement.GetAttribute("Civilization").Trim().ToUpperInvariant();
                    db[civId] = new StarSystemDescriptor(homeSystemElement["StarSystem"]);
                    //GameLog.Client.GameData.DebugFormat("HomeSystems.xml-civId={0}", civId);

                    strLine =
                        civId + separator +
                        db[civId].Name + separator +
                        db[civId].StarType + separator +
                        db[civId].Inhabitants + separator +
                        //db[civId].inh + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        db[civId].Name + separator
                        ;

                    streamWriter.WriteLine(strLine);





                    strLine2 =
                        civId + separator +       //  following entries not working yet
                                                  //db[civId].StartingOutposts + separator +
                                                  //db[civId].StartingShipyards + separator +
                                                  //db[civId].StartingBuildings + separator +
                                                  //db[civId].StartingOrbitalBatteries.Count + separator +
                                                  //db[civId].StartingShips.Count + separator +

                        //db[civId].FoodPF.DesignType + separator +
                        //db[civId].FoodPF.Active + separator +
                        //db[civId].FoodPF.Count + separator +

                        //db[civId].IndustryPF.DesignType + separator +
                        //db[civId].IndustryPF.Active + separator +
                        //db[civId].IndustryPF.Count + separator +


                        //db[civId].EnergyPF.DesignType + separator +
                        //db[civId].EnergyPF.Active + separator +
                        //db[civId].EnergyPF.Count + separator +

                        //db[civId].ResearchPF.DesignType + separator +
                        //db[civId].ResearchPF.Active + separator +
                        //db[civId].ResearchPF.Count + separator +

                        //db[civId].IntelligencePF.DesignType + separator +
                        //db[civId].IntelligencePF.Active + separator +
                        //db[civId].IntelligencePF.Count + separator +


                        //db[civId].IndustryPF.Count + separator +
                        //db[civId].FoodPF.Count + separator +
                        //db[civId].EnergyPF.Count + separator +
                        //db[civId].ResearchPF.Count + separator +
                        //db[civId].IntelligencePF.count + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        //db[civId].Name + separator +
                        db[civId].Name + separator
                        ;

                    streamWriter2.WriteLine(strLine2);


                }  // end of foreach
            WriterCloseHomeSystemsXML:;
                streamWriter.Close();
                streamWriter2.Close();
            }
            catch (Exception e)
            {
                GameLog.Core.GameData.Error("Problem with HomeSystems.xml or writing file z_FromHomeSystemsXML_(autoCreated).csv", e);
            }


            return db;
        }

        public void Save()
        {
            string path = Path.Combine(
                Environment.CurrentDirectory,
                XmlFilePath);
            Save(path);
        }

        public void Save(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                Save(writer);
            }
        }

        public void Save(TextWriter writer)
        {
            using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement rootElement = xmlDoc.CreateElement("HomeSystems");

                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 2;
                xmlWriter.IndentChar = ' ';

                xmlDoc.AppendChild(rootElement);

                foreach (string civId in Keys)
                {
                    XmlElement homeSystemElement = xmlDoc.CreateElement("HomeSystem");
                    homeSystemElement.SetAttribute("Civilization", civId);
                    this[civId].AppendXml(homeSystemElement);
                    rootElement.AppendChild(homeSystemElement);
                }

                xmlDoc.WriteTo(xmlWriter);
            }
        }
    }

    [Serializable]
    public sealed class ProductionFacilityDescriptor : INotifyPropertyChanged
    {
        private string _designType;
        private float _count = -1.0f;
        private float _active = -1.0f;

        public ProductionFacilityDescriptor()
        {
            _designType = "";
        }

        #region Properties
        public string DesignType
        {
            get { return _designType; }
            set
            {
                _designType = value;
                OnPropertyChanged("DesignType");
            }
        }

        public float Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        public float Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged("Active");
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    [Serializable]
    public sealed class StarSystemDescriptor : INotifyPropertyChanged
    {
        #region Fields
        private BindingList<PlanetDescriptor> _planets;
        private string _systemName;
        private string _inhabitants;
        private StarType? _starType;
        private SystemBonus _bonuses = SystemBonus.Random;
#pragma warning disable IDE0044 // Add readonly modifier
        private List<string> _startingShips;
        private List<string> _startingShipyards;
        private List<string> _startingBuildings;
        private List<string> _startingOutposts;
        private List<string> _startingOrbitalBatteries;
        private float _populationRatio = -1.0f;
        private float _credits = -1.0f;
        private float _deuterium = -1.0f;
        private float _dilithium = -1.0f;
        private float _rawMaterials = -1.0f;
        private float _food = -1.0f;
        private float _morale = -1.0f;
        private ProductionFacilityDescriptor _foodPF = null;
        private ProductionFacilityDescriptor _industryPF = null;
        private ProductionFacilityDescriptor _energyPF = null;
        private ProductionFacilityDescriptor _researchPF = null;
        private ProductionFacilityDescriptor _intelligencePF = null;
#pragma warning restore IDE0044 // Add readonly modifier
        #endregion

        #region Constructors
        public StarSystemDescriptor()
        {
            _planets = new BindingList<PlanetDescriptor>();
            _startingShips = new List<string>();
            _startingShipyards = new List<string>();
            _startingBuildings = new List<string>();
            _startingOutposts = new List<string>();
            _startingOrbitalBatteries = new List<string>();
        }
        #endregion

        public bool HasBonus(SystemBonus bonus)
        {
            return ((bonus & _bonuses) == bonus);
        }

        public void AddBonus(SystemBonus bonus)
        {
            _bonuses |= bonus;
        }

        public void RemoveBonus(SystemBonus bonus)
        {
            _bonuses &= ~bonus;
        }

        #region Properties
        public SystemBonus Bonuses
        {
            get { return _bonuses; }
            set
            {
                _bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("HasBonuses");
            }
        }

        [DependsOn("Bonuses")]
        public bool HasBonuses => (_bonuses != 0);

        public string Inhabitants
        {
            get { return _inhabitants; }
            set
            {
                _inhabitants = value;
                OnPropertyChanged("Inhabitants");
                OnPropertyChanged("IsInhabitantsDefined");
            }
        }

        public BindingList<PlanetDescriptor> Planets
        {
            get { return _planets; }
            internal set
            {
                if (value != _planets)
                {
                    _planets = value ?? throw new ArgumentNullException();
                }
            }
        }

        public string Name
        {
            get { return _systemName ?? String.Empty; }
            set
            {
                if (value != _systemName)
                {
                    _systemName = value?.Trim();
                    OnPropertyChanged("Name");
                    OnPropertyChanged("IsNameDefined");
                }
            }
        }

        public StarType? StarType
        {
            get { return _starType; }
            set
            {
                if (value != _starType)
                {
                    _starType = value;
                    OnPropertyChanged("StarType");
                    OnPropertyChanged("IsStarTypeDefined");
                }
            }
        }

        [DependsOn("Inhabitants")]
        public bool IsInhabitantsDefined => (_inhabitants != null);

        [DependsOn("StarType")]
        public bool IsStarTypeDefined => (_starType != null);

        [DependsOn("Name")]
        public bool IsNameDefined => (_systemName != null);

        public List<string> StartingShips => _startingShips;

        public List<string> StartingShipyards => _startingShipyards;

        public List<string> StartingBuildings => _startingBuildings;

        public List<string> StartingOutposts => _startingOutposts;

        public List<string> StartingOrbitalBatteries => _startingOrbitalBatteries;

        public float PopulationRatio => _populationRatio;

        public float Credits => _credits;

        public float Deuterium => _deuterium;

        public float Dilithium => _dilithium;

        public float RawMaterials => _rawMaterials;

        public float Food => _food;

        public float Morale => _morale;

        public ProductionFacilityDescriptor FoodPF => _foodPF;

        public ProductionFacilityDescriptor IndustryPF => _industryPF;

        public ProductionFacilityDescriptor EnergyPF => _energyPF;

        public ProductionFacilityDescriptor ResearchPF => _researchPF;

        public ProductionFacilityDescriptor IntelligencePF => _intelligencePF;
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public StarSystemDescriptor(XmlElement xmlNode) : this()
        {
            if (xmlNode.HasAttribute("Name"))
                Name = xmlNode.GetAttribute("Name").Trim();
            if (xmlNode.HasAttribute("StarType") && Enum.IsDefined(typeof(StarType), xmlNode.GetAttribute("StarType").Trim()))
                StarType = (StarType)Enum.Parse(typeof(StarType), xmlNode.GetAttribute("StarType").Trim());
            if (xmlNode["Inhabitants"] != null)
                Inhabitants = xmlNode["Inhabitants"].InnerText.Trim().ToUpperInvariant();

            var bonusElements = xmlNode.GetElementsByTagName("Bonus");
            if (bonusElements.Count > 0)
            {
                _bonuses = SystemBonus.NoBonus;

                foreach (XmlElement bonusElement in bonusElements)
                {
                    if (Enum.IsDefined(typeof(SystemBonus), bonusElement.GetAttribute("Type").Trim()))
                        AddBonus((SystemBonus)Enum.Parse(typeof(SystemBonus), bonusElement.GetAttribute("Type").Trim()));
                }
            }

            if (xmlNode["Planets"] != null)
            {
                foreach (XmlElement planetElement in xmlNode["Planets"].GetElementsByTagName("Planet"))
                    Planets.Add(new PlanetDescriptor(planetElement));
            }

            var startingLevelTech = xmlNode.GetElementsByTagName("TechLevel");
            if (startingLevelTech.Count > 0)
            {
                var curStartingLevel = GameContext.Current.Options.StartingTechLevel.ToString().ToUpperInvariant();
                foreach (XmlElement techLevel in startingLevelTech)
                {
                    if (techLevel.HasAttribute("Name"))
                    {
                        var techLevelName = techLevel.GetAttribute("Name").Trim().ToUpperInvariant();
                        if (techLevelName.Equals(curStartingLevel))
                        {
                            // population ratio
                            if (techLevel.HasAttribute("PopulationRatio"))
                            {
                                var popRatio = techLevel.GetAttribute("PopulationRatio").Trim().ToUpperInvariant();
                                try
                                {
                                    _populationRatio = float.Parse(popRatio, System.Globalization.CultureInfo.InvariantCulture) / 100.0f;
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            // credits
                            if (techLevel.HasAttribute("Credits"))
                            {
                                var credits = techLevel.GetAttribute("Credits").Trim().ToUpperInvariant();
                                try
                                {
                                    _credits = float.Parse(credits, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            // Resources
                            if (techLevel.HasAttribute("Deuterium"))
                            {
                                var res = techLevel.GetAttribute("Deuterium").Trim().ToUpperInvariant();
                                try
                                {
                                    _deuterium = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            if (techLevel.HasAttribute("Dilithium"))
                            {
                                var res = techLevel.GetAttribute("Dilithium").Trim().ToUpperInvariant();
                                try
                                {
                                    _dilithium = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            if (techLevel.HasAttribute("RawMaterials"))
                            {
                                var res = techLevel.GetAttribute("RawMaterials").Trim().ToUpperInvariant();
                                try
                                {
                                    _rawMaterials = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            if (techLevel.HasAttribute("Food"))
                            {
                                var res = techLevel.GetAttribute("Food").Trim().ToUpperInvariant();
                                try
                                {
                                    _food = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            // morale
                            if (techLevel.HasAttribute("Morale"))
                            {
                                var res = techLevel.GetAttribute("Morale").Trim().ToUpperInvariant();
                                try
                                {
                                    _morale = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            // Production Facilities
                            if (techLevel["Food"] != null)
                            {
                                XmlElement pf = techLevel["Food"];
                                _foodPF = new ProductionFacilityDescriptor { DesignType = pf.InnerText.Trim().ToUpperInvariant() };

                                if (pf.HasAttribute("Count"))
                                {
                                    var val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _foodPF.Count = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                                if (pf.HasAttribute("Active"))
                                {
                                    var val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _foodPF.Active = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                            }

                            if (techLevel["Industry"] != null)
                            {
                                XmlElement pf = techLevel["Industry"];

                                _industryPF = new ProductionFacilityDescriptor { DesignType = pf.InnerText.Trim().ToUpperInvariant() };

                                if (pf.HasAttribute("Count"))
                                {
                                    var val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _industryPF.Count = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                                if (pf.HasAttribute("Active"))
                                {
                                    var val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _industryPF.Active = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                            }

                            if (techLevel["Energy"] != null)
                            {
                                XmlElement pf = techLevel["Energy"];

                                _energyPF = new ProductionFacilityDescriptor { DesignType = pf.InnerText.Trim().ToUpperInvariant() };


                                if (pf.HasAttribute("Count"))
                                {
                                    var val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _energyPF.Count = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                                if (pf.HasAttribute("Active"))
                                {
                                    var val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _energyPF.Active = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                            }

                            if (techLevel["Research"] != null)
                            {
                                XmlElement pf = techLevel["Research"];

                                _researchPF = new ProductionFacilityDescriptor { DesignType = pf.InnerText.Trim().ToUpperInvariant() }; 

                                if (pf.HasAttribute("Count"))
                                {
                                    var val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _researchPF.Count = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                                if (pf.HasAttribute("Active"))
                                {
                                    var val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _researchPF.Active = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                            }

                            if (techLevel["Intelligence"] != null)
                            {
                                XmlElement pf = techLevel["Intelligence"];

                                _intelligencePF = new ProductionFacilityDescriptor { DesignType = pf.InnerText.Trim().ToUpperInvariant() };

                                if (pf.HasAttribute("Count"))
                                {
                                    var val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _intelligencePF.Count = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                                if (pf.HasAttribute("Active"))
                                {
                                    var val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
                                    try
                                    {
                                        _intelligencePF.Active = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception e)
                                    {
                                        GameLog.Core.GameData.Error(e);
                                    }
                                }
                            }

                            // ships to be spawned
                            var startingShips = techLevel.GetElementsByTagName("Ship");
                            if (startingShips.Count > 0)
                            {
                                foreach (XmlElement ship in startingShips)
                                {
                                    int shipCount = 1;
                                    if (ship.HasAttribute("Count"))
                                    {
                                        var val = ship.GetAttribute("Count").Trim().ToUpperInvariant();
                                        try
                                        {
                                            shipCount = int.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        catch (Exception e)
                                        {
                                            GameLog.Core.GameData.Error(e);
                                            shipCount = 0; // error in the xml, don't spawn the ship
                                        }
                                    }

                                    string shipDesign = ship.InnerText.Trim().ToUpperInvariant();
                                    for (int i = 0; i < shipCount; i++)
                                        _startingShips.Add(shipDesign);
                                }
                            }

                            // shipyards to be spawned
                            var startingShipyards = techLevel.GetElementsByTagName("Shipyard");
                            if (startingShipyards.Count > 0)
                            {
                                foreach (XmlElement shipyard in startingShipyards)
                                {
                                    _startingShipyards.Add(shipyard.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // buildings to be spawned
                            var startingBuildings = techLevel.GetElementsByTagName("Building");
                            if (startingBuildings.Count > 0)
                            {
                                foreach (XmlElement building in startingBuildings)
                                {
                                    _startingBuildings.Add(building.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // outposts to be spawned
                            var startingOutposts = techLevel.GetElementsByTagName("SpaceStation");
                            if (startingOutposts.Count > 0)
                            {
                                foreach (XmlElement outpost in startingOutposts)
                                {
                                    _startingOutposts.Add(outpost.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // OBs to be spawned
                            var startingOBs = techLevel.GetElementsByTagName("OrbitalBattery");
                            if (startingOBs.Count > 0)
                            {
                                foreach (XmlElement OB in startingOBs)
                                {
                                    int OBCount = 1;
                                    if (OB.HasAttribute("Count"))
                                    {
                                        var val = OB.GetAttribute("Count").Trim().ToUpperInvariant();
                                        try
                                        {
                                            OBCount = int.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        catch
                                        {
                                            OBCount = 0;  // error in the xml, don't spawn the ship
                                        }
                                    }

                                    string OBDesign = OB.InnerText.Trim().ToUpperInvariant();
                                    for (int i = 0; i < OBCount; i++)
                                        _startingOrbitalBatteries.Add(OBDesign);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        public void AppendXml(XmlElement baseElement)
        {
            var systemElement = baseElement.OwnerDocument.CreateElement("StarSystem");

            if (IsNameDefined)
                systemElement.SetAttribute("Name", Name);
            if (IsStarTypeDefined)
                systemElement.SetAttribute("StarType", StarType.Value.ToString());

            if (IsInhabitantsDefined)
            {
                var inhabitantsElement = systemElement.OwnerDocument.CreateElement("Inhabitants");
                inhabitantsElement.InnerText = Inhabitants;
                systemElement.AppendChild(inhabitantsElement);
            }

            var bonusesElement = systemElement.OwnerDocument.CreateElement("Bonuses");
            if (Bonuses == SystemBonus.NoBonus)
            {
                var bonusElement = systemElement.OwnerDocument.CreateElement("Bonus");
                bonusElement.SetAttribute("Type", SystemBonus.NoBonus.ToString());
                bonusesElement.AppendChild(bonusElement);
            }
            else
            {
                foreach (SystemBonus bonus in EnumUtilities.GetValues<SystemBonus>())
                {
                    if ((bonus != SystemBonus.NoBonus) && HasBonus(bonus))
                    {
                        var bonusElement = systemElement.OwnerDocument.CreateElement("Bonus");
                        bonusElement.SetAttribute("Type", bonus.ToString());
                        bonusesElement.AppendChild(bonusElement);
                    }
                }
            }
            systemElement.AppendChild(bonusesElement);

            if (Planets.Count > 0)
            {
                var planetsElement = systemElement.OwnerDocument.CreateElement("Planets");
                foreach (var planet in Planets)
                {
                    var planetElement = systemElement.OwnerDocument.CreateElement("Planet");
                    planet.AppendXml(planetElement);
                    planetsElement.AppendChild(planetElement);
                }
                systemElement.AppendChild(planetsElement);
            }

            baseElement.AppendChild(systemElement);
        }
    }

    [Serializable]
    public sealed class PlanetDescriptor : INotifyPropertyChanged
    {
        #region Fields
        private int _maxNumberOfPlanets;
        private int _minNumberOfPlanets;
        private string _planetName;
        private PlanetSize? _planetSize;
        private PlanetType? _planetType;
        private PlanetBonus _bonuses = PlanetBonus.Random;
        #endregion

        public bool HasBonus(PlanetBonus bonus)
        {
            return ((bonus & _bonuses) == bonus);
        }

        public void AddBonus(PlanetBonus bonus)
        {
            _bonuses |= bonus;
        }

        public void RemoveBonus(PlanetBonus bonus)
        {
            _bonuses &= ~bonus;
        }

        #region Properties
        public PlanetBonus Bonuses
        {
            get { return _bonuses; }
            set
            {
                _bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("HasBonuses");
            }
        }

        [DependsOn("Bonuses")]
        public bool HasBonuses => (_bonuses != 0);

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Name")]
        public bool IsNameDefined => (_planetName != null);

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Size")]
        public bool IsSizeDefined => (IsSinglePlanet && (_planetSize != null));

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Type")]
        public bool IsTypeDefined => (IsSinglePlanet && (_planetType != null));

        [DependsOn("IsSinglePlanet")]
        public string Name
        {
            get { return (IsSinglePlanet ? (_planetName ?? String.Empty) : String.Empty); }
            set
            {
                if (value != _planetName)
                {
                    _planetName = value?.Trim();
                    OnPropertyChanged("Name");
                    OnPropertyChanged("IsNameDefined");
                }
            }
        }

        [DependsOn("MaxNumberOfPlanets")]
        public bool IsSinglePlanet => (_maxNumberOfPlanets == 1);

        public int MinNumberOfPlanets
        {
            get { return _minNumberOfPlanets; }
            set
            {
                if (value != _minNumberOfPlanets)
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                    _minNumberOfPlanets = value;
                    OnPropertyChanged("MinNumberOfPlanets");
                }
            }
        }

        public int MaxNumberOfPlanets
        {
            get { return _maxNumberOfPlanets; }
            set
            {
                if (value != _maxNumberOfPlanets)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("value", "value must be greater than zero");
                    if (value > StarSystem.MaxPlanetsPerSystem)
                        throw new ArgumentOutOfRangeException("value", "value must be less than MaxPlanetsPerSystem");
                    _maxNumberOfPlanets = value;
                    OnPropertyChanged("MaxNumberOfPlanets");
                    OnIsSinglePlanetChanged();
                }
            }
        }

        [DependsOn("IsSinglePlanet")]
        public PlanetType? Type
        {
            get { return _planetType; }
            set
            {
                if (value != _planetType)
                {
                    _planetType = value;
                    OnPropertyChanged("Type");
                    OnPropertyChanged("IsTypeDefined");
                    if (_planetType == PlanetType.Asteroids)
                        Size = PlanetSize.Asteroids;
                    else if (_planetType == PlanetType.GasGiant)
                        Size = PlanetSize.GasGiant;
                }
            }
        }

        [DependsOn("IsSinglePlanet")]
        public PlanetSize? Size
        {
            get { return _planetSize; }
            set
            {
                if (value != _planetSize)
                {
                    _planetSize = value;
                    OnPropertyChanged("Size");
                    OnPropertyChanged("IsSizeDefined");
                    if (_planetSize == PlanetSize.Asteroids)
                        Type = PlanetType.Asteroids;
                    else if (_planetSize == PlanetSize.GasGiant)
                        Type = PlanetType.GasGiant;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void OnIsSinglePlanetChanged()
        {
            OnPropertyChanged("IsSinglePlanet");
            OnPropertyChanged("IsNameDefined");
            OnPropertyChanged("IsSizeDefined");
            OnPropertyChanged("IsTypeDefined");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PlanetDescriptor()
        {
            _minNumberOfPlanets = 1;
            _maxNumberOfPlanets = 1;
        }

        public PlanetDescriptor(XmlElement xmlNode) : this()
        {

            if (xmlNode.HasAttribute("Name"))
                Name = xmlNode.GetAttribute("Name").Trim();
            if (xmlNode.HasAttribute("Size") && Enum.IsDefined(typeof(PlanetSize), xmlNode.GetAttribute("Size").Trim()))
                Size = (PlanetSize)Enum.Parse(typeof(PlanetSize), xmlNode.GetAttribute("Size").Trim());
            if (xmlNode.HasAttribute("Type") && Enum.IsDefined(typeof(PlanetType), xmlNode.GetAttribute("Type").Trim()))
                Type = (PlanetType)Enum.Parse(typeof(PlanetType), xmlNode.GetAttribute("Type").Trim());
            if ((xmlNode.HasAttribute("MaxNumberOfPlanets")) && int.TryParse(xmlNode.GetAttribute("MaxNumberOfPlanets").Trim(), out int tempInteger))
                MaxNumberOfPlanets = tempInteger;
            if ((xmlNode.HasAttribute("MinNumberOfPlanets")) && int.TryParse(xmlNode.GetAttribute("MinNumberOfPlanets").Trim(), out tempInteger))
                MinNumberOfPlanets = tempInteger;

            var bonusElements = xmlNode.GetElementsByTagName("Bonus");
            if (bonusElements.Count > 0)
            {
                _bonuses = PlanetBonus.NoBonus;

                foreach (XmlElement bonusElement in bonusElements)
                {
                    if (Enum.IsDefined(typeof(PlanetBonus), bonusElement.GetAttribute("Type").Trim()))
                        AddBonus((PlanetBonus)Enum.Parse(typeof(PlanetBonus), bonusElement.GetAttribute("Type").Trim()));
                }
            }

            if (!IsSizeDefined && IsTypeDefined)
            {
                if (Type.Value == PlanetType.Asteroids)
                    Size = PlanetSize.Asteroids;
                else if (Type.Value == PlanetType.GasGiant)
                    Size = PlanetSize.GasGiant;
            }
        }

        public void AppendXml(XmlElement baseElement)
        {
            if (IsSinglePlanet)
            {
                if (IsNameDefined)
                    baseElement.SetAttribute("Name", Name);
                if (IsTypeDefined)
                    baseElement.SetAttribute("Type", Type.ToString());
                if (IsSizeDefined)
                    baseElement.SetAttribute("Size", Size.ToString());
                if (HasBonuses)
                {
                    foreach (PlanetBonus bonus in EnumUtilities.GetValues<PlanetBonus>())
                    {
                        if ((bonus != PlanetBonus.Random) && HasBonus(bonus))
                        {
                            XmlElement bonusElement = baseElement.OwnerDocument.CreateElement("Bonus");
                            bonusElement.SetAttribute("Type", bonus.ToString());
                            baseElement.AppendChild(bonusElement);
                        }
                    }
                }
            }
            else
            {
                baseElement.SetAttribute("MinNumberOfPlanets", MinNumberOfPlanets.ToString());
                baseElement.SetAttribute("MaxNumberOfPlanets", MaxNumberOfPlanets.ToString());
            }
        }
        #endregion
    }
}