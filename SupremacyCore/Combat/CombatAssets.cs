// File:CombatAssets.cs
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
        public CombatAssets(Civilization owner, MapLocation location) : this(-1, owner, location) { }

        public CombatAssets(int combatId, Civilization owner, MapLocation location)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            CombatID = combatId;
            OwnerID = owner.CivID;
            Location = location;
            CombatShips = new List<CombatUnit>();
            NonCombatShips = new List<CombatUnit>();
            EscapedShips = new List<CombatUnit>();
            DestroyedShips = new List<CombatUnit>();
            AssimilatedShips = new List<CombatUnit>();
        }
        public CombatAssets()
        {
        }
        public int CombatID { get; internal set; }

        public int OwnerID { get; }

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public MapLocation Location { get; }

        public Sector Sector => GameContext.Current.Universe.Map[Location];

        public List<CombatUnit> CombatShips { get; }

        public List<CombatUnit> NonCombatShips { get; }

        public List<CombatUnit> EscapedShips { get; }

        public List<CombatUnit> DestroyedShips { get; }

        public List<CombatUnit> AssimilatedShips { get; }

        public CombatUnit Station { get; set; }

        public bool IsTransport
        {
            get
            {
                if (CombatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Transport"))
                {
                    return true;
                }

                if (NonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Transport"))
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
                if (CombatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Spy"))
                {
                    return true;
                }

                if (NonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Spy"))
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
                if (CombatShips.Any(cs => cs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                if (NonCombatShips.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                return false;
            }
        }

        public bool HasSurvivingAssets => CombatShips.Count > 0 || NonCombatShips.Count > 0 || (Station?.IsDestroyed == false);
        public bool HasEscapedAssets => EscapedShips.Count > 0;
        public void UpdateAllSources()
        {
            foreach (CombatUnit shipStats in CombatShips)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in NonCombatShips)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in EscapedShips)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in DestroyedShips)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in AssimilatedShips)
            {
                shipStats.UpdateSource();
            }

            Station?.UpdateSource();
        }

        public static bool operator ==(CombatAssets a, CombatAssets b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if ((a is null) || (b is null))
            {
                return false;
            }

            return (a.CombatID == b.CombatID)
                    && (a.OwnerID == b.OwnerID);
        }

        public static bool operator !=(CombatAssets a, CombatAssets b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if ((a is null) || (b is null))
            {
                return true;
            }

            return (a.CombatID != b.CombatID)
                    || (a.OwnerID != b.OwnerID);
        }

        public bool Equals(CombatAssets other)
        {
            if (other == null)
            {
                return false;
            }

            return (CombatID == other.CombatID) && (OwnerID == other.OwnerID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as CombatAssets);
        }

        public override int GetHashCode()
        {
            return CombatID + (29 * OwnerID);
        }
    }
}
