// ClientEvents.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Threading;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Combat;
using Supremacy.Game;
using Supremacy.Client.Context;

namespace Supremacy.Client.Events
{
    public class ViewActivatingEventArgs : ClientCancelEventArgs
    {
        public object View { get; private set; }

        public ViewActivatingEventArgs([NotNull] object view)
        {
            View = view ?? throw new ArgumentNullException("view");
        }
    }

    public class ScreenActivatedEventArgs : ClientEventArgs
    {
        public string ScreenName { get; }

        public ScreenActivatedEventArgs([NotNull] string screenName)
        {
            ScreenName = screenName ?? throw new ArgumentNullException("screenName");
        }
    }

    public class ClientCancelEventArgs : CancelEventArgs
    {
        public static readonly ClientCancelEventArgs Default;

        public IAppContext AppContext { get; private set; }

        static ClientCancelEventArgs()
        {
            Default = new ClientCancelEventArgs(ServiceLocator.Current.GetInstance<IAppContext>());
        }

        public ClientCancelEventArgs([NotNull] IAppContext appContext)
        {
            AppContext = appContext ?? throw new ArgumentNullException("appContext");
        }

        public ClientCancelEventArgs() : this(ServiceLocator.Current.GetInstance<IAppContext>()) { }
    }

    public class ClientEventArgs : EventArgs
    {
        public static readonly ClientEventArgs Default;

        static ClientEventArgs()
        {
            Default = Designer.IsInDesignMode ? new ClientEventArgs() : new ClientEventArgs(ServiceLocator.Current.GetInstance<IAppContext>());
        }

        public IAppContext AppContext { get; private set; }

        public ClientEventArgs([NotNull] IAppContext appContext)
        {
            AppContext = appContext ?? throw new ArgumentNullException("appContext");
        }

        public ClientEventArgs()
        {
            AppContext = Designer.IsInDesignMode
                                     ? null
                                     : ServiceLocator.Current.GetInstance<IAppContext>();
        }
    }

    public class ClientDataEventArgs<TData> : DataEventArgs<TData>
    {
        public IAppContext AppContext { get; private set; }

        public ClientDataEventArgs([NotNull] IAppContext appContext, TData value) : base(value)
        {
            AppContext = appContext ?? throw new ArgumentNullException("appContext");
        }

        public ClientDataEventArgs(TData value) : this(ServiceLocator.Current.GetInstance<IAppContext>(), value) { }
    }

    public sealed class LocalPlayerJoinedEventArgs : ClientEventArgs
    {
        public LocalPlayerJoinedEventArgs([NotNull] IAppContext appContext, [NotNull] IPlayer player, [NotNull] ILobbyData lobbyData) : base(appContext)
        {
            Player = player ?? throw new ArgumentNullException("player");
            LobbyData = lobbyData ?? throw new ArgumentNullException("lobbyData");
        }

        public LocalPlayerJoinedEventArgs([NotNull] IPlayer player, [NotNull] ILobbyData lobbyData)
            : this(ServiceLocator.Current.GetInstance<IAppContext>(), player, lobbyData) { }

        public IPlayer Player { get; private set; }
        public ILobbyData LobbyData { get; private set; }
    }

    public sealed class ClientConnectedEventArgs : ClientEventArgs
    {
        public bool IsServerLocal { get; private set; }

        public ClientConnectedEventArgs(bool isServerLocal)
        {
            IsServerLocal = isServerLocal;
        }
    }

    public sealed class GameObjectIDRequestEventArgs : GameContextEventArgs
    {
        public GameObjectIDRequestEventArgs(
            [NotNull] IAppContext appContext,
            [NotNull] IGameContext gameContext)
            : base(appContext, gameContext)
        {
            WaitHandle = new ManualResetEvent(false);
        }

        public ManualResetEvent WaitHandle { get; private set; }
        public int? Value { get; set; }
    }

    public class GameContextEventArgs : ClientEventArgs
    {
        public GameContextEventArgs(
            [NotNull] IAppContext appContext,
            [NotNull] IGameContext gameContext)
            : base(appContext)
        {
            GameContext = gameContext ?? throw new ArgumentNullException("gameContext");
        }

        public IGameContext GameContext { get; }
    }

