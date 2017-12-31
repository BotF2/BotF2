// CivilizationView.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;

namespace Supremacy.Entities
{
    /// <summary>
    /// Represents one civilization's view of another civilization in the game.
    /// </summary>
    [Serializable]
    public sealed class CivilizationView
    {
        private int _ownerId;
        private int _targetId;

        /// <summary>
        /// Gets the owner (source) civilization.
        /// </summary>
        /// <value>The owner (source) civilization.</value>
        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        /// <summary>
        /// Gets the target civilization.
        /// </summary>
        /// <value>The target civilization.</value>
        public Civilization Target
        {
            get { return GameContext.Current.Civilizations[_targetId]; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationView"/> class.
        /// </summary>
        /// <param name="owner">The owner (source) civilization.</param>
        /// <param name="target">The target civilization.</param>
        public CivilizationView(Civilization owner, Civilization target)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (target == null)
                throw new ArgumentNullException("target");
            _ownerId = owner.CivID;
            _targetId = target.CivID;
        }
    }
}
