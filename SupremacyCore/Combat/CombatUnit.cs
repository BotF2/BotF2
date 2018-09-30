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

namespace Supremacy.Combat
{
    [Serializable]
    public class CombatUnit : IEquatable<CombatUnit>
    {
        private readonly int _sourceId;
        private readonly int _ownerId;
        private int _hullStrength;
        private int _shieldStrength;
        private bool _isCloaked;
        private bool _isCamouflaged;
        private bool _isAssimilated;
        private readonly string _name;
        private int _cloakStrength;
        private int _camouflageStrength;
        private int _scanStrength;

        protected CombatUnit() {}

        public CombatUnit(Orbital source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            
            var ship = source as Ship;
            if (ship != null)
            {
                _isCloaked = ship.IsCloaked;
                _isCamouflaged = ship.IsCamouflaged;
                _isAssimilated = ship.IsAssimilated;
            }
            _sourceId = source.ObjectID;
            _ownerId = source.OwnerID;
            _hullStrength = source.HullStrength.CurrentValue;
            _shieldStrength = source.ShieldStrength.CurrentValue;
            _name = source.Name;
        }

        public Orbital Source
        {
            get { return GameContext.Current.Universe.Get<Orbital>(_sourceId); }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get
            { 
                //GameLog.Client.GameData.DebugFormat("CombatUnit.cs: : Description.GetString");
            
                if (Source is Station)
                {
                    return ResourceManager.GetString("COMBAT_DESCRIPTION_STATION");
                }
                if (Source is Ship)
                {
                    return String.Format(
                        ResourceManager.GetString("COMBAT_DESCRIPTION_SHIP"),
                        ((Ship)Source).ShipDesign.ClassName,
                        GameContext.Current.Tables.EnumTables["ShipType"][((Ship)Source).ShipType.ToString()][0]);
                }
                return null;
            }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public int HullStrength
        {
            get { return _hullStrength; }
        }

        public Percentage HullIntegrity
        {
            get { return ((float)HullStrength / Source.OrbitalDesign.HullStrength); }
        }

        public int ShieldStrength
        {
            get { return _shieldStrength; }
        }

        public Percentage ShieldIntegrity
        {
            get { return ((float)ShieldStrength / Source.OrbitalDesign.ShieldStrength); }
        }

        public bool IsCloaked
        {
            get { return _isCloaked; }
        }

        public int CloakStrength
        {
            get { return _cloakStrength; }
        }

        public bool IsCamouflaged
        {
            get { return _isCamouflaged; }
        }

        public int CamouflageStrength
        {
            get { return _camouflageStrength; }
        }

        public int ScanStrength
        {
            get { return _scanStrength}
        }

        public bool IsDestroyed
        {
            get { return (HullStrength <= 0); }
        }

        public bool IsAssimilated
        {
            get { return _isAssimilated; }
            set {_isAssimilated = value; }
        }

        public bool IsMobile
        {
            get { return Source.IsMobile; }
        }

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
            var remainingDamage = damage;
            if (_shieldStrength > 0)
            {
                remainingDamage = Math.Max(0, remainingDamage - _shieldStrength);
                _shieldStrength = Math.Max(0, _shieldStrength - damage);
            }
            _hullStrength = Math.Max(0, _hullStrength - remainingDamage);
        }

        public void UpdateSource()
        {
            var source = Source;
            if (source == null)
                return;

            source.ShieldStrength.Reset(ShieldStrength);
            source.HullStrength.Reset(HullStrength);

            if (!source.HullStrength.IsMinimized)
                source.RegenerateShields();
        }

        public static bool operator ==(CombatUnit left, CombatUnit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CombatUnit left, CombatUnit right)
        {
            return !Equals(left, right);
        }

        public bool Equals(CombatUnit combatUnit)
        {
            if (ReferenceEquals(combatUnit, this))
                return true;
            if (ReferenceEquals(combatUnit, null))
                return false;
            return (_sourceId == combatUnit._sourceId);
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
