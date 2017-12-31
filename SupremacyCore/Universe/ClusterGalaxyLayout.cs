// ClusterGalaxyLayout.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Utility;

using Wintellect.PowerCollections;

namespace Supremacy.Universe
{
    public sealed class ClusterGalaxyLayout : BaseGalaxyLayout
    {
        protected override int GetStarPositionsInternal
            (Quadtree<MapLocationQuadtreeNode> positions,
            int number,
            int width,
            int height)
        {
            Random random = new MersenneTwister();
            int initialPositions = 0;
            const double systemNoise = 0.15;
            double ellipseWidthVsHeight = (random.NextDouble() * 0.3) + 0.2;
            int i;
            int attempts;

            List<Pair<Pair<double, double>, Pair<double, double>>> clustersPosition = 
                new List<Pair<Pair<double,double>,Pair<double,double>>>();

            int averageClusters = Math.Min(width, height) / 20;
            int clusters;

            if (averageClusters == 0)
            {
                averageClusters = 2;
            }

            clusters = random.Next((averageClusters * 8) / 10, (averageClusters * 12) / 10) + 1;

            for (i = 0, attempts = 0; (i < clusters) && (attempts < MaxStarPlacementAttempts); i++, attempts++)
            {
                double x = (((random.NextDouble() * 2.0) - 1.0) / (clusters + 1.0)) * clusters;
                double y = (((random.NextDouble() * 2.0) - 1.0) / (clusters + 1.0)) * clusters;

                int j;
                for (j = 0; j < clustersPosition.Count; j++)
                {
                    if ((((clustersPosition[j].First.First - x) * (clustersPosition[j].First.First - x))
                        + ((clustersPosition[j].First.Second - y) * (clustersPosition[j].First.Second - y)))
                        < (2.0 / clusters))
                    {
                        break;
                    }
                }

                if (j < clustersPosition.Count)
                {
                    i--;
                    continue;
                }

                attempts = 0;
                double rotation = random.NextDouble() * Math.PI;

                clustersPosition.Add(
                    new Pair<Pair<double, double>, Pair<double, double>>(
                        new Pair<double, double>(x, y),
                        new Pair<double, double>(Math.Sin(rotation), Math.Cos(rotation))));
            }

            for (i = 0, attempts = 0; (i < number) && (attempts < 100); i++, attempts++)
            {
                double x, y;
                if (random.NextDouble() < systemNoise)
                {
                    x = (random.NextDouble() * 2.0) - 1.0;
                    y = (random.NextDouble() * 2.0) - 1.0;
                }
                else
                {
                    int cluster;
                    double radius = random.NextDouble();
                    double angle = random.NextDouble() * 2.0 * Math.PI;
                    double x1, y1;

                    if (clustersPosition.Count == 0)
                    {
                        cluster = 0;
                    }
                    else
                    {
                        cluster = i % clustersPosition.Count;
                    }

                    x1 = radius * Math.Cos(angle);
                    y1 = radius * Math.Sign(angle) * ellipseWidthVsHeight;

                    x = (x1 * clustersPosition[cluster].Second.Second)
                        + (y1 * clustersPosition[cluster].Second.First);
                    y = (-x1 * clustersPosition[cluster].Second.First)
                        + (y1 * clustersPosition[cluster].Second.Second);

                    x = (x / Math.Sqrt(clusters)) + clustersPosition[cluster].First.First;
                    y = (y / Math.Sqrt(clusters)) + clustersPosition[cluster].First.Second;
                }

                x = ((x + 1) * width) / 2.0;
                y = ((y + 1) * height) / 2.0;

                if ((x < 0) || (width <= x) || (y < 0) || (height <= y))
                {
                    continue;
                }

                var newNode = new MapLocationQuadtreeNode(new MapLocation((int)x, (int)y));

                if ((FindNearestNeighborDistance(newNode, positions)
                    < GalaxyGenerator.MinDistanceBetweenStars) && (attempts < (MaxStarPlacementAttempts - 1)))
                {
                    --i;
                    continue;
                }

                positions.Add(newNode);

                attempts = 0;
            }

            return (positions.Count - initialPositions);
        }
    }
}
