// Enums.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
namespace Supremacy.Game
{
    /// <summary>
    /// Defines the possible results of an attempt to host a game
    /// </summary>
    public enum HostGameResult : sbyte
    {
        Success = 0,
        LoadGameFailure,
        UnknownFailure,
        ServiceAlreadyRunning,
        ChannelFaultFailure
    }

    /// <summary>
    /// Defines the possible results of an attempt to join a game
    /// </summary>
    public enum JoinGameResult : sbyte
    {
        Success = 0,
        ConnectionFailure,
        GameIsFull,
        GameAlreadyStarted,
        VersionMismatch
    }

    /// <summary>
    /// Defines the different types of war plans used in the game.
    /// </summary>
    public enum WarPlan : byte
    {
        NoWar = 0,
        AttackedRecent,
        Attacked,
        PreparingLimitedWar,
        PreparingTotalWar,
        LimitedWar,
        TotalWar,
        Dogpile
    }

    /// <summary>
    /// Defines the different types of area AIs used in the game.
    /// </summary>
    public enum AreaAIType : byte
    {
        Neutral = 0,
        Offensive,
        Defensive,
        Massing,
        Assault
    }

    /// <summary>
    /// Defines the types of messages exchanged during the game.
    /// </summary>
    public enum MessageType : byte
    {
        NoMessage = 0,
        Information,
        DisplayOnly,
        MajorEvent,
        MinorEvent,
        Chat,
        CombatMessage
    }

    /// <summary>
    /// Defines the client connection states used by the game server.
    /// </summary>
    public enum ClientConnectionState : byte
    {
        Inactive = 0,
        Connected,
        SentReady,
        Ready,
        AssignedID,
        SentID,
        Peer,
        FileTransfer,
        TransferComplete,
        Authorized,
        MapConfirmed,
        GameStarted
    }

    /// <summary>
    /// Defines the probability levels used in the game.
    /// </summary>
    public enum ProbabilityLevel : sbyte
    {
        NoProbability = -1,
        Low = 0,
        Normal,
        High
    }

    /// <summary>
    /// Defines the different unit AI types used in the game.
    /// </summary>
    public enum UnitAIType : byte
    {
        NoUnitAI = 0,   // unassigned
        Explorer,
        Colonizer,
        Constructor,
        Attack,
        Defense,
        Escort,
        Raider,
        Reserve,
        Counter,
        SystemAttack,
        SystemDefense,
        SystemCounter,
        Special         // special-purpose, never overridden
    }

    /// <summary>
    /// Defines the different activities of game units.
    /// </summary>
    public enum UnitActivity : byte
    {
        NoActivity = 0,
        UnMothball,
        Hold,
        Mothball,
        Repair,
        Patrol,
        Intercept,
        Mission
    }

    /// <summary>
    /// Defines the different types of games that can be started.
    /// </summary>
    public enum GameType : byte
    {
        SinglePlayerNew = 0,
        SinglePlayerLoad,
        MultiplayerNew,
        MultiplayerLoad
    }

    /// <summary>
    /// Defines the different types of claims on multiplayer game slots.
    /// </summary>
    public enum SlotClaim : byte
    {
        Unassigned = 0,
        Assigned,
        //Reserved
    }

    /// <summary>
    /// Defines the different states of multiplayer game slots.
    /// </summary>
    public enum SlotStatus : byte
    {
        Open = 0,
        Computer,
        Closed,
        Taken
    }

    /// <summary>
    /// Defines the AI strategies used in the game.
    /// </summary>
    [Flags]
    public enum AIStrategies : ushort
    {
        Default = (1 << 0),
        Dagger = (1 << 1),
        Sledgehammer = (1 << 2),
        Castle = (1 << 3),
        FastMovers = (1 << 4),
        SlowMovers = (1 << 5),
        Crush = (1 << 11),
        Production = (1 << 12),
        Peace = (1 << 13),
        GetBetterUnits = (1 << 14),
    }

    /// <summary>
    /// Defines the different colony roles used by the AI.
    /// </summary>
    [Flags]
    public enum ColonyRoles : ushort
    {
        None = (1 << 0),
        FocusOnProduction = (1 << 1),
        FocusOnMilitary = (1 << 2),
        FocusOnResearch = (1 << 3),
        FocusOnIntelligence = (1 << 4),
        FocusOnCredits = (1 << 5),
        Staging = (1 << 6),
        Linchpin = (1 << 7)
    }

    /// <summary>
    /// Defines the different path generation options used by the pathfinding engine.
    /// </summary>
    [Flags]
    public enum PathOptions : byte
    {
        IgnoreDanger = 0,
        SafeTerritory = (1 << 0),
        NoEnemyTerritory = (1 << 1),
        DeclareWar = (1 << 2),
        DirectAttack = (1 << 3)
    }

    /// <summary>
    /// Defines the different production focuses used by the AI.
    /// </summary>
    [Flags]
    public enum BuildingFocus : ushort
    {
        None = 0,
        Food = (1 << 1),
        Production = (1 << 2),
        Credits = (1 << 3),
        Resources = (1 << 4),
        Defense = (1 << 5),
        Morale = (1 << 6),
        Health = (1 << 7),
        Experience = (1 << 8),
        Maintenance = (1 << 9),
        Research = (1 << 10),
        Military = (1 << 11)
    }
}
