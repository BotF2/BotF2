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
    public class CombatTargetSecondaries //: IEnumerable<CombatTargetTwo>
    {
        private readonly Dictionary<int, Civilization> _targetSecondaries;

        public int CombatID { get; }

        public int OwnerID { get; }

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public CombatTargetSecondaries(Civilization owner, int combatId)

        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            OwnerID = owner.CivID;
            _targetSecondaries = new Dictionary<int, Civilization>();
            CombatID = combatId;
        }

        public void SetTargetTwoCiv(Orbital source, Civilization targetTwo)
        {
            if (source == null)
                GameLog.Core.CombatDetails.DebugFormat("Orbital source null for SetTargetTwoCiv");
            GameLog.Core.CombatDetails.DebugFormat("for SetTargetTwoCiv source civ attaker {0} and Civilization target Name = {1}", source.Owner, targetTwo);
            _targetSecondaries[source.ObjectID] = targetTwo; // Ditctionary of orbital shooter object id and its civ target
        }

        public void ClearTargetTwo(Orbital source)
        {
            if (source == null)
            {
                return;
            }

            _targetSecondaries.Remove(source.ObjectID);
        }

        public void Clear()
        {
            _targetSecondaries.Clear();
        }

        public bool IsTargetTwoSet(Orbital source)
        {
            if (source == null)
            {
                return false;
            }

            return _targetSecondaries.ContainsKey(source.ObjectID);
        }

        public Civilization GetTargetTwo(Orbital source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!_targetSecondaries.ContainsKey(source.ObjectID))
            {
                _targetSecondaries[source.ObjectID] = CombatHelper.GetDefaultHoldFireCiv();
                //throw new ArgumentException("No target two has been set for the specified source");
            }
            GameLog.Core.CombatDetails.DebugFormat("Orbital name {0} in GetTargetTwo() targeting {1}", source.Name, _targetSecondaries[source.ObjectID]);
            return _targetSecondaries[source.ObjectID];
        }
    }
}
