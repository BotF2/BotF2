// File:SavedGameHeader.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Supremacy.Game
{
    /// <summary>
    /// Represents a saved game file header.
    /// </summary>
    public sealed class SavedGameHeader
    {
        private static string _text;

        public string Title => IsAutoSave ? ResourceManager.GetString("AUTO_SAVE_GAME_TITLE") : FileName;

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
        public string LocalPlayerEmpireName => "Game";

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
        /// The version of the game that this game save was made with
        /// </summary>
        public string GameVersion { get; set; }

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
            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            if (localPlayer == null)
            {
                throw new ArgumentNullException("localPlayer");
            }

            IsMultiplayerGame = game.IsMultiplayerGame;
            _text = "Step_0101: IsMultiplayerGame" + IsMultiplayerGame.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            LocalPlayerName = localPlayer.Name;
            _text = "Step_0103: LocalPlayerName" + LocalPlayerName.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            LocalPlayerEmpireID = localPlayer.EmpireID;
            _text = "Step_0105: LocalPlayerEmpireID" + LocalPlayerEmpireID.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            TurnNumber = game.TurnNumber;
            _text = "Step_0107: TurnNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            Options = game.Options;
            _text = "Step_0111: Options" + Options.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            Timestamp = DateTimeOffset.Now;
            _text = "Step_0121: Timestamp" + Timestamp.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _text = "Step_0123: GameVersion" + GameVersion.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            Entities.Civilization[] empires = game.Civilizations.Where(o => o.IsEmpire).ToArray();
            _text = "Step_0105: TrunNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);
            
            EmpireIDs = new int[empires.Length];
            _text = "Step_0105: TrunNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            EmpireNames = new string[empires.Length];
            _text = "Step_0105: TrunNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            SlotStatus = new SlotStatus[empires.Length];
            _text = "Step_0105: TrunNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            SlotClaims = new SlotClaim[empires.Length];
            _text = "Step_0105: TrunNumber" + TurnNumber.ToString();
            Console.WriteLine(_text);
            GameLog.Client.SaveLoad.DebugFormat(_text);

            for (int i = 0; i < empires.Length; i++)
            {
                EmpireIDs[i] = empires[i].CivID;
                _text = "Step_0161: EmpireIDs" + EmpireIDs[i].ToString();
                Console.WriteLine(_text);
                GameLog.Client.SaveLoad.DebugFormat(_text);

                EmpireNames[i] = empires[i].ShortName;
                _text = "Step_01165: EmpireNames" + EmpireNames[i].ToString();
                Console.WriteLine(_text);
                GameLog.Client.SaveLoad.DebugFormat(_text);

            }
        }


        /// <summary>
        /// Writes this <see cref="SavedGameHeader"/> to the specified output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public void Write(Stream output)
        {
            if (!output.CanWrite)
            {
                throw new InvalidOperationException("Cannot write to stream");
            }

            BinaryWriter writer = new BinaryWriter(output);
            Options.Write(writer);
            writer.Write(IsMultiplayerGame);
            writer.Write(LocalPlayerName);
            writer.Write(LocalPlayerEmpireID);
            writer.Write(TurnNumber);
            writer.Write(Timestamp.Ticks);
            writer.Write(Timestamp.Offset.Ticks);
            writer.Write(GameVersion);

            byte empireCount = (byte)EmpireIDs.Length;

            writer.Write(empireCount);

            for (int i = 0; i < empireCount; i++)
            {
                GameLog.Core.SaveLoadDetails.DebugFormat("Writing Empires: empires in total={2}, SlotClaim={3}, Slotstatus={4}, CivID={1}, {0}", EmpireNames[i], EmpireIDs[i], empireCount, SlotClaims[i], SlotStatus[i]);
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

            _text = "trying to read HEADER ...";
            //Console.WriteLine(_text);
            //GameLog.Client.SaveLoad.DebugFormat(_text);

            if (!input.CanRead)
            {
                throw new InvalidOperationException("Cannot read from stream");
            }

            BinaryReader reader = new BinaryReader(input);
            GameOptions options = new GameOptions();

            options.Read(reader);  // if options not compatible to previous savedgame-Version > loading crashes here

            SavedGameHeader header = new SavedGameHeader
            {
                Options = options,
                IsMultiplayerGame = reader.ReadBoolean(),
                LocalPlayerName = reader.ReadString(),
                LocalPlayerEmpireID = reader.ReadInt32(),
                TurnNumber = reader.ReadInt32(),
                Timestamp = new DateTimeOffset(reader.ReadInt64(), TimeSpan.FromTicks(reader.ReadInt64())),
                GameVersion = reader.ReadString()
            };

            byte empireCount = reader.ReadByte();

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
            }

            _text = "Step_4080: SavedGame"
                /*+ Environment.NewLine*/ + ";GameVersion;" + header.GameVersion
                /*+ Environment.NewLine*/ + ";Turn;" + header.TurnNumber
                /*+ Environment.NewLine*/ + ";" + header.Title

                /*+ Environment.NewLine + ";FileName   ;" + reader.   --- no filename available here*/
                ;
            Console.WriteLine(_text);
            GameLog.Client.SaveLoadDetails.DebugFormat(_text);

            return header;
        }
    }
}
