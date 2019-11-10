// TechObjectDesignMap.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Tech
{
    /// <summary>
    /// Represents a collection of <see cref="TechObjectDesign"/>s of the specified
    /// design keyed by Design ID.
    /// </summary>
    /// <typeparam name="T">The design of TechObjectDesign.</typeparam>
    [Serializable]
    public sealed class TechObjectDesignMap<T> : Collections.KeyedCollectionBase<int, T>
        where T : TechObjectDesign
    {
        public TechObjectDesignMap() : base(o => o.DesignID) { }
    }
}
