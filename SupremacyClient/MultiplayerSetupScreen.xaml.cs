// MultiplayerSetupScreen.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using Supremacy.Client.Commands;
using Supremacy.IO;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for MultiplayerSetupScreen.xaml
    /// </summary>
    public partial class MultiplayerSetupScreen
    {
        public static readonly RoutedCommand DirectConnectCommand;
        public static readonly RoutedCommand JoinGameCommand;
        public static readonly RoutedCommand HostGameCommand;
        public static readonly RoutedCommand CancelCommand;

        static MultiplayerSetupScreen()
        {
            DirectConnectCommand = new RoutedCommand(
                "DirectConnect",
                typeof(MultiplayerSetupScreen));
            JoinGameCommand = new RoutedCommand(
                "JoinGame",
                typeof(MultiplayerSetupScreen));
            HostGameCommand = new RoutedCommand(
                "HostGame",
                typeof(MultiplayerSetupScreen));
            CancelCommand = new RoutedCommand(
                "Cancel",
                typeof(MultiplayerSetupScreen));
        }

        public MultiplayerSetupScreen()
        {
            InitializeComponent();

            CommandBindings.Add(
                new CommandBinding(DirectConnectCommand,
                                   ExecuteDirectConnectCommand,
                                   CanExecuteDirectConnectCommand));
            CommandBindings.Add(
                new CommandBinding(JoinGameCommand,
                                   ExecuteJoinGameCommand,
                                   CanExecuteJoinGameCommand));
            CommandBindings.Add(
                new CommandBinding(HostGameCommand,
                                   ExecuteHostGameCommand,
                                   CanExecuteHostGameCommand));
            CommandBindings.Add(
                new CommandBinding(CancelCommand,
                                   ExecuteCancelCommand));

            Loaded += OnMultiplayerSetupScreenLoaded;
            Unloaded += OnMultiplayerSetupScreenUnloaded;

            TryGetLastPlayerName();
            TryGetLastIP();
        }

        void TryGetLastPlayerName()
        {
            try
            {
                PlayerName.Text = StorageManager.ReadSetting<string, string>("LastPlayerName");
                //DirectConnectAddress = StorageManager.ReadSetting<string, string>("LastPlayerName");
                //PlayerName. = StorageManager.ReadSetting<string, string>("LastPlayerName");
                PlayerName.CaretIndex = PlayerName.Text.Length;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        void TrySetLastPlayerName()
        {
            try
            {
                string playerName = PlayerName.Text.Trim();
                if (playerName.Length > 0)
                {
                    StorageManager.WriteSetting("LastPlayerName", playerName);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        void TryGetLastIP()
        {
            try
            {
                DirectConnectAddress.Text = StorageManager.ReadSetting<string, string>("DirectConnectAddressString");
                //DirectConnectAddress = StorageManager.ReadSetting<string, string>("LastPlayerName");
                //PlayerName. = StorageManager.ReadSetting<string, string>("LastPlayerName");
                DirectConnectAddress.CaretIndex = PlayerName.Text.Length;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        void TrySetLastIP()
        {
            try
            {
                string DirectConnectAddressString = DirectConnectAddress.Text.Trim();
                if (DirectConnectAddressString.Length > 0)
                {
                    StorageManager.WriteSetting("DirectConnectAddressString", DirectConnectAddressString);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        void CanExecuteDirectConnectCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Regex regex = new Regex(
                @"(([a-zA-Z][-a-zA-Z0-9]*(.[a-zA-Z][-a-zA-Z0-9]*)*)"
                + @"|([0-9]{1,3}(.[0-9]{1,3}){3}))");
            e.CanExecute = (PlayerName.Text.Trim().Length > 0)
                && regex.IsMatch(DirectConnectAddress.Text);
        }

        void CanExecuteJoinGameCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Regex regex = new Regex(@"\w+");
            if (!regex.IsMatch(PlayerName.Text))
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = (PART_ServerList.SelectedItem != null);
        }

        void CanExecuteHostGameCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Regex regex = new Regex(@"\w+");
            if (!regex.IsMatch(PlayerName.Text))
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = (PlayerName.Text.Trim().Length > 0);
        }

        void ExecuteDirectConnectCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TrySetLastPlayerName();
            TrySetLastIP();
            Close();
            ClientCommands.JoinMultiplayerGame.Execute(
                new MultiplayerConnectParameters(
                    PlayerName.Text.Trim(),
                    DirectConnectAddress.Text.Trim()));
        }

        void ExecuteJoinGameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void ExecuteHostGameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TrySetLastPlayerName();
            Close();
            ClientCommands.HostMultiplayerGame.Execute(PlayerName.Text.Trim());
        }

        void ExecuteCancelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        void OnMultiplayerSetupScreenUnloaded(object sender, RoutedEventArgs e)
        {
            //HumanClient.Current.NetworkClient.StopFindingServers();
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Function(UpdateServerList));
        }

        void OnMultiplayerSetupScreenLoaded(object sender, RoutedEventArgs e)
        {
            //HumanClient.Current.NetworkClient.StartFindingServers();
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Function(UpdateServerList));
        }

        void UpdateServerList()
        {
            ServerList.Items.Refresh();
        }

        public Selector ServerList => PART_ServerList;
    }
}