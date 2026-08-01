// File:SavedGameManager.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.IO;
using Supremacy.Messages;
using Supremacy.Messaging;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using MessageBox = System.Windows.MessageBox;

namespace Supremacy.Game
{
    //public class XML_Item
    //{
    //    public string Key { get; set; }
    //    public string Value { get; set; }
    //}

    //public class XML_Items
    //{
    //    [XmlElement("Item")]
    //    public List<XML_Item> Items { get; set; } = new List<XML_Item>();
    //}

    public class CsvItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Helper class for managing saved _game files.
    /// </summary>
    public static class SavedGameManager
    {
        [NonSerialized]
        public const string AutoSaveFileName = ".autosav";

        static List<CsvItem> _csv_data = new List<CsvItem>();

        //private static readonly string _newline=Environment.NewLine;
        //private static string _text;

        public static string SavedGameDirectory
        {
            get
            {

                string _text = Path.Combine(ResourceManager.GetResourcePath(""), "SavedGames_V", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                _text = _text.Replace("V\\", "V");
                _text = _text.Replace(".\\", "");
                //GameLog.Client.SaveLoad.DebugFormat("SavedGameDirectory = {0}", _text);
                //Console.WriteLine(_text);

                return _text;
            }
        }


        /// <summary>
        /// Finds the saved games on the disk.
        /// </summary>
        /// <returns></returns>
        public static SavedGameHeader[] FindSavedGames(bool includeAutoSave = true)
        {
            List<SavedGameHeader> savedGames = new List<SavedGameHeader>();
            string path = SavedGameDirectory;

            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
            }

            string[] fileNames = Directory.GetFiles(path, "*.sav", SearchOption.TopDirectoryOnly);

            if (includeAutoSave)
            {
                string autoSaveFileName = Path.Combine(path, AutoSaveFileName);

                if (File.Exists(autoSaveFileName))
                {
                    SavedGameHeader header = LoadSavedGameHeader(AutoSaveFileName);
                    if (header != null)
                    {
                        if (header.GameVersion == Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            savedGames.Add(header);
                        }
                    }
                }
            }

            foreach (string fileName in fileNames)
            {
                SavedGameHeader header = LoadSavedGameHeader(fileName);
                string _currentGameVersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string _text;
                if (header != null)
                {

                    if (header.GameVersion == _currentGameVersionString)
                    {
                        savedGames.Add(header);
                    }
                    else
                    {
                        if (fileName.Contains("V17"))
                        {
                            _text = "Step_4500:; " + "currentGameVersion =" + _currentGameVersionString

                                + ", but " + header.GameVersion
                                + " for " + header.FileName
                                ;
                            Console.WriteLine("Step_4500:; " + _text);
                            GameLog.Client.SaveLoad.DebugFormat(_text);
                            //GameLog.Client.SaveLoad.DebugFormat("currentGameVersion = {2}, but {1} for {0}"
                            //    , header.FileName
                            //    , header.GameVersion
                            //    , _currentGameVersionString
                            //    );
                            header.GameVersion = _currentGameVersionString;
                            savedGames.Add(header);


                            //var result = MessageBox.Show(_text, "Loading this different version file might cause troubles ! - please move out of the SavedGame-folder !", MessageBoxButton.YesNo) ;
                            //if (result == MessageBoxResult.No) // no - this creates an endless loop
                            //{
                            //    result = MessageBox.Show("please move " + header.FileName + " out of the SavedGame-folder !");
                            //}
                        }
                    }
                }
            }
            return savedGames.OrderByDescending(s => s.Timestamp).ToArray();
        }

        /// <summary>
        /// Loads the saved _game header.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static SavedGameHeader LoadSavedGameHeader([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            string _text;

