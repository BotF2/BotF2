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
    public enum CombatOrder : byte
    {
        Engage = 0,
        Retreat = 1,
        Hail = 2,
        Standby = 3,  // used in System Assault, not used in Ship Combat
        LandTroops = 4,
        Rush = 5,
        Transports = 6,
        Formation = 7
        //Assimilate might be an order, instead of invade system, for only the borg
    }

    public enum AssaultStrategy
    {
        StagedAttack = 0,
        TotalAnnihilation = 1
    }

    [Serializable]
    public class CombatOrders : IEnumerable<CombatOrder>
    {
        private readonly Dictionary<int, CombatOrder> _orders; // Dictinary, int key is ownerID & combat order from enum

        public int CombatID { get; }

        public int OwnerID { get; }

        public Civilization Owner => GameContext.Current.Civilizations[OwnerID];

        public CombatOrders(Civilization owner, int combatId,
            AssaultStrategy assaultStrategy = AssaultStrategy.StagedAttack,
            InvasionTargetingStrategy assaultTargetingStrategy = InvasionTargetingStrategy.Balanced)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            OwnerID = owner.CivID;
            _orders = new Dictionary<int, CombatOrder>();
            CombatID = combatId;
            AssaultStrategy = assaultStrategy;
            AssaultTargetingStrategy = assaultTargetingStrategy;
        }

        public AssaultStrategy AssaultStrategy { get; }

        public InvasionTargetingStrategy AssaultTargetingStrategy { get; }

        public void SetOrder(Orbital source, CombatOrder order)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            GameLog.Core.CombatDetails.DebugFormat("Set order = {1} for attacker {0}", source.Owner, order.ToString());
            _orders[source.ObjectID] = order;
        }

        public void ClearOrder(Orbital source)
        {
            if (source == null)
            {
                return;
            }

            _orders.Remove(source.ObjectID);
        }

        public void Clear()
        {
            _orders.Clear();
        }

        public bool IsOrderSet(Orbital source)
        {
            if (source == null)
            {
                return false;
            }

            return _orders.ContainsKey(source.ObjectID);
        }

        public CombatOrder GetOrder(Orbital source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!_orders.ContainsKey(source.ObjectID))
            {
                throw new ArgumentException("No order has been set for the specified source");
            }

            // works   GameLog.Core.CombatDetails.DebugFormat("GetCombatOrder source {0}", source.Name);

            return _orders[source.ObjectID];
        }

        public IEnumerator<CombatOrder> GetEnumerator()
        {
            return _orders.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
