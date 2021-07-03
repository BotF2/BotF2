// GameOptionsWindow.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows;

using Supremacy.Game;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for GameOptionsWindow.xaml
    /// </summary>

    public partial class GameOptionsWindow : Window
    {

        public GameOptionsWindow()
        {
            InitializeComponent();
        }

        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _ = GameOptionsManager.SaveDefaults(Options);
            DialogResult = true;
        }

        public GameOptions Options => OptionsPanel.Options;

    }
}