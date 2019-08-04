// StationDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;

using Supremacy.Encyclopedia;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

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
        private Dictionary<string, int> _possibleStationNames;

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
            _possibleStationNames = new Dictionary<string, int>();

            if (element["RepairSlots"] != null)
            {
                _buildSlots = Number.ParseByte(element["RepairSlots"].InnerText.Trim());
            }
            if (element["RepairCapacity"] != null)
            {
                _buildOutput = Number.ParseUInt16(element["RepairCapacity"].InnerText.Trim());
            }
            if (element["StationNames"] == null)
            {
                GameLog.Core.Stations.DebugFormat("StationNames missing in TechObjectDatabase.xml for {0}", Name);
            }
            else
            {
                //GameLog.Core.GameData.DebugFormat("StationNames available (see TechObjectDatabase.xml or activate FullOutput in code) for {0}", Name);

                foreach (XmlElement name in element["StationNames"])
                {
                    _possibleStationNames.Add(name.InnerText.Trim(), 0);
                    //GameLog.Core.GameData.DebugFormat("StationNames - Possible Name for {0} = {1}", Name, name.InnerText.Trim());
                }
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

            if (_possibleStationNames.Count > 0)
            {
                newElement = doc.CreateElement("SationNames");
                foreach (var stationName in _possibleStationNames)
                {
                    XmlElement nameElement = doc.CreateElement("StationName");
                    nameElement.InnerText = stationName.Key;
                    newElement.AppendChild(nameElement);
                }
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
            if (!CanSpawn(location, owner))
            {
                spawnedInstance = null;
                return false;
            }
            var station = new Station(this);
            var sector = GameContext.Current.Universe.Map[location];
            var sectorOwner = sector.Owner;

            if (_possibleStationNames.Count > 0)
            {
                //Set this to -1 so we can check if we've checked any yet
                int timesUsed = -1;
                string leastUsedName = "";
                foreach (var stationName in _possibleStationNames)
                {
                    //If we haven't checked, assign this straight to the variables
                    if (timesUsed == -1)
                    {
                        timesUsed = stationName.Value;
                        leastUsedName = stationName.Key;
                    }
                    else
                    {
                        //Check to see if this name has been used less than the one in the variable
                        if (stationName.Value < timesUsed)
                        {
                            timesUsed = stationName.Value;
                            leastUsedName = stationName.Key;
                        }
                    }
                }
                string newStationName = "";
                if (owner.ShipPrefix != null)
                    newStationName = owner.ShipPrefix + " ";
                newStationName = newStationName + leastUsedName;
                //if (ship.Owner.Key == "BORG")
                //{
                //    newShipName = newShipName + " " + ShipSuffixes.Binary(timesUsed + 1).PadLeft(4, '0');
                //}
                //else
                //{
                    if (timesUsed > 0)
                    {
                        newStationName = newStationName + " " + ShipSuffixes.Alphabetical(timesUsed);
                    }
                //}

                station.Name = newStationName;

                _possibleStationNames[leastUsedName] = timesUsed + 1;
            }
            if (sectorOwner != null &&
                sectorOwner != owner)
            {
                GameLog.Core.General.DebugFormat(
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

            //var station = new Station(this);
            var civManager = GameContext.Current.CivilizationManagers[owner];

            station.Reset();
            station.Owner = owner;
            station.Location = location;

            GameContext.Current.Universe.Objects.Add(station);

            station.Sector.Station = station;

            civManager.MapData.SetExplored(location, true);
            civManager.MapData.SetScanned(location, true, SensorRange);
            civManager.MapData.UpgradeScanStrength(location, ScanStrength, SensorRange);
            GameLog.Core.GameData.DebugFormat("spawnedInstance ={0}, Owner ={1}, Location ={2}" );
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
