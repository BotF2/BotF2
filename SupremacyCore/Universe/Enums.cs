// Enums.cs
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
using System.Windows.Media;
using Supremacy.Resources;


namespace Supremacy.Universe
{
    /// <summary>
    /// Defines the four quadrants of the galaxy.
    /// </summary>
    public enum Quadrant : byte
    {
        Alpha = 0,
        Beta,
        Delta,
        Gamma
    }

    /// <summary>
    /// Defines the star types used in the game.
    /// </summary>
    [TypeConverter(typeof(StarTypeConverter))]
    public enum StarType : byte
    {
        [SupportsPlanets] White = 0,
        [SupportsPlanets] Blue,
        [SupportsPlanets] Yellow,
        [SupportsPlanets] Orange,
        [SupportsPlanets] Red,

        [SupportsPlanets(AllowedTypes = new[] { PlanetType.Rogue, PlanetType.Terran },
                            AllowedSizes = new[] { PlanetSize.Tiny, PlanetSize.Giant },
                            MaxNumberOfPlanets = 1)]  // PlanetSize Giant for Borg
        Nebula,  //  for nebula maximum ONE planet (Borg nebula, Dominion)
        Wormhole,
        NeutronStar,
        RadioPulsar,
        XRayPulsar,
        Quasar,
        BlackHole // other 'star' types
    }
    public class StarTypeConverter : EnumConverter
    {
        public StarTypeConverter()
            : base(typeof(StarType)) { }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(ImageSource).IsAssignableFrom(destinationType))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(ImageSource).IsAssignableFrom(destinationType))
            {
                StarType? starType = value as StarType?;
                if (starType.HasValue)
                {
                    return ResourceManager.GetResourceUri(string.Format("Resources/Images/UI/Stars/Map/{0}.png", starType.Value));
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }



    /// <summary>
    /// Defines the planet types used in the game.
    /// </summary>
    public enum PlanetType : byte
    {
        Barren = 0,
        Volcanic,
        Desert,
        Jungle,
        Terran,
        Oceanic,
        Arctic,
        Rogue,
        [Uninhabitable] Crystalline,
        [Uninhabitable] Demon,
        [Uninhabitable] GasGiant,
        [Uninhabitable] Asteroids
    }

    /// <summary>
    /// Defines the planet sizes used in the game
    /// </summary>
    public enum PlanetSize : byte
    {
        [Uninhabitable] NoWorld = 0,
        Tiny,
        Small,
        Medium,
        Large,
        Giant,
        [Uninhabitable] GasGiant,
        [Uninhabitable] Asteroids
    }

    /// <summary>
    /// Defines the moon sizes used in the game.
    /// </summary>
    [Flags]
    public enum MoonSize : byte
    {
        NoMoon = 0x00,
        Small = 0x01,
        Medium = 0x02,
        Large = 0x03
    }

    /// <summary>
    /// Defines the moon shapes used in the game.
    /// </summary>
    [Flags]
    public enum MoonShape : byte
    {
        Shape1 = 0x00,
        Shape2 = 0x01,
        Shape3 = 0x02,
        Shape4 = 0x03
    }

    /// <summary>
    /// Defines the moon types used in the game.
    /// </summary>
    /// <remarks>
    /// The members of the <see cref="MoonType"/> enumeration are bitwise combinations
    /// of <see cref="MoonSize"/> and <see cref="MoonShape"/> values.  The <see cref="MoonHelper"/>
    /// class provides helper functions for converting between a <see cref="MoonType"/>
    /// value and the corresponding <see cref="MoonSize"/> and <see cref="MoonShape"/> values.
    /// </remarks>
    [Flags]
    public enum MoonType
    {
        NoMoon = 0,
        SmallShape1 = MoonSize.Small | (MoonShape.Shape1 << 2),
        SmallShape2 = MoonSize.Small | (MoonShape.Shape2 << 2),
        SmallShape3 = MoonSize.Small | (MoonShape.Shape3 << 2),
        SmallShape4 = MoonSize.Small | (MoonShape.Shape4 << 2),
        MediumShape1 = MoonSize.Medium | (MoonShape.Shape1 << 2),
        MediumShape2 = MoonSize.Medium | (MoonShape.Shape2 << 2),
        MediumShape3 = MoonSize.Medium | (MoonShape.Shape3 << 2),
        MediumShape4 = MoonSize.Medium | (MoonShape.Shape4 << 2),
        LargeShape1 = MoonSize.Large | (MoonShape.Shape1 << 2),
        LargeShape2 = MoonSize.Large | (MoonShape.Shape2 << 2),
        LargeShape3 = MoonSize.Large | (MoonShape.Shape3 << 2),
        LargeShape4 = MoonSize.Large | (MoonShape.Shape4 << 2)
    }

    /// <summary>
    /// Defines the planet environments used in the game.
    /// </summary>
    public enum PlanetEnvironment : byte
    {
        NoWorld = 0,
        Ideal,
        Comfortable,
        Adequate,
        Marginal,
        Hostile,
        Uninhabitable
    }

    /// <summary>
    /// Defines the system-specific bonuses used in the game.
    /// </summary>
    [Flags]
    public enum SystemBonus : byte
    {
        NoBonus = 0x00,
        Dilithium = 0x01,
        Duranium = 0x02,
        Random = 0x80,
    }

    /// <summary>
    /// Defines the planet-specific bonuses used in the game.
    /// </summary>
    [Flags]
    public enum PlanetBonus : byte
    {
        NoBonus = 0x00,
        Energy = 0x01,
        Food = 0x02,
        Random = 0x80,
    }

    /// <summary>
    /// Defines the restrictions that can be placed on custom bonuses used in the game.
    /// </summary>
    [Flags]
    public enum BonusRestriction : uint
    {
        None = 0x00000000,
        HomeSystem = 0x00000001,
        NativeSystem = 0x00000002,
        NonNativeSystem = 0x00000004,
        ConqueredSystem = 0x00000008,
        WhiteStar = 0x00000010,
        BlueStar = 0x00000020,
        YellowStar = 0x00000080,
        OrangeStar = 0x00000100,
        RedStar = 0x00000200,
        ArcticPlanet = 0x00000400,
        Asteroids = 0x00000800,
        BarrenPlanet = 0x00001000,
        CrystallinePlanet = 0x00002000,
        DemonPlanet = 0x00004000,
        DesertPlanet = 0x00008000,
        GasGiant = 0x00010000,
        JunglePlanet = 0x00020000,
        OceanicPlanet = 0x00040000,
        RoguePlanet = 0x00080000,
        TerranPlanet = 0x00100000,
        VolcanicPlanet = 0x00200000,
    }
}
