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
using Supremacy.Utility;

namespace Supremacy.Combat
{

    [Serializable]
    public class  CombatTargetSecondaries //: IEnumerable<CombatTargetTwo>
    {
        private readonly int _combatId;
        private readonly int _ownerId;
        private readonly Dictionary<int, Civilization> _targetSecondaries;

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
            _targetSecondaries = new Dictionary<int, Civilization>();
            _combatId = combatId;
        }

        public void SetTargetTwoCiv(Orbital source, Civilization targetTwo)
        {
            if (source == null)
                GameLog.Core.CombatDetails.DebugFormat("Orbital source null for SetTargetTwoCiv");
                //throw new ArgumentNullException("source");
            _targetSecondaries[source.ObjectID] = targetTwo;
        }

        public void ClearTargetTwo(Orbital source)
        {
            if (source == null)
                return;
            _targetSecondaries.Remove(source.ObjectID);
        
        }

        public void Clear()
        {
            _targetSecondaries.Clear();
           
        }

        public bool IsTargetTwoSet(Orbital source)
        {
            if (source == null)
                return false;
            return _targetSecondaries.ContainsKey(source.ObjectID);
        }

        public Civilization GetTargetTwo(Orbital source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!_targetSecondaries.ContainsKey(source.ObjectID))
            {
                throw new ArgumentException("No target two has been set for the specified source");
            }
            return _targetSecondaries[source.ObjectID];
        }

    }
}