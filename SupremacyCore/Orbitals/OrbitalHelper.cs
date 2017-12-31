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
            var system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
                return null;

            var colony = system.Colony;
            if (colony == null)
                return null;

            var source = GameContext.Current.Universe.FindAt<OrbitalBattery>(location).Where(o => o.OwnerID == colony.OwnerID);

            if (predicate != null)
                source = source.Where(predicate);

            return source.OrderBy(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindWeakestOrbitalBattery(IEnumerable<OrbitalBattery> batteries, Func<OrbitalBattery, bool> predicate = null)
        {
            var source = (predicate != null) ? batteries.Where(predicate) : batteries;
            return source.OrderBy(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindStrongestOrbitalBattery(MapLocation location, Func<OrbitalBattery, bool> predicate = null)
        {
            var system = GameContext.Current.Universe.Map[location].System;
            if (system == null)
                return null;

            var colony = system.Colony;
            if (colony == null)
                return null;

            var source = GameContext.Current.Universe.FindAt<OrbitalBattery>(location).Where(o => o.OwnerID == colony.OwnerID);

            if (predicate != null)
                source = source.Where(predicate);

            return source.OrderByDescending(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }

        public static OrbitalBattery FindStrongestOrbitalBattery(IEnumerable<OrbitalBattery> batteries, Func<OrbitalBattery, bool> predicate = null)
        {
            var source = (predicate != null) ? batteries.Where(predicate) : batteries;
            return source.OrderByDescending(o => o.HullStrength.CurrentValue).FirstOrDefault();
        }
    }
}