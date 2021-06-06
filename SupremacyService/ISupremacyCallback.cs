// ISupremacyCallback.cs
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

namespace Supremacy.WCF
{
    public interface ISupremacyCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotifyOnJoin(Player localPlayer, LobbyData lobbyData);

        [OperationContract(IsOneWay = true)]
        void NotifyPlayerJoined(Player player);

        [OperationContract(IsOneWay = true)]
        void NotifyPlayerExited(Player player);

        [OperationContract(IsOneWay = true)]
        void NotifyGameStarting();

        [OperationContract(IsOneWay = true)]
        void NotifyGameStarted(GameStartMessage startMessage);

        [OperationContract(IsOneWay = true)]
        void NotifyTurnProgressChanged(TurnPhase phase);

        [OperationContract(IsOneWay = true)]
        void NotifyGameDataUpdated(GameUpdateMessage updateMessage);

        [OperationContract(IsOneWay = true)]
        void NotifyAllTurnEnded();

        [OperationContract(IsOneWay = true)]
        void NotifyTurnFinished();

        [OperationContract(IsOneWay = true)]
        void NotifyChatMessageReceived(int senderId, string message, int recipientId);

        [OperationContract(IsOneWay = true)]
        void NotifyLobbyUpdated(LobbyData lobbyData);

        [OperationContract(IsOneWay = true)]
        void NotifyDisconnected();

        [OperationContract(IsOneWay = true)]
        void Ping();

        [OperationContract(IsOneWay = true)]
        void NotifyCombatUpdate(CombatUpdate update);

        [OperationContract(IsOneWay = true)]
        void NotifyInvasionUpdate(InvasionArena update);

        [OperationContract(IsOneWay = true)]
        void NotifyPlayerFinishedTurn(int empireId);
    }
}
