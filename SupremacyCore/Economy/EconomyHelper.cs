using Supremacy.Game;

namespace Supremacy.Economy
{
    public static class EconomyHelper
    {
        /// <summary>
        /// Calculates the credit cost of the given amount of <see cref="ResourceType"/>
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static int ComputeResourceValue(ResourceType resourceType, int amount)
        {
            int baseValue;

            var table = GameContext.Current.Tables.ResourceTables["BaseCreditValues"];
            if (table == null || !int.TryParse(table[resourceType.ToString()][0], out baseValue))
                return amount;

            return baseValue * amount;
        }

        /// <summary>
        /// Calculates the credit cost of the given <see cref="ResourceValueCollection"/>
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static int ComputeResourceValue(ResourceValueCollection resources)
        {
            int runningTotal = 0;
            runningTotal += ComputeResourceValue(ResourceType.Deuterium, resources[ResourceType.Deuterium]);
            runningTotal += ComputeResourceValue(ResourceType.Dilithium, resources[ResourceType.Dilithium]);
            runningTotal += ComputeResourceValue(ResourceType.RawMaterials, resources[ResourceType.RawMaterials]);

            return runningTotal;
        }
    }
}
