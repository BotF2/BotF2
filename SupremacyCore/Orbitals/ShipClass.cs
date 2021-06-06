// ShipClass.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Defines the various types of ships used in the game.
    /// </summary>
    public enum ShipType : byte
    {
        [NonCombatant] Colony = 0,
        [NonCombatant] Construction,
        [NonCombatant] Medical,
        [NonCombatant] Transport,
        [NonCombatant] Spy,
        [NonCombatant] Diplomatic,
        Science,
        Scout,
        FastAttack, // Destroyer and Frigate
        Cruiser,
        HeavyCruiser,
        StrikeCruiser,
        Command
    }

    /// <summary>
    /// This attribute, when applied to a member of the <see cref="ShipType"/> enumeration,
    /// indicates that a the ship type is non-combatant.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum | AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class NonCombatantAttribute : Attribute
    {
        /// <summary>
        /// When overridden in a derived class, returns a value that indicates whether this instance equals a specified object.
        /// </summary>
        /// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance of <see cref="NonCombatantAttribute"/>.</param>
        /// <returns>
        /// true if this instance equals <paramref name="obj"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override bool Match(object obj)
        {
            if (obj == null)
                return false;
            if (obj is NonCombatantAttribute)
                return true;
            return false;
        }
    }
}
