// ServiceClient.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Supremacy.Combat;
using Supremacy.Game;
using Supremacy.WCF;

namespace Supremacy.Client
{
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IgnoreExtensionDataObject = true,
        UseSynchronizationContext = false)]
    public class ServiceClient : DuplexClientBase<ISupremacyService>, ISupremacyService
    {
        #region Constructors

        public ServiceClient(InstanceContext callbackInstance) 
            : base(callbackInstance)
        {
        }
        
        public ServiceClient(InstanceContext callbackInstance, string endpointConfigurationName) 
            : base(callbackInstance, endpointConfigurationName)
        {
        }
        
        public ServiceClient(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) 
            : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }
        
        public ServiceClient(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) 
            : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public ServiceClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base(callbackInstance, binding, remoteAddress)
        {
        }

        #endregion

        #region ISupremacyService Members

        public HostGameResult HostGame(GameInitData initData, out Player localPlayer, out LobbyData lobbyData)
        {
            try
            {
                return Channel.HostGame(initData, out localPlayer, out lobbyData);
            }
            catch (FaultException)
            {
                localPlayer = null;
                lobbyData = null;
                return HostGameResult.ChannelFaultFailure;
            }
        }

        public JoinGameResult JoinGame(string playerName, out Player localPlayer, out LobbyData lobbyData)
        {
            try
            {
                return Channel.JoinGame(playerName, out localPlayer, out lobbyData);
            }
            catch
            {
                localPlayer = null;
                lobbyData = null;
                return JoinGameResult.ConnectionFailure;
            }
        }

        public void SaveGame(string fileName)
        {
            try { Channel.SaveGame(fileName); }
            catch (FaultException) {}
        }

        public void SendChatMessage(string message, int recipientId)
        {
            try { Channel.SendChatMessage(message, recipientId); }
            catch (FaultException) {}
        }

        public bool IsClosing
        {
            get { return (InnerChannel.State == CommunicationState.Closing); }
        }

        public bool IsClosed
        {
            get { return (InnerChannel.State == CommunicationState.Closed); }
        }

        public void Pong(int pingId)
        {
            try
            {
                if (IsClosing || IsClosed)
                    return;
                Channel.Pong(pingId);
            }
            catch (CommunicationException) {}
            catch (InvalidOperationException) {}
        }

        public void Disconnect()
        {
            try { Channel.Disconnect(); }
            catch (FaultException) {}
            catch (InvalidOperationException) {}
        }

        public void EndTurn(PlayerOrdersMessage orders)
        {
            try { Channel.EndTurn(orders); }
            catch (FaultException) {}
        }

        public void UpdateGameOptions(GameOptions options)
        {
            try { Channel.UpdateGameOptions(options); }
            catch (FaultException) { }
        }

        public void ClearPlayerSlot(int slotId)
        {
            try { Channel.ClearPlayerSlot(slotId); }
            catch (FaultException) {}
        }

        public void ClosePlayerSlot(int slotId)
        {
            try { Channel.ClosePlayerSlot(slotId); }
            catch (FaultException) {}
        }

        public void AssignPlayerSlot(int slotId, int playerId)
        {
            try { Channel.AssignPlayerSlot(slotId, playerId); }
            catch (FaultException) {}
        }

        public void StartGame()
        {
            try { Channel.StartGame(); }
            catch (FaultException) {}
        }

        public int GetNewObjectID()
        {
            try { return Channel.GetNewObjectID(); }
            catch (FaultException) { return GameObjectID.InvalidID; }
        }

        public void SendCombatOrders(CombatOrders orders)
        {
            try { Channel.SendCombatOrders(orders); }
            catch (FaultException) {}
        }

        public void SendInvasionOrders(InvasionOrders orders)
        {
            try { Channel.SendInvasionOrders(orders); }
            catch (FaultException) {}
        }

        public void NotifyInvasionScreenReady()
        {
            try { Channel.NotifyInvasionScreenReady(); }
            catch (FaultException) { }
        }

        #endregion
    }
}
