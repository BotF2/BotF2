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

        public Path(TNode start) : this(start, null, 0) { }
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
            List<TNode> buffer = new List<TNode>();
            for (Path<TNode> path = this; path != null; path = path.PreviousSteps)
            {
                buffer.Insert(0, path.LastStep);
            }

            foreach (TNode node in buffer)
            {
                yield return node;
            }
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
            {
                throw new ArgumentNullException("waypoint");
            }

            List<Sector> waypointsList = new List<Sector>(((waypoints == null) ? 0 : waypoints.Length) + 1)
                                {
                                    waypoint
                                };
            if (waypoints != null)
            {
                waypointsList.AddRange(waypoints);
            }

            return FindPath(fleet, forbiddenSectors, waypointsList);
        }

        public static TravelRoute FindPath(Fleet fleet, PathOptions options, Sector waypoint, params Sector[] waypoints)
        {
            if (waypoint == null)
            {
                throw new ArgumentNullException("waypoint");
            }

            List<Sector> waypointsList = new List<Sector>(((waypoints == null) ? 0 : waypoints.Length) + 1)
                                {
                                    waypoint
                                };
            if (waypoints != null)
            {
                waypointsList.AddRange(waypoints);
            }

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
            {
                //GameLog.Client.AI.DebugFormat("TravelRout fleet is null");
                throw new ArgumentNullException("fleet");
            }
            if (waypoints == null)
            {
                //GameLog.Client.AI.DebugFormat("TravelRout waypoints is null");
                throw new ArgumentNullException("waypoints");
            }
            foreach (Ship ship in fleet.Ships)
            {
                if (fleet.Owner != null)
                {
                    break;
                }

                if (ship.Owner != null)
                {
                    fleet.Owner = ship.Owner;
                    break;
                }
            }

            ISet<Sector> forbiddenSectorSet;

            if (forbiddenSectors != null)
            {
                forbiddenSectorSet = forbiddenSectors as ISet<Sector> ?? new HashSet<Sector>(forbiddenSectors);
            }
            else
            {
                forbiddenSectorSet = null;
            }

            TravelRoute route = new TravelRoute(waypoints);
            Sector start = fleet.Sector;

            foreach (Sector waypoint in waypoints)
            {
                MapLocation waypointLocation = waypoint.Location;

                Path<Sector> segment = FindPath(
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
                {
                    continue;
                }

                foreach (Sector step in segment.Skip(1))
                {
                    route.Push(step.Location);
                    // GameLog.Core.Diplomacy.DebugFormat("start ={0} path step ={1}", start, step);
                }
                start = waypoint;
            }
            //GameLog.Core.Diplomacy.DebugFormat("star fleet sector ={0}, ",start);
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
            HashSet<TNode> closed = new HashSet<TNode>();
            PriorityQueue<double, Path<TNode>> queue = new PriorityQueue<double, Path<TNode>>();

            queue.Enqueue(0, new Path<TNode>(start));

            while (!queue.IsEmpty)
            {
                Path<TNode> path = queue.Dequeue();

                if (closed.Contains(path.LastStep))
                {
                    continue;
                }

                if (isFinishedCallback(path.LastStep))
                {
                    return path;
                }

                if ((canContinueCallback == null) || !canContinueCallback(path.LastStep))
                {
                    return null;
                }

                _ = closed.Add(path.LastStep);

                foreach (TNode neighbor in getNeighbors(path.LastStep))
                {
                    double d = distance(path.LastStep, neighbor);
                    Path<TNode> newPath = path.AddStep(neighbor, d);

                    queue.Enqueue(newPath.TotalCost + estimate(neighbor), newPath);
                }
            }

            return null;
        }
        #endregion
    }
}