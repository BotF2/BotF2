using System;
using System.Diagnostics;
using System.Xml;

using Supremacy.Buildings;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Tech
{
    [Serializable]
    public abstract class PlanetaryTechObjectDesign : TechObjectDesign
    {
        private const byte BuildLimitScopeMask = 0x03;
        private const byte BuildLimitCountMask = 0xFC;
        private const byte BuildLimitCountOffset = 2;

        private byte _buildLimit;
        private BuildRestriction _restriction;

        protected PlanetaryTechObjectDesign() { }

        protected PlanetaryTechObjectDesign(string key)
            : base(key) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDesign"/> class from XML data.
        /// </summary>
        /// <param name="element">The XML data.</param>
        protected PlanetaryTechObjectDesign(XmlElement element) : base(element)
        {
            XmlElement propertyElement = element["BuildLimit"];

            if (propertyElement != null)
            {
                BuildLimitScope buildLimitScope;

                if (EnumHelper.TryParse(propertyElement.GetAttribute("Scope"), out buildLimitScope))
                    BuildLimitScope = buildLimitScope;

                BuildLimit = Number.ParseInt32(propertyElement.GetAttribute("Value"));
            }

            propertyElement = element["Restrictions"];

            if (propertyElement != null)
            {
                foreach (XmlElement xmlRestriction in propertyElement.GetElementsByTagName("Restriction"))
                {
                    BuildRestriction restriction;

                    string restrictionText = xmlRestriction.InnerText.Trim();

                    if (!EnumHelper.TryParse(restrictionText, out restriction))
                    {
                        GameLog.Core.GameData.WarnFormat(
                            "Invalid build restriction specified for design '{0}': {1}",
                            Key ?? UnknownDesignKey,
                            restrictionText);

                        continue;
                    }

                    _restriction |= restriction;
                }
            }

            propertyElement = element["CaptureResult"];

            if (propertyElement != null)
                EnumHelper.TryParse(propertyElement.InnerText.Trim(), out _captureResult);
        }

        protected internal override void AppendXml(XmlElement baseElement)
        {
            base.AppendXml(baseElement);

            XmlDocument doc = baseElement.OwnerDocument;

            Debug.Assert(doc != null);

            XmlElement newElement;

            if (BuildLimit != 0)
            {
                newElement = doc.CreateElement("BuildLimit");
                newElement.SetAttribute("Scope", BuildLimitScope.ToString());
                newElement.SetAttribute("Value", BuildLimit.ToString(ResourceManager.NeutralCulture));
                baseElement.AppendChild(newElement);
            }

            if (_restriction != BuildRestriction.None)
            {
                newElement = doc.CreateElement("Restrictions");

                foreach (BuildRestriction restriction in EnumUtilities.GetValues<BuildRestriction>())
                {
                    if ((_restriction & restriction) != restriction)
                        continue;

                    XmlElement subElement = doc.CreateElement("Restriction");
                    subElement.InnerText = restriction.ToString();
                    newElement.AppendChild(subElement);
                }

                if (newElement.ChildNodes.Count > 0)
                    baseElement.AppendChild(newElement);
            }

            if (CaptureResult != default(CaptureResult))
            {
                newElement = doc.CreateElement("CaptureResult");
                doc.InnerText = CaptureResult.ToString();
                baseElement.AppendChild(newElement);
            }
        }

        /// <summary>
        /// Gets or sets the build limit for this particular building design.
        /// </summary>
        /// <value>The build limit.</value>
        public int BuildLimit
        {
            get
            {
                if (BuildLimitScope == BuildLimitScope.None)
                    return 0;
                return ((_buildLimit & BuildLimitCountMask) >> BuildLimitCountOffset);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        SR.ArgumentOutOfRangeException_ValueMustBeNonNegative);
                }
                _buildLimit = (byte)((value << BuildLimitCountOffset) | (_buildLimit & BuildLimitScopeMask));
                OnPropertyChanged("BuildLimit");
            }
        }

        /// <summary>
        /// Gets or sets the scope of the build limit (<c>n</c> per Civilization, Galaxy, or Game).
        /// </summary>
        /// <value>The scope of the build limit.</value>
        public BuildLimitScope BuildLimitScope
        {
            get { return (BuildLimitScope)(_buildLimit & BuildLimitScopeMask); }
            set
            {
                _buildLimit = (byte)((byte)value | (_buildLimit & BuildLimitCountMask));
                OnPropertyChanged("BuildLimitScope");
                OnPropertyChanged("BuildLimit");
            }
        }

        /// <summary>
        /// Gets or sets the restriction for constructing a building of this design.
        /// </summary>
        /// <value>The restriction.</value>
        public BuildRestriction Restriction
        {
            get { return _restriction; }
            set { _restriction = value; }
        }

        public bool HasRestriction(BuildRestriction restriction)
        {
            return (_restriction & restriction) == restriction;
        }

        private CaptureResult _captureResult;

        /// <summary>
        /// Gets or sets the capture result for a building of this design.
        /// </summary>
        /// <value>The capture result.</value>
        public virtual CaptureResult CaptureResult
        {
            get { return _captureResult; }
            set { _captureResult = value; }
        }
    }

    /// <summary>
    /// Enumerates the options for what happens to a building when a colony is captured.
    /// </summary>
    public enum CaptureResult : byte
    {
        /// <summary>
        /// The building is captured.
        /// </summary>
        Capture = 0,
        /// <summary>
        /// The building is destroyed.
        /// </summary>
        Destroy
    }

    /// <summary>
    /// Defines flags representing the conditions that must be met for building construction.
    /// </summary>
    [Flags]
    public enum BuildRestriction : uint
    {
        None = 0x00000000,
        /// <summary>
        /// Only one instance may be constructed per system.
        /// </summary>
        OnePerSystem = 0x04000000,
        /// <summary>
        /// Build location must be the home system of the inhabiting race.
        /// </summary>
        HomeSystem = 0x00000001,
        /// <summary>
        /// Must be constructed in a native system.
        /// </summary>
        NativeSystem = 0x00000002,
        /// <summary>
        /// Must be constructed in a non-native system.
        /// </summary>
        NonNativeSystem = 0x00000004,
        /// <summary>
        /// Must be constructed on a conquered system.
        /// </summary>
        ConqueredSystem = 0x00000008,
        /// <summary>
        /// Star system must have a White star.
        /// </summary>
        WhiteStar = 0x00000010,
        /// <summary>
        /// Star system must have a Blue star.
        /// </summary>
        BlueStar = 0x00000020,
        /// <summary>
        /// Star system must have a Green star.
        /// </summary>
        GreenStar = 0x00000040,
        /// <summary>
        /// Star system must have a Yellow star.
        /// </summary>
        YellowStar = 0x00000080,
        /// <summary>
        /// Star system must have a Orange star.
        /// </summary>
        OrangeStar = 0x00000100,
        /// <summary>
        /// Star system must have a Red star.
        /// </summary>
        RedStar = 0x00000200,
        /// <summary>
        /// Star system must contain an Arctic planet.
        /// </summary>
        ArcticPlanet = 0x00000400,
        /// <summary>
        /// Star system must contain Asteroids.
        /// </summary>
        Asteroids = 0x00000800,
        /// <summary>
        /// Star system must contain a Barren planet.
        /// </summary>
        BarrenPlanet = 0x00001000,
        /// <summary>
        /// Star system must contain a Crystalline planet.
        /// </summary>
        CrystallinePlanet = 0x00002000,
        /// <summary>
        /// Star system must contain a Demon planet.
        /// </summary>
        DemonPlanet = 0x00004000,
        /// <summary>
        /// Star system must contain a Desert planet.
        /// </summary>
        DesertPlanet = 0x00008000,
        /// <summary>
        /// Star system must contain a Gas Giant.
        /// </summary>
        GasGiant = 0x00010000,
        /// <summary>
        /// Star system must contain a Jungle planet.
        /// </summary>
        JunglePlanet = 0x00020000,
        /// <summary>
        /// Star system must contain an Oceanic planet.
        /// </summary>
        OceanicPlanet = 0x00040000,
        /// <summary>
        /// Star system must contain a Rogue planet.
        /// </summary>
        RoguePlanet = 0x00080000,
        /// <summary>
        /// Star system must contain a Terran planet.
        /// </summary>
        TerranPlanet = 0x00100000,
        /// <summary>
        /// Star system must contain a Volcanic planet.
        /// </summary>
        VolcanicPlanet = 0x00200000,
        /// <summary>
        /// Only one instance may be built within a civilization.
        /// </summary>
        OnePerEmpire = 0x00400000,
        /// <summary>
        /// A Dilithium source must be present in the star system.
        /// </summary>
        DilithiumBonus = 0x00800000,
        /// <summary>
        /// A Raw Materials source must be present in the star system.
        /// </summary>
        RawMaterialsBonus = 0x01000000,
        /// <summary>
        /// At least one planet in the star system must have moons.
        /// </summary>
        Moons = 0x02000000,
        /// <summary>
        /// Only one can be built per 100 max population units
        /// </summary>
        OnePer100MaxPopUnits = 0x08000000,
        /// <summary>
        /// Must be constructed in a member system.
        /// </summary>
        MemberSystem = 0x10000000,
        /// <summary>
        /// Star system must be within a nebula
        /// </summary>
        Nebula = 0x20000000
    }
}