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
        private List<Fleet> _friendlyShips;
        private List<Fleet> _oppositionShips;       
        private List<CombatUnit> _unitResults;

        public List<Fleet> FriendlyShips
        {
            get
            {
                if (_friendlyShips == null)
                {
                    _friendlyShips = new List<Fleet>();
                }
                return _friendlyShips;
            }
            set
            {
                _friendlyShips = value;
            }
        }
        public List<Fleet> OppositionShips
        {
            get
            {
                if (_oppositionShips == null)
                {
                    _oppositionShips = new List<Fleet>();
                }
                return _oppositionShips;
            }
            set
            {
                _oppositionShips = value;
            }
        }
        public int MaxOppositionScanStrengh { get; set; }

        public List<CombatUnit> UnitResults
        {
            get
            {
                if (_unitResults == null)
                {
                    _unitResults = new List<CombatUnit>();
                }
                return _unitResults;
            }
            set
            {
                _unitResults = value;
            }
        }

        public TakeSidesAssets(MapLocation location)
        {
            var fleetsAtLocation = GameContext.Current.Universe.FindAt<Fleet>(location).ToList();

            //https://www.youtube.com/watch?v=FAj4uvuITe0
            if (fleetsAtLocation != null)
            {
                //FriendlyShips = new List<Fleet>();
                //OppositionShips = new List<Fleet>();
                //UnitResults = new List<CombatUnit>();

                //for (int i = 0; i < fleetsAtLocation.Count; i++)
                //    FriendlyShips = fleetsAtLocation.Where(cs => !CombatHelper.WillEngage(fleetsAtLocation[i].Owner, cs.Owner));
                // _combatShips.Where((Tuple<CombatUnit> s) => s.Item1.CamouflagedStrength < maxScanStrength).ForEach((Tuple<CombatUnit> s) => s.Item1.Decamouflage());
                //FriendlyShips.Where((Tuple<Fleet> s) => s.Item1.(!CombatHelper.WillEngage(fleetsAtLocation[i].Owner, cs.Owner)).ForEach((Tuple<CombatUnit> s) => s.Item1.Decamouflage());

                for (int i = 0; i < fleetsAtLocation.Count; i++)
                {
                    for (int j = 0; j < fleetsAtLocation.Count; j++)
                    {
                        if (CombatHelper.WillEngage(fleetsAtLocation[j].Owner, fleetsAtLocation[i].Owner))
                        {
                            OppositionShips.Add(fleetsAtLocation[j]);
                        }
                        if (!CombatHelper.WillEngage(fleetsAtLocation[j].Owner, fleetsAtLocation[i].Owner))
                        {
                            FriendlyShips.Add(fleetsAtLocation[j]);
                        }
                    }

                    if (OppositionShips.Count() > 0)
                    {
                        foreach (var fleet in OppositionShips)
                        {
                            if (fleet.ScanStrength > MaxOppositionScanStrengh)
                            {
                                MaxOppositionScanStrengh = fleet.ScanStrength;
                            }
                        }
                    }
                }
            }
            foreach (var fleet in FriendlyShips)
            {
                foreach (var ship in fleet.Ships)
                    {
                        UnitResults.Add(new CombatUnit(ship));
                    }
            }
        }
    }
}
