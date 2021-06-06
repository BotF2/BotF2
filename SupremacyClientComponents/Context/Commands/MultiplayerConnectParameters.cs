// ClientCommands.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Client.Commands
{
    public class MultiplayerConnectParameters
    {
        private readonly string _playerName;
        private readonly string _remoteHost;

        public MultiplayerConnectParameters([NotNull] string playerName, [NotNull] string remoteHost)
        {
            if (playerName == null)
                throw new ArgumentNullException("playerName");
            if (remoteHost == null)
                throw new ArgumentNullException("remoteHost");
            _playerName = playerName;
            _remoteHost = remoteHost;
        }

        public string PlayerName => _playerName;

        public string RemoteHost => _remoteHost;
    }
}