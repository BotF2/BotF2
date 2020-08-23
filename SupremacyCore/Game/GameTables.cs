// GameTables.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Data;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Provides quick access to common game data tables.
    /// </summary>
    [Serializable]
    public sealed class GameTables
    {
        private TableMap _resourceTables;
        private TableMap _universeTables;
        private TableMap _map24x15;
        private TableMap _moraleTables;
        private TableMap _enumTables;
        private TableMap _shipTables;
        private TableMap _diplomacyTables;
        private TableMap _gameOptionTables;
        private TableMap _strengthTables;

        /// <summary>
        /// Gets the game option tables.
        /// </summary>
        /// <value>The game option tables.</value>
        public TableMap GameOptionTables
        {
            get { return _gameOptionTables; }
        }

        /// <summary>
        /// Gets the resource tables.
        /// </summary>
        /// <value>The resource tables.</value>
        public TableMap ResourceTables
        {
            get { return _resourceTables; }
        }

        /// <summary>
        /// Gets the universe tables.
        /// </summary>
        /// <value>The universe tables.</value>
        public TableMap UniverseTables
        {
            get { return _universeTables; }
        }

        /// <summary>
        /// Gets the morale tables.
        /// </summary>
        /// <value>The morale tables.</value>
        public TableMap MoraleTables
        {
            get { return _moraleTables; }
        }

        /// <summary>
        /// Gets the diplomacy tables.
        /// </summary>
        /// <value>The diplomacy tables.</value>
        public TableMap DiplomacyTables
        {
            get { return _diplomacyTables; }
        }

        /// <summary>
        /// Gets the enum string tables.
        /// </summary>
        /// <value>The enum string tables.</value>
        public TableMap EnumTables
        {
            get { return _enumTables; }
        }

        /// <summary>
        /// Gets the ship tables.
        /// </summary>
        /// <value>The ship tables.</value>
        public TableMap ShipTables
        {
            get { return _shipTables; }
        }

        /// <summary>
        /// Gets the strength tables.
        /// </summary>
        /// <value>The strength tables.</value>
        public TableMap StrengthTables
        {
            get { return _strengthTables; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTables"/> class.
        /// </summary>
        private GameTables() { }

        /// <summary>
        /// Loads the game tables from the disk.
        /// </summary>
        /// <returns>A new <see cref="GameTables"/> instance.</returns>
        public static GameTables Load()
        {
            const string tablesPath = @"Resources\Data\";

            GameLog.Client.GameInitData.DebugFormat("... no Output for often used Tables: MessageDialogButtons, TechCategory, SitRepCategory");

            var tables = new GameTables
                         {
                             _resourceTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "ResourceTables.txt")),
                             _universeTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "UniverseTables.txt")),
                            _map24x15 = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "UniverseMap24x15.txt")),
                            _moraleTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "MoraleTables.txt")),
                             _enumTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "EnumStrings.txt")),
                             _shipTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "ShipTables.txt")),
                             _diplomacyTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "DiplomacyTables.txt")),
                             _gameOptionTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "GameOptionTables.txt")),
                              _strengthTables = TableMap.ReadFromFile(
                                 ResourceManager.GetResourcePath(tablesPath + "StrengthTables.txt"))
            };

            return tables;
        }
    }
}
