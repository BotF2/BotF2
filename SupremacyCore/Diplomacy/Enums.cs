// Enums.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Xml.Serialization;

namespace Supremacy.Diplomacy
{

    public enum RegardValue
    { 
        TotalWar,
        ColdWar,
        Neutral,
        Friend,
        Allied,
        Unified 
    }

    public enum RegardLevel : byte
    {
        Detested,
        TotalWar,
        ColdWar,
        Cold,
        Neutral,
        Friend,
        Allied,
        Unified
    }

    public enum Tone : byte
    {
        Calm,
        Meek,
        Condescending,
        Indignant,
        Impatient,
        Annoyed,
        Enraged,
        Receptive,
        Enthusiastic
    }

    public enum StatementType : byte
    {
        NoStatement,
        CommendWar = 0,
        //CommendRelationship,
        //CommendAssault,
        //CommendInvasion,
        //CommendSabotage,
        DenounceWar,
        //DenounceRelationship,
        //DenounceAssault,
        //DenounceInvasion,
        //DenounceSabotage,
        //ThreatenDestroyColony,
        //ThreatenTradeEmbargo,
        //ThreatenDeclareWar,
        WarDeclaration,
        //SabotageOrder,
        StealCredits,
        StealResearch,
        SabotageFood,
        SabotageEnergy,
        SabotageIndustry,
        UpdateAcceptRejectDictionary
    }

    public enum ResponseType : byte
    {
        NoResponse,
        Accept,
        Reject,
        Counter
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd")]
    public enum ClauseType
    {
        NoClause,

        //OfferWithdrawTroops,
        //RequestWithdrawTroops,

        //OfferStopPiracy, // this could become OfferStopSpying
        //RequestStopPiracy,

        //OfferBreakAgreement,
        //RequestBreakAgreement,
        // proposal
        OfferGiveCredits,
        RequestGiveCredits,

        //OfferGiveResources,
        //RequestGiveResources,

        //OfferMapData,
        //RequestMapData,

        //OfferHonorMilitaryAgreement,
        //RequestHonorMilitaryAgreement,

        //OfferEndEmbargo,
        //RequestEndEmbargo,
        // proposals
        TreatyWarPact,
        TreatyCeaseFire,
        TreatyNonAggression,
        TreatyOpenBorders,
        //TreatyTradePact,
        //TreatyResearchPact,
        TreatyAffiliation,
        TreatyDefensiveAlliance,
        TreatyFullAlliance,
        TreatyMembership
    }

    public enum RegardEventType : byte
    {
        NoRegardEvent,
        LostBattle,
        AttackedCivilians,
        PeacetimeBorderIncursion,
        BorderIncursionPullout,
        InvaderMovement,
        UnprovokedAttack,
        ViolatedPeaceTreaty,
        ViolatedStopRaiding,
        ViolatedStopSpying,
        EnemySharesQuadrant,
        DeclaredWar,
        CapturedColony,
        TreatyCounter,
        TreatyReject,
        HealedPopulation,
        TreatyProposal,
        TreatyAccept, 
        DiplomaticShip,
        TraitsInCommon
    }

    [Flags]
    public enum RegardEventCategories : byte
    {
        None = 0x00,
        MilitaryPower = 0x01,
        MilitarySafety = 0x02,
        Diplomacy = 0x04,
        Economic = 0x08,
        Knowledge = 0x10,
        Production = 0x20,
        All = MilitaryPower | MilitarySafety | Diplomacy | Economic | Knowledge | Production
    }

    public enum MotivationType : byte
    {
        NoMotivation = 0,

        FearInvasion,
        FearColonyDefense,
        FearRaiding,
        FearTech,
        FearSpying,
        FearScienceRank,
        FearMilitaryRank,
        FearEconomyRank,

        DesireAttack,
        DesireAttackColony,
        DesireTrade,
        DesireGrowth,
        DesireCredits,
        DesireIntimidate,
        DesireMakeFriend,
        DesireEnlistFriend
    }

    /// <summary>
    /// Defines the types of denials to diplomatic offers used in the game.
    /// </summary>
    public enum DenialType : byte
    {
        NoDenial = 0,
        Unknown,
        Never,
        TooMuch,
        Mystery,
        Joking,
        AttitudeUs,
        AttitudeThem,
        TooPowerfulUs,
        TooPowerfulYou,
        TooPowerfulThem,
        TooManyWars,
        NoGain,
        NotAllied,
        NotTrustworthyThem,
        AtWarWithAllianceUs,
        AtWarWithAllianceThem,
        AlliedWithEnemyThem,
        WorstEnemy
    }

