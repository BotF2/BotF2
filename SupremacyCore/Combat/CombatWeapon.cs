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

        public Meter MaxDamage
        {
            get { return _maxDamage; }
        }

        public bool IsBeamWeapon
        {
            get { return (_weaponType == WeaponDeliveryType.Beam); }
        }

        public bool CanFire
        {
            get { return (_maxDamage.CurrentValue > 0); }
        }

        public void Discharge()
        {
            _maxDamage.CurrentValue = 0;
        }

        public void Recharge()
        {
            if (_maxDamage.IsMaximized)
                return;

            var maxDamage = (_maxDamage.LastValue == _maxDamage.Maximum)
                                ? (int)(_recharge * _maxDamage.Maximum)
                                : _maxDamage.Maximum;

            _maxDamage.Reset(maxDamage);
        }

        public static CombatWeapon CreateBeamWeapon(Orbital orbital)
        {
            var weapon = new CombatWeapon();
            if (orbital.IsCombatant)
            {
                weapon._maxDamage = new Meter(
                    orbital.OrbitalDesign.PrimaryWeapon.Damage,
                    0,
                    orbital.OrbitalDesign.PrimaryWeapon.Damage);
            }
            else
            {
                weapon._maxDamage = new Meter(0, 0);
            }
            return weapon;
        }

        private static CombatWeapon CreateWeapon(WeaponType weaponType)
        {
            var weapon = new CombatWeapon
                         {
                             _weaponType = weaponType.DeliveryType,
                             _maxDamage = new Meter(
                                 weaponType.Damage,
                                 0,
                                 weaponType.Damage),
                             _recharge = weaponType.Refire
                         };

            return weapon;
        }

        public static CombatWeapon[] CreateWeapons(Orbital orbital)
        {
            if (orbital == null)
                throw new ArgumentNullException("orbital");

            var beams = orbital.OrbitalDesign.PrimaryWeapon.Count;
            var torpedoes = orbital.OrbitalDesign.SecondaryWeapon.Count;
            var weapons = new CombatWeapon[beams + torpedoes];

            for (var i = 0; i < beams; i++)
                weapons[i] = CreateWeapon(orbital.OrbitalDesign.PrimaryWeapon);

            for (var i = beams; i < (beams + torpedoes); i++)
                weapons[i] = CreateWeapon(orbital.OrbitalDesign.SecondaryWeapon);

            return weapons;
        }
    }
}

