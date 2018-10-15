using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Supremacy.Diplomacy;
using Supremacy.Tech;
using Supremacy.Utility;
using Supremacy.Collections;

namespace Supremacy.Combat
{
    public class HiddenCombatAssets
    {
        protected List<Tuple<CombatUnit>> engagingFleets;

        public List<CombatAssets> ExposeHiddenAssets(MapLocation location)
        {
            var assets = new Dictionary<Civilization, CombatAssets>();
            var exposedAssets = new List<CombatAssets>();
            var engagingleets = new List<Tuple<CombatUnit, MapLocation>>();
            if (engagingFleets != null)
            {
                for (int i = 0; i < engagingFleets.Count; i++)
                {
                    var oppositionShips = engagingFleets.Where(cs => CombatHelper.WillEngage(engagingFleets[i].Item1.Owner, cs.Item1.Owner));
                    var friendlyShips = engagingFleets.Where(cs => !CombatHelper.WillEngage(engagingFleets[i].Item1.Owner, cs.Item1.Owner));
                    var maxScanStrengthOpposition = engagingFleets.Max(s => s.Item1.ScanStrength);

                    if (oppositionShips.Count() > 0)
                    {
                        maxScanStrengthOpposition = oppositionShips.Max(s => s.Item1.ScanStrength);
                        //friendlyShips.Where(s => s.Item1.CloakStrength < maxScanStrengthOpposition).ForEach(s => s.Item1.Decloak());
                        friendlyShips.Where(s => s.Item1.CamouflagedStrength < maxScanStrengthOpposition).ForEach(s => s.Item1.Decamouflage());
                        { GameLog.Core.Combat.DebugFormat("{0} has CamouflageStrenght {1} vs MaxScan {2}", 
                            engagingFleets[i].Item1.Name, engagingFleets[i].Item1.CamouflagedStrength, maxScanStrengthOpposition); };
                    }

                }
            }
            return exposedAssets;
        }
    }
}
