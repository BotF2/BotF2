// OfficerExperienceCategory.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Personnel
{
    /// <summary>
    /// Describes the various categories of experience ratings for Officers.
    /// </summary>
    public enum OfficerExperienceCategory : byte
    {
        /// <summary>
        /// Describes an officer's loyalty to king and country.
        /// </summary>
        Loyalty = 0,

        /// <summary>
        /// Describes an officer's ability to infiltrate enemy ranks or move undetected.
        /// </summary>
        Stealth = 1,

        /// <summary>
        /// Describes an officer's ability to coordinate efforts with others (e.g. other ship captains,
        /// other spies in joint missions, etc.)
        /// </summary>
        Leadership = 2,

        /// <summary>
        /// Describes an officer's charisma--a useful trait for diplomacy, and potentially useful for
        /// a spy who has been caught.
        /// </summary>
        Charisma = 3,

        /// <summary>
        /// Describes an officer's luck--good luck can prove useful in any field of endeavour.
        /// </summary>
        Luck = 4,

        /// <summary>
        /// Describes an officer's cunning--the ability to carry out undercover missions, turn the tides
        /// of battle, or perhaps even guide diplomatic negotiations.
        /// </summary>
        Cunning = 5
    }
}