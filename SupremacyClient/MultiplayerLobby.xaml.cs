// MultiplayerLobby.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity.Utility;
using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Views;
using Supremacy.Client.Views.LobbyScreen;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NavigationCommands = Supremacy.Client.Commands.NavigationCommands;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for MultiplayerLobby.xaml
    /// </summary>
    public partial class MultiplayerLobby : ILobbyScreenView
    {
        private readonly IResourceManager _resourceManager;

        public MultiplayerLobby([NotNull] IAppContext appContext, [NotNull] IResourceManager resourceManager)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");

            AppContext = appContext;
            _resourceManager = resourceManager;

            InitializeComponent();

            ChatOutbox.KeyDown += OnChatOutboxKeyDown;
            OptionsPanel.OptionsChanged += OnOptionsPanelOptionsChanged;
            CancelButton.Click += OnCancelButtonClick;
            StartButton.Click += OnStartButtonClick;

            Loaded += OnMultiplayerLobbyLoaded;

            OnCreated();
        }

        private void OnLobbyUpdated(ClientDataEventArgs<ILobbyData> args)
        {
            var lobbyData = args.Value;
            if (lobbyData == null)
            {
                OptionsPanel.IsEnabled = false;
                StartButton.IsEnabled = false;
                return;
            }

            StartButton.IsEnabled = AppContext.IsGameHost;
            OptionsPanel.IsEnabled = AppContext.IsGameHost && !lobbyData.GameOptions.IsFrozen;
            OptionsPanel.Options = lobbyData.GameOptions;

            if (PlayerInfoPanel.Children.Count == 0)
                CreateSlots(lobbyData);

            //GameLog.Print("GameContext.Current.IsMultiplayerGame={0}", GameContext.Current.IsMultiplayerGame    IffffsMultiplayerGame.ToString());
            if (AppContext.IsSinglePlayerGame)
                return;

            UpdateSlots(lobbyData);
        }

        private void OnClientDisconnected(EventArgs args)
        {
            ClearChatMessages();
            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.MenuScreen);
        }

        private void OnPlayerExited(DataEventArgs<IPlayer> args)
        {
            AppendNoticeToChatPanel(
                _resourceManager.GetStringFormat(
                    "MP_LOBBY_PLAYER_EXITED_MESSAGE_FORMAT",
                    args.Value.Name));
        }

        private void OnPlayerJoined(DataEventArgs<IPlayer> args)
        {
            AppendNoticeToChatPanel(
                _resourceManager.GetStringFormat(
                    "MP_LOBBY_PLAYER_JOINED_MESSAGE_FORMAT",
                    args.Value.Name));
        }

        private void AppendNoticeToChatPanel(string message)
        {
            var text = new TextBlock
                       {
                           Foreground = Brushes.Yellow,
                           TextWrapping = TextWrapping.Wrap,
                           Text = "*** " + message
                       };
            ChatPanel.Children.Add(text);
        }

        private void ClearChatMessages()
        {
            ChatPanel.Children.Clear();
        }

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (!AppContext.IsGameHost)
                return;

            GameLog.Print("EmpireID-Selected={0} ", AppContext.LocalPlayer.EmpireID);

            // IMPORTANT: at the moment only LocalPlayer is checked whether it's set to NOT Playable. At the moment NO check for the other players in Multi Player
            switch (MasterResources.CivDB[AppContext.LocalPlayer.EmpireID].ShortName)
            {
                case "Intro":
                    if (OptionsPanel.Options.IntroPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_0_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Federation":
                    if (OptionsPanel.Options.FederationPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_1_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Terran Empire":
                    if (OptionsPanel.Options.TerranEmpirePlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_2_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Romulans":
                    if (OptionsPanel.Options.RomulanPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_3_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Klingons":
                    if (OptionsPanel.Options.KlingonPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_4_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Cardassians":
                    if (OptionsPanel.Options.CardassianPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_5_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Dominion":
                    if (OptionsPanel.Options.DominionPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_6_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
                case "Borg":
                    if (OptionsPanel.Options.BorgPlayable == EmpirePlayable.No)
                    {
                        MessageDialog.Show(Environment.NewLine + _resourceManager.GetString("CIV_7_NOT_IN GAME"), MessageDialogButtons.Ok);
                        return;
                    }
                    break;
            }

            GameLog.Print("EmpireID STARTING Game={0}", AppContext.LocalPlayer.EmpireID);


            if (AppContext.LobbyData.UnassignedPlayers.Any())
            {
                var result = MessageDialog.Show(
                    _resourceManager.GetString("MP_LOBBY_WARN_UNASSIGNED_PLAYERS_HEADER"),
                    _resourceManager.GetString("MP_LOBBY_WARN_UNASSIGNED_PLAYERS_MESSAGE"),
                    MessageDialogButtons.YesNo);
                if (result == MessageDialogResult.No)
                    return;
            }
            if (OptionsPanel.Options.GalaxySize > GalaxySize.Small)
            {
                var result = MessageDialog.Show(
                    "Warning",
                    "For performance reasons, it is highly recommended that the galaxy size"
                    + " be restricted to 'Tiny' or 'Small' for multiplayer games."
                    + "  Are you sure you want to continue?",
                    MessageDialogButtons.YesNo);
                if ((result != MessageDialogResult.None) && (result != MessageDialogResult.Yes))
                {
                    return;
                }
            }

            GameOptionsManager.SaveDefaults(OptionsPanel.Options);
            ClientCommands.StartMultiplayerGame.Execute(null);
        }

        private static void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            ClientCommands.EndGame.Execute(false);
        }

        private void OnOptionsPanelOptionsChanged()
        {
            if (!AppContext.IsConnected || !AppContext.IsGameHost)
                return;
            ClientCommands.SendUpdatedGameOptions.Execute(OptionsPanel.Options);
        }

        private void OnMultiplayerLobbyLoaded(object sender, RoutedEventArgs e)
        {
            ChatOutbox.Focus();
        }

        private void CreateSlots(ILobbyData lobbyData)
        {
            if ((lobbyData == null) || (lobbyData.Slots == null) ||(lobbyData.Players == null))
                return;

            ClearSlots();

            foreach (var slot in lobbyData.Slots)
            {
                var slotView = new PlayerSlotView { Slot = slot };
                slotView.AssignedPlayerChanged += OnSlotViewAssignedPlayerChanged;
                slotView.SlotClosed += OnSlotViewSlotClosed;
                slotView.SlotOpened += OnSlotViewSlotOpened;
                PlayerInfoPanel.Children.Add(slotView);
            }

            UpdateSlots(lobbyData);
        }

        private void ClearSlots()
        {
            foreach (var slotView in PlayerInfoPanel.Children.OfType<PlayerSlotView>())
            {
                slotView.AssignedPlayerChanged -= OnSlotViewAssignedPlayerChanged;
                slotView.SlotClosed -= OnSlotViewSlotClosed;
                slotView.SlotOpened -= OnSlotViewSlotOpened;
            }

            PlayerInfoPanel.Children.Clear();
        }

        private static void OnSlotViewSlotOpened(object sender, EventArgs e)
        {
            var slotView = sender as PlayerSlotView;
            if (slotView == null)
                return;

            ClientCommands.ClearPlayerSlot.Execute(slotView.Slot.SlotID);
        }

        private static void OnSlotViewSlotClosed(object sender, EventArgs e)
        {
            var slotView = sender as PlayerSlotView;
            if (slotView == null)
                return;

            ClientCommands.ClosePlayerSlot.Execute(slotView.Slot.SlotID);
        }

        private static void OnSlotViewAssignedPlayerChanged(object sender, EventArgs e)
        {
            var slotView = sender as PlayerSlotView;
            if (slotView == null)
                return;

            var playerId = Player.UnassignedPlayerID;
            var player = slotView.AssignedPlayer;
            if (player != null)
                playerId = player.PlayerID;

            if (playerId == Player.UnassignedPlayerID)
            {
                ClientCommands.ClearPlayerSlot.Execute(slotView.Slot.SlotID);
            }
            else
            {
                ClientCommands.AssignPlayerSlot.Execute(
                    new Pair<int, int>(
                        slotView.Slot.SlotID,
                        playerId));
            }
        }

        private void UpdateSlots(ILobbyData lobbyData)
        {
            if ((lobbyData == null) || (lobbyData.Slots == null) || (lobbyData.Players == null))
                return;

            var localPlayer = AppContext.LocalPlayer;


            if (AppContext.IsSinglePlayerGame)
            {
                GameLog.Print("AppContext.IsSinglePlayerGame={0}", AppContext.IsSinglePlayerGame);
                return;
            }

            foreach (var slotView in PlayerInfoPanel.Children.OfType<PlayerSlotView>())
            {
                var playerSlot = lobbyData.Slots[slotView.Slot.SlotID];
                var assignablePlayers = lobbyData.Players.ToList();

                if (localPlayer.IsGameHost)
                {
                    //GameLog.Print("localPlayer.Name={0}, is hosting...", localPlayer.Name);
                    assignablePlayers.Add(Player.Unassigned);
                    assignablePlayers.Add(Player.Computer);
                    //assignablePlayers.Add(Player.TurnedToMinor);
                    //assignablePlayers.Add(Player.TurnedToExpandingPower);
                }
                else
                {
                    GameLog.Print("localplayer.Name={0} is NOT hosting...", localPlayer.Name);
                    assignablePlayers.RemoveAll(o => !Equals(o, localPlayer) && !Equals(o, playerSlot.Player));
                    if (playerSlot.IsVacant)
                    {
                        assignablePlayers.Add(Player.Unassigned);
                    }
                    else
                    {
                        if (Equals(playerSlot.Player, localPlayer))
                        {
                            assignablePlayers.Add(Player.Unassigned);
                        }
                        else
                        {
                            assignablePlayers.RemoveAll(o => Equals(o, localPlayer));
                            if (Equals(playerSlot.Player, Player.Computer))
                                assignablePlayers.Add(Player.Computer);
                        }
                    }
                }
                using (slotView.EnterUpdateScope())
                {
                    slotView.AssignablePlayers = assignablePlayers.ToList();
                    slotView.Slot = playerSlot;
                }
            }
        }

        private void OnChatOutboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            var message = ChatOutbox.Text.Trim();
            ChatOutbox.Text = "";
            if (String.Equals(message, String.Empty))
                return;
            ClientCommands.SendChatMessage.Execute(
                new ChatMessage(
                    AppContext.LocalPlayer,
                    message));
        }

        public void PushChatMessage(ChatMessage message)
        {
            var messageHost = new ContentControl();
            var widthBinding = new Binding { Source = ChatPanel, Path = new PropertyPath(ActualWidthProperty) };

            messageHost.Content = message;
            messageHost.SetBinding(WidthProperty, widthBinding);
            ChatPanel.Children.Add(messageHost);

            if (ChatPanel.ScrollOwner != null)
                ChatPanel.ScrollOwner.ScrollToEnd();
        }

        #region Implementation of IActiveAware
        private bool _isActive;

        public event EventHandler IsActiveChanged;

        private void OnIsActiveChanged()
        {
            var handler = IsActiveChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (Equals(_isActive, value))
                    return;
                _isActive = value;
                OnIsActiveChanged();
                if (_isActive)
                    EnsureSlots();
                else
                    ClearSlots();
            }
        }

        private void EnsureSlots()
        {
            if (PlayerInfoPanel.Children.Count != 0)
                return;
            CreateSlots(AppContext.LobbyData);
        }
        #endregion

        #region Implementation of IGameScreenView
        public IAppContext AppContext { get; set; }
        public object Model { get; set; }

        public void OnCreated()
        {
            ClientEvents.PlayerJoined.Subscribe(OnPlayerJoined, ThreadOption.UIThread);
            ClientEvents.PlayerExited.Subscribe(OnPlayerExited, ThreadOption.UIThread);
            ClientEvents.ClientDisconnected.Subscribe(OnClientDisconnected, ThreadOption.UIThread);
            ClientEvents.LobbyUpdated.Subscribe(OnLobbyUpdated, ThreadOption.UIThread);
            ClientEvents.ChatMessageReceived.Subscribe(OnChatMessageReceived, ThreadOption.UIThread);
        }

        private void OnChatMessageReceived(ClientDataEventArgs<ChatMessage> arg)
        {
            PushChatMessage(arg.Value);
        }

        public void OnDestroyed()
        {
            ClientEvents.PlayerJoined.Unsubscribe(OnPlayerJoined);
            ClientEvents.PlayerExited.Unsubscribe(OnPlayerExited);
            ClientEvents.ClientDisconnected.Unsubscribe(OnClientDisconnected);
            ClientEvents.LobbyUpdated.Unsubscribe(OnLobbyUpdated);
            ClientEvents.ChatMessageReceived.Unsubscribe(OnChatMessageReceived);
        }
        #endregion
    }
}