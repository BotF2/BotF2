// ISupremacyService.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.ServiceModel;

using Supremacy.Combat;
using Supremacy.Game;
using Supremacy.Intelligence;

namespace Supremacy.WCF
{
    [ServiceContract(
        Namespace = "http://Supremacy.WPF",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(ISupremacyCallback))]
    public interface ISupremacyService
    {
        [OperationContract(
            IsOneWay = false,
            IsInitiating = true,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/HostGame",
            ReplyAction = "http://Supremacy.WPF/ISupremacyService/HostGameResponse")]
        HostGameResult HostGame(GameInitData initData, out Player localPlayer, out LobbyData lobbyData);

        [OperationContract(
            IsOneWay = false,
            IsInitiating = true,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/JoinGame",
            ReplyAction = "http://Supremacy.WPF/ISupremacyService/JoinGameResponse")]
        JoinGameResult JoinGame(string playerName, out Player localPlayer, out LobbyData lobbyData);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = true,
            Action = "http://Supremacy.WPF/ISupremacyService/Disconnect")]
        void Disconnect();

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/StartGame")]
        void StartGame();

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/Pong")]
        void Pong(int pingId);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SendChatMessage")]
        void SendChatMessage(string message, int recipientId);

        [OperationContract(
            IsOneWay = true,
            //Name = "EndTurnOrder",
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/EndTurn")]
        void EndTurn(PlayerOrdersMessage orders); // not combat orders, see combatorders below

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SaveGame")]
        void SaveGame(string fileName);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/UpdateGameOptions")]
        void UpdateGameOptions(GameOptions options);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/ClearPlayerSlot")]
        void ClearPlayerSlot(int slotId);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/ClosePlayerSlot")]
        void ClosePlayerSlot(int slotId);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/AssignPlayerSlot")]
        void AssignPlayerSlot(int slotId, int playerId);

        [OperationContract(
            IsOneWay = false,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/GetNewObjectID")]
        int GetNewObjectID();

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SendCombatOrders")]
        void SendCombatOrders(CombatOrders orders);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SendCombatTarget1")]
        void SendCombatTarget1(CombatTargetPrimaries target1);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SendCombatTarget2")]
        void SendCombatTarget2(CombatTargetSecondaries target2);

        //[OperationContract(
        //    IsOneWay = true,
        //    IsInitiating = false,
        //    IsTerminating = false,
        //    Action = "http://Supremacy.WPF/ISupremacyService/SendIntelOrders")]
        //void SendIntelOrders(IntelOrders orders);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/SendInvasionOrders")]
        void SendInvasionOrders(InvasionOrders orders);

        [OperationContract(
            IsOneWay = true,
            IsInitiating = false,
            IsTerminating = false,
            Action = "http://Supremacy.WPF/ISupremacyService/NotifyInvasionScreenReady")]
        void NotifyInvasionScreenReady();
    }
}