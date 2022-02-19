// ProductionFacilityDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Xml;

using Supremacy.Encyclopedia;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Defines the planetary production categories used in the game.
    /// </summary>
    public enum ProductionCategory
    {
        Food,
        Industry,
        Energy,
        Research,
        Intelligence
    }

    /// <summary>
    /// Represents a production facility design in the game.
    /// </summary>
    [Serializable]
    public class ProductionFacilityDesign : TechObjectDesign
    {
        /// <summary>
        /// Gets or sets the production category.
        /// </summary>
        /// <value>The production category.</value>
        public ProductionCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the labor maintenance cost of a single facility of this design.
        /// </summary>
        /// <value>The labor maintenance cost.</value>
        public int LaborCost { get; set; }

        /// <summary>
        /// Gets or sets the unit output of a facility of this design.
        /// </summary>
        /// <value>The unit output.</value>
        public int UnitOutput { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductionFacilityDesign"/> class.
        /// </summary>
        public ProductionFacilityDesign() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductionFacilityDesign"/> class using XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public ProductionFacilityDesign(XmlElement element) : base(element)
        {
            XmlElement tempElement = element["LaborCost"];
            if (tempElement != null)
            {
                LaborCost = Number.ParseInt32(tempElement.InnerText.Trim());
            }

            tempElement = element["ProductionCategory"];
            if (tempElement != null)
            {
                Category = EnumHelper.Parse<ProductionCategory>(
                    tempElement.InnerText.Trim())
                    ?? default;
            }

            tempElement = element["UnitOutput"];
            if (tempElement != null)
            {
                UnitOutput = Number.ParseInt32(tempElement.InnerText.Trim());
            }
        }

        protected override string DefaultImageSubFolder => "Facilities/";

        /// <summary>
        /// Appends the XML data for this instance.
        /// </summary>
        /// <param name="baseElement">The base XML element.</param>
        protected internal override void AppendXml(XmlElement baseElement)
        {
            base.AppendXml(baseElement);

            XmlDocument doc = baseElement.OwnerDocument;

            XmlElement newElement = doc.CreateElement("LaborCost");
            newElement.InnerText = LaborCost.ToString();
            _ = baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("ProductionCategory");
            newElement.InnerText = Category.ToString();
            _ = baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("UnitOutput");
            newElement.InnerText = UnitOutput.ToString();
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

            Sector sector = GameContext.Current.Universe.Map[location];
            StarSystem system = sector.System;

            system.Colony.SetFacilityType(Category, this);
            system.Colony.AddFacility(Category);
            _ = system.Colony.ActivateFacility(Category);

            spawnedInstance = null;
            return true;
        }

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory => EncyclopediaCategory.Buildings;
    }
}
