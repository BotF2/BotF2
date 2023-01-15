// File:StarSystemDescriptor.cs
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
        private static readonly string blank = " ";
        private static string _startingBuildingsSummary;
        private static string _text;
        private static string _startingShipsSummary = " ;";
        private static string _type;
        private static int _scoutCount;
        private static string _scoutText;
        private static int _frigateCount;
        private static string _frigateText;
        private static int _scienceCount;
        private static string _scienceText;
        private static int _cruiserCount;
        private static string _cruiserText;
        private static int _destroyerCount;
        private static string _destroyerText;
        private static int _commandCount;
        private static string _commandText;
        private static int _colonyCount;
        private static string _colonyText;
        private static int _constructionCount;
        private static string _constructionText;
        private static int _strikeCruiserCount; private static string _strikeCruiserText;
        private static int _transportCount; private static string _transportText;
        private static int _diploCount; private static string _diploText;
        private static int _spyCount; private static string _spyText;
        private static int _medicalCount; private static string _medicalText;
        private static string _startingStation;
        private static string _startingShipyard;
        private static string _startingOrbitalBatteries;
        private static string _startingFoodPF;
        private static string _startingIndustryPF;
        private static string _startingEnergyPF;
        private static string _startingResearchPF;
        private static string _startingIntelligencePF;

        //private static int _colonyCount; private static string _colonyText;

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

            _ = schemas.Add("Supremacy:Supremacy.xsd",
                        ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
            _ = schemas.Add("Supremacy:TechObjectDatabase.xsd",
                        ResourceManager.GetResourcePath("Resources/Data/TechObjectDatabase.xsd"));

            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
            xmlDoc.Schemas.Add(schemas);
            xmlDoc.Validate(ValidateXml);

            xmlRoot = xmlDoc.DocumentElement;

            string separator = " ;";
            string hyphen = "-";
            //var line = "";
            StreamWriter streamWriter;
            //StreamWriter streamWriter2;
            string pathOutputFile = "./lib/";  // instead of ./Resources/Data/
            string file = pathOutputFile + "test-Output.txt";
            //string file2 = "./lib/test2-FromHomeSystems.txt";
            streamWriter = new StreamWriter(file);
            //streamWriter2 = new StreamWriter(file2);
            streamWriter.Close();
            //streamWriter2.Close();
            string strHeader = "";  // first line of output files
            string strLine = "";   // each civ gets one line
            //string strLine2 = "";   // each civ gets one line

            try // avoid hang up if this file is opened by another program 
            {
                // better //  file = "./From_HomeSystemsXML_(autoCreated).csv";
                file = pathOutputFile + "_HomeSystems-xml_"
                    + GameContext.Current.Options.StartingTechLevel.ToString() + "_List(autoCreated).csv";

                Console.WriteLine("writing {0}", file);

                //...but with the next lines it doesn't loaded the entries from HomeSystem.xml anymore
                //file = null; // quick set to off
                //if (file == null)
                //{
                //    goto WriterCloseHomeSystemsXML;
                //}

                streamWriter = new StreamWriter(file);

                strHeader = blank; // Dummy, needed

                strHeader =    // Head line
                    "Civilization" + separator +
                    "TechLvl" + separator +

                    "FoodPF_active" + separator +
                    "FoodPF" + separator +

                    "FoodPF_blank" + separator +

                    "IndustryPF_active" + separator +
                    "IndustryPF" + separator +

                    "IndustryPF_blank" + separator +

                    "EnergyPF_active" + separator +
                    "EnergyPF" + separator +

                    "EnergyPF_blank" + separator +

                    "ResearchPF_active" + separator +
                    "ResearchPF" + separator +

                    "ResearchPF_blank" + separator +

                    "IntelPF_active" + separator +
                    "IntelPF" + separator +

                    "IntelPF_blank" + separator +

                    "StartShips" + separator +
                    "COL" + separator + // Colony Ships
                    "CON" + separator +
                    "SCO" + separator +
                    "FRI" + separator +
                    "DES" + separator +
                    "CRU" + separator +
                    "SCR" + separator +
                    "COM" + separator +
                    "SCI" + separator +
                    "MED" + separator +
                    "TRANS" + separator +
                    "DIP" + separator +
                    "SPY" + separator +

                    "StarSystem" + separator +
                    "RAT" + separator +
                    "CREDITS" + separator +
                    "DEU" + separator +
                    "DIL" + separator +
                    "DUR" + separator +
                    "FOOD" + separator +
                    "MOR" + separator +
                    "OB" + separator +
                    "OB_2" + separator +
                    "SYard" + separator +
                    "SYard_2" + separator +
                    "STAT" + separator +
                    "STAT_2" + separator +
                    "Buildings" + separator +
                    "B_1" + separator +
                    "B_2" + separator +
                    "B_3" + separator +
                    "B_4" + separator +
                    "B_5" + separator +
                    "B_6" + separator +
                    "B_7" + separator +
                    "B_8" + separator +
                    "B_9" + separator +
                    "B_10" + separator +
                    "B_11" + separator +
                    "B_12" + separator +
                    "B_13" + separator +
                    "B_14" + separator +
                    "B_15" + separator +
                    "B_16" + separator +
                    "B_17" + separator +
                    "B_18" + separator +
                    "B_19" + separator +
                    "B_20" + separator +
                    "B_21" + separator +

                    separator;

                streamWriter.WriteLine(strHeader);
                // End of head line

                //file2 = pathOutputFile + "./lib/HomeSystemsXML_StartingLevel_(autoCreated).csv";
                //Console.WriteLine("writing {0}", file2);

                //if (file2 == null)
                //{
                //    goto WriterCloseHomeSystemsXML;
                //}

                //streamWriter2 = new StreamWriter(file2);

                //string strHeader2 =    // Head line

                foreach (XmlElement homeSystemElement in xmlRoot.GetElementsByTagName("HomeSystem"))
                {
                    string civId = homeSystemElement.GetAttribute("Civilization").Trim().ToUpperInvariant();
                    db[civId] = new StarSystemDescriptor(homeSystemElement["StarSystem"]);

                    _scoutCount = 0; _scoutText = "SCOUT";
                    _frigateCount = 0; _frigateText = "FRIGATE";
                    _scienceCount = 0; _scienceText = "SCIENCE";
                    _medicalCount = 0; _medicalText = "MEDICAL";
                    _colonyCount = 0; _colonyText = "COLONY";
                    _transportCount = 0; _transportText = "TRANSPORT";
                    _diploCount = 0; _diploText = "DIPLO";
                    _spyCount = 0; _spyText = "SPY";
                    _destroyerCount = 0; _destroyerText = "DESTROYER";
                    _cruiserCount = 0; _cruiserText = "CRUISER";
                    _strikeCruiserCount = 0; _strikeCruiserText = "STRIKE_CRUISER";
                    _commandCount = 0; _commandText = "COMMAND";
                    _constructionCount = 0; _constructionText = "CONSTRUCTION";

                    foreach (string item in db[civId].StartingShips)
                    {
                        if (item.Contains("SCOUT")) { _type = "SCOUT"; }
                        if (item.Contains("FRIGATE")) { _type = "FRIGATE"; }
                        if (item.Contains("SCIENCE")) { _type = "SCIENCE"; }
                        if (item.Contains("MEDICAL")) { _type = "MEDICAL"; }
                        if (item.Contains("COLONY")) { _type = "COLONY"; }
                        if (item.Contains("TRANSPORT")) { _type = "TRANSPORT"; }
                        if (item.Contains("DIPLO")) { _type = "DIPLO"; }
                        if (item.Contains("SPY")) { _type = "SPY"; }
                        if (item.Contains("DESTROYER")) { _type = "DESTROYER"; }
                        if (item.Contains("CRUISER")) { _type = "CRUISER"; }
                        if (item.Contains("STRIKE_CRUISER")) { _type = "STRIKE_CRUISER"; }
                        if (item.Contains("COMMAND")) { _type = "COMMAND"; }
                        if (item.Contains("CONSTRUCTION")) { _type = "CONSTRUCTION"; }

                        switch (_type)
                        {
                            case "SCOUT": _scoutCount += 1; _scoutText = item; break;
                            case "FRIGATE": _frigateCount += 1; _frigateText = item; break;
                            case "SCIENCE": _scienceCount += 1; _scienceText = item; break;
                            case "MEDICAL": _medicalCount += 1; _medicalText = item; break;
                            case "COLONY": _colonyCount += 1; _colonyText = item; break;
                            case "TRANSPORT": _transportCount += 1; _transportText = item; break;
                            case "DIPLO": _diploCount += 1; _diploText = item; break;
                            case "SPY": _spyCount += 1; _spyText = item; break;
                            case "DESTROYER": _destroyerCount += 1; _destroyerText = item; break;
                            case "CRUISER": _cruiserCount += 1; _cruiserText = item; break;
                            case "STRIKE_CRUISER": _strikeCruiserCount += 1; _strikeCruiserText = item; break;
                            case "COMMAND": _commandCount += 1; _commandText = item; break;
                            case "CONSTRUCTION": _constructionCount += 1; _constructionText = item; break;
                            default:
                                break;
                        }
                    }

                    _cruiserCount -= _strikeCruiserCount; if (_cruiserCount < 0)
                    {
                        _cruiserCount = 0;
                    }

                    _colonyText = _colonyCount > 0 ? separator + _colonyCount + hyphen + _colonyText : " ;";
                    _constructionText = _constructionCount > 0 ? separator + _constructionCount + hyphen + _constructionText : " ;";
                    _scoutText = _scoutCount > 0 ? separator + _scoutCount + hyphen + _scoutText : " ;";
                    _frigateText = _frigateCount > 0 ? separator + _frigateCount + hyphen + _frigateText : " ;";
                    _destroyerText = _destroyerCount > 0 ? separator + _destroyerCount + hyphen + _destroyerText : " ;";
                    _cruiserText = _cruiserCount > 0 ? separator + _cruiserCount + hyphen + _cruiserText : " ;";
                    _strikeCruiserText = _strikeCruiserCount > 0 ? separator + _strikeCruiserCount + hyphen + _strikeCruiserText : " ;";
                    _commandText = _commandCount > 0 ? separator + _commandCount + hyphen + _commandText : " ;";
                    _scienceText = _scienceCount > 0 ? separator + _scienceCount + hyphen + _scienceText : " ;";
                    _medicalText = _medicalCount > 0 ? separator + _medicalCount + hyphen + _medicalText : " ;";
                    _transportText = _transportCount > 0 ? separator + _transportCount + hyphen + _transportText : " ;";
                    _diploText = _diploCount > 0 ? separator + _diploCount + hyphen + _diploText : " ;";
                    _spyText = _spyCount > 0 ? separator + _spyCount + hyphen + _spyText : " ;";


                    _text
                        += /*separator + _colonyCount + hyphen + */_colonyText
                        + /*separator + _constructionCount + hyphen + */_constructionText
                        + /*separator + _scoutCount + hyphen + */_scoutText
                        + /*separator + _frigateCount + hyphen + */_frigateText
                        + /*separator + _destroyerCount + hyphen + */_destroyerText
                        + /*separator + _cruiserCount + hyphen + */_cruiserText
                        + /*separator + _strikeCruiserCount + hyphen + */ _strikeCruiserText
                        + /*separator + _commandCount + hyphen + */_commandText

                        + /*separator + _scienceCount + hyphen + */_scienceText
                        /*+ separator + _medicalCount + hyphen*/ + _medicalText
                        /*+ separator + _transportCount + hyphen*/ + _transportText
                        /*+ separator + _diploCount + hyphen*/ + _diploText
                        /*+ separator + _spyCount + hyphen*/ + _spyText
                        ;

                    _startingShipsSummary = "StartShips" + _text;

                    _text = " ";

                    //string _text;
                    foreach (string item in db[civId].StartingBuildings)
                    {
                        _text += item + " ;";
                    }
                    _startingBuildingsSummary = "StartBuildungs;" + _text;


                    _startingStation = " ;";
                    foreach (string item in db[civId].StartingOutposts)
                    {
                        _startingStation = item + " ;";
                    }


                    _startingShipyard = " ;";
                    foreach (string item in db[civId].StartingShipyards)
                    {
                        _startingShipyard = item + " ;";
                    }


                    _startingOrbitalBatteries = " ;";
                    if (db[civId].StartingOrbitalBatteries != null && db[civId].StartingOrbitalBatteries.Count > 0)  // Not all Minor races PF
                    {
                        _startingOrbitalBatteries = db[civId].StartingOrbitalBatteries.Count + hyphen + db[civId].StartingOrbitalBatteries[0] + " ;";
                    }

                    //_startingOrbitalBatteries = _startingOrbitalBatteries.Replace("-1", "0");


                    _startingFoodPF = ";;";
                    if (db[civId].FoodPF != null /*&& db[civId].FoodPF.Count > 0*/)  // Not all Minor races PF
                    {
                        _startingFoodPF = db[civId].FoodPF.Active + " ;" + db[civId].FoodPF.Count + hyphen + db[civId].FoodPF.DesignType + " ;";
                    }

                    _startingFoodPF = _startingFoodPF.Replace("-1", "0");


                    _startingIndustryPF = ";;";
                    if (db[civId].IndustryPF != null /*&& db[civId].IndustryPF.Count > 0*/)  // Not all Minor races PF
                    {
                        _startingIndustryPF = db[civId].IndustryPF.Active + " ;" + db[civId].IndustryPF.Count + hyphen + db[civId].IndustryPF.DesignType + " ;";
                    }

                    _startingIndustryPF = _startingIndustryPF.Replace("-1", "0");

                    _startingEnergyPF = ";;";
                    if (db[civId].EnergyPF != null /*&& db[civId].EnergyPF.Count > 0*/)  // Not all Minor races PF
                    {
                        _startingEnergyPF = db[civId].EnergyPF.Active + " ;" + db[civId].EnergyPF.Count + hyphen + db[civId].EnergyPF.DesignType + " ;";
                    }

                    _startingEnergyPF = _startingEnergyPF.Replace("-1", "0");

                    _startingResearchPF = ";;";
                    if (db[civId].ResearchPF != null /*&& db[civId].ResearchPF.Count > 0*/)  // Not all Minor races PF
                    {
                        _startingResearchPF = db[civId].ResearchPF.Active + " ;" + db[civId].ResearchPF.Count + hyphen + db[civId].ResearchPF.DesignType + " ;";
                    }

                    _startingResearchPF = _startingResearchPF.Replace("-1", "0");

                    _startingIntelligencePF = ";;";
                    if (db[civId].IntelligencePF != null /*&& db[civId].IntelligencePF.Count > 0*/)  // Not all Minor races PF
                    {
                        _startingIntelligencePF = db[civId].IntelligencePF.Active + " ;" + db[civId].IntelligencePF.Count + hyphen + db[civId].IntelligencePF.DesignType + " ;";
                    }

                    _startingIntelligencePF = _startingIntelligencePF.Replace("-1", "0");

                    strLine =
                        civId + separator +
                        GameContext.Current.Options.StartingTechLevel.ToString() + separator +

                        _startingFoodPF + separator +
                        _startingIndustryPF + separator +
                        _startingEnergyPF + separator +
                        _startingResearchPF + separator +
                        _startingIntelligencePF + separator +

                        _startingShipsSummary + separator +

                        db[civId].Name + separator +
                        db[civId].PopulationRatio + separator +
                        db[civId].Credits + separator +
                        db[civId].Deuterium + separator +
                        db[civId].Dilithium + separator +
                        db[civId].Duranium + separator +
                        db[civId].Food + separator +
                        db[civId].Morale + separator +

                        _startingOrbitalBatteries + separator +
                        _startingShipyard + separator +
                        _startingStation + separator +
                        _startingBuildingsSummary + separator +
                        separator;

                    strLine = strLine.Replace("-1", " ");
                    strLine = strLine.Replace("-", " ");

                    //Console.WriteLine(strLine);
                    streamWriter.WriteLine(strLine);

                    strLine = "";
                    _text = " ";
                    _startingOrbitalBatteries = " ";
                    _startingShipyard = " ";
                    _startingStation = " ";
                    _startingBuildingsSummary = " ";

                    //strLine2 =
                    //    civId + separator +       //  following entries not working yet
                    //    db[civId].Name + separator
                    //    ;

                    //streamWriter2.WriteLine(strLine2);

                    //strLine2 = "";


                }  // end of foreach
            WriterCloseHomeSystemsXML:;
                streamWriter.Close();
                //streamWriter2.Close();
            }
            catch (Exception e)
            {
                _text = "Cannot write ... " + file + e;
                GameLog.Core.GameData.ErrorFormat(_text);
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
            {
                throw new ArgumentNullException("fileName");
            }

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

                _ = xmlDoc.AppendChild(rootElement);

                foreach (string civId in Keys)
                {
                    XmlElement homeSystemElement = xmlDoc.CreateElement("HomeSystem");
                    homeSystemElement.SetAttribute("Civilization", civId);
                    this[civId].AppendXml(homeSystemElement);
                    _ = rootElement.AppendChild(homeSystemElement);
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
            get => _designType;
            set
            {
                _designType = value;
                OnPropertyChanged("DesignType");
            }
        }

        public float Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        public float Active
        {
            get => _active;
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
        private float _duranium = -1.0f;
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
            return (bonus & _bonuses) == bonus;
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
            get => _bonuses;
            set
            {
                _bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("HasBonuses");
            }
        }

        [DependsOn("Bonuses")]
        public bool HasBonuses => _bonuses != 0;

        public string Inhabitants
        {
            get => _inhabitants;
            set
            {
                _inhabitants = value;
                OnPropertyChanged("Inhabitants");
                OnPropertyChanged("IsInhabitantsDefined");
            }
        }

        public BindingList<PlanetDescriptor> Planets
        {
            get => _planets;
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
            get => _systemName ?? string.Empty;
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
            get => _starType;
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
        public bool IsInhabitantsDefined => _inhabitants != null;

        [DependsOn("StarType")]
        public bool IsStarTypeDefined => _starType != null;

        [DependsOn("Name")]
        public bool IsNameDefined => _systemName != null;

        public List<string> StartingShips => _startingShips;

        public List<string> StartingShipyards => _startingShipyards;

        public List<string> StartingBuildings => _startingBuildings;

        public List<string> StartingOutposts => _startingOutposts;

        public List<string> StartingOrbitalBatteries => _startingOrbitalBatteries;

        public float PopulationRatio => _populationRatio;

        public float Credits => _credits;

        public float Deuterium => _deuterium;

        public float Dilithium => _dilithium;

        public float Duranium => _duranium;

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
            {
                Name = xmlNode.GetAttribute("Name").Trim();
            }

            if (xmlNode.HasAttribute("StarType") && Enum.IsDefined(typeof(StarType), xmlNode.GetAttribute("StarType").Trim()))
            {
                StarType = (StarType)Enum.Parse(typeof(StarType), xmlNode.GetAttribute("StarType").Trim());
            }

            if (xmlNode["Inhabitants"] != null)
            {
                Inhabitants = xmlNode["Inhabitants"].InnerText.Trim().ToUpperInvariant();
            }

            XmlNodeList bonusElements = xmlNode.GetElementsByTagName("Bonus");
            if (bonusElements.Count > 0)
            {
                _bonuses = SystemBonus.NoBonus;

                foreach (XmlElement bonusElement in bonusElements)
                {
                    if (Enum.IsDefined(typeof(SystemBonus), bonusElement.GetAttribute("Type").Trim()))
                    {
                        AddBonus((SystemBonus)Enum.Parse(typeof(SystemBonus), bonusElement.GetAttribute("Type").Trim()));
                    }
                }
            }

            if (xmlNode["Planets"] != null)
            {
                foreach (XmlElement planetElement in xmlNode["Planets"].GetElementsByTagName("Planet"))
                {
                    Planets.Add(new PlanetDescriptor(planetElement));
                }
            }

            XmlNodeList startingLevelTech = xmlNode.GetElementsByTagName("TechLevel");
            if (startingLevelTech.Count > 0)
            {
                string curStartingLevel = GameContext.Current.Options.StartingTechLevel.ToString().ToUpperInvariant();
                foreach (XmlElement techLevel in startingLevelTech)
                {
                    if (techLevel.HasAttribute("Name"))
                    {
                        string techLevelName = techLevel.GetAttribute("Name").Trim().ToUpperInvariant();
                        if (techLevelName.Equals(curStartingLevel))
                        {
                            // population ratio
                            if (techLevel.HasAttribute("PopulationRatio"))
                            {
                                string popRatio = techLevel.GetAttribute("PopulationRatio").Trim().ToUpperInvariant();
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
                                string credits = techLevel.GetAttribute("Credits").Trim().ToUpperInvariant();
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
                                string res = techLevel.GetAttribute("Deuterium").Trim().ToUpperInvariant();
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
                                string res = techLevel.GetAttribute("Dilithium").Trim().ToUpperInvariant();
                                try
                                {
                                    _dilithium = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            if (techLevel.HasAttribute("Duranium"))
                            {
                                string res = techLevel.GetAttribute("Duranium").Trim().ToUpperInvariant();
                                try
                                {
                                    _duranium = float.Parse(res, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception e)
                                {
                                    GameLog.Core.GameData.Error(e);
                                }
                            }

                            if (techLevel.HasAttribute("Food"))
                            {
                                string res = techLevel.GetAttribute("Food").Trim().ToUpperInvariant();
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
                                string res = techLevel.GetAttribute("Morale").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    string val = pf.GetAttribute("Active").Trim().ToUpperInvariant();
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
                            XmlNodeList startingShips = techLevel.GetElementsByTagName("Ship");
                            if (startingShips.Count > 0)
                            {
                                foreach (XmlElement ship in startingShips)
                                {
                                    int shipCount = 1;
                                    if (ship.HasAttribute("Count"))
                                    {
                                        string val = ship.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    {
                                        _startingShips.Add(shipDesign);
                                    }
                                }
                            }

                            // shipyards to be spawned
                            XmlNodeList startingShipyards = techLevel.GetElementsByTagName("Shipyard");
                            if (startingShipyards.Count > 0)
                            {
                                foreach (XmlElement shipyard in startingShipyards)
                                {
                                    _startingShipyards.Add(shipyard.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // buildings to be spawned
                            XmlNodeList startingBuildings = techLevel.GetElementsByTagName("Building");
                            if (startingBuildings.Count > 0)
                            {
                                foreach (XmlElement building in startingBuildings)
                                {
                                    _startingBuildings.Add(building.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // outposts to be spawned
                            XmlNodeList startingOutposts = techLevel.GetElementsByTagName("SpaceStation");
                            if (startingOutposts.Count > 0)
                            {
                                foreach (XmlElement outpost in startingOutposts)
                                {
                                    _startingOutposts.Add(outpost.InnerText.Trim().ToUpperInvariant());
                                }
                            }

                            // OBs to be spawned
                            XmlNodeList startingOBs = techLevel.GetElementsByTagName("OrbitalBattery");
                            if (startingOBs.Count > 0)
                            {
                                foreach (XmlElement OB in startingOBs)
                                {
                                    int OBCount = 1;
                                    if (OB.HasAttribute("Count"))
                                    {
                                        string val = OB.GetAttribute("Count").Trim().ToUpperInvariant();
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
                                    {
                                        _startingOrbitalBatteries.Add(OBDesign);
                                    }
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
            XmlElement systemElement = baseElement.OwnerDocument.CreateElement("StarSystem");

            if (IsNameDefined)
            {
                systemElement.SetAttribute("Name", Name);
            }

            if (IsStarTypeDefined)
            {
                systemElement.SetAttribute("StarType", StarType.Value.ToString());
            }

            if (IsInhabitantsDefined)
            {
                XmlElement inhabitantsElement = systemElement.OwnerDocument.CreateElement("Inhabitants");
                inhabitantsElement.InnerText = Inhabitants;
                _ = systemElement.AppendChild(inhabitantsElement);
            }

            XmlElement bonusesElement = systemElement.OwnerDocument.CreateElement("Bonuses");
            if (Bonuses == SystemBonus.NoBonus)
            {
                XmlElement bonusElement = systemElement.OwnerDocument.CreateElement("Bonus");
                bonusElement.SetAttribute("Type", SystemBonus.NoBonus.ToString());
                _ = bonusesElement.AppendChild(bonusElement);
            }
            else
            {
                foreach (SystemBonus bonus in EnumUtilities.GetValues<SystemBonus>())
                {
                    if ((bonus != SystemBonus.NoBonus) && HasBonus(bonus))
                    {
                        XmlElement bonusElement = systemElement.OwnerDocument.CreateElement("Bonus");
                        bonusElement.SetAttribute("Type", bonus.ToString());
                        _ = bonusesElement.AppendChild(bonusElement);
                    }
                }
            }
            _ = systemElement.AppendChild(bonusesElement);

            if (Planets.Count > 0)
            {
                XmlElement planetsElement = systemElement.OwnerDocument.CreateElement("Planets");
                foreach (PlanetDescriptor planet in Planets)
                {
                    XmlElement planetElement = systemElement.OwnerDocument.CreateElement("Planet");
                    planet.AppendXml(planetElement);
                    _ = planetsElement.AppendChild(planetElement);
                }
                _ = systemElement.AppendChild(planetsElement);
            }

            _ = baseElement.AppendChild(systemElement);
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
            return (bonus & _bonuses) == bonus;
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
            get => _bonuses;
            set
            {
                _bonuses = value;
                OnPropertyChanged("Bonuses");
                OnPropertyChanged("HasBonuses");
            }
        }

        [DependsOn("Bonuses")]
        public bool HasBonuses => _bonuses != 0;

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Name")]
        public bool IsNameDefined => _planetName != null;

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Size")]
        public bool IsSizeDefined => IsSinglePlanet && (_planetSize != null);

        [DependsOn("IsSinglePlanet")]
        [DependsOn("Type")]
        public bool IsTypeDefined => IsSinglePlanet && (_planetType != null);

        [DependsOn("IsSinglePlanet")]
        public string Name
        {
            get => IsSinglePlanet ? (_planetName ?? string.Empty) : string.Empty;
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
        public bool IsSinglePlanet => _maxNumberOfPlanets == 1;

        public int MinNumberOfPlanets
        {
            get => _minNumberOfPlanets;
            set
            {
                if (value != _minNumberOfPlanets)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                    }

                    _minNumberOfPlanets = value;
                    OnPropertyChanged("MinNumberOfPlanets");
                }
            }
        }

        public int MaxNumberOfPlanets
        {
            get => _maxNumberOfPlanets;
            set
            {
                if (value != _maxNumberOfPlanets)
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("value", "value must be greater than zero");
                    }

                    if (value > StarSystem.MaxPlanetsPerSystem)
                    {
                        throw new ArgumentOutOfRangeException("value", "value must be less than MaxPlanetsPerSystem");
                    }

                    _maxNumberOfPlanets = value;
                    OnPropertyChanged("MaxNumberOfPlanets");
                    OnIsSinglePlanetChanged();
                }
            }
        }

        [DependsOn("IsSinglePlanet")]
        public PlanetType? Type
        {
            get => _planetType;
            set
            {
                if (value != _planetType)
                {
                    _planetType = value;
                    OnPropertyChanged("Type");
                    OnPropertyChanged("IsTypeDefined");
                    if (_planetType == PlanetType.Asteroids)
                    {
                        Size = PlanetSize.Asteroids;
                    }
                    else if (_planetType == PlanetType.GasGiant)
                    {
                        Size = PlanetSize.GasGiant;
                    }
                }
            }
        }

        [DependsOn("IsSinglePlanet")]
        public PlanetSize? Size
        {
            get => _planetSize;
            set
            {
                if (value != _planetSize)
                {
                    _planetSize = value;
                    OnPropertyChanged("Size");
                    OnPropertyChanged("IsSizeDefined");
                    if (_planetSize == PlanetSize.Asteroids)
                    {
                        Type = PlanetType.Asteroids;
                    }
                    else if (_planetSize == PlanetSize.GasGiant)
                    {
                        Type = PlanetType.GasGiant;
                    }
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
            {
                Name = xmlNode.GetAttribute("Name").Trim();
            }

            if (xmlNode.HasAttribute("Size") && Enum.IsDefined(typeof(PlanetSize), xmlNode.GetAttribute("Size").Trim()))
            {
                Size = (PlanetSize)Enum.Parse(typeof(PlanetSize), xmlNode.GetAttribute("Size").Trim());
            }

            if (xmlNode.HasAttribute("Type") && Enum.IsDefined(typeof(PlanetType), xmlNode.GetAttribute("Type").Trim()))
            {
                Type = (PlanetType)Enum.Parse(typeof(PlanetType), xmlNode.GetAttribute("Type").Trim());
            }

            if (xmlNode.HasAttribute("MaxNumberOfPlanets") && int.TryParse(xmlNode.GetAttribute("MaxNumberOfPlanets").Trim(), out int tempInteger))
            {
                MaxNumberOfPlanets = tempInteger;
            }

            if (xmlNode.HasAttribute("MinNumberOfPlanets") && int.TryParse(xmlNode.GetAttribute("MinNumberOfPlanets").Trim(), out tempInteger))
            {
                MinNumberOfPlanets = tempInteger;
            }

            XmlNodeList bonusElements = xmlNode.GetElementsByTagName("Bonus");
            if (bonusElements.Count > 0)
            {
                _bonuses = PlanetBonus.NoBonus;

                foreach (XmlElement bonusElement in bonusElements)
                {
                    if (Enum.IsDefined(typeof(PlanetBonus), bonusElement.GetAttribute("Type").Trim()))
                    {
                        AddBonus((PlanetBonus)Enum.Parse(typeof(PlanetBonus), bonusElement.GetAttribute("Type").Trim()));
                    }
                }
            }

            if (!IsSizeDefined && IsTypeDefined)
            {
                if (Type.Value == PlanetType.Asteroids)
                {
                    Size = PlanetSize.Asteroids;
                }
                else if (Type.Value == PlanetType.GasGiant)
                {
                    Size = PlanetSize.GasGiant;
                }
            }
        }

        public void AppendXml(XmlElement baseElement)
        {
            if (IsSinglePlanet)
            {
                if (IsNameDefined)
                {
                    baseElement.SetAttribute("Name", Name);
                }

                if (IsTypeDefined)
                {
                    baseElement.SetAttribute("Type", Type.ToString());
                }

                if (IsSizeDefined)
                {
                    baseElement.SetAttribute("Size", Size.ToString());
                }

                if (HasBonuses)
                {
                    foreach (PlanetBonus bonus in EnumUtilities.GetValues<PlanetBonus>())
                    {
                        if ((bonus != PlanetBonus.Random) && HasBonus(bonus))
                        {
                            XmlElement bonusElement = baseElement.OwnerDocument.CreateElement("Bonus");
                            bonusElement.SetAttribute("Type", bonus.ToString());
                            _ = baseElement.AppendChild(bonusElement);
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