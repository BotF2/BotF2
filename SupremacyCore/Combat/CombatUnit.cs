// CombatUnit.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    [Serializable]
    public class CombatUnit : IEquatable<CombatUnit>
    {
        private readonly int _sourceId;
        private int _hullStrength;
        private int _shieldStrength;
        private bool _isCloaked;
        private bool _isCamouflaged;

        //protected CombatUnit(System.Collections.Generic.IEnumerable<Ship> ship) { }

        public CombatUnit()
        {
        }
        public CombatUnit(Orbital source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Ship ship = source as Ship;
            if (ship != null)
            {
                _isCloaked = ship.IsCloaked;
                _isCamouflaged = ship.IsCamouflaged;
                IsAssimilated = ship.IsAssimilated;
                if (source.CloakStrength != null)
                {
                    CloakStrength = source.CloakStrength.CurrentValue;
                }

                if (source.CamouflagedMeter != null)
                {
                    CamouflagedStrength = source.CamouflagedMeter.CurrentValue;
                }
            }
            _sourceId = source.ObjectID;
            OwnerID = source.OwnerID;
            _hullStrength = source.HullStrength.CurrentValue;
            Firepower = (source.OrbitalDesign.PrimaryWeapon.Damage * source.OrbitalDesign.PrimaryWeapon.Count) + (source.OrbitalDesign.SecondaryWeapon.Damage * source.OrbitalDesign.SecondaryWeapon.Count);
            RemainingFirepower = Firepower;
            _shieldStrength = source.ShieldStrength.CurrentValue;
            Name = source.Name;
            Accuracy = source.GetAccuracyModifier();
            DamageControl = source.GetDamageControlModifier();
        }

        public Orbital Source => GameContext.Current.Universe.Get<Orbital>(_sourceId);

        public string Name { get; }

        public string Description
        {
            get
            {
                //GameLog.Client.GameData.DebugFormat("CombatUnit.cs: : Description.GetString");

                if (Source is Station)
                {
                    return ResourceManager.GetString("COMBAT_DESCRIPTION_STATION");
                }
                if (Source is Ship ship)
                {
                    return string.Format(
                        ResourceManager.GetString("COMBAT_DESCRIPTION_SHIP"),
                        ship.ShipDesign.ClassName,
                        GameContext.Current.Tables.EnumTables["ShipType"][ship.ShipType.ToString()][0]);
                }
                return null;
            }
        }

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public int OwnerID { get; }

        public int Firepower { get; }

        public int RemainingFirepower { get; set; }

        public double Accuracy { get; }

        public double DamageControl { get; }
        public int HullStrength => _hullStrength;

        public Percentage HullIntegrity
        {
            get
            {
                if (HullStrength == 0)
                {
                    return 0;
                }
                else
                {
                    return (float)HullStrength / Source.OrbitalDesign.HullStrength;
                }
            }
        }

        public int ShieldStrength => _shieldStrength;

        public Percentage ShieldIntegrity
        {
            get
            {
                if (ShieldStrength == 0)
                {
                    return 0;
                }
                else
                {
                    GameLog.Client.Test.DebugFormat("ShieldStrenth crash owner ={0} {1} {2}", Source.Owner.Key, Source.Design.Key, Source.ShieldStrength.CurrentValue);
                    { return (float)ShieldStrength / Source.OrbitalDesign.ShieldStrength; }
                }
            }
        }

        public bool IsCloaked => _isCloaked;

        public int CloakStrength { get; }

        public bool IsCamouflaged => _isCamouflaged;

        public int CamouflagedStrength { get; }

        public int ScanStrength { get; }

        public bool IsDestroyed => HullStrength <= 0;

        public bool IsAssimilated { get; set; }

        public bool IsMobile => Source.IsMobile;

        public void Cloak()
        {
            _isCloaked = true;
        }
        public void Camouflage()
        {
            _isCamouflaged = true;
        }

        public void Decloak()
        {
            _isCloaked = false;
        }

        public void Decamouflage()
        {
            _isCamouflaged = false;
        }

        public void TakeDamage(int damage)
        {
            int remainingDamage = damage;
            if (_shieldStrength > 0)
            {
                remainingDamage = Math.Max(0, remainingDamage - _shieldStrength);
                _shieldStrength = Math.Max(0, _shieldStrength - damage);
            }
            _hullStrength = Math.Max(0, _hullStrength - remainingDamage);
        }

        public void UpdateSource()
        {
            Orbital source = Source;
            if (source == null)
            {
                return;
            }

            source.ShieldStrength.Reset(ShieldStrength);
            source.HullStrength.Reset(HullStrength);

            if (!source.HullStrength.IsMinimized)
            {
                source.RegenerateShields();
            }
        }

        public static bool operator ==(CombatUnit left, CombatUnit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CombatUnit left, CombatUnit right)
        {
            return !Equals(left, right);
        }

        public bool Equals(CombatUnit other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return _sourceId == other._sourceId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CombatUnit);
        }

        public override int GetHashCode()
        {
            return _sourceId;
        }
    }
}

