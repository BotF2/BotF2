using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Universe
{
    public static class ColonyHelper
    {
        public static int ColonyValue(this Colony colony)
        {
            if (colony == null)
                return 0;

            var total = 0;

            /*
             * Include the value of all buildings.
             */
            foreach (var building in colony.Buildings)
            {
                total += building.Design.BuildCost;
                total += EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, building.Design.BuildResourceCosts[r]));
            }

            /*
             * Include the value of the shipyard, but not the ships under construction within (see below).
             */
            var shipyard = colony.Shipyard;
            if (shipyard != null)
            {
                total += shipyard.Design.BuildCost;
                total += EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, shipyard.Design.BuildResourceCosts[r]));
            }

            var buildSlots = colony.BuildSlots.Concat(shipyard != null ? shipyard.BuildSlots : IndexedEnumerable.Empty<BuildSlot>());

            /*
             * Include the resources invested in partially completed construction.
             */
            total +=
                (
                    from slot in buildSlots
                    let project = slot.Project
                    where project != null &&
                          project.IsPartiallyComplete
                    let resourceValue = EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, project.ResourcesInvested[r]))
                    select project.IndustryInvested + resourceValue
                ).Sum();

            /*
             * Include all production facilities.
             */
            total +=
                (
                    from productionCategory in EnumHelper.GetValues<ProductionCategory>()
                    let facilityCount = colony.GetTotalFacilities(productionCategory)
                    where facilityCount > 0
                    let facilityType = colony.GetFacilityType(productionCategory)
                    where facilityType != null
                    let baseCost = facilityType.BuildCost
                    let resourceCosts = facilityType.BuildResourceCosts.Sum()
                    select (facilityType.BuildCost + resourceCosts) * facilityCount
                ).Sum();

            total += colony.Population.CurrentValue * 100;

            // ReSharper restore AccessToModifiedClosure

            return total;
        }
    }
}
