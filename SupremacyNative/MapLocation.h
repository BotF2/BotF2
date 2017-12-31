// MapLocation.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

//#define Min(a, b) (((a) <= (b)) ? (a) : (b))
//#define Max(a, b) (((a) > (b)) ? (a) : (b))
//#define Abs(n) (((n) < 0) ? (-1 * (n)) : (n))

using namespace System;

namespace Supremacy
{
    namespace Universe
    {
        __forceinline static int Min(int a, int b)
        {
            return a <= b ? a : b;
        };

        __forceinline static int Max(int a, int b)
        {
            return a > b ? a : b;
        };

        __forceinline static int Abs(int a)
        {
            return (a < 0) ? -a : a;
        };

        [Serializable]
        public value class MapLocation : public IEquatable<MapLocation>, public IComparable<MapLocation>
        {
        public:
            literal int MaxValue = 255;
            literal int MinValue = 0;

            MapLocation(int x, int y)
            {
                if (x > MaxValue)
                    x = MaxValue;
                else if (x < MinValue)
                    x = MinValue;
                if (y > MaxValue)
                    y = MaxValue;
                else if (y < MinValue)
                    y = MinValue;
                _x = (unsigned char) x;
                _y = (unsigned char) y;
            }

        public:        
            __forceinline static double Area2(const MapLocation p0, const MapLocation p1, const MapLocation p2)
            {
                return (((p0._x * (p1._y - p2._y)) + (p1._x * (p2._y - p0._y))) + (p2._x * (p0._y - p1._y)));
            }

            __forceinline virtual int CompareTo(MapLocation other) sealed
            {
                int major = _x.CompareTo(other._x);
                return ((major != 0) ? major : _y.CompareTo(other._y));
            }

            __forceinline virtual bool Equals(MapLocation other) sealed
            {
                return ((other._x == _x) && (other._y == _y));
            }

            virtual bool Equals(Object^ obj) override
            {
                MapLocation^ other = dynamic_cast<MapLocation^>(obj);
                if (other != nullptr)
                {
                    return ((other->_x == _x) && (other->_y == _y));
                }
                return false;
            }

            __forceinline static int GetDistance(const MapLocation a, const MapLocation b)
            {
                return Max(Abs(a._x - b._x), Abs(a._y - b._y));
            }

            __forceinline static int GetDistanceSquared(const MapLocation a, const MapLocation b)
            {
                int distance = Max(Abs(a._x - b._x), Abs(a._y - b._y));
                return (distance * distance);
            }

            __forceinline static double GetEuclideanDistance(const MapLocation a, const MapLocation b)
            {
                return Math::Sqrt(((b._x - a._x) * (b._x - a._x)) + ((b._y - a._y) * (b._y - a._y)));
            }

            virtual int GetHashCode() override
            {
                return ((_x << 8) | _y);
            }

            __forceinline static bool operator ==(const MapLocation a, const MapLocation b)
            {
                return ((a._x == b._x) && (a._y == b._y));
            }

            __forceinline static bool operator !=(const MapLocation a, const MapLocation b)
            {
                return ((a._x != b._x) || (a._y != b._y));
            }

            virtual String^ ToString() override
            {
                return ("(" + _x + ", " + _y + ")");
            }

            property int X
            {
                __forceinline int get() { return _x; }
            }

            property int Y
            {
                __forceinline int get() { return _y; }
            }
            
        internal:
            unsigned initonly char _x;
            unsigned initonly char _y;
        };
    }
}
