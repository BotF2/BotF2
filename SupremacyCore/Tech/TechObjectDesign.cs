// TechObjectDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Effects;
using Supremacy.Encyclopedia;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Text;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Markup;
using System.Xml;

namespace Supremacy.Tech
{
    public enum BuildLimitScope : byte
    {
        None = 0,
        Civilization = 1,
        Galaxy = 2,
        System = 3
    }

    /// <summary>
    /// Represents a group of equivalent tech object designs, any one of which
    /// fulfills a prerequisite for the construction of another tech object design.
    /// </summary>
    public interface IPrerequisiteGroup : IList<TechObjectDesign> { }

    /// <summary>
    /// Represents a group of equivalent tech object designs, any one of which
    /// fulfills a prerequisite for the construction of another tech object design.
    /// </summary>
    [Serializable]
    public class PrerequisiteGroup : CollectionBase<TechObjectDesign>, IPrerequisiteGroup { }

    /// <summary>
    /// Represents a collection of prerequisites required for the construction of a
    /// given tech object design.  Each item in the collection is actually a group of
    /// "equivalent" prerequisites (a sub-collection), and any single item in a
    /// prerequisite group satisfies the prerequisite.
    /// </summary>
    public interface IPrerequisiteCollection : IList<PrerequisiteGroup> { }

    /// <summary>
    /// Represents a collection of prerequisites required for the construction of a
    /// given tech object design.  Each item in the collection is actually a group of
    /// "equivalent" prerequisites (a sub-collection), and any single item in a
    /// prerequisite group satisfies the prerequisite.
    /// </summary>
    [Serializable]
    public class PrerequisiteCollection : CollectionBase<PrerequisiteGroup>, IPrerequisiteCollection { }

    [Serializable]
    [TypeConverter(typeof(TechObjectTextGroupKeyConverter))]
    public class TechObjectTextGroupKey : IEquatable<TechObjectTextGroupKey>
    {
        private readonly string _designKey;

        public TechObjectTextGroupKey([NotNull] string designKey)
        {
            _designKey = designKey ?? throw new ArgumentNullException("designKey");
        }

        public string DesignKey => _designKey;

