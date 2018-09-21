using Supremacy.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Economy
{
    public static class EconomyHelper
    {
        public static int ComputeResourceValue(ResourceType resourceType, int amount)
        {
            int baseValue;

            var table = GameContext.Current.Tables.ResourceTables["BaseCreditValues"];
            if (table == null || !int.TryParse(table[resourceType.ToString()][0], out baseValue))
                return amount;

            return baseValue * amount;
        }
    }
}
