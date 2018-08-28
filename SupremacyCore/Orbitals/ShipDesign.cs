// ShipDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Reflection;
using System.Xml;

using Supremacy.Economy;
using Supremacy.Encyclopedia;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using System.Collections.Generic;
using Supremacy.Utility;

namespace Supremacy.Orbitals
{
    [Serializable]
    public class ShipDesign : OrbitalDesign
    {
        private byte _speed;
        private byte _range;
        private byte _fuelCapacity;
        private byte _cloakStrength;
        private byte _camouflagedStrength;
        private byte _maneuverability;
        private byte _evacuationLimit;
        private ushort _workCapacity;
        private Percentage _interceptAbility;
        private Percentage _raidAbility;
        private Dictionary<string, int> _possibleNames;
        private ShipType _shipClass;

        /// <summary>
        /// Gets or sets the ship class name.
        /// </summary>
        /// <value>The ship class name.</value>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ShipDesign"/> is combatant.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="ShipDesign"/> is combatant; otherwise, <c>false</c>.
        /// </value>
        public override bool IsCombatant
        {
            get
            {
                if (base.IsCombatant)
                    return true;
                FieldInfo fieldInfo = typeof(ShipType).GetField(_shipClass.ToString());
                if (fieldInfo != null)
                    return !Attribute.IsDefined(fieldInfo, typeof(NonCombatantAttribute));
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the intercept ability.
        /// </summary>
        /// <value>The intercept ability.</value>
        public Percentage InterceptAbility
        {
            get { return _interceptAbility; }
            set { _interceptAbility = value; }
        }

        /// <summary>
        /// Gets or sets the raid ability.
        /// </summary>
        /// <value>The raid ability.</value>
        public Percentage RaidAbility
        {
            get { return _raidAbility; }
            set { _raidAbility = value; }
        }

        /// <summary>
        /// Gets or sets the evacuation limit.
        /// </summary>
        /// <value>The evacuation limit.</value>
        public int EvacuationLimit
        {
            get { return _evacuationLimit; }
            set { _evacuationLimit = (byte)Math.Max(0, Math.Min(value, Byte.MaxValue)); }
        }

        /// <summary>
        /// Gets or sets the work capacity.
        /// </summary>
        /// <value>The work capacity.</value>
        /// <remarks>
        /// The work capacity value is used for different purposes based on the type of ship.
        /// For a colony ship, it represents the maximum initial population that can be settled
        /// on a new colony; for a transport ship, it represents capacity; etc.
        /// </remarks>
        public int WorkCapacity
        {
            get { return _workCapacity; }
            set { _workCapacity = (ushort)Math.Max(0, Math.Min(value, UInt16.MaxValue)); }
        }

        /// <summary>
        /// Gets or sets the maneuverability rating.
        /// </summary>
        /// <value>The maneuverability rating.</value>
        public int Maneuverability
        {
            get { return _maneuverability; }
            set { _maneuverability = (byte)Math.Max(0, Math.Min(value, Byte.MaxValue)); }
        }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public int Speed
        {
            get { return _speed; }
            set { _speed = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        /// <value>The range.</value>
        public int Range
        {
            get { return _range; }
            set { _range = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the fuel capacity.
        /// </summary>
        /// <value>The fuel capacity.</value>
        public int FuelCapacity
        {
            get { return _fuelCapacity; }
            set { _fuelCapacity = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the cloak strength.
        /// </summary>
        /// <value>The cloak strength.</value>
        public int CloakStrength
        {
            get { return _cloakStrength; }
            set { _cloakStrength = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the camouflaged strength.
        /// </summary>
        /// <value>The camouflaged strength.</value>
        public int CamouflagedStrength
        {
            get { return _camouflagedStrength; }
            set { _camouflagedStrength = (byte)value; }
        }
        /// <summary>
        /// Gets or sets the type of the ship.
        /// </summary>
        /// <value>The type of the ship.</value>
        public new ShipType ShipType
        {
            get { return _shipClass; }
            set { _shipClass = value; }
        }

        /// <summary>
        /// Gets or sets the dilithium cost.
        /// </summary>
        /// <value>The dilithium cost.</value>
        public int Dilithium
        {
            get { return BuildResourceCosts[ResourceType.Dilithium]; }
            set { BuildResourceCosts[ResourceType.Dilithium] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipDesign"/> class.
        /// </summary>
        public ShipDesign() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipDesign"/> class using XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public ShipDesign(XmlElement element) :  base(element)
        {

            _possibleNames = new Dictionary<string, int>();

            if (element["ShipType"] != null)
            {
                _shipClass = (ShipType)Enum.Parse(
                    typeof(ShipType), element["ShipType"].InnerText.Trim());
            }
            if (element["ClassName"] != null)
            {
                ClassName = element["ClassName"].InnerText.Trim();
            }
            if (element["Dilithium"] != null)
            {
                BuildResourceCosts[ResourceType.Dilithium] = 
                    Number.ParseInt32(element["Dilithium"].InnerText.Trim());
            }
            if (element["CloakStrength"] != null)
            {
                _cloakStrength = Number.ParseByte(element["CloakStrength"].InnerText.Trim());
            }
            if (element["CamouflagedStrength"] != null)
            {
                _camouflagedStrength = Number.ParseByte(element["CamouflagedStrength"].InnerText.Trim());
            }
            //if (element["RawMaterials"] != null)
            //{
            //    BuildResourceCosts[ResourceType.RawMaterials] =
            //        ParseInt32(element["RawMaterials"].InnerText.Trim());
            //}
            if (element["Range"] != null)
            {
                _range = Number.ParseByte(element["Range"].InnerText.Trim());
            }
            if (element["Speed"] != null)
            {
                _speed = Number.ParseByte(element["Speed"].InnerText.Trim());
            }
            if (element["FuelReserve"] != null)
            {
                _fuelCapacity = Number.ParseByte(element["FuelReserve"].InnerText.Trim());
//                BuildResourceCosts[ResourceType.Deuterium] = _fuelCapacity;
            }
            if (element["InterceptAbility"] != null)
            {
                _interceptAbility = Number.ParsePercentage(element["InterceptAbility"].InnerText.Trim());
            }
            if (element["RaidAbility"] != null)
            {
                _interceptAbility = Number.ParsePercentage(element["RaidAbility"].InnerText.Trim());
            }
            if (element["Maneuverability"] != null)
            {
                _maneuverability = Number.ParseByte(element["Maneuverability"].InnerText.Trim());
            }
            if (element["EvacuationLimit"] != null)
            {
                _evacuationLimit = Number.ParseByte(element["EvacuationLimit"].InnerText.Trim());
            }
            if (element["WorkCapacity"] != null)
            {
                _workCapacity = Number.ParseUInt16(element["WorkCapacity"].InnerText.Trim());
            }
            if (element["ShipNames"] == null)
            {
                bool _tracePossibleShipNamesSmallOutput = false;
                if (_tracePossibleShipNamesSmallOutput == true)
                    GameLog.Print("ShipNames missing in TechObjectDatabase.xml for {0}", this.Name);
            }
            if (element["ShipNames"] != null)
            {
                bool _tracePossibleShipNamesSmallOutput = false;
                if (_tracePossibleShipNamesSmallOutput == true)
                    GameLog.Print("ShipNames available (see TechObjectDatabase.xml or activate FullOutput in code) for {0}", this.Name);

                foreach (XmlElement name in element["ShipNames"])
                {
                    _possibleNames.Add(name.InnerText.Trim(), 0);

                    bool _tracePossibleShipNamesFullOutput = false;
                    if (_tracePossibleShipNamesFullOutput == true)
                        GameLog.Print("ShipNames - Possible Name for {0} = {1}", this.Name, name.InnerText.Trim());
                }
            }
        }

        protected override string DefaultImageSubFolder
        {
            get { return "Ships/"; }
        }

        protected override string DefaultShipsUnderConstructionSubFolder
        {
            get { return "Ships_Under_Construction/"; }
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

            newElement = doc.CreateElement("ShipType");
            newElement.InnerText = ShipType.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("ClassName");
            newElement.InnerText = ClassName;
            baseElement.AppendChild(newElement);

            if (Dilithium > 0)
            {
                newElement = doc.CreateElement("Dilithium");
                newElement.InnerText = Dilithium.ToString();
                baseElement.AppendChild(newElement);
            }

            if (CloakStrength > 0)
            {
                newElement = doc.CreateElement("CloakStrength");
                newElement.InnerText = CloakStrength.ToString();
                baseElement.AppendChild(newElement);
            }

            if (CamouflagedStrength > 0)
            {
                newElement = doc.CreateElement("CamouflagedStrength");
                newElement.InnerText = CamouflagedStrength.ToString();
                baseElement.AppendChild(newElement);
            }

            newElement = doc.CreateElement("Range");
            newElement.InnerText = Range.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("Speed");
            newElement.InnerText = Speed.ToString();
            baseElement.AppendChild(newElement);

            newElement = doc.CreateElement("FuelReserve");
            newElement.InnerText = FuelCapacity.ToString();
            baseElement.AppendChild(newElement);

            if (InterceptAbility > 0)
            {
                newElement = doc.CreateElement("InterceptAbility");
                newElement.InnerText = _interceptAbility.ToString();
                baseElement.AppendChild(newElement);
            }

            if (RaidAbility > 0)
            {
                newElement = doc.CreateElement("RaidAbility");
                newElement.InnerText = _raidAbility.ToString();
                baseElement.AppendChild(newElement);
            }

            if (Maneuverability > 0)
            {
                newElement = doc.CreateElement("Maneuverability");
                newElement.InnerText = Maneuverability.ToString();
                baseElement.AppendChild(newElement);
            }

            if (EvacuationLimit > 0)
            {
                newElement = doc.CreateElement("EvacuationLimit");
                newElement.InnerText = EvacuationLimit.ToString();
                baseElement.AppendChild(newElement);
            }

            if (WorkCapacity > 0)
            {
                newElement = doc.CreateElement("WorkCapacity");
                newElement.InnerText = WorkCapacity.ToString();
                baseElement.AppendChild(newElement);
            }
            if (_possibleNames.Count > 0)
            {
                newElement = doc.CreateElement("ShipNames");
                foreach (var shipName in _possibleNames)
                {
                    XmlElement nameElement = doc.CreateElement("ShipName");
                    nameElement.InnerText = shipName.Key;
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

            var civManager = GameContext.Current.CivilizationManagers[owner];
            var ship = new Ship(this);

            ship.Owner = owner;
            //If we have any possible names for this ship class, pick one
            if (_possibleNames.Count > 0)
            {
                //Set this to -1 so we can check if we've checked any yet
                int timesUsed = -1;
                string leastUsedName = "";
                foreach (var shipName in _possibleNames)
                {
                    //If we haven't checked, assign this straight to the variables
                    if (timesUsed == -1)
                    {
                        timesUsed = shipName.Value;
                        leastUsedName = shipName.Key;
                    }
                    else
                    {
                        //Check to see if this name has been used less than the one in the variable
                        if (shipName.Value < timesUsed)
                        {
                            timesUsed = shipName.Value;
                            leastUsedName = shipName.Key;
                        }
                    }
                }
                string newShipName = "";
                if (owner.ShipPrefix != null)
                    newShipName = owner.ShipPrefix + " ";
                newShipName = newShipName + leastUsedName;
                if (timesUsed > 0)
                    newShipName = newShipName + " " + Utility.NameSuffixes.GetFromNumber(timesUsed);
                ship.Name = newShipName;
                
                _possibleNames[leastUsedName] = timesUsed + 1;
            }
            ship.Reset();
            ship.Location = location;
            ship.CreateFleet();

            var fuelNeeded = ship.FuelReserve.Maximum - ship.FuelReserve.CurrentValue;
            if (fuelNeeded > 0)
                ship.FuelReserve.AdjustCurrent(civManager.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));

            GameContext.Current.Universe.Objects.Add(ship);
            
            civManager.MapData.SetExplored(location, true);
            civManager.MapData.SetScanned(location, true, SensorRange);
            civManager.MapData.UpgradeScanStrength(location, ScanStrength, SensorRange);

            spawnedInstance = ship;
            return true;
        }

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory
        {
            get { return EncyclopediaCategory.Ships; }
        }
    }
}