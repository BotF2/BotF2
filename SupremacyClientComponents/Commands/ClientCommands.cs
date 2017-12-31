// ClientCommands.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Input;

using Microsoft.Practices.Composite.Presentation.Commands;

namespace Supremacy.Client.Commands
{
    public static class ClientCommands
    {
        public static readonly CompositeCommand StartSinglePlayerGame = new CompositeCommand(true);
        public static readonly CompositeCommand StartMultiplayerGame = new CompositeCommand(true);
        public static readonly CompositeCommand ContinueGame = new CompositeCommand(true);
        public static readonly CompositeCommand OptionsCommand = new CompositeCommand(true);
        public static readonly CompositeCommand ShowCreditsDialog = new CompositeCommand(true);
        public static readonly CompositeCommand ShowMultiplayerConnectDialog = new CompositeCommand(true);
        public static readonly CompositeCommand HostMultiplayerGame = new CompositeCommand(true);
        public static readonly CompositeCommand JoinMultiplayerGame = new CompositeCommand(true);
        public static readonly CompositeCommand AssignPlayerSlot = new CompositeCommand(true);
        public static readonly CompositeCommand ClearPlayerSlot = new CompositeCommand(true);
        public static readonly CompositeCommand ClosePlayerSlot = new CompositeCommand(true);
        public static readonly CompositeCommand SendUpdatedGameOptions = new CompositeCommand(true);
        public static readonly CompositeCommand LoadGame = new CompositeCommand(true);
        public static readonly CompositeCommand SaveGame = new CompositeCommand(true);
        public static readonly CompositeCommand ShowSaveGameDialog = new CompositeCommand(true);
        public static readonly CompositeCommand CancelCommand = new CompositeCommand(true);
        public static readonly CompositeCommand EndTurn = new CompositeCommand(true);
        public static readonly CompositeCommand ShowEndOfTurnSummary = new CompositeCommand(true);
        public static readonly CompositeCommand SendChatMessage = new CompositeCommand(true);
        public static readonly CompositeCommand EndGame = new CompositeCommand(true);
        public static readonly CompositeCommand Exit = new CompositeCommand(false);
        public static readonly CompositeCommand SendCombatOrders = new CompositeCommand(true);
        public static readonly CompositeCommand SendInvasionOrders = new CompositeCommand(true);
        public static readonly CompositeCommand EndInvasion = new CompositeCommand(true);
        public static readonly CompositeCommand ToggleConsole = new CompositeCommand(false);
        public static readonly CompositeCommand ConsoleCommand = new CompositeCommand(false);
        
        public static readonly RoutedCommand EscapeCommand = new RoutedCommand("Escape", typeof(ClientCommands));
    }
}