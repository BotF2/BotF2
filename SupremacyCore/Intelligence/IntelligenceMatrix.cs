//// IntelMatrix.cs
////
//// Copyright (c) 2007 Mike Strobel
////
//// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
//// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
////
//// All other rights reserved.

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Xml;
//using System.Xml.Schema;
//using Supremacy.Game;
//using Supremacy.Resources;
//using Supremacy.Types;
//using Supremacy.Utility;



//using System.Linq;

//namespace Supremacy.Economy
//{
//    /// <summary>
//    /// Defines the intel categories used in the game.
//    /// </summary>
//    public enum IntelCategory : byte // do we still need this? No longer makeing intel like research ResearchMatrix (IntelligenceMatrix)
//    {
//        /// <summary>
//        /// Bio-Intel
//        /// </summary>
//        Military = 0,
//        /// <summary>
//        /// Energy
//        /// </summary>
//        Economy,
//        /// <summary>
//        /// Computers
//        /// </summary>
//        Bribe,
//        /// <summary>
//        /// Propulsion
//        /// </summary>
//        Research,
//        /// <summary>
//        /// Construction
//        /// </summary>
//        Reputation,
//        /// <summary>
//        /// Weapons
//        /// </summary>
//        //Weapons
//    }

//    /// <summary>
//    /// Contains intel levels for each <see cref="IntelCategory"/>.
//    /// </summary>
//    [Serializable]
//    public sealed class IntelLevelCollection : IEnumerable<KeyValuePair<IntelCategory, int>> // do we still need this? No longer makeing intel like research ResearchMatrix (IntelligenceMatrix)
//    {
//        private readonly byte[] _values;

//        /// <summary>
//        /// Gets or sets the intel level for the specified <see cref="IntelCategory"/>.
//        /// </summary>
//        /// <value>The intel level.</value>
//        public int this[IntelCategory category]
//        {
//            get { return _values[(int)category]; }
//            set { _values[(int)category] = (byte)value; }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelLevelCollection"/> class.
//        /// </summary>
//        public IntelLevelCollection()
//        {
//            _values = new byte[EnumUtilities.GetValues<IntelCategory>().Count];
//        }

//        public int HighestIntelLevel
//        {
//            get { return _values.Max(); }
//        }

//        #region IEnumerable<KeyValuePair<IntelCategory,int>> Members
//        public IEnumerator<KeyValuePair<IntelCategory, int>> GetEnumerator()
//        {
//            foreach (IntelCategory category in EnumUtilities.GetValues<IntelCategory>())
//            {
//                yield return new KeyValuePair<IntelCategory, int>(category, this[category]);
//            }
//        }
//        #endregion

//        #region IEnumerable Members
//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//        #endregion
//    }

//    /// <summary>
//    /// Represents the intelligence matrix used in the game.  The matrix is a set of unique
//    /// <see cref="IntelField"/>s, each containing a set of unique
//    /// <see cref="IntelApplication"/>s.
//    /// </summary>
//    [Serializable]
//    public sealed class IntelMatrix // do we still need this? No longer makeing intel like research ResearchMatrix (IntelligenceMatrix)
//    {
//        private const string XmlFilePath = "Resources/Data/IntelMatrix.xml";

//        private readonly List<IntelField> _fields;
//        private readonly Dictionary<int, IntelApplication> _applicationMap;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelMatrix"/> class.
//        /// </summary>
//        public IntelMatrix()
//        {
//            _fields = new List<IntelField>();
//            _applicationMap = new Dictionary<int, IntelApplication>();
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelMatrix"/> class from XML data.
//        /// </summary>
//        /// <param name="element">The root XML element.</param>
//        public IntelMatrix(XmlElement element) : this()
//        {
//            if (element == null)
//                throw new ArgumentNullException("element");
//            if (element["Fields"] != null)
//            {
//                foreach (XmlElement fieldElement in
//                    element["Fields"].GetElementsByTagName("Field"))
//                {
//                    _fields.Add(new IntelField(fieldElement));
//                    GameLog.Core.Intel.DebugFormat("added intelligence field: {0}", fieldElement.Name);
//                }
//            }
//        }

