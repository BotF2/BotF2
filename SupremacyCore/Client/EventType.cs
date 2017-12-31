// EventType.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Client
{
    public enum EventType : byte
    {
        None = 0,
        FirstContact,
        CombatVictory,
        CombatDefeat,
        SystemUnderAttack,
        SystemLost,
        SystemColonized,
        SystemInfiltrated,
        SystemRaided,
        TaskForceDestroyed,
        DiplomaticMessageReceived,
        IntelligenceReportReceived,
        ResearchCompleted,
        NewTechnolgiesAvailable
    }
}
