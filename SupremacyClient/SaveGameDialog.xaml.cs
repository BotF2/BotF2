// SaveGameDialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Game;
using System.IO;

using Supremacy.Resources;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for SaveGameDialog.xaml
    /// </summary>
    public sealed partial class SaveGameDialog
    {
        #region Constructors
        public SaveGameDialog()
        {
            InitializeComponent();
            SaveButton.IsEnabled = true;
            IsVisibleChanged += SaveGameDialogIsVisibleChanged;

            _ = InputBindings.Add(
                new KeyBinding(GenericCommands.CancelCommand,
                               Key.Escape,
                               ModifierKeys.None));
            _ = InputBindings.Add(
                new KeyBinding(GenericCommands.AcceptCommand,
                               Key.Enter,
                               ModifierKeys.None));

            _ = CommandBindings.Add(new CommandBinding(
                                    GameCommands.SaveGameCommand,
                                    GameCommandsSaveGameCommandExecuted));
            _ = CommandBindings.Add(
                new CommandBinding(GenericCommands.CancelCommand,
                                   GenericCommandsCancelCommandExecuted));
            _ = CommandBindings.Add(
                new CommandBinding(GenericCommands.AcceptCommand,
                                   GenericCommandsAcceptCommandExecuted,
                                   GenericCommandsAcceptCommandCanExecute));
            SaveButton.IsEnabled = true;
            SaveGameList.SelectionChanged += SaveGameListSelectionChanged;
        }

        private void SaveGameListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveGameList.SelectedItem is SavedGameHeader header)
            {
                SaveGameFilename.Text = header.FileName;
                SaveGameInfoText.Visibility = Visibility.Visible;
                SaveButton.IsEnabled = true;
            }
            else
            {
                SaveGameInfoText.Visibility = Visibility.Hidden;
            }
        }

        private void GenericCommandsAcceptCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = !string.IsNullOrEmpty(Path.GetFileName(SaveGameFilename.Text.Trim()));
                SaveButton.IsEnabled = true;
            }
            catch
            {
                e.CanExecute = false;
            }
        }
        #endregion

        #region Methods
        private void GameCommandsSaveGameCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TrySaveGame();
        }

        private void GenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            TrySaveGame();
        }

        private void TrySaveGame()
        {
            try
            {
                if (!string.IsNullOrEmpty(Path.GetFileName(SaveGameFilename.Text.Trim())))
                {
                    if (File.Exists(SaveGameFilename.Text + ".sav"))
                    {
                        MessageDialogResult overwriteResponse = MessageDialog.Show(
                            ResourceManager.GetString("SAVE_OVERWRITE_CONFIRM_HEADER"),
                            ResourceManager.GetString("SAVE_OVERWRITE_CONFIRM_MESSAGE"),
                            MessageDialogButtons.YesNo);
                        if (overwriteResponse == MessageDialogResult.No)
                        {
                            return;
                        }
                    }
                    IsEnabled = false;
                    ClientCommands.SaveGame.Execute(SaveGameFilename.Text);
                    Close();
                }
            }
            catch
            {
                _ = MessageDialog.Show(
                    ResourceManager.GetString("SAVE_ERROR_INVALID_FILE_NAME_HEADER"),
                    ResourceManager.GetString("SAVE_ERROR_INVALID_FILE_NAME_MESSAGE"),
                    MessageDialogButtons.Ok);
            }
        }

        private void GenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void SaveGameDialogIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            SaveButton.IsEnabled = true;
            DataContext = SavedGameManager.FindSavedGames(includeAutoSave: false);

            SaveGameFilename.Clear();
            SaveGameFilename.Text = GenerateFileName(GameContext.Current);
            _ = SaveGameFilename.Focus();
            SaveGameFilename.SelectAll();
        }

        private string GenerateFileName(GameContext game)
        {
            StringBuilder sb = new StringBuilder();

            if (game.IsMultiplayerGame)
            {
                _ = sb.Append("MP ");
            }
            else
            {
                _ = sb.Append("SP ");
            }

            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            if (appContext != null)
            {
                _ = sb.Append(appContext.LocalPlayer.Empire.ShortName).Append(' ');
            }

            return sb.Append(game.Options.GalaxySize)
                     .Append(' ')
                     .Append(game.Options.GalaxyShape)
                     .Append(' ')
                     .Append(game.TurnNumber)
                     .ToString();
        }

        #endregion
    }
}