//        /// <summary>
//        /// Gets the <see cref="IntelField"/>s contained in this <see cref="IntelMatrix"/>.
//        /// </summary>
//        /// <value>The <see cref="IntelField"/>.</value>
//        public IList<IntelField> Fields
//        {
//            get { return _fields.AsReadOnly(); }
//        }

//        /// <summary>
//        /// Gets the <see cref="IntelField"/> corresponding to the specified field ID.
//        /// </summary>
//        /// <param name="fieldId">The field ID.</param>
//        /// <returns>The <see cref="IntelField"/> corresponding to the specified field ID.</returns>
//        public IntelField GetField(int fieldId)
//        {
//            return _fields[fieldId];
//        }

//        /// <summary>
//        /// Gets the <see cref="IntelApplication"/> corresponding to the specified application ID.
//        /// </summary>
//        /// <param name="applicationId">The application ID.</param>
//        /// <returns>The <see cref="IntelApplication"/> corresponding to the specified application ID.</returns>
//        public IntelApplication GetApplication(int applicationId)
//        {
//            if (_applicationMap.ContainsKey(applicationId))
//                return _applicationMap[applicationId];
//            return null;
//        }

//        /// <summary>
//        /// Loads the intelligence matrix from XML.
//        /// </summary>
//        /// <returns>The intelligence matrix.</returns>
//        public static IntelMatrix Load()
//        {
//            int nextFieldId = 0;
//            int nextApplicationId = 0;

//            XmlSchemaSet schemas = new XmlSchemaSet();
//            IntelMatrix matrix = new IntelMatrix();
//            XmlDocument xmlDoc = new XmlDocument();

//            XmlElement xmlFields;

//            // the check against Supremacy.xsd goes still to TechCategroy... why??
//            //schemas.Add(
//            //    "Supremacy:Supremacy.xsd",
//            //    ResourceManager.GetResourcePath("Resources/Data/Supremacy.xsd"));
//            schemas.Add(
//                "Supremacy:IntelMatrix.xsd",
//                ResourceManager.GetResourcePath("Resources/Data/IntelMatrix.xsd"));

//            xmlDoc.Load(ResourceManager.GetResourcePath(XmlFilePath));
//            xmlDoc.Schemas.Add(schemas);
//            xmlDoc.Validate(ValidateXml);

//            xmlFields = xmlDoc.DocumentElement["Fields"];

//            foreach (XmlElement xmlField in xmlFields.GetElementsByTagName("Field"))
//            {
//                matrix._fields.Add(new IntelField(xmlField));
//                //GameLog.Core.Intel.DebugFormat("added xmlField: {0}", 
//                //    xmlField.FirstChild.Name);
//            }

//            foreach (IntelField field in matrix.Fields)
//            {
//                field.FieldID = nextFieldId++;
//                //GameLog.Core.Intel.DebugFormat("----------------------");
//                foreach (IntelApplication application in field.Applications)
//                {
//                    var techLevel = GameContext.Current.Options.StartingTechLevel;
//                    //GameLog.Core.Intel.DebugFormat("Level {0} = equal to {1} ??", application.Description.ToString(), techLevel.ToString().ToUpper());
//                    if (application.Description.ToString() == techLevel.ToString().ToUpper())  // just the current Intel level (EARLY etc.)
//                    {
//                        application.ApplicationID = nextApplicationId++;
//                        application.Field = field;
//                        matrix._applicationMap[application.ApplicationID] = application;
                        
//                        // IntelMatrix isn't usedd anyway at the moment
//                        //GameLog.Core.Intel.DebugFormat("adding: {0};{1};{2};{3};{4}  for StartingLevel={5}={6}"
//                        //    , application.ApplicationID
//                        //    , field.IntelCategory
//                        //    , field.Name
//                        //    , application.Name
//                        //    , application.InitialIntelValue
//                        //    , application.Level
//                        //    , application.Description
//                        //    );
//                    }
//                }
//            }

//            return matrix;
//        }

