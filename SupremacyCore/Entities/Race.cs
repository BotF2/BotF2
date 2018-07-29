// Race.cs
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
using System.Xml.Linq;

using Supremacy.Annotations;
using Supremacy.Encyclopedia;
using Supremacy.IO.Serialization;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Entities
{
    /// <summary>
    /// Represents a race in the game.
    /// </summary>
    [Serializable]
    public class Race : IEncyclopediaEntry, IOwnedDataSerializable
    {
        public const string InvalidRaceKey = null;
        protected const string MissingImageUri = "vfs:///Resources/Images/__image_missing.png";

        private PlanetType _homePlanetType;
        private PlanetTypeFlags _habitablePlanetTypes = PlanetTypeFlags.StandardHabitablePlanets;
        private double _combatEffectiveness = 1.0;

        /// <summary>
        /// Gets or sets the unique key of this <see cref="Race"/>.
        /// </summary>
        /// <value>The unique key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the singular-form name of this <see cref="Race"/>.
        /// </summary>
        /// <value>The singular-form name.</value>
        public string SingularName { get; set; }

        /// <summary>
        /// Gets or sets the plural-form name of this <see cref="Race"/>.
        /// </summary>
        /// <value>The plural-form name.</value>
        public string PluralName { get; set; }

        /// <summary>
        /// Gets the display name of this <see cref="Race"/>.
        /// </summary>
        /// <value>The display name.</value>
        public string Name
        {
            get { return PluralName; }
        }

        /// <summary>
        /// Gets or sets the description of this <see cref="Race"/>.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the home planet type of this <see cref="Race"/>.
        /// </summary>
        /// <value>The home planet type.</value>
        public PlanetType HomePlanetType
        {
            get { return _homePlanetType; }
            set
            {
                if (!value.IsHabitable())
                    throw new ArgumentException("Planet type is marked as Uninhabitable: " + value);
                _homePlanetType = value;
            }
        }

        public double CombatEffectiveness
        {
            get { return _combatEffectiveness; }
            set { _combatEffectiveness = value; }
        }

        public PlanetTypeFlags HabitablePlanetTypes
        {
            get { return _habitablePlanetTypes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Race"/> class.
        /// </summary>
        public Race() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Race"/> class using XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public Race([NotNull] XElement element)
        {
            System.Diagnostics.Debug.Assert(
                element.Document != null &&
                element.Document.Root != null);

            var ns = element.Document.Root.Name.Namespace;

            Key = (string)element.Attribute("Key");
            SingularName = (string)element.Element(ns + "SingularName");
            PluralName = (string)element.Element(ns + "PluralName");

            var description = (string)element.Element(ns + "Description");
            if (!string.IsNullOrEmpty(description))
                description = TextHelper.TrimParagraphs(description);

            Description = description ?? string.Empty;
            CombatEffectiveness = (double?)element.Element(ns + "CombatEffectiveness") ?? 1.0;

            var homePlanetType = EnumHelper.Parse<PlanetType>((string)element.Element(ns + "HomePlanetType"));
            if (!homePlanetType.HasValue)
                homePlanetType = PlanetType.Terran;

            var habitablePlanetTypes = (string)element.Element(ns + "HabitablePlanetTypes");
            if (habitablePlanetTypes != null)
            {
                var planetTypeNames = habitablePlanetTypes.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                var planetTypes = new List<PlanetType>();

                foreach (var name in planetTypeNames)
                {
                    PlanetType planetType;

                    if (EnumHelper.TryParse(name.Trim(), out planetType))
                        planetTypes.Add(planetType);
                }

                if (planetTypes.Count == 0)
                {
                    GameLog.Client.GameData.WarnFormat(
                        "Race {0} has no valid habitable planets defined.  Falling back to default list.",
                        Name);

                    _habitablePlanetTypes = PlanetTypeFlags.StandardHabitablePlanets;
                }
                else
                {
                    _habitablePlanetTypes = new PlanetTypeFlags(planetTypes);
                }
            }
            else
            {
                _habitablePlanetTypes = PlanetTypeFlags.StandardHabitablePlanets;                
            }

            HomePlanetType = homePlanetType.Value;
        }

        /// <summary>
        /// Appends the XML data for this <see cref="Race"/> to the specified parent element.
        /// </summary>
        /// <param name="parentElement">The parent element.</param>
        public XContainer AppendXml(XElement parentElement)
        {
            var ns = parentElement.Name.Namespace;

            parentElement.Add(
                new XElement(
                    ns + "Race",
                    new XAttribute(
                        "Key",
                        Key),
                    new XElement(
                        ns + "SingularName",
                        SingularName),
                    new XElement(
                        ns + "PluralName",
                        PluralName),
                    new XElement(
                        ns + "Description",
                        string.Join(Environment.NewLine, Description.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))),
                    new XElement(
                        ns + "CombatEffectiveness",
                        CombatEffectiveness),
                    new XElement(
                        ns + "HomePlanetType",
                        HomePlanetType.ToString()),
                    _habitablePlanetTypes != PlanetTypeFlags.StandardHabitablePlanets
                        ? new XElement(
                              "HabitablePlanetTypes",
                              string.Join(", ", EnumUtilities.GetValues<PlanetType>().Where(p => _habitablePlanetTypes[p])))
                        : null
                    ));

            return parentElement;
        }

        #region IEncyclopediaEntry Members
        /// <summary>
        /// Gets the heading displayed in the Encyclopedia index.
        /// </summary>
        /// <value>The heading.</value>
        public string EncyclopediaHeading
        {
            get { return ResourceManager.GetString(PluralName); }
        }

        /// <summary>
        /// Gets the text displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The text.</value>
        public string EncyclopediaText
        {
            get
            {
                var description = Description;

                /*
                   No sense in doing a potentially long string comparison to look up the description text
                   when we already have it.
                 */
                if (description != null &&
                    description.Length > ResourceManager.MaxStringKeyLength)
                {
                    return description;
                }

                return ResourceManager.GetString(description);
            }
        }

        /// <summary>
        /// Gets the path of the image file for this Race.
        /// </summary>
        /// <value>The image file path.</value>
        public string ImagePath
        {
            get
            {
                var searchPaths = new[]
                                  {
                                      "vfs:///Resources/Images/Races/{0}.png",
                                      "vfs:///Resources/Images/Races/{0}.jpg"
                                  };

                foreach (var searchPath in searchPaths)
                {
                    var imagePath = ResourceManager.GetResourcePath(string.Format(searchPath, Key));
                    if (File.Exists(imagePath))
                        return ResourceManager.GetResourceUri(imagePath).ToString();
                }

                return MissingImageUri;
            }
        }

        /// <summary>
        /// Gets the image displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The image.</value>
        public string EncyclopediaImage
        {
            get { return ImagePath; }
        }

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public EncyclopediaCategory EncyclopediaCategory
        {
            get { return EncyclopediaCategory.Races; }
        }

        #endregion

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Key = reader.ReadString();
            SingularName = reader.ReadString();
            PluralName = reader.ReadString();
            Description = reader.ReadString();
            HomePlanetType = (PlanetType)reader.ReadByte();
            CombatEffectiveness = reader.ReadDouble();
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(Key);
            writer.Write(SingularName);
            writer.Write(PluralName);
            writer.Write(Description);
            writer.Write((byte)HomePlanetType);
            writer.Write(CombatEffectiveness);
        }
    }
}
