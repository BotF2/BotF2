// MapLocationCollection.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Supremacy.Universe
{
    /// <summary>
    /// A QuadTree
    /// </summary>
    public class Quadtree<T> : QuadNode<T> where T : IQuadTreeMember
    {
        public Quadtree(MapRectangle region)
            : base(region, 8)
        {
        }

        #region Searching
        private int searchGeneration;

        public IEnumerable<T> Find(IIntersectsRectangle2D r)
        {
            foreach (T item in Find(r, searchGeneration++))
            {
                yield return item;
            }
        }
        #endregion
    }

    public class QuadNode<T> : IEnumerable<T> where T : IQuadTreeMember
    {
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public MapRectangle Region
        {
            get
            {
                return _region;
            }
        }

        private readonly MapRectangle _region;
        private readonly int _depthLeft;

        public QuadNode(MapRectangle region, int depthLeft)
        {
            _region = region;
            _depthLeft = depthLeft;
        }

        protected QuadNode<T>[,] _nodes;
        protected List<T> _members = new List<T>();

        protected bool IsLeaf
        {
            get
            {
                return _nodes == null;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (IsLeaf)
            {
                foreach (T member in _members)
                    yield return member;
            }
            else
            {
                foreach (QuadNode<T> node in _nodes)
                {
                    foreach (T member in node)
                        yield return member;
                }
            }
        }

        protected IEnumerable<T> Find(IIntersectsRectangle2D r, int generation)
        {
            if (r.Intersects(Region))
            {
                if (IsLeaf)
                {
                    foreach (T member in _members)
                    {
                        if (generation > member.LastSearchHit)
                        {
                            member.LastSearchHit = generation;
                            yield return member;
                        }
                    }
                }
                else
                {
                    foreach (QuadNode<T> node in _nodes)
                    {
                        foreach (T member in node.Find(r, generation))
                        {
                            yield return member;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if this node should split into 4 nodes
        /// </summary>
        protected void CheckSplit()
        {
            if (_depthLeft > 0 && _members != null && _members.Count > 20)
            {
                // There are too many nodes
                MapRectangle[,] rectangles = Region.Quarter();
                _nodes = new QuadNode<T>[2, 2];
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        _nodes[x, y] = new QuadNode<T>(rectangles[x, y], _depthLeft - 1);
                        foreach (T member in _members)
                        {
                            if (member.BoundingBox.Intersects(_nodes[x, y].Region))
                            {
                                _nodes[x, y].Add(member);
                            }
                        }
                    }
                }
                _members = null;
            }
        }

        public void Add(T member)
        {
            ++_count;
            if (IsLeaf)
            {
                if (member.BoundingBox.Intersects(Region))
                {
                    _members.Add(member);
                    CheckSplit();
                }
            }
            else
            {
                foreach (QuadNode<T> node in _nodes)
                {
                    node.Add(member);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IQuadTreeMember
    {
        /// <summary>
        /// This is used to prevent
        /// </summary>
        int LastSearchHit
        {
            get;
            set;
        }

        IIntersectsRectangle2D BoundingBox
        {
            get;
        }
    }

    public struct MapLocationQuadtreeNode : IQuadTreeMember
    {
        private int _lastSearchHit;
        private readonly MapLocation _location;
        private readonly MapRectangle _boundingBox;

        public MapLocationQuadtreeNode(MapLocation location)
        {
            _location = location;
            _boundingBox = new MapRectangle(location);
            _lastSearchHit = 0;
        }

        public MapLocation Location
        {
            get { return _location; }
        }

        public int LastSearchHit
        {
            get { return _lastSearchHit; }
            set { _lastSearchHit = value; }
        }

        public MapRectangle BoundingBox
        {
            get { return _boundingBox; }
        }

        IIntersectsRectangle2D IQuadTreeMember.BoundingBox
        {
            get { return _boundingBox; }
        }
    }

    public interface IIntersectsRectangle2D
    {
        bool Intersects(MapRectangle rectangle);
    }

    public struct MapRectangle : IIntersectsRectangle2D, IEquatable<MapRectangle>
    {
        private int? _hashCode;

        public MapRectangle(MapLocation location)
            : this(location.X, location.Y, location.X, location.Y) {}

        public MapRectangle(int x1, int y1, int x2, int y2)
        {
            if (x1 < MapLocation.MinValue)
                throw new ArgumentOutOfRangeException("x1", "value must be >= MapLocation.MinValue");
            if (y1 < MapLocation.MinValue)
                throw new ArgumentOutOfRangeException("y1", "value must be >= MapLocation.MinValue");
            if (x1 > MapLocation.MaxValue)
                throw new ArgumentOutOfRangeException("x1", "value must be <= MapLocation.MaxValue");
            if (y1 > MapLocation.MaxValue)
                throw new ArgumentOutOfRangeException("y1", "value must be <= MapLocation.MaxValue");
            if (x2 < MapLocation.MinValue)
                throw new ArgumentOutOfRangeException("x2", "value must be >= MapLocation.MinValue");
            if (y2 < MapLocation.MinValue)
                throw new ArgumentOutOfRangeException("y2", "value must be >= MapLocation.MinValue");
            if (x2 > MapLocation.MaxValue)
                throw new ArgumentOutOfRangeException("x2", "value must be <= MapLocation.MaxValue");
            if (y2 > MapLocation.MaxValue)
                throw new ArgumentOutOfRangeException("y2", "value must be <= MapLocation.MaxValue");
            _x1 = (byte)x1;
            _y1 = (byte)y1;
            _x2 = (byte)x2;
            _y2 = (byte)y2;
            _hashCode = null;
        }

        private readonly byte _x1;
        private readonly byte _x2;
        private readonly byte _y1;
        private readonly byte _y2;

        public int MinX
        {
            get
            {
                return Math.Min(_x1, _x2);
            }
        }
        public int MaxX
        {
            get
            {
                return Math.Max(_x1, _x2);
            }
        }
        public int MinY
        {
            get
            {
                return Math.Min(_y1, _y2);
            }
        }
        public int MaxY
        {
            get
            {
                return Math.Max(_y1, _y2);
            }
        }

        public bool Contains(MapRectangle rectangle)
        {
            return ((rectangle.MinX >= MinX) &&
                    (rectangle.MinY >= MinY) &&
                    (rectangle.MaxX <= MaxX) &&
                    (rectangle.MaxY <= MaxY));
        }

        public bool Intersects(MapRectangle rectangle)
        {
            if (rectangle.MaxX < MinX)
                return false;
            if (rectangle.MinX > MaxX)
                return false;
            if (rectangle.MaxY < MinY)
                return false;
            if (rectangle.MinY > MaxY)
                return false;
            return true;
        }

        /// <summary>
        /// Divides this rectangle in 4 equal-sized rectangles
        /// </summary>
        /// <returns></returns>
        public MapRectangle[,] Quarter()
        {
            var result = new MapRectangle[2, 2];
            var hMiddle = (MaxY + MinY) / 2;
            var vMiddle = (MaxX + MinX) / 2;
            result[0, 0] = new MapRectangle(MinX, MinY, hMiddle, vMiddle);
            result[0, 1] = new MapRectangle(MinX, vMiddle, hMiddle, MaxY);
            result[1, 0] = new MapRectangle(hMiddle, MinY, MaxX, vMiddle);
            result[1, 1] = new MapRectangle(hMiddle, vMiddle, MaxX, MaxY);
            return result;
        }

        public bool Equals(MapRectangle other)
        {
            return ((other._x1 == _x1) &&
                    (other._x2 == _x2) &&
                    (other._y1 == _y1) &&
                    (other._y2 == _y2));
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(MapRectangle))
                return false;
            return Equals((MapRectangle)obj);
        }

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                unchecked
                {
                    var hashCode = _x1.GetHashCode();
                    hashCode = (hashCode * 397) ^ _x2.GetHashCode();
                    hashCode = (hashCode * 397) ^ _y1.GetHashCode();
                    hashCode = (hashCode * 397) ^ _y2.GetHashCode();
                    _hashCode = hashCode;
                }
            }
            return _hashCode.Value;
        }

        public static bool operator ==(MapRectangle a, MapRectangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MapRectangle a, MapRectangle b)
        {
            return !a.Equals(b);
        }
    }
}
