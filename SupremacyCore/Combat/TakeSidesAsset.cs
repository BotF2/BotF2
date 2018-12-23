//using Supremacy.Entities;
//using Obtics.Collections;

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
        private List<Fleet> _oppositionFleets;

        public List<Fleet> OppositionFleets
        {
            get
            {
                if (_oppositionFleets == null)
                {
                    _oppositionFleets = new List<Fleet>();
                }
                return _oppositionFleets;
            }
            set
            {
                _oppositionFleets = value;
            }
        }
        public int MaxOppositionScanStrengh { get; set; }

        public TakeSidesAssets(MapLocation location)
        {
            var fleetsAtLocation = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();

            if (fleetsAtLocation != null)
            {

                for (int i = 0; i < fleetsAtLocation.Count; i++)
                {
                    for (int j = 0; j < fleetsAtLocation.Count; j++)
                    {
                        if (CombatHelper.WillEngage(fleetsAtLocation[j].Owner, fleetsAtLocation[i].Owner))
                        {
                            OppositionFleets.Add(fleetsAtLocation[j]);
                            OppositionFleets.Distinct();
                        }

                        MaxOppositionScanStrengh = 0;
                        if (OppositionFleets.Count() > 0)
                        {
                            foreach (var fleet in OppositionFleets)
                            {
                                if (fleet.ScanStrength > MaxOppositionScanStrengh)
                                {
                                    MaxOppositionScanStrengh = fleet.ScanStrength;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
