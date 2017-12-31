// MoraleEvent.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Game
{
    /// <summary>
    /// Defines the various morale-affecting events used in the game.
    /// </summary>
    public enum MoraleEvent
    {
        NoMoraleEvent = 0,
        EliminateEmpire,
        WinMajorBattle,
        WinSignificantBattle,
        WinMinorBattle,
        LoseMajorBattle,
        LoseSignificantBattle,
        LoseMinorBattle,
        LoseFlagship,
        LoseHospitalShip,
        EliminateEnemyHospitalShip,
        EliminateEnemyFlagship,
        LoseStarbase,
        LoseSpacedock,
        SignMembershipTreaty,
        SubjugateSystem,
        ColonizeSystem,
        InfiltrateSystem,
        SabotageSystem,
        RaidSystem,
        LiberateNonNativeSystem,
        LiberateNativeSystem,
        OccupyNativeSystem,
        OccupyNonNativeSystem,
        SubjugateOccupiedSystem,
        GrantOccupiedSystemIndependence,
        AcceptDependencyRequest,
        AcceptProtectorateRequest,
        LoseHomeSystemToInvasion,
        LoseNonNativeSystemToInvasion,
        LoseSubjugatedSystemToInvasion,
        LoseSystemToRebellion,
        LoseNativeSystemToBribery,
        LoseNonNativeSystemToBribery,
        BombardSystemAsDefender,
        BombardSystemAsAggressor,
        BombardNativeRebelSystem,
        EliminateMinorRace,
        SufferBombardmentOfSystem,
        SufferHalfPopulationLossInHomeSystemBombardment,
        EvacuateSystem,
        RelocateEvacuatedCitizens,
        LoseEvacuationRefugeesInBattle,
        DeclareWarOnNeutralEmpire,
        DeclareWarOnPeacefulEmpire,
        DeclareWarOnFriendEmpire,
        DeclareWarOnAffiliateEmpire,
        DeclareWarOnAlly,
        WarDeclaredByNeutralEmpire,
        WarDeclaredByEmpireWithTreaty,
        WarDeclaredByAlly,
        AcceptSurrender,
        ReceiveSurrender,
        AcceptVictoryNonAggressionTreaty,
        ReceiveVictoryNonAggressionTreatyDemand,
        SignFriendshipOrAffiliationTreaty,
        SignAntiBorgAllianceTreaty,
        ReceiveAcceptanceOfWarPact,
        AcceptWarPact,
        ReceiveAcceptanceOfDefencePact,
        AcceptDefencePact,
        BreakTreaty,
        RefuseTreaty,
        AcceptDemand,
        ReceiveAcceptanceOfDemand,
        AcceptRequest,
        ReceiveAcceptanceOfRequest,
        SignAlliance,
        BreakAlliance,
        AcceptRequestForIndependence,
        RefuseRequestForIndependence,
        RepelBorgAttack,
        LosePopulationToNaturalEvent
    }
}
