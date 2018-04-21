// OrbitalDesign.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xml;
using Supremacy.Orbitals;
using Supremacy.Annotations;
using Supremacy.Tech;
using Supremacy.Types;

namespace Supremacy.Orbitals
{
    [Serializable]
    public abstract class OrbitalDesign : TechObjectDesign
    {
        private ushort _crewSize;
        private ushort _hullStrength;
        private ushort _shieldStrength;
        private Percentage _shieldRechargeRate;
        private Percentage _scienceAbility;
        private byte _scanPower;
        private byte _sensorRange;
        private string _shipType;
        private WeaponType _primaryWeapon;
        private WeaponType _secondaryWeapon;
        private string _modelFile;

        /// <summary>
        /// Gets or sets the size of the crew.
        /// </summary>
        /// <value>The size of the crew.</value>
        public int CrewSize
        {
            get { return _crewSize; }
            set { _crewSize = (ushort)Math.Max(0, Math.Min(value, UInt16.MaxValue)); }
        }

        /// <summary>
        /// Gets a value indicating whether the orbital is manned.
        /// </summary>
        /// <value><c>true</c> if the orbital is manned; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// An orbital is manned if it has a crew size greater than zero.
        /// </remarks>
        public bool IsManned
        {
            get { return (_crewSize > 0); }
        }

        /// <summary>
        /// Gets or sets the hull strength.
        /// </summary>
        /// <value>The hull strength.</value>
        public int HullStrength
        {
            get { return _hullStrength; }
            set { _hullStrength = (ushort)Math.Max(0, Math.Min(value, UInt16.MaxValue)); }
        }

        /// <summary>
        /// Gets or sets the shield strength.
        /// </summary>
        /// <value>The shield strength.</value>
        public int ShieldStrength
        {
            get { return _shieldStrength; }
            set { _shieldStrength = (ushort)Math.Max(0, Math.Min(value, UInt16.MaxValue)); }
        }

        /// <summary>
        /// Gets or sets the shield recharge rate.
        /// </summary>
        /// <value>The shield recharge rate.</value>
        public Percentage ShieldRechargeRate
        {
            get { return _shieldRechargeRate; }
            set { _shieldRechargeRate = value; }
        }

