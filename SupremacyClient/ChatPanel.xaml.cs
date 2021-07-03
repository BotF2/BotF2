// ChatPanel.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Commands;
using Supremacy.Client.Events;
using Supremacy.Game;
using Supremacy.Types;

using System.Linq;
using Supremacy.Client.Context;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for ChatPanel.xaml
    /// </summary>
    public partial class ChatPanel
    {
        #region Fields
        public static readonly DependencyProperty IsMessageWaitingProperty;
        public static readonly DependencyProperty MostRecentMessageProperty;

        private readonly Player _globalPlayer;
        private bool _isLoaded;
        #endregion

        #region Constructors
        static ChatPanel()
        {
            MostRecentMessageProperty = DependencyProperty.Register(
                "MostRecentMessage",
                typeof(ChatMessage),
                typeof(ChatPanel),
                new PropertyMetadata(null));

            IsMessageWaitingProperty = DependencyProperty.Register(
                "IsMessageWaiting",
                typeof(bool),
                typeof(ChatPanel),
                new PropertyMetadata(false));
        }

        public ChatPanel()
        {
            InitializeComponent();
            Loaded += ChatPanel_Loaded;
            Unloaded += ChatPanel_Unloaded;
            IsVisibleChanged += ChatPanel_IsVisibleChanged;

            _globalPlayer = new Player { Name = "All Players", PlayerID = -1 };
        }
        #endregion

        #region Properties
        public ChatMessage MostRecentMessage
        {
            get => GetValue(MostRecentMessageProperty) as ChatMessage;
            set => SetValue(MostRecentMessageProperty, value);
        }

        public bool IsMessageWaiting
        {
            get => (bool)GetValue(IsMessageWaitingProperty);
            set => SetValue(IsMessageWaitingProperty, value);
        }
        #endregion

        #region Methods
        private void ChatPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                IsMessageWaiting = false;
            }
        }

        private void ChatPanel_Loaded(object sender, RoutedEventArgs e)
        {
            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            _isLoaded = true;
            _ = RecipientBox.Items.Add(_globalPlayer);

            foreach (Player player in appContext.RemotePlayers)
            {
                _ = RecipientBox.Items.Add(player);
            }

            RecipientBox.SelectedItem = _globalPlayer;

            _ = ClientEvents.ChatMessageReceived.Subscribe(OnChatMessageReceived, ThreadOption.UIThread);
            _ = ClientEvents.PlayerExited.Subscribe(OnPlayerExited, ThreadOption.UIThread);

            if (IsVisible)
            {
                _ = Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new BoolFunction(InputText.Focus));
            }
        }

        private void OnPlayerExited(DataEventArgs<IPlayer> args)
        {
            IPlayer player = args.Value;
            if (player == null)
            {
                return;
            }

            Player item = RecipientBox.Items.OfType<Player>().FirstOrDefault(o => o.PlayerID == player.PlayerID);
            if (item != null)
            {
                RecipientBox.Items.Remove(item);
            }
        }

        private void OnChatMessageReceived(DataEventArgs<ChatMessage> args)
        {
            ChatMessage message = args.Value;
            if (message == null)
            {
                return;
            }

            DisplayChatMessage(message);
        }

        private void ChatPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                _isLoaded = false;
                ClientEvents.ChatMessageReceived.Unsubscribe(OnChatMessageReceived);
                ClientEvents.PlayerExited.Unsubscribe(OnPlayerExited);
            }
        }

        private void InputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string messageText = InputText.Text.Trim();
                if (messageText.Length > 0)
                {
                    IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
                    Player recipient = RecipientBox.SelectedItem as Player;
                    if (recipient == _globalPlayer)
                    {
                        recipient = null;
                    }

                    ClientCommands.SendChatMessage.Execute(new ChatMessage(appContext.LocalPlayer, messageText, recipient));
                    InputText.Clear();
                }
            }
        }

        private void DisplayChatMessage(ChatMessage message)
        {
            ContentControl messageContainer = new ContentControl { Content = message };

            MostRecentMessage = message;

            _ = MessagePanel.Children.Add(messageContainer);
            if (MessagePanel.ScrollOwner != null)
            {
                MessagePanel.ScrollOwner.ScrollToBottom();
            }

            if (!IsVisible)
            {
                IsMessageWaiting = true;
            }
        }
        #endregion
    }
}