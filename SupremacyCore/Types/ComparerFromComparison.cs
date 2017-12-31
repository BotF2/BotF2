// ComparerFromComparison.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Supremacy.Types
{
    /// <summary>
    /// Creates an <see cref="IComparer&lt;T&gt;"/> implementation that wraps a <see cref="Comparison&lt;T&gt;"/>;
    /// </summary>
    [Serializable]
    public sealed class ComparerFromComparison<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        /// <summary>
        /// Initializes a new <see cref="ComparerFromComparison&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparison">The <see cref="Comparison&lt;T&gt;"/> to be wrapped.</param>
        public ComparerFromComparison(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");
            _comparison = comparison;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Value</term>
        ///     <description>Condition</description>
        ///   </listheader>
        ///   <item>
        ///     <term>Less than Zero</term>
        ///     <description><paramref name="x"/> is less than <paramref name="y"/>.</description>
        ///   </item>
        ///   <item>
        ///     <term>Zero</term>
        ///     <description><paramref name="x"/> equals <paramref name="y"/>.</description>
        ///   </item>
        ///   <item>
        ///     <term>Greater than Zero</term>
        ///     <description><paramref name="x"/> is greater than <paramref name="y"/>.</description>
        ///   </item>
        /// </list>
        /// </returns>
        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }
    }
}