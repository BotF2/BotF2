using System;
using System.Xml;

using Supremacy.Entities;
using Supremacy.Encyclopedia;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Orbitals
{
    [Serializable]
    public class OrbitalBatteryDesign : OrbitalDesign
    {
        public OrbitalBatteryDesign() { }

        public OrbitalBatteryDesign(XmlElement element)
            : base(element)
        {
            XmlElement currentElement = element["UnitEnergyCost"];
            if (currentElement != null)
            {
                int unitEnergyCost;
                string unitEnergyCostText = currentElement.InnerText.Trim();

                if (int.TryParse(unitEnergyCostText, out unitEnergyCost))
                {
                    UnitEnergyCost = unitEnergyCost;
                }
                else
                {
                    GameLog.Core.GameData.WarnFormat(
                        "Invalid unit energy cost specified for design '{0}': {1}",
                        Key ?? UnknownDesignKey,
                        unitEnergyCostText);
                }
            }
        }

        public int UnitEnergyCost { get; set; }

        protected override string DefaultImageSubFolder => "OrbitalBatteries/";

        /// <summary>
        /// Gets the encyclopedia category for a building of this design.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        public override EncyclopediaCategory EncyclopediaCategory => EncyclopediaCategory.Batteries;

        protected internal override void AppendXml(XmlElement baseElement)
        {
            base.AppendXml(baseElement);

            XmlDocument doc = baseElement.OwnerDocument;
            XmlElement newElement = doc.CreateElement("UnitEnergyCost");

            newElement.InnerText = UnitEnergyCost.ToString();
            baseElement.AppendChild(newElement);
        }

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

            system.Colony.OrbitalBatteryDesign = this;
            system.Colony.AddOrbitalBatteries(1);
            system.Colony.ActivateOrbitalBattery();

            spawnedInstance = null;
            return true;
        }
    }

    [Serializable]
    public class OrbitalBattery : Orbital
    {
        public OrbitalBattery() { }

        // ReSharper disable RedundantOverridenMember
        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.Write(_isActive);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);
            _isActive = reader.ReadBoolean();
        }
        // ReSharper restore RedundantOverridenMember

        public OrbitalBattery(OrbitalBatteryDesign design)
            : base(design) { }

        public new OrbitalBatteryDesign Design
        {
            get { return (OrbitalBatteryDesign)base.Design; }
            set { base.Design = value; }
        }

        #region IsActive Property

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (Equals(value, _isActive))
                    return;

                _isActive = value;

                OnIsActiveChanged();
            }
        }

        protected virtual void OnIsActiveChanged()
        {
            OnPropertyChanged("IsActive");
        }

        #endregion
    }
}