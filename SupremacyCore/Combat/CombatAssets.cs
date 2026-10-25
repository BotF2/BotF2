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
            Ships_Combat = new List<CombatUnit>();
            Ships_NonCombat = new List<CombatUnit>();
            Ships_Escaped = new List<CombatUnit>();
            Ships_Destroyed = new List<CombatUnit>();
            Ships_Assimilated = new List<CombatUnit>();
        }
        public CombatAssets()
        {
        }
        public int CombatID { get; internal set; }

        public int OwnerID { get; }

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public MapLocation Location { get; }

        public string All_info_CombatAssets 
        {
            get
            {
                return this.Location + " > " + Owner 
                    + ": CS= " + Ships_Combat.Count
                    + ", NCS= " + Ships_NonCombat.Count
                    + ", KILL= " + this.Ships_Destroyed.Count
                    //+ ", RET= " + this.re.Count
                    + ", ESC= " + this.Ships_Escaped.Count
                    + ", ASSIMI= " + this.Ships_Assimilated.Count
                    + ", STATION= " + Station
                    //+ ", ASSIMI= " + this..Count

                    ;
            } 
        
        }

        public Sector Sector => GameContext.Current.Universe.Map[Location];

        public List<CombatUnit> Ships_Combat { get; }

        public List<CombatUnit> Ships_NonCombat { get; }

        public List<CombatUnit> Ships_Escaped { get; }

        public List<CombatUnit> Ships_Destroyed { get; }

        public List<CombatUnit> Ships_Assimilated { get; }

        public CombatUnit Station { get; set; }

        public bool IsTransport
        {
            get
            {
                    if (Ships_Combat.Any(cs => cs.Source != null && cs.Source.OrbitalDesign.ShipType == "Transport"))
                    {
                        return true;
                    }

                    if (Ships_NonCombat.Any(ncs => ncs.Source != null && ncs.Source.OrbitalDesign.ShipType == "Transport"))
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
                if (Ships_Combat.Any(cs => cs.Source.OrbitalDesign.ShipType == "Spy"))
                {
                    return true;
                }

                if (Ships_NonCombat.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Spy"))
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
                if (Ships_Combat.Any(cs => cs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                if (Ships_NonCombat.Any(ncs => ncs.Source.OrbitalDesign.ShipType == "Diplomatic"))
                {
                    return true;
                }

                return false;
            }
        }

        public bool HasSurvivingAssets => Ships_Combat.Count > 0 || Ships_NonCombat.Count > 0 || (Station?.IsDestroyed == false);
        public bool HasEscapedAssets => Ships_Escaped.Count > 0;
        public void UpdateAllSources()
        {
            foreach (CombatUnit shipStats in Ships_Combat)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in Ships_NonCombat)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in Ships_Escaped)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in Ships_Destroyed)
            {
                shipStats.UpdateSource();
            }

            foreach (CombatUnit shipStats in Ships_Assimilated)
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
