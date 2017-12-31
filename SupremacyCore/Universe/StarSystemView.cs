// StarSystemView.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Entities;
using Supremacy.Types;

namespace Supremacy.Universe
{
    [Serializable]
    public sealed class StarSystemView : UniverseObjectView
    {
        private int? population;
        private StarType? starType;
        private Planet[] planets;

        public new StarSystem Target
        {
            get { return base.Target as StarSystem; }
        }

        public StarType? StarType
        {
            get { return starType; }
            internal set { starType = value; }
        }

        public Planet[] Planets
        {
            get { return planets; }
            internal set { planets = value; }
        }

        public int? Population
        {
            get { return population; }
            internal set { population = value; }
        }

        public StarSystemView(StarSystem target) : base(target)
        {
            starType = target.StarType;
        }

        public int GetMaxPopulation(Race race)
        {
            int result = 0;
            foreach (Planet planet in planets)
                result += planet.GetMaxPopulation(race);
            return result;
        }

        public Percentage GetGrowthRate(Race race)
        {
            float maxPopMultiplier = 0.01f * GetMaxPopulation(race);
            Percentage growthRate = 0;
            foreach (Planet planet in planets)
            {
                growthRate += (maxPopMultiplier * planet.GetMaxPopulation(race))
                    * planet.GetGrowthRate(race);
                    
            }
            return growthRate;
        }
    }
}