            try
            {
                string fullPath;

                if (Path.IsPathRooted(fileName))
                {
                    fullPath = fileName;
                }
                else
                {
                    string _shortSavedGameDirectory = SavedGameDirectory;
                    _shortSavedGameDirectory = _shortSavedGameDirectory.Replace(".\\", "");
                    fullPath = Path.Combine(Environment.CurrentDirectory, _shortSavedGameDirectory, FixFileName(fileName));
                    //Console.WriteLine(fullPath);
                    fullPath = fullPath.Replace(_shortSavedGameDirectory + "\\" + _shortSavedGameDirectory, _shortSavedGameDirectory);  // removing double _shortSavedGameDirectory
                    //Console.WriteLine(fullPath);
                }

                _text = /*Environment.NewLine + */"Step_0292:; --------------";
                Console.WriteLine(_text);
                // works but doubled     GameLog.Client.SaveLoad.DebugFormat(_text);

                _text = /*Environment.NewLine + */"Step_0293:; fullPath = " + fullPath;
                //Console.WriteLine(_text);
                // works but doubled     GameLog.Client.SaveLoad.DebugFormat(_text);

                SavedGameHeader header;
                using (FileStream fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _text = "Step_0286:; "+DateTime.Now+" > reading HEADER of " + fileName;
                    Console.WriteLine(_text);
                    GameLog.Client.SaveLoadDetails.DebugFormat(_text);

                    header = SavedGameHeader.Read(fileStream);
                }

                if (string.Equals(Path.GetExtension(fileName), AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
                {
                    header.IsAutoSave = true;
                }

                header.FileName = header.IsAutoSave ? AutoSaveFileName : Path.GetFileNameWithoutExtension(fileName);

                return header;
            }
            catch
            {
                _text = "Step_0291:; is the file there ? ...not able to read HEADER of " + fileName; // command line parameter ... e.g. started out of VS
                Console.WriteLine(_text);
                GameLog.Client.SaveLoad.DebugFormat(_text);

                return null;
            }
        }

        /// <summary>
        /// Loads a _game and stores the _game _xml_data in the output parameters.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="header">The header.</param>
        /// <param name="game">The _game context.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        public static bool LoadGame(string fileName, out SavedGameHeader header, out GameContext game, out DateTime timestamp)
        {
            string _text;
            try
            {
                if (!Path.IsPathRooted(fileName))
                {
                    fileName = Path.Combine(SavedGameDirectory, FixFileName(fileName));
                }
                GameLog.Core.General.InfoFormat("Step_0275:; Loading saved _game {0}", fileName);

                using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    GameLog.Core.SaveLoad.DebugFormat("Step_0277:; beginning loading {0} ...", fileName);
                    header = SavedGameHeader.Read(fileStream);
                    string thisGameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (header.GameVersion != thisGameVersion)
                    {
                        if (!fileName.Contains("V17"))
                            throw new Exception(string.Format("Incompatible _game save - {0} vs {1}", header.GameVersion, thisGameVersion));
                    }
                    GameLog.Core.SaveLoad.DebugFormat("Step_0286:; loading SavedGameHeader of {0}", fileName);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int value;
                        while (fileStream.CanRead && ((value = fileStream.ReadByte()) != -1))
                        {
                            memoryStream.WriteByte((byte)value);
                        }
                        GameLog.Core.SaveLoad.DebugFormat("Step_0283:; loading {0}, Stream was read...", fileName);
                        _ = memoryStream.Seek(0, SeekOrigin.Begin);
                        Console.WriteLine("Step_0288:; reading memoryStream into _game");
                        game = StreamUtility.Read<GameContext>(memoryStream.ToArray());
                    }
                }

                _text = "Step_0333:; loading GameTables from HDD...";
                Console.WriteLine(_text);
                //GameLog.Core.SaveLoad.DebugFormat(_text);
                game.Tables = GameTables.Load();

                _text = "Step_0344:; loading ResearchMatrix from HDD...";
                Console.WriteLine(_text);
                //GameLog.Core.SaveLoad.DebugFormat(_text);
                game.ResearchMatrix = ResearchMatrix.Load();

                game.OnDeserialized();

                //_navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
                timestamp = File.GetLastWriteTime(fileName);
            }
            catch (Exception e)
            {
                _text = "Step_4098:; Error occurred loading saved _game" + e;
                Console.WriteLine(_text);
                GameLog.Core.General.Error(_text);

                header = null;
                game = null;
                timestamp = default;

                return false;
            }
            //_navigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            return true;
        }