//        /// <summary>
//        /// Validates the XML.
//        /// </summary>
//        /// <param name="sender">The sender.</param>
//        /// <param name="e">
//        /// The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.
//        /// </param>
//        private static void ValidateXml(object sender, ValidationEventArgs e)
//        {
//            //XmlHelper.ValidateXml(XmlFilePath, e);
//        }
//    }

//    /// <summary>
//    /// Represents a intelligence field in the game's intelligence matrix.
//    /// </summary>
//    [Serializable]
//    public sealed class IntelField
//    {
//        /// <summary>
//        /// Represents an invalid value for the <see cref="FieldID"/> property.
//        /// </summary>
//        public const int InvalidFieldID = -1;

//        private int _fieldId;
//        private string _name;
//        private string _description;
//        private IntelCategory _category;
//        private List<IntelApplication> _applications;

//        /// <summary>
//        /// Gets or sets the unique ID of this <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The unique ID.</value>
//        public int FieldID
//        {
//            get { return _fieldId; }
//            internal set { _fieldId = value; }
//        }

//        /// <summary>
//        /// Gets or sets the name of this <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The name.</value>
//        public string Name
//        {
//            get { return _name; }
//            set { _name = value; }
//        }

//        /// <summary>
//        /// Gets or sets the description of this <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The description.</value>
//        public string Description
//        {
//            get { return _description; }
//            set { _description = value; }
//        }

//        /// <summary>
//        /// Gets or sets the intel category on which this <see cref="IntelField"/> depends.
//        /// </summary>
//        /// <value>The intel category.</value>
//        public IntelCategory IntelCategory
//        {
//            get { return _category; }
//            set { _category = value; }
//        }

//        /// <summary>
//        /// Gets image file path that represents this <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The image file path.</value>
//        public string Image
//        {
//            get
//            {
//                var imagePath = ResourceManager.GetResourcePath(
//                    string.Format(
//                        @"vfs:///Resources/Images/Intelligence/Fields/{0}.png",
//                        ResourceManager.GetString(_name)));

//                if (File.Exists(imagePath))
//                    return ResourceManager.GetResourceUri(imagePath).ToString();

//                return "vfs:///Resources/Images/__image_missing.png";
//            }
//        }

//        /// <summary>
//        /// Gets the collection of <see cref="IntelApplication"/>s contained in this
//        /// <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The the collection of <see cref="IntelApplication"/>s contained in
//        /// this <see cref="IntelField"/>.</value>
//        public ICollection<IntelApplication> Applications
//        {
//            get { return _applications.AsReadOnly(); }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelField"/> class.
//        /// </summary>
//        public IntelField()
//        {
//            _applications = new List<IntelApplication>();
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelField"/> class using the
//        /// specified <see cref="IntelCategory"/>.
//        /// </summary>
//        /// <param name="category">The intel category.</param>
//        public IntelField(IntelCategory category) : this()
//        {
//            _category = category;
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelField"/> class using XML data.
//        /// </summary>
//        /// <param name="element">The XML element.</param>
//        public IntelField(XmlElement element) : this()
//        {
//            if (element == null)
//                throw new ArgumentNullException("element");

//            if (element["Name"] != null)
//            {
//                _name = element["Name"].InnerText.Trim();
//            }
//            if (element["Description"] != null)
//            {
//                _description = element["Description"].InnerText.Trim();
//            }
//            if (element["IntelCategory"] != null)
//            {
//                _category = (IntelCategory)Enum.Parse(
//                    typeof(IntelCategory),
//                    element["IntelCategory"].InnerText.Trim());
//            }
//            if (element["Applications"] != null)
//            {
//                foreach (XmlElement appElement in
//                    element["Applications"].GetElementsByTagName("Application"))
//                {
//                    _applications.Add(new IntelApplication(appElement));
//                }
//                _applications.Sort(
//                    delegate (IntelApplication left, IntelApplication right)
//                    {
//                        if (left == null)
//                            return -1;
//                        if (right == null)
//                            return 1;
//                        if (left.Level != right.Level)
//                            return left.Level.CompareTo(right.Level);
//                        return left.InitialIntelValue.CompareTo(right.InitialIntelValue);
//                    });
//            }
//        }

