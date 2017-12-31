// IClientContext.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections.Generic;
using System.ComponentModel;

using Supremacy.Collections;
using Supremacy.Game;

namespace Supremacy.Client
{
    public interface IClientContext : INotifyPropertyChanged
    {
        #region Properties and Indexers
        IGameContext CurrentGame { get; }
        bool IsConnected { get; }
        bool IsGameHost { get; }
        bool IsGameInPlay { get; }
        bool IsGameEnding { get; }
        bool IsSinglePlayerGame { get; }

        bool IsFederationPlayable { get; }
        bool IsRomulanPlayable { get; }
        bool IsKlingonPlayable { get; }
        bool IsCardassianPlayable { get; }
        bool IsDominionPlayable { get; }
        bool IsBorgPlayable { get; }
        bool IsTerranEmpirePlayable { get; }

        IPlayer LocalPlayer { get; }
        ILobbyData LobbyData { get; }
        CivilizationManager LocalPlayerEmpire { get; }
        IEnumerable<IPlayer> RemotePlayers { get; }
        IKeyedCollection<int, IPlayer> Players { get; }
        bool IsTurnFinished { get; }
        #endregion
    }
}