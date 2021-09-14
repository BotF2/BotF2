// ResearchMatrix.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;



using System.Linq;

namespace Supremacy.Tech
{
    /// <summary>
    /// Defines the tech categories used in the game.
    /// </summary>
    public enum TechCategory : byte
    {
        /// <summary>
        /// Bio-Tech
        /// </summary>
        BioTech = 0,
        /// <summary>
        /// Energy
        /// </summary>
        Energy,
        /// <summary>
        /// Computers
        /// </summary>
        Computers,
        /// <summary>
        /// Propulsion
        /// </summary>
        Propulsion,
        /// <summary>
        /// Construction
        /// </summary>
        Construction,
        /// <summary>
        /// Weapons
        /// </summary>
        Weapons
    }

    /// <summary>
    /// Contains tech levels for each <see cref="TechCategory"/>.
    /// </summary>
    [Serializable]
    public sealed class TechLevelCollection : IEnumerable<KeyValuePair<TechCategory, int>>
    {
        private readonly byte[] _values;

        /// <summary>
        /// Gets or sets the tech level for the specified <see cref="TechCategory"/>.
        /// </summary>
        /// <value>The tech level.</value>
        public int this[TechCategory category]
        {
            get => _values[(int)category];
            set => _values[(int)category] = (byte)value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechLevelCollection"/> class.
        /// </summary>
        public TechLevelCollection()
        {
            _values = new byte[EnumUtilities.GetValues<TechCategory>().Count];
        }

        public int HighestTechLevel => _values.Max();

        #region IEnumerable<KeyValuePair<TechCategory,int>> Members
        public IEnumerator<KeyValuePair<TechCategory, int>> GetEnumerator()
        {
            foreach (TechCategory category in EnumUtilities.GetValues<TechCategory>())
            {
                yield return new KeyValuePair<TechCategory, int>(category, this[category]);
            }
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    /// <summary>
    /// Represents the research matrix used in the game.  The matrix is a set of unique
    /// <see cref="ResearchField"/>s, each containing a set of unique
    /// <see cref="ResearchApplication"/>s.
    /// </summary>
    [Serializable]
    public sealed class ResearchMatrix
    {
        private const string XmlFilePath = "Resources/Data/ResearchMatrix.xml";

        private readonly List<ResearchField> _fields;
        private readonly Dictionary<int, ResearchApplication> _applicationMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchMatrix"/> class.
        /// </summary>
        public ResearchMatrix()
        {
            _fields = new List<ResearchField>();
            _applicationMap = new Dictionary<int, ResearchApplication>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchMatrix"/> class from XML data.
        /// </summary>
        /// <param name="element">The root XML element.</param>
        public ResearchMatrix(XmlElement element) : this()
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element["Fields"] != null)
            {
                foreach (XmlElement fieldElement in
                    element["Fields"].GetElementsByTagName("Field"))
                {
                    _fields.Add(new ResearchField(fieldElement));
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ResearchField"/>s contained in this <see cref="ResearchMatrix"/>.
        /// </summary>
        /// <value>The <see cref="ResearchField"/>.</value>
        public IList<ResearchField> Fields => _fields.AsReadOnly();

        /// <summary>
        /// Gets the <see cref="ResearchField"/> corresponding to the specified field ID.
        /// </summary>
        /// <param name="fieldId">The field ID.</param>
        /// <returns>The <see cref="ResearchField"/> corresponding to the specified field ID.</returns>
        public ResearchField GetField(int fieldId)
        {
            return _fields[fieldId];
        }

        /// <summary>
        /// Gets the <see cref="ResearchApplication"/> corresponding to the specified application ID.
        /// </summary>
        /// <param name="applicationId">The application ID.</param>
        /// <returns>The <see cref="ResearchApplication"/> corresponding to the specified application ID.</returns>
        public ResearchApplication GetApplication(int applicationId)
        {
            if (_applicationMap.ContainsKey(applicationId))
            {
                return _applicationMap[applicationId];
            }

            return null;
        }

        /// <summary>
        /// Loads the research matrix from XML.
        /// </summary>
        /// <returns>The research matrix.</returns>
        public static ResearchMatrix Load()
        {
            int nextFieldId = 0;
            int nextApplicationId = 0;

            XmlSchemaSet schemas = new XmlSchemaSet();
            ResearchMatrix matrix = new ResearchMatrix();
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement xmlFields;

            _ = schemas.Add(
                "Supremacy:Supremacy.xsd",
                ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
            _ = schemas.Add(
                "Supremacy:ResearchMatrix.xsd",
                ResourceManager.GetResourcePath("Resources/Data/ResearchMatrix.xsd"));

            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
            xmlDoc.Schemas.Add(schemas);
            xmlDoc.Validate(ValidateXml);

            xmlFields = xmlDoc.DocumentElement["Fields"];

            foreach (XmlElement xmlField in xmlFields.GetElementsByTagName("Field"))
            {
                matrix._fields.Add(new ResearchField(xmlField));
            }

            foreach (ResearchField field in matrix.Fields)
            {
                field.FieldID = nextFieldId++;
                foreach (ResearchApplication application in field.Applications)
                {
                    application.ApplicationID = nextApplicationId++;
                    application.Field = field;
                    matrix._applicationMap[application.ApplicationID] = application;
                }
            }

            return matrix;
        }

        /// <summary>
        /// Validates the XML.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="ValidationEventArgs"/> instance containing the event data.
        /// </param>
        private static void ValidateXml(object sender, ValidationEventArgs e)
        {
            XmlHelper.ValidateXml(XmlFilePath, e);
        }
    }

    /// <summary>
    /// Represents a research field in the game's research matrix.
    /// </summary>
    [Serializable]
    public sealed class ResearchField
    {
        /// <summary>
        /// Represents an invalid value for the <see cref="FieldID"/> property.
        /// </summary>
        public const int InvalidFieldID = -1;

        private int _fieldId;
        private string _name;
        private string _description;
        private TechCategory _category;
        private List<ResearchApplication> _applications;

        /// <summary>
        /// Gets or sets the unique ID of this <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The unique ID.</value>
        public int FieldID
        {
            get => _fieldId;
            internal set => _fieldId = value;
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Gets or sets the description of this <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        /// <summary>
        /// Gets or sets the tech category on which this <see cref="ResearchField"/> depends.
        /// </summary>
        /// <value>The tech category.</value>
        public TechCategory TechCategory
        {
            get => _category;
            set => _category = value;
        }

        /// <summary>
        /// Gets image file path that represents this <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The image file path.</value>
        public string Image
        {
            get
            {
                string imagePath = ResourceManager.GetResourcePath(
                    string.Format(
                        @"vfs:///Resources/Images/Research/Fields/{0}.png",
                        ResourceManager.GetString(_name)));

                if (File.Exists(imagePath))
                {
                    return ResourceManager.GetResourceUri(imagePath).ToString();
                }

                return "vfs:///Resources/Images/__image_missing.png";
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="ResearchApplication"/>s contained in this
        /// <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The the collection of <see cref="ResearchApplication"/>s contained in
        /// this <see cref="ResearchField"/>.</value>
        public ICollection<ResearchApplication> Applications => _applications.AsReadOnly();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchField"/> class.
        /// </summary>
        public ResearchField()
        {
            _applications = new List<ResearchApplication>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchField"/> class using the
        /// specified <see cref="TechCategory"/>.
        /// </summary>
        /// <param name="category">The tech category.</param>
        public ResearchField(TechCategory category) : this()
        {
            _category = category;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchField"/> class using XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public ResearchField(XmlElement element) : this()
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element["Name"] != null)
            {
                _name = element["Name"].InnerText.Trim();
            }
            if (element["Description"] != null)
            {
                _description = element["Description"].InnerText.Trim();
            }
            if (element["TechCategory"] != null)
            {
                _category = (TechCategory)Enum.Parse(
                    typeof(TechCategory),
                    element["TechCategory"].InnerText.Trim());
            }
            if (element["Applications"] != null)
            {
                foreach (XmlElement appElement in
                    element["Applications"].GetElementsByTagName("Application"))
                {
                    _applications.Add(new ResearchApplication(appElement));
                }
                _applications.Sort(
                    delegate (ResearchApplication left, ResearchApplication right)
                    {
                        if (left == null)
                        {
                            return -1;
                        }

                        if (right == null)
                        {
                            return 1;
                        }

                        if (left.Level != right.Level)
                        {
                            return left.Level.CompareTo(right.Level);
                        }

                        return left.ResearchCost.CompareTo(right.ResearchCost);
                    });
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ResearchField a, ResearchField b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a._fieldId == b._fieldId;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ResearchField a, ResearchField b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return true;
            }

            return a._fieldId != b._fieldId;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="ResearchField"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="ResearchField"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="ResearchField"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            return Equals(obj as ResearchField);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ResearchField"/> is equal to the current <see cref="ResearchField"/>.
        /// </summary>
        /// <param name="researchField">The <see cref="ResearchField"/> to compare with the current <see cref="ResearchField"/>.</param>
        /// <returns>
        /// true if the specified <see cref="ResearchField"/> is equal to the current <see cref="ResearchField"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public bool Equals(ResearchField researchField)
        {
            if (researchField == null)
            {
                return false;
            }

            return _fieldId == researchField._fieldId;
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
            return _fieldId;
        }
    }

    /// <summary>
    /// Represents a research application belonging to some field in the game's research matrix.
    /// </summary>
    [Serializable]
    public sealed class ResearchApplication : ICloneable
    {
        private short _applicationId;
        private short _fieldId;
        private byte _level;
        private string _name;
        private string _description;
        private int _researchCost;

        /// <summary>
        /// Gets or sets the unique ID of this <see cref="ResearchApplication"/>.
        /// </summary>
        /// <value>The unique ID.</value>
        public int ApplicationID
        {
            get => _applicationId;
            internal set => _applicationId = (short)Math.Min(value, short.MaxValue);
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="ResearchApplication"/>.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Gets or sets the description of this <see cref="ResearchApplication"/>.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        /// <summary>
        /// Gets or sets the field to which this <see cref="ResearchApplication"/> belongs.
        /// </summary>
        /// <value>The field.</value>
        public ResearchField Field
        {
            get => (_fieldId == ResearchField.InvalidFieldID)
                    ? null
                    : GameContext.Current.ResearchMatrix.GetField(_fieldId);
            set => _fieldId = (value != null)
                    ? (short)Math.Min(value.FieldID, short.MaxValue)
                    : (short)ResearchField.InvalidFieldID;
        }

        /// <summary>
        /// Gets or sets the tech level at which this <see cref="ResearchApplication"/>
        /// appears in its parent <see cref="ResearchField"/>.
        /// </summary>
        /// <value>The tech level.</value>
        public int Level
        {
            get => _level;
            set => _level = (byte)value;
        }

        /// <summary>
        /// Gets or sets the research cost of this <see cref="ResearchApplication"/>.
        /// </summary>
        /// <value>The research cost.</value>
        public int ResearchCost
        {
            get => _researchCost;
            set => _researchCost = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchApplication"/> class.
        /// </summary>
        public ResearchApplication() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchApplication"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public ResearchApplication(XmlElement element) : this()
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element["Name"] != null)
            {
                _name = element["Name"].InnerText.Trim();
            }
            if (element["Description"] != null)
            {
                _description = element["Description"].InnerText.Trim();
            }
            if (element["Level"] != null)
            {
                _level = Number.ParseByte(element["Level"].InnerText.Trim());
            }
            if (element["ResearchCost"] != null)
            {
                _researchCost = Number.ParseInt32(element["ResearchCost"].InnerText.Trim());
            }
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
            return _applicationId;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="ResearchApplication"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="ResearchApplication"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="ResearchApplication"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as ResearchApplication);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ResearchApplication"/> is equal to the current <see cref="ResearchApplication"/>.
        /// </summary>
        /// <param name="researchApplication">The <see cref="ResearchApplication"/> to compare with the current <see cref="ResearchApplication"/>.</param>
        /// <returns>
        /// true if the specified <see cref="ResearchApplication"/> is equal to the current <see cref="ResearchApplication"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public bool Equals(ResearchApplication researchApplication)
        {
            if (ReferenceEquals(researchApplication, this))
            {
                return true;
            }

            if (researchApplication is null)
            {
                return false;
            }

            return _applicationId == researchApplication._applicationId;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ResearchApplication a, ResearchApplication b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a._applicationId == b._applicationId;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ResearchApplication a, ResearchApplication b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return true;
            }

            return a._applicationId != b._applicationId;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The clone.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The clone.</returns>
        public ResearchApplication Clone()
        {
            ResearchApplication item = new ResearchApplication();
            item._applicationId = _applicationId;
            item._level = _level;
            item._name = _name;
            item._description = _description;
            item._researchCost = _researchCost;
            return item;
        }
        #endregion
    }
}
