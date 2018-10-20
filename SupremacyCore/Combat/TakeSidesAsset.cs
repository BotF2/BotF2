//using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Combat
{
    public class TakeSidesAssets
    {
        public List<Fleet> FriendlyShips { get; set; }
        public List<Fleet> OppositionShips { get; set; }
        public int MaxOppositionScanStrengh { get; set; }
        public List<CombatUnit> UnitResults { get; set; }

        public TakeSidesAssets(MapLocation location)
        {
            var fleetsAtLocation = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();

            if (fleetsAtLocation != null)
            {
                for (int i = 0; i < fleetsAtLocation.Count; i++)
                {
                    for (int j = 0; j < fleetsAtLocation.Count; j++)
                    {
                        if (CombatHelper.WillEngage(fleetsAtLocation[i].Owner, fleetsAtLocation[j].Owner))
                        {
                            OppositionShips.Add(fleetsAtLocation[j]);
                        }
                        if (!CombatHelper.WillEngage(fleetsAtLocation[i].Owner, fleetsAtLocation[j].Owner))
                        {
                            FriendlyShips.Add(fleetsAtLocation[j]);
                        }
                    }

                    if (OppositionShips.Count() > 0)
                    {
                        foreach (var ship in OppositionShips)
                        {
                            if (ship.ScanStrength > MaxOppositionScanStrengh)
                            {
                                MaxOppositionScanStrengh = ship.ScanStrength;
                            }
                        }
                    }
                }
            }
            foreach (var fleet in fleetsAtLocation)
            {
                foreach (var ship in fleet.Ships)
                {
                    UnitResults.Add(new CombatUnit(ship));
                }
            }
        }
    }
}
