// CombatAssets.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    [Serializable]
    public class CombatAssets : IEquatable<CombatAssets>
    {
        private int _combatId;
        private readonly int _ownerId;
        private readonly MapLocation _location;
        private readonly List<CombatUnit> _combatShips;
        private readonly List<CombatUnit> _nonCombatShips;
        private readonly List<CombatUnit> _escapedShips;
        private readonly List<CombatUnit> _destroyedShips;
        private readonly List<CombatUnit> _assimilatedShips;
        private CombatUnit _station;

        public CombatAssets(Civilization owner, MapLocation location) : this(-1, owner, location) { }

        public CombatAssets(int combatId, Civilization owner, MapLocation location)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            _combatId = combatId;
            _ownerId = owner.CivID;
            _location = location;
            _combatShips = new List<CombatUnit>();
            _nonCombatShips = new List<CombatUnit>();
            _escapedShips = new List<CombatUnit>();
            _destroyedShips = new List<CombatUnit>();
            _assimilatedShips = new List<CombatUnit>();
            _station = new CombatUnit();
        }
        public CombatAssets()
        {

        }
        public int CombatID
        {
            get { return _combatId; }
            internal set { _combatId = value; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public MapLocation Location
        {
            get { return _location; }
        }

        public Sector Sector
        {
            get { return GameContext.Current.Universe.Map[_location]; }
        }

        public List<CombatUnit> CombatShips
        {
            get { return _combatShips; }
        }

        public List<CombatUnit> NonCombatShips
        {
            get { return _nonCombatShips; }
        }

        public List<CombatUnit> EscapedShips
        {
            get { return _escapedShips; }
        }

        public List<CombatUnit> DestroyedShips
        {
            get { return _destroyedShips; }
        }

        public List<CombatUnit> AssimilatedShips
        {
            get { return _assimilatedShips; }
        }

        public CombatUnit Station
        {
            get { return _station; }
            set { _station = value; }
        }

        public bool IsTransport
        {
            get
            {
                if (_combatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Transport"))
                {
                    return true;
                }

                if (_nonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Transport"))
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsSpy
        {
            get
            {
                if (_combatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Spy"))
                {
                    return true;
                }

                if (_nonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Spy"))
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsDiplomcatic
        {
            get
            {
                if (_combatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                if (_nonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                return false;
            }
        }


        public bool HasSurvivingAssets
        {
            get { return CombatShips.Any() || NonCombatShips.Any() || ((Station != null) && !Station.IsDestroyed); }
        }
         public bool HasEscapedAssets
        {
            get { return EscapedShips.Any(); }
        }
        public void UpdateAllSources()
        {
            foreach (CombatUnit shipStats in _combatShips)
                shipStats.UpdateSource();
            foreach (CombatUnit shipStats in _nonCombatShips)
                shipStats.UpdateSource();
            foreach (CombatUnit shipStats in _escapedShips)
                shipStats.UpdateSource();
            foreach (CombatUnit shipStats in _destroyedShips)
                shipStats.UpdateSource();
            foreach (CombatUnit shipStats in _assimilatedShips)
                shipStats.UpdateSource();
            if (_station != null)
                _station.UpdateSource();
        }

        public static bool operator ==(CombatAssets a, CombatAssets b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return ((a._combatId == b._combatId)
                    && (a._ownerId == b._ownerId));
        }

        public static bool operator !=(CombatAssets a, CombatAssets b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (((object)a == null) || ((object)b == null))
                return true;
            return ((a._combatId != b._combatId)
                    || (a._ownerId != b._ownerId));
        }

        public bool Equals(CombatAssets combatAssets)
        {
            if (combatAssets == null)
                return false;
            return (_combatId == combatAssets._combatId) && (_ownerId == combatAssets._ownerId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as CombatAssets);
        }

        public override int GetHashCode()
        {
            return _combatId + 29 * _ownerId;
        }
    }
}
