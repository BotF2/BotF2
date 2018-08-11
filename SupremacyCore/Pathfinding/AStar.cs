// AStar.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//#define DEBUG_ASTAR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;

namespace Supremacy.Pathfinding
{
    public delegate double AStarHeuristic(Sector start, Sector goal);

    public class Path<TNode> : IEnumerable<TNode>
    {
        #region Constructors
        private Path(TNode lastStep, Path<TNode> previousSteps, double totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(TNode start) : this(start, null, 0) {}
        #endregion

        #region Properties and Indexers
        public TNode LastStep { get; private set; }
        public Path<TNode> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }
        #endregion

        #region Methods
        public Path<TNode> AddStep(TNode step, double stepCost)
        {
            return new Path<TNode>(step, this, TotalCost + stepCost);
        }
        #endregion

        #region IEnumerable<TNode> Members
        public IEnumerator<TNode> GetEnumerator()
        {
            var buffer = new List<TNode>();
            for (var path = this; path != null; path = path.PreviousSteps)
                buffer.Insert(0, path.LastStep);
            foreach (var node in buffer)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public static class AStar
    {
        #region Methods
        public static TravelRoute FindPath(Fleet fleet, IEnumerable<Sector> forbiddenSectors, Sector waypoint, params Sector[] waypoints)
        {
            return FindPath(fleet, PathOptions.IgnoreDanger, waypoint, waypoints);
        }

        public static TravelRoute FindPath(Fleet fleet, Sector waypoint, params Sector[] waypoints)
        {
            return FindPath(fleet, PathOptions.IgnoreDanger, waypoint, waypoints);
        }

        public static TravelRoute FindPath(Fleet fleet, PathOptions options, IEnumerable<Sector> forbiddenSectors, Sector waypoint, params Sector[] waypoints)
        {
            if (waypoint == null)
                throw new ArgumentNullException("waypoint");
            var waypointsList = new List<Sector>(((waypoints == null) ? 0 : waypoints.Length) + 1)
                                {
                                    waypoint
                                };
            if (waypoints != null)
                waypointsList.AddRange(waypoints);
            return FindPath(fleet, forbiddenSectors, waypointsList);
        }

        public static TravelRoute FindPath(Fleet fleet, PathOptions options, Sector waypoint, params Sector[] waypoints)
        {
            if (waypoint == null)
                throw new ArgumentNullException("waypoint");
            var waypointsList = new List<Sector>(((waypoints == null) ? 0 : waypoints.Length) + 1)
                                {
                                    waypoint
                                };
            if (waypoints != null)
                waypointsList.AddRange(waypoints);
            return FindPath(fleet, null, waypointsList);
        }

        public static TravelRoute FindPath(Fleet fleet, IEnumerable<Sector> forbiddenSectors, IList<Sector> waypoints)
        {
            return FindPath(fleet, PathOptions.IgnoreDanger, null, waypoints);
        }

        public static TravelRoute FindPath(Fleet fleet, IList<Sector> waypoints)
        {
            return FindPath(fleet, PathOptions.IgnoreDanger, null, waypoints);
        }

        public static TravelRoute FindPath(Fleet fleet, PathOptions options, IEnumerable<Sector> forbiddenSectors, IEnumerable<Sector> waypoints)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            if (waypoints == null)
                throw new ArgumentNullException("waypoints");

            ISet<Sector> forbiddenSectorSet;

            if (forbiddenSectors != null)
                forbiddenSectorSet = forbiddenSectors as ISet<Sector> ?? new HashSet<Sector>(forbiddenSectors);
            else
                forbiddenSectorSet = null;

            var route = new TravelRoute(waypoints);
            var start = fleet.Sector;

            foreach (var waypoint in waypoints)
            {
                var waypointLocation = waypoint.Location;

                var segment = FindPath(
                    start,
                    s => s.Location == waypointLocation,
                    s => true,
                    sector => ((options & PathOptions.NoEnemyTerritory) == PathOptions.NoEnemyTerritory)
                                  ? sector.GetNeighbors().Where(
                                      o => (forbiddenSectorSet == null || !forbiddenSectorSet.Contains(sector)) &&
                                           DiplomacyHelper.IsSafeTravelGuaranteed(fleet.Owner, o))
                                  : sector.GetNeighbors().Where(
                                      o => (forbiddenSectorSet == null || !forbiddenSectorSet.Contains(sector)) &&
                                           DiplomacyHelper.IsTravelAllowed(fleet.Owner, o)),
                    StandardHeuristics.EuclideanDistance,
                    sector => 1.0);

                if (segment == null || !segment.Any())
                    continue;

                foreach (var step in segment.Skip(1))
                    route.Push(step.Location);

                start = waypoint;
            }

            route.Compact();
            return route;
        }

        public static Path<TNode> FindPath<TNode>(
            TNode start,
            Func<TNode, bool> isFinishedCallback,
            Func<TNode, bool> canContinueCallback,
            Func<TNode, IEnumerable<TNode>> getNeighbors,
            Func<TNode, TNode, double> distance,
            Func<TNode, double> estimate)
        {
            var closed = new HashSet<TNode>();
            var queue = new PriorityQueue<double, Path<TNode>>();

            queue.Enqueue(0, new Path<TNode>(start));

            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();

                if (closed.Contains(path.LastStep))
                    continue;

                if (isFinishedCallback(path.LastStep))
                    return path;

                if ((canContinueCallback == null) || !canContinueCallback(path.LastStep))
                    return null;

                closed.Add(path.LastStep);

                foreach (var neighbor in getNeighbors(path.LastStep))
                {
                    var d = distance(path.LastStep, neighbor);
                    var newPath = path.AddStep(neighbor, d);

                    queue.Enqueue(newPath.TotalCost + estimate(neighbor), newPath);
                }
            }

            return null;
        }
        #endregion
    }
}