// DieRoll.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
//using System.Collections.Generic;

namespace Supremacy.Utility
{
    public static class DieRoll
    {
        private static readonly Random _random;
        //private static readonly SortedDictionary<int, ChanceTree<int>> _chanceTrees;

        static DieRoll()
        {
            _random = new MersenneTwister();
            //_chanceTrees = new SortedDictionary<int, ChanceTree<int>>();
        }

        public static int Roll(int numSides)
        {
            return _random.Next(numSides) + 1;
            //if (numSides < 0)
            //    numSides = 0;
            //EnsureChanceTree(numSides);
            //return _chanceTrees[numSides].Get();
        }

        //private static void EnsureChanceTree(int numSides)
        //{
        //    if (!_chanceTrees.ContainsKey(numSides))
        //    {
        //        ChanceTree<int> chanceTree = new ChanceTree<int>();
        //        for (int i = 1; i <= numSides; i++)
        //        {
        //            chanceTree.AddChance(1, i);
        //        }
        //        _chanceTrees[numSides] = chanceTree;
        //    }
        //}

        public static int Roll(int count, int numSides)
        {
            int max = 1;
            for (int i = 0; i < count; i++)
            {
                int result = Roll(numSides);
                if (result > max)
                    max = result;
            }
            return max;
        }

        public static bool Chance(int numSides)
        {
            return Roll(numSides) == numSides;
        }
    }
}
