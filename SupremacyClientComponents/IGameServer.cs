// IGameServer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;

namespace Supremacy.Client
{
    public interface IGameServer : IDisposable
    {
        #region Events
        event Action<EventArgs> Faulted;
        event Action<EventArgs> Started;
        event Action<EventArgs> Stopped;
        #endregion

        #region Properties and Indexers
        bool IsRunning { get; }
        #endregion

        #region Public and Protected Methods
        void Start(GameOptions gameOptions, bool allowRemoteConnections);
        void Stop();
        #endregion
    }
}