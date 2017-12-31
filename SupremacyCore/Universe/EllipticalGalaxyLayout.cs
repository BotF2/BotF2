// EllipticalGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Universe
{
    public sealed class EllipticalGalaxyLayout : BaseGalaxyLayout
    {
        protected override int GetStarPositionsInternal
             (Quadtree<MapLocationQuadtreeNode> positions,
             int number,
             int width,
             int height)
        {
            Random random = new Random(328250362);
            int initialCount = positions.Count;

            double ellipseWidthVsHeight = (random.NextDouble() * 0.2) + 0.4;
            double rotation = random.NextDouble() * Math.PI;
            double rotationSin = Math.Sin(rotation);
            double rotationCos = Math.Cos(rotation);
            double gapConstant = 0.95;
            double gapSize = 1.0 - (gapConstant * gapConstant * gapConstant);

            for (int i = 0, attempts = 1;
                 (i < number) && (attempts < MaxStarPlacementAttempts);
                 i++, attempts++)
            {
                double radius = random.NextDouble() * gapConstant;
                double angle = random.NextDouble() * 2.0 * Math.PI;

                double x1 = radius * Math.Cos(angle);
                double y1 = radius * Math.Sin(angle) * ellipseWidthVsHeight;

                double x = (x1 * rotationCos) - (y1 * rotationSin);
                double y = (x1 * rotationSin) + (y1 * rotationCos);

                MapLocation location = new MapLocation(
                    (int)((x + 1.0) * width / 2.0),
                    (int)((y + 1.0) * height / 2.0));

                var newNode = new MapLocationQuadtreeNode(location);

                double lowestDist = FindNearestNeighborDistance(newNode, positions);
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
