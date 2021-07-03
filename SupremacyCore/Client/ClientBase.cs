// ClientBase.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Game;
using Supremacy.Network;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace Supremacy.Client
{
    public delegate void PlayerEventHandler(Player player);
    public delegate void LobbyEventHandler(LobbyData lobbyData);
    public delegate void ChatMessageEventHandler(ChatMessage message);
    public delegate void CombatUpdateHandler(CombatUpdate update);

    [Serializable]
    public abstract class ClientBase : INotifyPropertyChanged
    {
        #region Static Members

        public static ClientBase Current { get; set; }
        #endregion

        #region Fields
        private readonly List<Order> _orders;
        private readonly List<Order> _target1;
        private readonly List<Order> _target2;
        #endregion

        #region Events
        public event DefaultEventHandler GameStarting;
        public event DefaultEventHandler GameStarted;
        public event TurnPhaseEventHandler TurnPhaseChanged;
        public event DefaultEventHandler TurnFinished;
        public event DefaultEventHandler PlayerTurnFinished;
        public event PlayerEventHandler PlayerJoined;
        public event PlayerEventHandler PlayerExited;
        public event DefaultEventHandler Connected;
        public event DefaultEventHandler ConnectionBroken;
        public event DefaultEventHandler Disconnected;
        public event LobbyEventHandler LobbyUpdated;
        public event ChatMessageEventHandler ChatMessageReceived;
        public event DefaultEventHandler GameEnded;
        public event CombatUpdateHandler CombatUpdate;
        #endregion

        #region Properties
        public IList<Order> Orders => _orders.AsReadOnly();
        public IList<Order> Target1 => _target1.AsReadOnly();
        public IList<Order> Target2 => _target2.AsReadOnly();
        public abstract GameContext Game { get; protected set; }
        public abstract TurnPhase TurnPhase { get; protected set; }
        public abstract Player LocalPlayer { get; protected set; }
        public abstract IIndexedCollection<Player> Players { get; protected set; }
        public abstract LobbyData LobbyData { get; protected set; }
        public abstract CivilizationManager CivilizationManager { get; protected internal set; }
        public abstract bool IsConnected { get; }
        public abstract bool IsGameHost { get; }
        public abstract bool IsGameInPlay { get; }
        public abstract bool IsSinglePlayerGame { get; }

        public abstract bool IsFederationPlayable { get; }
        public abstract bool IsRomulanPlayable { get; }
        public abstract bool IsKlingonPlayable { get; }
        public abstract bool IsCardassianPlayable { get; }
        public abstract bool IsDominionPlayable { get; }
        public abstract bool IsBorgPlayable { get; }
        public abstract bool IsTerranEmpirePlayable { get; }

        public abstract bool IsTurnFinished { get; }
        #endregion

        #region Constructors
        protected ClientBase()
        {
            _orders = new List<Order>();
            _target1 = new List<Order>();
            _target2 = new List<Order>();
        }
        #endregion

        #region Event Invokers
        protected void OnGameStarting()
        {
            GameStarting?.Invoke();
        }

        protected void OnGameStarted()
        {
            PlayerContext.Current = new PlayerContext(Players);
            GameStarted?.Invoke();
        }

        protected void OnTurnPhaseChanged(TurnPhase phase)
        {
            TurnPhaseChanged?.Invoke(phase);
        }

        protected void OnTurnFinished()
        {
            TurnFinished?.Invoke();
        }

        protected void OnPlayerTurnFinished()
        {
            PlayerTurnFinished?.Invoke();
        }

        protected void OnPlayerJoined(Player player)
        {
            PlayerJoined?.Invoke(player);
        }

        protected void OnPlayerExited(Player player)
        {
            PlayerExited?.Invoke(player);
        }

        protected void OnConnected()
        {
            Connected?.Invoke();
        }

        protected void OnConnectionBroken()
        {
            ConnectionBroken?.Invoke();
        }

        protected void OnDisconnected()
        {
            Disconnected?.Invoke();
        }

        protected void OnLobbyUpdated(LobbyData lobbyData)
        {
            LobbyUpdated?.Invoke(lobbyData);
        }

        protected void OnChatMessageReceived(ChatMessage message)
        {
            ChatMessageReceived?.Invoke(message);
        }

        protected void OnGameEnded()
        {
            GameEnded?.Invoke();
            PlayerContext.Current = null;
        }

        protected void OnCombatUpdate(CombatUpdate update)
        {
            if ((update != null) && (CombatUpdate != null))
            {
                CombatUpdate(update);
            }
        }
        #endregion

        #region Methods
        public void AddOrder(Order order)
        {
            if (order == null)
            {
                return;
            }

            order.Owner = LocalPlayer.Empire;
            lock (_orders)
            {
                while ((_orders.Count > 0) && order.Overrides(_orders[_orders.Count - 1]))
                {
                    _orders.RemoveAt(_orders.Count - 1);
                }

                _orders.Add(order);
            }
        }

        public bool RemoveOrder(Order order)
        {
            if (order == null)
            {
                return false;
            }

            lock (_orders)
            {
                return _orders.Remove(order);
            }
        }

        public void ClearOrders()
        {
            _orders.Clear();
        }
        public void AddTarget1(Order target1)
        {
            if (target1 == null)
            {
                return;
            }

            target1.Owner = LocalPlayer.Empire;
            lock (_target1)
            {
                while ((_target1.Count > 0) && target1.Overrides(_target1[_target1.Count - 1]))
                {
                    _target1.RemoveAt(_target1.Count - 1);
                }

                _target1.Add(target1);
            }
        }

        public bool RemoveTarget1(Order target1)
        {
            if (target1 == null)
            {
                return false;
            }

            lock (_target1)
            {
                return _target1.Remove(target1);
            }
        }

        public void ClearTarget1()
        {
            _target1.Clear();
        }

        public void AddTarget2(Order target)
        {
            if (target == null)
            {
                return;
            }

            target.Owner = LocalPlayer.Empire;
            lock (_target2)
            {
                while ((_target2.Count > 0) && target.Overrides(_target2[_target2.Count - 1]))
                {
                    _target2.RemoveAt(_target2.Count - 1);
                }

                _target2.Add(target);
            }
        }

        public bool RemoveTarget2(Order target)
        {
            if (target == null)
            {
                return false;
            }

            lock (_target2)
            {
                return _target2.Remove(target);
            }
        }

        public void ClearTarget2()
        {
            _target2.Clear();
        }

        public abstract void HostGame(string playerName);
        public abstract void HostSinglePlayerGame(GameOptions options, int empireId);
        public abstract void LoadSinglePlayerGame(string fileName);

        public void HostSinglePlayerGame()
        {
            HostSinglePlayerGame(GameOptionsManager.LoadDefaults(), 0);
        }

        public abstract void JoinGame(string playerName, IPAddress server);

        public void JoinGame(string playerName, string host)
        {
            if (IPAddress.TryParse(host, out IPAddress hostAddress))
            {
                JoinGame(playerName, hostAddress);
            }
            else
            {
                bool succeeded = false;
                IPHostEntry hostEntry = NetUtility.Resolve(host);
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    try
                    {
                        JoinGame(playerName, address);
                        succeeded = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }
                if (!succeeded)
                {
                    throw new SupremacyException(
                        "Could not connect to host at " + host + ".");
                }
            }
        }

        public abstract void Disconnect();

        public abstract void StartGame();
        public abstract void EndGame();

        public abstract void SaveGame(string fileName);

        public abstract void FinishTurn();

        public abstract void SendChatMessage(string message);
        public abstract void SendChatMessage(Player recipient, string message);

        public abstract void SendCombatOrders(CombatOrders orders);
        public abstract void SendCombatTarget1(CombatTargetPrimaries target1);
        public abstract void SendCombatTarget2(CombatTargetSecondaries target2);

        //  public abstract void SendIntelOrders(IntelOrders orders);
        public abstract void UpdateGameOptions(GameOptions options);
        public abstract void UpdateEmpireSelection(int playerId, int empireId);

        public abstract int GetNewObjectID();

        //public void StartServer()
        //{
        //    lock (serverLock)
        //    {
        //        if (!isServerLoaded)
        //        {
        //            try
        //            {
        //                serverDomain = AppDomain.CreateDomain("SupremacyServerDomain");
        //                serverDomain.Load(Assembly.GetExecutingAssembly().GetName());
        //                networkServer = (NetServer)serverDomain.CreateInstanceAndUnwrap(
        //                    Assembly.GetExecutingAssembly().FullName,
        //                    "Supremacy.Network.NetServer");
        //                if (networkServer != null)
        //                {
        //                    isServerLoaded = true;
        //                    networkServer.StartListening();
        //                }
        //            }
        //            catch
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}

        //public void StopServer()
        //{
        //    lock (serverLock)
        //    {
        //        if (isServerLoaded)
        //        {
        //            try
        //            {
        //                if (networkServer != null)
        //                {
        //                    networkServer.StopListening();
        //                    networkServer = null;
        //                }
        //                if (serverDomain != null)
        //                {
        //                    AppDomain.Unload(serverDomain);
        //                }
        //            }
        //            finally
        //            {
        //                isServerLoaded = false;
        //            }
        //        }
        //    }
        //}
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
