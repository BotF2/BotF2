// ShipyardDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Xml;

using Supremacy.Entities;
using Supremacy.Encyclopedia;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;

namespace Supremacy.Orbitals
{
    public enum ShipyardOutputType
    {
        Static = 0,
        PopulationRatio,
        IndustryRatio
    };

    /// <summary>
    /// Represents a shipyard design in the game.
    /// </summary>
    [Serializable]
    public class ShipyardDesign : PlanetaryTechObjectDesign
    {
        public const int NoMaxBuildTechLevel = 255;

        private byte _buildSlots;
        private byte _maxBuildTechLevel;
        private ushort _buildSlotEnergyCost;
        private ushort _buildSlotOutput;
        private ushort _buildSlotMaxOutput;
        private ShipyardOutputType _buildSlotOutputType = ShipyardOutputType.Static;

        /// <summary>
        /// Gets or sets the number of build slots.
        /// </summary>
        /// <value>The numer of build slots.</value>
        public int BuildSlots
        {
            get { return _buildSlots; }
            set { _buildSlots = (byte)value; }
        }

        public int BuildSlotEnergyCost
        {
            get { return _buildSlotEnergyCost; }
            set { _buildSlotEnergyCost = (ushort)value; }
        }

        public int BuildSlotOutput
        {
            get { return _buildSlotOutput; }
            set { _buildSlotOutput = (ushort)value; }
        }

        public int BuildSlotMaxOutput
        {
            get { return _buildSlotMaxOutput; }
            set { _buildSlotMaxOutput = (ushort)value; }
        }

        public String BuildSlotOutputString
        {
            get 
            {
                String slotOutputString = "UNDETERMINED SHIPYARD OUTPUT TYPE";
                switch (BuildSlotOutputType)
                {
                    case ShipyardOutputType.Static:
                        slotOutputString = BuildSlotOutput.ToString();
                        break;
                    case ShipyardOutputType.PopulationRatio:
                        slotOutputString = BuildSlotOutput.ToString() + "% Pop";
                        break;
                    case ShipyardOutputType.IndustryRatio:
                        slotOutputString = BuildSlotOutput.ToString() + "% Industry";
                        break;
                }
                return slotOutputString; 
            }
        }

        public ShipyardOutputType BuildSlotOutputType
        {
            get { return _buildSlotOutputType; }
            set { _buildSlotOutputType = value; }
        }

        /// <summary>
        /// Gets the encyclopedia category for a building of this design.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory
        {
            get { return EncyclopediaCategory.Shipyards; }
        }

        /// <summary>
        /// Gets or sets the maximum tech level of ships that can be constructed at a station of this design.
        /// </summary>
        /// <value>The maximum tech level of construction projects.</value>
        public int MaxBuildTechLevel
        {
            get { return _maxBuildTechLevel; }
            set { _maxBuildTechLevel = (byte)value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipyardDesign"/> class.
        /// </summary>
        public ShipyardDesign()
        {
            _maxBuildTechLevel = NoMaxBuildTechLevel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipyardDesign"/> class using XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public ShipyardDesign(XmlElement element)
            : base(element)
        {
            if (element["BuildSlots"] != null)
                _buildSlots = Number.ParseByte(element["BuildSlots"].InnerText.Trim());

            if (element["BuildSlotOutput"] != null)
            {
                _buildSlotOutput = Number.ParseUInt16(element["BuildSlotOutput"].InnerText.Trim());
                _buildSlotMaxOutput = _buildSlotOutput;
            }

            if (element["BuildSlotMaxOutput"] != null)
                _buildSlotMaxOutput = Number.ParseUInt16(element["BuildSlotMaxOutput"].InnerText.Trim());

            if (element["BuildSlotOutputType"] != null)
            {
                String outputType = element["BuildSlotOutputType"].InnerText.Trim().ToUpperInvariant();
                if(outputType.Equals(ShipyardOutputType.Static.ToString().ToUpperInvariant()))
                    _buildSlotOutputType = ShipyardOutputType.Static;
                else if (outputType.Equals(ShipyardOutputType.PopulationRatio.ToString().ToUpperInvariant()))
                    _buildSlotOutputType = ShipyardOutputType.PopulationRatio;
                else if (outputType.Equals(ShipyardOutputType.IndustryRatio.ToString().ToUpperInvariant()))
                    _buildSlotOutputType = ShipyardOutputType.IndustryRatio;
            }

            if (element["BuildSlotEnergyCost"] != null)
                _buildSlotEnergyCost = Number.ParseUInt16(element["BuildSlotEnergyCost"].InnerText.Trim());

            if (element["MaxBuildTechLevel"] != null)
                _maxBuildTechLevel = Number.ParseByte(element["MaxBuildTechLevel"].InnerText.Trim());
            else
                _maxBuildTechLevel = NoMaxBuildTechLevel;
        }

        protected override string DefaultImageSubFolder
        {
            get { return "Shipyards/"; }
        }

        /// <summary>
        /// Appends the XML data for this instance.
        /// </summary>
        /// <param name="baseElement">The base XML element.</param>
        protected internal override void AppendXml(XmlElement baseElement)
        {
            base.AppendXml(baseElement);

            XmlDocument doc = baseElement.OwnerDocument;
            XmlElement newElement;

            newElement = doc.CreateElement("BuildSlots");
            newElement.InnerText = _buildSlots.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("BuildSlotOutput");
            newElement.InnerText = _buildSlotOutput.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("BuildSlotMaxOutput");
            newElement.InnerText = _buildSlotMaxOutput.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("BuildSlotOutputType");
            newElement.InnerText = _buildSlotOutputType.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("BuildSlotEnergyCost");
            newElement.InnerText = _buildSlotEnergyCost.ToString();
            baseElement.AppendChild(newElement);

            if (MaxBuildTechLevel != NoMaxBuildTechLevel)
            {
                newElement = doc.CreateElement("MaxBuildTechLevel");
                newElement.InnerText = _maxBuildTechLevel.ToString();
                baseElement.AppendChild(newElement);
            }
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

            var system = GameContext.Current.Universe.Map[location].System;
            var shipyard = new Shipyard(this);

            shipyard.Reset();
            shipyard.Owner = system.Colony.Owner;
            shipyard.Location = system.Colony.Location;
            
            GameContext.Current.Universe.Objects.Add(shipyard);

            system.Colony.Shipyard = shipyard;

            spawnedInstance = shipyard;
            return true;
        }
    }
}