        private static string FixFileName(string fileName)
        {
            if (string.Equals(Path.GetExtension(fileName), AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
            {
                return fileName;
            }

            if (!string.Equals(Path.GetExtension(fileName), ".sav", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + ".sav";
            }

            return fileName;
        }

        public static FileInfo GetSavedGameFile([NotNull] SavedGameHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            return new FileInfo(Path.Combine(
                ResourceManager.GetResourcePath(""),
                SavedGameDirectory,
                FixFileName(header.FileName)));
        }

        /// <summary>
        /// Saves the _game to the disk.
        /// </summary>
        /// <param name="fileName">The outpu filename.</param>
        /// <param name="game">The _game.</param>
        /// <param name="localPlayer">The local player.</param>
        /// <param name="lobbyData">The server lobby _xml_data.</param>
        /// <returns></returns>
        public static bool SaveGame([NotNull] string fileName, [NotNull] GameContext game, [NotNull] Player localPlayer, [NotNull] LobbyData lobbyData)
        {
            if (fileName == null)
            {
                fileName = "_manual_save_(CTRL+S)";
            }
            string _text;

            _text = "Step_3100:; SaveGame: localPlayer= " + localPlayer + ", fileName= " + fileName;
            Console.WriteLine(_text);
            GameLog.Core.SaveLoad.DebugFormat(_text);

            if (game == null)
            {
                throw new ArgumentNullException("_game");
            }

            if (localPlayer == null)
            {
                throw new ArgumentNullException("localPlayer");
            }

            if (lobbyData == null)
            {
                throw new ArgumentNullException("lobbyData");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DateTime.Now.ToLongDateString();
            }

            SavedGameHeader header;

            GameContext.PushThreadContext(game);

            try
            {
                fileName = Path.Combine(SavedGameDirectory, FixFileName(fileName));

                if (!Directory.Exists(SavedGameDirectory))
                {
                    _ = Directory.CreateDirectory(SavedGameDirectory);
                }

                header = new SavedGameHeader(game, localPlayer);

                if (string.Equals(fileName, AutoSaveFileName, StringComparison.OrdinalIgnoreCase))
                {
                    header.IsAutoSave = true;
                }

                _text = "Step_9000:; Writing _game... Turn " + game.TurnNumber;
                Console.WriteLine(_text);
                GameLog.Client.GameData.DebugFormat(_text);

                byte[] buffer = StreamUtility.Write(game);
                _text = "hier > StreamUtility.Write austauschen";
                _text = "objekt in xml serialisieren";

                using (FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    header.Write(fileStream);
                    fileStream.Write(buffer, 0, buffer.Length);
                }

                //XML_Utility_Write(game);
                CSV_Utility_Write(game);



            }
            catch (Exception e)
            {
                GameLog.Core.General.Error("Error saving _game", e);

                return false;
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }

            Channel.Publish(new GameSavedMessage(header));

            return true;
        }

        private static void CSV_Utility_Write(GameContext _game)
        {
            //var _csv_data = new List<CsvItem>();
            
        //{
        //    new CsvItem { Key = "TurnNumber", Value = "12" },
        //    new CsvItem { Key = "IsMultiplayerGame", Value = "True" }
        //};


            string _game_mod_text = "null";
            if (_game.GameMod != null) _game_mod_text = _game.GameMod.ToString();

            CSV_ADD("TurnNumber", _game.TurnNumber.ToString() );
            CSV_ADD("IsMultiplayerGame", _game.IsMultiplayerGame.ToString() );
            CSV_ADD("GameMod", _game_mod_text );
            CSV_ADD("CivCount", _game.Civilizations.Count.ToString() );

            foreach (var item in _game.Civilizations)
            {
                CSV_ADD("CivID", item.CivID.ToString() 
                                 +";"+ item.Key
                );
                
                CivilizationManager _civM = GameContext.Current.CivilizationManagers[item.CivID];
                CSV_ADD("CivM"
                    , _civM.Credits.CurrentValue.ToString()
                    + ";" + _civM.Z_RankingCredits

                    + ";" + _civM.MaintenanceCostLastTurn
                    + ";" + _civM.Z_RankingMaint
                    + ";" + _civM.Z_RankingResearch
                    + ";" + _civM.Z_RankingIntelAttack

                    + ";" + _civM.AverageMorale
                    + ";" + _civM.AverageTechLevel
                    );
            }

            bool bool_debugger = true;
            if (bool_debugger)
            {
                //Debugger.Break();
            }

            //var serializer = new XmlSerializer(typeof(XML_Items));

            //using (var fs = new FileStream("Saved_B2data.xml", FileMode.Create))
            //{
            //    serializer.Serialize(fs, _xml_data, ns);
            //}

            using (var writer = new StreamWriter("Saved_B2data.csv", false, Encoding.UTF8))
            {
                writer.WriteLine("Key,Value");
                foreach (var item in _csv_data)
                {
                    writer.WriteLine(Escape(item.Key) + "," + Escape(item.Value));
                }
            }
        }

        private static void CSV_ADD(string v1, string v2)
        {
            _csv_data.Add(new CsvItem
            {
                Key = Escape(v1),
                Value = Escape(v2)
            });
        }

        private static void XML_Utility_Write(GameContext _game)
        {
        //    //var ns = new XmlSerializerNamespaces();
        //    //ns.Add("xsi", "noNamespaceSchemaLocation=\"Saved_B2data.xsd\"");

        //    var data = new List<CsvItem>
        //{
        //    new CsvItem { Key = "TurnNumber", Value = "12" },
        //    new CsvItem { Key = "IsMultiplayerGame", Value = "True" }
        //};


        //    string _game_mod_text = "null";
        //    if (_game.GameMod != null) _game_mod_text = _game.GameMod.ToString();



        //    //var _xml_data = new XML_Items();
        //    //_xml_data.Items.Add(new XML_Item { Key = "TurnNumber", Value = _game.TurnNumber.ToString() });
        //    //_xml_data.Items.Add(new XML_Item { Key = "IsMultiplayerGame", Value = _game.IsMultiplayerGame.ToString() });
        //    //_xml_data.Items.Add(new XML_Item { Key = "GameMod", Value = _game_mod_text });
        //    //_xml_data.Items.Add(new XML_Item { Key = "CivCount", Value = _game.Civilizations.Count.ToString() });

        //    //foreach (var item in _game.Civilizations)
        //    //{
        //    //    _xml_data.Items.Add(new XML_Item { Key = "CivID", Value = item.CivID.ToString() });
        //    //    _xml_data.Items.Add(new XML_Item { Key = "CivKey", Value = item.Key.ToString() });
        //    //}



        //    //var serializer = new XmlSerializer(typeof(XML_Items));

        //    //using (var fs = new FileStream("Saved_B2data.xml", FileMode.Create))
        //    //{
        //    //    serializer.Serialize(fs, _xml_data, ns);
        //    //}

        //    //using (var writer = new StreamWriter("Saved_B2data.csv", false, Encoding.UTF8))
        //    {
        //        writer.WriteLine("Key,Value");
        //        foreach (var item in data)
        //        {
        //            writer.WriteLine(Escape(item.Key) + "," + Escape(item.Value));
        //        }
        //    }
        }

        static string Escape(string value)
        {
            if (value == null)
                return "";

            if (value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0)
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }


        public static bool SaveGameDeleteManualSaved()
        {
            string file = Path.Combine(Environment.CurrentDirectory + "\\" + SavedGameDirectory, FixFileName("_manual_save_(CTRL+S).sav"));

            //ResourceManager.GetString("Do you really want to delete > ")
            var result = MessageBox.Show(
                "ALT+S: Do you really want to delete > "
                + " " + file, "REALLY ?",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                return false;
            }

            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    _ = MessageBox.Show("Deleted: " + file + Environment.NewLine + "Create again with CTRL+S");
                    return true;
                }
            }
            catch { _ = MessageBox.Show("Problem at deleting: " + file); ; return false; }
            return false;
        }

        public static bool SaveGameDeleteAutoSaved()
        {
            string file = Path.Combine(Environment.CurrentDirectory + "\\" + SavedGameDirectory, FixFileName(".autosav"));
            string _text;

            //ResourceManager.GetString("Do you really want to delete > ")
            var result = MessageBox.Show(
                "ALT+Y: Do you really want to delete > "
                + " " + file, "REALLY ?",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                return false;
            }

            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    _text = "Deleted: " + file;
                    Console.WriteLine(_text);

                    //_ = MessageBox.Show("Deleted: " + file /*+ _newline + "Create again with CTRL+S");
                    return true;
                }
            }
            catch { _ = MessageBox.Show("Problem at deleting: " + file); ; return false; }
            return false;
        }

        /// <summary>
        /// Automatically saves the current _game.
        /// </summary>
        /// <param name="localPlayer">The local player.</param>
        /// <param name="lobbyData">The server lobby _xml_data.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public static bool AutoSave(Player localPlayer, LobbyData lobbyData)
        {
            GameContext game = GameContext.Current;
            if (game == null)
            {
                return false;
            }
            string _text;

            try
            {
                string SavedGameFolder = SavedGameDirectory + "\\";
                //string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                //string SavedGameFolder = appDataFolder + "\\Star Trek Supremacy\\Saved Games\\" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\\";


                string file_autosav_current = SavedGameFolder + ".autosav";
                string file_autosav_1_turn_ago = SavedGameFolder + "autosav_1_turn_ago.sav";
                string file_autosav_2_turns_ago = SavedGameFolder + "autosav_2_turns_ago.sav";
                string file_autosav_3_turns_ago = SavedGameFolder + "autosav_3_turns_ago.sav";
                string file_autosav_4_turns_ago = SavedGameFolder + "autosav_4_turns_ago.sav";

                _text = "Step_9501:; Turn " + game.TurnNumber + " > Autosaving > " + file_autosav_current;
                Console.WriteLine(_text);
                GameLog.Core.General.InfoFormat(_text);

                if (File.Exists(file_autosav_3_turns_ago))
                {
                    File.Copy(file_autosav_3_turns_ago, file_autosav_4_turns_ago, true);
                }

                if (File.Exists(file_autosav_2_turns_ago))
                {
                    File.Copy(file_autosav_2_turns_ago, file_autosav_3_turns_ago, true);
                }

                if (File.Exists(file_autosav_1_turn_ago))
                {
                    File.Copy(file_autosav_1_turn_ago, file_autosav_2_turns_ago, true);
                }

                if (File.Exists(file_autosav_current))
                {
                    File.Copy(file_autosav_current, file_autosav_1_turn_ago, true);
                }

            }
            catch (Exception e)
            {
                _text = "Step_9505:; Problem at saving autosav and previous autosav Exception "
                    + e.Message + Environment.NewLine + e.StackTrace;
                //GameLog.Core.SaveLoad.WarnFormat("Step_9505:; Problem at saving autosav and previous autosav Exception {0} {1}", e.Message, e.StackTrace);
            }

            return SaveGame(AutoSaveFileName, game, localPlayer, lobbyData);
        }
    }




}