        public bool Equals(TechObjectTextGroupKey other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other._designKey, _designKey);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TechObjectTextGroupKey);
        }

        public override int GetHashCode()
        {
            return _designKey.GetHashCode();
        }
    }

    internal class TechObjectTextGroupKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is TechObjectTextGroupKey key && destinationType == typeof(MarkupExtension))
            {
                return new TechObjectTextGroupKey(key.DesignKey);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static class TechObjectStringKeys
    {
        public static readonly object Name = new TechObjectStringKey("Name");
        public static readonly object Description = new TechObjectStringKey("Description");
    }

    [Serializable]
    [TypeConverter(typeof(TechObjectStringKeyConverter))]
    public class TechObjectStringKey : IEquatable<TechObjectStringKey>
    {
        private readonly string _name;

        public TechObjectStringKey([NotNull] string name)
        {
            _name = name ?? throw new ArgumentNullException("name");

            GameLog.Client.GameData.DebugFormat("TechObjectStringKey-Name={0}", Name);
        }

        public string Name => _name;

        public bool Equals(TechObjectStringKey other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other._name, _name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TechObjectStringKey);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }

    internal class TechObjectStringKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is TechObjectStringKey techObjectKey &&
                destinationType == typeof(MarkupExtension))
            {
                if (context is IValueSerializerContext serializerContext)
                {
                    ValueSerializer typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
                    if (typeSerializer != null)
                    {
                        return new StaticExtension(
                            typeSerializer.ConvertToString(typeof(TechObjectStringKeys), serializerContext) +
                            "." +
                            techObjectKey.Name);
                    }
                }
                return new StaticExtension
                {
                    MemberType = typeof(TechObjectStringKeys),
                    Member = techObjectKey.Name
                };
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    /// <summary>
    /// The base class representing a tech object design.
    /// </summary>
    [Serializable]
    public abstract class TechObjectDesign : IBuildable, IEncyclopediaEntry, INotifyPropertyChanged
    {
        /// <summary>
        /// Represents an invalid value of the <see cref="DesignID"/> property.
        /// </summary>
        public const int InvalidDesignID = -1;

        public const string MissingImageUri = "vfs:///Resources/Images/__image_missing.png";

        protected const string UnknownDesignKey = "<unknown>";

        private ushort _designId;
        private string _key;
        //private string _name;
        //private string _description;
        private string _image;
        private int _buildCost;
        private int _maintenanceCost;
        private byte _populationHealth;
        private bool _isUniversallyAvailable;
        private readonly ScriptExpression _buildCondition;

        [NonSerialized]
        private ITechObjectTextDatabaseEntry _textDatabaseEntry;
        [NonSerialized]
        private LocalizedTextGroup _localizedText;
        private readonly ResourceValueCollection _resourceCosts;
        private readonly TechLevelCollection _techRequirements;
        private readonly List<TechObjectDesign> _upgradableDesigns;
        private readonly List<TechObjectDesign> _obsoletedDesigns;
        private readonly PrerequisiteCollection _prerequisites;
        private readonly List<EffectGroup> _effects;

        /// <summary>
        /// Gets or sets the design ID.
        /// </summary>
        /// <value>The design ID.</value>
        public int DesignID
        {
            get => _designId;
            set => _designId = (ushort)value;
        }

        /// <summary>
        /// Gets or sets the unique key.
        /// </summary>
        /// <value>The unique key.</value>
        [ConstructorArgument("key")]
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                if (_localizedText != null)
                {
                    string name = _localizedText.GetString(TechObjectStringKeys.Name);
                    if (name != null)
                    {
                        return name;
                    }
                }
                if (TryEnsureObjectString())
                {
                    return TextDatabaseEntry.Name;
                }

                return Key;
            }
        }

        /// <summary>
        /// Gets the localized name.
        /// </summary>
        /// <value>The localized name.</value>
        public string LocalizedName => Name;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                if (_localizedText != null)
                {
                    string description = _localizedText.GetString(TechObjectStringKeys.Description);
                    if (description != null)
                    {
                        return description;
                    }
                }
                if (TryEnsureObjectString())
                {
                    return TextDatabaseEntry.Description;
                }

                return string.Empty;
            }
        }
        protected bool TryEnsureObjectString()
        {
            //if (!_objectStringLoaded && (ObjectString == null))
            //{
            //    if (!String.IsNullOrEmpty(_key))
            //    {
            //        try { this.ObjectString = ResourceManager.Database.FindObjectString(_key); }
            //        catch { }
            //    }
            //}
            return TextDatabaseEntry != null;
        }

        /// <summary>
        /// Gets the localized description.
        /// </summary>
        /// <value>The localized description.</value>
        public string LocalizedDescription => Description;

        //public string LocalizedClassLevel
        //{
        //    get { return ClassLevel; }
        //}

        /// <summary>
        /// Gets or sets the maintenance cost of an object of this design.
        /// </summary>
        /// <value>The maintenance cost.</value>
        public int MaintenanceCost
        {
            get => _maintenanceCost;
            set
            {
                _maintenanceCost = Math.Max(value, 0);
                OnPropertyChanged("MaintenanceCost");
            }
        }

        /// <summary>
        /// Gets or sets the raw materials cost of an object of this design.
        /// </summary>
        /// <value>The raw materials.</value>
        public int Duranium
        {
            get => BuildResourceCosts[ResourceType.Duranium];
            set
            {
                BuildResourceCosts[ResourceType.Duranium] = value;
                OnPropertyChanged("Duranium");
            }
        }

        /// <summary>
        /// Gets or sets the population health bonus of an object of this design.
        /// </summary>
        /// <value>The population health bonus.</value>
        public Percentage PopulationHealth
        {
            get => (Percentage)Math.Round(0.01 * _populationHealth, 3);
            set
            {
                _populationHealth = (byte)(value * 100);
                OnPropertyChanged("PopulationHealth");
            }
        }

        /// <summary>
        /// Gets or sets the filename of the image for this design.
        /// </summary>
        /// <value>The image filename.</value>
        public virtual string Image
        {
            get
            {
                if (IsImageDefined)
                {
                    if (IsImageLinked)
                    {
                        TechObjectDesign linkSource = ImageLinkSource;
                        if (linkSource != null)
                        {
                            return linkSource.Image;
                        }
                    }
                    else
                    {
                        return ResourceManager.GetResourceUri(_image).ToString();
                    }
                }

                string localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}.png",
                        DefaultImageSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}.jpg",
                        DefaultImageSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                return MissingImageUri;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
                OnPropertyChanged("ImageLinkSource");
                OnPropertyChanged("IsImageDefined");
                OnPropertyChanged("IsImageAutomatic");
                OnPropertyChanged("IsImageLinked");
            }
        }

        /// <summary>
        /// Gets or sets the filename of the image for this design.
        /// </summary>
        /// <value>The image filename.</value>
        public virtual string ShipUnderConstructionImage
        {
            get
            {
                if (IsImageDefined)
                {
                    if (IsImageLinked)
                    {
                        TechObjectDesign linkSource = ImageLinkSource;
                        if (linkSource != null)
                        {
                            return linkSource.Image;
                        }
                    }
                    else
                    {
                        return ResourceManager.GetResourceUri(_image).ToString();
                    }
                }

                string localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}_uc.png",
                        DefaultShipsUnderConstructionSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}_uc.jpg",
                        DefaultShipsUnderConstructionSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                // if not ShipsUnderConstruction-Image avaiable then try to get regular ship image - each .png or .jpg
                localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}.png",
                        DefaultImageSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                localPath = ResourceManager.GetResourcePath(
                    string.Format(
                        "vfs:///Resources/Images/{0}{1}.jpg",
                        DefaultImageSubFolder,
                        _key.ToLowerInvariant()));

                if (File.Exists(localPath))
                {
                    return ResourceManager.GetResourceUri(localPath).ToString();
                }

                // else return MissingImageUri
                return MissingImageUri;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
                OnPropertyChanged("ImageLinkSource");
                OnPropertyChanged("IsImageDefined");
                OnPropertyChanged("IsImageAutomatic");
                OnPropertyChanged("IsImageLinked");
            }
        }

        protected virtual string DefaultImageSubFolder => string.Empty;

        protected virtual string DefaultShipsUnderConstructionSubFolder => string.Empty;

        public ScriptExpression BuildCondition => _buildCondition;

        /// <summary>
        /// Gets a value indicating whether an image is explicitly defined.
        /// </summary>
        /// <value>
        /// <c>true</c> if an image is explicitly defined; otherwise, <c>false</c>.
        /// </value>
        public bool IsImageDefined => _image != null;

        /// <summary>
        /// Gets a value indicating whether the image is the default (automatic).
        /// </summary>
        /// <value>
        /// <c>true</c> if the image is the default (automatic); otherwise, <c>false</c>.
        /// </value>
        public bool IsImageAutomatic => _image == null;

        /// <summary>
        /// Gets a value indicating whether the image is linked to another object.
        /// </summary>
        /// <value>
        /// <c>true</c> if the image is linked to another object; otherwise, <c>false</c>.
        /// </value>
        public bool IsImageLinked => (_image != null) && _image.StartsWith("@");

        public TechObjectDesign ImageLinkSource
        {
            get
            {
                if (!string.IsNullOrEmpty(_image) && _image.StartsWith("@") && (_image.Length > 1))
                {
                    string linkKey = _image.Substring(1);
                    TechObjectDesign linkDesign = GameContext.Current.TechDatabase[linkKey];
                    if (linkDesign != null)
                    {
                        return linkDesign;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the list of prerequisite groups for this design.
        /// </summary>
        /// <value>The prerequisite groups.</value>
        /// <remarks>
        /// The return value is a list containing lists of equivalent prerequisites.
        /// That is, for each list in the return value, at least one of the items
        /// in the list must be present in order to construct an object of this design.
        /// </remarks>
        public IPrerequisiteCollection Prerequisites => _prerequisites;

        public IList<EffectGroup> Effects => _effects;

        /// <summary>
        /// Gets the list of designs to which an object of this design is upgradable.
        /// </summary>
        /// <value>The list of designs.</value>
        public IList<TechObjectDesign> UpgradableDesigns => _upgradableDesigns;

        /// <summary>
        /// Gets the list of designs rendered obsolete by this design.
        /// </summary>
        /// <value>The list of designs.</value>
        public IList<TechObjectDesign> ObsoletedDesigns => _obsoletedDesigns;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TechObjectDesign"/> is universally available.
        /// </summary>
        /// <value>
        /// <c>true</c> if universally available; otherwise, <c>false</c>.
        /// </value>
        public bool IsUniversallyAvailable
        {
            get => _isUniversallyAvailable;
            set
            {
                _isUniversallyAvailable = value;
                OnPropertyChanged("IsUniversallyAvailable");
            }
        }

        protected TechObjectDesign(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(SR.ArgumentException_ValueMustBeNonEmptyString);
            }

            _key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechObjectDesign"/> class.
        /// </summary>
        internal TechObjectDesign()
        {
            _buildCost = 0;
            _resourceCosts = new ResourceValueCollection();
            _techRequirements = new TechLevelCollection();
            _upgradableDesigns = new List<TechObjectDesign>();
            _obsoletedDesigns = new List<TechObjectDesign>();
            _prerequisites = new PrerequisiteCollection();
            _effects = new List<EffectGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechObjectDesign"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        protected internal TechObjectDesign(XmlElement element)
            : this()
        {
            // ReSharper disable PossibleNullReferenceException

            _key = element.GetAttribute("Key");
            //_name = element["Name"].InnerText.Trim();
            //_description = element["Description"].InnerText.Trim();
            if (element["Image"] != null)
            {
                _image = element["Image"].InnerText.Trim();
            }
            if (element["TechRequirements"] != null)
            {
                foreach (XmlNode xmlTechReqNode in element["TechRequirements"].ChildNodes)
                {
                    if (xmlTechReqNode is XmlElement xmlTechReq)
                    {
                        TechCategory category = (TechCategory)Enum.Parse(
                            typeof(TechCategory),
                            xmlTechReq.Name);
                        _techRequirements[category] = Number.ParseInt32(xmlTechReq.InnerText.Trim());
                    }
                }
                //_description = element["Description"].InnerText.Trim();
            }
            if (element["BuildCost"] != null)
            {
                _buildCost = Number.ParseInt32(element["BuildCost"].InnerText.Trim());
            }
            if (element["Duranium"] != null)
            {
                _resourceCosts[ResourceType.Duranium] = Number.ParseInt32(
                    element["Duranium"].InnerText.Trim());
            }
            if (element["MaintenanceCost"] != null)
            {
                _maintenanceCost = Number.ParseInt32(element["MaintenanceCost"].InnerText.Trim());
            }
            if (element["PopulationHealth"] != null)
            {
                _populationHealth = Number.ParseByte(element["PopulationHealth"].InnerText.Trim());
            }
            if (element["IsUniversallyAvailable"] != null)
            {
                _isUniversallyAvailable = Number.ParseBoolean(element["IsUniversallyAvailable"].InnerText.Trim().ToLowerInvariant());
            }
            if (element["BuildCondition"] != null)
            {
                _buildCondition = new ScriptExpression
                {
                    Parameters = new ScriptParameters(
                                          new ScriptParameter("$source", typeof(Colony)),
                                          new ScriptParameter("$design", GetType())),
                    ScriptCode = element["BuildCondition"].InnerText.Trim()
                };
            }
            // ReSharper restore PossibleNullReferenceException
        }

        public void LinkImageToDesign(TechObjectDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException("design");
            }

            if (design == this)
            {
                throw new ArgumentException("Cannot link to self.");
            }

            Image = "@" + design.Key;
        }

        /// <summary>
        /// Compacts this instance to reduce serialization footprint.
        /// </summary>
        public virtual void Compact()
        {
            _obsoletedDesigns.TrimExcess();
            _upgradableDesigns.TrimExcess();
            foreach (PrerequisiteGroup prerequisite in _prerequisites)
            {
                prerequisite.TrimExcess();
            }

            _prerequisites.TrimExcess();
            _effects.TrimExcess();
        }

        /// <summary>
        /// Appends the XML data for this instance.
        /// </summary>
        /// <param name="baseElement">The base XML element.</param>
        protected internal virtual void AppendXml(XmlElement baseElement)
        {
            XmlDocument doc = baseElement.OwnerDocument;
            XmlElement newElement;
            bool hasTechRequirements = false;

            baseElement.SetAttribute("Key", _key);

            //newElement = doc.CreateElement("Name");
            //newElement.InnerText = _name;
            //baseElement.AppendChild(newElement);

            //newElement = doc.CreateElement("Description");
            //newElement.InnerText = _description;
            //baseElement.AppendChild(newElement);

            if (_image != null)
            {
                newElement = doc.CreateElement("Image");
                newElement.InnerText = _image;
                _ = baseElement.AppendChild(newElement);
            }

            newElement = doc.CreateElement("TechRequirements");
            foreach (TechCategory category in EnumUtilities.GetValues<TechCategory>())
            {
                if (_techRequirements[category] > 0)
                {
                    XmlElement subElement = doc.CreateElement(category.ToString());
                    subElement.InnerText = _techRequirements[category].ToString();
                    _ = newElement.AppendChild(subElement);
                    hasTechRequirements = true;
                }
            }
            if (hasTechRequirements)
            {
                _ = baseElement.AppendChild(newElement);
            }

            newElement = doc.CreateElement("BuildCost");
            newElement.InnerText = _buildCost.ToString();
            _ = baseElement.AppendChild(newElement);

            if (Duranium > 0)
            {
                newElement = doc.CreateElement("Duranium");
                newElement.InnerText = _resourceCosts[ResourceType.Duranium].ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (MaintenanceCost > 0)
            {
                newElement = doc.CreateElement("MaintenanceCost");
                newElement.InnerText = _maintenanceCost.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (PopulationHealth > 0)
            {
                newElement = doc.CreateElement("PopulationHealth");
                newElement.InnerText = _populationHealth.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            newElement = doc.CreateElement("IsUniversallyAvailable");
            newElement.InnerText = _isUniversallyAvailable.ToString().ToLowerInvariant();
            _ = baseElement.AppendChild(newElement);

            if (_prerequisites.Count > 0)
            {
                newElement = doc.CreateElement("Prerequisites");
                foreach (PrerequisiteGroup prereqList in _prerequisites)
                {
                    XmlElement subElement = doc.CreateElement("EquivalentPrerequisites");
                    foreach (TechObjectDesign prereq in prereqList)
                    {
                        XmlElement subSubElement = doc.CreateElement("Prerequisite");
                        subSubElement.InnerText = prereq.Key;
                        _ = subElement.AppendChild(subSubElement);
                    }
                    if (subElement.ChildNodes.Count > 0)
                    {
                        _ = newElement.AppendChild(subElement);
                    }
                }
                if (newElement.ChildNodes.Count > 0)
                {
                    _ = baseElement.AppendChild(newElement);
                }
            }

            if (_obsoletedDesigns.Count > 0)
            {
                newElement = doc.CreateElement("ObsoletedItems");
                foreach (TechObjectDesign design in _obsoletedDesigns)
                {
                    XmlElement subElement = doc.CreateElement("ObsoletedItem");
                    subElement.InnerText = design.Key;
                    _ = newElement.AppendChild(subElement);
                }
                if (newElement.ChildNodes.Count > 0)
                {
                    _ = baseElement.AppendChild(newElement);
                }
            }

            if (_upgradableDesigns.Count > 0)
            {
                newElement = doc.CreateElement("UpgradeOptions");
                foreach (TechObjectDesign design in _upgradableDesigns)
                {
                    XmlElement subElement = doc.CreateElement("UpgradeOption");
                    subElement.InnerText = design.Key;
                    _ = newElement.AppendChild(subElement);
                }
                if (newElement.ChildNodes.Count > 0)
                {
                    _ = baseElement.AppendChild(newElement);
                }
            }
        }

        /// <summary>
        /// Gets the credits and resources that should be returned to a civilization when an object
        /// of this design is scrapped.
        /// </summary>
        /// <param name="credits">The credits.</param>
        /// <param name="resources">The resources.</param>
        protected internal virtual void GetScrapReturn(out int credits, out ResourceValueCollection resources)
        {
            Data.Table returnsTable = GameContext.Current.Tables.GameOptionTables["ScrapReturns"];
            double multiplier = Number.ParseDouble(returnsTable[0][0]);
            credits = (int)Math.Floor(multiplier * BuildCost);
            resources = new ResourceValueCollection();
            foreach (ResourceType resource in EnumUtilities.GetValues<ResourceType>())
            {
                multiplier = Number.ParseDouble(returnsTable[resource.ToString()][0]);
                resources[resource] += (int)Math.Floor(
                    multiplier * BuildResourceCosts[resource]);
            }
        }

        /// <summary>
        /// Spawns an instance of an object of this design at the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="spawnedInstance"> </param>
        public abstract bool TrySpawn(MapLocation location, Civilization owner, out TechObject spawnedInstance);

        protected virtual bool CanSpawn(
            MapLocation location,
            Civilization owner,
            bool requireSectorOwned = false,
            bool requireStarSystem = false,
            bool requireColony = false)
        {
            CivilizationManager civManager = GameContext.Current.CivilizationManagers[owner];
            if (civManager == null)
            {
                GameLog.Core.General.DebugFormat("Cannot spawn {0} at location {1} because owner {2} is not active in this game.",
                    Key, location, owner.Key);

                return false;
            }

            Sector sector = GameContext.Current.Universe.Map[location];

            if (requireSectorOwned &&
                !Equals(sector.Owner, owner))
            {
                GameLog.Core.General.DebugFormat("Cannot spawn {0} at location {1} because the sector is not owned by {2}.",
                    Key, location, owner.Key);

                return false;
            }

            if (requireStarSystem || requireColony)
            {
                StarSystem system = sector.System;
                if (system == null)
                {
                    GameLog.Core.General.DebugFormat("Cannot spawn {0} at location {1} because there is no star system at that location.",
                        Key, location);

                    return false;
                }

                if (requireColony && !system.HasColony)
                {
                    GameLog.Core.General.DebugFormat("Cannot spawn {0} at location {1} because there is no colony at that location.",
                        Key, location);

                    return false;
                }
            }

            return true;
        }

        #region IBuildable Members
        /// <summary>
        /// Gets the resource costs for building an object of this design..
        /// </summary>
        /// <value>The resource costs.</value>
        public ResourceValueCollection BuildResourceCosts => _resourceCosts;

        /// <summary>
        /// Gets or sets the industry cost for building an object of this design.
        /// </summary>
        /// <value>The build cost.</value>
        public int BuildCost
        {
            get
            {
                if (_buildCost == 0)
                {
                    _buildCost = 1;  // e.g. MARTIAL LAW  // a zero causes a crash if it is devided by _buildCosts for calculating percentage
                }

                return _buildCost;
            }
            set => _buildCost = Math.Max(value, 0);
        }

        /// <summary>
        /// Gets the tech level requirements for building an object of this design.
        /// </summary>
        /// <value>The tech level requirements.</value>
        public TechLevelCollection TechRequirements => _techRequirements;
        #endregion

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TechObjectDesign a, TechObjectDesign b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a._key == b._key;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TechObjectDesign a, TechObjectDesign b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (a is null || b is null)
            {
                return true;
            }

            return a._key != b._key;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="TechObjectDesign"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="TechObjectDesign"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="TechObjectDesign"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as TechObjectDesign);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TechObjectDesign"/> is equal to the current <see cref="TechObjectDesign"/>.
        /// </summary>
        /// <param name="design">The <see cref="TechObjectDesign"/> to compare with the current <see cref="TechObjectDesign"/>.</param>
        /// <returns>
        /// true if the specified <see cref="TechObjectDesign"/> is equal to the current <see cref="TechObjectDesign"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public virtual bool Equals(TechObjectDesign design)
        {
            if (design == null)
            {
                return false;
            }

            return Equals(_key, design._key);
        }

        /// <summary>
        /// Serves as a hash function for a particular design.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override int GetHashCode()
        {
            return _designId;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="TechObjectDesign"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="TechObjectDesign"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return _key;
        }

        #region IEncyclopediaEntry Members
        /// <summary>
        /// Gets the heading displayed in the Encyclopedia index.
        /// </summary>
        /// <value>The heading.</value>
        public string EncyclopediaHeading => Name;

        /// <summary>
        /// Gets the text displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The text.</value>
        public string EncyclopediaText => Description;

        /// <summary>
        /// Gets the image displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The image.</value>
        public string EncyclopediaImage => Image;

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public virtual EncyclopediaCategory EncyclopediaCategory => EncyclopediaCategory.None;

        protected internal ITechObjectTextDatabaseEntry TextDatabaseEntry
        {
            get => _textDatabaseEntry;
            set
            {
                //_objectStringLoaded = true;
                _textDatabaseEntry = value;
                OnPropertyChanged("TextDatabaseEntry");
                OnPropertyChanged("Name");
                OnPropertyChanged("Description");
                OnPropertyChanged("EncyclopediaHeading");
                OnPropertyChanged("EncyclopediaText");
            }
        }

        protected internal LocalizedTextGroup LocalizedText
        {
            get => _localizedText;
            set
            {
                //_objectStringLoaded = true;
                _localizedText = value;
                OnPropertyChanged("LocalizedText");
                OnPropertyChanged("Name");
                OnPropertyChanged("Description");
                OnPropertyChanged("EncyclopediaHeading");
                OnPropertyChanged("EncyclopediaText");
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected internal void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}