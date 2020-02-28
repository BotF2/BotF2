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

namespace Supremacy.Intelligence
{
    public enum IntelOrder : byte
    {
        StealCredits,
        StealResearch,
        SabotageFood,
        SabotageEnergy,
        SabotageIndustry
    }

    //public enum AssaultStrategy
    //{
    //    StagedAttack,
    //    TotalAnnihilation
    //}

    [Serializable]
    public class IntelOrders : IEnumerable<IntelOrder>
    {
        private readonly int _intelId;
        private readonly int _ownerId;
        //private readonly AssaultStrategy _assaultStrategy;
        //private readonly InvasionTargetingStrategy _assaultTargetingStrategy;
        private readonly Dictionary<int, IntelOrder> _orders; // Dictinary, int key is ownerID &  order from enum


        public int IntelID
        {
            get { return _intelId; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public Civilization Owner
        {
            get
            {
                return GameContext.Current.Civilizations[_ownerId];
            }
        }

        public IntelOrders(Civilization owner, int intelId)
            //AssaultStrategy assaultStrategy = AssaultStrategy.StagedAttack,
            //InvasionTargetingStrategy assaultTargetingStrategy = InvasionTargetingStrategy.Balanced)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;
            _orders = new Dictionary<int, IntelOrder>();
            _intelId = intelId;
            //_assaultStrategy = assaultStrategy;
            //_assaultTargetingStrategy = assaultTargetingStrategy;
        }

        //public AssaultStrategy AssaultStrategy
        //{
        //    get { return _assaultStrategy; }
        //}

        //public InvasionTargetingStrategy AssaultTargetingStrategy
        //{
        //    get { return _assaultTargetingStrategy; }
        //}

        public void SetOrder(CivilizationManager source, IntelOrder order)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            GameLog.Core.CombatDetails.DebugFormat("Set order = {1} for attacker {0}", source.Civilization.Key, order.ToString());
            _orders[source.CivilizationID] = order;
        }

        public void ClearOrder(CivilizationManager source)
        {
            if (source == null)
                return;
            _orders.Remove(source.CivilizationID);
        }

        public void Clear()
        {
            _orders.Clear();
        }

        public bool IsOrderSet(CivilizationManager source)
        {
            if (source == null)
                return false;
            return _orders.ContainsKey(source.CivilizationID);
        }

        public IntelOrder GetOrder(CivilizationManager source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (!_orders.ContainsKey(source.CivilizationID))
                throw new ArgumentException("No order has been set for the specified source");

            GameLog.Core.Intel.DebugFormat("GetCombatOrder source {0}", source.Civilization.Key);

            return _orders[source.CivilizationID];
        }

        public IEnumerator<IntelOrder> GetEnumerator()
        {
            return _orders.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
