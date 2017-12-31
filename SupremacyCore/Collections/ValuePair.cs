// ValuePair.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Supremacy.Collections
{
    /// <summary>
    /// Stores a pair of objects within a single struct. This struct is useful to use as the
    /// T of a collection, or as the TKey or TValue of a dictionary.
    /// </summary>
    [Serializable]
    public struct ValuePair<TFirst, TSecond> : IComparable, IComparable<ValuePair<TFirst, TSecond>>
    {
        private const int FirstNullHash = 0x61E04917;
        private const int SecondNullHash = 0x198ED6A3;

        private static readonly IComparer<TFirst> _firstComparer = Comparer<TFirst>.Default;
        private static readonly IComparer<TSecond> _secondComparer = Comparer<TSecond>.Default;

        private static readonly IEqualityComparer<TFirst> _firstEqualityComparer = EqualityComparer<TFirst>.Default;
        private static readonly IEqualityComparer<TSecond> _secondEqualityComparer = EqualityComparer<TSecond>.Default;

        /// <summary>
        /// The first element of the pair.
        /// </summary>
        public TFirst First;

        /// <summary>
        /// The second element of the pair.
        /// </summary>
        public TSecond Second;

        /// <summary>
        /// Creates a new pair with given first and second elements.
        /// </summary>
        /// <param name="first">The first element of the pair.</param>
        /// <param name="second">The second element of the pair.</param>
        public ValuePair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        /// <summary>
        /// Creates a new pair using elements from a KeyValuePair structure. The
        /// First element gets the Key, and the Second elements gets the Value.
        /// </summary>
        /// <param name="keyAndValue">The KeyValuePair to initialize the Pair with .</param>
        public ValuePair(KeyValuePair<TFirst, TSecond> keyAndValue)
        {
            First = keyAndValue.Key;
            Second = keyAndValue.Value;
        }

        public static ValuePair<TFirst, TSecond> Create(TFirst first, TSecond second)
        {
            return new ValuePair<TFirst, TSecond>(first, second);
        }

        /// <summary>
        /// Determines if this pair is equal to another object. The pair is equal to another object 
        /// if that object is a Pair, both element types are the same, and the first and second elements
        /// both compare equal using object.Equals.
        /// </summary>
        /// <param name="obj">Object to compare for equality.</param>
        /// <returns>True if the objects are equal. False if the objects are not equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ValuePair<TFirst, TSecond>))
                return false;

            return Equals((ValuePair<TFirst, TSecond>)obj);
        }

        /// <summary>
        /// Determines if this pair is equal to another pair. The pair is equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="other">Pair to compare with for equality.</param>
        /// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
        public bool Equals(ValuePair<TFirst, TSecond> other)
        {
            return _firstEqualityComparer.Equals(First, other.First) &&
                   _secondEqualityComparer.Equals(Second, other.Second);
        }

        /// <summary>
        /// Returns a hash code for the pair, suitable for use in a hash-table or other hashed collection.
        /// Two pairs that compare equal (using Equals) will have the same hash code. The hash code for
        /// the pair is derived by combining the hash codes for each of the two elements of the pair.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // Build the hash code from the hash codes of First and Second. 
            var hashFirst = ReferenceEquals(First, null) ? FirstNullHash : First.GetHashCode();
            var hashSecond = ReferenceEquals(Second, null) ? SecondNullHash : Second.GetHashCode();
            return hashFirst ^ hashSecond;
        }

        /// <summary>
        /// <para> Compares this pair to another pair of the some type. The pairs are compared by using
        /// the IComparable&lt;T&gt; or IComparable interface on TFirst and TSecond. The pairs
        /// are compared by their first elements first, if their first elements are equal, then they
        /// are compared by their second elements.</para>
        /// <para>If either TFirst or TSecond does not implement IComparable&lt;T&gt; or IComparable, then
        /// an NotSupportedException is thrown, because the pairs cannot be compared.</para>
        /// </summary>
        /// <param name="other">The pair to compare to.</param>
        /// <returns>An integer indicating how this pair compares to <paramref name="other"/>. Less
        /// than zero indicates this pair is less than <paramref name="other"/>. Zero indicate this pair is
        /// equals to <paramref name="other"/>. Greater than zero indicates this pair is greater than
        /// <paramref name="other"/>.</returns>
        /// <exception cref="NotSupportedException">Either FirstSecond or TSecond is not comparable
        /// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
        public int CompareTo(ValuePair<TFirst, TSecond> other)
        {
            try
            {
                var firstCompare = _firstComparer.Compare(First, other.First);
                if (firstCompare != 0)
                    return firstCompare;
                return _secondComparer.Compare(Second, other.Second);
            }
            catch (ArgumentException)
            {
                // Determine which type caused the problem for a better error message.
                if (!typeof(IComparable<TFirst>).IsAssignableFrom(typeof(TFirst)) &&
                    !typeof(IComparable).IsAssignableFrom(typeof(TFirst)))
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Uncomparable type: {0}",
                            typeof(TSecond).FullName));
                }
                if (!typeof(IComparable<TSecond>).IsAssignableFrom(typeof(TSecond)) &&
                    !typeof(IComparable).IsAssignableFrom(typeof(TSecond)))
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Uncomparable type: {0}",
                            typeof(TSecond).FullName));
                }
                throw;
            }
        }

        /// <summary>
        /// <para> Compares this pair to another pair of the some type. The pairs are compared by using
        /// the IComparable&lt;T&gt; or IComparable interface on TFirst and TSecond. The pairs
        /// are compared by their first elements first, if their first elements are equal, then they
        /// are compared by their second elements.</para>
        /// <para>If either TFirst or TSecond does not implement IComparable&lt;T&gt; or IComparable, then
        /// an NotSupportedException is thrown, because the pairs cannot be compared.</para>
        /// </summary>
        /// <param name="obj">The pair to compare to.</param>
        /// <returns>An integer indicating how this pair compares to <paramref name="obj"/>. Less
        /// than zero indicates this pair is less than <paramref name="obj"/>. Zero indicate this pair is
        /// equals to <paramref name="obj"/>. Greater than zero indicates this pair is greater than
        /// <paramref name="obj"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not of the correct type.</exception>
        /// <exception cref="NotSupportedException">Either FirstSecond or TSecond is not comparable
        /// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
        int IComparable.CompareTo(object obj)
        {
            if (obj is ValuePair<TFirst, TSecond>)
                return CompareTo((ValuePair<TFirst, TSecond>)obj);
            throw new ArgumentException("Bad comparand type.", "obj");
        }

        /// <summary>
        /// Returns a string representation of the pair. The string representation of the pair is
        /// of the form:
        /// <c>First: {0}, Second: {1}</c>
        /// where {0} is the result of First.ToString(), and {1} is the result of Second.ToString() (or
        /// "null" if they are null.)
        /// </summary>
        /// <returns> The string representation of the pair.</returns>
        public override string ToString()
        {
            return string.Format(
                "First: {0}, Second: {1}",
                ReferenceEquals(First, null) ? "null" : First.ToString(),
                ReferenceEquals(Second, null) ? "null" : Second.ToString());
        }

        /// <summary>
        /// Determines if two pairs are equal. Two pairs are equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="pair1">First pair to compare.</param>
        /// <param name="pair2">Second pair to compare.</param>
        /// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
        public static bool operator ==(ValuePair<TFirst, TSecond> pair1, ValuePair<TFirst, TSecond> pair2)
        {
            return _firstEqualityComparer.Equals(pair1.First, pair2.First) &&
                   _secondEqualityComparer.Equals(pair1.Second, pair2.Second);
        }

        /// <summary>
        /// Determines if two pairs are not equal. Two pairs are equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="pair1">First pair to compare.</param>
        /// <param name="pair2">Second pair to compare.</param>
        /// <returns>True if the pairs are not equal. False if the pairs are equal.</returns>
        public static bool operator !=(ValuePair<TFirst, TSecond> pair1, ValuePair<TFirst, TSecond> pair2)
        {
            return !(pair1 == pair2);
        }

        /// <summary>
        /// Converts a Pair to a KeyValuePair. The Key part of the KeyValuePair gets
        /// the First element, and the Value part of the KeyValuePair gets the Second 
        /// elements.
        /// </summary>
        /// <param name="pair">Pair to convert.</param>
        /// <returns>The KeyValuePair created from <paramref name="pair"/>.</returns>
        public static explicit operator KeyValuePair<TFirst, TSecond>(ValuePair<TFirst, TSecond> pair)
        {
            return new KeyValuePair<TFirst, TSecond>(pair.First, pair.Second);
        }

        /// <summary>
        /// Converts this Pair to a KeyValuePair. The Key part of the KeyValuePair gets
        /// the First element, and the Value part of the KeyValuePair gets the Second 
        /// elements.
        /// </summary>
        /// <returns>The KeyValuePair created from this Pair.</returns>
        public KeyValuePair<TFirst, TSecond> ToKeyValuePair()
        {
            return new KeyValuePair<TFirst, TSecond>(First, Second);
        }

        /// <summary>
        /// Converts a KeyValuePair structure into a Pair. The
        /// First element gets the Key, and the Second element gets the Value.
        /// </summary>
        /// <param name="keyAndValue">The KeyValuePair to convert.</param>
        /// <returns>The Pair created by converted the KeyValuePair into a Pair.</returns>
        public static explicit operator ValuePair<TFirst, TSecond>(KeyValuePair<TFirst, TSecond> keyAndValue)
        {
            return new ValuePair<TFirst, TSecond>(keyAndValue);
        }
    }
}