    /// <summary>
    /// Defines the types of diplomacy events used in the game.
    /// </summary>
    public enum DiplomacyEvent : byte
    {
        None = 0,
        AIContact,
        FailedContact,
        GiveHelp,
        RefuseHelp,
        AcceptDemand,
        RejectDemand,
        DemandWar,
        JoinWar,
        RefuseJoinWar,
        StopTrading,
        RefuseStopTrading,
        AskHelp,
        MakeDemand,
        ResearchTech,
        Target,
        MakeDemandProtectorate
    }

    /// <summary>
    /// Defines the types of memory events used in the game's diplomacy system.
    /// </summary>
    public enum MemoryType : byte
    {
        None = 0,
        DeclaredWarOnUs,
        DeclaredWarOnFriend,
        HiredWarAlly,
        AttackedUs,
        AttackedEnemy,
        AttackedFriend,
        AttackedNeutral,
        BombardedUs,
        BombardedFriend,
        BombardedNeutral,
        BombardedEnemy,
        HeavilyBombardedUs,
        HeavilyBombardedFriend,
        HeavilyBombardedNeutral,
        HeavilyBombardedEnemy,
        InvadedUs,
        InvadedFriend,
        SpiedOnUs,
        SabotagedUs,
        GaveHelp,
        RefusedHelp,
        AcceptedDemand,
        RejectedDemand,
        AcceptedTreaty,
        RejectedTreaty,
        AcceptedWarPact,
        RejectedWarPact,
        AcceptedStopTrading,
        RejectedStopTrading,
        StoppedTrading,
        HiredTradeEmbargo,
        MadeDemand,
        CancelledOpenBorders,
        TradedTechToUs,
        ReceivedTechFromAny,
        GaveGift
    }

    public enum AttitudeModifier : byte
    {
        BaseAttitude = 0,
        BasePeaceWeight,
        PeaceWeightRand,
        WarmongerRespect,
        RefuseToTalkWarThreshold,
        MaxCreditsTradePercent,
        MaxCreditsPerTurnTradePercent,
        MaxWarRand,
        MaxWarNearbyPowerRatio,
        MaxWarDistantPowerRatio,
        MaxWarMinAdjacentSpacePercent,
        LimitedWarRand,
        LimitedWarPowerRatio,
        DogpileWarRand,
        MakePeaceRand,
        DeclareWarTradeRand,
        DemandRebukedSneakProb,
        DemandRebukedWarProb,
        AssaultSystemProb,
        BuildUnitProb,
        BaseAttackOddsChange,
        AttackOddsChangeRand,
        WorseRankDifferenceAttitudeChange,
        BetterRankDifferenceAttitudeChange,
        CloseBordersAttitudeChange,
        LostWarAttitudeChange,
        AtWarAttitudeDivisor,
        AtWarAttitudeChangeLimit,
        AtPeaceAttitudeDivisor,
        AtPeaceAttitudeChangeLimit,
        BonusTradeAttitudeDivisor,
        BonusTradeAttitudeChangeLimit,
        OpenBordersAttitudeDivisor,
        OpenBordersAttitudeChangeLimit,
        DefensivePactAttitudeDivisor,
        DefensivePactAttitudeChangeLimit,
        ShareWarAttitudeChange,
        ShareWarAttitudeDivisor,
        ShareWarAttitudeChangeLimit,
        ProtectoratePowerModifier
    }

    public enum AttitudeThreshold : byte
    {
        DemandTributeAttitude,
        NoGiveHelpAttitude,
        TechRefuseAttitude,
        StrategicBonusRefuseAttitude,
        HappinessBonusRefuseAttitude,
        HealthBonusRefuseAttitude,
        MapRefuseAttitude,
        DeclareWarRefuseAttitude,
        DeclareWarThemRefuseAttitude,
        StopTradingRefuseAttitude,
        StopTradingThemRefuseAttitude,
        OpenBordersRefuseAttitude,
        DefensivepactRefuseAttitude,
        PermanentAllianceRefuseAttitude,
        ProtectorateRefuseAttitudeThreshold
    }
}
