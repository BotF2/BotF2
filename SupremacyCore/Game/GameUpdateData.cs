// GameUpdateData.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.IO.Serialization;
using Supremacy.Universe;

namespace Supremacy.Game
{
    /// <summary>
    /// Contains the data sent to the clients at the beginning of each turn.
    /// </summary>
    [Serializable]
    public class GameUpdateData : IOwnedDataSerializableAndRecreatable
    {
        #region Fields
        private int _turnNumber = 1;
        private CivilizationManager[] _civManagers;
        private UniverseObjectSet _objects;
        private SectorClaimGrid _sectorClaims;
        private CivilizationPairedMap<IDiplomacyData> _diplomacyData;
        private Diplomat[] _diplomats;
        private AgreementMatrix _agreementMatrix;
        #endregion

        /// <summary>
        /// Updates the local game context with the data contained in this <see cref="GameUpdateData"/> instance.
        /// </summary>
        public void UpdateLocalGame([NotNull] GameContext game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            GameContext.PushThreadContext(game);
            try
            {
                game.TurnNumber = _turnNumber;
                game.CivilizationManagers.Clear();
                game.CivilizationManagers.AddRange(_civManagers);
                game.Universe.Objects = _objects;
                game.SectorClaims = _sectorClaims;
                game.AgreementMatrix = _agreementMatrix;
                game.DiplomacyData = _diplomacyData;

                game.Diplomats.Clear();

                if (_diplomats != null)
                {
                    foreach (var diplomat in _diplomats)
                    {
                        var ownerId = diplomat.OwnerID;

                        game.Diplomats.Add(diplomat);

                        foreach (var civ in game.Civilizations)
                        {
                            if (civ.CivID == ownerId)
                                continue;
                            var foreignPower = diplomat.GetForeignPower(civ);
                            _diplomacyData[ownerId, civ.CivID] = foreignPower.DiplomacyData;
                        }
                    }
                }

                game.OnDeserialized();
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }

        /// <summary>
        /// Creates a new <see cref="GameUpdateData"/> instance for the specified player
        /// in the given game context.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="player">The player.</param>
        /// <returns>The new <see cref="GameUpdateData"/> instance.</returns>
        public static GameUpdateData Create(GameContext game, Player player)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            if (player == null)
                throw new ArgumentNullException("player");

            var data = new GameUpdateData();

            GameContext.PushThreadContext(game);
            try
            {
                data._turnNumber = game.TurnNumber;
                data._civManagers = game.CivilizationManagers.ToArray();
                data._objects = game.Universe.Objects;
                data._sectorClaims = game.SectorClaims;
                data._agreementMatrix = game.AgreementMatrix;
                data._diplomacyData = game.DiplomacyData;
//                game.Diplomats.TryGetValue(player.EmpireID, out data._diplomat);
                data._diplomats = new[] { Diplomat.Get(player) }; //game.Diplomats.ToArray();
                data._civManagers.ForEach(o => o.Compact());
            }
            finally
            {
                GameContext.PopThreadContext();
            }

            return data;
        }

        #region IOwnedDataSerializable Members
        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_turnNumber);
            _objects.SerializeOwnedData(writer, context);
            writer.WriteObject(_civManagers);
            writer.WriteObject(_sectorClaims);
            writer.WriteObject(_agreementMatrix);
            writer.WriteObject(_diplomacyData);
            writer.WriteArray(_diplomats);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _objects = new UniverseObjectSet();
            _turnNumber = reader.ReadOptimizedInt32();
            _objects.DeserializeOwnedData(reader, context);
            _civManagers = reader.ReadArray<CivilizationManager>();
            _sectorClaims = reader.Read<SectorClaimGrid>();
            _agreementMatrix = reader.Read<AgreementMatrix>();
            _diplomacyData = reader.Read<CivilizationPairedMap<IDiplomacyData>>();
            _diplomats = reader.ReadArray<Diplomat>();
        }
        #endregion
    }
}