//        /// <summary>
//        /// Implements the operator ==.
//        /// </summary>
//        /// <param name="a">The first operand.</param>
//        /// <param name="b">The second operand.</param>
//        /// <returns>The result of the operator.</returns>
//        public static bool operator ==(IntelField a, IntelField b)
//        {
//            if (ReferenceEquals(a, b))
//                return true;
//            if (((object)a == null) || ((object)b == null))
//                return false;
//            return (a._fieldId == b._fieldId);
//        }

//        /// <summary>
//        /// Implements the operator !=.
//        /// </summary>
//        /// <param name="a">The first operand.</param>
//        /// <param name="b">The second operand.</param>
//        /// <returns>The result of the operator.</returns>
//        public static bool operator !=(IntelField a, IntelField b)
//        {
//            if (ReferenceEquals(a, b))
//                return false;
//            if (((object)a == null) || ((object)b == null))
//                return true;
//            return (a._fieldId != b._fieldId);
//        }

//        /// <summary>
//        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="IntelField"/>.
//        /// </summary>
//        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="IntelField"/>.</param>
//        /// <returns>
//        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="IntelField"/>; otherwise, false.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(obj, this))
//                return true;
//            return Equals(obj as IntelField);
//        }

//        /// <summary>
//        /// Determines whether the specified <see cref="IntelField"/> is equal to the current <see cref="IntelField"/>.
//        /// </summary>
//        /// <param name="intelligenceField">The <see cref="IntelField"/> to compare with the current <see cref="IntelField"/>.</param>
//        /// <returns>
//        /// true if the specified <see cref="IntelField"/> is equal to the current <see cref="IntelField"/>; otherwise, false.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public bool Equals(IntelField intelligenceField)
//        {
//            if (intelligenceField == null)
//                return false;
//            return (_fieldId == intelligenceField._fieldId);
//        }

//        /// <summary>
//        /// Serves as a hash function for a particular design.
//        /// </summary>
//        /// <returns>
//        /// A hash code for the current <see cref="T:System.Object"/>.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public override int GetHashCode()
//        {
//            return _fieldId;
//        }
//    }

//    /// <summary>
//    /// Represents a intelligence application belonging to some field in the game's intelligence matrix.
//    /// </summary>
//    [Serializable]
//    public sealed class IntelApplication : ICloneable
//    {
//        private short _applicationId;
//        private short _fieldId;
//        private byte _level;
//        private string _name;
//        private string _description;
//        private int _initialIntelValue;

//        /// <summary>
//        /// Gets or sets the unique ID of this <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <value>The unique ID.</value>
//        public int ApplicationID
//        {
//            get { return _applicationId; }
//            internal set { _applicationId = (short)Math.Min(value, Int16.MaxValue); }
//        }

//        /// <summary>
//        /// Gets or sets the name of this <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <value>The name.</value>
//        public string Name
//        {
//            get { return _name; }
//            set { _name = value; }
//        }

//        /// <summary>
//        /// Gets or sets the description of this <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <value>The description.</value>
//        public string Description
//        {
//            get { return _description; }
//            set { _description = value; }
//        }

//        /// <summary>
//        /// Gets or sets the field to which this <see cref="IntelApplication"/> belongs.
//        /// </summary>
//        /// <value>The field.</value>
//        public IntelField Field
//        {
//            get
//            {
//                return (_fieldId == IntelField.InvalidFieldID)
//                    ? null
//                    : GameContext.Current.IntelMatrix.GetField(_fieldId);
//            }
//            set
//            {
//                _fieldId = (value != null)
//                    ? (short)Math.Min(value.FieldID, Int16.MaxValue)
//                    : (short)IntelField.InvalidFieldID;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the intel level at which this <see cref="IntelApplication"/>
//        /// appears in its parent <see cref="IntelField"/>.
//        /// </summary>
//        /// <value>The intel level.</value>
//        public int Level
//        {
//            get { return _level; }
//            set { _level = (byte)value; }
//        }

