// ColonyView.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Universe
{
    [Serializable]
    public class ColonyView : UniverseObjectView
    {
        // inhabitant id, health, population, max population
        private short? population;
        private string inhabitantId;

        public bool IsInhabited
        {
            get { return (population.HasValue && (population.Value > 0)); }
        }

        public Race Inhabitants
        {
            get
            {
                if (!IsInhabited || (inhabitantId == null))
                    return null;
                return GameContext.Current.Races[inhabitantId];
            }
            set
            {
                inhabitantId = (value == null) 
                    ? null 
                    : value.Key;
            }
        }

        public int? Population
        {
            get { return population; }
            internal set { population = (short?)value; }
        }

        public ColonyView(Colony colony)
            : base(colony)
        {
        }
    }
}
