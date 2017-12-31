// AttitudeVariable.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Diplomacy
{
    public enum AttitudeVariable : byte
    {
        BaseAttitude,
        BasePeaceWeight,
        PeaceWeightRand,
        WarmongerRespect,
        RefuseTalkAfterWarThreshold,
        NoTechTradeThreshold,
        TechTradeKnownThreshold,
        MaxCreditsTradePercent,
        MaxCreditsPerTurnTradePercent,
        MaxWarRand,
        MaxWarNearbyPowerRatio,
        MaxWarDistantPowerRatio,
        MaxWarMinAdjacentLandPercent,
        LimitedWarRand,
        LimitedWarPowerRatio,
        DogpileWarRand,
        MakePeaceRand,
        DeclareWarTradeRand,
        WorseRankAttitudeChange,
        BetterRankAttitudeChange,
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
        ShareWarAttitudeDivisor,
        ShareWarAttitudeChangeLimit,
        DemandTributeAttitudeThreshold,
        NoGiveHelpAttitudeThreshold,
        TechRefuseAttitudeThreshold,
        BonusRefuseAttitudeThreshold,
        MapRefuseAttitudeThreshold,
        DeclareWarRefuseAttitudeThreshold,
        DeclareWarThemRefuseAttitudeThreshold,
        StopTradingRefuseAttitudeThreshold,
        StopTradingThemRefuseAttitudeThreshold,
        OpenBordersRefuseAttitudeThreshold,
        DefensivePactRefuseAttitudeThreshold,
        AllianceRefuseAttitudeThreshold,
        ProtectorateRefuseAttitudeThreshold,
        ProtectoratePowerModifier
    }
    /*
     * CALCULATING ATTITUDE
     * ====================
     * 
     *    = US   BaseAttitude
     * 
     *    + THEM   AttitudeChange
     * 
     * IF ((AggressiveAI is True) AND (THEM is Human))
     *    - 2
     * 
     * IF (THEM is Not Human)
     *    + (4 - abs(US Peace Weight - THEM Peace Weight))
     * 
     * IF (THEM is Not Human)
     *    + min(US WarmongerRespect, THEM WarmongerRespect)
     * 
     * IF (THEM is Higher Ranking)
     *    + ((THEM Rank - US Rank) * (US iWorseRankDifferenceAttitudeChange / (NumberOfPlayersEverAlive + 1)))
     * ELSE IF (THEM is Lower Ranking)
     *    + ((-(THEM Rank - US Rank)) * (US iBetterRankDifferenceAttitudeChange / (NumberOfPlayersEverAlive + 1)))
     * 
     * IF ((US rank >= NumberOfPlayersEverAlive / 2) AND (THEM rank >= NumberOfPlayersEverAlive / 2))
     *    + 1
     * 
     * IF (THEM WarSuccess > US WarSuccess)
     *    + LostWarAttitudeChange
     * 
     * IF (Borders Closed)
     *    + CloseBordersAttitudeChange
     * ELSE
     *    + OpenBordersAttitudeChange
     * 
     * IF (At War)
     *    + WarAttitudeChange
     * 
     * IF (At Peace)
     *    + PeaceAttitudeChange
     * 
     *  + BonusTradeAttitude
     * 
     *  + DefensivePactAttitude, RivalDefensivePactAttitude, ShareWarAttitude, TradeAttitude, RivalTradeAttitude, MemoryAttitude, AttitudeExtra
     */
}