    public class GameContextEventArgs<TData> : ClientDataEventArgs<TData>
    {
        public GameContextEventArgs(
            [NotNull] IAppContext appContext,
            [NotNull] IGameContext gameContext,
            TData value)
            : base(appContext, value)
        {
            GameContext = gameContext ?? throw new ArgumentNullException("gameContext");
        }

        public IGameContext GameContext { get; }
    }

    public sealed class ServerInitializationFailedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ClientInitializationFailedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ClientConnectedEvent : CompositePresentationEvent<ClientConnectedEventArgs> { }
    public sealed class ClientConnectionFailedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ClientConnectionBrokenEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ClientDisconnectedEvent : CompositePresentationEvent<ClientDataEventArgs<ClientDisconnectReason>> { }
    public sealed class CombatUpdateReceivedEvent : CompositePresentationEvent<ClientDataEventArgs<CombatUpdate>> { }
    //public sealed class IntelUpdateReceivedEvent : CompositePresentationEvent<ClientDataEventArgs<IntelUpdate>> { }
    public sealed class InvasionUpdateReceivedEvent : CompositePresentationEvent<ClientDataEventArgs<InvasionArena>> { }
    public sealed class LocalPlayerJoinedEvent : CompositePresentationEvent<LocalPlayerJoinedEventArgs> { }
    public sealed class PlayerJoinedEvent : CompositePresentationEvent<ClientDataEventArgs<IPlayer>> { }
    public sealed class PlayerExitedEvent : CompositePresentationEvent<ClientDataEventArgs<IPlayer>> { }
    public sealed class LobbyUpdatedEvent : CompositePresentationEvent<ClientDataEventArgs<ILobbyData>> { }
    public sealed class GameStartingEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class GameStartedEvent : CompositePresentationEvent<ClientDataEventArgs<GameStartData>> { }
    public sealed class GameEndedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class GameEndingEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class TurnStartedEvent : CompositePresentationEvent<GameContextEventArgs> { }
    public sealed class TurnEndedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class AllTurnEndedEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ServerHeartbeatEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class TurnPhaseChangedEvent : CompositePresentationEvent<ClientDataEventArgs<TurnPhase>> { }
    public sealed class GameUpdateDataReceivedEvent : CompositePresentationEvent<ClientDataEventArgs<GameUpdateData>> { }
    public sealed class ChatMessageSentEvent : CompositePresentationEvent<ClientDataEventArgs<ChatMessage>> { }
    public sealed class ChatMessageReceivedEvent : CompositePresentationEvent<ClientDataEventArgs<ChatMessage>> { }
    public sealed class ScreenRefreshRequiredEvent : CompositePresentationEvent<ClientEventArgs> { }
    public sealed class ViewActivatingEvent : CompositePresentationEvent<ViewActivatingEventArgs> { }
    public sealed class GameObjectIDRequestedEvent : CompositePresentationEvent<GameObjectIDRequestEventArgs> { }
    public sealed class ScreenActivatedEvent : CompositePresentationEvent<ScreenActivatedEventArgs> { }
    public sealed class PlayerTurnFinishedEvent : CompositePresentationEvent<ClientDataEventArgs<IPlayer>> { }

