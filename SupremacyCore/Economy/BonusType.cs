// BonusType.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a bonus provided by a building.
    /// </summary>
    [Serializable]
    public struct Bonus
    {
        private BonusType _bonusType;
        private int _amount;

        /// <summary>
        /// Gets or sets the type of the bonus.
        /// </summary>
        /// <value>The type of the bonus.</value>
        public BonusType BonusType
        {
            get { return _bonusType; }
            set { _bonusType = value; }
        }

        /// <summary>
        /// Gets or sets the amount of the bonus (either an absolute amount or a whole
        /// number relative percentage).
        /// </summary>
        /// <value>The amount.</value>
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bonus"/> class.
        /// </summary>
        /// <param name="bonusType">Type of the bonus.</param>
        /// <param name="amount">The amount.</param>
        public Bonus(BonusType bonusType, int amount)
        {
            _bonusType = bonusType;
            _amount = amount;
        }
    }

    public static class BonusHelper
    {
        public static bool IsGlobalBonus(BonusType bonus)
        {
            switch (bonus)
            {
                case BonusType.ExternalAffairs:
                case BonusType.InternalAffairs:
                case BonusType.MoraleEmpireWide:
                case BonusType.PercentBioTechResearch:
                case BonusType.PercentBribeResistanceEmpireWide:
                case BonusType.PercentComputerResearch:
                case BonusType.PercentConstructionResearch:
                case BonusType.PercentEconomicSabotage:
                case BonusType.PercentEconomicSecurity:
                case BonusType.PercentEnergyResearch:
                case BonusType.PercentExternalAffairs:
                case BonusType.PercentGeneralIntelligence:
                case BonusType.PercentInternalAffairs:
                case BonusType.PercentMilitarySabotage:
                case BonusType.PercentPoliticalSabotage:
                case BonusType.PercentPropulsionResearch:
                case BonusType.PercentResearchEmpireWide:
                case BonusType.PercentSabotage:
                case BonusType.PercentShipExperience:
                case BonusType.PercentTotalIntelligence:
                case BonusType.PercentWeaponsResearch:
                    return true;
                default:
                    return false;

            }
        }
    }

    public enum BonusType : byte
    {
        Food = 0,
        PercentFood,
        Industry,
        PercentIndustry,
        Energy,
        PercentEnergy,
        Research,
        Credits,
        PercentCredits,
        Intelligence,
        PercentIntelligence,
        Morale,
        MoraleEmpireWide,
        TradeRoutes,
        PercentTotalCredits,
        PercentTradeIncome,
        GrowthRate,
        PercentGrowthRate,
        BribeResistance,
        PercentBribeResistanceEmpireWide,
        PercentPopulationHealth,
        InternalAffairs,
        PercentInternalAffairs,
        ExternalAffairs,
        PercentExternalAffairs,
        PercentShipExperience,
        AntiShipDefense,
        PercentAntiShipDefense,
        ShieldPerEnergyTech,
        PercentGroundCombat,
        PercentGroundDefense,
        PercentAntiCloak,
        PercentPlanetaryShielding,
        ScanRange,
        JammingRange,
        RawMaterials,
        PercentRawMaterials,
        Deuterium,
        PercentDeuterium,
        Dilithium,
        PercentShipBuilding,
        PercentScrapping,
        Raiding,
        PercentRaiding,
        Intercept,
        PercentIntercept,
        PercentTotalIntelligence,
        PercentInternalSecurity,
        PercentGeneralIntelligence,
        PercentEconomicSecurity,
        PercentSabotage,
        PercentEconomicSabotage,
        PercentMilitarySabotage,
        PercentPoliticalSabotage,
        PercentBioTechResearch,
        PercentComputerResearch,
        PercentConstructionResearch,
        PercentEnergyResearch,
        PercentPropulsionResearch,
        PercentWeaponsResearch,
        PercentResearchEmpireWide,
        MaxPopulationPerMoonSize,
        PlanetaryShielding
    }
}
