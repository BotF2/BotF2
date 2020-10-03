// BuildingDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Xml;

using Supremacy.Economy;
using Supremacy.Effects;
using Supremacy.Encyclopedia;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;

using System.Linq;

using Supremacy.Utility;

namespace Supremacy.Buildings
{
    /// <summary>
    /// Represents a planetary building design.
    /// </summary>
    [Serializable]
    public class BuildingDesign : PlanetaryTechObjectDesign
    {
        public const int MaxEnergyCost = 4000;
        public const int MaxRawMaterials = 1000000;

        private const byte AlwaysOnlineFlag = 0x01;
        private const byte IsPermanentFlag = 0x02;

        private readonly List<Bonus> _bonuses;

        private ushort _energyCost;
        private readonly byte _buildingFlags;

        /// <summary>
        /// Gets a value indicating whether buildings of this design are always online.
        /// </summary>
        /// <value><c>true</c> if always online; otherwise, <c>false</c>.</value>
        public bool AlwaysOnline => (_buildingFlags & AlwaysOnlineFlag) == AlwaysOnlineFlag;

        /// <summary>
        /// Gets a value indicating whether a building is permanent, e.g. cannot be powered down or destroyed.
        /// </summary>
        /// <value><c>true</c> if the building is permanent; otherwise, <c>false</c>.</value>
        public bool IsPermanent => (_buildingFlags & IsPermanentFlag) == IsPermanentFlag;

        /// <summary>
        /// Gets or sets the energy cost to maintain a building of this design.
        /// </summary>
        /// <value>The energy cost.</value>
        public int EnergyCost
        {
            get => _energyCost;
            set => _energyCost = (ushort)Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, value));
        }

        public override CaptureResult CaptureResult
        {
            get => IsPermanent ? CaptureResult.Capture : base.CaptureResult;
            set => base.CaptureResult = value;
        }

        /// <summary>
        /// Gets the bonuses produced by a building of this design.
        /// </summary>
        /// <value>The bonuses.</value>
        public IList<Bonus> Bonuses => _bonuses;

        /// <summary>
        /// Gets the encyclopedia category for a building of this design.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory => EncyclopediaCategory.Buildings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDesign"/> class.
        /// </summary>
        public BuildingDesign()
        {
            _bonuses = new List<Bonus>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDesign"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML data.</param>
        public BuildingDesign(XmlElement element)
            : base(element)
        {
            _bonuses = new List<Bonus>();

            XmlElement propertyElement = element["EnergyCost"];
            if (propertyElement != null)
            {
                EnergyCost = Number.ParseInt32(propertyElement.InnerText.Trim());
            }

            propertyElement = element["AlwaysOnline"];

            if (propertyElement != null && StringHelper.IsTrue(propertyElement.Value))
            {
                _buildingFlags |= AlwaysOnlineFlag;
            }

            propertyElement = element["IsPermanent"];

            if (propertyElement != null && StringHelper.IsTrue(propertyElement.Value))
            {
                _buildingFlags |= IsPermanentFlag;
            }

            propertyElement = element["Bonuses"];

            if (propertyElement == null)
            {
                return;
            }

            foreach (XmlElement xmlBonus in propertyElement.GetElementsByTagName("Bonus"))
            {
                string bonusTypeText = xmlBonus.GetAttribute("Type");

                if (!EnumHelper.TryParse(bonusTypeText, out BonusType bonusType))
                {
                    GameLog.Core.GameData.WarnFormat(
                        "Invalid bonus type specified for design '{0}': {1}",
                        Key ?? UnknownDesignKey,
                        bonusTypeText);

                    continue;
                }

                if (!int.TryParse(xmlBonus.GetAttribute("Amount"), out int bonusAmount))
                {
                    GameLog.Core.GameData.WarnFormat(
                        "Invalid bonus amount specified for bonus type '{0}' on design '{1}': {2}",
                        bonusType,
                        Key ?? UnknownDesignKey,
                        bonusTypeText);

                    continue;
                }

                _bonuses.Add(
                    new Bonus
                    {
                        BonusType = bonusType,
                        Amount = bonusAmount
                    });
            }
        }

        /// <summary>
        /// Gets the bonuses that match the provided bonus types.
        /// </summary>
        /// <param name="bonusTypes">The bonus types.</param>
        /// <returns>The bonuses matching the provided bonus types.</returns>
        public IEnumerable<Bonus> GetBonuses(params BonusType[] bonusTypes)
        {
            return Bonuses.Where(o => bonusTypes.Contains(o.BonusType));
        }

        protected override string DefaultImageSubFolder => "Buildings/";

        /// <summary>
        /// Compacts this instance to reduce serialization footprint.
        /// </summary>
        public override void Compact()
        {
            base.Compact();
            _bonuses.TrimExcess();
        }

        /// <summary>
        /// Appends the XML data for this instance.
        /// </summary>
        /// <param name="baseElement">The base XML element.</param>
        protected internal override void AppendXml(XmlElement baseElement)
        {
            base.AppendXml(baseElement);

            XmlDocument doc = baseElement.OwnerDocument;
            XmlElement newElement = doc.CreateElement("EnergyCost");

            newElement.InnerText = EnergyCost.ToString();
            _ = baseElement.AppendChild(newElement);

            if (_bonuses.Count == 0)
            {
                return;
            }

            newElement = doc.CreateElement("Bonuses");

            foreach (Bonus bonus in _bonuses)
            {
                XmlElement subElement = doc.CreateElement("Bonus");
                subElement.SetAttribute("Type", bonus.BonusType.ToString());
                subElement.SetAttribute("Amount", bonus.Amount.ToString());
                _ = newElement.AppendChild(subElement);
            }

            _ = baseElement.AppendChild(newElement);
        }

        /// <summary>
        /// Spawns an instance of an object of this design at the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="spawnedInstance"> </param>
        public override bool TrySpawn(MapLocation location, Civilization owner, out TechObject spawnedInstance)
        {
            if (!CanSpawn(
                location,
                owner,
                requireSectorOwned: true,
                requireStarSystem: true,
                requireColony: true))
            {
                spawnedInstance = null;
                return false;
            }

            StarSystem system = GameContext.Current.Universe.Map[location].System;
            Building building = new Building(this);

            building.Reset();

            building.Owner = system.Colony.Owner;
            building.Location = system.Colony.Location;

            system.Colony.AddBuilding(building);
            system.Colony.ActivateBuilding(building);

            GameContext.Current.Universe.Objects.Add(building);
            //building.EffectBindingsInternal.AddRange(this.Effects);
            EffectSystem.RegisterEffectSource(building);

            spawnedInstance = building;
            return true;
        }
    }

    /*
     * This class may eventually be used for Xaml serialization purposes.
     * It will enable us to specify constructor arguments when serializing
     * a tech object design to Xaml, thus eliminating the need for default
     * constructors on tech object designs.
     */
    internal class TechObjectDesignConverter : TypeConverter
    {
        private static readonly Type[] ConstructorArgumentTypes = new[] { typeof(string) };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (!(value is TechObjectDesign design))
            {
                return null;
            }

            Type sourceType = value.GetType();

            if (destinationType == typeof(InstanceDescriptor))
            {
                System.Reflection.ConstructorInfo constructor = sourceType.GetConstructor(ConstructorArgumentTypes);
                if (constructor == null)
                {
                    return null;
                }

                return new InstanceDescriptor(
                    constructor,
                    new[] { design.Key },
                    false);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
