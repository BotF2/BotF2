// SavedGameHeader.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;
using System.Linq;

using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Represents a saved game file header.
    /// </summary>
    public sealed class SavedGameHeader
    {
        private static readonly Lazy<string> AutoSaveGameTitle = new Lazy<string>(
            () =>
            {
                const string fallback = "(Auto Save)";

                GameLog.Core.SaveLoad.Debug("just for time stamp point 1");

                try
                {
                    var localizedTitle = ResourceManager.GetString("AUTO_SAVE_GAME_TITLE");

                    if (!string.IsNullOrWhiteSpace(localizedTitle))
                        return localizedTitle;

                    return fallback;
                }
                catch (Exception e)
                {
                    GameLog.Core.SaveLoad.DebugFormat("###### Problem with SavedGame" + Environment.NewLine + "{0}", e);
                    return fallback;
                }
            });

        public string Title
        {
            get
            {
                if (FileName != null)
                    GameLog.Core.SaveLoad.DebugFormat("FileName = {0}", FileName);
                return IsAutoSave ? AutoSaveGameTitle.Value : FileName;
            }
        }

        public bool IsAutoSave { get; set; }

        public bool IsMultiplayerGame { get; private set; }

        /// <summary>
        /// Gets the game options.
        /// </summary>
        /// <value>The game options.</value>
        public GameOptions Options { get; private set; }

        /// <summary>
        /// Gets the local player's empire ID.
        /// </summary>
        /// <value>The local player's empire ID.</value>
        public int LocalPlayerEmpireID { get; private set; }

        public int CivID { get; private set; }

        public string LocalPlayerName { get; private set; }

        public int[] EmpireIDs { get; private set; }

        public string[] EmpireNames { get; private set; }

        public SlotClaim[] SlotClaims { get; private set; }

        public SlotStatus[] SlotStatus { get; private set; }

        /// <summary>
        /// Gets the name of the empire.
        /// </summary>
        /// <value>The name of the empire.</value>
        public string LocalPlayerEmpireName
        {
            get
            {
                try
                {
                    return "Game";

                }
                catch
                {
                    // ToDo: this is just a workaround. No problem occures having all empires "playable", that means "in the game"
                    //generates much lines of output each second !!!     
                    GameLog.Core.General.Error("########### Problem with EmpireNames[LocalPlayerEmpireID]");
                    return LocalPlayerEmpireName;
                }
            }
        }

        /// <summary>
        /// Gets the turn number.
        /// </summary>
        /// <value>The turn number.</value>
        public int TurnNumber { get; private set; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedGameHeader"/> class.
        /// </summary>
        private SavedGameHeader() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedGameHeader"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="localPlayer">The local player.</param>
        public SavedGameHeader(IGameContext game, Player localPlayer)
        {
            GameLog.Core.SaveLoad.Debug("just for time stamp point 2");
            if (game == null)
                throw new ArgumentNullException("game");
            if (localPlayer == null)
                throw new ArgumentNullException("localPlayer");

            IsMultiplayerGame = game.IsMultiplayerGame;
            LocalPlayerName = localPlayer.Name;
            LocalPlayerEmpireID = localPlayer.EmpireID;
            TurnNumber = game.TurnNumber;
            Options = game.Options;
            Timestamp = DateTimeOffset.Now;

            var empires = game.Civilizations.Where(o => o.IsEmpire).ToArray();

            EmpireIDs = new int[empires.Length];
            EmpireNames = new string[empires.Length];
            SlotStatus = new SlotStatus[empires.Length];
            SlotClaims = new SlotClaim[empires.Length];

            for (int i = 0; i < empires.Length; i++)
            {
                EmpireIDs[i] = empires[i].CivID;
                EmpireNames[i] = empires[i].ShortName;
            }
        }

        /// <summary>
        /// Writes this <see cref="SavedGameHeader"/> to the specified output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public void Write(Stream output)
        {
            if (!output.CanWrite)
                throw new InvalidOperationException("Cannot write to stream");

            var writer = new BinaryWriter(output);

            Options.Write(writer);
            writer.Write(IsMultiplayerGame);
            writer.Write(LocalPlayerName);
            writer.Write(LocalPlayerEmpireID);
            writer.Write(TurnNumber);
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);

            var empireCount = (byte)EmpireIDs.Length;
            //GameLog.Print("empireCount={0}", empireCount);

            writer.Write(empireCount);

            for (int i = 0; i < empireCount; i++)
            {
                GameLog.Core.SaveLoad.DebugFormat("Writing Empires: {0}, CivID={1}, empires in total={2}, SlotClaim={3}, Slotstatus={4}", EmpireNames[i], EmpireIDs[i], empireCount, SlotClaims[i], SlotStatus[i]);
                writer.Write(EmpireIDs[i]);
                writer.Write(EmpireNames[i]);
                writer.Write((byte)SlotClaims[i]);
                writer.Write((byte)SlotStatus[i]);
            }
        }

        /// <summary>
        /// Reads a <see cref="SavedGameHeader"/> from the specified input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns></returns>
        public static SavedGameHeader Read(Stream input)
        {
            if (!input.CanRead)
                throw new InvalidOperationException("Cannot read from stream");

            var reader = new BinaryReader(input);
            var options = new GameOptions();

            GameLog.Core.SaveLoad.DebugFormat("----------------------------------------------------");
            GameLog.Core.SaveLoad.DebugFormat("########  Beginning reading a saved game...");

            options.Read(reader);

            var header = new SavedGameHeader
            {
                Options = options,
                IsMultiplayerGame = reader.ReadBoolean(),
                LocalPlayerName = reader.ReadString(),
                LocalPlayerEmpireID = reader.ReadInt32(),
                TurnNumber = reader.ReadInt32(),
                Timestamp = new DateTimeOffset(reader.ReadInt64(), TimeSpan.FromTicks(reader.ReadInt64()))
            };

            //doesn't work here
            //GameLog.Core.SaveLoad.DebugFormat("Reading SavedGameHeader: Timestamp={0}, LocalPlayerName={1}, LocalPlayerEmpireID={2}, TurnNumber={3}, IsMultiplayerGame={4}",
            //                            Timestamp, LocalPlayerName, LocalPlayerEmpireID, TurnNumber, IsMultiplayerGame, Options.ToString());

            var empireCount = reader.ReadByte();
            //works     GameLog.GameLog.Core.SaveLoad.DebugFormat("Read empireCount={0}", empireCount);

            header.EmpireIDs = new int[empireCount];
            header.EmpireNames = new string[empireCount];
            header.SlotClaims = new SlotClaim[empireCount];
            header.SlotStatus = new SlotStatus[empireCount];

            for (int i = 0; i < empireCount; i++)
            {

                header.EmpireIDs[i] = reader.ReadInt32();
                header.EmpireNames[i] = reader.ReadString();
                header.SlotClaims[i] = (SlotClaim)reader.ReadByte();
                header.SlotStatus[i] = (SlotStatus)reader.ReadByte();

                GameLog.Core.SaveLoad.DebugFormat("Reading: Empires in total={2}, SlotClaim={3}, Slotstatus={4}, CivID={1}, Empire: {0}",
                                        header.EmpireNames[i], header.EmpireIDs[i], empireCount, header.SlotClaims[i], header.SlotStatus[i]);
            }

            return header;
        }
    }
}
