// BaseGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Universe
{
    [Serializable]
    public abstract class BaseGalaxyLayout : IGalaxyLayout
    {
        public const int MaxStarPlacementAttempts = 100;

        protected static double FindNearestNeighborDistance(
            MapLocationQuadtreeNode source,
            Quadtree<MapLocationQuadtreeNode> neighbors)
        {
            int radius = 2;
            MapRectangle region = neighbors.Region;

            if (!neighbors.Any())
            {
                return double.MaxValue;
            }

            while (true)
            {
                MapRectangle boundingBox = new MapRectangle(
                    Math.Max(region.MinX, source.Location.X - radius),
                    Math.Max(region.MinY, source.Location.Y - radius),
                    Math.Min(region.MaxX, source.Location.X + radius),
                    Math.Min(region.MaxY, source.Location.Y + radius));

                IEnumerable<MapLocationQuadtreeNode> candidates = neighbors.Find(boundingBox);
                if (candidates.Any())
                {
                    double result = candidates.Min(o => GetDistance(o.Location, source.Location));
                    if (neighbors.Contains(source) && result != 0)
                    {
                        System.Diagnostics.Debugger.Break();
                    }

                    return result;
                }

                if (boundingBox.Contains(region))
                {
                    break;
                }

                radius *= 2;
            }

            return 0;
        }

        protected static double GetDistance(MapLocation a, MapLocation b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        protected abstract int GetStarPositionsInternal(
            Quadtree<MapLocationQuadtreeNode> positions,
            int number,
            int width,
            int height);

        #region IGalaxyLayout Members
        public int GetStarPositions(
            out ICollection<MapLocation> positions,
            int number,
            int width,
            int height)
        {
            Quadtree<MapLocationQuadtreeNode> quadtree = new Quadtree<MapLocationQuadtreeNode>(new MapRectangle(0, 0, width, height));
            int result = GetStarPositionsInternal(quadtree, number, width, height);
            positions = quadtree.Select(o => o.Location).Distinct().ToList();
            return result;
        }
        #endregion
    }
}
