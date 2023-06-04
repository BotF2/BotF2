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
        public static readonly CompositeCommand TracesCommand = new CompositeCommand(true);

        public static readonly CompositeCommand F06_Command = new CompositeCommand(true);  // F6
        public static readonly CompositeCommand F07_Command = new CompositeCommand(true);  // F7
        public static readonly CompositeCommand F08_Command = new CompositeCommand(true);  // F8 
        public static readonly CompositeCommand F09_Command = new CompositeCommand(true);  // F9
        public static readonly CompositeCommand F10_Command = new CompositeCommand(true); // 
        public static readonly CompositeCommand F11_Command = new CompositeCommand(true);
        public static readonly CompositeCommand F12_Command = new CompositeCommand(true);


        public static readonly CompositeCommand CTRL_F01_Command = new CompositeCommand(true);  // F1 + CTRL
        public static readonly CompositeCommand CTRL_F02_Command = new CompositeCommand(true);  // F2 + CTRL
        public static readonly CompositeCommand CTRL_F03_Command = new CompositeCommand(true);  // F3 + CTRL
        public static readonly CompositeCommand CTRL_F04_Command = new CompositeCommand(true);  // F4 + CTRL
        public static readonly CompositeCommand CTRL_F05_Command = new CompositeCommand(true);  // F5 + CTRL
        public static readonly CompositeCommand CTRL_F06_Command = new CompositeCommand(true);  // F6 + CTRL
        public static readonly CompositeCommand CTRL_F07_Command = new CompositeCommand(true);  // F7 + CTRL
        public static readonly CompositeCommand CTRL_F08_Command = new CompositeCommand(true);  // F8 + CTRL
        public static readonly CompositeCommand CTRL_F09_Command = new CompositeCommand(true);  // F9 + CTRL
        public static readonly CompositeCommand CTRL_F10_Command = new CompositeCommand(true); // 
        public static readonly CompositeCommand CTRL_F11_Command = new CompositeCommand(true);
        public static readonly CompositeCommand CTRL_F12_Command = new CompositeCommand(true);

        public static readonly CompositeCommand S0_Command = new CompositeCommand(true);  // Start Single Player Empire x
        public static readonly CompositeCommand S1_Command = new CompositeCommand(true);
        public static readonly CompositeCommand S2_Command = new CompositeCommand(true);
        public static readonly CompositeCommand S3_Command = new CompositeCommand(true);
        public static readonly CompositeCommand S4_Command = new CompositeCommand(true);
        public static readonly CompositeCommand S5_Command = new CompositeCommand(true);
        public static readonly CompositeCommand S6_Command = new CompositeCommand(true);

        public static readonly CompositeCommand Hotkey_Alt_D0 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D1 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D2 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D3 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D4 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D5 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D6 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D7 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D8 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D9 = new CompositeCommand(true);


        public static readonly CompositeCommand Hotkey_Alt_F01 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F02 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F03 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F04 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F05 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F06 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F07 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F08 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F09 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F10 = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F11 = new CompositeCommand(true);

        public static readonly CompositeCommand Hotkey_Alt_A = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_B = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_C = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_D = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_E = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_F = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_G = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_H = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_I = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_J = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_K = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_L = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_M = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_N = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_O = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_P = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_Q = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_R = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_S = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_T = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_U = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_V = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_W = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_X = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_Y = new CompositeCommand(true);
        public static readonly CompositeCommand Hotkey_Alt_Z = new CompositeCommand(true);

        //public static readonly CompositeCommand FakeCommand = new CompositeCommand(true);
        public static readonly CompositeCommand LogTxtCommand = new CompositeCommand(true);
        public static readonly CompositeCommand ErrorTxtCommand = new CompositeCommand(true);
        public static readonly CompositeCommand ShowSettingsFileCommand = new CompositeCommand(true);
        public static readonly CompositeCommand ShowPlayersHistoryFileCommand = new CompositeCommand(true);
        public static readonly CompositeCommand ShowAllHistoryFileCommand = new CompositeCommand(true);
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
        public static readonly CompositeCommand SaveGameDeleteManualSaved = new CompositeCommand(true);
        public static readonly CompositeCommand SaveGameDeleteAutoSaved = new CompositeCommand(true);
        public static readonly CompositeCommand ShowSaveGameDialog = new CompositeCommand(true);
        public static readonly CompositeCommand CancelCommand = new CompositeCommand(true);
        public static readonly CompositeCommand EndTurn = new CompositeCommand(true);
        public static readonly CompositeCommand ShowEndOfTurnSummary = new CompositeCommand(true);
        //public static readonly CompositeCommand ShowShipOverview = new CompositeCommand(true);
        public static readonly CompositeCommand SendChatMessage = new CompositeCommand(true);
        public static readonly CompositeCommand EndGame = new CompositeCommand(true);
        public static readonly CompositeCommand Exit = new CompositeCommand(false);
        public static readonly CompositeCommand SendCombatOrders = new CompositeCommand(true);
        public static readonly CompositeCommand SendCombatTarget1 = new CompositeCommand(true);
        public static readonly CompositeCommand SendCombatTarget2 = new CompositeCommand(true);
        public static readonly CompositeCommand SendIntelOrders = new CompositeCommand(true);
        public static readonly CompositeCommand SendInvasionOrders = new CompositeCommand(true);
        public static readonly CompositeCommand EndInvasion = new CompositeCommand(true);

        public static readonly RoutedCommand EscapeCommand = new RoutedCommand("Escape", typeof(ClientCommands));
        public static readonly RoutedCommand AutoTurnCommand = new RoutedCommand("AutoTurn", typeof(ClientCommands));

        public static readonly CompositeCommand OnMA_Ferengi = new CompositeCommand(true);
    }
}