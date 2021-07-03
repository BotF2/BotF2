// ColonyBuilder.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Economy;
using Supremacy.Tech;

namespace Supremacy.Universe
{
    /// <summary>
    /// Helper class to aid in the construction of new colonies.
    /// </summary>
    public static class ColonyBuilder
    {
        /// <summary>
        /// Create the initial infrastructure at the specified colony.
        /// </summary>
        /// <param name="colony">A new colony.</param>
        public static void Build(Colony colony)
        {
            int pop = colony.Population.CurrentValue;
            float growth = colony.System.GetGrowthRate(colony.Inhabitants);

            int foodNeeded = (int)(pop * (1 + (3 * growth)));
            ProductionFacilityDesign foodFacility = TechTreeHelper.GetBestFacilityDesign(colony, ProductionCategory.Food);
            if (foodFacility != null)
            {
                int facilityCount = Math.Max(2, foodNeeded / (foodFacility.UnitOutput + 1) + 1);
                colony.SetFacilityType(ProductionCategory.Food, foodFacility);
                for (int i = 0; i < facilityCount; i++)
                {
                    colony.AddFacility(ProductionCategory.Food);
                    _ = colony.ActivateFacility(ProductionCategory.Food);
                }
            }

            ProductionFacilityDesign industryFacility = TechTreeHelper.GetBestFacilityDesign(colony, ProductionCategory.Industry);
            if (industryFacility != null)
            {
                int facilityCount = pop / industryFacility.LaborCost;
                colony.SetFacilityType(ProductionCategory.Industry, industryFacility);
                for (int i = 0; i < facilityCount; i++)
                {
                    colony.AddFacility(ProductionCategory.Industry);
                    _ = colony.ActivateFacility(ProductionCategory.Industry);
                }
            }

            ProductionFacilityDesign energyFacility = TechTreeHelper.GetBestFacilityDesign(colony, ProductionCategory.Energy);
            if (energyFacility != null)
            {
                int facilityCount = pop / energyFacility.LaborCost / 2;
                colony.SetFacilityType(ProductionCategory.Energy, energyFacility);
                for (int i = 0; i < facilityCount; i++)
                {
                    colony.AddFacility(ProductionCategory.Energy);
                    _ = colony.ActivateFacility(ProductionCategory.Energy);
                }
            }

            ProductionFacilityDesign researchFacility = TechTreeHelper.GetBestFacilityDesign(colony, ProductionCategory.Research);
            if (researchFacility != null)
            {
                int facilityCount = pop / researchFacility.LaborCost / 2;
                colony.SetFacilityType(ProductionCategory.Research, researchFacility);
                for (int i = 0; i < facilityCount; i++)
                {
                    colony.AddFacility(ProductionCategory.Research);
                    _ = colony.ActivateFacility(ProductionCategory.Research);
                }
            }

            ProductionFacilityDesign intelligenceFacility = TechTreeHelper.GetBestFacilityDesign(colony, ProductionCategory.Intelligence);
            if (intelligenceFacility != null)
            {
                int facilityCount = pop / (intelligenceFacility.LaborCost * 10);
                colony.SetFacilityType(ProductionCategory.Intelligence, intelligenceFacility);
                for (int i = 0; i < facilityCount; i++)
                {
                    colony.AddFacility(ProductionCategory.Intelligence);
                    _ = colony.ActivateFacility(ProductionCategory.Intelligence);
                }
            }
        }
    }
}
