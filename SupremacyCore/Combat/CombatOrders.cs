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
    public enum CombatOrder : byte
    {
        Engage,
        Retreat,
        Hail,
        Standby,  // used in System Assault, not used in Ship Combat
        LandTroops,
        Rush,
        Transports,
        Formation
        //Assimilate
    }

    public enum AssaultStrategy
    {
        StagedAttack,
        TotalAnnihilation
    }

    [Serializable]
    public class CombatOrders : IEnumerable<CombatOrder>
    {
        private readonly int _combatId;
        private readonly int _ownerId;
        private readonly AssaultStrategy _assaultStrategy;
        private readonly InvasionTargetingStrategy _assaultTargetingStrategy;
        private readonly Dictionary<int, CombatOrder> _orders;

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

        public CombatOrders(Civilization owner, int combatId, AssaultStrategy assaultStrategy = AssaultStrategy.StagedAttack, InvasionTargetingStrategy assaultTargetingStrategy = InvasionTargetingStrategy.Balanced)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;
            _orders = new Dictionary<int, CombatOrder>();
            _combatId = combatId;
            _assaultStrategy = assaultStrategy;
            _assaultTargetingStrategy = assaultTargetingStrategy;
        }

        public AssaultStrategy AssaultStrategy
        {
            get { return _assaultStrategy; }
        }

        public InvasionTargetingStrategy AssaultTargetingStrategy
        {
            get { return _assaultTargetingStrategy; }
        }

        public void SetOrder(Orbital source, CombatOrder order)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _orders[source.ObjectID] = order;
        }

        public void ClearOrder(Orbital source)
        {
            if (source == null)
                return;
            _orders.Remove(source.ObjectID);
        }

        public void Clear()
        {
            _orders.Clear();
        }

        public bool IsOrderSet(Orbital source)
        {
            if (source == null)
                return false;
            return _orders.ContainsKey(source.ObjectID);
        }

        public CombatOrder GetOrder(Orbital source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (!_orders.ContainsKey(source.ObjectID))
                throw new ArgumentException("No order has been set for the specified source");
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
