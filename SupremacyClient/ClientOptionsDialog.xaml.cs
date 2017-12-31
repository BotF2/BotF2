// ClientSettingsWindow.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Input;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for ClientSettingsWindow.xaml
    /// </summary>
    public partial class ClientOptionsDialog
    {
        public ClientOptionsDialog()
        {
            InitializeComponent();

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
                    OnGenericCommandsCancelCommandExecuted));

            CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.AcceptCommand,
                    OnGenericCommandsAcceptCommandExecuted));
        }

        private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.Reload();
            Close();
        }

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            SaveChangesAndHide();
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: SAVE: File: {0}, Content: ", filePath, fileWriter);
        }

        private void SaveChangesAndHide()
        {
            ClientSettings.Current.Save();

            var settings = ClientSettings.Current;

            // Music & Audio
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: MasterVolume={0}", settings.MasterVolume);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: MusicVolume={0}", settings.MusicVolume);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: FXVolume={0}", settings.FXVolume);

            //Graphics
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableFullScreenMode={0}", settings.EnableFullScreenMode);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableDialogAnimations={0}", settings.EnableDialogAnimations);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableStarMapAnimations={0}", settings.EnableStarMapAnimations);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableHighQualityScaling={0}", settings.EnableHighQualityScaling);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableAntiAliasing={0}", settings.EnableAntiAliasing);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: EnableCombatScreen={0}", settings.EnableCombatScreen);

            ////General
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: DominionPlayable={0}", settings.DominionPlayable);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: BorgPlayable={0}", settings.BorgPlayable);
            //GameLog.Client.GameData.DebugFormat("ClientOptionsDialog.xaml.cs: TerranEmpirePlayable={0}", settings.TerranEmpirePlayable);

            Close();
        }
    }
}