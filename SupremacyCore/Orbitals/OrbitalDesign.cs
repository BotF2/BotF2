//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xml;

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
        private byte _cloakStrength;
        private byte _camouflagedStrength;
        private byte _sensorRange;
        private string _shipType;
        private WeaponType _primaryWeapon;
        private WeaponType _secondaryWeapon;
        private string _modelFile;
        private byte _maneuverability;

        public byte Maneuverability
        {
            get => _maneuverability;
            set => _maneuverability = (byte)value;
        }

        /// <summary>
        /// Gets or sets the size of the crew.
        /// </summary>
        /// <value>The size of the crew.</value>
        public int CrewSize
        {
            get => _crewSize;
            set => _crewSize = (ushort)Math.Max(0, Math.Min(value, ushort.MaxValue));
        }

        /// <summary>
        /// Gets a value indicating whether the orbital is manned.
        /// </summary>
        /// <value><c>true</c> if the orbital is manned; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// An orbital is manned if it has a crew size greater than zero.
        /// </remarks>
        public bool IsManned => _crewSize > 0;

        /// <summary>
        /// Gets or sets the hull strength.
        /// </summary>
        /// <value>The hull strength.</value>
        public int HullStrength
        {
            get => _hullStrength;
            set => _hullStrength = (ushort)Math.Max(0, Math.Min(value, ushort.MaxValue));
        }

        /// <summary>
        /// Gets or sets the shield strength.
        /// </summary>
        /// <value>The shield strength.</value>
        public int ShieldStrength
        {
            get => _shieldStrength;
            set => _shieldStrength = (ushort)Math.Max(0, Math.Min(value, ushort.MaxValue));
        }

        /// <summary>
        /// Gets or sets the shield recharge rate.
        /// </summary>
        /// <value>The shield recharge rate.</value>
        public Percentage ShieldRechargeRate
        {
            get => _shieldRechargeRate;
            set => _shieldRechargeRate = value;
        }

        /// <summary>
        /// Gets or sets the science ability.
        /// </summary>
        /// <value>The science ability.</value>
        public Percentage ScienceAbility
        {
            get => _scienceAbility;
            set => _scienceAbility = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="OrbitalDesign"/> is combatant.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="OrbitalDesign"/> is combatant; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCombatant => _primaryWeapon.Count + _secondaryWeapon.Count > 0;

        /// <summary>
        /// Gets string ShipType from TechObjectDatabase.xml <see cref="OrbitalDesign"/> ShipType.
        /// </summary>
        /// <value>
        /// <c>int</c>  <see cref="OrbitalDesign"/> ; Transport =2 <c>int</c>.
        /// </value>
        public string ShipType
        {
            get => _shipType;
            set => _shipType = value;
        }
        /// <summary>
        /// Gets or sets the scan strength.
        /// </summary>
        /// <value>The scan strength.</value>
        public int ScanStrength
        {
            get => _scanPower;
            set => _scanPower = (byte)value;
        }

        /// <summary>
        /// Gets or sets the cloak strength.
        /// </summary>
        /// <value>The cloak strength.</value>
        public int CloakStrength
        {
            get => _cloakStrength;
            set => _cloakStrength = (byte)value;
        }

        /// <summary>
        /// Gets or sets the scan strength.
        /// </summary>
        /// <value>The scan strength.</value>
        public int CamouflagedStrength
        {
            get => _camouflagedStrength;
            set => _camouflagedStrength = (byte)value;
        }

        /// <summary>
        /// Gets or sets the sensor range.
        /// </summary>
        /// <value>The sensor range.</value>
        public int SensorRange    // structures included here ?? ... seems so
        {
            get
            {
                if (_sensorRange > 6)  // not more than 6 sectors to all directions
                {
                    _sensorRange = 6;
                }

                return _sensorRange;
            }
            set
            {
                if (_sensorRange > 6)
                {
                    _sensorRange = 6;
                }

                _sensorRange = (byte)value;
            }
        }

        /// <summary>
        /// Gets or sets the primary weapon.
        /// </summary>
        /// <value>The primary weapon.</value>
        public WeaponType PrimaryWeapon
        {
            get => _primaryWeapon;
            set => _primaryWeapon = value;
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
                    string value = LocalizedText.GetString(OrbitalStringKeys.PrimaryWeaponName);
                    if (value == null)
                    {
                        GameLog.Core.XMLCheck.WarnFormat("PrimaryWeaponName is missing... (ship/station is unknown here");
                    }

                    if (value != null)
                    {
                        return value;
                    }
                }
                if (TryEnsureObjectString())
                {
                    return TextDatabaseEntry.Custom1;
                }
                //if (TextDatabaseEntry.Custom1 == null)
                //    GameLog.Core.XMLCheck.WarnFormat("PrimaryWeaponName is missing... (ship/station is unknown here");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the secondary weapon.
        /// </summary>
        /// <value>The secondary weapon.</value>
        public WeaponType SecondaryWeapon
        {
            get => _secondaryWeapon;
            set => _secondaryWeapon = value;
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
                    string value = LocalizedText.GetString(OrbitalStringKeys.SecondaryWeaponName);
                    if (value == null)
                    {
                        GameLog.Core.XMLCheck.WarnFormat("SecondaryWeaponName is missing... (ship/station is unknown here");
                    }

                    if (value != null)
                    {
                        return value;
                    }
                }
                if (TryEnsureObjectString())
                {
                    return TextDatabaseEntry.Custom2;
                }
                //if (TextDatabaseEntry.Custom2 == null)
                //    GameLog.Core.XMLCheck.WarnFormat("SecondaryWeaponName is missing... (ship/station is unknown here");
                return string.Empty;
            }
        }

        public string ModelFile
        {
            get => _modelFile;
            set => _modelFile = value;
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
                if (_crewSize != 0)
                {
                    if (_crewSize < 1 || _crewSize > 65000)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _crewSize should not be {1}", Name, _crewSize);
                    }
                }
            }
            if (element["ScienceAbility"] != null)
            {
                _scienceAbility = Number.ParsePercentage(element["ScienceAbility"].InnerText.Trim());
                //if (_scienceAbility != 0)
                //    if (_scienceAbility < 0.01 || _scienceAbility > 100)   // atm all values between x and x (or 0 for not having this ability)
                //        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _scienceAbility should not be {1}", Name, _scienceAbility);

            }
            if (element["ScanPower"] != null)
            {
                _scanPower = Number.ParseByte(element["ScanPower"].InnerText.Trim());
                if (_scanPower != 0)
                {
                    if (_scanPower < 1 || _scanPower > 255)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _scanPower should not be {1}", Name, _scanPower);
                    }
                }
            }
            if (element["SensorRange"] != null)
            {
                _sensorRange = Number.ParseByte(element["SensorRange"].InnerText.Trim());
                if (_sensorRange != 0)
                {
                    if (_sensorRange < 1 || _sensorRange > 255)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _sensorRange should not be {1}", Name, _sensorRange);
                    }
                }
            }
            if (element["HullStrength"] != null)
            {
                _hullStrength = Number.ParseUInt16(element["HullStrength"].InnerText.Trim());
                if (_hullStrength != 0)
                {
                    if (_hullStrength < 1)   // atm all values > 0
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _hullStrength should not be {1}", Name, _hullStrength);
                    }
                }
            }
            if (element["ShieldStrength"] != null)
            {
                _shieldStrength = Number.ParseUInt16(element["ShieldStrength"].InnerText.Trim());
                if (_shieldStrength != 0)
                {
                    if (_shieldStrength < 1 || _shieldStrength > 65000)   // atm all values between 1 and 10 (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _shieldStrength should not be {1}", Name, _shieldStrength);
                    }
                }
            }
            if (element["ShieldRecharge"] != null)
            {
                _shieldRechargeRate = Number.ParsePercentage(element["ShieldRecharge"].InnerText.Trim());
                if (_shieldRechargeRate != 0)
                {
                    if (_shieldRechargeRate < 0.01 || _shieldRechargeRate > 1)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _shieldRechargeRate should not be {1}", Name, _shieldRechargeRate);
                    }
                }
            }
            if (element["CloakStrength"] != null)
            {
                _cloakStrength = Number.ParseByte(element["CloakStrength"].InnerText.Trim());
                if (_cloakStrength != 0)
                {
                    if (_cloakStrength < 1 || _cloakStrength > 255)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _cloakStrength should not be {1}", Name, _cloakStrength);
                    }
                }
            }
            if (element["CamouflagedStrength"] != null)
            {
                _camouflagedStrength = Number.ParseByte(element["CamouflagedStrength"].InnerText.Trim());
                if (_camouflagedStrength != 0)
                {
                    if (_camouflagedStrength < 1 || _camouflagedStrength > 255)   // atm all values between x and x (or 0 for not having this ability)
                    {
                        GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _camouflagedStrength should not be {1}", Name, _camouflagedStrength);
                    }
                }
            }
            if (element["BeamType"] != null)
            {
                int beamCount = Number.ParseInt32(element["BeamType"].GetAttribute("Count").Trim());
                int beamDamage = Number.ParseInt32(element["BeamType"].GetAttribute("Damage").Trim());
                Percentage beamRefire = Number.ParsePercentage(element["BeamType"].GetAttribute("Refire").Trim());

                if (beamCount == 0)
                {
                    beamDamage = 0;
                    beamRefire = 0;
                }
                if (beamDamage == 0)
                {
                    beamCount = 0;
                }

                _primaryWeapon = new WeaponType
                {
                    DeliveryType = WeaponDeliveryType.Beam,
                    Count = beamCount,
                    Damage = beamDamage,
                    Refire = beamRefire
                };
                if (_primaryWeapon.Damage > 0 && _primaryWeapon.Count < 1)   // atm all values between x and x (or 0 for not having this ability)
                {
                    GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for a beam: Damage = {0}, but Count = {1}", _primaryWeapon.Damage, _primaryWeapon.Count);
                }
                //_primaryWeaponName = element["BeamType"].GetAttribute("Name").Trim();
            }
            if (element["TorpedoType"] != null)
            {
                int torpedoCount = Number.ParseInt32(element["TorpedoType"].GetAttribute("Count").Trim());
                int torpedoDamage = Number.ParseInt32(element["TorpedoType"].GetAttribute("Damage").Trim());

                if (torpedoCount == 0)
                {
                    torpedoDamage = 0;
                }

                if (torpedoDamage == 0)
                {
                    torpedoCount = 0;
                }

                _secondaryWeapon = new WeaponType
                {
                    DeliveryType = WeaponDeliveryType.Torpedo,
                    Count = torpedoCount,
                    Damage = torpedoDamage
                };
                if (_secondaryWeapon.Damage > 0 && _secondaryWeapon.Count < 1)   // atm all values between x and x (or 0 for not having this ability)
                {
                    GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for a torpedo: Damage = {0}, but Count = {1}", _secondaryWeapon.Damage, _secondaryWeapon.Count);

                }

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
            if (element["Maneuverability"] != null)
            {
                _maneuverability = Number.ParseByte(element["Maneuverability"].InnerText.Trim());
                //GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml for {0}: _maneuverability = {1}", Name, _maneuverability);        
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
                _ = baseElement.AppendChild(newElement);
            }

            if (ScienceAbility > 0)
            {
                newElement = doc.CreateElement("ScienceAbility");
                newElement.InnerText = _scienceAbility.ToString();
                _ = baseElement.AppendChild(newElement);

                if (_scienceAbility < 0 || _scienceAbility > 100)   // atm all values between x and x (or 0 for not having this ability)
                {
                    GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml ScienceAbility = {0}, but check for a correct value in percent", _scienceAbility);
                }

                _ = int.TryParse(_scienceAbility.ToString(), out int _scienceAbilityINT);
                if (_scienceAbility != _scienceAbilityINT)  // atm all values between x and x (or 0 for not having this ability)
                {
                    GameLog.Core.GameData.WarnFormat("In TechObjectDatabase.xml ScienceAbility = {0}, but check for a correct value (full numeric) in percent", _scienceAbility);
                }
            }

            if (ScanStrength > 0)
            {
                newElement = doc.CreateElement("ScanPower");
                newElement.InnerText = ScanStrength.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (SensorRange > 0)
            {
                newElement = doc.CreateElement("SensorRange");
                newElement.InnerText = SensorRange.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (HullStrength > 0)
            {
                newElement = doc.CreateElement("HullStrength");
                newElement.InnerText = HullStrength.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (ShieldStrength > 0)
            {
                newElement = doc.CreateElement("ShieldStrength");
                newElement.InnerText = ShieldStrength.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (ShieldRechargeRate > 0)
            {
                newElement = doc.CreateElement("ShieldRecharge");
                newElement.InnerText = _shieldRechargeRate.ToString();
                _ = baseElement.AppendChild(newElement);
            }

            if (PrimaryWeapon.Count > 0)
            {
                newElement = doc.CreateElement("BeamType");
                //newElement.SetAttribute("Name", PrimaryWeaponName);
                newElement.SetAttribute("Count", PrimaryWeapon.Count.ToString());
                newElement.SetAttribute("Damage", PrimaryWeapon.Damage.ToString());
                newElement.SetAttribute("Refire", PrimaryWeapon.Refire.ToString());
                _ = baseElement.AppendChild(newElement);
            }

            if (SecondaryWeapon.Count > 0)
            {
                newElement = doc.CreateElement("TorpedoType");
                //newElement.SetAttribute("Name", SecondaryWeaponName);
                newElement.SetAttribute("Count", SecondaryWeapon.Count.ToString());
                newElement.SetAttribute("Damage", SecondaryWeapon.Damage.ToString());
                _ = baseElement.AppendChild(newElement);
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
            : base(name) { }
    }

    internal class OrbitalStringKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is OrbitalStringKey orbitalKey &&
                destinationType == typeof(MarkupExtension))
            {
                if (context is IValueSerializerContext serializerContext)
                {
                    ValueSerializer typeSerializer = serializerContext.GetValueSerializerFor(typeof(Type));
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
