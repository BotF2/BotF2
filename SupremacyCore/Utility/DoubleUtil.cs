using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Supremacy.Utility
{
    public static class DoubleUtil
    {
        // Const values come from sdk\inc\crt\float.h
        public const double Epsilon = 2.2204460492503131e-016; /* smallest such that 1.0+Epsilon != 1.0 */

        public const float MinFloatValue = 1.175494351e-38F;
        /* Number close to zero, where float.MinValue is -float.MaxValue */

        /// <summary>
        /// AreClose - Determines whether or not two doubles are "close".  That is, whether or 
        /// not they are within epsilon of each other.  Note that this epsilon is proportional
        /// to the numbers themselves to that AreClose survives scalar multiplication.
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> and <paramref name="value2"/> are very close; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        public static bool AreClose(double value1, double value2)
        {
            /*
             * In case they are Infinities (then epsilon check does not work)
             */
            if (value1 == value2)
                return true;

            /*
             * This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < Epsilon
             */
            double epsilon = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * Epsilon;
            double delta = value1 - value2;

            return (-epsilon < delta) && (epsilon > delta);
        }

        /// <summary>
        /// LessThan - Determines whether or not the first double is less than the second double.
        /// That is, whether or not the first is strictly less than *and* not within epsilon of
        /// the other number.  Note that this epsilon is proportional to the numbers themselves
        /// to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is less than <paramref name="value2"/>; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        public static bool LessThan(double value1, double value2)
        {
            return (value1 < value2) && !AreClose(value1, value2);
        }

        /// <summary>
        /// GreaterThan - Determines whether or not the first double is greater than the second double.
        /// That is, whether or not the first is strictly greater than *and* not within epsilon of
        /// the other number.  Note that this epsilon is proportional to the numbers themselves
        /// to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is greater than <paramref name="value2"/>; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        public static bool GreaterThan(double value1, double value2)
        {
            return (value1 > value2) && !AreClose(value1, value2);
        }

        /// <summary>
        /// LessThanOrClose - Determines whether or not the first double is less than or close to
        /// the second double.  That is, whether or not the first is strictly less than or within
        /// epsilon of the other number.  Note that this epsilon is proportional to the numbers 
        /// themselves to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is less than or very close to <paramref name="value2"/>; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        public static bool LessThanOrClose(double value1, double value2)
        {
            return (value1 < value2) || AreClose(value1, value2);
        }

        /// <summary>
        /// GreaterThanOrClose - Determines whether or not the first double is greater than or close to
        /// the second double.  That is, whether or not the first is strictly greater than or within
        /// epsilon of the other number.  Note that this epsilon is proportional to the numbers 
        /// themselves to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is greater than or very close to <paramref name="value2"/>; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value1">The first double to compare.</param>
        /// <param name="value2">The second double to compare.</param>
        public static bool GreaterThanOrClose(double value1, double value2)
        {
            return (value1 > value2) || AreClose(value1, value2);
        }

        /// <summary>
        /// IsOne - Determines whether or not the double is "close" to <c>1</c>.  Same as AreClose(<c>n</c>, <c>1</c>),
        /// but this is faster.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is very close to <c>1</c>; otherwise, <c>false</c>.
        /// .</returns>
        /// <param name="value">The value to compare to <c>1</c>.</param>
        public static bool IsOne(double value)
        {
            return Math.Abs(value - 1.0) < 10.0 * Epsilon;
        }

        /// <summary>
        /// Compares two points for fuzzy equality.  This function
        /// helps compensate for the fact that double values can 
        /// acquire error when operated upon
        /// </summary>
        /// <param name='point1'>The first point to compare.</param>
        /// <param name='point2'>The second point to compare.</param>
        /// <returns>Whether or not the two points are equal.</returns>
        public static bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) &&
                   AreClose(point1.Y, point2.Y);
        }

        /// <summary>
        /// IsZero - Determines whether or not the double is "close" to 0.  Same as AreClose(double, 0),
        /// but this is faster.
        /// </summary>
        /// <returns>
        /// bool - the result of the AreClose comparision.
        /// .</returns>
        /// <param name="value">The double to compare to 0.</param>
        public static bool IsZero(double value)
        {
            return Math.Abs(value) < 10.0 * Epsilon;
        }

        /*
         * The Point, Size, Rect and Matrix class have moved to WinCorLib.  However, we provide
         * public AreClose methods for our own use here.
         */

        /// <summary>
        /// Compares two Size instances for fuzzy equality.  This function
        /// helps compensate for the fact that double values can 
        /// acquire error when operated upon
        /// </summary>
        /// <param name='size1'>The first size to compare.</param>
        /// <param name='size2'>The second size to compare.</param>
        /// <returns>Whether or not the two Size instances are equal.</returns>
        public static bool AreClose(Size size1, Size size2)
        {
            return AreClose(size1.Width, size2.Width) &&
                   AreClose(size1.Height, size2.Height);
        }

        /// <summary>
        /// Compares two Vector instances for fuzzy equality.  This function
        /// helps compensate for the fact that double values can 
        /// acquire error when operated upon
        /// </summary>
        /// <param name='vector1'>The first Vector to compare.</param>
        /// <param name='vector2'>The second Vector to compare.</param>
        /// <returns>Whether or not the two Vector instances are equal.</returns>
        public static bool AreClose(Vector vector1, Vector vector2)
        {
            return AreClose(vector1.X, vector2.X) &&
                   AreClose(vector1.Y, vector2.Y);
        }

        /// <summary>
        /// Compares two rectangles for fuzzy equality.  This function
        /// helps compensate for the fact that double values can 
        /// acquire error when operated upon
        /// </summary>
        /// <param name='rect1'>The first rectangle to compare.</param>
        /// <param name='rect2'>The second rectangle to compare.</param>
        /// <returns>Whether or not the two rectangles are equal.</returns>
        public static bool AreClose(Rect rect1, Rect rect2)
        {
            /*
             * If they're both empty, don't bother with the double logic.
             */
            if (rect1.IsEmpty)
                return rect2.IsEmpty;

            /* 
             * At this point, rect1 isn't empty, so the first thing we can test is
             * rect2.IsEmpty, followed by property-wise compares.
             */
            return !rect2.IsEmpty &&
                   AreClose(rect1.X, rect2.X) &&
                   AreClose(rect1.Y, rect2.Y) &&
                   AreClose(rect1.Height, rect2.Height) &&
                   AreClose(rect1.Width, rect2.Width);
        }

        /// <summary>
        /// Determines whether a value is between <c>0</c> and <c>1</c>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is between <c>0</c> and <c>1</c>; otherwise, <c>false</c>.</returns>
        public static bool IsBetweenZeroAndOne(double value)
        {
            return (GreaterThanOrClose(value, 0) && LessThanOrClose(value, 1));
        }

        /// <summary>
        /// Rounds a <see cref="double"/> value to an <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="double"/> value to round.</param>
        /// <returns>The rounded <see cref="int"/> value.</returns>
        public static int DoubleToInt(double value)
        {
            return (0 < value) ? (int)(value + 0.5) : (int)(value - 0.5);
        }

        /// <summary>
        /// Determines whether a <see cref="Rect"/> has <see cref="Rect.X"/>, <see cref="Rect.Y"/>,
        /// <see cref="Rect.Width"/>or <see cref="Rect.Height"/> as <see cref="double.NaN"/>.
        /// </summary>
        /// <param name='r'>The rectangle to test.</param>
        /// <returns><c>true</c> is the <see cref="Rect"/> has any <see cref="double.NaN"/> values.</returns>
        public static bool RectHasNaN(Rect r)
        {
            return IsNaN(r.X) ||
                   IsNaN(r.Y) ||
                   IsNaN(r.Height) ||
                   IsNaN(r.Width);
        }

        /// <summary>
        /// Determines whether the specified value is <see cref="double.NaN"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is <see cref="double.NaN"/>; otherwise, <c>false</c>.
        /// .</returns>
        /// <remarks>
        /// The standard CLR double.IsNaN() function is approximately 100 times slower than our own wrapper,
        /// so please make sure to use DoubleUtil.IsNaN() in performance sensitive code.
        /// 
        /// IEEE 754: If the argument is any value in the range <c>0x7ff0000000000001L</c>
        /// through <c>0x7fffffffffffffffL</c> or in the range <c>0xfff0000000000001L</c>
        /// through <c>0xffffffffffffffffL</c>, then the result will be <see cref="double.NaN"/>.
        /// </remarks>
        public static bool IsNaN(double value)
        {
            NanUnion nanUnion = new NanUnion { DoubleValue = value };

            ulong exponent = nanUnion.UintValue & 0xfff0000000000000;
            ulong mantissa = nanUnion.UintValue & 0x000fffffffffffff;

            return ((exponent == 0x7ff0000000000000) || (exponent == 0xfff0000000000000)) &&
                   (mantissa != 0);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)] public double DoubleValue;
            [FieldOffset(0)] public ulong UintValue;
        }
    }
}