        /// <summary>
        /// Gets or sets the science ability.
        /// </summary>
        /// <value>The science ability.</value>
        public Percentage ScienceAbility
        {
            get { return _scienceAbility; }
            set { _scienceAbility = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="OrbitalDesign"/> is combatant.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="OrbitalDesign"/> is combatant; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCombatant
        {
            get
            {
                if (_primaryWeapon.Count > 0)
                    return true;
                if (_secondaryWeapon.Count > 0)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets string ShipType from TechObjectDatabase.xml <see cref="OrbitalDesign"/> ShipType.
        /// </summary>
        /// <value>
        /// <c>int</c>  <see cref="OrbitalDesign"/> ; Transport =2 <c>int</c>.
        /// </value>
        public string ShipType
        { 
            get { return _shipType; }
            set { _shipType = value; } 
        }
        /// <summary>
        /// Gets or sets the scan strength.
        /// </summary>
        /// <value>The scan strength.</value>
        public int ScanStrength
        {
            get { return _scanPower; }
            set { _scanPower = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the sensor range.
        /// </summary>
        /// <value>The sensor range.</value>
        public int SensorRange
        {
            get { return _sensorRange; }
            set { _sensorRange = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the primary weapon.
        /// </summary>
        /// <value>The primary weapon.</value>
        public WeaponType PrimaryWeapon
        {
            get { return _primaryWeapon; }
            set { _primaryWeapon = value; }
        }

        /// <summary>
        /// Gets or sets the name of the primary weapon.
        /// </summary>
        /// <value>The name of the primary weapon.</value>
        public string PrimaryWeaponName
        {
            get
            {
                if (LocalizedText != null)
                {
                    var value = LocalizedText.GetString(OrbitalStringKeys.PrimaryWeaponName);
                    if (value != null)
                        return value;
                }
                if (TryEnsureObjectString())
                    return TextDatabaseEntry.Custom1;
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the secondary weapon.
        /// </summary>
        /// <value>The secondary weapon.</value>
        public WeaponType SecondaryWeapon
        {
            get { return _secondaryWeapon; }
            set { _secondaryWeapon = value; }
        }

        /// <summary>
        /// Gets or sets the name of the secondary weapon.
        /// </summary>
        /// <value>The name of the secondary weapon.</value>
        public string SecondaryWeaponName
        {
            get
            {
                if (LocalizedText != null)
                {
                    var value = LocalizedText.GetString(OrbitalStringKeys.SecondaryWeaponName);
                    if (value != null)
                        return value;
                }
                if (TryEnsureObjectString())
                    return TextDatabaseEntry.Custom2;
                return String.Empty;
            }
        }

        public string ModelFile
        {
            get { return _modelFile; }
            set { _modelFile = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrbitalDesign"/> class.
        /// </summary>
        protected OrbitalDesign()
        {
            _primaryWeapon = new WeaponType();
            _secondaryWeapon = new WeaponType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrbitalDesign"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML element.</param>
        protected OrbitalDesign(XmlElement element) : base(element)
        {
            _primaryWeapon = new WeaponType();
            _secondaryWeapon = new WeaponType();

            if (element["Crew"] != null)
            {
                _crewSize = Number.ParseUInt16(element["Crew"].InnerText.Trim());
                //BuildResourceCosts[ResourceType.Personnel] += _crewSize;
            }
            if (element["ScienceAbility"] != null)
            {
                _scienceAbility = Number.ParsePercentage(element["ScienceAbility"].InnerText.Trim());
            }
            if (element["ScanPower"] != null)
            {
                _scanPower = Number.ParseByte(element["ScanPower"].InnerText.Trim());
            }
            if (element["SensorRange"] != null)
            {
                _sensorRange = Number.ParseByte(element["SensorRange"].InnerText.Trim());
            }
            if (element["HullStrength"] != null)
            {
                _hullStrength = Number.ParseUInt16(element["HullStrength"].InnerText.Trim());
            }
            if (element["ShieldStrength"] != null)
            {
                _shieldStrength = Number.ParseUInt16(element["ShieldStrength"].InnerText.Trim());
            }
            if (element["ShieldRecharge"] != null)
            {
                _shieldRechargeRate = Number.ParsePercentage(element["ShieldRecharge"].InnerText.Trim());
            }
            if (element["BeamType"] != null)
            {
                _primaryWeapon = new WeaponType
                                 {
                                     DeliveryType = WeaponDeliveryType.Beam,
                                     Count = Number.ParseInt32(element["BeamType"].GetAttribute("Count").Trim()),
                                     Damage = Number.ParseInt32(element["BeamType"].GetAttribute("Damage").Trim()),
                                     Refire = Number.ParsePercentage(element["BeamType"].GetAttribute("Refire").Trim())
                                 };
                //_primaryWeaponName = element["BeamType"].GetAttribute("Name").Trim();
            }
            if (element["TorpedoType"] != null)
            {
                _secondaryWeapon = new WeaponType
                                   {
                                       DeliveryType = WeaponDeliveryType.Torpedo,
                                       Count = Number.ParseInt32(element["TorpedoType"].GetAttribute("Count").Trim()),
                                       Damage = Number.ParseInt32(element["TorpedoType"].GetAttribute("Damage").Trim())
                                   };
                //_secondaryWeaponName = element["TorpedoType"].GetAttribute("Name").Trim();
            }

            if (element["ShipType"] != null)
            {
                _shipType = element["ShipType"].InnerText.Trim();
            }

            if (element["ModelFile"] != null)
            {
                _modelFile = element["ModelFile"].InnerText.Trim();
            }
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

            if (CrewSize > 0)
            {
                newElement = doc.CreateElement("Crew");
                newElement.InnerText = CrewSize.ToString();
                baseElement.AppendChild(newElement);
            }

            if (ScienceAbility > 0)
            {
                newElement = doc.CreateElement("ScienceAbility");
                newElement.InnerText = _scienceAbility.ToString();
                baseElement.AppendChild(newElement);
            }

            if (ScanStrength > 0)
            {
                newElement = doc.CreateElement("ScanPower");
                newElement.InnerText = ScanStrength.ToString();
                baseElement.AppendChild(newElement);
            }

            if (SensorRange > 0)
            {
                newElement = doc.CreateElement("SensorRange");
                newElement.InnerText = SensorRange.ToString();
                baseElement.AppendChild(newElement);
            }

            if (HullStrength > 0)
            {
                newElement = doc.CreateElement("HullStrength");
                newElement.InnerText = HullStrength.ToString();
                baseElement.AppendChild(newElement);
            }

            if (ShieldStrength > 0)
            {
                newElement = doc.CreateElement("ShieldStrength");
                newElement.InnerText = ShieldStrength.ToString();
                baseElement.AppendChild(newElement);
            }

            if (ShieldRechargeRate > 0)
            {
                newElement = doc.CreateElement("ShieldRecharge");
                newElement.InnerText = _shieldRechargeRate.ToString();
                baseElement.AppendChild(newElement);
            }

            if (PrimaryWeapon.Count > 0)
            {
                newElement = doc.CreateElement("BeamType");
                //newElement.SetAttribute("Name", PrimaryWeaponName);
                newElement.SetAttribute("Count", PrimaryWeapon.Count.ToString());
                newElement.SetAttribute("Damage", PrimaryWeapon.Damage.ToString());
                newElement.SetAttribute("Refire", PrimaryWeapon.Refire.ToString());
                baseElement.AppendChild(newElement);
            }

            if (SecondaryWeapon.Count > 0)
            {
                newElement = doc.CreateElement("TorpedoType");
                //newElement.SetAttribute("Name", SecondaryWeaponName);
                newElement.SetAttribute("Count", SecondaryWeapon.Count.ToString());
                newElement.SetAttribute("Damage", SecondaryWeapon.Damage.ToString());
                baseElement.AppendChild(newElement);
            }
        }
    }

    public static class OrbitalStringKeys
    {
        public static readonly object PrimaryWeaponName = new OrbitalStringKey("PrimaryWeaponName");
        public static readonly object SecondaryWeaponName = new OrbitalStringKey("SecondaryWeaponName");
    }

    [Serializable]
    [TypeConverter(typeof(OrbitalStringKeyConverter))]
    public class OrbitalStringKey : TechObjectStringKey
    {
        public OrbitalStringKey([NotNull] string name)
            : base(name) {}
    }

    internal class OrbitalStringKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var orbitalKey = value as OrbitalStringKey;
            if (orbitalKey != null &&
                destinationType == typeof(MarkupExtension))
            {
                var serializerContext = context as IValueSerializerContext;
                if (serializerContext != null)
                {
                    var typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
                    if (typeSerializer != null)
                    {
                        return new StaticExtension(
                            typeSerializer.ConvertToString(typeof(OrbitalStringKeys), serializerContext) +
                            "." +
                            orbitalKey.Name);
                    }
                }
                return new StaticExtension
                       {
                           MemberType = typeof(OrbitalStringKeys),
                           Member = orbitalKey.Name
                       };
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
