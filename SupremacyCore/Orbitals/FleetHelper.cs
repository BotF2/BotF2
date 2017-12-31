// FleetHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;
using Supremacy.Universe;

using System.Linq;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Helper class containing logic related to the <see cref="Fleet"/> type.
    /// </summary>
    public static class FleetHelper
    {
        public static bool IsInDistress(this Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            return fleet.Ships.Any(ShipHelper.IsInDistress);
        }

        /// <summary>
        /// Determines whether a <see cref="Fleet"/> is located within the limits of its fuel range.
        /// </summary>
        /// <param name="fleet">The fleet.</param>
        /// <returns>
        /// <c>true</c> if the fleet is within the limits of its fuel range; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFleetInFuelRange(this Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            if (civManager == null)
                return false;

            return (civManager.MapData.GetFuelRange(fleet.Location) <= fleet.Range);
        }

        /// <summary>
        /// Determines whether a sector is within the fuel range of a given fleet.
        /// </summary>
        /// <param name="sector">The sector.</param>
        /// <param name="fleet">The fleet.</param>
        /// <returns>
        /// <c>true</c> if the sector is within fuel range; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSectorWithinFuelRange(Sector sector, Fleet fleet)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");

            if (fleet == null)
                throw new ArgumentNullException("fleet");

            if (!fleet.IsOwned)
                return false;

            var mapData = GameContext.Current.CivilizationManagers[fleet.Owner].MapData;

            return (mapData.GetFuelRange(sector.Location) <= fleet.Range);
        }
    }
}
