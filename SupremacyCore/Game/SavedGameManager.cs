// SavedGameManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.IO;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Tech;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Helper class for managing saved game files.
    /// </summary>
    public static class SavedGameManager
    {
        public const string AutoSaveFileName = ".autosav";

        public static string SavedGameDirectory
        {
            get { return Path.Combine(StorageManager.UserLocalProfileFolder, "Saved Games"); }
        }

        /// <summary>
        /// Finds the saved games on the disk.
        /// </summary>
        /// <returns></returns>
        public static SavedGameHeader[] FindSavedGames(bool includeAutoSave = true)
        {
            var savedGames = new List<SavedGameHeader>();
            var path = SavedGameDirectory;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileNames = Directory.GetFiles(path, "*.sav", SearchOption.TopDirectoryOnly);

            if (includeAutoSave)
            {
                var autoSaveFileName = Path.Combine(path, AutoSaveFileName);

                if (File.Exists(autoSaveFileName))
                {
                    var header = LoadSavedGameHeader(AutoSaveFileName);
                    if (header != null)
                        savedGames.Add(header);
                }
            }

            foreach (var fileName in fileNames)
            {
                var header = LoadSavedGameHeader(fileName);
                if (header != null)
                    savedGames.Add(header);
            }

            return savedGames.OrderByDescending(s => s.Timestamp).ToArray();
        }

        /// <summary>
        /// Loads the saved game header.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static SavedGameHeader LoadSavedGameHeader([NotNull] string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            try
            {
                string fullPath;

                if (Path.IsPathRooted(fileName))
                    fullPath = fileName;
                else
                    fullPath = Path.Combine(SavedGameDirectory, FixFileName(fileName));

                SavedGameHeader header;
                using (var fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    header = SavedGameHeader.Read(fileStream);
                }

                if (string.Equals(Path.GetExtension(fileName), AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
                    header.IsAutoSave = true;

                header.FileName = header.IsAutoSave ? AutoSaveFileName : Path.GetFileNameWithoutExtension(fileName);

                return header;
            }
            catch
            {
                //works      GameLog.Print("loading .... jump over....for Header {0} ", fileName);
                return null;
            }
        }

        /// <summary>
        /// Loads a game and stores the game data in the output parameters.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="header">The header.</param>
        /// <param name="game">The game context.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        public static bool LoadGame(string fileName, out SavedGameHeader header, out GameContext game, out DateTime timestamp)
        {
            try
            {
                if (!Path.IsPathRooted(fileName))
                    fileName = Path.Combine(SavedGameDirectory, FixFileName(fileName));
                GameLog.Print("########## loading {0}", fileName);

                using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //GameLog.Print("beginning loading {0} ...", fileName);
                    header = SavedGameHeader.Read(fileStream);
                    //GameLog.Print("loading SavedGameHeader of {0}", fileName);
                    using (var memoryStream = new MemoryStream())
                    {
                        int value;
                        while (fileStream.CanRead && ((value = fileStream.ReadByte()) != -1))
                            memoryStream.WriteByte((byte)value);
                        //GameLog.Print("loading {0}, Stream was read...", fileStream.ToString());
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        game = StreamUtility.Read<GameContext>(memoryStream.ToArray());
                    }
                }

                game.Tables = GameTables.Load();
                game.ResearchMatrix = ResearchMatrix.Load();
                game.OnDeserialized();
                timestamp = File.GetLastWriteTime(fileName);
            }
            catch (Exception e)
            {
                GameLog.Server.General.Error(
                    string.Format(
                        "Error occurred loading saved game '{0}'.",
                        fileName),
                    e);

                header = null;
                game = null;
                timestamp = default(DateTime);

                GameLog.Print("loading ## FAILED ## for {0}", fileName);

                return false;
            }
            return true;
        }

        private static string FixFileName(string fileName)
        {
            // works    GameLog.Print("FileName={0} (fixed)", fileName);

            if (string.Equals(Path.GetExtension(fileName), AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
                return fileName;
            
            if (!string.Equals(Path.GetExtension(fileName), ".sav", StringComparison.OrdinalIgnoreCase))
                fileName = Path.GetFileNameWithoutExtension(fileName) + ".sav";

            return fileName;
        }

        public static FileInfo GetSavedGameFile([NotNull] SavedGameHeader header)
        {
            //works   GameLog.Print("GetSavedGameFile FileName={0} ", header.FileName);
            if (header == null)
                throw new ArgumentNullException("header");

            //works   GameLog.Print("GetSavedGameFile with header={0} ", header.EmpireNames);

            return new FileInfo(
                Path.Combine(
                    StorageManager.UserLocalProfileFolder,
                    SavedGameDirectory,
                    FixFileName(header.FileName)));
        }

        /// <summary>
        /// Saves the game to the disk.
        /// </summary>
        /// <param name="fileName">The outpu filename.</param>
        /// <param name="game">The game.</param>
        /// <param name="localPlayer">The local player.</param>
        /// <param name="lobbyData">The server lobby data.</param>
        /// <returns></returns>
        public static bool SaveGame([NotNull] string fileName, [NotNull] GameContext game, [NotNull] Player localPlayer, [NotNull] LobbyData lobbyData)
        {
            if (fileName == null)
                fileName = "_manual_save";
            //throw new ArgumentNullException("fileName");

            GameLog.Print("SaveGame: localPlayer={1}, fileName= '{0}'",
                                    fileName, localPlayer);

            if (game == null)
            {
                GameLog.Print("SaveGame fileName={0}, Problem with 'game' !!!!");
                throw new ArgumentNullException("game");
            }

            if (localPlayer == null)
            {
                GameLog.Print("SaveGame fileName={0}, Problem with 'localPlayer' !!!!");
                throw new ArgumentNullException("localPlayer");
            }

            if (lobbyData == null)
            {
                GameLog.Print("SaveGame fileName={0}, Problem with 'lobbyData' !!!!");
                throw new ArgumentNullException("lobbyData");
            }

            if (string.IsNullOrEmpty(fileName))
                fileName = DateTime.Now.ToLongDateString();

            SavedGameHeader header;
            // works     GameLog.Print("SaveGame...SavedGameHeader header is done");

            GameContext.PushThreadContext(game);
            // works     GameLog.Print("SaveGame...GameContext.PushThreadContext(game) is done");

            try
            {
                // works     GameLog.Print("SaveGame...try saving file");
                fileName = Path.Combine(SavedGameDirectory, FixFileName(fileName));

                if (!Directory.Exists(SavedGameDirectory))
                    Directory.CreateDirectory(SavedGameDirectory);

                header = new SavedGameHeader(game, localPlayer);

                if (string.Equals(fileName, AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
                    header.IsAutoSave = true;

                var buffer = StreamUtility.Write(game);

                using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    header.Write(fileStream);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                GameLog.Server.General.Error(
                    "Error saving game.",
                    e);

                return false;
            }
            finally
            {
                GameContext.PopThreadContext();
            }

            Channel.Publish(new GameSavedMessage(header));

            return true;
        }

        /// <summary>
        /// Automatically saves the current game.
        /// </summary>
        /// <param name="localPlayer">The local player.</param>
        /// <param name="lobbyData">The server lobby data.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public static bool AutoSave(Player localPlayer, LobbyData lobbyData)
        {
            var game = GameContext.Current;
            if (game == null)
                return false;

            //works    GameLog.Print("doing AutoSave: {0},{1},{2},{3}", AutoSaveFileName, game, localPlayer.Empire, lobbyData.ToString());

            return SaveGame(AutoSaveFileName, game, localPlayer, lobbyData);
        }
    }
}
