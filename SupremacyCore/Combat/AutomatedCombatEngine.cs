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
        private object firstOwner;
        private int friendlyWeaponPower = 0;
        private int weakerSide = 0; // 0= no bigger ships counts, 1= First Friendly side bigger, 2= Oppostion side bigger
        private List<int> _unitOnwerIDs;
        private Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>> _oppositionUnits;
        private Dictionary<int, int> _stopLoop;
        private Dictionary<int, int> _stopLoop2;
        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {
            _unitOnwerIDs = new List<int>();
            _stopLoop = new Dictionary<int, int>();
            _stopLoop2 = new Dictionary<int, int>();
            _oppositionUnits = new Dictionary<int, List<Tuple<CombatUnit, CombatWeapon[]>>>();
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
            if (!IsCombatOver)
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
            // populate dictionary of list of target units (_oppositionUnits)
            foreach (var unitTuple in _combatShips)
            {
                if (!_unitOnwerIDs.Contains(unitTuple.Item1.OwnerID))
                {
                    _unitOnwerIDs.Add(unitTuple.Item1.OwnerID);
                    _unitOnwerIDs.Distinct().ToList();
                    GameLog.Core.Test.DebugFormat("Top of loop unitTuple {0} {1}", unitTuple.Item1.Owner, unitTuple.Item1.Name);
                    List<Tuple<CombatUnit, CombatWeapon[]>> targetUnitTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>();
                    int _attackerOnwerID = 0;
                    int _attackerCounter = 0; // new int();
                    foreach (var attackingTuple in _combatShips)
                    {
                        int _attackerCounter2 = 0;
                        var attackerOrder = GetCombatOrder(attackingTuple.Item1.Source);
                        _attackerOnwerID = attackingTuple.Item1.OwnerID;
                      
                        if (_attackerOnwerID != unitTuple.Item1.OwnerID) // && !_unitAttackerIDs.Contains(attackingTuple.Item1.OwnerID))
                        {
                            var attackerTragetOne = GetTargetOne(attackingTuple.Item1.Source);
                            var attackerTragetTwo = GetTargetTwo(attackingTuple.Item1.Source);

                            GameLog.Core.Test.DebugFormat("attacker ={0} {1} targeting? {2}", attackingTuple.Item1.Owner.ShortName, attackingTuple.Item1.Source.Name, attackerTragetOne.ShortName);
                            GameLog.Core.Test.DebugFormat("unitTuple {0} {1} & attacker {2} {3}", unitTuple.Item1.OwnerID, unitTuple.Item1.Name, attackingTuple.Item1.OwnerID, attackingTuple.Item1.Name);
                            if ((attackerOrder == CombatOrder.Hail) || (attackerOrder == CombatOrder.LandTroops) || (attackerOrder == CombatOrder.Retreat) || (attackerOrder == CombatOrder.Standby))
                            {
                                throw new ArgumentException("Cannot choose a target for unit that does not require a target");
                            }
                            List<Tuple<CombatUnit, CombatWeapon[]>> returnFireTupleList = new List<Tuple<CombatUnit, CombatWeapon[]>>();
                            _attackerCounter += 1;
                            int _loopCount = new int();
                            _loopCount = 0;
                            foreach (var unitTupleTarget in _combatShips)
                            {

                                _attackerCounter2 += 1;
                                _loopCount += 1;
                                if (_loopCount == 1);
                                var targetShipCount = _combatShips.Where(cs => cs.Item1.OwnerID == unitTuple.Item1.OwnerID).Select(cs => cs).ToList();

                                _stopLoop[unitTupleTarget.Item1.OwnerID] = _attackerCounter;
                                if (GetTargetOne(attackingTuple.Item1.Source) != GetTargetTwo(attackingTuple.Item1.Source) && _stopLoop[unitTupleTarget.Item1.OwnerID] > 1)
                                {
                                    GameLog.Core.Test.DebugFormat("Add Targeting at {0} for attacker {1}", unitTuple.Item1.Name, attackingTuple.Item1.Owner.ShortName);
                                    targetUnitTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == unitTuple.Item1.OwnerID).Select(sc => sc).ToList();    //.Add(unitTupleTarget);
                                    if (targetUnitTupleList != null)
                                        targetUnitTupleList.Distinct().ToList();

                                    GameLog.Core.Test.DebugFormat("Add returnfire at {0} for targeted' {1}", attackingTuple.Item1.Name, unitTuple.Item1.Owner.ShortName);
                                    returnFireTupleList = _combatShips.Where(cs => cs.Item1.OwnerID == attackingTuple.Item1.OwnerID).Select(cs => cs).ToList();
                                    if (returnFireTupleList != null)
                                        returnFireTupleList.Distinct().ToList();
                                    goto Targeting;
                                }

                                if (_stopLoop[unitTupleTarget.Item1.OwnerID] > 1)
                                    break;

                                if (attackerTragetOne == unitTuple.Item1.Owner || attackerTragetTwo == unitTuple.Item1.Owner) // || returningFire==unitTupleTarget.Item1.Owner )
                                //|| GameContext.Current.DiplomacyData[attackingUnitTuple.Item1.Owner, unitTupleTarget.Item1.Owner].Status == ForeignPowerStatus.AtWar) ;
                                {
                                    _stopLoop2[unitTupleTarget.Item1.OwnerID] = _attackerCounter;
                                    if (_stopLoop2[unitTupleTarget.Item1.OwnerID] > 1)
                                        continue;
                                    //Targeting:
                                    GameLog.Core.Test.DebugFormat("Add Targeting at {0} for attacker {1}", unitTuple.Item1.Name, attackingTuple.Item1.Owner.ShortName);
                                    targetUnitTupleList = _combatShips.Where(sc => sc.Item1.OwnerID == unitTuple.Item1.OwnerID).Select(sc => sc).ToList();    //.Add(unitTupleTarget);
                                    if(targetUnitTupleList!=null)
                                    targetUnitTupleList.Distinct().ToList();

                                    GameLog.Core.Test.DebugFormat("Add returnfire at {0} for targeted' {1}", attackingTuple.Item1.Name, unitTuple.Item1.Owner.ShortName);
                                    returnFireTupleList = _combatShips.Where(cs => cs.Item1.OwnerID == attackingTuple.Item1.OwnerID).Select(cs => cs).ToList();
                                    if(returnFireTupleList != null)
                                    returnFireTupleList.Distinct().ToList();
                                }
                                Targeting:
                                if (targetShipCount.Count() == _loopCount && attackingTuple.Item1 != unitTupleTarget.Item1)//_unitOnwerIDs.Contains(attackerOnwerID))
                                {
                                    GameLog.Core.Test.DebugFormat("Targeting on ={0} fromm attacker {1}", unitTuple.Item1.Owner.ShortName, attackingTuple.Item1.Name);
                                    if (targetUnitTupleList != null)
                                        _oppositionUnits[_attackerOnwerID] = targetUnitTupleList;
                                    GameLog.Core.Test.DebugFormat("Return Target ={0} from target {1}", attackingTuple.Item1.Name, unitTuple.Item1.Owner.ShortName);
                                    if (returnFireTupleList != null)
                                        _oppositionUnits[unitTuple.Item1.OwnerID] = returnFireTupleList;

                                    foreach (var oppositionUnit in _oppositionUnits[_attackerOnwerID])
                                    {
                                        GameLog.Core.Test.DebugFormat("opposition List unit {0} for attacker {1}", oppositionUnit.Item1.Name, attackingTuple.Item1.Owner.ShortName);

                                    }
                                    foreach (var oppositionReturnFireUnit in _oppositionUnits[unitTuple.Item1.OwnerID])
                                    {
                                        GameLog.Core.Test.DebugFormat("opposition returnfire List unit {0} for targeted {1}", oppositionReturnFireUnit.Item1.Name, unitTuple.Item1.Owner.ShortName);
                                    }
                            }  
                        }
                        targetUnitTupleList.Clear();
                        returnFireTupleList.Clear();
                        }
                    }
                }
            }
            CombatUnit target = new CombatUnit();
            //do combat for each unit in sector
            foreach (var attackingUnit in _combatShips)
            {
                    target = ChooseTarget(attackingUnit.Item1);
                if (target == null)
                    continue;
             
                if (!target.IsDestroyed)  //&& !target.) // Bug?: do not target retreated ships
                {
                    // just not firing full fire power of one ship before the other ship is firing, but ..
                    // but each 2nd Weapon e.g. first 5 Beams than 3 Torpedos
                    foreach (var weapon in attackingUnit.Item2)
                    {
                        GameLog.Core.Test.DebugFormat("Perform Attack Weapon {0} for attacker unit {1}", weapon, attackingUnit.Item1.Name);
                        PerformAttack(attackingUnit.Item1, target, weapon);
                    }
                }
            }
        }


        //    if (GetTargetOne(unitTuple.Item1.Source) == currentAsset.Item1.Source.Owner) 
        //{
        //    GameLog.Core.Test.DebugFormat("Populate Oppositon Ships currentAsset other ship ={0} owner ={1}, attacking ship target civ ={2} ",
        //        currentAsset.Item1.Source, currentAsset.Item1.Source.Owner, (GetTargetOne(unitTuple.Item1.Source).Name.ToString()));
        //    OppositionShips.Add(currentAsset);
        //    OppositionShips.Distinct().Randomize();
        //}
        ////}

        //else if (CombatHelper.WillFightAlongside(currentAsset.Item1.Owner, unitTuple.Item1.Owner)|| unitTuple.Item1.Owner == currentAsset.Item1.Owner)
        //{
        //    FriendlyShips.Add(currentAsset);
        //    FriendlyShips.Distinct().Randomize();
        //    GameLog.Core.Test.DebugFormat("added to FRIENDLY_CombatShips = {0} D={1} {2}",
        //                        currentAsset.Item1.Source.ObjectID, currentAsset.Item1.Source.Design, currentAsset.Item1.Source);
        //}

        //else
        //{
        //    OtherShips.Add(currentAsset);
        //    OtherShips.Distinct().Randomize();
        //    GameLog.Core.Test.DebugFormat("added to OtherShips = {0} D={1} {2}",
        //        currentAsset.Item1.Source.ObjectID, currentAsset.Item1.Source.Design, currentAsset.Item1.Source);
        //}

        //if (OppositionShips.Count() > 0)
        //{
        //    foreach (var oppShip in OppositionShips)
        //    {
        //        GameLog.Core.Test.DebugFormat("OppositionShips: {0} {1} {2}, TargetOne = {3}, TargetOne = {4}",
        //        oppShip.Item1.Owner, oppShip.Item1.Source.ObjectID, oppShip.Item1.Source.Name
        //            , GetTargetOne(oppShip.Item1.Source), GetTargetTwo(oppShip.Item1.Source));
        //    }
        //}

        //GameLog.Core.Test.DebugFormat("-------------------------------------------------------------------");
        //, ... GetTargetOne = not working at the moment
        // var countFriends = FriendlyShips.Count();

        //if (currentAsset.Item1.Owner != attackingShip.Item1.Owner)
        //    GameLog.Core.Test.DebugFormat("attackingShip: {3} DesignID={4} {5} O= {6} *vs* currentAsset = {0} DesignID={1} {2}: ",
        //                        //Targets: Prime={7}, Second={8}, # Friends {9} {10}, Count {11}
        //                        currentAsset.Item1.Source.ObjectID,
        //                        //currentAsset.Item1.Source.Design,     // just DesignId makes the lines shorter
        //                        currentAsset.Item1.Source.Design.DesignID,  // just DesignId makes the lines shorter
        //                        currentAsset.Item1.Source.Name,

        //                        attackingShip.Item1.Source.ObjectID,
        //                        //attackingShip.Item1.Source.Design,     // just DesignId makes the lines shorter
        //                        attackingShip.Item1.Source.Design.DesignID,  // just DesignId makes the lines shorter
        //                        attackingShip.Item1.Source.Name,
        //                        attackingShip.Item1.Source.Owner
        //                        //,
        //                        //GetTargetOne(currentAsset.Item1.Source).ToString(),
        //                        //GetTargetTwo(currentAsset.Item1.Source).ToString(),
        //                        //GetTargetOne(attackingShip.Item1.Source).ToString(),
        //                        //GetTargetTwo(attackingShip.Item1.Source).ToString(),
        //                        //countFriends
        //                        );
        //    }
        //}
        //GameLog.Core.Test.DebugFormat("OtherShips.Count() = {0}", OtherShips.Count());
        //GameLog.Core.Test.DebugFormat("FriendlyShips.Count() = {0}", FriendlyShips.Count());
        //GameLog.Core.Test.DebugFormat("OppositionShips.Count() = {0}", OppositionShips.Count());

        //// test section
        //for (int i = 0; i < _combatShips.Count; i++)
        //{
        //    //if ()
        //    //{
        //    //    var target = ChooseTarget(attackingShip);
        //    //}
        //}
        //double ratioATemp = 0.00; // used to transform ship.Count to double decimals
        //double ratioBTemp = 0.00; // used to transform ship.Count to double decimals

        //// Prevent division by 0, if one side has been wiped out / or retreated.
        //if (OppositionShips.ToList().Count == 0 || FriendlyShips.Count == 0)
        //{
        //    shipRatio = 1;
        //    excessShipsStartingAt = 0;
        //    weakerSide = 0;
        //}
        //else
        //{
        //    if (FriendlyShips.ToList().Count - OppositionShips.ToList().Count > 0)
        //    {
        //        excessShipsStartingAt = OppositionShips.ToList().Count * 2;

        //        ratioATemp = FriendlyShips.Count();
        //        ratioBTemp = OppositionShips.Count();
        //        shipRatio = ratioATemp / ratioBTemp;
        //        weakerSide = 1;
        //    }

        //    else
        //    {
        //        excessShipsStartingAt = FriendlyShips.Count * 2;
        //        ratioATemp = FriendlyShips.Count();
        //        ratioBTemp = OppositionShips.Count();
        //        shipRatio = ratioBTemp / ratioATemp;
        //        weakerSide = 2;
        //    }
        //}
        //if (FriendlyShips.Count() == OppositionShips.Count())
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
        //if (FriendlyShips.Count() < 4 || OppositionShips.Count() < 4) // small fleets attack each other at full power
        //{
        //    newCycleReduction = 1;
        //}
        //GameLog.Core.CombatDetails.DebugFormat("various values: newCycleReduction = {0}, excessShipsStartingAt = {1}, ratioATemp = {2}, ratioBTemp = {3},  shipRatio = {4}, weakerSide = {5}",
        //    newCycleReduction,
        //    excessShipsStartingAt,
        //    ratioATemp,
        //    ratioBTemp,
        //    shipRatio,
        //    weakerSide);

        //OppositionShips.Randomize();
        //FriendlyShips.Randomize();
        //for (int i = 0; i < _combatShips.Count; i++) // sorting combat Ships to have one ship of each side alternating
        //{
        //    if (i <= FriendlyShips.Count - 1)
        //        _combatShipsTemp.Add(FriendlyShips[i]);// First Ship in _ is Friendly (initialization)

        //    if (i <= OppositionShips.ToList().Count - 1)
        //        _combatShipsTemp.Add(OppositionShips.ToList()[i]); // Second Ship in _combatShipsTemp is opposition (initialization)   
        //}

        //_combatShips.Clear(); //  after ships where sorted into Temp, delete 
        //// the original Array. After that populate empty array with sorted temp array
        //for (int i = 0; i < _combatShipsTemp.Count; i++)
        //{
        //    _combatShips.Add(_combatShipsTemp[i]);

        //}
        //_combatShipsTemp.Clear(); // Temp cleared for next runthrough
        //_combatShips.Randomize();


        //// Stop using Temp, only use it to sort and then get rid of it

        //for (int i = 0; i < _combatShips.Count; i++) // 
        //{
        //    GameLog.Core.CombatDetails.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
        //        _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, _combatShips[i].Item1.Source.Design, i);
        //}

        //for (int i = 0; i < _combatShips.Count; i++)
        //{
        //    //GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
        //    //     _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Name, _combatShipsTemp[i].Item1.Source.Design, i);


        //    var ownerAssets = GetAssets(_combatShips[i].Item1.Owner);
        //    //var ships = new List<Ship>(_combatShips.).ToList();
        //    //for (int ship = 0; ship < _combatShips.; ship++)
        //    //{

        //    //}

        //    //var oppositionShips = _combatShips.Where(cs => CombatHelper.WillEngage(_combatShips[i].Item1.Owner, cs.Item1.Owner));
        //    //var oppositionShips = _combatShips.Where(cs => GetTargetOne(_combatShips[i].Item1.Source) == cs.Item1.Source));

        //    //var friendlyShips = _combatShips.Where(cs => CombatHelper.WillFightAlongside(_combatShips[i].Item1.Owner, cs.Item1.Owner));

        //    if (i + 1 > excessShipsStartingAt && excessShipsStartingAt != 0) // added: if ships equal = 0 = excessShips, then no cycle reduction
        //    {
        //        cycleReduction = newCycleReduction;
        //    } // +1 because a 2nd ship can have full firepower, but not the 3rd exceeding the other side
        //    else
        //    {
        //        cycleReduction = 1;
        //    }

        //    List<string> ownEmpires = _combatShips.Where(s =>
        //        (s.Item1.Owner == _combatShips[i].Item1.Owner))
        //        .Select(s => s.Item1.Owner.Key)
        //        .Distinct()
        //        .ToList();


        //    List<string> friendlyEmpires = _combatShips.Where(s =>
        //        (s.Item1.Owner != _combatShips[i].Item1.Owner) &&
        //        CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShips[i].Item1.Owner))
        //        .Select(s => s.Item1.Owner.Key)
        //        .Distinct()
        //        .ToList();


        //    List<string> hostileEmpires = _combatShips.Where(s =>
        //        (s.Item1.Owner != _combatShips[i].Item1.Owner) &&
        //        CombatHelper.WillEngage(s.Item1.Owner, _combatShips[i].Item1.Owner))
        //        .Select(s => s.Item1.Owner.Key)
        //        .Distinct()
        //        .ToList();


        //    firstOwner = _combatShips[0].Item1.Owner;
        //    if (CombatHelper.WillEngage(_combatShips[i].Item1.Owner, _combatShips[0].Item1.Owner) && _combatShips[0].Item1.Owner != _combatShips[i].Item1.Owner)
        //    {
        //        friendlyOwner = false;
        //    }
        //    else
        //    {
        //        friendlyOwner = true;
        //    }

        //    int friendlyWeaponPower = ownEmpires.Sum(e => _empireStrengths[e]) + friendlyEmpires.Sum(e => _empireStrengths[e]);
        //    int hostileWeaponPower = hostileEmpires.Sum(e => _empireStrengths[e]);
        //    int weaponRatio = friendlyWeaponPower * 10 / (hostileWeaponPower + 1);

        //    //Figure out if any of the opposition ships have sensors powerful enough to penetrate our camo. If so, will be decamo.
        //    if (OppositionShips.Count() > 0)

        //    {
        //        maxScanStrengthOpposition = OppositionShips.Max(s => s.Item1.Source.OrbitalDesign.ScanStrength);


        //        if (_combatShips[i].Item1.IsCamouflaged && _combatShips[i].Item1.CamouflagedStrength < maxScanStrengthOpposition)
        //        {
        //            _combatShips[i].Item1.Decamouflage();
        //            GameLog.Core.Combat.DebugFormat("{0} has camou strength {1} vs maxScan {2}",
        //                _combatShips[i].Item1.Name, _combatShips[i].Item1.CamouflagedStrength, maxScanStrengthOpposition);
        //        }
        //    }

        //    //TODO: Move this to DiplomacyHelper
        //    List<string> allEmpires = new List<string>();
        //    allEmpires.AddRange(ownEmpires);
        //    allEmpires.AddRange(friendlyEmpires);
        //    allEmpires.AddRange(hostileEmpires);
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

        //    bool oppositionIsRushing = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Rush));
        //    bool oppositionIsInFormation = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
        //    bool oppositionIsHailing = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Hail));
        //    bool oppositionIsRetreating = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Retreat));
        //    bool oppositionIsRaidTransports = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Transports));
        //    bool oppositionIsEngage = OppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Engage));

        //    var order = GetCombatOrder(_combatShips[i].Item1.Source);
        //    switch (order)
        //    {
        //        case CombatOrder.Engage:
        //        case CombatOrder.Rush:
        //        case CombatOrder.Transports:
        //        case CombatOrder.Formation:

        //            var attackingShip = _combatShips[i].Item1;
        //            var target = ChooseTarget(attackingShip);
        //           // GameLog.Core.Test.DebugFormat("Choose target call with attackingShip {0} of {1} to get target {2} {3}", attackingShip, attackingShip.Owner.ToString(), target.ToString(), target.Owner);
        //            if (order != CombatOrder.Formation && order != CombatOrder.Engage)
        //            {
        //                var maneuver = attackingShip.Source.OrbitalDesign.Maneuverability;// ship maneuver values 1 to 8 (stations and OB = zero)
        //                //target.TakeDamage((target.Source.OrbitalDesign.HullStrength +1) * (maneuver/32)+1); // max possible hull damage of 25%

        //                GameLog.Core.Combat.DebugFormat("({2}) {0} {1}: new hull strength {3}, took damage {4} due to Maneuverability {5} from ({8}) {6} {7}",
        //                    target.Source.ObjectID, target.Source.Name, target.Source.Design,
        //                    target.Source.OrbitalDesign.HullStrength,
        //                    (target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1,
        //                    maneuver,
        //                    attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.Design
        //                    );
        //                target.TakeDamage((target.Source.OrbitalDesign.HullStrength + 1) * (maneuver / 32) + 1); // max possible hull damage of 25%
        //            }
        //            if (target == null)
        //            {
        //                GameLog.Core.Combat.DebugFormat("No target for {0} {1} ({2})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design);
        //            }
        //            if (target != null)
        //            {
        //                GameLog.Core.Combat.DebugFormat("Target for {0} {1} ({2}) is {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);

        //                // If we rushed a formation we could take damage
        //                int chanceRushingFormation = RandomHelper.Random(100);
        //                if (oppositionIsInFormation && (order == CombatOrder.Rush || order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
        //                {
        //                    attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 4);  // 25 % down out of Hullstrength of TechObjectDatabase.xml

        //                    GameLog.Core.Combat.DebugFormat("{0} {1} rushed or raid transports in formation and took {2} damage to hull ({3} hull left)",
        //                        attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
        //                }
        //                if (oppositionIsEngage && (order == CombatOrder.Formation || order == CombatOrder.Rush) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
        //                {
        //                    attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml

        //                    GameLog.Core.Combat.DebugFormat("{0} {1} in Formation or Rushing while Engaged and took {2} damage to hull ({3} hull left)",
        //                        attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
        //                }
        //                if (oppositionIsRushing && (order == CombatOrder.Transports) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
        //                {
        //                    attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 5);  // 20 % down out of Hullstrength of TechObjectDatabase.xml

        //                    GameLog.Core.Combat.DebugFormat("{0} {1} Raiding Transports and got Rushed took {2} damage to hull ({3} hull left)",
        //                        attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
        //                }
        //                if (oppositionIsRaidTransports && (order == CombatOrder.Engage) && (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100))))
        //                {
        //                    attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 6);  // 17 % down out of Hullstrength of TechObjectDatabase.xml

        //                    GameLog.Core.Combat.DebugFormat("{0} {1} Engag order got Raided and took {2} damage to hull ({3} hull left)",
        //                        attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength);
        //                }

        //                bool assimilationSuccessful = false;
        //                //If the attacker is Borg, try and assimilate before you try destroying it
        //                if (attackingShip.Owner.Name == "Borg" && target.Owner.Name != "Borg")
        //                {
        //                    GameLog.Core.Combat.DebugFormat("{0} {1} attempting assimilation on {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
        //                    int chanceToAssimilate = RandomHelper.Random(100);
        //                    assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
        //                }

        //                //Perform the assimilation, but only on ships
        //                if (assimilationSuccessful && target.Source is Ship)
        //                {
        //                    GameLog.Core.Combat.DebugFormat("{0} {1} successfully assimilated {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);

        //                    CombatAssets oppositionAssets = GetAssets(target.Owner);
        //                    if (!ownerAssets.AssimilatedShips.Contains(target))
        //                    {
        //                        ownerAssets.AssimilatedShips.Add(target);
        //                        target.IsAssimilated = true;
        //                    }
        //                    if (target.Source.IsCombatant)
        //                    {
        //                        oppositionAssets.CombatShips.Remove(target);
        //                    }
        //                    else
        //                    {
        //                        oppositionAssets.NonCombatShips.Remove(target);
        //                    }

        //                }
        //                else // if not assmilated attack, perform attack fire weapons
        //                {
        //                    //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);


        //                    var weapons = _combatShips[i].Item2.Where(w => w.CanFire);
        //                    int amountOfWeapons = weapons.Count();
        //                    //var partlyFiring = 0;


        //                    foreach (var weapon in _combatShips[i].Item2.Where(w => w.CanFire))
        //                    {
        //                        if (!target.IsDestroyed)  //&& !target.) // Bug?: do not target retreated ships
        //                        {
        //                            // just not firing full fire power of one ship before the other ship is firing, but ..
        //                            // but each 2nd Weapon e.g. first 5 Beams than 3 Torpedos
        //                            //    commend unknown/old stuff if ((partlyFiring += 1) * 2 < amountOfWeapons)
        //                            // {


        //                            //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5}), amountOfWeapons = {6}, partlyFiring Step {7}",
        //                            //    attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design,
        //                            //    amountOfWeapons, partlyFiring);

        //                            PerformAttack(attackingShip, target, weapon);

        //                            //}
        //                        }
        //                    }
        //                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5}), amountOfWeapons = {6}",
        //                        attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design,
        //                        target.Source.ObjectID, target.Name, target.Source.Design,
        //                        amountOfWeapons);
        //                    // all weapons fired for current ship i

        //                }

        //            }


        //            foreach (var combatShip in _combatShips)
        //            {
        //                if (combatShip.Item1.IsDestroyed)
        //                {
        //                    ownerAssets.AssimilatedShips.Remove(target);
        //                }
        //            }

        //            break;

        //        case CombatOrder.Retreat:

        //            if (WasRetreateSuccessful(_combatShips[i].Item1, oppositionIsRushing, oppositionIsEngage, oppositionIsInFormation, oppositionIsHailing, oppositionIsRetreating, oppositionIsRaidTransports, weaponRatio))
        //            {
        //                try
        //                {
        //                    // added destroyed ship cannot retreat
        //                    if (!ownerAssets.EscapedShips.Contains(_combatShips[i].Item1) && !_combatShips[i].Item1.IsDestroyed)
        //                    {

        //                        ownerAssets.EscapedShips.Add(_combatShips[i].Item1);
        //                    }

        //                    if (_combatShips[i].Item1.Source.IsCombatant)
        //                    {

        //                        ownerAssets.CombatShips.Remove(_combatShips[i].Item1);
        //                    }
        //                    else
        //                    {

        //                        ownerAssets.NonCombatShips.Remove(_combatShips[i].Item1);
        //                    }

        //                }
        //                catch (Exception e)
        //                {
        //                    GameLog.Core.Combat.DebugFormat("Exception e {0} ship {1} {2} {3}", e, _combatShips[i].Item1.Source.Design, _combatShips[i].Item1.Source.Name, _combatShips[i].Item1.Source.ObjectID);
        //                }
        //                _combatShips.Remove(_combatShips[i]);
        //            }

        //            // Chance of hull damage if you fail to retreat and are being rushed
        //            else if (oppositionIsRushing && (weaponRatio > 1))
        //            {

        //                _combatShips[i].Item1.TakeDamage(_combatShips[i].Item1.Source.OrbitalDesign.HullStrength / 2);  // 50 % down out of Hullstrength of TechObjectDatabase.xml

        //                GameLog.Core.Combat.DebugFormat("{0} {1} failed to retreat whilst being rushed and took {2} damage to hull ({3} hull left)",

        //                    _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, _combatShips[i].Item1.Source.OrbitalDesign.HullStrength / 2, _combatShips[i].Item1.Source.HullStrength);
        //            }
        //            break;

        //        case CombatOrder.Hail:
        //            GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) hailing...", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Design.Name);
        //            break;

        //        case CombatOrder.Standby:
        //            GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) standing by...", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Design.Name);
        //            break;

        //    }

        //}

        ////Make sure that the station has a go at the enemy too
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

        //// remove desroyed ships. Now on this spot, so that they can fire, but get still removed later
        //foreach (var combatent in _combatShips) // now earch for destroyed ships
        //{
        //    if (combatent.Item1.IsDestroyed)
        //    {
        //        GameLog.Core.Combat.DebugFormat("Opposition {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
        //        if (combatent.Item1.Source is Ship)
        //        {
        //            var Assets = GetAssets(combatent.Item1.Owner);
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
        //        continue;
        //    }
        //}

        ////End the combat... at turn X = 5, by letting all sides reteat
        //if (_roundNumber == 5) // equals 4 engagements. Minors need an A.I. to find back to homeworld then...
        //{
        //    _roundNumber += 1;
        //    var allRetreatShips = _combatShips
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
        //}// end to end combat


        //}

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
            bool hasOppositionStation = (_combatStation != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
            var oppositionUnits = _oppositionUnits[attackingOwnerID].ToList();
            oppositionUnits.Randomize();
            var firstOppositionUint = oppositionUnits.First().Item1;

            while (true)
            {
                switch (attackerOrder)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Formation:
                        if (_oppositionUnits.Count() > 0)
                        {
                            return firstOppositionUint;
                        }
                        break;
                    ////Has both ships and station to target
                    //if (hasOppositionStation && (oppositionShips.Count() > 0))
                    //{
                    //    var oppOrder = GetCombatOrder(oppositionShips.FirstOrDefault().Item1.Source);
                    //    if (oppOrder == CombatOrder.Formation) //(RandomHelper.Random(5) == 0)
                    //    {
                    //        //GameLog.Core.Combat.DebugFormat("",
                    //        // oppositionShips.FirstOrDefault().Item1.Source.ObjectID,
                    //        // oppositionShips.FirstOrDefault().Item1.Source.Name,
                    //        // oppositionShips.FirstOrDefault().Item1.Source.Design,
                    //        // oppOrder);
                    //        return oppositionShips.FirstOrDefault().Item1;
                    //    }
                    //    else
                    //    {
                    //        return _combatStation.Item1;
                    //    }
                    //    // �MAYBE needs change that target cannot be retreated ships...
                    //}
                    ////Only has a station to target
                    //if (hasOppositionStation)
                    //{
                    //    return _combatStation.Item1;
                    //}
                    ////Nothing to target
                    //return null;

                    case CombatOrder.Rush:
                        //If there are any ships that are retreating, target them

                        var oppositionRetreating = _combatShips.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        if (oppositionRetreating.Count() > 0)
                        {
                            return oppositionRetreating.First().Item1;
                        }
                        attackerOrder = CombatOrder.Engage;
                        break;

                    case CombatOrder.Transports:
                        //If there are transports and they are not in formation, target them
                        var oppositionTransports = _combatShips.Where(cs => (cs.Item1.Source.OrbitalDesign.ShipType == "Transport") && !cs.Item1.IsDestroyed);
                        bool oppositionIsInFormation = oppositionUnits.Any(os => os.Item1.Source.IsCombatant && (GetCombatOrder(os.Item1.Source) == CombatOrder.Formation));
                        if ((oppositionTransports.Count() > 0) && (!oppositionIsInFormation))
                        {
                            return oppositionTransports.First().Item1;
                        }
                        //If there any ships retreating, target them

                        var oppositionRetreatingRaid = _combatShips.Where(cs => (GetCombatOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
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

        }
    }

