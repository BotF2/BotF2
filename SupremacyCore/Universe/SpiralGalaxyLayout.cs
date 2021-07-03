// SpiralGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;

namespace Supremacy.Universe
{
    [Serializable]
    public class SpiralGalaxyLayout : BaseGalaxyLayout
    {
        protected override int GetStarPositionsInternal
            (Quadtree<MapLocationQuadtreeNode> positions,
            int number,
            int width,
            int height)
        {
            const int numArms = 3;
            int initialCount = positions.Count;
            Random rand = new MersenneTwister();
            double armOffset = rand.NextDouble() * (2.0 * Math.PI);
            const double armAngle = 2.0 * Math.PI / numArms;
            const double armSpread = 0.3 * Math.PI / numArms;
            const double armLength = 1.5 * Math.PI;
            const double center = 0.15;

            for (int i = 0, attempts = 0;
                 (i < number) && (attempts < MaxStarPlacementAttempts);
                 ++i, ++attempts)
            {
                double radius = rand.NextDouble();

                MapLocation position;
                if (radius < center)
                {
                    double angle = rand.NextDouble() * 2.0 * Math.PI;
                    position = new MapLocation(
                        (int)(((radius * Math.Cos(angle + armOffset)) + 1) * width / 2),
                        (int)(((radius * Math.Sin(angle + armOffset)) + 1) * height / 2));
                }
                else
                {
                    double arm = rand.Next(0, numArms) * armAngle;
                    double angle = RandomHelper.Gaussian() * armSpread;
                    position = new MapLocation(
                        (int)(((radius * Math.Cos(armOffset + arm + angle + radius * armLength)) + 1) * width / 2),
                        (int)(((radius * Math.Sin(armOffset + arm + angle + radius * armLength)) + 1) * height / 2));
                }

                if ((position.X < 0) || (width <= position.X)
                    || (position.Y < 0) || (height <= position.Y)
                    || double.IsNaN(position.X) || double.IsNaN(position.Y))
                {
                    continue;
                }

                MapLocationQuadtreeNode newNode = new MapLocationQuadtreeNode(position);
                double nearest = FindNearestNeighborDistance(newNode, positions);
                if (nearest < GalaxyGenerator.MinDistanceBetweenStars)
                {
                    if (attempts < (MaxStarPlacementAttempts - 1))
                    {
                        --i;
                        continue;
                    }
                    GameLog.Core.GalaxyGenerator.Warn("Max star placement attempts reached.");
                }
                else
                {
                    positions.Add(newNode);
                }
                attempts = 0;
            }
            return positions.Count - initialCount;
        }
    }
}
