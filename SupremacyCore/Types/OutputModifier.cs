// OutputModifier.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Types
{
    [Serializable]
    public class OutputModifier
    {
        private int _bonus;
        private float _efficiency;

        public int Bonus
        {
            get { return _bonus; }
            set { _bonus = value; }
        }

        public Percentage Efficiency
        {
            get { return _efficiency; }
            set { _efficiency = value; }
        }

        public OutputModifier() : this(0, 0f) {}

        public OutputModifier(int bonus, Percentage efficiency)
        {
            _bonus = bonus;
            _efficiency = efficiency;
        }
    }
}
