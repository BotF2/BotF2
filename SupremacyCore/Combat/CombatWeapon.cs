// CombatWeapon.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Orbitals;
using Supremacy.Types;

namespace Supremacy.Combat
{
    [Serializable]
    public sealed class CombatWeapon
    {
        private Meter _maxDamage;
        private WeaponDeliveryType _weaponType;
        private Percentage _recharge;

        private CombatWeapon() { }

        public Meter MaxDamage => _maxDamage;

        public bool IsBeamWeapon => _weaponType == WeaponDeliveryType.Beam;

        public bool CanFire => _maxDamage.CurrentValue > 0;

        public void Discharge()
        {
            _maxDamage.CurrentValue = 0;
        }

        public void Recharge()
        {
            if (_maxDamage.IsMaximized)
            {
                return;
            }

            int maxDamage = (_maxDamage.LastValue == _maxDamage.Maximum)
                                ? (int)(_recharge * _maxDamage.Maximum)
                                : _maxDamage.Maximum;

            _maxDamage.Reset(maxDamage);
        }

        public static CombatWeapon CreateBeamWeapon(Orbital orbital)
        {
            CombatWeapon weapon = new CombatWeapon();
            weapon._maxDamage = orbital.IsCombatant
                ? new Meter(
                    orbital.OrbitalDesign.PrimaryWeapon.Damage,
                    0,
                    orbital.OrbitalDesign.PrimaryWeapon.Damage)
                : new Meter(0, 0);
            return weapon;
        }

        private static CombatWeapon CreateWeapon(WeaponType weaponType)
        {
            return new CombatWeapon
            {
                _weaponType = weaponType.DeliveryType,
                _maxDamage = new Meter(
                                 weaponType.Damage,
                                 0,
                                 weaponType.Damage),
                _recharge = weaponType.Refire
            };
        }

        public static CombatWeapon[] CreateWeapons(Orbital orbital)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException(nameof(orbital));
            }

            int beams = orbital.OrbitalDesign.PrimaryWeapon.Count;
            int torpedoes = orbital.OrbitalDesign.SecondaryWeapon.Count;
            CombatWeapon[] weapons = new CombatWeapon[beams + torpedoes];

            for (int i = 0; i < beams; i++)
            {
                weapons[i] = CreateWeapon(orbital.OrbitalDesign.PrimaryWeapon);
            }

            for (int i = beams; i < (beams + torpedoes); i++)
            {
                weapons[i] = CreateWeapon(orbital.OrbitalDesign.SecondaryWeapon);
            }

            return weapons;
        }
    }
}

