using System;
using System.Collections;

namespace Supremacy.Collections
{
    public interface IPair<out TFirst, out TSecond> : IComparable, IStructuralComparable, IStructuralEquatable
    {
        /// <summary>
        /// The first element of the pair.
        /// </summary>
        TFirst First { get; }

        /// <summary>
        /// The second element of the pair.
        /// </summary>
        TSecond Second { get; }
    }
}