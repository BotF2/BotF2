// IBuildable.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Tech;

namespace Supremacy.Economy
{
    /// <summary>
    /// Interface implemented by all items that can be built in the game.
    /// </summary>
    public interface IBuildable
    {
        /// <summary>
        /// Gets the resources costs required for construction.
        /// </summary>
        /// <value>The resource costs.</value>
        ResourceValueCollection BuildResourceCosts { get; }

        /// <summary>
        /// Gets the industry build cost required for construction.
        /// </summary>
        /// <value>The industry build cost.</value>
        int BuildCost { get; }

        /// <summary>
        /// Gets the tech levels required for construction.
        /// </summary>
        /// <value>The tech level requirements.</value>
        TechLevelCollection TechRequirements { get; }
    }
}
