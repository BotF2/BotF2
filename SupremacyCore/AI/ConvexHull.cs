// ConvexHull.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Universe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.AI
{
    [Serializable]
    public class ConvexHullSet : IEnumerable<ConvexHull>
    {
        #region Fields
        private readonly List<ConvexHull> _items;
        #endregion

        #region Constructors
        public ConvexHullSet(IEnumerable<ConvexHull> items)
        {
            _items = new List<ConvexHull>(items);
        }
        #endregion

        #region Properties
        public ConvexHull this[int index]
        {
            get { return _items[index]; }
        }

        public IList<ConvexHull> Items
        {
            get {return _items.AsReadOnly(); }
        }

        public IEnumerable<MapLocation> CombinedInterior
        {
            get
            {
                var locations = Enumerable.Empty<MapLocation>();
                foreach (ConvexHull item in _items)
                    locations = locations.Concat(item.Interior);
                return locations;
            }
        }
        #endregion

        #region IEnumerable<ConvexHull> Members
        public IEnumerator<ConvexHull> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    [Serializable]
    public class ConvexHull : IEnumerable<MapLocation>
    {
        #region Fields
        private readonly MapLocation[] _points;
        #endregion

        #region Constructors
        public ConvexHull(IEnumerable<MapLocation> points)
        {
            MapLocation[] allPoints = points.ToArray();
            int n = allPoints.Length;
            if (n <= 1)
            {
                _points = allPoints;
            }
            else
            {
                Array.Sort(allPoints);
                MapLocation left = allPoints[0];
                MapLocation right = allPoints[n - 1];
                C5.IList<MapLocation> lower = new C5.LinkedList<MapLocation>();
                C5.IList<MapLocation> upper = new C5.LinkedList<MapLocation>();
                lower.InsertFirst(left);
                upper.InsertLast(left);
                for (int i = 0; i < n; i++)
                {
                    double det = MapLocation.Area2(left, right, allPoints[i]);
                    if (det > 0)
                    {
                        upper.InsertLast(allPoints[i]);
                    }
                    else
                    {
                        lower.InsertFirst(allPoints[i]);
                    }
                }
                lower.InsertFirst(right);
                upper.InsertLast(right);
                Eliminate(lower);
                Eliminate(upper);
                _points = new MapLocation[lower.Count + upper.Count - 2];
                lower[0, lower.Count - 1].CopyTo(_points, 0);
                upper[0, upper.Count - 1].CopyTo(_points, lower.Count - 1);
            }
        }
        #endregion

        #region Properties
        public MapLocation[] Points
        {
            get { return (MapLocation[])_points.Clone(); }
        }

        public IEnumerable<MapLocation> Interior
        {
            get { return this; }
        }
        #endregion

        #region IEnumerable<MapLocation> Members
        public IEnumerator<MapLocation> GetEnumerator()
        {
            SectorMap map = GameContext.Current.Universe.Map;
            for (int x = _points.Min(o => o.X); x <= _points.Max(o => o.X); x++)
            {
                for (int y = _points.Min(o => o.Y); y <= _points.Max(o => o.Y); y++)
                {
                    MapLocation location = new MapLocation(x, y);
                    if (Contains(location))
                        yield return location;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Methods
        public static void Eliminate(C5.IList<MapLocation> lst)
        {
            C5.IList<MapLocation> view = lst.View(0, 0);
            int slide = 0;
            while (view.TrySlide(slide, 3))
            {
                if (MapLocation.Area2(view[0], view[1], view[2]) < 0) // right turn
                {
                    slide = 1;
                }
                else // left or straight
                {
                    view.RemoveAt(1);
                    slide = view.Offset != 0 ? -1 : 0;
                }
            }
        }

        public bool Contains(MapLocation point)
        {
            bool flipEdge;
            double tX, tY, tX2, tY2;
            PlaneSet[] planes = new PlaneSet[_points.Length];

            if (_points.Length < 3)
                return _points.Contains(point);

            flipEdge = (_points[0].X - _points[1].X) * (_points[1].Y - _points[2].Y) > (_points[0].Y - _points[1].Y) * (_points[1].X - _points[2].X);

            for (int i = 0, p0 = _points.Length - 1, p1 = 0; p1 < _points.Length; p0 = p1, p1++, i++)
            {
                planes[i].VX = _points[p0].Y - _points[p1].Y;
                planes[i].VY = _points[p1].X - _points[p0].X;
                planes[i].C = planes[i].VX * _points[p0].X + planes[i].VY * _points[p0].Y;

                // Check sense and reverse plane edge if need be.
                if (flipEdge)
                {
                    planes[i].VX = -planes[i].VX;
                    planes[i].VY = -planes[i].VY;
                    planes[i].C = -planes[i].C;
                }
            }

            tX = point.X - 0.5;
            tY = point.Y - 0.5;
            tX2 = point.X + 0.5;
            tY2 = point.Y + 0.5;

            for (int p0 = _points.Length + 1, i = 0; --p0 > 0; i++)
            {
                // Test if the point is outside this edge.
                if ((planes[i].VX * tX + planes[i].VY * tY > planes[i].C) && (planes[i].VX * tX2 + planes[i].VY * tY2 > planes[i].C))
                {
                    return false;
                }
            }

            // If we make it to here, we were inside all edges.
            return true;
        }
        #endregion

        #region PlaneSet Type
        [Serializable]
        protected struct PlaneSet
        {
            #region Fields
            public double C;
            public double VX, VY;
            #endregion
        }
        #endregion
    }
}