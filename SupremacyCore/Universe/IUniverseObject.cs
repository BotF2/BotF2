// IUniverseObject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Universe
{
    /// <summary>
    /// Interface of all objects that exist in the game universe.
    /// </summary>
    public interface IUniverseObject : IGameObject
    {
        /// <summary>
        /// Gets the location of this <see cref="IUniverseObject"/>.
        /// </summary>
        /// <value>The location.</value>
        MapLocation Location { get; }

        /// <summary>
        /// Gets or sets the owner ID of this <see cref="IUniverseObject"/>.  This should be the
        /// CivID property of the owner Civilization.
        /// </summary>
        /// <value>The owner ID.</value>
        int OwnerID { get;}

        /// <summary>
        /// Gets the owner of this <see cref="IUniverseObject"/>.
        /// </summary>
        /// <value>The owner.</value>
        Civilization Owner { get; }
    }
}
