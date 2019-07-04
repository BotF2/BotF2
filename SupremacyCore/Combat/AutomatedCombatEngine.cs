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
        //private double cycleReduction = 1d;
        //private double newCycleReduction = 1d;
        //private double excessShipsStartingAt;
        //private double shipRatio = 1;
        //private bool friendlyOwner = true;
        private int[,] empiresInBattle;
       // private int weakerSide = 0; // 0= no bigger ships counts, 1= First Friendly side bigger, 2= Oppostion side bigger
        //private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _targetDictionary;
        //private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _shipListDictionary;
        //private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _willFightAlongSide;
        //private List<Tuple<CombatUnit, CombatWeapon[]>> _friendlyCombatShips;
        //private List<Tuple<CombatUnit, CombatWeapon[]>> _oppositionCombatShips;
        //private List<Tuple<CombatUnit, CombatWeapon[]>> _defaultCombatShips;
        //private Dictionary<CombatUnit, int> _shipFirePower;
        //public Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> ShipListtDictionary // do we need this? just look in _combatShips by ownerID?
        //{
        //    get
        //    {
        //        return _shipListDictionary;
        //    }
        //    set
        //    {
        //        this._shipListDictionary = value;
        //    }
        //}
        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {
            //_targetDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            //_shipListDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            //_willFightAlongSide = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            //_friendlyCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            //_oppositionCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            //_defaultCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            empiresInBattle = new int[12, 3];
        }
        protected override void ResolveCombatRoundCore()
        {
            // Setting variables to standard (initilization) of these fields
            //shipRatio = 1;
            //excessShipsStartingAt = 0;
            //weakerSide = 0;
            //cycleReduction = 1;
            //newCycleReduction = 1;
            //int maxScanStrengthOpposition = 0;
            GameLog.Core.CombatDetails.DebugFormat("_combatShips.Count: {0}", _combatShips.Count());

            // Scouts, Frigate and cloaked ships have a special chance of retreating BEFORE round 3
            if (_roundNumber < 7) // multiplayer starts at round 5
            {
                GameLog.Core.Test.DebugFormat("round# ={0} now", _roundNumber);
                //  Once a ship has retreated, its important that it does not do it again..
                var easyRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked == true || (s.Item1.Source.OrbitalDesign.ShipType == "Frigate") || (s.Item1.Source.OrbitalDesign.ShipType == "Scout"))
                    .Where(s => !s.Item1.IsDestroyed) //  Destroyed ships cannot retreat
                    .Where(s => GetCombatOrder(s.Item1.Source) == CombatOrder.Retreat)
                    .ToList();
                foreach (var ship in easyRetreatShips)
                {
                    if (!RandomHelper.Chance(10) && (ship.Item1 != null)) // 90% to reatreat
                    {
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1)) // escaped ships cannot escape again
                        {
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                            GameLog.Core.Test.DebugFormat("Easy retreated ={0}", ship.Item1.Name);
                        }
                    }
                }
                // other ships with retreat order have a lesser chance to retreat
                var hardRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked != true && (s.Item1.Source.OrbitalDesign.ShipType != "Frigate") && (s.Item1.Source.OrbitalDesign.ShipType != "Scout"))
                    .Where(s => !s.Item1.IsDestroyed) //  Destroyed ships cannot retreat
                    .Where(s => GetCombatOrder(s.Item1.Source) == CombatOrder.Retreat)
                    .ToList();
                foreach (var ship in hardRetreatShips)
                {
                    if (!RandomHelper.Chance(2) && (ship.Item1 != null)) // 2 = 50% to reatreat
                    {
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1)) // escaped ships cannot escape again
                        {
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                            GameLog.Core.Test.DebugFormat("Hard retreated ={0}", ship.Item1.Name);
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

                //Resistance is futile, try assimilation before you attack then retreat if assimilated
                bool foundDaBorg = _combatShips.Any(borg => borg.Item1.Owner.ShortName == "Borg");
                bool assimilationSuccessful = false;
                var notDaBorg = _combatShips.Where(xborg => xborg.Item1.Owner.ShortName != "Borg").Select(xborg => xborg).ToList();
                if (foundDaBorg)
                {
                    foreach (var target in notDaBorg)
                    {
                        int chanceToAssimilate = RandomHelper.Random(100);
                        assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                        if (target.Item1.Source is Ship && assimilationSuccessful)
                        {
                            var ownerAssets = GetAssets(target.Item1.Owner);
                            if (!ownerAssets.EscapedShips.Contains(target.Item1)) // escaped ships cannot escape again
                            {
                                ownerAssets.EscapedShips.Add(target.Item1);
                                ownerAssets.CombatShips.Remove(target.Item1);
                                ownerAssets.NonCombatShips.Remove(target.Item1);
                                _combatShips.Remove(target);
                                GameLog.Core.Test.DebugFormat("Assimilated ={0}", target.Item1.Name);
                            }
                        }
                    }
                } 
            }
            // list of civs (owner ids) that are still in combat sector (going into combat) after retreat and assimilation - retreat
            List<int> ownerIDs = new List<int>();
            foreach (var tupleShip in _combatShips)
            {
                ownerIDs.Add(tupleShip.Item1.OwnerID);
                //_targetDictionary[tupleShip.Item1.OwnerID] = _defaultCombatShips;
            }
            ownerIDs.Distinct().ToList();
            #region setup dictionaries
            //_targetDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>(); // now a dictoinary and not a list
            //_targetDictionary.Clear();
            //ShipListtDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
            //ShipListtDictionary.Clear();
            // get owner ids for the ships in this sector (ownerIDs)

            //var CivOne = ownerIDs.First();
            //GameLog.Core.CombatDetails.DebugFormat("CivOne = {0}", CivOne);
            // populate dictionary of ships in a lists for each owner, The key is owner id (_shipListDictionary)
            //foreach (var ownerID in ownerIDs)
            //{
            //    var listOfShipsByOwnerID = _combatShips.Where(sc => sc.Item1.OwnerID == ownerID).Select(sc => sc).ToList();
            //    _shipListDictionary[ownerID] = listOfShipsByOwnerID;
            //}
            //// populate dictionary of will fight alongside ships in a list for each owner
            //for (int t = 0; t < _combatShips.Count(); t++)
            //{
            //    //var ownerAssets = GetAssets(_combatShips[t].Item1.Owner);
            //    _willFightAlongSide[_combatShips[t].Item1.OwnerID] = _combatShips.Where(cs => CombatHelper.WillFightAlongside(_combatShips[t].Item1.Owner, cs.Item1.Owner))
            //        .Select(cs => cs)
            //        .ToList();
            //    _willFightAlongSide.Distinct().ToList();
            //}

            //List<int> _unitTupleIDList = new List<int>();
            //List<int> _attackerIDList = new List<int>();
            //List<Tuple<CombatUnit, CombatWeapon[]>> targetUnitTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>(); // list of target tuples
            //List<Tuple<CombatUnit, CombatWeapon[]>> returnFireTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            #endregion

            #region populate target dictionaries
            // populate target dictionary with lists of target units (_oppositionCombatShips), key is owner id / civ id
            //foreach (var unitTuple in _combatShips)
            //{
            //    //bool foundBorgShip = (_combatShips.Where(sc => sc.Item1.Owner.ShortName == "Borg").Select(sc => sc).ToList().Any()); // any borg here?
            //    try
            //    {
            //        if (!_unitTupleIDList.Contains(unitTuple.Item1.OwnerID)) // only pass in each civ once
            //        {
            //            GameLog.Core.Test.DebugFormat("--------------------------");
            //            GameLog.Core.Test.DebugFormat("Top of loop unitTuple {0} {1}", unitTuple.Item1.Owner, unitTuple.Item1.Name);
            //            foreach (var attackingTuple in _combatShips)
            //            {
            //                if (attackingTuple.Item1.OwnerID == unitTuple.Item1.OwnerID)
            //                    continue;
            //                GameLog.Core.Test.DebugFormat("Top of loop attackingTuple = {0} {1}", attackingTuple.Item1.Source.ObjectID, attackingTuple.Item1.Name);
            //                if (attackingTuple.Item1.OwnerID != unitTuple.Item1.OwnerID && !_attackerIDList.Contains(attackingTuple.Item1.OwnerID))  // don't check your own ships & only pass in each civ as attacker once
            //                {
            //                    Civilization attackerTargetOne = new Civilization();
            //                    Civilization attackerTargetTwo = new Civilization();
            //                    Civilization unitTupleTargetOne = new Civilization();
            //                    Civilization unitTupleTargetTwo = new Civilization();
            //                    try
            //                    {
            //                        attackerTargetOne = GetTargetOne(attackingTuple.Item1.Source);
            //                    }
            //                    catch
            //                    {
            //                        attackerTargetOne = CombatHelper.GetDefaultHoldFireCiv();
            //                    }
            //                    try
            //                    {
            //                        attackerTargetTwo = GetTargetTwo(attackingTuple.Item1.Source);
            //                    }
            //                    catch
            //                    {
            //                        attackerTargetTwo = CombatHelper.GetDefaultHoldFireCiv();
            //                    }
            //                    try
            //                    {
            //                        unitTupleTargetOne = GetTargetOne(unitTuple.Item1.Source);
            //                    }
            //                    catch
            //                    {
            //                        unitTupleTargetOne = CombatHelper.GetDefaultHoldFireCiv();
            //                    }
            //                    try
            //                    {
            //                        unitTupleTargetTwo = GetTargetTwo(unitTuple.Item1.Source);
            //                    }
            //                    catch
            //                    {
            //                        unitTupleTargetTwo = CombatHelper.GetDefaultHoldFireCiv();
            //                    }
            //                    GameLog.Core.Test.DebugFormat("Attacker {0} with Target1 ={1} & 2={2}",
            //                        attackingTuple.Item1.Source.Name, attackerTargetOne.ShortName, attackerTargetTwo.ShortName);
            //                    GameLog.Core.Test.DebugFormat("unitTuple {0} with Target1 ={1} & 2={2}",
            //                        unitTuple.Item1.Source.Name, unitTupleTargetOne.ShortName, unitTupleTargetTwo.ShortName);
            //                    //GameLog.Core.Test.DebugFormat("found borg ship? {0}", foundBorgShip);
            //                    // if both sides are default targeting holdFireCiv then look for other targets in the sector anyway
            //                    //ToDo: AI put in check if they are already being targeted by a third party? (Why look for more) & put in a check of civ behavior like war-like or peaceful?
            //                    if (unitTupleTargetOne.ShortName == "DefaultHoldFireCiv" && attackerTargetOne.ShortName == "DefaultHoldFireCiv") //attackerTargetOne.ShortName == "Borg" && unitTupleTargetOne.ShortName == "Borg" && !foundBorgShip                 
            //                    {
            //                        if (!CombatHelper.WillFightAlongside(_combatShips.Last().Item1.Owner, attackingTuple.Item1.Owner) && attackingTuple.Item1.OwnerID != unitTuple.Item1.OwnerID)
            //                        {
            //                            GameLog.Core.Test.DebugFormat("Two defaulted Hold fire Players trying to add targets {0} or {1}", _combatShips.Last().Item1.Owner.ShortName, _combatShips.First().Item1.Owner.ShortName);
            //                            if (attackingTuple.Item1.OwnerID != _combatShips.Last().Item1.OwnerID)
            //                                attackerTargetOne = _combatShips.Last().Item1.Owner; // desperation target when no Borg and last ship is not your ally
            //                            else if (attackingTuple.Item1.OwnerID != _combatShips.First().Item1.OwnerID)
            //                                attackerTargetOne = _combatShips.First().Item1.Owner;
            //                        }
            //                    }
            //                    GameLog.Core.Test.DebugFormat("if attacker only returns fire 1 & 2 true={0} should see breaking loop next", (attackerTargetOne.ShortName == "Only Return Fire" && attackerTargetTwo.ShortName == "Only Return Fire"));
            //                    // if player takes only return fire than do not set targets  - unless a return fire adds them in code below
            //                    if (attackerTargetOne.ShortName == "Only Return Fire" && attackerTargetTwo.ShortName == "Only Return Fire")
            //                    {
            //                        GameLog.Core.Test.DebugFormat("breaking one attack loop for human attacker holding fire");
            //                        if (unitTupleTargetOne != attackingTuple.Item1.Owner || unitTupleTargetTwo != attackingTuple.Item1.Owner)
            //                            break;
            //                    }
            //                    GameLog.Core.Test.DebugFormat("attacker ={0} {1} target one? {2} & unitTuple {3}", attackingTuple.Item1.Owner.ShortName, attackingTuple.Item1.Source.Name, attackerTargetOne.ShortName, unitTuple.Item1.Name);
            //                    GameLog.Core.Test.DebugFormat("unitTuple {0} {1} & target one? {2} & 'attacker' = {3}", unitTuple.Item1.Owner.ShortName, unitTuple.Item1.Name, unitTupleTargetOne.ShortName, attackingTuple.Item1.Name);
            //                    if ((attackerTargetOne == unitTuple.Item1.Owner || attackerTargetTwo == unitTuple.Item1.Owner || !CombatHelper.AreNotAtWar(attackingTuple.Item1.Owner, unitTuple.Item1.Owner)))
            //                    {
            //                        GameLog.Core.Test.DebugFormat("Add Targeting of {0} for attacker {1}", unitTuple.Item1.Name, attackingTuple.Item1.Owner.ShortName);
            //                        targetUnitTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == unitTuple.Item1.OwnerID).Select(sc => sc).ToList();
            //                        if (targetUnitTupleList == null || targetUnitTupleList.Count() == 0)
            //                            break;
            //                        LoadTargets(attackingTuple.Item1.OwnerID, targetUnitTupleList); // method to load list into target dictionary
            //                        GameLog.Core.Test.DebugFormat("Add returnfire of {0} for targeted' {1}", attackingTuple.Item1.Name, unitTuple.Item1.Owner.ShortName);
            //                        returnFireTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == attackingTuple.Item1.OwnerID).Select(sc => sc).ToList();
            //                        LoadTargets(unitTuple.Item1.OwnerID, returnFireTupleList); // return fire
            //                    }
            //                }
            //                _attackerIDList.Add(unitTuple.Item1.OwnerID); // record civ as already having been attacker
            //                _attackerIDList.Distinct().ToList();
            //            }
            //            _unitTupleIDList.Add(unitTuple.Item1.OwnerID); // record civ as already having been unitTuple
            //            _unitTupleIDList.Distinct().ToList();
            //        }
            //    }
            //    catch
            //    {
            //        GameLog.Core.Test.DebugFormat("A try at unitTuple found no targets");
            //    }
            //    try
            //    {
            //        foreach (var q in ownerIDs)
            //        {
            //            GameLog.Core.Test.DebugFormat("dictionary entry {0} is ={1}", q, _targetDictionary[q].FirstOrDefault().Item1.Owner.ShortName);
            //            //_targetDictionary[q].FirstOrDefault();
            //        }
            //    }
            //    catch
            //    {
            //        GameLog.Core.Test.DebugFormat("dictionary has no targets");
            //    }
            //}
            #endregion // populate target dictionary

            #region Construct empires (civs) in battle and Ships per empires arrays
            int[,] empiresInBattle; // An Array of who is in the battle with what targets.
            empiresInBattle = new int[12, 3]; // an Array with 2 Dimensions. First with up to 12 elements, 2nd with up to 3 elements.
                                              // 12 Elements can hold 12 participating empires (civilizations CivID OwnerID). 
                                              //empiresInBattle[0, 0] contains the CivID of the FirstPlayer
                                              //empiresInBattle[0, 1] contains the Target1 of that empire (civ As CivID as well)
                                              //empiresInBattle[0, 2] contains the Target2 of that empire.
                                              // Re-Start Array with 999 everywhere
            // Initialize first Array  // UPDATE X 25 june 2019 changed 11 to 12  
            for (int i = 0; i < 12; i++)
            {
                for (int i2 = 0; i2 < 3; i2++)
                {
                    empiresInBattle[i, i2] = 999;
                }
            }
            int[,] shipsPerEmpire;
            shipsPerEmpire = new int[12, 3];
            // First int (12) = value of the 12 Empires (civilizations CivID OnwerID)
            // Second int 0 = value is EmpireID (CivID OwnerID)
            // Second int 1 = value is Total Ship in Battle (uncluding Station?)
            // Second int 2 = 0, meaning 0 ships have fired before battle starts.
            List<int> allparticipatings = new List<int>();
            allparticipatings.Clear();
            int z = 0;
            foreach (int ownerID in ownerIDs.Distinct())
            {
                allparticipatings.Add(ownerID);
                var ListOfShipsOfEmpire = _combatShips.Where(sc => sc.Item1.OwnerID == ownerID).Select(sc => sc).ToList();
                shipsPerEmpire[z, 0] = ownerID;
                shipsPerEmpire[z, 1] = ListOfShipsOfEmpire.Count();
                shipsPerEmpire[z, 2] = 0;
                    z += 1;
            }
            #endregion

            #region Add target civs into empires (civs) array
            //for (int q = 0; q < 12; q++)
            //{ 
            int q = 0;
            foreach (int ownerID in ownerIDs.Distinct())
            {
                empiresInBattle[q, 0] = ownerID;
                // foreach (var unitTuple in _combatShips)
                //{
                //  if (unitTuple.Item1.OwnerID == ownerID)
                //{
                var dummyships = _combatShips.Where(sc => sc.Item1.OwnerID == ownerID).Select(sc => sc).ToList();
                var dummyship = dummyships.FirstOrDefault();
                empiresInBattle[q, 1] = Convert.ToInt32(GetTargetOne(dummyship.Item1.Source).CivID);
                empiresInBattle[q, 2] = Convert.ToInt32(GetTargetTwo(dummyship.Item1.Source).CivID);
                // If AI DOES NOT HAVE TARGET
                var civi = GameContext.Current.Civilizations[empiresInBattle[q, 0]];
                if (civi.CivID == 999)
                {
                    break;
                }
                if (civi.IsHuman)
                {
                    if (empiresInBattle[q, 1] == 888 && empiresInBattle[q, 2] == 888) /// 888 = human 'hold fire' target click, 777 = AI dummy target
                    {
                        empiresInBattle[q, 1] = 999; // 999 = null = no active fire)
                        empiresInBattle[q, 2] = 999;
                    }
                    if (empiresInBattle[q, 1] == 888 && empiresInBattle[q, 2] != 888)
                    {
                        empiresInBattle[q, 1] = empiresInBattle[q, 2];
                    }
                    if (empiresInBattle[q, 2] == 888 && empiresInBattle[q, 1] != 888)
                        empiresInBattle[q, 2] = empiresInBattle[q, 1];
                }
                //if AI
                else
                {
                    // UPDATE X 25 june 2019 added if == 999 & warlike then choose a random target. Also DiplomaticReport needs to change to traits, but currently everyone has trait = compassion
                    if ((empiresInBattle[q, 1] == 777 || empiresInBattle[q, 1] == 999) && (civi.DiplomacyReport.Contains("Warlike") || civi.DiplomacyReport.Contains("Hostile")))
                    {
                        while (true)
                        {
                            empiresInBattle[q, 1] = allparticipatings.RandomElement();
                            if (empiresInBattle[q, 1] == empiresInBattle[q, 0])
                            {
                                // try again, i don´t want to fire on myselve
                            }
                            else
                            {
                                // found a target that not me, continue
                                break;
                            }
                        }
                    }
                    else
                    {
                        empiresInBattle[q, 1] = 999;
                    }
                    // UPDATE X 25 june 2019 added if == 999 & warlike then choose a random target. + Minichange, from DiplomacyReport back to Traits
                    if ((empiresInBattle[q, 2] == 777 || empiresInBattle[q, 2] == 999) && (civi.Traits.Contains("Warlike") || civi.Traits.Contains("Hostile")))
                    {
                        while (true)
                        {
                            empiresInBattle[q, 2] = allparticipatings.RandomElement();
                            if (empiresInBattle[q, 2] == empiresInBattle[q, 0])
                            {
                                // try again, i don´t want to fire on myselve
                            }
                            else
                            {
                                // found a target that not me, continue
                                break;
                            }
                        }
                    }
                    else
                    {
                        empiresInBattle[q, 2] = 999;
                    }
                    bool alreadyAtWar = false;
                    foreach (int ownerIDWar in ownerIDs)
                    {
                        var civi2 = GameContext.Current.Civilizations[ownerIDWar];
                        if (!CombatHelper.AreNotAtWar(civi, civi2))
                        {
                            // if(empiresInBattle[q, 1] = civi2.CivID)
                            //   empiresInBattle[q, 2] = civi2.CivID;
                            if (alreadyAtWar == true)
                            {
                                empiresInBattle[q, 2] = civi2.CivID;
                            }
                            else
                            {
                                empiresInBattle[q, 1] = civi2.CivID;
                                alreadyAtWar = true;
                            }
                            // Could add difficulty: if human, fire always at human?
                        }
                    }
                }
                //}
                GameLog.Core.Combat.DebugFormat("Empire Civ in Battle: {0} FirstTarget = {1} 2nd Target = {2}, Civ q = {3}", empiresInBattle[q, 0], empiresInBattle[q, 1], empiresInBattle[q, 2], q);
                q = q + 1;
                //    GameLog.Core.Combat.DebugFormat("Empire Civ in Battle: {0} FirstTarget = {1} 2nd Target = {2}", empiresInBattle[q, 0], empiresInBattle[q, 1], empiresInBattle[q, 2]);
            }
            #endregion
            foreach (var item in ownerIDs)
            {
                GameLog.Core.Test.DebugFormat("ownerIDs contains = {0}", item);
            }
            #region Friendly and Opposition from dictionaries JUST FIRST CIV CHECKED SO FAR
            //List<int> usedTheseCivIDs = new List<int>(); // do not repeat this civ
            //List<int> othersCivIDs = new List<int>();
            //_friendlyCombatShips = _combatShips.Where(s => s.Item1.OwnerID == ownerIDs[0]).Select(sc => sc).ToList();
            //if (_willFightAlongSide[ownerIDs[0]].Count() != 0)
            //    _friendlyCombatShips.AddRange(_willFightAlongSide[ownerIDs[0]]);
            //_friendlyCombatShips.Randomize();
            //_friendlyCombatShips.Distinct().ToList();
            //_oppositionCombatShips = _targetDictionary[ownerIDs[0]];
            //_oppositionCombatShips.Randomize();
            //var firstFriendlyUnit = _combatShips.FirstOrDefault();
            //foreach (var tupleShip in _oppositionCombatShips)
            //{
            //    othersCivIDs.Add(tupleShip.Item1.OwnerID);
            //    othersCivIDs.Distinct().ToList();
            //}
            //ownerIDs.Distinct().ToList();
            //// if opposition civ has third+ civs in target dictionary run that combat too
            //foreach (int usedID in ownerIDs)
            //{
            //    if (!usedTheseCivIDs.Contains(usedID))
            //    {
            //        usedTheseCivIDs.Add(usedID); // opposition civ id
            //        usedTheseCivIDs.Add(ownerIDs[0]); // friend civ id from dictionary
            //        usedTheseCivIDs.Distinct().ToList();
            //    }
            //}
            #endregion

            #region Get CycleReduction values in ResolveCombatRoundCore()
            //double ratioATemp = 0.00; // used to transform ship.Count to double decimals
            //double ratioBTemp = 0.00; // used to transform ship.Count to double decimals
            //// Prevent division by 0, if one side has been wiped out / or retreated.
            //if (_oppositionCombatShips.ToList().Count == 0 || _friendlyCombatShips.Count == 0)
            //{
            //    shipRatio = 1;
            //    excessShipsStartingAt = 0;
            //    weakerSide = 0; //0 = no bigger ships counts, 1 = First Friendly side bigger, 2 = Oppostion side bigger
            //}
            //else
            //{
            //    if (_friendlyCombatShips.ToList().Count - _oppositionCombatShips.ToList().Count > 0)
            //    {
            //        excessShipsStartingAt = _oppositionCombatShips.ToList().Count * 2;
            //        ratioATemp = _friendlyCombatShips.Count();
            //        ratioBTemp = _oppositionCombatShips.Count();
            //        shipRatio = ratioATemp / ratioBTemp;
            //        weakerSide = 1;
            //    }
            //    else
            //    {
            //        excessShipsStartingAt = _friendlyCombatShips.Count * 2;
            //        ratioATemp = _friendlyCombatShips.Count();
            //        ratioBTemp = _oppositionCombatShips.Count();
            //        shipRatio = ratioBTemp / ratioATemp;
            //        weakerSide = 2;
            //    }
            //}
            //if (_friendlyCombatShips.Count() == _oppositionCombatShips.Count())
            //    weakerSide = 0;
            //if (shipRatio > 1.0)
            //{
            //    newCycleReduction = 0.5;
            //}
            //if (shipRatio > 1.2)
            //{
            //    newCycleReduction = 0.25;
            //}
            //if (shipRatio > 1.5)
            //{
            //    newCycleReduction = 0.15;
            //}
            //if (shipRatio > 2.5)
            //{
            //    newCycleReduction = 0.08;
            //}
            //if (shipRatio > 10)
            //{
            //    newCycleReduction = 0.05;
            //}
            //if (_friendlyCombatShips.Count() < 4 || _oppositionCombatShips.Count() < 4) // small fleets attack each other at full power
            //{
            //    newCycleReduction = 1;
            //}
            //GameLog.Core.CombatDetails.DebugFormat("-------------  going into combat  -----------------------");
            //GameLog.Core.CombatDetails.DebugFormat("various values: newCycleReduction = {0}, excessShipsStartingAt = {1}, ratioATemp = {2}, ratioBTemp = {3},  shipRatio = {4}, weakerSide = {5}",
            //    newCycleReduction,
            //    excessShipsStartingAt,
            //    ratioATemp,
            //    ratioBTemp,
            //    shipRatio,
            //    weakerSide);
            //#endregion CycleReduction caluation values
            //#region Sort combat units into temp file that alternating friend and opposition
            //for (int l = 0; l < _combatShips.Count; l++) // sorting combat Ships to have one ship of each side alternating
            //{
            //    if (l <= _friendlyCombatShips.Count - 1)
            //        _combatShipsTemp.Add(_friendlyCombatShips[l]);// First Ship in _ is Friendly (initialization)
            //    if (l <= _oppositionCombatShips.ToList().Count - 1)
            //        _combatShipsTemp.Add(_oppositionCombatShips.ToList()[l]); // Second Ship in _combatShipsTemp is opposition (initialization)   
            //}
            //_combatShips.Clear(); //  after ships where sorted into Temp, delete orginal list
            //                      //  After that populate empty list with sorted temp list
            //for (int m = 0; m < _combatShipsTemp.Count; m++)
            //{
            //    _combatShips.Add(_combatShipsTemp[m]);
            //}
            //_combatShipsTemp.Clear(); // Temp cleared for next runthrough
            //_combatShips.Randomize();
            //// Stop using Temp, only use it to sort and then get rid of it
            //for (int j = 0; j < _combatShips.Count; j++) // 
            //{
            //    GameLog.Core.CombatDetails.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
            //        _combatShips[j].Item1.Source.ObjectID, _combatShips[j].Item1.Source.Name, _combatShips[j].Item1.Source.Design, j);
            //}
            #endregion

            #region now divide sides up for combat
            // look to define sides for each civ then target bonus and regular attack for each ship
            //for (int k = 0; k < _combatShips.Count; k++)
            //{
            //    //GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
            //    //     _combatShipsTemp[k].Item1.Source.ObjectID, _combatShipsTemp[k].Item1.Source.Name, _combatShipsTemp[k].Item1.Source.Design, k);
            //    var ownerAssets = GetAssets(_combatShips[k].Item1.Owner);
            //    var oppositionShips = _combatShips.Where(cs => (GetTargetOne(_combatShips[k].Item1.Source) == cs.Item1.Owner)).Where(cs => (GetTargetTwo(_combatShips[k].Item1.Source) == cs.Item1.Owner));
            //    var friendlyCombatShips = _combatShips.Where(cs => CombatHelper.WillFightAlongside(_combatShips[k].Item1.Owner, cs.Item1.Owner));
            //    //if (k + 1 > excessShipsStartingAt && excessShipsStartingAt != 0) // added: if ships equal = 0 = excessShips, then no cycle reduction
            //    //{
            //    //    cycleReduction = newCycleReduction;
            //    //} // +1 because a 2nd ship can have full firepower, but not the 3rd exceeding the other side
            //    //else
            //    //{
            //    //    cycleReduction = 1;
            //    //}
            //    List<string> ownCiv = _combatShips.Where(s =>
            //        (s.Item1.OwnerID == ownerIDs[0]))
            //        .Select(s => s.Item1.Owner.Key)
            //        .Distinct()
            //        .ToList();
            //    List<string> friendlyCivs = _combatShips.Where(s =>
            //        (s.Item1.OwnerID != ownerIDs[0]) &&
            //        CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShips[k].Item1.Owner))
            //        .Select(s => s.Item1.Owner.Key)
            //        .Distinct()
            //        .ToList();
            //    List<string> hostileCivs = new List<string>();
            //    var hostileShipList = _targetDictionary[ownerIDs[0]];
            //    foreach (var hostilShip in hostileShipList)
            //    {
            //        hostileCivs.Add(hostilShip.Item1.Owner.Key);
            //        hostileCivs.Distinct().ToList();
            //    }
            //    //var friendShips = _willFightAlongSide[ownerIDs[0]];
            //    //List<int> friendIDs = new List<int>();
            //    //foreach (var ship in friendShips)
            //    //{
            //    //    friendIDs.Add(ship.Item1.OwnerID);
            //    //}
            //    //if (_combatShips[k].Item1.OwnerID == ownerIDs[0] || friendIDs.Contains(_combatShips[k].Item1.OwnerID)) // need us or friendly
            //    //{
            //    //    friendlyOwner = true;
            //    //}
            //    //else
            //    //{
            //    //    friendlyOwner = false;
            //    //}
            //    #endregion
            //    #region calcuate firepower of civilizations
            //    int friendlyWeaponPower = ownCiv.Sum(e => _empireStrengths[e]) + friendlyCivs.Sum(e => _empireStrengths[e]);
            //    int hostileWeaponPower = hostileCivs.Sum(e => _empireStrengths[e]);
            //    int weaponRatio = friendlyWeaponPower * 10 / (hostileWeaponPower + 1);
            //    //Figure out if any of the opposition ships have sensors powerful enough to penetrate our camo. If so, will be decamo.
            //    if (_oppositionCombatShips.Count() > 0)
            //    {
            //        maxScanStrengthOpposition = _oppositionCombatShips.Max(s => s.Item1.Source.OrbitalDesign.ScanStrength);
            //        if (_combatShips[k].Item1.IsCamouflaged && _combatShips[k].Item1.CamouflagedStrength < maxScanStrengthOpposition)
            //        {
            //            _combatShips[k].Item1.Decamouflage();
            //            GameLog.Core.Combat.DebugFormat("{0} has camou strength {1} vs maxScan {2}",
            //                _combatShips[k].Item1.Name, _combatShips[k].Item1.CamouflagedStrength, maxScanStrengthOpposition);
            //        }
            //    }
            //    #endregion
            //    #region update diplomatic relations
            //    //TODO: Move this to DiplomacyHelper
            //    List<string> allEmpires = new List<string>();
            //    allEmpires.AddRange(ownCiv);
            //    allEmpires.AddRange(friendlyCivs);
            //    allEmpires.AddRange(hostileCivs);
            //    foreach (var firstEmpire in allEmpires.Distinct().ToList())
            //    {
            //        foreach (var secondEmpire in allEmpires.Distinct().ToList())
            //        {
            //            if (!DiplomacyHelper.IsContactMade(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire]))
            //            {
            //                DiplomacyHelper.EnsureContact(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire], _combatShips[0].Item1.Source.Location);
            //            }
            //        }
            //    }
            //    #endregion
            //    #region Top of Bonus damage for combat orders combinations  
            //    //Each ship by attacker order (switch) vs target order and find bonus damage
            //    //bool targetIsRushing = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Rush));
            //    //bool targetIsInFormation = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
            //    //bool targetIsHailing = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Hail));
            //    //bool targetIsRetreating = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Retreat));
            //    //bool targetIsRaidTransports = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Transports));
            //    //bool targetIsEngage = _oppositionCombatShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Engage));
            //    bool oppositionIsRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Rush));
            //    bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
            //    bool oppositionIsHailing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Hail));
            //    bool oppositionIsRetreating = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Retreat));
            //    bool oppositionIsRaidTransports = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Transports));
            //    bool oppositionIsEngage = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Engage));
            //    //var order = GetCombatOrder(_combatShips[k].Item1.Source);
            //    var attackOrder = GetCombatOrder(_combatShips[k].Item1.Source);
            //    var attackingShip = _combatShips[k].Item1;
            //    string attackingShipType = attackingShip.Source.OrbitalDesign.ShipType.ToString();
            //   //var targetShip = ChooseTarget(attackingShip);
            //   // string targetShipType = targetShip.Source.OrbitalDesign.ShipType.ToString();
            //   //var attackerManeuversToHullDamage = ((targetShip.Source.OrbitalDesign.HullStrength + 1) * attackingShip.Source.OrbitalDesign.Maneuverability / 32);// attacker ship maneuver values 1 to 8 (stations and OB = zero) so up to 25% 
            //   // var targetManeuversToHullDamage = ((attackingShip.Source.OrbitalDesign.HullStrength + 1) * targetShip.Source.OrbitalDesign.Maneuverability / 32); // target ship maneuver values 1 to 8 (stations and OB = zero) so up to 25% 
            //    int chanceRushingFormation = RandomHelper.Random(100);
            //    //var weapons = _combatShips[k].Item2.Where(w => w.CanFire); // attacking ships weapons
            //    //bool assimilationSuccessful = false;
            //    ////If the attacker is Borg, try and assimilate before you try destroying it
            //    //if (attackingShip.Owner.Name == "Borg" && targetShip.Owner.Name != "Borg")
            //    //{
            //    //    GameLog.Core.Combat.DebugFormat("{0} {1} attempting assimilation on {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, targetShip.Name, targetShip.Source.ObjectID);
            //    //    int chanceToAssimilate = RandomHelper.Random(100);
            //    //    assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
            //    //    if (assimilationSuccessful && (targetShip.Source is Ship))
            //    //    {
            //    //        Assimilate(targetShip, ownerAssets);
            //    //    }
            //    //}
            //switch (attackOrder)
            //{
            //    case CombatOrder.Engage:
            //    case CombatOrder.Rush:
            //    case CombatOrder.Transports:
            //    case CombatOrder.Formation:
            // Project multi: Overview
            // Description:
            // Initialize _combatShipsTemp with all _combatShips, initializing a variable with the current Firepower (total in the beginning)
            // Filling an array with the information about which empires participate and their targets. (maybe also include their combatOrder)
            // Ships fireing cycle though all the participating empires
            // Attacking ship fires all its weapons to a target ship (multiple ships if the attacker has enought weapons)
            // Then the other empire is immediatly fireing back, with up to the same amount of weapons (if possible) use multiple re-attacking ships, if you have to.
            // remember damage and the amount of weapons left.
            // Cycle though all the empires until all ships have fired. (maybe stopping it before that, so that some damaged ships remain). 
            // Also cycle though targets, e.g. if a empire targetes 2 empires, then fire at one empire in one round and at the next empire, the next round, alternating.
            // Project multi: Pseudo code (partly incomplete)
            //int[,] empiresInBattle; // An Array of who is in the battle.
            //empiresInBattle = new int[12, 3]; // an Array with 2 Dimensions. First with up to 12 elements, 2nd with up to 3 elements.
            // 12 Elements can hold 12 participating empires. 
            //empiresInBattle[0, 0] contains the CivID of the FirstPlayer
            //empiresInBattle[0, 1] contains the Target1 of that empire (As CivID as well)
            //empiresInBattle[0, 2] contains the Target2 of that empire.
            // Example:
            //empiresInBattle[0, 0] = 0; // Last 0 equals Federation
            //empiresInBattle[0, 1] = 2; // Last 2 equals Federation target Klingons
            //empiresInBattle[0, 2] = 1; // 1 equals Federation target Terrans, too
            //empiresInBattle[1, 0] = 1; // Last 1 Equals Terrans
            //empiresInBattle[1, 1] = 999; // Terran target = noone
            //empiresInBattle[1, 2] = 999; // Terran target Two = noone
            #endregion

            _combatShipsTemp = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            _combatShipsTemp.Clear(); // Initializing as nothing
            _combatShipsTemp = _combatShips; // filling with ALL participating ships
            if (_combatShipsTemp.Count() > 0)
                _combatShipsTemp.Randomize(); // Randomize ALL ships

            int indexOfAttackerEmpires = 0; // first position x on the array determins the empire who is currently firing. starting with index 0 (first player), [0,0] =which contains a civilization ID.
                                            //int 0 = 0; // 0 is the 2nd index on the array which contains targed one (on position 1) and target two (on position 0). 
            int TargetOneORTwo = 1; // starts with attacking first target
                                    //int shipFirepower = 0;
            int howOftenContinued = 0;
            
            GameLog.Core.Combat.DebugFormat("Main While is starting");

            #region top of Battle while loop to attacker while loop
            // ENTIRE BATTTLE
            // OVERALL LOOP
            // loops from one empire attacking (and recieving return fire) to the next, until all ships have fired
            int returnFireFirepower = 0; // Amount of Firepower the other Empire had. Its the base for return fire
            while (true)
            {
                returnFireFirepower = 0;
                
                if (TargetOneORTwo == 3) // if trying to attack target three (not available), target empire one again
                {
                    TargetOneORTwo = 1;
                }
                GameLog.Core.Combat.DebugFormat("Current Target One or Two? in Main While {0} ", TargetOneORTwo);
                int AttackingEmpireID = empiresInBattle[indexOfAttackerEmpires, 0];
                int targetedEmpireID = empiresInBattle[indexOfAttackerEmpires, 0 + TargetOneORTwo];
                int ReturnFireEmpire = targetedEmpireID;
                int EmpireAttackedInReturnFire = AttackingEmpireID;
                // Let Empire One (which is in empiresInBattle[0,0]) fire first
                // Search for the next fitting ship, that is of the targeted empire AND has Hull > 0
                
                var AttackingShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                    .Where(sc => sc.Item1.ReminingFirePower > 0).Select(sc => sc).ToList();
                var AttackingShip = AttackingShips.FirstOrDefault();
                if (AttackingShips != null)
                {
                    AttackingShip = AttackingShips.RandomElementOrDefault();
                    GameLog.Core.Combat.DebugFormat("Current Attacking Ship {0}", AttackingShip);
                }
                // COUNT ACTICE FIREROUND PER EMPIRE
                shipsPerEmpire[indexOfAttackerEmpires, 2] += 1;
                if (shipsPerEmpire[indexOfAttackerEmpires, 2] > 9)
                {
                    if (shipsPerEmpire[indexOfAttackerEmpires, 2] > shipsPerEmpire[indexOfAttackerEmpires, 1] / 2)
                    {
                        AttackingShip = null;
                    }
                }
                if (targetedEmpireID == 999 || targetedEmpireID == 888)
                {
                    AttackingShip = null; // refue to fire activly, if user / AI sais so
                }
                    GameLog.Core.Combat.DebugFormat("Index of current Attacker Empire {0}", indexOfAttackerEmpires);
                if (AttackingShip is null) // either because they cannot, or they refuse to fire activly.
                {
                    indexOfAttackerEmpires += 1; // give the next Empire a try
                    if(empiresInBattle[indexOfAttackerEmpires,0] == 999)
                    {
                        indexOfAttackerEmpires = 0; // change from empire 12 to 0 again
                        TargetOneORTwo += 1;
                    }
                    if (indexOfAttackerEmpires > 11)
                    {
                        indexOfAttackerEmpires = 0; // change from empire 12 to 0 again
                        TargetOneORTwo += 1;
                    }
                    howOftenContinued += 1; // counts how often we skipped fireing. If 12 times in a row, end Attacking Loop. 
                    if (howOftenContinued == 13)
                    {
                        returnFireFirepower = 0; //make sure there is no more retaliation either.
                        break;
                    }
                    else
                        continue;
                }
                else
                {
                    howOftenContinued = 0; 
                    
                    returnFireFirepower = AttackingShip.Item1.ReminingFirePower; // Tranfers Empire´s Attacking Ship Total Firepower to be the base for the other Empire return fire.
                }
                //END NEW123
                GameLog.Core.Combat.DebugFormat("Saved returnFirepower later used in next loop {0}", returnFireFirepower);
                double ScissorBonus = 0d; // This adds a bonus e.g. if a destroyer is firing on a command ship
                int remainingFirepowerInWhile = 0; // Counts if there is remaining firepower that would hit another ship, too.
                /// NEW123
                bool additionalRun = false; // addtional run  -> more targets
                // END NEW123
                // Attacking Ship looks for target(s)
                GameLog.Core.Combat.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts");
                #endregion

            #region attacker loop
                // HERE STARTS ATTACKER´S LOOP LOOKING FOR TARGETS
                while (true) // Attacking Ship looks for target(s) - all c# collections can be looped
                {
                    int rememberForDamage = 0;
                    if (targetedEmpireID == 999 || targetedEmpireID == 888)
                    {
                        GameLog.Core.Combat.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts BREAKS, becasue Human/AI has no target selected");
                        break; // refue to fire activly, if user / AI sais so
                    }
                    //var defenderOrder = CombatOrder.Retreat; // default order for now when 'target' is a dummy civilization
                    var currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                            .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                    var currentTarget = currentTargets.FirstOrDefault(); // Also make it distinct
                    if (currentTargets != null)
                    {
                        currentTarget = currentTargets.RandomElementOrDefault();
                    }
                    if (currentTarget == null)
                    {
                        if (_combatStation != null)
                        {
                            if (_combatStation.Item1.OwnerID == targetedEmpireID)
                            {
                                currentTarget = _combatStation;
                            }
                            else
                            {            // NO (MORE) TARGET. Save attackingships (remaining) Weapons
                                if (remainingFirepowerInWhile > 0)
                                {
                                    // let this AttackShip "RemainingFirepower" be returnFireFirepower
                                    AttackingShip.Item1.ReminingFirePower = remainingFirepowerInWhile;
                                    GameLog.Core.Combat.DebugFormat("No more target found in AttackingLoop. Trying to update for ship Name: {0} with remaining firepower = {1}", AttackingShip.Item1.Name, remainingFirepowerInWhile);
                                    break;
                                    // use Gamelog/test that ship needs to have reduced weapons in _combatShipsTemp
                                }
                                else
                                { 
                                    GameLog.Core.Combat.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts BREAKS because no target found");
                                    break; // VERY NEW
                                }
                            }
                        }
                    }
                        if(currentTarget is null)
                    {
                        GameLog.Core.Combat.DebugFormat("current Target is: (for Attacking loop) NONE, BREAK");
                        break;
                    }
                        else
                    {
                        GameLog.Core.Combat.DebugFormat("current Target is: (for Attacking loop){0}", currentTarget.Item1.Name);
                    }
                    var attackerOrder = GetCombatOrder(AttackingShip.Item1.Source);
                        var defenderOrder = GetCombatOrder(currentTarget.Item1.Source);
                        if (defenderOrder.ToString() == null || attackerOrder.ToString() == null)
                        {
                            GameLog.Core.Combat.DebugFormat("Warning. defender OR attackerOrder == null, in Attackerloop");
                        }
                        if ((_combatStation != null) && defenderOrder != CombatOrder.Formation) // Formation protects Starbase, otherwise ships are protected.
                        {
                            if (_combatStation.Item1.OwnerID == targetedEmpireID)
                            {
                                if (_combatStation.Item1.Source.HullStrength.CurrentValue > 0) // is this how to get int our of HullStrength Meter?
                                {
                                    currentTarget = _combatStation; // Station in _combatShips
                                }
                            }
                        }
                        GameLog.Core.Combat.DebugFormat("Still Attacking loop: Change target to (Station if station owner is not formation) {0}", currentTarget.Item1.Name);
                        // Calculate Bonus/Malus
                        // Get Accuracy, Damage Control when fixed
                       // double sourceAccuracyTemp = 1; // used to determin whether or not it is a hit
                        double sourceAccuracy = 1; // used to increase damage as well, if hero ship
                        double targetDamageControl = 0.5;
                        sourceAccuracy = AttackingShip.Item1.Source.GetAccuracyModifier();
                        if (sourceAccuracy > 1 || sourceAccuracy < 0.1)
                            sourceAccuracy = 1;
                        targetDamageControl = currentTarget.Item1.Source.GetDamageControlModifier();
                        if (targetDamageControl > 1 || targetDamageControl < 0.1)
                            targetDamageControl = 0.5;
                        // Hero Ship?
                        if (AttackingShip.Item1.Name.Contains("!"))
                        {
                            sourceAccuracy = 1.7; // change to 170% accuracy
                        }
                        // targed a Hero?                
                        if (currentTarget.Item1.Name.Contains("!"))
                        {
                            targetDamageControl = 1;
                        }
                        //  compare Orders
                        double combatOrderBonusMalus = 0;
                        // Engage
                        if (attackerOrder == CombatOrder.Engage && (defenderOrder == CombatOrder.Rush || defenderOrder == CombatOrder.Formation))
                            combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.ReminingFirePower * 0.08;
                        // RAID
                        if (attackerOrder == CombatOrder.Transports && defenderOrder != CombatOrder.Formation) // if Raid, and no Formation select Transportships to be targeted
                        {
                            combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.ReminingFirePower * 0.02;
                            currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0)
                                .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Transport"))
                                .Select(sc => sc).ToList();
                            currentTarget = currentTargets.RandomElementOrDefault();
                            if (currentTarget is null)
                            {
                                currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                    .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                                currentTarget = currentTargets.RandomElementOrDefault();
                            }
                            if (attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Engage)
                                combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.ReminingFirePower * 0.08; // even more weapon Bonus if defender is Engaging
                        }
                        else if ((attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Formation && _combatStation is null)) // IF Raiding and Defender is doing combat Formating, let Frigates protect Transports
                        {
                            currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0)
                                .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Frigate"))
                                .Select(sc => sc).ToList();
                            currentTarget = currentTargets.RandomElementOrDefault();
                            if (currentTarget is null) // If no Frigates, target Transports
                            {
                                currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0)
                                .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Transport"))
                                .Select(sc => sc).ToList();
                                currentTarget = currentTargets.RandomElementOrDefault();
                                if (currentTarget is null) // If no Transports, target anyone
                                {
                                    currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                    .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                                }
                                currentTarget = currentTargets.RandomElementOrDefault();
                            }
                        }
                        // Rush
                        if (attackerOrder == CombatOrder.Rush && (defenderOrder == CombatOrder.Retreat || defenderOrder == CombatOrder.Transports))
                            combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.ReminingFirePower * 0.10;
                        // Formation
                        if (attackerOrder == CombatOrder.Formation && (defenderOrder == CombatOrder.Transports || defenderOrder == CombatOrder.Rush))
                            combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.ReminingFirePower * 0.12;
                        Convert.ToInt32(combatOrderBonusMalus);
                        // Determin ScissorBonus depending on both ship types
                        if (
                        ((AttackingShip.Item1.Source.Design.Key.Contains("CRUISER") || AttackingShip.Item1.Source.Design.Key.Contains("SPHERE")) && !AttackingShip.Item1.Source.Design.Key.Contains("STRIKE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("DESTROYER") || currentTarget.Item1.Source.Design.Key.Contains("FRIGATE") || currentTarget.Item1.Source.Design.Key.Contains("PROBE"))
                        ||
                        ((AttackingShip.Item1.Source.Design.Key.Contains("DESTROYER") || AttackingShip.Item1.Source.Design.Key.Contains("FRIGATE") || AttackingShip.Item1.Source.Design.Key.Contains("PROBE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("COMMAND") || currentTarget.Item1.Source.Design.Key.Contains("BATTLESHIP") || currentTarget.Item1.Source.Design.Key.Contains("CUBE")))
                        ||
                        ((AttackingShip.Item1.Source.Design.Key.Contains("COMMAND") || AttackingShip.Item1.Source.Design.Key.Contains("BATTLESHIP") || AttackingShip.Item1.Source.Design.Key.Contains("CUBE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("CRUISER") || AttackingShip.Item1.Source.Design.Key.Contains("SPHERE")) && !currentTarget.Item1.Source.Design.Key.Contains("STRIKE"))
                        ||
                        (!currentTarget.Item1.Source.Design.Key.Contains("CRUISER")
                        && !currentTarget.Item1.Source.Design.Key.Contains("COMMAND")
                            && !currentTarget.Item1.Source.Design.Key.Contains("BATTLESHIP")
                            && !currentTarget.Item1.Source.Design.Key.Contains("DESTROYER")
                            && !currentTarget.Item1.Source.Design.Key.Contains("FRIGATE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("CUBE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("SPHERE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("PROBE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("DIAMOND"))
                                                            )
                            ScissorBonus = AttackingShip.Item1.ReminingFirePower * 0.2; // 20 % Scissor Bonus
                        // BOnus/Malus is applied to damage sum
                        GameLog.Core.Combat.DebugFormat("follogwing Bonus/Malus a) due to Order: = {0}, b) due to Scissor = {1}", combatOrderBonusMalus, ScissorBonus);
                        // Do we have more Weapons then target has shields? FirepowerRemains... /// NEW123 added combatOrderBonusMallus and other changes // Maneuverability 8 = 33% more shields. 1 = 4% more shields
                        int check = currentTarget.Item1.Source.GetManeuverablility(); // allows to check if maneuverability is gotten correctly
                        if (additionalRun) // if its a  new run use remainingFirepowerinWhile insstead
                        {
                            GameLog.Core.Combat.DebugFormat("We are in an addtiona´run (next target for attacking loop)");
                            // And new target can now aborb damage
                            if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                            remainingFirepowerInWhile) // if remainingFirepower is absorbed by targets Hull/shields/Maneuverability, set it to -1 and discharge weapons.
                            {
                                GameLog.Core.Combat.DebugFormat("this time the target absorbt all weapons");
                                remainingFirepowerInWhile = -1;
                                foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                                {
                                    weapon.Discharge();
                                }
                                AttackingShip.Item1.ReminingFirePower = 0;
                            } // Otherwise we have yet another run with remainingFirepowerinWhile
                            else
                            {
                                remainingFirepowerInWhile = remainingFirepowerInWhile
                                  - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24)) / 100);
                            }
                            // Otherwise we still have remainingFirepower The no -1 means we will get an addtional run
                        }
                        else
                        {
                            // If first run and target can absorb full damage
                            if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                                AttackingShip.Item1.ReminingFirePower * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus))
                            {
                                rememberForDamage = remainingFirepowerInWhile;
                                remainingFirepowerInWhile = -1;
                                GameLog.Core.Combat.DebugFormat("its the run on the first target and it can already absorb all weapons");
                                foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire)) // Discharge Weapons
                                {
                                    weapon.Discharge();
                                }
                            }
                            else
                            {
                                remainingFirepowerInWhile = Convert.ToInt32(AttackingShip.Item1.ReminingFirePower * sourceAccuracy) + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)
                                        - (currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength);
                                GameLog.Core.Combat.DebugFormat("its the first run on a target, an weapons remain RemainingFirepowerInWhile == {0}", remainingFirepowerInWhile);
                            }
                        }
                        // Fire Weapons, inflict damage. Either with all calculated bonus/malus. Or if this was done last turn, use remainingFirepower (if any)
                        if (remainingFirepowerInWhile == -1)
                        {
                            currentTarget.Item1.TakeDamage((int)(rememberForDamage));
                        }
                        else
                        {
                            currentTarget.Item1.TakeDamage((int)(AttackingShip.Item1.ReminingFirePower
                                * Convert.ToInt32(1.5 - targetDamageControl) * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus))); // minimal damage of 50 included
                        }
                        GameLog.Core.Combat.DebugFormat("now damage has just been applies either remainingFirepowerinWhile when addtional run {0} OR full shipsFirepower + Bonuses {1}", rememberForDamage, AttackingShip.Item1.ReminingFirePower);
                        // Gamelog
                        //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), DamageControl = {6}, Shields = {7}, Hull = {8}",
                        //    target.Source.ObjectID, target.Name, target.Source.Design,
                        //    (int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10),
                        //    cycleReduction,
                        //    sourceAccuracy,
                        //    targetDamageControl,
                        //    target.ShieldStrength,
                        //    target.HullStrength
                        //    //    );
                        //}
                        ////weapon.Discharge(); needed yes or no?
                        //END NEW123
                        if (remainingFirepowerInWhile == -1)
                        {
                            GameLog.Core.Combat.DebugFormat("No more weapons on the attacking ship (loop), so no more run, break");
                            // Set AttackingShips TotalWeapons to 0
                            //NEW123
                            additionalRun = false; // Remebers if next run is FirstTargetRun OR...
                            break;
                        }// break the while, we do not need more targets for this AttackingShip
                        else
                        { // More Weapons available, continue for more targets 
                            //if (remainingFirepowerInWhile > 0)
                            //    {
                            //        // let this AttackShip "RemainingFirepower" be returnFireFirepower
                            //        AttackingShip.Item1.ReminingFirePower = remainingFirepowerInWhile;
                            //          GameLog.Core.Combat.DebugFormat("Trying to update for ship Name: {0} with remaining firepower = {1}", AttackingShip.Item1.Name, remainingFirepowerInWhile);
                            //        // use Gamelog/test that ship needs to have reduced weapons in _combatShipsTemp
                            //         break;
                            //    }
                            GameLog.Core.Combat.DebugFormat("Attacker has more weapons, an additional run is done to get more targets: {0}", remainingFirepowerInWhile);
                            additionalRun = true; // Remembers if next run is an addtional Target Run.
                            // set AttackingShips TotalWeapons to remainingFirepower. Loop again
                        }
                }
                //..... END OF ATTACKING WHILE...
                #endregion
                // this while loop will fire on as many targets as nessecary to discharge attackingShips weapons fully
                //END OF ATTACKING WHILE

                // Re-Initilazing start Variables for retaliation while
                additionalRun = false; // If target could not absorb full weapons. If True, it means that we are on the next target
                ScissorBonus = 0D;
                remainingFirepowerInWhile = 0;
               
                targetedEmpireID = EmpireAttackedInReturnFire; // The guy who attacked is now the target
                AttackingEmpireID = ReturnFireEmpire; // Now the empire that was the targed will return fire
                bool needAdditionalAttackingShip = true; // get at least one attacking ship
                int applyDamage = 0; 

                #region returning fire loop
                // Here comes the next WHILE
                // HERE STARTS RETURNING FIRE LOOP
                // Now the attacked Empire returns fire, until same damage is dealed to attacking empire´s ship(s)
                GameLog.Core.Combat.DebugFormat("Here starts RETURN FIRE LOOP");
                while (true && returnFireFirepower > 0)
                {
                    GameLog.Core.Combat.DebugFormat("Loop for finding an Target(s) for Attacking Ship HAS STARTED (AGAIN)");
                    if (needAdditionalAttackingShip)
                    {
                        AttackingShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                        .Where(sc => sc.Item1.FirePower > 0).Select(sc => sc).ToList();
                        if (AttackingShips != null)
                        {
                            AttackingShip = AttackingShips.RandomElementOrDefault();
                        }
                        if (AttackingShip is null)
                        {
                            returnFireFirepower = 0;
                            break; // stopp all return fire if we don´t have ships/stations to fire
                        }
                    }
                   
                    GameLog.Core.Combat.DebugFormat("First Attacking Ship for return fire found {0}", AttackingShip.Item1.Name);
                    if (returnFireFirepower < AttackingShip.Item1.ReminingFirePower) // If AttackingShip can supply the required Weapons, we don´t need another attacking ship
                    {
                        needAdditionalAttackingShip = false;
                        applyDamage = returnFireFirepower;
                        returnFireFirepower = 0;
                        GameLog.Core.Combat.DebugFormat("First Attacking Ship has enought weapons to fully retaliate {0}", AttackingShip.Item1.Name);
                    }
                    else // we need another attacking ship, later, for the remaining returnFireFirepower
                    {
                        needAdditionalAttackingShip = true;
                        applyDamage = AttackingShip.Item1.ReminingFirePower;
                        returnFireFirepower = returnFireFirepower - applyDamage;
                        GameLog.Core.Combat.DebugFormat("Need more ships to apply full retailiation firepower: firepower left: {0}, applied first: {1}", returnFireFirepower, applyDamage);
                    }
                    // Getting a target // HEREX
                    var currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                    var currentTarget = currentTargets.RandomElementOrDefault(); // Also make it distinct
                    if (currentTarget != null)
                        GameLog.Core.Combat.DebugFormat("We found the target ship: {0} to retaliate", currentTarget.Item1.Name);
                    if (currentTarget == null)
                    {
                        if (_combatStation != null)
                            if (_combatStation.Item1.OwnerID == targetedEmpireID)
                            {
                                currentTarget = _combatStation;
                            }
                            else // VERY NEW
                            {
                                if (returnFireFirepower > 0)
                                {
                                    // let this AttackShip "RemainingFirepower" be returnFireFirepower
                                    AttackingShip.Item1.ReminingFirePower = returnFireFirepower;
                                    GameLog.Core.Combat.DebugFormat("We have no more targets, but still firepower to retaliate =  {0}, save it to Ship {1} BREAK", returnFireFirepower, AttackingShip.Item1.Name);
                                    // use Gamelog/test that ship needs to have reduced weapons in _combatShipsTemp
                                    break;
                                }
                                else
                                {
                                    GameLog.Core.Combat.DebugFormat("We have no more targets, AND no more firepower to retaliate =  {0} BREAK", returnFireFirepower);
                                    break;
                                }
                            }
                    }
                    else
                    {
                        GameLog.Core.Combat.DebugFormat("Found a target  {0}", currentTarget.Item1.Name);
                    }
                    // Prepare and apply Bonuses/Maluses
                        
                        var attackerOrder = GetCombatOrder(AttackingShip.Item1.Source);
                    if (currentTarget is null)
                    {
                        GameLog.Core.Combat.DebugFormat("Warning. no target for RETALIATIONloop. BREAK");
                        break;
                    }
                    var defenderOrder = GetCombatOrder(currentTarget.Item1.Source);
                        
                        if ((_combatStation != null) && defenderOrder != CombatOrder.Formation) // Formation protects Starbase, otherwise ships are protected.
                        {
                            if (_combatStation.Item1.Source.HullStrength.CurrentValue > 0 && _combatStation.Item1.OwnerID == targetedEmpireID)
                            {
                                currentTarget = _combatStation;
                            }
                        }
                    
                    // Calculate Bonus/Malus
                    // Get Accuracy, Damage Control when fixed
                    //double sourceAccuracyTemp = 1; // used to determin whether or not it is a hit
                    double sourceAccuracy = 1; // used to increase damage as well, if hero ship
                    double targetDamageControl = 0.5;
                    sourceAccuracy = AttackingShip.Item1.Source.GetAccuracyModifier();
                    if (sourceAccuracy > 1 || sourceAccuracy < 0.1)
                        sourceAccuracy = 1;
                    targetDamageControl = currentTarget.Item1.Source.GetDamageControlModifier();
                    if (targetDamageControl > 1 || targetDamageControl < 0.1)
                        targetDamageControl = 0.5;
                    // Hero Ship?
                    if (AttackingShip.Item1.Name.Contains("!"))
                    {
                        sourceAccuracy = 1.7; // change to 170% accuracy
                    }
                    // targed a Hero?                
                    if (currentTarget.Item1.Name.Contains("!"))
                    {
                        targetDamageControl = 1;
                    }
                    double combatOrderBonusMalus = 0;
                    // Engage rush formation
                    if (attackerOrder == CombatOrder.Engage && (defenderOrder == CombatOrder.Rush || defenderOrder == CombatOrder.Formation))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.08;
                    // RAID Transports
                    if (attackerOrder == CombatOrder.Transports && defenderOrder != CombatOrder.Formation) // if Raid, and no Formation select Transportships to be targeted
                    {
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.02;
                        currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Transport"))
                            .Select(sc => sc).ToList();
                        if (currentTargets != null)
                        {
                            currentTarget = currentTargets.RandomElementOrDefault();
                        }
                        if (currentTarget is null)
                        {
                            currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                            currentTarget = currentTargets.RandomElementOrDefault();
                        }
                        if (attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Engage)
                            combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.08; // even more weapon Bonus if defender is Engaging
                    }
                    else if ((attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Formation && _combatStation is null)) // IF Raiding and Defender is doing combat Formating, let Frigates protect Transports
                    {
                        // HEREX
                        currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Frigate"))
                            .Select(sc => sc).ToList();
                        if (currentTargets != null)
                        {
                            currentTarget = currentTargets.RandomElementOrDefault();
                        }
                        if (currentTarget is null) // If no Frigates, target Transports
                        {
                            currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Transport"))
                            .Select(sc => sc).ToList();
                            if (currentTargets != null)
                            {
                                currentTarget = currentTargets.RandomElementOrDefault();
                            }
                            if (currentTarget is null) // If no Transports, target anyone
                            {
                                currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                                currentTarget = currentTargets.RandomElementOrDefault();
                            }
                        }
                    }
                    // Rush
                    if (attackerOrder == CombatOrder.Rush && (defenderOrder == CombatOrder.Retreat || defenderOrder == CombatOrder.Transports))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.10;
                    // Formation
                    if (attackerOrder == CombatOrder.Formation && (defenderOrder == CombatOrder.Transports || defenderOrder == CombatOrder.Rush))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.12;
                    Convert.ToInt32(combatOrderBonusMalus);
                    // Determin ScissorBonus depending on both ship types
                    if (
                        ((AttackingShip.Item1.Source.Design.Key.Contains("CRUISER") || AttackingShip.Item1.Source.Design.Key.Contains("SPHERE")) && !AttackingShip.Item1.Source.Design.Key.Contains("STRIKE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("DESTROYER") || currentTarget.Item1.Source.Design.Key.Contains("FRIGATE") || currentTarget.Item1.Source.Design.Key.Contains("PROBE"))
                        ||
                        ((AttackingShip.Item1.Source.Design.Key.Contains("DESTROYER") || AttackingShip.Item1.Source.Design.Key.Contains("FRIGATE") || AttackingShip.Item1.Source.Design.Key.Contains("PROBE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("COMMAND") || currentTarget.Item1.Source.Design.Key.Contains("BATTLESHIP") || currentTarget.Item1.Source.Design.Key.Contains("CUBE")))
                        ||
                        ((AttackingShip.Item1.Source.Design.Key.Contains("COMMAND") || AttackingShip.Item1.Source.Design.Key.Contains("BATTLESHIP") || AttackingShip.Item1.Source.Design.Key.Contains("CUBE"))
                        && (currentTarget.Item1.Source.Design.Key.Contains("CRUISER") || AttackingShip.Item1.Source.Design.Key.Contains("SPHERE")) && !currentTarget.Item1.Source.Design.Key.Contains("STRIKE"))
                        ||
                        (!currentTarget.Item1.Source.Design.Key.Contains("CRUISER")
                        && !currentTarget.Item1.Source.Design.Key.Contains("COMMAND")
                            && !currentTarget.Item1.Source.Design.Key.Contains("BATTLESHIP")
                            && !currentTarget.Item1.Source.Design.Key.Contains("DESTROYER")
                            && !currentTarget.Item1.Source.Design.Key.Contains("FRIGATE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("CUBE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("SPHERE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("PROBE")
                            && !currentTarget.Item1.Source.Design.Key.Contains("DIAMOND"))
                                                            )
                        ScissorBonus = applyDamage * 0.2; // 20 % Scissor Bonus
                    // We have now calculated all bonuses/maluses
                    GameLog.Core.Combat.DebugFormat("added bonuses to retailiation firepower. OrderBonus = {0}, ScissorBonus = {1}", combatOrderBonusMalus, ScissorBonus);
                    // DO I USE remainingFirePowerinWHile OR applyDamage
                    // Do we have more Weapons then target has shields? FirepowerRemains... /// NEW123 added combatOrderBonusMallus and other changes // Maneuverability 8 = 33% more shields. 1 = 4% more shields
                    int check = currentTarget.Item1.Source.GetManeuverablility(); // allows to check if maneuverability is gotten correctly
                    if (additionalRun) // If True, it means that we are on the next target
                    {
                        if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                        applyDamage) // if remainingFirepower is absorbed by targets Hull/shields/Maneuverability, set it to -1 and discharge weapons.
                        {
                            GameLog.Core.Combat.DebugFormat("it was an additional reteliation run, weapons now fully applied");
                            remainingFirepowerInWhile = -1;
                            foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                            {
                                weapon.Discharge();
                            }
                            AttackingShip.Item1.ReminingFirePower = 0;
                            // SETTING SHIP WEAPONS TO 0
                            foreach (var ship in _combatShipsTemp)
                            {
                                //if(ship.Item1.Source.ObjectID == AttackingShip.Item1.Source.ObjectID)
                                //    _combatShipsTemp.
                            }
                        }
                        else
                        {
                            remainingFirepowerInWhile = applyDamage
                                                - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24)) / 100);
                            GameLog.Core.Combat.DebugFormat("it was an addtional run, we still have firepower =  {0}", remainingFirepowerInWhile);
                            // Otherwise we still have remainingFirepower
                        }
                    }
                    if (!additionalRun) // If false, it means that we are on first target
                    {
                        if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                            applyDamage * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus))
                        {
                            remainingFirepowerInWhile = -1;
                            foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                            {
                                weapon.Discharge();
                            }
                            AttackingShip.Item1.ReminingFirePower = 0;
                            GameLog.Core.Combat.DebugFormat("Retailiation completed in first run");
                        }
                        else
                        {
                            remainingFirepowerInWhile = Convert.ToInt32(applyDamage * sourceAccuracy) + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)
                                                - (currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength);
                            GameLog.Core.Combat.DebugFormat("First Retailiation run on target, weapons = {0} reaim", remainingFirepowerInWhile);
                        }
                    }
                    /// APPLY DAMAGE IN RETALIATION
                    // Fire Weapons, inflict damage. Either with all calculated bonus/malus. Or if this was done last turn, use remainingFirepower (if any)
                    if (additionalRun)
                    { // APPLY DAMAGE
                        currentTarget.Item1.TakeDamage((int)(remainingFirepowerInWhile));
                    }
                    else
                    { // APPLY DAMAGE // HEREX
                        currentTarget.Item1.TakeDamage((int)(applyDamage
                            * Convert.ToInt32(1.5 - targetDamageControl) * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus))); // minimal damage of 50 included
                    }
                    GameLog.Core.Combat.DebugFormat("target ship: {0} suffered damage: {1} has hull left: {2}", currentTarget.Item1.Name, Convert.ToInt32(applyDamage * Convert.ToInt32(1.5 - targetDamageControl) * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)), currentTarget.Item1.HullStrength);
                    GameLog.Core.Combat.DebugFormat("Retailiation damage of this round, now has been applied additional run: {0}, OR first run: {1} + Bonuse", remainingFirepowerInWhile, applyDamage);
                    // Gamelog
                    //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), DamageControl = {6}, Shields = {7}, Hull = {8}",
                    //    target.Source.ObjectID, target.Name, target.Source.Design,
                    //    (int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10),
                    //    cycleReduction,
                    //    sourceAccuracy,
                    //    targetDamageControl,
                    //    target.ShieldStrength,
                    //    target.HullStrength
                    //    //    );
                    //}
                    ////weapon.Discharge(); needed yes or no?
                    if (remainingFirepowerInWhile == -1)
                    {
                        // Set AttackingShips TotalWeapons to 0
                        additionalRun = false;
                        if (returnFireFirepower == 0)
                        {
                            GameLog.Core.Combat.DebugFormat("Retailiation complete. No more firepower. Break");
                            // Gamelog
                            break;
                        }
                    }// break the while, we do not need more targets for this AttackingShip
                    else
                    {
                        GameLog.Core.Combat.DebugFormat("One more retaliation run, because we still have weapons retunrFireFirepower = {0}", returnFireFirepower);
                        // Gamelog
                        additionalRun = true;
                        // set AttackingShips TotalWeapons to remainingFirepower. Loop again
                    }
                }
                #endregion
                // End Return fire

                #region end of combat now  house keeping
                GameLog.Core.Combat.DebugFormat("IndexofAttackerEmpire = {0}", indexOfAttackerEmpires);
                indexOfAttackerEmpires = indexOfAttackerEmpires + 1; // The next Empire in the Array gets its shot in the next whileloop
                GameLog.Core.Combat.DebugFormat("IndexOfAttackerEmpire now = {0}", indexOfAttackerEmpires);
                if (indexOfAttackerEmpires > 11)
                {
                    indexOfAttackerEmpires = 0;
                    TargetOneORTwo = TargetOneORTwo + 1; // cycle to next targeted empire
                    GameLog.Core.Combat.DebugFormat("Current Empire about to , return to 0, because last empire was 12"); 
                }
                    // Gamelog
                // Once all empires have fired once, the first empire fires again
                GameLog.Core.Combat.DebugFormat("Current Empire about to fire: {0}", empiresInBattle[indexOfAttackerEmpires, 0]);
                //if (empiresInBattle[indexOfAttackerEmpires, 0] == 999)
                //{
                //    indexOfAttackerEmpires = 0;
                //    GameLog.Core.Combat.DebugFormat("Current Empire about to , retarted to 0, because last empire was 999");
                //    TargetOneORTwo = TargetOneORTwo + 1; // cycle to next targeted empire
                //}
                //foreach (var combatent in _combatShipsTemp) // now search for destroyed ships
                //{
                //    if (combatent.Item1.IsDestroyed)
                //    {

                //        var Assets = GetAssets(combatent.Item1.Owner);
                //        GameLog.Core.Combat.DebugFormat("Opposition {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
                //        if (combatent.Item1.Source is Ship)
                //        {
                //            if (Assets != null)
                //            {
                //                GameLog.Core.Combat.DebugFormat("Name of Owner = {0}, Assets.CombatShips{1}, Assets.NonCobatShips{2}", Assets.Owner.Name, Assets.CombatShips.Count, Assets.NonCombatShips.Count);
                //                if (!Assets.DestroyedShips.Contains(combatent.Item1))
                //                {
                //                    Assets.DestroyedShips.Add(combatent.Item1);
                //                }
                //                if (combatent.Item1.Source.IsCombatant)
                //                {
                //                    Assets.CombatShips.Remove(combatent.Item1);
                //                }
                //                else
                //                {
                //                    Assets.NonCombatShips.Remove(combatent.Item1);
                //                }
                //            }
                //            else
                //                GameLog.Core.Combat.DebugFormat("Assets Null");
                //        }
                //        else if (_combatShips.Contains(_combatStation))
                //        {
                //            if (!Assets.DestroyedShips.Contains(combatent.Item1))
                //            {
                //                Assets.DestroyedShips.Add(combatent.Item1);
                //            }
                //        }
                //        continue;
                //    }
                //}

                ////End the combat... at turn X = 5, by letting all sides reteat
                //if (true) // End Combat after 3 While loops
                //{
                //    _roundNumber += 1;
                //    // _combatShips = _combatShipsTemp;
                //    // NEW123 CHANGED TO TEMP, does this work?
                //    var allRetreatShips = _combatShipsTemp // All non destroyed ships retreat (survive)
                //        .Where(s => !s.Item1.IsDestroyed)
                //        .Where(s => s.Item1.Owner != s.Item1.Source.Sector.Owner) // Ships in own territory make a stand (remain in the system they own), after 5 turns.
                //        .ToList();
                //    foreach (var ship in allRetreatShips)
                //    {
                //        if (ship.Item1 != null)
                //        {
                //            var ownerAssets = GetAssets(ship.Item1.Owner);
                //            if (!ownerAssets.EscapedShips.Contains(ship.Item1))
                //            {
                //                ownerAssets.EscapedShips.Add(ship.Item1);
                //                ownerAssets.CombatShips.Remove(ship.Item1);
                //                ownerAssets.NonCombatShips.Remove(ship.Item1);
                //                _combatShips.Remove(ship);
                //            }

                //        }
                //    }
                //}
                //break;
                // NEXT EMPIRE
                // Once no more ships available, close loop
                // Update _combatShips to current _combatShipsTemp
                // Investigate how and where "friendlyships" etc. are used to display remaining ships fitting to the screen
                // FINISH BATTLE destroy ships/stations
                GameLog.Core.Combat.DebugFormat("THE ENTIRE BATTLE WAS FULLY COMPLETED. May need to remove destroyed ships");
                #region Older combat code
                //    //var attackingShip = _combatShips[k].Item1;
                //    var target = ChooseTarget(attackingShip); // CHOOSE TARGET
                //    if (order != CombatOrder.Formation && order != CombatOrder.Engage)// so maneuverable _combatShips[k] has order Rush or Transports target takes damage
                //    {
                //        var maneuver = attackingShip.Source.OrbitalDesign.Maneuverability;// ship maneuver values 1 to 8 (stations and OB = zero)
                //        //target.TakeDamage((target.Source.OrbitalDesign.HullStrength +1) * (maneuver/32)+1); // max possible hull damage of 25%
                //        GameLog.Core.Combat.DebugFormat("({2}) {0} {1}: new hull strength {3}, took damage {4} due to Maneuverability {5} from ({8}) {6} {7}",
                //            target.Source.ObjectID, target.Source.Name, target.Source.Design,
                //            target.Source.OrbitalDesign.HullStrength,
                //            (target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1,
                //            maneuver,
                //            attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.Design
                //            );
                //        target.TakeDamage((target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1); // INFLICT HULL DAMAGE, max possible hull damage of 25%
                //    }
                //    if (target == null)
                //    {
                //        GameLog.Core.Combat.DebugFormat("No target for {0} {1} ({2})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design);
                //    }
                //    if (target != null)
                //    {
                //        GameLog.Core.Combat.DebugFormat("Target for {0} {1} ({2}) is {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);
                //        // If we rush or attack transports on a formation we could take damage
                //        //int chanceRushingFormation = RandomHelper.Random(100);
                //        if (oppositionIsInFormation && (order == CombatOrder.Rush || order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                //        {
                //            attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 4);  // 25 % down out of Hullstrength of TechObjectDatabase.xml
                //            GameLog.Core.Combat.DebugFormat("{0} {1} rushed or raid transports in formation and took {2} damage to hull ({3} hull left)",
                //                attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                //        }
                //        if (oppositionIsEngage && (order == CombatOrder.Formation || order == CombatOrder.Rush) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                //        {
                //            attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml
                //            GameLog.Core.Combat.DebugFormat("{0} {1} in Formation or Rushing while Engaged and took {2} damage to hull ({3} hull left)",
                //                attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                //        }
                //        if (oppositionIsRushing && (order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                //        {
                //            attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml
                //            GameLog.Core.Combat.DebugFormat("{0} {1} Raiding Transports and got Rushed took {2} damage to hull ({3} hull left)",
                //                attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                //        }
                //        if (oppositionIsRaidTransports && (order == CombatOrder.Engage) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
                //        {
                //            attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 6);  // 17 % down out of Hullstrength of TechObjectDatabase.xml
                //            GameLog.Core.Combat.DebugFormat("{0} {1} Engag order got Raided and took {2} damage to hull ({3} hull left)",
                //                attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
                //        }
                //        #endregion
                //        #region assimilation
                //        bool assimilationSuccessful = false;
                //        //If the attacker is Borg, try and assimilate before you try destroying it
                //        if (attackingShip.Owner.Name == "Borg" && target.Owner.Name != "Borg")
                //        {
                //            GameLog.Core.Combat.DebugFormat("{0} {1} attempting assimilation on {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
                //            int chanceToAssimilate = RandomHelper.Random(100);
                //            assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                //        }
                //        //Perform the assimilation, but only on ships
                //        if (assimilationSuccessful && target.Source is Ship)
                //        {
                //            GameLog.Core.Combat.DebugFormat("{0} {1} successfully assimilated {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
                //            CombatAssets oppositionAssets = GetAssets(target.Owner);
                //            if (!ownerAssets.AssimilatedShips.Contains(target))
                //            {
                //                ownerAssets.AssimilatedShips.Add(target);
                //                target.IsAssimilated = true;
                //            }
                //            if (target.Source.IsCombatant)
                //            {
                //                oppositionAssets.CombatShips.Remove(target);
                //            }
                //            else
                //            {
                //                oppositionAssets.NonCombatShips.Remove(target);
                //            }
                //            #endregion
                //        }
                //        #region performAttack in order switch
                //        else // if not assmilated attack, perform attack fire weapons
                //        {
                //            //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);
                //            var weapons = _combatShips[k].Item2.Where(w => w.CanFire);
                //            int amountOfWeapons = weapons.Count();
                //            //var partlyFiring = 0;
                //            foreach (var weapon in _combatShips[k].Item2.Where(w => w.CanFire))
                //            {
                //                if (!target.IsDestroyed)  //&& !target.) // Bug?: do not target retreated ships
                //                {
                //                    PerformAttack(attackingShip, target, weapon);
                //                }
                //            }
                //            GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5}), amountOfWeapons = {6}",
                //                attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design,
                //                target.Source.ObjectID, target.Name, target.Source.Design,
                //                amountOfWeapons);
                //            // all weapons fired for current ship k
                //        }
                //        #endregion
                //    }
                //    foreach (var combatShip in _combatShips)
                //    {
                //        if (combatShip.Item1.IsDestroyed)
                //        {
                //            ownerAssets.AssimilatedShips.Remove(target);
                //        }
                //    }
                //    break;
                #endregion
                #region old switch Retreat order to break order switch
                //case CombatOrder.Retreat:
                //    if (WasRetreateSuccessful(_combatShips[k].Item1, oppositionIsRushing, oppositionIsEngage, oppositionIsInFormation, oppositionIsHailing, oppositionIsRetreating, oppositionIsRaidTransports, weaponRatio))
                //    {
                //        try
                //        {
                //            // added destroyed ship cannot retreat
                //            if (!ownerAssets.EscapedShips.Contains(_combatShips[k].Item1) && !_combatShips[k].Item1.IsDestroyed)
                //            {
                //                ownerAssets.EscapedShips.Add(_combatShips[k].Item1);
                //            }
                //            if (_combatShips[k].Item1.Source.IsCombatant)
                //            {
                //                ownerAssets.CombatShips.Remove(_combatShips[k].Item1);
                //            }
                //            else
                //            {
                //                ownerAssets.NonCombatShips.Remove(_combatShips[k].Item1);
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            GameLog.Core.Combat.DebugFormat("Exception e {0} ship {1} {2} {3}", e, _combatShips[k].Item1.Source.Design, _combatShips[k].Item1.Source.Name, _combatShips[k].Item1.Source.ObjectID);
                //        }
                //        _combatShips.Remove(_combatShips[k]);
                //    }
                //    // Chance of hull damage if you fail to retreat and are being rushed
                //    else if (oppositionIsRushing && (weaponRatio > 1))
                //    {
                //        _combatShips[k].Item1.TakeDamage(_combatShips[k].Item1.Source.OrbitalDesign.HullStrength / 2);  // 50 % down out of Hullstrength of TechObjectDatabase.xml
                //        GameLog.Core.Combat.DebugFormat("{0} {1} failed to retreat whilst being rushed and took {2} damage to hull ({3} hull left)",
                //            _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Name, _combatShips[k].Item1.Source.OrbitalDesign.HullStrength / 2, _combatShips[k].Item1.Source.HullStrength);
                //    }
                //    break;
                //case CombatOrder.Hail:
                //    GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) hailing...", _combatShips[k].Item1.Name, _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Design.Name);
                //    break;
                //case CombatOrder.Standby:
                //    GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) standing by...", _combatShips[k].Item1.Name, _combatShips[k].Item1.Source.ObjectID, _combatShips[k].Item1.Source.Design.Name);
                //    break;
                #endregion
                // IN here is my code (while x3)
                #region Station attack 
                //Make sure that the station has a go at the enemy too
                //if ((_combatStation != null) && !_combatStation.Item1.IsDestroyed)
                //{
                //    var order = GetCombatOrder(_combatStation.Item1.Source);
                //    CombatUnit target = null;
                //    switch (order)
                //    {
                //        case CombatOrder.Engage:
                //        case CombatOrder.Rush:
                //        case CombatOrder.Transports:
                //        case CombatOrder.Formation:
                //            target = ChooseTarget(_combatStation.Item1);
                //            break;
                //    }
                //    if (target != null)
                //    {
                //        foreach (CombatWeapon weapon in _combatStation.Item2.Where(w => w.CanFire))
                //        {
                //            PerformAttack(_combatStation.Item1, target, weapon);
                //        }
                //    }
                //}
                #endregion
                #region old remove ships and retreat
                // remove desroyed ships. Now on this spot, so that they can fire, but get still removed later
                //foreach (var combatent in _combatShipsTemp) // now search for destroyed ships
                //{
                //    if (combatent.Item1.IsDestroyed)
                //    {

                //        var Assets = GetAssets(combatent.Item1.Owner);
                //        GameLog.Core.Combat.DebugFormat("Opposition {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
                //        if (combatent.Item1.Source is Ship)
                //        {
                //            if (Assets != null)
                //            {
                //                GameLog.Core.Combat.DebugFormat("Name of Owner = {0}, Assets.CombatShips{1}, Assets.NonCobatShips{2}", Assets.Owner.Name, Assets.CombatShips.Count, Assets.NonCombatShips.Count);
                //                if (!Assets.DestroyedShips.Contains(combatent.Item1))
                //                {
                //                    Assets.DestroyedShips.Add(combatent.Item1);
                //                }
                //                if (combatent.Item1.Source.IsCombatant)
                //                {
                //                    Assets.CombatShips.Remove(combatent.Item1);
                //                }
                //                else
                //                {
                //                    Assets.NonCombatShips.Remove(combatent.Item1);
                //                }
                //            }
                //            else
                //                GameLog.Core.Combat.DebugFormat("Assets Null");
                //        }
                //        else if (_combatShips.Contains(_combatStation))
                //        {
                //            if (!Assets.DestroyedShips.Contains(combatent.Item1))
                //            {
                //                Assets.DestroyedShips.Add(combatent.Item1);
                //            }
                //        }
                //        continue;
                //    }
                //}

                ////End the combat... at turn X = 5, by letting all sides reteat
                //if (true) // End Combat after 3 While loops
                //{
                //    _roundNumber += 1;
                //    // _combatShips = _combatShipsTemp;
                //    // NEW123 CHANGED TO TEMP, does this work?
                //    var allRetreatShips = _combatShipsTemp // ALl non destroyed ships retreat (survive)
                //        .Where(s => !s.Item1.IsDestroyed)
                //        .Where(s => s.Item1.Owner != s.Item1.Source.Sector.Owner) // Ships in own territory make a stand (remain in the system they own), after 5 turns.
                //        .ToList();
                //    foreach (var ship in allRetreatShips)
                //    {
                //        if (ship.Item1 != null)
                //        {
                //            var ownerAssets = GetAssets(ship.Item1.Owner);
                //            if (!ownerAssets.EscapedShips.Contains(ship.Item1))
                //            {
                //                ownerAssets.EscapedShips.Add(ship.Item1);
                //                ownerAssets.CombatShips.Remove(ship.Item1);
                //                ownerAssets.NonCombatShips.Remove(ship.Item1);
                //                _combatShips.Remove(ship);
                //            }

                //        }
                //    }

                // REFILL WITH _combatShipsTemp
                //_targetDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
                //_shipListDictionary = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
                //_willFightAlongSide = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
                //_friendlyCombatShips = _combatShips.Where(s => s.Item1.OwnerID == attacker.OwnerID).Select(s => s).ToList();
                //_oppositionCombatShips = _combatShips.Where(s => s.Item1.OwnerID == attacker.OwnerID).Select(s => s).ToList();
                ////_defaultCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
                #endregion
                for (int i = 0; i < _combatShips.Count; i++)
                {
                    GameLog.Core.Test.DebugFormat("the _combatShip[i] ={0}", _combatShips[i].Item1.Name);
                    GameLog.Core.Test.DebugFormat("_combatShipTemp[i] ={0}", _combatShipsTemp[i].Item1.Name);
                }
                break;
            }
            // break out of while loop end combat
             //End of Combat:
            foreach (var combatent in _combatShipsTemp) // now search for destroyed ships
            {
                GameLog.Core.Combat.DebugFormat("Combatent {0} {1} IsDestroid ={2} if true see second line, Hull ={3} ", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.IsDestroyed, combatent.Item1.HullStrength);
                if (combatent.Item1.IsDestroyed)
                {
                    var Assets = GetAssets(combatent.Item1.Owner);
                    Assets.AssimilatedShips.Remove(combatent.Item1);
                    GameLog.Core.Combat.DebugFormat("Combatent {0} {1} IsDestroid ={2} the second line, Hull ={3}", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.IsDestroyed, combatent.Item1.HullStrength);
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
                    else if (_combatShips.Contains(_combatStation))
                    {
                        if (!Assets.DestroyedShips.Contains(combatent.Item1))
                        {
                            Assets.DestroyedShips.Add(combatent.Item1);
                        }
                    }
                    continue;
                }
            }

           // End the combat... at turn X = 5, by letting all sides reteat
            if (true) // End Combat after 3 While loops
            {
                GameLog.Core.Combat.DebugFormat("round# ={0}", _roundNumber);
                _roundNumber += 1;
                GameLog.Core.Combat.DebugFormat("round# ={0} now", _roundNumber);
                // _combatShips = _combatShipsTemp;
                // NEW123 CHANGED TO TEMP, does this work?
                var allRetreatShips = _combatShipsTemp // All non destroyed ships retreat (survive)
                    .Where(s => !s.Item1.IsDestroyed)
                    .Where(s => s.Item1.Owner != s.Item1.Source.Sector.Owner) // Ships in own territory make a stand (remain in the system they own), after 5 turns.
                    .ToList();
                foreach (var ship in allRetreatShips)
                {
                    if (ship.Item1 != null)
                    {
                        GameLog.Core.Combat.DebugFormat("retreated ship = {0} {1}", ship.Item1.Name, ship.Item1.Description);
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1))
                        {
                            GameLog.Core.Combat.DebugFormat("EscapedShips ={0}", ship.Item1.Name);
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                        }

                    }
                }
            }
            //********************************************************************
        }// END OF RESOVLECOMBATROUNDCORE
        #region ChooseTarget()
        ///// <summary>
        ///// Chooses a target for the given <see cref="CombatUnit"/>
        ///// </summary>
        ///// <param name="attacker"></param>
        ///// <returns></returns>
        //private CombatUnit ChooseTarget(CombatUnit attacker)
        //{
        //    if (attacker == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    var attackerOrder = GetCombatOrder(attacker.Source);
        //    var attackerCivID = attacker.Owner.CivID;
        //    var attackingOwnerID = attacker.OwnerID;
        //    //if (_targetDictionary[attackingOwnerID].Count() == 0)
        //    //{
        //    //    EndCombatConditions(attacker);
        //    //}
        //    var oppositionUnits = _targetDictionary[attackingOwnerID].ToList();
        //    bool hasOppositionStation = (_combatStation != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
        //    oppositionUnits.Randomize();
        //    var firstOppositionUint = oppositionUnits.First().Item1;
        //    while (true)
        //    {
        //        switch (attackerOrder)
        //        {
        //            case CombatOrder.Engage:
        //            case CombatOrder.Formation:
        //                if (_targetDictionary[attacker.OwnerID].Count() > 0)
        //                {
        //                    return firstOppositionUint;
        //                }
        //                break;
        //            case CombatOrder.Rush:
        //                //If there are any ships that are retreating, target them
        //                var oppositionRetreating = oppositionUnits.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
        //                if (oppositionRetreating.Count() > 0)
        //                {
        //                    return oppositionRetreating.First().Item1;
        //                }
        //                attackerOrder = CombatOrder.Engage;
        //                break;
        //            case CombatOrder.Transports:
        //                //If there are transports and they are not in formation, target them
        //                var oppositionTransports = oppositionUnits.Where(cs => (cs.Item1.Source.OrbitalDesign.ShipType == "Transport") && !cs.Item1.IsDestroyed);
        //                bool oppositionIsInFormation = oppositionUnits.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
        //                if ((oppositionTransports.Count() > 0) && (!oppositionIsInFormation))
        //                {
        //                    return oppositionTransports.First().Item1;
        //                }
        //                //If there any ships retreating, target them
        //                var oppositionRetreatingRaid = oppositionUnits.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
        //                if (oppositionRetreatingRaid.Count() > 0)
        //                {
        //                    return oppositionRetreatingRaid.First().Item1;
        //                }
        //                attackerOrder = CombatOrder.Engage;
        //                break;
        //            default:
        //                return firstOppositionUint; ///oppositionRetreatingRaid.First().Item1; ;
        //        }
        //    }
        //}
        ///// <summary>
        ///// Deals damage to the target, and calculates whether the target has been destroyed
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="target"></param>
        ///// <param name="weapon"></param>
        ///// 
        #endregion //choose target
        #region PerformAttack
        //private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        //{
        //    var sourceAccuracy = source.Source.GetAccuracyModifier(); // var? Or double?
        //    var maneuverability = target.Source.GetManeuverablility(); // byte
        //    if (sourceAccuracy > 1 || sourceAccuracy < 0.1)  // if getting odd numbers, take normal one, until bug fixed
        //    {
        //        GameLog.Core.CombatDetails.DebugFormat("sourceAccuracy {0} out of range, now reset to 0.5", sourceAccuracy);
        //        sourceAccuracy = 0.5;
        //    }
        //    var targetDamageControl = target.Source.GetDamageControlModifier();
        //    if (targetDamageControl > 1 || targetDamageControl < 0.1)  // if getting damge control is odd, take standard until bug fixed
        //        targetDamageControl = 0.5;
        //    // if side ==2 opposition is stronger, first frienldy side gets the bonus and side ==1 first friendly side has more ships, opposition side gets the bonus
        //    switch (weakerSide)
        //    {
        //        //if (weakerSide == 1) //first (Friendly) side has more ships
        //        case 1:
        //            {
        //                if (source.Owner != target.Owner || !friendlyOwner) //(If it is an opposition ship[ not first owner of firendly to first owner] improve on thier fire)
        //                {
        //                    sourceAccuracy = 1.0 + (1 - newCycleReduction);
        //                    if (sourceAccuracy > 1.5)
        //                    {
        //                        sourceAccuracy = 1.45;
        //                    }
        //                    cycleReduction = 1;
        //                }
        //                break;
        //            }
        //        //else if (wearkerSide == 0)
        //        case 0:
        //            {
        //                // if wearkerSide == 0, then both are equal. Do no change
        //                cycleReduction = 1;
        //                break;
        //            }
        //        //else if (wearkerSide == 2) 
        //        case 2:// Opposition side has more ships so cycle
        //            {
        //                if (source.Owner == target.Owner || friendlyOwner) //(If it is samne owner as first, or friendly to first, improve on thier fire)
        //                {
        //                    sourceAccuracy = 1.0 + (1 - newCycleReduction);
        //                    if (sourceAccuracy > 1.5)
        //                    {
        //                        sourceAccuracy = 1.45;
        //                    }
        //                    cycleReduction = 1;
        //                } // First (friend) owner is source owner or performAttack is on a friendlyOwner as source owner call from the _combatShipTemp cycle
        //                break;
        //            }
        //    }
        //    // if firing ship OR targeted ship are heroShips, change values to be better.
        //    if (source.Name.Contains("!"))
        //    {
        //        sourceAccuracy = 1.7; // change to 170% accuracy
        //    }
        //    if (target.Name.Contains("!"))
        //    {
        //        targetDamageControl = 1;
        //    }
        //    // Added lines to reduce damage to SB and OB to 10%. Also  Changed damage to 2.5 instead of 4. and 10 instead of 50
        //    if (!target.IsMobile &&
        //        target.Source.Sector.Name == "Sol"
        //        || target.Source.Sector.Name == "Terra"
        //        || target.Source.Sector.Name == "Omarion"
        //        || target.Source.Sector.Name == "Borg"
        //        || target.Source.Sector.Name == "Qo'noS"
        //        || target.Source.Sector.Name == "Romulus"
        //        || target.Source.Sector.Name == "Cardassia")
        //    {
        //        targetDamageControl = 1.4;
        //        //GameLog.Core.Combat.DebugFormat("targetDamageControl = {0} due to HomeSystemStation or OB at {1}", targetDamageControl, target.Source.Sector.Name);
        //    } // end added lines
        //      // currentx
        //    double currentManeuverability = maneuverability;// get int target maneuverablity, convert to double
        //    double ManeuverabilityModifer = 0.0;
        //    var sourceAccuracyTemp = 0.5;
        //    if (sourceAccuracy > 0.9 && sourceAccuracy < 1.7)
        //        sourceAccuracyTemp = 0.6;
        //    ManeuverabilityModifer = ((5 - currentManeuverability) / 10); // +/- 0.4 Targets maneuverablity
        //    sourceAccuracyTemp = sourceAccuracyTemp + ManeuverabilityModifer;
        //    if (sourceAccuracyTemp < 0.0 || sourceAccuracyTemp > 1) // prevent out of range numbers
        //        sourceAccuracyTemp = 0.5;
        //    if (sourceAccuracy == 1.7) // if heroship value, use it
        //        sourceAccuracyTemp = 1.7;
        //    //GameLog.Core.CombatDetails.DebugFormat("various values: {0} {1} {2} at {3} ({4}), OTHERS: friendlyOwner = {6}, firstOwner = {6}",
        //    //source.Source.ObjectID, source.Source.Name, source.Source.Design, target.Source.Sector.Name, target.Source.Sector.Location, friendlyOwner.ToString(), firstOwner.ToString());
        //    //GameLog.Core.CombatDetails.DebugFormat("various values: sourceAccuracy = {0}, sourceAccuracyTemp = {1}, maneuverability = {2}, currentManeuverability = {3}, ManeuverabilityModifer = {4}, targetDamageControl = {5}",
        //    //sourceAccuracy,
        //    //sourceAccuracyTemp,
        //    //maneuverability,
        //    //currentManeuverability,
        //    //ManeuverabilityModifer,
        //    //targetDamageControl
        //    //);
        //    if (RandomHelper.Random(100) <= (100 * sourceAccuracyTemp))  // not every weapons does a hit
        //    {
        //        // Fire Weapons, inflict damage
        //        target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10)); // minimal damage of 50 included
        //        GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), DamageControl = {6}, Shields = {7}, Hull = {8}",
        //            target.Source.ObjectID, target.Name, target.Source.Design,
        //            (int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 2.5 + 10),
        //            cycleReduction,
        //            sourceAccuracy,
        //            targetDamageControl,
        //            target.ShieldStrength,
        //            target.HullStrength
        //            );
        //    }
        //    //weapon.Discharge();
        //}
        #endregion
        #region Methodes
        //private void LoadTargets(int ownerID, List<Tuple<CombatUnit, CombatWeapon[]>> listTuple)
        //{
        //    //listTuple = _shipListDictionary[ownerID];  //_combatShips.Where(cs => cs.Item1.OwnerID == ownerID).Select(cs => cs).ToList();
        //    if (_targetDictionary[ownerID].Count() != 0)
        //    {
        //        GameLog.Core.Test.DebugFormat("dictionary at {0} is count 0? ={1}", ownerID, _targetDictionary[ownerID].FirstOrDefault().Item1.Owner.ShortName);
        //        listTuple.Distinct().ToList();
        //        _targetDictionary[ownerID].AddRange(listTuple);
        //    }
        //    else
        //    {
        //        _targetDictionary[ownerID] = listTuple;
        //        GameLog.Core.Test.DebugFormat("dictionary at {0} has ={1}", ownerID, _targetDictionary[ownerID].FirstOrDefault().Item1.Owner.ShortName);
        //    }
        //}
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
        //private void EndCombatConditions(CombatUnit attacker)
        //{
        //    _friendlyCombatShips = _combatShips.Where(s => s.Item1.OwnerID == attacker.OwnerID).Select(s => s).ToList();
        //    if (_willFightAlongSide[attacker.OwnerID].Count() != 0)
        //        _friendlyCombatShips.AddRange(_willFightAlongSide[attacker.OwnerID]);
        //}
        #endregion


    }
}
#endregion