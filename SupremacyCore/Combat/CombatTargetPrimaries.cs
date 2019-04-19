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
    public enum CombatTargetOne : byte
    {
        Federation = 1,
        Romulans,
        Klingons,
        Cardassians,  
        Dominion,
        TerranEmpire,
        Borg
       
    }

    [Serializable]
    public class  CombatTargetPrimaries : IEnumerable<CombatTargetOne>
    {
        private readonly int _combatId; // will we use this? think so
        private readonly int _ownerId; // will we use this?
        private readonly Dictionary<int, CombatTargetOne> _targetPrimaries;
    
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

        public CombatTargetPrimaries(Civilization owner, int combatId)
 
        {
        if (owner == null)
                throw new ArgumentNullException("owner");

        _ownerId = owner.CivID;
        _targetPrimaries = new Dictionary<int, CombatTargetOne>();
        _combatId = combatId;
        }

        //public void SetTarget(Civilization source, CombatTargetOne targetOne)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    _targetPrimaries[source.CivID] = targetOne;
        //}

        //public void ClearTarget(Civilization source)
        //{
        //    if (source == null)
        //        return;
        //    _targetPrimaries.Remove(source.CivID);
        //}

        //public void Clear()
        //{
        //    _targetPrimaries.Clear();
        //}

        //public bool IsTargetSet(Civilization source)
        //{
        //    if (source == null)
        //        return false;
        //    return _targetPrimaries.ContainsKey(source.CivID);
        //}

        public CombatTargetOne GetTargetOne(Civilization source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (!_targetPrimaries.ContainsKey(source.CivID))
                throw new ArgumentException("No targetOne has been set for the specified source");
            return _targetPrimaries[source.CivID];
        }

        public IEnumerator<CombatTargetOne> GetEnumerator()
        {
            return _targetPrimaries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
