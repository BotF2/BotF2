// PlayerContext.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq;

using Supremacy.Collections;
using Supremacy.Entities;

namespace Supremacy.Game
{
    public class PlayerContext
    {
        #region Fields
        private readonly IIndexedCollection<Player> _players;
        #endregion

        #region Constructors
        public PlayerContext(IIndexedCollection<Player> players)
        {
            _players = players ?? throw new ArgumentNullException("players");
        }
        #endregion

        #region Properties
        public static PlayerContext Current { get; set; }

        public IIndexedCollection<Player> Players => _players;
        #endregion

        #region Methods
        public bool IsHumanPlayer(ICivIdentity civ)
        {
            return _players.Any(player => player.Empire.CivID == civ.CivID);
        }
        #endregion
    }
}