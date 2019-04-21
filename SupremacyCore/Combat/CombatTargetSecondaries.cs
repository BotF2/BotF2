// CombatOrders.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;

namespace Supremacy.Combat
{

    public enum CombatTargetTwo : byte
    {
       
        FEDERATION =0,
        TERRANEMPIRE,
        ROMULANS,
        KLINGONS,
        CARDASSIANS,
        DOMINION,
        BORG
    }


    [Serializable]
    public class  CombatTargetSecondaries : IEnumerable<CombatTargetTwo>
    {
        private readonly int _combatId;
        private readonly int _ownerId;
        private readonly Dictionary<int, CombatTargetTwo> _targetSecondaries;

        public int CombatID
        {
            get { return _combatId; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public CombatTargetSecondaries(Civilization owner, int combatId)
 
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;
            _targetSecondaries = new Dictionary<int, CombatTargetTwo>();
            _combatId = combatId;
        }

        public void SetTargetTwo(Civilization source, CombatTargetTwo targetTwo)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _targetSecondaries[source.CivID] = targetTwo;
        }

        public void ClearTargetTwo(Civilization source)
        {
            if (source == null)
                return;
            _targetSecondaries.Remove(source.CivID);
        }

        public void Clear()
        {
            _targetSecondaries.Clear();
        }

        public bool IsTargetTwoSet(Civilization source)
        {
            if (source == null)
                return false;
            return _targetSecondaries.ContainsKey(source.CivID);
        }

        public CombatTargetTwo GetTargetTwo(Civilization source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!_targetSecondaries.ContainsKey(source.CivID))
            {
                throw new ArgumentException("No target two has been set for the specified source");
            }
            return _targetSecondaries[source.CivID];
        }

        public IEnumerator<CombatTargetTwo> GetEnumerator()
        {
            return _targetSecondaries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
