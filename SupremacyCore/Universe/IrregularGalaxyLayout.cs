// IrregularGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;

namespace Supremacy.Universe
{
    public sealed class IrregularGalaxyLayout : BaseGalaxyLayout
    {
        protected override int GetStarPositionsInternal
             (Quadtree<MapLocationQuadtreeNode> positions,
             int number,
             int width,
             int height)
        {
            int initialCount = positions.Count;
            for (int i = 0, attempts = 1;
                 (i < number) && (attempts < MaxStarPlacementAttempts);
                 i++, attempts++)
            {
                var location = new MapLocation(
                    RandomHelper.Random(width),
                    RandomHelper.Random(height));
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
