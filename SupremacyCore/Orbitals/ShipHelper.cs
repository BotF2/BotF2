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
    }
}
