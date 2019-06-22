// AutomatedCombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Supremacy.Combat
{
    public sealed class AutomatedCombatEngine : CombatEngine
    {
        private double cycleReduction = 1d;
        private double newCycleReduction = 1d;
        private double excessShipsStartingAt;
        private double shipRatio = 1;
        private bool friendlyOwner = true;
        // private object firstOwner;
        // private int friendlyWeaponPower = 0;
        private int weakerSide = 0; // 0= no bigger ships counts, 1= First Friendly side bigger, 2= Oppostion side bigger
        private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _targetDictionary;
        private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _shipListDictionary;
        private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _willFightAlongSide;
        private List<Tuple<CombatUnit, CombatWeapon[]>> _friendlyCombatShips;
        private List<Tuple<CombatUnit, CombatWeapon[]>> _oppositionCombatShips;
        private List<Tuple<CombatUnit, CombatWeapon[]>> _defaultCombatShips;

        public List<Tuple<CombatUnit, CombatWeapon[]>> FriendlyCombatShips
        {
            get
            {
                return this._friendlyCombatShips;
            }
            set
            {
                this._friendlyCombatShips = value;
            }
        }
        public List<Tuple<CombatUnit, CombatWeapon[]>> OppositionCombatShips
        {
            get
            {
                return this._oppositionCombatShips;
            }
            set
            {
                this._oppositionCombatShips = value;
            }
        }

        public Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> TargetDictionary
        {
            get
            {
                return _targetDictionary;
            }
            set
            {
                this._targetDictionary = value;
            }
        }

        public Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> ShipListtDictionary // do we need this? just look in _combatShips by ownerID?
        {
            get
            {
                return _shipListDictionary;
            }
            set
            {
                this._shipListDictionary = value;
            }
        }
        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {
            _targetDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            _shipListDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            _willFightAlongSide = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            _friendlyCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _oppositionCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _defaultCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
        }

        protected override void ResolveCombatRoundCore()
        {
            // Setting variables to standard (initilization) of these fields
            shipRatio = 1;
            excessShipsStartingAt = 0;
            weakerSide = 0;
            cycleReduction = 1;
            newCycleReduction = 1;

            int maxScanStrengthOpposition = 0;

            GameLog.Core.CombatDetails.DebugFormat("_combatShips.Count: {0}", _combatShips.Count());

            // Scouts, Frigate and cloaked ships have a special chance of retreating BEFORE round 3
            if (_roundNumber < 3)
            {
                //  Once a ship has retreated, its important that it does not do it again..
                var easyRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked == true || (s.Item1.Source.OrbitalDesign.ShipType == "Frigate") || (s.Item1.Source.OrbitalDesign.ShipType == "Scout"))
                    .Where(s => !s.Item1.IsDestroyed) //  Destroyed ships cannot retreat
                    .Where(s => GetCombatOrder(s.Item1.Source) == CombatOrder.Retreat)
                    .ToList();

                foreach (var ship in easyRetreatShips)
                {
                    if (!RandomHelper.Chance(10) && (ship.Item1 != null))
                    {
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1)) // escaped ships cannot escape again
                        {
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                        }
                    }
                }

                //Decloak any cloaked ships 
                foreach (var combatShip in _combatShips)
                {
                    if (combatShip.Item1.IsCloaked)
                    {
                        combatShip.Item1.Decloak();
                        GameLog.Core.Combat.DebugFormat("Ship  {0} {1} ({2}) cloak status {3})",
                            combatShip.Item1.Source.ObjectID, combatShip.Item1.Name, combatShip.Item1.Source.Design, combatShip.Item1.IsCloaked);
                    }
                }
            }

            _combatShipsTemp = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _combatShipsTemp.Clear();

            TargetDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>(); // now a dictoinary and not a list
            TargetDictionary.Clear();

            ShipListtDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            ShipListtDictionary.Clear();

            // get owner ids for the ships in this sector (ownerIDs)
            List<int> ownerIDs = new List<int>();
            foreach (var tupleShip in _combatShips)
            {         
                ownerIDs.Add(tupleShip.Item1.OwnerID);
                _targetDictionary[tupleShip.Item1.OwnerID] = _defaultCombatShips;
            }
            ownerIDs.Distinct().ToList();

            //var CivOne = ownerIDs.First();
            //GameLog.Core.CombatDetails.DebugFormat("CivOne = {0}", CivOne);

            // populate dictionary of ships in a lists for each owner, The key is owner id (_shipListDictionary)
            foreach (var ownerID in ownerIDs)
            {
                var listOfShipsByOwnerID = _combatShips.Where(sc => sc.Item1.OwnerID == ownerID).Select(sc => sc).ToList();
                _shipListDictionary[ownerID] = listOfShipsByOwnerID;
            }

            // populate dictionary of will fight alongside ships in a list for each owner
            for (int t = 0; t < _combatShips.Count(); t++)
            {
                //var ownerAssets = GetAssets(_combatShips[t].Item1.Owner);
                _willFightAlongSide[_combatShips[t].Item1.OwnerID] = _combatShips.Where(cs => CombatHelper.WillFightAlongside(_combatShips[t].Item1.Owner, cs.Item1.Owner))
                    .Select(cs => cs)
                    .ToList();
                _willFightAlongSide.Distinct().ToList();
            }

            List<int> _unitTupleIDList = new List<int>();
            List<int> _attackerIDList = new List<int>();
            List<Tuple<CombatUnit, CombatWeapon[]>> targetUnitTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>(); // list of target tuples
            List<Tuple<CombatUnit, CombatWeapon[]>> returnFireTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>();

            #region populate target dictionaries
            // populate target dictionary with lists of target units (_oppositionCombatShips), key is owner id / civ id
            foreach (var unitTuple in _combatShips)
            {
                //bool foundBorgShip = (_combatShips.Where(sc => sc.Item1.Owner.ShortName == "Borg").Select(sc => sc).ToList().Any()); // any borg here?

                try
                {
                    if (!_unitTupleIDList.Contains(unitTuple.Item1.OwnerID)) // only pass in each civ once
                    {
                        GameLog.Core.Test.DebugFormat("--------------------------");
                        GameLog.Core.Test.DebugFormat("Top of loop unitTuple {0} {1}", unitTuple.Item1.Owner, unitTuple.Item1.Name);

                        foreach (var attackingTuple in _combatShips)
                        {
                            if (attackingTuple.Item1.OwnerID == unitTuple.Item1.OwnerID)
                                continue;
                            GameLog.Core.Test.DebugFormat("Top of loop attackingTuple = {0} {1}", attackingTuple.Item1.Source.ObjectID, attackingTuple.Item1.Name);
                            if (attackingTuple.Item1.OwnerID != unitTuple.Item1.OwnerID && !_attackerIDList.Contains(attackingTuple.Item1.OwnerID))  // don't check your own ships & only pass in each civ as attacker once
                            {
                                Civilization attackerTargetOne = new Civilization();
                                Civilization attackerTargetTwo = new Civilization();
                                Civilization unitTupleTargetOne = new Civilization();
                                Civilization unitTupleTargetTwo = new Civilization();

                                try
                                {
                                    attackerTargetOne = GetTargetOne(attackingTuple.Item1.Source);
                                }
                                catch
                                {
                                    attackerTargetOne = CombatHelper.GetDefaultHoldFireCiv();
                                }
                                try
                                {
                                    attackerTargetTwo = GetTargetTwo(attackingTuple.Item1.Source);

                                }
                                catch
                                {
                                    attackerTargetTwo = CombatHelper.GetDefaultHoldFireCiv();
                                }
                                try
                                {
                                    unitTupleTargetOne = GetTargetOne(unitTuple.Item1.Source);
                                }
                                catch
                                {
                                    unitTupleTargetOne = CombatHelper.GetDefaultHoldFireCiv();
                                }
                                try
                                {
                                    unitTupleTargetTwo = GetTargetTwo(unitTuple.Item1.Source);
                                }
                                catch
                                {
                                    unitTupleTargetTwo = CombatHelper.GetDefaultHoldFireCiv();
                                }

                                GameLog.Core.Test.DebugFormat("Attacker {0} with Target1 ={1} & 2={2}",
                                    attackingTuple.Item1.Source.Name, attackerTargetOne.ShortName, attackerTargetTwo.ShortName);
                                GameLog.Core.Test.DebugFormat("unitTuple {0} with Target1 ={1} & 2={2}",
                                    unitTuple.Item1.Source.Name, unitTupleTargetOne.ShortName, unitTupleTargetTwo.ShortName);

                                //GameLog.Core.Test.DebugFormat("found borg ship? {0}", foundBorgShip);

                                // if both sides are default targeting holdFireCiv then look for other targets in the sector anyway
                                //ToDo: AI put in check if they are already being targeted by a third party? (Why look for more) & put in a check of civ behavior like war-like or peaceful?
                                if (unitTupleTargetOne.ShortName == "DefaultHoldFireCiv" && attackerTargetOne.ShortName == "DefaultHoldFireCiv") //attackerTargetOne.ShortName == "Borg" && unitTupleTargetOne.ShortName == "Borg" && !foundBorgShip                 
                                {
                                    if (!CombatHelper.WillFightAlongside(_combatShips.Last().Item1.Owner, attackingTuple.Item1.Owner) && attackingTuple.Item1.OwnerID != unitTuple.Item1.OwnerID)
                                    {
                                        GameLog.Core.Test.DebugFormat("Two defaulted Hold fire Players trying to add targets {0} or {1}", _combatShips.Last().Item1.Owner.ShortName, _combatShips.First().Item1.Owner.ShortName);
                                        if (attackingTuple.Item1.OwnerID != _combatShips.Last().Item1.OwnerID)
                                            attackerTargetOne = _combatShips.Last().Item1.Owner; // desperation target when no Borg and last ship is not your ally
                                        else if (attackingTuple.Item1.OwnerID != _combatShips.First().Item1.OwnerID)
                                            attackerTargetOne = _combatShips.First().Item1.Owner;
                                    }
                                }

                                GameLog.Core.Test.DebugFormat("if attacker only returns fire 1 & 2 true={0} should see breaking loop next", (attackerTargetOne.ShortName == "Only Return Fire" && attackerTargetTwo.ShortName == "Only Return Fire"));

                                // if player takes only return fire than do not set targets  - unless a return fire adds them in code below
                                if (attackerTargetOne.ShortName == "Only Return Fire" && attackerTargetTwo.ShortName == "Only Return Fire")
                                {
                                    GameLog.Core.Test.DebugFormat("breaking one attack loop for human attacker holding fire");
                                    if (unitTupleTargetOne != attackingTuple.Item1.Owner || unitTupleTargetTwo != attackingTuple.Item1.Owner) 
                                    break;
                                }

                                GameLog.Core.Test.DebugFormat("attacker ={0} {1} target one? {2} & unitTuple {3}", attackingTuple.Item1.Owner.ShortName, attackingTuple.Item1.Source.Name, attackerTargetOne.ShortName, unitTuple.Item1.Name);
                                GameLog.Core.Test.DebugFormat("unitTuple {0} {1} & target one? {2} & 'attacker' = {3}", unitTuple.Item1.Owner.ShortName, unitTuple.Item1.Name, unitTupleTargetOne.ShortName, attackingTuple.Item1.Name);

                                if ((attackerTargetOne == unitTuple.Item1.Owner || attackerTargetTwo == unitTuple.Item1.Owner || !CombatHelper.AreNotAtWar(attackingTuple.Item1.Owner, unitTuple.Item1.Owner)))
                                {
                                    GameLog.Core.Test.DebugFormat("Add Targeting of {0} for attacker {1}", unitTuple.Item1.Name, attackingTuple.Item1.Owner.ShortName);
                                    targetUnitTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == unitTuple.Item1.OwnerID).Select(sc => sc).ToList();
                                    if (targetUnitTupleList == null || targetUnitTupleList.Count() == 0)
                                        break;
                                    LoadTargets(attackingTuple.Item1.OwnerID, targetUnitTupleList); // method to load list into target dictionary

                                    GameLog.Core.Test.DebugFormat("Add returnfire of {0} for targeted' {1}", attackingTuple.Item1.Name, unitTuple.Item1.Owner.ShortName);
                                    returnFireTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == attackingTuple.Item1.OwnerID).Select(sc => sc).ToList();
                                    LoadTargets(unitTuple.Item1.OwnerID, returnFireTupleList); // return fire
                                } 

                            }
                            _attackerIDList.Add(unitTuple.Item1.OwnerID); // record civ as already having been attacker
                            _attackerIDList.Distinct().ToList();
                        }
                        _unitTupleIDList.Add(unitTuple.Item1.OwnerID); // record civ as already having been unitTuple
                        _unitTupleIDList.Distinct().ToList();
                    }
                }
                catch
                {
                    GameLog.Core.Test.DebugFormat("A try at unitTuple found no targets");
                }
                try
                {
                    foreach (var q in ownerIDs)
                    {
                        GameLog.Core.Test.DebugFormat("dictionary entry {0} is ={1}", q, TargetDictionary[q].FirstOrDefault().Item1.Owner.ShortName);
                        //TargetDictionary[q].FirstOrDefault();
                    }
                }
                catch
                {
                    GameLog.Core.Test.DebugFormat("dictionary has no targets");

                }
            }
            #endregion // populate target dictionary

            foreach (var item in ownerIDs)
            {
                GameLog.Core.Test.DebugFormat("ownerIDs contains = {0}", item);
            }

            #region Friendly and Opposition from dictionaries JUST FIRST CIV CHECKED SO FAR

            List<int> usedTheseCivIDs = new List<int>(); // do not repeat this civ
            List<int> othersCivIDs = new List<int>();

            _friendlyCombatShips = _combatShips.Where(s => s.Item1.OwnerID == ownerIDs[0]).Select(sc => sc).ToList();
            if (_willFightAlongSide[ownerIDs[0]].Count() != 0)
                _friendlyCombatShips.AddRange(_willFightAlongSide[ownerIDs[0]]);
            _friendlyCombatShips.Randomize();
            _friendlyCombatShips.Distinct().ToList();

            OppositionCombatShips = _targetDictionary[ownerIDs[0]]; 
            OppositionCombatShips.Randomize();

            var firstFriendlyUnit = _combatShips.FirstOrDefault();

            foreach (var tupleShip in OppositionCombatShips)
            {
                othersCivIDs.Add(tupleShip.Item1.OwnerID);
                othersCivIDs.Distinct().ToList();
            }

            ownerIDs.Distinct().ToList();
            // if opposition civ has third+ civs in target dictionary run that combat too
            foreach (int usedID in ownerIDs)
            {
                if (!usedTheseCivIDs.Contains(usedID))
                {
                    usedTheseCivIDs.Add(usedID); // opposition civ id
                    usedTheseCivIDs.Add(ownerIDs[0]); // friend civ id from dictionary
                    usedTheseCivIDs.Distinct().ToList();
                }
            }
            #endregion

            #region Get CycleReduction values in ResolveCombatRoundCore()

            double ratioATemp = 0.00; // used to transform ship.Count to double decimals
            double ratioBTemp = 0.00; // used to transform ship.Count to double decimals

            // Prevent division by 0, if one side has been wiped out / or retreated.
            if (OppositionCombatShips.ToList().Count == 0 || _friendlyCombatShips.Count == 0)
            {
                shipRatio = 1;
                excessShipsStartingAt = 0;
                weakerSide = 0; //0 = no bigger ships counts, 1 = First Friendly side bigger, 2 = Oppostion side bigger
            }
            else
            {
                if (_friendlyCombatShips.ToList().Count - OppositionCombatShips.ToList().Count > 0)
                {
                    excessShipsStartingAt = OppositionCombatShips.ToList().Count * 2;
                    ratioATemp = _friendlyCombatShips.Count();
                    ratioBTemp = OppositionCombatShips.Count();
                    shipRatio = ratioATemp / ratioBTemp;
                    weakerSide = 1;
                }

                else
                {
                    excessShipsStartingAt = _friendlyCombatShips.Count * 2;
                    ratioATemp = _friendlyCombatShips.Count();
                    ratioBTemp = OppositionCombatShips.Count();
                    shipRatio = ratioBTemp / ratioATemp;
                    weakerSide = 2;
                }
            }

            if (_friendlyCombatShips.Count() == OppositionCombatShips.Count())
                weakerSide = 0;
            if (shipRatio > 1.0)
            {
                newCycleReduction = 0.5;
            }

            if (shipRatio > 1.2)
            {
                newCycleReduction = 0.25;
            }
            if (shipRatio > 1.5)
            {
                newCycleReduction = 0.15;
            }
            if (shipRatio > 2.5)
            {
                newCycleReduction = 0.08;
            }
            if (shipRatio > 10)
            {
                newCycleReduction = 0.05;
            }
            if (_friendlyCombatShips.Count() < 4 || OppositionCombatShips.Count() < 4) // small fleets attack each other at full power
            {
                newCycleReduction = 1;
            }

            GameLog.Core.CombatDetails.DebugFormat("-------------  going into combat  -----------------------");
            GameLog.Core.CombatDetails.DebugFormat("various values: newCycleReduction = {0}, excessShipsStartingAt = {1}, ratioATemp = {2}, ratioBTemp = {3},  shipRatio = {4}, weakerSide = {5}",
                newCycleReduction,
                excessShipsStartingAt,
                ratioATemp,
                ratioBTemp,
                shipRatio,
                weakerSide);
            #endregion CycleReduction caluation values

            #region Sort combat units into temp file that alternating friend and opposition
           
            for (int l = 0; l < _combatShips.Count; l++) // sorting combat Ships to have one ship of each side alternating
            {
                if (l <= _friendlyCombatShips.Count - 1)
                    _combatShipsTemp.Add(_friendlyCombatShips[l]);// First Ship in _ is Friendly (initialization)

                if (l <= OppositionCombatShips.ToList().Count - 1)
                    _combatShipsTemp.Add(OppositionCombatShips.ToList()[l]); // Second Ship in _combatShipsTemp is opposition (initialization)   
            }

            _combatShips.Clear(); //  after ships where sorted into Temp, delete orginal list
                                  //  After that populate empty list with sorted temp list
            for (int m = 0; m < _combatShipsTemp.Count; m++)
            {
                _combatShips.Add(_combatShipsTemp[m]);
            }
            _combatShipsTemp.Clear(); // Temp cleared for next runthrough
            _combatShips.Randomize();

            // Stop using Temp, only use it to sort and then get rid of it

            for (int j = 0; j < _combatShips.Count; j++) // 
            {
                GameLog.Core.CombatDetails.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
                    _combatShips[j].Item1.Source.ObjectID, _combatShips[j].Item1.Source.Name, _combatShips[j].Item1.Source.Design, j);
            }
            #endregion

            #region now divide sides up for combat
            // look to define sides for each civ then target bonus and regular attack for each ship
            for (int k = 0; k < _combatShips.Count; k++)
            {
                //GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
                //     _combatShipsTemp[k].Item1.Source.ObjectID, _combatShipsTemp[k].Item1.Source.Name, _combatShipsTemp[k].Item1.Source.Design, k);

                var ownerAssets = GetAssets(_combatShips[k].Item1.Owner);

               // var oppositionCombatShips = _combatShips.Where(cs => CombatHelper.WillEngage(_combatShips[k].Item1.Owner, cs.Item1.Owner));
                var oppositionShips = _combatShips.Where(cs => (GetTargetOne(_combatShips[k].Item1.Source) == cs.Item1.Owner));

                var friendlyCombatShips = _combatShips.Where(cs => CombatHelper.WillFightAlongside(_combatShips[k].Item1.Owner, cs.Item1.Owner));

                if (k + 1 > excessShipsStartingAt && excessShipsStartingAt != 0) // added: if ships equal = 0 = excessShips, then no cycle reduction
                {
                    cycleReduction = newCycleReduction;
                } // +1 because a 2nd ship can have full firepower, but not the 3rd exceeding the other side
                else
                {
                    cycleReduction = 1;
                }

                List<string> ownCiv = _combatShips.Where(s =>
                    (s.Item1.OwnerID == ownerIDs[0]))
                    .Select(s => s.Item1.Owner.Key)
                    .Distinct()
                    .ToList();

                List<string> friendlyCivs = _combatShips.Where(s =>
                    (s.Item1.OwnerID != ownerIDs[0]) &&
                    CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShips[k].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .Distinct()
                    .ToList();

                List<string> hostileCivs = new List<string>();

                var hostileShipList = _targetDictionary[ownerIDs[0]];
                foreach (var hostilShip in hostileShipList)
                {
                    hostileCivs.Add(hostilShip.Item1.Owner.Key);
                    hostileCivs.Distinct().ToList();
                }

                //var friendShips = _willFightAlongSide[ownerIDs[0]];

                //List<int> friendIDs = new List<int>();
                //foreach (var ship in friendShips)
                //{
                //    friendIDs.Add(ship.Item1.OwnerID);
                //}
                //if (_combatShips[k].Item1.OwnerID == ownerIDs[0] || friendIDs.Contains(_combatShips[k].Item1.OwnerID)) // need us or friendly
                //{
                //    friendlyOwner = true;
                //}
                //else
                //{
                //    friendlyOwner = false;
                //}
                #endregion
                #region calcuate firepower of civilizations
                int friendlyWeaponPower = ownCiv.Sum(e => _empireStrengths[e]) + friendlyCivs.Sum(e => _empireStrengths[e]);
                int hostileWeaponPower = hostileCivs.Sum(e => _empireStrengths[e]);
                int weaponRatio = friendlyWeaponPower * 10 / (hostileWeaponPower + 1);

                //Figure out if any of the opposition ships have sensors powerful enough to penetrate our camo. If so, will be decamo.
                if (OppositionCombatShips.Count() > 0)

                {
                    maxScanStrengthOpposition = OppositionCombatShips.Max(s => s.Item1.Source.OrbitalDesign.ScanStrength);


                    if (_combatShips[k].Item1.IsCamouflaged && _combatShips[k].Item1.CamouflagedStrength < maxScanStrengthOpposition)
                    {
                        _combatShips[k].Item1.Decamouflage();
                        GameLog.Core.Combat.DebugFormat("{0} has camou strength {1} vs maxScan {2}",
                            _combatShips[k].Item1.Name, _combatShips[k].Item1.CamouflagedStrength, maxScanStrengthOpposition);
                    }
                }
                #endregion

                #region update diplomatic relations

                //TODO: Move this to DiplomacyHelper
                List<string> allEmpires = new List<string>();
                allEmpires.AddRange(ownCiv);
                allEmpires.AddRange(friendlyCivs);
                allEmpires.AddRange(hostileCivs);
                foreach (var firstEmpire in allEmpires.Distinct().ToList())
                {
                    foreach (var secondEmpire in allEmpires.Distinct().ToList())
                    {
                        if (!DiplomacyHelper.IsContactMade(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire]))
                        {
                            DiplomacyHelper.EnsureContact(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire], _combatShips[0].Item1.Source.Location);
                        }
                    }
                }
                #endregion

                #region Top of Bonus damage for combat orders combinations  
                //Each ship by attacker order (switch) vs target order and find bonus damage
                
                //bool targetIsRushing = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Rush));
                //bool targetIsInFormation = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
                //bool targetIsHailing = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Hail));
                //bool targetIsRetreating = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Retreat));
                //bool targetIsRaidTransports = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Transports));
                //bool targetIsEngage = OppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Engage));

                bool oppositionIsRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Rush));
                bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation)); 
                bool oppositionIsHailing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Hail));
                bool oppositionIsRetreating = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Retreat));
                bool oppositionIsRaidTransports = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Transports));
                bool oppositionIsEngage = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Engage));

                var order = GetCombatOrder(_combatShips[k].Item1.Source);

                var attackOrder = GetCombatOrder(_combatShips[k].Item1.Source);
                var attackingShip = _combatShips[k].Item1;
                string attackingShipType = attackingShip.Source.OrbitalDesign.ShipType.ToString();
                var targetShip = ChooseTarget(attackingShip);
                string targetShipType = targetShip.Source.OrbitalDesign.ShipType.ToString();
                var attackerManeuversToHullDamage = ((targetShip.Source.OrbitalDesign.HullStrength + 1) * attackingShip.Source.OrbitalDesign.Maneuverability / 32);// attacker ship maneuver values 1 to 8 (stations and OB = zero) so up to 25% 
                var targetManeuversToHullDamage = ((attackingShip.Source.OrbitalDesign.HullStrength + 1) * targetShip.Source.OrbitalDesign.Maneuverability / 32); // target ship maneuver values 1 to 8 (stations and OB = zero) so up to 25% 
                int chanceRushingFormation = RandomHelper.Random(100);
                //var weapons = _combatShips[k].Item2.Where(w => w.CanFire); // attacking ships weapons

                //bool assimilationSuccessful = false;
                ////If the attacker is Borg, try and assimilate before you try destroying it
                //if (attackingShip.Owner.Name == "Borg" && targetShip.Owner.Name != "Borg")
                //{
                //    GameLog.Core.Combat.DebugFormat("{0} {1} attempting assimilation on {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, targetShip.Name, targetShip.Source.ObjectID);
                //    int chanceToAssimilate = RandomHelper.Random(100);
                //    assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                //    if (assimilationSuccessful && (targetShip.Source is Ship))
                //    {
                //        Assimilate(targetShip, ownerAssets);
                //    }
                //}

                switch (attackOrder)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Rush:
                    case CombatOrder.Transports:
                    case CombatOrder.Formation:

                        //var attackingShip = _combatShips[k].Item1;
                        var target = ChooseTarget(attackingShip); // CHOOSE TARGET
                        if (order != CombatOrder.Formation && order != CombatOrder.Engage)// so maneuverable _combatShips[k] has order Rush or Transports target takes damage
                        {
                            var maneuver = attackingShip.Source.OrbitalDesign.Maneuverability;// ship maneuver values 1 to 8 (stations and OB = zero)
                            //target.TakeDamage((target.Source.OrbitalDesign.HullStrength +1) * (maneuver/32)+1); // max possible hull damage of 25%

                            GameLog.Core.Combat.DebugFormat("({2}) {0} {1}: new hull strength {3}, took damage {4} due to Maneuverability {5} from ({8}) {6} {7}",
                                target.Source.ObjectID, target.Source.Name, target.Source.Design,
                                target.Source.OrbitalDesign.HullStrength,
                                (target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1,
                                maneuver,
                                attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.Design
                                );
                            target.TakeDamage((target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1); // INFLICT HULL DAMAGE, max possible hull damage of 25%
                        }
                        if (target == null)
                        {
                            GameLog.Core.Combat.DebugFormat("No target for {0} {1} ({2})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design);
                        }
                        if (target != null)
                        {
                            GameLog.Core.Combat.DebugFormat("Target for {0} {1} ({2}) is {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);

                            // If we rush or attack transports on a formation we could take damage
                            //int chanceRushingFormation = RandomHelper.Random(100);
                            if (oppositionIsInFormation && (order == CombatOrder.Rush || order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                            {
                                attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 4);  // 25 % down out of Hullstrength of TechObjectDatabase.xml

                                GameLog.Core.Combat.DebugFormat("{0} {1} rushed or raid transports in formation and took {2} damage to hull ({3} hull left)",
                                    attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                            }
                            if (oppositionIsEngage && (order == CombatOrder.Formation || order == CombatOrder.Rush) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                            {
                                attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml

                                GameLog.Core.Combat.DebugFormat("{0} {1} in Formation or Rushing while Engaged and took {2} damage to hull ({3} hull left)",
                                    attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                            }
                            if (oppositionIsRushing && (order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                            {
                                attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml

                                GameLog.Core.Combat.DebugFormat("{0} {1} Raiding Transports and got Rushed took {2} damage to hull ({3} hull left)",
                                    attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                            }
                            if (oppositionIsRaidTransports && (order == CombatOrder.Engage) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                            {
                                attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 6);  // 17 % down out of Hullstrength of TechObjectDatabase.xml

                                GameLog.Core.Combat.DebugFormat("{0} {1} Engag order got Raided and took {2} damage to hull ({3} hull left)",
                                    attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                            }

                            #endregion

                            #region assimilation
                            bool assimilationSuccessful = false;
                            //If the attacker is Borg, try and assimilate before you try destroying it
                            if (attackingShip.Owner.Name == "Borg" && target.Owner.Name != "Borg")
                            {
                                GameLog.Core.Combat.DebugFormat("{0} {1} attempting assimilation on {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
                                int chanceToAssimilate = RandomHelper.Random(100);
                                assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                            }

                            //Perform the assimilation, but only on ships
                            if (assimilationSuccessful && target.Source is Ship)
                            {
                                GameLog.Core.Combat.DebugFormat("{0} {1} successfully assimilated {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);

                                CombatAssets oppositionAssets = GetAssets(target.Owner);
                                if (!ownerAssets.AssimilatedShips.Contains(target))
                                {
                                    ownerAssets.AssimilatedShips.Add(target);
                                    target.IsAssimilated = true;
                                }
                                if (target.Source.IsCombatant)
                                {
                                    oppositionAssets.CombatShips.Remove(target);
                                }
                                else
                                {
                                    oppositionAssets.NonCombatShips.Remove(target);
                                }
                                #endregion
                            }
                            #region performAttack in order switch
                            else // if not assmilated attack, perform attack fire weapons
                            {
                                //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);


                                var weapons = _combatShips[k].Item2.Where(w => w.CanFire);
                                int amountOfWeapons = weapons.Count();
                                //var partlyFiring = 0;


                                foreach (var weapon in _combatShips[k].Item2.Where(w => w.CanFire))
                                {
                                    if (!target.IsDestroyed)  //&& !target.) // Bug?: do not target retreated ships
                                    {
                                        PerformAttack(attackingShip, target, weapon);
                                    }
                                }
                                GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5}), amountOfWeapons = {6}",
                                    attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design,
                                    target.Source.ObjectID, target.Name, target.Source.Design,
                                    amountOfWeapons);
                                // all weapons fired for current ship k

                            }
                            #endregion
                        }


                        foreach (var combatShip in _combatShips)
                        {
                            if (combatShip.Item1.IsDestroyed)
                            {
                                ownerAssets.AssimilatedShips.Remove(target);
                            }
                        }

                        break;
                    
                    #region Retreat to break order switch

                    case CombatOrder.Retreat:

                        if (WasRetreateSuccessful(_combatShips[k].Item1, oppositionIsRushing, oppositionIsEngage, oppositionIsInFormation, oppositionIsHailing, oppositionIsRetreating, oppositionIsRaidTransports, weaponRatio))
                        {
                            try
                            {
                                // added destroyed ship cannot retreat
                                if (!ownerAssets.EscapedShips.Contains(_combatShips[k].Item1) && !_combatShips[k].Item1.IsDestroyed)
                                {

                                    ownerAssets.EscapedShips.Add(_combatShips[k].Item1);
                                }

                                if (_combatShips[k].Item1.Source.IsCombatant)
                                {

                                    ownerAssets.CombatShips.Remove(_combatShips[k].Item1);
                                }
                                else
                                {

                                    ownerAssets.NonCombatShips.Remove(_combatShips[k].Item1);
                                }

                            }
                            catch (Exception e)
                            {
                                GameLog.Core.Combat.DebugFormat("Exception e {0} ship {1} {2} {3}", e, _combatShips[k].Item1.Source.Design, _combatShips[k].Item1.Source.Name, _combatShips[k].Item1.Source.ObjectID);
                            }
                            _combatShips.Remove(_combatShips[k]);
                        }

                        // Chance of hull damage if you fail to retreat and are being rushed
                        else if (oppositionIsRushing && (weaponRatio > 1))
                        {

                            _combatShips[k].Item1.TakeDamage(_combatShips[k].Item1.Source.OrbitalDesign.HullStrength / 2);  // 50 % down out of Hullstrength of TechObjectDatabase.xml

                            GameLog.Core.Combat.DebugFormat("{0} {1} failed to retreat whilst being rushed and took {2} damage to hull ({3} hull left)",

                                _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Name, _combatShips[k].Item1.Source.OrbitalDesign.HullStrength / 2, _combatShips[k].Item1.Source.HullStrength);
                        }
                        break;

                    case CombatOrder.Hail:
                        GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) hailing...", _combatShips[k].Item1.Name, _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Design.Name);
                        break;

                    case CombatOrder.Standby:
                        GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) standing by...", _combatShips[k].Item1.Name, _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Design.Name);
                        break;
                        #endregion
                }

            }
            #region Station attack and remove Destroid

            //Make sure that the station has a go at the enemy too
            if ((_combatStation != null) && !_combatStation.Item1.IsDestroyed)
            {
                var order = GetCombatOrder(_combatStation.Item1.Source);

                CombatUnit target = null;
                switch (order)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Rush:
                    case CombatOrder.Transports:
                    case CombatOrder.Formation:
                        target = ChooseTarget(_combatStation.Item1);
                        break;
                }

                if (target != null)
                {
                    foreach (CombatWeapon weapon in _combatStation.Item2.Where(w => w.CanFire))
                    {
                        PerformAttack(_combatStation.Item1, target, weapon);
                    }
                }
            }
            
            // remove desroyed ships. Now on this spot, so that they can fire, but get still removed later
            foreach (var combatent in _combatShips) // now search for destroyed ships
            {
                if (combatent.Item1.IsDestroyed)
                {
                    var Assets = GetAssets(combatent.Item1.Owner);
                    GameLog.Core.Combat.DebugFormat("Opposition {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
                    if (combatent.Item1.Source is Ship)
                    {                      
                        if (Assets != null)
                        {
                            GameLog.Core.Combat.DebugFormat("Name of Owner = {0}, Assets.CombatShips{1}, Assets.NonCobatShips{2}", Assets.Owner.Name, Assets.CombatShips.Count, Assets.NonCombatShips.Count);

                            if (!Assets.DestroyedShips.Contains(combatent.Item1))
                            {
                                Assets.DestroyedShips.Add(combatent.Item1);
                            }
                            if (combatent.Item1.Source.IsCombatant)
                            {
                                Assets.CombatShips.Remove(combatent.Item1);
                            }
                            else
                            {
                                Assets.NonCombatShips.Remove(combatent.Item1);
                            }
                        }
                        else
                            GameLog.Core.Combat.DebugFormat("Assets Null");

                    }
                    else
                    {
                        if (_combatStation.Item1.IsDestroyed)
                        {
                            Assets = GetAssets(_combatStation.Item1.Owner);
                            // Assets.Station. How to destroy a station?
                        }
                        //remove station ???
                    }
                    continue;
                }
            }
            #endregion clean up retreated ships
            //End the combat... at turn X = 5, by letting all sides reteat
            if (_roundNumber == 5) // equals 4 engagements. Minors need an A.I. to find back to homeworld then...
            {
                _roundNumber += 1;
                var allRetreatShips = _combatShips
                    .Where(s => !s.Item1.IsDestroyed)
                    .Where(s => s.Item1.Owner != s.Item1.Source.Sector.Owner) // Ships in own territory make a stand (remain in the system they own), after 5 turns.
                    .ToList();
                foreach (var ship in allRetreatShips)
                {
                    if (ship.Item1 != null)
                    {
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1))
                        {
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                        }

                    }
                }
            }// end to end combat
             //End of Combat:
             //********************************************************************

        }// END OF RESOVLECOMBATROUNDCORE

        #region ChooseTarget()
        /// <summary>
        /// Chooses a target for the given <see cref="CombatUnit"/>
        /// </summary>
        /// <param name="attacker"></param>
        /// <returns></returns>
        private CombatUnit ChooseTarget(CombatUnit attacker)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException();
            }
            var attackerOrder = GetCombatOrder(attacker.Source);
            var attackerCivID = attacker.Owner.CivID;
            var attackingOwnerID = attacker.OwnerID;
            if (_targetDictionary[attackingOwnerID].Count() == 0)
            {
                EndCombatConditions(attacker);
            }
            var oppositionUnits = _targetDictionary[attackingOwnerID].ToList();
            bool hasOppositionStation = (_combatStation != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
            oppositionUnits.Randomize();
            var firstOppositionUint = oppositionUnits.First().Item1;

            while (true)
            {
                switch (attackerOrder)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Formation:
                        if (_targetDictionary[attacker.OwnerID].Count() > 0)
                        {
                            return firstOppositionUint;
                        }
                        break;

                    case CombatOrder.Rush:
                        //If there are any ships that are retreating, target them

                        var oppositionRetreating = oppositionUnits.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        if (oppositionRetreating.Count() > 0)
                        {
                            return oppositionRetreating.First().Item1;
                        }
                        attackerOrder = CombatOrder.Engage;
                        break;

                    case CombatOrder.Transports:
                        //If there are transports and they are not in formation, target them
                        var oppositionTransports = oppositionUnits.Where(cs => (cs.Item1.Source.OrbitalDesign.ShipType == "Transport") && !cs.Item1.IsDestroyed);
                        bool oppositionIsInFormation = oppositionUnits.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
                        if ((oppositionTransports.Count() > 0) && (!oppositionIsInFormation))
                        {
                            return oppositionTransports.First().Item1;
                        }
                        //If there any ships retreating, target them

                        var oppositionRetreatingRaid = oppositionUnits.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        if (oppositionRetreatingRaid.Count() > 0)
                        {
                            return oppositionRetreatingRaid.First().Item1;
                        }
                        attackerOrder = CombatOrder.Engage;
                        break;
                    default:
                        return firstOppositionUint; ///oppositionRetreatingRaid.First().Item1; ;
                }
            }
        }
        /// <summary>
        /// Deals damage to the target, and calculates whether the target has been destroyed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="weapon"></param>
        /// 
        #endregion //choose target

        #region PerformAttack
        private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        {                
            var sourceAccuracy = source.Source.GetAccuracyModifier(); // var? Or double?
            var maneuverability = target.Source.GetManeuverablility(); // byte
            if (sourceAccuracy > 1 || sourceAccuracy < 0.1)  // if getting odd numbers, take normal one, until bug fixed
            {
                GameLog.Core.CombatDetails.DebugFormat("sourceAccuracy {0} out of range, now reset to 0.5", sourceAccuracy);
                sourceAccuracy = 0.5;
            }
            var targetDamageControl = target.Source.GetDamageControlModifier();
            if (targetDamageControl > 1 || targetDamageControl < 0.1)  // if getting damge control is odd, take standard until bug fixed
                targetDamageControl = 0.5;

            // if side ==2 opposition is stronger, first frienldy side gets the bonus and side ==1 first friendly side has more ships, opposition side gets the bonus
            switch (weakerSide)
            {
                //if (weakerSide == 1) //first (Friendly) side has more ships
                case 1:
                    {
                        if (source.Owner != target.Owner || !friendlyOwner) //(If it is an opposition ship[ not first owner of firendly to first owner] improve on thier fire)
                        {
                            sourceAccuracy = 1.0 + (1 - newCycleReduction);
                            if (sourceAccuracy > 1.5)
                            {
                                sourceAccuracy = 1.45;
                            }
                            cycleReduction = 1;
                        }
                        break;
                    }
                //else if (wearkerSide == 0)
                case 0:
                    {
                        // if wearkerSide == 0, then both are equal. Do no change
                        cycleReduction = 1;
                        break;
                    }
                //else if (wearkerSide == 2) 
                case 2:// Opposition side has more ships so cycle
                    {
                        if (source.Owner == target.Owner || friendlyOwner) //(If it is samne owner as first, or friendly to first, improve on thier fire)
                        {
                            sourceAccuracy = 1.0 + (1 - newCycleReduction);
                            if (sourceAccuracy > 1.5)
                            {
                                sourceAccuracy = 1.45;
                            }
                            cycleReduction = 1;
                        } // First (friend) owner is source owner or performAttack is on a friendlyOwner as source owner call from the _combatShipTemp cycle
                        break;
                    }
            }
            // if firing ship OR targeted ship are heroShips, change values to be better.
            if (source.Name.Contains("!"))
            {
                sourceAccuracy = 1.7; // change to 170% accuracy
            }

            if (target.Name.Contains("!"))
            {
                targetDamageControl = 1;
            }
            // Added lines to reduce damage to SB and OB to 10%. Also  Changed damage to 2.5 instead of 4. and 10 instead of 50
            if (!target.IsMobile &&
                target.Source.Sector.Name == "Sol"
                || target.Source.Sector.Name == "Terra"
                || target.Source.Sector.Name == "Omarion"
                || target.Source.Sector.Name == "Borg"
                || target.Source.Sector.Name == "Qo'noS"
                || target.Source.Sector.Name == "Romulus"
                || target.Source.Sector.Name == "Cardassia")
            {
                targetDamageControl = 1.4;
                //GameLog.Core.Combat.DebugFormat("targetDamageControl = {0} due to HomeSystemStation or OB at {1}", targetDamageControl, target.Source.Sector.Name);
            } // end added lines
              // currentx
            double currentManeuverability = maneuverability;// get int target maneuverablity, convert to double
            double ManeuverabilityModifer = 0.0;
            var sourceAccuracyTemp = 0.5;
            if (sourceAccuracy > 0.9 && sourceAccuracy < 1.7)
                sourceAccuracyTemp = 0.6;
            ManeuverabilityModifer = ((5 - currentManeuverability) / 10); // +/- 0.4 Targets maneuverablity
            sourceAccuracyTemp = sourceAccuracyTemp + ManeuverabilityModifer;
            if (sourceAccuracyTemp < 0.0 || sourceAccuracyTemp > 1) // prevent out of range numbers
                sourceAccuracyTemp = 0.5;

            if (sourceAccuracy == 1.7) // if heroship value, use it
                sourceAccuracyTemp = 1.7;

            //GameLog.Core.CombatDetails.DebugFormat("various values: {0} {1} {2} at {3} ({4}), OTHERS: friendlyOwner = {6}, firstOwner = {6}",
            //source.Source.ObjectID, source.Source.Name, source.Source.Design, target.Source.Sector.Name, target.Source.Sector.Location, friendlyOwner.ToString(), firstOwner.ToString());

            //GameLog.Core.CombatDetails.DebugFormat("various values: sourceAccuracy = {0}, sourceAccuracyTemp = {1}, maneuverability = {2}, currentManeuverability = {3}, ManeuverabilityModifer = {4}, targetDamageControl = {5}",
            //sourceAccuracy,
            //sourceAccuracyTemp,
            //maneuverability,
            //currentManeuverability,
            //ManeuverabilityModifer,
            //targetDamageControl
            //);

            if (RandomHelper.Random(100) <= (100 * sourceAccuracyTemp))  // not every weapons does a hit
            {

                // Fire Weapons, inflict damage
                target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10)); // minimal damage of 50 included

                GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), DamageControl = {6}, Shields = {7}, Hull = {8}",
                    target.Source.ObjectID, target.Name, target.Source.Design,
                    (int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10),
                    cycleReduction,
                    sourceAccuracy,
                    targetDamageControl,
                    target.ShieldStrength,
                    target.HullStrength
                    );
            }
            weapon.Discharge();
        }
        #endregion

        #region Methodes

        private void LoadTargets(int ownerID, List<Tuple<CombatUnit, CombatWeapon[]>> listTuple)
        {
            //listTuple = _shipListDictionary[ownerID];  //_combatShips.Where(cs => cs.Item1.OwnerID == ownerID).Select(cs => cs).ToList();
            if (_targetDictionary[ownerID].Count() != 0)
            {
                GameLog.Core.Test.DebugFormat("dictionary at {0} is count 0? ={1}", ownerID, _targetDictionary[ownerID].FirstOrDefault().Item1.Owner.ShortName);
                listTuple.Distinct().ToList();
                _targetDictionary[ownerID].AddRange(listTuple);
            }
            else
            {
                _targetDictionary[ownerID] = listTuple;
                GameLog.Core.Test.DebugFormat("dictionary at {0} has ={1}", ownerID, _targetDictionary[ownerID].FirstOrDefault().Item1.Owner.ShortName); 
            }

        }
        //private void Assimilate(CombatUnit target, CombatAssets ownerAssets)
        //{
        //    //Perform the assimilation, but only on ships
        //        GameLog.Core.Combat.DebugFormat("successfully assimilated {0} {1}", target.Name, target.Source.ObjectID);

        //        CombatAssets oppositionAssets = GetAssets(target.Owner);
        //        if (!ownerAssets.AssimilatedShips.Contains(target))
        //        {
        //            ownerAssets.AssimilatedShips.Add(target);
        //            target.IsAssimilated = true;
        //        }
        //        if (target.Source.IsCombatant)
        //        {
        //            oppositionAssets.CombatShips.Remove(target);
        //        }
        //        else
        //        {
        //            oppositionAssets.NonCombatShips.Remove(target);
        //        }
            
        //}
        private void EndCombatConditions(CombatUnit attacker)
        {
            FriendlyCombatShips = _combatShips.Where(s => s.Item1.OwnerID == attacker.OwnerID).Select(s => s).ToList();
        }
        #endregion
    }
}

