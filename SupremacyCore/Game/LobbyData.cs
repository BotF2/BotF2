// LobbyData.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Game
{
    public interface ILobbyData
    {
        GameMod GameMod { get; }

        /// <summary>
        /// Gets or sets the game options.
        /// </summary>
        /// <value>The game options.</value>
        GameOptions GameOptions { get; }

        /// <summary>
        /// Gets or sets the currently connected players.
        /// </summary>
        /// <value>The currently connected players.</value>
        Player[] Players { get; }

        /// <summary>
        /// Gets the unassigned players.
        /// </summary>
        /// <value>The unassigned players.</value>
        IEnumerable<Player> UnassignedPlayers { get; }

        /// <summary>
        /// Gets or sets the playable empires.
        /// </summary>
        /// <value>The playable empires.</value>
        string[] Empires { get; }

        bool IsMultiplayerGame { get; }

        /// <summary>
        /// Gets or sets the player slots.
        /// </summary>
        /// <value>The player slots.</value>
        PlayerSlot[] Slots { get; }
    }

    /// <summary>
    /// Contains the multiplayer lobby data used by the client and server.
    /// </summary>
    [Serializable]
    public sealed class LobbyData : ILobbyData
    {
        public bool IsMultiplayerGame { get; set; }

        public GameMod GameMod { get; set; }

        /// <summary>
        /// Gets or sets the game options.
        /// </summary>
        /// <value>The game options.</value>
        public GameOptions GameOptions { get; set; }

        /// <summary>
        /// Gets or sets the currently connected players.
        /// </summary>
        /// <value>The currently connected players.</value>
        public Player[] Players { get; set; }

        /// <summary>
        /// Gets the unassigned players.
        /// </summary>
        /// <value>The unassigned players.</value>
        public IEnumerable<Player> UnassignedPlayers => Players.Where(p => !Slots.Any(s => s.Player == p));

        /// <summary>
        /// Gets or sets the playable empires.
        /// </summary>
        /// <value>The playable empires.</value>
        public string[] Empires { get; set; }

        /// <summary>
        /// Gets or sets the player slots.
        /// </summary>
        /// <value>The player slots.</value>
        public PlayerSlot[] Slots { get; set; }
    }
}