//        /// <summary>
//        /// Gets or sets the intelligence cost of this <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <value>The intelligence cost.</value>
//        public int InitialIntelValue
//        {
//            get { return _initialIntelValue; }
//            set { _initialIntelValue = value; }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelApplication"/> class.
//        /// </summary>
//        public IntelApplication() { }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntelApplication"/> class from XML data.
//        /// </summary>
//        /// <param name="element">The XML element.</param>
//        public IntelApplication(XmlElement element) : this()
//        {
//            if (element == null)
//                throw new ArgumentNullException("element");

//            if (element["Name"] != null)
//            {
//                _name = element["Name"].InnerText.Trim();
//            }
//            if (element["Description"] != null)
//            {
//                _description = element["Description"].InnerText.Trim();
//            }
//            if (element["Level"] != null)
//            {
//                _level = Number.ParseByte(element["Level"].InnerText.Trim());
//            }
//            if (element["InitialIntelValue"] != null)
//            {
//                _initialIntelValue = Number.ParseInt32(element["InitialIntelValue"].InnerText.Trim());
//            }
//        }

//        /// <summary>
//        /// Serves as a hash function for a particular design.
//        /// </summary>
//        /// <returns>
//        /// A hash code for the current <see cref="T:System.Object"/>.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public override int GetHashCode()
//        {
//            return _applicationId;
//        }

//        /// <summary>
//        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="IntelApplication"/>.</param>
//        /// <returns>
//        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="IntelApplication"/>; otherwise, false.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public override bool Equals(object obj)
//        {
//            return Equals(obj as IntelApplication);
//        }

//        /// <summary>
//        /// Determines whether the specified <see cref="IntelApplication"/> is equal to the current <see cref="IntelApplication"/>.
//        /// </summary>
//        /// <param name="IntelApplication">The <see cref="IntelApplication"/> to compare with the current <see cref="IntelApplication"/>.</param>
//        /// <returns>
//        /// true if the specified <see cref="IntelApplication"/> is equal to the current <see cref="IntelApplication"/>; otherwise, false.
//        /// </returns>
//        /// <filterPriority>2</filterPriority>
//        public bool Equals(IntelApplication IntelApplication)
//        {
//            if (ReferenceEquals(IntelApplication, this))
//                return true;
//            if (ReferenceEquals(IntelApplication, null))
//                return false;
//            return (_applicationId == IntelApplication._applicationId);
//        }

//        /// <summary>
//        /// Implements the operator ==.
//        /// </summary>
//        /// <param name="a">The first operand.</param>
//        /// <param name="b">The second operand.</param>
//        /// <returns>The result of the operator.</returns>
//        public static bool operator ==(IntelApplication a, IntelApplication b)
//        {
//            if (ReferenceEquals(a, b))
//                return true;
//            if (((object)a == null) || ((object)b == null))
//                return false;
//            return (a._applicationId == b._applicationId);
//        }

//        /// <summary>
//        /// Implements the operator !=.
//        /// </summary>
//        /// <param name="a">The first operand.</param>
//        /// <param name="b">The second operand.</param>
//        /// <returns>The result of the operator.</returns>
//        public static bool operator !=(IntelApplication a, IntelApplication b)
//        {
//            if (ReferenceEquals(a, b))
//                return false;
//            if (((object)a == null) || ((object)b == null))
//                return true;
//            return (a._applicationId != b._applicationId);
//        }

//        #region ICloneable Members
//        /// <summary>
//        /// Clones this instance.
//        /// </summary>
//        /// <returns>The clone.</returns>
//        object ICloneable.Clone()
//        {
//            return Clone();
//        }

//        /// <summary>
//        /// Clones this instance.
//        /// </summary>
//        /// <returns>The clone.</returns>
//        public IntelApplication Clone()
//        {
//            IntelApplication item = new IntelApplication();
//            item._applicationId = _applicationId;
//            item._level = _level;
//            item._name = _name;
//            item._description = _description;
//            item._initialIntelValue = _initialIntelValue;
//            return item;
//        }
//        #endregion
//    }
//}

