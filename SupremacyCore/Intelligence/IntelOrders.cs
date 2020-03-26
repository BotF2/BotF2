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

    [Serializable]
    public class IntelOrdersStealCredits
    {
        private Civilization _attackingCiv;
        private Civilization _attackedCiv;
        string _blamed;

        public Civilization AttackingCiv
        {
            get { return _attackingCiv; }
            // set { _attackingCiv = value; }
        }
        public Civilization AttackedCiv
        {
            get { return _attackedCiv; }
            //set { _attackedCiv = value; }
        }
        public string Blamed
        {
            get { return _blamed; }
        }

        public IntelOrdersStealCredits(Civilization attacking, Civilization attacked, string blamed)
            {
            _attackingCiv = attacking;
            _attackedCiv = attacked;
            _blamed = blamed;
            }

    }
}
