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

    //[Serializable]
    //public class AStarNode
    //{
    //    private Sector location;
    //    private AStarNode parent;
    //    private double hCost;
    //    private double gCost;
    //    private double fCost;
    //    private double cost;
    //    private double totalCost;
    //    private bool onOpen;
    //    private int openIndex;

    //    public Sector Sector
    //    {
    //        get { return location; }
    //        set { location = value; }
    //    }

    //    public AStarNode Parent
    //    {
    //        get { return parent; }
    //        set { parent = value; }
    //    }

    //    public double Cost
    //    {
    //        get { return cost; }
    //        set { cost = value; }
    //    }

    //    public double FCost
    //    {
    //        get { return fCost; }
    //        set { fCost = value; }
    //    }

    //    public double GCost
    //    {
    //        get { return gCost; }
    //        set { gCost = value; }
    //    }

    //    public double HCost
    //    {
    //        get { return hCost; }
    //        set { hCost = value; }
    //    }

    //    public double TotalCost
    //    {
    //        get { return totalCost; }
    //        set { totalCost = value; }
    //    }

    //    public bool OnOpen
    //    {
    //        get { return onOpen; }
    //        set { onOpen = value; }
    //    }

    //    public int OpenIndex
    //    {
    //        get { return openIndex; }
    //        set { openIndex = value; }
    //    }
    //}

    //[Serializable]
    //internal class AStarNodeComparer : Comparer<AStarNode>
    //{
    //    public override int Compare(AStarNode x, AStarNode y)
    //    {
    //        return x.FCost.CompareTo(y.FCost);
    //    }
    //}

    //    [Serializable]
    //    public class AStar
    //    {
    //        private AStarNode[,] _nodes;
    //        private AStarHeuristic _heuristic;
    //        private AStarNodeComparer _comparer;        
    //        private PriorityQueue<AStarNode> _open;

    //        public AStar(SectorMap map) 
    //            : this(map, StandardHeuristics.EuclideanDistance) { }

    //        public AStar(SectorMap map, AStarHeuristic heuristic)
    //        {
    //            _nodes = new AStarNode[map.Width, map.Height];
    //            _heuristic = heuristic;
    //            _comparer = new AStarNodeComparer();
    //            _open = new PriorityQueue<AStarNode>(_comparer, map.Width * map.Height);

    //            for (int x = 0; x < map.Width; x++)
    //            {
    //                for (int y = 0; y < map.Height; y++)
    //                {
    //                    _nodes[x, y] = new AStarNode();
    //                }
    //            }

    //            Initialize();
    //        }

    //        protected void Initialize()
    //        {
    //            for (int x = 0; x < _nodes.GetLength(0); x++)
    //            {
    //                for (int y = 0; y < _nodes.GetLength(1); y++)
    //                {
    //                    _nodes[x, y].Sector = null;
    //                    _nodes[x, y].Parent = null;
    //                    _nodes[x, y].HCost = 0;
    //                    _nodes[x, y].GCost = 0;
    //                    _nodes[x, y].FCost = 0;
    //                    _nodes[x, y].OnOpen = false;
    //                    _nodes[x, y].OpenIndex = -1;
    //                }
    //            }
    //        }

    //        [MethodImpl(MethodImplOptions.Synchronized)]
    //        public TravelRoute FindPath(Fleet fleet, IList<Sector> waypoints)
    //        {
    //            if (fleet == null)
    //                throw new ArgumentNullException("fleet");
    //            if (waypoints == null)
    //                throw new ArgumentNullException("waypoints");

    //            TravelRoute route = new TravelRoute(waypoints);
    //            Sector start = fleet.Sector;
    //            foreach (Sector waypoint in waypoints)
    //            {
    //                Initialize();
    //                IList<Sector> segment = FindPath2(fleet, start, waypoint);
    //                if (segment.Count == 0)
    //                    continue;
    //                foreach (Sector step in segment)
    //                    route.Push(step.Location);
    //                start = waypoint;
    //            }
    //            route.Compact();
    //            return route;
    //        }

    //        protected IList<Sector> FindPath2(Fleet fleet, Sector start, Sector goal)
    //        {
    //            AStarNode startNode = new AStarNode();

    //#if DEBUG_ASTAR
    //            char[,] visitedNodes = new char[GameContext.Current.Universe.Map.Width,
    //                                            GameContext.Current.Universe.Map.Height];
    //            for (int y = 0; y < visitedNodes.GetLength(0); y++)
    //            {
    //                for (int x = 0; x < visitedNodes.GetLength(1); x++)
    //                {
    //                    visitedNodes[x, y] = '0';
    //                }
    //            }
    //#endif
    //            _open.Clear();

    //            startNode.Parent = null;
    //            startNode.Sector = start;
    //            startNode.Cost = 0;
    //            startNode.HCost = 0;
    //            startNode.GCost = 0;
    //            startNode.FCost = 0;
    //            startNode.TotalCost = 0;
    //            startNode.OnOpen = true;
    //            startNode.OpenIndex = _open.Push(startNode);

    //            while (!_open.IsEmpty)
    //            {
    //                AStarNode nextNode = _open.Pop();
    //                if (nextNode.Sector == goal)
    //                {
    //#if DEBUG_ASTAR
    //                    System.IO.StreamWriter fout = new System.IO.StreamWriter("AStarPath.txt");
    //                    visitedNodes[start.Location.X, start.Location.Y] = 'S';
    //                    visitedNodes[nextNode.Sector.Location.X, nextNode.Sector.Location.Y] = 'X';
    //                    for (int y = 0; y < visitedNodes.GetLength(0); y++)
    //                    {
    //                        for (int x = 0; x < visitedNodes.GetLength(1); x++)
    //                        {
    //                            fout.Write(visitedNodes[x, y]);
    //                        }
    //                        fout.WriteLine();
    //                    }
    //                    fout.Close();
    //#endif
    //                    return ConstructPath(startNode, nextNode);
    //                }
    //                foreach (Sector neighbor in nextNode.Sector.GetNeighbors())
    //                {
    //                    double hCost = _heuristic(neighbor, goal);
    //                    double gCost = nextNode.GCost + 1;
    //                    AStarNode newNode = _nodes[neighbor.Location.X, neighbor.Location.Y];
    //                    if ((newNode.Parent == null) && (newNode.Sector != nextNode.Sector))
    //                    {
    //#if DEBUG_ASTAR
    //                        visitedNodes[neighbor.Location.X, neighbor.Location.Y] = '1';
    //#endif
    //                        newNode.Sector = neighbor;
    //                        newNode.FCost = gCost + hCost;
    //                        newNode.GCost = gCost;
    //                        newNode.Parent = nextNode;
    //                        newNode.OpenIndex = _open.Push(newNode);
    //                        newNode.OnOpen = true;
    //                    }
    //                    else if ((gCost < newNode.GCost) && newNode.OnOpen)
    //                    {
    //                        newNode.FCost = gCost + hCost;
    //                        newNode.GCost = gCost;
    //                        newNode.Parent = nextNode;
    //                        _open.Update(newNode);
    //                    }
    //                }
    //            }
    //            return new List<Sector>();
    //        }

    //        protected static IList<Sector> ConstructPath(AStarNode start, AStarNode goal)
    //        {
    //            AStarNode current = goal;
    //            Deque<Sector> path = new Deque<Sector>();           
    //            while (current != start)
    //            {
    //                path.AddToFront(current.Sector);
    //                current = current.Parent;
    //            }
    //            return path;
    //        }
    //    }

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