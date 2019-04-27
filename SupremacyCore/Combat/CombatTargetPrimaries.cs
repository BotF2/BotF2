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
    public enum CombatTargetOne : byte
    {
        //HOLDYOURFIRE = 0,
        FEDERATION,
        TERRANEMPIRE,
        ROMULANS,
        KLINGONS,
        CARDASSIANS,  
        DOMINION,
        BORG
       
    }

    [Serializable]
    public class  CombatTargetPrimaries : IEnumerable<Civilization>
    {
        private readonly int _combatId; // will we use this? think so
        private readonly int _ownerId; // will we use this?
        private readonly Dictionary<int, Civilization> _targetPrimaries;
    
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
            {
                throw new ArgumentNullException("owner");
            }
            _ownerId = owner.CivID;
            _targetPrimaries = new Dictionary<int, Civilization>();
            _combatId = combatId;

            GameLog.Core.Combat.DebugFormat("CombatTargetPrimaries owner = {0}, _combatID = {1}", owner, _combatId);

        }
        public void SetTargetOne(Civilization source, Civilization targetOne)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            _targetPrimaries[source.CivID] = targetOne;

            GameLog.Core.Test.DebugFormat("source short name ={0}, source ={1} CombatTargetOne = {2}", source.ShortName, source, targetOne);
        }

        public void ClearTargetOne(Civilization source)
        {
            if (source == null)
                return;
            _targetPrimaries.Remove(source.CivID);
        }

        public void Clear()
        {
            _targetPrimaries.Clear();
        }

        public bool IsTargetOneSet(Civilization source)
        {
            if (source == null)
                return false;
            return _targetPrimaries.ContainsKey(source.CivID);
        }

        public Civilization GetTargetOne(Civilization source)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!_targetPrimaries.ContainsKey(source.CivID))
            {
                GameLog.Core.Test.DebugFormat("No target One in _targetPrimaries. source short name ={0}, source ={1} CombatTargetOne = {2}",
                    source.ShortName, source);
                throw new ArgumentException("No target one has been set for the specified source");
            }
             return _targetPrimaries[source.CivID];

        }

        public IEnumerator<Civilization> GetEnumerator()
        {
            return _targetPrimaries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
