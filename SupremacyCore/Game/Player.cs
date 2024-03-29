// Player.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Entities;

namespace Supremacy.Game
{
    public interface IPlayer : IEquatable<IPlayer>, ICivIdentity
    {
        /// <summary>
        /// Gets or sets the player ID.
        /// </summary>
        /// <value>The player ID.</value>
        int PlayerID { get; }

        /// <summary>
        /// Gets or sets the player name.
        /// </summary>
        /// <value>The player name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the player is human.
        /// </summary>
        /// <value>
        /// <c>true</c> if the player is human; <c>false</c> if the player is AI-controlled.
        /// </value>
        bool IsHumanPlayer { get; }

        /// <summary>
        /// Gets or sets the selected empire ID.
        /// </summary>
        /// <value>The selected empire ID.</value>
        int EmpireID { get; }

        /// <summary>
        /// Gets or sets the selected empire.
        /// </summary>
        /// <value>The selected empire.</value>
        Civilization Empire { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Player"/> is the game host.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Player"/> is the game host; otherwise, <c>false</c>.
        /// </value>
        bool IsGameHost { get; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="Player"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="Player"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        string ToString();

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Player"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        int GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="Player"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="Player"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="Player"/>; otherwise, false.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        bool Equals(object obj);
    }

    /// <summary>
    /// Represents a player in the game.
    /// </summary>
    [Serializable]
    public sealed class Player : IPlayer, IEquatable<Player>, ICivIdentity
    {
        /// <summary>
        /// The value of <see cref="PlayerID"/> for the game host.
        /// </summary>
        public const int GameHostID = 0;

        /// <summary>
        /// The value of the <see cref="PlayerID"/> for the "Unassigned" player.
        /// </summary>
        public const int UnassignedPlayerID = -1;

        /// <summary>
        /// The value of the <see cref="PlayerID"/> for the "Computer" player.
        /// </summary>
        public const int ComputerPlayerID = -2;

        /// <summary>
        /// The value of the <see cref="PlayerID"/> for the "TurnedToMinor" player.
        /// </summary>
        public const int TurnedToMinorPlayerID = -3;

        /// <summary>
        /// The value of the <see cref="PlayerID"/> for the "TurnedToExpandingPower" player.
        /// </summary>
        public const int TurnedToExpandingPowerPlayerID = -4;

        /// <summary>
        /// The "Unassigned" player.
        /// </summary>
        public static readonly Player Unassigned = new Player { Name = "Unassigned", PlayerID = UnassignedPlayerID };

        /// <summary>
        /// The "Computer" player.
        /// </summary>
        public static readonly Player Computer = new Player { Name = "Computer", PlayerID = ComputerPlayerID };

        /// <summary>
        /// The "TurnedToMinor" player.
        /// </summary>
        public static readonly Player TurnedToMinor = new Player { Name = "TurnedToMinor", PlayerID = TurnedToMinorPlayerID };

        /// <summary>
        /// The "TurnedToExpandingPower" player.
        /// </summary>
        public static readonly Player TurnedToExpandingPower = new Player { Name = "TurnedToExpandingPower", PlayerID = TurnedToExpandingPowerPlayerID };

        /// <summary>
        /// The value of <see cref="EmpireID"/> if an empire has not been selected.
        /// </summary>
        public const int InvalidEmpireID = -1;

        private int _playerId;
        private string _name;
        private int _empireId = InvalidEmpireID;

        /// <summary>
        /// Gets or sets the player ID.
        /// </summary>
        /// <value>The player ID.</value>
        public int PlayerID
        {
            get => _playerId;
            set => _playerId = value;
        }

        /// <summary>
        /// Gets or sets the player name.
        /// </summary>
        /// <value>The player name.</value>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Gets a value indicating whether the player is human.
        /// </summary>
        /// <value>
        /// <c>true</c> if the player is human; <c>false</c> if the player is AI-controlled.
        /// </value>
        public bool IsHumanPlayer => _playerId >= GameHostID;

        /// <summary>
        /// Gets or sets the selected empire ID.
        /// </summary>
        /// <value>The selected empire ID.</value>
        public int EmpireID
        {
            get => _empireId;
            set => _empireId = value;
        }

        /// <summary>
        /// Gets or sets the selected empire.
        /// </summary>
        /// <value>The selected empire.</value>
        public Civilization Empire
        {
            get
            {
                if (GameContext.Current != null)
                {
                    return GameContext.Current.Civilizations[EmpireID];
                }
                else
                {
                    return PlayerContext.Current.Players[0].Empire;
                }

                // ToDo: solve the crashes out of Multi Player Start
            }
            set
            {
                if (value == null)
                {
                    EmpireID = Civilization.InvalidID;
                    return;
                }
                EmpireID = value.CivID;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Player"/> is the game host.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Player"/> is the game host; otherwise, <c>false</c>.
        /// </value>
        public bool IsGameHost => _playerId == GameHostID;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="Player"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="Player"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override string ToString()
        {
            return Name ?? base.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IPlayer other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return other.PlayerID == _playerId;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Player other)
        {
            return Equals((IPlayer)other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as IPlayer);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return _playerId;
        }

        public static bool operator ==(Player left, Player right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Player left, Player right)
        {
            return !Equals(left, right);
        }

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID => _empireId;

        #endregion
    }
}
