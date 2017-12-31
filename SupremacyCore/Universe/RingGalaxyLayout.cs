// RingGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Utility;

namespace Supremacy.Universe
{
    public sealed class RingGalaxyLayout : BaseGalaxyLayout
    {
        protected override int GetStarPositionsInternal
            (Quadtree<MapLocationQuadtreeNode> positions,
            int number,
            int width,
            int height)
        {
            int initialCount = positions.Count;
            double ringWidth = width / 4.0;
            double ringRadius = (width - ringWidth) / 2.0;
            Random random = new MersenneTwister();

            for (int i = 0, attempts = 1;
                 (i < number) && (attempts < MaxStarPlacementAttempts);
                 i++, attempts++)
            {
                double theta = random.NextDouble() * 2.0 * Math.PI;
                double radius = Statistics.Gaussian(ringRadius, ringWidth / 3.0);

                MapLocation location = new MapLocation(
                    (int)((width / 2.0) + (radius * Math.Cos(theta))),
                    (int)((height / 2.0) + (radius * Math.Sin(theta))));

                if ((location.X < 0) || (location.X >= width)
                    || (location.Y < 0) || (location.Y >= height))
                {
                    continue;
                }

                var newNode = new MapLocationQuadtreeNode(location);
                var lowestDist = FindNearestNeighborDistance(newNode, positions);
                if ((lowestDist < GalaxyGenerator.MinDistanceBetweenStars)
                    && (attempts < MaxStarPlacementAttempts))
                {
                    --i;
                    continue;
                }

                positions.Add(newNode);
                attempts = 0;
            }
            return positions.Count - initialCount;
        }
    }
}
