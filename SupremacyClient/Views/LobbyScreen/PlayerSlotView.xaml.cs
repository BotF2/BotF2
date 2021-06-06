using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Supremacy.Game;
using Supremacy.Types;

namespace Supremacy.Client.Views.LobbyScreen
{
    /// <summary>
    /// Interaction logic for PlayerSlotView.xaml
    /// </summary>
    public partial class PlayerSlotView
    {
        public static readonly DependencyProperty SlotProperty;
        public static readonly DependencyProperty AssignablePlayersProperty;
        public static readonly DependencyProperty AssignedPlayerProperty;

        private readonly StateScope _updateScope;

        static PlayerSlotView()
        {
            SlotProperty = DependencyProperty.Register(
                "Slot",
                typeof(PlayerSlot),
                typeof(PlayerSlotView),
                new PropertyMetadata(
                    null,
                    OnSlotPropertyChanged));

            AssignablePlayersProperty = DependencyProperty.Register(
                "AssignablePlayers",
                typeof(IEnumerable<Player>),
                typeof(PlayerSlotView),
                new PropertyMetadata(
                    new[] { Player.Unassigned, Player.Computer },
                    OnAssignablePlayersPropertyChanged));

            AssignedPlayerProperty = DependencyProperty.Register(
                "AssignedPlayer",
                typeof(Player),
                typeof(PlayerSlotView),
                new PropertyMetadata(
                    Player.Unassigned,
                    OnAssignedPlayerPropertyChanged,
                    CoerceAssignedPlayer));
        }

        private static object CoerceAssignedPlayer(DependencyObject dependencyObject, object baseValue)
        {
            return baseValue ?? Player.Unassigned;
        }

        private static void OnSlotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerSlotView view = d as PlayerSlotView;
            if (view == null)
                return;
            if (e.OldValue != null)
                ((PlayerSlot)e.OldValue).PropertyChanged -= view.OnSlotSubPropertyChanged;
            PlayerSlot newValue = e.NewValue as PlayerSlot;
            if (newValue == null)
                return;
            newValue.PropertyChanged += view.OnSlotSubPropertyChanged;
            view.IsOpenCheckBox.IsChecked = !newValue.IsClosed;
            view.AssignedPlayer = newValue.Player ?? Player.Unassigned;
            //GameLog.Client.GameData.DebugFormat("PlayerSlotView.xaml.cs: SlotProperty has changed: newValue.Player={0}, newValue.IsClosed={1}, view.AssignedPlayer={2}, PlayerSlotView={3}",
            //                newValue.Player, newValue.IsClosed, view.AssignedPlayer, d);
        }

        private void OnSlotSubPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_updateScope.IsWithin)
                return;
            switch (e.PropertyName)
            {
                case "IsClosed":
                    if (Slot.IsClosed)
                        OnSlotClosed();
                    else
                        OnSlotOpened();
                    break;
            }
        }

        private static void OnAssignedPlayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerSlotView view = d as PlayerSlotView;
            if (view == null)
                return;
            if (view._updateScope.IsWithin)
                return;
            view.OnAssignedPlayerChanged();
        }

        private static void OnAssignablePlayersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerSlotView view = d as PlayerSlotView;
            if (view == null)
                return;
            if (view._updateScope.IsWithin)
                return;
            view.OnAssignablePlayersChanged();
        }

        public event EventHandler SlotClosed;
        public event EventHandler SlotOpened;
        public event EventHandler AssignablePlayersChanged;
        public event EventHandler AssignedPlayerChanged;

        public IDisposable EnterUpdateScope()
        {
            return _updateScope.Enter();
        }

        protected void OnSlotClosed()
        {
            EventHandler handler = SlotClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnSlotOpened()
        {
            EventHandler handler = SlotOpened;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnAssignablePlayersChanged()
        {
            EventHandler handler = AssignablePlayersChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnAssignedPlayerChanged()
        {
            EventHandler handler = AssignedPlayerChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public PlayerSlotView()
        {
            _updateScope = new StateScope();

            InitializeComponent();
        }

        public PlayerSlot Slot
        {
            get { return GetValue(SlotProperty) as PlayerSlot; }
            set { SetValue(SlotProperty, value); }
        }

        public IEnumerable<Player> AssignablePlayers
        {
            get { return GetValue(AssignablePlayersProperty) as IEnumerable<Player>; }
            set { SetValue(AssignablePlayersProperty, value); }
        }

        public Player AssignedPlayer
        {
            get { return GetValue(AssignedPlayerProperty) as Player; }
            set { SetValue(AssignedPlayerProperty, value); }
        }

        private void OnIsOpenCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (_updateScope.IsWithin)
                return;
            if (IsOpenCheckBox.IsChecked.HasValue && IsOpenCheckBox.IsChecked.Value)
                OnSlotOpened();
            else if (IsOpenCheckBox.IsChecked.HasValue && !IsOpenCheckBox.IsChecked.Value)
                OnSlotClosed();
        }
    }
}
