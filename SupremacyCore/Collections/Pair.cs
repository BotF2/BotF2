using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Supremacy.Collections
{
    /// <summary>
    /// Stores a pair of objects within a single struct. This struct is useful to use as the
    /// T of a collection, or as the TKey or TValue of a dictionary.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct Pair<TFirst, TSecond> : IPair<TFirst, TSecond>, IComparable<Pair<TFirst, TSecond>>
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
        private readonly TFirst _first;

        /// <summary>
        /// The second element of the pair.
        /// </summary>
        private readonly TSecond _second;

        /// <summary>
        /// Creates a new pair with given first and second elements.
        /// </summary>
        /// <param name="first">The first element of the pair.</param>
        /// <param name="second">The second element of the pair.</param>
        public Pair(TFirst first, TSecond second)
        {
            _first = first;
            _second = second;
        }

        /// <summary>
        /// Creates a new pair using elements from a KeyValuePair structure. The
        /// First element gets the Key, and the Second elements gets the Value.
        /// </summary>
        /// <param name="keyAndValue">The KeyValuePair to initialize the Pair with .</param>
        public Pair(KeyValuePair<TFirst, TSecond> keyAndValue)
        {
            _first = keyAndValue.Key;
            _second = keyAndValue.Value;
        }

        /// <summary>
        /// The first element of the pair.
        /// </summary>
        public TFirst First
        {
            get { return _first; }
        }

        /// <summary>
        /// The second element of the pair.
        /// </summary>
        public TSecond Second
        {
            get { return _second; }
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
            if (obj == null || !(obj is Pair<TFirst, TSecond>))
                return false;

            return Equals((Pair<TFirst, TSecond>)obj);
        }

        /// <summary>
        /// Determines if this pair is equal to another pair. The pair is equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="other">Pair to compare with for equality.</param>
        /// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
        public bool Equals(Pair<TFirst, TSecond> other)
        {
            return _firstEqualityComparer.Equals(_first, other._first) &&
                   _secondEqualityComparer.Equals(_second, other._second);
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
            var hashFirst = ReferenceEquals(_first, null) ? FirstNullHash : _first.GetHashCode();
            var hashSecond = ReferenceEquals(_second, null) ? SecondNullHash : _second.GetHashCode();
            return hashFirst ^ hashSecond;
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            var otherPair = other as IPair<TFirst, TSecond>;
            if (otherPair == null)
            {
                throw new ArgumentException(
                    string.Format(
                        SR.ArgumentException_IncorrectType,
                        typeof(IPair<TFirst, TSecond>).FullName),
                    "other");
            }

            var first = comparer.Compare(_first, otherPair.First);
            if (first != 0)
                return first;

            return comparer.Compare(_second, otherPair.Second);
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
        public int CompareTo(Pair<TFirst, TSecond> other)
        {
            try
            {
                var firstCompare = _firstComparer.Compare(_first, other._first);
                if (firstCompare != 0)
                    return firstCompare;
                return _secondComparer.Compare(_second, other._second);
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
            if (obj is Pair<TFirst, TSecond>)
                return CompareTo((Pair<TFirst, TSecond>)obj);
            throw new ArgumentException(SR.ArgumentException_BadComparandType, "obj");
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
                ReferenceEquals(_first, null) ? "null" : _first.ToString(),
                ReferenceEquals(_second, null) ? "null" : _second.ToString());
        }

        /// <summary>
        /// Determines if two pairs are equal. Two pairs are equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="pair1">First pair to compare.</param>
        /// <param name="pair2">Second pair to compare.</param>
        /// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
        public static bool operator ==(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
        {
            return _firstEqualityComparer.Equals(pair1._first, pair2._first) &&
                   _secondEqualityComparer.Equals(pair1._second, pair2._second);
        }

        /// <summary>
        /// Determines if two pairs are not equal. Two pairs are equal if  the first and second elements
        /// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
        /// </summary>
        /// <param name="pair1">First pair to compare.</param>
        /// <param name="pair2">Second pair to compare.</param>
        /// <returns>True if the pairs are not equal. False if the pairs are equal.</returns>
        public static bool operator !=(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
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
        public static explicit operator KeyValuePair<TFirst, TSecond>(Pair<TFirst, TSecond> pair)
        {
            return new KeyValuePair<TFirst, TSecond>(pair._first, pair._second);
        }

        /// <summary>
        /// Converts this Pair to a KeyValuePair. The Key part of the KeyValuePair gets
        /// the First element, and the Value part of the KeyValuePair gets the Second 
        /// elements.
        /// </summary>
        /// <returns>The KeyValuePair created from this Pair.</returns>
        public KeyValuePair<TFirst, TSecond> ToKeyValuePair()
        {
            return new KeyValuePair<TFirst, TSecond>(_first, _second);
        }

        /// <summary>
        /// Converts a KeyValuePair structure into a Pair. The
        /// First element gets the Key, and the Second element gets the Value.
        /// </summary>
        /// <param name="keyAndValue">The KeyValuePair to convert.</param>
        /// <returns>The Pair created by converted the KeyValuePair into a Pair.</returns>
        public static explicit operator Pair<TFirst, TSecond>(KeyValuePair<TFirst, TSecond> keyAndValue)
        {
            return new Pair<TFirst, TSecond>(keyAndValue);
        }

        #region Implementation of IStructuralEquatable
        public bool Equals(object other, IEqualityComparer comparer)
        {
            var otherPair = other as IPair<TFirst, TSecond>;
            if (otherPair == null)
            {
                throw new ArgumentException(
                    string.Format(
                        SR.ArgumentException_IncorrectType,
                        typeof(IPair<TFirst, TSecond>).FullName),
                    "other");
            }

            return comparer.Equals(_first, otherPair.Second) &&
                   comparer.Equals(_second, otherPair.Second);
        }

        public int GetHashCode(IEqualityComparer comparer)
        {
            return CombineHashCodes(
                comparer.GetHashCode(_first),
                comparer.GetHashCode(_second));
        }

        private static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
        #endregion
    }
}