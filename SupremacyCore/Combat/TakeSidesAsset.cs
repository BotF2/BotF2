//using Supremacy.Entities;
//using Obtics.Collections;

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Combat
{
    public class TakeSidesAssets
    {
        private List<Fleet> _oppositionFleets; // opposition is now others

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
            set => _oppositionFleets = value;
        }
        public int MaxOppositionScanStrengh { get; set; }

        public TakeSidesAssets(MapLocation location)
        {
            List<Fleet> fleetsAtLocation = GameContext.Current.Universe.FindAt<Fleet>(location).ToList(); // ToDo - part of altering collection while using, CombatHelper.cs line 58 

            if (fleetsAtLocation != null)
            {
                for (int i = 0; i < fleetsAtLocation.Count; i++)
                {
                    for (int j = 0; j < fleetsAtLocation.Count; j++)
                    {
                        if (CombatHelper.WillEngage(fleetsAtLocation[j].Owner, fleetsAtLocation[i].Owner)) // ToDo 3+ directional scan
                        {
                            if (OppositionFleets.Contains(fleetsAtLocation[j]))
                                continue;
                            else
                            OppositionFleets.Add(fleetsAtLocation[j]);
                            //_ = OppositionFleets.Distinct();
                        }

                        //if (OppositionFleets.Count() > 0)
                        //    GameLog.Core.Combat.DebugFormat("OppositionFleets.Count() = {0} ", OppositionFleets.Count());

                        MaxOppositionScanStrengh = 0;

                        if (OppositionFleets.Count > 0)
                        {
                            foreach (Fleet fleet in OppositionFleets)
                            {
                                //GameLog.Core.Combat.DebugFormat("{0} {1} ScanStrength = {2}, MaxOppositionScanStrengh = {3}", fleet.ObjectID, fleet.Name, fleet.ScanStrength, MaxOppositionScanStrengh);
                                if (fleet.ScanStrength > MaxOppositionScanStrengh)
                                {
                                    MaxOppositionScanStrengh = fleet.ScanStrength;
                                    //GameLog.Core.Combat.DebugFormat("{0} {1} ScanStrength = {2}, MaxOppositionScanStrengh grows to = {3}", 
                                    //    fleet.ObjectID, fleet.Name, fleet.ScanStrength, MaxOppositionScanStrengh);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
