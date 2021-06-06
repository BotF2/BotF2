using System;

namespace Supremacy.Universe
{
    [Serializable]
    public struct MapLocation : IEquatable<MapLocation>, IComparable<MapLocation>
    {
        private readonly int _x;
        private readonly int _y;
        public const int MinValue = 0;
        public const int MaxValue = 255;

        public int X => _x;
        public int Y => _y;

        public MapLocation(int x, int y)
        {
            if (x > MaxValue)
                x = MaxValue;
            else if (x < MinValue)
                x = MinValue;
            if (y > MaxValue)
                y = MaxValue;
            else if (y < MinValue)
                y = MinValue;
            _x = x;
            _y = y;
        }

        public static double Area2(MapLocation p0, MapLocation p1, MapLocation p2)
        {
            return (((p0._x * (p1._y - p2._y)) + (p1._x * (p2._y - p0._y))) + (p2._x * (p0._y - p1._y)));
        }

        public static int GetDistance(MapLocation a, MapLocation b)
        {
            return Math.Max(Math.Abs(a._x - b._x), Math.Abs(a._y - b._y));
        }

        public static int GetDistanceSquared(MapLocation a, MapLocation b)
        {
            int distance = Math.Max(Math.Abs(a._x - b._x), Math.Abs(a._y - b._y));
            return (distance * distance);
        }

        public static double GetEuclideanDistance(MapLocation a, MapLocation b)
        {
            return Math.Sqrt(((b._x - a._x) * (b._x - a._x)) + ((b._y - a._y) * (b._y - a._y)));
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", _x, _y);
        }

        public override int GetHashCode()
        {
            return ((_x << 8) | _y);
        }

        public bool Equals(MapLocation other)
        {
            return (other._x == _x) && (other._y == _y);
        }

        public override bool Equals(Object obj)
        {
            MapLocation other = (MapLocation)obj;
            if (other != null)
            {
                return (other._x == _x) && (other._y == _y);
            }
            return false;
        }

        public int CompareTo(MapLocation other)
        {
            int major = _x.CompareTo(other._x);
            return (major != 0) ? major : _y.CompareTo(other._y);
        }

        public static bool operator ==(MapLocation a, MapLocation b)
        {
            return (a._x == b._x) && (a._y == b._y);
        }

        public static bool operator !=(MapLocation a, MapLocation b)
        {
            return (a._x != b._x) || (a._y != b._y);
        }
    }
}
