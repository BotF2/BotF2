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

    //[NonSerializable]
    public class IntelOrders : IEnumerable<IntelOrder>
    {
        private readonly int _intelId;
        public  Dictionary<int, IntelOrder> _intelOrders; // Dictionary, int key is ownerID & order from enum above this IntelOrders class

        /*Not used:*/
        private readonly int _ownerId;
        //private readonly AssaultStrategy _assaultStrategy;
        //private readonly InvasionTargetingStrategy _assaultTargetingStrategy;


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

        public Dictionary<int, IntelOrder> _localIntelOrders { get; private set; }

        public IntelOrders(Civilization owner, int intelId)
        //AssaultStrategy assaultStrategy = AssaultStrategy.StagedAttack,
        //InvasionTargetingStrategy assaultTargetingStrategy = InvasionTargetingStrategy.Balanced)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;
            _intelOrders = new Dictionary<int, IntelOrder>();
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


        //public void SetIntelOrder(int attackingCivID, IntelOrder _intelOrder)
        public void SetIntelOrders()

        {
            //if (attackingCivID < 0 || attackingCivID > 6)
            //    throw new ArgumentN_localIntelOrdersullException("source");
            //GameLog.Core.CombatDetails.DebugFormat("Set order = {1} for attacker {0}", attackingCivID, _intelOrder.ToString());
            //_intelOrders[attackingCivID] = _intelOrder;
            _intelOrders = _localIntelOrders; /*= _intelOrders;*/
        }

        public void ClearIntelOrder(CivilizationManager source)
        {
            if (source == null)
                return;
            _intelOrders.Remove(source.CivilizationID);
        }

        public void Clear()
        {
            _intelOrders.Clear();
        }

        public bool IsIntelOrderSet(CivilizationManager source)
        {
            if (source == null)
                return false;
            return _intelOrders.ContainsKey(source.CivilizationID);
        }

        public IntelOrder GetIntelOrder(CivilizationManager source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (!_intelOrders.ContainsKey(source.CivilizationID))
                throw new ArgumentException("No order has been set for the specified source");

            GameLog.Core.Intel.DebugFormat("GetCombatOrder source {0}", source.Civilization.Key);

            return _intelOrders[source.CivilizationID];
        }

        public IEnumerator<IntelOrder> GetEnumerator()
        {
            return _intelOrders.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //}


        //public class SetNewIntelOrders
        //{

        //    //public Dictionary<int , string> _local_IntelOrders; // Dictionary, int key is ownerID & order from enum above this IntelOrders class
        //    public SetNewIntelOrders(int attackingCivID, int attackedCivID, string _intelOrder, string _intelOrderBlamed)
        //    {
        //        if (attackingCivID < 0 || attackingCivID > 6)
        //            throw new ArgumentNullException("source");
        //        GameLog.Core.Intel.DebugFormat("******************** NEW: Set order = {0} for attacker {1} VS {2}", _intelOrder.ToString(), attackingCivID, attackedCivID);

        //        //if (IntelHelper._localIntelOrders.Count = 0)
        //        //    IntelHelper._localIntelOrders.
        //        //    else
        //        var _newIntelOrder = new IntelHelper.NewIntelOrders(999,999,"d","e");
        //        //var _newIntelOrder = (0, 0, "F");
        //        _newIntelOrder.AttackedCivID = attackedCivID;
        //        _newIntelOrder.AttackingCivID = attackingCivID;
        //        _newIntelOrder.Intel_Order = _intelOrder;
        //        _newIntelOrder.Intel_Order_Blamed = _intelOrderBlamed;


        //        IntelHelper._local_IntelOrders.Add(_newIntelOrder);// _intelOrder.ToString());

        //        foreach (var item in IntelHelper._local_IntelOrders)
        //        {
        //            GameLog.Core.Intel.DebugFormat("_localIntelOrders: {2} for civ = {0} VS {1}, Blamed = {3}", item.AttackingCivID, item.AttackedCivID, item.Intel_Order, item.Intel_Order_Blamed);
        //        }

        //        //GameContext.Current.CivilizationManagers[attackingCivID].UpdateIntelOrdersGoingToHost(_newIntelOrder);

        //        //_localIntelOrders[attackingCivID] = _intelOrder;
        //    }
        //}
    }
}
