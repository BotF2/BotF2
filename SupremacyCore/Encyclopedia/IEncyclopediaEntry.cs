// IEncyclopediaEntry.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Encyclopedia
{
    /// <summary>
    /// The interface for any object that appears in the in-game Encyclopedia.
    /// </summary>
    public interface IEncyclopediaEntry
    {
        /// <summary>
        /// Gets the heading displayed in the Encyclopedia index.
        /// </summary>
        /// <value>The heading.</value>
        string EncyclopediaHeading { get; }

        /// <summary>
        /// Gets the text displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The text.</value>
        string EncyclopediaText { get; }

        /// <summary>
        /// Gets the image displayed in the Encyclopedia entry.
        /// </summary>
        /// <value>The image.</value>
        string EncyclopediaImage { get; }

        /// <summary>
        /// Gets the encyclopedia category under which the entry appears.
        /// </summary>
        /// <value>The encyclopedia category.</value>
        EncyclopediaCategory EncyclopediaCategory { get; }
    }

    /// <summary>
    /// Defines the categories under which an entry can appear in the encyclopedia index.
    /// </summary>
    public enum EncyclopediaCategory
    {
        /// <summary>
        /// Entry does not appear in the encyclopedia index.
        /// </summary>
        None,
        /// <summary>
        /// Entry appears under the 'Stations' category.
        /// </summary>
        Stations,
        /// <summary>
        /// Entry appears under the 'Shipyards' category.
        /// </summary>
        Shipyards,
        /// <summary>
        /// Entry appears under the 'Batteries' category.
        /// </summary>
        Batteries,
        /// <summary>
        /// Entry appears under the 'Ships' category.
        /// </summary>
        Ships,
        /// <summary>
        /// Entry appears under the 'Civilizations' category.
        /// </summary>
        Civilizations,
        /// <summary>
        /// Entry appears under the 'Races' category.
        /// </summary>
        Races,
        /// <summary>
        /// Entry appears under the 'Buildings' category.
        /// </summary>
        Buildings,
        /// <summary>
        /// Entry appears under the 'Facilites' category.
        /// </summary>
        Facilites,


    }
}
