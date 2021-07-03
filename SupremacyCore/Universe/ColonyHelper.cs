using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Utility;
using System.Linq;

namespace Supremacy.Universe
{
    public static class ColonyHelper
    {
        public static int ColonyValue(this Colony colony)
        {
            if (colony == null)
            {
                return 0;
            }

            int total = 0;

            /*
             * Include the value of all buildings.
             */
            foreach (Buildings.Building building in colony.Buildings)
            {
                total += building.Design.BuildCost;
                total += EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, building.Design.BuildResourceCosts[r]));
            }

            /*
             * Include the value of the shipyard, but not the ships under construction within (see below).
             */
            Orbitals.Shipyard shipyard = colony.Shipyard;
            int shipyardBuildSlotsCount = 0;

            if (shipyard != null)
            {
                total += shipyard.Design.BuildCost;
                total += EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, shipyard.Design.BuildResourceCosts[r]));
                shipyardBuildSlotsCount = shipyard.BuildSlots.Count;
            }

            IIndexedEnumerable<BuildSlot> buildSlots = colony.BuildSlots.Concat(shipyard != null ? shipyard.BuildSlots : IndexedEnumerable.Empty<BuildSlot>());

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
                    let resourceCosts = EnumHelper.GetValues<ResourceType>().Sum(r => EconomyHelper.ComputeResourceValue(r, facilityType.BuildResourceCosts[r]))
                    select (facilityType.BuildCost + resourceCosts) * facilityCount
                ).Sum();

            total += colony.Population.CurrentValue * 100;


            GameLog.Core.CivsAndRacesDetails.DebugFormat("Turn {0};{3};{4};Pop;{5};ShipYardSlots;{6};Buildings;ColonyValue={7};{1};{2}"
                , GameContext.Current.TurnNumber
                , colony.Owner
                , colony.Name
                , colony.Location
                , colony.Population.CurrentValue
                , shipyardBuildSlotsCount
                , colony.Buildings.Count
                , total
                );



            return total;
        }
    }
}
