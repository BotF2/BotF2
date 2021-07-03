// Weapon.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Types;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Defines the delivery types of weapons used in the game.
    /// </summary>
    public enum WeaponDeliveryType : byte
    {
        /// <summary>
        /// Particle beam weapons
        /// </summary>
        Beam,
        /// <summary>
        /// Energy pulse weapons
        /// </summary>
        Pulse,
        /// <summary>
        /// Projected torpedo weapons
        /// </summary>
        Torpedo,
        /// <summary>
        /// Propelled missile weapons
        /// </summary>
        Missile
    }

    /// <summary>
    /// Defines a weapon type used in the game.
    /// </summary>
    [Serializable]
    public sealed class WeaponType
    {
        private byte _count;
        private Percentage _refire;
        private byte _damage;
        private WeaponDeliveryType _deliveryType;

        /// <summary>
        /// Gets or sets the delivery type.
        /// </summary>
        /// <value>The delivery type.</value>
        public WeaponDeliveryType DeliveryType
        {
            get => _deliveryType;
            set => _deliveryType = value;
        }

        /// <summary>
        /// Gets or sets the number of individual weapons.
        /// </summary>
        /// <value>The number of individual weapons.</value>
        public int Count
        {
            get => _count;
            set => _count = (byte)Math.Min(value, byte.MaxValue);
        }

        /// <summary>
        /// Gets or sets the refire rate.
        /// </summary>
        /// <value>The refire rate.</value>
        public Percentage Refire
        {
            get
            {
                if (DeliveryType == WeaponDeliveryType.Beam)
                {
                    return _refire;
                }

                return 1.0f;
            }
            set => _refire = value;
        }

        /// <summary>
        /// Gets or sets the maximum damage caused by a direct hit.
        /// </summary>
        /// <value>The maximum damage.</value>
        public int Damage
        {
            get => _damage;
            set => _damage = (byte)Math.Min(value, byte.MaxValue);
        }
    }
}
