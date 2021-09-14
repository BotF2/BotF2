// IGameClient.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

using Supremacy.Annotations;
using Supremacy.Client.Events;
using Supremacy.Game;
using Supremacy.Network;
using System.Linq;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public enum ClientDisconnectReason
    {
        Disconnected,
        ConnectionClosed,
        ConnectionBroken,
        LocalServiceFailure,
        GameIsFull,
        GameAlreadyStarted,
        LoadGameFailure,
        VersionMismatch,
        UnknownFailure
    }

    public interface IGameClient
    {
        #region Events
        event Action<ClientEventArgs> Connected;
        event Action<ClientDataEventArgs<ClientDisconnectReason>> Disconnected;
        #endregion

        #region Properties and Indexers
        bool IsConnected { get; }
        #endregion

        #region Public and Protected Methods
        void Connect(string playerName, [NotNull] IPAddress remoteServerAddress);
        void HostAndConnect([NotNull] GameInitData initData, [NotNull] IPAddress remoteServerAddress);
        void Disconnect();
        #endregion
    }

    public class ClientException : ApplicationException
    {
        #region Constructors and Finalizers
        public ClientException() { }
        public ClientException(string message) : base(message) { }
        public ClientException(string message, Exception innerException) : base(message, innerException) { }
        protected ClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }

    public static class GameClientExtensions
    {
        #region Extension Methods
        public static void Connect(
            [NotNull] this IGameClient self,
            [NotNull] string playerName,
            [NotNull] string remoteServerHostName)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (playerName == null)
            {
                throw new ArgumentNullException("playerName");
            }

            if (remoteServerHostName == null)
            {
                throw new ArgumentNullException("remoteServerHostName");
            }

            if (IPAddress.TryParse(remoteServerHostName, out IPAddress hostAddress))
            {
                self.Connect(playerName, hostAddress);
            }
            else
            {
                IPHostEntry hostEntry = NetUtility.Resolve(remoteServerHostName);
                bool succeeded = false;
                if (hostEntry != null)
                {
                    foreach (IPAddress address in hostEntry.AddressList.Where(o => o.AddressFamily == AddressFamily.InterNetwork))
                    {
                        try
                        {
                            self.Connect(playerName, address);
                            succeeded = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            GameLog.Client.General.Error(e);
                        }
                    }
                }

                if (!succeeded)
                {
                    throw new ClientException("Could not connect to host at " + remoteServerHostName + ".");
                }
            }
        }

        public static void HostAndConnect(
            [NotNull] this IGameClient self,
            [NotNull] GameInitData initData,
            [NotNull] string remoteServerHostName)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (initData == null)
            {
                throw new ArgumentNullException("initData");
            }

            if (remoteServerHostName == null)
            {
                throw new ArgumentNullException("remoteServerHostName");
            }

            if (IPAddress.TryParse(remoteServerHostName, out IPAddress hostAddress))
            {
                self.HostAndConnect(initData, hostAddress);
            }
            else
            {
                IPHostEntry hostEntry = NetUtility.Resolve(remoteServerHostName);
                bool succeeded = false;
                if (hostEntry != null)
                {
                    foreach (IPAddress address in hostEntry.AddressList)
                    {
                        try
                        {
                            self.HostAndConnect(initData, address);
                            succeeded = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            GameLog.Client.General.Error(e);
                        }
                    }
                }

                if (!succeeded)
                {
                    throw new ClientException("Could not connect to host at " + remoteServerHostName + ".");
                }
            }
        }
        #endregion
    }
}