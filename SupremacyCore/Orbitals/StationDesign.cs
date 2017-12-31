// StationDesign.cs
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

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Represents a space station design in the game.
    /// </summary>
    [Serializable]
    public class StationDesign : OrbitalDesign
    {
        private byte _buildSlots;
        private ushort _buildOutput;

        /// <summary>
        /// Gets or sets the build slots.
        /// </summary>
        /// <value>The build slots.</value>
        public int BuildSlots
        {
            get { return _buildSlots; }
            set { _buildSlots = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the build output.
        /// </summary>
        /// <value>The build output.</value>
        public int BuildOutput
        {
            get { return _buildOutput; }
            set { _buildOutput = (ushort)value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StationDesign"/> class.
        /// </summary>
        public StationDesign()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StationDesign"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public StationDesign(XmlElement element) : base(element)
        {
            if (element["RepairSlots"] != null)
            {
                _buildSlots = Number.ParseByte(element["RepairSlots"].InnerText.Trim());
            }
            if (element["RepairCapacity"] != null)
            {
                _buildOutput = Number.ParseUInt16(element["RepairCapacity"].InnerText.Trim());
            }
        }

        protected override string DefaultImageSubFolder
        {
            get { return "Stations/"; }
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

            newElement = doc.CreateElement("RepairSlots");
            newElement.InnerText = BuildSlots.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("RepairCapacity");
            newElement.InnerText = BuildOutput.ToString();
            baseElement.AppendChild(newElement);
        }

        /// <summary>
        /// Spawns an instance of an object of this design at the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="spawnedInstance"> </param>
        public override bool TrySpawn(MapLocation location, Civilization owner, out TechObject spawnedInstance)
        {
            if (!CanSpawn(location, owner))
            {
                spawnedInstance = null;
                return false;
            }

            var sector = GameContext.Current.Universe.Map[location];
            var sectorOwner = sector.Owner;

            if (sectorOwner != null &&
                sectorOwner != owner)
            {
                Log.ErrorFormat(
                    "{0} cannot spawn {1} at location {2} because that sector is owned by {3}.",
                    owner.Key,
                    Key ?? UnknownDesignKey,
                    location,
                    sectorOwner.Key);

                spawnedInstance = null;
                return false;
            }

            var existingStation = sector.Station;
            if (existingStation != null)
            {
                //GameLog.Print(
                //    "Destroying station {0} in order to spawn {1} at location {2}.",
                //    existingStation.Design.Key,
                //    this.Key ?? UnknownDesignKey,
                //    location);

                GameContext.Current.Universe.Destroy(existingStation);
            }

            var station = new Station(this);
            var civManager = GameContext.Current.CivilizationManagers[owner];

            station.Reset();
            station.Owner = owner;
            station.Location = location;

            GameContext.Current.Universe.Objects.Add(station);

            station.Sector.Station = station;

            civManager.MapData.SetExplored(location, true);
            civManager.MapData.SetScanned(location, true, SensorRange);
            civManager.MapData.UpgradeScanStrength(location, ScanStrength, SensorRange);

            spawnedInstance = station;
            return true;
        }

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory
        {
            get { return EncyclopediaCategory.Stations; }
        }
    }
}
