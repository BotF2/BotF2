// 
// LoadGameDialog.xaml.cs
// 
// Copyright (c) 2007-2016 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Supremacy.Client.Commands;
using Supremacy.Game;
using Supremacy.Utility;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for LoadGameDialog.xaml
    /// </summary>
    public partial class LoadGameDialog
    {
        #region Constructors

        public LoadGameDialog()
        {
            InitializeComponent();

            IsVisibleChanged += LoadGameDialogIsVisibleChanged;
            Loaded += LoadGameDialogLoaded;

            SaveGameList.SelectionChanged += SaveGameListSelectionChanged;

            InputBindings.Add(
                new KeyBinding(
                    GenericCommands.CancelCommand,
                    Key.Escape,
                    ModifierKeys.None));
            InputBindings.Add(
                new KeyBinding(
                    GenericCommands.AcceptCommand,
                    Key.Enter,
                    ModifierKeys.None));

            CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.CancelCommand,
                    GenericCommandsCancelCommandExecuted));
            CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.AcceptCommand,
                    GenericCommandsAcceptCommandExecuted));
        }

        #endregion

        #region Methods

        private void GenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            SavedGameHeader saveGameHeader = SaveGameList.SelectedItem as SavedGameHeader;
            if (saveGameHeader == null)
                return;
            Close();
            GameLog.Client.General.DebugFormat("LOAD was pressed (GenericCommandsAcceptCommandExecuted)");
            ClientCommands.LoadGame.Execute(saveGameHeader);
        }

        private void GenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void LoadGameDialogIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                DataContext = SavedGameManager.FindSavedGames();

            if (IsLoaded)
            {
                Focus();
                SaveGameList.Focus();
            }
        }

        private void LoadGameDialogLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            SaveGameList.Focus();
        }

        private void SaveGameListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveGameInfoText.Visibility = (SaveGameList.SelectedItem is SavedGameHeader)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        #endregion
    }
}