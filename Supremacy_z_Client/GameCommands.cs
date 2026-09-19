// GameCommands.cs
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
    public static class GameCommands
    {
        public static readonly RoutedCommand BeginSinglePlayerGameCommand;
        public static readonly RoutedCommand LoadSinglePlayerGameCommand;
        public static readonly RoutedCommand SaveGameCommand;

        static GameCommands()
        {
            BeginSinglePlayerGameCommand = new RoutedCommand(
                "BeginSinglePlayerGame",
                typeof(GameCommands));
            LoadSinglePlayerGameCommand = new RoutedCommand(
                "LoadSinglePlayerGame",
                typeof(GameCommands));
            SaveGameCommand = new RoutedCommand(
                "SaveGame",
                typeof(GameCommands));
        }
    }
}
