// File:Civilization.cs
// Copyright (c) 2007 Mike Strobel
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace Supremacy.Entities
{
    /// <summary>
    /// Defines the different civilization classifications used in the game.
    /// </summary>
    public enum CivilizationType : byte
    {
        MinorPower = 0,
        DevelopingPower,
        ExpandingPower,
        Empire,
        NotInGameRace
    }

    /// <summary>
    /// Defines the different civilization tech curves used in the game.
    /// </summary>
    public enum TechCurve : byte
    {
        TecCurve1 = 0,
        TecCurve2,
        TecCurve3,
        TecCurve4,
        TecCurve5,
        TecCurve6
    }

    public enum CivTraits : ushort
    {
        Warlike = 0x0001, // only used in one line in DiplomacyHelper.cs at this time
        Peaceful = 0x0002,
        Superiority = 0x0004,
        Submissive = 0x0008,
        Materialistic = 0x0010,
        Spiritual = 0x0020,
        Kindness = 0x0040,
        Hostile = 0x0080,
        Honourable = 0x0100,
        Subversive = 0x0200
        //Spiritual = 0x0400
    }

    /// <summary>
    /// Represents a civilization in the game (an empire or minor race).
    /// </summary>
    [Serializable]
    public class Civilization : ICivIdentity
    {
        #region Constants
        /// <summary>
        /// The default color of a civilization (usually only redefined for empires).
        /// </summary>
        public const string DefaultColor = "White";
        protected const string MissingImageUri = "vfs:///Resources/Images/__image_missing.png";
        protected const string DefaultInsigniaUri = "vfs:///Resources/Images/Insignias/__default.png";

        /// <summary>
        /// Represents an invalid value for the <see cref="CivID"/> property.
        /// </summary>
        public static readonly int InvalidID = -1;
        #endregion

        #region Fields
        private int _civId = InvalidID;
        private CivilizationType _civType;
        private TechCurve _techCurve;
        private readonly string _traits;
        private string _color;
        private string _diplomacyReport;
        private Quadrant _homeQuadrant;
        private string _homeSystemName;
        private string _key;
        private string _longName;
        private string _longNameArtCap;
        private string _longNameArtDir;
        private string _longNameFromThe;
        private string _longNameOfTheGreater;
        private string _raceId;
        private string _shortName;
        private string _shortNameArtCap;
        private string _shortNameArtDir;
        private string _shortNameFromThe;
        private string _shortNameToThe;
        private readonly string _shipPrefix;
        private float _industryToCreditsConversionRatio = 0.0f;
        private readonly int _baseMoraleLevel = 100;
        private readonly int _moraleDriftRate = 1;
        private List<Civilization> _spiedCivList;
        private Civilization _targetCivilization;
        private bool _inTransit;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Civilization"/> class.
        /// </summary>
        public Civilization() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Civilization"/> class.
        /// </summary>
        /// <param name="race">The primary race.</param>
        public Civilization(Race race)
        {
            if (race == null)
                throw new ArgumentNullException("race");
            _key = race.Key;
            _raceId = race.Key;
            _civType = CivilizationType.MinorPower;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Civilization"/> class.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <param name="race">The primary race.</param>
        public Civilization(string key, Race race)
        {
            if (race == null)
                throw new ArgumentNullException("race");
            _key = key;
            _raceId = race.Key;
            _civType = CivilizationType.MinorPower;
        }

        public List<Civilization> SpiedCivList
        {
            get { return _spiedCivList; }
            set
            {
                // code here to .Add civs to list as spy ships install spy network
                _spiedCivList = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Civilization"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public Civilization(XElement element)
        {
            XNamespace ns = element.Document.Root.Name.Namespace;

            _key = (string)element.Attribute("Key");
            _raceId = (string)element.Element(ns + "Race");
            _shortName = (string)element.Element(ns + "ShortName");
            _shortNameArtCap = (string)element.Element(ns + "ShortNameArtCap");
            _shortNameArtDir = (string)element.Element(ns + "ShortNameArtDir");
            _shortNameFromThe = (string)element.Element(ns + "ShortNameFromThe");
            _shortNameToThe = (string)element.Element(ns + "ShortNameToThe");
            _longName = (string)element.Element(ns + "LongName");
            _longNameArtCap = (string)element.Element(ns + "LongNameArtCap");
            _longNameArtDir = (string)element.Element(ns + "LongNameArtDir");
            _longNameFromThe = (string)element.Element(ns + "LongNameFromThe");
            _longNameOfTheGreater = (string)element.Element(ns + "LongNameOfTheGreater");
            _diplomacyReport = (string)element.Element(ns + "DiplomacyReport");
            _homeSystemName = (string)element.Element(ns + "HomeSystemName");
            _color = (string)element.Element(ns + "Color");
            _shipPrefix = (string)element.Element(ns + "ShipPrefix");
            if (element.Element(ns + "BaseMoraleLevel") != null)
            {
                _baseMoraleLevel = (int)element.Element(ns + "BaseMoraleLevel");
            }
            if (element.Element(ns + "MoraleDriftRate") != null)
            {
                _moraleDriftRate = (int)element.Element(ns + "MoraleDriftRate");
            }
            EnumHelper.TryParse((string)element.Element(ns + "HomeQuadrant"), out _homeQuadrant);
            EnumHelper.TryParse((string)element.Element(ns + "CivilizationType"), out _civType);
            EnumHelper.TryParse((string)element.Element(ns + "TechCurve"), out _techCurve);
            string indConvRation = (string)element.Element(ns + "IndustryToCreditsConversionRatio");
            if (!string.IsNullOrEmpty(indConvRation))
            {
                ushort convRatio = Number.ParseUInt16(indConvRation);
                if (convRatio > 0)
                    _industryToCreditsConversionRatio = (float)convRatio / 100.0f;
            }

            _traits = (string)element.Element(ns + "Traits");
            _traits = _traits.Trim();
            _spiedCivList = SpiedCivList;
            _targetCivilization = TargetCivilization;

            //_intelOrdersIncomingToHost = IntelOrdersIncomingToHost;

            // When starting a game, options is null
            //TODO: This should be in with the code to start the game
            if (GameContext.Current.Options != null)
            {
                if ((_key == "FEDERATION") && (GameContext.Current.Options.FederationPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "ROMULANS") && (GameContext.Current.Options.RomulanPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "KLINGONS") && (GameContext.Current.Options.KlingonPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "CARDASSIANS") && (GameContext.Current.Options.CardassianPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "DOMINION") && (GameContext.Current.Options.DominionPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "BORG") && (GameContext.Current.Options.BorgPlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }

                if ((_key == "TERRANEMPIRE") && (GameContext.Current.Options.TerranEmpirePlayable == EmpirePlayable.No))
                {
                    _civType = CivilizationType.ExpandingPower;
                    GameLog.Client.GameData.DebugFormat("Civilization {0} is set to ExpandingPower", Name);
                }
            }

            if (string.IsNullOrEmpty(_raceId))
                _raceId = Race.InvalidRaceKey;

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the civilization ID.
        /// </summary>
        /// <value>The civilization ID.</value>
        public int CivID
        {
            get { return _civId; }
            set { _civId = value; }
        }

        /// <summary>
        /// Gets or sets the unique key of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The unique key.</value>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Gets or sets the short name of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The short name.</value>
        public string ShortName
        {
            get { return _shortName; }
            set { _shortName = value; }
        }


        /// <summary>
        /// Gets or sets the short name ArtCap of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The short name ArtCap.</value>
        public string ShortNameArtCap
        {
            get { return _shortNameArtCap; }
            set { _shortNameArtCap = value; }
        }

        /// <summary>
        /// Gets or sets the short name ArtDir of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The short name ArtDir.</value>
        public string ShortNameArtDir
        {
            get { return _shortNameArtDir; }
            set { _shortNameArtDir = value; }
        }

        /// <summary>
        /// Gets or sets the short name FromThe of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The short name FromThe.</value>
        public string ShortNameFromThe
        {
            get { return _shortNameFromThe; }
            set { _shortNameFromThe = value; }
        }

        /// <summary>
        /// Gets or sets the short name ToThe of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The short name ToThe.</value>
        public string ShortNameToThe
        {
            get { return _shortNameToThe; }
            set { _shortNameToThe = value; }
        }

        /// <summary>
        /// Gets or sets the long name of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The long name.</value>
        public string LongName
        {
            get { return _longName; }
            set { _longName = value; }
        }

        /// <summary>
        /// Gets or sets the long name ArtCap of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The long name ArtCap.</value>
        public string LongNameArtCap
        {
            get { return _longNameArtCap; }
            set { _longNameArtCap = value; }
        }

        /// <summary>
        /// Gets or sets the long name ArtDir of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The long name ArtDir.</value>
        public string LongNameArtDir
        {
            get { return _longNameArtDir; }
            set { _longNameArtDir = value; }
        }

        /// <summary>
        /// Gets or sets the long name FromThe of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The long name FromThe.</value>
        public string LongNameFromThe
        {
            get { return _longNameFromThe; }
            set { _longNameFromThe = value; }
        }

        /// <summary>
        /// Gets or sets the long name OfTheGreater of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The long name OfTheGreater.</value>
        public string LongNameOfTheGreater
        {
            get { return _longNameOfTheGreater; }
            set { _longNameOfTheGreater = value; }
        }

        /// <summary>
        /// Gets or sets the diplomacy report text for this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The diplomacy report text.</value>
        public string DiplomacyReport
        {
            get { return _diplomacyReport; }
            set { _diplomacyReport = value; }
        }

        /// <summary>
        /// Gets or sets the Industry to credits conversion ratio for this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The conversion ratio.</value>
        public float IndustryToCreditsConversionRatio
        {
            get { return _industryToCreditsConversionRatio; }
            set { _industryToCreditsConversionRatio = value; }
        }

        /// <summary>
        /// Gets the path of the image file for this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The image file path.</value>
        public string Image
        {
            get
            {
                string[] searchPaths = new[]
                                  {
                                      "vfs:///Resources/Images/Civilizations/{0}.png",
                                      "vfs:///Resources/Images/Civilizations/{0}.jpg"
                                  };

                foreach (string searchPath in searchPaths)
                {
                    string imagePath = ResourceManager.GetResourcePath(string.Format(searchPath, Key));
                    if (File.Exists(imagePath))
                        return ResourceManager.GetResourceUri(imagePath).ToString();
                }

                return Race.ImagePath;
            }
        }

        public string InsigniaPath
        {
            get
            {
                string[] searchPaths = new[]
                                  {
                                      "vfs:///Resources/Images/Insignias/{0}.png",
                                      "vfs:///Resources/Images/Insignias/{0}.jpg"
                                  };
                foreach (string searchPath in searchPaths)
                {
                    string imageUri = string.Format(searchPath, Key);
                    string imagePath = ResourceManager.GetResourcePath(imageUri);

                    if (File.Exists(imagePath))
                        return imageUri;
                }

                return DefaultInsigniaUri;
            }
        }

        /// <summary>
        /// Gets or sets the territory color of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The territory color.</value>
        public string Color
        {
            get { return _color ?? DefaultColor; }
            set { _color = value; }
        }

        /// <summary>
        /// Gets or sets the classification of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The classification.</value>
        public CivilizationType CivilizationType
        {
            get { return _civType; }
            set { _civType = value; }
        }

        /// <summary>
        /// Gets or sets the tech curve of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The tech curve.</value>
        public TechCurve TechCurve
        {
            get { return _techCurve; }
            set { _techCurve = value; }
        }

        /// <summary>
        /// Gets or sets the traits of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The tech curve.</value>
        public string Traits => _traits;

        /// <summary>
        /// Gets the <see cref="Civilization"/> this <see cref="Civilization"/>
        /// is at in Total War with
        /// </summary>
        /// <value>The tech curve.</value>
        public Civilization TargetCivilization
        {
            get { return _targetCivilization; }
            set { if (_targetCivilization == null) { _targetCivilization = value; } }
        }

        /// <summary>
        /// Gets the <see cref="bool"/> this civ AI attacking fleet on the move />        
        /// </summary>
        /// <value> bool </value>
        public bool InTransit
        {
            get { return _inTransit; }
            set { _inTransit = value; } 
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Civilization"/> is an empire.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Civilization"/> is an empire; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpire => (_civType == CivilizationType.Empire);

        /// <summary>
        /// Gets a value indicating whether this <see cref="Civilization"/> can expand.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can expand; otherwise, <c>false</c>.
        /// </value>
        public bool CanExpand => (_civType != CivilizationType.MinorPower);

        /// <summary>
        /// Gets or sets the primary race of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The primary race.</value>
        public Race Race
        { 
        get
            {
               try
                {
                    return (_raceId == Race.InvalidRaceKey) ? null : GameContext.Current.Races[_raceId];
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.ErrorFormat("##### Problem with Race = {0} {1} {2}", Race, e.Message, e.StackTrace);
                    
                }
                return (_raceId == Race.InvalidRaceKey) ? null : GameContext.Current.Races[_raceId];
            }
            set
            {
                _raceId = (value != null) ? value.Key : Race.InvalidRaceKey;
            }
        }

        /// <summary>
        /// Gets the race ID of the primary race of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The primary race ID.</value>
        public string RaceID
        {
            get { return _raceId; }
            set { _raceId = value ?? Race.InvalidRaceKey; }
        }

        /// <summary>
        /// Gets or sets the home quadrant of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The home quadrant.</value>
        public Quadrant HomeQuadrant
        {
            get { return _homeQuadrant; }
            set { _homeQuadrant = value; }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="Civilization"/>'s home system.
        /// </summary>
        /// <value>The name of the home system.</value>
        public string HomeSystemName
        {
            get { return _homeSystemName; }
            set { _homeSystemName = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Civilization"/> is being controlled
        /// by a human player in the current game context.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Civilization"/> is human-controlled; otherwise, <c>false</c>.
        /// </value>
        public bool IsHuman
        {
            get
            {
                if (!IsEmpire)
                    return false;
                return PlayerContext.Current == null || PlayerContext.Current.IsHumanPlayer(this);
            }
        }

        /// <summary>
        /// Returns this civilizations ship name prefix
        /// </summary>
        public string ShipPrefix => _shipPrefix;

        /// <summary>
        /// Return the base morale level for this civilization
        /// </summary>
        public int BaseMoraleLevel => _baseMoraleLevel;

        /// <summary>
        /// The morale drift rate for the civilization
        /// </summary>
        public int MoraleDriftRate => _moraleDriftRate;

        /// <summary>
        /// Gets the display name of this <see cref="Civilization"/>.
        /// </summary>
        /// <value>The display name.</value>
        public string Name => ShortName;
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="Civilization"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="Civilization"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="Civilization"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as Civilization);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Civilization"/> is equal to the current <see cref="Civilization"/>.
        /// </summary>
        /// <param name="civ">The <see cref="Civilization"/> to compare with the current <see cref="Civilization"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Civilization"/> is equal to the current <see cref="Civilization"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public virtual bool Equals(Civilization civ)
        {
            if (civ == null)
                return false;
            return (civ.Key == Key);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Civilization"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override int GetHashCode()
        {
            return (_key != null)
                    ? _key.GetHashCode()
                    : base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="Civilization"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="Civilization"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override string ToString()
        {
            return _shortName;
        }

        /// <summary>
        /// Writes the XML.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Civilization");
            writer.WriteStartAttribute("Key");
            writer.WriteValue(Key);
            writer.WriteEndAttribute();
            writer.WriteElementString("Race", RaceID);
            writer.WriteElementString("ShortName", ShortName);
            writer.WriteElementString("ShortNameArtCap", ShortNameArtCap);
            writer.WriteElementString("ShortNameArtDir", ShortNameArtDir);
            writer.WriteElementString("ShortNameFromThe", ShortNameFromThe);
            writer.WriteElementString("ShortNameToThe", ShortNameToThe);
            writer.WriteElementString("LongName", LongName);
            writer.WriteElementString("LongNameArtCap", LongNameArtCap);
            writer.WriteElementString("LongNameArtDir", LongNameArtDir);
            writer.WriteElementString("LongNameFromThe", LongNameFromThe);
            writer.WriteElementString("LongNameOfTheGreater", LongNameOfTheGreater);
            if (!String.IsNullOrEmpty(DiplomacyReport))
                writer.WriteElementString("DiplomacyReport", DiplomacyReport);
            if (!String.IsNullOrEmpty(HomeSystemName))
                writer.WriteElementString("HomeSystemName", HomeSystemName);
            writer.WriteElementString("Color", Color);
            writer.WriteElementString("HomeQuadrant", HomeQuadrant.ToString());
            writer.WriteElementString("CivilizationType", CivilizationType.ToString());
            writer.WriteElementString("TechCurve", TechCurve.ToString());
            writer.WriteElementString("Traits", _traits);
            writer.WriteElementString("IndustryToCreditsConversionRatio", ((int)(IndustryToCreditsConversionRatio * 100)).ToString());
            writer.WriteElementString("ShipPrefix", _shipPrefix);
            writer.WriteElementString("BaseMoraleLevel", _baseMoraleLevel.ToString());
            writer.WriteElementString("MoraleDriftRate", _moraleDriftRate.ToString());
            writer.WriteEndElement();
        }

        public XContainer AppendXml(XContainer parentElement)
        {
            XNamespace ns = parentElement.Document.Root.Name.Namespace;

            parentElement.Add(
                new XElement(
                    ns + "Civilization",
                    new XAttribute(
                        "Key",
                        Key),
                    new XElement(
                        ns + "Race",
                        Race),
                    
                    new XElement(
                        ns + "ShortName",
                        ShortName),

                    new XElement(
                        ns + "ShortNameArtCap",
                        ShortNameArtCap),

                    new XElement(
                        ns + "ShortNameArtDir",
                        ShortNameArtDir),

                    new XElement(
                        ns + "ShortNameFromThe",
                        ShortNameFromThe),

                    new XElement(
                        ns + "ShortNameToThe",
                        ShortNameToThe),

                    new XElement(
                        ns + "LongName",
                        LongName),

                    new XElement(
                        ns + "LongNameArtCap",
                        LongNameArtCap),

                    new XElement(
                        ns + "LongNameArtDir",
                        LongNameArtDir),

                    new XElement(
                        ns + "LongNameFromThe",
                        LongNameFromThe),

                    new XElement(
                        ns + "LongNameOfTheGreater",
                        LongNameOfTheGreater),
                    new XElement(
                        ns + "DiplomacyReport",
                        string.Join(
                            Environment.NewLine,
                            DiplomacyReport.Split(
                                new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))),
                    new XElement(
                        ns + "HomeSystemName",
                        HomeSystemName),
                    new XElement(
                        ns + "Color",
                        Color),
                    new XElement(
                        ns + "HomeQuadrant",
                        HomeQuadrant),
                    new XElement(
                        ns + "CivilizationType",
                        CivilizationType),
                    new XElement(
                        ns + "TechCurve",
                        TechCurve),
                    new XElement(
                        ns + "Traits",
                        TechCurve),
                    new XElement(
                        ns + "IndustryToCreditsConversionRatio",
                        (int)(IndustryToCreditsConversionRatio * 100))
                        ),
                    new XElement(
                        ns + "ShipPrefix",
                        ShipPrefix),
                    new XElement(
                        ns + "BaseMoraleLevel",
                        _baseMoraleLevel),
                    new XElement(
                        ns + "MoraleDriftRate",
                        _moraleDriftRate)
                    );

            return parentElement;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Civilization a, Civilization b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((a is null) || (b is null))
                return false;
            return (a.Key == b.Key);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Civilization a, Civilization b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if ((a is null) || (b is null))
                return true;
            return (a.Key != b.Key);
        }

        //public static implicit operator CombatTargetTwo(Civilization v)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion
    }
}
