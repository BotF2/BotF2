// LobbyPlayerInfoControl.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Supremacy.Game;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for LobbyPlayerInfoControl.xaml
    /// </summary>

    public partial class LobbyPlayerInfoControl : UserControl
    {
        public static readonly DependencyProperty LobbyDataProperty;
        public static readonly DependencyProperty PlayerProperty;

        static LobbyPlayerInfoControl()
        {
            LobbyDataProperty = DependencyProperty.Register(
                "LobbyData",
                typeof(LobbyData),
                typeof(LobbyPlayerInfoControl));
            PlayerProperty = DependencyProperty.Register(
                "Player",
                typeof(Player),
                typeof(LobbyPlayerInfoControl));
        }

        public event EventHandler EmpireChanged;

        public LobbyPlayerInfoControl(LobbyData lobbyData, Player player)
        {
            InitializeComponent();
            LobbyData = lobbyData;
            Player = player;
            EmpireList.SelectionChanged += EmpireList_SelectionChanged;
        }

        void EmpireList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmpireChanged != null)
                EmpireChanged(this, new EventArgs());
        }

        public int SelectedEmpireID
        {
            get { return ((KeyValuePair<int, string>)EmpireList.SelectedItem).Key; }
        }

        public LobbyData LobbyData
        {
            get { return GetValue(LobbyDataProperty) as LobbyData; }
            set { SetValue(LobbyDataProperty, value); }
        }

        public Player Player
        {
            get { return GetValue(PlayerProperty) as Player; }
            set
            {
                SetValue(PlayerProperty, value);
                Update();
            }
        }

        protected void Update()
        {
            Dictionary<int, string> empires = new Dictionary<int, string>();
            List<KeyValuePair<int, string>> itemsSource = new List<KeyValuePair<int, string>>();
            KeyValuePair<int, string> playerEmpire = new KeyValuePair<int, string>(Player.InvalidEmpireID, "Random");
            PlayerNameText.Text = Player.Name;
            empires.Add(Player.InvalidEmpireID, "Random");
            for (int i = 0; i < LobbyData.Empires.Length; i++)
            {
                empires.Add(i, LobbyData.Empires[i]);
                if (i == Player.EmpireID)
                    playerEmpire = new KeyValuePair<int, string>(i, LobbyData.Empires[i]);
            }
            foreach (Player player in LobbyData.Players)
            {
                if (player != Player)
                {
                    if (player.EmpireID >= 0)
                        empires.Remove(player.EmpireID);
                }
            }
            itemsSource.AddRange(empires);
            EmpireList.ItemsSource = itemsSource;
            EmpireList.SelectedItem = playerEmpire;
        }
    }
}