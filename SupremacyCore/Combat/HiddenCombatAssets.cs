//using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Supremacy.Utility;
//using Supremacy.Collections;
using Obtics.Collections;

namespace Supremacy.Combat
{
    public class HiddenCombatAssets
    {
        /// <summary>
        /// finds a list of fleets at a map location <see cref="Fleet"/>
        /// and decamouflages ships with weak camouflage values
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>

        public List<Fleet> ExposeHiddenAssets(MapLocation location)
        {
            var engagingFleets = new List<Fleet>();
            var fleetsAtLocation = new List<Fleet>(); // realy need to get something like the next commented out line to collect a list of fleets at location
                //GameContext.Current.Universe.FindAt<Fleet>(location).ToList();
            
            if (fleetsAtLocation != null)
            {
                for (int i = 0; i < fleetsAtLocation.Count; i++)
                {
                    //var oppositionShips = fleetsAtLocation.Where((Tuple<CombatUnit> cs) => CombatHelper.WillEngage(fleetsAtLocation[i].Item1.Owner, cs.Item1.Owner));
                    //var friendlyShips = fleetsAtLocation.Where((Tuple<CombatUnit> cs) => !CombatHelper.WillEngage(fleetsAtLocation[i].Item1.Owner, cs.Item1.Owner));
                    //var maxScanStrengthOpposition = fleetsAtLocation.Max((Tuple<CombatUnit> s) => s.Item1.ScanStrength);

                    //if (oppositionShips.Count() > 0)
                    //{
                    //    maxScanStrengthOpposition = oppositionShips.Max((Tuple<CombatUnit> s) => s.Item1.ScanStrength);
                    //    //friendlyShips.Where(s => s.Item1.CloakStrength < maxScanStrengthOpposition).ForEach(s => s.Item1.Decloak());
                    //    friendlyShips.Where((Tuple<CombatUnit> s) => s.Item1.CamouflagedStrength < maxScanStrengthOpposition).ForEach((Tuple<CombatUnit> s) => s.Item1.Decamouflage());
                    //    {
                    //        GameLog.Core.Combat.DebugFormat("{0} has CamouflageStrenght {1} vs MaxScan {2}", 
                    //        fleetsAtLocation[i].Item1.Name, fleetsAtLocation[i].Item1.CamouflagedStrength, maxScanStrengthOpposition); };
                    //}

                }
                engagingFleets = fleetsAtLocation;
            }
            return engagingFleets;
        }
    }
}
