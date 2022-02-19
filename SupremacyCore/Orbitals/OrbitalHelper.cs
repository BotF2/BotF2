using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Game;
using Supremacy.Universe;

namespace Supremacy.Orbitals
{
    public static class OrbitalHelper
    {
        public static OrbitalBattery FindWeakestOrbitalBattery(MapLocation location, Func<OrbitalBattery, bool> predicate)
        {
            StarSystem system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
            {
                return null;
            }

            Colony colony = system.Colony;
            if (colony == null)
            {
                return null;
            }

            IEnumerable<OrbitalBattery> source = GameContext.Current.Universe.FindAt<OrbitalBattery>(location).Where(o => o.OwnerID == colony.OwnerID);

            if (predicate != null)
            {
                source = source.Where(predicate);
            }

            return source.OrderBy(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindWeakestOrbitalBattery(IEnumerable<OrbitalBattery> batteries, Func<OrbitalBattery, bool> predicate = null)
        {
            IEnumerable<OrbitalBattery> source = (predicate != null) ? batteries.Where(predicate) : batteries;
            return source.OrderBy(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindStrongestOrbitalBattery(MapLocation location, Func<OrbitalBattery, bool> predicate = null)
        {
            StarSystem system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
            {
                return null;
            }

            Colony colony = system.Colony;
            if (colony == null)
            {
                return null;
            }

            IEnumerable<OrbitalBattery> source = GameContext.Current.Universe.FindAt<OrbitalBattery>(location).Where(o => o.OwnerID == colony.OwnerID);

            if (predicate != null)
            {
                source = source.Where(predicate);
            }

            return source.OrderByDescending(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindStrongestOrbitalBattery(IEnumerable<OrbitalBattery> batteries, Func<OrbitalBattery, bool> predicate = null)
        {
            IEnumerable<OrbitalBattery> source = (predicate != null) ? batteries.Where(predicate) : batteries;
            return source.OrderByDescending(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        /// <summary>
        /// Computes a damage control modifier based on an orbital's
        /// experience level.  The resulting value should be multiplied
        /// against the projected hull damage to yield the actual hull
        /// damage.
        /// </summary>
        /// <param name="orbital">
        /// Orbitalfor which a damage control modifier should be computed
        /// </param>
        /// <returns>Damage control modifier</returns>
        public static double GetDamageControlModifier(this Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            Data.Table dcmTable = GameContext.Current.Tables.GameOptionTables["DamageControlModifiers"];
            if (dcmTable != null)
            {
                if (dcmTable[orbital.ExperienceRank.ToString()] != null)
                {
                    if (double.TryParse(dcmTable[orbital.ExperienceRank.ToString()][0], out double modifier))
                    {
                        return modifier;  // for some reasons the previous way gave a 5 to 10, whyever
                    }
                }
            }
            return 1.0;
        }

        /// <summary>
        /// Computes a weapons accuracy modifier based on an orbital's
        /// experience level.  The resulting value should be multiplied
        /// against any other computed accuracy value.
        /// </summary>
        /// <param name="orbital">
        /// Orbital for which a weapons accuracy modifier should be computed
        /// </param>
        /// <returns>Weapons accuracy modifier</returns>
        public static double GetAccuracyModifier(this Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            double returnModifier = 0.4;
            Data.Table accuracyTable = GameContext.Current.Tables.GameOptionTables["AccuracyModifiers"];
            if (accuracyTable != null)
            {
                if (accuracyTable[orbital.ExperienceRank.ToString()] != null)
                {
                    if (double.TryParse(accuracyTable[orbital.ExperienceRank.ToString()][0], out double modifier))
                    {
                        returnModifier = modifier;
                    }
                }
            }
            return returnModifier;
        }

        public static byte GetManeuverablility(this Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }
            if (orbital.IsMobile)
            {
                byte maneuverablity = orbital.OrbitalDesign.Maneuverability;
                return maneuverablity;
            }
            return 0;

        }

        /// <summary>
        /// Gets the total firepower for this <see cref="Orbital"/>
        /// </summary>
        /// <param name="orbital"></param>
        /// <returns></returns>
        public static int Firepower(this Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            int firepower = 0;
            if (orbital.OrbitalDesign.PrimaryWeapon != null)
            {
                firepower += orbital.OrbitalDesign.PrimaryWeapon.Damage
                              * orbital.OrbitalDesign.PrimaryWeapon.Count;
            }
            if (orbital.OrbitalDesign.SecondaryWeapon != null)
            {
                firepower += orbital.OrbitalDesign.SecondaryWeapon.Damage
                              * orbital.OrbitalDesign.SecondaryWeapon.Count;
            }
            return firepower;
        }

        /// <summary>
        /// Calculates a combat strength for the given orbital
        /// </summary>
        /// <param name="orbital"></param>
        /// <returns></returns>
        public static int EffectiveCombatStrength(this Orbital orbital)
        {
            int effectiveStrength = orbital.EffectiveCombatStrength();
            effectiveStrength *= orbital.ShieldStrength.Maximum
                + orbital.ShieldStrength.CurrentValue
                + orbital.HullStrength.Maximum
                + orbital.HullStrength.CurrentValue;
            effectiveStrength *= (2 * orbital.ShieldStrength.Maximum)
                                  + (2 * orbital.HullStrength.Maximum);
            return effectiveStrength;
        }

        /// <summary>
        /// Gets the total hit points (Hull + Shield) for this <see cref="Orbital"/>
        /// </summary>
        /// <param name="orbital"></param>
        /// <returns></returns>
        public static int TotalHitPoints(this Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            return orbital.HullStrength.CurrentValue + orbital.ShieldStrength.CurrentValue;
        }

        public static int TotalHitPoints(IList<Orbital> orbitals)
        {
            if (orbitals == null)
            {
                throw new ArgumentNullException("orbitals");
            }

            return orbitals.Sum(o => o.TotalHitPoints());
        }
    }
}