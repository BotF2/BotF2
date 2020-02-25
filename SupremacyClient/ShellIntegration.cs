using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public static class ShellIntegration
    {
        private static readonly Lazy<string> _autoSaveGameTitle = new Lazy<string>(() => ResourceManager.GetString("AUTO_SAVE_GAME_TITLE"));

        public static void UpdateJumpList()
        {
            //works    GameLog.Print("UpdateJumpList (List of saved games)");
            var savedGames = (
                                 from savedGame in SavedGameManager.FindSavedGames()
                                 let file = SavedGameManager.GetSavedGameFile(savedGame)
                                 where file.Exists
                                 orderby savedGame.Timestamp descending
                                 select savedGame
                             )
                .Take(10)
                .ToList();

            var jumpListCreated = false;
            JumpList jumpList = null;

            _ = Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Background,
                (Action)
                (() => jumpList = JumpList.GetJumpList(Application.Current)));

            if (jumpList == null)
            {
                jumpList = new JumpList();
                jumpListCreated = true;
            }
            else
            {
                jumpList.JumpItems.Clear();
            }

            var entryExe = typeof(ClientModule).Assembly.Location;

            var singlePlayerCategory = ResourceManager.GetString("SAVED_SINGLE_PLAYER_GAMES_CATEGORY");
            var multiplayerCategory = ResourceManager.GetString("SAVED_MULTIPLAYER_GAMES_CATEGORY");
            var singlePlayerDescription = ResourceManager.GetString("SAVED_SINGLE_PLAYER_GAME_DESCRIPTION");
            var multiplayerDescription = ResourceManager.GetString("SAVED_MULTIPLAYER_GAME_DESCRIPTION");

            var iconFiles = savedGames
                .Select(o => o.LocalPlayerEmpireName)
                .Distinct()
                .Select(o => new { EmpireName = o, IconFile = FindEmpireShellIcon(o) })
                .ToDictionary(o => o.EmpireName);

            foreach (var savedGame in savedGames)
            {
                AddToJumpList(
                    jumpList,
                    savedGame,
                    savedGame.IsAutoSave
                        ? _autoSaveGameTitle.Value
                        : Path.GetFileNameWithoutExtension(savedGame.FileName),
                    entryExe,
                    iconFiles[savedGame.LocalPlayerEmpireName].IconFile,
                    savedGame.IsMultiplayerGame ? multiplayerCategory : singlePlayerCategory,
                    savedGame.IsMultiplayerGame ? multiplayerDescription : singlePlayerDescription);
            }

            object v = _ = Application.Current.Dispatcher.Invoke(
                priority: DispatcherPriority.Background,
                (Action)
                (() =>
                {
                    if (jumpListCreated)
                    {
                        JumpList.SetJumpList(
                            Application.Current,
                            jumpList);
                    }
                    jumpList.Apply();
                }));
        }

        private static void AddToJumpList(
            JumpList jumpList,
            SavedGameHeader savedGame,
            string title,
            string entryExe,
            string iconFile,
            string category,
            string description)
        {
            jumpList.JumpItems.Add(
                new JumpTask
                {
                    CustomCategory = category,
                    ApplicationPath = entryExe,
                    WorkingDirectory = Path.GetDirectoryName(entryExe),
                    IconResourcePath = iconFile,
                    Title = title,
                    Arguments = "-SavedGame:\"" + savedGame.FileName + "\"",
                    Description = string.Format(
                        description,
                        savedGame.LocalPlayerEmpireName,
                        savedGame.TurnNumber,
                        savedGame.Timestamp.LocalDateTime,
                        savedGame.LocalPlayerName)
                });
        }

        private static string FindEmpireShellIcon(string empireName)
        {
            const string iconDirectory = @"Resources\Shell";
            const string defaultIconName = "Default.ico";

            // works    GameLog.Print("empireName={0}, iconDirectory={1}", empireName, iconDirectory);

            try
            {
                string iconFile;

                if (empireName == null)
                    empireName = "Default";

                var empireIconFile = empireName + ".ico";

                var currentMod = ResourceManager.CurrentMod;

                //if (currentMod == null)   // just says whether "command line argument MOD" or not or similar
                //    GameLog.Print("currentMod == null");

                if (currentMod != null)
                {
                    iconFile = Path.Combine(currentMod.RootPath, iconDirectory, empireIconFile);

                    GameLog.Client.General.DebugFormat("{0} is used out of {1}...maybe check \\Resources\\Shell", empireIconFile, iconDirectory);

                    if (File.Exists(iconFile))
                        return iconFile;
                }

                iconFile = Path.Combine(PathHelper.GetWorkingDirectory(), iconDirectory, empireIconFile);

                if (File.Exists(iconFile))
                    return iconFile;

                if (currentMod != null)
                {
                    iconFile = Path.Combine(currentMod.RootPath, iconDirectory, defaultIconName);

                    GameLog.Client.General.DebugFormat("defaultIcon {0} is used ...maybe check \\Resources\\Shell", defaultIconName);
                    
                    if (File.Exists(iconFile))
                        return iconFile;
                }

                iconFile = Path.Combine(PathHelper.GetWorkingDirectory(), iconDirectory, defaultIconName);

                if (File.Exists(iconFile))
                    return iconFile;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return null;
        }
    }
}