    public static class ClientEvents
    {
        public static readonly ServerInitializationFailedEvent ServerInitializationFailed;
        public static readonly ClientInitializationFailedEvent ClientInitializationFailed;
        public static readonly ClientConnectedEvent ClientConnected;
        public static readonly ClientConnectionFailedEvent ClientConnectionFailed;
        public static readonly ClientConnectionBrokenEvent ClientConnectionBroken;
        public static readonly ClientDisconnectedEvent ClientDisconnected;
        public static readonly CombatUpdateReceivedEvent CombatUpdateReceived;
        //public static readonly IntelUpdateReceivedEvent IntelUpdateReceived;
        public static readonly InvasionUpdateReceivedEvent InvasionUpdateReceived;
        public static readonly LocalPlayerJoinedEvent LocalPlayerJoined;
        public static readonly PlayerJoinedEvent PlayerJoined;
        public static readonly PlayerExitedEvent PlayerExited;
        public static readonly LobbyUpdatedEvent LobbyUpdated;
        public static readonly GameStartingEvent GameStarting;
        public static readonly GameStartedEvent GameStarted;
        public static readonly GameEndedEvent GameEnded;
        public static readonly GameEndingEvent GameEnding;
        public static readonly TurnStartedEvent TurnStarted;
        public static readonly TurnEndedEvent TurnEnded;
        public static readonly AllTurnEndedEvent AllTurnEnded;
        public static readonly ServerHeartbeatEvent ServerHeartbeat;
        public static readonly TurnPhaseChangedEvent TurnPhaseChanged;
        public static readonly GameUpdateDataReceivedEvent GameUpdateDataReceived;
        public static readonly ChatMessageSentEvent ChatMessageSent;
        public static readonly ChatMessageReceivedEvent ChatMessageReceived;
        public static readonly ScreenRefreshRequiredEvent ScreenRefreshRequired;
        public static readonly ViewActivatingEvent ViewActivating;
        public static readonly GameObjectIDRequestedEvent GameObjectIDRequested;
        public static readonly ScreenActivatedEvent ScreenActivated;
        public static readonly PlayerTurnFinishedEvent PlayerTurnFinished;

        static ClientEvents()
        {
            IEventAggregator eventAggregator = Designer.IsInDesignMode ? new EventAggregator() : ServiceLocator.Current.GetInstance<IEventAggregator>();

            ServerInitializationFailed = eventAggregator.GetEvent<ServerInitializationFailedEvent>();
            ClientInitializationFailed = eventAggregator.GetEvent<ClientInitializationFailedEvent>();
            ClientConnected = eventAggregator.GetEvent<ClientConnectedEvent>();
            ClientConnectionFailed = eventAggregator.GetEvent<ClientConnectionFailedEvent>();
            ClientConnectionBroken = eventAggregator.GetEvent<ClientConnectionBrokenEvent>();
            ClientDisconnected = eventAggregator.GetEvent<ClientDisconnectedEvent>();
            CombatUpdateReceived = eventAggregator.GetEvent<CombatUpdateReceivedEvent>();
            // IntelUpdateReceived = eventAggregator.GetEvent<IntelUpdateReceivedEvent>();
            InvasionUpdateReceived = eventAggregator.GetEvent<InvasionUpdateReceivedEvent>();
            LocalPlayerJoined = eventAggregator.GetEvent<LocalPlayerJoinedEvent>();
            PlayerJoined = eventAggregator.GetEvent<PlayerJoinedEvent>();
            PlayerExited = eventAggregator.GetEvent<PlayerExitedEvent>();
            LobbyUpdated = eventAggregator.GetEvent<LobbyUpdatedEvent>();
            GameStarting = eventAggregator.GetEvent<GameStartingEvent>();
            GameStarted = eventAggregator.GetEvent<GameStartedEvent>();
            GameEnded = eventAggregator.GetEvent<GameEndedEvent>();
            GameEnding = eventAggregator.GetEvent<GameEndingEvent>();
            TurnStarted = eventAggregator.GetEvent<TurnStartedEvent>();
            TurnEnded = eventAggregator.GetEvent<TurnEndedEvent>();
            AllTurnEnded = eventAggregator.GetEvent<AllTurnEndedEvent>();
            ServerHeartbeat = eventAggregator.GetEvent<ServerHeartbeatEvent>();
            TurnPhaseChanged = eventAggregator.GetEvent<TurnPhaseChangedEvent>();
            GameUpdateDataReceived = eventAggregator.GetEvent<GameUpdateDataReceivedEvent>();
            ChatMessageSent = eventAggregator.GetEvent<ChatMessageSentEvent>();
            ChatMessageReceived = eventAggregator.GetEvent<ChatMessageReceivedEvent>();
            ScreenRefreshRequired = eventAggregator.GetEvent<ScreenRefreshRequiredEvent>();
            ViewActivating = eventAggregator.GetEvent<ViewActivatingEvent>();
            GameObjectIDRequested = eventAggregator.GetEvent<GameObjectIDRequestedEvent>();
            ScreenActivated = eventAggregator.GetEvent<ScreenActivatedEvent>();
            PlayerTurnFinished = eventAggregator.GetEvent<PlayerTurnFinishedEvent>();
        }
    }
}