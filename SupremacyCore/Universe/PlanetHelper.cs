// PlanetHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;

namespace Supremacy.Universe
{
    public static class PlanetHelper
    {
        private static readonly int HabitablePlanetTypesMask;
        private static readonly int HabitablePlanetSizesMask;

        static PlanetHelper()
        {
            int habitableTypes = 0;
            int habitableSizes = 0;

            foreach (PlanetType planetType in EnumHelper.GetValues<PlanetType>())
            {
                if (!planetType.MatchAttribute(UninhabitableAttribute.Default))
                    habitableTypes |= 1 << (int)planetType;
            }

            foreach (PlanetSize planetSize in EnumHelper.GetValues<PlanetSize>())
            {
                if (!planetSize.MatchAttribute(UninhabitableAttribute.Default))
                    habitableSizes |= 1 << (int)planetSize;
            }

            HabitablePlanetTypesMask = habitableTypes;
            HabitablePlanetSizesMask = habitableSizes;
        }

        public static bool IsGaseous(this PlanetType planetType)
        {
            switch (planetType)
            {
                case PlanetType.GasGiant:
                case PlanetType.Crystalline:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsHabitable(this PlanetType planetType)
        {
            int mask = 1 << (int)planetType;
            return (HabitablePlanetTypesMask & mask) == mask;
        }

        public static bool IsHabitable(this PlanetSize planetSize)
        {
            int mask = 1 << (int)planetSize;
            return (HabitablePlanetSizesMask & mask) == mask;
        }

        public static bool IsHabitable(PlanetType planetType, PlanetSize planetSize)
        {
            return (IsHabitable(planetType) && IsHabitable(planetSize));
        }
    }
}
