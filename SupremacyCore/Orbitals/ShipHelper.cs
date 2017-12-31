// ShipHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Helper class containing logic related to the <see cref="Ship"/> type.
    /// </summary>
    public static class ShipHelper
    {
        public static bool IsInDistress(this Ship ship)
        {
            if (ship == null)
                throw new ArgumentNullException("ship");

            /*
             * TODO: Determine other circumstances under which a ship should be
             *       considered 'in distress' and update this method accordingly.
             */
            if (ship.IsStranded && !ship.Fleet.IsInTow)
                return true;

            return false;
        }

        /// <summary>
        /// Computes a modifier that based on a ship's current crew level with
        /// respect to its full crew level.  The resulting modifier can be
        /// multiplied against various ship capabilities such that those
        /// capabilities will be reduced when the ship's crew complement is
        /// less than optimal.
        /// </summary>
        /// <param name="ship">
        /// The Ship for which a crew size modifier should be computed
        /// </param>
        /// <returns>Crew size modifier</returns>
        /// <remarks>
        /// The algorithm used to compute the modifier is logarithmic, so
        /// efficiency suffers the most when only a skeleton crew is
        /// available.  Example values are provided below:
        /// <list design="bullet">
        ///   <item>1.000 for a crew at 100% capacity</item>
        ///   <item>0.889 for a crew at 75% capacity</item>
        ///   <item>0.740 for a crew at 50% capacity</item>
        ///   <item>0.512 for a crew at 25% capacity</item>
        ///   <item>0.000 for a crew at 0% capacity</item>
        /// </list>
        /// </remarks>
        public static double GetCrewSizeModifier(this Ship ship)
        {
            return Math.Log10((9 * (ship.Crew.PercentFilled * 10)) - 1);
        }

        /// <summary>
        /// Computes a damage control modifier based on a ship's crew
        /// experience level.  The resulting value should be multiplied
        /// against the projected hull damage to yield the actual hull
        /// damage.
        /// </summary>
        /// <param name="ship">
        /// Ship for which a damage control modifier should be computed
        /// </param>
        /// <returns>Damage control modifier</returns>
        public static double GetDamageControlModifier(this Ship ship)
        {
            var dcmTable = GameContext.Current.Tables.ShipTables["DamageControlModifiers"];
            if (dcmTable != null)
            {
                if (dcmTable[ship.ExperienceRank.ToString()] != null)
                {
                    double modifier;
                    if (Double.TryParse(dcmTable[ship.ExperienceRank.ToString()][0], out modifier))
                    {
                        return modifier;
                    }
                }
            }
            return 1.0;
        }

        /// <summary>
        /// Computes a weapons accuracy modifier based on a ship's crew
        /// experience level.  The resulting value should be multiplied
        /// against any other computed accuracy value.
        /// </summary>
        /// <param name="ship">
        /// Ship for which a weapons accuracy modifier should be computed
        /// </param>
        /// <returns>Weapons accuracy modifier</returns>
        public static double GetAccuracyModifier(this Ship ship)
        {
            var accuracyTable = GameContext.Current.Tables.ShipTables["AccuracyModifiers"];
            if (accuracyTable != null)
            {
                if (accuracyTable[ship.ExperienceRank.ToString()] != null)
                {
                    double modifier;
                    if (Double.TryParse(accuracyTable[ship.ExperienceRank.ToString()][0], out modifier))
                    {
                        return modifier;
                    }
                }
            }
            return 1.0;
        }
    }
}
