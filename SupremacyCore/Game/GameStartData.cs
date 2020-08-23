// 
// GameStartData.cs
// 
// Copyright (c) 2011-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Linq;

using Supremacy.AI;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Text;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Contains the data sent to the clients at the beginning of the game.
    /// </summary>
    [Serializable]
    public sealed class GameStartData : IOwnedDataSerializableAndRecreatable
    {
        #region Fields

        [NonSerialized]
        private GameContext _localGame;

        private int _turnNumber = 1;
        private ITextDatabase _textDatabase;
        private GameOptions _options;
        private GameMod _gameMod;
        private RaceDatabase _races;
        private CivDatabase _civilizations;
        private TechDatabase _techDatabase;
        private ResearchMatrix _researchMatrix;
        private CivilizationManager[] _civManagers;
        private GameTables _tables;
        private UniverseManager _universe;
        private SectorClaimGrid _sectorClaims;
        private TechTreeMap _techTrees;
        private Diplomat[] _diplomats;
        private CivilizationPairedMap<IDiplomacyData> _diplomacyData;
        private StrategyDatabase _strategyDatabase;
        private AgreementMatrix _agreementMatrix;

        #endregion

        /// <summary>
        /// Creates the local game context useing the data in this <see cref="GameStartData"/> instance.
        /// </summary>
        /// <returns>The new game context.</returns>
        public GameContext CreateLocalGame()
        {
            if (_localGame != null)
                return _localGame;

            _localGame = new GameContext();

            GameContext.PushThreadContext(_localGame);

            try
            {
                _localGame.TurnNumber = _turnNumber;
                _localGame.Options = _options;
                _localGame.GameMod = _gameMod;
                _localGame.Civilizations = _civilizations;
                _localGame.CivilizationManagers = new CivilizationManagerMap();
                _localGame.CivilizationManagers.AddRange(_civManagers);
                _localGame.Races = _races;
                _localGame.Universe = _universe;
                _localGame.TechDatabase = _techDatabase;
                _localGame.Tables = _tables;
                _localGame.ResearchMatrix = _researchMatrix;
                _localGame.SectorClaims = _sectorClaims;
                _localGame.TechTrees = _techTrees;
                _localGame.StrategyDatabase = _strategyDatabase;
                _localGame.DiplomacyData = _diplomacyData;
                _localGame.AgreementMatrix = _agreementMatrix;

                _localGame.Diplomats = new CivilizationKeyedMap<Diplomat>(o => o.OwnerID);

                if (_diplomats != null)
                {
                    foreach (var diplomat in _diplomats)
                    {
                        var ownerId = diplomat.OwnerID;

                        _localGame.Diplomats.Add(diplomat);

                        foreach (var civ in _localGame.Civilizations)
                        {
                            if (civ.CivID == ownerId)
                                continue;
                            var foreignPower = diplomat.GetForeignPower(civ);
                            _diplomacyData[ownerId, civ.CivID] = foreignPower.DiplomacyData;
                        }
                    }
                }

                _localGame.OnDeserialized();
                _localGame.LoadStrings(_textDatabase);
            }
            finally
            {
                GameContext.PopThreadContext();
            }

            return _localGame;
        }

        /// <summary>
        /// Creates a new <see cref="GameStartData"/> instance for a specified player using
        /// the given game context.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="player">The player.</param>
        /// <param name="textDatabase">The text database.</param>
        /// <returns>The new <see cref="GameStartData"/> instance.</returns>
        public static GameStartData Create(GameContext game, Player player, ITextDatabase textDatabase)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            if (player == null)
                throw new ArgumentNullException("player");

            var data = new GameStartData();

            GameContext.PushThreadContext(game);

            try
            {
                data._textDatabase = textDatabase;
                data._turnNumber = game.TurnNumber;
                data._options = game.Options;
                data._gameMod = game.GameMod;
                data._civilizations = game.Civilizations;
                data._civManagers = game.CivilizationManagers.ToArray();
                data._races = game.Races;
                data._universe = game.Universe;
                data._techDatabase = game.TechDatabase;
                data._tables = game.Tables;
                data._researchMatrix = game.ResearchMatrix;
                data._sectorClaims = game.SectorClaims;
                data._techTrees = game.TechTrees;
                data._strategyDatabase = game.StrategyDatabase;
                data._diplomacyData = game.DiplomacyData;
                data._diplomats = game.Diplomats.ToArray(); //new[] { Diplomat.Get(player) }; //game.Diplomats.ToArray();
                data._agreementMatrix = game.AgreementMatrix;

                data._civManagers.ForEach(o => o.Compact());
            }
            finally
            {
                GameContext.PopThreadContext();
            }

            return data;
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            if (_localGame == null)
                _localGame = new GameContext();

            GameContext.PushThreadContext(_localGame);

            try
            {
                _textDatabase = reader.Read<ITextDatabase>();
                _localGame.TurnNumber = _turnNumber = reader.ReadOptimizedInt32();
                _localGame.Options = _options = reader.Read<GameOptions>();
                _localGame.GameMod = _gameMod = reader.Read<GameMod>();
                _localGame.Civilizations = _civilizations = reader.Read<CivDatabase>();
                _localGame.CivilizationManagers = new CivilizationManagerMap();
                _localGame.CivilizationManagers.AddRange((_civManagers = reader.ReadArray<CivilizationManager>()));
                _localGame.Races = _races = reader.Read<RaceDatabase>();
                _localGame.Universe = _universe = reader.Read<UniverseManager>();
                _localGame.TechDatabase = _techDatabase = reader.Read<TechDatabase>();
                _localGame.Tables = _tables = reader.Read<GameTables>();
                _localGame.ResearchMatrix = _researchMatrix = reader.Read<ResearchMatrix>();
                _localGame.SectorClaims = _sectorClaims = reader.Read<SectorClaimGrid>();
                _localGame.TechTrees = _techTrees = reader.Read<TechTreeMap>();
                _localGame.StrategyDatabase = _strategyDatabase = reader.Read<StrategyDatabase>();
                _localGame.DiplomacyData = _diplomacyData = reader.Read<CivilizationPairedMap<IDiplomacyData>>();
                _localGame.AgreementMatrix = _agreementMatrix = reader.Read<AgreementMatrix>();

                _localGame.Diplomats = new CivilizationKeyedMap<Diplomat>(o => o.OwnerID);

                _diplomats = reader.ReadArray<Diplomat>();

                if (_diplomats != null)
                {
                    foreach (var diplomat in _diplomats)
                    {
                        var ownerId = diplomat.OwnerID;

                        _localGame.Diplomats.Add(diplomat);

                        foreach (var civ in _localGame.Civilizations)
                        {
                            if (civ.CivID == ownerId)
                                continue;
                            var foreignPower = diplomat.GetForeignPower(civ);
                            _diplomacyData[ownerId, civ.CivID] = foreignPower.DiplomacyData;
                        }
                    }
                }

                _localGame.OnDeserialized();
                _localGame.LoadStrings(_textDatabase);
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteObject(_textDatabase);
            writer.WriteOptimized(_turnNumber);
            writer.WriteObject(_options);
            writer.WriteObject(_gameMod);
            writer.WriteObject(_civilizations);
            writer.WriteArray(_civManagers);
            writer.WriteObject(_races);
            writer.WriteObject(_universe);
            writer.WriteObject(_techDatabase);
            writer.WriteObject(_tables);
            writer.WriteObject(_researchMatrix);
            writer.WriteObject(_sectorClaims);
            writer.WriteObject(_techTrees);
            writer.WriteObject(_strategyDatabase);
            writer.WriteObject(_diplomacyData);
            writer.WriteObject(_agreementMatrix);
            writer.WriteArray(_diplomats);
        }
    }
}
