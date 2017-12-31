// IGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections.Generic;

namespace Supremacy.Universe
{
    /// <summary>
    /// Interface implemented by all galaxy layouts.  Provides a mechanism for
    /// calculating the positions of stars in the galaxy.
    /// </summary>
    public interface IGalaxyLayout
    {
        /// <summary>
        /// Calculates the star positions for a galaxy map and places them in the
        /// <paramref name="positions"/> parameter.
        /// </summary>
        /// <param name="positions">The star positions.</param>
        /// <param name="number">The number of stars to place.</param>
        /// <param name="width">The width of the map.</param>
        /// <param name="height">The height of the map.</param>
        /// <returns>The number of positions added to <paramref name="positions"/>.</returns>
        int GetStarPositions(
            out ICollection<MapLocation> positions,
            int number,
            int width,
            int height);
    }
}
