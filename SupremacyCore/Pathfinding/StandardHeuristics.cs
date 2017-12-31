// StandardHeuristics.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Universe;

namespace Supremacy.Pathfinding
{
    public static class StandardHeuristics
    {
        public static double ManhattanDistance(Sector start, Sector goal)
        {
            return Math.Max(Math.Abs(start.Location.X - goal.Location.X),
                            Math.Abs(start.Location.Y - goal.Location.Y));
        }

        public static double EuclideanDistance(Sector start, Sector goal)
        {
            return Math.Sqrt(Math.Pow(goal.Location.X - start.Location.X, 2) +
                             Math.Pow(goal.Location.Y - start.Location.Y, 2));
        }
    }
}
